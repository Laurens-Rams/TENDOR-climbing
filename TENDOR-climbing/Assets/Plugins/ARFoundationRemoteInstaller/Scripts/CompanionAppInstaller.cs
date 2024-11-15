#if AR_FOUNDATION_REMOTE_INSTALLED
    using ARFoundationRemote.Runtime;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;


namespace ARFoundationRemote.Editor {
    internal class CompanionAppInstaller: IPreprocessBuildWithReport, IPostprocessBuildWithReport {
        const string appName = "ARCompanion";
        const string arCompanionDefine = "AR_COMPANION";


        int IOrderedCallback.callbackOrder => 0;

        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report) {
            log($"CompanionAppInstaller IPreprocessBuildWithReport.OnPreprocessBuild, isBuildingCompanionApp(report): {isBuildingCompanionApp(report)}, defines: {scriptingDefineSymbols}, isCloudBuild: {isCloudBuild}");
            if (isCloudBuild && isARCompanionDefineSet) {
                var settings = new InstallerSettings {
                    modifyAppId = false,
                    modifyAppName = false,
                    removeCompanionAppDefineAfterBuild = false
                };
                if (!prepareBuild(settings)) {
                    throw new BuildFailedException($"{ARFoundationRemoteInstaller.displayName}: build failed.");
                }

                return;
            }

            if (!isBuildingCompanionApp(report)) {
                if (EditorBuildSettings.scenes.Any(_ => _.path.Contains("ARCompanion.unity"))) {
                    throw new BuildFailedException(
                        $"{ARFoundationRemoteInstaller.displayName}: please build the companion app via Installer object by pressing 'Install AR Companion App' or 'Build AR Companion and show in folder' button.");
                }
            }
        }

        static string applicationIdentifier {
            get => PlayerSettings.GetApplicationIdentifier(activeBuildTargetGroup);
            set => PlayerSettings.SetApplicationIdentifier(activeBuildTargetGroup, value);
        }

        void IPostprocessBuildWithReport.OnPostprocessBuild(BuildReport report) {
            if (isCloudBuild) {
                Debug.Log(appName + " UCB build succeeded.");
            }
        }

        static void restoreChanges(InstallerSettings settings) {
            log("restoreChanges()");
            if (settings.modifyAppId) {
                applicationIdentifier = removeAppName(applicationIdentifier);
            }

            if (settings.modifyAppName) {
                PlayerSettings.productName = removeAppName(PlayerSettings.productName);
            }

            if (settings.removeCompanionAppDefineAfterBuild) {
                toggleDefine(arCompanionDefine, false);
            }
        }

        static void toggleDefine(string define, bool enable) {
            if (enable) {
                if (isDefineSet(define)) {
                    return;
                }

                var defines = scriptingDefineSymbols;
                setDefines($"{defines};{define}");
            } else {
                if (!isDefineSet(define)) {
                    return;
                }

                var defines = scriptingDefineSymbols.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
                setDefines(string.Join(";", defines.Where(d => d.Trim() != define).ToArray()));
            }

            void setDefines(string defines) {
                log($"set defines: {defines}");
                scriptingDefineSymbols = defines;
            }
        }

        static string scriptingDefineSymbols {
            get {
                #if UNITY_2023_1_OR_NEWER
                return PlayerSettings.GetScriptingDefineSymbols(activeBuildTargetGroup);
                #else
                return PlayerSettings.GetScriptingDefineSymbolsForGroup(activeBuildTargetGroup);
                #endif
            }
            set {
                #if UNITY_2023_1_OR_NEWER
                PlayerSettings.SetScriptingDefineSymbols(activeBuildTargetGroup, value);
                #else
                PlayerSettings.SetScriptingDefineSymbolsForGroup(activeBuildTargetGroup, value);
                #endif
            }
        }

        static
            #if UNITY_2023_1_OR_NEWER
            NamedBuildTarget
            #else
            BuildTargetGroup
            #endif
            activeBuildTargetGroup {
            get {
                var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                #if UNITY_2023_1_OR_NEWER
                return NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
                #else
                return buildTargetGroup;
                #endif
            }
        }

        static bool isDefineSet(string define) {
            return scriptingDefineSymbols.Contains(define);
        }

        static bool isCloudBuild {
            get {
                #if UNITY_CLOUD_BUILD
                    return true;
                #else
                    return false;
                #endif
            }
        }

        static bool isARCompanionDefineSet {
            get {
                #if AR_COMPANION
                    return true;
                #else
                    return false;
                #endif
            }
        }

        /// returns false in UCB because <see cref="companionAppFolder"/> path is not used while building with UCB
        static bool isBuildingCompanionApp(BuildReport report) {
            var result = report.summary.outputPath.Contains(companionAppFolder);
            log("isBuildingCompanionApp " + result);
            return result;
        }

        static string removeAppName(string s) {
            return s.Replace(appName, "");
        }

        public static void Build(InstallerSettings settings) {
            build(buildOptions | BuildOptions.ShowBuiltPlayer, settings);
        }

        public static void BuildAndRun(InstallerSettings settings) {
            build(buildOptions | BuildOptions.AutoRunPlayer, settings);
        }

