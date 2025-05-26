using UnityEngine;
using UnityEditor;
using BodyTracking;
using BodyTracking.Animation;
using BodyTracking.AR;
using BodyTracking.Recording;
using BodyTracking.Playback;

namespace BodyTracking.Editor
{
    /// <summary>
    /// Comprehensive system validation and testing tool
    /// </summary>
    public class SystemValidator : EditorWindow
    {
        [MenuItem("TENDOR/System Validator")]
        public static void ShowWindow()
        {
            GetWindow<SystemValidator>("System Validator");
        }

        void OnGUI()
        {
            GUILayout.Label("TENDOR System Validation", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Validate All Systems"))
            {
                ValidateAllSystems();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Test Animation System"))
            {
                TestAnimationSystem();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Fix Scene Connections"))
            {
                FixSceneConnections();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Create Missing Character"))
            {
                CreateMissingCharacter();
            }

            GUILayout.Space(10);
            GUILayout.Label("Console Commands:", EditorStyles.boldLabel);

            if (GUILayout.Button("List Available Animations"))
            {
                FBXCharacterController.ListAvailableAnimations();
            }

            if (GUILayout.Button("Force Reload Animation"))
            {
                FBXCharacterController.ForceReloadAnimationFromNewAnimationFBX();
            }

            if (GUILayout.Button("Make Character Visible"))
            {
                FBXCharacterController.MakeCharacterVisible();
            }
        }

        private void ValidateAllSystems()
        {
            Debug.Log("=== TENDOR SYSTEM VALIDATION ===");

            // Check BodyTrackingController
            var controller = FindObjectOfType<BodyTrackingController>();
            if (controller != null)
            {
                Debug.Log("✅ BodyTrackingController found");
                
                // Check references
                var humanBodyManager = controller.GetComponent<UnityEngine.XR.ARFoundation.ARHumanBodyManager>();
                if (humanBodyManager != null)
                    Debug.Log("✅ ARHumanBodyManager reference OK");
                else
                    Debug.LogError("❌ ARHumanBodyManager reference missing");

                var imageTargetManager = controller.GetComponent<ARImageTargetManager>();
                if (imageTargetManager != null)
                    Debug.Log("✅ ARImageTargetManager reference OK");
                else
                    Debug.LogError("❌ ARImageTargetManager reference missing");

                var recorder = controller.GetComponent<BodyTrackingRecorder>();
                if (recorder != null)
                    Debug.Log("✅ BodyTrackingRecorder reference OK");
                else
                    Debug.LogError("❌ BodyTrackingRecorder reference missing");

                var player = controller.GetComponent<BodyTrackingPlayer>();
                if (player != null)
                    Debug.Log("✅ BodyTrackingPlayer reference OK");
                else
                    Debug.LogError("❌ BodyTrackingPlayer reference missing");
            }
            else
            {
                Debug.LogError("❌ BodyTrackingController not found in scene");
            }

            // Check FBXCharacterController
            var characterController = FindObjectOfType<FBXCharacterController>();
            if (characterController != null)
            {
                Debug.Log("✅ FBXCharacterController found");
                
                if (characterController.CharacterRootForEditor != null)
                    Debug.Log("✅ Character root assigned");
                else
                    Debug.LogWarning("⚠️ Character root not assigned - will auto-find");

                if (characterController.AnimatorOverrideControllerForEditor != null)
                    Debug.Log("✅ Animator override controller assigned");
                else
                    Debug.LogError("❌ Animator override controller missing");
            }
            else
            {
                Debug.LogError("❌ FBXCharacterController not found in scene");
            }

            // Check Animation Controllers
            CheckAnimationControllers();

            Debug.Log("=== VALIDATION COMPLETE ===");
        }

        private void CheckAnimationControllers()
        {
            Debug.Log("--- Animation Controllers Check ---");

            // Check base controller
            var baseController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/Scripts/Animation/Controllers/CharacterAnimatorController.controller");
            if (baseController != null)
            {
                Debug.Log("✅ Base animator controller found");
            }
            else
            {
                Debug.LogError("❌ Base animator controller missing");
            }

            // Check override controller
            var overrideController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(
                "Assets/Scripts/Animation/Controllers/CharacterAnimationOverride.overrideController");
            if (overrideController != null)
            {
                Debug.Log("✅ Override animator controller found");
                
                var overrides = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>>();
                overrideController.GetOverrides(overrides);
                Debug.Log($"Override slots: {overrides.Count}");
                
                foreach (var pair in overrides)
                {
                    Debug.Log($"  - {pair.Key?.name} -> {pair.Value?.name ?? "Empty"}");
                }
            }
            else
            {
                Debug.LogError("❌ Override animator controller missing");
            }
        }

        private void TestAnimationSystem()
        {
            Debug.Log("=== TESTING ANIMATION SYSTEM ===");

            var characterController = FindObjectOfType<FBXCharacterController>();
            if (characterController != null)
            {
                // Initialize if needed
                if (!characterController.IsInitialized)
                {
                    bool initialized = characterController.Initialize();
                    Debug.Log($"Character controller initialization: {(initialized ? "SUCCESS" : "FAILED")}");
                }

                // Test animation playback
                bool animationStarted = characterController.StartAnimationPlayback();
                Debug.Log($"Animation playback test: {(animationStarted ? "SUCCESS" : "FAILED")}");

                // Check animator status
                if (characterController.CharacterAnimatorForEditor != null)
                {
                    var animator = characterController.CharacterAnimatorForEditor;
                    Debug.Log($"Animator enabled: {animator.enabled}");
                    Debug.Log($"Animator speed: {animator.speed}");
                    Debug.Log($"Controller: {(animator.runtimeAnimatorController ? animator.runtimeAnimatorController.name : "None")}");
                    
                    if (animator.layerCount > 0)
                    {
                        var state = animator.GetCurrentAnimatorStateInfo(0);
                        Debug.Log($"Current state length: {state.length:F2}s");
                        Debug.Log($"Animation time: {state.normalizedTime:F3}");
                    }
                }
            }
            else
            {
                Debug.LogError("❌ FBXCharacterController not found for testing");
            }

            Debug.Log("=== ANIMATION TEST COMPLETE ===");
        }

        private void FixSceneConnections()
        {
            Debug.Log("=== FIXING SCENE CONNECTIONS ===");

            var controller = FindObjectOfType<BodyTrackingController>();
            var characterController = FindObjectOfType<FBXCharacterController>();

            if (controller != null && characterController != null)
            {
                // Use reflection to fix connections if needed
                var controllerType = typeof(BodyTrackingController);
                
                // Fix imageTargetManager reference
                var imageTargetManager = controller.GetComponent<ARImageTargetManager>();
                if (imageTargetManager != null)
                {
                    var field = controllerType.GetField("imageTargetManager", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(controller, imageTargetManager);
                        Debug.Log("✅ Fixed imageTargetManager reference");
                    }
                }

                // Fix recorder reference
                var recorder = controller.GetComponent<BodyTrackingRecorder>();
                if (recorder != null)
                {
                    var field = controllerType.GetField("recorder", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(controller, recorder);
                        Debug.Log("✅ Fixed recorder reference");
                    }
                }

                // Fix player reference
                var player = controller.GetComponent<BodyTrackingPlayer>();
                if (player != null)
                {
                    var field = controllerType.GetField("player", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(controller, player);
                        Debug.Log("✅ Fixed player reference");
                    }
                }

                EditorUtility.SetDirty(controller);
                Debug.Log("✅ Scene connections fixed and marked dirty");
            }
            else
            {
                Debug.LogError("❌ Required components not found for fixing connections");
            }

            Debug.Log("=== CONNECTION FIX COMPLETE ===");
        }

        private void CreateMissingCharacter()
        {
            Debug.Log("=== CREATING MISSING CHARACTER ===");

            var characterController = FindObjectOfType<FBXCharacterController>();
            if (characterController != null)
            {
                if (characterController.CharacterRootForEditor == null)
                {
                    // Try to find NewBody in scene first
                    var existingCharacter = GameObject.Find("NewBody");
                    if (existingCharacter == null)
                    {
                        // Try to instantiate from prefab
                        string[] guids = AssetDatabase.FindAssets("t:GameObject NewBody");
                        foreach (string guid in guids)
                        {
                            string path = AssetDatabase.GUIDToAssetPath(guid);
                            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                            if (prefab != null && (prefab.name == "NewBody" || prefab.name == "Newbody"))
                            {
                                GameObject newCharacter = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                                newCharacter.name = "NewBody";
                                newCharacter.transform.position = Vector3.zero;
                                
                                // Assign to character controller
                                var field = typeof(FBXCharacterController).GetField("characterRoot", 
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                if (field != null)
                                {
                                    field.SetValue(characterController, newCharacter);
                                    EditorUtility.SetDirty(characterController);
                                }

                                Debug.Log($"✅ Created and assigned NewBody character from prefab: {path}");
                                Selection.activeGameObject = newCharacter;
                                return;
                            }
                        }
                        Debug.LogError("❌ Could not find NewBody prefab in project");
                    }
                    else
                    {
                        Debug.Log("✅ Found existing NewBody character in scene");
                        Selection.activeGameObject = existingCharacter;
                    }
                }
                else
                {
                    Debug.Log("✅ Character already assigned to controller");
                }
            }
            else
            {
                Debug.LogError("❌ FBXCharacterController not found");
            }

            Debug.Log("=== CHARACTER CREATION COMPLETE ===");
        }

        [MenuItem("TENDOR/Run Validation Tests")]
        public static void ValidateAllSystemsFromCommandLine()
        {
            Debug.Log("=== COMMAND LINE SYSTEM VALIDATION ===");
            
            // Check BodyTrackingController
            var controller = FindFirstObjectByType<BodyTrackingController>();
            if (controller != null)
            {
                Debug.Log("✅ BodyTrackingController found");
                Debug.Log($"Initialized: {controller.IsInitialized}");
                Debug.Log($"Current Mode: {controller.CurrentMode}");
            }
            else
            {
                Debug.LogError("❌ BodyTrackingController not found in scene");
            }

            // Check FBXCharacterController
            var characterController = FindFirstObjectByType<FBXCharacterController>();
            if (characterController != null)
            {
                Debug.Log("✅ FBXCharacterController found");
                Debug.Log($"Initialized: {characterController.IsInitialized}");
                
                if (characterController.CharacterRootForEditor != null)
                {
                    Debug.Log($"✅ Character Root: {characterController.CharacterRootForEditor.name}");
                }
                else
                {
                    Debug.LogWarning("⚠️ Character Root not assigned - will auto-find");
                }

                if (characterController.AnimatorOverrideControllerForEditor != null)
                {
                    Debug.Log($"✅ Override Controller: {characterController.AnimatorOverrideControllerForEditor.name}");
                }
                else
                {
                    Debug.LogError("❌ Override Controller missing");
                }
            }
            else
            {
                Debug.LogError("❌ FBXCharacterController not found in scene");
            }

            // Check Animation Controllers
            var overrideController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(
                "Assets/Scripts/Animation/Controllers/CharacterAnimationOverride.overrideController");
            if (overrideController != null)
            {
                Debug.Log("✅ Override animator controller found");
                
                var overrides = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>>();
                overrideController.GetOverrides(overrides);
                Debug.Log($"Override slots: {overrides.Count}");
                
                foreach (var pair in overrides)
                {
                    Debug.Log($"  - {pair.Key?.name} -> {pair.Value?.name ?? "Empty"}");
                }
            }

            // Check scene setup
            var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
            {
                Debug.Log("✅ XR Origin found");
            }

            // Check NewBody character in scene
            var newBodyInScene = GameObject.Find("NewBody");
            if (newBodyInScene != null)
            {
                Debug.Log($"✅ NewBody found in scene at {newBodyInScene.transform.position}");
                Debug.Log($"NewBody active: {newBodyInScene.activeInHierarchy}");
            }
            else
            {
                Debug.LogWarning("⚠️ NewBody not found in scene - needs to be created");
            }

            Debug.Log("=== COMMAND LINE VALIDATION COMPLETE ===");
        }
    }
} 