using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class iOSPostProcessor
{
    [PostProcessBuild(999)] // Run after other post-processors
    public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            AddPhotosLibraryPermission(pathToBuiltProject);
            AddPhotosFramework(pathToBuiltProject);
        }
    }

    private static void AddPhotosLibraryPermission(string pathToBuiltProject)
    {
        // Get plist
        string plistPath = pathToBuiltProject + "/Info.plist";
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        // Get root
        PlistElementDict rootDict = plist.root;

        // Add Photos library usage description
        rootDict.SetString("NSPhotoLibraryAddUsageDescription", 
            "TENDOR Climbing needs access to save recorded climbing videos to your Photos library.");

        // Write to file
        File.WriteAllText(plistPath, plist.WriteToString());
        
        Debug.Log("[iOSPostProcessor] Added Photos library permission to Info.plist");
    }

    private static void AddPhotosFramework(string pathToBuiltProject)
    {
        // Get the Xcode project
        string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
        
        if (!File.Exists(projPath))
        {
            Debug.LogError($"[iOSPostProcessor] Xcode project file not found: {projPath}");
            return;
        }
        
        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));

        // Get the target GUID - try both Unity main target and UnityFramework
        string mainTargetGuid = proj.GetUnityMainTargetGuid();
        string frameworkTargetGuid = proj.GetUnityFrameworkTargetGuid();
        
        Debug.Log($"[iOSPostProcessor] Main target GUID: {mainTargetGuid}");
        Debug.Log($"[iOSPostProcessor] Framework target GUID: {frameworkTargetGuid}");

        // Add Photos framework to both targets
        if (!string.IsNullOrEmpty(mainTargetGuid))
        {
            proj.AddFrameworkToProject(mainTargetGuid, "Photos.framework", true); // true = weak link
            Debug.Log("[iOSPostProcessor] Added Photos.framework to main target (weak link)");
        }
        
        if (!string.IsNullOrEmpty(frameworkTargetGuid))
        {
            proj.AddFrameworkToProject(frameworkTargetGuid, "Photos.framework", true); // true = weak link
            Debug.Log("[iOSPostProcessor] Added Photos.framework to framework target (weak link)");
        }

        // Write the project file
        File.WriteAllText(projPath, proj.WriteToString());
        
        Debug.Log("[iOSPostProcessor] Added Photos.framework to Xcode project");
    }
} 