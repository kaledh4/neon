using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NeonSplash.V0_1
{
    public enum FloorType
    {
        Grass,
        Wood,
        Tile,
        Water
    }

    [System.Serializable]
    public struct ChunkMapping
    {
        public string id;           // e.g., "Trees", "TikiBar"
        public GameObject fbxModel; // Drag your FBX Prefab here
        public FloorType floorType; // Select the floor texture/material
    }

    public class MapGeneratorV2 : MonoBehaviour
    {
        [Header("Settings")]
        public int seed = 12345;
        public float chunkSize = 10f;
        
        [Header("Visuals")]
        public Material skyboxMaterial;
        public Material grassMat;
        public Material woodMat;
        public Material tileMat;
        public Material waterMat;

        [Header("Parallel Maps")]
        public bool spawnParallelMaps = true;
        public float parallelDistance = 60f; // Distance between main map and side maps

        [Header("Configuration")]
        public List<ChunkMapping> chunkLibrary = new List<ChunkMapping>();

        // Internal tracking
        private List<GameObject> activeObjects = new List<GameObject>();
        private ColorPalette palette; // Assuming you kept your ColorPalette class
        
        // Data structure to hold the "Blueprint" of the map so we can copy it
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

            // 1. Setup Environment
            SetupSkybox();
            
            // 2. Initialize Random & Palette
            Random.InitState(seed);
            palette = new ColorPalette(seed); // Using your existing class

            // 3. Calculate the "Blueprint" (The logic of what goes where)
            CalculateMapLogic();

            // 4. Build Main Map (Center)
            BuildMapInstance(Vector3.zero, "MainMap");

            // 5. Build Parallel Maps (Visual Only)
            if (spawnParallelMaps)
            {
                // Right Map
                BuildMapInstance(new Vector3(parallelDistance, 0, 0), "Parallel_Right");
            }

            // 6. Spawn Player and Setup Camera
            SpawnPlayer();
        }

        private void SetupSkybox()
        {
            if (skyboxMaterial != null)
            {
                RenderSettings.skybox = skyboxMaterial;
                RenderSettings.fog = true;
                // Optional: Match fog to skybox or palette
                RenderSettings.fogColor = palette.Fog; 
                RenderSettings.fogDensity = 0.015f;
            }
        }

        private void CalculateMapLogic()
        {
            currentMapBlueprint.Clear();

            string[] randomTypes = { "Trees", "Garden", "TikiBar", "HotTub" };
            List<string> firstHalfTypes = new List<string>();
            List<float> firstHalfOffsets = new List<float>();

            // --- Logic from your original script preserved below ---

            // Chunk 0: Blue Base (Start)
            AddBlueprintStep("BaseStart", new Vector3(0, 0, 0), false);

            // Chunks 1-3: Random
            for (int i = 1; i <= 3; i++)
            {
                string type = randomTypes[Random.Range(0, randomTypes.Length)];
                float zOffset = (Random.Range(0, 3) - 1) * 3f;

                firstHalfTypes.Add(type);
                firstHalfOffsets.Add(zOffset);

                AddBlueprintStep(type, new Vector3(i * chunkSize, 0, zOffset), false);
            }

            // Chunks 4-5: Pool
            AddBlueprintStep("PoolStart", new Vector3(40, 0, 0), true);
            AddBlueprintStep("PoolEnd", new Vector3(50, 0, 0), false);

            // Chunks 6-8: Mirrored
            for (int i = 0; i < 3; i++)
            {
                int destIdx = 6 + i;
                int srcIdx = 2 - i;
                string type = firstHalfTypes[srcIdx];
                float zOffset = -firstHalfOffsets[srcIdx]; // Mirror Z

                AddBlueprintStep(type, new Vector3(destIdx * chunkSize, 0, zOffset), true);
            }

            // Chunk 9: Red Base (End)
            AddBlueprintStep("BaseEnd", new Vector3(90, 0, 0), false);
        }

        private void AddBlueprintStep(string id, Vector3 pos, bool mirror)
        {
            currentMapBlueprint.Add(new MapStep { typeId = id, position = pos, isMirrored = mirror });
        }

        // This function actually spawns objects based on the blueprint
        private void BuildMapInstance(Vector3 worldOffset, string containerName)
        {
            GameObject container = new GameObject(containerName);
            activeObjects.Add(container);

            foreach (var step in currentMapBlueprint)
            {
                SpawnChunk(step, worldOffset, container.transform);
            }

            // Generate Bridges for this specific map instance
            GenerateBridges(worldOffset, container.transform);

            // 6. If this is a parallel map, remove all colliders to ensure it's visual-only
            if (containerName.Contains("Parallel"))
            {
                DisableColliders(container);
            }
        }

        private void DisableColliders(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                DestroyImmediate(col);
            }
                }

        private void SpawnChunk(MapStep step, Vector3 offset, Transform parent)
        {
            // 1. Find the config for this type
            ChunkMapping config = chunkLibrary.FirstOrDefault(c => c.id == step.typeId);
            
            // If ID not found, default to first or skip
            if (string.IsNullOrEmpty(config.id)) 
            {
                // Fallback for BaseStart/End if not defined in list
                if(step.typeId.Contains("Base")) config.floorType = FloorType.Tile;
                else if(step.typeId.Contains("Pool")) config.floorType = FloorType.Water;
                else return; 
            }

            Vector3 finalPos = step.position + offset;

            // 2. Create Floor
            CreateFloor(finalPos, config.floorType, parent);

            // 3. Spawn FBX Model (If assigned)
            if (config.fbxModel != null)
            {
                GameObject model = Instantiate(config.fbxModel, finalPos, Quaternion.identity);
                model.transform.SetParent(parent);
                
                // Handle Mirroring
                if (step.isMirrored)
                {
                    Vector3 scale = model.transform.localScale;
                    scale.z *= -1; // Mirror on Z axis
                    model.transform.localScale = scale;
                }
            }
            
            // 4. Game Logic Injection (Capture Zone)
            // Only add to MainMap (not parallel visual ones) and only on PoolStart
            if (step.typeId == "PoolStart" && parent.name == "MainMap")
            {
                // Ensure GameManager exists
                if (GameObject.FindObjectOfType<GameManager>() == null)
                {
                    new GameObject("GameManager").AddComponent<GameManager>();
                }
                
                // Add Capture Zone
                GameObject zoneObj = config.fbxModel != null ? parent.GetChild(parent.childCount - 1).gameObject : parent.gameObject;
                // Actually, let's just add it to the 'floor' or create a child if no model exists, 
                // but since SpawnChunk creates logic iteratively, let's attach to the last spawned object or the parent anchor logic?
                // Safest is to create a dedicated child object for the trigger to ensure clean scaling.
                
                GameObject triggerObj = new GameObject("CaptureZone_Logic");
                triggerObj.transform.SetParent(parent); // Set to container
                triggerObj.transform.position = finalPos; 
                triggerObj.AddComponent<CaptureZone>();
            }
        }

        private void CreateFloor(Vector3 pos, FloorType type, Transform parent)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = pos + Vector3.down * 0.5f; // Move down so top is at 0
            floor.transform.localScale = new Vector3(chunkSize, 1, chunkSize);
            floor.transform.SetParent(parent);

            Renderer rend = floor.GetComponent<Renderer>();
            switch (type)
            {
                case FloorType.Grass: rend.material = grassMat; break;
                case FloorType.Wood: rend.material = woodMat; break;
                case FloorType.Tile: rend.material = tileMat; break;
                case FloorType.Water: rend.material = waterMat; break;
            }
        }

        private void GenerateBridges(Vector3 offset, Transform parent)
        {
            // Simple bridge logic connecting the blueprint nodes
            for (int i = 0; i < currentMapBlueprint.Count - 1; i++)
            {
                Vector3 p1 = currentMapBlueprint[i].position;
                Vector3 p2 = currentMapBlueprint[i+1].position;

                // Check if there is a Z-gap (meaning they aren't aligned)
                if (Mathf.Abs(p1.z - p2.z) > 0.1f)
                {
                    Vector3 bridgePos = (p1 + p2) / 2 + offset;
                    GameObject bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bridge.name = "Bridge";
                    bridge.transform.position = bridgePos + Vector3.down * 0.2f;
                    
                    // Calculate rotation to face next point
                    Vector3 dir = (p2 - p1).normalized;
                    bridge.transform.rotation = Quaternion.LookRotation(dir);
                    
                    // Stretch to fit
                    float dist = Vector3.Distance(p1, p2);
                    bridge.transform.localScale = new Vector3(2f, 0.5f, dist);
                    
                    bridge.GetComponent<Renderer>().material = woodMat; // Bridges are wood
                    bridge.transform.SetParent(parent);
                }
            }
        }

        public void Cleanup()
        {
            foreach (var obj in activeObjects)
            {
                if (obj) DestroyImmediate(obj);
            }
            activeObjects.Clear();
            
            // Also cleanup old players/cameras if specifically named/tracked (optional, but good for "Generate World" spam)
            GameObject existingPlayer = GameObject.Find("Player_Generated");
            if (existingPlayer) DestroyImmediate(existingPlayer);
        }

        private void SpawnPlayer()
        {
            // 1. Determine Spawn Point (Blue Base or Red Base)
            // BaseStart is roughly at 0,0,0. BaseEnd is at 90,0,0 (or last chunk)
            bool startAtBlue = Random.value > 0.5f;

            // Find actual positions from blueprint to be safe
            Vector3 spawnPos = Vector3.zero;
            Team team = Team.Blue;

            var startStep = currentMapBlueprint.FirstOrDefault(s => s.typeId == "BaseStart");
            var endStep = currentMapBlueprint.FirstOrDefault(s => s.typeId == "BaseEnd");

            if (startAtBlue)
            {
                spawnPos = startStep.position != Vector3.zero ? startStep.position : Vector3.zero;
                team = Team.Blue;
            }
            else
            {
                spawnPos = endStep.position != Vector3.zero ? endStep.position : new Vector3(chunkSize * 9, 0, 0);
                team = Team.Red;
            }

            // Lift up slightly
            spawnPos += Vector3.up * 2f;

            // 2. Create Player (Cylinder)
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            player.name = "Player_Generated";
            player.transform.position = spawnPos;
            
            // components
            PlayerController controller = player.AddComponent<PlayerController>();
            PlayerTeam teamComp = player.AddComponent<PlayerTeam>();
            teamComp.team = team;

            // Color the player
            player.GetComponent<Renderer>().material.color = team == Team.Blue ? Color.blue : Color.red;

            // 3. Setup Camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                CameraFollow camScript = mainCam.GetComponent<CameraFollow>();
                if (camScript == null) camScript = mainCam.gameObject.AddComponent<CameraFollow>();
                
                camScript.target = player.transform;
            }
        }
    }
}
