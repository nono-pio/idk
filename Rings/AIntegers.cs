using System.Numerics;

namespace Rings;

public abstract class AIntegers : ARing<BigInteger>
{
    private static readonly long serialVersionUID = 1L;
    
    public override BigInteger getZero()
    {
        return BigInteger.Zero;
    }


    public override BigInteger getOne()
    {
        return BigInteger.One;
    }


    public override bool isZero(BigInteger element)
    {
        return element.IsZero;
    }


    public  override bool isOne(BigInteger element)
    {
        return element.IsOne;
    }


    public  BigInteger parse(String str)
    {
        return valueOf(BigInteger.Parse(str.Trim()));
    }


    public override int compare(BigInteger o1, BigInteger o2)
    {
        return o1.CompareTo(o2);
    }
    


    public override BigInteger valueOfBigInteger(BigInteger val)
    {
        return valueOf(val);
    }


    public override BigInteger copy(BigInteger element)
    {
        return element;
    }
}