using HollowGround.NPCs;
using HollowGround.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollowGround.Buildings
{
    public class BuildingSelector : MonoBehaviour
    {
        [SerializeField] private float _raycastDistance = 100f;

        private Building _selectedBuilding;
        private SettlerWalker _selectedSettler;
        private UnityEngine.Camera _cam;
        private BuildingInfoUI _buildingInfoUI;
        private SettlerInfoUI _settlerInfoUI;

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
            _settlerInfoUI = FindAnyObjectByType<SettlerInfoUI>(FindObjectsInactive.Include);
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
                TrySelect();
            }
            else if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                DeselectAll();
            }
        }

        private void TrySelect()
        {
            if (_cam == null) _cam = UnityEngine.Camera.main;
            if (_cam == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = _cam.ScreenPointToRay(mousePos);

            var hits = Physics.RaycastAll(ray, _raycastDistance);

            SettlerWalker closestSettler = null;
            float closestSettlerDist = float.MaxValue;
            Building closestBuilding = null;
            float closestBuildingDist = float.MaxValue;

            foreach (var hit in hits)
            {
                var settler = hit.collider.GetComponent<SettlerWalker>();
                if (settler == null) settler = hit.collider.GetComponentInParent<SettlerWalker>();
                if (settler != null && settler.IsActive && hit.distance < closestSettlerDist)
                {
                    closestSettler = settler;
                    closestSettlerDist = hit.distance;
                    continue;
                }

                Building building = hit.collider.GetComponent<Building>();
                if (building == null) building = hit.collider.GetComponentInParent<Building>();
                if (building != null && hit.distance < closestBuildingDist)
                {
                    closestBuilding = building;
                    closestBuildingDist = hit.distance;
                }
            }

            if (closestSettler != null && closestSettlerDist <= closestBuildingDist)
            {
                DeselectAll();
                SelectSettler(closestSettler);
            }
            else if (closestBuilding != null)
            {
                DeselectAll();
                SelectBuilding(closestBuilding);
            }
            else
            {
                DeselectAll();
            }
        }

        public void SelectBuilding(Building building)
        {
            if (_selectedBuilding == building) return;

            DeselectAll();
            if (_buildingInfoUI == null)
                _buildingInfoUI = FindAnyObjectByType<BuildingInfoUI>(FindObjectsInactive.Include);

            _selectedBuilding = building;
            OnBuildingSelected?.Invoke(_selectedBuilding);

            if (_buildingInfoUI != null)
                _buildingInfoUI.ShowInfo(building);
        }

        private void SelectSettler(SettlerWalker walker)
        {
            _selectedSettler = walker;

            if (_settlerInfoUI == null)
                _settlerInfoUI = FindAnyObjectByType<SettlerInfoUI>(FindObjectsInactive.Include);

            if (_settlerInfoUI != null)
                _settlerInfoUI.ShowInfo(walker);
        }

        public void DeselectBuilding()
        {
            if (_selectedBuilding == null) return;

            _selectedBuilding = null;
            OnBuildingDeselected?.Invoke();

            if (_buildingInfoUI != null)
                _buildingInfoUI.HideInfo();
        }

        private void DeselectSettler()
        {
            _selectedSettler = null;

            if (_settlerInfoUI != null)
                _settlerInfoUI.HideInfo();
        }

        public void DeselectAll()
        {
            DeselectBuilding();
            DeselectSettler();
        }
    }
}
