using UnityEngine;
using UnityEngine.XR.ARFoundation;

[ExecuteInEditMode]
public class Globals : MonoBehaviour
{
    public static ARTrackedImageManager ARTrackedImageManager;
    public static GameObject UI;

    [SerializeField]
    private ARTrackedImageManager arTrackedImageManager;
    [SerializeField]
    private GameObject ui;

    void Awake()
    {
        ARTrackedImageManager = arTrackedImageManager;
        UI = ui;
    }
}
