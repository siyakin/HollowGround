using HollowGround.Buildings;
using HollowGround.Core;
using UnityEngine;

namespace HollowGround.Grid
{
    public class SceneFogManager : Singleton<SceneFogManager>
    {
        public enum FogState : byte { Fogged = 0, Explored = 1, Visible = 2 }

        [Header("Vision Radii Fallbacks (overridden by GameConfig)")]
        [SerializeField] private int _ccRadius         = 6;
        [SerializeField] private int _watchTowerRadius = 8;
        [SerializeField] private int _wallRadius       = 1;
        [SerializeField] private int _baseRadius       = 3;
        [SerializeField] private int _roadVisionRadius = 1;
        [SerializeField] private int _initialRevealRadius = 12;

        [Header("Visual")]
        [SerializeField] private float _fogPlaneY      = 0.05f;
        [SerializeField] private Color _foggedColor    = new Color(0f,    0f,    0f,    1f);
        [SerializeField] private Color _exploredColor  = new Color(0.04f, 0.06f, 0.18f, 0.72f);
        [SerializeField] private float _overlapPadding = 3000f;
        [SerializeField] private bool  _tintCamera     = true;
        [SerializeField] private int   _edgeBlurPasses = 3;

        // ── runtime state ──────────────────────────────────────────────────
        private FogState[,] _fogGrid;
        private Texture2D   _fogTex;
        private Material    _fogMat;
        private GameObject  _fogPlane;
        private bool        _initialized;

        // Cached grid metrics (set in Initialize)
        private Vector3 _origin;
        private float   _worldW;
        private float   _worldH;
        private int     _gW;
        private int     _gH;

        // Cached arrays to avoid GC every FlushTexture call
        private Color[] _flushPixels;
        private Color[] _blurTmp;

        // Cached camera reference (Discovery #6)
        private UnityEngine.Camera _cachedCam;

        // Resolved radii (from GameConfig or serialized fallbacks)
        private int _resolvedCCRadius;
        private int _resolvedWatchTowerRadius;
        private int _resolvedBaseRadius;
        private int _resolvedInitialRevealRadius;

        public bool        IsInitialized => _initialized;
        public Texture2D   FogTexture    => _fogTex;

        // ── lifecycle ──────────────────────────────────────────────────────

        private void Start()
        {
            ResolveRadii();
            InitializeFromGrid();

            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.OnBuildingAdded   += OnBuildingAdded;
                BuildingManager.Instance.OnBuildingRemoved += OnBuildingRemoved;
            }
            if (Roads.RoadManager.Instance != null)
                Roads.RoadManager.Instance.OnRoadsChanged += OnRoadsChanged;
        }

