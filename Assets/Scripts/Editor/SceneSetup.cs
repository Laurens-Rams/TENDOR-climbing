using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TENDOR.Services;
using TENDOR.Services.AR;
using TENDOR.Services.Firebase;
using TENDOR.Testing;

namespace TENDOR.Editor
{
    /// <summary>
    /// Scene setup utilities for TENDOR project
    /// </summary>
    public static class SceneSetup
    {
        [MenuItem("TENDOR/Setup Scene for Testing")]
        public static void SetupSceneForTesting()
        {
            // Create Services GameObject
            GameObject servicesGO = GameObject.Find("Services");
            if (servicesGO == null)
            {
                servicesGO = new GameObject("Services");
                servicesGO.transform.position = Vector3.zero;
            }

            // Add GameStateManager
            if (servicesGO.GetComponent<GameStateManager>() == null)
            {
                servicesGO.AddComponent<GameStateManager>();
                Debug.Log("✅ Added GameStateManager");
            }

            // Add FirebaseService
            if (servicesGO.GetComponent<FirebaseService>() == null)
            {
                servicesGO.AddComponent<FirebaseService>();
                Debug.Log("✅ Added FirebaseService");
            }

            // Add ARService
            if (servicesGO.GetComponent<ARService>() == null)
            {
                servicesGO.AddComponent<ARService>();
                Debug.Log("✅ Added ARService");
            }

            // Add CompilationTest
            if (servicesGO.GetComponent<CompilationTest>() == null)
            {
                var compilationTest = servicesGO.AddComponent<CompilationTest>();
                // Enable run on start for testing
                var serializedObject = new SerializedObject(compilationTest);
                var runOnStartProp = serializedObject.FindProperty("runTestOnStart");
                if (runOnStartProp != null)
                {
                    runOnStartProp.boolValue = true;
                    serializedObject.ApplyModifiedProperties();
                }
                Debug.Log("✅ Added CompilationTest (run on start enabled)");
            }

            // Add AR Remote Test Manager
            GameObject testManagerGO = GameObject.Find("AR Remote Test Manager");
            if (testManagerGO == null)
            {
                testManagerGO = new GameObject("AR Remote Test Manager");
                testManagerGO.AddComponent<ARRemoteTestManager>();
                Debug.Log("✅ Added AR Remote Test Manager");
            }

            // Fix AR Camera setup
            FixARCameraSetup();

            // Mark scene as dirty
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            
            Debug.Log("🎉 Scene setup complete! Ready for testing.");
            Debug.Log("📋 Services added: GameStateManager, FirebaseService, ARService, CompilationTest, ARRemoteTestManager");
            Debug.Log("🎮 Test Controls: D=Debug, R=Record, S=Stop, Space=Switch State");
        }

        [MenuItem("TENDOR/Fix AR Camera Setup")]
        public static void FixARCameraSetup()
        {
            // Find XR Origin
            var xrOrigin = Object.FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("❌ No XR Origin found in scene");
                return;
            }

            // Find the camera in XR Origin
            Camera camera = xrOrigin.Camera;
            if (camera == null)
            {
                // Look for camera in children
                camera = xrOrigin.GetComponentInChildren<Camera>();
            }

            if (camera == null)
            {
                Debug.LogError("❌ No Camera found in XR Origin");
                return;
            }

            // Add AR Camera Manager if missing
            var arCameraManager = camera.GetComponent<UnityEngine.XR.ARFoundation.ARCameraManager>();
            if (arCameraManager == null)
            {
                arCameraManager = camera.gameObject.AddComponent<UnityEngine.XR.ARFoundation.ARCameraManager>();
                Debug.Log("✅ Added AR Camera Manager to camera");
            }
            else
            {
                Debug.Log("📱 AR Camera Manager already exists");
            }

            // Add AR Tracked Image Manager to XR Origin if missing
            var trackedImageManager = xrOrigin.GetComponent<UnityEngine.XR.ARFoundation.ARTrackedImageManager>();
            if (trackedImageManager == null)
            {
                trackedImageManager = xrOrigin.gameObject.AddComponent<UnityEngine.XR.ARFoundation.ARTrackedImageManager>();
                Debug.Log("✅ Added AR Tracked Image Manager to XR Origin");
            }
            else
            {
                Debug.Log("🖼️ AR Tracked Image Manager already exists");
            }

