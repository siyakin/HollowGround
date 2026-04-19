using System;
using System.Collections.Generic;
using HollowGround.Core;
using HollowGround.Grid;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.Buildings
{
    public enum BuildingState
    {
        Placing,
        Constructing,
        Active,
        Upgrading,
        Damaged
    }

    public class Building : MonoBehaviour
    {
        [SerializeField] private BuildingData _data;

        public BuildingData Data => _data;
        public int Level { get; private set; } = 1;
        public BuildingState State { get; private set; } = BuildingState.Constructing;
        public Vector2Int GridOrigin { get; private set; }
        public float ConstructionProgress { get; private set; }
        public float UpgradeProgress { get; private set; }

        private float _productionTimer;
        private GameObject _currentModel;

        public event Action<Building> OnConstructionComplete;
        public event Action<Building> OnUpgradeComplete;
        public event Action<Building, ResourceType, int> OnProduced;
        public event Action<Building> OnDestroyed;

        public void Initialize(BuildingData data, Vector2Int gridOrigin)
        {
            _data = data;
            GridOrigin = gridOrigin;
            Level = 1;
            State = BuildingState.Constructing;
            ConstructionProgress = 0f;
            UpdateModel();
        }

        private void Update()
        {
            switch (State)
            {
                case BuildingState.Constructing:
                    TickConstruction();
                    break;
                case BuildingState.Active:
                    TickProduction();
                    break;
                case BuildingState.Upgrading:
                    TickUpgrade();
                    break;
            }
        }

        private void TickConstruction()
        {
            float buildTime = _data.GetBuildTimeForLevel(Level);
            ConstructionProgress += Time.deltaTime / buildTime;

            if (ConstructionProgress >= 1f)
            {
                ConstructionProgress = 1f;
                State = BuildingState.Active;
                OnConstructionComplete?.Invoke(this);
            }
        }

        private void TickUpgrade()
        {
            float buildTime = _data.GetBuildTimeForLevel(Level + 1);
            UpgradeProgress += Time.deltaTime / buildTime;

            if (UpgradeProgress >= 1f)
            {
                UpgradeProgress = 1f;
                Level++;
                State = BuildingState.Active;
                UpdateModel();
                ApplyLevelEffects();
                OnUpgradeComplete?.Invoke(this);
            }
        }

        private void TickProduction()
        {
            if (!_data.HasProduction) return;

            _productionTimer += Time.deltaTime;
            if (_productionTimer >= _data.ProductionInterval)
            {
                _productionTimer = 0f;
                int amount = _data.GetProductionForLevel(Level);
                ResourceType resType = _data.ProducedResource;

                if (ResourceManager.Instance != null)
                {
                    ResourceManager.Instance.Add(resType, amount);
                    OnProduced?.Invoke(this, resType, amount);
                }
            }
        }

        public bool CanUpgrade()
        {
            return Level < _data.MaxLevel && State == BuildingState.Active;
        }

        public bool StartUpgrade()
        {
            if (!CanUpgrade()) return false;

            var costs = _data.GetCostForLevel(Level + 1);
            if (ResourceManager.Instance == null || !ResourceManager.Instance.CanAfford(costs))
                return false;

            ResourceManager.Instance.SpendMultiple(costs);
            State = BuildingState.Upgrading;
            UpgradeProgress = 0f;
            return true;
        }

        public void Demolish()
        {
            var gridSystem = GridSystem.Instance;
            if (gridSystem != null)
            {
                gridSystem.FreeCells(GridOrigin.x, GridOrigin.y, _data.SizeX, _data.SizeZ);
            }

            float refundRatio = 0.5f;
            var costs = _data.GetCostForLevel(Level);
            if (ResourceManager.Instance != null)
            {
                foreach (var kvp in costs)
                {
                    ResourceManager.Instance.Add(kvp.Key, Mathf.CeilToInt(kvp.Value * refundRatio));
                }
            }

            OnDestroyed?.Invoke(this);
            Destroy(gameObject);
        }

        private void UpdateModel()
        {
            if (_currentModel != null)
                Destroy(_currentModel);

            GameObject prefab = _data.GetPrefabForLevel(Level);
            if (prefab != null)
            {
                _currentModel = Instantiate(prefab, transform);
                _currentModel.transform.localPosition = Vector3.zero;
            }
        }

        private void ApplyLevelEffects()
        {
            if (_data.PopulationCapacity > 0 && GameManager.Instance != null)
            {
                // Population capacity will be handled by BuildingManager
            }

            if (_data.StorageCapacity > 0 && ResourceManager.Instance != null)
            {
                // Storage capacity updates will be handled by BuildingManager
            }
        }

        public float GetProductionProgress()
        {
            if (!_data.HasProduction) return 0f;
            return _productionTimer / _data.ProductionInterval;
        }
    }
}
