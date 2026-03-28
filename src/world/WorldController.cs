using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class WorldController : Node3D
{
    [Export] public int Seed { get; set; } = 0;
    [Export] public Node3D FocusPosition { get; set; } = new Node3D();
    [Export] public Material ModuleMat { get; set; }
    [Export] public int ModuleSize { get; set; } = 32;
    [Export] public int SimulationDistance { get; set; } = 5;
    [Export] public int RenderDistance { get; set; } = 10;
    [Export] public Viewport.DebugDrawEnum DebugDraw { get; set; } = Viewport.DebugDrawEnum.ClusterDecals;
    private ConstructWorld blockWorld;
    private MeshInstance3D worldMesh;

    private Vector3I prevCameraModulePos = Vector3I.MaxValue;

    public override async void _Ready()
    {
        SetPhysicsProcess(false);
        RenderingServer.SetDebugGenerateWireframes(true);

        var vp = GetViewport();
        vp.DebugDraw = DebugDraw;

        blockWorld = new ConstructWorld(Seed, ModuleSize, ModuleMat);
        AddChild(blockWorld);

        await GatherConstuctChildren();

        await blockWorld.UpdateConstructModuleLoading(new((Vector3I)FocusPosition.Position), RenderDistance, SimulationDistance);
        SetPhysicsProcess(true);
    }

    public override async void _PhysicsProcess(double delta)
    {
        Vector3I cameraModulePos = (Vector3I)FocusPosition.Position / ModuleSize;

        if (cameraModulePos != prevCameraModulePos)
        {
            prevCameraModulePos = cameraModulePos;
            await blockWorld.UpdateConstructModuleLoading(new((Vector3I)FocusPosition.Position), RenderDistance, SimulationDistance);
        }
    }

    public async Task GatherConstuctChildren()
    {
        foreach (var construct in GetChildren().OfType<Construct>())
        {
            RemoveChild(construct);
            construct.Initialize(
                    ModuleSize,
                    ModuleMat,
                    blockWorld,
                    SimulationState.ACTIVE
                );
            if (construct.IsGlobal)
                blockWorld.AddGlobalConstruct(construct);
            else
                await blockWorld.AddConstruct(construct);
        }
    }
}