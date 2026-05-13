using System.Collections.Generic;
using System.Text;
using HollowGround.Army;
using HollowGround.Buildings;
using HollowGround.Combat;
using HollowGround.Core;
using HollowGround.Grid;
using HollowGround.Heroes;
using HollowGround.NPCs;
using HollowGround.Quests;
using HollowGround.Resources;
using HollowGround.Roads;
using HollowGround.Tech;
using HollowGround.World;
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
            public ResourceCostDisplay CostDisplay;
            public GameObject LockedOverlay;

            [System.NonSerialized] private ThemedButton _themedBtn;
            [System.NonSerialized] private TooltipTrigger _tooltipTrigger;

            public ThemedButton ThemedBtn
            {
                get
                {
                    if (_themedBtn == null && Button != null)
                        _themedBtn = Button.GetComponent<ThemedButton>() ?? Button.gameObject.AddComponent<ThemedButton>();
                    return _themedBtn;
                }
            }

            public TooltipTrigger Tooltip
            {
                get
                {
                    if (_tooltipTrigger == null && Button != null)
                    {
                        _tooltipTrigger = Button.GetComponent<TooltipTrigger>();
                        if (_tooltipTrigger == null)
                            _tooltipTrigger = Button.gameObject.AddComponent<TooltipTrigger>();
                    }
                    return _tooltipTrigger;
                }
            }
        }

        [SerializeField] private BuildingPlacer _buildingPlacer;
        [SerializeField] private List<BuildingCard> _cards = new();
        [SerializeField] private BuildingCategory _currentCategory = BuildingCategory.Resource;
        [SerializeField] private Transform _cardContainer;

        private float _refreshTimer;
        private bool _pendingFirstRefresh;

        private void Update()
        {
            if (_pendingFirstRefresh)
            {
                _pendingFirstRefresh = false;
                _refreshTimer = 0f;
                RefreshCards();
                return;
            }

            _refreshTimer += Time.deltaTime;
            if (_refreshTimer >= 1f)
            {
                _refreshTimer = 0f;
                RefreshCards();
            }
        }

        private void OnEnable()
        {
            SubscribeEvents();
            SetAllCardsLoading();
            _pendingFirstRefresh = true;
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.OnBuildingAdded += HandleBuildingChanged;
                BuildingManager.Instance.OnCommandCenterLevelChanged += HandleCCLevelChanged;
            }
        }

        private void UnsubscribeEvents()
        {
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.OnBuildingAdded -= HandleBuildingChanged;
                BuildingManager.Instance.OnCommandCenterLevelChanged -= HandleCCLevelChanged;
            }
        }

        private void SetAllCardsLoading()
        {
            foreach (var card in _cards)
            {
                if (card.Data == null) continue;
                if (card.Button != null)
                    card.Button.interactable = false;
                if (card.NameText != null)
                {
                    card.NameText.text = card.Data.DisplayName;
                    card.NameText.color = UIColors.Default.Muted;
                }
                if (card.CostDisplay != null)
                    card.CostDisplay.Clear();
                if (card.LockedOverlay != null)
                    card.LockedOverlay.SetActive(false);
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
            if (BuildingManager.Instance == null || ResourceManager.Instance == null)
                return;

            foreach (var card in _cards)
            {
                if (card.Data == null) continue;

                int ccLevel = BuildingManager.Instance != null ? BuildingManager.Instance.GetCommandCenterLevel() : 0;
                bool ccUnlocked = card.Data.CommandCenterLevelRequired <= ccLevel;
                bool hasResources = HasEnoughResources(card.Data);

                if (card.Button != null)
                {
                    card.Button.gameObject.SetActive(true);
                    card.Button.interactable = ccUnlocked && hasResources;

                    var tb = card.ThemedBtn;
                    if (tb != null && tb.styleType != UIStyleType.BuildingCardButton)
                        tb.styleType = UIStyleType.BuildingCardButton;
                    else if (tb != null)
                        tb.ApplyStyle();
                }

                if (card.NameText != null)
                {
                    card.NameText.text = card.Data.DisplayName;
                    card.NameText.color = ccUnlocked ? (hasResources ? UIColors.Default.Text : UIColors.Default.Warn) : UIColors.Default.Muted;
                }

                if (card.CostDisplay != null)
                {
                    var costs = card.Data.GetCostForLevel(1);
                    if (costs.Count > 0)
                    {
                        Dictionary<ResourceType, int> have = null;
                        if (ResourceManager.Instance != null)
                        {
                            have = new Dictionary<ResourceType, int>();
                            foreach (var kvp in costs)
                                have[kvp.Key] = ResourceManager.Instance.Get(kvp.Key);
                        }
                        card.CostDisplay.SetCosts(costs, have);
                    }
                    else
                    {
                        card.CostDisplay.Clear();
                    }
                }

                if (card.LockedOverlay != null)
                    card.LockedOverlay.SetActive(!ccUnlocked);

                UpdateCardTooltip(card);
            }
        }

        private bool HasEnoughResources(BuildingData data)
        {
            if (ResourceManager.Instance == null) return true;
            var costs = data.GetCostForLevel(1);
            if (costs.Count == 0) return true;
            return ResourceManager.Instance.CanAfford(costs);
        }

        private void UpdateCardTooltip(BuildingCard card)
        {
            if (card.Data == null || card.Tooltip == null) return;

            var cfg = Core.GameConfig.Instance;
            bool enabled = cfg == null || cfg.TooltipBuildMenu;

            if (!enabled)
            {
                card.Tooltip.Clear();
                return;
            }

            var captured = card.Data;
            card.Tooltip.SetProvider(() => TooltipContentBuilder.ForBuildingData(captured));
        }

        public void SelectBuilding(int cardIndex)
        {
            if (cardIndex < 0 || cardIndex >= _cards.Count) return;

            BuildingCard card = _cards[cardIndex];
            if (card.Data == null) return;

            int ccLevel = BuildingManager.Instance != null ? BuildingManager.Instance.GetCommandCenterLevel() : 0;
            if (card.Data.CommandCenterLevelRequired > ccLevel)
            {
                ToastUI.Show($"Need Command Center Lv.{card.Data.CommandCenterLevelRequired}! (Current: Lv.{ccLevel})", UIColors.Default.Danger);
                return;
            }

            if (!HasEnoughResources(card.Data))
            {
                ShowMissingResources(card.Data);
                return;
            }

            if (_buildingPlacer == null)
                _buildingPlacer = FindAnyObjectByType<BuildingPlacer>();

            if (_buildingPlacer != null)
            {
                _buildingPlacer.StartPlacement(card.Data);
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
