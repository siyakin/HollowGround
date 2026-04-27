using System;
using UnityEngine;

namespace HollowGround.Core
{
    public enum WeatherState
    {
        Clear,
        LightRain,
        HeavyRain,
        DustStorm,
        RadiationStorm
    }

    public class WeatherSystem : Singleton<WeatherSystem>
    {
        [Header("Weather Timing")]
        [SerializeField] private float _weatherChangeIntervalMin = 60f;
        [SerializeField] private float _weatherChangeIntervalMax = 180f;
        [SerializeField] private float _transitionDuration = 5f;
        [SerializeField] private bool _weatherEnabled = true;

        [Header("References")]
        [SerializeField] private PostProcessingSetup _postProcessing;

        public event Action<WeatherState> OnWeatherChanged;
        public event Action OnRadiationStormStart;
        public event Action OnRadiationStormEnd;

        public WeatherState CurrentWeather { get; private set; } = WeatherState.Clear;

        private WeatherState _previousWeather = WeatherState.Clear;
        private WeatherState _targetWeather = WeatherState.Clear;
        private float _transitionTimer;
        private bool _inTransition;
        private float _nextWeatherChangeTime;

        private ParticleSystem _rainSystem;
        private ParticleSystem _splashSystem;
        private ParticleSystem _dustStormSystem;
        private ParticleSystem _radiationSystem;

        private static readonly float[] _weatherWeights = { 0.4f, 0.2f, 0.1f, 0.2f, 0.1f };

        private readonly Color _baseFogColor = new(0.18f, 0.16f, 0.14f, 1f);
        private readonly Color _baseAmbientColor = new(0.25f, 0.22f, 0.18f, 1f);
        private readonly Color _baseColorFilter = new(0.95f, 0.9f, 0.85f, 1f);
        private const float _baseFogDensity = 0.004f;
        private const float _baseVignette = 0.2f;
        private const float _baseSaturation = -10f;
        private const float _baseBloom = 0.2f;

        protected override void Awake()
        {
            base.Awake();
            if (_postProcessing == null)
                _postProcessing = FindAnyObjectByType<PostProcessingSetup>();
        }

        private void Start()
        {
            _rainSystem = CreateRainParticles();
            _splashSystem = CreateSplashParticles();
            _dustStormSystem = CreateDustStormParticles();
            _radiationSystem = CreateRadiationParticles();

            StopAllWeatherParticles();
            ScheduleNextWeatherChange();
        }

        private void Update()
        {
            if (!_weatherEnabled) return;

            if (!_inTransition && Time.time >= _nextWeatherChangeTime)
                BeginWeatherTransition(SelectNextWeather());

            if (_inTransition)
                UpdateTransition();

            UpdateParticlePositions();
        }

        private WeatherState SelectNextWeather()
        {
            float roll = UnityEngine.Random.value;
            float cumulative = 0f;
            for (int i = 0; i < _weatherWeights.Length; i++)
            {
                cumulative += _weatherWeights[i];
                if (roll <= cumulative)
                    return (WeatherState)i;
            }
            return WeatherState.Clear;
        }

        private void BeginWeatherTransition(WeatherState target)
        {
            if (target == CurrentWeather)
            {
                ScheduleNextWeatherChange();
                return;
            }

            _previousWeather = CurrentWeather;
            _targetWeather = target;
            _transitionTimer = 0f;
            _inTransition = true;

            StopAllWeatherParticles();
            StartWeatherParticles(target);

            if (_previousWeather == WeatherState.RadiationStorm)
                OnRadiationStormEnd?.Invoke();
            if (target == WeatherState.RadiationStorm)
                OnRadiationStormStart?.Invoke();
        }

        private void UpdateTransition()
        {
            _transitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_transitionTimer / _transitionDuration);

            LerpWeatherEffects(_previousWeather, _targetWeather, t);

            if (t >= 1f)
            {
                _inTransition = false;
                CurrentWeather = _targetWeather;
                OnWeatherChanged?.Invoke(CurrentWeather);
                ScheduleNextWeatherChange();
            }
        }

