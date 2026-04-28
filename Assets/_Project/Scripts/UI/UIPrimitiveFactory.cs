using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public static class UIPrimitiveFactory
    {
        public static TMP_FontAsset ResolvedThemeFont => _themeFont ??= ResolveThemeFont();
        private static TMP_FontAsset _themeFont;
        private static UIThemeSO _cachedTheme;

        public static UIThemeSO LoadTheme()
        {
            if (_cachedTheme != null) return _cachedTheme;
#if UNITY_EDITOR
            _cachedTheme = UnityEditor.AssetDatabase.LoadAssetAtPath<UIThemeSO>(
                "Assets/_Project/ScriptableObjects/UITheme.asset");
#else
            var results = UnityEngine.Resources.LoadAll<UIThemeSO>("UITheme");
            _cachedTheme = results.Length > 0 ? results[0] : null;
#endif
            return _cachedTheme;
        }

        private static TMP_FontAsset ResolveThemeFont()
        {
            var theme = LoadTheme();
            return theme != null ? theme.defaultFont : null;
        }

        public static void ApplyThemeStyles(Transform root)
        {
            var theme = LoadTheme();
            if (theme == null) return;

            foreach (var tag in root.GetComponentsInChildren<UIThemeTag>(true))
            {
                switch (tag.styleType)
                {
                    case UIStyleType.ConfirmButton:
                        ApplyButtonTheme(tag.gameObject, theme.confirmButton, theme);
                        break;
                    case UIStyleType.DangerButton:
                        ApplyButtonTheme(tag.gameObject, theme.dangerButton, theme);
                        break;
                    case UIStyleType.ActionBarButton:
                        ApplyButtonTheme(tag.gameObject, theme.actionBarButton, theme);
                        break;
                    case UIStyleType.BuildingCardButton:
                        ApplyButtonTheme(tag.gameObject, theme.buildingCardButton, theme);
                        break;
                    case UIStyleType.TabButton:
                        ApplyButtonTheme(tag.gameObject, theme.tabButton, theme);
                        break;
                    case UIStyleType.HeaderText:
                        ApplyTextTheme(tag.gameObject, theme.headerStyle, theme.headerTextColor, theme);
                        break;
                    case UIStyleType.BodyText:
                        ApplyTextTheme(tag.gameObject, theme.bodyStyle, theme.bodyTextColor, theme);
                        break;
                    case UIStyleType.LabelText:
                        ApplyTextTheme(tag.gameObject, theme.labelStyle, theme.labelTextColor, theme);
                        break;
                    case UIStyleType.CostText:
                        ApplyTextTheme(tag.gameObject, theme.costStyle, theme.costTextColor, theme);
                        break;
                    case UIStyleType.WarningText:
                        ApplyTextTheme(tag.gameObject, theme.bodyStyle, theme.warningTextColor, theme);
                        break;
                    case UIStyleType.DangerText:
                        ApplyTextTheme(tag.gameObject, theme.bodyStyle, theme.dangerTextColor, theme);
                        break;
                }
            }
        }

        private static void ApplyButtonTheme(GameObject go, ButtonTheme btnTheme, UIThemeSO theme)
        {
            if (btnTheme == null) return;
            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                btn.colors = btnTheme.ToColorBlock();
                var img = go.GetComponent<Image>();
                if (img != null) img.color = btnTheme.imageColor;
            }
            var txt = go.GetComponentInChildren<TMP_Text>();
            if (txt != null)
            {
                txt.color = btnTheme.textColor;
                txt.fontSize = btnTheme.fontSize;
                txt.characterSpacing = btnTheme.characterSpacing;
                if (theme.defaultFont != null) txt.font = theme.defaultFont;
            }
        }

        private static void ApplyTextTheme(GameObject go, TextStyle textStyle, Color textColor, UIThemeSO theme)
        {
            if (textStyle == null) return;
            var txt = go.GetComponent<TMP_Text>();
            if (txt == null) return;
            txt.color = textColor;
            txt.fontSize = textStyle.fontSize;
            txt.fontStyle = textStyle.fontStyle;
            txt.characterSpacing = textStyle.characterSpacing;
            txt.wordSpacing = textStyle.wordSpacing;
            if (textStyle.lineSpacing > 0) txt.lineSpacing = textStyle.lineSpacing;
            if (theme.defaultFont != null) txt.font = theme.defaultFont;
        }

        public static RectTransform CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
            return rt;
        }

        public static TMP_Text AddText(Transform parent, string text, float size, Color color,
            TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft)
        {
            var go = new GameObject("T", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }

        public static TMP_Text AddThemedText(Transform parent, string text, float size, Color color,
            TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft)
        {
            var tmp = AddText(parent, text, size, color, alignment);
            if (ResolvedThemeFont != null) tmp.font = ResolvedThemeFont;
            return tmp;
        }

        public static Image AddImage(RectTransform rt, Color color, bool raycastTarget = true)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = raycastTarget;
            return img;
        }

        public static Button CreateButton(Transform parent, string name, string label,
            System.Action onClick, Color? bgColor = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;

            var img = go.AddComponent<Image>();
            img.color = bgColor ?? UIColors.Default.RowBg;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            btn.colors = colors;

            var lbl = AddThemedText(go.transform, label, 16, UIColors.Default.Text,
                TextAlignmentOptions.Center);
            StretchFull(lbl.rectTransform);

            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }

        public static void StretchFull(RectTransform rt, Vector2? offsetMin = null, Vector2? offsetMax = null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = offsetMin ?? Vector2.zero;
            rt.offsetMax = offsetMax ?? Vector2.zero;
        }

        public static void SetAnchors(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
        }

        public static void SetupPanelBackground(GameObject panel, UIColors.PanelColors colors)
        {
            var oldVlg = panel.GetComponent<VerticalLayoutGroup>();
            if (oldVlg != null) Object.DestroyImmediate(oldVlg);
            var oldImages = panel.GetComponents<Image>();
            foreach (var img in oldImages) Object.DestroyImmediate(img);
            var oldCg = panel.GetComponent<CanvasGroup>();
            if (oldCg != null) Object.DestroyImmediate(oldCg);

            var bg = panel.AddComponent<Image>();
            bg.color = colors.PanelBg;
            bg.raycastTarget = true;

            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        public static VerticalLayoutGroup AddStandardVLG(GameObject go, RectOffset padding = null,
            float spacing = 8f)
        {
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.padding = padding ?? new RectOffset(20, 20, 15, 15);
            vlg.spacing = spacing;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            return vlg;
        }

        public static LayoutElement AddLayoutElement(GameObject go, float? minWidth = null,
            float? preferredWidth = null, float? minHeight = null, float? preferredHeight = null)
        {
            var le = go.AddComponent<LayoutElement>();
            if (minWidth.HasValue) le.minWidth = minWidth.Value;
            if (preferredWidth.HasValue) le.preferredWidth = preferredWidth.Value;
            if (minHeight.HasValue) le.minHeight = minHeight.Value;
            if (preferredHeight.HasValue) le.preferredHeight = preferredHeight.Value;
            return le;
        }

        public static HorizontalLayoutGroup AddRowHLG(GameObject go, RectOffset padding = null,
            float spacing = 10f)
        {
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = padding ?? new RectOffset(12, 12, 4, 4);
            hlg.spacing = spacing;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            return hlg;
        }
    }
}
