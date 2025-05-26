using UnityEngine;
using BodyTracking.Data;
using BodyTracking.Animation;

namespace BodyTracking.Playback
{
    /// <summary>
    /// Handles frame-accurate playback of recorded hip position data
    /// </summary>
    public class BodyTrackingPlayer : MonoBehaviour
    {
        [Header("Playback Settings")]
        [SerializeField] private bool loopPlayback = true;
        [SerializeField] private float playbackSpeed = 1.0f;
        [SerializeField] private bool showVisualization = true;
        
        [Header("Visualization")]
        [SerializeField] private bool showPath = true;
        [SerializeField] private int maxPathPoints = 100;
        
        [Header("Character Integration")]
        [SerializeField] private FBXCharacterController characterController;
        [SerializeField] private bool autoFindCharacterController = true;
        
        // Dependencies
        private Transform imageTargetTransform;
        private CoordinateFrame currentImageTargetFrame;
        
        // Playback state
        private HipRecording recording;
        private bool isPlaying = false;
        private float playbackStartTime;
        private float currentPlaybackTime = 0f;
        
        // Visualization
        private GameObject hipSphere;
        private LineRenderer pathLine;
        
        // Events
        public event System.Action OnPlaybackStarted;
        public event System.Action OnPlaybackStopped;
        public event System.Action OnPlaybackLooped;
        public event System.Action<float> OnPlaybackProgress; // 0-1
        
        // Public properties
        public bool IsPlaying => isPlaying;
        public float PlaybackProgress => recording != null ? Mathf.Clamp01(currentPlaybackTime / recording.duration) : 0f;
        public float CurrentTime => currentPlaybackTime;
        public float Duration => recording?.duration ?? 0f;

        /// <summary>
        /// Initialize the player with required dependencies
        /// </summary>
        public bool Initialize(Transform imageTarget)
        {
            imageTargetTransform = imageTarget;
            
            if (imageTargetTransform == null)
            {
               UnityEngine.Debug.LogError("[BodyTrackingPlayer] Image target transform is required");
                return false;
            }
            
            currentImageTargetFrame = new CoordinateFrame(imageTargetTransform);
            
            if (showVisualization)
            {
                InitializeVisualization();
            }
            
            // Setup character controller integration
            SetupCharacterController();
            
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
                   UnityEngine.Debug.Log("[BodyTrackingPlayer] Found FBXCharacterController automatically");
                }
            }
            
