using UnityEditor;
using UnityEngine;

namespace HollowGround.Editor
{
#if UNITY_EDITOR
    public static class GhostMaterialCreator
    {
        [MenuItem("HollowGround/Create Ghost Materials")]
        public static void CreateGhostMaterials()
        {
            string folder = "Assets/_Project/Materials/Ghost";

            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/_Project/Materials", "Ghost");

            if (!AssetDatabase.LoadAssetAtPath<Material>($"{folder}/Ghost_Valid.mat"))
            {
                var valid = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    color = new Color(0, 1, 0, 0.4f)
                };
                valid.SetFloat("_Surface", 1);
                valid.SetFloat("_Blend", 0);
                valid.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                valid.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                valid.SetInt("_ZWrite", 0);
                valid.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                AssetDatabase.CreateAsset(valid, $"{folder}/Ghost_Valid.mat");
            }

            if (!AssetDatabase.LoadAssetAtPath<Material>($"{folder}/Ghost_Invalid.mat"))
            {
                var invalid = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    color = new Color(1, 0, 0, 0.4f)
                };
                invalid.SetFloat("_Surface", 1);
                invalid.SetFloat("_Blend", 0);
                invalid.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                invalid.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                invalid.SetInt("_ZWrite", 0);
                invalid.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                AssetDatabase.CreateAsset(invalid, $"{folder}/Ghost_Invalid.mat");
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[HollowGround] Ghost materials created at Assets/_Project/Materials/Ghost/");
        }
    }
#endif
}
