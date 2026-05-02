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
        [Tooltip("Enable auto-save every N seconds (may cause brief hitch)")]
        public bool EnableAutoSave = false;
        public float AutoSaveInterval = 300f;
        [Tooltip("Enable weather system")]
        public bool EnableWeather = true;
        [Tooltip("Show debug HUD panel")]
        public bool EnableDebugHUD = true;

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
        public float SettlerWorkDuration = 8f;
        public float SettlerRestDuration = 5f;

        [Header("Settler Models")]
        public bool EnableWorker = true;
        public bool EnableAdventurer = true;
        public bool EnableSuit = true;

        [Header("Water")]
        [Tooltip("Use fancy water shader (Gerstner waves, depth, foam). Off = simple flat quad.")]
        public bool EnableFancyWater = true;
        [Tooltip("Wave animation speed. 0 = still, 0.1 = calm lake, 0.5 = rough lake")]
        [Range(0f, 2f)] public float WaterWaveSpeed = 0.15f;
        [Tooltip("Wave vertex displacement height")]
        [Range(0f, 0.2f)] public float WaterWaveHeight = 0.02f;
        [Tooltip("Foam amount at edges and crests")]
        [Range(0f, 1f)] public float WaterFoamAmount = 0.25f;
        [Tooltip("Procedural surface normal strength")]
        [Range(0f, 1f)] public float WaterNormalStrength = 0.3f;
        [Tooltip("Fresnel reflection power")]
        [Range(1f, 5f)] public float WaterFresnelPower = 2.5f;
        [Tooltip("Depth-based color transition")]
        [Range(0f, 5f)] public float WaterDepthFactor = 1.5f;
        [Tooltip("Screen-space refraction distortion")]
        [Range(0f, 0.1f)] public float WaterRefractionStrength = 0.01f;
        [Tooltip("Water surface opacity")]
        [Range(0f, 1f)] public float WaterOpacity = 0.85f;
        public Color WaterBaseColor = new Color(0.04f, 0.12f, 0.30f, 0.90f);
        public Color WaterShallowColor = new Color(0.10f, 0.50f, 0.60f, 0.60f);
        public Color WaterFoamColor = new Color(0.85f, 0.92f, 0.95f, 1.00f);

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
