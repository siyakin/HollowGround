using System;
using System.Collections.Generic;
using HollowGround.Army;
using UnityEngine;

namespace HollowGround.Combat
{
    public static class BattleCalculator
    {
        public static float RandomVariance = 0.1f;

        public struct BattleSide
        {
            public Dictionary<TroopType, int> Troops;
            public int ArmyPower;
            public float Morale;
        }

        public struct BattleResult
        {
            public bool AttackerWins;
            public Dictionary<TroopType, int> AttackerLosses;
            public Dictionary<TroopType, int> DefenderLosses;
            public Dictionary<TroopType, int> AttackerSurvivors;
            public Dictionary<TroopType, int> DefenderSurvivors;
            public int TotalAttackerPower;
            public int TotalDefenderPower;
            public float PowerRatio;
        }

        public static BattleResult Calculate(BattleSide attacker, BattleSide defender)
        {
            int atkPower = CalculateSidePower(attacker);
            int defPower = CalculateSidePower(defender);

            float randomFactor = UnityEngine.Random.Range(1f - RandomVariance, 1f + RandomVariance);
            atkPower = Mathf.CeilToInt(atkPower * randomFactor);

            randomFactor = UnityEngine.Random.Range(1f - RandomVariance, 1f + RandomVariance);
            defPower = Mathf.CeilToInt(defPower * randomFactor);

            float ratio = defPower > 0 ? (float)atkPower / defPower : 10f;
            bool attackerWins = atkPower > defPower;

            var atkLosses = CalculateLosses(attacker.Troops, attackerWins ? ratio : 1f / ratio, attackerWins);
            var defLosses = CalculateLosses(defender.Troops, attackerWins ? 1f / ratio : ratio, !attackerWins);

            var atkSurvivors = GetSurvivors(attacker.Troops, atkLosses);
            var defSurvivors = GetSurvivors(defender.Troops, defLosses);

            return new BattleResult
            {
                AttackerWins = attackerWins,
                AttackerLosses = atkLosses,
                DefenderLosses = defLosses,
                AttackerSurvivors = atkSurvivors,
                DefenderSurvivors = defSurvivors,
                TotalAttackerPower = atkPower,
                TotalDefenderPower = defPower,
                PowerRatio = ratio
            };
        }

        private static int CalculateSidePower(BattleSide side)
        {
            int power = 0;
            foreach (var kvp in side.Troops)
            {
                power += kvp.Value * 10;
            }
            power = Mathf.CeilToInt(power * side.Morale);
            return power;
        }

        private static Dictionary<TroopType, int> CalculateLosses(
            Dictionary<TroopType, int> troops, float ratio, bool won)
        {
            var losses = new Dictionary<TroopType, int>();

            float baseLossRate = won
                ? Mathf.Clamp(0.1f / Mathf.Max(ratio, 0.1f), 0.02f, 0.4f)
                : Mathf.Clamp(0.3f * (1f + 1f / Mathf.Max(ratio, 0.1f)), 0.3f, 0.9f);

            foreach (var kvp in troops)
            {
                float unitLoss = baseLossRate;

                float roll = UnityEngine.Random.Range(0.8f, 1.2f);
                int lost = Mathf.CeilToInt(kvp.Value * unitLoss * roll);
                lost = Mathf.Clamp(lost, 0, kvp.Value);

                losses[kvp.Key] = lost;
            }

            return losses;
        }

        private static Dictionary<TroopType, int> GetSurvivors(
            Dictionary<TroopType, int> troops, Dictionary<TroopType, int> losses)
        {
            var survivors = new Dictionary<TroopType, int>();

            foreach (var kvp in troops)
            {
                int lost = losses.TryGetValue(kvp.Key, out int l) ? l : 0;
                survivors[kvp.Key] = Mathf.Max(0, kvp.Value - lost);
            }

            return survivors;
        }
    }
}
