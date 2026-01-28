using UnityEngine;

namespace Visual
{
    /// <summary>
    /// Adds a random "dirty" flicker effect to a neon material.
    /// Works with individual renderers for organic, non-synchronized effects.
    /// </summary>
    public class NeonFlicker : MonoBehaviour
    {
        [Header("Settings")]
        public float flickerChance = 0.05f; // Chance per frame to flicker
        public float flickerDuration = 0.1f; // How long it stays dim
        public float dimIntensity = 0.2f;    // 0 to 1 multiplier
        public float smoothing = 0.1f;       // Transition speed

        private Renderer _renderer;
        private MaterialPropertyBlock _propBlock;
        private Color _originalColor;
        private float _currentModifier = 1.0f;
        private float _targetModifier = 1.0f;
        private float _timer = 0f;

        private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
            _propBlock = new MaterialPropertyBlock();
            
            // Randomize settings slightly for organic feel
            flickerChance *= Random.Range(0.8f, 1.2f);
            flickerDuration *= Random.Range(0.5f, 1.5f);
            smoothing = Random.Range(0.05f, 0.2f);

            if (_renderer != null)
            {
                // Capture the initial emission color
                _originalColor = _renderer.sharedMaterial.GetColor(EmissionColorID);
            }
        }

        private void Update()
        {
            if (_renderer == null) return;

            // Logic to trigger flicker
            if (_timer > 0)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0) _targetModifier = 1.0f;
            }
            else
            {
                if (Random.value < flickerChance)
                {
                    _targetModifier = dimIntensity;
                    _timer = flickerDuration;
                }
            }

            // Smooth transition
            _currentModifier = Mathf.Lerp(_currentModifier, _targetModifier, smoothing);

            // Apply
            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(EmissionColorID, _originalColor * _currentModifier);
            _renderer.SetPropertyBlock(_propBlock);
        }
    }
}
