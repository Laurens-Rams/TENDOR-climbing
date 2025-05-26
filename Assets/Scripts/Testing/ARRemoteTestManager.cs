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
            
            // Larger area for better visibility
            GUILayout.BeginArea(new Rect(10, 10, 400, 500));
            GUILayout.BeginVertical("box");
            
            // Title with larger font
            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 18;
            titleStyle.fontStyle = FontStyle.Bold;
            
#if UNITY_EDITOR
            GUILayout.Label("🎮 AR Remote Testing", EditorStyles.boldLabel);
#else
            GUILayout.Label("🎮 AR Remote Testing", titleStyle);
#endif
            GUILayout.Space(15);
            
            // AR Status section
            var headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 14;
            headerStyle.fontStyle = FontStyle.Bold;
            
            GUILayout.Label("📱 AR Status:", headerStyle);
            GUILayout.Space(5);
            
            var statusStyle = new GUIStyle(GUI.skin.label);
            statusStyle.fontSize = 12;
            
            GUILayout.Label($"Session: {(arSession != null && arSession.enabled ? "✅" : "❌")}", statusStyle);
            GUILayout.Label($"Camera: {(cameraManager != null && cameraManager.enabled ? "✅" : "❌")}", statusStyle);
            GUILayout.Label($"Image Tracking: {(trackedImageManager != null && trackedImageManager.enabled ? "✅" : "❌")}", statusStyle);
            
            GUILayout.Space(15);
            
            // Game State section
            GUILayout.Label("🎯 Game State:", headerStyle);
            GUILayout.Space(5);
            
            if (stateManager != null)
            {
                var stateStyle = new GUIStyle(GUI.skin.label);
                stateStyle.fontSize = 14;
                stateStyle.fontStyle = FontStyle.Bold;
                
                // Color code the state
                switch (stateManager.GetCurrentState())
                {
                    case GameState.Idle:
                        stateStyle.normal.textColor = Color.white;
                        break;
                    case GameState.Recording:
                        stateStyle.normal.textColor = Color.red;
                        break;
                    case GameState.Processing:
                        stateStyle.normal.textColor = Color.yellow;
                        break;
                    case GameState.Playback:
                        stateStyle.normal.textColor = Color.green;
                        break;
                }
                
                GUILayout.Label($"State: {stateManager.GetCurrentState()}", stateStyle);
            }
            
            GUILayout.Space(20);
            
            // Controls section with bigger buttons
            GUILayout.Label("🎮 Controls:", headerStyle);
            GUILayout.Space(10);
            
            // Button style
            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 14;
            buttonStyle.fontStyle = FontStyle.Bold;
            
            // Debug toggle button
            var debugButtonColor = showDebugInfo ? Color.green : Color.gray;
            GUI.backgroundColor = debugButtonColor;
            if (GUILayout.Button($"[{toggleDebugKey}] Toggle Debug Info", buttonStyle, GUILayout.Height(40)))
            {
                showDebugInfo = !showDebugInfo;
            }
            GUI.backgroundColor = Color.white;
            
            GUILayout.Space(5);
            
            // Recording controls
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button($"[{startRecordingKey}] Start Recording", buttonStyle, GUILayout.Height(40)))
            {
                TestStartRecording();
            }
            GUI.backgroundColor = Color.white;
            
            GUILayout.Space(5);
            
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button($"[{stopRecordingKey}] Stop Recording", buttonStyle, GUILayout.Height(40)))
            {
                TestStopRecording();
            }
            GUI.backgroundColor = Color.white;
            
            GUILayout.Space(5);
            
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button($"[{switchStateKey}] Switch State", buttonStyle, GUILayout.Height(40)))
            {
                TestSwitchState();
            }
            GUI.backgroundColor = Color.white;
            
            GUILayout.Space(15);
            
            // Instructions
            var instructionStyle = new GUIStyle(GUI.skin.label);
            instructionStyle.fontSize = 10;
            instructionStyle.fontStyle = FontStyle.Italic;
            instructionStyle.wordWrap = true;
            
            GUILayout.Label("💡 Use keyboard shortcuts or click buttons above to test state transitions. Check Console for detailed logs.", instructionStyle);
            
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