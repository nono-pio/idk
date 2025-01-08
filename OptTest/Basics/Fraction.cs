using OptTest.Utils;

namespace OptTest.Basics;

public class Fraction<T> : Field<Fraction<T>> where T : IntegralDomain<T>
{
    public T Numerator { get; }
    public T Denominator { get; }

    public Fraction(T numerator)
    {
        Numerator = numerator;
        Denominator = T.OneFactory;
    }
    
    public Fraction(T numerator, T denominator)
    {
        Numerator = numerator;
        Denominator = denominator;
    }

    public static Boolean operator ==(Fraction<T> a, Fraction<T> b)
    {
        return a.Numerator == b.Numerator && a.Denominator == b.Denominator;
    }

    public static bool operator !=(Fraction<T> a, Fraction<T> b)
    {
        return !(a == b);
    }

    public static Fraction<T> operator +(Fraction<T> a, Fraction<T> b)
    {
        return new Fraction<T>(a.Numerator * b.Denominator + a.Denominator * b.Numerator,
            a.Denominator * b.Denominator);
    }

    public static Fraction<T> ZeroFactory => new(T.ZeroFactory, T.OneFactory);
    public static MayFail<Fraction<T>> SubtractIfCan(Fraction<T> a, Fraction<T> b)
    {
        throw new NotImplementedException();
    }

    public static Fraction<T> operator -(Fraction<T> a)
    {
        return new Fraction<T>(-a.Numerator, a.Denominator);
    }

    public static Fraction<T> operator *(Fraction<T> a, Fraction<T> b)
    {
        return new Fraction<T>(a.Numerator * b.Numerator, a.Denominator * b.Numerator);
    }

    public static Fraction<T> OneFactory => new Fraction<T>(T.OneFactory, T.OneFactory);
    public static uint CharacteristicFactory => T.CharacteristicFactory;
    public static MayFail<Fraction<T>> Exquo(Fraction<T> a, Fraction<T> b)
    {
        throw new NotImplementedException();
    }

    public static (Fraction<T> Unit, Fraction<T> Canonical, Fraction<T> Associate) UnitNormal(Fraction<T> a)
    {
        throw new NotImplementedException();
    }

    public static Boolean IsUnit(Fraction<T> a)
    {
        throw new NotImplementedException();
    }

    public static Fraction<T> Gcd(Fraction<T> a, Fraction<T> b)
    {
        return OneFactory;
    }

    public uint EuclideanSize(Fraction<T> a)
    {
        throw new NotImplementedException();
    }

    public (Fraction<T> Quotient, Fraction<T> Remainder) Divide(Fraction<T> a, Fraction<T> b)
    {
        throw new NotImplementedException();
    }

    public (Fraction<T> Coef1, Fraction<T> Coef2, Fraction<T> Generator) ExtendedEuclidean(Fraction<T> a, Fraction<T> b)
    {
        throw new NotImplementedException();
    }

    public MayFail<(Fraction<T> Coef1, Fraction<T> Coef2)> ExtendedEuclidean(Fraction<T> a, Fraction<T> b, Fraction<T> c)
    {
        throw new NotImplementedException();
    }

    public static Fraction<T> Inv(Fraction<T> a)
    {
        return new Fraction<T>(a.Denominator, a.Numerator);
    }

    public static Fraction<T> operator /(Fraction<T> a, Fraction<T> b)
    {
        return new Fraction<T>(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
    }

    public static Fraction<T> operator -(Fraction<T> a, Fraction<T> b)
    {
        return new Fraction<T>(a.Numerator * b.Denominator - a.Denominator * b.Numerator,
            a.Denominator * b.Denominator);
    }

    public override string ToString()
    {
        return $"{Numerator} / {Denominator}";
    }
}