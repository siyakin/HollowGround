using UnityEngine;

namespace HollowGround.Core
{
    public class AtmosphereEffects : MonoBehaviour
    {
        [Header("Ambient Lighting")]
        [SerializeField] private Color _ambientColor = new(0.25f, 0.22f, 0.18f, 1f);
        [SerializeField] private float _ambientIntensity = 0.4f;
        [SerializeField] private Color _fogColor = new(0.18f, 0.16f, 0.14f, 1f);
        [SerializeField] private float _fogDensity = 0.004f;

        [Header("Directional Light")]
        [SerializeField] private Color _sunColor = new(1f, 0.85f, 0.65f, 1f);
        [SerializeField] private float _sunIntensity = 0.8f;
        [SerializeField] private Vector3 _sunRotation = new(50f, -30f, 0f);

        [Header("Dust Particles")]
        [SerializeField] private bool _enableDust = false;
        [SerializeField] private int _dustCount = 80;
        [SerializeField] private float _dustAreaSize = 80f;
        [SerializeField] private float _dustHeight = 15f;
        [SerializeField] private float _dustSpeed = 0.5f;
        [SerializeField] private float _dustParticleSize = 0.08f;
        [SerializeField] private Color _dustColor = new(0.6f, 0.55f, 0.45f, 0.25f);
        [SerializeField] private float _dustLifetime = 8f;

        [Header("Fog Particles")]
        [SerializeField] private bool _enableFogParticles = false;
        [SerializeField] private int _fogParticleCount = 15;
        [SerializeField] private float _fogAreaSize = 100f;
        [SerializeField] private float _fogParticleHeight = 2f;
        [SerializeField] private float _fogSpeed = 0.2f;
        [SerializeField] private float _fogParticleSize = 6f;
        [SerializeField] private Color _fogParticleColor = new(0.35f, 0.30f, 0.25f, 0.08f);

        [Header("Wind")]
        [SerializeField] private Vector3 _windDirection = new(1f, 0f, 0.5f);
        [SerializeField] private float _windStrength = 1f;

        [Header("Day/Night Cycle")]
        [SerializeField] private bool _enableDayNight = false;
        [SerializeField] private float _dayDuration = 600f;
        [SerializeField] private Color _dayAmbientColor = new(0.3f, 0.27f, 0.22f, 1f);
        [SerializeField] private Color _nightAmbientColor = new(0.08f, 0.08f, 0.15f, 1f);
        [SerializeField] private Color _daySunColor = new(1f, 0.85f, 0.65f, 1f);
        [SerializeField] private Color _nightSunColor = new(0.15f, 0.18f, 0.35f, 1f);
        [SerializeField] private float _daySunIntensity = 0.8f;
        [SerializeField] private float _nightSunIntensity = 0.15f;

        private Light _sunLight;
        private ParticleSystem _dustSystem;
        private ParticleSystem _fogSystem;

        private void Start()
        {
            SetupLighting();
            SetupFog();

            if (_enableDust)
                _dustSystem = CreateDustParticles();

            if (_enableFogParticles)
                _fogSystem = CreateFogParticles();
        }

        private void Update()
        {
            UpdateParticlePositions();

            if (_enableDayNight)
                UpdateDayNight();
        }

        private void SetupLighting()
        {
            if (RenderSettings.ambientLight.grayscale < 0.01f)
            {
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = _ambientColor;
                RenderSettings.ambientIntensity = _ambientIntensity;
            }

            if (!RenderSettings.fog)
            {
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Exponential;
                RenderSettings.fogColor = _fogColor;
                RenderSettings.fogDensity = _fogDensity;
            }

            _sunLight = FindAnyObjectByType<Light>();
            if (_sunLight != null && _sunLight.type == LightType.Directional)
                return;

            _sunLight = GetComponentInChildren<Light>();
            if (_sunLight == null)
            {
                GameObject sunGO = new("Directional Light");
                sunGO.transform.SetParent(transform);
                _sunLight = sunGO.AddComponent<Light>();
                _sunLight.type = LightType.Directional;
            }

            _sunLight.color = _sunColor;
            _sunLight.intensity = _sunIntensity;
            _sunLight.shadows = LightShadows.Soft;
            _sunLight.shadowStrength = 0.6f;
            _sunLight.transform.rotation = Quaternion.Euler(_sunRotation);
        }