            // Add AR Human Body Manager to XR Origin if missing
            var humanBodyManager = xrOrigin.GetComponent<UnityEngine.XR.ARFoundation.ARHumanBodyManager>();
            if (humanBodyManager == null)
            {
                humanBodyManager = xrOrigin.gameObject.AddComponent<UnityEngine.XR.ARFoundation.ARHumanBodyManager>();
                Debug.Log("✅ Added AR Human Body Manager to XR Origin");
            }
            else
            {
                Debug.Log("🚶 AR Human Body Manager already exists");
            }

            // Add AR Occlusion Manager to camera if missing
            var occlusionManager = camera.GetComponent<UnityEngine.XR.ARFoundation.AROcclusionManager>();
            if (occlusionManager == null)
            {
                occlusionManager = camera.gameObject.AddComponent<UnityEngine.XR.ARFoundation.AROcclusionManager>();
                Debug.Log("✅ Added AR Occlusion Manager to camera");
            }
            else
            {
                Debug.Log("👻 AR Occlusion Manager already exists");
            }

            Debug.Log("🔧 AR Camera setup complete!");
        }

        [MenuItem("TENDOR/Remove Test Services")]
        public static void RemoveTestServices()
        {
            GameObject servicesGO = GameObject.Find("Services");
            if (servicesGO != null)
            {
                Object.DestroyImmediate(servicesGO);
                Debug.Log("🗑️ Removed Services GameObject");
            }

            GameObject testManagerGO = GameObject.Find("AR Remote Test Manager");
            if (testManagerGO != null)
            {
                Object.DestroyImmediate(testManagerGO);
                Debug.Log("🗑️ Removed AR Remote Test Manager");
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("✅ Test services removed");
        }

        [MenuItem("TENDOR/Validate Scene Setup")]
        public static void ValidateSceneSetup()
        {
            Debug.Log("🔍 Validating scene setup...");

            // Check for AR components
            var arSession = Object.FindFirstObjectByType<UnityEngine.XR.ARFoundation.ARSession>();
            var arSessionOrigin = Object.FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            var arCamera = Object.FindFirstObjectByType<UnityEngine.XR.ARFoundation.ARCameraManager>();

            Debug.Log($"AR Session: {(arSession != null ? "✅ Found" : "❌ Missing")}");
            Debug.Log($"XR Origin: {(arSessionOrigin != null ? "✅ Found" : "❌ Missing")}");
            Debug.Log($"AR Camera: {(arCamera != null ? "✅ Found" : "❌ Missing")}");

            // Check for our services
            var gameStateManager = Object.FindFirstObjectByType<GameStateManager>();
            var firebaseService = Object.FindFirstObjectByType<FirebaseService>();
            var arService = Object.FindFirstObjectByType<ARService>();
            var compilationTest = Object.FindFirstObjectByType<CompilationTest>();
            var testManager = Object.FindFirstObjectByType<ARRemoteTestManager>();

            Debug.Log($"GameStateManager: {(gameStateManager != null ? "✅ Found" : "❌ Missing")}");
            Debug.Log($"FirebaseService: {(firebaseService != null ? "✅ Found" : "❌ Missing")}");
            Debug.Log($"ARService: {(arService != null ? "✅ Found" : "❌ Missing")}");
            Debug.Log($"CompilationTest: {(compilationTest != null ? "✅ Found" : "❌ Missing")}");
            Debug.Log($"ARRemoteTestManager: {(testManager != null ? "✅ Found" : "❌ Missing")}");

            bool isReady = arSession != null && arSessionOrigin != null && 
                          gameStateManager != null && firebaseService != null && arService != null;

            Debug.Log(isReady ? "🎉 Scene is ready for testing!" : "⚠️ Scene needs setup. Use 'TENDOR/Setup Scene for Testing'");
        }
    }
} 