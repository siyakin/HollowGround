using HollowGround.Core;
using UnityEngine;

namespace HollowGround.Grid
{
    public class GridSystem : Singleton<GridSystem>
    {

        [Header("Grid Settings")]
        [SerializeField] private int _width = 50;
        [SerializeField] private int _height = 50;
        [SerializeField] private float _cellSize = 2f;
        [SerializeField] private Vector3 _origin = Vector3.zero;

        public int Width => _width;
        public int Height => _height;
        public float CellSize => _cellSize;

        private GridCell[,] _cells;

        protected override void Awake()
        {
            base.Awake();
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            _cells = new GridCell[_width, _height];
            for (int x = 0; x < _width; x++)
            {
                for (int z = 0; z < _height; z++)
                {
                    Vector3 worldPos = GetWorldPosition(x, z);
                    _cells[x, z] = new GridCell(x, z, worldPos);
                }
            }
        }

        public Vector3 GetWorldPosition(int x, int z)
        {
            return new Vector3(
                _origin.x + x * _cellSize + _cellSize * 0.5f,
                _origin.y,
                _origin.z + z * _cellSize + _cellSize * 0.5f
            );
        }

        public Vector2Int GetGridCoordinates(Vector3 worldPosition)
        {
            int x = Mathf.FloorToInt((worldPosition.x - _origin.x) / _cellSize);
            int z = Mathf.FloorToInt((worldPosition.z - _origin.z) / _cellSize);
            return new Vector2Int(x, z);
        }

        public GridCell GetCell(int x, int z)
        {
            if (!IsValidCoordinate(x, z)) return null;
            return _cells[x, z];
        }

        public GridCell GetCell(Vector3 worldPosition)
        {
            var coords = GetGridCoordinates(worldPosition);
            return GetCell(coords.x, coords.y);
        }

        public bool IsValidCoordinate(int x, int z)
        {
            return x >= 0 && x < _width && z >= 0 && z < _height;
        }

        public bool IsAreaBuildable(int startX, int startZ, int sizeX, int sizeZ)
        {
            for (int x = startX; x < startX + sizeX; x++)
            {
                for (int z = startZ; z < startZ + sizeZ; z++)
                {
                    GridCell cell = GetCell(x, z);
                    if (cell == null || !cell.IsBuildable)
                        return false;
                }
            }
            return true;
        }

        public void OccupyCells(int startX, int startZ, int sizeX, int sizeZ, GameObject occupant)
        {
            for (int x = startX; x < startX + sizeX; x++)
            {
                for (int z = startZ; z < startZ + sizeZ; z++)
                {
                    GridCell cell = GetCell(x, z);
                    if (cell != null)
                    {
                        cell.State = CellState.Occupied;
                        cell.Occupant = occupant;
                    }
                }
            }
        }

        public void FreeCells(int startX, int startZ, int sizeX, int sizeZ)
        {
            for (int x = startX; x < startX + sizeX; x++)
            {
                for (int z = startZ; z < startZ + sizeZ; z++)
                {
                    GridCell cell = GetCell(x, z);
                    if (cell != null)
                    {
                        cell.State = CellState.Empty;
                        cell.Occupant = null;
                    }
                }
            }
        }

        public Vector3 SnapToGrid(Vector3 worldPosition)
        {
            var coords = GetGridCoordinates(worldPosition);
            return GetWorldPosition(coords.x, coords.y);
        }

        public void ResetGrid()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int z = 0; z < _height; z++)
                {
                    _cells[x, z].State = CellState.Empty;
                    _cells[x, z].Occupant = null;
                }
            }
        }
    }
}
