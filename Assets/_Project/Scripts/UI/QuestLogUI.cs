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
        private int _currentTab;
        private QuestInstance _selectedQuest;
        private TMP_Text _headerText;
        private TMP_Text _detailName;
        private TMP_Text _detailDesc;
        private TMP_Text _detailObjectives;
        private TMP_Text _detailRewards;
        private GameObject _detailPanel;
        private Button _acceptBtn;
        private Button _turnInBtn;
        private Transform _listContainer;
        private bool _built;

        private const int TabActive = 0;
        private const int TabAvailable = 1;
        private const int TabCompleted = 2;

        private void OnEnable()
        {
            if (!_built) BuildUI();
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

        private void BuildUI()
        {
            var root = GetComponent<RectTransform>();
            if (root == null) return;

            root.anchorMin = new Vector2(0f, 0f);
            root.anchorMax = new Vector2(1f, 1f);
            root.offsetMin = new Vector2(0f, 60f);
            root.offsetMax = new Vector2(0f, 0f);

            foreach (Transform child in root)
                Destroy(child.gameObject);

            var oldVlg = GetComponent<VerticalLayoutGroup>();
            if (oldVlg != null) DestroyImmediate(oldVlg);
            var oldImages = GetComponents<Image>();
            foreach (var img in oldImages) DestroyImmediate(img);

            UIPrimitiveFactory.SetupPanelBackground(gameObject, UIColors.Default);

            var vlg = UIPrimitiveFactory.AddStandardVLG(gameObject);

            var headerRow = new GameObject("Header", typeof(RectTransform));
            headerRow.transform.SetParent(root, false);
            var headerLE = headerRow.AddComponent<LayoutElement>();
            headerLE.preferredHeight = 40;
            var headerHLG = headerRow.AddComponent<HorizontalLayoutGroup>();
            headerHLG.spacing = 10;
            headerHLG.childAlignment = TextAnchor.MiddleCenter;
            headerHLG.childControlWidth = true;
            headerHLG.childControlHeight = true;
            headerHLG.childForceExpandWidth = true;
            headerHLG.childForceExpandHeight = false;

            var prevBtn = MakeTabButton(headerRow.transform, "< Prev", () => PrevTab());
            _headerText = UIPrimitiveFactory.AddThemedText(headerRow.transform, "QUEST LOG", 22, UIColors.Default.Gold);
            _headerText.alignment = TextAlignmentOptions.Center;
            var headerTextLE = _headerText.gameObject.AddComponent<LayoutElement>();
            headerTextLE.preferredWidth = 200;
            var nextBtn = MakeTabButton(headerRow.transform, "Next >", () => NextTab());

            var listObj = new GameObject("QuestList", typeof(RectTransform));
            listObj.transform.SetParent(root, false);
            var listLE = listObj.AddComponent<LayoutElement>();
            listLE.preferredHeight = 200;
            listLE.minHeight = 80;
            var listVLG = listObj.AddComponent<VerticalLayoutGroup>();
            listVLG.spacing = 4;
            listVLG.childControlWidth = true;
            listVLG.childControlHeight = false;
            listVLG.childForceExpandWidth = true;
            listVLG.childForceExpandHeight = false;
            _listContainer = listObj.transform;

            var detailObj = new GameObject("DetailPanel", typeof(RectTransform));
            detailObj.transform.SetParent(root, false);
            var detailLE = detailObj.AddComponent<LayoutElement>();
            detailLE.preferredHeight = 200;
            detailLE.minHeight = 100;
            var detailBg = detailObj.AddComponent<Image>();
            detailBg.color = UIColors.Default.RowBg;
            var detailVLG = detailObj.AddComponent<VerticalLayoutGroup>();
            detailVLG.padding = new RectOffset(15, 15, 10, 10);
            detailVLG.spacing = 6;
            detailVLG.childControlWidth = true;
            detailVLG.childControlHeight = false;
            detailVLG.childForceExpandWidth = true;
            detailVLG.childForceExpandHeight = false;

            _detailName = UIPrimitiveFactory.AddThemedText(detailObj.transform, "", 18, UIColors.Default.Text);
            _detailDesc = UIPrimitiveFactory.AddThemedText(detailObj.transform, "", 14, UIColors.Default.Muted);
            _detailObjectives = UIPrimitiveFactory.AddThemedText(detailObj.transform, "", 14, UIColors.Default.Text);
            _detailRewards = UIPrimitiveFactory.AddThemedText(detailObj.transform, "", 14, UIColors.Default.Gold);

            var btnRow = new GameObject("BtnRow", typeof(RectTransform));
            btnRow.transform.SetParent(detailObj.transform, false);
            var btnRowLE = btnRow.AddComponent<LayoutElement>();
            btnRowLE.preferredHeight = 40;
            var btnRowHLG = btnRow.AddComponent<HorizontalLayoutGroup>();
            btnRowHLG.spacing = 10;
            btnRowHLG.childAlignment = TextAnchor.MiddleCenter;
            btnRowHLG.childControlWidth = true;
            btnRowHLG.childControlHeight = true;
            btnRowHLG.childForceExpandWidth = true;
            btnRowHLG.childForceExpandHeight = false;

            _acceptBtn = MakeActionButton(btnRow.transform, "ACCEPT QUEST", AcceptSelectedQuest, UIStyleType.ConfirmButton);
            _turnInBtn = MakeActionButton(btnRow.transform, "TURN IN", TurnInSelectedQuest, UIStyleType.ConfirmButton);

            _detailPanel = detailObj;
            _detailPanel.SetActive(false);

            _built = true;
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
            if (!_built || _listContainer == null || QuestManager.Instance == null) return;

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
                    string check = prog >= obj.RequiredAmount ? "[X]" : "[ ]";
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

        private Button MakeTabButton(Transform parent, string label, System.Action onClick)
        {
            var btn = UIPrimitiveFactory.CreateThemedButton(parent, "TabBtn", label, onClick, UIStyleType.ActionBarButton);
            var btnLE = btn.gameObject.AddComponent<LayoutElement>();
            btnLE.minWidth = 80;
            btnLE.preferredWidth = 100;
            btnLE.minHeight = 32;
            return btn;
        }

        private Button MakeActionButton(Transform parent, string label, System.Action onClick, UIStyleType styleType)
        {
            var btn = UIPrimitiveFactory.CreateThemedButton(parent, "ActionBtn", label, onClick, styleType);
            var btnLE = btn.gameObject.AddComponent<LayoutElement>();
            btnLE.minWidth = 140;
            btnLE.preferredWidth = 180;
            btnLE.minHeight = 36;
            return btn;
        }
    }
}
