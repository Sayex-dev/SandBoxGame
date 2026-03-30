using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GameController : Node3D
{
    [Export] public int Seed { get; set; } = 0;
    [Export] public Node3D CameraPosition { get; set; } = new Node3D();
    [Export] public Material ModuleMat { get; set; }
    [Export] public int ModuleSize { get; set; } = 32;
    [Export] public int SimulationDistance { get; set; } = 5;
    [Export] public int RenderDistance { get; set; } = 10;
    [Export] public Viewport.DebugDrawEnum DebugDraw { get; set; } = Viewport.DebugDrawEnum.ClusterDecals;
    [Export] private Godot.Collections.Dictionary<SimulationMode, float> simulationModeDistances = new Godot.Collections.Dictionary<SimulationMode, float>();
    private MeshInstance3D worldMesh;
    [Export] private ConstructWorld blockWorld;

    private Vector3I prevCameraModulePos = Vector3I.MaxValue;

    public override async void _Ready()
    {
        SetPhysicsProcess(false);
        RenderingServer.SetDebugGenerateWireframes(true);

        if (blockWorld == null)
            blockWorld = this.FindChildOfType<ConstructWorld>(1);
        var vp = GetViewport();
        vp.DebugDraw = DebugDraw;

        List<Tuple<SimulationMode, float>> modes = simulationModeDistances
            .OrderBy(kvp => kvp.Value)
            .Select(kvp => new Tuple<SimulationMode, float>(kvp.Key, kvp.Value))
            .ToList();
        blockWorld.Initialize(Seed, ModuleSize, ModuleMat, modes);

        CreateConstructsFromNode();

        SetPhysicsProcess(true);
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3I cameraModulePos = (Vector3I)CameraPosition.Position / ModuleSize;

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
            Construct construct = constructNode.CreateConstruct(blockWorld, ModuleMat, ModuleSize, blockWorld);
            blockWorld.AddConstruct(construct);
        }
    }
}