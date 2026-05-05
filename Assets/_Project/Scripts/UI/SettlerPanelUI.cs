using System.Collections.Generic;
using HollowGround.Buildings;
using HollowGround.Core;
using HollowGround.Domain.Walkers;
using HollowGround.NPCs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class SettlerPanelUI : MonoBehaviour
    {
        private TMP_Text _headerText;
        private TMP_Text _summaryText;
        private TMP_Text _warningsText;
        private Transform _buildingsContainer;
        private Transform _settlersContainer;
        private GameObject _leftPanel;
        private GameObject _rightPanel;
        private bool _built;

        private void OnEnable()
        {
            if (!_built) BuildUI();
            Refresh();

            if (SettlerJobManager.Instance != null)
            {
                SettlerJobManager.Instance.OnJobAssigned += OnJobChanged;
                SettlerJobManager.Instance.OnJobReleased += OnJobChanged;
            }

            if (SettlerManager.Instance != null)
            {
                SettlerManager.Instance.OnSettlerSpawned += OnSettlerChanged;
                SettlerManager.Instance.OnSettlerRemoved += OnSettlerChanged;
            }
        }

        private void OnDisable()
        {
            if (SettlerJobManager.Instance != null)
            {
                SettlerJobManager.Instance.OnJobAssigned -= OnJobChanged;
                SettlerJobManager.Instance.OnJobReleased -= OnJobChanged;
            }

            if (SettlerManager.Instance != null)
            {
                SettlerManager.Instance.OnSettlerSpawned -= OnSettlerChanged;
                SettlerManager.Instance.OnSettlerRemoved -= OnSettlerChanged;
            }
        }

        private void OnJobChanged(SettlerWalker walker, Building building) => Refresh();
        private void OnSettlerChanged(SettlerWalker walker) => Refresh();

        private void BuildUI()
        {
            var root = GetComponent<RectTransform>();
            if (root == null) return;

            UIPrimitiveFactory.SetupPanelBackground(gameObject, UIColors.Default);
            UIPrimitiveFactory.StretchFull(root, new Vector2(0, 60), Vector2.zero);

            foreach (Transform child in root)
                Destroy(child.gameObject);

            var mainHLG = gameObject.AddComponent<HorizontalLayoutGroup>();
            mainHLG.padding = new RectOffset(15, 15, 15, 15);
            mainHLG.spacing = 15;
            mainHLG.childControlWidth = true;
            mainHLG.childControlHeight = true;
            mainHLG.childForceExpandWidth = true;
            mainHLG.childForceExpandHeight = true;

            _leftPanel = CreateLeftPanel();
            _rightPanel = CreateRightPanel();

            _built = true;
        }

        private (GameObject root, Transform content) CreateScrollView(string name, Transform parent)
        {
            var scrollObj = UIPrimitiveFactory.CreateUIObject(name, parent);
            var scrollRect = scrollObj.gameObject.AddComponent<ScrollRect>();

            var viewport = UIPrimitiveFactory.CreateUIObject("Viewport", scrollObj);
            UIPrimitiveFactory.StretchFull(viewport);
            viewport.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.15f);
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;

            var content = UIPrimitiveFactory.CreateUIObject("Content", viewport);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.spacing = 8;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = content;
            scrollRect.viewport = viewport;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 20f;

            return (scrollObj.gameObject, content);
        }

        private void AddSeparator(Transform parent)
        {
            var sep = UIPrimitiveFactory.CreateUIObject("Separator", parent);
            UIPrimitiveFactory.AddLayoutElement(sep.gameObject, preferredHeight: 1);
            sep.gameObject.AddComponent<Image>().color = UIColors.Default.Muted * 0.4f;
        }

        private GameObject CreateLeftPanel()
        {
            var panelObj = UIPrimitiveFactory.CreateUIObject("LeftPanel", transform).gameObject;
            var vlg = panelObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Header Area
            var headerRow = UIPrimitiveFactory.CreateUIObject("HeaderRow", panelObj.transform);
            UIPrimitiveFactory.AddLayoutElement(headerRow.gameObject, preferredHeight: 40);
            UIPrimitiveFactory.AddRowHLG(headerRow.gameObject, new RectOffset(5, 5, 0, 0), 10);

            UIPrimitiveFactory.AddThemedText(headerRow, "BUILDING WORKERS", 20,
                UIColors.Default.Gold, TextAlignmentOptions.MidlineLeft, UIStyleType.HeaderText);

            var autoBtn = UIPrimitiveFactory.CreateButton(headerRow, "AutoAssignToggle", "Auto-Assign: ON", null, UIColors.Default.Ok);
            UIPrimitiveFactory.AddLayoutElement(autoBtn.gameObject, preferredWidth: 140);

            AddSeparator(panelObj.transform);

            // Summary Section
            var summaryBox = UIPrimitiveFactory.CreateUIObject("SummaryBox", panelObj.transform);
            UIPrimitiveFactory.AddLayoutElement(summaryBox.gameObject, preferredHeight: 80);
            summaryBox.gameObject.AddComponent<Image>().color = UIColors.Default.RowBg * 0.8f;
            var summaryVLG = summaryBox.gameObject.AddComponent<VerticalLayoutGroup>();
            summaryVLG.padding = new RectOffset(10, 10, 10, 10);

            _summaryText = UIPrimitiveFactory.AddThemedText(summaryBox, "", 14,
                UIColors.Default.Text, TextAlignmentOptions.TopLeft, UIStyleType.BodyText);

            _warningsText = UIPrimitiveFactory.AddThemedText(summaryBox, "<i>No alerts.</i>", 12,
                UIColors.Default.Muted, TextAlignmentOptions.BottomLeft, UIStyleType.BodyText);

            AddSeparator(panelObj.transform);

            // Scroll Area
            var scrollData = CreateScrollView("BuildingsScroll", panelObj.transform);
            UIPrimitiveFactory.AddLayoutElement(scrollData.root, preferredHeight: 100).flexibleHeight = 1;
            _buildingsContainer = scrollData.content;

            return panelObj;
        }

        private GameObject CreateRightPanel()
        {
            var panelObj = UIPrimitiveFactory.CreateUIObject("RightPanel", transform).gameObject;
            var vlg = panelObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Header Area
            var headerRow = UIPrimitiveFactory.CreateUIObject("HeaderRow", panelObj.transform);
            UIPrimitiveFactory.AddLayoutElement(headerRow.gameObject, preferredHeight: 40);
            UIPrimitiveFactory.AddRowHLG(headerRow.gameObject, new RectOffset(5, 5, 0, 0), 10);

            _headerText = UIPrimitiveFactory.AddThemedText(headerRow, "ACTIVE SETTLERS", 20,
                UIColors.Default.Gold, TextAlignmentOptions.MidlineLeft, UIStyleType.HeaderText);

            var sortGroup = UIPrimitiveFactory.CreateUIObject("SortGroup", headerRow);
            UIPrimitiveFactory.AddRowHLG(sortGroup.gameObject, new RectOffset(0, 0, 0, 0), 4);
            
            var btnRole = UIPrimitiveFactory.CreateButton(sortGroup, "SortRole", "Role", null);
            UIPrimitiveFactory.AddLayoutElement(btnRole.gameObject, preferredWidth: 50);
            
            var btnStat = UIPrimitiveFactory.CreateButton(sortGroup, "SortStatus", "Stat", null);
            UIPrimitiveFactory.AddLayoutElement(btnStat.gameObject, preferredWidth: 50);
            
            var btnBld = UIPrimitiveFactory.CreateButton(sortGroup, "SortBld", "Bld", null);
            UIPrimitiveFactory.AddLayoutElement(btnBld.gameObject, preferredWidth: 50);

            AddSeparator(panelObj.transform);

            // Scroll Area
            var scrollData = CreateScrollView("SettlersScroll", panelObj.transform);
            UIPrimitiveFactory.AddLayoutElement(scrollData.root, preferredHeight: 100).flexibleHeight = 1;
            _settlersContainer = scrollData.content;

            return panelObj;
        }

        private void Refresh()
        {
            if (!_built) return;

            RefreshSummary();
            RefreshBuildings();
            RefreshSettlers();
        }

        private void RefreshSummary()
        {
            var sm = SettlerManager.Instance;
            var jm = SettlerJobManager.Instance;
            if (sm == null || _summaryText == null) return;

            int total = sm.SettlerCount;
            int idle = jm != null ? jm.IdleCount : total;
            int working = jm != null ? jm.WorkingCount : 0;
            int population = sm.TotalPopulation;

            string c_ok = ColorUtility.ToHtmlStringRGB(UIColors.Default.Ok);
            string c_warn = ColorUtility.ToHtmlStringRGB(UIColors.Default.Warn);
            string c_gold = ColorUtility.ToHtmlStringRGB(UIColors.Default.Gold);

            _summaryText.text =
                $"Population: <color=#{c_gold}>{population}</color>  |  Settlers: <color=#{c_gold}>{total}</color>\n" +
                $"Working: <color=#{c_ok}>{working}</color>  |  Idle: <color=#{c_warn}>{idle}</color>";

            if (_warningsText != null)
            {
                if (idle > 0)
                    _warningsText.text = $"<color=#{c_warn}>! {idle} settlers are currently idle.</color>";
                else
                    _warningsText.text = "<i>All settlers are assigned tasks.</i>";
            }

            if (_headerText != null)
                _headerText.text = $"ACTIVE SETTLERS ({total})";
        }

        private void RefreshBuildings()
        {
            if (_buildingsContainer == null) return;
            foreach (Transform child in _buildingsContainer)
                Destroy(child.gameObject);

            if (BuildingManager.Instance == null) return;

            var jm = SettlerJobManager.Instance;
            bool anyWorkers = false;

            foreach (var building in BuildingManager.Instance.AllBuildings)
            {
                if (building.State != BuildingState.Active) continue;
                if (building.Data.RequiredWorkers == null || building.Data.RequiredWorkers.Count == 0) continue;

                anyWorkers = true;
                int assigned = jm != null ? jm.GetAssignedWorkerCount(building) : 0;
                int required = building.Data.GetTotalRequiredWorkers();
                float ratio = required > 0 ? (float)assigned / required : 1f;

                var row = UIPrimitiveFactory.CreateUIObject($"Bld_{building.Data.DisplayName}", _buildingsContainer).gameObject;
                UIPrimitiveFactory.AddLayoutElement(row, preferredHeight: 44);
                var rbg = row.AddComponent<Image>();
                rbg.color = UIColors.Default.RowBg;
                UIPrimitiveFactory.AddRowHLG(row, new RectOffset(8, 8, 4, 4), 10);

                var nameCol = UIPrimitiveFactory.CreateUIObject("NameCol", row.transform);
                var nameVLG = nameCol.gameObject.AddComponent<VerticalLayoutGroup>();
                nameVLG.childControlHeight = true;
                nameVLG.childForceExpandHeight = false;
                nameCol.gameObject.AddComponent<LayoutElement>().preferredWidth = 140;

                UIPrimitiveFactory.AddThemedText(nameCol, building.Data.DisplayName, 14,
                    UIColors.Default.Text, TextAlignmentOptions.MidlineLeft);

                var roleNames = new List<string>();
                foreach (var slot in building.Data.RequiredWorkers)
                    roleNames.Add($"{SettlerRoleInfo.GetDisplayName(slot.Role)} x{slot.Count}");
                UIPrimitiveFactory.AddThemedText(nameCol, string.Join(", ", roleNames), 11,
                    UIColors.Default.Muted, TextAlignmentOptions.MidlineLeft);

                // Progress Bar
                var barContainer = UIPrimitiveFactory.CreateUIObject("BarContainer", row.transform);
                UIPrimitiveFactory.AddLayoutElement(barContainer.gameObject, preferredWidth: 100, preferredHeight: 12);
                barContainer.gameObject.AddComponent<Image>().color = Color.black * 0.4f;

                var fillObj = UIPrimitiveFactory.CreateUIObject("Fill", barContainer);
                UIPrimitiveFactory.StretchFull(fillObj);
                var fill = fillObj.gameObject.AddComponent<Image>();
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;
                fill.fillAmount = ratio;
                fill.color = ratio >= 1f ? UIColors.Default.Ok :
                            ratio > 0.5f ? UIColors.Default.Warn : UIColors.Default.Danger;
            }

            if (!anyWorkers)
            {
                var empty = UIPrimitiveFactory.AddThemedText(_buildingsContainer,
                    "No active buildings require workers.", 14, UIColors.Default.Muted);
                empty.alignment = TextAlignmentOptions.Center;
            }
        }

        private void RefreshSettlers()
        {
            if (_settlersContainer == null) return;
            foreach (Transform child in _settlersContainer)
                Destroy(child.gameObject);

            var jm = SettlerJobManager.Instance;
            if (jm == null) return;

            var allSettlers = jm.GetAllSettlers();
            if (allSettlers.Count == 0)
            {
                var empty = UIPrimitiveFactory.AddThemedText(_settlersContainer,
                    "No settlers yet. Build shelters.", 14, UIColors.Default.Muted);
                empty.alignment = TextAlignmentOptions.Center;
                return;
            }

            foreach (var walker in allSettlers)
            {
                if (walker == null) continue;

                var row = UIPrimitiveFactory.CreateUIObject($"Settler_{walker.GetHashCode()}", _settlersContainer).gameObject;
                UIPrimitiveFactory.AddLayoutElement(row, preferredHeight: 40);
                var rbg = row.AddComponent<Image>();
                rbg.color = UIColors.Default.RowBg;
                UIPrimitiveFactory.AddRowHLG(row, new RectOffset(8, 8, 4, 4), 8);

                // Click to highlight logic placeholder
                var btn = row.AddComponent<Button>();
                btn.targetGraphic = rbg;

                // Role Dot
                var dotObj = UIPrimitiveFactory.CreateUIObject("RoleDot", row.transform);
                UIPrimitiveFactory.AddLayoutElement(dotObj.gameObject, preferredWidth: 8, preferredHeight: 8);
                var dot = dotObj.gameObject.AddComponent<Image>();
                dot.color = walker.Role != SettlerRole.None ? UIColors.Default.Ok : UIColors.Default.Warn;

                // Role & Name
                var nameCol = UIPrimitiveFactory.CreateUIObject("NameCol", row.transform);
                nameCol.gameObject.AddComponent<LayoutElement>().preferredWidth = 110;
                var roleName = walker.Role != SettlerRole.None ? SettlerRoleInfo.GetDisplayName(walker.Role) : "Idle";
                UIPrimitiveFactory.AddThemedText(nameCol, roleName, 13, UIColors.Default.Text);

                // Task Icon Placeholder
                var taskIcon = UIPrimitiveFactory.CreateUIObject("TaskIcon", row.transform);
                UIPrimitiveFactory.AddLayoutElement(taskIcon.gameObject, preferredWidth: 16, preferredHeight: 16);
                taskIcon.gameObject.AddComponent<Image>().color = UIColors.Default.Muted * 0.5f;

                string taskStr = walker.CurrentTask switch
                {
                    WalkerState.WalkingToTarget => "> Working",
                    WalkerState.WaitingAtTarget => "= Working",
                    WalkerState.ReturningHome => "< Returning",
                    WalkerState.Resting => "~ Resting",
                    _ => ""
                };
                if (!string.IsNullOrEmpty(taskStr))
                {
                    var taskText = UIPrimitiveFactory.AddThemedText(row.transform, taskStr, 12,
                        UIColors.Default.Muted, TextAlignmentOptions.MidlineRight);
                    taskText.gameObject.AddComponent<LayoutElement>().preferredWidth = 70;
                }

                // Target
                var bldT = UIPrimitiveFactory.AddThemedText(row.transform,
                    walker.AssignedBuilding != null ? walker.AssignedBuilding.Data.DisplayName : "-",
                    12, UIColors.Default.Text);
                bldT.gameObject.AddComponent<LayoutElement>().preferredWidth = 90;

                // Morale & Reassign
                var rightCol = UIPrimitiveFactory.CreateUIObject("RightCol", row.transform);
                UIPrimitiveFactory.AddRowHLG(rightCol.gameObject, new RectOffset(0, 0, 0, 0), 6);

                var moraleBg = UIPrimitiveFactory.CreateUIObject("MoraleBar", rightCol);
                UIPrimitiveFactory.AddLayoutElement(moraleBg.gameObject, preferredWidth: 40, preferredHeight: 4);
                moraleBg.gameObject.AddComponent<Image>().color = Color.black * 0.3f;
                var moraleFill = UIPrimitiveFactory.CreateUIObject("Fill", moraleBg);
                UIPrimitiveFactory.StretchFull(moraleFill);
                moraleFill.gameObject.AddComponent<Image>().color = UIColors.Default.Muted;

                var reassignBtn = UIPrimitiveFactory.CreateButton(rightCol, "ReassignBtn", "R", null);
                UIPrimitiveFactory.AddLayoutElement(reassignBtn.gameObject, preferredWidth: 24);
            }
        }
    }
}

