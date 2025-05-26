using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using BodyTracking.Data;

namespace BodyTracking.AR
{
    public class ARImageTargetManager : MonoBehaviour
    {
        [Header("AR Settings")]
        public ARTrackedImageManager trackedImageManager;
        public string targetImageName = "Wall 1";
        
        [Header("Wall Configuration")]
        [SerializeField] private Vector2 realWorldWallSize = new Vector2(2f, 3f); // Default 2m x 3m wall
        [SerializeField] private bool autoScaleToImageTarget = true;
        [SerializeField] private bool maintainAspectRatio = true;
        [SerializeField] private Vector3 wallRotationOffset = new Vector3(90f, 0f, 0f); // Rotation correction - 90° X to make wall upright
        [SerializeField] private Vector3 wallPositionOffset = Vector3.zero; // Position offset from image center
        
        private ARTrackedImage currentTrackedImage;
        private bool isImageDetected = false;
        private bool contentAttachedToTarget = false;
        
        // Content management (like TrackingLogic)
        private GameObject[] contents;
        private GameObject activeContent;
        
        public event System.Action<Transform> OnImageTargetDetected;
        public event System.Action<Transform> OnImageTargetUpdated;
        public event System.Action OnImageTargetLost;
        
        public bool IsImageDetected => isImageDetected && currentTrackedImage != null && currentTrackedImage.trackingState == TrackingState.Tracking;
        public Transform ImageTargetTransform => (IsImageDetected) ? currentTrackedImage.transform : null;
        public Vector2 RealWorldWallSize => realWorldWallSize;

        void OnEnable()
        {
            // Find all child GameObjects that could be content
            contents = new GameObject[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                contents[i] = transform.GetChild(i).gameObject;
                // Initially deactivate all content
                contents[i].SetActive(false);
            }
            
            if (trackedImageManager == null)
            {
                trackedImageManager = FindFirstObjectByType<ARTrackedImageManager>();
            }
            
            if (trackedImageManager != null)
            {
                trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
                Debug.Log("[ARImageTargetManager] Subscribed to tracked images changed events");
            }
            else
            {
                Debug.LogError("[ARImageTargetManager] No ARTrackedImageManager found in scene!");
            }
        }

        void OnDisable()
        {
            if (trackedImageManager != null)
            {
                trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
            }
        }

        private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            if (contentAttachedToTarget)
            {
                // Silently ignore further tracking when content is already attached
                return; // Stop further image target searching if content is already attached
            }

            foreach (var trackedImage in eventArgs.added)
            {
                Debug.Log($"[ARImageTargetManager] Image added: {trackedImage.referenceImage.name}");
                if (TryActivateContent(trackedImage))
                    break;
            }

            foreach (var trackedImage in eventArgs.updated)
            {
                if (trackedImage.trackingState == TrackingState.Tracking)
                {
                    Debug.Log($"[ARImageTargetManager] Image updated and tracking: {trackedImage.referenceImage.name}");
                    if (TryActivateContent(trackedImage))
                        break;
                }
                else
                {
                    Debug.Log($"[ARImageTargetManager] Image updated but not tracking: {trackedImage.referenceImage.name}, state: {trackedImage.trackingState}");
                }
            }
            
            foreach (var trackedImage in eventArgs.removed)
            {
                Debug.Log($"[ARImageTargetManager] Image removed: {trackedImage.referenceImage.name}");
                HandleImageRemoved(trackedImage);
            }
        }

        private bool TryActivateContent(ARTrackedImage trackedImage)
        {
            if (contentAttachedToTarget || trackedImage == null)
                return false;

            if (trackedImage.referenceImage.name != targetImageName)
            {
                Debug.Log($"[ARImageTargetManager] Ignoring image '{trackedImage.referenceImage.name}', looking for '{targetImageName}'");
                return false;
            }

            Debug.Log($"[ARImageTargetManager] ✅ Found target image: {trackedImage.referenceImage.name}");
            
            currentTrackedImage = trackedImage;
            isImageDetected = true;
            
            // Find and activate content with matching name (like TrackingLogic does)
            activeContent = System.Array.Find(contents, content => content != null && content.name == trackedImage.referenceImage.name);
            
            if (activeContent != null)
            {
                // Attach content to the tracked image
                activeContent.transform.parent = trackedImage.transform;
                
                // Apply proper positioning and scaling
                SetupWallTransform(trackedImage, activeContent);
                
                // Activate the content
                activeContent.SetActive(true);
                
                Debug.Log($"[ARImageTargetManager] Activated content '{activeContent.name}' for image '{trackedImage.referenceImage.name}'");
                Debug.Log($"[ARImageTargetManager] Wall size: {realWorldWallSize}, Image size: {trackedImage.size}");
            }
            else
            {
                Debug.LogWarning($"[ARImageTargetManager] No content found with name '{trackedImage.referenceImage.name}'. Make sure you have a child GameObject with this exact name.");
            }
            
            contentAttachedToTarget = true;
            
            OnImageTargetDetected?.Invoke(trackedImage.transform);
            
            Debug.Log($"[ARImageTargetManager] Content successfully attached to image target at position: {trackedImage.transform.position}");
            
            return true;
        }

