using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureImporter : EditorWindow
{
    [MenuItem("TENDOR/Import Custom Texture")]
    public static void ShowWindow()
    {
        GetWindow<TextureImporter>("Texture Importer");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Custom Texture Importer", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("Instructions:", EditorStyles.boldLabel);
        GUILayout.Label("1. Click 'Create Textures Folder' to create a folder for your textures");
        GUILayout.Label("2. Copy your image files to the created folder");
        GUILayout.Label("3. Return to Unity and the textures will be imported automatically");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create Textures Folder"))
        {
            CreateTexturesFolder();
        }
        
        if (GUILayout.Button("Open Textures Folder"))
        {
            OpenTexturesFolder();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Refresh Asset Database"))
        {
            AssetDatabase.Refresh();
            Debug.Log("Asset database refreshed!");
        }
    }
    
    void CreateTexturesFolder()
    {
        string folderPath = "Assets/Textures";
        
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Textures");
            Debug.Log($"✅ Created folder: {folderPath}");
        }
        else
        {
            Debug.Log($"Folder already exists: {folderPath}");
        }
        
        AssetDatabase.Refresh();
    }
    
    void OpenTexturesFolder()
    {
        string folderPath = Path.Combine(Application.dataPath, "Textures");
        
        if (!Directory.Exists(folderPath))
        {
            CreateTexturesFolder();
            folderPath = Path.Combine(Application.dataPath, "Textures");
        }
        
        // Open the folder in the system file explorer
        EditorUtility.RevealInFinder(folderPath);
        Debug.Log($"Opened folder: {folderPath}");
    }
} 