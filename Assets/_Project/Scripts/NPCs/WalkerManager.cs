using System;
using System.Collections.Generic;
using HollowGround.Army;
using HollowGround.Core;
using HollowGround.Domain.Walkers;
using HollowGround.Grid;
using HollowGround.Heroes;
using HollowGround.Roads;
using UnityEngine;

namespace HollowGround.NPCs
{
    public class WalkerManager : Singleton<WalkerManager>
    {
        private readonly List<WalkerBase> _walkers = new();
        private readonly Dictionary<(Vector2Int, Vector2Int), List<Vector2Int>> _pathCache = new();
        private readonly Dictionary<Vector2Int, WalkerBase> _occupiedCells = new();
        private readonly Stack<SettlerWalker> _recyclePool = new();

        private readonly List<PatrolWalker> _heroPatrols = new();
        private readonly List<PatrolWalker> _troopPatrols = new();
        private readonly PatrolScheduler _scheduler = new();
        private float _patrolSpawnTimer;

        public int WalkerCount => _walkers.Count;
        public int ActiveWalkerCount
        {
            get
            {
                int count = 0;
                foreach (var w in _walkers)
                    if (w != null && w.IsActive) count++;
                return count;
            }
        }
        public int RecycledCount => _recyclePool.Count;
        public int OccupiedCellCount => _occupiedCells.Count;
        public IReadOnlyList<PatrolWalker> HeroPatrols => _heroPatrols;
        public IReadOnlyList<PatrolWalker> TroopPatrols => _troopPatrols;

        private void Start()
        {
            if (RoadManager.Instance != null)
                RoadManager.Instance.OnRoadsChanged += InvalidatePathCache;

            SubscribePatrolEvents();
        }

        protected override void OnDestroy()
        {
            if (RoadManager.Instance != null)
                RoadManager.Instance.OnRoadsChanged -= InvalidatePathCache;

            UnsubscribePatrolEvents();
            base.OnDestroy();
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Paused) return;
            if (TimeManager.Instance != null && TimeManager.Instance.IsPaused) return;

            float dt = Time.deltaTime;
            float speed = TimeManager.Instance != null ? TimeManager.Instance.GameSpeed : 1f;
            if (speed <= 0f) return;

            for (int i = _walkers.Count - 1; i >= 0; i--)
            {
                if (_walkers[i] != null)
                    _walkers[i].Tick(dt, speed);
            }

            TickPatrolSpawning(dt);
        }

        #region Registration

        public void Register(WalkerBase walker)
        {
            if (walker == null || _walkers.Contains(walker)) return;
            _walkers.Add(walker);
        }

        public void Unregister(WalkerBase walker)
        {
            if (walker == null) return;
            _walkers.Remove(walker);
            if (walker is SettlerWalker sw)
                ReleaseOccupiedCell(sw);
        }

        public void Recycle(SettlerWalker walker)
        {
            if (walker == null) return;
            _walkers.Remove(walker);
            ReleaseOccupiedCell(walker);
            walker.gameObject.SetActive(false);
            _recyclePool.Push(walker);
        }

        public SettlerWalker GetRecycled()
        {
            if (_recyclePool.Count == 0) return null;
            var walker = _recyclePool.Pop();
            if (walker == null || walker.gameObject == null) return null;
            walker.gameObject.SetActive(false);
            return walker;
        }

        public void ClearRecyclePool()
        {
            while (_recyclePool.Count > 0)
            {
                var walker = _recyclePool.Pop();
                if (walker != null && walker.gameObject != null)
                    Destroy(walker.gameObject);
            }
        }

        #endregion

        #region Cell Occupancy

        public bool IsCellOccupied(Vector2Int cell)
        {
            return _occupiedCells.ContainsKey(cell);
        }

        public bool TryOccupyCell(Vector2Int cell, WalkerBase walker)
        {
            if (_occupiedCells.TryGetValue(cell, out var occupant))
            {
                if (occupant != null && occupant != walker && occupant.IsActive)
                    return false;
                _occupiedCells.Remove(cell);
            }
            _occupiedCells[cell] = walker;
            return true;
        }

        public void ReleaseOccupiedCell(SettlerWalker walker)
        {
            if (walker == null) return;
            var cell = walker.GetGridPosition();
            if (_occupiedCells.TryGetValue(cell, out var occupant) && occupant == walker)
                _occupiedCells.Remove(cell);
        }

        public void UpdateOccupancy(SettlerWalker walker, Vector2Int previousCell, Vector2Int newCell)
        {
            if (previousCell == newCell) return;
            if (_occupiedCells.TryGetValue(previousCell, out var prev) && prev == walker)
                _occupiedCells.Remove(previousCell);
            TryOccupyCell(newCell, walker);
        }

