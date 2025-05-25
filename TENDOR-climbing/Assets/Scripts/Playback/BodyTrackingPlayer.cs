using UnityEngine;
using BodyTracking.Data;

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
        [SerializeField] private bool debugMode = false;
        
        [Header("Visualization")]
        [SerializeField] private bool showPath = true;
        [SerializeField] private int maxPathPoints = 100;
        
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
                Debug.LogError("[BodyTrackingPlayer] Image target transform is required");
                return false;
            }
            
            currentImageTargetFrame = new CoordinateFrame(imageTargetTransform);
            
            if (showVisualization)
            {
                InitializeVisualization();
            }
            
            Debug.Log("[BodyTrackingPlayer] Successfully initialized for hip playback");
            return true;
        }

        /// <summary>
        /// Load a recording for playback
        /// </summary>
        public bool LoadRecording(HipRecording newRecording)
        {
            if (newRecording == null || !newRecording.IsValid)
            {
                Debug.LogError("[BodyTrackingPlayer] Invalid hip recording provided");
                return false;
            }
            
            // Stop current playback if running
            if (isPlaying)
            {
                StopPlayback();
            }
            
            recording = newRecording;
            currentPlaybackTime = 0f;
            
            Debug.Log($"[BodyTrackingPlayer] Loaded hip recording: {recording.FrameCount} frames, {recording.duration:F2}s");
            return true;
        }

        /// <summary>
        /// Start playback of loaded recording
        /// </summary>
        public void StartPlayback()
        {
            if (recording == null || !recording.IsValid)
            {
                Debug.LogError("[BodyTrackingPlayer] No valid hip recording loaded");
                return;
            }
            
            if (isPlaying)
            {
                Debug.LogWarning("[BodyTrackingPlayer] Already playing");
                return;
            }
            
            // Update current image target frame for coordinate transformation
            currentImageTargetFrame = new CoordinateFrame(imageTargetTransform);
            
            isPlaying = true;
            playbackStartTime = Time.time;
            currentPlaybackTime = 0f;
            
            Debug.Log("[BodyTrackingPlayer] Started hip playback");
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
            
            Debug.Log("[BodyTrackingPlayer] Stopped hip playback");
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
            
            if (debugMode && Time.frameCount % 30 == 0)
            {
                Debug.Log($"[BodyTrackingPlayer] Hip playback: {currentPlaybackTime:F2}s / {recording.duration:F2}s ({PlaybackProgress * 100:F1}%)");
            }
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
                var material = new Material(Shader.Find("Standard"));
                material.color = Color.blue;
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.blue * 2f);
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
            
            Debug.Log("[BodyTrackingPlayer] Initialized hip visualization");
        }

        /// <summary>
        /// Update visualization for current frame
        /// </summary>
        private void UpdateVisualization(HipFrame frame)
        {
            if (!frame.hipJoint.IsValid) return;
            
            // Transform from recording space to current world space
            Vector3 worldPosition = TransformHipToCurrentSpace(frame.hipJoint.position);
            
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
        }

        /// <summary>
        /// Transform hip position from recording space to current world space
        /// </summary>
        private Vector3 TransformHipToCurrentSpace(Vector3 recordedPosition)
        {
            // Transform from recording reference frame to current image target frame
            return currentImageTargetFrame.TransformPoint(recordedPosition);
        }

        /// <summary>
        /// Update path line for hip movement
        /// </summary>
        private void UpdatePathLine(Vector3 newPosition)
        {
            if (pathLine == null) return;
            
            // Add new point to path
            int currentPoints = pathLine.positionCount;
            
            if (currentPoints < maxPathPoints)
            {
                pathLine.positionCount = currentPoints + 1;
                pathLine.SetPosition(currentPoints, newPosition);
            }
            else
            {
                // Shift points and add new one
                Vector3[] positions = new Vector3[maxPathPoints];
                pathLine.GetPositions(positions);
                
                for (int i = 0; i < maxPathPoints - 1; i++)
                {
                    positions[i] = positions[i + 1];
                }
                positions[maxPathPoints - 1] = newPosition;
                
                pathLine.SetPositions(positions);
            }
        }

        /// <summary>
        /// Hide all visualization elements
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
            // Clean up visualization objects
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