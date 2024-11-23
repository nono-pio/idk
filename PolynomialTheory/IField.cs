namespace PolynomialTheory;

public interface IField<T> : IRing<T> where T : IEquatable<T>
{
    public static abstract T Divide(T a, T b);
    public static abstract T Inverse(T a);
}