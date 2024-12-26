using System.Numerics;
using System.Text;
using Polynomials.Poly.Univar;

namespace Polynomials.Poly.Multivar;

public interface IMultivariateRing
{
    
}
public sealed class MultivariateRing<E> : PolynomialRing<MultivariatePolynomial<E>>, IMultivariateRing
{
    public readonly MultivariatePolynomial<E> factory;

    public MultivariateRing(MultivariatePolynomial<E> factory)
    {
        this.factory = factory.CreateZero();
    }


    public MonomialAlgebra<E> MonomialAlgebra()
    {
        MonomialAlgebra<E> monomialAlgebra = factory.monomialAlgebra;
        return monomialAlgebra;
    }


    public override int NVariables()
    {
        return factory.nVariables;
    }


    public IComparer<DegreeVector> Ordering()
    {
        return factory.ordering;
    }


    public MultivariateRing<E> DropVariable()
    {
        return new MultivariateRing<E>(factory.DropVariable(0));
    }


    public override MultivariatePolynomial<E>[] DivideAndRemainder(MultivariatePolynomial<E> dividend,
        MultivariatePolynomial<E> divider)
    {
        var arr = divider.CreateArray(1);
        arr[0] = divider;
        return MultivariateDivision.DivideAndRemainder(dividend, arr);
    }


    public override MultivariatePolynomial<E> Gcd(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        return MultivariateGCD.PolynomialGCD(a, b);
    }


    public new MultivariatePolynomial<E> Gcd(MultivariatePolynomial<E>[] elements)
    {
        return MultivariateGCD.PolynomialGCD(elements);
    }


    public new MultivariatePolynomial<E> Gcd(IEnumerable<MultivariatePolynomial<E>> elements)
    {
        return MultivariateGCD.PolynomialGCD(elements);
    }


    public override PolynomialFactorDecomposition<MultivariatePolynomial<E>> FactorSquareFree(
        MultivariatePolynomial<E> element)
    {
        throw new NotImplementedException();
        // return (PolynomialFactorDecomposition<MultivariatePolynomial<E>>)
        //     MultivariateSquareFreeFactorization.SquareFreeFactorization(element);
    }


    public override PolynomialFactorDecomposition<MultivariatePolynomial<E>> Factor(MultivariatePolynomial<E> element)
    {
        throw new NotImplementedException();
        // return MultivariateFactorization.Factor(element);
    }


    public override MultivariatePolynomial<E> Variable(int variable)
    {
        return factory.CreateMonomial(variable, 1);
    }


    public MultivariatePolynomial<E> Create(DegreeVector term)
    {
        return factory.Create(term);
    }


    public MultivariatePolynomial<E> RandomElement(int degree, int size, Random rnd)
    {
        throw new NotImplementedException();
        // return RandomMultivariatePolynomials.RandomPolynomial(factory, degree, size, rnd);
    }


    public MultivariatePolynomial<E> RandomElementTree(int degree, int size, Random rnd)
    {
        throw new NotImplementedException();

        // if (factory is MultivariatePolynomial)
        // {
        //     MultivariatePolynomial f = (MultivariatePolynomial)this.factory;
        //     Ring cfRing = f.ring;
        //     Func<Random, TWildcardTodo> method = cfRing.RandomElementTree();
        //     return (Poly)RandomMultivariatePolynomials.RandomPolynomial(NVariables(), degree, size, cfRing,
        //         ((MultivariatePolynomial)factory).ordering, method, rnd);
        // }
        // else
        //     return RandomElement(degree, size, rnd);
    }


    private static readonly Random privateRandom = new Random(DateTime.Now.Nanosecond);


    public MultivariatePolynomial<E> RandomElement(int degree, int size)
    {
        return RandomElement(degree, size, privateRandom);
    }


    public override MultivariatePolynomial<E> RandomElement(Random rnd)
    {
        return ((Ring<MultivariatePolynomial<E>>)this).RandomElement(rnd);
    }


    public static readonly int DEGREE_OF_RANDOM_POLY = 16;


    public static readonly int SIZE_OF_RANDOM_POLY = 16;


