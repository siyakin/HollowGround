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

        [Serializable]
        public class BuildingModels
        {
            public GameObject ConstructModel;
            public GameObject Level01Model;
            public GameObject Level03Model;
            public GameObject Level05Model;
            public GameObject Level10Model;
            public GameObject DamagedModel;
            public GameObject DestroyedModel;

            public GameObject GetActiveModel(int level)
            {
                if (level >= 10 && Level10Model != null) return Level10Model;
                if (level >= 5 && Level05Model != null) return Level05Model;
                if (level >= 3 && Level03Model != null) return Level03Model;
                return Level01Model;
            }

            public GameObject GetModelForState(BuildingState state, int level)
            {
                return state switch
                {
                    BuildingState.Constructing => ConstructModel != null ? ConstructModel : GetActiveModel(level),
                    BuildingState.Damaged => DamagedModel != null ? DamagedModel : GetActiveModel(level),
                    BuildingState.Destroyed => DestroyedModel != null ? DestroyedModel : GetActiveModel(level),
                    _ => GetActiveModel(level)
                };
            }
        }

        [Header("Info")]
        public string DisplayName;
        public string Description;
        public BuildingType Type;
        public BuildingCategory Category;
        public int MaxLevel = 10;

        [Header("Models")]
        public BuildingModels Models;

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
            return Models != null ? Models.GetActiveModel(level) : null;
        }

        public GameObject GetModelForState(BuildingState state, int level)
        {
            return Models != null ? Models.GetModelForState(state, level) : null;
        }
    }
}
