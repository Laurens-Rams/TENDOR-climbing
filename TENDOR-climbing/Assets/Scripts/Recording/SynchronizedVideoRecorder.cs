using UnityEngine;
using RenderHeads.Media.AVProMovieCapture;
using BodyTracking.Data;
using BodyTracking.Recording;
using BodyTracking.Storage;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using System.IO;
using System.Linq;

namespace BodyTracking.Recording
{
    /// <summary>
    /// Synchronized video and hip tracking recorder using AVPro Movie Capture
    /// Records video at 30fps with matching timestamps to hip tracking data
    /// Uses texture-based recording compatible with AR Foundation
    /// </summary>
    [RequireComponent(typeof(CaptureFromTexture))]
    public class SynchronizedVideoRecorder : MonoBehaviour
    {
        [Header("Video Recording Settings")]
        [SerializeField] private ARCameraManager arCameraManager;
        [SerializeField] private bool autoFindARCamera = true;
        [SerializeField] private string videoOutputFolder = "TENDOR_Recordings";
        [SerializeField] private float videoFrameRate = 30f;
        [SerializeField] private bool useH264Codec = true;
        
        [Header("AR Camera Settings")]
        [SerializeField] private Vector2Int customResolution = new Vector2Int(1080, 1920);
        [Tooltip("Custom resolution is only used as fallback when camera data is unavailable")]
        
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
        private CaptureFromTexture videoCapture;
        private Texture2D recordingTexture;
        private XRCpuImage cpuImage;
        
        // Recording state
        private bool isRecording = false;
        private string currentSessionId;
        private string currentVideoPath;
        private DateTime recordingStartTime;
        private int frameCount = 0;
        
        // Events
        public event System.Action<string> OnVideoRecordingStarted;
        public event System.Action<string, HipRecording> OnSynchronizedRecordingComplete;
        public event System.Action<string> OnVideoRecordingFailed;
        
        // Public properties
        public bool IsRecording => isRecording;
        public string CurrentVideoPath => currentVideoPath;
        public string OutputFolder => GetOutputFolderPath();
        public int FrameCount => frameCount;

