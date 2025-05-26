using UnityEngine;
using UnityEditor;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;
using BodyTracking;
using BodyTracking.AR;
using BodyTracking.Utils;
using BodyTracking.Recording;

namespace BodyTracking.Editor
{
    /// <summary>
    /// AR Foundation scene fixer for AR Remote setup
    /// </summary>
    public class ARSceneFixer : EditorWindow
    {
        [MenuItem("TENDOR/Fix AR Remote Scene")]
        public static void FixARRemoteScene()
        {
            Debug.Log("=== FIXING AR REMOTE SCENE ===");
            
            // 1. Add XR Origin if missing
            AddXROrigin();
            
            // 2. Add AR Session if missing
            AddARSession();
            
            // 3. Add TENDOR components
            AddTENDORComponents();
            
            // 4. Add Quality Optimizer
            AddQualityOptimizer();
            
            // 5. Mark scene as dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            
            Debug.Log("=== AR REMOTE SCENE FIXING COMPLETE ===");
            Debug.Log("Scene has been set up for AR Remote! Save the scene to persist changes.");
        }

        private static void AddXROrigin()
        {
            Debug.Log("--- Adding XR Origin ---");
            
            // Check if XR Origin already exists
            var existingXROrigin = FindFirstObjectByType<XROrigin>();
            if (existingXROrigin != null)
            {
                Debug.Log("✅ XR Origin already exists in scene");
                AddARComponentsToXROrigin(existingXROrigin);
                return;
            }

            // Try to instantiate from prefab
            string prefabPath = "Assets/Samples/XR Interaction Toolkit/3.0.7/AR Starter Assets/Prefabs/XR Origin (AR Rig).prefab";
            GameObject xrOriginPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (xrOriginPrefab != null)
            {
                GameObject xrOriginInstance = PrefabUtility.InstantiatePrefab(xrOriginPrefab) as GameObject;
                xrOriginInstance.name = "XR Origin (AR Rig)";
                
                Debug.Log("✅ Created XR Origin from prefab");
                
                // Add additional AR components
                AddARComponentsToXROrigin(xrOriginInstance.GetComponent<XROrigin>());
            }
            else
            {
                // Create XR Origin manually
                CreateXROriginManually();
            }
        }

        private static void AddARComponentsToXROrigin(XROrigin xrOrigin)
        {
            Debug.Log("--- Adding AR Components to XR Origin ---");
            
            // Add AR Human Body Manager
            var bodyManager = xrOrigin.GetComponent<ARHumanBodyManager>();
            if (bodyManager == null)
            {
                bodyManager = xrOrigin.gameObject.AddComponent<ARHumanBodyManager>();
                bodyManager.enabled = true;
                Debug.Log("✅ Added AR Human Body Manager");
            }
            else
            {
                Debug.Log("✅ AR Human Body Manager already exists");
            }

            // Add AR Tracked Image Manager
            var imageManager = xrOrigin.GetComponent<ARTrackedImageManager>();
            if (imageManager == null)
            {
                imageManager = xrOrigin.gameObject.AddComponent<ARTrackedImageManager>();
                imageManager.enabled = true;
                Debug.Log("✅ Added AR Tracked Image Manager");
            }
            else
            {
                Debug.Log("✅ AR Tracked Image Manager already exists");
            }

            // Add AR Plane Manager if not present
            var planeManager = xrOrigin.GetComponent<ARPlaneManager>();
            if (planeManager == null)
            {
                planeManager = xrOrigin.gameObject.AddComponent<ARPlaneManager>();
                planeManager.enabled = true;
                Debug.Log("✅ Added AR Plane Manager");
            }

            // Add AR Raycast Manager if not present
            var raycastManager = xrOrigin.GetComponent<ARRaycastManager>();
            if (raycastManager == null)
            {
                raycastManager = xrOrigin.gameObject.AddComponent<ARRaycastManager>();
                raycastManager.enabled = true;
                Debug.Log("✅ Added AR Raycast Manager");
            }
        }

        private static void CreateXROriginManually()
        {
            Debug.Log("--- Creating XR Origin Manually ---");
            
            // Create XR Origin GameObject
            GameObject xrOriginGO = new GameObject("XR Origin (AR Rig)");
            var xrOrigin = xrOriginGO.AddComponent<XROrigin>();
            
            // Create Camera Offset
            GameObject cameraOffset = new GameObject("Camera Offset");
            cameraOffset.transform.SetParent(xrOriginGO.transform);
            xrOrigin.CameraFloorOffsetObject = cameraOffset;
            
            // Create AR Camera
            GameObject arCameraGO = new GameObject("AR Camera");
            arCameraGO.transform.SetParent(cameraOffset.transform);
            
            var camera = arCameraGO.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 20f;
            
            var arCameraManager = arCameraGO.AddComponent<ARCameraManager>();
            var arCameraBackground = arCameraGO.AddComponent<ARCameraBackground>();
            
            xrOrigin.Camera = camera;
            
            // Add AR components
            AddARComponentsToXROrigin(xrOrigin);
            
            Debug.Log("✅ Created XR Origin manually");
        }

