using System;
using Godot;

public partial class ConstructVisualMotionController
{
    public Vector3 Position { get; private set; }
    public Vector3 Rotation { get; private set; }

    private ConstructData data;
    private SecondOrderDynamics<Vector3> moveSecondOrderDynamics;
    private SecondOrderDynamics<float> rotationSecondOrderDynamics;

    private Vector3 positionTarget;
    private float rotationTarget;

    public ConstructVisualMotionController(
        ConstructData data,
        SecondOrderDynamics<Vector3> moveSod,
        SecondOrderDynamics<float> rotSod
    )
    {
        this.data = data;
        Position = (Vector3I)data.GridTransform.WorldPos;
        Rotation = YRotToVec(data.GridTransform.YRotation);

        positionTarget = (Vector3I)data.GridTransform.WorldPos;
        rotationTarget = data.GridTransform.YRotation;

        moveSecondOrderDynamics = moveSod;
        rotationSecondOrderDynamics = rotSod;

        data.GridTransform.Changed += OnGridTransformChanged;
    }

    public void Update(double delta)
    {
        Position = moveSecondOrderDynamics.Update((float)delta, positionTarget);

        float currentRot = Mathf.RadToDeg(Rotation.Y);
        if (Math.Abs(currentRot - rotationTarget) > 180)
        {
            currentRot -= (float)Math.CopySign(360, currentRot - rotationTarget);
            rotationSecondOrderDynamics.SetPrevious(currentRot);
        }
        currentRot = rotationSecondOrderDynamics.Update((float)delta, rotationTarget);

        Rotation = YRotToVec(currentRot);
    }

    private void OnGridTransformChanged()
    {
        positionTarget = data.GridTransform.WorldPos.Value;
        rotationTarget = data.GridTransform.YRotation;
    }

    private Vector3 YRotToVec(float yRot)
    {
        return new Vector3(Rotation.X, Mathf.DegToRad(yRot), Rotation.Z);
    }
}
