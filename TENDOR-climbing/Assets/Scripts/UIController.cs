using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button recordButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button switchToARButton;
    [SerializeField] private Button switchToRecordButton;
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    
    [Header("Component References")]
    [SerializeField] private TrackingManager trackingManager;
    [SerializeField] private ViewSwitcher viewSwitcher;
    
    void Start()
    {
        if (trackingManager == null)
            trackingManager = FindObjectOfType<TrackingManager>();
        if (viewSwitcher == null)
            viewSwitcher = FindObjectOfType<ViewSwitcher>();
        
        SetupButtons();
        UpdateButtonStates();
    }
    
    void Update()
    {
        UpdateButtonStates();
    }
    
    private void SetupButtons()
    {
        if (recordButton != null)
            recordButton.onClick.AddListener(StartRecording);
        if (stopButton != null)
            stopButton.onClick.AddListener(StopRecording);
        if (switchToARButton != null)
            switchToARButton.onClick.AddListener(() => SwitchToMode(true));
        if (switchToRecordButton != null)
            switchToRecordButton.onClick.AddListener(() => SwitchToMode(false));
    }
    
    private void UpdateButtonStates()
    {
        bool isRecordingMode = !ViewSwitcher.isARMode;
        bool isRecordingReady = trackingManager != null && trackingManager.IsRecordingReady();
        bool isCurrentlyRecording = trackingManager != null && trackingManager.IsCurrentlyRecording();
        
        // Record button - only enabled if in recording mode and system is ready
        if (recordButton != null)
        {
            recordButton.interactable = isRecordingMode && isRecordingReady && !isCurrentlyRecording;
        }
        
        // Stop button - only enabled if currently recording
        if (stopButton != null)
        {
            stopButton.interactable = isCurrentlyRecording;
        }
        
        // Mode switch buttons
        if (switchToARButton != null)
        {
            switchToARButton.interactable = !ViewSwitcher.isARMode;
        }
        
        if (switchToRecordButton != null)
        {
            switchToRecordButton.interactable = ViewSwitcher.isARMode;
        }
    }
    
    public void StartRecording()
    {
        if (trackingManager == null || !trackingManager.IsRecordingReady())
        {
            ShowStatus("Not ready to record - scan image target first!");
            return;
        }
        
        Debug.Log("[UIController] Starting recording...");
        trackingManager.StartRecording();
    }
    
    public void StopRecording()
    {
        if (trackingManager == null)
        {
            ShowStatus("Error: TrackingManager not found!");
            return;
        }
        
        Debug.Log("[UIController] Stopping recording...");
        trackingManager.StopRecording();
    }
    
    public void SwitchToMode(bool arMode)
    {
        if (viewSwitcher == null)
        {
            ShowStatus("Error: ViewSwitcher not found!");
            return;
        }
        
        Debug.Log($"[UIController] Switching to {(arMode ? "AR Playback" : "Recording")} mode");
        viewSwitcher.SetARMode(arMode);
    }
    
    private void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[UIController] {message}");
    }
    
    // Public methods for button events (fallback)
    public void OnRecordButtonPressed()
    {
        StartRecording();
    }
    
    public void OnStopButtonPressed()
    {
        StopRecording();
    }
    
    public void OnSwitchToARButtonPressed()
    {
        SwitchToMode(true);
    }
    
    public void OnSwitchToRecordButtonPressed()
    {
        SwitchToMode(false);
    }
} 