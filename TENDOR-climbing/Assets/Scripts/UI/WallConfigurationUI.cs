using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BodyTracking.AR;

namespace BodyTracking.UI
{
    /// <summary>
    /// UI for configuring wall settings during development and testing
    /// </summary>
    public class WallConfigurationUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Dropdown presetDropdown;
        [SerializeField] private Slider widthSlider;
        [SerializeField] private Slider heightSlider;
        [SerializeField] private TMP_Text widthValueText;
        [SerializeField] private TMP_Text heightValueText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Toggle customModeToggle;
        
        [Header("Rotation Controls")]
        [SerializeField] private Slider rotationXSlider;
        [SerializeField] private Slider rotationYSlider;
        [SerializeField] private Slider rotationZSlider;
        [SerializeField] private TMP_Text rotationXText;
        [SerializeField] private TMP_Text rotationYText;
        [SerializeField] private TMP_Text rotationZText;
        
        [Header("Position Controls")]
        [SerializeField] private Slider positionXSlider;
        [SerializeField] private Slider positionYSlider;
        [SerializeField] private Slider positionZSlider;
        [SerializeField] private TMP_Text positionXText;
        [SerializeField] private TMP_Text positionYText;
        [SerializeField] private TMP_Text positionZText;
        
        [Header("Settings")]
        [SerializeField] private float minWallSize = 0.5f;
        [SerializeField] private float maxWallSize = 5f;
        [SerializeField] private float rotationRange = 180f;
        [SerializeField] private float positionRange = 2f;
        
        // References
        private WallConfigurationManager configManager;
        private bool isInitialized = false;

        void Start()
        {
            configManager = FindObjectOfType<WallConfigurationManager>();
            if (configManager == null)
            {
                Debug.LogError("[WallConfigurationUI] WallConfigurationManager not found!");
                if (statusText != null)
                    statusText.text = "Error: Wall Configuration Manager not found";
                return;
            }
            
            InitializeUI();
            SetupEventListeners();
            UpdateUIFromConfiguration();
            
            isInitialized = true;
            Debug.Log("[WallConfigurationUI] Initialized successfully");
        }

        /// <summary>
        /// Initialize UI components
        /// </summary>
        private void InitializeUI()
        {
            // Setup preset dropdown
            if (presetDropdown != null)
            {
                presetDropdown.ClearOptions();
                var presetNames = configManager.GetPresetNames();
                presetDropdown.AddOptions(new System.Collections.Generic.List<string>(presetNames));
            }
            
            // Setup sliders
            if (widthSlider != null)
            {
                widthSlider.minValue = minWallSize;
                widthSlider.maxValue = maxWallSize;
            }
            
            if (heightSlider != null)
            {
                heightSlider.minValue = minWallSize;
                heightSlider.maxValue = maxWallSize;
            }
            
            // Setup rotation sliders
            if (rotationXSlider != null)
            {
                rotationXSlider.minValue = -rotationRange;
                rotationXSlider.maxValue = rotationRange;
            }
            
            if (rotationYSlider != null)
            {
                rotationYSlider.minValue = -rotationRange;
                rotationYSlider.maxValue = rotationRange;
            }
            
            if (rotationZSlider != null)
            {
                rotationZSlider.minValue = -rotationRange;
                rotationZSlider.maxValue = rotationRange;
            }
            
            // Setup position sliders
            if (positionXSlider != null)
            {
                positionXSlider.minValue = -positionRange;
                positionXSlider.maxValue = positionRange;
            }
            
            if (positionYSlider != null)
            {
                positionYSlider.minValue = -positionRange;
                positionYSlider.maxValue = positionRange;
            }
            
            if (positionZSlider != null)
            {
                positionZSlider.minValue = -positionRange;
                positionZSlider.maxValue = positionRange;
            }
        }

