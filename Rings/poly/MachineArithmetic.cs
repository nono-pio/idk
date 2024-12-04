using System.Diagnostics;
using System.Numerics;

namespace Rings.poly;

/**
 * Helper methods for arithmetic with machine numbers.
 *
 * @since 1.0
 */
public sealed class MachineArithmetic {
    private MachineArithmetic() {}

    /** Max supported modulus bits which fits into machine word */
    public static readonly int MAX_SUPPORTED_MODULUS_BITS = 62;
    /** Max supported modulus which fits into machine word */
    public static readonly long MAX_SUPPORTED_MODULUS = (1L << MAX_SUPPORTED_MODULUS_BITS) - 1L;
    /** Max supported modulus */
    public static readonly BigInteger b_MAX_SUPPORTED_MODULUS = new BigInteger(MAX_SUPPORTED_MODULUS);

    /**
     * Returns true if {@code val} fits into 32-bit machine word (unsigned) and false otherwise
     *
     * @param val the value
     * @return true if {@code val} fits into 32-bit machine word (unsigned) and false otherwise
     */
    public static bool fits32bitWord(long val) {
        return val.CompareTo((1L << 32) - 1) <= 0;
    }

    /**
     * Returns true if {@code val} fits into 32-bit machine word (unsigned) and false otherwise
     *
     * @param val the value
     * @return true if {@code val} fits into 32-bit machine word (unsigned) and false otherwise
     */
    public static bool fits31bitWord(long val) {
        return val.CompareTo((1L << 31) - 1) <= 0;
    }

    /**
     * Delegates to {@link Math#multiplyExact(long, long)}
     *
     * @throws ArithmeticException if the result overflows a long
     **/
    public static long safeMultiply(long x, long y)
    {
        return checked(x * y);

    }

    /**
     * Delegates to {@link Math#multiplyExact(long, long)}
     *
     * @throws ArithmeticException if the result overflows a long
     **/
    public static long safeMultiply(long x, long y, long z) {
        return checked(x * y * z);
    }

    /**
     * Delegates to {@link Math#addExact(long, long)}
     *
     * @throws ArithmeticException if the result overflows a long
     **/
    public static long safeAdd(long x, long y) {
        return checked(x + y);

    }

    /**
     * Delegates to {@link Math#subtractExact(long, long)}
     *
     * @throws ArithmeticException if the result overflows a long
     **/
    public static long safeSubtract(long a, long b) {
        return checked(a - b);

    }

    /**
     * Delegates to {@link Math#negateExact(long)}
     *
     * @throws ArithmeticException if the result overflows a long
     **/
    public static long safeNegate(long x)
    {
        return checked(-x);
    }

    /**
     * Tests whether the multiplication of {@code x*y} will cause long overflow
     */
    public static bool isOverflowMultiply(long x, long y) {
        long r = x * y;
        long ax = Math.Abs(x);
        long ay = Math.Abs(y);
        if (((ax | ay) >>> 31 != 0)) {
            // Some bits greater than 2^31 that might cause overflow
            // Check the result using the divide operator
            // and check for the special case of Long.MIN_VALUE * -1
            if (((y != 0) && (r / y != x)) ||
                    (x == long.MinValue && y == -1)) {
                return true;
            }
        }
        return false;
    }

    /**
     * Tests whether the addition of {@code x + y} will cause long overflow
     */
    public static bool isOverflowAdd(long x, long y) {
        long r = x + y;
        // HD 2-12 Overflow iff both arguments have the opposite sign of the result
        if (((x ^ r) & (y ^ r)) < 0) {
            return true;
        }
        return false;
    }

    /**
     * Returns {@code base} in a power of {@code e} (non negative)
     *
     * @param base     base
     * @param exponent exponent (non negative)
     * @return {@code base} in a power of {@code e}
     * @throws ArithmeticException if the result overflows a long
     */
    public static long safePow(long @base, long exponent) {
        if (exponent < 0)
            throw new ArgumentException();

        long result = 1L;
        long k2p = @base;
        for (; ; ) {
            if ((exponent & 1) != 0)
                result = safeMultiply(result, k2p);
            exponent = exponent >> 1;
            if (exponent == 0)
                return result;
            k2p = safeMultiply(k2p, k2p);
        }
    }

    /**
     * Returns {@code base} in a power of {@code e} (non negative)
     *
     * @param base     base
     * @param exponent exponent (non negative)
     * @return {@code base} in a power of {@code e}
     */
    public static long unsafePow(long @base, long exponent) {
        if (exponent < 0)
            throw new ArgumentException();

        long result = 1L;
        long k2p = @base;
        for (; ; ) {
            if ((exponent & 1) != 0)
                result *= k2p;
            exponent = exponent >> 1;
            if (exponent == 0)
                return result;
            k2p *= k2p;
        }
    }

