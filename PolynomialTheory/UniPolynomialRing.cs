namespace PolynomialTheory;

public class UniPolynomialRing<T> : IRing<UniPolynomial<T>> where T : IEquatable<T>
{

    public IRing<T> Ring { get; }
    public UniPolynomialRing(IRing<T> ring)
    {
        Ring = ring;
    }
    
    public UniPolynomial<T> Zero => UniPolynomial<T>.Zero(Ring);
    public UniPolynomial<T> One => UniPolynomial<T>.One(Ring);
    public UniPolynomial<T> Add(UniPolynomial<T> a, UniPolynomial<T> b)
    {
        return a + b;
    }

    public UniPolynomial<T> Subtract(UniPolynomial<T> a, UniPolynomial<T> b)
    {
        return a - b;
    }

    public UniPolynomial<T> Negate(UniPolynomial<T> a)
    {
        return -a;
    }

    public UniPolynomial<T> Multiply(UniPolynomial<T> a, UniPolynomial<T> b)
    {
        return a * b;
    }

    public UniPolynomial<T> Multiply(UniPolynomial<T> a, int b)
    {
        return a * b;
    }

    public UniPolynomial<T> Inverse(UniPolynomial<T> a)
    {
        throw new NotImplementedException();
    }

    public UniPolynomial<T> Divide(UniPolynomial<T> a, UniPolynomial<T> b)
    {
        return a / b;
    }

    public bool IsZero(UniPolynomial<T> a)
    {
        return a.IsZero();
    }

    public bool IsInversible(UniPolynomial<T> a)
    {
        throw new NotImplementedException();
    }

    public bool IsDivisible(UniPolynomial<T> a, UniPolynomial<T> b)
    {
        try
        {
            Divide(a, b);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public bool IsField()
    {
        return false;
    }
}