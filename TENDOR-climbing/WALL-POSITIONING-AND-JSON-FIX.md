# Wall Positioning and JSON Recording Fixes

This document outlines the improvements made to the TENDOR climbing project for better wall positioning and fixed JSON recording functionality.

## Quick Fix Summary

✅ **FIXED**: Wall positioning and scaling now works correctly for 1m-3m walls
✅ **FIXED**: JSON recording now automatically saves hip tracking data
✅ **FIXED**: Image target rotation - walls now stand upright by default
✅ **FIXED**: Video recording now defaults to portrait format (2048×4096)
✅ **FIXED**: UI elements excluded from video recordings - only clean camera view is saved

## Issues Resolved

### 1. Wall Positioning and Scaling
- **Problem**: Wall wasn't properly sized/rotated when placed on image target
- **Solution**: Enhanced ARImageTargetManager with proper scaling and rotation logic
- **Features**: Support for variable wall sizes (1m-3m), aspect ratio maintenance, rotation correction

### 2. JSON Recording
- **Problem**: Hip location data wasn't being saved to JSON files
- **Solution**: Integrated RecordingStorage with SynchronizedVideoRecorder
- **Features**: Automatic JSON saving, session ID naming, metadata support

### 3. Image Target Rotation
- **Problem**: Wall was tilted 90° towards user (appearing to fall forward)
- **Solution**: Added default rotation offset of 90° on X-axis to make walls stand upright
- **Features**: Configurable rotation presets, real-time rotation testing

### 4. Video Recording Format
- **Problem**: Videos were being saved in landscape format instead of portrait
- **Solution**: Changed default resolution from HD_1920x1080 to POW2_2048x4096 (portrait)
- **Features**: Runtime resolution control, portrait/landscape detection, configuration methods

### 5. UI Elements in Video Recording
- **Problem**: UI controls, debug points, and interface elements were being captured in videos
- **Solution**: Added UI exclusion system with camera layer filtering
- **Features**: Dedicated recording camera option, runtime UI exclusion control, layer mask configuration

## Components Added/Modified

### Enhanced Wall Configuration

#### ARImageTargetManager Improvements
- **Real-world scaling**: Walls now scale based on actual dimensions (1m to 3m configurable)
- **Aspect ratio maintenance**: Proper scaling that maintains wall proportions
- **Position and rotation offsets**: Fine-tune wall placement relative to image target
- **Runtime configuration**: Change wall settings during runtime

#### New Configuration System
- `WallConfigurationManager`: Manages wall presets and custom configurations
- `WallConfigurationUI`: UI for testing and adjusting wall settings
- Preset system with Small (1×2m), Medium (2×3m), and Large (3×4m) walls

### Fixed JSON Recording

#### SynchronizedVideoRecorder Enhancements
- **Automatic JSON saving**: Hip tracking data is now automatically saved as JSON
- **Proper file naming**: Uses session IDs for consistent file naming
- **Storage integration**: Uses the existing `RecordingStorage` system
- **Validation**: Checks for both video and JSON file existence

## Usage Instructions

### Setting Up Wall Configuration

1. **Add WallConfigurationManager to your scene**:
   ```csharp
   // The manager will automatically find the ARImageTargetManager
   var wallConfig = gameObject.AddComponent<WallConfigurationManager>();
   ```

2. **Configure wall presets in the inspector**:
   - Small Wall: 1m × 2m
   - Medium Wall: 2m × 3m (default)
   - Large Wall: 3m × 4m

3. **Runtime configuration**:
   ```csharp
   // Select a preset
   wallConfig.SelectPreset("Large Wall");
   
   // Or set custom dimensions
   wallConfig.SetCustomWallSize(2.5f, 3.5f);
   wallConfig.SetCustomRotation(new Vector3(0, 90, 0));
   wallConfig.SetCustomPosition(new Vector3(0, 0.1f, 0));
   ```

### Using the Configuration UI

1. **Add WallConfigurationUI to a Canvas**
2. **Assign UI elements in the inspector**:
   - Preset dropdown
   - Size sliders (width/height)
   - Rotation sliders (X/Y/Z)
   - Position sliders (X/Y/Z)
   - Value text displays
   - Apply/Reset buttons

3. **Test different configurations** in real-time during development

### JSON Recording

The JSON recording now works automatically when using `SynchronizedVideoRecorder`:

```csharp
// Start recording (both video and JSON)
bool success = videoRecorder.StartSynchronizedRecording();

// Stop recording
var result = videoRecorder.StopSynchronizedRecording();

// Check if both files were created
if (result.IsValid && result.HasHipDataFile)
{
    Debug.Log($"Video: {result.videoFilePath}");
    Debug.Log($"JSON: {result.hipDataPath}");
}

// Load hip data later
HipRecording hipData = videoRecorder.LoadHipRecording(result.sessionId);
```

### UI Exclusion from Video Recording

The video recorder now automatically excludes UI elements from recordings, capturing only the clean camera view:

