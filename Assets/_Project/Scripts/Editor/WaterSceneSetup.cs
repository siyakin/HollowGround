#if UNITY_EDITOR
using HollowGround.Grid;
using UnityEditor;
using UnityEngine;

namespace HollowGround.Editor
{
    public static class WaterSceneSetup
    {
        private const string TemplatePath = "Assets/_Project/ScriptableObjects/Maps/MapTemplate.asset";
        private const float CellSize = 2f;

        [MenuItem("HollowGround/Terrain/Setup Water Scene")]
        public static void SetupWaterScene()
        {
            var mapRenderer = Object.FindAnyObjectByType<MapRenderer>();
            if (mapRenderer == null)
            {
                Debug.LogWarning("[WaterSceneSetup] No MapRenderer found in scene.");
                return;
            }

            var waterSurface = mapRenderer.GetComponent<WaterSurface>();
            if (waterSurface == null)
                waterSurface = Undo.AddComponent<WaterSurface>(mapRenderer.gameObject);

            var template = AssetDatabase.LoadAssetAtPath<MapTemplate>(TemplatePath);
            if (template == null)
            {
                Debug.LogWarning($"[WaterSceneSetup] MapTemplate not found at: {TemplatePath}");
                return;
            }

            var gridSystem = Object.FindAnyObjectByType<GridSystem>();
            if (gridSystem == null)
            {
                Debug.LogWarning("[WaterSceneSetup] No GridSystem found in scene.");
                return;
            }

            Undo.RecordObject(mapRenderer, "Setup Water Scene");
            gridSystem.ApplyMapTemplate(template);

            int minX = int.MaxValue, maxX = int.MinValue;
            int minZ = int.MaxValue, maxZ = int.MinValue;
            int waterTileCount = 0;

            for (int x = 0; x < template.Width; x++)
            {
                for (int z = 0; z < template.Height; z++)
                {
                    var t = template.GetTile(x, z);
                    if (t != TerrainType.Water && t != TerrainType.River) continue;
                    waterTileCount++;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (z < minZ) minZ = z;
                    if (z > maxZ) maxZ = z;
                }
            }

            if (waterTileCount == 0)
            {
                Debug.Log("[WaterSceneSetup] No water tiles found in template.");
                Selection.activeObject = mapRenderer;
                return;
            }

            float waterY = new SerializedObject(mapRenderer).FindProperty("_waterY").floatValue;
            var minWorld = gridSystem.GetWorldPosition(minX, minZ);
            var maxWorld = gridSystem.GetWorldPosition(maxX, maxZ);
            var center = new Vector3(
                (minWorld.x + maxWorld.x) * 0.5f,
                waterY - 0.25f,
                (minWorld.z + maxWorld.z) * 0.5f
            );

            float width = (maxX - minX + 1) * CellSize + 1f;
            float depth = (maxZ - minZ + 1) * CellSize + 1f;
            const float bodyHeight = 0.5f;

            waterSurface.CreateWaterBody(center, width, depth, bodyHeight);

            var waterMat = waterSurface.GetWaterMaterial();
            int waterObjectCount = 0;
            if (waterMat != null)
            {
                var renderers = Object.FindObjectsByType<MeshRenderer>();
                foreach (var mr in renderers)
                {
                    if (mr.sharedMaterial == waterMat)
                        waterObjectCount++;
                }
            }

            Debug.Log($"[WaterSceneSetup] Water tiles: {waterTileCount}, Water body: {width}x{depth}x{bodyHeight}, Total water objects: {waterObjectCount}");
            Selection.activeObject = mapRenderer;
        }

        [MenuItem("HollowGround/Terrain/Clear Water")]
        public static void ClearWater()
        {
            var mapRenderer = Object.FindAnyObjectByType<MapRenderer>();
            if (mapRenderer == null) return;
            Undo.RecordObject(mapRenderer, "Clear Water");
            mapRenderer.ClearAllImmediate();
            Debug.Log("[WaterSceneSetup] Cleared all terrain and water.");
        }

