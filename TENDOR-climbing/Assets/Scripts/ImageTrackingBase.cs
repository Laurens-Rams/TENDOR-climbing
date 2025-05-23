using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public abstract class ImageTrackingBase : MonoBehaviour
{
    [SerializeField] protected ARTrackedImageManager imageManager;
    [SerializeField] protected GameObject wallImagePrefab;
    
    protected Transform imageTransform;
    protected GameObject wallInstance;
    protected bool isWallPlaced = false;

    protected virtual void OnEnable()
    {
        if (!imageManager)
            imageManager = FindFirstObjectByType<ARTrackedImageManager>();
            
        imageManager.trackedImagesChanged += OnImagesChanged;
    }

    protected virtual void OnDisable()
    {
        if (imageManager)
            imageManager.trackedImagesChanged -= OnImagesChanged;
            
        CleanupInstances();
    }

    protected virtual void CleanupInstances()
    {
        if (wallInstance)
            Destroy(wallInstance);
        wallInstance = null;
    }

    private void OnImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var img in eventArgs.added)
        {
            HandleImageTracked(img);
        }

        foreach (var img in eventArgs.updated)
        {
            if (img.trackingState == TrackingState.Tracking)
            {
                HandleImageTracked(img);
            }
        }
    }

    protected virtual void HandleImageTracked(ARTrackedImage img)
    {
        if (isWallPlaced) return;

        Debug.Log($"[ImageTracking] Target found: {img.referenceImage.name}");
        imageTransform = img.transform;

        if (wallImagePrefab && !wallInstance)
        {
            var ray = new Ray(img.transform.position, img.transform.forward);
            var hits = new List<ARRaycastHit>();
            var raycastManager = FindFirstObjectByType<ARRaycastManager>();
            
            if (raycastManager && raycastManager.Raycast(ray, hits, TrackableType.Planes))
            {
                foreach (var hit in hits)
                {
                    var plane = hit.trackable as ARPlane;
                    if (plane != null && plane.alignment == PlaneAlignment.Vertical)
                    {
                        PlaceWallAndInitialize(img, hit.pose.position, hit.pose.rotation);
                        break;
                    }
                }
            }
        }
    }

    protected virtual void PlaceWallAndInitialize(ARTrackedImage img, Vector3 position, Quaternion rotation)
    {
        wallInstance = Instantiate(wallImagePrefab, position, rotation);
        wallInstance.transform.localScale = new Vector3(img.size.x, img.size.y, 1f);
        isWallPlaced = true;
        OnWallPlaced(position, rotation);
    }

    // Override this in derived classes to handle what happens after wall placement
    protected abstract void OnWallPlaced(Vector3 position, Quaternion rotation);
} 