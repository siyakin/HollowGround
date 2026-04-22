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
