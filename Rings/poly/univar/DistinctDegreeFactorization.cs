namespace Rings.poly.univar;

public static class DistinctDegreeFactorization
{
    public static PolynomialFactorDecomposition<UnivariatePolynomialZp64> DistinctDegreeFactorizationPlain(
        UnivariatePolynomialZp64 poly)
    {
        if (poly.isConstant())
            return PolynomialFactorDecomposition<UnivariatePolynomialZp64>.unit(poly);

        long factor = poly.lc();
        UnivariatePolynomialZp64 @base = poly.clone().monic();
        UnivariatePolynomialZp64 polyModulus = @base.clone();

        if (@base.Degree <= 1)
            return PolynomialFactorDecomposition<UnivariatePolynomialZp64>.of(@base.createConstant(factor), @base);

        if (@base.isMonomial())
            return PolynomialFactorDecomposition<UnivariatePolynomialZp64>.of(@base.createConstant(factor), @base);

        var invMod = UnivariateDivision.fastDivisionPreConditioning(polyModulus);
        var exponent = poly.createMonomial(1);
        var result = PolynomialFactorDecomposition<UnivariatePolynomialZp64>.unit(poly.createOne());
        int i = 0;
        while (!@base.isConstant())
        {
            ++i;
            exponent = polyPowMod(exponent, poly.ring.Modulus, polyModulus, invMod, false);
            UnivariatePolynomialZp64 tmpExponent = exponent.clone();
            tmpExponent.ensureCapacity(1);
            tmpExponent.data[1] = @base.subtract(tmpExponent.data[1], 1);
            tmpExponent.fixDegree();
            UnivariatePolynomialZp64 gcd = PolynomialGCD(tmpExponent, @base);
            if (!gcd.isConstant())
                result.addFactor(gcd.monic(), i);


            @base = UnivariateDivision.quotient(@base, gcd, false); //can safely destroy reused @base
            if (@base.Degree < 2 * (i + 1))
            {
                // <- early termination
                if (!@base.isConstant())
                    result.addFactor(@base.monic(), @base.Degree);
                break;
            }
        }

        return result.setUnit(poly.createConstant(factor));
    }


    public static PolynomialFactorDecomposition<UnivariatePolynomialZp64>
        DistinctDegreeFactorizationPrecomputedExponents(UnivariatePolynomialZp64 poly)
    {
        if (poly.isConstant())
            return PolynomialFactorDecomposition<UnivariatePolynomialZp64>.unit(poly);

        long factor = poly.lc();
        UnivariatePolynomialZp64 @base = poly.clone().monic();
        UnivariatePolynomialZp64 polyModulus = @base.clone();

        if (@base.Degree <= 1)
            return PolynomialFactorDecomposition<UnivariatePolynomialZp64>.of(@base.createConstant(factor), @base);

        if (@base.isMonomial())
            return PolynomialFactorDecomposition<UnivariatePolynomialZp64>.of(@base.createConstant(factor), @base);

        UnivariateDivision.InverseModMonomial<UnivariatePolynomialZp64> invMod =
            UnivariateDivision.fastDivisionPreConditioning(polyModulus);
        UnivariatePolynomialZp64 exponent = poly.createMonomial(1);
        PolynomialFactorDecomposition<UnivariatePolynomialZp64> result =
            PolynomialFactorDecomposition<UnivariatePolynomialZp64>.unit(poly.createOne());

        List<UnivariatePolynomialZp64> xPowers = xPowers(polyModulus, invMod);
        int i = 0;
        while (!@base.isConstant())
        {
            ++i;
            exponent = powModulusMod(exponent, polyModulus, invMod, xPowers);
            UnivariatePolynomialZp64 tmpExponent = exponent.clone();
            tmpExponent.ensureCapacity(1);
            tmpExponent.data[1] = poly.subtract(tmpExponent.data[1], 1);
            tmpExponent.fixDegree();
            UnivariatePolynomialZp64 gcd = PolynomialGCD(tmpExponent, @base);
            if (!gcd.isConstant())
                result.addFactor(gcd.monic(), i);

            @base = UnivariateDivision.quotient(@base, gcd, false); //can safely destroy reused @base
            if (@base.Degree < 2 * (i + 1))
            {
                // <- early termination
                if (!@base.isConstant())
                    result.addFactor(@base.monic(), @base.Degree);
                break;
            }
        }

        return result.setUnit(poly.createConstant(factor));
    }


    private static readonly double SHOUP_BETA = 0.5;


    private static void DistinctDegreeFactorizationShoup<T>(T poly,
        BabyGiantSteps<T> steps,
        PolynomialFactorDecomposition<T> result) where T : IUnivariatePolynomial<T>
    {
        //generate each I_j
        T current = poly.clone();
        for (int j = 1; j <= steps.m; ++j)
        {
            T iBase = poly.createOne();
            for (int i = 0; i <= steps.l - 1; ++i)
            {
                T tmp = steps.giantStep(j).clone().subtract(steps.babySteps.get(i));
                iBase = polyMultiplyMod(iBase, tmp, poly, steps.invMod, false);
            }

            T gcd = UnivariateGCD.PolynomialGCD(current, iBase);
            if (gcd.isConstant())
                continue;
            current = UnivariateDivision.quotient(current, gcd, false);
            for (int i = steps.l - 1; i >= 0; --i)
            {
                T tmp = UnivariateGCD.PolynomialGCD(gcd, steps.giantStep(j).clone().subtract(steps.babySteps.get(i)));
                if (!tmp.isConstant())
                    result.addFactor(tmp.clone().monic(), steps.l * j - i);

                gcd = UnivariateDivision.quotient(gcd, tmp, false);
            }

            if (current.isOne())
                break;
        }

        if (!current.isOne())
            result.addFactor(current.monic(), current.degree());
    }


