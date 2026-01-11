using System;
using Godot;

public class SecondOrderDynamics<T>
{
    private readonly ISecondOrderMath<T> m;

    private T xp;      // previous input
    private T y, yd;   // state variables

    private float k1, k2, k3;

    public SecondOrderDynamics(
        float f,
        float z,
        float r,
        T x0,
        ISecondOrderMath<T> m
    )
    {
        if (f == 0)
            throw new DivideByZeroException("f cannot be zero.");

        this.m = m;
        float PI = (float)Math.PI;
        k1 = z / (PI * f);
        k2 = 1 / (2 * PI * f * (2 * PI * f));
        k3 = r * z / (2 * PI * f);

        xp = x0;
        y = x0;
        yd = m.Zero;
    }

    public static SecondOrderDynamics<float> Get(float f, float z, float r, float x0)
    {
        return new SecondOrderDynamics<float>(f, z, r, x0, new FloatSodMath());
    }

    public static SecondOrderDynamics<Vector3> Get(float f, float z, float r, Vector3 x0)
    {
        return new SecondOrderDynamics<Vector3>(f, z, r, x0, new Vector3SodMath());
    }

    public void SetPrevious(T prev)
    {
        xp = prev;
        y = prev;
    }

    public T Update(float T, T x, T? xd = default)
    {
        if (xd == null)
        {
            xd = m.Div(m.Sub(x, xp), T);
            xp = x;
        }

        float k2Stable = Math.Max(
            k2,
            Math.Max(T * T / 2 + T * k1 / 2, T * k1)
        );

        y = m.Add(y, m.Mul(yd, T));

        var accel =
            m.Sub(
                m.Sub(
                    m.Add(x, m.Mul(xd, k3)),
                    y
                ),
                m.Mul(yd, k1)
            );

        yd = m.Add(
            yd,
            m.Mul(m.Div(accel, k2Stable), T)
        );

        return y;
    }
}