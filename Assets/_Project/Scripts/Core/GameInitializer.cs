using System.Collections.Generic;
using System.Linq;
using HollowGround.Army;
using HollowGround.Buildings;
using HollowGround.Combat;
using HollowGround.Grid;
using HollowGround.Heroes;
using HollowGround.NPCs;
using HollowGround.Quests;
using HollowGround.Roads;
using UnityEngine;

namespace HollowGround.Core
{
    [DefaultExecutionOrder(-50)]
    public class GameInitializer : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private bool _centerCameraOnStart = true;

        [Header("Quests")]
        [SerializeField] private List<QuestData> _questPool = new();

        [Header("Terrain")]
        [SerializeField] private MapTemplate _mapTemplate;
        [SerializeField] private bool _applyTerrainOnStart = true;

        [Header("Starting Buildings")]
        [SerializeField] private BuildingData _commandCenterData;
        [SerializeField] private BuildingData _farmData;
        [SerializeField] private BuildingData _woodFactoryData;
        [SerializeField] private BuildingData _waterWellData;

        [Header("Starting Positions (overridden by GameConfig.StartingCCPosition)")]
        [SerializeField] private Vector2Int _ccPos = new(6, 6);
        [SerializeField] private Vector2Int _farmOffset = new(2, 0);
        [SerializeField] private Vector2Int _woodOffset = new(0, 2);
        [SerializeField] private Vector2Int _waterOffset = new(2, 2);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureExists()
        {
            if (FindAnyObjectByType<GameInitializer>() != null) return;

            var go = GameObject.Find("GameInitializer");
            if (go == null)
            {
                go = new GameObject("GameInitializer");
                Debug.LogWarning("[GameInitializer] Created missing GameInitializer at runtime.");
            }
            go.AddComponent<GameInitializer>();
        }

        private void Start()
        {
            ResetAllState();
            EnsureSessionLogger();
            EnsureBuildingPlacer();
            ApplyTerrain();
            PlaceStartingBuildings();
            ResetSettlers();
            CenterCamera();
            InitializeWorldMap();
            InitializeQuests();
            InitializeMutantAttacks();
            StartGame();
        }

        private void ResetAllState()
        {
            ResetTechNodes();
            ResetBuildings();
            ResetArmy();
            ResetHeroes();
        }

        private void ResetTechNodes()
        {
            var techNodes = UnityEngine.Resources.LoadAll<Tech.TechNode>("TechNodes");
            foreach (var tech in techNodes)
            {
                tech.IsResearched = false;
                tech.IsResearching = false;
                tech.ResearchProgress = 0f;
            }
        }

        private void ResetBuildings()
        {
            if (BuildingManager.Instance == null) return;
            var existing = BuildingManager.Instance.AllBuildings.ToList();
            foreach (var b in existing)
                b.Demolish();

            if (RoadManager.Instance != null)
                RoadManager.Instance.ClearAllRoads();
        }

        private void ResetArmy()
        {
            if (ArmyManager.Instance == null) return;
            ArmyManager.Instance.ResetAll();
        }

        private void ResetHeroes()
        {
            if (HeroManager.Instance == null) return;
            HeroManager.Instance.ResetAll();
        }

        private void EnsureSessionLogger()
        {
            if (FindAnyObjectByType<SessionLogger>() == null)
                gameObject.AddComponent<SessionLogger>();
        }

        private void EnsureBuildingPlacer()
        {
            if (BuildingPlacer.Instance != null) return;
            if (FindAnyObjectByType<BuildingPlacer>() != null)
            {
                Debug.LogWarning("[GameInitializer] BuildingPlacer component found but Instance is null — possible hot reload artifact.");
                return;
            }
            gameObject.AddComponent<BuildingPlacer>();
            Debug.Log("[GameInitializer] BuildingPlacer was missing, added to Managers.");
        }

        private void PlaceStartingBuildings()
        {
            var cfg = GameConfig.Instance;
            Vector2Int cc = cfg != null ? cfg.StartingCCPosition : _ccPos;

            PlaceBuilding(_commandCenterData, cc);
            PlaceBuilding(_farmData, cc + _farmOffset);
            PlaceBuilding(_woodFactoryData, cc + _woodOffset);
            PlaceBuilding(_waterWellData, cc + _waterOffset);
        }

        private void PlaceBuilding(BuildingData data, Vector2Int gridPos)
        {
            if (data == null) return;
            if (GridSystem.Instance == null) return;
            if (!GridSystem.Instance.IsAreaBuildable(gridPos.x, gridPos.y, data.SizeX, data.SizeZ)) return;

            Building building = Building.Create(data, gridPos);
            int sx = data.SizeX;
            int sz = data.SizeZ;

            GridSystem.Instance.OccupyCells(gridPos.x, gridPos.y, sx, sz, building.gameObject);
            RoadManager.Instance?.RemoveRoadCellsUnderBuilding(building);

            if (BuildingManager.Instance != null)
                BuildingManager.Instance.RegisterBuilding(building);
        }

        private void CenterCamera()
        {
            if (!_centerCameraOnStart) return;

            var strategyCam = FindAnyObjectByType<HollowGround.Camera.StrategyCamera>();
            if (strategyCam == null) return;

            var cfg = GameConfig.Instance;
            Vector2Int gridPos;

            if (cfg != null && cfg.UseCustomCameraPosition)
                gridPos = cfg.StartingCameraPosition;
            else if (cfg != null)
                gridPos = cfg.StartingCCPosition;
            else
                gridPos = _ccPos;

            Vector3 target;
            if (GridSystem.Instance != null)
                target = GridSystem.Instance.GetWorldPosition(gridPos.x, gridPos.y);
            else
                target = new Vector3(gridPos.x * 2f, 0f, gridPos.y * 2f);

            strategyCam.transform.position = target;
            strategyCam.FocusOn(target);
        }

        private void InitializeWorldMap()
        {
            if (HollowGround.World.WorldMap.Instance == null) return;
            if (HollowGround.World.WorldMap.Instance.AllNodes.Count > 0) return;

            HollowGround.World.WorldMap.Instance.GenerateDefaultMap();
        }

        private void InitializeQuests()
        {
            if (QuestManager.Instance == null) return;
            if (_questPool == null || _questPool.Count == 0) return;

            QuestManager.Instance.LoadQuests(_questPool);
        }

        private void InitializeMutantAttacks()
        {
            if (MutantAttackManager.Instance == null) return;
            MutantAttackManager.Instance.StartAttackCycle();
        }

        private void StartGame()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.StartGame();
        }

        private void ResetSettlers()
        {
            if (WalkerManager.Instance != null)
                WalkerManager.Instance.ClearRecyclePool();
            if (SettlerManager.Instance == null) return;
            SettlerManager.Instance.RemoveAllSettlers();
        }

        private void ApplyTerrain()
        {
            if (!_applyTerrainOnStart) return;
            if (GridSystem.Instance == null) return;
            if (_mapTemplate != null)
                GridSystem.Instance.ApplyMapTemplate(_mapTemplate);
        }
    }
}
