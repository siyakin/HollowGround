using HollowGround.Core;
using HollowGround.NPCs;
using HollowGround.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollowGround.Buildings
{
    public class BuildingSelector : MonoBehaviour
    {
        [SerializeField] private float _raycastDistance = 100f;
        [SerializeField] private float _hoverRaycastInterval = 0.08f;

        private Building _selectedBuilding;
        private SettlerWalker _selectedSettler;
        private UnityEngine.Camera _cam;
        private BuildingInfoUI _buildingInfoUI;
        private SettlerInfoUI _settlerInfoUI;

        private float _hoverTimer;
        private Building _hoveredBuilding;
        private SettlerWalker _hoveredSettler;

        public Building SelectedBuilding => _selectedBuilding;
        public SettlerWalker SelectedSettler => _selectedSettler;

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

            if (!overUI)
                HandleHover();
            else
                ClearHover();
        }

        private void HandleHover()
        {
            var cfg = GameConfig.Instance;
            bool buildingsEnabled = cfg != null && cfg.TooltipWorldBuildings;
            bool settlersEnabled = cfg != null && cfg.TooltipWorldSettlers;

            if (!buildingsEnabled && !settlersEnabled) return;

            _hoverTimer += Time.unscaledDeltaTime;
            if (_hoverTimer < _hoverRaycastInterval) return;
            _hoverTimer = 0f;

            if (_cam == null) _cam = UnityEngine.Camera.main;
            if (_cam == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = _cam.ScreenPointToRay(mousePos);
            var hits = Physics.RaycastAll(ray, _raycastDistance);

            Building newBuilding = null;
            float buildingDist = float.MaxValue;
            SettlerWalker newSettler = null;
            float settlerDist = float.MaxValue;

            foreach (var hit in hits)
            {
                Building b = hit.collider.GetComponent<Building>();
                if (b == null) b = hit.collider.GetComponentInParent<Building>();
                if (b != null && b.State != BuildingState.Placing && buildingsEnabled
                    && IsStateAllowed(b.State) && hit.distance < buildingDist)
                {
                    newBuilding = b;
                    buildingDist = hit.distance;
                    continue;
                }

                SettlerWalker s = hit.collider.GetComponent<SettlerWalker>();
                if (s == null) s = hit.collider.GetComponentInParent<SettlerWalker>();
                if (s != null && s.IsActive && settlersEnabled && hit.distance < settlerDist)
                {
                    newSettler = s;
                    settlerDist = hit.distance;
                }
            }

            if (newSettler != null && settlerDist <= buildingDist)
            {
                if (_selectedSettler == newSettler)
                {
                    ClearHover();
                    return;
                }
                if (_hoveredSettler != newSettler)
                {
                    _hoveredSettler = newSettler;
                    _hoveredBuilding = null;
                    var captured = newSettler;
                    TooltipUI.ScheduleShow(() => TooltipContentBuilder.ForSettler(captured));
                }
            }
            else if (newBuilding != null)
            {
                if (_selectedBuilding == newBuilding)
                {
                    ClearHover();
                    return;
                }
                if (_hoveredBuilding != newBuilding)
                {
                    _hoveredBuilding = newBuilding;
                    _hoveredSettler = null;
                    var captured = newBuilding;
                    TooltipUI.ScheduleShow(() => TooltipContentBuilder.ForBuilding(captured));
                }
            }
            else
            {
                ClearHover();
            }
        }

        private void ClearHover()
        {
            if (_hoveredBuilding != null || _hoveredSettler != null)
            {
                _hoveredBuilding = null;
                _hoveredSettler = null;
                TooltipUI.Hide();
            }
        }

        private void TrySelect()
        {
            if (_cam == null) _cam = UnityEngine.Camera.main;
            if (_cam == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = _cam.ScreenPointToRay(mousePos);

            var hits = Physics.RaycastAll(ray, _raycastDistance);

            Building closestBuilding = null;
            float closestBuildingDist = float.MaxValue;
            SettlerWalker closestSettler = null;
            float closestSettlerDist = float.MaxValue;
            PatrolWalker closestPatrol = null;
            float closestPatrolDist = float.MaxValue;

            foreach (var hit in hits)
            {
                Building building = hit.collider.GetComponent<Building>();
                if (building == null) building = hit.collider.GetComponentInParent<Building>();
                if (building != null)
                {
                    if (hit.distance < closestBuildingDist)
                    {
                        closestBuilding = building;
                        closestBuildingDist = hit.distance;
                    }
                    continue;
                }

                SettlerWalker settler = hit.collider.GetComponent<SettlerWalker>();
                if (settler == null) settler = hit.collider.GetComponentInParent<SettlerWalker>();
                if (settler != null && settler.IsActive)
                {
                    if (hit.distance < closestSettlerDist)
                    {
                        closestSettler = settler;
                        closestSettlerDist = hit.distance;
                    }
                    continue;
                }

                PatrolWalker patrol = hit.collider.GetComponent<PatrolWalker>();
                if (patrol == null) patrol = hit.collider.GetComponentInParent<PatrolWalker>();
                if (patrol != null && patrol.IsActive)
                {
                    if (hit.distance < closestPatrolDist)
                    {
                        closestPatrol = patrol;
                        closestPatrolDist = hit.distance;
                    }
                }
            }

            if (closestPatrol != null && closestPatrolDist <= closestSettlerDist && closestPatrolDist <= closestBuildingDist)
            {
                DeselectAll();
                SelectPatrol(closestPatrol);
                return;
            }

            if (closestSettler != null && closestSettlerDist <= closestBuildingDist)
            {
                DeselectBuilding();
                SelectSettler(closestSettler);
                return;
            }

            if (closestBuilding != null)
            {
                DeselectSettler();
                HideSettlerInfo();
                SelectBuilding(closestBuilding);
                return;
            }

            DeselectAll();
        }

        private void SelectPatrol(PatrolWalker patrol)
        {
            string name = patrol.GetDisplayName();
            string role = patrol.GetRoleLabel();
            string type = patrol.IsHeroWalker ? "Hero" : "Soldier";
            ToastUI.Show($"{type}: {name} [{role}]", UIColors.Default.Gold);
        }

        private void SelectSettler(SettlerWalker settler)
        {
            _selectedSettler = settler;
            if (_settlerInfoUI == null)
                _settlerInfoUI = FindAnyObjectByType<SettlerInfoUI>(FindObjectsInactive.Include);
            if (_settlerInfoUI != null)
                _settlerInfoUI.ShowInfo(settler);
        }

        private void DeselectSettler()
        {
            _selectedSettler = null;
        }

        public void DeselectAll()
        {
            DeselectBuilding();
            DeselectSettler();
            HideSettlerInfo();
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

        private void HideSettlerInfo()
        {
            if (_settlerInfoUI == null)
                _settlerInfoUI = FindAnyObjectByType<SettlerInfoUI>(FindObjectsInactive.Include);
            if (_settlerInfoUI != null)
                _settlerInfoUI.HideInfo();
        }

        private static bool IsStateAllowed(BuildingState state)
        {
            var cfg = GameConfig.Instance;
            if (cfg == null) return true;

            return cfg.TooltipBuildingMinState switch
            {
                TooltipBuildingState.None => false,
                TooltipBuildingState.Constructing => state != BuildingState.Placing,
                TooltipBuildingState.Active => state == BuildingState.Active || state == BuildingState.Upgrading,
                TooltipBuildingState.Upgrading => state == BuildingState.Upgrading,
                TooltipBuildingState.Damaged => state == BuildingState.Damaged,
                _ => true
            };
        }
    }
}
