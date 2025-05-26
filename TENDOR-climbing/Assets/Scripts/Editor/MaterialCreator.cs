using UnityEngine;
using UnityEditor;
using System.IO;

public class MaterialCreator : EditorWindow
{
    private string materialName = "MyCustomMaterial";
    private Shader selectedShader;
    
    [MenuItem("TENDOR/Create Custom Material")]
    public static void ShowWindow()
    {
        GetWindow<MaterialCreator>("Material Creator");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Custom Material Creator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("The issue you're experiencing happens when trying to modify", EditorStyles.wordWrappedLabel);
        GUILayout.Label("materials from Unity packages. Let's create your own material!", EditorStyles.wordWrappedLabel);
        
        GUILayout.Space(10);
        
        materialName = EditorGUILayout.TextField("Material Name", materialName);
        
        GUILayout.Space(10);
        
        selectedShader = (Shader)EditorGUILayout.ObjectField("Shader", selectedShader, typeof(Shader), false);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create Standard Material"))
        {
            CreateMaterial("Standard");
        }
        
        if (GUILayout.Button("Create URP/Lit Material"))
        {
            CreateMaterial("Universal Render Pipeline/Lit");
        }
        
        if (GUILayout.Button("Create URP/Unlit Material"))
        {
            CreateMaterial("Universal Render Pipeline/Unlit");
        }
        
        GUILayout.Space(10);
        
        if (selectedShader != null && GUILayout.Button($"Create Material with {selectedShader.name}"))
        {
            CreateMaterialWithShader(selectedShader);
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create Materials Folder"))
        {
            CreateMaterialsFolder();
        }
        
        GUILayout.Space(10);
        
        GUILayout.Label("Quick Fixes:", EditorStyles.boldLabel);
        if (GUILayout.Button("Fix Current Scene Materials"))
        {
            FixSceneMaterials();
        }
    }
    
    void CreateMaterial(string shaderName)
    {
        Shader shader = Shader.Find(shaderName);
        if (shader == null)
        {
            Debug.LogError($"Could not find shader: {shaderName}");
            return;
        }
        
        CreateMaterialWithShader(shader);
    }
    
    void CreateMaterialWithShader(Shader shader)
    {
        // Ensure Materials folder exists
        CreateMaterialsFolder();
        
        // Create the material
        Material newMaterial = new Material(shader);
        
        // Set some default properties
        if (shader.name.Contains("Standard") || shader.name.Contains("Lit"))
        {
            newMaterial.SetFloat("_Metallic", 0f);
            newMaterial.SetFloat("_Smoothness", 0.5f);
            newMaterial.SetColor("_Color", Color.white);
            newMaterial.SetColor("_BaseColor", Color.white);
        }
        
        // Save the material
        string path = $"Assets/Materials/{materialName}.mat";
        AssetDatabase.CreateAsset(newMaterial, path);
        AssetDatabase.SaveAssets();
        
        // Select the new material
        Selection.activeObject = newMaterial;
        EditorGUIUtility.PingObject(newMaterial);
        
        Debug.Log($"✅ Created material: {path}");
        Debug.Log($"📁 Material is now editable and located in your project!");
    }
    
    void CreateMaterialsFolder()
    {
        string folderPath = "Assets/Materials";
        
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
            Debug.Log($"✅ Created folder: {folderPath}");
        }
        
        AssetDatabase.Refresh();
    }
    
    void FixSceneMaterials()
    {
        // Find all renderers in the scene
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        int fixedCount = 0;
        
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            bool needsUpdate = false;
            
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(materials[i]);
                    
                    // Check if material is from a package (outside project)
                    if (assetPath.Contains("PackageCache") || assetPath.Contains("Library"))
                    {
                        // Create a copy of this material in our project
                        Material newMaterial = new Material(materials[i]);
                        
                        CreateMaterialsFolder();
                        string newPath = $"Assets/Materials/{materials[i].name}_Copy.mat";
                        
                        // Make sure we don't overwrite existing materials
                        int counter = 1;
                        while (AssetDatabase.LoadAssetAtPath<Material>(newPath) != null)
                        {
                            newPath = $"Assets/Materials/{materials[i].name}_Copy_{counter}.mat";
                            counter++;
                        }
                        
                        AssetDatabase.CreateAsset(newMaterial, newPath);
                        materials[i] = newMaterial;
                        needsUpdate = true;
                        fixedCount++;
                        
                        Debug.Log($"✅ Fixed material: {materials[i].name} -> {newPath}");
                    }
                }
            }
            
            if (needsUpdate)
            {
                renderer.materials = materials;
            }
        }
        
        AssetDatabase.SaveAssets();
        
        if (fixedCount > 0)
        {
            Debug.Log($"🎉 Fixed {fixedCount} materials! They are now editable in your project.");
        }
        else
        {
            Debug.Log("No materials needed fixing - all materials are already in your project!");
        }
    }
} 