    public override MultivariatePolynomial<E> RandomElementTree(Random rnd)
    {
        return RandomElementTree(DEGREE_OF_RANDOM_POLY, SIZE_OF_RANDOM_POLY, rnd);
    }

    public override MultivariatePolynomial<E> Factory()
    {
        return factory;
    }


    public override bool IsEuclideanRing()
    {
        return factory.IsOverField();
    }


    public override bool IsField()
    {
        return false;
    }


    public override BigInteger? Cardinality()
    {
        return null;
    }


    public override BigInteger Characteristic()
    {
        return factory.CoefficientRingCharacteristic();
    }


    public override MultivariatePolynomial<E> Add(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        return a.Clone().Add(b);
    }


    public override MultivariatePolynomial<E> Subtract(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        return a.Clone().Subtract(b);
    }


    public override MultivariatePolynomial<E> Multiply(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        return a.Clone().Multiply(b);
    }


    public override MultivariatePolynomial<E> Negate(MultivariatePolynomial<E> element)
    {
        return element.Clone().Negate();
    }


    public override MultivariatePolynomial<E> Pow(MultivariatePolynomial<E> @base, BigInteger exponent)
    {
        return PolynomialMethods.PolyPow(@base, exponent, true);
    }


    public new MultivariatePolynomial<E> AddMutable(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        return a.Add(b);
    }


    public new MultivariatePolynomial<E> SubtractMutable(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        return a.Subtract(b);
    }


    public new MultivariatePolynomial<E> MultiplyMutable(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        return a.Multiply(b);
    }


    public new MultivariatePolynomial<E> NegateMutable(MultivariatePolynomial<E> element)
    {
        return element.Negate();
    }


    public override MultivariatePolynomial<E> Reciprocal(MultivariatePolynomial<E> element)
    {
        if (element.IsConstant())
            return DivideExact(GetOne(), element);
        throw new ArithmeticException("not divisible: 1 / " + element);
    }


    public override MultivariatePolynomial<E> GetZero()
    {
        return factory.CreateZero();
    }


    public override MultivariatePolynomial<E> GetOne()
    {
        return factory.CreateOne();
    }


    public override bool IsZero(MultivariatePolynomial<E> element)
    {
        return element.IsZero();
    }


    public override bool IsOne(MultivariatePolynomial<E> element)
    {
        return element.IsOne();
    }


    public override bool IsUnit(MultivariatePolynomial<E> element)
    {
        return element.IsOverField() ? element.IsConstant() : (IsOne(element) || IsMinusOne(element));
    }


    public override MultivariatePolynomial<E> ValueOfLong(long val)
    {
        return factory.CreateOne().Multiply(val);
    }


    public override MultivariatePolynomial<E> ValueOfBigInteger(BigInteger val)
    {
        return factory.CreateOne().MultiplyByBigInteger(val);
    }


    public override MultivariatePolynomial<E> ValueOf(MultivariatePolynomial<E> val)
    {
        if (factory.SameCoefficientRingWith(val))
            return val;
        else
            return val.SetCoefficientRingFrom(factory);
    }


    public override MultivariatePolynomial<E> Copy(MultivariatePolynomial<E> element)
    {
        return element.Clone();
    }

    public override bool Equal(MultivariatePolynomial<E> x, MultivariatePolynomial<E> y)
    {
        return x.Equals(y);
    }


    public override int Compare(MultivariatePolynomial<E> o1, MultivariatePolynomial<E> o2)
    {
        return o1.CompareTo(o2);
    }

    public override object Clone()
    {
        return new MultivariateRing<E>(factory);
    }


    public override bool Equals(object? o)
    {
        if (this == o)
            return true;
        if (o == null || GetType() != o.GetType())
            return false;
        var that = (MultivariateRing<E>)o;
        return that.factory.GetType().Equals(factory.GetType()) && factory.SameCoefficientRingWith(that.factory);
    }


    public override int GetHashCode()
    {
        return GetOne().GetHashCode();
    }


    public override IEnumerable<MultivariatePolynomial<E>> Iterator()
    {
        throw new NotSupportedException("Ring of infinite cardinality.");
    }
}