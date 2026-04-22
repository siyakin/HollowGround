using System.Collections.Generic;
using HollowGround.NPCs;
using HollowGround.Resources;
using UnityEditor;
using UnityEngine;

namespace HollowGround.Editor
{
#if UNITY_EDITOR
    public static class FactionDataFactory
    {
        [MenuItem("HollowGround/Create All FactionData")]
        public static void CreateAllFactions()
        {
            string folder = "Assets/_Project/ScriptableObjects/Factions";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "Factions");

            CreateFactionDirect(folder, "ScavengerGuild", "Scavenger Guild",
                "A loose coalition of survivors who excel at finding resources in the wasteland. Willing to trade scavenged goods for tech parts.",
                10,
                new List<FactionData.TradeOffer>
                {
                    new() { Resource = ResourceType.Metal, Amount = 30, Price = 5 },
                    new() { Resource = ResourceType.Food, Amount = 20, Price = 4 }
                },
                new List<FactionData.TradeOffer>
                {
                    new() { Resource = ResourceType.Wood, Amount = 40, Price = 6 },
                    new() { Resource = ResourceType.Metal, Amount = 30, Price = 5 }
                });

            CreateFactionDirect(folder, "IronLegion", "Iron Legion",
                "A militaristic faction that controls the old factory district. They manufacture weapons and armor, and value strength above all.",
                -5,
                new List<FactionData.TradeOffer>
                {
                    new() { Resource = ResourceType.Metal, Amount = 60, Price = 10 },
                    new() { Resource = ResourceType.TechPart, Amount = 2, Price = 15 }
                },
                new List<FactionData.TradeOffer>
                {
                    new() { Resource = ResourceType.Metal, Amount = 50, Price = 8 },
                    new() { Resource = ResourceType.Food, Amount = 30, Price = 5 }
                });

            CreateFactionDirect(folder, "GreenHaven", "Green Haven",
                "A peaceful community of farmers and healers living in a relatively uncontaminated valley. They trade food and medical supplies.",
                20,
                new List<FactionData.TradeOffer>
                {
                    new() { Resource = ResourceType.Food, Amount = 80, Price = 8 },
                    new() { Resource = ResourceType.Water, Amount = 40, Price = 6 }
                },
                new List<FactionData.TradeOffer>
                {
                    new() { Resource = ResourceType.Wood, Amount = 30, Price = 4 },
                    new() { Resource = ResourceType.TechPart, Amount = 1, Price = 12 }
                });

            AssetDatabase.SaveAssets();
            Debug.Log("[FactionData] 3 factions created in " + folder);
        }

        private static void CreateFactionDirect(
            string folder, string fileName, string displayName, string description,
            int startRelation, List<FactionData.TradeOffer> sells, List<FactionData.TradeOffer> buys)
        {
            string path = $"{folder}/{fileName}.asset";
            if (AssetDatabase.LoadAssetAtPath<FactionData>(path) != null) return;

            var data = ScriptableObject.CreateInstance<FactionData>();
            data.DisplayName = displayName;
            data.Description = description;
            data.RelationPoints = startRelation;
            data.Sells = sells;
            data.Buys = buys;
            AssetDatabase.CreateAsset(data, path);
        }

        [MenuItem("Assets/Create/HollowGround/FactionData/Scavenger Guild")]
        public static void CreateScavengerGuild() => CreateFaction(
            "ScavengerGuild", "Scavenger Guild",
            "A loose coalition of survivors who excel at finding resources in the wasteland. Willing to trade scavenged goods for tech parts.",
            10,
            new List<FactionData.TradeOffer>
            {
                new() { Resource = ResourceType.Metal, Amount = 30, Price = 5 },
                new() { Resource = ResourceType.Food, Amount = 20, Price = 4 }
            },
            new List<FactionData.TradeOffer>
            {
                new() { Resource = ResourceType.Wood, Amount = 40, Price = 6 },
                new() { Resource = ResourceType.Metal, Amount = 30, Price = 5 }
            });

        [MenuItem("Assets/Create/HollowGround/FactionData/Iron Legion")]
        public static void CreateIronLegion() => CreateFaction(
            "IronLegion", "Iron Legion",
            "A militaristic faction that controls the old factory district. They manufacture weapons and armor, and value strength above all.",
            -5,
            new List<FactionData.TradeOffer>
            {
                new() { Resource = ResourceType.Metal, Amount = 60, Price = 10 },
                new() { Resource = ResourceType.TechPart, Amount = 2, Price = 15 }
            },
            new List<FactionData.TradeOffer>
            {
                new() { Resource = ResourceType.Metal, Amount = 50, Price = 8 },
                new() { Resource = ResourceType.Food, Amount = 30, Price = 5 }
            });

        [MenuItem("Assets/Create/HollowGround/FactionData/Green Haven")]
        public static void CreateGreenHaven() => CreateFaction(
            "GreenHaven", "Green Haven",
            "A peaceful community of farmers and healers living in a relatively uncontaminated valley. They trade food and medical supplies.",
            20,
            new List<FactionData.TradeOffer>
            {
                new() { Resource = ResourceType.Food, Amount = 80, Price = 8 },
                new() { Resource = ResourceType.Water, Amount = 40, Price = 6 }
            },
            new List<FactionData.TradeOffer>
            {
                new() { Resource = ResourceType.Wood, Amount = 30, Price = 4 },
                new() { Resource = ResourceType.TechPart, Amount = 1, Price = 12 }
            });

        private static void CreateFaction(
            string fileName,
            string displayName,
            string description,
            int startRelation,
            List<FactionData.TradeOffer> sells,
            List<FactionData.TradeOffer> buys)
        {
            var data = ScriptableObject.CreateInstance<FactionData>();
            data.DisplayName = displayName;
            data.Description = description;
            data.RelationPoints = startRelation;
            data.Sells = sells;
            data.Buys = buys;

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Faction Data", fileName, "asset", "Save faction data",
                "Assets/_Project/ScriptableObjects/Factions");

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
