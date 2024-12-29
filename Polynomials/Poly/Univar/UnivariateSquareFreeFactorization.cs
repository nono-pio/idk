using Polynomials.Poly.Multivar;
using Polynomials.Utils;
using static Polynomials.Poly.Univar.Conversions64bit;

namespace Polynomials.Poly.Univar;

public static class UnivariateSquareFreeFactorization
{
    public static bool IsSquareFree<E>(UnivariatePolynomial<E> poly)
    {
        return UnivariateGCD.PolynomialGCD(poly, poly.Derivative()).IsConstant();
    }


    public static PolynomialFactorDecomposition<UnivariatePolynomial<E>> SquareFreeFactorization<E>(
        UnivariatePolynomial<E> poly)
    {
        if (poly.IsOverFiniteField())
            return SquareFreeFactorizationMusser(poly);
        else if (UnivariateFactorization.IsOverMultivariate(poly))
            return (PolynomialFactorDecomposition<UnivariatePolynomial<E>>)GenericHandler.InvokeForGeneric<E>(
                typeof(MultivariatePolynomial<>),
                nameof(SquareFreeFactorizationOverMultivariate), typeof(UnivariateSquareFreeFactorization),
                poly); // UnivariateFactorization.FactorOverMultivariate(poly, MultivariateSquareFreeFactorization.SquareFreeFactorization);
        else if (UnivariateFactorization.IsOverUnivariate(poly))
            return (PolynomialFactorDecomposition<UnivariatePolynomial<E>>)GenericHandler.InvokeForGeneric<E>(
                typeof(UnivariatePolynomial<>),
                nameof(SquareFreeFactorizationOverUnivariate), typeof(UnivariateSquareFreeFactorization),
                poly); // UnivariateFactorization.FactorOverUnivariate(poly, MultivariateSquareFreeFactorization.SquareFreeFactorization);
        else if (poly.CoefficientRingCharacteristic().IsZero)
            return SquareFreeFactorizationYunZeroCharacteristics(poly);
        else
            return SquareFreeFactorizationMusser(poly);
    }

    public static PolynomialFactorDecomposition<UnivariatePolynomial<MultivariatePolynomial<E>>>
        SquareFreeFactorizationOverMultivariate<E>(UnivariatePolynomial<MultivariatePolynomial<E>> poly)
    {
        return UnivariateFactorization.FactorOverMultivariate(poly,
            MultivariateSquareFreeFactorization.SquareFreeFactorization);
    }

    public static PolynomialFactorDecomposition<UnivariatePolynomial<UnivariatePolynomial<E>>>
        SquareFreeFactorizationOverUnivariate<E>(UnivariatePolynomial<UnivariatePolynomial<E>> poly)
    {
        return UnivariateFactorization.FactorOverUnivariate(poly,
            MultivariateSquareFreeFactorization.SquareFreeFactorization);
    }


    public static UnivariatePolynomial<E> SquareFreePart<E>(UnivariatePolynomial<E> poly)
    {
        return SquareFreeFactorization(poly).Factors.Where((x) => !x.IsMonomial()).Aggregate(poly.CreateOne(),
            (acc, p) => acc.Multiply(p));
    }


    public static PolynomialFactorDecomposition<UnivariatePolynomial<E>>
        SquareFreeFactorizationYunZeroCharacteristics<E>(UnivariatePolynomial<E> poly)
    {
        if (!poly.CoefficientRingCharacteristic().IsZero)
            throw new ArgumentException("Characteristics 0 expected");
        if (poly.IsConstant())
            return PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Of(poly);

        // x^2 + x^3 -> x^2 (1 + x)
        var exponent = 0;
        while (exponent <= poly.Degree() && poly.IsZeroAt(exponent))
        {
            ++exponent;
        }

        if (exponent == 0)
            return SquareFreeFactorizationYun0(poly);

        var expFree = poly.GetRange(exponent, poly.Degree() + 1);
        var fd = SquareFreeFactorizationYun0(expFree);
        fd.AddFactor(poly.CreateMonomial(1), exponent);
        return fd;
    }


