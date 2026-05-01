using System.Collections.Generic;
using HollowGround.Buildings;
using HollowGround.NPCs;
using UnityEditor;
using UnityEngine;

namespace HollowGround.Editor
{
#if UNITY_EDITOR
    public static class SettlerJobDataFactory
    {
        [MenuItem("HollowGround/Settlers/Apply Default Worker Requirements to All Buildings")]
        public static void ApplyDefaultWorkerRequirements()
        {
            string[] guids = AssetDatabase.FindAssets("t:BuildingData");
            int updated = 0;

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<BuildingData>(path);
                if (data == null) continue;

                var workers = GetDefaultWorkers(data.Type);
                if (workers == null || workers.Count == 0)
                {
                    data.RequiredWorkers = new List<WorkerSlot>();
                    data.WorkerProductionBonus = 0f;
                }
                else
                {
                    data.RequiredWorkers = workers;
                    data.WorkerProductionBonus = GetDefaultBonus(data.Type);
                }

                EditorUtility.SetDirty(data);
                updated++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[SettlerJobDataFactory] Updated {updated} BuildingData assets with worker requirements.");
        }

        [MenuItem("HollowGround/Settlers/Show Worker Requirements Report")]
        public static void ShowReport()
        {
            string[] guids = AssetDatabase.FindAssets("t:BuildingData");
            string report = "=== Building Worker Requirements ===\n\n";

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<BuildingData>(path);
                if (data == null) continue;

                report += $"{data.DisplayName ?? data.name} ({data.Type}):\n";
                if (data.RequiredWorkers == null || data.RequiredWorkers.Count == 0)
                {
                    report += "  No workers required\n";
                }
                else
                {
                    foreach (var slot in data.RequiredWorkers)
                        report += $"  {SettlerRoleInfo.GetDisplayName(slot.Role)} x{slot.Count}\n";
                    report += $"  Bonus: {data.WorkerProductionBonus:P0}\n";
                }
                report += "\n";
            }

            Debug.Log(report);
        }

        private static List<WorkerSlot> GetDefaultWorkers(BuildingType type)
        {
            return type switch
            {
                BuildingType.Farm => new List<WorkerSlot>
                {
                    new() { Role = SettlerRole.Farmer, Count = 2 }
                },
                BuildingType.Mine => new List<WorkerSlot>
                {
                    new() { Role = SettlerRole.Miner, Count = 2 }
                },
                BuildingType.WoodFactory => new List<WorkerSlot>
                {
                    new() { Role = SettlerRole.Woodcutter, Count = 2 }
                },
                BuildingType.WaterWell => new List<WorkerSlot>
                {
                    new() { Role = SettlerRole.WaterCarrier, Count = 1 }
                },
                BuildingType.Generator => new List<WorkerSlot>
                {
                    new() { Role = SettlerRole.Engineer, Count = 1 }
                },
                BuildingType.Workshop => new List<WorkerSlot>
                {
                    new() { Role = SettlerRole.Engineer, Count = 1 }
                },
                BuildingType.ResearchLab => new List<WorkerSlot>
                {
                    new() { Role = SettlerRole.Researcher, Count = 1 }
                },
                BuildingType.Hospital => new List<WorkerSlot>
                {
                    new() { Role = SettlerRole.Medic, Count = 1 }
                },
                BuildingType.WatchTower => new List<WorkerSlot>
                {
                    new() { Role = SettlerRole.Guard, Count = 1 }
                },
                BuildingType.Barracks => new List<WorkerSlot>
                {
                    new() { Role = SettlerRole.Guard, Count = 1 }
                },
                BuildingType.TradeCenter => new List<WorkerSlot>
                {
                    new() { Role = SettlerRole.Trader, Count = 1 }
                },
                BuildingType.Storage => new List<WorkerSlot>
                {
                    new() { Role = SettlerRole.Hauler, Count = 1 }
                },
                BuildingType.CommandCenter => new List<WorkerSlot>
                {
                    new() { Role = SettlerRole.Hauler, Count = 1 }
                },
                _ => null
            };
        }

        private static float GetDefaultBonus(BuildingType type)
        {
            return type switch
            {
                BuildingType.Farm => 0.5f,
                BuildingType.Mine => 0.5f,
                BuildingType.WoodFactory => 0.5f,
                BuildingType.WaterWell => 0.4f,
                BuildingType.Generator => 0.4f,
                BuildingType.Workshop => 0.3f,
                BuildingType.ResearchLab => 0.4f,
                BuildingType.Hospital => 0.3f,
                BuildingType.WatchTower => 0.2f,
                BuildingType.Barracks => 0.2f,
                BuildingType.TradeCenter => 0.3f,
                BuildingType.Storage => 0.2f,
                BuildingType.CommandCenter => 0.2f,
                _ => 0f
            };
        }
    }
#endif
}
