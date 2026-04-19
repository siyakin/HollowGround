using System;
using System.Collections.Generic;
using HollowGround.Buildings;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.Army
{
    [CreateAssetMenu(fileName = "TroopData", menuName = "HollowGround/TroopData")]
    public class TroopData : ScriptableObject
    {
        [Header("Info")]
        public string DisplayName;
        public string Description;
        public TroopType Type;
        public TroopRole Role;
        public GameObject UnitPrefab;

        [Header("Stats (Base)")]
        public int BaseHP = 100;
        public int BaseAttack = 10;
        public int BaseDefense = 5;
        public float BaseSpeed = 3f;

        [Header("Matchup Bonus")]
        public TroopType StrongAgainst;
        public TroopType WeakAgainst;
        public float MatchupMultiplier = 1.5f;

        [Header("Training Cost")]
        public List<BuildingData.CostEntry> TrainingCost = new();
        public float TrainingTime = 30f;

        [Header("Barracks Requirement")]
        public int BarracksLevelRequired = 1;

        public int GetHP(int level) => Mathf.CeilToInt(BaseHP * (1f + (level - 1) * 0.1f));
        public int GetAttack(int level) => Mathf.CeilToInt(BaseAttack * (1f + (level - 1) * 0.08f));
        public int GetDefense(int level) => Mathf.CeilToInt(BaseDefense * (1f + (level - 1) * 0.08f));

        public Dictionary<ResourceType, int> GetTrainingCost()
        {
            var result = new Dictionary<ResourceType, int>();
            foreach (var entry in TrainingCost)
                result[entry.Type] = entry.Amount;
            return result;
        }
    }
}
