using System;
using System.Collections.Generic;
using HollowGround.Army;
using HollowGround.Combat;
using UnityEngine;

namespace HollowGround.World
{
    public enum ExpeditionPhase
    {
        Traveling,
        Engaging,
        Returning,
        Completed
    }

    public class Expedition
    {
        public string Id { get; }
        public MapNodeData Target { get; }
        public Dictionary<TroopType, int> Troops { get; }
        public ExpeditionPhase Phase { get; private set; }

        public float TravelTime { get; private set; }
        public float RemainingTime { get; private set; }
        public float Progress => TravelTime > 0f ? 1f - (RemainingTime / TravelTime) : 1f;

        public bool IsReturning => Phase == ExpeditionPhase.Returning;
        public bool IsComplete => Phase == ExpeditionPhase.Completed;

        public BattleReport BattleResult { get; private set; }

        public event Action<Expedition> OnPhaseChanged;
        public event Action<Expedition> OnCompleted;

        public Expedition(MapNodeData target, Dictionary<TroopType, int> troops, float travelTime)
        {
            Id = Guid.NewGuid().ToString().Substring(0, 8);
            Target = target;
            Troops = new Dictionary<TroopType, int>(troops);
            TravelTime = travelTime;
            RemainingTime = travelTime;
            Phase = ExpeditionPhase.Traveling;
        }

        public void RestorePhase(ExpeditionPhase phase)
        {
            Phase = phase;
        }

        public void SetRemainingTime(float time)
        {
            RemainingTime = Mathf.Max(time, 0f);
        }

        public void Tick(float deltaTime)
        {
            if (Phase == ExpeditionPhase.Completed) return;

            RemainingTime -= deltaTime;
            if (RemainingTime <= 0f)
            {
                RemainingTime = 0f;
                AdvancePhase();
            }
        }

        private void AdvancePhase()
        {
            switch (Phase)
            {
                case ExpeditionPhase.Traveling:
                    if (Target.HasBattle)
                    {
                        SetPhase(ExpeditionPhase.Engaging);
                        ExecuteBattle();
                    }
                    else
                    {
                        SetPhase(ExpeditionPhase.Returning);
                        SetupReturn(TravelTime * 0.5f);
                    }
                    break;

                case ExpeditionPhase.Engaging:
                    SetPhase(ExpeditionPhase.Returning);
                    SetupReturn(TravelTime * 0.5f);
                    break;

                case ExpeditionPhase.Returning:
                    SetPhase(ExpeditionPhase.Completed);
                    ReturnTroops();
                    OnCompleted?.Invoke(this);
                    break;
            }
        }

        private void SetPhase(ExpeditionPhase newPhase)
        {
            if (Phase == newPhase) return;
            Phase = newPhase;
            OnPhaseChanged?.Invoke(this);
        }

        private void ExecuteBattle()
        {
            if (BattleManager.Instance == null || Target?.BattleTarget == null)
            {
                BattleResult = new BattleReport
                {
                    TargetName = Target?.DisplayName ?? "Unknown",
                    Victory = true,
                    AttackerLosses = new Dictionary<TroopType, int>(),
                    Survivors = new Dictionary<TroopType, int>(Troops),
                    Loot = new Dictionary<Resources.ResourceType, int>()
                };
                return;
            }

            var attacker = new BattleCalculator.BattleSide
            {
                Troops = Troops,
                Morale = ArmyManager.Instance?.GetMorale() ?? 1f
            };

            var defender = new BattleCalculator.BattleSide
            {
                Troops = Target.BattleTarget.GetDefenderArmy(),
                Morale = 1f
            };

            var result = BattleCalculator.Calculate(attacker, defender);

            BattleResult = new BattleReport
            {
                TargetName = Target.DisplayName,
                Victory = result.AttackerWins,
                AttackerLosses = result.AttackerLosses,
                Survivors = result.AttackerSurvivors,
                Loot = result.AttackerWins ? Target.BattleTarget.GetLoot() : new Dictionary<Resources.ResourceType, int>(),
                TotalAttackerPower = result.TotalAttackerPower,
                TotalDefenderPower = result.TotalDefenderPower,
                PowerRatio = result.PowerRatio
            };

            if (result.AttackerWins && Target != null)
            {
                Target.IsCleared = true;
                if (!Target.IsRepeatable)
                    Target.IsRepeatable = false;
            }
        }

        private void SetupReturn(float returnTime)
        {
            TravelTime = returnTime;
            RemainingTime = returnTime;
        }

        private void ReturnTroops()
        {
            if (ArmyManager.Instance == null) return;

            Dictionary<TroopType, int> survivors = BattleResult != null && BattleResult.Victory
                ? BattleResult.Survivors
                : Troops;

            foreach (var kvp in survivors)
            {
                if (kvp.Value > 0)
                    ArmyManager.Instance.AddTroops(kvp.Key, kvp.Value);
            }

            if (BattleResult?.Victory == true && BattleResult.Loot != null)
            {
                foreach (var kvp in BattleResult.Loot)
                {
                    Resources.ResourceManager.Instance?.Add(kvp.Key, kvp.Value);
                }
            }
        }

        public void SkipToArrival()
        {
            RemainingTime = 0f;
            while (Phase != ExpeditionPhase.Completed)
                AdvancePhase();
        }
    }
}
