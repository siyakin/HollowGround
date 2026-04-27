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
        private TMP_Text _headerText;
        private TMP_Text _detailName;
        private TMP_Text _detailRelation;
        private TMP_Text _detailDesc;
        private Transform _sellContainer;
        private Transform _buyContainer;
        private GameObject _detailPanel;
        private Transform _factionList;
        private List<FactionData> _factions = new();
        private FactionData _selectedFaction;
        private bool _built;

        private void OnEnable()
        {
            if (!_built) BuildUI();
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

        private void BuildUI()
        {
            var root = GetComponent<RectTransform>();
            if (root == null) return;

            UIPrimitiveFactory.SetupPanelBackground(gameObject, UIColors.Default);
            UIPrimitiveFactory.StretchFull(root, new Vector2(0f, 60f), Vector2.zero);

            foreach (Transform child in root)
                Destroy(child.gameObject);

            var mainHLG = gameObject.AddComponent<HorizontalLayoutGroup>();
            mainHLG.padding = new RectOffset(15, 15, 15, 15);
            mainHLG.spacing = 15;
            mainHLG.childControlWidth = true;
            mainHLG.childControlHeight = true;
            mainHLG.childForceExpandWidth = true;
            mainHLG.childForceExpandHeight = true;

            var leftPanel = new GameObject("FactionList", typeof(RectTransform));
            leftPanel.transform.SetParent(root, false);
            var leftBg = leftPanel.AddComponent<Image>();
            leftBg.color = UIColors.Default.RowBg;
            var leftVLG = leftPanel.AddComponent<VerticalLayoutGroup>();
            leftVLG.padding = new RectOffset(10, 10, 10, 10);
            leftVLG.spacing = 6;
            leftVLG.childControlWidth = true;
            leftVLG.childControlHeight = false;
            leftVLG.childForceExpandWidth = true;
            leftVLG.childForceExpandHeight = false;

            var leftHeader = UIPrimitiveFactory.AddThemedText(leftPanel.transform, "FACTIONS", 22, UIColors.Default.Gold);
            leftHeader.alignment = TextAlignmentOptions.Center;
            UIPrimitiveFactory.AddLayoutElement(leftHeader.gameObject, preferredHeight: 35);

            var listObj = UIPrimitiveFactory.CreateUIObject("List", leftPanel.transform);
            UIPrimitiveFactory.AddLayoutElement(listObj.gameObject, preferredHeight: 300);
            var listVLG = listObj.gameObject.AddComponent<VerticalLayoutGroup>();
            listVLG.spacing = 4;
            listVLG.childControlWidth = true;
            listVLG.childControlHeight = false;
            listVLG.childForceExpandWidth = true;
            listVLG.childForceExpandHeight = false;
            _factionList = listObj.transform;

            _detailPanel = UIPrimitiveFactory.CreateUIObject("DetailPanel", root).gameObject;
            var detailBg = _detailPanel.AddComponent<Image>();
            detailBg.color = UIColors.Default.RowBg;
            var detailVLG = _detailPanel.AddComponent<VerticalLayoutGroup>();
            detailVLG.padding = new RectOffset(15, 15, 10, 10);
            detailVLG.spacing = 8;
            detailVLG.childControlWidth = true;
            detailVLG.childControlHeight = false;
            detailVLG.childForceExpandWidth = true;
            detailVLG.childForceExpandHeight = false;

            _detailName = UIPrimitiveFactory.AddThemedText(_detailPanel.transform, "Select a faction", 22, UIColors.Default.Text);
            _detailRelation = UIPrimitiveFactory.AddThemedText(_detailPanel.transform, "", 16, UIColors.Default.Muted);
            _detailDesc = UIPrimitiveFactory.AddThemedText(_detailPanel.transform, "", 15, UIColors.Default.Muted);
            UIPrimitiveFactory.AddLayoutElement(_detailDesc.gameObject, preferredHeight: 50);

            UIPrimitiveFactory.AddThemedText(_detailPanel.transform, "-- BUY FROM FACTION --", 16, UIColors.Default.Ok).alignment = TextAlignmentOptions.Center;
            var sellObj = UIPrimitiveFactory.CreateUIObject("SellList", _detailPanel.transform);
            UIPrimitiveFactory.AddLayoutElement(sellObj.gameObject, preferredHeight: 120);
            var sellVLG = sellObj.gameObject.AddComponent<VerticalLayoutGroup>();
            sellVLG.spacing = 3;
            sellVLG.childControlWidth = true;
            sellVLG.childControlHeight = false;
            sellVLG.childForceExpandWidth = true;
            sellVLG.childForceExpandHeight = false;
            _sellContainer = sellObj.transform;

            UIPrimitiveFactory.AddThemedText(_detailPanel.transform, "-- SELL TO FACTION --", 16, UIColors.Default.Gold).alignment = TextAlignmentOptions.Center;
            var buyObj = UIPrimitiveFactory.CreateUIObject("BuyList", _detailPanel.transform);
            UIPrimitiveFactory.AddLayoutElement(buyObj.gameObject, preferredHeight: 120);
            var buyVLG = buyObj.gameObject.AddComponent<VerticalLayoutGroup>();
            buyVLG.spacing = 3;
            buyVLG.childControlWidth = true;
            buyVLG.childControlHeight = false;
            buyVLG.childForceExpandWidth = true;
            buyVLG.childForceExpandHeight = false;
            _buyContainer = buyObj.transform;

            _detailPanel.SetActive(false);
            _built = true;
        }

        private void RefreshFactions()
        {
            if (!_built || _factionList == null) return;

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

            var btnObj = UIPrimitiveFactory.CreateUIObject("Btn", row);
            UIPrimitiveFactory.AddLayoutElement(btnObj.gameObject, minWidth: 70, preferredWidth: 90, minHeight: 28);
            var btnImg = btnObj.gameObject.AddComponent<Image>();
            btnImg.color = isBuy ? UIColors.Default.Ok : UIColors.Default.Gold;
            var btn = btnObj.gameObject.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            var btnLabel = UIPrimitiveFactory.AddThemedText(btnObj, isBuy ? "BUY" : "SELL", 14, Color.black);
            btnLabel.alignment = TextAlignmentOptions.Center;
            UIPrimitiveFactory.StretchFull(btnLabel.rectTransform);

            bool canAfford = isBuy
                ? TradeSystem.Instance != null && TradeSystem.Instance.CanBuyFrom(_selectedFaction, offer)
                : ResourceManager.Instance != null && ResourceManager.Instance.Get(offer.Resource) >= offer.Amount;

            btn.interactable = canAfford;

            var captured = offer;
            btn.onClick.AddListener(() =>
            {
                if (isBuy)
                {
                    if (TradeSystem.Instance != null && TradeSystem.Instance.BuyFrom(_selectedFaction, captured))
                        ShowTradePanel(_selectedFaction);
                    else
                        ToastUI.Show("Trade failed!", UIColors.Default.Danger);
                }
                else
                {
                    if (TradeSystem.Instance != null && TradeSystem.Instance.SellTo(_selectedFaction, captured))
                        ShowTradePanel(_selectedFaction);
                    else
                        ToastUI.Show("Trade failed!", UIColors.Default.Danger);
                }
            });
        }

        private void ClearContainer(Transform container)
        {
            if (container == null) return;
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }
    }
}
