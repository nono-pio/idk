namespace PolynomialTheory;

public class RationalUniPolynomial<T> : IEquatable<RationalUniPolynomial<T>> where T : IEquatable<T>
{
    public IRing<T> Ring => Numerator.Ring;
    public UniPolynomial<T> Numerator { get; }
    public UniPolynomial<T> Denominator { get; }
    
    public RationalUniPolynomial(UniPolynomial<T> numerator, UniPolynomial<T>? denominator = null)
    {
        if (denominator is not null && numerator.Ring != denominator.Ring)
            throw new ArgumentException("Les anneaux des deux polynômes ne sont pas les mêmes.");

        if (numerator.Equals(denominator))
        {
            Numerator = UniPolynomial<T>.One(numerator.Ring);
            Denominator = UniPolynomial<T>.One(numerator.Ring);
            return;
        }
        
        denominator ??= UniPolynomial<T>.One(numerator.Ring);
        
        Numerator = numerator;
        Denominator = denominator;
    }
    
    public static RationalUniPolynomial<T> Zero(IRing<T> ring) => new (UniPolynomial<T>.Zero(ring));
    public static RationalUniPolynomial<T> One(IRing<T> ring) => new (UniPolynomial<T>.One(ring));
    
    public static RationalUniPolynomial<T> operator +(RationalUniPolynomial<T> a, RationalUniPolynomial<T> b)
    {
        return new RationalUniPolynomial<T>(a.Numerator * b.Denominator + b.Numerator * a.Denominator, a.Denominator * b.Denominator);
    }
    
    public static RationalUniPolynomial<T> operator -(RationalUniPolynomial<T> a, RationalUniPolynomial<T> b)
    {
        return new RationalUniPolynomial<T>(a.Numerator * b.Denominator - b.Numerator * a.Denominator, a.Denominator * b.Denominator);
    }
    
    public static RationalUniPolynomial<T> operator -(RationalUniPolynomial<T> a)
    {
        return new RationalUniPolynomial<T>(-a.Numerator, a.Denominator);
    }
    
    public static RationalUniPolynomial<T> operator *(RationalUniPolynomial<T> a, RationalUniPolynomial<T> b)
    {
        return new RationalUniPolynomial<T>(a.Numerator * b.Numerator, a.Denominator * b.Denominator);
    }

    public static RationalUniPolynomial<T> operator *(RationalUniPolynomial<T> a, int b)
    {
        return new RationalUniPolynomial<T>(a.Numerator * b, a.Denominator);
    }

    
    public static RationalUniPolynomial<T> operator /(RationalUniPolynomial<T> a, RationalUniPolynomial<T> b)
    {
        return new RationalUniPolynomial<T>(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
    }
    
    public static RationalUniPolynomial<T> operator /(int a, RationalUniPolynomial<T> b)
    {
        return new RationalUniPolynomial<T>(b.Denominator * a, b.Numerator);
    }
    
    public static RationalUniPolynomial<T> operator %(RationalUniPolynomial<T> a, RationalUniPolynomial<T> b)
    {
        return Zero(a.Ring);
    }


    public bool Equals(RationalUniPolynomial<T>? other)
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
        return Equals((RationalUniPolynomial<T>)obj);
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