using UnityEngine;
using BodyTracking.Data;
using System.Collections.Generic;

namespace BodyTracking.Animation
{
    /// <summary>
    /// Controls FBX character positioning and animation alignment with ARKit hip tracking data
    /// </summary>
    public class FBXCharacterController : MonoBehaviour
    {
        [Header("Character Setup")]
        [SerializeField] private GameObject characterRoot;
        [SerializeField] private Transform hipBone;
        
        [Header("Alignment Settings")]
        [SerializeField] private bool autoFindHipBone = true;
        [SerializeField] private string[] hipBoneNames = { "Hips Node", "Hips", "Hip", "Pelvis", "mixamorig:Hips", "Root" };
        [SerializeField] private bool showDebugSphere = true;
        [SerializeField] private bool adjustCharacterHeight = true;
        [SerializeField] private float groundOffset = 0.0f; // How much above ground to place character
        
        [Header("Debug")]
        [SerializeField] private bool enableLogging = false; // Disabled to reduce console spam
        
        [Header("Animation Management")]
        [SerializeField] private AnimatorOverrideController animatorOverrideController;
        [SerializeField] private string defaultAnimationClipName = ""; // Will auto-detect from NewAnimationOnly.fbx
        
        // State
        private bool isInitialized = false;
        private Vector3 targetHipPosition;
        private bool hasValidTarget = false;
        
        // Debug visualization
        private GameObject debugSphere;
        
        // Animation state
        private Animator characterAnimator;
        private AnimatorOverrideController runtimeOverrideController;
        
        // Events
        public event System.Action<Vector3> OnHipPositionUpdated;
        
        // Public properties
        public bool IsInitialized => isInitialized;
        public Vector3 CurrentHipPosition => hipBone != null ? hipBone.position : Vector3.zero;
        public GameObject CharacterRoot => characterRoot;

        // Public properties for editor access
        public GameObject CharacterRootForEditor => characterRoot;
        public Animator CharacterAnimatorForEditor => characterAnimator;
        public AnimatorOverrideController AnimatorOverrideControllerForEditor => animatorOverrideController;

        void Start()
        {
            Initialize();
        }

        /// <summary>
        /// Initialize the character controller
        /// </summary>
        public bool Initialize()
        {
            if (isInitialized)
            {
                if (enableLogging) Debug.Log("[FBXCharacterController] Already initialized");
                return true;
            }
            
            // Find character if not assigned
            if (characterRoot == null)
            {
                characterRoot = FindCharacterInScene();
            }
            
            if (characterRoot == null)
            {
                Debug.LogError("[FBXCharacterController] No character found! Please assign 'Character Root' or ensure 'Newbody' exists in scene");
                return false;
            }
            
            // Find hip bone
            if (hipBone == null && autoFindHipBone)
            {
                hipBone = FindHipBone();
            }
            
            if (hipBone == null)
            {
                Debug.LogError($"[FBXCharacterController] Hip bone not found in character '{characterRoot.name}'. Please assign manually or check bone names.");
                return false;
            }
            
            // Create debug visualization
            if (showDebugSphere)
            {
                CreateDebugVisualization();
            }
            
            isInitialized = true;
            
            // Ensure character uses normal materials (not debug cyan)
            RestoreNormalMaterials();
            
            // Initialize animation system
            InitializeAnimationSystem();
            
            if (enableLogging)
            {
                Debug.Log($"[FBXCharacterController] Initialized character '{characterRoot.name}' with hip bone '{hipBone.name}'");
                Debug.Log($"[FBXCharacterController] Initial hip position: {hipBone.position:F3}");
            }
            
            return true;
        }

        /// <summary>
        /// Set the target hip position from ARKit tracking
        /// </summary>
        public void SetTargetHipPosition(Vector3 worldPosition)
        {
            targetHipPosition = worldPosition;
            hasValidTarget = true;
            
            if (isInitialized)
            {
                UpdateCharacterPosition();
            }
        }

        /// <summary>
        /// Update character position to align hip with target
        /// </summary>
        private void UpdateCharacterPosition()
        {
            if (!hasValidTarget || hipBone == null || characterRoot == null) return;
            
            // Calculate offset needed to move hip to target position
            Vector3 currentHipWorld = hipBone.position;
            Vector3 targetPosition = targetHipPosition;
            
            // Remove height adjustment - use target position directly
            Vector3 offset = targetPosition - currentHipWorld;
            
            // Move the entire character root
            characterRoot.transform.position += offset;
            
            // Update debug visualization to show actual hip target
            if (debugSphere != null)
            {
                debugSphere.transform.position = targetHipPosition; // Show original target, not adjusted
                debugSphere.SetActive(true);
            }
            
            // Notify listeners
            OnHipPositionUpdated?.Invoke(targetHipPosition);
            
            // Removed frequent logging to reduce console spam
        }

        /// <summary>
        /// Get the bounds of the entire character mesh
        /// </summary>
        private Bounds GetCharacterBounds()
        {
            if (characterRoot == null) return new Bounds();
            
            Renderer[] renderers = characterRoot.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds();
            
            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
            {
                if (renderer.enabled && renderer.gameObject.activeInHierarchy)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
            
            return bounds;
        }

        /// <summary>
        /// Log basic character debug information
        /// </summary>
        private void LogCharacterDebugInfo()
        {
            if (characterRoot == null) return;
            
            if (enableLogging)
            {
                Debug.Log($"[FBXCharacterController] Character '{characterRoot.name}' at {characterRoot.transform.position:F3}");
                
                // Check if character is visible to camera
                Camera arCamera = Camera.main ?? FindObjectOfType<Camera>();
                if (arCamera != null)
                {
                    Vector3 relativePos = arCamera.transform.InverseTransformPoint(characterRoot.transform.position);
                    bool inFront = relativePos.z > 0; // Z positive means in front of camera
                    float distance = Vector3.Distance(characterRoot.transform.position, arCamera.transform.position);
                    
                    Debug.Log($"[FBXCharacterController] Distance from camera: {distance:F1}m, In front: {inFront}");
                }
            }
        }

        /// <summary>
        /// Find character GameObject in scene by name
        /// </summary>
        private GameObject FindCharacterInScene()
        {
            // First try to find "Newbody" in scene (including inactive objects)
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "NewBody" || obj.name == "Newbody")
                {
                    // Prefer scene objects over prefab assets
                    if (obj.scene.IsValid()) // This means it's in a scene, not an asset
                    {
                        if (enableLogging) Debug.Log($"[FBXCharacterController] Found scene GameObject '{obj.name}' (active: {obj.activeInHierarchy})");
                        return obj;
                    }
                }
            }
            
