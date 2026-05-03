using System;
using System.Collections;
using System.Collections.Generic;
using HollowGround.Buildings;
using HollowGround.Core;
using HollowGround.Domain.Pathfinding;
using HollowGround.Grid;
using HollowGround.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollowGround.Roads
{
    public class RoadManager : Singleton<RoadManager>, IGridDataProvider
    {
        private readonly HashSet<Vector2Int> _roadCells = new();
        private RoadVisualizer _visualizer;
        private Coroutine _cleanupCoroutine;
        private int _groundMask;

        public event Action OnRoadsChanged;

        private static readonly Vector2Int[] Directions = {
            new(0, 1),
            new(1, 0),
            new(0, -1),
            new(-1, 0)
        };

        private const int SearchRadius = 15;
        private const int MaxBfsIterations = 500;
        private const float OrphanCleanupDelay = 30f;
        private const float FadeOutDuration = 2f;

        protected override void Awake()
        {
            base.Awake();
            _visualizer = gameObject.AddComponent<RoadVisualizer>();
            _groundMask = LayerMask.GetMask("Ground");
        }

        private void Start()
        {
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.OnBuildingAdded += OnBuildingAdded;
                BuildingManager.Instance.OnBuildingRemoved += OnBuildingRemoved;
                foreach (var b in BuildingManager.Instance.AllBuildings)
                    OnBuildingAdded(b);
            }
        }

        protected override void OnDestroy()
        {
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.OnBuildingAdded -= OnBuildingAdded;
                BuildingManager.Instance.OnBuildingRemoved -= OnBuildingRemoved;
            }
            base.OnDestroy();
        }

        private void Update()
        {
            HandleManualRoadRemoval();
        }

        private void HandleManualRoadRemoval()
        {
            if (BuildingPlacer.Instance != null && BuildingPlacer.Instance.IsPlacing) return;
            if (UIManager.Instance != null && UIManager.Instance.IsInputBlocked) return;
            if (Mouse.current == null) return;
            if (!Mouse.current.rightButton.wasPressedThisFrame) return;

            var cam = UnityEngine.Camera.main;
            if (cam == null) return;
            if (GridSystem.Instance == null) return;

            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _groundMask)) return;

            var coords = GridSystem.Instance.GetGridCoordinates(hit.point);
            if (!_roadCells.Contains(coords)) return;

            _roadCells.Remove(coords);
            _visualizer.FadeOutAndRemove(new HashSet<Vector2Int> { coords });
            OnRoadsChanged?.Invoke();
        }

        private void OnBuildingAdded(Building building)
        {
            RemoveRoadCellsUnderBuilding(building);
            building.OnConstructionComplete += OnBuildingCompleted;

            if (building.State == BuildingState.Active)
                GenerateRoadsFromBuilding(building);
        }

        private void OnBuildingRemoved(Building building)
        {
            building.OnConstructionComplete -= OnBuildingCompleted;
            ScheduleOrphanCleanup();
        }

        private void ScheduleOrphanCleanup()
        {
            if (_cleanupCoroutine != null)
                StopCoroutine(_cleanupCoroutine);
            _cleanupCoroutine = StartCoroutine(DelayedOrphanCleanup());
        }

        private IEnumerator DelayedOrphanCleanup()
        {
            yield return new WaitForSeconds(OrphanCleanupDelay);
            RemoveOrphanedRoads();
            _cleanupCoroutine = null;
        }

        private void RemoveOrphanedRoads()
        {
            if (BuildingManager.Instance == null) return;
            if (_roadCells.Count == 0) return;

            var reachable = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();

            foreach (var b in BuildingManager.Instance.AllBuildings)
            {
                if (b.State != BuildingState.Active) continue;
                var door = b.GetDoorCell();

                if (_roadCells.Contains(door))
                {
                    if (reachable.Add(door))
                        queue.Enqueue(door);
                }

                foreach (var dir in Directions)
                {
                    var adj = door + dir;
                    if (_roadCells.Contains(adj) && reachable.Add(adj))
                        queue.Enqueue(adj);
                }
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var dir in Directions)
                {
                    var next = current + dir;
                    if (reachable.Contains(next)) continue;
                    if (!_roadCells.Contains(next)) continue;
                    reachable.Add(next);
                    queue.Enqueue(next);
                }
            }

            var orphaned = new HashSet<Vector2Int>();
            foreach (var cell in _roadCells)
            {
                if (!reachable.Contains(cell))
                    orphaned.Add(cell);
            }

            if (orphaned.Count == 0) return;

            foreach (var cell in orphaned)
                _roadCells.Remove(cell);

            _visualizer.FadeOutAndRemove(orphaned);
            OnRoadsChanged?.Invoke();
        }

        private void RemoveRoadCellsUnderBuilding(Building building)
        {
            var (sx, sz) = building.GetRotatedFootprint();
            bool changed = false;
            for (int x = building.GridOrigin.x; x < building.GridOrigin.x + sx; x++)
            {
                for (int z = building.GridOrigin.y; z < building.GridOrigin.y + sz; z++)
                {
                    if (_roadCells.Remove(new Vector2Int(x, z)))
                        changed = true;
                }
            }
            if (changed)
                _visualizer.RebuildVisuals(_roadCells);
        }

        private void OnBuildingCompleted(Building building)
        {
            GenerateRoadsFromBuilding(building);
        }

        private void GenerateRoadsFromBuilding(Building source)
        {
            if (GridSystem.Instance == null) return;
            if (BuildingManager.Instance == null) return;

            Vector2Int sourceDoor = source.GetDoorCell();
            if (!IsReachableCell(sourceDoor))
            {
                Vector2Int? alt = FindNearestReachableCell(sourceDoor);
                if (alt == null) return;
                sourceDoor = alt.Value;
            }

            bool changed = false;

            var nearestRoad = FindNearestRoadCell(sourceDoor);
            if (nearestRoad.HasValue)
            {
                var path = FindPath(sourceDoor, nearestRoad.Value);
                if (path != null)
                {
                    foreach (var cell in path)
                    {
                        if (CanPlaceRoad(cell) && _roadCells.Add(cell))
                            changed = true;
                    }
                }
            }

            var nearbyBuildings = GetNearbyActiveBuildings(source);
            foreach (var target in nearbyBuildings)
            {
                Vector2Int targetDoor = target.GetDoorCell();
                if (!IsReachableCell(targetDoor))
                {
                    Vector2Int? alt = FindNearestReachableCell(targetDoor);
                    if (alt == null) continue;
                    targetDoor = alt.Value;
                }

                if (targetDoor == sourceDoor) continue;

                var path = FindPath(sourceDoor, targetDoor);
                if (path != null)
                {
                    foreach (var cell in path)
                    {
                        if (CanPlaceRoad(cell) && _roadCells.Add(cell))
                            changed = true;
                    }
                }
            }

            if (changed)
            {
                _visualizer.RebuildVisuals(_roadCells);
                OnRoadsChanged?.Invoke();
            }
        }

        private bool IsReachableCell(Vector2Int cell)
        {
            if (GridSystem.Instance == null) return false;
            if (!GridSystem.Instance.IsValidCoordinate(cell.x, cell.y)) return false;
            var gridCell = GridSystem.Instance.GetCell(cell.x, cell.y);
            return gridCell != null && gridCell.IsPassable;
        }

        private Vector2Int? FindNearestReachableCell(Vector2Int cell)
        {
            foreach (var dir in Directions)
            {
                var next = cell + dir;
                if (IsReachableCell(next))
                    return next;
            }
            return null;
        }

        private List<Building> GetNearbyActiveBuildings(Building source)
        {
            var result = new List<Building>();

            foreach (var b in BuildingManager.Instance.AllBuildings)
            {
                if (b == source) continue;
                if (b.State != BuildingState.Active) continue;

                int dx = Mathf.Abs(b.GridOrigin.x - source.GridOrigin.x);
                int dz = Mathf.Abs(b.GridOrigin.y - source.GridOrigin.y);
                if (dx + dz <= SearchRadius)
                    result.Add(b);
            }

            result.Sort((a, b) =>
            {
                int distA = Mathf.Abs(a.GridOrigin.x - source.GridOrigin.x) + Mathf.Abs(a.GridOrigin.y - source.GridOrigin.y);
                int distB = Mathf.Abs(b.GridOrigin.x - source.GridOrigin.x) + Mathf.Abs(b.GridOrigin.y - source.GridOrigin.y);
                return distA.CompareTo(distB);
            });

            return result;
        }

        private Vector2Int? FindNearestRoadCell(Vector2Int from)
        {
            if (_roadCells.Count == 0) return null;

            var queue = new Queue<Vector2Int>();
            var visited = new HashSet<Vector2Int>();
            queue.Enqueue(from);
            visited.Add(from);

            int iterations = 0;
            while (queue.Count > 0 && iterations < 100)
            {
                iterations++;
                var current = queue.Dequeue();

                if (_roadCells.Contains(current) && current != from)
                    return current;

                foreach (var dir in Directions)
                {
                    var next = current + dir;
                    if (visited.Contains(next)) continue;
                if (!GridSystem.Instance.IsValidCoordinate(next.x, next.y)) continue;
                var cell = GridSystem.Instance.GetCell(next.x, next.y);
                if (cell == null || !cell.IsPassable) continue;
                visited.Add(next);
                    queue.Enqueue(next);
                }
            }

            return null;
        }

        private List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
        {
            var preferred = new HashSet<GridPos>();
            foreach (var cell in _roadCells)
                preferred.Add(new GridPos(cell.x, cell.y));

            var result = PathfinderService.BFS(this, preferred, new GridPos(start.x, start.y), new GridPos(end.x, end.y), MaxBfsIterations);
            if (result == null) return null;

            var path = new List<Vector2Int>();
            foreach (var p in result)
                path.Add(new Vector2Int(p.X, p.Z));
            return path;
        }

        public bool IsValid(int x, int z)
        {
            return GridSystem.Instance != null && GridSystem.Instance.IsValidCoordinate(x, z);
        }

        public bool IsPassable(int x, int z)
        {
            if (GridSystem.Instance == null) return false;
            var cell = GridSystem.Instance.GetCell(x, z);
            return cell != null && cell.IsPassable;
        }

        public void ClearAllRoads()
        {
            _roadCells.Clear();
            if (_visualizer != null)
                _visualizer.RebuildVisuals(_roadCells);
            OnRoadsChanged?.Invoke();
        }

        public List<Vector2Int> GetRoadCellsForSave()
        {
            return new List<Vector2Int>(_roadCells);
        }

        public void LoadRoadCells(List<Vector2Int> cells)
        {
            _roadCells.Clear();
            foreach (var cell in cells)
            {
                if (CanPlaceRoad(cell))
                    _roadCells.Add(cell);
            }
            if (_visualizer != null)
                _visualizer.RebuildVisuals(_roadCells);
            OnRoadsChanged?.Invoke();
        }

        public bool HasRoadAt(Vector2Int cell) => _roadCells.Contains(cell);

        private bool CanPlaceRoad(Vector2Int cell)
        {
            if (GridSystem.Instance == null) return false;
            var gridCell = GridSystem.Instance.GetCell(cell.x, cell.y);
            return gridCell != null && gridCell.IsPassable && gridCell.Terrain.IsBuildable();
        }

        public List<Vector2Int> FindPublicPath(Vector2Int start, Vector2Int end)
        {
            if (GridSystem.Instance == null) return null;
            if (!GridSystem.Instance.IsValidCoordinate(start.x, start.y)) return null;
            if (!GridSystem.Instance.IsValidCoordinate(end.x, end.y)) return null;
            return FindPath(start, end);
        }

        public HashSet<Vector2Int> GetAllRoadCells() => _roadCells;

        public bool HasRoads => _roadCells.Count > 0;

        public List<Vector2Int> GetActiveBuildingDoorCells()
        {
            var doors = new List<Vector2Int>();
            if (BuildingManager.Instance == null) return doors;

            foreach (var b in BuildingManager.Instance.AllBuildings)
            {
                if (b.State == BuildingState.Active)
                    doors.Add(b.GetDoorCell());
            }

            return doors;
        }
    }
}
