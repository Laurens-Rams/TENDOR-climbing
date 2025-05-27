using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TENDOR.Core;
using Logger = TENDOR.Core.Logger;

namespace TENDOR.Testing
{
    /// <summary>
    /// Provides visual feedback for AR image tracking and body tracking
    /// </summary>
    public class ImageTrackingVisualizer : MonoBehaviour
    {
        [Header("Image Tracking Visualization")]
        [SerializeField] private Material trackedImageMaterial;
        [SerializeField] private bool showImageOverlay = true;
        [SerializeField] private bool logTrackingEvents = true;
        
        [Header("Body Tracking Visualization")]
        [SerializeField] private bool showHipSpheres = true;
        [SerializeField] private Material hipSphereMaterial;
        [SerializeField] private float sphereSize = 0.05f;
        
        private ARTrackedImageManager trackedImageManager;
        private ARHumanBodyManager humanBodyManager;
        private System.Collections.Generic.Dictionary<ARTrackedImage, GameObject> imageOverlays = 
            new System.Collections.Generic.Dictionary<ARTrackedImage, GameObject>();
        private System.Collections.Generic.Dictionary<ARHumanBody, GameObject[]> hipSpheres = 
            new System.Collections.Generic.Dictionary<ARHumanBody, GameObject[]>();
        
        private void Start()
        {
            // Find AR managers
            trackedImageManager = FindFirstObjectByType<ARTrackedImageManager>();
            humanBodyManager = FindFirstObjectByType<ARHumanBodyManager>();
            
            // Create default materials if not assigned
            CreateDefaultMaterials();
            
            // Subscribe to events
            if (trackedImageManager != null)
            {
                trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
                Logger.Log("📱 Image Tracking Visualizer initialized", "VISUALIZER");
            }
            
            if (humanBodyManager != null)
            {
                humanBodyManager.humanBodiesChanged += OnHumanBodiesChanged;
                Logger.Log("🏃 Body Tracking Visualizer initialized", "VISUALIZER");
            }
        }
        
        private void CreateDefaultMaterials()
        {
            if (trackedImageMaterial == null)
            {
                trackedImageMaterial = new Material(Shader.Find("Sprites/Default"));
                trackedImageMaterial.color = new Color(0, 1, 0, 0.5f); // Semi-transparent green
            }
            
            if (hipSphereMaterial == null)
            {
                hipSphereMaterial = new Material(Shader.Find("Standard"));
                hipSphereMaterial.color = Color.red;
                hipSphereMaterial.SetFloat("_Metallic", 0.5f);
                hipSphereMaterial.SetFloat("_Smoothness", 0.8f);
            }
        }
        
        private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            // Handle newly detected images
            foreach (var trackedImage in eventArgs.added)
            {
                if (showImageOverlay)
                {
                    CreateImageOverlay(trackedImage);
                }
                
                if (logTrackingEvents)
                {
                    Logger.Log($"🎯 Image detected: {trackedImage.referenceImage.name}", "VISUALIZER");
                }
            }
            
            // Handle updated images
            foreach (var trackedImage in eventArgs.updated)
            {
                UpdateImageOverlay(trackedImage);
                
                if (logTrackingEvents)
                {
                    var state = trackedImage.trackingState;
                    var stateEmoji = state == TrackingState.Tracking ? "🟢" : 
                                   state == TrackingState.Limited ? "🟡" : "🔴";
                    Logger.Log($"{stateEmoji} Image {trackedImage.referenceImage.name}: {state}", "VISUALIZER");
                }
            }
            
            // Handle removed images
            foreach (var trackedImage in eventArgs.removed)
            {
                RemoveImageOverlay(trackedImage);
                
                if (logTrackingEvents)
                {
                    Logger.Log($"❌ Image lost: {trackedImage.referenceImage.name}", "VISUALIZER");
                }
            }
        }
        
        private void CreateImageOverlay(ARTrackedImage trackedImage)
        {
            // Create a quad to overlay on the tracked image
            var overlayGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
            overlayGO.name = $"ImageOverlay_{trackedImage.referenceImage.name}";
            
            // Remove collider as we don't need it
            Destroy(overlayGO.GetComponent<Collider>());
            
            // Set material
            var renderer = overlayGO.GetComponent<Renderer>();
            renderer.material = trackedImageMaterial;
            
            // Set size to match the reference image
            var imageSize = trackedImage.referenceImage.size;
            overlayGO.transform.localScale = new Vector3(imageSize.x, imageSize.y, 1);
            
            // Parent to the tracked image
            overlayGO.transform.SetParent(trackedImage.transform, false);
            overlayGO.transform.localPosition = Vector3.zero;
            overlayGO.transform.localRotation = Quaternion.identity;
            
            // Add text label
            CreateImageLabel(overlayGO, trackedImage.referenceImage.name);
            
            // Store reference
            imageOverlays[trackedImage] = overlayGO;
            
            Logger.Log($"✅ Created overlay for {trackedImage.referenceImage.name} ({imageSize.x:F2}x{imageSize.y:F2}m)", "VISUALIZER");
        }
        
        private void CreateImageLabel(GameObject parent, string imageName)
        {
            // Create a 3D text label
            var labelGO = new GameObject($"Label_{imageName}");
            labelGO.transform.SetParent(parent.transform, false);
            labelGO.transform.localPosition = new Vector3(0, 0, -0.01f); // Slightly in front
            
            var textMesh = labelGO.AddComponent<TextMesh>();
            textMesh.text = imageName;
            textMesh.fontSize = 20;
            textMesh.color = Color.white;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            
            // Scale text appropriately
            labelGO.transform.localScale = Vector3.one * 0.1f;
        }
        
