using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using BodyTracking.Data;
using System;

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
        [SerializeField] private bool debugMode = false;
        
        // Dependencies
        private ARHumanBodyManager bodyManager;
        private Transform imageTargetTransform;
        private CoordinateFrame referenceFrame;
        
        // Recording state
        private HipRecording currentRecording;
        private bool isRecording = false;
        private float recordingStartTime;
        private float nextRecordTime;
        
        // Visualization
        private GameObject hipVisualizationSphere;
        
        // Events
        public event System.Action<HipRecording> OnRecordingComplete;
        public event System.Action<float> OnRecordingProgress;
        
        // Public properties
        public bool IsRecording => isRecording;
        public float RecordingDuration => isRecording ? Time.time - recordingStartTime : 0f;
        public HipRecording LastRecording => currentRecording;

        /// <summary>
        /// Create a test sphere at the body pose position for debugging
        /// </summary>
        private GameObject bodyPoseTestSphere;
        private void CreateBodyPoseTestSphere(Vector3 bodyPosePosition)
        {
            if (bodyPoseTestSphere == null)
            {
                bodyPoseTestSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bodyPoseTestSphere.name = "BodyPoseTestSphere";
                bodyPoseTestSphere.transform.localScale = Vector3.one * 0.3f;
                
                // Parent to AR Session Origin for proper AR sync
                var arSessionOrigin = FindObjectOfType<UnityEngine.XR.ARFoundation.ARSessionOrigin>();
                if (arSessionOrigin != null)
                {
                    bodyPoseTestSphere.transform.SetParent(arSessionOrigin.transform);
                    Debug.Log("[BodyTrackingRecorder] Parented body pose test sphere to AR Session Origin");
                }
                
                // Make it blue to distinguish from red hip sphere
                var renderer = bodyPoseTestSphere.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var material = new Material(Shader.Find("Unlit/Color"));
                    material.color = Color.blue;
                    renderer.material = material;
                }
                
                // Remove collider
                var collider = bodyPoseTestSphere.GetComponent<Collider>();
                if (collider != null)
                {
                    DestroyImmediate(collider);
                }
                
                Debug.Log("[BodyTrackingRecorder] Created blue body pose test sphere");
            }
            
            bodyPoseTestSphere.transform.position = bodyPosePosition;
            bodyPoseTestSphere.SetActive(true);
            
            Debug.Log($"[BodyTrackingRecorder] Body pose test sphere at: {bodyPosePosition:F3}");
        }

        /// <summary>
        /// Initialize the recorder with required dependencies
        /// </summary>
        public bool Initialize(ARHumanBodyManager humanBodyManager, Transform imageTarget)
        {
            bodyManager = humanBodyManager;
            imageTargetTransform = imageTarget;
            
            if (bodyManager == null)
            {
                Debug.LogError("[BodyTrackingRecorder] ARHumanBodyManager is required");
                return false;
            }
            
            if (imageTargetTransform == null)
            {
                Debug.LogError("[BodyTrackingRecorder] Image target transform is required");
                return false;
            }
            
            // Store reference frame for coordinate transformations
            referenceFrame = new CoordinateFrame(imageTargetTransform);
            
            Debug.Log("[BodyTrackingRecorder] Successfully initialized for hip joint recording");
            return true;
        }

        /// <summary>
        /// Start recording hip joint position
        /// </summary>
        public bool StartRecording()
        {
            if (isRecording)
            {
                Debug.LogWarning("[BodyTrackingRecorder] Already recording");
                return false;
            }
            
            if (bodyManager == null || imageTargetTransform == null)
            {
                Debug.LogError("[BodyTrackingRecorder] Not properly initialized");
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
            
            Debug.Log($"[BodyTrackingRecorder] Started hip recording at {targetFrameRate} FPS");
            return true;
        }

        /// <summary>
        /// Stop recording and return the recorded data
        /// </summary>
        public HipRecording StopRecording()
        {
            if (!isRecording)
            {
                Debug.LogWarning("[BodyTrackingRecorder] Not currently recording");
                return null;
            }
            
            isRecording = false;
            currentRecording.duration = Time.time - recordingStartTime;
            
            // Hide visualization
            if (hipVisualizationSphere != null)
            {
                hipVisualizationSphere.SetActive(false);
            }
            
            Debug.Log($"[BodyTrackingRecorder] Hip recording completed: {currentRecording.FrameCount} frames, {currentRecording.duration:F2}s");
            
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
            
            bool hasValidData = false;
            
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
                int bestJointIndex = -1;
                
                // First, try to use joint 2 if it's tracked and has position data (hip/pelvis in ARKit)
                if (joints.Length > 2 && joints[2].tracked && joints[2].localPose.position != Vector3.zero)
                {
                    bestJointPosition = joints[2].localPose.position;
                    bestJointIndex = 2;
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
                            bestJointIndex = i;
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
                    
                    if (debugMode)
                    {
                        Debug.Log($"[BodyTrackingRecorder] Using joint {bestJointIndex}: local={bestJointPosition.Value}, world={worldPosition}, ref={referencePosition}");
                    }
                    
                    frame.hipJoint = new HipJointData(
                        referencePosition,
                        1.0f, // Could use actual confidence if available
                        true
                    );
                    
                    hasValidData = true;
                    
                    // Update visualization
                    if (showVisualization)
                    {
                        UpdateVisualization(worldPosition);
                        
                        // Also create a test sphere at the raw body pose for comparison
                        if (debugMode)
                        {
                            CreateBodyPoseTestSphere(humanBody.pose.position);
                        }
                    }
                    
                    if (debugMode && currentRecording.FrameCount % 30 == 0)
                    {
                        Debug.Log($"[BodyTrackingRecorder] Using joint {bestJointIndex}: Local={bestJointPosition.Value:F3}, World={worldPosition:F3}, Relative={referencePosition:F3}");
                    }
                }
                
                // Fallback: if no joints are tracked, try using body pose
                if (!hasValidData && debugMode)
                {
                    Debug.LogWarning($"[BodyTrackingRecorder] No tracked joints found, frame will be invalid");
                }
                
                break; // Use first tracked body only
            }
            
            // Always add frame (even if no body detected) to maintain timing
            currentRecording.frames.Add(frame);
            
            if (debugMode && currentRecording.FrameCount % 90 == 0)
            {
                int validFrames = 0;
                foreach (var recordedFrame in currentRecording.frames)
                {
                    if (recordedFrame.hipJoint.IsValid) validFrames++;
                }
                
                float validPercentage = (float)validFrames / currentRecording.FrameCount * 100f;
                Debug.Log($"[BodyTrackingRecorder] Frame {currentRecording.FrameCount}: {validPercentage:F1}% valid positions");
            }
        }

        /// <summary>
        /// Update hip visualization
        /// </summary>
        private void UpdateVisualization(Vector3 worldPosition)
        {
            if (hipVisualizationSphere == null)
            {
                // Create red sphere for hip visualization
                hipVisualizationSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hipVisualizationSphere.name = "HipVisualization_AR";
                hipVisualizationSphere.transform.localScale = Vector3.one * 0.3f; // Visible but not too big
                
                // Parent to AR Session Origin for proper AR sync
                var arSessionOrigin = FindObjectOfType<UnityEngine.XR.ARFoundation.ARSessionOrigin>();
                if (arSessionOrigin != null)
                {
                    hipVisualizationSphere.transform.SetParent(arSessionOrigin.transform);
                    Debug.Log($"[BodyTrackingRecorder] Parented hip sphere to AR Session Origin");
                }
                
                // Make it bright red and emissive using AR-compatible shader
                var renderer = hipVisualizationSphere.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // Use Unlit shader which works better in AR
                    var material = new Material(Shader.Find("Unlit/Color"));
                    material.color = Color.red;
                    renderer.material = material;
                    Debug.Log($"[BodyTrackingRecorder] Applied Unlit/Color red material");
                }
                
                // Remove collider
                var collider = hipVisualizationSphere.GetComponent<Collider>();
                if (collider != null)
                {
                    DestroyImmediate(collider);
                }
                
                Debug.Log($"[BodyTrackingRecorder] Created hip visualization sphere with scale {hipVisualizationSphere.transform.localScale}");
            }
            
            hipVisualizationSphere.transform.position = worldPosition;
            hipVisualizationSphere.SetActive(true);
            
            // Debug logging for sphere positioning (every few frames to avoid spam)
            if (debugMode && Time.frameCount % 30 == 0)
            {
                Debug.Log($"[BodyTrackingRecorder] Hip sphere at: {worldPosition:F3}");
                Debug.Log($"[BodyTrackingRecorder] Sphere active: {hipVisualizationSphere.activeInHierarchy}");
                
                // Check distance from camera for visibility
                var arCamera = Camera.main ?? FindObjectOfType<Camera>();
                if (arCamera != null)
                {
                    float distance = Vector3.Distance(worldPosition, arCamera.transform.position);
                    Debug.Log($"[BodyTrackingRecorder] Distance from camera: {distance:F3}m");
                    
                    // Check if sphere is in front of camera
                    Vector3 relativePos = arCamera.transform.InverseTransformPoint(worldPosition);
                    Debug.Log($"[BodyTrackingRecorder] Relative to camera: {relativePos:F3} (Z should be positive)");
                }
            }
        }

        /// <summary>
        /// Get the current hip position from AR body tracking
        /// </summary>
        private Vector3? GetCurrentHipPosition()
        {
            if (!bodyManager.enabled) return null;
            
            foreach (var humanBody in bodyManager.trackables)
            {
                if (humanBody.trackingState == TrackingState.None || humanBody.trackingState == TrackingState.Limited)
                    continue;
                
                var joints = humanBody.joints;
                if (!joints.IsCreated || joints.Length == 0)
                    continue;
                
                // Find the best hip/pelvis joint - prioritize joint 2 (appears to be hip in ARKit)
                Vector3? bestJointPosition = null;
                int bestJointIndex = -1;
                
                // First, try to use joint 2 if it's tracked and has position data (hip/pelvis in ARKit)
                if (joints.Length > 2 && joints[2].tracked && joints[2].localPose.position != Vector3.zero)
                {
                    bestJointPosition = joints[2].localPose.position;
                    bestJointIndex = 2;
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
                            bestJointIndex = i;
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
                    
                    if (debugMode)
                    {
                        Debug.Log($"[BodyTrackingRecorder] Using joint {bestJointIndex}: local={bestJointPosition.Value}, world={worldPosition}, ref={referencePosition}");
                    }
                    
                    return worldPosition;
                }
                
                // Fallback: if no joints are tracked, try using body pose
                if (debugMode)
                {
                    Debug.LogWarning($"[BodyTrackingRecorder] No tracked joints found, using body pose: {humanBody.pose.position}");
                }
                return referenceFrame.TransformPoint(humanBody.pose.position);
            }
            
            return null;
        }

        void OnDestroy()
        {
            // Clean up visualization
            if (hipVisualizationSphere != null)
            {
                Destroy(hipVisualizationSphere);
            }
            
            if (bodyPoseTestSphere != null)
            {
                Destroy(bodyPoseTestSphere);
            }
        }
    }
} 