    /**
     * Performs distinct-Degree factorization for square-free polynomial {@code poly} using Victor Shoup's baby step /
     * giant step algorithm.
     *
     * <p> In the case of not square-free input, the algorithm works, but the resulting d.d.f. may be incomplete.
     *
     * @param poly the polynomial
     * @return distinct-Degree decomposition of {@code poly}
     */
    public static PolynomialFactorDecomposition<Poly>
        DistinctDegreeFactorizationShoup<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
    {
        Util.ensureOverFiniteField(poly);
        Poly factor = poly.lcAsPoly();
        poly = poly.clone().monic();
        PolynomialFactorDecomposition<Poly> result = PolynomialFactorDecomposition<Poly>.unit(factor);
        DistinctDegreeFactorizationShoup(poly, new BabyGiantSteps<Poly>(poly), result);
        return result;
    }


    /** baby/giant steps for Shoup's d.d.f. algorithm */
    private sealed class BabyGiantSteps<Poly> where Poly : IUnivariatePolynomial<Poly>
    {
        public readonly int l, m;
        public readonly List<Poly> babySteps;
        public readonly List<Poly> giantSteps;
        public readonly UnivariateDivision.InverseModMonomial<Poly> invMod;

        public BabyGiantSteps(Poly poly)
        {
            int n = poly.degree();
            l = (int)Math.Ceiling(Math.Pow(1.0 * n, SHOUP_BETA));
            m = (int)Math.Ceiling(1.0 * n / 2 / l);

            invMod = UnivariateDivision.fastDivisionPreConditioning(poly);
            List<Poly> xPowers = xPowers(poly, invMod);

            //baby steps
            babySteps = new List<Poly>();
            babySteps.Add(poly.createMonomial(1)); // <- Add x
            Poly xPower = xPowers[1]; // x^p mod poly
            babySteps.Add(xPower); // <- Add x^p mod poly
            for (int i = 0; i <= l - 2; ++i)
                babySteps.Add(xPower = powModulusMod(xPower, poly, invMod, xPowers));

            // <- xPower = x^(p^l) mod poly

            //giant steps
            giantSteps = [];
            giantSteps.Add(poly.createMonomial(1)); // <- Add x
            giantSteps.Add(xPower);

            xPowerBig = xPower;
            tBrentKung = (int)Math.Sqrt(poly.degree());
            hPowers = polyPowers(xPowerBig, poly, invMod, tBrentKung);
            this.poly = poly;
        }

        readonly Poly poly;
        readonly List<Poly> hPowers;
        readonly int tBrentKung;
        Poly xPowerBig;

        Poly giantStep(int j)
        {
            if (giantSteps.Count > j)
                return giantSteps[j];

            while (j >= giantSteps.Count)
                giantSteps.Add(xPowerBig = compositionBrentKung(xPowerBig, hPowers, poly, invMod, tBrentKung));

            return giantSteps[j];
        }
    }

    private static readonly int DEGREE_SWITCH_TO_SHOUP = 256;

    public static PolynomialFactorDecomposition<UnivariatePolynomialZp64> DistinctDegreeFactorization(
        UnivariatePolynomialZp64 poly)
    {
        if (poly.Degree < DEGREE_SWITCH_TO_SHOUP)
            return DistinctDegreeFactorizationPrecomputedExponents(poly);
        else
            return DistinctDegreeFactorizationShoup(poly);
    }


    public static PolynomialFactorDecomposition<Poly>
        DistinctDegreeFactorization<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
    {
        Util.ensureOverFiniteField(poly);
        if (poly is UnivariatePolynomialZp64)
        return (PolynomialFactorDecomposition<Poly>)DistinctDegreeFactorization((UnivariatePolynomialZp64)poly);
        else
        return DistinctDegreeFactorizationShoup(poly);
    }


    static PolynomialFactorDecomposition<UnivariatePolynomialZp64> DistinctDegreeFactorizationComplete(
        UnivariatePolynomialZp64 poly)
    {
        PolynomialFactorDecomposition<UnivariatePolynomialZp64> squareFree =
            UnivariateSquareFreeFactorization.SquareFreeFactorization(poly);
        long overallFactor = squareFree.unit.lc();

        var result = PolynomialFactorDecomposition<UnivariatePolynomialZp64>.unit(poly.createOne());
        for (int i = squareFree.size() - 1; i >= 0; --i)
        {
            PolynomialFactorDecomposition<UnivariatePolynomialZp64> dd = DistinctDegreeFactorization(squareFree.get(i));
            int nFactors = dd.size();
            for (int j = nFactors - 1; j >= 0; --j)
                result.addFactor(dd.get(j), squareFree.getExponent(i));
            overallFactor = poly.multiply(overallFactor, dd.unit.lc());
        }

        return result.setUnit(poly.createConstant(overallFactor));
    }
}