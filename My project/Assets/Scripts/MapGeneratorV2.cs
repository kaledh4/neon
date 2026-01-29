using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NeonSplash;

namespace NeonSplash.V0_1
{
    public enum FloorType { Grass, Wood, Tile, Water }

    public class MapGeneratorV2 : MonoBehaviour
    {
        [Header("Settings")]
        public int seed = 12345;
        public float chunkSize = 110f; // 100 size + 10 gap
        
        [Header("Visuals")]
        public Material skyboxMaterial;

        [Header("Parallel Maps")]
        public bool spawnParallelMaps = true;
        public float parallelDistance = 250f; // Further out for the larger world

        private List<GameObject> activeObjects = new List<GameObject>();
        private ColorPalette palette; 
        
        private struct MapStep
        {
            public string typeId;
            public Vector3 position;
            public bool isMirrored;
        }
        
        private List<MapStep> currentMapBlueprint = new List<MapStep>();

        void Start()
        {
            GenerateWorld();
        }

        [System.Serializable]
        public struct ScatterRule
        {
            public string name;
            public GameObject prefab;
            [Tooltip("Total number of these objects to spawn across the entire map")]
            public int totalCount;
            [Tooltip("Don't spawn on these chunk types (e.g. 'BaseStart')")]
            public List<string> excludedTypes;
            [Range(0.1f, 5f)] public float scaleMultiplier;
            public float heightOffset;
        }

        [Header("Mission Control - Global Scatter")]
        public List<ScatterRule> globalScatterRules;

        [ContextMenu("Generate World")]
        public void GenerateWorld()
        {
            Cleanup();

            Random.InitState(seed);
            palette = new ColorPalette(seed); 

            SetupSkybox();
            CalculateMapLogic();

            // Build Maps
            GameObject mainMap = BuildMapInstance(Vector3.zero, "MainMap", true); // Capture reference
            if (spawnParallelMaps)
            {
                BuildMapInstance(new Vector3(parallelDistance, 0, 0), "Parallel_Right", false);
                BuildMapInstance(new Vector3(-parallelDistance, 0, 0), "Parallel_Left", false);
            }

            // GLOBAL SCATTER PASS
            if (mainMap != null) 
            {
                RunGlobalScatter(mainMap);
                SpawnProceduralCrates(mainMap); // Auto-spawn 50 crates request
            }

            SpawnPlayer();
        }

        private void SpawnProceduralCrates(GameObject mapRoot)
        {
            // The user requested: "box 50 times... not over main stuff... random sizes"
            // We implement this as a hardcoded default pass.
            
            GameObject crateGroup = new GameObject("Procedural_Crates_Auto");
            crateGroup.transform.SetParent(mapRoot.transform);
            activeObjects.Add(crateGroup);

            int totalCrates = 50;
            int spawned = 0;
            int attempts = 0;
            
            // Material key
            Material crateMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (crateMat.shader.name == "Hidden/InternalErrorShader") crateMat = new Material(Shader.Find("Standard"));
            crateMat.color = new Color(1f, 0.5f, 0f); // Orange Neon
            crateMat.EnableKeyword("_EMISSION");
            crateMat.SetColor("_EmissionColor", crateMat.color * 2f);

            while (spawned < totalCrates && attempts < 500)
            {
                attempts++;
                if (currentMapBlueprint.Count == 0) break;
                MapStep chunk = currentMapBlueprint[Random.Range(0, currentMapBlueprint.Count)];

                // Exclude Main 'Features' to avoid clutter (Base, Pool)
                if (chunk.typeId.Contains("Base") || chunk.typeId.Contains("Pool")) continue;

                float halfSize = chunkSize * 0.4f;
                Vector3 offset = new Vector3(Random.Range(-halfSize, halfSize), 0, Random.Range(-halfSize, halfSize));
                Vector3 worldPos = chunk.position + offset;

                // Collision Check
                if (Physics.CheckSphere(worldPos + Vector3.up, 1f, spawnObstacleMask)) continue;

                // Spawn Crate
                GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crate.name = "Auto_Crate";
                crate.transform.SetParent(crateGroup.transform);
                
                // Random Size
                float s = Random.Range(0.8f, 2.5f);
                crate.transform.localScale = new Vector3(s, s, s);
                
                // Position (Ground it)
                crate.transform.position = worldPos + Vector3.up * (s * 0.5f);
                crate.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                
                crate.GetComponent<Renderer>().material = crateMat;
                
                spawned++;
            }
        }

