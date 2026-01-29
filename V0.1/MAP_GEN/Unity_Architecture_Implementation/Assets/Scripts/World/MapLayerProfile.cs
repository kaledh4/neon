using UnityEngine;
using UnityEngine.Rendering;

namespace World
{
    /// <summary>
    /// Defines a "Reality Layer" (e.g., Cyber, Spirit, Past).
    /// </summary>
    [CreateAssetMenu(fileName = "New Map Profile", menuName = "Architecture/Map Layer Profile")]
    public class MapLayerProfile : ScriptableObject
    {
        public string layerName;
        
        [Header("Transform Rules")]
        public bool invertX = false;
        public bool invertY = false;
        public Vector3 offset = Vector3.zero;

        [Header("Visual Rules")]
        public Material overrideMaterial;
        public VolumeProfile postProcessOverrides; // URP Volume Profile

        [Tooltip("If true, this layer is ghosted (partially transparent) when not active.")]
        public bool showGhostWhenInactive = false;
    }
}
