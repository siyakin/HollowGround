using System.Collections.Generic;
using UnityEngine;

namespace HollowGround.Quests
{
    public class QuestInstance
    {
        public QuestData Data { get; }
        public QuestStatus Status { get; set; }
        public Dictionary<int, int> Progress { get; } = new();

        public QuestInstance(QuestData data)
        {
            Data = data;
            Status = QuestStatus.Available;
            for (int i = 0; i < data.Objectives.Count; i++)
                Progress[i] = 0;
        }

        public bool IsComplete()
        {
            for (int i = 0; i < Data.Objectives.Count; i++)
            {
                if (Progress[i] < Data.Objectives[i].RequiredAmount)
                    return false;
            }
            return true;
        }

        public float GetProgress()
        {
            if (Data.Objectives.Count == 0) return 0f;

            float total = 0f;
            for (int i = 0; i < Data.Objectives.Count; i++)
            {
                float objProgress = Mathf.Min(
                    (float)Progress[i] / Data.Objectives[i].RequiredAmount, 1f);
                total += objProgress;
            }
            return total / Data.Objectives.Count;
        }

        public void AddProgress(int objectiveIndex, int amount)
        {
            if (objectiveIndex < 0 || objectiveIndex >= Data.Objectives.Count) return;
            Progress[objectiveIndex] = Mathf.Min(
                Progress[objectiveIndex] + amount,
                Data.Objectives[objectiveIndex].RequiredAmount);
        }
    }
}
