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

        public static List<GridPos> BFS(
            IGridDataProvider grid,
            HashSet<GridPos> preferredCells,
            GridPos start,
            GridPos end,
            int maxIterations = 500)
        {
            var deque = new LinkedList<GridPos>();
            var visited = new HashSet<GridPos>();
            var parent = new Dictionary<GridPos, GridPos>();

            deque.AddLast(start);
            visited.Add(start);

            int iterations = 0;
            while (deque.Count > 0 && iterations < maxIterations)
            {
                iterations++;
                var current = deque.First.Value;
                deque.RemoveFirst();

                if (current.X == end.X && current.Z == end.Z)
                    return ReconstructPath(parent, start, end, maxIterations);

                for (int d = 0; d < 4; d++)
                {
                    var next = new GridPos(current.X + DirX[d], current.Z + DirZ[d]);

                    if (visited.Contains(next)) continue;
                    if (!grid.IsValid(next.X, next.Z)) continue;
                    if (!grid.IsPassable(next.X, next.Z)) continue;

                    visited.Add(next);
                    parent[next] = current;

                    if (preferredCells != null && preferredCells.Contains(next))
                        deque.AddFirst(next);
                    else
                        deque.AddLast(next);
                }
            }

            return null;
        }

        private static List<GridPos> ReconstructPath(
            Dictionary<GridPos, GridPos> parent,
            GridPos start,
            GridPos end,
            int maxIterations)
        {
            var path = new List<GridPos>();
            var current = end;
            int safety = 0;

            while (!(current.X == start.X && current.Z == start.Z) && safety < maxIterations)
            {
                path.Add(current);
                if (!parent.ContainsKey(current)) return null;
                current = parent[current];
                safety++;
            }

            path.Add(start);
            path.Reverse();
            return path;
        }
    }
}
