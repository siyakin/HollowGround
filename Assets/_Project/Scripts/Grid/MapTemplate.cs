using UnityEngine;

namespace HollowGround.Grid
{
    [CreateAssetMenu(fileName = "MapTemplate", menuName = "HollowGround/MapTemplate")]
    public class MapTemplate : ScriptableObject
    {
        public int Width = 50;
        public int Height = 50;
        public TerrainType[] Tiles;

        public void Initialize(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new TerrainType[width * height];
        }

        public TerrainType GetTile(int x, int z)
        {
            if (x < 0 || x >= Width || z < 0 || z >= Height) return TerrainType.Mountain;
            return Tiles[z * Width + x];
        }

        public void SetTile(int x, int z, TerrainType type)
        {
            if (x < 0 || x >= Width || z < 0 || z >= Height) return;
            Tiles[z * Width + x] = type;
        }

        public void Fill(TerrainType type)
        {
            if (Tiles == null || Tiles.Length != Width * Height)
                Tiles = new TerrainType[Width * Height];
            for (int i = 0; i < Tiles.Length; i++)
                Tiles[i] = type;
        }

        public void CarveRect(int startX, int startZ, int w, int h, TerrainType type)
        {
            for (int x = startX; x < startX + w; x++)
                for (int z = startZ; z < startZ + h; z++)
                    SetTile(x, z, type);
        }

        public void CarveLine(int x0, int z0, int x1, int z1, TerrainType type, int thickness = 1)
        {
            int dx = Mathf.Abs(x1 - x0);
            int dz = Mathf.Abs(z1 - z0);
            int sx = x0 < x1 ? 1 : -1;
            int sz = z0 < z1 ? 1 : -1;
            int err = dx - dz;
            int half = thickness / 2;

            while (true)
            {
                for (int ox = -half; ox <= half; ox++)
                    for (int oz = -half; oz <= half; oz++)
                        SetTile(x0 + ox, z0 + oz, type);

                if (x0 == x1 && z0 == z1) break;
                int e2 = 2 * err;
                if (e2 > -dz) { err -= dz; x0 += sx; }
                if (e2 < dx) { err += dx; z0 += sz; }
            }
        }
    }
}
