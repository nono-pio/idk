namespace PolynomialTheory;

public static class PolynomialHelper
{
    public static UniPolynomial<T> UniPolynomial<T>(IRing<T> ring, T[] coefficients)
        where T : IEquatable<T> 
        => new UniPolynomial<T>(ring, coefficients);
    
    public static MultiPolynomial<T> MultiPolynomial<T>(IRing<T> ring, (T, int[])[] multinomials)
        where T : IEquatable<T> 
        => new MultiPolynomial<T>(ring, multinomials.Select(m => new Multinomial<T>(m.Item1, m.Item2)).ToArray());
    
    public static MultiPolynomial<T> MultiPolynomial<T>(IRing<T> ring, (T, int[]) multinomial)
        where T : IEquatable<T> 
        => new MultiPolynomial<T>(ring, [new Multinomial<T>(multinomial.Item1, multinomial.Item2)]);
    
    public static MultiPolynomial<T> MultiPolynomial<T>(IRing<T> ring, T cste, int nVars)
        where T : IEquatable<T> 
        => new MultiPolynomial<T>(ring, [ new Multinomial<T>(cste, new int[nVars]) ]);
    
    public static RationalUniPolynomial<T> RationalPolynomial<T>(UniPolynomial<T> numerator, UniPolynomial<T>? denominator = null)
        where T : IEquatable<T> 
        => new RationalUniPolynomial<T>(numerator, denominator);
    
    public static RationalMultiPolynomial<T> RationalPolynomial<T>(MultiPolynomial<T> numerator, MultiPolynomial<T>? denominator = null)
        where T : IEquatable<T> 
        => new RationalMultiPolynomial<T>(numerator, denominator);

    public static UniPolynomial<T> Gcd<T>(UniPolynomial<T> a, UniPolynomial<T> b) 
        where T : IEquatable<T> =>
        PolynomialTheory.UniPolynomial<T>.GCD(a.Ring, a, b);

}