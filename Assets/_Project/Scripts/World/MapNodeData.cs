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

    [CreateAssetMenu(fileName = "MapNodeData", menuName = "HollowGround/MapNodeData")]
    public class MapNodeData : ScriptableObject
    {
        [Header("Info")]
        public string DisplayName;
        public string Description;
        public MapNodeType NodeType;
        public Vector2Int GridPosition;
        public Sprite Icon;

        [Header("Discovery")]
        public bool StartsRevealed;
        public int RevealRadius = 1;

        [Header("Content")]
        public BattleTarget BattleTarget;
        public bool IsRepeatable = true;
        public bool IsCleared;

        [Header("Neighbors (set at runtime)")]
        public bool AutoConnect = true;

        public bool HasBattle => BattleTarget != null;
        public bool IsExplored => _isExplored;
        public bool IsVisible => _isVisible;

        private bool _isExplored;
        private bool _isVisible;

        public void SetExplored(bool value) => _isExplored = value;
        public void SetVisible(bool value) => _isVisible = value;

        public MapNodeData GetRuntimeCopy()
        {
            var copy = Instantiate(this);
            copy._isExplored = StartsRevealed;
            copy._isVisible = StartsRevealed;
            copy.IsCleared = false;
            return copy;
        }
    }
}
