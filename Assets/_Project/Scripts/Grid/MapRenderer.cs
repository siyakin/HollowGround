using System.Collections.Generic;
using HollowGround.Core;
using UnityEngine;

namespace HollowGround.Grid
{
    public class MapRenderer : MonoBehaviour
    {
        [Header("Materials")]
        [SerializeField] private Material _flatMaterial;
        [SerializeField] private Material _waterMaterial;
        [SerializeField] private Material _riverMaterial;
        [SerializeField] private Material _mountainMaterial;
        [SerializeField] private Material _rockMaterial;
        [SerializeField] private Material _cliffMaterial;
        [SerializeField] private Material _sandMaterial;
        [SerializeField] private Material _forestMaterial;

        [Header("Settings")]
        [SerializeField] private float _waterY = -0.1f;
        [SerializeField] private float _mountainHeight = 3f;
        [SerializeField] private float _cliffHeight = 1.5f;
        [SerializeField] private float _rockScale = 0.6f;
        [SerializeField] private float _groundLevel = 0.02f;

        private GameObject _terrainRoot;
        private Dictionary<Vector2Int, GameObject> _terrainObjects = new();
        private MapTemplate _currentTemplate;
        private WaterSurface _waterSurface;

        public IReadOnlyDictionary<Vector2Int, GameObject> TerrainObjects => _terrainObjects;

        private void Update()
        {
        }

        public void RenderMap(MapTemplate template, GridSystem gridSystem)
        {
            ClearAllImmediate();
            _currentTemplate = template;

            if (template == null || gridSystem == null) return;

            _terrainRoot = new GameObject("TerrainTiles");
            _terrainRoot.transform.SetParent(transform);
            _terrainRoot.transform.localPosition = Vector3.zero;

            float cellSize = gridSystem.CellSize;
            Mesh quadMesh = CreateQuadMesh(cellSize);
            Mesh cubeMesh = CreateCubeMesh(cellSize);

            for (int x = 0; x < template.Width; x++)
            {
                for (int z = 0; z < template.Height; z++)
                {
                    var terrain = template.GetTile(x, z);
                    if (terrain == TerrainType.Flat) continue;

                    var worldPos = gridSystem.GetWorldPosition(x, z);
                    var key = new Vector2Int(x, z);
                    GameObject tileObj = CreateTerrainTile(terrain, worldPos, cellSize, quadMesh, cubeMesh, x, z);
                    if (tileObj != null)
                    {
                        tileObj.transform.SetParent(_terrainRoot.transform);
                        tileObj.name = $"Terrain_{terrain}_{x}_{z}";
                        _terrainObjects[key] = tileObj;
                    }
                }
            }

            EnsureWaterSurface();
        }

        private GameObject CreateTerrainTile(TerrainType terrain, Vector3 worldPos, float cellSize, Mesh quadMesh, Mesh cubeMesh, int gx, int gz)
        {
            Material mat = GetMaterial(terrain);
            if (mat == null) return null;

            GameObject go = terrain switch
            {
                TerrainType.Water => CreateWaterTileRouted(worldPos, cellSize, gx, gz),
                TerrainType.River => CreateRiverTileRouted(worldPos, cellSize, mat, quadMesh, gx, gz),
                TerrainType.Mountain => CreateMountainTile(worldPos, cellSize, mat, cubeMesh),
                TerrainType.Rock => CreateRockTile(worldPos, cellSize, mat),
                TerrainType.Cliff => CreateCliffTile(worldPos, cellSize, mat, cubeMesh),
                TerrainType.Sand => CreateSandTile(worldPos, cellSize, mat, quadMesh),
                TerrainType.Forest => CreateForestTile(worldPos, cellSize, mat),
                _ => null
            };

            if (go != null && terrain != TerrainType.Water && terrain != TerrainType.River)
            {
                var tag = go.AddComponent<TerrainTile>();
                tag.TerrainType = terrain;
                tag.GridX = gx;
                tag.GridZ = gz;
            }

            return go;
        }

