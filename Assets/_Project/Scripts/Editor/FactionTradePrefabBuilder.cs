using HollowGround.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HollowGround.Editor
{
    public static class FactionTradePrefabBuilder
    {
        [MenuItem("HollowGround/UI/Build FactionTrade Panel")]
        public static void Build()
        {
            var canvas = GameObject.Find("GameCanvas");
            if (canvas == null) { Debug.LogError("GameCanvas not found!"); return; }

            var existing = canvas.transform.Find("FactionTradePanel");
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Replace?", "FactionTradePanel exists. Replace?", "Yes", "No")) return;
                Object.DestroyImmediate(existing.gameObject);
            }

            var root = new GameObject("FactionTradePanel", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            var rt = root.GetComponent<RectTransform>();
            PanelBuilderUtil.StretchFull(rt);
            rt.offsetMin = new Vector2(0, 60);
            rt.offsetMax = Vector2.zero;
            root.AddComponent<Image>().color = PanelBuilderUtil.PanelBg;
            var mainHLG = root.AddComponent<HorizontalLayoutGroup>();
            mainHLG.padding = new RectOffset(15, 15, 15, 15);
            mainHLG.spacing = 15;
            mainHLG.childControlWidth = true;
            mainHLG.childControlHeight = true;
            mainHLG.childForceExpandWidth = true;
            mainHLG.childForceExpandHeight = true;

            // Left panel
            var leftPanel = new GameObject("FactionListPanel", typeof(RectTransform));
            leftPanel.transform.SetParent(root.transform, false);
            leftPanel.AddComponent<Image>().color = PanelBuilderUtil.SectionBg;
            var leftVLG = leftPanel.AddComponent<VerticalLayoutGroup>();
            leftVLG.padding = new RectOffset(10, 10, 10, 10);
            leftVLG.spacing = 6;
            leftVLG.childControlWidth = true;
            leftVLG.childControlHeight = false;
            leftVLG.childForceExpandWidth = true;
            leftVLG.childForceExpandHeight = false;

            var leftHeader = PanelBuilderUtil.CreateHeader("Header", leftPanel.transform, "FACTIONS", 22);
            leftHeader.alignment = TextAlignmentOptions.Center;
            leftHeader.GetComponent<LayoutElement>().preferredHeight = 35;

            Transform factionListContainer;
            PanelBuilderUtil.CreateScrollView("FactionList", leftPanel.transform, out factionListContainer);

            // Right panel (detail)
            var detailPanel = PanelBuilderUtil.CreateSection("DetailPanel", root.transform, new RectOffset(15, 15, 10, 10));
            var detailVLG = detailPanel.GetComponent<VerticalLayoutGroup>();
            detailVLG.spacing = 8;

            var detailName = PanelBuilderUtil.CreateTMP("DetailName", detailPanel.transform, "Select a faction", 22, PanelBuilderUtil.TextColor);
            var detailRelation = PanelBuilderUtil.CreateTMP("DetailRelation", detailPanel.transform, "", 16, PanelBuilderUtil.LabelColor);
            var detailDesc = PanelBuilderUtil.CreateTMP("DetailDesc", detailPanel.transform, "", 15, PanelBuilderUtil.LabelColor);
            detailDesc.GetComponent<LayoutElement>().preferredHeight = 50;

            PanelBuilderUtil.AddSeparator(detailPanel.transform);

            var buyHeader = PanelBuilderUtil.CreateTMP("BuyHeader", detailPanel.transform, "-- BUY FROM FACTION --", 16, PanelBuilderUtil.OkColor);
            buyHeader.alignment = TextAlignmentOptions.Center;

            Transform sellContainer;
            PanelBuilderUtil.CreateScrollView("SellList", detailPanel.transform, out sellContainer);
            sellContainer.parent.GetComponent<LayoutElement>().preferredHeight = 120;
            sellContainer.parent.GetComponent<LayoutElement>().flexibleHeight = 0;

            var sellHeader = PanelBuilderUtil.CreateTMP("SellHeader", detailPanel.transform, "-- SELL TO FACTION --", 16, PanelBuilderUtil.GoldColor);
            sellHeader.alignment = TextAlignmentOptions.Center;

            Transform buyContainer;
            PanelBuilderUtil.CreateScrollView("BuyList", detailPanel.transform, out buyContainer);
            buyContainer.parent.GetComponent<LayoutElement>().preferredHeight = 120;
            buyContainer.parent.GetComponent<LayoutElement>().flexibleHeight = 0;

            detailPanel.SetActive(false);

            var info = root.AddComponent<FactionTradeUI>();
            root.SetActive(false);

            var so = new SerializedObject(info);
            PanelBuilderUtil.WireField(so, "_factionList", factionListContainer.gameObject);
            PanelBuilderUtil.WireField(so, "_detailPanel", detailPanel);
            PanelBuilderUtil.WireField(so, "_detailName", detailName);
            PanelBuilderUtil.WireField(so, "_detailRelation", detailRelation);
            PanelBuilderUtil.WireField(so, "_detailDesc", detailDesc);
            PanelBuilderUtil.WireField(so, "_sellContainer", sellContainer.gameObject);
            PanelBuilderUtil.WireField(so, "_buyContainer", buyContainer.gameObject);
            so.ApplyModifiedProperties();

            Selection.activeGameObject = root;
            Debug.Log("FactionTradePanel created.");
        }
    }
}
