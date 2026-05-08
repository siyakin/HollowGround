using HollowGround.Camera;
using HollowGround.Grid;
using UnityEngine;

namespace HollowGround.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class MinimapCamera : MonoBehaviour
    {
        [SerializeField] private float _height = 120f;
        [SerializeField] private float _orthographicPadding = 10f;
        [SerializeField] private bool _rotateWithCamera;
        [SerializeField] private bool _followPlayer;

        private UnityEngine.Camera _cam;
        private StrategyCamera _strategyCamera;
        private GridSystem _gridSystem;
        private float _worldSizeX;
        private float _worldSizeZ;

        public UnityEngine.Camera Camera => _cam;

        private void Awake()
        {
            _cam = GetComponent<UnityEngine.Camera>();
            _cam.orthographic = true;
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = new Color(0.06f, 0.07f, 0.08f, 1f);
            _cam.depth = -2;
            _cam.allowHDR = false;
            _cam.allowMSAA = false;

            _strategyCamera = FindAnyObjectByType<StrategyCamera>();
            _gridSystem = GridSystem.Instance;

            CalculateWorldBounds();
        }

        private void CalculateWorldBounds()
        {
            if (_gridSystem != null)
            {
                _worldSizeX = _gridSystem.Width * _gridSystem.CellSize;
                _worldSizeZ = _gridSystem.Height * _gridSystem.CellSize;
            }
            else
            {
                _worldSizeX = 200f;
                _worldSizeZ = 200f;
            }

            float maxSize = Mathf.Max(_worldSizeX, _worldSizeZ) * 0.5f + _orthographicPadding;
            _cam.orthographicSize = maxSize;
        }

        private void LateUpdate()
        {
            if (_strategyCamera == null)
            {
                _strategyCamera = FindAnyObjectByType<StrategyCamera>();
                if (_strategyCamera == null) return;
            }

            if (_followPlayer)
            {
                Vector3 targetPos = _strategyCamera.transform.position;
                transform.position = new Vector3(targetPos.x, _height, targetPos.z);

                if (_rotateWithCamera)
                    transform.rotation = Quaternion.Euler(90f, _strategyCamera.transform.eulerAngles.y, 0f);
                else
                    transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
            else
            {
                float centerX = _worldSizeX * 0.5f;
                float centerZ = _worldSizeZ * 0.5f;
                transform.position = new Vector3(centerX, _height, centerZ);
                transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }

        public float WorldSizeX => _worldSizeX;
        public float WorldSizeZ => _worldSizeZ;
    }
}
