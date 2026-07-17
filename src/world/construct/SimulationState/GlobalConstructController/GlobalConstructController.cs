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

        ModuleIntegrationHelper.UnloadModules(
            generationResponse.ToUnload, core.Data, voxelVisuals);
    }

}