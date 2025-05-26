using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace BodyTracking.Animation
{
    /// <summary>
    /// Simple UI for testing animation and character switching
    /// Can be used during development and for runtime switching
    /// </summary>
    public class AnimationSwitcherUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Dropdown animationDropdown;
        [SerializeField] private Dropdown characterDropdown;
        [SerializeField] private Button switchAnimationButton;
        [SerializeField] private Button switchCharacterButton;
        [SerializeField] private Button requestDeepMotionButton;
        [SerializeField] private InputField deepMotionPromptInput;
        [SerializeField] private Text statusText;
        [SerializeField] private Text currentInfoText;
        
        [Header("References")]
        [SerializeField] private AnimationManager animationManager;
        
        [Header("Settings")]
        [SerializeField] private bool autoRefreshDropdowns = true;
        [SerializeField] private float refreshInterval = 2f;
        
        private float lastRefreshTime;
        
        void Start()
        {
            SetupUI();
            RefreshDropdowns();
            
            // Subscribe to events
            if (animationManager != null)
            {
                animationManager.OnAnimationChanged += OnAnimationChanged;
                animationManager.OnCharacterChanged += OnCharacterChanged;
                animationManager.OnDeepMotionAnimationLoaded += OnDeepMotionAnimationLoaded;
            }
        }
        
        void Update()
        {
            if (autoRefreshDropdowns && Time.time - lastRefreshTime > refreshInterval)
            {
                RefreshDropdowns();
                lastRefreshTime = Time.time;
            }
            
            UpdateCurrentInfo();
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events
            if (animationManager != null)
            {
                animationManager.OnAnimationChanged -= OnAnimationChanged;
                animationManager.OnCharacterChanged -= OnCharacterChanged;
                animationManager.OnDeepMotionAnimationLoaded -= OnDeepMotionAnimationLoaded;
            }
        }
        
        #region UI Setup
        
        private void SetupUI()
        {
            // Setup button listeners
            if (switchAnimationButton != null)
            {
                switchAnimationButton.onClick.AddListener(OnSwitchAnimationClicked);
            }
            
            if (switchCharacterButton != null)
            {
                switchCharacterButton.onClick.AddListener(OnSwitchCharacterClicked);
            }
            
            if (requestDeepMotionButton != null)
            {
                requestDeepMotionButton.onClick.AddListener(OnRequestDeepMotionClicked);
            }
            
            // Setup default prompt
            if (deepMotionPromptInput != null)
            {
                deepMotionPromptInput.text = "A person climbing a rock wall";
            }
            
            UpdateStatus("UI Initialized");
        }
        
        private void RefreshDropdowns()
        {
            if (animationManager == null) return;
            
            // Refresh animation dropdown
            if (animationDropdown != null)
            {
                var animationNames = animationManager.GetAvailableAnimationNames();
                animationDropdown.ClearOptions();
                animationDropdown.AddOptions(animationNames.ToList());
            }
            
            // Refresh character dropdown
            if (characterDropdown != null)
            {
                var characterNames = animationManager.GetAvailableCharacterNames();
                characterDropdown.ClearOptions();
                characterDropdown.AddOptions(characterNames.ToList());
            }
        }
        
        #endregion
        
        #region Button Handlers
        
        private void OnSwitchAnimationClicked()
        {
            if (animationManager == null || animationDropdown == null)
            {
                UpdateStatus("Error: Missing references");
                return;
            }
            
            if (animationDropdown.options.Count == 0)
            {
                UpdateStatus("Error: No animations available");
                return;
            }
            
            string selectedAnimation = animationDropdown.options[animationDropdown.value].text;
            bool success = animationManager.SwitchAnimation(selectedAnimation);
            
            if (success)
            {
                UpdateStatus($"Switched to animation: {selectedAnimation}");
            }
            else
            {
                UpdateStatus($"Failed to switch to animation: {selectedAnimation}");
            }
        }
        
        private void OnSwitchCharacterClicked()
        {
            if (animationManager == null || characterDropdown == null)
            {
                UpdateStatus("Error: Missing references");
                return;
            }
            
            if (characterDropdown.options.Count == 0)
            {
                UpdateStatus("Error: No characters available");
                return;
            }
            
            string selectedCharacter = characterDropdown.options[characterDropdown.value].text;
            
            // Also get selected animation if any
            string selectedAnimation = null;
            if (animationDropdown != null && animationDropdown.options.Count > 0)
            {
                selectedAnimation = animationDropdown.options[animationDropdown.value].text;
            }
            
            bool success = animationManager.SwitchCharacter(selectedCharacter, selectedAnimation);
            
            if (success)
            {
                UpdateStatus($"Switched to character: {selectedCharacter}" + 
                           (selectedAnimation != null ? $" with animation: {selectedAnimation}" : ""));
            }
            else
            {
                UpdateStatus($"Failed to switch to character: {selectedCharacter}");
            }
        }
        
        private void OnRequestDeepMotionClicked()
        {
            if (animationManager == null || deepMotionPromptInput == null)
            {
                UpdateStatus("Error: Missing references");
                return;
            }
            
            string prompt = deepMotionPromptInput.text;
            if (string.IsNullOrEmpty(prompt))
            {
                UpdateStatus("Error: Please enter a prompt");
                return;
            }
            
            animationManager.RequestDeepMotionAnimation(prompt);
            UpdateStatus($"Requesting DeepMotion animation: {prompt}");
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnAnimationChanged(AnimationAsset animation)
        {
            UpdateStatus($"Animation changed to: {animation.GetDisplayName()}");
            RefreshDropdowns(); // In case it's a new animation
        }
        
        private void OnCharacterChanged(CharacterAsset character)
        {
            UpdateStatus($"Character changed to: {character.GetDisplayName()}");
            RefreshDropdowns(); // In case it's a new character
        }
        
        private void OnDeepMotionAnimationLoaded(AnimationAsset animation)
        {
            UpdateStatus($"DeepMotion animation loaded: {animation.GetDisplayName()}");
            RefreshDropdowns(); // Add new animation to dropdown
        }
        
        #endregion
        
        #region UI Updates
        
        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
            }
            
            Debug.Log($"[AnimationSwitcherUI] {message}");
        }
        
        private void UpdateCurrentInfo()
        {
            if (currentInfoText == null || animationManager == null) return;
            
            string info = "Current State:\n";
            info += animationManager.GetCurrentCharacterInfo() + "\n";
            info += animationManager.GetCurrentAnimationInfo();
            
            currentInfoText.text = info;
        }
        
        #endregion
        
        #region Public API (for external scripts)
        
        /// <summary>
        /// Programmatically switch animation
        /// </summary>
        public void SwitchToAnimation(string animationName)
        {
            if (animationManager != null)
            {
                animationManager.SwitchAnimation(animationName);
            }
        }
        
        /// <summary>
        /// Programmatically switch character
        /// </summary>
        public void SwitchToCharacter(string characterName, string animationName = null)
        {
            if (animationManager != null)
            {
                animationManager.SwitchCharacter(characterName, animationName);
            }
        }
        
        /// <summary>
        /// Load DeepMotion animation from external source
        /// </summary>
        public void LoadDeepMotionAnimation(AnimationClip clip, string jobId, string prompt)
        {
            if (animationManager != null)
            {
                animationManager.LoadDeepMotionAnimation(clip, jobId, prompt);
            }
        }
        
        #endregion
    }
} 