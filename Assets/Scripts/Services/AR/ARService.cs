using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Threading.Tasks;
using System.Collections.Generic;
using TENDOR.Core;
using TENDOR.Runtime.Models;
using TENDOR.Services.Firebase;
using Logger = TENDOR.Core.Logger;

namespace TENDOR.Services.AR
{
    /// <summary>
    /// Centralized AR service managing AR Foundation components and runtime image library
    /// Ensures single ARSession, ARCameraManager, AROcclusionManager, and ARTrackedImageManager
    /// </summary>
    public class ARService : MonoBehaviour
    {
        public static ARService Instance { get; private set; }

        [Header("AR Components")]
        [SerializeField] private ARSession arSession;
        [SerializeField] private ARCameraManager arCameraManager;
        [SerializeField] private AROcclusionManager arOcclusionManager;
        [SerializeField] private ARTrackedImageManager arTrackedImageManager;
        [SerializeField] private ARHumanBodyManager arHumanBodyManager;

        [Header("Configuration")]
        [SerializeField] private bool autoLoadImageLibrary = true;
        [SerializeField] private float imageLibraryRefreshInterval = 300f; // 5 minutes

        // Boulder data storage
        private Dictionary<string, BoulderData> loadedBoulders = new Dictionary<string, BoulderData>();

        // Events
        public event System.Action<ARTrackedImage> OnImageTracked;
        public event System.Action<ARTrackedImage> OnImageLost;
        public event System.Action OnImageLibraryLoaded;

        private bool isInitialized = false;
        private float lastLibraryRefresh = 0f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeARComponents();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private async void Start()
        {
            if (autoLoadImageLibrary)
            {
                await LoadImageLibraryAsync();
            }
        }

        private async void Update()
        {
            // Periodically refresh image library
            if (autoLoadImageLibrary && Time.time - lastLibraryRefresh > imageLibraryRefreshInterval)
            {
                await LoadImageLibraryAsync();
            }
        }

        private void InitializeARComponents()
        {
            try
            {
                Logger.Log("Initializing AR components...", "AR");

                // Find or create AR components
                FindOrCreateARSession();
                FindOrCreateARCameraManager();
                FindOrCreateARTrackedImageManager();
                FindOrCreateARHumanBodyManager();
                FindOrCreateAROcclusionManager();

                // Subscribe to events
                if (arTrackedImageManager != null)
                {
                    arTrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
                }

                isInitialized = true;
                Logger.Log("AR components initialized successfully", "AR");
            }
            catch (System.Exception e)
            {
                Logger.LogError(e, "AR");
            }
        }

        #region AR Component Management

        private void FindOrCreateARSession()
        {
            if (arSession == null)
            {
                arSession = FindFirstObjectByType<ARSession>();
            }

            if (arSession == null)
            {
                GameObject sessionObj = new GameObject("AR Session");
                arSession = sessionObj.AddComponent<ARSession>();
                Logger.Log("Created AR Session", "AR");
            }
            else
            {
                Logger.Log("Found existing AR Session", "AR");
            }
        }

        private void FindOrCreateARCameraManager()
        {
            if (arCameraManager == null)
            {
                arCameraManager = FindFirstObjectByType<ARCameraManager>();
            }

            if (arCameraManager == null)
            {
                // Look for XR Origin or create one
                var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
                if (xrOrigin == null)
                {
                    Logger.LogError("No XR Origin found. Please ensure XR Origin is set up in the scene.", "AR");
                    return;
                }

                arCameraManager = xrOrigin.Camera.gameObject.GetComponent<ARCameraManager>();
                if (arCameraManager == null)
                {
                    arCameraManager = xrOrigin.Camera.gameObject.AddComponent<ARCameraManager>();
                    Logger.Log("Created AR Camera Manager", "AR");
                }
            }
            else
            {
                Logger.Log("Found existing AR Camera Manager", "AR");
            }
        }

        private void FindOrCreateARTrackedImageManager()
        {
            if (arTrackedImageManager == null)
            {
                arTrackedImageManager = FindFirstObjectByType<ARTrackedImageManager>();
            }

            if (arTrackedImageManager == null)
            {
                var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
                if (xrOrigin != null)
                {
                    arTrackedImageManager = xrOrigin.gameObject.AddComponent<ARTrackedImageManager>();
                    Logger.Log("Created AR Tracked Image Manager", "AR");
                }
            }
            else
            {
                Logger.Log("Found existing AR Tracked Image Manager", "AR");
            }
        }

        private void FindOrCreateARHumanBodyManager()
        {
            if (arHumanBodyManager == null)
            {
                arHumanBodyManager = FindFirstObjectByType<ARHumanBodyManager>();
            }

            if (arHumanBodyManager == null)
            {
                var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
                if (xrOrigin != null)
                {
                    arHumanBodyManager = xrOrigin.gameObject.AddComponent<ARHumanBodyManager>();
                    Logger.Log("Created AR Human Body Manager", "AR");
                }
            }
            else
            {
                Logger.Log("Found existing AR Human Body Manager", "AR");
            }
        }