        private GameObject CreateWaterTileRouted(Vector3 worldPos, float cellSize, int gx, int gz)
        {
            EnsureWaterSurface();
            if (_waterSurface != null)
            {
                bool npX = IsWaterTile(gx + 1, gz);
                bool nnX = IsWaterTile(gx - 1, gz);
                bool npZ = IsWaterTile(gx, gz + 1);
                bool nnZ = IsWaterTile(gx, gz - 1);
                return _waterSurface.CreateWaterTile(worldPos, cellSize, gx, gz, npX, nnX, npZ, nnZ);
            }
            return CreateWaterTileFallback(worldPos, cellSize, _waterMaterial, CreateQuadMesh(cellSize));
        }

        private GameObject CreateRiverTileRouted(Vector3 worldPos, float cellSize, Material mat, Mesh quadMesh, int gx, int gz)
        {
            EnsureWaterSurface();
            if (_waterSurface != null)
            {
                bool npX = IsWaterTile(gx + 1, gz);
                bool nnX = IsWaterTile(gx - 1, gz);
                bool npZ = IsWaterTile(gx, gz + 1);
                bool nnZ = IsWaterTile(gx, gz - 1);
                return _waterSurface.CreateWaterTile(worldPos, cellSize, gx, gz, npX, nnX, npZ, nnZ);
            }
            return CreateRiverTileFallback(worldPos, cellSize, mat, quadMesh, gx, gz);
        }

        private bool IsWaterTile(int x, int z)
        {
            if (_currentTemplate == null) return false;
            if (x < 0 || x >= _currentTemplate.Width || z < 0 || z >= _currentTemplate.Height) return false;
            var t = _currentTemplate.GetTile(x, z);
            return t == TerrainType.Water || t == TerrainType.River;
        }

        private void EnsureWaterSurface()
        {
            if (_waterSurface != null) return;
            _waterSurface = GetComponent<WaterSurface>();
            if (_waterSurface != null) return;
            _waterSurface = gameObject.AddComponent<WaterSurface>();
        }

        private GameObject CreateWaterTileFallback(Vector3 worldPos, float cellSize, Material mat, Mesh quadMesh)
        {
            var go = new GameObject("Water");
            go.transform.position = new Vector3(worldPos.x, _waterY, worldPos.z);
            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = quadMesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.material = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        private GameObject CreateRiverTileFallback(Vector3 worldPos, float cellSize, Material mat, Mesh quadMesh, int gx, int gz)
        {
            var go = new GameObject("River");
            go.transform.position = new Vector3(worldPos.x, _waterY, worldPos.z);
            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = quadMesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.material = new Material(mat);
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            Vector2 uvOffset = new Vector2(
                Mathf.PerlinNoise(gx * 0.5f, gz * 0.5f) * 0.5f,
                Mathf.PerlinNoise(gx * 0.3f + 100f, gz * 0.3f) * 0.5f
            );
            mr.material.SetTextureOffset("_BaseMap", uvOffset);

            return go;
        }

        private GameObject CreateMountainTile(Vector3 worldPos, float cellSize, Material mat, Mesh cubeMesh)
        {
            var go = new GameObject("Mountain");
            float height = _mountainHeight + Random.Range(-0.5f, 0.5f);
            go.transform.position = new Vector3(worldPos.x, height * 0.5f, worldPos.z);
            go.transform.localScale = new Vector3(0.95f, height / cellSize, 0.95f);
            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = cubeMesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.material = mat;
            var col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(cellSize, cellSize, cellSize);
            return go;
        }

        private GameObject CreateRockTile(Vector3 worldPos, float cellSize, Material mat)
        {
            var go = new GameObject("Rock");
            float scale = _rockScale + Random.Range(-0.1f, 0.15f);
            go.transform.position = new Vector3(worldPos.x, scale * 0.5f, worldPos.z);
            go.transform.localScale = new Vector3(scale, scale, scale);
            go.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), Random.Range(-5, 5));
            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = CreateIcoSphereMesh(cellSize * 0.35f);
            var mr = go.AddComponent<MeshRenderer>();
            mr.material = mat;
            return go;
        }

