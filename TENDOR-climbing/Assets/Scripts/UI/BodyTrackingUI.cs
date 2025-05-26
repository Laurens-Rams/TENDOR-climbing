using UnityEngine;
using UnityEngine.UI;
using BodyTracking;
using BodyTracking.Storage;
using BodyTracking.AR;
using System.Collections.Generic;

namespace BodyTracking.UI
{
    /// <summary>
    /// Simple UI controller for hip tracking recording and playback
    /// </summary>
    public class BodyTrackingUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public Button recordButton;
        public Button stopRecordButton;
        public Button playButton;
        public Button stopPlayButton;
        public Button loadButton;
        public TMPro.TextMeshProUGUI statusText;
        public TMPro.TextMeshProUGUI modeText;
        public TMPro.TMP_Dropdown recordingsDropdown;
        
        [Header("System")]
        public BodyTrackingController controller;
        
        // State
        private List<string> availableRecordings = new List<string>();
        private OrientationManager orientationManager;
        private BodyTracking.Recording.BodyTrackingRecorder bodyRecorder;

        void Start()
        {
            // Find controller if not assigned
            if (controller == null)
            {
                controller = FindFirstObjectByType<BodyTrackingController>();
            }
            
            if (controller == null)
            {
               UnityEngine.Debug.LogError("[BodyTrackingUI] BodyTrackingController not found");
                return;
            }
            
            // Find orientation manager
            orientationManager = FindFirstObjectByType<OrientationManager>();
            if (orientationManager != null)
            {
                orientationManager.OnOrientationChanged += OnOrientationChanged;
                orientationManager.OnBodyTrackingOptimalChanged += OnBodyTrackingOptimalChanged;
            }
            
            // Find body recorder for tracking issue events
            bodyRecorder = FindFirstObjectByType<BodyTracking.Recording.BodyTrackingRecorder>();
            if (bodyRecorder != null)
            {
                bodyRecorder.OnTrackingIssueDetected += OnTrackingIssueDetected;
            }
            
            // Setup UI
            SetupUI();
            
            // Subscribe to controller events
            controller.OnModeChanged += OnModeChanged;
            controller.OnRecordingComplete += OnRecordingComplete;
            
            // Initial update
            UpdateUI();
            RefreshRecordingsList();
        }

        void OnDestroy()
        {
            // Unsubscribe from events
            if (controller != null)
            {
                controller.OnModeChanged -= OnModeChanged;
                controller.OnRecordingComplete -= OnRecordingComplete;
            }
            
            if (orientationManager != null)
            {
                orientationManager.OnOrientationChanged -= OnOrientationChanged;
                orientationManager.OnBodyTrackingOptimalChanged -= OnBodyTrackingOptimalChanged;
            }
            
            if (bodyRecorder != null)
            {
                bodyRecorder.OnTrackingIssueDetected -= OnTrackingIssueDetected;
            }
        }

        void Update()
        {
            // Periodically update UI to ensure button states stay synchronized
            // Reduced frequency for better performance
            if (Time.frameCount % 90 == 0) // Update every 90 frames (~once per 1.5 seconds at 60fps)
            {
                UpdateUI();
            }
        }

