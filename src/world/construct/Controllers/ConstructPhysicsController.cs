using System;
using Godot;

public class ConstructPhysicsController : IUpdate
{
    public event Action<float> OnMassChanged;
    public event Action<Vector3> OnVelocityChanged;
    public event Action<Vector3> OnPhysicsPositionChanged;

    public float Gravity { get; private set; } = 0.1f;


    public float BlockMass { get; private set; }
    public Vector3 Velocity { get; private set; }
    public Vector3 PhysicsPosition { get; private set; }
    public bool IsStatic { get; private set; }

    private ConstructMotionController motionController;
    private ConstructGridTransformController gridTransform;

    public ConstructPhysicsController(ConstructMotionController motionController, ConstructGridTransformController gridTransform)
    {
        this.motionController = motionController;
        this.gridTransform = gridTransform;
    }

    public void Update(double deltaTime)
    {
        if (IsStatic)
            return;

        // Gravity
        Velocity += Vector3.Down * (Gravity * (float)deltaTime);

        // Apply
        PhysicsPosition += Velocity;
        Vector3 div = PhysicsPosition - gridTransform.WorldPos.Value;
        Vector3 absDiv = div.Abs();
        bool couldMove = true;
        if (absDiv.X > 1 || absDiv.Y > 1 || absDiv.Z > 1)
            couldMove = motionController.TryTakeStep(DirectionTools.GetClosestDirection(div));

        if (!couldMove)
        {
            Velocity = Vector3.Zero;
            PhysicsPosition = gridTransform.WorldPos.Value;
        }
    }

    public void SetPosition(WorldGridPos pos)
    {
        PhysicsPosition = (Vector3I)pos;
    }

    public void ApplyForce(Vector3 direction, float force)
    {
        Velocity += direction * (force / BlockMass);
    }

    public void CancleVelocity()
    {
        Velocity = Vector3.Zero;
    }

    public void ChangeWeightBy(float weight)
    {
        BlockMass += weight;
        BlockMass = Math.Max(BlockMass, 0);
    }

    public void AddBlock(Block block)
    {
        BlockDefault blockDefault = BlockStore.Instance.GetBlockDefault(block);
        ChangeWeightBy(blockDefault.Weight);
    }

    public void RemoveBlock(Block block)
    {
        BlockDefault blockDefault = BlockStore.Instance.GetBlockDefault(block);
        ChangeWeightBy(-blockDefault.Weight);
    }
}
