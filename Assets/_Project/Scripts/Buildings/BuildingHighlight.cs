using System.Collections.Generic;
using HollowGround.UI;
using UnityEngine;

namespace HollowGround.Buildings
{
    public class BuildingHighlight : MonoBehaviour
    {
        [SerializeField] private Color _highlightColor;
        [SerializeField] private float _highlightAlpha = 0.15f;
        [SerializeField] private float _pulseSpeed = 2f;
        [SerializeField] private float _scaleFactor = 1.05f;

        private Building _building;
        private BuildingSelector _selector;
        private GameObject _outlineRoot;
        private Renderer[] _outlineRenderers;
        private MaterialPropertyBlock _mpb;
        private Material _outlineMaterial;
        private bool _isHighlighted;

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        private void Reset()
        {
            _highlightColor = UIColors.Selected;
        }

        private void Awake()
        {
            _building = GetComponent<Building>();
            _mpb = new MaterialPropertyBlock();

            if (_highlightColor == default)
                _highlightColor = UIColors.Selected;
        }

        private void OnEnable()
        {
            _selector = FindAnyObjectByType<BuildingSelector>();
            if (_selector != null)
            {
                _selector.OnBuildingSelected += OnBuildingSelected;
                _selector.OnBuildingDeselected += OnBuildingDeselected;
            }

            if (_building != null)
            {
                _building.OnConstructionComplete += OnModelChanged;
                _building.OnUpgradeComplete += OnModelChanged;
                _building.OnDamaged += OnModelChanged;
                _building.OnRepaired += OnModelChanged;
                _building.OnDestroyed += OnBuildingDestroyed;
            }
        }

        private void OnDisable()
        {
            if (_selector != null)
            {
                _selector.OnBuildingSelected -= OnBuildingSelected;
                _selector.OnBuildingDeselected -= OnBuildingDeselected;
            }

            if (_building != null)
            {
                _building.OnConstructionComplete -= OnModelChanged;
                _building.OnUpgradeComplete -= OnModelChanged;
                _building.OnDamaged -= OnModelChanged;
                _building.OnRepaired -= OnModelChanged;
                _building.OnDestroyed -= OnBuildingDestroyed;
            }

            DestroyOutline();
        }

        private void OnDestroy()
        {
            if (_outlineMaterial != null)
                Destroy(_outlineMaterial);
        }

        private void Update()
        {
            if (!_isHighlighted || _outlineRenderers == null) return;

            float pulse = Mathf.Sin(Time.time * _pulseSpeed) * 0.5f + 0.5f;
            float alpha = Mathf.Lerp(0.1f, 0.2f, pulse);
            Color c = new Color(_highlightColor.r, _highlightColor.g, _highlightColor.b, alpha);

            foreach (var rend in _outlineRenderers)
            {
                if (rend == null) continue;
                rend.GetPropertyBlock(_mpb);
                _mpb.SetColor(BaseColor, c);
                rend.SetPropertyBlock(_mpb);
            }
        }

        private void OnBuildingSelected(Building building)
        {
            if (building != _building) return;
            if (BuildingPlacer.Instance != null && BuildingPlacer.Instance.IsPlacing) return;

            _isHighlighted = true;
            CreateOutline();
        }

        private void OnBuildingDeselected()
        {
            _isHighlighted = false;
            DestroyOutline();
        }

        private void OnModelChanged(Building _)
        {
            if (!_isHighlighted) return;
            DestroyOutline();
            CreateOutline();
        }

        private void OnBuildingDestroyed(Building _)
        {
            _isHighlighted = false;
            DestroyOutline();
        }

        private void CreateOutline()
        {
            DestroyOutline();

            var meshFilters = GetComponentsInChildren<MeshFilter>();
            if (meshFilters.Length == 0) return;

            if (_outlineMaterial == null)
                _outlineMaterial = CreateHighlightMaterial();

            _outlineRoot = new GameObject("BuildingOutline");
            _outlineRoot.transform.SetParent(transform);
            _outlineRoot.transform.localPosition = Vector3.zero;
            _outlineRoot.transform.localRotation = Quaternion.identity;
            _outlineRoot.transform.localScale = Vector3.one * _scaleFactor;

            var renderers = new List<Renderer>();
            Vector3 buildingScale = transform.lossyScale;

            foreach (var mf in meshFilters)
            {
                if (mf.sharedMesh == null) continue;

                var go = new GameObject("OutlineMesh");
                go.transform.SetParent(_outlineRoot.transform);

                Vector3 localPos = transform.InverseTransformPoint(mf.transform.position);
                Quaternion localRot = Quaternion.Inverse(transform.rotation) * mf.transform.rotation;
                Vector3 meshScale = mf.transform.lossyScale;
                Vector3 localScale = new Vector3(
                    buildingScale.x > 0f ? meshScale.x / buildingScale.x : 1f,
                    buildingScale.y > 0f ? meshScale.y / buildingScale.y : 1f,
                    buildingScale.z > 0f ? meshScale.z / buildingScale.z : 1f
                );

                go.transform.localPosition = localPos;
                go.transform.localRotation = localRot;
                go.transform.localScale = localScale;

                var newMf = go.AddComponent<MeshFilter>();
                newMf.sharedMesh = mf.sharedMesh;

                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = _outlineMaterial;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = false;

                renderers.Add(mr);
            }

            _outlineRenderers = renderers.ToArray();
        }

        private void DestroyOutline()
        {
            if (_outlineRoot != null)
            {
                Destroy(_outlineRoot);
                _outlineRoot = null;
            }
            _outlineRenderers = null;
        }

        private Material CreateHighlightMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Sprites/Default");

            var mat = new Material(shader);
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            Color c = new Color(_highlightColor.r, _highlightColor.g, _highlightColor.b, _highlightAlpha);
            mat.SetColor("_BaseColor", c);

            return mat;
        }
    }
}
