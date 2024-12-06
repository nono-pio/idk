using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Poly;
using Cc.Redberry.Rings.Primes;
using Cc.Redberry.Rings.Util;
using Gnu.Trove.Map;
using Gnu.Trove.Map.Hash;
using Org.Apache.Commons.Math3.Random;
using Cc.Redberry.Rings.Poly.Univar.Conversions64bit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Poly.Univar.RoundingMode;
using static Cc.Redberry.Rings.Poly.Univar.Associativity;
using static Cc.Redberry.Rings.Poly.Univar.Operator;
using static Cc.Redberry.Rings.Poly.Univar.TokenType;
using static Cc.Redberry.Rings.Poly.Univar.SystemInfo;

namespace Cc.Redberry.Rings.Poly.Univar
{
    /// <summary>
    /// Irreducibility tests and generators for random irreducible polynomials.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class IrreduciblePolynomials
    {
        private IrreduciblePolynomials()
        {
        }

        /// <summary>
        /// Tests whether {@code poly} is irreducible
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>whether {@code poly} is an irreducible polynomial</returns>
        public static bool IrreducibleQ<Poly extends IUnivariatePolynomial<Poly>>(Poly poly)
        {
            if (poly.IsOverFiniteField())
                return FiniteFieldIrreducibleQ(poly);
            else
                return !poly.IsMonomial() && UnivariateFactorization.Factor(poly).IsTrivial();
        }

        /// <summary>
        /// Tests whether {@code poly} is irreducible over the finite field
        /// </summary>
        /// <param name="poly">the polynomial over finite field</param>
        /// <returns>whether {@code poly} is an irreducible polynomial</returns>
        public static bool FiniteFieldIrreducibleQ<Poly extends IUnivariatePolynomial<Poly>>(Poly poly)
        {
            return FiniteFieldIrreducibleBenOr(poly);
        }

        /// <summary>
        /// Tests whether {@code poly} is irreducible over the finite field
        /// </summary>
        /// <param name="poly">the polynomial over finite field</param>
        /// <returns>whether {@code poly} is an irreducible polynomial</returns>
        public static bool FiniteFieldIrreducibleViaModularComposition<Poly extends IUnivariatePolynomial<Poly>>(Poly poly)
        {
            Util.EnsureOverFiniteField(poly);
            if (poly.IsMonomial())
                return false;
            if (CanConvertToZp64(poly))
                return FiniteFieldIrreducibleViaModularComposition(AsOverZp64(poly));
            if (poly.Degree() <= 1)
                return true;
            poly = poly.Clone().Monic();
            UnivariateDivision.InverseModMonomial<Poly> invMod = UnivariateDivision.FastDivisionPreConditioning(poly);

            // x^q
            Poly xq = UnivariatePolynomialArithmetic.CreateMonomialMod(poly.CoefficientRingCardinality(), poly, invMod);

            // cached powers x^(q^i) for different i
            TIntObjectMap<Poly> cache = new TIntObjectHashMap();
            int degree = poly.Degree();

            // x^(q^n)
            Poly xqn = Composition(xq, degree, poly, invMod, cache);
            Poly xMonomial = poly.CreateMonomial(1);
            if (!xqn.Equals(xMonomial))

                // x^(q^n) != x
                return false;

            // primes factors of poly.degree
            int[] primes = ArraysUtil.GetSortedDistinct(SmallPrimes.PrimeFactors(degree));
            foreach (int prime in primes)
            {

                // x^(q^(n/p))
                Poly xqp = Composition(xq, degree / prime, poly, invMod, cache);
                if (!UnivariateGCD.PolynomialGCD(xqp.Subtract(xMonomial), poly).IsOne())
                    return false;
            }

            return true;
        }

        /* fives xq^exponent using repeated compositions */
        static Poly Composition<Poly extends IUnivariatePolynomial<Poly>>(Poly xq, int exponent, Poly poly, UnivariateDivision.InverseModMonomial<Poly> invMod, TIntObjectMap<Poly> cache)
        {
            Poly cached = cache[exponent];
            if (cached != null)
                return cached;
            Poly result = xq.CreateMonomial(1);
            Poly k2p = xq;
            int rExp = 0, kExp = 1;
            for (;;)
            {
                if ((exponent & 1) != 0)
                    cache.Put(rExp += kExp, result = ModularComposition.Composition(k2p, result, poly, invMod));
                exponent = exponent >> 1;
                if (exponent == 0)
                {
                    cache.Put(rExp, result);
                    return result;
                }

                cache.Put(kExp *= 2, k2p = ModularComposition.Composition(k2p, k2p, poly, invMod));
            }
        }

        /// <summary>
        /// Tests whether {@code poly} is irreducible over the finite field
        /// </summary>
        /// <param name="poly">the polynomial over finite field</param>
        /// <returns>whether {@code poly} is an irreducible polynomial</returns>
        public static bool FiniteFieldIrreducibleBenOr<Poly extends IUnivariatePolynomial<Poly>>(Poly poly)
        {
            Util.EnsureOverFiniteField(poly);
            if (poly.IsMonomial())
                return false;
            if (CanConvertToZp64(poly))
                return FiniteFieldIrreducibleBenOr(AsOverZp64(poly));
            if (!poly.IsMonic())
                poly = poly.Clone().Monic();
            UnivariateDivision.InverseModMonomial<Poly> invMod = UnivariateDivision.FastDivisionPreConditioning(poly);

            // x
            Poly xMonomial = poly.CreateMonomial(1);

            // x^q
            Poly xq = xMonomial.Clone();

            // primes factors of poly.degree
            for (int i = 1; i <= (poly.Degree() / 2); ++i)
            {

                // x^(q^i)
                xq = UnivariatePolynomialArithmetic.PolyPowMod(xq, poly.CoefficientRingCardinality(), poly, invMod, false);
                if (!UnivariateGCD.PolynomialGCD(xq.Clone().Subtract(xMonomial), poly).IsOne())
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Generated random irreducible Zp polynomial of degree {@code degree}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="degree">the degree</param>
        /// <param name="rnd">random source</param>
        /// <returns>irreducible polynomial</returns>
        public static UnivariatePolynomialZp64 RandomIrreduciblePolynomial(long modulus, int degree, RandomGenerator rnd)
        {
            return RandomIrreduciblePolynomial(UnivariatePolynomialZp64.Zero(modulus), degree, rnd);
        }

        /// <summary>
        /// Generated random irreducible polynomial over specified ring of degree {@code degree}
        /// </summary>
        /// <param name="ring">coefficient ring</param>
        /// <param name="degree">the degree</param>
        /// <param name="rnd">random source</param>
        /// <returns>irreducible polynomial</returns>
        public static UnivariatePolynomial<E> RandomIrreduciblePolynomial<E>(Ring<E> ring, int degree, RandomGenerator rnd)
        {
            UnivariatePolynomial<E> poly;
            do
            {
                poly = RandomUnivariatePolynomials.RandomPoly(degree, ring, rnd);
                if (ring.IsField())
                    poly.Monic();
            }
            while (!IrreducibleQ(poly));
            return poly;
        }

        /// <summary>
        /// Generated random irreducible polynomial over Z
        /// </summary>
        /// <param name="degree">the degree</param>
        /// <param name="rnd">random source</param>
        /// <returns>irreducible polynomial over Z</returns>
        public static UnivariatePolynomial<BigInteger> RandomIrreduciblePolynomialOverZ(int degree, RandomGenerator rnd)
        {

            // some prime number
            long modulus = SmallPrimes.NextPrime(1 << 25);
            return RandomIrreduciblePolynomial(modulus, degree, rnd).ToBigPoly().SetRing(Rings.Z);
        }

        /// <summary>
        /// Generated random irreducible polynomial of degree {@code degree}
        /// </summary>
        /// <param name="factory">type marker</param>
        /// <param name="degree">the degree</param>
        /// <param name="rnd">random source</param>
        /// <returns>irreducible polynomial</returns>
        public static Poly RandomIrreduciblePolynomial<Poly extends IUnivariatePolynomial<Poly>>(Poly factory, int degree, RandomGenerator rnd)
        {
            Poly poly;
            do
            {
                poly = RandomUnivariatePolynomials.RandomPoly(factory, degree, rnd);
            }
            while (!IrreducibleQ(poly));
            return poly;
        }
    }
}