using System.Collections.Generic;
using System.Linq;
using HollowGround.Buildings;
using HollowGround.Core;
using HollowGround.Grid;
using HollowGround.Resources;
using HollowGround.Roads;
using UnityEngine;

namespace HollowGround.NPCs
{
    public class SettlerManager : Singleton<SettlerManager>
    {
        [Header("Settler Models (drag FBX here)")]
        [SerializeField] private GameObject[] _settlerModels;
        [Header("Animator Controllers (per skeleton type)")]
        [SerializeField] private RuntimeAnimatorController _animatorController;
        [SerializeField] private RuntimeAnimatorController _manAnimatorController;
        [SerializeField] private RuntimeAnimatorController _womanAnimatorController;

        private readonly List<SettlerWalker> _pool = new();
        private GameObject _settlerParent;
        private float _idleSpawnTimer;
        private float _lastDispatchTime;
        private const float MinDispatchInterval = 2f;

        private const string SettlerParentName = "Settlers";
        private const float SettlerYOffset = 0.05f;
        private const float IdleSpawnInterval = 15f;

        public int SettlerCount => _pool.Count;
        public int ActiveCount { get; private set; }
        public int TotalPopulation => GetPopulation();

        public event System.Action<SettlerWalker> OnSettlerSpawned;
        public event System.Action<SettlerWalker> OnSettlerRemoved;

        protected override void Awake()
        {
            base.Awake();
            _settlerParent = new GameObject(SettlerParentName);
            _settlerParent.transform.SetParent(transform);
        }

        private void Start()
        {
            SubscribeEvents();
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
            if (TimeManager.Instance != null && TimeManager.Instance.IsPaused) return;

            var cfg = GameConfig.Instance;
            if (cfg != null && cfg.DisableSettlers) return;

            _idleSpawnTimer += Time.deltaTime;
            if (_idleSpawnTimer >= IdleSpawnInterval)
            {
                _idleSpawnTimer = 0f;
                DispatchIdleWalker();
            }
        }

        private void SubscribeEvents()
        {
            var bm = BuildingManager.Instance;
            if (bm != null)
            {
                bm.OnBuildingAdded += OnBuildingAdded;
                bm.OnBuildingRemoved += OnBuildingRemoved;
            }
        }

        private void OnBuildingAdded(Building building)
        {
            building.OnConstructionComplete += OnConstructionComplete;
            building.OnProduced += OnProduced;
        }

        private void OnBuildingRemoved(Building building)
        {
            building.OnConstructionComplete -= OnConstructionComplete;
            building.OnProduced -= OnProduced;
        }

        private void OnConstructionComplete(Building building)
        {
            var cc = FindCommandCenter();
            if (cc != null)
                Dispatch(cc.GetDoorCell(), building.GetDoorCell(), 3f);
            else
                DispatchFromRandom(building.GetDoorCell(), 3f);
        }

        private void OnProduced(Building building, ResourceType type, int amount)
        {
            if (ActiveCount >= 3) return;
            var storage = FindNearestStorage(building);
            Vector2Int dest = storage != null ? storage.GetDoorCell() : FindCommandCenter()?.GetDoorCell() ?? building.GetDoorCell();
            Dispatch(building.GetDoorCell(), dest, 2f);
        }

        private void DispatchIdleWalker()
        {
            if (RoadManager.Instance == null || !RoadManager.Instance.HasRoads) return;
            if (ActiveCount >= GetMaxSettlers()) return;

            var doors = RoadManager.Instance.GetActiveBuildingDoorCells();
            if (doors.Count < 2) return;

            Vector2Int origin = doors[Random.Range(0, doors.Count)];
            Vector2Int dest;
            int attempts = 0;
            do
            {
                dest = doors[Random.Range(0, doors.Count)];
                attempts++;
            } while (dest == origin && attempts < 10);

            if (dest == origin) return;

            Dispatch(origin, dest, Random.Range(1f, 4f));
        }

        private void Dispatch(Vector2Int origin, Vector2Int destination, float waitDuration)
        {
            if (RoadManager.Instance == null || !RoadManager.Instance.HasRoads) return;
            if (GridSystem.Instance == null) return;

            var cfg = GameConfig.Instance;
            if (cfg != null && cfg.DisableSettlers) return;
            if (ActiveCount >= GetMaxSettlers()) return;
            if (Time.time - _lastDispatchTime < MinDispatchInterval) return;

            SettlerWalker walker = GetFreeSettler();
            if (walker == null) return;

            _lastDispatchTime = Time.time;
            ActiveCount++;
            walker.Dispatch(origin, destination, waitDuration, () =>
            {
                ActiveCount--;
            });
        }

        private void DispatchFromRandom(Vector2Int destination, float waitDuration)
        {
            var doors = RoadManager.Instance.GetActiveBuildingDoorCells();
            if (doors.Count == 0) return;
            Vector2Int origin = doors[Random.Range(0, doors.Count)];
            Dispatch(origin, destination, waitDuration);
        }

