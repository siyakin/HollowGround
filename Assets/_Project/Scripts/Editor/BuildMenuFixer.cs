using HollowGround.Buildings;
using HollowGround.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.Editor
{
    public static class BuildMenuFixer
    {
        [MenuItem("HollowGround/Fix BuildMenu")]
        public static void Fix()
        {
            var menu = Object.FindAnyObjectByType<BuildMenuUI>();
            if (menu == null)
            {
                Debug.LogError("[FixBuildMenu] BuildMenuUI not found in scene!");
                return;
            }

            var allBuildings = AssetDatabase.FindAssets("t:BuildingData", new[] { "Assets/_Project/ScriptableObjects/Buildings" });

            BuildingData commandCenter = null;
            BuildingData farm = null;
            BuildingData mine = null;

            foreach (var guid in allBuildings)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<BuildingData>(path);
                if (data == null) continue;

                if (data.Type == BuildingType.CommandCenter) commandCenter = data;
                else if (data.Type == BuildingType.Farm) farm = data;
                else if (data.Type == BuildingType.Mine) mine = data;
            }

            var so = new SerializedObject(menu);
            var cardsProp = so.FindProperty("_cards");
            cardsProp.ClearArray();

            AddCard(cardsProp, 0, commandCenter);
            AddCard(cardsProp, 1, farm);
            AddCard(cardsProp, 2, mine);

            so.ApplyModifiedProperties();

            WireButtonOnClick(menu, 0);
            WireButtonOnClick(menu, 1);
            WireButtonOnClick(menu, 2);

            Debug.Log($"[FixBuildMenu] Cards: CC={commandCenter?.DisplayName}, Farm={farm?.DisplayName}, Mine={mine?.DisplayName}");
        }

        private static void AddCard(SerializedProperty cardsProp, int index, BuildingData data)
        {
            cardsProp.InsertArrayElementAtIndex(index);
            var element = cardsProp.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("Data").objectReferenceValue = data;
            element.FindPropertyRelative("LockedOverlay").objectReferenceValue = null;
        }

        private static void WireButtonOnClick(BuildMenuUI menu, int cardIndex)
        {
            var so = new SerializedObject(menu);
            var cardsProp = so.FindProperty("_cards");
            if (cardIndex >= cardsProp.arraySize) return;

            var buttonProp = cardsProp.GetArrayElementAtIndex(cardIndex).FindPropertyRelative("Button");
            var button = buttonProp.objectReferenceValue as Button;
            if (button == null) return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => menu.SelectBuilding(cardIndex));

            Debug.Log($"[FixBuildMenu] Button {cardIndex} wired: {button.name}");
        }
    }
}
