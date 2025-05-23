using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using RenderHeads.Media.AVProMovieCapture;

public class HipsRecorder : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField]  private ARHumanBodyManager   bodyManager;
    [SerializeField]  private ARTrackedImageManager imageManager;
    [SerializeField]  private CaptureFromTexture   capture;
    [SerializeField]  private RawImage             previewImage;   // ‼️ new – optional
    [SerializeField]  private GameObject           startButton;

    [Header("Debug / overlay")]
    [SerializeField]  private TMPro.TextMeshProUGUI frameCounter;  // optional
    [SerializeField]  private TMPro.TextMeshProUGUI debugText;     // new - for showing debug info

    // ────────────────────────────────────────────────────────────────
    private readonly List<SerializablePose> poses = new();
    private Transform   imageTransform;
    private bool        recording;
    private float       startTime;
    private Texture2D   tex;
    private XRCpuImage  cpuImage;
    
    private bool        eventRegistered = false;  // Track if event was registered

    // ────────────────────────────────────────────────────────────────
    #region Unity lifecycle
    //----------------------------------------------------------------
    void OnEnable()
    {
        bodyManager  ??= FindFirstObjectByType<ARHumanBodyManager>();
        imageManager ??= FindFirstObjectByType<ARTrackedImageManager>();
        capture      ??= FindFirstObjectByType<CaptureFromTexture>();

        if (bodyManager != null)
        {
            Debug.Log("[HipsRecorder] ARHumanBodyManager found: " + bodyManager.name);
            Debug.Log("[HipsRecorder] Human body tracking enabled: " + bodyManager.enabled);
        
            Debug.Log("[HipsRecorder] Body tracking subsystem running: " + (bodyManager.subsystem != null && bodyManager.subsystem.running));
            
            // Check if human body pose is in the required features of AR session
            var arSession = FindFirstObjectByType<UnityEngine.XR.ARFoundation.ARSession>();
            if (arSession != null)
            {
                Debug.Log("[HipsRecorder] AR Session is " + (arSession.enabled ? "enabled" : "disabled"));
                var arSessionOrigin = FindFirstObjectByType<UnityEngine.XR.ARFoundation.ARSessionOrigin>();
                if (arSessionOrigin != null)
                {
                    Debug.Log("[HipsRecorder] AR Session Origin is " + (arSessionOrigin.enabled ? "enabled" : "disabled"));
                }
            }
            
            UpdateDebugText("Manager found: " + bodyManager.name);
        }
        else
        {
            Debug.LogError("[HipsRecorder] ARHumanBodyManager is null!");
            UpdateDebugText("ERROR: No body manager");
        }

        if (ViewSwitcher.isARMode)
        {
            enabled = false;
            Debug.Log("[HipsRecorder] Disabled – AR-mode active.");
            return;
        }

        imageManager.trackedImagesChanged += OnImagesChanged;
        startButton.SetActive(false);
        Debug.Log("[HipsRecorder] Waiting for image target…");
        
        lastEventCheckTime = Time.time;
    }

    private float lastEventCheckTime;
    private float eventCheckInterval = 3.0f; // Check every 3 seconds
    private int eventsReceivedCounter = 0;
    private int framesCounter = 0;
    
    void Update()
    {
        framesCounter++;
        
        // Debug for body tracking - check if we're detecting any bodies
        if (bodyManager != null && bodyManager.trackables.count > 0)
        {
            foreach (var body in bodyManager.trackables)
            {
                if (body != null)
                {
                    Debug.Log($"[HipsRecorder] Body detected: ID={body.trackableId}, has joints: {body.joints.IsCreated}");
                    UpdateDebugText($"Body detected: joints={body.joints.IsCreated}");
                    break;  // Just log the first one to avoid spamming
                }
            }
        }
        
        // Every few seconds, check if we're getting body events
        if (Time.time - lastEventCheckTime > eventCheckInterval)
        {
            lastEventCheckTime = Time.time;
            
            if (recording)
            {
                Debug.Log($"[HipsRecorder] Status check: Events received: {eventsReceivedCounter}, " +
                          $"Frames: {framesCounter}, Bodies tracked: {bodyManager?.trackables.count ?? 0}, " +
                          $"Poses collected: {poses.Count}");
                
                // If recording but not receiving events, try re-registering
                if (eventsReceivedCounter == 0 && bodyManager != null && recording)
                {
                    if (eventRegistered)
                    {
                        bodyManager.humanBodiesChanged -= OnBodiesChanged;
                        Debug.Log("[HipsRecorder] Re-registering event handler");
                    }
                    
                    bodyManager.humanBodiesChanged += OnBodiesChanged;
                    eventRegistered = true;
                    UpdateDebugText("Re-registered event handler");
                }
                
                eventsReceivedCounter = 0;
                framesCounter = 0;
                
                // Auto-save progress periodically while recording, every 30 seconds
                if (poses.Count > 0 && Time.time % 30 < 1.0f)
                {
                    string tempPath = Path.Combine(Application.persistentDataPath, "hips_temp.json");
                    try
                    {
                        var data = new SerializablePoseCollection { poses = poses };
                        string json = JsonUtility.ToJson(data);
                        File.WriteAllText(tempPath, json);
                        Debug.Log($"[HipsRecorder] Auto-saved {poses.Count} poses to temp file");
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[HipsRecorder] Failed to auto-save: {e.Message}");
                    }
                }
            }
        }
        
        // Debug keyboard shortcut for saving (for testing in editor)
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("[HipsRecorder] Manual save triggered by F5 key");
            TestSave();
        }
        #endif
    }

    void OnDisable()
    {
        if (eventRegistered) 
        {
            bodyManager.humanBodiesChanged -= OnBodiesChanged;
            eventRegistered = false;
            Debug.Log("[HipsRecorder] Unregistered bodies changed event");
        }
        
        imageManager.trackedImagesChanged -= OnImagesChanged;
        Globals.CameraManager.frameReceived -= RecordFrame;

        if (startButton) startButton.SetActive(false);
    }
    #endregion
    // ────────────────────────────────────────────────────────────────

    #region Image tracking
    //----------------------------------------------------------------
    private void OnImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var img in args.added)   TryAssign(img);
        foreach (var img in args.updated) TryAssign(img);
    }

    private void TryAssign(ARTrackedImage img)
    {
        Debug.Log($"[HipsRecorder] TryAssign ➜ {img.referenceImage.name} ({img.trackingState})");

        if (imageTransform == null &&
            (img.referenceImage.name == "Wall 1" ||
             img.referenceImage.name == "test-target" ||
             img.referenceImage.name == "Wall_1" ||
             img.referenceImage.name == "Target" ||
             img.referenceImage.name.Contains("Wall") ||
             img.referenceImage.name.Contains("Target")) &&
            img.trackingState == TrackingState.Tracking)
        {
            imageTransform = img.transform;
            startButton.SetActive(true);
            Debug.Log($"[HipsRecorder] 🎯 Target found: {img.referenceImage.name} – start button enabled");
            UpdateDebugText($"Target found: {img.referenceImage.name}");
        }
    }
    #endregion
    // ────────────────────────────────────────────────────────────────

    #region Recording control (UI hook)
    //----------------------------------------------------------------
    public void StartRecording()
    {
        if (recording || imageTransform == null) 
        { 
            Debug.LogWarning("[HipsRecorder] Cannot start – already recording or target missing"); 
            UpdateDebugText("Cannot start - check logs");
            return; 
        }

        // ── Check if AR body tracking is set up properly ──
        if (bodyManager == null)
        {
            bodyManager = FindFirstObjectByType<ARHumanBodyManager>();
            if (bodyManager == null)
            {
                Debug.LogError("[HipsRecorder] No ARHumanBodyManager found in the scene!");
                UpdateDebugText("ERROR: No body tracker");
                return;
            }
        }

        // Make sure the body manager is enabled and running
        bodyManager.enabled = true;
        
        // Verify the subsystem is available and can be started
        if (bodyManager.subsystem == null)
        {
            Debug.LogWarning("[HipsRecorder] Body tracking subsystem not available!");
            UpdateDebugText("No body tracking subsystem");
            // Still continue to try recording - maybe debug builds will work differently
        }
        else if (!bodyManager.subsystem.running)
        {
            Debug.Log("[HipsRecorder] Starting body tracking subsystem");
            try
            {
                bodyManager.subsystem.Start();
            }
            catch (Exception e)
            {
                Debug.LogError($"[HipsRecorder] Failed to start body tracking: {e.Message}");
                UpdateDebugText("Failed to start tracking");
                // Continue anyway to see if it works
            }
        }

        // ── 1. video setup (identical to your VideoRecorder) ──
        int w = (int)Globals.CameraManager.currentConfiguration?.width;
        int h = (int)Globals.CameraManager.currentConfiguration?.height;
        tex = new Texture2D(w, h, TextureFormat.RGBA32, false);

        if (previewImage) previewImage.texture = tex;   // live preview (optional)

        capture.SetSourceTexture(tex);
        capture.StartCapture();
        Debug.Log($"[HipsRecorder] ▶️ AVPro capture started ({w}×{h})");

        Globals.CameraManager.frameReceived += RecordFrame;

        // ── 2. pose setup ──
        poses.Clear();
        startTime = Time.time;
        
        // Check if bodyManager is valid before registering event
        if (bodyManager != null)
        {
            // First ensure we're not already registered
            if (eventRegistered)
            {
                bodyManager.humanBodiesChanged -= OnBodiesChanged;
            }
            
            bodyManager.humanBodiesChanged += OnBodiesChanged;
            eventRegistered = true;
            Debug.Log("[HipsRecorder] Registered humanBodiesChanged event");
            
            UpdateDebugText("Recording started - event registered");
        }
        else
        {
            Debug.LogError("[HipsRecorder] Cannot start - bodyManager is null!");
            UpdateDebugText("ERROR: No body manager");
            return;
        }

        if (frameCounter) frameCounter.text = "0";

        startButton.SetActive(false);
        recording = true;
    }

    public void StopRecording()
    {
        if (!recording) 
        {
            Debug.LogWarning("[HipsRecorder] Cannot stop - not recording");
            return;
        }

        Debug.Log($"[HipsRecorder] Stopping recording with {poses.Count} poses collected");

        // 1. video off
        Globals.CameraManager.frameReceived -= RecordFrame;
        capture.StopCapture();
        Debug.Log("[HipsRecorder] ⏹️ AVPro capture stopped");

        // 2. pose off
        if (eventRegistered)
        {
            bodyManager.humanBodiesChanged -= OnBodiesChanged;
            eventRegistered = false;
            Debug.Log("[HipsRecorder] Unregistered humanBodiesChanged event");
        }
        
        // 3. save the data
        Save();
        Debug.Log("[HipsRecorder] Save completed");

        startButton.SetActive(true);
        recording = false;
        UpdateDebugText($"Recording stopped - {poses.Count} poses");
    }

    // Test method to save directly
    public void TestSave()
    {
        Debug.Log($"[HipsRecorder] Test save triggered with {poses.Count} poses");
        if (poses.Count == 0)
        {
            // Add a test pose if none exist
            poses.Add(new SerializablePose(new Vector3(1, 2, 3), Quaternion.Euler(45, 45, 45), 0.5f));
            Debug.Log("[HipsRecorder] Added test pose");
        }
        Save();
    }
    #endregion
    // ────────────────────────────────────────────────────────────────

    #region Frame capture + pose tracking
    //----------------------------------------------------------------
    private void RecordFrame(ARCameraFrameEventArgs args)
    {
        if (!Globals.CameraManager.TryAcquireLatestCpuImage(out cpuImage)) return;

        var conv = new XRCpuImage.ConversionParams(cpuImage, TextureFormat.RGBA32, XRCpuImage.Transformation.MirrorY);
        var data = tex.GetRawTextureData<byte>();
        cpuImage.Convert(conv, data);
        tex.Apply();

        capture.UpdateSourceTexture();
        capture.UpdateFrame();
        cpuImage.Dispose();

        if (frameCounter)
        {
            int n = int.Parse(frameCounter.text);
            frameCounter.text = (n + 1).ToString();
        }
    }

    private void OnBodiesChanged(ARHumanBodiesChangedEventArgs args)
    {
        eventsReceivedCounter++;
        
        // Debug body tracking status
        Debug.Log($"[Body] event fired – updated: {args.updated.Count}, added: {args.added.Count}, removed: {args.removed.Count}, recording: {recording}");

        if (!recording)
        {
            Debug.Log("[Body] Event fired but not recording - ignoring");
            return;
        }

        // Process any tracked bodies (either updated or added)
        List<ARHumanBody> bodiesToProcess = new List<ARHumanBody>();
        bodiesToProcess.AddRange(args.updated);
        bodiesToProcess.AddRange(args.added);
        
        if (bodiesToProcess.Count == 0 || imageTransform == null) 
        {
            UpdateDebugText("No bodies or no target");
            return;
        }

        // ————————————————————————————————————————
        // Process the first available body
        var body = bodiesToProcess[0];
        
        // Double check the body is valid
        if (body == null)
        {
            Debug.LogWarning("[Body] Body reference is null");
            return;
        }
        
        // Log body information
        Debug.Log($"[Body] Processing body ID={body.trackableId}");
        
        // Access joints safely
        if (!body.joints.IsCreated)
        {
            Debug.LogWarning("[Body] Joints array is not created");
            UpdateDebugText("No joints data available");
            return;
        }
        
        int jointCount = body.joints.Length;
        Debug.Log($"[Body] Joint count: {jointCount}");
        
        if (jointCount == 0)
        {
            Debug.LogWarning("[Body] No joints in the array");
            UpdateDebugText("Joint array empty");
            return;
        }
        
        // Log all joint tracking states
        for (int i = 0; i < Math.Min(jointCount, 5); i++) // Just log first 5 to avoid spam
        {
            var joint = body.joints[i];
            Debug.Log($"[Body] Joint {i}: tracked={joint.tracked}, pos={joint.anchorPose.position}");
        }
        
        // Try to find any tracked joint instead of only using the hips joint
        int trackedJointIndex = -1;
        for (int i = 0; i < jointCount; i++)
        {
            if (body.joints[i].tracked)
            {
                trackedJointIndex = i;
                Debug.Log($"[Body] Found tracked joint at index {i}");
                break;  // Use the first tracked joint we find
            }
        }
        
        if (trackedJointIndex == -1)
        {
            Debug.Log("[Body] No tracked joints found in this frame");
            UpdateDebugText("No tracked joints");
            return;
        }
        
        // Use the tracked joint
        var trackedJoint = body.joints[trackedJointIndex];
        
        // ⬇️ Detailed debugging info
        Debug.Log($"[Body] joints.IsCreated = {body.joints.IsCreated}, " +
                 $"joints.Length = {jointCount}, " +
                 $"Using joint #{trackedJointIndex}, tracked = {trackedJoint.tracked}, " +
                 $"runningOn = {Application.platform}");

        UpdateDebugText($"Using joint #{trackedJointIndex}");

        // ————————————————————————————————————————
        // 3. convert to image-target local space
        Vector3 jointWorldPos = trackedJoint.anchorPose.position;
        Quaternion jointWorldRot = trackedJoint.anchorPose.rotation;

        Debug.Log($"[Body] Joint world pos: {jointWorldPos}, rot: {jointWorldRot.eulerAngles}");
        Debug.Log($"[Body] Image target pos: {imageTransform.position}, rot: {imageTransform.rotation.eulerAngles}");

        Vector3 localPos = imageTransform.InverseTransformPoint(jointWorldPos);
        Quaternion localRot = Quaternion.Inverse(imageTransform.rotation) * jointWorldRot;

        Debug.Log($"[Body] Joint local pos: {localPos}, rot: {localRot.eulerAngles}");

        poses.Add(new SerializablePose(localPos, localRot, Time.time - startTime));
        Debug.Log($"[HipsRecorder] Pose #{poses.Count} recorded at {Time.time - startTime:F2}s (joint #{trackedJointIndex})");
        UpdateDebugText($"Recording: {poses.Count} poses");
    }
    #endregion
    // ────────────────────────────────────────────────────────────────

    #region Save JSON
    //----------------------------------------------------------------
    private void Save()
    {
        if (poses.Count == 0)
        {
            Debug.LogWarning("[HipsRecorder] No poses to save!");
            UpdateDebugText("No poses to save!");
            return;
        }
        
        try 
        {
            // Create a clean copy of the pose collection to ensure proper serialization
            var cleanPoses = new List<SerializablePose>();
            foreach (var pose in poses)
            {
                cleanPoses.Add(new SerializablePose(pose.position, pose.rotation, pose.time));
            }
            
            var data = new SerializablePoseCollection { poses = cleanPoses };
            string json = JsonUtility.ToJson(data, true);
            
            // Log a sample of the JSON to verify data
            string jsonSample = json.Length > 200 ? json.Substring(0, 200) + "..." : json;
            Debug.Log($"[HipsRecorder] JSON data (sample): {jsonSample}");
            
            // Ensure the directory exists
            string directory = Application.persistentDataPath;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            string path = Path.Combine(directory, "hips.json");
            File.WriteAllText(path, json);
            
            // Verify the file was written
            if (File.Exists(path))
            {
                long fileSize = new FileInfo(path).Length;
                Debug.Log($"[HipsRecorder] Successfully saved {poses.Count} poses ➜ {path} (file size: {fileSize} bytes)");
                
                // Log the absolute path for easier finding
                Debug.Log($"[HipsRecorder] Absolute path: {Path.GetFullPath(path)}");
            }
            else
            {
                Debug.LogError($"[HipsRecorder] Failed to verify saved file at {path}");
            }
            
            UpdateDebugText($"Saved {poses.Count} poses to {Path.GetFileName(path)}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[HipsRecorder] Error saving poses: {e.Message}\n{e.StackTrace}");
            UpdateDebugText($"Error saving: {e.Message}");
        }
    }
    #endregion
    
    // ────────────────────────────────────────────────────────────────
    #region Helper methods
    //----------------------------------------------------------------
    private void UpdateDebugText(string message)
    {
        if (debugText != null)
        {
            debugText.text = message;
        }
    }
    #endregion
}