        private GameObject CreateCliffTile(Vector3 worldPos, float cellSize, Material mat, Mesh cubeMesh)
        {
            var go = new GameObject("Cliff");
            float height = _cliffHeight + Random.Range(-0.3f, 0.3f);
            go.transform.position = new Vector3(worldPos.x, height * 0.5f, worldPos.z);
            go.transform.localScale = new Vector3(0.98f, height / cellSize, 0.98f);
            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = cubeMesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.material = mat;
            var col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(cellSize, cellSize, cellSize);
            return go;
        }

        private GameObject CreateSandTile(Vector3 worldPos, float cellSize, Material mat, Mesh quadMesh)
        {
            var go = new GameObject("Sand");
            go.transform.position = new Vector3(worldPos.x, _groundLevel, worldPos.z);
            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = quadMesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.material = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        private GameObject CreateForestTile(Vector3 worldPos, float cellSize, Material mat)
        {
            var go = new GameObject("Forest");
            go.transform.position = new Vector3(worldPos.x, _groundLevel, worldPos.z);

            int treeCount = Random.Range(2, 5);
            for (int i = 0; i < treeCount; i++)
            {
                var tree = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tree.transform.SetParent(go.transform);
                float offsetX = Random.Range(-cellSize * 0.35f, cellSize * 0.35f);
                float offsetZ = Random.Range(-cellSize * 0.35f, cellSize * 0.35f);
                float height = Random.Range(0.8f, 1.6f);
                tree.transform.localPosition = new Vector3(offsetX, height * 0.5f, offsetZ);
                tree.transform.localScale = new Vector3(0.15f, height * 0.5f, 0.15f);
                var treeRenderer = tree.GetComponent<MeshRenderer>();
                treeRenderer.material = new Material(mat);
                treeRenderer.material.color = new Color(0.2f, 0.5f + Random.Range(0f, 0.15f), 0.15f);
                DestroyImmediate(tree.GetComponent<Collider>());
            }

            var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.transform.SetParent(go.transform);
            float canopySize = Random.Range(0.4f, 0.7f);
            canopy.transform.localPosition = new Vector3(0, 1.2f, 0);
            canopy.transform.localScale = new Vector3(canopySize * 2, canopySize, canopySize * 2);
            var canopyRenderer = canopy.GetComponent<MeshRenderer>();
            canopyRenderer.material = new Material(mat);
            canopyRenderer.material.color = new Color(0.15f, 0.45f + Random.Range(0f, 0.1f), 0.1f, 0.9f);
            DestroyImmediate(canopy.GetComponent<Collider>());

            return go;
        }

        public Material GetMaterial(TerrainType terrain)
        {
            return terrain switch
            {
                TerrainType.Flat => _flatMaterial,
                TerrainType.Water => _waterMaterial,
                TerrainType.River => _riverMaterial,
                TerrainType.Mountain => _mountainMaterial,
                TerrainType.Rock => _rockMaterial,
                TerrainType.Cliff => _cliffMaterial,
                TerrainType.Sand => _sandMaterial,
                TerrainType.Forest => _forestMaterial,
                _ => null
            };
        }

        public void ClearAll()
        {
            if (_waterSurface != null) _waterSurface.ClearWater();

            var existing = GameObject.Find("TerrainTiles");
            if (existing != null)
                DestroyImmediate(existing);

            if (_terrainRoot != null && _terrainRoot != existing)
                DestroyImmediate(_terrainRoot);

            _terrainObjects.Clear();
        }

        public void ClearAllImmediate()
        {
            if (_waterSurface != null) _waterSurface.ClearWater();

            var existing = GameObject.Find("TerrainTiles");
            if (existing != null)
                DestroyImmediate(existing);

            if (_terrainRoot != null && _terrainRoot != existing)
                DestroyImmediate(_terrainRoot);

            _terrainObjects.Clear();
        }

        public void RefreshSingleTile(int x, int z, TerrainType terrain, GridSystem gridSystem)
        {
            var key = new Vector2Int(x, z);
            if (_terrainObjects.TryGetValue(key, out var existing))
            {
                DestroyImmediate(existing);
                _terrainObjects.Remove(key);
            }

            if (terrain == TerrainType.Flat) return;
            if (gridSystem == null) return;

            float cellSize = gridSystem.CellSize;
            Mesh quadMesh = CreateQuadMesh(cellSize);
            Mesh cubeMesh = CreateCubeMesh(cellSize);
            var worldPos = gridSystem.GetWorldPosition(x, z);

            GameObject tileObj = CreateTerrainTile(terrain, worldPos, cellSize, quadMesh, cubeMesh, x, z);
            if (tileObj != null)
            {
                if (_terrainRoot == null)
                {
                    _terrainRoot = new GameObject("TerrainTiles");
                    _terrainRoot.transform.SetParent(transform);
                    _terrainRoot.transform.localPosition = Vector3.zero;
                }
                tileObj.transform.SetParent(_terrainRoot.transform);
                tileObj.name = $"Terrain_{terrain}_{x}_{z}";
                _terrainObjects[key] = tileObj;
            }
        }

        public static Mesh CreateQuadMesh(float size)
        {
            float half = size * 0.5f;
            var mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new(-half, 0, -half),
                new(half, 0, -half),
                new(half, 0, half),
                new(-half, 0, half)
            };
            mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            mesh.uv = new Vector2[]
            {
                new(0, 0), new(1, 0), new(1, 1), new(0, 1)
            };
            mesh.RecalculateNormals();
            return mesh;
        }