        private void SetupFog()
        {
            if (RenderSettings.fog) return;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = _fogColor;
            RenderSettings.fogDensity = _fogDensity;
        }

        private ParticleSystem CreateDustParticles()
        {
            GameObject dustGO = new("AtmosphereDust");
            dustGO.transform.SetParent(transform);
            dustGO.transform.position = transform.position;

            ParticleSystem ps = dustGO.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.maxParticles = _dustCount;
            main.startLifetime = _dustLifetime;
            main.startSpeed = 0f;
            main.startSize = _dustParticleSize;
            main.startColor = _dustColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.01f;

            var emission = ps.emission;
            emission.rateOverTime = _dustCount / _dustLifetime;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(_dustAreaSize, _dustHeight, _dustAreaSize);

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            Vector3 windDir = _windDirection.normalized * _windStrength * _dustSpeed;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(windDir.x, windDir.x * 1.5f);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-0.1f, 0.2f);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(windDir.z, windDir.z * 1.5f);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new Gradient
            {
                alphaKeys = new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.3f, 0.3f),
                    new GradientAlphaKey(0.3f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                },
                colorKeys = new[]
                {
                    new GradientColorKey(_dustColor, 0f),
                    new GradientColorKey(_dustColor, 1f)
                }
            };

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1.2f));

            return ps;
        }

        private ParticleSystem CreateFogParticles()
        {
            GameObject fogGO = new("AtmosphereFog");
            fogGO.transform.SetParent(transform);
            fogGO.transform.position = transform.position;

            ParticleSystem ps = fogGO.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.maxParticles = _fogParticleCount;
            main.startLifetime = 15f;
            main.startSpeed = 0f;
            main.startSize = _fogParticleSize;
            main.startColor = _fogParticleColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0f;

            var emission = ps.emission;
            emission.rateOverTime = _fogParticleCount / 15f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(_fogAreaSize, _fogParticleHeight, _fogAreaSize);

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            Vector3 fogDir = _windDirection.normalized * _windStrength * _fogSpeed;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(fogDir.x * 0.5f, fogDir.x);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(fogDir.z * 0.5f, fogDir.z);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new Gradient
            {
                alphaKeys = new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(_fogParticleColor.a, 0.3f),
                    new GradientAlphaKey(_fogParticleColor.a, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                },
                colorKeys = new[]
                {
                    new GradientColorKey(_fogParticleColor, 0f),
                    new GradientColorKey(_fogParticleColor, 1f)
                }
            };

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                new AnimationCurve(
                    new Keyframe(0f, 0.5f),
                    new Keyframe(0.3f, 1f),
                    new Keyframe(0.7f, 1f),
                    new Keyframe(1f, 0.6f)
                ));

            return ps;
        }

        private void UpdateParticlePositions()
        {
            if (UnityEngine.Camera.main == null) return;
            Vector3 camPos = UnityEngine.Camera.main.transform.position;

            if (_dustSystem != null)
            {
                var dustShape = _dustSystem.shape;
                _dustSystem.transform.position = new Vector3(
                    camPos.x, camPos.y * 0.5f, camPos.z);
            }

            if (_fogSystem != null)
            {
                _fogSystem.transform.position = new Vector3(
                    camPos.x, 0f, camPos.z);
            }
        }

        private void UpdateDayNight()
        {
            float cycle = (Time.time % _dayDuration) / _dayDuration;
            float dayFactor = (1f - Mathf.Cos(cycle * 2f * Mathf.PI)) * 0.5f;

            RenderSettings.ambientLight = Color.Lerp(_nightAmbientColor, _dayAmbientColor, dayFactor);

            if (_sunLight != null)
            {
                _sunLight.color = Color.Lerp(_nightSunColor, _daySunColor, dayFactor);
                _sunLight.intensity = Mathf.Lerp(_nightSunIntensity, _daySunIntensity, dayFactor);
            }

            float sunAngle = cycle * 360f - 90f;
            if (_sunLight != null)
            {
                _sunLight.transform.rotation = Quaternion.Euler(sunAngle, -30f, 0f);
            }
        }
    }
}
