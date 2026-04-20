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

            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log($"[SceneSetup] {created} UI panel created/verified.");

            WireUIManager(canvas);
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
            if (t != null)
                so.FindProperty(field).objectReferenceValue = t.gameObject;
        }

        static Canvas FindOrCreateCanvas()
        {
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas != null) return canvas;

            GameObject go = new(CANVAS_NAME);
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>();
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
            Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax, Color color, int fontSize)
        {
            GameObject go = MakeChild(parent, objName, aMin, aMax, oMin, oMax);
            TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.color = color;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Left;
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
            Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax, Color bgColor)
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
            so.FindProperty(field).objectReferenceValue = value;
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
            GameObject panel = CreatePanel(canvas, "BuildingInfoPanel", false);
            var infoUI = panel.GetComponent<BuildingInfoUI>() ?? panel.AddComponent<BuildingInfoUI>();

            Image panelBg = panel.GetComponent<Image>();
            if (panelBg != null) panelBg.color = DarkPanel;

            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.5f, 0.5f);
            prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(350, 450);
            prt.anchoredPosition = new Vector2(300, 0);

            MakeChild(panel.transform, "Header",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -50), new Vector2(-10, -10));
            var nameText = MakeLabel(panel.transform, "NameText", "Building Name",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -45), new Vector2(-20, -15), Color.white, 22);

            var levelText = MakeLabel(panel.transform, "LevelText", "Level 1",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -75), new Vector2(-20, -50), LabelTextColor, 16);

            var stateText = MakeLabel(panel.transform, "StateText", "Constructing",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -100), new Vector2(-20, -80), WarningColor, 14);

            var progressSlider = MakeSlider(panel.transform, "ProgressSlider",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -140), new Vector2(-20, -120));

            var progressLabel = MakeLabel(panel.transform, "ProgressLabel", "0%",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -160), new Vector2(-20, -140), Color.white, 12);

            GameObject productionGroup = MakeChild(panel.transform, "ProductionGroup",
                new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(10, -20), new Vector2(-10, 40));

            var productionText = MakeLabel(productionGroup.transform, "ProductionText", "10 Food / 5dk",
                new Vector2(0, 0.5f), new Vector2(1, 1), new Vector2(10, 5), new Vector2(-10, -5), BodyTextColor, 14);

            var productionSlider = MakeSlider(productionGroup.transform, "ProductionSlider",
                new Vector2(0, 0), new Vector2(1, 0.4f), new Vector2(0, 0), new Vector2(0, 0));

            var upgradeBtn = MakeButton(panel.transform, "UpgradeButton", "UPGRADE",
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(20, 60), new Vector2(-20, 95),
                new Color(0.13f, 0.17f, 0.10f, 0.95f));

            var upgradeCostText = MakeLabel(panel.transform, "UpgradeCostText", "Cost: -",
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(20, 100), new Vector2(-20, 80), LabelTextColor, 12);

            var demolishBtn = MakeButton(panel.transform, "DemolishButton", "DEMOLISH",
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(20, 15), new Vector2(-20, 50),
                new Color(0.35f, 0.07f, 0.07f, 0.95f));

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
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(infoUI);
            return 1;
        }

        #endregion

        #region Toast

        static int SetupToastPanel(Canvas canvas)
        {
            GameObject panel = CreatePanel(canvas, "ToastPanel", false);
            var toastUI = panel.GetComponent<ToastUI>() ?? panel.AddComponent<ToastUI>();

            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(1, 1);
            prt.anchorMax = new Vector2(1, 1);
            prt.pivot = new Vector2(1, 1);
            prt.sizeDelta = new Vector2(350, 300);
            prt.anchoredPosition = new Vector2(-20, -20);

            Image panelBg = panel.GetComponent<Image>();
            if (panelBg != null) panelBg.color = Color.clear;

            Transform container = panel.transform.Find("ToastContainer");
            if (container == null)
            {
                GameObject containerGO = new("ToastContainer");
                containerGO.transform.SetParent(panel.transform, false);
                RectTransform crt = containerGO.AddComponent<RectTransform>();
                crt.anchorMin = new Vector2(0.5f, 1);
                crt.anchorMax = new Vector2(0.5f, 1);
                crt.pivot = new Vector2(0.5f, 1);
                crt.sizeDelta = new Vector2(320, 300);
                crt.anchoredPosition = Vector2.zero;

                VerticalLayoutGroup vlg = containerGO.AddComponent<VerticalLayoutGroup>();
                vlg.childAlignment = TextAnchor.UpperCenter;
                vlg.childControlWidth = true;
                vlg.childControlHeight = false;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;
                vlg.spacing = 8;
                vlg.padding = new RectOffset(5, 5, 5, 5);

                ContentSizeFitter csf = containerGO.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                container = containerGO.transform;
            }

            GameObject toastPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/ToastItem.prefab");

            SerializedObject so = new SerializedObject(toastUI);
            SetSerializedField(so, "_toastContainer", container);
            if (toastPrefab != null)
                SetSerializedField(so, "_toastPrefab", toastPrefab);
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(toastUI);
            return 1;
        }

        #endregion

        #region TechTree

        static int SetupTechTreePanel(Canvas canvas)
        {
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
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -35), new Vector2(-10, -5), Color.white, 20);

            var detailDesc = MakeLabel(detailPanel.transform, "DetailDesc", "Description",
                new Vector2(0, 0.5f), new Vector2(1, 0.7f), new Vector2(10, 10), new Vector2(-10, -10), BodyTextColor, 14);

            var detailCost = MakeLabel(detailPanel.transform, "DetailCost", "Cost: -",
                new Vector2(0, 0.3f), new Vector2(1, 0.5f), new Vector2(10, 0), new Vector2(-10, 0), WarningColor, 13);

            var detailBonuses = MakeLabel(detailPanel.transform, "DetailBonuses", "Bonuses: -",
                new Vector2(0, 0.15f), new Vector2(1, 0.35f), new Vector2(10, 0), new Vector2(-10, 0), BonusColor, 13);

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
                        new Vector2(0, 0.6f), new Vector2(1, 1), new Vector2(5, 0), new Vector2(-5, 0), Color.white, 14);
                    var cardDesc = MakeLabel(cardGO.transform, "DescText", "",
                        new Vector2(0, 0.3f), new Vector2(1, 0.6f), new Vector2(5, 0), new Vector2(-5, 0), BodyTextColor, 11);
                    var cardCost = MakeLabel(cardGO.transform, "CostText", "",
                        new Vector2(0, 0.1f), new Vector2(0.6f, 0.3f), new Vector2(5, 0), new Vector2(-5, 0), WarningColor, 11);
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
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -35), new Vector2(-10, -5), Color.white, 20);

            var factionRelationText = MakeLabel(tradePanel.transform, "FactionRelation", "Relation: Neutral",
                new Vector2(0, 0.85f), new Vector2(1, 0.95f), new Vector2(10, 0), new Vector2(-10, 0), WarningColor, 14);

            var factionDescText = MakeLabel(tradePanel.transform, "FactionDesc", "Description",
                new Vector2(0, 0.75f), new Vector2(1, 0.85f), new Vector2(10, 0), new Vector2(-10, 0), BodyTextColor, 13);

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
                new Color(0.18f, 0.18f, 0.16f, 0.95f));

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
                        new Vector2(0, 0.5f), new Vector2(1, 1), new Vector2(8, 0), new Vector2(-8, 0), Color.white, 14);
                    MakeLabel(slotGO.transform, "RelationText", "Neutral",
                        new Vector2(0, 0), new Vector2(1, 0.5f), new Vector2(8, 0), new Vector2(-8, 0), WarningColor, 11);

                    int idx = i;
                    slotBtn.onClick.AddListener(() => { tradeUI.SelectFaction(idx); });

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
                new Vector2(0.1f, 0), new Vector2(0.9f, 1), new Vector2(10, 0), new Vector2(-10, 0), Color.white, 18);

            var prevBtn = MakeButton(tabBar.transform, "PrevBtn", "<",
                new Vector2(0, 0), new Vector2(0.1f, 1), new Vector2(0, 0), new Vector2(0, 0),
                new Color(0.18f, 0.18f, 0.16f, 0.95f));

            var nextBtn = MakeButton(tabBar.transform, "NextBtn", ">",
                new Vector2(0.9f, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 0),
                new Color(0.18f, 0.18f, 0.16f, 0.95f));

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
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -35), new Vector2(-10, -5), Color.white, 20);

            var detailDesc = MakeLabel(detailPanel.transform, "DetailDesc", "Description",
                new Vector2(0, 0.6f), new Vector2(1, 0.85f), new Vector2(10, 0), new Vector2(-10, 0), BodyTextColor, 14);

            var detailObjectives = MakeLabel(detailPanel.transform, "DetailObjectives", "Objectives",
                new Vector2(0, 0.3f), new Vector2(1, 0.6f), new Vector2(10, 0), new Vector2(-10, 0), WarningColor, 14);

            var detailRewards = MakeLabel(detailPanel.transform, "DetailRewards", "Rewards",
                new Vector2(0, 0.15f), new Vector2(1, 0.3f), new Vector2(10, 0), new Vector2(-10, 0), BonusColor, 14);

            var acceptBtn = MakeButton(detailPanel.transform, "AcceptBtn", "ACCEPT",
                new Vector2(0.1f, 0), new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(0, 45),
                new Color(0.13f, 0.17f, 0.10f, 0.95f));

            var turnInBtn = MakeButton(detailPanel.transform, "TurnInBtn", "TURN IN",
                new Vector2(0.5f, 0), new Vector2(0.9f, 0), new Vector2(0, 10), new Vector2(0, 45),
                new Color(0.13f, 0.25f, 0.10f, 0.95f));

            var closeBtn = MakeButton(panel.transform, "CloseBtn", "CLOSE",
                new Vector2(0.35f, 0), new Vector2(0.65f, 0), new Vector2(0, 10), new Vector2(0, 45),
                new Color(0.18f, 0.18f, 0.16f, 0.95f));

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
    }
}
