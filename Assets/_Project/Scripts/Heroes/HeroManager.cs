using System;
using System.Collections.Generic;
using System.Linq;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.Heroes
{
    public class HeroManager : MonoBehaviour
    {
        public static HeroManager Instance { get; private set; }

        private readonly List<Hero> _heroes = new();

        public List<Hero> AllHeroes => _heroes;
        public int HeroCount => _heroes.Count;

        public event Action<Hero> OnHeroAdded;
        public event Action<Hero> OnHeroRemoved;
        public event Action<Hero> OnHeroLevelUp;
        public event Action OnHeroesChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            foreach (var hero in _heroes)
            {
                hero.TickInjury(Time.deltaTime);
            }
        }

        public Hero GetHero(string id)
        {
            return _heroes.FirstOrDefault(h => h.Id == id);
        }

        public List<Hero> GetAvailableHeroes()
        {
            return _heroes.Where(h => !h.IsDeployed && !h.IsInjured).ToList();
        }

        public List<Hero> GetDeployedHeroes()
        {
            return _heroes.Where(h => h.IsDeployed).ToList();
        }

        public void AddHero(HeroData data)
        {
            var hero = new Hero(data);
            hero.OnLevelUp += HandleHeroLevelUp;
            _heroes.Add(hero);
            OnHeroAdded?.Invoke(hero);
            OnHeroesChanged?.Invoke();
        }

        public void RemoveHero(string id)
        {
            var hero = GetHero(id);
            if (hero == null) return;

            hero.OnLevelUp -= HandleHeroLevelUp;
            _heroes.Remove(hero);
            OnHeroRemoved?.Invoke(hero);
            OnHeroesChanged?.Invoke();
        }

        public bool CanSummon(HeroData data)
        {
            if (ResourceManager.Instance == null) return false;
            return ResourceManager.Instance.Get(data.SummonResource) >= data.SummonCost;
        }

        public bool SummonHero(HeroData data)
        {
            if (!CanSummon(data)) return false;

            ResourceManager.Instance.Spend(data.SummonResource, data.SummonCost);
            AddHero(data);
            return true;
        }

        public HeroData GetRandomHeroByRarity()
        {
            float roll = UnityEngine.Random.value;

            HeroRarity rarity;
            if (roll < 0.50f) rarity = HeroRarity.Common;
            else if (roll < 0.80f) rarity = HeroRarity.Uncommon;
            else if (roll < 0.95f) rarity = HeroRarity.Rare;
            else if (roll < 0.99f) rarity = HeroRarity.Epic;
            else rarity = HeroRarity.Legendary;

            var allData = UnityEngine.Resources.LoadAll<HeroData>("Heroes");
            var matching = allData.Where(h => h.Rarity == rarity).ToList();

            if (matching.Count == 0)
            {
                allData = UnityEngine.Resources.LoadAll<HeroData>("Heroes");
                matching = allData.ToList();
            }

            if (matching.Count == 0) return null;

            return matching[UnityEngine.Random.Range(0, matching.Count)];
        }

        public float GetDeployedArmyBonus()
        {
            float bonus = 0f;
            foreach (var hero in GetDeployedHeroes())
            {
                bonus += hero.TotalArmyBonus;
            }
            return bonus;
        }

        public void GiveXPToAll(int amount)
        {
            foreach (var hero in _heroes)
            {
                if (hero.IsDeployed)
                    hero.AddXP(amount);
            }
        }

        private void HandleHeroLevelUp(Hero hero)
        {
            OnHeroLevelUp?.Invoke(hero);
            OnHeroesChanged?.Invoke();
        }
    }
}
