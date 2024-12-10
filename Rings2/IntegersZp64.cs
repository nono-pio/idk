using System.Numerics;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Poly;


namespace Cc.Redberry.Rings
{
    /// <summary>
    /// Zp ring over machine numbers which provides fast modular arithmetic.
    /// </summary>
    /// <remarks>
    /// @seecc.redberry.libdivide4j.FastDivision
    /// @since1.0
    /// </remarks>
    public sealed class IntegersZp64
    {
        private static readonly long serialVersionUID = 1;

        /// <summary>
        /// the modulus
        /// </summary>
        public readonly long modulus;

        /// <summary>
        /// magic *
        /// </summary>
        public readonly Magic magic, magic32MulMod;


        /// <summary>
        /// whether modulus less then 2^32 (if so, faster mulmod available) *
        /// </summary>
        public readonly bool modulusFits32;


        public IntegersZp64(long modulus, Magic magic, Magic magic32MulMod, bool modulusFits32)
        {
            this.modulus = modulus;
            this.magic = magic;
            this.magic32MulMod = magic32MulMod;
            this.modulusFits32 = modulusFits32;
        }


        /// <summary>
        /// Creates the ring.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        public IntegersZp64(long modulus) : this(modulus, MagicSigned(modulus), Magic32ForMultiplyMod(modulus),
            MachineArithmetic.Fits31bitWord(modulus))
        {
        }


        /// <summary>
        /// Returns {@code val % this.modulus}
        /// </summary>
        public long Modulus(long val)
        {
            return ModSignedFast(val, magic);
        }


        /// <summary>
        /// Returns {@code val % this.modulus}
        /// </summary>
        public long Modulus(BigInteger val)
        {
            return val.IsLong()
                ? ModSignedFast(val.LongValue(), magic)
                : val.Mod(new BigInteger(modulus)).LongValue();
        }


        /// <summary>
        /// Inplace sets elements of {@code data} to {@code data % this.modulus}
        /// </summary>
        public void Modulus(long[] data)
        {
            for (int i = 0; i < data.Length; ++i)
                data[i] = Modulus(data[i]);
        }


        /// <summary>
        /// Multiply mod operation
        /// </summary>
        public long Multiply(long a, long b)
        {
            return modulusFits32 ? Modulus(a * b) : MultiplyMod128Unsigned(a, b, modulus, magic32MulMod);
        }


        /// <summary>
        /// Add mod operation
        /// </summary>
        public long Add(long a, long b)
        {
            long r = a + b;
            return r - modulus >= 0 ? r - modulus : r;
        }


        /// <summary>
        /// Subtract mod operation
        /// </summary>
        public long Subtract(long a, long b)
        {
            long r = a - b;
            return r + ((r >> 63) & modulus);
        }


        public long Divide(long a, long b)
        {
            return Multiply(a, Reciprocal(b));
        }


        /// <summary>
        /// cached modular inverses
        /// </summary>
        private volatile int[]? cachedReciprocals = null;


        /// <summary>
        /// builds a table of cached reciprocals
        /// </summary>
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


        /// <summary>
        /// Returns modular inverse of {@code val}
        /// </summary>
        public long Reciprocal(long val)
        {
            return cachedReciprocals == null
                ? MachineArithmetic.ModInverse(val, modulus)
                : (val < cachedReciprocals.Length
                    ? cachedReciprocals[(int)val]
                    : MachineArithmetic.ModInverse(val, modulus));
        }


        /// <summary>
        /// Negate mod operation
        /// </summary>
        public long Negate(long val)
        {
            return val == 0 ? val : modulus - val;
        }


        /// <summary>
        /// to symmetric modulus
        /// </summary>
        public long SymmetricForm(long value)
        {
            return value <= modulus / 2 ? value : value - modulus;
        }


        /// <summary>
        /// Converts this to a generic ring over big integers
        /// </summary>
        /// <returns>generic ring</returns>
        public IntegersZp AsGenericRing()
        {
            return new IntegersZp(modulus);
        }


