using System;
using HollowGround.Army;
using HollowGround.Domain.Walkers;
using HollowGround.Grid;
using HollowGround.Heroes;
using HollowGround.Roads;
using UnityEngine;

namespace HollowGround.NPCs
{
    public class PatrolWalker : WalkerBase
    {
        private readonly WalkerStateMachine _sm = new();

        private Hero _hero;
        private TroopType _troopType;
        private bool _isHeroWalker;
        private float _idleTimer;
        private float _idleDuration;
        private bool _patrolling;

        public Hero Hero => _hero;
        public TroopType TroopType => _troopType;
        public bool IsHeroWalker => _isHeroWalker;
        public bool IsPatrolling => _patrolling;
        public WalkerState CurrentState => _sm.State;

        public void InitializeHero(Hero hero, float moveSpeed)
        {
            base.Initialize(moveSpeed);
            _hero = hero;
            _isHeroWalker = true;
            _idleDuration = UnityEngine.Random.Range(8f, 20f);
        }

        public void InitializeTroop(TroopType type, float moveSpeed)
        {
            base.Initialize(moveSpeed);
            _troopType = type;
            _isHeroWalker = false;
            _idleDuration = UnityEngine.Random.Range(5f, 15f);
        }

        public void StartPatrol()
        {
            _patrolling = true;
            SnapToRandomRoadCell();
            _idleTimer = _idleDuration;
            gameObject.SetActive(true);
        }

        public void StopPatrol()
        {
            _patrolling = false;
            ClearPath();
            _sm.Deactivate();
            SetAnimSpeed(0f);

            if (WalkerManager.Instance != null)
                WalkerManager.Instance.Unregister(this);

            gameObject.SetActive(false);
        }

        protected override void OnTick(float dt, float gameSpeed)
        {
            if (!_patrolling) return;

            if (_sm.State == WalkerState.None)
            {
                _idleTimer += dt * gameSpeed;
                if (_idleTimer >= _idleDuration)
                {
                    _idleTimer = 0f;
                    _idleDuration = UnityEngine.Random.Range(8f, 20f);
                    PickNewDestination();
                }
                return;
            }

            var result = _sm.Tick(dt, gameSpeed);

            switch (result)
            {
                case TickResult.Walking:
                    bool complete = TickMovement(dt, gameSpeed);
                    if (complete)
                    {
                        ClearPath();
                        _sm.OnPathComplete();

                        if (_sm.State == WalkerState.WaitingAtTarget)
                        {
                            SetAnimSpeed(0f);
                            _sm.SetWaitDuration(UnityEngine.Random.Range(3f, 10f));
                        }
                    }
                    break;

                case TickResult.WaitComplete:
                    _sm.Deactivate();
                    SetAnimSpeed(0f);
                    break;
            }
        }

        private void PickNewDestination()
        {
            var doors = RoadManager.Instance?.GetActiveBuildingDoorCells();
            if (doors == null || doors.Count == 0) return;
            if (GridSystem.Instance == null) return;

            Vector2Int currentCell = GridSystem.Instance.GetGridCoordinates(transform.position);
            Vector2Int destCell = doors[UnityEngine.Random.Range(0, doors.Count)];

            if (currentCell == destCell)
            {
                int attempts = 0;
                while (currentCell == destCell && attempts < 5)
                {
                    destCell = doors[UnityEngine.Random.Range(0, doors.Count)];
                    attempts++;
                }
                if (currentCell == destCell) return;
            }

            var path = FindPath(currentCell, destCell);
            if (path == null || path.Count < 2) return;

            _sm.SetHomeCell(currentCell.x, currentCell.y);
            _sm.AssignJob(currentCell.x, currentCell.y, 5f);
            _sm.StartWalkingToTarget((destCell.x, destCell.y));

            SetPath(path);
            SetAnimSpeed(1f);
        }

        private void SnapToRandomRoadCell()
        {
            var doors = RoadManager.Instance?.GetActiveBuildingDoorCells();
            if (doors == null || doors.Count == 0) return;
            if (GridSystem.Instance == null) return;

            Vector2Int cell = doors[UnityEngine.Random.Range(0, doors.Count)];
            SnapToCell(cell.x, cell.y);
        }

        public string GetDisplayName()
        {
            if (_isHeroWalker && _hero != null)
                return _hero.Data.DisplayName;
            if (!_isHeroWalker)
                return _troopType.ToString();
            return "Patrol";
        }

        public string GetRoleLabel()
        {
            if (_isHeroWalker && _hero != null)
                return _hero.Data.Role.ToString();
            return _isHeroWalker ? "Hero" : "Soldier";
        }
    }
}
