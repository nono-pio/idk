using System.Numerics;
using OptTest.Utils;

namespace OptTest.Basics;

public interface IntegerNumberSystem<Self> : EuclideanDomain<Self> where Self : IntegerNumberSystem<Self>
{
    public static abstract Self FromBigInteger(BigInteger value);
    public static abstract BigInteger ToBigInteger(Self value);
    public BigInteger ToBigInteger() => Self.ToBigInteger((Self) this);

}

public sealed class Integer : IntegerNumberSystem<Integer>
{
    private BigInteger Value;
    public static uint CharacteristicFactory => 0;
    public static Integer OneFactory => FromBigInteger(1);
    public static Integer ZeroFactory => FromBigInteger(0);

    public Integer(BigInteger value)
    {
        Value = value;
    }


    public static MayFail<Integer> Exquo(Integer a, Integer b)
    {
        throw new NotImplementedException();
    }

    public static (Integer Unit, Integer Canonical, Integer Associate) UnitNormal(Integer a)
    {
        throw new NotImplementedException();
    }

    public static Boolean IsUnit(Integer a)
    {
        throw new NotImplementedException();
    }

    public static Integer Gcd(Integer a, Integer b)
    {
        return FromBigInteger(BigInteger.GreatestCommonDivisor(a.Value, b.Value));
    }

    public uint EuclideanSize(Integer a)
    {
        throw new NotImplementedException();
    }

    public (Integer Quotient, Integer Remainder) Divide(Integer a, Integer b)
    {
        var (q, r) = BigInteger.DivRem(a.Value, b.Value);
        return (FromBigInteger(q), FromBigInteger(r));
    }

    public (Integer Coef1, Integer Coef2, Integer Generator) ExtendedEuclidean(Integer a, Integer b)
    {
        throw new NotImplementedException();
    }

    public MayFail<(Integer Coef1, Integer Coef2)> ExtendedEuclidean(Integer a, Integer b, Integer c)
    {
        throw new NotImplementedException();
    }

    public static Integer FromBigInteger(BigInteger value)
    {
        return new Integer(value);
    }

    public static BigInteger ToBigInteger(Integer value)
    {
        return value.Value;
    }
    
    
 

    public static MayFail<Integer> SubtractIfCan(Integer a, Integer b)
    {
        return new MayFail<Integer>(a - b);
    }

    public static Integer operator +(Integer a, Integer b) => FromBigInteger(a.Value + b.Value);
    public static Integer operator -(Integer a) => FromBigInteger(-a.Value);
    public static Integer operator -(Integer a, Integer b) => FromBigInteger(a.Value - b.Value);
    public static Integer operator *(Integer a, Integer b) => FromBigInteger(a.Value * b.Value);
    public static Boolean operator ==(Integer a, Integer b) => a.Value == b.Value;
    public static Boolean operator !=(Integer a, Integer b) => a.Value != b.Value;
    public override string ToString()
    {
        return Value.ToString();
    }
}