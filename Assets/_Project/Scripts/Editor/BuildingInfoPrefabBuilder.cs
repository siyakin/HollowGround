using HollowGround.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HollowGround.Editor
{
    public static class BuildingInfoPrefabBuilder
    {
        [MenuItem("HollowGround/UI/Build BuildingInfo Panel")]
        public static void Build()
        {
            var canvas = GameObject.Find("GameCanvas");
            if (canvas == null) { Debug.LogError("GameCanvas not found!"); return; }

            var existing = canvas.transform.Find("BuildingInfo") ?? canvas.transform.Find("BuildingInfoPanel");
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Replace?", "BuildingInfo already exists. Replace?", "Yes", "No")) return;
                Object.DestroyImmediate(existing.gameObject);
            }

            var root = new GameObject("BuildingInfo", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);

            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(320, 0);

            root.AddComponent<CanvasGroup>().blocksRaycasts = true;
            root.AddComponent<Image>().color = PanelBuilderUtil.PanelBg;

            var vlg = root.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 12, 12);
            vlg.spacing = 8;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            root.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var info = root.AddComponent<BuildingInfoUI>();
            root.SetActive(false);

            var headerRow = PanelBuilderUtil.CreateRow("Header", root.transform, 8);
            var nameText = PanelBuilderUtil.CreateTMP("NameText", headerRow.transform, "Building", 20, PanelBuilderUtil.GoldColor);
            nameText.GetComponent<LayoutElement>().flexibleWidth = 1;
            var levelText = PanelBuilderUtil.CreateTMP("LevelText", headerRow.transform, "Level 1", 14, PanelBuilderUtil.LabelColor);

            var stateRow = PanelBuilderUtil.CreateRow("StateRow", root.transform, 6);
            PanelBuilderUtil.CreateLabel(stateRow.transform, "State:");
            var stateText = PanelBuilderUtil.CreateTMP("StateText", stateRow.transform, "Active", 14, PanelBuilderUtil.OkColor);
            stateText.GetComponent<LayoutElement>().flexibleWidth = 1;

            PanelBuilderUtil.AddSeparator(root.transform);

            var progressSection = PanelBuilderUtil.CreateSection("Progress", root.transform);
            var progressSlider = PanelBuilderUtil.CreateSlider("ProgressSlider", progressSection.transform, new Color(0.3f, 0.7f, 1f, 0.9f));
            var progressLabel = PanelBuilderUtil.CreateTMP("ProgressLabel", progressSection.transform, "50%", 12, PanelBuilderUtil.TextColor);
            progressLabel.alignment = TextAlignmentOptions.Center;

            PanelBuilderUtil.AddSeparator(root.transform);

            var productionGroup = PanelBuilderUtil.CreateSection("Production", root.transform);
            var prodRow = PanelBuilderUtil.CreateRow("ProdRow", productionGroup.transform, 6);
            PanelBuilderUtil.CreateLabel(prodRow.transform, "Output:");
            var productionText = PanelBuilderUtil.CreateTMP("ProductionText", prodRow.transform, "10 Food", 13, PanelBuilderUtil.TextColor);
            productionText.GetComponent<LayoutElement>().flexibleWidth = 1;
            var productionSlider = PanelBuilderUtil.CreateSlider("ProdSlider", productionGroup.transform, new Color(0.2f, 0.8f, 0.3f, 0.9f));

            PanelBuilderUtil.AddSeparator(root.transform);

            var workersText = PanelBuilderUtil.CreateTMP("WorkersText", root.transform, "Workers: 0/0", 13, new Color(0.7f, 0.9f, 1f));
            workersText.alignment = TextAlignmentOptions.TopLeft;

            PanelBuilderUtil.AddSeparator(root.transform);

            var actionsSection = PanelBuilderUtil.CreateSection("Actions", root.transform);

            var upgradeRow = PanelBuilderUtil.CreateRow("UpgradeRow", actionsSection.transform, 6);
            var upgradeBtn = PanelBuilderUtil.CreateButton("UpgradeBtn", upgradeRow.transform, "UPGRADE", PanelBuilderUtil.OkColor, 28);
            upgradeBtn.GetComponent<LayoutElement>().preferredWidth = 100;
            var upgradeCostGO = ResourceCostDisplayBuilder.Create(upgradeRow.transform, "UpgradeCost");
            upgradeCostGO.GetComponent<LayoutElement>().flexibleWidth = 1;

            var repairRow = PanelBuilderUtil.CreateRow("RepairRow", actionsSection.transform, 6);
            var repairBtn = PanelBuilderUtil.CreateButton("RepairBtn", repairRow.transform, "REPAIR", PanelBuilderUtil.AccentColor, 28);
            repairBtn.GetComponent<LayoutElement>().preferredWidth = 100;
            var repairCostGO = ResourceCostDisplayBuilder.Create(repairRow.transform, "RepairCost");
            repairCostGO.GetComponent<LayoutElement>().flexibleWidth = 1;

            var demolishRow = PanelBuilderUtil.CreateRow("DemolishRow", actionsSection.transform, 6);
            var demolishBtn = PanelBuilderUtil.CreateButton("DemolishBtn", demolishRow.transform, "DEMOLISH [D]", PanelBuilderUtil.DangerColor, 28);
            demolishBtn.GetComponent<LayoutElement>().flexibleWidth = 1;

            var so = new SerializedObject(info);
            PanelBuilderUtil.WireField(so, "_nameText", nameText);
            PanelBuilderUtil.WireField(so, "_levelText", levelText);
            PanelBuilderUtil.WireField(so, "_stateText", stateText);
            PanelBuilderUtil.WireField(so, "_progressSlider", progressSlider);
            PanelBuilderUtil.WireField(so, "_progressLabel", progressLabel);
            PanelBuilderUtil.WireField(so, "_productionGroup", productionGroup);
            PanelBuilderUtil.WireField(so, "_productionText", productionText);
            PanelBuilderUtil.WireField(so, "_productionSlider", productionSlider);
            PanelBuilderUtil.WireField(so, "_upgradeButton", upgradeBtn);
            PanelBuilderUtil.WireField(so, "_upgradeCostDisplay", upgradeCostGO.GetComponent<ResourceCostDisplay>());
            PanelBuilderUtil.WireField(so, "_demolishButton", demolishBtn);
            PanelBuilderUtil.WireField(so, "_repairButton", repairBtn);
            PanelBuilderUtil.WireField(so, "_repairCostDisplay", repairCostGO.GetComponent<ResourceCostDisplay>());
            PanelBuilderUtil.WireField(so, "_workersText", workersText);
            so.ApplyModifiedProperties();

            Selection.activeGameObject = root;
            Debug.Log("BuildingInfoPanel created.");
        }
    }
}