        private void UpdateImageOverlay(ARTrackedImage trackedImage)
        {
            if (imageOverlays.TryGetValue(trackedImage, out var overlay))
            {
                // Update visibility based on tracking state
                var isTracking = trackedImage.trackingState == TrackingState.Tracking;
                overlay.SetActive(isTracking);
                
                // Update material color based on tracking quality
                var renderer = overlay.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var color = trackedImage.trackingState == TrackingState.Tracking ? 
                               new Color(0, 1, 0, 0.5f) : // Green when tracking
                               new Color(1, 1, 0, 0.3f);  // Yellow when limited
                    renderer.material.color = color;
                }
            }
        }
        
        private void RemoveImageOverlay(ARTrackedImage trackedImage)
        {
            if (imageOverlays.TryGetValue(trackedImage, out var overlay))
            {
                Destroy(overlay);
                imageOverlays.Remove(trackedImage);
            }
        }
        
        private void OnHumanBodiesChanged(ARHumanBodiesChangedEventArgs eventArgs)
        {
            // Handle newly detected bodies
            foreach (var humanBody in eventArgs.added)
            {
                if (showHipSpheres)
                {
                    CreateHipSpheres(humanBody);
                }
                
                if (logTrackingEvents)
                {
                    Logger.Log($"🏃 Body detected: {humanBody.trackableId}", "VISUALIZER");
                }
            }
            
            // Handle updated bodies
            foreach (var humanBody in eventArgs.updated)
            {
                UpdateHipSpheres(humanBody);
            }
            
            // Handle removed bodies
            foreach (var humanBody in eventArgs.removed)
            {
                RemoveHipSpheres(humanBody);
                
                if (logTrackingEvents)
                {
                    Logger.Log($"❌ Body lost: {humanBody.trackableId}", "VISUALIZER");
                }
            }
        }
        
        private void CreateHipSpheres(ARHumanBody humanBody)
        {
            // Create spheres for left and right hip
            var spheres = new GameObject[2];
            
            // Left hip sphere
            spheres[0] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spheres[0].name = "LeftHipSphere";
            spheres[0].transform.localScale = Vector3.one * sphereSize;
            spheres[0].GetComponent<Renderer>().material = hipSphereMaterial;
            Destroy(spheres[0].GetComponent<Collider>());
            
            // Right hip sphere
            spheres[1] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spheres[1].name = "RightHipSphere";
            spheres[1].transform.localScale = Vector3.one * sphereSize;
            spheres[1].GetComponent<Renderer>().material = hipSphereMaterial;
            Destroy(spheres[1].GetComponent<Collider>());
            
            // Store reference
            hipSpheres[humanBody] = spheres;
            
            Logger.Log($"✅ Created hip spheres for body {humanBody.trackableId}", "VISUALIZER");
        }
        
        private void UpdateHipSpheres(ARHumanBody humanBody)
        {
            if (!hipSpheres.TryGetValue(humanBody, out var spheres)) return;
            
            // Get the joints data
            var joints = humanBody.joints;
            
            if (joints.IsCreated && joints.Length > 0)
            {
                // Update sphere positions based on hip joints
                // Left hip is typically joint 11, right hip is joint 12 in ARKit
                var leftHipIndex = 11;
                var rightHipIndex = 12;
                
                if (leftHipIndex < joints.Length && rightHipIndex < joints.Length)
                {
                    var leftHipJoint = joints[leftHipIndex];
                    var rightHipJoint = joints[rightHipIndex];
                    
                    // Transform to world space
                    var leftHipWorld = humanBody.transform.TransformPoint(leftHipJoint.localPose.position);
                    var rightHipWorld = humanBody.transform.TransformPoint(rightHipJoint.localPose.position);
                    
                    spheres[0].transform.position = leftHipWorld;
                    spheres[1].transform.position = rightHipWorld;
                    
                    // Show/hide based on tracking confidence
                    var isTracking = humanBody.trackingState == TrackingState.Tracking;
                    spheres[0].SetActive(isTracking);
                    spheres[1].SetActive(isTracking);
                    
                    if (logTrackingEvents && Time.frameCount % 30 == 0) // Log every 30 frames
                    {
                        Logger.Log($"🎯 Hip positions - L: {leftHipWorld:F2}, R: {rightHipWorld:F2}", "VISUALIZER");
                    }
                }
            }
        }
        
        private void RemoveHipSpheres(ARHumanBody humanBody)
        {
            if (hipSpheres.TryGetValue(humanBody, out var spheres))
            {
                foreach (var sphere in spheres)
                {
                    if (sphere != null) Destroy(sphere);
                }
                hipSpheres.Remove(humanBody);
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (trackedImageManager != null)
            {
                trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
            }
            
            if (humanBodyManager != null)
            {
                humanBodyManager.humanBodiesChanged -= OnHumanBodiesChanged;
            }
            
            // Clean up overlays
            foreach (var overlay in imageOverlays.Values)
            {
                if (overlay != null) Destroy(overlay);
            }
            
            // Clean up spheres
            foreach (var sphereArray in hipSpheres.Values)
            {
                foreach (var sphere in sphereArray)
                {
                    if (sphere != null) Destroy(sphere);
                }
            }
        }
        
        // Public methods for runtime control
        public void ToggleImageOverlay()
        {
            showImageOverlay = !showImageOverlay;
            Logger.Log($"📱 Image overlay: {(showImageOverlay ? "ON" : "OFF")}", "VISUALIZER");
        }
        
        public void ToggleHipSpheres()
        {
            showHipSpheres = !showHipSpheres;
            Logger.Log($"🏃 Hip spheres: {(showHipSpheres ? "ON" : "OFF")}", "VISUALIZER");
        }
    }
} 