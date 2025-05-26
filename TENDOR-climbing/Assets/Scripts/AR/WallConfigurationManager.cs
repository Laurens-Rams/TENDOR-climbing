using UnityEngine;
using BodyTracking.AR;

namespace BodyTracking.AR
{
    /// <summary>
    /// Manages wall configuration settings for different climbing walls
    /// Allows easy adjustment of wall dimensions, rotation, and positioning
    /// </summary>
    public class WallConfigurationManager : MonoBehaviour
    {
        [Header("Wall Presets")]
        [SerializeField] private WallPreset[] wallPresets = new WallPreset[]
        {
            new WallPreset { name = "Small Wall", size = new Vector2(1f, 2f), rotation = new Vector3(90f, 0f, 0f), position = Vector3.zero },
            new WallPreset { name = "Medium Wall", size = new Vector2(2f, 3f), rotation = new Vector3(90f, 0f, 0f), position = Vector3.zero },
            new WallPreset { name = "Large Wall", size = new Vector2(3f, 4f), rotation = new Vector3(90f, 0f, 0f), position = Vector3.zero }
        };
        
        [Header("Current Configuration")]
        [SerializeField] private int selectedPresetIndex = 1; // Default to medium wall
        [SerializeField] private Vector2 customWallSize = new Vector2(2f, 3f);
        [SerializeField] private Vector3 customRotation = new Vector3(90f, 0f, 0f);
        [SerializeField] private Vector3 customPosition = Vector3.zero;
        [SerializeField] private bool useCustomSettings = false;
        
        [Header("References")]
        [SerializeField] private ARImageTargetManager imageTargetManager;
        [SerializeField] private bool autoFindImageTargetManager = true;
        
        // Events
        public event System.Action<Vector2> OnWallSizeChanged;
        public event System.Action<Vector3> OnWallRotationChanged;
        public event System.Action<Vector3> OnWallPositionChanged;
        
        // Properties
        public Vector2 CurrentWallSize => useCustomSettings ? customWallSize : wallPresets[selectedPresetIndex].size;
        public Vector3 CurrentRotation => useCustomSettings ? customRotation : wallPresets[selectedPresetIndex].rotation;
        public Vector3 CurrentPosition => useCustomSettings ? customPosition : wallPresets[selectedPresetIndex].position;
        public string CurrentPresetName => useCustomSettings ? "Custom" : wallPresets[selectedPresetIndex].name;

        void Start()
        {
            if (imageTargetManager == null)
            {
                imageTargetManager = FindFirstObjectByType<ARImageTargetManager>();
            }
            
            if (imageTargetManager == null)
            {
                Debug.LogError("[WallConfigurationManager] No ARImageTargetManager found in scene!");
                return;
            }
            
            // Apply initial configuration
            ApplyCurrentConfiguration();
            
            Debug.Log($"[WallConfigurationManager] Initialized with preset: {CurrentPresetName}");
        }

        /// <summary>
        /// Apply the current configuration to the image target manager
        /// </summary>
        public void ApplyCurrentConfiguration()
        {
            if (imageTargetManager == null) return;
            
            imageTargetManager.UpdateWallSize(CurrentWallSize);
            imageTargetManager.UpdateWallRotation(CurrentRotation);
            imageTargetManager.UpdateWallPosition(CurrentPosition);
            
            // Trigger events
            OnWallSizeChanged?.Invoke(CurrentWallSize);
            OnWallRotationChanged?.Invoke(CurrentRotation);
            OnWallPositionChanged?.Invoke(CurrentPosition);
            
            Debug.Log($"[WallConfigurationManager] Applied configuration: {CurrentPresetName} - Size: {CurrentWallSize}, Rotation: {CurrentRotation}, Position: {CurrentPosition}");
        }

