using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using HollowGround.Buildings;
namespace HollowGround.Grid
{
    public class GridOverlayRenderer : MonoBehaviour
    {
        [SerializeField] private GridSystem _gridSystem;
        [SerializeField] private Color _lineColor = new Color(0.35f, 0.32f, 0.28f, 0.3f);
        [SerializeField] private Color _validCellColor = new Color(0.2f, 0.8f, 0.2f, 0.25f);
        [SerializeField] private Color _invalidCellColor = new Color(0.8f, 0.2f, 0.2f, 0.25f);
        [SerializeField] private float _fadeDuration = 0.3f;
        [SerializeField] private int _visibleRadius = 30;
        [SerializeField] private float _lineWidth = 0.025f;

        private LineRenderer _horizontalLR;
        private LineRenderer _verticalLR;
        private Material _lineMaterial;
        private GameObject _cellHighlight;
        private MeshRenderer _cellHighlightRenderer;
        private Material _validMat;
        private Material _invalidMat;

        private float _currentAlpha;
        private float _targetAlpha;
        private Vector2Int _lastCamCell = new Vector2Int(-999, -999);
        private const float YPos = 0.02f;
        private List<GameObject> _footprintHighlights = new();

        private void Awake()
        {
            _lineMaterial = new Material(Shader.Find("Sprites/Default"));
            _lineMaterial.color = new Color(_lineColor.r, _lineColor.g, _lineColor.b, 0f);
            _horizontalLR = CreateLineRenderer("HLines");
            _verticalLR = CreateLineRenderer("VLines");
            SetRenderersActive(false);

            _validMat = new Material(Shader.Find("Sprites/Default"));
            _validMat.color = _validCellColor;
            _invalidMat = new Material(Shader.Find("Sprites/Default"));
            _invalidMat.color = _invalidCellColor;

            SetupCellHighlight();
        }

        private void Start()
        {
            if (_gridSystem == null)
                _gridSystem = FindAnyObjectByType<GridSystem>();
        }

        private void Update()
        {
            bool shouldShow = BuildingPlacer.Instance != null && BuildingPlacer.Instance.IsPlacing;

            if (shouldShow && _targetAlpha < 1f)
            {
                SetRenderersActive(true);
                _targetAlpha = 1f;
                _lastCamCell = new Vector2Int(-999, -999);
            }
            else if (!shouldShow && _targetAlpha > 0f)
            {
                _targetAlpha = 0f;
            }

            if (_currentAlpha != _targetAlpha)
            {
                _currentAlpha = Mathf.MoveTowards(_currentAlpha, _targetAlpha, Time.deltaTime / _fadeDuration);
                _lineMaterial.color = new Color(_lineColor.r, _lineColor.g, _lineColor.b, _lineColor.a * _currentAlpha);

                if (_currentAlpha <= 0f)
                {
                    SetRenderersActive(false);
                    _cellHighlight.SetActive(false);
                    return;
                }
            }

            if (_currentAlpha > 0f)
            {
                RefreshGridLines();
                UpdateCellHighlight();
            }
        }

        private void SetRenderersActive(bool active)
        {
            _horizontalLR.enabled = active;
            _verticalLR.enabled = active;
        }

        private void RefreshGridLines()
        {
            UnityEngine.Camera cam = UnityEngine.Camera.main;
            if (cam == null || _gridSystem == null) return;

            Vector2Int camCell = _gridSystem.GetGridCoordinates(cam.transform.position);
            if (camCell == _lastCamCell) return;
            _lastCamCell = camCell;

            int minX = Mathf.Max(0, camCell.x - _visibleRadius);
            int maxX = Mathf.Min(_gridSystem.Width, camCell.x + _visibleRadius);
            int minZ = Mathf.Max(0, camCell.y - _visibleRadius);
            int maxZ = Mathf.Min(_gridSystem.Height, camCell.y + _visibleRadius);

            BuildHorizontalLines(minX, maxX, minZ, maxZ);
            BuildVerticalLines(minX, maxX, minZ, maxZ);
        }

        private void UpdateCellHighlight()
        {
            Vector2Int cell = GetMouseGridCell();
            if (cell.x < 0 || cell.x >= _gridSystem.Width || cell.y < 0 || cell.y >= _gridSystem.Height)
            {
                _cellHighlight.SetActive(false);
                ClearFootprint();
                return;
            }

            BuildingPlacer placer = BuildingPlacer.Instance;
            if (placer != null && placer.IsPlacing && placer.CurrentBuilding != null)
            {
                _cellHighlight.SetActive(false);
                UpdateFootprint(cell);
                return;
            }

            _cellHighlight.SetActive(true);
            GridCell gridCell = _gridSystem.GetCell(cell.x, cell.y);
            float cs = _gridSystem.CellSize;
            Vector3 pos = _gridSystem.GetWorldPosition(cell.x, cell.y) + new Vector3(cs * 0.5f, YPos, cs * 0.5f);
            _cellHighlight.transform.position = pos;
            _cellHighlight.transform.localScale = new Vector3(cs * 0.95f, cs * 0.95f, 1f);
            _cellHighlightRenderer.material = gridCell.IsBuildable ? _validMat : _invalidMat;
            ClearFootprint();
        }

