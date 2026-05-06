using HollowGround.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HollowGround.Editor
{
    public static class TechTreePrefabBuilder
    {
        [MenuItem("HollowGround/UI/Build TechTree Panel")]
        public static void Build()
        {
            var canvas = GameObject.Find("GameCanvas");
            if (canvas == null) { Debug.LogError("GameCanvas not found!"); return; }

            var existing = canvas.transform.Find("TechTreePanel");
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Replace?", "TechTreePanel exists. Replace?", "Yes", "No")) return;
                Object.DestroyImmediate(existing.gameObject);
            }

            var root = new GameObject("TechTreePanel", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            var rt = root.GetComponent<RectTransform>();
            PanelBuilderUtil.StretchFull(rt);
            rt.offsetMin = new Vector2(0, 60);
            rt.offsetMax = Vector2.zero;
            root.AddComponent<Image>().color = PanelBuilderUtil.PanelBg;
            var cg = root.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = true;

            // Background
            var bg = new GameObject("Background", typeof(RectTransform));
            bg.transform.SetParent(root.transform, false);
            var bgRt = bg.GetComponent<RectTransform>();
            PanelBuilderUtil.StretchFull(bgRt);
            bg.AddComponent<Image>().color = PanelBuilderUtil.PanelBg;

            // Header
            var header = new GameObject("Header", typeof(RectTransform));
            header.transform.SetParent(root.transform, false);
            var headerRt = header.GetComponent<RectTransform>();
            UIPrimitiveFactory.SetAnchors(headerRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            headerRt.anchoredPosition = new Vector2(0, -30);
            headerRt.sizeDelta = new Vector2(-40, 50);
            var titleTmp = header.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "TECHNOLOGY TREE";
            titleTmp.fontSize = 28;
            titleTmp.color = PanelBuilderUtil.TextColor;
            titleTmp.alignment = TextAlignmentOptions.Center;

            // Columns container (left side)
            var columns = new GameObject("Columns", typeof(RectTransform));
            columns.transform.SetParent(root.transform, false);
            var colRt = columns.GetComponent<RectTransform>();
            UIPrimitiveFactory.SetAnchors(colRt, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
            colRt.offsetMin = new Vector2(40, 30);
            colRt.offsetMax = new Vector2(-420, -80);
            var colHLG = columns.AddComponent<HorizontalLayoutGroup>();
            colHLG.spacing = 10;
            colHLG.childAlignment = TextAnchor.UpperLeft;
            colHLG.childControlWidth = true;
            colHLG.childControlHeight = true;
            colHLG.childForceExpandWidth = true;
            colHLG.childForceExpandHeight = true;

            // Detail panel (right side)
            var detailPanel = new GameObject("DetailPanel", typeof(RectTransform));
            detailPanel.transform.SetParent(root.transform, false);
            var dpRt = detailPanel.GetComponent<RectTransform>();
            UIPrimitiveFactory.SetAnchors(dpRt, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f));
            dpRt.offsetMin = new Vector2(-400, 30);
            dpRt.offsetMax = new Vector2(-20, -80);
            var dpImg = detailPanel.AddComponent<Image>();
            dpImg.color = UIColors.PanelInner;

            // Detail panel children (anchored positioning like original)
            var detailName = CreateDetailText(detailPanel.transform, "DetailName", "Select a technology", 22, PanelBuilderUtil.TextColor, -15, 30);
            var detailCategory = CreateDetailText(detailPanel.transform, "DetailCategory", "", 14, PanelBuilderUtil.LabelColor, -48, 20);
            var detailDesc = CreateDetailText(detailPanel.transform, "DetailDesc", "", 15, PanelBuilderUtil.TextColor, -78, 60);
            var detailCost = CreateDetailText(detailPanel.transform, "DetailCost", "", 15, PanelBuilderUtil.LabelColor, -150, 80);

            // Research button
            var resBtnObj = new GameObject("ResearchBtn", typeof(RectTransform));
            resBtnObj.transform.SetParent(detailPanel.transform, false);
            var resBtnRt = resBtnObj.GetComponent<RectTransform>();
            UIPrimitiveFactory.SetAnchors(resBtnRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            resBtnRt.anchoredPosition = new Vector2(0, -250);
            resBtnRt.sizeDelta = new Vector2(-24, 50);
            var resBtnBg = resBtnObj.AddComponent<Image>();
            resBtnBg.color = PanelBuilderUtil.OkColor;
            var resBtn = resBtnObj.AddComponent<Button>();
            resBtn.targetGraphic = resBtnBg;
            var resBtnLabel = new GameObject("Label", typeof(RectTransform));
            resBtnLabel.transform.SetParent(resBtnObj.transform, false);
            var resBtnTmp = resBtnLabel.AddComponent<TextMeshProUGUI>();
            resBtnTmp.text = "START RESEARCH";
            resBtnTmp.fontSize = 14;
            resBtnTmp.color = Color.white;
            resBtnTmp.alignment = TextAlignmentOptions.Center;
            var resBtnLabelRt = resBtnLabel.GetComponent<RectTransform>();
            PanelBuilderUtil.StretchFull(resBtnLabelRt);

            var detailBonuses = CreateDetailText(detailPanel.transform, "DetailBonuses", "", 15, PanelBuilderUtil.TextColor, -310, 100);
            var detailPrereqs = CreateDetailText(detailPanel.transform, "DetailPrereqs", "", 14, UIColors.TextDim, -420, 60);

            var info = root.AddComponent<TechTreeUI>();
            resBtn.onClick.AddListener(info.StartResearchFromDetail);
            root.SetActive(false);

            var so = new SerializedObject(info);
            PanelBuilderUtil.WireField(so, "_columnsContainer", colRt);
            PanelBuilderUtil.WireField(so, "_detailPanel", dpRt);
            PanelBuilderUtil.WireField(so, "_detailName", detailName);
            PanelBuilderUtil.WireField(so, "_detailCategory", detailCategory);
            PanelBuilderUtil.WireField(so, "_detailDesc", detailDesc);
            PanelBuilderUtil.WireField(so, "_detailCost", detailCost);
            PanelBuilderUtil.WireField(so, "_detailBonuses", detailBonuses);
            PanelBuilderUtil.WireField(so, "_detailPrereqs", detailPrereqs);
            PanelBuilderUtil.WireField(so, "_detailResearchBtn", resBtn);
            PanelBuilderUtil.WireField(so, "_detailResearchBtnText", resBtnTmp);
            so.ApplyModifiedProperties();

            Selection.activeGameObject = root;
            Debug.Log("TechTreePanel created.");
        }

        private static TMP_Text CreateDetailText(Transform parent, string name, string text, int size, Color color, float yPos, float height)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var goRt = go.GetComponent<RectTransform>();
            UIPrimitiveFactory.SetAnchors(goRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            goRt.anchoredPosition = new Vector2(0, yPos);
            goRt.sizeDelta = new Vector2(-24, height);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.richText = true;
            return tmp;
        }
    }
}
