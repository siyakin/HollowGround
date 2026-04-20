using System.Collections.Generic;
using HollowGround.Combat;
using HollowGround.Grid;
using HollowGround.Quests;
using UnityEngine;

namespace HollowGround.Core
{
    public class GameInitializer : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private bool _centerCameraOnStart = true;

        [Header("Quests")]
        [SerializeField] private List<QuestData> _questPool = new();

        private void Start()
        {
            CenterCamera();
            InitializeWorldMap();
            InitializeQuests();
            InitializeMutantAttacks();
            StartGame();
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
    }
}