            if (characterController != null)
            {
                if (!characterController.IsInitialized)
                {
                    characterController.Initialize();
                }
               UnityEngine.Debug.Log("[BodyTrackingPlayer] Character controller integration enabled for playback");
            }
            else
            {
               UnityEngine.Debug.Log("[BodyTrackingPlayer] No character controller found - visualization only");
            }
        }

        /// <summary>
        /// Load a recording for playback
        /// </summary>
        public bool LoadRecording(HipRecording newRecording)
        {
            if (newRecording == null || !newRecording.IsValid)
            {
               UnityEngine.Debug.LogError("[BodyTrackingPlayer] Invalid hip recording provided");
                return false;
            }
            
            // Stop current playback if running
            if (isPlaying)
            {
                StopPlayback();
            }
            
            recording = newRecording;
            currentPlaybackTime = 0f;
            
            return true;
        }

        /// <summary>
        /// Start playback of loaded recording
        /// </summary>
        public void StartPlayback()
        {
            if (recording == null || !recording.IsValid)
            {
               UnityEngine.Debug.LogError("[BodyTrackingPlayer] No valid hip recording loaded");
                return;
            }
            
            if (isPlaying)
            {
               UnityEngine.Debug.LogWarning("[BodyTrackingPlayer] Already playing");
                return;
            }
            
            // Update current image target frame for coordinate transformation
            currentImageTargetFrame = new CoordinateFrame(imageTargetTransform);
            
            isPlaying = true;
            playbackStartTime = Time.time;
            currentPlaybackTime = 0f;
            
            // Start animation playback if character controller is available
            if (characterController != null && characterController.IsInitialized)
            {
                bool animationStarted = characterController.StartAnimationPlayback();
                if (animationStarted)
                {
                   UnityEngine.Debug.Log("[BodyTrackingPlayer] Started synchronized animation playback");
                }
                else
                {
                   UnityEngine.Debug.LogWarning("[BodyTrackingPlayer] Failed to start animation playback - continuing with hip-only playback");
                }
            }
            
            OnPlaybackStarted?.Invoke();
        }

        /// <summary>
        /// Stop playback
        /// </summary>
        public void StopPlayback()
        {
            if (!isPlaying) return;
            
            isPlaying = false;
            
            if (showVisualization)
            {
                HideVisualization();
            }
            
            // Stop animation playback if character controller is available
            if (characterController != null && characterController.IsInitialized)
            {
                characterController.StopAnimationPlayback();
               UnityEngine.Debug.Log("[BodyTrackingPlayer] Stopped synchronized animation playback");
            }
            
            OnPlaybackStopped?.Invoke();
        }

        /// <summary>
        /// Seek to specific time in the recording
        /// </summary>
        public void SeekToTime(float time)
        {
            if (recording == null) return;
            
            currentPlaybackTime = Mathf.Clamp(time, 0f, recording.duration);
            
            if (isPlaying)
            {
                playbackStartTime = Time.time - currentPlaybackTime / playbackSpeed;
            }
        }

        void Update()
        {
            if (!isPlaying || recording == null) return;
            
            // Update playback time
            float elapsedTime = (Time.time - playbackStartTime) * playbackSpeed;
            currentPlaybackTime = elapsedTime;
            
            // Handle looping
            if (currentPlaybackTime >= recording.duration)
            {
                if (loopPlayback)
                {
                    currentPlaybackTime = currentPlaybackTime % recording.duration;
                    playbackStartTime = Time.time - currentPlaybackTime / playbackSpeed;
                    OnPlaybackLooped?.Invoke();
                    
                    // Restart animation for loop
                    if (characterController != null && characterController.IsInitialized)
                    {
                        characterController.StartAnimationPlayback();
                    }
                }
                else
                {
                    StopPlayback();
                    return;
                }
            }
            
            // Get current frame
            var currentFrame = recording.GetFrameAtTime(currentPlaybackTime);
            
            // Update visualization
            if (showVisualization && currentFrame.IsValid && currentFrame.hipJoint.IsValid)
            {
                UpdateVisualization(currentFrame);
            }
            
            // Notify progress
            OnPlaybackProgress?.Invoke(PlaybackProgress);
        }

        /// <summary>
        /// Initialize visualization components
        /// </summary>
        private void InitializeVisualization()
        {
            // Create hip sphere
            hipSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hipSphere.name = "PlaybackHipSphere";
            hipSphere.transform.localScale = Vector3.one * 0.15f;
            
            // Setup material - blue for playback
            var renderer = hipSphere.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Unlit/Color"));
                material.color = Color.blue;
                renderer.material = material;
            }
            
            // Remove collider
            if (hipSphere.TryGetComponent<Collider>(out var collider))
            {
                Destroy(collider);
            }
            
            hipSphere.SetActive(false);
            
            // Create path line renderer if enabled
            if (showPath)
            {
                var pathLineObject = new GameObject("HipPathLine");
                pathLine = pathLineObject.AddComponent<LineRenderer>();
                
                pathLine.material = new Material(Shader.Find("Sprites/Default"));
                pathLine.startColor = Color.cyan;
                pathLine.endColor = Color.cyan;
                pathLine.startWidth = 0.03f;
                pathLine.endWidth = 0.03f;
                pathLine.positionCount = 0;
                pathLine.useWorldSpace = true;
            }
        }

        /// <summary>
        /// Update visualization for current frame
        /// </summary>
        private void UpdateVisualization(HipFrame frame)
        {
            // Transform hip position to current coordinate system
            Vector3 worldPosition = TransformHipToCurrentSpace(frame.hipJoint.position);
            
            // Update hip sphere
            if (hipSphere != null)
            {
                hipSphere.transform.position = worldPosition;
                hipSphere.SetActive(true);
            }
            
            // Update path line
            if (showPath && pathLine != null)
            {
                UpdatePathLine(worldPosition);
            }
            
            // Update character position if controller is available
            if (characterController != null && characterController.IsInitialized)
            {
                characterController.SetTargetHipPosition(worldPosition);
            }
        }

        /// <summary>
        /// Transform recorded hip position to current image target space
        /// </summary>
        private Vector3 TransformHipToCurrentSpace(Vector3 recordedPosition)
        {
            // Transform from recorded reference space to current world space
            return currentImageTargetFrame.TransformPoint(recordedPosition);
        }

        /// <summary>
        /// Update the path line with new position
        /// </summary>
        private void UpdatePathLine(Vector3 newPosition)
        {
            if (pathLine.positionCount >= maxPathPoints)
            {
                // Shift points back to make room for new point
                Vector3[] positions = new Vector3[pathLine.positionCount];
                pathLine.GetPositions(positions);
                
                for (int i = 0; i < positions.Length - 1; i++)
                {
                    positions[i] = positions[i + 1];
                }
                positions[positions.Length - 1] = newPosition;
                pathLine.SetPositions(positions);
            }
            else
            {
                // Add new point
                pathLine.positionCount++;
                pathLine.SetPosition(pathLine.positionCount - 1, newPosition);
            }
        }

        /// <summary>
        /// Hide visualization elements
        /// </summary>
        private void HideVisualization()
        {
            if (hipSphere != null)
            {
                hipSphere.SetActive(false);
            }
            
            if (pathLine != null)
            {
                pathLine.positionCount = 0;
            }
        }

        void OnDestroy()
        {
            // Clean up visualization
            if (hipSphere != null)
            {
                Destroy(hipSphere);
            }
            
            if (pathLine != null)
            {
                Destroy(pathLine.gameObject);
            }
        }
    }
} 