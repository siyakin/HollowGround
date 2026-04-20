using UnityEditor;
using UnityEngine;

namespace HollowGround.UI
{
    public static class ResourceIconGenerator
    {
        [MenuItem("HollowGround/Generate Resource Icons")]
        public static void Generate()
        {
            string folder = "Assets/_Project/Icons";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/_Project", "Icons");

            GenerateIcon(folder, "Icon_Wood", new Color(0.6f, 0.35f, 0.1f));
            GenerateIcon(folder, "Icon_Metal", new Color(0.7f, 0.7f, 0.75f));
            GenerateIcon(folder, "Icon_Food", new Color(0.2f, 0.75f, 0.2f));
            GenerateIcon(folder, "Icon_Water", new Color(0.2f, 0.5f, 0.9f));
            GenerateIcon(folder, "Icon_TechPart", new Color(0.85f, 0.65f, 0.1f));
            GenerateIcon(folder, "Icon_Energy", new Color(0.95f, 0.9f, 0.15f));

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log("[ResourceIconGenerator] Icons generated in " + folder);
        }

        private static void GenerateIcon(string folder, string name, Color color)
        {
            int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inside = IsInsideIcon(x, y, size);
                    tex.SetPixel(x, y, inside ? color : Color.clear);
                }
            }

            tex.Apply(false);

            string path = $"{folder}/{name}.png";
            byte[] png = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            System.IO.File.WriteAllBytes(path, png);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 64;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }
        }

        private static bool IsInsideIcon(int x, int y, int size)
        {
            int margin = 6;
            int inner = size - margin * 2;

            int cx = x - margin;
            int cy = y - margin;

            if (cx < 0 || cy < 0 || cx >= inner || cy >= inner) return false;

            float radius = inner * 0.45f;
            float dx = cx - inner * 0.5f;
            float dy = cy - inner * 0.5f;
            return dx * dx + dy * dy <= radius * radius;
        }
    }
}
