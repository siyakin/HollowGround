using HollowGround.Grid;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace HollowGround.Editor
{
    public static class GroundSetupEditor
    {
        const string GROUND_LAYER = "Ground";

        [MenuItem("HollowGround/Setup Ground & Camera")]
        public static void SetupGroundAndCamera()
        {
            CreateGroundLayer();
            GameObject ground = CreateGroundPlane();
            SetupCameraRig();
            SetupLighting();

            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("[GroundSetup] Ground plane + camera + lighting configured.");
        }

        static void CreateGroundLayer()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            int groundLayerIndex = -1;
            for (int i = 8; i < layersProp.arraySize; i++)
            {
                SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(i);
                if (layerProp.stringValue == GROUND_LAYER)
                {
                    groundLayerIndex = i;
                    break;
                }
            }

            if (groundLayerIndex == -1)
            {
                for (int i = 8; i < layersProp.arraySize; i++)
                {
                    SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(i);
                    if (string.IsNullOrEmpty(layerProp.stringValue))
                    {
                        layerProp.stringValue = GROUND_LAYER;
                        groundLayerIndex = i;
                        break;
                    }
                }
            }

            tagManager.ApplyModifiedProperties();

            if (groundLayerIndex != -1)
                Debug.Log($"[GroundSetup] Layer '{GROUND_LAYER}' = {groundLayerIndex}");
        }

        static GameObject CreateGroundPlane()
        {
            GameObject groundGO = GameObject.Find("Ground");

            if (groundGO == null)
            {
                groundGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
                groundGO.name = "Ground";
            }

            var gridSystem = Object.FindAnyObjectByType<HollowGround.Grid.GridSystem>();
            float gridW = gridSystem != null ? gridSystem.Width * gridSystem.CellSize : 100f;
            float gridH = gridSystem != null ? gridSystem.Height * gridSystem.CellSize : 100f;
            float groundScale = Mathf.Max(gridW, gridH) / 10f + 2f;

            groundGO.transform.position = new Vector3(gridW * 0.5f, -0.5f, gridH * 0.5f);
            groundGO.transform.localScale = new Vector3(groundScale, 1f, groundScale);

            groundGO.layer = LayerMask.NameToLayer(GROUND_LAYER);
            foreach (Transform child in groundGO.transform)
                child.gameObject.layer = LayerMask.NameToLayer(GROUND_LAYER);

            if (groundGO.GetComponent<Renderer>()?.sharedMaterial?.name != "GroundMaterial")
            {
                Renderer renderer = groundGO.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material groundMat = new(Shader.Find("Universal Render Pipeline/Lit"));
                    groundMat.name = "GroundMaterial";
                    groundMat.color = new Color(0.28f, 0.24f, 0.18f, 1f);
                    renderer.sharedMaterial = groundMat;
                }
            }

            Collider collider = groundGO.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = true;

            Debug.Log($"[GroundSetup] Ground: pos=({gridW * 0.5f},-0.5,{gridH * 0.5f}) scale={groundScale:F0} ({groundScale * 10}x{groundScale * 10} units)");
            return groundGO;
        }

        static void SetupCameraRig()
        {
            var strategyCam = Object.FindAnyObjectByType<HollowGround.Camera.StrategyCamera>();
            if (strategyCam == null)
            {
                Debug.LogWarning("[GroundSetup] StrategyCamera not found in scene.");
                return;
            }

            var gridSystem = Object.FindAnyObjectByType<HollowGround.Grid.GridSystem>();
            float gridW = gridSystem != null ? gridSystem.Width * gridSystem.CellSize : 100f;
            float gridH = gridSystem != null ? gridSystem.Height * gridSystem.CellSize : 100f;
            float margin = 15f;

            SerializedObject so = new SerializedObject(strategyCam);

            var boundsMin = so.FindProperty("_boundsMin");
            if (boundsMin != null) boundsMin.vector2Value = new Vector2(-margin, -margin);

            var boundsMax = so.FindProperty("_boundsMax");
            if (boundsMax != null) boundsMax.vector2Value = new Vector2(gridW + margin, gridH + margin);

            var initialZoom = so.FindProperty("_initialZoom");
            if (initialZoom != null) initialZoom.floatValue = 35f;

            so.ApplyModifiedProperties();

            Vector3 center = new(gridW * 0.5f, 0f, gridH * 0.5f);
            strategyCam.transform.position = center;
            strategyCam.FocusOn(center);

            EditorUtility.SetDirty(strategyCam);
            Debug.Log($"[GroundSetup] Camera bounds=[{-margin} to {gridW + margin}, {-margin} to {gridH + margin}], centered at {center}");
        }

        static void SetupLighting()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.3f, 0.27f, 0.22f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = new Color(0.22f, 0.20f, 0.17f, 1f);
            RenderSettings.fogDensity = 0.004f;
        }
    }
}
