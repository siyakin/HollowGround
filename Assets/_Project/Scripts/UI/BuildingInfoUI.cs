using HollowGround.Core;
using System.Collections.Generic;
using System.Linq;
using HollowGround.Buildings;
using HollowGround.Grid;
using HollowGround.NPCs;
using HollowGround.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class BuildingInfoUI : MonoBehaviour
    {
        [Header("Info")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _stateText;
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private TMP_Text _progressLabel;

        [Header("Production")]
        [SerializeField] private GameObject _productionGroup;
        [SerializeField] private TMP_Text _productionText;
        [SerializeField] private Slider _productionSlider;

        [Header("Actions")]
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private TMP_Text _upgradeCostText;
        [SerializeField] private Button _demolishButton;
        [SerializeField] private Button _repairButton;
        [SerializeField] private TMP_Text _repairCostText;

        [Header("Workers")]
        [SerializeField] private TMP_Text _workersText;

        private Building _current;
        private UnityEngine.Camera _cam;
        private const float ActionBarHeight = 70f;
        private const float TopBarHeight = 50f;
        private const float ScreenMargin = 15f;

        private void Start()
        {
            SubscribeSelector();
        }

        private void OnEnable()
        {
            SubscribeSelector();

            if (_upgradeButton != null)
                _upgradeButton.onClick.AddListener(OnUpgradeClicked);

            if (_demolishButton != null)
                _demolishButton.onClick.AddListener(OnDemolishClicked);

            if (_repairButton != null)
                _repairButton.onClick.AddListener(OnRepairClicked);
        }

        private void OnDisable()
        {
            if (_upgradeButton != null)
                _upgradeButton.onClick.RemoveListener(OnUpgradeClicked);

            if (_demolishButton != null)
                _demolishButton.onClick.RemoveListener(OnDemolishClicked);

            if (_repairButton != null)
                _repairButton.onClick.RemoveListener(OnRepairClicked);
        }

        private void OnDestroy()
        {
            UnsubscribeSelector();
        }

        private void SubscribeSelector()
        {
            BuildingSelector selector = FindAnyObjectByType<BuildingSelector>();
            if (selector != null)
            {
                selector.OnBuildingSelected -= ShowInfo;
                selector.OnBuildingDeselected -= HideInfo;
                selector.OnBuildingSelected += ShowInfo;
                selector.OnBuildingDeselected += HideInfo;
            }
        }

        private void UnsubscribeSelector()
        {
            BuildingSelector selector = FindAnyObjectByType<BuildingSelector>();
            if (selector != null)
            {
                selector.OnBuildingSelected -= ShowInfo;
                selector.OnBuildingDeselected -= HideInfo;
            }
        }

        private void Update()
        {
            if (_current == null) return;
            UpdateProgress();
            UpdateProductionProgress();
            UpdateWorkersInfo();

            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.dKey.wasPressedThisFrame)
            {
                OnDemolishClicked();
            }
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.uKey.wasPressedThisFrame)
            {
                OnUpgradeClicked();
            }
        }

        public void ShowInfo(Building building)
        {
            _current = building;
            gameObject.SetActive(true);
            SmartPosition(building);
            RefreshDisplay();
        }

        public void HideInfo()
        {
            _current = null;
            gameObject.SetActive(false);
        }

        private void RefreshDisplay()
        {
            if (_current == null) return;

            if (_nameText != null)
                _nameText.text = _current.Data.DisplayName;

            if (_levelText != null)
                _levelText.text = $"Level {_current.Level}";

            if (_stateText != null)
            {
                _stateText.text = _current.State.ToString();
                _stateText.color = UIColors.GetStateColor(_current.State);
            }

            if (_productionGroup != null)
                _productionGroup.SetActive(_current.Data.HasProduction);

            if (_productionText != null && _current.Data.HasProduction)
            {
                int prod = _current.Data.GetProductionForLevel(_current.Level);
                _productionText.text = $"{prod} {_current.Data.ProducedResource}";
            }

            UpdateUpgradeButton();
            UpdateRepairButton();
            UpdateWorkersInfo();
        }

        private void UpdateProgress()
        {
            if (_progressSlider == null) return;

            float progress = _current.State switch
            {
                BuildingState.Constructing => _current.ConstructionProgress,
                BuildingState.Upgrading => _current.UpgradeProgress,
                _ => 0f
            };

            _progressSlider.value = progress;

            if (_progressLabel != null)
            {
                _progressLabel.gameObject.SetActive(progress > 0f && progress < 1f);
                _progressLabel.text = $"{(progress * 100):F0}%";
            }
        }

        private void UpdateProductionProgress()
        {
            if (_productionSlider == null || !_current.Data.HasProduction) return;
            _productionSlider.value = _current.GetProductionProgress();
        }

        private void UpdateUpgradeButton()
        {
            if (_upgradeButton == null) return;

            bool canUpgrade = _current.CanUpgrade();
            _upgradeButton.interactable = canUpgrade;

            if (_upgradeCostText != null && canUpgrade)
            {
                var costs = _current.Data.GetCostForLevel(_current.Level + 1);
                var parts = new List<string>();
                foreach (var kvp in costs)
                    parts.Add($"{kvp.Key}: {kvp.Value}");
                _upgradeCostText.text = string.Join("  ", parts);
            }
            else if (_upgradeCostText != null)
            {
                _upgradeCostText.text = "MAX";
            }
        }

        public void OnUpgradeClicked()
        {
            if (_current == null) return;
            if (!_current.CanUpgrade())
            {
                ToastUI.Show("Cannot upgrade right now!", UIColors.Default.Warn);
                return;
            }

            if (!CanAffordUpgrade())
            {
                ShowMissingUpgradeResources();
                return;
            }

            if (_current.StartUpgrade())
                ToastUI.Show($"Upgrading {_current.Data.DisplayName} to Lv.{_current.Level + 1}...", UIColors.Default.Gold);
            RefreshDisplay();
        }

        public void OnDemolishClicked()
        {
            if (_current == null) return;
            string name = _current.Data.DisplayName;
            _current.Demolish();
            ToastUI.Show($"{name} demolished. Resources refunded.", UIColors.Default.Warn);
            HideInfo();
        }

        private void UpdateRepairButton()
        {
            if (_repairButton == null) return;

            bool isDamaged = _current.State == BuildingState.Damaged;
            _repairButton.gameObject.SetActive(isDamaged);

            if (isDamaged && _repairCostText != null)
            {
                var costs = _current.Data.GetCostForLevel(_current.Level);
                var parts = new List<string>();
                foreach (var kvp in costs)
                    parts.Add($"{kvp.Key}: {Mathf.CeilToInt(kvp.Value * (GameConfig.Instance != null ? GameConfig.Instance.RepairCostRatio : 0.5f))}");
                _repairCostText.text = string.Join("  ", parts);
            }
        }

        private void UpdateWorkersInfo()
        {
            if (_workersText == null) return;

            if (_current.Data.RequiredWorkers == null || _current.Data.RequiredWorkers.Count == 0)
            {
                _workersText.gameObject.SetActive(false);
                return;
            }

            _workersText.gameObject.SetActive(true);

            int assigned = _current.AssignedWorkerCount;
            int required = _current.Data.GetTotalRequiredWorkers();

            var jm = SettlerJobManager.Instance;
            if (jm == null)
            {
                _workersText.text = $"Workers: {assigned}/{required}";
                return;
            }

            var lines = new List<string> { $"Workers: {assigned}/{required}" };

            foreach (var slot in _current.Data.RequiredWorkers)
            {
                int count = jm.GetAssignedWorkerCountForRole(_current, slot.Role);
                string roleName = SettlerRoleInfo.GetDisplayName(slot.Role);
                lines.Add($"  {roleName}: {count}/{slot.Count}");
            }

            _workersText.text = string.Join("\n", lines);
        }

        public void OnRepairClicked()
        {
            if (_current == null) return;
            if (_current.Repair())
            {
                ToastUI.Show($"{_current.Data.DisplayName} repaired!", UIColors.Default.Ok);
            }
            else
            {
                ToastUI.Show("Not enough resources to repair!", UIColors.Default.Danger);
            }
            RefreshDisplay();
        }

        private void SmartPosition(Building building)
        {
            if (_cam == null)
                _cam = UnityEngine.Camera.main;
            if (_cam == null) return;

            var rt = GetComponent<RectTransform>();
            if (rt == null) return;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            var canvasRt = canvas.GetComponent<RectTransform>();
            float canvasW = canvasRt.rect.width;
            float canvasH = canvasRt.rect.height;
            float panelW = rt.rect.width;
            float panelH = rt.rect.height;
            float halfPW = panelW * 0.5f;
            float halfPH = panelH * 0.5f;
            float spacing = 20f;

            float cellSize = GridSystem.Instance != null ? GridSystem.Instance.CellSize : 2f;
            float bw = building.Data.SizeX * cellSize;
            float bd = building.Data.SizeZ * cellSize;
            float bh = 5f;
            Vector3 center = building.transform.position + new Vector3(0f, bh * 0.5f, 0f);
            Vector3 ext = new Vector3(bw * 0.5f, bh * 0.5f, bd * 0.5f);

            Vector3[] corners = new Vector3[8];
            int idx = 0;
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sy = -1; sy <= 1; sy += 2)
                    for (int sz = -1; sz <= 1; sz += 2)
                        corners[idx++] = center + Vector3.Scale(new Vector3(sx, sy, sz), ext);

            float minSX = float.MaxValue, maxSX = float.MinValue;
            float minSY = float.MaxValue, maxSY = float.MinValue;
            foreach (var c in corners)
            {
                Vector2 sp = _cam.WorldToScreenPoint(c);
                minSX = Mathf.Min(minSX, sp.x);
                maxSX = Mathf.Max(maxSX, sp.x);
                minSY = Mathf.Min(minSY, sp.y);
                maxSY = Mathf.Max(maxSY, sp.y);
            }

            float screenMidX = (minSX + maxSX) * 0.5f;
            float screenMidY = (minSY + maxSY) * 0.5f;

            Vector2 canvasMid;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, new Vector2(screenMidX, screenMidY), null, out canvasMid);

            Vector2 canvasMin;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, new Vector2(minSX, minSY), null, out canvasMin);
            Vector2 canvasMax;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, new Vector2(maxSX, maxSY), null, out canvasMax);

            float buildingRight = canvasMax.x + spacing;
            float buildingLeft = canvasMin.x - spacing;
            float buildingTop = canvasMax.y + spacing;
            float buildingBottom = canvasMin.y - spacing;

            float safeLeft = -canvasW * 0.5f + spacing;
            float safeRight = canvasW * 0.5f - spacing;
            float safeTop = canvasH * 0.5f - TopBarHeight - spacing;
            float safeBottom = -canvasH * 0.5f + ActionBarHeight + spacing;

            float targetX = buildingRight + halfPW;
            float targetY = canvasMid.y;

            if (targetX + halfPW > safeRight)
                targetX = buildingLeft - halfPW;

            if (targetX - halfPW < safeLeft)
            {
                targetX = canvasMid.x;
                targetY = buildingTop + halfPH;
                if (targetY + halfPH > safeTop)
                    targetY = buildingBottom - halfPH;
            }

            targetY = Mathf.Clamp(targetY, safeBottom + halfPH, safeTop - halfPH);

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(targetX, targetY);
        }

        private bool CanAffordUpgrade()
        {
            if (ResourceManager.Instance == null) return false;
            var costs = _current.Data.GetCostForLevel(_current.Level + 1);
            return ResourceManager.Instance.CanAfford(costs);
        }

        private void ShowMissingUpgradeResources()
        {
            if (ResourceManager.Instance == null) return;
            var costs = _current.Data.GetCostForLevel(_current.Level + 1);
            var sb = new System.Text.StringBuilder();
            sb.Append($"Upgrade needs: ");
            bool first = true;
            foreach (var kvp in costs)
            {
                int have = ResourceManager.Instance.Get(kvp.Key);
                if (have < kvp.Value)
                {
                    if (!first) sb.Append(", ");
                    sb.Append($"{kvp.Key} {kvp.Value - have} more");
                    first = false;
                }
            }
            ToastUI.Show(sb.ToString(), UIColors.Default.Danger);
        }
    }
}
