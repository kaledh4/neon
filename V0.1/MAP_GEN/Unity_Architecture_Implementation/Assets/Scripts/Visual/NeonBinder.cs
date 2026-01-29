using UnityEngine;

namespace Visual
{
    /// <summary>
    /// Attached to any object (MeshRenderer or Light) that should participate in the global Neon system.
    /// "Dumb" receiver of "Smart" data from the Registry.
    /// </summary>
    [ExecuteAlways]
    public class NeonBinder : MonoBehaviour
    {
        public string channelID = "Default";
        
        private MaterialPropertyBlock _propBlock;
        private Renderer _renderer;
        private Light _light;
        
        // Shader property ID
        private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
        private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

        private void OnEnable()
        {
            _renderer = GetComponent<Renderer>();
            _light = GetComponent<Light>(); // Optional: Can also control real-time lights
            _propBlock = new MaterialPropertyBlock();
        }

        private void Update()
        {
            // In a real optimized system, this would be event-driven, not per-frame polling.
            // For simplicity/robustness in this demo, we poll the Registry.
            
            if (NeonRegistry.Instance == null) return;

            var channel = NeonRegistry.Instance.GetChannel(channelID);
            if (channel == null) return;

            Color finalColor = channel.GetCurrentColor();

            if (_renderer != null)
            {
                _renderer.GetPropertyBlock(_propBlock);
                
                // Assuming URP Lit or Unlit shader usage
                _propBlock.SetColor(EmissionColorID, finalColor);
                // Also tint base color slightly?
                // _propBlock.SetColor(BaseColorID, finalColor);
                
                _renderer.SetPropertyBlock(_propBlock);
            }

            if (_light != null)
            {
                _light.color = finalColor;
                _light.intensity = channel.intensity; 
            }
        }
    }
}
