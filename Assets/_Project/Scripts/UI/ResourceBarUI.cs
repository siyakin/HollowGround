using System;
using System.Collections.Generic;
using HollowGround.Resources;
using TMPro;
using UnityEngine;

namespace HollowGround.UI
{
    public class ResourceBarUI : MonoBehaviour
    {
        [Serializable]
        public class ResourceSlot
        {
            public ResourceType Type;
            public TMP_Text AmountText;
            public TMP_Text CapacityText;
        }

        [SerializeField] private List<ResourceSlot> _slots = new();
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _populationText;

        private Dictionary<ResourceType, ResourceSlot> _slotMap;

        private void Awake()
        {
            _slotMap = new Dictionary<ResourceType, ResourceSlot>();
            foreach (var slot in _slots)
                _slotMap[slot.Type] = slot;
        }

        private void Start()
        {
            SubscribeEvents();
            RefreshAll();
        }

        private void OnEnable()
        {
            SubscribeEvents();
            RefreshAll();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (ResourceManager.Instance == null) return;
            ResourceManager.Instance.OnResourceChanged -= HandleResourceChanged;
            ResourceManager.Instance.OnAllResourcesChanged -= RefreshAll;
            ResourceManager.Instance.OnResourceChanged += HandleResourceChanged;
            ResourceManager.Instance.OnAllResourcesChanged += RefreshAll;
        }

        private void UnsubscribeEvents()
        {
            if (ResourceManager.Instance == null) return;
            ResourceManager.Instance.OnResourceChanged -= HandleResourceChanged;
            ResourceManager.Instance.OnAllResourcesChanged -= RefreshAll;
        }

        private void HandleResourceChanged(ResourceType type, int amount)
        {
            if (_slotMap.TryGetValue(type, out ResourceSlot slot))
                UpdateSlot(slot);
        }

        private void RefreshAll()
        {
            foreach (var slot in _slots)
                UpdateSlot(slot);
            UpdatePopulation();
        }

        private void UpdateSlot(ResourceSlot slot)
        {
            if (ResourceManager.Instance == null || slot.AmountText == null) return;
            int amount = ResourceManager.Instance.Get(slot.Type);
            int capacity = ResourceManager.Instance.GetCapacity(slot.Type);
            slot.AmountText.text = $"{amount}/{capacity}";
        }

        private void UpdatePopulation()
        {
            if (_populationText == null) return;
            _populationText.text = "0/0";
        }

        public void UpdateLevel(int level)
        {
            if (_levelText != null)
                _levelText.text = $"Lv.{level}";
        }
    }
}
