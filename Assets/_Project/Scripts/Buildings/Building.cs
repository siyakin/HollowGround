using System;
using System.Collections.Generic;
using HollowGround.Core;
using HollowGround.Grid;
using HollowGround.Resources;
using HollowGround.UI;
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

            var col = gameObject.AddComponent<BoxCollider>();
            col.size = new Vector3(data.SizeX * GridSystem.Instance.CellSize, 5f, data.SizeZ * GridSystem.Instance.CellSize);
            col.center = new Vector3(0f, 2.5f, 0f);

            gameObject.layer = LayerMask.NameToLayer("Building");

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
            float speed = HollowGround.Core.TimeManager.Instance != null ? HollowGround.Core.TimeManager.Instance.GameSpeed : 1f;
            float buildTime = _data.GetBuildTimeForLevel(Level);
            ConstructionProgress += Time.deltaTime * speed / buildTime;

            if (ConstructionProgress >= 1f)
            {
                ConstructionProgress = 1f;
                State = BuildingState.Active;
                OnConstructionComplete?.Invoke(this);
                ToastUI.Show($"{_data.DisplayName} construction complete!", Color.green);
            }
        }

        private void TickUpgrade()
        {
            float speed = HollowGround.Core.TimeManager.Instance != null ? HollowGround.Core.TimeManager.Instance.GameSpeed : 1f;
            float buildTime = _data.GetBuildTimeForLevel(Level + 1);
            UpgradeProgress += Time.deltaTime * speed / buildTime;

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

            float speed = HollowGround.Core.TimeManager.Instance != null ? HollowGround.Core.TimeManager.Instance.GameSpeed : 1f;
            _productionTimer += Time.deltaTime * speed;

            float productionInterval = _data.ProductionInterval;
            float productionBonus = GetTotalProductionBonus();
            if (productionBonus > 0f)
                productionInterval *= (1f - productionBonus);

            if (_productionTimer >= productionInterval)
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

        private float GetTotalProductionBonus()
        {
            float bonus = 0f;
            var rm = HollowGround.Tech.ResearchManager.Instance;
            if (rm == null) return 0f;
            foreach (var tech in rm.GetResearchedTechs())
                bonus += tech.ProductionBonus;
            return Mathf.Clamp01(bonus);
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
            if (_data.StorageCapacity > 0 && BuildingManager.Instance != null)
                BuildingManager.Instance.RefreshStorageCapacities();
        }

        public float GetProductionProgress()
        {
            if (!_data.HasProduction) return 0f;
            return _productionTimer / _data.ProductionInterval;
        }
    }
}
