using System.Collections;
using System.Diagnostics;
using System.Numerics;
using Polynomials.Poly.Univar;
using Polynomials.Primes;
using Polynomials.Utils;
using static Polynomials.Poly.Multivar.Conversions64bit;
using MultivariatePolynomialZp64 = Polynomials.Poly.Multivar.MultivariatePolynomial<long>;
using UnivariatePolynomialZp64 = Polynomials.Poly.Univar.UnivariatePolynomial<long>;

namespace Polynomials.Poly.Multivar;

public static class MultivariateFactorization
{
    public static PolynomialFactorDecomposition<MultivariatePolynomial<E>> Factor<E>(MultivariatePolynomial<E> poly)
    {
        if (poly.IsOverFiniteField())
            return FactorInGF(poly);
        else if (poly.IsOverZ())
            return FactorInZ(poly.AsZ()) as PolynomialFactorDecomposition<MultivariatePolynomial<E>>;
        else if (Util.IsOverRationals(poly))
            return (PolynomialFactorDecomposition<MultivariatePolynomial<E>>)GenericHandler.InvokeForGeneric<E>(typeof(Rational<>), nameof(FactorInQ),
                typeof(MultivariateFactorization), poly);//FactorInQ(poly);
        // else if (Util.IsOverSimpleNumberField(poly))
        //     return  FactorInNumberField( poly);
        // else if (Util.IsOverMultipleFieldExtension(poly))
        //     return  FactorInMultipleFieldExtension( poly);
        else
        {
            var factors = tryNested(poly, MultivariateFactorization.Factor);
            if (factors != null)
                return factors;
            throw new Exception("Unsupported ring");
        }
    }


    static bool isOverMultivariate<E>(MultivariatePolynomial<E> poly)
    {
        return poly.ring is IMultivariateRing;
    }


    static bool isOverUnivariate<E>(MultivariatePolynomial<E> poly)
    {
        return poly.ring is IUnivariateRing;
    }


    public static PolynomialFactorDecomposition<MultivariatePolynomial<E>>? tryNested<E>(MultivariatePolynomial<E> poly,
        Func<MultivariatePolynomial<E>, PolynomialFactorDecomposition<MultivariatePolynomial<E>>> factorFunction)
    {
        if (MultivariateGCD.isOverUnivariate(poly))
            return (PolynomialFactorDecomposition<MultivariatePolynomial<E>>) GenericHandler.InvokeForGeneric<E>(typeof(UnivariatePolynomial<>), nameof(FactorOverUnivariate), typeof(MultivariateFactorization), poly, factorFunction); // FactorOverUnivariate(poly, factorFunction)
        else if (MultivariateGCD.isOverMultivariate(poly))
            return (PolynomialFactorDecomposition<MultivariatePolynomial<E>>) GenericHandler.InvokeForGeneric<E>(typeof(MultivariatePolynomial<>), nameof(FactorOverMultivariate), typeof(MultivariateFactorization), poly, factorFunction); // FactorOverMultivariate(poly, factorFunction);
        return null;
    }

    private static PolynomialFactorDecomposition<MultivariatePolynomial<UnivariatePolynomial<E>>>
        FactorOverUnivariate<E>(MultivariatePolynomial<UnivariatePolynomial<E>> poly,
            Func<MultivariatePolynomial<E>, PolynomialFactorDecomposition<MultivariatePolynomial<E>>> factorFunction)
    {
        return factorFunction(
            MultivariatePolynomial<E>.AsNormalMultivariate(poly, 0)).MapTo(p => p.AsOverUnivariateEliminate(0));
    }

    private static PolynomialFactorDecomposition<MultivariatePolynomial<MultivariatePolynomial<E>>>
        FactorOverMultivariate<E>(MultivariatePolynomial<MultivariatePolynomial<E>> poly,
            Func<MultivariatePolynomial<E>, PolynomialFactorDecomposition<MultivariatePolynomial<E>>> factorFunction)
    {
        int[] cfVars = Utils.Utils.Sequence(poly.Lc().nVariables);
        int[] mainVars = Utils.Utils.Sequence(poly.Lc().nVariables, poly.Lc().nVariables + poly.nVariables);
        return factorFunction(
                MultivariatePolynomial<E>.AsNormalMultivariate(poly, cfVars, mainVars))
            .MapTo(p => p.AsOverMultivariateEliminate(cfVars));
    }

    public static PolynomialFactorDecomposition<MultivariatePolynomial<Rational<E>>> FactorInQ<E>(
        MultivariatePolynomial<Rational<E>> polynomial)
    {
        (MultivariatePolynomial<E>, E) cmd = Util.ToCommonDenominator(polynomial);
        MultivariatePolynomial<E> integral = cmd.Item1;
        E denominator = cmd.Item2;
        return Factor(integral)
            .MapTo(p => Util.AsOverRationals(polynomial.ring, p))
            .AddUnit(polynomial.CreateConstant(new Rational<E>(integral.ring, integral.ring.GetOne(), denominator)));
    }

    // TODO
    // private static <
    //         Term extends AMonomial<Term>,
    //         mPoly extends AMultivariatePolynomial<Term, mPoly>,
    //         sPoly extends IUnivariatePolynomial<sPoly>
    //         > PolynomialFactorDecomposition<MultivariatePolynomial<mPoly>>
    // FactorInMultipleFieldExtension(MultivariatePolynomial<mPoly> poly) {
    //     MultipleFieldExtension<Term, mPoly, sPoly> ring = (MultipleFieldExtension<Term, mPoly, sPoly>) poly.ring;
    //     SimpleFieldExtension<sPoly> simpleExtension = ring.getSimpleExtension();
    //     return Factor(poly.mapCoefficients(simpleExtension, ring::inverse))
    //             .MapTo(p => p.mapCoefficients(ring, ring::image));
    // }

    public static PolynomialFactorDecomposition<MultivariatePolynomial<BigInteger>> FactorInZ(
        MultivariatePolynomial<BigInteger> polynomial)
    {
        return Factor(polynomial, MultivariateFactorization.factorPrimitiveInZ);
    }


    // public static PolynomialFactorDecomposition<MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>>
    // FactorInNumberField(MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> polynomial) {
    //     return Factor(polynomial, MultivariateFactorization.factorPrimitiveInNumberField);
    // }


    public static PolynomialFactorDecomposition<MultivariatePolynomial<E>> FactorInGF<E>(
        MultivariatePolynomial<E> polynomial)
    {
        if (CanConvertToZp64(polynomial))
            return FactorInGF(AsOverZp64(polynomial)).MapTo(Conversions64bit.ConvertFromZp64<E>);

        return Factor(polynomial, MultivariateFactorization.factorPrimitiveInGF);
    }

    private static PolynomialFactorDecomposition<MultivariatePolynomial<E>> Factor<E>(
        MultivariatePolynomial<E> polynomial,
        Func<MultivariatePolynomial<E>, PolynomialFactorDecomposition<MultivariatePolynomial<E>>> algorithm)
    {
        if (polynomial.IsEffectiveUnivariate())
            return factorUnivariate(polynomial);

        var
            // square-free decomposition
            sqf = MultivariateSquareFreeFactorization.SquareFreeFactorization(polynomial);
        // the result
        var res = PolynomialFactorDecomposition<MultivariatePolynomial<E>>.FromUnit(sqf.Unit);
        for (int i = 0; i < sqf.Count; i++)
        {
            var factor = sqf.Factors[i];
            // factor into primitive polynomials
            var primitiveFactors = factorToPrimitive(factor);
            res.AddUnit(primitiveFactors.Unit, sqf.Exponents[i]);
            foreach (var primitiveFactor in primitiveFactors.Factors)
            {
                // factor each primitive polynomial
                var pFactors = algorithm(primitiveFactor);
                res.AddUnit(pFactors.Unit, sqf.Exponents[i]);
                foreach (var pFactor in pFactors.Factors)
                    res.AddFactor(pFactor, sqf.Exponents[i]);
            }
        }

        return res;
    }

    /* ============================================== Auxiliary methods ============================================= */


    private static PolynomialFactorDecomposition<MultivariatePolynomial<E>> factorUnivariate<E>(
        MultivariatePolynomial<E> poly)
    {
        int uVar = poly.UnivariateVariable();
        var
            uFactors = UnivariateFactorization.Factor(poly.AsUnivariate());
        return uFactors.MapTo(u => MultivariatePolynomial<E>.AsMultivariate(u, poly.nVariables, uVar, poly.ordering));
    }


    static PolynomialFactorDecomposition<MultivariatePolynomial<E>> factorToPrimitive<E>(MultivariatePolynomial<E> poly)
    {
        if (poly.IsEffectiveUnivariate())
            return PolynomialFactorDecomposition<MultivariatePolynomial<E>>.Of(poly);
        var result = PolynomialFactorDecomposition<MultivariatePolynomial<E>>.Empty(poly);
        for (int i = 0; i < poly.nVariables; i++)
        {
            if (poly.Degree(i) == 0)
                continue;
            var factor = poly.AsUnivariate(i).Content();
            result.AddAll(factorToPrimitive(factor));
            poly = MultivariateDivision.DivideExact(poly, factor);
        }

        result.AddFactor(poly, 1);
        return result;
    }

    private static int[] add(int[] array, int value)
    {
        int[] res = new int[array.Length];
        for (int i = 0; i < array.Length; i++)
            res[i] = array[i] = value;
        return res;
    }

    private delegate PolynomialFactorDecomposition<MultivariatePolynomial<E>> FactorizationAlgorithm<E>(
        MultivariatePolynomial<E> poly, bool switchToExtensionField);

    private const long UNIVARIATE_FACTORIZATION_ATTEMPTS = 3;

    private const int EXTENSION_FIELD_EXPONENT = 3;


    // private static PolynomialFactorDecomposition<MultivariatePolynomialZp64> factorInExtensionField(
    //         MultivariatePolynomialZp64 poly,
    //         FactorizationAlgorithm<Monomial<UnivariatePolynomialZp64>, MultivariatePolynomial<UnivariatePolynomialZp64>> algorithm) {
    //
    //     IntegersZp64 ring = poly.ring;
    //     int startingDegree = EXTENSION_FIELD_EXPONENT;
    //     while (true) {
    //         FiniteField<UnivariatePolynomialZp64> extensionField = new FiniteField<>(
    //                 IrreduciblePolynomials.randomIrreduciblePolynomial(
    //                         ring.modulus, startingDegree++, cc.redberry.rings.poly.multivar.PrivateRandom.getRandom()));
    //
    //         PolynomialFactorDecomposition<MultivariatePolynomialZp64> result =
    //                 factorInExtensionField(poly, extensionField, algorithm);
    //
    //         if (result != null)
    //             return result;
    //     }
    // }


    // private static PolynomialFactorDecomposition<MultivariatePolynomialZp64> factorInExtensionField(
    //         MultivariatePolynomialZp64 poly,
    //         FiniteField<UnivariatePolynomialZp64> extensionField,
    //         FactorizationAlgorithm<Monomial<UnivariatePolynomialZp64>, MultivariatePolynomial<UnivariatePolynomialZp64>> algorithm) {
    //
    //     PolynomialFactorDecomposition<MultivariatePolynomial<UnivariatePolynomialZp64>> factorization
    //             = algorithm.factor(poly.mapCoefficients(extensionField, extensionField::valueOf), false);
    //     if (factorization == null)
    //         // too small extension
    //         return null;
    //     if (!factorization.unit.cc().IsConstant())
    //         return null;
    //
    //     PolynomialFactorDecomposition<MultivariatePolynomialZp64> result = PolynomialFactorDecomposition.unit(poly.createConstant(factorization.unit.cc().cc()));
    //     for (int i = 0; i < factorization.size(); i++) {
    //         if (!factorization.get(i).stream().allMatch(p => p.IsConstant()))
    //             return null;
    //         result.AddFactor(factorization.get(i).mapCoefficientsZp64(poly.ring, UnivariatePolynomialZp64::cc), factorization.getExponent(i));
    //     }
    //     return result;
    // }

    private static PolynomialFactorDecomposition<MultivariatePolynomial<E>> factorInExtensionField<E>(
        MultivariatePolynomial<E> poly,
        FactorizationAlgorithm<UnivariatePolynomial<E>> algorithm)
    {
        Ring<E> ring = poly.ring;

        int startingDegree = EXTENSION_FIELD_EXPONENT;
        while (true)
        {
            FiniteField<E> extensionField = new FiniteField<E>(
                IrreduciblePolynomials.RandomIrreduciblePolynomial(
                    ring, startingDegree++, PrivateRandom.GetRandom()));

            PolynomialFactorDecomposition<MultivariatePolynomial<E>> result =
                factorInExtensionField(poly, extensionField, algorithm);

            if (result != null)
                return result;
        }
    }

    private static PolynomialFactorDecomposition<MultivariatePolynomial<E>> factorInExtensionField<E>(
        MultivariatePolynomial<E> poly,
        FiniteField<E> extensionField,
        FactorizationAlgorithm<UnivariatePolynomial<E>> algorithm)
    {
        PolynomialFactorDecomposition<MultivariatePolynomial<UnivariatePolynomial<E>>> factorization
            = algorithm(poly.MapCoefficients(extensionField, c => UnivariatePolynomial<E>.Constant(poly.ring, c)),
                false);
        if (factorization == null)
            // too small extension
            return null;
        if (!factorization.Unit.Cc().IsConstant())
            return null;

        PolynomialFactorDecomposition<MultivariatePolynomial<E>> result =
            PolynomialFactorDecomposition<MultivariatePolynomial<E>>.FromUnit(
                poly.CreateConstant(factorization.Unit.Cc().Cc()));
        for (int i = 0; i < factorization.Count; i++)
        {
            if (!factorization[i].Stream().All(u => u.IsConstant()))
                return null;
            result.AddFactor(factorization[i].MapCoefficients(poly.ring, u => u.Cc()), factorization.Exponents[i]);
        }

        return result;
    }

    /* ========================================= Newton polygons (bivariate) ======================================== */


    static int[][] convexHull(int[][] points)
    {
        if (points.Length <= 2)
        {
            if (points[0][0] == points[1][0] && points[0][1] == points[1][1])
                return new int[][] { points[0] };
            return points;
        }

        // find the base point
        int basePointIndex = 0, minY = int.MinValue, minX = int.MaxValue;
        for (int i = 0; i < points.Length; ++i)
        {
            int[] point = points[i];
            if (point[1] < minY || (point[1] == minY && point[0] < minX))
            {
                minY = point[1];
                minX = point[0];
                basePointIndex = i;
            }
        }

        int[] basePoint = points[basePointIndex];
        Array.Sort(points, new PolarAngleComparator(basePoint));

        Stack<int[]> stack = new Stack<int[]>();
        stack.Push(points[0]);
        stack.Push(points[1]);

        for (int i = 2; i < points.Length; i++)
        {
            int[] head = points[i];
            int[] middle = stack.Pop();
            int[] tail = stack.Peek();

            int turn = ccw(tail, middle, head);

            if (turn > 0)
            {
                stack.Push(middle);
                stack.Push(head);
            }
            else if (turn < 0)
                i--;
            else
                stack.Push(head);
        }

        return stack.ToArray();
    }

