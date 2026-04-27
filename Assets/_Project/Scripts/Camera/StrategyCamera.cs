using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace HollowGround.Camera
{
    public class StrategyCamera : MonoBehaviour
    {
        [Header("Camera Reference")]
        [SerializeField] private UnityEngine.Camera _cam;

        [Header("Pan")]
        [SerializeField] private float _keyPanSpeed    = 25f;
        [SerializeField] private float _mousePanSpeed  = 0.06f;
        [SerializeField] private float _panSmoothing   = 8f;

        [Header("Edge Scroll")]
        [SerializeField] private bool  _edgeScrollEnabled = true;
        [SerializeField] private float _edgeZone           = 20f;
        [SerializeField] private float _edgeSpeed          = 20f;

        [Header("Zoom")]
        [SerializeField] private float _zoomSpeed    = 5f;
        [SerializeField] private float _minZoom      = 5f;
        [SerializeField] private float _maxZoom      = 45f;
        [SerializeField] private float _zoomSmooth   = 8f;
        [SerializeField] private float _initialZoom  = 12f;

        [Header("Rotation")]
        [SerializeField] private float _rotateSpeed  = 120f;
        [SerializeField] private float _rotateSmooth = 8f;

        [Header("Camera Angle")]
        [SerializeField] private float _tiltAngle = 60f;

        [Header("Bounds")]
        [SerializeField] private Vector2 _boundsMin = new(-30f, -30f);
        [SerializeField] private Vector2 _boundsMax = new(130f, 130f);

        private Vector3 _targetPos;
        private float   _targetYaw;
        private float   _targetZoom;

        private Vector3 _currentPos;
        private float   _currentYaw;
        private float   _currentZoom;

        private bool    _middleDragging;
        private Vector2 _lastMousePos;

        private bool    _rightDragging;

        private void Awake()
        {
            if (_cam == null)
                _cam = GetComponentInChildren<UnityEngine.Camera>();

            _targetPos  = transform.position;
            _targetYaw  = transform.eulerAngles.y;
            _targetZoom = _initialZoom;

            _currentPos  = _targetPos;
            _currentYaw  = _targetYaw;
            _currentZoom = _targetZoom;

            ApplyTransform();
        }

        private void Update()
        {
            if (Mouse.current == null || Keyboard.current == null) return;

            HandleDragState();
            HandleKeyboardPan();
            HandleMousePan();
            HandleEdgeScroll();
            HandleRotation();
            HandleZoom();
            ClampBounds();
            SmoothApply();
        }

        // ── Drag state ─────────────────────────────────────────────────────────

        private void HandleDragState()
        {
            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

            // Orta mouse → pan
            if (Mouse.current.middleButton.wasPressedThisFrame && !overUI)
            {
                _middleDragging = true;
                _lastMousePos   = Mouse.current.position.ReadValue();
            }
            if (Mouse.current.middleButton.wasReleasedThisFrame)
                _middleDragging = false;

            // Sağ mouse → döndür
            if (Mouse.current.rightButton.wasPressedThisFrame && !overUI)
            {
                _rightDragging = true;
                _lastMousePos  = Mouse.current.position.ReadValue();
            }
            if (Mouse.current.rightButton.wasReleasedThisFrame)
                _rightDragging = false;
        }

        // ── Pan ────────────────────────────────────────────────────────────────

        private void HandleKeyboardPan()
        {
            var kb = Keyboard.current;
            Vector2 input = Vector2.zero;

            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    input.y += 1;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  input.y -= 1;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  input.x -= 1;

            if (input.sqrMagnitude < 0.01f) return;

            float speedScale = _currentZoom / _initialZoom;
            _targetPos += GetMoveDir(input) * (_keyPanSpeed * speedScale * Time.unscaledDeltaTime);
        }

        private void HandleMousePan()
        {
            if (!_middleDragging) return;

            Vector2 currentMousePos = Mouse.current.position.ReadValue();
            Vector2 delta = currentMousePos - _lastMousePos;
            _lastMousePos = currentMousePos;

            if (delta.sqrMagnitude < 0.01f) return;

            float scale = _currentZoom * _mousePanSpeed;
            _targetPos -= GetMoveDir(delta * scale * Time.unscaledDeltaTime);
        }

        private void HandleEdgeScroll()
        {
            if (!_edgeScrollEnabled) return;
            if (_middleDragging || _rightDragging) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Vector2 mp = Mouse.current.position.ReadValue();
            if (mp.x < 0 || mp.y < 0 || mp.x > Screen.width || mp.y > Screen.height) return;

            Vector2 edgeInput = Vector2.zero;
            if (mp.x < _edgeZone)                  edgeInput.x -= 1;
            if (mp.x > Screen.width  - _edgeZone)  edgeInput.x += 1;
            if (mp.y < _edgeZone)                   edgeInput.y -= 1;
            if (mp.y > Screen.height - _edgeZone)   edgeInput.y += 1;

            if (edgeInput.sqrMagnitude < 0.01f) return;

            float speedScale = _currentZoom / _initialZoom;
            _targetPos += GetMoveDir(edgeInput) * (_edgeSpeed * speedScale * Time.unscaledDeltaTime);
        }

        // ── Rotation ───────────────────────────────────────────────────────────

        private void HandleRotation()
        {
            if (!_rightDragging) return;

            Vector2 currentMousePos = Mouse.current.position.ReadValue();
            float deltaX = currentMousePos.x - _lastMousePos.x;
            _lastMousePos = currentMousePos;

            _targetYaw += deltaX * _rotateSpeed * Time.unscaledDeltaTime;
        }

        // ── Zoom ───────────────────────────────────────────────────────────────

        private void HandleZoom()
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) < 0.01f) return;

            _targetZoom -= scroll * _zoomSpeed;
            _targetZoom  = Mathf.Clamp(_targetZoom, _minZoom, _maxZoom);
        }

        // ── Bounds & Smooth Apply ──────────────────────────────────────────────

        private void ClampBounds()
        {
            _targetPos.x = Mathf.Clamp(_targetPos.x, _boundsMin.x, _boundsMax.x);
            _targetPos.z = Mathf.Clamp(_targetPos.z, _boundsMin.y, _boundsMax.y);
            _targetPos.y = 0f;
        }

        private void SmoothApply()
        {
            float t = Time.unscaledDeltaTime;

            _currentPos  = Vector3.Lerp(_currentPos,  _targetPos,  _panSmoothing    * t);
            _currentYaw  = Mathf.LerpAngle(_currentYaw, _targetYaw, _rotateSmooth   * t);
            _currentZoom = Mathf.Lerp(_currentZoom,  _targetZoom,  _zoomSmooth      * t);

            ApplyTransform();
        }

        private void ApplyTransform()
        {
            transform.position = _currentPos;
            transform.rotation = Quaternion.Euler(0f, _currentYaw, 0f);

            if (_cam != null)
            {
                float rad    = _tiltAngle * Mathf.Deg2Rad;
                float height = _currentZoom * Mathf.Sin(rad);
                float depth  = _currentZoom * Mathf.Cos(rad);

                _cam.transform.localPosition = new Vector3(0f, height, -depth);
                _cam.transform.localRotation = Quaternion.Euler(_tiltAngle, 0f, 0f);
            }
        }

        // ── Yardımcı ───────────────────────────────────────────────────────────

        private Vector3 GetMoveDir(Vector2 input)
        {
            float yawRad = _currentYaw * Mathf.Deg2Rad;
            Vector3 forward = new Vector3( Mathf.Sin(yawRad), 0f, Mathf.Cos(yawRad));
            Vector3 right   = new Vector3( Mathf.Cos(yawRad), 0f,-Mathf.Sin(yawRad));
            return forward * input.y + right * input.x;
        }

        public void FocusOn(Vector3 worldPos)
        {
            Vector3 pos = new(worldPos.x, 0f, worldPos.z);
            _targetPos = pos;
            _currentPos = pos;
            ApplyTransform();
        }

        public void SetBounds(Vector2 min, Vector2 max)
        {
            _boundsMin = min;
            _boundsMax = max;
        }
    }
}
