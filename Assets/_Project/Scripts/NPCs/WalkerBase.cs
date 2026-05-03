using System.Collections.Generic;
using HollowGround.Grid;
using HollowGround.Roads;
using UnityEngine;

namespace HollowGround.NPCs
{
    public abstract class WalkerBase : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        protected List<Vector2Int> _currentPath;
        protected int _pathIndex;
        protected float _moveSpeed = 2f;
        protected float _yOffset = 0.05f;
        protected bool _active = true;
        protected Vector3 _targetWorldPos;
        protected Animator _animator;

        private Vector2Int _lastGridCell = new(int.MinValue, int.MinValue);
        private bool _occupancyTracked;

        public bool IsActive => _active;

        public virtual void Initialize(float moveSpeed)
        {
            _moveSpeed = moveSpeed;
        }

        public void SetAnimator(Animator animator)
        {
            _animator = animator;
        }

        public void Deactivate()
        {
            _active = false;
        }

        public void Tick(float dt, float gameSpeed)
        {
            if (!_active) return;
            OnTick(dt, gameSpeed);
        }

        protected abstract void OnTick(float dt, float gameSpeed);

        protected bool TickMovement(float dt, float gameSpeed)
        {
            if (_currentPath == null || _pathIndex >= _currentPath.Count)
                return true;

            Vector3 targetPos = _targetWorldPos;
            targetPos.y = _yOffset;

            Vector3 currentPos = transform.position;
            Vector3 direction = targetPos - currentPos;
            direction.y = 0f;

            float dist = direction.magnitude;
            float step = _moveSpeed * gameSpeed * dt;

            if (dist < step || dist < 0.05f)
            {
                transform.position = new Vector3(targetPos.x, _yOffset, targetPos.z);
                _pathIndex++;

                ReportCellChange();

                if (_pathIndex < _currentPath.Count)
                {
                    var nextCell = _currentPath[_pathIndex];
                    if (GridSystem.Instance != null)
                        _targetWorldPos = GridSystem.Instance.GetWorldPosition(nextCell.x, nextCell.y);
                }
            }
            else
            {
                direction.Normalize();
                transform.position = currentPos + direction * step;

                if (direction != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, dt * 8f);
                }
            }

            return _pathIndex >= _currentPath.Count;
        }

        protected void SetPath(List<Vector2Int> path)
        {
            _currentPath = path;
            _pathIndex = 1;
            if (path != null && path.Count > 1 && GridSystem.Instance != null)
                _targetWorldPos = GridSystem.Instance.GetWorldPosition(path[1].x, path[1].y);
        }

        protected void ClearPath()
        {
            _currentPath = null;
            _pathIndex = 0;
        }

        protected void SnapToCell(int gridX, int gridZ)
        {
            if (GridSystem.Instance == null) return;
            var wp = GridSystem.Instance.GetWorldPosition(gridX, gridZ);
            transform.position = new Vector3(wp.x, _yOffset, wp.z);
            _lastGridCell = new Vector2Int(gridX, gridZ);
        }

        protected void SetAnimSpeed(float speed)
        {
            if (_animator == null || !gameObject.activeInHierarchy) return;
            _animator.SetFloat(SpeedHash, speed);
            if (speed > 0.1f)
                _animator.CrossFade("Walk", 0.1f);
            else
                _animator.CrossFade("Idle", 0.1f);
        }

        protected List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
        {
            if (WalkerManager.Instance != null)
                return WalkerManager.Instance.RequestPath(start, end);
            if (RoadManager.Instance != null)
                return RoadManager.Instance.FindPublicPath(start, end);
            return null;
        }

        public Vector2Int GetGridPosition()
        {
            if (GridSystem.Instance == null) return Vector2Int.zero;
            return GridSystem.Instance.GetGridCoordinates(transform.position);
        }

        private void ReportCellChange()
        {
            if (this is not SettlerWalker sw) return;
            var currentCell = GetGridPosition();
            if (currentCell == _lastGridCell) return;

            if (WalkerManager.Instance != null)
                WalkerManager.Instance.UpdateOccupancy(sw, _lastGridCell, currentCell);

            _lastGridCell = currentCell;
        }
    }
}
