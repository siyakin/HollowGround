using System.Collections.Generic;
using HollowGround.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace HollowGround.Grid
{
    [ExecuteAlways]
    public class WaterSurface : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _waterY = -0.1f;
        [SerializeField] private Shader _waterShader;
        [SerializeField] private Material _fallbackMaterial;

        private MapRenderer _mapRenderer;
        private Material _sharedMaterial;
        private Material _bodyMaterial;
        private readonly List<GameObject> _waterObjects = new();
        private bool _prevFancy = true;

        private void Awake()
        {
            _mapRenderer = GetComponentInParent<MapRenderer>();
        }

        private void Update()
        {
            var config = GameConfig.Instance;
            if (config == null) return;

            bool fancy = config.EnableFancyWater;
            bool materialRecreated = false;

            if (fancy)
            {
                if (_sharedMaterial == null)
                    materialRecreated = true;
                EnsureMaterial();
                if (_sharedMaterial != null)
                    ApplyMaterialProperties(config);
            }

            if (fancy != _prevFancy || materialRecreated)
            {
                _prevFancy = fancy;
                SwapMaterialsOnTiles(fancy);
            }
        }

        private void ApplyMaterialProperties(GameConfig config)
        {
            float waveSpeed = config.WaterWaveSpeed;
            float waveHeight = config.WaterWaveHeight;
            float foamAmount = config.WaterFoamAmount;
            float normalStr = config.WaterNormalStrength;
            float fresnel = config.WaterFresnelPower;
            float depthFactor = config.WaterDepthFactor;
            float refrStr = config.WaterRefractionStrength;
            float opacity = config.WaterOpacity;
            Color baseCol = config.WaterBaseColor;
            Color shallowCol = config.WaterShallowColor;
            Color foamCol = config.WaterFoamColor;

            SetKeyword(_sharedMaterial, "_HG_WATER_WAVES", config.EnableWaterWaves);
            SetKeyword(_sharedMaterial, "_HG_WATER_FOAM", config.EnableWaterFoam);
            SetKeyword(_sharedMaterial, "_HG_WATER_DEPTH", config.EnableWaterDepthEffects);

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
                SetKeyword(_bodyMaterial, "_HG_WATER_WAVES", false);
                SetKeyword(_bodyMaterial, "_HG_WATER_FOAM", false);
                SetKeyword(_bodyMaterial, "_HG_WATER_DEPTH", config.EnableWaterDepthEffects);

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

        private void SwapMaterialsOnTiles(bool fancy)
        {
            Material target = fancy ? _sharedMaterial : GetFallbackMaterial();
            if (target == null) return;

            if (_waterObjects.Count == 0)
                RefreshWaterObjectList();

            foreach (var obj in _waterObjects)
            {
                if (obj == null) continue;
                var mr = obj.GetComponent<MeshRenderer>();
                if (mr != null)
                    mr.sharedMaterial = target;
            }
        }

        private void RefreshWaterObjectList()
        {
            _waterObjects.Clear();
            var root = GetTerrainRoot();
            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == "WaterBody")
                {
                    _waterObjects.Add(child.gameObject);
                    continue;
                }
                var tile = child.GetComponent<TerrainTile>();
                if (tile != null && tile.TerrainType.IsWater())
                    _waterObjects.Add(child.gameObject);
            }
        }

        private void EnsureMaterial()
        {
            if (_sharedMaterial != null) return;

            var shader = _waterShader != null ? _waterShader : Shader.Find("HollowGround/Water");
            if (shader == null)
            {
                Debug.LogError("[WaterSurface] Shader 'HollowGround/Water' not found. Assign it in the Inspector.");
                return;
            }

            _sharedMaterial = new Material(shader) { name = "Water_Shared" };
            _bodyMaterial = new Material(shader) { name = "WaterBody_Shared", renderQueue = 2999 };
        }

        private Material GetFallbackMaterial()
        {
            if (_fallbackMaterial != null) return _fallbackMaterial;
            _fallbackMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            _fallbackMaterial.color = new Color(0.1f, 0.3f, 0.6f, 0.85f);
            _fallbackMaterial.SetFloat("_Surface", 1);
            _fallbackMaterial.renderQueue = 3000;
            return _fallbackMaterial;
        }

        public GameObject CreateWaterTile(Vector3 worldPos, float cellSize, int gridX, int gridZ,
            bool neighborPosX = false, bool neighborNegX = false,
            bool neighborPosZ = false, bool neighborNegZ = false)
        {
            var config = GameConfig.Instance;
            bool fancy = config != null && config.EnableFancyWater;
            Material mat = fancy ? _sharedMaterial ?? GetFallbackMaterial() : GetFallbackMaterial();
            if (mat == null) return null;

            var go = new GameObject($"Water_{gridX}_{gridZ}");
            go.transform.position = new Vector3(worldPos.x, _waterY, worldPos.z);

            var mf = go.AddComponent<MeshFilter>();
            bool useShoreNoise = config != null && config.EnableWaterShoreNoise && fancy;

            mf.mesh = useShoreNoise
                ? CreateSubdividedWaterMesh(cellSize, worldPos, neighborPosX, neighborNegX, neighborPosZ, neighborNegZ)
                : MapRenderer.CreateQuadMesh(cellSize);

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
            var config = GameConfig.Instance;
            if (config == null || !config.EnableFancyWater) return null;
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
            var config = GameConfig.Instance;
            if (config != null && config.EnableFancyWater && _sharedMaterial != null)
                ApplyMaterialProperties(config);
        }

        private static void SetKeyword(Material mat, string keyword, bool enabled)
        {
            if (enabled)
                mat.EnableKeyword(keyword);
            else
                mat.DisableKeyword(keyword);
        }

        public Material GetWaterMaterial() => _sharedMaterial;

        public void MarkDirty() { }

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
            _prevFancy = true;
        }

        private Transform GetTerrainRoot()
        {
            if (_mapRenderer == null)
                _mapRenderer = GetComponentInParent<MapRenderer>();
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

        private static Mesh CreateSubdividedWaterMesh(float cellSize, Vector3 worldPos,
            bool neighborPosX, bool neighborNegX, bool neighborPosZ, bool neighborNegZ)
        {
            var config = GameConfig.Instance;
            float noiseScale = config != null ? config.WaterShoreNoiseScale : 3f;
            float irregularity = config != null ? config.WaterShoreIrregularity : 0.35f;

            const int sub = 8;
            int stride = sub + 1;
            float half = cellSize * 0.5f;
            float step = cellSize / sub;

            var vertices = new Vector3[stride * stride];
            var uvs = new Vector2[stride * stride];
            var triangles = new int[sub * sub * 6];

            float wx0 = worldPos.x - half;
            float wz0 = worldPos.z - half;

            for (int iz = 0; iz <= sub; iz++)
            {
                for (int ix = 0; ix <= sub; ix++)
                {
                    int idx = iz * stride + ix;
                    float lx = -half + ix * step;
                    float lz = -half + iz * step;

                    bool edgeX0 = ix == 0;
                    bool edgeX1 = ix == sub;
                    bool edgeZ0 = iz == 0;
                    bool edgeZ1 = iz == sub;

                    if (edgeX0 || edgeX1 || edgeZ0 || edgeZ1)
                    {
                        float wx = wx0 + (ix * step);
                        float wz = wz0 + (iz * step);
                        float n = Mathf.PerlinNoise(wx * noiseScale * 0.15f, wz * noiseScale * 0.15f);
                        float push = n * irregularity * step * 2.5f;

                        if (edgeX0 && !neighborNegX) lx += push;
                        if (edgeX1 && !neighborPosX) lx -= push;
                        if (edgeZ0 && !neighborNegZ) lz += push;
                        if (edgeZ1 && !neighborPosZ) lz -= push;
                    }

                    vertices[idx] = new Vector3(lx, 0, lz);
                    uvs[idx] = new Vector2((float)ix / sub, (float)iz / sub);
                }
            }

            int ti = 0;
            for (int iz = 0; iz < sub; iz++)
            {
                for (int ix = 0; ix < sub; ix++)
                {
                    int bl = iz * stride + ix;
                    int br = bl + 1;
                    int tl = bl + stride;
                    int tr = tl + 1;

                    triangles[ti++] = bl;
                    triangles[ti++] = tl;
                    triangles[ti++] = br;

                    triangles[ti++] = br;
                    triangles[ti++] = tl;
                    triangles[ti++] = tr;
                }
            }

            var mesh = new Mesh
            {
                vertices = vertices,
                uv = uvs,
                triangles = triangles
            };
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
