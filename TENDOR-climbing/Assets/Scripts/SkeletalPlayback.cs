using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.XR.ARFoundation;

public class SkeletalPlayback : MonoBehaviour
{
    [Header("Playback Settings")]
    [SerializeField] private bool loopPlayback = true;
    [SerializeField] private float playbackSpeed = 1f;
    
    private GameObject wallInstance;
    private GameObject skeletonVisualization;
    private StickFigureSkeleton stickFigure;
    private TMPro.TextMeshProUGUI debugText;
    
    private SkeletalRecording recording;
    private float playbackStartTime;
    private int currentFrameIndex;
    private bool isInitialized = false;
    private bool isPlaying = false;

    public void Initialize(GameObject wall, GameObject skeletonPrefab, TMPro.TextMeshProUGUI debugUI)
    {
        wallInstance = wall;
        debugText = debugUI;
        
        Load();
        
        if (recording != null && recording.frames.Count > 0 && wallInstance != null)
        {
            CreateSkeletonVisualization(skeletonPrefab);
            isInitialized = true;
            Debug.Log($"[SkeletalPlayback] Initialized with {recording.frames.Count} skeletal frames");
            UpdateDebugText($"Skeletal playback ready - {recording.frames.Count} frames");
            StartPlayback();
        }
        else if (recording == null || recording.frames.Count == 0)
        {
            Debug.LogWarning("[SkeletalPlayback] No skeletal recording found");
            UpdateDebugText("No skeletal recording - record first!");
        }
        else
        {
            Debug.LogError("[SkeletalPlayback] Failed to initialize skeletal playback");
            UpdateDebugText("Skeletal playback setup failed");
        }
    }

    void OnDisable()
    {
        StopPlayback();
        CleanupPlayback();
        isInitialized = false;
    }

    private void CleanupPlayback()
    {
        if (skeletonVisualization != null)
        {
            Destroy(skeletonVisualization);
            skeletonVisualization = null;
        }
        stickFigure = null;
    }

    private void Load()
    {
        string path = Path.Combine(Application.persistentDataPath, "skeletal_recording.json");
        Debug.Log($"[SkeletalPlayback] Loading skeletal data from: {path}");
        
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogError("[SkeletalPlayback] Skeletal recording file is empty!");
                    UpdateDebugText("Skeletal file is empty!");
                    return;
                }
                
