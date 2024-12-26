using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using System.Text;

namespace Polynomials.Poly.Univar;

public interface IUnivariateRing
{
    
}

public sealed class UnivariateRing<E> : PolynomialRing<UnivariatePolynomial<E>>, IUnivariateRing
{
    public readonly UnivariatePolynomial<E> factory;


    public UnivariateRing(UnivariatePolynomial<E> factory)
    {
        this.factory = factory.CreateZero();
    }


    public override UnivariatePolynomial<E> Factory()
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


    public override UnivariatePolynomial<E> Add(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return a.Clone().Add(b);
    }


    public override UnivariatePolynomial<E> Subtract(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return a.Clone().Subtract(b);
    }


    public override UnivariatePolynomial<E> Multiply(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return a.Clone().Multiply(b);
    }


    public override UnivariatePolynomial<E> Negate(UnivariatePolynomial<E> element)
    {
        return element.Clone().Negate();
    }


    public override UnivariatePolynomial<E> Pow(UnivariatePolynomial<E> @base, BigInteger exponent)
    {
        return PolynomialMethods.PolyPow(@base, exponent, true);
    }


    public new UnivariatePolynomial<E> AddMutable(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return a.Add(b);
    }


    public new UnivariatePolynomial<E> SubtractMutable(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return a.Subtract(b);
    }


    public new UnivariatePolynomial<E> MultiplyMutable(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return a.Multiply(b);
    }


    public new UnivariatePolynomial<E> NegateMutable(UnivariatePolynomial<E> element)
    {
        return element.Negate();
    }


    public override UnivariatePolynomial<E> Reciprocal(UnivariatePolynomial<E> element)
    {
        if (element.IsConstant())
            return DivideExact(GetOne(), element);
        throw new ArithmeticException("not divisible: 1 / " + element);
    }


    public override UnivariatePolynomial<E> GetZero()
    {
        return factory.CreateZero();
    }


    public override UnivariatePolynomial<E> GetOne()
    {
        return factory.CreateOne();
    }


    public override bool IsZero(UnivariatePolynomial<E> element)
    {
        return element.IsZero();
    }


    public override bool IsOne(UnivariatePolynomial<E> element)
    {
        return element.IsOne();
    }


    public override bool IsUnit(UnivariatePolynomial<E> element)
    {
        return element.IsOverField() ? element.IsConstant() : (IsOne(element) || IsMinusOne(element));
    }


    public override UnivariatePolynomial<E> ValueOfLong(long val)
    {
        return factory.CreateOne().Multiply(val);
    }


    public override UnivariatePolynomial<E> ValueOfBigInteger(BigInteger val)
    {
        return factory.CreateOne().MultiplyByBigInteger(val);
    }


    public override UnivariatePolynomial<E> ValueOf(UnivariatePolynomial<E> val)
    {
        if (factory.SameCoefficientRingWith(val))
            return val;
        else
            return val.SetCoefficientRingFrom(factory);
    }


    public override UnivariatePolynomial<E> Copy(UnivariatePolynomial<E> element)
    {
        return element.Clone();
    }

    public override bool Equal(UnivariatePolynomial<E> x, UnivariatePolynomial<E> y)
    {
        return x.Equals(y);
    }


    public override int Compare(UnivariatePolynomial<E> o1, UnivariatePolynomial<E> o2)
    {
        return o1.CompareTo(o2);
    }

    public override object Clone()
    {
        return new UnivariateRing<E>(factory);
    }


    public override bool Equals(object? o)
    {
        if (this == o)
            return true;
        if (o == null || GetType() != o.GetType())
            return false;
        UnivariateRing<E> that = (UnivariateRing<E>)o;
        return that.factory.GetType().Equals(factory.GetType()) && factory.SameCoefficientRingWith(that.factory);
    }


    public override int GetHashCode()
    {
        return GetOne().GetHashCode();
    }


    public override IEnumerable<UnivariatePolynomial<E>> Iterator()
    {
        throw new NotSupportedException("Ring of infinite cardinality.");
    }


    public override int NVariables()
    {
        return 1;
    }


    public override UnivariatePolynomial<E> Remainder(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return UnivariateDivision.Remainder(a, b, true);
    }


    public override UnivariatePolynomial<E>[]? DivideAndRemainder(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return UnivariateDivision.DivideAndRemainder(a, b, true);
    }


    public override UnivariatePolynomial<E> Gcd(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return UnivariateGCD.PolynomialGCD(a, b);
    }


    public override UnivariatePolynomial<E>[] ExtendedGCD(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return UnivariateGCD.PolynomialExtendedGCD(a, b);
    }


    public override UnivariatePolynomial<E>[] FirstBezoutCoefficient(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return UnivariateGCD.PolynomialFirstBezoutCoefficient(a, b);
    }


    public override PolynomialFactorDecomposition<UnivariatePolynomial<E>> FactorSquareFree(UnivariatePolynomial<E> element)
    {
        return UnivariateSquareFreeFactorization.SquareFreeFactorization(element);
    }


    public override PolynomialFactorDecomposition<UnivariatePolynomial<E>> Factor(UnivariatePolynomial<E> element)
    {
        return UnivariateFactorization.Factor(element);
    }


    public override UnivariatePolynomial<E> Variable(int variable)
    {
        if (variable != 0)
            throw new ArgumentException();
        return factory.CreateMonomial(1);
    }


    public UnivariatePolynomial<E> RandomElement(int minDegree, int maxDegree, Random rnd)
    {
        return RandomUnivariatePolynomials.RandomPoly(factory,
            minDegree + (minDegree == maxDegree ? 0 : rnd.Next(maxDegree - minDegree)), rnd);
    }


    public UnivariatePolynomial<E> RandomElement(int degree, Random rnd)
    {
        return RandomElement(degree, degree, rnd);
    }


    public static readonly int MIN_DEGREE_OF_RANDOM_POLY = 0;


    public static readonly int MAX_DEGREE_OF_RANDOM_POLY = 32;


    public override UnivariatePolynomial<E> RandomElement(Random rnd)
    {
        return RandomElement(MIN_DEGREE_OF_RANDOM_POLY, MAX_DEGREE_OF_RANDOM_POLY, rnd);
    }


    public UnivariatePolynomial<E> RandomElementTree(int minDegree, int maxDegree, Random rnd)
    {
        return RandomElement(minDegree, maxDegree, rnd);
    }


    public override UnivariatePolynomial<E> RandomElementTree(Random rnd)
    {
        return RandomElementTree(MIN_DEGREE_OF_RANDOM_POLY, MAX_DEGREE_OF_RANDOM_POLY, rnd);
    }
}