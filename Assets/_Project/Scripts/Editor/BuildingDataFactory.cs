using System.Collections.Generic;
using HollowGround.Buildings;
using HollowGround.Core;
using HollowGround.Resources;
using UnityEditor;
using UnityEngine;

namespace HollowGround.Editor
{
#if UNITY_EDITOR
    public static class BuildingDataFactory
    {
        [MenuItem("Assets/Create/HollowGround/BuildingData/Command Center")]
        public static void CreateCommandCenter() => CreateBuildingData("CommandCenter", BuildingType.CommandCenter, BuildingCategory.Special,
            2, 2, false, ResourceType.Wood, 0, 0f, null, 0, 0, 1);

        [MenuItem("Assets/Create/HollowGround/BuildingData/Farm")]
        public static void CreateFarm() => CreateBuildingData("Farm", BuildingType.Farm, BuildingCategory.Resource,
            2, 2, true, ResourceType.Food, 10, 300f,
            CostEntryHelper.Costs(ResourceType.Wood, 50, ResourceType.Metal, 30));

        [MenuItem("Assets/Create/HollowGround/BuildingData/Wood Factory")]
        public static void CreateWoodFactory() => CreateBuildingData("WoodFactory", BuildingType.WoodFactory, BuildingCategory.Resource,
            2, 2, true, ResourceType.Wood, 12, 300f,
            CostEntryHelper.Costs(ResourceType.Metal, 40));

        [MenuItem("Assets/Create/HollowGround/BuildingData/Mine")]
        public static void CreateMine() => CreateBuildingData("Mine", BuildingType.Mine, BuildingCategory.Resource,
            2, 2, true, ResourceType.Metal, 8, 300f,
            CostEntryHelper.Costs(ResourceType.Wood, 50));

        [MenuItem("Assets/Create/HollowGround/BuildingData/Water Well")]
        public static void CreateWaterWell() => CreateBuildingData("WaterWell", BuildingType.WaterWell, BuildingCategory.Resource,
            1, 1, true, ResourceType.Water, 8, 300f,
            CostEntryHelper.Costs(ResourceType.Metal, 30));

        [MenuItem("Assets/Create/HollowGround/BuildingData/Barracks")]
        public static void CreateBarracks() => CreateBuildingData("Barracks", BuildingType.Barracks, BuildingCategory.Military,
            2, 2, false, ResourceType.Wood, 0, 0f,
            CostEntryHelper.Costs(ResourceType.Food, 80, ResourceType.Metal, 60));

        [MenuItem("Assets/Create/HollowGround/BuildingData/Shelter")]
        public static void CreateShelter() => CreateBuildingData("Shelter", BuildingType.Shelter, BuildingCategory.Social,
            2, 2, false, ResourceType.Wood, 0, 0f,
            CostEntryHelper.Costs(ResourceType.Wood, 50, ResourceType.Metal, 30),
            populationCapacity: 10);

        [MenuItem("Assets/Create/HollowGround/BuildingData/Storage")]
        public static void CreateStorage() => CreateBuildingData("Storage", BuildingType.Storage, BuildingCategory.Resource,
            2, 2, false, ResourceType.Wood, 0, 0f,
            CostEntryHelper.Costs(ResourceType.Metal, 60),
            storageCapacity: 500);

        [MenuItem("Assets/Create/HollowGround/BuildingData/Generator")]
        public static void CreateGenerator() => CreateBuildingData("Generator", BuildingType.Generator, BuildingCategory.Resource,
            2, 2, true, ResourceType.Energy, 5, 300f,
            CostEntryHelper.Costs(ResourceType.Metal, 80, ResourceType.TechPart, 10));

        [MenuItem("Assets/Create/HollowGround/BuildingData/Workshop")]
        public static void CreateWorkshop() => CreateBuildingData("Workshop", BuildingType.Workshop, BuildingCategory.Military,
            2, 2, false, ResourceType.Wood, 0, 0f,
            CostEntryHelper.Costs(ResourceType.Metal, 100, ResourceType.TechPart, 20),
            commandCenterLevelRequired: 2);

        [MenuItem("Assets/Create/HollowGround/BuildingData/Research Lab")]
        public static void CreateResearchLab() => CreateBuildingData("ResearchLab", BuildingType.ResearchLab, BuildingCategory.Special,
            2, 2, false, ResourceType.Wood, 0, 0f,
            CostEntryHelper.Costs(ResourceType.Metal, 120, ResourceType.Food, 30),
            commandCenterLevelRequired: 3);

        private static void CreateBuildingData(
            string name,
            BuildingType type,
            BuildingCategory category,
            int sizeX,
            int sizeZ,
            bool hasProduction,
            ResourceType producedRes,
            int baseProdAmt,
            float prodInterval,
            List<BuildingData.CostEntry> costs,
            int populationCapacity = 0,
            int storageCapacity = 0,
            int commandCenterLevelRequired = 1)
        {
            var data = ScriptableObject.CreateInstance<BuildingData>();
            data.DisplayName = name;
            data.Type = type;
            data.Category = category;
            data.SizeX = sizeX;
            data.SizeZ = sizeZ;
            data.MaxLevel = 10;
            data.HasProduction = hasProduction;
            data.ProducedResource = producedRes;
            data.BaseProductionAmount = baseProdAmt;
            data.ProductionInterval = prodInterval;
            data.PopulationCapacity = populationCapacity;
            data.StorageCapacity = storageCapacity;
            data.CommandCenterLevelRequired = commandCenterLevelRequired;

            if (costs != null)
                data.BaseCost = costs;

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Building Data", name, "asset", "Save building data asset",
                "Assets/_Project/ScriptableObjects/Buildings");

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(data, path);
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = data;
            }
        }
    }
#endif
}
