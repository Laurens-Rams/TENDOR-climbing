# TENDOR-climbing

This repository contains a Unity-based AR climbing project.

## Recording & Playback

The project now includes a **SessionRecorder** that captures both RGB video and
ARKit body tracking data. Each recording creates an MP4 and a matching `.body`
file under `Application.persistentDataPath/Captures/`.

Use **ModeSwitcher** to toggle between recording and playback. When switching
to playback, the latest session file is loaded and a humanoid avatar is spawned
via `PlaybackManager`.

## Running Tests

Open the project in the Unity Editor and use **Window > General > Test Runner** to run all Edit Mode and Play Mode tests.