        void Awake()
        {
            // Get or add AVPro capture component
            videoCapture = GetComponent<CaptureFromTexture>();
            if (videoCapture == null)
            {
                videoCapture = gameObject.AddComponent<CaptureFromTexture>();
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
            // Find AR camera manager
            if (arCameraManager == null && autoFindARCamera)
            {
                arCameraManager = FindFirstObjectByType<ARCameraManager>();
                if (arCameraManager == null)
                {
                    // Try to get from Globals if available
                    if (Globals.CameraManager != null)
                    {
                        arCameraManager = Globals.CameraManager;
                    }
                }
            }
            
            // Find hip recorder
            if (hipRecorder == null && autoFindHipRecorder)
            {
                hipRecorder = FindFirstObjectByType<BodyTrackingRecorder>();
            }
            
            if (arCameraManager == null)
            {
                Debug.LogError("[SynchronizedVideoRecorder] No AR Camera Manager found!");
                return;
            }
            
            if (hipRecorder == null)
            {
                Debug.LogError("[SynchronizedVideoRecorder] No hip recorder found!");
                return;
            }
            
            Debug.Log($"[SynchronizedVideoRecorder] Initialized with AR Camera Manager: {arCameraManager.name}");
        }

        /// <summary>
        /// Setup AVPro video capture settings for texture-based recording
        /// </summary>
        private void SetupVideoCapture()
        {
            if (videoCapture == null) return;
            
            // Basic settings
            videoCapture.FrameRate = videoFrameRate;
            videoCapture.IsRealTime = true;
            videoCapture.IsManualUpdate = true; // We'll manually update frames
            
            // Audio settings
            if (recordAudio)
            {
                videoCapture.AudioCaptureSource = AudioCaptureSource.Unity;
            }
            else
            {
                videoCapture.AudioCaptureSource = AudioCaptureSource.None;
            }
            
            // Quality settings for AR compatibility
            videoCapture.AllowOfflineVSyncDisable = true;
            
            // Output settings
            videoCapture.OutputTarget = OutputTarget.VideoFile;
            
            // Only override output folder settings if not already configured
            if (string.IsNullOrEmpty(videoCapture.OutputFolderPath) || videoCapture.OutputFolderPath == "Captures")
            {
                videoCapture.OutputFolder = CaptureBase.OutputPath.RelativeToPersistentData;
                videoCapture.OutputFolderPath = videoOutputFolder;
                Debug.Log($"[SynchronizedVideoRecorder] Set output folder to: {videoOutputFolder}");
            }
            else
            {
                Debug.Log($"[SynchronizedVideoRecorder] Using existing output folder: {videoCapture.OutputFolderPath}");
            }
            
            // Subscribe to events
            videoCapture.OnCaptureStart.AddListener(OnVideoCaptureStarted);
            videoCapture.CompletedFileWritingAction += OnVideoCaptureCompleted;
            
            Debug.Log($"[SynchronizedVideoRecorder] Video capture configured: {videoFrameRate}fps, Texture-based recording");
        }

        /// <summary>
        /// Create recording texture based on AR camera configuration
        /// </summary>
        private void CreateRecordingTexture()
        {
            int width, height;
            bool usedActualImageSize = false;
            
            // Always try to use the camera's native resolution like the working VideoRecorder
            // First try to get actual camera image size (most accurate)
            XRCpuImage testImage;
            if (Globals.CameraManager != null && Globals.CameraManager.TryAcquireLatestCpuImage(out testImage))
            {
                width = testImage.width;
                height = testImage.height;
                usedActualImageSize = true;
                testImage.Dispose();
                Debug.Log($"[SynchronizedVideoRecorder] Using actual camera image size: {width}x{height}");
            }
            else if (arCameraManager != null && arCameraManager.TryAcquireLatestCpuImage(out testImage))
            {
                width = testImage.width;
                height = testImage.height;
                usedActualImageSize = true;
                testImage.Dispose();
                Debug.Log($"[SynchronizedVideoRecorder] Using actual camera image size from arCameraManager: {width}x{height}");
            }
            // Use camera configuration like the working VideoRecorder
            else if (Globals.CameraManager != null && Globals.CameraManager.currentConfiguration.HasValue)
            {
                var config = Globals.CameraManager.currentConfiguration.Value;
                width = (int)config.width;
                height = (int)config.height;
                Debug.Log($"[SynchronizedVideoRecorder] Using camera configuration from Globals: {width}x{height}");
            }
            else if (arCameraManager != null && arCameraManager.currentConfiguration.HasValue)
            {
                var config = arCameraManager.currentConfiguration.Value;
                width = (int)config.width;
                height = (int)config.height;
                Debug.Log($"[SynchronizedVideoRecorder] Using camera configuration from arCameraManager: {width}x{height}");
            }
            else
            {
                // Only use custom resolution as absolute last resort
                width = customResolution.x;
                height = customResolution.y;
                Debug.LogWarning($"[SynchronizedVideoRecorder] No camera data available, using custom resolution as fallback: {width}x{height}");
            }
            
            // Create texture for recording
            recordingTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            recordingTexture.name = usedActualImageSize ? 
                "[SynchronizedVideoRecorder] Recording Texture (Actual Size)" : 
                "[SynchronizedVideoRecorder] Recording Texture (Camera Config)";
            
            Debug.Log($"[SynchronizedVideoRecorder] Created recording texture: {width}x{height} (Native camera resolution)");
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
            
            if (videoCapture == null || hipRecorder == null || arCameraManager == null)
            {
                Debug.LogError("[SynchronizedVideoRecorder] Missing required components");
                return false;
            }
            
            // Generate session ID
            currentSessionId = GenerateSessionId();
            
            // Ensure output directory exists
            string outputDir = GetOutputFolderPath();
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
                Debug.Log($"[SynchronizedVideoRecorder] Created output directory: {outputDir}");
            }
            
            // Create recording texture
            CreateRecordingTexture();
            
            // Setup video file naming (don't override folder settings)
            videoCapture.AppendFilenameTimestamp = true;
            videoCapture.FilenamePrefix = filePrefix;
            videoCapture.FilenameExtension = ".mp4";
            
            // Set the source texture
            videoCapture.SetSourceTexture(recordingTexture);
            
            recordingStartTime = DateTime.Now;
            frameCount = 0;
            
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
            
            // Subscribe to AR camera frame events - use Globals.CameraManager like the working old VideoRecorder
            if (Globals.CameraManager != null)
            {
                Globals.CameraManager.frameReceived += OnARCameraFrameReceived;
                Debug.Log("[SynchronizedVideoRecorder] Subscribed to Globals.CameraManager.frameReceived");
            }
            else if (arCameraManager != null)
            {
                arCameraManager.frameReceived += OnARCameraFrameReceived;
                Debug.Log("[SynchronizedVideoRecorder] Subscribed to arCameraManager.frameReceived");
            }
            else
            {
                Debug.LogError("[SynchronizedVideoRecorder] No camera manager available for frame events");
                return false;
            }
            
            isRecording = true;
            
            Debug.Log($"[SynchronizedVideoRecorder] Started synchronized recording: {currentSessionId}");
            OnVideoRecordingStarted?.Invoke(currentSessionId);
            
            return true;
        }

