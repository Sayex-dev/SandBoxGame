using Godot;

[GlobalClass]
public partial class ConstructCreationSettings : Resource
{
    [Export] public ConstructGeneratorSettings ConstructGeneratorSettings { get; private set; }
    [Export] public SecondOrderDynamicsSettings MoveSodSettings { get; private set; }
    [Export] public SecondOrderDynamicsSettings RotSodSettings { get; private set; }

    [Export] public bool IsGlobal { get; private set; } = false;
}