namespace Rings.poly.univar;

public static class ModularComposition
{
    public static List<T> xPowers<T>(T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod)
        where T : IUnivariatePolynomial<T>
    {
        return polyPowers(
            UnivariatePolynomialArithmetic.createMonomialMod(polyModulus.coefficientRingCardinality(), polyModulus,
                invMod), polyModulus, invMod, polyModulus.degree());
    }


    public static List<T> polyPowers<T>(T poly, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod,
        int nIterations) where T : IUnivariatePolynomial<T>
    {
        List<T> exponents = [];
        polyPowers(UnivariatePolynomialArithmetic.polyMod(poly, polyModulus, invMod, true), polyModulus, invMod,
            nIterations, exponents);
        return exponents;
    }


    private static void polyPowers<T>(T polyReduced, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod,
        int nIterations, List<T> exponents) where T : IUnivariatePolynomial<T>
    {
        exponents.Add(polyReduced.createOne());
        // polyReduced must be reduced!
        T @base = polyReduced.clone(); //polyMod(poly, polyModulus, invMod, true);
        exponents.Add(@base);
        T prev = @base;
        for (int i = 0; i < nIterations; i++)
            exponents.Add(prev =
                UnivariatePolynomialArithmetic.polyMod(prev.clone().multiply(@base), polyModulus, invMod, false));
    }


    public static UnivariatePolynomialZp64 powModulusMod(UnivariatePolynomialZp64 poly,
        UnivariatePolynomialZp64 polyModulus,
        UnivariateDivision.InverseModMonomial<UnivariatePolynomialZp64> invMod,
        List<UnivariatePolynomialZp64> xPowers)
    {
        poly = UnivariatePolynomialArithmetic.polyMod(poly, polyModulus, invMod, true);
        return powModulusMod0(poly, polyModulus, invMod, xPowers);
    }


    private static UnivariatePolynomialZp64 powModulusMod0(UnivariatePolynomialZp64 poly,
        UnivariatePolynomialZp64 polyModulus,
        UnivariateDivision.InverseModMonomial<UnivariatePolynomialZp64> invMod,
        List<UnivariatePolynomialZp64> xPowers)
    {
        UnivariatePolynomialZp64 res = poly.createZero();
        for (int i = poly.Degree; i >= 0; --i)
        {
            if (poly.data[i] == 0)
                continue;
            res.addMul(xPowers[i], poly.data[i]);
        }

        return UnivariatePolynomialArithmetic.polyMod(res, polyModulus, invMod, false);
    }


    public static UnivariatePolynomial<E> powModulusMod<E>(UnivariatePolynomial<E> poly,
        UnivariatePolynomial<E> polyModulus,
        UnivariateDivision.InverseModMonomial<UnivariatePolynomial<E>> invMod,
        List<UnivariatePolynomial<E>> xPowers)
    {
        poly = UnivariatePolynomialArithmetic.polyMod(poly, polyModulus, invMod, true);
        return powModulusMod0(poly, polyModulus, invMod, xPowers);
    }


    private static UnivariatePolynomial<E> powModulusMod0<E>(UnivariatePolynomial<E> poly,
        UnivariatePolynomial<E> polyModulus,
        UnivariateDivision.InverseModMonomial<UnivariatePolynomial<E>> invMod,
        List<UnivariatePolynomial<E>> xPowers)
    {
        UnivariatePolynomial<E> res = poly.createZero();
        for (int i = poly.Degree; i >= 0; --i)
        {
            if (poly.ring.isZero(poly.data[i]))
                continue;
            res.addMul(xPowers.get(i), poly.data[i]);
        }

        return UnivariatePolynomialArithmetic.polyMod(res, polyModulus, invMod, false);
    }


