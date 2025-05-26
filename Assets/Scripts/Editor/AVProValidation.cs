using UnityEngine;
using UnityEditor;
using RenderHeads.Media.AVProMovieCapture;

namespace BodyTracking.Editor
{
    /// <summary>
    /// Validates AVPro Movie Capture API usage
    /// </summary>
    public static class AVProValidation
    {
        [MenuItem("TENDOR/Validate AVPro API")]
        public static void ValidateAVProAPI()
        {
            Debug.Log("=== VALIDATING AVPRO API ===");
            
            try
            {
                // Test Resolution enum
                var resolution = CaptureBase.Resolution.HD_1920x1080;
                Debug.Log($"✅ Resolution enum works: {resolution}");
                
                // Test OutputPath enum
                var outputPath = CaptureBase.OutputPath.RelativeToPersistentData;
                Debug.Log($"✅ OutputPath enum works: {outputPath}");
                
                // Test creating a CaptureFromCamera component
                var go = new GameObject("TestCapture");
                var capture = go.AddComponent<CaptureFromCamera>();
                
                // Test setting properties
                capture.FilenamePrefix = "Test";
                capture.AppendFilenameTimestamp = true;
                capture.OutputFolder = outputPath;
                capture.OutputFolderPath = "TestFolder";
                capture.FilenameExtension = ".mp4";
                
                Debug.Log($"✅ CaptureFromCamera properties work:");
                Debug.Log($"  FilenamePrefix: {capture.FilenamePrefix}");
                Debug.Log($"  AppendFilenameTimestamp: {capture.AppendFilenameTimestamp}");
                Debug.Log($"  OutputFolder: {capture.OutputFolder}");
                Debug.Log($"  OutputFolderPath: {capture.OutputFolderPath}");
                Debug.Log($"  FilenameExtension: {capture.FilenameExtension}");
                
                // Test LastFilePath property
                string lastPath = capture.LastFilePath;
                Debug.Log($"✅ LastFilePath property accessible: {(string.IsNullOrEmpty(lastPath) ? "empty" : lastPath)}");
                
                // Cleanup
                Object.DestroyImmediate(go);
                
                Debug.Log("🎉 AVPro API validation completed successfully!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ AVPro API validation failed: {e.Message}");
                Debug.LogError($"Stack trace: {e.StackTrace}");
            }
        }
    }
} 