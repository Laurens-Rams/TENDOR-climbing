using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace BodyTracking.AR
{
    /// <summary>
    /// Manages device orientation for optimal ARKit body tracking performance
    /// ARKit body tracking works best in landscape orientation
    /// </summary>
    public class OrientationManager : MonoBehaviour
    {
        [Header("Orientation Settings")]
        [SerializeField] private bool forceOptimalOrientation = true;
        [SerializeField] private ScreenOrientation preferredOrientation = ScreenOrientation.LandscapeLeft;
        [SerializeField] private bool allowPortraitFallback = false;
        [SerializeField] private bool showOrientationWarnings = true;
        
        [Header("Body Tracking Integration")]
        [SerializeField] private ARHumanBodyManager humanBodyManager;
        [SerializeField] private bool autoFindBodyManager = true;
        
        // State tracking
        private ScreenOrientation currentOrientation;
        private bool isBodyTrackingOptimal = false;
        private float lastOrientationCheck = 0f;
        private const float ORIENTATION_CHECK_INTERVAL = 1f;
        
        // Events
        public event System.Action<ScreenOrientation> OnOrientationChanged;
        public event System.Action<bool> OnBodyTrackingOptimalChanged;
        
        // Public properties
        public bool IsBodyTrackingOptimal => isBodyTrackingOptimal;
        public ScreenOrientation CurrentOrientation => currentOrientation;
        public bool IsLandscape => currentOrientation == ScreenOrientation.LandscapeLeft || currentOrientation == ScreenOrientation.LandscapeRight;
        public bool IsPortrait => currentOrientation == ScreenOrientation.Portrait || currentOrientation == ScreenOrientation.PortraitUpsideDown;

        void Start()
        {
            InitializeOrientationManager();
        }

        void Update()
        {
            // Check orientation periodically
            if (Time.time - lastOrientationCheck > ORIENTATION_CHECK_INTERVAL)
            {
                CheckAndUpdateOrientation();
                lastOrientationCheck = Time.time;
            }
        }

        /// <summary>
        /// Initialize the orientation manager
        /// </summary>
        private void InitializeOrientationManager()
        {
            // Find body manager if not assigned
            if (humanBodyManager == null && autoFindBodyManager)
            {
                humanBodyManager = FindFirstObjectByType<ARHumanBodyManager>();
            }
            
            // Set initial orientation
            currentOrientation = Screen.orientation;
            
            // Force optimal orientation if enabled
            if (forceOptimalOrientation)
            {
                SetOptimalOrientation();
            }
            
            Debug.Log($"[OrientationManager] Initialized - Current: {currentOrientation}, Preferred: {preferredOrientation}");
        }

        /// <summary>
        /// Check and update current orientation status
        /// </summary>
        private void CheckAndUpdateOrientation()
        {
            ScreenOrientation newOrientation = Screen.orientation;
            
            if (newOrientation != currentOrientation)
            {
                ScreenOrientation previousOrientation = currentOrientation;
                currentOrientation = newOrientation;
                
                Debug.Log($"[OrientationManager] Orientation changed: {previousOrientation} → {currentOrientation}");
                OnOrientationChanged?.Invoke(currentOrientation);
                
                // Check if body tracking is optimal
                UpdateBodyTrackingOptimalStatus();
                
                // Show warnings if needed
                if (showOrientationWarnings && IsPortrait)
                {
                    Debug.LogWarning("[OrientationManager] ⚠️ Portrait orientation detected - ARKit body tracking may not work properly. Consider rotating to landscape.");
                }
            }
        }

        /// <summary>
        /// Update body tracking optimal status
        /// </summary>
        private void UpdateBodyTrackingOptimalStatus()
        {
            bool wasOptimal = isBodyTrackingOptimal;
            
            // ARKit body tracking works best in landscape
            isBodyTrackingOptimal = IsLandscape;
            
            if (wasOptimal != isBodyTrackingOptimal)
            {
                Debug.Log($"[OrientationManager] Body tracking optimal status changed: {isBodyTrackingOptimal}");
                OnBodyTrackingOptimalChanged?.Invoke(isBodyTrackingOptimal);
                
                if (isBodyTrackingOptimal)
                {
                    Debug.Log("[OrientationManager] ✅ Orientation is now optimal for body tracking");
                }
                else
                {
                    Debug.LogWarning("[OrientationManager] ⚠️ Orientation is not optimal for body tracking");
                }
            }
        }

        /// <summary>
        /// Force the device to use optimal orientation for body tracking
        /// </summary>
        public void SetOptimalOrientation()
        {
            if (Screen.orientation != preferredOrientation)
            {
                Screen.orientation = preferredOrientation;
                Debug.Log($"[OrientationManager] Forced orientation to: {preferredOrientation}");
            }
        }

        /// <summary>
        /// Allow auto-rotation with optimal preferences
        /// </summary>
        public void EnableAutoRotation()
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
            
            // Set landscape as preferred orientations
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = allowPortraitFallback;
            Screen.autorotateToPortraitUpsideDown = allowPortraitFallback;
            
            Debug.Log($"[OrientationManager] Enabled auto-rotation - Portrait fallback: {allowPortraitFallback}");
        }

        /// <summary>
        /// Get orientation recommendation for user
        /// </summary>
        public string GetOrientationRecommendation()
        {
            if (IsLandscape)
            {
                return "✅ Optimal orientation for body tracking";
            }
            else if (IsPortrait)
            {
                return "⚠️ Rotate to landscape for better body tracking";
            }
            else
            {
                return "🔄 Checking orientation...";
            }
        }

        /// <summary>
        /// Check if current orientation supports reliable body tracking
        /// </summary>
        public bool CanReliablyTrackBody()
        {
            // ARKit body tracking is most reliable in landscape
            return IsLandscape && humanBodyManager != null && humanBodyManager.enabled;
        }

        /// <summary>
        /// Get body tracking status summary
        /// </summary>
        public string GetBodyTrackingStatusSummary()
        {
            if (humanBodyManager == null)
            {
                return "❌ No body manager found";
            }
            
            if (!humanBodyManager.enabled)
            {
                return "❌ Body tracking disabled";
            }
            
            string orientationStatus = IsLandscape ? "✅ Landscape" : "⚠️ Portrait";
            string trackingStatus = CanReliablyTrackBody() ? "✅ Optimal" : "⚠️ Suboptimal";
            
            return $"Orientation: {orientationStatus} | Tracking: {trackingStatus}";
        }

        #region Public API

        /// <summary>
        /// Force landscape orientation for optimal body tracking
        /// </summary>
        public void ForceLandscape()
        {
            preferredOrientation = ScreenOrientation.LandscapeLeft;
            SetOptimalOrientation();
        }

        /// <summary>
        /// Allow portrait mode (not recommended for body tracking)
        /// </summary>
        public void AllowPortrait()
        {
            allowPortraitFallback = true;
            EnableAutoRotation();
        }

        /// <summary>
        /// Disable portrait mode for better body tracking
        /// </summary>
        public void DisablePortrait()
        {
            allowPortraitFallback = false;
            if (IsPortrait)
            {
                SetOptimalOrientation();
            }
            else
            {
                EnableAutoRotation();
            }
        }

        #endregion

        #region Unity Editor Support

#if UNITY_EDITOR
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void EditorSimulateOrientationChange(ScreenOrientation newOrientation)
        {
            currentOrientation = newOrientation;
            UpdateBodyTrackingOptimalStatus();
            Debug.Log($"[OrientationManager] Editor simulation: {newOrientation}");
        }
#endif

        #endregion
    }
} 