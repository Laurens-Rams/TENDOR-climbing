using UnityEngine;

public class ViewSwitcher : MonoBehaviour
{
    public static bool isARMode { get; private set; } = true;

    [SerializeField] private GameObject arUI;
    [SerializeField] private GameObject recordUI;
    [SerializeField] private TrackingLogic trackingLogic;
    [SerializeField] private HipsRecorder hipsRecorder;
    [SerializeField] private HipsPlayback hipsPlayback;

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
        if (trackingLogic)
            trackingLogic.enabled = isARMode;
        if (hipsRecorder)
            hipsRecorder.enabled = !isARMode;
        if (hipsPlayback)
            hipsPlayback.enabled = isARMode;
        if (arUI)
            arUI.SetActive(isARMode);
        if (recordUI)
            recordUI.SetActive(!isARMode);
    }
}
