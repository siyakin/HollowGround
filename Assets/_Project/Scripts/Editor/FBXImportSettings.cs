using System.IO;
using UnityEditor;
using UnityEngine;

namespace HollowGround.Editor
{
#if UNITY_EDITOR
    public static class FBXImportSettings
    {
        private const string BUILDINGS_PATH = "Assets/_Project/Models/Buildings";
        private const string MENU_PATH = "HollowGround/FBX/";

        [MenuItem(MENU_PATH + "Configure All Building FBX Imports")]
        public static void ConfigureAllBuildingFBX()
        {
            string[] fbxGuids = AssetDatabase.FindAssets("t:Model", new[] { BUILDINGS_PATH });
            if (fbxGuids.Length == 0)
            {
                Debug.LogWarning("[FBXImportSettings] No FBX files found in " + BUILDINGS_PATH);
                return;
            }

            int configured = 0;
            int skipped = 0;

            for (int i = 0; i < fbxGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(fbxGuids[i]);
                if (Path.GetExtension(assetPath).ToLower() != ".fbx") continue;

                if (EditorUtility.DisplayCancelableProgressBar(
                    "Configuring FBX Imports",
                    $"Processing {Path.GetFileName(assetPath)} ({i + 1}/{fbxGuids.Length})",
                    (float)i / fbxGuids.Length))
                {
                    break;
                }

                if (ConfigureSingleFBX(assetPath))
                    configured++;
                else
                    skipped++;
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[FBXImportSettings] Configured {configured} FBX files, skipped {skipped}.");
        }

        public static void ForceReimportAll()
        {
            string[] fbxGuids = AssetDatabase.FindAssets("t:Model", new[] { BUILDINGS_PATH });

            for (int i = 0; i < fbxGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(fbxGuids[i]);
                if (Path.GetExtension(assetPath).ToLower() != ".fbx") continue;

                if (EditorUtility.DisplayCancelableProgressBar(
                    "Force Reimporting FBX",
                    $"Reimporting {Path.GetFileName(assetPath)} ({i + 1}/{fbxGuids.Length})",
                    (float)i / fbxGuids.Length))
                {
                    break;
                }

                ConfigureSingleFBX(assetPath, force: true);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[FBXImportSettings] Force reimported {fbxGuids.Length} FBX files.");
        }

        private static bool ConfigureSingleFBX(string assetPath, bool force = false)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null) return false;

            if (!force && IsAlreadyConfigured(importer))
                return false;

            Undo.RecordObject(importer, $"Configure FBX: {Path.GetFileName(assetPath)}");

            importer.useFileScale = true;
            importer.globalScale = 1f;
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.isReadable = false;
            importer.optimizeMeshVertices = true;
            importer.optimizeMeshPolygons = true;
            importer.importBlendShapes = false;
            importer.importVisibility = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.sortHierarchyByName = true;
            importer.indexFormat = ModelImporterIndexFormat.Auto;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.swapUVChannels = false;

            ApplySerializedSettings(importer);

            try
            {
                importer.SaveAndReimport();
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FBXImportSettings] Failed to configure {assetPath}: {e.Message}");
                return false;
            }
        }

        private static void ApplySerializedSettings(ModelImporter importer)
        {
            var so = new SerializedObject(importer);

            SetBoolProperty(so, "m_ImportColors", true);
            SetBoolProperty(so, "m_GenerateColliders", false);
            SetBoolProperty(so, "m_GenerateSecondaryUV", false);
            SetIntProperty(so, "m_Normals", 1);
            SetIntProperty(so, "m_Tangents", 1);

            so.ApplyModifiedProperties();
        }

        private static void SetBoolProperty(SerializedObject so, string name, bool value)
        {
            var prop = so.FindProperty(name);
            if (prop != null)
                prop.boolValue = value;
        }

        private static void SetIntProperty(SerializedObject so, string name, int value)
        {
            var prop = so.FindProperty(name);
            if (prop != null)
                prop.intValue = value;
        }

        private static bool IsAlreadyConfigured(ModelImporter importer)
        {
            return Mathf.Approximately(importer.globalScale, 1f)
                && !importer.isReadable
                && !importer.importBlendShapes
                && !importer.importCameras
                && !importer.importLights;
        }
    }
#endif
}
