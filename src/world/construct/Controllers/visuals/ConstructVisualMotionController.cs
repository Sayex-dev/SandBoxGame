using System;
using Godot;

public partial class ConstructVisualMotionController : IDisposable, IUpdate
{
    public event Action<Vector3> OnVisualPositionChanged;
    public event Action<Vector3> OnVisualRotationChanged;

    private Vector3 position;
    private Vector3 rotation;

    private ConstructPhysicsController physicsController;
    private ConstructGridTransformController transform;
    private SecondOrderDynamics<Vector3> moveSecondOrderDynamics;
    private SecondOrderDynamics<float> rotationSecondOrderDynamics;

    public Vector3 Position => position;
    public Vector3 Rotation => rotation;

    public ConstructVisualMotionController(
        ConstructPhysicsController physicsController,
        ConstructGridTransformController transform,
        SecondOrderDynamics<Vector3> moveSod,
        SecondOrderDynamics<float> rotSod
    )
    {
        this.physicsController = physicsController;
        this.transform = transform;
        moveSecondOrderDynamics = moveSod;
        rotationSecondOrderDynamics = rotSod;
    }

    public void Update(double delta)
    {
        float dt = (float)delta;

        UpdatePosition(dt);
        UpdateRotation(dt);
    }

    private void UpdatePosition(float delta)
    {
        Vector3 targetPosition = transform.WorldPos.Value;

        if (position.IsEqualApprox(targetPosition))
            return;

        position = moveSecondOrderDynamics.Update(delta, targetPosition);

        OnVisualPositionChanged?.Invoke(position);
    }

    private void UpdateRotation(float delta)
    {
        float currentRot = Mathf.RadToDeg(rotation.Y);
        float targetRot = transform.YRotation;

        if (Mathf.IsEqualApprox(currentRot, targetRot))
            return;

        if (Math.Abs(currentRot - targetRot) > 180)
        {
            currentRot -= (float)Math.CopySign(360, currentRot - targetRot);
            rotationSecondOrderDynamics.SetPrevious(currentRot);
        }

        currentRot = rotationSecondOrderDynamics.Update(delta, targetRot);
        rotation = YRotToVec(currentRot);

        OnVisualRotationChanged?.Invoke(rotation);
    }

    private Vector3 YRotToVec(float yRot)
    {
        return new Vector3(rotation.X, Mathf.DegToRad(yRot), rotation.Z);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

}