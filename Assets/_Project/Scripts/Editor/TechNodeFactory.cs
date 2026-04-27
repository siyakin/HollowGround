using System.Collections.Generic;
using HollowGround.Buildings;
using HollowGround.Core;
using HollowGround.Resources;
using HollowGround.Tech;
using UnityEditor;
using UnityEngine;

namespace HollowGround.Editor
{
    public static class TechNodeFactory
    {
        private static readonly string Folder = "Assets/_Project/Resources/TechNodes";

        [MenuItem("HollowGround/Create All TechNodes")]
        public static void CreateAll()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources"))
                AssetDatabase.CreateFolder("Assets/_Project", "Resources");
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets/_Project/Resources", "TechNodes");

            Create("BasicConstruction", "Basic Construction", "Reduces construction time and enables advanced structures.",
                TechCategory.Construction, 60, Costs(ResourceType.Metal, 50, ResourceType.TechPart, 5),
                productionBonus: 0.1f);

            Create("AdvancedConstruction", "Advanced Construction", "Stronger structures and defensive walls.",
                TechCategory.Construction, 120, Costs(ResourceType.Metal, 150, ResourceType.TechPart, 15),
                productionBonus: 0.15f, defenseBonus: 0.1f, prereq: "BasicConstruction");

            Create("BasicAgriculture", "Basic Agriculture", "Improved farming increases food production.",
                TechCategory.Agriculture, 60, Costs(ResourceType.Wood, 40, ResourceType.TechPart, 5),
                productionBonus: 0.15f);

            Create("EfficientFarming", "Efficient Farming", "Advanced irrigation doubles food output.",
                TechCategory.Agriculture, 120, Costs(ResourceType.Food, 100, ResourceType.TechPart, 20),
                productionBonus: 0.25f, prereq: "BasicAgriculture");

            Create("BasicWeapons", "Basic Weapons", "Basic weapon modifications for troops.",
                TechCategory.Military, 60, Costs(ResourceType.Metal, 60, ResourceType.TechPart, 10),
                trainingSpeedBonus: 0.1f);

            Create("AdvancedWeapons", "Advanced Weapons", "Powerful upgrades and faster training.",
                TechCategory.Military, 120, Costs(ResourceType.Metal, 200, ResourceType.TechPart, 25),
                trainingSpeedBonus: 0.2f, defenseBonus: 0.1f, prereq: "BasicWeapons");

            Create("BasicMedicine", "Basic Medicine", "Troops recover faster, reduced casualties.",
                TechCategory.Medicine, 60, Costs(ResourceType.Food, 50, ResourceType.TechPart, 8),
                trainingSpeedBonus: 0.05f);

            Create("RadiationTreatment", "Radiation Treatment", "Anti-radiation drugs for exploration.",
                TechCategory.Medicine, 120, Costs(ResourceType.TechPart, 30, ResourceType.Food, 80),
                expeditionSpeedBonus: 0.15f, prereq: "BasicMedicine");

            Create("BasicExploration", "Basic Exploration", "Scout training speeds up expeditions.",
                TechCategory.Exploration, 60, Costs(ResourceType.Wood, 30, ResourceType.TechPart, 5),
                expeditionSpeedBonus: 0.2f);

            Create("RadioactiveCrossing", "Radioactive Crossing", "Safe travel through irradiated areas.",
                TechCategory.Exploration, 120, Costs(ResourceType.TechPart, 40, ResourceType.Metal, 100),
                expeditionSpeedBonus: 0.3f, prereq: "BasicExploration");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[TechNodeFactory] 10 TechNode SOs created in " + Folder);
        }

        private static List<BuildingData.CostEntry> Costs(params object[] pairs)
        {
            var list = new List<BuildingData.CostEntry>();
            for (int i = 0; i < pairs.Length - 1; i += 2)
                list.Add(new BuildingData.CostEntry { Type = (ResourceType)pairs[i], Amount = (int)pairs[i + 1] });
            return list;
        }

        private static void Create(string fileName, string displayName, string desc,
            TechCategory category, float researchTime, List<BuildingData.CostEntry> cost,
            float productionBonus = 0f, float trainingSpeedBonus = 0f,
            float expeditionSpeedBonus = 0f, float defenseBonus = 0f,
            string prereq = null)
        {
            string path = $"{Folder}/{fileName}.asset";
            if (AssetDatabase.LoadAssetAtPath<TechNode>(path) != null) return;

            var node = ScriptableObject.CreateInstance<TechNode>();
            node.DisplayName = displayName;
            node.Description = desc;
            node.Category = category;
            node.ResearchCost = cost;
            node.ResearchTime = researchTime;
            node.ProductionBonus = productionBonus;
            node.TrainingSpeedBonus = trainingSpeedBonus;
            node.ExpeditionSpeedBonus = expeditionSpeedBonus;
            node.DefenseBonus = defenseBonus;

            AssetDatabase.CreateAsset(node, path);

            if (!string.IsNullOrEmpty(prereq))            {
                string prereqPath = $"{Folder}/{prereq}.asset";
                var prereqNode = AssetDatabase.LoadAssetAtPath<TechNode>(prereqPath);
                if (prereqNode != null)
                {
                    node.Prerequisites = new List<TechNode> { prereqNode };
                    EditorUtility.SetDirty(node);
                }
                else
                {
                    Debug.LogWarning($"[TechNodeFactory] Prerequisite '{prereq}' not found for '{fileName}'");
                }
            }
        }
    }
}
