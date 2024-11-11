using System.Globalization;
using System.Runtime.InteropServices;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Classes;

[StructLayout(LayoutKind.Explicit)]
public struct NumberStruct
{

    enum NumberType
    {
        Fraction,
        Float,
        Nan
    }

    [FieldOffset(0)]
    private NumberType Type;
    
    // Fraction
    [FieldOffset(4)]
    public long Numerator;
    [FieldOffset(12)]
    public long Denominator;
    
    // Float
    [FieldOffset(4)]
    public double FloatValue;
    
    public static implicit operator NumberStruct(int value) => new NumberStruct(value);
    public static implicit operator NumberStruct(long value) => new NumberStruct(value);
    public static implicit operator NumberStruct(float value) => new NumberStruct(value);
    public static implicit operator NumberStruct(double value) => new NumberStruct(value);
    
    public static NumberStruct Nan => new NumberStruct{ Type = NumberType.Nan };

    public bool IsNan => Type == NumberType.Nan;
    public bool IsFraction => Type == NumberType.Fraction;
    public bool IsInt => Type == NumberType.Fraction && Denominator == 1;
    public bool IsFloat => Type == NumberType.Float;
    
    public bool IsZero => (IsFraction && Numerator == 0) || (IsFloat && FloatValue == 0);
    public bool IsOne => (IsFraction && Numerator == 1 && Denominator == 1) || (IsFloat && FloatValue.Equals(1));
    
    public bool IsPositive => (IsFraction && Numerator >= 0) || (IsFloat && FloatValue >= 0);
    public bool IsNegative => (IsFraction && Numerator <= 0) || (IsFloat && FloatValue <= 0);
    
    public bool IsInfinity => IsFloat && double.IsPositiveInfinity(FloatValue);
    public bool IsNegativeInfinity => IsFloat && double.IsNegativeInfinity(FloatValue);
    
    public bool Is(int n) => IsFraction && Numerator == n && Denominator == 1;
    
    public NumberStruct(long numerator, long denominator = 1)
    {
        if (denominator == 0)
        {
            Type = NumberType.Nan;
            return;
        }

        Type = NumberType.Fraction;
        
        if (denominator < 0)
        {
            numerator = -numerator;
            denominator = -denominator;
        }

        var gcd = NumberUtils.Gcd(numerator, denominator);

        Numerator = numerator / gcd;
        Denominator = denominator / gcd;
    }
    
    public NumberStruct(double floatValue)
    {
        if (double.IsNaN(floatValue))
        {
            Type = NumberType.Nan;
            return;
        }
        
        Type = NumberType.Float;
        FloatValue = floatValue;
    }
    
    public NumberStruct()
    {
        Type = NumberType.Nan;
    }

    public Expr Expr() => new Number(this);

    public readonly double N()
    {
        return Type switch
        {
            NumberType.Fraction => (double) Numerator / Denominator,
            NumberType.Float => FloatValue,
            _ => double.NaN
        };
    }
    
    #region Operators
    
    public static NumberStruct operator +(NumberStruct a, NumberStruct b)
    {
        if (a.IsNan || b.IsNan)
        {
            return Nan;
        }
        
        if (a.IsFraction && b.IsFraction)
        {
            return new NumberStruct(a.Numerator * b.Denominator + b.Numerator * a.Denominator, a.Denominator * b.Denominator);
        }
        
        
        return new NumberStruct(a.N() + b.N());
    }
    
    public static NumberStruct operator +(NumberStruct a, long b) => a + new NumberStruct(b);
    public static NumberStruct operator +(long a, NumberStruct b) => new NumberStruct(a) + b;
    public static NumberStruct operator +(NumberStruct a, double b) => a + new NumberStruct(b);
    public static NumberStruct operator +(double a, NumberStruct b) => new NumberStruct(a) + b;
    
    public static NumberStruct operator -(NumberStruct a, NumberStruct b)
    {
        if (a.IsNan || b.IsNan)
        {
            return Nan;
        }
        
        if (a.IsFraction && b.IsFraction)
        {
            return new NumberStruct(a.Numerator * b.Denominator - b.Numerator * a.Denominator, a.Denominator * b.Denominator);
        }
        
        
        return new NumberStruct(a.N() - b.N());
    }
    
    public static NumberStruct operator -(NumberStruct a, long b) => a - new NumberStruct(b);
    public static NumberStruct operator -(long a, NumberStruct b) => new NumberStruct(a) - b;
    public static NumberStruct operator -(NumberStruct a, double b) => a - new NumberStruct(b);
    public static NumberStruct operator -(double a, NumberStruct b) => new NumberStruct(a) - b;
    
    public static NumberStruct operator *(NumberStruct a, NumberStruct b)
    {
        if (a.IsNan || b.IsNan)
        {
            return Nan;
        }
        
        if (a.IsFraction && b.IsFraction)
        {
            return new NumberStruct(a.Numerator * b.Numerator, a.Denominator * b.Denominator);
        }
        
        
        return new NumberStruct(a.N() * b.N());
    }
    
