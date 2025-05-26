using UnityEngine;
using UnityEngine.UI;
using BodyTracking;
using BodyTracking.Storage;
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

        void Start()
        {
            // Find controller if not assigned
            if (controller == null)
            {
                controller = FindObjectOfType<BodyTrackingController>();
            }
            
            if (controller == null)
            {
               UnityEngine.Debug.LogError("[BodyTrackingUI] BodyTrackingController not found");
                return;
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
                        modeInfo += " 🔴";
                        break;
                    case OperationMode.Playing:
                        modeInfo += " ▶️";
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
                    buttonText.text = canRecord ? "🔴 RECORD" : "⏸️ RECORD";
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
                    buttonText.text = canPlayback ? "▶️ PLAY" : "⏸️ PLAY";
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
            
            // Image tracking status
            var imageManager = controller.GetComponent<BodyTracking.AR.ARImageTargetManager>();
            if (imageManager != null)
            {
                if (imageManager.IsImageDetected)
                {
                    status.AppendLine("📷 Image target detected");
                    status.AppendLine($"📍 Target: {imageManager.ImageTargetTransform?.position}");
                }
                else
                {
                    status.AppendLine("🔍 Searching for image target...");
                    status.AppendLine("📱 Point camera at wall target");
                }
            }
            else
            {
                status.AppendLine("❌ Image manager not found");
            }
            
            // Body tracking status
            var humanBodyManager = FindObjectOfType<UnityEngine.XR.ARFoundation.ARHumanBodyManager>();
            if (humanBodyManager != null)
            {
                status.AppendLine($"🚶 Body tracking: {(humanBodyManager.enabled ? "Enabled" : "Disabled")}");
                
                // Check for tracked bodies
                var trackedBodies = FindObjectsOfType<UnityEngine.XR.ARFoundation.ARHumanBody>();
                if (trackedBodies.Length > 0)
                {
                    status.AppendLine($"👤 Bodies detected: {trackedBodies.Length}");
                }
                else
                {
                    status.AppendLine("👤 No bodies detected");
                }
            }
            
            // Recording/Playback status
            switch (controller.CurrentMode)
            {
                case OperationMode.Recording:
                    status.AppendLine("🔴 RECORDING IN PROGRESS");
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

        #endregion

        #region Public Methods (for external UI integration)

        /// <summary>
        /// Get current system status for display
        /// </summary>
        public string GetSystemStatus()
        {
            if (controller == null) return "Controller not available";
            
            if (!controller.IsInitialized) return "Initializing...";
            
            switch (controller.CurrentMode)
            {
                case OperationMode.Ready:
                    if (controller.CanRecord && controller.CanPlayback)
                        return "Ready - can record and playback hip position";
                    else if (controller.CanRecord)
                        return "Ready - can record hip position";
                    else
                        return "Waiting for image target...";
                        
                case OperationMode.Recording:
                    return "Recording hip position...";
                    
                case OperationMode.Playing:
                    return "Playing back hip movement...";
                    
                default:
                    return "Unknown state";
            }
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