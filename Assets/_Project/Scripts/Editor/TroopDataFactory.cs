using System.Collections.Generic;
using HollowGround.Army;
using HollowGround.Buildings;
using HollowGround.Core;
using HollowGround.Resources;
using UnityEditor;
using UnityEngine;

namespace HollowGround.Editor
{
    public static class TroopDataFactory
    {
        private static readonly string Folder = "Assets/_Project/ScriptableObjects/Troops";

        [MenuItem("HollowGround/Create All TroopData")]
        public static void CreateAll()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "Troops");

            Create("Infantry", "Balanced unit. Fights on the front line.",
                TroopType.Infantry, TroopRole.Balanced, 100, 10, 5, 3f,
                TroopType.Heavy, TroopType.Sniper, 30f, 1,
                Costs(ResourceType.Food, 5, ResourceType.Metal, 3));

            Create("Sniper", "Long range damage. Weak in close combat.",
                TroopType.Sniper, TroopRole.Ranged, 60, 15, 3, 2.5f,
                TroopType.Infantry, TroopType.Heavy, 45f, 1,
                Costs(ResourceType.Food, 8, ResourceType.Metal, 5));

            Create("Heavy", "Tank unit. High HP, slow movement.",
                TroopType.Heavy, TroopRole.Tank, 200, 8, 15, 1.5f,
                TroopType.Sniper, TroopType.Infantry, 60f, 2,
                Costs(ResourceType.Food, 12, ResourceType.Metal, 10));

            Create("Scout", "Fast recon unit. Stealthy movement.",
                TroopType.Scout, TroopRole.Recon, 50, 6, 2, 6f,
                TroopType.Engineer, TroopType.Heavy, 20f, 1,
                Costs(ResourceType.Food, 3, ResourceType.Metal, 2));

            Create("Engineer", "Support unit. Repair and trap building.",
                TroopType.Engineer, TroopRole.Support, 70, 5, 8, 2f,
                TroopType.Heavy, TroopType.Scout, 50f, 2,
                Costs(ResourceType.Food, 10, ResourceType.Metal, 8));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[TroopDataFactory] 5 TroopData SOs created in " + Folder);
        }

        private static List<BuildingData.CostEntry> Costs(params object[] pairs)
        {
            var list = new List<BuildingData.CostEntry>();
            for (int i = 0; i < pairs.Length - 1; i += 2)
                list.Add(new BuildingData.CostEntry { Type = (ResourceType)pairs[i], Amount = (int)pairs[i + 1] });
            return list;
        }

        private static void Create(string name, string desc,
            TroopType type, TroopRole role,
            int hp, int atk, int def, float spd,
            TroopType strong, TroopType weak,
            float trainTime, int barracksLvl,
            List<BuildingData.CostEntry> costs)
        {
            string path = $"{Folder}/{name}.asset";
            if (AssetDatabase.LoadAssetAtPath<TroopData>(path) != null) return;

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

            AssetDatabase.CreateAsset(data, path);
        }
    }
}
