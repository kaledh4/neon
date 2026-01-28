using UnityEngine;
using System.Collections.Generic;

namespace NeonSplash
{
    public class MapGenerator : MonoBehaviour
    {
        public int seed = 12345;
        public List<GameObject> chunks = new List<GameObject>();
        public List<GameObject> bridges = new List<GameObject>();
        private ColorPalette palette;
        private float[] zOffsets = new float[10];

        void Start()
        {
            GenerateMap();
        }

        [ContextMenu("Generate Map")]
        public void GenerateMap()
        {
            // Clear existing
            foreach (var c in chunks) if(c) DestroyImmediate(c);
            foreach (var b in bridges) if(b) DestroyImmediate(b);
            chunks.Clear();
            bridges.Clear();

            Random.InitState(seed);
            palette = new ColorPalette(seed);

            string[] prefabTypes = { "Trees", "Garden", "TikiBar", "HotTub" };
            List<string> firstHalfTypes = new List<string>();
            List<float> firstHalfOffsets = new List<float>();

            // Chunk 0: Blue Base
            zOffsets[0] = 0;
            chunks.Add(ChunkPrefabs.CreateBase(new Vector3(0, 0, 0), palette, true));

            // Chunks 1-3
            for (int i = 1; i <= 3; i++)
            {
                string type = prefabTypes[Random.Range(0, prefabTypes.Length)];
                float zOffset = (Random.Range(0, 3) - 1) * 3f;
                
                firstHalfTypes.Add(type);
                firstHalfOffsets.Add(zOffset);
                zOffsets[i] = zOffset;

                chunks.Add(CreateChunkPrefab(type, i, zOffset, false));
            }

            // Chunks 4-5: Pool
            zOffsets[4] = 0;
            zOffsets[5] = 0;
            chunks.Add(ChunkPrefabs.CreatePool(new Vector3(40, 0, 0), palette, true));
            chunks.Add(ChunkPrefabs.CreatePool(new Vector3(50, 0, 0), palette, false));

            // Chunks 6-8: Mirrored
            for (int i = 0; i < 3; i++)
            {
                int destIdx = 6 + i;
                int srcIdx = 2 - i;
                string type = firstHalfTypes[srcIdx];
                float zOffset = -firstHalfOffsets[srcIdx];

                zOffsets[destIdx] = zOffset;
                chunks.Add(CreateChunkPrefab(type, destIdx, zOffset, true));
            }

            // Chunk 9: Red Base
            zOffsets[9] = 0;
            chunks.Add(ChunkPrefabs.CreateBase(new Vector3(90, 0, 0), palette, false));

            // Bridges
            CreateBridges();
            
            // Apply fog and background colors to scene if possible (optional but nice)
            RenderSettings.fog = true;
            RenderSettings.fogColor = palette.Fog;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.02f;
            if (Camera.main) Camera.main.backgroundColor = palette.Background;
        }

        private GameObject CreateChunkPrefab(string type, int index, float zOffset, bool mirror)
        {
            Vector3 pos = new Vector3(index * 10, 0, zOffset);
            switch (type)
            {
                case "Trees": return ChunkPrefabs.CreateTrees(pos, palette, mirror);
                case "Garden": return ChunkPrefabs.CreateGarden(pos, palette, mirror);
                case "TikiBar": return ChunkPrefabs.CreateTikiBar(pos, palette, mirror);
                case "HotTub": return ChunkPrefabs.CreateHotTub(pos, palette, mirror);
                default: return new GameObject("EmptyChunk");
            }
        }

        private void CreateBridges()
        {
            for (int i = 0; i < 9; i++)
            {
                if (zOffsets[i] != zOffsets[i + 1])
                {
                    Vector3 start = new Vector3(i * 10 + 5, 0, zOffsets[i]);
                    Vector3 end = new Vector3((i + 1) * 10 - 5, 0, zOffsets[i + 1]);
                    bridges.Add(ChunkPrefabs.CreateBridge(start, end, palette));
                }
            }
        }
    }
}
