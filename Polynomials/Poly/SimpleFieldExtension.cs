using System.Numerics;
using Polynomials.Poly.Multivar;
using Polynomials.Poly.Univar;
using Polynomials.Utils;
using static Polynomials.Poly.Univar.UnivariatePolynomialArithmetic;

namespace Polynomials.Poly;

public abstract class SimpleFieldExtension<E> : PolynomialRing<UnivariatePolynomial<E>>
{


    public readonly UnivariatePolynomial<E> minimalPoly;


    public readonly UnivariatePolynomial<E> factory;


    readonly UnivariateDivision.InverseModMonomial<E> inverseMod;


    readonly BigInteger? cardinality;


    protected SimpleFieldExtension(UnivariatePolynomial<E> minimalPoly)
    {
        minimalPoly = minimalPoly.Monic();
        if (minimalPoly == null)
            throw new ArgumentException("Minimal polynomial must be monic");
        this.minimalPoly = minimalPoly;
        this.factory = minimalPoly.Clone();
        this.inverseMod = UnivariateDivision.FastDivisionPreConditioning(minimalPoly);
        this.cardinality = minimalPoly.CoefficientRingCardinality() is null
            ? null
            : BigIntegerUtils.Pow(minimalPoly.CoefficientRingCardinality().Value, minimalPoly.Degree());
    }


    public virtual bool IsInTheBaseField(UnivariatePolynomial<E> element)
    {
        return element.IsConstant();
    }


    public virtual UnivariatePolynomial<E> Generator()
    {
        return minimalPoly.CreateMonomial(1);
    }


    public virtual int Degree()
    {
        return minimalPoly.Degree();
    }


    public virtual UnivariatePolynomial<E> GetMinimalPolynomial()
    {
        return minimalPoly.Clone();
    }


    public virtual UnivariatePolynomial<E> GetMinimalPolynomialRef()
    {
        return minimalPoly;
    }


    public virtual UnivariatePolynomial<E> Norm(UnivariatePolynomial<E> element)
    {
        return UnivariateResultants.ResultantAsPoly(minimalPoly, element);
    }


    public virtual UnivariatePolynomial<E> ConjugatesProduct(UnivariatePolynomial<E> element)
    {
        return DivideExact(Norm(element), element);
    }

    // TODO
    // public virtual UnivariatePolynomial<E> Trace(UnivariatePolynomial<E> element)
    // {
    //     var minimalPoly = MinimalPolynomial(element);
    //     return Negate(DivideExact(minimalPoly.GetAsPoly(minimalPoly.Degree() - 1), minimalPoly.LcAsPoly()));
    // }


    // TODO
    // public virtual UnivariatePolynomial<E> NormOfPolynomial(UnivariatePolynomial<UnivariatePolynomial<E>> poly)
    // {
    //     if (!poly.ring.Equals(this))
    //         throw new ArgumentException();
    //     
    //     return MultivariateResultants.Resultant(minimalPoly.AsMultivariate(MonomialOrder.DEFAULT).SetNVariables(2),
    //         MultivariatePolynomial<E>.AsNormalMultivariate(poly.AsMultivariate(), 0), 0).AsUnivariate();
    // }
    //
    //
    // public virtual MultivariatePolynomial<E> NormOfPolynomial(MultivariatePolynomial<UnivariatePolynomial<E>> poly)
    // {
    //     if (!poly.ring.Equals(this))
    //         throw new ArgumentException();
    //    
    //     return MultivariateResultants
    //         .Resultant(minimalPoly.AsMultivariate(MonomialOrder.DEFAULT).SetNVariables(poly.nVariables + 1),
    //             MultivariatePolynomial<E>.AsNormalMultivariate(poly, 0), 0).DropVariable(0);
    // }

    // TODO
    // public virtual UnivariatePolynomial<E> MinimalPolynomial(UnivariatePolynomial<E> element)
    // {
    //     //if (element.equals(getOne()))
    //     //    return getMinimalPolynomial();
    //     var es = UnivariatePolynomial<UnivariatePolynomial<E>>.Create(this, [Negate(element), GetOne()]);
    //     return UnivariateSquareFreeFactorization.SquareFreePart(NormOfPolynomial(es));
    // }


    // TODO
    // public virtual MultipleFieldExtension<Term, mPoly, E> AsMultipleExtension<Term extends AMonomial<Term>, mPoly
    //     extends AMultivariatePolynomial<Term, mPoly>>()
    // {
    //     return MultipleFieldExtension.MkMultipleExtension(this);
    // }


    public override int NVariables()
    {
        return 1;
    }


    public override UnivariatePolynomial<E> Factory()
    {
        return factory;
    }


    public override bool IsEuclideanRing()
    {
        return minimalPoly.IsOverField();
    }


    public override BigInteger? Cardinality()
    {
        return cardinality;
    }


    public override BigInteger Characteristic()
    {
        return minimalPoly.CoefficientRingCharacteristic();
    }


    protected virtual bool ShouldReduceFast(int dividendDegree)
    {
        int mDeg = minimalPoly.Degree();
        if (dividendDegree < mDeg)
            return false;
        if (IsFiniteField())
        {
            if (mDeg < 8)
                return false;
            int defect = dividendDegree / mDeg;
            if (mDeg <= 20)
                return defect <= 16;
            else
                return defect <= 30;
        }
        else
            return false;
    }


