using System.Numerics;
using Polynomials.Poly.Univar;
using static Polynomials.Poly.Multivar.Conversions64bit;

namespace Polynomials.Poly.Multivar;

public static class MultivariateSquareFreeFactorization
{
    public static PolynomialFactorDecomposition<MultivariatePolynomial<E>> SquareFreeFactorization<E>(MultivariatePolynomial<E> poly)
    {
        if (poly.IsOverFiniteField())
            return SquareFreeFactorizationBernardin(poly);
        else if (MultivariateGCD.isOverPolynomialRing(poly))
            return MultivariateFactorization.tryNested(poly,
                MultivariateSquareFreeFactorization.SquareFreeFactorization);
        else if (poly.CoefficientRingCharacteristic().IsZero)
            return SquareFreeFactorizationYunZeroCharacteristics(poly);
        else
            return SquareFreeFactorizationBernardin(poly);
    }


    public static bool IsSquareFree<E>(MultivariatePolynomial<E> poly)
    {
        return MultivariateGCD.PolynomialGCD(poly, poly.Derivative()).IsConstant();
    }


    public static MultivariatePolynomial<E> SquareFreePart<E>(MultivariatePolynomial<E> poly)
    {
        return SquareFreeFactorization(poly).Factors.Where((x) => !x.IsMonomial()).Aggregate(poly.CreateOne(),
            (acc, p) => acc.Multiply(p));
    }

    private static void AddMonomial<E>(PolynomialFactorDecomposition<MultivariatePolynomial<E>> decomposition, MultivariatePolynomial<E> poly)
    {
        decomposition.AddUnit(poly.LcAsPoly());
        poly = poly.Monic();
        var term = poly.Lt();
        var mAlgebra = poly.monomialAlgebra;
        for (int i = 0; i < poly.nVariables; i++)
            if (term.exponents[i] > 0)
                decomposition.AddFactor(poly.Create(mAlgebra.GetUnitTerm(poly.nVariables).Set(i, 1)), term.exponents[i]);
    }

    private static PolynomialFactorDecomposition<MultivariatePolynomial<E>> FactorUnivariate<E>(MultivariatePolynomial<E> poly)
    {
        int var = poly.UnivariateVariable();
        return UnivariateSquareFreeFactorization.SquareFreeFactorization(poly.AsUnivariate()).MapTo((p) =>
            MultivariatePolynomial<E>.AsMultivariate(p, poly.nVariables, var, poly.ordering));
    }

    private static MultivariatePolynomial<E>[] ReduceContent<E>(MultivariatePolynomial<E> poly)
    {
        var monomialContent = poly.MonomialContent();
        poly = poly.DivideOrNull(monomialContent)!;
        var constantContent = poly.ContentAsPoly();
        if (poly.SignumOfLC() < 0)
            constantContent = constantContent.Negate();
        poly = poly.DivideByLC(constantContent)!;
        return [constantContent, poly.Create(monomialContent)];
    }


    public static PolynomialFactorDecomposition<MultivariatePolynomial<E>>
        SquareFreeFactorizationYunZeroCharacteristics<E>(MultivariatePolynomial<E> poly) 
    {
        if (!poly.CoefficientRingCharacteristic().IsZero)
            throw new ArgumentException("Characteristics 0 expected");
        if (poly.IsEffectiveUnivariate())
            return FactorUnivariate(poly);
        var original = poly;
        poly = poly.Clone();
        var content = ReduceContent(poly);
        var decomposition = PolynomialFactorDecomposition<MultivariatePolynomial<E>>.FromUnit(content[0]);
        AddMonomial(decomposition, content[1]);
        SquareFreeFactorizationYun0(poly, decomposition);

        // lc correction
        decomposition.SetLcFrom(original);
        return decomposition;
    }

    private static void SquareFreeFactorizationYun0<E>(MultivariatePolynomial<E> poly,
        PolynomialFactorDecomposition<MultivariatePolynomial<E>> factorization)
    {
        var derivative = poly.Derivative();
        var gcd = MultivariateGCD.PolynomialGCD(poly, derivative);
        if (gcd.IsConstant())
        {
            factorization.AddFactor(poly, 1);
            return;
        }

        var quot = MultivariateDivision.DivideExact(poly, gcd); // safely destroy (cloned) poly (not used further)
        var dQuot = poly.CreateArray(derivative.Length);
        for (int k = 0; k < derivative.Length; k++)
            dQuot[k] = MultivariateDivision.DivideExact(derivative[k], gcd);
        int i = 0;
        while (!quot.IsConstant())
        {
            ++i;
            var qd = quot.Derivative();
            for (int j = 0; j < derivative.Length; j++)
                dQuot[j] = dQuot[j].Subtract(qd[j]);
            var factor = MultivariateGCD.PolynomialGCD(quot, dQuot);
            quot = MultivariateDivision.DivideExact(quot, factor); // can destroy quot in divideAndRemainder
            for (int j = 0; j < derivative.Length; j++)
                dQuot[j] = MultivariateDivision.DivideExact(dQuot[j], factor); // can destroy dQuot in divideAndRemainder
            if (!factor.IsOne())
                factorization.AddFactor(factor, i);
        }
    }


