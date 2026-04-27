using System.Collections.Generic;
using HollowGround.Buildings;
using HollowGround.Core;
using UnityEngine;

namespace HollowGround.Camera
{
    public class ScreenShake : Singleton<ScreenShake>
    {
        [SerializeField] private float _defaultIntensity = 0.15f;
        [SerializeField] private float _defaultDuration = 0.4f;
        [SerializeField] private float _defaultFrequency = 20f;
        [SerializeField] private AnimationCurve _decayCurve;

        private float _shakeIntensity;
        private float _shakeDuration;
        private float _shakeFrequency;
        private float _shakeTimer;
        private bool _isShaking;

        private readonly HashSet<Building> _subscribedBuildings = new();

        protected override void Awake()
        {
            base.Awake();

            if (_decayCurve == null || _decayCurve.length == 0)
            {
                _decayCurve = new AnimationCurve(
                    new Keyframe(0f, 1f, 0f, -3f),
                    new Keyframe(1f, 0f, -0.5f, 0f)
                );
            }
        }

        private void Start()
        {
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.OnBuildingAdded += OnBuildingAdded;
                foreach (var building in BuildingManager.Instance.AllBuildings)
                    SubscribeToBuilding(building);
            }
        }

        protected override void OnDestroy()
        {
            if (BuildingManager.Instance != null)
                BuildingManager.Instance.OnBuildingAdded -= OnBuildingAdded;

            foreach (var building in _subscribedBuildings)
            {
                if (building != null)
                {
                    building.OnDamaged -= OnAnyBuildingDamaged;
                    building.OnDestroyed -= OnAnyBuildingDestroyed;
                }
            }
            _subscribedBuildings.Clear();

            base.OnDestroy();
        }

        private void LateUpdate()
        {
            if (!_isShaking) return;

            _shakeTimer += Time.deltaTime;
            if (_shakeTimer >= _shakeDuration)
            {
                _isShaking = false;
                return;
            }

            float progress = _shakeTimer / _shakeDuration;
            float decay = _decayCurve.Evaluate(progress);
            float freq = _shakeFrequency;

            float x = (Mathf.PerlinNoise(_shakeTimer * freq, 0.5f) * 2f - 1f) * _shakeIntensity * decay;
            float z = (Mathf.PerlinNoise(0.5f, _shakeTimer * freq) * 2f - 1f) * _shakeIntensity * decay;

            transform.position += new Vector3(x, 0f, z);
        }

        public void Shake(float intensity, float duration, float frequency)
        {
            _shakeIntensity = intensity;
            _shakeDuration = duration;
            _shakeFrequency = frequency;
            _shakeTimer = 0f;
            _isShaking = true;
        }

        public void TriggerShake(float intensity, float duration, float frequency)
        {
            if (!_isShaking || intensity >= _shakeIntensity)
                Shake(intensity, duration, frequency);
        }

        public void TriggerShake() =>
            TriggerShake(_defaultIntensity, _defaultDuration, _defaultFrequency);

        private void OnBuildingAdded(Building building) => SubscribeToBuilding(building);

        private void SubscribeToBuilding(Building building)
        {
            if (building == null || _subscribedBuildings.Contains(building)) return;
            building.OnDamaged += OnAnyBuildingDamaged;
            building.OnDestroyed += OnAnyBuildingDestroyed;
            _subscribedBuildings.Add(building);
        }

        private void OnAnyBuildingDamaged(Building _) => TriggerShake(0.1f, 0.3f, 15f);

        private void OnAnyBuildingDestroyed(Building building)
        {
            TriggerShake(0.3f, 0.5f, 25f);
            _subscribedBuildings.Remove(building);
            building.OnDamaged -= OnAnyBuildingDamaged;
            building.OnDestroyed -= OnAnyBuildingDestroyed;
        }
    }
}
