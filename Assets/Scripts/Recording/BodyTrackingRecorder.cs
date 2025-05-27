using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TENDOR.Core;
using TENDOR.Runtime.Models;
using System.Collections.Generic;
using System.IO;
using Logger = TENDOR.Core.Logger;

namespace TENDOR.Recording
{
    /// <summary>
    /// Records hip tracking data from ARHumanBodyManager to JSON files
    /// Uses the new API structure with fixed ARHumanBody joints API
    /// </summary>
    public class BodyTrackingRecorder : MonoBehaviour
    {
        [Header("Recording Settings")]
        [SerializeField] private int maxRecordingFrames = 3000; // 100 seconds at 30fps
        [SerializeField] private bool logRecordingProgress = true;
        
        private ARHumanBodyManager humanBodyManager;
        private bool isRecording = false;
        private string currentRecordingPath;
        private List<PoseData> recordedHipPoses = new List<PoseData>();
        private float recordingStartTime;
        private int frameCount = 0;
        
        public bool IsRecording => isRecording;
        public int RecordedFrameCount => recordedHipPoses.Count;
        public float RecordingDuration => isRecording ? Time.time - recordingStartTime : 0f;
        
        public void Initialize(ARHumanBodyManager bodyManager)
        {
            humanBodyManager = bodyManager;
            
            if (humanBodyManager != null)
            {
                humanBodyManager.humanBodiesChanged += OnHumanBodiesChanged;
                Logger.Log("🎯 BodyTrackingRecorder initialized", "RECORDER");
            }
            else
            {
                Logger.LogError("❌ No ARHumanBodyManager provided", "RECORDER");
            }
        }
        
        public bool StartRecording(string filePath)
        {
            if (isRecording)
            {
                Logger.LogWarning("⚠️ Already recording", "RECORDER");
                return false;
            }
            
            if (humanBodyManager == null)
            {
                Logger.LogError("❌ No ARHumanBodyManager available", "RECORDER");
                return false;
            }
            
            // Clear previous recording
            recordedHipPoses.Clear();
            frameCount = 0;
            
            // Set recording parameters
            currentRecordingPath = filePath;
            recordingStartTime = Time.time;
            isRecording = true;
            
            Logger.Log($"🔴 Started recording to: {Path.GetFileName(filePath)}", "RECORDER");
            return true;
        }
        
        public bool StopRecording()
        {
            if (!isRecording)
            {
                Logger.LogWarning("⚠️ Not currently recording", "RECORDER");
                return false;
            }
            
            isRecording = false;
            
            // Save recording to file
            bool success = SaveRecordingToFile();
            
            if (success)
            {
                var duration = Time.time - recordingStartTime;
                Logger.Log($"⏹️ Recording saved: {recordedHipPoses.Count} frames, {duration:F1}s", "RECORDER");
            }
            
            return success;
        }
        
        private void OnHumanBodiesChanged(ARHumanBodiesChangedEventArgs eventArgs)
        {
            if (!isRecording) return;
            
            // Process updated bodies
            foreach (var humanBody in eventArgs.updated)
            {
                RecordHipPose(humanBody);
            }
            
            // Process newly added bodies
            foreach (var humanBody in eventArgs.added)
            {
                RecordHipPose(humanBody);
            }
        }
        
        private void RecordHipPose(ARHumanBody humanBody)
        {
            if (!isRecording || humanBody.trackingState != TrackingState.Tracking)
                return;
            
            // Check frame limit
            if (recordedHipPoses.Count >= maxRecordingFrames)
            {
                Logger.LogWarning($"⚠️ Recording frame limit reached ({maxRecordingFrames})", "RECORDER");
                return;
            }
            
            // Get hip position using the new joints API
            var joints = humanBody.joints;
            if (joints.IsCreated && joints.Length > 0)
            {
                // Hip joint is typically at index 1 (after root at index 0)
                var hipIndex = 1;
                
                if (hipIndex < joints.Length)
                {
                    var hipJoint = joints[hipIndex];
                    
                    // Transform to world space
                    var hipWorldPos = humanBody.transform.TransformPoint(hipJoint.localPose.position);
                    var hipWorldRot = humanBody.transform.rotation * hipJoint.localPose.rotation;
                    
                    // Create pose data
                    var poseData = new PoseData(hipWorldPos, hipWorldRot, Time.time - recordingStartTime);
                    recordedHipPoses.Add(poseData);
                    
                    frameCount++;
                    
                    // Log progress occasionally
                    if (logRecordingProgress && frameCount % 30 == 0) // Every second at 30fps
                    {
                        Logger.Log($"📹 Recording: {frameCount} frames, {RecordingDuration:F1}s", "RECORDER");
                    }
                }
            }
        }
        
        private bool SaveRecordingToFile()
        {
            try
            {
                // Create BodyTrackingData
                var bodyTrackingData = new BodyTrackingData(recordedHipPoses.Count);
                
                // Copy recorded poses
                for (int i = 0; i < recordedHipPoses.Count; i++)
                {
                    bodyTrackingData.hipPoses[i] = recordedHipPoses[i];
                }
                
                // Set metadata
                bodyTrackingData.recordingDuration = Time.time - recordingStartTime;
                bodyTrackingData.recordingId = System.Guid.NewGuid().ToString();
                bodyTrackingData.recordingTimestamp = System.DateTime.UtcNow;
                
                // Serialize to JSON
                var json = JsonUtility.ToJson(bodyTrackingData, true);
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(currentRecordingPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Write to file
                File.WriteAllText(currentRecordingPath, json);
                
                Logger.Log($"💾 Saved recording: {Path.GetFileName(currentRecordingPath)}", "RECORDER");
                return true;
            }
            catch (System.Exception e)
            {
                Logger.LogError($"❌ Failed to save recording: {e.Message}", "RECORDER");
                return false;
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (humanBodyManager != null)
            {
                humanBodyManager.humanBodiesChanged -= OnHumanBodiesChanged;
            }
            
            // Save recording if still recording
            if (isRecording)
            {
                StopRecording();
            }
        }
    }
} 