using System.Diagnostics;
using Godot;

public partial class WorldController : Node3D
{
    [Export] public int Seed { get; set; } = 0;
    [Export] public Node3D FocusPosition { get; set; } = new Node3D();
    [Export] public Material ModuleMat { get; set; }
    [Export] public ConstructGenerator WorldGenerator { get; set; }
    [Export] public ConstructGenerator TestGenerator { get; set; }
    [Export] public SecondOrderDynamicsSettings sodSettings { get; set; }
    [Export] public BlockStore GameBlockStore { get; set; }
    [Export] public int ModuleSize { get; set; } = 32;
    [Export] public Vector3I RenderDistance { get; set; } = new Vector3I(5, 5, 5);
    [Export] public Viewport.DebugDrawEnum DebugDraw { get; set; } = Viewport.DebugDrawEnum.ClusterDecals;
    private BlockWorld blockWorld;
    private MeshInstance3D worldMesh;
    private WorldClock worldClock;

    private Vector3I prevCameraModulePos = Vector3I.MaxValue;

    public override void _Ready()
    {
        RenderingServer.SetDebugGenerateWireframes(true);

        GameBlockStore.SetBlockIds();
        WorldGenerator.Init(ModuleSize);
        TestGenerator.Init(ModuleSize);

        var vp = GetViewport();
        vp.DebugDraw = DebugDraw;

        worldClock = new WorldClock();
        AddChild(worldClock);

        var abilityManager = new AbilityManager(worldClock);

        blockWorld = new BlockWorld(Seed, ModuleSize, GameBlockStore, WorldGenerator, ModuleMat, abilityManager);
        AddChild(blockWorld);


        moveConstruct = new Construct(ModuleSize, TestGenerator, new Vector3I(1, 5, 0), GameBlockStore, ModuleMat, sodSettings);
        blockWorld.AddGlobalConstruct(new Construct(ModuleSize, WorldGenerator, Vector3I.Zero, GameBlockStore, ModuleMat, sodSettings));
        blockWorld.AddConstruct(moveConstruct);

        blockWorld.UpdateConstructLoading((Vector3I)FocusPosition.Position, RenderDistance);
    }

    private double tempMoveTime = 0;
    private Construct moveConstruct;

    public override void _PhysicsProcess(double delta)
    {
        tempMoveTime += delta;
        if (tempMoveTime > 1)
        {
            moveConstruct.MoveTo(moveConstruct.WorldOffset + Vector3I.Forward);
            tempMoveTime = 0;
        }


        Vector3I cameraModulePos = (Vector3I)FocusPosition.Position / ModuleSize;

        if (cameraModulePos != prevCameraModulePos)
        {
            prevCameraModulePos = cameraModulePos;
            blockWorld.UpdateConstructLoading((Vector3I)FocusPosition.Position, RenderDistance);
        }
    }
}