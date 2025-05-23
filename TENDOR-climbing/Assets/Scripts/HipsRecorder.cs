using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class HipsRecorder : ImageTrackingBase
{
    [SerializeField] private GameObject recordingAvatarPrefab;
    [SerializeField] private TMPro.TextMeshProUGUI debugText;
    
    private GameObject avatarInstance;
    private List<SerializablePose> recordedPoses;
    private float recordingStartTime;
    private bool isRecording = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        
        if (ViewSwitcher.isARMode)
        {
            enabled = false;
            UpdateDebugText("Disabled - in AR mode");
            return;
        }
        
        recordedPoses = new List<SerializablePose>();
        Debug.Log("[HipsRecorder] Ready to record");
        UpdateDebugText("Ready to record");
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        StopRecording();
        if (avatarInstance)
            Destroy(avatarInstance);
        avatarInstance = null;
    }

    protected override void OnWallPlaced(Vector3 position, Quaternion rotation)
    {
        if (recordingAvatarPrefab && !avatarInstance)
        {
            avatarInstance = Instantiate(recordingAvatarPrefab, position, rotation);
            Debug.Log("[HipsRecorder] Avatar placed for recording");
            UpdateDebugText("Ready - Press record to start");
        }
    }

    public void StartRecording()
    {
        if (!isWallPlaced || !avatarInstance)
        {
            Debug.LogWarning("[HipsRecorder] Cannot start recording - wall or avatar not ready");
            UpdateDebugText("Cannot record - setup not ready");
            return;
        }

        recordedPoses.Clear();
        recordingStartTime = Time.time;
        isRecording = true;
        Debug.Log("[HipsRecorder] Recording started");
        UpdateDebugText("Recording...");
    }

    public void StopRecording()
    {
        if (!isRecording) return;

        isRecording = false;
        SaveRecording();
        Debug.Log("[HipsRecorder] Recording stopped");
        UpdateDebugText($"Recording stopped - {recordedPoses.Count} poses saved");
    }

    private void Update()
    {
        if (!isRecording || !avatarInstance) return;

        // Record the current pose
        var currentPose = new SerializablePose
        {
            time = Time.time - recordingStartTime,
            position = wallInstance.transform.InverseTransformPoint(avatarInstance.transform.position),
            rotation = Quaternion.Inverse(wallInstance.transform.rotation) * avatarInstance.transform.rotation
        };

        recordedPoses.Add(currentPose);

        if (Time.frameCount % 90 == 0)
        {
            UpdateDebugText($"Recording... {recordedPoses.Count} poses");
        }
    }

    private void SaveRecording()
    {
        if (recordedPoses == null || recordedPoses.Count == 0)
        {
            Debug.LogWarning("[HipsRecorder] No poses to save");
            return;
        }

        var data = new SerializablePoseCollection { poses = recordedPoses };
        string json = JsonUtility.ToJson(data);
        string path = Path.Combine(Application.persistentDataPath, "hips.json");

        try
        {
            File.WriteAllText(path, json);
            Debug.Log($"[HipsRecorder] Saved {recordedPoses.Count} poses to {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[HipsRecorder] Failed to save recording: {e.Message}");
            UpdateDebugText("Error saving recording!");
        }
    }

    private void UpdateDebugText(string message)
    {
        if (debugText != null)
        {
            debugText.text = message;
        }
    }
} 