    public override UnivariatePolynomial<E> Add(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return ShouldReduceFast(Math.Max(a.Degree(), b.Degree()))
            ? PolyAddMod(a, b, minimalPoly, inverseMod, true)
            : PolyAddMod(a, b, minimalPoly, true);
    }


    public override UnivariatePolynomial<E> Subtract(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return ShouldReduceFast(Math.Max(a.Degree(), b.Degree()))
            ? PolySubtractMod(a, b, minimalPoly, inverseMod, true)
            : PolySubtractMod(a, b, minimalPoly, true);
    }


    public override UnivariatePolynomial<E> Multiply(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return ShouldReduceFast(a.Degree() + b.Degree())
            ? PolyMultiplyMod(a, b, minimalPoly, inverseMod, true)
            : PolyMultiplyMod(a, b, minimalPoly, true);
    }


    public override UnivariatePolynomial<E> Negate(UnivariatePolynomial<E> element)
    {
        return ShouldReduceFast(element.Degree())
            ? PolyNegateMod(element, minimalPoly, inverseMod, true)
            : PolyNegateMod(element, minimalPoly, true);
    }


    public new UnivariatePolynomial<E> AddMutable(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return ShouldReduceFast(Math.Max(a.Degree(), b.Degree()))
            ? PolyAddMod(a, b, minimalPoly, inverseMod, false)
            : PolyAddMod(a, b, minimalPoly, false);
    }


    public new UnivariatePolynomial<E> SubtractMutable(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return ShouldReduceFast(Math.Max(a.Degree(), b.Degree()))
            ? PolySubtractMod(a, b, minimalPoly, inverseMod, false)
            : PolySubtractMod(a, b, minimalPoly, false);
    }


    public new UnivariatePolynomial<E> MultiplyMutable(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return ShouldReduceFast(a.Degree() + b.Degree())
            ? PolyMultiplyMod(a, b, minimalPoly, inverseMod, false)
            : PolyMultiplyMod(a, b, minimalPoly, false);
    }


    public new UnivariatePolynomial<E> NegateMutable(UnivariatePolynomial<E> element)
    {
        return ShouldReduceFast(element.Degree())
            ? PolyNegateMod(element, minimalPoly, inverseMod, false)
            : PolyNegateMod(element, minimalPoly, false);
    }


    public override UnivariatePolynomial<E> Reciprocal(UnivariatePolynomial<E> element)
    {
        if (element.IsZero())
            throw new ArithmeticException("divide by zero");
        if (IsOne(element))
            return element;
        if (IsMinusOne(element))
            return element;
        UnivariatePolynomial<E>[] xgcd = UnivariateGCD.PolynomialFirstBezoutCoefficient(element, minimalPoly);
        return xgcd[1].DivideByLC(xgcd[0]);
    }


    public override FactorDecomposition<UnivariatePolynomial<E>> Factor(UnivariatePolynomial<E> element)
    {
        return FactorDecomposition<UnivariatePolynomial<E>>.FromUnit(this, element);
    }


    public override UnivariatePolynomial<E> GetZero()
    {
        return minimalPoly.CreateZero();
    }


    public override UnivariatePolynomial<E> GetOne()
    {
        return minimalPoly.CreateOne();
    }


    public override bool IsZero(UnivariatePolynomial<E> element)
    {
        return element.IsZero();
    }


    public override bool IsOne(UnivariatePolynomial<E> element)
    {
        return element.IsOne();
    }


    public override UnivariatePolynomial<E> ValueOfLong(long val)
    {
        return GetOne().Multiply(val);
    }


    public override UnivariatePolynomial<E> ValueOfBigInteger(BigInteger val)
    {
        return GetOne().MultiplyByBigInteger(val);
    }


    public override UnivariatePolynomial<E> ValueOf(UnivariatePolynomial<E> val)
    {
        return ShouldReduceFast(val.Degree())
            ? PolyMod(val.SetCoefficientRingFrom(factory), minimalPoly, inverseMod, false)
            : PolyMod(val.SetCoefficientRingFrom(factory), minimalPoly, false);
    }


    public override UnivariatePolynomial<E> Copy(UnivariatePolynomial<E> element)
    {
        return element.Clone();
    }


    public override int Compare(UnivariatePolynomial<E> o1, UnivariatePolynomial<E> o2)
    {
        return o1.CompareTo(o2);
    }


    public override UnivariatePolynomial<E> RandomElement(Random rnd)
    {
        UnivariatePolynomial<E> r =
            RandomUnivariatePolynomials.RandomPoly(minimalPoly, rnd.Next(minimalPoly.Degree()), rnd);
        if (r.IsOverFiniteField())
        {
            r.Multiply(r.ring.RandomElement(rnd));
        }

        return r;
    }


    public override UnivariatePolynomial<E> Variable(int variable)
    {
        if (variable != 0)
            throw new ArgumentException();
        return ValueOf(minimalPoly.CreateMonomial(1));
    }


    public override bool Equals(object? o)
    {
        if (this == o)
            return true;
        if (o == null || GetType() != o.GetType())
            return false;
        SimpleFieldExtension<E> that = (SimpleFieldExtension<E>)o;
        return minimalPoly.Equals(that.minimalPoly);
    }


    public override int GetHashCode()
    {
        return minimalPoly.GetHashCode();
    }
}