# TENDOR Animation System

A scalable animation and character management system for Unity with DeepMotion API integration.

## Overview

This system provides a flexible architecture for:
- **Runtime animation switching** (keeping the same character)
- **Character + animation swapping** 
- **DeepMotion API integration** for generating new animations
- **Scalable asset management** with ScriptableObjects
- **Build-compatible operation** (no editor dependencies)

## Components

### Core Scripts

1. **`AnimationAsset.cs`** - ScriptableObject for animation metadata
2. **`CharacterAsset.cs`** - ScriptableObject for character metadata  
3. **`AnimationManager.cs`** - Central manager for runtime switching
4. **`AnimationSwitcherUI.cs`** - UI controller for testing
5. **`DeepMotionAPI.cs`** - DeepMotion API integration

### Legacy Integration

- **`FBXCharacterController.cs`** - Updated for build compatibility
- Works seamlessly with the new system

## Setup Guide

### 1. Create Animation Assets

```csharp
// In Unity Editor, right-click in Project window:
// Create > TENDOR > Animation Asset

// Or create programmatically:
AnimationAsset asset = AnimationAsset.CreateFromClip(myClip);
asset.animationName = "Take 001";
asset.description = "Original climbing animation";
asset.tags = new string[] { "climbing", "original" };
```

### 2. Create Character Assets

```csharp
// In Unity Editor:
// Create > TENDOR > Character Asset

// Or create programmatically:
CharacterAsset character = CharacterAsset.CreateFromPrefab(myPrefab);
character.characterName = "NewBody";
character.defaultAnimation = myAnimationAsset;
```

### 3. Setup Animation Manager

1. Add `AnimationManager` component to a GameObject
2. Assign your `AnimationAsset[]` and `CharacterAsset[]` arrays
3. Assign reference to `FBXCharacterController`

### 4. Runtime Usage

```csharp
// Get reference to AnimationManager
AnimationManager animManager = FindObjectOfType<AnimationManager>();

// Switch animations (keeping current character)
animManager.SwitchAnimation("Take 001");
animManager.SwitchAnimation("DeepMotion_Climbing");

// Switch characters (with optional animation)
animManager.SwitchCharacter("NewBody", "Take 001");
animManager.SwitchCharacter("RobotCharacter"); // Uses default animation

// Load DeepMotion animation
animManager.LoadDeepMotionAnimation(clip, jobId, prompt);
```

## DeepMotion Integration

### Setup

1. Get API key from DeepMotion
2. Add `DeepMotionAPI` component
3. Set API key in inspector or via code:

```csharp
DeepMotionAPI api = FindObjectOfType<DeepMotionAPI>();
api.SetApiKey("your-api-key-here");
```

### Usage

```csharp
// Request new animation
api.RequestAnimation("A person climbing a steep rock wall");

// Listen for completion
api.OnAnimationReady += (clip, jobId, prompt) => {
    Debug.Log($"Animation ready: {prompt}");
    // Automatically added to AnimationManager if connected
};
```

### Workflow

1. **Submit Request** - Send prompt to DeepMotion API
2. **Poll Status** - Check job progress every 5 seconds
3. **Download FBX** - Get completed animation file
4. **Process & Import** - Convert to Unity AnimationClip
5. **Auto-Switch** - Optionally switch to new animation

## UI Testing

### Setup UI

1. Create Canvas with UI elements:
   - Dropdown for animations
   - Dropdown for characters  
   - Buttons for switching
   - Input field for DeepMotion prompts

2. Add `AnimationSwitcherUI` component
3. Assign UI references and `AnimationManager`

### Usage

- **Animation Dropdown** - Select and switch animations
- **Character Dropdown** - Select and switch characters
- **DeepMotion Input** - Enter prompt and request new animation
- **Status Text** - Shows current operations
- **Info Text** - Shows current character/animation state

## Advanced Usage

### Custom Animation Loading

```csharp
// Load from Resources
AnimationClip clip = Resources.Load<AnimationClip>("MyAnimation");
AnimationAsset asset = AnimationAsset.CreateFromClip(clip);
animManager.AddAnimationToLibrary(asset);

// Load from DeepMotion
animManager.LoadDeepMotionAnimation(clip, "job123", "climbing motion");
```

### Character Compatibility

```csharp
// Set animation compatibility
characterAsset.compatibleAnimations = new AnimationAsset[] { 
    climbingAnim, 
    walkingAnim 
};

// Check compatibility
bool compatible = characterAsset.IsCompatibleWith(animationAsset);
```

### Event Handling

```csharp
// Subscribe to events
animManager.OnAnimationChanged += (animation) => {
    Debug.Log($"Switched to: {animation.GetDisplayName()}");
};

animManager.OnCharacterChanged += (character) => {
    Debug.Log($"Switched to: {character.GetDisplayName()}");
};

animManager.OnDeepMotionAnimationLoaded += (animation) => {
    Debug.Log($"New DeepMotion animation: {animation.deepMotionPrompt}");
};
```

## Migration from Legacy System

### Existing Projects

1. **Keep existing setup** - `FBXCharacterController` still works
2. **Add AnimationManager** - Enhances with new features
3. **Create assets gradually** - Convert animations as needed
4. **Test compatibility** - Ensure smooth operation

### Benefits

- **No breaking changes** - Legacy code continues working
- **Enhanced functionality** - Runtime switching capabilities
- **Better organization** - ScriptableObject-based assets
- **API integration** - DeepMotion support
- **Build compatibility** - No editor dependencies

## Troubleshooting

### Common Issues

1. **Animation not switching**
   - Check if AnimationAsset is valid
   - Verify character compatibility
   - Ensure FBXCharacterController is initialized

2. **Character not loading**
   - Check if CharacterAsset prefab is assigned
   - Verify hip bone configuration
   - Check console for initialization errors

3. **DeepMotion not working**
   - Verify API key is set
   - Check internet connection
   - Monitor console for API errors

### Debug Tips

```csharp
// Enable logging
animManager.enableLogging = true;
deepMotionAPI.enableLogging = true;

// Check current state
Debug.Log(animManager.GetCurrentAnimationInfo());
Debug.Log(animManager.GetCurrentCharacterInfo());

// List available assets
string[] animations = animManager.GetAvailableAnimationNames();
string[] characters = animManager.GetAvailableCharacterNames();
```

## Performance Considerations

- **Asset Loading** - Use Resources or AssetBundles for large libraries
- **Character Switching** - Destroys/creates GameObjects (use sparingly)
- **Animation Switching** - Lightweight operation (use freely)
- **DeepMotion Downloads** - Cache locally to avoid re-downloading

## Future Enhancements

- **Asset Bundle Support** - For larger animation libraries
- **Animation Blending** - Smooth transitions between animations
- **Retargeting** - Cross-character animation compatibility
- **Compression** - Optimized animation storage
- **Cloud Storage** - Shared animation libraries

## Example Scene Setup

1. **Create GameObject** with `AnimationManager`
2. **Create Animation Assets** for your clips
3. **Create Character Assets** for your characters
4. **Assign arrays** in AnimationManager inspector
5. **Add UI** with `AnimationSwitcherUI` for testing
6. **Test switching** in Play mode

This system provides a solid foundation for scalable animation management while maintaining compatibility with your existing setup. 