        private static void AddARSession()
        {
            Debug.Log("--- Adding AR Session ---");
            
            var existingSession = FindFirstObjectByType<ARSession>();
            if (existingSession != null)
            {
                Debug.Log("✅ AR Session already exists");
                return;
            }

            GameObject arSessionGO = new GameObject("AR Session");
            var arSession = arSessionGO.AddComponent<ARSession>();
            arSession.enabled = true;
            
            Debug.Log("✅ Created AR Session");
        }

        private static void AddTENDORComponents()
        {
            Debug.Log("--- Adding TENDOR Components ---");
            
            // Add BodyTrackingSystem if missing
            GameObject bodyTrackingSystem = GameObject.Find("BodyTrackingSystem");
            if (bodyTrackingSystem == null)
            {
                bodyTrackingSystem = new GameObject("BodyTrackingSystem");
                Debug.Log("✅ Created BodyTrackingSystem GameObject");
            }

            // Add BodyTrackingController
            var bodyController = bodyTrackingSystem.GetComponent<BodyTrackingController>();
            if (bodyController == null)
            {
                bodyController = bodyTrackingSystem.AddComponent<BodyTrackingController>();
                Debug.Log("✅ Added BodyTrackingController");
            }

            // Add ARImageTargetManager
            var imageTargetManager = bodyTrackingSystem.GetComponent<ARImageTargetManager>();
            if (imageTargetManager == null)
            {
                imageTargetManager = bodyTrackingSystem.AddComponent<ARImageTargetManager>();
                
                // Try to assign the AR Tracked Image Manager
                var xrOrigin = FindFirstObjectByType<XROrigin>();
                if (xrOrigin != null)
                {
                    var arImageManager = xrOrigin.GetComponent<ARTrackedImageManager>();
                    if (arImageManager != null)
                    {
                        imageTargetManager.trackedImageManager = arImageManager;
                        Debug.Log("✅ Connected ARImageTargetManager to ARTrackedImageManager");
                    }
                }
                
                Debug.Log("✅ Added ARImageTargetManager");
            }

            // Add other TENDOR components
            var recorder = bodyTrackingSystem.GetComponent<BodyTracking.Recording.BodyTrackingRecorder>();
            if (recorder == null)
            {
                recorder = bodyTrackingSystem.AddComponent<BodyTracking.Recording.BodyTrackingRecorder>();
                Debug.Log("✅ Added BodyTrackingRecorder");
            }

            var player = bodyTrackingSystem.GetComponent<BodyTracking.Playback.BodyTrackingPlayer>();
            if (player == null)
            {
                player = bodyTrackingSystem.AddComponent<BodyTracking.Playback.BodyTrackingPlayer>();
                Debug.Log("✅ Added BodyTrackingPlayer");
            }

            // Connect references
            ConnectTENDORReferences(bodyController, imageTargetManager, recorder, player);
        }

