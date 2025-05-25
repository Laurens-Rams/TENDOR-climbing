using UnityEngine;
using UnityEditor;
using BodyTracking;
using BodyTracking.AR;
using BodyTracking.Storage;
using UnityEngine.XR.ARFoundation;

namespace BodyTracking.Editor
{
    public class BodyTrackingSetupTool : EditorWindow
    {
        private Vector2 scrollPosition;
        private BodyTrackingController controller;
        private bool autoRefresh = true;
        
        [MenuItem("Tools/Body Tracking/Setup Tool")]
        public static void ShowWindow()
        {
            GetWindow<BodyTrackingSetupTool>("Body Tracking Setup");
        }

        void OnEnable()
        {
            // Auto-refresh every second
            EditorApplication.update += AutoRefresh;
        }

        void OnDisable()
        {
            EditorApplication.update -= AutoRefresh;
        }

        void AutoRefresh()
        {
            if (autoRefresh)
            {
                Repaint();
            }
        }

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.LabelField("Body Tracking Setup Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Find controller
            if (controller == null)
            {
                controller = FindObjectOfType<BodyTrackingController>();
            }
            
            // Auto-refresh toggle
            autoRefresh = EditorGUILayout.Toggle("Auto Refresh", autoRefresh);
            EditorGUILayout.Space();
            
            // System Status
            DrawSystemStatus();
            EditorGUILayout.Space();
            
            // Setup Section
            DrawSetupSection();
            EditorGUILayout.Space();
            
            // AR Configuration
            DrawARConfiguration();
            EditorGUILayout.Space();
            
            // Recording Management
            DrawRecordingManagement();
            EditorGUILayout.Space();
            
            // Testing Tools
            DrawTestingTools();
            
            EditorGUILayout.EndScrollView();
        }

        void DrawSystemStatus()
        {
            EditorGUILayout.LabelField("System Status", EditorStyles.boldLabel);
            
            if (controller == null)
            {
                EditorGUILayout.HelpBox("No BodyTrackingController found in scene", MessageType.Warning);
                return;
            }
            
            // Status indicators
            string status = Application.isPlaying ? GetRuntimeStatus() : "Editor Mode";
            EditorGUILayout.LabelField("Status:", status);
            
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Mode:", controller.CurrentMode.ToString());
                EditorGUILayout.LabelField("Can Record:", controller.CanRecord.ToString());
                EditorGUILayout.LabelField("Can Playback:", controller.CanPlayback.ToString());
            }
        }

        string GetRuntimeStatus()
        {
            if (!controller.IsInitialized) return "Not Initialized";
            
            switch (controller.CurrentMode)
            {
                case OperationMode.Ready:
                    return "Ready";
                case OperationMode.Recording:
                    return "Recording...";
                case OperationMode.Playing:
                    return "Playing...";
                default:
                    return "Unknown";
            }
        }

