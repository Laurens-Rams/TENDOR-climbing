using UnityEngine;
using UnityEngine.Rendering;

namespace BodyTracking.Utils
{
    /// <summary>
    /// Optimizes quality settings for AR Remote and device performance
    /// </summary>
    public class QualityOptimizer : MonoBehaviour
    {
        [Header("Quality Settings")]
        [SerializeField] private bool optimizeForARRemote = true;
        [SerializeField] private bool optimizeForDevice = true;
        
        [Header("Rendering Settings")]
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private int pixelLightCount = 2;
        [SerializeField] private ShadowQuality shadowQuality = ShadowQuality.Disable;
        [SerializeField] private int textureQuality = 0; // Full resolution
        [SerializeField] private AnisotropicFiltering anisotropicFiltering = AnisotropicFiltering.Disable;
        [SerializeField] private bool enableVSync = false;

        void Start()
        {
            if (optimizeForARRemote || optimizeForDevice)
            {
                ApplyOptimizations();
            }
        }

        [ContextMenu("Apply Optimizations")]
        public void ApplyOptimizations()
        {
           UnityEngine.Debug.Log("[QualityOptimizer] Applying quality optimizations...");
            
            // Frame rate
            Application.targetFrameRate = targetFrameRate;
            
            // Quality settings
            QualitySettings.pixelLightCount = pixelLightCount;
            QualitySettings.shadows = shadowQuality;
            QualitySettings.globalTextureMipmapLimit = textureQuality;
            QualitySettings.anisotropicFiltering = anisotropicFiltering;
            QualitySettings.vSyncCount = enableVSync ? 1 : 0;
            
            // Disable unnecessary features for AR
            QualitySettings.softParticles = false;
            QualitySettings.realtimeReflectionProbes = false;
            QualitySettings.billboardsFaceCameraPosition = false;
            
            // AR Remote specific optimizations
            if (optimizeForARRemote)
            {
                ApplyARRemoteOptimizations();
            }
            
            // Device specific optimizations
            if (optimizeForDevice)
            {
                ApplyDeviceOptimizations();
            }
            
           UnityEngine.Debug.Log("[QualityOptimizer] Quality optimizations applied");
        }

        private void ApplyARRemoteOptimizations()
        {
           UnityEngine.Debug.Log("[QualityOptimizer] Applying AR Remote optimizations...");
            
            // Lower quality for streaming
            QualitySettings.SetQualityLevel(2, false); // Medium quality
            
            // Reduce shadow distance
            QualitySettings.shadowDistance = 20f;
            
            // Optimize LOD bias
            QualitySettings.lodBias = 0.7f;
            
            // Reduce maximum LOD level
            QualitySettings.maximumLODLevel = 1;
        }

        private void ApplyDeviceOptimizations()
        {
           UnityEngine.Debug.Log("[QualityOptimizer] Applying device optimizations...");
            
            // Adjust based on device performance
            if (SystemInfo.processorCount <= 4)
            {
                // Lower-end device
                QualitySettings.SetQualityLevel(1, false); // Low quality
                Application.targetFrameRate = 30;
                QualitySettings.pixelLightCount = 1;
            }
            else
            {
                // Higher-end device
                QualitySettings.SetQualityLevel(3, false); // High quality
                Application.targetFrameRate = 60;
                QualitySettings.pixelLightCount = 2;
            }
            
            // Memory optimizations
            if (SystemInfo.systemMemorySize < 4096) // Less than 4GB RAM
            {
                QualitySettings.globalTextureMipmapLimit = 1; // Half resolution textures
                QualitySettings.particleRaycastBudget = 64;
            }
        }

        [ContextMenu("Reset to Default Quality")]
        public void ResetToDefaultQuality()
        {
            QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1, true);
            Application.targetFrameRate = -1;
           UnityEngine.Debug.Log("[QualityOptimizer] Reset to default quality settings");
        }

        void OnValidate()
        {
            // Ensure target frame rate is reasonable
            targetFrameRate = Mathf.Clamp(targetFrameRate, 15, 120);
        }
    }
} 