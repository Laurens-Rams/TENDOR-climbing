# TENDOR-climbing

This repository contains a Unity-based AR climbing project.

## Body Tracking Capture

The project now includes scripts for recording ARKit body tracking data while capturing video. Use **SessionRecorder** instead of **VideoRecorder** to save a `.body` file alongside the MP4. Add **RecordPlaybackController** to a GameObject and press **R** to toggle recording or **V** to view the most recent capture via `LatestPlaybackManager`.

For touch devices, attach **RecordPlaybackUI** as well. It draws simple on‑screen buttons that call `RecordPlaybackController.ToggleRecording()` and `TogglePlayback()` so you can start and stop without a keyboard.

## Running Tests

Open the project in the Unity Editor and use **Window > General > Test Runner** to run all Edit Mode and Play Mode tests.
