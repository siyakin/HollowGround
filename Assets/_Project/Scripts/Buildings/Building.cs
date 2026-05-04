using System;
using System.Collections.Generic;
using HollowGround.Core;
using HollowGround.Domain.Production;
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
        Damaged,
        Destroyed
    }

    public class Building : MonoBehaviour
    {
        [SerializeField] private BuildingData _data;

        public BuildingData Data => _data;
        public int Level { get; private set; } = 1;
        public BuildingState State { get; private set; } = BuildingState.Constructing;
        public Vector2Int GridOrigin { get; private set; }
        public int Rotation { get; private set; }
        public float ConstructionProgress { get; private set; }
        public float UpgradeProgress { get; private set; }
        public int AssignedWorkerCount { get; set; }

        private float _productionTimer;
        private GameObject _currentModel;
        private float _destroyedTimer;
        private const float DestroyedDisplayDuration = 2.5f;
        private const float GroundYOffset = 0.05f;

        public event Action<Building> OnConstructionComplete;
        public event Action<Building> OnUpgradeComplete;
        public event Action<Building, ResourceType, int> OnProduced;
        public event Action<Building> OnDestroyed;
        public event Action<Building> OnDamaged;
        public event Action<Building> OnRepaired;

        public void Initialize(BuildingData data, Vector2Int gridOrigin, int rotation = 0)
        {
            _data = data;
            GridOrigin = gridOrigin;
            Rotation = rotation;
            Level = 1;
            State = BuildingState.Constructing;
            ConstructionProgress = 0f;

            var (sx, sz) = GetRotatedFootprint();
            var col = gameObject.AddComponent<BoxCollider>();
            col.size = new Vector3(sx * GridSystem.Instance.CellSize, 5f, sz * GridSystem.Instance.CellSize);
            col.center = new Vector3(0f, 2.5f, 0f);

            gameObject.layer = LayerMask.NameToLayer("Building");

            gameObject.AddComponent<BuildingHighlight>();
            gameObject.AddComponent<DamageEffects>();

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
                case BuildingState.Destroyed:
                    TickDestroyed();
                    break;
            }
        }

        private void TickConstruction()
        {
            float speed = HollowGround.Core.TimeManager.Instance != null ? HollowGround.Core.TimeManager.Instance.GameSpeed : 1f;
            float devMult = HollowGround.Core.GameConfig.Instance != null ? HollowGround.Core.GameConfig.Instance.GetBuildTimeMultiplier : 1f;
            float buildTime = _data.GetBuildTimeForLevel(Level) * devMult;
            ConstructionProgress += Time.deltaTime * speed / buildTime;

            if (ConstructionProgress >= 1f)
            {
                ConstructionProgress = 1f;
                State = BuildingState.Active;
                OnConstructionComplete?.Invoke(this);
                UpdateModel();
            }
        }

        private void TickUpgrade()
        {
            float speed = HollowGround.Core.TimeManager.Instance != null ? HollowGround.Core.TimeManager.Instance.GameSpeed : 1f;
            float devMult = HollowGround.Core.GameConfig.Instance != null ? HollowGround.Core.GameConfig.Instance.GetBuildTimeMultiplier : 1f;
            float buildTime = _data.GetBuildTimeForLevel(Level + 1) * devMult;
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
            float devMult = HollowGround.Core.GameConfig.Instance != null ? HollowGround.Core.GameConfig.Instance.GetProductionIntervalMultiplier : 1f;
            productionInterval *= devMult;
            float productionBonus = GetTotalProductionBonus();
            if (productionBonus > 0f)
                productionInterval *= (1f - productionBonus);

            float workerModifier = GetWorkerProductionModifier();
            if (workerModifier <= 0.01f) return;
            productionInterval /= workerModifier;

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

        private float GetWorkerProductionModifier()
        {
            int required = _data.GetTotalRequiredWorkers();
            float dependency = _data.WorkerProductionBonus;
            return ProductionCalc.WorkerModifier(AssignedWorkerCount, required, dependency);
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

        public void RestoreFromSave(int level, BuildingState state, float constructionProgress, float upgradeProgress)
        {
            Level = level;
            State = state;
            ConstructionProgress = constructionProgress;
            UpgradeProgress = upgradeProgress;
            UpdateModel();
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
            FreeGridCells();

            float refundRatio = GameConfig.Instance != null ? GameConfig.Instance.DemolishRefundRatio : 0.5f;
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

        public void ClearForLoad()
        {
            FreeGridCells();
            BuildingManager.Instance?.UnregisterBuilding(this);
            DestroyImmediate(gameObject);
        }

        private void FreeGridCells()
        {
            var gridSystem = GridSystem.Instance;
            if (gridSystem != null)
            {
                var (sx, sz) = GetRotatedFootprint();
                gridSystem.FreeCells(GridOrigin.x, GridOrigin.y, sx, sz);
            }
        }

        private void UpdateModel()
        {
            if (_currentModel != null)
                Destroy(_currentModel);

            GameObject prefab = _data.GetModelForState(State, Level);
            if (prefab != null)
            {
                _currentModel = Instantiate(prefab, transform);
                _currentModel.transform.localPosition = new Vector3(0f, GroundYOffset, 0f);
                _currentModel.transform.localRotation = Quaternion.identity;
            }
        }

        private void TickDestroyed()
        {
            _destroyedTimer += Time.deltaTime;
            if (_destroyedTimer >= DestroyedDisplayDuration)
                RemoveBuilding();
        }

        public void ApplyDamage()
        {
            if (State == BuildingState.Destroyed) return;

            if (State == BuildingState.Damaged)
            {
                SetDestroyed();
                return;
            }

            State = BuildingState.Damaged;
            UpdateModel();
            OnDamaged?.Invoke(this);
        }

        private void SetDestroyed()
        {
            State = BuildingState.Destroyed;
            _destroyedTimer = 0f;
            UpdateModel();

            var col = GetComponent<Collider>();
            if (col != null)
                col.enabled = false;
        }

        public bool Repair()
        {
            if (State != BuildingState.Damaged) return false;

            var costs = _data.GetCostForLevel(Level);
            float repairRatio = GameConfig.Instance != null ? GameConfig.Instance.RepairCostRatio : 0.5f;
            var repairCosts = new Dictionary<ResourceType, int>();
            foreach (var kvp in costs)
                repairCosts[kvp.Key] = Mathf.CeilToInt(kvp.Value * repairRatio);

            if (ResourceManager.Instance == null || !ResourceManager.Instance.CanAfford(repairCosts))
            {
                return false;
            }

            ResourceManager.Instance.SpendMultiple(repairCosts);
            State = BuildingState.Active;
            UpdateModel();
            OnRepaired?.Invoke(this);
            return true;
        }

        private void RemoveBuilding()
        {
            var gridSystem = GridSystem.Instance;
            if (gridSystem != null)
            {
                var (sx, sz) = GetRotatedFootprint();
                gridSystem.FreeCells(GridOrigin.x, GridOrigin.y, sx, sz);
            }

            OnDestroyed?.Invoke(this);
            Destroy(gameObject);
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

        public (int sizeX, int sizeZ) GetRotatedFootprint()
        {
            if (_data == null) return (1, 1);
            return Rotation % 2 == 0
                ? (_data.SizeX, _data.SizeZ)
                : (_data.SizeZ, _data.SizeX);
        }

        public Vector2Int GetDoorCell()
        {
            var (sx, sz) = GetRotatedFootprint();
            int cx = GridOrigin.x + sx / 2;
            int cz = GridOrigin.y + sz / 2;

            return Rotation switch
            {
                0 => new Vector2Int(cx, GridOrigin.y - 1),
                1 => new Vector2Int(GridOrigin.x - 1, cz),
                2 => new Vector2Int(cx, GridOrigin.y + sz),
                3 => new Vector2Int(GridOrigin.x + sx, cz),
                _ => new Vector2Int(cx, GridOrigin.y - 1)
            };
        }

        public Vector3 GetDoorWorldPosition()
        {
            var doorCell = GetDoorCell();
            if (GridSystem.Instance != null && GridSystem.Instance.IsValidCoordinate(doorCell.x, doorCell.y))
                return GridSystem.Instance.GetWorldPosition(doorCell.x, doorCell.y);
            return transform.position;
        }
    }
}
