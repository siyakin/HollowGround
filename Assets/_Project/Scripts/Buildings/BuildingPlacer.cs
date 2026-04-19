using HollowGround.Core;
using HollowGround.Grid;
using HollowGround.Resources;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollowGround.Buildings
{
    public class BuildingPlacer : MonoBehaviour
    {
        public static BuildingPlacer Instance { get; private set; }

        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private Material _validMaterial;
        [SerializeField] private Material _invalidMaterial;

        private BuildingData _currentBuilding;
        private GameObject _ghostObject;
        private bool _isPlacing;
        private int _rotation;
        private bool _isValidPlacement;

        public bool IsPlacing => _isPlacing;
        public BuildingData CurrentBuilding => _currentBuilding;

        public event System.Action OnPlacementStarted;
        public event System.Action<Building> OnPlacementCompleted;
        public event System.Action OnPlacementCancelled;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
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

            GameObject prefab = buildingData.GetPrefabForLevel(1);
            if (prefab != null)
            {
                _ghostObject = Instantiate(prefab);
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

            var cam = UnityEngine.Camera.main;
            if (cam == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePos);
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _groundMask)) return;

            Vector3 snapped = GridSystem.Instance.SnapToGrid(hit.point);
            (int sx, int sz) = GetRotatedSize();
            float offsetX = (sx - 1) * GridSystem.Instance.CellSize * 0.5f;
            float offsetZ = (sz - 1) * GridSystem.Instance.CellSize * 0.5f;
            _ghostObject.transform.position = new Vector3(snapped.x + offsetX, snapped.y, snapped.z + offsetZ);

            _ghostObject.transform.rotation = Quaternion.Euler(0, _rotation * 90f, 0);

            var coords = GridSystem.Instance.GetGridCoordinates(hit.point);
            _isValidPlacement = GridSystem.Instance.IsAreaBuildable(coords.x, coords.y, sx, sz);

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
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(mousePos);
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _groundMask)) return;

            var coords = GridSystem.Instance.GetGridCoordinates(hit.point);
            (int sx, int sz) = GetRotatedSize();

            if (!GridSystem.Instance.IsAreaBuildable(coords.x, coords.y, sx, sz)) return;

            var costs = _currentBuilding.GetCostForLevel(1);
            if (!BuildingManager.Instance.CanAffordBuilding(_currentBuilding, 1)) return;

            var rm = ResourceManager.Instance;
            if (rm != null) rm.SpendMultiple(costs);

            GameObject buildingObj = new(_currentBuilding.DisplayName);
            buildingObj.transform.position = _ghostObject.transform.position;
            buildingObj.transform.rotation = _ghostObject.transform.rotation;

            Building building = buildingObj.AddComponent<Building>();
            building.Initialize(_currentBuilding, coords);

            GridSystem.Instance.OccupyCells(coords.x, coords.y, sx, sz, buildingObj);
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
