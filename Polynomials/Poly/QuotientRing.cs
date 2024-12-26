using System.Numerics;
using Polynomials.Poly.Multivar;
using Polynomials.Poly.Univar;

namespace Polynomials.Poly;

public class QuotientRing<E> : PolynomialRing<MultivariatePolynomial<E>>
{
    public readonly MultivariateRing<E> baseRing;


    public readonly Ideal<E> ideal;


    private readonly MultivariatePolynomial<E> factory;


    public QuotientRing(MultivariateRing<E> baseRing, Ideal<E> ideal)
    {
        this.baseRing = baseRing;
        this.ideal = ideal;
        this.factory = ideal.GetBasisGenerator(0).CreateZero();
    }


    public override int NVariables()
    {
        return factory.nVariables;
    }


    public override MultivariatePolynomial<E> Factory()
    {
        return factory;
    }


    public override MultivariatePolynomial<E> Variable(int variable)
    {
        return factory.CreateMonomial(variable, 1);
    }


    public override bool IsField()
    {
        return factory.IsOverField() && ideal.IsMaximal();
    }


    public override bool IsEuclideanRing()
    {
        throw new NotSupportedException("Algebraic structure of ring is unknown");
    }


    public override BigInteger? Cardinality()
    {
        return factory.CoefficientRingCardinality().Value.IsZero ? BigInteger.Zero :
            ideal.Dimension() != 0 ? null :
            new BigInteger(ideal.Degree()) * factory.CoefficientRingCardinality().Value;
    }


    public override BigInteger Characteristic()
    {
        return factory.CoefficientRingCharacteristic();
    }


    public virtual MultivariatePolynomial<E> Mod(MultivariatePolynomial<E> el)
    {
        return ideal.NormalForm(el);
    }


    public virtual MultivariatePolynomial<E> NormalForm(MultivariatePolynomial<E> el)
    {
        return Mod(el);
    }


    public override MultivariatePolynomial<E> Add(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        return Mod(baseRing.Add(a, b));
    }


    public override MultivariatePolynomial<E> Subtract(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        return Mod(baseRing.Subtract(a, b));
    }


    public override MultivariatePolynomial<E> Multiply(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        return Mod(baseRing.Multiply(a, b));
    }


    public override MultivariatePolynomial<E> Negate(MultivariatePolynomial<E> element)
    {
        return Mod(baseRing.Negate(element));
    }


    public override MultivariatePolynomial<E> Copy(MultivariatePolynomial<E> element)
    {
        return baseRing.Copy(element);
    }

    public override bool Equal(MultivariatePolynomial<E> x, MultivariatePolynomial<E> y)
    {
        return x.Equals(y);
    }


    public override MultivariatePolynomial<E>[] DivideAndRemainder(MultivariatePolynomial<E> dividend,
        MultivariatePolynomial<E> divider)
    {
        if (baseRing.IsUnit(divider))
            return [Multiply(dividend, baseRing.Reciprocal(divider)), GetZero()];
        if (IsField())
            return [Multiply(dividend, Reciprocal(divider)), GetZero()];
        throw new NotSupportedException("Algebraic structure of ring is unknown");
    }


    public override MultivariatePolynomial<E> Reciprocal(MultivariatePolynomial<E> element)
    {
        if (IsOne(element))
            return element;
        if (IsMinusOne(element))
            return element;
        if (baseRing.IsUnit(element))
            return ValueOf(baseRing.Reciprocal(element));
        if (IsField())
        {
            if (!element.IsConstant())
                element = Mod(element);
            if (!element.IsConstant())
                throw new NotSupportedException("Algebraic structure of ring is unknown");
            return baseRing.GetOne().DivideByLC(element);
        }

        throw new NotSupportedException("Algebraic structure of ring is unknown");
    }


    public override MultivariatePolynomial<E> GetZero()
    {
        return baseRing.GetZero();
    }


    public override MultivariatePolynomial<E> GetOne()
    {
        return baseRing.GetOne();
    }


    public override bool IsZero(MultivariatePolynomial<E> element)
    {
        return baseRing.IsZero(element);
    }


    public override bool IsOne(MultivariatePolynomial<E> element)
    {
        return baseRing.IsOne(element);
    }


    public override bool IsUnit(MultivariatePolynomial<E> element)
    {
        return baseRing.IsUnit(element);
    }


    public override MultivariatePolynomial<E> ValueOfLong(long val)
    {
        return Mod(baseRing.ValueOfLong(val));
    }


    public override MultivariatePolynomial<E> ValueOfBigInteger(BigInteger val)
    {
        return Mod(baseRing.ValueOfBigInteger(val));
    }


    public override MultivariatePolynomial<E> ValueOf(MultivariatePolynomial<E> val)
    {
        return Mod(baseRing.ValueOf(val));
    }


    public override IEnumerable<MultivariatePolynomial<E>> Iterator()
    {
        throw new NotSupportedException("Algebraic structure of ring is unknown");
    }


    public override int Compare(MultivariatePolynomial<E>? o1, MultivariatePolynomial<E>? o2)
    {
        return baseRing.Compare(o1, o2);
    }

    public override object Clone()
    {
        return new QuotientRing<E>((MultivariateRing<E>)baseRing.Clone(), ideal);
    }


    public override MultivariatePolynomial<E> RandomElement(Random rnd)
    {
        return ValueOf(baseRing.RandomElement(rnd));
    }


    public override MultivariatePolynomial<E> RandomElementTree(Random rnd)
    {
        return ValueOf(baseRing.RandomElementTree(rnd));
    }
}