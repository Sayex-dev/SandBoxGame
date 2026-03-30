using Godot;

public partial class CosntructCreationSettings : Resource
{
    [Export] public ConstructGeneratorSettings constructGeneratorSettings { get; private set; }
    [Export] public SecondOrderDynamicsSettings sodSettings { get; private set; }
    [Export] public SecondOrderDynamicsSettings rotSodSettings { get; private set; }

    [Export] public int seed { get; private set; }
    [Export] public bool IsGlobal { get; private set; } = false;
}