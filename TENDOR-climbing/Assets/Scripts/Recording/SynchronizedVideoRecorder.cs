using UnityEngine;
using RenderHeads.Media.AVProMovieCapture;
using BodyTracking.Data;
using BodyTracking.Recording;
using BodyTracking.Storage;
using System;
using System.IO;
using System.Linq;

namespace BodyTracking.Recording
{
    /// <summary>
    /// Synchronized video and hip tracking recorder using AVPro Movie Capture
    /// Records video at 30fps with matching timestamps to hip tracking data
    /// </summary>
    [RequireComponent(typeof(CaptureFromCamera))]
    public class SynchronizedVideoRecorder : MonoBehaviour
    {
        [Header("Video Recording Settings")]
        [SerializeField] private Camera recordingCamera;
        [SerializeField] private bool autoFindCamera = true;
        [SerializeField] private string videoOutputFolder = "TENDOR_Recordings";
        [SerializeField] private CaptureBase.Resolution videoResolution = CaptureBase.Resolution.POW2_2048x4096;
        [SerializeField] private float videoFrameRate = 30f;
        [SerializeField] private bool useH264Codec = true;
        
        [Header("UI Exclusion Settings")]
        [SerializeField] private bool excludeUIFromRecording = true;
        [SerializeField] private LayerMask uiLayersToExclude = 1 << 5; // Default: exclude UI layer (layer 5)
        [SerializeField] private bool createDedicatedRecordingCamera = false;
        [SerializeField] private string recordingCameraName = "VideoRecordingCamera";
        
        [Header("Synchronization")]
        [SerializeField] private BodyTrackingRecorder hipRecorder;
        [SerializeField] private bool autoFindHipRecorder = true;
        [SerializeField] private bool recordAudio = false;
        
        [Header("File Naming")]
        [SerializeField] private string filePrefix = "TENDOR";
        [SerializeField] private bool includeTimestamp = true;
        
        [Header("JSON Storage")]
        [SerializeField] private bool autoSaveHipDataAsJSON = true;
        [SerializeField] private RecordingStorage.StorageFormat storageFormat = RecordingStorage.StorageFormat.JSON;
        
        // Components
        private CaptureFromCamera videoCapture;
        private Camera dedicatedRecordingCamera;
        
        // Recording state
        private bool isRecording = false;
        private string currentSessionId;
        private string currentVideoPath;
        private DateTime recordingStartTime;
        
        // UI exclusion state
        private int originalCullingMask = -1;
        private bool cullingMaskModified = false;
        
        // Events
        public event System.Action<string> OnVideoRecordingStarted;
        public event System.Action<string, HipRecording> OnSynchronizedRecordingComplete;
        public event System.Action<string> OnVideoRecordingFailed;
        
        // Public properties
        public bool IsRecording => isRecording;
        public string CurrentVideoPath => currentVideoPath;
        public string OutputFolder => GetOutputFolderPath();

        void Awake()
        {
            // Get or add AVPro capture component
            videoCapture = GetComponent<CaptureFromCamera>();
            if (videoCapture == null)
            {
                videoCapture = gameObject.AddComponent<CaptureFromCamera>();
            }
        }

        void Start()
        {
            InitializeComponents();
            SetupVideoCapture();
        }

