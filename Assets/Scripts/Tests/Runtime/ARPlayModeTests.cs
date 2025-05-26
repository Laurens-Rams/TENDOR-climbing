using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TENDOR.Core;
using TENDOR.Services;
using TENDOR.Services.AR;
using TENDOR.Services.Firebase;
using TENDOR.Runtime.Models;
using Logger = TENDOR.Core.Logger;

namespace TENDOR.Tests.Runtime
{
    /// <summary>
    /// Play-mode tests that verify AR functionality and recording workflows
    /// </summary>
    public class ARPlayModeTests
    {
        private GameObject testARService;
        private GameObject testFirebaseService;
        private GameObject testGameStateManager;

        [SetUp]
        public void Setup()
        {
            // Create test services
            testFirebaseService = new GameObject("TestFirebaseService");
            testFirebaseService.AddComponent<FirebaseService>();

            testARService = new GameObject("TestARService");
            testARService.AddComponent<ARService>();

            testGameStateManager = new GameObject("TestGameStateManager");
            testGameStateManager.AddComponent<GameStateManager>();

            // Set up basic XR Origin for AR tests
            SetupBasicXROrigin();
        }

        [TearDown]
        public void TearDown()
        {
            if (testARService != null)
                Object.DestroyImmediate(testARService);
            if (testFirebaseService != null)
                Object.DestroyImmediate(testFirebaseService);
            if (testGameStateManager != null)
                Object.DestroyImmediate(testGameStateManager);

            // Clean up XR Origin
            var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
                Object.DestroyImmediate(xrOrigin.gameObject);
        }

        private void SetupBasicXROrigin()
        {
            // Create basic XR Origin if none exists
            var existingOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (existingOrigin == null)
            {
                var xrOriginGO = new GameObject("XR Origin");
                var xrOrigin = xrOriginGO.AddComponent<Unity.XR.CoreUtils.XROrigin>();
                
                // Create camera offset
                var cameraOffsetGO = new GameObject("Camera Offset");
                cameraOffsetGO.transform.SetParent(xrOriginGO.transform);
                
                // Create main camera
                var mainCameraGO = new GameObject("Main Camera");
                mainCameraGO.transform.SetParent(cameraOffsetGO.transform);
                var camera = mainCameraGO.AddComponent<Camera>();
                
                // Set up XR Origin references
                xrOrigin.CameraFloorOffsetObject = cameraOffsetGO;
                xrOrigin.Camera = camera;
            }
        }

        [UnityTest]
        public IEnumerator ARService_InitializesCorrectly()
        {
            // Wait a frame for initialization
            yield return null;

            var arService = ARService.Instance;
            Assert.IsNotNull(arService);
            Assert.IsTrue(arService.IsInitialized);
        }

        [UnityTest]
        public IEnumerator ARService_FindsARComponents()
        {
            yield return null;

            var arService = ARService.Instance;
            
            Assert.IsNotNull(arService.GetCameraManager());
            Assert.IsNotNull(arService.GetTrackedImageManager());
            Assert.IsNotNull(arService.GetHumanBodyManager());
        }

        [UnityTest]
        public IEnumerator RuntimeImageLibrary_LoadsTestTexture()
        {
            yield return null;

            var arService = ARService.Instance;
            var trackedImageManager = arService.GetTrackedImageManager();
            
            // Create a test texture
            var testTexture = new Texture2D(256, 256, TextureFormat.RGB24, false);
            var pixels = new Color[256 * 256];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }
            testTexture.SetPixels(pixels);
            testTexture.Apply();

            // Test that we can create a runtime library
            var runtimeLibrary = trackedImageManager.CreateRuntimeLibrary();
            Assert.IsNotNull(runtimeLibrary);

            // Test adding an image (this verifies the runtime library functionality)
            if (runtimeLibrary != null)
            {
                var addImageJob = runtimeLibrary.ScheduleAddImageWithValidationJob(
                    testTexture, 
                    "test-image", 
                    1.0f
                );

                // Wait for job completion
                while (!addImageJob.jobHandle.IsCompleted)
                {
                    yield return null;
                }

                // Complete the job
                addImageJob.jobHandle.Complete();

                // Verify the image was added successfully
                Assert.AreEqual(UnityEngine.XR.ARSubsystems.AddReferenceImageJobStatus.Success, addImageJob.status);
            }

            Object.DestroyImmediate(testTexture);
        }

