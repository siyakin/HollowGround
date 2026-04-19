using System.Collections.Generic;
using HollowGround.Combat;
using HollowGround.Core;
using HollowGround.Quests;
using UnityEngine;

namespace HollowGround.Core
{
    public class GameInitializer : MonoBehaviour
    {
        [Header("Quests")]
        [SerializeField] private List<QuestData> _questPool = new();

        private void Start()
        {
            InitializeQuests();
            InitializeMutantAttacks();
            StartGame();
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
