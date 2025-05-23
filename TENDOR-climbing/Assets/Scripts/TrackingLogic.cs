using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using System.Collections.Generic;

public class TrackingLogic : MonoBehaviour
{
    private Content[] contents;
    private bool contentAttachedToPlane = false;
    private bool isRecordingMode = false;

    [SerializeField]
    private float wallScaleFactor = 1.0f; // Adjustable scale factor for wall projection

    void OnEnable()
    {
        if (!ViewSwitcher.isARMode)
        {
            isRecordingMode = true;
            // Still enable tracking for positioning, but hide visual elements
            foreach (var content in GetComponentsInChildren<Content>(true))
            {
                if (content is DeepMotionContent dmc)
                {
                    dmc.SetImageTargetVisibility(false);
                }
            }
        }

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
            return;

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

        if (isRecordingMode)
        {
            // In recording mode, just track position without visual overlay
            content.transform.parent = trackedImage.transform;
            content.transform.localPosition = Vector3.zero;
            content.transform.localRotation = Quaternion.identity;
            return true;
        }

        // AR Mode - align with wall
        var ray = new Ray(trackedImage.transform.position, trackedImage.transform.forward);
        var hits = new List<ARRaycastHit>();
        Globals.RaycastManager.Raycast(ray, hits, TrackableType.Planes);

        if (hits.Count > 0)
        {
            foreach (var hit in hits)
            {
                var plane = hit.trackable as ARPlane;
                if (plane != null && plane.alignment == PlaneAlignment.Vertical)
                {
                    // Use the image target's size for proper scaling
                    float targetWidth = trackedImage.size.x;
                    float targetHeight = trackedImage.size.y;
                    
                    content.transform.parent = plane.transform;
                    content.transform.position = hit.pose.position;
                    content.transform.rotation = hit.pose.rotation;
                    
                    // Scale based on the image target's actual size
                    Vector3 scale = new Vector3(
                        targetWidth * wallScaleFactor,
                        targetHeight * wallScaleFactor,
                        1f
                    );
                    content.transform.localScale = scale;

                    contentAttachedToPlane = true;
                    break;
                }
            }
        }
        return contentAttachedToPlane;
    }
} 