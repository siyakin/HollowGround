using System.Collections.Generic;
using HollowGround.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class SaveMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject _saveSlotPrefab;
        [SerializeField] private Transform _saveListContainer;
        [SerializeField] private Button _newSaveBtn;
        [SerializeField] private Button _loadBtn;
        [SerializeField] private Button _deleteBtn;
        [SerializeField] private Button _closeBtn;
        [SerializeField] private TMP_Text _infoText;

        private int _selectedSlotIndex = -1;
        private List<SaveData> _saves = new();

        private void OnEnable()
        {
            RefreshList();
        }

        public void ShowSaveMode()
        {
            if (_newSaveBtn != null) _newSaveBtn.gameObject.SetActive(true);
            RefreshList();
        }

        public void ShowLoadMode()
        {
            if (_newSaveBtn != null) _newSaveBtn.gameObject.SetActive(false);
            RefreshList();
        }

        public void RefreshList()
        {
            ClearContainer();
            _selectedSlotIndex = -1;

            if (SaveSystem.Instance == null) return;

            _saves = SaveSystem.Instance.GetAllSaves();

            if (_saveSlotPrefab == null || _saveListContainer == null) return;

            for (int i = 0; i < _saves.Count; i++)
            {
                var save = _saves[i];
                var slot = Instantiate(_saveSlotPrefab, _saveListContainer);
                SetupSlot(slot, save, i);
            }

            UpdateInfo("Select a save file");
        }

        private void SetupSlot(GameObject slot, SaveData save, int index)
        {
            var nameText = slot.transform.Find("NameText")?.GetComponent<TMP_Text>();
            var dateText = slot.transform.Find("DateText")?.GetComponent<TMP_Text>();
            var infoText = slot.transform.Find("InfoText")?.GetComponent<TMP_Text>();
            var button = slot.GetComponent<Button>();

            if (nameText != null)
                nameText.text = save.SaveName;

            if (dateText != null)
                dateText.text = save.SaveDate;

            if (infoText != null)
            {
                int minutes = Mathf.FloorToInt(save.PlayTime / 60f);
                int hours = minutes / 60;
                minutes %= 60;
                infoText.text = $"Playtime: {hours}h {minutes}m | Buildings: {save.Buildings.Count} | Heroes: {save.Heroes.Count}";
            }

            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    _selectedSlotIndex = index;
                    UpdateInfo($"Selected: {save.SaveName}");
                });
            }
        }

        public void NewSave()
        {
            if (SaveSystem.Instance == null) return;

            string fileName = $"save_{System.DateTime.Now:yyyyMMdd_HHmmss}";
            SaveSystem.Instance.Save(fileName);
            UpdateInfo("Game saved!");
            RefreshList();
        }

        public void LoadSelected()
        {
            if (_selectedSlotIndex < 0 || _selectedSlotIndex >= _saves.Count) return;
            if (SaveSystem.Instance == null) return;

            var save = _saves[_selectedSlotIndex];
            SaveSystem.Instance.Load(save.SaveName);
            UpdateInfo("Game loaded!");
            gameObject.SetActive(false);
        }

        public void DeleteSelected()
        {
            if (_selectedSlotIndex < 0 || _selectedSlotIndex >= _saves.Count) return;
            if (SaveSystem.Instance == null) return;

            var save = _saves[_selectedSlotIndex];
            SaveSystem.Instance.DeleteSave(save.SaveName);
            UpdateInfo("Save deleted!");
            RefreshList();
        }

        public void QuickSave()
        {
            if (SaveSystem.Instance == null) return;
            SaveSystem.Instance.QuickSave();
            UpdateInfo("Quick saved!");
        }

        public void QuickLoad()
        {
            if (SaveSystem.Instance == null) return;
            if (SaveSystem.Instance.HasSave("quicksave"))
            {
                SaveSystem.Instance.Load("quicksave");
                gameObject.SetActive(false);
            }
            else
            {
                UpdateInfo("No quicksave found!");
            }
        }

        private void UpdateInfo(string message)
        {
            if (_infoText != null)
                _infoText.text = message;
        }

        private void ClearContainer()
        {
            if (_saveListContainer == null) return;
            for (int i = _saveListContainer.childCount - 1; i >= 0; i--)
                Destroy(_saveListContainer.GetChild(i).gameObject);
        }
    }
}
