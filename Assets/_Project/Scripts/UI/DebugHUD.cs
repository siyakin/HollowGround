using System.Collections.Generic;
using HollowGround.Army;
using HollowGround.Buildings;
using HollowGround.Core;
using HollowGround.Grid;
using HollowGround.Heroes;
using HollowGround.NPCs;
using HollowGround.Resources;
using HollowGround.Roads;
using TMPro;
using UnityEngine;

namespace HollowGround.UI
{
    public class DebugHUD : MonoBehaviour
    {
        private static readonly string ColGold = "<color=#FFD700>";
        private static readonly string ColGray = "<color=#888888>";
        private static readonly string ColSilver = "<color=#CCCCCC>";
        private const string ColEnd = "</color>";

        public enum DebugTab
        {
            Basic,
            Buildings,
            Events
        }

        [SerializeField] private TMP_Text _debugText;
        [SerializeField] private float _updateInterval = 0.5f;

        private float _timer;
        private DebugTab _currentTab = DebugTab.Basic;

        private const int MaxEventLog = 50;
        private readonly List<string> _eventLog = new();

        public void SetTab(int tab)
        {
            _currentTab = (DebugTab)tab;
            _timer = _updateInterval;
        }

        private void OnEnable()
        {
            ToastUI.OnToastShown += OnToastShown;
        }

        private void OnDisable()
        {
            ToastUI.OnToastShown -= OnToastShown;
        }

        private void OnToastShown(string text, Color color)
        {
            LogEvent($"{ColSilver}[TOAST]{ColEnd} {text}");
        }

        private void LogEvent(string msg)
        {
            float t = Time.unscaledTime;
            int min = (int)(t / 60);
            int sec = (int)(t % 60);
            _eventLog.Insert(0, $"[{min:D2}:{sec:D2}] {msg}");
            if (_eventLog.Count > MaxEventLog)
                _eventLog.RemoveRange(MaxEventLog, _eventLog.Count - MaxEventLog);
        }

        private void Update()
        {
            var cfg = GameConfig.Instance;
            if (cfg != null && !cfg.EnableDebugHUD)
            {
                if (_debugText != null) _debugText.gameObject.SetActive(false);
                return;
            }

            _timer += Time.unscaledDeltaTime;
            if (_timer < _updateInterval) return;
            _timer = 0f;
            Refresh();
        }

        private void Refresh()
        {
            if (_debugText == null) return;

            switch (_currentTab)
            {
                case DebugTab.Basic: RefreshBasic(); break;
                case DebugTab.Buildings: RefreshBuildings(); break;
                case DebugTab.Events: RefreshEvents(); break;
            }
        }

        private void RefreshBasic()
        {
            var lines = new List<string>
            {
                $"{ColGold}--- BASIC ---{ColEnd}",
                ""
            };

            lines.Add("<b>Game State:</b>");
            if (GameManager.Instance != null)
                lines.Add($"  State: {GameManager.Instance.CurrentState}");
            if (TimeManager.Instance != null)
                lines.Add($"  Speed: {TimeManager.Instance.GameSpeed}x | Time: {TimeManager.Instance.GameTime:F0}s");
            lines.Add("");

            lines.Add("<b>Resources:</b>");
            if (ResourceManager.Instance != null)
            {
                foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
                {
                    int amount = ResourceManager.Instance.Get(type);
                    int cap = ResourceManager.Instance.GetCapacity(type);
                    lines.Add($"  {type}: {amount}/{cap}");
                }
            }
            lines.Add("");

            lines.Add("<b>Army:</b>");
            if (ArmyManager.Instance != null)
            {
                lines.Add($"  Total: {ArmyManager.Instance.TotalTroopCount}");
                lines.Add($"  Power: {ArmyManager.Instance.CalculateArmyPower()}");
                lines.Add($"  Morale: {ArmyManager.Instance.GetMorale():P0}");

                foreach (TroopType type in System.Enum.GetValues(typeof(TroopType)))
                {
                    int count = ArmyManager.Instance.GetTroopCount(type);
                    if (count > 0)
                        lines.Add($"  {type}: {count}");
                }

                var queue = ArmyManager.Instance.GetTrainingQueue();
                if (queue.Count > 0)
                {
                    lines.Add("  --- Training ---");
                    foreach (var entry in queue)
                        lines.Add($"  {entry.Data.DisplayName} x{entry.Amount} ({entry.RemainingTime:F0}s) {entry.Progress:P0}");
                }
            }
            lines.Add("");

            lines.Add("<b>Heroes:</b>");
            if (HeroManager.Instance != null)
            {
                lines.Add($"  Count: {HeroManager.Instance.HeroCount}");
                foreach (var h in HeroManager.Instance.AllHeroes)
                    lines.Add($"  [{h.Data.DisplayName}] Lv{h.Level} {h.Data.Rarity} | XP: {h.CurrentXP} ({h.GetXPProgress():P0})");
            }
            lines.Add("");

            lines.Add("<b>Settlers:</b>");
            if (SettlerManager.Instance != null)
            {
                lines.Add($"  Active: {SettlerManager.Instance.ActiveCount}/{SettlerManager.Instance.SettlerCount}");
                lines.Add($"  Population: {SettlerManager.Instance.TotalPopulation}");
            }
            lines.Add("");

            lines.Add("<b>Grid:</b>");
            if (GridSystem.Instance != null)
            {
                var coords = GridSystem.Instance.GetGridCoordinates(UnityEngine.Camera.main != null ? UnityEngine.Camera.main.transform.position : Vector3.zero);
                lines.Add($"  Size: {GridSystem.Instance.Width}x{GridSystem.Instance.Height}");
                lines.Add($"  Cam Grid: ({coords.x}, {coords.y})");
            }

            lines.Add("");
            lines.Add($"{ColGray}F12 = Debug | ESC = Pause{ColEnd}");

            _debugText.text = string.Join("\n", lines);
        }