        /// <summary>
        /// Handle AR camera frame received event
        /// </summary>
        private void OnARCameraFrameReceived(ARCameraFrameEventArgs args)
        {
            if (!isRecording || recordingTexture == null) return;
            
            // Try to acquire the latest CPU image using Globals.CameraManager like the working old VideoRecorder
            bool imageAcquired = false;
            if (Globals.CameraManager != null)
            {
                imageAcquired = Globals.CameraManager.TryAcquireLatestCpuImage(out cpuImage);
            }
            else if (arCameraManager != null)
            {
                imageAcquired = arCameraManager.TryAcquireLatestCpuImage(out cpuImage);
            }
            
            if (!imageAcquired)
                return;
            
            try
            {
                // Check if texture size matches the camera image size
                if (recordingTexture.width != cpuImage.width || recordingTexture.height != cpuImage.height)
                {
                    Debug.LogWarning($"[SynchronizedVideoRecorder] Texture size mismatch! Texture: {recordingTexture.width}x{recordingTexture.height}, Camera: {cpuImage.width}x{cpuImage.height}");
                    
                    // Recreate texture with correct size
                    DestroyImmediate(recordingTexture);
                    recordingTexture = new Texture2D(cpuImage.width, cpuImage.height, TextureFormat.RGBA32, false);
                    recordingTexture.name = "[SynchronizedVideoRecorder] Recording Texture (Resized)";
                    
                    // Update the video capture source texture
                    videoCapture.SetSourceTexture(recordingTexture);
                    
                    Debug.Log($"[SynchronizedVideoRecorder] Recreated texture with camera size: {cpuImage.width}x{cpuImage.height}");
                }
                
                // Convert the image to the texture format
                var conversionParams = new XRCpuImage.ConversionParams(cpuImage, TextureFormat.RGBA32, XRCpuImage.Transformation.MirrorY);
                var data = recordingTexture.GetRawTextureData<byte>();
                
                // Verify buffer sizes match
                int requiredBytes = cpuImage.GetConvertedDataSize(conversionParams);
                int availableBytes = data.Length;
                
                if (requiredBytes != availableBytes)
                {
                    Debug.LogError($"[SynchronizedVideoRecorder] Buffer size mismatch! Required: {requiredBytes}, Available: {availableBytes}");
                    return;
                }
                
                cpuImage.Convert(conversionParams, data);
                
                // Apply the texture changes
                recordingTexture.Apply();
                
                // Update the video capture
                videoCapture.UpdateSourceTexture();
                videoCapture.UpdateFrame();
                
                frameCount++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SynchronizedVideoRecorder] Error processing AR camera frame: {e.Message}");
            }
            finally
            {
                // Always dispose the CPU image
                cpuImage.Dispose();
            }
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
            
