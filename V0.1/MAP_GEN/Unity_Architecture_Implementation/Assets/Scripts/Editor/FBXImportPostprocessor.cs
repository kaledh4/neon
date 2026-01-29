using UnityEngine;
using UnityEditor;
using Core;

namespace Editor
{
    /// <summary>
    /// Automatically runs Asset Semantic Rules whenever an FBX is imported.
    /// "Golden Workflow": Import -> Rename -> Done.
    /// </summary>
    public class FBXImportPostprocessor : AssetPostprocessor
    {
        private void OnPostprocessModel(GameObject root)
        {
            // Only run on model files, not everything
            if (!assetPath.EndsWith(".fbx") && !assetPath.EndsWith(".obj")) return;

            // Run the semantic resolver logic on the imported hierarchy
            // Note: Since this is "OnPostprocessModel", the components added here will be part of the Prefab.
            
            // We use the same cleanup logic as the runtime resolver
            PerformRecursiveResolve(root.transform);
        }

        private void PerformRecursiveResolve(Transform t)
        {
            AssetSemanticResolver.Resolve(t.gameObject);

            foreach (Transform child in t)
            {
                PerformRecursiveResolve(child);
            }
        }
    }
}
