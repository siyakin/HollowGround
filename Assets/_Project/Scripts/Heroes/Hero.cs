using System;
using UnityEngine;

namespace HollowGround.Heroes
{
    [Serializable]
    public class Hero
    {
        public string Id;
        public HeroData Data;
        public int Level = 1;
        public int CurrentXP;
        public bool IsDeployed;
        public bool IsInjured;
        public float InjuryTimer;

        public int CurrentHP => Data.GetHP(Level);
        public int CurrentAttack => Mathf.CeilToInt(Data.GetAttack(Level) * Data.GetRarityMultiplier());
        public int CurrentDefense => Mathf.CeilToInt(Data.GetDefense(Level) * Data.GetRarityMultiplier());
        public float TotalArmyBonus => Data.ArmyAttackBonus + Data.ArmyDefenseBonus;

        public EquipmentItem Weapon;
        public EquipmentItem Armor;
        public EquipmentItem Accessory;

        public event Action<Hero> OnLevelUp;

        public Hero(HeroData data)
        {
            Id = Guid.NewGuid().ToString().Substring(0, 8);
            Data = data;
            Level = 1;
            CurrentXP = 0;
            IsDeployed = false;
            IsInjured = false;
        }

        public void AddXP(int amount)
        {
            CurrentXP += amount;

            int xpNeeded = Data.GetXPForLevel(Level);
            while (CurrentXP >= xpNeeded && Level < 50)
            {
                CurrentXP -= xpNeeded;
                Level++;
                OnLevelUp?.Invoke(this);
                xpNeeded = Data.GetXPForLevel(Level);
            }
        }

        public float GetXPProgress()
        {
            int xpNeeded = Data.GetXPForLevel(Level);
            return xpNeeded > 0 ? (float)CurrentXP / xpNeeded : 0f;
        }

        public void SetInjured(float duration)
        {
            IsInjured = true;
            InjuryTimer = duration;
        }

        public void TickInjury(float deltaTime)
        {
            if (!IsInjured) return;
            InjuryTimer -= deltaTime;
            if (InjuryTimer <= 0f)
            {
                IsInjured = false;
                InjuryTimer = 0f;
            }
        }

        public int GetTotalBonusAttack()
        {
            int bonus = 0;
            if (Weapon != null) bonus += Weapon.AttackBonus;
            if (Armor != null) bonus += Armor.AttackBonus;
            if (Accessory != null) bonus += Accessory.AttackBonus;
            return bonus;
        }

        public int GetTotalBonusDefense()
        {
            int bonus = 0;
            if (Weapon != null) bonus += Weapon.DefenseBonus;
            if (Armor != null) bonus += Armor.DefenseBonus;
            if (Accessory != null) bonus += Accessory.DefenseBonus;
            return bonus;
        }
    }

    [Serializable]
    public class EquipmentItem
    {
        public string Name;
        public EquipmentSlot Slot;
        public int AttackBonus;
        public int DefenseBonus;
        public int HPBonus;
        public HeroRarity Rarity;
    }
}
