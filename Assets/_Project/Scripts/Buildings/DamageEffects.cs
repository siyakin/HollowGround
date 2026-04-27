using UnityEngine;

namespace HollowGround.Buildings
{
    public class DamageEffects : MonoBehaviour
    {
        [SerializeField] private int _fireParticleCount = 15;
        [SerializeField] private float _fireSize = 0.4f;
        [SerializeField] private int _smokeParticleCount = 10;
        [SerializeField] private float _smokeSize = 0.7f;
        [SerializeField] private int _explosionParticleCount = 40;
        [SerializeField] private float _explosionSpeed = 8f;

        private Building _building;
        private GameObject _damageRoot;

        private static Material CreateParticleMaterial(bool additive)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                         ?? Shader.Find("Sprites/Default");
            if (shader == null) return null;

            var mat = new Material(shader);
            mat.SetFloat("_Surface", 1f);
            mat.SetInt("_ZWrite", 0);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            mat.SetColor("_BaseColor", Color.white);

            if (additive)
            {
                mat.SetFloat("_Blend", 2f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            }
            else
            {
                mat.SetFloat("_Blend", 0f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }

            return mat;
        }

        private static void ApplyParticleMaterial(ParticleSystem ps, bool additive)
        {
            var r = ps.GetComponent<ParticleSystemRenderer>();
            if (r == null) return;
            r.renderMode = ParticleSystemRenderMode.Billboard;
            var mat = CreateParticleMaterial(additive);
            if (mat != null) r.material = mat;
        }

        private void Awake()
        {
            _building = GetComponent<Building>();
        }

        private void OnEnable()
        {
            if (_building != null)
            {
                _building.OnDamaged += OnDamaged;
                _building.OnRepaired += OnRepaired;
                _building.OnDestroyed += OnDestroyed;
            }
        }

        private void OnDisable()
        {
            if (_building != null)
            {
                _building.OnDamaged -= OnDamaged;
                _building.OnRepaired -= OnRepaired;
                _building.OnDestroyed -= OnDestroyed;
            }

            StopDamageEffects();
        }

        private void Start()
        {
            if (_building != null && _building.State == BuildingState.Damaged)
                StartDamageEffects();
        }

        private void OnDamaged(Building _) => StartDamageEffects();

        private void OnRepaired(Building _) => StopDamageEffects();

        private void OnDestroyed(Building _)
        {
            StopDamageEffects();
            SpawnExplosion();
        }

        private void StartDamageEffects()
        {
            StopDamageEffects();

            _damageRoot = new GameObject("DamageEffects");
            _damageRoot.transform.SetParent(transform);
            _damageRoot.transform.localPosition = Vector3.zero;

            Vector3 center = Vector3.up * 1.5f;

            Vector3[] fireOffsets = {
                center,
                center + new Vector3(0.3f, 0f, 0.1f),
                center + new Vector3(-0.25f, 0.2f, 0.15f)
            };

            foreach (var offset in fireOffsets)
                CreateFireEmitter(_damageRoot.transform, offset);

            Vector3[] smokeOffsets = {
                center + new Vector3(0.15f, 0.5f, 0f),
                center + new Vector3(-0.2f, 0.3f, 0.2f)
            };

            foreach (var offset in smokeOffsets)
                CreateSmokeEmitter(_damageRoot.transform, offset);
        }

        private void StopDamageEffects()
        {
            if (_damageRoot != null)
            {
                Destroy(_damageRoot);
                _damageRoot = null;
            }
        }

        private void CreateFireEmitter(Transform parent, Vector3 localOffset)
        {
            var go = new GameObject("Fire");
            go.transform.SetParent(parent);
            go.transform.localPosition = localOffset;

            var ps = go.AddComponent<ParticleSystem>();
            ApplyParticleMaterial(ps, additive: true);

            var main = ps.main;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSize = new ParticleSystem.MinMaxCurve(_fireSize * 0.75f, _fireSize * 1.25f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.8f, 0.1f, 0.8f),
                new Color(1f, 0.3f, 0.05f, 0.7f)
            );
            main.maxParticles = _fireParticleCount * 3;
            main.gravityModifier = -0.1f;

            var emission = ps.emission;
            emission.rateOverTime = _fireParticleCount;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 12f;
            shape.radius = 0.1f;

            var velOverLifetime = ps.velocityOverLifetime;
            velOverLifetime.enabled = true;
            velOverLifetime.space = ParticleSystemSimulationSpace.World;
            velOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);
            velOverLifetime.y = new ParticleSystem.MinMaxCurve(0f, 0.5f);
            velOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.2f),
                new Keyframe(0.25f, 1f),
                new Keyframe(0.5f, 0.6f),
                new Keyframe(0.75f, 0.9f),
                new Keyframe(1f, 0f)
            ));

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var fireGrad = new Gradient();
            fireGrad.SetKeys(
                new GradientColorKey[] {
                    new(new Color(1f, 0.95f, 0.3f), 0f),
                    new(new Color(1f, 0.5f, 0.05f), 0.5f),
                    new(new Color(0.4f, 0.05f, 0f), 1f)
                },
                new GradientAlphaKey[] {
                    new(0f, 0f),
                    new(0.7f, 0.1f),
                    new(0.5f, 0.75f),
                    new(0f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(fireGrad);

            ps.Play();
        }

        private void CreateSmokeEmitter(Transform parent, Vector3 localOffset)
        {
            var go = new GameObject("Smoke");
            go.transform.SetParent(parent);
            go.transform.localPosition = localOffset;

            var ps = go.AddComponent<ParticleSystem>();
            ApplyParticleMaterial(ps, additive: false);

            var main = ps.main;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 2f);
            main.startSize = new ParticleSystem.MinMaxCurve(_smokeSize * 0.7f, _smokeSize * 1.3f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.25f, 0.25f, 0.25f, 0.35f),
                new Color(0.15f, 0.15f, 0.15f, 0.25f)
            );
            main.maxParticles = _smokeParticleCount * 6;
            main.gravityModifier = -0.05f;

            var emission = ps.emission;
            emission.rateOverTime = _smokeParticleCount * 0.4f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.Linear(0f, 0.5f, 1f, 2f));

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var smokeGrad = new Gradient();
            smokeGrad.SetKeys(
                new GradientColorKey[] {
                    new(new Color(0.3f, 0.3f, 0.3f), 0f),
                    new(new Color(0.2f, 0.2f, 0.2f), 1f)
                },
                new GradientAlphaKey[] {
                    new(0f, 0f),
                    new(0.3f, 0.1f),
                    new(0.2f, 0.7f),
                    new(0f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(smokeGrad);

            ps.Play();
        }

        private void SpawnExplosion()
        {
            Vector3 worldCenter = transform.position + Vector3.up * 1.5f;
            SpawnDebrisBurst(worldCenter);
            SpawnSmokeCloud(worldCenter);
        }

        private void SpawnDebrisBurst(Vector3 position)
        {
            var go = new GameObject("ExplosionDebris");
            go.transform.position = position;

            var ps = go.AddComponent<ParticleSystem>();
            ApplyParticleMaterial(ps, additive: false);

            var main = ps.main;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 1.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(_explosionSpeed * 0.5f, _explosionSpeed * 1.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.4f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.5f, 0.35f, 0.1f, 1f),
                new Color(0.35f, 0.35f, 0.35f, 1f)
            );
            main.maxParticles = _explosionParticleCount;
            main.gravityModifier = 1.5f;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.22f);

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, _explosionParticleCount) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.4f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var debrisGrad = new Gradient();
            debrisGrad.SetKeys(
                new GradientColorKey[] {
                    new(new Color(1f, 0.6f, 0.1f), 0f),
                    new(new Color(0.4f, 0.3f, 0.2f), 0.3f),
                    new(new Color(0.3f, 0.3f, 0.3f), 1f)
                },
                new GradientAlphaKey[] {
                    new(1f, 0f),
                    new(0.8f, 0.3f),
                    new(0f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(debrisGrad);

            ps.Play();
            Destroy(go, 2.5f);
        }

        private void SpawnSmokeCloud(Vector3 position)
        {
            var go = new GameObject("ExplosionSmoke");
            go.transform.position = position;

            var ps = go.AddComponent<ParticleSystem>();
            ApplyParticleMaterial(ps, additive: false);

            var main = ps.main;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 3.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.3f, 0.3f, 0.3f, 0.5f),
                new Color(0.2f, 0.2f, 0.2f, 0.4f)
            );
            main.maxParticles = 20;
            main.gravityModifier = -0.08f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.Linear(0f, 0.6f, 1f, 1.6f));

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var cloudGrad = new Gradient();
            cloudGrad.SetKeys(
                new GradientColorKey[] {
                    new(new Color(0.4f, 0.3f, 0.2f), 0f),
                    new(new Color(0.25f, 0.25f, 0.25f), 1f)
                },
                new GradientAlphaKey[] {
                    new(0f, 0f),
                    new(0.5f, 0.1f),
                    new(0.3f, 0.6f),
                    new(0f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(cloudGrad);

            ps.Play();
            Destroy(go, 4f);
        }
    }
}
