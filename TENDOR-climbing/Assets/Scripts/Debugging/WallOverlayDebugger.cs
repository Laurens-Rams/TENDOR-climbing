using UnityEngine;
using BodyTracking.AR;

namespace BodyTracking.Debug
{
    /// <summary>
    /// Debug helper to visualize wall overlay issues
    /// </summary>
    public class WallOverlayDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        public bool enableDebugLogs = true;
        public bool showWireframe = true;
        public Color wireframeColor = Color.red;
        
        private WallOverlay wallOverlay;
        private LineRenderer wireframeRenderer;
        
        void Start()
        {
            wallOverlay = GetComponent<WallOverlay>();
            if (wallOverlay == null)
            {
                Debug.LogError("[WallOverlayDebugger] No WallOverlay component found!");
                return;
            }
            
            if (showWireframe)
            {
                CreateWireframe();
            }
            
            if (enableDebugLogs)
            {
                InvokeRepeating(nameof(LogDebugInfo), 1f, 2f);
            }
        }
        
        private void CreateWireframe()
        {
            GameObject wireframeObject = new GameObject("WallOverlay_Wireframe");
            wireframeObject.transform.SetParent(transform);
            wireframeObject.transform.localPosition = Vector3.forward * -0.002f; // Slightly in front
            wireframeObject.transform.localRotation = Quaternion.identity;
            wireframeObject.transform.localScale = Vector3.one;
            
            wireframeRenderer = wireframeObject.AddComponent<LineRenderer>();
            wireframeRenderer.material = new Material(Shader.Find("Sprites/Default"));
            wireframeRenderer.startColor = wireframeColor;
            wireframeRenderer.endColor = wireframeColor;
            wireframeRenderer.startWidth = 0.01f;
            wireframeRenderer.endWidth = 0.01f;
            wireframeRenderer.positionCount = 5;
            wireframeRenderer.useWorldSpace = false;
            
            // Create wireframe rectangle
            Vector2 size = wallOverlay.wallSize;
            Vector3[] positions = new Vector3[]
            {
                new Vector3(-size.x/2, -size.y/2, 0), // Bottom left
                new Vector3(size.x/2, -size.y/2, 0),  // Bottom right
                new Vector3(size.x/2, size.y/2, 0),   // Top right
                new Vector3(-size.x/2, size.y/2, 0),  // Top left
                new Vector3(-size.x/2, -size.y/2, 0)  // Back to bottom left
            };
            
            wireframeRenderer.SetPositions(positions);
            
            Debug.Log($"[WallOverlayDebugger] Wireframe created with size: {size}");
        }
        
        private void LogDebugInfo()
        {
            if (wallOverlay == null) return;
            
            Debug.Log($"[WallOverlayDebugger] Position: {transform.position}");
            Debug.Log($"[WallOverlayDebugger] Rotation: {transform.rotation.eulerAngles}");
            Debug.Log($"[WallOverlayDebugger] Scale: {transform.localScale}");
            Debug.Log($"[WallOverlayDebugger] Wall Size: {wallOverlay.wallSize}");
            
            // Check if renderers are active
            var renderers = GetComponentsInChildren<Renderer>();
            Debug.Log($"[WallOverlayDebugger] Found {renderers.Length} renderers:");
            
            foreach (var renderer in renderers)
            {
                Debug.Log($"[WallOverlayDebugger] - Renderer: {renderer.name}, Enabled: {renderer.enabled}, Material: {renderer.material?.name}");
            }
        }
        
        void OnDrawGizmos()
        {
            if (wallOverlay != null)
            {
                // Draw gizmo wireframe
                Gizmos.color = wireframeColor;
                Gizmos.matrix = transform.localToWorldMatrix;
                
                Vector2 size = wallOverlay.wallSize;
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(size.x, size.y, 0.01f));
                
                // Draw forward direction
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(Vector3.zero, Vector3.forward * 0.5f);
            }
        }
    }
} 