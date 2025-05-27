using UnityEngine;
using TENDOR.Core;
using TENDOR.Runtime.Models;
using System.IO;
using Logger = TENDOR.Core.Logger;

namespace TENDOR.Recording
{
    /// <summary>
    /// Plays back recorded hip tracking data from JSON files
    /// Creates visual spheres to show hip positions during playback
    /// </summary>
    public class BodyTrackingPlayer : MonoBehaviour
    {
        [Header("Playback Settings")]
        [SerializeField] private bool loopPlayback = false;
        [SerializeField] private float playbackSpeed = 1.0f;
        
        [Header("Visualization")]
        [SerializeField] private GameObject hipSpherePrefab;
        [SerializeField] private float sphereSize = 0.1f;
        [SerializeField] private Color sphereColor = Color.red;
        
        private bool isPlaying = false;
        private BodyTrackingData currentRecording;
        private float playbackStartTime;
        private int currentFrameIndex = 0;
        private GameObject hipSphere;
        
        public bool IsPlaying => isPlaying;
        public float PlaybackProgress => currentRecording != null && currentRecording.recordingDuration > 0 
            ? Mathf.Clamp01((Time.time - playbackStartTime) * playbackSpeed / currentRecording.recordingDuration) 
            : 0f;
        public int CurrentFrame => currentFrameIndex;
        public int TotalFrames => currentRecording?.hipPoses?.Length ?? 0;
        
        public void Initialize()
        {
            // Create hip sphere if no prefab provided
            if (hipSpherePrefab == null)
            {
                CreateDefaultHipSphere();
            }
            
            Logger.Log("🎬 BodyTrackingPlayer initialized", "PLAYER");
        }
        
        private void CreateDefaultHipSphere()
        {
            // Create a simple sphere for hip visualization
            var sphereGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphereGO.name = "HipSphere";
            sphereGO.transform.localScale = Vector3.one * sphereSize;
            
            // Set color
            var renderer = sphereGO.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Standard"));
                material.color = sphereColor;
                material.SetFloat("_Metallic", 0.0f);
                material.SetFloat("_Smoothness", 0.5f);
                renderer.material = material;
            }
            
            // Remove collider
            var collider = sphereGO.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }
            
            // Hide initially
            sphereGO.SetActive(false);
            
