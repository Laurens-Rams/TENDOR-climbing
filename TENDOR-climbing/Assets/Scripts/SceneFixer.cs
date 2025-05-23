using UnityEngine;

public class SceneFixer : MonoBehaviour
{
    [Header("Fix Scene Setup")]
    [SerializeField] private bool autoFixOnStart = true;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            FixSceneReferences();
        }
    }
    
    public void FixSceneReferences()
    {
        Debug.Log("[SceneFixer] Starting scene reference fix...");
        
        // Find and fix ViewSwitcher
        FixViewSwitcher();
        
        // Add missing components if needed
        AddMissingComponents();
        
        // Fix UI button references
        FixUIButtons();
        
        Debug.Log("[SceneFixer] Scene reference fix completed!");
    }
    
    private void FixViewSwitcher()
    {
        var viewSwitcher = FindObjectOfType<ViewSwitcher>();
        if (viewSwitcher == null)
        {
            Debug.LogError("[SceneFixer] ViewSwitcher not found!");
            return;
        }
        
        // Find the TrackingManager (it might be on the old "Image Tracking" GameObject)
        var trackingManager = FindObjectOfType<TrackingManager>();
        if (trackingManager == null)
        {
            // Try to add TrackingManager to the Image Tracking GameObject
            var imageTrackingGO = GameObject.Find("Image Tracking");
            if (imageTrackingGO != null)
            {
                trackingManager = imageTrackingGO.AddComponent<TrackingManager>();
                Debug.Log("[SceneFixer] Added TrackingManager to Image Tracking GameObject");
            }
            else
            {
                // Create a new GameObject for TrackingManager
                var trackingGO = new GameObject("TrackingManager");
                trackingGO.transform.SetParent(viewSwitcher.transform.parent);
                trackingManager = trackingGO.AddComponent<TrackingManager>();
                Debug.Log("[SceneFixer] Created new TrackingManager GameObject");
            }
        }
        
        // Use reflection to set the trackingManager field if ViewSwitcher is properly configured
        var viewSwitcherType = viewSwitcher.GetType();
        var trackingManagerField = viewSwitcherType.GetField("trackingManager", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (trackingManagerField != null)
        {
            trackingManagerField.SetValue(viewSwitcher, trackingManager);
            Debug.Log("[SceneFixer] Fixed ViewSwitcher.trackingManager reference");
        }
        
        Debug.Log("[SceneFixer] ViewSwitcher configuration fixed");
    }
    
    private void AddMissingComponents()
    {
        // Make sure we have all required components
        EnsureComponent<HipsRecorder>("HipsRecorder");
        EnsureComponent<HipsPlayback>("HipsPlayback");
        EnsureComponent<SkeletalRecorder>("SkeletalRecorder");
        EnsureComponent<SkeletalPlayback>("SkeletalPlayback");
        EnsureComponent<UIController>("UIController");
    }
    
    private void EnsureComponent<T>(string gameObjectName) where T : MonoBehaviour
    {
        var existing = FindObjectOfType<T>();
        if (existing == null)
        {
            var go = GameObject.Find(gameObjectName);
            if (go == null)
            {
                go = new GameObject(gameObjectName);
                go.transform.SetParent(this.transform.parent);
            }
            
            var component = go.AddComponent<T>();
            Debug.Log($"[SceneFixer] Added missing {typeof(T).Name} component");
        }
    }
    
    private void FixUIButtons()
    {
        // Find UI controller and set up button references
        var uiController = FindObjectOfType<UIController>();
        if (uiController == null)
        {
            Debug.LogWarning("[SceneFixer] UIController not found - buttons may not work properly");
            return;
        }
        
        // Find buttons and set up event handlers using UIController methods
        SetupButton("RECORD", "OnRecordButtonPressed", uiController);
        SetupButton("STOP", "OnStopButtonPressed", uiController);
        SetupButton("SETARMODE", "OnSwitchToARButtonPressed", uiController);
        SetupButton("SETRECORDMODE", "OnSwitchToRecordButtonPressed", uiController);
        
        Debug.Log("[SceneFixer] UI buttons configured");
    }
    
    private void SetupButton(string buttonName, string methodName, UIController uiController)
    {
        var buttonGO = GameObject.Find(buttonName);
        if (buttonGO == null)
        {
            Debug.LogWarning($"[SceneFixer] Button '{buttonName}' not found");
            return;
        }
        
        var button = buttonGO.GetComponent<UnityEngine.UI.Button>();
        if (button == null)
        {
            Debug.LogWarning($"[SceneFixer] Button component not found on '{buttonName}'");
            return;
        }
        
        // Clear existing listeners and add our method
        button.onClick.RemoveAllListeners();
        
        // Use reflection to get the method and add it as a listener
        var method = uiController.GetType().GetMethod(methodName);
        if (method != null)
        {
            button.onClick.AddListener(() => method.Invoke(uiController, null));
            Debug.Log($"[SceneFixer] Fixed button '{buttonName}' -> {methodName}");
        }
        else
        {
            Debug.LogWarning($"[SceneFixer] Method '{methodName}' not found on UIController");
        }
    }
    
    [ContextMenu("Fix Scene References")]
    public void ManualFix()
    {
        FixSceneReferences();
    }
} 