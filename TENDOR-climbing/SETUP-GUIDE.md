# TENDOR AR Climbing App - Setup Guide

## Overview
This guide will help you set up the refactored AR climbing application with proper image tracking, recording, and playback functionality.

## Fixed Architecture

### Core Components

1. **TrackingManager** - Central coordinator for all AR functionality
2. **ViewSwitcher** - Handles mode switching between recording and playback
3. **HipsRecorder/HipsPlayback** - Basic position recording and playback
4. **SkeletalRecorder/SkeletalPlayback** - Advanced full-body motion capture
5. **UIController** - Manages button interactions and UI state
6. **SceneFixer** - Automatically fixes scene references at runtime

### Data Flow

```
Recording Mode:
ViewSwitcher.SetARMode(false) → 
TrackingManager waits for image detection → 
Places wall at detected image → 
Initializes HipsRecorder/SkeletalRecorder → 
Records avatar movement relative to wall → 
Saves JSON data

AR Playback Mode:
ViewSwitcher.SetARMode(true) → 
TrackingManager detects image → 
Places wall at detected image → 
Initializes HipsPlayback/SkeletalPlayback → 
Loads JSON data → 
Plays animation relative to wall position
```

## Setup Instructions

### 1. Scene Configuration

#### Option A: Automatic Fix (Recommended)
1. Add the `SceneFixer` component to any GameObject in your scene
2. Set "Auto Fix On Start" to true
3. Run the scene - it will automatically fix all references

#### Option B: Manual Setup
1. Find the "ViewSwitcher" GameObject in your scene
2. In the ViewSwitcher component, assign:
   - `trackingManager`: Reference to TrackingManager component
   - `arUI`: The AR mode UI panel
   - `recordUI`: The recording mode UI panel

### 2. Add Required Components

Ensure these components exist in your scene:
- `TrackingManager` (add to "Image Tracking" GameObject)
- `HipsRecorder` (can be on any GameObject)
- `HipsPlayback` (can be on any GameObject) 
- `SkeletalRecorder` (can be on any GameObject)
- `SkeletalPlayback` (can be on any GameObject)
- `UIController` (add to a UI GameObject)

### 3. Configure TrackingManager

In the TrackingManager component, assign:

#### Wall Settings:
- `wallPrefab`: Your climbing wall 3D model
- `wallScaleFactor`: Scaling factor for the wall (default: 1.0)

#### Recording Settings:
- `recordingAvatarPrefab`: Avatar used during recording
- `useSkeletalRecording`: True for full-body capture, false for basic

#### Playback Settings:
- `playbackAvatarPrefab`: Avatar used during playback
- `skeletonPrefab`: Skeleton visualization prefab

#### UI:
- `debugText`: Text component for status messages

### 4. Configure UI Buttons

The UIController will automatically find and configure these buttons:
- "RECORD" - Starts recording
- "STOP" - Stops recording  
- "SETARMODE" - Switches to AR playback mode
- "SETRECORDMODE" - Switches to recording mode

### 5. AR Foundation Setup

Ensure your scene has:
- `ARSessionOrigin` with `ARCamera`
- `ARSession` 
- `ARTrackedImageManager` with image library
- `ARHumanBodyManager` (for skeletal recording)

## Recording Modes

### Basic Recording (HipsRecorder)
- Records only the main avatar position and rotation
- Lighter weight, simpler data
- Good for basic climbing movement replay
- File saved as: `hips.json`

### Skeletal Recording (SkeletalRecorder)
- Records full body joint data from ARKit/ARCore
- More detailed climbing movements
- Requires device with human body tracking
- File saved as: `skeletal_recording.json`

## Usage Flow

1. **Start Application**: SceneFixer automatically configures everything
2. **Select Mode**: Use mode switch buttons to choose Recording or AR Playback
3. **Scan Image Target**: Point camera at image target to place wall
4. **Recording Mode**: 
   - Avatar appears on wall
   - Press RECORD to start capturing movement
   - Press STOP to save recording
5. **AR Playback Mode**:
   - Scan image target to place wall
   - Recorded animation automatically plays on wall

## Troubleshooting

### No Image Detection
- Check AR image library is assigned to ARTrackedImageManager
- Ensure image targets are properly configured
- Verify camera permissions

### Recording Not Starting
- Make sure image target is detected first
- Check debug text for status messages
- Verify all prefabs are assigned in TrackingManager

### Missing Components Error
- Run SceneFixer to automatically add missing components
- Or manually add components listed in setup section

### Animation Not Playing
- Check that recording file exists
- Verify recording was completed successfully
- Ensure wall placement is working

## File Locations

Recorded data is saved to:
- Basic recording: `Application.persistentDataPath/hips.json`
- Skeletal recording: `Application.persistentDataPath/skeletal_recording.json`

## Performance Notes

- Skeletal recording requires more processing power
- Basic recording is recommended for older devices
- Recording frame rate can be adjusted in SkeletalRecorder settings

## Development Notes

- All scripts use consistent logging with component prefixes
- Transform math handles wall-relative positioning correctly
- System gracefully handles missing components and failed initialization
- UI provides clear feedback for all states 