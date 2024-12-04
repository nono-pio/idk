using System.Numerics;

namespace Rings;

public sealed class IntegersZp64
{
    private static readonly long serialVersionUID = 1L;

    public readonly long Modulus;

    public readonly Magic magic, magic32MulMod;

    public readonly bool ModulusFits32;

    public IntegersZp64(long Modulus, Magic magic, Magic magic32MulMod, bool ModulusFits32)
    {
        this.Modulus = Modulus;
        this.magic = magic;
        this.magic32MulMod = magic32MulMod;
        this.ModulusFits32 = ModulusFits32;
    }

    public IntegersZp64(long Modulus) : this(Modulus, magicSigned(Modulus), magic32ForMultiplyMod(Modulus), MachineArithmetic.fits31bitWord(Modulus))
    {
    }

    public long modulus(long val)
    {
        return modSignedFast(val, magic);
    }

    public long modulus(BigInteger val)
    {
        return val.isLong() ? modSignedFast(val.longValue(), magic) : (val % new BigInteger(Modulus)).longValue();
    }


    public void modulus(long[] data)
    {
        for (int i = 0; i < data.length; ++i)
            data[i] = modulus(data[i]);
    }


    public long multiply(long a, long b)
    {
        return ModulusFits32 ? modulus(a * b) : multiplyMod128Unsigned(a, b, Modulus, magic32MulMod);
    }


    public long add(long a, long b)
    {
        long r = a + b;
        return r - Modulus >= 0 ? r - Modulus : r;
    }


    public long subtract(long a, long b)
    {
        long r = a - b;
        return r + ((r >> 63) & Modulus);
    }


    public long divide(long a, long b)
    {
        return multiply(a, reciprocal(b));
    }


    private volatile int[] cachedReciprocals = null;


    public void buildCachedReciprocals()
    {
        if (cachedReciprocals != null)
            return;
        synchronized(this) {
            if (cachedReciprocals == null)
            {
                int[] cachedReciprocals = new int[MachineArithmetic.safeToInt(Modulus)];
                for (int val = 1; val < cachedReciprocals.Length; ++val)
                    cachedReciprocals[val] = (int)MachineArithmetic.modInverse(val, Modulus);
                this.cachedReciprocals = cachedReciprocals;
            }
        }
    }


    public long reciprocal(long val)
    {
        return cachedReciprocals == null
            ? MachineArithmetic.modInverse(val, Modulus)
            : (val < cachedReciprocals.Length
                ? cachedReciprocals[(int)val]
                : MachineArithmetic.modInverse(val, Modulus));
    }


    public long negate(long val)
    {
        return val == 0 ? val : Modulus - val;
    }


    public long symmetricForm(long value)
    {
        return value <= Modulus / 2 ? value : value - Modulus;
    }


    public IntegersZp asGenericRing()
    {
        return new IntegersZp(Modulus);
    }


    public long powMod(readonly long @base, long exponent)
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
                result = multiply(result, k2p);
            exponent = exponent >> 1;
            if (exponent == 0)
                return result;
            k2p = multiply(k2p, k2p);
        }
    }

    public long randomElement(RandomGenerator rnd)
    {
        return modulus(rnd.nextLong());
    }

    public long randomElement()
    {
        return randomElement(new Well44497b(DateTime.Now.Nanosecond));
    }

    public long randomNonZeroElement(RandomGenerator rnd)
    {
        long el;
        do
        {
            el = randomElement(rnd);
        } while (el == 0);

        return el;
    }

    public long factorial(int value)
    {
        long result = 1;
        for (int i = 2; i <= value; ++i)
            result = multiply(result, i);
        return result;
    }


    private readonly long[] perfectPowerDecomposition = { -1, -1 };

    private void checkPerfectPower()
    {
        // lazy initialization
        if (perfectPowerDecomposition[0] == -1)
        {
            synchronized(perfectPowerDecomposition) {
                if (perfectPowerDecomposition[0] != -1)
                    return;

                long[] ipp = MachineArithmetic.perfectPowerDecomposition(Modulus);
                if (ipp == null)
                {
                    // not a perfect power
                    perfectPowerDecomposition[0] = Modulus;
                    perfectPowerDecomposition[1] = 1;
                    return;
                }

                perfectPowerDecomposition[0] = ipp[0];
                perfectPowerDecomposition[1] = ipp[1];
            }
        }
    }


    public bool isPerfectPower()
    {
        return perfectPowerExponent() > 1;
    }


    public long perfectPowerBase()
    {
        checkPerfectPower();
        return perfectPowerDecomposition[0];
    }


    public long perfectPowerExponent()
    {
        checkPerfectPower();
        return perfectPowerDecomposition[1];
    }


    private IntegersZp64 ppBaseDomain = null;

    public IntegersZp64 perfectPowerBaseDomain()
    {
        if (ppBaseDomain == null)
        {
            synchronized(this) {
                if (ppBaseDomain == null)
                {
                    long @base = perfectPowerBase();
                    if (@base == -1)
                        ppBaseDomain = this;
                    else
                        ppBaseDomain = new IntegersZp64(@base);
                }
            }
        }

        return ppBaseDomain;
    }


    public bool equals(Object o)
    {
        if (this == o) return true;
        if (o == null || GetType() != o.GetType()) return false;

        IntegersZp64 that = (IntegersZp64)o;

        return Modulus == that.Modulus;
    }


    public String toString()
    {
        return "Z/" + Modulus;
    }


    public int hashCode()
    {
        return (int)(Modulus ^ (Modulus >>> 32));
    }
}