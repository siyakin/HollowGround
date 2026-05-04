using System;
using System.Collections.Generic;

namespace HollowGround.Domain.Pathfinding
{
    public interface IGridDataProvider
    {
        bool IsValid(int x, int z);
        bool IsPassable(int x, int z);
    }

    public struct GridPos : IEquatable<GridPos>
    {
        public int X;
        public int Z;

        public GridPos(int x, int z) { X = x; Z = z; }

        public bool Equals(GridPos other) => other.X == X && other.Z == Z;
        public override bool Equals(object obj) => obj is GridPos p && p.X == X && p.Z == Z;
        public override int GetHashCode() => HashCode.Combine(X, Z);
        public static bool operator ==(GridPos a, GridPos b) => a.X == b.X && a.Z == b.Z;
        public static bool operator !=(GridPos a, GridPos b) => a.X != b.X || a.Z != b.Z;
    }

    public static class PathfinderService
    {
        private static readonly int[] DirX = { 0, 1, 0, -1 };
        private static readonly int[] DirZ = { 1, 0, -1, 0 };

        private static readonly LinkedList<GridPos> _deque = new();
        private static readonly HashSet<GridPos> _visited = new();
        private static readonly Dictionary<GridPos, GridPos> _parent = new();
        private static readonly List<GridPos> _pathBuffer = new();

        public static List<GridPos> BFS(
            IGridDataProvider grid,
            HashSet<GridPos> preferredCells,
            GridPos start,
            GridPos end,
            int maxIterations = 500)
        {
            _deque.Clear();
            _visited.Clear();
            _parent.Clear();

            _deque.AddLast(start);
            _visited.Add(start);

            int iterations = 0;
            while (_deque.Count > 0 && iterations < maxIterations)
            {
                iterations++;
                var current = _deque.First.Value;
                _deque.RemoveFirst();

                if (current.X == end.X && current.Z == end.Z)
                    return ReconstructPath(start, end, maxIterations);

                for (int d = 0; d < 4; d++)
                {
                    var next = new GridPos(current.X + DirX[d], current.Z + DirZ[d]);

                    if (_visited.Contains(next)) continue;
                    if (!grid.IsValid(next.X, next.Z)) continue;
                    if (!grid.IsPassable(next.X, next.Z)) continue;

                    _visited.Add(next);
                    _parent[next] = current;

                    if (preferredCells != null && preferredCells.Contains(next))
                        _deque.AddFirst(next);
                    else
                        _deque.AddLast(next);
                }
            }

            return null;
        }

        private static List<GridPos> ReconstructPath(GridPos start, GridPos end, int maxIterations)
        {
            _pathBuffer.Clear();
            var current = end;
            int safety = 0;

            while (!(current.X == start.X && current.Z == start.Z) && safety < maxIterations)
            {
                _pathBuffer.Add(current);
                if (!_parent.ContainsKey(current)) return null;
                current = _parent[current];
                safety++;
            }

            _pathBuffer.Add(start);
            _pathBuffer.Reverse();

            return new List<GridPos>(_pathBuffer);
        }
    }
}
