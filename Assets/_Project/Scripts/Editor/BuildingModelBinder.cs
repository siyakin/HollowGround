using System;
using System.Collections.Generic;
using System.IO;
using HollowGround.Buildings;
using UnityEditor;
using UnityEngine;

namespace HollowGround.Editor
{
#if UNITY_EDITOR
    public static class BuildingModelBinder
    {
        private const string BUILDINGS_MODEL_PATH = "Assets/_Project/Models/Buildings";
        private const string BUILDINGS_SO_PATH = "Assets/_Project/ScriptableObjects/Buildings";

        private static readonly Dictionary<BuildingType, string[]> FolderNameMap = new()
        {
            { BuildingType.CommandCenter, new[] { "CommandCenter" } },
            { BuildingType.Farm, new[] { "Farm" } },
            { BuildingType.WaterWell, new[] { "WaterWell" } },
            { BuildingType.WoodFactory, new[] { "WoodFactory" } },
            { BuildingType.Mine, new[] { "Mine" } },
            { BuildingType.Barracks, new[] { "Barracks" } },
            { BuildingType.Hospital, new[] { "Hospital" } },
            { BuildingType.Generator, new[] { "Generator" } },
            { BuildingType.Storage, new[] { "Storage" } },
            { BuildingType.Shelter, new[] { "Shelter" } },
            { BuildingType.TradeCenter, new[] { "TradeCenter" } },
            { BuildingType.ResearchLab, new[] { "ResearchLab" } },
            { BuildingType.Workshop, new[] { "Workshop" } },
            { BuildingType.Walls, new[] { "Walls" } },
            { BuildingType.WatchTower, new[] { "WatchTower" } },
        };

