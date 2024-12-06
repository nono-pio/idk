using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Util;
using Org.Apache.Commons.Math3.Random;
using Java.Util.Function;
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
    /// Methods to generate random polynomials.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class RandomUnivariatePolynomials
    {
        private RandomUnivariatePolynomials()
        {
        }

        private static readonly int DEFAULT_BOUND = 100;
        /// <summary>
        /// Creates random polynomial of specified {@code degree}.
        /// </summary>
        /// <param name="factory">the factory (used to infer the type and the ring)</param>
        /// <param name="degree">polynomial degree</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial of specified {@code degree}</returns>
        public static Poly RandomPoly<Poly extends IUnivariatePolynomial<Poly>>(Poly factory, int degree, RandomGenerator rnd)
        {
            if (factory is UnivariatePolynomialZ64)
                return (Poly)RandomPoly(degree, rnd);
            else if (factory is UnivariatePolynomialZp64)
                return (Poly)RandomMonicPoly(degree, ((UnivariatePolynomialZp64)factory).Modulus(), rnd);
            else if (factory is UnivariatePolynomial)
            {
                UnivariatePolynomial p = RandomPoly(degree, ((UnivariatePolynomial)factory).ring, rnd);
                if (factory.IsOverField())
                    p = p.Monic();
                return (Poly)p;
            }

            throw new Exception(factory.GetType().ToString());
        }

        /// <summary>
        /// Creates random polynomial of specified {@code degree}.
        /// </summary>
        /// <param name="degree">polynomial degree</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial of specified {@code degree}</returns>
        public static UnivariatePolynomialZ64 RandomPoly(int degree, RandomGenerator rnd)
        {
            return RandomPoly(degree, DEFAULT_BOUND, rnd);
        }

        /// <summary>
        /// Creates random polynomial of specified {@code degree}.
        /// </summary>
        /// <param name="degree">polynomial degree</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial of specified {@code degree}</returns>
        public static UnivariatePolynomialZp64 RandomMonicPoly(int degree, long modulus, RandomGenerator rnd)
        {
            UnivariatePolynomialZ64 r = RandomPoly(degree, modulus, rnd);
            while (r.data[degree] % modulus == 0)
            {
                r.data[r.degree] = rnd.NextLong();
            }

            return r.Modulus(modulus, false).Monic();
        }

        /// <summary>
        /// Creates random polynomial of specified {@code degree}.
        /// </summary>
        /// <param name="degree">polynomial degree</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial of specified {@code degree}</returns>
        public static UnivariatePolynomial<BigInteger> RandomMonicPoly(int degree, BigInteger modulus, RandomGenerator rnd)
        {
            UnivariatePolynomial<BigInteger> r = RandomPoly(degree, modulus, rnd);
            while ((r.data[degree].Mod(modulus)).IsZero())
            {
                r.data[r.degree] = RandomUtil.RandomInt(modulus, rnd);
            }

            return r.SetRing(new IntegersZp(modulus)).Monic();
        }

        /// <summary>
        /// Creates random polynomial of specified {@code degree}.
        /// </summary>
        /// <param name="degree">polynomial degree</param>
        /// <param name="ring">the ring</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial of specified {@code degree}</returns>
        public static UnivariatePolynomial<E> RandomMonicPoly<E>(int degree, Ring<E> ring, RandomGenerator rnd)
        {
            return RandomPoly(degree, ring, rnd).Monic();
        }

        /// <summary>
        /// Creates random polynomial of specified {@code degree} with elements bounded by {@code bound} (by absolute
        /// value).
        /// </summary>
        /// <param name="degree">polynomial degree</param>
        /// <param name="bound">absolute bound for coefficients</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial of specified {@code degree} with elements bounded by {@code bound} (by absolute value)</returns>
        public static UnivariatePolynomialZ64 RandomPoly(int degree, long bound, RandomGenerator rnd)
        {
            return UnivariatePolynomialZ64.Create(RandomLongArray(degree, bound, rnd));
        }

        /// <summary>
        /// Creates random polynomial of specified {@code degree} with elements bounded by {@code bound} (by absolute
        /// value).
        /// </summary>
        /// <param name="degree">polynomial degree</param>
        /// <param name="bound">absolute bound for coefficients</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial of specified {@code degree} with elements bounded by {@code bound} (by absolute value)</returns>
        public static UnivariatePolynomial<BigInteger> RandomPoly(int degree, BigInteger bound, RandomGenerator rnd)
        {
            return UnivariatePolynomial.CreateUnsafe(Rings.Z, RandomBigArray(degree, bound, rnd));
        }

        /// <summary>
        /// Creates random polynomial of specified {@code degree} with elements from specified ring
        /// </summary>
        /// <param name="degree">polynomial degree</param>
        /// <param name="ring">the ring</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial of specified {@code degree} with elements bounded by {@code bound} (by absolute value)</returns>
        public static UnivariatePolynomial<E> RandomPoly<E>(int degree, Ring<E> ring, RandomGenerator rnd)
        {
            return UnivariatePolynomial.CreateUnsafe(ring, RandomArray(degree, ring, rnd));
        }

        /// <summary>
        /// Creates random polynomial of specified {@code degree} with elements from specified ring
        /// </summary>
        /// <param name="degree">polynomial degree</param>
        /// <param name="ring">the ring</param>
        /// <param name="method">method for generating random coefficients</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial of specified {@code degree} with elements bounded by {@code bound} (by absolute value)</returns>
        public static UnivariatePolynomial<E> RandomPoly<E>(int degree, Ring<E> ring, Function<RandomGenerator, E> method, RandomGenerator rnd)
        {
            return UnivariatePolynomial.CreateUnsafe(ring, RandomArray(degree, ring, method, rnd));
        }

        /// <summary>
        /// Creates random array of length {@code degree + 1} with elements bounded by {@code bound} (by absolute value).
        /// </summary>
        /// <param name="degree">polynomial degree</param>
        /// <param name="bound">absolute bound for coefficients</param>
        /// <param name="rnd">random source</param>
        /// <returns>array of length {@code degree + 1} with elements bounded by {@code bound} (by absolute value)</returns>
        public static long[] RandomLongArray(int degree, long bound, RandomGenerator rnd)
        {
            long[] data = new long[degree + 1];
            RandomDataGenerator rndd = new RandomDataGenerator(rnd);
            for (int i = 0; i <= degree; ++i)
            {
                data[i] = rndd.NextLong(0, bound - 1);
                if (rnd.NextBoolean() && rnd.NextBoolean())
                    data[i] = -data[i];
            }

            while (data[degree] == 0)
                data[degree] = rndd.NextLong(0, bound - 1);
            return data;
        }

        /// <summary>
        /// Creates random array of length {@code degree + 1} with elements bounded by {@code bound} (by absolute value).
        /// </summary>
        /// <param name="degree">polynomial degree</param>
        /// <param name="bound">absolute bound for coefficients</param>
        /// <param name="rnd">random source</param>
        /// <returns>array of length {@code degree + 1} with elements bounded by {@code bound} (by absolute value)</returns>
        public static BigInteger[] RandomBigArray(int degree, BigInteger bound, RandomGenerator rnd)
        {
            long lBound = bound.IsLong() ? bound.LongValue() : Long.MAX_VALUE;
            RandomDataGenerator rndd = new RandomDataGenerator(rnd);
            BigInteger[] data = new BigInteger[degree + 1];
            for (int i = 0; i <= degree; ++i)
            {
                data[i] = RandomUtil.RandomInt(bound, rnd);
                if (rnd.NextBoolean() && rnd.NextBoolean())
                    data[i] = data[i].Negate();
            }

            while (data[degree].Equals(BigInteger.ZERO))
                data[degree] = BigInteger.ValueOf(rndd.NextLong(0, lBound));
            return data;
        }

        /// <summary>
        /// Creates random array of length {@code degree + 1} with elements from the specified ring
        /// </summary>
        /// <param name="degree">polynomial degree</param>
        /// <param name="ring">the ring</param>
        /// <param name="rnd">random source</param>
        /// <returns>array of length {@code degree + 1} with elements from specified ring</returns>
        public static E[] RandomArray<E>(int degree, Ring<E> ring, RandomGenerator rnd)
        {
            return RandomArray(degree, ring, ring.RandomElement(), rnd);
        }

        /// <summary>
        /// Creates random array of length {@code degree + 1} with elements from the specified ring
        /// </summary>
        /// <param name="degree">polynomial degree</param>
        /// <param name="ring">the ring</param>
        /// <param name="method">method for generating random coefficients</param>
        /// <param name="rnd">random source</param>
        /// <returns>array of length {@code degree + 1} with elements from specified ring</returns>
        public static E[] RandomArray<E>(int degree, Ring<E> ring, Function<RandomGenerator, E> method, RandomGenerator rnd)
        {
            E[] data = ring.CreateArray(degree + 1);
            for (int i = 0; i <= degree; ++i)
                data[i] = method.Apply(rnd);
            while (ring.IsZero(data[degree]))
                data[degree] = method.Apply(rnd);
            return data;
        }
    }
}