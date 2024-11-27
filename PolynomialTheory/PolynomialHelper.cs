namespace PolynomialTheory;

public static class PolynomialHelper
{
    public static UniPolynomial<T> UniPolynomial<T>(IRing<T> ring, T[] coefficients)
        where T : IEquatable<T> 
        => new UniPolynomial<T>(ring, coefficients);
    
    public static RationalUniPolynomial<T> RationalPolynomial<T>(UniPolynomial<T> numerator, UniPolynomial<T>? denominator = null)
        where T : IEquatable<T> 
        => new RationalUniPolynomial<T>(numerator, denominator);
    
    public static RationalMultiPolynomial<T> RationalPolynomial<T>(MultiPolynomial<T> numerator, MultiPolynomial<T>? denominator = null)
        where T : IEquatable<T> 
        => new RationalMultiPolynomial<T>(numerator, denominator);

}