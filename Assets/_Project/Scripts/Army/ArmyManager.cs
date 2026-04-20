using System.Collections.Generic;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.Army
{
    public class ArmyManager : MonoBehaviour
    {
        public static ArmyManager Instance { get; private set; }

        private readonly Dictionary<TroopType, int> _troops = new();
        private readonly List<TrainingQueueEntry> _trainingQueue = new();

        public int TotalTroopCount { get; private set; }
        public int TotalArmyPower { get; private set; }
        public float Morale { get; private set; } = 1f;

        public event System.Action<TroopType, int> OnTroopCountChanged;
        public event System.Action OnArmyUpdated;
        public event System.Action<TrainingQueueEntry> OnTrainingStarted;
        public event System.Action<TrainingQueueEntry> OnTrainingCompleted;

        [System.Serializable]
        public class TrainingQueueEntry
        {
            public TroopData Data;
            public int Amount;
            public float RemainingTime;
            public float TotalTime;
            public bool IsComplete => RemainingTime <= 0f;
            public float Progress => TotalTime > 0 ? 1f - (RemainingTime / TotalTime) : 0f;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            foreach (TroopType type in System.Enum.GetValues(typeof(TroopType)))
            {
                _troops[type] = 0;
            }
        }

        private void Update()
        {
            ProcessTrainingQueue();
        }

        public int GetTroopCount(TroopType type)
        {
            return _troops.TryGetValue(type, out int val) ? val : 0;
        }

        public Dictionary<TroopType, int> GetAllTroops()
        {
            return new Dictionary<TroopType, int>(_troops);
        }

        public void AddTroops(TroopType type, int amount)
        {
            if (amount <= 0) return;
            _troops[type] = GetTroopCount(type) + amount;
            Recalculate();
            OnTroopCountChanged?.Invoke(type, _troops[type]);
            OnArmyUpdated?.Invoke();
        }

        public bool RemoveTroops(TroopType type, int amount)
        {
            if (GetTroopCount(type) < amount) return false;
            _troops[type] -= amount;
            Recalculate();
            OnTroopCountChanged?.Invoke(type, _troops[type]);
            OnArmyUpdated?.Invoke();
            return true;
        }

        public bool CanAffordTraining(TroopData data, int amount)
        {
            if (ResourceManager.Instance == null) return false;

            var cost = data.GetTrainingCost();
            foreach (var kvp in cost)
            {
                if (ResourceManager.Instance.Get(kvp.Key) < kvp.Value * amount)
                    return false;
            }
            return true;
        }

        public bool StartTraining(TroopData data, int amount)
        {
            if (!CanAffordTraining(data, amount)) return false;

            var cost = data.GetTrainingCost();
            foreach (var kvp in cost)
            {
                ResourceManager.Instance.Spend(kvp.Key, kvp.Value * amount);
            }

            var entry = new TrainingQueueEntry
            {
                Data = data,
                Amount = amount,
                RemainingTime = data.TrainingTime * amount,
                TotalTime = data.TrainingTime * amount
            };

            _trainingQueue.Add(entry);
            OnTrainingStarted?.Invoke(entry);
            return true;
        }

        public List<TrainingQueueEntry> GetTrainingQueue()
        {
            return new List<TrainingQueueEntry>(_trainingQueue);
        }

        private void ProcessTrainingQueue()
        {
            if (_trainingQueue.Count == 0) return;

            float speed = HollowGround.Core.TimeManager.Instance != null ? HollowGround.Core.TimeManager.Instance.GameSpeed : 1f;

            var entry = _trainingQueue[0];
            entry.RemainingTime -= Time.deltaTime * speed;

            if (entry.IsComplete)
            {
                AddTroops(entry.Data.Type, entry.Amount);
                OnTrainingCompleted?.Invoke(entry);
                _trainingQueue.RemoveAt(0);
            }
        }

        public float GetMorale()
        {
            return Morale;
        }

        public void UpdateMorale()
        {
            if (ResourceManager.Instance == null) return;

            float foodRatio = ResourceManager.Instance.Get(ResourceType.Food) /
                              (float)ResourceManager.Instance.GetCapacity(ResourceType.Food);
            float waterRatio = ResourceManager.Instance.Get(ResourceType.Water) /
                               (float)ResourceManager.Instance.GetCapacity(ResourceType.Water);

            Morale = Mathf.Clamp01((foodRatio + waterRatio) * 0.5f);
            OnArmyUpdated?.Invoke();
        }

        public int CalculateArmyPower()
        {
            int power = 0;
            foreach (var kvp in _troops)
            {
                // Temel güç = birlik sayısı * varsayılan atak gücü
                power += kvp.Value * 10;
            }
            power = Mathf.CeilToInt(power * Morale);
            TotalArmyPower = power;
            return power;
        }

        private void Recalculate()
        {
            TotalTroopCount = 0;
            foreach (var kvp in _troops)
                TotalTroopCount += kvp.Value;

            CalculateArmyPower();
        }
    }
}
