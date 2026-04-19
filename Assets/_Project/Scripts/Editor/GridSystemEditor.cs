using HollowGround.Buildings;
using HollowGround.Grid;
using UnityEngine;

namespace HollowGround.Editor
{
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(GridSystem))]
    public class GridSystemEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GridSystem grid = (GridSystem)target;

            GUILayout.Space(10);
            GUILayout.Label("Debug Info", UnityEditor.EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                GUILayout.Label("Grid info available in Play mode only.");
                return;
            }

            int empty = 0, occupied = 0, blocked = 0;
            for (int x = 0; x < grid.Width; x++)
            {
                for (int z = 0; z < grid.Height; z++)
                {
                    GridCell cell = grid.GetCell(x, z);
                    if (cell == null) continue;
                    switch (cell.State)
                    {
                        case CellState.Empty: empty++; break;
                        case CellState.Occupied: occupied++; break;
                        case CellState.Blocked: blocked++; break;
                    }
                }
            }

            GUILayout.Label($"Empty: {empty} | Occupied: {occupied} | Blocked: {blocked}");
            GUILayout.Label($"Total Cells: {grid.Width * grid.Height}");

            if (GUILayout.Button("Reset Grid"))
            {
                UnityEditor.Undo.RecordObject(target, "Reset Grid");
                grid.ResetGrid();
            }
        }
    }
#endif
}
