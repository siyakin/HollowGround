using System.Collections.Generic;
using System.Linq;
using HollowGround.Army;
using HollowGround.Buildings;
using HollowGround.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class TrainingPanelUI : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private Transform _rowsContainer;
        [SerializeField] private TMP_Text _armySummaryText;
        [SerializeField] private TMP_Text _statusText;

        private readonly List<TroopRow> _rows = new();

        private class TroopRow
        {
            public TroopData Data;
            public Button Button;
            public TMP_Text Label;
            public TMP_Text CostLabel;
            public TMP_Text CountLabel;
        }

        private void OnEnable()
        {
            BuildRows();
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
            if (ArmyManager.Instance == null || _statusText == null) return;

            var queue = ArmyManager.Instance.GetTrainingQueue();
            if (queue.Count > 0)
            {
                var c = queue[0];
                _statusText.text = $"Training: {c.Data.DisplayName} x{c.Amount} ({c.RemainingTime:F0}s)";
            }
            else
            {
                _statusText.text = "No active training";
            }
        }

        private void BuildRows()
        {
            if (_rowsContainer == null) return;
            _rows.Clear();

            foreach (Transform child in _rowsContainer)
                Destroy(child.gameObject);

            var allTroops = UnityEngine.Resources.LoadAll<TroopData>("Troops").ToList();

            foreach (var troop in allTroops)
                BuildRow(_rowsContainer, troop);
        }

        private void BuildRow(Transform parent, TroopData troop)
        {
            var row = new GameObject($"Row_{troop.name}", typeof(RectTransform));
            row.transform.SetParent(parent, false);

            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 60;
            le.minHeight = 60;

            var bg = row.AddComponent<Image>();
            bg.color = UIColors.Default.RowBg;

            var hlg = UIPrimitiveFactory.AddRowHLG(row);

            var nameLabel = UIPrimitiveFactory.AddThemedText(row.transform, troop.DisplayName, 18, UIColors.Default.Text);
            nameLabel.alignment = TextAlignmentOptions.MidlineLeft;
            var nameLE = nameLabel.gameObject.AddComponent<LayoutElement>();
            nameLE.minWidth = 120;
            nameLE.preferredWidth = 160;

            var cost = troop.GetTrainingCost();
            var parts = new List<string>();
            foreach (var kvp in cost) parts.Add($"{kvp.Key}:{kvp.Value}");
            var costLabel = UIPrimitiveFactory.AddThemedText(row.transform, string.Join(" ", parts), 15, UIColors.Default.Muted);
            costLabel.alignment = TextAlignmentOptions.MidlineLeft;
            var costLE = costLabel.gameObject.AddComponent<LayoutElement>();
            costLE.minWidth = 100;
            costLE.preferredWidth = 180;

            var countLabel = UIPrimitiveFactory.AddThemedText(row.transform, "x0", 20, UIColors.Default.Text);
            countLabel.alignment = TextAlignmentOptions.Center;
            var countLE = countLabel.gameObject.AddComponent<LayoutElement>();
            countLE.minWidth = 60;
            countLE.preferredWidth = 80;

            var btn = UIPrimitiveFactory.CreateThemedButton(row.transform, "TrainBtn", "TRAIN", () => TrainTroop(troop), UIStyleType.ConfirmButton);
            var btnLE = btn.gameObject.AddComponent<LayoutElement>();
            btnLE.minWidth = 120;
            btnLE.preferredWidth = 140;
            btnLE.minHeight = 42;

            _rows.Add(new TroopRow { Data = troop, Button = btn, Label = countLabel, CostLabel = costLabel });
        }

        private void TrainTroop(TroopData troop)
        {
            if (ArmyManager.Instance == null)
            {
                ToastUI.Show("Army system not available!", UIColors.Default.Danger);
                return;
            }

            if (BuildingManager.Instance == null || BuildingManager.Instance.GetCommandCenterLevel() < 1)
            {
                ToastUI.Show("Build a Command Center first!", UIColors.Default.Warn);
                return;
            }

            var barracks = BuildingManager.Instance.GetBuildingsOfType(BuildingType.Barracks);
            if (barracks == null || barracks.Count == 0)
            {
                ToastUI.Show("Build a Barracks first!", UIColors.Default.Warn);
                return;
            }

            bool hasActive = false;
            foreach (var b in barracks)
            {
                if (b.State == BuildingState.Active && b.Level >= troop.BarracksLevelRequired)
                { hasActive = true; break; }
            }
            if (!hasActive)
            {
                ToastUI.Show($"Need Barracks Lv.{troop.BarracksLevelRequired}!", UIColors.Default.Warn);
                return;
            }

            if (ArmyManager.Instance.GetTrainingQueue().Count >= 3)
            {
                ToastUI.Show("Training queue full! (max 3)", UIColors.Default.Warn);
                return;
            }

            if (!ArmyManager.Instance.CanAffordTraining(troop, 1))
            {
                ToastUI.Show("Not enough resources!", UIColors.Default.Danger);
                return;
            }

            ArmyManager.Instance.StartTraining(troop, 1);
        }

        private void RefreshAll()
        {
            if (ArmyManager.Instance == null) return;

            foreach (var row in _rows)
            {
                if (row.Data == null) continue;
                int count = ArmyManager.Instance.GetTroopCount(row.Data.Type);
                if (row.Label != null) row.Label.text = $"x{count}";
                if (row.Button != null)
                    row.Button.interactable = ArmyManager.Instance.CanAffordTraining(row.Data, 1);
            }

            if (_armySummaryText != null)
            {
                int total = ArmyManager.Instance.TotalTroopCount;
                int power = ArmyManager.Instance.CalculateArmyPower();
                float morale = ArmyManager.Instance.GetMorale() * 100f;
                _armySummaryText.text = $"Army: {total} troops | Power: {power} | Morale: {morale:F0}%";
            }
        }

        private void HandleTrainingStarted(ArmyManager.TrainingQueueEntry entry)
        {
            ToastUI.Show($"{entry.Data.DisplayName} training started", UIColors.Default.Ok);
            RefreshAll();
        }

        private void HandleTrainingCompleted(ArmyManager.TrainingQueueEntry entry)
        {
            ToastUI.Show($"{entry.Data.DisplayName} ready!", UIColors.Default.Ok);
            RefreshAll();
        }
    }
}
