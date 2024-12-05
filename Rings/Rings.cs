using System.Numerics;
using Rings.poly;
using Rings.poly.univar;

namespace Rings;


public sealed class Rings {
    private Rings() {}

    public static RandomGenerator privateRandom = new Well44497b(DateTime.Now.Nanosecond);

    
    public static readonly Integers Z = Integers.ZZ;

    
    public static readonly Rationals<BigInteger> Q = new(Z);

    
    public static Rationals<E> Frac<E>(Ring<E> ring) {
        return new(ring);
    }

    
    public static IntegersZp64 Zp64(long modulus) {return new IntegersZp64(modulus);}

    
    public static IntegersZp Zp(long modulus) {return new IntegersZp(modulus);}

    
    public static IntegersZp Zp(BigInteger modulus) {return new IntegersZp(modulus);}

    
    public static FiniteField<UnivariatePolynomialZp64> GF(long prime, int exponent) {
        if (exponent <= 0)
            throw new ArgumentException("Exponent must be positive");
        // provide random generator with fixed seed to make the behavior predictable
        return new (IrreduciblePolynomials.randomIrreduciblePolynomial(prime, exponent, new Well19937c(0x77f3dfae)));
    }

    
    public static FiniteField<UnivariatePolynomial<BigInteger>> GF(BigInteger prime, int exponent) {
        if (exponent <= 0)
            throw new ArgumentException("Exponent must be positive");
        // provide random generator with fixed seed to make the behavior predictable
        return new (IrreduciblePolynomials.randomIrreduciblePolynomial(Zp(prime), exponent, new Well19937c(0x77f3dfae)));
    }

    
    public static FiniteField<Poly> GF<Poly>(Poly irreducible) where Poly : IUnivariatePolynomial<Poly> {
        return new(irreducible);
    }

    
    public static AlgebraicNumberField<Poly> AlgebraicNumberField<Poly>(Poly minimalPoly) where Poly : IUnivariatePolynomial<Poly> {
        return new (minimalPoly);
    }

    
    public static  AlgebraicNumberField<UnivariatePolynomial<E>> GaussianNumbers<E>(Ring<E> ring) {
        return AlgebraicNumberField(UnivariatePolynomial<E>.create(ring, [ring.getOne(), ring.getZero(), ring.getOne()]));
    }

    
    public static AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> GaussianRationals = GaussianNumbers(Q);

    
    public static AlgebraicNumberField<UnivariatePolynomial<BigInteger>> GaussianIntegers = GaussianNumbers(Z);

