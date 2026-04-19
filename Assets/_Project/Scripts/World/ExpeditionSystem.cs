using System;
using System.Collections.Generic;
using HollowGround.Army;
using HollowGround.Combat;
using UnityEngine;

namespace HollowGround.World
{
    public class ExpeditionSystem : MonoBehaviour
    {
        public static ExpeditionSystem Instance { get; private set; }

        private readonly List<ActiveExpedition> _expeditions = new();

        public List<ActiveExpedition> Expeditions => _expeditions;

        public event Action<ActiveExpedition> OnExpeditionLaunched;
        public event Action<ActiveExpedition> OnExpeditionCompleted;
        public event Action OnExpeditionUpdated;

        [Serializable]
        public class ActiveExpedition
        {
            public string Id;
            public Vector2Int TargetPosition;
            public string TargetName;
            public BattleTarget BattleTarget;
            public Dictionary<TroopType, int> Troops;
            public float TravelTime;
            public float RemainingTime;
            public bool IsReturning;
            public float Progress => TravelTime > 0 ? 1f - (RemainingTime / TravelTime) : 1f;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            for (int i = _expeditions.Count - 1; i >= 0; i--)
            {
                var exp = _expeditions[i];
                exp.RemainingTime -= Time.deltaTime;

                if (exp.RemainingTime <= 0f)
                {
                    _expeditions.RemoveAt(i);

                    if (!exp.IsReturning)
                    {
                        CompleteExpedition(exp);
                    }
                    else
                    {
                        ReturnExpedition(exp);
                    }
                }
            }

            OnExpeditionUpdated?.Invoke();
        }

        public bool CanSendExpedition(Vector2Int target, Dictionary<TroopType, int> troops)
        {
            if (ArmyManager.Instance == null) return false;
            if (WorldMap.Instance == null) return false;

            var node = WorldMap.Instance.GetNode(target);
            if (node == null || !node.IsVisible) return false;

            foreach (var kvp in troops)
            {
                if (ArmyManager.Instance.GetTroopCount(kvp.Key) < kvp.Value)
                    return false;
            }

            return true;
        }

        public bool SendExpedition(Vector2Int target, Dictionary<TroopType, int> troops)
        {
            if (!CanSendExpedition(target, troops)) return false;

            foreach (var kvp in troops)
                ArmyManager.Instance.RemoveTroops(kvp.Key, kvp.Value);

            var node = WorldMap.Instance.GetNode(target);
            float distance = WorldMap.Instance.GetDistance(WorldMap.Instance.BasePosition, target);
            float travelTime = distance * 30f;

            var expedition = new ActiveExpedition
            {
                Id = Guid.NewGuid().ToString().Substring(0, 8),
                TargetPosition = target,
                TargetName = node.DisplayName,
                BattleTarget = node.BattleTarget,
                Troops = new Dictionary<TroopType, int>(troops),
                TravelTime = travelTime,
                RemainingTime = travelTime,
                IsReturning = false
            };

            _expeditions.Add(expedition);
            OnExpeditionLaunched?.Invoke(expedition);
            return true;
        }

        private void CompleteExpedition(ActiveExpedition expedition)
        {
            WorldMap.Instance.ExploreNode(expedition.TargetPosition);

            if (expedition.BattleTarget != null && BattleManager.Instance != null)
            {
                BattleManager.Instance.SendExpedition(expedition.BattleTarget, expedition.Troops);
            }
            else
            {
                expedition.IsReturning = true;
                expedition.RemainingTime = expedition.TravelTime * 0.5f;
                expedition.TravelTime = expedition.TravelTime * 0.5f;
                _expeditions.Add(expedition);
            }

            OnExpeditionCompleted?.Invoke(expedition);
        }

        private void ReturnExpedition(ActiveExpedition expedition)
        {
            if (ArmyManager.Instance != null)
            {
                foreach (var kvp in expedition.Troops)
                    ArmyManager.Instance.AddTroops(kvp.Key, kvp.Value);
            }
        }

        public List<ActiveExpedition> GetActiveExpeditions()
        {
            return new List<ActiveExpedition>(_expeditions);
        }
    }
}
