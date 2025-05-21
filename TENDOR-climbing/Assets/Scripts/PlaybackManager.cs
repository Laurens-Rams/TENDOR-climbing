using System;
using System.Collections.Generic;
using UnityEngine;

public class PlaybackManager : MonoBehaviour
{
    [SerializeField] private TextAsset bodyFile;
    [SerializeField] private GameObject avatarPrefab;

    private BodyFrame[] frames;
    private int index;
    private Transform avatar;

    void Start()
    {
        if (bodyFile != null)
        {
            frames = JsonUtility.FromJson<BodyFrameCollection>(bodyFile.text).frames;
        }
        if (avatarPrefab != null)
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
}
