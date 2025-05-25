# Hip Tracking UI Setup Guide

## Step-by-Step Unity Scene Setup

### 1. Create the Main System GameObject

1. **Create Empty GameObject**:
   - In Hierarchy: Right-click → Create Empty
   - Name it: `HipTrackingSystem`

2. **Add Main Controller**:
   - Select `HipTrackingSystem`
   - In Inspector: Add Component → `BodyTrackingController`

3. **Add AR Image Target Manager**:
   - With `HipTrackingSystem` selected
   - In Inspector: Add Component → `ARImageTargetManager`

### 2. Setup AR Components

1. **Create AR Session Origin**:
   - In Hierarchy: Right-click → XR → AR Session Origin
   - This creates the main AR camera setup

2. **Add AR Session**:
   - In Hierarchy: Right-click → Create Empty
   - Name it: `AR Session`
   - Add Component → `AR Session`

3. **Add AR Human Body Manager**:
   - Select `AR Session Origin`
   - In Inspector: Add Component → `AR Human Body Manager`

4. **Add AR Tracked Image Manager**:
   - Select `AR Session Origin`
   - In Inspector: Add Component → `AR Tracked Image Manager`
   - In `Reference Library` field: Assign your `TENDORImageLibrary` asset

### 3. Configure AR Image Target Manager

1. **Select `HipTrackingSystem`**

2. **In ARImageTargetManager component**:
   - `Tracked Image Manager`: Drag `AR Session Origin` from Hierarchy
   - `Target Image Name`: Type `"TENDORImage"` (must match your library)
   - `Wall Prefab`: (Optional) Assign wall visualization prefab if you have one
   - `Wall Rotation Offset`: Set to `(0, 0, -90)` or adjust as needed

### 4. Configure Body Tracking Controller

1. **Select `HipTrackingSystem`**

2. **In BodyTrackingController component**:
   - `Human Body Manager`: Drag `AR Session Origin` from Hierarchy
   - `Image Target Manager`: Should auto-populate with the ARImageTargetManager on same GameObject
   - `Auto Initialize`: ✓ Check this
   - `Debug Mode`: ✓ Check for detailed logging (optional)

### 5. Create UI Canvas

1. **Create Canvas**:
   - In Hierarchy: Right-click → UI → Canvas
   - Canvas settings should be Screen Space - Overlay (default)

2. **Add BodyTrackingUI Component**:
   - Select `Canvas`
   - In Inspector: Add Component → `BodyTrackingUI`

### 6. Create UI Elements

#### A. Record Button
1. **Create Button**:
   - Right-click Canvas → UI → Button - TextMeshPro
   - Name: `RecordButton`
   - Position: Top-left area
   - Button text: "Record Hip"

#### B. Stop Record Button
1. **Create Button**:
   - Right-click Canvas → UI → Button - TextMeshPro
   - Name: `StopRecordButton`
   - Position: Next to Record button
   - Button text: "Stop Record"

#### C. Play Button
1. **Create Button**:
   - Right-click Canvas → UI → Button - TextMeshPro
   - Name: `PlayButton`
   - Position: Below Record button
   - Button text: "Play Hip"

#### D. Stop Play Button
1. **Create Button**:
   - Right-click Canvas → UI → Button - TextMeshPro
   - Name: `StopPlayButton`
   - Position: Next to Play button
   - Button text: "Stop Play"

#### E. Load Button
1. **Create Button**:
   - Right-click Canvas → UI → Button - TextMeshPro
   - Name: `LoadButton`
   - Position: Below dropdown (created next)
   - Button text: "Load Selected"

#### F. Recordings Dropdown
1. **Create Dropdown**:
   - Right-click Canvas → UI → Dropdown - TextMeshPro
   - Name: `RecordingsDropdown`
   - Position: Middle area
   - Placeholder text: "Select Recording..."

#### G. Status Text
1. **Create Text**:
   - Right-click Canvas → UI → Text - TextMeshPro
   - Name: `StatusText`
   - Position: Bottom area
   - Text: "Initializing..."
   - Font Size: 16-20

#### H. Mode Text
1. **Create Text**:
   - Right-click Canvas → UI → Text - TextMeshPro
   - Name: `ModeText`
   - Position: Top-right area
   - Text: "Mode: Ready"
   - Font Size: 14

### 7. Connect UI Elements to BodyTrackingUI

1. **Select `Canvas`**

2. **In BodyTrackingUI component, assign each field**:

   - `Record Button`: Drag `RecordButton` from Hierarchy
   - `Stop Record Button`: Drag `StopRecordButton` from Hierarchy
   - `Play Button`: Drag `PlayButton` from Hierarchy
   - `Stop Play Button`: Drag `StopPlayButton` from Hierarchy
   - `Load Button`: Drag `LoadButton` from Hierarchy
   - `Status Text`: Drag `StatusText` from Hierarchy
   - `Mode Text`: Drag `ModeText` from Hierarchy
   - `Recordings Dropdown`: Drag `RecordingsDropdown` from Hierarchy
   - `Controller`: Drag `HipTrackingSystem` from Hierarchy

### 8. Final Configuration

1. **Position UI Elements**:
   - Arrange buttons and text for good user experience
   - Test different screen sizes using Game view aspect ratios

2. **Style UI Elements** (Optional):
   - Set button colors, fonts, sizes as desired
   - Add background panels for better visibility

### 9. Test Setup

1. **In Play Mode**:
   - Check Console for "Hip tracking system successfully initialized"
   - UI should show "Mode: Ready"
   - Point camera at TENDOR image to see "Image target detected"
   - Record and Play buttons should become enabled

### 10. Build Settings

1. **Player Settings**:
   - Switch platform to iOS
   - Set minimum iOS version to 11.0+
   - Enable ARKit in XR Settings

2. **Required Capabilities**:
   - Camera usage permission
   - ARKit support

## UI Layout Example

```
┌─────────────────────────────────┐
│ [Record Hip] [Stop Record]  Mode: Ready │
│ [Play Hip]   [Stop Play]               │
│                                     │
│ ┌─────────────────────────────┐        │
│ │ Select Recording...    ▼    │        │
│ └─────────────────────────────┘        │
│ [Load Selected]                    │
│                                     │
│ Status: Hip tracking system         │
│ initialized - waiting for target    │
└─────────────────────────────────┘
```

## Troubleshooting

### Common Issues:

1. **"Controller not found"**:
   - Ensure `HipTrackingSystem` GameObject exists
   - Verify `BodyTrackingController` component is attached

2. **"Image target not detected"**:
   - Check `ARTrackedImageManager` has the correct Reference Library
   - Verify image name matches exactly (`TENDORImage`)
   - Ensure good lighting and image is clearly visible

3. **Buttons not responding**:
   - Verify UI elements are properly assigned in `BodyTrackingUI`
   - Check Console for any error messages

4. **No hip visualization**:
   - Ensure device supports ARKit body tracking
   - Check that person is fully visible in camera frame
   - Verify good lighting conditions

## Testing Checklist

- [ ] UI loads without errors
- [ ] Mode text updates correctly
- [ ] Image target detection works
- [ ] Record button enables when ready
- [ ] Hip visualization appears during recording
- [ ] Recording saves successfully
- [ ] Dropdown populates with saved recordings
- [ ] Playback works with blue sphere
- [ ] Status messages update appropriately

This setup provides a complete, functional hip tracking system focused specifically on your video-to-FBX API workflow needs. 