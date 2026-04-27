using System.Collections.Generic;
using HollowGround.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class SaveMenuUI : MonoBehaviour
    {
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

            UIPrimitiveFactory.SetupPanelBackground(gameObject, UIColors.Default);
            UIPrimitiveFactory.StretchFull(root, new Vector2(0f, 60f), Vector2.zero);

            foreach (Transform child in root)
                Destroy(child.gameObject);

            UIPrimitiveFactory.AddStandardVLG(gameObject);

            var header = UIPrimitiveFactory.AddThemedText(transform, "SAVE / LOAD", 26, UIColors.Default.Gold);
            header.alignment = TextAlignmentOptions.Center;
            UIPrimitiveFactory.AddLayoutElement(header.gameObject, preferredHeight: 40);

            var listObj = UIPrimitiveFactory.CreateUIObject("SaveList", root);
            UIPrimitiveFactory.AddLayoutElement(listObj.gameObject, preferredHeight: 300, minHeight: 100);
            var listVLG = listObj.gameObject.AddComponent<VerticalLayoutGroup>();
            listVLG.spacing = 4;
            listVLG.childControlWidth = true;
            listVLG.childControlHeight = false;
            listVLG.childForceExpandWidth = true;
            listVLG.childForceExpandHeight = false;
            _listContainer = listObj.transform;

            _infoText = UIPrimitiveFactory.AddThemedText(transform, "Select a save file", 16, UIColors.Default.Muted);
            _infoText.alignment = TextAlignmentOptions.Center;
            UIPrimitiveFactory.AddLayoutElement(_infoText.gameObject, preferredHeight: 28);

            var btnRow = UIPrimitiveFactory.CreateUIObject("BtnRow", root);
            UIPrimitiveFactory.AddLayoutElement(btnRow.gameObject, preferredHeight: 45);
            var btnRowHLG = UIPrimitiveFactory.AddRowHLG(btnRow.gameObject);
            btnRowHLG.childAlignment = TextAnchor.MiddleCenter;

            MakeButton(btnRow.transform, "NEW SAVE", UIColors.Default.Ok, NewSave);
            MakeButton(btnRow.transform, "LOAD", UIColors.Default.Gold, LoadSelected);
            MakeButton(btnRow.transform, "DELETE", UIColors.Default.Danger, DeleteSelected);
            MakeButton(btnRow.transform, "BACK", UIColors.Default.Muted, CloseSelf);

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
                var empty = UIPrimitiveFactory.AddThemedText(_listContainer, "No save files found.", 16, UIColors.Default.Muted);
                empty.alignment = TextAlignmentOptions.Center;
                return;
            }

            for (int i = 0; i < _saves.Count; i++)
            {
                var save = _saves[i];
                var row = UIPrimitiveFactory.CreateUIObject($"Save_{i}", _listContainer);
                UIPrimitiveFactory.AddLayoutElement(row.gameObject, preferredHeight: 50);
                var rbg = row.gameObject.AddComponent<Image>();
                rbg.color = UIColors.Default.RowBg;
                UIPrimitiveFactory.AddRowHLG(row.gameObject);

                var nameT = UIPrimitiveFactory.AddThemedText(row, save.SaveName, 16, UIColors.Default.Text);
                nameT.alignment = TextAlignmentOptions.MidlineLeft;
                UIPrimitiveFactory.AddLayoutElement(nameT.gameObject, preferredWidth: 220);

                int minutes = Mathf.FloorToInt(save.PlayTime / 60f);
                int hours = minutes / 60;
                minutes %= 60;
                var infoT = UIPrimitiveFactory.AddThemedText(row, $"{hours}h{minutes}m | B:{save.Buildings.Count} H:{save.Heroes.Count}", 14, UIColors.Default.Muted);
                infoT.alignment = TextAlignmentOptions.MidlineRight;

                var btn = row.gameObject.AddComponent<Button>();
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

        public void CloseSelf()
        {
            gameObject.SetActive(false);
        }

        private void MakeButton(Transform parent, string label, Color color, System.Action onClick)
        {
            var btnObj = UIPrimitiveFactory.CreateUIObject("Btn", parent);
            UIPrimitiveFactory.AddLayoutElement(btnObj.gameObject, minWidth: 120, preferredWidth: 160, minHeight: 40);
            var btnImg = btnObj.gameObject.AddComponent<Image>();
            btnImg.color = color;
            var btn = btnObj.gameObject.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            var btnLabel = UIPrimitiveFactory.AddThemedText(btnObj, label, 16, Color.black);
            btnLabel.alignment = TextAlignmentOptions.Center;
            UIPrimitiveFactory.StretchFull(btnLabel.rectTransform);
            btn.onClick.AddListener(() => onClick());
        }
    }
}
