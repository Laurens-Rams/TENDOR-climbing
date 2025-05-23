using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.XR.ARFoundation;

[System.Serializable]
public class JointData
{
    public int jointType;
    public Vector3 position;
    public Quaternion rotation;
    public bool isTracked;
}

[System.Serializable]
public class SkeletalFrame
{
    public float time;
    public List<JointData> joints = new List<JointData>();
}

[System.Serializable]
public class SkeletalRecording
{
    public List<SkeletalFrame> frames = new List<SkeletalFrame>();
}

public class SkeletalRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    [SerializeField] private float recordingFrameRate = 30f;
    
    private GameObject wallInstance;
    private ARHumanBodyManager bodyManager;
    private TMPro.TextMeshProUGUI debugText;
    
    private SkeletalRecording recording;
    private float recordingStartTime;
    private float lastRecordTime;
    private bool isRecording = false;
    private bool isInitialized = false;

    public void Initialize(GameObject wall, ARHumanBodyManager humanBodyManager, TMPro.TextMeshProUGUI debugUI)
    {
        wallInstance = wall;
        bodyManager = humanBodyManager;
        debugText = debugUI;
        
        if (wallInstance != null && bodyManager != null)
        {
            isInitialized = true;
            recording = new SkeletalRecording();
            Debug.Log("[SkeletalRecorder] Initialized for full skeletal recording");
            UpdateDebugText("Skeletal recorder ready");
        }
        else
        {
            Debug.LogError("[SkeletalRecorder] Failed to initialize - missing wall or body manager");
            UpdateDebugText("Skeletal setup failed");
        }
    }

    void OnDisable()
    {
        StopRecording();
        isInitialized = false;
    }

    public void StartRecording()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[SkeletalRecorder] Cannot start recording - not initialized");
            UpdateDebugText("Skeletal tracking not ready");
            return;
        }

        recording.frames.Clear();
        recordingStartTime = Time.time;
        lastRecordTime = 0f;
        isRecording = true;
        
        Debug.Log("[SkeletalRecorder] Skeletal recording started");
        UpdateDebugText("Recording full body motion...");
    }

    public void StopRecording()
    {
        if (!isRecording) return;

        isRecording = false;
        SaveSkeletalRecording();
        Debug.Log("[SkeletalRecorder] Skeletal recording stopped");
        UpdateDebugText($"Skeletal recording saved - {recording.frames.Count} frames");
    }

    void Update()
    {
        if (!isRecording || !isInitialized || wallInstance == null || bodyManager == null) return;

        float currentTime = Time.time - recordingStartTime;
        
        // Record at specified frame rate
        if (currentTime - lastRecordTime >= 1f / recordingFrameRate)
        {
            RecordCurrentSkeleton(currentTime);
            lastRecordTime = currentTime;
        }
    }

    private void RecordCurrentSkeleton(float time)
    {
        // Get the current human body
        if (bodyManager.humanBodies.count == 0)
        {
            // No body detected, record empty frame
            var emptyFrame = new SkeletalFrame { time = time };
            recording.frames.Add(emptyFrame);
            return;
        }

        var humanBody = bodyManager.humanBodies[0]; // Use first detected body
        if (humanBody.pose == null)
        {
            return;
        }

        var frame = new SkeletalFrame { time = time };

        // Record all joint positions relative to the wall
        for (int i = 0; i < humanBody.pose.joints.Length; i++)
        {
            var joint = humanBody.pose.joints[i];
            
            var jointData = new JointData
            {
                jointType = i,
                position = wallInstance.transform.InverseTransformPoint(joint.localPose.position),
                rotation = Quaternion.Inverse(wallInstance.transform.rotation) * joint.localPose.rotation,
                isTracked = joint.tracked
            };
            
            frame.joints.Add(jointData);
        }

        recording.frames.Add(frame);

        // Update UI periodically
        if (recording.frames.Count % 90 == 0)
        {
            UpdateDebugText($"Recording... {recording.frames.Count} frames");
        }
    }

    private void SaveSkeletalRecording()
    {
        if (recording == null || recording.frames.Count == 0)
        {
            Debug.LogWarning("[SkeletalRecorder] No skeletal frames to save");
            UpdateDebugText("No skeletal data recorded!");
            return;
        }

        string json = JsonUtility.ToJson(recording);
        string path = Path.Combine(Application.persistentDataPath, "skeletal_recording.json");

        try
        {
            File.WriteAllText(path, json);
            Debug.Log($"[SkeletalRecorder] Saved {recording.frames.Count} skeletal frames to {path}");
            UpdateDebugText($"Saved {recording.frames.Count} skeletal frames");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SkeletalRecorder] Failed to save skeletal recording: {e.Message}");
            UpdateDebugText("Error saving skeletal data!");
        }
    }

    public bool IsInitialized()
    {
        return isInitialized;
    }

    public bool IsRecording()
    {
        return isRecording;
    }

    public bool HasBodyTracking()
    {
        return bodyManager != null && bodyManager.humanBodies.count > 0;
    }

    private void UpdateDebugText(string message)
    {
        if (debugText != null)
        {
            debugText.text = message;
        }
        Debug.Log($"[SkeletalRecorder] {message}");
    }
} 