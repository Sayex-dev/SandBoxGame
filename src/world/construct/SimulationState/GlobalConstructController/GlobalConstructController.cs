using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GlobalConstructController : ConstructController
{
    private List<Tuple<SimulationMode, float>> simulationModeDistances;
    private ConstructBlockController blockController;

    public GlobalConstructController(
        ConstructGridTransformController transform,
        ConstructBlockController blockController,
        ConstructGenerator generator,
        ConstructModuleBuilder moduleBuilder,
        ConstructVoxelBlockVisualsController voxelVisuals,
        ConstructModelBlockVisualsController modelVisuals) : base(transform, generator, moduleBuilder, voxelVisuals, modelVisuals)
    {
        simulationModeDistances = GameSettings.Instance.SimulationModeDistances;

        this.blockController = blockController;
    }

    protected override void UpdateInternal(double delta) { }

    protected override void UpdateLoadingInternal(WorldGridPos loadPos)
    {
        BuildAround(loadPos, (int)simulationModeDistances[0].Item2).FireAndForget();
    }

    private async Task BuildAround(WorldGridPos worldPos, int loadDistance)
    {
        var generationResponse = moduleBuilder.GenerateModulesAround(
            worldPos, loadDistance, transform, blockController, generator);

        await ModuleIntegrationHelper.IntegrateGeneratedModules(
            generationResponse.GenerationTaskHandles, core.Data, voxelVisuals);

        UnloadModules(generationResponse.ToUnload, core.Data, voxelVisuals);
    }


    /// <summary>
    /// Unloads modules and removes their visuals. Rebuilds bounds if necessary.
    /// </summary>
    public static void UnloadModules(
        List<ModuleLocation> toUnload,
        ConstructVoxelBlockVisualsController visuals)
    {
        bool needsBoundsRebuild = false;
        foreach (ModuleLocation moduleLocation in toUnload)
        {
            // Remove from modules (fires OnModuleRemoved → visuals handled by event)
            if (data.Modules.Remove(moduleLocation, out Module module))
            {
                // Update bounds if necessary
                if (module.HasBlocks)
                {
                    ConstructGridPos minPos = module.MinPos.ToConstruct(moduleLocation);
                    ConstructGridPos maxPos = module.MaxPos.ToConstruct(moduleLocation);

                    if (data.Bounds.IsOnBounds(minPos) || data.Bounds.IsOnBounds(maxPos))
                    {
                        needsBoundsRebuild = true;
                    }
                }
            }
        }
    }

}