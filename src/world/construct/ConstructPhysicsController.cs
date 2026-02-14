using System;
using Godot;

public class ConstructPhysicsController
{
    public float BlockMass {get; private set;} = 0;

    private Vector3 velocity = Vector3.Zero;

    public ConstructPhysicsController()
    {
        
    }

    public void Update(double deltaTime)
    {
        
    }

    public void ApplyForce(Vector3 direction, float force)
    {
        // f = m * a => a = f / m
        velocity += direction * (force / BlockMass);
    }

    public void ChangeWeightBy(float weight)
    {
        BlockMass += weight;
        BlockMass = Math.Max(BlockMass, 0);
    }
}