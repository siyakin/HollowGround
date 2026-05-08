using System.Collections.Generic;
using System.Linq;
using HollowGround.Army;
using HollowGround.Buildings;
using HollowGround.Core;
using HollowGround.Resources;
using HollowGround.World;
using UnityEngine;

namespace HollowGround.Quests
{
    public class QuestManager : Singleton<QuestManager>
    {
        private readonly List<QuestInstance> _quests = new();

        public event System.Action<QuestInstance> OnQuestAccepted;
        public event System.Action<QuestInstance> OnQuestObjectiveUpdated;
        public event System.Action<QuestInstance> OnQuestCompleted;
        public event System.Action<QuestInstance> OnQuestTurnedIn;
        public event System.Action OnQuestListChanged;

        public void LoadQuests(List<QuestData> questPool)
        {
            foreach (var questData in questPool)
            {
                var instance = new QuestInstance(questData);
                instance.Status = QuestStatus.Locked;
                _quests.Add(instance);
            }
            RefreshAvailability();
        }

        public List<QuestInstance> GetAllQuests() => new(_quests);

        public List<QuestInstance> GetQuestsByStatus(QuestStatus status)
        {
            return _quests.Where(q => q.Status == status).ToList();
        }

        public List<QuestInstance> GetAvailableQuests()
        {
            return _quests.Where(q => q.Status == QuestStatus.Available).ToList();
        }

        public List<QuestInstance> GetActiveQuests()
        {
            return _quests.Where(q => q.Status == QuestStatus.Active).ToList();
        }

        public bool AcceptQuest(QuestData data)
        {
            var instance = _quests.FirstOrDefault(q => q.Data == data);
            if (instance == null)
            {
                Debug.LogWarning($"[QuestManager] AcceptQuest: quest not found for {data?.name ?? "null"}");
                return false;
            }
            if (instance.Status != QuestStatus.Available)
            {
                Debug.LogWarning($"[QuestManager] AcceptQuest: {data.name} status={instance.Status}, expected Available");
                return false;
            }

            instance.Status = QuestStatus.Active;
            Debug.Log($"[QuestManager] Quest accepted: {data.name} -> Active");
            CheckExistingProgress(instance);
            OnQuestAccepted?.Invoke(instance);
            OnQuestListChanged?.Invoke();
            return true;
        }

        public void ProgressObjective(ObjectiveType type, string targetId, int amount)
        {
            foreach (var quest in _quests)
            {
                if (quest.Status != QuestStatus.Active) continue;

                for (int i = 0; i < quest.Data.Objectives.Count; i++)
                {
                    var obj = quest.Data.Objectives[i];
                    if (obj.Type != type) continue;
                    if (!string.IsNullOrEmpty(obj.TargetId) && obj.TargetId != targetId) continue;

                    quest.AddProgress(i, amount);
                    OnQuestObjectiveUpdated?.Invoke(quest);

                    if (quest.IsComplete())
                    {
                        quest.Status = QuestStatus.Completed;
                        OnQuestCompleted?.Invoke(quest);
                    }
                }
            }
            OnQuestListChanged?.Invoke();
        }

        public bool TurnInQuest(QuestData data)
        {
            var instance = _quests.FirstOrDefault(q => q.Data == data);
            if (instance == null) return false;
            if (instance.Status != QuestStatus.Completed) return false;

            instance.Status = QuestStatus.TurnedIn;

            GrantRewards(instance);

            if (instance.Data.NextQuest != null)
            {
                var nextInst = _quests.FirstOrDefault(q => q.Data == instance.Data.NextQuest);
                if (nextInst != null)
                    nextInst.Status = QuestStatus.Available;
            }

            foreach (var unlock in instance.Data.UnlocksQuests)
            {
                var unlockInst = _quests.FirstOrDefault(q => q.Data == unlock);
                if (unlockInst != null && unlockInst.Status == QuestStatus.Locked)
                    unlockInst.Status = QuestStatus.Available;
            }

            OnQuestTurnedIn?.Invoke(instance);
            RefreshAvailability();
            OnQuestListChanged?.Invoke();
            return true;
        }

