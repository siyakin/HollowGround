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
        private static readonly Color PanelBg = new(0.08f, 0.09f, 0.11f, 0.92f);
        private static readonly Color RowBg = new(0.14f, 0.15f, 0.17f, 1f);
        private static readonly Color ColorText = new(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color ColorMuted = new(0.65f, 0.65f, 0.7f, 1f);
        private static readonly Color ColorOk = new(0.35f, 0.8f, 0.4f, 1f);
        private static readonly Color ColorGold = new(1f, 0.85f, 0.3f, 1f);
        private static readonly Color ColorDanger = new(0.9f, 0.3f, 0.3f, 1f);

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
            ToastUI.Show($"Trade completed! Relation with {faction.DisplayName}: {faction.RelationPoints}", ColorOk);
            if (_selectedFaction != null)
                ShowTradePanel(_selectedFaction);
            RefreshFactions();
        }

        private void BuildUI()
        {
            var root = GetComponent<RectTransform>();
            if (root == null) return;

            root.anchorMin = new Vector2(0f, 0f);
            root.anchorMax = new Vector2(1f, 1f);
            root.offsetMin = new Vector2(0f, 60f);
            root.offsetMax = new Vector2(0f, 0f);

            foreach (Transform child in root)
                Destroy(child.gameObject);

            var oldVlg = GetComponent<VerticalLayoutGroup>();
            if (oldVlg != null) DestroyImmediate(oldVlg);
            var oldImages = GetComponents<Image>();
            foreach (var img in oldImages) DestroyImmediate(img);
            var oldCg = GetComponent<CanvasGroup>();
            if (oldCg != null) DestroyImmediate(oldCg);

            var bg = gameObject.AddComponent<Image>();
            bg.color = PanelBg;
            bg.raycastTarget = true;

            var cg = gameObject.AddComponent<CanvasGroup>();
            cg.interactable = true;
            cg.blocksRaycasts = true;

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
            leftBg.color = RowBg;
            var leftVLG = leftPanel.AddComponent<VerticalLayoutGroup>();
            leftVLG.padding = new RectOffset(10, 10, 10, 10);
            leftVLG.spacing = 6;
            leftVLG.childControlWidth = true;
            leftVLG.childControlHeight = false;
            leftVLG.childForceExpandWidth = true;
            leftVLG.childForceExpandHeight = false;

            var leftHeader = AddText(leftPanel.transform, "FACTIONS", 22, ColorGold);
            leftHeader.alignment = TextAlignmentOptions.Center;
            var lhLE = leftHeader.gameObject.AddComponent<LayoutElement>();
            lhLE.preferredHeight = 35;

            var listObj = new GameObject("List", typeof(RectTransform));
            listObj.transform.SetParent(leftPanel.transform, false);
            var listLE = listObj.AddComponent<LayoutElement>();
            listLE.preferredHeight = 300;
            var listVLG = listObj.AddComponent<VerticalLayoutGroup>();
            listVLG.spacing = 4;
            listVLG.childControlWidth = true;
            listVLG.childControlHeight = false;
            listVLG.childForceExpandWidth = true;
            listVLG.childForceExpandHeight = false;
            _factionList = listObj.transform;

            _detailPanel = new GameObject("DetailPanel", typeof(RectTransform));
            _detailPanel.transform.SetParent(root, false);
            var detailBg = _detailPanel.AddComponent<Image>();
            detailBg.color = RowBg;
            var detailVLG = _detailPanel.AddComponent<VerticalLayoutGroup>();
            detailVLG.padding = new RectOffset(15, 15, 10, 10);
            detailVLG.spacing = 8;
            detailVLG.childControlWidth = true;
            detailVLG.childControlHeight = false;
            detailVLG.childForceExpandWidth = true;
            detailVLG.childForceExpandHeight = false;

            _detailName = AddText(_detailPanel.transform, "Select a faction", 22, ColorText);
            _detailRelation = AddText(_detailPanel.transform, "", 16, ColorMuted);
            _detailDesc = AddText(_detailPanel.transform, "", 15, ColorMuted);
            var descLE = _detailDesc.gameObject.AddComponent<LayoutElement>();
            descLE.preferredHeight = 50;

            AddText(_detailPanel.transform, "-- BUY FROM FACTION --", 16, ColorOk).alignment = TextAlignmentOptions.Center;
            var sellObj = new GameObject("SellList", typeof(RectTransform));
            sellObj.transform.SetParent(_detailPanel.transform, false);
            var sellLE = sellObj.AddComponent<LayoutElement>();
            sellLE.preferredHeight = 120;
            var sellVLG = sellObj.AddComponent<VerticalLayoutGroup>();
            sellVLG.spacing = 3;
            sellVLG.childControlWidth = true;
            sellVLG.childControlHeight = false;
            sellVLG.childForceExpandWidth = true;
            sellVLG.childForceExpandHeight = false;
            _sellContainer = sellObj.transform;

            AddText(_detailPanel.transform, "-- SELL TO FACTION --", 16, ColorGold).alignment = TextAlignmentOptions.Center;
            var buyObj = new GameObject("BuyList", typeof(RectTransform));
            buyObj.transform.SetParent(_detailPanel.transform, false);
            var buyLE = buyObj.AddComponent<LayoutElement>();
            buyLE.preferredHeight = 120;
            var buyVLG = buyObj.AddComponent<VerticalLayoutGroup>();
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
                var empty = AddText(_factionList, "No factions found. Create FactionData SOs.", 15, ColorMuted);
                empty.alignment = TextAlignmentOptions.Center;
                return;
            }

            foreach (var faction in allFactions)
                _factions.Add(faction);

            foreach (var faction in _factions)
            {
                var row = new GameObject($"Faction_{faction.name}", typeof(RectTransform));
                row.transform.SetParent(_factionList, false);
                var le = row.AddComponent<LayoutElement>();
                le.preferredHeight = 50;
                var rbg = row.AddComponent<Image>();
                rbg.color = RowBg;
                var hlg = row.AddComponent<HorizontalLayoutGroup>();
                hlg.padding = new RectOffset(12, 12, 6, 6);
                hlg.spacing = 10;
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = true;
                hlg.childForceExpandHeight = false;

                var nameT = AddText(row.transform, faction.DisplayName, 16, ColorText);
                nameT.alignment = TextAlignmentOptions.MidlineLeft;
                var nle = nameT.gameObject.AddComponent<LayoutElement>();
                nle.preferredWidth = 140;

                var relColor = faction.CanTrade() ? ColorOk : ColorDanger;
                var relT = AddText(row.transform, $"{faction.GetCurrentRelation()} ({faction.RelationPoints})", 14, relColor);
                relT.alignment = TextAlignmentOptions.MidlineRight;

                var btn = row.AddComponent<Button>();
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
            var row = new GameObject("Offer", typeof(RectTransform));
            row.transform.SetParent(container, false);
            var le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 36;
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(8, 8, 4, 4);
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;

            var nameT = AddText(row.transform, $"{offer.Resource} x{offer.Amount}", 14, ColorText);
            nameT.alignment = TextAlignmentOptions.MidlineLeft;
            var nle = nameT.gameObject.AddComponent<LayoutElement>();
            nle.preferredWidth = 140;

            var priceT = AddText(row.transform, $"{offer.Price} TechPart", 14, ColorGold);
            priceT.alignment = TextAlignmentOptions.MidlineLeft;
            var ple = priceT.gameObject.AddComponent<LayoutElement>();
            ple.preferredWidth = 100;

            var btnObj = new GameObject("Btn", typeof(RectTransform));
            btnObj.transform.SetParent(row.transform, false);
            var btnLE = btnObj.AddComponent<LayoutElement>();
            btnLE.minWidth = 70;
            btnLE.preferredWidth = 90;
            btnLE.minHeight = 28;
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = isBuy ? ColorOk : ColorGold;
            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            var btnLabel = AddText(btnObj.transform, isBuy ? "BUY" : "SELL", 14, Color.black);
            btnLabel.alignment = TextAlignmentOptions.Center;
            StretchFull(btnLabel.rectTransform);

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
                        ToastUI.Show("Trade failed!", ColorDanger);
                }
                else
                {
                    if (TradeSystem.Instance != null && TradeSystem.Instance.SellTo(_selectedFaction, captured))
                        ShowTradePanel(_selectedFaction);
                    else
                        ToastUI.Show("Trade failed!", ColorDanger);
                }
            });
        }

        private void ClearContainer(Transform container)
        {
            if (container == null) return;
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }

        private static TMP_Text AddText(Transform parent, string text, float size, Color color)
        {
            var go = new GameObject("T", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