    /**
     * Returns the greatest common divisor of two longs.
     *
     * @param p a long
     * @param q a long
     * @return greatest common divisor of {@code a} and {@code b}
     */
    public static long gcd(long p, long q) {
        long u = p;
        long v = q;
        if ((u == 0) || (v == 0)) {
            if ((u == long.MinValue) || (v == long.MinValue))
                throw new ArgumentException("long overflow");
            return Math.Abs(u) + Math.Abs(v);
        }
        // keep u and v negative, as negative integers range down to
        // -2^63, while positive numbers can only be as large as 2^63-1
        // (i.e. we can't necessarily negate a negative number without
        // overflow)
        /* assert u!=0 && v!=0; */
        if (u > 0) {
            u = -u;
        } // make u negative
        if (v > 0) {
            v = -v;
        } // make v negative
        // B1. [Find power of 2]
        int k = 0;
        while ((u & 1) == 0 && (v & 1) == 0 && k < 63) { // while u and v are
            // both even...
            u /= 2;
            v /= 2;
            k++; // cast out twos.
        }
        if (k == 63) {
            throw new ArgumentException("Overflow");
        }
        // B2. Initialize: u and v have been divided by 2^k and at least
        // one is odd.
        long t = ((u & 1) == 1) ? v : -(u / 2)/* B3 */;
        // t negative: u was odd, v may be even (t replaces v)
        // t positive: u was even, v is odd (t replaces u)
        do {
            /* assert u<0 && v<0; */
            // B4/B3: cast out twos from t.
            while ((t & 1) == 0) { // while t is even..
                t /= 2; // cast out twos
            }
            // B5 [reset max(u,v)]
            if (t > 0) {
                u = -t;
            } else {
                v = t;
            }
            // B6/B3. at this point both u and v should be odd.
            t = (v - u) / 2;
            // |u| larger: t positive (replace u)
            // |v| larger: t negative (replace v)
        } while (t != 0);
        return -u * (1L << k); // gcd is u*2^k
    }

    /**
     * Computes the greatest common divisor of the absolute value of two numbers, using a modified version of the
     * "binary gcd" method. See Knuth 4.5.2 algorithm B. The algorithm is due to Josef Stein (1961). <br/> Special
     * cases: <ul> <li>The invocations {@code gcd(Integer.MIN_VALUE, Integer.MIN_VALUE)}, {@code gcd(Integer.MIN_VALUE,
     * 0)} and {@code gcd(0, Integer.MIN_VALUE)} throw an {@code ArithmeticException}, because the result would be 2^31,
     * which is too large for an int value.</li> <li>The result of {@code gcd(x, x)}, {@code gcd(0, x)} and {@code
     * gcd(x, 0)} is the absolute value of {@code x}, except for the special cases above.</li> <li>The invocation {@code
     * gcd(0, 0)} is the only one which returns {@code 0}.</li> </ul>
     *
     * @param p Number.
     * @param q Number.
     * @return the greatest common divisor (never negative).
     */
    public static int gcd(int p, int q) {
        int a = p;
        int b = q;
        if (a == 0 || b == 0) {
            if (a == int.MinValue || b == int.MinValue)
                throw new ArgumentException("int overflow");
            return Math.Abs(a + b);
        }

        long al = a;
        long bl = b;
        bool useLong = false;
        if (a < 0) {
            if (int.MinValue == a)
                useLong = true;
            else
                a = -a;
            al = -al;
        }
        if (b < 0) {
            if (int.MaxValue == b)
                useLong = true;
            else
                b = -b;
            bl = -bl;
        }
        if (useLong) {
            if (al == bl)
                throw new ArgumentException("int overflow");

            long blbu = bl;
            bl = al;
            al = blbu % al;
            if (al == 0) {
                if (bl > int.MaxValue)
                    throw new ArgumentException("int overflow");
                return (int) bl;
            }
            blbu = bl;

            // Now "al" and "bl" fit in an "int".
            b = (int) al;
            a = (int) (blbu % al);
        }

        return gcdPositive(a, b);
    }

