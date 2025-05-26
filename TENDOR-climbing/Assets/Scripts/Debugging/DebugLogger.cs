using UnityEngine;

namespace BodyTracking.Utils
{
    /// <summary>
    /// Debug logging utility for the body tracking system
    /// </summary>
    public static class DebugLogger
    {
        public static void Log(string message)
        {
            Debug.Log(message);
        }
        
        public static void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }
        
        public static void LogError(string message)
        {
            Debug.LogError(message);
        }
    }
} 