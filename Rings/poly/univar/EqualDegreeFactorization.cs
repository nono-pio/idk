using System.Diagnostics;
using System.Numerics;

namespace Rings.poly.univar;


/**
 * Equal-degree factorization of univariate polynomials over finite fields.
 *
 * @since 1.0
 */
public static class EqualDegreeFactorization {

    static T randomMonicPoly<T>(T factory) where T : IUnivariatePolynomial<T> {
        RandomGenerator rnd = PrivateRandom.getRandom();
        int degree = Math.Max(1, rnd.nextInt(2 * factory.degree() + 1));
        if (factory is UnivariatePolynomialZp64) {
            UnivariatePolynomialZp64 fm = (UnivariatePolynomialZp64) factory;
            return (T) RandomUnivariatePolynomials.randomMonicPoly(degree, fm.ring.Modulus, rnd);
        } else {
            UnivariatePolynomial fm = (UnivariatePolynomial) factory;
            return (T) RandomUnivariatePolynomials.randomMonicPoly(degree, fm.ring, rnd);
        }
    }

    /**
     * Plain Cantor-Zassenhaus algorithm implementation
     *
     * @param input the polynomial
     * @param d     distinct degree
     * @return irreducible factor of {@code poly}
     */
    public static PolynomialFactorDecomposition<Poly> CantorZassenhaus<Poly>(Poly input, int d) where Poly : IUnivariatePolynomial<Poly> {
        Util.ensureOverFiniteField(input);
        PolynomialFactorDecomposition<Poly> result = PolynomialFactorDecomposition.unit(input.lcAsPoly());
        if (!input.coefficientRingCardinality().testBit(0))
            //even characteristic => GF2p
            CantorZassenhaus(input, d, result, (int)input.coefficientRingPerfectPowerExponent());
        else
            CantorZassenhaus(input, d, result, -1);
        return result;
    }

    /**
     * Plain Cantor-Zassenhaus algorithm implementation
     *
     * @param input the polynomial
     * @param d     distinct degree
     * @return irreducible factor of {@code poly}
     */
    private static void CantorZassenhaus<T>(T input, int d, PolynomialFactorDecomposition<T> result, int pPower) where T : IUnivariatePolynomial<T>{
        Debug.Assert(input.degree() % d == 0);
        int nFactors = input.degree() / d;
        if (input.degree() == 1 || nFactors == 1) {
            result.addFactor(input, 1);
            return;
        }

        Debug.Assert(input.degree() != d);

        T poly = input.clone();
        while (true) {

            poly = poly.monic();
            if (poly.degree() == d) {
                result.addFactor(poly, 1);
                break;
            }

            T factor;
            do {
                if (pPower == -1)
                    factor = CantorZassenhaus0(poly, d);
                else
                    factor = CantorZassenhausGF2p(poly, d, pPower);
            } while (factor == null);

            if (factor.degree() != d)
                CantorZassenhaus(factor, d, result, pPower);
            else
                result.addFactor(factor.monic(), 1);
            poly = UnivariateDivision.quotient(poly, factor, false);
        }
    }

    /**
     * Plain Cantor-Zassenhaus algorithm for odd characteristic
     *
     * @param poly the monic polynomial
     * @param d    distinct degree
     * @return irreducible factor of {@code poly}
     */
    private static Poly CantorZassenhaus0<Poly>(Poly poly, int d) where Poly : IUnivariatePolynomial<Poly> {
        Debug.Assert(poly.isMonic());

        Poly a = randomMonicPoly(poly);
        if (a.isConstant() || a.Equals(poly))
            return null;

        Poly gcd1 = UnivariateGCD.PolynomialGCD(a, poly);
        if (!gcd1.isConstant() && !gcd1.Equals(poly))
            return gcd1;

        // (modulus^d - 1) / 2
        BigInteger exponent = (BigInteger.Pow(poly.coefficientRingCardinality(), 2) - 1) >> 1;
        Poly b = UnivariatePolynomialArithmetic.polyPowMod(a, exponent, poly, UnivariateDivision.fastDivisionPreConditioning(poly), true);

        Poly gcd2 = UnivariateGCD.PolynomialGCD(b.decrement(), poly);
        if (!gcd2.isConstant() && !gcd2.Equals(poly))
            return gcd2;

        return null;
    }

    /**
     * Plain Cantor-Zassenhaus algorithm in GF2p.
     *
     * @param poly the monic polynomial
     * @param d    distinct degree
     * @return irreducible factor of {@code poly}
     */
    private static Poly CantorZassenhausGF2p<Poly>(Poly poly, int d, int pPower)where Poly : IUnivariatePolynomial<Poly> {
        Debug.Assert( poly.isMonic());

        Poly a = randomMonicPoly(poly);
        if (a.isConstant() || a.Equals(poly))
            return null;

        Poly gcd1 = UnivariateGCD.PolynomialGCD(a, poly);
        if (!gcd1.isConstant() && !gcd1.Equals(poly))
            return gcd1;

        UnivariateDivision.InverseModMonomial<Poly> invMod = UnivariateDivision.fastDivisionPreConditioning(poly);
        Poly b = tracePolyGF2(a, MachineArithmetic.safeToInt(1L * pPower * d), poly, invMod);

        Poly gcd2 = UnivariateGCD.PolynomialGCD(b, poly);
        if (!gcd2.isConstant() && !gcd2.Equals(poly))
            return gcd2;

        return null;
    }

    static Poly tracePolyGF2<Poly>(Poly a, int m, Poly modulus, UnivariateDivision.InverseModMonomial<Poly> invMod) where Poly : IUnivariatePolynomial<Poly> {
        Poly tmp = a.clone();
        Poly result = a.clone();
        for (int i = 0; i < (m - 1); i++) {
            tmp = UnivariatePolynomialArithmetic.polyMultiplyMod(tmp, tmp, modulus, invMod, false);
            result.add(tmp);
        }

        return result;
    }
}
