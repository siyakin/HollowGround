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
        private static readonly Color PanelBg = new(0.08f, 0.09f, 0.11f, 0.92f);
        private static readonly Color RowBg = new(0.14f, 0.15f, 0.17f, 1f);
        private static readonly Color ColorOk = new(0.35f, 0.8f, 0.4f, 1f);
        private static readonly Color ColorWarn = new(0.95f, 0.55f, 0.2f, 1f);
        private static readonly Color ColorDanger = new(0.9f, 0.3f, 0.3f, 1f);
        private static readonly Color ColorText = new(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color ColorMuted = new(0.65f, 0.65f, 0.7f, 1f);
        private static readonly Color ColorGold = new(1f, 0.85f, 0.3f, 1f);

        private readonly List<TroopRow> _rows = new();
        private TMP_Text _statusText;
        private TMP_Text _armySummaryText;
        private TMP_FontAsset _themeFont;
        private bool _built;

        private class TroopRow
        {
            public TroopData Data;
            public Button Button;
            public TMP_Text Label;
            public TMP_Text CostLabel;
            public TMP_Text CountLabel;
        }

        private TMP_FontAsset ThemeFont
        {
            get
            {
                if (_themeFont != null) return _themeFont;
#if UNITY_EDITOR
                var theme = UnityEditor.AssetDatabase.LoadAssetAtPath<UIThemeSO>("Assets/_Project/ScriptableObjects/UITheme.asset");
#else
                var theme = UnityEngine.Resources.LoadAll<UIThemeSO>("").Length > 0
                    ? UnityEngine.Resources.LoadAll<UIThemeSO>("")[0] : null;
#endif
                if (theme != null && theme.defaultFont != null)
                    _themeFont = theme.defaultFont;
                return _themeFont;
            }
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

            var bg = gameObject.AddComponent<Image>();
            bg.color = PanelBg;
            bg.raycastTarget = true;

            var cg = gameObject.AddComponent<CanvasGroup>();
            cg.interactable = true;
            cg.blocksRaycasts = true;

            var vlg = gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 15, 15);
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var headerText = AddThemedText(transform, "ARMY TRAINING", 26, ColorGold);
            headerText.alignment = TextAlignmentOptions.Center;
            var headerLE = headerText.gameObject.AddComponent<LayoutElement>();
            headerLE.preferredHeight = 40;

            var allTroops = UnityEngine.Resources.LoadAll<TroopData>("Troops").ToList();
            Debug.Log($"[TrainingPanelUI] Found {allTroops.Count} troop types");

            foreach (var troop in allTroops)
                BuildRow(transform, troop);

            _armySummaryText = AddThemedText(transform, "", 18, ColorOk);
            _armySummaryText.alignment = TextAlignmentOptions.Center;
            var summaryLE = _armySummaryText.gameObject.AddComponent<LayoutElement>();
            summaryLE.preferredHeight = 30;

            _statusText = AddThemedText(transform, "No active training", 16, ColorMuted);
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
            bg.color = RowBg;

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(15, 15, 8, 8);
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;

            var nameLabel = AddThemedText(row.transform, troop.DisplayName, 18, ColorText);
            nameLabel.alignment = TextAlignmentOptions.MidlineLeft;
            var nameLE = nameLabel.gameObject.AddComponent<LayoutElement>();
            nameLE.minWidth = 120;
            nameLE.preferredWidth = 160;

            var cost = troop.GetTrainingCost();
            var parts = new List<string>();
            foreach (var kvp in cost) parts.Add($"{kvp.Key}:{kvp.Value}");
            var costLabel = AddThemedText(row.transform, string.Join(" ", parts), 15, ColorMuted);
            costLabel.alignment = TextAlignmentOptions.MidlineLeft;
            var costLE = costLabel.gameObject.AddComponent<LayoutElement>();
            costLE.minWidth = 100;
            costLE.preferredWidth = 180;

            var countLabel = AddThemedText(row.transform, "x0", 20, ColorText);
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
            btnImg.color = ColorOk;
            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            var btnLabel = AddThemedText(btnObj.transform, "TRAIN", 17, Color.black);
            btnLabel.alignment = TextAlignmentOptions.Center;
            StretchFull(btnLabel.rectTransform);

            btn.onClick.AddListener(() => TrainTroop(troop));

            _rows.Add(new TroopRow { Data = troop, Button = btn, Label = countLabel, CostLabel = costLabel });
        }

        private void TrainTroop(TroopData troop)
        {
            if (ArmyManager.Instance == null)
            {
                ToastUI.Show("Army system not available!", Color.red);
                return;
            }

            if (BuildingManager.Instance == null || BuildingManager.Instance.GetCommandCenterLevel() < 1)
            {
                ToastUI.Show("Build a Command Center first!", ColorWarn);
                return;
            }

            var barracks = BuildingManager.Instance.GetBuildingsOfType(BuildingType.Barracks);
            if (barracks == null || barracks.Count == 0)
            {
                ToastUI.Show("Build a Barracks first!", ColorWarn);
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
                ToastUI.Show($"Need Barracks Lv.{troop.BarracksLevelRequired}!", ColorWarn);
                return;
            }

            if (ArmyManager.Instance.GetTrainingQueue().Count >= 3)
            {
                ToastUI.Show("Training queue full! (max 3)", ColorWarn);
                return;
            }

            if (!ArmyManager.Instance.CanAffordTraining(troop, 1))
            {
                ToastUI.Show("Not enough resources!", ColorDanger);
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
            ToastUI.Show($"{entry.Data.DisplayName} training started", ColorOk);
            RefreshAll();
        }

        private void HandleTrainingCompleted(ArmyManager.TrainingQueueEntry entry)
        {
            ToastUI.Show($"{entry.Data.DisplayName} ready!", ColorOk);
            RefreshAll();
        }

        private TMP_Text AddThemedText(Transform parent, string text, float size, Color color)
        {
            var go = new GameObject("T", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = color;
            tmp.raycastTarget = false;
            if (ThemeFont != null) tmp.font = ThemeFont;
            return tmp;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
