using System.Numerics;
using Polynomials.Utils;
using static Polynomials.Utils.FastDivision;

namespace Polynomials;

public class IntegersZp64 : Ring<long>
{
    private static readonly long serialVersionUID = 1;
    
    public readonly long modulus;
    
    public readonly Magic magic, magic32MulMod;
    
    public readonly bool modulusFits32;
    
    public IntegersZp64(long modulus, Magic magic, Magic magic32MulMod, bool modulusFits32)
    {
        this.modulus = modulus;
        this.magic = magic;
        this.magic32MulMod = magic32MulMod;
        this.modulusFits32 = modulusFits32;
    }


    public IntegersZp64(long modulus) : this(modulus, MagicSigned(modulus), Magic32ForMultiplyMod(modulus),
        MachineArithmetic.Fits31bitWord(modulus))
    {
    }


    public long Modulus(long val)
    {
        return ModSignedFast(val, magic);
    }


    public long Modulus(BigInteger val)
    {
        return val.IsLong()
            ? ModSignedFast((long)val, magic)
            : (long)(val % modulus);
    }


    public void Modulus(long[] data)
    {
        for (int i = 0; i < data.Length; ++i)
            data[i] = Modulus(data[i]);
    }

    public override long Add(long a, long b)
    {
        long r = a + b;
        return r - modulus >= 0 ? r - modulus : r;
    }
    
    public override long Subtract(long a, long b)
    {
        long r = a - b;
        return r < 0 ? r + modulus : r;
    }

    public override long MultiplyLong(long a, long b)
    {
        return modulusFits32 ? Modulus(a * b) : MultiplyMod128Unsigned(a, b, modulus, magic32MulMod);
    }

    public override long Multiply(long a, long b)
    {
        return modulusFits32 ? Modulus(a * b) : MultiplyMod128Unsigned(a, b, modulus, magic32MulMod);
    }

    public long Divide(long a, long b)
    {
        return Multiply(a, Reciprocal(b));
    }


    private volatile int[]? cachedReciprocals = null;


    public void BuildCachedReciprocals()
    {
        if (cachedReciprocals != null)
            return;
        lock (this)
        {
            if (cachedReciprocals == null)
            {
                int[] cachedReciprocals = new int[MachineArithmetic.SafeToInt(modulus)];
                for (int val = 1; val < cachedReciprocals.Length; ++val)
                    cachedReciprocals[val] = (int)MachineArithmetic.ModInverse(val, modulus);
                this.cachedReciprocals = cachedReciprocals;
            }
        }
    }


    public override long Reciprocal(long val)
    {
        return cachedReciprocals == null
            ? MachineArithmetic.ModInverse(val, modulus)
            : (val < cachedReciprocals.Length
                ? cachedReciprocals[(int)val]
                : MachineArithmetic.ModInverse(val, modulus));
    }

    public override long GetZero()
    {
        return 0;
    }

    public override long GetOne()
    {
        return 1;
    }

    public override bool IsZero(long element)
    {
        return element == 0;
    }

    public override bool IsOne(long element)
    {
        return element == 1;
    }

    public override bool IsUnit(long element)
    {
        return element != 0 && modulus % element != 0;
    }


    public override long ValueOfLong(long val)
    {
        return Modulus(val);
    }
    
    public override long ValueOf(long val)
    {
        return Modulus(val);
    }

    public override long ValueOfBigInteger(BigInteger val)
    {
        return Modulus(val);
    }


    public override long Negate(long val)
    {
        return val == 0 ? val : modulus - val;
    }

    public override long Copy(long element)
    {
        return element;
    }

    public override bool Equal(long x, long y)
    {
        return x == y;
    }

    public override int Compare(long x, long y)
    {
        return x.CompareTo(y);
    }

    public override object Clone()
    {
        return new IntegersZp64(modulus, magic, magic32MulMod, modulusFits32);
    }

    public override long[]? DivideAndRemainder(long dividend, long divider)
    {
        return new long[] { Divide(dividend, divider), 0 };
    }


    public long SymmetricForm(long value)
    {
        return value <= modulus / 2 ? value : value - modulus;
    }


    public IntegersZp AsGenericRing()
    {
        return new IntegersZp(modulus);
    }


    public long PowMod(long @base, long exponent)
    {
        if (exponent < 0)
            throw new ArgumentException();
        if (exponent == 0)
            return 1;
        long result = 1;
        long k2p = @base;
        for (;;)
        {
            if ((exponent & 1) != 0)
                result = Multiply(result, k2p);
            exponent = exponent >> 1;
            if (exponent == 0)
                return result;
            k2p = Multiply(k2p, k2p);
        }
    }


    public override long RandomElement(Random rnd)
    {
        return Modulus(rnd.NextInt64());
    }


    public override IEnumerator<long> Iterator()
    {
        long val = 0;
        while (val < modulus)
        {
            yield return val;
            val++;
        }
    }

    public override long RandomElement()
    {
        return RandomElement(new Random(DateTime.Now.Nanosecond));
    }


    public override long RandomNonZeroElement(Random rnd)
    {
        long el;
        do
        {
            el = RandomElement(rnd);
        } while (el == 0);

        return el;
    }


    public long Factorial(int value)
    {
        long result = 1;
        for (int i = 2; i <= value; ++i)
            result = Multiply(result, i);
        return result;
    }


    private readonly long[] perfectPowerDecomposition = new long[]
    {
        -1,
        -1
    };


    private void CheckPerfectPower()
    {
        // lazy initialization
        if (perfectPowerDecomposition[0] == -1)
        {
            lock (perfectPowerDecomposition)
            {
                if (perfectPowerDecomposition[0] != -1)
                    return;
                var ipp = MachineArithmetic.PerfectPowerDecomposition(modulus);
                if (ipp is null)
                {
                    // not a perfect power
                    perfectPowerDecomposition[0] = modulus;
                    perfectPowerDecomposition[1] = 1;
                    return;
                }

                perfectPowerDecomposition[0] = ipp[0];
                perfectPowerDecomposition[1] = ipp[1];
            }
        }
    }


    public override bool IsField()
    {
        return true;
    }

    public override bool IsEuclideanRing()
    {
        return true;
    }

    public override BigInteger? Cardinality()
    {
        return modulus;
    }

    public override BigInteger Characteristic()
    {
        return modulus;
    }

    public override bool IsPerfectPower()
    {
        return PerfectPowerExponent() > 1;
    }


    public new long PerfectPowerBase()
    {
        CheckPerfectPower();
        return perfectPowerDecomposition[0];
    }


    public new long PerfectPowerExponent()
    {
        CheckPerfectPower();
        return perfectPowerDecomposition[1];
    }


    private IntegersZp64? ppBaseDomain = null;


    public IntegersZp64 PerfectPowerBaseDomain()
    {
        if (ppBaseDomain == null)
        {
            lock (this)
            {
                if (ppBaseDomain == null)
                {
                    long @base = PerfectPowerBase();
                    if (@base == -1)
                        ppBaseDomain = this;
                    else
                        ppBaseDomain = new IntegersZp64(@base);
                }
            }
        }

        return ppBaseDomain;
    }


    public override bool Equals(object? o)
    {
        if (this == o)
            return true;
        if (o == null || GetType() != o.GetType())
            return false;
        IntegersZp64 that = (IntegersZp64)o;
        return modulus == that.modulus;
    }


    public override string ToString()
    {
        return "Z/" + modulus;
    }


    public override int GetHashCode()
    {
        return (int)(modulus ^ (modulus >>> 32));
    }
}