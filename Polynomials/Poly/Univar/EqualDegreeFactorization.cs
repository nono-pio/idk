using System.Numerics;
using Polynomials.Utils;
using UnivariatePolynomialZp64 = Polynomials.Poly.Univar.UnivariatePolynomial<long>;

namespace Polynomials.Poly.Univar;

public static class EqualDegreeFactorization
{
    static UnivariatePolynomialZp64 RandomMonicPoly(UnivariatePolynomialZp64 factory)
    {
        var rnd = PrivateRandom.GetRandom();
        var degree = Math.Max(1, rnd.Next(2 * factory.Degree() + 1));
        return RandomUnivariatePolynomials.RandomMonicPoly(degree, ((IntegersZp64)factory.ring).modulus, rnd);
    }


    static UnivariatePolynomial<E> RandomMonicPoly<E>(UnivariatePolynomial<E> factory)
    {
        var rnd = PrivateRandom.GetRandom();
        var degree = Math.Max(1, rnd.Next(2 * factory.Degree() + 1));
        return RandomUnivariatePolynomials.RandomMonicPoly(degree, factory.ring, rnd);
    }


    public static PolynomialFactorDecomposition<UnivariatePolynomial<E>> CantorZassenhaus<E>(UnivariatePolynomial<E> input, int d)
    {
        Util.EnsureOverFiniteField(input);
        var result = PolynomialFactorDecomposition<UnivariatePolynomial<E>>.FromUnit(input.LcAsPoly());
        if (!input.CoefficientRingCardinality().Value.IsEven)

            //even characteristic => GF2p
            CantorZassenhaus(input, d, result, (int) input.CoefficientRingPerfectPowerExponent());
        else
            CantorZassenhaus(input, d, result, -1);
        return result;
    }


    private static void CantorZassenhaus<E>(UnivariatePolynomial<E> input, int d, PolynomialFactorDecomposition<UnivariatePolynomial<E>> result, int pPower)
    {
        var nFactors = input.Degree() / d;
        if (input.Degree() == 1 || nFactors == 1)
        {
            result.AddFactor(input, 1);
            return;
        }

        var poly = input.Clone();
        while (true)
        {
            poly = poly.Monic();
            if (poly.Degree() == d)
            {
                result.AddFactor(poly, 1);
                break;
            }

            UnivariatePolynomial<E> factor;
            do
            {
                if (pPower == -1)
                    factor = CantorZassenhaus0(poly, d);
                else
                    factor = CantorZassenhausGF2p(poly, d, pPower);
            } while (factor == null);

            if (factor.Degree() != d)
                CantorZassenhaus(factor, d, result, pPower);
            else
                result.AddFactor(factor.Monic(), 1);
            poly = UnivariateDivision.Quotient(poly, factor, false);
        }
    }


    private static UnivariatePolynomial<E> CantorZassenhaus0<E>(UnivariatePolynomial<E> poly, int d)
    {
        var a = RandomMonicPoly(poly);
        if (a.IsConstant() || a.Equals(poly))
            return null;
        var gcd1 = UnivariateGCD.PolynomialGCD(a, poly);
        if (!gcd1.IsConstant() && !gcd1.Equals(poly))
            return gcd1;

        // (modulus^d - 1) / 2
        var exponent = (BigInteger.Pow(poly.CoefficientRingCardinality().Value, d) - 1) >> 1;
        var b = UnivariatePolynomialArithmetic.PolyPowMod(a, exponent, poly,
            UnivariateDivision.FastDivisionPreConditioning(poly), true);
        var gcd2 = UnivariateGCD.PolynomialGCD(b.Decrement(), poly);
        if (!gcd2.IsConstant() && !gcd2.Equals(poly))
            return gcd2;
        return null;
    }


    private static UnivariatePolynomial<E> CantorZassenhausGF2p<E>(UnivariatePolynomial<E> poly, int d, int pPower)
    {
        var a = RandomMonicPoly(poly);
        if (a.IsConstant() || a.Equals(poly))
            return null;
        var gcd1 = UnivariateGCD.PolynomialGCD(a, poly);
        if (!gcd1.IsConstant() && !gcd1.Equals(poly))
            return gcd1;
        UnivariateDivision.InverseModMonomial<E> invMod = UnivariateDivision.FastDivisionPreConditioning(poly);
        var b = TracePolyGF2(a, MachineArithmetic.SafeToInt(1 * pPower * d), poly, invMod);
        var gcd2 = UnivariateGCD.PolynomialGCD(b, poly);
        if (!gcd2.IsConstant() && !gcd2.Equals(poly))
            return gcd2;
        return null;
    }

    static UnivariatePolynomial<E> TracePolyGF2<E>(UnivariatePolynomial<E> a, int m, UnivariatePolynomial<E> modulus, UnivariateDivision.InverseModMonomial<E> invMod)
    {
        var tmp = a.Clone();
        var result = a.Clone();
        for (var i = 0; i < (m - 1); i++)
        {
            tmp = UnivariatePolynomialArithmetic.PolyMultiplyMod(tmp, tmp, modulus, invMod, false);
            result.Add(tmp);
        }

        return result;
    }
}