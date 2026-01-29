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

        [ContextMenu("Generate World")]
        public void GenerateWorld()
        {
            Cleanup();

            Random.InitState(seed);
            palette = new ColorPalette(seed); 

            SetupSkybox();
            CalculateMapLogic();

            // Build Maps
            BuildMapInstance(Vector3.zero, "MainMap", true);
            if (spawnParallelMaps)
            {
                BuildMapInstance(new Vector3(parallelDistance, 0, 0), "Parallel_Right", false);
                BuildMapInstance(new Vector3(-parallelDistance, 0, 0), "Parallel_Left", false);
            }

            SpawnPlayer();
        }

        private void SetupSkybox()
        {
            if (skyboxMaterial != null) RenderSettings.skybox = skyboxMaterial;
            RenderSettings.fog = true;
            RenderSettings.fogColor = palette.Fog; 
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.005f;

            // Match Progress Ambient Shift (Temporal Variation)
            float matchProgress = 0f;
            if (GameManager.Instance != null)
                matchProgress = 1f - (GameManager.Instance.currentMatchTime / GameManager.Instance.matchDuration);

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

            // Ensure Performance Tracking
            if (gameObject.GetComponent<PerformanceMonitor>() == null)
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
                float zOffset = (Random.Range(0, 3) - 1) * (chunkSize * 0.2f); // Offset scaled
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

        private void BuildMapInstance(Vector3 worldOffset, string containerName, bool isGameplay)
        {
            GameObject container = new GameObject(containerName);
            activeObjects.Add(container);

            foreach (var step in currentMapBlueprint)
            {
                SpawnChunk(step, worldOffset, container.transform);
            }

            if (!isGameplay) DisableColliders(container);
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
                case "BaseStart": chunkObj = ChunkPrefabs.CreateBase(finalPos, palette, true, floorSize); break;
                case "BaseEnd":   chunkObj = ChunkPrefabs.CreateBase(finalPos, palette, false, floorSize); break;
                case "PoolStart": chunkObj = ChunkPrefabs.CreatePool(finalPos, palette, true, floorSize, poolFloorOffset); break;
                case "PoolEnd":   chunkObj = ChunkPrefabs.CreatePool(finalPos, palette, false, floorSize, poolFloorOffset); break;
                case "Trees":     chunkObj = ChunkPrefabs.CreateTrees(finalPos, palette, step.isMirrored, floorSize, treeFloorOffset); break;
                case "Garden":    chunkObj = ChunkPrefabs.CreateGarden(finalPos, palette, step.isMirrored, floorSize, gardenFloorOffset); break;
                case "TikiBar":   chunkObj = ChunkPrefabs.CreateTikiBar(finalPos, palette, step.isMirrored, floorSize, tikiFloorOffset); break;
                case "HotTub":    chunkObj = ChunkPrefabs.CreateHotTub(finalPos, palette, step.isMirrored, floorSize, hotTubFloorOffset); break;
            }

            if (chunkObj != null)
            {
                chunkObj.transform.SetParent(parent);
                
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

        private void SpawnFallbackTrees(GameObject chunk, float size)
        {
            int count = 10;
            for (int i = 0; i < count; i++)
            {
                // Simple random placement for fallback - Use ANY to be safe
                bool found = ItemPlacementHelper.TryGetValidPlacement(chunk.transform.position, size, 2f * fallbackScale, 
                    PlacementType.Any, spawnObstacleMask, out Vector3 localPos);

                if (found)
                {
                    GameObject spawned = Instantiate(fallbackTreeModel, chunk.transform);
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
            // FALLBACK: If no DataStore, uses simple fallback for Trees
            if (chunkDataStore == null && typeId == "Trees" && fallbackTreeModel != null)
            {
                SpawnFallbackTrees(chunk, size);
                return;
            }

            if (chunkDataStore == null) return;

            ChunkObjectMapping mapping = chunkDataStore.GetMapping(typeId);
            if (mapping == null || mapping.weightedPrefabs == null || mapping.weightedPrefabs.Count == 0) return;

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