    private static int ccw(int[] p1, int[] p2, int[] p3)
    {
        return (p2[0] - p1[0]) * (p3[1] - p1[1]) - (p2[1] - p1[1]) * (p3[0] - p1[0]);
    }

    private sealed class PolarAngleComparator : IComparer<int[]>
    {
        private readonly int[] basePoint;

        public PolarAngleComparator(int[] basePoint)
        {
            this.basePoint = basePoint;
        }

        public int Compare(int[] p1, int[] p2)
        {
            if (Array.Equals(p1, p2))
                return 0;
            int
                p1x = p1[0] - basePoint[0],
                p2x = p2[0] - basePoint[0],
                p1y = p1[1] - basePoint[1],
                p2y = p2[1] - basePoint[1];
            double
                d1 = Math.Sqrt(p1x * p1x + p1y * p1y),
                d2 = Math.Sqrt(p2x * p2x + p2y * p2y),
                cos1 = p1x / d1,
                cos2 = p2x / d2;

            int c = cos2.CompareTo(cos1);
            if (c != 0)
                return c;
            return d1.CompareTo(d2);
        }
    }

    static int[][] NewtonPolygon(IEnumerable poly)
    {
        List<int[]> points = new List<int[]>();
        foreach (DegreeVector dv in poly)
            points.Add((int[])dv.exponents.Clone());
        return convexHull(points.ToArray());
    }

    static int[][] NewtonPolygon<E>(MultivariatePolynomial<E> poly)
    {
        return NewtonPolygon(poly.GetSkeleton());
    }

    static bool isCertainlyIndecomposable(int[][] np)
    {
        if (np.Length == 2)
        {
            int xDeg = int.Max(np[0][0], np[1][0]);
            int yDeg = int.Max(np[0][1], np[1][1]);
            return MachineArithmetic.Gcd(xDeg, yDeg) == 1;
        }
        else if (np.Length == 3)
        {
            // if np of form (n, 0), (0, m), (u, v)

            int n = -1, m = -1, u = -1, v = -1;
            foreach (int[] xy in np)
            {
                if (xy[0] != 0 && xy[1] == 0)
                    n = xy[0];
                else if (xy[1] != 0 && xy[0] == 0)
                    m = xy[1];
                else
                {
                    u = xy[0];
                    v = xy[1];
                }
            }

            return n != -1 && m != -1 && u != -1 && v != -1
                   && MachineArithmetic.Gcd(n, m, u, v) == 1;
        }
        else
            return false;
    }

    static bool isBivariateCertainlyIrreducible<E>(MultivariatePolynomial<E> poly)
    {
        return poly.nVariables == 2 && isCertainlyIndecomposable(NewtonPolygon(poly));
    }

    /* ================================= Bivariate factorization over finite fields ================================= */

    static PolynomialFactorDecomposition<MultivariatePolynomialZp64> bivariateDenseFactorSquareFreeInGF(
        MultivariatePolynomialZp64 poly)
    {
        return bivariateDenseFactorSquareFreeInGF(poly, true);
    }


    static PolynomialFactorDecomposition<MultivariatePolynomialZp64> bivariateDenseFactorSquareFreeInGF(
        MultivariatePolynomialZp64 poly, bool switchToExtensionField, bool doGCDTest)
    {
        // assert poly.nUsedVariables() <= 2 && IntStream.range(2, poly.nVariables).allMatch(i => poly.Degree(i) == 0) : poly;

        if (poly.IsEffectiveUnivariate())
            return factorUnivariate(poly);

        var mContent = poly.MonomialContent();
        if (mContent.totalDegree != 0)
            return bivariateDenseFactorSquareFreeInGF(poly.DivideOrNull(mContent)).AddFactor(poly.Create(mContent), 1);

        if (isBivariateCertainlyIrreducible(poly))
            return PolynomialFactorDecomposition<MultivariatePolynomialZp64>.Of(poly);

        MultivariatePolynomialZp64 reducedPoly = poly;
        int[] degreeBounds = reducedPoly.Degrees();

        // use main variable with maximal degree
        bool swapVariables = false;
        if (degreeBounds[1] > degreeBounds[0])
        {
            swapVariables = true;
            reducedPoly = MultivariatePolynomial<long>.SwapVariables(reducedPoly, 0, 1);
            Utils.Utils.Swap(degreeBounds, 0, 1);
        }

        MultivariatePolynomialZp64 xDerivative = reducedPoly.Derivative(0);
        if (xDerivative.IsZero())
        {
            reducedPoly = MultivariatePolynomialZp64.SwapVariables(reducedPoly, 0, 1);
            swapVariables = !swapVariables;
            xDerivative = reducedPoly.Derivative(0);
        }

        if (doGCDTest)
        {
            // if we are in extension, gcd test was already done
            MultivariatePolynomialZp64 yDerivative = reducedPoly.Derivative(1);
            // use yDerivative first, since it is more simple
            foreach (var derivative in (MultivariatePolynomialZp64[]) [yDerivative, xDerivative])
            {
                if (derivative.IsZero())
                    continue;
                var dGCD = MultivariateGCD.PolynomialGCD(derivative, reducedPoly);
                if (!dGCD.IsConstant())
                {
                    PolynomialFactorDecomposition<MultivariatePolynomialZp64>
                        gcdFactorization = bivariateDenseFactorSquareFreeInGF(dGCD, switchToExtensionField, doGCDTest),
                        restFactorization = bivariateDenseFactorSquareFreeInGF(
                            MultivariateDivision.DivideExact(reducedPoly, dGCD), switchToExtensionField, doGCDTest);

                    if (gcdFactorization == null || restFactorization == null)
                    {
                        Debug.Assert(!switchToExtensionField);
                        return null;
                    }

                    gcdFactorization.AddAll(restFactorization);
                    if (swapVariables)
                        swap(gcdFactorization);

                    return gcdFactorization;
                }
            }
        }

        IntegersZp64 ring = (IntegersZp64)reducedPoly.ring;
        // degree in main variable
        int degree = reducedPoly.Degree(0);
        // substitution value for second variable
        long ySubstitution = -1;
        // univariate factorization
        PolynomialFactorDecomposition<UnivariatePolynomialZp64> uFactorization = null;

        // number of univariate factorizations tried
        int univariateFactorizations = 0;
        bool tryZeroFirst = true;

        HashSet<long> evaluationStack = new HashSet<long>();
        Random random = PrivateRandom.GetRandom();
        while (univariateFactorizations < UNIVARIATE_FACTORIZATION_ATTEMPTS)
        {
            if (evaluationStack.Count == ring.modulus)
                if (uFactorization != null)
                    // found at least one univariate factorization => use it
                    break;
                else if (switchToExtensionField)
                    // switch to extension field
                    return factorInExtensionField(poly,
                        (p, toExtension) => bivariateDenseFactorSquareFreeInGF(p, toExtension, false));
                else
                    return null;

            long substitution;
            if (tryZeroFirst)
            {
                // first try to substitute 0 for second variable, then use random values
                substitution = 0;
                tryZeroFirst = false;
            }
            else
                do
                {
                    substitution = ring.RandomElement(random);
                } while (evaluationStack.Contains(substitution));

            evaluationStack.Add(substitution);

            MultivariatePolynomialZp64 image = reducedPoly.Evaluate(1, substitution);
            if (image.Degree() != degree)
                // unlucky substitution
                continue;

            if (image.Cc() == 0)
                // c.c. must not be zero since input is primitive
                // => unlucky substitution
                continue;

            UnivariatePolynomialZp64 uImage = image.AsUnivariate();
            if (!UnivariateSquareFreeFactorization.IsSquareFree(uImage))
                // ensure that univariate image is also square free
                continue;

            PolynomialFactorDecomposition<UnivariatePolynomialZp64> factorization =
                UnivariateFactorization.FactorSquareFreeInGF(uImage);
            if (factorization.Count == 1)
                // irreducible polynomial
                return PolynomialFactorDecomposition<MultivariatePolynomialZp64>.Of(poly);


            if (uFactorization == null || factorization.Count < uFactorization.Count)
            {
                // better univariate factorization found
                uFactorization = factorization;
                ySubstitution = substitution;
            }

            //if (ySubstitution == 0)
            //   break;

            ++univariateFactorizations;
        }

        Debug.Assert(ySubstitution != -1);
        Debug.Assert(uFactorization.Factors.All(u => u.IsMonic()));

        // univariate factors are calculated
        List<UnivariatePolynomialZp64> factorList = uFactorization.Factors;

        // we don't precompute correct leading coefficients of bivariate factors
        // instead, we add the l.c. of the product to a list of lifting factors
        // in order to obtain correct factorization with monic factors mod (y - y0)^l
        // and then perform l.c. correction at the recombination stage

        long[] evals = new long[poly.nVariables - 1];
        evals[0] = ySubstitution;
        HenselLifting.Evaluation<long> evaluation =
            new HenselLifting.Evaluation<long>(poly.nVariables, evals, ring, reducedPoly.ordering);
        MultivariatePolynomialZp64 lc = reducedPoly.Lc(0);
        if (!lc.IsConstant())
        {
            // add lc to lifting factors
            UnivariatePolynomialZp64 ulc = evaluation.evaluateFrom(lc, 1).AsUnivariate();
            Debug.Assert(ulc.IsConstant());
            factorList.Insert(0, ulc);
        }
        else
            factorList[0].Multiply(lc.Cc());

        // final factors to lift
        UnivariatePolynomialZp64[] factors = factorList.ToArray();

        // lift univariate factorization
        int liftDegree = reducedPoly.Degree(1) + 1;

        Ring<UnivariatePolynomialZp64> uRing = new UnivariateRing<long>(factors[0]);
        // series expansion around y = y0 for initial poly
        UnivariatePolynomial<UnivariatePolynomialZp64> baseSeries =
            HenselLifting.seriesExpansionDense(uRing, reducedPoly, 1, evaluation);

        // lifted factors (each factor represented as series around y = y0)
        UnivariatePolynomial<UnivariatePolynomialZp64>[] lifted =
            HenselLifting.bivariateLiftDense(baseSeries, factors, liftDegree);

        if (!lc.IsConstant())
            // drop auxiliary l.c. from factors
            lifted = lifted[1..];

        // factors are lifted => do recombination
        PolynomialFactorDecomposition<MultivariatePolynomialZp64> result =
            denseBivariateRecombination(reducedPoly, baseSeries, lifted, evaluation, liftDegree);

        if (swapVariables)
            // reconstruct original variables order
            swap(result);

        return result;
    }

    private static void swap<E>(PolynomialFactorDecomposition<MultivariatePolynomial<E>> factorDecomposition)
    {
        for (int i = 0; i < factorDecomposition.Factors.Count; i++)
            factorDecomposition.Factors[i] = MultivariatePolynomial<E>.SwapVariables(factorDecomposition[i], 0, 1);
    }


    static PolynomialFactorDecomposition<MultivariatePolynomial<E>>
        bivariateDenseFactorSquareFreeInGF<E>(MultivariatePolynomial<E> poly)
    {
        return bivariateDenseFactorSquareFreeInGF(poly, true);
    }


