using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GameSettings : Node
{
    public static GameSettings Instance;

    // Generation Settings
    [Export] public int Seed { get; set; } = 0;
    [Export] public Material ModuleMat { get; set; }

    // Construct Settings
    [Export] public int ModuleSize { get; private set; } = 32;

    // Simulation Settings
    [Export] public int SimulationDistance { get; private set; } = 5;
    [Export] public int RenderDistance { get; private set; } = 10;
    [Export] private Godot.Collections.Dictionary<SimulationMode, float> simulationModeDistances = [];

    public List<Tuple<SimulationMode, float>> SimulationModeDistances;

    public override void _EnterTree()
    {
        if (Instance == null)
            Instance = this;
    }

    public override void _Ready()
    {
        SimulationModeDistances = simulationModeDistances
            .OrderBy(kvp => kvp.Value)
            .Select(kvp => new Tuple<SimulationMode, float>(kvp.Key, kvp.Value))
            .ToList();

    }
}