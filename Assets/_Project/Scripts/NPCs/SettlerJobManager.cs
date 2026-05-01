using System.Collections.Generic;
using System.Linq;
using HollowGround.Buildings;
using HollowGround.Core;
using UnityEngine;

namespace HollowGround.NPCs
{
    public class SettlerJobManager : Singleton<SettlerJobManager>
    {
        private readonly List<SettlerWalker> _allSettlers = new();
        private readonly Dictionary<Building, List<SettlerWalker>> _buildingWorkers = new();

        public int IdleCount => _allSettlers.Count(s => s != null && s.Role == SettlerRole.None);
        public int WorkingCount => _allSettlers.Count(s => s != null && s.Role != SettlerRole.None);
        public event System.Action<SettlerWalker, Building> OnJobAssigned;
        public event System.Action<SettlerWalker, Building> OnJobReleased;

        private void Start()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            var bm = BuildingManager.Instance;
            if (bm != null)
            {
                bm.OnBuildingAdded += OnBuildingAdded;
                bm.OnBuildingRemoved += OnBuildingRemoved;
            }
        }

        private void OnBuildingAdded(Building building)
        {
            building.OnConstructionComplete += OnBuildingActivated;
            building.OnDestroyed += OnBuildingDestroyed;
            building.OnRepaired += OnBuildingRepaired;
        }

        private void OnBuildingRemoved(Building building)
        {
            building.OnConstructionComplete -= OnBuildingActivated;
            building.OnDestroyed -= OnBuildingDestroyed;
            building.OnRepaired -= OnBuildingRepaired;
        }

        private void OnBuildingActivated(Building building)
        {
            if (building.Data.RequiredWorkers == null || building.Data.RequiredWorkers.Count == 0) return;
            AssignWorkersToBuilding(building);
        }

        private void OnBuildingDestroyed(Building building)
        {
            ReleaseWorkersFromBuilding(building);
        }

        private void OnBuildingRepaired(Building building)
        {
            if (building.Data.RequiredWorkers == null || building.Data.RequiredWorkers.Count == 0) return;
            AssignWorkersToBuilding(building);
        }

        public void RegisterSettler(SettlerWalker walker)
        {
            if (walker == null || _allSettlers.Contains(walker)) return;
            _allSettlers.Add(walker);
            TryAssignBestJob(walker);
        }

        public void UnregisterSettler(SettlerWalker walker)
        {
            if (walker == null) return;
            _allSettlers.Remove(walker);
            RemoveWorkerFromBuilding(walker);
        }

        public int GetAssignedWorkerCount(Building building)
        {
            if (building == null || !_buildingWorkers.TryGetValue(building, out var list)) return 0;
            return list.Count;
        }

        public int GetAssignedWorkerCountForRole(Building building, SettlerRole role)
        {
            if (building == null || !_buildingWorkers.TryGetValue(building, out var list)) return 0;
            return list.Count(w => w.Role == role);
        }

        public float GetWorkerFillRatio(Building building)
        {
            if (building == null) return 0f;
            int required = building.Data?.GetTotalRequiredWorkers() ?? 0;
            if (required == 0) return 1f;
            int assigned = GetAssignedWorkerCount(building);
            return Mathf.Min(1f, (float)assigned / required);
        }

        private void TryAssignBestJob(SettlerWalker walker)
        {
            if (walker.Role != SettlerRole.None) return;

            var needs = GetUnmetWorkerNeeds();
            foreach (var need in needs)
            {
                int currentCount = GetAssignedWorkerCountForRole(need.Building, need.Role);
                if (currentCount < need.RequiredCount)
                {
                    AssignWorker(walker, need.Role, need.Building);
                    return;
                }
            }
        }

        private void AssignWorkersToBuilding(Building building)
        {
            if (building.Data.RequiredWorkers == null) return;

            foreach (var slot in building.Data.RequiredWorkers)
            {
                int currentCount = GetAssignedWorkerCountForRole(building, slot.Role);
                int missing = slot.Count - currentCount;

                for (int i = 0; i < missing; i++)
                {
                    SettlerWalker idle = FindIdleSettler();
                    if (idle == null) return;
                    AssignWorker(idle, slot.Role, building);
                }
            }
        }

