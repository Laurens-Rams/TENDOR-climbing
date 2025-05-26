using UnityEngine;
using UnityEditor;

public class MaterialTextureHelper : EditorWindow
{
    private Material targetMaterial;
    private Texture2D albedoTexture;
    private Texture2D normalTexture;
    private Texture2D metallicTexture;
    
    [MenuItem("TENDOR/Material Texture Helper")]
    public static void ShowWindow()
    {
        GetWindow<MaterialTextureHelper>("Material Texture Helper");
    }
    
    // Static method for batch mode execution
    public static void QuickSetupFromCommandLine()
    {
        Debug.Log("🎯 Starting automatic texture setup...");
        
        // Find all materials in the Materials folder
        string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Materials" });
        
        if (materialGuids.Length == 0)
        {
            Debug.LogWarning("No materials found in Assets/Materials folder");
            return;
        }
        
        // Load the PlanePatternDot texture
        string texturePath = "Assets/Samples/XR Interaction Toolkit/3.0.7/AR Starter Assets/Textures/PlanePatternDot.png";
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        
        if (texture == null)
        {
            Debug.LogError("Could not find PlanePatternDot texture for automatic setup");
            return;
        }
        
        int appliedCount = 0;
        
        foreach (string guid in materialGuids)
        {
            string materialPath = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            
            if (material != null)
            {
                // Apply texture to both Standard and URP material properties
                material.SetTexture("_MainTex", texture);
                material.SetTexture("_BaseMap", texture); // For URP
                
                EditorUtility.SetDirty(material);
                appliedCount++;
                
                Debug.Log($"✅ Applied texture to: {material.name}");
            }
        }
        
        AssetDatabase.SaveAssets();
        
        Debug.Log($"🎉 Automatic texture setup complete! Applied textures to {appliedCount} materials.");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Material Texture Helper", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        targetMaterial = (Material)EditorGUILayout.ObjectField("Target Material", targetMaterial, typeof(Material), false);
        
        GUILayout.Space(10);
        
        albedoTexture = (Texture2D)EditorGUILayout.ObjectField("Albedo Texture", albedoTexture, typeof(Texture2D), false);
        normalTexture = (Texture2D)EditorGUILayout.ObjectField("Normal Texture", normalTexture, typeof(Texture2D), false);
        metallicTexture = (Texture2D)EditorGUILayout.ObjectField("Metallic Texture", metallicTexture, typeof(Texture2D), false);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Apply Textures to Material"))
        {
            ApplyTextures();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Quick Setup - Use Available Textures"))
        {
            QuickSetupTextures();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Auto-Apply to All Materials in Project"))
        {
            QuickSetupFromCommandLine();
        }
        
        GUILayout.Space(10);
        
        GUILayout.Label("Available Textures:", EditorStyles.boldLabel);
        if (GUILayout.Button("Load PlanePatternDot (AR Texture)"))
        {
            LoadTexture("Assets/Samples/XR Interaction Toolkit/3.0.7/AR Starter Assets/Textures/PlanePatternDot.png");
        }
        
        if (GUILayout.Button("Load ConcreteNormal (Normal Map)"))
        {
            LoadTexture("Assets/Samples/XR Interaction Toolkit/3.0.7/AR Starter Assets/ARDemoSceneAssets/Textures/ConcreteNormal.tif");
        }
        
        if (GUILayout.Button("Load DefaultMaterial_AO"))
        {
            LoadTexture("Assets/Samples/XR Interaction Toolkit/3.0.7/Starter Assets/Textures/DefaultMaterial_AO.png");
        }
    }
    
    void LoadTexture(string path)
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texture != null)
        {
            albedoTexture = texture;
            Debug.Log($"Loaded texture: {texture.name}");
        }
        else
        {
            Debug.LogError($"Could not load texture at path: {path}");
        }
    }
    
    void ApplyTextures()
    {
        if (targetMaterial == null)
        {
            Debug.LogError("No target material selected!");
            return;
        }
        
        if (albedoTexture != null)
        {
            targetMaterial.SetTexture("_MainTex", albedoTexture);
            targetMaterial.SetTexture("_BaseMap", albedoTexture); // For URP
            Debug.Log($"Applied albedo texture: {albedoTexture.name}");
        }
        
        if (normalTexture != null)
        {
            targetMaterial.SetTexture("_BumpMap", normalTexture);
            targetMaterial.SetTexture("_NormalMap", normalTexture); // For URP
            Debug.Log($"Applied normal texture: {normalTexture.name}");
        }
        
        if (metallicTexture != null)
        {
            targetMaterial.SetTexture("_MetallicGlossMap", metallicTexture);
            Debug.Log($"Applied metallic texture: {metallicTexture.name}");
        }
        
        EditorUtility.SetDirty(targetMaterial);
        AssetDatabase.SaveAssets();
        
        Debug.Log("✅ Textures applied successfully!");
    }
    
    void QuickSetupTextures()
    {
        if (targetMaterial == null)
        {
            Debug.LogError("No target material selected!");
            return;
        }
        
        // Try to load and apply the PlanePatternDot texture
        string texturePath = "Assets/Samples/XR Interaction Toolkit/3.0.7/AR Starter Assets/Textures/PlanePatternDot.png";
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        
        if (texture != null)
        {
            targetMaterial.SetTexture("_MainTex", texture);
            targetMaterial.SetTexture("_BaseMap", texture); // For URP
            albedoTexture = texture;
            
            EditorUtility.SetDirty(targetMaterial);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"✅ Quick setup complete! Applied {texture.name} to {targetMaterial.name}");
        }
        else
        {
            Debug.LogError("Could not find PlanePatternDot texture for quick setup");
        }
    }
} 