        /// <summary>
        /// Initialize required components
        /// </summary>
        private void InitializeComponents()
        {
            // Find recording camera
            if (recordingCamera == null && autoFindCamera)
            {
                recordingCamera = Camera.main;
                if (recordingCamera == null)
                {
                    recordingCamera = FindFirstObjectByType<Camera>();
                }
            }
            
            // Create dedicated recording camera if requested
            if (createDedicatedRecordingCamera && excludeUIFromRecording)
            {
                SetupDedicatedRecordingCamera();
            }
            
            // Find hip recorder
            if (hipRecorder == null && autoFindHipRecorder)
            {
                hipRecorder = FindFirstObjectByType<BodyTrackingRecorder>();
            }
            
            if (recordingCamera == null && dedicatedRecordingCamera == null)
            {
                Debug.LogError("[SynchronizedVideoRecorder] No recording camera found!");
                return;
            }
            
            if (hipRecorder == null)
            {
                Debug.LogError("[SynchronizedVideoRecorder] No hip recorder found!");
                return;
            }
            
            Camera activeRecordingCamera = dedicatedRecordingCamera != null ? dedicatedRecordingCamera : recordingCamera;
            Debug.Log($"[SynchronizedVideoRecorder] Initialized with camera: {activeRecordingCamera.name}");
            Debug.Log($"[SynchronizedVideoRecorder] UI exclusion: {excludeUIFromRecording}");
        }

        /// <summary>
        /// Setup dedicated recording camera that excludes UI elements
        /// </summary>
        private void SetupDedicatedRecordingCamera()
        {
            // Find or create dedicated recording camera
            GameObject existingCameraObj = GameObject.Find(recordingCameraName);
            if (existingCameraObj != null)
            {
                dedicatedRecordingCamera = existingCameraObj.GetComponent<Camera>();
            }
            
            if (dedicatedRecordingCamera == null)
            {
                // Create new camera object
                GameObject cameraObj = new GameObject(recordingCameraName);
                dedicatedRecordingCamera = cameraObj.AddComponent<Camera>();
                
                // Copy settings from main camera
                if (recordingCamera != null)
                {
                    CopyCameraSettings(recordingCamera, dedicatedRecordingCamera);
                }
                else
                {
                    // Use default AR camera settings
                    dedicatedRecordingCamera.clearFlags = CameraClearFlags.SolidColor;
                    dedicatedRecordingCamera.backgroundColor = Color.black;
                    dedicatedRecordingCamera.fieldOfView = 60f;
                    dedicatedRecordingCamera.nearClipPlane = 0.1f;
                    dedicatedRecordingCamera.farClipPlane = 1000f;
                }
                
                Debug.Log($"[SynchronizedVideoRecorder] Created dedicated recording camera: {recordingCameraName}");
            }
            
            // Configure UI exclusion
            if (excludeUIFromRecording)
            {
                // Simple solution: Just exclude the UI layer (layer 5)
                originalCullingMask = dedicatedRecordingCamera.cullingMask;
                dedicatedRecordingCamera.cullingMask = originalCullingMask & ~(1 << 5);
                
                Debug.Log($"[SynchronizedVideoRecorder] UI exclusion configured - Original mask: {originalCullingMask}, New mask: {dedicatedRecordingCamera.cullingMask}");
                Debug.Log($"[SynchronizedVideoRecorder] UI layer (5) excluded from dedicated camera");
            }
            
            // Disable the camera by default (AVPro will control rendering)
            dedicatedRecordingCamera.enabled = false;
        }

        /// <summary>
        /// Copy camera settings from source to target camera
        /// </summary>
        private void CopyCameraSettings(Camera source, Camera target)
        {
            target.clearFlags = source.clearFlags;
            target.backgroundColor = source.backgroundColor;
            target.cullingMask = source.cullingMask;
            target.orthographic = source.orthographic;
            target.fieldOfView = source.fieldOfView;
            target.orthographicSize = source.orthographicSize;
            target.nearClipPlane = source.nearClipPlane;
            target.farClipPlane = source.farClipPlane;
            target.rect = source.rect;
            target.depth = source.depth;
            target.renderingPath = source.renderingPath;
            target.useOcclusionCulling = source.useOcclusionCulling;
            target.allowHDR = source.allowHDR;
            target.allowMSAA = source.allowMSAA;
            
            // Copy transform
            target.transform.position = source.transform.position;
            target.transform.rotation = source.transform.rotation;
            target.transform.localScale = source.transform.localScale;
        }

