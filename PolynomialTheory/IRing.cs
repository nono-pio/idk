namespace PolynomialTheory;

public interface IRing<T> where T : IEquatable<T>
{
    T Add(T a, T b);
    T Subtract(T a, T b);
    T Multiply(T a, T b);
    T Negate(T a);
    T Zero { get; }
    T One { get; }
    public bool IsZero(T a) => a.Equals(Zero);
}
