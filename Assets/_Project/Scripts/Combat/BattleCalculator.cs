using System;
using System.Collections.Generic;
using HollowGround.Army;
using HollowGround.Domain.Combat;
using UnityEngine;

namespace HollowGround.Combat
{
    public static class BattleCalculator
    {
        public static float RandomVariance
        {
            get => BattleCalc.RandomVariance;
            set => BattleCalc.RandomVariance = value;
        }

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
            var domainAtk = ToDomainSide(attacker);
            var domainDef = ToDomainSide(defender);

            var result = BattleCalc.Calculate(domainAtk, domainDef);

            return new BattleResult
            {
                AttackerWins = result.AttackerWins,
                AttackerLosses = FromDomainDict(result.AttackerLosses),
                DefenderLosses = FromDomainDict(result.DefenderLosses),
                AttackerSurvivors = FromDomainDict(result.AttackerSurvivors),
                DefenderSurvivors = FromDomainDict(result.DefenderSurvivors),
                TotalAttackerPower = result.TotalAttackerPower,
                TotalDefenderPower = result.TotalDefenderPower,
                PowerRatio = result.PowerRatio
            };
        }

        private static BattleCalc.BattleSide ToDomainSide(BattleSide side)
        {
            var troops = new Dictionary<int, int>();
            foreach (var kvp in side.Troops)
                troops[(int)kvp.Key] = kvp.Value;

            return new BattleCalc.BattleSide
            {
                Troops = troops,
                ArmyPower = side.ArmyPower,
                Morale = side.Morale
            };
        }

        private static Dictionary<TroopType, int> FromDomainDict(Dictionary<int, int> dict)
        {
            var result = new Dictionary<TroopType, int>();
            foreach (var kvp in dict)
                result[(TroopType)kvp.Key] = kvp.Value;
            return result;
        }
    }
}
