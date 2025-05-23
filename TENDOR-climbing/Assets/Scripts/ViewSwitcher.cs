using UnityEngine;

public class ViewSwitcher : MonoBehaviour
{
    public static bool isARMode { get; private set; } = true;

    [SerializeField] private GameObject arUI;
    [SerializeField] private GameObject recordUI;
    [SerializeField] private TrackingManager trackingManager;

    void Awake()
    {
        ApplyMode();
    }

    public void SetARMode(bool value)
    {
        isARMode = value;
        ApplyMode();
    }

    private void ApplyMode()
    {
        if (trackingManager != null)
        {
            trackingManager.enabled = true;
            trackingManager.SwitchMode();
        }
        else
        {
            Debug.LogWarning("[ViewSwitcher] TrackingManager not assigned! Please assign it in the inspector.");
        }
        
        if (arUI)
            arUI.SetActive(isARMode);
        if (recordUI)
            recordUI.SetActive(!isARMode);
            
        Debug.Log($"[ViewSwitcher] Mode set to: {(isARMode ? "AR Playback" : "Recording")}");
    }
}
