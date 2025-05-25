using UnityEngine;
using UnityEditor;
using BodyTracking;
using BodyTracking.UI;
using UnityEngine.UI;

namespace BodyTracking.Editor
{
    public class UIDebugTool : EditorWindow
    {
        [MenuItem("Tools/Body Tracking/Debug UI Connections")]
        public static void ShowWindow()
        {
            GetWindow<UIDebugTool>("UI Debug");
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("UI Connection Debug Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            var bodyTrackingUI = FindObjectOfType<BodyTrackingUI>();
            var controller = FindObjectOfType<BodyTrackingController>();
            
            if (bodyTrackingUI == null)
            {
                EditorGUILayout.HelpBox("No BodyTrackingUI found in scene!", MessageType.Error);
                if (GUILayout.Button("Find Canvas and Add BodyTrackingUI"))
                {
                    var canvas = FindObjectOfType<Canvas>();
                    if (canvas != null)
                    {
                        canvas.gameObject.AddComponent<BodyTrackingUI>();
                        Debug.Log("Added BodyTrackingUI to Canvas");
                    }
                    else
                    {
                        Debug.LogError("No Canvas found!");
                    }
                }
                return;
            }
            
            EditorGUILayout.LabelField("BodyTrackingUI Found ✓", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Check controller connection
            EditorGUILayout.LabelField("Controller Connection:");
            if (bodyTrackingUI.controller == null)
            {
                EditorGUILayout.HelpBox("Controller not assigned!", MessageType.Warning);
                if (controller != null && GUILayout.Button("Auto-assign Controller"))
                {
                    bodyTrackingUI.controller = controller;
                    EditorUtility.SetDirty(bodyTrackingUI);
                    Debug.Log("Assigned controller to BodyTrackingUI");
                }
            }
            else
            {
                EditorGUILayout.LabelField("✓ Controller assigned");
            }
            
            EditorGUILayout.Space();
            
            // Check button connections
            EditorGUILayout.LabelField("Button Connections:", EditorStyles.boldLabel);
            
            CheckButtonConnection(bodyTrackingUI, "Record Button", bodyTrackingUI.recordButton, "RecordButton");
            CheckButtonConnection(bodyTrackingUI, "Stop Record Button", bodyTrackingUI.stopRecordButton, "StopRecordButton");
            CheckButtonConnection(bodyTrackingUI, "Play Button", bodyTrackingUI.playButton, "PlayButton");
            CheckButtonConnection(bodyTrackingUI, "Stop Play Button", bodyTrackingUI.stopPlayButton, "StopPlayButton");
            CheckButtonConnection(bodyTrackingUI, "Load Button", bodyTrackingUI.loadButton, "LoadButton");
            
            EditorGUILayout.Space();
            
            // Check text components
            EditorGUILayout.LabelField("Text Components:", EditorStyles.boldLabel);
            
            CheckTextConnection(bodyTrackingUI, "Status Text", bodyTrackingUI.statusText, "StatusText");
            CheckTextConnection(bodyTrackingUI, "Mode Text", bodyTrackingUI.modeText, "ModeText");
            
            EditorGUILayout.Space();
            
            // Check dropdown
            EditorGUILayout.LabelField("Dropdown Connection:");
            if (bodyTrackingUI.recordingsDropdown == null)
            {
                EditorGUILayout.HelpBox("Recordings dropdown not assigned!", MessageType.Warning);
                if (GUILayout.Button("Find and Assign Dropdown"))
                {
                    var dropdown = FindObjectByName<TMPro.TMP_Dropdown>("RecordingsDropdown");
                    if (dropdown != null)
                    {
                        bodyTrackingUI.recordingsDropdown = dropdown;
                        EditorUtility.SetDirty(bodyTrackingUI);
                        Debug.Log("Assigned dropdown to BodyTrackingUI");
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("✓ Dropdown assigned");
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Auto-Fix All Connections", GUILayout.Height(30)))
            {
                AutoFixAllConnections();
            }
            
            if (Application.isPlaying && GUILayout.Button("Test Record Button", GUILayout.Height(30)))
            {
                TestRecordButton();
            }
        }

        private void CheckButtonConnection(BodyTrackingUI ui, string displayName, Button button, string expectedName)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(displayName, GUILayout.Width(150));
            
            if (button == null)
            {
                EditorGUILayout.LabelField("✗ Not assigned", GUILayout.Width(100));
                if (GUILayout.Button("Find", GUILayout.Width(50)))
                {
                    var foundButton = FindObjectByName<Button>(expectedName);
                    if (foundButton != null)
                    {
                        AssignButtonToUI(ui, displayName, foundButton);
                        Debug.Log($"Found and assigned {expectedName}");
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find button named {expectedName}");
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("✓ Assigned", GUILayout.Width(100));
                if (GUILayout.Button("Select", GUILayout.Width(50)))
                {
                    Selection.activeGameObject = button.gameObject;
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void CheckTextConnection(BodyTrackingUI ui, string displayName, TMPro.TextMeshProUGUI text, string expectedName)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(displayName, GUILayout.Width(150));
            
            if (text == null)
            {
                EditorGUILayout.LabelField("✗ Not assigned", GUILayout.Width(100));
                if (GUILayout.Button("Find", GUILayout.Width(50)))
                {
                    var foundText = FindObjectByName<TMPro.TextMeshProUGUI>(expectedName);
                    if (foundText != null)
                    {
                        AssignTextToUI(ui, displayName, foundText);
                        Debug.Log($"Found and assigned {expectedName}");
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("✓ Assigned", GUILayout.Width(100));
                if (GUILayout.Button("Select", GUILayout.Width(50)))
                {
                    Selection.activeGameObject = text.gameObject;
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private T FindObjectByName<T>(string name) where T : Component
        {
            var allObjects = FindObjectsOfType<T>();
            foreach (var obj in allObjects)
            {
                if (obj.gameObject.name == name)
                {
                    return obj;
                }
            }
            return null;
        }

        private void AssignButtonToUI(BodyTrackingUI ui, string buttonType, Button button)
        {
            switch (buttonType)
            {
                case "Record Button":
                    ui.recordButton = button;
                    break;
                case "Stop Record Button":
                    ui.stopRecordButton = button;
                    break;
                case "Play Button":
                    ui.playButton = button;
                    break;
                case "Stop Play Button":
                    ui.stopPlayButton = button;
                    break;
                case "Load Button":
                    ui.loadButton = button;
                    break;
            }
            EditorUtility.SetDirty(ui);
        }

        private void AssignTextToUI(BodyTrackingUI ui, string textType, TMPro.TextMeshProUGUI text)
        {
            switch (textType)
            {
                case "Status Text":
                    ui.statusText = text;
                    break;
                case "Mode Text":
                    ui.modeText = text;
                    break;
            }
            EditorUtility.SetDirty(ui);
        }

        private void AutoFixAllConnections()
        {
            var ui = FindObjectOfType<BodyTrackingUI>();
            var controller = FindObjectOfType<BodyTrackingController>();
            
            if (ui == null) return;
            
            // Assign controller
            if (ui.controller == null && controller != null)
            {
                ui.controller = controller;
            }
            
            // Auto-assign buttons by name
            if (ui.recordButton == null)
                ui.recordButton = FindObjectByName<Button>("RecordButton");
            if (ui.stopRecordButton == null)
                ui.stopRecordButton = FindObjectByName<Button>("StopRecordButton");
            if (ui.playButton == null)
                ui.playButton = FindObjectByName<Button>("PlayButton");
            if (ui.stopPlayButton == null)
                ui.stopPlayButton = FindObjectByName<Button>("StopPlayButton");
            if (ui.loadButton == null)
                ui.loadButton = FindObjectByName<Button>("LoadButton");
            
            // Auto-assign text components
            if (ui.statusText == null)
                ui.statusText = FindObjectByName<TMPro.TextMeshProUGUI>("StatusText");
            if (ui.modeText == null)
                ui.modeText = FindObjectByName<TMPro.TextMeshProUGUI>("ModeText");
            
            // Auto-assign dropdown
            if (ui.recordingsDropdown == null)
                ui.recordingsDropdown = FindObjectByName<TMPro.TMP_Dropdown>("RecordingsDropdown");
            
            EditorUtility.SetDirty(ui);
            Debug.Log("Auto-fixed all UI connections");
        }

        private void TestRecordButton()
        {
            var ui = FindObjectOfType<BodyTrackingUI>();
            if (ui != null && ui.recordButton != null)
            {
                Debug.Log("Simulating record button click...");
                ui.recordButton.onClick.Invoke();
            }
        }
    }
} 