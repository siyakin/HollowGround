using System.Collections.Generic;
using HollowGround.Buildings;
using HollowGround.Camera;
using HollowGround.Core;
using HollowGround.Grid;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HollowGround.UI
{
    public class MinimapUI : MonoBehaviour, IPointerClickHandler, IDragHandler
    {
        [SerializeField] private RenderTexture _renderTexture;
        [SerializeField] private float _minimapSize = 220f;
        [SerializeField] private float _marginRight = 15f;
        [SerializeField] private float _marginTop = 50f;
        [SerializeField] private Color _frameColor = new(0.12f, 0.13f, 0.14f, 0.9f);
        [SerializeField] private Color _viewportColor = new(1f, 1f, 1f, 0.6f);
        [SerializeField] private float _viewportLineWidth = 2f;

        private RawImage _rawImage;
        private RectTransform _viewportFrame;
        private StrategyCamera _strategyCamera;
        private UnityEngine.Camera _mainCam;
        private GridSystem _gridSystem;
        private Image _markerLayer;
        private Texture2D _markerTexture;
        private RawImage _fogOverlay;
        private float _worldSizeX;
        private float _worldSizeZ;

        private readonly List<(Vector3 pos, Color color, float size)> _markers = new();

        private void Awake()
        {
            BuildUI();
            FindDependencies();
        }

        private void Start()
        {
            RefreshMarkers();
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.OnBuildingAdded += OnBuildingChanged;
                BuildingManager.Instance.OnBuildingRemoved += OnBuildingChanged;
            }
        }

        private void OnDestroy()
        {
            if (_markerTexture != null) Destroy(_markerTexture);
            if (BuildingManager.Instance != null)
            {
                BuildingManager.Instance.OnBuildingAdded -= OnBuildingChanged;
                BuildingManager.Instance.OnBuildingRemoved -= OnBuildingChanged;
            }
        }

        private void OnBuildingChanged(Building _) => RefreshMarkers();

        private void BuildUI()
        {
            var rt = GetComponent<RectTransform>();
            if (rt == null) rt = gameObject.AddComponent<RectTransform>();

            UIPrimitiveFactory.SetAnchors(rt,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f));
            rt.sizeDelta = new Vector2(_minimapSize, _minimapSize);
            rt.anchoredPosition = new Vector2(-_marginRight, -_marginTop);
            rt.pivot = new Vector2(1f, 1f);

            var frameImg = gameObject.AddComponent<Image>();
            frameImg.color = _frameColor;
            frameImg.raycastTarget = true;

            var borderGO = new GameObject("Border", typeof(RectTransform));
            borderGO.transform.SetParent(transform, false);
            var borderRt = borderGO.GetComponent<RectTransform>();
            UIPrimitiveFactory.StretchFull(borderRt, new Vector2(-2, -2), new Vector2(2, 2));
            var borderImg = borderGO.AddComponent<Image>();
            borderImg.color = new Color(0.83f, 0.52f, 0.04f, 0.7f);
            borderImg.raycastTarget = false;

            var mapGO = new GameObject("MapDisplay", typeof(RectTransform));
            mapGO.transform.SetParent(transform, false);
            var mapRt = mapGO.GetComponent<RectTransform>();
            UIPrimitiveFactory.StretchFull(mapRt, new Vector2(3, 3), new Vector2(-3, -3));
            _rawImage = mapGO.AddComponent<RawImage>();
            _rawImage.texture = _renderTexture;
            _rawImage.raycastTarget = false;

            var fogRt = UIPrimitiveFactory.CreateUIObject("FogOverlay", transform);
            UIPrimitiveFactory.StretchFull(fogRt, new Vector2(3, 3), new Vector2(-3, -3));
            _fogOverlay = fogRt.gameObject.AddComponent<RawImage>();
            _fogOverlay.raycastTarget = false;
            _fogOverlay.color = Color.white;

            var markerGO = new GameObject("Markers", typeof(RectTransform));
            markerGO.transform.SetParent(transform, false);
            var markerRt = markerGO.GetComponent<RectTransform>();
            UIPrimitiveFactory.StretchFull(markerRt, new Vector2(3, 3), new Vector2(-3, -3));
            _markerLayer = markerGO.AddComponent<Image>();
            _markerLayer.raycastTarget = false;

            var vpGO = new GameObject("ViewportFrame", typeof(RectTransform));
            vpGO.transform.SetParent(transform, false);
            _viewportFrame = vpGO.GetComponent<RectTransform>();
            UIPrimitiveFactory.StretchFull(_viewportFrame, new Vector2(3, 3), new Vector2(-3, -3));
            var vpImg = vpGO.AddComponent<Image>();
            vpImg.color = Color.clear;
            vpImg.raycastTarget = false;

            var borderTL = new GameObject("B_TL", typeof(RectTransform));
            borderTL.transform.SetParent(_viewportFrame, false);
            var tlRt = borderTL.GetComponent<RectTransform>();
            tlRt.anchorMin = new Vector2(0, 1); tlRt.anchorMax = new Vector2(1, 1);
            tlRt.pivot = new Vector2(0.5f, 1f);
            tlRt.sizeDelta = new Vector2(0, _viewportLineWidth);
            var tlImg = borderTL.AddComponent<Image>();
            tlImg.color = _viewportColor;
            tlImg.raycastTarget = false;

            var borderBL = new GameObject("B_BL", typeof(RectTransform));
            borderBL.transform.SetParent(_viewportFrame, false);
            var blRt = borderBL.GetComponent<RectTransform>();
            blRt.anchorMin = new Vector2(0, 0); blRt.anchorMax = new Vector2(1, 0);
            blRt.pivot = new Vector2(0.5f, 0f);
            blRt.sizeDelta = new Vector2(0, _viewportLineWidth);
            var blImg = borderBL.AddComponent<Image>();
            blImg.color = _viewportColor;
            blImg.raycastTarget = false;

            var borderLT = new GameObject("B_LT", typeof(RectTransform));
            borderLT.transform.SetParent(_viewportFrame, false);
            var ltRt = borderLT.GetComponent<RectTransform>();
            ltRt.anchorMin = new Vector2(0, 0); ltRt.anchorMax = new Vector2(0, 1);
            ltRt.pivot = new Vector2(0f, 0.5f);
            ltRt.sizeDelta = new Vector2(_viewportLineWidth, 0);
            var ltImg = borderLT.AddComponent<Image>();
            ltImg.color = _viewportColor;
            ltImg.raycastTarget = false;

            var borderRT = new GameObject("B_RT", typeof(RectTransform));
            borderRT.transform.SetParent(_viewportFrame, false);
            var rtRt = borderRT.GetComponent<RectTransform>();
            rtRt.anchorMin = new Vector2(1, 0); rtRt.anchorMax = new Vector2(1, 1);
            rtRt.pivot = new Vector2(1f, 0.5f);
            rtRt.sizeDelta = new Vector2(_viewportLineWidth, 0);
            var rtImg = borderRT.AddComponent<Image>();
            rtImg.color = _viewportColor;
            rtImg.raycastTarget = false;
        }

        private void FindDependencies()
        {
            _strategyCamera = FindAnyObjectByType<StrategyCamera>();
            _gridSystem = GridSystem.Instance;

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
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused) return;
            UpdateViewportFrame();
            TryBindFogTexture();
        }

        private void TryBindFogTexture()
        {
            if (_fogOverlay == null) return;
            if (_fogOverlay.texture != null) return;
            var fog = SceneFogManager.Instance;
            if (fog == null || !fog.IsInitialized) return;
            _fogOverlay.texture = fog.FogTexture;
        }

        private void UpdateViewportFrame()
        {
            if (_strategyCamera == null || _viewportFrame == null) return;

            if (_mainCam == null)
                _mainCam = _strategyCamera.GetComponentInChildren<UnityEngine.Camera>();
            if (_mainCam == null) return;

            float[] corners = GetCameraFrustumCorners(_mainCam);
            if (corners == null) return;

            float minX = corners[0], maxX = corners[1], minZ = corners[2], maxZ = corners[3];

            Vector2 center = WorldToMinimap((minX + maxX) * 0.5f, (minZ + maxZ) * 0.5f);
            Vector2 size = new Vector2(
                (maxX - minX) / _worldSizeX,
                (maxZ - minZ) / _worldSizeZ);

            _viewportFrame.anchorMin = new Vector2(
                Mathf.Clamp01(center.x - size.x * 0.5f),
                Mathf.Clamp01(center.y - size.y * 0.5f));
            _viewportFrame.anchorMax = new Vector2(
                Mathf.Clamp01(center.x + size.x * 0.5f),
                Mathf.Clamp01(center.y + size.y * 0.5f));
            _viewportFrame.offsetMin = Vector2.zero;
            _viewportFrame.offsetMax = Vector2.zero;
        }

        private float[] GetCameraFrustumCorners(UnityEngine.Camera cam)
        {
            Vector3[] frustumCorners = new Vector3[4];
            cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.farClipPlane, UnityEngine.Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

            Vector3 camPos = cam.transform.position;

            float minX = float.MaxValue, maxX = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;

            for (int i = 0; i < 4; i++)
            {
                Vector3 worldCorner = cam.transform.TransformPoint(frustumCorners[i]);
                float denom = worldCorner.y - camPos.y;
                if (Mathf.Abs(denom) < 0.001f) continue;
                float rayLength = -camPos.y / denom;
                if (rayLength <= 0) continue;

                Vector3 groundHit = camPos + (worldCorner - camPos) * rayLength;
                minX = Mathf.Min(minX, groundHit.x);
                maxX = Mathf.Max(maxX, groundHit.x);
                minZ = Mathf.Min(minZ, groundHit.z);
                maxZ = Mathf.Max(maxZ, groundHit.z);
            }

            if (minX == float.MaxValue) return null;
            return new float[] { minX, maxX, minZ, maxZ };
        }

        private Vector2 WorldToMinimap(float worldX, float worldZ)
        {
            return new Vector2(
                (worldX) / _worldSizeX,
                (worldZ) / _worldSizeZ);
        }

        private Vector3 MinimapToWorld(Vector2 minimapPos)
        {
            return new Vector3(
                minimapPos.x * _worldSizeX,
                0f,
                minimapPos.y * _worldSizeZ);
        }

        private Vector2 ScreenToMinimapLocal(Vector2 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                GetComponent<RectTransform>(), screenPos, null, out Vector2 localPos);

            Rect rect = GetComponent<RectTransform>().rect;
            return new Vector2(
                (localPos.x - rect.xMin) / rect.width,
                (localPos.y - rect.yMin) / rect.height);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            NavigateToClick(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            NavigateToClick(eventData.position);
        }

        private void NavigateToClick(Vector2 screenPos)
        {
            if (_strategyCamera == null) return;

            Vector2 minimapPos = ScreenToMinimapLocal(screenPos);
            if (minimapPos.x < 0 || minimapPos.x > 1 || minimapPos.y < 0 || minimapPos.y > 1) return;

            Vector3 worldTarget = MinimapToWorld(minimapPos);
            _strategyCamera.FocusOn(worldTarget);
        }

        private void RefreshMarkers()
        {
            _markers.Clear();

            if (BuildingManager.Instance == null) return;

            foreach (var building in BuildingManager.Instance.AllBuildings)
            {
                if (building.State == BuildingState.Destroyed) continue;

                Color color = GetBuildingMarkerColor(building);
                float size = Mathf.Max(building.Data.SizeX, building.Data.SizeZ);
                _markers.Add((building.transform.position, color, size));
            }

            DrawMarkers();
        }

        private Color GetBuildingMarkerColor(Building building)
        {
            if (building.State == BuildingState.Constructing)
                return UIColors.GetStateColor(BuildingState.Constructing);
            if (building.State == BuildingState.Damaged)
                return UIColors.GetStateColor(BuildingState.Damaged);
            if (building.State == BuildingState.Upgrading)
                return UIColors.GetStateColor(BuildingState.Upgrading);

            return building.Data.Type switch
            {
                BuildingType.CommandCenter => UIColors.Default.Gold,
                BuildingType.Farm => new Color(0.4f, 0.85f, 0.3f, 1f),
                BuildingType.Mine => new Color(0.7f, 0.7f, 0.8f, 1f),
                BuildingType.Barracks => new Color(0.9f, 0.35f, 0.25f, 1f),
                BuildingType.Generator => new Color(1f, 0.9f, 0.2f, 1f),
                BuildingType.Storage => new Color(0.6f, 0.5f, 0.3f, 1f),
                BuildingType.Shelter => new Color(0.5f, 0.65f, 0.7f, 1f),
                BuildingType.Hospital => new Color(0.3f, 0.9f, 0.5f, 1f),
                BuildingType.WaterWell => new Color(0.3f, 0.6f, 0.95f, 1f),
                BuildingType.WoodFactory => new Color(0.6f, 0.4f, 0.2f, 1f),
                _ => UIColors.Default.Text
            };
        }

        private void DrawMarkers()
        {
            if (_markerLayer == null) return;

            int texSize = 256;
            if (_markerTexture == null)
            {
                _markerTexture = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
                _markerTexture.filterMode = FilterMode.Point;
                _markerLayer.sprite = Sprite.Create(_markerTexture,
                    new Rect(0, 0, texSize, texSize), new Vector2(0.5f, 0.5f));
            }

            Color[] pixels = new Color[texSize * texSize];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            foreach (var marker in _markers)
            {
                int px = Mathf.RoundToInt((marker.pos.x / _worldSizeX) * texSize);
                int py = Mathf.RoundToInt((marker.pos.z / _worldSizeZ) * texSize);
                int radius = Mathf.Max(1, Mathf.RoundToInt(marker.size * 0.8f));

                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        int x = px + dx;
                        int y = py + dy;
                        if (x < 0 || x >= texSize || y < 0 || y >= texSize) continue;
                        pixels[y * texSize + x] = marker.color;
                    }
                }
            }

            _markerTexture.SetPixels(pixels);
            _markerTexture.Apply();
        }

    }
}
