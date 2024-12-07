using System.Numerics;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Poly;


namespace Cc.Redberry.Rings.Poly.Univar
{
    /// <summary>
    /// Equal-degree factorization of univariate polynomials over finite fields.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class EqualDegreeFactorization
    {
        private EqualDegreeFactorization()
        {
        }

        static T RandomMonicPoly<T>(T factory) where T : IUnivariatePolynomial<T>
        {
            Random rnd = PrivateRandom.GetRandom();
            int degree = Math.Max(1, rnd.Next(2 * factory.Degree() + 1));
            if (factory is UnivariatePolynomialZp64)
            {
                UnivariatePolynomialZp64 fm = (UnivariatePolynomialZp64)factory;
                return (T)RandomUnivariatePolynomials.RandomMonicPoly(degree, fm.ring.modulus, rnd);
            }
            else
            {
                UnivariatePolynomial fm = (UnivariatePolynomial)factory;
                return (T)RandomUnivariatePolynomials.RandomMonicPoly(degree, fm.ring, rnd);
            }
        }

        /// <summary>
        /// Plain Cantor-Zassenhaus algorithm implementation
        /// </summary>
        /// <param name="input">the polynomial</param>
        /// <param name="d">distinct degree</param>
        /// <returns>irreducible factor of {@code poly}</returns>
        public static PolynomialFactorDecomposition<Poly> CantorZassenhaus<Poly>(Poly input, int d) where Poly : IUnivariatePolynomial<Poly>
        {
            Util.EnsureOverFiniteField(input);
            PolynomialFactorDecomposition<Poly> result = PolynomialFactorDecomposition<Poly>.Unit(input.LcAsPoly());
            if (!input.CoefficientRingCardinality().TestBit(0))

                //even characteristic => GF2p
                CantorZassenhaus(input, d, result, input.CoefficientRingPerfectPowerExponent().IntValueExact());
            else
                CantorZassenhaus(input, d, result, -1);
            return result;
        }

        /// <summary>
        /// Plain Cantor-Zassenhaus algorithm implementation
        /// </summary>
        /// <param name="input">the polynomial</param>
        /// <param name="d">distinct degree</param>
        /// <returns>irreducible factor of {@code poly}</returns>
        private static void CantorZassenhaus<T>(T input, int d, PolynomialFactorDecomposition<T> result, int pPower) where T : IUnivariatePolynomial<T>
        {
            int nFactors = input.Degree() / d;
            if (input.Degree() == 1 || nFactors == 1)
            {
                result.AddFactor(input, 1);
                return;
            }

            T poly = input.Clone();
            while (true)
            {
                poly = poly.Monic();
                if (poly.Degree() == d)
                {
                    result.AddFactor(poly, 1);
                    break;
                }

                T factor;
                do
                {
                    if (pPower == -1)
                        factor = CantorZassenhaus0(poly, d);
                    else
                        factor = CantorZassenhausGF2p(poly, d, pPower);
                }
                while (factor == null);
                if (factor.Degree() != d)
                    CantorZassenhaus(factor, d, result, pPower);
                else
                    result.AddFactor(factor.Monic(), 1);
                poly = UnivariateDivision.Quotient(poly, factor, false);
            }
        }

        /// <summary>
        /// Plain Cantor-Zassenhaus algorithm for odd characteristic
        /// </summary>
        /// <param name="poly">the monic polynomial</param>
        /// <param name="d">distinct degree</param>
        /// <returns>irreducible factor of {@code poly}</returns>
        private static Poly CantorZassenhaus0<Poly>(Poly poly, int d) where Poly : IUnivariatePolynomial<Poly>
        {
            Poly a = RandomMonicPoly(poly);
            if (a.IsConstant() || a.Equals(poly))
                return null;
            Poly gcd1 = UnivariateGCD.PolynomialGCD(a, poly);
            if (!gcd1.IsConstant() && !gcd1.Equals(poly))
                return gcd1;

            // (modulus^d - 1) / 2
            BigInteger exponent = poly.CoefficientRingCardinality().Pow(d).Decrement().ShiftRight(1);
            Poly b = UnivariatePolynomialArithmetic.PolyPowMod(a, exponent, poly, UnivariateDivision.FastDivisionPreConditioning(poly), true);
            Poly gcd2 = UnivariateGCD.PolynomialGCD(b.Decrement(), poly);
            if (!gcd2.IsConstant() && !gcd2.Equals(poly))
                return gcd2;
            return null;
        }

        /// <summary>
        /// Plain Cantor-Zassenhaus algorithm in GF2p.
        /// </summary>
        /// <param name="poly">the monic polynomial</param>
        /// <param name="d">distinct degree</param>
        /// <returns>irreducible factor of {@code poly}</returns>
        private static Poly CantorZassenhausGF2p<Poly>(Poly poly, int d, int pPower) where Poly : IUnivariatePolynomial<Poly>
        {
            Poly a = RandomMonicPoly(poly);
            if (a.IsConstant() || a.Equals(poly))
                return null;
            Poly gcd1 = UnivariateGCD.PolynomialGCD(a, poly);
            if (!gcd1.IsConstant() && !gcd1.Equals(poly))
                return gcd1;
            UnivariateDivision.InverseModMonomial<Poly> invMod = UnivariateDivision.FastDivisionPreConditioning(poly);
            Poly b = TracePolyGF2(a, MachineArithmetic.SafeToInt(1 * pPower * d), poly, invMod);
            Poly gcd2 = UnivariateGCD.PolynomialGCD(b, poly);
            if (!gcd2.IsConstant() && !gcd2.Equals(poly))
                return gcd2;
            return null;
        }

        static Poly TracePolyGF2<Poly >(Poly a, int m, Poly modulus, UnivariateDivision.InverseModMonomial<Poly> invMod) where Poly : IUnivariatePolynomial<Poly>
        {
            Poly tmp = a.Clone();
            Poly result = a.Clone();
            for (int i = 0; i < (m - 1); i++)
            {
                tmp = UnivariatePolynomialArithmetic.PolyMultiplyMod(tmp, tmp, modulus, invMod, false);
                result.Add(tmp);
            }

            return result;
        }
    }
}