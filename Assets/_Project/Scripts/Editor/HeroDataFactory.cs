using HollowGround.Heroes;
using UnityEditor;
using UnityEngine;

namespace HollowGround.Editor
{
    public static class HeroDataFactory
    {
        private static readonly string Folder = "Assets/_Project/ScriptableObjects/Heroes";

        [MenuItem("HollowGround/Create All HeroData")]
        public static void CreateAll()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "Heroes");

            Create("Commander", "Born leader. Boosts army attack and defense.",
                HeroRole.Commander, HeroRarity.Rare, 250, 18, 15, 3f,
                "Rally", "Army attack +10% when deployed", 0.1f,
                "War Cry", "Temporarily doubles army power", 30f,
                0.08f, 0.05f, 150);

            Create("Warrior", "Frontline fighter. High HP and defense.",
                HeroRole.Warrior, HeroRarity.Common, 300, 15, 20, 2f,
                "Iron Skin", "Takes 15% less damage", 0.15f,
                "Shield Bash", "Stuns enemies briefly", 20f,
                0.03f, 0.08f, 100);

            Create("Ranger", "Long range specialist. High attack power.",
                HeroRole.Ranger, HeroRarity.Uncommon, 180, 25, 8, 4f,
                "Precision", "Critical hit chance +20%", 0.2f,
                "Sniper Shot", "Deals massive single-target damage", 25f,
                0.06f, 0.02f, 100);

            Create("Engineer", "Support specialist. Boosts defense and healing.",
                HeroRole.Engineer, HeroRarity.Uncommon, 200, 12, 18, 2.5f,
                "Fortify", "Buildings produce 15% faster", 0.15f,
                "Repair Field", "Heals wounded soldiers over time", 40f,
                0.02f, 0.06f, 100);

            Create("Scout", "Recon expert. Increases expedition speed.",
                HeroRole.Scout, HeroRarity.Common, 150, 10, 8, 6f,
                "Pathfinder", "Expeditions complete 20% faster", 0.2f,
                "Ambush", "First strike deals double damage", 15f,
                0.04f, 0.01f, 100);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[HeroDataFactory] 5 HeroData SOs created in " + Folder);
        }

        private static void Create(string name, string desc,
            HeroRole role, HeroRarity rarity,
            int hp, int atk, int def, float spd,
            string passiveName, string passiveDesc, float passiveBonus,
            string activeName, string activeDesc, float activeCd,
            float armyAtkBonus, float armyDefBonus,
            int summonCost)
        {
            string path = $"{Folder}/{name}.asset";
            if (AssetDatabase.LoadAssetAtPath<HeroData>(path) != null) return;

            var data = ScriptableObject.CreateInstance<HeroData>();
            data.DisplayName = name;
            data.Description = desc;
            data.Role = role;
            data.Rarity = rarity;
            data.BaseHP = hp;
            data.BaseAttack = atk;
            data.BaseDefense = def;
            data.BaseSpeed = spd;
            data.PassiveAbilityName = passiveName;
            data.PassiveAbilityDesc = passiveDesc;
            data.PassiveBonus = passiveBonus;
            data.ActiveAbilityName = activeName;
            data.ActiveAbilityDesc = activeDesc;
            data.ActiveAbilityCooldown = activeCd;
            data.ArmyAttackBonus = armyAtkBonus;
            data.ArmyDefenseBonus = armyDefBonus;
            data.SummonCost = summonCost;

            AssetDatabase.CreateAsset(data, path);
        }
    }
}