        /// <summary>
        /// Setup UI button events
        /// </summary>
        private void SetupUI()
        {
            if (recordButton != null)
            {
                recordButton.onClick.AddListener(OnRecordClicked);
            }
            
            if (stopRecordButton != null)
            {
                stopRecordButton.onClick.AddListener(OnStopRecordClicked);
            }
            
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayClicked);
            }
            
            if (stopPlayButton != null)
            {
                stopPlayButton.onClick.AddListener(OnStopPlayClicked);
            }
            
            if (loadButton != null)
            {
                loadButton.onClick.AddListener(OnLoadClicked);
            }
        }

        /// <summary>
        /// Update UI state based on controller status
        /// </summary>
        private void UpdateUI()
        {
            if (controller == null) return;
            
            // Update mode text with detailed information
            if (modeText != null)
            {
                string modeInfo = $"Mode: {controller.CurrentMode}";
                
                // Add additional context based on mode
                switch (controller.CurrentMode)
                {
                    case OperationMode.Ready:
                        if (controller.CanRecord)
                            modeInfo += " (Ready to Record)";
                        else if (!controller.IsInitialized)
                            modeInfo += " (Initializing...)";
                        else
                            modeInfo += " (Waiting for Image Target)";
                        break;
                    case OperationMode.Recording:
                        modeInfo += " [REC]";
                        break;
                    case OperationMode.Playing:
                        modeInfo += " [PLAY]";
                        break;
                }
                
                modeText.text = modeInfo;
            }
            
            // Update main status text with comprehensive system information
            if (statusText != null)
            {
                statusText.text = GetDetailedSystemStatus();
            }
            
            // Update button states
            bool canRecord = controller.CanRecord;
            bool canPlayback = controller.CanPlayback;
            bool isRecording = controller.IsRecording;
            bool isPlaying = controller.IsPlaying;
            
            if (recordButton != null)
            {
                recordButton.interactable = canRecord;
                // Update button text based on state
                var buttonText = recordButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = canRecord ? "RECORD" : "RECORD";
                }
            }
            
            if (stopRecordButton != null)
            {
                stopRecordButton.interactable = isRecording;
            }
            
            if (playButton != null)
            {
                playButton.interactable = canPlayback;
                var buttonText = playButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = canPlayback ? "PLAY" : "PLAY";
                }
            }
            
            if (stopPlayButton != null)
            {
                stopPlayButton.interactable = isPlaying;
            }
            
            if (loadButton != null)
            {
                loadButton.interactable = availableRecordings.Count > 0 && !isRecording && !isPlaying;
            }
            
            // Debug log for troubleshooting
            if (Time.frameCount % 300 == 0) // Every 5 seconds at 60fps
            {
                Debug.Log($"[BodyTrackingUI] Status Update - CanRecord: {canRecord}, CanPlayback: {canPlayback}, IsRecording: {isRecording}, IsPlaying: {isPlaying}");
            }
        }

        /// <summary>
        /// Get detailed system status for display
        /// </summary>
        private string GetDetailedSystemStatus()
        {
            if (controller == null) return "❌ Controller not found";
            
            var status = new System.Text.StringBuilder();
            
            // System initialization status
            if (!controller.IsInitialized)
            {
                status.AppendLine("🔄 System initializing...");
                return status.ToString();
            }
            
            status.AppendLine("✅ System initialized");
            
            // Orientation status
            if (orientationManager != null)
            {
                string orientationIcon = orientationManager.IsBodyTrackingOptimal ? "✅" : "⚠️";
                status.AppendLine($"{orientationIcon} {orientationManager.GetOrientationRecommendation()}");
                
                if (!orientationManager.IsBodyTrackingOptimal)
                {
                    status.AppendLine("📱 Rotate device to landscape for better tracking");
                }
            }
            else
            {
                // Fallback orientation check
                bool isLandscape = Screen.width > Screen.height;
                string orientationIcon = isLandscape ? "✅" : "⚠️";
                string orientationText = isLandscape ? "Landscape (optimal)" : "Portrait (suboptimal)";
                status.AppendLine($"{orientationIcon} Orientation: {orientationText}");
            }
            
            // Image tracking status
            var imageManager = controller.GetComponent<BodyTracking.AR.ARImageTargetManager>();
            if (imageManager != null)
            {
                if (imageManager.IsImageDetected)
                {
                    status.AppendLine("📷 Image target detected");
                    status.AppendLine($"🎯 Target: {imageManager.ImageTargetTransform?.position}");
                }
                else
                {
                    status.AppendLine("🔍 Searching for image target...");
                    status.AppendLine("📷 Point camera at wall target");
                }
            }
            else
            {
                status.AppendLine("❌ Image manager not found");
            }
            
            // Body tracking status
            var humanBodyManager = FindFirstObjectByType<UnityEngine.XR.ARFoundation.ARHumanBodyManager>();
            if (humanBodyManager != null)
            {
                string bodyIcon = humanBodyManager.enabled ? "✅" : "❌";
                status.AppendLine($"{bodyIcon} Body tracking: {(humanBodyManager.enabled ? "Enabled" : "Disabled")}");
                
                // Check for tracked bodies
                var trackedBodies = FindObjectsOfType<UnityEngine.XR.ARFoundation.ARHumanBody>();
                if (trackedBodies.Length > 0)
                {
                    status.AppendLine($"👤 Bodies detected: {trackedBodies.Length}");
                    
                    // Show tracking quality info if available
                    if (bodyRecorder != null)
                    {
                        if (bodyRecorder.HasRecentSuccessfulTracking)
                        {
                            status.AppendLine("✅ Joint data valid");
                        }
                        else if (bodyRecorder.ConsecutiveFailures > 30)
                        {
                            status.AppendLine($"⚠️ Tracking issues ({bodyRecorder.ConsecutiveFailures} failures)");
                        }
                    }
                }
                else
                {
                    status.AppendLine("👤 No bodies detected");
                    if (orientationManager != null && !orientationManager.IsBodyTrackingOptimal)
                    {
                        status.AppendLine("💡 Try landscape orientation");
                    }
                }
            }
            
            // Recording/Playback status
            switch (controller.CurrentMode)
            {
                case OperationMode.Recording:
                    status.AppendLine("🔴 RECORDING IN PROGRESS");
                    if (bodyRecorder != null)
                    {
                        status.AppendLine($"⏱️ Duration: {bodyRecorder.RecordingDuration:F1}s");
                    }
                    break;
                case OperationMode.Playing:
                    status.AppendLine("▶️ PLAYBACK IN PROGRESS");
                    break;
                case OperationMode.Ready:
                    if (controller.CanRecord)
                        status.AppendLine("✅ Ready to record");
                    if (controller.CanPlayback)
                        status.AppendLine("✅ Ready to playback");
                    break;
            }
            
            // Available recordings
            if (availableRecordings.Count > 0)
            {
                status.AppendLine($"💾 Recordings available: {availableRecordings.Count}");
            }
            else
            {
                status.AppendLine("💾 No recordings found");
            }
            
            return status.ToString();
        }

        /// <summary>
        /// Refresh the list of available recordings
        /// </summary>
        private void RefreshRecordingsList()
        {
            availableRecordings = RecordingStorage.GetAvailableRecordings();
            
            if (recordingsDropdown != null)
            {
                recordingsDropdown.ClearOptions();
                
                var options = new List<string>();
                foreach (var recording in availableRecordings)
                {
                    var metadata = RecordingStorage.GetRecordingMetadata(recording);
                    if (metadata != null)
                    {
                        options.Add($"{recording} ({metadata.FormattedDuration}, {metadata.FormattedFileSize})");
                    }
                    else
                    {
                        options.Add(recording);
                    }
                }
                
                recordingsDropdown.AddOptions(options);
            }
            
            UpdateUI();
        }

        #region Button Handlers

        private void OnRecordClicked()
        {
            if (controller.StartRecording())
            {
               UnityEngine.Debug.Log("[BodyTrackingUI] Started hip recording");
            }
            else
            {
               UnityEngine.Debug.LogWarning("[BodyTrackingUI] Failed to start hip recording");
            }
        }

        private void OnStopRecordClicked()
        {
            var recording = controller.StopRecording();
            if (recording != null)
            {
               UnityEngine.Debug.Log($"[BodyTrackingUI] Hip recording stopped: {recording.FrameCount} frames");
                RefreshRecordingsList(); // Update list with new recording
            }
        }

        private void OnPlayClicked()
        {
            if (controller.StartPlayback())
            {
               UnityEngine.Debug.Log("[BodyTrackingUI] Started hip playback");
            }
            else
            {
               UnityEngine.Debug.LogWarning("[BodyTrackingUI] Failed to start hip playback");
            }
        }

        private void OnStopPlayClicked()
        {
            controller.StopPlayback();
           UnityEngine.Debug.Log("[BodyTrackingUI] Stopped hip playback");
        }

        private void OnLoadClicked()
        {
            if (recordingsDropdown != null && availableRecordings.Count > 0)
            {
                int selectedIndex = recordingsDropdown.value;
                if (selectedIndex >= 0 && selectedIndex < availableRecordings.Count)
                {
                    string fileName = availableRecordings[selectedIndex];
                    if (controller.LoadRecording(fileName))
                    {
                       UnityEngine.Debug.Log($"[BodyTrackingUI] Loaded hip recording: {fileName}");
                        UpdateUI(); // Update button states after loading
                    }
                    else
                    {
                       UnityEngine.Debug.LogWarning($"[BodyTrackingUI] Failed to load hip recording: {fileName}");
                    }
                }
            }
        }

        #endregion

        #region Event Handlers

        private void OnModeChanged(OperationMode newMode)
        {
            UpdateUI();
        }

        private void OnRecordingComplete(BodyTracking.Data.HipRecording recording)
        {
            RefreshRecordingsList();
            UpdateUI();
        }
        
        private void OnOrientationChanged(ScreenOrientation newOrientation)
        {
            UpdateUI(); // Refresh UI to show new orientation status
        }
        
        private void OnBodyTrackingOptimalChanged(bool isOptimal)
        {
            UpdateUI(); // Refresh UI to show tracking status change
            
            if (isOptimal)
            {
                Debug.Log("[BodyTrackingUI] ✅ Orientation now optimal for body tracking");
            }
            else
            {
                Debug.LogWarning("[BodyTrackingUI] ⚠️ Orientation not optimal for body tracking");
            }
        }
        
        private void OnTrackingIssueDetected(string issue)
        {
            Debug.LogWarning($"[BodyTrackingUI] Tracking Issue: {issue}");
            UpdateUI(); // Refresh UI to show tracking issues
        }

        #endregion

        #region Public Methods (for external UI integration)

        /// <summary>
        /// Get current system status
        /// </summary>
        private string GetSystemStatus()
        {
            if (controller == null) return "Controller not available";
            
            if (!controller.IsInitialized) return "Initializing...";
            
            string baseStatus = "";
            switch (controller.CurrentMode)
            {
                case OperationMode.Ready:
                    if (controller.CanRecord && controller.CanPlayback)
                        baseStatus = "Ready - can record and playback";
                    else if (controller.CanRecord)
                        baseStatus = "Ready - can record";
                    else
                        baseStatus = "Waiting for image target...";
                    break;
                    
                case OperationMode.Recording:
                    if (controller.IsVideoRecordingEnabled)
                        baseStatus = "Recording video + hip tracking...";
                    else
                        baseStatus = "Recording hip position...";
                    break;
                    
                case OperationMode.Playing:
                    baseStatus = "Playing back movement...";
                    break;
                    
                default:
                    baseStatus = "Unknown state";
                    break;
            }
            
            // Add video recording status
            if (controller.IsVideoRecordingEnabled)
            {
                baseStatus += "\n📹 Video recording enabled";
            }
            
            return baseStatus;
        }

        /// <summary>
        /// Get recording statistics
        /// </summary>
        public string GetRecordingStats()
        {
            var recordings = RecordingStorage.GetAvailableRecordings();
            var totalSize = RecordingStorage.GetTotalStorageUsed();
            
            string sizeString = FormatBytes(totalSize);
            return $"{recordings.Count} hip recordings, {sizeString} total";
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        #endregion
    }
} 