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
        private const float GroundYOffset = 0.001f;

        public event Action<Building> OnConstructionComplete;
        public event Action<Building> OnUpgradeComplete;
        public event Action<Building, ResourceType, int> OnProduced;
        public event Action<Building> OnDestroyed;
        public event Action<Building> OnDamaged;
        public event Action<Building> OnRepaired;
        public event Action<Building, BuildingState> OnStateChanged;

        public static Building Create(BuildingData data, Vector2Int gridOrigin, int rotation = 0)
        {
            var go = new GameObject(data.DisplayName);
            PositionBuildingObject(go, data, gridOrigin, rotation);

            var building = go.AddComponent<Building>();
            building.Initialize(data, gridOrigin, rotation);
            return building;
        }

        public static void PositionBuildingObject(GameObject go, BuildingData data, Vector2Int gridOrigin, int rotation)
        {
            int sx = rotation % 2 == 0 ? data.SizeX : data.SizeZ;
            int sz = rotation % 2 == 0 ? data.SizeZ : data.SizeX;

            Vector3 worldPos = GridSystem.Instance != null
                ? GridSystem.Instance.GetWorldPosition(gridOrigin.x, gridOrigin.y)
                : Vector3.zero;
            float offsetX = (sx - 1) * (GridSystem.Instance != null ? GridSystem.Instance.CellSize : 2f) * 0.5f;
            float offsetZ = (sz - 1) * (GridSystem.Instance != null ? GridSystem.Instance.CellSize : 2f) * 0.5f;

            go.transform.position = new Vector3(worldPos.x + offsetX, worldPos.y, worldPos.z + offsetZ);
            go.transform.rotation = Quaternion.Euler(0, rotation * 90f, 0);
        }

        private void SetState(BuildingState newState)
        {
            if (State == newState) return;
            State = newState;
            OnStateChanged?.Invoke(this, newState);
        }

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

        internal void TickConstruction(float dt, float speed, float devMult)
        {
            float buildTime = _data.GetBuildTimeForLevel(Level) * devMult;
            ConstructionProgress += dt * speed / buildTime;

            if (ConstructionProgress >= 1f)
            {
                ConstructionProgress = 1f;
                SetState(BuildingState.Active);
                OnConstructionComplete?.Invoke(this);
                if (!this) return;
                UpdateModel();
            }
        }

        internal void TickUpgrade(float dt, float speed, float devMult)
        {
            float buildTime = _data.GetBuildTimeForLevel(Level + 1) * devMult;
            UpgradeProgress += dt * speed / buildTime;

            if (UpgradeProgress >= 1f)
            {
                UpgradeProgress = 1f;
                Level++;
                SetState(BuildingState.Active);
                UpdateModel();
                ApplyLevelEffects();
                OnUpgradeComplete?.Invoke(this);
            }
        }

        internal void TickProduction(float dt, float speed, float productionBonus)
        {
            if (!_data.HasProduction) return;

            _productionTimer += dt * speed;

            float productionInterval = _data.ProductionInterval;
            float devMult = HollowGround.Core.GameConfig.Instance != null ? HollowGround.Core.GameConfig.Instance.GetProductionIntervalMultiplier : 1f;
            productionInterval *= devMult;
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

        internal void TickDestroyed(float dt)
        {
            _destroyedTimer += dt;
            if (_destroyedTimer >= DestroyedDisplayDuration)
                RemoveBuilding();
        }

        private float GetWorkerProductionModifier()
        {
            int required = _data.GetTotalRequiredWorkers();
            float dependency = _data.WorkerProductionBonus;
            return ProductionCalc.WorkerModifier(AssignedWorkerCount, required, dependency);
        }

    public float ProductionTimer => _productionTimer;

    public void RestoreFromSave(int level, BuildingState state, float constructionProgress, float upgradeProgress, float productionTimer = 0f)
    {
        Level = level;
        State = state;
        ConstructionProgress = constructionProgress;
        UpgradeProgress = upgradeProgress;
        _productionTimer = productionTimer;
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
            SetState(BuildingState.Upgrading);
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

        public void FreeGridCells()
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
            SetState(BuildingState.Destroyed);
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
            SetState(BuildingState.Active);
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
