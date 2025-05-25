using UnityEngine;
using BodyTracking.Data;
using System.IO;
using System.Collections.Generic;
using System;

namespace BodyTracking.Storage
{
    /// <summary>
    /// Handles persistent storage and retrieval of hip tracking recordings
    /// </summary>
    public static class RecordingStorage
    {
        private const string RECORDINGS_FOLDER = "BodyTrackingRecordings";
        private const string FILE_EXTENSION = ".json";
        private const string BINARY_EXTENSION = ".dat";
        
        private static string RecordingsPath => Path.Combine(Application.persistentDataPath, RECORDINGS_FOLDER);
        
        /// <summary>
        /// Storage format options
        /// </summary>
        public enum StorageFormat
        {
            JSON,    // Human readable, larger size
            Binary   // Compact, faster loading
        }

        /// <summary>
        /// Save a hip recording to persistent storage
        /// </summary>
        public static bool SaveRecording(HipRecording recording, string fileName, StorageFormat format = StorageFormat.JSON)
        {
            if (recording == null || !recording.IsValid)
            {
                Debug.LogError("[RecordingStorage] Invalid hip recording provided");
                return false;
            }
            
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = $"hip_recording_{DateTime.Now:yyyyMMdd_HHmmss}";
            }
            
            // Ensure recordings directory exists
            Directory.CreateDirectory(RecordingsPath);
            
            try
            {
                string filePath = GetFilePath(fileName, format);
                
                switch (format)
                {
                    case StorageFormat.JSON:
                        return SaveAsJSON(recording, filePath);
                    case StorageFormat.Binary:
                        return SaveAsBinary(recording, filePath);
                    default:
                        Debug.LogError($"[RecordingStorage] Unsupported format: {format}");
                        return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[RecordingStorage] Error saving hip recording: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load a hip recording from persistent storage
        /// </summary>
        public static HipRecording LoadRecording(string fileName, StorageFormat format = StorageFormat.JSON)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError("[RecordingStorage] File name is required");
                return null;
            }
            
            try
            {
                string filePath = GetFilePath(fileName, format);
                
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[RecordingStorage] File not found: {filePath}");
                    return null;
                }
                
                switch (format)
                {
                    case StorageFormat.JSON:
                        return LoadFromJSON(filePath);
                    case StorageFormat.Binary:
                        return LoadFromBinary(filePath);
                    default:
                        Debug.LogError($"[RecordingStorage] Unsupported format: {format}");
                        return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[RecordingStorage] Error loading hip recording: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get list of available recordings
        /// </summary>
        public static List<string> GetAvailableRecordings(StorageFormat format = StorageFormat.JSON)
        {
            var recordings = new List<string>();
            
            if (!Directory.Exists(RecordingsPath))
                return recordings;
            
            string extension = format == StorageFormat.JSON ? FILE_EXTENSION : BINARY_EXTENSION;
            var files = Directory.GetFiles(RecordingsPath, $"*{extension}");
            
            foreach (var file in files)
            {
                recordings.Add(Path.GetFileNameWithoutExtension(file));
            }
            
            return recordings;
        }

        /// <summary>
        /// Delete a recording
        /// </summary>
        public static bool DeleteRecording(string fileName, StorageFormat format = StorageFormat.JSON)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;
            
            try
            {
                string filePath = GetFilePath(fileName, format);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"[RecordingStorage] Deleted hip recording: {fileName}");
                    return true;
                }
                
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[RecordingStorage] Error deleting hip recording: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get recording metadata without loading full data
        /// </summary>
        public static RecordingMetadata GetRecordingMetadata(string fileName, StorageFormat format = StorageFormat.JSON)
        {
            try
            {
                string filePath = GetFilePath(fileName, format);
                
                if (!File.Exists(filePath))
                    return null;
                
                var fileInfo = new FileInfo(filePath);
                
                // For JSON, we can quickly parse just the metadata
                if (format == StorageFormat.JSON)
                {
                    string json = File.ReadAllText(filePath);
                    var recording = JsonUtility.FromJson<HipRecording>(json);
                    
                    return new RecordingMetadata
                    {
                        fileName = fileName,
                        duration = recording.duration,
                        frameCount = recording.FrameCount,
                        frameRate = recording.frameRate,
                        recordingTimestamp = recording.recordingTimestamp,
                        fileSizeBytes = fileInfo.Length,
                        format = format
                    };
                }
                
                // For binary, return basic file info
                return new RecordingMetadata
                {
                    fileName = fileName,
                    fileSizeBytes = fileInfo.Length,
                    format = format
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"[RecordingStorage] Error getting metadata: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get total storage used by recordings
        /// </summary>
        public static long GetTotalStorageUsed()
        {
            if (!Directory.Exists(RecordingsPath))
                return 0;
            
            long totalSize = 0;
            var files = Directory.GetFiles(RecordingsPath);
            
            foreach (var file in files)
            {
                totalSize += new FileInfo(file).Length;
            }
            
            return totalSize;
        }

        #region Private Methods

        private static string GetFilePath(string fileName, StorageFormat format)
        {
            string extension = format == StorageFormat.JSON ? FILE_EXTENSION : BINARY_EXTENSION;
            return Path.Combine(RecordingsPath, fileName + extension);
        }

        private static bool SaveAsJSON(HipRecording recording, string filePath)
        {
            string json = JsonUtility.ToJson(recording, true);
            File.WriteAllText(filePath, json);
            
            Debug.Log($"[RecordingStorage] Saved hip recording as JSON: {filePath} ({new FileInfo(filePath).Length} bytes)");
            return true;
        }

        private static bool SaveAsBinary(HipRecording recording, string filePath)
        {
            // For binary format, we'd implement custom serialization
            // For now, falling back to JSON for simplicity
            Debug.LogWarning("[RecordingStorage] Binary format not yet implemented, using JSON");
            return SaveAsJSON(recording, filePath.Replace(BINARY_EXTENSION, FILE_EXTENSION));
        }

        private static HipRecording LoadFromJSON(string filePath)
        {
            string json = File.ReadAllText(filePath);
            var recording = JsonUtility.FromJson<HipRecording>(json);
            
            Debug.Log($"[RecordingStorage] Loaded hip recording from JSON: {recording.FrameCount} frames, {recording.duration:F2}s");
            return recording;
        }

        private static HipRecording LoadFromBinary(string filePath)
        {
            // For binary format, we'd implement custom deserialization
            // For now, falling back to JSON for simplicity
            Debug.LogWarning("[RecordingStorage] Binary format not yet implemented, trying JSON");
            return LoadFromJSON(filePath.Replace(BINARY_EXTENSION, FILE_EXTENSION));
        }

        #endregion
    }

    /// <summary>
    /// Metadata about a recording file
    /// </summary>
    [Serializable]
    public class RecordingMetadata
    {
        public string fileName;
        public float duration;
        public int frameCount;
        public float frameRate;
        public DateTime recordingTimestamp;
        public long fileSizeBytes;
        public RecordingStorage.StorageFormat format;
        
        public string FormattedFileSize
        {
            get
            {
                string[] sizes = { "B", "KB", "MB", "GB" };
                double len = fileSizeBytes;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }
                return $"{len:0.##} {sizes[order]}";
            }
        }
        
        public string FormattedDuration => TimeSpan.FromSeconds(duration).ToString(@"mm\:ss");
    }
} 