        /// <summary>
        /// Setup AVPro video capture settings
        /// </summary>
        private void SetupVideoCapture()
        {
            if (videoCapture == null) return;
            
            // Determine which camera to use for recording
            Camera cameraToUse = dedicatedRecordingCamera != null ? dedicatedRecordingCamera : recordingCamera;
            
            // Configure UI exclusion for existing camera if not using dedicated camera
            if (dedicatedRecordingCamera == null && excludeUIFromRecording && recordingCamera != null)
            {
                // Temporarily modify the main camera's culling mask during recording
                // Note: This will be restored when recording stops
                Debug.Log($"[SynchronizedVideoRecorder] Will exclude UI layers from main camera during recording");
            }
            
            // Basic settings
            videoCapture.SetCamera(cameraToUse, false);
            videoCapture.FrameRate = videoFrameRate;
            videoCapture.IsRealTime = true;
            videoCapture.CameraRenderResolution = videoResolution;
            
            // Audio settings
            if (recordAudio)
            {
                videoCapture.AudioCaptureSource = AudioCaptureSource.Unity;
            }
            else
            {
                videoCapture.AudioCaptureSource = AudioCaptureSource.None;
            }
            
            // Quality settings for AR Remote compatibility
            videoCapture.AllowOfflineVSyncDisable = true;
            
            // Output settings
            videoCapture.OutputTarget = OutputTarget.VideoFile;
            
            // Subscribe to events
            videoCapture.OnCaptureStart.AddListener(OnVideoCaptureStarted);
            videoCapture.CompletedFileWritingAction += OnVideoCaptureCompleted;
            
            Debug.Log($"[SynchronizedVideoRecorder] Video capture configured: {videoFrameRate}fps, {videoResolution}");
            Debug.Log($"[SynchronizedVideoRecorder] Recording camera: {cameraToUse.name}");
        }

        /// <summary>
        /// Apply UI exclusion settings before recording
        /// </summary>
        private void ApplyUIExclusion()
        {
            if (!excludeUIFromRecording) return;
            
            Camera cameraToModify = dedicatedRecordingCamera != null ? dedicatedRecordingCamera : recordingCamera;
            
            if (cameraToModify != null && !cullingMaskModified)
            {
                originalCullingMask = cameraToModify.cullingMask;
                
                // Simple solution: Just exclude the UI layer (layer 5)
                cameraToModify.cullingMask = originalCullingMask & ~(1 << 5);
                cullingMaskModified = true;
                
                Debug.Log($"[SynchronizedVideoRecorder] Applied UI exclusion - Original mask: {originalCullingMask}, New mask: {cameraToModify.cullingMask}");
                Debug.Log($"[SynchronizedVideoRecorder] UI layer (5) excluded from recording");
            }
        }

        /// <summary>
        /// Restore original camera settings after recording
        /// </summary>
        private void RestoreUISettings()
        {
            if (!cullingMaskModified) return;
            
            Camera cameraToRestore = dedicatedRecordingCamera != null ? dedicatedRecordingCamera : recordingCamera;
            
            if (cameraToRestore != null && originalCullingMask != -1)
            {
                cameraToRestore.cullingMask = originalCullingMask;
                cullingMaskModified = false;
                
                Debug.Log($"[SynchronizedVideoRecorder] Restored original culling mask: {originalCullingMask}");
            }
        }

        /// <summary>
        /// Get configuration summary for debugging
        /// </summary>
        public string GetUIExclusionSummary()
        {
            var summary = $"UI Exclusion: {excludeUIFromRecording}\n";
            summary += $"Dedicated Camera: {(dedicatedRecordingCamera != null ? dedicatedRecordingCamera.name : "None")}\n";
            summary += $"UI Layers to Exclude: {uiLayersToExclude}\n";
            
            Camera activeCamera = dedicatedRecordingCamera != null ? dedicatedRecordingCamera : recordingCamera;
            if (activeCamera != null)
            {
                summary += $"Active Camera: {activeCamera.name}\n";
                summary += $"Current Culling Mask: {activeCamera.cullingMask}\n";
            }
            
            return summary;
        }

