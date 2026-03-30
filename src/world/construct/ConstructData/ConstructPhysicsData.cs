using Godot;

public class ConstructPhysicsData
{
    public float BlockMass { get; set; } = 0;

    public Vector3 Velocity = Vector3.Zero;
    public Vector3 PhysicsPosition = Vector3.Zero;
    public bool IsStatic;
}