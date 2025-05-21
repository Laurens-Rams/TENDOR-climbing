using System;
using System.IO;
using UnityEngine;

public class LatestPlaybackManager : MonoBehaviour
{
    [SerializeField] private GameObject avatarPrefab;
    [SerializeField] private bool autoLoad = false;

    private BodyFrame[] frames;
    private int index;
    private Transform avatar;

    void Start()
    {
        if (autoLoad)
            LoadLatest();
    }

    public void LoadLatest()
    {
        string folder = Path.Combine(Application.persistentDataPath, "Captures");
        if (Directory.Exists(folder))
        {
            var files = Directory.GetFiles(folder, "*.body");
            if (files.Length > 0)
            {
                Array.Sort(files);
                string last = files[files.Length - 1];
                string json = File.ReadAllText(last);
                frames = JsonUtility.FromJson<BodyFrameCollection>(json).frames;
            }
        }

        if (avatarPrefab != null && avatar == null)
        {
            avatar = Instantiate(avatarPrefab, Vector3.zero, Quaternion.identity).transform;
        }
        index = 0;
    }

    void Update()
    {
        if (frames == null || index >= frames.Length || avatar == null || Globals.ClimbWallAnchor == null)
            return;

        var frame = frames[index];
        avatar.position = Globals.ClimbWallAnchor.TransformPoint(frame.position);
        avatar.rotation = Globals.ClimbWallAnchor.rotation * frame.rotation;
        index++;
    }

    public void Clear()
    {
        frames = null;
        index = 0;
        if (avatar != null)
        {
            Destroy(avatar.gameObject);
            avatar = null;
        }
    }
}
