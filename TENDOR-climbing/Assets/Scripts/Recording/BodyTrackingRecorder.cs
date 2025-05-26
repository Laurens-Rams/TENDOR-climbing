using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using BodyTracking.Data;
using BodyTracking.Animation;
using BodyTracking.AR;
using System;
using Unity.XR.CoreUtils;

namespace BodyTracking.Recording
{
    /// <summary>
    /// Handles recording of AR hip joint position with proper coordinate system management
    /// </summary>
    public class BodyTrackingRecorder : MonoBehaviour
    {
        [Header("Recording Settings")]
        [SerializeField] private float targetFrameRate = 30f;
        [SerializeField] private bool showVisualization = true;
        
        [Header("Character Integration")]
        [SerializeField] private FBXCharacterController characterController;
        [SerializeField] private bool autoFindCharacterController = true;
        
        [Header("Orientation Handling")]
        [SerializeField] private bool warnAboutPortraitMode = true;
        [SerializeField] private int maxConsecutiveFailures = 90; // 3 seconds at 30fps
        
        // Dependencies
        private ARHumanBodyManager bodyManager;
        private Transform imageTargetTransform;
        private CoordinateFrame referenceFrame;
        private OrientationManager orientationManager;
        
        // Recording state
        private HipRecording currentRecording;
        private bool isRecording = false;
        private float recordingStartTime;
        private float nextRecordTime;
        
        // Tracking state
        private int consecutiveFailures = 0;
        private bool hasShownOrientationWarning = false;
        private float lastSuccessfulTrackingTime = 0f;
        
        // Visualization
        private GameObject hipVisualizationSphere;
        
        // Events
        public event System.Action<HipRecording> OnRecordingComplete;
        public event System.Action<float> OnRecordingProgress;
        public event System.Action<string> OnTrackingIssueDetected;
        
        // Public properties
        public bool IsRecording => isRecording;
        public float RecordingDuration => isRecording ? Time.time - recordingStartTime : 0f;
        public HipRecording LastRecording => currentRecording;
        public bool HasRecentSuccessfulTracking => (Time.time - lastSuccessfulTrackingTime) < 5f;
        public int ConsecutiveFailures => consecutiveFailures;

        /// <summary>
        /// Initialize the recorder with required dependencies
        /// </summary>
        public bool Initialize(ARHumanBodyManager humanBodyManager, Transform imageTarget)
        {
            bodyManager = humanBodyManager;
            imageTargetTransform = imageTarget;
            
            if (bodyManager == null)
            {
               UnityEngine.Debug.LogError("[BodyTrackingRecorder] ARHumanBodyManager is required");
                return false;
            }
            
            if (imageTargetTransform == null)
            {
               UnityEngine.Debug.LogError("[BodyTrackingRecorder] Image target transform is required");
                return false;
            }
            
            // Store reference frame for coordinate transformations
            referenceFrame = new CoordinateFrame(imageTargetTransform);
            
            // Setup character controller integration
            SetupCharacterController();
            
            // Find orientation manager
            orientationManager = FindFirstObjectByType<OrientationManager>();
            if (orientationManager != null)
            {
                orientationManager.OnOrientationChanged += OnOrientationChanged;
                UnityEngine.Debug.Log("[BodyTrackingRecorder] Connected to OrientationManager");
            }
            
           UnityEngine.Debug.Log($"[BodyTrackingRecorder] Initialized - showVisualization: {showVisualization}, targetFrameRate: {targetFrameRate}");
            return true;
        }

        /// <summary>
        /// Handle orientation changes
        /// </summary>
        private void OnOrientationChanged(ScreenOrientation newOrientation)
        {
            bool isPortrait = newOrientation == ScreenOrientation.Portrait || newOrientation == ScreenOrientation.PortraitUpsideDown;
            
            if (isPortrait && warnAboutPortraitMode && !hasShownOrientationWarning)
            {
                string warning = "⚠️ Portrait orientation detected - ARKit body tracking may not work properly. Consider rotating to landscape for better results.";
                UnityEngine.Debug.LogWarning($"[BodyTrackingRecorder] {warning}");
                OnTrackingIssueDetected?.Invoke(warning);
                hasShownOrientationWarning = true;
            }
            else if (!isPortrait)
            {
                hasShownOrientationWarning = false; // Reset warning for next portrait switch
                if (consecutiveFailures > 10)
                {
                    UnityEngine.Debug.Log("[BodyTrackingRecorder] ✅ Switched to landscape - body tracking should improve");
                }
            }
        }

