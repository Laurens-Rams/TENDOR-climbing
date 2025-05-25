using UnityEngine;
using UnityEditor;
using BodyTracking;
using BodyTracking.UI;
using BodyTracking.AR;
using UnityEngine.XR.ARFoundation;

namespace BodyTracking.Editor
{
    public class SystemStateDebug : EditorWindow
    {
        [MenuItem("Tools/Body Tracking/Debug System State")]
        public static void ShowWindow()
        {
            GetWindow<SystemStateDebug>("System State Debug");
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("System State Debug", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to debug system state", MessageType.Info);
                return;
            }
            
            var controller = FindObjectOfType<BodyTrackingController>();
            var imageManager = FindObjectOfType<ARImageTargetManager>();
            var ui = FindObjectOfType<BodyTrackingUI>();
            
            if (controller == null)
            {
                EditorGUILayout.HelpBox("No BodyTrackingController found!", MessageType.Error);
                return;
            }
            
            // System Status
            EditorGUILayout.LabelField("System Status:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Initialized: {controller.IsInitialized}");
            EditorGUILayout.LabelField($"Current Mode: {controller.CurrentMode}");
            EditorGUILayout.Space();
            
            // Image Target Status
            EditorGUILayout.LabelField("Image Target Status:", EditorStyles.boldLabel);
            if (imageManager != null)
            {
                EditorGUILayout.LabelField($"Image Detected: {imageManager.IsImageDetected}");
                EditorGUILayout.LabelField($"Target Name: {imageManager.targetImageName}");
                
                var trackedImageManager = imageManager.trackedImageManager;
                if (trackedImageManager != null)
                {
                    EditorGUILayout.LabelField($"Reference Library: {(trackedImageManager.referenceLibrary != null ? "✓" : "✗")}");
                    if (trackedImageManager.referenceLibrary != null)
                    {
                        EditorGUILayout.LabelField($"Library Image Count: {trackedImageManager.referenceLibrary.count}");
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Tracked Image Manager: ✗ Not assigned!");
                }
            }
            else
            {
                EditorGUILayout.LabelField("ARImageTargetManager: ✗ Not found!");
            }
            EditorGUILayout.Space();
            
            // Button States
            EditorGUILayout.LabelField("Button States:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Can Record: {controller.CanRecord}");
            EditorGUILayout.LabelField($"Can Playback: {controller.CanPlayback}");
            EditorGUILayout.LabelField($"Is Recording: {controller.IsRecording}");
            EditorGUILayout.LabelField($"Is Playing: {controller.IsPlaying}");
            EditorGUILayout.Space();
            
            // Button Interactable States
            if (ui != null)
            {
                EditorGUILayout.LabelField("UI Button States:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Record Button Interactable: {(ui.recordButton != null ? ui.recordButton.interactable.ToString() : "No Button")}");
                EditorGUILayout.LabelField($"Play Button Interactable: {(ui.playButton != null ? ui.playButton.interactable.ToString() : "No Button")}");
                EditorGUILayout.Space();
            }
            
            // Detailed Analysis
            EditorGUILayout.LabelField("Issue Analysis:", EditorStyles.boldLabel);
            
            if (!controller.IsInitialized)
            {
                EditorGUILayout.HelpBox("❌ System not initialized", MessageType.Error);
            }
            
            if (imageManager != null && !imageManager.IsImageDetected)
            {
                EditorGUILayout.HelpBox("❌ No image target detected - point camera at TENDOR image", MessageType.Warning);
                
                if (imageManager.trackedImageManager == null)
                {
                    EditorGUILayout.HelpBox("❌ No ARTrackedImageManager assigned to ARImageTargetManager", MessageType.Error);
                }
                else if (imageManager.trackedImageManager.referenceLibrary == null)
                {
                    EditorGUILayout.HelpBox("❌ No reference library assigned to ARTrackedImageManager", MessageType.Error);
                }
            }
            
            if (!controller.CanRecord)
            {
                string reason = "Cannot record because: ";
                if (!controller.IsInitialized) reason += "Not initialized, ";
                if (imageManager != null && !imageManager.IsImageDetected) reason += "No image target, ";
                if (controller.CurrentMode != OperationMode.Ready) reason += $"Mode is {controller.CurrentMode}, ";
                
                EditorGUILayout.HelpBox($"❌ {reason.TrimEnd(' ', ',')}", MessageType.Warning);
            }
            
            EditorGUILayout.Space();
            
            // Quick Actions
            EditorGUILayout.LabelField("Quick Actions:", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Force Enable Record Button (TEST ONLY)"))
            {
                if (ui != null && ui.recordButton != null)
                {
                    ui.recordButton.interactable = true;
                    Debug.Log("Forced record button to be interactable");
                }
            }
            
            if (GUILayout.Button("Simulate Button Click"))
            {
                if (ui != null && ui.recordButton != null)
                {
                    Debug.Log("Simulating record button click...");
                    // Call the actual button handler directly
                    var method = typeof(BodyTrackingUI).GetMethod("OnRecordClicked", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (method != null)
                    {
                        method.Invoke(ui, null);
                    }
                }
            }
            
            if (GUILayout.Button("Check AR Remote Settings"))
            {
                CheckARRemoteSettings();
            }
            
            EditorGUILayout.Space();
            
            // Auto-refresh
            if (GUILayout.Toggle(true, "Auto-refresh"))
            {
                Repaint();
            }
        }

        private void CheckARRemoteSettings()
        {
            Debug.Log("=== AR Remote Settings Check ===");
            
            var trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
            if (trackedImageManager != null)
            {
                Debug.Log($"ARTrackedImageManager found: {trackedImageManager.name}");
                Debug.Log($"Reference Library: {(trackedImageManager.referenceLibrary != null ? "Assigned" : "None")}");
                Debug.Log($"Max Number Of Moving Images: {trackedImageManager.maxNumberOfMovingImages}");
                
                if (trackedImageManager.referenceLibrary != null)
                {
                    var library = trackedImageManager.referenceLibrary;
                    Debug.Log($"Library image count: {library.count}");
                    
                    for (int i = 0; i < library.count; i++)
                    {
                        var referenceImage = library[i];
                        Debug.Log($"Image {i}: {referenceImage.name} - Size: {referenceImage.size}");
                    }
                }
            }
            else
            {
                Debug.LogError("No ARTrackedImageManager found in scene!");
            }
            
            var imageTargetManager = FindObjectOfType<ARImageTargetManager>();
            if (imageTargetManager != null)
            {
                Debug.Log($"ARImageTargetManager target name: {imageTargetManager.targetImageName}");
            }
        }
    }
} 