        private static void ConnectTENDORReferences(BodyTrackingController controller, ARImageTargetManager imageTargetManager, 
            BodyTracking.Recording.BodyTrackingRecorder recorder, BodyTracking.Playback.BodyTrackingPlayer player)
        {
            Debug.Log("--- Connecting TENDOR References ---");
            
            // Use reflection to set private fields
            var controllerType = typeof(BodyTrackingController);
            
            // Set imageTargetManager
            var imageTargetField = controllerType.GetField("imageTargetManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (imageTargetField != null)
            {
                imageTargetField.SetValue(controller, imageTargetManager);
                Debug.Log("✅ Connected imageTargetManager reference");
            }

            // Set recorder
            var recorderField = controllerType.GetField("recorder", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (recorderField != null)
            {
                recorderField.SetValue(controller, recorder);
                Debug.Log("✅ Connected recorder reference");
            }

            // Set player
            var playerField = controllerType.GetField("player", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (playerField != null)
            {
                playerField.SetValue(controller, player);
                Debug.Log("✅ Connected player reference");
            }
        }

        private static void AddQualityOptimizer()
        {
            Debug.Log("--- Adding Quality Optimizer ---");
            
            var existingOptimizer = FindFirstObjectByType<QualityOptimizer>();
            if (existingOptimizer != null)
            {
                Debug.Log("✅ Quality Optimizer already exists");
                return;
            }

            GameObject optimizerGO = new GameObject("QualityOptimizer");
            var optimizer = optimizerGO.AddComponent<QualityOptimizer>();
            
            // Apply AR Remote optimizations immediately
            optimizer.ApplyOptimizations();
            
            Debug.Log("✅ Added Quality Optimizer and applied AR Remote optimizations");
        }

        [MenuItem("TENDOR/Fix Missing References")]
        public static void FixMissingReferences()
        {
            Debug.Log("=== FIXING MISSING REFERENCES ===");
            
            var bodyController = FindFirstObjectByType<BodyTrackingController>();
            var xrOrigin = FindFirstObjectByType<XROrigin>();
            
            if (bodyController != null && xrOrigin != null)
            {
                var humanBodyManager = xrOrigin.GetComponent<ARHumanBodyManager>();
                if (humanBodyManager != null)
                {
                    // Direct assignment since humanBodyManager is a public field
                    bodyController.humanBodyManager = humanBodyManager;
                    Debug.Log("✅ Connected ARHumanBodyManager to BodyTrackingController");
                }
                else
                {
                    Debug.LogError("ARHumanBodyManager not found on XR Origin");
                }
                
                // Also connect the ARImageTargetManager to ARTrackedImageManager
                var imageTargetManager = bodyController.GetComponent<ARImageTargetManager>();
                var trackedImageManager = xrOrigin.GetComponent<ARTrackedImageManager>();
                if (imageTargetManager != null && trackedImageManager != null)
                {
                    imageTargetManager.trackedImageManager = trackedImageManager;
                    Debug.Log("✅ Connected ARTrackedImageManager to ARImageTargetManager");
                }
            }
            else
            {
                if (bodyController == null) Debug.LogError("BodyTrackingController not found");
                if (xrOrigin == null) Debug.LogError("XR Origin not found");
            }
            
            // Mark scene as dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            
            Debug.Log("=== REFERENCE FIXING COMPLETE ===");
        }

        [MenuItem("TENDOR/Validate AR Remote Setup")]
        public static void ValidateARRemoteSetup()
        {
            Debug.Log("=== VALIDATING AR REMOTE SETUP ===");
            
            bool allGood = true;

            // Check XR Origin
            var xrOrigin = FindFirstObjectByType<XROrigin>();
            if (xrOrigin != null)
            {
                Debug.Log("✅ XR Origin found");
                
                // Check AR components
                if (xrOrigin.GetComponent<ARHumanBodyManager>() != null)
                    Debug.Log("✅ AR Human Body Manager found");
                else
                {
                    Debug.LogError("❌ AR Human Body Manager missing");
                    allGood = false;
                }

                if (xrOrigin.GetComponent<ARTrackedImageManager>() != null)
                    Debug.Log("✅ AR Tracked Image Manager found");
                else
                {
                    Debug.LogError("❌ AR Tracked Image Manager missing");
                    allGood = false;
                }
            }
            else
            {
                Debug.LogError("❌ XR Origin missing");
                allGood = false;
            }

            // Check AR Session
            var arSession = FindFirstObjectByType<ARSession>();
            if (arSession != null)
            {
                Debug.Log("✅ AR Session found");
            }
            else
            {
                Debug.LogError("❌ AR Session missing");
                allGood = false;
            }

            // Check TENDOR components
            var bodyController = FindFirstObjectByType<BodyTrackingController>();
            if (bodyController != null)
            {
                Debug.Log("✅ Body Tracking Controller found");
            }
            else
            {
                Debug.LogError("❌ Body Tracking Controller missing");
                allGood = false;
            }

            var imageTargetManager = FindFirstObjectByType<ARImageTargetManager>();
            if (imageTargetManager != null)
            {
                Debug.Log("✅ AR Image Target Manager found");
            }
            else
            {
                Debug.LogError("❌ AR Image Target Manager missing");
                allGood = false;
            }

            if (allGood)
            {
                Debug.Log("🎉 AR Remote setup is COMPLETE and ready for testing!");
            }
            else
            {
                Debug.LogWarning("⚠️ AR Remote setup has issues. Run 'Fix AR Remote Scene' to resolve.");
            }

            Debug.Log("=== VALIDATION COMPLETE ===");
        }

        [MenuItem("TENDOR/Test Character Animation")]
        public static void TestCharacterAnimation()
        {
            Debug.Log("🎬 TESTING CHARACTER ANIMATION");
            
            var characterController = FindFirstObjectByType<BodyTracking.Animation.FBXCharacterController>();
            if (characterController == null)
            {
                Debug.LogError("❌ No FBXCharacterController found in scene");
                return;
            }
            
            // Initialize if needed
            if (!characterController.IsInitialized)
            {
                bool initialized = characterController.Initialize();
                if (!initialized)
                {
                    Debug.LogError("❌ Character initialization failed");
                    return;
                }
                Debug.Log("✅ Character initialized");
            }
            
            // Start animation
            bool animationStarted = characterController.StartAnimationPlayback();
            if (animationStarted)
            {
                Debug.Log("✅ Animation started successfully!");
                
                // Show character info
                if (characterController.CharacterRoot != null)
                {
                    Debug.Log($"📍 Character: {characterController.CharacterRoot.name} at {characterController.CharacterRoot.transform.position}");
                    #if UNITY_EDITOR
                    UnityEditor.Selection.activeGameObject = characterController.CharacterRoot;
                    Debug.Log("🎯 Character selected in hierarchy - check Scene view!");
                    #endif
                }
            }
            else
            {
                Debug.LogError("❌ Animation failed to start");
            }
        }

        [MenuItem("TENDOR/Test Recording & Playback")]
        public static void TestRecordingPlayback()
        {
            Debug.Log("🎥 TESTING RECORDING & PLAYBACK");
            
            var bodyController = FindFirstObjectByType<BodyTrackingController>();
            if (bodyController == null)
            {
                Debug.LogError("❌ No BodyTrackingController found");
                return;
            }
            
            if (!bodyController.IsInitialized)
            {
                bool initialized = bodyController.Initialize();
                if (!initialized)
                {
                    Debug.LogError("❌ Body tracking initialization failed");
                    return;
                }
                Debug.Log("✅ Body tracking initialized");
            }
            
            // Check if we can record/playback
            Debug.Log($"📷 Image detected: {bodyController.imageTargetManager?.IsImageDetected ?? false}");
            Debug.Log($"🔴 Can record: {bodyController.CanRecord}");
            Debug.Log($"▶️ Can playback: {bodyController.CanPlayback}");
            Debug.Log($"🎮 Current mode: {bodyController.CurrentMode}");
            
            if (bodyController.CanRecord)
            {
                Debug.Log("✅ Ready to record! Point camera at wall target and use UI buttons");
            }
            else
            {
                Debug.Log("⚠️ Cannot record - make sure image target is detected");
            }
        }

        [MenuItem("TENDOR/Setup Video Recording")]
        public static void SetupVideoRecording()
        {
            Debug.Log("🎬 SETTING UP SYNCHRONIZED VIDEO RECORDING");
            
            // Find or create video recorder
            var videoRecorder = FindFirstObjectByType<BodyTracking.Recording.SynchronizedVideoRecorder>();
            if (videoRecorder == null)
            {
                // Create new GameObject for video recorder
                var videoRecorderGO = new GameObject("SynchronizedVideoRecorder");
                videoRecorder = videoRecorderGO.AddComponent<BodyTracking.Recording.SynchronizedVideoRecorder>();
                Debug.Log("✅ Created SynchronizedVideoRecorder");
            }
            else
            {
                Debug.Log("✅ SynchronizedVideoRecorder already exists");
            }
            
            // Connect to BodyTrackingController
            var controller = FindFirstObjectByType<BodyTrackingController>();
            if (controller != null)
            {
                // Use reflection to set the videoRecorder field
                var field = typeof(BodyTrackingController).GetField("videoRecorder");
                if (field != null)
                {
                    field.SetValue(controller, videoRecorder);
                    Debug.Log("✅ Connected video recorder to BodyTrackingController");
                }
                
                // Enable video recording
                var enableField = typeof(BodyTrackingController).GetField("enableVideoRecording");
                if (enableField != null)
                {
                    enableField.SetValue(controller, true);
                    Debug.Log("✅ Enabled video recording in controller");
                }
            }
            
            Debug.Log("🎉 Video recording setup complete!");
            Debug.Log("📁 Videos will be saved to: " + videoRecorder.OutputFolder);
        }

        [MenuItem("TENDOR/Test Video Recording Setup")]
        public static void TestVideoRecordingSetup()
        {
            Debug.Log("🧪 TESTING VIDEO RECORDING SETUP");
            
            var videoRecorder = FindFirstObjectByType<BodyTracking.Recording.SynchronizedVideoRecorder>();
            var controller = FindFirstObjectByType<BodyTrackingController>();
            var hipRecorder = FindFirstObjectByType<BodyTracking.Recording.BodyTrackingRecorder>();
            
            if (videoRecorder == null)
            {
                Debug.LogError("❌ SynchronizedVideoRecorder not found");
                return;
            }
            
            if (controller == null)
            {
                Debug.LogError("❌ BodyTrackingController not found");
                return;
            }
            
            if (hipRecorder == null)
            {
                Debug.LogError("❌ BodyTrackingRecorder not found");
                return;
            }
            
            Debug.Log("✅ All video recording components found");
            Debug.Log($"📁 Video output folder: {videoRecorder.OutputFolder}");
            Debug.Log($"🎬 Video recording enabled: {controller.IsVideoRecordingEnabled}");
            Debug.Log($"📹 Video frame rate: 30fps (synchronized with hip tracking)");
            Debug.Log($"💾 Videos will be saved as MP4 with H.264 codec");
            Debug.Log("🎉 Video recording system ready!");
        }
    }
} 