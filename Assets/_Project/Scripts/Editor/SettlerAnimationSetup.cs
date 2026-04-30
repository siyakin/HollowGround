using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace HollowGround.Editor
{
#if UNITY_EDITOR
    public static class SettlerAnimationSetup
    {
        private const string MENU_PATH = "HollowGround/Settlers/";
        private const string CHARACTERS_FOLDER = "Assets/_Project/Models";
        private const string CONTROLLER_PATH = "Assets/_Project/Animations/Characters/SettlerController.controller";
        private const string ANIMATIONS_FOLDER = "Assets/_Project/Animations/Characters";
        private const string WORKER_FBX = "CityPack/Worker/Worker.fbx";

        private static readonly string[] CityPackCharacters = { "Worker", "Adventurer", "Business Man", "Man", "Woman" };

        [MenuItem(MENU_PATH + "Fix: Revert All to Generic Rig")]
        public static void RevertAllToGeneric()
        {
            int fixedCount = 0;
            string[] allCharFolders = { "Worker", "Adventurer", "Business Man", "Man", "Woman",
                "Punk", "Suit", "Casual Character", "Animated Woman" };

            foreach (var charName in allCharFolders)
            {
                string folder = Path.Combine(CHARACTERS_FOLDER, "CityPack", charName).Replace('\\', '/');
                if (!Directory.Exists(folder)) continue;

                foreach (var fbx in Directory.GetFiles(folder, "*.fbx"))
                {
                    string assetPath = fbx.Replace('\\', '/');
                    var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                    if (importer == null) continue;
                    if (importer.animationType != ModelImporterAnimationType.Generic)
                    {
                        importer.animationType = ModelImporterAnimationType.Generic;
                        importer.avatarSetup = (ModelImporterAvatarSetup)1;
                        importer.SaveAndReimport();
                        fixedCount++;
                    }
                }
            }
            Debug.Log($"[SettlerSetup] Reverted {fixedCount} FBX to Generic + Avatar.");
        }

        [MenuItem(MENU_PATH + "Fix: Enable Avatar on All Characters")]
        public static void FixAvatarOnAllCharacters()
        {
            string[] characterNames = CityPackCharacters;
            int fixedCount = 0;

            foreach (var charName in characterNames)
            {
                string folder = Path.Combine(CHARACTERS_FOLDER, "CityPack", charName).Replace('\\', '/');
                if (!Directory.Exists(folder)) continue;

                var fbxFiles = Directory.GetFiles(folder, "*.fbx");
                foreach (var fbx in fbxFiles)
                {
                    string assetPath = fbx.Replace('\\', '/');
                    var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                    if (importer == null) continue;

                    if (importer.animationType == ModelImporterAnimationType.Generic
                        && (int)importer.avatarSetup != 1)
                    {
                        Debug.Log($"[SettlerSetup] Fixing Avatar for '{assetPath}' (was avatarSetup={(int)importer.avatarSetup})");
                        importer.avatarSetup = (ModelImporterAvatarSetup)1;
                        importer.SaveAndReimport();
                        fixedCount++;
                    }
                    else
                    {
                        Debug.Log($"[SettlerSetup] Skip '{assetPath}' (animationType={importer.animationType}, avatarSetup={importer.avatarSetup})");
                    }
                }
            }

            Debug.Log($"[SettlerSetup] Fixed {fixedCount} FBX imports. Now run 'Test: Spawn Animated Settler' to verify.");
        }

        [MenuItem(MENU_PATH + "Fix: Rebuild All Clips + Controllers")]
        public static void FixAndRebuildAll()
        {
            BakeFromModel("CityPack/Worker/Worker.fbx", "Characters", "SettlerController");
            BakeFromModel("CityPack/Man/Male_Casual.fbx", "Characters_Man", "SettlerController_Man");
            BakeFromModel("CityPack/Woman/Female_Alternative.fbx", "Characters_Woman", "SettlerController_Woman");
            Debug.Log("[SettlerSetup] All clips + controllers rebuilt.");
        }

        [MenuItem(MENU_PATH + "Fix: Rebuild Clips + Controller (Worker only)")]
        public static void FixAndRebuild()
        {
            BakeFromModel(WORKER_FBX, "Characters", "SettlerController");
        }

        private static void BakeFromModel(string fbxRelativePath, string animFolder, string controllerName)
        {
            string animDir = $"Assets/_Project/Animations/{animFolder}";
            string fbxPath = Path.Combine(CHARACTERS_FOLDER, fbxRelativePath).Replace('\\', '/');

            if (Directory.Exists(animDir))
            {
                foreach (var f in Directory.GetFiles(animDir, "*.anim"))
                    AssetDatabase.DeleteAsset(animDir + "/" + Path.GetFileName(f));
                var ctrl = $"{animDir}/{controllerName}.controller";
                if (File.Exists(ctrl)) AssetDatabase.DeleteAsset(ctrl);
            }
            else
            {
                Directory.CreateDirectory(animDir);
            }

            var allAssets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            if (allAssets == null || allAssets.Length == 0)
            {
                Debug.LogError($"[SettlerSetup] No assets at '{fbxPath}'.");
                return;
            }

            var byName = new System.Collections.Generic.Dictionary<string, AnimationClip>();
            var byNamePreview = new System.Collections.Generic.Dictionary<string, AnimationClip>();

            foreach (var asset in allAssets)
            {
                if (!(asset is AnimationClip clip)) continue;
                if (clip.name.StartsWith("__preview__"))
                {
                    string key = CleanName(clip.name.Substring("__preview__".Length));
                    if (!byNamePreview.ContainsKey(key)) byNamePreview[key] = clip;
                }
                else
                {
                    string key = CleanName(clip.name);
                    if (!byName.ContainsKey(key)) byName[key] = clip;
                }
            }

            foreach (var kv in byNamePreview)
                if (!byName.ContainsKey(kv.Key)) byName[kv.Key] = kv.Value;

            int extracted = 0;
            foreach (var kv in byName)
            {
                string destPath = $"{animDir}/{kv.Key}.anim";
                bool isWalk = kv.Key == "Walk" || kv.Key == "Run";
                var clean = BakeFreshClip(kv.Value, kv.Key, isWalk);
                AssetDatabase.CreateAsset(clean, destPath);
                extracted++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SettlerSetup] Baked {extracted} clips from '{fbxRelativePath}' to '{animDir}'.");
            BuildController(animDir, controllerName);
        }
            else
            {
                Directory.CreateDirectory(ANIMATIONS_FOLDER);
            }

            string fbxPath = Path.Combine(CHARACTERS_FOLDER, WORKER_FBX).Replace('\\', '/');
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            if (allAssets == null || allAssets.Length == 0)
            {
                Debug.LogError($"[SettlerSetup] No assets at '{fbxPath}'.");
                return;
            }

            var byName = new System.Collections.Generic.Dictionary<string, AnimationClip>();
            var byNamePreview = new System.Collections.Generic.Dictionary<string, AnimationClip>();

            foreach (var asset in allAssets)
            {
                if (!(asset is AnimationClip clip)) continue;
                if (clip.name.StartsWith("__preview__"))
                {
                    string key = CleanName(clip.name.Substring("__preview__".Length));
                    if (!byNamePreview.ContainsKey(key)) byNamePreview[key] = clip;
                }
                else
                {
                    string key = CleanName(clip.name);
                    if (!byName.ContainsKey(key)) byName[key] = clip;
                }
            }

            Debug.Log($"[SettlerSetup] Found {byName.Count} real clips, {byNamePreview.Count} preview clips.");

            foreach (var kv in byNamePreview)
                if (!byName.ContainsKey(kv.Key)) byName[kv.Key] = kv.Value;

            int extracted = 0;
            foreach (var kv in byName)
            {
                string destPath = $"{ANIMATIONS_FOLDER}/{kv.Key}.anim";
                bool isWalk = kv.Key == "Walk";
                var clean = BakeFreshClip(kv.Value, kv.Key, isWalk);
                AssetDatabase.CreateAsset(clean, destPath);
                extracted++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SettlerSetup] Baked {extracted} clips.");
            BuildController();
        }

        [MenuItem(MENU_PATH + "Test: Spawn Animated Settler in Scene")]
        public static void TestSpawnSettler()
        {
            var go = new GameObject("TestSettler");
            go.transform.position = Vector3.zero;

            string fbxPath = Path.Combine(CHARACTERS_FOLDER, WORKER_FBX).Replace('\\', '/');
            var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbxAsset == null)
            {
                Debug.LogError("[SettlerTest] Worker.fbx not found.");
                return;
            }

            var instance = Object.Instantiate(fbxAsset, go.transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

            Debug.Log($"[SettlerTest] Instance name: '{instance.name}'");
            Debug.Log($"[SettlerTest] Instance child count: {instance.transform.childCount}");
            for (int i = 0; i < instance.transform.childCount; i++)
                Debug.Log($"[SettlerTest]   child[{i}] = '{instance.transform.GetChild(i).name}'");

            var allAnimators = instance.GetComponentsInChildren<Animator>();
            Debug.Log($"[SettlerTest] Animators found: {allAnimators.Length}");
            foreach (var a in allAnimators)
            {
                Debug.Log($"[SettlerTest]   Animator on '{a.gameObject.name}': " +
                    $"avatar={(a.avatar != null ? a.avatar.name : "NULL")} " +
                    $"controller={(a.runtimeAnimatorController != null ? a.runtimeAnimatorController.name : "NULL")} " +
                    $"enabled={a.enabled} " +
                    $"avatarValid={(a.avatar != null ? a.avatar.isValid : "N/A")}");
            }

            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CONTROLLER_PATH);
            if (controller == null)
            {
                Debug.LogError("[SettlerTest] SettlerController not found. Run 'Fix: Rebuild Clips + Controller' first.");
                return;
            }

            Animator targetAnimator = null;
            foreach (var a in allAnimators)
            {
                if (a.avatar != null)
                {
                    targetAnimator = a;
                    break;
                }
            }

            var sourceAnimator = fbxAsset.GetComponent<Animator>();
            Debug.Log($"[SettlerTest] Source FBX asset Animator: {(sourceAnimator != null ? "found" : "null")}, " +
                $"avatar: {(sourceAnimator != null && sourceAnimator.avatar != null ? sourceAnimator.avatar.name : "NULL")}");

            Avatar avatar = null;
            if (sourceAnimator != null && sourceAnimator.avatar != null)
                avatar = sourceAnimator.avatar;

            if (avatar == null)
            {
                var allSubAssets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
                foreach (var sub in allSubAssets)
                {
                    if (sub is Avatar av)
                    {
                        avatar = av;
                        Debug.Log($"[SettlerTest] Found Avatar sub-asset: '{av.name}' isValid={av.isValid}");
                        break;
                    }
                }
            }

            if (targetAnimator == null)
            {
                targetAnimator = instance.gameObject.AddComponent<Animator>();
                Debug.Log("[SettlerTest] Created new Animator on instance root.");
            }

            if (avatar != null)
                targetAnimator.avatar = avatar;

            targetAnimator.runtimeAnimatorController = controller;
            targetAnimator.applyRootMotion = false;
            targetAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            targetAnimator.Rebind();

            Debug.Log($"[SettlerTest] Final Animator on '{targetAnimator.gameObject.name}': " +
                $"avatar={(targetAnimator.avatar != null ? targetAnimator.avatar.name : "NULL")} " +
                $"controller={targetAnimator.runtimeAnimatorController.name} " +
                $"enabled={targetAnimator.enabled}");

            var ac = controller as UnityEditor.Animations.AnimatorController;
            if (ac != null)
            {
                Debug.Log($"[SettlerTest] Controller layer count: {ac.layers.Length}");
                if (ac.layers.Length > 0)
                {
                    var sm = ac.layers[0].stateMachine;
                    Debug.Log($"[SettlerTest] States: {sm.states.Length}");
                    foreach (var s in sm.states)
                    {
                        var clip = s.state.motion as AnimationClip;
                        Debug.Log($"[SettlerTest]   State '{s.state.name}' → clip={(clip != null ? $"{clip.name} ({clip.length:F2}s, loop={clip.isLooping})" : "NULL")}");
                    }
                }
            }

            Selection.activeGameObject = go;
            SceneView.FrameLastActiveSceneView();

            Debug.Log("[SettlerTest] Press Play to test. Animator should show Walk when Speed > 0.1.");
            Debug.Log("[SettlerTest] Check Animator window (Window > Animation > Animator) to see live state.");
        }

        [MenuItem(MENU_PATH + "Test: Verify Model Hierarchy")]
        public static void VerifyHierarchy()
        {
            string fbxPath = Path.Combine(CHARACTERS_FOLDER, WORKER_FBX).Replace('\\', '/');
            var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbxAsset == null) { Debug.LogError("Worker.fbx not found."); return; }

            var instance = Object.Instantiate(fbxAsset);
            try
            {
                Debug.Log($"[Hierarchy] Root: '{instance.name}'");
                LogHierarchy(instance.transform, 0);

                var smrs = instance.GetComponentsInChildren<SkinnedMeshRenderer>();
                Debug.Log($"[Hierarchy] SkinnedMeshRenderers: {smrs.Length}");
                foreach (var smr in smrs)
                {
                    Debug.Log($"[Hierarchy]   SMR '{smr.gameObject.name}': bones={smr.bones.Length}, mesh={smr.sharedMesh?.name ?? "null"}");
                    if (smr.bones.Length > 0)
                        Debug.Log($"[Hierarchy]     bone[0]='{smr.bones[0]?.name ?? "null"}', rootBone='{smr.rootBone?.name ?? "null"}'");
                }

                var animators = instance.GetComponentsInChildren<Animator>();
                Debug.Log($"[Hierarchy] Animators: {animators.Length}");
                foreach (var a in animators)
                {
                    Debug.Log($"[Hierarchy]   Animator '{a.gameObject.name}': avatar={a.avatar?.name ?? "null"}, " +
                        $"hasAvatar={a.avatar != null}, isValid={a.avatar?.isValid ?? false}");
                    if (a.avatar != null)
                    {
                        var humanBones = a.humanScale;
                        Debug.Log($"[Hierarchy]     humanScale={humanBones} (1.0 = Generic, ~1.0 = Humanoid)");
                    }
                }
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static void LogHierarchy(Transform t, int depth)
        {
            string indent = new string(' ', depth * 2);
            string components = "";
            var compList = t.gameObject.GetComponents<Component>();
            for (int i = 0; i < compList.Length; i++)
                components += (i > 0 ? ", " : "") + compList[i].GetType().Name;
            Debug.Log($"[Hierarchy] {indent}'{t.name}' [{components}]");
            foreach (Transform child in t)
                LogHierarchy(child, depth + 1);
        }

        [MenuItem(MENU_PATH + "Debug: Log Walk Bindings")]
        public static void DebugLogWalkBindings()
        {
            var walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{ANIMATIONS_FOLDER}/Walk.anim");
            if (walkClip == null) { Debug.LogError("[SettlerSetup] Walk.anim bulunamadı."); return; }

            var bindings = AnimationUtility.GetCurveBindings(walkClip);
            Debug.Log($"[SettlerSetup] Walk.anim bindings: {bindings.Length}");
            for (int i = 0; i < Mathf.Min(10, bindings.Length); i++)
                Debug.Log($"  [{i}] path='{bindings[i].path}' type={bindings[i].type?.Name} prop='{bindings[i].propertyName}'");
        }

        [MenuItem(MENU_PATH + "Debug: Log All FBX Clip Names")]
        public static void DebugLogClipNames()
        {
            string fbxPath = Path.Combine(CHARACTERS_FOLDER, WORKER_FBX).Replace('\\', '/');
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            if (allAssets == null || allAssets.Length == 0)
            {
                Debug.LogError("[SettlerSetup] No assets found.");
                return;
            }

            var names = new System.Collections.Generic.List<string>();
            foreach (var asset in allAssets)
                if (asset is AnimationClip clip)
                    names.Add($"{clip.name} (type={clip.GetType().Name})");

            Debug.Log($"[SettlerSetup] {names.Count} clips:\n  {string.Join("\n  ", names)}");
        }

        private static void BuildController(string animDir, string controllerName)
        {
            string controllerPath = $"{animDir}/{controllerName}.controller";
            var idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{animDir}/Idle.anim");
            var walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{animDir}/Walk.anim");
            var femaleIdle = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{animDir}/Female_Idle.anim");
            var femaleWalk = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{animDir}/Female_Walk.anim");

            if (femaleIdle != null) idleClip = femaleIdle;
            if (femaleWalk != null) walkClip = femaleWalk;

            if (idleClip == null || walkClip == null)
            {
                Debug.LogError($"[SettlerSetup] Idle or Walk clip not found in '{animDir}'.");
                return;
            }

            if (File.Exists(controllerPath))
            {
                var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
                if (existing != null)
                {
                    foreach (var layer in existing.layers)
                    {
                        foreach (var cs in layer.stateMachine.states)
                        {
                            if (cs.state.name == "Idle") cs.state.motion = idleClip;
                            else if (cs.state.name == "Walk") cs.state.motion = walkClip;
                        }
                    }
                    EditorUtility.SetDirty(existing);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"[SettlerSetup] Controller updated — Idle: {idleClip.GetType().Name}, Walk: {walkClip.GetType().Name}");
                    return;
                }
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

            var sm = controller.layers[0].stateMachine;

            var idleState = sm.AddState("Idle");
            idleState.motion = idleClip;

            var walkState = sm.AddState("Walk");
            walkState.motion = walkClip;

            var idleToWalk = idleState.AddTransition(walkState);
            idleToWalk.hasExitTime = false;
            idleToWalk.duration = 0.15f;
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

            var walkToIdle = walkState.AddTransition(idleState);
            walkToIdle.hasExitTime = false;
            walkToIdle.duration = 0.15f;
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

            sm.defaultState = idleState;

            AssetDatabase.SaveAssets();
            Debug.Log($"[SettlerSetup] Controller created — Idle: {idleClip.GetType().Name}, Walk: {walkClip.GetType().Name}");
        }

        private static AnimationClip BakeFreshClip(AnimationClip source, string clipName, bool loop = false)
        {
            var dst = new AnimationClip { name = clipName, legacy = false };

            foreach (var binding in AnimationUtility.GetCurveBindings(source))
            {
                var curve = AnimationUtility.GetEditorCurve(source, binding);
                if (curve != null)
                    AnimationUtility.SetEditorCurve(dst, binding, curve);
            }

            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(source))
            {
                var keys = AnimationUtility.GetObjectReferenceCurve(source, binding);
                if (keys != null)
                    AnimationUtility.SetObjectReferenceCurve(dst, binding, keys);
            }

            var settings = AnimationUtility.GetAnimationClipSettings(source);
            if (loop)
            {
                settings.loopTime = true;
            }
            AnimationUtility.SetAnimationClipSettings(dst, settings);

            if (loop)
            {
                var settings2 = AnimationUtility.GetAnimationClipSettings(dst);
                Debug.Log($"[SettlerSetup] {clipName} loopTime={settings2.loopTime}");
            }

            return dst;
        }

        private static string CleanName(string raw) =>
            raw.Replace("CharacterArmature|", "").Replace("|", "_");
    }
#endif
}
