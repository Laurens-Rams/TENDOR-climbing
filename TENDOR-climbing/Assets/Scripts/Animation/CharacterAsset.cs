using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace BodyTracking.Animation
{
    /// <summary>
    /// ScriptableObject that represents a character asset with metadata
    /// Allows for runtime character swapping
    /// </summary>
    [CreateAssetMenu(fileName = "New Character Asset", menuName = "TENDOR/Character Asset")]
    public class CharacterAsset : ScriptableObject
    {
        [Header("Character Info")]
        public string characterName;
        public string description;
        public GameObject characterPrefab;
        
        [Header("Bone Configuration")]
        public string[] hipBoneNames = { "Hips Node", "Hips", "Hip", "Pelvis", "mixamorig:Hips", "Root" };
        public string preferredHipBoneName; // If you know the exact bone name
        
        [Header("Animation Compatibility")]
#if UNITY_EDITOR
        public AnimatorController defaultAnimatorController;
#endif
        public AnimationAsset[] compatibleAnimations;
        public AnimationAsset defaultAnimation;
        
        [Header("Visual Settings")]
        public Material[] defaultMaterials;
        public Vector3 defaultScale = Vector3.one;
        public Vector3 positionOffset = Vector3.zero;
        
        [Header("Metadata")]
        public string[] tags; // For categorization (e.g., "male", "female", "robot", "human")
        public System.DateTime createdDate;
        
        /// <summary>
        /// Validate the character asset
        /// </summary>
        public bool IsValid()
        {
            return characterPrefab != null && !string.IsNullOrEmpty(characterName);
        }
        
        /// <summary>
        /// Get display name for UI
        /// </summary>
        public string GetDisplayName()
        {
            if (!string.IsNullOrEmpty(description))
                return $"{characterName} - {description}";
            return characterName;
        }
        
        /// <summary>
        /// Check if this character is compatible with an animation
        /// </summary>
        public bool IsCompatibleWith(AnimationAsset animation)
        {
            if (compatibleAnimations == null || compatibleAnimations.Length == 0)
                return true; // Assume compatible if no restrictions
                
            foreach (var compatibleAnim in compatibleAnimations)
            {
                if (compatibleAnim == animation)
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Create character asset from prefab
        /// </summary>
        public static CharacterAsset CreateFromPrefab(GameObject prefab)
        {
            var asset = CreateInstance<CharacterAsset>();
            asset.characterPrefab = prefab;
            asset.characterName = prefab.name;
            asset.createdDate = System.DateTime.Now;
            
            // Try to auto-detect animator controller
            var animator = prefab.GetComponent<Animator>();
#if UNITY_EDITOR
            if (animator != null && animator.runtimeAnimatorController is AnimatorController controller)
            {
                asset.defaultAnimatorController = controller;
            }
#endif
            
            return asset;
        }
    }
} 