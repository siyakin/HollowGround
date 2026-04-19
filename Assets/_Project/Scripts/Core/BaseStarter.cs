using System.Collections.Generic;
using HollowGround.Buildings;
using HollowGround.Combat;
using HollowGround.Grid;
using HollowGround.World;
using UnityEngine;

namespace HollowGround.Core
{
    public class BaseStarter : MonoBehaviour
    {
        [Header("Starting Buildings")]
        [SerializeField] private BuildingData _commandCenterData;
        [SerializeField] private BuildingData _farmData;
        [SerializeField] private BuildingData _woodFactoryData;
        [SerializeField] private BuildingData _waterWellData;

        [Header("Grid Positions (relative to center)")]
        [SerializeField] private Vector2Int _ccPos = new(24, 24);
        [SerializeField] private Vector2Int _farmPos = new(26, 24);
        [SerializeField] private Vector2Int _woodPos = new(24, 26);
        [SerializeField] private Vector2Int _waterPos = new(26, 26);

        [ContextMenu("Setup Base")]
        public void SetupBase()
        {
            PlaceStartingBuildings();
            SetupWorldMap();
            StartAttackCycle();
        }

        private void PlaceStartingBuildings()
        {
            if (GridSystem.Instance == null) return;

            PlaceBuilding(_commandCenterData, _ccPos);
            PlaceBuilding(_farmData, _farmPos);
            PlaceBuilding(_woodFactoryData, _woodPos);
            PlaceBuilding(_waterWellData, _waterPos);
        }

        private void PlaceBuilding(BuildingData data, Vector2Int gridPos)
        {
            if (data == null) return;
            if (GridSystem.Instance == null) return;
            if (!GridSystem.Instance.IsAreaBuildable(gridPos.x, gridPos.y, data.SizeX, data.SizeZ)) return;

            Vector3 worldPos = GridSystem.Instance.GetWorldPosition(gridPos.x, gridPos.y);
            float offsetX = (data.SizeX - 1) * GridSystem.Instance.CellSize * 0.5f;
            float offsetZ = (data.SizeZ - 1) * GridSystem.Instance.CellSize * 0.5f;

            var go = new GameObject(data.DisplayName);
            go.transform.position = new Vector3(worldPos.x + offsetX, worldPos.y, worldPos.z + offsetZ);

            var building = go.AddComponent<Building>();
            building.Initialize(data, gridPos);

            GridSystem.Instance.OccupyCells(gridPos.x, gridPos.y, data.SizeX, data.SizeZ, go);

            if (BuildingManager.Instance != null)
                BuildingManager.Instance.RegisterBuilding(building);
        }

        private void SetupWorldMap()
        {
            if (WorldMap.Instance == null) return;
            WorldMap.Instance.GenerateDefaultMap();
        }

        private void StartAttackCycle()
        {
            if (MutantAttackManager.Instance != null)
                MutantAttackManager.Instance.StartAttackCycle();
        }
    }
}
