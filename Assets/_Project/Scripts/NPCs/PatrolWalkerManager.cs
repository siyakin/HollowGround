using System.Collections.Generic;
using HollowGround.Army;
using HollowGround.Core;
using HollowGround.Grid;
using HollowGround.Heroes;
using HollowGround.Roads;
using UnityEngine;

namespace HollowGround.NPCs
{
    public class PatrolWalkerManager : Singleton<PatrolWalkerManager>
    {
        private readonly List<PatrolWalker> _heroWalkers = new();
        private readonly List<PatrolWalker> _troopWalkers = new();
        private GameObject _walkerParent;
        private float _spawnTimer;
        private const float SpawnCheckInterval = 15f;
        private const int MaxTroopWalkers = 5;
        private const float PatrolMoveSpeed = 1.8f;

        protected override void Awake()
        {
            base.Awake();
            _walkerParent = new GameObject("PatrolWalkers");
            _walkerParent.transform.SetParent(transform);
        }

        private void Start()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (HeroManager.Instance != null)
            {
                HeroManager.Instance.OnHeroAdded += OnHeroAdded;
                HeroManager.Instance.OnHeroRemoved += OnHeroRemoved;
                HeroManager.Instance.OnHeroesChanged += OnHeroesChanged;
            }

            if (ArmyManager.Instance != null)
                ArmyManager.Instance.OnArmyUpdated += OnArmyUpdated;
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Paused) return;
            if (TimeManager.Instance != null && TimeManager.Instance.IsPaused) return;

