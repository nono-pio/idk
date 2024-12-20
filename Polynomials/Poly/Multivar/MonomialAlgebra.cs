using System.Numerics;

namespace Polynomials.Poly.Multivar;

public class MonomialAlgebra<E>
{
    public readonly Ring<E> ring;

    public MonomialAlgebra(Ring<E> ring)
    {
        this.ring = ring;
    }

    public Monomial<E> Multiply(Monomial<E> a, Monomial<E> b)
    {
        DegreeVector dv = a.DvMultiply(b);
        return new Monomial<E>(dv, ring.Multiply(a.coefficient, b.coefficient));
    }

    public Monomial<E> Multiply(Monomial<E> a, BigInteger b)
    {
        return new Monomial<E>(a.exponents, ring.Multiply(a.coefficient, ring.ValueOfBigInteger(b)));
    }

    public Monomial<E>? DivideOrNull(Monomial<E> dividend, Monomial<E> divider)
    {
        var dv = dividend.DvDivideOrNull(divider);
        if (dv == null)
            return null;
        var div = ring.DivideOrNull(dividend.coefficient, divider.coefficient);
        if (div.IsNull)
            return null;
        return new Monomial<E>(dv, div.Value);
    }

    public Monomial<E> Pow(Monomial<E> term, int exponent)
    {
        if (exponent == 1)
            return term;
        if (exponent == 0)
            return GetUnitTerm(term.NVariables());
        if (term.totalDegree > int.MaxValue / exponent)
            throw new ArithmeticException("overflow");
        int[] exps = new int[term.exponents.Length];
        for (int i = 0; i < exps.Length; ++i)
            exps[i] = term.exponents[i] * exponent;
        return new Monomial<E>(exps, term.totalDegree * exponent, ring.Pow(term.coefficient, exponent));
    }

    public Monomial<E> Negate(Monomial<E> term)
    {
        return term.SetCoefficient(ring.Negate(term.coefficient));
    }

    public bool IsZero(Monomial<E> term)
    {
        return ring.IsZero(term.coefficient);
    }

    public bool IsOne(Monomial<E> term)
    {
        return IsConstant(term) && ring.IsOne(term.coefficient);
    }

    public bool IsUnit(Monomial<E> term)
    {
        return IsConstant(term) && ring.IsUnit(term.coefficient);
    }

    public bool IsPureDegreeVector(Monomial<E> term)
    {
        return ring.IsOne(term.coefficient);
    }

    public Monomial<E> Create(int[] exponents)
    {
        return new Monomial<E>(exponents, ring.GetOne());
    }

    public Monomial<E> Create(DegreeVector degreeVector)
    {
        return new Monomial<E>(degreeVector, ring.GetOne());
    }


    public Monomial<E> GetUnitTerm(int nVariables)
    {
        return new Monomial<E>(nVariables, ring.GetOne());
    }

    public Monomial<E> GetZeroTerm(int nVariables)
    {
        return new Monomial<E>(nVariables, ring.GetZero());
    }

    public bool HaveSameCoefficients(Monomial<E> a, Monomial<E> b)
    {
        return a.Equals(b);
    }


    public Monomial<E> DivideExact(Monomial<E> dividend, Monomial<E> divider)
    {
        var r = DivideOrNull(dividend, divider);
        if (r == null)
            throw new ArithmeticException("not divisible");
        return r;
    }


    public Monomial<E> DivideExact(DegreeVector dividend, Monomial<E> divider)
    {
        return DivideExact(Create(dividend), divider);
    }


    public bool IsConstant(Monomial<E> term)
    {
        return term.IsZeroVector();
    }
}