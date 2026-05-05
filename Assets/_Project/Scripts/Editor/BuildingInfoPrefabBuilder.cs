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
            if (canvas == null)
            {
                Debug.LogError("GameCanvas not found!");
                return;
            }

            var existing = canvas.transform.Find("BuildingInfoPanel");
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Replace?",
                    "BuildingInfoPanel already exists. Replace?", "Yes", "No"))
                    return;
                Object.DestroyImmediate(existing.gameObject);
            }

            var panel = CreatePanel(canvas.transform);
            Selection.activeGameObject = panel;
            Debug.Log("BuildingInfoPanel created. Select it and adjust visually in Inspector.");
        }

        private static GameObject CreatePanel(Transform parent)
        {
            var root = new GameObject("BuildingInfoPanel", typeof(RectTransform));
            root.transform.SetParent(parent, false);

            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(320, 0);

            var canvasGroup = root.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = true;

            var rootImg = root.AddComponent<Image>();
            rootImg.color = new Color(0.12f, 0.12f, 0.15f, 0.95f);

            var rootVLG = root.AddComponent<VerticalLayoutGroup>();
            rootVLG.padding = new RectOffset(12, 12, 12, 12);
            rootVLG.spacing = 8;
            rootVLG.childControlWidth = true;
            rootVLG.childControlHeight = false;
            rootVLG.childForceExpandWidth = true;
            rootVLG.childForceExpandHeight = false;

            var csf = root.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var info = root.AddComponent<BuildingInfoUI>();
            root.SetActive(false);

            // --- HEADER ---
            var headerRow = CreateRow("Header", root.transform);
            var nameText = CreateTMP("NameText", headerRow.transform, "Building", 20, Color.yellow);
            nameText.GetComponent<LayoutElement>().flexibleWidth = 1;
            var levelText = CreateTMP("LevelText", headerRow.transform, "Level 1", 16, new Color(0.7f, 0.7f, 0.7f));

            // --- STATE ---
            var stateRow = CreateRow("StateRow", root.transform);
            CreateLabel(stateRow.transform, "State:");
            var stateText = CreateTMP("StateText", stateRow.transform, "Active", 14, Color.green);
            stateText.GetComponent<LayoutElement>().flexibleWidth = 1;

            AddSeparator(root.transform);

            // --- PROGRESS ---
            var progressBox = CreateSectionBox("Progress", root.transform);
            var progressSlider = progressBox.AddComponent<Slider>();
            progressSlider.interactable = false;
            progressSlider.targetGraphic = progressBox.GetComponent<Image>();
            var sliderBg = progressBox.GetComponent<Image>();
            sliderBg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(progressBox.transform, false);
            var handleAreaRT = handleArea.GetComponent<RectTransform>();
            handleAreaRT.anchorMin = Vector2.zero;
            handleAreaRT.anchorMax = Vector2.one;
            handleAreaRT.offsetMin = Vector2.zero;
            handleAreaRT.offsetMax = Vector2.zero;
            var handle = new GameObject("Handle", typeof(RectTransform));
            handle.transform.SetParent(handleArea.transform, false);
            var handleRT = handle.GetComponent<RectTransform>();
            handleRT.sizeDelta = new Vector2(0, 0);
            var handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.clear;
            progressSlider.handleRect = handleRT;
            progressSlider.direction = Slider.Direction.LeftToRight;
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(progressBox.transform, false);
            var fillAreaRT = fillArea.GetComponent<RectTransform>();
            fillAreaRT.anchorMin = Vector2.zero;
            fillAreaRT.anchorMax = Vector2.one;
            fillAreaRT.offsetMin = Vector2.zero;
            fillAreaRT.offsetMax = Vector2.zero;
            var sliderFill = new GameObject("Fill", typeof(RectTransform));
            sliderFill.transform.SetParent(fillArea.transform, false);
            var fillRT = sliderFill.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(1, 1);
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;
            var fillImg = sliderFill.AddComponent<Image>();
            fillImg.color = new Color(0.3f, 0.7f, 1f, 0.9f);
            progressSlider.fillRect = fillRT;
            progressSlider.value = 0f;

            var progressLabelRow = CreateRow("ProgressLabel", progressBox.transform);
            var progressLabel = CreateTMP("ProgressLabel", progressLabelRow.transform, "50%", 12, Color.white);
            progressLabel.alignment = TextAlignmentOptions.Center;

            AddSeparator(root.transform);

            // --- PRODUCTION ---
            var productionGroup = CreateSectionBox("Production", root.transform);
            var prodRow = CreateRow("ProdRow", productionGroup.transform);
            CreateLabel(prodRow.transform, "Output:");
            var productionText = CreateTMP("ProductionText", prodRow.transform, "10 Food", 13, Color.white);
            productionText.GetComponent<LayoutElement>().flexibleWidth = 1;

            var prodSlider = productionGroup.AddComponent<Slider>();
            prodSlider.interactable = false;
            prodSlider.targetGraphic = productionGroup.GetComponents<Image>()[0];
            var prodHandleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            prodHandleArea.transform.SetParent(productionGroup.transform, false);
            var prodHART = prodHandleArea.GetComponent<RectTransform>();
            prodHART.anchorMin = Vector2.zero;
            prodHART.anchorMax = Vector2.one;
            prodHART.offsetMin = Vector2.zero;
            prodHART.offsetMax = Vector2.zero;
            var prodHandle = new GameObject("Handle", typeof(RectTransform));
            prodHandle.transform.SetParent(prodHandleArea.transform, false);
            var prodHRT = prodHandle.GetComponent<RectTransform>();
            prodHRT.sizeDelta = Vector2.zero;
            prodHandle.AddComponent<Image>().color = Color.clear;
            prodSlider.handleRect = prodHRT;
            prodSlider.direction = Slider.Direction.LeftToRight;
            var prodFillArea = new GameObject("Fill Area", typeof(RectTransform));
            prodFillArea.transform.SetParent(productionGroup.transform, false);
            var prodFART = prodFillArea.GetComponent<RectTransform>();
            prodFART.anchorMin = Vector2.zero;
            prodFART.anchorMax = Vector2.one;
            prodFART.offsetMin = Vector2.zero;
            prodFART.offsetMax = Vector2.zero;
            var prodSliderFill = new GameObject("Fill", typeof(RectTransform));
            prodSliderFill.transform.SetParent(prodFillArea.transform, false);
            var prodFRT = prodSliderFill.GetComponent<RectTransform>();
            prodFRT.anchorMin = Vector2.zero;
            prodFRT.anchorMax = Vector2.one;
            prodFRT.offsetMin = Vector2.zero;
            prodFRT.offsetMax = Vector2.zero;
            var prodFillImg = prodSliderFill.AddComponent<Image>();
            prodFillImg.color = new Color(0.2f, 0.8f, 0.3f, 0.9f);
            prodSlider.fillRect = prodFRT;
            prodSlider.value = 0f;

            AddSeparator(root.transform);

            // --- WORKERS ---
            var workersText = CreateTMP("WorkersText", root.transform, "Workers: 0/0", 13, new Color(0.7f, 0.9f, 1f));
            workersText.alignment = TextAlignmentOptions.TopLeft;

            AddSeparator(root.transform);

            // --- ACTIONS ---
            var actionsBox = CreateSectionBox("Actions", root.transform);

            var upgradeRow = CreateRow("UpgradeRow", actionsBox.transform);
            var upgradeBtn = CreateTMPButton("UpgradeBtn", upgradeRow.transform, "Upgrade", new Color(0.2f, 0.5f, 0.2f));
            upgradeBtn.GetComponent<LayoutElement>().flexibleWidth = 1;
            var upgradeCostDisplay = ResourceCostDisplayBuilder.Create(upgradeRow.transform, "UpgradeCostDisplay");
            upgradeCostDisplay.GetComponent<LayoutElement>().flexibleWidth = 1;

            var repairRow = CreateRow("RepairRow", actionsBox.transform);
            var repairBtn = CreateTMPButton("RepairBtn", repairRow.transform, "Repair", new Color(0.2f, 0.4f, 0.7f));
            repairBtn.GetComponent<LayoutElement>().flexibleWidth = 1;
            var repairCostDisplay = ResourceCostDisplayBuilder.Create(repairRow.transform, "RepairCostDisplay");
            repairCostDisplay.GetComponent<LayoutElement>().flexibleWidth = 1;

            var demolishRow = CreateRow("DemolishRow", actionsBox.transform);
            var demolishBtn = CreateTMPButton("DemolishBtn", demolishRow.transform, "Demolish [D]", new Color(0.7f, 0.2f, 0.2f));
            demolishBtn.GetComponent<LayoutElement>().flexibleWidth = 1;

            // --- WIRE SERIALIZED FIELDS ---
            var so = new SerializedObject(info);
            so.FindProperty("_nameText").objectReferenceValue = nameText;
            so.FindProperty("_levelText").objectReferenceValue = levelText;
            so.FindProperty("_stateText").objectReferenceValue = stateText;
            so.FindProperty("_progressSlider").objectReferenceValue = progressSlider;
            so.FindProperty("_progressLabel").objectReferenceValue = progressLabel;
            so.FindProperty("_productionGroup").objectReferenceValue = productionGroup;
            so.FindProperty("_productionText").objectReferenceValue = productionText;
            so.FindProperty("_productionSlider").objectReferenceValue = prodSlider;
            so.FindProperty("_upgradeButton").objectReferenceValue = upgradeBtn;
            so.FindProperty("_upgradeCostDisplay").objectReferenceValue = upgradeCostDisplay.GetComponent<ResourceCostDisplay>();
            so.FindProperty("_demolishButton").objectReferenceValue = demolishBtn;
            so.FindProperty("_repairButton").objectReferenceValue = repairBtn;
            so.FindProperty("_repairCostDisplay").objectReferenceValue = repairCostDisplay.GetComponent<ResourceCostDisplay>();
            so.FindProperty("_workersText").objectReferenceValue = workersText;
            so.ApplyModifiedProperties();

            return root;
        }

        private static GameObject CreateRow(string name, Transform parent)
        {
            var row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            return row;
        }

        private static GameObject CreateSectionBox(string name, Transform parent)
        {
            var box = new GameObject(name, typeof(RectTransform));
            box.transform.SetParent(parent, false);
            var img = box.AddComponent<Image>();
            img.color = new Color(0.16f, 0.16f, 0.19f, 0.9f);
            var vlg = box.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(8, 8, 6, 6);
            vlg.spacing = 4;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            return box;
        }

        private static void AddSeparator(Transform parent)
        {
            var sep = new GameObject("Separator", typeof(RectTransform));
            sep.transform.SetParent(parent, false);
            sep.AddComponent<LayoutElement>().preferredHeight = 1;
            var img = sep.AddComponent<Image>();
            img.color = new Color(0.4f, 0.4f, 0.4f, 0.4f);
        }

        private static TMP_Text CreateLabel(Transform parent, string text, int size = 12, Color? color = null)
        {
            var label = CreateTMP("Label", parent, text, size, color ?? new Color(0.6f, 0.6f, 0.6f));
            label.GetComponent<LayoutElement>().preferredWidth = 60;
            return label;
        }

        private static TMP_Text CreateTMP(string name, Transform parent, string text, int size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            go.AddComponent<LayoutElement>();
            if (PanelBuilderUtil.ThemeFont != null) tmp.font = PanelBuilderUtil.ThemeFont;
            return tmp;
        }

        private static Button CreateTMPButton(string name, Transform parent, string text, Color bgColor)
        {
            var btnObj = new GameObject(name, typeof(RectTransform));
            btnObj.transform.SetParent(parent, false);

            var bgImg = btnObj.AddComponent<Image>();
            bgImg.color = bgColor;
            bgImg.raycastTarget = true;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bgImg;

            var labelObj = new GameObject("Label", typeof(RectTransform));
            labelObj.transform.SetParent(btnObj.transform, false);
            var tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 12;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            if (PanelBuilderUtil.ThemeFont != null) tmp.font = PanelBuilderUtil.ThemeFont;
            var labelRT = labelObj.GetComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;

            var le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = 28;

            return btn;
        }
    }
}