            // If no valid scene object found, try to instantiate from prefab
            if (enableLogging) Debug.Log("[FBXCharacterController] No scene character found, attempting to instantiate from prefab...");
            return InstantiateCharacterFromPrefab();
        }

        /// <summary>
        /// Instantiate character from prefab asset into the scene
        /// </summary>
        private GameObject InstantiateCharacterFromPrefab()
        {
            #if UNITY_EDITOR
            // Find the prefab asset by searching for it in the project
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:GameObject NewBody");
            GameObject prefabAsset = null;
            
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                GameObject candidate = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (candidate != null && (candidate.name == "NewBody" || candidate.name == "Newbody"))
                {
                    prefabAsset = candidate;
                    if (enableLogging) Debug.Log($"[FBXCharacterController] Found prefab asset at: {path}");
                    break;
                }
            }
            
            if (prefabAsset != null)
            {
                // Instantiate a proper scene copy
                GameObject newSceneCharacter = GameObject.Instantiate(prefabAsset);
                newSceneCharacter.name = prefabAsset.name; // Remove "(Clone)" from name
                
                // Position it at origin initially
                newSceneCharacter.transform.position = Vector3.zero;
                newSceneCharacter.transform.rotation = Quaternion.identity;
                newSceneCharacter.transform.localScale = Vector3.one;
                
                if (enableLogging) 
                {
                    Debug.Log($"[FBXCharacterController] Successfully instantiated character '{newSceneCharacter.name}' in scene '{newSceneCharacter.scene.name}'");
                }
                
                return newSceneCharacter;
            }
            else
            {
                Debug.LogError("[FBXCharacterController] Could not find NewBody prefab asset in project");
                return null;
            }
            #else
            Debug.LogError("[FBXCharacterController] Character instantiation only available in editor mode");
            return null;
            #endif
        }

        /// <summary>
        /// Find the hip bone in the character hierarchy
        /// </summary>
        private Transform FindHipBone()
        {
            if (characterRoot == null) return null;
            
            if (enableLogging)
            {
                Debug.Log("[FBXCharacterController] === SEARCHING FOR HIP BONE ===");
                Debug.Log("[FBXCharacterController] Available bones in character hierarchy:");
                
                // List all bones for debugging
                Transform[] allTransforms = characterRoot.GetComponentsInChildren<Transform>();
                foreach (Transform t in allTransforms)
                {
                    Debug.Log($"  - {t.name} (parent: {(t.parent ? t.parent.name : "none")})");
                }
                
                Debug.Log($"[FBXCharacterController] Searching for hip bone using names: [{string.Join(", ", hipBoneNames)}]");
            }
            
            // Try each potential hip bone name
            foreach (string boneName in hipBoneNames)
            {
                Transform bone = FindChildRecursive(characterRoot.transform, boneName);
                if (bone != null)
                {
                    if (enableLogging) 
                    {
                        Debug.Log($"[FBXCharacterController] ✅ Found hip bone: '{bone.name}' at position {bone.position:F3}");
                        Debug.Log($"[FBXCharacterController] Hip bone world position: {bone.position:F3}");
                        Debug.Log($"[FBXCharacterController] Hip bone local position: {bone.localPosition:F3}");
                    }
                    return bone;
                }
                else
                {
                    if (enableLogging) Debug.Log($"[FBXCharacterController] ❌ Bone '{boneName}' not found");
                }
            }
            
            // If no hip bone found by name, try to use Animator if available
            Animator animator = characterRoot.GetComponent<Animator>();
            if (animator != null && animator.isHuman)
            {
                Transform hipBone = animator.GetBoneTransform(HumanBodyBones.Hips);
                if (hipBone != null)
                {
                    if (enableLogging) 
                    {
                        Debug.Log($"[FBXCharacterController] ✅ Found hip bone via Animator: '{hipBone.name}' at position {hipBone.position:F3}");
                    }
                    return hipBone;
                }
            }
            
            if (enableLogging) Debug.Log("[FBXCharacterController] ❌ No hip bone found with any method");
            return null;
        }

        /// <summary>
        /// Recursively search for a child transform by name
        /// </summary>
        private Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                return parent;
            }
            
            foreach (Transform child in parent)
            {
                Transform result = FindChildRecursive(child, name);
                if (result != null)
                {
                    return result;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Create debug visualization sphere
        /// </summary>
        private void CreateDebugVisualization()
        {
            debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugSphere.name = "CharacterHipTarget_Debug";
            debugSphere.transform.localScale = Vector3.one * 0.2f;
            
            // Make it yellow to distinguish from red ARKit sphere
            var renderer = debugSphere.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Unlit/Color"));
                material.color = Color.yellow;
                renderer.material = material;
            }
            
            // Remove collider
            var collider = debugSphere.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }
            
            debugSphere.SetActive(false);
            
            if (enableLogging) Debug.Log("[FBXCharacterController] Created yellow debug sphere for character hip target");
        }

        void Update()
        {
            if (isInitialized && hasValidTarget)
            {
                UpdateCharacterPosition();
            }
        }

        void OnDestroy()
        {
            // Clean up debug visualization
            if (debugSphere != null)
            {
                Destroy(debugSphere);
            }
        }

        #region Public API Methods

        /// <summary>
        /// Manually set the character root GameObject
        /// </summary>
        public void SetCharacter(GameObject character)
        {
            characterRoot = character;
            isInitialized = false;
            Initialize();
        }

        /// <summary>
        /// Manually set the hip bone transform
        /// </summary>
        public void SetHipBone(Transform hip)
        {
            hipBone = hip;
            if (enableLogging) Debug.Log($"[FBXCharacterController] Hip bone manually set to '{hip.name}'");
        }

        /// <summary>
        /// Enable/disable debug visualization
        /// </summary>
        public void SetDebugVisualization(bool enabled)
        {
            showDebugSphere = enabled;
            if (debugSphere != null)
            {
                debugSphere.SetActive(enabled && hasValidTarget);
            }
        }

        /// <summary>
        /// Get detailed character information for debugging
        /// </summary>
        public string GetCharacterInfo()
        {
            if (characterRoot == null) return "No character assigned";
            
            string info = $"Character: {characterRoot.name}\n";
            info += $"Position: {characterRoot.transform.position:F3}\n";
            info += $"Scale: {characterRoot.transform.localScale:F3}\n";
            info += $"Active: {characterRoot.activeInHierarchy} (self: {characterRoot.activeSelf})\n";
            
            if (hipBone != null)
            {
                info += $"Hip Bone: {hipBone.name}\n";
                info += $"Hip Position: {hipBone.position:F3}\n";
            }
            
            Renderer[] renderers = characterRoot.GetComponentsInChildren<Renderer>(true);
            int activeRenderers = 0;
            foreach (var r in renderers)
            {
                if (r != null && r.enabled && r.gameObject.activeInHierarchy)
                    activeRenderers++;
            }
            info += $"Renderers: {renderers.Length} total, {activeRenderers} active";
            
            return info;
        }

        #endregion

        #region Console Commands

        /// <summary>
        /// Console command to make character visible - can be called from Unity console
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void MakeNewBodyVisible()
        {
            FBXCharacterController controller = FindObjectOfType<FBXCharacterController>();
            if (controller != null)
            {
                // Re-initialize to ensure character is properly instantiated
                controller.isInitialized = false;
                if (controller.Initialize())
                {
                    Debug.Log("[FBXCharacterController] Console command: Character reinitialized successfully");
                }
                else
                {
                    Debug.LogError("[FBXCharacterController] Console command: Failed to reinitialize character");
                }
            }
            else
            {
                Debug.LogError("[FBXCharacterController] Console command: No FBXCharacterController found in scene");
            }
        }

        /// <summary>
        /// Console command to test loading animation from FBX
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void LoadAnimationFromPath(string fbxPath, string animationName = "IMG_36822")
        {
            FBXCharacterController controller = FindObjectOfType<FBXCharacterController>();
            if (controller != null)
            {
                bool success = controller.LoadAnimationFromFBX(fbxPath, animationName);
                if (success)
                {
                    Debug.Log($"[FBXCharacterController] Console command: Successfully loaded animation '{animationName}' from '{fbxPath}'");
                }
                else
                {
                    Debug.LogError($"[FBXCharacterController] Console command: Failed to load animation '{animationName}' from '{fbxPath}'");
                }
            }
            else
            {
                Debug.LogError("[FBXCharacterController] Console command: No FBXCharacterController found in scene");
            }
        }

        /// <summary>
        /// Console command to manually set hip bone by name for testing
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void SetHipBoneByName(string boneName)
        {
            FBXCharacterController controller = FindObjectOfType<FBXCharacterController>();
            if (controller != null)
            {
                Transform bone = controller.FindChildRecursive(controller.characterRoot.transform, boneName);
                if (bone != null)
                {
                    controller.SetHipBone(bone);
                    Debug.Log($"[FBXCharacterController] Console command: Hip bone set to '{boneName}' at position {bone.position:F3}");
                }
                else
                {
                    Debug.LogError($"[FBXCharacterController] Console command: Bone '{boneName}' not found in character hierarchy");
                }
            }
            else
            {
                Debug.LogError("[FBXCharacterController] Console command: No FBXCharacterController found in scene");
            }
        }

        /// <summary>
        /// Quick debug method to make character immediately visible for testing
        /// </summary>
        public void MakeCharacterVisibleForTesting()
        {
            if (characterRoot == null) return;
            
            Debug.Log("[FBXCharacterController] Making character visible for testing...");
            
            // Position in front of camera
            Camera arCamera = Camera.main ?? FindObjectOfType<Camera>();
            if (arCamera != null)
            {
                Vector3 frontPos = arCamera.transform.position + arCamera.transform.forward * 2.0f;
                characterRoot.transform.position = frontPos;
                Debug.Log($"[FBXCharacterController] Positioned character at {frontPos:F3}");
            }
            
            // Make it larger and bright
            characterRoot.transform.localScale = Vector3.one * 2.0f;
            
            // Apply bright cyan material to all renderers
            Renderer[] renderers = characterRoot.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.gameObject.SetActive(true);
                    renderer.enabled = true;
                    
                    // Create bright cyan material for visibility
                    var material = new Material(Shader.Find("Unlit/Color"));
                    material.color = Color.cyan;
                    renderer.material = material;
                }
            }
            
            Debug.Log($"[FBXCharacterController] Made character visible: Scale 2x, Cyan materials, {renderers.Length} renderers");
        }

        /// <summary>
        /// Restore normal materials to character renderers
        /// </summary>
        public void RestoreNormalMaterials()
        {
            if (characterRoot == null) return;
            
            if (enableLogging) Debug.Log("[FBXCharacterController] Restoring normal materials...");
            
            // Get all renderers
            Renderer[] renderers = characterRoot.GetComponentsInChildren<Renderer>(true);
            
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    // Ensure renderer and GameObject are active
                    renderer.gameObject.SetActive(true);
                    renderer.enabled = true;
                    
                    // Use the default material from the FBX
                    // This will restore the original appearance
                    var meshRenderer = renderer as MeshRenderer;
                    if (meshRenderer != null)
                    {
                        // Let Unity use the default materials from the FBX import
                        // We don't need to manually assign anything
                    }
                }
            }
            
            if (enableLogging) Debug.Log($"[FBXCharacterController] Restored materials for {renderers.Length} renderers");
        }

        /// <summary>
        /// Console command to list all animations in NewAnimationOnly.fbx and NewBody.fbx
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void ListAvailableAnimations()
        {
            #if UNITY_EDITOR
            Debug.Log("=== LISTING ALL AVAILABLE ANIMATIONS ===");
            
            // Check NewAnimationOnly.fbx
            string animationFbxPath = "Assets/DeepMotion/NewAnimationOnly.fbx";
            GameObject animationFbx = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(animationFbxPath);
            if (animationFbx != null)
            {
                Debug.Log($"Animations in {animationFbxPath}:");
                Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(animationFbxPath);
                foreach (Object asset in assets)
                {
                    if (asset is AnimationClip clip)
                    {
                        Debug.Log($"  - '{clip.name}' ({clip.length:F2}s, {clip.frameRate}fps)");
                    }
                }
            }
            else
            {
                Debug.LogError($"Could not load {animationFbxPath}");
            }
            
            // Check NewBody.fbx
            string bodyFbxPath = "Assets/DeepMotion/NewBody.fbx";
            GameObject bodyFbx = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(bodyFbxPath);
            if (bodyFbx != null)
            {
                Debug.Log($"Animations in {bodyFbxPath}:");
                Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(bodyFbxPath);
                foreach (Object asset in assets)
                {
                    if (asset is AnimationClip clip)
                    {
                        Debug.Log($"  - '{clip.name}' ({clip.length:F2}s, {clip.frameRate}fps)");
                    }
                }
            }
            else
            {
                Debug.LogError($"Could not load {bodyFbxPath}");
            }
            #endif
        }

        /// <summary>
        /// Console command to manually test animation playback
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void TestAnimationPlayback()
        {
            FBXCharacterController controller = FindObjectOfType<FBXCharacterController>();
            if (controller != null)
            {
                Debug.Log("[FBXCharacterController] === MANUAL ANIMATION TEST ===");
                bool success = controller.StartAnimationPlayback();
                if (success)
                {
                    Debug.Log("[FBXCharacterController] Console command: Animation playback test completed");
                }
                else
                {
                    Debug.LogError("[FBXCharacterController] Console command: Animation playback test failed");
                }
            }
            else
            {
                Debug.LogError("[FBXCharacterController] Console command: No FBXCharacterController found in scene");
            }
        }

        /// <summary>
        /// Console command to check animator status
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void CheckAnimatorStatus()
        {
            FBXCharacterController controller = FindObjectOfType<FBXCharacterController>();
            if (controller != null && controller.characterAnimator != null)
            {
                var animator = controller.characterAnimator;
                Debug.Log($"[FBXCharacterController] === ANIMATOR STATUS ===");
                Debug.Log($"Animator enabled: {animator.enabled}");
                Debug.Log($"Animator speed: {animator.speed}");
                Debug.Log($"Runtime controller: {(animator.runtimeAnimatorController ? animator.runtimeAnimatorController.name : "None")}");
                Debug.Log($"Layer count: {animator.layerCount}");
                Debug.Log($"Parameter count: {animator.parameterCount}");
                
                if (animator.layerCount > 0)
                {
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    Debug.Log($"Current state hash: {stateInfo.shortNameHash}");
                    Debug.Log($"State length: {stateInfo.length:F2}s");
                    Debug.Log($"Normalized time: {stateInfo.normalizedTime:F3}");
                    Debug.Log($"Animation playing: {animator.GetCurrentAnimatorStateInfo(0).length > 0 && animator.speed > 0}");
                }
                
                // List all states in controller
                if (animator.runtimeAnimatorController != null)
                {
                    Debug.Log($"Controller states:");
                    // This would require more complex reflection to get state names
                }
            }
            else
            {
                Debug.LogError("[FBXCharacterController] Console command: No FBXCharacterController or Animator found");
            }
        }

        /// <summary>
        /// Console command to find and highlight the runtime NewBody character
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void FindRuntimeCharacter()
        {
            #if UNITY_EDITOR
            FBXCharacterController controller = FindObjectOfType<FBXCharacterController>();
            if (controller != null && controller.characterRoot != null)
            {
                Debug.Log($"[FBXCharacterController] === RUNTIME CHARACTER FOUND ===");
                Debug.Log($"Character name: {controller.characterRoot.name}");
                Debug.Log($"Scene position: {controller.characterRoot.transform.position}");
                Debug.Log($"Active in hierarchy: {controller.characterRoot.activeInHierarchy}");
                
                // Try to select it in the hierarchy
                UnityEditor.Selection.activeGameObject = controller.characterRoot;
                Debug.Log("[FBXCharacterController] Character selected in hierarchy - check Inspector!");
                
                // Log all components
                var components = controller.characterRoot.GetComponents<Component>();
                Debug.Log($"Components on character ({components.Length}):");
                foreach (var comp in components)
                {
                    Debug.Log($"  - {comp.GetType().Name}");
                }
                
                // Log children count
                Debug.Log($"Child objects: {controller.characterRoot.transform.childCount}");
                for (int i = 0; i < controller.characterRoot.transform.childCount; i++)
                {
                    var child = controller.characterRoot.transform.GetChild(i);
                    Debug.Log($"  - {child.name} (active: {child.gameObject.activeInHierarchy})");
                }
            }
            else
            {
                Debug.LogError("[FBXCharacterController] No runtime character found or FBXCharacterController not initialized");
            }
            #endif
        }

        /// <summary>
        /// Console command to force character visibility for debugging
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void MakeCharacterVisible()
        {
            FBXCharacterController controller = FindObjectOfType<FBXCharacterController>();
            if (controller != null)
            {
                controller.MakeCharacterVisibleForTesting();
                
                // Also select it
                #if UNITY_EDITOR
                if (controller.characterRoot != null)
                {
                    UnityEditor.Selection.activeGameObject = controller.characterRoot;
                }
                #endif
            }
            else
            {
                Debug.LogError("[FBXCharacterController] No FBXCharacterController found");
            }
        }

        /// <summary>
        /// Console command to force reload animation from NewAnimationOnly.fbx
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void ForceReloadAnimationFromNewAnimationFBX()
        {
            FBXCharacterController controller = FindObjectOfType<FBXCharacterController>();
            if (controller != null)
            {
                Debug.Log("[FBXCharacterController] === FORCE RELOADING ANIMATION ===");
                
                // First, list available animations
                ListAvailableAnimations();
                
                // Get first animation from NewAnimationOnly.fbx
                string animationName = controller.GetFirstAnimationFromNewAnimationFBX();
                if (!string.IsNullOrEmpty(animationName))
                {
                    Debug.Log($"[FBXCharacterController] Attempting to load animation: {animationName}");
                    bool success = controller.LoadAnimationFromNewAnimationFBX(animationName);
                    if (success)
                    {
                        Debug.Log($"[FBXCharacterController] Successfully loaded animation '{animationName}' from NewAnimationOnly.fbx");
                        
                        // Try to start playback
                        bool playbackStarted = controller.StartAnimationPlayback();
                        if (playbackStarted)
                        {
                            Debug.Log("[FBXCharacterController] Animation playback started successfully!");
                        }
                        else
                        {
                            Debug.LogError("[FBXCharacterController] Failed to start animation playback");
                        }
                    }
                    else
                    {
                        Debug.LogError($"[FBXCharacterController] Failed to load animation '{animationName}'");
                    }
                }
                else
                {
                    Debug.LogError("[FBXCharacterController] No animations found in NewAnimationOnly.fbx");
                }
            }
            else
            {
                Debug.LogError("[FBXCharacterController] No FBXCharacterController found in scene");
            }
        }

        #endregion

        #region Animation Management

        /// <summary>
        /// Initialize animation system for the character
        /// </summary>
        private void InitializeAnimationSystem()
        {
            if (characterRoot == null) return;
            
            characterAnimator = characterRoot.GetComponent<Animator>();
            if (characterAnimator == null)
            {
                // Add Animator component if missing
                characterAnimator = characterRoot.AddComponent<Animator>();
                if (enableLogging) Debug.Log("[FBXCharacterController] Added Animator component to character");
            }
            
            // Detailed logging of animator state
            if (enableLogging)
            {
                Debug.Log("[FBXCharacterController] === ANIMATOR INITIALIZATION DEBUG ===");
                Debug.Log($"[FBXCharacterController] Character GameObject: {characterRoot.name}");
                Debug.Log($"[FBXCharacterController] Animator found: {characterAnimator != null}");
                Debug.Log($"[FBXCharacterController] Animator enabled: {characterAnimator.enabled}");
                Debug.Log($"[FBXCharacterController] Initial controller: {(characterAnimator.runtimeAnimatorController ? characterAnimator.runtimeAnimatorController.name : "None")}");
                Debug.Log($"[FBXCharacterController] Avatar assigned: {characterAnimator.avatar != null}");
                Debug.Log($"[FBXCharacterController] Is human: {characterAnimator.isHuman}");
            }
            
            // Set up override controller if we have one
            if (animatorOverrideController != null)
            {
                runtimeOverrideController = new AnimatorOverrideController(animatorOverrideController);
                characterAnimator.runtimeAnimatorController = runtimeOverrideController;
                
                if (enableLogging) 
                {
                    Debug.Log("[FBXCharacterController] Animation system initialized with override controller");
                    Debug.Log($"[FBXCharacterController] Override controller name: {animatorOverrideController.name}");
                    Debug.Log($"[FBXCharacterController] Base controller: {(animatorOverrideController.runtimeAnimatorController ? animatorOverrideController.runtimeAnimatorController.name : "None")}");
                    
                    // Log override slots
                    var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                    runtimeOverrideController.GetOverrides(overrides);
                    Debug.Log($"[FBXCharacterController] Override slots available: {overrides.Count}");
                    foreach (var pair in overrides)
                    {
                        Debug.Log($"[FBXCharacterController] Slot '{pair.Key.name}' -> {(pair.Value ? pair.Value.name : "Empty")}");
                    }
                }
            }
            else 
            {
                if (enableLogging) Debug.LogWarning("[FBXCharacterController] No override controller assigned - animations will not work. Please assign 'Animator Override Controller' in inspector.");
            }
            
            // Force animator to be ready
            characterAnimator.enabled = true;
            characterAnimator.Rebind();
            
            if (enableLogging)
            {
                Debug.Log("[FBXCharacterController] === ANIMATOR POST-SETUP ===");
                Debug.Log($"[FBXCharacterController] Final controller: {(characterAnimator.runtimeAnimatorController ? characterAnimator.runtimeAnimatorController.name : "None")}");
                Debug.Log($"[FBXCharacterController] Layer count: {characterAnimator.layerCount}");
                Debug.Log($"[FBXCharacterController] Parameter count: {characterAnimator.parameterCount}");
            }
        }

        /// <summary>
        /// Start animation playback synchronized with hip recording
        /// </summary>
        public bool StartAnimationPlayback()
        {
            if (characterAnimator == null)
            {
                Debug.LogWarning("[FBXCharacterController] No animator available for animation playback");
                return false;
            }
            
            if (runtimeOverrideController == null)
            {
                Debug.LogWarning("[FBXCharacterController] No override controller available for animation playback");
                return false;
            }
            
            // Try to load default animation if not already loaded
            if (!LoadDefaultAnimation())
            {
                Debug.LogWarning("[FBXCharacterController] No animation clip available for playback");
                return false;
            }
            
            // Debug current animator state
            if (enableLogging)
            {
                Debug.Log($"[FBXCharacterController] === ANIMATION PLAYBACK DEBUG ===");
                Debug.Log($"[FBXCharacterController] Animator enabled: {characterAnimator.enabled}");
                Debug.Log($"[FBXCharacterController] Animator speed: {characterAnimator.speed}");
                Debug.Log($"[FBXCharacterController] Runtime controller assigned: {characterAnimator.runtimeAnimatorController != null}");
                Debug.Log($"[FBXCharacterController] Controller name: {(characterAnimator.runtimeAnimatorController ? characterAnimator.runtimeAnimatorController.name : "None")}");
                
                // List all available states
                if (characterAnimator.runtimeAnimatorController != null)
                {
                    var controller = characterAnimator.runtimeAnimatorController as AnimatorOverrideController;
                    if (controller != null)
                    {
                        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                        controller.GetOverrides(overrides);
                        Debug.Log($"[FBXCharacterController] Override clips available: {overrides.Count}");
                        foreach (var pair in overrides)
                        {
                            Debug.Log($"[FBXCharacterController] Override: '{pair.Key?.name}' -> '{pair.Value?.name}'");
                        }
                    }
                }
                
                // Check current state
                if (characterAnimator.layerCount > 0)
                {
                    AnimatorStateInfo stateInfo = characterAnimator.GetCurrentAnimatorStateInfo(0);
                    Debug.Log($"[FBXCharacterController] Current state: {stateInfo.shortNameHash} (length: {stateInfo.length:F2}s)");
                    Debug.Log($"[FBXCharacterController] State time: {stateInfo.normalizedTime:F3}");
                    Debug.Log($"[FBXCharacterController] Is playing: {!stateInfo.IsName("New State") && stateInfo.length > 0}");
                }
            }
            
            // Ensure animator is enabled and has proper speed
            characterAnimator.enabled = true;
            characterAnimator.speed = 1.0f; // Normal speed - will stay in sync with hip recording
            
            // Try different state names to play the animation
            string[] stateNames = { "CharacterAnimation", "climber1", "Base Layer.CharacterAnimation", "Base Layer.climber1" };
            bool animationStarted = false;
            
            foreach (string stateName in stateNames)
            {
                try
                {
                    characterAnimator.Play(stateName, 0, 0f); // Play from start
                    characterAnimator.Update(0f); // Force update
                    
                    // Check if the state actually changed
                    if (characterAnimator.layerCount > 0)
                    {
                        AnimatorStateInfo newStateInfo = characterAnimator.GetCurrentAnimatorStateInfo(0);
                        if (newStateInfo.length > 1.0f) // If we got a longer animation than the default 1s state
                        {
                            if (enableLogging) Debug.Log($"[FBXCharacterController] Successfully started animation with state: '{stateName}'");
                            animationStarted = true;
                            break;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    if (enableLogging) Debug.Log($"[FBXCharacterController] Failed to play state '{stateName}': {e.Message}");
                }
            }
            
            if (!animationStarted)
            {
                // Fallback: try to play any available state
                if (enableLogging) Debug.Log("[FBXCharacterController] Trying fallback animation playback...");
                characterAnimator.Play(0, 0, 0f); // Play first state in first layer
                characterAnimator.Update(0f);
            }
            
            if (enableLogging) 
            {
                Debug.Log("[FBXCharacterController] Started animation playback - will stay in sync with hip recording");
                
                // Check state after play command
                if (characterAnimator.layerCount > 0)
                {
                    AnimatorStateInfo stateInfo = characterAnimator.GetCurrentAnimatorStateInfo(0);
                    Debug.Log($"[FBXCharacterController] After play - Current state: {stateInfo.shortNameHash} (length: {stateInfo.length:F2}s)");
                    Debug.Log($"[FBXCharacterController] After play - State time: {stateInfo.normalizedTime:F3}");
                }
            }
            
            return true;
        }

        /// <summary>
        /// Stop animation playback
        /// </summary>
        public void StopAnimationPlayback()
        {
            if (characterAnimator != null)
            {
                characterAnimator.speed = 0f; // Pause animation
                if (enableLogging) Debug.Log("[FBXCharacterController] Stopped animation playback");
            }
        }

        /// <summary>
        /// Load animation from external FBX and apply to character
        /// </summary>
        /// <param name="animationFbxPath">Path to FBX containing animation (e.g., NewAnimation.fbx or DeepMotion result)</param>
        /// <param name="animationName">Name of animation clip to extract</param>
        public bool LoadAnimationFromFBX(string animationFbxPath, string animationName = null)
        {
            if (characterAnimator == null || runtimeOverrideController == null)
            {
                Debug.LogError("[FBXCharacterController] Animation system not initialized");
                return false;
            }
            
            if (string.IsNullOrEmpty(animationName))
            {
                animationName = defaultAnimationClipName;
            }
            
            #if UNITY_EDITOR
            // Load the animation FBX asset
            GameObject animationFbx = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(animationFbxPath);
            if (animationFbx == null)
            {
                Debug.LogError($"[FBXCharacterController] Could not load animation FBX at path: {animationFbxPath}");
                return false;
            }
            
            // Find the animation clip in the FBX
            AnimationClip animationClip = FindAnimationClipInFBX(animationFbx, animationName);
            if (animationClip == null)
            {
                Debug.LogError($"[FBXCharacterController] Animation '{animationName}' not found in FBX: {animationFbxPath}");
                return false;
            }
            
            return ApplyAnimationClip(animationClip, "CharacterAnimation");
            #else
            Debug.LogError("[FBXCharacterController] FBX loading only available in editor mode");
            return false;
            #endif
        }

        /// <summary>
        /// Apply animation clip to character using override controller
        /// </summary>
        /// <param name="animationClip">The animation clip to apply</param>
        /// <param name="overrideSlotName">Name of the animation slot to override</param>
        public bool ApplyAnimationClip(AnimationClip animationClip, string overrideSlotName = "CharacterAnimation")
        {
            if (runtimeOverrideController == null)
            {
                Debug.LogError("[FBXCharacterController] No runtime override controller available");
                return false;
            }
            
            // Override the animation clip
            runtimeOverrideController[overrideSlotName] = animationClip;
            
            if (enableLogging)
            {
                Debug.Log($"[FBXCharacterController] Applied animation '{animationClip.name}' to slot '{overrideSlotName}'");
                Debug.Log($"[FBXCharacterController] Animation length: {animationClip.length:F2}s, framerate: {animationClip.frameRate}fps");
            }
            
            return true;
        }

        /// <summary>
        /// Find animation clip in FBX asset
        /// </summary>
        private AnimationClip FindAnimationClipInFBX(GameObject fbxAsset, string animationName)
        {
            #if UNITY_EDITOR
            string fbxPath = UnityEditor.AssetDatabase.GetAssetPath(fbxAsset);
            Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            
            foreach (Object asset in assets)
            {
                if (asset is AnimationClip clip)
                {
                    // Check for exact match first
                    if (clip.name == animationName)
                    {
                        if (enableLogging) Debug.Log($"[FBXCharacterController] Found exact animation match: '{clip.name}'");
                        return clip;
                    }
                    
                    // Check for partial match (in case of Take 001, etc.)
                    if (clip.name.Contains(animationName))
                    {
                        if (enableLogging) Debug.Log($"[FBXCharacterController] Found partial animation match: '{clip.name}' for '{animationName}'");
                        return clip;
                    }
                }
            }
            
            // List all available animations for debugging
            if (enableLogging)
            {
                Debug.Log($"[FBXCharacterController] Available animations in FBX:");
                foreach (Object asset in assets)
                {
                    if (asset is AnimationClip clip)
                    {
                        Debug.Log($"  - {clip.name} ({clip.length:F2}s)");
                    }
                }
            }
            #endif
            
            return null;
        }

        /// <summary>
        /// Load default animation from the NewBody FBX
        /// </summary>
        private bool LoadDefaultAnimation()
        {
            if (runtimeOverrideController == null) return false;
            
            // Check if animation is already loaded
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            runtimeOverrideController.GetOverrides(overrides);
            
            // Look for CharacterAnimation slot
            foreach (var pair in overrides)
            {
                if (pair.Key.name == "CharacterAnimation" && pair.Value != null)
                {
                    // Animation already loaded
                    if (enableLogging) Debug.Log($"[FBXCharacterController] Animation already loaded: {pair.Value.name}");
                    return true;
                }
            }
            
            // Auto-detect animation name if not specified
            string animationToLoad = defaultAnimationClipName;
            if (string.IsNullOrEmpty(animationToLoad))
            {
                animationToLoad = GetFirstAnimationFromNewAnimationFBX();
                if (string.IsNullOrEmpty(animationToLoad))
                {
                    Debug.LogError("[FBXCharacterController] No animations found in NewAnimationOnly.fbx");
                    return false;
                }
                if (enableLogging) Debug.Log($"[FBXCharacterController] Auto-detected animation: {animationToLoad}");
            }
            
            // Try to load animation from NewAnimationOnly.fbx first, then fallback to NewBody.fbx
            if (LoadAnimationFromFBX("Assets/DeepMotion/NewAnimationOnly.fbx", animationToLoad))
            {
                if (enableLogging) Debug.Log($"[FBXCharacterController] Loaded animation '{animationToLoad}' from NewAnimationOnly.fbx");
                return true;
            }
            
            // Fallback to NewBody.fbx in case animation is embedded there
            if (LoadAnimationFromFBX("Assets/DeepMotion/NewBody.fbx", animationToLoad))
            {
                if (enableLogging) Debug.Log($"[FBXCharacterController] Loaded animation '{animationToLoad}' from NewBody.fbx");
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Load animation from NewAnimationOnly.fbx (convenience method)
        /// </summary>
        public bool LoadAnimationFromNewAnimationFBX(string animationName = null)
        {
            if (string.IsNullOrEmpty(animationName))
            {
                animationName = defaultAnimationClipName;
                if (string.IsNullOrEmpty(animationName))
                {
                    animationName = GetFirstAnimationFromNewAnimationFBX();
                }
            }
            
            bool success = LoadAnimationFromFBX("Assets/DeepMotion/NewAnimationOnly.fbx", animationName);
            if (success && enableLogging)
            {
                Debug.Log($"[FBXCharacterController] Successfully loaded animation '{animationName}' from NewAnimationOnly.fbx");
            }
            return success;
        }

        /// <summary>
        /// Get the first available animation clip name from NewAnimationOnly.fbx
        /// </summary>
        private string GetFirstAnimationFromNewAnimationFBX()
        {
            #if UNITY_EDITOR
            string animationFbxPath = "Assets/DeepMotion/NewAnimationOnly.fbx";
            GameObject animationFbx = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(animationFbxPath);
            if (animationFbx != null)
            {
                Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(animationFbxPath);
                foreach (Object asset in assets)
                {
                    if (asset is AnimationClip clip)
                    {
                        if (enableLogging) Debug.Log($"[FBXCharacterController] Found animation clip: '{clip.name}' in NewAnimationOnly.fbx");
                        return clip.name;
                    }
                }
            }
            #endif
            return null;
        }

        /// <summary>
        /// Load animation from DeepMotion result FBX 
        /// </summary>
        /// <param name="deepMotionFbxPath">Path to the FBX file returned from DeepMotion API</param>
        /// <param name="animationName">Name of animation clip (often "Take 001" or similar)</param>
        public bool LoadAnimationFromDeepMotionFBX(string deepMotionFbxPath, string animationName = "Take 001")
        {
            bool success = LoadAnimationFromFBX(deepMotionFbxPath, animationName);
            if (success && enableLogging)
            {
                Debug.Log($"[FBXCharacterController] Successfully loaded DeepMotion animation from {deepMotionFbxPath}");
            }
            return success;
        }

        /// <summary>
        /// Get available animation clips from current override controller
        /// </summary>
        public string[] GetAvailableAnimationSlots()
        {
            if (runtimeOverrideController == null) return new string[0];
            
            // Get all override clips
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            runtimeOverrideController.GetOverrides(overrides);
            
            var slots = new string[overrides.Count];
            for (int i = 0; i < overrides.Count; i++)
            {
                slots[i] = overrides[i].Key.name;
            }
            
            return slots;
        }

        #endregion
    }

    #if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(FBXCharacterController))]
    public class FBXCharacterControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            GUILayout.Space(10);
            GUILayout.Label("Debug Controls", UnityEditor.EditorStyles.boldLabel);
            
            FBXCharacterController controller = (FBXCharacterController)target;
            
            if (GUILayout.Button("Find Runtime Character"))
            {
                if (controller.CharacterRootForEditor != null)
                {
                    UnityEditor.Selection.activeGameObject = controller.CharacterRootForEditor;
                    Debug.Log("[FBXCharacterController] Character selected in hierarchy!");
                }
                else
                {
                    Debug.LogError("[FBXCharacterController] No character found!");
                }
            }
            
            if (GUILayout.Button("Check Animator Status"))
            {
                if (controller.CharacterAnimatorForEditor != null)
                {
                    var animator = controller.CharacterAnimatorForEditor;
                    Debug.Log($"=== ANIMATOR STATUS ===");
                    Debug.Log($"Enabled: {animator.enabled}");
                    Debug.Log($"Speed: {animator.speed}");
                    Debug.Log($"Controller: {(animator.runtimeAnimatorController ? animator.runtimeAnimatorController.name : "None")}");
                    Debug.Log($"Layers: {animator.layerCount}");
                    
                    if (animator.layerCount > 0)
                    {
                        var state = animator.GetCurrentAnimatorStateInfo(0);
                        Debug.Log($"Current state length: {state.length:F2}s");
                        Debug.Log($"Animation time: {state.normalizedTime:F3}");
                    }
                }
                else
                {
                    Debug.LogError("No animator found!");
                }
            }
            
            if (GUILayout.Button("Test Animation Playback"))
            {
                bool success = controller.StartAnimationPlayback();
                if (success)
                {
                    Debug.Log("Animation test completed!");
                }
                else
                {
                    Debug.LogError("Animation test failed!");
                }
            }
            
            if (GUILayout.Button("Make Character Visible"))
            {
                controller.MakeCharacterVisibleForTesting();
                if (controller.CharacterRootForEditor != null)
                {
                    UnityEditor.Selection.activeGameObject = controller.CharacterRootForEditor;
                }
            }
            
            if (GUILayout.Button("Fix Override Controller"))
            {
                FixOverrideController(controller);
            }
            
            if (GUILayout.Button("Force Reload Animation from NewAnimationOnly.fbx"))
            {
                FBXCharacterController.ForceReloadAnimationFromNewAnimationFBX();
            }
            
            if (GUILayout.Button("List Available Animations"))
            {
                FBXCharacterController.ListAvailableAnimations();
            }
        }
        
        private void FixOverrideController(FBXCharacterController controller)
        {
            if (controller.AnimatorOverrideControllerForEditor == null)
            {
                Debug.LogError("No AnimatorOverrideController assigned!");
                return;
            }
            
            // Check if base controller is assigned
            if (controller.AnimatorOverrideControllerForEditor.runtimeAnimatorController == null)
            {
                Debug.LogError("AnimatorOverrideController has no base controller assigned!");
                Debug.Log("Please assign a base Animator Controller to your Override Controller");
                return;
            }
            
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            controller.AnimatorOverrideControllerForEditor.GetOverrides(overrides);
            
            Debug.Log($"Override controller '{controller.AnimatorOverrideControllerForEditor.name}' has {overrides.Count} slots:");
            foreach (var pair in overrides)
            {
                Debug.Log($"  - {pair.Key.name} -> {(pair.Value ? pair.Value.name : "Empty")}");
            }
            
            if (overrides.Count == 0)
            {
                Debug.LogError("Override controller has no animation slots! You need to:");
                Debug.Log("1. Create a base Animator Controller with states");
                Debug.Log("2. Assign it to your Override Controller");
                Debug.Log("3. The Override Controller will then show animation slots to fill");
            }
        }
    }
    #endif
} 