        /// <summary>
        /// Setup event listeners for UI components
        /// </summary>
        private void SetupEventListeners()
        {
            if (presetDropdown != null)
                presetDropdown.onValueChanged.AddListener(OnPresetChanged);
            
            if (widthSlider != null)
                widthSlider.onValueChanged.AddListener(OnWidthChanged);
            
            if (heightSlider != null)
                heightSlider.onValueChanged.AddListener(OnHeightChanged);
            
            if (rotationXSlider != null)
                rotationXSlider.onValueChanged.AddListener(OnRotationXChanged);
            
            if (rotationYSlider != null)
                rotationYSlider.onValueChanged.AddListener(OnRotationYChanged);
            
            if (rotationZSlider != null)
                rotationZSlider.onValueChanged.AddListener(OnRotationZChanged);
            
            if (positionXSlider != null)
                positionXSlider.onValueChanged.AddListener(OnPositionXChanged);
            
            if (positionYSlider != null)
                positionYSlider.onValueChanged.AddListener(OnPositionYChanged);
            
            if (positionZSlider != null)
                positionZSlider.onValueChanged.AddListener(OnPositionZChanged);
            
            if (applyButton != null)
                applyButton.onClick.AddListener(OnApplyClicked);
            
            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetClicked);
            
            if (customModeToggle != null)
                customModeToggle.onValueChanged.AddListener(OnCustomModeToggled);
        }

        /// <summary>
        /// Update UI to reflect current configuration
        /// </summary>
        private void UpdateUIFromConfiguration()
        {
            if (configManager == null) return;
            
            var currentSize = configManager.CurrentWallSize;
            var currentRotation = configManager.CurrentRotation;
            var currentPosition = configManager.CurrentPosition;
            
            // Update size sliders
            if (widthSlider != null)
                widthSlider.value = currentSize.x;
            
            if (heightSlider != null)
                heightSlider.value = currentSize.y;
            
            // Update rotation sliders
            if (rotationXSlider != null)
                rotationXSlider.value = currentRotation.x;
            
            if (rotationYSlider != null)
                rotationYSlider.value = currentRotation.y;
            
            if (rotationZSlider != null)
                rotationZSlider.value = currentRotation.z;
            
            // Update position sliders
            if (positionXSlider != null)
                positionXSlider.value = currentPosition.x;
            
            if (positionYSlider != null)
                positionYSlider.value = currentPosition.y;
            
            if (positionZSlider != null)
                positionZSlider.value = currentPosition.z;
            
            // Update text displays
            UpdateValueTexts();
            UpdateStatusText();
        }

        /// <summary>
        /// Update value text displays
        /// </summary>
        private void UpdateValueTexts()
        {
            if (widthValueText != null && widthSlider != null)
                widthValueText.text = $"{widthSlider.value:F1}m";
            
            if (heightValueText != null && heightSlider != null)
                heightValueText.text = $"{heightSlider.value:F1}m";
            
            if (rotationXText != null && rotationXSlider != null)
                rotationXText.text = $"{rotationXSlider.value:F0}°";
            
            if (rotationYText != null && rotationYSlider != null)
                rotationYText.text = $"{rotationYSlider.value:F0}°";
            
            if (rotationZText != null && rotationZSlider != null)
                rotationZText.text = $"{rotationZSlider.value:F0}°";
            
            if (positionXText != null && positionXSlider != null)
                positionXText.text = $"{positionXSlider.value:F2}m";
            
            if (positionYText != null && positionYSlider != null)
                positionYText.text = $"{positionYSlider.value:F2}m";
            
            if (positionZText != null && positionZSlider != null)
                positionZText.text = $"{positionZSlider.value:F2}m";
        }

        /// <summary>
        /// Update status text
        /// </summary>
        private void UpdateStatusText()
        {
            if (statusText == null || configManager == null) return;
            
            statusText.text = $"Current: {configManager.CurrentPresetName}\n" +
                             $"Size: {configManager.CurrentWallSize.x:F1}m × {configManager.CurrentWallSize.y:F1}m";
        }

        #region Event Handlers

        private void OnPresetChanged(int presetIndex)
        {
            if (!isInitialized || configManager == null) return;
            
            configManager.SelectPreset(presetIndex);
            UpdateUIFromConfiguration();
        }

        private void OnWidthChanged(float value)
        {
            if (!isInitialized) return;
            UpdateValueTexts();
            ApplyCustomSettings();
        }

        private void OnHeightChanged(float value)
        {
            if (!isInitialized) return;
            UpdateValueTexts();
            ApplyCustomSettings();
        }

        private void OnRotationXChanged(float value)
        {
            if (!isInitialized) return;
            UpdateValueTexts();
            ApplyCustomSettings();
        }

        private void OnRotationYChanged(float value)
        {
            if (!isInitialized) return;
            UpdateValueTexts();
            ApplyCustomSettings();
        }

        private void OnRotationZChanged(float value)
        {
            if (!isInitialized) return;
            UpdateValueTexts();
            ApplyCustomSettings();
        }

        private void OnPositionXChanged(float value)
        {
            if (!isInitialized) return;
            UpdateValueTexts();
            ApplyCustomSettings();
        }

        private void OnPositionYChanged(float value)
        {
            if (!isInitialized) return;
            UpdateValueTexts();
            ApplyCustomSettings();
        }

        private void OnPositionZChanged(float value)
        {
            if (!isInitialized) return;
            UpdateValueTexts();
            ApplyCustomSettings();
        }

        private void OnApplyClicked()
        {
            if (configManager == null) return;
            
            configManager.ApplyCurrentConfiguration();
            UpdateStatusText();
            Debug.Log("[WallConfigurationUI] Configuration applied manually");
        }

        private void OnResetClicked()
        {
            if (configManager == null) return;
            
            configManager.ResetToDefault();
            UpdateUIFromConfiguration();
            Debug.Log("[WallConfigurationUI] Configuration reset to default");
        }

        private void OnCustomModeToggled(bool isCustom)
        {
            // Enable/disable sliders based on custom mode
            bool enableSliders = isCustom;
            
            if (widthSlider != null)
                widthSlider.interactable = enableSliders;
            
            if (heightSlider != null)
                heightSlider.interactable = enableSliders;
            
            if (rotationXSlider != null)
                rotationXSlider.interactable = enableSliders;
            
            if (rotationYSlider != null)
                rotationYSlider.interactable = enableSliders;
            
            if (rotationZSlider != null)
                rotationZSlider.interactable = enableSliders;
            
            if (positionXSlider != null)
                positionXSlider.interactable = enableSliders;
            
            if (positionYSlider != null)
                positionYSlider.interactable = enableSliders;
            
            if (positionZSlider != null)
                positionZSlider.interactable = enableSliders;
        }

        #endregion

        /// <summary>
        /// Apply custom settings from sliders
        /// </summary>
        private void ApplyCustomSettings()
        {
            if (configManager == null) return;
            
            Vector2 size = new Vector2(
                widthSlider != null ? widthSlider.value : 2f,
                heightSlider != null ? heightSlider.value : 3f
            );
            
            Vector3 rotation = new Vector3(
                rotationXSlider != null ? rotationXSlider.value : 0f,
                rotationYSlider != null ? rotationYSlider.value : 0f,
                rotationZSlider != null ? rotationZSlider.value : 0f
            );
            
            Vector3 position = new Vector3(
                positionXSlider != null ? positionXSlider.value : 0f,
                positionYSlider != null ? positionYSlider.value : 0f,
                positionZSlider != null ? positionZSlider.value : 0f
            );
            
            configManager.SetCustomWallSize(size);
            configManager.SetCustomRotation(rotation);
            configManager.SetCustomPosition(position);
            
            UpdateStatusText();
        }

        void OnDestroy()
        {
            // Clean up event listeners
            if (presetDropdown != null)
                presetDropdown.onValueChanged.RemoveAllListeners();
            
            if (applyButton != null)
                applyButton.onClick.RemoveAllListeners();
            
            if (resetButton != null)
                resetButton.onClick.RemoveAllListeners();
        }
    }
} 