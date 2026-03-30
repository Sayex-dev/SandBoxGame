using Godot;

[GlobalClass]
public partial class ConstructNode : Node3D
{
    [Export] private ConstructCreationSettings settings;

    public Construct CreateConstruct(
        Node parent,
        Material material,
        int moduleSize,
        IWorldQuery collisionQuery,
        SimulationMode initialMode = SimulationMode.FROZEN
    )
    {
        return new Construct(settings, moduleSize, material, collisionQuery, parent, (Vector3I)Position, initialMode);
    }
}