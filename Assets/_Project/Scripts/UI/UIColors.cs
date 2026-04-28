using HollowGround.Buildings;
using HollowGround.Heroes;
using HollowGround.Resources;
using HollowGround.World;
using UnityEngine;

namespace HollowGround.UI
{
    public static class UIColors
    {
        public struct PanelColors
        {
            public Color PanelBg;
            public Color RowBg;
            public Color Text;
            public Color Muted;
            public Color Ok;
            public Color Gold;
            public Color Danger;
            public Color Warn;
        }

        public static readonly PanelColors Default = new()
        {
            PanelBg = new Color(0.08f, 0.09f, 0.11f, 0.92f),
            RowBg = new Color(0.14f, 0.15f, 0.17f, 1f),
            Text = new Color(0.95f, 0.95f, 0.95f, 1f),
            Muted = new Color(0.65f, 0.65f, 0.7f, 1f),
            Ok = new Color(0.35f, 0.8f, 0.4f, 1f),
            Gold = new Color(1f, 0.85f, 0.3f, 1f),
            Danger = new Color(0.9f, 0.3f, 0.3f, 1f),
            Warn = new Color(0.95f, 0.55f, 0.2f, 1f)
        };

        public static readonly Color Summon = new(0.3f, 0.6f, 0.95f, 1f);

        public static Color GetRarityColor(HeroRarity rarity)
        {
            return rarity switch
            {
                HeroRarity.Common => Default.Muted,
                HeroRarity.Uncommon => new Color(0.3f, 0.9f, 0.3f, 1f),
                HeroRarity.Rare => new Color(0.3f, 0.6f, 1f, 1f),
                HeroRarity.Epic => new Color(0.7f, 0.3f, 0.9f, 1f),
                HeroRarity.Legendary => Default.Gold,
                _ => Default.Muted
            };
        }

        public static Color GetNodeColor(MapNodeType type)
        {
            return type switch
            {
                MapNodeType.PlayerBase => new Color(0.95f, 0.75f, 0.15f, 1f),
                MapNodeType.ResourceNode => new Color(0.3f, 0.75f, 0.35f, 1f),
                MapNodeType.MutantCamp => new Color(0.85f, 0.25f, 0.25f, 1f),
                MapNodeType.AbandonedBuilding => new Color(0.6f, 0.45f, 0.25f, 1f),
                MapNodeType.NPCSettlement => new Color(0.3f, 0.7f, 0.9f, 1f),
                MapNodeType.RadioactiveZone => new Color(0.75f, 0.3f, 0.85f, 1f),
                MapNodeType.BossArea => new Color(0.95f, 0.5f, 0.1f, 1f),
                _ => new Color(0.28f, 0.28f, 0.3f, 1f)
            };
        }

        public static Color GetStateColor(BuildingState state)
        {
            return state switch
            {
                BuildingState.Damaged => new Color(0.9f, 0.3f, 0.2f),
                BuildingState.Destroyed => new Color(0.6f, 0.1f, 0.1f),
                BuildingState.Constructing => new Color(0.9f, 0.7f, 0.2f),
                BuildingState.Upgrading => new Color(0.3f, 0.7f, 0.9f),
                BuildingState.Active => Default.Ok,
                _ => Default.Text
            };
        }

        public static Color GetResourceColor(ResourceType type)
        {
            return type switch
            {
                ResourceType.Wood => new Color(0.76f, 0.55f, 0.2f, 1f),
                ResourceType.Metal => new Color(0.7f, 0.75f, 0.82f, 1f),
                ResourceType.Food => new Color(0.35f, 0.8f, 0.4f, 1f),
                ResourceType.Water => new Color(0.3f, 0.6f, 0.95f, 1f),
                ResourceType.TechPart => new Color(0.7f, 0.3f, 0.9f, 1f),
                ResourceType.Energy => new Color(1f, 0.85f, 0.3f, 1f),
                _ => Default.Text
            };
        }

        public static readonly Color Fog = new(0.12f, 0.12f, 0.14f, 1f);
        public static readonly Color Empty = new(0.28f, 0.28f, 0.3f, 1f);
        public static readonly Color Selected = new(1f, 0.95f, 0.4f, 1f);
        public static readonly Color PanelInner = new(0.14f, 0.15f, 0.17f, 1f);
        public static readonly Color TextDim = new(0.45f, 0.45f, 0.48f, 1f);
    }
}
