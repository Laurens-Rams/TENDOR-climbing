using UnityEngine;
using UnityEditor;

namespace BodyTracking.Editor
{
    /// <summary>
    /// Simple video recording setup for TENDOR system
    /// </summary>
    public class SimpleVideoSetup : EditorWindow
    {
        [MenuItem("TENDOR/Setup Video Recording (Simple)")]
        public static void SetupVideoRecording()
        {
            Debug.Log("🎬 SETTING UP SYNCHRONIZED VIDEO RECORDING");
            
            // Find or create video recorder
            var videoRecorderType = System.Type.GetType("BodyTracking.Recording.SynchronizedVideoRecorder");
            if (videoRecorderType != null)
            {
                var videoRecorder = FindFirstObjectByType(videoRecorderType);
                if (videoRecorder == null)
                {
                    // Create new GameObject for video recorder
                    var videoRecorderGO = new GameObject("SynchronizedVideoRecorder");
                    videoRecorder = videoRecorderGO.AddComponent(videoRecorderType);
                    Debug.Log("✅ Created SynchronizedVideoRecorder");
                }
                else
                {
                    Debug.Log("✅ SynchronizedVideoRecorder already exists");
                }
                
                // Connect to BodyTrackingController
                var controllerType = System.Type.GetType("BodyTracking.BodyTrackingController");
                if (controllerType != null)
                {
                    var controller = FindFirstObjectByType(controllerType);
                    if (controller != null)
                    {
                        // Use reflection to set the videoRecorder field
                        var field = controllerType.GetField("videoRecorder");
                        if (field != null)
                        {
                            field.SetValue(controller, videoRecorder);
                            Debug.Log("✅ Connected video recorder to BodyTrackingController");
                        }
                        
                        // Enable video recording
                        var enableField = controllerType.GetField("enableVideoRecording");
                        if (enableField != null)
                        {
                            enableField.SetValue(controller, true);
                            Debug.Log("✅ Enabled video recording in controller");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ BodyTrackingController not found in scene");
                    }
                }
                
                Debug.Log("🎉 Video recording setup complete!");
            }
            else
            {
                Debug.LogError("❌ SynchronizedVideoRecorder type not found");
            }
        }

        [MenuItem("TENDOR/Create Wall 1 Content (Simple)")]
        public static void CreateWall1Content()
        {
            Debug.Log("🧱 CREATING WALL 1 CONTENT FOR IMAGE TARGET");
            
            // Find ARImageTargetManager
            var imageTargetManagerType = System.Type.GetType("BodyTracking.AR.ARImageTargetManager");
            if (imageTargetManagerType != null)
            {
                var imageTargetManager = FindFirstObjectByType(imageTargetManagerType);
                if (imageTargetManager != null)
                {
                    var managerGO = ((Component)imageTargetManager).gameObject;
                    
                    // Check if "Wall 1" content already exists
                    var existingContent = managerGO.transform.Find("Wall 1");
                    if (existingContent != null)
                    {
                        Debug.Log("✅ Wall 1 content already exists");
                        return;
                    }
                    
                    // Create "Wall 1" content GameObject
                    var wall1Content = new GameObject("Wall 1");
                    wall1Content.transform.SetParent(managerGO.transform);
                    wall1Content.transform.localPosition = Vector3.zero;
                    wall1Content.transform.localRotation = Quaternion.identity;
                    wall1Content.SetActive(false); // Initially inactive
                    
                    // Add a simple cube as visual content
                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.SetParent(wall1Content.transform);
                    cube.transform.localPosition = Vector3.zero;
                    cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    cube.name = "Wall Indicator";
                    
                    // Make it green to indicate successful tracking
                    var renderer = cube.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = Color.green;
                    }
                    
                    Debug.Log("✅ Created Wall 1 content with visual indicator");
                    Debug.Log("📍 Content will appear when 'Wall 1' image target is detected");
                    
                    // Mark scene as dirty
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                        UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                }
                else
                {
                    Debug.LogError("❌ ARImageTargetManager not found. Make sure the scene has the TENDOR system set up.");
                }
            }
            else
            {
                Debug.LogError("❌ ARImageTargetManager type not found");
            }
        }
    }
} 