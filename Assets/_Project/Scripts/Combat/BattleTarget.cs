using System.Collections.Generic;
using HollowGround.Army;
using HollowGround.Buildings;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.Combat
{
    public enum TargetType
    {
        MutantCamp,
        AbandonedBuilding,
        NPCSettlement,
        RadioactiveZone,
        BossArea
    }

    [CreateAssetMenu(fileName = "BattleTarget", menuName = "HollowGround/BattleTarget")]
    public class BattleTarget : ScriptableObject
    {
        [Header("Info")]
        public string DisplayName;
        public string Description;
        public TargetType Type;
        public int Difficulty = 1;

        [Header("Distance")]
        public float Distance = 1f;

        [Header("Defender Troops")]
        public List<TroopCount> DefenderTroops = new();

        [Header("Loot")]
        public List<BuildingData.CostEntry> LootResources = new();

        [System.Serializable]
        public class TroopCount
        {
            public TroopType Type;
            public int Count;
        }

        public Dictionary<TroopType, int> GetDefenderArmy()
        {
            var army = new Dictionary<TroopType, int>();
            foreach (var tc in DefenderTroops)
                army[tc.Type] = tc.Count * Difficulty;
            return army;
        }

        public Dictionary<ResourceType, int> GetLoot()
        {
            var loot = new Dictionary<ResourceType, int>();
            foreach (var entry in LootResources)
                loot[entry.Type] = entry.Amount * Difficulty;
            return loot;
        }

        public float GetTravelTime()
        {
            return Distance * 30f;
        }
    }
}
