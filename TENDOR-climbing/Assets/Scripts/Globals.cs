using UnityEngine;
using UnityEngine.XR.ARFoundation;

[ExecuteInEditMode]
public class Globals : MonoBehaviour
{
    public static ARTrackedImageManager ARTrackedImageManager;

    [SerializeField]
    private ARTrackedImageManager arTrackedImageManager;

    void Awake()
    {
        ARTrackedImageManager = arTrackedImageManager;
    }
}
