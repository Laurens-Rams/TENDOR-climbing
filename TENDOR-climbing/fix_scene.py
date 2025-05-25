#!/usr/bin/env python3

import re

# Read the scene file
with open('TENDOR-climbing/Assets/Scenes/TENDOR-App.unity', 'r') as f:
    content = f.read()

# Find the TrackingManager component (fileID 1179582792) and add the configuration
# Look for the pattern: m_EditorClassIdentifier: followed by --- !u!1
pattern = r'(m_Script: \{fileID: 11500000, guid: 862bec245ffee44b38bda305b5d146dc, type: 3\}\s+m_Name:\s+m_EditorClassIdentifier: )\s+(--- !u!1)'

replacement = r'''\1
  wallPrefab: {fileID: 0}
  wallScaleFactor: 1
  recordingAvatarPrefab: {fileID: 0}
  useSkeletalRecording: 1
  playbackAvatarPrefab: {fileID: 0}
  skeletonPrefab: {fileID: 0}
  debugText: {fileID: 0}
\2'''

# Apply the replacement
fixed_content = re.sub(pattern, replacement, content, flags=re.MULTILINE | re.DOTALL)

# Write the fixed content back
with open('TENDOR-climbing/Assets/Scenes/TENDOR-App.unity', 'w') as f:
    f.write(fixed_content)

print("Scene file fixed successfully!") 