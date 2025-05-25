using UnityEngine;
using UnityEngine.XR.ARFoundation;
using BodyTracking.Data;
using BodyTracking.Recording;
using BodyTracking.Playback;
using BodyTracking.Storage;
using BodyTracking.AR;

namespace BodyTracking
{
    /// <summary>
    /// Main controller for hip tracking recording and playback system
    /// Provides a clean interface and coordinates all modular components
    /// </summary>
    public class BodyTrackingController : MonoBehaviour
    {
        [Header("Dependencies")]
        public ARHumanBodyManager humanBodyManager;
        public ARImageTargetManager imageTargetManager;
        
        [Header("Components")]
        public BodyTrackingRecorder recorder;
        public BodyTrackingPlayer player;
        
        [Header("Settings")]
        public bool autoInitialize = true;
        public bool debugMode = false;
        
        [Header("UI")]
        public TMPro.TextMeshProUGUI statusText;
        
        // State
        private bool isInitialized = false;
        private OperationMode currentMode = OperationMode.Ready;
        private HipRecording lastRecording;
        
        // Events
        public event System.Action<OperationMode> OnModeChanged;
        public event System.Action<HipRecording> OnRecordingComplete;
        public event System.Action OnPlaybackStarted;
        public event System.Action OnPlaybackStopped;
        
        // Public Properties
        public bool IsInitialized => isInitialized;
        public OperationMode CurrentMode => currentMode;
        public bool CanRecord => isInitialized && imageTargetManager.IsImageDetected && currentMode == OperationMode.Ready;
        public bool CanPlayback => isInitialized && imageTargetManager.IsImageDetected && lastRecording != null && currentMode == OperationMode.Ready;
        public bool IsRecording => currentMode == OperationMode.Recording;
        public bool IsPlaying => currentMode == OperationMode.Playing;

        void Start()
        {
            if (autoInitialize)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Initialize the hip tracking system
        /// </summary>
        public bool Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[BodyTrackingController] Already initialized");
                return true;
            }
            
            // Validate dependencies
            if (!ValidateDependencies())
            {
                return false;
            }
            
            // Setup components
            SetupComponents();
            
            // Subscribe to events
            SubscribeToEvents();
            
            isInitialized = true;
            UpdateStatus("Hip tracking system initialized - waiting for image target");
            
            Debug.Log("[BodyTrackingController] Hip tracking system successfully initialized");
            return true;
        }

