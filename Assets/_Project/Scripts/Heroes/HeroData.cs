using HollowGround.Buildings;
using HollowGround.Resources;
using UnityEngine;

namespace HollowGround.Heroes
{
    [CreateAssetMenu(fileName = "HeroData", menuName = "HollowGround/HeroData")]
    public class HeroData : ScriptableObject
    {
        [Header("Info")]
        public string DisplayName;
        public string Description;
        public HeroRole Role;
        public HeroRarity Rarity;
        public GameObject ModelPrefab;
        public Sprite Portrait;

        [Header("Base Stats")]
        public int BaseHP = 200;
        public int BaseAttack = 20;
        public int BaseDefense = 10;
        public float BaseSpeed = 3f;

        [Header("Abilities")]
        public string PassiveAbilityName;
        public string PassiveAbilityDesc;
        public float PassiveBonus = 0.1f;

        public string ActiveAbilityName;
        public string ActiveAbilityDesc;
        public float ActiveAbilityCooldown = 30f;

        [Header("Hero Buff")]
        public float ArmyAttackBonus = 0.05f;
        public float ArmyDefenseBonus = 0.03f;

        [Header("Summon Cost")]
        public int SummonCost = 100;
        public ResourceType SummonResource = ResourceType.TechPart;

        public int GetHP(int level) => Mathf.CeilToInt(BaseHP * (1f + (level - 1) * 0.12f));
        public int GetAttack(int level) => Mathf.CeilToInt(BaseAttack * (1f + (level - 1) * 0.1f));
        public int GetDefense(int level) => Mathf.CeilToInt(BaseDefense * (1f + (level - 1) * 0.1f));

        public int GetXPForLevel(int level)
        {
            return Mathf.CeilToInt(100 * Mathf.Pow(1.5f, level - 1));
        }

        public float GetRarityMultiplier()
        {
            return Rarity switch
            {
                HeroRarity.Common => 1f,
                HeroRarity.Uncommon => 1.2f,
                HeroRarity.Rare => 1.5f,
                HeroRarity.Epic => 2f,
                HeroRarity.Legendary => 3f,
                _ => 1f
            };
        }
    }
}
