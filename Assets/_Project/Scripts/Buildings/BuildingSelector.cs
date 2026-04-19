using UnityEngine;
using UnityEngine.InputSystem;

namespace HollowGround.Buildings
{
    public class BuildingSelector : MonoBehaviour
    {
        [SerializeField] private LayerMask _buildingMask;
        [SerializeField] private float _raycastDistance = 100f;

        private Building _selectedBuilding;
        private UnityEngine.Camera _cam;

        public Building SelectedBuilding => _selectedBuilding;

        public event System.Action<Building> OnBuildingSelected;
        public event System.Action OnBuildingDeselected;

        private void Awake()
        {
            _cam = UnityEngine.Camera.main;
        }

        private void Update()
        {
            if (BuildingPlacer.Instance != null && BuildingPlacer.Instance.IsPlacing) return;
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
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

            if (Physics.Raycast(ray, out RaycastHit hit, _raycastDistance, _buildingMask))
            {
                Building building = hit.collider.GetComponentInParent<Building>();
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
            _selectedBuilding = building;
            OnBuildingSelected?.Invoke(_selectedBuilding);
        }

        public void DeselectBuilding()
        {
            if (_selectedBuilding == null) return;

            _selectedBuilding = null;
            OnBuildingDeselected?.Invoke();
        }
    }
}
