using UnityEngine;
using UnityEditor;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace BodyTracking.Editor
{
    public class BodyTrackingDebug : EditorWindow
    {
        [MenuItem("Tools/Body Tracking/Debug Body Detection")]
        public static void ShowWindow()
        {
            GetWindow<BodyTrackingDebug>("Body Tracking Debug");
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Body Tracking Debug", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to debug body tracking", MessageType.Info);
                return;
            }
            
            var humanBodyManager = FindObjectOfType<ARHumanBodyManager>();
            
            if (humanBodyManager == null)
            {
                EditorGUILayout.HelpBox("No ARHumanBodyManager found in scene!", MessageType.Error);
                return;
            }
            
            // Human Body Manager Status
            EditorGUILayout.LabelField("ARHumanBodyManager Status:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Enabled: {humanBodyManager.enabled}");
            EditorGUILayout.LabelField($"Tracked Bodies Count: {humanBodyManager.trackables.count}");
            EditorGUILayout.Space();
            
            // Check for tracked bodies
            if (humanBodyManager.trackables.count > 0)
            {
                EditorGUILayout.LabelField("Tracked Bodies:", EditorStyles.boldLabel);
                
                int bodyIndex = 0;
                foreach (var humanBody in humanBodyManager.trackables)
                {
                    EditorGUILayout.LabelField($"Body {bodyIndex}:");
                    EditorGUILayout.LabelField($"  Tracking State: {humanBody.trackingState}");
                    EditorGUILayout.LabelField($"  Pose Position: {humanBody.pose.position}");
                    EditorGUILayout.LabelField($"  Pose Rotation: {humanBody.pose.rotation}");
                    
                    // Check joints availability
                    var joints = humanBody.joints;
                    if (joints.IsCreated && joints.Length > 0)
                    {
                        EditorGUILayout.LabelField($"  Total Joints: {joints.Length}");
                        
                        // Find tracked joints with actual position data
                        int trackedCount = 0;
                        int validCount = 0;
                        int firstTrackedIndex = -1;
                        int firstValidIndex = -1;
                        int selectedJointIndex = -1; // The joint our tracking logic would actually use
                        
                        for (int i = 0; i < joints.Length; i++)
                        {
                            if (joints[i].tracked)
                            {
                                trackedCount++;
                                if (firstTrackedIndex == -1)
                                    firstTrackedIndex = i;
                                
                                // Check if it also has non-zero position
                                if (joints[i].localPose.position != Vector3.zero)
                                {
                                    validCount++;
                                    if (firstValidIndex == -1)
                                        firstValidIndex = i;
                                }
                            }
                        }
                        
                        // Determine which joint our tracking logic would actually select
                        if (joints.Length > 2 && joints[2].tracked && joints[2].localPose.position != Vector3.zero)
                        {
                            selectedJointIndex = 2; // Preferred hip joint
                        }
                        else if (firstValidIndex >= 0)
                        {
                            selectedJointIndex = firstValidIndex; // Fallback
                        }
                        
                        EditorGUILayout.LabelField($"  Tracked Joints: {trackedCount}");
                        EditorGUILayout.LabelField($"  Joints with Position Data: {validCount}");
                        
                        if (firstTrackedIndex >= 0)
                        {
                            var trackedJoint = joints[firstTrackedIndex];
                            EditorGUILayout.LabelField($"  First Tracked Joint: #{firstTrackedIndex}");
                            EditorGUILayout.LabelField($"  Its Position: {trackedJoint.localPose.position}");
                        }
                        
                        if (firstValidIndex >= 0)
                        {
                            var validJoint = joints[firstValidIndex];
                            EditorGUILayout.LabelField($"  First Valid Joint: #{firstValidIndex}");
                            EditorGUILayout.LabelField($"  Its Position: {validJoint.localPose.position}");
                        }
                        
                        if (selectedJointIndex >= 0)
                        {
                            var selectedJoint = joints[selectedJointIndex];
                            string jointType = selectedJointIndex == 2 ? " (HIP)" : " (FALLBACK)";
                            EditorGUILayout.LabelField($"  🎯 TRACKING JOINT: #{selectedJointIndex}{jointType}");
                            EditorGUILayout.LabelField($"  Its Position: {selectedJoint.localPose.position}");
                        }
                        else if (trackedCount > 0)
                        {
                            EditorGUILayout.LabelField($"  ⚠️ All tracked joints have (0,0,0) position!");
                        }
                        else
                        {
                            EditorGUILayout.LabelField($"  ⚠️ No joints are tracked!");
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"  Joints: No joint data available");
                    }
                    
                    bodyIndex++;
                    EditorGUILayout.Space();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("❌ No bodies detected", MessageType.Warning);
                
                // Troubleshooting tips
                EditorGUILayout.LabelField("Troubleshooting Tips:", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("• Make sure you're visible in camera frame\n" +
                    "• Ensure good lighting\n" +
                    "• Try facing camera directly (not through mirror)\n" +
                    "• Make sure device supports body tracking\n" +
                    "• Check AR Remote connection\n" +
                    "• Stand further back to get full body in view", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // AR Session info
            var arSession = FindObjectOfType<ARSession>();
            if (arSession != null)
            {
                EditorGUILayout.LabelField("AR Session Status:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"State: {ARSession.state}");
                EditorGUILayout.LabelField($"Not Tracking Reason: {ARSession.notTrackingReason}");
            }
            
            EditorGUILayout.Space();
            
            // Quick actions
            EditorGUILayout.LabelField("Quick Actions:", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Reset AR Session"))
            {
                if (arSession != null)
                {
                    arSession.Reset();
                    Debug.Log("AR Session reset - try body tracking again");
                }
            }
            
            if (GUILayout.Button("Log Body Tracking Info"))
            {
                LogDetailedBodyInfo();
            }
            
            if (GUILayout.Button("Test Without Mirror"))
            {
                EditorUtility.DisplayDialog("Mirror Test", 
                    "Turn your AR Remote camera to face you directly (no mirror).\n\n" +
                    "This is the most common fix for body tracking issues!", "OK");
            }
            
            if (GUILayout.Button("Create Test Sphere"))
            {
                CreateTestSphere();
            }
            
            if (GUILayout.Button("Enable Recorder Debug & Visualization"))
            {
                EnableRecorderDebug();
            }
            
            if (GUILayout.Button("Debug Scene Objects"))
            {
                DebugSceneObjects();
            }
            
            if (GUILayout.Button("Create Simple Cube (Test)"))
            {
                CreateSimpleCube();
            }
            
            if (GUILayout.Button("Clear Test Objects"))
            {
                ClearTestObjects();
            }
            
            // Auto-refresh
            if (GUILayout.Toggle(true, "Auto-refresh"))
            {
                Repaint();
            }
        }

        private void LogDetailedBodyInfo()
        {
            Debug.Log("=== Detailed Body Tracking Info ===");
            
            var humanBodyManager = FindObjectOfType<ARHumanBodyManager>();
            if (humanBodyManager == null)
            {
                Debug.LogError("No ARHumanBodyManager found");
                return;
            }
            
            Debug.Log($"Human Body Manager - Enabled: {humanBodyManager.enabled}");
            Debug.Log($"Tracked bodies count: {humanBodyManager.trackables.count}");
            
            if (humanBodyManager.trackables.count == 0)
            {
                Debug.LogWarning("No bodies currently tracked");
                Debug.Log("Common fixes:");
                Debug.Log("1. Stop using mirror - face camera directly");
                Debug.Log("2. Ensure good, even lighting");
                Debug.Log("3. Make sure full body is visible in frame");
                Debug.Log("4. Try different angles/distances");
                Debug.Log("5. Check AR Remote connection");
                return;
            }
            
            int bodyIndex = 0;
            foreach (var humanBody in humanBodyManager.trackables)
            {
                Debug.Log($"Body {bodyIndex}:");
                Debug.Log($"  Tracking state: {humanBody.trackingState}");
                Debug.Log($"  Body pose position: {humanBody.pose.position}");
                Debug.Log($"  Body pose rotation: {humanBody.pose.rotation}");
                
                // Log basic joint info
                var joints = humanBody.joints;
                if (joints.IsCreated && joints.Length > 0)
                {
                    Debug.Log($"  Available joints: {joints.Length}");
                    
                    // Just log first few joints to avoid spam
                    int maxJointsToLog = Mathf.Min(5, joints.Length);
                    for (int i = 0; i < maxJointsToLog; i++)
                    {
                        var joint = joints[i];
                        Debug.Log($"    Joint {i}: tracked={joint.tracked}, pos={joint.localPose.position}");
                    }
                    
                    if (joints.Length > 5)
                    {
                        Debug.Log($"    ... and {joints.Length - 5} more joints");
                    }
                }
                else
                {
                    Debug.Log("  No joint data available");
                }
                
                bodyIndex++;
            }
        }

        private void CreateTestSphere()
        {
            // Find AR Session Origin (where AR objects should be parented)
            var arSessionOrigin = FindObjectOfType<UnityEngine.XR.ARFoundation.ARSessionOrigin>();
            var arCamera = Camera.main ?? FindObjectOfType<Camera>();
            if (arCamera == null)
            {
                Debug.LogError("No camera found for test sphere positioning");
                return;
            }
            
            // Create test sphere 1 meter in front of camera
            Vector3 testPosition = arCamera.transform.position + arCamera.transform.forward * 1.0f;
            
            var testSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            testSphere.name = "TestSphere_AR_Debug";
            testSphere.transform.position = testPosition;
            testSphere.transform.localScale = Vector3.one * 0.3f;
            
            // Parent to AR Session Origin for proper AR sync
            if (arSessionOrigin != null)
            {
                testSphere.transform.SetParent(arSessionOrigin.transform);
                Debug.Log($"Parented sphere to AR Session Origin: {arSessionOrigin.name}");
            }
            
            // Make it bright red and very visible
            var renderer = testSphere.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Use Unlit shader which works better in AR
                var material = new Material(Shader.Find("Unlit/Color"));
                material.color = Color.red;
                renderer.material = material;
                Debug.Log($"Applied Unlit/Color material to sphere");
            }
            else
            {
                Debug.LogError("Failed to get renderer component on test sphere!");
            }
            
            // Remove collider
            var collider = testSphere.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }
            
            Debug.Log($"✅ Created PERSISTENT test sphere at: {testPosition:F3}");
            Debug.Log($"Camera position: {arCamera.transform.position:F3}");
            Debug.Log($"Camera forward: {arCamera.transform.forward:F3}");
            Debug.Log($"Sphere will stay visible until manually destroyed");
            
            // DON'T auto-destroy - let it stay visible for testing
        }

        private void EnableRecorderDebug()
        {
            var recorder = FindObjectOfType<BodyTracking.Recording.BodyTrackingRecorder>();
            if (recorder != null)
            {
                // Use reflection to set private fields since they're not public
                var recorderType = recorder.GetType();
                
                var debugModeField = recorderType.GetField("debugMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var showVisualizationField = recorderType.GetField("showVisualization", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (debugModeField != null)
                {
                    debugModeField.SetValue(recorder, true);
                    Debug.Log("✅ Enabled debug mode on BodyTrackingRecorder");
                }
                
                if (showVisualizationField != null)
                {
                    showVisualizationField.SetValue(recorder, true);
                    Debug.Log("✅ Enabled visualization on BodyTrackingRecorder");
                }
                
                Debug.Log("BodyTrackingRecorder debug settings updated!");
            }
            else
            {
                Debug.LogWarning("No BodyTrackingRecorder found in scene");
            }
        }

        private void DebugSceneObjects()
        {
            Debug.Log("=== SCENE OBJECTS DEBUG ===");
            
            // Find all spheres in scene
            var allObjects = FindObjectsOfType<GameObject>();
            var spheres = new System.Collections.Generic.List<GameObject>();
            
            foreach (var obj in allObjects)
            {
                if (obj.name.Contains("Sphere") || obj.name.Contains("sphere"))
                {
                    spheres.Add(obj);
                }
            }
            
            Debug.Log($"Found {spheres.Count} sphere objects in scene:");
            
            foreach (var sphere in spheres)
            {
                Debug.Log($"--- SPHERE: {sphere.name} ---");
                Debug.Log($"  Position: {sphere.transform.position:F3}");
                Debug.Log($"  Scale: {sphere.transform.localScale:F3}");
                Debug.Log($"  Active: {sphere.activeInHierarchy}");
                Debug.Log($"  Layer: {LayerMask.LayerToName(sphere.layer)} ({sphere.layer})");
                
                var renderer = sphere.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Debug.Log($"  Renderer enabled: {renderer.enabled}");
                    Debug.Log($"  Material: {renderer.material?.name ?? "NULL"}");
                    Debug.Log($"  Shader: {renderer.material?.shader?.name ?? "NULL"}");
                    Debug.Log($"  Visible: {renderer.isVisible}");
                }
                else
                {
                    Debug.Log($"  NO RENDERER COMPONENT!");
                }
            }
            
            // Check camera info
            var arCamera = Camera.main ?? FindObjectOfType<Camera>();
            if (arCamera != null)
            {
                Debug.Log($"--- AR CAMERA INFO ---");
                Debug.Log($"  Position: {arCamera.transform.position:F3}");
                Debug.Log($"  Forward: {arCamera.transform.forward:F3}");
                Debug.Log($"  Culling Mask: {arCamera.cullingMask}");
                Debug.Log($"  Clear Flags: {arCamera.clearFlags}");
                Debug.Log($"  Depth: {arCamera.depth}");
                Debug.Log($"  Enabled: {arCamera.enabled}");
            }
            else
            {
                Debug.LogError("NO CAMERA FOUND!");
            }
        }

        private void CreateSimpleCube()
        {
            // Find AR camera
            var arCamera = Camera.main ?? FindObjectOfType<Camera>();
            if (arCamera == null)
            {
                Debug.LogError("No camera found for simple cube positioning");
                return;
            }
            
            // Create simple cube 2 meters in front of camera
            Vector3 testPosition = arCamera.transform.position + arCamera.transform.forward * 2.0f;
            
            var testCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testCube.name = "TestCube_DebugTool";
            testCube.transform.position = testPosition;
            testCube.transform.localScale = Vector3.one * 0.5f;
            
            // Make it bright green for visibility
            var renderer = testCube.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Standard"));
                material.color = Color.green;
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.green * 5f);
                renderer.material = material;
            }
            
            // Remove collider
            if (testCube.TryGetComponent<Collider>(out var collider))
            {
                Destroy(collider);
            }
            
            Debug.Log($"Created simple cube at: {testPosition:F3}");
            Debug.Log($"Camera position: {arCamera.transform.position:F3}");
            Debug.Log($"Camera forward: {arCamera.transform.forward:F3}");
            
            // Auto-destroy after 10 seconds
            if (Application.isPlaying)
            {
                DestroyImmediate(testCube, false);
                var tempObj = new GameObject("TempCubeDestroyer");
                var destroyer = tempObj.AddComponent<TestCubeDestroyer>();
                destroyer.cubeToDestroy = testCube;
                destroyer.destroyAfterSeconds = 10f;
            }
        }

        private void ClearTestObjects()
        {
            var allObjects = FindObjectsOfType<GameObject>();
            int clearedCount = 0;
            
            foreach (var obj in allObjects)
            {
                if (obj.name.Contains("Test") && (obj.name.Contains("Sphere") || obj.name.Contains("Cube")))
                {
                    Debug.Log($"Destroying test object: {obj.name}");
                    DestroyImmediate(obj);
                    clearedCount++;
                }
            }
            
            Debug.Log($"✅ Cleared {clearedCount} test objects from scene");
        }
    }
    
    // Helper component to destroy test sphere after delay
    public class TestSphereDestroyer : MonoBehaviour
    {
        public GameObject sphereToDestroy;
        public float destroyAfterSeconds = 10f;
        
        void Start()
        {
            if (sphereToDestroy != null)
            {
                Destroy(sphereToDestroy, destroyAfterSeconds);
            }
            Destroy(gameObject, destroyAfterSeconds + 0.1f);
        }
    }

    // Helper component to destroy test cube after delay
    public class TestCubeDestroyer : MonoBehaviour
    {
        public GameObject cubeToDestroy;
        public float destroyAfterSeconds = 10f;
        
        void Start()
        {
            if (cubeToDestroy != null)
            {
                Destroy(cubeToDestroy, destroyAfterSeconds);
            }
            Destroy(gameObject, destroyAfterSeconds + 0.1f);
        }
    }
} 