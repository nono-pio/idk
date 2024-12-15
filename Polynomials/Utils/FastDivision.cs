using System.Diagnostics;

namespace Polynomials.Utils;

public static class FastDivision
{
    public static int CompareUnsigned(long x, long y)
    {
        var ux = unchecked((ulong)x);
        var uy = unchecked((ulong)y);
        return ux < uy ? -1 : (ux > uy ? 1 : 0);
    }

    public static long DivideUnsigned(long dividend, long divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Le diviseur ne peut pas être zéro.");

        // Convertir en non signé
        var uDividend = unchecked((ulong)dividend);
        var uDivisor = unchecked((ulong)divisor);

        // Diviser en tant qu'ulong
        var result = uDividend / uDivisor;

        // Retourner le résultat signé
        return unchecked((long)result);
    }

    public static long MultiplyHighSigned(long x, long y)
    {
        var x_high = x >> 32;
        var x_low = x & 0xFFFFFFFFL;
        var y_high = y >> 32;
        var y_low = y & 0xFFFFFFFFL;

        var z2 = x_low * y_low;
        var t = x_high * y_low + (z2 >>> 32);
        var z1 = t & 0xFFFFFFFFL;
        var z0 = t >> 32;
        z1 += x_low * y_high;
        return x_high * y_high + z0 + (z1 >> 32);
    }

    public static long MultiplyHighUnsigned(long x, long y)
    {
        var x_high = x >>> 32;
        var y_high = y >>> 32;
        var x_low = x & 0xFFFFFFFFL;
        var y_low = y & 0xFFFFFFFFL;

        var z2 = x_low * y_low;
        var t = x_high * y_low + (z2 >>> 32);
        var z1 = t & 0xFFFFFFFFL;
        var z0 = t >>> 32;
        z1 += x_low * y_high;
        return x_high * y_high + z0 + (z1 >>> 32);
    }

    public static long MultiplyLow(long x, long y)
    {
        var n = x * y;
        return n;
    }

    public static long[] DivideAndRemainder128(long u1, long u0, long v)
    {
        var b = (1L << 32); // Number base (16 bits).
        long
            un1,
            un0, // Norm. dividend LSD's.
            vn1,
            vn0, // Norm. divisor digits.
            q1,
            q0, // Quotient digits.
            un64,
            un21,
            un10, // Dividend digit pairs.
            rhat; // A remainder.
        int s; // Shift amount for norm.

        if (u1 >= v) // If overflow, set rem.
            return new long[] { -1L, -1L }; // possible quotient.


        // count leading zeros
        s = (int)long.LeadingZeroCount(v); // 0 <= s <= 63.
        if (s > 0)
        {
            v = v << s; // Normalize divisor.
            un64 = (u1 << s) | ((u0 >>> (64 - s)) & (-s >> 31));
            un10 = u0 << s; // Shift dividend left.
        }
        else
        {
            // Avoid undefined behavior.
            un64 = u1 | u0;
            un10 = u0;
        }

        vn1 = v >>> 32; // Break divisor up into
        vn0 = v & 0xFFFFFFFFL; // two 32-bit digits.

        un1 = un10 >>> 32; // Break right half of
        un0 = un10 & 0xFFFFFFFFL; // dividend into two digits.

        q1 = DivideUnsigned(un64, vn1); // Compute the first
        rhat = un64 - q1 * vn1; // quotient digit, q1.
        while (true)
        {
            if (CompareUnsigned(q1, b) >= 0 || CompareUnsigned(q1 * vn0, b * rhat + un1) > 0)
            {
                //if (q1 >= b || q1 * vn0 > b * rhat + un1) {
                q1 = q1 - 1;
                rhat = rhat + vn1;
                if (CompareUnsigned(rhat, b) < 0)
                    continue;
            }

            break;
        }

        un21 = un64 * b + un1 - q1 * v; // Multiply and subtract.

        q0 = DivideUnsigned(un21, vn1); // Compute the second
        rhat = un21 - q0 * vn1; // quotient digit, q0.
        while (true)
        {
            if (CompareUnsigned(q0, b) >= 0 || CompareUnsigned(q0 * vn0, b * rhat + un0) > 0)
            {
                q0 = q0 - 1;
                rhat = rhat + vn1;
                if (CompareUnsigned(rhat, b) < 0)
                    continue;
            }

            break;
        }

        var r = (un21 * b + un0 - q0 * v) >>> s; // return it.
        return new long[] { q1 * b + q0, r };
    }


