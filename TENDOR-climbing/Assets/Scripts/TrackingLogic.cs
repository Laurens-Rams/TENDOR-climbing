using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;

public class TrackingLogic : MonoBehaviour
{
    private TargetContent[] contents;

    void OnEnable()
    {
        contents = GetComponentsInChildren<TargetContent>(true);
        Globals.ARTrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        Globals.ARTrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
            if (TryActivateContent(trackedImage.referenceImage.name))
                break;

        foreach (var trackedImage in eventArgs.updated)
            if (trackedImage.trackingState == TrackingState.Tracking && TryActivateContent(trackedImage.referenceImage.name))
                break;
    }

    private bool TryActivateContent(string imageName)
    {
        var content = Array.Find(contents, content => content.name == imageName);
        if (!content)
            return false;

        // Trigger content detected action
        return true;
    }
}
