using UnityEngine;

namespace HollowGround.Core
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "HollowGround/GameConfig")]
    public class GameConfig : SingletonScriptableObject<GameConfig>
    {
        [Header("Developer Mode")]
        [Tooltip("Enable to speed up all timings for testing")]
        public bool DevMode = false;
        [Tooltip("Disable mutant attacks entirely (for testing)")]
        public bool DisableMutantAttacks = false;
        [Tooltip("Disable settler spawning entirely (for testing)")]
        public bool DisableSettlers = false;
        [Tooltip("Enable session logging to file")]
        public bool EnableSessionLog = false;
        [Tooltip("Start with multiplied resources (for testing)")]
        public bool BoostStartingResources = false;
        public int BoostMultiplier = 10;

        [Header("Speed Multipliers (DevMode ON)")]
        [Range(0.01f, 1f)] public float BuildTimeMultiplier = 0.1f;
        [Range(0.01f, 1f)] public float ProductionIntervalMultiplier = 0.1f;
        [Range(0.01f, 1f)] public float TrainingTimeMultiplier = 0.1f;
        [Range(0.01f, 1f)] public float ResearchTimeMultiplier = 0.1f;
        [Range(0.01f, 1f)] public float ExpeditionTimeMultiplier = 0.1f;
        [Range(0.01f, 1f)] public float MutantAttackIntervalMultiplier = 0.1f;

        [Header("Normal Multipliers (DevMode OFF)")]
        public float NormalBuildTimeMultiplier = 1f;
        public float NormalProductionIntervalMultiplier = 1f;
        public float NormalTrainingTimeMultiplier = 1f;
        public float NormalResearchTimeMultiplier = 1f;
        public float NormalExpeditionTimeMultiplier = 1f;
        public float NormalMutantAttackIntervalMultiplier = 1f;

        [Header("Economy")]
        [Range(0f, 1f)] public float DemolishRefundRatio = 0.5f;
        [Range(0f, 1f)] public float RepairCostRatio = 0.5f;

        [Header("Combat")]
        public int WallDefenseBonus = 20;
        [Range(0f, 1f)] public float DefeatTroopLossRatio = 0.6f;

        [Header("Settlers")]
        public float SettlersPerPopulation = 0.2f;
        public int MaxSettlers = 20;
        public float SettlerMoveSpeed = 2f;
        public float SettlerIdleTime = 3f;
        [Tooltip("How often (seconds) SettlerManager checks population and adjusts count")]
        public float SettlerSpawnCheckInterval = 5f;

        public float GetBuildTimeMultiplier => DevMode ? BuildTimeMultiplier : NormalBuildTimeMultiplier;
        public float GetProductionIntervalMultiplier => DevMode ? ProductionIntervalMultiplier : NormalProductionIntervalMultiplier;
        public float GetTrainingTimeMultiplier => DevMode ? TrainingTimeMultiplier : NormalTrainingTimeMultiplier;
        public float GetResearchTimeMultiplier => DevMode ? ResearchTimeMultiplier : NormalResearchTimeMultiplier;
        public float GetExpeditionTimeMultiplier => DevMode ? ExpeditionTimeMultiplier : NormalExpeditionTimeMultiplier;
        public float GetMutantAttackIntervalMultiplier => DevMode ? MutantAttackIntervalMultiplier : NormalMutantAttackIntervalMultiplier;
    }

    public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                var results = UnityEngine.Resources.LoadAll<T>("");
                _instance = results.Length > 0 ? results[0] : null;
                return _instance;
            }
        }
    }
}