        private void AssignWorker(SettlerWalker walker, SettlerRole role, Building building)
        {
            if (!_buildingWorkers.ContainsKey(building))
                _buildingWorkers[building] = new List<SettlerWalker>();

            _buildingWorkers[building].Add(walker);
            building.AssignedWorkerCount = GetAssignedWorkerCount(building);

            walker.AssignJob(role, building);
            OnJobAssigned?.Invoke(walker, building);
        }

        private void ReleaseWorkersFromBuilding(Building building)
        {
            if (!_buildingWorkers.TryGetValue(building, out var workers)) return;

            var toRelease = workers.ToList();
            foreach (var walker in toRelease)
            {
                var previousBuilding = walker.AssignedBuilding;
                walker.ClearJob();
                OnJobReleased?.Invoke(walker, previousBuilding);
            }

            workers.Clear();
            _buildingWorkers.Remove(building);
            building.AssignedWorkerCount = 0;

            foreach (var walker in toRelease)
                TryAssignBestJob(walker);
        }

        private void RemoveWorkerFromBuilding(SettlerWalker walker)
        {
            if (walker.AssignedBuilding == null) return;

            if (_buildingWorkers.TryGetValue(walker.AssignedBuilding, out var list))
            {
                list.Remove(walker);
                if (walker.AssignedBuilding != null)
                    walker.AssignedBuilding.AssignedWorkerCount = list.Count;
            }
        }

        private SettlerWalker FindIdleSettler()
        {
            return _allSettlers.FirstOrDefault(s => s != null && s.Role == SettlerRole.None && s.IsActive);
        }

        private List<WorkerNeed> GetUnmetWorkerNeeds()
        {
            var needs = new List<WorkerNeed>();

            if (BuildingManager.Instance == null) return needs;

            foreach (var building in BuildingManager.Instance.AllBuildings)
            {
                if (building.State != BuildingState.Active) continue;
                if (building.Data.RequiredWorkers == null) continue;

                foreach (var slot in building.Data.RequiredWorkers)
                {
                    int current = GetAssignedWorkerCountForRole(building, slot.Role);
                    int missing = slot.Count - current;
                    if (missing > 0)
                    {
                        needs.Add(new WorkerNeed
                        {
                            Building = building,
                            Role = slot.Role,
                            RequiredCount = slot.Count,
                            MissingCount = missing,
                            Priority = GetBuildingPriority(building.Data.Type)
                        });
                    }
                }
            }

            needs.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            return needs;
        }

        private static int GetBuildingPriority(BuildingType type)
        {
            return type switch
            {
                BuildingType.Farm => 0,
                BuildingType.WaterWell => 1,
                BuildingType.Mine => 2,
                BuildingType.Generator => 3,
                BuildingType.WoodFactory => 4,
                BuildingType.ResearchLab => 5,
                BuildingType.Hospital => 6,
                BuildingType.Workshop => 7,
                BuildingType.Storage => 8,
                BuildingType.Barracks => 9,
                BuildingType.WatchTower => 10,
                BuildingType.TradeCenter => 11,
                BuildingType.CommandCenter => 12,
                BuildingType.Shelter => 13,
                BuildingType.Walls => 14,
                _ => 15
            };
        }

        public void RebuildAssignmentsFromLoad()
        {
            _buildingWorkers.Clear();

            foreach (var walker in _allSettlers.ToList())
            {
                if (walker == null) continue;

                if (walker.AssignedBuilding != null && walker.Role != SettlerRole.None)
                {
                    var building = walker.AssignedBuilding;
                    if (!_buildingWorkers.ContainsKey(building))
                        _buildingWorkers[building] = new List<SettlerWalker>();
                    _buildingWorkers[building].Add(walker);
                    building.AssignedWorkerCount = _buildingWorkers[building].Count;
                }
            }
        }

        public List<SettlerWalker> GetAllSettlers() => _allSettlers;

        public Dictionary<Building, List<SettlerWalker>> GetAllAssignments() => _buildingWorkers;

        private struct WorkerNeed
        {
            public Building Building;
            public SettlerRole Role;
            public int RequiredCount;
            public int MissingCount;
            public int Priority;
        }
    }
}
