using UnityEngine;
using System;

namespace TENDOR.Runtime.Models
{
    /// <summary>
    /// Authoritative pose data structure for body tracking
    /// Replaces any duplicate SerializablePose definitions
    /// </summary>
    [Serializable]
    public struct PoseData
    {
        [SerializeField] public Vector3 position;
        [SerializeField] public Quaternion rotation;
        [SerializeField] public float timestamp;

        public PoseData(Vector3 position, Quaternion rotation, float timestamp = 0f)
        {
            this.position = position;
            this.rotation = rotation;
            this.timestamp = timestamp > 0f ? timestamp : Time.time;
        }

        public PoseData(Transform transform, float timestamp = 0f)
        {
            this.position = transform.position;
            this.rotation = transform.rotation;
            this.timestamp = timestamp > 0f ? timestamp : Time.time;
        }

        public void ApplyTo(Transform transform)
        {
            if (transform != null)
            {
                transform.position = position;
                transform.rotation = rotation;
            }
        }

        public static PoseData Identity => new PoseData(Vector3.zero, Quaternion.identity, 0f);

        public override string ToString()
        {
            return $"PoseData(pos: {position:F3}, rot: {rotation:F3}, time: {timestamp:F3})";
        }
    }

    /// <summary>
    /// Collection of pose data for body tracking recording
    /// </summary>
    [Serializable]
    public class BodyTrackingData
    {
        [SerializeField] public PoseData[] hipPoses;
        [SerializeField] public float recordingDuration;
        [SerializeField] public string recordingId;
        [SerializeField] public DateTime recordingTimestamp;

        public BodyTrackingData(int capacity = 1000)
        {
            hipPoses = new PoseData[capacity];
            recordingDuration = 0f;
            recordingId = Guid.NewGuid().ToString();
            recordingTimestamp = DateTime.UtcNow;
        }

        public bool IsValid => hipPoses != null && hipPoses.Length > 0 && recordingDuration > 0f;
    }
} 