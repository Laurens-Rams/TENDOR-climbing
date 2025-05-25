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
        public string targetImageName = "TENDORImage";
        
        [Header("Wall Visualization")]
        public GameObject wallPrefab;
        public Vector3 wallRotationOffset = new Vector3(0, 0, -90);
        
        private ARTrackedImage currentTrackedImage;
        private GameObject wallInstance;
        private bool isImageDetected = false;
        
        public event System.Action<Transform> OnImageTargetDetected;
        public event System.Action<Transform> OnImageTargetUpdated;
        public event System.Action OnImageTargetLost;
        
        public bool IsImageDetected => isImageDetected && currentTrackedImage != null && currentTrackedImage.trackingState == TrackingState.Tracking;
        public Transform ImageTargetTransform => (IsImageDetected) ? currentTrackedImage.transform : null;
        public GameObject WallInstance => wallInstance;

        void OnEnable()
        {
            if (trackedImageManager == null)
            {
                trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
            }
            
            if (trackedImageManager != null)
            {
                trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
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
            foreach (var trackedImage in eventArgs.added)
            {
                HandleImageAdded(trackedImage);
            }

            foreach (var trackedImage in eventArgs.updated)
            {
                HandleImageUpdated(trackedImage);
            }
            
            foreach (var trackedImage in eventArgs.removed)
            {
                HandleImageRemoved(trackedImage);
            }
        }

        private void HandleImageAdded(ARTrackedImage trackedImage)
        {
            if (trackedImage.referenceImage.name != targetImageName) return;
            
            currentTrackedImage = trackedImage;
            isImageDetected = true;
            
            CreateWallVisualization(trackedImage);
            OnImageTargetDetected?.Invoke(trackedImage.transform);
        }

        private void HandleImageUpdated(ARTrackedImage trackedImage)
        {
            if (trackedImage.referenceImage.name != targetImageName) return;
            
            currentTrackedImage = trackedImage;
            
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                isImageDetected = true;
                UpdateWallVisualization(trackedImage);
                OnImageTargetUpdated?.Invoke(trackedImage.transform);
            }
        }

        private void HandleImageRemoved(ARTrackedImage trackedImage)
        {
            if (trackedImage.referenceImage.name != targetImageName) return;
            
            currentTrackedImage = null;
            isImageDetected = false;
            OnImageTargetLost?.Invoke();
        }

        private void CreateWallVisualization(ARTrackedImage trackedImage)
        {
            if (wallPrefab == null) return;
            
            if (wallInstance != null)
            {
                Destroy(wallInstance);
            }
            
            wallInstance = Instantiate(wallPrefab);
            UpdateWallVisualization(trackedImage);
        }

        private void UpdateWallVisualization(ARTrackedImage trackedImage)
        {
            if (wallInstance == null) return;
            
            wallInstance.transform.position = trackedImage.transform.position;
            wallInstance.transform.rotation = trackedImage.transform.rotation * Quaternion.Euler(wallRotationOffset);
        }

        public CoordinateFrame GetCurrentCoordinateFrame()
        {
            if (IsImageDetected)
            {
                return new CoordinateFrame(currentTrackedImage.transform);
            }
            return default;
        }
    }
} 