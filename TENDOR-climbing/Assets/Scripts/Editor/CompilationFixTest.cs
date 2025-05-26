using UnityEngine;
using UnityEditor;
using RenderHeads.Media.AVProMovieCapture;
using BodyTracking.Recording;

namespace BodyTracking.Editor
{
    /// <summary>
    /// Test to verify compilation fixes are working
    /// </summary>
    public static class CompilationFixTest
    {
        [MenuItem("TENDOR/Test Compilation Fixes")]
        public static void TestCompilationFixes()
        {
            Debug.Log("=== TESTING COMPILATION FIXES ===");
            
            try
            {
                // Test 1: AVPro API fixes
                var resolution = CaptureBase.Resolution.HD_1920x1080;
                var outputPath = CaptureBase.OutputPath.RelativeToPersistentData;
                Debug.Log($"✅ AVPro API works: {resolution}, {outputPath}");
                
                // Test 2: Object.DestroyImmediate works
                var testGO = new GameObject("TestObject");
                Object.DestroyImmediate(testGO);
                Debug.Log("✅ Object.DestroyImmediate works");
                
                // Test 3: BodyTrackingRecorder type is accessible
                var recorderType = typeof(BodyTrackingRecorder);
                Debug.Log($"✅ BodyTrackingRecorder type accessible: {recorderType.Name}");
                
                // Test 4: SynchronizedVideoRecorder type is accessible
                var videoRecorderType = typeof(SynchronizedVideoRecorder);
                Debug.Log($"✅ SynchronizedVideoRecorder type accessible: {videoRecorderType.Name}");
                
                Debug.Log("🎉 All compilation fixes verified successfully!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Compilation fix test failed: {e.Message}");
                Debug.LogError($"Stack trace: {e.StackTrace}");
            }
        }
    }
} 