using Polynomials.Poly.Univar;

namespace Polynomials.Poly;

using UnivariatePolynomialZ64 = Polynomials.Poly.Univar.UnivariatePolynomial<long>;

public sealed class FiniteField<E> : SimpleFieldExtension<E>
{


    public static readonly FiniteField<long> GF27 =
        new FiniteField<long>(UnivariatePolynomialZ64.Create(-1, -1, 0, 1).Modulus(3));


    public static readonly FiniteField<long> GF17p5 =
        new FiniteField<long>(UnivariatePolynomialZ64.Create(11, 11, 0, 3, 9, 9).Modulus(17).Monic()!);


    public FiniteField(UnivariatePolynomial<E> minimalPoly) : base(minimalPoly)
    {
        if (!minimalPoly.IsOverFiniteField())
            throw new ArgumentException("Irreducible poly must be over finite field.");
    }


    public override bool IsField()
    {
        return true;
    }


    public override bool IsUnit(UnivariatePolynomial<E> element)
    {
        return !element.IsZero();
    }


    public override UnivariatePolynomial<E> Gcd(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return a;
    }


    public override bool Equal(UnivariatePolynomial<E> x, UnivariatePolynomial<E> y)
    {
        return x.Equals(y);
    }

    public override object Clone()
    {
        return new FiniteField<E>(minimalPoly.Clone());
    }

    public override UnivariatePolynomial<E>[] DivideAndRemainder(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return [Multiply(a, Reciprocal(b)), GetZero()];
    }


    public override UnivariatePolynomial<E> Remainder(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider)
    {
        return GetZero();
    }


    public override IEnumerable<UnivariatePolynomial<E>> Iterator()
    {
        if (!IsFinite())
            throw new Exception("Ring of infinite cardinality.");
        
        var degree = Degree();
        var ring = minimalPoly.ring;
        var iterators = new IEnumerator<E>[degree];
        var data = new E[degree];
        for (int k = 0; k < iterators.Length; k++)
        {
            iterators[k] = ring.Iterator().GetEnumerator();
            iterators[k].MoveNext();
            data[k] = iterators[k].Current;   
        }
        
        yield return UnivariatePolynomial<E>.Create(ring, (E[])data.Clone());

        while (true)
        {
            int i = 0;
            if (!iterators[i].MoveNext())
                while (i < iterators.Length && !iterators[i].MoveNext())
                {
                    iterators[i] = ring.Iterator().GetEnumerator();
                    iterators[i].MoveNext();
                    data[i] = iterators[i].Current;
                    ++i;
                }

            if (i >= iterators.Length)
                yield break;
        
            data[i] = iterators[i].Current;
            yield return UnivariatePolynomial<E>.CreateUnsafe(ring, (E[])data.Clone());
        }
    }
}