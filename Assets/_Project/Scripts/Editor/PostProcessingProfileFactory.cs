using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace HollowGround.Editor
{
    public static class PostProcessingProfileFactory
    {
        const string PROFILE_PATH = "Assets/_Project/Settings/PostProcessProfile.asset";

        [MenuItem("HollowGround/Create Post-Processing Profile")]
        public static void CreateProfile()
        {
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(PROFILE_PATH);

            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, PROFILE_PATH);
                AssetDatabase.SaveAssets();
            }

            SetupBloom(profile);
            SetupVignette(profile);
            SetupColorAdjustments(profile);
            SetupWhiteBalance(profile);
            SetupFilmGrain(profile);
            SetupChromaticAberration(profile);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            Selection.activeObject = profile;
            EditorGUIUtility.PingObject(profile);

            Debug.Log("[PostProcess] Profile created at " + PROFILE_PATH);
        }

        [MenuItem("HollowGround/Setup Post-Processing Volume")]
        public static void SetupVolumeInScene()
        {
            var volume = Object.FindAnyObjectByType<Volume>();
            if (volume == null)
            {
                GameObject go = new("PostProcessVolume");
                volume = go.AddComponent<Volume>();
                go.transform.position = Vector3.zero;
            }

            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(PROFILE_PATH);
            if (profile == null)
            {
                CreateProfile();
                profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(PROFILE_PATH);
            }

            volume.profile = profile;
            volume.isGlobal = true;
            volume.priority = 0;
            volume.blendDistance = 0f;

            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("[PostProcess] Volume configured in scene.");
        }

        static void SetupBloom(VolumeProfile profile)
        {
            if (!profile.TryGet(out Bloom bloom))
                bloom = profile.Add<Bloom>(true);

            bloom.intensity.Override(0.2f);
            bloom.threshold.Override(1.5f);
            bloom.scatter.Override(0.5f);
            bloom.tint.Override(new Color(1f, 0.9f, 0.8f, 1f));
            bloom.active = true;
        }

        static void SetupVignette(VolumeProfile profile)
        {
            if (!profile.TryGet(out Vignette vignette))
                vignette = profile.Add<Vignette>(true);

            vignette.intensity.Override(0.2f);
            vignette.smoothness.Override(0.4f);
            vignette.color.Override(new Color(0f, 0f, 0f, 1f));
            vignette.active = true;
        }

        static void SetupColorAdjustments(VolumeProfile profile)
        {
            if (!profile.TryGet(out ColorAdjustments ca))
                ca = profile.Add<ColorAdjustments>(true);

            ca.saturation.Override(-10f);
            ca.contrast.Override(15f);
            ca.colorFilter.Override(new Color(0.95f, 0.9f, 0.85f, 1f));
            ca.active = true;
        }

        static void SetupWhiteBalance(VolumeProfile profile)
        {
            if (!profile.TryGet(out WhiteBalance wb))
                wb = profile.Add<WhiteBalance>(true);

            wb.temperature.Override(10f);
            wb.tint.Override(-5f);
            wb.active = true;
        }

        static void SetupFilmGrain(VolumeProfile profile)
        {
            if (!profile.TryGet(out FilmGrain fg))
                fg = profile.Add<FilmGrain>(true);

            fg.intensity.Override(0.15f);
            fg.type.Override(FilmGrainLookup.Thin2);
            fg.active = true;
        }

        static void SetupChromaticAberration(VolumeProfile profile)
        {
            if (!profile.TryGet(out ChromaticAberration ca))
                ca = profile.Add<ChromaticAberration>(true);

            ca.intensity.Override(0.05f);
            ca.active = true;
        }
    }
}
