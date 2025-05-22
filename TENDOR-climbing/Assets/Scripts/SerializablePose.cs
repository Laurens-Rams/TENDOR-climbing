using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public struct SerializablePose
{
    public Vector3 position;
    public Quaternion rotation;
    public float time;

    public SerializablePose(Vector3 pos, Quaternion rot, float t)
    {
        position = pos;
        rotation = rot;
        time = t;
    }
}

[Serializable]
public class SerializablePoseCollection
{
    public List<SerializablePose> poses = new List<SerializablePose>();
}