    static PolynomialFactorDecomposition<MultivariatePolynomial<E>>
        bivariateDenseFactorSquareFreeInGF<E>(MultivariatePolynomial<E> poly, bool switchToExtensionField,
            bool doGCDTest)
    {
        Debug.Assert(poly.NUsedVariables() <= 2 &&
                     Enumerable.Range(2, poly.nVariables).All(i => poly.Degree(i) == 0));

        if (poly.IsEffectiveUnivariate())
            return factorUnivariate(poly);

        Monomial<E> mContent = poly.MonomialContent();
        if (mContent.totalDegree != 0)
            return bivariateDenseFactorSquareFreeInGF(poly.DivideOrNull(mContent)).AddFactor(poly.Create(mContent), 1);

        if (isBivariateCertainlyIrreducible(poly))
            return PolynomialFactorDecomposition<MultivariatePolynomial<E>>.Of(poly);

        MultivariatePolynomial<E> reducedPoly = poly;
        int[] degreeBounds = reducedPoly.Degrees();

        // use main variable with maximal degree
        bool swapVariables = false;
        if (degreeBounds[1] > degreeBounds[0])
        {
            swapVariables = true;
            reducedPoly = MultivariatePolynomial<E>.SwapVariables(reducedPoly, 0, 1);
            Utils.Utils.Swap(degreeBounds, 0, 1);
        }

        MultivariatePolynomial<E> xDerivative = reducedPoly.Derivative(0);
        if (xDerivative.IsZero())
        {
            reducedPoly = MultivariatePolynomial<E>.SwapVariables(reducedPoly, 0, 1);
            swapVariables = !swapVariables;
            xDerivative = reducedPoly.Derivative(0);
        }

        if (doGCDTest)
        {
            MultivariatePolynomial<E> yDerivative = reducedPoly.Derivative(1);
            // use yDerivative first, since it is more simple
            foreach (MultivariatePolynomial<E> derivative in (MultivariatePolynomial<E>[]) [yDerivative, xDerivative])
            {
                if (derivative.IsZero())
                    continue;
                MultivariatePolynomial<E> dGCD = MultivariateGCD.PolynomialGCD(xDerivative, reducedPoly);
                if (!dGCD.IsConstant())
                {
                    PolynomialFactorDecomposition<MultivariatePolynomial<E>>
                        gcdFactorization = bivariateDenseFactorSquareFreeInGF(dGCD, switchToExtensionField),
                        restFactorization =
                            bivariateDenseFactorSquareFreeInGF(MultivariateDivision.DivideExact(reducedPoly, dGCD),
                                switchToExtensionField);

                    if (gcdFactorization == null || restFactorization == null)
                    {
                        Debug.Assert(!switchToExtensionField);
                        return null;
                    }

                    gcdFactorization.AddAll(restFactorization);
                    if (swapVariables)
                        swap(gcdFactorization);

                    return gcdFactorization;
                }
            }
        }

        Ring<E> ring = reducedPoly.ring;
        // degree in main variable
        int degree = reducedPoly.Degree(0);
        // substitution value for second variable
        Utils.Nullable<E> ySubstitution = Utils.Nullable<E>.Null;
        // univariate factorization
        PolynomialFactorDecomposition<UnivariatePolynomial<E>> uFactorization = null;

        // number of univariate factorizations tried
        int univariateFactorizations = 0;
        bool tryZeroFirst = true;
        HashSet<E> evaluationStack = new HashSet<E>();
        while (univariateFactorizations < UNIVARIATE_FACTORIZATION_ATTEMPTS)
        {
            if (ring.Cardinality().Value.IsInt() && (int)ring.Cardinality().Value == evaluationStack.Count)
                if (uFactorization != null)
                    // found at least one univariate factorization => use it
                    break;
                else if (switchToExtensionField)
                    // switch to extension field
                    return factorInExtensionField(poly,
                        (p, toExtension) => bivariateDenseFactorSquareFreeInGF(p, toExtension, false));
                else
                    return null;

            E substitution;
            if (tryZeroFirst)
            {
                // first try to substitute 0 for second variable, then use random values
                substitution = ring.GetZero();
                tryZeroFirst = false;
            }
            else
                do
                {
                    substitution = ring.RandomElement(PrivateRandom.GetRandom());
                } while (evaluationStack.Contains(substitution));

            evaluationStack.Add(substitution);

            MultivariatePolynomial<E> image = reducedPoly.Evaluate(1, substitution);
            if (image.Degree() != degree)
                // unlucky substitution
                continue;

            if (ring.IsZero(image.Cc()))
                // c.c. must not be zero since input is primitive
                // => unlucky substitution
                continue;

            UnivariatePolynomial<E> uImage = image.AsUnivariate();
            if (!UnivariateSquareFreeFactorization.IsSquareFree(uImage))
                // ensure that univariate image is also square free
                continue;

            PolynomialFactorDecomposition<UnivariatePolynomial<E>> factorization =
                UnivariateFactorization.FactorSquareFreeInGF(uImage);
            if (factorization.Count == 1)
                // irreducible polynomial
                return PolynomialFactorDecomposition<MultivariatePolynomial<E>>.Of(poly);


            if (uFactorization == null || factorization.Count < uFactorization.Count)
            {
                // better univariate factorization found
                uFactorization = factorization;
                ySubstitution = substitution;
            }

            //if (ySubstitution == 0)
            //   break;

            ++univariateFactorizations;
        }

        Debug.Assert(ySubstitution != null);

        // univariate factors are calculated

        List<UnivariatePolynomial<E>> factorList = uFactorization.Factors;

        // we don't precompute correct leading coefficients of bivariate factors
        // instead, we add the l.c. of the product to a list of lifting factors
        // in order to obtain correct factorization with monic factors mod (y - y0)^l
        // and then perform l.c. correction at the recombination stage

        E[] evals = ring.CreateZeroesArray(poly.nVariables - 1);
        evals[0] = ySubstitution.Value;
        HenselLifting.Evaluation<E> evaluation =
            new HenselLifting.Evaluation<E>(poly.nVariables, evals, ring, reducedPoly.ordering);
        MultivariatePolynomial<E> lc = reducedPoly.Lc(0);
        if (!lc.IsConstant())
        {
            // add lc to lifting factors
            UnivariatePolynomial<E> ulc = evaluation.evaluateFrom(lc, 1).AsUnivariate();
            Debug.Assert(ulc.IsConstant());
            factorList.Insert(0, ulc);
        }
        else
            factorList[0].Multiply(lc.Cc());

        // final factors to lift

        UnivariatePolynomial<E>[] factors = factorList.ToArray();

        // lift univariate factorization
        int liftDegree = reducedPoly.Degree(1) + 1;

        Ring<UnivariatePolynomial<E>> uRing = new UnivariateRing<E>(factors[0]);
        // series expansion around y = y0 for initial poly
        UnivariatePolynomial<UnivariatePolynomial<E>> baseSeries =
            HenselLifting.seriesExpansionDense(uRing, reducedPoly, 1, evaluation);

        // lifted factors (each factor represented as series around y = y0)
        UnivariatePolynomial<UnivariatePolynomial<E>>[] lifted =
            HenselLifting.bivariateLiftDense(baseSeries, factors, liftDegree);

        if (!lc.IsConstant())
            // drop auxiliary l.c. from factors
            lifted = lifted[1..];

        // factors are lifted => do recombination
        PolynomialFactorDecomposition<MultivariatePolynomial<E>> result =
            denseBivariateRecombination(reducedPoly, baseSeries, lifted, evaluation, liftDegree);

        if (swapVariables)
            // reconstruct original variables order
            for (int i = 0; i < result.Factors.Count; i++)
                result.Factors[i] = MultivariatePolynomial<E>.SwapVariables(result[i], 0, 1);

        return result;
    }

    /** cache of references **/
    private static int[][] naturalSequenceRefCache = new int[32][];

    private static int[] createSeq(int n)
    {
        int[] r = new int[n];
        for (int i = 0; i < n; i++)
            r[i] = i;
        return r;
    }

    /** returns sequence of natural numbers */
    private static int[] naturalSequenceRef(int n)
    {
        if (n >= naturalSequenceRefCache.Length)
            return createSeq(n);
        if (naturalSequenceRefCache[n] != null)
            return naturalSequenceRefCache[n];
        return naturalSequenceRefCache[n] = createSeq(n);
    }

    /** select elements by their positions */
    private static int[] select(int[] data, int[] positions)
    {
        int[] r = new int[positions.Length];
        int i = 0;
        foreach (int p in positions)
            r[i++] = data[p];
        return r;
    }

    static PolynomialFactorDecomposition<MultivariatePolynomial<E>> denseBivariateRecombination<E>(
        MultivariatePolynomial<E> factory,
        UnivariatePolynomial<UnivariatePolynomial<E>> poly,
        UnivariatePolynomial<UnivariatePolynomial<E>>[] modularFactors,
        HenselLifting.IEvaluation<E> evaluation,
        int liftDegree)
    {
        int[] modIndexes = naturalSequenceRef(modularFactors.Length);
        var trueFactors = PolynomialFactorDecomposition<MultivariatePolynomial<E>>.Empty(factory);
        var fRest = poly;
        int s = 1;


        while (2 * s <= modIndexes.Length)
        {
            foreach (int[] combination in Combinatorics.GetCombinations(modIndexes.Length, s))
            {
                int[] indexes = select(modIndexes, combination);

                var mFactor = lcInSeries(fRest);
                foreach (int i in indexes)
                    // todo:
                    // implement IUnivariatePolynomial#MultiplyLow(int)
                    // and replace truncate(int) with MultiplyLow(int)
                    mFactor = mFactor.Multiply(modularFactors[i]).Truncate(liftDegree - 1);

                // get primitive part in first variable (remove R[y] Content)
                var factor = changeDenseRepresentation(changeDenseRepresentation(mFactor).PrimitivePart());

                var qd = UnivariateDivision.DivideAndRemainder(fRest, factor, true);
                if (qd != null && qd[1].IsZero())
                {
                    modIndexes = Utils.Utils.IntSetDifference(modIndexes, indexes);
                    trueFactors.AddFactor(HenselLifting.denseSeriesToPoly(factory, factor, 1, evaluation), 1);
                    fRest = qd[0];
                    goto factor_combinations;
                }
            }

            ++s;
            factor_combinations: ;
        }

        if (!fRest.IsConstant() || !fRest.Cc().IsConstant())
            trueFactors.AddFactor(HenselLifting.denseSeriesToPoly(factory, fRest, 1, evaluation), 1);

        return trueFactors.Monic();
    }

    /* ======================================= Bivariate factorization over Z ======================================= */

    /**
     * Factors primitive, square-free bivariate polynomial over Z
     *
     * @param poly primitive, square-free bivariate polynomial over Z
     * @return factor decomposition
     */
    static PolynomialFactorDecomposition<MultivariatePolynomial<BigInteger>>
        bivariateDenseFactorSquareFreeInZ(MultivariatePolynomial<BigInteger> poly)
    {
        Debug.Assert(poly.NUsedVariables() <= 2 &&
                     Enumerable.Range(2, poly.nVariables - 2).All(i => poly.Degree(i) == 0));

        if (poly.IsEffectiveUnivariate())
            return factorUnivariate(poly);

        Monomial<BigInteger> mContent = poly.MonomialContent();
        if (mContent.totalDegree != 0)
            return bivariateDenseFactorSquareFreeInZ(poly.DivideOrNull(mContent)).AddFactor(poly.Create(mContent), 1);

        if (isBivariateCertainlyIrreducible(poly))
            return PolynomialFactorDecomposition<MultivariatePolynomial<BigInteger>>.Of(poly);

        MultivariatePolynomial<BigInteger> Content = poly.ContentAsPoly();
        MultivariatePolynomial<BigInteger> reducedPoly = Content.IsOne() ? poly : poly.Clone().DivideByLC(Content);
        int[] degreeBounds = reducedPoly.Degrees();

        // use main variable with maximal degree
        bool swapVariables = false;
        if (degreeBounds[1] > degreeBounds[0])
        {
            swapVariables = true;
            reducedPoly = MultivariatePolynomial<BigInteger>.SwapVariables(reducedPoly, 0, 1);
            Utils.Utils.Swap(degreeBounds, 0, 1);
        }

        MultivariatePolynomial<BigInteger> xDerivative = reducedPoly.Derivative(0);
        Debug.Assert(!xDerivative.IsZero());
        MultivariatePolynomial<BigInteger> dGCD = MultivariateGCD.PolynomialGCD(xDerivative, reducedPoly);
        if (!dGCD.IsConstant())
        {
            PolynomialFactorDecomposition<MultivariatePolynomial<BigInteger>>
                gcdFactorization = bivariateDenseFactorSquareFreeInZ(dGCD),
                restFactorization =
                    bivariateDenseFactorSquareFreeInZ(MultivariateDivision.DivideExact(reducedPoly, dGCD));

            gcdFactorization.AddAll(restFactorization);
            if (swapVariables)
                swap(gcdFactorization);

            return gcdFactorization.AddUnit(Content);
        }

        // degree in main variable
        int degree = reducedPoly.Degree(0);
        // substitution value for second variable
        BigInteger? ySubstitution = null;
        // univariate factorization
        PolynomialFactorDecomposition<UnivariatePolynomial<BigInteger>> uFactorization = null;

        // number of univariate factorizations tried
        int univariateFactorizations = 0;
        bool tryZeroFirst = true;
        UnivariatePolynomial<BigInteger> uImage = null;
        HashSet<BigInteger> usedSubstitutions = new HashSet<BigInteger>();
        while (univariateFactorizations < UNIVARIATE_FACTORIZATION_ATTEMPTS)
        {
            BigInteger substitution;
            if (tryZeroFirst)
            {
                // first try to substitute 0 for second variable, then use random values
                substitution = BigInteger.Zero;
                tryZeroFirst = false;
            }
            else
            {
                int bound = 10 * (univariateFactorizations / 5 + 1);
                if (bound < usedSubstitutions.Count)
                    bound = usedSubstitutions.Count;
                do
                {
                    if (usedSubstitutions.Count == bound)
                        bound *= 2;
                    substitution = new BigInteger(PrivateRandom.GetRandom().Next(bound));
                } while (usedSubstitutions.Contains(substitution));

                usedSubstitutions.Add(substitution);
            }

            MultivariatePolynomial<BigInteger> image = reducedPoly.Evaluate(1, substitution);
            if (image.Degree() != degree)
                // unlucky substitution
                continue;

            if (image.Cc().IsZero)
                // c.c. must not be zero since input is primitive
                // => unlucky substitution
                continue;

            uImage = image.AsUnivariate();
            if (!UnivariateSquareFreeFactorization.IsSquareFree(uImage))
                // ensure that univariate image is also square free
                continue;

            PolynomialFactorDecomposition<UnivariatePolynomial<BigInteger>> factorization =
                UnivariateFactorization.FactorSquareFreeInZ(uImage);
            if (factorization.Count == 1)
                // irreducible polynomial
                return PolynomialFactorDecomposition<MultivariatePolynomial<BigInteger>>.Of(poly);

            if (uFactorization == null || factorization.Count < uFactorization.Count)
            {
                // better univariate factorization found
                uFactorization = factorization;
                ySubstitution = substitution;
            }

            ++univariateFactorizations;
        }

        // univariate factors are calculated
        Debug.Assert(ySubstitution != null);

        // choose appropriate prime modulus
        int basePrime = 1 << 22;
        BigInteger bBasePrime;
        while (true)
        {
            basePrime = SmallPrimes.NextPrime(basePrime);
            bBasePrime = new BigInteger(basePrime);
            if (!isGoodPrime(bBasePrime, uImage.Lc(), uImage.Cc()))
                continue;

            IntegersZp moduloDomain = new IntegersZp(bBasePrime);
            // ensure that univariate factors are still co-prime
            if (!PolynomialMethods.CoprimeQ(uFactorization.MapTo(f => f.SetRing(moduloDomain)).Factors))
                continue;

            break;
        }

        // chose prime**k which exceeds the coefficient bound

        // prime bound is 2 * bound(poly) * bound(poly.lc(0)) (lc must be included since we don't
        // precompute correct lc but use instead exhaustive lifting and recombination)
        BigInteger bound2 = (coefficientsBound(reducedPoly) * (coefficientsBound(reducedPoly.Lc(0)))) << 1;
        BigInteger modulus = bBasePrime;
        while (modulus.CompareTo(bound2) < 0)
            modulus *= bBasePrime;
        IntegersZp zpDomain = new IntegersZp(modulus);

        List<UnivariatePolynomial<BigInteger>> factorsListZp =
            uFactorization.MapTo(f => f.SetRing(zpDomain)).Monic().Factors;
        MultivariatePolynomial<BigInteger>
            baseZp = reducedPoly.SetRing(zpDomain),
            lcZp = baseZp.Lc(0);
        baseZp = baseZp.DivideOrNull(lcZp.Evaluate(1, ySubstitution.Value).Lc());
        Debug.Assert(baseZp != null);

        // we don't precompute correct leading coefficients of bivariate factors
        // instead, we add the l.c. of the product to a list of lifting factors
        // in order to obtain correct factorization with monic factors mod (y - y0)^l
        // and then perform l.c. correction at the recombination stage

        BigInteger[] evals = new BigInteger[poly.nVariables - 1];
        evals[0] = ySubstitution.Value;
        HenselLifting.Evaluation<BigInteger> evaluation =
            new HenselLifting.Evaluation<BigInteger>(poly.nVariables, evals, zpDomain, baseZp.ordering);
        if (!lcZp.IsConstant())
        {
            // add lc to lifting factors
            Debug.Assert(evaluation.evaluateFrom(lcZp, 1).IsConstant());
            factorsListZp.Insert(0, factorsListZp[0].CreateOne());
        }

        // final factors to lift

        UnivariatePolynomial<BigInteger>[] factorsZp = factorsListZp.ToArray();

        // lift univariate factorization
        int liftDegree = baseZp.Degree(1) + 1;

        // series expansion around y = y0 for initial poly
        UnivariatePolynomial<UnivariatePolynomial<BigInteger>> baseSeriesZp =
            HenselLifting.seriesExpansionDense(Rings.UnivariateRingZp(modulus), baseZp, 1, evaluation);

        // lifted factors (each factor represented as series around y = y0)
        UnivariatePolynomial<UnivariatePolynomial<BigInteger>>[] liftedZp;
        try
        {
            liftedZp = HenselLifting.bivariateLiftDense(baseSeriesZp, factorsZp, liftDegree);
        }
        catch (ArithmeticException e)
        {
            // bad base prime selected
            // try again
            return bivariateDenseFactorSquareFreeInZ(poly);
        }

        if (!lcZp.IsConstant())
            // drop auxiliary l.c. from factors
            liftedZp = liftedZp[1..];

        // factors are lifted => do recombination
        UnivariatePolynomial<UnivariatePolynomial<BigInteger>> baseSeriesZ =
            seriesExpansionDenseZ(reducedPoly, ySubstitution.Value);
        PolynomialFactorDecomposition<MultivariatePolynomial<BigInteger>> result = denseBivariateRecombinationZ(
            reducedPoly, baseZp, baseSeriesZ, liftedZp, evaluation, ySubstitution.Value, zpDomain, liftDegree);

        if (swapVariables)
            // reconstruct original variables order
            for (int i = 0; i < result.Factors.Count; i++)
                result.Factors[i] = MultivariatePolynomial<BigInteger>.SwapVariables(result[i], 0, 1);

        return result.AddUnit(Content);
    }

