using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class WorldController : Node3D
{
    [Export] public int Seed { get; set; } = 0;
    [Export] public Node3D FocusPosition { get; set; } = new Node3D();
    [Export] public Material ModuleMat { get; set; }
    [Export] public BlockStore BlockStore { get; set; }
    [Export] public int ModuleSize { get; set; } = 32;
    [Export] public Vector3I RenderDistance { get; set; } = new Vector3I(5, 5, 5);
    [Export] public Viewport.DebugDrawEnum DebugDraw { get; set; } = Viewport.DebugDrawEnum.ClusterDecals;
    private BlockWorld blockWorld;
    private MeshInstance3D worldMesh;
    private WorldClock worldClock;

    private Vector3I prevCameraModulePos = Vector3I.MaxValue;

    public override async void _Ready()
    {
        SetPhysicsProcess(false);
        RenderingServer.SetDebugGenerateWireframes(true);

        BlockStore.SetBlockIds();

        var vp = GetViewport();
        vp.DebugDraw = DebugDraw;

        worldClock = new WorldClock();
        AddChild(worldClock);

        var abilityManager = new AbilityManager(worldClock);

        blockWorld = new BlockWorld(Seed, ModuleSize, BlockStore, ModuleMat, abilityManager);
        AddChild(blockWorld);

        GatherConstuctChildren();

        await blockWorld.UpdateConstructLoading(new((Vector3I)FocusPosition.Position), RenderDistance);
        SetPhysicsProcess(true);
    }

    public override async void _PhysicsProcess(double delta)
    {
        Vector3I cameraModulePos = (Vector3I)FocusPosition.Position / ModuleSize;

        if (cameraModulePos != prevCameraModulePos)
        {
            prevCameraModulePos = cameraModulePos;
            await blockWorld.UpdateConstructLoading(new((Vector3I)FocusPosition.Position), RenderDistance);
        }
    }

    public void GatherConstuctChildren()
    {
        foreach (var construct in GetChildren().OfType<Construct>())
        {
            RemoveChild(construct);
            construct.Initialize(
                new ConstructVisualsController(ModuleSize, construct),
                new ConstructMotionController(),
                BlockStore, ModuleMat);
            if (construct.IsGlobal)
                blockWorld.AddGlobalConstruct(construct);
            else
                blockWorld.AddConstruct(construct);
        }
    }
}