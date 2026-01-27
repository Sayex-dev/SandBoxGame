using Godot;

[GlobalClass]
public partial class SecondOrderDynamicsSettings : Resource
{
    [Export] public float f = 0;
    [Export] public float z = 0;
    [Export] public float r = 0;

    public SecondOrderDynamics<Vector3> GetInstance(Vector3 pos)
    {
        return new(f, z, r, pos, new Vector3SodMath());
    }

    public SecondOrderDynamics<float> GetInstance(float start)
    {
        return new(f, z, r, start, new FloatSodMath());
    }
}
