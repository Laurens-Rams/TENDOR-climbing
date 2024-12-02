using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using System.Collections.Generic;

public class TrackingLogic : MonoBehaviour
{
    private Content[] contents;
    private bool contentAttachedToPlane = false;

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
        if (contentAttachedToPlane)
            return; // Stop further image target searching if content is already attached to a plane.

        foreach (var trackedImage in eventArgs.added)
        {
            if (TryActivateContent(trackedImage))
                break;
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking && TryActivateContent(trackedImage))
                break;
        }
    }

    private bool TryActivateContent(ARTrackedImage trackedImage)
    {
        if (contentAttachedToPlane || trackedImage == null)
            return false;

        var content = Array.Find(contents, content => content.name == trackedImage.referenceImage.name);
        if (content == null)
            return false;

        // Initially attach content to the tracked image
        content.transform.parent = trackedImage.transform;
        content.transform.localPosition = Vector3.zero;
        content.transform.localRotation = Quaternion.identity;

        // Perform raycast to find planes
        var ray = new Ray(Globals.XROrigin.Camera.transform.position, (trackedImage.transform.position - Globals.XROrigin.Camera.transform.position).normalized);
        var hits = new List<ARRaycastHit>();
        Globals.RaycastManager.Raycast(ray, hits, TrackableType.Planes);

        if (hits.Count > 0)
        {
            foreach (var hit in hits)
            {
                var plane = hit.trackable as ARPlane;
                if (plane != null && plane.alignment == PlaneAlignment.Vertical) // Check for vertical planes
                {
                    Vector3 originalWorldScale = content.transform.lossyScale;

                    content.transform.parent = plane.transform;
                    content.transform.position = hit.pose.position;
                    content.transform.rotation = hit.pose.rotation;
                    content.transform.localScale = originalWorldScale * 0.26f; 

                    contentAttachedToPlane = true; // Stop further checks
                    break;
                }
            }
        }

        content.Show(trackedImage);

        return contentAttachedToPlane;
    }
}