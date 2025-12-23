using Godot;

public partial class WorldController : Node3D
{
    [Export] public Node3D FocusPosition { get; set; } = new Node3D();
    [Export] public Material ChunkMat { get; set; }
    [Export] public Godot.Collections.Array<BlockDefault> DefaultBlockStore { get; set; }
    [Export] public WorldGenerator WorldGenerator { get; set; }
    [Export] public Vector3I ChunkSize { get; set; } = new Vector3I(16, 16, 16);
    [Export] public Vector3I RenderDistance { get; set; } = new Vector3I(5, 2, 5);
    [Export] public Viewport.DebugDrawEnum DebugDraw { get; set; } = Viewport.DebugDrawEnum.ClusterDecals;
    private BlockWorld _blockWorld;
    private MeshInstance3D _worldMesh;
    private WorldClock _worldClock;

    private Vector3I _prevCameraChunkPos = Vector3I.MaxValue;

    public override void _Ready()
    {
        RenderingServer.SetDebugGenerateWireframes(true);

        var vp = GetViewport();
        vp.DebugDraw = DebugDraw;

        _worldClock = new WorldClock();
        AddChild(_worldClock);

        var abilityManager = new AbilityManager(_worldClock);

        _blockWorld = new BlockWorld(ChunkSize, WorldGenerator, ChunkMat, abilityManager);
        AddChild(_blockWorld);

        _blockWorld.LoadPosition(FocusPosition.Position, RenderDistance);
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3I cameraChunkPos = (Vector3I)FocusPosition.Position / ChunkSize;

        if (cameraChunkPos != _prevCameraChunkPos)
        {
            _prevCameraChunkPos = cameraChunkPos;
            _blockWorld.LoadPosition(FocusPosition.Position, RenderDistance);
        }
    }

}