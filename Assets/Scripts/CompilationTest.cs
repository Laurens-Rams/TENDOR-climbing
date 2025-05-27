using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;
using TENDOR.Core;
using TENDOR.Services;
using TENDOR.Services.AR;
using TENDOR.Services.Firebase;
using TENDOR.Recording;
using TENDOR.Runtime.Models;
using TENDOR.Testing;
using Logger = TENDOR.Core.Logger;

namespace TENDOR
{
    /// <summary>
    /// Tests that all assemblies compile correctly and can reference each other
    /// Updated to test the restored BodyTrackingController system
    /// </summary>
    public class CompilationTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool runTestOnStart = false;
        [SerializeField] private bool logDetailedResults = true;

        private void Start()
        {
            if (runTestOnStart)
            {
                RunCompilationTest();
            }
        }

        [ContextMenu("Run Compilation Test")]
        public void RunCompilationTest()
        {
            Logger.Log("🧪 Starting compilation test...", "COMPILATION");

            bool allTestsPassed = true;

            // Test 1: Core assembly
            allTestsPassed &= TestCoreAssembly();

            // Test 2: Runtime assembly
            allTestsPassed &= TestRuntimeAssembly();

            // Test 3: Services assembly
            allTestsPassed &= TestServicesAssembly();

            // Test 4: Recording assembly (new)
            allTestsPassed &= TestRecordingAssembly();

            // Test 5: Testing assembly
            allTestsPassed &= TestTestingAssembly();

            // Test 6: AR Foundation integration
            allTestsPassed &= TestARFoundationIntegration();

            // Test 7: Cross-assembly references
            allTestsPassed &= TestCrossAssemblyReferences();

            // Final result
            if (allTestsPassed)
            {
                Logger.Log("✅ All compilation tests passed!", "COMPILATION");
            }
            else
            {
                Logger.LogError("❌ Some compilation tests failed!", "COMPILATION");
            }
        }

        private bool TestCoreAssembly()
        {
            try
            {
                // Test Logger functionality
                Logger.Log("Testing Core assembly", "TEST");
                
                if (logDetailedResults)
                    Logger.Log("✅ Core assembly test passed", "COMPILATION");
                return true;
            }
            catch (System.Exception e)
            {
                Logger.LogError($"❌ Core assembly test failed: {e.Message}", "COMPILATION");
                return false;
            }
        }

        private bool TestRuntimeAssembly()
        {
            try
            {
                // Test data models
                var poseData = new PoseData(Vector3.zero, Quaternion.identity, 0f);
                var bodyData = new BodyTrackingData(10);
                var climbData = new ClimbData();
                var boulderData = new BoulderData();
                var gymData = new GymData();
                
                // Test enums
                var gameState = GameState.Idle;
                var climbStatus = ClimbStatus.Uploading;
                
                if (logDetailedResults)
                    Logger.Log("✅ Runtime assembly test passed", "COMPILATION");
                return true;
            }
            catch (System.Exception e)
            {
                Logger.LogError($"❌ Runtime assembly test failed: {e.Message}", "COMPILATION");
                return false;
            }
        }

        private bool TestServicesAssembly()
        {
            try
            {
                // Test service access
                var arService = ARService.Instance;
                var firebaseService = FirebaseService.Instance;
                
                if (logDetailedResults)
                    Logger.Log("✅ Services assembly test passed", "COMPILATION");
                return true;
            }
            catch (System.Exception e)
            {
                Logger.LogError($"❌ Services assembly test failed: {e.Message}", "COMPILATION");
                return false;
            }
        }

        private bool TestRecordingAssembly()
        {
            try
            {
                // Test that we can reference Recording classes
                var controllerType = typeof(BodyTrackingController);
                var recorderType = typeof(BodyTrackingRecorder);
                var playerType = typeof(BodyTrackingPlayer);
                
                if (logDetailedResults)
                    Logger.Log("✅ Recording assembly test passed", "COMPILATION");
                return true;
            }
            catch (System.Exception e)
            {
                Logger.LogError($"❌ Recording assembly test failed: {e.Message}", "COMPILATION");
                return false;
            }
        }

        private bool TestTestingAssembly()
        {
            try
            {
                // Test testing utilities
                var visualizerType = typeof(ImageTrackingVisualizer);
                var testManagerType = typeof(ARRemoteTestManager);
                
                if (logDetailedResults)
                    Logger.Log("✅ Testing assembly test passed", "COMPILATION");
                return true;
            }
            catch (System.Exception e)
            {
                Logger.LogError($"❌ Testing assembly test failed: {e.Message}", "COMPILATION");
                return false;
            }
        }

        private bool TestARFoundationIntegration()
        {
            try
            {
                // Test AR Foundation types
                var sessionType = typeof(ARSession);
                var cameraManagerType = typeof(ARCameraManager);
                var trackedImageManagerType = typeof(ARTrackedImageManager);
                var humanBodyManagerType = typeof(ARHumanBodyManager);
                var xrOriginType = typeof(XROrigin);
                
                if (logDetailedResults)
                    Logger.Log("✅ AR Foundation integration test passed", "COMPILATION");
                return true;
            }
            catch (System.Exception e)
            {
                Logger.LogError($"❌ AR Foundation integration test failed: {e.Message}", "COMPILATION");
                return false;
            }
        }

        private bool TestCrossAssemblyReferences()
        {
            try
            {
                // Test that assemblies can reference each other correctly
                
                // Services can access Core and Runtime
                var arService = ARService.Instance;
                var poseData = new PoseData();
                
                // Recording can access Core and Runtime
                var bodyData = new BodyTrackingData(5);
                
                // Testing can access all assemblies
                var testManager = FindFirstObjectByType<ARRemoteTestManager>();
                var visualizer = FindFirstObjectByType<ImageTrackingVisualizer>();
                
                if (logDetailedResults)
                    Logger.Log("✅ Cross-assembly references test passed", "COMPILATION");
                return true;
            }
            catch (System.Exception e)
            {
                Logger.LogError($"❌ Cross-assembly references test failed: {e.Message}", "COMPILATION");
                return false;
            }
        }
    }
} 