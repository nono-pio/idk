using System.Numerics;
using Polynomials.Primes;
using static Polynomials.Poly.Univar.Conversions64bit;
using UnivariatePolynomialZp64 = Polynomials.Poly.Univar.UnivariatePolynomial<long>;

namespace Polynomials.Poly.Univar;

public static class IrreduciblePolynomials
{
    public static bool IrreducibleQ<E>(UnivariatePolynomial<E> poly)
    {
        if (poly.IsOverFiniteField())
            return FiniteFieldIrreducibleQ(poly);
        else
            return !poly.IsMonomial() && UnivariateFactorization.Factor(poly).IsTrivial();
    }


    public static bool FiniteFieldIrreducibleQ<E>(UnivariatePolynomial<E> poly) 
    {
        return FiniteFieldIrreducibleBenOr(poly);
    }


    public static bool FiniteFieldIrreducibleViaModularComposition<E>(UnivariatePolynomial<E> poly)
    {
        Util.EnsureOverFiniteField(poly);
        if (poly.IsMonomial())
            return false;
        if (CanConvertToZp64(poly))
            return FiniteFieldIrreducibleViaModularComposition(AsOverZp64(poly));
        if (poly.Degree() <= 1)
            return true;
        poly = poly.Clone().Monic();
        var invMod = UnivariateDivision.FastDivisionPreConditioning(poly);

        // x^q
        var xq = UnivariatePolynomialArithmetic.CreateMonomialMod(poly.CoefficientRingCardinality().Value, poly, invMod);

        // cached powers x^(q^i) for different i
        var cache = new Dictionary<int, UnivariatePolynomial<E>>();
        int degree = poly.Degree();

        // x^(q^n)
        var xqn = Composition(xq, degree, poly, invMod, cache);
        var xMonomial = poly.CreateMonomial(1);
        if (!xqn.Equals(xMonomial))

            // x^(q^n) != x
            return false;

        // primes factors of poly.degree
        int[] primes = Utils.Utils.GetSortedDistinct(SmallPrimes.PrimeFactors(degree));
        foreach (int prime in primes)
        {
            // x^(q^(n/p))
            var xqp = Composition(xq, degree / prime, poly, invMod, cache);
            if (!UnivariateGCD.PolynomialGCD(xqp.Subtract(xMonomial), poly).IsOne())
                return false;
        }

        return true;
    }

    /* fives xq^exponent using repeated compositions */
    static UnivariatePolynomial<E> Composition<E>(UnivariatePolynomial<E> xq, int exponent, UnivariatePolynomial<E> poly, UnivariateDivision.InverseModMonomial<E> invMod,
        Dictionary<int, UnivariatePolynomial<E>> cache)
    {
        var cached = cache[exponent];
        if (cached != null)
            return cached;
        var result = xq.CreateMonomial(1);
        var k2p = xq;
        int rExp = 0, kExp = 1;
        for (;;)
        {
            if ((exponent & 1) != 0)
                cache.Add(rExp += kExp, result = ModularComposition.Composition(k2p, result, poly, invMod));
            exponent = exponent >> 1;
            if (exponent == 0)
            {
                cache.Add(rExp, result);
                return result;
            }

            cache.Add(kExp *= 2, k2p = ModularComposition.Composition(k2p, k2p, poly, invMod));
        }
    }


    public static bool FiniteFieldIrreducibleBenOr<E>(UnivariatePolynomial<E> poly)
    {
        Util.EnsureOverFiniteField(poly);
        if (poly.IsMonomial())
            return false;
        if (CanConvertToZp64(poly))
            return FiniteFieldIrreducibleBenOr(AsOverZp64(poly));
        if (!poly.IsMonic())
            poly = poly.Clone().Monic();
        var invMod = UnivariateDivision.FastDivisionPreConditioning(poly);

        // x
        var xMonomial = poly.CreateMonomial(1);

        // x^q
        var xq = xMonomial.Clone();

        // primes factors of poly.degree
        for (int i = 1; i <= (poly.Degree() / 2); ++i)
        {
            // x^(q^i)
            xq = UnivariatePolynomialArithmetic.PolyPowMod(xq, poly.CoefficientRingCardinality().Value, poly, invMod, false);
            if (!UnivariateGCD.PolynomialGCD(xq.Clone().Subtract(xMonomial), poly).IsOne())
                return false;
        }

        return true;
    }


    public static UnivariatePolynomialZp64 RandomIrreduciblePolynomial(long modulus, int degree, Random rnd)
    {
        return RandomIrreduciblePolynomial(UnivariatePolynomialZp64.Zero(Rings.Zp64(modulus)), degree, rnd);
    }


    public static UnivariatePolynomial<E> RandomIrreduciblePolynomial<E>(Ring<E> ring, int degree, Random rnd)
    {
        UnivariatePolynomial<E> poly;
        do
        {
            poly = RandomUnivariatePolynomials.RandomPoly(degree, ring, rnd);
            if (ring.IsField())
                poly.Monic();
        } while (!IrreducibleQ(poly));

        return poly;
    }


    public static UnivariatePolynomial<BigInteger> RandomIrreduciblePolynomialOverZ(int degree, Random rnd)
    {
        // some prime number
        long modulus = SmallPrimes.NextPrime(1 << 25);
        return RandomIrreduciblePolynomial(modulus, degree, rnd).ToBigPoly().SetRing(Rings.Z);
    }


    public static UnivariatePolynomial<E> RandomIrreduciblePolynomial<E>(UnivariatePolynomial<E> factory, int degree, Random rnd)
    {
        UnivariatePolynomial<E> poly;
        do
        {
            poly = RandomUnivariatePolynomials.RandomPoly(factory, degree, rnd);
        } while (!IrreducibleQ(poly));

        return poly;
    }
}
