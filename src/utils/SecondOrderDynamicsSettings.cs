using Godot;
using System;
using System.IO;
using System.Linq;

[GlobalClass]
public partial class SecondOrderDynamicsSettings : Resource
{
    [Export] public float f = 0;
    [Export] public float z = 0;
    [Export] public float r = 0;

    public SecondOrderDynamics GetInstance(Vector3 worldPos)
    {
        return new SecondOrderDynamics(f, z, r, worldPos);
    }
}

public class SecondOrderDynamics
{
    private Vector3 xp; // previous input
    private Vector3 y, yd; // state variables
    private float k1, k2, k3; // dynamics constants
    public SecondOrderDynamics(float f, float z, float r, Vector3 x0)
    {
        if (f == 0)
        {
            throw new DivideByZeroException("f cannot be zero.");
        }

        // compute constants
        float PI = (float)Math.PI;
        k1 = z / (PI * f);
        k2 = 1 / (2 * PI * f * (2 * PI * f));
        k3 = r * z / (2 * PI * f);
        // initialize variables
        xp = x0;
        y = x0;
        yd = Vector3.Zero;
    }

    public Vector3 Update(float T, Vector3 x, Nullable<Vector3> xd = null)
    {
        if (xd == null)
        { // estimate velocity
            xd = (x - xp) / T;
            xp = x;
        }

        float k2_stable = Math.Max(k2, Math.Max(T * T / 2 + T * k1 / 2, T * k1)); // clamp k2 to guarantee stability without jitter
        y = y + T * yd; // integrate position by velocity
        yd = yd + T * (x + k3 * xd.Value - y - k1 * yd) / k2_stable; // integrate velocity by acceleration
        return y;
    }
}
