using Godot;

public partial class CosntructCreationSettings : Resource
{
    [Export] private ConstructGeneratorSettings constructGeneratorSettings;
    [Export] private SecondOrderDynamicsSettings sodSettings;
    [Export] private int seed;
    [Export] public bool IsGlobal = false;
}