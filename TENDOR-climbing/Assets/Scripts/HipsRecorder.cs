using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using RenderHeads.Media.AVProMovieCapture;

public class HipsRecorder : MonoBehaviour
{
    [SerializeField] private ARHumanBodyManager bodyManager;
    [SerializeField] private ARTrackedImageManager imageManager;
    [SerializeField] private CaptureFromTexture capture;
    [SerializeField] private GameObject startButton;

    private List<SerializablePose> poses = new List<SerializablePose>();
    private Transform imageTransform;
    private bool recording;
    private float startTime;

    void OnEnable()
    {
        if (ViewSwitcher.isARMode)
        {
            enabled = false;
            return;
        }
        imageManager.trackedImagesChanged += OnImagesChanged;
        startButton.SetActive(true);
    }

    void OnDisable()
    {
        imageManager.trackedImagesChanged -= OnImagesChanged;
        bodyManager.humanBodiesChanged -= OnBodiesChanged;
        if (startButton)
            startButton.SetActive(true);
    }

    private void OnImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var img in args.added)
            TryAssign(img);
        foreach (var img in args.updated)
            TryAssign(img);
    }

    private void TryAssign(ARTrackedImage img)
    {
        if (imageTransform == null && img.referenceImage.name == "test-target" && img.trackingState == TrackingState.Tracking)
        {
            imageTransform = img.transform;
            startButton.SetActive(true);
        }
    }

    public void StartRecording()
    {
        if (recording || imageTransform == null)
            return;
        poses.Clear();
        startTime = Time.time;
        capture.StartCapture();
        recording = true;
        bodyManager.humanBodiesChanged += OnBodiesChanged;
    }

    public void StopRecording()
    {
        if (!recording)
            return;
        recording = false;
        bodyManager.humanBodiesChanged -= OnBodiesChanged;
        capture.StopCapture();
        Save();
    }

    private void OnBodiesChanged(ARHumanBodiesChangedEventArgs args)
    {
        if (args.updated.Count == 0) return;
        var body = args.updated[0];
        var animator = body.GetComponent<Animator>();
        if (animator == null) return;
        var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
        if (hips == null) return;

        Vector3 localPos = imageTransform.InverseTransformPoint(hips.position);
        Quaternion localRot = Quaternion.Inverse(imageTransform.rotation) * hips.rotation;
        poses.Add(new SerializablePose(localPos, localRot, Time.time - startTime));
    }

    private void Save()
    {
        var data = new SerializablePoseCollection();
        data.poses = poses;
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, "hips.json");
        File.WriteAllText(path, json);
    }
}
