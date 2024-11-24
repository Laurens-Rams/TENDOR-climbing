using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System;
using System.Collections.Generic;

public class TrackingLogic : MonoBehaviour
{
    private Content[] contents;

    void OnEnable()
    {
        contents = GetComponentsInChildren<Content>(true);
        Globals.TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        Globals.TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
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

        var ray = new Ray(Globals.XROrigin.Camera.transform.position, (trackedImage.transform.position - Globals.XROrigin.Camera.transform.position).normalized);
        var hits = new List<ARRaycastHit>();
        Globals.RaycastManager.Raycast(ray, hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes);
        if (hits.Count > 0)
        {
            foreach (var hit in hits)
            {
                var plane = hit.trackable as ARPlane;
                if (plane.alignment == UnityEngine.XR.ARSubsystems.PlaneAlignment.Vertical)
                {
                    content.transform.parent = plane.transform;
                    break;
                }
            }
        }

        content.Show(trackedImage);

        return true;
    }
}
