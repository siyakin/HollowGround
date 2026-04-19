using System.Collections.Generic;
using HollowGround.Buildings;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.Tech
{
    public enum TechCategory
    {
        Construction,
        Agriculture,
        Military,
        Medicine,
        Exploration
    }

    [CreateAssetMenu(fileName = "TechNode", menuName = "HollowGround/TechNode")]
    public class TechNode : ScriptableObject
    {
        [Header("Info")]
        public string DisplayName;
        public string Description;
        public TechCategory Category;
        public int Level = 1;
        public Sprite Icon;

        [Header("Prerequisites")]
        public List<TechNode> Prerequisites = new();

        [Header("Cost")]
        public List<BuildingData.CostEntry> ResearchCost = new();
        public float ResearchTime = 300f;

        [Header("Unlocks")]
        public List<BuildingData> UnlockedBuildings = new();
        public List<string> UnlockedTags = new();

        [Header("Bonuses")]
        public float ProductionBonus;
        public float TrainingSpeedBonus;
        public float ExpeditionSpeedBonus;
        public float DefenseBonus;

        public bool IsResearched { get; set; }
        public bool IsResearching { get; set; }
        public float ResearchProgress { get; set; }

        public Dictionary<ResourceType, int> GetCost()
        {
            var result = new Dictionary<ResourceType, int>();
            foreach (var entry in ResearchCost)
                result[entry.Type] = entry.Amount;
            return result;
        }

        public bool CanResearch()
        {
            if (IsResearched || IsResearching) return false;

            foreach (var prereq in Prerequisites)
            {
                if (prereq == null || !prereq.IsResearched)
                    return false;
            }

            return true;
        }
    }
}
