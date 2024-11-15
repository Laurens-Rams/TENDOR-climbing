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
        //        break;
    }

    private bool TryActivateContent(ARTrackedImage image)
    {
        var content = image ? Array.Find(contents, content => content.name == image.referenceImage.name) : null;
        if (!content)
            return false;

        content.transform.parent = image.transform;
        content.transform.localPosition = new Vector3(0f, 25f, 0f);
        content.transform.localRotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));
        content.transform.localScale = Vector3.one;
        content.Show();
        return true;
    }
}