                recording = JsonUtility.FromJson<SkeletalRecording>(json);
                if (recording != null && recording.frames != null && recording.frames.Count > 0)
                {
                    Debug.Log($"[SkeletalPlayback] Successfully loaded {recording.frames.Count} skeletal frames");
                }
                else
                {
                    Debug.LogError("[SkeletalPlayback] Failed to parse skeletal data");
                    UpdateDebugText("Invalid skeletal file!");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SkeletalPlayback] Error loading skeletal data: {e.Message}");
                UpdateDebugText("Error loading skeletal data!");
            }
        }
        else
        {
            Debug.LogWarning($"[SkeletalPlayback] No skeletal recording file found at {path}");
            UpdateDebugText("No skeletal recording found!");
        }
    }

    private void CreateSkeletonVisualization(GameObject skeletonPrefab)
    {
        if (wallInstance == null || recording.frames.Count == 0) return;

        // Get first frame position for initial placement
        var firstFrame = recording.frames[0];
        if (firstFrame.joints.Count == 0) return;

        // Find root joint (usually hips/pelvis - typically joint 0 or 1)
        JointData rootJoint = null;
        foreach (var joint in firstFrame.joints)
        {
            if (joint.jointType <= 1) // Root or pelvis joint
            {
                rootJoint = joint;
                break;
            }
        }

        if (rootJoint == null) return;

        // Transform to world space
        Vector3 worldPos = wallInstance.transform.TransformPoint(rootJoint.position);
        Quaternion worldRot = wallInstance.transform.rotation * rootJoint.rotation;

        // Create skeleton visualization
        if (skeletonPrefab != null)
        {
            skeletonVisualization = Instantiate(skeletonPrefab, worldPos, worldRot);
            stickFigure = skeletonVisualization.GetComponent<StickFigureSkeleton>();
            
            if (stickFigure != null)
            {
                stickFigure.enabled = true;
                Debug.Log("[SkeletalPlayback] Stick figure skeleton visualization created");
            }
        }
        else
        {
            // Create simple visualization if no prefab provided
            skeletonVisualization = new GameObject("SkeletalVisualization");
            skeletonVisualization.transform.position = worldPos;
            skeletonVisualization.transform.rotation = worldRot;
        }
    }

    public void StartPlayback()
    {
        if (!isInitialized || recording == null || recording.frames.Count == 0)
        {
            Debug.LogWarning("[SkeletalPlayback] Cannot start playback - not ready");
            return;
        }

        playbackStartTime = Time.time;
        currentFrameIndex = 0;
        isPlaying = true;
        
        Debug.Log("[SkeletalPlayback] Skeletal playback started");
        UpdateDebugText("Playing skeletal animation...");
    }

    public void StopPlayback()
    {
        isPlaying = false;
        Debug.Log("[SkeletalPlayback] Skeletal playback stopped");
    }

    void Update()
    {
        if (!isInitialized || !isPlaying || recording == null || recording.frames.Count == 0 || wallInstance == null)
            return;

        float elapsedTime = (Time.time - playbackStartTime) * playbackSpeed;
        
        // Find current frame
        while (currentFrameIndex + 1 < recording.frames.Count && 
               recording.frames[currentFrameIndex + 1].time <= elapsedTime)
        {
            currentFrameIndex++;
        }
        
        // Loop if enabled
        if (currentFrameIndex >= recording.frames.Count - 1)
        {
            if (loopPlayback)
            {
                playbackStartTime = Time.time;
                currentFrameIndex = 0;
                elapsedTime = 0;
            }
            else
            {
                StopPlayback();
                return;
            }
        }

        // Apply current frame
        ApplySkeletalFrame(currentFrameIndex, elapsedTime);
        
        // Update UI periodically
        if (Time.frameCount % 90 == 0)
        {
            UpdateDebugText($"Playing skeletal: {currentFrameIndex}/{recording.frames.Count-1}");
        }
    }

    private void ApplySkeletalFrame(int frameIndex, float elapsedTime)
    {
        if (frameIndex >= recording.frames.Count) return;

        var currentFrame = recording.frames[frameIndex];
        if (currentFrame.joints.Count == 0) return;

        // Update skeleton visualization if available
        if (stickFigure != null)
        {
            // Update stick figure with current joint positions
            UpdateStickFigure(currentFrame);
        }
        else if (skeletonVisualization != null)
        {
            // Update basic visualization position from root joint
            foreach (var joint in currentFrame.joints)
            {
                if (joint.jointType <= 1 && joint.isTracked) // Root joint
                {
                    Vector3 worldPos = wallInstance.transform.TransformPoint(joint.position);
                    Quaternion worldRot = wallInstance.transform.rotation * joint.rotation;
                    skeletonVisualization.transform.SetPositionAndRotation(worldPos, worldRot);
                    break;
                }
            }
        }
    }

    private void UpdateStickFigure(SkeletalFrame frame)
    {
        if (stickFigure == null) return;

        // Convert skeletal frame data to format expected by StickFigureSkeleton
        // This would need to be customized based on your StickFigureSkeleton implementation
        
        // Example: Update root position
        foreach (var joint in frame.joints)
        {
            if (joint.jointType <= 1 && joint.isTracked) // Root joint
            {
                Vector3 worldPos = wallInstance.transform.TransformPoint(joint.position);
                stickFigure.transform.position = worldPos;
                break;
            }
        }
        
        // Additional joint updates would go here based on StickFigureSkeleton API
    }

    public bool IsInitialized()
    {
        return isInitialized;
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }

    public bool HasSkeletalRecording()
    {
        return recording != null && recording.frames.Count > 0;
    }

    private void UpdateDebugText(string message)
    {
        if (debugText != null)
        {
            debugText.text = message;
        }
        Debug.Log($"[SkeletalPlayback] {message}");
    }
} 