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
        private readonly List<Building> _constructing = new();
        private readonly List<Building> _producing = new();
        private readonly List<Building> _upgrading = new();
        private readonly List<Building> _destroyed = new();
        private Building _commandCenter;
        private float _cachedProductionBonus = -1f;
        private bool _productionBonusDirty = true;

        public List<Building> AllBuildings => _buildings;
        public Building CommandCenter => _commandCenter;
        public int TotalPopulationCapacity => _buildings.Sum(b => b.Data.PopulationCapacity * (b.State == BuildingState.Active ? b.Level : 0));
        public int TotalStorageCapacity => _buildings.Sum(b => b.Data.StorageCapacity * (b.State == BuildingState.Active ? b.Level : 0));

        public event System.Action<Building> OnBuildingAdded;
        public event System.Action<Building> OnBuildingRemoved;
        public event System.Action<int> OnCommandCenterLevelChanged;

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            float dt = Time.deltaTime;
            float speed = TimeManager.Instance != null ? TimeManager.Instance.GameSpeed : 1f;
            float buildDevMult = GameConfig.Instance != null ? GameConfig.Instance.GetBuildTimeMultiplier : 1f;

            TickConstructing(dt, speed, buildDevMult);
            TickUpgrading(dt, speed, buildDevMult);
            TickProducing(dt, speed);
            TickDestroyed(dt);
        }

        private void TickConstructing(float dt, float speed, float devMult)
        {
            for (int i = _constructing.Count - 1; i >= 0; i--)
            {
                var b = _constructing[i];
                b.TickConstruction(dt, speed, devMult);
            }
        }

        private void TickUpgrading(float dt, float speed, float devMult)
        {
            for (int i = _upgrading.Count - 1; i >= 0; i--)
            {
                var b = _upgrading[i];
                b.TickUpgrade(dt, speed, devMult);
            }
        }

        private void TickProducing(float dt, float speed)
        {
            if (_productionBonusDirty)
            {
                _cachedProductionBonus = Tech.ResearchManager.Instance != null
                    ? Tech.ResearchManager.Instance.GetTotalProductionBonus()
                    : 0f;
                _productionBonusDirty = false;
            }

            for (int i = 0; i < _producing.Count; i++)
                _producing[i].TickProduction(dt, speed, _cachedProductionBonus);
        }

        private void TickDestroyed(float dt)
        {
            for (int i = _destroyed.Count - 1; i >= 0; i--)
            {
                var b = _destroyed[i];
                b.TickDestroyed(dt);
                if (b == null)
                    _destroyed.RemoveAt(i);
            }
        }

        private void AddToStateList(Building building)
        {
            switch (building.State)
            {
                case BuildingState.Constructing:
                    _constructing.Add(building);
                    break;
                case BuildingState.Active:
                    if (building.Data.HasProduction)
                        _producing.Add(building);
                    break;
                case BuildingState.Upgrading:
                    _upgrading.Add(building);
                    break;
                case BuildingState.Destroyed:
                    _destroyed.Add(building);
                    break;
            }
        }

        private void RemoveFromStateList(Building building)
        {
            _constructing.Remove(building);
            _producing.Remove(building);
            _upgrading.Remove(building);
            _destroyed.Remove(building);
        }

        public void RegisterBuilding(Building building)
        {
            if (_buildings.Contains(building)) return;

            _buildings.Add(building);
            AddToStateList(building);

            if (building.Data.Type == BuildingType.CommandCenter)
            {
                _commandCenter = building;
                OnCommandCenterLevelChanged?.Invoke(building.Level);
            }

            building.OnUpgradeComplete += HandleBuildingUpgraded;
            building.OnDestroyed += HandleBuildingDestroyed;
            building.OnStateChanged += HandleBuildingStateChanged;

            UpdateStorageCapacities();
            OnBuildingAdded?.Invoke(building);
        }

        public void UnregisterBuilding(Building building)
        {
            _buildings.Remove(building);
            RemoveFromStateList(building);
            building.OnUpgradeComplete -= HandleBuildingUpgraded;
            building.OnDestroyed -= HandleBuildingDestroyed;
            building.OnStateChanged -= HandleBuildingStateChanged;

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
            if (ResourceManager.Instance == null) return true;
            var costs = data.GetCostForLevel(level);
            return ResourceManager.Instance.CanAfford(costs);
        }

        protected override void OnDestroy()
        {
            if (Tech.ResearchManager.Instance != null)
                Tech.ResearchManager.Instance.OnResearchCompleted -= InvalidateProductionBonus;
            base.OnDestroy();
        }

        private void InvalidateProductionBonus(Tech.TechNode _)
        {
            _productionBonusDirty = true;
        }

        private void Start()
        {
            if (Tech.ResearchManager.Instance != null)
                Tech.ResearchManager.Instance.OnResearchCompleted += InvalidateProductionBonus;
        }

        private void HandleBuildingUpgraded(Building building)
        {
            UpdateStorageCapacities();

            if (building.Data.Type == BuildingType.CommandCenter)
                OnCommandCenterLevelChanged?.Invoke(building.Level);
        }

        private void HandleBuildingStateChanged(Building building, BuildingState newState)
        {
            RemoveFromStateList(building);
            AddToStateList(building);
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

            int totalStorage = TotalStorageCapacity;
            if (totalStorage <= 0) return;

            foreach (ResourceType resType in System.Enum.GetValues(typeof(ResourceType)))
                ResourceManager.Instance.SetCapacity(resType, totalStorage);
        }
    }
}
