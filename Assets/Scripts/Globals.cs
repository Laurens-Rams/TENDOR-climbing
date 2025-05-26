using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[ExecuteInEditMode]
public class Globals : MonoBehaviour
{
    public static XROrigin XROrigin;
    public static ARTrackedImageManager TrackedImageManager;
    public static ARRaycastManager RaycastManager;
    public static ARCameraManager CameraManager;
    public static GameObject UI;

    [SerializeField]
    private XROrigin xrOrigin;
    [SerializeField]
    private ARTrackedImageManager trackedImageManager;
    [SerializeField]
    private ARRaycastManager raycastManager;
    [SerializeField]
    private ARCameraManager cameraManager;
    [SerializeField]
    private GameObject ui;

    void Awake()
    {
        XROrigin = xrOrigin;
        TrackedImageManager = trackedImageManager;
        RaycastManager = raycastManager;
        CameraManager = cameraManager;

        UI = ui;
    }
}
