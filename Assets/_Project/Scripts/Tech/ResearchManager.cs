using System.Collections.Generic;
using HollowGround.Core;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.Tech
{
    public class ResearchManager : MonoBehaviour
    {
        public static ResearchManager Instance { get; private set; }

        private TechNode _currentResearch;
        private float _researchTimer;

        public TechNode CurrentResearch => _currentResearch;
        public bool IsResearching => _currentResearch != null;
        public float ResearchProgress => _currentResearch != null ? _currentResearch.ResearchProgress : 0f;

        public event System.Action<TechNode> OnResearchStarted;
        public event System.Action<TechNode> OnResearchCompleted;
        public event System.Action<float> OnResearchProgressChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (_currentResearch == null) return;

            float speedMult = 1f;
            if (TimeManager.Instance != null)
                speedMult = TimeManager.Instance.GameSpeed;
            float devMult = GameConfig.Instance != null ? GameConfig.Instance.GetResearchTimeMultiplier : 1f;
            speedMult /= devMult;

            _researchTimer -= Time.deltaTime * speedMult;
            _currentResearch.ResearchProgress = 1f - (_researchTimer / _currentResearch.ResearchTime);
            OnResearchProgressChanged?.Invoke(_currentResearch.ResearchProgress);

            if (_researchTimer <= 0f)
            {
                CompleteResearch();
            }
        }

        public bool CanStartResearch(TechNode node)
        {
            if (node == null) { Debug.Log("[Research] FAIL: node null"); return false; }
            if (_currentResearch != null) { Debug.Log($"[Research] FAIL: busy with {_currentResearch.DisplayName}"); return false; }
            if (!node.CanResearch()) { Debug.Log($"[Research] FAIL: CanResearch false for {node.DisplayName}"); return false; }

            if (ResourceManager.Instance == null) { Debug.Log("[Research] FAIL: RM null"); return false; }
            bool afford = ResourceManager.Instance.CanAfford(node.GetCost());
            Debug.Log($"[Research] {node.DisplayName} afford={afford}");
            return afford;
        }

        public bool StartResearch(TechNode node)
        {
            if (!CanStartResearch(node)) return false;

            var costs = node.GetCost();
            ResourceManager.Instance.SpendMultiple(costs);

            _currentResearch = node;
            node.IsResearching = true;
            node.ResearchProgress = 0f;
            _researchTimer = node.ResearchTime;

            OnResearchStarted?.Invoke(node);
            return true;
        }

        private void CompleteResearch()
        {
            var node = _currentResearch;
            node.IsResearching = false;
            node.IsResearched = true;
            node.ResearchProgress = 1f;
            _currentResearch = null;

            ApplyBonuses(node);
            OnResearchCompleted?.Invoke(node);
        }

        private void ApplyBonuses(TechNode node)
        {
            Debug.Log($"[Research] Completed '{node.DisplayName}'. " +
                $"Bonuses — Production: +{node.ProductionBonus:P0}, " +
                $"Training: +{node.TrainingSpeedBonus:P0}, " +
                $"Expedition: +{node.ExpeditionSpeedBonus:P0}, " +
                $"Defense: +{node.DefenseBonus:P0}");
        }

        public float GetTotalProductionBonus()
        {
            return SumBonus(t => t.ProductionBonus);
        }

        public float GetTotalTrainingSpeedBonus()
        {
            return SumBonus(t => t.TrainingSpeedBonus);
        }

        public float GetTotalExpeditionSpeedBonus()
        {
            return SumBonus(t => t.ExpeditionSpeedBonus);
        }

        public float GetTotalDefenseBonus()
        {
            return SumBonus(t => t.DefenseBonus);
        }

        private float SumBonus(System.Func<TechNode, float> selector)
        {
            float sum = 0f;
            foreach (var tech in GetResearchedTechs())
                sum += selector(tech);
            return Mathf.Clamp01(sum);
        }

        public List<TechNode> GetAvailableTechs()
        {
            var allTechs = UnityEngine.Resources.LoadAll<TechNode>("TechNodes");
            var available = new List<TechNode>();

            foreach (var tech in allTechs)
            {
                if (tech.CanResearch() && !tech.IsResearched)
                    available.Add(tech);
            }

            return available;
        }

        public List<TechNode> GetResearchedTechs()
        {
            var allTechs = UnityEngine.Resources.LoadAll<TechNode>("TechNodes");
            var researched = new List<TechNode>();

            foreach (var tech in allTechs)
            {
                if (tech.IsResearched)
                    researched.Add(tech);
            }

            return researched;
        }
    }
}
