using System.Numerics;
using Rings.primes;

namespace Rings;

/**
 * The ring of integers (Z).
 *
 * @since 1.0
 */
public sealed class Integers : AIntegers
{
    private static readonly long serialVersionUID = 1L;

    /** The ring of integers (Z) */
    public static readonly Integers ZZ = new Integers();

    private Integers()
    {
    }

    public new bool isField()
    {
        return false;
    }

    public new bool isEuclideanRing()
    {
        return true;
    }

    public new BigInteger? cardinality()
    {
        return null;
    }

    public new  BigInteger characteristic()
    {
        return BigInteger.Zero;
    }

    public override  bool isUnit(BigInteger element)
    {
        return isOne(element) || isMinusOne(element);
    }

    public override  BigInteger add(BigInteger a, BigInteger b)
    {
        return a + b;
    }

    public override  BigInteger subtract(BigInteger a, BigInteger b)
    {
        return a - b;
    }

    public override  BigInteger negate(BigInteger element)
    {
        return -element;
    }

    public override  BigInteger multiply(BigInteger a, BigInteger b)
    {
        return a * b;
    }

    public override BigInteger[] divideAndRemainder(BigInteger a, BigInteger b)
    {
        return [a / b, a % b];
    }

    public BigInteger remainder(BigInteger a, BigInteger b)
    {
        return a % b;
    }

    public override BigInteger reciprocal(BigInteger element)
    {
        if (isOne(element) || isMinusOne(element))
            return element;
        throw new InvalidOperationException();
    }

    public BigInteger pow(BigInteger @base, int exponent)
    {
        return BigInteger.Pow(@base, exponent);
    }

    public BigInteger pow(BigInteger @base, long exponent)
    {
        if (exponent < int.MaxValue)
            return pow(@base, (int)exponent);
        return base.pow(@base, exponent);
    }

    public BigInteger pow(BigInteger @base, BigInteger exponent)
    {
        if (exponent.isLong())
            return pow(@base, exponent.longValueExact());
        return base.pow(@base, exponent);
    }

    public BigInteger gcd(BigInteger a, BigInteger b)
    {
        return BigInteger.GreatestCommonDivisor(a, b);
    }

    public FactorDecomposition<BigInteger> factorSquareFree(BigInteger element)
    {
        return factor(element);
    }

    public FactorDecomposition<BigInteger> factor(BigInteger element)
    {
        return FactorDecomposition.of(this, BigPrimes.primeFactors(element));
    }

    public override BigInteger valueOf(BigInteger val)
    {
        return val;
    }

    public override BigInteger valueOf(long val)
    {
        return new BigInteger(val);
    }

    public BigInteger getNegativeOne()
    {
        return BigInteger.MinusOne;
    }

    public bool isMinusOne(BigInteger bigInteger)
    {
        return bigInteger == -1;
    }

    public int signum(BigInteger element)
    {
        return element.Sign;
    }

    public BigInteger abs(BigInteger el)
    {
        return BigInteger.Abs(el);
    }

    public String toString()
    {
        return "Z";
    }

    public override IEnumerable<BigInteger> iterator()
    {
        throw new InvalidOperationException("Ring of infinite cardinality.");
    }

    /**
     * Gives a binomial coefficient C(n, k)
     */
    public BigInteger binomial(long n, long k)
    {
        return factorial(n).divideExact(factorial(k)).divideExact(factorial(n - k));
    }

    protected Object readResolve()
    {
        return ZZ;
    }

    protected Object clone()
    {
        return ZZ;
    }

    public bool equals(Object obj)
    {
        return this.GetType() == obj.GetType();
    }

    public int hashCode()
    {
        return 0x1a2e9d8f;
    }
}