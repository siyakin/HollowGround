using System.Collections.Generic;
using HollowGround.NPCs;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.NPCs
{
    public class TradeSystem : MonoBehaviour
    {
        public static TradeSystem Instance { get; private set; }

        public event System.Action<FactionData> OnTradeCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool CanBuyFrom(FactionData faction, FactionData.TradeOffer offer)
        {
            if (!faction.CanTrade()) return false;
            if (ResourceManager.Instance == null) return false;

            var cost = new Dictionary<ResourceType, int> { { ResourceType.TechPart, offer.Price } };
            return ResourceManager.Instance.CanAfford(cost);
        }

        public bool BuyFrom(FactionData faction, FactionData.TradeOffer offer)
        {
            if (!CanBuyFrom(faction, offer)) return false;

            ResourceManager.Instance.Spend(ResourceType.TechPart, offer.Price);
            ResourceManager.Instance.Add(offer.Resource, offer.Amount);

            faction.ChangeRelation(1);
            OnTradeCompleted?.Invoke(faction);
            return true;
        }

        public bool SellTo(FactionData faction, FactionData.TradeOffer offer)
        {
            if (!faction.CanTrade()) return false;
            if (ResourceManager.Instance == null) return false;
            if (ResourceManager.Instance.Get(offer.Resource) < offer.Amount) return false;

            ResourceManager.Instance.Spend(offer.Resource, offer.Amount);
            ResourceManager.Instance.Add(ResourceType.TechPart, offer.Price);

            faction.ChangeRelation(1);
            OnTradeCompleted?.Invoke(faction);
            return true;
        }
    }
}
