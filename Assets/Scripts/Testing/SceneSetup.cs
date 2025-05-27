using UnityEditor;
using UnityEngine;
using TENDOR.Testing;
using TENDOR.Recording;

namespace TENDOR.Testing
{
    public class SceneSetup
    {
        [MenuItem("TENDOR/Setup/Add Image Tracking Visualizer")]
        public static void AddImageTrackingVisualizer()
        {
            var existingVisualizer = UnityEngine.Object.FindFirstObjectByType<ImageTrackingVisualizer>();
            if (existingVisualizer != null)
            {
                Debug.Log("✅ Image Tracking Visualizer already exists");
                return;
            }
            
            // Find or create a GameObject for the visualizer
            var servicesGO = GameObject.Find("Services");
            if (servicesGO == null)
            {
                servicesGO = new GameObject("Services");
            }
            
            var visualizer = servicesGO.AddComponent<ImageTrackingVisualizer>();
            
            Debug.Log("✅ Added Image Tracking Visualizer to scene");
            Debug.Log("💡 This will show visual indicators when images are tracked");
        }
        
        [MenuItem("TENDOR/Setup/Add AR Remote Test Manager")]
        public static void AddARRemoteTestManager()
        {
            var existingManager = UnityEngine.Object.FindFirstObjectByType<ARRemoteTestManager>();
            if (existingManager != null)
            {
                Debug.Log("✅ AR Remote Test Manager already exists");
                return;
            }
            
            // Find or create a GameObject for the manager
            var testingGO = GameObject.Find("Testing");
            if (testingGO == null)
            {
                testingGO = new GameObject("Testing");
            }
            
            var manager = testingGO.AddComponent<ARRemoteTestManager>();
            
            Debug.Log("✅ Added AR Remote Test Manager to scene");
            Debug.Log("💡 Press D to toggle debug UI, R to record, S to stop, P to play");
        }
        
        [MenuItem("TENDOR/Setup/Add Body Tracking Controller")]
        public static void AddBodyTrackingController()
        {
            var existingController = UnityEngine.Object.FindFirstObjectByType<BodyTrackingController>();
            if (existingController != null)
            {
                Debug.Log("✅ Body Tracking Controller already exists");
                return;
            }
            
            // Find or create a GameObject for the controller
            var bodyTrackingGO = GameObject.Find("BodyTrackingSystem");
            if (bodyTrackingGO == null)
            {
                bodyTrackingGO = new GameObject("BodyTrackingSystem");
            }
            
            // Add the controller and its components
            var controller = bodyTrackingGO.AddComponent<BodyTrackingController>();
            var recorder = bodyTrackingGO.AddComponent<BodyTrackingRecorder>();
            var player = bodyTrackingGO.AddComponent<BodyTrackingPlayer>();
            
            Debug.Log("✅ Added Body Tracking Controller to scene");
            Debug.Log("💡 This handles recording and playback of hip tracking data");
        }
        
        [MenuItem("TENDOR/Setup/Complete Scene Setup")]
        public static void CompleteSceneSetup()
        {
            AddImageTrackingVisualizer();
            AddARRemoteTestManager();
            AddBodyTrackingController();
            
            Debug.Log("✅ Complete scene setup finished!");
            Debug.Log("💡 Your scene now has visual tracking indicators, debug controls, and body tracking");
        }
    }
} 