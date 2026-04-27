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
    public class MutantAttackManager : Singleton<MutantAttackManager>
    {

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

        private MutantWaveData _pendingWave;

        public int CurrentWave => _currentWaveIndex;
        public bool IsWarningActive => _warningActive;
        public float TimeUntilAttack => _attackTimer;
        public float WarningTimeRemaining => _warningTimer;
        public bool IsUnderAttack { get; private set; }

        public event System.Action<MutantWaveData> OnWaveWarning;
        public event System.Action<MutantWaveData> OnWaveStarted;
        public event System.Action<MutantWaveData, bool> OnWaveEnded;
        public event System.Action<float> OnAttackTimerChanged;

        private void Update()
        {
            if (HollowGround.Core.GameConfig.Instance != null && HollowGround.Core.GameConfig.Instance.DisableMutantAttacks)
                return;

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
            float devMult = HollowGround.Core.GameConfig.Instance != null ? HollowGround.Core.GameConfig.Instance.GetMutantAttackIntervalMultiplier : 1f;
            _attackTimer = interval * devMult;
        }

        private void TriggerWarning()
        {
            _warningActive = true;
            _warningTimer = _warningDuration;
            _pendingWave = GenerateWave(_currentWaveIndex);
            OnWaveWarning?.Invoke(_pendingWave);
        }

        private void ExecuteWave(MutantWaveData wave)
        {
            _warningActive = false;
            IsUnderAttack = true;

            OnWaveStarted?.Invoke(wave);

            int defenderPower = CalculateDefensePower();
            int attackerPower = wave.MutantPower;
            bool victory = defenderPower >= attackerPower;

            float ratio = attackerPower > 0 ? (float)defenderPower / attackerPower : 999f;
            int damagedCount = 0;

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
                ApplyTroopLosses(GameConfig.Instance != null ? GameConfig.Instance.DefeatTroopLossRatio : 0.6f);
                damagedCount = ApplyBuildingDamage();
            }

            _currentWaveIndex++;
            IsUnderAttack = false;
            ResetTimer();
            OnWaveEnded?.Invoke(wave, victory);
        }

        private MutantWaveData GenerateWave(int waveIndex)
        {
            var wave = new MutantWaveData
            {
                DisplayName = $"Mutant Wave {waveIndex + 1}",
                WaveNumber = waveIndex + 1,
                MutantCount = 5 + waveIndex * 3,
                MutantPower = Mathf.CeilToInt(_baseWavePower * Mathf.Pow(_powerGrowthFactor, waveIndex)),
                Description = $"{5 + waveIndex * 3} mutants approaching with power {Mathf.CeilToInt(_baseWavePower * Mathf.Pow(_powerGrowthFactor, waveIndex))}."
            };
            return wave;
        }

        private int CalculateDefensePower()
        {
            int power = 0;

            if (ArmyManager.Instance != null)
                power += ArmyManager.Instance.CalculateArmyPower();

            if (BuildingManager.Instance != null)
            {
                int walls = BuildingManager.Instance.GetBuildingCount(BuildingType.Walls);
                int wallBonus = GameConfig.Instance != null ? GameConfig.Instance.WallDefenseBonus : 20;
                power += walls * wallBonus;
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

        private void ApplyPenalties(MutantWaveData wave)
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

        private void GrantRewards(MutantWaveData wave)
        {
            if (ResourceManager.Instance == null) return;

            var rewards = wave.GetRewardMap();
            foreach (var kvp in rewards)
                ResourceManager.Instance.Add(kvp.Key, kvp.Value);
        }

        private int ApplyBuildingDamage()
        {
            if (BuildingManager.Instance == null) return 0;

            var buildings = BuildingManager.Instance.AllBuildings;
            if (buildings.Count == 0) return 0;

            int damageCount = Mathf.Max(1, buildings.Count / 4);
            var rng = new System.Random();
            var toDamage = new List<Building>(buildings);
            int actuallyDamaged = 0;

            for (int i = 0; i < damageCount && toDamage.Count > 0; i++)
            {
                int idx = rng.Next(toDamage.Count);
                var b = toDamage[idx];
                toDamage.RemoveAt(idx);

                if (b.Data.Type == BuildingType.CommandCenter) continue;

                if (b.State != BuildingState.Damaged && b.State != BuildingState.Destroyed)
                {
                    b.ApplyDamage();
                    actuallyDamaged++;
                }
            }

            return actuallyDamaged;
        }
    }
}
