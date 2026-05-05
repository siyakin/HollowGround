using HollowGround.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HollowGround.Editor
{
    public static class QuestLogPrefabBuilder
    {
        [MenuItem("HollowGround/UI/Build QuestLog Panel")]
        public static void Build()
        {
            var canvas = GameObject.Find("GameCanvas");
            if (canvas == null) { Debug.LogError("GameCanvas not found!"); return; }

            var existing = canvas.transform.Find("QuestLogPanel");
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Replace?", "QuestLogPanel exists. Replace?", "Yes", "No")) return;
                Object.DestroyImmediate(existing.gameObject);
            }

            var root = new GameObject("QuestLogPanel", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            var rt = root.GetComponent<RectTransform>();
            PanelBuilderUtil.StretchFull(rt);
            rt.offsetMin = new Vector2(0, 60);
            rt.offsetMax = Vector2.zero;
            root.AddComponent<Image>().color = PanelBuilderUtil.PanelBg;
            var rootVLG = root.AddComponent<VerticalLayoutGroup>();
            rootVLG.padding = new RectOffset(10, 10, 10, 10);
            rootVLG.spacing = 4;
            rootVLG.childControlWidth = true;
            rootVLG.childControlHeight = false;
            rootVLG.childForceExpandWidth = true;
            rootVLG.childForceExpandHeight = false;

            // Header with tab buttons
            var headerRow = PanelBuilderUtil.CreateRow("Header", root.transform, 10);
            headerRow.GetComponent<LayoutElement>().preferredHeight = 40;
            headerRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
            headerRow.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = true;

            var prevBtn = PanelBuilderUtil.CreateButton("PrevBtn", headerRow.transform, "< Prev", PanelBuilderUtil.AccentColor, 32);
            prevBtn.GetComponent<LayoutElement>().preferredWidth = 100;

            var headerText = PanelBuilderUtil.CreateHeader("Title", headerRow.transform, "QUEST LOG", 22);
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.GetComponent<LayoutElement>().preferredWidth = 200;

            var nextBtn = PanelBuilderUtil.CreateButton("NextBtn", headerRow.transform, "Next >", PanelBuilderUtil.AccentColor, 32);
            nextBtn.GetComponent<LayoutElement>().preferredWidth = 100;

            PanelBuilderUtil.AddSeparator(root.transform);

            // Quest list
            Transform listContainer;
            PanelBuilderUtil.CreateScrollView("QuestList", root.transform, out listContainer);
            listContainer.parent.GetComponent<LayoutElement>().preferredHeight = 200;
            listContainer.parent.GetComponent<LayoutElement>().flexibleHeight = 1;

            PanelBuilderUtil.AddSeparator(root.transform);

            // Detail panel
            var detailPanel = PanelBuilderUtil.CreateSection("DetailPanel", root.transform, new RectOffset(15, 15, 10, 10));
            detailPanel.GetComponent<LayoutElement>().preferredHeight = 200;

            var detailName = PanelBuilderUtil.CreateTMP("DetailName", detailPanel.transform, "", 18, PanelBuilderUtil.TextColor);
            var detailDesc = PanelBuilderUtil.CreateTMP("DetailDesc", detailPanel.transform, "", 14, PanelBuilderUtil.LabelColor);
            var detailObjectives = PanelBuilderUtil.CreateTMP("DetailObjectives", detailPanel.transform, "", 14, PanelBuilderUtil.TextColor);
            var detailRewards = PanelBuilderUtil.CreateTMP("DetailRewards", detailPanel.transform, "", 14, PanelBuilderUtil.GoldColor);

            var btnRow = PanelBuilderUtil.CreateRow("BtnRow", detailPanel.transform, 10);
            btnRow.GetComponent<LayoutElement>().preferredHeight = 40;
            btnRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
            btnRow.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = true;

            var acceptBtn = PanelBuilderUtil.CreateButton("AcceptBtn", btnRow.transform, "ACCEPT QUEST", PanelBuilderUtil.OkColor, 36);
            acceptBtn.GetComponent<LayoutElement>().preferredWidth = 180;

            var turnInBtn = PanelBuilderUtil.CreateButton("TurnInBtn", btnRow.transform, "TURN IN", PanelBuilderUtil.GoldColor, 36);
            turnInBtn.GetComponent<LayoutElement>().preferredWidth = 180;

            detailPanel.SetActive(false);

            var info = root.AddComponent<QuestLogUI>();
            root.SetActive(false);

            var so = new SerializedObject(info);
            PanelBuilderUtil.WireField(so, "_headerText", headerText);
            PanelBuilderUtil.WireField(so, "_prevBtn", prevBtn);
            PanelBuilderUtil.WireField(so, "_nextBtn", nextBtn);
            PanelBuilderUtil.WireField(so, "_listContainer", listContainer.gameObject);
            PanelBuilderUtil.WireField(so, "_detailPanel", detailPanel);
            PanelBuilderUtil.WireField(so, "_detailName", detailName);
            PanelBuilderUtil.WireField(so, "_detailDesc", detailDesc);
            PanelBuilderUtil.WireField(so, "_detailObjectives", detailObjectives);
            PanelBuilderUtil.WireField(so, "_detailRewards", detailRewards);
            PanelBuilderUtil.WireField(so, "_acceptBtn", acceptBtn);
            PanelBuilderUtil.WireField(so, "_turnInBtn", turnInBtn);
            so.ApplyModifiedProperties();

            Selection.activeGameObject = root;
            Debug.Log("QuestLogPanel created.");
        }
    }
}