```csharp
// Configure UI exclusion (enabled by default)
videoRecorder.SetUIExclusionSettings(true, LayerMask.GetMask("UI", "Debug"));

// Use dedicated recording camera for complete UI separation
videoRecorder.SetDedicatedCameraMode(true);

// Get current configuration
string summary = videoRecorder.GetUIExclusionSummary();
Debug.Log(summary);
```

**UI Exclusion Options:**

1. **Layer Mask Filtering** (Default): Temporarily modifies the main camera's culling mask during recording
2. **Dedicated Recording Camera**: Creates a separate camera specifically for recording without UI layers

**Default Settings:**
- UI exclusion: **Enabled**
- Excluded layers: **UI layer (layer 5)**
- Recording camera: **Uses main camera with layer filtering**

## Configuration Parameters

### Wall Scaling Options

- **autoScaleToImageTarget**: Scale wall based on detected image size
- **maintainAspectRatio**: Keep wall proportions when scaling
- **realWorldWallSize**: Target wall dimensions in meters
- **wallRotationOffset**: Rotation correction (degrees)
- **wallPositionOffset**: Position offset from image center (meters)

### JSON Storage Options

- **autoSaveHipDataAsJSON**: Automatically save hip data when stopping video recording
- **hipDataPath**: Custom path for JSON files (optional)
- **sessionId**: Unique identifier for recording sessions

### UI Exclusion Options

- **excludeUIFromRecording**: Enable/disable UI exclusion from video recordings
- **uiLayersToExclude**: Layer mask specifying which layers to exclude (default: UI layer 5)
- **createDedicatedRecordingCamera**: Create separate camera for recording without UI
- **recordingCameraName**: Name for the dedicated recording camera

### Video Recording Options

- **videoResolution**: Recording resolution (default: POW2_2048x4096 for portrait)
- **videoFrameRate**: Recording frame rate (default: 30fps)
- **recordAudio**: Include audio in video recordings
- **videoOutputFolder**: Folder name for video files

## Rotation Fix and Testing

### Default Rotation Correction

The wall rotation issue has been fixed with a default rotation offset of **90° on the X-axis**. This makes walls stand upright instead of tilting towards the user.

### Testing Different Rotations

If the default rotation doesn't work perfectly for your setup, use the `RotationTestHelper` script:

1. **Add RotationTestHelper to your scene**:
   ```csharp
   // Add to any GameObject in your scene
   var rotationHelper = gameObject.AddComponent<RotationTestHelper>();
   ```

2. **Use Context Menu for quick testing**:
   - Right-click the RotationTestHelper component in Inspector
   - Try different presets:
     - "Apply Preset 1: 90° X (Wall Upright)" - Default fix
     - "Apply Preset 2: -90° X (Wall Upright, Opposite)" - Alternative
     - "Apply Preset 6: 90° X + 180° Y (Wall Upright, Flipped)" - If wall is backwards

3. **Fine-tune rotation**:
   - Use "Rotate X +15°" / "Rotate X -15°" for small adjustments
   - Modify the `customRotation` field in Inspector for precise control

4. **Common rotation fixes**:
   ```csharp
   // Wall tilted towards you
   rotationHelper.SetRotation(new Vector3(90f, 0f, 0f));
   
   // Wall tilted away from you  
   rotationHelper.SetRotation(new Vector3(-90f, 0f, 0f));
   
   // Wall rotated left/right
   rotationHelper.SetRotation(new Vector3(90f, 90f, 0f));
   
   // Wall upside down
   rotationHelper.SetRotation(new Vector3(90f, 180f, 0f));
   ```

### Troubleshooting Rotation Issues

**Problem**: Wall still appears tilted after applying the fix
- **Solution**: Try preset 2 (-90° X) instead of preset 1 (90° X)
- **Alternative**: Use the WallConfigurationUI sliders to adjust rotation in real-time

**Problem**: Wall appears upside down or backwards
- **Solution**: Add 180° to Y rotation: `new Vector3(90f, 180f, 0f)`

**Problem**: Wall is rotated left or right
- **Solution**: Adjust Y rotation: `new Vector3(90f, ±90f, 0f)`

## Performance Considerations

## File Structure

```
TENDOR-climbing/Assets/Scripts/
├── AR/
│   ├── ARImageTargetManager.cs (enhanced)
│   ├── WallConfigurationManager.cs (new)
├── Recording/
│   ├── SynchronizedVideoRecorder.cs (fixed)
│   ├── BodyTrackingRecorder.cs (existing)
├── Storage/
│   ├── RecordingStorage.cs (existing)
├── UI/
│   ├── WallConfigurationUI.cs (new)
```

## Testing

### Wall Positioning Test
1. Place "Wall 1" image target in scene
2. Adjust wall configuration using the UI
3. Verify wall appears at correct size and position
4. Test with different image target sizes

### JSON Recording Test
1. Start synchronized recording
2. Move around to generate hip tracking data
3. Stop recording
4. Check that both .mp4 and .json files are created
5. Verify JSON contains hip position data

## Troubleshooting

### Wall Not Appearing
- Check that content GameObject is named exactly "Wall 1"
- Verify ARImageTargetManager is properly configured
- Check image target is being detected (tracking state)

### JSON Not Saving
- Ensure `