using HollowGround.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollowGround.Buildings
{
    public class BuildingSelector : MonoBehaviour
    {
        [SerializeField] private float _raycastDistance = 100f;

        private Building _selectedBuilding;
        private UnityEngine.Camera _cam;
        private BuildingInfoUI _buildingInfoUI;

        public Building SelectedBuilding => _selectedBuilding;

        public event System.Action<Building> OnBuildingSelected;
        public event System.Action OnBuildingDeselected;

        private void Awake()
        {
            _cam = UnityEngine.Camera.main;
            if (_cam == null)
            {
                var strategyCam = FindAnyObjectByType<HollowGround.Camera.StrategyCamera>();
                if (strategyCam != null)
                    _cam = strategyCam.GetComponentInChildren<UnityEngine.Camera>();
            }

            _buildingInfoUI = FindAnyObjectByType<BuildingInfoUI>(FindObjectsInactive.Include);
        }

        private void Update()
        {
            if (UIManager.Instance != null && UIManager.Instance.IsInputBlocked) return;
            if (BuildingPlacer.Instance != null && BuildingPlacer.Instance.IsPlacing) return;
            if (Mouse.current == null) return;

            bool overUI = UnityEngine.EventSystems.EventSystem.current != null
                && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

            if (Mouse.current.leftButton.wasPressedThisFrame && !overUI)
            {
                TrySelectBuilding();
            }
            else if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                DeselectBuilding();
            }
        }

        private void TrySelectBuilding()
        {
            if (_cam == null) _cam = UnityEngine.Camera.main;
            if (_cam == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = _cam.ScreenPointToRay(mousePos);

            var hits = Physics.RaycastAll(ray, _raycastDistance);
            foreach (var hit in hits)
            {
                Building building = hit.collider.GetComponent<Building>();
                if (building == null) building = hit.collider.GetComponentInParent<Building>();
                if (building != null)
                {
                    SelectBuilding(building);
                    return;
                }
            }

            DeselectBuilding();
        }

        public void SelectBuilding(Building building)
        {
            if (_selectedBuilding == building) return;

            DeselectBuilding();
            if (_buildingInfoUI == null)
                _buildingInfoUI = FindAnyObjectByType<BuildingInfoUI>(FindObjectsInactive.Include);

            _selectedBuilding = building;
            OnBuildingSelected?.Invoke(_selectedBuilding);

            if (_buildingInfoUI != null)
                _buildingInfoUI.ShowInfo(building);
        }

        public void DeselectBuilding()
        {
            if (_selectedBuilding == null) return;

            _selectedBuilding = null;
            OnBuildingDeselected?.Invoke();

            if (_buildingInfoUI != null)
                _buildingInfoUI.HideInfo();
        }
    }
}
