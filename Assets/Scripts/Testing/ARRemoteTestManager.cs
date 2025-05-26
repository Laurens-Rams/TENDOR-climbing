using UnityEngine;
using UnityEngine.XR.ARFoundation;
using TENDOR.Core;
using TENDOR.Services;
using TENDOR.Services.AR;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Logger = TENDOR.Core.Logger;

namespace TENDOR.Testing
{
    /// <summary>
    /// Manages AR Remote testing functionality
    /// </summary>
    public class ARRemoteTestManager : MonoBehaviour
    {
        [Header("AR Remote Testing")]
        [SerializeField] private bool enableRemoteTesting = true;
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private KeyCode toggleDebugKey = KeyCode.D;
        
        [Header("Test Controls")]
        [SerializeField] private KeyCode startRecordingKey = KeyCode.R;
        [SerializeField] private KeyCode stopRecordingKey = KeyCode.S;
        [SerializeField] private KeyCode switchStateKey = KeyCode.Space;
        
        private ARSession arSession;
        private ARCameraManager cameraManager;
        private ARTrackedImageManager trackedImageManager;
        private GameStateManager stateManager;
        
        private void Start()
        {
            if (!enableRemoteTesting) return;
            
            // Find AR components
            arSession = UnityEngine.Object.FindFirstObjectByType<ARSession>();
            cameraManager = UnityEngine.Object.FindFirstObjectByType<ARCameraManager>();
            trackedImageManager = UnityEngine.Object.FindFirstObjectByType<ARTrackedImageManager>();
            stateManager = GameStateManager.Instance;
            
            Logger.Log("🎮 AR Remote Test Manager initialized", "AR_REMOTE");
            Logger.Log($"📱 Controls: {toggleDebugKey}=Debug, {startRecordingKey}=Record, {stopRecordingKey}=Stop, {switchStateKey}=Switch State", "AR_REMOTE");
        }
        
        private void Update()
        {
            if (!enableRemoteTesting) return;
            
            // Toggle debug info
            if (Input.GetKeyDown(toggleDebugKey))
            {
                showDebugInfo = !showDebugInfo;
                Logger.Log($"🔍 Debug info: {(showDebugInfo ? "ON" : "OFF")}", "AR_REMOTE");
            }
            
            // Recording controls
            if (Input.GetKeyDown(startRecordingKey))
            {
                TestStartRecording();
            }
            
            if (Input.GetKeyDown(stopRecordingKey))
            {
                TestStopRecording();
            }
            
            // State switching
            if (Input.GetKeyDown(switchStateKey))
            {
                TestSwitchState();
            }
        }
        
        private void OnGUI()
        {
            if (!enableRemoteTesting || !showDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.BeginVertical("box");
            
#if UNITY_EDITOR
            GUILayout.Label("🎮 AR Remote Testing", EditorStyles.boldLabel);
#else
            GUILayout.Label("🎮 AR Remote Testing", GUI.skin.label);
#endif
            GUILayout.Space(10);
            
            // AR Status
            GUILayout.Label("📱 AR Status:");
            GUILayout.Label($"Session: {(arSession != null && arSession.enabled ? "✅" : "❌")}");
            GUILayout.Label($"Camera: {(cameraManager != null && cameraManager.enabled ? "✅" : "❌")}");
            GUILayout.Label($"Image Tracking: {(trackedImageManager != null && trackedImageManager.enabled ? "✅" : "❌")}");
            
            GUILayout.Space(10);
            
            // Game State
            if (stateManager != null)
            {
                GUILayout.Label($"🎯 State: {stateManager.GetCurrentState()}");
            }
            
            GUILayout.Space(10);
            
            // Controls
            GUILayout.Label("🎮 Controls:");
            if (GUILayout.Button($"[{toggleDebugKey}] Toggle Debug"))
            {
                showDebugInfo = !showDebugInfo;
            }
            
            if (GUILayout.Button($"[{startRecordingKey}] Start Recording"))
            {
                TestStartRecording();
            }
            
            if (GUILayout.Button($"[{stopRecordingKey}] Stop Recording"))
            {
                TestStopRecording();
            }
            
            if (GUILayout.Button($"[{switchStateKey}] Switch State"))
            {
                TestSwitchState();
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        private void TestStartRecording()
        {
            if (stateManager != null)
            {
                Logger.Log("🎬 Testing: Start Recording", "AR_REMOTE");
                stateManager.StartRecording();
            }
        }
        
        private void TestStopRecording()
        {
            if (stateManager != null)
            {
                Logger.Log("⏹️ Testing: Stop Recording", "AR_REMOTE");
                stateManager.StopRecording("/fake/test/video.mp4");
            }
        }
        
        private void TestSwitchState()
        {
            if (stateManager != null)
            {
                var currentState = stateManager.GetCurrentState();
                Logger.Log($"🔄 Testing: Current state is {currentState}", "AR_REMOTE");
                
                switch (currentState)
                {
                    case GameState.Idle:
                        stateManager.StartRecording();
                        break;
                    case GameState.Recording:
                        stateManager.StopRecording("/fake/test/video.mp4");
                        break;
                    case GameState.Processing:
                        Logger.Log("⏳ Processing state - waiting...", "AR_REMOTE");
                        break;
                    case GameState.Playback:
                        stateManager.ReturnToIdle();
                        break;
                }
            }
        }
    }
} 