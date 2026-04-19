using System.Collections.Generic;
using HollowGround.Buildings;
using HollowGround.Quests;
using HollowGround.Resources;
using UnityEditor;
using UnityEngine;

namespace HollowGround.Editor
{
#if UNITY_EDITOR
    public static class QuestDataFactory
    {
        [MenuItem("Assets/Create/HollowGround/QuestData/Tutorial Build Farm")]
        public static void CreateTutorialBuildFarm() => CreateQuest(
            "Build Your First Farm", "Build a Farm to produce food for your survivors.",
            QuestType.Main, 1,
            new() { new QuestData.QuestObjective { Type = ObjectiveType.BuildBuilding, TargetId = "Farm", RequiredAmount = 1, Description = "Build a Farm" } },
            new() { new QuestData.ResourceReward { Type = ResourceType.Wood, Amount = 50 }, new QuestData.ResourceReward { Type = ResourceType.Metal, Amount = 30 } },
            50, 0);

        [MenuItem("Assets/Create/HollowGround/QuestData/Gather Resources")]
        public static void CreateGatherResources() => CreateQuest(
            "Stockpile Resources", "Gather 200 Wood and 100 Metal to prepare for expansion.",
            QuestType.Main, 1,
            new()
            {
                new QuestData.QuestObjective { Type = ObjectiveType.GatherResource, TargetId = "Wood", RequiredAmount = 200, Description = "Gather 200 Wood" },
                new QuestData.QuestObjective { Type = ObjectiveType.GatherResource, TargetId = "Metal", RequiredAmount = 100, Description = "Gather 100 Metal" }
            },
            new() { new QuestData.ResourceReward { Type = ResourceType.TechPart, Amount = 5 } },
            100, 0);

        [MenuItem("Assets/Create/HollowGround/QuestData/Train Army")]
        public static void CreateTrainArmy() => CreateQuest(
            "Raise an Army", "Train 5 soldiers to defend your base.",
            QuestType.Main, 2,
            new() { new QuestData.QuestObjective { Type = ObjectiveType.TrainTroops, TargetId = "Infantry", RequiredAmount = 5, Description = "Train 5 Infantry" } },
            new() { new QuestData.ResourceReward { Type = ResourceType.Food, Amount = 100 }, new QuestData.ResourceReward { Type = ResourceType.Water, Amount = 50 } },
            75, 0);

        [MenuItem("Assets/Create/HollowGround/QuestData/First Battle")]
        public static void CreateFirstBattle() => CreateQuest(
            "First Blood", "Win a battle against a nearby mutant camp.",
            QuestType.Main, 3,
            new() { new QuestData.QuestObjective { Type = ObjectiveType.WinBattles, TargetId = "", RequiredAmount = 1, Description = "Win 1 battle" } },
            new() { new QuestData.ResourceReward { Type = ResourceType.TechPart, Amount = 10 }, new QuestData.ResourceReward { Type = ResourceType.Metal, Amount = 50 } },
            150, 5);

        [MenuItem("Assets/Create/HollowGround/QuestData/Survive Waves")]
        public static void CreateSurviveWaves() => CreateQuest(
            "Hold the Line", "Survive 3 mutant attack waves.",
            QuestType.Side, 4,
            new() { new QuestData.QuestObjective { Type = ObjectiveType.SurviveWaves, TargetId = "", RequiredAmount = 3, Description = "Survive 3 waves" } },
            new() { new QuestData.ResourceReward { Type = ResourceType.TechPart, Amount = 20 }, new QuestData.ResourceReward { Type = ResourceType.Energy, Amount = 30 } },
            200, 10);

        [MenuItem("Assets/Create/HollowGround/QuestData/Build Shelter")]
        public static void CreateBuildShelter() => CreateQuest(
            "Home Sweet Home", "Build shelters to house more survivors.",
            QuestType.Main, 2,
            new() { new QuestData.QuestObjective { Type = ObjectiveType.BuildBuilding, TargetId = "Shelter", RequiredAmount = 2, Description = "Build 2 Shelters" } },
            new() { new QuestData.ResourceReward { Type = ResourceType.Food, Amount = 80 }, new QuestData.ResourceReward { Type = ResourceType.Wood, Amount = 60 } },
            100, 0);

        [MenuItem("Assets/Create/HollowGround/QuestData/Expand Territory")]
        public static void CreateExpandTerritory() => CreateQuest(
            "Expand Your Reach", "Explore 10 map nodes to expand your territory.",
            QuestType.Main, 3,
            new() { new QuestData.QuestObjective { Type = ObjectiveType.ExploreNodes, TargetId = "", RequiredAmount = 10, Description = "Explore 10 nodes" } },
            new() { new QuestData.ResourceReward { Type = ResourceType.TechPart, Amount = 15 }, new QuestData.ResourceReward { Type = ResourceType.Metal, Amount = 80 } },
            200, 5);

        [MenuItem("Assets/Create/HollowGround/QuestData/Build Generator")]
        public static void CreateBuildGenerator() => CreateQuest(
            "Power Up", "Build a Generator to produce energy for advanced buildings.",
            QuestType.Main, 3,
            new() { new QuestData.QuestObjective { Type = ObjectiveType.BuildBuilding, TargetId = "Generator", RequiredAmount = 1, Description = "Build a Generator" } },
            new() { new QuestData.ResourceReward { Type = ResourceType.TechPart, Amount = 10 }, new QuestData.ResourceReward { Type = ResourceType.Energy, Amount = 50 } },
            120, 0);

