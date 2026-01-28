using UnityEngine;

namespace NeonSplash.V0_1
{
    public class NeonPulse : MonoBehaviour
    {
        public float baseIntensity = 1.0f;
        public float frequency = 2f;
        public float amplitude = 0.2f;
        private Light targetLight;
        private Material targetMaterial;
        private Color baseColor;

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
            
            // Random offset so they don't all pulse in sync
            frequency *= Random.Range(0.8f, 1.2f);
        }

        void Update()
        {
            float pulse = 1.0f + Mathf.Sin(Time.time * frequency) * amplitude;
            
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
                // "Breathing" when still
                activeBobSpeed = 1.2f;
                activeBobAmount = 0.005f;
            }

            float bob = Mathf.Sin(Time.time * activeBobSpeed) * activeBobAmount;
            float sway = Mathf.Cos(Time.time * swaySpeed) * swayAmount;
            
            transform.localPosition = baseLocalPos + new Vector3(sway, bob, 0);

            // Dynamic FOV
            if (cam != null)
            {
                float targetFOV = (velocity > 5f) ? 75f : 65f;
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 4f);
            }
        }
    }
}