    /**
     * Computes the greatest common divisor of two <em>positive</em> numbers (this precondition is <em>not</em> checked
     * and the result is undefined if not fulfilled) using the "binary gcd" method which avoids division and modulo
     * operations. See Knuth 4.5.2 algorithm B. The algorithm is due to Josef Stein (1961). <br/> Special cases: <ul>
     * <li>The result of {@code gcd(x, x)}, {@code gcd(0, x)} and {@code gcd(x, 0)} is the value of {@code x}.</li>
     * <li>The invocation {@code gcd(0, 0)} is the only one which returns {@code 0}.</li> </ul>
     *
     * @param a Positive number.
     * @param b Positive number.
     * @return the greatest common divisor.
     */
    private static int gcdPositive(int a, int b) {
        if (a == 0)
            return b;

        else if (b == 0)
            return a;

        // Make "a" and "b" odd, keeping track of common power of 2.
        int aTwos = int.TrailingZeroCount(a);
        a >>= aTwos;
        int bTwos = int.TrailingZeroCount(b);
        b >>= bTwos;
        int shift = Math.Min(aTwos, bTwos);

        // "a" and "b" are positive.
        // If a > b then "gdc(a, b)" is equal to "gcd(a - b, b)".
        // If a < b then "gcd(a, b)" is equal to "gcd(b - a, a)".
        // Hence, in the successive iterations:
        //  "a" becomes the absolute difference of the current values,
        //  "b" becomes the minimum of the current values.
        while (a != b) {
            int delta = a - b;
            b = Math.Min(a, b);
            a = Math.Abs(delta);

            // Remove any power of 2 in "a" ("b" is guaranteed to be odd).
            a >>= int.TrailingZeroCount(a);
        }

        // Recover the common power of 2.
        return a << shift;
    }


