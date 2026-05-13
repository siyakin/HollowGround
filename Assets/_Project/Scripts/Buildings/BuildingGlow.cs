using UnityEngine;

namespace HollowGround.Buildings
{
    public class BuildingGlow : MonoBehaviour
    {
        [SerializeField] private Color _glowColor = new Color(1f, 0.75f, 0.35f, 1f);
        [SerializeField] private float _glowIntensity = 0.5f;
        [SerializeField] private float _glowRange = 5f;
        [SerializeField] private float _fadeSpeed = 2f;

        private Light _glowLight;
        private Building _building;
        private float _targetIntensity;

        private void Awake()
        {
            _building = GetComponent<Building>();
        }

        private void Start()
        {
            CreateGlowLight();
        }

        private void CreateGlowLight()
        {
            GameObject lightGO = new("BuildingGlow");
            lightGO.transform.SetParent(transform);
            lightGO.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            _glowLight = lightGO.AddComponent<Light>();
            _glowLight.type = LightType.Point;
            _glowLight.color = _glowColor;
            _glowLight.intensity = 0f;
            _glowLight.range = _glowRange;
            _glowLight.shadows = LightShadows.None;
            _glowLight.renderMode = LightRenderMode.Auto;
            _glowLight.innerSpotAngle = 180f;
            _glowLight.spotAngle = 180f;

            UpdateGlowState();
        }

        private void Update()
        {
            if (_glowLight == null) return;

            if (_glowLight.intensity != _targetIntensity)
            {
                _glowLight.intensity = Mathf.MoveTowards(
                    _glowLight.intensity, _targetIntensity, _fadeSpeed * Time.deltaTime);
            }
        }

        private void OnEnable()
        {
            if (_building != null)
                _building.OnStateChanged += OnBuildingStateChanged;
        }

        private void OnDisable()
        {
            if (_building != null)
                _building.OnStateChanged -= OnBuildingStateChanged;
        }

        private void OnBuildingStateChanged(Building building, BuildingState state)
        {
            UpdateGlowState();
        }

        private void UpdateGlowState()
        {
            if (_building == null) return;

            _targetIntensity = _building.State == BuildingState.Active ? _glowIntensity : 0f;
        }
    }
}
