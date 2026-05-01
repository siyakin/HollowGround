using System;
using System.Collections.Generic;
using HollowGround.Buildings;
using HollowGround.Core;
using HollowGround.Grid;
using HollowGround.Roads;
using UnityEngine;

namespace HollowGround.NPCs
{
    public enum SettlerTask
    {
        None,
        WalkingToTarget,
        WaitingAtTarget,
        ReturningHome,
        Resting
    }

    public class SettlerWalker : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        private List<Vector2Int> _currentPath;
        private int _pathIndex;
        private float _moveSpeed = 2f;
        private float _yOffset = 0.05f;
        private bool _active = true;
        private SettlerTask _task = SettlerTask.None;
        private Vector3 _targetWorldPos;
        private Vector2Int _homeCell = new Vector2Int(-1, -1);
        private float _waitTimer;
        private float _waitDuration;
        private Action _onDone;
        private Animator _animator;

        private SettlerRole _role = SettlerRole.None;
        private Building _assignedBuilding;
        private float _restTimer;
        private float _restDuration = 5f;

        public SettlerTask CurrentTask => _task;
        public bool IsActive => _active;
        public bool IsBusy => _task != SettlerTask.None;
        public SettlerRole Role => _role;
        public Building AssignedBuilding => _assignedBuilding;
        public bool HasJob => _assignedBuilding != null && _role != SettlerRole.None;

        public void Initialize(float moveSpeed)
        {
            _moveSpeed = moveSpeed;
            var cfg = GameConfig.Instance;
            if (cfg != null)
                _restDuration = cfg.SettlerRestDuration;
        }

        public void SetAnimator(Animator animator)
        {
            _animator = animator;
        }

        public void AssignJob(SettlerRole role, Building building)
        {
            _role = role;
            _assignedBuilding = building;
            _onDone = null;
            StartWorkCycle();
        }

        public void ClearJob()
        {
            _role = SettlerRole.None;
            _assignedBuilding = null;
            _currentPath = null;
            _onDone = null;
            _task = SettlerTask.None;
            _restTimer = 0f;
            SetAnimSpeed(0f);
            gameObject.SetActive(false);
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
            _waitDuration = workDuration;

            Vector2Int startCell = _homeCell;
            if (startCell.x < 0)
                startCell = FindHomeCell();

            if (_homeCell.x < 0)
                _homeCell = startCell;

            Vector2Int destCell = _assignedBuilding.GetDoorCell();

            if (startCell == destCell)
            {
                _task = SettlerTask.WaitingAtTarget;
                _waitTimer = 0f;
                var wp = GridSystem.Instance.GetWorldPosition(startCell.x, startCell.y);
                transform.position = new Vector3(wp.x, _yOffset, wp.z);
                gameObject.SetActive(true);
                SetAnimSpeed(0f);
                return;
            }

            var path = RoadManager.Instance != null
                ? RoadManager.Instance.FindPublicPath(startCell, destCell)
                : null;

            if (path == null || path.Count < 2)
            {
                _task = SettlerTask.Resting;
                _restTimer = 0f;
                return;
            }

            var worldPos = GridSystem.Instance.GetWorldPosition(startCell.x, startCell.y);
            transform.position = new Vector3(worldPos.x, _yOffset, worldPos.z);

            _currentPath = path;
            _pathIndex = 1;
            _targetWorldPos = GridSystem.Instance.GetWorldPosition(path[1].x, path[1].y);
            _task = SettlerTask.WalkingToTarget;
            gameObject.SetActive(true);
            SetAnimSpeed(1f);
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

            _homeCell = origin;
            _onDone = onDone;
            _waitDuration = waitAtDest;

            var path = RoadManager.Instance != null
                ? RoadManager.Instance.FindPublicPath(origin, destination)
                : null;

            if (path == null || path.Count < 2)
            {
                onDone?.Invoke();
                return;
            }

            var worldPos = GridSystem.Instance.GetWorldPosition(origin.x, origin.y);
            transform.position = new Vector3(worldPos.x, _yOffset, worldPos.z);

            _currentPath = path;
            _pathIndex = 1;
            _targetWorldPos = GridSystem.Instance.GetWorldPosition(path[1].x, path[1].y);
            _task = SettlerTask.WalkingToTarget;
            gameObject.SetActive(true);
            SetAnimSpeed(1f);
        }

        public void Deactivate()
        {
            _active = false;
        }

