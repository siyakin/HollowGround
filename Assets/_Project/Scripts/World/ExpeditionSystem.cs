using System;
using System.Collections.Generic;
using HollowGround.Army;
using HollowGround.Combat;
using HollowGround.Core;
using UnityEngine;

namespace HollowGround.World
{
    public class ExpeditionSystem : Singleton<ExpeditionSystem>
    {

        private readonly List<ActiveExpedition> _expeditions = new();

        public List<ActiveExpedition> Expeditions => _expeditions;

        public event Action<ActiveExpedition> OnExpeditionLaunched;
        public event Action<ActiveExpedition> OnExpeditionCompleted;

        [Serializable]
        public class TroopEntry
        {
            public TroopType Type;
            public int Count;
        }

        [Serializable]
        public class ActiveExpedition
        {
            public string Id;
            public Vector2Int TargetPosition;
            public string TargetName;
            public BattleTarget BattleTarget;
            public List<TroopEntry> Troops;
            public float TravelTime;
            public float RemainingTime;
            public bool IsReturning;
            public float Progress => TravelTime > 0 ? 1f - (RemainingTime / TravelTime) : 1f;

            public Dictionary<TroopType, int> TroopsLookup
            {
                get
                {
                    var dict = new Dictionary<TroopType, int>();
                    if (Troops != null)
                        foreach (var e in Troops)
                            dict[e.Type] = e.Count;
                    return dict;
                }
            }
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
            float devMult = HollowGround.Core.GameConfig.Instance != null ? HollowGround.Core.GameConfig.Instance.GetExpeditionTimeMultiplier : 1f;
            travelTime *= devMult;

            float expeditionBonus = 0f;
            if (HollowGround.Tech.ResearchManager.Instance != null)
                expeditionBonus = HollowGround.Tech.ResearchManager.Instance.GetTotalExpeditionSpeedBonus();
            travelTime *= (1f - expeditionBonus);

            var expedition = new ActiveExpedition
            {
                Id = Guid.NewGuid().ToString().Substring(0, 8),
                TargetPosition = target,
                TargetName = node.DisplayName,
                BattleTarget = node.BattleTarget,
                Troops = ToTroopEntries(troops),
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
                BattleManager.Instance.SendExpedition(expedition.BattleTarget, expedition.TroopsLookup);
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
            if (ArmyManager.Instance != null && expedition.Troops != null)
            {
                foreach (var entry in expedition.Troops)
                    ArmyManager.Instance.AddTroops(entry.Type, entry.Count);
            }
        }

        public List<ActiveExpedition> GetActiveExpeditions()
        {
            return new List<ActiveExpedition>(_expeditions);
        }

        private static List<TroopEntry> ToTroopEntries(Dictionary<TroopType, int> troops)
        {
            var list = new List<TroopEntry>(troops.Count);
            foreach (var kvp in troops)
                list.Add(new TroopEntry { Type = kvp.Key, Count = kvp.Value });
            return list;
        }
    }
}