        [MenuItem("HollowGround/Terrain/Reset MapTemplate to Flat")]
        public static void ResetMapTemplate()
        {
            string[] paths = { TemplatePath, "Assets/_Project/ScriptableObjects/Maps/DefaultMap.asset" };
            int count = 0;
            foreach (var path in paths)
            {
                var template = AssetDatabase.LoadAssetAtPath<MapTemplate>(path);
                if (template == null) continue;
                Undo.RecordObject(template, "Reset MapTemplate to Flat");
                template.Fill(TerrainType.Flat);
                EditorUtility.SetDirty(template);
                count++;
            }
            if (count > 0) AssetDatabase.SaveAssets();
            Debug.Log($"[WaterSceneSetup] Reset {count} template(s) to all Flat.");
        }

        [MenuItem("HollowGround/Terrain/Create Lake in MapTemplate")]
        public static void CreateLakeInMapTemplate()
        {
            var template = AssetDatabase.LoadAssetAtPath<MapTemplate>(TemplatePath);
            if (template == null)
            {
                Debug.LogWarning($"[WaterSceneSetup] MapTemplate not found at: {TemplatePath}");
                return;
            }

            Undo.RecordObject(template, "Create Lake in MapTemplate");
            template.CarveRect(20, 20, 8, 8, TerrainType.Water);
            template.CarveRect(19, 24, 1, 4, TerrainType.River);
            EditorUtility.SetDirty(template);
            AssetDatabase.SaveAssets();
            Debug.Log("[WaterSceneSetup] Lake created: 8x8 Water (20,20) + 1x4 River (19,24).");
        }

        [MenuItem("HollowGround/Terrain/Check URP Water Requirements")]
        public static void CheckURPRequirements()
        {
            var urpAssets = UnityEditor.AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
            if (urpAssets.Length == 0)
            {
                Debug.LogWarning("[WaterSceneSetup] No URP Asset found in project.");
                return;
            }

            foreach (var guid in urpAssets)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset>(path);
                if (asset == null) continue;

                var so = new SerializedObject(asset);
                bool depthTexture = so.FindProperty("m_RequireDepthTexture")?.boolValue ?? false;
                bool opaqueTexture = so.FindProperty("m_RequireOpaqueTexture")?.boolValue ?? false;

                Debug.Log($"[WaterSceneSetup] URP Asset: {path}\n" +
                    $"  Depth Texture: {(depthTexture ? "ON" : "OFF (REQUIRED for water depth)")}\n" +
                    $"  Opaque Texture: {(opaqueTexture ? "ON" : "OFF (REQUIRED for water refraction)")}\n" +
                    $"  Water shader will {(depthTexture ? "work" : "NOT work correctly")} without Depth Texture.");

                if (!depthTexture || !opaqueTexture)
                {
                    Debug.LogWarning("[WaterSceneSetup] Enable Depth Texture and Opaque Texture in URP Asset for water shader to work correctly.\n" +
                        "Project Settings > Graphics > URP Asset > Depth Texture + Opaque Texture");
                }
            }
        }

        [MenuItem("HollowGround/Terrain/Enable URP Water Settings")]
        public static void EnableURPWaterSettings()
        {
            var urpAssets = UnityEditor.AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
            if (urpAssets.Length == 0)
            {
                Debug.LogWarning("[WaterSceneSetup] No URP Asset found in project.");
                return;
            }

            int fixedCount = 0;
            foreach (var guid in urpAssets)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset>(path);
                if (asset == null) continue;

                var so = new SerializedObject(asset);
                so.Update();

                var depthProp = so.FindProperty("m_RequireDepthTexture");
                var opaqueProp = so.FindProperty("m_RequireOpaqueTexture");

                bool changed = false;
                if (depthProp != null && !depthProp.boolValue) { depthProp.boolValue = true; changed = true; }
                if (opaqueProp != null && !opaqueProp.boolValue) { opaqueProp.boolValue = true; changed = true; }

                if (changed)
                {
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(asset);
                    fixedCount++;
                    Debug.Log($"[WaterSceneSetup] Enabled Depth + Opaque Texture in: {path}");
                }
            }

            if (fixedCount > 0)
            {
                UnityEditor.AssetDatabase.SaveAssets();
                Debug.Log($"[WaterSceneSetup] Updated {fixedCount} URP Asset(s). Restart play mode to apply.");
            }
            else
            {
                Debug.Log("[WaterSceneSetup] All URP Assets already have Depth + Opaque Texture enabled.");
            }
        }
    }
}
#endif
