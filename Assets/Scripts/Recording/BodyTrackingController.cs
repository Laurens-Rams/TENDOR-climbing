using UnityEngine;
using UnityEngine.XR.ARFoundation;
using TENDOR.Core;
using TENDOR.Runtime.Models;
using System.Collections.Generic;
using System.IO;
using Logger = TENDOR.Core.Logger;

namespace TENDOR.Recording
{
    /// <summary>
    /// Main controller for body tracking recording and playback
    /// Original functionality restored with new API structure
    /// </summary>
    public class BodyTrackingController : MonoBehaviour
    {
        [Header("AR References")]
        public ARHumanBodyManager humanBodyManager;
        
        [Header("Recording Settings")]
        public bool enableVideoRecording = true;
        public string recordingDirectory = "BodyTrackingRecordings";
        
        [Header("Components")]
        public BodyTrackingRecorder recorder;
        public BodyTrackingPlayer player;
        
        // Current mode
        public enum Mode
        {
            Idle,
            Recording,
            Playing
        }
        
        private Mode currentMode = Mode.Idle;
        private string currentRecordingPath;
        private List<string> availableRecordings = new List<string>();
        
        // Properties for UI
        public bool CanRecord => currentMode == Mode.Idle;
        public bool CanPlayback => currentMode == Mode.Idle && availableRecordings.Count > 0;
        public bool IsRecording => currentMode == Mode.Recording;
        public bool IsPlaying => currentMode == Mode.Playing;
        public Mode CurrentMode => currentMode;
        public bool IsVideoRecordingEnabled => enableVideoRecording;
        
        private void Start()
        {
            // Find components if not assigned
            if (recorder == null)
                recorder = GetComponent<BodyTrackingRecorder>();
            
            if (player == null)
                player = GetComponent<BodyTrackingPlayer>();
            
            if (humanBodyManager == null)
                humanBodyManager = FindFirstObjectByType<ARHumanBodyManager>();
            
            // Setup recorder
            if (recorder != null)
            {
                recorder.Initialize(humanBodyManager);
            }
            
            // Setup player
            if (player != null)
            {
                player.Initialize();
            }
            
            // Create recording directory
            CreateRecordingDirectory();
            
            // Load available recordings
            RefreshRecordingsList();
            
            Logger.Log("🎮 BodyTrackingController initialized", "CONTROLLER");
        }
        
        private void CreateRecordingDirectory()
        {
            var fullPath = Path.Combine(Application.persistentDataPath, recordingDirectory);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                Logger.Log($"📁 Created recording directory: {fullPath}", "CONTROLLER");
            }
        }
        
        public void RefreshRecordingsList()
        {
            availableRecordings.Clear();
            
            var recordingPath = Path.Combine(Application.persistentDataPath, recordingDirectory);
            if (Directory.Exists(recordingPath))
            {
                var jsonFiles = Directory.GetFiles(recordingPath, "*.json");
                availableRecordings.AddRange(jsonFiles);
                Logger.Log($"📋 Found {availableRecordings.Count} recordings", "CONTROLLER");
            }
        }
        
        public bool StartRecording()
        {
            if (!CanRecord)
            {
                Logger.LogWarning($"Cannot start recording in mode {currentMode}", "CONTROLLER");
                return false;
            }
            
            if (recorder == null)
            {
                Logger.LogError("❌ No recorder component found", "CONTROLLER");
                return false;
            }
            
            // Generate recording filename
            var timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"BodyTracking_{timestamp}.json";
            currentRecordingPath = Path.Combine(Application.persistentDataPath, recordingDirectory, filename);
            
            // Start recording
            if (recorder.StartRecording(currentRecordingPath))
            {
                currentMode = Mode.Recording;
                Logger.Log($"🔴 Started recording: {filename}", "CONTROLLER");
                return true;
            }
            
            return false;
        }
        
        public bool StopRecording()
        {
            if (currentMode != Mode.Recording)
            {
                Logger.LogWarning($"Cannot stop recording in mode {currentMode}", "CONTROLLER");
                return false;
            }
            
            if (recorder == null)
            {
                Logger.LogError("❌ No recorder component found", "CONTROLLER");
                return false;
            }
            
            // Stop recording
            if (recorder.StopRecording())
            {
                currentMode = Mode.Idle;
                Logger.Log($"⏹️ Stopped recording: {Path.GetFileName(currentRecordingPath)}", "CONTROLLER");
                
                // Refresh recordings list
                RefreshRecordingsList();
                return true;
            }
            
            return false;
        }
        
        public bool StartPlayback(string recordingPath = null)
        {
            if (!CanPlayback)
            {
                Logger.LogWarning($"Cannot start playback in mode {currentMode}", "CONTROLLER");
                return false;
            }
            
            if (player == null)
            {
                Logger.LogError("❌ No player component found", "CONTROLLER");
                return false;
            }
            
            // Use latest recording if no path specified
            if (string.IsNullOrEmpty(recordingPath) && availableRecordings.Count > 0)
            {
                recordingPath = availableRecordings[availableRecordings.Count - 1];
            }
            
            if (string.IsNullOrEmpty(recordingPath))
            {
                Logger.LogError("❌ No recording path specified", "CONTROLLER");
                return false;
            }
            
            // Start playback
            if (player.StartPlayback(recordingPath))
            {
                currentMode = Mode.Playing;
                Logger.Log($"▶️ Started playback: {Path.GetFileName(recordingPath)}", "CONTROLLER");
                return true;
            }
            
            return false;
        }
        
        public bool StopPlayback()
        {
            if (currentMode != Mode.Playing)
            {
                Logger.LogWarning($"Cannot stop playback in mode {currentMode}", "CONTROLLER");
                return false;
            }
            
            if (player == null)
            {
                Logger.LogError("❌ No player component found", "CONTROLLER");
                return false;
            }
            
            // Stop playback
            if (player.StopPlayback())
            {
                currentMode = Mode.Idle;
                Logger.Log("⏹️ Stopped playback", "CONTROLLER");
                return true;
            }
            
            return false;
        }
        
        public List<string> GetAvailableRecordings()
        {
            return new List<string>(availableRecordings);
        }
        
        public string GetCurrentRecordingPath()
        {
            return currentRecordingPath;
        }
        
        private void Update()
        {
            // Update player if playing
            if (currentMode == Mode.Playing && player != null)
            {
                player.UpdatePlayback();
            }
        }
        
        private void OnDestroy()
        {
            // Clean up
            if (recorder != null && IsRecording)
            {
                StopRecording();
            }
            
            if (player != null && IsPlaying)
            {
                StopPlayback();
            }
        }
    }
} 