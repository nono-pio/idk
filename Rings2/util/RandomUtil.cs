using System.Numerics;
using Cc.Redberry.Rings.Bigint;

namespace Cc.Redberry.Rings.Util
{
    /// <summary>
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public static class RandomUtil
    {

        /// <summary>
        /// Creates random array of length {@code degree + 1} with elements bounded by {@code bound} (by absolute value).
        /// </summary>
        /// <param name="length">array length</param>
        /// <param name="min">min value</param>
        /// <param name="max">max value</param>
        /// <param name="rnd">random source</param>
        /// <returns>array of length {@code length} with elements bounded by {@code bound} (by absolute value)</returns>
        public static int[] RandomIntArray(int length, int min, int max, Random rnd)
        {
            var data = new int[length];
            for (var i = 0; i < length; ++i)
                data[i] = min + rnd.Next(max - min);
            return data;
        }

        /// <summary>
        /// Creates random array of length {@code degree + 1} with elements bounded by {@code bound} (by absolute value).
        /// </summary>
        /// <param name="length">array length</param>
        /// <param name="total">total</param>
        /// <param name="rnd">random source</param>
        /// <returns>array of length {@code length} with elements bounded by {@code bound} (by absolute value)</returns>
        public static int[] RandomSharpIntArray(int length, int total, Random rnd)
        {
            var data = new int[length];
            for (var i = 0; i < length; ++i)
            {
                if (total <= 0)
                    break;
                data[i] = rnd.Next(total);
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
        public static long[] RandomLongArray(int length, long min, long max, Random rnd)
        {
            var rndd = new RandomDataGenerator(rnd);
            var data = new long[length];
            for (var i = 0; i < length; ++i)
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
        public static BigInteger[] RandomBigIntegerArray(int length, BigInteger min, BigInteger max, Random rnd)
        {
            var data = new BigInteger[length];
            var delta = max.Subtract(min);
            for (var i = 0; i < length; ++i)
                data[i] = min.Add(RandomInt(delta, rnd));
            return data;
        }

        /// <summary>
        /// Returns random integer in range {@code [0, bound)}.
        /// </summary>
        /// <param name="bound">maximal allowed value</param>
        /// <param name="rnd">random</param>
        /// <returns>a BigInteger {@code b} so that {@code 0 smaller or equal than b smaller than bound}</returns>
        public static BigInteger RandomInt(BigInteger bound, Random rnd)
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