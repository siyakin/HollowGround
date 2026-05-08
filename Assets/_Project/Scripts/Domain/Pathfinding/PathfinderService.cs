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

    public struct PathfinderContext
    {
        public LinkedList<GridPos> Deque;
        public HashSet<GridPos> Visited;
        public Dictionary<GridPos, GridPos> Parent;
        public List<GridPos> PathBuffer;

        public static PathfinderContext Create()
        {
            return new PathfinderContext
            {
                Deque = new LinkedList<GridPos>(),
                Visited = new HashSet<GridPos>(),
                Parent = new Dictionary<GridPos, GridPos>(),
                PathBuffer = new List<GridPos>()
            };
        }
    }

    public static class PathfinderService
    {
        private static readonly int[] DirX = { 0, 1, 0, -1 };
        private static readonly int[] DirZ = { 1, 0, -1, 0 };

        public static List<GridPos> BFS(
            ref PathfinderContext ctx,
            IGridDataProvider grid,
            HashSet<GridPos> preferredCells,
            GridPos start,
            GridPos end,
            int maxIterations = 500)
        {
            ctx.Deque.Clear();
            ctx.Visited.Clear();
            ctx.Parent.Clear();

            ctx.Deque.AddLast(start);
            ctx.Visited.Add(start);

            int iterations = 0;
            while (ctx.Deque.Count > 0 && iterations < maxIterations)
            {
                iterations++;
                var current = ctx.Deque.First.Value;
                ctx.Deque.RemoveFirst();

                if (current.X == end.X && current.Z == end.Z)
                    return ReconstructPath(ref ctx, start, end, maxIterations);

                for (int d = 0; d < 4; d++)
                {
                    var next = new GridPos(current.X + DirX[d], current.Z + DirZ[d]);

                    if (ctx.Visited.Contains(next)) continue;
                    if (!grid.IsValid(next.X, next.Z)) continue;
                    if (!grid.IsPassable(next.X, next.Z)) continue;

                    ctx.Visited.Add(next);
                    ctx.Parent[next] = current;

                    if (preferredCells != null && preferredCells.Contains(next))
                        ctx.Deque.AddFirst(next);
                    else
                        ctx.Deque.AddLast(next);
                }
            }

            return null;
        }

        private static List<GridPos> ReconstructPath(ref PathfinderContext ctx, GridPos start, GridPos end, int maxIterations)
        {
            ctx.PathBuffer.Clear();
            var current = end;
            int safety = 0;

            while (!(current.X == start.X && current.Z == start.Z) && safety < maxIterations)
            {
                ctx.PathBuffer.Add(current);
                if (!ctx.Parent.ContainsKey(current)) return null;
                current = ctx.Parent[current];
                safety++;
            }

            ctx.PathBuffer.Add(start);
            ctx.PathBuffer.Reverse();

            return new List<GridPos>(ctx.PathBuffer);
        }
    }
}
