using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public class GlobalConstructController : IConstructController
{
    private ConstructCore core;

    private IWorldQuery collisionQuery;
    private ConstructGenerator generator;
    private List<Tuple<SimulationMode, float>> simulationModeDistances;

    private ConstructModuleBuilder moduleBuilder;
    private ConstructVisualsController visuals;

    public GlobalConstructController(
        ConstructCore core,
        IWorldQuery collisionQuery,
        ConstructGenerator generator,
        Node3D parent)
    {
        this.core = core;
        this.collisionQuery = collisionQuery;
        this.generator = generator;

        simulationModeDistances = GameSettings.Instance.SimulationModeDistances;

        moduleBuilder = new ConstructModuleBuilder();
        visuals = new ConstructVisualsController();
        parent.AddChild(visuals);
    }

    public virtual void SetBlock(Block block, ConstructGridPos pos) => core.Blocks.SetBlock(pos, block);
    public virtual void RemoveBlock(ConstructGridPos pos) => core.Blocks.SetBlock(pos, new Block());

    public virtual bool TryGetBlock(ConstructGridPos pos, out Block block)
    {
        return core.Blocks.TryGetBlock(pos, out block);
    }
    public void Update(double delta)
    {

    }

    public void UpdateLoading(WorldGridPos loadPos)
    {
        BuildAround(loadPos, (int)simulationModeDistances[0].Item2).FireAndForget();
    }

    private async Task BuildAround(WorldGridPos worldPos, int loadDistance)
    {
        var generationResponse = moduleBuilder.GenerateModulesAround(
            worldPos, loadDistance, core.Data.GridTransform, core.Data.Modules, generator);

        await ModuleIntegrationHelper.IntegrateGeneratedModules(
            generationResponse.GenerationTaskHandles, core.Data, visuals);

        ModuleIntegrationHelper.UnloadModules(
            generationResponse.ToUnload, core.Data, visuals);
    }
}