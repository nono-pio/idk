using System.Numerics;
using Polynomials.Poly;
using Polynomials.Poly.Multivar;
using Polynomials.Poly.Univar;

namespace Polynomials;

public static class Rings
{
    public static Random privateRandom = new Random(DateTime.Now.Nanosecond);
    public static Integers Z = new Integers();
    public static Rationals<BigInteger> Q = new Rationals<BigInteger>(Z);
    public static Integers64 Z64 = new Integers64();
    public static IntegersZp64 Zp64(long modulus) => new IntegersZp64(modulus);
    public static IntegersZp Zp(BigInteger modulus) => new IntegersZp(modulus);
    public static UnivariateRing<BigInteger> UnivariateRingZ => UnivariateRing(Z);
    public static UnivariateRing<BigInteger> UnivariateRingZp(BigInteger modulus) 
        => UnivariateRing(new UnivariatePolynomial<BigInteger>(Zp(modulus), [0]));
    public static UnivariateRing<long> UnivariateRingZp64(long modulus) 
    
        => UnivariateRing(new UnivariatePolynomial<long>(Zp64(modulus), [0]));
    public static Ring<Poly> PolynomialRing<Poly>(Poly unit) where Poly : Polynomial<Poly>
    {
        return unit.AsRing();
    }
    
    public static UnivariateRing<E> UnivariateRing<E>(UnivariatePolynomial<E> factory)
    {
        return new UnivariateRing<E>(factory);
    }
    public static UnivariateRing<E> UnivariateRing<E>(Ring<E> ring) => UnivariateRing(new UnivariatePolynomial<E>(ring, [ring.GetZero()]));

    
    public static MultivariateRing<E> MultivariateRing<E>(MultivariatePolynomial<E> factory)
    {
        return new MultivariateRing<E>(factory);
    }
}