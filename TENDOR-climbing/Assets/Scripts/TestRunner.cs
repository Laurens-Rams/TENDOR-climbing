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

    [ContextMenu("Test Body Tracking Controller")]
    public void TestBodyTrackingController()
    {
        var controller = FindFirstObjectByType<BodyTrackingController>();
        if (controller != null)
        {
            Debug.Log($"[TestRunner] Found BodyTrackingController: {controller.name}");
            Debug.Log($"[TestRunner] Current mode: {controller.CurrentMode}");
            Debug.Log($"[TestRunner] Can record: {controller.CanRecord}");
            Debug.Log($"[TestRunner] Can playback: {controller.CanPlayback}");
            Debug.Log($"[TestRunner] Is recording: {controller.IsRecording}");
            Debug.Log($"[TestRunner] Is playing: {controller.IsPlaying}");
        }
        else
        {
            Debug.LogError("[TestRunner] No BodyTrackingController found in scene");
        }
    }

    [ContextMenu("Test Character Controller")]
    public void TestCharacterController()
    {
        var characterController = FindFirstObjectByType<FBXCharacterController>();
        if (characterController != null)
        {
            Debug.Log($"[TestRunner] Found FBXCharacterController on: {characterController.name}");
            Debug.Log($"[TestRunner] Character info:\n{characterController.GetCharacterInfo()}");
            
            // Test making character visible
            characterController.MakeCharacterVisibleForTesting();
            Debug.Log("[TestRunner] Made character visible for testing");
        }
        else
        {
            Debug.LogError("[TestRunner] No FBXCharacterController found in scene");
        }
    }

    [ContextMenu("Test Character Animation")]
    public void TestCharacterAnimation()
    {
        var characterController = FindFirstObjectByType<FBXCharacterController>();
        if (characterController != null)
        {
            Debug.Log($"[TestRunner] Testing animation on: {characterController.name}");
            
            // Try to start animation playback
            bool success = characterController.StartAnimationPlayback();
            if (success)
            {
                Debug.Log("[TestRunner] ✅ Animation playback started successfully");
            }
            else
            {
                Debug.LogError("[TestRunner] ❌ Failed to start animation playback");
            }
        }
        else
        {
            Debug.LogError("[TestRunner] No FBXCharacterController found in scene");
        }
    }

    [ContextMenu("Test AR Session")]
    public void TestARSession()
    {
        // Use XROrigin instead of deprecated ARSessionOrigin
        var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin != null)
        {
            Debug.Log($"[TestRunner] Found XROrigin: {xrOrigin.name}");
            Debug.Log($"[TestRunner] XROrigin position: {xrOrigin.transform.position}");
            Debug.Log($"[TestRunner] XROrigin rotation: {xrOrigin.transform.rotation}");
        }
        else
        {
            Debug.LogError("[TestRunner] No XROrigin found in scene");
        }
    }

    [ContextMenu("Test UI Canvas")]
    public void TestUICanvas()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"[TestRunner] Found Canvas: {canvas.name}");
            Debug.Log($"[TestRunner] Canvas render mode: {canvas.renderMode}");
            Debug.Log($"[TestRunner] Canvas world camera: {canvas.worldCamera}");
            
            // Find UI components
            var buttons = canvas.GetComponentsInChildren<UnityEngine.UI.Button>();
            var texts = canvas.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            
            Debug.Log($"[TestRunner] Found {buttons.Length} buttons and {texts.Length} text components");
        }
        else
        {
            Debug.LogError("[TestRunner] No Canvas found in scene");
        }
    }

    [ContextMenu("Run All Tests")]
    public void RunAllTests()
    {
        var testRunner = FindFirstObjectByType<TestRunner>();
        if (testRunner != null)
        {
            Debug.Log("[TestRunner] === RUNNING ALL TESTS ===");
            testRunner.TestBodyTrackingController();
            testRunner.TestCharacterController();
            testRunner.TestCharacterAnimation();
            testRunner.TestARSession();
            testRunner.TestUICanvas();
            Debug.Log("[TestRunner] === ALL TESTS COMPLETED ===");
        }
        else
        {
            Debug.LogError("[TestRunner] No TestRunner found in scene");
        }
    }

    private void TestFBXCharacterController()
    {
        Debug.Log("--- Testing FBXCharacterController ---");
        
        var characterController = FindFirstObjectByType<FBXCharacterController>();
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
        
        var characterController = FindFirstObjectByType<FBXCharacterController>();
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
        var arSessionOrigin = FindFirstObjectByType<UnityEngine.XR.ARFoundation.ARSessionOrigin>();
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
        var canvas = FindFirstObjectByType<Canvas>();
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
        var testRunner = FindFirstObjectByType<TestRunner>();
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