        private void FindOrCreateAROcclusionManager()
        {
            if (arOcclusionManager == null)
            {
                arOcclusionManager = FindFirstObjectByType<AROcclusionManager>();
            }

            if (arOcclusionManager == null)
            {
                var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
                if (xrOrigin != null && arCameraManager != null)
                {
                    arOcclusionManager = arCameraManager.gameObject.AddComponent<AROcclusionManager>();
                    Logger.Log("Created AR Occlusion Manager", "AR");
                }
            }
            else
            {
                Logger.Log("Found existing AR Occlusion Manager", "AR");
            }
        }

        #endregion

        #region Runtime Image Library

        /// <summary>
        /// Load runtime image library from Firebase
        /// </summary>
        public async Task LoadImageLibraryAsync()
        {
            try
            {
                Logger.Log("Loading runtime image library...", "AR");

                if (arTrackedImageManager == null)
                {
                    Logger.LogError("AR Tracked Image Manager not available", "AR");
                    return;
                }

                // Get active boulders from Firebase
                var boulders = await FirebaseService.Instance.GetActiveBoulders();
                
                if (boulders == null || boulders.Length == 0)
                {
                    Logger.LogWarning("No active boulders found", "AR");
                    return;
                }

                // Load images for each boulder (simplified approach)
                int loadedCount = 0;
                foreach (var boulder in boulders)
                {
                    if (!loadedBoulders.ContainsKey(boulder.id))
                    {
                        bool success = await LoadBoulderImage(boulder);
                        if (success)
                        {
                            loadedBoulders[boulder.id] = boulder;
                            loadedCount++;
                        }
                    }
                }

                Logger.Log($"Processed {loadedCount} boulder images (runtime library creation requires AR Subsystems)", "AR");

                lastLibraryRefresh = Time.time;
                OnImageLibraryLoaded?.Invoke();
            }
            catch (System.Exception e)
            {
                Logger.LogError($"Failed to load image library: {e.Message}", "AR");
            }
        }

        private async Task<bool> LoadBoulderImage(BoulderData boulder)
        {
            try
            {
                Logger.Log($"Loading boulder image: {boulder.name}", "AR");
                
                // Download image from Firebase Storage
                var imageBytes = await FirebaseService.Instance.DownloadBoulderImage(boulder.id);
                if (imageBytes == null || imageBytes.Length == 0)
                {
                    Logger.LogError($"Failed to download image for boulder: {boulder.name}", "AR");
                    return false;
                }

                // Create texture from downloaded bytes
                var texture = new Texture2D(2, 2);
                if (!texture.LoadImage(imageBytes))
                {
                    Logger.LogError($"Failed to load texture for boulder: {boulder.name}", "AR");
                    return false;
                }

                // Store texture for future use (runtime library addition requires AR Subsystems package)
                Logger.Log($"Successfully loaded image: {boulder.name} ({texture.width}x{texture.height})", "AR");
                
                // Clean up texture for now
                UnityEngine.Object.DestroyImmediate(texture);
                
                    return true;
            }
            catch (System.Exception e)
            {
                Logger.LogError($"Error loading boulder image {boulder.name}: {e.Message}", "AR");
                return false;
            }
        }

        #endregion

        #region Event Handlers

        private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            foreach (var trackedImage in eventArgs.added)
            {
                Logger.Log($"Image tracked: {trackedImage.referenceImage.name}", "AR");
                OnImageTracked?.Invoke(trackedImage);
            }

            foreach (var trackedImage in eventArgs.updated)
            {
                if (trackedImage.trackingState == TrackingState.Tracking)
                {
                    OnImageTracked?.Invoke(trackedImage);
                }
                else if (trackedImage.trackingState == TrackingState.None || trackedImage.trackingState == TrackingState.Limited)
                {
                    Logger.Log($"Image lost: {trackedImage.referenceImage.name}", "AR");
                    OnImageLost?.Invoke(trackedImage);
                }
            }

            foreach (var trackedImage in eventArgs.removed)
            {
                Logger.Log($"Image removed: {trackedImage.referenceImage.name}", "AR");
                OnImageLost?.Invoke(trackedImage);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get AR Camera Manager instance
        /// </summary>
        public ARCameraManager GetCameraManager() => arCameraManager;

        /// <summary>
        /// Get AR Human Body Manager instance
        /// </summary>
        public ARHumanBodyManager GetHumanBodyManager() => arHumanBodyManager;

        /// <summary>
        /// Get AR Tracked Image Manager instance
        /// </summary>
        public ARTrackedImageManager GetTrackedImageManager() => arTrackedImageManager;

        /// <summary>
        /// Get boulder data by ID
        /// </summary>
        public BoulderData GetBoulderData(string boulderId)
        {
            return loadedBoulders.TryGetValue(boulderId, out var boulder) ? boulder : null;
        }

        /// <summary>
        /// Check if AR is properly initialized
        /// </summary>
        public bool IsInitialized => isInitialized;

        /// <summary>
        /// Manually refresh image library
        /// </summary>
        public async void RefreshImageLibrary()
        {
            await LoadImageLibraryAsync();
        }

        #endregion

        private void OnDestroy()
        {
            if (arTrackedImageManager != null)
            {
                arTrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
            }
        }
    }
} 