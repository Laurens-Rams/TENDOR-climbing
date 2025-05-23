using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HipsPlayback : MonoBehaviour
{
    private GameObject wallInstance;
    private GameObject avatarInstance;
    private GameObject skeletonInstance;
    private TMPro.TextMeshProUGUI debugText;

    private List<SerializablePose> poses;
    private float startTime;
    private int index;
    private bool isInitialized = false;

    public void Initialize(GameObject wall, GameObject avatarPrefab, GameObject skeletonPrefab, TMPro.TextMeshProUGUI debugUI)
    {
        wallInstance = wall;
        debugText = debugUI;
        
        Load();
        
        if (poses != null && poses.Count > 0 && avatarPrefab != null && wallInstance != null)
        {
            PlaceAvatar(avatarPrefab, skeletonPrefab);
            isInitialized = true;
            Debug.Log($"[HipsPlayback] Initialized with {poses.Count} poses");
            UpdateDebugText($"Playback ready - {poses.Count} poses loaded");
        }
        else if (poses == null || poses.Count == 0)
        {
            Debug.LogWarning("[HipsPlayback] No recording found to playback");
            UpdateDebugText("No recording found - record first!");
        }
        else
        {
            Debug.LogError("[HipsPlayback] Failed to initialize - missing wall or avatar prefab");
            UpdateDebugText("Setup failed - missing prefabs");
        }
    }

    void OnDisable()
    {
        CleanupPlaybackInstances();
        isInitialized = false;
    }

    private void CleanupPlaybackInstances()
    {
        if (avatarInstance) Destroy(avatarInstance);
        avatarInstance = null;
        if (skeletonInstance) Destroy(skeletonInstance);
        skeletonInstance = null;
    }

    private void Load()
    {
        string path = Path.Combine(Application.persistentDataPath, "hips.json");
        Debug.Log($"[HipsPlayback] Attempting to load from: {path}");
        
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogError("[HipsPlayback] JSON file is empty!");
                    UpdateDebugText("Recording file is empty!");
                    return;
                }
                
                var data = JsonUtility.FromJson<SerializablePoseCollection>(json);
                if (data != null && data.poses != null)
                {
                    poses = data.poses;
                    Debug.Log($"[HipsPlayback] Successfully loaded {poses.Count} poses");
                }
                else
                {
                    Debug.LogError("[HipsPlayback] Failed to parse JSON data");
                    UpdateDebugText("Invalid recording file!");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[HipsPlayback] Error loading JSON: {e.Message}");
                UpdateDebugText("Error loading recording!");
            }
        }
        else
        {
            Debug.LogWarning($"[HipsPlayback] No recording file found at {path}");
            UpdateDebugText("No recording file found!");
        }
    }

    private void PlaceAvatar(GameObject avatarPrefab, GameObject skeletonPrefab)
    {
        if (poses == null || poses.Count == 0 || avatarPrefab == null || wallInstance == null) return;

        // Initialize avatar at the first recorded position relative to the wall
        Vector3 localPos = poses[0].position;
        Quaternion localRot = poses[0].rotation;
        
        Vector3 worldPos = wallInstance.transform.TransformPoint(localPos);
        Quaternion worldRot = wallInstance.transform.rotation * localRot;
        
        avatarInstance = Instantiate(avatarPrefab, worldPos, worldRot);
        
        if (avatarInstance == null)
        {
            Debug.LogError("[HipsPlayback] Failed to instantiate avatar!");
            UpdateDebugText("Failed to create avatar!");
            return;
        }
        
        // Add skeleton visualization if needed
        if (skeletonPrefab)
        {
            skeletonInstance = Instantiate(skeletonPrefab, worldPos, worldRot, avatarInstance.transform);
            var skeleton = skeletonInstance.GetComponent<StickFigureSkeleton>();
            if (skeleton)
            {
                skeleton.enabled = true;
                Debug.Log("[HipsPlayback] Skeleton visualization enabled");
            }
        }
        
        startTime = Time.time;
        index = 0;
    }

    void Update()
    {
        if (!isInitialized || avatarInstance == null || poses == null || poses.Count == 0 || wallInstance == null) return;
        
        float elapsed = Time.time - startTime;
        
        // Update the current position in the animation
        while (index + 1 < poses.Count && poses[index + 1].time <= elapsed)
            index++;
        
        // Loop the animation
        if (index >= poses.Count - 1)
        {
            startTime = Time.time;
            index = 0;
            return;
        }
        
        if (index + 1 < poses.Count)
        {
            var currentPose = poses[index];
            var nextPose = poses[index + 1];
            float t = Mathf.InverseLerp(currentPose.time, nextPose.time, elapsed);
            
            // Interpolate position and rotation in local space
            Vector3 localPos = Vector3.Lerp(currentPose.position, nextPose.position, t);
            Quaternion localRot = Quaternion.Slerp(currentPose.rotation, nextPose.rotation, t);
            
            // Transform to world space relative to wall
            Vector3 worldPos = wallInstance.transform.TransformPoint(localPos);
            Quaternion worldRot = wallInstance.transform.rotation * localRot;
            
            // Update avatar position and rotation
            avatarInstance.transform.SetPositionAndRotation(worldPos, worldRot);
            
            if (Time.frameCount % 90 == 0)
            {
                UpdateDebugText($"Playing: {index}/{poses.Count-1} (Loop)");
            }
        }
    }
    
    public bool IsInitialized()
    {
        return isInitialized;
    }
    
    public bool HasRecording()
    {
        return poses != null && poses.Count > 0;
    }
    
    private void UpdateDebugText(string message)
    {
        if (debugText != null)
        {
            debugText.text = message;
        }
        Debug.Log($"[HipsPlayback] {message}");
    }
} 
} 