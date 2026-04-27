using System.Collections.Generic;
using HollowGround.Core;
using UnityEngine;

namespace HollowGround.Resources
{
    public class ResourceManager : Singleton<ResourceManager>
    {

        [System.Serializable]
        public class ResourceAmount
        {
            public ResourceType Type;
            public int Amount;
        }

        [Header("Starting Resources")]
        [SerializeField] private List<ResourceAmount> _startingResources = new()
        {
            new() { Type = ResourceType.Wood, Amount = 200 },
            new() { Type = ResourceType.Metal, Amount = 100 },
            new() { Type = ResourceType.Food, Amount = 150 },
            new() { Type = ResourceType.Water, Amount = 80 },
            new() { Type = ResourceType.TechPart, Amount = 20 },
            new() { Type = ResourceType.Energy, Amount = 0 }
        };

        [Header("Capacity")]
        [SerializeField] private int _baseCapacity = 500;

        private readonly Dictionary<ResourceType, int> _resources = new();
        private readonly Dictionary<ResourceType, int> _capacity = new();

        public event System.Action<ResourceType, int> OnResourceChanged;
        public event System.Action OnAllResourcesChanged;

        protected override void Awake()
        {
            base.Awake();
            InitializeResources();
        }

        private void InitializeResources()
        {
            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                _resources[type] = 0;
                _capacity[type] = _baseCapacity;
            }

            foreach (var res in _startingResources)
            {
                int amount = res.Amount;
                var cfg = HollowGround.Core.GameConfig.Instance;
                if (cfg != null && cfg.BoostStartingResources)
                    amount *= cfg.BoostMultiplier;
                _resources[res.Type] = amount;
            }

            var config = HollowGround.Core.GameConfig.Instance;
            if (config != null && config.DevMode)
            {
                _resources[ResourceType.TechPart] = 200;
                _resources[ResourceType.Energy] = 100;
            }
        }

        public int Get(ResourceType type)
        {
            return _resources.TryGetValue(type, out int val) ? val : 0;
        }

        public int GetCapacity(ResourceType type)
        {
            return _capacity.TryGetValue(type, out int val) ? val : _baseCapacity;
        }

        public void Add(ResourceType type, int amount)
        {
            if (amount <= 0) return;

            int current = Get(type);
            int cap = GetCapacity(type);
            _resources[type] = Mathf.Min(current + amount, cap);
            OnResourceChanged?.Invoke(type, _resources[type]);
            OnAllResourcesChanged?.Invoke();
        }

        public bool Spend(ResourceType type, int amount)
        {
            if (amount <= 0) return true;

            int current = Get(type);
            if (current < amount) return false;

            _resources[type] = current - amount;
            OnResourceChanged?.Invoke(type, _resources[type]);
            OnAllResourcesChanged?.Invoke();
            return true;
        }

        public bool CanAfford(ResourceType type, int amount)
        {
            return Get(type) >= amount;
        }

        public bool CanAfford(Dictionary<ResourceType, int> costs)
        {
            foreach (var cost in costs)
            {
                if (Get(cost.Key) < cost.Value) return false;
            }
            return true;
        }

        public bool SpendMultiple(Dictionary<ResourceType, int> costs)
        {
            if (!CanAfford(costs)) return false;

            foreach (var cost in costs)
            {
                Spend(cost.Key, cost.Value);
            }
            return true;
        }

        public void SetCapacity(ResourceType type, int capacity)
        {
            _capacity[type] = capacity;
            if (_resources[type] > capacity)
                _resources[type] = capacity;
            OnResourceChanged?.Invoke(type, _resources[type]);
        }

        public void AddCapacity(ResourceType type, int extra)
        {
            SetCapacity(type, GetCapacity(type) + extra);
        }
    }
}
