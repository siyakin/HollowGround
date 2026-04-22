using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        public float characterSpacing = 1f;

        public ColorBlock ToColorBlock() => new ColorBlock
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

    [System.Serializable]
    public class TextStyle
    {
        public TMP_FontAsset font;               // null → defaultFont kullanılır
        public int           fontSize         = 14;
        public FontStyles    fontStyle        = FontStyles.Normal;
        public float         characterSpacing = 0f;
        public float         wordSpacing      = 0f;
        public float         lineSpacing      = 0f;
    }

    [CreateAssetMenu(fileName = "UITheme", menuName = "HollowGround/UI Theme")]
    public class UIThemeSO : ScriptableObject
    {
        [Header("Buton Stilleri")]
        public ButtonTheme buildingCardButton;
        public ButtonTheme tabButton;
        public ButtonTheme actionBarButton;
        public ButtonTheme confirmButton;
        public ButtonTheme dangerButton;

        [Header("Panel Renkleri")]
        public Color darkPanelColor   = new Color(0.05f, 0.05f, 0.04f, 0.92f);
        public Color resourceBarColor = new Color(0.05f, 0.05f, 0.04f, 0.90f);
        public Color actionBarColor   = new Color(0.06f, 0.07f, 0.05f, 0.82f);

        [Header("Metin Renkleri")]
        public Color headerTextColor  = Color.white;
        public Color bodyTextColor    = new Color(0.85f, 0.85f, 0.85f, 1f);
        public Color labelTextColor   = new Color(0.78f, 0.78f, 0.74f, 1f);
        public Color costTextColor    = new Color(0.95f, 0.80f, 0.40f, 1f);
        public Color warningTextColor = new Color(0.95f, 0.65f, 0.10f, 1f);
        public Color dangerTextColor  = new Color(0.90f, 0.25f, 0.20f, 1f);

        [Header("Font")]
        public TMP_FontAsset defaultFont;

        [Header("Metin Stilleri")]
        public TextStyle headerStyle = new TextStyle { fontSize = 30, fontStyle = FontStyles.Bold,   characterSpacing = 0f };
        public TextStyle bodyStyle   = new TextStyle { fontSize = 22, fontStyle = FontStyles.Normal, characterSpacing = 0f };
        public TextStyle labelStyle  = new TextStyle { fontSize = 18, fontStyle = FontStyles.Normal, characterSpacing = 0f };
        public TextStyle costStyle   = new TextStyle { fontSize = 16, fontStyle = FontStyles.Normal, characterSpacing = 0f };

        void Reset()
        {
            buildingCardButton = new ButtonTheme
            {
                normalColor      = new Color(0.13f, 0.17f, 0.10f, 0.95f),
                highlightedColor = new Color(0.83f, 0.52f, 0.04f, 1.00f),
                pressedColor     = new Color(0.55f, 0.32f, 0.02f, 1.00f),
                selectedColor    = new Color(0.83f, 0.52f, 0.04f, 1.00f),
                disabledColor    = new Color(0.20f, 0.20f, 0.18f, 0.50f),
                imageColor       = Color.white,
                textColor        = Color.white,
                fontSize         = 22,
                characterSpacing = 0f,
            };

            tabButton = new ButtonTheme
            {
                normalColor      = new Color(0.15f, 0.15f, 0.13f, 0.85f),
                highlightedColor = new Color(0.30f, 0.30f, 0.25f, 1.00f),
                pressedColor     = new Color(0.10f, 0.10f, 0.08f, 1.00f),
                selectedColor    = new Color(0.83f, 0.52f, 0.04f, 1.00f),
                disabledColor    = new Color(0.15f, 0.15f, 0.13f, 0.40f),
                imageColor       = Color.white,
                textColor        = new Color(0.80f, 0.80f, 0.76f, 1f),
                fontSize         = 18,
                characterSpacing = 0f,
            };

            actionBarButton = new ButtonTheme
            {
                normalColor      = new Color(0.08f, 0.10f, 0.07f, 0.80f),
                highlightedColor = new Color(0.83f, 0.52f, 0.04f, 1.00f),
                pressedColor     = new Color(0.55f, 0.32f, 0.02f, 1.00f),
                selectedColor    = new Color(0.83f, 0.52f, 0.04f, 1.00f),
                disabledColor    = new Color(0.15f, 0.15f, 0.13f, 0.40f),
                imageColor       = Color.white,
                textColor        = new Color(0.85f, 0.85f, 0.80f, 1f),
                fontSize         = 18,
                characterSpacing = 0f,
            };

            confirmButton = new ButtonTheme
            {
                normalColor      = new Color(0.12f, 0.38f, 0.16f, 0.95f),
                highlightedColor = new Color(0.20f, 0.60f, 0.25f, 1.00f),
                pressedColor     = new Color(0.08f, 0.25f, 0.10f, 1.00f),
                selectedColor    = new Color(0.20f, 0.60f, 0.25f, 1.00f),
                disabledColor    = new Color(0.15f, 0.22f, 0.15f, 0.50f),
                imageColor       = Color.white,
                textColor        = Color.white,
                fontSize         = 22,
                characterSpacing = 0f,
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
                fontSize         = 22,
                characterSpacing = 0f,
            };
        }
    }
}
