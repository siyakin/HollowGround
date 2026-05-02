#if UNITY_EDITOR
using HollowGround.Grid;
using UnityEditor;
using UnityEngine;

namespace HollowGround.Editor
{
    public static class TerrainEditorMenu
    {
        [MenuItem("HollowGround/Terrain/Create Empty MapTemplate")]
        public static void CreateDefaultMapTemplate()
        {
            string folder = "Assets/_Project/ScriptableObjects/Maps";
            if (!AssetDatabase.IsValidFolder("Assets/_Project/ScriptableObjects/Maps"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/ScriptableObjects"))
                    AssetDatabase.CreateFolder("Assets/_Project", "ScriptableObjects");
                AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "Maps");
            }

            string path = $"{folder}/MapTemplate.asset";
            var template = ScriptableObject.CreateInstance<MapTemplate>();
            template.Initialize(50, 50);
            template.Fill(TerrainType.Flat);
            AssetDatabase.CreateAsset(template, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = template;
            Debug.Log("[Terrain] Created empty MapTemplate (50x50, all Flat). Use Paint tool to add terrain.");
        }

        [MenuItem("HollowGround/Terrain/Create Terrain Materials")]
        public static void CreateMaterials()
        {
            var go = new GameObject("_TempMapRenderer");
            var renderer = go.AddComponent<MapRenderer>();
            MapRenderer.CreateDefaultMaterials(renderer);
            Object.DestroyImmediate(go);
            Debug.Log("[Terrain] Created terrain materials in Assets/_Project/Materials/Terrain/");
        }

        [MenuItem("HollowGround/Terrain/Apply MapTemplate to Scene")]
        public static void ApplyToScene()
        {
            var template = Selection.activeObject as MapTemplate;
            if (template == null)
            {
                Debug.LogWarning("[Terrain] Select a MapTemplate asset first.");
                return;
            }

            var gridSystem = Object.FindAnyObjectByType<GridSystem>();
            if (gridSystem == null)
            {
                Debug.LogWarning("[Terrain] No GridSystem in scene.");
                return;
            }

            Undo.RecordObject(gridSystem, "Apply MapTemplate");
            gridSystem.ApplyMapTemplate(template);
            Debug.Log($"[Terrain] Applied {template.name} to scene ({template.Width}x{template.Height}).");
        }

        [MenuItem("HollowGround/Terrain/Clear Scene Terrain")]
        public static void ClearSceneTerrain()
        {
            var gridSystem = Object.FindAnyObjectByType<GridSystem>();
            if (gridSystem == null) return;
            Undo.RecordObject(gridSystem, "Clear Terrain");
            gridSystem.ClearTerrain();
            Debug.Log("[Terrain] Cleared all terrain from scene.");
        }

        [MenuItem("HollowGround/Terrain/Fix Missing TerrainTile Components")]
        public static void FixMissingTerrainTileComponents()
        {
            var root = GameObject.Find("TerrainTiles");
            if (root == null)
            {
                Debug.LogWarning("[Terrain] No TerrainTiles object in scene.");
                return;
            }

            var gridSystem = Object.FindAnyObjectByType<GridSystem>();
            if (gridSystem == null) return;

            int fixedCount = 0;
            foreach (Transform child in root.transform)
            {
                if (child.GetComponent<TerrainTile>() != null) continue;

                string name = child.gameObject.name;
                var parts = name.Split('_');
                if (parts.Length < 4) continue;

                if (!System.Enum.TryParse<TerrainType>(parts[1], out var type)) continue;
                if (!int.TryParse(parts[2], out int gx)) continue;
                if (!int.TryParse(parts[3], out int gz)) continue;

                var tag = child.gameObject.AddComponent<TerrainTile>();
                tag.TerrainType = type;
                tag.GridX = gx;
                tag.GridZ = gz;
                fixedCount++;
            }

            Debug.Log($"[Terrain] Fixed {fixedCount} tiles with missing TerrainTile component.");
        }
    }
}
#endif
