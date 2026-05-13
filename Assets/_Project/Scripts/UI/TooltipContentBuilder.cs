using System.Collections.Generic;
using System.Text;
using HollowGround.Buildings;
using HollowGround.NPCs;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.UI
{
    public static class TooltipContentBuilder
    {
        public static TooltipData ForBuilding(Building building)
        {
            var data = building.Data;
            var tooltip = new TooltipData
            {
                Title = data.DisplayName,
                Subtitle = $"Lv.{building.Level} {data.Category}",
                Description = data.Description,
                HasTitleColor = false
            };

            if (building.State != BuildingState.Placing)
            {
                tooltip.HasState = true;
                tooltip.StateText = building.State.ToString();
                tooltip.StateColor = UIColors.GetStateColor(building.State);
            }

            if (data.HasProduction && building.State == BuildingState.Active)
            {
                int prod = data.GetProductionForLevel(building.Level);
                tooltip.InfoLines.Add($"Produces {prod} {data.ProducedResource}/{FormatTime(data.ProductionInterval)}");
            }

            if (data.PopulationCapacity > 0)
                tooltip.InfoLines.Add($"Population Capacity: +{data.PopulationCapacity}");

            if (data.StorageCapacity > 0)
                tooltip.InfoLines.Add($"Storage Capacity: +{data.StorageCapacity}");

            int totalWorkers = data.GetTotalRequiredWorkers();
            if (totalWorkers > 0)
            {
                int assigned = building.AssignedWorkerCount;
                tooltip.InfoLines.Add($"Workers: {assigned}/{totalWorkers}");
            }

            if (data.NeedsRoads)
                tooltip.InfoLines.Add("Requires road connection");

            return tooltip;
        }

        public static TooltipData ForBuildingData(BuildingData data)
        {
            var tooltip = new TooltipData
            {
                Title = data.DisplayName,
                Subtitle = $"{data.Category} | {data.SizeX}x{data.SizeZ}",
                Description = data.Description,
                HasTitleColor = false
            };

            var costs = data.GetCostForLevel(1);
            if (costs.Count > 0 && ResourceManager.Instance != null)
            {
                foreach (var kvp in costs)
                {
                    tooltip.Costs.Add(new TooltipData.CostLine
                    {
                        Type = kvp.Key,
                        Amount = kvp.Value,
                        Have = ResourceManager.Instance.Get(kvp.Key),
                        ShowHave = true
                    });
                }
            }

            if (data.HasProduction)
                tooltip.InfoLines.Add($"Produces {data.BaseProductionAmount} {data.ProducedResource}/{FormatTime(data.ProductionInterval)}");

            if (data.PopulationCapacity > 0)
                tooltip.InfoLines.Add($"Population Capacity: +{data.PopulationCapacity}");

            if (data.StorageCapacity > 0)
                tooltip.InfoLines.Add($"Storage Capacity: +{data.StorageCapacity}");

            int totalWorkers = data.GetTotalRequiredWorkers();
            if (totalWorkers > 0)
                tooltip.InfoLines.Add($"Workers needed: {totalWorkers}");

            if (data.CommandCenterLevelRequired > 1)
            {
                int ccLevel = BuildingManager.Instance != null ? BuildingManager.Instance.GetCommandCenterLevel() : 0;
                bool locked = data.CommandCenterLevelRequired > ccLevel;
                tooltip.InfoLines.Add(locked
                    ? $"Requires Command Center Lv.{data.CommandCenterLevelRequired}"
                    : $"Command Center Lv.{data.CommandCenterLevelRequired} (met)");
            }

            if (data.NeedsRoads)
                tooltip.InfoLines.Add("Requires road connection");

            return tooltip;
        }

        public static TooltipData ForSettler(SettlerWalker settler)
        {
            var tooltip = new TooltipData
            {
                Title = "Settler",
                Subtitle = SettlerRoleInfo.GetDisplayName(settler.Role),
                Description = GetSettlerDescription(settler),
                HasTitleColor = false
            };

            if (settler.HasJob && settler.AssignedBuilding != null)
                tooltip.InfoLines.Add($"Assigned: {settler.AssignedBuilding.Data.DisplayName}");

            tooltip.InfoLines.Add($"Task: {settler.CurrentTask}");

            return tooltip;
        }

        public static TooltipData ForText(string title, string description)
        {
            return new TooltipData
            {
                Title = title,
                Description = description,
                HasTitleColor = false
            };
        }

        public static TooltipData ForText(string title, string subtitle, string description)
        {
            return new TooltipData
            {
                Title = title,
                Subtitle = subtitle,
                Description = description,
                HasTitleColor = false
            };
        }

        public static TooltipData ForText(string title, string description, Color titleColor)
        {
            return new TooltipData
            {
                Title = title,
                Description = description,
                TitleColor = titleColor,
                HasTitleColor = true
            };
        }

        private static string GetSettlerDescription(SettlerWalker settler)
        {
            var sb = new StringBuilder();
            sb.Append("A survivor working in your settlement.");
            if (settler.Role == SettlerRole.None)
                sb.Append(" Currently idle.");
            return sb.ToString();
        }

        private static string FormatTime(float seconds)
        {
            if (seconds < 60) return $"{(int)seconds}s";
            int min = (int)(seconds / 60);
            int sec = (int)(seconds % 60);
            return sec > 0 ? $"{min}m {sec}s" : $"{min}m";
        }
    }
}
