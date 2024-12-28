using Polynomials.Poly.Univar;

namespace Polynomials.Poly;

public class AlgebraicNumberField<E> : SimpleFieldExtension<E>
{
    public AlgebraicNumberField(UnivariatePolynomial<E> minimalPoly) : base(minimalPoly)
    {
        if (minimalPoly.IsOverFiniteField())
            throw new ArgumentException("Use FiniteField for constructing extensions of finite fields.");
    }


    public override bool IsField()
    {
        return minimalPoly.IsOverField();
    }


    public override bool IsUnit(UnivariatePolynomial<E> element)
    {
        return (IsField() && !IsZero(element)) || (IsOne(element) || IsMinusOne(element));
    }


    public override UnivariatePolynomial<E> Gcd(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        // NOTE: don't change this
        return IsField() ? a : UnivariateGCD.PolynomialGCD(a, b);
    }


    // NOTE: don't change this
    public override bool Equal(UnivariatePolynomial<E> x, UnivariatePolynomial<E> y)
    {
        return x.Equals(y);
    }

    public override object Clone()
    {
        return new AlgebraicNumberField<E>(minimalPoly.Clone());
    }

    public override UnivariatePolynomial<E>[] DivideAndRemainder(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        if (IsField())
            return [Multiply(a, Reciprocal(b)), GetZero()];

        // the following code has no any particular math meaning,
        // just to make some programming things easier
        var cancel = Normalizer2(b);
        var cb = cancel[0];
        var bcb = cancel[1];
        var pseudoQuot = Multiply(a, cb);
        var quot = pseudoQuot.Clone().DivideByLC(bcb);
        if (quot != null)
            return [quot, GetZero()];
        return UnivariateDivision.DivideAndRemainder(a, b, true);
    }


    public override UnivariatePolynomial<E> Remainder(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider)
    {
        return DivideAndRemainder(dividend, divider)[1];
    }




    public virtual UnivariatePolynomial<E> Normalizer(UnivariatePolynomial<E> element)
    {
        return Normalizer2(element)[0];
    }




    public virtual UnivariatePolynomial<E>[] Normalizer2(UnivariatePolynomial<E> element)
    {
        if (IsField())
            return [Reciprocal(element), GetOne()];
        if (element.IsZero())
            throw new ArithmeticException("divide by zero");
        if (IsOne(element))
            return [element, GetOne()];
        if (IsMinusOne(element))
            return [element, GetOne()];
        var xgcd = UnivariateGCD.PolynomialExtendedGCD(element, minimalPoly);
        var conjugate = xgcd[1];
        var content = conjugate.ContentAsPoly();
        return [conjugate, xgcd[0].DivideByLC(content)];
    }




    public override IEnumerable<UnivariatePolynomial<E>> Iterator()
    {
        throw new NotSupportedException("this field has infinite cardinality");
    }
}