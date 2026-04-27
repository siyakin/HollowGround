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
        private readonly List<TroopRow> _rows = new();
        private TMP_Text _statusText;
        private TMP_Text _armySummaryText;
        private bool _built;

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
            if (!_built) BuildUI();
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

        private void BuildUI()
        {
            var root = GetComponent<RectTransform>();
            if (root == null) return;

            root.anchorMin = new Vector2(0f, 0f);
            root.anchorMax = new Vector2(1f, 1f);
            root.offsetMin = new Vector2(0f, 60f);
            root.offsetMax = new Vector2(0f, 0f);

            foreach (Transform child in root)
                Destroy(child.gameObject);

            var oldVlg = GetComponent<VerticalLayoutGroup>();
            if (oldVlg != null) DestroyImmediate(oldVlg);

            var oldImages = GetComponents<UnityEngine.UI.Image>();
            foreach (var img in oldImages)
                DestroyImmediate(img);

            var oldCg = GetComponent<CanvasGroup>();
            if (oldCg != null) DestroyImmediate(oldCg);

            UIPrimitiveFactory.SetupPanelBackground(gameObject, UIColors.Default);

            var vlg = UIPrimitiveFactory.AddStandardVLG(gameObject);

            var headerText = UIPrimitiveFactory.AddThemedText(transform, "ARMY TRAINING", 26, UIColors.Default.Gold);
            headerText.alignment = TextAlignmentOptions.Center;
            var headerLE = headerText.gameObject.AddComponent<LayoutElement>();
            headerLE.preferredHeight = 40;

            var allTroops = UnityEngine.Resources.LoadAll<TroopData>("Troops").ToList();

            foreach (var troop in allTroops)
                BuildRow(transform, troop);

            _armySummaryText = UIPrimitiveFactory.AddThemedText(transform, "", 18, UIColors.Default.Ok);
            _armySummaryText.alignment = TextAlignmentOptions.Center;
            var summaryLE = _armySummaryText.gameObject.AddComponent<LayoutElement>();
            summaryLE.preferredHeight = 30;

            _statusText = UIPrimitiveFactory.AddThemedText(transform, "No active training", 16, UIColors.Default.Muted);
            _statusText.alignment = TextAlignmentOptions.Center;
            var statusLE = _statusText.gameObject.AddComponent<LayoutElement>();
            statusLE.preferredHeight = 30;

            _built = true;
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

            var btnObj = new GameObject("TrainBtn", typeof(RectTransform));
            btnObj.transform.SetParent(row.transform, false);
            var btnLE = btnObj.AddComponent<LayoutElement>();
            btnLE.minWidth = 120;
            btnLE.preferredWidth = 140;
            btnLE.minHeight = 42;
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = UIColors.Default.Ok;
            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            var btnLabel = UIPrimitiveFactory.AddThemedText(btnObj.transform, "TRAIN", 17, Color.black);
            btnLabel.alignment = TextAlignmentOptions.Center;
            UIPrimitiveFactory.StretchFull(btnLabel.rectTransform);

            btn.onClick.AddListener(() => TrainTroop(troop));

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
            if (!_built || ArmyManager.Instance == null) return;

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