        private void RunGlobalScatter(GameObject mapRoot)
        {
            if (globalScatterRules == null) return;

            foreach (var rule in globalScatterRules)
            {
                if (rule.prefab == null || rule.totalCount <= 0) continue;

                // Create container
                GameObject scatterGroup = new GameObject($"GlobalScatter_{rule.name}");
                scatterGroup.transform.SetParent(mapRoot.transform);
                activeObjects.Add(scatterGroup); // Mark for cleanup

                int spawned = 0;
                int attempts = 0;
                int maxAttempts = rule.totalCount * 5;

                while (spawned < rule.totalCount && attempts < maxAttempts)
                {
                    attempts++;
                    // Pick random chunk from blueprint
                    if (currentMapBlueprint.Count == 0) break;
                    MapStep randomChunk = currentMapBlueprint[Random.Range(0, currentMapBlueprint.Count)];

                    // Check exclusions
                    if (rule.excludedTypes != null && rule.excludedTypes.Contains(randomChunk.typeId)) continue;

                    // Valid Chunk -> Pick random position
                    // Limit spawn area to avoid walls (80% of chunk size)
                    float halfSize = chunkSize * 0.4f; 
                    Vector3 randomOffset = new Vector3(Random.Range(-halfSize, halfSize), 0, Random.Range(-halfSize, halfSize));
                    Vector3 targetWorldPos = randomChunk.position + randomOffset;
                    
                    // Simple collision check to avoid embedding inside trees/buildings
                    // Check radius 2f
                    if (Physics.CheckSphere(targetWorldPos + Vector3.up * 2f, 2f, spawnObstacleMask)) continue;

                    // Spawn
                    GameObject obj = Instantiate(rule.prefab, scatterGroup.transform);
                    obj.transform.position = targetWorldPos + Vector3.up * rule.heightOffset;
                    obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                    obj.transform.localScale = Vector3.one * rule.scaleMultiplier;
                    
                    spawned++;
                }
            }
        }
        




        private void SetupSkybox()
        {
            if (palette == null) palette = new ColorPalette(seed);
            if (palette == null) return;

            if (skyboxMaterial != null) RenderSettings.skybox = skyboxMaterial;
            RenderSettings.fog = true;
            RenderSettings.fogColor = palette.Fog; 
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.005f;

            // Match Progress Ambient Shift (Temporal Variation)
            // Note: GameManager might be null in Editor
            float matchProgress = 0f;
            var gm = GameObject.FindFirstObjectByType<GameManager>();
            if (gm != null)
                matchProgress = 1f - (gm.currentMatchTime / gm.matchDuration);

            Color ambientStart = new Color(0.1f, 0.15f, 0.2f); // Calm Blue
            Color ambientEnd = new Color(0.25f, 0.1f, 0.15f); // Lethal Magenta
            RenderSettings.ambientLight = Color.Lerp(ambientStart, ambientEnd, matchProgress);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

            // Sky Motion (System 5)
            if (RenderSettings.skybox != null)
            {
                float rotation = Time.time * 0.5f; // Slow constant drift
                RenderSettings.skybox.SetFloat("_Rotation", rotation);
            }

            if (Camera.main) Camera.main.backgroundColor = palette.Background;

            // Ensure Performance Tracking (Only play mode)
            if (Application.isPlaying && gameObject.GetComponent<PerformanceMonitor>() == null)
                gameObject.AddComponent<PerformanceMonitor>();
        }

        void Update()
        {
            // Update match visuals every frame
            SetupSkybox();
        }