        /// <summary>
        /// Configure UI exclusion settings at runtime
        /// </summary>
        public void SetUIExclusionSettings(bool excludeUI, LayerMask layersToExclude = default)
        {
            excludeUIFromRecording = excludeUI;
            if (layersToExclude != default)
            {
                uiLayersToExclude = layersToExclude;
            }
            
            Debug.Log($"[SynchronizedVideoRecorder] UI exclusion updated: {excludeUI}, Layers: {uiLayersToExclude}");
        }

        /// <summary>
        /// Enable/disable dedicated recording camera at runtime
        /// </summary>
        public void SetDedicatedCameraMode(bool useDedicatedCamera)
        {
            if (isRecording)
            {
                Debug.LogWarning("[SynchronizedVideoRecorder] Cannot change camera mode during recording");
                return;
            }
            
            createDedicatedRecordingCamera = useDedicatedCamera;
            
            if (useDedicatedCamera && excludeUIFromRecording)
            {
                SetupDedicatedRecordingCamera();
                SetupVideoCapture(); // Reconfigure with new camera
            }
            
            Debug.Log($"[SynchronizedVideoRecorder] Dedicated camera mode: {useDedicatedCamera}");
        }

        /// <summary>
        /// Immediately exclude UI from the current camera (for testing)
        /// </summary>
        [ContextMenu("Test: Exclude UI Now")]
        public void TestExcludeUINow()
        {
            Camera testCamera = dedicatedRecordingCamera != null ? dedicatedRecordingCamera : recordingCamera;
            if (testCamera != null)
            {
                int originalMask = testCamera.cullingMask;
                testCamera.cullingMask = originalMask & ~(1 << 5); // Exclude UI layer
                Debug.Log($"[SynchronizedVideoRecorder] TEST: UI excluded from {testCamera.name}");
                Debug.Log($"[SynchronizedVideoRecorder] TEST: Original mask: {originalMask}, New mask: {testCamera.cullingMask}");
            }
        }

        /// <summary>
        /// Restore UI to the current camera (for testing)
        /// </summary>
        [ContextMenu("Test: Restore UI Now")]
        public void TestRestoreUINow()
        {
            Camera testCamera = dedicatedRecordingCamera != null ? dedicatedRecordingCamera : recordingCamera;
            if (testCamera != null)
            {
                testCamera.cullingMask = testCamera.cullingMask | (1 << 5); // Include UI layer
                Debug.Log($"[SynchronizedVideoRecorder] TEST: UI restored to {testCamera.name}");
                Debug.Log($"[SynchronizedVideoRecorder] TEST: New mask: {testCamera.cullingMask}");
            }
        }

        /// <summary>
        /// Start synchronized recording of video and hip tracking
        /// </summary>
        public bool StartSynchronizedRecording()
        {
            if (isRecording)
            {
                Debug.LogWarning("[SynchronizedVideoRecorder] Already recording");
                return false;
            }
            
            if (videoCapture == null || hipRecorder == null)
            {
                Debug.LogError("[SynchronizedVideoRecorder] Missing required components");
                return false;
            }
            
            // Generate session ID
            currentSessionId = GenerateSessionId();
            
            // Ensure output directory exists (AVPro will handle the actual path)
            string outputDir = GetOutputFolderPath();
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
                Debug.Log($"[SynchronizedVideoRecorder] Created output directory: {outputDir}");
            }
            
            // Setup video file path - AVPro will generate the actual filename
            videoCapture.AppendFilenameTimestamp = true;
            videoCapture.FilenamePrefix = filePrefix;
            videoCapture.OutputFolder = CaptureBase.OutputPath.RelativeToPersistentData;
            videoCapture.OutputFolderPath = videoOutputFolder;
            videoCapture.FilenameExtension = ".mp4";
            
            recordingStartTime = DateTime.Now;
            
