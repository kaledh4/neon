using UnityEngine;

namespace Visual
{
    /// <summary>
    /// Defines a flow of visual data (Color, Pulse, Intensity).
    /// Can be a ScriptableObject asset OR just a runtime class. 
    /// Making it a ScriptableObject allows easier Inspector editing.
    /// </summary>
    [CreateAssetMenu(fileName = "New Neon Channel", menuName = "Architecture/Neon Channel")]
    public class NeonChannelController : ScriptableObject
    {
        public string channelName;
        [ColorUsage(true, true)] public Color baseColor = Color.white;
        public float intensity = 1.0f;
        
        [Header("Animation")]
        public bool isPulsing = false;
        public float pulseSpeed = 1.0f;
        public float pulseMin = 0.5f;
        public float pulseMax = 1.5f;

        /// <summary>
        /// Calculates the current color output based on time.
        /// </summary>
        public Color GetCurrentColor()
        {
            float currentIntensity = intensity;
            
            if (isPulsing)
            {
                // Simple sine wave pulse
                float t = Mathf.PingPong(Time.time * pulseSpeed, 1.0f);
                float pulseMod = Mathf.Lerp(pulseMin, pulseMax, t);
                currentIntensity *= pulseMod;
            }

            // Return color * calculated intensity (HDR)
            // Note: Color.a isn't usually used for emission intensity in shaders, but linear multiplication is typical.
            return baseColor * currentIntensity;
        }
    }
}
