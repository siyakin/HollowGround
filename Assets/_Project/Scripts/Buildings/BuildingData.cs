using System;
using System.Collections.Generic;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.Buildings
{
    [CreateAssetMenu(fileName = "BuildingData", menuName = "HollowGround/New Building Data")]
    public class BuildingData : ScriptableObject
    {
        [Serializable]
        public class CostEntry
        {
            public ResourceType Type;
            public int Amount;
        }

        [Header("Info")]
        public string DisplayName;
        public string Description;
        public BuildingType Type;
        public BuildingCategory Category;
        public GameObject[] LevelPrefabs;
        public int MaxLevel = 10;

        [Header("Size")]
        public int SizeX = 1;
        public int SizeZ = 1;

        [Header("Costs (Level 1)")]
        public List<CostEntry> BaseCost = new();

        [Header("Cost Multiplier Per Level")]
        public float CostMultiplier = 1.5f;

        [Header("Build Time (seconds)")]
        public float BaseBuildTime = 10f;

        [Header("Production")]
        public bool HasProduction;
        public ResourceType ProducedResource;
        public int BaseProductionAmount = 10;
        public float ProductionInterval = 300f;
        public float ProductionMultiplierPerLevel = 1.2f;

        [Header("Population")]
        public int PopulationCapacity;
        public int StorageCapacity;

        [Header("Requirements")]
        public int CommandCenterLevelRequired = 1;

        public Dictionary<ResourceType, int> GetCostForLevel(int level)
        {
            var result = new Dictionary<ResourceType, int>();
            float mult = Mathf.Pow(CostMultiplier, level - 1);
            foreach (var entry in BaseCost)
            {
                result[entry.Type] = Mathf.CeilToInt(entry.Amount * mult);
            }
            return result;
        }

        public float GetBuildTimeForLevel(int level)
        {
            return BaseBuildTime * Mathf.Pow(1.3f, level - 1);
        }

        public int GetProductionForLevel(int level)
        {
            if (!HasProduction) return 0;
            return Mathf.CeilToInt(BaseProductionAmount * Mathf.Pow(ProductionMultiplierPerLevel, level - 1));
        }

        public GameObject GetPrefabForLevel(int level)
        {
            int index = Mathf.Clamp(level - 1, 0, LevelPrefabs.Length - 1);
            return LevelPrefabs.Length > 0 ? LevelPrefabs[index] : null;
        }
    }
}
