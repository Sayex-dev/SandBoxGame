using System;
using Godot;

public class ConstructPhysicsData
{
    public event Action Changed;

    private float blockMass;
    public float BlockMass
    {
        get => blockMass;
        set
        {
            blockMass = Math.Max(value, 0);
            Changed?.Invoke();
        }
    }

    private Vector3 velocity;
    public Vector3 Velocity
    {
        get => velocity;
        set
        {
            velocity = value;
            Changed?.Invoke();
        }
    }

    private Vector3 physicsPosition;
    public Vector3 PhysicsPosition
    {
        get => physicsPosition;
        set
        {
            physicsPosition = value;
            Changed?.Invoke();
        }
    }

    public bool IsStatic;
}
