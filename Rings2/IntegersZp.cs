using System.Numerics;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Util;


namespace Cc.Redberry.Rings
{
    /// <summary>
    /// Ring of integers modulo some {@code modulus}.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class IntegersZp : AIntegers
    {
        public new static readonly long serialVersionUID = 1;
        /// <summary>
        /// The modulus.
        /// </summary>
        public readonly BigInteger modulus;

        /// <summary>
        /// Creates Zp ring for specified modulus.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        public IntegersZp(BigInteger modulus)
        {
            this.modulus = modulus;
        }

        
        /// <summary>
        /// Creates Zp ring for specified modulus.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        public IntegersZp(long modulus) : this(new BigInteger(modulus))
        {
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

       
        public override bool IsUnit(BigInteger element)
        {
            return !element.IsZero && !modulus.DivideAndRemainder(element)[1].IsZero;
        }

      
        /// <summary>
        /// Returns {@code val mod this.modulus}
        /// </summary>
        /// <param name="val">the integer</param>
        /// <returns>{@code val mod this.modulus}</returns>
        public BigInteger Modulus(BigInteger val)
        {
            return (val.Signum() >= 0 && val.CompareTo(modulus) < 0) ? val : val.Mod(modulus);
        }

       
        /// <summary>
        /// Converts {@code value} to a symmetric representation of Zp
        /// </summary>
        /// <param name="value">field element</param>
        /// <returns>{@code value} in a symmetric representation of Zp</returns>
        public BigInteger SymmetricForm(BigInteger value)
        {
            return value.CompareTo(modulus.ShiftRight(1)) <= 0 ? value : value.Subtract(modulus);
        }

      
        public IntegersZp64 AsMachineRing()
        {
            return new IntegersZp64(modulus.LongValueExact());
        }

       
        public override BigInteger Add(BigInteger a, BigInteger b)
        {
            a = ValueOf(a);
            b = ValueOf(b);
            BigInteger r = a.Add(b), rm = r.Subtract(modulus);
            return rm.Signum() >= 0 ? rm : r;
        }

        public override BigInteger Subtract(BigInteger a, BigInteger b)
        {
            a = ValueOf(a);
            b = ValueOf(b);
            BigInteger r = a.Subtract(b);
            return r.Signum() < 0 ? r.Add(modulus) : r;
        }

        public override BigInteger Negate(BigInteger element)
        {
            return element.IsZero ? element : modulus.Subtract(ValueOf(element));
        }

        
        public override BigInteger Multiply(BigInteger a, BigInteger b)
        {
            return Modulus(a.Multiply(b));
        }

       
        public override BigInteger[] DivideAndRemainder(BigInteger a, BigInteger b)
        {
            return new BigInteger[]
            {
                Divide(a, b),
                BigInteger.Zero
            };
        }

      
        public BigInteger Divide(BigInteger a, BigInteger b)
        {
            return Multiply(a, b.ModInverse(modulus));
        }

        
        public override BigInteger Remainder(BigInteger a, BigInteger b)
        {
            return GetZero();
        }

        
        public override BigInteger Reciprocal(BigInteger element)
        {
            return element.ModInverse(modulus);
        }

        
        public override FactorDecomposition<BigInteger> FactorSquareFree(BigInteger element)
        {
            return Factor(element);
        }

        
        public override FactorDecomposition<BigInteger> Factor(BigInteger element)
        {
            return FactorDecomposition<BigInteger>.Of(this, element);
        }

        
        public override BigInteger ValueOf(BigInteger val)
        {
            return Modulus(val);
        }

        
        public override BigInteger ValueOf(long val)
        {
            return ValueOf(new BigInteger(val));
        }

        
        public override BigInteger RandomElement(Random rnd)
        {
            return RandomUtil.RandomInt(modulus, rnd);
        }

        
        public override IEnumerator<BigInteger> Iterator()
        {
            BigInteger val = BigInteger.Zero;
            while (val.CompareTo(modulus) < 0)
            {
                yield return val;
                val = val.Increment();
            }
        }

        
        /// <summary>
        /// ring for perfectPowerBase()
        /// </summary>
        private IntegersZp? ppBaseDomain;
        
        /// <summary>
        /// Returns ring for {@link #perfectPowerBase()} or {@code this} if modulus is not a perfect power
        /// </summary>
        /// <returns>ring for {@link #perfectPowerBase()} or {@code this} if modulus is not a perfect power</returns>
        public IntegersZp PerfectPowerBaseDomain()
        {
            if (ppBaseDomain == null)
            {
                lock (this)
                {
                    if (ppBaseDomain == null)
                    {
                        BigInteger? @base = PerfectPowerBase();
                        if (@base is null)
                            ppBaseDomain = this;
                        else
                            ppBaseDomain = new IntegersZp(@base.Value);
                    }
                }
            }

            return ppBaseDomain;
        }


        private IntegersZp64? lDomain;
        
        /// <summary>
        /// Returns machine integer ring or null if modulus is larger than {@code long}
        /// </summary>
        /// <returns>machine integer ring or null if modulus is larger than {@code long}</returns>
        public IntegersZp64? AsZp64()
        {
            if (!modulus.IsLong())
                return null;
            if (lDomain == null)
                lock (this)
                {
                    if (lDomain == null)
                        lDomain = new IntegersZp64(modulus.LongValueExact());
                }

            return lDomain;
        }

       
        public override string ToString()
        {
            return "Z/" + modulus;
        }

        
        public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            IntegersZp that = (IntegersZp)o;
            return modulus.Equals(that.modulus);
        }

       
        public override int GetHashCode()
        {
            return modulus.GetHashCode();
        }
    }
}