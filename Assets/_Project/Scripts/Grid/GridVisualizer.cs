using UnityEngine;

namespace HollowGround.Grid
{
    public class GridVisualizer : MonoBehaviour
    {
        [SerializeField] private GridSystem _gridSystem;
        [SerializeField] private Color _lineColor = new(1f, 1f, 1f, 0.15f);
        [SerializeField] private Color _occupiedColor = new(1f, 0f, 0f, 0.3f);
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

            Gizmos.color = _occupiedColor;
            for (int x = 0; x < w; x++)
            {
                for (int z = 0; z < h; z++)
                {
                    GridCell cell = _gridSystem.GetCell(x, z);
                    if (cell != null && cell.State == CellState.Occupied)
                    {
                        Vector3 center = _gridSystem.GetWorldPosition(x, z);
                        center.y += 0.1f;
                        Gizmos.DrawCube(center, new Vector3(cs * 0.9f, 0.1f, cs * 0.9f));
                    }
                }
            }
        }
    }
}