        private void CalculateMapLogic()
        {
            currentMapBlueprint.Clear();
            string[] randomTypes = { "Trees", "Garden", "TikiBar", "HotTub" };
            List<string> firstHalfTypes = new List<string>();
            List<float> firstHalfOffsets = new List<float>();

            // 0. Base
            AddBlueprintStep("BaseStart", Vector3.zero, false);

            // 1-3. Random
            for (int i = 1; i <= 3; i++)
            {
                string type = randomTypes[Random.Range(0, randomTypes.Length)];
                // Restore randomness but keep it snapped to quarters (25%) so easier to fill
                float zOffset = (Random.Range(0, 3) - 1) * (chunkSize * 0.25f); 
                firstHalfTypes.Add(type);
                firstHalfOffsets.Add(zOffset);
                AddBlueprintStep(type, new Vector3(i * chunkSize, 0, zOffset), false);
            }

            // 4-5. Pool
            AddBlueprintStep("PoolStart", new Vector3(4 * chunkSize, 0, 0), true);
            AddBlueprintStep("PoolEnd", new Vector3(5 * chunkSize, 0, 0), false);

            // 6-8. Mirrored
            for (int i = 0; i < 3; i++)
            {
                int srcIdx = 2 - i;
                float xPos = (6 + i) * chunkSize;
                AddBlueprintStep(firstHalfTypes[srcIdx], new Vector3(xPos, 0, -firstHalfOffsets[srcIdx]), true);
            }

            // 9. Base
            AddBlueprintStep("BaseEnd", new Vector3(9 * chunkSize, 0, 0), false);
        }

        private void AddBlueprintStep(string id, Vector3 pos, bool mirror)
        {
            currentMapBlueprint.Add(new MapStep { typeId = id, position = pos, isMirrored = mirror });
        }

        private GameObject BuildMapInstance(Vector3 worldOffset, string containerName, bool isGameplay)
        {
            GameObject container = new GameObject(containerName);
            activeObjects.Add(container);

            foreach (var step in currentMapBlueprint)
            {
                SpawnChunk(step, worldOffset, container.transform);
            }

            if (!isGameplay) DisableColliders(container);
            return container;
        }

        private void DisableColliders(GameObject root)
        {
            foreach (var col in root.GetComponentsInChildren<Collider>()) DestroyImmediate(col);
        }

        [Header("Procedural Spawning")]
        public ChunkDataStore chunkDataStore;
        [Tooltip("Drag your 'Tree test.fbx' here for instant trees without DataStore setup.")]
        public GameObject fallbackTreeModel; 
        
        [Tooltip("Layers to avoid when spawning props (e.g., Walls, Other Props)")]
        public LayerMask spawnObstacleMask;
        
        [Header("Model Settings")]
        [Tooltip("Scale multiplier for fallback tree models")]
        public float fallbackScale = 1.0f;
        
        [Header("Floor Height Adjustments")]
        [Range(-5f, 5f)] public float treeFloorOffset = 0f;
        [Range(-5f, 5f)] public float tikiFloorOffset = 0f;
        [Range(-5f, 5f)] public float gardenFloorOffset = 0f;
        [Range(-5f, 5f)] public float poolFloorOffset = 0f;
        [Range(-5f, 5f)] public float hotTubFloorOffset = 0f;

        [Header("Fallback Models")]
        public GameObject fallbackTikiModel;
        public GameObject fallbackHotTubModel;
        public GameObject fallbackGardenModel;
        public GameObject fallbackPoolModel;

        private void SpawnChunk(MapStep step, Vector3 offset, Transform parent)
        {
            Vector3 finalPos = step.position + offset;
            
            // Vertical Depth Variation (System 4)
            float vBias = Mathf.PerlinNoise(finalPos.x * 0.05f, finalPos.z * 0.05f);
            finalPos += Vector3.up * vBias * 1.5f;

            GameObject chunkObj = null;
            
            // FIX: Use FULL size for the base chunk so floors connect (No Gaps)
            float floorSize = chunkSize; 
            
            // Use a slightly smaller area for the FBX spawning to prevent overlap
            float spawnAreaSize = chunkSize - 10f;

            switch (step.typeId)
            {
                case "BaseStart": chunkObj = ChunkPrefabs.CreateBase(finalPos, palette, true, floorSize, customFloorMaterial); break;
                case "BaseEnd":   chunkObj = ChunkPrefabs.CreateBase(finalPos, palette, false, floorSize, customFloorMaterial); break;
                case "PoolStart": chunkObj = ChunkPrefabs.CreatePool(finalPos, palette, true, floorSize, poolFloorOffset, customFloorMaterial); break;
                case "PoolEnd":   chunkObj = ChunkPrefabs.CreatePool(finalPos, palette, false, floorSize, poolFloorOffset, customFloorMaterial); break;
                case "Trees":     chunkObj = ChunkPrefabs.CreateTrees(finalPos, palette, step.isMirrored, floorSize, treeFloorOffset, customFloorMaterial); break;
                case "Garden":    chunkObj = ChunkPrefabs.CreateGarden(finalPos, palette, step.isMirrored, floorSize, gardenFloorOffset, customFloorMaterial); break;
                case "TikiBar":   chunkObj = ChunkPrefabs.CreateTikiBar(finalPos, palette, step.isMirrored, floorSize, tikiFloorOffset, customFloorMaterial); break;
                case "HotTub":    chunkObj = ChunkPrefabs.CreateHotTub(finalPos, palette, step.isMirrored, floorSize, hotTubFloorOffset, customFloorMaterial); break;
            }

            if (chunkObj != null)
            {
                chunkObj.transform.SetParent(parent);
                
                // Spawn Walls for Logic (Only on Main Map)
                // Removed exclusion of BaseStart/BaseEnd so they get proper walls too
                if (parent.name == "MainMap")
                {
                    GenerateWallsForChunk(chunkObj, step.position, floorSize);
                }

                // Ensure physics are synced before we try to spawn effectively
                Physics.SyncTransforms(); 
                
                // NEW: Spawn FBX Models based on mapping with collision checks
                GenerateObjectsForChunk(chunkObj, step.typeId, spawnAreaSize);
            }

            if (step.typeId == "PoolStart" && parent.name == "MainMap")
            {
                if (GameObject.FindFirstObjectByType<GameManager>() == null) new GameObject("GameManager").AddComponent<GameManager>();
                GameObject triggerObj = new GameObject("CaptureZone_Logic");
                triggerObj.transform.SetParent(parent);
                triggerObj.transform.position = finalPos; 
                triggerObj.AddComponent<CaptureZone>();
            }
        }

