using HollowGround.Combat;
using UnityEngine;

namespace HollowGround.World
{
    public enum MapNodeType
    {
        PlayerBase,
        ResourceNode,
        MutantCamp,
        AbandonedBuilding,
        NPCSettlement,
        RadioactiveZone,
        BossArea
    }

    public class MapNodeData
    {
        public string DisplayName;
        public string Description;
        public MapNodeType NodeType;
        public Vector2Int GridPosition;

        public bool StartsRevealed;
        public int RevealRadius = 1;

        public BattleTarget BattleTarget;
        public bool IsRepeatable = true;
        public bool IsCleared;

        private bool _isExplored;
        private bool _isVisible;

        public bool HasBattle => BattleTarget != null;
        public bool IsExplored => _isExplored;
        public bool IsVisible => _isVisible;

        public void SetExplored(bool value) => _isExplored = value;
        public void SetVisible(bool value) => _isVisible = value;
    }
}
