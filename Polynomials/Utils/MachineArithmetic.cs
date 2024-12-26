using System.Numerics;

namespace Polynomials.Utils;

public static class MachineArithmetic
{

    public static readonly int MAX_SUPPORTED_MODULUS_BITS = 62;


    public static readonly long MAX_SUPPORTED_MODULUS = (1 << MAX_SUPPORTED_MODULUS_BITS) - 1;


    public static readonly BigInteger b_MAX_SUPPORTED_MODULUS = new BigInteger(MAX_SUPPORTED_MODULUS);

    public static int CompareUnsigned(long x, long y)
    {
        ulong ux = unchecked((ulong)x);
        ulong uy = unchecked((ulong)y);
        return ux < uy ? -1 : (ux > uy ? 1 : 0);
    }

    public static bool Fits32bitWord(long val)
    {
        return CompareUnsigned(val, unchecked((long) ulong.MaxValue)) <= 0;
    }


    public static bool Fits31bitWord(long val)
    {
        return CompareUnsigned(val, long.MaxValue) <= 0;
    }


    public static long SafeMultiply(long x, long y)
    {
        return checked(x*y);
    }


    public static long SafeMultiply(long x, long y, long z)
    {
        return checked(x*y*z);
    }


    public static long SafeAdd(long x, long y)
    {
        return checked(x+y);
    }


    public static long SafeSubtract(long a, long b)
    {
        return checked(a-b);
    }


    public static long SafeNegate(long x)
    {
        return checked(-x);
    }


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
            if (((y != 0) && (r / y != x)) || (x == long.MinValue && y == -1))
            {
                return true;
            }
        }

        return false;
    }


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


    public static long Gcd(long p, long q)
    {
        long u = p;
        long v = q;
        if ((u == 0) || (v == 0))
        {
            if ((u == long.MinValue) || (v == long.MinValue))
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
        } while (t != 0);

        return -u * (1 << k); // gcd is u*2^k
    }


    public static int Gcd(int p, int q)
    {
        int a = p;
        int b = q;
        if (a == 0 || b == 0)
        {
            if (a == int.MinValue || b == int.MinValue)
                throw new ArgumentException("int overflow");
            return Math.Abs(a + b);
        }

        long al = a;
        long bl = b;
        bool useLong = false;
        if (a < 0)
        {
            if (int.MinValue == a)
                useLong = true;
            else
                a = -a;
            al = -al;
        }

        if (b < 0)
        {
            if (int.MinValue == b)
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
                if (bl > int.MaxValue)
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


    private static int GcdPositive(int a, int b)
    {
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
        while (a != b)
        {
            int delta = a - b;
            b = Math.Min(a, b);
            a = Math.Abs(delta);

            // Remove any power of 2 in "a" ("b" is guaranteed to be odd).
            a >>= int.TrailingZeroCount(a);
        }


        // Recover the common power of 2.
        return a << shift;
    }


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


    public static long Lcm(long a, long b)
    {
        if (a == 0 || b == 0)
            return 0;
        return SafeMultiply(a / Gcd(a, b), b);
    }


    public static int Lcm(int a, int b)
    {
        if (a == 0 || b == 0)
            return 0;
        return (a / Gcd(a, b)) * b;
    }


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


    public static long Gcd(params long[] integers)
    {
        return Gcd(integers, 0, integers.Length);
    }


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


    public static long Gcd(params int[] integers)
    {
        return Gcd(integers, 0, integers.Length);
    }


    public static long Mod(long num, long modulus)
    {
        if (num < 0)
            num += modulus; //<- may help
        return (num >= modulus || num < 0) ? num - (num / modulus) * modulus : num;
    }


    public static long SymMod(long value, long modulus)
    {
        value = Mod(value, modulus);
        return value <= modulus / 2 ? value : value - modulus;
    }


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
            throw new ArgumentException(string.Format("modInverse(%s, %s) : not invertible (old_r = %s)", num, modulus,
                old_r));
        return Mod(old_s, modulus);
    }


    public static int SafeToInt(long value)
    {
        if ((int)value != value)
        {
            throw new ArithmeticException("integer overflow: " + value);
        }

        return (int)value;
    }


    public static long PowMod(long @base, long exponent, long modulus)
    {
        return PowModSigned(@base, exponent, FastDivision.MagicSigned(modulus));
    }


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


    public static long[]? PerfectPowerDecomposition(long n)
    {
        if (n < 0)
        {
            var ipp = PerfectPowerDecomposition(-n);
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
                63 - long.LeadingZeroCount(n)
            };
        int lgn = 1 + (64 - (int)long.LeadingZeroCount(n));
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
                    var ipp = PerfectPowerDecomposition(mida);
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