            // Apply UI exclusion settings
            ApplyUIExclusion();
            
            // Start hip recording first
            if (!hipRecorder.StartRecording())
            {
                Debug.LogError("[SynchronizedVideoRecorder] Failed to start hip recording");
                return false;
            }
            
            // Start video recording
            if (!videoCapture.StartCapture())
            {
                Debug.LogError("[SynchronizedVideoRecorder] Failed to start video recording");
                hipRecorder.StopRecording(); // Stop hip recording if video fails
                return false;
            }
            
            isRecording = true;
            
            // The actual video path will be available after recording starts
            currentVideoPath = ""; // Will be updated when recording completes
            
            Debug.Log($"[SynchronizedVideoRecorder] Started synchronized recording: {currentSessionId}");
            OnVideoRecordingStarted?.Invoke(currentSessionId); // Pass session ID instead of path
            
            return true;
        }

        /// <summary>
        /// Stop synchronized recording
        /// </summary>
        public SynchronizedRecordingResult StopSynchronizedRecording()
        {
            if (!isRecording)
            {
                Debug.LogWarning("[SynchronizedVideoRecorder] Not currently recording");
                return null;
            }
            
            isRecording = false;
            
            // Restore UI settings
            RestoreUISettings();
            
            // Stop video recording
            videoCapture.StopCapture();
            
            // Get the actual video file path from AVPro
            currentVideoPath = videoCapture.LastFilePath;
            
            // Stop hip recording and get data
            HipRecording hipData = hipRecorder.StopRecording();
            
            if (hipData == null)
            {
                Debug.LogError("[SynchronizedVideoRecorder] Failed to get hip recording data");
                return null;
            }
            
            // Auto-save hip data as JSON if enabled
            string hipDataPath = "";
            if (autoSaveHipDataAsJSON)
            {
                if (RecordingStorage.SaveRecording(hipData, currentSessionId, storageFormat))
                {
                    hipDataPath = Path.Combine(Application.persistentDataPath, "BodyTrackingRecordings", currentSessionId + ".json");
                    Debug.Log($"[SynchronizedVideoRecorder] Hip data saved as JSON: {hipDataPath}");
                }
                else
                {
                    Debug.LogError("[SynchronizedVideoRecorder] Failed to save hip data as JSON");
                }
            }
            
            // Create result
            var result = new SynchronizedRecordingResult
            {
                sessionId = currentSessionId,
                videoFilePath = currentVideoPath,
                hipDataPath = hipDataPath,
                hipRecording = hipData,
                recordingStartTime = recordingStartTime,
                recordingEndTime = DateTime.Now,
                videoFrameRate = videoFrameRate,
                hipFrameRate = hipData.frameRate
            };
            
            Debug.Log($"[SynchronizedVideoRecorder] Synchronized recording completed: {result.sessionId}");
            Debug.Log($"[SynchronizedVideoRecorder] Video: {result.videoFilePath}");
            Debug.Log($"[SynchronizedVideoRecorder] Hip JSON: {result.hipDataPath}");
            Debug.Log($"[SynchronizedVideoRecorder] Hip frames: {hipData.FrameCount}");
            Debug.Log($"[SynchronizedVideoRecorder] Duration: {hipData.duration:F2}s");
            
            OnSynchronizedRecordingComplete?.Invoke(currentVideoPath, hipData);
            
            return result;
        }

