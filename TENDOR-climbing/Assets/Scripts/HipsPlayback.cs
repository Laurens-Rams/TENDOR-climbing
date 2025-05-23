using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI; // For debug text

public class HipsPlayback : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager imageManager;
    [SerializeField] private GameObject avatarPrefab;
    [SerializeField] private GameObject skeletonPrefab;
    [SerializeField] private TMPro.TextMeshProUGUI debugText; // For displaying debug info

    private List<SerializablePose> poses;
    private Transform imageTransform;
    private GameObject avatarInstance;
    private GameObject skeletonInstance;
    private float startTime;
    private int index;

    void OnEnable()
    {
        if (!imageManager)
            imageManager = FindFirstObjectByType<ARTrackedImageManager>();
        
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
            Debug.Log($"[HipsPlayback] Loaded {poses.Count} poses from JSON");
            UpdateDebugText($"Loaded {poses.Count} poses");
        }
        else
        {
            Debug.LogWarning("[HipsPlayback] No poses loaded from JSON!");
            UpdateDebugText("No poses loaded!");
        }
        
        if (avatarPrefab == null)
        {
            Debug.LogError("[HipsPlayback] Avatar prefab is not assigned!");
            UpdateDebugText("ERROR: No avatar prefab!");
        }
        
        imageManager.trackedImagesChanged += OnImagesChanged;
        Debug.Log("[HipsPlayback] Waiting for image target...");
    }

    void OnDisable()
    {
        Debug.Log("[HipsPlayback] OnDisable");
        imageManager.trackedImagesChanged -= OnImagesChanged;
        if (avatarInstance)
            Destroy(avatarInstance);
        avatarInstance = null;
        if (skeletonInstance)
            Destroy(skeletonInstance);
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
                Debug.Log($"[HipsPlayback] JSON loaded, length: {json.Length}");
                
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogError("[HipsPlayback] JSON file is empty!");
                    UpdateDebugText("JSON file is empty!");
                    return;
                }
                
                var data = JsonUtility.FromJson<SerializablePoseCollection>(json);
                
                if (data == null)
                {
                    Debug.LogError("[HipsPlayback] Failed to parse JSON data!");
                    UpdateDebugText("Failed to parse JSON!");
                    return;
                }
                
                poses = data.poses;
                
                if (poses == null || poses.Count == 0)
                {
                    Debug.LogWarning("[HipsPlayback] No poses in the loaded data!");
                    UpdateDebugText("No poses in data!");
                }
                else
                {
                    Debug.Log($"[HipsPlayback] Successfully loaded {poses.Count} poses");
                    // Print first pose info for debugging
                    var firstPose = poses[0];
                    Debug.Log($"[HipsPlayback] First pose - pos: {firstPose.position}, rot: {firstPose.rotation.eulerAngles}, time: {firstPose.time}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[HipsPlayback] Error loading JSON: {e.Message}");
                UpdateDebugText($"Error: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"[HipsPlayback] File not found: {path}");
            UpdateDebugText("JSON file not found!");
            
            // Try the temp file as backup
            string tempPath = Path.Combine(Application.persistentDataPath, "hips_temp.json");
            if (File.Exists(tempPath))
            {
                Debug.Log($"[HipsPlayback] Trying temp file: {tempPath}");
                try
                {
                    string json = File.ReadAllText(tempPath);
                    var data = JsonUtility.FromJson<SerializablePoseCollection>(json);
                    poses = data.poses;
                    Debug.Log($"[HipsPlayback] Loaded {poses.Count} poses from temp file");
                    UpdateDebugText($"Loaded {poses.Count} poses from temp");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[HipsPlayback] Error loading temp JSON: {e.Message}");
                }
            }
        }
    }

    private void OnImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        Debug.Log($"[HipsPlayback] Images changed: added={args.added.Count}, updated={args.updated.Count}");
        
        foreach (var img in args.added)
        {
            Debug.Log($"[HipsPlayback] Image added: {img.referenceImage.name}, state: {img.trackingState}");
            TrySpawn(img);
        }
        
        foreach (var img in args.updated)
        {
            Debug.Log($"[HipsPlayback] Image updated: {img.referenceImage.name}, state: {img.trackingState}");
            TrySpawn(img);
        }
    }

    private void TrySpawn(ARTrackedImage img)
    {
        Debug.Log($"[HipsPlayback] TrySpawn: {img.referenceImage.name}, tracking: {img.trackingState}");
        
        // Check for any image target with different possible names (matching recorder)
        if (imageTransform == null && 
            (img.referenceImage.name == "Wall 1" ||
             img.referenceImage.name == "test-target" ||
             img.referenceImage.name == "Wall_1" ||
             img.referenceImage.name == "Target" ||
             img.referenceImage.name.Contains("Wall") ||
             img.referenceImage.name.Contains("Target")) && 
            img.trackingState == TrackingState.Tracking)
        {
            Debug.Log($"[HipsPlayback] Target found: {img.referenceImage.name}");
            imageTransform = img.transform;
            
            if (poses != null && poses.Count > 0 && avatarPrefab)
            {
                Vector3 worldPos = imageTransform.TransformPoint(poses[0].position);
                Quaternion worldRot = imageTransform.rotation * poses[0].rotation;
                
                Debug.Log($"[HipsPlayback] Spawning avatar at pos: {worldPos}, rot: {worldRot.eulerAngles}");
                avatarInstance = Instantiate(avatarPrefab, worldPos, worldRot);
                
                if (avatarInstance == null)
                {
                    Debug.LogError("[HipsPlayback] Failed to instantiate avatar!");
                    UpdateDebugText("Failed to create avatar!");
                    return;
                }
                
                Debug.Log("[HipsPlayback] Avatar created successfully");
                
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
                UpdateDebugText($"Playback started - {poses.Count} poses");
            }
            else
            {
                if (poses == null || poses.Count == 0)
                    Debug.LogError("[HipsPlayback] No poses available for playback!");
                if (avatarPrefab == null)
                    Debug.LogError("[HipsPlayback] Avatar prefab is not assigned!");
                
                UpdateDebugText("Can't spawn - missing data or prefab");
            }
        }
    }

    void Update()
    {
        if (avatarInstance == null || poses == null || poses.Count == 0)
        {
            // Only log this once per second to avoid spam
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log("[HipsPlayback] Update - waiting for avatar/poses");
                if (avatarInstance == null) Debug.Log("[HipsPlayback] Avatar instance is null");
                if (poses == null) Debug.Log("[HipsPlayback] Poses list is null");
                else if (poses.Count == 0) Debug.Log("[HipsPlayback] Poses list is empty");
            }
            return;
        }
        
        float elapsed = Time.time - startTime;
        
        // Update the current position in the animation
        while (index + 1 < poses.Count && poses[index + 1].time <= elapsed)
            index++;
        
        // Log playback progress periodically
        if (Time.frameCount % 90 == 0)
        {
            Debug.Log($"[HipsPlayback] Playing pose {index}/{poses.Count-1} at time {elapsed:F2}s");
            UpdateDebugText($"Playing: {index}/{poses.Count-1}");
        }
        
        // Check if we need to loop the animation
        if (index >= poses.Count - 1)
        {
            Debug.Log("[HipsPlayback] End of animation, looping...");
            startTime = Time.time;
            index = 0;
            return;
        }
        
        if (index + 1 < poses.Count)
        {
            var a = poses[index];
            var b = poses[index + 1];
            float t = Mathf.InverseLerp(a.time, b.time, elapsed);
            Vector3 pos = Vector3.Lerp(a.position, b.position, t);
            Quaternion rot = Quaternion.Slerp(a.rotation, b.rotation, t);
            
            // Convert from local to world space
            Vector3 worldPos = imageTransform.TransformPoint(pos);
            Quaternion worldRot = imageTransform.rotation * rot;
            
            // Update avatar position and rotation
            avatarInstance.transform.SetPositionAndRotation(worldPos, worldRot);
        }
    }
    
    private void UpdateDebugText(string message)
    {
        if (debugText != null)
        {
            debugText.text = message;
        }
        else
        {
            Debug.LogWarning("[HipsPlayback] Debug text UI is not assigned");
        }
    }
}
