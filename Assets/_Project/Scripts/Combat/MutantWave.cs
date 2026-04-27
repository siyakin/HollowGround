using System.Collections.Generic;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.Combat
{
    [CreateAssetMenu(fileName = "MutantWave", menuName = "HollowGround/MutantWave")]
    public class MutantWave : ScriptableObject
    {
        [Header("Info")]
        public string DisplayName;
        public int WaveNumber;
        [TextArea] public string Description;

        [Header("Enemy Forces")]
        public int MutantCount;
        public int MutantPower;
        public List<PenaltyEntry> PenaltyOnLoss = new();

        [Header("Loot")]
        public List<ResourceReward> Rewards = new();

        [System.Serializable]
        public class PenaltyEntry
        {
            public ResourceType Type;
            public int Amount;
        }

        [System.Serializable]
        public class ResourceReward
        {
            public ResourceType Type;
            public int Amount;
        }

        public Dictionary<ResourceType, int> GetPenaltyMap()
        {
            var result = new Dictionary<ResourceType, int>();
            foreach (var p in PenaltyOnLoss)
            {
                if (result.ContainsKey(p.Type))
                    result[p.Type] += p.Amount;
                else
                    result[p.Type] = p.Amount;
            }
            return result;
        }

        public Dictionary<ResourceType, int> GetRewardMap()
        {
            var result = new Dictionary<ResourceType, int>();
            foreach (var r in Rewards)
            {
                if (result.ContainsKey(r.Type))
                    result[r.Type] += r.Amount;
                else
                    result[r.Type] = r.Amount;
            }
            return result;
        }
    }

    public class MutantWaveData
    {
        public string DisplayName;
        public int WaveNumber;
        public string Description;
        public int MutantCount;
        public int MutantPower;

        private static readonly Dictionary<ResourceType, int> DefaultPenalties = new()
        {
            { ResourceType.Food, 30 },
            { ResourceType.Metal, 20 }
        };

        private static readonly Dictionary<ResourceType, int> DefaultRewards = new()
        {
            { ResourceType.TechPart, 5 },
            { ResourceType.Food, 25 }
        };

        public Dictionary<ResourceType, int> GetPenaltyMap() => DefaultPenalties;
        public Dictionary<ResourceType, int> GetRewardMap() => DefaultRewards;
    }
}
