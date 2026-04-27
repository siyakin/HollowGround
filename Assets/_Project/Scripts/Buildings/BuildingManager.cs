using System.Collections.Generic;
using System.Linq;
using HollowGround.Core;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.Buildings
{
    public class BuildingManager : Singleton<BuildingManager>
    {
        private readonly List<Building> _buildings = new();
        private Building _commandCenter;

        public List<Building> AllBuildings => _buildings;
        public Building CommandCenter => _commandCenter;
        public int TotalPopulationCapacity => _buildings.Sum(b => b.Data.PopulationCapacity * (b.State == BuildingState.Active ? b.Level : 0));
        public int TotalStorageCapacity => _buildings.Sum(b => b.Data.StorageCapacity * (b.State == BuildingState.Active ? b.Level : 0));

        public event System.Action<Building> OnBuildingAdded;
        public event System.Action<Building> OnBuildingRemoved;
        public event System.Action<int> OnCommandCenterLevelChanged;

        public void RegisterBuilding(Building building)
        {
            if (_buildings.Contains(building)) return;

            _buildings.Add(building);

            if (building.Data.Type == BuildingType.CommandCenter)
                _commandCenter = building;

            building.OnUpgradeComplete += HandleBuildingUpgraded;
            building.OnDestroyed += HandleBuildingDestroyed;

            UpdateStorageCapacities();
            OnBuildingAdded?.Invoke(building);
        }

        public void UnregisterBuilding(Building building)
        {
            _buildings.Remove(building);
            building.OnUpgradeComplete -= HandleBuildingUpgraded;
            building.OnDestroyed -= HandleBuildingDestroyed;

            UpdateStorageCapacities();
            OnBuildingRemoved?.Invoke(building);
        }

        public List<Building> GetBuildingsOfType(BuildingType type)
        {
            return _buildings.Where(b => b.Data.Type == type).ToList();
        }

        public int GetBuildingCount(BuildingType type)
        {
            return _buildings.Count(b => b.Data.Type == type);
        }

        public int GetCommandCenterLevel()
        {
            return _commandCenter != null ? _commandCenter.Level : 0;
        }

        public bool CanBuild(BuildingData data)
        {
            if (data.CommandCenterLevelRequired > GetCommandCenterLevel())
                return false;

            if (!CanAffordBuilding(data, 1))
                return false;

            return true;
        }

        public bool CanAffordBuilding(BuildingData data, int level)
        {
            var costs = data.GetCostForLevel(level);
            return ResourceManager.Instance != null && ResourceManager.Instance.CanAfford(costs);
        }

        private void HandleBuildingUpgraded(Building building)
        {
            UpdateStorageCapacities();

            if (building.Data.Type == BuildingType.CommandCenter)
                OnCommandCenterLevelChanged?.Invoke(building.Level);
        }

        private void HandleBuildingDestroyed(Building building)
        {
            UnregisterBuilding(building);
        }

        public void RefreshStorageCapacities()
        {
            UpdateStorageCapacities();
        }

        private void UpdateStorageCapacities()
        {
            if (ResourceManager.Instance == null) return;

            foreach (ResourceType resType in System.Enum.GetValues(typeof(ResourceType)))
            {
                int totalStorage = TotalStorageCapacity;
                if (totalStorage > 0)
                    ResourceManager.Instance.SetCapacity(resType, totalStorage);
            }
        }
    }
}
