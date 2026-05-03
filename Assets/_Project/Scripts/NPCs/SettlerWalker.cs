using System;
using HollowGround.Buildings;
using HollowGround.Core;
using HollowGround.Domain.Walkers;
using HollowGround.Grid;
using HollowGround.Roads;
using UnityEngine;

namespace HollowGround.NPCs
{
    public class SettlerWalker : WalkerBase
    {
        private readonly WalkerStateMachine _sm = new();
        private Action _onDone;

        private SettlerRole _role = SettlerRole.None;
        private Building _assignedBuilding;

        public WalkerState CurrentTask => _sm.State;
        public bool IsBusy => _sm.State != WalkerState.None;
        public SettlerRole Role => _role;
        public Building AssignedBuilding => _assignedBuilding;
        public bool HasJob => _assignedBuilding != null && _role != SettlerRole.None;

        public override void Initialize(float moveSpeed)
        {
            base.Initialize(moveSpeed);
            var cfg = GameConfig.Instance;
            if (cfg != null)
                _sm.SetRestDuration(cfg.SettlerRestDuration);
        }

        public void AssignJob(SettlerRole role, Building building)
        {
            _role = role;
            _assignedBuilding = building;
            _onDone = null;
            StartWorkCycle();
        }

        public void ReassignJob(SettlerRole role, Building building)
        {
            _role = role;
            _assignedBuilding = building;
            _onDone = null;
            WalkToNewBuilding(building);
        }

        public void ResetForReuse()
        {
            ClearPath();
            _onDone = null;
            _sm.ClearJob();
            _role = SettlerRole.None;
            _assignedBuilding = null;
            _active = true;
            SetAnimSpeed(0f);
            if (WalkerManager.Instance != null)
                WalkerManager.Instance.ReleaseOccupiedCell(this);
        }

        public void ClearJob()
        {
            _role = SettlerRole.None;
            _assignedBuilding = null;
            ClearPath();
            _onDone = null;
            _sm.ClearJob();
            SetAnimSpeed(0f);
            if (WalkerManager.Instance != null)
                WalkerManager.Instance.ReleaseOccupiedCell(this);
            gameObject.SetActive(false);
        }

        public void ClearPathOnly()
        {
            ClearPath();
            _sm.ClearJob();
            SetAnimSpeed(1f);
        }

        private void StartWorkCycle()
        {
            if (_assignedBuilding == null || GridSystem.Instance == null) return;
            if (_assignedBuilding.State == BuildingState.Destroyed)
            {
                ClearJob();
                return;
            }

            var cfg = GameConfig.Instance;
            float workDuration = cfg != null ? cfg.SettlerWorkDuration : 8f;

            Vector2Int startCell = GetHomeCell();
            Vector2Int destCell = _assignedBuilding.GetDoorCell();

            if (startCell == destCell)
            {
                SnapToCell(startCell.x, startCell.y);
                _sm.SetHomeCell(startCell.x, startCell.y);
                _sm.AssignJob(startCell.x, startCell.y, workDuration);
                _sm.StartWaiting();
                gameObject.SetActive(true);
                SetAnimSpeed(0f);
                return;
            }

            var path = FindPath(startCell, destCell);

            if (path == null || path.Count < 2)
            {
                _sm.SetHomeCell(startCell.x, startCell.y);
                _sm.AssignJob(startCell.x, startCell.y, workDuration);
                _sm.StartResting();
                return;
            }

            _sm.SetHomeCell(startCell.x, startCell.y);
            _sm.AssignJob(startCell.x, startCell.y, workDuration);
            _sm.StartWalkingToTarget((destCell.x, destCell.y));

            SnapToCell(startCell.x, startCell.y);
            SetPath(path);
            gameObject.SetActive(true);
            SetAnimSpeed(1f);
        }

        private void WalkToNewBuilding(Building building)
        {
            if (building == null || GridSystem.Instance == null) return;
            if (building.State == BuildingState.Destroyed)
            {
                ClearJob();
                return;
            }

            var cfg = GameConfig.Instance;
            float workDuration = cfg != null ? cfg.SettlerWorkDuration : 8f;

            Vector2Int currentCell = GridSystem.Instance.GetGridCoordinates(transform.position);
            Vector2Int destCell = building.GetDoorCell();

            _sm.AssignJob(currentCell.x, currentCell.y, workDuration);
            _sm.StartWalkingToTarget((destCell.x, destCell.y));

            if (currentCell == destCell)
            {
                _sm.StartWaiting();
                SetAnimSpeed(0f);
                return;
            }

            var path = FindPath(currentCell, destCell);
            if (path == null || path.Count < 2)
            {
                _sm.StartResting();
                _sm.SetRestDuration(cfg != null ? cfg.SettlerRestDuration : 5f);
                SetAnimSpeed(0f);
                return;
            }

            SetPath(path);
            gameObject.SetActive(true);
            SetAnimSpeed(1f);
        }