        public static Mesh CreateCubeMesh(float size)
        {
            float half = size * 0.5f;
            var mesh = new Mesh
            {
                vertices = new Vector3[]
                {
                    new(-half, -half, -half), new(half, -half, -half),
                    new(half, -half, half), new(-half, -half, half),
                    new(-half, half, -half), new(half, half, -half),
                    new(half, half, half), new(-half, half, half)
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

        private static Mesh CreateIcoSphereMesh(float radius)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var filter = go.GetComponent<MeshFilter>();
            var mesh = filter.sharedMesh != null ? Object.Instantiate(filter.sharedMesh) : new Mesh();
            DestroyImmediate(go);
            return mesh;
        }

        public static void CreateDefaultMaterials(MapRenderer renderer)
        {
#if UNITY_EDITOR
            string folder = "Assets/_Project/Materials/Terrain";
            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/_Project/Materials"))
                UnityEditor.AssetDatabase.CreateFolder("Assets/_Project", "Materials");
            if (!UnityEditor.AssetDatabase.IsValidFolder(folder))
                UnityEditor.AssetDatabase.CreateFolder("Assets/_Project/Materials", "Terrain");

            var terrainTypes = new[]
            {
                (TerrainType.Water, "Water", new Color(0.1f, 0.3f, 0.6f, 0.85f)),
                (TerrainType.River, "River", new Color(0.15f, 0.35f, 0.55f, 0.8f)),
                (TerrainType.Mountain, "Mountain", new Color(0.35f, 0.3f, 0.25f)),
                (TerrainType.Rock, "Rock", new Color(0.45f, 0.42f, 0.38f)),
                (TerrainType.Cliff, "Cliff", new Color(0.4f, 0.35f, 0.3f)),
                (TerrainType.Sand, "Sand", new Color(0.76f, 0.7f, 0.5f)),
                (TerrainType.Forest, "Forest", new Color(0.2f, 0.45f, 0.15f))
            };

            foreach (var (type, name, color) in terrainTypes)
            {
                string path = $"{folder}/{name}.mat";
                if (UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path) != null) continue;

                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.name = name;
                mat.color = color;

                if (type == TerrainType.Water || type == TerrainType.River)
                {
                    mat.SetFloat("_Surface", 1);
                    mat.SetFloat("_Blend", 0);
                    mat.renderQueue = 3000;
                }

                UnityEditor.AssetDatabase.CreateAsset(mat, path);
            }

            UnityEditor.AssetDatabase.Refresh();
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }
    }
}
