using HollowGround.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HollowGround.Editor
{
    public static class HeroPanelPrefabBuilder
    {
        [MenuItem("HollowGround/UI/Build Hero Panel")]
        public static void Build()
        {
            var canvas = GameObject.Find("GameCanvas");
            if (canvas == null) { Debug.LogError("GameCanvas not found!"); return; }

            var existing = canvas.transform.Find("HeroPanel");
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Replace?", "HeroPanel exists. Replace?", "Yes", "No")) return;
                Object.DestroyImmediate(existing.gameObject);
            }

            var root = PanelBuilderUtil.CreateRoot("HeroPanel", canvas.transform, 560);
            PanelBuilderUtil.SetupFullPanel(root);

            var header = PanelBuilderUtil.CreateHeader("Header", root.transform, "HEROES", 24);
            header.alignment = TextAlignmentOptions.Center;
            header.GetComponent<LayoutElement>().preferredHeight = 40;

            PanelBuilderUtil.AddSeparator(root.transform);

            Transform listContainer;
            PanelBuilderUtil.CreateScrollView("HeroList", root.transform, out listContainer);

            PanelBuilderUtil.AddSeparator(root.transform);

            var summonRow = PanelBuilderUtil.CreateRow("SummonRow", root.transform, 15, new RectOffset(15, 15, 5, 5));
            var summonBg = summonRow.AddComponent<Image>();
            summonBg.color = PanelBuilderUtil.SectionBg;
            summonRow.GetComponent<LayoutElement>().preferredHeight = 45;
            summonRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var summonCostText = PanelBuilderUtil.CreateTMP("SummonCost", summonRow.transform, "Summon: 100 TechPart", 15, PanelBuilderUtil.LabelColor);
            summonCostText.GetComponent<LayoutElement>().preferredWidth = 180;

            var summonBtn = PanelBuilderUtil.CreateButton("SummonBtn", summonRow.transform, "SUMMON", PanelBuilderUtil.OkColor, 35);

            var info = root.AddComponent<HeroPanelUI>();
            root.SetActive(false);

            var so = new SerializedObject(info);
            PanelBuilderUtil.WireField(so, "_headerText", header);
            PanelBuilderUtil.WireField(so, "_summonCostText", summonCostText);
            PanelBuilderUtil.WireField(so, "_listContainer", listContainer.gameObject);
            PanelBuilderUtil.WireField(so, "_summonBtn", summonBtn);
            so.ApplyModifiedProperties();

            Selection.activeGameObject = root;
            Debug.Log("HeroPanel created.");
        }
    }
}