    private static bool isGoodPrime(BigInteger prime, BigInteger ulc, BigInteger ucc)
    {
        ucc = BigInteger.Abs(ucc);
        ulc = BigInteger.Abs(ulc);
        if (!ulc.IsOne && (prime.CompareTo(ulc) > 0 ? prime % ulc : ulc % prime).IsZero)
            return false;
        if (!ucc.IsOne && !ucc.IsZero && (prime.CompareTo(ucc) > 0 ? prime % ucc : ucc % prime).IsZero)
            return false;
        return true;
    }

    static BigInteger coefficientsBound(MultivariatePolynomial<BigInteger> poly)
    {
        BigInteger maxNorm = BigInteger.Zero;
        foreach (BigInteger c in poly.Coefficients())
        {
            BigInteger abs = BigInteger.Abs(c);
            if (abs.CompareTo(maxNorm) > 0)
                maxNorm = abs;
        }

        Debug.Assert(maxNorm.Sign > 0);

        int[] degrees = poly.Degrees();
        int degreeSum = 0;
        BigInteger bound = BigInteger.One;
        foreach (int d in degrees)
        {
            degreeSum += d;
            bound = bound * (new BigInteger(d) + 1);
        }

        bound = bound / (BigInteger.One << degrees.Length) + 1;
        bound = BigIntegerUtils.SqrtCeil(bound);
        bound = bound * (BigInteger.One << degreeSum);
        bound = bound * maxNorm;

        Debug.Assert(bound.Sign > 0);
        return bound;
    }

    static PolynomialFactorDecomposition<MultivariatePolynomial<BigInteger>> denseBivariateRecombinationZ(
        MultivariatePolynomial<BigInteger> baseZ,
        MultivariatePolynomial<BigInteger> factoryZp,
        UnivariatePolynomial<UnivariatePolynomial<BigInteger>> baseSeriesZ,
        UnivariatePolynomial<UnivariatePolynomial<BigInteger>>[] modularFactorsZp,
        HenselLifting.Evaluation<BigInteger> evaluation,
        BigInteger ySubstitution,
        Ring<BigInteger> modulus,
        int liftDegree)
    {
        int[] modIndexes = naturalSequenceRef(modularFactorsZp.Length);
        var trueFactors = PolynomialFactorDecomposition<MultivariatePolynomial<BigInteger>>.Empty(baseZ);
        UnivariatePolynomial<UnivariatePolynomial<BigInteger>> fRest = baseSeriesZ;
        int s = 1;

        var lPowersZ = new MultivariatePolynomial<BigInteger>.USubstitution(
            UnivariatePolynomial<BigInteger>.CreateUnsafe(Rings.Z, new BigInteger[] { -ySubstitution, BigInteger.One }),
            1, baseZ.nVariables, baseZ.ordering);

        UnivariateRing<BigInteger> moduloDomain = Rings.UnivariateRing(modulus);

        Debug.Assert(baseZ.Lc(0).Equals(denseSeriesToPolyZ(baseZ, lcInSeries(fRest), lPowersZ)));
        Debug.Assert(baseZ.Equals(denseSeriesToPolyZ(baseZ, baseSeriesZ, lPowersZ)));

        while (2 * s <= modIndexes.Length)
        {
            foreach (int[] combination in Combinatorics.GetCombinations(modIndexes.Length, s))
            {
                int[] indexes = select(modIndexes, combination);

                UnivariatePolynomial<UnivariatePolynomial<BigInteger>> factor = lcInSeries(fRest).SetRing(moduloDomain);

                foreach (int i in indexes)
                    // todo:
                    // implement IUnivariatePolynomial#MultiplyLow(int)
                    // and replace truncate(int) with MultiplyLow(int)
                    factor = factor.Multiply(modularFactorsZp[i]).Truncate(liftDegree - 1);

                factor = seriesExpansionDenseZ(
                    MultivariatePolynomial<BigInteger>
                        .AsPolyZSymmetric(HenselLifting.denseSeriesToPoly(factoryZp, factor, 1, evaluation))
                        .PrimitivePart(1), ySubstitution);
                UnivariatePolynomial<UnivariatePolynomial<BigInteger>>[] qd =
                    UnivariateDivision.DivideAndRemainder(fRest, factor, true);
                if (qd != null && qd[1].IsZero())
                {
                    modIndexes = Utils.Utils.IntSetDifference(modIndexes, indexes);
                    trueFactors.AddFactor(denseSeriesToPolyZ(baseZ, factor, lPowersZ), 1);
                    fRest = qd[0];
                    goto factor_combinations;
                }
            }

            ++s;
            factor_combinations: ;
        }

        if (!fRest.IsConstant() || !fRest.Cc().IsConstant())
            if (trueFactors.Count == 0)
                trueFactors.AddFactor(baseZ, 1);
            else
                trueFactors.AddFactor(denseSeriesToPolyZ(baseZ, fRest, lPowersZ), 1);

        return trueFactors;
    }

    private static UnivariatePolynomial<UnivariatePolynomial<BigInteger>> seriesExpansionDenseZ(
        MultivariatePolynomial<BigInteger> poly,
        BigInteger ySubstitution)
    {
        int degree = poly.Degree(1);
        UnivariatePolynomial<BigInteger>[] coefficients = new UnivariatePolynomial<BigInteger>[degree + 1];
        for (int i = 0; i <= degree; i++)
            coefficients[i] = poly.SeriesCoefficient(1, i).Evaluate(1, ySubstitution).AsUnivariate();
        return UnivariatePolynomial<UnivariatePolynomial<BigInteger>>.CreateUnsafe(Rings.UnivariateRingZ, coefficients);
    }

    private static MultivariatePolynomial<BigInteger> denseSeriesToPolyZ(
        MultivariatePolynomial<BigInteger> factory,
        UnivariatePolynomial<UnivariatePolynomial<BigInteger>> series,
        MultivariatePolynomial<BigInteger>.USubstitution linearPowers)
    {
        MultivariatePolynomial<BigInteger> result = factory.CreateZero();
        for (int i = 0; i <= series.Degree(); i++)
        {
            MultivariatePolynomial<BigInteger> mPoly =
                MultivariatePolynomial<BigInteger>.AsMultivariate(series[i], factory.nVariables, 0, factory.ordering);
            result = result.Add(mPoly.Multiply(linearPowers.Pow(i)));
        }

        return result;
    }

    /** Given poly as R[x][y] transform it to R[y][x] */
    private static UnivariatePolynomial<UnivariatePolynomial<uE>> changeDenseRepresentation<uE>(
        UnivariatePolynomial<UnivariatePolynomial<uE>> poly)
    {
        int xDegree = -1;
        for (int i = 0; i <= poly.Degree(); i++)
            xDegree = Math.Max(xDegree, poly[i].Degree());

        var result = poly.CreateZero();
        for (int i = 0; i <= xDegree; i++)
            result[i] = coefficientInSeries(i, poly);
        return result;
    }

    /** Given poly as R[x][y] returns coefficient of x^xDegree which is R[y] */
    private static UnivariatePolynomial<uE> coefficientInSeries<uE>(int xDegree,
        UnivariatePolynomial<UnivariatePolynomial<uE>> poly)
    {
        Ring<UnivariatePolynomial<uE>> ring = poly.ring;
        var result = ring.GetZero();
        for (int i = 0; i <= poly.Degree(); i++)
            result.SetFrom(i, poly[i], xDegree);
        return result;
    }

    /**
     * Given poly as R[x][y] returns leading coefficient of x which is R[y] viewed as R[x][y] (with all coefficients
     * constant)
     */
    private static UnivariatePolynomial<UnivariatePolynomial<uE>> lcInSeries<uE>(
        UnivariatePolynomial<UnivariatePolynomial<uE>> poly)
    {
        var result = poly.CreateZero();
        int xDegree = -1;
        for (int i = 0; i <= poly.Degree(); i++)
            xDegree = Math.Max(xDegree, poly[i].Degree());

        for (int i = 0; i <= poly.Degree(); i++)
            result.Set(i, poly[i].GetAsPoly(xDegree));
        return result;
    }

    /**
     * Factors primitive, square-free bivariate polynomial
     *
     * @param poly                   primitive, square-free bivariate polynomial over Zp
     * @param switchToExtensionField whether to switch to extension field if ring cardinality is too small
     * @return factor decomposition
     */
    static PolynomialFactorDecomposition<MultivariatePolynomial<E>> bivariateDenseFactorSquareFreeInGF<E>(
        MultivariatePolynomial<E> poly, bool switchToExtensionField)
    {
        if (poly is MultivariatePolynomialZp64)
            return bivariateDenseFactorSquareFreeInGF(poly.AsZp64(), switchToExtensionField, true) as
                PolynomialFactorDecomposition<MultivariatePolynomial<E>>;
        else
            return bivariateDenseFactorSquareFreeInGF(poly, switchToExtensionField, true);
    }

    /* ================================ Multivariate factorization over finite fields ================================ */

    private static PolynomialFactorDecomposition<MultivariatePolynomial<E>> factorInExtensionFieldGeneric<E>(
        MultivariatePolynomial<E> poly, FactorizationAlgorithm<E> algorithm)
    {
        throw new NotImplementedException();
        // if (poly is MultivariatePolynomialZp64)
        //     return (PolynomialFactorDecomposition<Poly>) factorInExtensionField((MultivariatePolynomialZp64) poly, (FactorizationAlgorithm<Monomial<UnivariatePolynomialZp64>, MultivariatePolynomial<UnivariatePolynomialZp64>>) algorithm);
        // else if (poly is MultivariatePolynomial)
        //     return (PolynomialFactorDecomposition<Poly>) factorInExtensionField((MultivariatePolynomial) poly, (FactorizationAlgorithm) algorithm);
        // else
        //     throw new Exception();
    }

    sealed class OrderByDegrees<E>
    {
        public readonly MultivariatePolynomial<E> ordered;
        readonly int[] degreeBounds;
        public readonly int[] variablesSorted;
        public readonly int[] variablesMapping;
        readonly int nVariables;

        public OrderByDegrees(MultivariatePolynomial<E> ordered, int[] degreeBounds, int[] variablesSorted,
            int nVariables)
        {
            this.ordered = ordered;
            this.degreeBounds = degreeBounds;
            this.variablesSorted = variablesSorted;
            this.variablesMapping = MultivariateGCD.inversePermutation(variablesSorted);
            this.nVariables = nVariables;
        }

        public MultivariatePolynomial<E> restoreOrder(MultivariatePolynomial<E> factor)
        {
            return MultivariatePolynomial<E>.RenameVariables(
                factor.SetNVariables(nVariables), variablesMapping);
        }

        MultivariatePolynomial<E> order(MultivariatePolynomial<E> factor)
        {
            return MultivariatePolynomial<E>.RenameVariables(factor, variablesSorted);
        }
    }


    static OrderByDegrees<E> orderByDegrees<E>(MultivariatePolynomial<E> poly, bool reduceNVariables, int mainVariable)
    {
        return orderByDegrees(poly, reduceNVariables, false, mainVariable);
    }


    static OrderByDegrees<E> orderByDegrees<E>(MultivariatePolynomial<E> poly, bool reduceNVariables,
        bool sortByOccurrences, int mainVariable)
    {
        int
            nVariables = poly.nVariables;
        int[] degreeBounds = poly.Degrees(), // degree bounds for lifting
            uniqueOccurrences = poly.UniqueOccurrences(), // occurrences for sorting
            occurrences = poly.Occurrences(); // occurrences for sorting

        // Swap variables so that the first variable will have the maximal degree,
        // and all non-used variables are at the end of poly.
        //
        // The rest of sorting is done in the following way (thanks to Takahiro Ueda,
        // https://github.com/PoslavskySV/rings/issues/71):
        // variables are ordered by occurrences in descendent order. This way the number of terms in
        // bivariate images will be higher, so heuristically this should reduce the probability of
        // false-positive bivariate factorizations.

        int[] variables = Utils.Utils.Sequence(nVariables);
        if (mainVariable != -1)
        {
            int mainDegree = degreeBounds[mainVariable];
            degreeBounds[mainVariable] = int.MaxValue;
            //sort
            sortByDegreeAndOccurrences(degreeBounds, variables, uniqueOccurrences, occurrences, sortByOccurrences);
            //recover degreeBounds
            degreeBounds[variables.FirstIndexOf(mainVariable)] = mainDegree;
        }
        else
        {
            //sort
            sortByDegreeAndOccurrences(degreeBounds, variables, uniqueOccurrences, occurrences, sortByOccurrences);

            // chose the main variable in such way that the derivative
            // with respect to the main variable is not zero (avoid p-Power)
            int i = 0;
            for (; i < variables.Length; i++)
                if (!isPPower(poly, variables[i]))
                    break;

            if (i > 0)
            {
                Utils.Utils.Swap(variables, 0, i);
                Utils.Utils.Swap(degreeBounds, 0, i);
            }
        }

        int lastPresentVariable;
        if (reduceNVariables)
        {
            lastPresentVariable = 0; //recalculate lastPresentVariable
            for (; lastPresentVariable < degreeBounds.Length; ++lastPresentVariable)
                if (degreeBounds[lastPresentVariable] == 0)
                    break;
            --lastPresentVariable;
        }
        else
            lastPresentVariable = nVariables - 1;

        poly = MultivariatePolynomial<E>.RenameVariables(poly, variables)
            .SetNVariables(lastPresentVariable + 1);

        return new OrderByDegrees<E>(poly, degreeBounds, variables, nVariables);
    }

    private static void sortByDegreeAndOccurrences(int[] degreeBounds, int[] variables,
        int[] uniqueOccurrences, int[] occurrences,
        bool sortByOccurrences)
    {
        if (!sortByOccurrences)
        {
            Utils.Utils.InsertionSort(Utils.Utils.Negate(degreeBounds), variables);
            Utils.Utils.Negate(degreeBounds);
            return;
        }

        DegreeWithOccurrence[] data = new DegreeWithOccurrence[degreeBounds.Length];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = new DegreeWithOccurrence(degreeBounds[i], uniqueOccurrences[i], occurrences[i]);
        }

