using System.Collections.Generic;
using HollowGround.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class SaveMenuUI : MonoBehaviour
    {
        private static readonly Color PanelBg = new(0.08f, 0.09f, 0.11f, 0.92f);
        private static readonly Color RowBg = new(0.14f, 0.15f, 0.17f, 1f);
        private static readonly Color ColorText = new(0.95f, 0.95f, 0.95f, 1f);
        private static readonly Color ColorMuted = new(0.65f, 0.65f, 0.7f, 1f);
        private static readonly Color ColorOk = new(0.35f, 0.8f, 0.4f, 1f);
        private static readonly Color ColorGold = new(1f, 0.85f, 0.3f, 1f);
        private static readonly Color ColorDanger = new(0.9f, 0.3f, 0.3f, 1f);

        private int _selectedSlotIndex = -1;
        private List<SaveData> _saves = new();
        private Transform _listContainer;
        private TMP_Text _infoText;
        private bool _built;

        private void OnEnable()
        {
            if (!_built) BuildUI();
            RefreshList();
        }

        private void BuildUI()
        {
            var root = GetComponent<RectTransform>();
            if (root == null) return;

            root.anchorMin = new Vector2(0f, 0f);
            root.anchorMax = new Vector2(1f, 1f);
            root.offsetMin = new Vector2(0f, 60f);
            root.offsetMax = new Vector2(0f, 0f);

            foreach (Transform child in root)
                Destroy(child.gameObject);

            var oldVlg = GetComponent<VerticalLayoutGroup>();
            if (oldVlg != null) DestroyImmediate(oldVlg);
            var oldImages = GetComponents<Image>();
            foreach (var img in oldImages) DestroyImmediate(img);
            var oldCg = GetComponent<CanvasGroup>();
            if (oldCg != null) DestroyImmediate(oldCg);

            var bg = gameObject.AddComponent<Image>();
            bg.color = PanelBg;
            bg.raycastTarget = true;

            var cg = gameObject.AddComponent<CanvasGroup>();
            cg.interactable = true;
            cg.blocksRaycasts = true;

            var vlg = gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 15, 15);
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var header = AddText(transform, "SAVE / LOAD", 26, ColorGold);
            header.alignment = TextAlignmentOptions.Center;
            var headerLE = header.gameObject.AddComponent<LayoutElement>();
            headerLE.preferredHeight = 40;

            var listObj = new GameObject("SaveList", typeof(RectTransform));
            listObj.transform.SetParent(root, false);
            var listLE = listObj.AddComponent<LayoutElement>();
            listLE.preferredHeight = 300;
            listLE.minHeight = 100;
            var listVLG = listObj.AddComponent<VerticalLayoutGroup>();
            listVLG.spacing = 4;
            listVLG.childControlWidth = true;
            listVLG.childControlHeight = false;
            listVLG.childForceExpandWidth = true;
            listVLG.childForceExpandHeight = false;
            _listContainer = listObj.transform;

            _infoText = AddText(transform, "Select a save file", 16, ColorMuted);
            _infoText.alignment = TextAlignmentOptions.Center;
            var infoLE = _infoText.gameObject.AddComponent<LayoutElement>();
            infoLE.preferredHeight = 28;

            var btnRow = new GameObject("BtnRow", typeof(RectTransform));
            btnRow.transform.SetParent(root, false);
            var btnRowLE = btnRow.AddComponent<LayoutElement>();
            btnRowLE.preferredHeight = 45;
            var btnRowHLG = btnRow.AddComponent<HorizontalLayoutGroup>();
            btnRowHLG.spacing = 10;
            btnRowHLG.childAlignment = TextAnchor.MiddleCenter;
            btnRowHLG.childControlWidth = true;
            btnRowHLG.childControlHeight = true;
            btnRowHLG.childForceExpandWidth = true;
            btnRowHLG.childForceExpandHeight = false;

            MakeButton(btnRow.transform, "NEW SAVE", ColorOk, NewSave);
            MakeButton(btnRow.transform, "LOAD", ColorGold, LoadSelected);
            MakeButton(btnRow.transform, "DELETE", ColorDanger, DeleteSelected);

            _built = true;
        }

        public void RefreshList()
        {
            if (!_built || _listContainer == null) return;
            _selectedSlotIndex = -1;

            for (int i = _listContainer.childCount - 1; i >= 0; i--)
                Destroy(_listContainer.GetChild(i).gameObject);

            if (SaveSystem.Instance == null)
            {
                _infoText.text = "SaveSystem not found!";
                return;
            }

            _saves = SaveSystem.Instance.GetAllSaves();

            if (_saves.Count == 0)
            {
                var empty = AddText(_listContainer, "No save files found.", 16, ColorMuted);
                empty.alignment = TextAlignmentOptions.Center;
                return;
            }

            for (int i = 0; i < _saves.Count; i++)
            {
                var save = _saves[i];
                var row = new GameObject($"Save_{i}", typeof(RectTransform));
                row.transform.SetParent(_listContainer, false);
                var le = row.AddComponent<LayoutElement>();
                le.preferredHeight = 50;
                var rbg = row.AddComponent<Image>();
                rbg.color = RowBg;
                var hlg = row.AddComponent<HorizontalLayoutGroup>();
                hlg.padding = new RectOffset(12, 12, 6, 6);
                hlg.spacing = 10;
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = true;
                hlg.childForceExpandHeight = false;

                var nameT = AddText(row.transform, save.SaveName, 16, ColorText);
                nameT.alignment = TextAlignmentOptions.MidlineLeft;
                var nle = nameT.gameObject.AddComponent<LayoutElement>();
                nle.preferredWidth = 220;

                int minutes = Mathf.FloorToInt(save.PlayTime / 60f);
                int hours = minutes / 60;
                minutes %= 60;
                var infoT = AddText(row.transform, $"{hours}h{minutes}m | B:{save.Buildings.Count} H:{save.Heroes.Count}", 14, ColorMuted);
                infoT.alignment = TextAlignmentOptions.MidlineRight;

                var btn = row.AddComponent<Button>();
                btn.targetGraphic = rbg;
                int idx = i;
                btn.onClick.AddListener(() =>
                {
                    _selectedSlotIndex = idx;
                    _infoText.text = $"Selected: {save.SaveName}";
                });
            }

            _infoText.text = "Select a save file";
        }

        public void NewSave()
        {
            if (SaveSystem.Instance == null) return;

            string fileName = $"Save_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
            SaveSystem.Instance.Save(fileName);
            _infoText.text = "Game saved!";
            RefreshList();
        }

        public void LoadSelected()
        {
            if (_selectedSlotIndex < 0 || _selectedSlotIndex >= _saves.Count) return;
            if (SaveSystem.Instance == null) return;

            var save = _saves[_selectedSlotIndex];
            bool loaded = SaveSystem.Instance.Load(save.SaveName) != null;
            _infoText.text = loaded ? "Game loaded!" : "Load failed!";
            if (loaded) gameObject.SetActive(false);
        }

        public void DeleteSelected()
        {
            if (_selectedSlotIndex < 0 || _selectedSlotIndex >= _saves.Count) return;
            if (SaveSystem.Instance == null) return;

            var save = _saves[_selectedSlotIndex];
            SaveSystem.Instance.DeleteSave(save.SaveName);
            _infoText.text = "Save deleted!";
            RefreshList();
        }

        private void MakeButton(Transform parent, string label, Color color, System.Action onClick)
        {
            var btnObj = new GameObject("Btn", typeof(RectTransform));
            btnObj.transform.SetParent(parent, false);
            var btnLE = btnObj.AddComponent<LayoutElement>();
            btnLE.minWidth = 120;
            btnLE.preferredWidth = 160;
            btnLE.minHeight = 40;
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = color;
            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            var btnLabel = AddText(btnObj.transform, label, 16, Color.black);
            btnLabel.alignment = TextAlignmentOptions.Center;
            StretchFull(btnLabel.rectTransform);
            btn.onClick.AddListener(() => onClick());
        }

        private static TMP_Text AddText(Transform parent, string text, float size, Color color)
        {
            var go = new GameObject("T", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
