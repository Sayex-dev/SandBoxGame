using System;
using Godot;

public class ConstructPhysicsController
{
    public float Gravity = 0.1f;

    private readonly ConstructData data;
    private readonly ConstructMotionController motionController;
    private Vector3 velocity = Vector3.Zero;
    private Vector3 physicsPosition = Vector3.Zero;
    private bool isStatic;

    public ConstructPhysicsController(ConstructData data, ConstructMotionController motionController, Vector3 initPos, bool isStatic)
    {
        this.data = data;
        this.motionController = motionController;
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
        Vector3 div = physicsPosition - data.Transform.WorldPos.Value;
        Vector3 absDiv = div.Abs();
        bool couldMove = true;
        if (absDiv.X > 1 || absDiv.Y > 1 || absDiv.Z > 1)
            couldMove = motionController.TryTakeStep(DirectionTools.GetClosestDirection(div));

        if (!couldMove)
        {
            velocity = Vector3.Zero;
            physicsPosition = data.Transform.WorldPos.Value;
        }
    }

    public void SetPosition(WorldGridPos pos)
    {
        physicsPosition = (Vector3I)pos;
    }

    public void ApplyForce(Vector3 direction, float force)
    {
        velocity += direction * (force / data.PhysicsData.BlockMass);
    }

    public void CancleVelocity()
    {
        velocity = Vector3.Zero;
    }

    public void ChangeWeightBy(float weight)
    {
        data.PhysicsData.BlockMass += weight;
        data.PhysicsData.BlockMass = Math.Max(data.PhysicsData.BlockMass, 0);
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