        private void RefreshBuildings()
        {
            var lines = new List<string>
            {
                $"{ColGold}--- BUILDINGS ---{ColEnd}",
                ""
            };

            if (BuildingManager.Instance != null)
            {
                lines.Add($"<b>Total: {BuildingManager.Instance.AllBuildings.Count} | CC Level: {BuildingManager.Instance.GetCommandCenterLevel()}</b>");
                lines.Add($"<b>Pop Cap: {BuildingManager.Instance.TotalPopulationCapacity} | Storage: {BuildingManager.Instance.TotalStorageCapacity}</b>");
                lines.Add("");

                var byState = new Dictionary<BuildingState, int>();
                var byType = new Dictionary<string, int>();

                foreach (var b in BuildingManager.Instance.AllBuildings)
                {
                    if (b == null) continue;

                    if (!byState.ContainsKey(b.State)) byState[b.State] = 0;
                    byState[b.State]++;

                    string key = b.Data.DisplayName;
                    if (!byType.ContainsKey(key)) byType[key] = 0;
                    byType[key]++;

                    string prod = b.Data.HasProduction ? $" Prod:{b.GetProductionProgress():P0}" : "";
                    string workers = b.Data.GetTotalRequiredWorkers() > 0 ? $" W:{b.AssignedWorkerCount}/{b.Data.GetTotalRequiredWorkers()}" : "";
                    string roadTag = !b.Data.NeedsRoads ? " [NR]" : "";
                    lines.Add($"  [{b.Data.DisplayName}] Lv{b.Level} {b.State}{prod}{workers}{roadTag}");
                }

                lines.Add("");
                lines.Add("<b>By State:</b>");
                foreach (var kvp in byState)
                    lines.Add($"  {kvp.Key}: {kvp.Value}");

                lines.Add("");
                lines.Add("<b>By Type:</b>");
                foreach (var kvp in byType)
                    lines.Add($"  {kvp.Key}: {kvp.Value}");
            }

            lines.Add("");
            lines.Add("<b>Roads:</b>");
            if (RoadManager.Instance != null)
            {
                lines.Add($"  Total tiles: {RoadManager.Instance.GetAllRoadCells().Count}");
                lines.Add($"  Has roads: {RoadManager.Instance.HasRoads}");
            }

            lines.Add("");
            lines.Add("<b>Garden:</b>");
            if (BuildingManager.Instance != null)
            {
                int small = 0, large = 0;
                foreach (var b in BuildingManager.Instance.AllBuildings)
                {
                    if (b.Data.Type == BuildingType.Garden) small++;
                    if (b.Data.Type == BuildingType.GardenLarge) large++;
                }
                lines.Add($"  Small: {small} | Large: {large}");
            }

            lines.Add("");
            lines.Add($"{ColGray}F12 = Debug | ESC = Pause{ColEnd}");

            _debugText.text = string.Join("\n", lines);
        }

        private void RefreshEvents()
        {
            var lines = new List<string>
            {
                $"{ColGold}--- EVENTS ---{ColEnd}",
                $"{ColGray}Last {_eventLog.Count} events{ColEnd}",
                ""
            };

            if (_eventLog.Count == 0)
            {
                lines.Add($"{ColGray}No events yet...{ColEnd}");
            }
            else
            {
                foreach (var entry in _eventLog)
                    lines.Add(entry);
            }

            lines.Add("");
            lines.Add($"{ColGray}F12 = Debug | ESC = Pause{ColEnd}");

            _debugText.text = string.Join("\n", lines);
        }
    }
}
