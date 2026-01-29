using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor.Core.Configuration;
#endif

namespace Core
{
    /// <summary>
    /// Ensures the project is always running on URP with required settings.
    /// Runs on Editor Load to prevent "It works on my machine" issues.
    /// </summary>
    [InitializeOnLoad]
    public class URPBootstrap
    {
        static URPBootstrap()
        {
            EditorApplication.delayCall += CheckURPStatus;
        }

        private static void CheckURPStatus()
        {
            var pipeline = GraphicsSettings.currentRenderPipeline;
            if (pipeline == null)
            {
                Debug.LogWarning("[URPBootstrap] No Render Pipeline Asset assigned! Attempting to fix...");
                TryCreateAndAssignURP();
            }
            else
            {
                // Optional: Check if it is actually URP
                if (pipeline.GetType().Name.Contains("UniversalRenderPipelineAsset")) 
                {
                    // Good
                }
                else
                {
                     Debug.LogWarning($"[URPBootstrap] Warning: Current pipeline is {pipeline.GetType().Name}, expected URP.");
                }
            }
        }

        private static void TryCreateAndAssignURP()
        {
            // Note: Creating a full URP asset via script is complex because it involves creating the Asset and the RendererData.
            // For this bootstrap, we will look for an existing one in the project first.
            
            string[] guids = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                RenderPipelineAsset asset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path);
                if (asset != null)
                {
                    GraphicsSettings.defaultRenderPipeline = asset;
                    QualitySettings.renderPipeline = asset;
                    Debug.Log($"[URPBootstrap] Auto-assigned URP Asset found at: {path}");
                    return;
                }
            }

            Debug.LogError("[URPBootstrap] CRITICAL: No URP Asset found in project. Please create one via Create > Rendering > URP Asset (with Universal Renderer) and assign it in Project Settings > Graphics.");
        }
    }
}