        private Building FindCommandCenter()
        {
            if (BuildingManager.Instance == null) return null;
            return BuildingManager.Instance.AllBuildings
                .FirstOrDefault(b => b.State == BuildingState.Active && b.Data.Type == BuildingType.CommandCenter);
        }

        private Building FindNearestStorage(Building source)
        {
            if (BuildingManager.Instance == null) return null;
            if (GridSystem.Instance == null) return null;

            Vector2Int srcCell = source.GetDoorCell();
            Building best = null;
            int bestDist = int.MaxValue;

            foreach (var b in BuildingManager.Instance.AllBuildings)
            {
                if (b.State != BuildingState.Active) continue;
                if (b.Data.Type != BuildingType.Storage && b.Data.Type != BuildingType.CommandCenter) continue;

                int dist = Mathf.Abs(b.GetDoorCell().x - srcCell.x) + Mathf.Abs(b.GetDoorCell().y - srcCell.y);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = b;
                }
            }

            return best;
        }

        private SettlerWalker GetFreeSettler()
        {
            foreach (var s in _pool)
            {
                if (s != null && !s.IsBusy && s.IsActive)
                    return s;
            }

            if (_pool.Count < GetMaxSettlers())
            {
                var walker = CreatePoolSettler();
                if (walker != null)
                    return walker;
            }

            return null;
        }

        private int GetMaxSettlers()
        {
            var cfg = GameConfig.Instance;
            int pop = GetPopulation();
            float ratio = cfg != null ? cfg.SettlersPerPopulation : 0.2f;
            int max = cfg != null ? cfg.MaxSettlers : 20;
            return Mathf.Min(Mathf.FloorToInt(pop * ratio), max);
        }

        private SettlerWalker CreatePoolSettler()
        {
            if (RoadManager.Instance == null || !RoadManager.Instance.HasRoads) return null;
            if (GridSystem.Instance == null) return null;

            var doors = RoadManager.Instance.GetActiveBuildingDoorCells();
            if (doors.Count == 0) return null;

            Vector2Int spawnCell = doors[Random.Range(0, doors.Count)];
            Vector3 worldPos = GridSystem.Instance.GetWorldPosition(spawnCell.x, spawnCell.y);

            var go = new GameObject("Settler");
            go.transform.SetParent(_settlerParent.transform);
            go.transform.position = new Vector3(worldPos.x, SettlerYOffset, worldPos.z);

            var walker = go.AddComponent<SettlerWalker>();
            float moveSpeed = GameConfig.Instance != null ? GameConfig.Instance.SettlerMoveSpeed : 2f;
            walker.Initialize(moveSpeed);

            CreateSettlerVisual(go.transform);
            go.SetActive(false);

            _pool.Add(walker);
            OnSettlerSpawned?.Invoke(walker);
            return walker;
        }

        private void CreateSettlerVisual(Transform parent)
        {
            var enabledModels = GetEnabledModels();
            if (enabledModels.Count == 0)
            {
                CreatePlaceholderVisual(parent);
                return;
            }

            GameObject model = enabledModels[Random.Range(0, enabledModels.Count)];
            if (model == null)
            {
                CreatePlaceholderVisual(parent);
                return;
            }

            var instance = Instantiate(model, parent);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

            foreach (var col in instance.GetComponentsInChildren<Collider>())
                col.enabled = false;

            FixMaterials(instance);

            RuntimeAnimatorController controller = GetControllerForModel(model);
            if (controller == null) return;

            Avatar avatar = null;
            var sourceAnimator = model.GetComponent<Animator>();
            if (sourceAnimator != null && sourceAnimator.avatar != null)
                avatar = sourceAnimator.avatar;

            Animator targetAnimator = null;
            foreach (var a in instance.GetComponentsInChildren<Animator>())
            {
                if (a.avatar != null && avatar == null)
                {
                    targetAnimator = a;
                    avatar = a.avatar;
                }
                else
                {
                    DestroyImmediate(a);
                }
            }

            if (targetAnimator == null)
            {
                targetAnimator = instance.gameObject.AddComponent<Animator>();
            }

            if (avatar != null)
                targetAnimator.avatar = avatar;

            targetAnimator.runtimeAnimatorController = GetControllerForModel(model);
            targetAnimator.applyRootMotion = false;
            targetAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            targetAnimator.Rebind();

            parent.GetComponent<SettlerWalker>()?.SetAnimator(targetAnimator);
        }

        private RuntimeAnimatorController GetControllerForModel(GameObject model)
        {
            if (model == null) return _animatorController;
            string name = model.name.ToLowerInvariant();

            if ((name.Contains("male_casual") || name.Contains("male_shirt") || name.Contains("man"))
                && _manAnimatorController != null)
                return _manAnimatorController;

            if ((name.Contains("female") || name.Contains("woman"))
                && _womanAnimatorController != null)
                return _womanAnimatorController;

            return _animatorController;
        }

        private List<GameObject> GetEnabledModels()
        {
            var cfg = GameConfig.Instance;
            var result = new List<GameObject>();
            if (_settlerModels == null) return result;

            for (int i = 0; i < _settlerModels.Length; i++)
            {
                if (_settlerModels[i] == null) continue;
                if (!IsModelEnabled(i, cfg)) continue;
                result.Add(_settlerModels[i]);
            }

            if (result.Count == 0)
            {
                foreach (var m in _settlerModels)
                    if (m != null) result.Add(m);
            }

            return result;
        }

