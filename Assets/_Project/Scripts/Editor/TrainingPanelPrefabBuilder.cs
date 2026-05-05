using HollowGround.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HollowGround.Editor
{
    public static class TrainingPanelPrefabBuilder
    {
        [MenuItem("HollowGround/UI/Build Training Panel")]
        public static void Build()
        {
            var canvas = GameObject.Find("GameCanvas");
            if (canvas == null) { Debug.LogError("GameCanvas not found!"); return; }

            var existing = canvas.transform.Find("TrainingPanel");
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Replace?", "TrainingPanel exists. Replace?", "Yes", "No")) return;
                Object.DestroyImmediate(existing.gameObject);
            }

            var root = PanelBuilderUtil.CreateRoot("TrainingPanel", canvas.transform, 500);
            PanelBuilderUtil.SetupFullPanel(root);

            var header = PanelBuilderUtil.CreateHeader("Header", root.transform, "ARMY TRAINING", 24);
            header.alignment = TextAlignmentOptions.Center;
            header.GetComponent<LayoutElement>().preferredHeight = 40;

            PanelBuilderUtil.AddSeparator(root.transform);

            Transform rowsContainer;
            PanelBuilderUtil.CreateScrollView("TroopRows", root.transform, out rowsContainer);

            PanelBuilderUtil.AddSeparator(root.transform);

            var summaryText = PanelBuilderUtil.CreateTMP("ArmySummary", root.transform, "Army: 0 troops", 16, PanelBuilderUtil.OkColor);
            summaryText.alignment = TextAlignmentOptions.Center;
            summaryText.GetComponent<LayoutElement>().preferredHeight = 30;

            var statusText = PanelBuilderUtil.CreateTMP("Status", root.transform, "No active training", 14, PanelBuilderUtil.LabelColor);
            statusText.alignment = TextAlignmentOptions.Center;
            statusText.GetComponent<LayoutElement>().preferredHeight = 30;

            var info = root.GetComponent<TrainingPanelUI>() ?? root.AddComponent<TrainingPanelUI>();
            root.SetActive(false);

            var so = new SerializedObject(info);
            so.FindProperty("_rowsContainer").objectReferenceValue = rowsContainer.gameObject;
            so.FindProperty("_armySummaryText").objectReferenceValue = summaryText;
            so.FindProperty("_statusText").objectReferenceValue = statusText;
            so.ApplyModifiedProperties();

            Selection.activeGameObject = root;
            Debug.Log("TrainingPanel created.");
        }
    }
}