        [Header("Materials & Textures")]
        public Material customFloorMaterial;
        public Material customFenceMaterial;

        private void GenerateWallsForChunk(GameObject chunk, Vector3 gridPos, float size)
        {
            Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
            
            foreach (var dir in directions)
            {
                // Find Neighbor
                MapStep? neighborStep = null;
                // Manual search because we need the actual step data
                foreach(var s in currentMapBlueprint) {
                   Vector3 diff = s.position - gridPos;
                   if (diff.magnitude < 1f) continue; // It's us
                   if (diff.magnitude > chunkSize * 1.5f) continue; // Too far (increased tolerance)
                   // More forgiving dot product check (0.7 instead of 0.9)
                   if (Vector3.Dot(diff.normalized, dir) > 0.7f) {
                       neighborStep = s;
                       break;
                   }
                }
                
                bool hasNeighbor = neighborStep.HasValue;

                // Case 1: No Neighbor -> Spawn Main Outer Wall
                if (!hasNeighbor)
                {
                    // FENCE GROUP
                    GameObject fenceGroup = new GameObject("NeonFence_Edge");
                    fenceGroup.transform.SetParent(chunk.transform);
                    Vector3 localPos = dir * (size * 0.5f);
                    fenceGroup.transform.localPosition = localPos;
                    fenceGroup.transform.localRotation = Quaternion.LookRotation(dir);

                    // Wall Dimensions
                    float wallLength = size + 5f; 
                    float height = 4f;
                    float thickness = 1f;

                    // Spawn Visuals (Method extracted or inline)
                    SpawnFenceVisuals(fenceGroup, wallLength, height, thickness, palette, customFenceMaterial);
                }
                // Case 2: Has Neighbor but Offset -> Spawn Bridge Wall
                else if (neighborStep.HasValue)
                {
                     float zDiff = neighborStep.Value.position.z - gridPos.z;
                     
                     // Only bridge if looking Left/Right (X neighbors) AND there's an offset
                     bool isXNeighbor = (dir == Vector3.left || dir == Vector3.right);
                     
                     if (isXNeighbor && Mathf.Abs(zDiff) > 1f)
                     {
                         // The bridge fills the Z-gap, so it runs along the Z-axis (Forward/Back)
                         // and is positioned at the X-edge of the chunk.
                         
                         GameObject bridgeGroup = new GameObject("NeonFence_Bridge");
                         bridgeGroup.transform.SetParent(chunk.transform);
                         
                         float bridgeLength = Mathf.Abs(zDiff) + 2f; // +2 overlap
                         
                         // Calculate Z position of the gap center
                         float zCenter = 0f;
                         if (zDiff > 0) // Neighbor is shifted +Z (up)
                         {
                             // My chunk covers Z: [-size/2, +size/2]
                             // Neighbor covers Z: [-size/2 + zDiff, +size/2 + zDiff]
                             // Gap on my side: Z from -size/2 to -size/2 + zDiff
                             zCenter = -size * 0.5f + Mathf.Abs(zDiff) * 0.5f;
                         }
                         else // Neighbor is shifted -Z (down)
                         {
                             // Gap on my side: Z from +size/2 + zDiff to +size/2
                             zCenter = size * 0.5f + zDiff * 0.5f;
                         }
                         
                         // Position at the X-edge, centered on the gap Z
                         Vector3 localPos = new Vector3(dir.x * (size * 0.5f), 0, zCenter);
                         bridgeGroup.transform.localPosition = localPos;
                         
                         // ROTATE to face Forward (Z+) so the wall spans along Z
                         bridgeGroup.transform.localRotation = Quaternion.LookRotation(Vector3.forward);

                         // Visuals
                         SpawnFenceVisuals(bridgeGroup, bridgeLength, 4f, 1f, palette, customFenceMaterial);
                     }
                }
            }
        }