        /// <summary>
        /// Returns {@code base} in a power of non-negative {@code e} modulo {@code magic.modulus}
        /// </summary>
        /// <param name="base">the base</param>
        /// <param name="exponent">the non-negative exponent</param>
        /// <returns>{@code base} in a power of {@code e}</returns>
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


        /// <summary>
        /// Returns a random element from this ring
        /// </summary>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random element from this ring</returns>
        public long RandomElement(Random rnd)
        {
            return Modulus(rnd.NextInt64());
        }


        /// <summary>
        /// Returns a random element from this ring
        /// </summary>
        /// <returns>random element from this ring</returns>
        public long RandomElement()
        {
            return RandomElement(new Random(DateTime.Now.Nanosecond));
        }


        /// <summary>
        /// Returns a random non zero element from this ring
        /// </summary>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random non zero element from this ring</returns>
        public long RandomNonZeroElement(Random rnd)
        {
            long el;
            do
            {
                el = RandomElement(rnd);
            } while (el == 0);

            return el;
        }


        /// <summary>
        /// Gives value!
        /// </summary>
        /// <param name="value">the number</param>
        /// <returns>value!</returns>
        public long Factorial(int value)
        {
            long result = 1;
            for (int i = 2; i <= value; ++i)
                result = Multiply(result, i);
            return result;
        }


        /// <summary>
        /// if modulus = a^b, a and b are stored in this array
        /// </summary>
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


        /// <summary>
        /// Returns whether the modulus is a perfect power
        /// </summary>
        /// <returns>whether the modulus is a perfect power</returns>
        public bool IsPerfectPower()
        {
            return PerfectPowerExponent() > 1;
        }


        /// <summary>
        /// Returns {@code base} if {@code modulus == base^exponent}, and {@code -1} otherwisec
        /// </summary>
        /// <returns>{@code base} if {@code modulus == base^exponent}, and {@code -1} otherwisec</returns>
        public long PerfectPowerBase()
        {
            CheckPerfectPower();
            return perfectPowerDecomposition[0];
        }


        /// <summary>
        /// Returns {@code exponent} if {@code modulus == base^exponent}, and {@code -1} otherwisec
        /// </summary>
        /// <returns>{@code exponent} if {@code modulus == base^exponent}, and {@code -1} otherwisec</returns>
        public long PerfectPowerExponent()
        {
            CheckPerfectPower();
            return perfectPowerDecomposition[1];
        }


        /// <summary>
        /// ring for perfectPowerBase()
        /// </summary>
        private IntegersZp64? ppBaseDomain = null;


        /// <summary>
        /// Returns ring for {@link #perfectPowerBase()} or {@code this} if modulus is not a perfect power
        /// </summary>
        /// <returns>ring for {@link #perfectPowerBase()} or {@code this} if modulus is not a perfect power</returns>
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


        /// <summary>
        /// Returns ring for {@link #perfectPowerBase()} or {@code this} if modulus is not a perfect power
        /// </summary>
        /// <returns>ring for {@link #perfectPowerBase()} or {@code this} if modulus is not a perfect power</returns>
        public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            IntegersZp64 that = (IntegersZp64)o;
            return modulus == that.modulus;
        }


        /// <summary>
        /// Returns ring for {@link #perfectPowerBase()} or {@code this} if modulus is not a perfect power
        /// </summary>
        /// <returns>ring for {@link #perfectPowerBase()} or {@code this} if modulus is not a perfect power</returns>
        public override string ToString()
        {
            return "Z/" + modulus;
        }


        /// <summary>
        /// Returns ring for {@link #perfectPowerBase()} or {@code this} if modulus is not a perfect power
        /// </summary>
        /// <returns>ring for {@link #perfectPowerBase()} or {@code this} if modulus is not a perfect power</returns>
        public override int GetHashCode()
        {
            return (int)(modulus ^ (modulus >>> 32));
        }
    }
}