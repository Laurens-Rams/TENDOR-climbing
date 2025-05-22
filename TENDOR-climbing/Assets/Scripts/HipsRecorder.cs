using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using RenderHeads.Media.AVProMovieCapture;

public class HipsRecorder : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField]  private ARHumanBodyManager   bodyManager;
    [SerializeField]  private ARTrackedImageManager imageManager;
    [SerializeField]  private CaptureFromTexture   capture;
    [SerializeField]  private RawImage             previewImage;   // ‼️ new – optional
    [SerializeField]  private GameObject           startButton;

    [Header("Debug / overlay")]
    [SerializeField]  private TMPro.TextMeshProUGUI frameCounter;  // optional

    // ────────────────────────────────────────────────────────────────
    private readonly List<SerializablePose> poses = new();
    private Transform   imageTransform;
    private bool        recording;
    private float       startTime;
    private Texture2D   tex;
    private XRCpuImage  cpuImage;

    // ────────────────────────────────────────────────────────────────
    #region Unity lifecycle
    //----------------------------------------------------------------
    void OnEnable()
    {
        bodyManager  ??= FindFirstObjectByType<ARHumanBodyManager>();
        imageManager ??= FindFirstObjectByType<ARTrackedImageManager>();
        capture      ??= FindFirstObjectByType<CaptureFromTexture>();

        if (ViewSwitcher.isARMode)
        {
            enabled = false;
            Debug.Log("[HipsRecorder] Disabled – AR-mode active.");
            return;
        }

        imageManager.trackedImagesChanged += OnImagesChanged;
        startButton.SetActive(false);
        Debug.Log("[HipsRecorder] Waiting for image target…");
    }

    void OnDisable()
    {
        imageManager.trackedImagesChanged -= OnImagesChanged;
        bodyManager.humanBodiesChanged    -= OnBodiesChanged;
        Globals.CameraManager.frameReceived -= RecordFrame;

        if (startButton) startButton.SetActive(false);
    }
    #endregion
    // ────────────────────────────────────────────────────────────────

    #region Image tracking
    //----------------------------------------------------------------
    private void OnImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var img in args.added)   TryAssign(img);
        foreach (var img in args.updated) TryAssign(img);
    }

    private void TryAssign(ARTrackedImage img)
    {
        Debug.Log($"[HipsRecorder] TryAssign ➜ {img.referenceImage.name} ({img.trackingState})");

        if (imageTransform == null &&
            img.referenceImage.name == "Wall 1" &&
            img.trackingState      == TrackingState.Tracking)
        {
            imageTransform = img.transform;
            startButton.SetActive(true);
            Debug.Log("[HipsRecorder] 🎯 Target found – start button enabled");
        }
    }
    #endregion
    // ────────────────────────────────────────────────────────────────

    #region Recording control (UI hook)
    //----------------------------------------------------------------
    public void StartRecording()
    {
        if (recording || imageTransform == null) { Debug.LogWarning("[HipsRecorder] Cannot start – already recording or target missing"); return; }

        // ── 1. video setup (identical to your VideoRecorder) ──
        int w = (int)Globals.CameraManager.currentConfiguration?.width;
        int h = (int)Globals.CameraManager.currentConfiguration?.height;
        tex = new Texture2D(w, h, TextureFormat.RGBA32, false);

        if (previewImage) previewImage.texture = tex;   // live preview (optional)

        capture.SetSourceTexture(tex);
        capture.StartCapture();
        Debug.Log($"[HipsRecorder] ▶️ AVPro capture started ({w}×{h})");

        Globals.CameraManager.frameReceived += RecordFrame;

        // ── 2. pose setup ──
        poses.Clear();
        startTime = Time.time;
        bodyManager.humanBodiesChanged += OnBodiesChanged;

        if (frameCounter) frameCounter.text = "0";

        startButton.SetActive(false);
        recording = true;
    }

    public void StopRecording()
    {
        if (!recording) return;

        // 1. video off
        Globals.CameraManager.frameReceived -= RecordFrame;
        capture.StopCapture();
        Debug.Log("[HipsRecorder] ⏹️ AVPro capture stopped");

        // 2. pose off
        bodyManager.humanBodiesChanged -= OnBodiesChanged;
        Save();

        startButton.SetActive(true);
        recording = false;
    }
    #endregion
    // ────────────────────────────────────────────────────────────────

    #region Frame capture + pose tracking
    //----------------------------------------------------------------
    private void RecordFrame(ARCameraFrameEventArgs args)
    {
        if (!Globals.CameraManager.TryAcquireLatestCpuImage(out cpuImage)) return;

        var conv = new XRCpuImage.ConversionParams(cpuImage, TextureFormat.RGBA32, XRCpuImage.Transformation.MirrorY);
        var data = tex.GetRawTextureData<byte>();
        cpuImage.Convert(conv, data);
        tex.Apply();

        capture.UpdateSourceTexture();
        capture.UpdateFrame();
        cpuImage.Dispose();

        if (frameCounter)
        {
            int n = int.Parse(frameCounter.text);
            frameCounter.text = (n + 1).ToString();
        }
    }

 private void OnBodiesChanged(ARHumanBodiesChangedEventArgs args)
{
    // Fired?  ✅  we already know this prints once per frame
    Debug.Log($"[Body] event fired – updated {args.updated.Count}");

    if (args.updated.Count == 0 || imageTransform == null) return;

    // ————————————————————————————————————————
    // 1. access the XR skeleton
    var body   = args.updated[0];
    var joints = body.joints;                   // NativeArray<XRHumanBodyJoint>

    // ⬇️  insert the debug line here
    Debug.Log("[Body] joints.IsCreated = " + joints.IsCreated +
              "  firstJoint.tracked = " + joints[0].tracked +
              "  runningOn = " + Application.platform);
              

    if (!joints.IsCreated)                      // 🔸 Check-point A
    {
        Debug.LogWarning("[Body] joints array NOT created (Pose 3D feature off or device unsupported)");
        return;
    }

    // ————————————————————————————————————————
    // 2. pick the hips joint  (index 0 is root/hips in ARKit)
    var hipsJoint = joints[0];

    if (!hipsJoint.tracked)                     // 🔸 Check-point B
    {
        Debug.Log("[Body] hips joint not tracked this frame");
        return;
    }

    // ————————————————————————————————————————
    // 3. convert to image-target local space
    Vector3 hipsWorldPos = hipsJoint.anchorPose.position;
    Quaternion hipsWorldRot = hipsJoint.anchorPose.rotation;

    Vector3 localPos = imageTransform.InverseTransformPoint(hipsWorldPos);
    Quaternion localRot = Quaternion.Inverse(imageTransform.rotation) * hipsWorldRot;

    poses.Add(new SerializablePose(localPos, localRot, Time.time - startTime));
    Debug.Log($"[HipsRecorder] Pose #{poses.Count} recorded");      // 🔸 Check-point C
}
    #endregion
    // ────────────────────────────────────────────────────────────────

    #region Save JSON
    //----------------------------------------------------------------
    private void Save()
    {
        var data = new SerializablePoseCollection { poses = poses };
        string json  = JsonUtility.ToJson(data, true);
        string path  = Path.Combine(Application.persistentDataPath, "hips.json");
        File.WriteAllText(path, json);
        Debug.Log($"[HipsRecorder] Saved {poses.Count} poses ➜ {path}");
    }
    #endregion


    
}
