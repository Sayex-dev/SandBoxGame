using System.Linq;
using Godot;
using System.Diagnostics;


public partial class GameController : Node3D
{
    [Export] public Node3D CameraPosition { get; set; } = new Node3D();
    [Export] public Material ModuleMat { get; set; }
    [Export] public Viewport.DebugDrawEnum DebugDraw { get; set; } = Viewport.DebugDrawEnum.ClusterDecals;

    [Export] private ConstructWorld blockWorld;

    private MeshInstance3D worldMesh;
    private Vector3I prevCameraModulePos = Vector3I.MaxValue;
    private int moduleSize;

    public override async void _Ready()
    {
        moduleSize = GameSettings.Instance.ModuleSize;

        SetPhysicsProcess(false);
        RenderingServer.SetDebugGenerateWireframes(true);

        if (blockWorld == null)
            blockWorld = this.FindChildOfType<ConstructWorld>();
        var vp = GetViewport();
        vp.DebugDraw = DebugDraw;

        blockWorld.Initialize(ModuleMat);

        CreateConstructsFromNode();

        SetPhysicsProcess(true);
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3I cameraModulePos = (Vector3I)CameraPosition.Position / moduleSize;

        if (cameraModulePos != prevCameraModulePos)
        {
            prevCameraModulePos = cameraModulePos;
            blockWorld.CameraMoved(cameraModulePos);
        }
    }

    public void CreateConstructsFromNode()
    {
        foreach (var constructNode in GetChildren().OfType<ConstructNode>())
        {
            RemoveChild(constructNode);
            Construct construct = constructNode.CreateConstruct(blockWorld, ModuleMat, blockWorld, (Vector3I)CameraPosition.Position);
            blockWorld.AddConstruct(construct);
        }
    }
}