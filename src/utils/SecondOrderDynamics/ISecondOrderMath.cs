public interface ISecondOrderMath<T>
{
    T Zero { get; }

    T Add(T a, T b);
    T Sub(T a, T b);
    T Mul(T a, float b);
    T Div(T a, float b);
}