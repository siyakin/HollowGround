using System.Collections.Generic;
using HollowGround.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class SaveMenuUI : MonoBehaviour
    {
        [Header("Scene Bindings")]
        [SerializeField] private RectTransform _contentContainer;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private Button _loadBtn;
        [SerializeField] private Button _deleteBtn;
        [SerializeField] private GameObject _confirmRow;

        private int _selectedSlotIndex = -1;
        private List<SaveData> _saves = new();
        private Transform _listContainer;
        private List<Image> _rowImages = new();
        private float _lastClickTime;
        private int _lastClickIndex = -1;

        private void OnEnable()
        {
            _listContainer = _contentContainer;
            if (HollowGround.Core.TimeManager.Instance != null && !HollowGround.Core.TimeManager.Instance.IsPaused)
                HollowGround.Core.TimeManager.Instance.TogglePause();
            RefreshList();
        }

        private void OnDisable()
        {
            if (HollowGround.Core.TimeManager.Instance != null && HollowGround.Core.TimeManager.Instance.IsPaused)
            {
                if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Paused)
                    HollowGround.Core.TimeManager.Instance.TogglePause();
            }
        }

        public void RefreshList()
        {
            if (_listContainer == null) return;

            _selectedSlotIndex = -1;
            _rowImages.Clear();
            if (_confirmRow != null) _confirmRow.SetActive(false);

            for (int i = _listContainer.childCount - 1; i >= 0; i--)
                DestroyImmediate(_listContainer.GetChild(i).gameObject);

            UpdateActionButtons();

            if (SaveSystem.Instance == null)
            {
                if (_statusText != null) _statusText.text = "SaveSystem not found!";
                return;
            }

            _saves = SaveSystem.Instance.GetAllSaves();

            if (_saves.Count == 0)
            {
                var empty = UIPrimitiveFactory.AddThemedText(_listContainer, "No save files found.", 16, UIColors.Default.Muted, TextAlignmentOptions.Center, UIStyleType.BodyText);
                UIPrimitiveFactory.AddLayoutElement(empty.gameObject, preferredHeight: 60, minHeight: 60);
                if (_statusText != null) _statusText.text = "No save files found.";
                return;
            }

            for (int i = 0; i < _saves.Count; i++)
                BuildSaveRow(i, _saves[i]);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_listContainer as RectTransform);

            if (_statusText != null) _statusText.text = $"Select a save file ({_saves.Count} saves)";
        }

        private void BuildSaveRow(int index, SaveData save)
        {
            var row = UIPrimitiveFactory.CreateUIObject($"Save_{index}", _listContainer);
            UIPrimitiveFactory.AddLayoutElement(row.gameObject, preferredHeight: 44, minHeight: 36);
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
            btn.onClick.AddListener(() => HandleRowClick(idx));
        }

        private void HandleRowClick(int idx)
        {
            if (_lastClickIndex == idx && Time.unscaledTime - _lastClickTime < 0.4f)
            {
                _lastClickIndex = -1;
                _selectedSlotIndex = idx;
                LoadSelected();
                return;
            }

            _lastClickIndex = idx;
            _lastClickTime = Time.unscaledTime;
            SelectSlot(idx);
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
            if (_confirmRow != null) _confirmRow.SetActive(false);

            Color selectedColor = UIColors.Default.Gold;
            selectedColor.a = 0.2f;

            for (int i = 0; i < _rowImages.Count; i++)
                _rowImages[i].color = i == idx ? selectedColor : UIColors.Default.RowBg;

            if (_statusText != null && idx >= 0 && idx < _saves.Count)
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

            string fileName = $"save_{System.DateTime.Now:yyyyMMdd_HHmmss}";
            SaveSystem.Instance.Save(fileName);
            RefreshList();

            for (int i = 0; i < _saves.Count; i++)
            {
                if (_saves[i].SaveName == fileName)
                {
                    SelectSlot(i);
                    break;
                }
            }
            if (_statusText != null) _statusText.text = "Game saved!";
        }

        public void LoadSelected()
        {
            if (_selectedSlotIndex < 0 || _selectedSlotIndex >= _saves.Count) return;
            if (SaveSystem.Instance == null) return;

            var save = _saves[_selectedSlotIndex];
            bool loaded = SaveSystem.Instance.Load(save.SaveName) != null;
            if (_statusText != null) _statusText.text = loaded ? "Game loaded!" : "Load failed!";
            if (loaded)
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.ResumeAfterLoad();
                ToastUI.Show($"Loaded: {save.SaveName}", UIColors.Default.Ok);
            }
        }

        public void RequestDelete()
        {
            if (_selectedSlotIndex < 0) return;
            if (_confirmRow != null) _confirmRow.SetActive(true);
        }

        public void ConfirmDelete()
        {
            if (_confirmRow != null) _confirmRow.SetActive(false);

            if (_selectedSlotIndex < 0 || _selectedSlotIndex >= _saves.Count) return;
            if (SaveSystem.Instance == null) return;

            var save = _saves[_selectedSlotIndex];
            SaveSystem.Instance.DeleteSave(save.SaveName);
            if (_statusText != null) _statusText.text = "Save deleted!";
            RefreshList();
        }

        public void CancelDelete()
        {
            if (_confirmRow != null) _confirmRow.SetActive(false);
        }

        public void CloseSelf()
        {
            gameObject.SetActive(false);
            if (UIManager.Instance != null)
                UIManager.Instance.ShowPauseFromSaveMenu();
        }
    }
}
