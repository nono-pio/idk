namespace PolynomialTheory;

public class MultiPolynomialRing<T> : IRing<MultiPolynomial<T>> where T : IEquatable<T>
{

    public IRing<T> Ring;
    public int NVars;

    public MultiPolynomialRing(IRing<T> ring, int nVars)
    {
        Ring = ring;
        NVars = nVars;
    }

    public MultiPolynomial<T> Zero => MultiPolynomial<T>.Zero(Ring, NVars);
    public MultiPolynomial<T> One => MultiPolynomial<T>.One(Ring, NVars);
    public MultiPolynomial<T> Add(MultiPolynomial<T> a, MultiPolynomial<T> b)
    {
        return a + b;
    }

    public MultiPolynomial<T> Subtract(MultiPolynomial<T> a, MultiPolynomial<T> b)
    {
        return a - b;
    }

    public MultiPolynomial<T> Negate(MultiPolynomial<T> a)
    {
        return -a;
    }

    public MultiPolynomial<T> Multiply(MultiPolynomial<T> a, MultiPolynomial<T> b)
    {
        return a * b;
    }

    public MultiPolynomial<T> Multiply(MultiPolynomial<T> a, int b)
    {
        return a * b;
    }

    public MultiPolynomial<T> Inverse(MultiPolynomial<T> a)
    {
        throw new NotImplementedException();
    }

    public MultiPolynomial<T> Divide(MultiPolynomial<T> a, MultiPolynomial<T> b)
    {
        throw new NotImplementedException();
    }

    public bool IsZero(MultiPolynomial<T> a)
    {
        return a.IsZero();
    }

    public bool IsInversible(MultiPolynomial<T> a)
    {
        throw new NotImplementedException();
    }

    public bool IsDivisible(MultiPolynomial<T> a, MultiPolynomial<T> b)
    {
        throw new NotImplementedException();
    }

    public bool IsField()
    {
        return false;
    }

    public MultiPolynomial<T> Divide(MultiPolynomial<T> a, int b)
    {
        return a / b;
    }
}