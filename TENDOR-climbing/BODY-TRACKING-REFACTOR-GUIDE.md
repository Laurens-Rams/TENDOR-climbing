# Body Tracking System Refactor Guide

## Overview

This document explains the new modular body tracking system that replaces the previous monolithic implementation. The new system provides:

- **Frame-accurate playback** of recorded motion
- **Consistent coordinate system handling** between recording and playback
- **Modular architecture** with clear separation of concerns
- **Improved data storage** and management
- **Better AR integration** with proper image target handling

## Architecture

### Core Components

The new system is built around these modular components:

#### 1. Data Layer (`BodyTracking.Data`)
- **`PoseData.cs`**: Core data structures for joints, frames, and recordings
- **`CoordinateFrame`**: Helper for coordinate system transformations
- Ensures consistent data representation throughout the system

#### 2. Recording Layer (`BodyTracking.Recording`)
- **`BodyTrackingRecorder.cs`**: Handles real-time recording of AR body tracking data
- Records all joint positions relative to the image target
- Provides visual feedback during recording

#### 3. Playback Layer (`BodyTracking.Playback`)
- **`BodyTrackingPlayer.cs`**: Frame-accurate playback of recorded data
- Transforms recorded positions to current image target location
- Shows motion paths and joint visualizations

#### 4. Storage Layer (`BodyTracking.Storage`)
- **`RecordingStorage.cs`**: Persistent storage and retrieval of recordings
- Supports JSON format with extensibility for binary storage
- Provides metadata and file management

#### 5. AR Integration (`BodyTracking.AR`)
- **`ARImageTargetManager.cs`**: Manages AR image target detection
- Provides coordinate system reference for recording/playback
- Handles image target loss and recovery

#### 6. Main Controller (`BodyTracking`)
- **`BodyTrackingController.cs`**: Central coordinator for all components
- Provides simple API for recording and playback operations
- Manages state transitions and error handling

#### 7. UI Layer (`BodyTracking.UI`)
- **`BodyTrackingUI.cs`**: Simple UI controls for recording and playback
- Can be extended or replaced with custom UI

## Key Improvements

### 1. Coordinate System Consistency

**Problem Solved**: Previously, coordinate transformations were inconsistent between recording and playback, causing the sphere to appear in wrong positions.

**Solution**:
- All joint positions are stored relative to the image target's coordinate frame at recording time
- During playback, positions are transformed from the recorded reference frame to the current image target position
- Uses `CoordinateFrame` helper class for consistent transformations

```csharp
// Recording: Transform world position to image-target-relative
Vector3 relativePosition = referenceFrame.InverseTransformPoint(worldPosition);

// Playback: Transform back to current world position
Vector3 worldPosition = currentImageTargetFrame.TransformPoint(relativePosition);
```

### 2. Frame-Accurate Playback

**Problem Solved**: Previous system only used static positioning without time-based animation.

**Solution**:
- Records timestamps for each frame at specified frame rate (30 FPS)
- Playback system interpolates between frames based on elapsed time
- Supports looping, seeking, and speed control

```csharp
public PoseFrame GetFrameAtTime(float time)
{
    int frameIndex = Mathf.FloorToInt(time * frameRate);
    return frames[Mathf.Clamp(frameIndex, 0, frames.Count - 1)];
}
```

### 3. Modular Architecture

**Problem Solved**: Previous `TrackingManager.cs` was 1329 lines with mixed responsibilities.

**Solution**:
- Each component has a single responsibility
- Components communicate through events and interfaces
- Easy to test, extend, and maintain individual parts

### 4. Better Data Storage

**Problem Solved**: Limited to single joint (hip) with basic serialization.

**Solution**:
- Records all 23 ARKit body joints with position, rotation, and confidence
- Efficient JSON storage with metadata
- Easy to extend with binary format for large datasets

## How to Use

### Setup

1. **Add Components to Scene**:
   ```
   1. Create empty GameObject "BodyTrackingSystem"
   2. Add BodyTrackingController component
   3. Add ARImageTargetManager component
   4. Assign references in inspector
   ```

2. **Configure AR Image Target**:
   ```
   - Set targetImageName in ARImageTargetManager
   - Assign wallPrefab for visualization
   - Configure wall rotation offset if needed
   ```

3. **Setup UI (Optional)**:
   ```
   - Add BodyTrackingUI component to Canvas
   - Create UI buttons and assign to component
   - Connect to BodyTrackingController
   ```

### Recording

```csharp
// Get controller reference
BodyTrackingController controller = FindObjectOfType<BodyTrackingController>();

// Start recording (requires image target detected)
if (controller.CanRecord)
{
    controller.StartRecording();
}

// Stop recording (saves automatically)
PoseRecording recording = controller.StopRecording();
```

### Playback

```csharp
// Play back last recording
if (controller.CanPlayback)
{
    controller.StartPlayback();
}

// Load and play specific recording
controller.LoadRecording("recording_20241201_143022");
controller.StartPlayback();

// Control playback
controller.StopPlayback();
```

