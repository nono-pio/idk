using System.Numerics;

namespace Polynomials;

public class ImageRing<F, I> : Ring<I>
{
    public readonly Ring<F> ring;

    public readonly Func<F, I> imageFunc;
    public readonly Func<I, F> inverseFunc;

    public ImageRing(Ring<F> ring, Func<I, F> inverseFunc, Func<F, I> imageFunc)
    {
        this.ring = ring;
        this.inverseFunc = inverseFunc;
        this.imageFunc = imageFunc;
    }

    public I Image(F el)
    {
        return imageFunc(el);
    }

    public I[] Image(F[] el)
    {
        I[] array = new I[el.Length];
        for (int i = 0; i < array.Length; i++)
            array[i] = Image(el[i]);
        return array;
    }

    public F Inverse(I el)
    {
        return inverseFunc(el);
    }

    public F[] Inverse(I[] el)
    {
        F[] array = new F[el.Length];
        for (int i = 0; i < array.Length; i++)
            array[i] = Inverse(el[i]);
        return array;
    }

    public override bool IsField()
    {
        return ring.IsField();
    }

    public override bool IsEuclideanRing()
    {
        return ring.IsEuclideanRing();
    }

    public override BigInteger? Cardinality()
    {
        return ring.Cardinality();
    }

    public override BigInteger Characteristic()
    {
        return ring.Characteristic();
    }

    public override bool IsPerfectPower()
    {
        return ring.IsPerfectPower();
    }

    public override BigInteger? PerfectPowerBase()
    {
        return ring.PerfectPowerBase();
    }

    public new virtual BigInteger? PerfectPowerExponent()
    {
        return ring.PerfectPowerExponent();
    }

    public override I Add(I a, I b)
    {
        return Image(ring.Add(Inverse(a), Inverse(b)));
    }

    public override I Subtract(I a, I b)
    {
        return Image(ring.Subtract(Inverse(a), Inverse(b)));
    }

    public override I Multiply(I a, I b)
    {
        return Image(ring.Multiply(Inverse(a), Inverse(b)));
    }

    public override I Negate(I element)
    {
        return Image(ring.Negate(Inverse(element)));
    }

    public new virtual I Increment(I element)
    {
        return Image(ring.Increment(Inverse(element)));
    }

    public new virtual I Decrement(I element)
    {
        return Image(ring.Decrement(Inverse(element)));
    }

    public new virtual I Add(params I[] elements)
    {
        return Image(ring.Add(Inverse(elements)));
    }

    public new virtual I Multiply(params I[] elements)
    {
        return Image(ring.Multiply(Inverse(elements)));
    }

    public override I Abs(I el)
    {
        return Image(ring.Abs(Inverse(el)));
    }

    public override I Copy(I element)
    {
        return element;
    }

    public override bool Equal(I x, I y)
    {
        return ring.Equal(Inverse(x), Inverse(y));
    }

    public override I[]? DivideAndRemainder(I dividend, I divider)
    {
        var qr = ring.DivideAndRemainder(Inverse(dividend), Inverse(divider));
        return qr is null ? null : Image(qr);
    }

    public new virtual I Quotient(I dividend, I divider)
    {
        return Image(ring.Quotient(Inverse(dividend), Inverse(divider)));
    }

    public override I Remainder(I dividend, I divider)
    {
        return Image(ring.Remainder(Inverse(dividend), Inverse(divider)));
    }

    public override I Reciprocal(I element)
    {
        return Image(ring.Reciprocal(Inverse(element)));
    }

    public override I GetZero()
    {
        return Image(ring.GetZero());
    }

    public override I GetOne()
    {
        return Image(ring.GetOne());
    }

    public override bool IsZero(I element)
    {
        return ring.IsZero(Inverse(element));
    }

    public override bool IsOne(I element)
    {
        return ring.IsOne(Inverse(element));
    }

    public override bool IsUnit(I element)
    {
        return ring.IsUnit(Inverse(element));
    }

    public override I ValueOfLong(long val)
    {
        return Image(ring.ValueOfLong(val));
    }

    public override I ValueOfBigInteger(BigInteger val)
    {
        return Image(ring.ValueOfBigInteger(val));
    }

    public override I ValueOf(I val)
    {
        return Image(ring.ValueOf(Inverse(val)));
    }

    public override IEnumerable<I> Iterator()
    {
        return ring.Iterator().Order().Select(Image);
    }

    public override I Gcd(I a, I b)
    {
        return Image(ring.Gcd(Inverse(a), Inverse(b)));
    }

    public override I[] ExtendedGCD(I a, I b)
    {
        return Image(ring.ExtendedGCD(Inverse(a), Inverse(b)));
    }

    public new virtual I Lcm(I a, I b)
    {
        return Image(ring.Lcm(Inverse(a), Inverse(b)));
    }

    public new virtual I Gcd(params I[] elements)
    {
        return Image(ring.Gcd(Inverse(elements)));
    }

    public new virtual I Gcd(IEnumerable<I> elements)
    {
        return Image(ring.Gcd(elements.Select(Inverse)));
    }

    public override object Clone()
    {
        return new ImageRing<F, I>((Ring<F>)ring.Clone(), inverseFunc, imageFunc);
    }

    public override int Signum(I element)
    {
        return ring.Signum(Inverse(element));
    }

    public override FactorDecomposition<I> FactorSquareFree(I element)
    {
        return ring.FactorSquareFree(Inverse(element)).MapTo(this, Image);
    }

    public override FactorDecomposition<I> Factor(I element)
    {
        return ring.Factor(Inverse(element)).MapTo(this, Image);
    }


    public override I Pow(I @base, int exponent)
    {
        return Image(ring.Pow(Inverse(@base), exponent));
    }

    public override I Pow(I @base, long exponent)
    {
        return Image(ring.Pow(Inverse(@base), exponent));
    }

    public override I Pow(I @base, BigInteger exponent)
    {
        return Image(ring.Pow(Inverse(@base), exponent));
    }

    public override I Factorial(long num)
    {
        return Image(ring.Factorial(num));
    }

    public override I RandomElement(Random rnd)
    {
        return Image(ring.RandomElement(rnd));
    }

    public override int Compare(I? o1, I? o2)
    {
        return ring.Compare(Inverse(o1), Inverse(o2));
    }

    public override bool Equals(object? o)
    {
        if (this == o)
            return true;
        if (o == null || GetType() != o.GetType())
            return false;
        ImageRing<F, I> that = (ImageRing<F, I>)o;
        if (!ring.Equals(that.ring))
            return false;
        if (!inverseFunc.Equals(that.inverseFunc))
            return false;
        return imageFunc.Equals(that.imageFunc);
    }

    public override int GetHashCode()
    {
        int result = ring.GetHashCode();
        result = 31 * result + inverseFunc.GetHashCode();
        result = 31 * result + imageFunc.GetHashCode();
        return result;
    }
}
