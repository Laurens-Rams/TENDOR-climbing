using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using BodyTracking.Data;
using BodyTracking.Animation;
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
            
           UnityEngine.Debug.Log($"[BodyTrackingRecorder] Initialized - showVisualization: {showVisualization}, targetFrameRate: {targetFrameRate}");
            return true;
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
               UnityEngine.Debug.Log("[BodyTrackingRecorder] Character controller integration enabled");
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
                    
                    // Update visualization
                    if (showVisualization)
                    {
                        UpdateVisualization(worldPosition);
                    }
                }
                
                break; // Use first tracked body only
            }
            
            // Debug logging for tracking issues
            if (!foundValidJoint && currentRecording.FrameCount % 90 == 0) // Reduced frequency from 30 to 90
            {
               UnityEngine.Debug.LogWarning($"[BodyTrackingRecorder] No valid joint data found at frame {currentRecording.FrameCount}");
            }
            
            // Always add frame (even if no body detected) to maintain timing
            currentRecording.frames.Add(frame);
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