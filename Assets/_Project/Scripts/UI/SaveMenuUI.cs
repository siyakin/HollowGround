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
        private TMP_Text _statusText;
        private Button _loadBtn;
        private Button _deleteBtn;
        private GameObject _confirmRow;
        private List<Image> _rowImages = new();
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

            UIPrimitiveFactory.AddStandardVLG(gameObject, spacing: 10f);

            var headerText = UIPrimitiveFactory.AddThemedText(transform, "SAVE / LOAD", 30, UIColors.Default.Gold, TextAlignmentOptions.Center, UIStyleType.HeaderText);
            UIPrimitiveFactory.AddLayoutElement(headerText.gameObject, preferredHeight: 50);

            BuildScrollList(root);

            _statusText = UIPrimitiveFactory.AddThemedText(transform, "Select a save file", 17, UIColors.Default.Muted, TextAlignmentOptions.Center, UIStyleType.BodyText);
            UIPrimitiveFactory.AddLayoutElement(_statusText.gameObject, preferredHeight: 30);

            BuildConfirmRow(root);
            BuildButtonRow(root);

            _built = true;
        }

        private void BuildScrollList(RectTransform root)
        {
            var scrollObj = UIPrimitiveFactory.CreateUIObject("ScrollList", root);
            UIPrimitiveFactory.AddLayoutElement(scrollObj.gameObject, preferredHeight: 280, minHeight: 100);

            var scrollRect = scrollObj.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.scrollSensitivity = 20f;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            var viewportRt = UIPrimitiveFactory.CreateUIObject("Viewport", scrollObj);
            UIPrimitiveFactory.StretchFull(viewportRt);
            viewportRt.gameObject.AddComponent<RectMask2D>();
            scrollRect.viewport = viewportRt;

            var contentRt = UIPrimitiveFactory.CreateUIObject("Content", viewportRt);
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = Vector2.zero;
            contentRt.anchoredPosition = Vector2.zero;

            var contentVLG = contentRt.gameObject.AddComponent<VerticalLayoutGroup>();
            contentVLG.padding = new RectOffset(8, 8, 6, 6);
            contentVLG.spacing = 6f;
            contentVLG.childControlWidth = true;
            contentVLG.childControlHeight = false;
            contentVLG.childForceExpandWidth = true;
            contentVLG.childForceExpandHeight = false;

            contentRt.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRt;
            _listContainer = contentRt;
        }

        private void BuildConfirmRow(RectTransform root)
        {
            _confirmRow = UIPrimitiveFactory.CreateUIObject("ConfirmRow", root).gameObject;
            UIPrimitiveFactory.AddLayoutElement(_confirmRow, preferredHeight: 48);
            var hlg = UIPrimitiveFactory.AddRowHLG(_confirmRow);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;

            var qText = UIPrimitiveFactory.AddThemedText(_confirmRow.transform, "Delete this save?", 17, UIColors.Default.Warn, TextAlignmentOptions.MidlineRight, UIStyleType.WarningText);
            UIPrimitiveFactory.AddLayoutElement(qText.gameObject, preferredWidth: 215, minHeight: 42);

            var yesBtn = UIPrimitiveFactory.CreateThemedButton(_confirmRow.transform, "YesBtn", "YES", ConfirmDelete, UIStyleType.DangerButton);
            UIPrimitiveFactory.AddLayoutElement(yesBtn.gameObject, minWidth: 88, preferredWidth: 88, minHeight: 42);

            var noBtn = UIPrimitiveFactory.CreateThemedButton(_confirmRow.transform, "NoBtn", "NO", CancelDelete, UIStyleType.ActionBarButton);
            UIPrimitiveFactory.AddLayoutElement(noBtn.gameObject, minWidth: 88, preferredWidth: 88, minHeight: 42);

            _confirmRow.SetActive(false);
        }

        private void BuildButtonRow(RectTransform root)
        {
            var btnRow = UIPrimitiveFactory.CreateUIObject("BtnRow", root);
            UIPrimitiveFactory.AddLayoutElement(btnRow.gameObject, preferredHeight: 52);
            var hlg = UIPrimitiveFactory.AddRowHLG(btnRow.gameObject);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.spacing = 12f;

            var newBtn = UIPrimitiveFactory.CreateThemedButton(btnRow, "NewSaveBtn", "NEW SAVE", NewSave, UIStyleType.ConfirmButton);
            UIPrimitiveFactory.AddLayoutElement(newBtn.gameObject, minWidth: 140, preferredWidth: 140, minHeight: 46);

            _loadBtn = UIPrimitiveFactory.CreateThemedButton(btnRow, "LoadBtn", "LOAD", LoadSelected, UIStyleType.ActionBarButton);
            UIPrimitiveFactory.AddLayoutElement(_loadBtn.gameObject, minWidth: 110, preferredWidth: 110, minHeight: 46);

            _deleteBtn = UIPrimitiveFactory.CreateThemedButton(btnRow, "DeleteBtn", "DELETE", RequestDelete, UIStyleType.DangerButton);
            UIPrimitiveFactory.AddLayoutElement(_deleteBtn.gameObject, minWidth: 110, preferredWidth: 110, minHeight: 46);

            var backBtn = UIPrimitiveFactory.CreateThemedButton(btnRow, "BackBtn", "BACK", CloseSelf, UIStyleType.ActionBarButton);
            UIPrimitiveFactory.AddLayoutElement(backBtn.gameObject, minWidth: 110, preferredWidth: 110, minHeight: 46);
        }

        public void RefreshList()
        {
            if (!_built || _listContainer == null) return;

            _selectedSlotIndex = -1;
            _rowImages.Clear();
            _confirmRow?.SetActive(false);

            for (int i = _listContainer.childCount - 1; i >= 0; i--)
                Destroy(_listContainer.GetChild(i).gameObject);

            UpdateActionButtons();

            if (SaveSystem.Instance == null)
            {
                _statusText.text = "SaveSystem not found!";
                return;
            }

            _saves = SaveSystem.Instance.GetAllSaves();

            if (_saves.Count == 0)
            {
                var empty = UIPrimitiveFactory.AddThemedText(_listContainer, "No save files found.", 16, UIColors.Default.Muted, TextAlignmentOptions.Center, UIStyleType.BodyText);
                UIPrimitiveFactory.AddLayoutElement(empty.gameObject, preferredHeight: 60, minHeight: 60);
                _statusText.text = "No save files found.";
                return;
            }

            for (int i = 0; i < _saves.Count; i++)
                BuildSaveRow(i, _saves[i]);

            _statusText.text = "Select a save file";
        }

        private void BuildSaveRow(int index, SaveData save)
        {
            var row = UIPrimitiveFactory.CreateUIObject($"Save_{index}", _listContainer);
            UIPrimitiveFactory.AddLayoutElement(row.gameObject, preferredHeight: 60, minHeight: 60);
            var rowImg = UIPrimitiveFactory.AddImage(row, UIColors.Default.RowBg);
            _rowImages.Add(rowImg);

            var hlg = UIPrimitiveFactory.AddRowHLG(row.gameObject, padding: new RectOffset(16, 16, 0, 0), spacing: 10f);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;

            var nameText = UIPrimitiveFactory.AddThemedText(row, save.SaveName, 18, UIColors.Default.Text, TextAlignmentOptions.MidlineLeft, UIStyleType.LabelText);
            var nameLE = UIPrimitiveFactory.AddLayoutElement(nameText.gameObject);
            nameLE.flexibleWidth = 1f;

            int totalMinutes = Mathf.FloorToInt(save.PlayTime / 60f);
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;
            var infoText = UIPrimitiveFactory.AddThemedText(row, $"B:{save.Buildings.Count}  H:{save.Heroes.Count}  |  {hours}h{minutes}m", 14, UIColors.Default.Muted, TextAlignmentOptions.MidlineLeft, UIStyleType.BodyText);
            UIPrimitiveFactory.AddLayoutElement(infoText.gameObject, preferredWidth: 170, minWidth: 130);

            var dateText = UIPrimitiveFactory.AddThemedText(row, ParseSaveDate(save.SaveDate), 14, UIColors.Default.Muted, TextAlignmentOptions.MidlineRight, UIStyleType.BodyText);
            UIPrimitiveFactory.AddLayoutElement(dateText.gameObject, preferredWidth: 100, minWidth: 80);

            var btn = row.gameObject.AddComponent<Button>();
            btn.targetGraphic = rowImg;

            int idx = index;
            btn.onClick.AddListener(() => SelectSlot(idx));
        }

        private string ParseSaveDate(string isoDate)
        {
            if (string.IsNullOrEmpty(isoDate)) return "";
            if (System.DateTime.TryParse(isoDate, out var dt))
                return dt.ToString("MM/dd  HH:mm");
            return isoDate;
        }

        private void SelectSlot(int idx)
        {
            _selectedSlotIndex = idx;
            _confirmRow?.SetActive(false);

            Color selectedColor = UIColors.Default.Gold;
            selectedColor.a = 0.2f;

            for (int i = 0; i < _rowImages.Count; i++)
                _rowImages[i].color = i == idx ? selectedColor : UIColors.Default.RowBg;

            if (idx >= 0 && idx < _saves.Count)
                _statusText.text = $"Selected: {_saves[idx].SaveName}";

            UpdateActionButtons();
        }

        private void UpdateActionButtons()
        {
            bool hasSelection = _selectedSlotIndex >= 0 && _selectedSlotIndex < _saves.Count;
            if (_loadBtn != null) _loadBtn.interactable = hasSelection;
            if (_deleteBtn != null) _deleteBtn.interactable = hasSelection;
        }

        public void NewSave()
        {
            if (SaveSystem.Instance == null) return;

            string fileName = $"Save_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
            SaveSystem.Instance.Save(fileName);
            RefreshList();
            if (_saves.Count > 0) SelectSlot(0);
            _statusText.text = "Game saved!";
        }

        public void LoadSelected()
        {
            if (_selectedSlotIndex < 0 || _selectedSlotIndex >= _saves.Count) return;
            if (SaveSystem.Instance == null) return;

            var save = _saves[_selectedSlotIndex];
            bool loaded = SaveSystem.Instance.Load(save.SaveName) != null;
            _statusText.text = loaded ? "Game loaded!" : "Load failed!";
            if (loaded) gameObject.SetActive(false);
        }

        private void RequestDelete()
        {
            if (_selectedSlotIndex < 0) return;
            _confirmRow?.SetActive(true);
        }

        private void ConfirmDelete()
        {
            _confirmRow?.SetActive(false);

            if (_selectedSlotIndex < 0 || _selectedSlotIndex >= _saves.Count) return;
            if (SaveSystem.Instance == null) return;

            var save = _saves[_selectedSlotIndex];
            SaveSystem.Instance.DeleteSave(save.SaveName);
            _statusText.text = "Save deleted!";
            RefreshList();
        }

        private void CancelDelete()
        {
            _confirmRow?.SetActive(false);
        }

        public void CloseSelf()
        {
            gameObject.SetActive(false);
            if (UIManager.Instance != null)
                UIManager.Instance.ShowPauseFromSaveMenu();
        }
    }
}
