using System.Numerics;

namespace Polynomials;

public static class Rings
{
    public static Random privateRandom = new Random(DateTime.Now.Nanosecond);
    public static Integers Z = new Integers();
    public static Integers64 Z64 = new Integers64();
    public static IntegersZp64 Zp64(long modulus) => new IntegersZp64(modulus);
    public static IntegersZp Zp(BigInteger modulus) => new IntegersZp(modulus);

    public static Ring<Poly> PolynomialRing<Poly>(Poly unit)
    {
        throw new NotImplementedException();
    }
}