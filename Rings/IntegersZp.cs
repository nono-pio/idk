using System.Collections;
using System.Numerics;

namespace Rings;

public sealed class IntegersZp : AIntegers
{
    private static readonly long serialVersionUID = 1L;

    public readonly BigInteger Modulus;
    
    public IntegersZp(BigInteger modulus)
    {
        this.Modulus = modulus;
    }
    
    public IntegersZp(long modulus):this(new BigInteger(modulus)) { }


    public new bool isField()
    {
        return true;
    }


    public new bool isEuclideanRing()
    {
        return true;
    }


    public new BigInteger cardinality()
    {
        return Modulus;
    }


    public new BigInteger characteristic()
    {
        return Modulus;
    }


    public override bool isUnit(BigInteger element)
    {
        return !element.IsZero && !(Modulus % element).IsZero;
    }

    public BigInteger modulus(BigInteger val)
    {
        return (val.Sign >= 0 && val.CompareTo(modulus) < 0) ? val : val % Modulus;
    }

    public BigInteger symmetricForm(BigInteger value)
    {
        return value.CompareTo(Modulus >> 1) <= 0 ? value : value - Modulus;
    }


    public IntegersZp64 asMachineRing()
    {
        return new IntegersZp64(Modulus.longValueExact());
    }


    public override BigInteger add(BigInteger a, BigInteger b)
    {
        a = valueOf(a);
        b = valueOf(b);
        BigInteger r = a + b, rm = r - Modulus;
        return rm.Sign >= 0 ? rm : r;
    }


    public override BigInteger subtract(BigInteger a, BigInteger b)
    {
        a = valueOf(a);
        b = valueOf(b);
        BigInteger r = a - b;
        return r.Sign < 0 ? r + Modulus : r;
    }


    public override BigInteger negate(BigInteger element)
    {
        return element.IsZero ? element : Modulus - valueOf(element);
    }


    public override BigInteger multiply(BigInteger a, BigInteger b)
    {
        return modulus(a * b);
    }


    public override BigInteger[] divideAndRemainder(BigInteger a, BigInteger b)
    {
        return new BigInteger[] { divide(a, b), BigInteger.Zero };
    }

    public BigInteger divide(BigInteger a, BigInteger b)
    {
        return multiply(a, b.modInverse(Modulus));
    }


    public BigInteger remainder(BigInteger a, BigInteger b)
    {
        return getZero();
    }


    public override BigInteger reciprocal(BigInteger element)
    {
        return element.modInverse(Modulus);
    }


    public FactorDecomposition<BigInteger> factorSquareFree(BigInteger element)
    {
        return factor(element);
    }


    public FactorDecomposition<BigInteger> factor(BigInteger element)
    {
        return FactorDecomposition.of(this, element);
    }


    public override BigInteger valueOf(BigInteger val)
    {
        return modulus(val);
    }


    public override BigInteger valueOf(long val)
    {
        return valueOf(new BigInteger(val));
    }


    public BigInteger randomElement(RandomGenerator rnd)
    {
        return RandomUtil.randomInt(modulus, rnd);
    }


    public override IEnumerable<BigInteger> iterator()
    {
        BigInteger val = BigInteger.Zero;

        while (val.CompareTo(Modulus) < 0)
        {
            yield return val;
            val++;
        }
            
        yield break;
    }

    private IntegersZp ppBaseDomain = null;

    public IntegersZp perfectPowerBaseDomain()
    {
        if (ppBaseDomain == null)
        {
            synchronized(this) {
                if (ppBaseDomain == null)
                {
                    BigInteger @base = perfectPowerBase();
                    if (@base == null)
                        ppBaseDomain = this;
                    else
                        ppBaseDomain = new IntegersZp(@base);
                }
            }
        }

        return ppBaseDomain;
    }

    private IntegersZp64 lDomain;


    public IntegersZp64 asZp64()
    {
        if (!Modulus.isLong())
            return null;
        if (lDomain == null)
            synchronized(this)
        {
            if (lDomain == null)
                lDomain = new IntegersZp64(Modulus.longValueExact());
        }

        return lDomain;
    }


    public String toString()
    {
        return "Z/" + modulus;
    }


    public bool equals(Object o)
    {
        if (this == o) 
            return true;
        if (o == null || GetType() != o.GetType()) return false;

        IntegersZp that = (IntegersZp)o;

        return Modulus == that.Modulus;
    }


    public int hashCode()
    {
        return Modulus.GetHashCode();
    }
}