using HollowGround.Core;
using UnityEditor;
using UnityEngine;

namespace HollowGround.Editor
{
    public static class GameConfigCreator
    {
        [MenuItem("HollowGround/Create GameConfig")]
        public static void Create()
        {
            if (GameConfig.Instance != null)
            {
                Debug.Log("[GameConfig] Already exists.");
                return;
            }

            var config = ScriptableObject.CreateInstance<GameConfig>();
            config.DevMode = false;
            config.DemolishRefundRatio = 0.5f;
            config.RepairCostRatio = 0.5f;
            config.WallDefenseBonus = 20;
            config.DefeatTroopLossRatio = 0.6f;
            config.SettlersPerPopulation = 0.2f;
            config.MaxSettlers = 20;
            config.SettlerMoveSpeed = 2f;
            config.SettlerIdleTime = 3f;
            config.SettlerSpawnCheckInterval = 5f;
            config.DisableSettlers = false;
            config.EnableWorker = true;
            config.EnableAdventurer = true;
            config.EnableSuit = true;
            config.EnableAutoSave = false;
            config.AutoSaveInterval = 300f;
            config.EnableWeather = true;
            config.EnableDebugHUD = true;
            config.BuildTimeMultiplier = 0.1f;
            config.ProductionIntervalMultiplier = 0.1f;
            config.TrainingTimeMultiplier = 0.1f;
            config.ResearchTimeMultiplier = 0.1f;
            config.ExpeditionTimeMultiplier = 0.1f;
            config.MutantAttackIntervalMultiplier = 0.1f;

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            AssetDatabase.CreateAsset(config, "Assets/Resources/GameConfig.asset");
            AssetDatabase.SaveAssets();
            Selection.activeObject = config;
            Debug.Log("[GameConfig] Created at Assets/Resources/GameConfig.asset");
        }
    }
}