        /// <summary>
        /// Setup the wall transform based on image target size and real-world wall dimensions
        /// </summary>
        private void SetupWallTransform(ARTrackedImage trackedImage, GameObject wallContent)
        {
            // Start with base position and rotation
            wallContent.transform.localPosition = wallPositionOffset;
            wallContent.transform.localRotation = Quaternion.Euler(wallRotationOffset);
            
            Debug.Log($"[ARImageTargetManager] Applied wall rotation offset: {wallRotationOffset} (Euler angles)");
            Debug.Log($"[ARImageTargetManager] Applied wall position offset: {wallPositionOffset}");
            
            if (autoScaleToImageTarget)
            {
                Vector3 targetScale = CalculateWallScale(trackedImage);
                wallContent.transform.localScale = targetScale;
                
                Debug.Log($"[ARImageTargetManager] Applied wall scale: {targetScale}");
            }
            else
            {
                // Use real-world dimensions directly
                wallContent.transform.localScale = new Vector3(realWorldWallSize.x, realWorldWallSize.y, 1f);
                Debug.Log($"[ARImageTargetManager] Applied fixed wall scale: {new Vector3(realWorldWallSize.x, realWorldWallSize.y, 1f)}");
            }
            
            Debug.Log($"[ARImageTargetManager] Final wall transform - Position: {wallContent.transform.position}, Rotation: {wallContent.transform.rotation.eulerAngles}, Scale: {wallContent.transform.localScale}");
        }

        /// <summary>
        /// Calculate the appropriate scale for the wall based on image target size
        /// </summary>
        private Vector3 CalculateWallScale(ARTrackedImage trackedImage)
        {
            Vector2 imageSize = trackedImage.size;
            
            if (imageSize.x <= 0 || imageSize.y <= 0)
            {
                Debug.LogWarning("[ARImageTargetManager] Invalid image size, using default scale");
                return new Vector3(realWorldWallSize.x, realWorldWallSize.y, 1f);
            }
            
            Vector3 scale;
            
            if (maintainAspectRatio)
            {
                // Scale based on the larger dimension to ensure wall fits
                float imageAspect = imageSize.x / imageSize.y;
                float wallAspect = realWorldWallSize.x / realWorldWallSize.y;
                
                if (imageAspect > wallAspect)
                {
                    // Image is wider relative to wall - scale based on width
                    float scaleX = realWorldWallSize.x / imageSize.x;
                    scale = new Vector3(realWorldWallSize.x, realWorldWallSize.x / imageAspect, 1f);
                }
                else
                {
                    // Image is taller relative to wall - scale based on height
                    float scaleY = realWorldWallSize.y / imageSize.y;
                    scale = new Vector3(realWorldWallSize.y * imageAspect, realWorldWallSize.y, 1f);
                }
            }
            else
            {
                // Scale each dimension independently
                scale = new Vector3(realWorldWallSize.x, realWorldWallSize.y, 1f);
            }
            
            return scale;
        }

        /// <summary>
        /// Update wall size at runtime (for when users change wall dimensions)
        /// </summary>
        public void UpdateWallSize(Vector2 newWallSize)
        {
            realWorldWallSize = newWallSize;
            
            if (activeContent != null && currentTrackedImage != null)
            {
                SetupWallTransform(currentTrackedImage, activeContent);
                Debug.Log($"[ARImageTargetManager] Updated wall size to: {newWallSize}");
            }
        }

        /// <summary>
        /// Update wall rotation offset
        /// </summary>
        public void UpdateWallRotation(Vector3 rotationOffset)
        {
            wallRotationOffset = rotationOffset;
            
            if (activeContent != null)
            {
                activeContent.transform.localRotation = Quaternion.Euler(wallRotationOffset);
                Debug.Log($"[ARImageTargetManager] Updated wall rotation to: {rotationOffset}");
            }
        }

        /// <summary>
        /// Update wall position offset
        /// </summary>
        public void UpdateWallPosition(Vector3 positionOffset)
        {
            wallPositionOffset = positionOffset;
            
            if (activeContent != null)
            {
                activeContent.transform.localPosition = wallPositionOffset;
                Debug.Log($"[ARImageTargetManager] Updated wall position to: {positionOffset}");
            }
        }

        private void HandleImageRemoved(ARTrackedImage trackedImage)
        {
            if (trackedImage.referenceImage.name != targetImageName) return;
            
            currentTrackedImage = null;
            isImageDetected = false;
            contentAttachedToTarget = false;
            
            // Deactivate and detach content
            if (activeContent != null)
            {
                activeContent.SetActive(false);
                activeContent.transform.parent = this.transform; // Return to original parent
                activeContent = null;
            }
            
            OnImageTargetLost?.Invoke();
            Debug.Log("[ARImageTargetManager] Image target lost, content detached");
        }

        void Update()
        {
            // Update content position if we have a tracked image and active content
            if (currentTrackedImage != null && activeContent != null)
            {
                if (currentTrackedImage.trackingState == TrackingState.Tracking)
                {
                    OnImageTargetUpdated?.Invoke(currentTrackedImage.transform);
                }
            }
        }

        public CoordinateFrame GetCurrentCoordinateFrame()
        {
            if (IsImageDetected)
            {
                return new CoordinateFrame(currentTrackedImage.transform);
            }
            return default;
        }

        // Public methods for debugging
        public void ToggleContentVisibility()
        {
            if (activeContent != null)
            {
                activeContent.SetActive(!activeContent.activeSelf);
                Debug.Log($"[ARImageTargetManager] Content visibility toggled: {activeContent.activeSelf}");
            }
        }

        public void ListAvailableContent()
        {
            Debug.Log($"[ARImageTargetManager] Available content objects:");
            for (int i = 0; i < contents.Length; i++)
            {
                if (contents[i] != null)
                {
                    Debug.Log($"  - {contents[i].name} (active: {contents[i].activeSelf})");
                }
            }
        }
    }
} 