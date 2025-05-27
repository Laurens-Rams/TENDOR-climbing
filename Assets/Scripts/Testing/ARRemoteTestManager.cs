using UnityEngine;
using UnityEngine.XR.ARFoundation;
using TENDOR.Core;
using TENDOR.Recording;
using TENDOR.Services.AR;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Logger = TENDOR.Core.Logger;

namespace TENDOR.Testing
{
    /// <summary>
    /// Manages AR Foundation Remote testing functionality for iPhone connection
    /// Updated to work with original BodyTrackingController system
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
        [SerializeField] private KeyCode playbackKey = KeyCode.P;
        
        private ARSession arSession;
        private ARCameraManager cameraManager;
        private ARTrackedImageManager trackedImageManager;
        private BodyTrackingController bodyController;
        private ImageTrackingVisualizer visualizer;
        
        private void Start()
        {
            if (!enableRemoteTesting) return;
            
            // Find AR components
            arSession = UnityEngine.Object.FindFirstObjectByType<ARSession>();
            cameraManager = UnityEngine.Object.FindFirstObjectByType<ARCameraManager>();
            trackedImageManager = UnityEngine.Object.FindFirstObjectByType<ARTrackedImageManager>();
            bodyController = UnityEngine.Object.FindFirstObjectByType<BodyTrackingController>();
            
            // Find or create visualizer
            visualizer = UnityEngine.Object.FindFirstObjectByType<ImageTrackingVisualizer>();
            if (visualizer == null)
            {
                var visualizerGO = new GameObject("ImageTrackingVisualizer");
                visualizer = visualizerGO.AddComponent<ImageTrackingVisualizer>();
                Logger.Log("✅ Created ImageTrackingVisualizer", "AR_REMOTE");
            }
            
            Logger.Log("🎮 AR Remote Test Manager initialized", "AR_REMOTE");
            Logger.Log($"📱 Controls: {toggleDebugKey}=Debug, {startRecordingKey}=Record, {stopRecordingKey}=Stop, {playbackKey}=Play", "AR_REMOTE");
            Logger.Log("📡 AR Foundation Remote: Check Window > XR > AR Foundation Remote for connection", "AR_REMOTE");
            
            // Automatically fix image tracking on start
            StartCoroutine(AutoFixImageTracking());
        }
        
        private System.Collections.IEnumerator AutoFixImageTracking()
        {
            // Wait a frame for everything to initialize
            yield return null;
            
            Logger.Log("🔧 Auto-fixing image tracking setup...", "AR_REMOTE");
            FixImageTracking();
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
            
            // Playback control
            if (Input.GetKeyDown(playbackKey))
            {
                TestTogglePlayback();
            }
        }
        