    public static Magic MagicUnsigned(long d)
    {
        return MagicUnsigned(d, false);
    }


    public static Magic MagicUnsigned(long d, bool branchfree)
    {
        if (d == 0)
            throw new ArithmeticException("divide by zero");
        // 1 is not supported with branchfree algorithm
        Debug.Assert(!branchfree || d != 1);

        long resultMagic;
        int resultMore;
        var floor_log_2_d = 63 - (int)long.LeadingZeroCount(d);
        if ((d & (d - 1)) == 0)
        {
            // Power of 2
            if (!branchfree)
            {
                resultMagic = 0;
                resultMore = floor_log_2_d | 0x80;
            }
            else
            {
                // We want a magic number of 2**64 and a shift of floor_log_2_d
                // but one of the shifts is taken up by LIBDIVIDE_ADD_MARKER, so we
                // subtract 1 from the shift
                resultMagic = 0;
                resultMore = (floor_log_2_d - 1) | 0x40;
            }
        }
        else
        {
            long proposed_m, rem;
            int more;

            var tmp = DivideAndRemainder128(1L << floor_log_2_d, 0, d); // == (1 << (64 + floor_log_2_d)) / d
            proposed_m = tmp[0];
            rem = tmp[1];

//            assert (rem > 0 && rem < d);
            var e = d - rem;

            // This power works if e < 2**floor_log_2_d.
            if (!branchfree && e < (1L << floor_log_2_d))
            {
                // This power works
                more = floor_log_2_d;
            }
            else
            {
                // We have to use the general 65-bit algorithm.  We need to compute
                // (2**power) / d. However, we already have (2**(power-1))/d and
                // its remainder. By doubling both, and then correcting the
                // remainder, we can compute the larger division.
                // don't care about overflow here - in fact, we expect it
                proposed_m += proposed_m;
                var twice_rem = rem + rem;
                if (twice_rem >= d || twice_rem < rem) proposed_m += 1;
                more = floor_log_2_d | 0x40;
            }

            resultMagic = 1 + proposed_m;
            resultMore = more;
            // result.more's shift should in general be ceil_log_2_d. But if we
            // used the smaller power, we subtract one from the shift because we're
            // using the smaller power. If we're using the larger power, we
            // subtract one from the shift because it's taken care of by the add
            // indicator. So floor_log_2_d happens to be correct in both cases,
            // which is why we do it outside of the if statement.
        }

        return new Magic(resultMagic, resultMore, d);
    }

    public static long DivideUnsignedFast(long dividend, Magic divider)
    {
        var more = divider.more;
        if ((more & 0x80) != 0)
        {
            return dividend >>> (more & 0x3F);
        }
        else
        {
            var q = MultiplyHighUnsigned(divider.magic, dividend);
            if ((more & 0x40) != 0)
            {
                var t = ((dividend - q) >>> 1) + q;
                return t >>> (more & 0x3F);
            }
            else
            {
                return q >>> more; // all upper bits are 0 - don't need to mask them off
            }
        }
    }


    public static Magic MagicSigned(long d)
    {
        return MagicSigned(d, false);
    }