        #endregion

        #region Pathfinding

        public List<Vector2Int> RequestPath(Vector2Int start, Vector2Int end)
        {
            var key = (start, end);
            if (_pathCache.TryGetValue(key, out var cached))
                return cached;

            if (RoadManager.Instance == null) return null;

            var path = RoadManager.Instance.FindPublicPath(start, end);
            if (path != null)
                _pathCache[key] = path;

            return path;
        }

        public void InvalidatePathCache()
        {
            _pathCache.Clear();
        }

        #endregion

        #region Patrol Walkers

        private void SubscribePatrolEvents()
        {
            if (HeroManager.Instance != null)
            {
                HeroManager.Instance.OnHeroAdded += OnHeroAdded;
                HeroManager.Instance.OnHeroRemoved += OnHeroRemoved;
                HeroManager.Instance.OnHeroesChanged += SyncHeroPatrols;
            }

            if (ArmyManager.Instance != null)
                ArmyManager.Instance.OnArmyUpdated += SyncTroopPatrols;
        }

        private void UnsubscribePatrolEvents()
        {
            if (HeroManager.Instance != null)
            {
                HeroManager.Instance.OnHeroAdded -= OnHeroAdded;
                HeroManager.Instance.OnHeroRemoved -= OnHeroRemoved;
                HeroManager.Instance.OnHeroesChanged -= SyncHeroPatrols;
            }

            if (ArmyManager.Instance != null)
                ArmyManager.Instance.OnArmyUpdated -= SyncTroopPatrols;
        }

        private void TickPatrolSpawning(float dt)
        {
            if (RoadManager.Instance == null || !RoadManager.Instance.HasRoads) return;

            _patrolSpawnTimer += dt;
            if (_patrolSpawnTimer >= PatrolScheduler.SpawnCheckInterval)
            {
                _patrolSpawnTimer = 0f;
                SyncTroopPatrols();
            }
        }

        private void OnHeroAdded(Hero hero)
        {
            SyncHeroPatrols();
        }

        private void OnHeroRemoved(Hero hero)
        {
            for (int i = _heroPatrols.Count - 1; i >= 0; i--)
            {
                if (_heroPatrols[i].Hero != null && _heroPatrols[i].Hero.Id == hero.Id)
                    RemovePatrolAt(_heroPatrols, i);
            }
        }

        private void SyncHeroPatrols()
        {
            if (HeroManager.Instance == null) return;

            var available = HeroManager.Instance.GetAvailableHeroes();
            int desired = _scheduler.GetDesiredHeroWalkerCount(available);

            var deployedIds = new HashSet<string>();
            foreach (var h in HeroManager.Instance.GetDeployedHeroes()) deployedIds.Add(h.Id);
            foreach (var h in HeroManager.Instance.AllHeroes) if (h.IsInjured) deployedIds.Add(h.Id);

            for (int i = _heroPatrols.Count - 1; i >= 0; i--)
            {
                var w = _heroPatrols[i];
                if (w.Hero == null || deployedIds.Contains(w.Hero.Id))
                    RemovePatrolAt(_heroPatrols, i);
            }

            var existingIds = new HashSet<string>();
            foreach (var w in _heroPatrols) if (w.Hero != null) existingIds.Add(w.Hero.Id);

            foreach (var hero in available)
            {
                if (!existingIds.Contains(hero.Id) && _heroPatrols.Count < desired)
                    SpawnHeroPatrol(hero);
            }
        }

        private void SyncTroopPatrols()
        {
            if (ArmyManager.Instance == null) return;
            if (RoadManager.Instance == null || !RoadManager.Instance.HasRoads) return;

            int desired = _scheduler.GetDesiredTroopWalkerCount(ArmyManager.Instance.TotalTroopCount);

            while (_scheduler.ShouldSpawn(_troopPatrols.Count, desired))
                SpawnTroopPatrol();

            while (_scheduler.ShouldDespawn(_troopPatrols.Count, desired))
                RemovePatrolAt(_troopPatrols, _troopPatrols.Count - 1);
        }

        private void SpawnHeroPatrol(Hero hero)
        {
            if (hero?.Data == null) return;
            if (RoadManager.Instance == null || !RoadManager.Instance.HasRoads) return;

            var go = new GameObject($"Hero_{hero.Data.DisplayName}");
            go.transform.SetParent(transform);

            var walker = go.AddComponent<PatrolWalker>();
            walker.InitializeHero(hero, PatrolScheduler.PatrolMoveSpeed);

            AddClickCollider(go);
            CreatePatrolVisual(go.transform, hero.Data.ModelPrefab, true);

            Register(walker);
            walker.StartPatrol();
            _heroPatrols.Add(walker);
        }

