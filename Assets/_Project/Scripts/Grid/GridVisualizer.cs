using UnityEngine;

namespace HollowGround.Grid
{
    public class GridVisualizer : MonoBehaviour
    {
        [SerializeField] private GridSystem _gridSystem;
        [SerializeField] private Color _lineColor = new(1f, 1f, 1f, 0.15f);
        [SerializeField] private Color _occupiedColor = new(1f, 0f, 0f, 0.3f);
        [SerializeField] private Color _waterColor = new(0.1f, 0.3f, 0.6f, 0.3f);
        [SerializeField] private Color _mountainColor = new(0.35f, 0.3f, 0.25f, 0.3f);
        [SerializeField] private Color _rockColor = new(0.45f, 0.42f, 0.38f, 0.3f);
        [SerializeField] private Color _forestColor = new(0.2f, 0.45f, 0.15f, 0.3f);
        [SerializeField] private Color _sandColor = new(0.76f, 0.7f, 0.5f, 0.2f);
        [SerializeField] private Color _riverColor = new(0.15f, 0.35f, 0.55f, 0.3f);
        [SerializeField] private Color _cliffColor = new(0.4f, 0.35f, 0.3f, 0.3f);
        [SerializeField] private bool _showGrid = true;

        private void OnDrawGizmos()
        {
            if (!_showGrid || _gridSystem == null) return;

            int w = _gridSystem.Width;
            int h = _gridSystem.Height;
            float cs = _gridSystem.CellSize;

            Gizmos.color = _lineColor;

            for (int x = 0; x <= w; x++)
            {
                Vector3 start = _gridSystem.GetWorldPosition(x, 0) - new Vector3(cs * 0.5f, 0, cs * 0.5f);
                Vector3 end = _gridSystem.GetWorldPosition(x, h) - new Vector3(cs * 0.5f, 0, cs * 0.5f);
                Gizmos.DrawLine(start, end);
            }

            for (int z = 0; z <= h; z++)
            {
                Vector3 start = _gridSystem.GetWorldPosition(0, z) - new Vector3(cs * 0.5f, 0, cs * 0.5f);
                Vector3 end = _gridSystem.GetWorldPosition(w, z) - new Vector3(cs * 0.5f, 0, cs * 0.5f);
                Gizmos.DrawLine(start, end);
            }

            for (int x = 0; x < w; x++)
            {
                for (int z = 0; z < h; z++)
                {
                    GridCell cell = _gridSystem.GetCell(x, z);
                    if (cell == null) continue;

                    Vector3 center = _gridSystem.GetWorldPosition(x, z);
                    center.y += 0.1f;

                    if (cell.State == CellState.Occupied)
                    {
                        Gizmos.color = _occupiedColor;
                        Gizmos.DrawCube(center, new Vector3(cs * 0.9f, 0.1f, cs * 0.9f));
                    }
                    else if (cell.Terrain != TerrainType.Flat)
                    {
                        Gizmos.color = GetTerrainColor(cell.Terrain);
                        Gizmos.DrawCube(center, new Vector3(cs * 0.9f, 0.1f, cs * 0.9f));
                    }
                }
            }
        }

        private Color GetTerrainColor(TerrainType terrain)
        {
            return terrain switch
            {
                TerrainType.Water => _waterColor,
                TerrainType.River => _riverColor,
                TerrainType.Mountain => _mountainColor,
                TerrainType.Rock => _rockColor,
                TerrainType.Cliff => _cliffColor,
                TerrainType.Sand => _sandColor,
                TerrainType.Forest => _forestColor,
                _ => _occupiedColor
            };
        }
    }
}
