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
        ConstructData data,
        ConstructVisualsController visuals)
    {
        foreach (Task<GenerateModulesResponse> task in generationTasks)
        {
            var response = await task;
            foreach (var kvp in response.GeneratedModules)
            {
                ModuleLocation moduleLocation = kvp.Key;
                Module module = kvp.Value;
                Mesh mesh = response.Meshes[moduleLocation];

                // Update bounds
                data.Bounds.AddPosition(module.MinPos.ToConstruct(moduleLocation, module.ModuleSize));
                data.Bounds.AddPosition(module.MaxPos.ToConstruct(moduleLocation, module.ModuleSize));

                // Update modules
                data.Modules.Add(moduleLocation, module);

                // Update visuals
                visuals.AddModule(moduleLocation, mesh);
            }
        }
    }

    /// <summary>
    /// Unloads modules and removes their visuals. Rebuilds bounds if necessary.
    /// </summary>
    public static void UnloadModules(
        List<ModuleLocation> toUnload,
        ConstructData data,
        ConstructVisualsController visuals)
    {
        bool needsBoundsRebuild = false;
        foreach (ModuleLocation moduleLocation in toUnload)
        {
            // Remove from modules
            if (data.Modules.Remove(moduleLocation, out Module module))
            {
                // Update bounds if necessary
                if (module.HasBlocks)
                {
                    ConstructGridPos minPos = module.MinPos.ToConstruct(moduleLocation, module.ModuleSize);
                    ConstructGridPos maxPos = module.MaxPos.ToConstruct(moduleLocation, module.ModuleSize);

                    if (data.Bounds.IsOnBounds(minPos) || data.Bounds.IsOnBounds(maxPos))
                    {
                        needsBoundsRebuild = true;
                    }
                }
            }

            // Remove visuals
            visuals.RemoveModule(moduleLocation);
        }

        if (needsBoundsRebuild)
        {
            RebuildBounds(data);
        }
    }

    /// <summary>
    /// Rebuilds bounds from all currently loaded modules.
    /// </summary>
    public static void RebuildBounds(ConstructData data)
    {
        data.Bounds.Clear();
        foreach (var kvp in data.Modules.Modules)
        {
            var remainingModule = kvp.Value;
            var remainingLocation = kvp.Key;
            if (remainingModule.HasBlocks)
            {
                data.Bounds.AddPosition(remainingModule.MinPos.ToConstruct(remainingLocation, remainingModule.ModuleSize));
                data.Bounds.AddPosition(remainingModule.MaxPos.ToConstruct(remainingLocation, remainingModule.ModuleSize));
            }
        }
    }
}
