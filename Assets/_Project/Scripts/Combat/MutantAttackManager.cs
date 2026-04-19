using System;
using System.Collections.Generic;
using System.Linq;
using HollowGround.Army;
using HollowGround.Buildings;
using HollowGround.Core;
using HollowGround.Quests;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.Combat
{
    public class MutantAttackManager : MonoBehaviour
    {
        public static MutantAttackManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float _baseAttackInterval = 600f;
        [SerializeField] private float _intervalGrowthFactor = 0.1f;
        [SerializeField] private int _baseWavePower = 50;
        [SerializeField] private float _powerGrowthFactor = 1.2f;

        private float _attackTimer;
        private int _currentWaveIndex;
        private bool _warningActive;
        private float _warningDuration = 30f;
        private float _warningTimer;

        private MutantWave _pendingWave;

        public int CurrentWave => _currentWaveIndex;
        public bool IsWarningActive => _warningActive;
        public float TimeUntilAttack => _attackTimer;
        public float WarningTimeRemaining => _warningTimer;
        public bool IsUnderAttack { get; private set; }

        public event System.Action<MutantWave> OnWaveWarning;
        public event System.Action<MutantWave> OnWaveStarted;
        public event System.Action<MutantWave, bool> OnWaveEnded;
        public event System.Action<float> OnAttackTimerChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
                return;

            if (IsUnderAttack) return;

            if (_warningActive)
            {
                _warningTimer -= Time.deltaTime;
                if (_warningTimer <= 0f)
                {
                    ExecuteWave(_pendingWave);
                }
                return;
            }

            _attackTimer -= Time.deltaTime;
            OnAttackTimerChanged?.Invoke(_attackTimer);

            if (_attackTimer <= 0f)
            {
                TriggerWarning();
            }
        }

        public void StartAttackCycle()
        {
            _currentWaveIndex = 0;
            ResetTimer();
        }

        private void ResetTimer()
        {
            float interval = _baseAttackInterval * Mathf.Pow(1f + _intervalGrowthFactor, _currentWaveIndex);
            _attackTimer = interval;
        }

        private void TriggerWarning()
        {
            _warningActive = true;
            _warningTimer = _warningDuration;
            _pendingWave = GenerateWave(_currentWaveIndex);
            OnWaveWarning?.Invoke(_pendingWave);
        }

        private void ExecuteWave(MutantWave wave)
        {
            _warningActive = false;
            IsUnderAttack = true;

            OnWaveStarted?.Invoke(wave);

            int defenderPower = CalculateDefensePower();
            int attackerPower = wave.MutantPower;
            bool victory = defenderPower >= attackerPower;

            float ratio = attackerPower > 0 ? (float)defenderPower / attackerPower : 999f;

            if (victory)
            {
                int survivorRate = Mathf.Clamp(Mathf.CeilToInt(ratio * 40), 30, 100);
                float troopLoss = (100 - survivorRate) / 100f;
                ApplyTroopLosses(troopLoss);

                GrantRewards(wave);

                if (QuestManager.Instance != null)
                    QuestManager.Instance.ProgressObjective(ObjectiveType.SurviveWaves, "", 1);
            }
            else
            {
                ApplyPenalties(wave);
                ApplyTroopLosses(0.6f);
                ApplyBuildingDamage();
            }

            _currentWaveIndex++;
            IsUnderAttack = false;
            ResetTimer();
            OnWaveEnded?.Invoke(wave, victory);
        }

        private MutantWave GenerateWave(int waveIndex)
        {
            var wave = ScriptableObject.CreateInstance<MutantWave>();
            wave.DisplayName = $"Mutant Wave {waveIndex + 1}";
            wave.WaveNumber = waveIndex + 1;
            wave.MutantCount = 5 + waveIndex * 3;
            wave.MutantPower = Mathf.CeilToInt(_baseWavePower * Mathf.Pow(_powerGrowthFactor, waveIndex));
            wave.Description = $"{wave.MutantCount} mutants approaching with power {wave.MutantPower}.";
            return wave;
        }

        private int CalculateDefensePower()
        {
            int power = 0;

            if (ArmyManager.Instance != null)
                power += ArmyManager.Instance.CalculateArmyPower();

            if (BuildingManager.Instance != null)
            {
                int walls = BuildingManager.Instance.GetBuildingCount(BuildingType.Barracks);
                power += walls * 20;
            }

            float defenseBonus = 0f;
            if (HollowGround.Tech.ResearchManager.Instance != null)
            {
                var researched = HollowGround.Tech.ResearchManager.Instance.GetResearchedTechs();
                foreach (var tech in researched)
                    defenseBonus += tech.DefenseBonus;
            }

            power = Mathf.CeilToInt(power * (1f + defenseBonus));
            return power;
        }

        private void ApplyTroopLosses(float lossRatio)
        {
            if (ArmyManager.Instance == null) return;

            var troops = ArmyManager.Instance.GetAllTroops();
            foreach (var kvp in troops)
            {
                int losses = Mathf.CeilToInt(kvp.Value * lossRatio);
                if (losses > 0)
                    ArmyManager.Instance.RemoveTroops(kvp.Key, losses);
            }
        }

        private void ApplyPenalties(MutantWave wave)
        {
            if (ResourceManager.Instance == null) return;

            var penalty = wave.GetPenaltyMap();
            foreach (var kvp in penalty)
            {
                int current = ResourceManager.Instance.Get(kvp.Key);
                int loss = Mathf.Min(kvp.Value, current);
                ResourceManager.Instance.Spend(kvp.Key, loss);
            }
        }

        private void GrantRewards(MutantWave wave)
        {
            if (ResourceManager.Instance == null) return;

            var rewards = wave.GetRewardMap();
            foreach (var kvp in rewards)
                ResourceManager.Instance.Add(kvp.Key, kvp.Value);
        }

        private void ApplyBuildingDamage()
        {
            if (BuildingManager.Instance == null) return;

            var buildings = BuildingManager.Instance.AllBuildings;
            if (buildings.Count == 0) return;

            int damageCount = Mathf.Max(1, buildings.Count / 4);
            var rng = new System.Random();

            for (int i = 0; i < damageCount && buildings.Count > 0; i++)
            {
                int idx = rng.Next(buildings.Count);
                var b = buildings[idx];
                if (b.Data.Type != BuildingType.CommandCenter)
                    b.Demolish();
            }
        }
    }
}