            hipSpherePrefab = sphereGO;
            Logger.Log("🔴 Created default hip sphere", "PLAYER");
        }
        
        public bool StartPlayback(string recordingPath)
        {
            if (isPlaying)
            {
                Logger.LogWarning("⚠️ Already playing", "PLAYER");
                return false;
            }
            
            if (!File.Exists(recordingPath))
            {
                Logger.LogError($"❌ Recording file not found: {recordingPath}", "PLAYER");
                return false;
            }
            
            // Load recording
            if (!LoadRecording(recordingPath))
            {
                return false;
            }
            
            // Start playback
            isPlaying = true;
            playbackStartTime = Time.time;
            currentFrameIndex = 0;
            
            // Create hip sphere instance
            CreateHipSphereInstance();
            
            Logger.Log($"▶️ Started playback: {Path.GetFileName(recordingPath)}", "PLAYER");
            return true;
        }
        
        public bool StopPlayback()
        {
            if (!isPlaying)
            {
                Logger.LogWarning("⚠️ Not currently playing", "PLAYER");
                return false;
            }
            
            isPlaying = false;
            currentRecording = null;
            currentFrameIndex = 0;
            
            // Destroy hip sphere instance
            DestroyHipSphereInstance();
            
            Logger.Log("⏹️ Stopped playback", "PLAYER");
            return true;
        }
        
        private bool LoadRecording(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                currentRecording = JsonUtility.FromJson<BodyTrackingData>(json);
                
                if (currentRecording == null || !currentRecording.IsValid)
                {
                    Logger.LogError($"❌ Invalid recording data: {Path.GetFileName(filePath)}", "PLAYER");
                    return false;
                }
                
                Logger.Log($"📁 Loaded recording: {currentRecording.hipPoses.Length} frames, {currentRecording.recordingDuration:F1}s", "PLAYER");
                return true;
            }
            catch (System.Exception e)
            {
                Logger.LogError($"❌ Failed to load recording: {e.Message}", "PLAYER");
                return false;
            }
        }
        
        private void CreateHipSphereInstance()
        {
            if (hipSpherePrefab != null && hipSphere == null)
            {
                hipSphere = Instantiate(hipSpherePrefab);
                hipSphere.name = "HipSphere_Playback";
                hipSphere.SetActive(true);
                Logger.Log("🔴 Created hip sphere instance", "PLAYER");
            }
        }
        
        private void DestroyHipSphereInstance()
        {
            if (hipSphere != null)
            {
                Destroy(hipSphere);
                hipSphere = null;
                Logger.Log("🗑️ Destroyed hip sphere instance", "PLAYER");
            }
        }
        
        public void UpdatePlayback()
        {
            if (!isPlaying || currentRecording == null || currentRecording.hipPoses == null)
                return;
            
            // Calculate current time in recording
            var currentTime = (Time.time - playbackStartTime) * playbackSpeed;
            
            // Check if playback is complete
            if (currentTime >= currentRecording.recordingDuration)
            {
                if (loopPlayback)
                {
                    // Restart playback
                    playbackStartTime = Time.time;
                    currentFrameIndex = 0;
                    currentTime = 0f;
                }
                else
                {
                    // Stop playback
                    StopPlayback();
                    return;
                }
            }
            
            // Find the appropriate frame for current time
            UpdateCurrentFrame(currentTime);
            
            // Update hip sphere position
            UpdateHipSpherePosition();
        }
        
        private void UpdateCurrentFrame(float currentTime)
        {
            // Find frame closest to current time
            int bestFrame = 0;
            float bestTimeDiff = float.MaxValue;
            
            for (int i = 0; i < currentRecording.hipPoses.Length; i++)
            {
                var pose = currentRecording.hipPoses[i];
                if (pose.timestamp == 0f) break; // End of valid data
                
                var timeDiff = Mathf.Abs(pose.timestamp - currentTime);
                if (timeDiff < bestTimeDiff)
                {
                    bestTimeDiff = timeDiff;
                    bestFrame = i;
                }
                else
                {
                    break; // Times are increasing, so we've found the best match
                }
            }
            
            currentFrameIndex = bestFrame;
        }
        
        private void UpdateHipSpherePosition()
        {
            if (hipSphere == null || currentFrameIndex >= currentRecording.hipPoses.Length)
                return;
            
            var currentPose = currentRecording.hipPoses[currentFrameIndex];
            if (currentPose.timestamp > 0f) // Valid pose data
            {
                hipSphere.transform.position = currentPose.position;
                hipSphere.transform.rotation = currentPose.rotation;
                
                // Ensure sphere is visible
                if (!hipSphere.activeInHierarchy)
                {
                    hipSphere.SetActive(true);
                }
            }
        }
        
        public void SetPlaybackSpeed(float speed)
        {
            playbackSpeed = Mathf.Max(0.1f, speed);
            Logger.Log($"⚡ Playback speed: {playbackSpeed:F1}x", "PLAYER");
        }
        
        public void SetLooping(bool loop)
        {
            loopPlayback = loop;
            Logger.Log($"🔄 Looping: {(loopPlayback ? "ON" : "OFF")}", "PLAYER");
        }
        
        public void SeekToTime(float time)
        {
            if (!isPlaying || currentRecording == null)
                return;
            
            time = Mathf.Clamp(time, 0f, currentRecording.recordingDuration);
            playbackStartTime = Time.time - (time / playbackSpeed);
            
            Logger.Log($"⏭️ Seeked to: {time:F1}s", "PLAYER");
        }
        
        public void SeekToFrame(int frame)
        {
            if (!isPlaying || currentRecording == null || currentRecording.hipPoses == null)
                return;
            
            frame = Mathf.Clamp(frame, 0, currentRecording.hipPoses.Length - 1);
            if (frame < currentRecording.hipPoses.Length)
            {
                var targetTime = currentRecording.hipPoses[frame].timestamp;
                SeekToTime(targetTime);
            }
        }
        
        private void OnDestroy()
        {
            // Clean up
            if (isPlaying)
            {
                StopPlayback();
            }
            
            // Destroy prefab if we created it
            if (hipSpherePrefab != null && hipSpherePrefab.name == "HipSphere")
            {
                Destroy(hipSpherePrefab);
            }
        }
    }
} 