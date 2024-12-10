using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Primes;
using System.Numerics;

namespace Cc.Redberry.Rings
{
    /// <summary>
    /// The ring of integers (Z).
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class Integers : AIntegers
    {
        public static readonly long serialVersionUID = 1;
        /// <summary>
        /// The ring of integers (Z)
        /// </summary>
        public static readonly Integers Z = new Integers();
 
        private Integers()
        {
        }

        public override bool IsField()
        {
            return false;
        }


        public override bool IsEuclideanRing()
        {
            return true;
        }

        public override BigInteger? Cardinality()
        {
            return null;
        }

        public override BigInteger Characteristic()
        {
            return BigInteger.Zero;
        }

 
        public override bool IsUnit(BigInteger element)
        {
            return IsOne(element) || IsMinusOne(element);
        }


        public override BigInteger Add(BigInteger a, BigInteger b)
        {
            return a.Add(b);
        }

        public override BigInteger Subtract(BigInteger a, BigInteger b)
        {
            return a.Subtract(b);
        }

     
        public override BigInteger Negate(BigInteger element)
        {
            return element.Negate();
        }

        public override BigInteger Multiply(BigInteger a, BigInteger b)
        {
            return a.Multiply(b);
        }

     
        public override BigInteger[] DivideAndRemainder(BigInteger a, BigInteger b)
        {
            return a.DivideAndRemainder(b);
        }

        public override BigInteger Remainder(BigInteger a, BigInteger b)
        {
            return a.Mod(b);
        }

        public override BigInteger Reciprocal(BigInteger element)
        {
            if (IsOne(element) || IsMinusOne(element))
                return element;
            throw new NotSupportedException();
        }


        public override BigInteger Pow(BigInteger @base, int exponent)
        {
            return @base.Pow(exponent);
        }


        public override BigInteger Pow(BigInteger @base, long exponent)
        {
            if (exponent < int.MaxValue)
                return Pow(@base, (int) exponent);
            return @base.Pow(@base, exponent);
        }


        public override BigInteger Pow(BigInteger @base, BigInteger exponent)
        {
            if (exponent.IsLong())
                return Pow(@base, exponent.LongValueExact());
            return @base.Pow(@base, exponent);
        }


        public override BigInteger Gcd(BigInteger a, BigInteger b)
        {
            return a.Gcd(b);
        }

  
        public override FactorDecomposition<BigInteger> FactorSquareFree(BigInteger element)
        {
            return Factor(element);
        }

  
        public override FactorDecomposition<BigInteger> Factor(BigInteger element)
        {
            return FactorDecomposition<BigInteger>.Of(this, BigPrimes.PrimeFactors(element));
        }

  
        public override BigInteger ValueOf(BigInteger val)
        {
            return val;
        }

  
        public override BigInteger ValueOf(long val)
        {
            return new BigInteger(val);
        }

  
        public override BigInteger GetNegativeOne()
        {
            return BigInteger.MinusOne;
        }

  
        public override bool IsMinusOne(BigInteger bigInteger)
        {
            return bigInteger == -1;
        }

  
        public override int Signum(BigInteger element)
        {
            return element.Signum();
        }

  
        public override BigInteger Abs(BigInteger el)
        {
            return el.Abs();
        }

  
        public override string ToString()
        {
            return "Z";
        }

  
        public override IEnumerator<BigInteger> Iterator()
        {
            throw new NotSupportedException("Ring of infinite cardinality.");
        }

  
        /// <summary>
        /// Gives a binomial coefficient C(n, k)
        /// </summary>
        public BigInteger Binomial(long n, long k)
        {
            return Factorial(n).DivideExact(Factorial(k)).DivideExact(Factorial(n - k));
        }

        
        protected object ReadResolve()
        {
            return new Integers();
        }

        
        protected override object Clone()
        {
            return new Integers();
        }

        
        public override bool Equals(object? obj)
        {
            return this.GetType() == obj.GetType();
        }

        
        public override int GetHashCode()
        {
            return 0x1a2e9d8f;
        }
    }
}