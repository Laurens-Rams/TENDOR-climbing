using System.IO;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SessionRecorder : VideoRecorder
{
    StreamWriter writer;

    public override void StartRecording()
    {
        base.StartRecording();
        var folder = Path.Combine(Application.persistentDataPath, "Captures");
        Directory.CreateDirectory(folder);
        var name = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var path = Path.Combine(folder, name + ".body");
        writer = new StreamWriter(path);
    }

    public override void StopRecording()
    {
        base.StopRecording();
        writer?.Close();
    }

    protected override void RecordFrame(ARCameraFrameEventArgs args)
    {
        base.RecordFrame(args);
        if (writer == null) return;

        var body = Globals.BodyTracker?.Current;
        if (body == null || Globals.ClimbWallAnchor == null)
            return;

        var hips = body.joints[(int)XRHumanBodyJointType.Spine];
        if (!hips.tracked)
            return;
        var worldPos = hips.anchorPose.position;
        var worldRot = hips.anchorPose.rotation;
        var localPos = Globals.ClimbWallAnchor.InverseTransformPoint(worldPos);
        var localRot = Quaternion.Inverse(Globals.ClimbWallAnchor.rotation) * worldRot;
        writer.WriteLine($"{Time.time:F4},{localPos.x:F4},{localPos.y:F4},{localPos.z:F4},{localRot.x:F4},{localRot.y:F4},{localRot.z:F4},{localRot.w:F4}");
    }
}
