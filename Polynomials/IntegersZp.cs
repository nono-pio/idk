using System.Numerics;
using Polynomials.Utils;

namespace Polynomials;

public class IntegersZp : Ring<BigInteger>
{
    public readonly BigInteger modulus;
    
    public IntegersZp(BigInteger modulus)
    {
        this.modulus = modulus;
    }


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
        return !element.IsZero && !(modulus % element).IsZero;
    }


    public BigInteger Modulus(BigInteger val)
    {
        if (val.Sign < 0)
        {
            return modulus - Modulus(-val);
        }
        
        
        return val.CompareTo(modulus) < 0 ? val : val % modulus;
    }


    public BigInteger SymmetricForm(BigInteger value)
    {
        return value.CompareTo(modulus >> 1) <= 0 ? value : value - modulus;
    }


    public IntegersZp64 AsMachineRing()
    {
        return new IntegersZp64((long) modulus);
    }


    public override BigInteger Add(BigInteger a, BigInteger b)
    {
        a = ValueOf(a);
        b = ValueOf(b);
        BigInteger r = a + b, rm = r - modulus;
        return rm.Sign >= 0 ? rm : r;
    }

    public override BigInteger Subtract(BigInteger a, BigInteger b)
    {
        a = ValueOf(a);
        b = ValueOf(b);
        BigInteger r = a - b;
        return r.Sign < 0 ? r + modulus : r;
    }

    public override BigInteger Negate(BigInteger element)
    {
        return element.IsZero ? element : modulus - ValueOf(element);
    }


    public override BigInteger Multiply(BigInteger a, BigInteger b)
    {
        return Modulus(a * b);
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
        return Multiply(a, BigIntegerUtils.ModInverse(b, modulus));
    }


    public override BigInteger Remainder(BigInteger a, BigInteger b)
    {
        return GetZero();
    }


    public override BigInteger Reciprocal(BigInteger element)
    {
        return BigIntegerUtils.ModInverse(element, modulus);
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


    public override BigInteger ValueOfLong(long val)
    {
        return ValueOf(new BigInteger(val));
    }

    public override BigInteger RandomElement(Random rnd)
    {
        return BigIntegerUtils.RandomBigIntBound(modulus, rnd);
    }


    public override IEnumerable<BigInteger> Iterator()
    {
        BigInteger val = BigInteger.Zero;
        while (val.CompareTo(modulus) < 0)
        {
            yield return val;
            val++;
        }
    }


    private IntegersZp? ppBaseDomain;


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


    public IntegersZp64? AsZp64()
    {
        if (!modulus.IsLong())
            return null;
        if (lDomain == null)
            lock (this)
            {
                if (lDomain == null)
                    lDomain = new IntegersZp64((long)modulus);
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
    
    public override object Clone()
    {
        return new IntegersZp(modulus);
    }
    
    public override BigInteger GetZero()
    {
        return BigInteger.Zero;
    }

    public override BigInteger GetOne()
    {
        return BigInteger.One;
    }

    public override bool IsZero(BigInteger element)
    {
        return element.IsZero;
    }

    public override bool IsOne(BigInteger element)
    {
        return element.IsOne;
    }

    public override bool Equal(BigInteger x, BigInteger y)
    {
        return x == y;
    }

    public override int Compare(BigInteger o1, BigInteger o2)
    {
        return o1.CompareTo(o2);
    }

    public override BigInteger ValueOfBigInteger(BigInteger val)
    {
        return ValueOf(val);
    }

    public override BigInteger Copy(BigInteger element)
    {
        return element;
    }
}