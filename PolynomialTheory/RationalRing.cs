namespace PolynomialTheory;

public struct Rational : IEquatable<Rational>
{
    public int Numerator { get; }
    public int Denominator { get; }
    
    public static implicit operator Rational(int value) => new Rational(value);
    public Rational(int numerator, int denominator = 1)
    {
        if (denominator == 0)
            throw new ArgumentException("Le dénominateur ne peut pas être nul.");
        
        if (numerator == 0)
        {
            Numerator = 0;
            Denominator = 1;
            return;
        }
        
        (numerator, denominator) = Simplify(numerator, denominator);
        
        Numerator = numerator;
        Denominator = denominator;
    }
    
    public static (int, int) Simplify(int n, int d)
    {
        if (d < 0)
        {
            n = -n;
            d = -d;
        }
        
        int gcd = GCDAlgorithms.GCD(n, d);
        return (n / gcd, d / gcd);
    }
    
    public static Rational operator +(Rational a, Rational b)
    {
        int numerator = a.Numerator * b.Denominator + b.Numerator * a.Denominator;
        int denominator = a.Denominator * b.Denominator;
        
        return new Rational(numerator, denominator);
    }
    
    public static Rational operator -(Rational a, Rational b)
    {
        int numerator = a.Numerator * b.Denominator - b.Numerator * a.Denominator;
        int denominator = a.Denominator * b.Denominator;
        
        return new Rational(numerator, denominator);
    }
    
    public static Rational operator -(Rational a)
    {
        return new Rational(-a.Numerator, a.Denominator);
    }
    
    public static Rational operator *(Rational a, Rational b)
    {
        int numerator = a.Numerator * b.Numerator;
        int denominator = a.Denominator * b.Denominator;
        
        return new Rational(numerator, denominator);
    }
    
    public static Rational operator /(Rational a, Rational b)
    {
        int numerator = a.Numerator * b.Denominator;
        int denominator = a.Denominator * b.Numerator;
        
        return new Rational(numerator, denominator);
    }

    public bool Equals(Rational other)
    {
        return Numerator == other.Numerator && Denominator == other.Denominator; // assert this and other are already simplified
    }

    public override bool Equals(object? obj)
    {
        return obj is Rational other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Numerator, Denominator);
    }
    
    public override string ToString()
    {
        return Denominator == 1 ? Numerator.ToString() : Numerator + "/" + Denominator;
    }
}

public class RationalRing : IRing<Rational>
{
    public Rational Zero => new Rational(0);
    public Rational One => new Rational(1);
    
    public Rational Add(Rational a, Rational b) => a + b;
    public Rational Subtract(Rational a, Rational b) => a - b;

    public Rational Negate(Rational a) => -a;

    public Rational Multiply(Rational a, Rational b) => a * b;
    public Rational Multiply(Rational a, int b) => a * b;

    public Rational Inverse(Rational a) => 1 / a;

    public Rational Divide(Rational a, Rational b) => a / b;
    public bool IsZero(Rational a) => a.Numerator == 0;

    public bool IsInversible(Rational a) => a.Numerator != 0;

    public bool IsDivisible(Rational a, Rational b) => a.Numerator != 0;
    public bool IsField() => true;
}