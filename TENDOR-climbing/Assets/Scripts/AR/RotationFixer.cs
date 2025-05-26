using UnityEngine;

namespace BodyTracking.AR
{
    /// <summary>
    /// Temporary script to fix rotation issues by forcing the correct rotation at startup
    /// Add this to any GameObject in your scene to automatically apply the rotation fix
    /// </summary>
    public class RotationFixer : MonoBehaviour
    {
        [Header("Rotation Fix")]
        [SerializeField] private Vector3 correctRotation = new Vector3(90f, 0f, 0f);
        [SerializeField] private bool applyOnStart = true;
        [SerializeField] private bool findImageTargetManagerAutomatically = true;
        
        [Header("Manual Assignment")]
        [SerializeField] private ARImageTargetManager imageTargetManager;
        
        void Start()
        {
            if (applyOnStart)
            {
                ApplyRotationFix();
            }
        }
        
        [ContextMenu("Apply Rotation Fix Now")]
        public void ApplyRotationFix()
        {
            // Find the ARImageTargetManager if not assigned
            if (imageTargetManager == null && findImageTargetManagerAutomatically)
            {
                imageTargetManager = FindObjectOfType<ARImageTargetManager>();
            }
            
            if (imageTargetManager == null)
            {
                Debug.LogError("[RotationFixer] ARImageTargetManager not found! Please assign it manually or make sure it exists in the scene.");
                return;
            }
            
            // Force update the rotation
            imageTargetManager.UpdateWallRotation(correctRotation);
            
            Debug.Log($"[RotationFixer] ✅ Applied rotation fix: {correctRotation}");
            Debug.Log("[RotationFixer] Wall should now stand upright instead of tilting towards you.");
        }
        
        [ContextMenu("Test Different Rotations")]
        public void TestRotations()
        {
            if (imageTargetManager == null)
            {
                imageTargetManager = FindObjectOfType<ARImageTargetManager>();
            }
            
            if (imageTargetManager == null)
            {
                Debug.LogError("[RotationFixer] ARImageTargetManager not found!");
                return;
            }
            
            Debug.Log("[RotationFixer] Testing different rotations...");
            
            // Test the main fix
            imageTargetManager.UpdateWallRotation(new Vector3(90f, 0f, 0f));
            Debug.Log("[RotationFixer] Applied 90° X rotation (wall upright)");
        }
        
        [ContextMenu("Try Alternative Rotation")]
        public void TryAlternativeRotation()
        {
            if (imageTargetManager == null)
            {
                imageTargetManager = FindObjectOfType<ARImageTargetManager>();
            }
            
            if (imageTargetManager == null)
            {
                Debug.LogError("[RotationFixer] ARImageTargetManager not found!");
                return;
            }
            
            // Try the opposite rotation
            Vector3 alternativeRotation = new Vector3(-90f, 0f, 0f);
            imageTargetManager.UpdateWallRotation(alternativeRotation);
            
            Debug.Log($"[RotationFixer] Applied alternative rotation: {alternativeRotation}");
        }
        
        [ContextMenu("Reset to No Rotation")]
        public void ResetRotation()
        {
            if (imageTargetManager == null)
            {
                imageTargetManager = FindObjectOfType<ARImageTargetManager>();
            }
            
            if (imageTargetManager == null)
            {
                Debug.LogError("[RotationFixer] ARImageTargetManager not found!");
                return;
            }
            
            imageTargetManager.UpdateWallRotation(Vector3.zero);
            Debug.Log("[RotationFixer] Reset rotation to zero (original tilted state)");
        }
        
        /// <summary>
        /// Set a custom rotation
        /// </summary>
        public void SetCustomRotation(Vector3 rotation)
        {
            correctRotation = rotation;
            ApplyRotationFix();
        }
        
        /// <summary>
        /// Quick method to apply the standard fix
        /// </summary>
        public static void ApplyStandardFix()
        {
            var imageTargetManager = FindObjectOfType<ARImageTargetManager>();
            if (imageTargetManager != null)
            {
                imageTargetManager.UpdateWallRotation(new Vector3(90f, 0f, 0f));
                Debug.Log("[RotationFixer] Applied standard rotation fix (90° X)");
            }
            else
            {
                Debug.LogError("[RotationFixer] ARImageTargetManager not found!");
            }
        }
    }
} 