        [MenuItem("HollowGround/Models/Bind All Building Models")]
        public static void BindAllBuildingModels()
        {
            string[] soGuids = AssetDatabase.FindAssets("t:BuildingData", new[] { BUILDINGS_SO_PATH });
            if (soGuids.Length == 0)
            {
                Debug.LogWarning("[BuildingModelBinder] No BuildingData SOs found in " + BUILDINGS_SO_PATH);
                return;
            }

            int bound = 0;
            int totalModels = 0;
            int missing = 0;

            for (int i = 0; i < soGuids.Length; i++)
            {
                string soPath = AssetDatabase.GUIDToAssetPath(soGuids[i]);
                var data = AssetDatabase.LoadAssetAtPath<BuildingData>(soPath);
                if (data == null) continue;

                if (!FolderNameMap.TryGetValue(data.Type, out string[] folderNames))
                {
                    Debug.LogWarning($"[BuildingModelBinder] No folder mapping for {data.Type}");
                    continue;
                }

                if (EditorUtility.DisplayCancelableProgressBar(
                    "Binding Building Models",
                    $"Processing {data.DisplayName} ({i + 1}/{soGuids.Length})",
                    (float)i / soGuids.Length))
                {
                    break;
                }

                Undo.RecordObject(data, $"Bind models: {data.DisplayName}");

                if (data.Models == null)
                    data.Models = new BuildingData.BuildingModels();

                int modelCount = 0;
                bool foundAny = false;

                foreach (string folderName in folderNames)
                {
                    string folderPath = $"{BUILDINGS_MODEL_PATH}/{folderName}";
                    if (!AssetDatabase.IsValidFolder(folderPath)) continue;

                    modelCount += TryBindModel(folderPath, data);
                    foundAny = true;
                }

                if (foundAny)
                {
                    EditorUtility.SetDirty(data);
                    bound++;
                    totalModels += modelCount;
                }
                else
                {
                    missing++;
                    Debug.LogWarning($"[BuildingModelBinder] No model folder found for {data.DisplayName} ({data.Type})");
                }
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();

            Debug.Log($"[BuildingModelBinder] Bound {totalModels} models across {bound} BuildingData SOs. Missing: {missing}.");
            Selection.activeObject = null;
        }

        [MenuItem("HollowGround/Models/Show Binding Report")]
        public static void ShowBindingReport()
        {
            string[] soGuids = AssetDatabase.FindAssets("t:BuildingData", new[] { BUILDINGS_SO_PATH });
            var report = new List<string> { "=== Building Model Binding Report ===\n" };

            foreach (string guid in soGuids)
            {
                string soPath = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<BuildingData>(soPath);
                if (data == null) continue;

                string status = GetBindingStatus(data);
                report.Add($"[{status}] {data.DisplayName} ({data.Type})");
                report.Add($"  Construct: {(data.Models?.ConstructModel != null ? "OK" : "MISSING")}");
                report.Add($"  L01: {(data.Models?.Level01Model != null ? "OK" : "MISSING")}");
                report.Add($"  L03: {(data.Models?.Level03Model != null ? "OK" : "MISSING")}");
                report.Add($"  L05: {(data.Models?.Level05Model != null ? "OK" : "MISSING")}");
                report.Add($"  L10: {(data.Models?.Level10Model != null ? "OK" : "MISSING")}");
                report.Add($"  Damaged: {(data.Models?.DamagedModel != null ? "OK" : "MISSING")}");
                report.Add($"  Destroyed: {(data.Models?.DestroyedModel != null ? "OK" : "MISSING")}");
                report.Add("");
            }

            Debug.Log(string.Join("\n", report));
        }

        private static int TryBindModel(string folderPath, BuildingData data)
        {
            string[] fbxGuids = AssetDatabase.FindAssets("t:Model", new[] { folderPath });
            int boundCount = 0;

            foreach (string fbxGuid in fbxGuids)
            {
                string fbxPath = AssetDatabase.GUIDToAssetPath(fbxGuid);
                string fileName = Path.GetFileNameWithoutExtension(fbxPath);
                string lower = fileName.ToLowerInvariant();

                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                if (model == null) continue;

                bool assigned = false;

                if (ContainsState(lower, "construct"))
                {
                    data.Models.ConstructModel = model;
                    assigned = true;
                }
                else if (ContainsState(lower, "destroyed"))
                {
                    data.Models.DestroyedModel = model;
                    assigned = true;
                }
                else if (ContainsState(lower, "damaged"))
                {
                    data.Models.DamagedModel = model;
                    assigned = true;
                }
                else if (ContainsLevel(lower, "l10"))
                {
                    data.Models.Level10Model = model;
                    assigned = true;
                }
                else if (ContainsLevel(lower, "l05"))
                {
                    data.Models.Level05Model = model;
                    assigned = true;
                }
                else if (ContainsLevel(lower, "l03"))
                {
                    data.Models.Level03Model = model;
                    assigned = true;
                }
                else if (ContainsLevel(lower, "l01"))
                {
                    data.Models.Level01Model = model;
                    assigned = true;
                }

                if (assigned)
                    boundCount++;
            }

            return boundCount;
        }

        private static bool ContainsState(string lowerName, string state)
        {
            return lowerName.Contains(state);
        }

        private static bool ContainsLevel(string lowerName, string level)
        {
            int idx = lowerName.LastIndexOf(level, StringComparison.Ordinal);
            if (idx < 0) return false;

            if (level == "l10")
                return lowerName.Contains("l10") && !lowerName.Contains("l100");

            return lowerName.Contains(level);
        }

        private static string GetBindingStatus(BuildingData data)
        {
            if (data.Models == null) return "EMPTY";

            int filled = 0;
            if (data.Models.ConstructModel != null) filled++;
            if (data.Models.Level01Model != null) filled++;
            if (data.Models.Level03Model != null) filled++;
            if (data.Models.Level05Model != null) filled++;
            if (data.Models.Level10Model != null) filled++;
            if (data.Models.DamagedModel != null) filled++;
            if (data.Models.DestroyedModel != null) filled++;

            if (filled == 7) return "COMPLETE";
            if (filled == 0) return "EMPTY";
            return $"PARTIAL ({filled}/7)";
        }
    }
#endif
}
