using System.IO;
using System.Text;
using HollowGround.Army;
using HollowGround.Buildings;
using HollowGround.Core;
using HollowGround.Heroes;
using HollowGround.NPCs;
using HollowGround.Quests;
using HollowGround.Tech;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class AboutPanelUI : MonoBehaviour
    {
        private bool _built;
        private TMP_Text _contentText;

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

        private void OnEnable()
        {
            if (!_built) BuildUI();
            RefreshStats();
        }

        public void Show()
        {
            if (!_built) BuildUI();
            ApplyFont();
            RefreshStats();
            gameObject.SetActive(true);
        }

        private void ApplyFont()
        {
            var theme = UIThemeManager.Instance?.CurrentTheme;
            if (theme == null || theme.defaultFont == null) return;
            foreach (var tmp in GetComponentsInChildren<TMP_Text>(true))
                tmp.font = theme.defaultFont;
        }

        private void RefreshStats()
        {
            if (_contentText == null) return;

            int buildingCount = CountScriptableObjects<BuildingData>();
            int troopCount = CountScriptableObjects<TroopData>();
            int heroCount = CountScriptableObjects<HeroData>();
            int techCount = CountScriptableObjects<TechNode>();
            int factionCount = CountScriptableObjects<FactionData>();
            int questCount = CountScriptableObjects<QuestData>();
            int totalSO = buildingCount + troopCount + heroCount + techCount + factionCount + questCount;

            int buildings = CountSceneBuildings();
            int heroes = CountSceneHeroes();
            float playTime = CountPlayTime();

            _contentText.text = BuildContent(ReadVersion(), totalSO, buildings, heroes, playTime);
        }

        private static int CountScriptableObjects<T>() where T : Object
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T).Name}").Length;
#else
            return UnityEngine.Resources.LoadAll<T>("").Length;
#endif
        }

        private static int CountSceneBuildings()
        {
            var bm = BuildingManager.Instance;
            if (bm == null) return 0;
            return FindAnyObjectByType<Building>(FindObjectsInactive.Include) != null
                ? FindObjectsByType<Building>(FindObjectsInactive.Include).Length : 0;
        }

        private static int CountSceneHeroes()
        {
            var hm = HeroManager.Instance;
            return hm != null ? hm.HeroCount : 0;
        }

        private static float CountPlayTime()
        {
            var ss = SaveSystem.Instance;
            if (ss == null) return 0f;
            var saves = ss.GetAllSaves();
            float total = 0f;
            foreach (var s in saves) total += s.PlayTime;
            return total;
        }

        private static string BuildContent(string ver, int soCount, int buildings, int heroes, float playTime)
        {
            int playMin = Mathf.FloorToInt(playTime / 60f);
            int playHr = playMin / 60;
            playMin %= 60;

            var sb = new StringBuilder();
            sb.AppendLine("<color=#FFD700><b>HOLLOW GROUND</b></color>");
            sb.AppendLine($"<color=#888888>v</color><color=#FFFFFF>{ver}</color> <color=#888888>| Unity 6 + URP | Low Poly 3D</color>");
            sb.AppendLine();
            sb.AppendLine("<color=#FFD700>-- S U R V I V O R S --</color>");
            sb.AppendLine("<color=#FFFFFF>siyakin</color>  <color=#888888>Director / Survivor #001</color>");
            sb.AppendLine();
            sb.AppendLine("<color=#FFD700>-- A I   S Q U A D --</color>");
            sb.AppendLine("<color=#4FC3F7>z.ai GLM-5.1</color>  <color=#888888>Architecture, Refactoring</color>");
            sb.AppendLine("<color=#7C4DFF>Claude</color>  <color=#888888>Blender Modeling, 105 FBX</color>");
            sb.AppendLine("<color=#FF6D00>Grok 4.3</color>  <color=#888888>Parametric Modeling</color>");
            sb.AppendLine("<color=#00E676>Kilo</color>  <color=#888888>UI Theme, Visual Polish</color>");
            sb.AppendLine();
            sb.AppendLine("<color=#FFD700>-- L I V E   S T A T S --</color>");
            sb.AppendLine($"  <color=#AAAAAA>ScriptableObjects:</color>  <color=#FFFFFF>{soCount}</color>");
            sb.AppendLine($"  <color=#AAAAAA>Buildings:</color>          <color=#FFFFFF>{buildings}</color>");
            sb.AppendLine($"  <color=#AAAAAA>Heroes:</color>             <color=#FFFFFF>{heroes}</color>");
            sb.AppendLine($"  <color=#AAAAAA>Total Play:</color>         <color=#FFFFFF>{playHr}h {playMin}m</color>");
            sb.AppendLine();
            sb.AppendLine("<color=#666666>Built with human-AI collaboration. No AI went rogue.</color>");
            sb.Append("<color=#888888>F1 / ESC to close</color>");
            return sb.ToString();
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

            _contentText = UIPrimitiveFactory.AddThemedText(transform, "", 16, Color.white, TextAlignmentOptions.Midline);
            _contentText.raycastTarget = false;
            _contentText.richText = true;
            _contentText.lineSpacing = 6f;

            var rt = _contentText.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(16f, 12f);
            rt.offsetMax = new Vector2(-16f, -12f);

            gameObject.SetActive(false);
            _built = true;
        }
    }
}
