using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TENDOR.Core;
using TENDOR.Services;
using TENDOR.Services.Firebase;
using TENDOR.Runtime.Models;
using Logger = TENDOR.Core.Logger;

namespace TENDOR.Tests.Editor
{
    /// <summary>
    /// Edit-mode unit tests for service logic
    /// </summary>
    public class ServiceTests
    {
        [SetUp]
        public void Setup()
        {
            // Reset logger for tests
            Logger.SetLogLevel(Logger.LogLevel.Debug);
        }

        [Test]
        public void Logger_FormatsMessagesCorrectly()
        {
            // Test logger message formatting
            LogAssert.Expect(LogType.Log, "ℹ️ [TEST] Test message");
            Logger.Log("Test message", "TEST");
        }

        [Test]
        public void Logger_FiltersLogLevels()
        {
            // Set minimum level to Warning
            Logger.SetLogLevel(Logger.LogLevel.Warning);
            
            // Debug and Info should be filtered out
            Logger.LogDebug("Debug message", "TEST");
            Logger.Log("Info message", "TEST");
            
            // Warning and Error should pass through
            LogAssert.Expect(LogType.Warning, "⚠️ [TEST] Warning message");
            Logger.LogWarning("Warning message", "TEST");
            
            LogAssert.Expect(LogType.Error, "❌ [TEST] Error message");
            Logger.LogError("Error message", "TEST");
        }

        [Test]
        public void PoseData_ConstructorSetsValues()
        {
            var position = new Vector3(1, 2, 3);
            var rotation = Quaternion.Euler(45, 90, 135);
            var timestamp = 123.456f;

            var pose = new PoseData(position, rotation, timestamp);

            Assert.AreEqual(position, pose.position);
            Assert.AreEqual(rotation, pose.rotation);
            Assert.AreEqual(timestamp, pose.timestamp);
        }

        [Test]
        public void PoseData_TransformConstructor()
        {
            var go = new GameObject("TestObject");
            go.transform.position = new Vector3(5, 10, 15);
            go.transform.rotation = Quaternion.Euler(30, 60, 90);

            var pose = new PoseData(go.transform);

            Assert.AreEqual(go.transform.position, pose.position);
            Assert.AreEqual(go.transform.rotation, pose.rotation);
            Assert.Greater(pose.timestamp, 0f);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void PoseData_ApplyToTransform()
        {
            var go = new GameObject("TestObject");
            var targetPosition = new Vector3(7, 8, 9);
            var targetRotation = Quaternion.Euler(15, 30, 45);
            
            var pose = new PoseData(targetPosition, targetRotation);
            pose.ApplyTo(go.transform);

            Assert.AreEqual(targetPosition, go.transform.position);
            Assert.AreEqual(targetRotation, go.transform.rotation);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void BodyTrackingData_InitializesCorrectly()
        {
            var data = new BodyTrackingData(100);

            Assert.IsNotNull(data.hipPoses);
            Assert.AreEqual(100, data.hipPoses.Length);
            Assert.AreEqual(0f, data.recordingDuration);
            Assert.IsNotEmpty(data.recordingId);
            Assert.Greater(data.recordingTimestamp.Ticks, 0);
        }

        [Test]
        public void BodyTrackingData_IsValidCheck()
        {
            var data = new BodyTrackingData(10);
            Assert.IsFalse(data.IsValid); // No duration set

            data.recordingDuration = 5.0f;
            Assert.IsTrue(data.IsValid);

            data.hipPoses = null;
            Assert.IsFalse(data.IsValid);
        }

        [Test]
        public void ClimbData_InitializesWithDefaults()
        {
            var climb = new ClimbData();

            Assert.IsNotEmpty(climb.id);
            Assert.AreEqual(string.Empty, climb.ownerUid);
            Assert.AreEqual(string.Empty, climb.boulderId);
            Assert.AreEqual(string.Empty, climb.videoUrl);
            Assert.IsNull(climb.fbxUrl);
            Assert.IsNull(climb.jsonUrl);
            Assert.AreEqual(ClimbStatus.Uploading, climb.status);
            Assert.Greater(climb.createdAt.Ticks, 0);
        }

        [Test]
        public void BoulderData_InitializesWithDefaults()
        {
            var boulder = new BoulderData();

            Assert.IsNotEmpty(boulder.id);
            Assert.IsNull(boulder.gymId);
            Assert.AreEqual(string.Empty, boulder.name);
            Assert.AreEqual(string.Empty, boulder.grade);
            Assert.AreEqual(string.Empty, boulder.targetUrl);
            Assert.AreEqual(1.0f, boulder.physicalWidthM);
            Assert.IsTrue(boulder.isActive);
            Assert.Greater(boulder.createdAt.Ticks, 0);
        }

        [Test]
        public void GeoPoint_ConstructorSetsValues()
        {
            var lat = 37.7749;
            var lng = -122.4194;
            var geoPoint = new GeoPoint(lat, lng);

            Assert.AreEqual(lat, geoPoint.latitude);
            Assert.AreEqual(lng, geoPoint.longitude);
        }

        [Test]
        public void GeoPoint_ZeroProperty()
        {
            var zero = GeoPoint.Zero;
            Assert.AreEqual(0.0, zero.latitude);
            Assert.AreEqual(0.0, zero.longitude);
        }

        [Test]
        public void GameState_EnumValues()
        {
            Assert.AreEqual(0, (int)GameState.Idle);
            Assert.AreEqual(1, (int)GameState.Recording);
            Assert.AreEqual(2, (int)GameState.Processing);
            Assert.AreEqual(3, (int)GameState.Playback);
        }

        [Test]
        public void ClimbStatus_EnumValues()
        {
            Assert.AreEqual(0, (int)ClimbStatus.Uploading);
            Assert.AreEqual(1, (int)ClimbStatus.Processing);
            Assert.AreEqual(2, (int)ClimbStatus.Ready);
            Assert.AreEqual(3, (int)ClimbStatus.Error);
        }
    }

    /// <summary>
    /// Async tests for Firebase service logic
    /// </summary>
    public class FirebaseServiceTests
    {
        private GameObject testGameObject;
        private FirebaseService firebaseService;

        [SetUp]
        public void Setup()
        {
            testGameObject = new GameObject("TestFirebaseService");
            firebaseService = testGameObject.AddComponent<FirebaseService>();
        }

        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(testGameObject);
            }
        }