        /// <summary>
        /// Generate unique session ID
        /// </summary>
        private string GenerateSessionId()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return includeTimestamp ? $"{filePrefix}_{timestamp}" : $"{filePrefix}_{Guid.NewGuid().ToString("N")[..8]}";
        }

        /// <summary>
        /// Get full output folder path
        /// </summary>
        private string GetOutputFolderPath()
        {
            return Path.Combine(Application.persistentDataPath, videoOutputFolder);
        }

        /// <summary>
        /// Get available synchronized recordings
        /// </summary>
        public System.Collections.Generic.List<string> GetAvailableRecordings()
        {
            var recordings = new System.Collections.Generic.List<string>();
            string outputDir = GetOutputFolderPath();
            
            if (Directory.Exists(outputDir))
            {
                var videoFiles = Directory.GetFiles(outputDir, "*.mp4");
                foreach (var videoFile in videoFiles)
                {
                    string sessionId = Path.GetFileNameWithoutExtension(videoFile);
                    recordings.Add(sessionId);
                }
            }
            
            return recordings;
        }

        /// <summary>
        /// Check if synchronized recording exists (both video and JSON)
        /// </summary>
        public bool HasSynchronizedRecording(string sessionId)
        {
            // Check for video file
            string outputDir = GetOutputFolderPath();
            if (!Directory.Exists(outputDir))
                return false;
                
            var videoFiles = Directory.GetFiles(outputDir, "*.mp4");
            bool hasVideo = videoFiles.Any(f => Path.GetFileNameWithoutExtension(f).Contains(sessionId));
            
            // Check for JSON file using RecordingStorage
            var availableRecordings = RecordingStorage.GetAvailableRecordings(storageFormat);
            bool hasHipData = availableRecordings.Contains(sessionId);
            
            Debug.Log($"[SynchronizedVideoRecorder] Session {sessionId} - Video: {hasVideo}, Hip Data: {hasHipData}");
            
            return hasVideo && hasHipData;
        }

        /// <summary>
        /// Load hip recording data for a session
        /// </summary>
        public HipRecording LoadHipRecording(string sessionId)
        {
            return RecordingStorage.LoadRecording(sessionId, storageFormat);
        }

        /// <summary>
        /// Get recording metadata
        /// </summary>
        public RecordingMetadata GetRecordingMetadata(string sessionId)
        {
            return RecordingStorage.GetRecordingMetadata(sessionId, storageFormat);
        }

        #region Event Handlers

        private void OnVideoCaptureStarted()
        {
            Debug.Log("[SynchronizedVideoRecorder] Video capture started successfully");
        }

        private void OnVideoCaptureCompleted(FileWritingHandler handler)
        {
            Debug.Log($"[SynchronizedVideoRecorder] Video file writing completed: {handler.Path}");
        }

        #endregion

        void OnDestroy()
        {
            // Restore UI settings if they were modified
            RestoreUISettings();
            
            if (videoCapture != null)
            {
                videoCapture.OnCaptureStart.RemoveListener(OnVideoCaptureStarted);
                videoCapture.CompletedFileWritingAction -= OnVideoCaptureCompleted;
            }
        }
    }

    /// <summary>
    /// Result of synchronized recording containing both video and hip data
    /// </summary>
    [System.Serializable]
    public class SynchronizedRecordingResult
    {
        public string sessionId;
        public string videoFilePath;
        public string hipDataPath;
        public HipRecording hipRecording;
        public DateTime recordingStartTime;
        public DateTime recordingEndTime;
        public float videoFrameRate;
        public float hipFrameRate;
        
        public TimeSpan Duration => recordingEndTime - recordingStartTime;
        public bool IsValid => !string.IsNullOrEmpty(videoFilePath) && hipRecording != null && hipRecording.IsValid;
        public bool HasHipDataFile => !string.IsNullOrEmpty(hipDataPath) && File.Exists(hipDataPath);
        
        public string GetSummary()
        {
            return $"Session: {sessionId}\n" +
                   $"Duration: {Duration.TotalSeconds:F2}s\n" +
                   $"Video: {videoFrameRate}fps\n" +
                   $"Hip Frames: {hipRecording?.FrameCount ?? 0}\n" +
                   $"Hip Rate: {hipFrameRate}fps\n" +
                   $"Video File: {(File.Exists(videoFilePath) ? "✓" : "✗")}\n" +
                   $"Hip Data File: {(HasHipDataFile ? "✓" : "✗")}";
        }
    }
} 