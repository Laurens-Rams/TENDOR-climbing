using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlaybackManager : MonoBehaviour
{
    [SerializeField] private GameObject avatarPrefab;
    GameObject avatarInstance;
    List<Pose> poses = new List<Pose>();
    int index;

    public void Load(string path)
    {
        poses.Clear();
        foreach (var line in File.ReadAllLines(path))
        {
            var parts = line.Split(',');
            if (parts.Length < 8) continue;
            var pos = new Vector3(
                float.Parse(parts[1]),
                float.Parse(parts[2]),
                float.Parse(parts[3]));
            var rot = new Quaternion(
                float.Parse(parts[4]),
                float.Parse(parts[5]),
                float.Parse(parts[6]),
                float.Parse(parts[7]));
            poses.Add(new Pose(pos, rot));
        }
        index = 0;
        if (avatarInstance)
            Destroy(avatarInstance);
        avatarInstance = Instantiate(avatarPrefab, Vector3.zero, Quaternion.identity);
    }

    void Update()
    {
        if (avatarInstance == null || Globals.ClimbWallAnchor == null) return;
        if (index >= poses.Count) return;

        var local = poses[index++];
        avatarInstance.transform.position = Globals.ClimbWallAnchor.TransformPoint(local.position);
        avatarInstance.transform.rotation = Globals.ClimbWallAnchor.rotation * local.rotation;
    }
}