        [Test]
        public void FirebaseService_SingletonPattern()
        {
            Assert.AreEqual(firebaseService, FirebaseService.Instance);
            
            // Creating another instance should destroy it
            var secondGameObject = new GameObject("SecondFirebaseService");
            var secondService = secondGameObject.AddComponent<FirebaseService>();
            
            // The second service should be destroyed, first one should remain
            Assert.AreEqual(firebaseService, FirebaseService.Instance);
            Assert.IsNull(secondService);
        }

        [Test]
        public void FirebaseService_IsInitialized()
        {
            // Service should initialize automatically
            Assert.IsTrue(firebaseService.IsInitialized);
        }

        [Test]
        public void FirebaseService_GetCurrentUserId()
        {
            var userId = firebaseService.GetCurrentUserId();
            Assert.IsNotEmpty(userId);
        }

        [Test]
        public void FirebaseService_IsAuthenticated()
        {
            var isAuth = firebaseService.IsAuthenticated();
            Assert.IsTrue(isAuth); // Placeholder returns true
        }

        [UnityTest]
        public IEnumerator FirebaseService_SignInAsync()
        {
            var task = firebaseService.SignInAsync("test@example.com", "password");
            yield return new WaitUntil(() => task.IsCompleted);
            
            Assert.IsTrue(task.Result);
        }

        [UnityTest]
        public IEnumerator FirebaseService_UploadVideoAsync()
        {
            var progress = new System.Progress<float>();
            var progressValues = new System.Collections.Generic.List<float>();
            progress.ProgressChanged += (sender, value) => progressValues.Add(value);

            var task = firebaseService.UploadVideoAsync("/fake/path.mp4", "test-climb-id", progress);
            yield return new WaitUntil(() => task.IsCompleted);
            
            Assert.IsNotNull(task.Result);
            Assert.IsTrue(task.Result.Contains("test-climb-id"));
            Assert.Greater(progressValues.Count, 0);
            Assert.AreEqual(1.0f, progressValues[progressValues.Count - 1]);
        }

        [UnityTest]
        public IEnumerator FirebaseService_CreateClimbAsync()
        {
            var climbData = new ClimbData
            {
                id = "test-climb",
                ownerUid = "test-user",
                boulderId = "test-boulder"
            };

            var task = firebaseService.CreateClimbAsync(climbData);
            yield return new WaitUntil(() => task.IsCompleted);
            
            Assert.IsTrue(task.Result);
        }

        [UnityTest]
        public IEnumerator FirebaseService_GetActiveBoulders()
        {
            var task = firebaseService.GetActiveBoulders();
            yield return new WaitUntil(() => task.IsCompleted);
            
            Assert.IsNotNull(task.Result);
            Assert.Greater(task.Result.Length, 0);
            Assert.AreEqual("wall-1", task.Result[0].id);
        }
    }
} 