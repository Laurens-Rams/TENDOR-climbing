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

    private bool TryActivateContent(ARTrackedImage image)
    {
        var content = image ? Array.Find(contents, content => content.name == image.referenceImage.name) : null;
        if (!content)
            return false;

        content.transform.parent = image.transform;
        content.transform.localPosition = Vector3.zero;
        content.transform.localRotation = Quaternion.identity;
        //content.transform.localScale = new Vector3(1f / image.referenceImage.width, 1f, 1f / image.referenceImage.height);
        content.Show();
        return true;
    }
}
