using UnityEngine;

namespace BodyTracking.AR
{
    /// <summary>
    /// Helper script for testing and adjusting wall rotation in real-time
    /// Attach this to a GameObject in your scene for quick rotation testing
    /// </summary>
    public class RotationTestHelper : MonoBehaviour
    {
        [Header("Rotation Testing")]
        [SerializeField] private ARImageTargetManager imageTargetManager;
        [SerializeField] private bool autoFindImageTargetManager = true;
        
        [Header("Quick Rotation Presets")]
        [SerializeField] private Vector3[] rotationPresets = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),      // No rotation
            new Vector3(90f, 0f, 0f),     // 90° X (wall upright)
            new Vector3(-90f, 0f, 0f),    // -90° X (wall upright, opposite)
            new Vector3(0f, 90f, 0f),     // 90° Y (wall rotated left)
            new Vector3(0f, -90f, 0f),    // -90° Y (wall rotated right)
            new Vector3(0f, 0f, 90f),     // 90° Z (wall tilted)
            new Vector3(90f, 180f, 0f),   // 90° X + 180° Y (wall upright, flipped)
        };
        
        [Header("Fine Tuning")]
        [SerializeField] private Vector3 customRotation = new Vector3(90f, 0f, 0f);
        [SerializeField] private float rotationStep = 15f; // Degrees per adjustment
        
        void Start()
        {
            if (imageTargetManager == null && autoFindImageTargetManager)
            {
                imageTargetManager = FindFirstObjectByType<ARImageTargetManager>();
            }
            
            if (imageTargetManager == null)
            {
                Debug.LogError("[RotationTestHelper] ARImageTargetManager not found!");
            }
            else
            {
                Debug.Log("[RotationTestHelper] Ready for rotation testing. Use the context menu or call methods from code.");
            }
        }
        
        [ContextMenu("Apply Preset 0: No Rotation")]
        public void ApplyPreset0() => ApplyRotationPreset(0);
        
        [ContextMenu("Apply Preset 1: 90° X (Wall Upright)")]
        public void ApplyPreset1() => ApplyRotationPreset(1);
        
        [ContextMenu("Apply Preset 2: -90° X (Wall Upright, Opposite)")]
        public void ApplyPreset2() => ApplyRotationPreset(2);
        
        [ContextMenu("Apply Preset 3: 90° Y (Wall Left)")]
        public void ApplyPreset3() => ApplyRotationPreset(3);
        
        [ContextMenu("Apply Preset 4: -90° Y (Wall Right)")]
        public void ApplyPreset4() => ApplyRotationPreset(4);
        
        [ContextMenu("Apply Preset 5: 90° Z (Wall Tilted)")]
        public void ApplyPreset5() => ApplyRotationPreset(5);
        
        [ContextMenu("Apply Preset 6: 90° X + 180° Y (Wall Upright, Flipped)")]
        public void ApplyPreset6() => ApplyRotationPreset(6);
        
        [ContextMenu("Apply Custom Rotation")]
        public void ApplyCustomRotation()
        {
            if (imageTargetManager != null)
            {
                imageTargetManager.UpdateWallRotation(customRotation);
                Debug.Log($"[RotationTestHelper] Applied custom rotation: {customRotation}");
            }
        }
        
        [ContextMenu("Rotate X +15°")]
        public void RotateXPlus() => AdjustRotation(rotationStep, 0, 0);
        
        [ContextMenu("Rotate X -15°")]
        public void RotateXMinus() => AdjustRotation(-rotationStep, 0, 0);
        
        [ContextMenu("Rotate Y +15°")]
        public void RotateYPlus() => AdjustRotation(0, rotationStep, 0);
        
        [ContextMenu("Rotate Y -15°")]
        public void RotateYMinus() => AdjustRotation(0, -rotationStep, 0);
        
        [ContextMenu("Rotate Z +15°")]
        public void RotateZPlus() => AdjustRotation(0, 0, rotationStep);
        
        [ContextMenu("Rotate Z -15°")]
        public void RotateZMinus() => AdjustRotation(0, 0, -rotationStep);
        
        public void ApplyRotationPreset(int presetIndex)
        {
            if (imageTargetManager == null)
            {
                Debug.LogError("[RotationTestHelper] ARImageTargetManager not found!");
                return;
            }
            
            if (presetIndex < 0 || presetIndex >= rotationPresets.Length)
            {
                Debug.LogError($"[RotationTestHelper] Invalid preset index: {presetIndex}");
                return;
            }
            
            Vector3 rotation = rotationPresets[presetIndex];
            imageTargetManager.UpdateWallRotation(rotation);
            customRotation = rotation; // Update custom rotation to match
            
            Debug.Log($"[RotationTestHelper] Applied rotation preset {presetIndex}: {rotation}");
        }
        
        public void AdjustRotation(float deltaX, float deltaY, float deltaZ)
        {
            if (imageTargetManager == null) return;
            
            customRotation += new Vector3(deltaX, deltaY, deltaZ);
            imageTargetManager.UpdateWallRotation(customRotation);
            
            Debug.Log($"[RotationTestHelper] Adjusted rotation by ({deltaX}, {deltaY}, {deltaZ}). New rotation: {customRotation}");
        }
        
        public void SetRotation(Vector3 rotation)
        {
            if (imageTargetManager == null) return;
            
            customRotation = rotation;
            imageTargetManager.UpdateWallRotation(customRotation);
            
            Debug.Log($"[RotationTestHelper] Set rotation to: {customRotation}");
        }
        
        [ContextMenu("Print Current Rotation")]
        public void PrintCurrentRotation()
        {
            if (imageTargetManager != null && imageTargetManager.ImageTargetTransform != null)
            {
                var activeContent = imageTargetManager.ImageTargetTransform.GetChild(0);
                if (activeContent != null)
                {
                    Debug.Log($"[RotationTestHelper] Current wall rotation: {activeContent.localRotation.eulerAngles}");
                    Debug.Log($"[RotationTestHelper] Current wall position: {activeContent.localPosition}");
                    Debug.Log($"[RotationTestHelper] Current wall scale: {activeContent.localScale}");
                }
            }
        }
    }
} 