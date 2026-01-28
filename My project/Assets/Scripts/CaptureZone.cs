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

            // Add Collider if missing
            if (GetComponent<Collider>() == null)
            {
                BoxCollider box = gameObject.AddComponent<BoxCollider>();
                box.isTrigger = true;
                box.size = new Vector3(10, 5, 10); // Check chunk size
                box.center = new Vector3(0, 2.5f, 0);
            }
        }

        void CreateVisualFallback()
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            DestroyImmediate(ring.GetComponent<Collider>());
            ring.name = "CaptureOverlay_Generated";
            ring.transform.SetParent(transform);
            ring.transform.localPosition = new Vector3(0, 0.1f, 0); // Just above floor
            ring.transform.localScale = new Vector3(12, 0.1f, 12); // Flattish disk
            overlayRenderer = ring.GetComponent<Renderer>();
            overlayRenderer.material = new Material(Shader.Find("Standard")); // Or URP/Lit
            UpdateVisuals(Team.None);
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
