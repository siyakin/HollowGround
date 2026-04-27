using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public static class UIPrimitiveFactory
    {
        public static TMP_FontAsset ResolvedThemeFont => _themeFont ??= ResolveThemeFont();
        private static TMP_FontAsset _themeFont;

        private static TMP_FontAsset ResolveThemeFont()
        {
#if UNITY_EDITOR
            var theme = UnityEditor.AssetDatabase.LoadAssetAtPath<UIThemeSO>(
                "Assets/_Project/ScriptableObjects/UITheme.asset");
#else
            var results = UnityEngine.Resources.LoadAll<UIThemeSO>("");
            var theme = results.Length > 0 ? results[0] : null;
#endif
            return theme != null ? theme.defaultFont : null;
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
            img.color = bgColor ?? new Color(0.25f, 0.27f, 0.32f, 1f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            btn.colors = colors;

            var lbl = AddThemedText(go.transform, label, 16, new Color(0.95f, 0.95f, 0.95f, 1f),
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
