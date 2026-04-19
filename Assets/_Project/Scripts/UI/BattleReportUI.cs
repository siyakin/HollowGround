using System.Collections.Generic;
using HollowGround.Army;
using HollowGround.Combat;
using HollowGround.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class BattleReportUI : MonoBehaviour
    {
        [Header("Result")]
        [SerializeField] private TMP_Text _resultTitle;
        [SerializeField] private TMP_Text _targetName;

        [Header("Power")]
        [SerializeField] private TMP_Text _attackerPowerText;
        [SerializeField] private TMP_Text _defenderPowerText;

        [Header("Losses")]
        [SerializeField] private TMP_Text _lossesText;
        [SerializeField] private TMP_Text _survivorsText;

        [Header("Loot")]
        [SerializeField] private TMP_Text _lootText;

        [Header("Expeditions")]
        [SerializeField] private GameObject _expeditionPanel;
        [SerializeField] private TMP_Text _expeditionLabel;
        [SerializeField] private Slider _expeditionProgress;

        [Header("Close")]
        [SerializeField] private Button _closeButton;

        private void OnEnable()
        {
            if (BattleManager.Instance != null)
                BattleManager.Instance.OnBattleCompleted += ShowReport;

            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);
        }

        private void OnDisable()
        {
            if (BattleManager.Instance != null)
                BattleManager.Instance.OnBattleCompleted -= ShowReport;

            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(Close);
        }

        private void Update()
        {
            UpdateExpeditionTracker();
        }

        public void ShowReport(BattleManager.BattleReport report)
        {
            gameObject.SetActive(true);

            if (_resultTitle != null)
                _resultTitle.text = report.Victory ? "<color=green>ZAFER!</color>" : "<color=red>YENİLGİ</color>";

            if (_targetName != null)
                _targetName.text = $"Hedef: {report.TargetName}";

            if (_attackerPowerText != null)
                _attackerPowerText.text = $"Atak Gücü: {report.TotalAttackerPower}";

            if (_defenderPowerText != null)
                _defenderPowerText.text = $"Savunma Gücü: {report.TotalDefenderPower}";

            if (_lossesText != null)
            {
                var parts = new List<string>();
                foreach (var kvp in report.AttackerLosses)
                    if (kvp.Value > 0) parts.Add($"{kvp.Key}: -{kvp.Value}");
                _lossesText.text = parts.Count > 0 ? string.Join("  ", parts) : "Kayıp yok";
            }

            if (_survivorsText != null)
            {
                var parts = new List<string>();
                foreach (var kvp in report.Survivors)
                    if (kvp.Value > 0) parts.Add($"{kvp.Key}: {kvp.Value}");
                _survivorsText.text = parts.Count > 0 ? string.Join("  ", parts) : "Hayatta kalan yok";
            }

            if (_lootText != null)
            {
                if (report.Victory && report.Loot.Count > 0)
                {
                    var parts = new List<string>();
                    foreach (var kvp in report.Loot)
                        parts.Add($"{kvp.Key}: +{kvp.Value}");
                    _lootText.text = string.Join("  ", parts);
                }
                else
                {
                    _lootText.text = report.Victory ? "Ganimet yok" : "-";
                }
            }
        }

        private void UpdateExpeditionTracker()
        {
            if (BattleManager.Instance == null) return;

            var expeditions = BattleManager.Instance.GetExpeditions();
            bool hasActive = expeditions.Count > 0;

            if (_expeditionPanel != null)
                _expeditionPanel.SetActive(hasActive);

            if (!hasActive) return;

            var current = expeditions[0];
            if (_expeditionLabel != null)
                _expeditionLabel.text = $"{current.Name} ({current.RemainingTime:F0}sn)";

            if (_expeditionProgress != null)
                _expeditionProgress.value = current.Progress;
        }

        private void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
