using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class iOSPostProcessor
{
    [PostProcessBuild]
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
        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));

        // Get the target GUID
        string targetGuid = proj.GetUnityMainTargetGuid();

        // Add Photos framework
        proj.AddFrameworkToProject(targetGuid, "Photos.framework", false);

        // Write the project file
        File.WriteAllText(projPath, proj.WriteToString());
        
        Debug.Log("[iOSPostProcessor] Added Photos.framework to Xcode project");
    }
} 