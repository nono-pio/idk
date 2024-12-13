using System.Numerics;
using Polynomials.Primes;
using Polynomials.Utils;

namespace Polynomials;

public class Integers : Ring<BigInteger>
{
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
        return a + b;
    }

    public override BigInteger Subtract(BigInteger a, BigInteger b)
    {
        return a - b;
    }


    public override BigInteger Negate(BigInteger element)
    {
        return -element;
    }

    public override BigInteger Multiply(BigInteger a, BigInteger b)
    {
        return a * b;
    }


    public override BigInteger[] DivideAndRemainder(BigInteger a, BigInteger b)
    {
        var qr = BigInteger.DivRem(a, b);
        return [qr.Quotient, qr.Remainder];
    }

    public override BigInteger Remainder(BigInteger a, BigInteger b)
    {
        return BigInteger.Remainder(a, b);
    }

    public override BigInteger Reciprocal(BigInteger element)
    {
        if (IsOne(element) || IsMinusOne(element))
            return element;
        throw new NotSupportedException();
    }


    public override BigInteger Pow(BigInteger @base, int exponent)
    {
        return BigInteger.Pow(@base, exponent);
    }


    public override BigInteger Pow(BigInteger @base, long exponent)
    {
        if (exponent < int.MaxValue)
            return Pow(@base, (int)exponent);
        return BigIntegerUtils.Pow(@base, exponent);
    }


    public override BigInteger Pow(BigInteger @base, BigInteger exponent)
    {
        if (exponent.IsLong())
            return Pow(@base, (long) exponent);
        return BigIntegerUtils.Pow(@base, exponent);
    }


    public override BigInteger Gcd(BigInteger a, BigInteger b)
    {
        return BigInteger.GreatestCommonDivisor(a, b);
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
        return element.Sign;
    }


    public override BigInteger Abs(BigInteger el)
    {
        return BigInteger.Abs(el);
    }


    public override string ToString()
    {
        return "Z";
    }


    public override IEnumerator<BigInteger> Iterator()
    {
        throw new NotSupportedException("Ring of infinite cardinality.");
    }


    public BigInteger Binomial(long n, long k)
    {
        return (Factorial(n) / Factorial(k)) / Factorial(n - k);
    }


    protected object ReadResolve()
    {
        return new Integers();
    }


    public override object Clone()
    {
        return new Integers();
    }


    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        return this.GetType() == obj.GetType();
    }


    public override int GetHashCode()
    {
        return 0x1a2e9d8f;
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