        private void SpawnFenceVisuals(GameObject parent, float length, float height, float thickness, ColorPalette pal, Material customMat)
        {
            // 1. Bottom Base
            GameObject baseWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseWall.transform.SetParent(parent.transform);
            baseWall.transform.localPosition = new Vector3(0, 1f, 0); 
            baseWall.transform.localRotation = Quaternion.identity;
            baseWall.transform.localScale = new Vector3(length, 2f, 1f); 
            
            Material baseMat = customMat != null ? customMat : new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (customMat == null) {
                if (baseMat.shader.name == "Hidden/InternalErrorShader") baseMat = new Material(Shader.Find("Standard"));
                baseMat.color = pal.Secondary;
                baseMat.SetColor("_EmissionColor", pal.Secondary * 1.5f);
                baseMat.EnableKeyword("_EMISSION");
            }
            baseWall.GetComponent<Renderer>().material = baseMat;

            // 2. Top Rail
            GameObject topRail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topRail.transform.SetParent(parent.transform);
            topRail.transform.localPosition = new Vector3(0, height, 0); 
            topRail.transform.localScale = new Vector3(length, 0.5f, 0.5f);
            
            Material topMat = new Material(baseMat);
            topMat.color = pal.Primary;
            topMat.SetColor("_EmissionColor", pal.Primary * 2.0f);
            topRail.GetComponent<Renderer>().material = topMat;

            // 3. Posts
            int postCount = Mathf.Max(2, Mathf.CeilToInt(length / 10f) + 1);
            float spacing = length / (postCount - 1);
            if (postCount < 2) spacing = 0;
            
            for (int i = 0; i < postCount; i++)
            {
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                post.transform.SetParent(parent.transform);
                float xPos = -length/2 + i * spacing;
                post.transform.localPosition = new Vector3(xPos, height/2 + 0.5f, 0);
                post.transform.localScale = new Vector3(0.5f, height - 1f, 0.5f);
                post.GetComponent<Renderer>().material = topMat;
            }
            
            // 4. Corner Pillars
            for (int k = -1; k <= 1; k += 2)
            {
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillar.transform.SetParent(parent.transform);
                pillar.transform.localPosition = new Vector3(k * (length * 0.5f), height * 0.5f, 0); 
                pillar.transform.localScale = new Vector3(thickness * 1.2f, height + 1f, thickness * 1.2f);
                pillar.GetComponent<Renderer>().material = topMat;
            }
        }


        private void SpawnFallbackTrees(GameObject chunk, float size, GameObject model)
        {
            if (model == null) return;
            
            int count = 10;
            for (int i = 0; i < count; i++)
            {
                // Simple random placement for fallback - Use ANY to be safe
                bool found = ItemPlacementHelper.TryGetValidPlacement(chunk.transform.position, size, 2f * fallbackScale, 
                    PlacementType.Any, spawnObstacleMask, out Vector3 localPos);

                if (found)
                {
                    GameObject spawned = Instantiate(model, chunk.transform);
                    spawned.transform.localPosition = localPos;
                    spawned.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                    
                    // Controlled Scale
                    float scale = Random.Range(0.8f, 1.2f) * fallbackScale;
                    spawned.transform.localScale = Vector3.one * scale;

                    if (spawned.GetComponent<Collider>() == null && spawned.GetComponentInChildren<Collider>() == null)
                    {
                        var col = spawned.AddComponent<BoxCollider>();
                        col.size = Vector3.one * 1f; 
                        col.center = Vector3.up * 1f;
                    }
                    Physics.SyncTransforms();
                }
            }
        }

