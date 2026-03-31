using Godot;

[GlobalClass]
public partial class ConstructNode : Node3D
{
    [Export] private ConstructCreationSettings settings;

    public Construct CreateConstruct(
        Node3D parent,
        IWorldQuery collisionQuery,
        WorldGridPos loadPos
    )
    {
        return new Construct(settings, collisionQuery, parent, (Vector3I)Position, loadPos);
    }
}