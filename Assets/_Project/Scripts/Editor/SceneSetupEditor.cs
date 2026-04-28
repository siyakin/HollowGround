using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using HollowGround.UI;
using HollowGround.Tech;
using HollowGround.NPCs;

namespace HollowGround.Editor
{
    public static class SceneSetupEditor
    {
        const string CANVAS_NAME = "GameCanvas";

        static readonly Color DarkPanel = new(0.05f, 0.05f, 0.04f, 0.92f);
        static readonly Color ActionBar = new(0.06f, 0.07f, 0.05f, 0.82f);
        static readonly Color AccentColor = new(0.83f, 0.52f, 0.04f, 1f);
        static readonly Color BodyTextColor = new(0.85f, 0.85f, 0.85f, 1f);
        static readonly Color LabelTextColor = new(0.60f, 0.60f, 0.55f, 1f);
        static readonly Color WarningColor = new(0.95f, 0.65f, 0.10f, 1f);
        static readonly Color BonusColor = new(0.6f, 0.8f, 0.5f, 1f);

        [MenuItem("HollowGround/Setup UI Panels")]
        public static void SetupAllUIPanels()
        {
            Canvas canvas = FindOrCreateCanvas();
            if (canvas == null) return;

            int created = 0;
            created += SetupBuildingInfoPanel(canvas);
            created += SetupToastPanel(canvas);
            created += SetupTechTreePanel(canvas);
            created += SetupFactionTradePanel(canvas);
            created += SetupQuestLogPanel(canvas);

            WireUIManager(canvas);

            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log($"[SceneSetup] {created} UI panel created/verified.");
        }

        static void WireUIManager(Canvas canvas)
        {
            var uiManager = canvas.GetComponent<UIManager>() ?? canvas.gameObject.AddComponent<UIManager>();

            SerializedObject so = new SerializedObject(uiManager);

            WirePanel(so, canvas, "_buildingInfoPanel", "BuildingInfoPanel");
            WirePanel(so, canvas, "_toastPanel", "ToastPanel");
            WirePanel(so, canvas, "_techTreePanel", "TechTreePanel");
            WirePanel(so, canvas, "_factionTradePanel", "FactionTradePanel");
            WirePanel(so, canvas, "_questLogPanel", "QuestLogPanel");
            WirePanel(so, canvas, "_buildMenuPanel", "BuildMenuPanel");
            WirePanel(so, canvas, "_resourceBarPanel", "ResourceBarPanel");
            WirePanel(so, canvas, "_pausePanel", "PausePanel");
            WirePanel(so, canvas, "_trainingPanel", "TrainingPanel");
            WirePanel(so, canvas, "_armyPanel", "ArmyPanel");
            WirePanel(so, canvas, "_battleReportPanel", "BattleReportPanel");
            WirePanel(so, canvas, "_heroPanel", "HeroPanel");
            WirePanel(so, canvas, "_worldMapPanel", "WorldMapPanel");
            WirePanel(so, canvas, "_saveMenuPanel", "SaveMenuPanel");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(uiManager);
        }

        static void WirePanel(SerializedObject so, Canvas canvas, string field, string panelName)
        {
            Transform t = canvas.transform.Find(panelName);
            if (t == null) return;
            var prop = so.FindProperty(field);
            if (prop != null)
                prop.objectReferenceValue = t.gameObject;
        }

        static void DestroyExisting(Canvas canvas, string panelName)
        {
            Transform existing = canvas.transform.Find(panelName);
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);
        }

        static Canvas FindOrCreateCanvas()
        {
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null) return canvas;

            GameObject go = new(CANVAS_NAME);
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        static GameObject CreatePanel(Canvas canvas, string name, bool activeByDefault = false)
        {
            Transform existing = canvas.transform.Find(name);
            if (existing != null)
            {
                existing.gameObject.SetActive(activeByDefault);
                return existing.gameObject;
            }

            GameObject panel = new(name);
            panel.transform.SetParent(canvas.transform, false);
            panel.SetActive(activeByDefault);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = panel.AddComponent<Image>();
            bg.color = Color.clear;

            return panel;
        }

        static GameObject MakeChild(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, Color? bg = null)
        {
            GameObject go = new(name);
            go.transform.SetParent(parent, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            if (bg.HasValue)
            {
                Image img = go.AddComponent<Image>();
                img.color = bg.Value;
            }

            return go;
        }

        static TMP_Text MakeLabel(Transform parent, string objName, string text,
            Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax, Color color, int fontSize,
            UIStyleType styleTag = UIStyleType.LabelText)
        {
            GameObject go = MakeChild(parent, objName, aMin, aMax, oMin, oMax);
            TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.color = color;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Left;
            go.AddComponent<UIThemeTag>().styleType = styleTag;
            return tmp;
        }

        static TMP_Text MakeText(Transform parent, string text, Color color, int fontSize)
        {
            GameObject go = new("Text");
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(4, 2);
            rt.offsetMax = new Vector2(-4, -2);

            TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.color = color;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            return tmp;
        }

        static Button MakeButton(Transform parent, string objName, string label,
            Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax, Color bgColor,
            UIStyleType styleTag = UIStyleType.ConfirmButton)
        {
            GameObject go = MakeChild(parent, objName, aMin, aMax, oMin, oMax, bgColor);
            Button btn = go.AddComponent<Button>();

            Image img = go.GetComponent<Image>();
            btn.targetGraphic = img;

            ColorBlock cb = new()
            {
                normalColor = Color.white,
                highlightedColor = AccentColor,
                pressedColor = new Color(0.55f, 0.32f, 0.02f, 1f),
                disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f),
                colorMultiplier = 1f,
                fadeDuration = 0.12f
            };
            btn.colors = cb;

            MakeText(go.transform, label, Color.white, 16);
            go.AddComponent<UIThemeTag>().styleType = styleTag;
            return btn;
        }

