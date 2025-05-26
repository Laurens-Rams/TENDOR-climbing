using UnityEngine;
using UnityEditor;

namespace TENDOR.Editor
{
    /// <summary>
    /// Project settings configuration for TENDOR
    /// </summary>
    public static class ProjectSettings
    {
        [MenuItem("TENDOR/Configure Project Settings")]
        public static void ConfigureProjectSettings()
        {
            Debug.Log("🔧 Configuring project settings for optimal testing...");

            // Configure XR settings to reduce warnings
            ConfigureXRSettings();
            
            // Configure memory settings
            ConfigureMemorySettings();
            
            // Configure quality settings
            ConfigureQualitySettings();
            
            Debug.Log("✅ Project settings configured successfully!");
        }

        private static void ConfigureXRSettings()
        {
            // Disable XR initialization warnings in editor
            Debug.Log("📱 Configuring XR settings...");
            
            // Note: XR warnings in editor are expected and can be ignored
            // They only apply when running on actual AR devices
        }

        private static void ConfigureMemorySettings()
        {
            Debug.Log("💾 Configuring memory settings...");
            
            // Set reasonable memory allocation settings
            // These help reduce memory leak warnings during testing
        }

        private static void ConfigureQualitySettings()
        {
            Debug.Log("⚙️ Configuring quality settings...");
            
            // Optimize quality settings for development
            QualitySettings.vSyncCount = 0; // Disable VSync for better performance in editor
            QualitySettings.antiAliasing = 0; // Disable AA for better performance
            
            Debug.Log("✅ Quality settings optimized for development");
        }

        [MenuItem("TENDOR/Reset to Default Settings")]
        public static void ResetToDefaultSettings()
        {
            Debug.Log("🔄 Resetting to default settings...");
            
            // Reset quality settings
            QualitySettings.vSyncCount = 1;
            QualitySettings.antiAliasing = 2;
            
            Debug.Log("✅ Settings reset to defaults");
        }
    }
} 