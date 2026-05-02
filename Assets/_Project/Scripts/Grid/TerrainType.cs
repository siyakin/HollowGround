namespace HollowGround.Grid
{
    public enum TerrainType
    {
        Flat = 0,
        Water = 1,
        River = 2,
        Mountain = 3,
        Rock = 4,
        Cliff = 5,
        Sand = 6,
        Forest = 7
    }

    public static class TerrainRules
    {
        public static bool IsBuildable(this TerrainType t)
        {
            return t == TerrainType.Flat || t == TerrainType.Sand;
        }

        public static bool IsPassable(this TerrainType t)
        {
            return t != TerrainType.Water
                && t != TerrainType.Mountain
                && t != TerrainType.Rock
                && t != TerrainType.Cliff;
        }

        public static bool IsWater(this TerrainType t)
        {
            return t == TerrainType.Water || t == TerrainType.River;
        }

        public static bool IsRemovable(this TerrainType t)
        {
            return t == TerrainType.Rock || t == TerrainType.Forest;
        }

        public static float MovementSpeedMultiplier(this TerrainType t)
        {
            return t switch
            {
                TerrainType.Sand => 0.8f,
                TerrainType.Forest => 0.9f,
                TerrainType.Flat => 1f,
                _ => 0f
            };
        }
    }
}