    public static NumberStruct operator *(NumberStruct a, long b) => a * new NumberStruct(b);
    public static NumberStruct operator *(long a, NumberStruct b) => new NumberStruct(a) * b;
    public static NumberStruct operator *(NumberStruct a, double b) => a * new NumberStruct(b);
    public static NumberStruct operator *(double a, NumberStruct b) => new NumberStruct(a) * b;
    
    public static NumberStruct operator /(NumberStruct a, NumberStruct b)
    {
        if (a.IsNan || b.IsNan)
        {
            return Nan;
        }
        
        if (a.IsFraction && b.IsFraction)
        {
            return new NumberStruct(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
        }
        
        
        return new NumberStruct(a.N() / b.N());
    }
    
    public static NumberStruct operator /(NumberStruct a, long b) => a / new NumberStruct(b);
    public static NumberStruct operator /(long a, NumberStruct b) => new NumberStruct(a) / b;
    public static NumberStruct operator /(NumberStruct a, double b) => a / new NumberStruct(b);
    public static NumberStruct operator /(double a, NumberStruct b) => new NumberStruct(a) / b;
    
    public static NumberStruct operator -(NumberStruct a)
    {
        return a.Type switch
        {
            NumberType.Fraction => new NumberStruct(-a.Numerator, a.Denominator),
            NumberType.Float => new NumberStruct(-a.FloatValue),
            _ => Nan
        };
    }
    
    #endregion
    
    public NumberStruct Abs()
    {
        return Type switch
        {
            NumberType.Fraction => new NumberStruct(Math.Abs(Numerator), Denominator),
            NumberType.Float => new NumberStruct(Math.Abs(FloatValue)),
            _ => Nan
        };
    }

    // x^n = a * sqrt[n](b)
    // return (a, b)
    public static (long, long) Sqrt(long x, long n)
    {

        if (n < 0)
            throw new ArgumentException("n must be positive");
        
        if (n == 0)
            return (1, 1);

        if (n == 1)
            return (x, 0);

        int a = 1;
        int b = 1;

        // x = p1^i1 * p2^i2 * ... * pn^in
        foreach (var (p, i) in NumberUtils.AsFactorExp(x))
        {
            var q = i / n;
            var r = i % n;
            
            // p^(q*n + r)
            // sqrt[n](p^(q*n + r)) = p^q * sqrt[n](p^r)
            
            a *= (int) Math.Pow(p, q);
            b *= (int) Math.Pow(p, r);
        }

        return (a, b);
    }
    
    /* Compare */

    public int CompareTo(NumberStruct other)
    {
        if (this.IsNan || other.IsNan)
            return this.IsNan.CompareTo(other.IsNan);
        
        if (this.IsFraction && other.IsFraction)
            return (Numerator * other.Denominator).CompareTo(other.Numerator * Denominator);
        
        if (this.IsFloat && other.IsFloat)
            return FloatValue.CompareTo(other.FloatValue);
        
        return N().CompareTo(other.N());
    }
    
    public bool Equals(NumberStruct other)
    {
        return CompareTo(other) == 0;
    }

    public override bool Equals(object? obj)
    {
        return obj is NumberStruct other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Type, Numerator, Denominator, FloatValue);
    }
    
    public static bool operator ==(NumberStruct a, NumberStruct b) => a.CompareTo(b) == 0;
    public static bool operator !=(NumberStruct a, NumberStruct b) => a.CompareTo(b) != 0;
    public static bool operator <(NumberStruct a, NumberStruct b) => a.CompareTo(b) < 0;
    public static bool operator <=(NumberStruct a, NumberStruct b) => a.CompareTo(b) <= 0;
    public static bool operator >(NumberStruct a, NumberStruct b) => a.CompareTo(b) > 0;
    public static bool operator >=(NumberStruct a, NumberStruct b) => a.CompareTo(b) >= 0;

    /* To String */
    public override string ToString()
    {
        return Type switch
        {
            NumberType.Fraction => Denominator == 1 ? Numerator.ToString() : $"{Numerator}/{Denominator}",
            NumberType.Float => FloatValue switch
            {
                double.PositiveInfinity => "oo",
                double.NegativeInfinity => "-oo",
                _ => FloatValue.ToString(CultureInfo.CurrentCulture)
            },
            _ => "NaN"
        };
    
    }
    
    public string ToLatex()
    {
        return Type switch
        {
            NumberType.Fraction => Denominator == 1 ? 
                Numerator.ToString() : LatexUtils.Fraction(Numerator.ToString(), Denominator.ToString()),
            NumberType.Float => FloatValue switch
            {
                double.PositiveInfinity => @"\infty",
                double.NegativeInfinity => @"-\infty",
                _ => FloatValue.ToString(CultureInfo.InvariantCulture)
            },
            _ => "NaN"
        };
    
    }

    public int ToInt()
    {
        if (Type != NumberType.Fraction || Denominator != 1)
            throw new InvalidCastException("Cannot convert to int");
        return (int) Numerator;
    }
}