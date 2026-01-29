using UnityEngine;
using Visual;
using System.Text.RegularExpressions;

namespace Core
{
    /// <summary>
    /// The "Brain" of the Semantic Architecture.
    /// Parses object names to infer their logical and visual role in the world.
    /// "Scripts are Smart, FBX is dumb."
    /// </summary>
    [ExecuteAlways]
    public class AssetSemanticResolver : MonoBehaviour
    {
        // Singleton access for runtime resolving if needed
        public static AssetSemanticResolver Instance;

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Can be called manually via context menu to fix up the scene in Editor.
        /// </summary>
        [ContextMenu("Force Resolve Scene")]
        public void ForceResolveScene()
        {
            var allObjects = FindObjectsOfType<Transform>();
            foreach (var t in allObjects)
            {
                Resolve(t.gameObject);
            }
            Debug.Log($"[AssetSemanticResolver] Scanned {allObjects.Length} objects.");
        }

        public static void Resolve(GameObject obj)
        {
            string name = obj.name;

            // 1. NEON LOGIC
            if (name.StartsWith("Neon_", System.StringComparison.OrdinalIgnoreCase))
            {
                // Ensure it has a binder
                if (!obj.TryGetComponent<NeonBinder>(out var binder))
                {
                    binder = obj.AddComponent<NeonBinder>();
                    // Try to infer channel from name, e.g., Neon_Cyber_Sign -> Channel: Cyber
                    string[] parts = name.Split('_');
                    if (parts.Length > 1)
                    {
                        binder.channelID = parts[1]; // Simple inference
                    }
                }
            }

            // 2. NATURE LOGIC
            if (name.StartsWith("Tree_", System.StringComparison.OrdinalIgnoreCase) || 
                name.StartsWith("Bush_", System.StringComparison.OrdinalIgnoreCase))
            {
                // Tag it for minimal interaction
                if (obj.tag != "Untagged") obj.tag = "Nature";
                
                // Example: Add a sway component if it doesn't exist (simulated here)
                // if (!obj.GetComponent<FoliageSway>()) obj.AddComponent<FoliageSway>();
            }

            // 3. INTERACTABLE LOGIC
            if (name.StartsWith("Int_", System.StringComparison.OrdinalIgnoreCase))
            {
                if (obj.layer == LayerMask.NameToLayer("Default"))
                {
                    // Move to Interactable layer if it exists, otherwise log warning
                    int interactableLayer = LayerMask.NameToLayer("Interactable");
                    if (interactableLayer != -1) obj.layer = interactableLayer;
                }
            }

            // 4. FLOOR/GROUND
            if (name.StartsWith("Floor_", System.StringComparison.OrdinalIgnoreCase))
            {
               // Static navigation logic could go here
               // GameObjectUtility.SetStaticEditorFlags(obj, StaticEditorFlags.NavigationStatic);
            }
        }
    }
}
