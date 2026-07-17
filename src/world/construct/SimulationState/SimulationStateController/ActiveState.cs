using Godot;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
public class ActiveState : SimulationState
{
    private ConstructPhysicsController physics;
    private ConstructVisualMotionController visualMotion;
    private ConstructModuleBuilder moduleBuilder;
    private SecondOrderDynamicsSettings rotSodSettings;
    private SecondOrderDynamicsSettings moveSodSettings;
    private ConstructGenerator generator;
    private Node3D constructNode;
    private int moduleSize;
    private Vector3I prevModulePos;

    private Action<WorldGridPos> updateLoading;

    public ActiveState(
        ConstructCore core,
        ConstructGenerator generator,
        Node3D constructNode,
        Action<WorldGridPos> updateLoading,
        ConstructVoxelBlockVisualsController visuals,
        ConstructModelBlockVisualsController modelBlocks
    ) : base(core, visuals, modelBlocks)
    {
        this.generator = generator;
        this.constructNode = constructNode;
        this.updateLoading = updateLoading;

        moduleSize = GameSettings.Instance.ModuleSize;
        prevModulePos = Vector3I.Zero;
    }

    public override void Enter()
    {
        var rotSod = rotSodSettings.GetInstance(0);
        var moveSod = moveSodSettings.GetInstance(core.Data.PhysicsData.PhysicsPosition);

        if (core.Data.Modules.FullyLoaded)
        {
            RebuildAllModules();
        }
        else
        {
            GenerateAll(core.Data, moduleBuilder, visuals, generator).FireAndForget();
        }
    }

    public override void Exit()
    {
        Debug.WriteLine("Exited Active State");
    }

    public override void Update(double delta)
    {
        physics?.Update(delta);
        visualMotion?.Update(delta);

        constructNode.Position = visualMotion.Position;
        constructNode.Rotation = visualMotion.Rotation;

        Vector3I newModulePos = (Vector3I)visualMotion.Position / moduleSize;

        if (prevModulePos != newModulePos)
        {
            prevModulePos = newModulePos;
            updateLoading.Invoke((WorldGridPos)newModulePos);
        }
    }

    private async Task UpdateModuleMesh(ModuleLocation moduleLoc, Module module)
    {
        var context = new ModuleMeshGenerateContext(module, moduleLoc);
        var mesh = await moduleBuilder.GenerateModuleMesh(context);
        visuals.RemoveModule(moduleLoc);
        visuals.AddModule(moduleLoc, mesh);
    }

    private async Task UpdateModuleMesh(ModuleLocation moduleLoc)
    {
        if (!core.Data.Modules.TryGet(moduleLoc, out Module module))
            return;

        await UpdateModuleMesh(moduleLoc, module);
    }

    private void RebuildAllModules()
    {
        foreach (var kvp in core.Data.Modules.All)
        {
            UpdateModuleMesh(kvp.Key, kvp.Value).FireAndForget();
        }
    }

    public static async Task GenerateAll(
        ConstructData data,
        ConstructModuleBuilder moduleBuilder,
        ConstructVoxelBlockVisualsController visuals,
        ConstructGenerator generator)
    {
        var generationTasks = moduleBuilder.GenerateAllModules(generator);

        await ModuleIntegrationHelper.IntegrateGeneratedModules(
            generationTasks, data, visuals);
        data.Modules.FullyLoaded = true;
    }
}
