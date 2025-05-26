using UnityEngine;
using UnityEditor;
using BodyTracking;
using BodyTracking.Animation;
using BodyTracking.AR;
using BodyTracking.Recording;
using BodyTracking.Playback;

namespace BodyTracking.Editor
{
    /// <summary>
    /// Automatic scene fixer for TENDOR system
    /// </summary>
    public class SceneFixer : EditorWindow
    {
        [MenuItem("TENDOR/Fix Scene Automatically")]
        public static void FixSceneAutomatically()
        {
            Debug.Log("=== AUTOMATIC SCENE FIXER ===");
            
            // 1. Find or create BodyTrackingSystem GameObject
            GameObject bodyTrackingSystem = GameObject.Find("BodyTrackingSystem");
            if (bodyTrackingSystem == null)
            {
                bodyTrackingSystem = new GameObject("BodyTrackingSystem");
                Debug.Log("✅ Created BodyTrackingSystem GameObject");
            }
            else
            {
                Debug.Log("✅ Found existing BodyTrackingSystem GameObject");
            }

            // 2. Add BodyTrackingController if missing
            var bodyController = bodyTrackingSystem.GetComponent<BodyTrackingController>();
            if (bodyController == null)
            {
                bodyController = bodyTrackingSystem.AddComponent<BodyTrackingController>();
                Debug.Log("✅ Added BodyTrackingController component");
            }

            // 3. Add ARImageTargetManager if missing
            var imageTargetManager = bodyTrackingSystem.GetComponent<ARImageTargetManager>();
            if (imageTargetManager == null)
            {
                imageTargetManager = bodyTrackingSystem.AddComponent<ARImageTargetManager>();
                Debug.Log("✅ Added ARImageTargetManager component");
            }

            // 4. Add BodyTrackingRecorder if missing
            var recorder = bodyTrackingSystem.GetComponent<BodyTrackingRecorder>();
            if (recorder == null)
            {
                recorder = bodyTrackingSystem.AddComponent<BodyTrackingRecorder>();
                Debug.Log("✅ Added BodyTrackingRecorder component");
            }

            // 5. Add BodyTrackingPlayer if missing
            var player = bodyTrackingSystem.GetComponent<BodyTrackingPlayer>();
            if (player == null)
            {
                player = bodyTrackingSystem.AddComponent<BodyTrackingPlayer>();
                Debug.Log("✅ Added BodyTrackingPlayer component");
            }

            // 6. Find or create CharacterController GameObject
            GameObject characterControllerObj = GameObject.Find("CharacterController");
            if (characterControllerObj == null)
            {
                characterControllerObj = new GameObject("CharacterController");
                Debug.Log("✅ Created CharacterController GameObject");
            }

            // 7. Add FBXCharacterController if missing
            var fbxController = characterControllerObj.GetComponent<FBXCharacterController>();
            if (fbxController == null)
            {
                fbxController = characterControllerObj.AddComponent<FBXCharacterController>();
                Debug.Log("✅ Added FBXCharacterController component");
            }

            // 8. Set up component references
            bodyController.imageTargetManager = imageTargetManager;
            bodyController.recorder = recorder;
            bodyController.player = player;
            
            Debug.Log("✅ Connected all component references");

            // 9. Load and assign animation controller
            var overrideController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(
                "Assets/Scripts/Animation/Controllers/CharacterAnimationOverride.overrideController");
            
            if (overrideController != null)
            {
                Debug.Log("✅ Found animation override controller");
            }

            // 10. Create NewBody character if missing
            CreateNewBodyCharacter(fbxController);

            // 11. Mark scene as dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("=== SCENE FIXING COMPLETE ===");
            Debug.Log("Scene has been automatically fixed! Save the scene to persist changes.");
        }

        private static void CreateNewBodyCharacter(FBXCharacterController fbxController)
        {
            // Check if NewBody already exists
            GameObject existingNewBody = GameObject.Find("NewBody");
            if (existingNewBody != null)
            {
                Debug.Log("✅ NewBody character already exists in scene");
                return;
            }

            // Load NewBody prefab
            GameObject newBodyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/NewBody.fbx");
            if (newBodyPrefab != null)
            {
                GameObject newBodyInstance = PrefabUtility.InstantiatePrefab(newBodyPrefab) as GameObject;
                newBodyInstance.name = "NewBody";
                newBodyInstance.transform.position = Vector3.zero;
                
                Debug.Log("✅ Created NewBody character instance");
                Debug.Log($"NewBody position: {newBodyInstance.transform.position}");
            }
            else
            {
                Debug.LogError("❌ Could not find NewBody.fbx prefab");
            }
        }

        [MenuItem("TENDOR/Validate Scene After Fix")]
        public static void ValidateSceneAfterFix()
        {
            Debug.Log("=== POST-FIX VALIDATION ===");
            
            var bodyController = FindFirstObjectByType<BodyTrackingController>();
            var fbxController = FindFirstObjectByType<FBXCharacterController>();
            var newBody = GameObject.Find("NewBody");

            if (bodyController != null)
                Debug.Log("✅ BodyTrackingController found");
            else
                Debug.LogError("❌ BodyTrackingController still missing");

            if (fbxController != null)
                Debug.Log("✅ FBXCharacterController found");
            else
                Debug.LogError("❌ FBXCharacterController still missing");

            if (newBody != null)
                Debug.Log("✅ NewBody character found");
            else
                Debug.LogWarning("⚠️ NewBody character still missing");

            Debug.Log("=== POST-FIX VALIDATION COMPLETE ===");
        }
    }
} 