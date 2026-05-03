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

        private GameObject CreateLeftPanel()
        {
            var panel = UIPrimitiveFactory.CreateUIObject("LeftPanel", transform).gameObject;
            var bg = panel.AddComponent<Image>();
            bg.color = UIColors.Default.RowBg;
            bg.raycastTarget = false;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.spacing = 6;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var header = UIPrimitiveFactory.AddThemedText(panel.transform, "BUILDING WORKERS", 22,
                UIColors.Default.Gold, TextAlignmentOptions.Center, UIStyleType.HeaderText);
            UIPrimitiveFactory.AddLayoutElement(header.gameObject, preferredHeight: 35);

            _summaryText = UIPrimitiveFactory.AddThemedText(panel.transform, "", 14,
                UIColors.Default.Text, TextAlignmentOptions.Center, UIStyleType.BodyText);
            UIPrimitiveFactory.AddLayoutElement(_summaryText.gameObject, preferredHeight: 50);

            var listObj = UIPrimitiveFactory.CreateUIObject("BuildingsList", panel.transform);
            UIPrimitiveFactory.AddLayoutElement(listObj.gameObject, preferredHeight: 300);
            var listVLG = listObj.gameObject.AddComponent<VerticalLayoutGroup>();
            listVLG.spacing = 4;
            listVLG.childControlWidth = true;
            listVLG.childControlHeight = false;
            listVLG.childForceExpandWidth = true;
            listVLG.childForceExpandHeight = false;
            _buildingsContainer = listObj.transform;

            return panel;
        }

        private GameObject CreateRightPanel()
        {
            var panel = UIPrimitiveFactory.CreateUIObject("RightPanel", transform).gameObject;
            var bg = panel.AddComponent<Image>();
            bg.color = UIColors.Default.RowBg;
            bg.raycastTarget = false;

            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.spacing = 6;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            _headerText = UIPrimitiveFactory.AddThemedText(panel.transform, "ACTIVE WORKERS", 22,
                UIColors.Default.Gold, TextAlignmentOptions.Center, UIStyleType.HeaderText);
            UIPrimitiveFactory.AddLayoutElement(_headerText.gameObject, preferredHeight: 35);

            var listObj = UIPrimitiveFactory.CreateUIObject("SettlersList", panel.transform);
            UIPrimitiveFactory.AddLayoutElement(listObj.gameObject, preferredHeight: 350);
            var listVLG = listObj.gameObject.AddComponent<VerticalLayoutGroup>();
            listVLG.spacing = 4;
            listVLG.childControlWidth = true;
            listVLG.childControlHeight = false;
            listVLG.childForceExpandWidth = true;
            listVLG.childForceExpandHeight = false;
            _settlersContainer = listObj.transform;

            return panel;
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

            _summaryText.text =
                $"Population: {population}  |  Settlers: {total}\n" +
                $"Working: <color=#{ColorUtility.ToHtmlStringRGB(UIColors.Default.Ok)}>{working}</color>  |  " +
                $"Idle: <color=#{ColorUtility.ToHtmlStringRGB(UIColors.Default.Warn)}>{idle}</color>";

            if (_headerText != null)
                _headerText.text = $"ACTIVE WORKERS ({total})";
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
                row.AddComponent<LayoutElement>().preferredHeight = 36;
                var rbg = row.AddComponent<Image>();
                rbg.color = UIColors.Default.RowBg;
                rbg.raycastTarget = false;
                UIPrimitiveFactory.AddRowHLG(row, new RectOffset(8, 8, 4, 4), 8);

                var nameT = UIPrimitiveFactory.AddThemedText(row.transform, building.Data.DisplayName, 14,
                    UIColors.Default.Text, TextAlignmentOptions.MidlineLeft);
                nameT.gameObject.AddComponent<LayoutElement>().preferredWidth = 140;

                var roleNames = new List<string>();
                foreach (var slot in building.Data.RequiredWorkers)
                    roleNames.Add($"{SettlerRoleInfo.GetDisplayName(slot.Role)} x{slot.Count}");
                string roleStr = string.Join(", ", roleNames);

                var roleT = UIPrimitiveFactory.AddThemedText(row.transform, roleStr, 13,
                    UIColors.Default.Muted, TextAlignmentOptions.MidlineLeft);
                roleT.gameObject.AddComponent<LayoutElement>().preferredWidth = 160;

                var ratioColor = ratio >= 1f ? UIColors.Default.Ok :
                    ratio > 0f ? UIColors.Default.Warn : UIColors.Default.Danger;
                UIPrimitiveFactory.AddThemedText(row.transform, $"{assigned}/{required}", 14,
                    ratioColor, TextAlignmentOptions.MidlineRight);
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
            if (jm == null)
            {
                var empty = UIPrimitiveFactory.AddThemedText(_settlersContainer,
                    "Job system not available.", 14, UIColors.Default.Muted);
                empty.alignment = TextAlignmentOptions.Center;
                return;
            }

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

                string roleName = walker.Role != SettlerRole.None
                    ? SettlerRoleInfo.GetDisplayName(walker.Role)
                    : "Idle";
                string buildingName = walker.AssignedBuilding != null
                    ? walker.AssignedBuilding.Data.DisplayName
                    : "-";
                string taskStr = walker.CurrentTask switch
                {
                    WalkerState.WalkingToTarget => "> Working",
                    WalkerState.WaitingAtTarget => "= Working",
                    WalkerState.ReturningHome => "< Returning",
                    WalkerState.Resting => "~ Resting",
                    _ => ""
                };

                var roleColor = walker.Role != SettlerRole.None ? UIColors.Default.Ok : UIColors.Default.Warn;

                var row = UIPrimitiveFactory.CreateUIObject($"Settler_{roleName}", _settlersContainer).gameObject;
                row.AddComponent<LayoutElement>().preferredHeight = 30;
                var rbg = row.AddComponent<Image>();
                rbg.color = UIColors.Default.RowBg;
                rbg.raycastTarget = false;
                UIPrimitiveFactory.AddRowHLG(row, new RectOffset(8, 8, 2, 2), 8);

                var roleT = UIPrimitiveFactory.AddThemedText(row.transform, roleName, 13,
                    roleColor, TextAlignmentOptions.MidlineLeft);
                roleT.gameObject.AddComponent<LayoutElement>().preferredWidth = 110;

                var bldT = UIPrimitiveFactory.AddThemedText(row.transform, buildingName, 13,
                    UIColors.Default.Text, TextAlignmentOptions.MidlineLeft);
                bldT.gameObject.AddComponent<LayoutElement>().preferredWidth = 130;

                if (!string.IsNullOrEmpty(taskStr))
                {
                    UIPrimitiveFactory.AddThemedText(row.transform, taskStr, 12,
                        UIColors.Default.Muted, TextAlignmentOptions.MidlineRight);
                }
            }
        }
    }
}
