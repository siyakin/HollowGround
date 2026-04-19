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

        public GridCell(int x, int z, Vector3 worldPosition)
        {
            X = x;
            Z = z;
            WorldPosition = worldPosition;
            State = CellState.Empty;
            Occupant = null;
        }

        public bool IsBuildable => State == CellState.Empty;
    }
}
