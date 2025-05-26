using UnityEngine;
using RenderHeads.Media.AVProMovieCapture;
using BodyTracking.Data;
using BodyTracking.Recording;
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
        [SerializeField] private CaptureBase.Resolution videoResolution = CaptureBase.Resolution.HD_1920x1080;
        [SerializeField] private float videoFrameRate = 30f;
        [SerializeField] private bool useH264Codec = true;
        
        [Header("Synchronization")]
        [SerializeField] private BodyTrackingRecorder hipRecorder;
        [SerializeField] private bool autoFindHipRecorder = true;
        [SerializeField] private bool recordAudio = false;
        
        [Header("File Naming")]
        [SerializeField] private string filePrefix = "TENDOR";
        [SerializeField] private bool includeTimestamp = true;
        
        // Components
        private CaptureFromCamera videoCapture;
        
        // Recording state
        private bool isRecording = false;
        private string currentSessionId;
        private string currentVideoPath;
        private DateTime recordingStartTime;
        
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
            
            // Find hip recorder
            if (hipRecorder == null && autoFindHipRecorder)
            {
                hipRecorder = FindFirstObjectByType<BodyTrackingRecorder>();
            }
            
            if (recordingCamera == null)
            {
                Debug.LogError("[SynchronizedVideoRecorder] No recording camera found!");
                return;
            }
            
            if (hipRecorder == null)
            {
                Debug.LogError("[SynchronizedVideoRecorder] No hip recorder found!");
                return;
            }
            
            Debug.Log($"[SynchronizedVideoRecorder] Initialized with camera: {recordingCamera.name}");
        }

        /// <summary>
        /// Setup AVPro video capture settings
        /// </summary>
        private void SetupVideoCapture()
        {
            if (videoCapture == null) return;
            
            // Basic settings
            videoCapture.SetCamera(recordingCamera, false);
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
            
            // Create result
            var result = new SynchronizedRecordingResult
            {
                sessionId = currentSessionId,
                videoFilePath = currentVideoPath,
                hipRecording = hipData,
                recordingStartTime = recordingStartTime,
                recordingEndTime = DateTime.Now,
                videoFrameRate = videoFrameRate,
                hipFrameRate = hipData.frameRate
            };
            
            Debug.Log($"[SynchronizedVideoRecorder] Synchronized recording completed: {result.sessionId}");
            Debug.Log($"[SynchronizedVideoRecorder] Video: {result.videoFilePath}");
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
        /// Check if synchronized recording exists
        /// </summary>
        public bool HasSynchronizedRecording(string sessionId)
        {
            // Since AVPro generates filenames automatically, we need to search for files
            string outputDir = GetOutputFolderPath();
            if (!Directory.Exists(outputDir))
                return false;
                
            var videoFiles = Directory.GetFiles(outputDir, "*.mp4");
            bool hasVideo = videoFiles.Any(f => Path.GetFileNameWithoutExtension(f).Contains(sessionId));
            
            string hipPath = Path.Combine(Application.persistentDataPath, "Recordings", $"{sessionId}.json");
            bool hasHip = File.Exists(hipPath);
            
            return hasVideo && hasHip;
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
        public HipRecording hipRecording;
        public DateTime recordingStartTime;
        public DateTime recordingEndTime;
        public float videoFrameRate;
        public float hipFrameRate;
        
        public TimeSpan Duration => recordingEndTime - recordingStartTime;
        public bool IsValid => !string.IsNullOrEmpty(videoFilePath) && hipRecording != null && hipRecording.IsValid;
        
        public string GetSummary()
        {
            return $"Session: {sessionId}\n" +
                   $"Duration: {Duration.TotalSeconds:F2}s\n" +
                   $"Video: {videoFrameRate}fps\n" +
                   $"Hip Frames: {hipRecording?.FrameCount ?? 0}\n" +
                   $"Hip Rate: {hipFrameRate}fps";
        }
    }
} 