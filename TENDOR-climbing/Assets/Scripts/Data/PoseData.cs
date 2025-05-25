using UnityEngine;
using System.Collections.Generic;
using System;

namespace BodyTracking.Data
{
    /// <summary>
    /// Represents hip joint data in 3D space with tracking confidence
    /// </summary>
    [Serializable]
    public struct HipJointData
    {
        public Vector3 position;
        public float confidence;
        public bool isTracked;

        public HipJointData(Vector3 pos, float conf = 1.0f, bool tracked = true)
        {
            position = pos;
            confidence = conf;
            isTracked = tracked;
        }

        public static HipJointData Invalid => new HipJointData(Vector3.zero, 0f, false);
        public bool IsValid => isTracked && confidence > 0f;
    }

    /// <summary>
    /// Hip position data for a single frame
    /// </summary>
    [Serializable]
    public struct HipFrame
    {
        public float timestamp;
        public HipJointData hipJoint;
        
        public bool IsValid => timestamp >= 0 && hipJoint.IsValid;
    }

    /// <summary>
    /// Complete hip tracking session data
    /// </summary>
    [Serializable]
    public class HipRecording
    {
        public List<HipFrame> frames = new List<HipFrame>();
        public float duration;
        public float frameRate;
        public Vector3 referenceImageTargetPosition;
        public Quaternion referenceImageTargetRotation;
        public Vector3 referenceImageTargetScale;
        public DateTime recordingTimestamp;
        
        public int FrameCount => frames.Count;
        public bool IsValid => frames.Count > 0 && duration > 0;
        
        /// <summary>
        /// Get frame at specific time
        /// </summary>
        public HipFrame GetFrameAtTime(float time)
        {
            if (frames.Count == 0) return default;
            
            // Clamp time to recording bounds
            time = Mathf.Clamp(time, 0, duration);
            
            // Find frame index
            int frameIndex = Mathf.FloorToInt(time * frameRate);
            frameIndex = Mathf.Clamp(frameIndex, 0, frames.Count - 1);
            
            return frames[frameIndex];
        }
    }

    /// <summary>
    /// Coordinate system transformation helper
    /// </summary>
    [Serializable]
    public struct CoordinateFrame
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        
        public CoordinateFrame(Transform transform)
        {
            position = transform.position;
            rotation = transform.rotation;
            scale = transform.localScale;
        }
        
        public Matrix4x4 ToMatrix()
        {
            return Matrix4x4.TRS(position, rotation, scale);
        }
        
        public Vector3 TransformPoint(Vector3 point)
        {
            return ToMatrix().MultiplyPoint3x4(point);
        }
        
        public Vector3 InverseTransformPoint(Vector3 point)
        {
            return ToMatrix().inverse.MultiplyPoint3x4(point);
        }
    }
} 