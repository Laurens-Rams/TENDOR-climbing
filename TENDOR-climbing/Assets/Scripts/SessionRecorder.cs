using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[Serializable]
public struct BodyFrame
{
    public float time;
    public Vector3 position;
    public Quaternion rotation;
}

[Serializable]
public class BodyFrameCollection
{
    public BodyFrame[] frames;
}

public class SessionRecorder : VideoRecorder
{
    [SerializeField] private BodyTracker bodyTracker;

    private List<BodyFrame> bodyFrames = new List<BodyFrame>();
    private float startTime;

    public override void StartRecording()
    {
        base.StartRecording();
        startTime = Time.time;
        bodyFrames.Clear();
        if (!bodyTracker)
            bodyTracker = Globals.BodyTracker;
    }

    public override void StopRecording()
    {
        base.StopRecording();
        SaveFrames();
    }

    protected override void RecordFrame(ARCameraFrameEventArgs args)
    {
        base.RecordFrame(args);
        if (bodyTracker && bodyTracker.Current)
        {
            Transform hips = bodyTracker.Current.transform;
            Vector3 pos = hips.position;
            Quaternion rot = hips.rotation;
            if (Globals.ClimbWallAnchor)
            {
                pos = Globals.ClimbWallAnchor.InverseTransformPoint(pos);
                rot = Quaternion.Inverse(Globals.ClimbWallAnchor.rotation) * rot;
            }
            BodyFrame frame = new BodyFrame
            {
                time = Time.time - startTime,
                position = pos,
                rotation = rot
            };
            bodyFrames.Add(frame);
        }
    }

    private void SaveFrames()
    {
        BodyFrameCollection collection = new BodyFrameCollection { frames = bodyFrames.ToArray() };
        string folder = Path.Combine(Application.persistentDataPath, "Captures");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".body");
        File.WriteAllText(path, JsonUtility.ToJson(collection));
    }
}
