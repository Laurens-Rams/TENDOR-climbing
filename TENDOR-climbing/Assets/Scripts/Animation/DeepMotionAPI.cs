using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace BodyTracking.Animation
{
    /// <summary>
    /// DeepMotion API integration for generating and downloading animations
    /// Handles the complete workflow from prompt to AnimationClip
    /// </summary>
    public class DeepMotionAPI : MonoBehaviour
    {
        [Header("API Configuration")]
        [SerializeField] private string apiKey;
        [SerializeField] private string baseUrl = "https://api.deepmotion.com/v1";
        [SerializeField] private float pollInterval = 5f; // Seconds between status checks
        [SerializeField] private float maxWaitTime = 300f; // Max wait time for job completion
        
        [Header("Download Settings")]
        [SerializeField] private string downloadFolder = "DeepMotionAnimations";
        [SerializeField] private bool deleteAfterImport = false;
        
        [Header("Debug")]
        [SerializeField] private bool enableLogging = true;
        
        // Events
        public System.Action<AnimationClip, string, string> OnAnimationReady; // clip, jobId, prompt
        public System.Action<string> OnError; // error message
        public System.Action<string, float> OnProgress; // jobId, progress (0-1)
        
        // Runtime state
        private Dictionary<string, JobRequest> activeJobs = new Dictionary<string, JobRequest>();
        
        [System.Serializable]
        private class JobRequest
        {
            public string jobId;
            public string prompt;
            public float startTime;
            public JobStatus status;
            public string downloadUrl;
        }
        
        private enum JobStatus
        {
            Submitted,
            Processing,
            Completed,
            Failed,
            Downloaded
        }
        
        void Start()
        {
            // Create download folder if it doesn't exist
            string fullPath = Path.Combine(Application.persistentDataPath, downloadFolder);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }
        
        #region Public API
        
        /// <summary>
        /// Request animation generation from DeepMotion API
        /// </summary>
        public void RequestAnimation(string prompt, AnimationManager animationManager = null)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                LogError("API key not set");
                return;
            }
            
            if (string.IsNullOrEmpty(prompt))
            {
                LogError("Prompt cannot be empty");
                return;
            }
            
            StartCoroutine(SubmitAnimationRequest(prompt, animationManager));
        }
        
        /// <summary>
        /// Set API key at runtime
        /// </summary>
        public void SetApiKey(string key)
        {
            apiKey = key;
        }
        
        /// <summary>
        /// Get status of all active jobs
        /// </summary>
        public Dictionary<string, string> GetActiveJobsStatus()
        {
            var status = new Dictionary<string, string>();
            foreach (var job in activeJobs.Values)
            {
                status[job.jobId] = $"{job.status} - {job.prompt}";
            }
            return status;
        }
        
        #endregion
        
        #region API Workflow
        
        /// <summary>
        /// Submit animation request to DeepMotion API
        /// </summary>
        private IEnumerator SubmitAnimationRequest(string prompt, AnimationManager animationManager)
        {
            Log($"Submitting animation request: {prompt}");
            
            // Create request data
            var requestData = new
            {
                prompt = prompt,
                duration = 10f, // Default duration
                character_type = "humanoid",
                output_format = "fbx"
            };
            
            string jsonData = JsonUtility.ToJson(requestData);
            
            // Create web request
            using (UnityWebRequest request = new UnityWebRequest($"{baseUrl}/generate", "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
                
                yield return request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogError($"Failed to submit request: {request.error}");
                    yield break;
                }
                
                // Parse response to get job ID
                var response = JsonUtility.FromJson<SubmitResponse>(request.downloadHandler.text);
                if (response == null || string.IsNullOrEmpty(response.job_id))
                {
                    LogError("Invalid response from API");
                    yield break;
                }
                
                // Create job tracking
                var job = new JobRequest
                {
                    jobId = response.job_id,
                    prompt = prompt,
                    startTime = Time.time,
                    status = JobStatus.Submitted
                };
                
                activeJobs[job.jobId] = job;
                Log($"Job submitted with ID: {job.jobId}");
                
                // Start polling for completion
                StartCoroutine(PollJobStatus(job, animationManager));
            }
        }
        
        /// <summary>
        /// Poll job status until completion
        /// </summary>
        private IEnumerator PollJobStatus(JobRequest job, AnimationManager animationManager)
        {
            while (job.status != JobStatus.Completed && job.status != JobStatus.Failed)
            {
                // Check timeout
                if (Time.time - job.startTime > maxWaitTime)
                {
                    job.status = JobStatus.Failed;
                    LogError($"Job {job.jobId} timed out");
                    break;
                }
                
                yield return new WaitForSeconds(pollInterval);
                
                // Check job status
                using (UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/jobs/{job.jobId}"))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
                    
                    yield return request.SendWebRequest();
                    
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        LogError($"Failed to check job status: {request.error}");
                        continue;
                    }
                    
                    var statusResponse = JsonUtility.FromJson<JobStatusResponse>(request.downloadHandler.text);
                    if (statusResponse == null)
                    {
                        LogError("Invalid status response");
                        continue;
                    }
                    
                    // Update job status
                    switch (statusResponse.status.ToLower())
                    {
                        case "processing":
                            job.status = JobStatus.Processing;
                            OnProgress?.Invoke(job.jobId, statusResponse.progress);
                            Log($"Job {job.jobId} processing: {statusResponse.progress * 100:F1}%");
                            break;
                            
                        case "completed":
                            job.status = JobStatus.Completed;
                            job.downloadUrl = statusResponse.download_url;
                            Log($"Job {job.jobId} completed");
                            break;
                            
                        case "failed":
                            job.status = JobStatus.Failed;
                            LogError($"Job {job.jobId} failed: {statusResponse.error_message}");
                            break;
                    }
                }
            }
            
            // Download and process if completed
            if (job.status == JobStatus.Completed)
            {
                yield return StartCoroutine(DownloadAndProcessAnimation(job, animationManager));
            }
            
            // Clean up
            activeJobs.Remove(job.jobId);
        }
        
        /// <summary>
        /// Download and process completed animation
        /// </summary>
        private IEnumerator DownloadAndProcessAnimation(JobRequest job, AnimationManager animationManager)
        {
            if (string.IsNullOrEmpty(job.downloadUrl))
            {
                LogError($"No download URL for job {job.jobId}");
                yield break;
            }
            
            Log($"Downloading animation for job {job.jobId}");
            
            // Download FBX file
            using (UnityWebRequest request = UnityWebRequest.Get(job.downloadUrl))
            {
                yield return request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    LogError($"Failed to download animation: {request.error}");
                    yield break;
                }
                
                // Save to file
                string fileName = $"deepmotion_{job.jobId}.fbx";
                string filePath = Path.Combine(Application.persistentDataPath, downloadFolder, fileName);
                
                // Save file without try-catch around yield
                bool saveSuccess = false;
                try
                {
                    File.WriteAllBytes(filePath, request.downloadHandler.data);
                    Log($"Animation saved to: {filePath}");
                    saveSuccess = true;
                }
                catch (System.Exception e)
                {
                    LogError($"Failed to save animation file: {e.Message}");
                    yield break;
                }
                
                // Process the FBX file only if save was successful
                if (saveSuccess)
                {
                    yield return StartCoroutine(ProcessFBXFile(filePath, job, animationManager));
                }
            }
        }
        
        /// <summary>
        /// Process downloaded FBX file and extract animation
        /// </summary>
        private IEnumerator ProcessFBXFile(string filePath, JobRequest job, AnimationManager animationManager)
        {
            // Note: In a real implementation, you would need to:
            // 1. Import the FBX into Unity's asset database (editor only)
            // 2. Extract the animation clip from the FBX
            // 3. Convert it to a runtime-usable AnimationClip
            
            // For now, this is a placeholder that simulates the process
            Log($"Processing FBX file: {filePath}");
            
            // Simulate processing time
            yield return new WaitForSeconds(1f);
            
            // In a real implementation, you would extract the actual animation clip here
            // For demonstration, we'll create a dummy clip
            AnimationClip dummyClip = CreateDummyAnimationClip(job.prompt);
            
            if (dummyClip != null)
            {
                // Notify that animation is ready
                OnAnimationReady?.Invoke(dummyClip, job.jobId, job.prompt);
                
                // Add to animation manager if provided
                if (animationManager != null)
                {
                    animationManager.LoadDeepMotionAnimation(dummyClip, job.jobId, job.prompt);
                }
                
                Log($"Animation ready for job {job.jobId}");
            }
            else
            {
                LogError($"Failed to process animation for job {job.jobId}");
            }
            
            // Clean up file if requested
            if (deleteAfterImport && File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    Log($"Cleaned up file: {filePath}");
                }
                catch (System.Exception e)
                {
                    LogError($"Failed to delete file: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// Create a dummy animation clip for demonstration
        /// In a real implementation, this would extract from the FBX
        /// </summary>
        private AnimationClip CreateDummyAnimationClip(string prompt)
        {
            // Create a simple animation clip
            AnimationClip clip = new AnimationClip();
            clip.name = $"DeepMotion_{prompt.Replace(" ", "_")}";
            clip.frameRate = 30f;
            
            // Add some basic animation curves (this is just for demonstration)
            AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 2f, 1f);
            clip.SetCurve("", typeof(Transform), "localPosition.y", curve);
            
            return clip;
        }
        
        #endregion
        
        #region Response Classes
        
        [System.Serializable]
        private class SubmitResponse
        {
            public string job_id;
            public string status;
        }
        
        [System.Serializable]
        private class JobStatusResponse
        {
            public string status;
            public float progress;
            public string download_url;
            public string error_message;
        }
        
        #endregion
        
        #region Logging
        
        private void Log(string message)
        {
            if (enableLogging)
            {
                Debug.Log($"[DeepMotionAPI] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[DeepMotionAPI] {message}");
            OnError?.Invoke(message);
        }
        
        #endregion
    }
} 