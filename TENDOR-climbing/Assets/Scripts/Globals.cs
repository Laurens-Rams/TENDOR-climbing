using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[ExecuteInEditMode]
public class Globals : MonoBehaviour
{
    public static XROrigin XROrigin;
    public static ARTrackedImageManager TrackedImageManager;
    public static ARRaycastManager RaycastManager;
    public static GameObject UI;

    [SerializeField]
    private XROrigin xrOrigin;
    [SerializeField]
    private ARTrackedImageManager trackedImageManager;
    [SerializeField]
    private ARRaycastManager raycastManager;
    [SerializeField]
    private GameObject ui;

    void Awake()
    {
        XROrigin = xrOrigin;
        TrackedImageManager = trackedImageManager;
        RaycastManager = raycastManager;
        UI = ui;
    }
}
