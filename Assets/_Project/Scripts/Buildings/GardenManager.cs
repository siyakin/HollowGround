using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HollowGround.Core;
using HollowGround.Grid;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.Buildings
{
    public class GardenManager : Singleton<GardenManager>
    {
        private BuildingData _smallGardenData;
        private BuildingData _largeGardenData;

        public event Action<Building> OnGardenMerged;

        private void Start()
        {
            CacheGardenData();
            if (BuildingManager.Instance != null)
                BuildingManager.Instance.OnBuildingAdded += HandleBuildingAdded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (BuildingManager.Instance != null)
                BuildingManager.Instance.OnBuildingAdded -= HandleBuildingAdded;
        }

        private void CacheGardenData()
        {
            _smallGardenData = BuildingDatabase.LoadBuildingData(BuildingType.Garden);
            _largeGardenData = BuildingDatabase.LoadBuildingData(BuildingType.GardenLarge);

            if (_smallGardenData == null)
                Debug.LogWarning("[GardenManager] SmallGarden BuildingData not found");
            if (_largeGardenData == null)
                Debug.LogWarning("[GardenManager] LargeGarden BuildingData not found");
        }

        private void HandleBuildingAdded(Building building)
        {
            if (building.Data.Type != BuildingType.Garden) return;
            building.OnConstructionComplete += OnGardenConstructionComplete;
        }

        private void OnGardenConstructionComplete(Building building)
        {
            building.OnConstructionComplete -= OnGardenConstructionComplete;
            StartCoroutine(DeferredMerge(building));
        }

        private IEnumerator DeferredMerge(Building trigger)
        {
            yield return null;
            if (trigger == null) yield break;
            CheckAndMerge(trigger);
        }

        public void CheckAndMerge(Building trigger)
        {
            if (_smallGardenData == null || _largeGardenData == null) return;
            if (trigger.Data.Type != BuildingType.Garden) return;

            Vector2Int origin = trigger.GridOrigin;

            Vector2Int[] offsets = new Vector2Int[]
            {
                new(0, 0),
                new(-1, 0),
                new(0, -1),
                new(-1, -1)
            };

            foreach (var offset in offsets)
            {
                Vector2Int corner = origin + offset;
                if (TryGetMergeGroup(corner, out List<Building> group))
                {
                    ExecuteMerge(group, corner);
                    return;
                }
            }
        }

        private bool TryGetMergeGroup(Vector2Int bottomLeft, out List<Building> group)
        {
            group = new List<Building>();
            var bm = BuildingManager.Instance;
            if (bm == null) return false;

            for (int dx = 0; dx < 2; dx++)
            {
                for (int dz = 0; dz < 2; dz++)
                {
                    Vector2Int cell = new(bottomLeft.x + dx, bottomLeft.y + dz);
                    Building found = FindActiveGardenAt(cell);
                    if (found == null) return false;
                    group.Add(found);
                }
            }

            return group.Count == 4;
        }

        private Building FindActiveGardenAt(Vector2Int gridPos)
        {
            var bm = BuildingManager.Instance;
            if (bm == null) return null;

            foreach (var b in bm.AllBuildings)
            {
                if (b.Data.Type != BuildingType.Garden) continue;
                if (b.State != BuildingState.Active) continue;
                if (b.GridOrigin == gridPos) return b;
            }
            return null;
        }

        private void ExecuteMerge(List<Building> smallGardens, Vector2Int bottomLeft)
        {
            Vector3 centerWorld = GridSystem.Instance.GetWorldPosition(bottomLeft.x, bottomLeft.y);
            float cellSize = GridSystem.Instance.CellSize;
            centerWorld.x += cellSize * 0.5f;
            centerWorld.z += cellSize * 0.5f;

            foreach (var garden in smallGardens.ToList())
            {
                BuildingManager.Instance.UnregisterBuilding(garden);
                garden.FreeGridCells();
                DestroyImmediate(garden.gameObject);
            }

            GameObject largeObj = new("Garden_Large");
            largeObj.transform.position = centerWorld;

            Building largeBuilding = largeObj.AddComponent<Building>();
            largeBuilding.Initialize(_largeGardenData, bottomLeft);

            GridSystem.Instance.OccupyCells(bottomLeft.x, bottomLeft.y, 2, 2, largeObj);
            BuildingManager.Instance.RegisterBuilding(largeBuilding);

            OnGardenMerged?.Invoke(largeBuilding);
        }
    }
}
