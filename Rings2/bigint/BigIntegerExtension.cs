using System.Numerics;

namespace Cc.Redberry.Rings.Bigint;

public static class BigIntegerExtension
{
    public static BigInteger Abs(this BigInteger a) => BigInteger.Abs(a);
    public static BigInteger Gcd(this BigInteger a, BigInteger b) => BigInteger.GreatestCommonDivisor(a, b);
    public static BigInteger Multiply(this BigInteger a, BigInteger b) => BigInteger.Multiply(a, b);
    public static BigInteger Add(this BigInteger a, BigInteger b) => BigInteger.Add(a, b);
    public static BigInteger Subtract(this BigInteger a, BigInteger b) => BigInteger.Subtract(a, b);
    public static BigInteger Divide(this BigInteger a, BigInteger b) => BigInteger.Divide(a, b);
    public static BigInteger Remainder(this BigInteger a, BigInteger b) => BigInteger.Remainder(a, b);
    public static BigInteger Mod(this BigInteger a, BigInteger b) => a % b;
    public static BigInteger Pow(this BigInteger a, int b) => BigInteger.Pow(a, b);
    public static bool TestBit(this BigInteger a, int b) => throw new NotImplementedException(); // TODO
    public static BigInteger ShiftLeft(this BigInteger a, int b) => a << b;
    public static BigInteger ShiftRight(this BigInteger a, int b) => a >> b;
    public static int Signum(this BigInteger a) => a.Sign;
    public static BigInteger Negate(this BigInteger a) => -a;
    public static int BitCount(this BigInteger a) => throw new NotImplementedException(); // TODO
    public static int BitLength(this BigInteger a) => (int)a.GetBitLength();
    public static BigInteger Decrement(this BigInteger a) => a - 1;
    public static BigInteger Increment(this BigInteger a) => a + 1;
    public static BigInteger Square(this BigInteger a) => a * a;
    public static BigInteger ModInverse(this BigInteger a, BigInteger mod) => throw new NotImplementedException(); // TODO
    public static BigInteger DivideExact(this BigInteger a, BigInteger b) => BigInteger.Divide(a, b);
    public static BigInteger[] DivideAndRemainder(this BigInteger a, BigInteger b)
    {
        var (q, r) = BigInteger.DivRem(a, b);
        return [q, r];
    }
    public static bool PrimeToCertainty(this BigInteger a, int b) => throw new NotImplementedException(); // TODO
    public static bool IsLong(this BigInteger a) => throw new NotImplementedException(); // TODO
    public static long LongValueExact(this BigInteger a) => (long)a;
    public static long LongValue(this BigInteger a) => (long)a; // TODO : chack diff between LongValue and LongValueExact
    
    
    /*Other*/
    public static bool StartsWith(this string input, string value, int offset)
    {
        throw new NotImplementedException(); // TODO
    }
}