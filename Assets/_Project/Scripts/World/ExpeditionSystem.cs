using System;
using System.Collections.Generic;
using System.Linq;
using HollowGround.Army;
using HollowGround.Combat;
using HollowGround.Core;
using UnityEngine;

namespace HollowGround.World
{
    public class ExpeditionSystem : Singleton<ExpeditionSystem>
    {
        private readonly List<Expedition> _expeditions = new();

        public IReadOnlyList<Expedition> Expeditions => _expeditions;

        public event Action<Expedition> OnExpeditionLaunched;
        public event Action<Expedition> OnExpeditionPhaseChanged;
        public event Action<Expedition> OnExpeditionCompleted;

        private void Update()
        {
            if (TimeManager.Instance != null && TimeManager.Instance.IsPaused) return;

            float dt = Time.deltaTime * (TimeManager.Instance?.GameSpeed ?? 1f);

            for (int i = _expeditions.Count - 1; i >= 0; i--)
            {
                var exp = _expeditions[i];
                exp.Tick(dt);

                if (exp.IsComplete)
                {
                    _expeditions.RemoveAt(i);
                }
            }
        }

        public bool CanSendExpedition(Vector2Int target, Dictionary<TroopType, int> troops)
        {
            if (ArmyManager.Instance == null) return false;
            if (WorldMap.Instance == null) return false;

            var node = WorldMap.Instance.GetNode(target);
            if (node == null || (!node.IsVisible && !node.IsExplored)) return false;

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
            float travelTime = CalculateTravelTime(target);

            var expedition = new Expedition(node, troops, travelTime);
            expedition.OnPhaseChanged += HandlePhaseChanged;
            expedition.OnCompleted += HandleCompleted;

            _expeditions.Add(expedition);
            OnExpeditionLaunched?.Invoke(expedition);

            WorldMap.Instance.RefreshVisibility();

            return true;
        }

        public float CalculateTravelTime(Vector2Int target)
        {
            if (WorldMap.Instance == null) return 30f;

            float pathCost;
            var path = WorldMap.Instance.FindPath(WorldMap.Instance.BasePosition, target);
            if (path.Count > 0)
            {
                pathCost = 0f;
                foreach (var pos in path)
                {
                    var node = WorldMap.Instance.GetNode(pos);
                    pathCost += node != null ? WorldMap.GetNodeMoveCost(node) : 1f;
                }
            }
            else
            {
                // Fallback to Euclidean if no path found
                pathCost = WorldMap.Instance.GetDistance(WorldMap.Instance.BasePosition, target);
            }

            float baseTime = pathCost * 30f;

            float devMult = GameConfig.Instance?.GetExpeditionTimeMultiplier ?? 1f;
            baseTime *= devMult;

            float expeditionBonus = 0f;
            if (Tech.ResearchManager.Instance != null)
                expeditionBonus = Tech.ResearchManager.Instance.GetTotalExpeditionSpeedBonus();
            baseTime *= (1f - expeditionBonus);

            return Mathf.Max(baseTime, 1f);
        }

        public List<Expedition> GetActiveExpeditions()
        {
            return new List<Expedition>(_expeditions);
        }

        public Expedition GetExpedition(string id)
        {
            return _expeditions.FirstOrDefault(e => e.Id == id);
        }

        public void RestoreExpedition(Expedition expedition)
        {
            if (expedition == null) return;
            expedition.OnPhaseChanged += HandlePhaseChanged;
            expedition.OnCompleted += HandleCompleted;
            _expeditions.Add(expedition);
        }

        public void CancelAllExpeditions()
        {
            foreach (var exp in _expeditions)
            {
                exp.OnPhaseChanged -= HandlePhaseChanged;
                exp.OnCompleted -= HandleCompleted;
            }
            _expeditions.Clear();
        }

        private void HandlePhaseChanged(Expedition expedition)
        {
            OnExpeditionPhaseChanged?.Invoke(expedition);

            if (expedition.Phase == ExpeditionPhase.Engaging)
            {
                if (expedition.Target != null && !expedition.Target.IsExplored)
                    WorldMap.Instance?.ExploreNode(expedition.Target.GridPosition);

                if (expedition.BattleResult != null)
                    BattleManager.Instance?.PublishBattleReport(expedition.BattleResult);
            }

            if (WorldMap.Instance != null)
                WorldMap.Instance.RefreshVisibility();
        }

        private void HandleCompleted(Expedition expedition)
        {
            expedition.OnPhaseChanged -= HandlePhaseChanged;
            expedition.OnCompleted -= HandleCompleted;

            if (WorldMap.Instance != null)
                WorldMap.Instance.RefreshVisibility();

            OnExpeditionCompleted?.Invoke(expedition);
        }
    }
}
