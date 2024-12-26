using System.Numerics;


namespace Polynomials;

public interface IRationals;

public sealed class Rationals<E> : Ring<Rational<E>>, IRationals
{

    public readonly Ring<E> ring;

    public Rationals(Ring<E> ring)
    {
        this.ring = ring;
    }

    public Rational<E>[] FromArray(E[] arr)
    {
        return arr.Select(MkNumerator).ToArray();
    }

    public Rational<E> MkNumerator(E num)
    {
        return new Rational<E>(ring, num);
    }


    public Rational<E> MkNumerator(long num)
    {
        return MkNumerator(ring.ValueOfLong(num));
    }


    public Rational<E> MkDenominator(E den)
    {
        return new Rational<E>(ring, ring.GetOne(), den);
    }


    public Rational<E> MkDenominator(long den)
    {
        return MkDenominator(ring.ValueOfLong(den));
    }


    public Rational<E> Mk(E num, E den)
    {
        return new Rational<E>(ring, num, den);
    }


    public Rational<E> Mk(long num, long den)
    {
        return new Rational<E>(ring, ring.ValueOfLong(num), ring.ValueOfLong(den));
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
        return null;
    }


    public override BigInteger Characteristic()
    {
        return BigInteger.Zero;
    }


    public override bool IsPerfectPower()
    {
        return false;
    }


    public override BigInteger? PerfectPowerBase()
    {
        return null;
    }


    public new BigInteger? PerfectPowerExponent()
    {
        return null;
    }


    public override Rational<E> Add(Rational<E> a, Rational<E> b)
    {
        return a.Add(b);
    }


    public override Rational<E> Subtract(Rational<E> a, Rational<E> b)
    {
        return a.Subtract(b);
    }


    public override Rational<E> Multiply(Rational<E> a, Rational<E> b)
    {
        return a.Multiply(b);
    }


    public override Rational<E> Negate(Rational<E> element)
    {
        return element.Negate();
    }


    public override object Clone()
    {
        return new Rationals<E>(ring);
    }

    public override int Signum(Rational<E> element)
    {
        return element.Signum();
    }

    public override Rational<E>[] DivideAndRemainder(Rational<E> dividend, Rational<E> divider)
    {
        return new Rational<E>[]
        {
            dividend.Divide(divider),
            Rational<E>.Zero(ring)
        };
    }

    public override Rational<E> Reciprocal(Rational<E> element)
    {
        return element.Reciprocal();
    }


    public override Rational<E> Gcd(Rational<E> a, Rational<E> b)
    {
        return Rational<E>.One(ring);
    }


    private FactorDecomposition<Rational<E>> Factor(Rational<E> element, Func<E, FactorDecomposition<E>> factor)
    {
        if (element.IsZero())
            return FactorDecomposition<Rational<E>>.Of(this, element);

        var factors = FactorDecomposition<Rational<E>>.Empty(this);

        var numFactors = element.numerator.Select(factor).Aggregate(
            FactorDecomposition<E>.Empty(ring), (f, newFac) => f.AddAll(newFac));
        for (int i = 0; i < numFactors.Factors.Count; i++)
            factors.AddNonUnitFactor(new Rational<E>(ring, numFactors.Factors[i]), numFactors.Exponents[i]);
        factors.AddFactor(new Rational<E>(ring, numFactors.Unit), 1);

        var denFactors = element.denominator.Select(factor).Aggregate(
            FactorDecomposition<E>.Empty(ring),
            (f, newFac) => f.AddAll(newFac));
        for (int i = 0; i < denFactors.Factors.Count; i++)
            factors.AddNonUnitFactor(new Rational<E>(ring, ring.GetOne(), denFactors.Factors[i]),
                denFactors.Exponents[i]);
        factors.AddFactor(new Rational<E>(ring, ring.GetOne(), denFactors.Unit), 1);

        return factors;
    }

    public override FactorDecomposition<Rational<E>> FactorSquareFree(Rational<E> element)
    {
        return Factor(element, ring.FactorSquareFree);
    }


    public override FactorDecomposition<Rational<E>> Factor(Rational<E> element)
    {
        return Factor(element, ring.Factor);
    }


    public override Rational<E> GetZero()
    {
        return Rational<E>.Zero(ring);
    }


    public override Rational<E> GetOne()
    {
        return Rational<E>.One(ring);
    }


    public override bool IsZero(Rational<E> element)
    {
        return element.IsZero();
    }

    public override bool IsOne(Rational<E> element)
    {
        return element.IsOne();
    }


    public override bool IsUnit(Rational<E> element)
    {
        return !IsZero(element);
    }

    public override Rational<E> ValueOfLong(long val)
    {
        return new Rational<E>(ring, ring.ValueOfLong(val));
    }


    public override Rational<E> ValueOfBigInteger(BigInteger val)
    {
        return new Rational<E>(ring, ring.ValueOfBigInteger(val));
    }


    public override Rational<E> Copy(Rational<E> element)
    {
        return new Rational<E>(true, ring, element.numerator.DeepCopy(ring), element.denominator.DeepCopy(ring));
    }

    public override bool Equal(Rational<E> x, Rational<E> y)
    {
        return x.Equals(y);
    }


    public override Rational<E> ValueOf(Rational<E> val)
    {
        if (val.ring.Equals(ring))
            return val;
        else
            return new Rational<E>(ring, val.numerator.Map(ring.ValueOf), val.denominator.Map(ring.ValueOf));
    }

    public override int Compare(Rational<E>? o1, Rational<E>? o2)
    {
        if (o1 == null)
            return o2 == null ? 0 : -1;
        return o1.CompareTo(o2);
    }


    public override Rational<E> GetNegativeOne()
    {
        return Rational<E>.One(ring).Negate();
    }

    public override Rational<E> RandomElement(Random rnd)
    {
        long den;
        E eden;
        do
        {
            den = rnd.Next();
        } while (ring.IsZero(eden = ring.ValueOfLong(den)));

        return new Rational<E>(ring, ring.ValueOfLong(rnd.Next()), eden);
    }

    public override Rational<E> RandomElementTree(Random rnd)
    {
        E den;
        do
        {
            den = ring.RandomElementTree(rnd);
        } while (ring.IsZero(den));

        return new Rational<E>(ring, ring.RandomElementTree(rnd), den);
    }

    public override IEnumerable<Rational<E>> Iterator()
    {
        throw new NotSupportedException("Ring of infinite cardinality.");
    }


    public IEnumerable<Rational<E>> GetEnumerator()
    {
        return Iterator();
    }

    public override bool Equals(object? o)
    {
        if (this == o)
            return true;
        if (o == null || GetType() != o.GetType())
            return false;

        var rationals = (Rationals<E>)o;
        return ring.Equals(rationals.ring);
    }

    public override int GetHashCode()
    {
        return ring.GetHashCode();
    }


    public override string ToString()
    {
        return ring.Equals(Rings.Z) ? "Q" : "Frac(" + ring + ")";
    }
    
}
