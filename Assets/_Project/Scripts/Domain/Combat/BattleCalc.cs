using System;
using System.Collections.Generic;

namespace HollowGround.Domain.Combat
{
    public static class BattleCalc
    {
        public static float RandomVariance = 0.1f;

        public struct BattleSide
        {
            public Dictionary<int, int> Troops;
            public int ArmyPower;
            public float Morale;
        }

        public struct BattleResult
        {
            public bool AttackerWins;
            public Dictionary<int, int> AttackerLosses;
            public Dictionary<int, int> DefenderLosses;
            public Dictionary<int, int> AttackerSurvivors;
            public Dictionary<int, int> DefenderSurvivors;
            public int TotalAttackerPower;
            public int TotalDefenderPower;
            public float PowerRatio;
        }

        public static BattleResult Calculate(BattleSide attacker, BattleSide defender, System.Random rng = null)
        {
            rng ??= new System.Random();

            int atkPower = CalculateSidePower(attacker);
            int defPower = CalculateSidePower(defender);

            float atkRoll = 1f - RandomVariance + (float)(rng.NextDouble() * 2 * RandomVariance);
            atkPower = (int)MathF.Ceiling(atkPower * atkRoll);

            float defRoll = 1f - RandomVariance + (float)(rng.NextDouble() * 2 * RandomVariance);
            defPower = (int)MathF.Ceiling(defPower * defRoll);

            float ratio = defPower > 0 ? (float)atkPower / defPower : 10f;
            bool attackerWins = atkPower > defPower;

            var atkLosses = CalculateLosses(attacker.Troops, attackerWins ? ratio : 1f / ratio, attackerWins, rng);
            var defLosses = CalculateLosses(defender.Troops, attackerWins ? 1f / ratio : ratio, !attackerWins, rng);

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

        public static int CalculateSidePower(BattleSide side)
        {
            int power = 0;
            foreach (var kvp in side.Troops)
                power += kvp.Value * 10;

            power = (int)MathF.Ceiling(power * side.Morale);
            return power;
        }

        public static Dictionary<int, int> CalculateLosses(
            Dictionary<int, int> troops, float ratio, bool won, System.Random rng = null)
        {
            rng ??= new System.Random();
            var losses = new Dictionary<int, int>();

            float baseLossRate = won
                ? Math.Clamp(0.1f / Math.Max(ratio, 0.1f), 0.02f, 0.4f)
                : Math.Clamp(0.3f * (1f + 1f / Math.Max(ratio, 0.1f)), 0.3f, 0.9f);

            foreach (var kvp in troops)
            {
                float roll = 0.8f + (float)(rng.NextDouble() * 0.4f);
                int lost = (int)MathF.Ceiling(kvp.Value * baseLossRate * roll);
                lost = Math.Clamp(lost, 0, kvp.Value);

                losses[kvp.Key] = lost;
            }

            return losses;
        }

        public static Dictionary<int, int> GetSurvivors(
            Dictionary<int, int> troops, Dictionary<int, int> losses)
        {
            var survivors = new Dictionary<int, int>();

            foreach (var kvp in troops)
            {
                int lost = losses.TryGetValue(kvp.Key, out int l) ? l : 0;
                survivors[kvp.Key] = Math.Max(0, kvp.Value - lost);
            }

            return survivors;
        }
    }
}
