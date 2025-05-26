using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

namespace TENDOR.Editor
{
    /// <summary>
    /// Helper for building iOS MVP builds
    /// </summary>
    public class iOSBuildHelper : EditorWindow
    {
        private string buildPath = "Builds/iOS";
        private string bundleIdentifier = "com.tendor.climbing";
        private string productName = "TENDOR Climbing";
        private bool developmentBuild = true;
        private bool autoRunPlayer = false;
        
        [MenuItem("TENDOR/iOS Build Helper")]
        public static void ShowWindow()
        {
            GetWindow<iOSBuildHelper>("iOS Build Helper");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("📱 iOS Build Helper", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            // Build Settings
            GUILayout.Label("Build Settings:", EditorStyles.boldLabel);
            buildPath = EditorGUILayout.TextField("Build Path:", buildPath);
            bundleIdentifier = EditorGUILayout.TextField("Bundle Identifier:", bundleIdentifier);
            productName = EditorGUILayout.TextField("Product Name:", productName);
            developmentBuild = EditorGUILayout.Toggle("Development Build:", developmentBuild);
            autoRunPlayer = EditorGUILayout.Toggle("Auto Run Player:", autoRunPlayer);
            
            GUILayout.Space(10);
            
            // Pre-build checks
            GUILayout.Label("Pre-build Checks:", EditorStyles.boldLabel);
            
            bool hasGoogleServiceInfo = File.Exists(Path.Combine(Application.dataPath, "GoogleService-Info.plist"));
            GUILayout.Label($"GoogleService-Info.plist: {(hasGoogleServiceInfo ? "✅" : "❌ Missing!")}", 
                hasGoogleServiceInfo ? EditorStyles.label : EditorStyles.miniLabel);
            
            bool isIOSPlatform = EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
            GUILayout.Label($"iOS Platform Selected: {(isIOSPlatform ? "✅" : "❌ Switch to iOS!")}", 
                isIOSPlatform ? EditorStyles.label : EditorStyles.miniLabel);
            
            GUILayout.Space(10);
            
            // Build buttons
            GUI.enabled = hasGoogleServiceInfo && isIOSPlatform;
            
            if (GUILayout.Button("🔧 Configure iOS Settings", GUILayout.Height(30)))
            {
                ConfigureiOSSettings();
            }
            
            if (GUILayout.Button("🚀 Build for iOS", GUILayout.Height(40)))
            {
                BuildForIOS();
            }
            
            GUI.enabled = true;
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("🔄 Switch to iOS Platform", GUILayout.Height(25)))
            {
                SwitchToIOSPlatform();
            }
            
            GUILayout.Space(10);
            
            // Instructions
            GUILayout.Label("Instructions:", EditorStyles.boldLabel);
            GUILayout.Label("1. Download GoogleService-Info.plist from Firebase");
            GUILayout.Label("2. Place it in Assets/ folder");
            GUILayout.Label("3. Switch to iOS platform");
            GUILayout.Label("4. Configure iOS settings");
            GUILayout.Label("5. Build for iOS");
            GUILayout.Label("6. Open Xcode project and build to device");
        }
        
        private void ConfigureiOSSettings()
        {
            Debug.Log("🔧 Configuring iOS settings...");
            
            // Player Settings
            PlayerSettings.productName = productName;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, bundleIdentifier);
            
            // iOS specific settings
            PlayerSettings.iOS.targetOSVersionString = "12.0";
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.iOS.cameraUsageDescription = "This app uses the camera for AR climbing tracking";
            
            // XR Settings
            PlayerSettings.iOS.requiresFullScreen = false;
            
            // Graphics
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.iOS.scriptCallOptimization = ScriptCallOptimizationLevel.SlowAndSafe;
            
            // Architecture
            PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 1); // ARM64
            
            Debug.Log("✅ iOS settings configured");
            EditorUtility.DisplayDialog("Settings Configured", "iOS settings have been configured for AR and Firebase.", "OK");
        }
        
        private void BuildForIOS()
        {
            Debug.Log("🚀 Starting iOS build...");
            
            // Ensure build directory exists
            Directory.CreateDirectory(buildPath);
            
            // Build options
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetEnabledScenes();
            buildPlayerOptions.locationPathName = buildPath;
            buildPlayerOptions.target = BuildTarget.iOS;
            buildPlayerOptions.options = BuildOptions.None;
            
            if (developmentBuild)
            {
                buildPlayerOptions.options |= BuildOptions.Development;
            }
            
            if (autoRunPlayer)
            {
                buildPlayerOptions.options |= BuildOptions.AutoRunPlayer;
            }
            
            // Build
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"✅ Build succeeded: {summary.outputPath}");
                EditorUtility.DisplayDialog("Build Successful", 
                    $"iOS build completed successfully!\n\nPath: {summary.outputPath}\n\nNext: Open in Xcode and build to device.", 
                    "Open Folder", "OK");
                
                if (EditorUtility.DisplayDialog("Build Successful", 
                    $"iOS build completed successfully!\n\nPath: {summary.outputPath}\n\nNext: Open in Xcode and build to device.", 
                    "Open Folder", "OK"))
                {
                    EditorUtility.RevealInFinder(summary.outputPath);
                }
            }
            else
            {
                Debug.LogError($"❌ Build failed: {summary.result}");
                EditorUtility.DisplayDialog("Build Failed", $"Build failed with result: {summary.result}", "OK");
            }
        }
        
        private void SwitchToIOSPlatform()
        {
            Debug.Log("🔄 Switching to iOS platform...");
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        }
        
        private string[] GetEnabledScenes()
        {
            var scenes = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
            {
                scenes[i] = EditorBuildSettings.scenes[i].path;
            }
            return scenes;
        }
    }
}