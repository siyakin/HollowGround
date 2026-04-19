using System.Collections.Generic;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.Quests
{
    [CreateAssetMenu(fileName = "QuestData", menuName = "HollowGround/QuestData")]
    public class QuestData : ScriptableObject
    {
        [Header("Info")]
        public string DisplayName;
        [TextArea] public string Description;
        public QuestType QuestType;
        public Sprite Icon;
        public int RecommendedLevel = 1;

        [Header("Chain")]
        public List<QuestData> Prerequisites = new();
        public QuestData NextQuest;

        [Header("Objectives")]
        public List<QuestObjective> Objectives = new();

        [Header("Rewards")]
        public List<ResourceReward> Rewards = new();
        public int XPReward;
        public int RelationReward;
        public List<QuestData> UnlocksQuests = new();

        [System.Serializable]
        public class QuestObjective
        {
            public ObjectiveType Type;
            public string TargetId;
            public int RequiredAmount = 1;
            public string Description;
        }

        [System.Serializable]
        public class ResourceReward
        {
            public ResourceType Type;
            public int Amount;
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
}
