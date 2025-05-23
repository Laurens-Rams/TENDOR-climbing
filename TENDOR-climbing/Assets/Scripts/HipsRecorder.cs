using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class HipsRecorder : MonoBehaviour
{
    private GameObject wallInstance;
    private GameObject avatarInstance;
    private TMPro.TextMeshProUGUI debugText;
    
    private List<SerializablePose> recordedPoses;
    private float recordingStartTime;
    private bool isRecording = false;
    private bool isInitialized = false;

    public void Initialize(GameObject wall, GameObject avatarPrefab, TMPro.TextMeshProUGUI debugUI)
    {
        wallInstance = wall;
        debugText = debugUI;
        
        if (avatarPrefab != null && wallInstance != null)
        {
            avatarInstance = Instantiate(avatarPrefab, wallInstance.transform.position, wallInstance.transform.rotation);
            Debug.Log("[HipsRecorder] Avatar placed for recording");
            UpdateDebugText("Ready - Press record to start");
            isInitialized = true;
        }
        else
        {
            Debug.LogError("[HipsRecorder] Failed to initialize - missing wall or avatar prefab");
            UpdateDebugText("Setup failed - missing prefabs");
        }
        
        recordedPoses = new List<SerializablePose>();
    }

    void OnDisable()
    {
        StopRecording();
        if (avatarInstance)
        {
            Destroy(avatarInstance);
            avatarInstance = null;
        }
        isInitialized = false;
    }

    public void StartRecording()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[HipsRecorder] Cannot start recording - not initialized. Make sure image target is detected first.");
            UpdateDebugText("Scan image target first!");
            return;
        }
        
        if (!avatarInstance || wallInstance == null)
        {
            Debug.LogWarning("[HipsRecorder] Cannot start recording - setup not ready");
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
        if (!isRecording || !avatarInstance || wallInstance == null) return;

        // Record the current pose relative to the wall
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
            UpdateDebugText("No poses recorded!");
            return;
        }

        var data = new SerializablePoseCollection { poses = recordedPoses };
        string json = JsonUtility.ToJson(data);
        string path = Path.Combine(Application.persistentDataPath, "hips.json");

        try
        {
            File.WriteAllText(path, json);
            Debug.Log($"[HipsRecorder] Saved {recordedPoses.Count} poses to {path}");
            UpdateDebugText($"Saved {recordedPoses.Count} poses to file");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[HipsRecorder] Failed to save recording: {e.Message}");
            UpdateDebugText("Error saving recording!");
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

    private void UpdateDebugText(string message)
    {
        if (debugText != null)
        {
            debugText.text = message;
        }
        Debug.Log($"[HipsRecorder] {message}");
    }
} 