using System;
using Godot;

public class ConstructPhysicsController
{
    public float Gravity = 0.1f;

    private readonly ConstructData data;
    private readonly ConstructMotionController motionController;

    public ConstructPhysicsController(ConstructData data, ConstructMotionController motionController)
    {
        this.data = data;
        this.motionController = motionController;
    }

    public void Update(double deltaTime)
    {
        if (data.PhysicsData.IsStatic)
            return;

        // Gravity
        data.PhysicsData.Velocity += Vector3.Down * (Gravity * (float)deltaTime);

        // Apply
        data.PhysicsData.PhysicsPosition += data.PhysicsData.Velocity;
        Vector3 div = data.PhysicsData.PhysicsPosition - data.GridTransform.WorldPos.Value;
        Vector3 absDiv = div.Abs();
        bool couldMove = true;
        if (absDiv.X > 1 || absDiv.Y > 1 || absDiv.Z > 1)
            couldMove = motionController.TryTakeStep(DirectionTools.GetClosestDirection(div));

        if (!couldMove)
        {
            data.PhysicsData.Velocity = Vector3.Zero;
            data.PhysicsData.PhysicsPosition = data.GridTransform.WorldPos.Value;
        }
    }

    public void SetPosition(WorldGridPos pos)
    {
        data.PhysicsData.PhysicsPosition = (Vector3I)pos;
    }

    public void ApplyForce(Vector3 direction, float force)
    {
        data.PhysicsData.Velocity += direction * (force / data.PhysicsData.BlockMass);
    }

    public void CancleVelocity()
    {
        data.PhysicsData.Velocity = Vector3.Zero;
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
