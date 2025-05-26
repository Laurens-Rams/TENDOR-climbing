using UnityEngine;
using UnityEditor;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;
using TENDOR.Core;
using TENDOR.Services;
using TENDOR.Services.AR;
using TENDOR.Services.Firebase;
using TENDOR.Testing;

namespace TENDOR.Editor
{
    /// <summary>
    /// Automatically sets up the MVP scene with all necessary components
    /// </summary>
    public class MVPSceneSetup : EditorWindow
    {
        [MenuItem("TENDOR/Setup MVP Scene")]
        public static void ShowWindow()
        {
            GetWindow<MVPSceneSetup>("MVP Scene Setup");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("🚀 TENDOR MVP Scene Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            GUILayout.Label("This will set up your scene with all necessary components for the MVP:");
            GUILayout.Label("• AR Foundation components");
            GUILayout.Label("• Firebase services");
            GUILayout.Label("• Game state management");
            GUILayout.Label("• AR Remote testing");
            GUILayout.Label("• Logger system");
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("🔧 Setup MVP Scene", GUILayout.Height(40)))
            {
                SetupMVPScene();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("🧪 Add Test Components Only", GUILayout.Height(30)))
            {
                AddTestComponents();
            }
        }
        
        private static void SetupMVPScene()
        {
            Debug.Log("🚀 Setting up MVP scene...");
            
            // 1. Create XR Origin if it doesn't exist
            SetupXROrigin();
            
            // 2. Create Services Manager
            SetupServicesManager();
            
            // 3. Create Game State Manager
            SetupGameStateManager();
            
            // 4. Add AR Remote Testing
            SetupARRemoteTesting();
            
            // 5. Create UI Canvas
            SetupUICanvas();
            
            Debug.Log("✅ MVP scene setup complete!");
            EditorUtility.DisplayDialog("Setup Complete", "MVP scene has been set up successfully!\n\nNext steps:\n1. Add your GoogleService-Info.plist to Assets/\n2. Test with AR Foundation Remote\n3. Build to iOS device", "OK");
        }
        
        private static void SetupXROrigin()
        {
            var existingXROrigin = FindObjectOfType<XROrigin>();
            if (existingXROrigin != null)
            {
                Debug.Log("📱 XR Origin already exists");
                return;
            }
            
            // Create XR Origin
            var xrOriginGO = new GameObject("XR Origin");
            var xrOrigin = xrOriginGO.AddComponent<XROrigin>();
            
            // Create Camera Offset
            var cameraOffsetGO = new GameObject("Camera Offset");
            cameraOffsetGO.transform.SetParent(xrOriginGO.transform);
            xrOrigin.CameraFloorOffsetObject = cameraOffsetGO;
            
            // Create Main Camera
            var cameraGO = new GameObject("Main Camera");
            cameraGO.transform.SetParent(cameraOffsetGO.transform);
            cameraGO.tag = "MainCamera";
            
            var camera = cameraGO.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 20f;
            
            xrOrigin.Camera = camera;
            
            // Add AR components
            var arSession = xrOriginGO.AddComponent<ARSession>();
            var arCameraManager = cameraGO.AddComponent<ARCameraManager>();
            var arTrackedImageManager = xrOriginGO.AddComponent<ARTrackedImageManager>();
            var arHumanBodyManager = xrOriginGO.AddComponent<ARHumanBodyManager>();
            
            Debug.Log("📱 Created XR Origin with AR components");
        }
        
        private static void SetupServicesManager()
        {
            var existingServices = FindObjectOfType<FirebaseConfig>();
            if (existingServices != null)
            {
                Debug.Log("🔥 Firebase services already exist");
                return;
            }
            
            var servicesGO = new GameObject("Services Manager");
            servicesGO.AddComponent<FirebaseConfig>();
            
            var arService = servicesGO.AddComponent<ARService>();
            var firebaseService = servicesGO.AddComponent<FirebaseService>();
            
            Debug.Log("🔥 Created Services Manager");
        }
        
        private static void SetupGameStateManager()
        {
            var existingStateManager = FindObjectOfType<GameStateManager>();
            if (existingStateManager != null)
            {
                Debug.Log("🎯 Game State Manager already exists");
                return;
            }
            
            var stateManagerGO = new GameObject("Game State Manager");
            stateManagerGO.AddComponent<GameStateManager>();
            
            Debug.Log("🎯 Created Game State Manager");
        }
        
        private static void SetupARRemoteTesting()
        {
            var existingTestManager = FindObjectOfType<ARRemoteTestManager>();
            if (existingTestManager != null)
            {
                Debug.Log("🎮 AR Remote Test Manager already exists");
                return;
            }
            
            var testManagerGO = new GameObject("AR Remote Test Manager");
            testManagerGO.AddComponent<ARRemoteTestManager>();
            
            Debug.Log("🎮 Created AR Remote Test Manager");
        }
        
        private static void SetupUICanvas()
        {
            var existingCanvas = FindObjectOfType<Canvas>();
            if (existingCanvas != null)
            {
                Debug.Log("🖼️ UI Canvas already exists");
                return;
            }
            
            var canvasGO = new GameObject("UI Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Create EventSystem if it doesn't exist
            var existingEventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (existingEventSystem == null)
            {
                var eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            
            Debug.Log("🖼️ Created UI Canvas");
        }
        
        private static void AddTestComponents()
        {
            SetupARRemoteTesting();
            Debug.Log("🧪 Added test components only");
        }
    }
} 