    public static Magic MagicSigned(long d, bool branchfree)
    {
        if (d == 0)
            throw new ArithmeticException("divide by zero");
        Debug.Assert(!branchfree || (d != 1 && d != -1));

        long resultMagic;
        int resultMore;
        // If d is a power of 2, or negative a power of 2, we have to use a shift.
        // This is especially important because the magic algorithm fails for -1.
        // To check if d is a power of 2 or its inverse, it suffices to check
        // whether its absolute value has exactly one bit set.  This works even for
        // INT_MIN, because abs(INT_MIN) == INT_MIN, and INT_MIN has one bit set
        // and is a power of 2.
        var ud = d;
        var absD = (d < 0 ? -ud : ud); // gcc optimizes this to the fast abs trick
        var floor_log_2_d = 63 - (int)long.LeadingZeroCount(absD);
        // check if exactly one bit is set,
        // don't care if absD is 0 since that's divide by zero
        if ((absD & (absD - 1)) == 0)
        {
            // Branchfree and non-branchfree cases are the same
            resultMagic = 0;
            resultMore = floor_log_2_d; //|(d < 0 ? 0x80 : 0);
        }
        else
        {
            // the dividend here is 2**(floor_log_2_d + 63), so the low 64 bit word
            // is 0 and the high word is floor_log_2_d - 1
            int more;
            long rem, proposed_m;
            var tmp = DivideAndRemainder128(1L << (floor_log_2_d - 1), 0, absD);
            proposed_m = tmp[0];
            rem = tmp[1];
            var e = absD - rem;

            // We are going to start with a power of floor_log_2_d - 1.
            // This works if works if e < 2**floor_log_2_d.
            if (!branchfree && e < (1L << floor_log_2_d))
            {
                // This power works
                more = floor_log_2_d - 1;
            }
            else
            {
                // We need to go one higher. This should not make proposed_m
                // overflow, but it will make it negative when interpreted as an
                // int32_t.
                proposed_m += proposed_m;
                var twice_rem = rem + rem;
                if (CompareUnsigned(twice_rem, absD) >= 0 || CompareUnsigned(twice_rem, rem) < 0)
                    proposed_m += 1;
                // note that we only set the LIBDIVIDE_NEGATIVE_DIVISOR bit if we
                // also set ADD_MARKER this is an annoying optimization that
                // enables algorithm #4 to avoid the mask. However we always set it
                // in the branchfree case
                more = floor_log_2_d | 0x40;
            }

            proposed_m += 1;
            var magic = proposed_m;

//            //Mark if we are negative
//            if (d < 0) {
//                more |= 0x80;
//                if (!branchfree) {
//                    magic = -magic;
//                }
//            }

            resultMore = more;
            resultMagic = magic;
        }

        return new Magic(resultMagic, resultMore, d);
    }


    public static long DivideSignedFast(long dividend, Magic divider)
    {
        var more = divider.more;
        var magic = divider.magic;
        if (magic == 0)
        {
            //shift path
            var shifter = more & 0x3F;
            var uq = dividend + ((dividend >> 63) & ((1L << shifter) - 1));
            var q = uq;
            q = q >> shifter;
            // must be arithmetic shift and then sign-extend
            long shiftMask = more >> 7;
            q = (q ^ shiftMask) - shiftMask;
            return divider.divider < 0 ? -q : q;
        }
        else
        {
            var uq = MultiplyHighSigned(magic, dividend);
            if ((more & 0x40) != 0)
            {
                // must be arithmetic shift and then sign extend
                long sign = more >>> 7;
                uq += ((dividend ^ sign) - sign);
            }

            var q = uq;
            q >>= more & 0x3F;
            if (q < 0)
                q += 1;
            return divider.divider < 0 ? -q : q;
        }
    }


    public static long RemainderSignedFast(long dividend, Magic divider)
    {
        var quot = DivideSignedFast(dividend, divider);
        return dividend - quot * divider.divider;
    }


    public static long remainderUnsignedFast(long dividend, Magic divider)
    {
        var quot = DivideUnsignedFast(dividend, divider);
        return dividend - quot * divider.divider;
    }


