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
        private static readonly Color PanelBg = new(0.08f, 0.09f, 0.11f, 0.92f);
        private static readonly Color RowBg = new(0.14f, 0.15f, 0.17f, 1f);
        private static readonly Color ColorText = new(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color ColorMuted = new(0.65f, 0.65f, 0.7f, 1f);
        private static readonly Color ColorOk = new(0.35f, 0.8f, 0.4f, 1f);
        private static readonly Color ColorGold = new(1f, 0.85f, 0.3f, 1f);

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

            var bg = gameObject.AddComponent<Image>();
            bg.color = PanelBg;
            bg.raycastTarget = true;

            var cg = gameObject.GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
            cg.interactable = true;
            cg.blocksRaycasts = true;

            var vlg = gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 15, 15);
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

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
            _headerText = AddText(headerRow.transform, "QUEST LOG", 22, ColorGold);
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
            detailBg.color = RowBg;
            var detailVLG = detailObj.AddComponent<VerticalLayoutGroup>();
            detailVLG.padding = new RectOffset(15, 15, 10, 10);
            detailVLG.spacing = 6;
            detailVLG.childControlWidth = true;
            detailVLG.childControlHeight = false;
            detailVLG.childForceExpandWidth = true;
            detailVLG.childForceExpandHeight = false;

            _detailName = AddText(detailObj.transform, "", 18, ColorText);
            _detailDesc = AddText(detailObj.transform, "", 14, ColorMuted);
            _detailObjectives = AddText(detailObj.transform, "", 14, ColorText);
            _detailRewards = AddText(detailObj.transform, "", 14, ColorGold);

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

            _acceptBtn = MakeActionButton(btnRow.transform, "ACCEPT QUEST", ColorOk, AcceptSelectedQuest);
            _turnInBtn = MakeActionButton(btnRow.transform, "TURN IN", ColorGold, TurnInSelectedQuest);

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
                var empty = AddText(_listContainer, "No quests in this tab.", 15, ColorMuted);
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
                rbg.color = RowBg;
                var hlg = row.AddComponent<HorizontalLayoutGroup>();
                hlg.padding = new RectOffset(12, 12, 4, 4);
                hlg.spacing = 10;
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = true;
                hlg.childForceExpandHeight = false;

                var nameT = AddText(row.transform, quest.Data.DisplayName, 15, ColorText);
                nameT.alignment = TextAlignmentOptions.MidlineLeft;
                var nle = nameT.gameObject.AddComponent<LayoutElement>();
                nle.preferredWidth = 180;

                string progress = quest.IsComplete() ? "COMPLETE" : $"{quest.GetProgress():P0}";
                Color pColor = quest.IsComplete() ? ColorOk : ColorMuted;
                var progT = AddText(row.transform, progress, 14, pColor);
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
            ToastUI.Show($"Quest accepted: {_selectedQuest.Data.DisplayName}", ColorOk);
            _detailPanel.SetActive(false);
            _selectedQuest = null;
            RefreshList();
        }

        public void TurnInSelectedQuest()
        {
            if (_selectedQuest == null || QuestManager.Instance == null) return;
            QuestManager.Instance.TurnInQuest(_selectedQuest.Data);
            ToastUI.Show($"Quest turned in: {_selectedQuest.Data.DisplayName}", ColorGold);
            _detailPanel.SetActive(false);
            _selectedQuest = null;
            RefreshList();
        }

        private void HandleQuestChanged(QuestInstance _) => RefreshList();

        private Button MakeTabButton(Transform parent, string label, System.Action onClick)
        {
            var btnObj = new GameObject("TabBtn", typeof(RectTransform));
            btnObj.transform.SetParent(parent, false);
            var btnLE = btnObj.AddComponent<LayoutElement>();
            btnLE.minWidth = 80;
            btnLE.preferredWidth = 100;
            btnLE.minHeight = 32;
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.2f, 0.22f, 1f);
            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            var btnLabel = AddText(btnObj.transform, label, 14, ColorText);
            btnLabel.alignment = TextAlignmentOptions.Center;
            StretchFull(btnLabel.rectTransform);
            btn.onClick.AddListener(() => onClick());
            return btn;
        }

        private Button MakeActionButton(Transform parent, string label, Color color, System.Action onClick)
        {
            var btnObj = new GameObject("ActionBtn", typeof(RectTransform));
            btnObj.transform.SetParent(parent, false);
            var btnLE = btnObj.AddComponent<LayoutElement>();
            btnLE.minWidth = 140;
            btnLE.preferredWidth = 180;
            btnLE.minHeight = 36;
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = color;
            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            var btnLabel = AddText(btnObj.transform, label, 15, Color.black);
            btnLabel.alignment = TextAlignmentOptions.Center;
            StretchFull(btnLabel.rectTransform);
            btn.onClick.AddListener(() => onClick());
            return btn;
        }

        private static TMP_Text AddText(Transform parent, string text, float size, Color color)
        {
            var go = new GameObject("T", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
