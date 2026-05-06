using HollowGround.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HollowGround.Editor
{
    public static class PanelBuilderUtil
    {
        public static readonly Color PanelBg = new(0.12f, 0.12f, 0.15f, 0.95f);
        public static readonly Color SectionBg = new(0.16f, 0.16f, 0.19f, 0.9f);
        public static readonly Color SeparatorColor = new(0.4f, 0.4f, 0.4f, 0.4f);
        public static readonly Color BarBgColor = new(0.1f, 0.1f, 0.1f, 0.8f);
        public static readonly Color LabelColor = new(0.6f, 0.6f, 0.6f);
        public static readonly Color TextColor = Color.white;
        public static readonly Color GoldColor = Color.yellow;
        public static readonly Color OkColor = new(0.2f, 0.8f, 0.3f);
        public static readonly Color WarnColor = new(1f, 0.6f, 0.2f);
        public static readonly Color DangerColor = new(0.7f, 0.2f, 0.2f);
        public static readonly Color AccentColor = new(0.2f, 0.4f, 0.7f);

        static TMP_FontAsset _themeFont;
        public static TMP_FontAsset ThemeFont
        {
            get
            {
                if (_themeFont == null)
                {
                    var theme = AssetDatabase.LoadAssetAtPath<UIThemeSO>(
                        "Assets/_Project/ScriptableObjects/UITheme.asset");
                    _themeFont = theme != null ? theme.defaultFont : null;
                }
                return _themeFont;
            }
        }

        static void ApplyFont(TMP_Text tmp)
        {
            if (tmp != null && ThemeFont != null)
                tmp.font = ThemeFont;
        }

        public static GameObject CreateRoot(string name, Transform parent, float width = 300)
        {
            var root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(width, 0);

            var cg = root.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = true;

            root.AddComponent<Image>().color = PanelBg;
            var vlg = root.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.spacing = 4;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = root.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return root;
        }

        public static void SetupFullPanel(GameObject root)
        {
            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = new Vector2(0, -60);

            root.GetComponent<Image>().color = PanelBg;
            var vlg = root.GetComponent<VerticalLayoutGroup>();
            if (vlg == null)
                vlg = root.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.spacing = 4;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
        }

        public static GameObject CreateRow(string name, Transform parent, int spacing = 4, RectOffset padding = null)
        {
            var row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = spacing;
            hlg.padding = padding ?? new RectOffset(0, 0, 0, 0);
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            row.AddComponent<LayoutElement>();
            return row;
        }

        public static GameObject CreateSection(string name, Transform parent, RectOffset padding = null)
        {
            var box = new GameObject(name, typeof(RectTransform));
            box.transform.SetParent(parent, false);
            box.AddComponent<Image>().color = SectionBg;
            var vlg = box.AddComponent<VerticalLayoutGroup>();
            vlg.padding = padding ?? new RectOffset(8, 8, 4, 4);
            vlg.spacing = 2;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            box.AddComponent<LayoutElement>();
            return box;
        }

        public static void AddSeparator(Transform parent)
        {
            var sep = new GameObject("Separator", typeof(RectTransform));
            sep.transform.SetParent(parent, false);
            sep.AddComponent<LayoutElement>().preferredHeight = 1;
            sep.AddComponent<Image>().color = SeparatorColor;
        }

        public static TMP_Text CreateLabel(Transform parent, string text, float width = 60)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 12;
            tmp.color = LabelColor;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            ApplyFont(tmp);
            return tmp;
        }

        public static TMP_Text CreateTMP(string name, Transform parent, string text, int size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            go.AddComponent<LayoutElement>();
            ApplyFont(tmp);
            return tmp;
        }

        public static TMP_Text CreateHeader(string name, Transform parent, string text, int size = 20)
        {
            return CreateTMP(name, parent, text, size, GoldColor);
        }

        public static TMP_Text CreateLabelValueRow(Transform parent, string label, out TMP_Text valueText)
        {
            var row = CreateRow(label + "Row", parent);
            CreateLabel(row.transform, label);
            valueText = CreateTMP("Value", row.transform, "-", 13, TextColor);
            valueText.GetComponent<LayoutElement>().flexibleWidth = 1;
            return valueText;
        }

        public static Image CreateBar(Transform parent, float height = 6)
        {
            var bg = new GameObject("BarBg", typeof(RectTransform));
            bg.transform.SetParent(parent, false);
            var le = bg.AddComponent<LayoutElement>();
            le.minWidth = 60;
            le.preferredHeight = height;
            le.flexibleWidth = 1;
            bg.AddComponent<Image>().color = BarBgColor;

            var fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(bg.transform, false);
            var frt = fill.GetComponent<RectTransform>();
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.offsetMin = Vector2.zero;
            frt.offsetMax = Vector2.zero;
            var img = fill.AddComponent<Image>();
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillAmount = 0.75f;
            img.color = OkColor;
            return img;
        }

        public static Button CreateButton(string name, Transform parent, string text, Color bgColor, float height = 28)
        {
            var btnObj = new GameObject(name, typeof(RectTransform));
            btnObj.transform.SetParent(parent, false);
            var bgImg = btnObj.AddComponent<Image>();
            bgImg.color = bgColor;
            bgImg.raycastTarget = true;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bgImg;

            var label = new GameObject("Label", typeof(RectTransform));
            label.transform.SetParent(btnObj.transform, false);
            var tmp = label.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 12;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            ApplyFont(tmp);
            var lrt = label.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;

            var le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.flexibleWidth = 1;

            return btn;
        }

        public static ScrollRect CreateScrollView(string name, Transform parent, out Transform content)
        {
            var scroll = new GameObject(name, typeof(RectTransform));
            scroll.transform.SetParent(parent, false);
            var sr = scroll.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.scrollSensitivity = 20f;

            var viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.transform.SetParent(scroll.transform, false);
            StretchFull(viewport.GetComponent<RectTransform>());
            var vpImg = viewport.AddComponent<Image>();
            vpImg.color = new Color(0, 0, 0, 0.1f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            var contentObj = new GameObject("Content", typeof(RectTransform));
            contentObj.transform.SetParent(viewport.transform, false);
            var crt = contentObj.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 1);
            crt.anchorMax = Vector2.one;
            crt.pivot = new Vector2(0.5f, 1);
            crt.offsetMin = Vector2.zero;
            crt.offsetMax = Vector2.zero;

            var vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4;
            vlg.padding = new RectOffset(4, 4, 4, 4);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            contentObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = contentObj.GetComponent<RectTransform>();
            sr.viewport = viewport.GetComponent<RectTransform>();

            var scrollLE = scroll.AddComponent<LayoutElement>();
            scrollLE.flexibleHeight = 1;
            scrollLE.minHeight = 100;

            content = contentObj.transform;
            return sr;
        }

        public static Slider CreateSlider(string name, Transform parent, Color fillColor, float height = 8)
        {
            var container = new GameObject(name, typeof(RectTransform));
            container.transform.SetParent(parent, false);
            var le = container.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            le.flexibleWidth = 1;

            var bgImg = container.AddComponent<Image>();
            bgImg.color = BarBgColor;

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(container.transform, false);
            var fart = fillArea.GetComponent<RectTransform>();
            fart.anchorMin = Vector2.zero;
            fart.anchorMax = Vector2.one;
            fart.offsetMin = Vector2.zero;
            fart.offsetMax = Vector2.zero;

            var fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(fillArea.transform, false);
            var frt = fill.GetComponent<RectTransform>();
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.offsetMin = Vector2.zero;
            frt.offsetMax = Vector2.zero;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = fillColor;

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(container.transform, false);
            var hart = handleArea.GetComponent<RectTransform>();
            hart.anchorMin = Vector2.zero;
            hart.anchorMax = Vector2.one;
            hart.offsetMin = Vector2.zero;
            hart.offsetMax = Vector2.zero;

            var handle = new GameObject("Handle", typeof(RectTransform));
            handle.transform.SetParent(handleArea.transform, false);
            var handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.clear;

            var slider = container.AddComponent<Slider>();
            slider.interactable = false;
            slider.targetGraphic = bgImg;
            slider.fillRect = frt;
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.value = 0f;

            return slider;
        }

        public static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static void WireField(SerializedObject so, string field, Object obj)
        {
            so.FindProperty(field).objectReferenceValue = obj;
        }
    }
}