        private void UpdateFootprint(Vector2Int origin)
        {
            BuildingData data = BuildingPlacer.Instance.CurrentBuilding;
            int rot = BuildingPlacer.Instance.CurrentRotation;

            int sx = rot % 2 == 0 ? data.SizeX : data.SizeZ;
            int sz = rot % 2 == 0 ? data.SizeZ : data.SizeX;

            int needed = sx * sz;
            while (_footprintHighlights.Count < needed)
                _footprintHighlights.Add(CreateFootprintQuad());

            for (int i = needed; i < _footprintHighlights.Count; i++)
                _footprintHighlights[i].SetActive(false);

            float cs = _gridSystem.CellSize;
            bool allBuildable = _gridSystem.IsAreaBuildable(origin.x, origin.y, sx, sz);

            int idx = 0;
            for (int dx = 0; dx < sx; dx++)
            {
                for (int dz = 0; dz < sz; dz++)
                {
                    int cx = origin.x + dx;
                    int cz = origin.y + dz;
                    bool valid = _gridSystem.IsValidCoordinate(cx, cz) && _gridSystem.GetCell(cx, cz).IsBuildable;

                    GameObject q = _footprintHighlights[idx++];
                    Vector3 pos = _gridSystem.GetWorldPosition(cx, cz) + new Vector3(cs * 0.5f, YPos, cs * 0.5f);
                    q.transform.position = pos;
                    q.transform.localScale = new Vector3(cs * 0.95f, cs * 0.95f, 1f);
                    q.GetComponent<MeshRenderer>().material = valid ? _validMat : _invalidMat;
                    q.SetActive(true);
                }
            }
        }

        private void ClearFootprint()
        {
            foreach (var go in _footprintHighlights)
                go.SetActive(false);
        }

        private GameObject CreateFootprintQuad()
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "FootprintCell";
            quad.transform.SetParent(transform);
            quad.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Destroy(quad.GetComponent<MeshCollider>());
            var mr = quad.GetComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            quad.SetActive(false);
            return quad;
        }

        private Vector2Int GetMouseGridCell()
        {
            if (Mouse.current == null || _gridSystem == null || UnityEngine.Camera.main == null)
                return new Vector2Int(-1, -1);

            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane ground = new Plane(Vector3.up, Vector3.zero);
            if (ground.Raycast(ray, out float dist))
                return _gridSystem.GetGridCoordinates(ray.GetPoint(dist));

            return new Vector2Int(-1, -1);
        }

        private void BuildHorizontalLines(int minX, int maxX, int minZ, int maxZ)
        {
            var pts = new List<Vector3>();
            for (int z = minZ; z <= maxZ; z++)
            {
                bool forward = (z - minZ) % 2 == 0;
                pts.Add(WorldPos(forward ? minX : maxX, z));
                pts.Add(WorldPos(forward ? maxX : minX, z));
            }
            _horizontalLR.positionCount = pts.Count;
            _horizontalLR.SetPositions(pts.ToArray());
        }

        private void BuildVerticalLines(int minX, int maxX, int minZ, int maxZ)
        {
            var pts = new List<Vector3>();
            for (int x = minX; x <= maxX; x++)
            {
                bool forward = (x - minX) % 2 == 0;
                pts.Add(WorldPos(x, forward ? minZ : maxZ));
                pts.Add(WorldPos(x, forward ? maxZ : minZ));
            }
            _verticalLR.positionCount = pts.Count;
            _verticalLR.SetPositions(pts.ToArray());
        }

        private Vector3 WorldPos(int x, int z)
        {
            Vector3 p = _gridSystem.GetWorldPosition(x, z);
            p.y = YPos;
            return p;
        }

        private LineRenderer CreateLineRenderer(string lrName)
        {
            var go = new GameObject(lrName);
            go.transform.SetParent(transform, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.material = _lineMaterial;
            lr.startWidth = _lineWidth;
            lr.endWidth = _lineWidth;
            lr.useWorldSpace = true;
            lr.numCapVertices = 2;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            return lr;
        }

        private void SetupCellHighlight()
        {
            _cellHighlight = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _cellHighlight.name = "CellHighlight";
            _cellHighlight.transform.SetParent(transform);
            _cellHighlight.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Destroy(_cellHighlight.GetComponent<MeshCollider>());
            _cellHighlightRenderer = _cellHighlight.GetComponent<MeshRenderer>();
            _cellHighlightRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _cellHighlightRenderer.receiveShadows = false;
            _cellHighlight.SetActive(false);
        }
    }
}
