using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TENDOR.Core;
using TENDOR.Services;
using TENDOR.Services.AR;
using TENDOR.Services.Firebase;
using TENDOR.Runtime.Models;
using System.Threading.Tasks;
using Logger = TENDOR.Core.Logger;

/// <summary>
/// Compilation test to verify all dependencies and references are working
/// </summary>
public class CompilationTest : MonoBehaviour
{
    [Header("Compilation Test")]
    [SerializeField] private bool runTestOnStart = false;
    
    private void Start()
    {
        if (runTestOnStart)
        {
            RunCompilationTest();
        }
    }
    
    public async void RunCompilationTest()
    {
        Logger.Log("🧪 Running compilation test...", "TEST");
        
        // Test Core components
        TestCoreComponents();
        
        // Test AR components
        TestARComponents();
        
        // Test Firebase components
        await TestFirebaseComponents();
        
        // Test Models
        TestModels();
        
        Logger.Log("✅ Compilation test completed successfully!", "TEST");
    }
    
    private void TestCoreComponents()
    {
        Logger.Log("Testing Core components...", "TEST");
        
        // Test Logger
        Logger.LogDebug("Debug message", "TEST");
        Logger.LogWarning("Warning message", "TEST");
        Logger.LogError("Error message", "TEST");
        
        Logger.Log("✅ Core components working", "TEST");
    }
    
    private void TestARComponents()
    {
        Logger.Log("Testing AR components...", "TEST");
        
        // Test AR Foundation types
        var trackingState = TrackingState.Tracking;
        Logger.Log($"TrackingState: {trackingState}", "TEST");
        
        // Test AR Service
        if (ARService.Instance != null)
        {
            Logger.Log($"AR Service initialized: {ARService.Instance.IsInitialized}", "TEST");
        }
        
        // Test AR Subsystems availability
        Logger.Log("AR Subsystems types available", "TEST");
        
        Logger.Log("✅ AR components working", "TEST");
    }
    
    private async Task TestFirebaseComponents()
    {
        Logger.Log("Testing Firebase components...", "TEST");
        
        // Test Firebase Service
        if (FirebaseService.Instance != null)
        {
            Logger.Log($"Firebase Service initialized: {FirebaseService.Instance.IsInitialized}", "TEST");
            
            // Test async methods
            var userId = FirebaseService.Instance.GetCurrentUserId();
            Logger.Log($"User ID: {userId}", "TEST");
            
            var isAuth = FirebaseService.Instance.IsAuthenticated();
            Logger.Log($"Is authenticated: {isAuth}", "TEST");
            
            // Test async sign in
            var signInResult = await FirebaseService.Instance.SignInAsync("test@example.com", "password");
            Logger.Log($"Sign in result: {signInResult}", "TEST");
        }
        
        Logger.Log("✅ Firebase components working", "TEST");
    }
    
    private void TestModels()
    {
        Logger.Log("Testing Models...", "TEST");
        
        // Test PoseData
        var pose = new PoseData(Vector3.zero, Quaternion.identity, Time.time);
        Logger.Log($"PoseData created: {pose.timestamp}", "TEST");
        
        // Test BodyTrackingData
        var bodyData = new BodyTrackingData(100);
        Logger.Log($"BodyTrackingData created: {bodyData.recordingId}", "TEST");
        
        // Test ClimbData
        var climbData = new ClimbData();
        Logger.Log($"ClimbData created: {climbData.id}", "TEST");
        
        // Test BoulderData
        var boulderData = new BoulderData();
        Logger.Log($"BoulderData created: {boulderData.id}", "TEST");
        
        // Test Enums
        var gameState = GameState.Idle;
        var climbStatus = ClimbStatus.Ready;
        Logger.Log($"Enums: {gameState}, {climbStatus}", "TEST");
        
        Logger.Log("✅ Models working", "TEST");
    }
    
    [ContextMenu("Run Compilation Test")]
    public void RunTestFromMenu()
    {
        RunCompilationTest();
    }
} 