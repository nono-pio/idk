namespace PolynomialTheory;

public class RationalMultiPolynomial<T> : IEquatable<RationalMultiPolynomial<T>> where T : IEquatable<T>
{
    
    public IRing<T> Ring => Numerator.Ring;
    public int NVars => Numerator.NVars;
    public MultiPolynomial<T> Numerator { get; }
    public MultiPolynomial<T> Denominator { get; }
    
    public RationalMultiPolynomial(MultiPolynomial<T> numerator, MultiPolynomial<T>? denominator = null)
    {
        if (denominator is not null && (numerator.Ring != denominator.Ring || numerator.NVars != denominator.NVars))
            throw new ArgumentException("Les anneaux des deux polynômes ne sont pas les mêmes.");

        if (numerator.Equals(denominator))
        {
            Numerator = MultiPolynomial<T>.One(numerator.Ring, numerator.NVars);
            Denominator = MultiPolynomial<T>.One(numerator.Ring, numerator.NVars);
            return;
        }
        
        denominator ??= MultiPolynomial<T>.One(numerator.Ring, numerator.NVars);
        
        Numerator = numerator;
        Denominator = denominator;
    }
    
    public static RationalMultiPolynomial<T> Zero(IRing<T> ring, int nVars) => new (MultiPolynomial<T>.Zero(ring, nVars));
    public static RationalMultiPolynomial<T> One(IRing<T> ring, int nVars) => new (MultiPolynomial<T>.One(ring, nVars));
    
    public static RationalMultiPolynomial<T> operator +(RationalMultiPolynomial<T> a, RationalMultiPolynomial<T> b)
    {
        return new RationalMultiPolynomial<T>(a.Numerator * b.Denominator + b.Numerator * a.Denominator, a.Denominator * b.Denominator);
    }
    
    public static RationalMultiPolynomial<T> operator -(RationalMultiPolynomial<T> a, RationalMultiPolynomial<T> b)
    {
        return new RationalMultiPolynomial<T>(a.Numerator * b.Denominator - b.Numerator * a.Denominator, a.Denominator * b.Denominator);
    }
    
    public static RationalMultiPolynomial<T> operator -(RationalMultiPolynomial<T> a)
    {
        return new RationalMultiPolynomial<T>(-a.Numerator, a.Denominator);
    }
    
    public static RationalMultiPolynomial<T> operator *(RationalMultiPolynomial<T> a, RationalMultiPolynomial<T> b)
    {
        return new RationalMultiPolynomial<T>(a.Numerator * b.Numerator, a.Denominator * b.Denominator);
    }

    public static RationalMultiPolynomial<T> operator *(RationalMultiPolynomial<T> a, int b)
    {
        return new RationalMultiPolynomial<T>(a.Numerator * b, a.Denominator);
    }

    
    public static RationalMultiPolynomial<T> operator /(RationalMultiPolynomial<T> a, RationalMultiPolynomial<T> b)
    {
        return new RationalMultiPolynomial<T>(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
    }
    
    public static RationalMultiPolynomial<T> operator /(int a, RationalMultiPolynomial<T> b)
    {
        return new RationalMultiPolynomial<T>(b.Denominator * a, b.Numerator);
    }
    
    public static RationalMultiPolynomial<T> operator %(RationalMultiPolynomial<T> a, RationalMultiPolynomial<T> b)
    {
        return Zero(a.Ring, a.NVars);
    }


    public bool Equals(RationalMultiPolynomial<T>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Numerator.Equals(other.Numerator) && Denominator.Equals(other.Denominator);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((RationalMultiPolynomial<T>)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Numerator, Denominator);
    }

    public override string ToString()
    {
        return $"({Numerator}) / ({Denominator})";
    }
}