        private void GenerateObjectsForChunk(GameObject chunk, string typeId, float size)
        {
            // FALLBACK SYSTEM
            if (chunkDataStore == null)
            {
                if (typeId == "Trees" && fallbackTreeModel != null) SpawnFallbackTrees(chunk, size, fallbackTreeModel);
                else if (typeId == "TikiBar" && fallbackTikiModel != null) SpawnFallbackTrees(chunk, size, fallbackTikiModel);
                else if (typeId == "HotTub" && fallbackHotTubModel != null) SpawnFallbackTrees(chunk, size, fallbackHotTubModel);
                else if (typeId == "Garden" && fallbackGardenModel != null) SpawnFallbackTrees(chunk, size, fallbackGardenModel);
                else if (typeId.Contains("Pool") && fallbackPoolModel != null) SpawnFallbackTrees(chunk, size, fallbackPoolModel);
                return;
            }

            ChunkObjectMapping mapping = chunkDataStore.GetMapping(typeId);
            if (mapping == null || mapping.weightedPrefabs == null || mapping.weightedPrefabs.Count == 0)
            {
                // Try Fallbacks if mapping fails
                if (typeId == "Trees") SpawnFallbackTrees(chunk, size, fallbackTreeModel);
                else if (typeId == "TikiBar") SpawnFallbackTrees(chunk, size, fallbackTikiModel);
                return;
            }

            int count = Random.Range(mapping.minQuantity, mapping.maxQuantity + 1);

            // Safety limit for retries per chunk to avoid infinite loops if map is full
            int totalFailures = 0;

            for (int i = 0; i < count; i++)
            {
                if (totalFailures > 20) break; // Optimized: Give up if area is too crowded
                
                // Bulletproof: Check valid placement in World Space 
                // We use chunk.transform.position as the origin
                bool found = ItemPlacementHelper.TryGetValidPlacement(chunk.transform.position, size, mapping.itemSpacingRadius, 
                    mapping.preferredPlacement, spawnObstacleMask, out Vector3 localPos);

                if (found)
                {
                     GameObject prefab = mapping.GetRandomPrefab();
                     if (prefab != null)
                     {
                         GameObject spawned = Instantiate(prefab, chunk.transform);
                         spawned.transform.localPosition = localPos;
                         spawned.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                         ChunkPrefabs.AdjustForFlow(spawned, spawned.transform.position);
                         
                         // Bulletproof: Ensure it has a collider so future raycasts hit it
                         // If the FBX doesn't have a collider, dynamic placement will overlap.
                         // We add a simple BoxCollider if missing to ensure spacing works.
                         if (spawned.GetComponent<Collider>() == null && spawned.GetComponentInChildren<Collider>() == null)
                         {
                             var col = spawned.AddComponent<BoxCollider>();
                             col.size = Vector3.one * 1.5f; 
                             col.center = Vector3.up * 0.75f;
                         }
                         
                         // Force update physics system so the next iteration sees this new collider
                         Physics.SyncTransforms();
                     }
                }
                else
                {
                    totalFailures++;
                    i--; // Retry this index
                }
            }
        }

        public void Cleanup()
        {
            foreach (var obj in activeObjects) if (obj) DestroyImmediate(obj);
            activeObjects.Clear();
            GameObject existingPlayer = GameObject.Find("Player_Generated");
            if (existingPlayer) DestroyImmediate(existingPlayer);
        }

        private void SpawnPlayer()
        {
            var startStep = currentMapBlueprint.FirstOrDefault(s => s.typeId == "BaseStart");
            Vector3 spawnPos = startStep.position + Vector3.up * 5f;

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            player.name = "Player_Generated";
            player.transform.position = spawnPos;
            var controller = player.AddComponent<PlayerController>();
            controller.palette = palette;
            player.AddComponent<PlayerTeam>().team = Team.Blue;
            player.GetComponent<Renderer>().material.color = Color.blue;

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.SetParent(player.transform);
                mainCam.transform.localPosition = new Vector3(0, 1.8f, 0.2f); // Better first person feel 
                mainCam.transform.localRotation = Quaternion.identity;
                
                // Attach AAA Camera Systems
                mainCam.gameObject.AddComponent<CameraPerception>();
            }
        }
    }
}
