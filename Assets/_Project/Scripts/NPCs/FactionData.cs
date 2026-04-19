using System.Collections.Generic;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.NPCs
{
    public enum FactionRelation
    {
        Hostile,
        Neutral,
        Friendly,
        Allied
    }

    [CreateAssetMenu(fileName = "FactionData", menuName = "HollowGround/FactionData")]
    public class FactionData : ScriptableObject
    {
        [Header("Info")]
        public string DisplayName;
        public string Description;
        public Sprite Emblem;

        [Header("Relation")]
        public FactionRelation DefaultRelation = FactionRelation.Neutral;
        public int RelationPoints;
        public int FriendlyThreshold = 50;
        public int AlliedThreshold = 100;

        [Header("Trade")]
        public List<TradeOffer> Sells = new();
        public List<TradeOffer> Buys = new();

        [System.Serializable]
        public class TradeOffer
        {
            public ResourceType Resource;
            public int Amount;
            public int Price;
        }

        public FactionRelation GetCurrentRelation()
        {
            if (RelationPoints >= AlliedThreshold) return FactionRelation.Allied;
            if (RelationPoints >= FriendlyThreshold) return FactionRelation.Friendly;
            if (RelationPoints >= 0) return FactionRelation.Neutral;
            return FactionRelation.Hostile;
        }

        public void ChangeRelation(int amount)
        {
            RelationPoints = Mathf.Clamp(RelationPoints + amount, -100, 200);
        }

        public bool CanTrade()
        {
            return GetCurrentRelation() != FactionRelation.Hostile;
        }
    }
}
