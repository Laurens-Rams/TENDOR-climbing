using UnityEngine;
using UnityEditor;
using UnityEngine.XR.ARFoundation;
using BodyTracking;
using BodyTracking.UI;
using BodyTracking.AR;

namespace BodyTracking.Editor
{
    /// <summary>
    /// Editor tool to validate scene setup for body tracking system
    /// </summary>
    public class SceneValidationTool : EditorWindow
    {
        [MenuItem("TENDOR/Validate Scene Setup")]
        public static void ShowWindow()
        {
            GetWindow<SceneValidationTool>("Scene Validation");
        }

        private Vector2 scrollPosition;

        void OnGUI()
        {
            GUILayout.Label("TENDOR Scene Validation Tool", EditorStyles.boldLabel);
            GUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // AR Foundation Components
            GUILayout.Label("AR Foundation Components", EditorStyles.boldLabel);
            ValidateComponent<ARSession>("AR Session");
            ValidateComponent<ARSessionOrigin>("XR Origin (AR Rig)");
            ValidateComponent<ARHumanBodyManager>("AR Human Body Manager");
            ValidateComponent<ARTrackedImageManager>("AR Tracked Image Manager");

            GUILayout.Space(10);

            // Body Tracking Components
            GUILayout.Label("Body Tracking Components", EditorStyles.boldLabel);
            ValidateComponent<BodyTrackingController>("Body Tracking Controller");
            ValidateComponent<ARImageTargetManager>("AR Image Target Manager");
            ValidateComponent<BodyTrackingUI>("Body Tracking UI");

            GUILayout.Space(10);

            // UI Components
            GUILayout.Label("UI Validation", EditorStyles.boldLabel);
            ValidateUISetup();

            GUILayout.Space(10);

            // Quick Fix Buttons
            GUILayout.Label("Quick Fixes", EditorStyles.boldLabel);
            if (GUILayout.Button("Auto-Fix Missing Components"))
            {
                AutoFixMissingComponents();
            }

            if (GUILayout.Button("Create Body Tracking System GameObject"))
            {
                CreateBodyTrackingSystem();
            }

            EditorGUILayout.EndScrollView();
        }

        private void ValidateComponent<T>(string componentName) where T : Component
        {
            T component = FindObjectOfType<T>();
            if (component != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("✅ " + componentName, GUILayout.Width(200));
                EditorGUILayout.ObjectField(component, typeof(T), true);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("❌ " + componentName + " - MISSING", GUILayout.Width(200));
                GUI.color = Color.red;
                EditorGUILayout.LabelField("NOT FOUND");
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }
        }

        private void ValidateUISetup()
        {
            var ui = FindObjectOfType<BodyTrackingUI>();
            if (ui == null)
            {
                EditorGUILayout.LabelField("❌ BodyTrackingUI script not found");
                return;
            }

            // Check UI connections
            CheckUIField(ui.controller, "Controller");
            CheckUIField(ui.recordButton, "Record Button");
            CheckUIField(ui.stopRecordButton, "Stop Record Button");
            CheckUIField(ui.playButton, "Play Button");
            CheckUIField(ui.stopPlayButton, "Stop Play Button");
            CheckUIField(ui.loadButton, "Load Button");
            CheckUIField(ui.statusText, "Status Text");
            CheckUIField(ui.modeText, "Mode Text");
            CheckUIField(ui.recordingsDropdown, "Recordings Dropdown");
        }

        private void CheckUIField(Object field, string fieldName)
        {
            if (field != null)
            {
                EditorGUILayout.LabelField("✅ " + fieldName + " connected");
            }
            else
            {
                EditorGUILayout.LabelField("❌ " + fieldName + " - NOT CONNECTED");
            }
        }

        private void AutoFixMissingComponents()
        {
            // Find or create XR Origin
            var xrOrigin = FindObjectOfType<ARSessionOrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("XR Origin not found. Please add AR Foundation XR Origin prefab to scene.");
                return;
            }

            // Add missing AR components
            if (xrOrigin.GetComponent<ARHumanBodyManager>() == null)
            {
                xrOrigin.gameObject.AddComponent<ARHumanBodyManager>();
                Debug.Log("Added ARHumanBodyManager to XR Origin");
            }

            if (xrOrigin.GetComponent<ARTrackedImageManager>() == null)
            {
                xrOrigin.gameObject.AddComponent<ARTrackedImageManager>();
                Debug.Log("Added ARTrackedImageManager to XR Origin");
            }

            // Find or create AR Session
            if (FindObjectOfType<ARSession>() == null)
            {
                var sessionGO = new GameObject("AR Session");
                sessionGO.AddComponent<ARSession>();
                Debug.Log("Created AR Session GameObject");
            }

            Debug.Log("Auto-fix completed. Please manually configure component references.");
        }

        private void CreateBodyTrackingSystem()
        {
            // Check if already exists
            if (FindObjectOfType<BodyTrackingController>() != null)
            {
                Debug.LogWarning("BodyTrackingController already exists in scene");
                return;
            }

            // Create main system GameObject
            var systemGO = new GameObject("BodyTrackingSystem");
            
            // Add all required components
            var controller = systemGO.AddComponent<BodyTrackingController>();
            var imageTargetManager = systemGO.AddComponent<ARImageTargetManager>();
            var recorder = systemGO.AddComponent<BodyTracking.Recording.BodyTrackingRecorder>();
            var player = systemGO.AddComponent<BodyTracking.Playback.BodyTrackingPlayer>();

            // Try to auto-connect references
            var xrOrigin = FindObjectOfType<ARSessionOrigin>();
            if (xrOrigin != null)
            {
                var humanBodyManager = xrOrigin.GetComponent<ARHumanBodyManager>();
                var trackedImageManager = xrOrigin.GetComponent<ARTrackedImageManager>();

                // Use reflection to set private fields if needed
                if (humanBodyManager != null)
                {
                    var field = typeof(BodyTrackingController).GetField("humanBodyManager");
                    if (field != null) field.SetValue(controller, humanBodyManager);
                }

                if (trackedImageManager != null)
                {
                    var field = typeof(ARImageTargetManager).GetField("trackedImageManager");
                    if (field != null) field.SetValue(imageTargetManager, trackedImageManager);
                }
            }

            Debug.Log("Created BodyTrackingSystem GameObject with all components. Please configure references manually.");
            Selection.activeGameObject = systemGO;
        }
    }
} 