using UnityEngine;
using UnityEditor;
using RenderHeads.Media.AVProMovieCapture;
using BodyTracking.Recording;

namespace BodyTracking.Editor
{
    /// <summary>
    /// Simple compilation test for AVPro integration
    /// </summary>
    public class CompilationTest : EditorWindow
    {
        [MenuItem("TENDOR/Test Compilation")]
        public static void TestCompilation()
        {
            Debug.Log("=== TESTING COMPILATION ===");
            
            // Test that we can access AVPro types
            var resolution = CaptureBase.Resolution.HD_1920x1080;
            var outputPath = CaptureBase.OutputPath.RelativeToPersistentData;
            
            Debug.Log($"✅ AVPro types accessible:");
            Debug.Log($"  Resolution: {resolution}");
            Debug.Log($"  OutputPath: {outputPath}");
            
            // Test that we can find SynchronizedVideoRecorder
            var recorder = FindFirstObjectByType<SynchronizedVideoRecorder>();
            if (recorder != null)
            {
                Debug.Log($"✅ SynchronizedVideoRecorder found: {recorder.name}");
            }
            else
            {
                Debug.Log("ℹ️ SynchronizedVideoRecorder not found in scene (this is normal if not set up yet)");
            }
            
            Debug.Log("🎉 Compilation test completed successfully!");
        }
    }
} 