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
        [SerializeField] private float groundOffset = 0.0f;
        
        [Header("Animation Management")]
        [SerializeField] private AnimatorOverrideController animatorOverrideController;
        [SerializeField] private string defaultAnimationClipName = "";
        
        // State
        private bool isInitialized = false;
        private Vector3 targetHipPosition;
        private bool hasValidTarget = false;
        
        // Animation state
        private Animator characterAnimator;
        private AnimatorOverrideController runtimeOverrideController;
        
        // Events
        public event System.Action<Vector3> OnHipPositionUpdated;
        
        // Public properties
        public bool IsInitialized => isInitialized;
        public Vector3 CurrentHipPosition => hipBone != null ? hipBone.position : Vector3.zero;
        public GameObject CharacterRoot => characterRoot;

        void Start()
        {
            Initialize();
        }

        /// <summary>
        /// Initialize the character controller
        /// </summary>
        public bool Initialize()
        {
            if (isInitialized) return true;
            
            if (characterRoot == null)
            {
                characterRoot = FindCharacterInScene();
            }
            
            if (characterRoot == null)
            {
                Debug.LogError("[FBXCharacterController] No character found! Please assign 'Character Root' or ensure 'NewBody' exists in scene");
                return false;
            }
            
            if (hipBone == null && autoFindHipBone)
            {
                hipBone = FindHipBone();
            }
            
            if (hipBone == null)
            {
                Debug.LogError($"[FBXCharacterController] Hip bone not found in character '{characterRoot.name}'. Please assign manually or check bone names.");
                return false;
            }
            
            isInitialized = true;
            InitializeAnimationSystem();
            
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
            
            Vector3 currentHipWorld = hipBone.position;
            Vector3 offset = targetHipPosition - currentHipWorld;
            characterRoot.transform.position += offset;
            
            OnHipPositionUpdated?.Invoke(targetHipPosition);
        }

        /// <summary>
        /// Find character GameObject in scene by name
        /// </summary>
        private GameObject FindCharacterInScene()
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "NewBody" || obj.name == "Newbody")
                {
                    if (obj.scene.IsValid())
                    {
                        return obj;
                    }
                }
            }
            
            return InstantiateCharacterFromPrefab();
        }

        /// <summary>
        /// Instantiate character from prefab asset into the scene
        /// </summary>
        private GameObject InstantiateCharacterFromPrefab()
        {
            #if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:GameObject NewBody");
            GameObject prefabAsset = null;
            
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                GameObject candidate = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (candidate != null && (candidate.name == "NewBody" || candidate.name == "Newbody"))
                {
                    prefabAsset = candidate;
                    break;
                }
            }
            
            if (prefabAsset != null)
            {
                GameObject newSceneCharacter = GameObject.Instantiate(prefabAsset);
                newSceneCharacter.name = prefabAsset.name;
                newSceneCharacter.transform.position = Vector3.zero;
                newSceneCharacter.transform.rotation = Quaternion.identity;
                newSceneCharacter.transform.localScale = Vector3.one;
                
                return newSceneCharacter;
            }
            #endif
            return null;
        }

        /// <summary>
        /// Find the hip bone in the character hierarchy
        /// </summary>
        private Transform FindHipBone()
        {
            if (characterRoot == null) return null;
            
            foreach (string boneName in hipBoneNames)
            {
                Transform bone = FindChildRecursive(characterRoot.transform, boneName);
                if (bone != null)
                {
                    return bone;
                }
            }
            
            Animator animator = characterRoot.GetComponent<Animator>();
            if (animator != null && animator.isHuman)
            {
                Transform hipBone = animator.GetBoneTransform(HumanBodyBones.Hips);
                if (hipBone != null)
                {
                    return hipBone;
                }
            }
            
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

        void Update()
        {
            if (isInitialized && hasValidTarget)
            {
                UpdateCharacterPosition();
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
            info += $"Active: {characterRoot.activeInHierarchy}\n";
            
            if (hipBone != null)
            {
                info += $"Hip Bone: {hipBone.name}\n";
                info += $"Hip Position: {hipBone.position:F3}\n";
            }
            
            return info;
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
                characterAnimator = characterRoot.AddComponent<Animator>();
            }
            
            if (animatorOverrideController != null)
            {
                runtimeOverrideController = new AnimatorOverrideController(animatorOverrideController);
                characterAnimator.runtimeAnimatorController = runtimeOverrideController;
            }
            
            characterAnimator.enabled = true;
            characterAnimator.Rebind();
        }

        /// <summary>
        /// Start animation playback synchronized with hip recording
        /// </summary>
        public bool StartAnimationPlayback()
        {
            if (characterAnimator == null || runtimeOverrideController == null)
            {
                return false;
            }
            
            if (!LoadDefaultAnimation())
            {
                return false;
            }
            
            characterAnimator.enabled = true;
            characterAnimator.speed = 1.0f;
            
            string[] stateNames = { "CharacterAnimation", "climber1", "Base Layer.CharacterAnimation", "Base Layer.climber1" };
            
            foreach (string stateName in stateNames)
            {
                try
                {
                    characterAnimator.Play(stateName, 0, 0f);
                    characterAnimator.Update(0f);
                    
                    if (characterAnimator.layerCount > 0)
                    {
                        AnimatorStateInfo newStateInfo = characterAnimator.GetCurrentAnimatorStateInfo(0);
                        if (newStateInfo.length > 1.0f)
                        {
                            return true;
                        }
                    }
                }
                catch (System.Exception)
                {
                    continue;
                }
            }
            
            characterAnimator.Play(0, 0, 0f);
            characterAnimator.Update(0f);
            return true;
        }

        /// <summary>
        /// Stop animation playback
        /// </summary>
        public void StopAnimationPlayback()
        {
            if (characterAnimator != null)
            {
                characterAnimator.speed = 0f;
            }
        }

        /// <summary>
        /// Load animation from external FBX and apply to character
        /// </summary>
        public bool LoadAnimationFromFBX(string animationFbxPath, string animationName = null)
        {
            if (characterAnimator == null || runtimeOverrideController == null)
            {
                return false;
            }
            
            if (string.IsNullOrEmpty(animationName))
            {
                animationName = defaultAnimationClipName;
            }
            
            #if UNITY_EDITOR
            GameObject animationFbx = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(animationFbxPath);
            if (animationFbx == null)
            {
                return false;
            }
            
            AnimationClip animationClip = FindAnimationClipInFBX(animationFbx, animationName);
            if (animationClip == null)
            {
                return false;
            }
            
            return ApplyAnimationClip(animationClip, "CharacterAnimation");
            #else
            return false;
            #endif
        }

        /// <summary>
        /// Apply animation clip to character using override controller
        /// </summary>
        public bool ApplyAnimationClip(AnimationClip animationClip, string overrideSlotName = "CharacterAnimation")
        {
            if (runtimeOverrideController == null)
            {
                return false;
            }
            
            runtimeOverrideController[overrideSlotName] = animationClip;
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
                    if (clip.name == animationName || clip.name.Contains(animationName))
                    {
                        return clip;
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
            
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            runtimeOverrideController.GetOverrides(overrides);
            
            foreach (var pair in overrides)
            {
                if (pair.Key.name == "CharacterAnimation" && pair.Value != null)
                {
                    return true;
                }
            }
            
            string animationToLoad = defaultAnimationClipName;
            if (string.IsNullOrEmpty(animationToLoad))
            {
                animationToLoad = GetFirstAnimationFromNewAnimationFBX();
                if (string.IsNullOrEmpty(animationToLoad))
                {
                    return false;
                }
            }
            
            if (LoadAnimationFromFBX("Assets/DeepMotion/NewAnimationOnly.fbx", animationToLoad))
            {
                return true;
            }
            
            if (LoadAnimationFromFBX("Assets/DeepMotion/NewBody.fbx", animationToLoad))
            {
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Load animation from NewAnimationOnly.fbx
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
            
            return LoadAnimationFromFBX("Assets/DeepMotion/NewAnimationOnly.fbx", animationName);
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
        public bool LoadAnimationFromDeepMotionFBX(string deepMotionFbxPath, string animationName = "Take 001")
        {
            return LoadAnimationFromFBX(deepMotionFbxPath, animationName);
        }

        /// <summary>
        /// Get available animation clips from current override controller
        /// </summary>
        public string[] GetAvailableAnimationSlots()
        {
            if (runtimeOverrideController == null) return new string[0];
            
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
            GUILayout.Label("Animation Controls", UnityEditor.EditorStyles.boldLabel);
            
            FBXCharacterController controller = (FBXCharacterController)target;
            
            if (GUILayout.Button("Start Animation Playback"))
            {
                controller.StartAnimationPlayback();
            }
            
            if (GUILayout.Button("Stop Animation Playback"))
            {
                controller.StopAnimationPlayback();
            }
            
            if (GUILayout.Button("Load Animation from NewAnimationOnly.fbx"))
            {
                controller.LoadAnimationFromNewAnimationFBX();
            }
        }
    }
    #endif
} 