using Cc.Redberry.Rings.Bigint;
using Org.Apache.Commons.Math3.Random;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Util.RoundingMode;
using static Cc.Redberry.Rings.Util.Associativity;
using static Cc.Redberry.Rings.Util.Operator;
using static Cc.Redberry.Rings.Util.TokenType;
using static Cc.Redberry.Rings.Util.SystemInfo;

namespace Cc.Redberry.Rings.Util
{
    /// <summary>
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class RandomUtil
    {
        private RandomUtil()
        {
        }

        /// <summary>
        /// Creates random array of length {@code degree + 1} with elements bounded by {@code bound} (by absolute value).
        /// </summary>
        /// <param name="length">array length</param>
        /// <param name="min">min value</param>
        /// <param name="max">max value</param>
        /// <param name="rnd">random source</param>
        /// <returns>array of length {@code length} with elements bounded by {@code bound} (by absolute value)</returns>
        public static int[] RandomIntArray(int length, int min, int max, RandomGenerator rnd)
        {
            int[] data = new int[length];
            for (int i = 0; i < length; ++i)
                data[i] = min + rnd.NextInt(max - min);
            return data;
        }

        /// <summary>
        /// Creates random array of length {@code degree + 1} with elements bounded by {@code bound} (by absolute value).
        /// </summary>
        /// <param name="length">array length</param>
        /// <param name="total">total</param>
        /// <param name="rnd">random source</param>
        /// <returns>array of length {@code length} with elements bounded by {@code bound} (by absolute value)</returns>
        public static int[] RandomSharpIntArray(int length, int total, RandomGenerator rnd)
        {
            int[] data = new int[length];
            for (int i = 0; i < length; ++i)
            {
                if (total <= 0)
                    break;
                data[i] = rnd.NextInt(total);
                total -= data[i];
            }

            return data;
        }

        /// <summary>
        /// Creates random array of length {@code degree + 1} with elements bounded by {@code bound} (by absolute value).
        /// </summary>
        /// <param name="length">array length</param>
        /// <param name="min">min value</param>
        /// <param name="max">max value</param>
        /// <param name="rnd">random source</param>
        /// <returns>array of length {@code length} with elements bounded by {@code bound} (by absolute value)</returns>
        public static long[] RandomLongArray(int length, long min, long max, RandomGenerator rnd)
        {
            RandomDataGenerator rndd = new RandomDataGenerator(rnd);
            long[] data = new long[length];
            for (int i = 0; i < length; ++i)
                data[i] = rndd.NextLong(min, max - 1);
            return data;
        }

        /// <summary>
        /// Creates random array of length {@code degree + 1} with elements bounded by {@code bound} (by absolute value).
        /// </summary>
        /// <param name="length">array length</param>
        /// <param name="min">min value</param>
        /// <param name="max">max value</param>
        /// <param name="rnd">random source</param>
        /// <returns>array of length {@code length} with elements bounded by {@code bound} (by absolute value)</returns>
        public static BigInteger[] RandomBigIntegerArray(int length, BigInteger min, BigInteger max, RandomGenerator rnd)
        {
            RandomDataGenerator rndd = new RandomDataGenerator(rnd);
            BigInteger[] data = new BigInteger[length];
            BigInteger delta = max.Subtract(min);
            for (int i = 0; i < length; ++i)
                data[i] = min.Add(RandomInt(delta, rnd));
            return data;
        }

        /// <summary>
        /// Returns random integer in range {@code [0, bound)}.
        /// </summary>
        /// <param name="bound">maximal allowed value</param>
        /// <param name="rnd">random</param>
        /// <returns>a BigInteger {@code b} so that {@code 0 <= b < bound}</returns>
        public static BigInteger RandomInt(BigInteger bound, RandomGenerator rnd)
        {
            BigInteger r;
            do
            {
                r = new BigInteger(bound.BitLength(), rnd);
            }
            while (r.CompareTo(bound) >= 0);
            return r;
        }
    }
}