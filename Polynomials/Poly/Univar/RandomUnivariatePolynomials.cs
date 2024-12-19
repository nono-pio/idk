using System.Numerics;
using Polynomials.Utils;
using UnivariatePolynomialZ64 = Polynomials.Poly.Univar.UnivariatePolynomial<long>;
using UnivariatePolynomialZp64 = Polynomials.Poly.Univar.UnivariatePolynomial<long>;

namespace Polynomials.Poly.Univar;

public static class RandomUnivariatePolynomials
{
    private static readonly int DEFAULT_BOUND = 100;


    public static UnivariatePolynomial<E> RandomPoly<E>(UnivariatePolynomial<E> factory, int degree, Random rnd)
    {
        if (factory.IsZ64())
            return RandomPoly(degree, rnd) as UnivariatePolynomial<E>;
        else if (factory.IsZp64())
            return RandomMonicPoly(degree, (factory.ring as IntegersZp64).modulus, rnd) as UnivariatePolynomial<E>;
            
        var p = RandomPoly(degree, factory.ring, rnd);
        if (factory.IsOverField())
            p = p.Monic()!;
        
        return p;
    }


    public static UnivariatePolynomialZ64 RandomPoly(int degree, Random rnd)
    {
        return RandomPoly(degree, DEFAULT_BOUND, rnd);
    }


    public static UnivariatePolynomialZp64 RandomMonicPoly(int degree, long modulus, Random rnd)
    {
        UnivariatePolynomialZ64 r = RandomPoly(degree, modulus, rnd);
        while (r.data[degree] % modulus == 0)
        {
            r.data[r.degree] = rnd.NextInt64();
        }

        return r.Modulus(modulus, false).Monic();
    }


    public static UnivariatePolynomial<BigInteger> RandomMonicPoly(int degree, BigInteger modulus, Random rnd)
    {
        UnivariatePolynomial<BigInteger> r = RandomPoly(degree, modulus, rnd);
        while ((r.data[degree] % modulus).IsZero)
        {
            r.data[r.degree] = BigIntegerUtils.RandomBigIntBound(modulus, rnd);
        }

        return r.SetRing(new IntegersZp(modulus)).Monic();
    }


    public static UnivariatePolynomial<E> RandomMonicPoly<E>(int degree, Ring<E> ring, Random rnd)
    {
        return RandomPoly(degree, ring, rnd).Monic();
    }


    public static UnivariatePolynomialZ64 RandomPoly(int degree, long bound, Random rnd)
    {
        return new UnivariatePolynomialZ64(Rings.Z64, RandomLongArray(degree, bound, rnd));
    }


    public static UnivariatePolynomial<BigInteger> RandomPoly(int degree, BigInteger bound, Random rnd)
    {
        return UnivariatePolynomial<BigInteger>.CreateUnsafe(Rings.Z, RandomBigArray(degree, bound, rnd));
    }


    public static UnivariatePolynomial<E> RandomPoly<E>(int degree, Ring<E> ring, Random rnd)
    {
        return UnivariatePolynomial<E>.CreateUnsafe(ring, RandomArray(degree, ring, rnd));
    }


    public static UnivariatePolynomial<E> RandomPoly<E>(int degree, Ring<E> ring, Func<Random, E> method, Random rnd)
    {
        return UnivariatePolynomial<E>.CreateUnsafe(ring, RandomArray(degree, ring, method, rnd));
    }


    public static long[] RandomLongArray(int degree, long bound, Random rnd)
    {
        long[] data = new long[degree + 1];
        for (int i = 0; i <= degree; ++i)
        {
            data[i] = rnd.NextInt64(0, bound - 1);
            if (rnd.NextBoolean() && rnd.NextBoolean())
                data[i] = -data[i];
        }

        while (data[degree] == 0)
            data[degree] = rnd.NextInt64(0, bound - 1);
        return data;
    }


    public static BigInteger[] RandomBigArray(int degree, BigInteger bound, Random rnd)
    {
        long lBound = bound.IsLong() ? (long)bound : long.MaxValue;
        BigInteger[] data = new BigInteger[degree + 1];
        for (int i = 0; i <= degree; ++i)
        {
            data[i] = BigIntegerUtils.RandomBigIntBound(bound, rnd);
            if (rnd.NextBoolean() && rnd.NextBoolean())
                data[i] = -data[i];
        }

        while (data[degree].Equals(BigInteger.Zero))
            data[degree] = new BigInteger(rnd.NextInt64(0, lBound));
        return data;
    }


    public static E[] RandomArray<E>(int degree, Ring<E> ring, Random rnd)
    {
        return RandomArray(degree, ring, ring.RandomElement, rnd);
    }


    public static E[] RandomArray<E>(int degree, Ring<E> ring, Func<Random, E> method, Random rnd)
    {
        E[] data = new E[degree + 1];
        for (int i = 0; i <= degree; ++i)
            data[i] = method(rnd);
        while (ring.IsZero(data[degree]))
            data[degree] = method(rnd);
        return data;
    }
}
