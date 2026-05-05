using HollowGround.Resources;
using HollowGround.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HollowGround.Editor
{
    public static class ResourceCostDisplayBuilder
    {
        static readonly Color[] ResourceColors = new[]
        {
            new Color(0.76f, 0.55f, 0.2f),
            new Color(0.7f, 0.75f, 0.82f),
            new Color(0.35f, 0.8f, 0.4f),
            new Color(0.3f, 0.6f, 0.95f),
            new Color(0.7f, 0.3f, 0.9f),
            new Color(1f, 0.85f, 0.3f)
        };

        static readonly string[] ResourceNames = new[]
        {
            "Wood", "Metal", "Food", "Water", "TechPart", "Energy"
        };

        public static GameObject Create(Transform parent, string objectName = "CostDisplay")
        {
            var font = PanelBuilderUtil.ThemeFont;
            var root = new GameObject(objectName, typeof(RectTransform));
            root.transform.SetParent(parent, false);

            var rootLE = root.AddComponent<LayoutElement>();
            rootLE.minHeight = 14;
            rootLE.preferredHeight = -1;

            var hlg = root.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleRight;

            var display = root.AddComponent<ResourceCostDisplay>();

            var slots = new System.Collections.Generic.List<ResourceCostDisplay.CostSlot>();

            for (int i = 0; i < 6; i++)
            {
                var slotRoot = new GameObject(ResourceNames[i], typeof(RectTransform));
                slotRoot.transform.SetParent(root.transform, false);

                var slotHlg = slotRoot.AddComponent<HorizontalLayoutGroup>();
                slotHlg.spacing = 2;
                slotHlg.childControlWidth = true;
                slotHlg.childControlHeight = true;
                slotHlg.childForceExpandWidth = false;
                slotHlg.childForceExpandHeight = false;
                slotHlg.childAlignment = TextAnchor.MiddleLeft;

                var dot = new GameObject("Dot", typeof(RectTransform));
                dot.transform.SetParent(slotRoot.transform, false);
                var dotLE = dot.AddComponent<LayoutElement>();
                dotLE.preferredWidth = 18;
                dotLE.preferredHeight = 18;
                var dotImg = dot.AddComponent<Image>();
                dotImg.color = ResourceColors[i];

                var val = new GameObject("Value", typeof(RectTransform));
                val.transform.SetParent(slotRoot.transform, false);
                var tmp = val.AddComponent<TextMeshProUGUI>();
                tmp.text = "0";
                tmp.fontSize = 18;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.MidlineLeft;
                if (font != null) tmp.font = font;
                val.AddComponent<LayoutElement>().minWidth = 24;

                slotRoot.SetActive(false);

                slots.Add(new ResourceCostDisplay.CostSlot
                {
                    Type = (ResourceType)i,
                    Root = slotRoot,
                    Dot = dotImg,
                    ValueText = tmp
                });
            }

            var so = new SerializedObject(display);
            var slotsProp = so.FindProperty("_slots");
            slotsProp.arraySize = 6;
            for (int i = 0; i < 6; i++)
            {
                var entry = slotsProp.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("Type").enumValueIndex = i;
                entry.FindPropertyRelative("Root").objectReferenceValue = slots[i].Root;
                entry.FindPropertyRelative("Dot").objectReferenceValue = slots[i].Dot;
                entry.FindPropertyRelative("ValueText").objectReferenceValue = slots[i].ValueText;
            }
            so.ApplyModifiedProperties();

            return root;
        }
    }
}
