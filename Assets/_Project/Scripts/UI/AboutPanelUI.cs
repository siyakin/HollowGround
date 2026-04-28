using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class AboutPanelUI : MonoBehaviour
    {
        private bool _built;

        private static string ReadVersion()
        {
            try
            {
                var path = Path.Combine(UnityEngine.Application.dataPath, "..", "VERSION");
                if (File.Exists(path))
                    return File.ReadAllText(path).Trim();
            }
            catch { }
            return "dev";
        }

        private const string Content =
            "<color=#FFD700><b>HOLLOW GROUND</b></color>\n" +
            "<color=#888888>v</color><color=#FFFFFF>{VER}</color> <color=#888888>| Unity 6 + URP | Low Poly 3D</color>\n" +
            "\n" +
            "<color=#FFD700>-- S U R V I V O R S --</color>\n" +
            "<color=#FFFFFF>siyakin</color>  <color=#888888>Director / Survivor #001</color>\n" +
            "\n" +
            "<color=#FFD700>-- A I   S Q U A D --</color>\n" +
            "<color=#4FC3F7>z.ai GLM-5.1</color>  <color=#888888>Architecture, Refactoring</color>\n" +
            "<color=#7C4DFF>Claude</color>  <color=#888888>Blender Modeling, 105 FBX</color>\n" +
            "<color=#FF6D00>Grok 4.3</color>  <color=#888888>Parametric Modeling</color>\n" +
            "<color=#00E676>Kilo</color>  <color=#888888>UI Theme, Visual Polish</color>\n" +
            "\n" +
            "<color=#FFD700>-- S T A T S --</color>\n" +
            "<color=#AAAAAA>Phases:</color>   <color=#FFFFFF>15 completed</color>\n" +
            "<color=#AAAAAA>Scripts:</color>  <color=#FFFFFF>75+ C# files</color>\n" +
            "<color=#AAAAAA>Models:</color>   <color=#FFFFFF>105 FBX (15 bld x 7 state)</color>\n" +
            "<color=#AAAAAA>Panels:</color>   <color=#FFFFFF>15+ UI panels</color>\n" +
            "<color=#AAAAAA>SO:</color>       <color=#FFFFFF>38+ ScriptableObjects</color>\n" +
            "<color=#AAAAAA>Playtest:</color> <color=#44FF44>13/13 PASSED</color>\n" +
            "\n" +
            "<color=#666666>Built with human-AI collaboration. No AI went rogue.</color>\n" +
            "<color=#888888>F1 / ESC to close</color>";

        private void OnEnable()
        {
            if (!_built) BuildUI();
        }

        public void Show()
        {
            if (!_built) BuildUI();
            ApplyFont();
            gameObject.SetActive(true);
        }

        private void ApplyFont()
        {
            var theme = UIThemeManager.Instance?.CurrentTheme;
            if (theme == null || theme.defaultFont == null) return;
            foreach (var tmp in GetComponentsInChildren<TMP_Text>(true))
                tmp.font = theme.defaultFont;
        }

        private void BuildUI()
        {
            var root = GetComponent<RectTransform>();
            if (root == null) return;

            UIPrimitiveFactory.SetupPanelBackground(gameObject, UIColors.Default);

            var vlg = GetComponent<VerticalLayoutGroup>();
            if (vlg != null) Destroy(vlg);

            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.sizeDelta = new Vector2(520f, 640f);
            root.anchoredPosition = Vector2.zero;

            var text = Content.Replace("{VER}", ReadVersion());

            var tmp = UIPrimitiveFactory.AddThemedText(transform, text, 14, Color.white, TextAlignmentOptions.Center);
            tmp.raycastTarget = false;
            tmp.richText = true;
            tmp.lineSpacing = 4f;

            var rt = tmp.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(16f, 12f);
            rt.offsetMax = new Vector2(-16f, -12f);

            gameObject.SetActive(false);
            _built = true;
        }
    }
}
