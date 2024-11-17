using System.Numerics;
using Sdcb.Arithmetic.Mpfr;

namespace BigFloats;

public struct BigFloat : IComparable<BigFloat>, IEquatable<BigFloat>, IDisposable, IFormattable, IComparable
{
    private static MpfrFloat Float;

    public const int DEFAULT_PRECISION = 10;

    public static implicit operator BigFloat(float value) => new (MpfrFloat.From(value));
    public static implicit operator BigFloat(double value) => new (MpfrFloat.From(value));
    public static implicit operator BigFloat(int value) => new (MpfrFloat.From(value));
    public static implicit operator BigFloat(long value) => new (MpfrFloat.From(value));
    
    public BigFloat(int precision = DEFAULT_PRECISION)
    {
        Float = new MpfrFloat(PrecisionToBitsPrecision(precision));
    }

    private BigFloat(MpfrFloat @float)
    {
        Float = @float;
    }

    private int PrecisionToBitsPrecision(int precision)
    {
        return (int) (3.321928094887362 * precision);
        
    }
    
    public float ToFloat() => Float.ToFloat();
    public double ToDouble() => Float.ToDouble();
    public int ToInt() => Float.ToInt32();
    public bool CanBeInt() => Float.FitsInt32();


}