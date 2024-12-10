using System.Numerics;

namespace Cc.Redberry.Rings.Bigint
{
    /// <summary>
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class BigIntegerUtil
    {
        private BigIntegerUtil()
        {
        }

        public static BigInteger Max(BigInteger a, BigInteger b)
        {
            return a.CompareTo(b) > 0 ? a : b;
        }

        public static BigInteger Abs(BigInteger a)
        {
            return a.Abs();
        }

        public static BigInteger Gcd(BigInteger a, BigInteger b)
        {
            return a.Gcd(b);
        }

        /// <summary>
        /// Returns the greatest common an array of longs
        /// </summary>
        /// <param name="integers">array of longs</param>
        /// <param name="from">from position (inclusive)</param>
        /// <param name="to">to position (exclusive)</param>
        /// <returns>greatest common divisor of array</returns>
        public static BigInteger Gcd(BigInteger[] integers, int from, int to)
        {
            if (integers.Length < 2)
                throw new ArgumentException();
            BigInteger gcd = Gcd(integers[from], integers[from + 1]);
            if (gcd.IsOne)
                return gcd;
            for (int i = from + 2; i < to; i++)
            {
                gcd = Gcd(integers[i], gcd);
                if (gcd.IsOne)
                    return gcd;
            }

            return gcd;
        }

        /// <summary>
        /// Returns {@code base} in a power of {@code e} (non negative)
        /// </summary>
        /// <param name="base">base</param>
        /// <param name="exponent">exponent (non negative)</param>
        /// <returns>{@code base} in a power of {@code e}</returns>
        /// <exception cref="ArithmeticException">if the result overflows a long</exception>
        public static BigInteger Pow(long @base, long exponent)
        {
            return Pow(new BigInteger(@base), exponent);
        }

        /// <summary>
        /// Returns {@code base} in a power of {@code e} (non negative)
        /// </summary>
        /// <param name="base">base</param>
        /// <param name="exponent">exponent (non negative)</param>
        /// <returns>{@code base} in a power of {@code e}</returns>
        /// <exception cref="ArithmeticException">if the result overflows a long</exception>
        public static BigInteger Pow(BigInteger @base, long exponent)
        {
            if (exponent < 0)
                throw new ArgumentException();
            if (exponent < int.MaxValue)
                return Pow(@base, (int)exponent);
            BigInteger result = BigInteger.One;
            BigInteger k2p = @base;
            for (;;)
            {
                if ((exponent & 1) != 0)
                    result = result.Multiply(k2p);
                exponent = exponent >> 1;
                if (exponent == 0)
                    return result;
                k2p = k2p.Multiply(k2p);
            }
        }

        /// <summary>
        /// Returns {@code base} in a power of {@code e} (non negative)
        /// </summary>
        /// <param name="base">base</param>
        /// <param name="exponent">exponent (non negative)</param>
        /// <returns>{@code base} in a power of {@code e}</returns>
        /// <exception cref="ArithmeticException">if the result overflows a long</exception>
        public static BigInteger Pow(BigInteger @base, int exponent)
        {
            return @base.Pow(exponent);
        }

        /// <summary>
        /// Returns {@code base} in a power of {@code e} (non negative)
        /// </summary>
        /// <param name="base">base</param>
        /// <param name="exponent">exponent (non negative)</param>
        /// <returns>{@code base} in a power of {@code e}</returns>
        /// <exception cref="ArithmeticException">if the result overflows a long</exception>
        public static BigInteger Pow(BigInteger @base, BigInteger exponent)
        {
            if (exponent.Sign < 0)
                throw new ArgumentException();
            BigInteger result = BigInteger.One;
            BigInteger k2p = @base;
            for (;;)
            {
                if (exponent.TestBit(0))
                    result = result.Multiply(k2p);
                exponent = exponent.ShiftRight(1);
                if (exponent.IsZero)
                    return result;
                k2p = k2p.Multiply(k2p);
            }
        }

        /// <summary>
        /// Returns floor square root of {@code val}
        /// </summary>
        /// <param name="val">the number</param>
        /// <returns>floor square root</returns>
        public static BigInteger SqrtFloor(BigInteger val)
        {
            if (val.Signum() < 0)
                throw new ArgumentException("Negative argument.");
            if (val.IsZero || val.IsOne)
                return val;
            BigInteger y;

            // starting with y = x / 2 avoids magnitude issues with x squared
            for (y = val.ShiftRight(1); y.CompareTo(val.Divide(y)) > 0; y = ((val.Divide(y)).Add(y)).ShiftRight(1))
            {
            }

            return y;
        }

        /// <summary>
        /// Returns ceil square root of {@code val}
        /// </summary>
        /// <param name="val">the number</param>
        /// <returns>floor square root</returns>
        public static BigInteger SqrtCeil(BigInteger val)
        {
            if (val.Signum() < 0)
                throw new ArgumentException("Negative argument.");
            if (val.IsZero || val.IsOne)
                return val;
            BigInteger y;

            // starting with y = x / 2 avoids magnitude issues with x squared
            for (y = val.ShiftRight(1); y.CompareTo(val.Divide(y)) > 0; y = ((val.Divide(y)).Add(y)).ShiftRight(1))
            {
            }

            if (val.CompareTo(y.Multiply(y)) == 0)
                return y;
            else
                return y.Add(BigInteger.One);
        }

        /// <summary>
        /// Tests whether {@code n} is a perfect power {@code n == a^b} and returns {@code {a, b}} if so and {@code null}
        /// otherwise
        /// </summary>
        /// <param name="n">the number</param>
        /// <returns>array {@code {a, b}} so that {@code n = a^b} or {@code null} is {@code n} is not a perfect power</returns>
        public static BigInteger[]? PerfectPowerDecomposition(BigInteger n)
        {
            if (n.Signum() < 0)
            {
                n = n.Negate();
                BigInteger[] ipp = PerfectPowerDecomposition(n);
                if (ipp == null)
                    return null;
                if (ipp[1].TestBit(0))
                    return null;
                ipp[0] = ipp[0].Negate();
                return ipp;
            }

            if (n.BitCount() == 1)
                return new BigInteger[]
                {
                    2,
                    new BigInteger(n.BitLength() - 1)
                };
            int lgn = 1 + n.BitLength();
            for (int b = 2; b < lgn; b++)
            {

                //b lg a = lg n
                BigInteger lowa = BigInteger.One;
                BigInteger higha = BigInteger.One.ShiftLeft(lgn / b + 1);
                while (lowa.CompareTo(higha.Decrement()) < 0)
                {
                    BigInteger mida = (lowa.Add(higha)).ShiftRight(1);
                    BigInteger ab = Pow(mida, b);
                    if (ab.CompareTo(n) > 0)
                        higha = mida;
                    else if (ab.CompareTo(n) < 0)
                        lowa = mida;
                    else
                    {
                        BigInteger[] ipp = PerfectPowerDecomposition(mida);
                        if (ipp != null)
                            return new BigInteger[]
                            {
                                ipp[0],
                                ipp[1].Multiply(b)
                            };
                        else
                            return new BigInteger[]
                            {
                                mida,
                                new BigInteger(b)
                            };
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Factorial of a number
        /// </summary>
        public static BigInteger Factorial(int number)
        {
            BigInteger r = BigInteger.One;
            for (int i = 1; i <= number; i++)
                r = r.Multiply(i);
            return r;
        }

        /// <summary>
        /// Binomial coefficient
        /// </summary>
        public static BigInteger Binomial(int n, int k)
        {
            if (k > n - k)
                k = n - k;
            BigInteger b = BigInteger.One;
            for (int i = 1, m = n; i <= k; i++, m--)
                b = b.Multiply(m).DivideExact(new BigInteger(i));
            return b;
        }
    }
}