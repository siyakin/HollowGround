using System.Collections.Generic;
using HollowGround.Army;
using HollowGround.Buildings;
using HollowGround.Resources;
using UnityEditor;
using UnityEngine;

namespace HollowGround.Editor
{
#if UNITY_EDITOR
    public static class TroopDataFactory
    {
        [MenuItem("Assets/Create/HollowGround/TroopData/Piyade")]
        public static void CreateInfantry() => CreateTroopData("Piyade", "Dengeli birlik. Cephede çatışır.",
            TroopType.Infantry, TroopRole.Balanced, 100, 10, 5, 3f,
            TroopType.Heavy, TroopType.Sniper, 30f, 1,
            Costs(ResourceType.Food, 5, ResourceType.Metal, 3));

        [MenuItem("Assets/Create/HollowGround/TroopData/Nişancı")]
        public static void CreateSniper() => CreateTroopData("Nişancı", "Uzun menzilli hasar. Yakın dövüşte zayıf.",
            TroopType.Sniper, TroopRole.Ranged, 60, 15, 3, 2.5f,
            TroopType.Infantry, TroopType.Heavy, 45f, 1,
            Costs(ResourceType.Food, 8, ResourceType.Metal, 5));

        [MenuItem("Assets/Create/HollowGround/TroopData/Ağır Asker")]
        public static void CreateHeavy() => CreateTroopData("Ağır Asker", "Tank birlik. Yüksek HP, yavaş hareket.",
            TroopType.Heavy, TroopRole.Tank, 200, 8, 15, 1.5f,
            TroopType.Sniper, TroopType.Infantry, 60f, 2,
            Costs(ResourceType.Food, 12, ResourceType.Metal, 10));

        [MenuItem("Assets/Create/HollowGround/TroopData/Gözcü")]
        public static void CreateScout() => CreateTroopData("Gözcü", "Hızlı keşif birlik. Gizli hareket.",
            TroopType.Scout, TroopRole.Recon, 50, 6, 2, 6f,
            TroopType.Engineer, TroopType.Heavy, 20f, 1,
            Costs(ResourceType.Food, 3, ResourceType.Metal, 2));

        [MenuItem("Assets/Create/HollowGround/TroopData/Mühendis")]
        public static void CreateEngineer() => CreateTroopData("Mühendis", "Destek birlik. Tamir ve tuzak kurma.",
            TroopType.Engineer, TroopRole.Support, 70, 5, 8, 2f,
            TroopType.Heavy, TroopType.Scout, 50f, 2,
            Costs(ResourceType.Food, 10, ResourceType.Metal, 8));

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

        private static void CreateTroopData(
            string name, string desc,
            TroopType type, TroopRole role,
            int hp, int atk, int def, float spd,
            TroopType strong, TroopType weak,
            float trainTime, int barracksLvl,
            List<BuildingData.CostEntry> costs)
        {
            var data = ScriptableObject.CreateInstance<TroopData>();
            data.DisplayName = name;
            data.Description = desc;
            data.Type = type;
            data.Role = role;
            data.BaseHP = hp;
            data.BaseAttack = atk;
            data.BaseDefense = def;
            data.BaseSpeed = spd;
            data.StrongAgainst = strong;
            data.WeakAgainst = weak;
            data.TrainingTime = trainTime;
            data.BarracksLevelRequired = barracksLvl;
            data.TrainingCost = costs;

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Troop Data", name, "asset", "Save troop data",
                "Assets/_Project/ScriptableObjects/Troops");

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
