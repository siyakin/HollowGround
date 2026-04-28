using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    [RequireComponent(typeof(Button))]
    public class ThemedButton : MonoBehaviour
    {
        public UIStyleType styleType = UIStyleType.ActionBarButton;

        private Button _button;
        private Image _image;
        private TMP_Text _label;

        private void OnEnable()
        {
            CacheComponents();
            ApplyStyle();

            if (UIThemeManager.Instance != null)
                UIThemeManager.Instance.OnThemeChanged += ApplyStyle;
        }

        private void OnDisable()
        {
            if (UIThemeManager.Instance != null)
                UIThemeManager.Instance.OnThemeChanged -= ApplyStyle;
        }

        private void CacheComponents()
        {
            _button = GetComponent<Button>();
            _image = GetComponent<Image>();
            _label = GetComponentInChildren<TMP_Text>();
        }

        private void ApplyStyle()
        {
            var theme = UIThemeManager.Instance?.CurrentTheme;
            if (theme == null) return;

            var btnTheme = styleType switch
            {
                UIStyleType.ConfirmButton => theme.confirmButton,
                UIStyleType.DangerButton => theme.dangerButton,
                UIStyleType.BuildingCardButton => theme.buildingCardButton,
                UIStyleType.TabButton => theme.tabButton,
                _ => theme.actionBarButton
            };

            if (btnTheme == null) return;

            if (_button != null)
                _button.colors = btnTheme.ToColorBlock();

            if (_image != null)
                _image.color = btnTheme.imageColor;

            if (_label != null)
            {
                _label.color = UIColors.ContrastTextForButton(
                    _image != null ? _image.color : Color.white,
                    _button != null ? _button.colors.normalColor : Color.white);
                _label.fontSize = btnTheme.fontSize;
                _label.characterSpacing = btnTheme.characterSpacing;
                if (theme.defaultFont != null) _label.font = theme.defaultFont;
            }
        }
    }
}
