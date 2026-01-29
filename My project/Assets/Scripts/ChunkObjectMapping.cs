using UnityEngine;
using System.Collections.Generic;

namespace NeonSplash.V0_1
{
    [System.Serializable]
    public class PrefabWeight
    {
        [Tooltip("The FBX model to spawn.")]
        public GameObject prefab;

        [Tooltip("Relative chance to spawn. Higher = more frequent.")]
        [Range(0.1f, 100f)]
        public float weight = 1.0f;
    }

    [System.Serializable]
    public class ChunkObjectMapping
    {
        [Header("Classification")]
        [Tooltip("Algorithm matches this string to the Chunk Name in the generator.")]
        public string chunkTypeName; 

        [Header("Spawning Rules")]
        [Tooltip("List of prefabs with weighted probabilities.")]
        public List<PrefabWeight> weightedPrefabs = new List<PrefabWeight>();
        
        [Tooltip("Where should these items generally appear?")]
        public PlacementType preferredPlacement = PlacementType.Any;

        [Space(10)]
        [Tooltip("Minimum items to spawn per chunk.")]
        [Range(0, 50)]
        public int minQuantity = 5;

        [Tooltip("Maximum items to spawn per chunk.")]
        [Range(0, 50)]
        public int maxQuantity = 15;

        [Tooltip("Minimum distance between spawned items to prevent clipping.")]
        [Range(0.1f, 5f)]
        public float itemSpacingRadius = 1.5f;

        // Removed unused cached variable to keep it clean

        /// <summary>
        /// Efficiently gets a random prefab based on weights.
        /// </summary>
        public GameObject GetRandomPrefab()
        {
            if (weightedPrefabs == null || weightedPrefabs.Count == 0) return null;
            
            float totalWeight = 0;
            foreach (var pw in weightedPrefabs) totalWeight += pw.weight;
            
            float randomValue = Random.Range(0f, totalWeight);
            float currentSum = 0;
            
            foreach (var pw in weightedPrefabs)
            {
                currentSum += pw.weight;
                if (randomValue <= currentSum) return pw.prefab;
            }
            
            return weightedPrefabs[0].prefab; // Fallback
        }

        public void Validate()
        {
            if (minQuantity > maxQuantity) maxQuantity = minQuantity;
            if (itemSpacingRadius < 0.1f) itemSpacingRadius = 0.1f;
        }
    }

    public enum PlacementType { Any, NearWall, OpenSpace, Center }

    [CreateAssetMenu(fileName = "ChunkDataStore", menuName = "NeonSplash/Chunk Data Store")]
    public class ChunkDataStore : ScriptableObject
    {
        public List<ChunkObjectMapping> mappings = new List<ChunkObjectMapping>();

        public ChunkObjectMapping GetMapping(string typeName)
        {
            return mappings.Find(m => typeName.Contains(m.chunkTypeName, System.StringComparison.OrdinalIgnoreCase));
        }

        private void OnValidate()
        {
            foreach (var m in mappings)
            {
                if (m != null) m.Validate();
            }
        }
    }
}
