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
    private ConstructMotionController motionController;

    private IWorldQuery collisionQuery;
    private SecondOrderDynamicsSettings rotSodSettings;
    private SecondOrderDynamicsSettings moveSodSettings;
    private ConstructGenerator generator;
    private Node3D constructNode;
    private int moduleSize;
    private Vector3I prevModulePos;
    private bool ownsVisuals;

    private Action<WorldGridPos> updateLoading;

    public ActiveState(
        ConstructCore core,
        IWorldQuery collisionQuery,
        SecondOrderDynamicsSettings rotSodSettings,
        SecondOrderDynamicsSettings moveSodSettings,
        ConstructGenerator generator,
        Node3D constructNode,
        Action<WorldGridPos> updateLoading,
        ConstructVisualsController visuals = null,
        ConstructModelBlockController modelBlocks = null
    ) : base(core, visuals, modelBlocks)
    {
        this.collisionQuery = collisionQuery;
        this.rotSodSettings = rotSodSettings;
        this.moveSodSettings = moveSodSettings;
        this.generator = generator;
        this.constructNode = constructNode;
        this.updateLoading = updateLoading;

        moduleSize = GameSettings.Instance.ModuleSize;
        prevModulePos = Vector3I.Zero;
    }

    public override void Enter()
    {
        motionController = new ConstructMotionController(core.Data, collisionQuery);
        physics = new ConstructPhysicsController(core.Data, motionController);

        var rotSod = rotSodSettings.GetInstance(0);
        var moveSod = moveSodSettings.GetInstance(core.Data.PhysicsData.PhysicsPosition);
        visualMotion = new ConstructVisualMotionController(core.Data, moveSod, rotSod);

        // Create visuals only if not provided by parent (first activation)
        if (visuals == null)
        {
            visuals = new ConstructVisualsController(core.Data.Modules, moduleSize);
            constructNode.AddChild(visuals);
            ownsVisuals = true;
        }

        if (modelBlocks == null)
        {
            modelBlocks = new ConstructModelBlockController(constructNode, core.Data);
        }

        moduleBuilder = new ConstructModuleBuilder();

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
        // Only remove visuals if we created them
        if (ownsVisuals && visuals != null)
        {
            constructNode.RemoveChild(visuals);
            visuals.Dispose();
            visuals = null;
        }

        // Only dispose modelBlocks if we created them
        if (!ownsVisuals)
        {
            // Controllers were passed in — don't dispose, they survive
            modelBlocks = null;
        }
        else
        {
            modelBlocks?.Dispose();
            modelBlocks = null;
        }

        physics = null;
        visualMotion = null;
        moduleBuilder = null;
        motionController = null;
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
            updateLoading.Invoke(newModulePos);
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
        ConstructVisualsController visuals,
        ConstructGenerator generator)
    {
        var generationTasks = moduleBuilder.GenerateAllModules(generator);

        await ModuleIntegrationHelper.IntegrateGeneratedModules(
            generationTasks, data, visuals);
        data.Modules.FullyLoaded = true;
    }
}
