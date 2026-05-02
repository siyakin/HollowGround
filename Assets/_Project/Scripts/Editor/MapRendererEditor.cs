#if UNITY_EDITOR
using HollowGround.Grid;
using UnityEditor;
using UnityEngine;

namespace HollowGround.Editor
{
    [CustomEditor(typeof(MapRenderer))]
    public class MapRendererEditor : UnityEditor.Editor
    {
        private TerrainType _paintTerrain = TerrainType.Water;
        private int _brushSize = 1;
        private bool _paintMode;
        private bool _eraseMode;
        private MapRenderer _renderer;
        private GridSystem _gridSystem;

        private void OnEnable()
        {
            _renderer = (MapRenderer)target;
            _gridSystem = Object.FindAnyObjectByType<GridSystem>();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Terrain Painting Tool", EditorStyles.boldLabel);

            _paintTerrain = (TerrainType)EditorGUILayout.EnumPopup("Terrain Type", _paintTerrain);
            _brushSize = EditorGUILayout.IntSlider("Brush Size", _brushSize, 1, 5);

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = _paintMode ? Color.green : Color.white;
            if (GUILayout.Button(_paintMode ? "Painting ON" : "Paint", GUILayout.Height(30)))
            {
                _paintMode = !_paintMode;
                _eraseMode = false;
                SceneView.RepaintAll();
            }

            GUI.backgroundColor = _eraseMode ? Color.yellow : Color.white;
            if (GUILayout.Button(_eraseMode ? "Erasing ON" : "Erase", GUILayout.Height(30)))
            {
                _eraseMode = !_eraseMode;
                _paintMode = false;
                SceneView.RepaintAll();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            if (_paintMode || _eraseMode)
            {
                EditorGUILayout.HelpBox(
                    _paintMode
                        ? $"Click/drag in Scene to paint {_paintTerrain}. Hold Shift to erase."
                        : "Click/drag in Scene to erase terrain back to Flat.",
                    MessageType.Info);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Materials"))
                MapRenderer.CreateDefaultMaterials(_renderer);
            if (GUILayout.Button("Apply Template"))
                TerrainEditorMenu.ApplyToScene();
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Create Default MapTemplate"))
                TerrainEditorMenu.CreateDefaultMapTemplate();
        }

        private void OnSceneGUI()
        {
            if (!_paintMode && !_eraseMode) return;
            if (_gridSystem == null)
                _gridSystem = Object.FindAnyObjectByType<GridSystem>();
            if (_gridSystem == null) return;

            Event e = Event.current;
            if (e.type == EventType.Layout) return;

            if (e.type == EventType.MouseDrag || e.type == EventType.MouseDown)
            {
                if (e.button != 0) return;

                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                var groundMask = LayerMask.GetMask("Ground") | LayerMask.GetMask("Default");
                if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundMask))
                {
                    var plane = new Plane(Vector3.up, Vector3.zero);
                    if (plane.Raycast(ray, out float dist))
                        hit.point = ray.GetPoint(dist);
                    else return;
                }

                var coords = _gridSystem.GetGridCoordinates(hit.point);
                bool erasing = _eraseMode || e.shift;
                TerrainType type = erasing ? TerrainType.Flat : _paintTerrain;

                PaintBrush(coords.x, coords.y, type);
                e.Use();
            }

            if ((_paintMode || _eraseMode) && (e.type == EventType.Layout || e.type == EventType.Repaint))
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
        }

        private void PaintBrush(int cx, int cz, TerrainType type)
        {
            if (_gridSystem == null) return;

            int half = _brushSize / 2;
            bool changed = false;
            for (int dx = -half; dx <= half; dx++)
            {
                for (int dz = -half; dz <= half; dz++)
                {
                    int x = cx + dx;
                    int z = cz + dz;
                    if (!_gridSystem.IsValidCoordinate(x, z)) continue;

                    var cell = _gridSystem.GetCell(x, z);
                    if (cell == null) continue;
                    if (cell.State == CellState.Occupied) continue;

                    if (cell.Terrain == type) continue;
                    _gridSystem.SetTerrain(x, z, type);
                    changed = true;
                }
            }

            if (changed)
                EditorUtility.SetDirty(_renderer);
        }
    }
}
#endif
