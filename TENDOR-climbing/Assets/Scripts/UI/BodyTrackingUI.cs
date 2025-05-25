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
                Debug.LogError("[BodyTrackingUI] BodyTrackingController not found");
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
            if (Time.frameCount % 30 == 0) // Update every 30 frames (~twice per second)
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
            
            // Update mode text
            if (modeText != null)
            {
                modeText.text = $"Mode: {controller.CurrentMode}";
            }
            
            // Update button states
            bool canRecord = controller.CanRecord;
            bool canPlayback = controller.CanPlayback;
            bool isRecording = controller.IsRecording;
            bool isPlaying = controller.IsPlaying;
            
            if (recordButton != null)
            {
                recordButton.interactable = canRecord;
            }
            
            if (stopRecordButton != null)
            {
                stopRecordButton.interactable = isRecording;
            }
            
            if (playButton != null)
            {
                playButton.interactable = canPlayback;
            }
            
            if (stopPlayButton != null)
            {
                stopPlayButton.interactable = isPlaying;
            }
            
            if (loadButton != null)
            {
                loadButton.interactable = availableRecordings.Count > 0 && !isRecording && !isPlaying;
            }
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
                Debug.Log("[BodyTrackingUI] Started hip recording");
            }
            else
            {
                Debug.LogWarning("[BodyTrackingUI] Failed to start hip recording");
            }
        }

        private void OnStopRecordClicked()
        {
            var recording = controller.StopRecording();
            if (recording != null)
            {
                Debug.Log($"[BodyTrackingUI] Hip recording stopped: {recording.FrameCount} frames");
                RefreshRecordingsList(); // Update list with new recording
            }
        }

        private void OnPlayClicked()
        {
            if (controller.StartPlayback())
            {
                Debug.Log("[BodyTrackingUI] Started hip playback");
            }
            else
            {
                Debug.LogWarning("[BodyTrackingUI] Failed to start hip playback");
            }
        }

        private void OnStopPlayClicked()
        {
            controller.StopPlayback();
            Debug.Log("[BodyTrackingUI] Stopped hip playback");
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
                        Debug.Log($"[BodyTrackingUI] Loaded hip recording: {fileName}");
                        UpdateUI(); // Update button states after loading
                    }
                    else
                    {
                        Debug.LogWarning($"[BodyTrackingUI] Failed to load hip recording: {fileName}");
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