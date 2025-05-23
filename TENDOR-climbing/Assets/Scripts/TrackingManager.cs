using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class TrackingManager : MonoBehaviour
{
    [Header("Wall Settings")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private float wallScaleFactor = 1.0f;
    
    [Header("Recording Settings")]
    [SerializeField] private GameObject recordingAvatarPrefab;
    [SerializeField] private bool useSkeletalRecording = true;
    
    [Header("Playback Settings")]
    [SerializeField] private GameObject playbackAvatarPrefab;
    [SerializeField] private GameObject skeletonPrefab;
    
    [Header("UI")]
    [SerializeField] private TMPro.TextMeshProUGUI debugText;
    
    // Components
    private GameObject wallInstance;
    private bool isWallPlaced = false;
    private bool isInitialized = false;
    
    // Recording (basic)
    private HipsRecorder hipsRecorder;
    
    // Recording (advanced skeletal)
    private SkeletalRecorder skeletalRecorder;
    
    // Playback (basic)
    private HipsPlayback hipsPlayback;
    
    // Playback (advanced skeletal)
    private SkeletalPlayback skeletalPlayback;

    void OnEnable()
    {
        // Get references to recorder and playback components
        hipsRecorder = FindObjectOfType<HipsRecorder>();
        hipsPlayback = FindObjectOfType<HipsPlayback>();
        skeletalRecorder = FindObjectOfType<SkeletalRecorder>();
        skeletalPlayback = FindObjectOfType<SkeletalPlayback>();
        
        // Validate required components based on recording mode
        if (useSkeletalRecording)
        {
            if (skeletalRecorder == null)
            {
                Debug.LogError("[TrackingManager] SkeletalRecorder not found in scene!");
                UpdateDebugText("Error: SkeletalRecorder missing!");
                return;
            }
            
            if (skeletalPlayback == null)
            {
                Debug.LogError("[TrackingManager] SkeletalPlayback not found in scene!");
                UpdateDebugText("Error: SkeletalPlayback missing!");
                return;
            }
        }
        else
        {
            if (hipsRecorder == null)
            {
                Debug.LogError("[TrackingManager] HipsRecorder not found in scene!");
                UpdateDebugText("Error: HipsRecorder missing!");
                return;
            }
            
            if (hipsPlayback == null)
            {
                Debug.LogError("[TrackingManager] HipsPlayback not found in scene!");
                UpdateDebugText("Error: HipsPlayback missing!");
                return;
            }
        }
        
        // Subscribe to image tracking events
        if (Globals.TrackedImageManager != null)
        {
            Globals.TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
            Debug.Log("[TrackingManager] Successfully subscribed to image tracking events");
        }
        else
        {
            Debug.LogError("[TrackingManager] Globals.TrackedImageManager is null! Check AR setup.");
            UpdateDebugText("Error: AR tracking not available!");
            return;
        }
        
        Debug.Log($"[TrackingManager] OnEnable - AR Mode: {ViewSwitcher.isARMode}, Skeletal: {useSkeletalRecording}");
        UpdateDebugText($"Ready - Mode: {(ViewSwitcher.isARMode ? "AR Playback" : "Recording")} ({(useSkeletalRecording ? "Skeletal" : "Basic")})");
    }

    void OnDisable()
    {
        if (Globals.TrackedImageManager != null)
        {
            Globals.TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        if (isInitialized) return;

        // Handle newly detected images
        foreach (var trackedImage in eventArgs.added)
        {
            HandleImageDetected(trackedImage);
        }

        // Handle updated images
        foreach (var trackedImage in eventArgs.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                HandleImageDetected(trackedImage);
            }
        }
    }

    private void HandleImageDetected(ARTrackedImage trackedImage)
    {
        if (isInitialized) return;

        Debug.Log($"[TrackingManager] Image target detected: {trackedImage.referenceImage.name}");
        UpdateDebugText($"Image target found: {trackedImage.referenceImage.name}");
        
        // Place the wall
        if (PlaceWall(trackedImage))
        {
            // Initialize the appropriate mode
            if (ViewSwitcher.isARMode)
            {
                InitializePlaybackMode();
            }
            else
            {
                InitializeRecordingMode();
            }
            
            isInitialized = true;
        }
    }

    private bool PlaceWall(ARTrackedImage trackedImage)
    {
        if (wallPrefab == null)
        {
            Debug.LogError("[TrackingManager] Wall prefab not assigned!");
            UpdateDebugText("Error: No wall prefab assigned!");
            return false;
        }
        
        if (wallInstance != null)
        {
            Debug.LogWarning("[TrackingManager] Wall already placed");
            return true;
        }

        try
        {
            // Create wall instance at the tracked image position
            wallInstance = Instantiate(wallPrefab, trackedImage.transform);
            wallInstance.transform.localScale = new Vector3(
                trackedImage.size.x * wallScaleFactor,
                trackedImage.size.y * wallScaleFactor,
                1f
            );

            isWallPlaced = true;
            Debug.Log("[TrackingManager] Wall placed successfully");
            UpdateDebugText("Wall placed - Initializing...");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TrackingManager] Failed to place wall: {e.Message}");
            UpdateDebugText("Error placing wall!");
            return false;
        }
    }

    private void InitializeRecordingMode()
    {
        if (!isWallPlaced)
        {
            Debug.LogError("[TrackingManager] Cannot initialize recording - wall not placed");
            UpdateDebugText("Error: Wall not placed!");
            return;
        }

        if (useSkeletalRecording)
        {
            InitializeSkeletalRecording();
        }
        else
        {
            InitializeBasicRecording();
        }
    }

    private void InitializeBasicRecording()
    {
        if (recordingAvatarPrefab == null)
        {
            Debug.LogError("[TrackingManager] Recording avatar prefab not assigned!");
            UpdateDebugText("Error: No recording avatar assigned!");
            return;
        }
        
        if (hipsRecorder == null)
        {
            Debug.LogError("[TrackingManager] HipsRecorder is null!");
            UpdateDebugText("Error: HipsRecorder missing!");
            return;
        }

        // Enable basic recorder and set up recording
        hipsRecorder.enabled = true;
        hipsRecorder.Initialize(wallInstance, recordingAvatarPrefab, debugText);
        
        // Disable skeletal components if present
        if (skeletalRecorder != null) skeletalRecorder.enabled = false;
        
        Debug.Log("[TrackingManager] Basic recording mode initialized successfully");
    }

    private void InitializeSkeletalRecording()
    {
        if (skeletalRecorder == null)
        {
            Debug.LogError("[TrackingManager] SkeletalRecorder is null!");
            UpdateDebugText("Error: SkeletalRecorder missing!");
            return;
        }

        // Get the AR Human Body Manager
        var bodyManager = FindObjectOfType<ARHumanBodyManager>();
        if (bodyManager == null)
        {
            Debug.LogError("[TrackingManager] ARHumanBodyManager not found! Falling back to basic recording.");
            UpdateDebugText("No body tracking - using basic mode");
            InitializeBasicRecording();
            return;
        }

        // Enable skeletal recorder
        skeletalRecorder.enabled = true;
        skeletalRecorder.Initialize(wallInstance, bodyManager, debugText);
        
        // Disable basic components if present
        if (hipsRecorder != null) hipsRecorder.enabled = false;
        
        Debug.Log("[TrackingManager] Skeletal recording mode initialized successfully");
    }

    private void InitializePlaybackMode()
    {
        if (!isWallPlaced)
        {
            Debug.LogError("[TrackingManager] Cannot initialize playback - wall not placed");
            UpdateDebugText("Error: Wall not placed!");
            return;
        }

        if (useSkeletalRecording)
        {
            InitializeSkeletalPlayback();
        }
        else
        {
            InitializeBasicPlayback();
        }
    }

    private void InitializeBasicPlayback()
    {
        if (playbackAvatarPrefab == null)
        {
            Debug.LogError("[TrackingManager] Playback avatar prefab not assigned!");
            UpdateDebugText("Error: No playback avatar assigned!");
            return;
        }
        
        if (hipsPlayback == null)
        {
            Debug.LogError("[TrackingManager] HipsPlayback is null!");
            UpdateDebugText("Error: HipsPlayback missing!");
            return;
        }

        // Enable basic playback and set up playback
        hipsPlayback.enabled = true;
        hipsPlayback.Initialize(wallInstance, playbackAvatarPrefab, skeletonPrefab, debugText);
        
        // Disable skeletal components if present
        if (skeletalPlayback != null) skeletalPlayback.enabled = false;
        
        Debug.Log("[TrackingManager] Basic playback mode initialized successfully");
    }

    private void InitializeSkeletalPlayback()
    {
        if (skeletalPlayback == null)
        {
            Debug.LogError("[TrackingManager] SkeletalPlayback is null!");
            UpdateDebugText("Error: SkeletalPlayback missing!");
            return;
        }

        // Enable skeletal playback
        skeletalPlayback.enabled = true;
        skeletalPlayback.Initialize(wallInstance, skeletonPrefab, debugText);
        
        // Disable basic components if present
        if (hipsPlayback != null) hipsPlayback.enabled = false;
        
        Debug.Log("[TrackingManager] Skeletal playback mode initialized successfully");
    }

    public void SwitchMode()
    {
        // Reset initialization when switching modes
        isInitialized = false;
        isWallPlaced = false;
        
        if (wallInstance != null)
        {
            Destroy(wallInstance);
            wallInstance = null;
        }
        
        // Disable all recording/playback modes
        if (hipsRecorder != null) hipsRecorder.enabled = false;
        if (hipsPlayback != null) hipsPlayback.enabled = false;
        if (skeletalRecorder != null) skeletalRecorder.enabled = false;
        if (skeletalPlayback != null) skeletalPlayback.enabled = false;
        
        Debug.Log($"[TrackingManager] Mode switched to: {(ViewSwitcher.isARMode ? "AR Playback" : "Recording")} ({(useSkeletalRecording ? "Skeletal" : "Basic")})");
        UpdateDebugText($"Switched to {(ViewSwitcher.isARMode ? "AR Playback" : "Recording")} - Scan image target...");
    }
    
    // Public status methods for UI
    public bool IsInitialized()
    {
        return isInitialized;
    }
    
    public bool IsWallPlaced()
    {
        return isWallPlaced;
    }
    
    public bool IsRecordingReady()
    {
        if (!isInitialized || ViewSwitcher.isARMode) return false;
        
        if (useSkeletalRecording)
        {
            return skeletalRecorder != null && skeletalRecorder.IsInitialized();
        }
        else
        {
            return hipsRecorder != null && hipsRecorder.IsInitialized();
        }
    }
    
    public bool IsPlaybackReady()
    {
        if (!isInitialized || !ViewSwitcher.isARMode) return false;
        
        if (useSkeletalRecording)
        {
            return skeletalPlayback != null && skeletalPlayback.IsInitialized();
        }
        else
        {
            return hipsPlayback != null && hipsPlayback.IsInitialized();
        }
    }

    public void StartRecording()
    {
        if (!IsRecordingReady()) return;

        if (useSkeletalRecording && skeletalRecorder != null)
        {
            skeletalRecorder.StartRecording();
        }
        else if (hipsRecorder != null)
        {
            hipsRecorder.StartRecording();
        }
    }

    public void StopRecording()
    {
        if (useSkeletalRecording && skeletalRecorder != null)
        {
            skeletalRecorder.StopRecording();
        }
        else if (hipsRecorder != null)
        {
            hipsRecorder.StopRecording();
        }
    }

    public bool IsCurrentlyRecording()
    {
        if (useSkeletalRecording && skeletalRecorder != null)
        {
            return skeletalRecorder.IsRecording();
        }
        else if (hipsRecorder != null)
        {
            return hipsRecorder.IsRecording();
        }
        return false;
    }

    private void UpdateDebugText(string message)
    {
        if (debugText != null)
        {
            debugText.text = message;
        }
        Debug.Log($"[TrackingManager] {message}");
    }
} 