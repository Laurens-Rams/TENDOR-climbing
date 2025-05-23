using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct SerializablePose
{
    public float time;
    public Vector3 position;
    public Quaternion rotation;
}

[System.Serializable]
public class SerializablePoseCollection
{
    public List<SerializablePose> poses = new List<SerializablePose>();
}
