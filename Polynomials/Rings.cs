using System.Numerics;
using Polynomials.Poly;
using Polynomials.Poly.Multivar;
using Polynomials.Poly.Univar;

namespace Polynomials;

public static class Rings
{
    public static Random privateRandom = new Random(DateTime.Now.Nanosecond);
    public static Integers Z = new Integers();
    public static Rationals<BigInteger> Q = new Rationals<BigInteger>(Z);
    public static Integers64 Z64 = new Integers64();
    public static IntegersZp64 Zp64(long modulus) => new IntegersZp64(modulus);
    public static IntegersZp Zp(BigInteger modulus) => new IntegersZp(modulus);
    public static UnivariateRing<BigInteger> UnivariateRingZ => UnivariateRing(Z);

    public static UnivariateRing<BigInteger> UnivariateRingZp(BigInteger modulus)
        => UnivariateRing(new UnivariatePolynomial<BigInteger>(Zp(modulus), [0]));

    public static UnivariateRing<long> UnivariateRingZp64(long modulus)
        => UnivariateRing(new UnivariatePolynomial<long>(Zp64(modulus), [0]));

    public static Ring<Poly> PolynomialRing<Poly>(Poly unit) where Poly : Polynomial<Poly>
    {
        return unit.AsRing();
    }

    public static UnivariateRing<E> UnivariateRing<E>(UnivariatePolynomial<E> factory)
    {
        return new UnivariateRing<E>(factory);
    }

    public static UnivariateRing<E> UnivariateRing<E>(Ring<E> ring) =>
        UnivariateRing(new UnivariatePolynomial<E>(ring, [ring.GetZero()]));


    public static MultivariateRing<E> MultivariateRing<E>(MultivariatePolynomial<E> factory)
    {
        return new MultivariateRing<E>(factory);
    }

    public static MultivariateRing<E> MultivariateRing<E>(int nVars, Ring<E> ring)
    {
        return new MultivariateRing<E>(new MultivariatePolynomial<E>(nVars, ring, MonomialOrder.DEFAULT,
            new MonomialSet<E>(MonomialOrder.DEFAULT)));
    }

    public static SimpleFieldExtension<E> SimpleFieldExtension<E>(UnivariatePolynomial<E> minimalPolynomial)
    {
        return minimalPolynomial.IsOverFiniteField() ? GF(minimalPolynomial) : AlgebraicNumberField(minimalPolynomial);
    }

    public static AlgebraicNumberField<E> AlgebraicNumberField<E>(UnivariatePolynomial<E> minimalPolynomial)
    {
        return new AlgebraicNumberField<E>(minimalPolynomial);
    }

    public static FiniteField<E> GF<E>(UnivariatePolynomial<E> minimalPolynomial)
    {
        return new FiniteField<E>(minimalPolynomial);
    }


    public static Rationals<E> Frac<E>(Ring<E> ring)
    {
        return new Rationals<E>(ring);
    }


    public static FiniteField<long> GF(long prime, int exponent)
    {
        if (exponent <= 0)
            throw new ArgumentException("Exponent must be positive");

        // provide random generator with fixed seed to make the behavior predictable
        return new FiniteField<long>(
            IrreduciblePolynomials.RandomIrreduciblePolynomial(prime, exponent, new Random(0x77f3dfae)));
    }


    public static FiniteField<BigInteger> GF(BigInteger prime, int exponent)
    {
        if (exponent <= 0)
            throw new ArgumentException("Exponent must be positive");

        // provide random generator with fixed seed to make the behavior predictable
        return new FiniteField<BigInteger>(
            IrreduciblePolynomials.RandomIrreduciblePolynomial(Zp(prime), exponent, new Random(0x77f3dfae)));
    }


    public static AlgebraicNumberField<E> GaussianNumbers<E>(Ring<E> ring)
    {
        return AlgebraicNumberField(UnivariatePolynomial<E>.Create(ring,
            [ring.GetOne(), ring.GetZero(), ring.GetOne()]));
    }


    public static AlgebraicNumberField<Rational<BigInteger>> GaussianRationals =
        GaussianNumbers(Q);


    public static AlgebraicNumberField<BigInteger> GaussianIntegers = GaussianNumbers(Z);


    public static SimpleFieldExtension<E> UnivariateQuotientRing<E>(UnivariatePolynomial<E> modulus)
    {
        return SimpleFieldExtension(modulus);
    }


    public static MultipleFieldExtension<E>
        MultipleFieldExtension<E>(params UnivariatePolynomial<E>[] minimalPolynomials) 
    {
        return Poly.MultipleFieldExtension<E>.MkMultipleExtension(minimalPolynomials);
    }


    public static MultipleFieldExtension<E> SplittingField<E>(UnivariatePolynomial<E> polynomial)
    {
        return Poly.MultipleFieldExtension<E>.MkSplittingField(polynomial);
    }

    
    public static readonly UnivariateRing<Rational<BigInteger>> UnivariateRingQ =
        UnivariateRing(Q);
    

    public static UnivariateRing<long> UnivariateRingZp64(IntegersZp64 modulus)
    {
        return new UnivariateRing<long>(UnivariatePolynomial<long>.Zero(modulus));
    }


  


    public static MultivariateRing<E> MultivariateRing<E>(int nVariables,
        Ring<E> coefficientRing, IComparer<DegreeVector> monomialOrder)
    {
        return new MultivariateRing<E>(
            MultivariatePolynomial<E>.Zero(nVariables, coefficientRing, monomialOrder));
    }


    public static MultivariateRing<BigInteger> MultivariateRingZ(int nVariables)
    {
        return MultivariateRing(nVariables, Z);
    }


    public static MultivariateRing<Rational<BigInteger>> MultivariateRingQ(int nVariables)
    {
        return MultivariateRing(nVariables, Q);
    }


    public static MultivariateRing<long> MultivariateRingZp64(int nVariables, long modulus,
        IComparer<DegreeVector> monomialOrder)
    {
        return new MultivariateRing<long>(
            MultivariatePolynomial<long>.Zero(nVariables, Zp64(modulus), monomialOrder));
    }


    public static MultivariateRing<long> MultivariateRingZp64(int nVariables, long modulus)
    {
        return MultivariateRingZp64(nVariables, modulus, MonomialOrder.DEFAULT);
    }


    public static MultivariateRing<long> MultivariateRingZp64(int nVariables,
        IntegersZp64 modulus, IComparer<DegreeVector> monomialOrder)
    {
        return new MultivariateRing<long>(
            MultivariatePolynomial<long>.Zero(nVariables, modulus, monomialOrder));
    }


    public static MultivariateRing<long> MultivariateRingZp64(int nVariables,
        IntegersZp64 modulus)
    {
        return MultivariateRingZp64(nVariables, modulus, MonomialOrder.DEFAULT);
    }


    public static MultivariateRing<BigInteger> MultivariateRingZp(int nVariables,
        BigInteger modulus)
    {
        return MultivariateRing(nVariables, Zp(modulus));
    }


    public static PolynomialRing<UnivariatePolynomial<E>> PolynomialRing<E>(UnivariatePolynomial<E> factory)
        => UnivariateRing(factory);

    public static PolynomialRing<MultivariatePolynomial<E>> PolynomialRing<E>(MultivariatePolynomial<E> factory)
        => MultivariateRing(factory);




    public static QuotientRing<E> QuotientRing<E>(MultivariateRing<E> baseRing,
        Ideal<E> ideal)
    {
        return new QuotientRing<E>(baseRing, ideal);
    }
}