using System.Runtime.InteropServices.JavaScript;
using Rings.poly.univar;

namespace Rings.poly;


public sealed class UnivariateRing<Poly> : APolynomialRing<Poly> where Poly : IUnivariatePolynomial<Poly> {
    private static readonly long serialVersionUID = 1L;

    
    public UnivariateRing(Poly factory) : base(factory) { }

    
    public override int nVariables() { return 1; }

    
    public Poly remainder(Poly a, Poly b) {return UnivariateDivision.remainder(a, b, true);}

    
    public override Poly[] divideAndRemainder(Poly a, Poly b) {return UnivariateDivision.divideAndRemainder(a, b, true);}

    
    public Poly gcd(Poly a, Poly b) {return UnivariateGCD.PolynomialGCD(a, b);}

    
    public Poly[] extendedGCD(Poly a, Poly b) {
        return UnivariateGCD.PolynomialExtendedGCD(a, b);
    }

    
    public Poly[] firstBezoutCoefficient(Poly a, Poly b) {
        return UnivariateGCD.PolynomialFirstBezoutCoefficient(a, b);
    }

    
    public PolynomialFactorDecomposition<Poly> factorSquareFree(Poly element) {
        return UnivariateSquareFreeFactorization.SquareFreeFactorization(element);
    }

    
    public PolynomialFactorDecomposition<Poly> factor(Poly element) {
        return UnivariateFactorization.Factor(element);
    }

    
    public override Poly variable(int variable) {
        if (variable != 0)
            throw new ArgumentException();
        return Factory.createMonomial(1);
    }

    
    public Poly randomElement(int minDegree, int maxDegree, RandomGenerator rnd) {
        return RandomUnivariatePolynomials.randomPoly(factory, minDegree +
                (minDegree == maxDegree ? 0 : rnd.nextInt(maxDegree - minDegree)), rnd);
    }

    
    public Poly randomElement(int degree, RandomGenerator rnd) {
        return randomElement(degree, degree, rnd);
    }


    public const int MIN_DEGREE_OF_RANDOM_POLY = 0;

    public const int MAX_DEGREE_OF_RANDOM_POLY = 32;


    public Poly randomElement(RandomGenerator rnd) {
        return randomElement(MIN_DEGREE_OF_RANDOM_POLY, MAX_DEGREE_OF_RANDOM_POLY, rnd);
    }

    
    public Poly randomElementTree(int minDegree, int maxDegree, RandomGenerator rnd) {
        if (Factory is UnivariatePolynomial) {
            UnivariatePolynomial f = (UnivariatePolynomial) this.factory;
            Ring cfRing = f.ring;
            Func<RandomGenerator, ?> method = cfRing::randomElementTree;
            return (Poly) RandomUnivariatePolynomials.randomPoly(minDegree +
                            (minDegree == maxDegree ? 0 : rnd.nextInt(maxDegree - minDegree)),
                    cfRing, method, rnd);
        } else
            return randomElement(minDegree, maxDegree, rnd);
    }

    
    
    public Poly randomElementTree(RandomGenerator rnd) {
        return randomElementTree(MIN_DEGREE_OF_RANDOM_POLY, MAX_DEGREE_OF_RANDOM_POLY, rnd);
    }
}
