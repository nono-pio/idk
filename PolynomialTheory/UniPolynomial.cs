namespace PolynomialTheory;

public class UniPolynomial<T> where T : IEquatable<T>
{
    public IRing<T> Ring;
    public T[] Coefficients { get; } // index = degree
    public int Degree => Coefficients.Length - 1;
    
    public UniPolynomial(IRing<T> ring, T[] coefficients)
    {
        if (coefficients.Length == 0)
            throw new ArgumentException("Polynomial must have at least one coefficient");
        
        if (ring.IsZero(coefficients[^1]))
            coefficients = RemoveUnnecessaryCoefficients(ring, coefficients);
        
        Coefficients = coefficients;
    }
    
    public static T[] RemoveUnnecessaryCoefficients(IRing<T> ring, T[] coefficients)
    {
        int i = coefficients.Length - 1;
        while (i >= 0 && ring.IsZero(coefficients[i]))
            i--;
        
        return coefficients[..(i + 1)];
    }
    
}