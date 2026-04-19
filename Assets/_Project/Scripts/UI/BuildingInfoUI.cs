using System.Collections.Generic;
using HollowGround.Buildings;
using HollowGround.Resources;
using TMPro;
using UnityEngine;
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

        private Building _current;

        private void OnEnable()
        {
            BuildingSelector selector = FindAnyObjectByType<BuildingSelector>();
            if (selector != null)
            {
                selector.OnBuildingSelected += ShowInfo;
                selector.OnBuildingDeselected += HideInfo;
            }

            if (_upgradeButton != null)
                _upgradeButton.onClick.AddListener(OnUpgradeClicked);

            if (_demolishButton != null)
                _demolishButton.onClick.AddListener(OnDemolishClicked);
        }

        private void OnDisable()
        {
            BuildingSelector selector = FindAnyObjectByType<BuildingSelector>();
            if (selector != null)
            {
                selector.OnBuildingSelected -= ShowInfo;
                selector.OnBuildingDeselected -= HideInfo;
            }

            if (_upgradeButton != null)
                _upgradeButton.onClick.RemoveListener(OnUpgradeClicked);

            if (_demolishButton != null)
                _demolishButton.onClick.RemoveListener(OnDemolishClicked);
        }

        private void Update()
        {
            if (_current == null) return;
            UpdateProgress();
            UpdateProductionProgress();
        }

        public void ShowInfo(Building building)
        {
            _current = building;
            gameObject.SetActive(true);
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
                _levelText.text = $"Seviye {_current.Level}";

            if (_stateText != null)
                _stateText.text = _current.State.ToString();

            if (_productionGroup != null)
                _productionGroup.SetActive(_current.Data.HasProduction);

            if (_productionText != null && _current.Data.HasProduction)
            {
                int prod = _current.Data.GetProductionForLevel(_current.Level);
                _productionText.text = $"{prod} {_current.Data.ProducedResource}";
            }

            UpdateUpgradeButton();
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

        private void OnUpgradeClicked()
        {
            if (_current == null) return;
            _current.StartUpgrade();
            RefreshDisplay();
        }

        private void OnDemolishClicked()
        {
            if (_current == null) return;
            _current.Demolish();
            HideInfo();
        }
    }
}
