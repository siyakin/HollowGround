using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HollowGround.Core
{
    public class PostProcessingSetup : MonoBehaviour
    {
        [Header("Bloom")]
        [SerializeField] private bool _enableBloom = true;
        [SerializeField] private float _bloomIntensity = 0.2f;
        [SerializeField] private float _bloomThreshold = 1.5f;
        [SerializeField] private float _bloomScatter = 0.5f;
        [SerializeField] private Color _bloomTint = new(1f, 0.9f, 0.8f, 1f);

        [Header("Vignette")]
        [SerializeField] private bool _enableVignette = true;
        [SerializeField] private float _vignetteIntensity = 0.2f;
        [SerializeField] private float _vignetteSmoothness = 0.4f;
        [SerializeField] private Color _vignetteColor = new(0f, 0f, 0f, 1f);

        [Header("Color Grading")]
        [SerializeField] private bool _enableColorGrading = true;
        [SerializeField] private float _saturation = -10f;
        [SerializeField] private float _contrast = 15f;
        [SerializeField] private float _temperature = 10f;
        [SerializeField] private float _tint = -5f;
        [SerializeField] private Color _colorFilter = new(0.98f, 0.95f, 0.92f, 1f);

        [Header("Film Grain")]
        [SerializeField] private bool _enableFilmGrain = false;
        [SerializeField] private float _filmGrainIntensity = 0.08f;

        [Header("Chromatic Aberration")]
        [SerializeField] private bool _enableChromatic = false;
        [SerializeField] private float _chromaticIntensity = 0.03f;

        private Volume _volume;
        private Bloom _bloom;
        private Vignette _vignette;
        private ColorAdjustments _colorAdjustments;
        private FilmGrain _filmGrain;
        private ChromaticAberration _chromaticAberration;

        private void Start()
        {
            SetupVolume();
        }

        private void SetupVolume()
        {
            _volume = GetComponent<Volume>();
            if (_volume == null)
                _volume = gameObject.AddComponent<Volume>();

            if (_volume.profile == null)
            {
                var profile = UnityEngine.Resources.Load<VolumeProfile>("PostProcessProfile");
                if (profile != null)
                {
                    _volume.profile = profile;
                }
                else
                {
                    _volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
                }
            }

            SetupBloom();
            SetupVignette();
            SetupColorGrading();
            SetupFilmGrain();
            SetupChromaticAberration();
        }

        private void SetupBloom()
        {
            if (!_enableBloom) return;

            if (!_volume.profile.TryGet(out _bloom))
                _bloom = _volume.profile.Add<Bloom>(true);

            _bloom.intensity.Override(_bloomIntensity);
            _bloom.threshold.Override(_bloomThreshold);
            _bloom.scatter.Override(_bloomScatter);
            _bloom.tint.Override(_bloomTint);
            _bloom.active = true;
        }

        private void SetupVignette()
        {
            if (!_enableVignette) return;

            if (!_volume.profile.TryGet(out _vignette))
                _vignette = _volume.profile.Add<Vignette>(true);

            _vignette.intensity.Override(_vignetteIntensity);
            _vignette.smoothness.Override(_vignetteSmoothness);
            _vignette.color.Override(_vignetteColor);
            _vignette.active = true;
        }

        private void SetupColorGrading()
        {
            if (!_enableColorGrading) return;

            if (!_volume.profile.TryGet(out _colorAdjustments))
                _colorAdjustments = _volume.profile.Add<ColorAdjustments>(true);

            _colorAdjustments.saturation.Override(_saturation);
            _colorAdjustments.contrast.Override(_contrast);
            _colorAdjustments.colorFilter.Override(_colorFilter);
            _colorAdjustments.active = true;

            if (!_volume.profile.TryGet(out WhiteBalance whiteBalance))
                whiteBalance = _volume.profile.Add<WhiteBalance>(true);

            whiteBalance.temperature.Override(_temperature);
            whiteBalance.tint.Override(_tint);
            whiteBalance.active = true;
        }

        private void SetupFilmGrain()
        {
            if (!_enableFilmGrain) return;

            if (!_volume.profile.TryGet(out _filmGrain))
                _filmGrain = _volume.profile.Add<FilmGrain>(true);

            _filmGrain.intensity.Override(_filmGrainIntensity);
            _filmGrain.type.Override(FilmGrainLookup.Thin2);
            _filmGrain.active = true;
        }

        private void SetupChromaticAberration()
        {
            if (!_enableChromatic) return;

            if (!_volume.profile.TryGet(out _chromaticAberration))
                _chromaticAberration = _volume.profile.Add<ChromaticAberration>(true);

            _chromaticAberration.intensity.Override(_chromaticIntensity);
            _chromaticAberration.active = true;
        }

        public void SetBloomIntensity(float value)
        {
            if (_bloom != null) _bloom.intensity.value = value;
        }

        public void SetVignetteIntensity(float value)
        {
            if (_vignette != null) _vignette.intensity.value = value;
        }

        public void SetSaturation(float value)
        {
            if (_colorAdjustments != null) _colorAdjustments.saturation.value = value;
        }

        public void SetFilmGrainIntensity(float value)
        {
            if (_filmGrain != null) _filmGrain.intensity.value = value;
        }

        public void SetColorFilter(Color value)
        {
            if (_colorAdjustments != null) _colorAdjustments.colorFilter.value = value;
        }

        public void SetChromaticAberration(float value)
        {
            if (_chromaticAberration == null && _volume != null)
            {
                if (!_volume.profile.TryGet(out _chromaticAberration))
                    _chromaticAberration = _volume.profile.Add<ChromaticAberration>(true);
            }
            if (_chromaticAberration != null)
            {
                _chromaticAberration.intensity.Override(value);
                _chromaticAberration.active = value > 0.001f;
            }
        }
    }
}
