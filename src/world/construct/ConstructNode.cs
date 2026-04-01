using Godot;

[GlobalClass]
public partial class ConstructNode : Node3D
{
    [Export] private ConstructCreationSettings settings;

    public Construct CreateConstruct(
        IWorldQuery collisionQuery
    )
    {
        return Construct.GetInitializedConstruct(settings, collisionQuery, (Vector3I)Position);
    }
}