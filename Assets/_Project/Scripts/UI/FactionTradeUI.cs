using System.Collections.Generic;
using HollowGround.NPCs;
using HollowGround.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class FactionTradeUI : MonoBehaviour
    {
        [System.Serializable]
        public class FactionSlot
        {
            public FactionData Data;
            public Button Button;
            public TMP_Text NameText;
            public TMP_Text RelationText;
            public Image EmblemImage;
        }

        [SerializeField] private List<FactionSlot> _factionSlots = new();
        [SerializeField] private GameObject _tradePanel;
        [SerializeField] private TMP_Text _factionNameText;
        [SerializeField] private TMP_Text _factionRelationText;
        [SerializeField] private TMP_Text _factionDescText;
        [SerializeField] private Transform _sellOffersContainer;
        [SerializeField] private Transform _buyOffersContainer;
        [SerializeField] private GameObject _offerItemPrefab;
        [SerializeField] private Button _closeTradeBtn;

        private FactionData _selectedFaction;

        private void OnEnable()
        {
            RefreshFactions();
        }

        public void RefreshFactions()
        {
            foreach (var slot in _factionSlots)
            {
                if (slot.Data == null) continue;

                if (slot.NameText != null)
                    slot.NameText.text = slot.Data.DisplayName;

                if (slot.RelationText != null)
                    slot.RelationText.text = slot.Data.GetCurrentRelation().ToString();

                if (slot.EmblemImage != null && slot.Data.Emblem != null)
                    slot.EmblemImage.sprite = slot.Data.Emblem;

                if (slot.Button != null)
                    slot.Button.interactable = slot.Data.CanTrade();
            }
        }

        public void SelectFaction(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _factionSlots.Count) return;

            _selectedFaction = _factionSlots[slotIndex].Data;
            ShowTradePanel(_selectedFaction);
        }

        private void ShowTradePanel(FactionData faction)
        {
            if (_tradePanel == null || faction == null) return;

            _tradePanel.SetActive(true);

            if (_factionNameText != null)
                _factionNameText.text = faction.DisplayName;

            if (_factionRelationText != null)
                _factionRelationText.text = $"Relation: {faction.GetCurrentRelation()} ({faction.RelationPoints})";

            if (_factionDescText != null)
                _factionDescText.text = faction.Description;

            PopulateOffers(faction);
        }

        private void PopulateOffers(FactionData faction)
        {
            ClearContainer(_sellOffersContainer);
            ClearContainer(_buyOffersContainer);

            if (_offerItemPrefab == null) return;

            for (int i = 0; i < faction.Sells.Count; i++)
            {
                var offer = faction.Sells[i];
                var item = Instantiate(_offerItemPrefab, _sellOffersContainer);
                SetupOfferItem(item, offer, true, i);
            }

            for (int i = 0; i < faction.Buys.Count; i++)
            {
                var offer = faction.Buys[i];
                var item = Instantiate(_offerItemPrefab, _buyOffersContainer);
                SetupOfferItem(item, offer, false, i);
            }
        }

        private void SetupOfferItem(GameObject item, FactionData.TradeOffer offer, bool isBuy, int index)
        {
            var nameText = item.transform.Find("NameText")?.GetComponent<TMP_Text>();
            var priceText = item.transform.Find("PriceText")?.GetComponent<TMP_Text>();
            var actionBtn = item.transform.Find("ActionButton")?.GetComponent<Button>();
            var actionText = actionBtn?.GetComponentInChildren<TMP_Text>();

            if (nameText != null)
                nameText.text = $"{offer.Resource} x{offer.Amount}";

            if (priceText != null)
                priceText.text = $"{offer.Price} TechParts";

            if (actionBtn != null)
            {
                bool canAfford = isBuy
                    ? TradeSystem.Instance != null && TradeSystem.Instance.CanBuyFrom(_selectedFaction, offer)
                    : HasResourceToSell(offer);

                actionBtn.interactable = canAfford;

                if (actionText != null)
                    actionText.text = isBuy ? "Buy" : "Sell";

                actionBtn.onClick.AddListener(() =>
                {
                    if (isBuy)
                        ExecuteBuy(offer);
                    else
                        ExecuteSell(offer);
                });
            }
        }

        private bool HasResourceToSell(FactionData.TradeOffer offer)
        {
            if (ResourceManager.Instance == null) return false;
            return ResourceManager.Instance.Get(offer.Resource) >= offer.Amount;
        }

        private void ExecuteBuy(FactionData.TradeOffer offer)
        {
            if (TradeSystem.Instance == null || _selectedFaction == null) return;

            if (TradeSystem.Instance.BuyFrom(_selectedFaction, offer))
            {
                ShowTradePanel(_selectedFaction);
                RefreshFactions();
            }
        }

        private void ExecuteSell(FactionData.TradeOffer offer)
        {
            if (TradeSystem.Instance == null || _selectedFaction == null) return;

            if (TradeSystem.Instance.SellTo(_selectedFaction, offer))
            {
                ShowTradePanel(_selectedFaction);
                RefreshFactions();
            }
        }

        public void CloseTrade()
        {
            if (_tradePanel != null)
                _tradePanel.SetActive(false);
            _selectedFaction = null;
        }

        private void ClearContainer(Transform container)
        {
            if (container == null) return;
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }
    }
}