        /// <summary>
        /// Start recording hip tracking data
        /// </summary>
        public bool StartRecording()
        {
            if (!CanRecord)
            {
                Debug.LogWarning($"[BodyTrackingController] Cannot start hip recording - CanRecord: {CanRecord}");
                UpdateStatus("Cannot start hip recording - check requirements");
                return false;
            }
            
            if (recorder.StartRecording())
            {
                SetMode(OperationMode.Recording);
                UpdateStatus("Recording hip position...");
                Debug.Log("[BodyTrackingController] Started hip recording");
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Stop recording and save the data
        /// </summary>
        public HipRecording StopRecording()
        {
            if (currentMode != OperationMode.Recording)
            {
                Debug.LogWarning("[BodyTrackingController] Not currently recording");
                return null;
            }
            
            lastRecording = recorder.StopRecording();
            
            if (lastRecording != null)
            {
                // Auto-save the recording
                string fileName = $"hip_recording_{System.DateTime.Now:yyyyMMdd_HHmmss}";
                if (RecordingStorage.SaveRecording(lastRecording, fileName))
                {
                    Debug.Log($"[BodyTrackingController] Hip recording saved: {fileName}");
                    UpdateStatus($"Hip recording saved: {lastRecording.FrameCount} frames");
                }
                else
                {
                    Debug.LogError("[BodyTrackingController] Failed to save hip recording");
                    UpdateStatus("Hip recording completed but save failed");
                }
                
                OnRecordingComplete?.Invoke(lastRecording);
            }
            
            SetMode(OperationMode.Ready);
            return lastRecording;
        }

        /// <summary>
        /// Start playback of the last recorded data
        /// </summary>
        public bool StartPlayback()
        {
            return StartPlayback(lastRecording);
        }

        /// <summary>
        /// Start playback of specific recording
        /// </summary>
        public bool StartPlayback(HipRecording recording)
        {
            if (!CanPlayback)
            {
                Debug.LogWarning($"[BodyTrackingController] Cannot start hip playback - CanPlayback: {CanPlayback}");
                UpdateStatus("Cannot start hip playback - check requirements");
                return false;
            }
            
            if (recording == null)
            {
                Debug.LogWarning("[BodyTrackingController] No hip recording provided for playback");
                UpdateStatus("No hip recording available for playback");
                return false;
            }
            
            if (player.LoadRecording(recording))
            {
                player.StartPlayback();
                SetMode(OperationMode.Playing);
                UpdateStatus($"Playing back hip movement: {recording.FrameCount} frames");
                Debug.Log("[BodyTrackingController] Started hip playback");
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Stop current playback
        /// </summary>
        public void StopPlayback()
        {
            if (currentMode != OperationMode.Playing)
            {
                Debug.LogWarning("[BodyTrackingController] Not currently playing");
                return;
            }
            
            player.StopPlayback();
            SetMode(OperationMode.Ready);
            UpdateStatus("Hip playback stopped");
            Debug.Log("[BodyTrackingController] Stopped hip playback");
        }

        /// <summary>
        /// Load a recording from storage
        /// </summary>
        public bool LoadRecording(string fileName)
        {
            var recording = RecordingStorage.LoadRecording(fileName);
            if (recording != null)
            {
                lastRecording = recording;
                UpdateStatus($"Loaded hip recording: {recording.FrameCount} frames");
                Debug.Log($"[BodyTrackingController] Loaded hip recording: {fileName}");
                return true;
            }
            
            UpdateStatus($"Failed to load hip recording: {fileName}");
            return false;
        }

        /// <summary>
        /// Get list of available recordings
        /// </summary>
        public System.Collections.Generic.List<string> GetAvailableRecordings()
        {
            return RecordingStorage.GetAvailableRecordings();
        }

        /// <summary>
        /// Get recording metadata
        /// </summary>
        public RecordingMetadata GetRecordingMetadata(string fileName)
        {
            return RecordingStorage.GetRecordingMetadata(fileName);
        }

        #region Private Methods

        private bool ValidateDependencies()
        {
            if (humanBodyManager == null)
            {
                humanBodyManager = FindObjectOfType<ARHumanBodyManager>();
                if (humanBodyManager == null)
                {
                    Debug.LogError("[BodyTrackingController] ARHumanBodyManager not found");
                    UpdateStatus("Error: ARHumanBodyManager missing");
                    return false;
                }
            }
            
            if (imageTargetManager == null)
            {
                imageTargetManager = FindObjectOfType<ARImageTargetManager>();
                if (imageTargetManager == null)
                {
                    Debug.LogError("[BodyTrackingController] ARImageTargetManager not found");
                    UpdateStatus("Error: ARImageTargetManager missing");
                    return false;
                }
            }
            
            return true;
        }

        private void SetupComponents()
        {
            // Setup recorder
            if (recorder == null)
            {
                recorder = gameObject.AddComponent<BodyTrackingRecorder>();
            }
            
            // Setup player
            if (player == null)
            {
                player = gameObject.AddComponent<BodyTrackingPlayer>();
            }
            
            // Initialize components if image target is available
            if (imageTargetManager.IsImageDetected)
            {
                InitializeRecorderAndPlayer();
            }
            // Otherwise, they will be initialized when image target is detected
        }
        
        private void InitializeRecorderAndPlayer()
        {
            var imageTarget = imageTargetManager.ImageTargetTransform;
            if (imageTarget != null)
            {
                recorder.Initialize(humanBodyManager, imageTarget);
                player.Initialize(imageTarget);
                Debug.Log("[BodyTrackingController] Recorder and player initialized with image target");
            }
        }

        private void SubscribeToEvents()
        {
            // Image target events
            imageTargetManager.OnImageTargetDetected += OnImageTargetDetected;
            imageTargetManager.OnImageTargetLost += OnImageTargetLost;
            
            // Recorder events
            recorder.OnRecordingComplete += OnRecorderComplete;
            recorder.OnRecordingProgress += OnRecorderProgress;
            
            // Player events
            player.OnPlaybackStarted += OnPlayerStarted;
            player.OnPlaybackStopped += OnPlayerStopped;
            player.OnPlaybackProgress += OnPlayerProgress;
        }

        private void SetMode(OperationMode newMode)
        {
            if (currentMode != newMode)
            {
                currentMode = newMode;
                OnModeChanged?.Invoke(currentMode);
                
                if (debugMode)
                {
                    Debug.Log($"[BodyTrackingController] Mode changed to: {currentMode}");
                }
            }
        }

        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            
            if (debugMode)
            {
                Debug.Log($"[BodyTrackingController] Status: {message}");
            }
        }

        #endregion

        #region Event Handlers

        private void OnImageTargetDetected(Transform imageTarget)
        {
            UpdateStatus("Image target detected - ready for hip tracking");
            
            // Initialize recorder and player with the detected image target
            InitializeRecorderAndPlayer();
        }

        private void OnImageTargetLost()
        {
            UpdateStatus("Image target lost");
            
            // Stop any ongoing operations
            if (IsRecording)
            {
                StopRecording();
            }
            
            if (IsPlaying)
            {
                StopPlayback();
            }
        }

        private void OnRecorderComplete(HipRecording recording)
        {
            // This is handled in StopRecording()
        }

        private void OnRecorderProgress(float time)
        {
            if (statusText != null)
            {
                statusText.text = $"Recording hip position... {time:F1}s";
            }
        }

        private void OnPlayerStarted()
        {
            OnPlaybackStarted?.Invoke();
        }

        private void OnPlayerStopped()
        {
            SetMode(OperationMode.Ready);
            OnPlaybackStopped?.Invoke();
        }

        private void OnPlayerProgress(float progress)
        {
            if (statusText != null && lastRecording != null)
            {
                statusText.text = $"Playing hip movement... {progress * 100:F0}%";
            }
        }

        #endregion

        void OnDestroy()
        {
            // Unsubscribe from events
            if (imageTargetManager != null)
            {
                imageTargetManager.OnImageTargetDetected -= OnImageTargetDetected;
                imageTargetManager.OnImageTargetLost -= OnImageTargetLost;
            }
            
            if (recorder != null)
            {
                recorder.OnRecordingComplete -= OnRecorderComplete;
                recorder.OnRecordingProgress -= OnRecorderProgress;
            }
            
            if (player != null)
            {
                player.OnPlaybackStarted -= OnPlayerStarted;
                player.OnPlaybackStopped -= OnPlayerStopped;
                player.OnPlaybackProgress -= OnPlayerProgress;
            }
        }
    }

    /// <summary>
    /// Operation modes for the hip tracking system
    /// </summary>
    public enum OperationMode
    {
        Ready,      // System ready, waiting for commands
        Recording,  // Currently recording
        Playing     // Currently playing back
    }
} 