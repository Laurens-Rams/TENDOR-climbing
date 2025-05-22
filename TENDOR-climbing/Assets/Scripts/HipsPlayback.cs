using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class HipsPlayback : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager imageManager;
    [SerializeField] private GameObject avatarPrefab;
    [SerializeField] private GameObject skeletonPrefab;

    private List<SerializablePose> poses;
    private Transform imageTransform;
    private GameObject avatarInstance;
    private GameObject skeletonInstance;
    private float startTime;
    private int index;

    void OnEnable()
    {
        if (!imageManager)
            imageManager = FindFirstObjectByType<ARTrackedImageManager>();
        if (!ViewSwitcher.isARMode)
        {
            enabled = false;
            return;
        }
        Load();
        imageManager.trackedImagesChanged += OnImagesChanged;
    }

    void OnDisable()
    {
        imageManager.trackedImagesChanged -= OnImagesChanged;
        if (avatarInstance)
            Destroy(avatarInstance);
        avatarInstance = null;
        if (skeletonInstance)
            Destroy(skeletonInstance);
        skeletonInstance = null;
    }

    private void Load()
    {
        string path = Path.Combine(Application.persistentDataPath, "hips.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<SerializablePoseCollection>(json);
            poses = data.poses;
        }
    }

    private void OnImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var img in args.added)
            TrySpawn(img);
        foreach (var img in args.updated)
            TrySpawn(img);
    }

    private void TrySpawn(ARTrackedImage img)
    {
        if (imageTransform == null && img.referenceImage.name == "test-target" && img.trackingState == TrackingState.Tracking)
        {
            imageTransform = img.transform;
            if (poses != null && poses.Count > 0 && avatarPrefab)
            {
                Vector3 worldPos = imageTransform.TransformPoint(poses[0].position);
                Quaternion worldRot = imageTransform.rotation * poses[0].rotation;
                avatarInstance = Instantiate(avatarPrefab, worldPos, worldRot);
                if (skeletonPrefab)
                {
                    skeletonInstance = Instantiate(skeletonPrefab, worldPos, worldRot, avatarInstance.transform);
                    var skeleton = skeletonInstance.GetComponent<StickFigureSkeleton>();
                    if (skeleton)
                        skeleton.enabled = true;
                }
                startTime = Time.time;
                index = 0;
            }
        }
    }

    void Update()
    {
        if (avatarInstance == null || poses == null || poses.Count == 0)
            return;
        float elapsed = Time.time - startTime;
        while (index + 1 < poses.Count && poses[index + 1].time <= elapsed)
            index++;
        if (index + 1 < poses.Count)
        {
            var a = poses[index];
            var b = poses[index + 1];
            float t = Mathf.InverseLerp(a.time, b.time, elapsed);
            Vector3 pos = Vector3.Lerp(a.position, b.position, t);
            Quaternion rot = Quaternion.Slerp(a.rotation, b.rotation, t);
            avatarInstance.transform.SetPositionAndRotation(imageTransform.TransformPoint(pos), imageTransform.rotation * rot);
        }
    }
}
