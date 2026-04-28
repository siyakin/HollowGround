using System.Collections.Generic;
using System.Text;
using HollowGround.Buildings;
using HollowGround.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class BuildMenuUI : MonoBehaviour
    {
        [System.Serializable]
        public class BuildingCard
        {
            public BuildingData Data;
            public Button Button;
            public TMP_Text NameText;
            public TMP_Text CostText;
            public GameObject LockedOverlay;

            private ThemedButton _themedBtn;
            public ThemedButton ThemedBtn
            {
                get
                {
                    if (_themedBtn == null && Button != null)
                        _themedBtn = Button.GetComponent<ThemedButton>() ?? Button.gameObject.AddComponent<ThemedButton>();
                    return _themedBtn;
                }
            }
        }

        [SerializeField] private List<BuildingCard> _cards = new();
        [SerializeField] private BuildingCategory _currentCategory = BuildingCategory.Resource;
        [SerializeField] private Transform _cardContainer;

        private float _refreshTimer;

        private void Update()
        {
            _refreshTimer += Time.deltaTime;
            if (_refreshTimer >= 1f)
            {
                _refreshTimer = 0f;
                RefreshCards();
            }
        }

        private void OnEnable()
        {
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.OnBuildingAdded += HandleBuildingChanged;
                BuildingManager.Instance.OnCommandCenterLevelChanged += HandleCCLevelChanged;
            }
            RefreshCards();
        }

        private void OnDisable()
        {
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.OnBuildingAdded -= HandleBuildingChanged;
                BuildingManager.Instance.OnCommandCenterLevelChanged -= HandleCCLevelChanged;
            }
        }

        private void HandleBuildingChanged(Building _)
        {
            RefreshCards();
        }

        private void HandleCCLevelChanged(int _)
        {
            RefreshCards();
        }

        public void SetCategory(int categoryIndex)
        {
            _currentCategory = (BuildingCategory)categoryIndex;
            RefreshCards();
        }

        public void RefreshCards()
        {
            var theme = UIThemeManager.Instance?.CurrentTheme;
            foreach (var card in _cards)
            {
                if (card.Data == null) continue;

                int ccLevel = BuildingManager.Instance != null ? BuildingManager.Instance.GetCommandCenterLevel() : 0;
                bool ccUnlocked = card.Data.CommandCenterLevelRequired <= ccLevel;
                bool hasResources = HasEnoughResources(card.Data);
                bool enabled = ccUnlocked && hasResources;

                if (card.Button != null)
                {
                    card.Button.gameObject.SetActive(true);
                    card.Button.interactable = enabled;

                    var tb = card.ThemedBtn;
                    if (tb != null && tb.styleType != UIStyleType.BuildingCardButton)
                        tb.styleType = UIStyleType.BuildingCardButton;
                }

                if (card.NameText != null)
                {
                    card.NameText.text = card.Data.DisplayName;
                    card.NameText.color = enabled ? UIColors.Default.Text : UIColors.Default.Muted;
                }

                if (card.CostText != null)
                {
                    var costs = card.Data.GetCostForLevel(1);
                    var parts = new List<string>();
                    foreach (var kvp in costs)
                    {
                        if (kvp.Value <= 0) continue;
                        var resColor = UIColors.GetResourceColor(kvp.Key);
                        int have = ResourceManager.Instance != null ? ResourceManager.Instance.Get(kvp.Key) : 0;
                        bool enough = have >= kvp.Value;
                        string hex = ColorUtility.ToHtmlStringRGBA(enough ? resColor : UIColors.Default.Danger);
                        parts.Add($"<color=#{hex}>\u25CF {kvp.Value}</color>");
                    }
                    card.CostText.text = parts.Count > 0 ? string.Join("  ", parts) : "<color=#C8C8C8>Free</color>";
                    card.CostText.color = enabled ? UIColors.Default.Text : UIColors.Default.Muted;
                }

                if (card.LockedOverlay != null)
                    card.LockedOverlay.SetActive(!ccUnlocked);
            }
        }

        private bool HasEnoughResources(BuildingData data)
        {
            if (ResourceManager.Instance == null) return false;
            var costs = data.GetCostForLevel(1);
            if (costs.Count == 0) return true;
            return ResourceManager.Instance.CanAfford(costs);
        }

        public void SelectBuilding(int cardIndex)
        {
            if (cardIndex < 0 || cardIndex >= _cards.Count) return;

            BuildingCard card = _cards[cardIndex];
            if (card.Data == null) return;

            if (!HasEnoughResources(card.Data))
            {
                ShowMissingResources(card.Data);
                return;
            }

            if (BuildingManager.Instance != null && !BuildingManager.Instance.CanBuild(card.Data))
            {
                int ccLevel = BuildingManager.Instance.GetCommandCenterLevel();
                int required = card.Data.CommandCenterLevelRequired;
                ToastUI.Show($"Need Command Center Lv.{required}! (Current: Lv.{ccLevel})", UIColors.Default.Danger);
                return;
            }

            if (BuildingPlacer.Instance != null)
            {
                BuildingPlacer.Instance.StartPlacement(card.Data);
                if (UIManager.Instance != null)
                    UIManager.Instance.ToggleBuildMenu();
                else
                    gameObject.SetActive(false);
            }
        }

        private void ShowMissingResources(BuildingData data)
        {
            if (ResourceManager.Instance == null) return;
            var costs = data.GetCostForLevel(1);
            var sb = new StringBuilder();
            sb.Append($"Not enough resources for {data.DisplayName}: ");
            bool first = true;
            foreach (var kvp in costs)
            {
                int have = ResourceManager.Instance.Get(kvp.Key);
                if (have < kvp.Value)
                {
                    if (!first) sb.Append(", ");
                    sb.Append($"{kvp.Key} {kvp.Value - have} short");
                    first = false;
                }
            }
            ToastUI.Show(sb.ToString(), UIColors.Default.Danger);
        }
    }
}