        static bool prepareBuild(InstallerSettings settings) {
            var listRequest = Client.List(true, true);
            ARFoundationRemoteInstaller.runPackageManagerRequestBlocking(listRequest);
            if (listRequest.Status != StatusCode.Success) {
                Debug.LogError("ARFoundationRemoteInstaller can't check installed packages.");
                return false;
            }

            if (!isPresent(ARFoundationRemoteInstaller.pluginId)) {
                Debug.LogError("Please install " + ARFoundationRemoteInstaller.displayName);
                return false;
            }

            #if AR_FOUNDATION_REMOTE_INSTALLED
            if (!Global.ShouldRunWithCurrentBuildTarget()) {
                var settingName = ObjectNames.NicifyVariableName(nameof(Settings.Instance.debugSettings.enablePluginInAllBuildTargets));
                Debug.LogError($"{Constants.packageName}: please switch Unity Editor to supported build target (iOS/Android). Or enable the '{settingName}' in plugin's debug settings.\n");
                return false;
            }

            var instance = Settings.Instance;
            instance.packages = PackageVersionData.Create(listRequest.Result);
            instance.inputHandlingData = InputHandlingData.Create();
            EditorUtility.SetDirty(instance);

            if (Defines.isAndroid) {
                if (!isPresent("com.unity.xr.arcore")) {
                    logPluginNotInstalledError("ARCore");
                    return false;
                }
            } else if (Defines.isIOS) {
                if (!isPresent("com.unity.xr.arkit") && !isPresent("com.unity.xr.arkit-face-tracking")) {
                    logPluginNotInstalledError("ARKit");
                    return false;
                }
            }
            if (!isAnyLoaderEnabledForCurrentPlatform()) {
                string providerName;
                var activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
                switch (activeBuildTarget) {
                    case BuildTarget.Android:
                        providerName = "ARCore";
                        break;
                    case BuildTarget.iOS:
                        providerName = "ARKit";
                        break;
                    default:
                        providerName = "AR Provider";
                        break;
                }
                Debug.LogError($"Please enable '{providerName}' in the 'XR Plug-in Management' window. Or switch the 'Build Setting/Platform' to another target. Current build target: {activeBuildTarget}.\n");
                return false;
            }

            void logPluginNotInstalledError(string packageName) {
                Debug.LogError($"Please install '{packageName}' via Package Manager and ENABLE IT in 'XR Plug-in Management' window.");
            }
            #endif

            bool isPresent(string packageId) {
                return listRequest.Result.SingleOrDefault(_ => _.name == packageId) != null;
            }

            if (settings.modifyAppId && !isCloudBuild) {
                applicationIdentifier = removeAppName(applicationIdentifier) + appName;
            }

            if (settings.modifyAppName) {
                PlayerSettings.productName = appName + removeAppName(PlayerSettings.productName);
            }

            toggleDefine(arCompanionDefine, true);
            return true;
        }

        static bool isAnyLoaderEnabledForCurrentPlatform() {
            #if !XR_MANAGEMENT_INSTALLED
            return false;
            #else
            if (!EditorBuildSettings.TryGetConfigObject(UnityEngine.XR.Management.XRGeneralSettings.k_SettingsKey, out UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget buildTargetSettings)) {
                return false;
            }
            var settings = buildTargetSettings.SettingsForBuildTarget(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            if (settings == null) {
                return false;
            }
            var manager = settings.Manager;
            if (manager == null) {
                return false;
            }
            var loaders = getLoaders();
            if (loaders == null) {
                return false;
            }
            // Debug.Log($"loaders: {string.Join(", ", loaders)}");
            return loaders.Count > 0;

            IReadOnlyList<UnityEngine.XR.Management.XRLoader> getLoaders() {
                return manager.
                    #if XR_MANAGEMENT_4_0_1_OR_NEWER
                    activeLoaders;
                    #else
                    loaders;
                    #endif
            }
            #endif
        }

        static void build(BuildOptions options, InstallerSettings settings) {
            if (!prepareBuild(settings)) {
                return;
            }

            var scenes = getSenderScenePaths().Select(_ => _.ToString()).ToArray();
            try {
                var report = BuildPipeline.BuildPlayer(scenes, getInstallDirectory() + EditorUserBuildSettings.activeBuildTarget + getExtension(settings.optionalCompanionAppExtension), EditorUserBuildSettings.activeBuildTarget, options);
                if (report.summary.result == BuildResult.Succeeded) {
                    Debug.Log(appName + " build succeeded.");
                }
            } finally {
                restoreChanges(settings);
            }
        }

        static string getExtension(string optionalCompanionAppExtension) {
            switch (EditorUserBuildSettings.activeBuildTarget) {
                case BuildTarget.iOS:
                    return "";
                case BuildTarget.Android:
                    return ".apk";
                default:
                    if (string.IsNullOrEmpty(optionalCompanionAppExtension)) {
                        Debug.LogWarning("Please specify optionalCompanionAppExtension if your target platform needs one. For example, Android target requires .apk extension for the builds.");
                    } else {
                        Debug.Log("Using optionalCompanionAppExtension: " + optionalCompanionAppExtension);
                    }
                    return optionalCompanionAppExtension;
            }
        }

        /// Adding BuildOptions.AcceptExternalModificationsToPlayer will produce gradle build for android instead of apk
        /// Enable BuildOptions.Development for assertions to work
        static BuildOptions buildOptions => BuildOptions.Development;

        static string getInstallDirectory() {
            var directoryInfo = Directory.GetParent(Application.dataPath);
            Assert.IsNotNull(directoryInfo);
            return directoryInfo.FullName + "/" + companionAppFolder + "/";
        }

        static string companionAppFolder => "ARFoundationRemoteCompanionApp";

        static IEnumerable<FileInfo> getSenderScenePaths() {
            return new DirectoryInfo(Application.dataPath + "/Plugins/ARFoundationRemoteInstaller/Resources")
                .GetFiles("*.unity");
        }

        public static void DeleteCompanionAppBuildFolder() {
            var path = getInstallDirectory();
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }
        }


        [Conditional("_")]
        static void log(string msg) {
            Debug.Log(msg);
        }
    }
}
