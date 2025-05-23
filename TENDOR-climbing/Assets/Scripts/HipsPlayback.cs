using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

public class HipsPlayback : ImageTrackingBase
{
    [SerializeField] private GameObject avatarPrefab;
    [SerializeField] private GameObject skeletonPrefab;
    [SerializeField] private TMPro.TextMeshProUGUI debugText;

    private List<SerializablePose> poses;
    private GameObject avatarInstance;
    private GameObject skeletonInstance;
    private float startTime;
    private int index;

    protected override void OnEnable()
    {
        base.OnEnable();
        
        Debug.Log("[HipsPlayback] OnEnable - AR mode: " + ViewSwitcher.isARMode);
        
        if (!ViewSwitcher.isARMode)
        {
            enabled = false;
            UpdateDebugText("Disabled - not in AR mode");
            return;
        }
        
        Load();
        
        if (poses != null && poses.Count > 0)
        {
            Debug.Log($"[HipsPlayback] Loaded {poses.Count} poses");
            UpdateDebugText($"Loaded {poses.Count} poses");
        }
        else
        {
            Debug.LogWarning("[HipsPlayback] No poses loaded from JSON!");
            UpdateDebugText("No poses loaded!");
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        CleanupPlaybackInstances();
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
                    UpdateDebugText("JSON file is empty!");
                    return;
                }
                
                var data = JsonUtility.FromJson<SerializablePoseCollection>(json);
                if (data != null)
                {
                    poses = data.poses;
                    Debug.Log($"[HipsPlayback] Successfully loaded {poses?.Count ?? 0} poses");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[HipsPlayback] Error loading JSON: {e.Message}");
                UpdateDebugText("Error loading JSON!");
            }
        }
        else
        {
            Debug.LogWarning($"[HipsPlayback] No recording file found at {path}");
            UpdateDebugText("No recording file found!");
        }
    }

    protected override void OnWallPlaced(Vector3 position, Quaternion rotation)
    {
        if (poses != null && poses.Count > 0 && avatarPrefab)
        {
            PlaceAvatar(position, rotation);
        }
    }

    private void PlaceAvatar(Vector3 wallPosition, Quaternion wallRotation)
    {
        if (poses == null || poses.Count == 0 || avatarPrefab == null) return;

        // Initialize avatar at the first recorded position
        Vector3 initialPos = wallPosition + (wallRotation * poses[0].position);
        avatarInstance = Instantiate(avatarPrefab, initialPos, wallRotation * poses[0].rotation);
        
        if (avatarInstance == null)
        {
            Debug.LogError("[HipsPlayback] Failed to instantiate avatar!");
            UpdateDebugText("Failed to create avatar!");
            return;
        }
        
        // Add skeleton visualization if needed
        if (skeletonPrefab)
        {
            skeletonInstance = Instantiate(skeletonPrefab, initialPos, wallRotation * poses[0].rotation, avatarInstance.transform);
            var skeleton = skeletonInstance.GetComponent<StickFigureSkeleton>();
            if (skeleton)
            {
                skeleton.enabled = true;
                Debug.Log("[HipsPlayback] Skeleton visualization enabled");
            }
        }
        
        startTime = Time.time;
        index = 0;
        UpdateDebugText($"Playback ready - {poses.Count} poses");
    }

    void Update()
    {
        if (!isWallPlaced || avatarInstance == null || poses == null || poses.Count == 0) return;
        
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
            
            // Interpolate position and rotation relative to the wall
            Vector3 pos = Vector3.Lerp(currentPose.position, nextPose.position, t);
            Quaternion rot = Quaternion.Slerp(currentPose.rotation, nextPose.rotation, t);
            
            // Transform to world space relative to wall position
            Vector3 worldPos = wallInstance.transform.TransformPoint(pos);
            Quaternion worldRot = wallInstance.transform.rotation * rot;
            
            // Update avatar position and rotation
            avatarInstance.transform.SetPositionAndRotation(worldPos, worldRot);
            
            if (Time.frameCount % 90 == 0)
            {
                UpdateDebugText($"Playing: {index}/{poses.Count-1}");
            }
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