using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using HollowGround.UI;

namespace HollowGround.Editor
{
    public static class TooltipPrefabBuilder
    {
        [MenuItem("HollowGround/Setup Tooltip")]
        public static void SetupTooltip()
        {
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[Tooltip] No Canvas found!");
                return;
            }

            Transform existing = canvas.transform.Find("TooltipUI");
            if (existing != null)
            {
                Debug.Log("[Tooltip] TooltipUI already exists under Canvas.");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            var root = new GameObject("TooltipUI", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            root.SetActive(true);

            var rootRt = root.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.pivot = new Vector2(0.5f, 0.5f);
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            var rootCg = root.AddComponent<CanvasGroup>();
            rootCg.blocksRaycasts = false;
            rootCg.interactable = false;

            var rootImg = root.AddComponent<Image>();
            rootImg.color = Color.clear;
            rootImg.raycastTarget = false;

            var tooltipUI = root.AddComponent<TooltipUI>();

            var container = new GameObject("Container", typeof(RectTransform));
            container.transform.SetParent(root.transform, false);

            var cRt = container.GetComponent<RectTransform>();
            cRt.pivot = new Vector2(0f, 1f);
            cRt.anchorMin = new Vector2(0f, 1f);
            cRt.anchorMax = new Vector2(0f, 1f);
            cRt.sizeDelta = new Vector2(300f, 100f);

            var bg = container.AddComponent<Image>();
            bg.color = UIColors.Default.PanelBg;
            bg.raycastTarget = false;

            var outline = container.AddComponent<Outline>();
            outline.effectColor = new Color(0.32f, 0.35f, 0.40f, 0.9f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var cCg = container.AddComponent<CanvasGroup>();
            cCg.blocksRaycasts = false;
            cCg.interactable = false;

            var le = container.AddComponent<LayoutElement>();
            le.minWidth = 180f;
            le.preferredWidth = 340f;

            var vlg = container.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(16, 16, 12, 12);
            vlg.spacing = 3f;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = container.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            container.SetActive(false);

            var so = new SerializedObject(tooltipUI);
            var containerProp = so.FindProperty("_container");
            if (containerProp != null)
                containerProp.objectReferenceValue = cRt;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(tooltipUI);

            EditorSceneManager.MarkAllScenesDirty();

            Selection.activeGameObject = root;
            Debug.Log("[Tooltip] TooltipUI created under GameCanvas. Select it to edit Container size in Inspector.");
        }
    }
}