        private void GrantRewards(QuestInstance instance)
        {
            if (ResourceManager.Instance == null) return;

            var rewards = instance.Data.GetRewardMap();
            foreach (var kvp in rewards)
            {
                ResourceManager.Instance.Add(kvp.Key, kvp.Value);
            }
        }

        private void RefreshAvailability()
        {
            foreach (var quest in _quests)
            {
                if (quest.Status != QuestStatus.Locked) continue;

                bool allMet = true;
                foreach (var prereq in quest.Data.Prerequisites)
                {
                    var prereqInst = _quests.FirstOrDefault(q => q.Data == prereq);
                    if (prereqInst == null || prereqInst.Status != QuestStatus.TurnedIn)
                    {
                        allMet = false;
                        break;
                    }
                }

                if (allMet)
                {
                    quest.Status = QuestStatus.Available;
                    OnQuestListChanged?.Invoke();
                }
            }
        }

        private void CheckExistingProgress(QuestInstance quest)
        {
            for (int i = 0; i < quest.Data.Objectives.Count; i++)
            {
                var obj = quest.Data.Objectives[i];
                int existing = QueryExistingCount(obj);
                if (existing > 0)
                {
                    quest.AddProgress(i, existing);
                    OnQuestObjectiveUpdated?.Invoke(quest);
                }
            }

            if (quest.IsComplete())
            {
                quest.Status = QuestStatus.Completed;
                OnQuestCompleted?.Invoke(quest);
                OnQuestListChanged?.Invoke();
            }
        }

        private int QueryExistingCount(QuestData.QuestObjective obj)
        {
            switch (obj.Type)
            {
                case ObjectiveType.BuildBuilding:
                    return CountBuildings(obj.TargetId);

                case ObjectiveType.GatherResource:
                    return GetResourceAmount(obj.TargetId);

                case ObjectiveType.TrainTroops:
                    return GetTroopCount(obj.TargetId);

                case ObjectiveType.ResearchTech:
                    return CountResearchedTech(obj.TargetId);

                case ObjectiveType.TradeWithFaction:
                    return 0;

                case ObjectiveType.ExploreNodes:
                    return CountExploredNodes();

                default:
                    return 0;
            }
        }

        private int CountBuildings(string targetId)
        {
            if (BuildingManager.Instance == null) return 0;
            int count = 0;
            foreach (var b in BuildingManager.Instance.AllBuildings)
            {
                if (b.Data == null) continue;
                if (b.Data.DisplayName == targetId || b.Data.name == targetId)
                    count++;
            }
            return count;
        }

        private int GetResourceAmount(string targetId)
        {
            if (ResourceManager.Instance == null) return 0;
            if (!System.Enum.TryParse<ResourceType>(targetId, out var type)) return 0;
            return ResourceManager.Instance.Get(type);
        }

        private int GetTroopCount(string targetId)
        {
            if (ArmyManager.Instance == null) return 0;
            if (!System.Enum.TryParse<TroopType>(targetId, out var type)) return 0;
            return ArmyManager.Instance.GetTroopCount(type);
        }

        private int CountResearchedTech(string targetId)
        {
            if (Tech.ResearchManager.Instance == null) return 0;
            foreach (var tech in Tech.ResearchManager.Instance.GetResearchedTechs())
            {
                if (tech.name == targetId || tech.DisplayName == targetId)
                    return 1;
            }
            return 0;
        }

        private int CountExploredNodes()
        {
            if (WorldMap.Instance == null) return 0;
            int count = 0;
            foreach (var node in WorldMap.Instance.AllNodes)
            {
                if (node.IsExplored) count++;
            }
            return count;
        }
    }
}