    /**
     * Runs extended Euclidean algorithm to compute {@code [gcd(a,b), x, y]} such that {@code x * a + y * b = gcd(a,
     * b)}
     *
     * @param a a long
     * @param b a long
     * @return array of {@code [gcd(a,b), x, y]} such that {@code x * a + y * b = gcd(a, b)}
     */
    public static long[] gcdExtended(long a, long b) {
        long s = 0, old_s = 1;
        long t = 1, old_t = 0;
        long r = b, old_r = a;

        long q;
        long tmp;
        while (r != 0) {
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
        Debug.Assert(old_r == a * old_s + b * old_t);
        return new long[]{old_r, old_s, old_t};
    }

    /**
     * Returns the least common multiple of two longs
     *
     * @param a a long
     * @param b a long
     * @return least common multiple of {@code a} and {@code b}
     * @throws ArithmeticException if the result overflows a long
     */
    public static long lcm(long a, long b) {
        if (a == 0 || b == 0)
            return 0;
        return safeMultiply(a / gcd(a, b), b);
    }

    /**
     * Returns the least common multiple of two integers
     *
     * @param a a number
     * @param b a number
     * @return least common multiple of {@code a} and {@code b}
     */
    public static int lcm(int a, int b) {
        if (a == 0 || b == 0)
            return 0;
        return (a / gcd(a, b)) * b;
    }

    /**
     * Returns the greatest common an array of longs
     *
     * @param integers array of longs
     * @param from     from position (inclusive)
     * @param to       to position (exclusive)
     * @return greatest common divisor of array
     */
    public static long gcd(long[] integers, int from, int to) {
        if (integers.Length < 2)
            throw new ArgumentException();
        long _gcd = gcd(integers[from], integers[from + 1]);
        if (_gcd == 1)
            return _gcd;
        for (int i = from + 2; i < to; i++) {
            _gcd = gcd(integers[i], _gcd);
            if (_gcd == 1)
                return _gcd;
        }
        return _gcd;
    }

    /**
     * Returns the greatest common an array of longs
     *
     * @param integers array of longs
     * @return greatest common divisor of array
     */
    public static long gcd(params long[] integers) {
        return gcd(integers, 0, integers.Length);
    }

    /**
     * Returns the greatest common an array of integers
     *
     * @param integers array of integers
     * @param from     from position (inclusive)
     * @param to       to position (exclusive)
     * @return greatest common divisor of array
     */
    public static long gcd(int[] integers, int from, int to) {
        if (integers.Length < 2)
            throw new ArgumentException();
        long _gcd = gcd(integers[from], integers[from + 1]);
        if (_gcd == 1)
            return _gcd;
        for (int i = from + 2; i < to; i++) {
            _gcd = gcd(integers[i], _gcd);
            if (_gcd == 1)
                return _gcd;
        }
        return _gcd;
    }

    /**
     * Returns the greatest common an array of integers
     *
     * @param integers array of integers
     * @return greatest common divisor of array
     */
    public static long gcd(params int[] integers) {
        return gcd(integers, 0, integers.Length);
    }

    /** Delegates to {@link Math#floorMod(long, long)} */
    public static long mod(long num, long modulus) {
        if (num < 0)
            num += modulus; //<- may help
        return (num >= modulus || num < 0) ? num - (long)Math.Floor((double)num / modulus) * modulus : num;
    }

    /**
     * Returns {@code value mod modulus} in the symmetric representation ({@code -modulus/2 <= result <= modulus/2})
     *
     * @param value   a long
     * @param modulus modulus
     * @return {@code value mod modulus} in the symmetric representation ({@code -modulus/2 <= result <= modulus/2})
     */
    public static long symMod(long value, long modulus) {
        value = mod(value, modulus);
        return value <= modulus / 2 ? value : value - modulus;
    }

    /**
     * Returns a solution of congruence {@code num * x = 1 mod modulus}
     *
     * @param num     base
     * @param modulus modulus
     * @return {@code a^(-1) mod p}
     * @throws IllegalArgumentException {@code a} and {@code modulus} are not coprime
     */
    public static long modInverse(long num, long modulus) {
        if (num == 1)
            return num;
        if (num < 0)
            num = mod(num, modulus);

        long s = 0, old_s = 1;
        long r = modulus, old_r = num;

        long q;
        long tmp;
        while (r != 0) {
            q = old_r / r;

            tmp = old_r;
            old_r = r;
            r = tmp - q * r;

            tmp = old_s;
            old_s = s;
            s = tmp - q * s;
        }

        if (old_r != 1)
            throw new ArgumentException(
                    string.Format("modInverse(%s, %s) : not invertible (old_r = %s)", num, modulus, old_r));
        return mod(old_s, modulus);
    }

    /**
     * Casts {@code long} to signed {@code int} throwing exception in case of overflow.
     *
     * @param value the long
     * @return int value
     * @throws ArithmeticException if the result overflows a long
     */
    public static int safeToInt(long value) {
        if ((int) value != value) {
            throw new ArithmeticException("integer overflow: " + value);
        }
        return (int) value;
    }

    /**
     * Returns {@code base} in a power of non-negative {@code e} modulo {@code modulus}
     *
     * @param base     the base
     * @param exponent the non-negative exponent
     * @param modulus  the modulus
     * @return {@code base} in a power of {@code e}
     */
    public static long powMod(long @base, long exponent, long modulus) {
        return powModSigned(@base, exponent, FastDivision.magicSigned(modulus));
    }

    /**
     * Returns {@code base} in a power of non-negative {@code e} modulo {@code magic.modulus}
     *
     * @param base     the base
     * @param exponent the non-negative exponent
     * @param magic    magic modulus
     * @return {@code base} in a power of {@code e}
     */
    public static long powModSigned(long @base, long exponent, FastDivision.Magic magic) {
        if (exponent < 0)
            throw new ArgumentException();
        if (exponent == 0)
            return 1;

        long result = 1;
        long k2p = FastDivision.modSignedFast(@base, magic);
        for (; ; ) {
            if ((exponent & 1) != 0)
                result = FastDivision.modSignedFast(result * k2p, magic);
            exponent = exponent >> 1;
            if (exponent == 0)
                return result;
            k2p = FastDivision.modSignedFast(k2p * k2p, magic);
        }
    }

    /**
     * Returns {@code base} in a power of non-negative {@code e} modulo {@code magic.modulus}
     *
     * @param base     the base
     * @param exponent the non-negative exponent
     * @param magic    magic modulus
     * @return {@code base} in a power of {@code e}
     */
    public static long powModUnsigned(long @base, long exponent, FastDivision.Magic magic) {
        if (exponent < 0)
            throw new ArgumentException();
        if (exponent == 0)
            return 1;

        long result = 1;
        long k2p = FastDivision.modUnsignedFast(@base, magic);
        for (; ; ) {
            if ((exponent & 1) != 0)
                result = FastDivision.modUnsignedFast(result * k2p, magic);
            exponent = exponent >> 1;
            if (exponent == 0)
                return result;
            k2p = FastDivision.modUnsignedFast(k2p * k2p, magic);
        }
    }

    /**
     * Tests whether {@code n} is a perfect power {@code n == a^b} and returns {@code {a, b}} if so and {@code null}
     * otherwise
     *
     * @param n the number
     * @return array {@code {a, b}} so that {@code n = a^b} or {@code null} is {@code n} is not a perfect power
     */
    public static long[] perfectPowerDecomposition(long n) {
        if (n < 0) {
            long[] ipp = perfectPowerDecomposition(-n);
            if (ipp == null)
                return null;
            if (ipp[1] % 2 == 0)
                return null;
            ipp[0] = -ipp[0];
            return ipp;
        }

        if ((-n & n) == n)
            return new long[]{2, 63 - long.LeadingZeroCount(n)};

        int lgn = 1 + (64 - (int)long.LeadingZeroCount(n));
        for (int b = 2; b < lgn; b++) {
            //b lg a = lg n
            long lowa = 1L;
            long higha = 1L << (lgn / b + 1);
            while (lowa < higha - 1) {
                long mida = (lowa + higha) >> 1;
                long ab = MachineArithmetic.unsafePow(mida, b);
                if (ab > n)
                    higha = mida;
                else if (ab < n)
                    lowa = mida;
                else {
                    long[] ipp = perfectPowerDecomposition(mida);
                    if (ipp != null)
                        return new long[]{ipp[0], ipp[1] * b};
                    else
                        return new long[]{mida, b};
                }
            }
        }
        return null;
    }
}
