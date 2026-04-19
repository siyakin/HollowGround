using HollowGround.Buildings;
using UnityEngine;

namespace HollowGround.Buildings
{
    public static class BuildingDatabase
    {
        private static readonly string SO_PATH = "Assets/_Project/ScriptableObjects/Buildings/";

        public static BuildingData LoadBuildingData(BuildingType type)
        {
            string assetName = type switch
            {
                BuildingType.CommandCenter => "CommandCenter",
                BuildingType.Farm => "Farm",
                BuildingType.WaterWell => "WaterWell",
                BuildingType.WoodFactory => "WoodFactory",
                BuildingType.Mine => "Mine",
                BuildingType.Workshop => "Workshop",
                BuildingType.ResearchLab => "ResearchLab",
                BuildingType.Barracks => "Barracks",
                BuildingType.Walls => "Walls",
                BuildingType.WatchTower => "WatchTower",
                BuildingType.TradeCenter => "TradeCenter",
                BuildingType.Hospital => "Hospital",
                BuildingType.Generator => "Generator",
                BuildingType.Shelter => "Shelter",
                BuildingType.Storage => "Storage",
                _ => type.ToString()
            };

#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"{assetName} t:BuildingData", new[] { SO_PATH });
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                return UnityEditor.AssetDatabase.LoadAssetAtPath<BuildingData>(path);
            }
#endif
            return null;
        }
    }
}
