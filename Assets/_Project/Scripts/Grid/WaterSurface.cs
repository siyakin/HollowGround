using System.Collections.Generic;
using HollowGround.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace HollowGround.Grid
{
    public class WaterSurface : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _waterY = -0.1f;
        [SerializeField] private Material _fallbackMaterial;

        private MapRenderer _mapRenderer;
        private Material _sharedMaterial;
        private Material _bodyMaterial;
        private readonly List<GameObject> _waterObjects = new();
        private bool _materialDirty = true;
        private bool _useFancyWater;

        private void Awake()
        {
            _mapRenderer = GetComponentInParent<MapRenderer>();
            ApplyGameConfig();
        }

        private void ApplyGameConfig()
        {
            var config = GameConfig.Instance;
            if (config == null) return;
            _useFancyWater = config.EnableFancyWater;
            if (_useFancyWater)
                EnsureMaterial();
            _materialDirty = true;
        }

        private void EnsureMaterial()
        {
            if (_sharedMaterial != null) return;

            var shader = Shader.Find("HollowGround/Water");
            if (shader == null)
            {
                Debug.LogError("[WaterSurface] Shader 'HollowGround/Water' not found.");
                return;
            }

            _sharedMaterial = new Material(shader) { name = "Water_Shared" };
            _bodyMaterial = new Material(shader) { name = "WaterBody_Shared", renderQueue = 2999 };
            _materialDirty = true;
            UpdateWaterMaterials();
        }

        public GameObject CreateWaterTile(Vector3 worldPos, float cellSize, int gridX, int gridZ)
        {
            Material mat;
            if (_useFancyWater)
            {
                EnsureMaterial();
                if (_sharedMaterial == null) return null;
                mat = _sharedMaterial;
            }
            else
            {
                mat = _fallbackMaterial;
                if (mat == null)
                {
                    mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mat.color = new Color(0.1f, 0.3f, 0.6f, 0.85f);
                    mat.SetFloat("_Surface", 1);
                    mat.renderQueue = 3000;
                    _fallbackMaterial = mat;
                }
            }

            var go = new GameObject($"Water_{gridX}_{gridZ}");
            go.transform.position = new Vector3(worldPos.x, _waterY, worldPos.z);

            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = MapRenderer.CreateQuadMesh(cellSize);

            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = ShadowCastingMode.Off;

            var tile = go.AddComponent<TerrainTile>();
            tile.TerrainType = TerrainType.Water;
            tile.GridX = gridX;
            tile.GridZ = gridZ;

            go.transform.SetParent(GetTerrainRoot());
            _waterObjects.Add(go);
            return go;
        }

        public GameObject CreateWaterBody(Vector3 center, float width, float depth, float height = 0.5f)
        {
            if (!_useFancyWater) return null;
            EnsureMaterial();
            if (_bodyMaterial == null) return null;

            var go = new GameObject("WaterBody");
            go.transform.position = center;

            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = BuildBodyMesh(width, height, depth);

            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = _bodyMaterial;
            mr.shadowCastingMode = ShadowCastingMode.Off;

            go.transform.SetParent(GetTerrainRoot());
            _waterObjects.Add(go);
            return go;
        }

        public void UpdateWaterMaterials()
        {
            if (!_materialDirty) return;

            var config = GameConfig.Instance;

            if (_useFancyWater && _sharedMaterial != null)
            {
                float waveSpeed = config != null ? config.WaterWaveSpeed : 0.15f;
                float waveHeight = config != null ? config.WaterWaveHeight : 0.02f;
                float foamAmount = config != null ? config.WaterFoamAmount : 0.25f;
                float normalStr = config != null ? config.WaterNormalStrength : 0.3f;
                float fresnel = config != null ? config.WaterFresnelPower : 2.5f;
                float depthFactor = config != null ? config.WaterDepthFactor : 1.5f;
                float refrStr = config != null ? config.WaterRefractionStrength : 0.01f;
                float opacity = config != null ? config.WaterOpacity : 0.85f;
                Color baseCol = config != null ? config.WaterBaseColor : new Color(0.04f, 0.12f, 0.30f, 0.90f);
                Color shallowCol = config != null ? config.WaterShallowColor : new Color(0.10f, 0.50f, 0.60f, 0.60f);
                Color foamCol = config != null ? config.WaterFoamColor : new Color(0.85f, 0.92f, 0.95f, 1.00f);

                _sharedMaterial.SetColor("_BaseColor", baseCol);
                _sharedMaterial.SetColor("_ShallowColor", shallowCol);
                _sharedMaterial.SetColor("_FoamColor", foamCol);
                _sharedMaterial.SetFloat("_DepthFactor", depthFactor);
                _sharedMaterial.SetFloat("_WaveSpeed", waveSpeed);
                _sharedMaterial.SetFloat("_WaveHeight", waveHeight);
                _sharedMaterial.SetFloat("_FoamAmount", foamAmount);
                _sharedMaterial.SetFloat("_FresnelPower", fresnel);
                _sharedMaterial.SetFloat("_NormalStrength", normalStr);
                _sharedMaterial.SetFloat("_Opacity", opacity);
                _sharedMaterial.SetFloat("_RefractionStrength", refrStr);

                if (_bodyMaterial != null)
                {
                    var darkerBase = new Color(baseCol.r * 0.6f, baseCol.g * 0.6f, baseCol.b * 0.6f, baseCol.a);
                    var darkerShallow = new Color(shallowCol.r * 0.6f, shallowCol.g * 0.6f, shallowCol.b * 0.6f, shallowCol.a);

                    _bodyMaterial.SetColor("_BaseColor", darkerBase);
                    _bodyMaterial.SetColor("_ShallowColor", darkerShallow);
                    _bodyMaterial.SetColor("_FoamColor", foamCol);
                    _bodyMaterial.SetFloat("_DepthFactor", depthFactor);
                    _bodyMaterial.SetFloat("_WaveSpeed", 0f);
                    _bodyMaterial.SetFloat("_WaveHeight", 0f);
                    _bodyMaterial.SetFloat("_FoamAmount", 0f);
                    _bodyMaterial.SetFloat("_FresnelPower", fresnel);
                    _bodyMaterial.SetFloat("_NormalStrength", 0f);
                    _bodyMaterial.SetFloat("_Opacity", 1f);
                    _bodyMaterial.SetFloat("_RefractionStrength", 0f);
                }
            }

            _materialDirty = false;
        }

        public Material GetWaterMaterial() => _sharedMaterial;

        public void MarkDirty() => _materialDirty = true;

        public void ClearWater()
        {
            foreach (var obj in _waterObjects)
            {
                if (obj != null)
                    DestroyImmediate(obj);
            }
            _waterObjects.Clear();
        }

        private void OnValidate()
        {
            _materialDirty = true;
            ApplyGameConfig();
            if (_sharedMaterial != null)
                UpdateWaterMaterials();
        }

        private Transform GetTerrainRoot()
        {
            if (_mapRenderer == null) return transform;
            var root = _mapRenderer.transform.Find("TerrainTiles");
            return root != null ? root : _mapRenderer.transform;
        }

        private static Mesh BuildBodyMesh(float width, float height, float depth)
        {
            float hw = width * 0.5f;
            float hd = depth * 0.5f;
            var mesh = new Mesh
            {
                vertices = new Vector3[]
                {
                    new(-hw,       0, -hd), new(hw,       0, -hd),
                    new(hw,        0,  hd), new(-hw,      0,  hd),
                    new(-hw, -height, -hd), new(hw, -height, -hd),
                    new(hw,  -height,  hd), new(-hw, -height, hd)
                },
                triangles = new int[]
                {
                    0,2,1, 0,3,2,
                    4,5,6, 4,6,7,
                    0,1,5, 0,5,4,
                    2,3,7, 2,7,6,
                    1,2,6, 1,6,5,
                    3,0,4, 3,4,7
                },
                uv = new Vector2[]
                {
                    new(0,0), new(1,0), new(1,1), new(0,1),
                    new(0,0), new(1,0), new(1,1), new(0,1)
                }
            };
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
