using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class DebugOverlay : MonoBehaviour
{
    [SerializeField] private GameObject skeletonGO;

    void Update()
    {
        if (Globals.BodyTracker?.Current == null) return;
        skeletonGO.SetActive(true);
        skeletonGO.transform.SetPositionAndRotation(
            Globals.BodyTracker.Current.transform.position,
            Globals.BodyTracker.Current.transform.rotation);
    }
}