        private static bool IsModelEnabled(int index, GameConfig cfg)
        {
            if (cfg == null) return true;
            return index switch
            {
                0 => cfg.EnableWorker,
                1 => cfg.EnableAdventurer,
                2 => cfg.EnableSuit,
                3 => cfg.EnableMan,
                4 => cfg.EnableWoman,
                _ => true
            };
        }

        private RuntimeAnimatorController GetControllerForModel(GameObject model)
        {
            if (model == null) return _animatorController;
            string name = model.name.ToLowerInvariant();

            if (name.Contains("male_casual") || name.Contains("male_shirt") || name.Contains("man"))
            {
                if (_manAnimatorController != null) return _manAnimatorController;
            }

            if (name.Contains("female") || name.Contains("woman"))
            {
                if (_womanAnimatorController != null) return _womanAnimatorController;
            }

            return _animatorController;
        }

        private static void FixMaterials(GameObject go)
        {
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) return;

            foreach (var renderer in go.GetComponentsInChildren<Renderer>())
            {
                var mats = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null || mats[i].shader == null) continue;
                    if (mats[i].shader.name.StartsWith("Universal Render Pipeline")) continue;

                    var newMat = new Material(urpLit);

                    string[] texProps = { "_MainTex", "_BaseMap", "_Albedo", "_Diffuse" };
                    foreach (var prop in texProps)
                    {
                        if (mats[i].HasProperty(prop) && mats[i].GetTexture(prop) != null)
                        {
                            newMat.SetTexture("_BaseMap", mats[i].GetTexture(prop));
                            break;
                        }
                    }

                    string[] colProps = { "_Color", "_BaseColor", "_AlbedoColor" };
                    foreach (var prop in colProps)
                    {
                        if (mats[i].HasProperty(prop))
                        {
                            newMat.SetColor("_BaseColor", mats[i].GetColor(prop));
                            break;
                        }
                    }

                    mats[i] = newMat;
                    changed = true;
                }
                if (changed)
                    renderer.sharedMaterials = mats;
            }
        }

        private static void CreatePlaceholderVisual(Transform parent)
        {
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(parent);
            visual.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            visual.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);

            var renderer = visual.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                renderer.material.color = GetRandomSettlerColor();
            }

            var collider = visual.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(parent);
            head.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            head.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            var headRenderer = head.GetComponent<MeshRenderer>();
            if (headRenderer != null)
            {
                headRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                headRenderer.material.color = new Color(0.85f, 0.75f, 0.65f);
            }

            var headCollider = head.GetComponent<Collider>();
            if (headCollider != null)
                headCollider.enabled = false;
        }

        private static readonly Color[] SettlerColors = {
            new(0.4f, 0.35f, 0.3f),
            new(0.3f, 0.4f, 0.35f),
            new(0.45f, 0.4f, 0.35f),
            new(0.35f, 0.35f, 0.4f),
            new(0.5f, 0.45f, 0.35f),
            new(0.3f, 0.35f, 0.3f),
        };

        private static Color GetRandomSettlerColor()
        {
            return SettlerColors[Random.Range(0, SettlerColors.Length)];
        }

        public int GetPopulation()
        {
            if (BuildingManager.Instance == null) return 0;
            int total = 0;
            int activeCount = 0;
            foreach (var b in BuildingManager.Instance.AllBuildings)
            {
                if (b.State == BuildingState.Active)
                {
                    total += b.Data.PopulationCapacity * b.Level;
                    activeCount++;
                }
            }
            if (total == 0 && activeCount > 1)
                total = activeCount;
            return total;
        }

        public void RemoveAllSettlers()
        {
            for (int i = _pool.Count - 1; i >= 0; i--)
            {
                if (_pool[i] != null)
                {
                    OnSettlerRemoved?.Invoke(_pool[i]);
                    Destroy(_pool[i].gameObject);
                }
            }
            _pool.Clear();
            ActiveCount = 0;
        }

        public List<SettlerWalkerSave> CaptureSettlersSave()
        {
            var saves = new List<SettlerWalkerSave>();
            foreach (var s in _pool)
            {
                if (s != null && s.IsActive)
                    saves.Add(s.CaptureSave());
            }
            return saves;
        }

        public void LoadSettlers(List<SettlerWalkerSave> saves)
        {
            RemoveAllSettlers();
            if (saves == null || saves.Count == 0) return;
            if (GridSystem.Instance == null) return;

            float moveSpeed = GameConfig.Instance != null ? GameConfig.Instance.SettlerMoveSpeed : 2f;

            foreach (var save in saves)
            {
                var go = new GameObject("Settler");
                go.transform.SetParent(_settlerParent.transform);

                var walker = go.AddComponent<SettlerWalker>();
                walker.Initialize(moveSpeed);
                walker.RestoreFromSave(save);

                CreateSettlerVisual(go.transform);

                _pool.Add(walker);
            }
        }
    }
}
