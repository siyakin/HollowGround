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
                EditorUtility.DisplayDialog("Hata",
                    $"UITheme asset bulunamadı:\n{THEME_PATH}\n\nÖnce 'HollowGround > Create UI Theme Asset' menüsünü çalıştır.",
                    "Tamam");
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
            Debug.Log($"[UITheme] {count} element güncellendi.");
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
            Debug.Log("[UITheme] Asset oluşturuldu: " + THEME_PATH);
        }

        static void Apply(UIThemeTag tag, UIThemeSO theme)
        {
            switch (tag.styleType)
            {
                case UIStyleType.PrimaryButton:
                    ApplyButton(tag, theme.primaryButton);
                    break;
                case UIStyleType.DangerButton:
                    ApplyButton(tag, theme.dangerButton);
                    break;
                case UIStyleType.SecondaryButton:
                    ApplyButton(tag, theme.secondaryButton);
                    break;
                case UIStyleType.DarkPanel:
                    ApplyImage(tag, theme.darkPanelColor);
                    break;
                case UIStyleType.ResourceBar:
                    ApplyImage(tag, theme.resourceBarColor);
                    break;
                case UIStyleType.ActionBar:
                    ApplyImage(tag, theme.actionBarColor);
                    break;
                case UIStyleType.HeaderText:
                    ApplyText(tag, theme.headerTextColor, theme.headerFontSize);
                    break;
                case UIStyleType.BodyText:
                    ApplyText(tag, theme.bodyTextColor, theme.bodyFontSize);
                    break;
                case UIStyleType.LabelText:
                    ApplyText(tag, theme.labelTextColor, theme.labelFontSize);
                    break;
                case UIStyleType.WarningText:
                    ApplyText(tag, theme.warningTextColor, 0);
                    break;
                case UIStyleType.DangerText:
                    ApplyText(tag, theme.dangerTextColor, 0);
                    break;
            }
        }

        static void ApplyButton(UIThemeTag tag, ButtonTheme bt)
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
                tmp.color    = bt.textColor;
                tmp.fontSize = bt.fontSize;
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

        static void ApplyText(UIThemeTag tag, Color color, int fontSize)
        {
            var tmp = tag.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.color = color;
                if (fontSize > 0) tmp.fontSize = fontSize;
                EditorUtility.SetDirty(tmp);
            }
        }
    }
}