        private void OnGUI()
        {
            if (!enableRemoteTesting || !showDebugInfo) return;
            
            // Full width area that adapts to screen size
            var areaWidth = Mathf.Min(400, Screen.width - 20);
            var areaHeight = Mathf.Min(Screen.height - 40, 600);
            
            GUILayout.BeginArea(new Rect(10, 10, areaWidth, areaHeight));
            GUILayout.BeginVertical("box");
            
            // Title with bigger font
            var titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 18;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            
            GUILayout.Label("🎮 AR Remote Testing", titleStyle);
            GUILayout.Space(10);
            
            // AR Status section
            var headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 14;
            headerStyle.fontStyle = FontStyle.Bold;
            
            GUILayout.Label("📱 Status:", headerStyle);
            
            var statusStyle = new GUIStyle(GUI.skin.label);
            statusStyle.fontSize = 12;
            
            GUILayout.Label($"Session: {(arSession != null && arSession.enabled ? "✅" : "❌")} | Camera: {(cameraManager != null && cameraManager.enabled ? "✅" : "❌")}", statusStyle);
            GUILayout.Label($"Tracking: {(trackedImageManager != null && trackedImageManager.enabled ? "✅" : "❌")}", statusStyle);
            
            // Add image tracking debug info
            if (trackedImageManager != null)
            {
                var trackedImages = trackedImageManager.trackables;
                GUILayout.Label($"Images: {trackedImages.count}", statusStyle);
                
                foreach (var trackedImage in trackedImages)
                {
                    var trackingState = trackedImage.trackingState;
                    var stateColor = trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking ? "🟢" : 
                                   trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Limited ? "🟡" : "🔴";
                    GUILayout.Label($"  {stateColor} {trackedImage.referenceImage.name}", statusStyle);
                }
            }
            
            GUILayout.Space(10);
            
            // Body Tracking State section
            GUILayout.Label("🎯 Body Tracking:", headerStyle);
            
            if (bodyController != null)
            {
                var stateStyle = new GUIStyle(GUI.skin.label);
                stateStyle.fontSize = 14;
                stateStyle.fontStyle = FontStyle.Bold;
                stateStyle.alignment = TextAnchor.MiddleCenter;
                
                // Color code the mode
                switch (bodyController.CurrentMode)
                {
                    case BodyTrackingController.Mode.Idle:
                        stateStyle.normal.textColor = Color.white;
                        break;
                    case BodyTrackingController.Mode.Recording:
                        stateStyle.normal.textColor = Color.red;
                        break;
                    case BodyTrackingController.Mode.Playing:
                        stateStyle.normal.textColor = Color.green;
                        break;
                }
                
                GUILayout.Label($"{bodyController.CurrentMode}", stateStyle);
                
                // Show recording/playback info
                if (bodyController.IsRecording && bodyController.recorder != null)
                {
                    GUILayout.Label($"🔴 Recording: {bodyController.recorder.RecordedFrameCount} frames", statusStyle);
                }
                else if (bodyController.IsPlaying && bodyController.player != null)
                {
                    var progress = bodyController.player.PlaybackProgress * 100f;
                    GUILayout.Label($"▶️ Playing: {progress:F0}%", statusStyle);
                }
                else
                {
                    var recordings = bodyController.GetAvailableRecordings();
                    GUILayout.Label($"⏸️ Ready - {recordings.Count} recordings", statusStyle);
                }
            }
            else
            {
                GUILayout.Label("❌ No BodyTrackingController found", statusStyle);
            }
            
            GUILayout.Space(15);
            
            // Controls section with full-width buttons
            GUILayout.Label("🎮 Controls:", headerStyle);
            GUILayout.Space(5);
            
            // Button style - bigger and full width
            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 16;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.alignment = TextAnchor.MiddleCenter;
            
            // Recording controls - full width
            if (bodyController != null && bodyController.CanRecord)
            {
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button($"[{startRecordingKey}] Start Recording", buttonStyle, GUILayout.Height(40)))
                {
                    TestStartRecording();
                }
                GUI.backgroundColor = Color.white;
            }
            else if (bodyController != null && bodyController.IsRecording)
            {
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button($"[{stopRecordingKey}] Stop Recording", buttonStyle, GUILayout.Height(40)))
                {
                    TestStopRecording();
                }
                GUI.backgroundColor = Color.white;
            }
            
            GUILayout.Space(5);
            
            // Playback controls
            if (bodyController != null && bodyController.CanPlayback)
            {
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button($"[{playbackKey}] Start Playback", buttonStyle, GUILayout.Height(40)))
                {
                    TestTogglePlayback();
                }
                GUI.backgroundColor = Color.white;
            }
            else if (bodyController != null && bodyController.IsPlaying)
            {
                GUI.backgroundColor = Color.cyan;
                if (GUILayout.Button($"[{playbackKey}] Stop Playback", buttonStyle, GUILayout.Height(40)))
                {
                    TestTogglePlayback();
                }
                GUI.backgroundColor = Color.white;
            }
            
            GUILayout.Space(10);
            
            // Visualization controls
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("📱 Toggle Image Overlay", buttonStyle, GUILayout.Height(40)))
            {
                ToggleImageOverlay();
            }
            GUI.backgroundColor = Color.white;
            
            GUILayout.Space(5);
            
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("🏃 Toggle Hip Spheres", buttonStyle, GUILayout.Height(40)))
            {
                ToggleHipSpheres();
            }
            GUI.backgroundColor = Color.white;
            
            GUILayout.Space(15);
            
            // Troubleshooting section
            GUILayout.Label("🔧 Troubleshooting:", headerStyle);
            GUILayout.Space(5);
            
            if (GUILayout.Button("🔄 Fix Image Tracking", buttonStyle, GUILayout.Height(35)))
            {
                FixImageTracking();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("📊 Check Camera Status", buttonStyle, GUILayout.Height(35)))
            {
                CheckCameraStatus();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("🖼️ Debug Image Tracking", buttonStyle, GUILayout.Height(35)))
            {
                DebugImageTracking();
            }
            
            // AR Foundation Remote status
            GUILayout.Space(10);
            GUILayout.Label("📡 AR Foundation Remote:", headerStyle);
            