        private void Update()
        {
            if (!_active || _task == SettlerTask.None) return;
            if (TimeManager.Instance != null && TimeManager.Instance.IsPaused) return;

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

            float speed = TimeManager.Instance != null ? TimeManager.Instance.GameSpeed : 1f;
            if (speed <= 0f) return;

            switch (_task)
            {
                case SettlerTask.WalkingToTarget:
                case SettlerTask.ReturningHome:
                    TickWalking(speed);
                    break;
                case SettlerTask.WaitingAtTarget:
                    TickWaiting(speed);
                    break;
                case SettlerTask.Resting:
                    TickResting(speed);
                    break;
            }
        }

        private void TickWalking(float speed)
        {
            if (_currentPath == null || _pathIndex >= _currentPath.Count)
            {
                OnPathComplete();
                return;
            }

            Vector3 targetPos = _targetWorldPos;
            targetPos.y = _yOffset;

            Vector3 currentPos = transform.position;
            Vector3 direction = targetPos - currentPos;
            direction.y = 0f;

            float dist = direction.magnitude;
            float step = _moveSpeed * speed * Time.deltaTime;

            if (dist < step || dist < 0.05f)
            {
                transform.position = new Vector3(targetPos.x, _yOffset, targetPos.z);
                _pathIndex++;

                if (_pathIndex < _currentPath.Count)
                {
                    var nextCell = _currentPath[_pathIndex];
                    _targetWorldPos = GridSystem.Instance != null
                        ? GridSystem.Instance.GetWorldPosition(nextCell.x, nextCell.y)
                        : Vector3.zero;
                }
            }
            else
            {
                direction.Normalize();
                transform.position = currentPos + direction * step;

                if (direction != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
                }
            }
        }

        private void OnPathComplete()
        {
            _currentPath = null;

            if (_task == SettlerTask.WalkingToTarget)
            {
                _task = SettlerTask.WaitingAtTarget;
                _waitTimer = 0f;
                SetAnimSpeed(0f);
            }
            else if (_task == SettlerTask.ReturningHome)
            {
                Retire();
            }
        }

        private void TickWaiting(float speed)
        {
            _waitTimer += Time.deltaTime * speed;
            if (_waitTimer >= _waitDuration)
                StartReturnHome();
        }

        private void TickResting(float speed)
        {
            _restTimer += Time.deltaTime * speed;
            if (_restTimer >= _restDuration)
            {
                if (HasJob)
                    StartWorkCycle();
                else
                {
                    _task = SettlerTask.None;
                    gameObject.SetActive(false);
                }
            }
        }

        private void StartReturnHome()
        {
            Vector2Int currentCell = GridSystem.Instance != null
                ? GridSystem.Instance.GetGridCoordinates(transform.position)
                : _homeCell;

            if (currentCell == _homeCell)
            {
                Retire();
                return;
            }

            var path = RoadManager.Instance != null
                ? RoadManager.Instance.FindPublicPath(currentCell, _homeCell)
                : null;

            if (path == null || path.Count < 2)
            {
                Retire();
                return;
            }

            _currentPath = path;
            _pathIndex = 1;
            _targetWorldPos = GridSystem.Instance.GetWorldPosition(path[1].x, path[1].y);
            _task = SettlerTask.ReturningHome;
            SetAnimSpeed(1f);
        }

        private void Retire()
        {
            if (HasJob)
            {
                _task = SettlerTask.Resting;
                _restTimer = 0f;
                _currentPath = null;
                SetAnimSpeed(0f);
                return;
            }

            _task = SettlerTask.None;
            _currentPath = null;
            _onDone?.Invoke();
            _onDone = null;
            SetAnimSpeed(0f);
            gameObject.SetActive(false);
        }

        private void SetAnimSpeed(float speed)
        {
            if (_animator == null) return;
            _animator.SetFloat(SpeedHash, speed);
            if (speed > 0.1f)
                _animator.CrossFade("Walk", 0.1f);
            else
                _animator.CrossFade("Idle", 0.1f);
        }

        public SettlerWalkerSave CaptureSave()
        {
            var gridPos = GridSystem.Instance != null
                ? GridSystem.Instance.GetGridCoordinates(transform.position)
                : Vector2Int.zero;

            return new SettlerWalkerSave
            {
                GridX = gridPos.x,
                GridZ = gridPos.y,
                Task = _task.ToString(),
                HomeCellX = _homeCell.x,
                HomeCellZ = _homeCell.y,
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

            _homeCell = new Vector2Int(save.HomeCellX, save.HomeCellZ);
            if (_homeCell.x < 0)
                _homeCell = new Vector2Int(save.GridX, save.GridZ);

            if (System.Enum.TryParse<SettlerTask>(save.Task, out var task))
                _task = task;
            else
                _task = SettlerTask.None;

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

            if (_task == SettlerTask.None && !HasJob)
                gameObject.SetActive(false);

            _currentPath = null;
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