        private void LerpWeatherEffects(WeatherState from, WeatherState to, float t)
        {
            if (_postProcessing != null)
            {
                _postProcessing.SetVignetteIntensity(
                    Mathf.Lerp(GetVignetteForState(from), GetVignetteForState(to), t));
                _postProcessing.SetSaturation(
                    Mathf.Lerp(GetSaturationForState(from), GetSaturationForState(to), t));
                _postProcessing.SetBloomIntensity(
                    Mathf.Lerp(GetBloomForState(from), GetBloomForState(to), t));
                _postProcessing.SetColorFilter(
                    Color.Lerp(GetColorFilterForState(from), GetColorFilterForState(to), t));
                _postProcessing.SetChromaticAberration(
                    Mathf.Lerp(GetChromaticForState(from), GetChromaticForState(to), t));
            }

            RenderSettings.fogColor = Color.Lerp(
                GetFogColorForState(from), GetFogColorForState(to), t);
            RenderSettings.fogDensity = Mathf.Lerp(
                GetFogDensityForState(from), GetFogDensityForState(to), t);
            RenderSettings.ambientLight = Color.Lerp(
                GetAmbientColorForState(from), GetAmbientColorForState(to), t);
        }

        private float GetVignetteForState(WeatherState state) => state switch
        {
            WeatherState.LightRain => _baseVignette + 0.05f,
            WeatherState.HeavyRain => _baseVignette + 0.15f,
            WeatherState.RadiationStorm => _baseVignette + 0.2f,
            _ => _baseVignette
        };

        private float GetSaturationForState(WeatherState state) => state switch
        {
            WeatherState.LightRain => _baseSaturation - 5f,
            WeatherState.HeavyRain => _baseSaturation - 15f,
            WeatherState.DustStorm => _baseSaturation - 20f,
            _ => _baseSaturation
        };

        private float GetBloomForState(WeatherState state) => state switch
        {
            WeatherState.HeavyRain => _baseBloom - 0.1f,
            _ => _baseBloom
        };

        private Color GetColorFilterForState(WeatherState state) => state switch
        {
            WeatherState.DustStorm => new Color(1.0f, 0.85f, 0.7f, 1f),
            WeatherState.RadiationStorm => new Color(0.85f, 1.0f, 0.8f, 1f),
            _ => _baseColorFilter
        };

        private float GetChromaticForState(WeatherState state) => state switch
        {
            WeatherState.RadiationStorm => 0.15f,
            _ => 0f
        };

        private Color GetFogColorForState(WeatherState state) => state switch
        {
            WeatherState.DustStorm => new Color(0.25f, 0.2f, 0.15f, 1f),
            WeatherState.RadiationStorm => new Color(0.15f, 0.2f, 0.12f, 1f),
            _ => _baseFogColor
        };

        private float GetFogDensityForState(WeatherState state) => state switch
        {
            WeatherState.LightRain => 0.006f,
            WeatherState.HeavyRain => 0.008f,
            WeatherState.DustStorm => 0.012f,
            WeatherState.RadiationStorm => 0.01f,
            _ => _baseFogDensity
        };

        private Color GetAmbientColorForState(WeatherState state) => state switch
        {
            WeatherState.HeavyRain => new Color(
                _baseAmbientColor.r * 0.8f, _baseAmbientColor.g * 0.8f, _baseAmbientColor.b * 0.8f),
            WeatherState.DustStorm => new Color(
                _baseAmbientColor.r * 0.7f, _baseAmbientColor.g * 0.7f, _baseAmbientColor.b * 0.7f),
            WeatherState.RadiationStorm => new Color(0.15f, 0.2f, 0.12f, 1f),
            _ => _baseAmbientColor
        };

        private void StartWeatherParticles(WeatherState state)
        {
            if (state == WeatherState.LightRain || state == WeatherState.HeavyRain)
            {
                ConfigureRainForState(state);
                SetParticleActive(_rainSystem, true);
            }
            if (state == WeatherState.HeavyRain)
                SetParticleActive(_splashSystem, true);
            if (state == WeatherState.DustStorm)
                SetParticleActive(_dustStormSystem, true);
            if (state == WeatherState.RadiationStorm)
                SetParticleActive(_radiationSystem, true);
        }

        private void ConfigureRainForState(WeatherState state)
        {
            if (_rainSystem == null) return;
            bool heavy = state == WeatherState.HeavyRain;

            var main = _rainSystem.main;
            main.maxParticles = heavy ? 500 : 200;
            main.startSpeed = new ParticleSystem.MinMaxCurve(heavy ? 22f : 13f, heavy ? 28f : 17f);
            main.startSize = new ParticleSystem.MinMaxCurve(heavy ? 0.03f : 0.02f);

            var emission = _rainSystem.emission;
            emission.rateOverTime = heavy ? 250f : 100f;
        }

        private void StopAllWeatherParticles()
        {
            SetParticleActive(_rainSystem, false);
            SetParticleActive(_splashSystem, false);
            SetParticleActive(_dustStormSystem, false);
            SetParticleActive(_radiationSystem, false);
        }

