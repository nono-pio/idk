namespace PolynomialTheory;


public class RationalMultiPolynomialRing<T> : IRing<RationalMultiPolynomial<T>> where T : IEquatable<T>
{
    
    public IRing<T> Ring { get; }
    public int NVars;
    
    public RationalMultiPolynomialRing(IRing<T> ring, int nVars)
    {
        Ring = ring;
        NVars = nVars;
    }
    
    public RationalMultiPolynomial<T> Zero => RationalMultiPolynomial<T>.Zero(Ring, NVars);
    public RationalMultiPolynomial<T> One => RationalMultiPolynomial<T>.One(Ring, NVars);
    public RationalMultiPolynomial<T> Add(RationalMultiPolynomial<T> a, RationalMultiPolynomial<T> b)
    {
        return a + b;
    }

    public RationalMultiPolynomial<T> Subtract(RationalMultiPolynomial<T> a, RationalMultiPolynomial<T> b)
    {
        return a - b;
    }

    public RationalMultiPolynomial<T> Negate(RationalMultiPolynomial<T> a)
    {
        return -a;
    }

    public RationalMultiPolynomial<T> Multiply(RationalMultiPolynomial<T> a, RationalMultiPolynomial<T> b)
    {
        return a * b;
    }

    public RationalMultiPolynomial<T> Multiply(RationalMultiPolynomial<T> a, int b)
    {
        return a * b;
    }

    public RationalMultiPolynomial<T> Inverse(RationalMultiPolynomial<T> a)
    {
        return 1 / a;
    }

    public RationalMultiPolynomial<T> Divide(RationalMultiPolynomial<T> a, RationalMultiPolynomial<T> b)
    {
        return a / b;
    }

    public bool IsZero(RationalMultiPolynomial<T> a) => a.Numerator.IsZero();

    public bool IsInversible(RationalMultiPolynomial<T> a)
    {
        return true;
    }

    public bool IsDivisible(RationalMultiPolynomial<T> a, RationalMultiPolynomial<T> b)
    {
        return true;
    }

    public bool IsField()
    {
        return true;
    }
    
    
}