using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace BodyTracking.Animation
{
    /// <summary>
    /// Central manager for handling runtime animation and character switching
    /// Supports DeepMotion API integration and scalable asset management
    /// </summary>
    public class AnimationManager : MonoBehaviour
    {
        [Header("Asset Libraries")]
        [SerializeField] private AnimationAsset[] availableAnimations;
        [SerializeField] private CharacterAsset[] availableCharacters;
        
        [Header("Current State")]
        [SerializeField] private CharacterAsset currentCharacterAsset;
        [SerializeField] private AnimationAsset currentAnimationAsset;
        
        [Header("Runtime References")]
        [SerializeField] private FBXCharacterController characterController;
        
        [Header("DeepMotion Integration")]
        [SerializeField] private string deepMotionApiKey;
        [SerializeField] private bool autoLoadDeepMotionAnimations = true;
        
        [Header("Debug")]
        [SerializeField] private bool enableLogging = true;
        
        // Events
        public System.Action<AnimationAsset> OnAnimationChanged;
        public System.Action<CharacterAsset> OnCharacterChanged;
        public System.Action<AnimationAsset> OnDeepMotionAnimationLoaded;
        
        // Runtime state
        private Dictionary<string, AnimationAsset> animationLibrary = new Dictionary<string, AnimationAsset>();
        private Dictionary<string, CharacterAsset> characterLibrary = new Dictionary<string, CharacterAsset>();
        private Queue<System.Action> pendingOperations = new Queue<System.Action>();
        
        void Start()
        {
            InitializeLibraries();
            
            // Auto-setup if character controller is assigned
            if (characterController != null)
            {
                SetupWithCurrentCharacter();
            }
        }
        
        void Update()
        {
            // Process pending operations (for async operations)
            if (pendingOperations.Count > 0)
            {
                var operation = pendingOperations.Dequeue();
                operation?.Invoke();
            }
        }
        
        #region Library Management
        
        /// <summary>
        /// Initialize animation and character libraries
        /// </summary>
        private void InitializeLibraries()
        {
            // Build animation library
            animationLibrary.Clear();
            if (availableAnimations != null)
            {
                foreach (var animation in availableAnimations)
                {
                    if (animation != null && animation.IsValid())
                    {
                        animationLibrary[animation.animationName] = animation;
                    }
                }
            }
            
            // Build character library
            characterLibrary.Clear();
            if (availableCharacters != null)
            {
                foreach (var character in availableCharacters)
                {
                    if (character != null && character.IsValid())
                    {
                        characterLibrary[character.characterName] = character;
                    }
                }
            }
            
            if (enableLogging)
            {
                Debug.Log($"[AnimationManager] Initialized with {animationLibrary.Count} animations and {characterLibrary.Count} characters");
            }
        }
        
        /// <summary>
        /// Add animation to library at runtime
        /// </summary>
        public void AddAnimationToLibrary(AnimationAsset animation)
        {
            if (animation == null || !animation.IsValid())
            {
                Debug.LogError("[AnimationManager] Cannot add invalid animation to library");
                return;
            }
            
            animationLibrary[animation.animationName] = animation;
            
            if (enableLogging)
            {
                Debug.Log($"[AnimationManager] Added animation '{animation.animationName}' to library");
            }
        }
        
        /// <summary>
        /// Add character to library at runtime
        /// </summary>
        public void AddCharacterToLibrary(CharacterAsset character)
        {
            if (character == null || !character.IsValid())
            {
                Debug.LogError("[AnimationManager] Cannot add invalid character to library");
                return;
            }
            
            characterLibrary[character.characterName] = character;
            
            if (enableLogging)
            {
                Debug.Log($"[AnimationManager] Added character '{character.characterName}' to library");
            }
        }
        
        #endregion
        
        #region Animation Switching
        
        /// <summary>
        /// Switch to a different animation (keeping current character)
        /// </summary>
        public bool SwitchAnimation(string animationName)
        {
            if (!animationLibrary.TryGetValue(animationName, out AnimationAsset animation))
            {
                Debug.LogError($"[AnimationManager] Animation '{animationName}' not found in library");
                return false;
            }
            
            return SwitchAnimation(animation);
        }
        
        /// <summary>
        /// Switch to a different animation (keeping current character)
        /// </summary>
        public bool SwitchAnimation(AnimationAsset animation)
        {
            if (animation == null || !animation.IsValid())
            {
                Debug.LogError("[AnimationManager] Cannot switch to invalid animation");
                return false;
            }
            
            // Check compatibility with current character
            if (currentCharacterAsset != null && !currentCharacterAsset.IsCompatibleWith(animation))
            {
                Debug.LogWarning($"[AnimationManager] Animation '{animation.animationName}' may not be compatible with character '{currentCharacterAsset.characterName}'");
            }
            
            // Apply animation to character controller
            if (characterController != null)
            {
                bool success = characterController.ApplyAnimationClip(animation.clip, "CharacterAnimation");
                if (!success)
                {
                    Debug.LogError($"[AnimationManager] Failed to apply animation '{animation.animationName}' to character controller");
                    return false;
                }
            }
            
            currentAnimationAsset = animation;
            OnAnimationChanged?.Invoke(animation);
            
            if (enableLogging)
            {
                Debug.Log($"[AnimationManager] Switched to animation '{animation.animationName}'");
            }
            
            return true;
        }
        
        #endregion
        
        #region Character Switching
        
        /// <summary>
        /// Switch to a different character (with optional animation)
        /// </summary>
        public bool SwitchCharacter(string characterName, string animationName = null)
        {
            if (!characterLibrary.TryGetValue(characterName, out CharacterAsset character))
            {
                Debug.LogError($"[AnimationManager] Character '{characterName}' not found in library");
                return false;
            }
            
            AnimationAsset animation = null;
            if (!string.IsNullOrEmpty(animationName))
            {
                if (!animationLibrary.TryGetValue(animationName, out animation))
                {
                    Debug.LogWarning($"[AnimationManager] Animation '{animationName}' not found, using character's default");
                }
            }
            
            return SwitchCharacter(character, animation);
        }
        
        /// <summary>
        /// Switch to a different character (with optional animation)
        /// </summary>
        public bool SwitchCharacter(CharacterAsset character, AnimationAsset animation = null)
        {
            if (character == null || !character.IsValid())
            {
                Debug.LogError("[AnimationManager] Cannot switch to invalid character");
                return false;
            }
            
            // Use character's default animation if none specified
            if (animation == null)
            {
                animation = character.defaultAnimation;
            }
            
            // Validate animation compatibility
            if (animation != null && !character.IsCompatibleWith(animation))
            {
                Debug.LogWarning($"[AnimationManager] Animation '{animation.animationName}' may not be compatible with character '{character.characterName}'");
                animation = character.defaultAnimation; // Fallback to default
            }
            
            // Instantiate new character
            GameObject newCharacterInstance = InstantiateCharacter(character);
            if (newCharacterInstance == null)
            {
                Debug.LogError($"[AnimationManager] Failed to instantiate character '{character.characterName}'");
                return false;
            }
            
            // Setup character controller with new character
            if (characterController != null)
            {
                // Destroy old character if it exists
                if (characterController.CharacterRoot != null)
                {
                    DestroyImmediate(characterController.CharacterRoot);
                }
                
                // Set new character
                characterController.SetCharacter(newCharacterInstance);
                
                // Apply animation if specified
                if (animation != null)
                {
                    characterController.ApplyAnimationClip(animation.clip, "CharacterAnimation");
                    currentAnimationAsset = animation;
                }
            }
            
            currentCharacterAsset = character;
            OnCharacterChanged?.Invoke(character);
            
            if (animation != null)
            {
                OnAnimationChanged?.Invoke(animation);
            }
            
            if (enableLogging)
            {
                Debug.Log($"[AnimationManager] Switched to character '{character.characterName}'" + 
                         (animation != null ? $" with animation '{animation.animationName}'" : ""));
            }
            
            return true;
        }
        
        /// <summary>
        /// Instantiate character from asset
        /// </summary>
        private GameObject InstantiateCharacter(CharacterAsset character)
        {
            if (character.characterPrefab == null)
            {
                Debug.LogError($"[AnimationManager] Character '{character.characterName}' has no prefab assigned");
                return null;
            }
            
            GameObject instance = Instantiate(character.characterPrefab);
            instance.name = character.characterName; // Remove "(Clone)"
            
            // Apply character settings
            instance.transform.localScale = character.defaultScale;
            instance.transform.position += character.positionOffset;
            
            // Apply materials if specified
            if (character.defaultMaterials != null && character.defaultMaterials.Length > 0)
            {
                Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    if (character.defaultMaterials.Length == 1)
                    {
                        renderer.material = character.defaultMaterials[0];
                    }
                    else if (renderer.materials.Length <= character.defaultMaterials.Length)
                    {
                        renderer.materials = character.defaultMaterials;
                    }
                }
            }
            
            return instance;
        }
        
        #endregion
        
        #region DeepMotion Integration
        
        /// <summary>
        /// Load animation from DeepMotion API result
        /// This would be called after receiving animation from DeepMotion API
        /// </summary>
        public void LoadDeepMotionAnimation(AnimationClip clip, string jobId, string prompt, bool switchToIt = true)
        {
            if (clip == null)
            {
                Debug.LogError("[AnimationManager] Cannot load null animation clip from DeepMotion");
                return;
            }
            
            // Create animation asset
            AnimationAsset animationAsset = AnimationAsset.CreateFromDeepMotion(clip, jobId, prompt);
            animationAsset.name = $"DeepMotion_{jobId}";
            
            // Add to library
            AddAnimationToLibrary(animationAsset);
            
            // Switch to it if requested
            if (switchToIt)
            {
                SwitchAnimation(animationAsset);
            }
            
            OnDeepMotionAnimationLoaded?.Invoke(animationAsset);
            
            if (enableLogging)
            {
                Debug.Log($"[AnimationManager] Loaded DeepMotion animation '{animationAsset.animationName}' from job '{jobId}'");
            }
        }
        
        /// <summary>
        /// Placeholder for DeepMotion API integration
        /// You would implement the actual API calls here
        /// </summary>
        public void RequestDeepMotionAnimation(string prompt)
        {
            if (enableLogging)
            {
                Debug.Log($"[AnimationManager] Requesting DeepMotion animation with prompt: '{prompt}'");
            }
            
            // TODO: Implement actual DeepMotion API integration
            // This would involve:
            // 1. Making HTTP request to DeepMotion API
            // 2. Polling for job completion
            // 3. Downloading resulting FBX/animation
            // 4. Converting to AnimationClip
            // 5. Calling LoadDeepMotionAnimation()
            
            Debug.LogWarning("[AnimationManager] DeepMotion API integration not yet implemented");
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Setup with current character in scene
        /// </summary>
        private void SetupWithCurrentCharacter()
        {
            if (characterController == null || characterController.CharacterRoot == null)
                return;
                
            // Try to find matching character asset
            string characterName = characterController.CharacterRoot.name;
            if (characterLibrary.TryGetValue(characterName, out CharacterAsset character))
            {
                currentCharacterAsset = character;
                
                // Use default animation if available
                if (character.defaultAnimation != null)
                {
                    SwitchAnimation(character.defaultAnimation);
                }
            }
        }
        
        /// <summary>
        /// Get all available animation names
        /// </summary>
        public string[] GetAvailableAnimationNames()
        {
            var names = new string[animationLibrary.Count];
            animationLibrary.Keys.CopyTo(names, 0);
            return names;
        }
        
        /// <summary>
        /// Get all available character names
        /// </summary>
        public string[] GetAvailableCharacterNames()
        {
            var names = new string[characterLibrary.Count];
            characterLibrary.Keys.CopyTo(names, 0);
            return names;
        }
        
        /// <summary>
        /// Get current animation info
        /// </summary>
        public string GetCurrentAnimationInfo()
        {
            if (currentAnimationAsset == null)
                return "No animation loaded";
                
            return $"Animation: {currentAnimationAsset.GetDisplayName()}\n" +
                   $"Duration: {currentAnimationAsset.duration:F2}s\n" +
                   $"Source: {currentAnimationAsset.source}";
        }
        
        /// <summary>
        /// Get current character info
        /// </summary>
        public string GetCurrentCharacterInfo()
        {
            if (currentCharacterAsset == null)
                return "No character loaded";
                
            return $"Character: {currentCharacterAsset.GetDisplayName()}";
        }
        
        #endregion
    }
} 