        /// <summary>
        /// Setup integration with character controller
        /// </summary>
        private void SetupCharacterController()
        {
            if (characterController == null && autoFindCharacterController)
            {
                characterController = FindObjectOfType<FBXCharacterController>();
                if (characterController != null)
                {
                   UnityEngine.Debug.Log("[BodyTrackingRecorder] Found FBXCharacterController automatically");
                }
            }
            
            if (characterController != null)
            {
                if (!characterController.IsInitialized)
                {
                    characterController.Initialize();
                }
                
                // Make sure character is visible and positioned during recording
                characterController.SetDebugVisualization(true);
                
               UnityEngine.Debug.Log("[BodyTrackingRecorder] Character controller integration enabled for recording");
            }
            else
            {
               UnityEngine.Debug.Log("[BodyTrackingRecorder] No character controller found - hip tracking only");
            }
        }

        /// <summary>
        /// Start recording hip joint position
        /// </summary>
        public bool StartRecording()
        {
            if (isRecording)
            {
               UnityEngine.Debug.LogWarning("[BodyTrackingRecorder] Already recording");
                return false;
            }
            
            if (bodyManager == null || imageTargetTransform == null)
            {
               UnityEngine.Debug.LogError("[BodyTrackingRecorder] Not properly initialized");
                return false;
            }
            
            // Update reference frame at recording start
            referenceFrame = new CoordinateFrame(imageTargetTransform);
            
            // Initialize recording
            currentRecording = new HipRecording
            {
                frameRate = targetFrameRate,
                referenceImageTargetPosition = referenceFrame.position,
                referenceImageTargetRotation = referenceFrame.rotation,
                referenceImageTargetScale = referenceFrame.scale,
                recordingTimestamp = DateTime.Now
            };
            
            isRecording = true;
            recordingStartTime = Time.time;
            nextRecordTime = 0f;
            
            return true;
        }

        /// <summary>
        /// Stop recording and return the recorded data
        /// </summary>
        public HipRecording StopRecording()
        {
            if (!isRecording)
            {
               UnityEngine.Debug.LogWarning("[BodyTrackingRecorder] Not currently recording");
                return null;
            }
            
            isRecording = false;
            currentRecording.duration = Time.time - recordingStartTime;
            
            // Hide visualization
            if (hipVisualizationSphere != null)
            {
                hipVisualizationSphere.SetActive(false);
            }
            
            OnRecordingComplete?.Invoke(currentRecording);
            return currentRecording;
        }

        void Update()
        {
            if (!isRecording) return;
            
            float currentTime = Time.time - recordingStartTime;
            
            // Record at target frame rate
            if (currentTime >= nextRecordTime)
            {
                RecordCurrentFrame(currentTime);
                nextRecordTime += 1f / targetFrameRate;
                
                // Notify progress
                OnRecordingProgress?.Invoke(currentTime);
            }
        }

        /// <summary>
        /// Record the current frame's hip position
        /// </summary>
        private void RecordCurrentFrame(float timestamp)
        {
            var frame = new HipFrame
            {
                timestamp = timestamp,
                hipJoint = HipJointData.Invalid
            };
            
            bool foundValidJoint = false;
            
            // Process all tracked bodies (usually just one)
            foreach (var humanBody in bodyManager.trackables)
            {
                if (humanBody.trackingState == TrackingState.None || humanBody.trackingState == TrackingState.Limited)
                    continue;
                
                var joints = humanBody.joints;
                if (!joints.IsCreated || joints.Length == 0)
                    continue;
                
                // Find the best hip/pelvis joint - prioritize joint 2 (appears to be hip in ARKit)
                Vector3? bestJointPosition = null;
                
                // First, try to use joint 2 if it's tracked and has position data (hip/pelvis in ARKit)
                if (joints.Length > 2 && joints[2].tracked && joints[2].localPose.position != Vector3.zero)
                {
                    bestJointPosition = joints[2].localPose.position;
                }
                else
                {
                    // Fallback: find any tracked joint with position data
                    for (int i = 0; i < joints.Length; i++)
                    {
                        var joint = joints[i];
                        if (joint.tracked && joint.localPose.position != Vector3.zero)
                        {
                            bestJointPosition = joint.localPose.position;
                            break;
                        }
                    }
                }
                
                if (bestJointPosition.HasValue)
                {
                    // Transform from AR session space to world space  
                    Vector3 worldPosition = humanBody.transform.TransformPoint(bestJointPosition.Value);
                    
                    // Transform to reference frame space
                    Vector3 referencePosition = referenceFrame.InverseTransformPoint(worldPosition);
                    
                    frame.hipJoint = new HipJointData(
                        referencePosition,
                        1.0f, // Could use actual confidence if available
                        true
                    );
                    
                    foundValidJoint = true;
                    consecutiveFailures = 0; // Reset failure counter
                    lastSuccessfulTrackingTime = Time.time;
                    
                    // Update visualization
                    if (showVisualization)
                    {
                        UpdateVisualization(worldPosition);
                    }
                }
                
                break; // Use first tracked body only
            }
            
            // Handle tracking failures
            if (!foundValidJoint)
            {
                consecutiveFailures++;
                
                // Provide helpful feedback based on failure patterns
                if (consecutiveFailures == 30) // 1 second of failures
                {
                    CheckAndReportTrackingIssues();
                }
                else if (consecutiveFailures % 90 == 0) // Every 3 seconds
                {
                    UnityEngine.Debug.LogWarning($"[BodyTrackingRecorder] No valid joint data found at frame {currentRecording.FrameCount} (consecutive failures: {consecutiveFailures})");
                    CheckAndReportTrackingIssues();
                }
            }
            
            // Always add frame (even if no body detected) to maintain timing
            currentRecording.frames.Add(frame);
        }

