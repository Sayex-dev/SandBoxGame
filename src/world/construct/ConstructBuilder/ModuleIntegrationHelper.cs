using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// Shared helper for integrating generated modules into construct data and visuals.
/// Used by both ConstructStreamingLoader and ConstructOneTimeLoader.
/// </summary>
public static class ModuleIntegrationHelper
{
    /// <summary>
    /// Integrates generated modules into the construct's data and visuals (bounds, module store, mesh).
    /// </summary>
    public static async Task IntegrateGeneratedModules(
        IEnumerable<Task<GenerateModulesResponse>> generationTasks,
        ConstructBlockController blockController,
        ConstructVoxelBlockVisualsController visuals)
    {
        foreach (Task<GenerateModulesResponse> task in generationTasks)
        {
            var response = await task;
            foreach (var kvp in response.GeneratedModules)
            {
                ModuleLocation moduleLocation = kvp.Key;
                Module module = kvp.Value;
                Mesh mesh = response.Meshes[moduleLocation];

                // Update modules
                blockController.AddModule(moduleLocation, module);
                visuals.AddModule(moduleLocation, mesh);
            }
        }
    }
}
