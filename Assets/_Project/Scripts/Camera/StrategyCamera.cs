using UnityEngine;
using UnityEngine.InputSystem;

namespace HollowGround.Camera
{
    public class StrategyCamera : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private UnityEngine.InputSystem.InputActionAsset _inputAsset;

        [Header("Pan")]
        [SerializeField] private float _keyboardPanSpeed = 40f;
        [SerializeField] private float _mousePanSpeed = 1.5f;
        [SerializeField] private float _panSmoothTime = 0.12f;
        [SerializeField] private float _edgePanZone = 15f;
        [SerializeField] private float _edgePanSpeed = 20f;

        [Header("Zoom")]
        [SerializeField] private float _zoomSpeed = 3f;
        [SerializeField] private float _minZoom = 8f;
        [SerializeField] private float _maxZoom = 60f;
        [SerializeField] private float _zoomSmoothTime = 0.15f;

        [Header("Rotation")]
        [SerializeField] private float _rotationSpeed = 150f;
        [SerializeField] private float _rotationSmoothTime = 0.12f;

        [Header("Bounds")]
        [SerializeField] private Vector3 _boundsMin = new(-50, 0, -50);
        [SerializeField] private Vector3 _boundsMax = new(50, 0, 50);

        [Header("Angle")]
        [SerializeField] private float _tiltAngle = 40f;

        private UnityEngine.Camera _cam;
        private Transform _pivot;

        private Vector2 _keyPanInput;
        private Vector2 _mouseDelta;
        private Vector2 _mousePos;
        private float _scrollY;

        private Vector3 _targetPosition;
        private float _targetZoom;
        private float _targetRotation;

        private Vector3 _panVelocity;
        private float _zoomVelocity;
        private float _rotationVelocity;

        private Vector3 _dragStartWorld;
        private bool _isPanning;
        private bool _isRotating;

        private InputAction _panAction;
        private InputAction _zoomAction;
        private InputAction _rotateAction;
        private InputAction _rotateButtonAction;
        private InputAction _mousePosAction;

        private void Awake()
        {
            _cam = GetComponentInChildren<UnityEngine.Camera>();
            CreatePivot();
            _targetZoom = 25f;
            _targetRotation = transform.eulerAngles.y;
            _targetPosition = transform.position;
        }

        private void CreatePivot()
        {
            _pivot = new GameObject("CameraPivot").transform;
            _pivot.position = transform.position;
            _pivot.rotation = Quaternion.Euler(_tiltAngle, 0, 0);
            _cam.transform.SetParent(_pivot);
            _cam.transform.localPosition = new Vector3(0, _targetZoom, 0);
            _cam.transform.localRotation = Quaternion.identity;
        }

        private void Start()
        {
            if (_inputAsset != null)
            {
                var map = _inputAsset.FindActionMap("Strategy");
                if (map != null)
                {
                    _panAction = map.FindAction("Pan");
                    _zoomAction = map.FindAction("Zoom");
                    _rotateAction = map.FindAction("Rotate");
                    _rotateButtonAction = map.FindAction("RotateButton");
                    _mousePosAction = map.FindAction("MousePosition");

                    _panAction?.Enable();
                    _zoomAction?.Enable();
                    _rotateAction?.Enable();
                    _rotateButtonAction?.Enable();
                    _mousePosAction?.Enable();
                }
            }
        }

        private void Update()
        {
            ReadInput();
            HandleKeyboardPan();
            HandleMousePan();
            HandleMouseRotation();
            HandleZoom();
            HandleEdgePan();
            ApplyBounds();
        }

        private void ReadInput()
        {
            if (Mouse.current == null || Keyboard.current == null) return;

            _keyPanInput = _panAction != null ? _panAction.ReadValue<Vector2>() : Vector2.zero;
            _scrollY = _zoomAction != null ? _zoomAction.ReadValue<Vector2>().y : 0f;
            _mousePos = _mousePosAction != null ? _mousePosAction.ReadValue<Vector2>() : Vector2.zero;
            _mouseDelta = _rotateAction != null ? _rotateAction.ReadValue<Vector2>() : Vector2.zero;

            bool middlePressed = Mouse.current.middleButton.isPressed;
            bool rightPressed = Mouse.current.rightButton.isPressed;

            if (middlePressed && !_isPanning)
            {
                _isPanning = true;
                _dragStartWorld = GetWorldPoint(_mousePos);
            }
            else if (!middlePressed)
            {
                _isPanning = false;
            }

            if (rightPressed && !_isRotating)
                _isRotating = true;
            else if (!rightPressed)
                _isRotating = false;
        }

