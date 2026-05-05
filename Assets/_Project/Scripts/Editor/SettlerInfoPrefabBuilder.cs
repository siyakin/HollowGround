using HollowGround.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HollowGround.Editor
{
    public static class SettlerInfoPrefabBuilder
    {
        [MenuItem("HollowGround/UI/Build SettlerInfo Panel")]
        public static void Build()
        {
            var canvas = GameObject.Find("GameCanvas");
            if (canvas == null)
            {
                Debug.LogError("GameCanvas not found!");
                return;
            }

            var existing = canvas.transform.Find("SettlerInfoPanel");
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Replace?",
                    "SettlerInfoPanel already exists. Replace?", "Yes", "No"))
                    return;
                Object.DestroyImmediate(existing.gameObject);
            }

            var panel = CreatePanel(canvas.transform);
            Selection.activeGameObject = panel;
            Debug.Log("SettlerInfoPanel created. Select it and adjust visually in Inspector.");
        }

        private static GameObject CreatePanel(Transform parent)
        {
            var root = new GameObject("SettlerInfoPanel", typeof(RectTransform));
            root.transform.SetParent(parent, false);

            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(300, 0);

            var canvasGroup = root.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = true;

            var rootImage = root.AddComponent<Image>();
            rootImage.color = new Color(0.12f, 0.12f, 0.15f, 0.95f);

            var rootVLG = root.AddComponent<VerticalLayoutGroup>();
            rootVLG.padding = new RectOffset(12, 12, 12, 12);
            rootVLG.spacing = 8;
            rootVLG.childControlWidth = true;
            rootVLG.childControlHeight = false;
            rootVLG.childForceExpandWidth = true;
            rootVLG.childForceExpandHeight = false;

            var csf = root.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var info = root.AddComponent<SettlerInfoUI>();
            root.SetActive(false);

            // --- HEADER ---
            var header = CreateRow("Header", root.transform);

            var dot = new GameObject("RoleDot", typeof(RectTransform));
            dot.transform.SetParent(header.transform, false);
            var dotLE = dot.AddComponent<LayoutElement>();
            dotLE.preferredWidth = 10;
            dotLE.preferredHeight = 10;
            var dotImg = dot.AddComponent<Image>();
            dotImg.color = Color.green;

            var title = CreateTMP("RoleTitle", header.transform, "FARMER", 18, Color.yellow);
            title.alignment = TextAlignmentOptions.MidlineLeft;

            AddSeparator(root.transform);

            // --- IDENTITY ---
            var identityBox = CreateSectionBox("Identity", root.transform);

            var row1 = CreateRow("Row1", identityBox.transform);
            var col1 = CreateCol("Col1", row1.transform);
            CreateLabel(col1.transform, "Role:");
            var roleValue = CreateTMP("RoleValue", col1.transform, "-", 13, Color.white);

            var col2 = CreateCol("Col2", row1.transform);
            CreateLabel(col2.transform, "Work:");
            var buildingValue = CreateTMP("BuildingValue", col2.transform, "-", 13, Color.white);

            var statusRow = CreateRow("StatusRow", identityBox.transform);
            CreateLabel(statusRow.transform, "Status:");
            var taskValue = CreateTMP("TaskValue", statusRow.transform, "-", 13, Color.white);
            taskValue.GetComponent<LayoutElement>().flexibleWidth = 1;

            var workersRow = CreateRow("WorkersRow", identityBox.transform);
            CreateLabel(workersRow.transform, "Workers:");
            var workersValue = CreateTMP("WorkersValue", workersRow.transform, "-", 13, Color.white);
            workersValue.GetComponent<LayoutElement>().flexibleWidth = 1;

            AddSeparator(root.transform);

            // --- MORALE ---
            var moraleBox = CreateSectionBox("Morale", root.transform);
            CreateLabel(moraleBox.transform, "Morale", 12, new Color(0.6f, 0.6f, 0.6f));

            var moraleBarRow = CreateRow("MoraleBarRow", moraleBox.transform);
            var moraleBarBg = CreateBarBg("MoraleBar", moraleBarRow.transform, out var moraleFill);
            var moraleStatus = CreateTMP("MoraleStatus", moraleBarRow.transform, "Normal", 11, Color.white);
            moraleStatus.GetComponent<LayoutElement>().preferredWidth = 50;

            AddSeparator(root.transform);

            // --- HEALTH ---
            var healthBox = CreateSectionBox("Health", root.transform);
            CreateLabel(healthBox.transform, "Health", 12, new Color(0.6f, 0.6f, 0.6f));

            var healthBarRow = CreateRow("HealthBarRow", healthBox.transform);
            var healthBarBg = CreateBarBg("HealthBar", healthBarRow.transform, out var healthFill);
            var healthStatus = CreateTMP("HealthStatus", healthBarRow.transform, "Healthy", 11, Color.white);
            healthStatus.GetComponent<LayoutElement>().preferredWidth = 50;

            var hospitalText = CreateTMP("HospitalText", healthBox.transform, "Hospital recovery required", 10, new Color(1f, 0.6f, 0.2f));
            hospitalText.gameObject.SetActive(false);

            AddSeparator(root.transform);

            // --- PRODUCTION ---
            var prodBox = CreateSectionBox("Production", root.transform);
            CreateLabel(prodBox.transform, "Production", 12, new Color(0.6f, 0.6f, 0.6f));

            var effRow = CreateRow("EffRow", prodBox.transform);
            CreateLabel(effRow.transform, "Efficiency:");
            var effValue = CreateTMP("EffValue", effRow.transform, "100%", 12, Color.green);

            var specRow = CreateRow("SpecRow", prodBox.transform);
            CreateLabel(specRow.transform, "Specialist:");
            var specValue = CreateTMP("SpecValue", specRow.transform, "None", 12, Color.white);

            var iconRow = CreateRow("BonusSlots", prodBox.transform);
            for (int i = 0; i < 3; i++)
            {
                var slot = new GameObject($"Slot_{i}", typeof(RectTransform));
                slot.transform.SetParent(iconRow.transform, false);
                var slotLE = slot.AddComponent<LayoutElement>();
                slotLE.preferredWidth = 16;
                slotLE.preferredHeight = 16;
                slot.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            }

            AddSeparator(root.transform);

            // --- ACTIONS ---
            var actionsRow = CreateRow("Actions", root.transform);

            var reassignBtn = CreateTMPButton("ReassignBtn", actionsRow.transform, "Reassign", new Color(0.2f, 0.4f, 0.7f));
            reassignBtn.GetComponent<LayoutElement>().flexibleWidth = 1;

            var restBtn = CreateTMPButton("RestBtn", actionsRow.transform, "Rest", new Color(0.35f, 0.35f, 0.4f));
            restBtn.GetComponent<LayoutElement>().flexibleWidth = 1;

            var dismissBtn = CreateTMPButton("DismissBtn", actionsRow.transform, "Dismiss", new Color(0.7f, 0.2f, 0.2f));
            dismissBtn.GetComponent<LayoutElement>().flexibleWidth = 1;

            // --- WIRE SERIALIZED FIELDS ---
            var so = new SerializedObject(info);
            so.FindProperty("_roleDot").objectReferenceValue = dotImg;
            so.FindProperty("_roleTitleText").objectReferenceValue = title;
            so.FindProperty("_roleValueText").objectReferenceValue = roleValue;
            so.FindProperty("_buildingText").objectReferenceValue = buildingValue;
            so.FindProperty("_taskText").objectReferenceValue = taskValue;
            so.FindProperty("_workersText").objectReferenceValue = workersValue;
            so.FindProperty("_moraleFill").objectReferenceValue = moraleFill;
            so.FindProperty("_moraleStatusText").objectReferenceValue = moraleStatus;
            so.FindProperty("_healthFill").objectReferenceValue = healthFill;
            so.FindProperty("_healthStatusText").objectReferenceValue = healthStatus;
            so.FindProperty("_hospitalText").objectReferenceValue = hospitalText;
            so.FindProperty("_efficiencyText").objectReferenceValue = effValue;
            so.FindProperty("_specialistText").objectReferenceValue = specValue;
            so.FindProperty("_reassignBtn").objectReferenceValue = reassignBtn;
            so.FindProperty("_restBtn").objectReferenceValue = restBtn;
            so.FindProperty("_dismissBtn").objectReferenceValue = dismissBtn;
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

        private static GameObject CreateCol(string name, Transform parent)
        {
            var col = new GameObject(name, typeof(RectTransform));
            col.transform.SetParent(parent, false);
            var vlg = col.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 2;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            var le = col.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            return col;
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
            label.GetComponent<LayoutElement>().preferredWidth = 70;
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

        private static GameObject CreateBarBg(string name, Transform parent, out Image fillImage)
        {
            var bg = new GameObject(name, typeof(RectTransform));
            bg.transform.SetParent(parent, false);
            var bgLE = bg.AddComponent<LayoutElement>();
            bgLE.minWidth = 100;
            bgLE.preferredHeight = 8;
            bgLE.flexibleWidth = 1;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            var fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(bg.transform, false);
            var fillRT = fill.GetComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;
            fillImage = fill.AddComponent<Image>();
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillAmount = 0.75f;
            fillImage.color = Color.green;

            return bg;
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