        static Slider MakeSlider(Transform parent, string name,
            Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax)
        {
            GameObject go = MakeChild(parent, name, aMin, aMax, oMin, oMax);
            Slider slider = go.AddComponent<Slider>();

            GameObject bg = MakeChild(go.transform, "Background", Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, new Color(0.15f, 0.15f, 0.13f, 0.9f));
            Image bgImg = bg.GetComponent<Image>();

            GameObject fillArea = new("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            RectTransform frt = fillArea.AddComponent<RectTransform>();
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.offsetMin = Vector2.zero;
            frt.offsetMax = Vector2.zero;

            GameObject fill = new("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRt = fill.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(1, 1);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = AccentColor;

            slider.targetGraphic = bgImg;
            slider.fillRect = fillRt;

            return slider;
        }

        static void SetSerializedField(SerializedObject so, string field, Object value)
        {
            var prop = so.FindProperty(field);
            if (prop != null)
                prop.objectReferenceValue = value;
        }

        static GameObject CreateQuestItemPrefab()
        {
            string prefabPath = "Assets/_Project/Prefabs/UI/QuestItem.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) return existing;

            GameObject prefab = new("QuestItem");

            RectTransform rt = prefab.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = new Vector2(0, 60);

            Image bg = prefab.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.08f, 0.9f);

            Button btn = prefab.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = AccentColor,
                pressedColor = new Color(0.55f, 0.32f, 0.02f, 1f),
                disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f),
                colorMultiplier = 1f,
                fadeDuration = 0.12f
            };

            HorizontalLayoutGroup hlg = prefab.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.spacing = 5;
            hlg.padding = new RectOffset(8, 8, 4, 4);

            GameObject nameGO = new("NameText");
            nameGO.transform.SetParent(prefab.transform, false);
            var nameTmp = nameGO.AddComponent<TextMeshProUGUI>();
            nameTmp.text = "Quest";
            nameTmp.color = Color.white;
            nameTmp.fontSize = 14;
            nameTmp.alignment = TextAlignmentOptions.Left;

            GameObject statusGO = new("StatusText");
            statusGO.transform.SetParent(prefab.transform, false);
            var statusTmp = statusGO.AddComponent<TextMeshProUGUI>();
            statusTmp.text = "0%";
            statusTmp.color = WarningColor;
            statusTmp.fontSize = 12;
            statusTmp.alignment = TextAlignmentOptions.Right;

