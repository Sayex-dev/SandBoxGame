using System;
using Godot;

public partial class ConstructMotionController
{
    public Vector3 Position { get; private set; }
    public Vector3 Rotation { get; private set; }


    private SecondOrderDynamics<Vector3> moveSecondOrderDynamics;
    private SecondOrderDynamics<float> rotationSecondOrderDynamics;

    public ConstructMotionController(
        SecondOrderDynamics<Vector3> moveSod,
        SecondOrderDynamics<float> rotSod,
        Vector3 initPosition = default,
        Vector3 initRotation = default
    )
    {
        Position = initPosition;
        Rotation = initRotation;

        moveSecondOrderDynamics = moveSod;
        rotationSecondOrderDynamics = rotSod;
    }

    public void Update(double delta, WorldGridPos targetWorldPos, float degTargetYRotation)
    {
        float currentRot = Mathf.RadToDeg(Rotation.Y);

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

        Rotation = new Vector3(Rotation.X, Mathf.DegToRad(degTargetYRotation), Rotation.Z);
    }
}