        void DrawSetupSection()
        {
            EditorGUILayout.LabelField("Setup", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Auto Setup Scene"))
            {
                AutoSetupScene();
            }
            
            EditorGUILayout.Space();
            
            // Component checks
            DrawComponentCheck<BodyTrackingController>("Body Tracking Controller");
            DrawComponentCheck<ARImageTargetManager>("AR Image Target Manager");
            DrawComponentCheck<ARHumanBodyManager>("AR Human Body Manager");
            DrawComponentCheck<ARTrackedImageManager>("AR Tracked Image Manager");
            DrawComponentCheck<ARSessionOrigin>("AR Session Origin");
            DrawComponentCheck<ARSession>("AR Session");
        }

        void DrawComponentCheck<T>(string name) where T : Component
        {
            T component = FindObjectOfType<T>();
            bool exists = component != null;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(200));
            
            if (exists)
            {
                EditorGUILayout.LabelField("✓", GUILayout.Width(20));
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    Selection.activeGameObject = component.gameObject;
                }
            }
            else
            {
                EditorGUILayout.LabelField("✗", GUILayout.Width(20));
                if (GUILayout.Button("Add", GUILayout.Width(60)))
                {
                    AddComponent<T>();
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        void DrawARConfiguration()
        {
            EditorGUILayout.LabelField("AR Configuration", EditorStyles.boldLabel);
            
            var imageManager = FindObjectOfType<ARImageTargetManager>();
            if (imageManager != null)
            {
                EditorGUILayout.LabelField("Image Target Manager Found");
                if (Application.isPlaying)
                {
                    EditorGUILayout.LabelField("Image Detected:", imageManager.IsImageDetected.ToString());
                }
            }
            else
            {
                EditorGUILayout.HelpBox("ARImageTargetManager not found", MessageType.Warning);
            }
            
            var trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
            if (trackedImageManager != null)
            {
                EditorGUILayout.LabelField("Reference Library:", 
                    trackedImageManager.referenceLibrary != null ? "Loaded" : "None");
                
                if (trackedImageManager.referenceLibrary != null)
                {
                    var library = trackedImageManager.referenceLibrary;
                    EditorGUILayout.LabelField("Image Count:", library.count.ToString());
                }
            }
        }

        void DrawRecordingManagement()
        {
            EditorGUILayout.LabelField("Recording Management", EditorStyles.boldLabel);
            
            var recordings = RecordingStorage.GetAvailableRecordings();
            EditorGUILayout.LabelField("Available Recordings:", recordings.Count.ToString());
            
            var totalSize = RecordingStorage.GetTotalStorageUsed();
            EditorGUILayout.LabelField("Total Storage:", FormatBytes(totalSize));
            
            EditorGUILayout.Space();
            
            foreach (var recording in recordings)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(recording, GUILayout.Width(200));
                
                var metadata = RecordingStorage.GetRecordingMetadata(recording);
                if (metadata != null)
                {
                    EditorGUILayout.LabelField($"{metadata.FormattedDuration}, {metadata.FormattedFileSize}");
                }
                
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("Delete Recording", 
                        $"Delete recording '{recording}'?", "Delete", "Cancel"))
                    {
                        RecordingStorage.DeleteRecording(recording);
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Open Storage Folder"))
            {
                OpenStorageFolder();
            }
        }

        void DrawTestingTools()
        {
            EditorGUILayout.LabelField("Testing Tools", EditorStyles.boldLabel);
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Testing tools only available in Play Mode", MessageType.Info);
                return;
            }
            
            if (controller == null)
            {
                EditorGUILayout.HelpBox("No controller available", MessageType.Warning);
                return;
            }
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = controller.CanRecord;
            if (GUILayout.Button("Start Recording"))
            {
                controller.StartRecording();
            }
            
            GUI.enabled = controller.IsRecording;
            if (GUILayout.Button("Stop Recording"))
            {
                controller.StopRecording();
            }
            
            GUI.enabled = controller.CanPlayback;
            if (GUILayout.Button("Start Playback"))
            {
                controller.StartPlayback();
            }
            
            GUI.enabled = controller.IsPlaying;
            if (GUILayout.Button("Stop Playback"))
            {
                controller.StopPlayback();
            }
            
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        void AutoSetupScene()
        {
            Debug.Log("[BodyTrackingSetupTool] Starting auto setup...");
            
            // Create main system object if needed
            var controller = FindObjectOfType<BodyTrackingController>();
            if (controller == null)
            {
                var systemObject = new GameObject("BodyTrackingSystem");
                controller = systemObject.AddComponent<BodyTrackingController>();
                Debug.Log("[BodyTrackingSetupTool] Created BodyTrackingController");
            }
            
            // Add AR Image Target Manager
            var imageManager = FindObjectOfType<ARImageTargetManager>();
            if (imageManager == null)
            {
                imageManager = controller.gameObject.AddComponent<ARImageTargetManager>();
                Debug.Log("[BodyTrackingSetupTool] Added ARImageTargetManager");
            }
            
            // Ensure AR components exist
            EnsureARComponent<ARSession>("AR Session");
            EnsureARComponent<ARSessionOrigin>("AR Session Origin");
            EnsureARComponent<ARHumanBodyManager>("AR Human Body Manager");
            EnsureARComponent<ARTrackedImageManager>("AR Tracked Image Manager");
            
            Debug.Log("[BodyTrackingSetupTool] Auto setup completed");
        }

        void EnsureARComponent<T>(string name) where T : Component
        {
            var component = FindObjectOfType<T>();
            if (component == null)
            {
                Debug.LogWarning($"[BodyTrackingSetupTool] {name} not found - please add manually");
            }
        }

        void AddComponent<T>() where T : Component
        {
            if (Selection.activeGameObject != null)
            {
                Selection.activeGameObject.AddComponent<T>();
                Debug.Log($"Added {typeof(T).Name} to {Selection.activeGameObject.name}");
            }
            else
            {
                Debug.LogWarning("No GameObject selected");
            }
        }

        void OpenStorageFolder()
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, "BodyTrackingRecordings");
            if (System.IO.Directory.Exists(path))
            {
                EditorUtility.RevealInFinder(path);
            }
            else
            {
                Debug.LogWarning("Storage folder does not exist yet");
            }
        }

        string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
} 