            GameObject sliderGO = new("ProgressSlider");
            sliderGO.transform.SetParent(prefab.transform, false);
            var slider = sliderGO.AddComponent<Slider>();
            var sliderBg = new GameObject("Background");
            sliderBg.transform.SetParent(sliderGO.transform, false);
            var sliderBgRt = sliderBg.AddComponent<RectTransform>();
            sliderBgRt.anchorMin = Vector2.zero;
            sliderBgRt.anchorMax = Vector2.one;
            var sliderBgImg = sliderBg.AddComponent<Image>();
            sliderBgImg.color = new Color(0.15f, 0.15f, 0.13f, 0.9f);

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGO.transform, false);
            var faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero;
            faRt.anchorMax = Vector2.one;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRt = fill.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = AccentColor;

            slider.targetGraphic = sliderBgImg;
            slider.fillRect = fillRt;

            var savedPrefab = PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            Object.DestroyImmediate(prefab);
            return savedPrefab;
        }

        static GameObject CreateOfferItemPrefab()
        {
            string prefabPath = "Assets/_Project/Prefabs/UI/OfferItem.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) return existing;

            GameObject prefab = new("OfferItem");

            RectTransform rt = prefab.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 40);

            Image bg = prefab.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.08f, 0.8f);

            HorizontalLayoutGroup hlg = prefab.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.spacing = 5;
            hlg.padding = new RectOffset(8, 8, 4, 4);

            GameObject nameGO = new("NameText");
            nameGO.transform.SetParent(prefab.transform, false);
            var nameTmp = nameGO.AddComponent<TextMeshProUGUI>();
            nameTmp.text = "Resource x1";
            nameTmp.color = BodyTextColor;
            nameTmp.fontSize = 13;
            nameTmp.alignment = TextAlignmentOptions.Left;

            GameObject priceGO = new("PriceText");
            priceGO.transform.SetParent(prefab.transform, false);
            var priceTmp = priceGO.AddComponent<TextMeshProUGUI>();
            priceTmp.text = "0 TechParts";
            priceTmp.color = WarningColor;
            priceTmp.fontSize = 12;
            priceTmp.alignment = TextAlignmentOptions.Midline;

            GameObject actionBtnGO = new("ActionButton");
            actionBtnGO.transform.SetParent(prefab.transform, false);
            var actionImg = actionBtnGO.AddComponent<Image>();
            actionImg.color = new Color(0.13f, 0.17f, 0.10f, 0.95f);
            var actionBtn = actionBtnGO.AddComponent<Button>();
            actionBtn.targetGraphic = actionImg;

            var actionTextGO = new GameObject("Text");
            actionTextGO.transform.SetParent(actionBtnGO.transform, false);
            var atRt = actionTextGO.AddComponent<RectTransform>();
            atRt.anchorMin = Vector2.zero;
            atRt.anchorMax = Vector2.one;
            atRt.offsetMin = new Vector2(4, 2);
            atRt.offsetMax = new Vector2(-4, -2);
            var atTmp = actionTextGO.AddComponent<TextMeshProUGUI>();
            atTmp.text = "Buy";
            atTmp.color = Color.white;
            atTmp.fontSize = 13;
            atTmp.alignment = TextAlignmentOptions.Center;

            var savedPrefab = PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            Object.DestroyImmediate(prefab);
            return savedPrefab;
        }

        #region BuildingInfo

        static int SetupBuildingInfoPanel(Canvas canvas)
        {
            DestroyExisting(canvas, "BuildingInfoPanel");
            GameObject panel = CreatePanel(canvas, "BuildingInfoPanel", false);
            var infoUI = panel.GetComponent<BuildingInfoUI>() ?? panel.AddComponent<BuildingInfoUI>();

            Image panelBg = panel.GetComponent<Image>();
            if (panelBg != null) panelBg.color = DarkPanel;

            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.5f, 0.5f);
            prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(350, 500);
            prt.anchoredPosition = new Vector2(300, 0);

            MakeChild(panel.transform, "Header",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -50), new Vector2(-10, -10));
            var nameText = MakeLabel(panel.transform, "NameText", "Building Name",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -45), new Vector2(-20, -15), Color.white, 22,
                UIStyleType.HeaderText);

            var levelText = MakeLabel(panel.transform, "LevelText", "Level 1",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -75), new Vector2(-20, -50), LabelTextColor, 16,
                UIStyleType.LabelText);

            var stateText = MakeLabel(panel.transform, "StateText", "Constructing",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -100), new Vector2(-20, -80), WarningColor, 14,
                UIStyleType.WarningText);

            var progressSlider = MakeSlider(panel.transform, "ProgressSlider",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -140), new Vector2(-20, -120));

            var progressLabel = MakeLabel(panel.transform, "ProgressLabel", "0%",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -160), new Vector2(-20, -140), Color.white, 12);

            GameObject productionGroup = MakeChild(panel.transform, "ProductionGroup",
                new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(10, -20), new Vector2(-10, 40));

            var productionText = MakeLabel(productionGroup.transform, "ProductionText", "10 Food / 5dk",
                new Vector2(0, 0.5f), new Vector2(1, 1), new Vector2(10, 5), new Vector2(-10, -5), BodyTextColor, 14,
                UIStyleType.BodyText);

            var productionSlider = MakeSlider(productionGroup.transform, "ProductionSlider",
                new Vector2(0, 0), new Vector2(1, 0.4f), new Vector2(0, 0), new Vector2(0, 0));

            var upgradeBtn = MakeButton(panel.transform, "UpgradeButton", "UPGRADE",
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(20, 60), new Vector2(-20, 95),
                new Color(0.13f, 0.17f, 0.10f, 0.95f), UIStyleType.ConfirmButton);

            var upgradeCostText = MakeLabel(panel.transform, "UpgradeCostText", "Cost: -",
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(20, 100), new Vector2(-20, 80), LabelTextColor, 12,
                UIStyleType.CostText);

            var demolishBtn = MakeButton(panel.transform, "DemolishButton", "DEMOLISH",
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(20, 15), new Vector2(-20, 50),
                new Color(0.35f, 0.07f, 0.07f, 0.95f), UIStyleType.DangerButton);

            var repairBtn = MakeButton(panel.transform, "RepairButton", "REPAIR",
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(20, 105), new Vector2(-20, 140),
                new Color(0.13f, 0.25f, 0.10f, 0.95f), UIStyleType.ConfirmButton);

            var repairCostText = MakeLabel(panel.transform, "RepairCostText", "Repair: -",
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(20, 145), new Vector2(-20, 125), LabelTextColor, 12,
                UIStyleType.CostText);

            SerializedObject so = new SerializedObject(infoUI);
            SetSerializedField(so, "_nameText", nameText);
            SetSerializedField(so, "_levelText", levelText);
            SetSerializedField(so, "_stateText", stateText);
            SetSerializedField(so, "_progressSlider", progressSlider);
            SetSerializedField(so, "_progressLabel", progressLabel);
            SetSerializedField(so, "_productionGroup", productionGroup);
            SetSerializedField(so, "_productionText", productionText);
            SetSerializedField(so, "_productionSlider", productionSlider);
            SetSerializedField(so, "_upgradeButton", upgradeBtn);
            SetSerializedField(so, "_upgradeCostText", upgradeCostText);
            SetSerializedField(so, "_demolishButton", demolishBtn);
            SetSerializedField(so, "_repairButton", repairBtn);
            SetSerializedField(so, "_repairCostText", repairCostText);
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(infoUI);
            return 1;
        }

        #endregion

        #region Toast

        static int SetupToastPanel(Canvas canvas)
        {
            DestroyExisting(canvas, "ToastPanel");
            DestroyExisting(canvas, "ToastUI");
            GameObject panel = CreatePanel(canvas, "ToastUI", true);
            var toastUI = panel.GetComponent<ToastUI>() ?? panel.AddComponent<ToastUI>();

            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = Vector2.zero;
            prt.anchorMax = Vector2.one;
            prt.pivot = new Vector2(0.5f, 0.5f);
            prt.offsetMin = Vector2.zero;
            prt.offsetMax = Vector2.zero;

            Image panelBg = panel.GetComponent<Image>();
            if (panelBg != null) panelBg.color = Color.clear;

            var cg = panel.GetComponent<CanvasGroup>() ?? panel.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;

            EditorUtility.SetDirty(toastUI);
            return 1;
        }

        #endregion

        #region TechTree

        static int SetupTechTreePanel(Canvas canvas)
        {
            DestroyExisting(canvas, "TechTreePanel");
            GameObject panel = CreatePanel(canvas, "TechTreePanel", false);
            var techUI = panel.GetComponent<TechTreeUI>() ?? panel.AddComponent<TechTreeUI>();

            Image panelBg = panel.GetComponent<Image>();
            if (panelBg != null) panelBg.color = DarkPanel;

            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.15f, 0.1f);
            prt.anchorMax = new Vector2(0.85f, 0.9f);
            prt.offsetMin = Vector2.zero;
            prt.offsetMax = Vector2.zero;

            GameObject header = MakeChild(panel.transform, "Header",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -50), new Vector2(-10, -10));
            MakeText(header.transform, "TECH TREE", Color.white, 24);

            GameObject cardsContainer = new("CardsContainer");
            cardsContainer.transform.SetParent(panel.transform, false);
            RectTransform ccrt = cardsContainer.AddComponent<RectTransform>();
            ccrt.anchorMin = new Vector2(0, 0);
            ccrt.anchorMax = new Vector2(1, 1);
            ccrt.offsetMin = new Vector2(10, 60);
            ccrt.offsetMax = new Vector2(-10, -60);

            GridLayoutGroup glg = cardsContainer.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(200, 150);
            glg.spacing = new Vector2(10, 10);
            glg.childAlignment = TextAnchor.UpperCenter;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 4;

            GameObject detailPanel = MakeChild(panel.transform, "DetailPanel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-180, -200), new Vector2(180, 200),
                new Color(0.08f, 0.08f, 0.06f, 0.96f));

            var detailName = MakeLabel(detailPanel.transform, "DetailName", "Tech Name",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -35), new Vector2(-10, -5), Color.white, 20,
                UIStyleType.HeaderText);

            var detailDesc = MakeLabel(detailPanel.transform, "DetailDesc", "Description",
                new Vector2(0, 0.5f), new Vector2(1, 0.7f), new Vector2(10, 10), new Vector2(-10, -10), BodyTextColor, 14,
                UIStyleType.BodyText);

            var detailCost = MakeLabel(detailPanel.transform, "DetailCost", "Cost: -",
                new Vector2(0, 0.3f), new Vector2(1, 0.5f), new Vector2(10, 0), new Vector2(-10, 0), WarningColor, 13,
                UIStyleType.CostText);

            var detailBonuses = MakeLabel(detailPanel.transform, "DetailBonuses", "Bonuses: -",
                new Vector2(0, 0.15f), new Vector2(1, 0.35f), new Vector2(10, 0), new Vector2(-10, 0), BonusColor, 13,
                UIStyleType.BodyText);

            Button researchBtn = MakeButton(detailPanel.transform, "ResearchBtn", "RESEARCH",
                new Vector2(0.2f, 0), new Vector2(0.8f, 0), new Vector2(0, 15), new Vector2(0, 50),
                new Color(0.13f, 0.17f, 0.10f, 0.95f));

            TMP_Text researchBtnText = researchBtn.GetComponentInChildren<TMP_Text>();

            string[] techGuids = AssetDatabase.FindAssets("t:TechNode", new[] { "Assets/_Project/ScriptableObjects/Tech" });

            SerializedObject so = new SerializedObject(techUI);
            SetSerializedField(so, "_detailPanel", detailPanel);
            SetSerializedField(so, "_detailName", detailName);
            SetSerializedField(so, "_detailDesc", detailDesc);
            SetSerializedField(so, "_detailCost", detailCost);
            SetSerializedField(so, "_detailBonuses", detailBonuses);
            SetSerializedField(so, "_detailResearchBtn", researchBtn);
            SetSerializedField(so, "_detailResearchBtnText", researchBtnText);

            var techCardsProp = so.FindProperty("_techCards");
            if (techCardsProp != null && techGuids.Length > 0)
            {
                techCardsProp.arraySize = 0;
                for (int i = 0; i < techGuids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(techGuids[i]);
                    var techData = AssetDatabase.LoadAssetAtPath<TechNode>(path);
                    if (techData == null) continue;

                    GameObject cardGO = MakeChild(cardsContainer.transform, $"TechCard_{techData.name}",
                        Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero,
                        new Color(0.1f, 0.1f, 0.08f, 0.9f));

                    var cardBtn = cardGO.AddComponent<Button>();
                    var cardImg = cardGO.GetComponent<Image>();
                    cardBtn.targetGraphic = cardImg;

                    var cardName = MakeLabel(cardGO.transform, "NameText", techData.DisplayName,
                        new Vector2(0, 0.6f), new Vector2(1, 1), new Vector2(5, 0), new Vector2(-5, 0), Color.white, 14,
                        UIStyleType.HeaderText);

                    var cardDesc = MakeLabel(cardGO.transform, "DescText", "",
                        new Vector2(0, 0.3f), new Vector2(1, 0.6f), new Vector2(5, 0), new Vector2(-5, 0), BodyTextColor, 11,
                        UIStyleType.BodyText);

                    var cardCost = MakeLabel(cardGO.transform, "CostText", "",
                        new Vector2(0, 0.1f), new Vector2(0.6f, 0.3f), new Vector2(5, 0), new Vector2(-5, 0), WarningColor, 11,
                        UIStyleType.CostText);
                    var cardStatus = MakeLabel(cardGO.transform, "StatusText", "Locked",
                        new Vector2(0.6f, 0.1f), new Vector2(1, 0.3f), new Vector2(5, 0), new Vector2(-5, 0), LabelTextColor, 11);

                    GameObject lockedOverlay = MakeChild(cardGO.transform, "LockedOverlay",
                        Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                        new Color(0f, 0f, 0f, 0.5f));

                    var cardSlider = MakeSlider(cardGO.transform, "ProgressSlider",
                        new Vector2(0, 0), new Vector2(1, 0.1f), new Vector2(5, 0), new Vector2(-5, 0));

                    int idx = i;
                    cardBtn.onClick.AddListener(() => { });

                    techCardsProp.arraySize++;
                    var element = techCardsProp.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("Data").objectReferenceValue = techData;
                    element.FindPropertyRelative("Button").objectReferenceValue = cardBtn;
                    element.FindPropertyRelative("NameText").objectReferenceValue = cardName;
                    element.FindPropertyRelative("DescText").objectReferenceValue = cardDesc;
                    element.FindPropertyRelative("CostText").objectReferenceValue = cardCost;
                    element.FindPropertyRelative("StatusText").objectReferenceValue = cardStatus;
                    element.FindPropertyRelative("LockedOverlay").objectReferenceValue = lockedOverlay;
                    element.FindPropertyRelative("ProgressSlider").objectReferenceValue = cardSlider;
                }
            }

            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(techUI);
            return 1;
        }

        #endregion

        #region FactionTrade

        static int SetupFactionTradePanel(Canvas canvas)
        {
            DestroyExisting(canvas, "FactionTradePanel");
            GameObject panel = CreatePanel(canvas, "FactionTradePanel", false);
            var tradeUI = panel.GetComponent<FactionTradeUI>() ?? panel.AddComponent<FactionTradeUI>();

            Image panelBg = panel.GetComponent<Image>();
            if (panelBg != null) panelBg.color = DarkPanel;

            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.1f, 0.1f);
            prt.anchorMax = new Vector2(0.9f, 0.9f);
            prt.offsetMin = Vector2.zero;
            prt.offsetMax = Vector2.zero;

            GameObject header = MakeChild(panel.transform, "Header",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -50), new Vector2(-10, -10));
            MakeText(header.transform, "FACTION TRADE", Color.white, 24);

            GameObject factionList = new("FactionList");
            factionList.transform.SetParent(panel.transform, false);
            RectTransform flrt = factionList.AddComponent<RectTransform>();
            flrt.anchorMin = new Vector2(0, 0);
            flrt.anchorMax = new Vector2(0.3f, 1);
            flrt.offsetMin = new Vector2(10, 10);
            flrt.offsetMax = new Vector2(-5, -60);

            VerticalLayoutGroup vlg = factionList.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            GameObject tradePanel = MakeChild(panel.transform, "TradePanel",
                new Vector2(0.3f, 0), new Vector2(1, 1), new Vector2(5, 10), new Vector2(-10, -60),
                new Color(0.08f, 0.08f, 0.06f, 0.6f));

            var factionNameText = MakeLabel(tradePanel.transform, "FactionName", "Faction",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -35), new Vector2(-10, -5), Color.white, 20,
                UIStyleType.HeaderText);

            var factionRelationText = MakeLabel(tradePanel.transform, "FactionRelation", "Relation: Neutral",
                new Vector2(0, 0.85f), new Vector2(1, 0.95f), new Vector2(10, 0), new Vector2(-10, 0), WarningColor, 14,
                UIStyleType.WarningText);

            var factionDescText = MakeLabel(tradePanel.transform, "FactionDesc", "Description",
                new Vector2(0, 0.75f), new Vector2(1, 0.85f), new Vector2(10, 0), new Vector2(-10, 0), BodyTextColor, 13,
                UIStyleType.BodyText);

            GameObject sellContainer = new("SellOffersContainer");
            sellContainer.transform.SetParent(tradePanel.transform, false);
            RectTransform scrt = sellContainer.AddComponent<RectTransform>();
            scrt.anchorMin = new Vector2(0, 0);
            scrt.anchorMax = new Vector2(0.48f, 0.7f);
            scrt.offsetMin = new Vector2(10, 10);
            scrt.offsetMax = new Vector2(-10, 0);

            VerticalLayoutGroup svlg = sellContainer.AddComponent<VerticalLayoutGroup>();
            svlg.spacing = 4;
            svlg.childControlWidth = true;
            svlg.childControlHeight = false;

            GameObject buyContainer = new("BuyOffersContainer");
            buyContainer.transform.SetParent(tradePanel.transform, false);
            RectTransform bcrt = buyContainer.AddComponent<RectTransform>();
            bcrt.anchorMin = new Vector2(0.52f, 0);
            bcrt.anchorMax = new Vector2(1f, 0.7f);
            bcrt.offsetMin = new Vector2(10, 10);
            bcrt.offsetMax = new Vector2(-10, 0);

            VerticalLayoutGroup bvlg = buyContainer.AddComponent<VerticalLayoutGroup>();
            bvlg.spacing = 4;
            bvlg.childControlWidth = true;
            bvlg.childControlHeight = false;

            var closeBtn = MakeButton(panel.transform, "CloseTradeBtn", "CLOSE",
                new Vector2(0.4f, 0), new Vector2(0.6f, 0), new Vector2(0, 15), new Vector2(0, 50),
                new Color(0.18f, 0.18f, 0.16f, 0.95f), UIStyleType.DangerButton);

            GameObject offerItemPrefab = CreateOfferItemPrefab();

            SerializedObject so = new SerializedObject(tradeUI);
            SetSerializedField(so, "_tradePanel", tradePanel);
            SetSerializedField(so, "_factionNameText", factionNameText);
            SetSerializedField(so, "_factionRelationText", factionRelationText);
            SetSerializedField(so, "_factionDescText", factionDescText);
            SetSerializedField(so, "_sellOffersContainer", sellContainer.transform);
            SetSerializedField(so, "_buyOffersContainer", buyContainer.transform);
            SetSerializedField(so, "_closeTradeBtn", closeBtn);
            SetSerializedField(so, "_offerItemPrefab", offerItemPrefab);

            string[] factionGuids = AssetDatabase.FindAssets("t:FactionData", new[] { "Assets/_Project/ScriptableObjects/Factions" });
            var factionSlotProp = so.FindProperty("_factionSlots");

            if (factionSlotProp != null && factionGuids.Length > 0)
            {
                factionSlotProp.arraySize = 0;
                for (int i = 0; i < factionGuids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(factionGuids[i]);
                    var factionData = AssetDatabase.LoadAssetAtPath<FactionData>(path);
                    if (factionData == null) continue;

                    GameObject slotGO = MakeChild(factionList.transform, $"FactionSlot_{factionData.name}",
                        Vector2.zero, Vector2.one, new Vector2(0, 0), new Vector2(0, 50),
                        new Color(0.1f, 0.1f, 0.08f, 0.9f));
                    var slotBtn = slotGO.AddComponent<Button>();
                    var slotBg = slotGO.GetComponent<Image>();
                    slotBtn.targetGraphic = slotBg;
                    MakeLabel(slotGO.transform, "FactionName", factionData.DisplayName,
                        new Vector2(0, 0.5f), new Vector2(1, 1), new Vector2(8, 0), new Vector2(-8, 0), Color.white, 14,
                        UIStyleType.HeaderText);

                    MakeLabel(slotGO.transform, "RelationText", "Neutral",
                        new Vector2(0, 0), new Vector2(1, 0.5f), new Vector2(8, 0), new Vector2(-8, 0), WarningColor, 11,
                        UIStyleType.WarningText);

                    int idx = i;
                    slotBtn.onClick.AddListener(() => { });

                    factionSlotProp.arraySize++;
                    var element = factionSlotProp.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("Data").objectReferenceValue = factionData;
                    element.FindPropertyRelative("Button").objectReferenceValue = slotBtn;
                    element.FindPropertyRelative("NameText").objectReferenceValue = slotGO.transform.Find("FactionName")?.GetComponent<TMP_Text>();
                    element.FindPropertyRelative("RelationText").objectReferenceValue = slotGO.transform.Find("RelationText")?.GetComponent<TMP_Text>();
                }
            }

            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(tradeUI);
            return 1;
        }

        #endregion

        #region QuestLog

        static int SetupQuestLogPanel(Canvas canvas)
        {
            DestroyExisting(canvas, "QuestLogPanel");
            GameObject panel = CreatePanel(canvas, "QuestLogPanel", false);
            var questUI = panel.GetComponent<QuestLogUI>() ?? panel.AddComponent<QuestLogUI>();

            Image panelBg = panel.GetComponent<Image>();
            if (panelBg != null) panelBg.color = DarkPanel;

            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.15f, 0.1f);
            prt.anchorMax = new Vector2(0.85f, 0.9f);
            prt.offsetMin = Vector2.zero;
            prt.offsetMax = Vector2.zero;

            GameObject tabBar = MakeChild(panel.transform, "TabBar",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -50), new Vector2(-10, -10), ActionBar);

            var tabLabel = MakeLabel(tabBar.transform, "TabLabel", "Active Quests",
                new Vector2(0.1f, 0), new Vector2(0.9f, 1), new Vector2(10, 0), new Vector2(-10, 0), Color.white, 18,
                UIStyleType.HeaderText);

            var prevBtn = MakeButton(tabBar.transform, "PrevBtn", "<",
                new Vector2(0, 0), new Vector2(0.1f, 1), new Vector2(0, 0), new Vector2(0, 0),
                new Color(0.18f, 0.18f, 0.16f, 0.95f), UIStyleType.TabButton);

            var nextBtn = MakeButton(tabBar.transform, "NextBtn", ">",
                new Vector2(0.9f, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 0),
                new Color(0.18f, 0.18f, 0.16f, 0.95f), UIStyleType.TabButton);

            GameObject questListContainer = new("QuestListContainer");
            questListContainer.transform.SetParent(panel.transform, false);
            RectTransform qlrt = questListContainer.AddComponent<RectTransform>();
            qlrt.anchorMin = new Vector2(0, 0);
            qlrt.anchorMax = new Vector2(0.45f, 1);
            qlrt.offsetMin = new Vector2(10, 10);
            qlrt.offsetMax = new Vector2(-5, -60);

            VerticalLayoutGroup vlg = questListContainer.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            GameObject detailPanel = MakeChild(panel.transform, "DetailPanel",
                new Vector2(0.45f, 0), new Vector2(1, 1), new Vector2(5, 10), new Vector2(-10, -60),
                new Color(0.08f, 0.08f, 0.06f, 0.6f));

            var detailName = MakeLabel(detailPanel.transform, "DetailName", "Quest Name",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -35), new Vector2(-10, -5), Color.white, 20,
                UIStyleType.HeaderText);

            var detailDesc = MakeLabel(detailPanel.transform, "DetailDesc", "Description",
                new Vector2(0, 0.6f), new Vector2(1, 0.85f), new Vector2(10, 0), new Vector2(-10, 0), BodyTextColor, 14,
                UIStyleType.BodyText);

            var detailObjectives = MakeLabel(detailPanel.transform, "DetailObjectives", "Objectives",
                new Vector2(0, 0.3f), new Vector2(1, 0.6f), new Vector2(10, 0), new Vector2(-10, 0), WarningColor, 14,
                UIStyleType.WarningText);

            var detailRewards = MakeLabel(detailPanel.transform, "DetailRewards", "Rewards",
                new Vector2(0, 0.15f), new Vector2(1, 0.3f), new Vector2(10, 0), new Vector2(-10, 0), BonusColor, 14,
                UIStyleType.BodyText);

            var acceptBtn = MakeButton(detailPanel.transform, "AcceptBtn", "ACCEPT",
                new Vector2(0.1f, 0), new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(0, 45),
                new Color(0.13f, 0.17f, 0.10f, 0.95f));

            var turnInBtn = MakeButton(detailPanel.transform, "TurnInBtn", "TURN IN",
                new Vector2(0.5f, 0), new Vector2(0.9f, 0), new Vector2(0, 10), new Vector2(0, 45),
                new Color(0.13f, 0.25f, 0.10f, 0.95f));

            var closeBtn = MakeButton(panel.transform, "CloseBtn", "CLOSE",
                new Vector2(0.35f, 0), new Vector2(0.65f, 0), new Vector2(0, 10), new Vector2(0, 45),
                new Color(0.18f, 0.18f, 0.16f, 0.95f), UIStyleType.DangerButton);

            GameObject questItemPrefab = CreateQuestItemPrefab();

            SerializedObject so = new SerializedObject(questUI);
            SetSerializedField(so, "_questListContainer", questListContainer.transform);
            SetSerializedField(so, "_questItemPrefab", questItemPrefab);
            SetSerializedField(so, "_detailPanel", detailPanel);
            SetSerializedField(so, "_detailName", detailName);
            SetSerializedField(so, "_detailDesc", detailDesc);
            SetSerializedField(so, "_detailObjectives", detailObjectives);
            SetSerializedField(so, "_detailRewards", detailRewards);
            SetSerializedField(so, "_acceptBtn", acceptBtn);
            SetSerializedField(so, "_turnInBtn", turnInBtn);
            SetSerializedField(so, "_closeBtn", closeBtn);
            SetSerializedField(so, "_tabLabel", tabLabel);
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(questUI);
            return 1;
        }

        #endregion

        #region Save Menu ScrollList

        [MenuItem("HollowGround/Setup Save Menu")]
        public static void SetupSaveMenu()
        {
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[SceneSetup] No Canvas found in scene!");
                return;
            }

            Transform panelT = null;
            for (int i = 0; i < canvas.transform.childCount; i++)
            {
                if (canvas.transform.GetChild(i).name.Trim() == "SaveMenuPanel")
                {
                    panelT = canvas.transform.GetChild(i);
                    break;
                }
            }

            if (panelT == null)
            {
                Debug.LogError("[SceneSetup] SaveMenuPanel not found under Canvas! Run Setup UI Panels first.");
                return;
            }

            GameObject panel = panelT.gameObject;

            for (int i = panelT.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(panelT.GetChild(i).gameObject);

            var rect = panel.GetComponent<RectTransform>();
            if (rect == null) rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(0f, 60f);
            rect.offsetMax = Vector2.zero;

            var panelImg = panel.GetComponent<Image>();
            if (panelImg == null) panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.05f, 0.05f, 0.04f, 0.92f);

            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();

            var vlg = panel.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10f;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var headerGO = new GameObject("Header");
            headerGO.transform.SetParent(panelT, false);
            var headerRt = headerGO.AddComponent<RectTransform>();
            var headerLe = headerGO.AddComponent<LayoutElement>();
            headerLe.preferredHeight = 50;
            var headerTmp = headerGO.AddComponent<TextMeshProUGUI>();
            headerTmp.text = "SAVE / LOAD";
            headerTmp.fontSize = 30;
            headerTmp.alignment = TextAlignmentOptions.Center;
            headerTmp.color = new Color(0.83f, 0.52f, 0.04f, 1f);

            var scrollListObj = new GameObject("ScrollList");
            scrollListObj.transform.SetParent(panelT, false);
            var scrollRt = scrollListObj.AddComponent<RectTransform>();
            var scrollLe = scrollListObj.AddComponent<LayoutElement>();
            scrollLe.preferredHeight = 600f;
            scrollLe.minHeight = 200f;
            scrollLe.flexibleHeight = 1;

            var scrollRect = scrollListObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 30f;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            var viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollListObj.transform, false);
            var vpRt = viewportObj.AddComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.sizeDelta = Vector2.zero;
            viewportObj.AddComponent<RectMask2D>();
            scrollRect.viewport = vpRt;

            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            var ctRt = contentObj.AddComponent<RectTransform>();
            ctRt.anchorMin = new Vector2(0f, 0f);
            ctRt.anchorMax = new Vector2(1f, 1f);
            ctRt.pivot = new Vector2(0.5f, 1f);
            ctRt.sizeDelta = new Vector2(-18f, 0f);

            var ctVlg = contentObj.AddComponent<VerticalLayoutGroup>();
            ctVlg.padding = new RectOffset(8, 8, 6, 6);
            ctVlg.spacing = 4f;
            ctVlg.childControlWidth = true;
            ctVlg.childControlHeight = true;
            ctVlg.childForceExpandWidth = true;
            ctVlg.childForceExpandHeight = false;

            contentObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = ctRt;

            var scrollbarObj = new GameObject("Scrollbar");
            scrollbarObj.transform.SetParent(scrollListObj.transform, false);
            var sbRt = scrollbarObj.AddComponent<RectTransform>();
            sbRt.anchorMin = new Vector2(1f, 0f);
            sbRt.anchorMax = new Vector2(1f, 1f);
            sbRt.pivot = new Vector2(1f, 1f);
            sbRt.sizeDelta = new Vector2(14f, 0f);

            var sbBg = scrollbarObj.AddComponent<Image>();
            sbBg.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);

            var scrollbar = scrollbarObj.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.size = 0.3f;

            var handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(scrollbarObj.transform, false);
            var hRt = handleObj.AddComponent<RectTransform>();
            hRt.anchorMin = Vector2.zero;
            hRt.anchorMax = Vector2.one;
            hRt.sizeDelta = Vector2.zero;

            var handleImg = handleObj.AddComponent<Image>();
            handleImg.color = new Color(0.7f, 0.7f, 0.7f, 0.9f);
            scrollbar.targetGraphic = handleImg;
            scrollbar.handleRect = hRt;
            scrollRect.verticalScrollbar = scrollbar;

            var statusGO = new GameObject("StatusText");
            statusGO.transform.SetParent(panelT, false);
            var statusLe = statusGO.AddComponent<LayoutElement>();
            statusLe.preferredHeight = 30;
            var statusTmp = statusGO.AddComponent<TextMeshProUGUI>();
            statusTmp.text = "Select a save file";
            statusTmp.fontSize = 17;
            statusTmp.alignment = TextAlignmentOptions.Center;
            statusTmp.color = new Color(0.6f, 0.6f, 0.55f, 1f);

            var confirmGO = new GameObject("ConfirmRow");
            confirmGO.transform.SetParent(panelT, false);
            var confirmLe = confirmGO.AddComponent<LayoutElement>();
            confirmLe.preferredHeight = 48;
            confirmGO.SetActive(false);

            var btnRowGO = new GameObject("BtnRow");
            btnRowGO.transform.SetParent(panelT, false);
            var btnRowLe = btnRowGO.AddComponent<LayoutElement>();
            btnRowLe.preferredHeight = 52;
            var btnRowHlg = btnRowGO.AddComponent<HorizontalLayoutGroup>();
            btnRowHlg.childAlignment = TextAnchor.MiddleCenter;
            btnRowHlg.childForceExpandWidth = false;
            btnRowHlg.spacing = 12f;

            CreateEditorButton(btnRowGO.transform, "NewSaveBtn", "NEW SAVE", 140);
            CreateEditorButton(btnRowGO.transform, "LoadBtn", "LOAD", 110);
            CreateEditorButton(btnRowGO.transform, "DeleteBtn", "DELETE", 110);
            CreateEditorButton(btnRowGO.transform, "BackBtn", "BACK", 110);

            var saveMenuUI = panel.GetComponent<SaveMenuUI>() ?? panel.AddComponent<SaveMenuUI>();
            SerializedObject so = new SerializedObject(saveMenuUI);
            var contentProp = so.FindProperty("_contentContainer");
            var statusProp = so.FindProperty("_statusText");
            var loadProp = so.FindProperty("_loadBtn");
            var deleteProp = so.FindProperty("_deleteBtn");
            var confirmProp = so.FindProperty("_confirmRow");

            if (contentProp != null) contentProp.objectReferenceValue = ctRt;
            if (statusProp != null) statusProp.objectReferenceValue = statusTmp;
            if (loadProp != null) loadProp.objectReferenceValue = btnRowGO.transform.Find("LoadBtn").GetComponent<Button>();
            if (deleteProp != null) deleteProp.objectReferenceValue = btnRowGO.transform.Find("DeleteBtn").GetComponent<Button>();
            if (confirmProp != null) confirmProp.objectReferenceValue = confirmGO;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(saveMenuUI);

            panel.SetActive(false);
            EditorSceneManager.MarkSceneDirty(panel.scene);
            Debug.Log("[SceneSetup] SaveMenuPanel setup complete! All bindings applied.");
        }

        static void CreateEditorButton(Transform parent, string name, string label, int width)
        {
            var btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);
            var btnLe = btnGO.AddComponent<LayoutElement>();
            btnLe.minWidth = width;
            btnLe.preferredWidth = width;
            btnLe.minHeight = 46;

            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.15f, 0.15f, 0.13f, 1f);

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            var lblGO = new GameObject("Label");
            lblGO.transform.SetParent(btnGO.transform, false);
            var lblRt = lblGO.AddComponent<RectTransform>();
            lblRt.anchorMin = Vector2.zero;
            lblRt.anchorMax = Vector2.one;
            lblRt.sizeDelta = Vector2.zero;
            var lblTmp = lblGO.AddComponent<TextMeshProUGUI>();
            lblTmp.text = label;
            lblTmp.fontSize = 16;
            lblTmp.alignment = TextAlignmentOptions.Center;
            lblTmp.color = Color.white;
        }

        #endregion
    }
}
