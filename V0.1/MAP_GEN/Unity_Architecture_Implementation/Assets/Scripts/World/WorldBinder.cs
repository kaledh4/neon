using UnityEngine;
using System.Collections.Generic;

namespace World
{
    /// <summary>
    /// Manages the "Parallel Maps". 
    /// Can spawn copies of the world or toggle visibility masks.
    /// </summary>
    public class WorldBinder : MonoBehaviour
    {
        public static WorldBinder Instance;
        
        [Header("Configuration")]
        public List<MapLayerProfile> availableLayers;
        public Transform worldRoot; // The parent of the level geometry

        private Dictionary<string, GameObject> instancedLayers = new Dictionary<string, GameObject>();
        private string activeLayerID;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (worldRoot == null)
            {
                Debug.LogWarning("[WorldBinder] No World Root assigned!");
                return;
            }

            // For this implementation, we will assume we might simply spawn the layers 
            // or we might just apply effects to them.
            // Let's implement the "Instantiate Parallel Realities" approach.
            
            InitializeLayers();
        }

        private void InitializeLayers()
        {
            foreach (var profile in availableLayers)
            {
                if (profile == null) continue;

                // Create a container for this layer
                GameObject layerRoot = new GameObject($"Layer_Root_{profile.layerName}");
                layerRoot.transform.SetParent(this.transform);
                
                // In a real scenario, we might clone worldRoot here, or assume the level designer placed specific objects for specific layers.
                // For the "Parallel" constraint, let's assume we clone the geometry but apply the Profile rules.
                
                // Simulating binding:
                instancedLayers.Add(profile.layerName, layerRoot);
                ApplyProfileToRoot(layerRoot, profile);
            }
        }

        private void ApplyProfileToRoot(GameObject root, MapLayerProfile profile)
        {
            // Apply offsets
            root.transform.localPosition = profile.offset;
            
            // Invert logic (mockup)
            if (profile.invertX)
            {
                Vector3 s = root.transform.localScale;
                s.x *= -1;
                root.transform.localScale = s;
            }
        }

        public void SwitchLayer(string layerID)
        {
            activeLayerID = layerID;
            // Logic to enable/disable specific layer roots or Volume modifiers would go here.
            Debug.Log($"[WorldBinder] Switched to Reality: {layerID}");
        }
    }
}
