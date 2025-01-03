using System.Numerics;
using Polynomials.Poly;
using Polynomials.Poly.Multivar;
using Polynomials.Poly.Univar;

namespace Polynomials;

public static class PolynomialFactory
{
    public static UnivariatePolynomial<E> Uni<E>(Ring<E> ring, params E[] data)
    {
        return UnivariatePolynomial<E>.Create(ring, data);
    }

    public static UnivariatePolynomial<Rational<E>> Uni<E>(Rationals<E> ring, params E[] data)
    {
        return UnivariatePolynomial<Rational<E>>.Create(ring, data.Select(ring.MkNumerator).ToArray());
    }
    
    public static UnivariatePolynomial<E> Uni<E>(Ring<E> ring, params long[] data)
    {
        return Uni(ring, ring.ValueOfLong(data));
    }

    public static UnivariatePolynomial<long> UniZ64(params long[] data)
    {
        return UnivariatePolynomial<long>.Create(Rings.Z64, data);
    }


    public static UnivariatePolynomial<long> UniZp64(long modulus, params long[] data)
    {
        return UnivariatePolynomial<long>.Create(Rings.Zp64(modulus), data);
    }


    public static UnivariatePolynomial<BigInteger> UniZ(params BigInteger[] data)
    {
        return UnivariatePolynomial<BigInteger>.Create(Rings.Z, data);
    }


    public static UnivariatePolynomial<BigInteger> UniZp(BigInteger modulus, params BigInteger[] data)
    {
        return UnivariatePolynomial<BigInteger>.Create(Rings.Zp(modulus), data);
    }

    public static MultivariatePolynomial<E> Multi<E>(Ring<E> ring, params Monomial<E>[] monomials)
    {
        var nVars = monomials[0].NVariables();
        return MultivariatePolynomial<E>.Create(nVars, ring, MonomialOrder.DEFAULT, monomials);
    }
    
    public static MultivariatePolynomial<E> Multi<E>(Ring<E> ring, params (E Coef, int[] Degs)[] monomials)
    {
        var nVars = monomials[0].Degs.Length;
        return MultivariatePolynomial<E>.Create(nVars, ring, MonomialOrder.DEFAULT, 
            monomials.Select(m => new Monomial<E>(m.Degs, m.Coef)));
    }
    
    public static MultivariatePolynomial<Rational<E>> Multi<E>(Rationals<E> ring, params (E Coef, int[] Degs)[] monomials)
    {
        var nVars = monomials[0].Degs.Length;
        return MultivariatePolynomial<Rational<E>>.Create(nVars, ring, MonomialOrder.DEFAULT, 
            monomials.Select(m => new Monomial<Rational<E>>(m.Degs, new Rational<E>(ring.ring, m.Coef))));
    }

    public static Rational<Poly> RationalPoly<Poly>(Poly num, Poly den) where Poly : Polynomial<Poly>
    {
        return new Rational<Poly>(num.AsRing(), num, den);
    }
    
    public static Rational<Poly> RationalPoly<Poly>(Poly num) where Poly : Polynomial<Poly>
    {
        return new Rational<Poly>(num.AsRing(), num);
    }
}