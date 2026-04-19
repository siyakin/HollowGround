using System;
using System.Collections.Generic;
using HollowGround.Army;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.Combat
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        private readonly List<ActiveExpedition> _expeditions = new();

        public event System.Action<ActiveExpedition> OnExpeditionStarted;
        public event System.Action<ActiveExpedition> OnExpeditionArrived;
        public event System.Action<BattleReport> OnBattleCompleted;

        [System.Serializable]
        public class ActiveExpedition
        {
            public string Name;
            public BattleTarget Target;
            public Dictionary<TroopType, int> SentTroops;
            public float RemainingTime;
            public float TotalTime;
            public bool IsTraveling => RemainingTime > 0;
            public float Progress => TotalTime > 0 ? 1f - (RemainingTime / TotalTime) : 1f;
        }

        [System.Serializable]
        public class BattleReport
        {
            public string TargetName;
            public bool Victory;
            public Dictionary<TroopType, int> AttackerLosses;
            public Dictionary<TroopType, int> Survivors;
            public Dictionary<ResourceType, int> Loot;
            public int TotalAttackerPower;
            public int TotalDefenderPower;
            public float PowerRatio;
        }

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
            ProcessExpeditions();
        }

        public bool CanSendExpedition(BattleTarget target, Dictionary<TroopType, int> troops)
        {
            if (ArmyManager.Instance == null) return false;

            foreach (var kvp in troops)
            {
                if (ArmyManager.Instance.GetTroopCount(kvp.Key) < kvp.Value)
                    return false;
            }
            return true;
        }

        public bool SendExpedition(BattleTarget target, Dictionary<TroopType, int> troops)
        {
            if (!CanSendExpedition(target, troops)) return false;
            if (ArmyManager.Instance == null) return false;

            foreach (var kvp in troops)
            {
                ArmyManager.Instance.RemoveTroops(kvp.Key, kvp.Value);
            }

            var expedition = new ActiveExpedition
            {
                Name = target.DisplayName,
                Target = target,
                SentTroops = new Dictionary<TroopType, int>(troops),
                RemainingTime = target.GetTravelTime(),
                TotalTime = target.GetTravelTime()
            };

            _expeditions.Add(expedition);
            OnExpeditionStarted?.Invoke(expedition);
            return true;
        }

        public List<ActiveExpedition> GetExpeditions()
        {
            return new List<ActiveExpedition>(_expeditions);
        }

        private void ProcessExpeditions()
        {
            for (int i = _expeditions.Count - 1; i >= 0; i--)
            {
                var exp = _expeditions[i];
                exp.RemainingTime -= Time.deltaTime;

                if (exp.RemainingTime <= 0f)
                {
                    _expeditions.RemoveAt(i);
                    ExecuteBattle(exp);
                }
            }
        }

        private void ExecuteBattle(ActiveExpedition expedition)
        {
            OnExpeditionArrived?.Invoke(expedition);

            var attacker = new BattleCalculator.BattleSide
            {
                Troops = expedition.SentTroops,
                Morale = ArmyManager.Instance != null ? ArmyManager.Instance.GetMorale() : 1f
            };

            var defender = new BattleCalculator.BattleSide
            {
                Troops = expedition.Target.GetDefenderArmy(),
                Morale = 1f
            };

            BattleCalculator.BattleResult result = BattleCalculator.Calculate(attacker, defender);

            var report = new BattleReport
            {
                TargetName = expedition.Target.DisplayName,
                Victory = result.AttackerWins,
                AttackerLosses = result.AttackerLosses,
                Survivors = result.AttackerSurvivors,
                Loot = result.AttackerWins ? expedition.Target.GetLoot() : new Dictionary<ResourceType, int>(),
                TotalAttackerPower = result.TotalAttackerPower,
                TotalDefenderPower = result.TotalDefenderPower,
                PowerRatio = result.PowerRatio
            };

            if (result.AttackerWins && ArmyManager.Instance != null)
            {
                foreach (var kvp in result.AttackerSurvivors)
                {
                    ArmyManager.Instance.AddTroops(kvp.Key, kvp.Value);
                }
            }

            if (result.AttackerWins && ResourceManager.Instance != null)
            {
                foreach (var kvp in report.Loot)
                {
                    ResourceManager.Instance.Add(kvp.Key, kvp.Value);
                }
            }

            OnBattleCompleted?.Invoke(report);
        }
    }
}
