using UnityEngine;
using System.Collections.Generic;

namespace NeonSplash.V0_1
{
    public class CaptureZone : MonoBehaviour
    {
        public Team currentWinningTeam = Team.None;
        
        [Header("Zone Settings")]
        public float captureRadius = 5f; // If using Distance check instead of collider
        
        // Tracking players in zone
        private int redCount = 0;
        private int blueCount = 0;

        // Visuals
        private Renderer overlayRenderer;
        
        void Start()
        {
            // Try to find a specific overlay child, or just create a simple one if not found
            Transform overlayChild = transform.Find("Overlay");
            if (overlayChild != null)
            {
                overlayRenderer = overlayChild.GetComponent<Renderer>();
            }
            else
            {
                // Fallback: Create a visual ring
                CreateVisualFallback();
            }

            // Add Collider - Encompass the Pool width
            if (GetComponent<Collider>() == null)
            {
                BoxCollider box = gameObject.AddComponent<BoxCollider>();
                box.isTrigger = true;
                box.size = new Vector3(80, 20, 80); // Large enough for the pool area (90x90)
                box.center = new Vector3(0, 10f, 0); // Higher vertical reach
            }
        }

        void CreateVisualFallback()
        {
            // Create a tall vertical holographic cylinder
            GameObject zone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            DestroyImmediate(zone.GetComponent<Collider>());
            zone.name = "CaptureHologram";
            zone.transform.SetParent(transform);
            
            // Position it so the bottom is on the pool floor and it extends upwards
            zone.transform.localPosition = new Vector3(0, 5f, 0); 
            zone.transform.localScale = new Vector3(15f, 5f, 15f); // 10m tall holographic pillar
            
            overlayRenderer = zone.GetComponent<Renderer>();
            
            // Use a material that supports transparency
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (mat == null) mat = new Material(Shader.Find("Standard"));
            
            // Set rendering mode to Transparent manually if possible or just use low alpha
            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            
            overlayRenderer.material = mat;
            UpdateVisuals(Team.None);
        }

        void Update()
        {
            if (overlayRenderer == null) return;

            // Holographic Pulse
            float baseAlpha = 0.3f;
            if (currentWinningTeam == Team.None && (redCount > 0 || blueCount > 0)) baseAlpha = 0.5f; // Intense when contested
            
            float matchProgress = 0f;
            if (GameManager.Instance != null)
                matchProgress = 1f - (GameManager.Instance.currentMatchTime / GameManager.Instance.matchDuration);

            float pulse = Mathf.Sin(Time.time * (2f + matchProgress * 4f)) * 0.15f;
            Color c = overlayRenderer.material.color;
            c.a = baseAlpha + pulse;
            overlayRenderer.material.color = c;
            
            // Intensify emission over time
            overlayRenderer.material.SetColor("_EmissionColor", c * (2f + matchProgress * 3f));
        }

        void OnTriggerEnter(Collider other)
        {
            UpdateCounts(other, 1);
        }

        void OnTriggerExit(Collider other)
        {
            UpdateCounts(other, -1);
        }

        private void UpdateCounts(Collider other, int change)
        {
            PlayerTeam pt = other.GetComponent<PlayerTeam>();
            if (pt != null)
            {
                if (pt.team == Team.Red) redCount += change;
                if (pt.team == Team.Blue) blueCount += change;
                
                CheckControl();
            }
        }

        private void CheckControl()
        {
            Team oldTeam = currentWinningTeam;
            ChunkStateController state = GetComponentInParent<ChunkStateController>();

            if (redCount > 0 && blueCount == 0) 
            {
                currentWinningTeam = Team.Red;
                if (state != null) state.state = ChunkState.Controlled;
            }
            else if (blueCount > 0 && redCount == 0) 
            {
                currentWinningTeam = Team.Blue;
                if (state != null) state.state = ChunkState.Controlled;
            }
            else if (redCount > 0 && blueCount > 0) 
            {
                currentWinningTeam = Team.None; // Contested
                if (state != null) state.state = ChunkState.Contested;
            }
            else 
            {
                currentWinningTeam = Team.None; // Empty
                if (state != null) state.state = ChunkState.Neutral;
            }

            // Notify Manager
            if (GameManager.Instance != null && (currentWinningTeam != oldTeam || (redCount > 0 && blueCount > 0)))
            {
                GameManager.Instance.SetZoneOwner(currentWinningTeam);
                UpdateVisuals(currentWinningTeam);
            }
        }

        private void UpdateVisuals(Team team)
        {
            if (overlayRenderer == null) return;

            Color c = Color.white;
            switch (team)
            {
                case Team.Red: c = Color.red; break;
                case Team.Blue: c = Color.cyan; break;
                case Team.None: c = Color.grey; break;
            }
            
            // Set Alpha
            c.a = 0.5f;
            
            // Apply to material
            overlayRenderer.material.color = c;
            
            // If using standard shader, maybe set Emission too for "Glow"
            overlayRenderer.material.EnableKeyword("_EMISSION");
            overlayRenderer.material.SetColor("_EmissionColor", c * 0.5f);
        }
    }
}
