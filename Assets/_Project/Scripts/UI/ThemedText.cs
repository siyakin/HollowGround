using TMPro;
using UnityEngine;

namespace HollowGround.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class ThemedText : MonoBehaviour
    {
        public UIStyleType styleType = UIStyleType.BodyText;

        private TMP_Text _text;

        private void OnEnable()
        {
            _text = GetComponent<TMP_Text>();
            ApplyStyle();

            if (UIThemeManager.Instance != null)
                UIThemeManager.Instance.OnThemeChanged += ApplyStyle;
        }

        private void OnDisable()
        {
            if (UIThemeManager.Instance != null)
                UIThemeManager.Instance.OnThemeChanged -= ApplyStyle;
        }

        private void ApplyStyle()
        {
            var theme = UIThemeManager.Instance?.CurrentTheme;
            if (theme == null || _text == null) return;

            var (textStyle, textColor) = styleType switch
            {
                UIStyleType.HeaderText => (theme.headerStyle, theme.headerTextColor),
                UIStyleType.LabelText => (theme.labelStyle, theme.labelTextColor),
                UIStyleType.CostText => (theme.costStyle, theme.costTextColor),
                UIStyleType.WarningText => (theme.bodyStyle, theme.warningTextColor),
                UIStyleType.DangerText => (theme.bodyStyle, theme.dangerTextColor),
                _ => (theme.bodyStyle, theme.bodyTextColor)
            };

            if (textStyle == null) return;

            _text.color = textColor;
            _text.fontSize = textStyle.fontSize;
            _text.fontStyle = textStyle.fontStyle;
            _text.characterSpacing = textStyle.characterSpacing;
            _text.wordSpacing = textStyle.wordSpacing;
            if (textStyle.lineSpacing > 0) _text.lineSpacing = textStyle.lineSpacing;
            if (theme.defaultFont != null) _text.font = theme.defaultFont;
        }
    }
}
