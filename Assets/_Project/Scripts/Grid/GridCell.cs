using UnityEngine;

namespace HollowGround.Grid
{
    public enum CellState
    {
        Empty,
        Occupied,
        Blocked
    }

    public class GridCell
    {
        public int X { get; }
        public int Z { get; }
        public CellState State { get; set; }
        public GameObject Occupant { get; set; }
        public Vector3 WorldPosition { get; }
        public TerrainType Terrain { get; set; }

        public GridCell(int x, int z, Vector3 worldPosition)
        {
            X = x;
            Z = z;
            WorldPosition = worldPosition;
            State = CellState.Empty;
            Occupant = null;
            Terrain = TerrainType.Flat;
        }

        public bool IsBuildable => State == CellState.Empty && Terrain.IsBuildable();
        public bool IsPassable => State != CellState.Occupied && Terrain.IsPassable();
    }
}
