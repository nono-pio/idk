using System.Numerics;

namespace Rings.util;

public static class BigIntegerExtension
{
    public static bool IsLong(this BigInteger value)
    {
        return value >= long.MinValue && value <= long.MaxValue;
    }
    
    public static bool IsInt(this BigInteger value)
    {
        return value >= int.MinValue && value <= int.MaxValue;
    }
}