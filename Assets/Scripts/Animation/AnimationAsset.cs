using UnityEngine;

namespace BodyTracking.Animation
{
    /// <summary>
    /// ScriptableObject that represents an animation asset with metadata
    /// Can be created from DeepMotion API responses or local FBX files
    /// </summary>
    [CreateAssetMenu(fileName = "New Animation Asset", menuName = "TENDOR/Animation Asset")]
    public class AnimationAsset : ScriptableObject
    {
        [Header("Animation Info")]
        public string animationName;
        public string description;
        public AnimationClip clip;
        
        [Header("Source Info")]
        public AnimationSource source;
        public string sourcePath; // FBX path or DeepMotion job ID
        public string originalClipName; // Original name in FBX (e.g., "Take 001")
        
        [Header("Metadata")]
        public float duration;
        public float frameRate;
        public bool isLooping;
        public string[] tags; // For categorization (e.g., "climbing", "walking", "deepmotion")
        
        [Header("DeepMotion Specific")]
        public string deepMotionJobId;
        public string deepMotionPrompt;
        public System.DateTime createdDate;
        
        public enum AnimationSource
        {
            LocalFBX,
            DeepMotionAPI,
            Resources,
            AssetBundle
        }
        
        /// <summary>
        /// Validate the animation asset
        /// </summary>
        public bool IsValid()
        {
            return clip != null && !string.IsNullOrEmpty(animationName);
        }
        
        /// <summary>
        /// Get display name for UI
        /// </summary>
        public string GetDisplayName()
        {
            if (!string.IsNullOrEmpty(description))
                return $"{animationName} - {description}";
            return animationName;
        }
        
        /// <summary>
        /// Create animation asset from clip
        /// </summary>
        public static AnimationAsset CreateFromClip(AnimationClip clip, AnimationSource source = AnimationSource.LocalFBX)
        {
            var asset = CreateInstance<AnimationAsset>();
            asset.clip = clip;
            asset.animationName = clip.name;
            asset.source = source;
            asset.duration = clip.length;
            asset.frameRate = clip.frameRate;
            asset.isLooping = clip.isLooping;
            asset.createdDate = System.DateTime.Now;
            return asset;
        }
        
        /// <summary>
        /// Create animation asset for DeepMotion API result
        /// </summary>
        public static AnimationAsset CreateFromDeepMotion(AnimationClip clip, string jobId, string prompt)
        {
            var asset = CreateFromClip(clip, AnimationSource.DeepMotionAPI);
            asset.deepMotionJobId = jobId;
            asset.deepMotionPrompt = prompt;
            asset.description = $"Generated: {prompt}";
            asset.tags = new string[] { "deepmotion", "generated" };
            return asset;
        }
    }
} 