            if (RoadManager.Instance == null || !RoadManager.Instance.HasRoads) return;

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= SpawnCheckInterval)
            {
                _spawnTimer = 0f;
                RefreshTroopWalkers();
            }
        }

        private void OnHeroAdded(Hero hero)
        {
            if (hero.IsDeployed || hero.IsInjured) return;
            SpawnHeroWalker(hero);
        }

        private void OnHeroRemoved(Hero hero)
        {
            RemoveHeroWalker(hero);
        }

        private void OnHeroesChanged()
        {
            SyncHeroWalkers();
        }

        private void OnArmyUpdated()
        {
            RefreshTroopWalkers();
        }

        private void SyncHeroWalkers()
        {
            if (HeroManager.Instance == null) return;

            var activeHeroes = HeroManager.Instance.GetAvailableHeroes();
            var existingIds = new HashSet<string>();
            foreach (var w in _heroWalkers)
                if (w.Hero != null) existingIds.Add(w.Hero.Id);

            foreach (var hero in activeHeroes)
            {
                if (!existingIds.Contains(hero.Id))
                    SpawnHeroWalker(hero);
            }

            var deployedIds = new HashSet<string>();
            foreach (var hero in HeroManager.Instance.GetDeployedHeroes())
                deployedIds.Add(hero.Id);
            foreach (var hero in HeroManager.Instance.AllHeroes)
                if (hero.IsInjured) deployedIds.Add(hero.Id);

            for (int i = _heroWalkers.Count - 1; i >= 0; i--)
            {
                var w = _heroWalkers[i];
                if (w.Hero == null || deployedIds.Contains(w.Hero.Id))
                    RemoveHeroWalkerAt(i);
            }
        }

        private void RefreshTroopWalkers()
        {
            if (ArmyManager.Instance == null) return;
            if (RoadManager.Instance == null || !RoadManager.Instance.HasRoads) return;

            int totalTroops = ArmyManager.Instance.TotalTroopCount;
            int desired = Mathf.Min(Mathf.CeilToInt(totalTroops / 10f), MaxTroopWalkers);

            while (_troopWalkers.Count < desired)
                SpawnTroopWalker();

            while (_troopWalkers.Count > desired)
                RemoveTroopWalkerAt(_troopWalkers.Count - 1);
        }

        private void SpawnHeroWalker(Hero hero)
        {
            if (hero == null || hero.Data == null) return;
            if (RoadManager.Instance == null || !RoadManager.Instance.HasRoads) return;
            if (GridSystem.Instance == null) return;

            var go = new GameObject($"Hero_{hero.Data.DisplayName}");
            go.transform.SetParent(_walkerParent.transform);

            var walker = go.AddComponent<PatrolWalker>();
            walker.InitializeHero(hero, PatrolMoveSpeed);

            var clickCol = go.AddComponent<SphereCollider>();
            clickCol.radius = 0.8f;
            clickCol.center = new Vector3(0f, 0.7f, 0f);

            CreateWalkerVisual(go.transform, hero.Data.ModelPrefab, hero.Data.Role.ToString());

            if (WalkerManager.Instance != null)
                WalkerManager.Instance.Register(walker);

            walker.StartPatrol();
            _heroWalkers.Add(walker);
        }

        private void SpawnTroopWalker()
        {
            if (RoadManager.Instance == null || !RoadManager.Instance.HasRoads) return;
            if (GridSystem.Instance == null) return;

            TroopType? typeOpt = PickRandomTroopType();
            if (typeOpt == null) return;
            TroopType type = typeOpt.Value;

            var data = FindTroopData(type);
            var prefab = data != null ? data.UnitPrefab : null;

            var go = new GameObject($"Troop_{type}");
            go.transform.SetParent(_walkerParent.transform);

            var walker = go.AddComponent<PatrolWalker>();
            walker.InitializeTroop(type, PatrolMoveSpeed);

            var clickCol = go.AddComponent<SphereCollider>();
            clickCol.radius = 0.8f;
            clickCol.center = new Vector3(0f, 0.7f, 0f);

            CreateWalkerVisual(go.transform, prefab, type.ToString());

            if (WalkerManager.Instance != null)
                WalkerManager.Instance.Register(walker);

            walker.StartPatrol();
            _troopWalkers.Add(walker);
        }

        private void CreateWalkerVisual(Transform parent, GameObject modelPrefab, string label)
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

                var sourceAnimator = modelPrefab.GetComponent<Animator>();
                Avatar avatar = sourceAnimator != null ? sourceAnimator.avatar : null;

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

            CreatePlaceholderVisual(parent, label);
        }

        private void CreatePlaceholderVisual(Transform parent, string label)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) return;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(parent);
            visual.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            visual.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);

            var mat = new Material(shader);
            mat.color = label.Contains("Hero") ? new Color(0.2f, 0.4f, 0.7f) : new Color(0.4f, 0.3f, 0.2f);
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

        private void RemoveHeroWalker(Hero hero)
        {
            if (hero == null) return;
            for (int i = _heroWalkers.Count - 1; i >= 0; i--)
            {
                if (_heroWalkers[i].Hero != null && _heroWalkers[i].Hero.Id == hero.Id)
                    RemoveHeroWalkerAt(i);
            }
        }

        private void RemoveHeroWalkerAt(int index)
        {
            if (index < 0 || index >= _heroWalkers.Count) return;
            var walker = _heroWalkers[index];
            walker.StopPatrol();
            _heroWalkers.RemoveAt(index);
            Destroy(walker.gameObject);
        }

        private void RemoveTroopWalkerAt(int index)
        {
            if (index < 0 || index >= _troopWalkers.Count) return;
            var walker = _troopWalkers[index];
            walker.StopPatrol();
            _troopWalkers.RemoveAt(index);
            Destroy(walker.gameObject);
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
            return types[Random.Range(0, types.Count)];
        }

        private TroopData FindTroopData(TroopType type)
        {
            if (ArmyManager.Instance == null) return null;
            foreach (var td in ArmyManager.Instance.AllTroopData)
                if (td.Type == type) return td;
            return null;
        }

        public void RemoveAllWalkers()
        {
            foreach (var w in _heroWalkers)
            {
                if (w != null)
                {
                    w.StopPatrol();
                    Destroy(w.gameObject);
                }
            }
            _heroWalkers.Clear();

            foreach (var w in _troopWalkers)
            {
                if (w != null)
                {
                    w.StopPatrol();
                    Destroy(w.gameObject);
                }
            }
            _troopWalkers.Clear();
        }

        public IReadOnlyList<PatrolWalker> HeroWalkers => _heroWalkers;
        public IReadOnlyList<PatrolWalker> TroopWalkers => _troopWalkers;
        public int TotalPatrolCount => _heroWalkers.Count + _troopWalkers.Count;
    }
}