    static PolynomialFactorDecomposition<UnivariatePolynomial<E>> SquareFreeFactorizationYun0<E>(
        UnivariatePolynomial<E> poly)
    {
        if (poly.IsConstant())
            return PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Of(poly);
        var content = poly.ContentAsPoly();
        if (poly.SignumOfLC() < 0)
            content = content.Negate();
        poly = poly.Clone().DivideByLC(content);
        if (poly.Degree() <= 1)
            return PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Of(content, poly);
        var factorization = PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Of(content);
        SquareFreeFactorizationYun0(poly, factorization);
        return factorization;
    }

    private static void SquareFreeFactorizationYun0<E>(UnivariatePolynomial<E> poly,
        PolynomialFactorDecomposition<UnivariatePolynomial<E>> factorization)
    {
        var derivative = poly.Derivative();
        var gcd = UnivariateGCD.PolynomialGCD(poly, derivative);
        if (gcd.IsConstant())
        {
            factorization.AddFactor(poly, 1);
            return;
        }

        var quot = UnivariateDivision.DivideAndRemainder(poly, gcd, false)[0];
        var dQuot = UnivariateDivision.DivideAndRemainder(derivative, gcd,
            false)[0]; // safely destroy (cloned) derivative (not used further)
        var i = 0;
        while (!quot.IsConstant())
        {
            ++i;
            dQuot = dQuot.Subtract(quot.Derivative());
            var factor = UnivariateGCD.PolynomialGCD(quot, dQuot);
            quot = UnivariateDivision.DivideAndRemainder(quot, factor,
                false)[0]; // can destroy quot in divideAndRemainder
            dQuot = UnivariateDivision.DivideAndRemainder(dQuot, factor,
                false)[0]; // can destroy dQuot in divideAndRemainder
            if (!factor.IsOne())
                factorization.AddFactor(factor, i);
        }
    }


    public static PolynomialFactorDecomposition<UnivariatePolynomial<E>>
        SquareFreeFactorizationMusserZeroCharacteristics<E>(UnivariatePolynomial<E> poly)
    {
        if (!poly.CoefficientRingCharacteristic().IsZero)
            throw new ArgumentException("Characteristics 0 expected");
        if (poly.IsConstant())
            return PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Of(poly);
        var content = poly.ContentAsPoly();
        if (poly.SignumOfLC() < 0)
            content = content.Negate();
        poly = poly.Clone().DivideByLC(content);
        if (poly.Degree() <= 1)
            return PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Of(content, poly);
        var factorization = PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Of(content);
        SquareFreeFactorizationMusserZeroCharacteristics0(poly, factorization);
        return factorization;
    }

    private static void SquareFreeFactorizationMusserZeroCharacteristics0<E>(UnivariatePolynomial<E> poly,
        PolynomialFactorDecomposition<UnivariatePolynomial<E>> factorization)
    {
        var derivative = poly.Derivative();
        var gcd = UnivariateGCD.PolynomialGCD(poly, derivative);
        if (gcd.IsConstant())
        {
            factorization.AddFactor(poly, 1);
            return;
        }

        var quot = UnivariateDivision.DivideAndRemainder(poly, gcd, false)[0]; // safely destroy (cloned) poly
        var i = 0;
        while (true)
        {
            ++i;
            var nextQuot = UnivariateGCD.PolynomialGCD(gcd, quot);
            gcd = UnivariateDivision.DivideAndRemainder(gcd, nextQuot, false)[0]; // safely destroy gcd (reassigned)
            var factor =
                UnivariateDivision.DivideAndRemainder(quot, nextQuot, false)
                    [0]; // safely destroy quot (reassigned further)
            if (!factor.IsConstant())
                factorization.AddFactor(factor, i);
            if (nextQuot.IsConstant())
                break;
            quot = nextQuot;
        }
    }


