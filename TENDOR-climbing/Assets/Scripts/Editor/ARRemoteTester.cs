using UnityEngine;
using UnityEditor;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;
using BodyTracking;
using BodyTracking.AR;

namespace BodyTracking.Editor
{
    /// <summary>
    /// AR Foundation Remote testing and validation tool
    /// </summary>
    public class ARRemoteTester : EditorWindow
    {
        [MenuItem("TENDOR/AR Remote Tester")]
        public static void ShowWindow()
        {
            GetWindow<ARRemoteTester>("AR Remote Tester");
        }

        private Vector2 scrollPosition;

        void OnGUI()
        {
            GUILayout.Label("TENDOR AR Remote Testing Tool", EditorStyles.boldLabel);
            GUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // AR Remote Status
            GUILayout.Label("AR Remote Status", EditorStyles.boldLabel);
            CheckARRemoteStatus();

            GUILayout.Space(10);

            // AR Foundation Components
            GUILayout.Label("AR Foundation Components", EditorStyles.boldLabel);
            ValidateARFoundationComponents();

            GUILayout.Space(10);

            // TENDOR Integration
            GUILayout.Label("TENDOR AR Integration", EditorStyles.boldLabel);
            ValidateTENDORIntegration();

            GUILayout.Space(10);

            // Testing Tools
            GUILayout.Label("Testing Tools", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Run Complete AR Remote Test"))
            {
                RunCompleteARRemoteTest();
            }

            if (GUILayout.Button("Test AR Session"))
            {
                TestARSession();
            }

            if (GUILayout.Button("Test Body Tracking"))
            {
                TestBodyTracking();
            }

            if (GUILayout.Button("Test Image Tracking"))
            {
                TestImageTracking();
            }

            if (GUILayout.Button("Optimize for AR Remote"))
            {
                OptimizeForARRemote();
            }

            EditorGUILayout.EndScrollView();
        }

        private void CheckARRemoteStatus()
        {
            // Check if AR Remote package is installed
            bool arRemoteInstalled = System.IO.Directory.Exists("Library/PackageCache/com.kyrylokuzyk.arfoundationremote@b261627c706b");
            
            if (arRemoteInstalled)
            {
                EditorGUILayout.HelpBox("✅ AR Foundation Remote is installed", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("❌ AR Foundation Remote not found", MessageType.Error);
            }

            // Check XR Management settings
            var xrSettings = UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.iOS);
            if (xrSettings != null && xrSettings.Manager != null)
            {
                EditorGUILayout.HelpBox("✅ XR Management configured", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("⚠️ XR Management needs configuration", MessageType.Warning);
            }
        }

        private void ValidateARFoundationComponents()
        {
            // Check for XR Origin
            var xrOrigin = FindFirstObjectByType<XROrigin>();
            if (xrOrigin != null)
            {
                EditorGUILayout.HelpBox($"✅ XR Origin found: {xrOrigin.name}", MessageType.Info);
                
                // Check AR Camera
                var arCamera = xrOrigin.GetComponentInChildren<ARCameraManager>();
                if (arCamera != null)
                {
                    EditorGUILayout.HelpBox("✅ AR Camera Manager found", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("❌ AR Camera Manager missing", MessageType.Error);
                }

                // Check AR Session
                var arSession = FindFirstObjectByType<ARSession>();
                if (arSession != null)
                {
                    EditorGUILayout.HelpBox("✅ AR Session found", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("❌ AR Session missing", MessageType.Error);
                }

                // Check Human Body Manager
                var bodyManager = xrOrigin.GetComponent<ARHumanBodyManager>();
                if (bodyManager != null)
                {
                    EditorGUILayout.HelpBox("✅ AR Human Body Manager found", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("⚠️ AR Human Body Manager missing", MessageType.Warning);
                }

                // Check Tracked Image Manager
                var imageManager = xrOrigin.GetComponent<ARTrackedImageManager>();
                if (imageManager != null)
                {
                    EditorGUILayout.HelpBox("✅ AR Tracked Image Manager found", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("⚠️ AR Tracked Image Manager missing", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("❌ XR Origin not found in scene", MessageType.Error);
            }
        }

        private void ValidateTENDORIntegration()
        {
            // Check Body Tracking Controller
            var bodyController = FindFirstObjectByType<BodyTrackingController>();
            if (bodyController != null)
            {
                EditorGUILayout.HelpBox("✅ Body Tracking Controller found", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("❌ Body Tracking Controller missing", MessageType.Error);
            }

            // Check AR Image Target Manager
            var imageTargetManager = FindFirstObjectByType<ARImageTargetManager>();
            if (imageTargetManager != null)
            {
                EditorGUILayout.HelpBox("✅ AR Image Target Manager found", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("❌ AR Image Target Manager missing", MessageType.Error);
            }

            // Check Quality Optimizer
            var qualityOptimizer = FindFirstObjectByType<BodyTracking.Utils.QualityOptimizer>();
            if (qualityOptimizer != null)
            {
                EditorGUILayout.HelpBox("✅ Quality Optimizer found", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("⚠️ Quality Optimizer missing (recommended for AR Remote)", MessageType.Warning);
            }
        }

        [MenuItem("TENDOR/Run AR Remote Test")]
        public static void RunCompleteARRemoteTest()
        {
            Debug.Log("=== AR REMOTE COMPLETE TEST ===");

            // Test 1: Check AR Remote Installation
            TestARRemoteInstallation();

            // Test 2: Check Scene Setup
            TestSceneSetup();

            // Test 3: Check TENDOR Integration
            TestTENDORIntegration();

            // Test 4: Check Build Settings
            TestBuildSettings();

            Debug.Log("=== AR REMOTE TEST COMPLETE ===");
        }

        private static void TestARRemoteInstallation()
        {
            Debug.Log("--- Testing AR Remote Installation ---");

            // Check package installation
            bool packageExists = System.IO.Directory.Exists("Library/PackageCache/com.kyrylokuzyk.arfoundationremote@b261627c706b");
            if (packageExists)
            {
                Debug.Log("✅ AR Foundation Remote package installed");
            }
            else
            {
                Debug.LogError("❌ AR Foundation Remote package not found");
            }

            // Check installer
            bool installerExists = System.IO.File.Exists("Assets/Plugins/ARFoundationRemoteInstaller/Installer.asset");
            if (installerExists)
            {
                Debug.Log("✅ AR Remote Installer found");
            }
            else
            {
                Debug.LogWarning("⚠️ AR Remote Installer not found");
            }

            // Check XR settings
            var xrSettings = UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.iOS);
            if (xrSettings != null)
            {
                Debug.Log("✅ XR Management settings configured");
            }
            else
            {
                Debug.LogWarning("⚠️ XR Management settings need configuration");
            }
        }

        private static void TestSceneSetup()
        {
            Debug.Log("--- Testing Scene Setup ---");

            // Check XR Origin
            var xrOrigin = FindFirstObjectByType<XROrigin>();
            if (xrOrigin != null)
            {
                Debug.Log($"✅ XR Origin found: {xrOrigin.name}");

                // Check required components
                var arCamera = xrOrigin.GetComponentInChildren<ARCameraManager>();
                if (arCamera != null)
                {
                    Debug.Log("✅ AR Camera Manager found");
                }
                else
                {
                    Debug.LogError("❌ AR Camera Manager missing");
                }

                var bodyManager = xrOrigin.GetComponent<ARHumanBodyManager>();
                if (bodyManager != null)
                {
                    Debug.Log("✅ AR Human Body Manager found");
                    Debug.Log($"Body tracking enabled: {bodyManager.enabled}");
                }
                else
                {
                    Debug.LogWarning("⚠️ AR Human Body Manager missing");
                }

                var imageManager = xrOrigin.GetComponent<ARTrackedImageManager>();
                if (imageManager != null)
                {
                    Debug.Log("✅ AR Tracked Image Manager found");
                    Debug.Log($"Image tracking enabled: {imageManager.enabled}");
                }
                else
                {
                    Debug.LogWarning("⚠️ AR Tracked Image Manager missing");
                }
            }
            else
            {
                Debug.LogError("❌ XR Origin not found in scene");
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
            }
        }

        private static void TestTENDORIntegration()
        {
            Debug.Log("--- Testing TENDOR Integration ---");

            // Check Body Tracking Controller
            var bodyController = FindFirstObjectByType<BodyTrackingController>();
            if (bodyController != null)
            {
                Debug.Log("✅ Body Tracking Controller found");
                Debug.Log($"Initialized: {bodyController.IsInitialized}");
                Debug.Log($"Current Mode: {bodyController.CurrentMode}");
            }
            else
            {
                Debug.LogError("❌ Body Tracking Controller missing");
            }

            // Check AR Image Target Manager
            var imageTargetManager = FindFirstObjectByType<ARImageTargetManager>();
            if (imageTargetManager != null)
            {
                Debug.Log("✅ AR Image Target Manager found");
                Debug.Log($"Target image name: {imageTargetManager.targetImageName}");
            }
            else
            {
                Debug.LogError("❌ AR Image Target Manager missing");
            }

            // Check Quality Optimizer
            var qualityOptimizer = FindFirstObjectByType<BodyTracking.Utils.QualityOptimizer>();
            if (qualityOptimizer != null)
            {
                Debug.Log("✅ Quality Optimizer found");
            }
            else
            {
                Debug.LogWarning("⚠️ Quality Optimizer missing (recommended for AR Remote)");
            }
        }

        private static void TestBuildSettings()
        {
            Debug.Log("--- Testing Build Settings ---");

            // Check iOS build target
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                Debug.Log("✅ iOS build target selected");
            }
            else
            {
                Debug.LogWarning($"⚠️ Current build target: {EditorUserBuildSettings.activeBuildTarget} (iOS recommended for AR Remote)");
            }

            // Check ARKit settings
            var arkitSettings = UnityEditor.XR.ARKit.ARKitSettings.GetOrCreateSettings();
            if (arkitSettings != null)
            {
                Debug.Log("✅ ARKit settings found");
                Debug.Log($"Face tracking: {arkitSettings.faceTracking}");
                // Note: humanBodyTracking property may not be available in all Unity versions
                try
                {
                    var humanBodyField = arkitSettings.GetType().GetField("humanBodyTracking");
                    if (humanBodyField != null)
                    {
                        Debug.Log($"Human body tracking: {humanBodyField.GetValue(arkitSettings)}");
                    }
                    else
                    {
                        Debug.Log("Human body tracking: Property not available in this Unity version");
                    }
                }
                catch (System.Exception)
                {
                    Debug.Log("Human body tracking: Unable to check property");
                }
            }
        }

        private void TestARSession()
        {
            Debug.Log("=== TESTING AR SESSION ===");

            var arSession = FindFirstObjectByType<ARSession>();
            if (arSession != null)
            {
                Debug.Log($"✅ AR Session found: {arSession.name}");
                Debug.Log($"Enabled: {arSession.enabled}");
                Debug.Log($"GameObject active: {arSession.gameObject.activeInHierarchy}");
            }
            else
            {
                Debug.LogError("❌ AR Session not found");
            }

            Debug.Log("=== AR SESSION TEST COMPLETE ===");
        }

        private void TestBodyTracking()
        {
            Debug.Log("=== TESTING BODY TRACKING ===");

            var xrOrigin = FindFirstObjectByType<XROrigin>();
            if (xrOrigin != null)
            {
                var bodyManager = xrOrigin.GetComponent<ARHumanBodyManager>();
                if (bodyManager != null)
                {
                    Debug.Log("✅ AR Human Body Manager found");
                    Debug.Log($"Enabled: {bodyManager.enabled}");
                    Debug.Log($"Human body prefab: {(bodyManager.humanBodyPrefab ? bodyManager.humanBodyPrefab.name : "None")}");
                    Debug.Log($"Pose estimation: {bodyManager.pose2DRequested}");
                    Debug.Log($"3D body tracking: {bodyManager.pose3DRequested}");
                }
                else
                {
                    Debug.LogError("❌ AR Human Body Manager not found");
                }
            }

            Debug.Log("=== BODY TRACKING TEST COMPLETE ===");
        }

        private void TestImageTracking()
        {
            Debug.Log("=== TESTING IMAGE TRACKING ===");

            var xrOrigin = FindFirstObjectByType<XROrigin>();
            if (xrOrigin != null)
            {
                var imageManager = xrOrigin.GetComponent<ARTrackedImageManager>();
                if (imageManager != null)
                {
                    Debug.Log("✅ AR Tracked Image Manager found");
                    Debug.Log($"Enabled: {imageManager.enabled}");
                    
                    // Check reference image library
                    if (imageManager.referenceLibrary != null)
                    {
                        Debug.Log($"Reference image library: {imageManager.referenceLibrary.GetType().Name}");
                    }
                    else
                    {
                        Debug.Log("Reference image library: None");
                    }
                    
                    // Use the newer property instead of deprecated one
                    Debug.Log($"Requested max moving images: {imageManager.requestedMaxNumberOfMovingImages}");
                }
                else
                {
                    Debug.LogError("❌ AR Tracked Image Manager not found");
                }
            }

            Debug.Log("=== IMAGE TRACKING TEST COMPLETE ===");
        }

        private void OptimizeForARRemote()
        {
            Debug.Log("=== OPTIMIZING FOR AR REMOTE ===");

            // Apply quality optimizations
            var qualityOptimizer = FindFirstObjectByType<BodyTracking.Utils.QualityOptimizer>();
            if (qualityOptimizer != null)
            {
                qualityOptimizer.ApplyOptimizations();
                Debug.Log("✅ Applied quality optimizations");
            }
            else
            {
                // Create quality optimizer if missing
                var optimizerGO = new GameObject("QualityOptimizer");
                optimizerGO.AddComponent<BodyTracking.Utils.QualityOptimizer>();
                Debug.Log("✅ Created Quality Optimizer");
            }

            // Set recommended quality settings for AR Remote
            QualitySettings.SetQualityLevel(2, false); // Medium quality
            Application.targetFrameRate = 30; // Lower frame rate for streaming
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.vSyncCount = 0;

            Debug.Log("✅ Applied AR Remote optimizations");
            Debug.Log("=== OPTIMIZATION COMPLETE ===");
        }
    }
} 