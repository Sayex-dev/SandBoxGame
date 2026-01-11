using Godot;

public sealed class Vector3SodMath : ISecondOrderMath<Vector3>
{
    public Vector3 Zero => Vector3.Zero;

    public Vector3 Add(Vector3 a, Vector3 b) => a + b;
    public Vector3 Sub(Vector3 a, Vector3 b) => a - b;
    public Vector3 Mul(Vector3 a, float b) => a * b;
    public Vector3 Div(Vector3 a, float b) => a / b;
}