        [MenuItem("Assets/Create/HollowGround/QuestData/Military Might")]
        public static void CreateMilitaryMight() => CreateQuest(
            "Military Might", "Train 20 troops of any type to build a formidable army.",
            QuestType.Main, 4,
            new() { new QuestData.QuestObjective { Type = ObjectiveType.TrainTroops, TargetId = "", RequiredAmount = 20, Description = "Train 20 troops total" } },
            new() { new QuestData.ResourceReward { Type = ResourceType.TechPart, Amount = 25 }, new QuestData.ResourceReward { Type = ResourceType.Metal, Amount = 100 } },
            300, 10);

        [MenuItem("Assets/Create/HollowGround/QuestData/Warrior's Path")]
        public static void CreateWarriorsPath() => CreateQuest(
            "The Warrior's Path", "Win 5 battles to prove your strength.",
            QuestType.Main, 4,
            new() { new QuestData.QuestObjective { Type = ObjectiveType.WinBattles, TargetId = "", RequiredAmount = 5, Description = "Win 5 battles" } },
            new() { new QuestData.ResourceReward { Type = ResourceType.TechPart, Amount = 30 }, new QuestData.ResourceReward { Type = ResourceType.Metal, Amount = 150 } },
            400, 15);

        [MenuItem("Assets/Create/HollowGround/QuestData/Master Builder")]
        public static void CreateMasterBuilder() => CreateQuest(
            "Master Builder", "Build 10 buildings of any type.",
            QuestType.Side, 3,
            new() { new QuestData.QuestObjective { Type = ObjectiveType.BuildBuilding, TargetId = "", RequiredAmount = 10, Description = "Build 10 buildings" } },
            new() { new QuestData.ResourceReward { Type = ResourceType.Wood, Amount = 200 }, new QuestData.ResourceReward { Type = ResourceType.Metal, Amount = 100 } },
            150, 0);

        [MenuItem("Assets/Create/HollowGround/QuestData/Mutant Menace")]
        public static void CreateMutantMenace() => CreateQuest(
            "Mutant Menace", "Defeat 50 mutants in battle.",
            QuestType.Side, 4,
            new() { new QuestData.QuestObjective { Type = ObjectiveType.KillMutants, TargetId = "", RequiredAmount = 50, Description = "Kill 50 mutants" } },
            new() { new QuestData.ResourceReward { Type = ResourceType.TechPart, Amount = 20 }, new QuestData.ResourceReward { Type = ResourceType.Energy, Amount = 40 } },
            250, 5);

        [MenuItem("Assets/Create/HollowGround/QuestData/Resource Tycoon")]
        public static void CreateResourceTycoon() => CreateQuest(
            "Resource Tycoon", "Accumulate 500 Wood, 300 Metal and 200 Food.",
            QuestType.Side, 2,
            new()
            {
                new QuestData.QuestObjective { Type = ObjectiveType.GatherResource, TargetId = "Wood", RequiredAmount = 500, Description = "Gather 500 Wood" },
                new QuestData.QuestObjective { Type = ObjectiveType.GatherResource, TargetId = "Metal", RequiredAmount = 300, Description = "Gather 300 Metal" },
                new QuestData.QuestObjective { Type = ObjectiveType.GatherResource, TargetId = "Food", RequiredAmount = 200, Description = "Gather 200 Food" }
            },
            new() { new QuestData.ResourceReward { Type = ResourceType.TechPart, Amount = 15 } },
            200, 0);

        [MenuItem("Assets/Create/HollowGround/QuestData/Research Breakthrough")]
        public static void CreateResearchBreakthrough() => CreateQuest(
            "Research Breakthrough", "Complete 3 technology researches.",
            QuestType.Side, 3,
            new() { new QuestData.QuestObjective { Type = ObjectiveType.ResearchTech, TargetId = "", RequiredAmount = 3, Description = "Complete 3 researches" } },
            new() { new QuestData.ResourceReward { Type = ResourceType.TechPart, Amount = 25 }, new QuestData.ResourceReward { Type = ResourceType.Energy, Amount = 60 } },
            300, 10);

        [MenuItem("Assets/Create/HollowGround/QuestData/Survival Expert")]
        public static void CreateSurvivalExpert() => CreateQuest(
            "Survival Expert", "Survive 10 mutant attack waves without losing your base.",
            QuestType.Side, 5,
            new() { new QuestData.QuestObjective { Type = ObjectiveType.SurviveWaves, TargetId = "", RequiredAmount = 10, Description = "Survive 10 waves" } },
            new() { new QuestData.ResourceReward { Type = ResourceType.TechPart, Amount = 50 }, new QuestData.ResourceReward { Type = ResourceType.Metal, Amount = 200 } },
            500, 20);

        private static void CreateQuest(
            string name, string desc,
            QuestType type, int recLevel,
            List<QuestData.QuestObjective> objectives,
            List<QuestData.ResourceReward> rewards,
            int xp, int relationReward)
        {
            var data = ScriptableObject.CreateInstance<QuestData>();
            data.DisplayName = name;
            data.Description = desc;
            data.QuestType = type;
            data.RecommendedLevel = recLevel;
            data.Objectives = objectives;
            data.Rewards = rewards;
            data.XPReward = xp;
            data.RelationReward = relationReward;

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Quest Data", name.Replace(" ", ""), "asset", "Save quest data",
                "Assets/_Project/ScriptableObjects/Quests");

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
