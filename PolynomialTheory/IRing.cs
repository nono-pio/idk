namespace PolynomialTheory;

public interface IRing<T> where T : IEquatable<T>
{
    T Zero { get; }
    T One { get; }
    
    T Add(T a, T b);
    T Subtract(T a, T b);
    T Negate(T a);

    T Multiply(T a, T b);
    T Multiply(T a, int b);
    
    T Inverse(T a); // return a^-1, or throw exception if a^-1 is not in ring
    T Divide(T a, T b); // return a / b, or throw exception if a / b is not in ring
    T Divide(T a, int b); // return a / b, or throw exception if a / b is not in ring
    
    public bool IsZero(T a);
    public bool IsInversible(T a); // return true if a^-1 is in ring (ex: 1 is inversible in Z, but not 2)
    public bool IsDivisible(T a, T b); // return true if a is divisible by b in ring

    T Pow(T value, int exp)
    {
        if (exp < 0)
            throw new ArgumentException("L'exposant doit être positif.");
        
        T result = One;
        for (int i = 0; i < exp; i++)
            result = Multiply(result, value);
        return result;
    }

    bool IsField();
}
