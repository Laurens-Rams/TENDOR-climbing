using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System;

public class TrackingLogic : MonoBehaviour
{
    private Content[] contents;

    void OnEnable()
    {
        contents = GetComponentsInChildren<Content>(true);
        Globals.ARTrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        Globals.ARTrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
            if (TryActivateContent(trackedImage))
                break;

        //foreach (var trackedImage in eventArgs.updated)
        //    if (trackedImage.trackingState == TrackingState.Tracking && ...)
    }

    private bool TryActivateContent(ARTrackedImage trackedImage)
    {
        var content = trackedImage ? Array.Find(contents, content => content.name == trackedImage.referenceImage.name) : null;
        if (!content)
            return false;

        content.transform.parent = trackedImage.transform;
        content.transform.localPosition = Vector3.zero;
        content.transform.localRotation = Quaternion.identity;
        content.Show(trackedImage);
        return true;
    }
}
