using System;
using UnityEngine;

namespace TENDOR.Runtime.Models
{
    /// <summary>
    /// Firestore model for gym data
    /// </summary>
    [Serializable]
    public class GymData
    {
        public string id;
        public string name;
        public GeoPoint location;
        public DateTime createdAt;

        public GymData()
        {
            id = string.Empty;
            name = string.Empty;
            location = new GeoPoint();
            createdAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Firestore model for boulder/route data
    /// </summary>
    [Serializable]
    public class BoulderData
    {
        public string id;
        public string gymId;           // FK to gyms/* (nullable)
        public string name;            // "MoonBoard A-2"
        public string grade;           // "V4+"
        public string targetUrl;       // gs://imageTargets/...
        public float physicalWidthM;   // AR sizing
        public bool isActive;          // hide if reset
        public DateTime createdAt;

        public BoulderData()
        {
            id = string.Empty;
            gymId = null;
            name = string.Empty;
            grade = string.Empty;
            targetUrl = string.Empty;
            physicalWidthM = 1.0f;
            isActive = true;
            createdAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Firestore model for climb recording data
    /// </summary>
    [Serializable]
    public class ClimbData
    {
        public string id;
        public string ownerUid;        // auth.uid
        public string boulderId;       // FK to boulders/*
        public string videoUrl;        // gs://videos/...
        public string fbxUrl;          // gs://outputs/... (nullable)
        public string jsonUrl;         // gs://outputs/... (nullable)
        public ClimbStatus status;     // uploading | processing | ready | error
        public DateTime createdAt;

        public ClimbData()
        {
            id = string.Empty;
            ownerUid = string.Empty;
            boulderId = string.Empty;
            videoUrl = string.Empty;
            fbxUrl = null;
            jsonUrl = null;
            status = ClimbStatus.Uploading;
            createdAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Simple game state enum for compatibility
    /// </summary>
    public enum GameState
    {
        Idle,
        Recording,
        Processing,
        Playback
    }

    /// <summary>
    /// Climb processing status
    /// </summary>
    public enum ClimbStatus
    {
        Uploading,
        Processing,
        Ready,
        Error
    }

    /// <summary>
    /// Simple GeoPoint structure for Firestore compatibility
    /// </summary>
    [Serializable]
    public struct GeoPoint
    {
        public double latitude;
        public double longitude;

        public GeoPoint(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public static GeoPoint Zero => new GeoPoint(0, 0);
    }
} 