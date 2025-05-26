using UnityEngine;
using UnityEngine.XR.ARFoundation;
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

        // Runtime image library (stubbed for now)
        private object runtimeImageLibrary; // TODO: Replace with MutableRuntimeReferenceImageLibrary when AR Subsystems is available
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

        private void Start()
        {
            if (autoLoadImageLibrary)
            {
                LoadImageLibraryAsync();
            }
        }

        private void Update()
        {
            // Periodically refresh image library
            if (autoLoadImageLibrary && Time.time - lastLibraryRefresh > imageLibraryRefreshInterval)
            {
                LoadImageLibraryAsync();
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
                arSession = FindObjectOfType<ARSession>();
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
                arCameraManager = FindObjectOfType<ARCameraManager>();
            }

            if (arCameraManager == null)
            {
                // Look for XR Origin or create one
                var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
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
                arTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
            }

            if (arTrackedImageManager == null)
            {
                var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
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
                arHumanBodyManager = FindObjectOfType<ARHumanBodyManager>();
            }

            if (arHumanBodyManager == null)
            {
                var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
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
                arOcclusionManager = FindObjectOfType<AROcclusionManager>();
            }

            if (arOcclusionManager == null)
            {
                var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
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

                // Create runtime library if needed
                if (runtimeImageLibrary == null)
                {
                    // TODO: Replace with actual CreateRuntimeLibrary when AR Subsystems is available
                    // var library = arTrackedImageManager.CreateRuntimeLibrary();
                    runtimeImageLibrary = new object(); // Stub implementation
                    Logger.Log("Created stub runtime image library", "AR");
                }

                // Load images for each boulder
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

                // Update the tracked image manager (stubbed for now)
                if (loadedCount > 0)
                {
                    // TODO: Set actual reference library when AR Subsystems is available
                    // arTrackedImageManager.referenceLibrary = runtimeImageLibrary;
                    Logger.Log($"Loaded {loadedCount} new images into runtime library (stub)", "AR");
                }

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
                Logger.Log($"Loading boulder image (stub): {boulder.name}", "AR");
                
                // TODO: Implement actual image loading when AR Subsystems package is available
                // This is a stub implementation for now
                
                // Simulate download and processing
                await Task.Delay(100);
                
                Logger.Log($"Successfully loaded image (stub): {boulder.name}", "AR");
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
                // TrackingState.Tracking = 2, TrackingState.None = 0 (from AR Subsystems documentation)
                if ((int)trackedImage.trackingState == 2) // Tracking
                {
                    OnImageTracked?.Invoke(trackedImage);
                }
                else if ((int)trackedImage.trackingState == 0) // None
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
        public void RefreshImageLibrary()
        {
            LoadImageLibraryAsync();
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