using System.Collections.Generic;
using HollowGround.Buildings;
using HollowGround.UI;
using UnityEditor;
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
            BuildingType.Shelter
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
            "btnShelter"
        };

        public static void Fix()
        {
            var menu = UnityEngine.Object.FindAnyObjectByType<BuildMenuUI>(FindObjectsInactive.Include);
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
            var buttonLookup = new Dictionary<string, Button>();
            foreach (var b in allButtons)
                buttonLookup[b.gameObject.name] = b;

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

                cardsProp.InsertArrayElementAtIndex(connected);
                var element = cardsProp.GetArrayElementAtIndex(connected);
                element.FindPropertyRelative("Data").objectReferenceValue = data;
                element.FindPropertyRelative("LockedOverlay").objectReferenceValue = null;
                element.FindPropertyRelative("Button").objectReferenceValue = btn;

                if (btn != null)
                {
                    int idx = connected;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => menu.SelectBuilding(idx));

                    var tb = btn.GetComponent<ThemedButton>();
                    if (tb == null)
                        tb = btn.gameObject.AddComponent<ThemedButton>();
                    tb.styleType = UIStyleType.BuildingCardButton;
                }

                connected++;
                string btnStatus = btn != null ? $"-> {btn.name}" : "-> NO BUTTON";
                Debug.Log($"  [{i}] {data.Type}: {data.DisplayName} {btnStatus}");
            }

            so.ApplyModifiedProperties();
            Debug.Log($"[FixBuildMenu] {connected} buildings assigned");
        }
    }
}
