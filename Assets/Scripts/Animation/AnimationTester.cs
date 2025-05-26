using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace BodyTracking.Animation
{
    /// <summary>
    /// Simple animation tester with inspector controls
    /// Easy to use and debug - now with toggle functionality
    /// </summary>
    public class AnimationTester : MonoBehaviour
    {
        [Header("Animation Testing")]
        [SerializeField] private FBXCharacterController characterController;
        [SerializeField] private string newAnimationName = "Take 001";
        
        [Header("UI Controls")]
        [SerializeField] private Button testButton;
        [SerializeField] private Text statusText;
        [SerializeField] private Text currentAnimText;
        [SerializeField] private bool autoFindCharacter = true;
        [SerializeField] private bool createUIOnStart = true;
        
        [Header("Debug")]
        [SerializeField] private bool verboseLogging = true;
        
        // Animation state tracking
        private bool isUsingNewAnimation = false;
        private string originalAnimationName = "";
        private bool hasStoredOriginal = false;
        
        void Start()
        {
            if (autoFindCharacter && characterController == null)
            {
                characterController = FindFirstObjectByType<FBXCharacterController>();
                Log($"Auto-found character controller: {(characterController ? characterController.name : "None")}");
            }
            
            if (createUIOnStart)
            {
                CreateSimpleUI();
            }
            
            if (characterController == null)
            {
                LogError("No FBXCharacterController found! Please assign one in the inspector.");
            }
            else
            {
                // Store the original animation name
                StoreOriginalAnimation();
            }
        }
        
        private void StoreOriginalAnimation()
        {
            if (characterController?.CharacterAnimatorForEditor == null) return;
            
            var animator = characterController.CharacterAnimatorForEditor;
            var overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            
            if (overrideController != null)
            {
                var overrides = new List<System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>>();
                overrideController.GetOverrides(overrides);
                
                foreach (var pair in overrides)
                {
                    if (pair.Key.name == "CharacterAnimation" && pair.Value != null)
                    {
                        originalAnimationName = pair.Value.name;
                        hasStoredOriginal = true;
                        Log($"Stored original animation: {originalAnimationName}");
                        break;
                    }
                }
            }
            
            if (!hasStoredOriginal)
            {
                Log("No original animation found - will use default animation as fallback");
            }
        }
        
        private string GetCurrentAnimationName()
        {
            if (characterController?.CharacterAnimatorForEditor == null) return "Unknown";
            
            var animator = characterController.CharacterAnimatorForEditor;
            var overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            
            if (overrideController != null)
            {
                var overrides = new List<System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>>();
                overrideController.GetOverrides(overrides);
                
                foreach (var pair in overrides)
                {
                    if (pair.Key.name == "CharacterAnimation" && pair.Value != null)
                    {
                        return pair.Value.name;
                    }
                }
            }
            
            return "No Animation";
        }
        
        private void CreateSimpleUI()
        {
            Log("Creating simple UI...");
            
            // Find or create Canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("AnimationTestCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create UI Panel
            GameObject panel = new GameObject("AnimationTestPanel");
            panel.transform.SetParent(canvas.transform, false);
            
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(450, 250);
            panelRect.anchoredPosition = new Vector2(0, 100);
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            
            // Create Title
            CreateText(panel, "🎯 ANIMATION SWITCHER", new Vector2(0, 80), 18, Color.white, FontStyle.Bold);
            
            // Create Current Animation Display
            currentAnimText = CreateText(panel, "Current: Loading...", new Vector2(0, 50), 14, Color.cyan);
            
            // Create Test Button
            GameObject buttonObj = new GameObject("ToggleButton");
            buttonObj.transform.SetParent(panel.transform, false);
            
            testButton = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.3f, 0.7f, 1f, 1f);
            
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(250, 50);
            buttonRect.anchoredPosition = new Vector2(0, 10);
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            
            // Button Text
            GameObject buttonTextObj = new GameObject("ButtonText");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            
            Text buttonText = buttonTextObj.AddComponent<Text>();
            buttonText.text = $"Switch to {newAnimationName}";
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = 16;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.fontStyle = FontStyle.Bold;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.sizeDelta = Vector2.zero;
            buttonTextRect.anchoredPosition = Vector2.zero;
            
            // Create Status Text
            GameObject statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(panel.transform, false);
            
            statusText = statusObj.AddComponent<Text>();
            statusText.text = characterController ? "Ready to switch animations!" : "No character controller found";
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.fontSize = 14;
            statusText.color = characterController ? Color.green : Color.red;
            statusText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform statusRect = statusObj.GetComponent<RectTransform>();
            statusRect.sizeDelta = new Vector2(400, 30);
            statusRect.anchoredPosition = new Vector2(0, -40);
            statusRect.anchorMin = new Vector2(0.5f, 0.5f);
            statusRect.anchorMax = new Vector2(0.5f, 0.5f);
            statusRect.pivot = new Vector2(0.5f, 0.5f);
            
            // Create Info Text
            CreateText(panel, "Press T key or click button to toggle", new Vector2(0, -70), 12, Color.gray);
            
            // Setup button click
            testButton.onClick.AddListener(ToggleAnimation);
            
            // Start updating current animation display
            InvokeRepeating(nameof(UpdateCurrentAnimationDisplay), 1f, 1f);
            
            Log("✅ Enhanced UI created successfully!");
        }
        
        private Text CreateText(GameObject parent, string text, Vector2 position, int fontSize, Color color, FontStyle fontStyle = FontStyle.Normal)
        {
            GameObject textObj = new GameObject("Text_" + text.Substring(0, Mathf.Min(10, text.Length)));
            textObj.transform.SetParent(parent.transform, false);
            
            // Add RectTransform component first
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(400, fontSize + 10);
            textRect.anchoredPosition = position;
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            
            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = fontSize;
            textComponent.color = color;
            textComponent.fontStyle = fontStyle;
            textComponent.alignment = TextAnchor.MiddleCenter;
            
            return textComponent;
        }
        
        [ContextMenu("Toggle Animation")]
        public void ToggleAnimation()
        {
            if (characterController == null)
            {
                LogError("No character controller assigned!");
                UpdateStatus("❌ No character controller!", Color.red);
                return;
            }
            
            bool success = false;
            string targetAnimation = "";
            
            if (isUsingNewAnimation)
            {
                // Switch back to original animation
                if (hasStoredOriginal && !string.IsNullOrEmpty(originalAnimationName))
                {
                    targetAnimation = originalAnimationName;
                    Log($"Switching back to original animation: {targetAnimation}");
                    UpdateStatus($"🔄 Switching to {targetAnimation}...", Color.yellow);
                    
                    // For now, we'll reload the default animation since we don't have a direct way to load by name
                    // This will load the original animation from the character's default setup
                    success = characterController.LoadAnimationFromNewAnimationFBX(originalAnimationName);
                    
                    if (!success)
                    {
                        // Fallback: try to reload any animation from NewAnimationOnly.fbx
                        success = characterController.LoadAnimationFromNewAnimationFBX();
                    }
                }
                else
                {
                    targetAnimation = "Default Animation";
                    Log("Switching back to default animation");
                    UpdateStatus("🔄 Switching to default animation...", Color.yellow);
                    success = characterController.LoadAnimationFromNewAnimationFBX();
                }
                
                if (success)
                {
                    isUsingNewAnimation = false;
                    UpdateButtonText($"Switch to {newAnimationName}");
                }
            }
            else
            {
                // Switch to new animation
                targetAnimation = newAnimationName;
                Log($"Switching to new animation: {targetAnimation}");
                UpdateStatus($"🔄 Switching to {targetAnimation}...", Color.yellow);
                
                success = characterController.LoadAnimationFromNewAnimationFBX(newAnimationName);
                
                if (success)
                {
                    isUsingNewAnimation = true;
                    UpdateButtonText($"Switch to Original");
                }
            }
            
            if (success)
            {
                Log($"✅ Successfully switched to {targetAnimation}");
                UpdateStatus($"✅ Switched to {targetAnimation}!", Color.green);
                characterController.StartAnimationPlayback();
            }
            else
            {
                LogError($"❌ Failed to switch to {targetAnimation}");
                UpdateStatus($"❌ Failed to switch to {targetAnimation}", Color.red);
            }
        }
        
        private void UpdateButtonText(string newText)
        {
            if (testButton != null)
            {
                Text buttonText = testButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = newText;
                }
            }
        }
        
        private void UpdateCurrentAnimationDisplay()
        {
            if (currentAnimText != null)
            {
                string currentAnim = GetCurrentAnimationName();
                currentAnimText.text = $"Current: {currentAnim}";
                
                // Update color based on which animation is active
                if (currentAnim.Contains(newAnimationName))
                {
                    currentAnimText.color = Color.yellow;
                }
                else
                {
                    currentAnimText.color = Color.cyan;
                }
            }
        }
        
        [ContextMenu("Find Character Controller")]
        public void FindCharacterController()
        {
            characterController = FindFirstObjectByType<FBXCharacterController>();
            if (characterController)
            {
                Log($"Found character controller: {characterController.name}");
                UpdateStatus("✅ Character controller found!", Color.green);
                StoreOriginalAnimation();
            }
            else
            {
                LogError("No FBXCharacterController found in scene");
                UpdateStatus("❌ No character controller found", Color.red);
            }
        }
        
        private void UpdateStatus(string message, Color color)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color;
            }
        }
        
        private void Log(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[AnimationTester] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[AnimationTester] {message}");
        }
        
        void Update()
        {
            // Quick test with keyboard
            if (Input.GetKeyDown(KeyCode.T))
            {
                ToggleAnimation();
            }
        }
        
        void OnDestroy()
        {
            // Clean up the repeating invoke
            CancelInvoke(nameof(UpdateCurrentAnimationDisplay));
        }
    }
} 