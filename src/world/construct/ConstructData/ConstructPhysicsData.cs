using Godot;

public class ConstructPhysicsData
{
    public float BlockMass { get; set; } = 0;
    
    public Vector3 velocity = Vector3.Zero;
    public Vector3 physicsPosition = Vector3.Zero;
    public bool isStatic;
}