        protected override void OnDestroy()
        {
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.OnBuildingAdded   -= OnBuildingAdded;
                BuildingManager.Instance.OnBuildingRemoved -= OnBuildingRemoved;
            }
            if (Roads.RoadManager.Instance != null)
                Roads.RoadManager.Instance.OnRoadsChanged -= OnRoadsChanged;
            if (_fogTex   != null) Destroy(_fogTex);
            if (_fogMat   != null) Destroy(_fogMat);
            if (_fogPlane != null) Destroy(_fogPlane);
        }

        private void ResolveRadii()
        {
            var cfg = GameConfig.Instance;
            if (cfg != null)
            {
                _resolvedCCRadius         = cfg.SceneFogCCVisionRadius;
                _resolvedWatchTowerRadius = cfg.SceneFogWatchTowerVisionRadius;
                _resolvedBaseRadius       = cfg.SceneFogBaseVisionRadius;
                _resolvedInitialRevealRadius = cfg.SceneFogInitialRevealRadius;
            }
            else
            {
                _resolvedCCRadius         = _ccRadius;
                _resolvedWatchTowerRadius = _watchTowerRadius;
                _resolvedBaseRadius       = _baseRadius;
                _resolvedInitialRevealRadius = _initialRevealRadius;
            }
        }

        // ── public init (also called from SaveSystem / GameInitializer) ────

        public void InitializeFromGrid()
        {
            if (_initialized) return;
            if (GridSystem.Instance == null) return;
            if (GameConfig.Instance != null && !GameConfig.Instance.EnableSceneFog) return;

            _gW = GridSystem.Instance.Width;
            _gH = GridSystem.Instance.Height;
            float cellSize = GridSystem.Instance.CellSize;

            Vector3 cell00 = GridSystem.Instance.GetWorldPosition(0, 0);
            _origin = new Vector3(cell00.x - cellSize * 0.5f,
                                  cell00.y,
                                  cell00.z - cellSize * 0.5f);
            _worldW = _gW * cellSize;
            _worldH = _gH * cellSize;

            _fogGrid = new FogState[_gW, _gH];

            // Pre-allocate cached pixel arrays
            int pixelCount = _gW * _gH;
            _flushPixels = new Color[pixelCount];
            _blurTmp     = new Color[pixelCount];

            BuildFogTexture();
            BuildFogPlane();

            // Reveal and subscribe to buildings already placed
            if (BuildingManager.Instance != null)
            {
                foreach (var b in BuildingManager.Instance.AllBuildings)
                {
                    SubscribeBuildingEvents(b);
                    MarkVisible(b);
                }
            }

            MarkRoadsVisible();
            ApplyInitialReveal();
            FlushTexture();

            if (_tintCamera)
            {
                _cachedCam = UnityEngine.Camera.main;
                if (_cachedCam != null)
                {
                    _cachedCam.clearFlags      = CameraClearFlags.SolidColor;
                    _cachedCam.backgroundColor = _foggedColor;
                }
            }

            _initialized = true;
        }

        // ── building events ───────────────────────────────────────────────

        private void SubscribeBuildingEvents(Building b)
        {
            if (b == null) return;
            b.OnConstructionComplete += OnConstructionComplete;
            b.OnStateChanged         += OnStateChanged;
        }

        private void UnsubscribeBuildingEvents(Building b)
        {
            if (b == null) return;
            b.OnConstructionComplete -= OnConstructionComplete;
            b.OnStateChanged         -= OnStateChanged;
        }

        private void OnBuildingAdded(Building b)
        {
            if (!_initialized) return;
            SubscribeBuildingEvents(b);

            if (b.State == BuildingState.Active || b.State == BuildingState.Upgrading)
            {
                MarkVisible(b);
                FlushTexture();
            }
        }

        private void OnBuildingRemoved(Building b)
        {
            if (!_initialized) return;
            UnsubscribeBuildingEvents(b);
            FullRecalculate();
        }

        private void OnConstructionComplete(Building b)
        {
            if (!_initialized) return;
            MarkVisible(b);
            FlushTexture();
        }

        private void OnStateChanged(Building b, BuildingState newState)
        {
            if (!_initialized) return;
            if (newState == BuildingState.Destroyed) FullRecalculate();
        }

        // ── vision ────────────────────────────────────────────────────────

        private void MarkVisible(Building b)
        {
            if (b == null || b.State == BuildingState.Destroyed) return;

            int cx = b.GridOrigin.x;
            int cz = b.GridOrigin.y;
            var (sx, sz) = b.GetRotatedFootprint();
            int r = GetRadius(b);

            for (int x = cx - r; x < cx + sx + r; x++)
            {
                for (int z = cz - r; z < cz + sz + r; z++)
                {
                    if ((uint)x >= (uint)_gW || (uint)z >= (uint)_gH) continue;

                    float nx   = Mathf.Clamp(x, cx, cx + sx - 1);
                    float nz   = Mathf.Clamp(z, cz, cz + sz - 1);
                    float dist = Mathf.Sqrt((x - nx) * (x - nx) + (z - nz) * (z - nz));

                    if (dist <= r + 0.5f)
                        _fogGrid[x, z] = FogState.Visible;
                }
            }
        }

        private void OnRoadsChanged()
        {
            if (!_initialized) return;
            MarkRoadsVisible();
            FlushTexture();
        }

        private void MarkRoadsVisible()
        {
            var rm = Roads.RoadManager.Instance;
            if (rm == null) return;
            foreach (var cell in rm.GetAllRoadCells())
            {
                int r = _roadVisionRadius;
                for (int dx = -r; dx <= r; dx++)
                    for (int dz = -r; dz <= r; dz++)
                    {
                        int x = cell.x + dx;
                        int z = cell.y + dz;
                        if ((uint)x >= (uint)_gW || (uint)z >= (uint)_gH) continue;
                        _fogGrid[x, z] = FogState.Visible;
                    }
            }
        }

        private void FullRecalculate()
        {
            for (int x = 0; x < _gW; x++)
                for (int z = 0; z < _gH; z++)
                    if (_fogGrid[x, z] == FogState.Visible)
                        _fogGrid[x, z] = FogState.Explored;

            if (BuildingManager.Instance != null)
                foreach (var b in BuildingManager.Instance.AllBuildings)
                    if (b.State != BuildingState.Destroyed)
                        MarkVisible(b);

            MarkRoadsVisible();
            FlushTexture();
        }

        private int GetRadius(Building b)
        {
            if (b.Data == null) return _resolvedBaseRadius;
            return b.Data.Type switch
            {
                BuildingType.CommandCenter => _resolvedCCRadius,
                BuildingType.WatchTower    => _resolvedWatchTowerRadius,
                BuildingType.Walls         => _wallRadius,
                _                          => _resolvedBaseRadius
            };
        }

        private void ApplyInitialReveal()
        {
            if (_resolvedInitialRevealRadius <= 0) return;

            Vector2Int center = FindCommandCenterGridPos();
            int r = _resolvedInitialRevealRadius;
            float rSq = r * r;

            for (int dx = -r; dx <= r; dx++)
            {
                for (int dz = -r; dz <= r; dz++)
                {
                    float distSq = dx * dx + dz * dz;
                    if (distSq > rSq) continue;

                    int x = center.x + dx;
                    int z = center.y + dz;
                    if ((uint)x >= (uint)_gW || (uint)z >= (uint)_gH) continue;

                    if (_fogGrid[x, z] == FogState.Fogged)
                        _fogGrid[x, z] = FogState.Visible;
                }
            }
        }

        private Vector2Int FindCommandCenterGridPos()
        {
            if (BuildingManager.Instance != null)
            {
                foreach (var b in BuildingManager.Instance.AllBuildings)
                {
                    if (b.Data != null && b.Data.Type == BuildingType.CommandCenter)
                        return b.GridOrigin;
                }
            }

            var cfg = GameConfig.Instance;
            if (cfg != null)
                return cfg.StartingCCPosition;

            return new Vector2Int(_gW / 2, _gH / 2);
        }

        // ── texture ───────────────────────────────────────────────────────

        private void BuildFogTexture()
        {
            _fogTex = new Texture2D(_gW, _gH, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode   = TextureWrapMode.Clamp
            };

            Color[] px = new Color[_gW * _gH];
            for (int i = 0; i < px.Length; i++) px[i] = _foggedColor;
            _fogTex.SetPixels(px);
            _fogTex.Apply();
        }

        private void FlushTexture()
        {
            if (_fogTex == null || _fogGrid == null || _flushPixels == null) return;

            for (int x = 0; x < _gW; x++)
                for (int z = 0; z < _gH; z++)
                    _flushPixels[z * _gW + x] = _fogGrid[x, z] switch
                    {
                        FogState.Visible  => Color.clear,
                        FogState.Explored => _exploredColor,
                        _                 => _foggedColor
                    };

            if (_edgeBlurPasses > 0)
                BlurAlphaInPlace(_flushPixels, _blurTmp, _gW, _gH, _edgeBlurPasses);

            _fogTex.SetPixels(_flushPixels);
            _fogTex.Apply();
        }

        private void BlurAlphaInPlace(Color[] buf, Color[] tmp, int w, int h, int passes)
        {
            for (int p = 0; p < passes; p++)
            {
                for (int z = 0; z < h; z++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        float a = 0f;
                        int   n = 0;
                        for (int dz = -1; dz <= 1; dz++)
                        {
                            int nz = z + dz;
                            if (nz < 0 || nz >= h) continue;
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                int nx = x + dx;
                                if (nx < 0 || nx >= w) continue;
                                a += buf[nz * w + nx].a;
                                n++;
                            }
                        }
                        Color c = buf[z * w + x];
                        tmp[z * w + x] = new Color(c.r, c.g, c.b, a / n);
                    }
                }
                // swap: copy tmp → buf
                System.Array.Copy(tmp, buf, buf.Length);
            }
        }

        // ── fog plane ─────────────────────────────────────────────────────

        private void BuildFogPlane()
        {
            float planeW = _worldW + _overlapPadding * 2f;
            float planeH = _worldH + _overlapPadding * 2f;
            float uPad   = _overlapPadding / _worldW;
            float vPad   = _overlapPadding / _worldH;

            var mesh = new Mesh { name = "FogQuad" };
            mesh.vertices = new Vector3[]
            {
                new(-0.5f, -0.5f, 0f),
                new( 0.5f, -0.5f, 0f),
                new(-0.5f,  0.5f, 0f),
                new( 0.5f,  0.5f, 0f)
            };
            mesh.uv = new Vector2[]
            {
                new(-uPad,       -vPad      ),
                new( 1f + uPad,  -vPad      ),
                new(-uPad,        1f + vPad ),
                new( 1f + uPad,   1f + vPad )
            };
            mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
            mesh.RecalculateNormals();

            _fogPlane = new GameObject("SceneFogPlane");
            _fogPlane.transform.SetParent(transform);
            _fogPlane.layer = LayerMask.NameToLayer("Ignore Raycast");

            _fogPlane.AddComponent<MeshFilter>().mesh = mesh;

            var shader = Shader.Find("HollowGround/FogOfWar");
            if (shader == null)
            {
                Debug.LogError("[SceneFog] Shader 'HollowGround/FogOfWar' not found!");
                shader = Shader.Find("Unlit/Transparent");
            }

            _fogMat = new Material(shader) { name = "SceneFog", mainTexture = _fogTex };
            var mr = _fogPlane.AddComponent<MeshRenderer>();
            mr.material          = _fogMat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows    = false;

            _fogPlane.transform.position   = new Vector3(
                _origin.x + _worldW * 0.5f,
                _fogPlaneY,
                _origin.z + _worldH * 0.5f);
            _fogPlane.transform.rotation   = Quaternion.Euler(90f, 0f, 0f);
            _fogPlane.transform.localScale = new Vector3(planeW, planeH, 1f);
        }

        // ── save / load ───────────────────────────────────────────────────

        public FogState[] CaptureFogState()
        {
            if (_fogGrid == null) return null;
            var arr = new FogState[_gW * _gH];
            for (int x = 0; x < _gW; x++)
                for (int z = 0; z < _gH; z++)
                    arr[z * _gW + x] = _fogGrid[x, z];
            return arr;
        }

        public void RestoreFogState(FogState[] state)
        {
            if (_fogGrid == null || state == null || state.Length != _gW * _gH) return;
            for (int x = 0; x < _gW; x++)
                for (int z = 0; z < _gH; z++)
                    _fogGrid[x, z] = state[z * _gW + x];
            FlushTexture();
        }

        public void ClearForLoad()
        {
            if (_fogGrid == null) return;
            for (int x = 0; x < _gW; x++)
                for (int z = 0; z < _gH; z++)
                    _fogGrid[x, z] = FogState.Fogged;
            FlushTexture();
        }

        // ── public queries ────────────────────────────────────────────────

        public FogState GetCellFogState(int x, int z)
        {
            if (_fogGrid == null || (uint)x >= (uint)_gW || (uint)z >= (uint)_gH)
                return FogState.Fogged;
            return _fogGrid[x, z];
        }

        public bool IsVisible(int x, int z)  => GetCellFogState(x, z) == FogState.Visible;
        public bool IsExplored(int x, int z) => GetCellFogState(x, z) >= FogState.Explored;
    }
}
