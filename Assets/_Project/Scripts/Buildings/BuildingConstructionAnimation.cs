using System.Collections;
using UnityEngine;

namespace HollowGround.Buildings
{
    public class BuildingConstructionAnimation : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private float _scaleUpDuration = 0.6f;
        [SerializeField] private float _bounceScale = 1.08f;
        [SerializeField] private float _bounceDuration = 0.15f;
        [SerializeField] private AnimationCurve _scaleCurve = new(
            new Keyframe(0f, 0f),
            new Keyframe(0.5f, 1.05f),
            new Keyframe(0.75f, 0.97f),
            new Keyframe(1f, 1f)
        );

        [Header("Construction Phase")]
        [SerializeField] private float _constructPulseSpeed = 2f;
        [SerializeField] private Color _constructColor = new(1f, 0.85f, 0.4f, 1f);
        [SerializeField] private float _constructAlpha = 0.3f;

        [Header("Particles")]
        [SerializeField] private int _dustParticleCount = 20;
        [SerializeField] private float _dustSpread = 2f;
        [SerializeField] private float _dustDuration = 1.5f;
        [SerializeField] private float _dustUpSpeed = 3f;
        [SerializeField] private float _dustSize = 0.15f;

        private Building _building;
        private Renderer[] _renderers;
        private MaterialPropertyBlock _mpb;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            _building = GetComponent<Building>();
            _mpb = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            if (_building != null)
            {
                _building.OnConstructionComplete += OnComplete;
            }
        }

        private void OnDisable()
        {
            if (_building != null)
            {
                _building.OnConstructionComplete -= OnComplete;
            }
        }

        private Coroutine _scaleUpCoroutine;
        private bool _scaleUpComplete;

        private IEnumerator Start()
        {
            _renderers = GetComponentsInChildren<Renderer>();

            _scaleUpComplete = false;
            _scaleUpCoroutine = StartCoroutine(PlayScaleUpAnimation());
            yield return _scaleUpCoroutine;
            _scaleUpComplete = true;
            yield return StartCoroutine(PlayConstructionPulse());
        }

        private IEnumerator PlayScaleUpAnimation()
        {
            transform.localScale = Vector3.zero;
            float elapsed = 0f;

            while (elapsed < _scaleUpDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _scaleUpDuration);
                float curveValue = _scaleCurve.Evaluate(t);
                transform.localScale = Vector3.one * curveValue;
                yield return null;
            }

            transform.localScale = Vector3.one;
            SpawnDustParticles();
        }

        private IEnumerator PlayConstructionPulse()
        {
            if (_renderers == null) yield break;

            while (_building != null && _building.State == BuildingState.Constructing)
            {
                float pulse = (Mathf.Sin(Time.time * _constructPulseSpeed) * 0.5f + 0.5f) * _constructAlpha;

                foreach (var rend in _renderers)
                {
                    rend.GetPropertyBlock(_mpb);
                    Color emission = _constructColor * pulse;
                    _mpb.SetColor(EmissionColor, emission);
                    rend.SetPropertyBlock(_mpb);
                }

                yield return null;
            }

            foreach (var rend in _renderers)
            {
                rend.GetPropertyBlock(_mpb);
                _mpb.SetColor(EmissionColor, Color.black);
                rend.SetPropertyBlock(_mpb);
            }
        }

        private void OnComplete(Building building)
        {
            if (_scaleUpCoroutine != null && !_scaleUpComplete)
            {
                StopCoroutine(_scaleUpCoroutine);
                _scaleUpCoroutine = null;
                _scaleUpComplete = true;
                transform.localScale = Vector3.one;
            }

            StartCoroutine(PlayBounceAnimation());
            SpawnDustParticles();

            if (_renderers != null)
            {
                foreach (var rend in _renderers)
                {
                    rend.GetPropertyBlock(_mpb);
                    _mpb.SetColor(EmissionColor, Color.black);
                    rend.SetPropertyBlock(_mpb);
                }
            }
        }

        private IEnumerator PlayBounceAnimation()
        {
            float elapsed = 0f;

            while (elapsed < _bounceDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _bounceDuration;
                float scale = Mathf.Lerp(1f, _bounceScale, Mathf.Sin(t * Mathf.PI));
                transform.localScale = Vector3.one * scale;
                yield return null;
            }

            transform.localScale = Vector3.one;
        }

        private void SpawnDustParticles()
        {
            GameObject dustGO = new("DustParticles");
            dustGO.transform.position = transform.position;
            dustGO.transform.SetParent(transform);

            ParticleSystem ps = dustGO.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = _dustDuration;
            main.startSpeed = _dustUpSpeed;
            main.startSize = _dustSize;
            main.maxParticles = _dustParticleCount;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new Color(0.6f, 0.55f, 0.45f, 0.7f);

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = _dustSpread;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new(0f, _dustParticleCount)
            });

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new Gradient
            {
                alphaKeys = new GradientAlphaKey[]
                {
                    new(0.8f, 0f),
                    new(0f, 1f)
                },
                colorKeys = new GradientColorKey[]
                {
                    new(new Color(0.6f, 0.55f, 0.45f), 0f),
                    new(new Color(0.5f, 0.45f, 0.35f), 1f)
                }
            };

            Destroy(dustGO, _dustDuration + 0.5f);
        }
    }
}