        private void SetParticleActive(ParticleSystem ps, bool active)
        {
            if (ps == null) return;
            if (active && !ps.isPlaying) ps.Play();
            else if (!active) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void UpdateParticlePositions()
        {
            if (UnityEngine.Camera.main == null) return;
            Vector3 camPos = UnityEngine.Camera.main.transform.position;

            if (_rainSystem != null)
                _rainSystem.transform.position = new Vector3(camPos.x, camPos.y + 15f, camPos.z);
            if (_splashSystem != null)
                _splashSystem.transform.position = new Vector3(camPos.x, 0.1f, camPos.z);
            if (_dustStormSystem != null)
                _dustStormSystem.transform.position = new Vector3(camPos.x, camPos.y, camPos.z);
            if (_radiationSystem != null)
                _radiationSystem.transform.position = new Vector3(camPos.x, camPos.y + 15f, camPos.z);
        }

        private void ScheduleNextWeatherChange()
        {
            _nextWeatherChangeTime = Time.time + UnityEngine.Random.Range(
                _weatherChangeIntervalMin, _weatherChangeIntervalMax);
        }

        public void ForceWeather(WeatherState state)
        {
            BeginWeatherTransition(state);
        }

        public void SetWeatherEnabled(bool enabled)
        {
            _weatherEnabled = enabled;
            if (!enabled)
            {
                StopAllWeatherParticles();
                LerpWeatherEffects(WeatherState.Clear, WeatherState.Clear, 1f);
            }
        }

        private static void ApplyURPParticleMaterial(ParticleSystem ps)
        {
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer == null) return;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            if (mat == null) mat = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material = mat;
        }

        private ParticleSystem CreateRainParticles()
        {
            GameObject go = new("WeatherRain");
            go.transform.SetParent(transform);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ApplyURPParticleMaterial(ps);
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.maxParticles = 500;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(13f, 17f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f);
            main.startColor = new Color(0.85f, 0.92f, 1f, 0.55f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0f;

            var emission = ps.emission;
            emission.rateOverTime = 100f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(60f, 1f, 60f);

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
            velocity.y = new ParticleSystem.MinMaxCurve(-17f, -13f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.08f;
            renderer.lengthScale = 1.8f;

            return ps;
        }

        private ParticleSystem CreateSplashParticles()
        {
            GameObject go = new("WeatherSplash");
            go.transform.SetParent(transform);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ApplyURPParticleMaterial(ps);
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.maxParticles = 200;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
            main.startColor = new Color(0.85f, 0.92f, 1f, 0.45f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.5f;

            var emission = ps.emission;
            emission.rateOverTime = 80f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(60f, 0.1f, 60f);

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.y = new ParticleSystem.MinMaxCurve(1f, 3f);

            return ps;
        }

        private ParticleSystem CreateDustStormParticles()
        {
            GameObject go = new("WeatherDustStorm");
            go.transform.SetParent(transform);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ApplyURPParticleMaterial(ps);
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.maxParticles = 300;
            main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 8f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
            main.startColor = new Color(0.5f, 0.4f, 0.3f, 0.4f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.01f;

            var emission = ps.emission;
            emission.rateOverTime = 50f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(80f, 10f, 80f);

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(5f, 10f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.3f, 0.5f);
            velocity.z = new ParticleSystem.MinMaxCurve(1f, 4f);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new Gradient
            {
                alphaKeys = new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.4f, 0.25f),
                    new GradientAlphaKey(0.4f, 0.75f),
                    new GradientAlphaKey(0f, 1f)
                },
                colorKeys = new[]
                {
                    new GradientColorKey(new Color(0.5f, 0.4f, 0.3f), 0f),
                    new GradientColorKey(new Color(0.5f, 0.4f, 0.3f), 1f)
                }
            };

            return ps;
        }

        private ParticleSystem CreateRadiationParticles()
        {
            GameObject go = new("WeatherRadiation");
            go.transform.SetParent(transform);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ApplyURPParticleMaterial(ps);
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.maxParticles = 150;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.06f);
            main.startColor = new Color(0.3f, 0.8f, 0.2f, 0.3f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0f;

            var emission = ps.emission;
            emission.rateOverTime = 50f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(60f, 1f, 60f);

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
            velocity.y = new ParticleSystem.MinMaxCurve(-5f, -3f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new Gradient
            {
                alphaKeys = new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.3f, 0.2f),
                    new GradientAlphaKey(0.3f, 0.8f),
                    new GradientAlphaKey(0f, 1f)
                },
                colorKeys = new[]
                {
                    new GradientColorKey(new Color(0.3f, 0.9f, 0.2f), 0f),
                    new GradientColorKey(new Color(0.2f, 0.7f, 0.15f), 1f)
                }
            };

            return ps;
        }
    }
}
