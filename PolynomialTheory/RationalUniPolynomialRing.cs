namespace PolynomialTheory;

public class RationalUniPolynomialRing<T> : IRing<RationalUniPolynomial<T>> where T : IEquatable<T>
{
    
    public IRing<T> Ring { get; }

    public RationalUniPolynomialRing(IRing<T> ring)
    {
        Ring = ring;
    }
    
    public RationalUniPolynomial<T> Zero => RationalUniPolynomial<T>.Zero(Ring);
    public RationalUniPolynomial<T> One => RationalUniPolynomial<T>.One(Ring);
    public RationalUniPolynomial<T> Add(RationalUniPolynomial<T> a, RationalUniPolynomial<T> b)
    {
        return a + b;
    }

    public RationalUniPolynomial<T> Subtract(RationalUniPolynomial<T> a, RationalUniPolynomial<T> b)
    {
        return a - b;
    }

    public RationalUniPolynomial<T> Negate(RationalUniPolynomial<T> a)
    {
        return -a;
    }

    public RationalUniPolynomial<T> Multiply(RationalUniPolynomial<T> a, RationalUniPolynomial<T> b)
    {
        return a * b;
    }

    public RationalUniPolynomial<T> Multiply(RationalUniPolynomial<T> a, int b)
    {
        return a * b;
    }

    public RationalUniPolynomial<T> Inverse(RationalUniPolynomial<T> a)
    {
        return 1 / a;
    }

    public RationalUniPolynomial<T> Divide(RationalUniPolynomial<T> a, RationalUniPolynomial<T> b)
    {
        return a / b;
    }

    public bool IsZero(RationalUniPolynomial<T> a) => a.Numerator.IsZero();

    public bool IsInversible(RationalUniPolynomial<T> a)
    {
        return true;
    }

    public bool IsDivisible(RationalUniPolynomial<T> a, RationalUniPolynomial<T> b)
    {
        return true;
    }

    public bool IsField()
    {
        return true;
    }

    public RationalUniPolynomial<T> Divide(RationalUniPolynomial<T> a, int b)
    {
        return a / b;
    }
}