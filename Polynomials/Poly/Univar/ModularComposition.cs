namespace Polynomials.Poly.Univar;

using UnivariatePolynomialZp64 = UnivariatePolynomial<long>;

public static class ModularComposition
{
    public static List<UnivariatePolynomial<E>> XPowers<E>(UnivariatePolynomial<E> polyModulus,
        UnivariateDivision.InverseModMonomial<E> invMod)
    {
        return PolyPowers(
            UnivariatePolynomialArithmetic.CreateMonomialMod(polyModulus.CoefficientRingCardinality().Value,
                polyModulus,
                invMod), polyModulus, invMod, polyModulus.Degree());
    }


    public static List<UnivariatePolynomial<E>> PolyPowers<E>(UnivariatePolynomial<E> poly,
        UnivariatePolynomial<E> polyModulus, UnivariateDivision.InverseModMonomial<E> invMod,
        int nIterations)
    {
        List<UnivariatePolynomial<E>> exponents = new List<UnivariatePolynomial<E>>();
        PolyPowers(UnivariatePolynomialArithmetic.PolyMod(poly, polyModulus, invMod, true), polyModulus, invMod,
            nIterations, exponents);
        return exponents;
    }


    private static void PolyPowers<E>(UnivariatePolynomial<E> polyReduced, UnivariatePolynomial<E> polyModulus,
        UnivariateDivision.InverseModMonomial<E> invMod,
        int nIterations, List<UnivariatePolynomial<E>> exponents)
    {
        exponents.Add(polyReduced.CreateOne());

        // polyReduced must be reduced!
        var @base = polyReduced.Clone(); //polyMod(poly, polyModulus, invMod, true);
        exponents.Add(@base);
        var prev = @base;
        for (var i = 0; i < nIterations; i++)
            exponents.Add(prev =
                UnivariatePolynomialArithmetic.PolyMod(prev.Clone().Multiply(@base), polyModulus, invMod, false));
    }


    public static UnivariatePolynomial<E> PowModulusMod<E>(UnivariatePolynomial<E> poly,
        UnivariatePolynomial<E> polyModulus, UnivariateDivision.InverseModMonomial<E> invMod,
        List<UnivariatePolynomial<E>> xPowers)
    {
        poly = UnivariatePolynomialArithmetic.PolyMod(poly, polyModulus, invMod, true);
        return PowModulusMod0(poly, polyModulus, invMod, xPowers);
    }


    private static UnivariatePolynomial<E> PowModulusMod0<E>(UnivariatePolynomial<E> poly,
        UnivariatePolynomial<E> polyModulus, UnivariateDivision.InverseModMonomial<E> invMod,
        List<UnivariatePolynomial<E>> xPowers)
    {
        UnivariatePolynomial<E> res = poly.CreateZero();
        for (var i = poly.degree; i >= 0; --i)
        {
            if (poly.ring.IsZero(poly.data[i]))
                continue;
            res.AddMul(xPowers[i], poly.data[i]);
        }

        return UnivariatePolynomialArithmetic.PolyMod(res, polyModulus, invMod, false);
    }


    public static UnivariatePolynomial<E> CompositionBrentKung<E>(UnivariatePolynomial<E> poly,
        List<UnivariatePolynomial<E>> pointPowers, UnivariatePolynomial<E> polyModulus,
        UnivariateDivision.InverseModMonomial<E> invMod, int tBrentKung)
    {
        if (poly.IsConstant())
            return poly;
        List<UnivariatePolynomial<E>> gj = [];
        int degree = poly.Degree();
        for (var i = 0; i <= degree;)
        {
            var to = i + tBrentKung;
            if (to > (degree + 1))
                to = degree + 1;
            var g = poly.GetRange(i, to);
            gj.Add(PowModulusMod0(g, polyModulus, invMod, pointPowers));
            i = to;
        }

        var pt = pointPowers[tBrentKung];
        var res = poly.CreateZero();
        for (var i = gj.Count - 1; i >= 0; --i)
            res = UnivariatePolynomialArithmetic.PolyMod(res.Multiply(pt).Add(gj[i]), polyModulus, invMod, false);
        return res;
    }


    public static UnivariatePolynomial<E> CompositionBrentKung<E>(UnivariatePolynomial<E> poly,
        UnivariatePolynomial<E> point, UnivariatePolynomial<E> polyModulus,
        UnivariateDivision.InverseModMonomial<E> invMod)
    {
        if (poly.IsConstant())
            return poly;
        var t = SafeToInt(Math.Sqrt(poly.Degree()));
        var hPowers = PolyPowers(point, polyModulus, invMod, t);
        return CompositionBrentKung(poly, hPowers, polyModulus, invMod, t);
    }

    private static int SafeToInt(double dbl)
    {
        if (dbl > int.MaxValue || dbl < int.MinValue)
            throw new ArithmeticException("int overflow");
        return (int)dbl;
    }


    public static UnivariatePolynomialZp64 CompositionHorner(UnivariatePolynomialZp64 poly,
        UnivariatePolynomialZp64 point, UnivariatePolynomialZp64 polyModulus,
        UnivariateDivision.InverseModMonomial<long> invMod)
    {
        if (poly.IsConstant())
            return poly;
        var res = poly.CreateZero();
        for (var i = poly.degree; i >= 0; --i)
            res = UnivariatePolynomialArithmetic.PolyMod(res.Multiply(point).AddMonomial(poly.data[i], 0), polyModulus,
                invMod, false);
        return res;
    }


    public static UnivariatePolynomial<E> Composition<E>(UnivariatePolynomial<E> poly, UnivariatePolynomial<E> point,
        UnivariatePolynomial<E> polyModulus, UnivariateDivision.InverseModMonomial<E> invMod)
    {
        return CompositionBrentKung(poly, point, polyModulus, invMod);
    }


    public static UnivariatePolynomial<E> Composition<E>(UnivariatePolynomial<E> poly, UnivariatePolynomial<E> point,
        UnivariatePolynomial<E> polyModulus)
    {
        return CompositionBrentKung(poly, point, polyModulus, UnivariateDivision.FastDivisionPreConditioning(point));
    }
}