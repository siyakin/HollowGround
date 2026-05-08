using System.Collections.Generic;
using HollowGround.Buildings;
using HollowGround.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.Editor
{
    public static class BuildMenuFixer
    {
        private static readonly BuildingType[] Order =
        {
            BuildingType.CommandCenter,
            BuildingType.Farm,
            BuildingType.Mine,
            BuildingType.WoodFactory,
            BuildingType.WaterWell,
            BuildingType.Generator,
            BuildingType.Barracks,
            BuildingType.Storage,
            BuildingType.Shelter,
            BuildingType.Garden
        };

        private static readonly string[] ButtonNames =
        {
            "btnCommandCenter",
            "btnFarm",
            "btnMine",
            "btnWoodFactory",
            "btnWaterWell",
            "btnGenerator",
            "btnBarracks",
            "btnStorage",
            "btnShelter",
            "btnGarden"
        };

        [MenuItem("HollowGround/UI/Fix Build Menu")]
        public static void Fix()
        {
            if (Application.isPlaying)
            {
                Debug.LogError("[FixBuildMenu] Exit Play mode before running this tool!");
                return;
            }

            var menu = Object.FindAnyObjectByType<BuildMenuUI>(FindObjectsInactive.Include);
            if (menu == null)
            {
                Debug.LogError("[FixBuildMenu] BuildMenuUI not found!");
                return;
            }

            var allAssets = AssetDatabase.FindAssets("t:BuildingData", new[] { "Assets/_Project/ScriptableObjects/Buildings" });
            var lookup = new Dictionary<BuildingType, BuildingData>();

            foreach (var guid in allAssets)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<BuildingData>(path);
                if (data != null && !lookup.ContainsKey(data.Type))
                    lookup[data.Type] = data;
            }

            var allButtons = menu.GetComponentsInChildren<Button>(true);
            var buttonLookup = new Dictionary<string, Button>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var b in allButtons)
            {
                var key = b.gameObject.name.Trim();
                if (!buttonLookup.ContainsKey(key))
                    buttonLookup[key] = b;
            }

            var so = new SerializedObject(menu);
            var cardsProp = so.FindProperty("_cards");
            cardsProp.ClearArray();

            int connected = 0;
            for (int i = 0; i < Order.Length; i++)
            {
                if (!lookup.TryGetValue(Order[i], out var data)) continue;
                if (i >= ButtonNames.Length) break;

                Button btn = null;
                buttonLookup.TryGetValue(ButtonNames[i], out btn);

                TMP_Text nameText = null;
                ResourceCostDisplay costDisplay = null;

                if (btn != null)
                {
                    costDisplay = SetupButton(btn, out nameText);

                    int idx = connected;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => menu.SelectBuilding(idx));

                    var tb = btn.GetComponent<ThemedButton>();
                    if (tb == null)
                        tb = btn.gameObject.AddComponent<ThemedButton>();
                    tb.styleType = UIStyleType.BuildingCardButton;
                }

                cardsProp.InsertArrayElementAtIndex(connected);
                var element = cardsProp.GetArrayElementAtIndex(connected);
                element.FindPropertyRelative("Data").objectReferenceValue = data;
                element.FindPropertyRelative("LockedOverlay").objectReferenceValue = null;
                element.FindPropertyRelative("Button").objectReferenceValue = btn;
                element.FindPropertyRelative("NameText").objectReferenceValue = nameText;
                element.FindPropertyRelative("CostDisplay").objectReferenceValue = costDisplay;

                connected++;
                string cdStatus = costDisplay != null ? "CD:OK" : "CD:NULL!";
                string nameStatus = nameText != null ? $"Name:'{nameText.text}'" : "Name:NULL!";
                Debug.Log($"  [{i}] {data.Type}: {data.DisplayName} -> {btn?.name ?? "NO BUTTON"} | {nameStatus} | {cdStatus}");
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(menu);
            EditorSceneManager.MarkSceneDirty(menu.gameObject.scene);
            Debug.Log($"[FixBuildMenu] {connected} buildings assigned — SAVE the scene now (Ctrl+S)");
        }

        static ResourceCostDisplay SetupButton(Button btn, out TMP_Text nameText)
        {
            nameText = null;

            var staleCDs = btn.GetComponentsInChildren<ResourceCostDisplay>(true);
            foreach (var cd in staleCDs)
                Object.DestroyImmediate(cd.gameObject);

            var tmps = new List<TMP_Text>();
            foreach (var tmp in btn.GetComponentsInChildren<TMP_Text>(true))
            {
                if (tmp.GetComponentInParent<ResourceCostDisplay>() != null) continue;
                tmps.Add(tmp);
            }

            nameText = tmps.Count > 0 ? tmps[0] : null;
            if (PanelBuilderUtil.ThemeFont != null && nameText != null)
                nameText.font = PanelBuilderUtil.ThemeFont;

            for (int i = 1; i < tmps.Count; i++)
                Object.DestroyImmediate(tmps[i].gameObject);

            var costObj = ResourceCostDisplayBuilder.Create(btn.transform, "CostDisplay");
            var costLE = costObj.GetComponent<LayoutElement>();
            if (costLE == null) costLE = costObj.AddComponent<LayoutElement>();
            costLE.flexibleWidth = 1;
            costLE.minWidth = 80;

            var colors = btn.colors;
            colors.normalColor = new Color(0.15f, 0.16f, 0.14f, 0.95f);
            colors.highlightedColor = new Color(0.2f, 0.28f, 0.18f, 0.95f);
            colors.pressedColor = new Color(0.12f, 0.14f, 0.1f, 0.95f);
            colors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.7f);
            colors.colorMultiplier = 1f;
            btn.colors = colors;

            return costObj.GetComponent<ResourceCostDisplay>();
        }
    }
}
