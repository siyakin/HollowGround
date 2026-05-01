using HollowGround.Army;
using HollowGround.Buildings;
using HollowGround.Core;
using HollowGround.Grid;
using HollowGround.NPCs;
using HollowGround.Resources;
using TMPro;
using UnityEngine;

namespace HollowGround.UI
{
    public class DebugHUD : MonoBehaviour
    {
        [SerializeField] private TMP_Text _debugText;
        [SerializeField] private float _updateInterval = 0.5f;

        private float _timer;

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

            var lines = new System.Collections.Generic.List<string>
            {
                $"<b><color=#FFD700>--- DEBUG HUD ---</color></b>",
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

            lines.Add("<b>Buildings:</b>");
            if (BuildingManager.Instance != null)
            {
                lines.Add($"  Total: {BuildingManager.Instance.AllBuildings.Count}");
                lines.Add($"  CC Level: {BuildingManager.Instance.GetCommandCenterLevel()}");
                lines.Add($"  Pop Cap: {BuildingManager.Instance.TotalPopulationCapacity}");
                lines.Add($"  Storage Cap: {BuildingManager.Instance.TotalStorageCapacity}");

                foreach (var b in BuildingManager.Instance.AllBuildings)
                {
                    if (b == null) continue;
                    string prod = b.Data.HasProduction ? $" | Prod: {b.GetProductionProgress():P0}" : "";
                    lines.Add($"  [{b.Data.DisplayName}] Lv{b.Level} {b.State}{prod}");
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
                lines.Add($"  Cell: {GridSystem.Instance.CellSize}");
                lines.Add($"  Cam Grid: ({coords.x}, {coords.y})");
            }

            lines.Add("");
            lines.Add("<color=#888888>F1/F2/F3 = Speed | F12 = Debug | ESC = Pause</color>");

            _debugText.text = string.Join("\n", lines);
        }
    }
}
