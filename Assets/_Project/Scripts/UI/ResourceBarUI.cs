using System;
using System.Collections.Generic;
using HollowGround.Army;
using HollowGround.Buildings;
using HollowGround.Core;
using HollowGround.Heroes;
using HollowGround.NPCs;
using HollowGround.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class ResourceBarUI : MonoBehaviour
    {
        [Serializable]
        public class ResourceSlot
        {
            public ResourceType Type;
            public TMP_Text AmountText;
        }

        [SerializeField] private List<ResourceSlot> _slots = new();
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _populationText;
        [SerializeField] private TMP_Text _timeText;

        private Dictionary<ResourceType, ResourceSlot> _slotMap;
        private int _lastDisplayedSecond = -1;
        private float _lastDisplayedSpeed = -1f;

        private void Awake()
        {
            _slotMap = new Dictionary<ResourceType, ResourceSlot>();
            foreach (var slot in _slots)
                _slotMap[slot.Type] = slot;

            CompactSpacing();
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

        private void Update()
        {
            UpdateTime();
        }

        private void CompactSpacing()
        {
            var hlg = GetComponentInChildren<HorizontalLayoutGroup>();
            if (hlg != null)
            {
                hlg.spacing = 8;
                hlg.padding = new RectOffset(8, 8, 2, 2);
                hlg.childForceExpandWidth = true;
                hlg.childForceExpandHeight = false;
            }
        }

        private void SubscribeEvents()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged -= HandleResourceChanged;
                ResourceManager.Instance.OnAllResourcesChanged -= RefreshAll;
                ResourceManager.Instance.OnResourceChanged += HandleResourceChanged;
                ResourceManager.Instance.OnAllResourcesChanged += RefreshAll;
            }

            if (ArmyManager.Instance != null)
                ArmyManager.Instance.OnArmyUpdated += UpdatePopulation;

            if (HeroManager.Instance != null)
                HeroManager.Instance.OnHeroesChanged += UpdatePopulation;

            if (SettlerManager.Instance != null)
            {
                SettlerManager.Instance.OnSettlerSpawned += OnSettlerChanged;
                SettlerManager.Instance.OnSettlerRemoved += OnSettlerChanged;
            }

            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.OnCommandCenterLevelChanged -= UpdateLevel;
                BuildingManager.Instance.OnCommandCenterLevelChanged += UpdateLevel;
            }
        }

        private void UnsubscribeEvents()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged -= HandleResourceChanged;
                ResourceManager.Instance.OnAllResourcesChanged -= RefreshAll;
            }

            if (ArmyManager.Instance != null)
                ArmyManager.Instance.OnArmyUpdated -= UpdatePopulation;

            if (HeroManager.Instance != null)
                HeroManager.Instance.OnHeroesChanged -= UpdatePopulation;

            if (SettlerManager.Instance != null)
            {
                SettlerManager.Instance.OnSettlerSpawned -= OnSettlerChanged;
                SettlerManager.Instance.OnSettlerRemoved -= OnSettlerChanged;
            }

            if (BuildingManager.Instance != null)
                BuildingManager.Instance.OnCommandCenterLevelChanged -= UpdateLevel;
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
            UpdateTime();
            UpdateLevel(BuildingManager.Instance != null ? BuildingManager.Instance.GetCommandCenterLevel() : 0);
        }

        private void UpdateSlot(ResourceSlot slot)
        {
            if (ResourceManager.Instance == null || slot.AmountText == null) return;
            int amount   = ResourceManager.Instance.Get(slot.Type);
            int capacity = ResourceManager.Instance.GetCapacity(slot.Type);
            slot.AmountText.text = $"{GetDisplayName(slot.Type)} {amount}/{capacity}";
        }

        private static string GetDisplayName(ResourceType type) => type switch
        {
            ResourceType.Wood     => "Wood",
            ResourceType.Metal    => "Metal",
            ResourceType.Food     => "Food",
            ResourceType.Water    => "Water",
            ResourceType.TechPart => "Tech Part",
            ResourceType.Energy   => "Energy",
            _                     => type.ToString(),
        };

        private void UpdateTime()
        {
            if (_timeText == null || TimeManager.Instance == null) return;
            int t = Mathf.FloorToInt(TimeManager.Instance.GameTime);
            float speed = TimeManager.Instance.GameSpeed;
            if (t == _lastDisplayedSecond && Mathf.Approximately(speed, _lastDisplayedSpeed)) return;
            _lastDisplayedSecond = t;
            _lastDisplayedSpeed = speed;
            int h = t / 3600;
            int m = (t % 3600) / 60;
            int s = t % 60;
            _timeText.text = $"x{speed:F0}  {h:D2}:{m:D2}:{s:D2}";
        }

        private void UpdatePopulation()
        {
            if (_populationText == null) return;

            int troops = ArmyManager.Instance != null ? ArmyManager.Instance.TotalTroopCount : 0;
            int heroes = HeroManager.Instance != null ? HeroManager.Instance.HeroCount : 0;
            int people = SettlerManager.Instance != null ? SettlerManager.Instance.SettlerCount : 0;
            int pop = SettlerManager.Instance != null ? SettlerManager.Instance.TotalPopulation : 0;
            _populationText.text = $"Tro:{troops}  Her:{heroes}  Pop:{pop}  Ppl:{people}";
        }

        private void OnSettlerChanged(SettlerWalker _) => UpdatePopulation();

        public void UpdateLevel(int level)
        {
            if (_levelText != null)
                _levelText.text = $"Lv.{level}";
        }
    }
}