#if UNITY_EDITOR
            var remoteStyle = new GUIStyle(GUI.skin.label);
            remoteStyle.fontSize = 11;
            remoteStyle.wordWrap = true;
            
            // Check if AR Foundation Remote is available
            bool isRemoteEnabled = false;
            try
            {
                // Try to access AR Foundation Remote through reflection to avoid compilation errors
                var buildProcessorType = System.Type.GetType("UnityEditor.XR.ARSubsystems.ARBuildProcessor, Unity.XR.ARSubsystems.Editor");
                if (buildProcessorType != null)
                {
                    var property = buildProcessorType.GetProperty("isARFoundationRemoteEnabled");
                    if (property != null)
                    {
                        isRemoteEnabled = (bool)property.GetValue(null);
                    }
                }
            }
            catch
            {
                // Fallback - assume not enabled if we can't check
                isRemoteEnabled = false;
            }
            
            if (isRemoteEnabled)
            {
                GUILayout.Label("✅ AR Foundation Remote is enabled", remoteStyle);
                GUILayout.Label("📱 Connect your iPhone and check the console for connection status", remoteStyle);
            }
            else
            {
                GUILayout.Label("❌ AR Foundation Remote not detected", remoteStyle);
                GUILayout.Label("Enable it in Window > XR > AR Foundation Remote", remoteStyle);
            }
#else
            GUILayout.Label("AR Foundation Remote only works in Editor", statusStyle);