    public static long FloorDivideFast(long dividend, Magic divider)
    {
        var r = DivideSignedFast(dividend, divider);
        // if the signs are different and modulo not zero, round down
        if ((dividend ^ divider.divider) < 0 && (r * divider.divider != dividend))
        {
            r--;
        }

        return r;
    }

    public static long ModSignedFast(long dividend, Magic divider)
    {
        var div = DivideSignedFast(dividend, divider);
        var m = dividend - div * divider.divider;
        if (m < 0)
            m += divider.divider;
        return m;
    }


    public static long ModUnsignedFast(long dividend, Magic divider)
    {
        return dividend - DivideUnsignedFast(dividend, divider) * divider.divider;
    }

    public static Magic Magic32ForMultiplyMod(long divider)
    {
        var v = divider;
        var s = (int)long.LeadingZeroCount(v); // 0 <= s <= 63.
        if (s > 0)
            v = v << s;
        return MagicUnsigned(v >>> 32);
    }


    public static long MultiplyMod128Unsigned(long a, long b, long divider, Magic magic32)
    {
        return MultiplyMod128Unsigned0(MultiplyHighUnsigned(a, b), MultiplyLow(a, b), divider, magic32);
    }


    public static long MultiplyMod128Unsigned0(long high, long low, long divider, Magic magic32)
    {
        var b = (1L << 32); // Number base (16 bits).
        long
            un1,
            un0, // Norm. dividend LSD's.
            vn1,
            vn0, // Norm. divisor digits.
            q1,
            q0, // Quotient digits.
            un64,
            un21,
            un10, // Dividend digit pairs.
            rhat; // A remainder.
        int s; // Shift amount for norm.

        if (high >= divider) // If overflow, set rem.
            throw new ArgumentException();


        // count leading zeros
        s = (int)long.LeadingZeroCount(divider); // 0 <= s <= 63.
        if (s > 0)
        {
            divider = divider << s; // Normalize divisor.
            un64 = (high << s) | ((low >>> (64 - s)) & (-s >> 31));
            un10 = low << s; // Shift dividend left.
        }
        else
        {
            // Avoid undefined behavior.
            un64 = high | low;
            un10 = low;
        }

        vn1 = divider >>> 32; // Break divisor up into
        vn0 = divider & 0xFFFFFFFFL; // two 32-bit digits.

        un1 = un10 >>> 32; // Break right half of
        un0 = un10 & 0xFFFFFFFFL; // dividend into two digits.

        q1 = DivideUnsignedFast(un64, magic32); // Compute the first
        rhat = un64 - q1 * vn1; // quotient digit, q1.
        while (true)
        {
            if (CompareUnsigned(q1, b) >= 0 || CompareUnsigned(q1 * vn0, b * rhat + un1) > 0)
            {
                //if (q1 >= b || q1 * vn0 > b * rhat + un1) {
                q1 = q1 - 1;
                rhat = rhat + vn1;
                if (CompareUnsigned(rhat, b) < 0)
                    continue;
            }

            break;
        }

        un21 = un64 * b + un1 - q1 * divider; // Multiply and subtract.

        q0 = DivideUnsignedFast(un21, magic32); // Compute the second
        rhat = un21 - q0 * vn1; // quotient digit, q0.
        while (true)
        {
            if (CompareUnsigned(q0, b) >= 0 || CompareUnsigned(q0 * vn0, b * rhat + un0) > 0)
            {
                q0 = q0 - 1;
                rhat = rhat + vn1;
                if (CompareUnsigned(rhat, b) < 0)
                    continue;
            }

            break;
        }

        var r = (un21 * b + un0 - q0 * divider) >>> s; // return it.
        return r;
    }


    public sealed class Magic
    {
        public readonly long magic;
        public readonly int more;
        public readonly long divider;

        public Magic(long magic, int more, long divider)
        {
            this.magic = magic;
            this.more = more;
            this.divider = divider;
        }
    }
}