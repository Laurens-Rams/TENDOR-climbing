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
                trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
            }
            
            if (trackedImageManager != null)
            {
                trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
                Debug.Log("[ARImageTargetManager] Subscribed to tracked images changed events");
            }
            else
            {
                Debug.LogError("[ARImageTargetManager] ARTrackedImageManager not found!");
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
                // Attach content to the tracked image (like TrackingLogic)
                activeContent.transform.parent = trackedImage.transform;
                activeContent.transform.localPosition = Vector3.zero;
                activeContent.transform.localRotation = Quaternion.identity;
                
                // Activate the content
                activeContent.SetActive(true);
                
                Debug.Log($"[ARImageTargetManager] Activated content '{activeContent.name}' for image '{trackedImage.referenceImage.name}'");
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