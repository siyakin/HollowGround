using System.Collections.Generic;
using HollowGround.Buildings;
using HollowGround.Resources;

namespace HollowGround.Core
{
    public static class CostEntryHelper
    {
        public static List<BuildingData.CostEntry> Costs(params object[] pairs)
        {
            var list = new List<BuildingData.CostEntry>();
            for (int i = 0; i < pairs.Length - 1; i += 2)
            {
                list.Add(new BuildingData.CostEntry
                {
                    Type = (ResourceType)pairs[i],
                    Amount = (int)pairs[i + 1]
                });
            }
            return list;
        }
    }
}
