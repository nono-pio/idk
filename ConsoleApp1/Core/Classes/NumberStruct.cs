using System.Runtime.InteropServices;

namespace ConsoleApp1.Core.Classes;

[StructLayout(LayoutKind.Explicit)]
public struct NumberStruct
{

    [FieldOffset(0)]
    public NumberType Type;
    
    // Fraction
    [FieldOffset(4)]
    public long Numerator;
    [FieldOffset(12)]
    public long Denominator;
    
    // Float
    [FieldOffset(4)]
    public double FloatValue;
    
    public static NumberStruct Nan => new NumberStruct();
    public NumberStruct Zero => new NumberStruct(0, 1);
    public NumberStruct One => new NumberStruct(1, 1);

    public bool IsNan => Type == NumberType.Nan;
    public bool IsFraction => Type == NumberType.Fraction;
    public bool IsInt => Type == NumberType.Fraction && Denominator == 1;
    public bool IsFloat => Type == NumberType.Float;
    
    public bool IsZero => (IsFraction && Numerator == 0) || (IsFloat && FloatValue == 0);
    public bool IsOne => (IsFraction && Numerator == 1 && Denominator == 1) || (IsFloat && FloatValue == 1);
    
    public bool IsPositive => (IsFraction && Numerator > 0) || (IsFloat && FloatValue > 0);
    public bool IsNegative => (IsFraction && Numerator < 0) || (IsFloat && FloatValue < 0);
    
    public NumberStruct(long numerator, long denominator = 1)
    {
        if (denominator == 0)
        {
            Type = NumberType.Nan;
            return;
        }

        Type = NumberType.Fraction;

        var gcd = NumberUtils.Gcd(numerator, denominator);

        if (Denominator < 0)
        {
            Numerator = - Numerator / gcd;
            Denominator = - Denominator / gcd;
        }

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

    public double N()
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

    public override string ToString()
    {
        return Type switch
        {
            NumberType.Fraction => Denominator == 1 ? Numerator.ToString() : $"{Numerator}/{Denominator}",
            NumberType.Float => FloatValue.ToString(),
            _ => "NaN"
        };
    
    }
}

public enum NumberType
{
    Fraction,
    Float,
    Nan
}