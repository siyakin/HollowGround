using System.Collections.Generic;
using HollowGround.Buildings;
using HollowGround.Resources;
using HollowGround.Tech;
using UnityEditor;
using UnityEngine;

namespace HollowGround.Editor
{
#if UNITY_EDITOR
    public static class TechNodeFactory
    {
        [MenuItem("Assets/Create/HollowGround/TechNode/Basic Construction")]
        public static void CreateBasicConstruction() => CreateTech(
            "BasicConstruction", "Basic Construction",
            "Unlock fundamental building techniques. Reduces construction time and enables advanced structures.",
            TechCategory.Construction, 1,
            null,
            Costs(ResourceType.Metal, 50, ResourceType.TechPart, 5),
            120f,
            productionBonus: 0.1f);

        [MenuItem("Assets/Create/HollowGround/TechNode/Advanced Construction")]
        public static void CreateAdvancedConstruction() => CreateTech(
            "AdvancedConstruction", "Advanced Construction",
            "Master building techniques allow stronger structures and defensive walls.",
            TechCategory.Construction, 2,
            "BasicConstruction",
            Costs(ResourceType.Metal, 150, ResourceType.TechPart, 15),
            300f,
            productionBonus: 0.15f,
            defenseBonus: 0.1f);

        [MenuItem("Assets/Create/HollowGround/TechNode/Basic Agriculture")]
        public static void CreateBasicAgriculture() => CreateTech(
            "BasicAgriculture", "Basic Agriculture",
            "Improved farming methods increase food production significantly.",
            TechCategory.Agriculture, 1,
            null,
            Costs(ResourceType.Wood, 40, ResourceType.TechPart, 5),
            100f,
            productionBonus: 0.15f);

        [MenuItem("Assets/Create/HollowGround/TechNode/Efficient Farming")]
        public static void CreateEfficientFarming() => CreateTech(
            "EfficientFarming", "Efficient Farming",
            "Advanced irrigation and crop rotation techniques double food output.",
            TechCategory.Agriculture, 2,
            "BasicAgriculture",
            Costs(ResourceType.Food, 100, ResourceType.TechPart, 20),
            250f,
            productionBonus: 0.25f);

        [MenuItem("Assets/Create/HollowGround/TechNode/Basic Weapons")]
        public static void CreateBasicWeapons() => CreateTech(
            "BasicWeapons", "Basic Weapons",
            "Develop basic weapon modifications for your troops.",
            TechCategory.Military, 1,
            null,
            Costs(ResourceType.Metal, 60, ResourceType.TechPart, 10),
            150f,
            trainingSpeedBonus: 0.1f);

        [MenuItem("Assets/Create/HollowGround/TechNode/Advanced Weapons")]
        public static void CreateAdvancedWeapons() => CreateTech(
            "AdvancedWeapons", "Advanced Weapons",
            "Military research yields powerful weapon upgrades and faster training.",
            TechCategory.Military, 2,
            "BasicWeapons",
            Costs(ResourceType.Metal, 200, ResourceType.TechPart, 25),
            400f,
            trainingSpeedBonus: 0.2f,
            defenseBonus: 0.1f);

        [MenuItem("Assets/Create/HollowGround/TechNode/Basic Medicine")]
        public static void CreateBasicMedicine() => CreateTech(
            "BasicMedicine", "Basic Medicine",
            "Medical knowledge helps troops recover faster and reduces casualties.",
            TechCategory.Medicine, 1,
            null,
            Costs(ResourceType.Food, 50, ResourceType.TechPart, 8),
            120f,
            trainingSpeedBonus: 0.05f);

        [MenuItem("Assets/Create/HollowGround/TechNode/Radiation Treatment")]
        public static void CreateRadiationTreatment() => CreateTech(
            "RadiationTreatment", "Radiation Treatment",
            "Develop anti-radiation drugs allowing exploration of radioactive zones.",
            TechCategory.Medicine, 2,
            "BasicMedicine",
            Costs(ResourceType.TechPart, 30, ResourceType.Food, 80),
            350f,
            expeditionSpeedBonus: 0.15f);

        [MenuItem("Assets/Create/HollowGround/TechNode/Basic Exploration")]
        public static void CreateBasicExploration() => CreateTech(
            "BasicExploration", "Basic Exploration",
            "Scout training and pathfinding techniques speed up expeditions.",
            TechCategory.Exploration, 1,
            null,
            Costs(ResourceType.Wood, 30, ResourceType.TechPart, 5),
            100f,
            expeditionSpeedBonus: 0.2f);

        [MenuItem("Assets/Create/HollowGround/TechNode/Radioactive Crossing")]
        public static void CreateRadioactiveCrossing() => CreateTech(
            "RadioactiveCrossing", "Radioactive Crossing",
            "Special equipment allows safe travel through irradiated areas.",
            TechCategory.Exploration, 2,
            "BasicExploration",
            Costs(ResourceType.TechPart, 40, ResourceType.Metal, 100),
            400f,
            expeditionSpeedBonus: 0.3f);

        private static List<BuildingData.CostEntry> Costs(params object[] pairs)
        {
            var list = new List<BuildingData.CostEntry>();
            for (int i = 0; i < pairs.Length - 1; i += 2)
            {
                list.Add(new BuildingData.CostEntry
                {
                    Type = (ResourceType)pairs[i],
                    Amount = (int)pairs[i + 1]
                });
            }
            return list;
        }

        private static void CreateTech(
            string fileName,
            string displayName,
            string description,
            TechCategory category,
            int level,
            string prerequisiteName,
            List<BuildingData.CostEntry> researchCost,
            float researchTime,
            float productionBonus = 0f,
            float trainingSpeedBonus = 0f,
            float expeditionSpeedBonus = 0f,
            float defenseBonus = 0f)
        {
            var node = ScriptableObject.CreateInstance<TechNode>();
            node.DisplayName = displayName;
            node.Description = description;
            node.Category = category;
            node.Level = level;
            node.ResearchCost = researchCost;
            node.ResearchTime = researchTime;
            node.ProductionBonus = productionBonus;
            node.TrainingSpeedBonus = trainingSpeedBonus;
            node.ExpeditionSpeedBonus = expeditionSpeedBonus;
            node.DefenseBonus = defenseBonus;

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Tech Node", fileName, "asset", "Save tech node",
                "Assets/_Project/ScriptableObjects/TechNodes");

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(node, path);
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = node;
            }
        }
    }
#endif
}
