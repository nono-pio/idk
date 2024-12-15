using System.Numerics;
using Polynomials.Utils;

namespace Polynomials;

public class Integers64 : Ring<long>
{
    
    public Integers64(){}
    
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

    public override long Add(long a, long b)
    {
        return a + b;
    }

    public override long Subtract(long a, long b)
    {
        return a - b;
    }

    public override long Multiply(long a, long b)
    {
        return a * b;
    }

    public override long Negate(long element)
    {
        return -element;
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
        return new Integers64();
    }

    public override long[]? DivideAndRemainder(long dividend, long divider)
    {
        return new long[] { dividend / divider, dividend % divider };
    }

    public override long Reciprocal(long element)
    {
        if (element != 1 && element != -1)
            throw new ArithmeticException("Reciprocal does not exist");
        
        return element;
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
        return element == 1 || element == -1;
    }

    public override long ValueOfLong(long val)
    {
        return val;
    }

    public override long ValueOfBigInteger(BigInteger val)
    {
        return (long)val;
    }

    public override long ValueOf(long val)
    {
        return val;
    }

    public override IEnumerator<long> Iterator()
    {
        throw new NotSupportedException("Ring of infinite cardinality.");
    }

    public override long Gcd(long a, long b)
    {
        return MachineArithmetic.Gcd(a, b);
    }
}