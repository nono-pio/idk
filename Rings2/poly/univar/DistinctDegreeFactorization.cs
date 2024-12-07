using Cc.Redberry.Rings.Poly;

namespace Cc.Redberry.Rings.Poly.Univar
{
    /// <summary>
    /// Distinct-degree factorization of univariate polynomials over finite fields.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class DistinctDegreeFactorization
    {
        private DistinctDegreeFactorization()
        {
        }

        /// <summary>
        /// Performs distinct-degree factorization for square-free polynomial {@code poly} using plain incremental exponents
        /// algorithm.
        /// 
        /// <p> In the case of not square-free input, the algorithm works, but the resulting d.d.f. may be incomplete.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>distinct-degree decomposition of {@code poly}</returns>
        public static PolynomialFactorDecomposition<UnivariatePolynomialZp64> DistinctDegreeFactorizationPlain(UnivariatePolynomialZp64 poly)
        {
            if (poly.IsConstant())
                return PolynomialFactorDecomposition<UnivariatePolynomialZp64>.Unit(poly);
            long factor = poly.Lc();
            UnivariatePolynomialZp64 @base = poly.Clone().Monic();
            UnivariatePolynomialZp64 polyModulus = @base.Clone();
            if (@base.degree <= 1)
                return PolynomialFactorDecomposition<UnivariatePolynomialZp64>.Of(@base.CreateConstant(factor), @base);
            if (@base.IsMonomial())
                return PolynomialFactorDecomposition<UnivariatePolynomialZp64>.Of(@base.CreateConstant(factor), @base);
            UnivariateDivision.InverseModMonomial<UnivariatePolynomialZp64> invMod = UnivariateDivision.FastDivisionPreConditioning(polyModulus);
            UnivariatePolynomialZp64 exponent = poly.CreateMonomial(1);
            PolynomialFactorDecomposition<UnivariatePolynomialZp64> result = PolynomialFactorDecomposition<UnivariatePolynomialZp64>.Unit(poly.CreateOne());
            int i = 0;
            while (!@base.IsConstant())
            {
                ++i;
                exponent = PolyPowMod(exponent, poly.ring.modulus, polyModulus, invMod, false);
                UnivariatePolynomialZp64 tmpExponent = exponent.Clone();
                tmpExponent.EnsureCapacity(1);
                tmpExponent.data[1] = @base.Subtract(tmpExponent.data[1], 1);
                tmpExponent.FixDegree();
                UnivariatePolynomialZp64 gcd = PolynomialGCD(tmpExponent, @base);
                if (!gcd.IsConstant())
                    result.AddFactor(gcd.Monic(), i);
                @base = UnivariateDivision.Quotient(@base, gcd, false); //can safely destroy reused base
                if (@base.degree < 2 * (i + 1))
                {

                    // <- early termination
                    if (!@base.IsConstant())
                        result.AddFactor(@base.Monic(), @base.degree);
                    break;
                }
            }

            return result.SetUnit(poly.CreateConstant(factor));
        }

        /// <summary>
        /// Performs distinct-degree factorization for square-free polynomial {@code poly} using plain incremental exponents
        /// algorithm with precomputed exponents.
        /// 
        /// <p> In the case of not square-free input, the algorithm works, but the resulting d.d.f. may be incomplete.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>distinct-degree decomposition of {@code poly}</returns>
        public static PolynomialFactorDecomposition<UnivariatePolynomialZp64> DistinctDegreeFactorizationPrecomputedExponents(UnivariatePolynomialZp64 poly)
        {
            if (poly.IsConstant())
                return PolynomialFactorDecomposition<UnivariatePolynomialZp64>.Unit(poly);
            long factor = poly.Lc();
            UnivariatePolynomialZp64 @base = poly.Clone().Monic();
            UnivariatePolynomialZp64 polyModulus = @base.Clone();
            if (@base.degree <= 1)
                return PolynomialFactorDecomposition<UnivariatePolynomialZp64>.Of(@base.CreateConstant(factor), @base);
            if (@base.IsMonomial())
                return PolynomialFactorDecomposition<UnivariatePolynomialZp64>.Of(@base.CreateConstant(factor), @base);
            UnivariateDivision.InverseModMonomial<UnivariatePolynomialZp64> invMod = UnivariateDivision.FastDivisionPreConditioning(polyModulus);
            UnivariatePolynomialZp64 exponent = poly.CreateMonomial(1);
            PolynomialFactorDecomposition<UnivariatePolynomialZp64> result = PolynomialFactorDecomposition<UnivariatePolynomialZp64>.Unit(poly.CreateOne());
            List<UnivariatePolynomialZp64> xPowers = XPowers(polyModulus, invMod);
            int i = 0;
            while (!@base.IsConstant())
            {
                ++i;
                exponent = PowModulusMod(exponent, polyModulus, invMod, xPowers);
                UnivariatePolynomialZp64 tmpExponent = exponent.Clone();
                tmpExponent.EnsureCapacity(1);
                tmpExponent.data[1] = poly.Subtract(tmpExponent.data[1], 1);
                tmpExponent.FixDegree();
                UnivariatePolynomialZp64 gcd = PolynomialGCD(tmpExponent, @base);
                if (!gcd.IsConstant())
                    result.AddFactor(gcd.Monic(), i);
                @base = UnivariateDivision.Quotient(@base, gcd, false); //can safely destroy reused base
                if (@base.degree < 2 * (i + 1))
                {

                    // <- early termination
                    if (!@base.IsConstant())
                        result.AddFactor(@base.Monic(), @base.degree);
                    break;
                }
            }

            return result.SetUnit(poly.CreateConstant(factor));
        }

        /// <summary>
        /// Shoup's parameter
        /// </summary>
        private static readonly double SHOUP_BETA = 0.5;
        /// <summary>
        /// Shoup's main gcd loop
        /// </summary>
        private static void DistinctDegreeFactorizationShoup<T>(T poly, BabyGiantSteps<T> steps, PolynomialFactorDecomposition<T> result)
        where T : IUnivariatePolynomial<T>{

            //generate each I_j
            T current = poly.Clone();
            for (int j = 1; j <= steps.m; ++j)
            {
                T iBase = poly.CreateOne();
                for (int i = 0; i <= steps.l - 1; ++i)
                {
                    T tmp = steps.GiantStep(j).Clone().Subtract(steps.babySteps[i]);
                    iBase = PolyMultiplyMod(iBase, tmp, poly, steps.invMod, false);
                }

                T gcd = UnivariateGCD.PolynomialGCD(current, iBase);
                if (gcd.IsConstant())
                    continue;
                current = UnivariateDivision.Quotient(current, gcd, false);
                for (int i = steps.l - 1; i >= 0; --i)
                {
                    T tmp = UnivariateGCD.PolynomialGCD(gcd, steps.GiantStep(j).Clone().Subtract(steps.babySteps[i]));
                    if (!tmp.IsConstant())
                        result.AddFactor(tmp.Clone().Monic(), steps.l * j - i);
                    gcd = UnivariateDivision.Quotient(gcd, tmp, false);
                }

                if (current.IsOne())
                    break;
            }

            if (!current.IsOne())
                result.AddFactor(current.Monic(), current.Degree());
        }

        /// <summary>
        /// Performs distinct-degree factorization for square-free polynomial {@code poly} using Victor Shoup's baby step /
        /// giant step algorithm.
        /// 
        /// <p> In the case of not square-free input, the algorithm works, but the resulting d.d.f. may be incomplete.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>distinct-degree decomposition of {@code poly}</returns>
        public static PolynomialFactorDecomposition<Poly> DistinctDegreeFactorizationShoup<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
        {
            Util.EnsureOverFiniteField(poly);
            Poly factor = poly.LcAsPoly();
            poly = poly.Clone().Monic();
            PolynomialFactorDecomposition<Poly> result = PolynomialFactorDecomposition<Poly>.Unit(factor);
            DistinctDegreeFactorizationShoup(poly, new BabyGiantSteps<Poly>(poly), result);
            return result;
        }

        /// <summary>
        /// baby/giant steps for Shoup's d.d.f. algorithm
        /// </summary>
        private sealed class BabyGiantSteps<Poly> where Poly : IUnivariatePolynomial<Poly>
        {
            public readonly int l;
            public readonly int m;
            public readonly List<Poly> babySteps;
            readonly List<Poly> giantSteps;
            public readonly UnivariateDivision.InverseModMonomial<Poly> invMod;
            public BabyGiantSteps(Poly poly)
            {
                int n = poly.Degree();
                l = (int)Math.Ceiling(Math.Pow(1 * n, SHOUP_BETA));
                m = (int)Math.Ceiling(1 * n / 2 / l);
                invMod = UnivariateDivision.FastDivisionPreConditioning(poly);
                List<Poly> xPowers = XPowers(poly, invMod);

                //baby steps
                babySteps = new List<Poly>();
                babySteps.Add(poly.CreateMonomial(1)); // <- add x
                Poly xPower = xPowers[1]; // x^p mod poly
                babySteps.Add(xPower); // <- add x^p mod poly
                for (int i = 0; i <= l - 2; ++i)
                    babySteps.Add(xPower = PowModulusMod(xPower, poly, invMod, xPowers));

                // <- xPower = x^(p^l) mod poly
                //giant steps
                giantSteps = new List<Poly>();
                giantSteps.Add(poly.CreateMonomial(1)); // <- add x
                giantSteps.Add(xPower);
                xPowerBig = xPower;
                tBrentKung = (int)Math.Sqrt(poly.Degree());
                hPowers = PolyPowers(xPowerBig, poly, invMod, tBrentKung);
                this.poly = poly;
            }

            //baby steps
            // <- add x
            // x^p mod poly
            // <- add x^p mod poly
            // <- xPower = x^(p^l) mod poly
            //giant steps
            // <- add x
            readonly Poly poly;
            //baby steps
            // <- add x
            // x^p mod poly
            // <- add x^p mod poly
            // <- xPower = x^(p^l) mod poly
            //giant steps
            // <- add x
            readonly List<Poly> hPowers;
            //baby steps
            // <- add x
            // x^p mod poly
            // <- add x^p mod poly
            // <- xPower = x^(p^l) mod poly
            //giant steps
            // <- add x
            readonly int tBrentKung;
            //baby steps
            // <- add x
            // x^p mod poly
            // <- add x^p mod poly
            // <- xPower = x^(p^l) mod poly
            //giant steps
            // <- add x
            Poly xPowerBig;
            //baby steps
            // <- add x
            // x^p mod poly
            // <- add x^p mod poly
            // <- xPower = x^(p^l) mod poly
            //giant steps
            // <- add x
            public Poly GiantStep(int j)
            {
                if (giantSteps.Count > j)
                    return giantSteps[j];
                while (j >= giantSteps.Count)
                    giantSteps.Add(xPowerBig = CompositionBrentKung(xPowerBig, hPowers, poly, invMod, tBrentKung));
                return giantSteps[j];
            }
        }

        /// <summary>
        /// when to switch to Shoup's algorithm
        /// </summary>
        private static readonly int DEGREE_SWITCH_TO_SHOUP = 256;
        /// <summary>
        /// Performs distinct-degree factorization for square-free polynomial {@code poly}.
        /// 
        /// <p> In the case of not square-free input, the algorithm works, but the resulting d.d.f. may be incomplete.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>distinct-degree decomposition of {@code poly}</returns>
        public static PolynomialFactorDecomposition<UnivariatePolynomialZp64> DistinctDegreeFactorization(UnivariatePolynomialZp64 poly)
        {
            if (poly.degree < DEGREE_SWITCH_TO_SHOUP)
                return DistinctDegreeFactorizationPrecomputedExponents(poly);
            else
                return DistinctDegreeFactorizationShoup(poly);
        }

        /// <summary>
        /// Performs distinct-degree factorization for square-free polynomial {@code poly}.
        /// 
        /// <p> In the case of not square-free input, the algorithm works, but the resulting d.d.f. may be incomplete.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>distinct-degree decomposition of {@code poly}</returns>
        public static PolynomialFactorDecomposition<Poly> DistinctDegreeFactorization<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
        {
            Util.EnsureOverFiniteField(poly);
            if (poly is UnivariatePolynomialZp64)
                return (PolynomialFactorDecomposition<Poly>) DistinctDegreeFactorization((UnivariatePolynomialZp64)poly);
            else
                return DistinctDegreeFactorizationShoup(poly);
        }

        /// <summary>
        /// Performs square-free factorization followed by distinct-degree factorization modulo {@code modulus}.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>square-free and distinct-degree decomposition of {@code poly} modulo {@code modulus}</returns>
        static PolynomialFactorDecomposition<UnivariatePolynomialZp64> DistinctDegreeFactorizationComplete(UnivariatePolynomialZp64 poly)
        {
            PolynomialFactorDecomposition<UnivariatePolynomialZp64> squareFree = UnivariateSquareFreeFactorization.SquareFreeFactorization(poly);
            long overallFactor = squareFree.unit.Lc();
            PolynomialFactorDecomposition<UnivariatePolynomialZp64> result = PolynomialFactorDecomposition<UnivariatePolynomialZp64>.Unit(poly.CreateOne());
            for (int i = squareFree.size() - 1; i >= 0; --i)
            {
                PolynomialFactorDecomposition<UnivariatePolynomialZp64> dd = DistinctDegreeFactorization(squareFree[i]);
                int nFactors = dd.Count;
                for (int j = nFactors - 1; j >= 0; --j)
                    result.AddFactor(dd[j], squareFree.GetExponent(i));
                overallFactor = poly.Multiply(overallFactor, dd.unit.Lc());
            }

            return result.SetUnit(poly.CreateConstant(overallFactor));
        }
    }
}