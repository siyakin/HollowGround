using System.Collections.Generic;
using System.Linq;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.Quests
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        private readonly List<QuestInstance> _quests = new();

        public event System.Action<QuestInstance> OnQuestAccepted;
        public event System.Action<QuestInstance> OnQuestObjectiveUpdated;
        public event System.Action<QuestInstance> OnQuestCompleted;
        public event System.Action<QuestInstance> OnQuestTurnedIn;
        public event System.Action OnQuestListChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

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
            if (instance == null) return false;
            if (instance.Status != QuestStatus.Available) return false;

            instance.Status = QuestStatus.Active;
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
    }
}
