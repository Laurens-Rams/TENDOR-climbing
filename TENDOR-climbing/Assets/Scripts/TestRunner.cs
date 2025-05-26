using UnityEngine;
using BodyTracking;
using BodyTracking.Animation;

/// <summary>
/// Simple test runner for validating the TENDOR system
/// </summary>
public class TestRunner : MonoBehaviour
{
    [Header("Test Settings")]
    public bool runTestsOnStart = false; // Disabled to reduce console spam
    public bool enableDetailedLogging = false; // Disabled to reduce console spam

    void Start()
    {
        // Disabled automatic testing to reduce console spam
        // Use TENDOR menu items for manual testing instead
    }

    private System.Collections.IEnumerator RunSystemTests()
    {
        yield return new WaitForSeconds(1f); // Wait for initialization

        Debug.Log("=== TENDOR SYSTEM TEST RUNNER ===");
        
        // Test 1: Check BodyTrackingController
        TestBodyTrackingController();
        yield return new WaitForSeconds(0.5f);

        // Test 2: Check FBXCharacterController
        TestFBXCharacterController();
        yield return new WaitForSeconds(0.5f);

        // Test 3: Test Animation System
        TestAnimationSystem();
        yield return new WaitForSeconds(0.5f);

        // Test 4: Check Scene Setup
        TestSceneSetup();

        Debug.Log("=== SYSTEM TESTS COMPLETE ===");
    }

    private void TestBodyTrackingController()
    {
        Debug.Log("--- Testing BodyTrackingController ---");
        
        var controller = FindObjectOfType<BodyTrackingController>();
        if (controller != null)
        {
            Debug.Log("✅ BodyTrackingController found");
            Debug.Log($"Initialized: {controller.IsInitialized}");
            Debug.Log($"Current Mode: {controller.CurrentMode}");
            Debug.Log($"Can Record: {controller.CanRecord}");
            Debug.Log($"Can Playback: {controller.CanPlayback}");
        }
        else
        {
            Debug.LogError("❌ BodyTrackingController not found");
        }
    }

    private void TestFBXCharacterController()
    {
        Debug.Log("--- Testing FBXCharacterController ---");
        
        var characterController = FindObjectOfType<FBXCharacterController>();
        if (characterController != null)
        {
            Debug.Log("✅ FBXCharacterController found");
            Debug.Log($"Initialized: {characterController.IsInitialized}");
            
            if (characterController.CharacterRootForEditor != null)
            {
                Debug.Log($"✅ Character Root: {characterController.CharacterRootForEditor.name}");
                Debug.Log($"Character Position: {characterController.CharacterRootForEditor.transform.position}");
                Debug.Log($"Character Active: {characterController.CharacterRootForEditor.activeInHierarchy}");
            }
            else
            {
                Debug.LogWarning("⚠️ Character Root not assigned");
            }

            if (characterController.AnimatorOverrideControllerForEditor != null)
            {
                Debug.Log($"✅ Override Controller: {characterController.AnimatorOverrideControllerForEditor.name}");
            }
            else
            {
                Debug.LogError("❌ Override Controller missing");
            }
        }
        else
        {
            Debug.LogError("❌ FBXCharacterController not found");
        }
    }

    private void TestAnimationSystem()
    {
        Debug.Log("--- Testing Animation System ---");
        
        var characterController = FindObjectOfType<FBXCharacterController>();
        if (characterController != null)
        {
            // Try to initialize
            if (!characterController.IsInitialized)
            {
                bool initialized = characterController.Initialize();
                Debug.Log($"Character initialization: {(initialized ? "SUCCESS" : "FAILED")}");
            }

            // Check animator
            if (characterController.CharacterAnimatorForEditor != null)
            {
                var animator = characterController.CharacterAnimatorForEditor;
                Debug.Log($"Animator enabled: {animator.enabled}");
                Debug.Log($"Animator speed: {animator.speed}");
                Debug.Log($"Runtime controller: {(animator.runtimeAnimatorController ? animator.runtimeAnimatorController.name : "None")}");
                
                if (animator.layerCount > 0)
                {
                    var state = animator.GetCurrentAnimatorStateInfo(0);
                    Debug.Log($"Current state length: {state.length:F2}s");
                    Debug.Log($"State normalized time: {state.normalizedTime:F3}");
                    
                    if (state.length > 5.0f)
                    {
                        Debug.Log("✅ Animation appears to be loaded correctly (length > 5s)");
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ Animation may not be loaded (short length)");
                    }
                }
            }
            else
            {
                Debug.LogError("❌ No animator found on character");
            }

            // Test animation playback
            bool animationStarted = characterController.StartAnimationPlayback();
            Debug.Log($"Animation playback test: {(animationStarted ? "SUCCESS" : "FAILED")}");
        }
    }

    private void TestSceneSetup()
    {
        Debug.Log("--- Testing Scene Setup ---");
        
        // Check AR components
        var arSessionOrigin = FindObjectOfType<UnityEngine.XR.ARFoundation.ARSessionOrigin>();
        if (arSessionOrigin != null)
        {
            Debug.Log("✅ AR Session Origin found");
            
            var humanBodyManager = arSessionOrigin.GetComponent<UnityEngine.XR.ARFoundation.ARHumanBodyManager>();
            Debug.Log($"AR Human Body Manager: {(humanBodyManager != null ? "✅ Found" : "❌ Missing")}");
            
            var trackedImageManager = arSessionOrigin.GetComponent<UnityEngine.XR.ARFoundation.ARTrackedImageManager>();
            Debug.Log($"AR Tracked Image Manager: {(trackedImageManager != null ? "✅ Found" : "❌ Missing")}");
        }
        else
        {
            Debug.LogError("❌ AR Session Origin not found");
        }

        // Check UI
        var canvas = FindObjectOfType<Canvas>();
        Debug.Log($"UI Canvas: {(canvas != null ? "✅ Found" : "❌ Missing")}");

        // Check NewBody character in scene
        var newBodyInScene = GameObject.Find("NewBody");
        if (newBodyInScene != null)
        {
            Debug.Log($"✅ NewBody found in scene at {newBodyInScene.transform.position}");
            Debug.Log($"NewBody active: {newBodyInScene.activeInHierarchy}");
            
            var renderers = newBodyInScene.GetComponentsInChildren<Renderer>();
            Debug.Log($"NewBody renderers: {renderers.Length}");
            
            int activeRenderers = 0;
            foreach (var r in renderers)
            {
                if (r.enabled && r.gameObject.activeInHierarchy)
                    activeRenderers++;
            }
            Debug.Log($"Active renderers: {activeRenderers}");
        }
        else
        {
            Debug.LogWarning("⚠️ NewBody not found in scene - will be auto-created");
        }
    }

    // Console command for manual testing
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void RunTests()
    {
        var testRunner = FindObjectOfType<TestRunner>();
        if (testRunner != null)
        {
            testRunner.StartCoroutine(testRunner.RunSystemTests());
        }
        else
        {
            Debug.LogError("TestRunner not found in scene");
        }
    }
} 