        private void SpawnTroopPatrol()
        {
            if (RoadManager.Instance == null || !RoadManager.Instance.HasRoads) return;

            TroopType? typeOpt = PickRandomTroopType();
            if (typeOpt == null) return;
            TroopType type = typeOpt.Value;

            var data = FindTroopData(type);
            var prefab = data != null ? data.UnitPrefab : null;

            var go = new GameObject($"Troop_{type}");
            go.transform.SetParent(transform);

            var walker = go.AddComponent<PatrolWalker>();
            walker.InitializeTroop(type, PatrolScheduler.PatrolMoveSpeed);

            AddClickCollider(go);
            CreatePatrolVisual(go.transform, prefab, false);

            Register(walker);
            walker.StartPatrol();
            _troopPatrols.Add(walker);
        }

        private void RemovePatrolAt(List<PatrolWalker> list, int index)
        {
            if (index < 0 || index >= list.Count) return;
            var walker = list[index];
            walker.StopPatrol();
            Unregister(walker);
            list.RemoveAt(index);
            Destroy(walker.gameObject);
        }

        private void AddClickCollider(GameObject go)
        {
            var col = go.AddComponent<SphereCollider>();
            col.radius = 0.8f;
            col.center = new Vector3(0f, 0.7f, 0f);
        }

        private void CreatePatrolVisual(Transform parent, GameObject modelPrefab, bool isHero)
        {
            if (modelPrefab != null)
            {
                var instance = Instantiate(modelPrefab, parent);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

                foreach (var col in instance.GetComponentsInChildren<Collider>())
                    col.enabled = false;

                FixMaterials(instance);

                Avatar avatar = null;
                var sourceAnimator = modelPrefab.GetComponent<Animator>();
                if (sourceAnimator != null) avatar = sourceAnimator.avatar;

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
                    targetAnimator = instance.gameObject.AddComponent<Animator>();

                if (avatar != null)
                    targetAnimator.avatar = avatar;

                var controller = SettlerManager.Instance?.AnimatorController;
                if (controller != null)
                {
                    targetAnimator.runtimeAnimatorController = controller;
                    targetAnimator.applyRootMotion = false;
                    targetAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    targetAnimator.Rebind();
                    parent.GetComponent<PatrolWalker>()?.SetAnimator(targetAnimator);
                }

                return;
            }

            CreatePlaceholderVisual(parent, isHero);
        }

        private void CreatePlaceholderVisual(Transform parent, bool isHero)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) return;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(parent);
            visual.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            visual.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);

            var mat = new Material(shader);
            mat.color = isHero ? new Color(0.2f, 0.4f, 0.7f) : new Color(0.4f, 0.3f, 0.2f);
            var renderer = visual.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.sharedMaterial = mat;

            var collider = visual.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
        }

        private void FixMaterials(GameObject go)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) return;

            foreach (var renderer in go.GetComponentsInChildren<Renderer>())
            {
                var mats = renderer.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] != null && mats[i].shader != null &&
                        !mats[i].shader.name.StartsWith("Universal Render Pipeline"))
                    {
                        var converted = new Material(shader);
                        if (mats[i].HasProperty("_MainTex") && mats[i].GetTexture("_MainTex") != null)
                            converted.SetTexture("_BaseMap", mats[i].GetTexture("_MainTex"));
                        if (mats[i].HasProperty("_Color"))
                            converted.SetColor("_BaseColor", mats[i].GetColor("_Color"));
                        mats[i] = converted;
                    }
                }
                renderer.sharedMaterials = mats;
            }
        }

        private TroopType? PickRandomTroopType()
        {
            if (ArmyManager.Instance == null) return null;

            var types = new List<TroopType>();
            foreach (TroopType type in System.Enum.GetValues(typeof(TroopType)))
            {
                if (ArmyManager.Instance.GetTroopCount(type) > 0)
                    types.Add(type);
            }

            if (types.Count == 0) return null;
            return types[UnityEngine.Random.Range(0, types.Count)];
        }

        private TroopData FindTroopData(TroopType type)
        {
            if (ArmyManager.Instance == null) return null;
            foreach (var td in ArmyManager.Instance.AllTroopData)
                if (td.Type == type) return td;
            return null;
        }

        public void RemoveAllPatrolWalkers()
        {
            for (int i = _heroPatrols.Count - 1; i >= 0; i--)
                RemovePatrolAt(_heroPatrols, i);

            for (int i = _troopPatrols.Count - 1; i >= 0; i--)
                RemovePatrolAt(_troopPatrols, i);
        }

        #endregion
    }
}
