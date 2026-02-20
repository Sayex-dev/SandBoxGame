using System;
using Godot;

public class ConstructPhysicsController
{
    public float Gravity = 0.1f;
    public float BlockMass { get; private set; } = 0;

    private readonly ConstructData data;
    private Vector3 velocity = Vector3.Zero;
    private Vector3 physicsPosition = Vector3.Zero;
    private bool isStatic;

    public ConstructPhysicsController(ConstructData data, Vector3 initPos, bool isStatic)
    {
        this.data = data;
        physicsPosition = initPos;
        this.isStatic = isStatic;
    }

    public void Update(double deltaTime)
    {
        if (isStatic)
            return;

        // Gravity
        velocity += Vector3.Down * (Gravity * (float)deltaTime);

        // Apply
        physicsPosition += velocity;
        Vector3 absDif = (physicsPosition - (Vector3I)data.Transform.WorldPos).Abs();
        if (absDif.X > 1 || absDif.Y > 1 || absDif.Z > 1)
            data.Transform.MoveTo((Vector3I)physicsPosition);
    }

    public void SetPosition(WorldGridPos pos)
    {
        physicsPosition = (Vector3I)pos;
    }

    public void ApplyForce(Vector3 direction, float force)
    {
        velocity += direction * (force / BlockMass);
    }

    public void CancleVelocity()
    {
        velocity = Vector3.Zero;
    }

    public void ChangeWeightBy(float weight)
    {
        BlockMass += weight;
        BlockMass = Math.Max(BlockMass, 0);
    }
}