    /**
     * Quotient ring {@code baseRing/<modulus> }
     *
     * @deprecated Use either {@link #GF(IUnivariatePolynomial)} or {@link #AlgebraicNumberField(IUnivariatePolynomial)}
     */
    [Obsolete]
    public static SimpleFieldExtension<uPoly> UnivariateQuotientRing<uPoly>(uPoly modulus) where uPoly : IUnivariatePolynomial<uPoly> {
        return SimpleFieldExtension(modulus);
    }

    
    public static SimpleFieldExtension<uPoly> SimpleFieldExtension<uPoly>(uPoly minimalPolynomial) where uPoly : IUnivariatePolynomial<uPoly> {
        return minimalPolynomial.isOverFiniteField() ? GF(minimalPolynomial) : AlgebraicNumberField(minimalPolynomial);
    }

    
    public static MultipleFieldExtension<Term, mPoly, sPoly> MultipleFieldExtension<Term, mPoly, sPoly>(params sPoly[] minimalPolynomials) 
                where Term : AMonomial<Term> 
                where mPoly : AMultivariatePolynomial<Term, mPoly> 
                where sPoly : IUnivariatePolynomial<sPoly> {
        return MultipleFieldExtension.mkMultipleExtension(minimalPolynomials);
    }

    
    public static MultipleFieldExtension<Term, mPoly, sPoly> SplittingField<Term, mPoly, sPoly>(sPoly polynomial)
            where Term : AMonomial<Term>
            where mPoly : AMultivariatePolynomial<Term, mPoly>
            where sPoly : IUnivariatePolynomial<sPoly> {
        return MultipleFieldExtension.mkSplittingField(polynomial);
    }

    
    public static UnivariateRing<UnivariatePolynomial<E>> UnivariateRing<E>(Ring<E> coefficientRing) {
        return new (UnivariatePolynomial<E>.zero(coefficientRing));
    }

    
    public static UnivariateRing<Poly> UnivariateRing<Poly>(Poly factory) where Poly : IUnivariatePolynomial<Poly> {
        return new(factory);
    }

    
    public static readonly UnivariateRing<UnivariatePolynomial<BigInteger>> UnivariateRingZ = UnivariateRing(Z);

    
    public static readonly UnivariateRing<UnivariatePolynomial<Rational<BigInteger>>> UnivariateRingQ = UnivariateRing(Q);

    
    public static UnivariateRing<UnivariatePolynomialZp64> UnivariateRingZp64(long modulus) {
        return new (UnivariatePolynomialZp64.zero(modulus));
    }

    
    public static UnivariateRing<UnivariatePolynomialZp64> UnivariateRingZp64(IntegersZp64 modulus) {
        return new (UnivariatePolynomialZp64.zero(modulus));
    }

    
    public static UnivariateRing<UnivariatePolynomial<BigInteger>> UnivariateRingZp(BigInteger modulus) {
        return UnivariateRing(Zp(modulus));
    }

    
    public static  MultivariateRing<MultivariatePolynomial<E>> MultivariateRing<E>(int nVariables, Ring<E> coefficientRing, Comparator<DegreeVector> monomialOrder) {
        return new (MultivariatePolynomial.zero(nVariables, coefficientRing, monomialOrder));
    }

    
    public static MultivariateRing<MultivariatePolynomial<E>> MultivariateRing<E>(int nVariables, Ring<E> coefficientRing) {
        return MultivariateRing(nVariables, coefficientRing, MonomialOrder.DEFAULT);
    }

    
    public static MultivariateRing<Poly> MultivariateRing<Term, Poly>(Poly factory) 
        where Term : AMonomial<Term>
        where Poly : AMultivariatePolynomial<Term, Poly> {
        return new (factory);
    }

    
    public static MultivariateRing<MultivariatePolynomial<BigInteger>> MultivariateRingZ(int nVariables) {
        return MultivariateRing(nVariables, Z);
    }

    
    public static MultivariateRing<MultivariatePolynomial<Rational<BigInteger>>> MultivariateRingQ(int nVariables) {
        return MultivariateRing(nVariables, Q);
    }

    
    public static MultivariateRing<MultivariatePolynomialZp64> MultivariateRingZp64(int nVariables, long modulus, Comparator<DegreeVector> monomialOrder) {
        return new (MultivariatePolynomialZp64.zero(nVariables, Zp64(modulus), monomialOrder));
    }

    
    public static MultivariateRing<MultivariatePolynomialZp64> MultivariateRingZp64(int nVariables, long modulus) {
        return MultivariateRingZp64(nVariables, modulus, MonomialOrder.DEFAULT);
    }

    
    public static MultivariateRing<MultivariatePolynomialZp64> MultivariateRingZp64(int nVariables, IntegersZp64 modulus, Comparator<DegreeVector> monomialOrder) {
        return new (MultivariatePolynomialZp64.zero(nVariables, modulus, monomialOrder));
    }

    
    public static MultivariateRing<MultivariatePolynomialZp64> MultivariateRingZp64(int nVariables, IntegersZp64 modulus) {
        return MultivariateRingZp64(nVariables, modulus, MonomialOrder.DEFAULT);
    }

    
    public static MultivariateRing<MultivariatePolynomial<BigInteger>> MultivariateRingZp(int nVariables, BigInteger modulus) {
        return MultivariateRing(nVariables, Zp(modulus));
    }

    public static IPolynomialRing<Poly> PolynomialRing<Poly>(Poly factory) where Poly : IUnivariatePolynomial<Poly>
    {
        return UnivariateRing(factory);
    }

    // TODO
    public static IPolynomialRing<Poly> PolynomialRing<Poly>(Poly factory) where Poly : AMultivariatePolynomial<Poly>
    {
        return (IPolynomialRing<Poly>) MultivariateRing(factory);
    }

    /**
     * Quotient ring {@code baseRing/<ideal> }
     */
    public static QuotientRing<Term, Poly> QuotientRing<Term, Poly>(MultivariateRing<Poly> baseRing, Ideal<Term, Poly> ideal)
        where Term : AMonomial<Term>
        where Poly : AMultivariatePolynomial<Term, Poly> {
        return new (baseRing, ideal);
    }
}
