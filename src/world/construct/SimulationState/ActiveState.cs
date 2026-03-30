using Godot;
using System.Threading.Tasks;
public class ActiveState : SimulationState
{
    private ConstructPhysicsController physics;
    private ConstructVisualsController visuals;
    private ConstructVisualMotionController visualMotion;
    private ConstructModuleBuilder moduleBuilder;
    private ConstructMotionController motionController;

    private IWorldQuery collisionQuery;
    private SecondOrderDynamicsSettings rotSodSettings;
    private SecondOrderDynamicsSettings moveSodSettings;
    private int moduleSize;

    public ActiveState(
        ConstructCore core,
        IWorldQuery collisionQuery,
        SecondOrderDynamicsSettings rotSodSettings,
        SecondOrderDynamicsSettings moveSodSettings,
        int moduleSize) : base(core)
    {
        this.collisionQuery = collisionQuery;
        this.rotSodSettings = rotSodSettings;
        this.moveSodSettings = moveSodSettings;
        this.moduleSize = moduleSize;
    }

    public override void Enter()
    {
        motionController = new ConstructMotionController(core.Data, collisionQuery);
        physics = new ConstructPhysicsController(core.Data, motionController);

        var rotSod = rotSodSettings.GetInstance(0);
        var moveSod = moveSodSettings.GetInstance(core.SceneNode.Position);
        visualMotion = new ConstructVisualMotionController(core.Data, moveSod, rotSod);

        visuals = new ConstructVisualsController(moduleSize, core.SceneNode);
        moduleBuilder = new ConstructModuleBuilder();

        RebuildAllModules();
    }

    public override void Exit()
    {
        // Cleanup state-specific resources
        visuals?.Dispose();
        visuals = null;
        physics = null;
        visualMotion = null;
        moduleBuilder = null;
        motionController = null;
    }

    public override void Update(double delta)
    {
        physics.Update(delta);
        visualMotion?.Update(delta);
    }

    public override Vector3 GetPosition() => visualMotion.Position;
    public override Vector3 GetRotation() => visualMotion.Rotation;

    public override void AddBlock(Block block, ConstructGridPos pos)
    {
        core.Blocks.SetBlock(pos, block);
        UpdateModuleMesh(pos.ToModuleLocation(moduleSize)).FireAndForget();
    }

    private async Task UpdateModuleMesh(ModuleLocation moduleLoc)
    {
        if (!core.Data.Modules.TryGet(moduleLoc, out Module module))
            return;

        var context = new ModuleMeshGenerateContext(module, moduleLoc, core.Data.ModuleMaterial);
        var mesh = await moduleBuilder.GenerateModuleMesh(context);
        visuals.RemoveModule(moduleLoc);
        visuals.AddModule(moduleLoc, mesh);
    }

    private void RebuildAllModules()
    {
        foreach (var module in core.Data.Modules.GetAll())
        {
            UpdateModuleMesh(module.Location).FireAndForget();
        }
    }
}