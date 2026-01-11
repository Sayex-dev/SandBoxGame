public sealed class FloatSodMath : ISecondOrderMath<float>
{
    public float Zero => 0;

    public float Add(float a, float b) => a + b;
    public float Sub(float a, float b) => a - b;
    public float Mul(float a, float b) => a * b;
    public float Div(float a, float b) => a / b;
}