    public static PolynomialFactorDecomposition<UnivariatePolynomial<E>> SquareFreeFactorizationMusser<E>(
        UnivariatePolynomial<E> poly)
    {
        if (CanConvertToZp64(poly))
            return SquareFreeFactorizationMusser(AsOverZp64(poly)).MapTo(Convert<E>);
        poly = poly.Clone();
        var lc = poly.LcAsPoly();

        //make poly monic
        poly = poly.Monic();
        if (poly.IsConstant())
            return PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Of(lc);
        if (poly.Degree() <= 1)
            return PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Of(lc, poly);
        PolynomialFactorDecomposition<UnivariatePolynomial<E>> factorization;

        // x^2 + x^3 -> x^2 (1 + x)
        var exponent = 0;
        while (exponent <= poly.Degree() && poly.IsZeroAt(exponent))
        {
            ++exponent;
        }

        if (exponent == 0)
            factorization = SquareFreeFactorizationMusser0(poly);
        else
        {
            var expFree = poly.GetRange(exponent, poly.Degree() + 1);
            factorization = SquareFreeFactorizationMusser0(expFree);
            factorization.AddFactor(poly.CreateMonomial(1), exponent);
        }

        return factorization.SetUnit(lc);
    }


    private static PolynomialFactorDecomposition<UnivariatePolynomial<E>> SquareFreeFactorizationMusser0<E>(
        UnivariatePolynomial<E> poly)
    {
        poly.Monic();
        if (poly.IsConstant())
            return PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Of(poly);
        if (poly.Degree() <= 1)
            return PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Of(poly);
        var derivative = poly.Derivative();
        if (!derivative.IsZero())
        {
            var gcd = UnivariateGCD.PolynomialGCD(poly, derivative);
            if (gcd.IsConstant())
                return PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Of(poly);
            var quot = UnivariateDivision.DivideAndRemainder(poly, gcd,
                false)[0]; // can safely destroy poly (not used further)
            var result = PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Of(poly.CreateOne());
            var i = 0;

            //if (!quot.isConstant())
            while (true)
            {
                ++i;
                var nextQuot = UnivariateGCD.PolynomialGCD(gcd, quot);
                var factor =
                    UnivariateDivision.DivideAndRemainder(quot, nextQuot,
                        false)[0]; // can safely destroy quot (reassigned further)
                if (!factor.IsConstant())
                    result.AddFactor(factor.Monic(), i);
                gcd = UnivariateDivision.DivideAndRemainder(gcd, nextQuot, false)[0]; // can safely destroy gcd
                if (nextQuot.IsConstant())
                    break;
                quot = nextQuot;
            }

            if (!gcd.IsConstant())
            {
                gcd = PRoot(gcd);
                var gcdFactorization = SquareFreeFactorizationMusser0(gcd);
                gcdFactorization.RaiseExponents((int)poly.CoefficientRingCharacteristic());
                result.AddAll(gcdFactorization);
                return result;
            }
            else
                return result;
        }
        else
        {
            var pRoot = PRoot(poly);
            var fd = SquareFreeFactorizationMusser0(pRoot);
            fd.RaiseExponents((int)poly.CoefficientRingCharacteristic());
            return fd.SetUnit(poly.CreateOne());
        }
    }


    private static UnivariatePolynomial<E> PRoot<E>(UnivariatePolynomial<E> poly)
    {
        if (poly.CoefficientRingCharacteristic().CompareTo(int.MaxValue) >= 0)
            throw new ArgumentException("Infinite or too large ring: " + poly.ring);
        Ring<E> ring = poly.ring;

        // p^(m -1) used for computing p-th root of elements
        var inverseFactor = ring.Cardinality().Value / ring.Characteristic();
        var modulus = (int)poly.CoefficientRingCharacteristic();
        E[] rootData = poly.ring.CreateZeroesArray(poly.degree / modulus + 1);
        for (var i = poly.degree; i >= 0; --i)
            if (!poly.ring.IsZero(poly.data[i]))
            {
                rootData[i / modulus] = ring.Pow(poly.data[i], inverseFactor); // pRoot(poly.data[i], ring);
            }

        return poly.CreateFromArray(rootData);
    }
}