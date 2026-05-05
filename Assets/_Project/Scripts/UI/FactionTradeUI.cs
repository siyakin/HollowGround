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
        [Header("Left Panel")]
        [SerializeField] private Transform _factionList;

        [Header("Detail Panel")]
        [SerializeField] private GameObject _detailPanel;
        [SerializeField] private TMP_Text _detailName;
        [SerializeField] private TMP_Text _detailRelation;
        [SerializeField] private TMP_Text _detailDesc;
        [SerializeField] private Transform _sellContainer;
        [SerializeField] private Transform _buyContainer;

        private List<FactionData> _factions = new();
        private FactionData _selectedFaction;

        private void OnEnable()
        {
            RefreshFactions();

            if (TradeSystem.Instance != null)
                TradeSystem.Instance.OnTradeCompleted += OnTradeCompleted;
        }

        private void OnDisable()
        {
            if (TradeSystem.Instance != null)
                TradeSystem.Instance.OnTradeCompleted -= OnTradeCompleted;
        }

        private void OnTradeCompleted(FactionData faction)
        {
            ToastUI.Show($"Trade completed! Relation with {faction.DisplayName}: {faction.RelationPoints}", UIColors.Default.Ok);
            if (_selectedFaction != null)
                ShowTradePanel(_selectedFaction);
            RefreshFactions();
        }

        private void RefreshFactions()
        {
            if (_factionList == null) return;

            for (int i = _factionList.childCount - 1; i >= 0; i--)
                Destroy(_factionList.GetChild(i).gameObject);

            _factions.Clear();

            var allFactions = UnityEngine.Resources.LoadAll<FactionData>("Factions");
            if (allFactions == null || allFactions.Length == 0)
            {
                var empty = UIPrimitiveFactory.AddThemedText(_factionList, "No factions found. Create FactionData SOs.", 15, UIColors.Default.Muted);
                empty.alignment = TextAlignmentOptions.Center;
                return;
            }

            foreach (var faction in allFactions)
                _factions.Add(faction);

            foreach (var faction in _factions)
            {
                var row = UIPrimitiveFactory.CreateUIObject($"Faction_{faction.name}", _factionList);
                UIPrimitiveFactory.AddLayoutElement(row.gameObject, preferredHeight: 50);
                var rbg = row.gameObject.AddComponent<Image>();
                rbg.color = UIColors.Default.RowBg;
                var hlg = UIPrimitiveFactory.AddRowHLG(row.gameObject);

                var nameT = UIPrimitiveFactory.AddThemedText(row, faction.DisplayName, 16, UIColors.Default.Text);
                nameT.alignment = TextAlignmentOptions.MidlineLeft;
                UIPrimitiveFactory.AddLayoutElement(nameT.gameObject, preferredWidth: 140);

                var relColor = faction.CanTrade() ? UIColors.Default.Ok : UIColors.Default.Danger;
                var relT = UIPrimitiveFactory.AddThemedText(row, $"{faction.GetCurrentRelation()} ({faction.RelationPoints})", 14, relColor);
                relT.alignment = TextAlignmentOptions.MidlineRight;

                var btn = row.gameObject.AddComponent<Button>();
                btn.targetGraphic = rbg;
                btn.interactable = faction.CanTrade();
                var captured = faction;
                btn.onClick.AddListener(() =>
                {
                    _selectedFaction = captured;
                    ShowTradePanel(captured);
                });
            }
        }

        private void ShowTradePanel(FactionData faction)
        {
            if (_detailPanel == null || faction == null) return;
            _detailPanel.SetActive(true);

            _detailName.text = faction.DisplayName;
            _detailRelation.text = $"Relation: {faction.GetCurrentRelation()} ({faction.RelationPoints} pts)";
            _detailDesc.text = faction.Description;

            ClearContainer(_sellContainer);
            ClearContainer(_buyContainer);

            foreach (var offer in faction.Sells)
                CreateOfferRow(_sellContainer, offer, true);

            foreach (var offer in faction.Buys)
                CreateOfferRow(_buyContainer, offer, false);
        }

        private void CreateOfferRow(Transform container, FactionData.TradeOffer offer, bool isBuy)
        {
            var row = UIPrimitiveFactory.CreateUIObject("Offer", container);
            UIPrimitiveFactory.AddLayoutElement(row.gameObject, preferredHeight: 36);
            var hlg = UIPrimitiveFactory.AddRowHLG(row.gameObject, new RectOffset(8, 8, 4, 4));

            var nameT = UIPrimitiveFactory.AddThemedText(row, $"{offer.Resource} x{offer.Amount}", 14, UIColors.Default.Text);
            nameT.alignment = TextAlignmentOptions.MidlineLeft;
            UIPrimitiveFactory.AddLayoutElement(nameT.gameObject, preferredWidth: 140);

            var priceT = UIPrimitiveFactory.AddThemedText(row, $"{offer.Price} TechPart", 14, UIColors.Default.Gold);
            priceT.alignment = TextAlignmentOptions.MidlineLeft;
            UIPrimitiveFactory.AddLayoutElement(priceT.gameObject, preferredWidth: 100);

            var btn = UIPrimitiveFactory.CreateThemedButton(row, "Btn", isBuy ? "BUY" : "SELL", () =>
            {
                if (isBuy)
                {
                    if (TradeSystem.Instance != null && TradeSystem.Instance.BuyFrom(_selectedFaction, offer))
                        ShowTradePanel(_selectedFaction);
                    else
                        ToastUI.Show("Trade failed!", UIColors.Default.Danger);
                }
                else
                {
                    if (TradeSystem.Instance != null && TradeSystem.Instance.SellTo(_selectedFaction, offer))
                        ShowTradePanel(_selectedFaction);
                    else
                        ToastUI.Show("Trade failed!", UIColors.Default.Danger);
                }
            }, UIStyleType.ConfirmButton);
            UIPrimitiveFactory.AddLayoutElement(btn.gameObject, minWidth: 70, preferredWidth: 90, minHeight: 28);

            bool canAfford = isBuy
                ? TradeSystem.Instance != null && TradeSystem.Instance.CanBuyFrom(_selectedFaction, offer)
                : ResourceManager.Instance != null && ResourceManager.Instance.Get(offer.Resource) >= offer.Amount;

            btn.interactable = canAfford;
        }

        private void ClearContainer(Transform container)
        {
            if (container == null) return;
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }
    }
}
