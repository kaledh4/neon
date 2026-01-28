using UnityEngine;
using System.Collections.Generic;

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
        public Color factionBiasColor = Color.white;

        void Start()
        {
            // Auto-detect faction bias based on position or team
            if (transform.position.x < 450f) factionBiasColor = new Color(0, 0.8f, 1f); // Cyan Territory
            else if (transform.position.x > 550f) factionBiasColor = new Color(1f, 0, 0.8f); // Magenta Territory
            else factionBiasColor = new Color(0.5f, 0.5f, 0.5f); // Neutral Center
        }

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
                    targetPulse = 3.0f; // Faster escalation
                    targetIntensityScale = 1.5f;
                    break;
                case ChunkState.Controlled:
                    targetPulse = 4.5f;
                    targetIntensityScale = 2.0f;
                    break;
            }

            float matchProgress = 0f;
            if (GameManager.Instance != null)
                matchProgress = 1f - (GameManager.Instance.currentMatchTime / GameManager.Instance.matchDuration);
            
            targetPulse *= (1f + matchProgress * 0.5f);

            // Performance Scaling
            if (PerformanceMonitor.currentFPS < 50)
            {
                targetIntensityScale *= 0.8f;
                targetPulse *= 0.7f;
            }

            currentPulseMultiplier = Mathf.Lerp(currentPulseMultiplier, targetPulse, Time.deltaTime * 2f);

            foreach (var l in lights)
            {
                if (l != null)
                {
                    l.intensity = Mathf.Lerp(l.intensity, 1.5f * targetIntensityScale, Time.deltaTime * 3f);
                    // Bias light color toward faction
                    l.color = Color.Lerp(l.color, factionBiasColor, 0.3f);
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
        public enum PulseType { Building, Foliage, Objective }
        public PulseType role = PulseType.Building;
        
        public float baseIntensity = 1.0f;
        public float frequency = 2f;
        public float amplitude = 0.2f;
        public bool createGlowShell = true;

        private Light targetLight;
        private Renderer targetRenderer;
        private Material targetMaterial;
        private Color baseColor;
        private ChunkStateController chunkState;
        private GameObject glowShell;

        void Start()
        {
            targetLight = GetComponent<Light>();
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                targetMaterial = targetRenderer.material;
                if (targetMaterial.HasProperty("_EmissionColor"))
                    baseColor = targetMaterial.GetColor("_EmissionColor");
                
                if (createGlowShell) SpawnGlowShell();
            }
            
            chunkState = GetComponentInParent<ChunkStateController>();
            
            // Layered frequencies based on role
            switch (role)
            {
                case PulseType.Building: frequency = 0.8f; amplitude = 0.1f; break;
                case PulseType.Foliage: frequency = 2.5f; amplitude = 0.15f; break;
                case PulseType.Objective: frequency = 5.0f; amplitude = 0.4f; break;
            }
            frequency *= Random.Range(0.9f, 1.1f);
        }

        void SpawnGlowShell()
        {
            glowShell = GameObject.CreatePrimitive(PrimitiveType.Sphere); // Simple fallback or copy mesh
            // In a real scenario, we'd copy the mesh filter. 
            // For this procedural app, we'll just scale up the current object if it's simple.
            
            glowShell = Instantiate(gameObject);
            Destroy(glowShell.GetComponent<NeonPulse>());
            Destroy(glowShell.GetComponent<Collider>());
            if (glowShell.GetComponent<Light>()) Destroy(glowShell.GetComponent<Light>());

            glowShell.transform.SetParent(transform);
            glowShell.transform.localPosition = Vector3.up * 0.05f;
            glowShell.transform.localScale = Vector3.one * 1.15f;
            glowShell.transform.localRotation = Quaternion.identity;

            var r = glowShell.GetComponent<Renderer>();
            r.material = new Material(targetMaterial);
            Color c = baseColor;
            c.a = 0.2f;
            r.material.color = c;
            // Set to transparent
            r.material.SetFloat("_Mode", 3);
            r.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            r.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            r.material.SetInt("_ZWrite", 0);
            r.material.EnableKeyword("_ALPHABLEND_ON");
            r.material.renderQueue = 3000;
        }

        void Update()
        {
            float speedMult = chunkState != null ? chunkState.currentPulseMultiplier : 1f;
            float pulse = 1.0f + Mathf.Sin(Time.time * frequency * speedMult) * amplitude;
            
            if (targetLight != null) targetLight.intensity = baseIntensity * pulse;

            if (targetMaterial != null)
            {
                targetMaterial.SetColor("_EmissionColor", baseColor * pulse);
                // Apply Faction Bias from controller
                if (chunkState != null)
                {
                    targetMaterial.SetColor("_BaseColor", Color.Lerp(targetMaterial.color, chunkState.factionBiasColor, 0.2f));
                }
            }

            if (glowShell != null)
            {
                var r = glowShell.GetComponent<Renderer>();
                Color c = baseColor * (pulse * 0.5f);
                c.a = 0.15f * pulse;
                r.material.SetColor("_EmissionColor", c);
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
            
            // Epic Scale Drift (System 8)
            float slowDrift = Mathf.Sin(Time.time * 0.5f) * 0.05f;
            
            transform.localPosition = baseLocalPos + new Vector3(sway, bob + slowDrift, 0);

            if (cam != null)
            {
                float targetFOV = (velocity > 5f) ? 78f : 68f; // Wider FOV for epic scale
                targetFOV += matchProgress * 10f; // Scale FOV with urgency
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 3f);
            }
        }
    }
}
