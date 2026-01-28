using UnityEngine;

namespace NeonSplash.V0_1
{
    public enum ChunkState { Neutral, Contested, Controlled }

    public class ChunkStateController : MonoBehaviour
    {
        public ChunkState state = ChunkState.Neutral;
        public List<Light> lights = new List<Light>();
        public List<Renderer> renderers = new List<Renderer>();
        public float basePulseSpeed = 2f;
        public float currentPulseMultiplier = 1f;
        public Team owningTeam = Team.None;

        void Update()
        {
            float targetPulse = 1f;
            float targetIntensityScale = 1f;

            switch (state)
            {
                case ChunkState.Neutral:
                    targetPulse = 1f;
                    targetIntensityScale = 1.0f;
                    break;
                case ChunkState.Contested:
                    targetPulse = 2.5f;
                    targetIntensityScale = 1.5f;
                    break;
                case ChunkState.Controlled:
                    targetPulse = 4.0f;
                    targetIntensityScale = 1.8f;
                    break;
            }

            // Sync with match temporal evolution
            float matchProgress = 0f;
            if (GameManager.Instance != null)
                matchProgress = 1f - (GameManager.Instance.currentMatchTime / GameManager.Instance.matchDuration);
            
            targetPulse *= (1f + matchProgress * 0.5f);
            targetIntensityScale *= (1f + matchProgress * 0.2f);

            // Performance Scaling
            if (PerformanceMonitor.currentFPS < 60)
            {
                targetIntensityScale *= 0.8f;
                targetPulse *= 0.7f;
            }

            currentPulseMultiplier = Mathf.Lerp(currentPulseMultiplier, targetPulse, Time.deltaTime * 2f);

            foreach (var l in lights)
            {
                if (l != null)
                {
                    l.intensity = Mathf.Lerp(l.intensity, 1.2f * targetIntensityScale, Time.deltaTime * 3f);
                }
            }
        }
    }

    public class PerformanceMonitor : MonoBehaviour
    {
        public static float currentFPS;
        private float deltaTime = 0.0f;

        void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            currentFPS = 1.0f / deltaTime;
        }
    }

    public class NeonPulse : MonoBehaviour
    {
        public float baseIntensity = 1.0f;
        public float frequency = 2f;
        public float amplitude = 0.2f;
        private Light targetLight;
        private Material targetMaterial;
        private Color baseColor;
        private ChunkStateController chunkState;

        void Start()
        {
            targetLight = GetComponent<Light>();
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                targetMaterial = renderer.material;
                if (targetMaterial.HasProperty("_EmissionColor"))
                    baseColor = targetMaterial.GetColor("_EmissionColor");
            }
            
            chunkState = GetComponentInParent<ChunkStateController>();
            frequency *= Random.Range(0.8f, 1.2f);
        }

        void Update()
        {
            float speedMult = chunkState != null ? chunkState.currentPulseMultiplier : 1f;
            float pulse = 1.0f + Mathf.Sin(Time.time * frequency * speedMult) * amplitude;
            
            if (targetLight != null)
            {
                targetLight.intensity = baseIntensity * pulse;
            }

            if (targetMaterial != null)
            {
                targetMaterial.SetColor("_EmissionColor", baseColor * pulse);
            }
        }
    }

    public class CameraPerception : MonoBehaviour
    {
        public float bobSpeed = 6f;
        public float bobAmount = 0.02f;
        public float swaySpeed = 1.5f;
        public float swayAmount = 0.015f;
        
        private Vector3 baseLocalPos;
        private Camera cam;
        private Rigidbody playerRb;

        void Start()
        {
            cam = GetComponent<Camera>();
            baseLocalPos = transform.localPosition;
            playerRb = GetComponentInParent<Rigidbody>();
        }

        void LateUpdate()
        {
            float velocity = playerRb != null ? playerRb.linearVelocity.magnitude : 0;
            float activeBobSpeed = bobSpeed;
            float activeBobAmount = bobAmount;

            if (velocity < 0.1f)
            {
                activeBobSpeed = 1.2f;
                activeBobAmount = 0.005f;
            }

            // Match Intensity Increase
            float matchProgress = 0f;
            if (GameManager.Instance != null)
                matchProgress = 1f - (GameManager.Instance.currentMatchTime / GameManager.Instance.matchDuration);
            
            activeBobAmount *= (1f + matchProgress * 0.3f);

            float bob = Mathf.Sin(Time.time * activeBobSpeed) * activeBobAmount;
            float sway = Mathf.Cos(Time.time * swaySpeed) * swayAmount;
            
            transform.localPosition = baseLocalPos + new Vector3(sway, bob, 0);

            if (cam != null)
            {
                float targetFOV = (velocity > 5f) ? 75f : 65f;
                // FOV widening as match gets intense
                targetFOV += matchProgress * 5f; 
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 4f);
            }
        }
    }
}
}
