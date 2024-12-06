using Cc.Redberry.Libdivide4j;
using Cc.Redberry.Rings.Bigint;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Poly.RoundingMode;
using static Cc.Redberry.Rings.Poly.Associativity;
using static Cc.Redberry.Rings.Poly.Operator;
using static Cc.Redberry.Rings.Poly.TokenType;
using static Cc.Redberry.Rings.Poly.SystemInfo;

namespace Cc.Redberry.Rings.Poly
{
    /// <summary>
    /// Helper methods for arithmetic with machine numbers.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class MachineArithmetic
    {
        private MachineArithmetic()
        {
        }

        /// <summary>
        /// Max supported modulus bits which fits into machine word
        /// </summary>
        public static readonly int MAX_SUPPORTED_MODULUS_BITS = 62;
        /// <summary>
        /// Max supported modulus which fits into machine word
        /// </summary>
        public static readonly long MAX_SUPPORTED_MODULUS = (1 << MAX_SUPPORTED_MODULUS_BITS) - 1;
        /// <summary>
        /// Max supported modulus
        /// </summary>
        public static readonly BigInteger b_MAX_SUPPORTED_MODULUS = BigInteger.ValueOf(MAX_SUPPORTED_MODULUS);
        /// <summary>
        /// Returns true if {@code val} fits into 32-bit machine word (unsigned) and false otherwise
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>true if {@code val} fits into 32-bit machine word (unsigned) and false otherwise</returns>
        public static bool Fits32bitWord(long val)
        {
            return Long.CompareUnsigned(val, (1 << 32) - 1) <= 0;
        }

        /// <summary>
        /// Returns true if {@code val} fits into 32-bit machine word (unsigned) and false otherwise
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>true if {@code val} fits into 32-bit machine word (unsigned) and false otherwise</returns>
        public static bool Fits31bitWord(long val)
        {
            return Long.CompareUnsigned(val, (1 << 31) - 1) <= 0;
        }

        /// <summary>
        /// Delegates to {@link Math#multiplyExact(long, long)}
        /// </summary>
        /// <exception cref="ArithmeticException">if the result overflows a long</exception>
        public static long SafeMultiply(long x, long y)
        {
            return Math.MultiplyExact(x, y);
        }

        /// <summary>
        /// Delegates to {@link Math#multiplyExact(long, long)}
        /// </summary>
        /// <exception cref="ArithmeticException">if the result overflows a long</exception>
        public static long SafeMultiply(long x, long y, long z)
        {
            return Math.MultiplyExact(Math.MultiplyExact(x, y), z);
        }

        /// <summary>
        /// Delegates to {@link Math#addExact(long, long)}
        /// </summary>
        /// <exception cref="ArithmeticException">if the result overflows a long</exception>
        public static long SafeAdd(long x, long y)
        {
            return Math.AddExact(x, y);
        }

        /// <summary>
        /// Delegates to {@link Math#subtractExact(long, long)}
        /// </summary>
        /// <exception cref="ArithmeticException">if the result overflows a long</exception>
        public static long SafeSubtract(long a, long b)
        {
            return Math.SubtractExact(a, b);
        }

        /// <summary>
        /// Delegates to {@link Math#negateExact(long)}
        /// </summary>
        /// <exception cref="ArithmeticException">if the result overflows a long</exception>
        public static long SafeNegate(long x)
        {
            return Math.NegateExact(x);
        }

        /// <summary>
        /// Tests whether the multiplication of {@code x*y} will cause long overflow
        /// </summary>
        public static bool IsOverflowMultiply(long x, long y)
        {
            long r = x * y;
            long ax = Math.Abs(x);
            long ay = Math.Abs(y);
            if (((ax | ay) >>> 31 != 0))
            {

                // Some bits greater than 2^31 that might cause overflow
                // Check the result using the divide operator
                // and check for the special case of Long.MIN_VALUE * -1
                if (((y != 0) && (r / y != x)) || (x == Long.MIN_VALUE && y == -1))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tests whether the addition of {@code x + y} will cause long overflow
        /// </summary>
        public static bool IsOverflowAdd(long x, long y)
        {
            long r = x + y;

            // HD 2-12 Overflow iff both arguments have the opposite sign of the result
            if (((x ^ r) & (y ^ r)) < 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns {@code base} in a power of {@code e} (non negative)
        /// </summary>
        /// <param name="base">base</param>
        /// <param name="exponent">exponent (non negative)</param>
        /// <returns>{@code base} in a power of {@code e}</returns>
        /// <exception cref="ArithmeticException">if the result overflows a long</exception>
        public static long SafePow(long @base, long exponent)
        {
            if (exponent < 0)
                throw new ArgumentException();
            long result = 1;
            long k2p = @base;
            for (;;)
            {
                if ((exponent & 1) != 0)
                    result = SafeMultiply(result, k2p);
                exponent = exponent >> 1;
                if (exponent == 0)
                    return result;
                k2p = SafeMultiply(k2p, k2p);
            }
        }

        /// <summary>
        /// Returns {@code base} in a power of {@code e} (non negative)
        /// </summary>
        /// <param name="base">base</param>
        /// <param name="exponent">exponent (non negative)</param>
        /// <returns>{@code base} in a power of {@code e}</returns>
        public static long UnsafePow(long @base, long exponent)
        {
            if (exponent < 0)
                throw new ArgumentException();
            long result = 1;
            long k2p = @base;
            for (;;)
            {
                if ((exponent & 1) != 0)
                    result *= k2p;
                exponent = exponent >> 1;
                if (exponent == 0)
                    return result;
                k2p *= k2p;
            }
        }

        /// <summary>
        /// Returns the greatest common divisor of two longs.
        /// </summary>
        /// <param name="p">a long</param>
        /// <param name="q">a long</param>
        /// <returns>greatest common divisor of {@code a} and {@code b}</returns>
        public static long Gcd(long p, long q)
        {
            long u = p;
            long v = q;
            if ((u == 0) || (v == 0))
            {
                if ((u == Long.MIN_VALUE) || (v == Long.MIN_VALUE))
                    throw new ArgumentException("long overflow");
                return Math.Abs(u) + Math.Abs(v);
            }


            // keep u and v negative, as negative integers range down to
            // -2^63, while positive numbers can only be as large as 2^63-1
            // (i.e. we can't necessarily negate a negative number without
            // overflow)
            /* assert u!=0 && v!=0; */
            if (u > 0)
            {
                u = -u;
            } // make u negative

            if (v > 0)
            {
                v = -v;
            } // make v negative


            // B1. [Find power of 2]
            int k = 0;
            while ((u & 1) == 0 && (v & 1) == 0 && k < 63)
            {

                // while u and v are
                // both even...
                u /= 2;
                v /= 2;
                k++; // cast out twos.
            }

            if (k == 63)
            {
                throw new ArgumentException("Overflow");
            }


            // B2. Initialize: u and v have been divided by 2^k and at least
            // one is odd.
            long t = ((u & 1) == 1) ? v : -(u / 2);

            // t negative: u was odd, v may be even (t replaces v)
            // t positive: u was even, v is odd (t replaces u)
            do
            {
                /* assert u<0 && v<0; */

                // B4/B3: cast out twos from t.
                while ((t & 1) == 0)
                {

                    // while t is even..
                    t /= 2; // cast out twos
                }


                // B5 [reset max(u,v)]
                if (t > 0)
                {
                    u = -t;
                }
                else
                {
                    v = t;
                }


                // B6/B3. at this point both u and v should be odd.
                t = (v - u) / 2; // |u| larger: t positive (replace u)
                // |v| larger: t negative (replace v)
            }
            while (t != 0);
            return -u * (1 << k); // gcd is u*2^k
        }

        /// <summary>
        /// Computes the greatest common divisor of the absolute value of two numbers, using a modified version of the
        /// "binary gcd" method. See Knuth 4.5.2 algorithm B. The algorithm is due to Josef Stein (1961). <br/> Special
        /// cases: <ul> <li>The invocations {@code gcd(Integer.MIN_VALUE, Integer.MIN_VALUE)}, {@code gcd(Integer.MIN_VALUE,
        /// 0)} and {@code gcd(0, Integer.MIN_VALUE)} throw an {@code ArithmeticException}, because the result would be 2^31,
        /// which is too large for an int value.</li> <li>The result of {@code gcd(x, x)}, {@code gcd(0, x)} and {@code
        /// gcd(x, 0)} is the absolute value of {@code x}, except for the special cases above.</li> <li>The invocation {@code
        /// gcd(0, 0)} is the only one which returns {@code 0}.</li> </ul>
        /// </summary>
        /// <param name="p">Number.</param>
        /// <param name="q">Number.</param>
        /// <returns>the greatest common divisor (never negative).</returns>
        public static int Gcd(int p, int q)
        {
            int a = p;
            int b = q;
            if (a == 0 || b == 0)
            {
                if (a == Integer.MIN_VALUE || b == Integer.MIN_VALUE)
                    throw new ArgumentException("int overflow");
                return Math.Abs(a + b);
            }

            long al = a;
            long bl = b;
            bool useLong = false;
            if (a < 0)
            {
                if (Integer.MIN_VALUE == a)
                    useLong = true;
                else
                    a = -a;
                al = -al;
            }

            if (b < 0)
            {
                if (Integer.MIN_VALUE == b)
                    useLong = true;
                else
                    b = -b;
                bl = -bl;
            }

            if (useLong)
            {
                if (al == bl)
                    throw new ArgumentException("int overflow");
                long blbu = bl;
                bl = al;
                al = blbu % al;
                if (al == 0)
                {
                    if (bl > Integer.MAX_VALUE)
                        throw new ArgumentException("int overflow");
                    return (int)bl;
                }

                blbu = bl;

                // Now "al" and "bl" fit in an "int".
                b = (int)al;
                a = (int)(blbu % al);
            }

            return GcdPositive(a, b);
        }

        /// <summary>
        /// Computes the greatest common divisor of two <em>positive</em> numbers (this precondition is <em>not</em> checked
        /// and the result is undefined if not fulfilled) using the "binary gcd" method which avoids division and modulo
        /// operations. See Knuth 4.5.2 algorithm B. The algorithm is due to Josef Stein (1961). <br/> Special cases: <ul>
        /// <li>The result of {@code gcd(x, x)}, {@code gcd(0, x)} and {@code gcd(x, 0)} is the value of {@code x}.</li>
        /// <li>The invocation {@code gcd(0, 0)} is the only one which returns {@code 0}.</li> </ul>
        /// </summary>
        /// <param name="a">Positive number.</param>
        /// <param name="b">Positive number.</param>
        /// <returns>the greatest common divisor.</returns>
        private static int GcdPositive(int a, int b)
        {
            if (a == 0)
                return b;
            else if (b == 0)
                return a;

            // Make "a" and "b" odd, keeping track of common power of 2.
            int aTwos = Integer.NumberOfTrailingZeros(a);
            a >>= aTwos;
            int bTwos = Integer.NumberOfTrailingZeros(b);
            b >>= bTwos;
            int shift = Math.Min(aTwos, bTwos);

            // "a" and "b" are positive.
            // If a > b then "gdc(a, b)" is equal to "gcd(a - b, b)".
            // If a < b then "gcd(a, b)" is equal to "gcd(b - a, a)".
            // Hence, in the successive iterations:
            //  "a" becomes the absolute difference of the current values,
            //  "b" becomes the minimum of the current values.
            while (a != b)
            {
                int delta = a - b;
                b = Math.Min(a, b);
                a = Math.Abs(delta);

                // Remove any power of 2 in "a" ("b" is guaranteed to be odd).
                a >>= Integer.NumberOfTrailingZeros(a);
            }


            // Recover the common power of 2.
            return a << shift;
        }

        /// <summary>
        /// Runs extended Euclidean algorithm to compute {@code [gcd(a,b), x, y]} such that {@code x * a + y * b = gcd(a,
        /// b)}
        /// </summary>
        /// <param name="a">a long</param>
        /// <param name="b">a long</param>
        /// <returns>array of {@code [gcd(a,b), x, y]} such that {@code x * a + y * b = gcd(a, b)}</returns>
        public static long[] GcdExtended(long a, long b)
        {
            long s = 0, old_s = 1;
            long t = 1, old_t = 0;
            long r = b, old_r = a;
            long q;
            long tmp;
            while (r != 0)
            {
                q = old_r / r;
                tmp = old_r;
                old_r = r;
                r = tmp - q * r;
                tmp = old_s;
                old_s = s;
                s = tmp - q * s;
                tmp = old_t;
                old_t = t;
                t = tmp - q * t;
            }

            return new long[]
            {
                old_r,
                old_s,
                old_t
            };
        }

        /// <summary>
        /// Returns the least common multiple of two longs
        /// </summary>
        /// <param name="a">a long</param>
        /// <param name="b">a long</param>
        /// <returns>least common multiple of {@code a} and {@code b}</returns>
        /// <exception cref="ArithmeticException">if the result overflows a long</exception>
        public static long Lcm(long a, long b)
        {
            if (a == 0 || b == 0)
                return 0;
            return SafeMultiply(a / Gcd(a, b), b);
        }

        /// <summary>
        /// Returns the least common multiple of two integers
        /// </summary>
        /// <param name="a">a number</param>
        /// <param name="b">a number</param>
        /// <returns>least common multiple of {@code a} and {@code b}</returns>
        public static int Lcm(int a, int b)
        {
            if (a == 0 || b == 0)
                return 0;
            return (a / Gcd(a, b)) * b;
        }

        /// <summary>
        /// Returns the greatest common an array of longs
        /// </summary>
        /// <param name="integers">array of longs</param>
        /// <param name="from">from position (inclusive)</param>
        /// <param name="to">to position (exclusive)</param>
        /// <returns>greatest common divisor of array</returns>
        public static long Gcd(long[] integers, int from, int to)
        {
            if (integers.Length < 2)
                throw new ArgumentException();
            long gcd = Gcd(integers[from], integers[from + 1]);
            if (gcd == 1)
                return gcd;
            for (int i = from + 2; i < to; i++)
            {
                gcd = Gcd(integers[i], gcd);
                if (gcd == 1)
                    return gcd;
            }

            return gcd;
        }

        /// <summary>
        /// Returns the greatest common an array of longs
        /// </summary>
        /// <param name="integers">array of longs</param>
        /// <returns>greatest common divisor of array</returns>
        public static long Gcd(params long[] integers)
        {
            return Gcd(integers, 0, integers.Length);
        }

        /// <summary>
        /// Returns the greatest common an array of integers
        /// </summary>
        /// <param name="integers">array of integers</param>
        /// <param name="from">from position (inclusive)</param>
        /// <param name="to">to position (exclusive)</param>
        /// <returns>greatest common divisor of array</returns>
        public static long Gcd(int[] integers, int from, int to)
        {
            if (integers.Length < 2)
                throw new ArgumentException();
            long gcd = Gcd(integers[from], integers[from + 1]);
            if (gcd == 1)
                return gcd;
            for (int i = from + 2; i < to; i++)
            {
                gcd = Gcd(integers[i], gcd);
                if (gcd == 1)
                    return gcd;
            }

            return gcd;
        }

        /// <summary>
        /// Returns the greatest common an array of integers
        /// </summary>
        /// <param name="integers">array of integers</param>
        /// <returns>greatest common divisor of array</returns>
        public static long Gcd(params int[] integers)
        {
            return Gcd(integers, 0, integers.Length);
        }

        /// <summary>
        /// Delegates to {@link Math#floorMod(long, long)}
        /// </summary>
        public static long Mod(long num, long modulus)
        {
            if (num < 0)
                num += modulus; //<- may help
            return (num >= modulus || num < 0) ? Math.FloorMod(num, modulus) : num;
        }

        /// <summary>
        /// Returns {@code value mod modulus} in the symmetric representation ({@code -modulus/2 <= result <= modulus/2})
        /// </summary>
        /// <param name="value">a long</param>
        /// <param name="modulus">modulus</param>
        /// <returns>{@code value mod modulus} in the symmetric representation ({@code -modulus/2 <= result <= modulus/2})</returns>
        public static long SymMod(long value, long modulus)
        {
            value = Mod(value, modulus);
            return value <= modulus / 2 ? value : value - modulus;
        }

        /// <summary>
        /// Returns a solution of congruence {@code num * x = 1 mod modulus}
        /// </summary>
        /// <param name="num">base</param>
        /// <param name="modulus">modulus</param>
        /// <returns>{@code a^(-1) mod p}</returns>
        /// <exception cref="IllegalArgumentException">{@code a} and {@code modulus} are not coprime</exception>
        public static long ModInverse(long num, long modulus)
        {
            if (num == 1)
                return num;
            if (num < 0)
                num = Mod(num, modulus);
            long s = 0, old_s = 1;
            long r = modulus, old_r = num;
            long q;
            long tmp;
            while (r != 0)
            {
                q = old_r / r;
                tmp = old_r;
                old_r = r;
                r = tmp - q * r;
                tmp = old_s;
                old_s = s;
                s = tmp - q * s;
            }

            if (old_r != 1)
                throw new ArgumentException(String.Format("modInverse(%s, %s) : not invertible (old_r = %s)", num, modulus, old_r));
            return Mod(old_s, modulus);
        }

        /// <summary>
        /// Casts {@code long} to signed {@code int} throwing exception in case of overflow.
        /// </summary>
        /// <param name="value">the long</param>
        /// <returns>int value</returns>
        /// <exception cref="ArithmeticException">if the result overflows a long</exception>
        public static int SafeToInt(long value)
        {
            if ((int)value != value)
            {
                throw new ArithmeticException("integer overflow: " + value);
            }

            return (int)value;
        }

        /// <summary>
        /// Returns {@code base} in a power of non-negative {@code e} modulo {@code modulus}
        /// </summary>
        /// <param name="base">the base</param>
        /// <param name="exponent">the non-negative exponent</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code base} in a power of {@code e}</returns>
        public static long PowMod(long @base, long exponent, long modulus)
        {
            return PowModSigned(@base, exponent, FastDivision.MagicSigned(modulus));
        }

        /// <summary>
        /// Returns {@code base} in a power of non-negative {@code e} modulo {@code magic.modulus}
        /// </summary>
        /// <param name="base">the base</param>
        /// <param name="exponent">the non-negative exponent</param>
        /// <param name="magic">magic modulus</param>
        /// <returns>{@code base} in a power of {@code e}</returns>
        public static long PowModSigned(long @base, long exponent, FastDivision.Magic magic)
        {
            if (exponent < 0)
                throw new ArgumentException();
            if (exponent == 0)
                return 1;
            long result = 1;
            long k2p = FastDivision.ModSignedFast(@base, magic);
            for (;;)
            {
                if ((exponent & 1) != 0)
                    result = FastDivision.ModSignedFast(result * k2p, magic);
                exponent = exponent >> 1;
                if (exponent == 0)
                    return result;
                k2p = FastDivision.ModSignedFast(k2p * k2p, magic);
            }
        }

        /// <summary>
        /// Returns {@code base} in a power of non-negative {@code e} modulo {@code magic.modulus}
        /// </summary>
        /// <param name="base">the base</param>
        /// <param name="exponent">the non-negative exponent</param>
        /// <param name="magic">magic modulus</param>
        /// <returns>{@code base} in a power of {@code e}</returns>
        public static long PowModUnsigned(long @base, long exponent, FastDivision.Magic magic)
        {
            if (exponent < 0)
                throw new ArgumentException();
            if (exponent == 0)
                return 1;
            long result = 1;
            long k2p = FastDivision.ModUnsignedFast(@base, magic);
            for (;;)
            {
                if ((exponent & 1) != 0)
                    result = FastDivision.ModUnsignedFast(result * k2p, magic);
                exponent = exponent >> 1;
                if (exponent == 0)
                    return result;
                k2p = FastDivision.ModUnsignedFast(k2p * k2p, magic);
            }
        }

        /// <summary>
        /// Tests whether {@code n} is a perfect power {@code n == a^b} and returns {@code {a, b}} if so and {@code null}
        /// otherwise
        /// </summary>
        /// <param name="n">the number</param>
        /// <returns>array {@code {a, b}} so that {@code n = a^b} or {@code null} is {@code n} is not a perfect power</returns>
        public static long[] PerfectPowerDecomposition(long n)
        {
            if (n < 0)
            {
                long[] ipp = PerfectPowerDecomposition(-n);
                if (ipp == null)
                    return null;
                if (ipp[1] % 2 == 0)
                    return null;
                ipp[0] = -ipp[0];
                return ipp;
            }

            if ((-n & n) == n)
                return new long[]
                {
                    2,
                    63 - Long.NumberOfLeadingZeros(n)
                };
            int lgn = 1 + (64 - Long.NumberOfLeadingZeros(n));
            for (int b = 2; b < lgn; b++)
            {

                //b lg a = lg n
                long lowa = 1;
                long higha = 1 << (lgn / b + 1);
                while (lowa < higha - 1)
                {
                    long mida = (lowa + higha) >> 1;
                    long ab = MachineArithmetic.UnsafePow(mida, b);
                    if (ab > n)
                        higha = mida;
                    else if (ab < n)
                        lowa = mida;
                    else
                    {
                        long[] ipp = PerfectPowerDecomposition(mida);
                        if (ipp != null)
                            return new long[]
                            {
                                ipp[0],
                                ipp[1] * b
                            };
                        else
                            return new long[]
                            {
                                mida,
                                b
                            };
                    }
                }
            }

            return null;
        }
    }
}