using UnityEngine;
using UnityEngine.UI;
using TENDOR.Recording;
using TENDOR.Core;
using System.Collections.Generic;
using System.IO;
using Logger = TENDOR.Core.Logger;

namespace TENDOR.UI
{
    /// <summary>
    /// Simple UI for the original BodyTrackingController system
    /// Direct connection without complex state management
    /// </summary>
    public class BodyTrackingUI : MonoBehaviour
    {
        [Header("UI References")]
        public Button recordButton;
        public Button stopRecordButton;
        public Button playButton;
        public Button stopPlayButton;
        public Button loadButton;
        public Text statusText;
        public Text modeText;
        public Dropdown recordingsDropdown;
        
        [Header("Controller Reference")]
        public BodyTrackingController controller;
        
        private List<string> availableRecordings = new List<string>();
        
        private void Start()
        {
            // Find controller if not assigned
            if (controller == null)
            {
                controller = FindFirstObjectByType<BodyTrackingController>();
            }
            
            if (controller == null)
            {
                Logger.LogError("❌ No BodyTrackingController found", "UI");
                return;
            }
            
            // Setup button listeners
            SetupButtons();
            
            // Initial UI update
            UpdateUI();
            
            Logger.Log("🎮 BodyTrackingUI initialized", "UI");
        }
        
        private void SetupButtons()
        {
            if (recordButton != null)
                recordButton.onClick.AddListener(OnRecordClicked);
            
            if (stopRecordButton != null)
                stopRecordButton.onClick.AddListener(OnStopRecordClicked);
            
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);
            
            if (stopPlayButton != null)
                stopPlayButton.onClick.AddListener(OnStopPlayClicked);
            
            if (loadButton != null)
                loadButton.onClick.AddListener(OnLoadClicked);
            
            if (recordingsDropdown != null)
                recordingsDropdown.onValueChanged.AddListener(OnRecordingSelected);
        }
        
        private void Update()
        {
            if (controller != null)
            {
                UpdateUI();
            }
        }
        
        private void UpdateUI()
        {
            if (controller == null) return;
            
            // Update button states
            if (recordButton != null)
                recordButton.interactable = controller.CanRecord;
            
            if (stopRecordButton != null)
                stopRecordButton.interactable = controller.IsRecording;
            
            if (playButton != null)
                playButton.interactable = controller.CanPlayback;
            
            if (stopPlayButton != null)
                stopPlayButton.interactable = controller.IsPlaying;
            
            if (loadButton != null)
                loadButton.interactable = !controller.IsRecording && !controller.IsPlaying;
            
            // Update status text
            if (statusText != null)
            {
                var status = GetStatusText();
                statusText.text = status;
            }
            
            // Update mode text
            if (modeText != null)
            {
                modeText.text = $"Mode: {controller.CurrentMode}";
            }
        }
        
        private string GetStatusText()
        {
            switch (controller.CurrentMode)
            {
                case BodyTrackingController.Mode.Recording:
                    var recorder = controller.recorder;
                    if (recorder != null)
                    {
                        return $"🔴 Recording: {recorder.RecordedFrameCount} frames ({recorder.RecordingDuration:F1}s)";
                    }
                    return "🔴 Recording...";
                
                case BodyTrackingController.Mode.Playing:
                    var player = controller.player;
                    if (player != null)
                    {
                        var progress = player.PlaybackProgress * 100f;
                        return $"▶️ Playing: {player.CurrentFrame}/{player.TotalFrames} ({progress:F0}%)";
                    }
                    return "▶️ Playing...";
                
                case BodyTrackingController.Mode.Idle:
                default:
                    var recordings = controller.GetAvailableRecordings();
                    return $"⏸️ Ready - {recordings.Count} recordings available";
            }
        }
        
        private void OnRecordClicked()
        {
            if (controller != null)
            {
                bool success = controller.StartRecording();
                if (success)
                {
                    Logger.Log("🔴 Recording started via UI", "UI");
                }
                else
                {
                    Logger.LogError("❌ Failed to start recording", "UI");
                }
            }
        }
        
        private void OnStopRecordClicked()
        {
            if (controller != null)
            {
                bool success = controller.StopRecording();
                if (success)
                {
                    Logger.Log("⏹️ Recording stopped via UI", "UI");
                    RefreshRecordingsList();
                }
                else
                {
                    Logger.LogError("❌ Failed to stop recording", "UI");
                }
            }
        }
        
        private void OnPlayClicked()
        {
            if (controller != null)
            {
                string selectedRecording = GetSelectedRecording();
                bool success = controller.StartPlayback(selectedRecording);
                if (success)
                {
                    Logger.Log($"▶️ Playback started via UI: {Path.GetFileName(selectedRecording)}", "UI");
                }
                else
                {
                    Logger.LogError("❌ Failed to start playback", "UI");
                }
            }
        }
        
        private void OnStopPlayClicked()
        {
            if (controller != null)
            {
                bool success = controller.StopPlayback();
                if (success)
                {
                    Logger.Log("⏹️ Playback stopped via UI", "UI");
                }
                else
                {
                    Logger.LogError("❌ Failed to stop playback", "UI");
                }
            }
        }
        
        private void OnLoadClicked()
        {
            RefreshRecordingsList();
        }
        
        private void OnRecordingSelected(int index)
        {
            if (index >= 0 && index < availableRecordings.Count)
            {
                var selectedFile = availableRecordings[index];
                Logger.Log($"📁 Selected recording: {Path.GetFileName(selectedFile)}", "UI");
            }
        }
        
        private void RefreshRecordingsList()
        {
            if (controller == null || recordingsDropdown == null) return;
            
            // Refresh controller's recordings list
            controller.RefreshRecordingsList();
            
            // Get updated list
            availableRecordings = controller.GetAvailableRecordings();
            
            // Update dropdown
            recordingsDropdown.ClearOptions();
            
            var options = new List<string>();
            foreach (var recording in availableRecordings)
            {
                var fileName = Path.GetFileName(recording);
                options.Add(fileName);
            }
            
            if (options.Count == 0)
            {
                options.Add("No recordings found");
            }
            
            recordingsDropdown.AddOptions(options);
            
            // Select the latest recording
            if (availableRecordings.Count > 0)
            {
                recordingsDropdown.value = availableRecordings.Count - 1;
            }
            
            Logger.Log($"📋 Refreshed recordings list: {availableRecordings.Count} files", "UI");
        }
        
        private string GetSelectedRecording()
        {
            if (recordingsDropdown != null && availableRecordings.Count > 0)
            {
                int selectedIndex = recordingsDropdown.value;
                if (selectedIndex >= 0 && selectedIndex < availableRecordings.Count)
                {
                    return availableRecordings[selectedIndex];
                }
            }
            
            // Return latest recording as fallback
            if (availableRecordings.Count > 0)
            {
                return availableRecordings[availableRecordings.Count - 1];
            }
            
            return null;
        }
        
        // Public methods for external control (e.g., keyboard shortcuts)
        public void ToggleRecording()
        {
            if (controller == null) return;
            
            if (controller.IsRecording)
            {
                OnStopRecordClicked();
            }
            else if (controller.CanRecord)
            {
                OnRecordClicked();
            }
        }
        
        public void TogglePlayback()
        {
            if (controller == null) return;
            
            if (controller.IsPlaying)
            {
                OnStopPlayClicked();
            }
            else if (controller.CanPlayback)
            {
                OnPlayClicked();
            }
        }
    }
} 