        /// <summary>
        /// Select a wall preset by index
        /// </summary>
        public void SelectPreset(int presetIndex)
        {
            if (presetIndex < 0 || presetIndex >= wallPresets.Length)
            {
                Debug.LogWarning($"[WallConfigurationManager] Invalid preset index: {presetIndex}");
                return;
            }
            
            selectedPresetIndex = presetIndex;
            useCustomSettings = false;
            ApplyCurrentConfiguration();
        }

        /// <summary>
        /// Select a wall preset by name
        /// </summary>
        public void SelectPreset(string presetName)
        {
            for (int i = 0; i < wallPresets.Length; i++)
            {
                if (wallPresets[i].name.Equals(presetName, System.StringComparison.OrdinalIgnoreCase))
                {
                    SelectPreset(i);
                    return;
                }
            }
            
            Debug.LogWarning($"[WallConfigurationManager] Preset not found: {presetName}");
        }

        /// <summary>
        /// Set custom wall size
        /// </summary>
        public void SetCustomWallSize(Vector2 size)
        {
            customWallSize = size;
            useCustomSettings = true;
            ApplyCurrentConfiguration();
        }

        /// <summary>
        /// Set custom wall size with individual dimensions
        /// </summary>
        public void SetCustomWallSize(float width, float height)
        {
            SetCustomWallSize(new Vector2(width, height));
        }

        /// <summary>
        /// Set custom rotation
        /// </summary>
        public void SetCustomRotation(Vector3 rotation)
        {
            customRotation = rotation;
            useCustomSettings = true;
            ApplyCurrentConfiguration();
        }

        /// <summary>
        /// Set custom position
        /// </summary>
        public void SetCustomPosition(Vector3 position)
        {
            customPosition = position;
            useCustomSettings = true;
            ApplyCurrentConfiguration();
        }

        /// <summary>
        /// Reset to default medium wall preset
        /// </summary>
        public void ResetToDefault()
        {
            selectedPresetIndex = 1; // Medium wall
            useCustomSettings = false;
            ApplyCurrentConfiguration();
        }

        /// <summary>
        /// Get all available preset names
        /// </summary>
        public string[] GetPresetNames()
        {
            string[] names = new string[wallPresets.Length];
            for (int i = 0; i < wallPresets.Length; i++)
            {
                names[i] = wallPresets[i].name;
            }
            return names;
        }

        /// <summary>
        /// Add a new preset
        /// </summary>
        public void AddPreset(string name, Vector2 size, Vector3 rotation = default, Vector3 position = default)
        {
            var newPreset = new WallPreset
            {
                name = name,
                size = size,
                rotation = rotation,
                position = position
            };
            
            // Resize array and add new preset
            var newPresets = new WallPreset[wallPresets.Length + 1];
            System.Array.Copy(wallPresets, newPresets, wallPresets.Length);
            newPresets[wallPresets.Length] = newPreset;
            wallPresets = newPresets;
            
            Debug.Log($"[WallConfigurationManager] Added new preset: {name}");
        }

        /// <summary>
        /// Get configuration summary for debugging
        /// </summary>
        public string GetConfigurationSummary()
        {
            return $"Wall Configuration:\n" +
                   $"  Preset: {CurrentPresetName}\n" +
                   $"  Size: {CurrentWallSize.x}m x {CurrentWallSize.y}m\n" +
                   $"  Rotation: {CurrentRotation}\n" +
                   $"  Position: {CurrentPosition}\n" +
                   $"  Custom Settings: {useCustomSettings}";
        }

        // Inspector methods for testing
        [ContextMenu("Apply Configuration")]
        private void ApplyConfigurationFromInspector()
        {
            ApplyCurrentConfiguration();
        }

        [ContextMenu("Print Configuration")]
        private void PrintConfiguration()
        {
            Debug.Log(GetConfigurationSummary());
        }
    }

    /// <summary>
    /// Wall preset configuration
    /// </summary>
    [System.Serializable]
    public class WallPreset
    {
        public string name;
        public Vector2 size;
        public Vector3 rotation;
        public Vector3 position;
    }
} 