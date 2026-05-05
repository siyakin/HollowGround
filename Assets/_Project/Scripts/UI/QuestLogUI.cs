using System.Collections.Generic;
using System.Text;
using HollowGround.Quests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class QuestLogUI : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TMP_Text _headerText;
        [SerializeField] private Button _prevBtn;
        [SerializeField] private Button _nextBtn;

        [Header("List")]
        [SerializeField] private Transform _listContainer;

        [Header("Detail")]
        [SerializeField] private GameObject _detailPanel;
        [SerializeField] private TMP_Text _detailName;
        [SerializeField] private TMP_Text _detailDesc;
        [SerializeField] private TMP_Text _detailObjectives;
        [SerializeField] private TMP_Text _detailRewards;
        [SerializeField] private Button _acceptBtn;
        [SerializeField] private Button _turnInBtn;

        private int _currentTab;
        private QuestInstance _selectedQuest;

        private const int TabActive = 0;
        private const int TabAvailable = 1;
        private const int TabCompleted = 2;

        private void Awake()
        {
            if (_prevBtn != null) _prevBtn.onClick.AddListener(PrevTab);
            if (_nextBtn != null) _nextBtn.onClick.AddListener(NextTab);
            if (_acceptBtn != null) _acceptBtn.onClick.AddListener(AcceptSelectedQuest);
            if (_turnInBtn != null) _turnInBtn.onClick.AddListener(TurnInSelectedQuest);
        }

        private void OnEnable()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestAccepted += HandleQuestChanged;
                QuestManager.Instance.OnQuestObjectiveUpdated += HandleQuestChanged;
                QuestManager.Instance.OnQuestCompleted += HandleQuestChanged;
                QuestManager.Instance.OnQuestTurnedIn += HandleQuestChanged;
                QuestManager.Instance.OnQuestListChanged += RefreshList;
            }
            ShowTab(TabAvailable);
        }

        private void OnDisable()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestAccepted -= HandleQuestChanged;
                QuestManager.Instance.OnQuestObjectiveUpdated -= HandleQuestChanged;
                QuestManager.Instance.OnQuestCompleted -= HandleQuestChanged;
                QuestManager.Instance.OnQuestTurnedIn -= HandleQuestChanged;
                QuestManager.Instance.OnQuestListChanged -= RefreshList;
            }
        }

        public void ShowTab(int tabIndex)
        {
            _currentTab = tabIndex;
            if (_headerText != null)
            {
                _headerText.text = tabIndex switch
                {
                    TabActive => "ACTIVE QUESTS",
                    TabAvailable => "AVAILABLE QUESTS",
                    TabCompleted => "COMPLETED",
                    _ => "QUEST LOG"
                };
            }
            RefreshList();
        }

        public void NextTab()
        {
            _currentTab = (_currentTab + 1) % 3;
            ShowTab(_currentTab);
        }

        public void PrevTab()
        {
            _currentTab = (_currentTab + 2) % 3;
            ShowTab(_currentTab);
        }

        private void RefreshList()
        {
            if (_listContainer == null || QuestManager.Instance == null) return;

            for (int i = _listContainer.childCount - 1; i >= 0; i--)
                Destroy(_listContainer.GetChild(i).gameObject);

            List<QuestInstance> quests = _currentTab switch
            {
                TabActive => QuestManager.Instance.GetActiveQuests(),
                TabAvailable => QuestManager.Instance.GetAvailableQuests(),
                TabCompleted => QuestManager.Instance.GetQuestsByStatus(QuestStatus.Completed),
                _ => new List<QuestInstance>()
            };

            if (quests.Count == 0)
            {
                var empty = UIPrimitiveFactory.AddThemedText(_listContainer, "No quests in this tab.", 15, UIColors.Default.Muted);
                empty.alignment = TextAlignmentOptions.Center;
                return;
            }

            foreach (var quest in quests)
            {
                var row = new GameObject($"Quest_{quest.Data.DisplayName}", typeof(RectTransform));
                row.transform.SetParent(_listContainer, false);
                var le = row.AddComponent<LayoutElement>();
                le.preferredHeight = 40;
                var rbg = row.AddComponent<Image>();
                rbg.color = UIColors.Default.RowBg;
                var hlg = UIPrimitiveFactory.AddRowHLG(row);

                var nameT = UIPrimitiveFactory.AddThemedText(row.transform, quest.Data.DisplayName, 15, UIColors.Default.Text);
                nameT.alignment = TextAlignmentOptions.MidlineLeft;
                var nle = nameT.gameObject.AddComponent<LayoutElement>();
                nle.preferredWidth = 180;

                string progress = quest.IsComplete() ? "COMPLETE" : $"{quest.GetProgress():P0}";
                Color pColor = quest.IsComplete() ? UIColors.Default.Ok : UIColors.Default.Muted;
                var progT = UIPrimitiveFactory.AddThemedText(row.transform, progress, 14, pColor);
                progT.alignment = TextAlignmentOptions.MidlineRight;

                var btn = row.AddComponent<Button>();
                btn.targetGraphic = rbg;
                var captured = quest;
                btn.onClick.AddListener(() =>
                {
                    _selectedQuest = captured;
                    ShowDetail(captured);
                });
            }
        }

        private void ShowDetail(QuestInstance quest)
        {
            if (_detailPanel == null || quest == null) return;
            _detailPanel.SetActive(true);

            if (_detailName != null)
                _detailName.text = quest.Data.DisplayName;

            if (_detailDesc != null)
                _detailDesc.text = quest.Data.Description;

            if (_detailObjectives != null)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < quest.Data.Objectives.Count; i++)
                {
                    var obj = quest.Data.Objectives[i];
                    int prog = quest.Progress[i];
                    string check = prog >= obj.RequiredAmount ? "[OK]" : "[ ]";
                    sb.AppendLine($"{check} {obj.Description} ({prog}/{obj.RequiredAmount})");
                }
                _detailObjectives.text = sb.ToString();
            }

            if (_detailRewards != null)
            {
                var rewards = quest.Data.GetRewardMap();
                var parts = new List<string>();
                foreach (var kvp in rewards)
                    parts.Add($"{kvp.Key}: {kvp.Value}");
                if (quest.Data.XPReward > 0)
                    parts.Add($"XP: {quest.Data.XPReward}");
                _detailRewards.text = parts.Count > 0 ? "Rewards: " + string.Join(", ", parts) : "No rewards";
            }

            if (_acceptBtn != null)
            {
                _acceptBtn.gameObject.SetActive(quest.Status == QuestStatus.Available);
                _acceptBtn.interactable = quest.Status == QuestStatus.Available;
            }

            if (_turnInBtn != null)
            {
                _turnInBtn.gameObject.SetActive(quest.Status == QuestStatus.Completed);
                _turnInBtn.interactable = quest.Status == QuestStatus.Completed;
            }
        }

        public void AcceptSelectedQuest()
        {
            if (_selectedQuest == null || QuestManager.Instance == null) return;
            QuestManager.Instance.AcceptQuest(_selectedQuest.Data);
            ToastUI.Show($"Quest accepted: {_selectedQuest.Data.DisplayName}", UIColors.Default.Ok);
            _detailPanel.SetActive(false);
            _selectedQuest = null;
            RefreshList();
        }

        public void TurnInSelectedQuest()
        {
            if (_selectedQuest == null || QuestManager.Instance == null) return;
            QuestManager.Instance.TurnInQuest(_selectedQuest.Data);
            ToastUI.Show($"Quest turned in: {_selectedQuest.Data.DisplayName}", UIColors.Default.Gold);
            _detailPanel.SetActive(false);
            _selectedQuest = null;
            RefreshList();
        }

        private void HandleQuestChanged(QuestInstance _) => RefreshList();
    }
}