            // Unsubscribe from AR camera events
            if (Globals.CameraManager != null)
            {
                Globals.CameraManager.frameReceived -= OnARCameraFrameReceived;
                Debug.Log("[SynchronizedVideoRecorder] Unsubscribed from Globals.CameraManager.frameReceived");
            }
            else if (arCameraManager != null)
            {
                arCameraManager.frameReceived -= OnARCameraFrameReceived;
                Debug.Log("[SynchronizedVideoRecorder] Unsubscribed from arCameraManager.frameReceived");
            }
            
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
            
            // Clean up recording texture
            if (recordingTexture != null)
            {
                DestroyImmediate(recordingTexture);
                recordingTexture = null;
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
            Debug.Log($"[SynchronizedVideoRecorder] Video frames: {frameCount}");
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
            if (videoCapture != null && !string.IsNullOrEmpty(videoCapture.OutputFolderPath))
            {
                // Use the actual folder path from the video capture component
                if (videoCapture.OutputFolder == CaptureBase.OutputPath.RelativeToPersistentData)
                {
                    return Path.Combine(Application.persistentDataPath, videoCapture.OutputFolderPath);
                }
                else
                {
                    return videoCapture.OutputFolderPath;
                }
            }
            
            // Fallback to default
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

        /// <summary>
        /// Configure AR camera settings at runtime
        /// </summary>
        public void SetARCameraSettings(bool useARResolution, Vector2Int customRes = default)
        {
            if (isRecording)
            {
                Debug.LogWarning("[SynchronizedVideoRecorder] Cannot change camera settings during recording");
                return;
            }
            
            // Note: We always use camera native resolution now, this method only updates fallback custom resolution
            if (customRes != default)
            {
                customResolution = customRes;
                Debug.Log($"[SynchronizedVideoRecorder] Updated fallback custom resolution: {customResolution}");
            }
            
            Debug.Log($"[SynchronizedVideoRecorder] Camera settings updated - Always using native camera resolution, Custom fallback: {customResolution}");
        }

        /// <summary>
        /// Get current recording status summary
        /// </summary>
        public string GetRecordingStatusSummary()
        {
            var summary = $"Recording: {isRecording}\n";
            summary += $"Session ID: {currentSessionId}\n";
            summary += $"Frame Count: {frameCount}\n";
            
            // Check Globals.CameraManager first (like the working old VideoRecorder)
            if (Globals.CameraManager != null && Globals.CameraManager.currentConfiguration.HasValue)
            {
                var config = Globals.CameraManager.currentConfiguration.Value;
                summary += $"AR Camera (Globals): {config.width}x{config.height}\n";
            }
            else if (arCameraManager != null && arCameraManager.currentConfiguration.HasValue)
            {
                var config = arCameraManager.currentConfiguration.Value;
                summary += $"AR Camera (Local): {config.width}x{config.height}\n";
            }
            else
            {
                summary += "AR Camera: Not configured\n";
            }
            
            if (recordingTexture != null)
            {
                summary += $"Recording Texture: {recordingTexture.width}x{recordingTexture.height}\n";
            }
            
            return summary;
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
            // Unsubscribe from events
            if (Globals.CameraManager != null)
            {
                Globals.CameraManager.frameReceived -= OnARCameraFrameReceived;
            }
            else if (arCameraManager != null)
            {
                arCameraManager.frameReceived -= OnARCameraFrameReceived;
            }
            
            if (videoCapture != null)
            {
                videoCapture.OnCaptureStart.RemoveListener(OnVideoCaptureStarted);
                videoCapture.CompletedFileWritingAction -= OnVideoCaptureCompleted;
            }
            
            // Clean up recording texture
            if (recordingTexture != null)
            {
                DestroyImmediate(recordingTexture);
                recordingTexture = null;
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