using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using HollowGround.UI;

namespace HollowGround.Editor
{
    public static class UIThemeApplier
    {
        const string THEME_PATH = "Assets/_Project/ScriptableObjects/UITheme.asset";

        [MenuItem("HollowGround/Apply UI Theme %#t")]
        public static void ApplyTheme()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeSO>(THEME_PATH);
            if (theme == null)
            {
                EditorUtility.DisplayDialog("Error",
                    $"UITheme asset not found:\n{THEME_PATH}\n\nRun 'HollowGround > Create UI Theme Asset' first.",
                    "OK");
                return;
            }

            var tags = Object.FindObjectsByType<UIThemeTag>(FindObjectsInactive.Include);
            int count = 0;
            foreach (var tag in tags)
            {
                Apply(tag, theme);
                count++;
            }

            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log($"[UITheme] {count} elements updated.");
        }

        [MenuItem("HollowGround/Create UI Theme Asset")]
        public static void CreateThemeAsset()
        {
            if (AssetDatabase.LoadAssetAtPath<UIThemeSO>(THEME_PATH) != null)
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<UIThemeSO>(THEME_PATH);
                EditorGUIUtility.PingObject(Selection.activeObject);
                return;
            }

            var theme = ScriptableObject.CreateInstance<UIThemeSO>();
            AssetDatabase.CreateAsset(theme, THEME_PATH);
            AssetDatabase.SaveAssets();
            Selection.activeObject = theme;
            Debug.Log("[UITheme] Asset created: " + THEME_PATH);
        }

        static void Apply(UIThemeTag tag, UIThemeSO theme)
        {
            switch (tag.styleType)
            {
                case UIStyleType.BuildingCardButton: ApplyButton(tag, theme.buildingCardButton, theme.defaultFont); break;
                case UIStyleType.TabButton:          ApplyButton(tag, theme.tabButton,          theme.defaultFont); break;
                case UIStyleType.ActionBarButton:    ApplyButton(tag, theme.actionBarButton,    theme.defaultFont); break;
                case UIStyleType.ConfirmButton:      ApplyButton(tag, theme.confirmButton,      theme.defaultFont); break;
                case UIStyleType.DangerButton:       ApplyButton(tag, theme.dangerButton,       theme.defaultFont); break;

                case UIStyleType.DarkPanel:   ApplyImage(tag, theme.darkPanelColor);   break;
                case UIStyleType.ResourceBar: ApplyImage(tag, theme.resourceBarColor); break;
                case UIStyleType.ActionBar:   ApplyImage(tag, theme.actionBarColor);   break;

                case UIStyleType.HeaderText:  ApplyText(tag, theme.headerTextColor,  theme.headerStyle, theme.defaultFont); break;
                case UIStyleType.BodyText:    ApplyText(tag, theme.bodyTextColor,    theme.bodyStyle,   theme.defaultFont); break;
                case UIStyleType.LabelText:   ApplyText(tag, theme.labelTextColor,   theme.labelStyle,  theme.defaultFont); break;
                case UIStyleType.CostText:    ApplyText(tag, theme.costTextColor,    theme.costStyle,   theme.defaultFont); break;
                case UIStyleType.WarningText: ApplyText(tag, theme.warningTextColor, null,              theme.defaultFont); break;
                case UIStyleType.DangerText:  ApplyText(tag, theme.dangerTextColor,  null,              theme.defaultFont); break;
            }
        }

        static void ApplyButton(UIThemeTag tag, ButtonTheme bt, TMP_FontAsset fallbackFont)
        {
            if (bt == null) return;

            var btn = tag.GetComponent<Button>();
            if (btn != null)
            {
                btn.colors = bt.ToColorBlock();
                EditorUtility.SetDirty(btn);
            }

            var img = tag.GetComponent<Image>();
            if (img != null)
            {
                img.color = bt.imageColor;
                EditorUtility.SetDirty(img);
            }

            var tmp = tag.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
            {
                tmp.enableAutoSizing = false;
                tmp.color            = bt.textColor;
                tmp.fontSize         = bt.fontSize;
                tmp.characterSpacing = bt.characterSpacing;
                if (fallbackFont != null) tmp.font = fallbackFont;
                EditorUtility.SetDirty(tmp);
            }
        }

        static void ApplyImage(UIThemeTag tag, Color color)
        {
            var img = tag.GetComponent<Image>();
            if (img != null)
            {
                img.color = color;
                EditorUtility.SetDirty(img);
            }
        }

        static void ApplyText(UIThemeTag tag, Color color, TextStyle style, TMP_FontAsset fallbackFont)
        {
            var tmp = tag.GetComponent<TextMeshProUGUI>();
            if (tmp == null) return;

            tmp.enableAutoSizing = false;
            tmp.color            = color;

            if (style != null)
            {
                tmp.fontStyle        = style.fontStyle;
                tmp.characterSpacing = style.characterSpacing;
                tmp.wordSpacing      = style.wordSpacing;
                tmp.lineSpacing      = style.lineSpacing;
                if (style.fontSize > 0) tmp.fontSize = style.fontSize;
                var font = style.font ?? fallbackFont;
                if (font != null) tmp.font = font;
            }
            else if (fallbackFont != null)
            {
                tmp.font = fallbackFont;
            }

            EditorUtility.SetDirty(tmp);
        }
    }
}
