using HollowGround.Resources;
using HollowGround.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.Editor
{
    public static class ResourceBarRebuilder
    {
        private static readonly string[] Names = { "Wood", "Metal", "Food", "Water", "TechPart", "Energy" };
        private static readonly ResourceType[] Types =
        {
            ResourceType.Wood, ResourceType.Metal, ResourceType.Food,
            ResourceType.Water, ResourceType.TechPart, ResourceType.Energy
        };
        private static readonly Color[] Colors =
        {
            new(0.6f, 0.35f, 0.1f),
            new(0.7f, 0.7f, 0.75f),
            new(0.2f, 0.75f, 0.2f),
            new(0.2f, 0.5f, 0.9f),
            new(0.85f, 0.65f, 0.1f),
            new(0.95f, 0.9f, 0.15f)
        };

        [MenuItem("HollowGround/Rebuild Resource Bar")]
        public static void Rebuild()
        {
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) { Debug.LogError("No Canvas!"); return; }

            var oldBar = Object.FindAnyObjectByType<ResourceBarUI>();
            if (oldBar != null) Object.DestroyImmediate(oldBar.gameObject);

            var allToast = Object.FindObjectsByType<ToastUI>(FindObjectsInactive.Include);
            foreach (var t in allToast) Object.DestroyImmediate(t.gameObject);

            BuildResourceBar(canvas.transform);
            BuildToastUI(canvas.transform);

            Debug.Log("[Rebuild] Done! Press Play.");
        }

        private static void BuildResourceBar(Transform parent)
        {
            var bar = new GameObject("ResourceBar", typeof(RectTransform));
            bar.transform.SetParent(parent, false);
            bar.transform.SetAsFirstSibling();

            var bg = bar.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

            var rect = bar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, -25);
            rect.sizeDelta = new Vector2(0, 50);

            var hlg = bar.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(15, 15, 8, 8);
            hlg.spacing = 14;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            var barUI = bar.AddComponent<ResourceBarUI>();
            var slots = new System.Collections.Generic.List<ResourceBarUI.ResourceSlot>();

            for (int i = 0; i < Names.Length; i++)
            {
                var slot = new GameObject(Names[i], typeof(RectTransform));
                slot.transform.SetParent(bar.transform, false);

                var le = slot.AddComponent<LayoutElement>();
                le.minWidth = 100;
                le.minHeight = 32;
                le.preferredWidth = 160;
                le.preferredHeight = 32;

                var icon = new GameObject("Icon", typeof(RectTransform));
                icon.transform.SetParent(slot.transform, false);
                var iconImg = icon.AddComponent<Image>();
                iconImg.color = Colors[i];
                var ir = icon.GetComponent<RectTransform>();
                ir.anchorMin = new Vector2(0, 0.5f);
                ir.anchorMax = new Vector2(0, 0.5f);
                ir.pivot = new Vector2(0, 0.5f);
                ir.sizeDelta = new Vector2(18, 18);
                ir.anchoredPosition = Vector2.zero;

                var text = new GameObject("Text", typeof(RectTransform));
                text.transform.SetParent(slot.transform, false);
                var tmp = text.AddComponent<TextMeshProUGUI>();
                tmp.fontSize = 14;
                tmp.alignment = TextAlignmentOptions.MidlineLeft;
                tmp.color = Color.white;
                tmp.text = $"{Names[i]}: 0/500";
                var tr = text.GetComponent<RectTransform>();
                tr.anchorMin = Vector2.zero;
                tr.anchorMax = Vector2.one;
                tr.offsetMin = new Vector2(22, 0);
                tr.offsetMax = Vector2.zero;

                slots.Add(new ResourceBarUI.ResourceSlot
                {
                    Type       = Types[i],
                    AmountText = tmp,
                });
            }

            var so = new SerializedObject(barUI);
            var sp = so.FindProperty("_slots");
            sp.ClearArray();
            for (int i = 0; i < slots.Count; i++)
            {
                sp.InsertArrayElementAtIndex(i);
                sp.GetArrayElementAtIndex(i).FindPropertyRelative("Type").enumValueIndex = (int)slots[i].Type;
                sp.GetArrayElementAtIndex(i).FindPropertyRelative("AmountText").objectReferenceValue = slots[i].AmountText;
            }
            so.ApplyModifiedProperties();
        }

        private static void BuildToastUI(Transform parent)
        {
            var go = new GameObject("ToastUI", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.SetAsLastSibling();

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            go.AddComponent<ToastUI>();
        }
    }
}
