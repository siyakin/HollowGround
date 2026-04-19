using System.Collections.Generic;
using HollowGround.Army;
using HollowGround.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class TrainingPanelUI : MonoBehaviour
    {
        [System.Serializable]
        public class TroopSlot
        {
            public TroopData Data;
            public Button TrainButton;
            public TMP_Text NameText;
            public TMP_Text CostText;
            public TMP_Text TimeText;
            public TMP_Text CountText;
        }

        [SerializeField] private List<TroopSlot> _troopSlots = new();
        [SerializeField] private Slider _trainingProgress;
        [SerializeField] private TMP_Text _trainingLabel;
        [SerializeField] private TMP_Text _armyPowerText;
        [SerializeField] private TMP_Text _moraleText;

        private void OnEnable()
        {
            RefreshAll();

            if (ArmyManager.Instance != null)
            {
                ArmyManager.Instance.OnArmyUpdated += RefreshAll;
                ArmyManager.Instance.OnTrainingStarted += HandleTrainingStarted;
                ArmyManager.Instance.OnTrainingCompleted += HandleTrainingCompleted;
            }
        }

        private void OnDisable()
        {
            if (ArmyManager.Instance != null)
            {
                ArmyManager.Instance.OnArmyUpdated -= RefreshAll;
                ArmyManager.Instance.OnTrainingStarted -= HandleTrainingStarted;
                ArmyManager.Instance.OnTrainingCompleted -= HandleTrainingCompleted;
            }
        }

        private void Update()
        {
            UpdateTrainingProgress();
        }

        public void TrainTroop(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _troopSlots.Count) return;

            TroopSlot slot = _troopSlots[slotIndex];
            if (slot.Data == null || ArmyManager.Instance == null) return;

            if (!ArmyManager.Instance.CanAffordTraining(slot.Data, 1))
            {
                ToastUI.Show("Yetersiz kaynak!");
                return;
            }

            ArmyManager.Instance.StartTraining(slot.Data, 1);
            RefreshAll();
        }

        private void RefreshAll()
        {
            if (ArmyManager.Instance == null) return;

            foreach (var slot in _troopSlots)
            {
                if (slot.Data == null) continue;

                bool canAfford = ArmyManager.Instance.CanAffordTraining(slot.Data, 1);

                if (slot.TrainButton != null)
                    slot.TrainButton.interactable = canAfford;

                if (slot.NameText != null)
                    slot.NameText.text = slot.Data.DisplayName;

                if (slot.CostText != null)
                {
                    var cost = slot.Data.GetTrainingCost();
                    var parts = new List<string>();
                    foreach (var kvp in cost)
                        parts.Add($"{kvp.Key}: {kvp.Value}");
                    slot.CostText.text = string.Join("  ", parts);
                }

                if (slot.TimeText != null)
                    slot.TimeText.text = $"{slot.Data.TrainingTime:F0}sn";

                if (slot.CountText != null)
                    slot.CountText.text = $"x{ArmyManager.Instance.GetTroopCount(slot.Data.Type)}";
            }

            if (_armyPowerText != null)
                _armyPowerText.text = $"Güç: {ArmyManager.Instance.CalculateArmyPower()}";

            if (_moraleText != null)
                _moraleText.text = $"Moral: %{(ArmyManager.Instance.GetMorale() * 100):F0}";
        }

        private void UpdateTrainingProgress()
        {
            if (ArmyManager.Instance == null) return;

            var queue = ArmyManager.Instance.GetTrainingQueue();
            if (queue.Count > 0)
            {
                var current = queue[0];
                if (_trainingProgress != null)
                    _trainingProgress.value = current.Progress;

                if (_trainingLabel != null)
                    _trainingLabel.text = $"{current.Data.DisplayName} x{current.Amount} ({current.RemainingTime:F0}sn)";
            }
            else
            {
                if (_trainingProgress != null)
                    _trainingProgress.value = 0;

                if (_trainingLabel != null)
                    _trainingLabel.text = "Eğitim yok";
            }
        }

        private void HandleTrainingStarted(ArmyManager.TrainingQueueEntry entry)
        {
            ToastUI.Show($"{entry.Data.DisplayName} x{entry.Amount} eğitimi başladı");
        }

        private void HandleTrainingCompleted(ArmyManager.TrainingQueueEntry entry)
        {
            ToastUI.Show($"{entry.Data.DisplayName} x{entry.Amount} eğitim tamamlandı!");
            RefreshAll();
        }
    }
}
