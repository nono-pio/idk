using System.Numerics;

namespace BigFloats;

/// BigFloat(Mantissa, Exponent) = Mantissa * 2^Exponent
public struct BigFloat : IComparable<BigFloat>, IEquatable<BigFloat>
{
    public readonly BigInteger Mantissa;
    public readonly int Exponent;
    public readonly int Log2Mantissa => (int) Mantissa.GetBitLength();

    public BigFloat(BigInteger mantissa, int exponent)
    {
        Normalize(ref mantissa, ref exponent);
        Mantissa = mantissa;
        Exponent = exponent;
    }
    
    public BigFloat(BigInteger mantissa)
    {
        var exponent = 0;
        Normalize(ref mantissa, ref exponent);
        Mantissa = mantissa;
        Exponent = exponent;
    }
    
    public void Normalize(ref BigInteger mantissa, ref int exponent)
    {
        while (mantissa.IsEven)
        {
            mantissa >>= 1;
            exponent++;
        }
    }
    
    public static BigFloat CreatePow2(int exponent) => new BigFloat(1, exponent);
    
    public static BigFloat FromDouble(double value)
    {
        if (value == 0) return new BigFloat(0, 0);

        var exponent = 0;
        while (value % 1 != 0)
        {
            value *= 2;
            exponent--;
        }

        return new BigFloat(new BigInteger(value), exponent);
    }
    
    // ------------------Math--------------------
    public static BigFloat operator +(BigFloat a, BigFloat b)
    {
        if (a.Exponent > b.Exponent)
        {
            var mantissa = b.Mantissa + (a.Mantissa << (a.Exponent - b.Exponent));
            var exponent = b.Exponent;
            return new BigFloat(mantissa, exponent);
        }
        
        if (a.Exponent < b.Exponent)
        {
            var mantissa = a.Mantissa + (b.Mantissa << (b.Exponent - a.Exponent));
            var exponent = a.Exponent;
            return new BigFloat(mantissa, exponent);
        }
        
        return new BigFloat(a.Mantissa + b.Mantissa, a.Exponent);
    }
    
    public static BigFloat operator -(BigFloat a, BigFloat b)
    {
        if (a.Exponent > b.Exponent)
        {
            var mantissa = (a.Mantissa << (a.Exponent - b.Exponent)) - b.Mantissa;
            var exponent = b.Exponent;
            return new BigFloat(mantissa, exponent);
        }
        
        if (a.Exponent < b.Exponent)
        {
            var mantissa = a.Mantissa - (b.Mantissa << (b.Exponent - a.Exponent));
            var exponent = a.Exponent;
            return new BigFloat(mantissa, exponent);
        }
        
        return new BigFloat(a.Mantissa + b.Mantissa, a.Exponent);
    }
    
    public static BigFloat operator *(BigFloat a, BigFloat b)
    {
        var mantissa = a.Mantissa * b.Mantissa;
        var exponent = a.Exponent + b.Exponent;
        return new BigFloat(mantissa, exponent);
    }
    
    public static BigFloat operator /(BigFloat a, BigFloat b)
    {
        // TODO
        var mantissa = a.Mantissa / b.Mantissa;
        var exponent = a.Exponent - b.Exponent;
        return new BigFloat(mantissa, exponent);
    }
    
    public static BigFloat operator -(BigFloat a) => new BigFloat(-a.Mantissa, a.Exponent);
    
    // -------------------Math Functions---------------------------
    public static BigFloat Abs(BigFloat x) => new BigFloat(BigInteger.Abs(x.Mantissa), x.Exponent);

    public static BigFloat Floor(BigFloat x)
    {
        return new(0);
    }
    
    
    // ----------------Compare------------------------
    public static bool operator ==(BigFloat a, BigFloat b) => a.Mantissa == b.Mantissa && a.Exponent == b.Exponent;
    public static bool operator !=(BigFloat a, BigFloat b) => a.Mantissa != b.Mantissa || a.Exponent != b.Exponent;
    
    public static bool operator <(BigFloat a, BigFloat b)
    {
        var aExp = BigInteger.Log2(a.Mantissa) + a.Exponent;
        var bExp = BigInteger.Log2(b.Mantissa) + b.Exponent;
        
        if (aExp < bExp)
            return true; 
        
        if (aExp > bExp)
            return false;
        
        return a.Mantissa < b.Mantissa;
    }

    public static bool operator >(BigFloat a, BigFloat b)
    {
        var aExp = BigInteger.Log2(BigInteger.Abs(a.Mantissa)) + a.Exponent;
        var bExp = BigInteger.Log2(BigInteger.Abs(b.Mantissa)) + b.Exponent;
        
        if (aExp > bExp)
            return true; 
        
        if (aExp < bExp)
            return false;
        
        return a.Mantissa > b.Mantissa;
    }
    
    public static bool operator <=(BigFloat a, BigFloat b) => a < b || a == b;
    public static bool operator >=(BigFloat a, BigFloat b) => a > b || a == b;

    public override bool Equals(object? obj) => obj is BigFloat bigFloat && this == bigFloat;

    public bool Equals(BigFloat other)
    {
        return Mantissa.Equals(other.Mantissa) && Exponent == other.Exponent;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Mantissa, Exponent);
    }

    public int CompareTo(BigFloat other)
    {
        // TODO : aExp and bExp as int
        var aExp = BigInteger.Log2(BigInteger.Abs(Mantissa)) + Exponent;
        var bExp = BigInteger.Log2(BigInteger.Abs(other.Mantissa)) + other.Exponent;
        var cmpExp = aExp.CompareTo(bExp);
        if (cmpExp != 0) 
            return cmpExp;
        
        return Mantissa.CompareTo(other.Mantissa);
    }
    
    // -------------Printing-----------------
    public string ToDecimalString(int precision = 2)
    {
        BigInteger intValue = Mantissa << Math.Max(0, Exponent);
        BigInteger fractionalValue = Exponent < 0 
            ? Mantissa * BigInteger.Pow(2, Math.Abs(Exponent)) 
            : 0;

        // Partie entière
        string integerPart = intValue.ToString();

        // Partie fractionnaire
        string fractionalPart = "";
        if (fractionalValue > 0)
        {
            // Fraction binaire en base 10
            double fractional = (double)fractionalValue / Math.Pow(2, Math.Abs(Exponent));
            fractionalPart = (fractional % 1).ToString($"F{precision}").Substring(2); // Après le point
        }

        return $"{integerPart}.{fractionalPart}";
    }
    
    public override string ToString()
    {
        var mlog = (int) BigInteger.Log2(BigInteger.Abs(Mantissa));
        var exp = Exponent + mlog;

        if (int.Abs(exp) <= 14)
        {
            return ToDecimalString(precision: 5);
        }

        return $"{Mantissa}*2^{Exponent}";
    }
}