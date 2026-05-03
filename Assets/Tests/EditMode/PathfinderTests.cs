using System.Collections.Generic;
using HollowGround.Domain.Pathfinding;
using NUnit.Framework;

namespace HollowGround.Tests
{
    [TestFixture]
    public class PathfinderTests
    {
        private class TestGrid : IGridDataProvider
        {
            private readonly int _width;
            private readonly int _height;
            private readonly HashSet<GridPos> _blocked = new();

            public TestGrid(int width, int height)
            {
                _width = width;
                _height = height;
            }

            public void Block(int x, int z) => _blocked.Add(new GridPos(x, z));

            public bool IsValid(int x, int z) => x >= 0 && x < _width && z >= 0 && z < _height;
            public bool IsPassable(int x, int z) => !_blocked.Contains(new GridPos(x, z));
        }

        [Test]
        public void StraightPath_NoPreferred()
        {
            var grid = new TestGrid(10, 10);
            var result = PathfinderService.BFS(grid, null, new GridPos(0, 0), new GridPos(0, 3));

            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(new GridPos(0, 0), result[0]);
            Assert.AreEqual(new GridPos(0, 3), result[^1]);
        }

        [Test]
        public void NoPath_Blocked()
        {
            var grid = new TestGrid(10, 3);
            for (int x = 0; x < 10; x++) grid.Block(x, 1);

            var result = PathfinderService.BFS(grid, null, new GridPos(0, 0), new GridPos(5, 2));
            Assert.IsNull(result);
        }

        [Test]
        public void SameStartEnd()
        {
            var grid = new TestGrid(5, 5);
            var result = PathfinderService.BFS(grid, null, new GridPos(2, 2), new GridPos(2, 2));

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(new GridPos(2, 2), result[0]);
        }

        [Test]
        public void PreferredCellsUsedFirst()
        {
            var grid = new TestGrid(5, 5);
            var preferred = new HashSet<GridPos> { new(1, 0), new(2, 0), new(3, 0) };

            var result = PathfinderService.BFS(grid, preferred, new GridPos(0, 0), new GridPos(4, 0));

            Assert.IsNotNull(result);
            Assert.AreEqual(new GridPos(0, 0), result[0]);
            Assert.AreEqual(new GridPos(4, 0), result[^1]);

            foreach (var cell in preferred)
            {
                bool found = false;
                foreach (var p in result)
                    if (p.X == cell.X && p.Z == cell.Z) { found = true; break; }
                Assert.IsTrue(found, $"Preferred cell ({cell.X},{cell.Z}) should be in path");
            }
        }

        [Test]
        public void PathAroundObstacle()
        {
            var grid = new TestGrid(5, 5);
            grid.Block(2, 1);
            grid.Block(2, 2);
            grid.Block(2, 3);

            var result = PathfinderService.BFS(grid, null, new GridPos(1, 2), new GridPos(3, 2));

            Assert.IsNotNull(result);
            Assert.AreEqual(new GridPos(1, 2), result[0]);
            Assert.AreEqual(new GridPos(3, 2), result[^1]);
        }

        [Test]
        public void MaxIterationsStopsSearch()
        {
            var grid = new TestGrid(100, 100);
            var result = PathfinderService.BFS(grid, null, new GridPos(0, 0), new GridPos(99, 99), maxIterations: 5);
            Assert.IsNull(result);
        }

        [Test]
        public void LShapedPath()
        {
            var grid = new TestGrid(5, 5);
            var result = PathfinderService.BFS(grid, null, new GridPos(0, 0), new GridPos(3, 2));

            Assert.IsNotNull(result);
            Assert.AreEqual(new GridPos(0, 0), result[0]);
            Assert.AreEqual(new GridPos(3, 2), result[^1]);

            for (int i = 1; i < result.Count; i++)
            {
                int dx = System.Math.Abs(result[i].X - result[i - 1].X);
                int dz = System.Math.Abs(result[i].Z - result[i - 1].Z);
                Assert.IsTrue((dx == 1 && dz == 0) || (dx == 0 && dz == 1),
                    $"Step {i} is not adjacent");
            }
        }
    }
}
