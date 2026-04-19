using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    [System.Serializable]
    public class ButtonTheme
    {
        public Color normalColor      = new Color(0.13f, 0.17f, 0.10f, 0.95f);
        public Color highlightedColor = new Color(0.83f, 0.52f, 0.04f, 1.00f);
        public Color pressedColor     = new Color(0.55f, 0.32f, 0.02f, 1.00f);
        public Color selectedColor    = new Color(0.83f, 0.52f, 0.04f, 1.00f);
        public Color disabledColor    = new Color(0.20f, 0.20f, 0.18f, 0.50f);
        public float fadeDuration     = 0.12f;
        public Color imageColor       = Color.white;
        public Color textColor        = Color.white;
        public int   fontSize         = 16;

        public ColorBlock ToColorBlock()
        {
            return new ColorBlock
            {
                normalColor      = normalColor,
                highlightedColor = highlightedColor,
                pressedColor     = pressedColor,
                selectedColor    = selectedColor,
                disabledColor    = disabledColor,
                colorMultiplier  = 1f,
                fadeDuration     = fadeDuration,
            };
        }
    }

    [CreateAssetMenu(fileName = "UITheme", menuName = "HollowGround/UI Theme")]
    public class UIThemeSO : ScriptableObject
    {
        [Header("Button Styles")]
        public ButtonTheme primaryButton;
        public ButtonTheme dangerButton;
        public ButtonTheme secondaryButton;

        [Header("Panel Backgrounds")]
        public Color darkPanelColor   = new Color(0.05f, 0.05f, 0.04f, 0.92f);
        public Color resourceBarColor = new Color(0.05f, 0.05f, 0.04f, 0.90f);
        public Color actionBarColor   = new Color(0.06f, 0.07f, 0.05f, 0.82f);

        [Header("Text Colors")]
        public Color headerTextColor  = Color.white;
        public Color bodyTextColor    = new Color(0.85f, 0.85f, 0.85f, 1f);
        public Color labelTextColor   = new Color(0.60f, 0.60f, 0.55f, 1f);
        public Color warningTextColor = new Color(0.95f, 0.65f, 0.10f, 1f);
        public Color dangerTextColor  = new Color(0.90f, 0.25f, 0.20f, 1f);

        [Header("Font Sizes")]
        public int headerFontSize = 20;
        public int bodyFontSize   = 14;
        public int labelFontSize  = 12;

        void Reset()
        {
            primaryButton = new ButtonTheme
            {
                normalColor      = new Color(0.13f, 0.17f, 0.10f, 0.95f),
                highlightedColor = new Color(0.83f, 0.52f, 0.04f, 1.00f),
                pressedColor     = new Color(0.55f, 0.32f, 0.02f, 1.00f),
                selectedColor    = new Color(0.83f, 0.52f, 0.04f, 1.00f),
                disabledColor    = new Color(0.20f, 0.20f, 0.18f, 0.50f),
                imageColor       = Color.white,
                textColor        = Color.white,
                fontSize         = 16,
            };

            dangerButton = new ButtonTheme
            {
                normalColor      = new Color(0.35f, 0.07f, 0.07f, 0.95f),
                highlightedColor = new Color(0.80f, 0.15f, 0.15f, 1.00f),
                pressedColor     = new Color(0.55f, 0.08f, 0.08f, 1.00f),
                selectedColor    = new Color(0.80f, 0.15f, 0.15f, 1.00f),
                disabledColor    = new Color(0.20f, 0.10f, 0.10f, 0.50f),
                imageColor       = Color.white,
                textColor        = Color.white,
                fontSize         = 16,
            };

            secondaryButton = new ButtonTheme
            {
                normalColor      = new Color(0.18f, 0.18f, 0.16f, 0.95f),
                highlightedColor = new Color(0.35f, 0.35f, 0.30f, 1.00f),
                pressedColor     = new Color(0.12f, 0.12f, 0.10f, 1.00f),
                selectedColor    = new Color(0.35f, 0.35f, 0.30f, 1.00f),
                disabledColor    = new Color(0.15f, 0.15f, 0.13f, 0.50f),
                imageColor       = Color.white,
                textColor        = new Color(0.75f, 0.75f, 0.75f, 1f),
                fontSize         = 14,
            };
        }
    }
}
