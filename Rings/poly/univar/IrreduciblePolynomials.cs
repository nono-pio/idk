using System.Diagnostics;
using System.Numerics;
using Rings.primes;

namespace Rings.poly.univar;

public static class IrreduciblePolynomials
{
    public static bool irreducibleQ<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
    {
        if (poly.isOverFiniteField())
            return finiteFieldIrreducibleQ(poly);
        else
            return !poly.isMonomial() && UnivariateFactorization.Factor(poly).isTrivial();
    }


    public static bool finiteFieldIrreducibleQ<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
    {
        return finiteFieldIrreducibleBenOr(poly);
    }


    public static bool
        finiteFieldIrreducibleViaModularComposition<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
    {
        Util.ensureOverFiniteField(poly);

        if (poly.isMonomial())
            return false;

        if (canConvertToZp64(poly))
            return finiteFieldIrreducibleViaModularComposition(asOverZp64(poly));

        if (poly.degree() <= 1)
            return true;

        poly = poly.clone().monic();
        UnivariateDivision.InverseModMonomial<Poly> invMod = UnivariateDivision.fastDivisionPreConditioning(poly);
        // x^q
        Poly xq = UnivariatePolynomialArithmetic.createMonomialMod(poly.coefficientRingCardinality(), poly, invMod);

        // cached powers x^(q^i) for different i
        TIntObjectMap<Poly> cache = new TIntObjectHashMap<>();

        int degree = poly.degree();

        // x^(q^n)
        Poly xqn = composition(xq, degree, poly, invMod, cache);
        // assert
        // xqn.equals(UnivariatePolynomialArithmetic.createMonomialMod(
        //         BigIntegerUtil.pow(poly.coefficientRingCardinality(), degree), poly, invMod))
        //     : "\n" + xqn + "\n" +
        //     UnivariatePolynomialArithmetic.createMonomialMod(
        //         BigIntegerUtil.pow(poly.coefficientRingCardinality(), degree), poly, invMod);

        Poly xMonomial = poly.createMonomial(1);
        if (!xqn.Equals(xMonomial))
            // x^(q^n) != x
            return false;

        // primes factors of poly.degree
        int[] primes = ArraysUtil.getSortedDistinct(SmallPrimes.primeFactors(degree));
        foreach (int prime in
        primes) {
            // x^(q^(n/p))
            Poly xqp = composition(xq, degree / prime, poly, invMod, cache);
            if (!UnivariateGCD.PolynomialGCD(xqp.subtract(xMonomial), poly).isOne())
                return false;
        }
        return true;
    }

    /* fives xq^exponent using repeated compositions */
    static Poly composition<Poly>(
        Poly xq, int exponent,
        Poly poly, UnivariateDivision.InverseModMonomial<Poly> invMod,
        TIntObjectMap<Poly> cache) where Poly : IUnivariatePolynomial<Poly>
    {
        Debug.Assert(exponent > 0);

        Poly cached = cache.get(exponent);
        if (cached != null)
            return cached;

        Poly result = xq.createMonomial(1);
        Poly k2p = xq;
        int rExp = 0, kExp = 1;
        for (;;)
        {
            if ((exponent & 1) != 0)
                cache.put(rExp += kExp, result = ModularComposition.composition(k2p, result, poly, invMod));
            exponent = exponent >> 1;
            if (exponent == 0)
            {
                cache.put(rExp, result);
                return result;
            }

            cache.put(kExp *= 2, k2p = ModularComposition.composition(k2p, k2p, poly, invMod));
        }
    }


    public static bool
        finiteFieldIrreducibleBenOr<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
    {
        Util.ensureOverFiniteField(poly);

        if (poly.isMonomial())
            return false;

        if (canConvertToZp64(poly))
            return finiteFieldIrreducibleBenOr(asOverZp64(poly));

        if (!poly.isMonic())
            poly = poly.clone().monic();

        UnivariateDivision.InverseModMonomial<Poly> invMod = UnivariateDivision.fastDivisionPreConditioning(poly);
        // x
        Poly xMonomial = poly.createMonomial(1);
        // x^q
        Poly xq = xMonomial.clone();
        // primes factors of poly.degree
        for (int i = 1; i <= (poly.degree() / 2); ++i)
        {
            // x^(q^i)
            xq = UnivariatePolynomialArithmetic.polyPowMod(xq, poly.coefficientRingCardinality(), poly, invMod, false);
            if (!UnivariateGCD.PolynomialGCD(xq.clone().subtract(xMonomial), poly).isOne())
                return false;
        }

        return true;
    }


    public static UnivariatePolynomialZp64 randomIrreduciblePolynomial(long modulus, int degree, RandomGenerator rnd)
    {
        return randomIrreduciblePolynomial(UnivariatePolynomialZp64.zero(modulus), degree, rnd);
    }


    public static UnivariatePolynomial<E> randomIrreduciblePolynomial<E>(Ring<E> ring, int degree, RandomGenerator rnd)
    {
        UnivariatePolynomial<E> poly;
        do
        {
            poly = RandomUnivariatePolynomials.randomPoly(degree, ring, rnd);
            if (ring.isField())
                poly.monic();
        } while (!irreducibleQ(poly));

        Debug.Assert(poly.Degree == degree);
        return poly;
    }


    public static UnivariatePolynomial<BigInteger> randomIrreduciblePolynomialOverZ(int degree, RandomGenerator rnd)
    {
        // some prime number
        long modulus = SmallPrimes.nextPrime(1 << 25);
        return randomIrreduciblePolynomial(modulus, degree, rnd).toBigPoly().setRing(Rings.Z);
    }


    public static Poly randomIrreduciblePolynomial<Poly>(Poly factory, int degree, RandomGenerator rnd)
        where Poly : IUnivariatePolynomial<Poly>
    {
        Poly poly;
        do
        {
            poly = RandomUnivariatePolynomials.randomPoly(factory, degree, rnd);
        } while (!irreducibleQ(poly));

        Debug.Assert(poly.degree() == degree);
        return poly;
    }
}