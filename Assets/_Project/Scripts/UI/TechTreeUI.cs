using System.Collections.Generic;
using HollowGround.Tech;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class TechTreeUI : MonoBehaviour
    {
        [System.Serializable]
        public class TechCard
        {
            public TechNode Data;
            public Button Button;
            public TMP_Text NameText;
            public TMP_Text DescText;
            public TMP_Text CostText;
            public TMP_Text StatusText;
            public Image IconImage;
            public GameObject LockedOverlay;
            public Slider ProgressSlider;
        }

        [SerializeField] private List<TechCard> _techCards = new();
        [SerializeField] private GameObject _detailPanel;
        [SerializeField] private TMP_Text _detailName;
        [SerializeField] private TMP_Text _detailDesc;
        [SerializeField] private TMP_Text _detailCost;
        [SerializeField] private TMP_Text _detailBonuses;
        [SerializeField] private Button _detailResearchBtn;
        [SerializeField] private TMP_Text _detailResearchBtnText;

        private TechNode _selectedTech;

        private void OnEnable()
        {
            if (ResearchManager.Instance != null)
            {
                ResearchManager.Instance.OnResearchStarted += HandleResearchStarted;
                ResearchManager.Instance.OnResearchCompleted += HandleResearchCompleted;
                ResearchManager.Instance.OnResearchProgressChanged += HandleProgressChanged;
            }
            RefreshAll();
        }

        private void OnDisable()
        {
            if (ResearchManager.Instance != null)
            {
                ResearchManager.Instance.OnResearchStarted -= HandleResearchStarted;
                ResearchManager.Instance.OnResearchCompleted -= HandleResearchCompleted;
                ResearchManager.Instance.OnResearchProgressChanged -= HandleProgressChanged;
            }
        }

        private void Update()
        {
            if (ResearchManager.Instance != null && ResearchManager.Instance.IsResearching)
            {
                foreach (var card in _techCards)
                {
                    if (card.Data != null && card.Data.IsResearching && card.ProgressSlider != null)
                        card.ProgressSlider.value = card.Data.ResearchProgress;
                }
            }
        }

        public void RefreshAll()
        {
            foreach (var card in _techCards)
            {
                if (card.Data == null) continue;

                bool canResearch = ResearchManager.Instance != null &&
                                   ResearchManager.Instance.CanStartResearch(card.Data);
                bool isResearched = card.Data.IsResearched;
                bool isResearching = card.Data.IsResearching;

                if (card.NameText != null)
                    card.NameText.text = card.Data.DisplayName;

                if (card.DescText != null)
                    card.DescText.text = card.Data.Description;

                if (card.CostText != null)
                {
                    var costs = card.Data.GetCost();
                    var parts = new List<string>();
                    foreach (var kvp in costs)
                        parts.Add($"{kvp.Key}: {kvp.Value}");
                    card.CostText.text = string.Join("  ", parts);
                }

                if (card.StatusText != null)
                {
                    if (isResearched)
                        card.StatusText.text = "Completed";
                    else if (isResearching)
                        card.StatusText.text = $"Researching {card.Data.ResearchProgress:P0}";
                    else if (canResearch)
                        card.StatusText.text = "Available";
                    else
                        card.StatusText.text = "Locked";
                }

                if (card.Button != null)
                    card.Button.interactable = canResearch || isResearching;

                if (card.LockedOverlay != null)
                    card.LockedOverlay.SetActive(!card.Data.CanResearch() && !isResearched);

                if (card.ProgressSlider != null)
                {
                    card.ProgressSlider.gameObject.SetActive(isResearching);
                    card.ProgressSlider.value = card.Data.ResearchProgress;
                }
            }
        }

        public void SelectTech(int cardIndex)
        {
            if (cardIndex < 0 || cardIndex >= _techCards.Count) return;

            _selectedTech = _techCards[cardIndex].Data;
            ShowDetail(_selectedTech);
        }

        private void ShowDetail(TechNode node)
        {
            if (_detailPanel == null || node == null) return;

            _detailPanel.SetActive(true);

            if (_detailName != null)
                _detailName.text = node.DisplayName;

            if (_detailDesc != null)
                _detailDesc.text = node.Description;

            if (_detailCost != null)
            {
                var costs = node.GetCost();
                var parts = new List<string>();
                foreach (var kvp in costs)
                    parts.Add($"{kvp.Key}: {kvp.Value}");
                _detailCost.text = parts.Count > 0 ? string.Join("\n", parts) : "Free";
            }

            if (_detailBonuses != null)
            {
                var bonuses = new List<string>();
                if (node.ProductionBonus > 0) bonuses.Add($"Production +{node.ProductionBonus:P0}");
                if (node.TrainingSpeedBonus > 0) bonuses.Add($"Training Speed +{node.TrainingSpeedBonus:P0}");
                if (node.ExpeditionSpeedBonus > 0) bonuses.Add($"Expedition Speed +{node.ExpeditionSpeedBonus:P0}");
                if (node.DefenseBonus > 0) bonuses.Add($"Defense +{node.DefenseBonus:P0}");
                _detailBonuses.text = bonuses.Count > 0 ? string.Join("\n", bonuses) : "No bonuses";
            }

            if (_detailResearchBtn != null)
            {
                bool canStart = ResearchManager.Instance != null &&
                                ResearchManager.Instance.CanStartResearch(node);
                _detailResearchBtn.interactable = canStart;
            }

            if (_detailResearchBtnText != null)
            {
                if (node.IsResearched) _detailResearchBtnText.text = "Completed";
                else if (node.IsResearching) _detailResearchBtnText.text = "In Progress...";
                else _detailResearchBtnText.text = "Research";
            }
        }

        public void StartResearchFromDetail()
        {
            if (_selectedTech == null) return;
            if (ResearchManager.Instance == null) return;

            if (ResearchManager.Instance.StartResearch(_selectedTech))
                RefreshAll();
        }

        public void CloseDetail()
        {
            if (_detailPanel != null)
                _detailPanel.SetActive(false);
            _selectedTech = null;
        }

        private void HandleResearchStarted(TechNode node) => RefreshAll();
        private void HandleResearchCompleted(TechNode node) => RefreshAll();
        private void HandleProgressChanged(float progress) { }
    }
}