        /// <summary>
        /// Check for common tracking issues and provide helpful feedback
        /// </summary>
        private void CheckAndReportTrackingIssues()
        {
            string issue = "";
            
            // Check orientation
            if (orientationManager != null && !orientationManager.IsBodyTrackingOptimal)
            {
                issue = $"Device orientation issue: {orientationManager.GetOrientationRecommendation()}";
            }
            // Check if body manager is enabled
            else if (bodyManager == null || !bodyManager.enabled)
            {
                issue = "ARHumanBodyManager is disabled or missing";
            }
            // Check tracking state
            else if (bodyManager.trackables.count == 0)
            {
                issue = "No human bodies detected - ensure person is visible and well-lit";
            }
            else
            {
                // Check if bodies are tracked but joints are invalid
                bool hasTrackedBodies = false;
                foreach (var body in bodyManager.trackables)
                {
                    if (body.trackingState == TrackingState.Tracking)
                    {
                        hasTrackedBodies = true;
                        break;
                    }
                }
                
                if (hasTrackedBodies)
                {
                    issue = "Human body detected but joint data is invalid - try moving or improving lighting";
                }
                else
                {
                    issue = "Human body tracking is limited - ensure person is fully visible";
                }
            }
            
            if (!string.IsNullOrEmpty(issue))
            {
                UnityEngine.Debug.LogWarning($"[BodyTrackingRecorder] Tracking issue: {issue}");
                OnTrackingIssueDetected?.Invoke(issue);
            }
        }

        /// <summary>
        /// Get tracking status summary including orientation info
        /// </summary>
        public string GetTrackingStatusSummary()
        {
            string status = $"Recording: {isRecording}\n";
            status += $"Frame Count: {(currentRecording?.FrameCount ?? 0)}\n";
            status += $"Consecutive Failures: {consecutiveFailures}\n";
            status += $"Recent Success: {HasRecentSuccessfulTracking}\n";
            
            if (orientationManager != null)
            {
                status += $"Orientation: {orientationManager.GetBodyTrackingStatusSummary()}\n";
            }
            else
            {
                status += $"Orientation: {Screen.orientation}\n";
            }
            
            if (bodyManager != null)
            {
                status += $"Bodies Tracked: {bodyManager.trackables.count}\n";
                status += $"Body Manager Enabled: {bodyManager.enabled}\n";
            }
            
            return status;
        }

        /// <summary>
        /// Update hip visualization and character positioning
        /// </summary>
        private void UpdateVisualization(Vector3 worldPosition)
        {
            if (hipVisualizationSphere == null)
            {
                // Create red sphere for hip visualization
                hipVisualizationSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hipVisualizationSphere.name = "HipVisualization_AR";
                hipVisualizationSphere.transform.localScale = Vector3.one * 0.3f;
                
                // Parent to XR Origin for proper AR sync
                var xrOrigin = FindFirstObjectByType<XROrigin>();
                if (xrOrigin != null)
                {
                    hipVisualizationSphere.transform.SetParent(xrOrigin.transform);
                   UnityEngine.Debug.Log("[BodyTrackingRecorder] Parented hip sphere to XR Origin");
                }
                
                // Make it bright red using AR-compatible shader
                var renderer = hipVisualizationSphere.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var material = new Material(Shader.Find("Unlit/Color"));
                    material.color = Color.red;
                    renderer.material = material;
                   UnityEngine.Debug.Log("[BodyTrackingRecorder] Applied red material to hip sphere");
                }
                
                // Remove collider
                var collider = hipVisualizationSphere.GetComponent<Collider>();
                if (collider != null)
                {
                    DestroyImmediate(collider);
                }
                
               UnityEngine.Debug.Log($"[BodyTrackingRecorder] Created hip visualization sphere at {worldPosition}");
            }
            
            hipVisualizationSphere.transform.position = worldPosition;
            hipVisualizationSphere.SetActive(true);
            
            // Update character position if controller is available
            if (characterController != null && characterController.IsInitialized)
            {
                characterController.SetTargetHipPosition(worldPosition);
            }
        }

        void OnDestroy()
        {
            // Clean up visualization
            if (hipVisualizationSphere != null)
            {
                Destroy(hipVisualizationSphere);
            }
        }
    }
} 