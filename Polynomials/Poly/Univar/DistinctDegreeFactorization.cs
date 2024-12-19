namespace Polynomials.Poly.Univar;
using UnivariatePolynomialZp64 = UnivariatePolynomial<long>;
using static UnivariatePolynomialArithmetic;
using static ModularComposition;

public static class DistinctDegreeFactorization
{

    public static PolynomialFactorDecomposition<UnivariatePolynomialZp64>
        DistinctDegreeFactorizationPrecomputedExponents(UnivariatePolynomialZp64 poly)
    {
        if (poly.IsConstant())
            return PolynomialFactorDecomposition<UnivariatePolynomialZp64>.FromUnit(poly);
        var factor = poly.Lc();
        var @base = poly.Clone().Monic();
        var polyModulus = @base.Clone();
        if (@base.degree <= 1)
            return PolynomialFactorDecomposition<UnivariatePolynomialZp64>.Of(@base.CreateConstant(factor), @base);
        if (@base.IsMonomial())
            return PolynomialFactorDecomposition<UnivariatePolynomialZp64>.Of(@base.CreateConstant(factor), @base);
        var invMod = UnivariateDivision.FastDivisionPreConditioning(polyModulus);
        var exponent = poly.CreateMonomial(1);
        var result = PolynomialFactorDecomposition<UnivariatePolynomialZp64>.FromUnit(poly.CreateOne());
        List<UnivariatePolynomialZp64> xPowers = XPowers(polyModulus, invMod);
        var i = 0;
        while (!@base.IsConstant())
        {
            ++i;
            exponent = PowModulusMod(exponent, polyModulus, invMod, xPowers);
            var tmpExponent = exponent.Clone();
            tmpExponent.EnsureCapacity(1);
            tmpExponent.data[1] = ((IntegersZp64)poly.ring).Subtract(tmpExponent.data[1], 1);
            tmpExponent.FixDegree();
            var gcd = UnivariateGCD.PolynomialGCD(tmpExponent, @base);
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


    private static readonly double SHOUP_BETA = 0.5;


    private static void DistinctDegreeFactorizationShoup<E>(UnivariatePolynomial<E> poly, BabyGiantSteps<E> steps,
        PolynomialFactorDecomposition<UnivariatePolynomial<E>> result)
    {
        //generate each I_j
        var current = poly.Clone();
        for (var j = 1; j <= steps.m; ++j)
        {
            var iBase = poly.CreateOne();
            for (var i = 0; i <= steps.l - 1; ++i)
            {
                var tmp = steps.GiantStep(j).Clone().Subtract(steps.babySteps[i]);
                iBase = PolyMultiplyMod(iBase, tmp, poly, steps.invMod, false);
            }

            var gcd = UnivariateGCD.PolynomialGCD(current, iBase);
            if (gcd.IsConstant())
                continue;
            current = UnivariateDivision.Quotient(current, gcd, false);
            for (var i = steps.l - 1; i >= 0; --i)
            {
                var tmp = UnivariateGCD.PolynomialGCD(gcd, steps.GiantStep(j).Clone().Subtract(steps.babySteps[i]));
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


    public static PolynomialFactorDecomposition<UnivariatePolynomial<E>> DistinctDegreeFactorizationShoup<E>(UnivariatePolynomial<E> poly)
    {
        Util.EnsureOverFiniteField(poly);
        var factor = poly.LcAsPoly();
        poly = poly.Clone().Monic();
        var result = PolynomialFactorDecomposition<UnivariatePolynomial<E>>.FromUnit(factor);
        DistinctDegreeFactorizationShoup(poly, new BabyGiantSteps<E>(poly), result);
        return result;
    }


    private sealed class BabyGiantSteps<E>
    {
        public readonly int l;
        public readonly int m;
        public readonly List<UnivariatePolynomial<E>> babySteps;
        readonly List<UnivariatePolynomial<E>> giantSteps;
        public readonly UnivariateDivision.InverseModMonomial<E> invMod;

        public BabyGiantSteps(UnivariatePolynomial<E> poly)
        {
            var n = poly.Degree();
            l = (int)Math.Ceiling(Math.Pow(1 * n, SHOUP_BETA));
            m = (int)Math.Ceiling(1.0 * n / 2 / l);
            invMod = UnivariateDivision.FastDivisionPreConditioning(poly);
            List<UnivariatePolynomial<E>> xPowers = XPowers(poly, invMod);

            //baby steps
            babySteps = new List<UnivariatePolynomial<E>>();
            babySteps.Add(poly.CreateMonomial(1)); // <- add x
            var xPower = xPowers[1]; // x^p mod poly
            babySteps.Add(xPower); // <- add x^p mod poly
            for (var i = 0; i <= l - 2; ++i)
                babySteps.Add(xPower = PowModulusMod(xPower, poly, invMod, xPowers));

            // <- xPower = x^(p^l) mod poly
            //giant steps
            giantSteps = new List<UnivariatePolynomial<E>>();
            giantSteps.Add(poly.CreateMonomial(1)); // <- add x
            giantSteps.Add(xPower);
            xPowerBig = xPower;
            tBrentKung = (int)Math.Sqrt(poly.Degree());
            hPowers = PolyPowers(xPowerBig, poly, invMod, tBrentKung);
            this.poly = poly;
        }


        readonly UnivariatePolynomial<E> poly;


        readonly List<UnivariatePolynomial<E>> hPowers;

        readonly int tBrentKung;

        UnivariatePolynomial<E> xPowerBig;

        public UnivariatePolynomial<E> GiantStep(int j)
        {
            if (giantSteps.Count > j)
                return giantSteps[j];
            while (j >= giantSteps.Count)
                giantSteps.Add(xPowerBig = CompositionBrentKung(xPowerBig, hPowers, poly, invMod, tBrentKung));
            return giantSteps[j];
        }
    }


    private static readonly int DEGREE_SWITCH_TO_SHOUP = 256;

    public static PolynomialFactorDecomposition<UnivariatePolynomial<E>> GetDistinctDegreeFactorization<E>(UnivariatePolynomial<E> poly)
    {
        Util.EnsureOverFiniteField(poly);
        if (poly is UnivariatePolynomialZp64 p)
        {
            if (poly.degree < DEGREE_SWITCH_TO_SHOUP)
                return DistinctDegreeFactorizationPrecomputedExponents(p) as PolynomialFactorDecomposition<UnivariatePolynomial<E>>;
            else
                return DistinctDegreeFactorizationShoup(p) as PolynomialFactorDecomposition<UnivariatePolynomial<E>>;
        }
        else
            return DistinctDegreeFactorizationShoup(poly);
    }


    static PolynomialFactorDecomposition<UnivariatePolynomialZp64> DistinctDegreeFactorizationComplete(
        UnivariatePolynomialZp64 poly)
    {
        var squareFree = UnivariateSquareFreeFactorization.SquareFreeFactorization(poly);
        var overallFactor = squareFree.Unit.Lc();
        var result = PolynomialFactorDecomposition<UnivariatePolynomialZp64>.FromUnit(poly.CreateOne());
        for (var i = squareFree.Count - 1; i >= 0; --i)
        {
            var dd = GetDistinctDegreeFactorization(squareFree[i]);
            var nFactors = dd.Count;
            for (var j = nFactors - 1; j >= 0; --j)
                result.AddFactor(dd[j], squareFree.Exponents[i]);
            overallFactor = ((IntegersZp64)poly.ring).Multiply(overallFactor, dd.Unit.Lc());
        }

        return result.SetUnit(poly.CreateConstant(overallFactor));
    }
}