        private Vector2Int GetHomeCell()
        {
            var home = _sm.HomeCell;
            if (home.HasValue)
                return new Vector2Int(home.Value.x, home.Value.z);

            var cell = FindHomeCell();
            _sm.SetHomeCell(cell.x, cell.y);
            return cell;
        }

        private Vector2Int FindHomeCell()
        {
            if (BuildingManager.Instance != null)
            {
                var cc = FindFirstActiveBuilding(BuildingType.CommandCenter);
                if (cc != null) return cc.GetDoorCell();
            }

            var doors = RoadManager.Instance?.GetActiveBuildingDoorCells();
            if (doors != null && doors.Count > 0)
                return doors[UnityEngine.Random.Range(0, doors.Count)];

            return GridSystem.Instance != null
                ? GridSystem.Instance.GetGridCoordinates(transform.position)
                : Vector2Int.zero;
        }

        private static Building FindFirstActiveBuilding(BuildingType type)
        {
            if (BuildingManager.Instance == null) return null;
            foreach (var b in BuildingManager.Instance.AllBuildings)
                if (b.State == BuildingState.Active && b.Data.Type == type)
                    return b;
            return null;
        }

        public void Dispatch(Vector2Int origin, Vector2Int destination, float waitAtDest = 3f, Action onDone = null)
        {
            if (GridSystem.Instance == null) return;

            _onDone = onDone;
            _sm.SetHomeCell(origin.x, origin.y);

            var path = FindPath(origin, destination);

            if (path == null || path.Count < 2)
            {
                onDone?.Invoke();
                return;
            }

            SnapToCell(origin.x, origin.y);
            SetPath(path);
            _sm.StartWalkingToTarget((destination.x, destination.y));
            gameObject.SetActive(true);
            SetAnimSpeed(1f);
        }

        protected override void OnTick(float dt, float gameSpeed)
        {
            if (_sm.State == WalkerState.None) return;

            if (HasJob && _assignedBuilding == null)
            {
                ClearJob();
                return;
            }

            if (HasJob && _assignedBuilding.State == BuildingState.Destroyed)
            {
                ClearJob();
                return;
            }

            var result = _sm.Tick(dt, gameSpeed);

            switch (result)
            {
                case TickResult.Walking:
                    HandleWalking(dt, gameSpeed);
                    break;
                case TickResult.WaitComplete:
                    HandleWaitComplete();
                    break;
                case TickResult.RestComplete:
                    HandleRestComplete();
                    break;
            }
        }

        private void HandleWalking(float dt, float gameSpeed)
        {
            bool complete = TickMovement(dt, gameSpeed);
            if (complete)
            {
                ClearPath();
                _sm.OnPathComplete();

                if (_sm.State == WalkerState.WaitingAtTarget)
                    SetAnimSpeed(0f);
                else if (_sm.State == WalkerState.Resting || _sm.State == WalkerState.None)
                    HandleArrivalAfterReturn();
            }
        }

        private void HandleArrivalAfterReturn()
        {
            if (_sm.State == WalkerState.None)
            {
                SetAnimSpeed(0f);
                _onDone?.Invoke();
                _onDone = null;
                gameObject.SetActive(false);
            }
            else
            {
                SetAnimSpeed(0f);
            }
        }

        private void HandleWaitComplete()
        {
            var home = _sm.HomeCell;
            if (!home.HasValue) { Retire(); return; }

            Vector2Int homeCell = new(home.Value.x, home.Value.z);
            Vector2Int currentCell = GridSystem.Instance != null
                ? GridSystem.Instance.GetGridCoordinates(transform.position)
                : homeCell;

            if (currentCell == homeCell)
            {
                Retire();
                return;
            }

            var path = FindPath(currentCell, homeCell);

            if (path == null || path.Count < 2)
            {
                Retire();
                return;
            }

            SetPath(path);
            _sm.StartWalkingHome();
            SetAnimSpeed(1f);
        }

