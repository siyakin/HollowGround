using HollowGround.Core;
using UnityEngine;

namespace HollowGround.UI
{
    public class UIThemeManager : Singleton<UIThemeManager>
    {
        public UIThemeSO CurrentTheme { get; private set; }
        public event System.Action OnThemeChanged;

        protected override void Awake()
        {
            base.Awake();
            LoadDefaultTheme();
        }

        private void LoadDefaultTheme()
        {
#if UNITY_EDITOR
            CurrentTheme = UnityEditor.AssetDatabase.LoadAssetAtPath<UIThemeSO>(
                "Assets/_Project/ScriptableObjects/UITheme.asset");
#else
            var results = UnityEngine.Resources.LoadAll<UIThemeSO>("UITheme");
            CurrentTheme = results.Length > 0 ? results[0] : null;
#endif
        }

        public void SetTheme(UIThemeSO theme)
        {
            if (theme == null) return;
            CurrentTheme = theme;
            OnThemeChanged?.Invoke();
        }

        public void SetFont(TMPro.TMP_FontAsset font)
        {
            if (CurrentTheme == null || font == null) return;
            CurrentTheme.defaultFont = font;
            OnThemeChanged?.Invoke();
        }

        public void Refresh()
        {
            OnThemeChanged?.Invoke();
        }
    }
}
