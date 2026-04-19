using System.Collections.Generic;
using HollowGround.Quests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class QuestLogUI : MonoBehaviour
    {
        [SerializeField] private Transform _questListContainer;
        [SerializeField] private GameObject _questItemPrefab;
        [SerializeField] private GameObject _detailPanel;
        [SerializeField] private TMP_Text _detailName;
        [SerializeField] private TMP_Text _detailDesc;
        [SerializeField] private TMP_Text _detailObjectives;
        [SerializeField] private TMP_Text _detailRewards;
        [SerializeField] private Button _acceptBtn;
        [SerializeField] private Button _turnInBtn;
        [SerializeField] private Button _closeBtn;
        [SerializeField] private TMP_Text _tabLabel;

        private int _currentTab;
        private QuestInstance _selectedQuest;

        private const int TabActive = 0;
        private const int TabAvailable = 1;
        private const int TabCompleted = 2;

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

            if (_acceptBtn != null)
                _acceptBtn.onClick.AddListener(AcceptSelectedQuest);
            if (_turnInBtn != null)
                _turnInBtn.onClick.AddListener(TurnInSelectedQuest);
            if (_closeBtn != null)
                _closeBtn.onClick.AddListener(CloseDetail);

            ShowTab(TabActive);
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

            if (_acceptBtn != null)
                _acceptBtn.onClick.RemoveListener(AcceptSelectedQuest);
            if (_turnInBtn != null)
                _turnInBtn.onClick.RemoveListener(TurnInSelectedQuest);
            if (_closeBtn != null)
                _closeBtn.onClick.RemoveListener(CloseDetail);
        }

        public void ShowTab(int tabIndex)
        {
            _currentTab = tabIndex;

            if (_tabLabel != null)
            {
                _tabLabel.text = tabIndex switch
                {
                    TabActive => "Active Quests",
                    TabAvailable => "Available Quests",
                    TabCompleted => "Completed",
                    _ => "Quests"
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
            ClearContainer();

            if (QuestManager.Instance == null) return;

            List<QuestInstance> quests = _currentTab switch
            {
                TabActive => QuestManager.Instance.GetActiveQuests(),
                TabAvailable => QuestManager.Instance.GetAvailableQuests(),
                TabCompleted => QuestManager.Instance.GetQuestsByStatus(QuestStatus.Completed),
                _ => new List<QuestInstance>()
            };

            if (_questItemPrefab == null || _questListContainer == null) return;

            for (int i = 0; i < quests.Count; i++)
            {
                var quest = quests[i];
                var item = Instantiate(_questItemPrefab, _questListContainer);
                SetupQuestItem(item, quest, i);
            }
        }

        private void HandleQuestChanged(QuestInstance _) => RefreshList();

        private void SetupQuestItem(GameObject item, QuestInstance quest, int index)
        {
            var nameText = item.transform.Find("NameText")?.GetComponent<TMP_Text>();
            var statusText = item.transform.Find("StatusText")?.GetComponent<TMP_Text>();
            var progressSlider = item.transform.Find("ProgressSlider")?.GetComponent<Slider>();
            var button = item.GetComponent<Button>();

            if (nameText != null)
                nameText.text = quest.Data.DisplayName;

            if (statusText != null)
                statusText.text = quest.IsComplete() ? "Complete!" : $"{quest.GetProgress():P0}";

            if (progressSlider != null)
                progressSlider.value = quest.GetProgress();

            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    _selectedQuest = quest;
                    ShowDetail(quest);
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
                var lines = new List<string>();
                for (int i = 0; i < quest.Data.Objectives.Count; i++)
                {
                    var obj = quest.Data.Objectives[i];
                    int progress = quest.Progress[i];
                    string check = progress >= obj.RequiredAmount ? "[X]" : "[ ]";
                    lines.Add($"{check} {obj.Description} ({progress}/{obj.RequiredAmount})");
                }
                _detailObjectives.text = string.Join("\n", lines);
            }

            if (_detailRewards != null)
            {
                var rewards = quest.Data.GetRewardMap();
                var parts = new List<string>();
                foreach (var kvp in rewards)
                    parts.Add($"{kvp.Key}: {kvp.Value}");
                if (quest.Data.XPReward > 0)
                    parts.Add($"XP: {quest.Data.XPReward}");
                _detailRewards.text = parts.Count > 0 ? string.Join("\n", parts) : "No rewards";
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
            if (_selectedQuest == null) return;
            if (QuestManager.Instance == null) return;

            QuestManager.Instance.AcceptQuest(_selectedQuest.Data);
            CloseDetail();
            RefreshList();
        }

        public void TurnInSelectedQuest()
        {
            if (_selectedQuest == null) return;
            if (QuestManager.Instance == null) return;

            QuestManager.Instance.TurnInQuest(_selectedQuest.Data);
            CloseDetail();
            RefreshList();
        }

        public void CloseDetail()
        {
            if (_detailPanel != null)
                _detailPanel.SetActive(false);
            _selectedQuest = null;
        }

        private void ClearContainer()
        {
            if (_questListContainer == null) return;
            for (int i = _questListContainer.childCount - 1; i >= 0; i--)
                Destroy(_questListContainer.GetChild(i).gameObject);
        }
    }
}