        //sort in descending order (NOTE: use stable sorting algorithm!!!)
        Utils.Utils.InsertionSort(data, variables);
        for (int i = 0; i < data.Length; i++)
        {
            degreeBounds[i] = data[i].degree;
        }
    }

    private sealed class DegreeWithOccurrence : IComparable<DegreeWithOccurrence>
    {
        public readonly int degree;
        readonly int uniqueOccurrences, occurrences;

        public DegreeWithOccurrence(int degree, int uniqueOccurrences, int occurrences)
        {
            this.degree = degree;
            this.uniqueOccurrences = uniqueOccurrences;
            this.occurrences = occurrences;
        }

        public int CompareTo(DegreeWithOccurrence oth)
        {
            int c = -degree.CompareTo(oth.degree);
            if (c != 0)
                return c;
            c = -uniqueOccurrences.CompareTo(oth.uniqueOccurrences);
            if (c != 0)
                return c;
            return -occurrences.CompareTo(oth.occurrences);
        }
    }


    static PolynomialFactorDecomposition<MultivariatePolynomial<E>> factorPrimitiveInGF<E>(
        MultivariatePolynomial<E> polynomial)
    {
        return factorPrimitiveInGF(polynomial, true);
    }

    static PolynomialFactorDecomposition<MultivariatePolynomial<E>> factorPrimitiveInGF<E>(
        MultivariatePolynomial<E> polynomial,
        bool switchToExtensionField)
    {
        if (polynomial.IsEffectiveUnivariate())
            return factorUnivariate(polynomial);

        // order the polynomial by degrees
        OrderByDegrees<E> input = orderByDegrees(polynomial, true, true, -1);
        PolynomialFactorDecomposition<MultivariatePolynomial<E>> decomposition =
            factorPrimitiveInGF0(input.ordered, switchToExtensionField);
        if (decomposition == null)
            return null;
        return decomposition.MapTo(input.restoreOrder);
    }

    sealed class LeadingCoefficientData<E>
    {
        // the following data represented as F[x2,x3,...,xN] (i.e. with x1 dropped, variables shifted)

        readonly MultivariatePolynomial<E> lc;
        readonly PolynomialFactorDecomposition<MultivariatePolynomial<E>> lcSqFreeDecomposition;
        readonly MultivariatePolynomial<E> lcSqFreePart;
        public readonly SplitContent<E>[] lcSplits;
        public readonly bool fullyReconstructable;


        public LeadingCoefficientData(MultivariatePolynomial<E> lc)
        {
            lc = lc.DropVariable(0);
            this.lc = lc;
            this.lcSqFreeDecomposition = MultivariateSquareFreeFactorization.SquareFreeFactorization(lc);
            this.lcSqFreePart = lcSqFreeDecomposition.SquareFreePart();

            var splits = new List<SplitContent<E>>();
            // split sq.-free part of l.c. in the max degree variable
            var Content = lcSqFreePart;
            while (!Content.IsConstant())
            {
                // if there is some non trivial Content, additional bivariate evaluations will be necessary

                int[] cDegrees = Content.Degrees();
                int[] variables = Utils.Utils.Sequence(0, cDegrees.Length);
                // use stable sort
                Utils.Utils.InsertionSort(Utils.Utils.Negate(cDegrees), variables);

                int iMax = 0;
                for (; iMax < variables.Length; iMax++)
                {
                    int maxDegreeVariable = variables[iMax];
                    if (cDegrees[iMax] == 0)
                        goto out_main;
                    var pContent = lcSqFreePart.ContentExcept(maxDegreeVariable);
                    var primitivePart = MultivariateDivision.DivideExact(lcSqFreePart, pContent);
                    if (containsPPower(primitivePart, maxDegreeVariable))
                        // ppPart if a p-Power in main variable (e.g. (a^3*b + c) for characteristic 3)
                        // => any univariate image will not be square-free,  so we are not able to
                        // reconstruct the l.c. using this bivariate factorization and just skip it
                        continue;

                    splits.Add(new SplitContent<E>(maxDegreeVariable, pContent, primitivePart));
                    Content = Content.ContentExcept(maxDegreeVariable);
                    goto main;
                }

                // Content is pPower in each variables => nothing more can be done
                break;

                main : ;
            }

            out_main : ;

            this.lcSplits = splits.ToArray();
            this.fullyReconstructable = Content.IsConstant();

            // assert that for domains of large characteristic l.c. always can be fully reconstructed
            Debug.Assert(fullyReconstructable || (lc.CoefficientRingCharacteristic() >> 16).Sign == 0);
        }
    }

    private static bool isPPower<E>(MultivariatePolynomial<E> p, int variable)
    {
        BigInteger characteristics = p.CoefficientRingCharacteristic();
        if (characteristics.IsZero)
            // poly over Z
            return false;
        if (!characteristics.IsInt())
            // characteristic is larger than maximal possible exponent
            return false;

        int modulus = (int)characteristics;
        if (modulus > p.Degree())
            return false;
        foreach (var term in p.terms)
            if (term.exponents[variable] % modulus != 0)
                return false;
        return true;
    }

    private static bool containsPPower<E>(MultivariatePolynomial<E> p, int variable)
    {
        BigInteger characteristics = p.CoefficientRingCharacteristic();
        return characteristics.IsInt()
               && (int)characteristics <= p.Degree()
               && !MultivariateGCD.PolynomialGCD(p, p.Derivative(variable)).IsConstant();
    }

    sealed class SplitContent<E>
    {
        public readonly int variable;
        readonly MultivariatePolynomial<E> Content;
        public readonly MultivariatePolynomial<E> primitivePart;
        public readonly OrderByDegrees<E> ppOrdered;

//        SplitContent(int variable, Poly poly) {
//            this.variable = variable;
//            this.Content = poly.ContentExcept(variable);
//            this.primitivePart = MultivariateDivision.DivideExact(poly, Content);
//            this.ppOrdered = orderByDegrees(primitivePart, false, variable);
//        }

        public SplitContent(int variable, MultivariatePolynomial<E> Content, MultivariatePolynomial<E> primitivePart)
        {
            this.variable = variable;
            this.Content = Content;
            this.primitivePart = primitivePart;
            this.ppOrdered = orderByDegrees(primitivePart, false, true, variable);
            Debug.Assert(!containsPPower(primitivePart, variable));
        }
    }

    private const int N_FAILS_BEFORE_SWITCH_TO_EXTENSION = 32;

    private const int N_INCONSISTENT_BIFACTORS_BEFORE_SWITCH_TO_EXTENSION = 32;

    private const int N_SUPERFLUOUS_FACTORS_BEFORE_TRY_OTHER_VAR = 8;


    static PolynomialFactorDecomposition<MultivariatePolynomial<E>> factorPrimitiveInGF0<E>(
        MultivariatePolynomial<E> poly,
        bool switchToExtensionField)
    {
        return factorPrimitiveInGF0(poly, -1, switchToExtensionField);
    }

    static PolynomialFactorDecomposition<MultivariatePolynomial<E>> factorPrimitiveInGF0<E>(
        MultivariatePolynomial<E> initialPoly,
        int fixSecondVar,
        bool switchToExtensionField)
    {
        // assert that poly is at least bivariate
        Debug.Assert(initialPoly.NUsedVariables() >= 2);
        // assert that degrees of variables are in the descending order
        Debug.Assert(initialPoly.Degree(1) > 0 && initialPoly.Degree(0) > 0);

        if (initialPoly.NUsedVariables() == 2)
            // bivariate case
            return bivariateDenseFactorSquareFreeInGF(initialPoly, switchToExtensionField);

        var poly = swapSecondVar(initialPoly, fixSecondVar);

        var xDerivative = poly.Derivative(0);
        Debug.Assert(!xDerivative.IsZero());

        var dGCD = MultivariateGCD.PolynomialGCD(xDerivative, poly);
        if (!dGCD.IsConstant())
        {
            PolynomialFactorDecomposition<MultivariatePolynomial<E>>
                gcdFactorization = factorPrimitiveInGF(dGCD, switchToExtensionField),
                restFactorization =
                    factorPrimitiveInGF(MultivariateDivision.DivideExact(poly, dGCD), switchToExtensionField);

            if (gcdFactorization == null || restFactorization == null)
            {
                Debug.Assert(!switchToExtensionField);
                return null;
            }

            return swapSecondVar(gcdFactorization.AddAll(restFactorization), fixSecondVar);
        }

        // whether ring cardinality is less than 1024
        bool isSmallCardinality = poly.CoefficientRingCardinality().Value.GetBitLength() <= 10;

        // the leading coefficient
        var lc = poly.Lc(0);
        LeadingCoefficientData<E> lcData = new LeadingCoefficientData<E>(lc);

        IEvaluationLoop<E> evaluations = getEvaluationsGF(poly);
        // number of attempts to find a suitable evaluation point
        int nAttempts = 0;
        // maximal number of bivariate factors
        int nBivariateFactors = int.MaxValue;
        // number of attempts to factor which lead to incompatible factorization patterns over
        // different second variable
        int[] nInconsistentBiFactorizations = new int[poly.nVariables];
        // number of fails due to bifactorsMain.size() > nBivariateFactors
        int nFailedWithSuperfluousFactors = 0;

        while (true)
        {
            // choose next evaluation
            HenselLifting.IEvaluation<E> evaluation = evaluations.next();

            if (evaluation == null || (nAttempts++ > N_FAILS_BEFORE_SWITCH_TO_EXTENSION && isSmallCardinality))
            {
                // switch to field extension
                if (switchToExtensionField)
                    return factorInExtensionFieldGeneric(initialPoly, MultivariateFactorization.factorPrimitiveInGF0);

                return null;
            }

            // check that evaluation does not change the rest degrees
            var images = poly.CreateArray(poly.nVariables - 1);
            for (int i = 0; i < images.Length; i++)
            {
                int variable = poly.nVariables - i - 1;
                images[i] = evaluation.evaluate(i == 0 ? poly : images[i - 1], variable);
                if (images[i].Degree(variable - 1) != poly.Degree(variable - 1))
                    goto main;
            }

            var
                bivariateImage = images[images.Length - 2];
            var
                univariateImage = images[images.Length - 1];

            Debug.Assert(bivariateImage.Degree(0) == poly.Degree(0) && bivariateImage.Degree(1) == poly.Degree(1));

            // check that l.c. of bivariate image has same degree in second variable
            // as the original l.c.
            if (lc.Degree(1) != bivariateImage.Lc(0).Degree(1))
                continue;

            // check that univariate image is also square-free
            if (!UnivariateSquareFreeFactorization.IsSquareFree(univariateImage.AsUnivariate()))
                continue;

            // check that bivariate image is also primitive
            if (!bivariateImage.ContentUnivariate(1).IsConstant())
                continue;

            // factor bivariate image
            var biFactorsMain =
                bivariateDenseFactorSquareFreeInGF(bivariateImage, false);
            if (biFactorsMain == null)
                if (switchToExtensionField)
                    return factorInExtensionFieldGeneric(initialPoly, MultivariateFactorization.factorPrimitiveInGF0);
                else
                    return null;

            if (biFactorsMain.Count == 1)
                return PolynomialFactorDecomposition<MultivariatePolynomial<E>>.Of(initialPoly);

            if (biFactorsMain.Count > nBivariateFactors)
            {
                ++nFailedWithSuperfluousFactors;
                if (nFailedWithSuperfluousFactors > N_SUPERFLUOUS_FACTORS_BEFORE_TRY_OTHER_VAR)
                {
                    // so we have that for any evaluation f[x1, x2, b3, ..., bN] has more factors than the initial poly
                    // we try another second variable
                    return factorPrimitiveInGF0(initialPoly, fixSecondVar + 1, switchToExtensionField);
                }

                // bad evaluation
                continue;
            }

            // release counter
            nFailedWithSuperfluousFactors = 0;

            nBivariateFactors = biFactorsMain.Count;

            // array of bivariate factors for lifting
            // (polynomials in F[x1, x2])
            MultivariatePolynomial<E>[] biFactorsArrayMain;
            if (!lc.IsConstant())
            {
                // <= leading coefficients reconstruction

                // bring main bivariate factorization in canonical order
                // (required for one-to-one correspondence between different bivariate factorizations)
                toCanonicalSort(biFactorsMain, evaluation);
                biFactorsArrayMain = biFactorsMain.Factors.ToArray();

                // the rest of l.c. (lc/lcFactors), will be constant at the end
                var lcRest = lc.Clone();
                // the true leading coefficients (to be calculated)
                var lcFactors = poly.CreateArray(biFactorsMain.Count);
                // initialize lcFactors with constants (correct ones!)
                for (int i = 0; i < lcFactors.Length; i++)
                {
                    lcFactors[i] = evaluation.evaluateFrom(biFactorsArrayMain[i].Lc(0), 1);
                    lcRest = lcRest.DivideByLC(lcFactors[i]);
                }

                if (lcData.lcSplits.Length == 0)
                {
                    // <- very small characteristic
                    // no any way to reconstruct any part of the l.c.
                    // but still we may try different factorizations to ensure that we have the
                    // correct factorization pattern (nBivariateFactors)

                    for (int freeVariable = 2; freeVariable < poly.nVariables; freeVariable++)
                    {
                        var biImage = evaluation.evaluateFromExcept(poly, 1, freeVariable);
                        if (biImage.Degree(0) != poly.Degree(0)
                            || biImage.Degree(freeVariable) != poly.Degree(freeVariable)
                            || biImage.Lc(0).Degree(freeVariable) != lc.Degree(freeVariable))
                            continue;

                        var fct = bivariateDenseFactorSquareFreeInGF(
                            orderByDegrees(biImage, true, -1).ordered, false);
                        if (fct != null && fct.Count < nBivariateFactors)
                        {
                            nBivariateFactors = fct.Count;
                            goto main;
                        }
                    }
                }

                // we perform additional bivariate factorizations in F[x1, x_i] for i = (3,..., N) (in special order)
                lc_reconstruction:
                for (int i = 0; i < lcData.lcSplits.Length && !lcRest.IsConstant(); i++)
                {
                    SplitContent<E> lcSplit = lcData.lcSplits[i];
                    // x_i -- the variable to leave unevaluated in addition to the main variable x_1
                    // (+1 required to obtain indexing as in the original poly)
                    int freeVariable = 1 + lcSplit.variable;

                    HenselLifting.IEvaluation<E>
                        // original evaluation with shuffled variables
                        iEvaluation = evaluation.renameVariables(lcSplit.ppOrdered.variablesSorted),
                        // the evaluation for l.c. (x1 dropped)
                        ilcEvaluation = iEvaluation.dropVariable(1);


                    // target for lifting
                    var ppPart = lcSplit.ppOrdered.ordered;
                    if (!UnivariateSquareFreeFactorization.IsSquareFree(ilcEvaluation.evaluateFrom(ppPart, 1)
                            .AsUnivariate()))
                    {
                        // univariate image may be non square-free for two reasons:
                        //
                        // 1. bad evaluation (common for fields of large characteristic)
                        //    => we can try another evaluation
                        //
                        // 2. ppPart is a p-Power in main variable (e.g. (a^3*b + c) for characteristic 3)
                        //    => any evaluation will lead to non square-free univariate image,
                        //    so we are not able to fully reconstruct the l.c.
                        //    --- this is not the case since we have filtered such splits in lcData,
                        //        (see #LeadingCoefficientData) so we just assert this case
                        Debug.Assert(!containsPPower(lcSplit.primitivePart, lcSplit.variable));

                        // <= try another evaluation
                        goto main;
                    }

                    // bivariate factors in F[x1, x_i]
                    PolynomialFactorDecomposition<MultivariatePolynomial<E>> biFactors;
                    if (freeVariable == 1)
                        biFactors = biFactorsMain;
                    else
                    {
                        // good image must have
                        //  - the same degree in the main variable
                        //  - the same degree in freeVariable
                        //  - l.c. with the same degree in freeVariable
                        var biImage = evaluation.evaluateFromExcept(poly, 1, freeVariable);
                        if (biImage.Degree(0) != poly.Degree(0)
                            || biImage.Degree(freeVariable) != poly.Degree(freeVariable)
                            || biImage.Lc(0).Degree(freeVariable) != lc.Degree(freeVariable))
                            goto main;

                        // bivariate factors in F[x1, x_i]
                        biFactors = bivariateDenseFactorSquareFreeInGF(
                            orderByDegrees(biImage, false, -1).ordered, false);
                        if (biFactors == null)
                            if (switchToExtensionField)
                                return factorInExtensionFieldGeneric(initialPoly,
                                    MultivariateFactorization.factorPrimitiveInGF0);
                            else
                                return null;
                        // bring in one-to-one correspondence with biFactorsMain
                        toCanonicalSort(biFactors, iEvaluation);
                    }

                    if (biFactors.Count != biFactorsMain.Count)
                    {
                        // number of factors should be the same since polynomial is primitive
                        // => bad evaluation occurred
                        if (biFactors.Count > biFactorsMain.Count)
                        {
                            ++nInconsistentBiFactorizations[freeVariable];
                            if (nInconsistentBiFactorizations[freeVariable] >
                                N_INCONSISTENT_BIFACTORS_BEFORE_SWITCH_TO_EXTENSION)
                            {
//                                assert isSmallCharacteristics(poly) : poly.coefficientRingCharacteristic();
                                // bad factorization pattern (very rare)
                                if (switchToExtensionField)
                                    return factorInExtensionFieldGeneric(initialPoly,
                                        MultivariateFactorization.factorPrimitiveInGF0);
                                else
                                    return null;
                            }
                            else
                                // bad evaluation occurred
                                goto main;
                        }
                        else
                        {
                            nBivariateFactors = biFactors.Count;
                            goto main;
                        }
                    }

                    // check that bivariate factorizations are compatible
                    if (biFactors != biFactorsMain
                        && !biFactors.MapTo(p => iEvaluation.evaluateFrom(p, 1).AsUnivariate()).Monic()
                            .Equals(biFactorsMain.MapTo(p => evaluation.evaluateFrom(p, 1).AsUnivariate()).Monic()))
                    {
                        // very rare event occurs only for domains of small cardinality and typically means that
                        // actual factorization has smaller number of factors than found in biFactorsMain

                        ++nInconsistentBiFactorizations[freeVariable];
                        if (nInconsistentBiFactorizations[freeVariable] >
                            N_INCONSISTENT_BIFACTORS_BEFORE_SWITCH_TO_EXTENSION)
                        {
                            Debug.Assert(isSmallCharacteristics(poly));
                            // bad factorization pattern (very rare)
                            if (switchToExtensionField)
                                return factorInExtensionFieldGeneric(initialPoly,
                                    MultivariateFactorization.factorPrimitiveInGF0);
                            else
                                return null;
                        }
                        else
                            // bad evaluation occurred
                            goto main;
                    }

                    //assert biFactors
                    //        .map(p => iEvaluation.evaluateFrom(p, 1).AsUnivariate()).monic()
                    //        .equals(biFactorsMain.map(p => evaluation.evaluateFrom(p, 1).AsUnivariate()).monic());

                    // square-free decomposition of the leading coefficients of bivariate factors
                    var ulcFactors =
                        biFactors.Factors
                            .Select(f =>
                                UnivariateSquareFreeFactorization.SquareFreeFactorization(f.Lc(0).AsUnivariate()))
                            .ToArray();

                    // move to GCD-free basis of sq.-f. decomposition (univariate, because fast)
                    GCDFreeBasis(ulcFactors);

                    // map to multivariate factors for further Hensel lifting
                    PolynomialFactorDecomposition<MultivariatePolynomial<E>>[]
                        ilcFactors = ulcFactors
                            .Select(decomposition => decomposition.MapTo(p =>
                                MultivariatePolynomial<E>.AsMultivariate(p, poly.nVariables - 1, 0, poly.ordering)))
                            .ToArray();


                    // <- set same polys in ulcFactors with the same single reference!
                    // NOTE: this is very important since we will use polynomials as references
                    // when doing lifting!
                    //
                    // comment: normally in most cases this is done automatically by GCDFreeBasis routine
                    // but in some cases (symmetric polynomials) this is not the case and manual correction required
                    for (int l = 0; l < ilcFactors.Length; l++)
                    for (int m = 0; m < ilcFactors[l].Factors.Count; m++)
                    {
                        var p = ilcFactors[l].Factors[m];
                        for (int l1 = l; l1 < ilcFactors.Length; l1++)
                        {
                            int m1Begin = l1 == l ? m + 1 : 0;
                            for (int m1 = m1Begin; m1 < ilcFactors[l1].Factors.Count; m1++)
                                if (ilcFactors[l1].Factors[m1].Equals(p))
                                    ilcFactors[l1].Factors[m1] = p;
                        }
                    }


                    // pick unique factors from lc decompositions (complete square-free )
                    HashSet<MultivariatePolynomial<E>> ilcFactorsSet = ilcFactors
                        .SelectMany(dec => dec.StreamWithoutUnit()).ToHashSet();
                    var ilcFactorsSqFree = ilcFactorsSet
                        .ToArray();

                    Debug.Assert(ilcFactorsSqFree.Length > 0);
                    Debug.Assert(!ilcFactorsSqFree.Any(p => p.IsConstant()));

                    // the sum of degrees of all unique factors in univariate gcd-free decomposition
                    // must be equal to the degree of primitive part we want to lift to
                    Debug.Assert(ilcFactorsSqFree.Select(f => f.Degree()).Aggregate(0, (a, b) => a + b)
                                 == ppPart.Degree(0));
//                    if (totalUDegree != ppPart.Degree(0)) {
//                        assert !UnivariateSquareFreeFactorization.isSquareFree(ilcEvaluation.evaluateFrom(ppPart, 1).AsUnivariate());
//                        // univariate image is not square-free two reasons possible:
//                        // 1. bad evaluation (common for fields of large characteristic)
//                        //    => we can try another evaluation
//                        // 2. ppPart if a p-Power in main variable (e.g. (a^3*b + c) for characteristic 3)
//                        //    => any evaluation will lead to non square-free univariate image,
//                        //    so we are not able to fully reconstruct the l.c.
//
//                        if (containsPPower(ppPart, 0))
//                            // <= we are not possible to reconstruct l.c. fully
//                            continue lc_reconstruction;
//                        else
//                            // <= try another evaluation, otherwise
//                            continue main;
//                    }

                    // we need to correct lcSqFreePrimitive (obtain correct numerical l.c.)
                    var ppPartLC = ilcEvaluation.evaluateFrom(ppPart.Lc(0), 1);
                    var realLC = ilcFactorsSqFree
                        .Select(fac => fac.LcAsPoly())
                        .Aggregate(ilcFactorsSqFree[0].CreateOne(), (a, b) => a.Multiply(b));

                    Debug.Assert(ppPartLC.IsConstant());
                    Debug.Assert(realLC.IsConstant());

                    var @base = ppPart.Clone().MultiplyByLC(realLC.DivideByLC(ppPartLC));
                    if (ilcFactorsSqFree.Length == 1)
                        ilcFactorsSqFree[0].Set(@base);
                    else
                        // <= lifting leading coefficients
                        HenselLifting.multivariateLiftAutomaticLC(@base, ilcFactorsSqFree, ilcEvaluation);

                    //assert Multiply(ilcFactorsSqFree).monic().equals(base.Clone().monic());

                    // l.c. has Content in x2
                    for (int jFactor = 0; jFactor < lcFactors.Length; jFactor++)
                    {
                        var obtainedLcFactor = MultivariatePolynomial<E>.RenameVariables(
                                ilcFactors[jFactor].Multiply(), lcSplit.ppOrdered.variablesMapping)
                            .InsertVariable(0);
                        var commonPart = MultivariateGCD.PolynomialGCD(obtainedLcFactor, lcFactors[jFactor]);
                        var addon = MultivariateDivision.DivideExact(obtainedLcFactor, commonPart);
                        // ensure that lcRest is divisible by addon
                        var addonR = MultivariateGCD.PolynomialGCD(addon, lcRest);
                        // either lcRest is divisible by addon or we are in very small characteristic
                        Debug.Assert(addon.Clone().Monic().Equals(addonR.Clone().Monic()) ||
                                     isSmallCharacteristics(poly));
                        addon = addonR;

                        // make addon monic when evaluated with evaluation
                        addon = addon.DivideByLC(evaluation.evaluateFrom(addon, 1));
                        lcFactors[jFactor] = lcFactors[jFactor].Multiply(addon);
                        lcRest = MultivariateDivision.DivideExact(lcRest, addon);
                    }
                }

                if (lcRest.IsConstant())
                {
                    // <= here we must be in _most_ cases

                    MultivariatePolynomial<E> @base;
                    if (lcRest.IsOne())
                        @base = poly.Clone().DivideByLC(biFactorsMain.Unit);
                    else
                    {
                        @base = poly.Clone();
                        @base.DivideByLC(lcRest);
                    }

                    HenselLifting.multivariateLift0(@base, biFactorsArrayMain, lcFactors, evaluation, @base.Degrees(),
                        2);
                }
                else
                {
                    // <= very rare event (very small characteristic)

                    Debug.Assert(!lcData.fullyReconstructable || isSmallCharacteristics(poly));

                    // Poly lcCorrection = evaluation.evaluateFrom(lcRest, 2);
                    for (int i = 0; i < biFactorsMain.Count; i++)
                    {
                        Debug.Assert(biFactorsArrayMain[i].Lt(MonomialOrder.LEX).exponents[0] ==
                                     biFactorsArrayMain[i].Degree(0));

                        lcFactors[i].Multiply(lcRest);
                        var correction = MultivariateDivision.DivideExact(evaluation.evaluateFrom(lcFactors[i], 2),
                            biFactorsArrayMain[i].Lc(0));
                        biFactorsArrayMain[i].Multiply(correction);
                    }

                    var @base = poly.Clone().Multiply(PolynomialMethods.PolyPow(lcRest, biFactorsMain.Count - 1, true));

                    Debug.Assert(Enumerable.Range(0, biFactorsMain.Count).All(i =>
                        biFactorsArrayMain[i].Lc(0).Equals(evaluation.evaluateFrom(lcFactors[i], 2))));
                    HenselLifting.multivariateLift0(@base, biFactorsArrayMain, lcFactors, evaluation, @base.Degrees(),
                        2);

                    foreach (var factor in biFactorsArrayMain)
                        factor.Set(HenselLifting.primitivePart(factor));
                }
            }
            else
            {
                MultivariatePolynomial<E> @base;
                if (biFactorsMain.Unit.IsOne())
                    @base = poly;
                else
                {
                    @base = poly.Clone();
                    @base.DivideByLC(biFactorsMain.Unit);
                }

                biFactorsArrayMain = biFactorsMain.Factors.ToArray();
                HenselLifting.multivariateLift0(@base, biFactorsArrayMain, null, evaluation, poly.Degrees(), 2);
            }

            PolynomialFactorDecomposition<MultivariatePolynomial<E>> factorization
                = PolynomialFactorDecomposition<MultivariatePolynomial<E>>.Of(biFactorsArrayMain.ToList())
                    .Monic()
                    .SetUnit(poly.LcAsPoly());

            MultivariatePolynomial<E>
                lcNumeric = factorization.Factors.Aggregate(factorization.Unit.Clone(),
                    (a, b) => a.LcAsPoly().Multiply(b.LcAsPoly())),
                ccNumeric = factorization.Factors.Aggregate(factorization.Unit.Clone(),
                    (a, b) => a.CcAsPoly().Multiply(b.CcAsPoly()));
            if (!lcNumeric.Equals(poly.LcAsPoly()) || !ccNumeric.Equals(poly.CcAsPoly()) ||
                !factorization.Multiply().Equals(poly))
            {
                // bad bivariate factorization => recombination required
                // instead of recombination we try again with another evaluation
                // searching for good enough bivariate factorization
                nBivariateFactors = factorization.Count - 1;
                continue;
            }

            return swapSecondVar(factorization, fixSecondVar);

            main: ;
        }
    }

    private static MultivariatePolynomial<E> swapSecondVar<E>(MultivariatePolynomial<E> initialPoly, int fixSecondVar)
    {
        if (fixSecondVar == -1)
            return initialPoly;
        else
            return MultivariatePolynomial<E>.SwapVariables(initialPoly, 1, fixSecondVar + 2);
    }

    private static PolynomialFactorDecomposition<MultivariatePolynomial<E>> swapSecondVar<E>(
        PolynomialFactorDecomposition<MultivariatePolynomial<E>> factors, int fixSecondVar)
    {
        if (fixSecondVar == -1)
            return factors;
        else
            return factors.MapTo(p => MultivariatePolynomial<E>.SwapVariables(p, 1, fixSecondVar + 2));
    }

    private static bool isSmallCharacteristics<E>(MultivariatePolynomial<E> poly)
    {
        BigInteger ch = poly.CoefficientRingCharacteristic();
        Debug.Assert(!ch.IsZero);
        return ch.GetBitLength() <= 5;
    }

    private static void toCanonicalSort<E>(PolynomialFactorDecomposition<MultivariatePolynomial<E>> biFactors,
        HenselLifting.IEvaluation<E> evaluation)
    {
        // assertion removed since monomials may occur in factorization e.g/ (b)^2 * (a+b) * ...
        //assert biFactors.exponents.sum() == biFactors.size();

        var uFactorsArray = biFactors.MapTo(p => evaluation.evaluateFrom(p, 1).AsUnivariate())
            .ReduceUnitContent().ToArrayWithoutUnit();
        var biFactorsArray = biFactors.ToArrayWithoutUnit();
        Array.Sort(uFactorsArray, biFactorsArray);

        biFactors.Factors.Clear();
        biFactors.Factors.AddRange(biFactorsArray.ToList());
    }


    public static IEvaluationLoop<T> getEvaluationsGF<T>(MultivariatePolynomial<T> factory)
    {
        return new EvaluationLoop<T>(factory);
    }

    public interface IEvaluationLoop<T>
    {
        HenselLifting.Evaluation<T> next();
    }

    /** number of attempts to generate unique evaluation before switching to extension field */
    private const int N_DIFF_EVALUATIONS_FAIL = 32;

    sealed class EvaluationLoop<E> : IEvaluationLoop<E>
    {
        readonly MultivariatePolynomial<E> factory;
        readonly Random rnd = PrivateRandom.GetRandom();
        readonly HashSet<ArrayRef<E>> tried = new HashSet<ArrayRef<E>>();

        public EvaluationLoop(MultivariatePolynomial<E> factory)
        {
            this.factory = factory;
        }

        public HenselLifting.Evaluation<E> next()
        {
            E[] point = new E[factory.nVariables - 1];
            ArrayRef<E> array = new ArrayRef<E>(point);
            int tries = 0;
            do
            {
                if (tries > N_DIFF_EVALUATIONS_FAIL)
                    return null;
                for (int i = 0; i < point.Length; i++)
                    point[i] = factory.ring.RandomElement(rnd);
                ++tries;
            } while (tried.Contains(array));

            tried.Add(array);
            return new HenselLifting.Evaluation<E>(factory.nVariables, point, factory.ring, factory.ordering);
        }
    }

    private sealed class ArrayRef<T>
    {
        readonly T[] data;

        public ArrayRef(T[] data)
        {
            this.data = data;
        }

        public bool equals(Object o)
        {
            if (this == o) return true;
            return !(o == null || GetType() != o.GetType())
                   && Array.Equals(data, ((ArrayRef<T>)o).data);
        }

        public override int GetHashCode()
        {
            return data.GetHashCode();
        }
    }

    static void GCDFreeBasis<Poly>(PolynomialFactorDecomposition<Poly>[] decompositions) where Poly : Polynomial<Poly>
    {
        List<FactorRef<Poly>> allFactors = new List<FactorRef<Poly>>();
        foreach (PolynomialFactorDecomposition<Poly> decomposition in decompositions)
            for (int j = 0; j < decomposition.Count; j++)
                allFactors.Add(new FactorRef<Poly>(decomposition, j));

        for (int i = 0; i < allFactors.Count - 1; i++)
        {
            for (int j = i + 1; j < allFactors.Count; j++)
            {
                FactorRef<Poly>
                    a = allFactors[i],
                    b = allFactors[j];
                if (a == null || b == null)
                    continue;

                Poly gcd = PolynomialMethods.PolynomialGCD(a.factor(), b.factor());
                if (gcd.IsConstant())
                    continue;

                Poly
                    aReduced = PolynomialMethods.DivideExact(a.factor(), gcd),
                    bReduced = PolynomialMethods.DivideExact(b.factor(), gcd);

                if (bReduced.IsConstant())
                    allFactors[j] = null;

                List<int> aGCDIndexes = a.update(aReduced, gcd);
                List<int> bGCDIndexes = b.update(bReduced, gcd);

                FactorRef<Poly> gcdRef = new FactorRef<Poly>();
                gcdRef.decompositions.AddRange(a.decompositions);
                gcdRef.indexes.AddRange(aGCDIndexes);
                gcdRef.decompositions.AddRange(b.decompositions);
                gcdRef.indexes.AddRange(bGCDIndexes);

                allFactors.Add(gcdRef);
            }
        }

        decompositions.ForEach(el => MultivariateFactorization.normalizeGCDFreeDecomposition(el));
    }

    private static void normalizeGCDFreeDecomposition<Poly>(PolynomialFactorDecomposition<Poly> decomposition)
        where Poly : Polynomial<Poly>
    {
        for (int i = decomposition.Factors.Count - 1; i >= 0; --i)
        {
            Poly factor = decomposition.Factors[i].Clone();
            Poly Content = factor.IsOverField() ? factor.LcAsPoly() : factor.ContentAsPoly();
            decomposition.AddUnit(PolynomialMethods.PolyPow(Content, decomposition.Exponents[i], false));
            factor = factor.DivideByLC(Content);
            Debug.Assert(factor != null);


            if (factor.IsOne())
            {
                decomposition.Factors.RemoveAt(i);
                decomposition.Exponents.RemoveAt(i);
                continue;
            }

            decomposition.Factors[i] = factor;

            for (int j = i + 1; j < decomposition.Count; j++)
            {
                if (decomposition.Factors[j].Equals(factor))
                {
                    decomposition.Exponents[i] = decomposition.Exponents[j] + decomposition.Exponents[i];
                    decomposition.Factors.RemoveAt(j);
                    decomposition.Exponents.RemoveAt(j);
                    goto main;
                }
            }

            main: ;
        }
    }

    private sealed class FactorRef<Poly> where Poly : Polynomial<Poly>
    {
        public readonly List<PolynomialFactorDecomposition<Poly>> decompositions;
        public readonly List<int> indexes;

        public FactorRef()
        {
            this.decompositions = [];
            this.indexes = new List<int>();
        }

        public FactorRef(PolynomialFactorDecomposition<Poly> decomposition, int index)
        {
            this.decompositions = new List<PolynomialFactorDecomposition<Poly>>();
            this.indexes = new List<int>();
            decompositions.Add(decomposition);
            indexes.Add(index);
        }

        public Poly factor()
        {
            return decompositions[0].Factors[indexes[0]];
        }

        public List<int> update(Poly reduced, Poly gcd)
        {
            List<int> gcdIndexes = new List<int>(indexes.Count);
            // add gcd to all required decompositions
            for (int i = 0; i < decompositions.Count; i++)
            {
                PolynomialFactorDecomposition<Poly> decomposition = decompositions[i];
                decomposition.Factors[indexes[i]] = reduced; // <- just in case
                gcdIndexes.Add(decomposition.Count);
                decomposition.AddFactor(gcd, decomposition.Exponents[indexes[i]]);
            }

            return gcdIndexes;
        }
    }

    /* ===================================== Multivariate factorization over Z ====================================== */

    /** specialized evaluations which tries small integers first */
    sealed class EvaluationLoopZ : IEvaluationLoop<BigInteger>
    {
        readonly MultivariatePolynomial<BigInteger> factory;
        readonly Random rnd = PrivateRandom.GetRandom();
        readonly HashSet<ArrayRef<BigInteger>> tried = new HashSet<ArrayRef<BigInteger>>();

        public EvaluationLoopZ(MultivariatePolynomial<BigInteger> factory)
        {
            this.factory = factory;
        }

        private int counter = 0;

        public HenselLifting.Evaluation<BigInteger> next()
        {
            BigInteger[] point = new BigInteger[factory.nVariables - 1];
            ArrayRef<BigInteger> array = new ArrayRef<BigInteger>(point);
            int tries = 0;
            do
            {
                if (tries > N_DIFF_EVALUATIONS_FAIL)
                {
                    counter += 5;
                    return next();
                }

                for (int i = 0; i < point.Length; i++)
                    point[i] = new BigInteger(rnd.Next(10 * (counter / 5 + 1)));
                ++tries;
            } while (tried.Contains(array));

            tried.Add(array);
            ++counter;
            return new HenselLifting.Evaluation<BigInteger>(factory.nVariables, point, factory.ring, factory.ordering);
        }
    }


    static PolynomialFactorDecomposition<MultivariatePolynomial<BigInteger>> factorPrimitiveInZ(
        MultivariatePolynomial<BigInteger> polynomial)
    {
        if (polynomial.IsEffectiveUnivariate())
            return factorUnivariate(polynomial);

        // order the polynomial by degrees
        var input = orderByDegrees(polynomial, true, true, -1);
        PolynomialFactorDecomposition<MultivariatePolynomial<BigInteger>> decomposition =
            factorPrimitiveInZ0(input.ordered);
        if (decomposition == null)
            return null;
        return decomposition.MapTo(input.restoreOrder);
    }


    static PolynomialFactorDecomposition<MultivariatePolynomial<BigInteger>> factorPrimitiveInZ0(
        MultivariatePolynomial<BigInteger> poly)
    {
        // assert that poly is at least bivariate
        Debug.Assert(poly.NUsedVariables() >= 2);
        // assert that degrees of variables are in the descending order
        Debug.Assert(poly.Degree(1) > 0 && poly.Degree(0) > 0);
        // assert poly is primitive
        Debug.Assert(poly.Content().IsOne);

        if (poly.NUsedVariables() == 2)
            // bivariate case
            return bivariateDenseFactorSquareFreeInZ(poly);

        // the leading coefficient
        MultivariatePolynomial<BigInteger> lc = poly.Lc(0);
        BigInteger lcContent = lc.Content();
        MultivariatePolynomial<BigInteger> lcPrimitive = lc.Clone().DivideOrNull(lcContent);

        LeadingCoefficientData<BigInteger> lcData = new LeadingCoefficientData<BigInteger>(lcPrimitive);
        // for characteristic 0 l.c. can be always fully reconstructed
        Debug.Assert(lcData.fullyReconstructable);

        // coefficients bound
        BigInteger bound2 = coefficientsBound(poly) * coefficientsBound(lc) << 1;

        IEvaluationLoop<BigInteger> evaluations = new EvaluationLoopZ(poly); //getEvaluationsGF(poly);

        // maximal number of bivariate factors
        int nBivariateFactors = int.MaxValue;

        while (true)
        {
            // choose next evaluation
            HenselLifting.Evaluation<BigInteger> evaluation = (HenselLifting.Evaluation<BigInteger>)evaluations.next();

            if (evaluation == null /*|| (nAttempts++ > N_FAILS_BEFORE_SWITCH_TO_EXTENSION && isSmallCardinality )*/)
            {
                // <= not possible to reach this point
                throw new Exception();
            }

            // check that evaluation does not change the rest degrees
            MultivariatePolynomial<BigInteger>[] images = poly.CreateArray(poly.nVariables - 1);
            for (int i = 0; i < images.Length; i++)
            {
                int variable = poly.nVariables - i - 1;
                images[i] = evaluation.evaluate(i == 0 ? poly : images[i - 1], variable);
                if (images[i].Degree(variable - 1) != poly.Degree(variable - 1))
                    goto main;
            }

            MultivariatePolynomial<BigInteger>
                bivariateImage = images[images.Length - 2],
                univariateImage = images[images.Length - 1];

            Debug.Assert(bivariateImage.Degree(0) == poly.Degree(0) && bivariateImage.Degree(1) == poly.Degree(1));

            // check that l.c. of bivariate image has same degree in second variable
            // as the original l.c.
            if (lc.Degree(1) != bivariateImage.Lc(0).Degree(1))
                continue;

            // check that univariate image is also square-free
            if (!UnivariateSquareFreeFactorization.IsSquareFree(univariateImage.AsUnivariate()))
                continue;

            // check that bivariate image is also primitive
            if (!bivariateImage.ContentUnivariate(1).IsConstant())
                continue;

            // factor bivariate image
            PolynomialFactorDecomposition<MultivariatePolynomial<BigInteger>> biFactorsMain =
                bivariateDenseFactorSquareFreeInZ(bivariateImage);

            if (biFactorsMain.Count == 1)
                return PolynomialFactorDecomposition<MultivariatePolynomial<BigInteger>>.Of(poly);

            if (biFactorsMain.Count > nBivariateFactors)
                // bad evaluation
                continue;

            nBivariateFactors = biFactorsMain.Count;

            // choose prime Power p**k > bound2
            int basePrime = 1 << 22;
            BigInteger bBasePrime;
            while (true)
            {
                basePrime = SmallPrimes.NextPrime(basePrime);
                bBasePrime = new BigInteger(basePrime);
                if (!isGoodPrime(bBasePrime, univariateImage.Lc(), univariateImage.Cc()))
                    continue;

                IntegersZp moduloDomain = new IntegersZp(bBasePrime);
                // ensure that univariate factors are still co-prime
                // todo: do we really need this check?
                if (!PolynomialMethods.CoprimeQ(biFactorsMain.MapTo(f => f.SetRing(moduloDomain)).Factors))
                    continue;

                break;
            }

            BigInteger modulus = bBasePrime;
            while (modulus.CompareTo(bound2) < 0)
                modulus *= bBasePrime;
            IntegersZp zpDomain = new IntegersZp(modulus);

            // evaluation for Zp ring
            HenselLifting.Evaluation<BigInteger> evaluationZp = evaluation.setRing(zpDomain);

            // array of bivariate factors for lifting
            // (polynomials in F[x1, x2])
            MultivariatePolynomial<BigInteger>[] biFactorsArrayMainZ;
            if (!lc.IsConstant())
            {
                // <= leading coefficients reconstruction

                // bring main bivariate factorization in canonical order
                // (required for one-to-one correspondence between different bivariate factorizations)
                toCanonicalSort(biFactorsMain, evaluation);
                biFactorsArrayMainZ = biFactorsMain.Factors.ToArray();

                // the rest of l.c. (lc/lcFactors), will be constant at the end
                MultivariatePolynomial<BigInteger> lcRest = lc.Clone();
                // the true leading coefficients (to be calculated)
                MultivariatePolynomial<BigInteger>[] lcFactors = poly.CreateArray(biFactorsMain.Count);
                // initialize lcFactors with constants (correct ones!)
                for (int i = 0; i < lcFactors.Length; i++)
                    lcFactors[i] = poly.CreateOne();

                // we perform additional bivariate factorizations in F[x1, x_i] for i = (3,..., N) (in special order)
                lc_reconstruction:
                for (int i = 0; i < lcData.lcSplits.Length && !lcRest.IsConstant(); i++)
                {
                    SplitContent<BigInteger> lcSplit = lcData.lcSplits[i];
                    // x_i -- the variable to leave unevaluated in addition to the main variable x_1
                    // (+1 required to obtain indexing as in the original poly)
                    int freeVariable = 1 + lcSplit.variable;

                    HenselLifting.Evaluation<BigInteger>
                        // original evaluation with shuffled variables
                        iEvaluation = evaluation.renameVariables(lcSplit.ppOrdered.variablesSorted),
                        // the evaluation for l.c. (x1 dropped)
                        ilcEvaluation = iEvaluation.dropVariable(1);

                    // target for lifting
                    MultivariatePolynomial<BigInteger> ppPart = lcSplit.ppOrdered.ordered;
                    if (!UnivariateSquareFreeFactorization.IsSquareFree(ilcEvaluation.evaluateFrom(ppPart, 1)
                            .AsUnivariate()))
                    {
                        // univariate image may be non square-free for because of bad evaluation (for characteristic 0)
                        // <= try another evaluation
                        goto main;
                    }

                    // bivariate factors in F[x1, x_i]
                    PolynomialFactorDecomposition<MultivariatePolynomial<BigInteger>> biFactors;
                    if (freeVariable == 1)
                        biFactors = biFactorsMain;
                    else
                    {
                        // good image must have
                        //  - the same degree in the main variable
                        //  - the same degree in freeVariable
                        //  - l.c. with the same degree in freeVariable
                        MultivariatePolynomial<BigInteger> biImage =
                            evaluation.evaluateFromExcept(poly, 1, freeVariable);
                        if (biImage.Degree(0) != poly.Degree(0)
                            || biImage.Degree(freeVariable) != poly.Degree(freeVariable)
                            || biImage.Lc(0).Degree(freeVariable) != lc.Degree(freeVariable))
                            goto main;

                        // bivariate factors in F[x1, x_i]
                        biFactors = bivariateDenseFactorSquareFreeInZ(
                            orderByDegrees(biImage, false, -1).ordered);
                        // bring in one-to-one correspondence with biFactorsMain
                        toCanonicalSort(biFactors, iEvaluation);
                    }

                    if (biFactors.Count != biFactorsMain.Count)
                    {
                        // number of factors should be the same since polynomial is primitive
                        // => bad evaluation occurred
                        nBivariateFactors = Math.Min(biFactors.Count, biFactorsMain.Count);
                        goto main;
                    }

                    // Debug.Assert(biFactors
                    //         .MapTo(p => iEvaluation.evaluateFrom(p, 1).AsUnivariate()).Primitive().Canonical()
                    //         .Equals(biFactorsMain.MapTo(p => evaluation.evaluateFrom(p, 1).AsUnivariate()).Primitive()
                    //             .Canonical())
                    //     : poly.ToString());

                    // square-free decomposition of the leading coefficients of bivariate factors
                    var ulcFactors = biFactors.Factors.Select(f =>
                            UnivariateSquareFreeFactorization.SquareFreeFactorization(f.Lc(0).AsUnivariate()))
                        .ToArray();

                    // move to GCD-free basis of sq.-f. decomposition (univariate, because fast)
                    GCDFreeBasis(ulcFactors);

                    // map to multivariate factors for further Hensel lifting
                    PolynomialFactorDecomposition<MultivariatePolynomial<BigInteger>>[]
                        ilcFactors = ulcFactors
                            .Select(decomposition => decomposition.MapTo(p =>
                                MultivariatePolynomial<BigInteger>.AsMultivariate(p, poly.nVariables - 1, 0,
                                    poly.ordering)))
                            .ToArray();


                    // <- set same polys in ulcFactors with the same single reference!
                    // NOTE: this is very important since we will use polynomials as references
                    // when doing lifting!
                    //
                    // comment: normally in most cases this is done automatically by GCDFreeBasis routine
                    // but in some cases (symmetric polynomials) this is not the case and manual correction required
                    for (int l = 0; l < ilcFactors.Length; l++)
                    for (int m = 0; m < ilcFactors[l].Factors.Count; m++)
                    {
                        MultivariatePolynomial<BigInteger> p = ilcFactors[l].Factors[m];
                        for (int l1 = l; l1 < ilcFactors.Length; l1++)
                        {
                            int m1Begin = l1 == l ? m + 1 : 0;
                            for (int m1 = m1Begin; m1 < ilcFactors[l1].Factors.Count; m1++)
                                if (ilcFactors[l1].Factors[m1].Equals(p))
                                    ilcFactors[l1].Factors[m1] = p;
                        }
                    }

                    // pick unique factors from lc decompositions (complete square-free)
                    HashSet<MultivariatePolynomial<BigInteger>> ilcFactorsSet = ilcFactors
                        .SelectMany(dec => dec.StreamWithoutUnit()).ToHashSet();
                    MultivariatePolynomial<BigInteger>[] ilcFactorsSqFree = ilcFactorsSet
                        .ToArray();

                    Debug.Assert(ilcFactorsSqFree.Length > 0);
                    Debug.Assert(!ilcFactorsSqFree.Any(m => m.IsConstant()));

                    // the sum of degrees of all unique factors in univariate gcd-free decomposition
                    // must be equal to the degree of primitive part we want to lift to
                    Debug.Assert(ilcFactorsSqFree.Select(e => e.Degree()).Aggregate(0, (a, b) => a + b)
                                 == ppPart.Degree(0));

                    // we need to correct lcSqFreePrimitive (obtain correct numerical l.c.)
                    MultivariatePolynomial<BigInteger> ppPartLC = ilcEvaluation.evaluateFrom(ppPart.Lc(0), 1);
                    MultivariatePolynomial<BigInteger> realLC = ilcFactorsSqFree
                        .Select(e => e.LcAsPoly())
                        .Aggregate(ilcFactorsSqFree[0].CreateOne(), (a, b) => a.Multiply(b));

                    Debug.Assert(ppPartLC.IsConstant());
                    Debug.Assert(realLC.IsConstant());

                    MultivariatePolynomial<BigInteger> @base_ = ppPart.Clone();
                    BigInteger baseDivide = BigInteger.One;
                    if (!realLC.Cc().Equals(ppPartLC.Cc()))
                    {
                        BigInteger
                            lcm = Rings.Z.Lcm(realLC.Cc(), ppPartLC.Cc()),
                            factorCorrection = lcm / realLC.Cc(),
                            baseCorrection = lcm / ppPartLC.Cc();
                        @base_ = @base_.Multiply(baseCorrection);
                        baseDivide = baseDivide * factorCorrection;
                    }

                    if (!baseDivide.IsOne)
                        adjustConstants(baseDivide, @base_, ilcFactorsSqFree, null);

                    if (ilcFactorsSqFree.Length == 1)
                        ilcFactorsSqFree[0].Set(@base_);
                    else
                    {
                        var ilcFactorsSqFreeZp = ilcFactorsSqFree
                            .Select(f => f.SetRing(zpDomain))
                            .ToArray();

                        // <= lifting leading coefficients
                        HenselLifting.multivariateLiftAutomaticLC(@base_.SetRing(zpDomain), ilcFactorsSqFreeZp,
                            ilcEvaluation.setRing(zpDomain));

                        for (int j = 0; j < ilcFactorsSqFreeZp.Length; j++)
                            ilcFactorsSqFree[j].Set(MultivariatePolynomial<BigInteger>
                                .AsPolyZSymmetric(ilcFactorsSqFreeZp[j]).PrimitivePart());
                    }

                    //assert Multiply(ilcFactorsSqFree).monic().equals(base.Clone().monic());

                    // l.c. has Content in x2
                    for (int jFactor = 0; jFactor < lcFactors.Length; jFactor++)
                    {
                        MultivariatePolynomial<BigInteger> obtainedLcFactor = MultivariatePolynomial<BigInteger>
                            .RenameVariables(
                                ilcFactors[jFactor].Multiply(), lcSplit.ppOrdered.variablesMapping)
                            .InsertVariable(0);
                        MultivariatePolynomial<BigInteger> commonPart =
                            MultivariateGCD.PolynomialGCD(obtainedLcFactor, lcFactors[jFactor]);
                        MultivariatePolynomial<BigInteger> addon =
                            MultivariateDivision.DivideExact(obtainedLcFactor, commonPart);
                        // make addon monic when evaluated with evaluation
                        addon = addon.PrimitivePart();
                        lcFactors[jFactor] = lcFactors[jFactor].Multiply(addon);
                        lcRest = MultivariateDivision.DivideExact(lcRest, addon);
                    }
                }

                Debug.Assert(lcRest.IsConstant());

                //BigInteger biFactorsCF = biFactorsMain.constantFactor.cc();
                for (int i = 0; i < lcFactors.Length; i++)
                {
                    Debug.Assert(evaluation.evaluateFrom(biFactorsArrayMainZ[i].Lc(0), 1).IsConstant());
                    Debug.Assert(evaluation.evaluateFrom(lcFactors[i], 1).IsConstant());

                    BigInteger
                        lcInMain = evaluation.evaluateFrom(biFactorsArrayMainZ[i].Lc(0), 1).Cc(),
                        lcTrue = evaluation.evaluateFrom(lcFactors[i], 1).Cc();

                    if (!lcInMain.Equals(lcTrue))
                    {
                        BigInteger
                            lcm = Rings.Z.Lcm(lcInMain, lcTrue),
                            factorCorrection = lcm / lcInMain,
                            lcCorrection = lcm / lcTrue;

                        biFactorsArrayMainZ[i].Multiply(factorCorrection);
                        //biFactorsCF = biFactorsCF.DivideExact(factorCorrection);
                        lcFactors[i].Multiply(lcCorrection);

                        lcRest = lcRest.DivideOrNull(lcCorrection);
                        Debug.Assert(lcRest != null);
                    }
                }

                // switch to Z/p and lift

                MultivariatePolynomial<BigInteger> @base = poly.Clone();
                if (!lcRest.IsOne())
                    adjustConstants(lcRest.Cc(), @base, biFactorsArrayMainZ, lcFactors);

                @base = @base.SetRing(zpDomain);
                biFactorsArrayMainZ = liftZ(@base, zpDomain, evaluationZp, biFactorsArrayMainZ, lcFactors);
            }
            else
            {
                // switch to Z/p and lift
                MultivariatePolynomial<BigInteger> @base = poly.SetRing(zpDomain);
                if (!biFactorsMain.Unit.IsOne())
                {
                    BigInteger correction = biFactorsMain.Unit.Lc();
                    @base.Multiply(zpDomain.Pow(correction, biFactorsMain.Count - 1));
                    foreach (MultivariatePolynomial<BigInteger> f in biFactorsMain.Factors)
                        f.Multiply(correction);
                }

                biFactorsArrayMainZ = liftZ(@base, zpDomain, evaluationZp,
                    biFactorsMain.Factors.ToArray(), null);
            }

            PolynomialFactorDecomposition<MultivariatePolynomial<BigInteger>> factorization
                = PolynomialFactorDecomposition<MultivariatePolynomial<BigInteger>>.Of(biFactorsArrayMainZ.ToList())
                    .Primitive();

            if (factorization.Signum() != poly.SignumOfLC())
                factorization = factorization.AddUnit(poly.CreateOne().Negate());

            MultivariatePolynomial<BigInteger>
                lcNumeric = factorization.Factors.Aggregate(factorization.Unit.Clone(),
                    (a, b) => a.LcAsPoly().Multiply(b.LcAsPoly())),
                ccNumeric = factorization.Factors.Aggregate(factorization.Unit.Clone(),
                    (a, b) => a.CcAsPoly().Multiply(b.CcAsPoly()));
            if (!lcNumeric.Equals(poly.LcAsPoly()) || !ccNumeric.Equals(poly.CcAsPoly()) ||
                !factorization.Multiply().Equals(poly))
            {
                // bad bivariate factorization => recombination required
                // instead of recombination we try again with another evaluation
                // searching for good enough bivariate factorization
                nBivariateFactors = factorization.Count - 1;
                continue;
            }

            return factorization.Primitive();
            main: ;
        }
    }

    private static MultivariatePolynomial<BigInteger>[] liftZ(MultivariatePolynomial<BigInteger> @base,
        IntegersZp zpDomain, HenselLifting.Evaluation<BigInteger> evaluationZp,
        MultivariatePolynomial<BigInteger>[] biFactorsArrayMainZ, MultivariatePolynomial<BigInteger>[] lcFactors)
    {
        biFactorsArrayMainZ = biFactorsArrayMainZ.Select(f => f.SetRing(zpDomain)).ToArray();
        if (lcFactors != null)
            lcFactors = lcFactors.Select(f => f.SetRing(zpDomain)).ToArray();

        HenselLifting.multivariateLift0(@base, biFactorsArrayMainZ, lcFactors, evaluationZp, @base.Degrees(), 2);

        for (int i = 0; i < biFactorsArrayMainZ.Length; i++)
            biFactorsArrayMainZ[i] = MultivariatePolynomial<BigInteger>.AsPolyZSymmetric(biFactorsArrayMainZ[i])
                .PrimitivePart();
        return biFactorsArrayMainZ;
    }

    private static void adjustConstants(BigInteger constant, MultivariatePolynomial<BigInteger> @base,
        MultivariatePolynomial<BigInteger>[] factors,
        MultivariatePolynomial<BigInteger>[] lcs)
    {
        @base.Multiply(Rings.Z.Pow(constant, factors.Length - 1));
        foreach (MultivariatePolynomial<BigInteger> factor in factors)
            factor.Multiply(constant);
        if (lcs != null)
            foreach (MultivariatePolynomial<BigInteger> factor in lcs)
                factor.Multiply(constant);
    }


    /* =========================== Multivariate factorization over simple number fields ============================ */

    // static PolynomialFactorDecomposition<MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>>
    // factorPrimitiveInNumberField(MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> poly) {
    //     AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> numberField
    //             = (AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>>) poly.ring;
    //     int[] variables = ArraysUtil.sequence(0, poly.nVariables);
    //     ArraysUtil.quickSort(poly.degrees(), variables);
    //
    //     for (int s = 0; ; ++s) {
    //         for (int variable : variables) {
    //             if (poly.Degree(variable) == 0)
    //                 continue;
    //             // choose a substitution f(z) => f(z - s*alpha) so that norm is square-free
    //             MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>
    //                     backSubstitution, sPoly;
    //             if (s == 0) {
    //                 backSubstitution = null;
    //                 sPoly = poly;
    //             } else {
    //                 sPoly = poly.composition(variable, poly.createMonomial(variable, 1).subtract(numberField.generator().Multiply(s)));
    //                 backSubstitution = poly.createMonomial(variable, 1).add(numberField.generator().Multiply(s));
    //             }
    //
    //             MultivariatePolynomial<Rational<BigInteger>> sPolyNorm = numberField.normOfPolynomial(sPoly);
    //             if (!MultivariateSquareFreeFactorization.isSquareFree(sPolyNorm))
    //                 continue;
    //
    //             // factorize norm
    //             PolynomialFactorDecomposition<MultivariatePolynomial<Rational<BigInteger>>> normFactors = Factor(sPolyNorm);
    //             if (normFactors.isTrivial())
    //                 return PolynomialFactorDecomposition.of(poly);
    //
    //             PolynomialFactorDecomposition<MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>>
    //                     result = PolynomialFactorDecomposition.Empty(poly);
    //
    //             for (int i = 0; i < normFactors.size(); i++) {
    //                 assert normFactors.getExponent(i) == 1;
    //                 MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> factor =
    //                         MultivariateGCD.PolynomialGCD(sPoly, toNumberField(numberField, normFactors.get(i)));
    //                 if (backSubstitution != null)
    //                     factor = factor.composition(variable, backSubstitution);
    //                 result.AddFactor(factor, 1);
    //             }
    //
    //             if (result.isTrivial())
    //                 return PolynomialFactorDecomposition.of(poly);
    //
    //             // correct unit
    //             return result.setLcFrom(poly);
    //         }
    //     }
    // }

    // private static MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>
    // toNumberField(AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> numberField,
    //               MultivariatePolynomial<Rational<BigInteger>> poly) {
    //     return poly.mapCoefficients(numberField, cf => UnivariatePolynomial.constant(Rings.Q, cf));
    // }
}