using System.Collections.Generic;
using HollowGround.Army;
using HollowGround.Resources;

namespace HollowGround.Combat
{
    public class BattleReport
    {
        public string TargetName;
        public bool Victory;
        public Dictionary<TroopType, int> AttackerLosses;
        public Dictionary<TroopType, int> Survivors;
        public Dictionary<ResourceType, int> Loot;
        public int TotalAttackerPower;
        public int TotalDefenderPower;
        public float PowerRatio;
    }
}
