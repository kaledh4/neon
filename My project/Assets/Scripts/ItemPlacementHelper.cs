using UnityEngine;
using System.Collections.Generic;

namespace NeonSplash.V0_1
{
    public static class ItemPlacementHelper
    {
        // Cache used by the retry logic to avoid allocating new arrays
        private static readonly Collider[] _overlapResults = new Collider[1];

        /// <summary>
        /// Attempts to find a valid position for an item within the chunk.
        /// Checks against existing colliders in WORLD SPACE to prevent overlap.
        /// </summary>
        public static bool TryGetValidPlacement(Vector3 worldOrigin, float chunkSize, float itemRadius, PlacementType placementType, 
            LayerMask obstacleMask, out Vector3 localPosition)
        {
            localPosition = Vector3.zero;
            float halfSize = chunkSize * 0.5f;
            float margin = 10f; // Edge buffer

            for (int attempt = 0; attempt < 15; attempt++)
            {
                float x = Random.Range(-halfSize + margin, halfSize - margin);
                float z = Random.Range(-halfSize + margin, halfSize - margin);
                
                bool isNearEdge = (Mathf.Abs(x) > halfSize - (margin + 5f) || Mathf.Abs(z) > halfSize - (margin + 5f));

                // 1. Filter by Preferred Zone
                bool zoneValid = false;
                switch (placementType)
                {
                    case PlacementType.NearWall: zoneValid = isNearEdge; break;
                    case PlacementType.OpenSpace: zoneValid = !isNearEdge; break;
                    case PlacementType.Center: zoneValid = (Mathf.Abs(x) < 15f && Mathf.Abs(z) < 15f); break;
                    case PlacementType.Any: zoneValid = true; break;
                }

                if (!zoneValid) continue;

                // 2. Physical Clearance Check (World Space)
                Vector3 proposedLocal = new Vector3(x, 0, z);
                
                // CRITICAL FIX: Lift the check sphere HIGHER.
                // The floor is at Y=0. If check is at Y=0.5 with radius 1.5, it hits the floor.
                // Center should be at Radius + SmallEpsilon to sit EXACTLY on top of floor without intersecting it.
                float checkHeight = itemRadius + 0.1f; 
                Vector3 checkWorldPos = worldOrigin + proposedLocal + Vector3.up * checkHeight;
                
                // Optimized Non-Allocating Check
                int hitCount = Physics.OverlapSphereNonAlloc(checkWorldPos, itemRadius, _overlapResults, obstacleMask);
                
                if (hitCount == 0)
                {
                    localPosition = proposedLocal;
                    return true;
                }
            }

            return false;
        }
    }
}