#endif
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        private void TestStartRecording()
        {
            if (bodyController == null)
            {
                Logger.LogError("❌ No BodyTrackingController found", "AR_REMOTE");
                return;
            }
            
            bool success = bodyController.StartRecording();
            if (success)
            {
                Logger.Log("🔴 Recording started via test manager", "AR_REMOTE");
            }
            else
            {
                Logger.LogError("❌ Failed to start recording", "AR_REMOTE");
            }
        }
        
        private void TestStopRecording()
        {
            if (bodyController == null)
            {
                Logger.LogError("❌ No BodyTrackingController found", "AR_REMOTE");
                return;
            }
            
            bool success = bodyController.StopRecording();
            if (success)
            {
                Logger.Log("⏹️ Recording stopped via test manager", "AR_REMOTE");
            }
            else
            {
                Logger.LogError("❌ Failed to stop recording", "AR_REMOTE");
            }
        }
        
        private void TestTogglePlayback()
        {
            if (bodyController == null)
            {
                Logger.LogError("❌ No BodyTrackingController found", "AR_REMOTE");
                return;
            }
            
            if (bodyController.IsPlaying)
            {
                bool success = bodyController.StopPlayback();
                if (success)
                {
                    Logger.Log("⏹️ Playback stopped via test manager", "AR_REMOTE");
                }
                else
                {
                    Logger.LogError("❌ Failed to stop playback", "AR_REMOTE");
                }
            }
            else if (bodyController.CanPlayback)
            {
                bool success = bodyController.StartPlayback();
                if (success)
                {
                    Logger.Log("▶️ Playback started via test manager", "AR_REMOTE");
                }
                else
                {
                    Logger.LogError("❌ Failed to start playback", "AR_REMOTE");
                }
            }
        }
        
        private void ToggleImageOverlay()
        {
            if (visualizer != null)
            {
                visualizer.ToggleImageOverlay();
                Logger.Log("📱 Toggled image overlays", "AR_REMOTE");
            }
        }
        
        private void ToggleHipSpheres()
        {
            if (visualizer != null)
            {
                visualizer.ToggleHipSpheres();
                Logger.Log("🏃 Toggled hip spheres", "AR_REMOTE");
            }
        }
        
        private void CheckCameraStatus()
        {
            Logger.Log("🔍 Checking camera status...", "AR_REMOTE");
            
            if (cameraManager != null)
            {
                Logger.Log($"📷 Camera Manager enabled: {cameraManager.enabled}", "AR_REMOTE");
                Logger.Log($"📷 Camera Manager active: {cameraManager.gameObject.activeInHierarchy}", "AR_REMOTE");
                
                var camera = cameraManager.GetComponent<Camera>();
                if (camera != null)
                {
                    Logger.Log($"📷 Camera component found: {camera.name}", "AR_REMOTE");
                    Logger.Log($"📷 Camera enabled: {camera.enabled}", "AR_REMOTE");
                    Logger.Log($"📷 Camera clear flags: {camera.clearFlags}", "AR_REMOTE");
                    Logger.Log($"📷 Camera background color: {camera.backgroundColor}", "AR_REMOTE");
                }
                else
                {
                    Logger.LogError("❌ No Camera component found on AR Camera Manager", "AR_REMOTE");
                }
            }
            else
            {
                Logger.LogError("❌ AR Camera Manager not found", "AR_REMOTE");
            }
            
            if (arSession != null)
            {
                Logger.Log($"📱 AR Session enabled: {arSession.enabled}", "AR_REMOTE");
                Logger.Log($"📱 AR Session state: {ARSession.state}", "AR_REMOTE");
            }
            else
            {
                Logger.LogError("❌ AR Session not found", "AR_REMOTE");
            }
            
#if UNITY_EDITOR
            Logger.Log("💡 In Unity Editor: Camera feed requires AR Foundation Remote connection", "AR_REMOTE");
            Logger.Log("💡 Black screen is normal without device connection", "AR_REMOTE");
#endif
        }
        
        private void DebugImageTracking()
        {
            Logger.Log("🎯 Debugging image tracking...", "AR_REMOTE");
            
            if (trackedImageManager == null)
            {
                Logger.LogError("❌ AR Tracked Image Manager not found", "AR_REMOTE");
                return;
            }
            
            Logger.Log($"📷 Tracked Image Manager enabled: {trackedImageManager.enabled}", "AR_REMOTE");
            Logger.Log($"📷 Tracked Image Manager active: {trackedImageManager.gameObject.activeInHierarchy}", "AR_REMOTE");
            
            // Check reference image library
            var referenceLibrary = trackedImageManager.referenceLibrary;
            if (referenceLibrary != null)
            {
                Logger.Log($"📚 Reference library loaded: {referenceLibrary.count} images", "AR_REMOTE");
                
                for (int i = 0; i < referenceLibrary.count; i++)
                {
                    var refImage = referenceLibrary[i];
                    Logger.Log($"  📖 Image {i}: {refImage.name} ({refImage.size.x}x{refImage.size.y})", "AR_REMOTE");
                }
            }
            else
            {
                Logger.LogError("❌ No reference image library found", "AR_REMOTE");
            }
            
            // Check runtime library
            var runtimeLibrary = trackedImageManager.CreateRuntimeLibrary();
            if (runtimeLibrary != null)
            {
                Logger.Log($"🔧 Runtime library created successfully", "AR_REMOTE");
            }
            else
            {
                Logger.LogError("❌ Failed to create runtime library", "AR_REMOTE");
            }
            
            // Check tracked images
            var trackedImages = trackedImageManager.trackables;
            Logger.Log($"🎯 Currently tracked images: {trackedImages.count}", "AR_REMOTE");
            
            foreach (var trackedImage in trackedImages)
            {
                Logger.Log($"  🎯 {trackedImage.referenceImage.name}:", "AR_REMOTE");
                Logger.Log($"    State: {trackedImage.trackingState}", "AR_REMOTE");
                Logger.Log($"    Position: {trackedImage.transform.position}", "AR_REMOTE");
                Logger.Log($"    Rotation: {trackedImage.transform.rotation.eulerAngles}", "AR_REMOTE");
                Logger.Log($"    Active: {trackedImage.gameObject.activeInHierarchy}", "AR_REMOTE");
            }
            
            // Check AR Session state
            Logger.Log($"📱 AR Session state: {ARSession.state}", "AR_REMOTE");
            
            // Tips for better tracking
            Logger.Log("💡 Image tracking tips:", "AR_REMOTE");
            Logger.Log("  • Ensure good lighting conditions", "AR_REMOTE");
            Logger.Log("  • Hold image steady and flat", "AR_REMOTE");
            Logger.Log("  • Image should be at least 15cm wide in real world", "AR_REMOTE");
            Logger.Log("  • Avoid reflective surfaces or glass", "AR_REMOTE");
            Logger.Log("  • Make sure image has good contrast and detail", "AR_REMOTE");
        }
        
        private void FixImageTracking()
        {
            Logger.Log("🔧 Fixing image tracking setup...", "AR_REMOTE");
            
            if (trackedImageManager == null)
            {
                Logger.LogError("❌ AR Tracked Image Manager not found", "AR_REMOTE");
                return;
            }
            
            // Load the reference image library
            var libraryPath = "Assets/ImageTracking/TENDORImageLibrary.asset";
            
#if UNITY_EDITOR
            var referenceLibrary = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.XR.ARSubsystems.XRReferenceImageLibrary>(libraryPath);
            
            if (referenceLibrary != null)
            {
                Logger.Log($"📚 Found reference library with {referenceLibrary.count} images", "AR_REMOTE");
                
                // Assign the library to the tracked image manager
                trackedImageManager.referenceLibrary = referenceLibrary;
                
                // Enable the manager if it's not already enabled
                if (!trackedImageManager.enabled)
                {
                    trackedImageManager.enabled = true;
                    Logger.Log("✅ Enabled AR Tracked Image Manager", "AR_REMOTE");
                }
                
                // Set max number of moving images
                if (trackedImageManager.maxNumberOfMovingImages == 0)
                {
                    trackedImageManager.maxNumberOfMovingImages = 2;
                    Logger.Log("✅ Set max moving images to 2", "AR_REMOTE");
                }
                
                // Mark scene as dirty so changes are saved
                UnityEditor.EditorUtility.SetDirty(trackedImageManager);
                
                Logger.Log("✅ Image tracking setup fixed!", "AR_REMOTE");
                Logger.Log("💡 Available images for tracking:", "AR_REMOTE");
                
                for (int i = 0; i < referenceLibrary.count; i++)
                {
                    var refImage = referenceLibrary[i];
                    Logger.Log($"  📖 {refImage.name} ({refImage.size.x:F2}x{refImage.size.y:F2}m)", "AR_REMOTE");
                }
            }
            else
            {
                Logger.LogError($"❌ Could not load reference library from {libraryPath}", "AR_REMOTE");
                Logger.Log("💡 Make sure TENDORImageLibrary.asset exists in Assets/ImageTracking/", "AR_REMOTE");
            }
#else
            Logger.LogWarning("⚠️ Image tracking fix only works in Unity Editor", "AR_REMOTE");
#endif
        }
    }
} 