    public static T powModulusMod<T>(T poly,
        T polyModulus,
        UnivariateDivision.InverseModMonomial<T> invMod,
        List<T> xPowers) where T : IUnivariatePolynomial<T>
    {
        if (poly is UnivariatePolynomialZp64)
            return (T)powModulusMod((UnivariatePolynomialZp64)poly, (UnivariatePolynomialZp64)polyModulus,
                (UnivariateDivision.InverseModMonomial<UnivariatePolynomialZp64>)invMod,
                (List<UnivariatePolynomialZp64>)xPowers);
        else if (poly is UnivariatePolynomial)
            return (T)powModulusMod((UnivariatePolynomial)poly, (UnivariatePolynomial)polyModulus,
                (UnivariateDivision.InverseModMonomial)invMod, (List<T>)xPowers);
        else
            throw new Exception();
    }

    private static T powModulusMod0<T>(T poly,
        T polyModulus,
        UnivariateDivision.InverseModMonomial<T> invMod,
        List<T> xPowers) where T : IUnivariatePolynomial<T>
    {
        if (poly is UnivariatePolynomialZp64)
            return (T)powModulusMod0((UnivariatePolynomialZp64)poly, (UnivariatePolynomialZp64)polyModulus,
                (UnivariateDivision.InverseModMonomial)invMod, (List<T>)xPowers);
        else if (poly is UnivariatePolynomial)
            return (T)powModulusMod0((UnivariatePolynomial)poly, (UnivariatePolynomial)polyModulus,
                (UnivariateDivision.InverseModMonomial)invMod, (List<T>)xPowers);
        else
            throw new RuntimeException();
    }


    public static T compositionBrentKung<T>(
        T poly,
        List<T> pointPowers,
        T polyModulus,
        UnivariateDivision.InverseModMonomial<T> invMod,
        int tBrentKung) where T : IUnivariatePolynomial<T>
    {
        if (poly.isConstant())
            return poly;
        List<T> gj = [];
        int degree = poly.degree();
        for (int i = 0; i <= degree;)
        {
            int to = i + tBrentKung;
            if (to > (degree + 1))
                to = degree + 1;
            T g = poly.getRange(i, to);
            gj.Add(powModulusMod0(g, polyModulus, invMod, pointPowers));
            i = to;
        }

        T pt = pointPowers[tBrentKung];
        T res = poly.createZero();
        for (int i = gj.Count - 1; i >= 0; --i)
            res = UnivariatePolynomialArithmetic.polyMod(res.multiply(pt).add(gj[i]), polyModulus, invMod, false);
        return res;
    }


    public static T compositionBrentKung<T>(T poly, T point, T polyModulus,
        UnivariateDivision.InverseModMonomial<T> invMod) where T : IUnivariatePolynomial<T>
    {
        if (poly.isConstant())
            return poly;
        int t = safeToInt(Math.Sqrt(poly.degree()));
        List<T> hPowers = polyPowers(point, polyModulus, invMod, t);
        return compositionBrentKung(poly, hPowers, polyModulus, invMod, t);
    }

    private static int safeToInt(double dbl)
    {
        if (dbl > int.MaxValue || dbl < int.MinValue)
            throw new ArithmeticException("int overflow");
        return (int)dbl;
    }


    public static UnivariatePolynomialZp64 compositionHorner(UnivariatePolynomialZp64 poly,
        UnivariatePolynomialZp64 point, UnivariatePolynomialZp64 polyModulus,
        UnivariateDivision.InverseModMonomial<UnivariatePolynomialZp64> invMod)
    {
        if (poly.isConstant())
            return poly;
        UnivariatePolynomialZp64 res = poly.createZero();
        for (int i = poly.Degree; i >= 0; --i)
            res = UnivariatePolynomialArithmetic.polyMod(res.multiply(point).addMonomial(poly.data[i], 0), polyModulus,
                invMod, false);
        return res;
    }


    public static T composition<T>(T poly, T point, T polyModulus,
        UnivariateDivision.InverseModMonomial<T> invMod) where T : IUnivariatePolynomial<T>
    {
        return compositionBrentKung(poly, point, polyModulus, invMod);
    }


    public static T composition<T>(T poly, T point, T polyModulus) where T : IUnivariatePolynomial<T>
    {
        return compositionBrentKung(poly, point, polyModulus, UnivariateDivision.fastDivisionPreConditioning(point));
    }
}