# Simple Animation Switching Test Setup

## Quick Setup (5 minutes)

### Step 1: Create UI Canvas
1. In Unity, right-click in Hierarchy → UI → Canvas
2. Right-click on Canvas → UI → Panel (this will be your test panel)
3. Rename the Panel to "AnimationTestPanel"

### Step 2: Add UI Elements
Right-click on AnimationTestPanel and add these UI elements:

1. **UI → Button** - Rename to "SwitchToTake001Button"
   - Set Text to "Switch to Take 001"
   - Position: Top-left of panel

2. **UI → Button** - Rename to "SwitchToCurrentButton" 
   - Set Text to "Back to Original"
   - Position: Top-right of panel

3. **UI → Text** - Rename to "StatusText"
   - Position: Middle of panel
   - Set text to "Ready to test animations"

4. **UI → Text** - Rename to "CurrentAnimationText"
   - Position: Bottom of panel
   - Set text to "Current Animation: Loading..."

### Step 3: Add the Script
1. Right-click on AnimationTestPanel
2. Add Component → Scripts → Simple Animation Switcher
3. In the inspector, drag the UI elements to their respective fields:
   - Switch To Take001 Button → SwitchToTake001Button
   - Switch To Current Button → SwitchToCurrentButton  
   - Status Text → StatusText
   - Current Animation Text → CurrentAnimationText

### Step 4: Test It!
1. Press Play in Unity
2. The script will automatically find your FBXCharacterController
3. Click "Switch to Take 001" to load the animation from NewAnimationOnly.fbx
4. Click "Back to Original" to return to the previous animation

## What This Does

- **Keeps your current character** - No character switching needed
- **Switches between animations** - Current animation ↔ Take 001 from NewAnimationOnly.fbx
- **Shows status** - Real-time feedback on what's happening
- **Simple UI** - Just two buttons to test switching

## Troubleshooting

### "No FBXCharacterController found"
- Make sure you have an FBXCharacterController component in your scene
- Check that it's properly initialized

### "Failed to load Take 001 animation"
- Verify that NewAnimationOnly.fbx exists in Assets/DeepMotion/
- Check that the FBX has "Take 001" animation imported
- Make sure you're in the Unity Editor (FBX loading doesn't work in builds)

### Animation doesn't play
- Check that the character has an Animator component
- Verify that the AnimatorOverrideController is assigned
- Look at the Console for detailed error messages

## Console Commands (Alternative)

If you prefer console commands instead of UI:

```csharp
// In Unity Console window, type these commands:

// Switch to Take 001
FBXCharacterController.LoadAnimationFromPath("Assets/DeepMotion/NewAnimationOnly.fbx", "Take 001");

// Test animation playback
FBXCharacterController.TestAnimationPlayback();

// Check current status
FBXCharacterController.CheckAnimatorStatus();
```

## Next Steps

Once this basic switching works, you can:
1. Add more animations to test
2. Create animation assets using the full AnimationManager system
3. Test with DeepMotion API integration
4. Build more complex switching logic

This simple setup gives you the fastest way to test animation switching without any complex setup! 