using System;
using System.Collections.Generic;
using HollowGround.Core;
using HollowGround.Grid;
using HollowGround.Roads;
using UnityEngine;

namespace HollowGround.NPCs
{
    public class WalkerManager : Singleton<WalkerManager>
    {
        private readonly List<WalkerBase> _walkers = new();
        private readonly Dictionary<(Vector2Int, Vector2Int), List<Vector2Int>> _pathCache = new();
        private readonly Dictionary<Vector2Int, WalkerBase> _occupiedCells = new();
        private readonly Stack<SettlerWalker> _recyclePool = new();

        public int WalkerCount => _walkers.Count;
        public int ActiveWalkerCount
        {
            get
            {
                int count = 0;
                foreach (var w in _walkers)
                    if (w != null && w.IsActive) count++;
                return count;
            }
        }
        public int RecycledCount => _recyclePool.Count;
        public int OccupiedCellCount => _occupiedCells.Count;

        private void Start()
        {
            if (RoadManager.Instance != null)
                RoadManager.Instance.OnRoadsChanged += InvalidatePathCache;
        }

        protected override void OnDestroy()
        {
            if (RoadManager.Instance != null)
                RoadManager.Instance.OnRoadsChanged -= InvalidatePathCache;
            base.OnDestroy();
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
            if (TimeManager.Instance != null && TimeManager.Instance.IsPaused) return;

            float dt = Time.deltaTime;
            float speed = TimeManager.Instance != null ? TimeManager.Instance.GameSpeed : 1f;
            if (speed <= 0f) return;

            for (int i = _walkers.Count - 1; i >= 0; i--)
            {
                if (_walkers[i] != null)
                    _walkers[i].Tick(dt, speed);
            }
        }

        public void Register(WalkerBase walker)
        {
            if (walker == null || _walkers.Contains(walker)) return;
            _walkers.Add(walker);
        }

        public void Unregister(WalkerBase walker)
        {
            if (walker == null) return;
            _walkers.Remove(walker);
            if (walker is SettlerWalker sw)
                ReleaseOccupiedCell(sw);
        }

        public void Recycle(SettlerWalker walker)
        {
            if (walker == null) return;
            _walkers.Remove(walker);
            ReleaseOccupiedCell(walker);
            walker.gameObject.SetActive(false);
            _recyclePool.Push(walker);
        }

        public SettlerWalker GetRecycled()
        {
            if (_recyclePool.Count == 0) return null;
            var walker = _recyclePool.Pop();
            if (walker == null || walker.gameObject == null) return null;
            walker.gameObject.SetActive(false);
            return walker;
        }

        public void ClearRecyclePool()
        {
            while (_recyclePool.Count > 0)
            {
                var walker = _recyclePool.Pop();
                if (walker != null && walker.gameObject != null)
                    Destroy(walker.gameObject);
            }
        }

        public bool IsCellOccupied(Vector2Int cell)
        {
            return _occupiedCells.ContainsKey(cell);
        }

        public bool TryOccupyCell(Vector2Int cell, WalkerBase walker)
        {
            if (_occupiedCells.TryGetValue(cell, out var occupant))
            {
                if (occupant != null && occupant != walker && occupant.IsActive)
                    return false;
                _occupiedCells.Remove(cell);
            }
            _occupiedCells[cell] = walker;
            return true;
        }

        public void ReleaseOccupiedCell(SettlerWalker walker)
        {
            if (walker == null) return;
            var cell = walker.GetGridPosition();
            if (_occupiedCells.TryGetValue(cell, out var occupant) && occupant == walker)
                _occupiedCells.Remove(cell);
        }

        public void UpdateOccupancy(SettlerWalker walker, Vector2Int previousCell, Vector2Int newCell)
        {
            if (previousCell == newCell) return;
            if (_occupiedCells.TryGetValue(previousCell, out var prev) && prev == walker)
                _occupiedCells.Remove(previousCell);
            TryOccupyCell(newCell, walker);
        }

        public List<Vector2Int> RequestPath(Vector2Int start, Vector2Int end)
        {
            var key = (start, end);
            if (_pathCache.TryGetValue(key, out var cached))
                return cached;

            if (RoadManager.Instance == null) return null;

            var path = RoadManager.Instance.FindPublicPath(start, end);
            if (path != null)
                _pathCache[key] = path;

            return path;
        }

        public void InvalidatePathCache()
        {
            _pathCache.Clear();
        }
    }
}