        [UnityTest]
        public IEnumerator ARService_LoadsImageLibraryFromFirebase()
        {
            yield return null;

            var arService = ARService.Instance;
            bool libraryLoaded = false;

            // Subscribe to library loaded event
            arService.OnImageLibraryLoaded += () => libraryLoaded = true;

            // Trigger library loading
            arService.LoadImageLibraryAsync();

            // Wait for library to load (with timeout)
            float timeout = 10f;
            float elapsed = 0f;
            while (!libraryLoaded && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            Assert.IsTrue(libraryLoaded, "Image library should have loaded within timeout");
        }

        [UnityTest]
        public IEnumerator GameStateManager_InitializesCorrectly()
        {
            yield return null;

            var stateManager = GameStateManager.Instance;
            Assert.IsNotNull(stateManager);
            Assert.AreEqual(GameState.Idle, stateManager.GetCurrentState());
        }

        [UnityTest]
        public IEnumerator GameStateManager_StateTransitions()
        {
            yield return null;

            var stateManager = GameStateManager.Instance;
            bool stateChanged = false;
            GameState newState = GameState.Idle;

            // Subscribe to state change events
            stateManager.OnStateChanged += (oldState, state) => {
                stateChanged = true;
                newState = state;
            };

            // Test transition to Recording
            stateManager.StartRecording();
            yield return null;

            Assert.IsTrue(stateChanged);
            Assert.AreEqual(GameState.Recording, newState);
            Assert.AreEqual(GameState.Recording, stateManager.GetCurrentState());

            // Reset for next test
            stateChanged = false;

            // Test transition to Processing
            stateManager.StopRecording("/fake/video/path.mp4");
            yield return null;

            Assert.IsTrue(stateChanged);
            Assert.AreEqual(GameState.Processing, newState);
            Assert.AreEqual(GameState.Processing, stateManager.GetCurrentState());
        }

        [UnityTest]
        public IEnumerator RecordingToUploadCoroutine_SignalsCompletion()
        {
            yield return null;

            var stateManager = GameStateManager.Instance;
            var firebaseService = FirebaseService.Instance;

            bool recordingStarted = false;
            bool recordingCompleted = false;
            bool processingStarted = false;
            string recordingId = null;

            // Subscribe to events
            stateManager.OnRecordingStarted += (id) => {
                recordingStarted = true;
                recordingId = id;
            };
            stateManager.OnRecordingCompleted += (id) => {
                recordingCompleted = true;
            };
            stateManager.OnProcessingStarted += (climb) => {
                processingStarted = true;
            };

            // Start recording
            stateManager.StartRecording();
            yield return null;

            Assert.IsTrue(recordingStarted);
            Assert.IsNotNull(recordingId);

            // Simulate recording completion
            stateManager.StopRecording("/fake/video/path.mp4");
            yield return null;

            Assert.IsTrue(recordingCompleted);
            Assert.IsTrue(processingStarted);

            // Test upload process
            bool uploadCompleted = false;
            string uploadResult = null;

            firebaseService.OnUploadComplete += (climbId, success, result) => {
                uploadCompleted = true;
                uploadResult = result;
            };

            // Simulate upload
            var uploadTask = firebaseService.UploadVideoAsync("/fake/video/path.mp4", recordingId);
            
            // Wait for upload completion
            while (!uploadTask.IsCompleted)
            {
                yield return null;
            }

            Assert.IsTrue(uploadCompleted);
            Assert.IsNotNull(uploadResult);
            Assert.IsTrue(uploadResult.Contains(recordingId));
        }

        [UnityTest]
        public IEnumerator ARTrackedImageManager_DiscoversTestTexture()
        {
            yield return null;

            var arService = ARService.Instance;
            var trackedImageManager = arService.GetTrackedImageManager();

            // Create and add a test texture to the runtime library
            var testTexture = new Texture2D(128, 128, TextureFormat.RGB24, false);
            
            // Create a simple pattern
            var pixels = new Color[128 * 128];
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    // Create a checkerboard pattern
                    bool isWhite = ((x / 16) + (y / 16)) % 2 == 0;
                    pixels[y * 128 + x] = isWhite ? Color.white : Color.black;
                }
            }
            testTexture.SetPixels(pixels);
            testTexture.Apply();

            // Add to runtime library
            var runtimeLibrary = trackedImageManager.CreateRuntimeLibrary();
            if (runtimeLibrary != null)
            {
                var addImageJob = runtimeLibrary.ScheduleAddImageWithValidationJob(
                    testTexture,
                    "test-pattern",
                    0.5f
                );

                // Wait for job completion
                while (!addImageJob.jobHandle.IsCompleted)
                {
                    yield return null;
                }

                addImageJob.jobHandle.Complete();

                // Update the tracked image manager
                trackedImageManager.referenceLibrary = runtimeLibrary;

                // Verify the library has our image
                Assert.IsNotNull(trackedImageManager.referenceLibrary);
                Assert.Greater(trackedImageManager.referenceLibrary.count, 0);
            }

            Object.DestroyImmediate(testTexture);
        }

        [UnityTest]
        public IEnumerator Logger_WorksInPlayMode()
        {
            yield return null;

            // Test that logger works correctly in play mode
            LogAssert.Expect(LogType.Log, "ℹ️ [PLAYMODE] Test log message");
            Logger.Log("Test log message", "PLAYMODE");

            LogAssert.Expect(LogType.Warning, "⚠️ [PLAYMODE] Test warning message");
            Logger.LogWarning("Test warning message", "PLAYMODE");

            LogAssert.Expect(LogType.Error, "❌ [PLAYMODE] Test error message");
            Logger.LogError("Test error message", "PLAYMODE");
        }

        [UnityTest]
        public IEnumerator FullWorkflow_RecordingToPlayback()
        {
            yield return null;

            var stateManager = GameStateManager.Instance;
            
            // Track workflow completion
            bool workflowCompleted = false;
            
            stateManager.OnPlaybackStarted += (climb) => {
                workflowCompleted = true;
            };

            // Start the full workflow
            stateManager.StartRecording();
            yield return new WaitForSeconds(0.1f);

            stateManager.StopRecording("/fake/video/path.mp4");
            yield return new WaitForSeconds(0.1f);

            // Simulate processing completion
            stateManager.CompleteProcessing("fake-fbx-url", "fake-json-url");
            yield return new WaitForSeconds(0.1f);

            Assert.IsTrue(workflowCompleted);
            Assert.AreEqual(GameState.Playback, stateManager.GetCurrentState());

            // Verify climb data
            var currentClimb = stateManager.GetCurrentClimb();
            Assert.IsNotNull(currentClimb);
            Assert.AreEqual("fake-fbx-url", currentClimb.fbxUrl);
            Assert.AreEqual("fake-json-url", currentClimb.jsonUrl);
            Assert.AreEqual(TENDOR.Runtime.Models.ClimbStatus.Ready, currentClimb.status);
        }
    }
} 