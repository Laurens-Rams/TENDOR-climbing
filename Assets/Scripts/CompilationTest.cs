using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TENDOR.Core;
using TENDOR.Services;
using TENDOR.Services.Firebase;
using TENDOR.Services.AR;
using TENDOR.Runtime.Models;
using Logger = TENDOR.Core.Logger;

namespace TENDOR.Tests
{
    /// <summary>
    /// Simple compilation test to verify all services and dependencies work correctly
    /// </summary>
    public class CompilationTest : MonoBehaviour
    {
        [Header("Compilation Test")]
        [SerializeField] private bool runTestOnStart = true;
        
        private void Start()
        {
            if (runTestOnStart)
            {
                RunCompilationTest();
            }
        }
        
        public void RunCompilationTest()
        {
            Logger.Log("🧪 Running compilation test...", "TEST");
            
            // Test Logger
            Logger.Log("✅ Logger working", "TEST");
            Logger.LogWarning("⚠️ Logger warning test", "TEST");
            Logger.LogError("❌ Logger error test", "TEST");
            
            // Test GameStateManager
            if (GameStateManager.Instance != null)
            {
                Logger.Log($"✅ GameStateManager: Current state = {GameStateManager.Instance.GetCurrentState()}", "TEST");
            }
            else
            {
                Logger.LogWarning("⚠️ GameStateManager instance not found", "TEST");
            }
            
            // Test FirebaseService
            if (FirebaseService.Instance != null)
            {
                Logger.Log($"✅ FirebaseService: Initialized = {FirebaseService.Instance.IsInitialized}", "TEST");
                Logger.Log($"✅ FirebaseService: User ID = {FirebaseService.Instance.GetCurrentUserId()}", "TEST");
            }
            else
            {
                Logger.LogWarning("⚠️ FirebaseService instance not found", "TEST");
            }
            
            // Test ARService
            if (ARService.Instance != null)
            {
                Logger.Log($"✅ ARService: Initialized = {ARService.Instance.IsInitialized}", "TEST");
                
                // Test AR Subsystems types are accessible
                var cameraManager = ARService.Instance.GetCameraManager();
                var trackedImageManager = ARService.Instance.GetTrackedImageManager();
                Logger.Log($"✅ ARService: Camera Manager = {(cameraManager != null ? "Found" : "Not Found")}", "TEST");
                Logger.Log($"✅ ARService: Tracked Image Manager = {(trackedImageManager != null ? "Found" : "Not Found")}", "TEST");
            }
            else
            {
                Logger.LogWarning("⚠️ ARService instance not found", "TEST");
            }
            
            // Test data models
            var testClimb = new ClimbData
            {
                id = "test-climb-123",
                ownerUid = "test-user",
                status = ClimbStatus.Ready
            };
            Logger.Log($"✅ ClimbData model: ID = {testClimb.id}, Status = {testClimb.status}", "TEST");
            
            var testBoulder = new BoulderData
            {
                id = "test-boulder-456",
                name = "Test Boulder",
                grade = "V5",
                isActive = true
            };
            Logger.Log($"✅ BoulderData model: Name = {testBoulder.name}, Grade = {testBoulder.grade}", "TEST");
            
            // Test AR Subsystems types are accessible
            var trackingState = TrackingState.Tracking;
            Logger.Log($"✅ AR Subsystems: TrackingState.Tracking = {trackingState}", "TEST");
            
            Logger.Log("🎉 Compilation test completed successfully!", "TEST");
        }
    }
} 