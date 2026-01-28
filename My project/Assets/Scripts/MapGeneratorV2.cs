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
            RenderSettings.fogDensity = 0.005f; // reduced for larger scale
            if (Camera.main) Camera.main.backgroundColor = palette.Background;
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

        private void SpawnChunk(MapStep step, Vector3 offset, Transform parent)
        {
            Vector3 finalPos = step.position + offset;
            GameObject chunkObj = null;

            float innerSize = 100f; // Scale property

            switch (step.typeId)
            {
                case "BaseStart": chunkObj = ChunkPrefabs.CreateBase(finalPos, palette, true, innerSize); break;
                case "BaseEnd":   chunkObj = ChunkPrefabs.CreateBase(finalPos, palette, false, innerSize); break;
                case "PoolStart": chunkObj = ChunkPrefabs.CreatePool(finalPos, palette, true, innerSize); break;
                case "PoolEnd":   chunkObj = ChunkPrefabs.CreatePool(finalPos, palette, false, innerSize); break;
                case "Trees":     chunkObj = ChunkPrefabs.CreateTrees(finalPos, palette, step.isMirrored, innerSize); break;
                case "Garden":    chunkObj = ChunkPrefabs.CreateGarden(finalPos, palette, step.isMirrored, innerSize); break;
                case "TikiBar":   chunkObj = ChunkPrefabs.CreateTikiBar(finalPos, palette, step.isMirrored, innerSize); break;
                case "HotTub":    chunkObj = ChunkPrefabs.CreateHotTub(finalPos, palette, step.isMirrored, innerSize); break;
            }

            if (chunkObj != null) chunkObj.transform.SetParent(parent);

            if (step.typeId == "PoolStart" && parent.name == "MainMap")
            {
                if (GameObject.FindFirstObjectByType<GameManager>() == null) new GameObject("GameManager").AddComponent<GameManager>();
                GameObject triggerObj = new GameObject("CaptureZone_Logic");
                triggerObj.transform.SetParent(parent);
                triggerObj.transform.position = finalPos; 
                triggerObj.AddComponent<CaptureZone>();
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
                mainCam.transform.localPosition = new Vector3(0, 2.5f, -6f); 
                mainCam.transform.localRotation = Quaternion.identity;
            }
        }
    }
}