    public static PolynomialFactorDecomposition<MultivariatePolynomial<E>>
        SquareFreeFactorizationMusserZeroCharacteristics<E>(MultivariatePolynomial<E> poly)
    {
        if (!poly.CoefficientRingCharacteristic().IsZero)
            throw new ArgumentException("Characteristics 0 expected");
        if (poly.IsEffectiveUnivariate())
            return FactorUnivariate(poly);
        poly = poly.Clone();
        var content = ReduceContent(poly);
        var decomposition = PolynomialFactorDecomposition<MultivariatePolynomial<E>>.FromUnit(content[0]);
        AddMonomial(decomposition, content[1]);
        SquareFreeFactorizationMusserZeroCharacteristics0(poly, decomposition);
        return decomposition;
    }

    private static void SquareFreeFactorizationMusserZeroCharacteristics0<E>(MultivariatePolynomial<E> poly,
        PolynomialFactorDecomposition<MultivariatePolynomial<E>> factorization)
    {
        var derivative = poly.Derivative();
        var gcd = MultivariateGCD.PolynomialGCD(poly, derivative);
        if (gcd.IsConstant())
        {
            factorization.AddFactor(poly, 1);
            return;
        }

        var quot = MultivariateDivision.DivideExact(poly, gcd); // safely destroy (cloned) poly
        int i = 0;
        while (true)
        {
            ++i;
            var nextQuot = MultivariateGCD.PolynomialGCD(gcd, quot);
            gcd = MultivariateDivision.DivideExact(gcd, nextQuot); // safely destroy gcd (reassigned)
            var factor = MultivariateDivision.DivideExact(quot, nextQuot); // safely destroy quot (reassigned further)
            if (!factor.IsConstant())
                factorization.AddFactor(factor, i);
            if (nextQuot.IsConstant())
                break;
            quot = nextQuot;
        }
    }


    public static PolynomialFactorDecomposition<MultivariatePolynomial<E>> SquareFreeFactorizationBernardin<E>(MultivariatePolynomial<E> poly)
    {
        if (poly.CoefficientRingCharacteristic().IsZero)
            throw new ArgumentException("Positive characteristic expected");
        if (CanConvertToZp64(poly))
            return SquareFreeFactorizationBernardin(AsOverZp64(poly)).MapTo(Conversions64bit.ConvertFromZp64<E>);
        if (poly.IsEffectiveUnivariate())
            return FactorUnivariate(poly);
        poly = poly.Clone();
        var content = ReduceContent(poly);
        var lc = poly.LcAsPoly();
        var fct = SquareFreeFactorizationBernardin0(poly);
        AddMonomial(fct, content[1]);
        return fct.AddFactor(content[0], 1).AddFactor(lc, 1);
    }


    private static PolynomialFactorDecomposition<MultivariatePolynomial<E>> SquareFreeFactorizationBernardin0<E>(MultivariatePolynomial<E> poly)
    {
        poly.Monic();
        if (poly.IsConstant())
            return PolynomialFactorDecomposition<MultivariatePolynomial<E>>.FromUnit(poly);
        if (poly.Degree() <= 1)
            return PolynomialFactorDecomposition<MultivariatePolynomial<E>>.Of(poly);
        var derivative = poly.Derivative();
        if (!derivative.All(d => d.IsZero()))
        {
            var gcd = MultivariateGCD.PolynomialGCD(poly, derivative);
            if (gcd.IsConstant())
                return PolynomialFactorDecomposition<MultivariatePolynomial<E>>.Of(poly);
            var quot = MultivariateDivision.DivideExact(poly, gcd); // can safely destroy poly (not used further)
            var result = PolynomialFactorDecomposition<MultivariatePolynomial<E>>.FromUnit(poly.CreateOne());
            int i = 0;

            //if (!quot.isConstant())
            while (true)
            {
                ++i;
                var nextQuot = MultivariateGCD.PolynomialGCD(gcd, quot);
                var factor = MultivariateDivision.DivideExact(quot, nextQuot); // can safely destroy quot (reassigned further)
                if (!factor.IsConstant())
                    result.AddFactor(factor.Monic(), i);
                gcd = MultivariateDivision.DivideExact(gcd, nextQuot); // can safely destroy gcd
                if (nextQuot.IsConstant())
                    break;
                quot = nextQuot;
            }

            if (!gcd.IsConstant())
            {
                gcd = PRoot(gcd);
                var gcdFactorization = SquareFreeFactorizationBernardin0(gcd);
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
            var fd = SquareFreeFactorizationBernardin0(pRoot);
            fd.RaiseExponents((int)poly.CoefficientRingCharacteristic());
            return fd.SetUnit(poly.CreateOne());
        }
    }

    private static MultivariatePolynomial<E> PRoot<E>(MultivariatePolynomial<E> poly)
    {
        Ring<E> ring = poly.ring;

        // p^(m -1) used for computing p-th root of elements
        BigInteger inverseFactor = ring.Cardinality()!.Value / ring.Characteristic();
        int modulus = (int)poly.CoefficientRingCharacteristic();
        MonomialSet<E> pRoot = new MonomialSet<E>(poly.ordering);
        foreach (Monomial<E> term in poly.terms)
        {
            int[] exponents = (int[])term.exponents.Clone();
            for (int i = 0; i < exponents.Length; i++)
            {
                exponents[i] = exponents[i] / modulus;
            }

            poly.Add(pRoot, new Monomial<E>(exponents, ring.Pow(term.coefficient, inverseFactor)));
        }

        return poly.Create(pRoot);
    }
}
