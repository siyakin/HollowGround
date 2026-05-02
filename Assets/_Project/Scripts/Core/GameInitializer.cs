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
    public class GameInitializer : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private bool _centerCameraOnStart = true;

        [Header("Quests")]
        [SerializeField] private List<QuestData> _questPool = new();

        [Header("Terrain")]
        [SerializeField] private MapTemplate _mapTemplate;
        [SerializeField] private bool _applyTerrainOnStart = false;

        private void Start()
        {
            ResetAllState();
            EnsureSessionLogger();
            ApplyTerrain();
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

        private void CenterCamera()
        {
            if (!_centerCameraOnStart) return;

            var strategyCam = FindAnyObjectByType<HollowGround.Camera.StrategyCamera>();
            if (strategyCam == null) return;

            Vector3 target;
            if (GridSystem.Instance != null)
            {
                float cx = GridSystem.Instance.Width * GridSystem.Instance.CellSize * 0.5f;
                float cz = GridSystem.Instance.Height * GridSystem.Instance.CellSize * 0.5f;
                target = new Vector3(cx, 0f, cz);
            }
            else
            {
                target = new Vector3(50f, 0f, 50f);
            }

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
            if (SettlerManager.Instance == null) return;
            SettlerManager.Instance.RemoveAllSettlers();
        }

        private void ApplyTerrain()
        {
            if (!_applyTerrainOnStart) return;
            if (GridSystem.Instance == null) return;
            GridSystem.Instance.ClearTerrain();

            if (_mapTemplate != null)
            {
                GridSystem.Instance.ApplyMapTemplate(_mapTemplate);
            }
        }
    }
}
