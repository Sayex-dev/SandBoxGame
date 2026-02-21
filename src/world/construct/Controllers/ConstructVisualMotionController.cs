using System;
using Godot;

public partial class ConstructVisualMotionController
{
    public Vector3 Position { get; private set; }
    public Vector3 Rotation { get; private set; }

    private ConstructData data;
    private SecondOrderDynamics<Vector3> moveSecondOrderDynamics;
    private SecondOrderDynamics<float> rotationSecondOrderDynamics;

    public ConstructVisualMotionController(
        ConstructData data,
        SecondOrderDynamics<Vector3> moveSod,
        SecondOrderDynamics<float> rotSod
    )
    {
        this.data = data;
        Position = (Vector3I)data.Transform.WorldPos;
        Rotation = YRotToVec(data.Transform.YRotation);

        moveSecondOrderDynamics = moveSod;
        rotationSecondOrderDynamics = rotSod;
    }

    public void Update(double delta)
    {
        float currentRot = Mathf.RadToDeg(Rotation.Y);
        float degTargetYRotation = data.Transform.YRotation;
        WorldGridPos targetWorldPos = data.Transform.WorldPos;

        if (Position != targetWorldPos.Value)
            Position = moveSecondOrderDynamics.Update((float)delta, targetWorldPos.Value);

        if (currentRot != degTargetYRotation)
        {
            if (Math.Abs(currentRot - degTargetYRotation) > 180)
            {
                currentRot -= (float)Math.CopySign(360, currentRot - degTargetYRotation);
                rotationSecondOrderDynamics.SetPrevious(currentRot);
            }
            currentRot = rotationSecondOrderDynamics.Update((float)delta, degTargetYRotation);
        }

        Rotation = YRotToVec(currentRot);
    }

    private Vector3 YRotToVec(float yRot)
    {
        return new Vector3(Rotation.X, Mathf.DegToRad(yRot), Rotation.Z);
    }
}