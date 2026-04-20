using System.Collections.Generic;
using HollowGround.Army;
using TMPro;
using UnityEngine;

namespace HollowGround.UI
{
    public class ArmyPanelUI : MonoBehaviour
    {
        [System.Serializable]
        public class TroopRow
        {
            public TroopType Type;
            public TMP_Text NameText;
            public TMP_Text CountText;
            public TMP_Text PowerText;
        }

        [SerializeField] private List<TroopRow> _rows = new();
        [SerializeField] private TMP_Text _totalTroopsText;
        [SerializeField] private TMP_Text _totalPowerText;
        [SerializeField] private TMP_Text _moraleText;

        private void OnEnable()
        {
            Refresh();
            if (ArmyManager.Instance != null)
                ArmyManager.Instance.OnArmyUpdated += Refresh;
        }

        private void OnDisable()
        {
            if (ArmyManager.Instance != null)
                ArmyManager.Instance.OnArmyUpdated -= Refresh;
        }

        private void Refresh()
        {
            if (ArmyManager.Instance == null) return;

            foreach (var row in _rows)
            {
                int count = ArmyManager.Instance.GetTroopCount(row.Type);

                if (row.NameText != null)
                    row.NameText.text = row.Type.ToString();

                if (row.CountText != null)
                    row.CountText.text = $"x{count}";

                if (row.PowerText != null)
                    row.PowerText.text = count > 0 ? $"Power: {count * 10}" : "-";
            }

            if (_totalTroopsText != null)
                _totalTroopsText.text = $"Total: {ArmyManager.Instance.TotalTroopCount}";

            if (_totalPowerText != null)
                _totalPowerText.text = $"Army Power: {ArmyManager.Instance.CalculateArmyPower()}";

            if (_moraleText != null)
                _moraleText.text = $"Morale: {(ArmyManager.Instance.GetMorale() * 100):F0}%";
        }
    }
}