        private void HandleRestComplete()
        {
            if (HasJob)
                StartWorkCycle();
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void Retire()
        {
            ClearPath();
            _sm.OnPathComplete();

            if (_sm.State == WalkerState.None)
            {
                SetAnimSpeed(0f);
                _onDone?.Invoke();
                _onDone = null;
                gameObject.SetActive(false);
            }
            else
            {
                SetAnimSpeed(0f);
            }
        }

        public SettlerWalkerSave CaptureSave()
        {
            var gridPos = GetGridPosition();

            return new SettlerWalkerSave
            {
                GridX = gridPos.x,
                GridZ = gridPos.y,
                Task = _sm.State.ToString(),
                HomeCellX = _sm.HomeCell?.x ?? -1,
                HomeCellZ = _sm.HomeCell?.z ?? -1,
                Role = _role.ToString(),
                AssignedBuildingGridX = _assignedBuilding != null ? _assignedBuilding.GridOrigin.x : -1,
                AssignedBuildingGridZ = _assignedBuilding != null ? _assignedBuilding.GridOrigin.y : -1
            };
        }

        public void RestoreFromSave(SettlerWalkerSave save)
        {
            if (GridSystem.Instance != null)
            {
                var worldPos = GridSystem.Instance.GetWorldPosition(save.GridX, save.GridZ);
                transform.position = new Vector3(worldPos.x, _yOffset, worldPos.z);
            }

            if (save.HomeCellX >= 0)
                _sm.SetHomeCell(save.HomeCellX, save.HomeCellZ);

            if (System.Enum.TryParse<SettlerRole>(save.Role, out var role))
                _role = role;
            else
                _role = SettlerRole.None;

            if (_role != SettlerRole.None && save.AssignedBuildingGridX >= 0 && save.AssignedBuildingGridZ >= 0)
            {
                var building = FindBuildingByGrid(save.AssignedBuildingGridX, save.AssignedBuildingGridZ);
                if (building != null && building.State != BuildingState.Destroyed)
                    _assignedBuilding = building;
                else
                {
                    _role = SettlerRole.None;
                    _assignedBuilding = null;
                }
            }

            if (!HasJob)
            {
                _sm.ClearJob();
                gameObject.SetActive(false);
                return;
            }

            var cfg = GameConfig.Instance;
            float workDuration = cfg != null ? cfg.SettlerWorkDuration : 8f;
            Vector2Int startCell = new(save.GridX, save.GridZ);
            _sm.AssignJob(_sm.HomeCell?.x ?? startCell.x, _sm.HomeCell?.z ?? startCell.y, workDuration);

            if (!System.Enum.TryParse<WalkerState>(save.Task, out var state) || state == WalkerState.None)
            {
                _sm.StartResting();
                _sm.SetRestDuration(cfg != null ? cfg.SettlerRestDuration : 5f);
                gameObject.SetActive(false);
                return;
            }

            Vector2Int destCell = _assignedBuilding.GetDoorCell();

            if (state == WalkerState.WaitingAtTarget)
            {
                SnapToCell(save.GridX, save.GridZ);
                _sm.StartWaiting();
                gameObject.SetActive(true);
                SetAnimSpeed(0f);
                return;
            }

            if (state == WalkerState.Resting)
            {
                _sm.StartResting();
                _sm.SetRestDuration(cfg != null ? cfg.SettlerRestDuration : 5f);
                gameObject.SetActive(false);
                return;
            }

            Vector2Int target;
            bool toWork = state == WalkerState.WalkingToTarget;

            if (toWork)
                target = destCell;
            else
            {
                var home = _sm.HomeCell;
                target = home.HasValue ? new Vector2Int(home.Value.x, home.Value.z) : startCell;
            }

            if (startCell == target)
            {
                if (toWork)
                {
                    _sm.StartWaiting();
                    gameObject.SetActive(true);
                    SetAnimSpeed(0f);
                }
                else
                {
                    _sm.StartResting();
                    _sm.SetRestDuration(cfg != null ? cfg.SettlerRestDuration : 5f);
                    SetAnimSpeed(0f);
                }
                return;
            }

            var path = FindPath(startCell, target);
            if (path == null || path.Count < 2)
            {
                _sm.StartResting();
                _sm.SetRestDuration(cfg != null ? cfg.SettlerRestDuration : 5f);
                gameObject.SetActive(false);
                return;
            }

            SnapToCell(save.GridX, save.GridZ);
            SetPath(path);

            if (toWork)
            {
                _sm.StartWalkingToTarget((destCell.x, destCell.y));
            }
            else
            {
                _sm.StartWalkingHome();
            }

            gameObject.SetActive(true);
            SetAnimSpeed(1f);
        }

        private static Building FindBuildingByGrid(int gridX, int gridZ)
        {
            if (BuildingManager.Instance == null) return null;
            foreach (var b in BuildingManager.Instance.AllBuildings)
                if (b.GridOrigin.x == gridX && b.GridOrigin.y == gridZ)
                    return b;
            return null;
        }
    }

    [System.Serializable]
    public class SettlerWalkerSave
    {
        public int GridX;
        public int GridZ;
        public string Task;
        public int HomeCellX = -1;
        public int HomeCellZ = -1;
        public string Role;
        public int AssignedBuildingGridX = -1;
        public int AssignedBuildingGridZ = -1;
    }
}
