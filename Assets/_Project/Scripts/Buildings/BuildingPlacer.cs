using System.Collections.Generic;
using System.Linq;
using HollowGround.Core;
using HollowGround.Grid;
using HollowGround.Resources;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollowGround.Buildings
{
    public class BuildingPlacer : Singleton<BuildingPlacer>
    {
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private Material _validMaterial;
        [SerializeField] private Material _invalidMaterial;

        private BuildingData _currentBuilding;
        private GameObject _ghostObject;
        private bool _isPlacing;
        private int _rotation;
        private bool _isValidPlacement;
        private UnityEngine.Camera _cam;
        private Vector2Int _cachedCoords;
        private Vector3 _cachedWorldPos;

        public bool IsPlacing => _isPlacing;
        public BuildingData CurrentBuilding => _currentBuilding;

        public event System.Action OnPlacementStarted;
        public event System.Action<Building> OnPlacementCompleted;
        public event System.Action OnPlacementCancelled;

        protected override void Awake()
        {
            base.Awake();
            ResolveCamera();
        }

        private void ResolveCamera()
        {
            _cam = UnityEngine.Camera.main;
            if (_cam != null) return;

            var strategyCam = FindAnyObjectByType<HollowGround.Camera.StrategyCamera>();
            if (strategyCam != null)
                _cam = strategyCam.GetComponentInChildren<UnityEngine.Camera>();
        }

        private void Update()
        {
            if (_isPlacing && _cam == null)
                ResolveCamera();
            if (!_isPlacing) return;

            UpdateGhostPosition();
            HandlePlacementInput();
        }

        public void StartPlacement(BuildingData buildingData)
        {
            if (_isPlacing) CancelPlacement();

            _currentBuilding = buildingData;
            _isPlacing = true;
            _rotation = 0;
            _isValidPlacement = false;
            _cachedCoords = new Vector2Int(-1, -1);
            _cachedWorldPos = Vector3.zero;

            GameObject prefab = buildingData.GetPrefabForLevel(1);
            if (prefab != null)
            {
                _ghostObject = Instantiate(prefab);
                _ghostObject.transform.rotation = Quaternion.Euler(0, _rotation * 90f, 0);
                SetGhostMaterial(_validMaterial);
            }

            GameManager.Instance.SetState(GameState.Building);
            OnPlacementStarted?.Invoke();
        }

        public void CancelPlacement()
        {
            if (!_isPlacing) return;

            if (_ghostObject != null)
                Destroy(_ghostObject);

            _isPlacing = false;
            _currentBuilding = null;
            _isValidPlacement = false;

            GameManager.Instance.SetState(GameState.Playing);
            OnPlacementCancelled?.Invoke();
        }

        private void UpdateGhostPosition()
        {
            if (_ghostObject == null) return;
            if (Mouse.current == null) return;
            if (GridSystem.Instance == null) return;
            if (_cam == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = _cam.ScreenPointToRay(mousePos);
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _groundMask)) return;

            _cachedCoords = GridSystem.Instance.GetGridCoordinates(hit.point);
            Vector3 snapped = GridSystem.Instance.GetWorldPosition(_cachedCoords.x, _cachedCoords.y);
            (int sx, int sz) = GetRotatedSize();
            float offsetX = (sx - 1) * GridSystem.Instance.CellSize * 0.5f;
            float offsetZ = (sz - 1) * GridSystem.Instance.CellSize * 0.5f;
            _cachedWorldPos = new Vector3(snapped.x + offsetX, snapped.y, snapped.z + offsetZ);
            _ghostObject.transform.position = _cachedWorldPos;

            _ghostObject.transform.rotation = Quaternion.Euler(0, _rotation * 90f, 0);

            _isValidPlacement = GridSystem.Instance.IsAreaBuildable(_cachedCoords.x, _cachedCoords.y, sx, sz);

            SetGhostMaterial(_isValidPlacement ? _validMaterial : _invalidMaterial);
        }

        private void HandlePlacementInput()
        {
            if (Mouse.current == null || Keyboard.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame && _isValidPlacement)
            {
                PlaceBuilding();
            }
            else if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CancelPlacement();
            }
            else if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                _rotation = (_rotation + 1) % 4;
            }
        }

        private void PlaceBuilding()
        {
            if (_cam == null) return;
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = _cam.ScreenPointToRay(mousePos);
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _groundMask)) return;

            var coords = GridSystem.Instance.GetGridCoordinates(hit.point);
            (int sx, int sz) = GetRotatedSize();

            if (!GridSystem.Instance.IsAreaBuildable(coords.x, coords.y, sx, sz)) return;

            var costs = _currentBuilding.GetCostForLevel(1);

            var rm = ResourceManager.Instance;
            if (rm != null && !rm.SpendMultiple(costs))
                return;

            string buildingName = _currentBuilding.DisplayName;

            GameObject buildingObj = new(buildingName);

            if (_ghostObject != null)
            {
                buildingObj.transform.position = _ghostObject.transform.position;
                buildingObj.transform.rotation = _ghostObject.transform.rotation;
            }
            else
            {
                if (_cam == null) return;
                Vector2 placePos = Mouse.current.position.ReadValue();
                Ray placeRay = _cam.ScreenPointToRay(placePos);
                if (Physics.Raycast(placeRay, out RaycastHit placeHit, Mathf.Infinity, _groundMask))
                {
                    buildingObj.transform.position = GridSystem.Instance.SnapToGrid(placeHit.point);
                }
            }

            Building building = buildingObj.AddComponent<Building>();
            building.Initialize(_currentBuilding, coords);

            GridSystem.Instance.OccupyCells(coords.x, coords.y, sx, sz, buildingObj);
            if (BuildingManager.Instance != null)
                BuildingManager.Instance.RegisterBuilding(building);

            if (_ghostObject != null) Destroy(_ghostObject);
            _ghostObject = null;
            _isPlacing = false;
            _currentBuilding = null;

            GameManager.Instance.SetState(GameState.Playing);
            OnPlacementCompleted?.Invoke(building);
        }

        private (int x, int z) GetRotatedSize()
        {
            if (_currentBuilding == null) return (1, 1);

            return _rotation % 2 == 0
                ? (_currentBuilding.SizeX, _currentBuilding.SizeZ)
                : (_currentBuilding.SizeZ, _currentBuilding.SizeX);
        }

        private void SetGhostMaterial(Material mat)
        {
            if (_ghostObject == null || mat == null) return;

            var renderers = _ghostObject.GetComponentsInChildren<Renderer>();
            foreach (var rend in renderers)
            {
                rend.material = mat;
            }
        }
    }
}