        private void HandleKeyboardPan()
        {
            if (_keyPanInput.sqrMagnitude < 0.01f) return;

            Vector3 forward = _pivot.forward;
            forward.y = 0;
            forward.Normalize();
            Vector3 right = _pivot.right;
            right.y = 0;
            right.Normalize();

            Vector3 move = (forward * _keyPanInput.y + right * _keyPanInput.x) * _keyboardPanSpeed;
            _targetPosition += move * Time.unscaledDeltaTime;
        }

        private void HandleMousePan()
        {
            if (!_isPanning) return;
            if (_mouseDelta.sqrMagnitude < 0.01f) return;

            Vector3 currentWorld = GetWorldPoint(_mousePos);
            if (currentWorld == Vector3.zero && _dragStartWorld == Vector3.zero) return;

            Vector3 diff = _dragStartWorld - currentWorld;
            diff.y = 0;
            _targetPosition += diff * _mousePanSpeed;
        }

        private void HandleMouseRotation()
        {
            if (!_isRotating) return;
            if (_mouseDelta.sqrMagnitude < 0.01f) return;

            _targetRotation += _mouseDelta.x * _rotationSpeed * Time.unscaledDeltaTime;

            float currentRot = Mathf.SmoothDampAngle(
                _pivot.eulerAngles.y,
                _targetRotation,
                ref _rotationVelocity,
                _rotationSmoothTime
            );
            _pivot.rotation = Quaternion.Euler(_tiltAngle, currentRot, 0);
        }

        private void HandleZoom()
        {
            if (Mathf.Abs(_scrollY) < 0.01f) return;

            _targetZoom -= _scrollY * _zoomSpeed;
            _targetZoom = Mathf.Clamp(_targetZoom, _minZoom, _maxZoom);

            float currentZoom = Mathf.SmoothDamp(
                _cam.transform.localPosition.y,
                _targetZoom,
                ref _zoomVelocity,
                _zoomSmoothTime
            );
            _cam.transform.localPosition = new Vector3(0, currentZoom, 0);
        }

        private void HandleEdgePan()
        {
            if (_isPanning || _isRotating) return;
            if (_mousePos.x <= 1 || _mousePos.y <= 1) return;
            if (_mousePos.x >= Screen.width - 1 || _mousePos.y >= Screen.height - 1) return;

            Vector3 edgeDir = Vector3.zero;

            if (_mousePos.x < _edgePanZone) edgeDir -= _pivot.right;
            if (_mousePos.x > Screen.width - _edgePanZone) edgeDir += _pivot.right;
            if (_mousePos.y < _edgePanZone) edgeDir -= _pivot.forward;
            if (_mousePos.y > Screen.height - _edgePanZone) edgeDir += _pivot.forward;

            edgeDir.y = 0;
            if (edgeDir.sqrMagnitude < 0.01f) return;
            edgeDir.Normalize();

            _targetPosition += edgeDir * _edgePanSpeed * Time.unscaledDeltaTime;
        }

        private void ApplyBounds()
        {
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, _boundsMin.x, _boundsMax.x);
            _targetPosition.y = 0;
            _targetPosition.z = Mathf.Clamp(_targetPosition.z, _boundsMin.z, _boundsMax.z);

            transform.position = Vector3.SmoothDamp(
                transform.position,
                _targetPosition,
                ref _panVelocity,
                _panSmoothTime
            );
            _pivot.position = transform.position;
        }

        private Vector3 GetWorldPoint(Vector2 screenPos)
        {
            var cam = UnityEngine.Camera.main;
            if (cam == null) return Vector3.zero;

            Ray ray = cam.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
                return hit.point;

            return Vector3.zero;
        }

        public void SetBounds(Vector3 min, Vector3 max)
        {
            _boundsMin = min;
            _boundsMax = max;
        }

        private void OnDestroy()
        {
            _panAction?.Disable();
            _zoomAction?.Disable();
            _rotateAction?.Disable();
            _rotateButtonAction?.Disable();
            _mousePosAction?.Disable();
        }
    }
}
