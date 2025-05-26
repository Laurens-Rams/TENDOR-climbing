# Unity Scene Setup Fix Guide

## 🚨 CRITICAL ISSUES IDENTIFIED

Your Unity scene is missing essential components and script connections. Here's how to fix it:

## 📋 STEP 1: Add Missing AR Foundation Components

### 1.1 Add ARHumanBodyManager
1. Select the **XR Origin (AR Rig)** GameObject in the scene
2. In Inspector, click **Add Component**
3. Search for and add **AR Human Body Manager**
4. Configure settings:
   - Human Body Prefab: Leave empty for now
   - Maximum Human Bodies: 1

### 1.2 Add ARTrackedImageManager  
1. Select the **XR Origin (AR Rig)** GameObject
2. In Inspector, click **Add Component**
3. Search for and add **AR Tracked Image Manager**
4. Configure settings:
   - Reference Image Library: Assign `TENDORImageLibrary` from `Assets/ImageTracking/`
   - Max Number of Moving Images: 1

## 📋 STEP 2: Create Main System GameObject

### 2.1 Create BodyTrackingController GameObject
1. Right-click in Hierarchy → **Create Empty**
2. Rename to **"BodyTrackingSystem"**
3. Add Component → Search for **"BodyTrackingController"**
4. Configure in Inspector:
   - **Human Body Manager**: Drag the XR Origin (AR Rig) here
   - **Auto Initialize**: ✅ Check this
   - **Status Text**: Drag one of the StatusText UI elements

### 2.2 Add ARImageTargetManager
1. Select the **BodyTrackingSystem** GameObject
2. Add Component → Search for **"ARImageTargetManager"**
3. Configure:
   - **Tracked Image Manager**: Drag XR Origin (AR Rig) here
   - **Target Image Name**: "Wall 1"
   - **Wall Prefab**: Assign `WallOverlay` prefab from Assets/Prefabs/

### 2.3 Add Recording Components
1. Select **BodyTrackingSystem** GameObject
2. Add Component → **"BodyTrackingRecorder"**
3. Add Component → **"BodyTrackingPlayer"**

## 📋 STEP 3: Fix UI Connections

### 3.1 Add BodyTrackingUI Script
1. Select the **Canvas** GameObject
2. Add Component → Search for **"BodyTrackingUI"**
3. Configure all UI references:
   - **Controller**: Drag BodyTrackingSystem GameObject
   - **Record Button**: Drag RecordButton from scene
   - **Stop Record Button**: Drag StopRecordButton from scene  
   - **Play Button**: Drag PlayButton from scene
   - **Stop Play Button**: Drag StopPlayButton from scene
   - **Load Button**: Drag LoadButton from scene
   - **Status Text**: Drag StatusText from scene
   - **Mode Text**: Drag ModeText from scene
   - **Recordings Dropdown**: Drag Dropdown from scene

### 3.2 Connect BodyTrackingController References
1. Select **BodyTrackingSystem** GameObject
2. In BodyTrackingController component:
   - **Image Target Manager**: Drag the same GameObject (self-reference)
   - **Recorder**: Drag the same GameObject (self-reference)  
   - **Player**: Drag the same GameObject (self-reference)

## 📋 STEP 4: Remove Broken References

### 4.1 Fix HipTrackingSystem GameObject
1. Find **HipTrackingSystem** in hierarchy
2. Remove the broken script component (shows as "Missing Script")
3. Either delete this GameObject or repurpose it

## 📋 STEP 5: Verify Setup

### 5.1 Check Dependencies
- ✅ AR Session exists
- ✅ XR Origin (AR Rig) with ARHumanBodyManager and ARTrackedImageManager
- ✅ BodyTrackingSystem with all required scripts
- ✅ Canvas with BodyTrackingUI script
- ✅ All UI elements connected

### 5.2 Test in Play Mode
1. Enter Play Mode
2. Check Console for any errors
3. Verify status text shows: "Hip tracking system initialized - waiting for image target"
4. Test buttons (should be disabled until image target detected)

## 🎯 EXPECTED RESULT

After following these steps:
- ✅ No missing script errors
- ✅ Status text updates properly
- ✅ Buttons enable/disable based on system state
- ✅ AR image tracking works
- ✅ Body tracking recording/playback functions

## 🔧 TROUBLESHOOTING

### If you get compilation errors:
1. Check all script files exist in Assets/Scripts/
2. Verify namespace declarations match
3. Check for missing using statements

### If AR doesn't work:
1. Verify TENDORImageLibrary is assigned
2. Check image target "Wall 1" exists in library
3. Ensure proper AR permissions in build settings

### If UI doesn't respond:
1. Verify EventSystem exists in scene
2. Check all button OnClick events are connected
3. Ensure Canvas has GraphicRaycaster component

## 📱 BUILD SETTINGS

For iOS/Android builds, ensure:
- XR Plug-in Management → ARFoundation enabled
- Player Settings → XR Settings configured
- Camera permissions enabled
- Target SDK versions correct 