### Storage Management

```csharp
// Get available recordings
List<string> recordings = RecordingStorage.GetAvailableRecordings();

// Get recording metadata
RecordingMetadata metadata = RecordingStorage.GetRecordingMetadata("recording_name");
Debug.Log($"Duration: {metadata.FormattedDuration}, Size: {metadata.FormattedFileSize}");

// Save custom recording
RecordingStorage.SaveRecording(recording, "my_custom_recording");

// Delete recording
RecordingStorage.DeleteRecording("old_recording");
```

## Configuration Options

### Recording Settings
- **Frame Rate**: 30 FPS (configurable)
- **Visualization**: Show joint spheres during recording
- **Debug Mode**: Detailed logging

### Playback Settings
- **Loop Playback**: Automatically restart when complete
- **Playback Speed**: Control animation speed
- **Path Visualization**: Show motion trails
- **Joint Colors**: Different colors for different joint types

### Storage Settings
- **Auto-save**: Recordings saved automatically with timestamp
- **Format**: JSON (human-readable) or Binary (compact)
- **Metadata**: Duration, frame count, file size tracking

## Troubleshooting

### Common Issues

1. **"Cannot start recording"**
   - Ensure image target is detected
   - Check that ARHumanBodyManager is in scene
   - Verify camera permissions are granted

2. **"Playback position incorrect"**
   - Ensure same image target is used for recording and playback
   - Check that image target is properly tracked
   - Verify coordinate frame transformations

3. **"No body detected during recording"**
   - Ensure good lighting conditions
   - Check that person is fully visible in camera
   - ARKit requires iOS device with A12+ chip

4. **"Recording file not found"**
   - Check Application.persistentDataPath/BodyTrackingRecordings/
   - Ensure recording was saved successfully
   - Verify file permissions

### Debug Mode

Enable debug mode in components for detailed logging:

```csharp
// In BodyTrackingController
[SerializeField] private bool debugMode = true;

// In BodyTrackingRecorder
[SerializeField] private bool debugMode = true;
```

This provides detailed logs about:
- Coordinate transformations
- Frame recording statistics
- AR tracking quality
- File operations

## Migration from Old System

### Key Changes

1. **Replace TrackingManager**: Use `BodyTrackingController` instead
2. **Update Data Format**: Old hip-only data won't work with new system
3. **New UI Integration**: Update UI to use new event-driven system
4. **Coordinate System**: New system handles transformations automatically

### Migration Steps

1. **Backup existing recordings** (if any)
2. **Remove old scripts**: `TrackingManager.cs`, `SkeletalRecorder.cs`
3. **Add new components** to scene
4. **Update UI references** to use new controller
5. **Test recording and playback** with new system

## Performance Considerations

### Recording
- **30 FPS recording**: ~1800 frames per minute
- **JSON storage**: ~500KB per minute of recording
- **Memory usage**: Minimal during recording (streaming to disk)

### Playback
- **Real-time playback**: No performance impact
- **Visualization**: 7 joint spheres + path lines (configurable)
- **Memory usage**: Full recording loaded into memory

### Storage
- **Compression**: JSON is human-readable but larger
- **Binary format**: Future enhancement for large datasets
- **Auto-cleanup**: Consider implementing recording retention policies

## Future Enhancements

### Planned Features
1. **Binary storage format** for better compression
2. **Interpolation between frames** for smoother playback
3. **Recording compression** and optimization
4. **Multi-person tracking** support
5. **Export to standard formats** (FBX, BVH)
6. **Cloud storage integration**

### Extension Points
- **Custom visualizations**: Extend `BodyTrackingPlayer`
- **Data filters**: Add noise reduction in `BodyTrackingRecorder`
- **Storage backends**: Implement cloud storage in `RecordingStorage`
- **Analysis tools**: Build motion analysis on top of `PoseRecording`

## API Reference

### Main Controller Methods
```csharp
// System control
bool Initialize()
bool StartRecording()
PoseRecording StopRecording()
bool StartPlayback()
void StopPlayback()

// Recording management
bool LoadRecording(string fileName)
List<string> GetAvailableRecordings()
RecordingMetadata GetRecordingMetadata(string fileName)

// State queries
bool CanRecord { get; }
bool CanPlayback { get; }
bool IsRecording { get; }
bool IsPlaying { get; }
```

### Events
```csharp
// Controller events
event Action<OperationMode> OnModeChanged
event Action<PoseRecording> OnRecordingComplete
event Action OnPlaybackStarted
event Action OnPlaybackStopped

// Recorder events
event Action<PoseRecording> OnRecordingComplete
event Action<float> OnRecordingProgress

// Player events
event Action OnPlaybackStarted
event Action OnPlaybackStopped
event Action<float> OnPlaybackProgress
```

This new system provides a solid foundation for body tracking applications with proper separation of concerns, consistent coordinate handling, and frame-accurate playback. 