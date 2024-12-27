using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Polynomials.Linear;
using Polynomials.Poly.Univar;
using Polynomials.Primes;
using Polynomials.Utils;
using UnivariatePolynomialZp64 = Polynomials.Poly.Univar.UnivariatePolynomial<long>;
using MultivariatePolynomialZp64 = Polynomials.Poly.Multivar.MultivariatePolynomial<long>;
using static Polynomials.Poly.Multivar.Conversions64bit;

namespace Polynomials.Poly.Multivar;

public static class MultivariateGCD
{
    public static MultivariatePolynomial<E> PolynomialGCD<E>(params MultivariatePolynomial<E>[] arr)
    {
        return PolynomialGCD(arr, MultivariateGCD.PolynomialGCD);
    }

    public static MultivariatePolynomial<E> PolynomialGCD<E>(MultivariatePolynomial<E> poly, MultivariatePolynomial<E>[] arr)
    {
        var all = new MultivariatePolynomial<E>[arr.Length + 1];
        all[0] = poly;
        Array.Copy(arr, 0, all, 1, arr.Length);
        return PolynomialGCD(all);
    }

    public static MultivariatePolynomial<E> PolynomialGCD<E>(IEnumerable<MultivariatePolynomial<E>> arr)
    {
        return PolynomialGCD(arr, MultivariateGCD.PolynomialGCD);
    }

    private static MultivariatePolynomial<E>[] nonZeroElements<E>(MultivariatePolynomial<E>[] arr)
    {
        var res = new List<MultivariatePolynomial<E>>(arr.Length);
        foreach (var el in arr)
            if (!el.IsZero())
                res.Add(el);
        if (res.Count == 0)
            res.Add(arr[0]);
        return res.ToArray();
    }

    static MultivariatePolynomial<E> PolynomialGCD<E>(MultivariatePolynomial<E>[] arr,
        Func<MultivariatePolynomial<E>, MultivariatePolynomial<E>, MultivariatePolynomial<E>> algorithm)
    {
        arr = nonZeroElements(arr);
        Debug.Assert(arr.Length > 0);

        if (arr.Length == 1)
            return arr[0];

        if (arr.Length == 2)
            return algorithm(arr[0], arr[1]);

        // quick checks
        foreach (var p in arr)
        {
            if (p.IsConstant())
                return ContentGCD(arr);
            if (p.IsMonomial())
            {
                var monomial = p.Lt();
                foreach (var el in arr)
                    monomial = el.CommonContent(monomial);
                return arr[0].Create(monomial).MonicWithLC(ContentGCD(arr));
            }
        }

        // <- choosing strategy of gcd

        // first sort polys by "sparsity"
        Array.Sort(arr, (a, b) => a.Size().CompareTo(b.Size()));
        int
            minSize = arr[0].Size(),
            maxSize = arr[arr.Length - 1].Size();

        if (maxSize / minSize > 20)
        {
            int split = 1;
            for (; split < arr.Length && arr[split].Size() < maxSize / 3; ++split) ;
            // use "divide and conqueror" strategy
            if (split > 1)
            {
                var smallGCD = PolynomialGCD(arr[..split], algorithm);
                var rest = new MultivariatePolynomial<E>[arr.Length - split + 1];
                rest[0] = smallGCD;
                Array.Copy(arr, split, rest, 1, arr.Length - split);

                return PolynomialGCD(rest, algorithm);
            }
        }

        //choose poly of minimal total Degree
        int iMin = 0, degSum = arr[0].DegreeSum();
        for (int i = 1; i < arr.Length; ++i)
        {
            int t = arr[i].DegreeSum();
            if (t < degSum)
            {
                iMin = i;
                degSum = t;
            }
        }

        Utils.Utils.Swap(arr, 0, iMin);
        MultivariatePolynomial<E>
            // the base (minimal total Degree)
            @base = arr[0],
            // sum of other polynomials
            sum = arr[1].Clone();

        List<MultivariatePolynomial<E>>
            // the list of polys which we do not put in the sum b
            polysNotInSum = new List<MultivariatePolynomial<E>>(),
            // the list of polys which we put in the sum b
            polysInSum = new List<MultivariatePolynomial<E>>();

        polysInSum.Add(arr[1]);

        Random rnd = PrivateRandom.GetRandom();
        int[] sumDegrees = sum.DegreesRef();
        int nFails = 0; // #tries to add a poly
        for (int i = 2; i < arr.Length; i++)
        {
            MultivariatePolynomial<E> tmp;
            do
            {
                tmp = arr[i].Clone().Multiply(rnd.Next(2048)); 
            } while (tmp.IsZero());

            bool shouldHaveCC = !arr[i].CcAsPoly().IsZero() || !sum.CcAsPoly().IsZero();
            int[] expectedDegrees = Utils.Utils.Max(sumDegrees, tmp.DegreesRef());
            sum = sum.Add(tmp);
            if (!Enumerable.SequenceEqual(expectedDegrees, sum.DegreesRef()) ||
                (shouldHaveCC && sum.CcAsPoly().IsZero()))
            {
                // adding of a non-zero factor reduced the Degree of the result
                // the common reason is that the cardinality is very small (e.g. 2, so that x + x = 0)
                if (nFails == 2)
                {
                    nFails = 0;
                    polysNotInSum.Add(arr[i]);
                }
                else
                {
                    // try once more
                    ++nFails;
                    --i;
                }

                sum = sum.Subtract(tmp);
            }
            else
            {
                polysInSum.Add(arr[i]);
                sumDegrees = expectedDegrees;
            }
        }

        Debug.Assert(polysInSum.Count + polysNotInSum.Count + 1 == arr.Length);

        var gcd = algorithm(@base, sum);
        if (gcd.IsConstant())
        {
            // Content gcd
            for (int i = 1; i < arr.Length; i++)
                gcd = algorithm(gcd, arr[i].ContentAsPoly());
            Utils.Utils.Swap(arr, 0, iMin); // <-restore
            return gcd;
        }

        foreach (var notInSum in polysNotInSum)
            gcd = algorithm(gcd, notInSum);

        var remainders = new List<MultivariatePolynomial<E>>();
        foreach (var inSum in polysInSum)
            if (!MultivariateDivision.DividesQ(inSum, gcd))
                remainders.Add(inSum);

        if (remainders.Count != 0)
            foreach (var remainder in remainders)
                gcd = algorithm(gcd, remainder);

        Utils.Utils.Swap(arr, 0, iMin); // <-restore
        return gcd;
    }

    private static MultivariatePolynomial<E> ContentGCD<E>(MultivariatePolynomial<E>[] arr)
    {
        if (arr[0].IsOverField())
            return arr[0].CreateOne();
        return ContentGCD0(arr);
    }

    private static MultivariatePolynomial<E> ContentGCD0<E>(MultivariatePolynomial<E>[] arr)
    {
        MultivariatePolynomial<E> factory = arr[0];
        return factory.CreateConstant(factory.ring.Gcd(arr.Select(m => m.Content()).ToList()));
    }

    static MultivariatePolynomial<E> PolynomialGCD<E>(IEnumerable<MultivariatePolynomial<E>> arr,
        Func<MultivariatePolynomial<E>, MultivariatePolynomial<E>, MultivariatePolynomial<E>> algorithm)
    {
        using var iterator = arr.GetEnumerator();
        var list = new List<MultivariatePolynomial<E>>();
        while (iterator.MoveNext())
            list.Add(iterator.Current);
        if (list.Count == 0)
            throw new ArgumentException("Empty iterable");
        return PolynomialGCD(list.ToArray(), algorithm);
    }

    public static MultivariatePolynomial<E> PolynomialGCD<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        a.AssertSameCoefficientRingWith(b);
        if (a.IsOverFiniteField())
            return ensureMonicOverGF(PolynomialGCDinGF(a, b));
        if (a.IsOverZ())
            return PolynomialGCDinZ(
                    a as MultivariatePolynomial<BigInteger>,
                    b as MultivariatePolynomial<BigInteger>)
                as MultivariatePolynomial<E>;
        if (Util.IsOverRationals(a))
            return (MultivariatePolynomial<E>)GenericHandler.InvokeForGeneric<E>(typeof(Rational<>),
                nameof(PolynomialGCD),
                typeof(MultivariateGCD), a, b); // PolynomialGCDInQ( a,  b);
        // if (Util.IsOverSimpleNumberField(a))
        //     return  PolynomialGCDinNumberField( a,  b);
        // if (Util.IsOverRingOfIntegersOfSimpleNumberField(a))
        //     return  PolynomialGCDinRingOfIntegersOfNumberField( a,  b);
        // if (Util.IsOverMultipleFieldExtension(a))
        //     return  PolynomialGCDinMultipleFieldExtension( a,  b);
        var r = tryNested(a, b);
        if (r != null)
            return r;
        if (a.IsOverField())
            return ZippelGCD(a, b);
        throw new Exception();
    }

    public static MultivariatePolynomial<E> PolynomialGCDinGF<E>(MultivariatePolynomial<E> a,
        MultivariatePolynomial<E> b)
    {
        a.AssertSameCoefficientRingWith(b);
        if (!a.IsOverFiniteField())
            throw new ArgumentException();

        // use EEZGCD for dense problems
        if (isDenseGCDProblem(a, b))
            return EEZGCD(a, b, true);

        // use ZippelGCD for sparse problems
        if (a is MultivariatePolynomialZp64)
            return ZippelGCD(a.AsZp64(), b.AsZp64()).AsT<E>();

        return ZippelGCD(a, b);
    }

    public static MultivariatePolynomial<BigInteger> PolynomialGCDinZ(MultivariatePolynomial<BigInteger> a,
        MultivariatePolynomial<BigInteger> b)
    {
        a.AssertSameCoefficientRingWith(b);
        if (!a.IsOverZ())
            throw new ArgumentException();

        if (isDenseGCDProblem(a, b))
            // use EEZGCD with ModularGCD for dense problems
            return ModularGCDInZ(a, b, (u, v) => EEZGCD(u, v, true), true);
        else
            // use ZippelGCD for sparse problems
            return ZippelGCDInZ(a, b);
    }

    // public static MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>
    // PolynomialGCDinNumberField(MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a,
    //                            MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> b) {
    //     a.assertSameCoefficientRingWith(b);
    //     if (isDenseGCDProblem(a, b))
    //         // use EEZGCD with ModularGCD for dense problems
    //         return ModularGCDInNumberFieldViaRationalReconstruction(a, b, (u, v) -> EEZGCD(u, v, true));
    //     else
    //         // use ZippelGCD for sparse problems
    //         return ZippelGCDInNumberFieldViaRationalReconstruction(a, b);
    // }
    // public static MultivariatePolynomial<UnivariatePolynomial<BigInteger>>
    // PolynomialGCDinRingOfIntegersOfNumberField(MultivariatePolynomial<UnivariatePolynomial<BigInteger>> a,
    //                                            MultivariatePolynomial<UnivariatePolynomial<BigInteger>> b) {
    //     a.assertSameCoefficientRingWith(b);
    //     if (!a.Lc().IsConstant() || !b.Lc().IsConstant())
    //         throw new IllegalArgumentException("lc must be constant");
    //     AlgebraicNumberField<UnivariatePolynomial<BigInteger>> ring = (AlgebraicNumberField<UnivariatePolynomial<BigInteger>>) a.ring;
    //     AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> field = new AlgebraicNumberField<>(ring.getMinimalPolynomial().mapCoefficients(Q, Q::mkNumerator));
    //     MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> gcd =
    //             PolynomialGCDinNumberField(
    //                     a.mapCoefficients(field, cf -> cf.mapCoefficients(Q, Q::mkNumerator)),
    //                     b.mapCoefficients(field, cf -> cf.mapCoefficients(Q, Q::mkNumerator)));
    //     return gcd.Multiply(field.valueOfBigInteger(iDenominator(gcd)))
    //             .mapCoefficients(ring, cf -> cf.mapCoefficients(Z, Rational::numeratorExact))
    //             .primitivePart();
    // }

    private const double SPARSITY_THRESHOLD_NVARS_4 = 0.2;
    private const double SPARSITY2_THRESHOLD = 0.5;
    private const int SPARSITY_SIZE_THRESHOLD = 256;

    private static bool isDenseGCDProblem<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        return a.nVariables >= 4
               && a.Size() > SPARSITY_SIZE_THRESHOLD
               && b.Size() > SPARSITY_SIZE_THRESHOLD
               && a.Sparsity2() > SPARSITY2_THRESHOLD
               && b.Sparsity2() > SPARSITY2_THRESHOLD
               && (a.nVariables > 4 || (a.Sparsity() > SPARSITY_THRESHOLD_NVARS_4 &&
                                        b.Sparsity() > SPARSITY_THRESHOLD_NVARS_4));
    }

    private static MultivariatePolynomial<E>? tryNested<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        // TODO see can merge uni uniZp64 and multi multiZp64
        if (isOverUnivariate(a))
            return (MultivariatePolynomial<E>)GenericHandler.InvokeForGeneric<E>(typeof(UnivariatePolynomial<>),
                nameof(PolynomialGCDOverUnivariate), typeof(MultivariateGCD), a,
                b); // PolynomialGCDOverUnivariate(a,  b);
        else if (isOverUnivariateZp64(a))
            return PolynomialGCDOverUnivariateZp64(
                a.AsT<UnivariatePolynomialZp64>(),
                b.AsT<UnivariatePolynomialZp64>()
            ).AsT<E>();
        else if (isOverMultivariate(a))
            return (MultivariatePolynomial<E>)GenericHandler.InvokeForGeneric<E>(typeof(MultivariatePolynomial<>),
                nameof(PolynomialGCDOverMultivariate), typeof(MultivariateGCD), a,
                b); //PolynomialGCDOverMultivariate(a,  b);
        else if (isOverMultivariateZp64(a))
            return PolynomialGCDOverMultivariateZp64(
                a.AsT<MultivariatePolynomialZp64>(),
                b.AsT<MultivariatePolynomialZp64>()
            ).AsT<E>();

        return null;
    }

    public static bool isOverPolynomialRing<E>(MultivariatePolynomial<E> p)
    {
        return isOverUnivariate(p) || isOverUnivariateZp64(p) || isOverMultivariate(p) || isOverMultivariateZp64(p);
    }

    public static bool isOverUnivariate<E>(MultivariatePolynomial<E> p)
    {
        return p.ring is IUnivariateRing;
    }

    private static MultivariatePolynomial<UnivariatePolynomial<E>>
        PolynomialGCDOverUnivariate<E>(MultivariatePolynomial<UnivariatePolynomial<E>> a,
            MultivariatePolynomial<UnivariatePolynomial<E>> b)
    {
        return PolynomialGCD(
                MultivariatePolynomial<E>.AsNormalMultivariate(a, 0),
                MultivariatePolynomial<E>.AsNormalMultivariate(b, 0))
            .AsOverUnivariateEliminate(0);
    }

    public static
        bool isOverMultivariate<E>(MultivariatePolynomial<E> p)
    {
        return p.ring is IMultivariateRing;
    }

    private static MultivariatePolynomial<MultivariatePolynomial<E>>
        PolynomialGCDOverMultivariate<E>(MultivariatePolynomial<MultivariatePolynomial<E>> a,
            MultivariatePolynomial<MultivariatePolynomial<E>> b)
    {
        int[] cfVars = Utils.Utils.Sequence(a.Lc().nVariables);
        int[] mainVars = Utils.Utils.Sequence(a.Lc().nVariables, a.Lc().nVariables + a.nVariables);
        return PolynomialGCD(
                MultivariatePolynomial<E>.AsNormalMultivariate(a, cfVars, mainVars),
                MultivariatePolynomial<E>.AsNormalMultivariate(b, cfVars, mainVars))
            .AsOverMultivariateEliminate(cfVars);
    }

    public static bool isOverUnivariateZp64<E>(MultivariatePolynomial<E> p)
    {
        return p.ring is IUnivariateRing && p.Lc() is UnivariatePolynomialZp64 uZp64 && uZp64.ring is IntegersZp64;
    }

    private static MultivariatePolynomial<UnivariatePolynomialZp64>
        PolynomialGCDOverUnivariateZp64(MultivariatePolynomial<UnivariatePolynomialZp64> a,
            MultivariatePolynomial<UnivariatePolynomialZp64> b)
    {
        return PolynomialGCD(
            MultivariatePolynomialZp64.AsNormalMultivariate(a, 0),
            MultivariatePolynomialZp64.AsNormalMultivariate(b, 0)).AsOverUnivariateEliminate(0);
    }

    public static bool isOverMultivariateZp64<E>(MultivariatePolynomial<E> p)
    {
        return p.ring is IMultivariateRing && p.Lc() is MultivariatePolynomialZp64 mZp64 && mZp64.ring is IntegersZp64;
    }

    private static MultivariatePolynomial<MultivariatePolynomialZp64>
        PolynomialGCDOverMultivariateZp64(MultivariatePolynomial<MultivariatePolynomialZp64> a,
            MultivariatePolynomial<MultivariatePolynomialZp64> b)
    {
        int[] cfVars = Utils.Utils.Sequence(a.Lc().nVariables);
        int[] mainVars = Utils.Utils.Sequence(a.Lc().nVariables, a.Lc().nVariables + a.nVariables);
        return PolynomialGCD(
            MultivariatePolynomialZp64.AsNormalMultivariate(a, cfVars, mainVars),
            MultivariatePolynomialZp64.AsNormalMultivariate(b, cfVars, mainVars)).AsOverMultivariateEliminate(cfVars);
    }

    private static MultivariatePolynomial<Rational<E>> PolynomialGCDInQ<E>(
        MultivariatePolynomial<Rational<E>> a,
        MultivariatePolynomial<Rational<E>> b)
    {
        var aRat = Util.ToCommonDenominator(a);
        var bRat = Util.ToCommonDenominator(b);

        return Util.AsOverRationals(a.ring, PolynomialGCD(aRat.Item1, bRat.Item1));
    }

    // private static <
    //         Term extends AMonomial<Term>,
    //         mPoly extends AMultivariatePolynomial<Term, mPoly>,
    //         sPoly extends IUnivariatePolynomial<sPoly>
    //         > MultivariatePolynomial<mPoly>
    // PolynomialGCDinMultipleFieldExtension(MultivariatePolynomial<mPoly> a, MultivariatePolynomial<mPoly> b) {
    //     MultipleFieldExtension<Term, mPoly, sPoly> ring = (MultipleFieldExtension<Term, mPoly, sPoly>) a.ring;
    //     SimpleFieldExtension<sPoly> simpleExtension = ring.getSimpleExtension();
    //     return PolynomialGCD(
    //             a.mapCoefficients(simpleExtension, ring::inverse),
    //             b.mapCoefficients(simpleExtension, ring::inverse))
    //             .mapCoefficients(ring, ring::image);
    // }
    /* ============================================== Auxiliary methods ============================================= */
    public static int[] inversePermutation(int[] permutation)
    {
        int[] inv = new int[permutation.Length];
        for (int i = permutation.Length - 1; i >= 0; --i)
            inv[permutation[i]] = i;
        return inv;
    }

    sealed class GCDInput<E>
    {
        public readonly MultivariatePolynomial<E>? aReduced;
        public readonly MultivariatePolynomial<E>? bReduced;
        public readonly MultivariatePolynomial<E>? earlyGCD;

        public readonly int[]? DegreeBounds;
        readonly int[]? mapping;

        public readonly int lastPresentVariable;

        public readonly int evaluationStackLimit;

        readonly Monomial<E>? monomialGCD;

        public readonly int finiteExtensionDegree;

        public GCDInput(MultivariatePolynomial<E> earlyGCD)
        {
            this.earlyGCD = earlyGCD;
            aReduced = bReduced = null;
            DegreeBounds = mapping = null;
            lastPresentVariable = evaluationStackLimit = -1;
            monomialGCD = null;
            finiteExtensionDegree = -1;
        }

        public GCDInput(MultivariatePolynomial<E> aReduced, MultivariatePolynomial<E> bReduced, Monomial<E> monomialGCD,
            int evaluationStackLimit, int[] DegreeBounds, int[] mapping, int lastPresentVariable,
            int finiteExtensionDegree)
        {
            //assert monomialGCD == null || aReduced.ring.isOne(monomialGCD.coefficient);
            this.aReduced = aReduced;
            this.bReduced = bReduced;
            this.monomialGCD = monomialGCD;
            this.earlyGCD = null;
            this.evaluationStackLimit = evaluationStackLimit;
            this.DegreeBounds = DegreeBounds;
            this.mapping = inversePermutation(mapping);
            this.lastPresentVariable = lastPresentVariable;
            this.finiteExtensionDegree = finiteExtensionDegree;
        }


        public MultivariatePolynomial<E> restoreGCD(MultivariatePolynomial<E> result)
        {
            return MultivariatePolynomial<E>.RenameVariables(result, mapping).Multiply(monomialGCD);
        }
    }

    private static MultivariatePolynomial<E> ensureMonicOverGF<E>(MultivariatePolynomial<E> g)
    {
        if (g.IsOverFiniteField() && !g.IsMonic())
            return g.Monic();
        return g;
    }

    private static MultivariatePolynomial<E> trivialGCD<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        if (a == b)
            return a.Clone();
        if (a.IsZero())
            return b.Clone();
        if (b.IsZero())
            return a.Clone();
        if (a.IsConstant() || b.IsConstant())
            return a.CreateOne();
        if (a.Size() == 1)
            return gcdWithMonomial(a.Lt(), b);
        if (b.Size() == 1)
            return gcdWithMonomial(b.Lt(), a);
        if (a.Degree() == 1)
            return gcdWithLinearPoly(b, a);
        if (b.Degree() == 1)
            return gcdWithLinearPoly(a, b);

        var eq = equalsUpToConstant(a, b);
        if (eq != null)
            return eq;

        return null;
    }

    private static MultivariatePolynomial<E>? equalsUpToConstant<E>(MultivariatePolynomial<E> a,
        MultivariatePolynomial<E> b)
    {
        if (a.ring is IntegersZp64)
            return equalsUpToConstant(a.AsZp64(), b.AsZp64())?.AsT<E>();

        return equalsUpToConstantGeneric(a, b);
    }

    private static MultivariatePolynomial<E>? equalsUpToConstantGeneric<E>(MultivariatePolynomial<E> a,
        MultivariatePolynomial<E> b)
    {
        if (a == b)
            return a.Clone();
        if (a.Size() != b.Size())
            return null;
        if (!a.Lt().DvEquals(b.Lt()))
            return null;
        using var aIt = a.terms.Values.GetEnumerator();
        using var bIt = b.terms.Values.GetEnumerator();
        Ring<E> ring = a.ring;
        MonomialSet<E> result = new MonomialSet<E>(a.ordering);
        E c = ring.Gcd(a.Lc(), b.Lc());
        if (ring.Signum(a.Lc()) != ring.Signum(c))
            c = ring.Negate(c);
        E aCorrection = ring.DivideExact(a.Lc(), c);
        E bCorrection = ring.DivideExact(b.Lc(), c);
        while (aIt.MoveNext())
        {
            Monomial<E>
                aTerm = aIt.Current,
                bTerm = bIt.Current;
            if (!aTerm.DvEquals(bTerm))
                return null;

            c = ring.Gcd(aTerm.coefficient, bTerm.coefficient);
            if (ring.Signum(aTerm.coefficient) != ring.Signum(c))
                c = ring.Negate(c);

            if (!ring.DivideExact(aTerm.coefficient, c).Equals(aCorrection)
                || !ring.DivideExact(bTerm.coefficient, c).Equals(bCorrection))
                return null;

            result.Add(aTerm.SetCoefficient(c));
        }

        return ensureMonicOverGF(a.Create(result));
    }

    private static MultivariatePolynomialZp64? equalsUpToConstant(MultivariatePolynomialZp64 a,
        MultivariatePolynomialZp64 b) // TODO check if can use generic
    {
        if (a == b)
            return a.Clone();
        if (a.Size() != b.Size())
            return null;
        if (!a.Lt().DvEquals(b.Lt()))
            return null;
        using var
            aIt = a.terms.Values.GetEnumerator();
        using var
            bIt = b.terms.Values.GetEnumerator();
        IntegersZp64 ring = (IntegersZp64)a.ring;
        long bCorrection = ring.Divide(b.Lc(), a.Lc());
        while (aIt.MoveNext())
        {
            var
                aTerm = aIt.Current;
            var
                bTerm = bIt.Current;
            if (!aTerm.DvEquals(bTerm))
                return null;

            long bc = ring.Divide(bTerm.coefficient, aTerm.coefficient);
            if (bc != bCorrection)
                return null;
        }

        return a.Clone().Monic();
    }

    private static MultivariatePolynomial<E> gcdWithLinearPoly<E>(MultivariatePolynomial<E> poly,
        MultivariatePolynomial<E> linear)
    {
        if (poly.IsOverField())
        {
            if (MultivariateDivision.DividesQ(poly, linear))
                return linear.Clone().Monic();
            else
                return linear.CreateOne();
        }
        else
            return linearGCDe(poly, linear);
    }

    private static MultivariatePolynomial<E> linearGCDe<E>(MultivariatePolynomial<E> poly,
        MultivariatePolynomial<E> linear)
    {
        E lContent = linear.Content();
        E pContent = poly.Content();
        E cGCD = poly.ring.Gcd(lContent, pContent);

        linear = linear.Clone().DivideExact(lContent);
        if (MultivariateDivision.DividesQ(poly, linear))
            return linear.Multiply(cGCD);
        else
            return linear.CreateConstant(cGCD);
    }

    static int
        EARLY_ADJUST_SMALL_POLY_SIZE_THRESHOLD = 1024,
        EARLY_ADJUST_POLY_DISBALANCE = 10,
        EARLY_ADJUST_LARGE_POLY_SIZE_THRESHOLD = EARLY_ADJUST_SMALL_POLY_SIZE_THRESHOLD * EARLY_ADJUST_POLY_DISBALANCE;

    static GCDInput<E> preparedGCDInput<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b,
        Func<MultivariatePolynomial<E>, MultivariatePolynomial<E>, MultivariatePolynomial<E>> gcdAlgorithm)
    {
        var trivialGCD = MultivariateGCD.trivialGCD(a, b);
        if (trivialGCD != null)
            return new GCDInput<E>(trivialGCD);

        var ringSize = a.CoefficientRingCardinality();
        // ring cardinality, i.e. number of possible random choices
        int evaluationStackLimit = ringSize == null ? -1 : (ringSize.Value.IsInt() ? (int)ringSize.Value : -1);

        // find monomial GCD
        // and remove monomial Content from a and b
        a = a.Clone();
        b = b.Clone(); // prevent rewriting original data
        var monomialGCD = reduceMonomialContent(a, b);

        trivialGCD = MultivariateGCD.trivialGCD(a, b);
        if (trivialGCD != null)
            return new GCDInput<E>(trivialGCD.Multiply(monomialGCD));
        int
            nVariables = a.nVariables;
        int[] aDegrees = a.DegreesRef(),
            bDegrees = b.DegreesRef(),
            DegreeBounds = new int[nVariables]; // Degree bounds for gcd

        // populate initial gcd Degree bounds
        int nUnused = 0;
        for (int i = 0; i < nVariables; i++)
        {
            DegreeBounds[i] = Math.Min(aDegrees[i], bDegrees[i]);
            if (DegreeBounds[i] == 0)
                ++nUnused;
        }

        if (nUnused == nVariables)
            // all variables are unused
            return new GCDInput<E>(a.Create(monomialGCD));

        bool adjusted = false;
        int
            maxSize = Math.Max(a.Size(), b.Size()),
            minSize = Math.Min(a.Size(), b.Size());
        if (1.0 * nUnused / nVariables <= 0.25 && // if there are not much redundant vars
            (maxSize < EARLY_ADJUST_SMALL_POLY_SIZE_THRESHOLD
             || (maxSize < EARLY_ADJUST_LARGE_POLY_SIZE_THRESHOLD
                 && (maxSize / minSize) >= EARLY_ADJUST_POLY_DISBALANCE)))
        {
            // adjust Degree bounds with randomized substitutions and univariate images (relatively expensive)
            // do this only if polynomials are relatively small
            adjustDegreeBounds(a, b, DegreeBounds);
            adjusted = true;

            if (Enumerable.SequenceEqual(DegreeBounds, a.DegreesRef()) && MultivariateDivision.DividesQ(b, a))
                return new GCDInput<E>(ensureMonicOverGF(a.Clone().Multiply(monomialGCD)));
            if (Enumerable.SequenceEqual(DegreeBounds, b.DegreesRef()) && MultivariateDivision.DividesQ(a, b))
                return new GCDInput<E>(ensureMonicOverGF(b.Clone().Multiply(monomialGCD)));
        }

        GCDInput<E> earlyGCD;
        // get rid of variables with DegreeBounds[var] == 0
        earlyGCD = getRidOfUnusedVariables(a, b, gcdAlgorithm, monomialGCD, DegreeBounds);
        if (earlyGCD != null)
            return earlyGCD;

        if (!adjusted)
        {
            // adjust Degree bounds with randomized substitutions and univariate images (relatively expensive)
            adjustDegreeBounds(a, b, DegreeBounds);

            if (Enumerable.SequenceEqual(DegreeBounds, a.DegreesRef()) && MultivariateDivision.DividesQ(b, a))
                return new GCDInput<E>(ensureMonicOverGF(a.Clone().Multiply(monomialGCD)));
            if (Enumerable.SequenceEqual(DegreeBounds, b.DegreesRef()) && MultivariateDivision.DividesQ(a, b))
                return new GCDInput<E>(ensureMonicOverGF(b.Clone().Multiply(monomialGCD)));
        }

        // get rid of variables with DegreeBounds[var] == 0 if such occured after a call to #adjustDegreeBounds
        earlyGCD = getRidOfUnusedVariables(a, b, gcdAlgorithm, monomialGCD, DegreeBounds);
        if (earlyGCD != null)
            return earlyGCD;

        // now swap variables so that the first variable will have the maximal Degree (univariate gcd is fast),
        // and all non-used variables are at the end of poly's

        int[] variables = Utils.Utils.Sequence(nVariables);
        //sort in descending order
        Array.Sort(Utils.Utils.Negate(DegreeBounds), variables);
        Utils.Utils.Negate(DegreeBounds); //recover DegreeBounds

        int lastGCDVariable = 0; //recalculate lastPresentVariable
        for (; lastGCDVariable < DegreeBounds.Length; ++lastGCDVariable)
            if (DegreeBounds[lastGCDVariable] == 0)
                break;
        --lastGCDVariable;

        a = MultivariatePolynomial<E>.RenameVariables(a, variables);
        b = MultivariatePolynomial<E>.RenameVariables(b, variables);

        // check whether coefficient ring cardinality is large enough
        int finiteExtensionDegree = 1;
        int cardinalityBound = 9 * DegreeBounds.Max();
        if (ringSize != null && ringSize.Value.IsInt() && (int)ringSize.Value < cardinalityBound)
        {
            long ds = (long)ringSize;
            finiteExtensionDegree = 2;
            long tmp = ds;
            for (; tmp < cardinalityBound; ++finiteExtensionDegree)
                tmp = tmp * ds;
        }

        return new GCDInput<E>(a, b, monomialGCD, evaluationStackLimit, DegreeBounds, variables, lastGCDVariable,
            finiteExtensionDegree);
    }

    static GCDInput<E>? getRidOfUnusedVariables<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b,
        Func<MultivariatePolynomial<E>, MultivariatePolynomial<E>, MultivariatePolynomial<E>> gcdAlgorithm,
        Monomial<E> monomialGCD, int[] DegreeBounds)
    {
        int
            nVariables = a.nVariables,
            nUsedVariables = 0, // number of really present variables in gcd
            lastGCDVariable = -1; // last variable that present in both input polynomials

        for (int i = 0; i < nVariables; i++)
            if (DegreeBounds[i] != 0)
            {
                ++nUsedVariables;
                lastGCDVariable = i;
            }

        if (nUsedVariables == 0)
            // gcd is constant => 1
            return new GCDInput<E>(a.Create(monomialGCD));

        if (nUsedVariables == 1)
            // switch to univariate gcd
        {
            var uaContent = a.AsOverUnivariate(lastGCDVariable).Content();
            var ubContent = b.AsOverUnivariate(lastGCDVariable).Content();
            var iUnivar = UnivariateGCD.PolynomialGCD(uaContent, ubContent);

            var poly = MultivariatePolynomial<E>.AsMultivariate(iUnivar, nVariables, lastGCDVariable, a.ordering);
            return new GCDInput<E>(poly.Multiply(monomialGCD));
        }

        if (nUsedVariables != nVariables)
        {
            // some of the variables are present only in either a or b but not in both simultaneously
            // call this vars {dummies} and the rest vars as {used}
            // => then we can consider polynomials as over R[{used}][{dummies}] and calculate
            // gcd via recursive call as GCD[Content(a) in R[{used}], Content(b) in R[{used}]]

            int[] usedVariables = new int[nUsedVariables];
            int counter = 0;
            for (int i = 0; i < nVariables; ++i)
                if (DegreeBounds[i] != 0)
                    usedVariables[counter++] = i;

            // coefficients in R[{used}][{dummies}]
            var aEffective = getRidOfUnusedVariables(a, DegreeBounds);
            var bEffective = getRidOfUnusedVariables(b, DegreeBounds);

            var all = Utils.Utils.AddAll(aEffective, bEffective);
            // recursive call in PolynomialGCD
            var gcd = PolynomialGCD(all, gcdAlgorithm);

            gcd = gcd.JoinNewVariables(nVariables, usedVariables);
            return new GCDInput<E>(gcd.Multiply(monomialGCD));
        }

        return null;
    }

    // get rid of unused variables
    private static MultivariatePolynomial<E>[] getRidOfUnusedVariables<E>(MultivariatePolynomial<E> poly,
        int[] DegreeBounds)
    {
        List<int>
            // variables that absent in poly, i.e.
            // may be dropped with dropVariables (very fast)
            drop = new List<int>(),
            // variables that present in poly, but don't
            // present in GCD numerated as after invocation of dropVariables
            unused = new List<int>();
        for (int i = 0; i < poly.nVariables; ++i)
        {
            if (poly.Degree(i) == 0)
                drop.Add(i);
            else if (DegreeBounds[i] == 0)
                unused.Add(i - drop.Count);
        }

        // fast drop variables
        var reduced = poly.DropVariables(drop.ToArray());

        // if there are no more redundant variables, just return
        if (unused.Count == 0)
        {
            var array = reduced.CreateArray(1);
            array[0] = reduced;
            return array;
        }

        // faster method for univariate
        if (unused.Count == 1)
            return reduced.AsUnivariateEliminate(unused[0]).GetDataReferenceUnsafe().Where(p => !p.IsZero()).ToArray();

        // used variables in transformed poly
        int[] usedVariables =
            Utils.Utils.IntSetDifference(Utils.Utils.Sequence(0, reduced.nVariables), unused.ToArray());
        return reduced.AsOverMultivariateEliminate(usedVariables).CoefficientsArray();
    }

    private static void adjustDegreeBounds<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b,
        int[] gcdDegreeBounds)
    {
        if (a.IsOverZ())
            adjustDegreeBoundsZ(a.AsZ(), b.AsZ(), gcdDegreeBounds);
        // else if (Util.isOverSimpleNumberField(a))
        //     adjustDegreeBoundsNumberField((MultivariatePolynomial) a, (MultivariatePolynomial) b, gcdDegreeBounds);
        else if (a.IsOverFiniteField())
            adjustDegreeBoundsFiniteField(a, b, gcdDegreeBounds);
    }

    private static void adjustDegreeBoundsFiniteField<E>(MultivariatePolynomial<E> a,
        MultivariatePolynomial<E> b,
        int[] gcdDegreeBounds)
    {
        int nVariables = a.nVariables;
        // for (int i = 0; i < nVariables; i++)
        //     if (gcdDegreeBounds[i] == 0) {
        //         if (a.Degree(i) != 0)
        //             a = a.EvaluateAtRandomPreservingSkeleton(i, PrivateRandom.GetRandom());
        //         if (b.Degree(i) != 0)
        //             b = b.EvaluateAtRandomPreservingSkeleton(i, PrivateRandom.GetRandom());
        //     }

        E[] subs = new E[nVariables];
        Random rnd = PrivateRandom.GetRandom();
        if (a.CoefficientRingCardinality() != null
            && a.CoefficientRingCardinality().Value.GetBitLength() <= 10)
        {
            // in case of small cardinality we have to do clever
            int cardinality = (int)a.CoefficientRingCardinality().Value;
            for (int i = 0; i < nVariables; i++)
            {
                HashSet<E> seen = new HashSet<E>();
                do
                {
                    if (seen.Count == cardinality)
                        return; // nothing can be done
                    // find non trivial substitution
                    E rval;
                    do
                    {
                        rval = randomElement(a.ring, rnd);
                    } while (seen.Contains(rval));

                    seen.Add(rval);
                    subs[i] = rval;
                } while (a.Evaluate(i, subs[i]).IsZero() || b.Evaluate(i, subs[i]).IsZero());
            }
        }
        else
            for (int i = 0; i < nVariables; i++)
                subs[i] = randomElement(a.ring, rnd);

        UnivariatePolynomial<E>[]
            uaImages = univariateImages(a, subs),
            ubImages = univariateImages(b, subs);
        for (int i = 0; i < nVariables; i++)
        {
            UnivariatePolynomial<E> ua = uaImages[i], ub = ubImages[i];
            if (ua.Degree() != a.Degree(i) || ub.Degree() != b.Degree(i))
                continue;
            gcdDegreeBounds[i] = Math.Min(gcdDegreeBounds[i], UnivariateGCD.PolynomialGCD(ua, ub).Degree());
        }
    }

    static UnivariatePolynomial<E>[] univariateImages<E>(MultivariatePolynomial<E> poly, E[] subs)
    {
        E[][] univariate = new E[poly.nVariables][];
        for (int i = 0; i < univariate.Length; ++i)
            univariate[i] = poly.ring.CreateZeroesArray(poly.Degree(i) + 1);

        E[] tmp = new E[poly.nVariables];
        MultivariatePolynomial<E>.PrecomputedPowersHolder powers = poly.MkPrecomputedPowers(subs);
        foreach (var term in poly.terms)
        {
            Array.Fill(tmp, term.coefficient);
            for (int i = 0; i < poly.nVariables; ++i)
            {
                E val = powers.Pow(i, term.exponents[i]);
                for (int j = 0; j < i; ++j)
                    tmp[j] = poly.ring.Multiply(tmp[j], val);
                for (int j = i + 1; j < poly.nVariables; ++j)
                    tmp[j] = poly.ring.Multiply(tmp[j], val);
            }

            for (int i = 0; i < poly.nVariables; ++i)
                univariate[i][term.exponents[i]] = poly.ring.Add(univariate[i][term.exponents[i]], tmp[i]);
        }

        UnivariatePolynomial<E>[] result = new UnivariatePolynomial<E>[poly.nVariables];
        for (int i = 0; i < poly.nVariables; ++i)
        {
            result[i] = UnivariatePolynomial<E>.CreateUnsafe(poly.ring, univariate[i]);
            Debug.Assert(result[i].Equals(
                poly.Evaluate(Utils.Utils.Remove(Utils.Utils.Sequence(0, poly.nVariables), i),
                    Utils.Utils.Remove(subs, i)).AsUnivariate()));
        }

        return result;
    }

    static void adjustDegreeBoundsZ(MultivariatePolynomial<BigInteger> a,
        MultivariatePolynomial<BigInteger> b,
        int[] gcdDegreeBounds)
    {
        // perform some test modulo prime
        MultivariatePolynomialZp64 aMod, bMod;
        //do {
        // some random prime number
        IntegersZp64 zpRing = Rings.Zp64(SmallPrimes.NextPrime((1 << 20) + PrivateRandom.GetRandom().Next(1 << 10)));
        aMod = MultivariatePolynomial<BigInteger>.AsOverZp64(a, zpRing);
        bMod = MultivariatePolynomial<BigInteger>.AsOverZp64(b, zpRing);
        //} while (!a.sameSkeletonQ(aMod) || !b.sameSkeletonQ(bMod));

        adjustDegreeBounds(aMod, bMod, gcdDegreeBounds);
        if (gcdDegreeBounds.Sum() == 0)
            return;

        int nVariables = a.nVariables;
        // for (int i = 0; i < nVariables; i++)
        //     if (gcdDegreeBounds[i] == 0) {
        //         if (a.Degree(i) != 0)
        //             a = a.EvaluateAtRandomPreservingSkeleton(i, PrivateRandom.GetRandom());
        //         if (b.Degree(i) != 0)
        //             b = b.EvaluateAtRandomPreservingSkeleton(i, PrivateRandom.GetRandom());
        //     }

        BigInteger[] subs = new BigInteger[a.nVariables];
        Array.Fill(subs, BigInteger.One);

        UnivariatePolynomial<BigInteger>[]
            uaImages = univariateImagesZ(a, subs),
            ubImages = univariateImagesZ(b, subs);
        for (int i = 0; i < nVariables; i++)
        {
            UnivariatePolynomial<BigInteger> ua = uaImages[i], ub = ubImages[i];
            if (ua.Degree() != a.Degree(i) || ub.Degree() != b.Degree(i))
                continue;
            gcdDegreeBounds[i] = Math.Min(gcdDegreeBounds[i], UnivariateGCD.PolynomialGCD(ua, ub).Degree());
        }
    }

    // static void adjustDegreeBoundsNumberField(MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a,
    //                                           MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> b,
    //                                           int[] gcdDegreeBounds) {
    //     // perform some test modulo prime
    //     MultivariatePolynomial<UnivariatePolynomialZp64> aMod, bMod;
    //     //do {
    //     // some random prime number
    //     IntegersZp64 zpRing = Rings.Zp64(SmallPrimes.nextPrime((1 << 20) + PrivateRandom.GetRandom().nextInt(1 << 10)));
    //     AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> ring = (AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>>) a.ring;
    //     FiniteField<UnivariatePolynomialZp64> numberFieldMod = new FiniteField<>(UnivariatePolynomial.asOverZp64Q(ring.getMinimalPolynomial(), zpRing));
    //     aMod = a.mapCoefficients(numberFieldMod, cf -> UnivariatePolynomial.asOverZp64Q(cf, zpRing));
    //     bMod = b.mapCoefficients(numberFieldMod, cf -> UnivariatePolynomial.asOverZp64Q(cf, zpRing));
    //     //} while (!a.sameSkeletonQ(aMod) || !b.sameSkeletonQ(bMod));
    //
    //     adjustDegreeBounds(aMod, bMod, gcdDegreeBounds);
    // }
    static UnivariatePolynomial<BigInteger>[] univariateImagesZ(MultivariatePolynomial<BigInteger> poly,
        BigInteger[] subs)
    {
        Debug.Assert(subs.All(e => e.IsOne));
        BigInteger[][] univariate = new BigInteger[poly.nVariables][];
        for (int i = 0; i < univariate.Length; ++i)
        {
            univariate[i] = new BigInteger[poly.Degree(i) + 1];
            Array.Fill(univariate[i], BigInteger.Zero);
        }

        BigInteger[] tmp = new BigInteger[poly.nVariables];
        foreach (var term in poly.terms)
        {
            Array.Fill(tmp, term.coefficient);
            for (int i = 0; i < poly.nVariables; ++i)
                univariate[i][term.exponents[i]] = poly.ring.Add(univariate[i][term.exponents[i]], tmp[i]);
        }

        UnivariatePolynomial<BigInteger>[] result = new UnivariatePolynomial<BigInteger>[poly.nVariables];
        for (int i = 0; i < poly.nVariables; ++i)
        {
            result[i] = UnivariatePolynomial<BigInteger>.CreateUnsafe(poly.ring, univariate[i]);
            Debug.Assert(result[i].Equals(poly.Evaluate(Utils.Utils.Remove(Utils.Utils.Sequence(0, poly.nVariables), i),
                Utils.Utils.Remove(subs, i)).AsUnivariate()));
        }

        return result;
    }

    private static MultivariatePolynomial<E> gcdWithMonomial<E>(Monomial<E> monomial, MultivariatePolynomial<E> poly)
    {
        return poly.Create(poly.CommonContent(monomial));
    }

    static Monomial<E> reduceMonomialContent<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        var aMonomialContent = a.MonomialContent();
        int[] exponentsGCD = b.MonomialContent().exponents;
        MultivariatePolynomial<E>.SetMin(aMonomialContent.exponents, exponentsGCD);
        var monomialGCD = a.monomialAlgebra.Create(exponentsGCD);

        a = a.DivideDegreeVectorOrNull(monomialGCD);
        b = b.DivideDegreeVectorOrNull(monomialGCD);
        Debug.Assert(a != null && b != null);

        // remove the rest of monomial Content
        a = a.DivideDegreeVectorOrNull(a.MonomialContent());
        b = b.DivideDegreeVectorOrNull(b.MonomialContent());
        Debug.Assert(a != null && b != null);

        return monomialGCD;
    }

    private sealed class UnivariateContent<E>
    {
        public readonly MultivariatePolynomial<UnivariatePolynomial<E>> poly;
        public readonly MultivariatePolynomial<E> primitivePart;
        public readonly UnivariatePolynomial<E> Content;

        public UnivariateContent(MultivariatePolynomial<UnivariatePolynomial<E>> poly,
            MultivariatePolynomial<E> primitivePart, UnivariatePolynomial<E> Content)
        {
            this.poly = poly;
            this.primitivePart = primitivePart;
            this.Content = Content;
        }
    }

    private static
        MultivariatePolynomial<UnivariatePolynomial<E>> AsOverUnivariate<E>(MultivariatePolynomial<E> poly,
            int variable)
    {
        return poly.AsOverUnivariateEliminate(variable);
    }

    private static UnivariateContent<E> univariateContent<E>(
        MultivariatePolynomial<E> poly, int variable)
    {
        //convert poly to Zp[var][x...]

        MultivariatePolynomial<UnivariatePolynomial<E>> conv = AsOverUnivariate(poly, variable);
        //univariate Content
        var uContent = UnivariateGCD.PolynomialGCD(conv.Coefficients());
        var mContent = MultivariatePolynomial<E>.AsMultivariate(uContent, poly.nVariables, variable, poly.ordering);
        var primitivePart = MultivariateDivision.DivideExact(poly, mContent);
        return new UnivariateContent<E>(conv, primitivePart, uContent);
    }

    private sealed class PrimitiveInput<E>
    {
        public readonly MultivariatePolynomial<E> aPrimitive;
        public readonly MultivariatePolynomial<E> bPrimitive;

        public readonly UnivariatePolynomial<E> contentGCD;
        public readonly UnivariatePolynomial<E> lcGCD;

        public PrimitiveInput(MultivariatePolynomial<E> aPrimitive, MultivariatePolynomial<E> bPrimitive,
            UnivariatePolynomial<E> contentGCD, UnivariatePolynomial<E> lcGCD)
        {
            this.aPrimitive = aPrimitive;
            this.bPrimitive = bPrimitive;
            this.contentGCD = contentGCD;
            this.lcGCD = lcGCD;
        }
    }

    private static PrimitiveInput<E> makePrimitive<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b,
        int variable)
    {
        // a and b as Zp[x_k][x_1 ... x_{k-1}]
        UnivariateContent<E>
            aContent = univariateContent(a, variable),
            bContent = univariateContent(b, variable);

        a = aContent.primitivePart;
        b = bContent.primitivePart;

        // gcd of Zp[x_k] Content and lc
        var
            ContentGCD = UnivariateGCD.PolynomialGCD(aContent.Content, bContent.Content);
        var
            lcGCD = UnivariateGCD.PolynomialGCD(aContent.poly.Lc(), bContent.poly.Lc());
        return new PrimitiveInput<E>(a, b, ContentGCD, lcGCD);
    }

    private static MultivariatePolynomial<E>[] multivariateCoefficients<E>(MultivariatePolynomial<E> poly, int variable)
    {
        return poly.Degrees(variable).Select(d => poly.CoefficientOf(variable, d)).ToArray();
    }

    static MultivariatePolynomial<E> ContentGCD<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b,
        int variable, Func<MultivariatePolynomial<E>, MultivariatePolynomial<E>, MultivariatePolynomial<E>> algorithm)
    {
        var aCfs = multivariateCoefficients(a, variable);
        var bCfs = multivariateCoefficients(b, variable);

        var all = a.CreateArray(aCfs.Length + bCfs.Length);
        Array.Copy(aCfs, 0, all, 0, aCfs.Length);
        Array.Copy(bCfs, 0, all, aCfs.Length, bCfs.Length);

        var gcd = PolynomialGCD(all, algorithm);
        Debug.Assert(MultivariateDivision.DividesQ(a, gcd));
        Debug.Assert(MultivariateDivision.DividesQ(b, gcd));
        return gcd;
    }

    public static E randomElement<E>(Ring<E> ring, Random rnd)
    {
        if (ring.IsFiniteField() && ring.Characteristic().GetBitLength() > 16)
            return ring.ValueOfLong(rnd.NextInt64());
        else if (ring is Rationals<BigInteger> || ring is Integers /* TODO || (ring is AlgebraicNumberField)*/)
            return ring.ValueOfLong(rnd.Next(1 << 16));
        else
            return ring.RandomElement(rnd);
    }

    /* =========================================== Multivariate GCD over Z ========================================== */
    public static MultivariatePolynomial<BigInteger> ZippelGCDInZ(MultivariatePolynomial<BigInteger> a,
        MultivariatePolynomial<BigInteger> b)
    {
        Util.EnsureOverZ(a, b);
        if (a == b)
            return a.Clone();
        if (a.IsZero()) return b.Clone();
        if (b.IsZero()) return a.Clone();

        if (a.Degree() < b.Degree())
            return ZippelGCDInZ(b, a);
        BigInteger aContent = a.Content(), bContent = b.Content();
        BigInteger ContentGCD = BigInteger.GreatestCommonDivisor(aContent, bContent);
        if (a.IsConstant() || b.IsConstant())
            return a.CreateConstant(ContentGCD);

        a = a.Clone().DivideOrNull(aContent)!;
        b = b.Clone().DivideOrNull(bContent)!;
        return ZippelGCDInZ0(a, b).Multiply(ContentGCD);
    }

    static MultivariatePolynomial<BigInteger> ZippelGCDInZ0(
        MultivariatePolynomial<BigInteger> a,
        MultivariatePolynomial<BigInteger> b)
    {
        GCDInput<BigInteger>
            gcdInput = preparedGCDInput(a, b, MultivariateGCD.ZippelGCDInZ);
        if (gcdInput.earlyGCD != null)
            return gcdInput.earlyGCD;

        a = gcdInput.aReduced;
        b = gcdInput.bReduced;

        MultivariatePolynomial<BigInteger> pContentGCD = ContentGCD(a, b, 0, MultivariateGCD.ZippelGCDInZ);
        if (!pContentGCD.IsConstant())
        {
            a = MultivariateDivision.DivideExact(a, pContentGCD);
            b = MultivariateDivision.DivideExact(b, pContentGCD);
            return gcdInput.restoreGCD(ZippelGCDInZ(a, b).Multiply(pContentGCD));
        }

        BigInteger
            lcGCD = BigInteger.GreatestCommonDivisor(a.Lc(), b.Lc()),
            ccGCD = BigInteger.GreatestCommonDivisor(a.Cc(), b.Cc());

        // choose better prime for start
        long startingPrime;
        if (Math.Max(lcGCD.GetBitLength(), ccGCD.GetBitLength()) < 128)
            startingPrime = 1L << 30;
        else
            startingPrime = 1L << 60;
        PrimesIterator primesLoop = new PrimesIterator(startingPrime - (1 << 12));
        Random random = PrivateRandom.GetRandom();

        while (true)
        {
            main_loop : ;
            // prepare the skeleton
            long basePrime = primesLoop.Take();
            BigInteger bPrime = new BigInteger(basePrime);
            // TODO assert basePrime != -1 : "long overflow";

            IntegersZp ring = new IntegersZp(basePrime);
            // reduce Z -> Zp
            MultivariatePolynomial<BigInteger>
                abMod = a.SetRing(ring),
                bbMod = b.SetRing(ring);
            if (!abMod.SameSkeletonQ(a) || !bbMod.SameSkeletonQ(b))
                continue;

            MultivariatePolynomialZp64
                aMod = MultivariatePolynomial<BigInteger>.AsOverZp64(abMod),
                bMod = MultivariatePolynomial<BigInteger>.AsOverZp64(bbMod);

            // the base image
            // accumulator to update coefficients via Chineese remainding
            MultivariatePolynomialZp64 @base = PolynomialGCD(aMod, bMod);
            long lLcGCD = (long)(lcGCD % bPrime);
            // scale to correct l.c.
            @base = @base.Monic(lLcGCD);

            if (@base.IsConstant())
                return gcdInput.restoreGCD(a.CreateOne());

            // cache the previous base
            MultivariatePolynomial<BigInteger> previousBase = null;

            // over all primes
            while (true)
            {
                long prime = primesLoop.Take();
                if (MachineArithmetic.IsOverflowMultiply(basePrime, prime) ||
                    basePrime * prime > MachineArithmetic.MAX_SUPPORTED_MODULUS)
                    break;

                bPrime = new BigInteger(prime);
                ring = new IntegersZp(bPrime);

                // reduce Z -> Zp
                abMod = a.SetRing(ring);
                bbMod = b.SetRing(ring);
                if (!abMod.SameSkeletonQ(a) || !bbMod.SameSkeletonQ(b))
                    continue;

                aMod = MultivariatePolynomial<BigInteger>.AsOverZp64(abMod);
                bMod = MultivariatePolynomial<BigInteger>.AsOverZp64(bbMod);

                IntegersZp64 lDomain = new IntegersZp64(prime);

                // calculate new GCD using previously calculated skeleton via sparse interpolation
                MultivariatePolynomialZp64
                    modularGCD = interpolateGCD(aMod, bMod, @base.SetRingUnsafe(lDomain), random);
                if (modularGCD == null)
                {
                    // interpolation failed => assumed form is wrong => start over
                    goto main_loop;
                }

                if (!MultivariateDivision.DividesQ(aMod, modularGCD) ||
                    !MultivariateDivision.DividesQ(bMod, modularGCD))
                {
                    // extremely rare event
                    // bad base prime chosen
                    goto main_loop;
                }

                if (modularGCD.IsConstant())
                    return gcdInput.restoreGCD(a.CreateOne());

                // better Degree bound found -> start over
                if (modularGCD.Degree(0) < @base.Degree(0))
                {
                    lLcGCD = (long)(lcGCD % bPrime);
                    // scale to correct l.c.
                    @base = modularGCD.Monic(lLcGCD);
                    basePrime = prime;
                    previousBase = null;
                    continue;
                }

                //skip unlucky prime
                if (modularGCD.Degree(0) > @base.Degree(0))
                    continue;

                //lifting
                long newBasePrime = basePrime * prime;
                long MonicFactor = modularGCD.ring.Multiply(
                    MachineArithmetic.ModInverse(modularGCD.Lc(), prime),
                    (long)(lcGCD % bPrime));

                ChineseRemainders.ChineseRemaindersMagicZp64 magic = ChineseRemainders.CreateMagic(basePrime, prime);
                PairedIterator<long, long> iterator = new PairedIterator<long, long>(@base, modularGCD);
                while (iterator.MoveNext())
                {
                    Monomial<long>
                        baseTerm = iterator.aTerm,
                        imageTerm = iterator.bTerm;

                    if (baseTerm.coefficient == 0)
                        // term is absent in the base
                        continue;

                    if (imageTerm.coefficient == 0)
                    {
                        // term is absent in the modularGCD => remove it from the base
                        // base.subtract(baseTerm);
                        @base.terms.Remove(baseTerm);
                        continue;
                    }

                    long oth = lDomain.Multiply(imageTerm.coefficient, MonicFactor);

                    // update base term
                    long newCoeff = ChineseRemainders.ChineseRemainder(magic, baseTerm.coefficient, oth);
                    @base.Put(baseTerm.SetCoefficient(newCoeff));
                }

                @base = @base.SetRingUnsafe(new IntegersZp64(newBasePrime));
                basePrime = newBasePrime;

                // two trials didn't change the result, probably we are done
                MultivariatePolynomial<BigInteger> candidate =
                    MultivariatePolynomial<BigInteger>.AsPolyZSymmetric(@base).PrimitivePart();
                if (previousBase != null && candidate.Equals(previousBase))
                {
                    previousBase = candidate;
                    //first check b since b is less Degree
                    if (!MultivariateDivision.DividesQ(b, candidate))
                        continue;

                    if (!MultivariateDivision.DividesQ(a, candidate))
                        continue;

                    return gcdInput.restoreGCD(candidate);
                }

                previousBase = candidate;
            }

            //continue lifting with multi-precision integers
            MultivariatePolynomial<BigInteger> bBase = @base.ToBigPoly();
            BigInteger bBasePrime = new BigInteger(basePrime);
            // over all primes
            while (true)
            {
                long prime = primesLoop.Take();
                bPrime = new BigInteger(prime);
                ring = new IntegersZp(bPrime);

                // reduce Z -> Zp
                abMod = a.SetRing(ring);
                bbMod = b.SetRing(ring);
                if (!abMod.SameSkeletonQ(a) || !bbMod.SameSkeletonQ(b))
                    continue;

                aMod = MultivariatePolynomial<BigInteger>.AsOverZp64(abMod);
                bMod = MultivariatePolynomial<BigInteger>.AsOverZp64(bbMod);

                IntegersZp64 lDomain = new IntegersZp64(prime);

                // calculate new GCD using previously calculated skeleton via sparse interpolation
                MultivariatePolynomialZp64
                    modularGCD = interpolateGCD(aMod, bMod, @base.SetRingUnsafe(lDomain), random);
                if (modularGCD == null)
                {
                    // interpolation failed => assumed form is wrong => start over
                    goto main_loop;
                }

                Debug.Assert(MultivariateDivision.DividesQ(aMod, modularGCD));
                Debug.Assert(MultivariateDivision.DividesQ(bMod, modularGCD));

                if (modularGCD.IsConstant())
                    return gcdInput.restoreGCD(a.CreateOne());

                // better Degree bound found -> start over
                if (modularGCD.Degree(0) < bBase.Degree(0))
                {
                    lLcGCD = (long)(lcGCD % bPrime);
                    @base = modularGCD.Monic(lLcGCD);
                    // scale to correct l.c.
                    bBase = @base.ToBigPoly();
                    bBasePrime = bPrime;
                    previousBase = null;
                    continue;
                }

                //skip unlucky prime
                if (modularGCD.Degree(0) > bBase.Degree(0))
                    continue;

                //lifting
                BigInteger newBasePrime = bBasePrime * (bPrime);
                long MonicFactor = lDomain.Multiply(
                    lDomain.Reciprocal(modularGCD.Lc()),
                    (long)(lcGCD % bPrime));

                PairedIterator<BigInteger, long> iterator = new PairedIterator<BigInteger, long>(bBase, modularGCD);
                ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic =
                    ChineseRemainders.CreateMagic(Rings.Z, bBasePrime, bPrime);
                while (iterator.MoveNext())
                {
                    Monomial<BigInteger> baseTerm = iterator.aTerm;
                    Monomial<long> imageTerm = iterator.bTerm;

                    if (baseTerm.coefficient.IsZero)
                        // term is absent in the base
                        continue;

                    if (imageTerm.coefficient == 0)
                    {
                        // term is absent in the modularGCD => remove it from the base
                        // bBase.subtract(baseTerm);
                        bBase.terms.Remove(baseTerm);
                        continue;
                    }

                    long oth = lDomain.Multiply(imageTerm.coefficient, MonicFactor);

                    // update base term
                    BigInteger newCoeff =
                        ChineseRemainders.ChineseRemainder(Rings.Z, magic, baseTerm.coefficient, new BigInteger(oth));
                    bBase.Put(baseTerm.SetCoefficient(newCoeff));
                }

                bBase = bBase.SetRingUnsafe(new IntegersZp(newBasePrime));
                bBasePrime = newBasePrime;

                // two trials didn't change the result, probably we are done
                MultivariatePolynomial<BigInteger> candidate =
                    MultivariatePolynomial<BigInteger>.AsPolyZSymmetric(bBase).PrimitivePart();
                if (previousBase != null && candidate.Equals(previousBase))
                {
                    previousBase = candidate;
                    //first check b since b is less Degree
                    if (!isGCDTriplet(b, a, candidate))
                        continue;

                    return gcdInput.restoreGCD(candidate);
                }

                previousBase = candidate;
            }
        }
    }

    static MultivariatePolynomial<E> divideSkeletonExact<E>(MultivariatePolynomial<E> dividend,
        MultivariatePolynomial<E> divider)
    {
        if (divider.IsConstant())
            return dividend;
        if (divider.IsMonomial())
            return dividend.Clone().DivideDegreeVectorOrNull(divider.Lt());

        dividend = dividend.Clone().SetAllCoefficientsToUnit();
        divider = divider.Clone().SetAllCoefficientsToUnit();

        var quotient = dividend.CreateZero();
        dividend = dividend.Clone();
        while (!dividend.IsZero())
        {
            var dlt = dividend.Lt();
            var ltDiv = dlt.DivideOrNull(divider.Lt());
            if (ltDiv == null)
                throw new Exception();
            quotient = quotient.Add(ltDiv);
            dividend = dividend.Subtract(divider.Clone().Multiply(ltDiv));
        }

        return quotient;
    }


    public static MultivariatePolynomial<BigInteger> ModularGCDInZ(MultivariatePolynomial<BigInteger> a,
        MultivariatePolynomial<BigInteger> b,
        Func<MultivariatePolynomialZp64, MultivariatePolynomialZp64, MultivariatePolynomialZp64> gcdInZp)
    {
        return ModularGCDInZ(a, b, gcdInZp, false);
    }

    static MultivariatePolynomial<BigInteger> ModularGCDInZ(MultivariatePolynomial<BigInteger> a,
        MultivariatePolynomial<BigInteger> b,
        Func<MultivariatePolynomialZp64, MultivariatePolynomialZp64, MultivariatePolynomialZp64> gcdInZp,
        bool switchToSparse)
    {
        Util.EnsureOverZ(a, b);
        if (a == b)
            return a.Clone();
        if (a.IsZero()) return b.Clone();
        if (b.IsZero()) return a.Clone();

        BigInteger aContent = a.Content(), bContent = b.Content();
        BigInteger ContentGCD = BigInteger.GreatestCommonDivisor(aContent, bContent);
        if (a.IsConstant() || b.IsConstant())
            return a.CreateConstant(ContentGCD);

        a = a.Clone().DivideOrNull(aContent);
        b = b.Clone().DivideOrNull(bContent);
        return ModularGCDInZ0(a, b, gcdInZp, switchToSparse).Multiply(ContentGCD);
    }

    static MultivariatePolynomial<BigInteger> ModularGCDInZ0(
        MultivariatePolynomial<BigInteger> a,
        MultivariatePolynomial<BigInteger> b,
        Func<MultivariatePolynomialZp64, MultivariatePolynomialZp64, MultivariatePolynomialZp64> gcdInZp,
        bool switchToSparse)
    {
        GCDInput<BigInteger>
            gcdInput = preparedGCDInput(a, b, MultivariateGCD.ZippelGCDInZ);
        if (gcdInput.earlyGCD != null)
            return gcdInput.earlyGCD;

        a = gcdInput.aReduced;
        b = gcdInput.bReduced;

        if (switchToSparse && !isDenseGCDProblem(a, b))
            gcdInput.restoreGCD(PolynomialGCD(a, b));

        BigInteger lcGCD = BigInteger.GreatestCommonDivisor(a.Lc(), b.Lc());

        // the base polynomial
        MultivariatePolynomial<BigInteger> @base = null;
        PrimesIterator primesLoop = new PrimesIterator((1 << 16) - (1 << 12));
        BigInteger? bBasePrime = null;

        main_loop:
        while (true)
        {
            // prepare the skeleton
            BigInteger bPrime = new BigInteger(primesLoop.Take());
            IntegersZp bRing = new IntegersZp(bPrime);

            // reduce Z -> Zp
            MultivariatePolynomial<BigInteger>
                abMod = a.SetRing(bRing),
                bbMod = b.SetRing(bRing);
            if (!abMod.SameSkeletonQ(a) || !bbMod.SameSkeletonQ(b))
                continue;

            MultivariatePolynomialZp64
                aMod = MultivariatePolynomial<BigInteger>.AsOverZp64(abMod),
                bMod = MultivariatePolynomial<BigInteger>.AsOverZp64(bbMod);

            // gcd in Zp
            MultivariatePolynomialZp64 modGCD = gcdInZp(aMod, bMod);
            if (modGCD.IsConstant())
                return gcdInput.restoreGCD(a.CreateOne());

            long lLcGCD = (long)(lcGCD % bPrime);
            // scale to correct l.c.
            modGCD = modGCD.Monic(lLcGCD);

            if (@base == null)
            {
                @base = modGCD.ToBigPoly();
                bBasePrime = bPrime;
                continue;
            }

            IntegersZp64 pRing = bRing.AsMachineRing();

            //lifting
            BigInteger newBasePrime = bBasePrime.Value * (bPrime);
            long MonicFactor = pRing.Multiply(
                pRing.Reciprocal(modGCD.Lc()),
                (long)(lcGCD % bPrime));


            PairedIterator<BigInteger, long> iterator = new PairedIterator<BigInteger, long>(@base, modGCD);
            ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic =
                ChineseRemainders.CreateMagic(Rings.Z, bBasePrime.Value, bPrime);
            while (iterator.MoveNext())
            {
                Monomial<BigInteger> baseTerm = iterator.aTerm;
                Monomial<long> imageTerm = iterator.bTerm;

                if (baseTerm.coefficient.IsZero)
                    // term is absent in the base
                    continue;

                if (imageTerm.coefficient == 0)
                {
                    // term is absent in the modularGCD => remove it from the base
                    // bBase.subtract(baseTerm);
                    @base.terms.Remove(baseTerm);
                    continue;
                }

                long oth = pRing.Multiply(imageTerm.coefficient, MonicFactor);

                // update base term
                BigInteger newCoeff =
                    ChineseRemainders.ChineseRemainder(Rings.Z, magic, baseTerm.coefficient, new BigInteger(oth));
                @base.Put(baseTerm.SetCoefficient(newCoeff));
            }

            @base = @base.SetRingUnsafe(new IntegersZp(newBasePrime));
            bBasePrime = newBasePrime;

            // two trials didn't change the result, probably we are done
            MultivariatePolynomial<BigInteger> candidate =
                MultivariatePolynomial<BigInteger>.AsPolyZSymmetric(@base).PrimitivePart();
            //first check b since b is less Degree
            if (!isGCDTriplet(b, a, candidate))
                continue;

            return gcdInput.restoreGCD(candidate);
        }
    }

//     /* =============================== Multivariate GCD over algebraic number fields ================================= */
//
//     
//     private static <E> MultivariatePolynomial<UnivariatePolynomial<E>>
//     TrivialGCDInExtension(MultivariatePolynomial<UnivariatePolynomial<E>> a, MultivariatePolynomial<UnivariatePolynomial<E>> b) {
//         AlgebraicNumberField<UnivariatePolynomial<E>> ring
//                 = (AlgebraicNumberField<UnivariatePolynomial<E>>) a.ring;
//
//         if (!a.stream().allMatch(ring::isInTheBaseField)
//                 || !b.stream().allMatch(ring::isInTheBaseField))
//             return null;
//
//         Ring<E> cfRing = ring.getMinimalPolynomial().ring;
//         MultivariatePolynomial<E>
//                 ar = a.mapCoefficients(cfRing, UnivariatePolynomial::cc),
//                 br = b.mapCoefficients(cfRing, UnivariatePolynomial::cc);
//         return PolynomialGCD(ar, br)
//                 .mapCoefficients(ring, cf -> UnivariatePolynomial.constant(cfRing, cf));
//     }
//
//     
//     static BigInteger iContent(MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a) {
//         return Z.gcd(a.stream().flatMap(UnivariatePolynomial::stream).map(Rational::numerator).collect(Collectors.toList()));
//     }
//
//     
//     static BigInteger iDenominator(MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a) {
//         return Z.lcm(a.stream().flatMap(UnivariatePolynomial::stream).map(Rational::denominator).collect(Collectors.toList()));
//     }
//
//     
//     static BigInteger iMax(MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a) {
//         return a.stream()
//                 .flatMap(p -> p.stream().flatMap(cf -> Stream.of(cf.numerator(), cf.denominator())))
//                 .map(Z::abs).max(Z).orElse(Z.getZero());
//     }
//
//     
//     static BigInteger iMaxZ(MultivariatePolynomial<UnivariatePolynomial<BigInteger>> a) {
//         return a.stream()
//                 .flatMap(UnivariatePolynomial::stream)
//                 .map(Z::abs).max(Z).orElse(Z.getZero());
//     }
//
//     /**
//      * Modular algorithm for polynomials over simple field extensions, which switches to "integer" associates of input
//      * polynomials and number field (by applying substitution to minimal polynomial)
//      *
//      * @param a               first poly
//      * @param b               second poly
//      * @param algorithmInRing the algorithm for ring of integers
//      */
//     private static MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>
//     PolynomialGCDInNumberFieldSwitchToRingOfInteger(
//             MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a,
//             MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> b,
//             BiFunction<MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>,
//                     MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>,
//                     MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>> algorithmInRing) {
//
//         // if there is a trivial case
//         MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> simpleGCD = TrivialGCDInExtension(a, b);
//         if (simpleGCD != null)
//             return simpleGCD;
//
//         AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> numberField
//                 = (AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>>) a.ring;
//         UnivariatePolynomial<Rational<BigInteger>> minimalPoly = numberField.getMinimalPolynomial();
//
//         if (minimalPoly.stream().allMatch(Rational::isIntegral)) {
//             // minimal polynomial is integral (Monic)
//
//             a = a.Clone(); b = b.Clone();
//             // remove integer Content (to facilitate rational reconstruction)
//             BigInteger aiCont = iContent(a), biCont = iContent(b);
//             a.Multiply(UnivariatePolynomial.constant(Q, Q.mkDenominator(aiCont)));
//             b.Multiply(UnivariatePolynomial.constant(Q, Q.mkDenominator(biCont)));
//             // to common denominator
//             BigInteger aiDen = iDenominator(a), biDen = iDenominator(b);
//             a.Multiply(UnivariatePolynomial.constant(Q, Q.mkNumerator(aiDen)));
//             b.Multiply(UnivariatePolynomial.constant(Q, Q.mkNumerator(biDen)));
//
//             return algorithmInRing.apply(a, b);
//         } else {
//             // scaling of minimal poly is practically faster than working with rationals
//
//             // replace s -> s / lc(minPoly)
//             BigInteger minPolyLeadCoeff = commonDenominator(minimalPoly);
//             Rational<BigInteger>
//                     scale = new Rational<>(Z, Z.getOne(), minPolyLeadCoeff),
//                     scaleReciprocal = scale.reciprocal();
//
//             // scaled number field
//             AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>>
//                     numberFieldScaled = new AlgebraicNumberField<>(minimalPoly.scale(scale).Monic());
//
//             return PolynomialGCDInNumberFieldSwitchToRingOfInteger(
//                     a.mapCoefficients(numberFieldScaled, cf -> cf.scale(scale)),
//                     b.mapCoefficients(numberFieldScaled, cf -> cf.scale(scale)),
//                     algorithmInRing)
//                     .mapCoefficients(numberField, cf -> cf.scale(scaleReciprocal));
//         }
//     }
//
//     /**
//      * Modular interpolation algorithm for polynomials over simple field extensions by integer with integer
//      * coefficients
//      */
//     private static MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>
//     PolynomialGCDAssociateInRingOfIntegerOfNumberField(
//             MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a,
//             MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> b,
//             BiFunction<
//                     MultivariatePolynomial<UnivariatePolynomial<BigInteger>>,
//                     MultivariatePolynomial<UnivariatePolynomial<BigInteger>>,
//                     MultivariatePolynomial<UnivariatePolynomial<BigInteger>>
//                     > algorithmForGcdAssociate) {
//         assert a.stream().allMatch(cf -> cf.stream().allMatch(Rational::isIntegral));
//         assert b.stream().allMatch(cf -> cf.stream().allMatch(Rational::isIntegral));
//
//         GCDInput<
//                 Monomial<UnivariatePolynomial<Rational<BigInteger>>>,
//                 MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>
//                 > gcdInput = preparedGCDInput(a, b, (l, r) -> PolynomialGCDAssociateInRingOfIntegerOfNumberField(l, r, algorithmForGcdAssociate));
//         if (gcdInput.earlyGCD != null)
//             return gcdInput.earlyGCD;
//
//         a = gcdInput.aReduced;
//         b = gcdInput.bReduced;
//
//         // remove Content (required by Zippel algorithm)
//         MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> pContentGCD =
//                 ContentGCD(a, b, 0, (l, r) -> PolynomialGCDAssociateInRingOfIntegerOfNumberField(l, r, algorithmForGcdAssociate));
//         if (!pContentGCD.IsConstant()) {
//             a = MultivariateDivision.divideExact(a, pContentGCD);
//             b = MultivariateDivision.divideExact(b, pContentGCD);
//             return gcdInput.restoreGCD(
//                     PolynomialGCDInNumberFieldSwitchToRingOfInteger(a, b,
//                             (l, r) -> PolynomialGCDAssociateInRingOfIntegerOfNumberField(l, r, algorithmForGcdAssociate).Multiply(pContentGCD)));
//         }
//
//         AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> numberField
//                 = (AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>>) a.ring;
//         UnivariatePolynomial<BigInteger> minimalPolyZ = numberField.getMinimalPolynomial().mapCoefficients(Z, Rational::numeratorExact);
//         AlgebraicNumberField<UnivariatePolynomial<BigInteger>> numberRingZ = new AlgebraicNumberField<>(minimalPolyZ);
//
//         return gcdInput.restoreGCD(algorithmForGcdAssociate.apply(
//                 a.mapCoefficients(numberRingZ, cf -> cf.mapCoefficients(Z, Rational::numeratorExact)),
//                 b.mapCoefficients(numberRingZ, cf -> cf.mapCoefficients(Z, Rational::numeratorExact)))
//                 .mapCoefficients(numberField, cf -> cf.mapCoefficients(Q, Q::mkNumerator)));
//     }
//
//     /**
//      * Zippel's sparse modular interpolation algorithm for polynomials over simple field extensions with the use of
//      * rational reconstruction to reconstruct the result
//      */
//     public static MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>
//     ZippelGCDInNumberFieldViaRationalReconstruction(
//             MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a,
//             MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> b) {
//         return PolynomialGCDInNumberFieldSwitchToRingOfInteger(a, b,
//                 (l, r) -> PolynomialGCDAssociateInRingOfIntegerOfNumberField(l, r, MultivariateGCD::ZippelGCDInNumberFieldViaRationalReconstruction0));
//     }
//
//     /**
//      * Zippel's sparse modular interpolation algorithm for computing GCD associate for polynomials over simple field
//      * extensions with the use of rational reconstruction to reconstruct the result
//      */
//     private static MultivariatePolynomial<UnivariatePolynomial<BigInteger>>
//     ZippelGCDInNumberFieldViaRationalReconstruction0(
//             MultivariatePolynomial<UnivariatePolynomial<BigInteger>> a,
//             MultivariatePolynomial<UnivariatePolynomial<BigInteger>> b) {
//         // choose better prime to start
//         long startingPrime;
//         if (Math.max(iMaxZ(a).bitLength(), iMaxZ(b).bitLength()) < 256)
//             startingPrime = 1L << 30;
//         else
//             startingPrime = 1L << 60;
//
//         // for efficient division test we prepare polynomials with integer coefficients
//         AlgebraicNumberField<UnivariatePolynomial<BigInteger>>
//                 numberField = (AlgebraicNumberField<UnivariatePolynomial<BigInteger>>) a.ring;
//         UnivariatePolynomial<BigInteger> minimalPoly = numberField.getMinimalPolynomial();
//
//         // auxiliary ring
//         UnivariateRing<UnivariatePolynomial<BigInteger>> auxRing = UnivariateRing(Z);
//         PrimesIEnumerable primesLoop = new PrimesIEnumerable(startingPrime - (1 << 12));
//         Random random = PrivateRandom.GetRandom();
//         main_loop:
//         while (true) {
//             long basePrime = primesLoop.take();
//             assert basePrime != -1 : "long overflow";
//
//             IntegersZp64 baseRing = new IntegersZp64(basePrime);
//             UnivariatePolynomialZp64 minimalPolyMod = UnivariatePolynomial.asOverZp64(minimalPoly, baseRing);
//             FiniteField<UnivariatePolynomialZp64> numberFieldMod = new FiniteField<>(minimalPolyMod);
//
//             // reduce mod p
//             MultivariatePolynomial<UnivariatePolynomialZp64>
//                     aMod = a.mapCoefficients(numberFieldMod, cf -> UnivariatePolynomial.asOverZp64(cf, baseRing)),
//                     bMod = b.mapCoefficients(numberFieldMod, cf -> UnivariatePolynomial.asOverZp64(cf, baseRing));
//             if (!aMod.sameSkeletonQ(a) || !bMod.sameSkeletonQ(b))
//                 continue;
//
//             // the base image
//             MultivariatePolynomial<UnivariatePolynomialZp64> baseMod;
//             try {
//                 baseMod = PolynomialGCD(aMod, bMod).Monic();
//             } catch (Throwable t) { continue; } // bad prime
//
//             // accumulator to update coefficients via Chineese remainding
//             MultivariatePolynomial<UnivariatePolynomial<BigInteger>> base = baseMod
//                     .mapCoefficients(auxRing, cf -> cf.asPolyZ(false).toBigPoly());
//
//             if (base.IsConstant())
//                 return a.CreateOne();
//
//             // accumulated CRT prime
//             BigInteger crtPrime = Z.valueOf(basePrime);
//             // number of already generated primes
//             int nPrimes = 0;
//             // Fibonacci numbers for testing of rational reconstruction
//             int prevFibonacci = 1, nextFibonacci = 2;
//             // over all primes
//             inner_loop:
//             while (true) {
//                 long prime = primesLoop.take();
//                 IntegersZp64 ring = new IntegersZp64(prime);
//
//                 minimalPolyMod = UnivariatePolynomial.asOverZp64(minimalPoly, ring);
//                 numberFieldMod = new FiniteField<>(minimalPolyMod);
//                 // reduce Z -> Zp
//                 aMod = a.mapCoefficients(numberFieldMod, cf -> UnivariatePolynomial.asOverZp64(cf, ring));
//                 bMod = b.mapCoefficients(numberFieldMod, cf -> UnivariatePolynomial.asOverZp64(cf, ring));
//                 if (!aMod.sameSkeletonQ(a) || !bMod.sameSkeletonQ(b))
//                     continue;
//
//                 // calculate new GCD using previously calculated skeleton via sparse interpolation
//                 MultivariatePolynomial<UnivariatePolynomialZp64> modularGCD;
//                 try {
//                     modularGCD = interpolateGCD(aMod, bMod, baseMod.mapCoefficients(numberFieldMod, cf -> cf.setModulusUnsafe(prime)), random);
//                 } catch (Throwable t) {continue;} // unlucky prime
//
//                 if (modularGCD == null) {
//                     // interpolation failed => assumed form is wrong => start over
//                     continue;
//                 }
//
//                 if (modularGCD.IsConstant())
//                     return a.CreateOne();
//
//                 // Monicize modular gcd
//                 modularGCD.Monic();
//
//                 assert MultivariateDivision.DividesQ(aMod, modularGCD);
//                 assert MultivariateDivision.DividesQ(bMod, modularGCD);
//
//                 // better Degree bound found -> start over
//                 if (modularGCD.Degree(0) < base.Degree(0)) {
//                     baseMod = modularGCD;
//                     base = baseMod.mapCoefficients(auxRing, cf -> cf.asPolyZ(false).toBigPoly());
//                     basePrime = prime;
//                     crtPrime = Z.valueOf(basePrime);
//                     continue;
//                 }
//
//                 //skip unlucky prime
//                 if (modularGCD.Degree(0) > base.Degree(0))
//                     continue;
//
//                 // applying CRT
//                 PairedIEnumerable<
//                         Monomial<UnivariatePolynomial<BigInteger>>, MultivariatePolynomial<UnivariatePolynomial<BigInteger>>,
//                         Monomial<UnivariatePolynomialZp64>, MultivariatePolynomial<UnivariatePolynomialZp64>
//                         > iterator = new PairedIEnumerable<>(base, modularGCD);
//                 ChineseRemaindersMagic<BigInteger> magic = CreateMagic(Z, crtPrime, Z.valueOf(prime));
//                 while (iterator.hasNext()) {
//                     iterator.advance();
//
//                     Monomial<UnivariatePolynomial<BigInteger>> baseTerm = iterator.aTerm;
//                     Monomial<UnivariatePolynomialZp64> imageTerm = iterator.bTerm;
//
//                     if (baseTerm.coefficient.IsZero())
//                         // term is absent in the base
//                         continue;
//
//                     if (imageTerm.coefficient.IsZero()) {
//                         // term is absent in the modularGCD => remove it from the base
//                         // bBase.subtract(baseTerm);
//                         iterator.aIEnumerable.remove();
//                         continue;
//                     }
//                     UnivariatePolynomial<BigInteger> baseCf = baseTerm.coefficient;
//                     UnivariatePolynomialZp64 imageCf = imageTerm.coefficient;
//
//                     assert baseCf.ring == Z;
//                     UnivariateGCD.updateCRT(magic, baseCf, imageCf);
//                 }
//                 crtPrime = crtPrime.Multiply(Z.valueOf(prime));
//                 ++nPrimes;
//
//                 // attempt to apply rational reconstruction (quite costly) only for
//                 // sufficiently large number of prime homomorphisms,
//                 // since we don't know a priori any coefficient bounds
//                 //
//                 // We use Fibonacci numbers as in van Hoeij & Monagan "A Modular GCD
//                 // algorithm over Number Fields presented with Multiple Extensions."
//                 if (nPrimes == nextFibonacci) {
//                     int nextNextFibonacci = (prevFibonacci + 1) / 2 + nextFibonacci;
//                     prevFibonacci = nextFibonacci;
//                     nextFibonacci = nextNextFibonacci;
//
//                     // rational reconstruction
//                     BigInteger lcm = Z.getOne();
//                     List<Monomial<UnivariatePolynomial<Rational<BigInteger>>>> candidateTerms = new ArrayList<>();
//                     for (Monomial<UnivariatePolynomial<BigInteger>> term : base.terms) {
//                         UnivariatePolynomial<Rational<BigInteger>> rrCf = rationalReconstruction(term.coefficient, crtPrime);
//                         if (rrCf == null)
//                             continue inner_loop; // rational reconstruction failed
//                         candidateTerms.add(new Monomial<>(term, rrCf));
//                         lcm = Z.lcm(lcm, Z.lcm(rrCf.stream().map(Rational::denominator).collect(Collectors.toList())));
//                     }
//                     final BigInteger lcm0 = lcm;
//                     MultivariatePolynomial<UnivariatePolynomial<BigInteger>> candidate = a.Create(candidateTerms
//                             .stream()
//                             .map(m -> new Monomial<>(m, m.coefficient.mapCoefficients(Z, cf -> cf.Multiply(lcm0).numeratorExact())))
//                             .collect(Collectors.toList()));
//
//
//                     // test candidate with pseudoD division
//                     if (pseudoRemainder(a, candidate).IsZero()
//                             && pseudoRemainder(b, candidate).IsZero())
//                         return candidate;
//                 }
//             }
//         }
//     }
//
//     
//     private static UnivariatePolynomial<Rational<BigInteger>>
//     rationalReconstruction(UnivariatePolynomial<BigInteger> base, BigInteger crtPrime) {
//         UnivariatePolynomial<Rational<BigInteger>> candidate = UnivariatePolynomial.zero(Q);
//         for (int j = 0; j <= base.Degree(); ++j) {
//             BigInteger[] numDen = RationalReconstruction.reconstructFarey(base.get(j), crtPrime);
//             if (numDen == null)
//                 return null;
//             candidate.set(j, new Rational<>(Z, numDen[0], numDen[1]));
//         }
//         return candidate;
//     }
//
//     /**
//      * Zippel's sparse modular interpolation algorithm for computing GCD associate for polynomials over simple field
//      * extensions with the use of Langemyr & McCallum approach to avoid rational reconstruction
//      */
//     public static MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>
//     ZippelGCDInNumberFieldViaLangemyrMcCallum(
//             MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a,
//             MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> b) {
//         return PolynomialGCDInNumberFieldSwitchToRingOfInteger(a, b,
//                 (l, r) -> PolynomialGCDAssociateInRingOfIntegerOfNumberField(l, r,
//                         (u, v) -> PolynomialGCDAssociateInNumberFieldViaLangemyrMcCallum(u, v,
//                                 MultivariateGCD::ZippelGCDAssociateInNumberFieldViaLangemyrMcCallum0)));
//     }
//
//     
//     @SuppressWarnings("ConstantConditions")
//     private static MultivariatePolynomial<UnivariatePolynomial<BigInteger>>
//     PolynomialGCDAssociateInNumberFieldViaLangemyrMcCallum(
//             MultivariatePolynomial<UnivariatePolynomial<BigInteger>> a,
//             MultivariatePolynomial<UnivariatePolynomial<BigInteger>> b,
//             BiFunction<
//                     MultivariatePolynomial<UnivariatePolynomial<BigInteger>>,
//                     MultivariatePolynomial<UnivariatePolynomial<BigInteger>>,
//                     MultivariatePolynomial<UnivariatePolynomial<BigInteger>>
//                     > baseAlgorithm) {
//
//         AlgebraicNumberField<UnivariatePolynomial<BigInteger>> numberField
//                 = (AlgebraicNumberField<UnivariatePolynomial<BigInteger>>) a.ring;
//
//         integerPrimitivePart(a);
//         integerPrimitivePart(b);
//
//         if (!a.Lc().IsConstant())
//             a.Multiply(numberField.normalizer(a.Lc()));
//
//         if (!b.Lc().IsConstant())
//             b.Multiply(numberField.normalizer(b.Lc()));
//
//         integerPrimitivePart(a);
//         integerPrimitivePart(b);
//
//         // if all coefficients are simple numbers (no algebraic elements)
//         MultivariatePolynomial<UnivariatePolynomial<BigInteger>> simpleGCD = TrivialGCDInExtension(a, b);
//         if (simpleGCD != null)
//             return simpleGCD;
//
//         return baseAlgorithm.apply(a, b);
//     }
//
//     static void integerPrimitivePart(MultivariatePolynomial<UnivariatePolynomial<BigInteger>> p) {
//         BigInteger gcd = Z.gcd(p.stream().flatMap(UnivariatePolynomial::stream).sorted().collect(Collectors.toList()));
//         p.stream().forEach(cf -> cf.divideExact(gcd));
//     }
//
//     /**
//      * Zippel's sparse modular interpolation algorithm for polynomials over simple field extensions with Langemyr &
//      * McCallum correction for lead coefficients to avoid rational reconstruction
//      */
//     private static MultivariatePolynomial<UnivariatePolynomial<BigInteger>>
//     ZippelGCDAssociateInNumberFieldViaLangemyrMcCallum0(
//             MultivariatePolynomial<UnivariatePolynomial<BigInteger>> a,
//             MultivariatePolynomial<UnivariatePolynomial<BigInteger>> b) {
//
//         assert a.Lc().IsConstant();
//         assert b.Lc().IsConstant();
//         // choose better prime for start
//         long startingPrime;
//         if (Math.max(iMaxZ(a).bitLength(), iMaxZ(b).bitLength()) < 2 * 256)
//             startingPrime = 1L << 30;
//         else
//             startingPrime = 1L << 60;
//
//         AlgebraicNumberField<UnivariatePolynomial<BigInteger>> numberField
//                 = (AlgebraicNumberField<UnivariatePolynomial<BigInteger>>) a.ring;
//         UnivariatePolynomial<BigInteger> minimalPoly = numberField.getMinimalPolynomial();
//
//         // Weinberger & Rothschild (1976) correction denominator
//         BigInteger
//                 lcGCD = Z.gcd(a.Lc().cc(), b.Lc().cc()),
//                 disc = UnivariateResultants.Discriminant(minimalPoly),
//                 correctionFactor = disc.pow(1).Multiply(lcGCD);
//
//         UnivariateRing<UnivariatePolynomial<BigInteger>> auxRing = UnivariateRing(Z);
//         PrimesIEnumerable primesLoop = new PrimesIEnumerable(startingPrime - (1 << 12));
//         Random random = PrivateRandom.GetRandom();
//         while (true) {
//             // prepare the skeleton
//             long basePrime = primesLoop.take();
//             assert basePrime != -1 : "long overflow";
//
//             IntegersZp64 baseRing = new IntegersZp64(basePrime);
//             UnivariatePolynomialZp64 minimalPolyMod = UnivariatePolynomial.asOverZp64(minimalPoly, baseRing);
//             FiniteField<UnivariatePolynomialZp64> numberFieldMod = new FiniteField<>(minimalPolyMod);
//
//             // reduce Z -> Zp
//             MultivariatePolynomial<UnivariatePolynomialZp64>
//                     aMod = a.mapCoefficients(numberFieldMod, cf -> UnivariatePolynomial.asOverZp64(cf, baseRing)),
//                     bMod = b.mapCoefficients(numberFieldMod, cf -> UnivariatePolynomial.asOverZp64(cf, baseRing));
//             if (!aMod.sameSkeletonQ(a) || !bMod.sameSkeletonQ(b))
//                 continue;
//
//             // the base image
//             MultivariatePolynomial<UnivariatePolynomialZp64> baseMod;
//             try {
//                 baseMod = PolynomialGCD(aMod, bMod).Monic();
//             } catch (Throwable t) { continue;} // bad prime
//
//             // correction
//             baseMod.Monic(numberFieldMod.valueOf(correctionFactor.mod(basePrime).longValueExact()));
//             // accumulator to update coefficients via Chineese remainding
//             MultivariatePolynomial<UnivariatePolynomial<BigInteger>> base = baseMod
//                     .mapCoefficients(auxRing, cf -> cf.asPolyZ(false).toBigPoly());
//
//             if (base.IsConstant())
//                 return a.CreateOne();
//
//             // accumulated CRT prime
//             BigInteger crtPrime = Z.valueOf(basePrime);
//             // previous candidate to test
//             MultivariatePolynomial<UnivariatePolynomial<BigInteger>> prevCandidate = null;
//             // over all primes
//             inner_loop:
//             while (true) {
//                 long prime = primesLoop.take();
//                 IntegersZp64 ring = new IntegersZp64(prime);
//
//                 minimalPolyMod = UnivariatePolynomial.asOverZp64(minimalPoly, ring);
//                 numberFieldMod = new FiniteField<>(minimalPolyMod);
//                 // reduce Z -> Zp
//                 aMod = a.mapCoefficients(numberFieldMod, cf -> UnivariatePolynomial.asOverZp64(cf, ring));
//                 bMod = b.mapCoefficients(numberFieldMod, cf -> UnivariatePolynomial.asOverZp64(cf, ring));
//                 if (!aMod.sameSkeletonQ(a) || !bMod.sameSkeletonQ(b))
//                     continue;
//
//                 // calculate new GCD using previously calculated skeleton via sparse interpolation
//                 MultivariatePolynomial<UnivariatePolynomialZp64> modularGCD;
//                 try {
//                     modularGCD = interpolateGCD(aMod, bMod, baseMod.mapCoefficients(numberFieldMod, cf -> cf.setModulusUnsafe(prime)), random);
//                 } catch (Throwable t) {continue;} // unlucky prime
//
//                 if (modularGCD == null) {
//                     // interpolation failed => assumed form is wrong => start over
//                     continue;
//                 }
//
//                 // correction
//                 modularGCD.Monic(numberFieldMod.valueOf(correctionFactor.mod(prime).longValueExact()));
//
//                 assert MultivariateDivision.DividesQ(aMod, modularGCD);
//                 assert MultivariateDivision.DividesQ(bMod, modularGCD);
//
//                 if (modularGCD.IsConstant())
//                     return a.CreateOne();
//
//                 // better Degree bound found -> start over
//                 if (modularGCD.Degree(0) < base.Degree(0)) {
//                     baseMod = modularGCD;
//                     prevCandidate = null;
//                     base = baseMod.mapCoefficients(auxRing, cf -> cf.asPolyZ(false).toBigPoly());
//                     basePrime = prime;
//                     crtPrime = Z.valueOf(basePrime);
//                     continue;
//                 }
//
//                 //skip unlucky prime
//                 if (modularGCD.Degree(0) > base.Degree(0))
//                     continue;
//
//                 ChineseRemaindersMagic<BigInteger> magic = CreateMagic(Z, crtPrime, Z.valueOf(prime));
//                 //lifting
//                 PairedIEnumerable<
//                         Monomial<UnivariatePolynomial<BigInteger>>, MultivariatePolynomial<UnivariatePolynomial<BigInteger>>,
//                         Monomial<UnivariatePolynomialZp64>, MultivariatePolynomial<UnivariatePolynomialZp64>
//                         > iterator = new PairedIEnumerable<>(base, modularGCD);
//                 while (iterator.hasNext()) {
//                     iterator.advance();
//
//                     Monomial<UnivariatePolynomial<BigInteger>> baseTerm = iterator.aTerm;
//                     Monomial<UnivariatePolynomialZp64> imageTerm = iterator.bTerm;
//
//                     if (baseTerm.coefficient.IsZero())
//                         // term is absent in the base
//                         continue;
//
//                     if (imageTerm.coefficient.IsZero()) {
//                         // term is absent in the modularGCD => remove it from the base
//                         // bBase.subtract(baseTerm);
//                         iterator.aIEnumerable.remove();
//                         continue;
//                     }
//                     UnivariatePolynomial<BigInteger> baseCf = baseTerm.coefficient;
//                     UnivariatePolynomialZp64 imageCf = imageTerm.coefficient;
//
//                     assert baseCf.ring == Z;
//                     UnivariateGCD.updateCRT(magic, baseCf, imageCf);
//                 }
//                 crtPrime = crtPrime.Multiply(Z.valueOf(prime));
//
//                 IntegersZp crtRing = new IntegersZp(crtPrime);
//                 MultivariatePolynomial<UnivariatePolynomial<BigInteger>>
//                         candidate = base.mapCoefficients(numberField,
//                         cf -> numberField.valueOf(UnivariatePolynomial.asPolyZSymmetric(cf.setRingUnsafe(crtRing))));
//
//                 if (prevCandidate == null) {
//                     prevCandidate = candidate;
//                     continue;
//                 }
//
//                 if (prevCandidate.equals(candidate)) {
//                     // division test
//                     if (pseudoRemainder(a, candidate).IsZero() && pseudoRemainder(b, candidate).IsZero())
//                         return candidate;
//                 }
//                 prevCandidate = candidate;
//             }
//         }
//     }
//
//     /**
//      * Modular interpolation algorithm for polynomials over simple field extensions with the use of Langemyr & McCallum
//      * approach to avoid rational reconstruction
//      */
//     public static MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>
//     ModularGCDInNumberFieldViaRationalReconstruction(
//             MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a,
//             MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> b,
//             BiFunction<
//                     MultivariatePolynomial<UnivariatePolynomialZp64>,
//                     MultivariatePolynomial<UnivariatePolynomialZp64>,
//                     MultivariatePolynomial<UnivariatePolynomialZp64>
//                     > modularAlgorithm) {
//         return PolynomialGCDInNumberFieldSwitchToRingOfInteger(a, b,
//                 (l, r) -> PolynomialGCDAssociateInRingOfIntegerOfNumberField(l, r,
//                         (u, v) -> ModularGCDInNumberFieldViaRationalReconstruction0(u, v, modularAlgorithm)));
//     }
//
//     /**
//      * <=odular interpolation algorithm for polynomials over simple field extensions with the use of rational
//      * reconstruction to reconstruct the result
//      */
//     private static MultivariatePolynomial<UnivariatePolynomial<BigInteger>>
//     ModularGCDInNumberFieldViaRationalReconstruction0(
//             MultivariatePolynomial<UnivariatePolynomial<BigInteger>> a,
//             MultivariatePolynomial<UnivariatePolynomial<BigInteger>> b,
//             BiFunction<
//                     MultivariatePolynomial<UnivariatePolynomialZp64>,
//                     MultivariatePolynomial<UnivariatePolynomialZp64>,
//                     MultivariatePolynomial<UnivariatePolynomialZp64>
//                     > modularAlgorithm) {
//         // choose better prime to start
//         long startingPrime;
//         if (Math.max(iMaxZ(a).bitLength(), iMaxZ(b).bitLength()) < 256)
//             startingPrime = 1L << 30;
//         else
//             startingPrime = 1L << 60;
//
//         // for efficient division test we prepare polynomials with integer coefficients
//         AlgebraicNumberField<UnivariatePolynomial<BigInteger>>
//                 numberField = (AlgebraicNumberField<UnivariatePolynomial<BigInteger>>) a.ring;
//         UnivariatePolynomial<BigInteger> minimalPoly = numberField.getMinimalPolynomial();
//
//         // auxiliary ring
//         UnivariateRing<UnivariatePolynomial<BigInteger>> auxRing = UnivariateRing(Z);
//         PrimesIEnumerable primesLoop = new PrimesIEnumerable(startingPrime - (1 << 12));
//
//         // accumulator to update coefficients via Chineese remainding
//         MultivariatePolynomial<UnivariatePolynomial<BigInteger>> base = null;
//         // accumulated CRT prime
//         BigInteger crtPrime = null;
//         // number of already generated primes
//         int nPrimes = 0;
//         // Fibonacci numbers for testing of rational reconstruction
//         int prevFibonacci = 1, nextFibonacci = 2;
//         // over all primes
//         main_loop:
//         while (true) {
//             long prime = primesLoop.take();
//             IntegersZp64 ring = new IntegersZp64(prime);
//             UnivariatePolynomialZp64 minimalPolyMod = UnivariatePolynomial.asOverZp64(minimalPoly, ring);
//             FiniteField<UnivariatePolynomialZp64> numberFieldMod = new FiniteField<>(minimalPolyMod);
//
//             // reduce Z -> Zp
//             MultivariatePolynomial<UnivariatePolynomialZp64>
//                     aMod = a.mapCoefficients(numberFieldMod, cf -> UnivariatePolynomial.asOverZp64(cf, ring)),
//                     bMod = b.mapCoefficients(numberFieldMod, cf -> UnivariatePolynomial.asOverZp64(cf, ring));
//             if (!aMod.sameSkeletonQ(a) || !bMod.sameSkeletonQ(b))
//                 continue;
//
//             // calculate new GCD using previously calculated skeleton via sparse interpolation
//             MultivariatePolynomial<UnivariatePolynomialZp64> modularGCD;
//             try {
//                 modularGCD = modularAlgorithm.apply(aMod, bMod);
//             } catch (Throwable t) {continue;} // unlucky prime
//
//             if (modularGCD == null) {
//                 // interpolation failed => assumed form is wrong => start over
//                 continue;
//             }
//
//             if (modularGCD.IsConstant())
//                 return a.CreateOne();
//
//             // Monicize modular gcd
//             modularGCD.Monic();
//
//             assert MultivariateDivision.DividesQ(aMod, modularGCD);
//             assert MultivariateDivision.DividesQ(bMod, modularGCD);
//
//             // better Degree bound found -> start over
//             if (base == null || modularGCD.Degree(0) < base.Degree(0)) {
//                 base = modularGCD.mapCoefficients(auxRing, cf -> cf.asPolyZ(false).toBigPoly());
//                 crtPrime = Z.valueOf(prime);
//                 continue;
//             }
//
//             //skip unlucky prime
//             if (modularGCD.Degree(0) > base.Degree(0))
//                 continue;
//
//             // applying CRT
//             PairedIEnumerable<
//                     Monomial<UnivariatePolynomial<BigInteger>>, MultivariatePolynomial<UnivariatePolynomial<BigInteger>>,
//                     Monomial<UnivariatePolynomialZp64>, MultivariatePolynomial<UnivariatePolynomialZp64>
//                     > iterator = new PairedIEnumerable<>(base, modularGCD);
//             ChineseRemaindersMagic<BigInteger> magic = CreateMagic(Z, crtPrime, Z.valueOf(prime));
//             while (iterator.hasNext()) {
//                 iterator.advance();
//
//                 Monomial<UnivariatePolynomial<BigInteger>> baseTerm = iterator.aTerm;
//                 Monomial<UnivariatePolynomialZp64> imageTerm = iterator.bTerm;
//
//                 if (baseTerm.coefficient.IsZero())
//                     // term is absent in the base
//                     continue;
//
//                 if (imageTerm.coefficient.IsZero()) {
//                     // term is absent in the modularGCD => remove it from the base
//                     // bBase.subtract(baseTerm);
//                     iterator.aIEnumerable.remove();
//                     continue;
//                 }
//                 UnivariatePolynomial<BigInteger> baseCf = baseTerm.coefficient;
//                 UnivariatePolynomialZp64 imageCf = imageTerm.coefficient;
//
//                 assert baseCf.ring == Z;
//                 UnivariateGCD.updateCRT(magic, baseCf, imageCf);
//             }
//             crtPrime = crtPrime.Multiply(Z.valueOf(prime));
//             ++nPrimes;
//
//             // attempt to apply rational reconstruction (quite costly) only for
//             // sufficiently large number of prime homomorphisms,
//             // since we don't know a priori any coefficient bounds
//             //
//             // We use Fibonacci numbers as in van Hoeij & Monagan "A Modular GCD
//             // algorithm over Number Fields presented with Multiple Extensions."
//             if (nPrimes == nextFibonacci) {
//                 int nextNextFibonacci = (prevFibonacci + 1) / 2 + nextFibonacci;
//                 prevFibonacci = nextFibonacci;
//                 nextFibonacci = nextNextFibonacci;
//
//                 // rational reconstruction
//                 BigInteger lcm = Z.getOne();
//                 List<Monomial<UnivariatePolynomial<Rational<BigInteger>>>> candidateTerms = new ArrayList<>();
//                 for (Monomial<UnivariatePolynomial<BigInteger>> term : base.terms) {
//                     UnivariatePolynomial<Rational<BigInteger>> rrCf = rationalReconstruction(term.coefficient, crtPrime);
//                     if (rrCf == null)
//                         continue main_loop; // rational reconstruction failed
//                     candidateTerms.add(new Monomial<>(term, rrCf));
//                     lcm = Z.lcm(lcm, Z.lcm(rrCf.stream().map(Rational::denominator).collect(Collectors.toList())));
//                 }
//                 final BigInteger lcm0 = lcm;
//                 MultivariatePolynomial<UnivariatePolynomial<BigInteger>> candidate = a.Create(candidateTerms
//                         .stream()
//                         .map(m -> new Monomial<>(m, m.coefficient.mapCoefficients(Z, cf -> cf.Multiply(lcm0).numeratorExact())))
//                         .collect(Collectors.toList()));
//
//
//                 // test candidate with pseudoD division
//                 if (pseudoRemainder(a, candidate).IsZero()
//                         && pseudoRemainder(b, candidate).IsZero())
//                     return candidate;
//             }
//         }
//     }
//
//     /**
//      * Zippel's sparse modular interpolation algorithm for polynomials over simple field extensions with the use of
//      * Langemyr & McCallum approach to avoid rational reconstruction
//      */
//     public static MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>
//     ModularGCDInNumberFieldViaLangemyrMcCallum(
//             MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a,
//             MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> b,
//             BiFunction<
//                     MultivariatePolynomial<UnivariatePolynomialZp64>,
//                     MultivariatePolynomial<UnivariatePolynomialZp64>,
//                     MultivariatePolynomial<UnivariatePolynomialZp64>
//                     > modularAlgorithm) {
//         return PolynomialGCDInNumberFieldSwitchToRingOfInteger(a, b,
//                 (l, r) -> PolynomialGCDAssociateInRingOfIntegerOfNumberField(l, r,
//                         (u, v) -> PolynomialGCDAssociateInNumberFieldViaLangemyrMcCallum(u, v,
//                                 (s, t) -> ModularGCDAssociateInNumberFieldViaLangemyrMcCallum0(s, t, modularAlgorithm))));
//     }
//
//     /**
//      * Zippel's sparse modular interpolation algorithm for polynomials over simple field extensions with Langemyr &
//      * McCallum correction for lead coefficients to avoid rational reconstruction
//      */
//     private static MultivariatePolynomial<UnivariatePolynomial<BigInteger>>
//     ModularGCDAssociateInNumberFieldViaLangemyrMcCallum0(
//             MultivariatePolynomial<UnivariatePolynomial<BigInteger>> a,
//             MultivariatePolynomial<UnivariatePolynomial<BigInteger>> b,
//             BiFunction<
//                     MultivariatePolynomial<UnivariatePolynomialZp64>,
//                     MultivariatePolynomial<UnivariatePolynomialZp64>,
//                     MultivariatePolynomial<UnivariatePolynomialZp64>
//                     > modularAlgorithm) {
//
//         assert a.Lc().IsConstant();
//         assert b.Lc().IsConstant();
//         // choose better prime for start
//         long startingPrime;
//         if (Math.max(iMaxZ(a).bitLength(), iMaxZ(b).bitLength()) < 2 * 256)
//             startingPrime = 1L << 30;
//         else
//             startingPrime = 1L << 60;
//
//         AlgebraicNumberField<UnivariatePolynomial<BigInteger>> numberField
//                 = (AlgebraicNumberField<UnivariatePolynomial<BigInteger>>) a.ring;
//         UnivariatePolynomial<BigInteger> minimalPoly = numberField.getMinimalPolynomial();
//
//         // Weinberger & Rothschild (1976) correction denominator
//         BigInteger
//                 lcGCD = Z.gcd(a.Lc().cc(), b.Lc().cc()),
//                 disc = UnivariateResultants.Discriminant(minimalPoly),
//                 correctionFactor = disc.pow(1).Multiply(lcGCD);
//
//         UnivariateRing<UnivariatePolynomial<BigInteger>> auxRing = UnivariateRing(Z);
//         PrimesIEnumerable primesLoop = new PrimesIEnumerable(startingPrime - (1 << 12));
//
//         // accumulator to update coefficients via Chineese remainding
//         MultivariatePolynomial<UnivariatePolynomial<BigInteger>> base = null;
//         // accumulated CRT prime
//         BigInteger crtPrime = null;
//         // previous candidate to test
//         MultivariatePolynomial<UnivariatePolynomial<BigInteger>> prevCandidate = null;
//         // over all primes
//         while (true) {
//             long prime = primesLoop.take();
//             IntegersZp64 ring = new IntegersZp64(prime);
//             UnivariatePolynomialZp64 minimalPolyMod = UnivariatePolynomial.asOverZp64(minimalPoly, ring);
//             FiniteField<UnivariatePolynomialZp64> numberFieldMod = new FiniteField<>(minimalPolyMod);
//
//             minimalPolyMod = UnivariatePolynomial.asOverZp64(minimalPoly, ring);
//             numberFieldMod = new FiniteField<>(minimalPolyMod);
//             // reduce Z -> Zp
//             MultivariatePolynomial<UnivariatePolynomialZp64>
//                     aMod = a.mapCoefficients(numberFieldMod, cf -> UnivariatePolynomial.asOverZp64(cf, ring)),
//                     bMod = b.mapCoefficients(numberFieldMod, cf -> UnivariatePolynomial.asOverZp64(cf, ring));
//             if (!aMod.sameSkeletonQ(a) || !bMod.sameSkeletonQ(b))
//                 continue;
//
//             // calculate new GCD using previously calculated skeleton via sparse interpolation
//             MultivariatePolynomial<UnivariatePolynomialZp64> modularGCD;
//             try {
//                 modularGCD = modularAlgorithm.apply(aMod, bMod).Monic();
//             } catch (Throwable t) {continue;} // unlucky prime
//
//             if (modularGCD == null) {
//                 // interpolation failed => assumed form is wrong => start over
//                 continue;
//             }
//
//             // correction
//             modularGCD.Monic(numberFieldMod.valueOf(correctionFactor.mod(prime).longValueExact()));
//
//             assert MultivariateDivision.DividesQ(aMod, modularGCD);
//             assert MultivariateDivision.DividesQ(bMod, modularGCD);
//
//             if (modularGCD.IsConstant())
//                 return a.CreateOne();
//
//             // better Degree bound found -> start over
//             if (base == null || modularGCD.Degree(0) < base.Degree(0)) {
//                 prevCandidate = null;
//                 base = modularGCD.mapCoefficients(auxRing, cf -> cf.asPolyZ(false).toBigPoly());
//                 crtPrime = Z.valueOf(prime);
//                 continue;
//             }
//
//             //skip unlucky prime
//             if (modularGCD.Degree(0) > base.Degree(0))
//                 continue;
//
//             ChineseRemaindersMagic<BigInteger> magic = CreateMagic(Z, crtPrime, Z.valueOf(prime));
//             //lifting
//             PairedIEnumerable<
//                     Monomial<UnivariatePolynomial<BigInteger>>, MultivariatePolynomial<UnivariatePolynomial<BigInteger>>,
//                     Monomial<UnivariatePolynomialZp64>, MultivariatePolynomial<UnivariatePolynomialZp64>
//                     > iterator = new PairedIEnumerable<>(base, modularGCD);
//             while (iterator.hasNext()) {
//                 iterator.advance();
//
//                 Monomial<UnivariatePolynomial<BigInteger>> baseTerm = iterator.aTerm;
//                 Monomial<UnivariatePolynomialZp64> imageTerm = iterator.bTerm;
//
//                 if (baseTerm.coefficient.IsZero())
//                     // term is absent in the base
//                     continue;
//
//                 if (imageTerm.coefficient.IsZero()) {
//                     // term is absent in the modularGCD => remove it from the base
//                     // bBase.subtract(baseTerm);
//                     iterator.aIEnumerable.remove();
//                     continue;
//                 }
//                 UnivariatePolynomial<BigInteger> baseCf = baseTerm.coefficient;
//                 UnivariatePolynomialZp64 imageCf = imageTerm.coefficient;
//
//                 assert baseCf.ring == Z;
//                 UnivariateGCD.updateCRT(magic, baseCf, imageCf);
//             }
//             crtPrime = crtPrime.Multiply(Z.valueOf(prime));
//
//             IntegersZp crtRing = new IntegersZp(crtPrime);
//             MultivariatePolynomial<UnivariatePolynomial<BigInteger>>
//                     candidate = base.mapCoefficients(numberField,
//                     cf -> numberField.valueOf(UnivariatePolynomial.asPolyZSymmetric(cf.setRingUnsafe(crtRing))));
//
//             if (prevCandidate == null) {
//                 prevCandidate = candidate;
//                 continue;
//             }
//
//             if (prevCandidate.equals(candidate)) {
//                 // division test
//                 if (pseudoRemainder(a, candidate).IsZero() && pseudoRemainder(b, candidate).IsZero())
//                     return candidate;
//             }
//             prevCandidate = candidate;
//         }
//     }
//
//
//     /* ======================== Multivariate GCD over finite fields with small cardinality ========================== */


    public static MultivariatePolynomial<E> KaltofenMonaganSparseModularGCDInGF<E>(
        MultivariatePolynomial<E> a,
        MultivariatePolynomial<E> b)
    {
        return KaltofenMonaganModularGCDInGF(a, b, MultivariateGCD.KaltofenMonaganSparseModularGCDInGF0);
    }

    public static MultivariatePolynomial<E> KaltofenMonaganEEZModularGCDInGF<E>(
        MultivariatePolynomial<E> a,
        MultivariatePolynomial<E> b)
    {
        return KaltofenMonaganModularGCDInGF(a, b, MultivariateGCD.KaltofenMonaganEEZModularGCDInGF0);
    }

    public delegate MultivariatePolynomial<UnivariatePolynomial<uE>> KaltofenMonaganAlgorithm<uE>(
        MultivariatePolynomial<UnivariatePolynomial<uE>> a, MultivariatePolynomial<UnivariatePolynomial<uE>> b,
        int uDegreeBound, int finiteExtensionDegree);


    static MultivariatePolynomial<E> KaltofenMonaganModularGCDInGF<E>(
        MultivariatePolynomial<E> a,
        MultivariatePolynomial<E> b,
        KaltofenMonaganAlgorithm<E> algorithm)
    {
        Util.EnsureOverFiniteField(a, b);
        a.AssertSameCoefficientRingWith(b);

        if (CanConvertToZp64(a))
            return ConvertFromZp64<E>(KaltofenMonaganModularGCDInGF(AsOverZp64(a), AsOverZp64(b),
                algorithm as KaltofenMonaganAlgorithm<long>));

        if (a == b)
            return a.Clone();
        if (a.IsZero()) return b.Clone();
        if (b.IsZero()) return a.Clone();

        if (a.Degree() < b.Degree())
            return KaltofenMonaganModularGCDInGF(b, a, algorithm);

        GCDInput<E>
            gcdInput = preparedGCDInput(a, b, (u, v) => KaltofenMonaganModularGCDInGF(u, v, algorithm));
        if (gcdInput.earlyGCD != null)
            return gcdInput.earlyGCD;

        return KaltofenMonaganModularGCDInGF(gcdInput, algorithm);
    }

    private static MultivariatePolynomial<E> KaltofenMonaganSparseModularGCDInGF<E>(
        GCDInput<E> gcdInput)
    {
        return KaltofenMonaganModularGCDInGF(gcdInput, MultivariateGCD.KaltofenMonaganSparseModularGCDInGF0);
    }


    private static MultivariatePolynomial<E> KaltofenMonaganModularGCDInGF<E>(
        GCDInput<E> gcdInput,
        KaltofenMonaganAlgorithm<E> algorithm)
    {
        MultivariatePolynomial<E> a = gcdInput.aReduced;
        MultivariatePolynomial<E> b = gcdInput.bReduced;

        MultivariatePolynomial<E> pContentGCD =
            ContentGCD(a, b, 0, (u, v) => KaltofenMonaganModularGCDInGF(u, v, algorithm));
        if (!pContentGCD.IsConstant())
        {
            a = MultivariateDivision.DivideExact(a, pContentGCD);
            b = MultivariateDivision.DivideExact(b, pContentGCD);
            return gcdInput.restoreGCD(KaltofenMonaganModularGCDInGF(a, b, algorithm).Multiply(pContentGCD));
        }

        var algorithmZp = algorithm as KaltofenMonaganAlgorithm<long>;

        for (int uVariable = a.nVariables - 1; uVariable >= 0; --uVariable)
            if (a.ring is IntegersZp && a.CoefficientRingCardinality().Value.IsLong())
            {
                // use machine integers

                MultivariatePolynomial<UnivariatePolynomialZp64>
                    ua = AsOverUnivariate0(a as MultivariatePolynomial<BigInteger>, uVariable),
                    ub = AsOverUnivariate0(b as MultivariatePolynomial<BigInteger>, uVariable);

                UnivariatePolynomialZp64 aContent = ua.Content(), bContent = ub.Content();
                UnivariatePolynomialZp64 ContentGCD = ua.ring.Gcd(aContent, bContent);

                ua = ua.DivideOrNull(aContent);
                ub = ub.DivideOrNull(bContent);


                MultivariatePolynomial<UnivariatePolynomialZp64> ugcd =
                    algorithmZp(ua, ub, gcdInput.DegreeBounds[uVariable], gcdInput.finiteExtensionDegree);
                if (ugcd == null)
                    continue;
                ugcd = ugcd.Multiply(ContentGCD);


                MultivariatePolynomial<E> r =
                    gcdInput.restoreGCD(AsNormalMultivariate0(ugcd, uVariable) as MultivariatePolynomial<E>);
                return r;
            }
            else
            {
                MultivariatePolynomial<UnivariatePolynomial<E>>
                    ua = a.AsOverUnivariateEliminate(uVariable),
                    ub = b.AsOverUnivariateEliminate(uVariable);

                UnivariatePolynomial<E> aContent = ua.Content(), bContent = ub.Content();
                UnivariatePolynomial<E> ContentGCD = ua.ring.Gcd(aContent, bContent);

                ua = ua.DivideOrNull(aContent);
                ub = ub.DivideOrNull(bContent);

                MultivariatePolynomial<UnivariatePolynomial<E>> ugcd =
                    algorithm(ua, ub, gcdInput.DegreeBounds[uVariable], gcdInput.finiteExtensionDegree);
                if (ugcd == null)
                    continue;
                ugcd = ugcd.Multiply(ContentGCD);

                return gcdInput.restoreGCD(MultivariatePolynomial<E>.AsNormalMultivariate(ugcd, uVariable));
            }

        throw new Exception();
    }

    private static MultivariatePolynomial<UnivariatePolynomialZp64> AsOverUnivariate0(
        MultivariatePolynomial<BigInteger> poly, int variable)
    {
        IntegersZp64 ring = new IntegersZp64((long)((IntegersZp)poly.ring).modulus);
        UnivariatePolynomialZp64 factory = UnivariatePolynomialZp64.Zero(ring);
        UnivariateRing<long> pDomain = new UnivariateRing<long>(factory);
        MonomialSet<UnivariatePolynomialZp64> newData = new MonomialSet<UnivariatePolynomialZp64>(poly.ordering);
        foreach (Monomial<BigInteger> e in poly.terms)
        {
            MultivariatePolynomial<UnivariatePolynomialZp64>.Add(newData, new Monomial<UnivariatePolynomialZp64>(
                    e.Without(variable),
                    factory.CreateMonomial((long)e.coefficient, e.exponents[variable])),
                pDomain);
        }

        return new MultivariatePolynomial<UnivariatePolynomialZp64>(poly.nVariables - 1, pDomain, poly.ordering,
            newData);
    }

    private static MultivariatePolynomial<BigInteger> AsNormalMultivariate0(
        MultivariatePolynomial<UnivariatePolynomialZp64> poly, int variable)
    {
        Ring<BigInteger> ring = ((IntegersZp64)poly.ring.GetZero().ring).AsGenericRing();
        int nVariables = poly.nVariables + 1;
        MultivariatePolynomial<BigInteger> result =
            MultivariatePolynomial<BigInteger>.Zero(nVariables, ring, poly.ordering);
        foreach (Monomial<UnivariatePolynomialZp64> entry in poly.terms)
        {
            UnivariatePolynomialZp64 uPoly = entry.coefficient;
            DegreeVector dv = entry.DvInsert(variable);
            for (int i = 0; i <= uPoly.Degree(); ++i)
            {
                if (uPoly.IsZeroAt(i))
                    continue;
                result.Add(new Monomial<BigInteger>(dv.DvSet(variable, i), new BigInteger(uPoly.Get(i))));
            }
        }

        return result;
    }

    public static MultivariatePolynomialZp64 KaltofenMonaganEEZModularGCDInGF(
        MultivariatePolynomialZp64 a,
        MultivariatePolynomialZp64 b)
    {
        return KaltofenMonaganModularGCDInGF(a, b, MultivariateGCD.KaltofenMonaganEEZModularGCDInGF0);
    }

    public static MultivariatePolynomialZp64 KaltofenMonaganModularGCDInGF(
        MultivariatePolynomialZp64 a,
        MultivariatePolynomialZp64 b,
        KaltofenMonaganAlgorithm<long> algorithm)
    {
        Util.EnsureOverFiniteField(a, b);
        if (a == b)
            return a.Clone();
        if (a.IsZero()) return b.Clone();
        if (b.IsZero()) return a.Clone();

        if (a.Degree() < b.Degree())
            return KaltofenMonaganModularGCDInGF(b, a, algorithm);

        GCDInput<long>
            gcdInput = preparedGCDInput(a, b, (u, v) => KaltofenMonaganModularGCDInGF(u, v, algorithm));
        if (gcdInput.earlyGCD != null)
            return gcdInput.earlyGCD;

        return lKaltofenMonaganModularGCDInGF(gcdInput, algorithm);
    }

    private static MultivariatePolynomialZp64 lKaltofenMonaganSparseModularGCDInGF(
        GCDInput<long> gcdInput)
    {
        return lKaltofenMonaganModularGCDInGF(gcdInput, MultivariateGCD.KaltofenMonaganSparseModularGCDInGF0);
    }

    private static MultivariatePolynomialZp64 lKaltofenMonaganModularGCDInGF(
        GCDInput<long> gcdInput,
        KaltofenMonaganAlgorithm<long> algorithm)
    {
        MultivariatePolynomialZp64 a = gcdInput.aReduced;
        MultivariatePolynomialZp64 b = gcdInput.bReduced;
        MultivariatePolynomialZp64 pContentGCD =
            ContentGCD(a, b, 0, (u, v) => KaltofenMonaganModularGCDInGF(u, v, algorithm));

        if (!pContentGCD.IsConstant())
        {
            a = MultivariateDivision.DivideExact(a, pContentGCD);
            b = MultivariateDivision.DivideExact(b, pContentGCD);
            return gcdInput.restoreGCD(KaltofenMonaganModularGCDInGF(a, b, algorithm).Multiply(pContentGCD));
        }

        for (int uVariable = a.nVariables - 1; uVariable >= 0; --uVariable)
        {
            MultivariatePolynomial<UnivariatePolynomialZp64>
                ua = a.AsOverUnivariateEliminate(uVariable),
                ub = b.AsOverUnivariateEliminate(uVariable);

            UnivariatePolynomialZp64 aContent = ua.Content(), bContent = ub.Content();
            UnivariatePolynomialZp64 ContentGCD = ua.ring.Gcd(aContent, bContent);

            ua = ua.DivideOrNull(aContent);
            ub = ub.DivideOrNull(bContent);

            MultivariatePolynomial<UnivariatePolynomialZp64> ugcd =
                algorithm(ua, ub, gcdInput.DegreeBounds[uVariable], gcdInput.finiteExtensionDegree);
            if (ugcd == null)
                // bad variable chosen
                continue;

            ugcd = ugcd.Multiply(ContentGCD);
            return gcdInput.restoreGCD(MultivariatePolynomialZp64.AsNormalMultivariate(ugcd, uVariable));
        }

        throw new Exception("\na: " + a + "\nb: " + b);
    }

    private const int MAX_OVER_ITERATIONS = 18;

    static MultivariatePolynomial<UnivariatePolynomial<uE>> KaltofenMonaganSparseModularGCDInGF0<uE>(
        MultivariatePolynomial<UnivariatePolynomial<uE>> a,
        MultivariatePolynomial<UnivariatePolynomial<uE>> b,
        int uDegreeBound,
        int finiteExtensionDegree)
    {
        var lcGCD = UnivariateGCD.PolynomialGCD(a.Lc(), b.Lc());

        Ring<UnivariatePolynomial<uE>> univariateRing = a.ring;
        Random random = PrivateRandom.GetRandom();
        IrreduciblePolynomialsIEnumerable<uE> primesLoop =
            new IrreduciblePolynomialsIEnumerable<uE>(univariateRing.GetOne(), finiteExtensionDegree);
        while (true)
        {
            main_loop: ;

            // prepare the skeleton
            UnivariatePolynomial<uE> basePrime = primesLoop.next();

            FiniteField<uE> fField = new FiniteField<uE>(basePrime);

            // reduce Zp[x] -> GF
            MultivariatePolynomial<UnivariatePolynomial<uE>>
                aMod = a.SetRing(fField),
                bMod = b.SetRing(fField);
            if (!aMod.SameSkeletonQ(a) || !bMod.SameSkeletonQ(b))
                continue;

            //accumulator to update coefficients via Chineese remainding
            MultivariatePolynomial<UnivariatePolynomial<uE>> @base = PolynomialGCD(aMod, bMod);
            UnivariatePolynomial<uE> lLcGCD = fField.ValueOf(lcGCD);

            if (@base.IsConstant())
                return a.CreateOne();

            // scale to correct l.c.
            @base = @base.Monic(lLcGCD);

            // cache the previous base
            MultivariatePolynomial<UnivariatePolynomial<uE>> previousBase = null;

            // over all primes
            while (true)
            {
                if (basePrime.Degree() >= uDegreeBound + MAX_OVER_ITERATIONS)
                    // fixme:
                    // probably this should not ever happen, but it happens (extremely rare, only for small
                    // characteristic and independently on the particular value of MAX_OVER_ITERATIONS)
                    // the current workaround is to switch to another variable in R[x_N][x1....x_(N-1)]
                    // representation and try again
                    //
                    // UPDATE: when increasing NUMBER_OF_UNDER_DETERMINED_RETRIES the problem seems to be disappeared
                    // (at the expense of longer time spent in LinZip)
                    return null;

                UnivariatePolynomial<uE> prime = primesLoop.next();
                fField = new FiniteField<uE>(prime);

                // reduce Zp[x] -> GF
                aMod = a.SetRing(fField);
                bMod = b.SetRing(fField);
                if (!aMod.SameSkeletonQ(a) || !bMod.SameSkeletonQ(b))
                    continue;

                // calculate new GCD using previously calculated skeleton via sparse interpolation
                MultivariatePolynomial<UnivariatePolynomial<uE>> modularGCD;
                if (aMod.nVariables == 1)
                    modularGCD = MultivariatePolynomial<UnivariatePolynomial<uE>>.AsMultivariate(
                        UnivariateGCD.PolynomialGCD(aMod.AsUnivariate(), bMod.AsUnivariate()), 1, 0, aMod.ordering);
                else
                    modularGCD = interpolateGCD(aMod, bMod, @base.SetRingUnsafe(fField), random);

                if (modularGCD == null)
                {
                    // interpolation failed => assumed form is wrong => start over
                    goto main_loop;
                }

                if (!isGCDTriplet(aMod, bMod, modularGCD))
                {
                    // extremely rare event
                    // bad base prime chosen
                    goto main_loop;
                }

                if (modularGCD.IsConstant())
                    return a.CreateOne();

                // better Degree bound found -> start over
                if (modularGCD.Degree(0) < @base.Degree(0))
                {
                    lLcGCD = fField.ValueOf(lcGCD);
                    // scale to correct l.c.
                    @base = modularGCD.Monic(lLcGCD);
                    basePrime = prime;
                    continue;
                }

                //skip unlucky prime
                if (modularGCD.Degree(0) > @base.Degree(0))
                    continue;

                //lifting
                var newBasePrime = basePrime.Clone().Multiply(prime);
                var MonicFactor = fField.DivideExact(fField.ValueOf(lcGCD), modularGCD.Lc());

                PairedIterator<UnivariatePolynomial<uE>, UnivariatePolynomial<uE>> iterator =
                    new PairedIterator<UnivariatePolynomial<uE>, UnivariatePolynomial<uE>>(@base, modularGCD);
                ChineseRemainders.ChineseRemaindersMagic<UnivariatePolynomial<uE>> magic =
                    ChineseRemainders.CreateMagic(univariateRing, basePrime, prime);
                while (iterator.MoveNext())
                {
                    Monomial<UnivariatePolynomial<uE>>
                        baseTerm = iterator.aTerm,
                        imageTerm = iterator.bTerm;

                    if (baseTerm.coefficient.IsZero())
                        // term is absent in the base
                        continue;

                    if (imageTerm.coefficient.IsZero())
                    {
                        // term is absent in the modularGCD => remove it from the base
                        @base.Subtract(baseTerm);
                        continue;
                    }

                    UnivariatePolynomial<uE> oth = fField.Multiply(imageTerm.coefficient, MonicFactor);

                    // update base term
                    UnivariatePolynomial<uE> newCoeff =
                        ChineseRemainders.ChineseRemainder(univariateRing, magic, baseTerm.coefficient, oth);
                    @base.Put(baseTerm.SetCoefficient(newCoeff));
                }

                basePrime = newBasePrime;

                // set ring back to the normal univariate ring
                @base = @base.SetRingUnsafe(univariateRing);
                // two trials didn't change the result, probably we are done
                MultivariatePolynomial<UnivariatePolynomial<uE>> candidate = @base.Clone().PrimitivePart();
                if (basePrime.Degree() >= uDegreeBound || (previousBase != null && candidate.Equals(previousBase)))
                {
                    previousBase = candidate;
                    //first check b since b is less Degree
                    if (!isGCDTriplet(b, a, candidate))
                        continue;

                    return candidate;
                }

                previousBase = candidate;
            }
        }
    }

    static MultivariatePolynomial<UnivariatePolynomial<uE>> KaltofenMonaganEEZModularGCDInGF0<uE>(
        MultivariatePolynomial<UnivariatePolynomial<uE>> a,
        MultivariatePolynomial<UnivariatePolynomial<uE>> b,
        int uDegreeBound,
        int finiteExtensionDegree)
    {
        UnivariatePolynomial<uE> lcGCD = UnivariateGCD.PolynomialGCD(a.Lc(), b.Lc());

        Ring<UnivariatePolynomial<uE>> univariateRing = a.ring;

        MultivariatePolynomial<UnivariatePolynomial<uE>> @base = null;
        UnivariatePolynomial<uE> basePrime = null;

        IrreduciblePolynomialsIEnumerable<uE> primesLoop =
            new IrreduciblePolynomialsIEnumerable<uE>(univariateRing.GetOne(), finiteExtensionDegree);
        main_loop:
        while (true)
        {
            if (basePrime != null && basePrime.Degree() >= uDegreeBound + MAX_OVER_ITERATIONS)
                // fixme:
                // probably this should not ever happen, but it happens (extremely rare, only for small
                // characteristic and independently on the particular value of MAX_OVER_ITERATIONS)
                // the current workaround is to switch to another variable in R[x_N][x1....x_(N-1)]
                // representation and try again
                //
                // UPDATE: when increasing NUMBER_OF_UNDER_DETERMINED_RETRIES the problem seems to be disappeared
                // (at the expense of longer time spent in LinZip)
                return null;

            // prepare the skeleton
            UnivariatePolynomial<uE> prime = primesLoop.next();

            FiniteField<uE> fField = new FiniteField<uE>(prime);

            // reduce Zp[x] -> GF
            MultivariatePolynomial<UnivariatePolynomial<uE>>
                aMod = a.SetRing(fField),
                bMod = b.SetRing(fField);
            if (!aMod.SameSkeletonQ(a) || !bMod.SameSkeletonQ(b))
                continue;

            //accumulator to update coefficients via Chineese remainding
            MultivariatePolynomial<UnivariatePolynomial<uE>> modGCD = PolynomialGCD(aMod, bMod);
            UnivariatePolynomial<uE> lLcGCD = fField.ValueOf(lcGCD);

            if (modGCD.IsConstant())
                return a.CreateOne();

            // scale to correct l.c.
            modGCD = modGCD.Monic(lLcGCD);

            if (@base == null)
            {
                @base = modGCD;
                basePrime = prime;
                continue;
            }


            //lifting
            UnivariatePolynomial<uE> newBasePrime = basePrime.Clone().Multiply(prime);
            UnivariatePolynomial<uE> MonicFactor = fField.DivideExact(fField.ValueOf(lcGCD), modGCD.Lc());

            PairedIterator<UnivariatePolynomial<uE>, UnivariatePolynomial<uE>> iterator =
                new PairedIterator<UnivariatePolynomial<uE>, UnivariatePolynomial<uE>>(@base, modGCD);
            ChineseRemainders.ChineseRemaindersMagic<UnivariatePolynomial<uE>> magic =
                ChineseRemainders.CreateMagic(univariateRing, basePrime, prime);
            while (iterator.MoveNext())
            {
                Monomial<UnivariatePolynomial<uE>>
                    baseTerm = iterator.aTerm,
                    imageTerm = iterator.bTerm;

                if (baseTerm.coefficient.IsZero())
                    // term is absent in the base
                    continue;

                if (imageTerm.coefficient.IsZero())
                {
                    // term is absent in the modularGCD => remove it from the base
                    @base.Subtract(baseTerm);
                    continue;
                }

                UnivariatePolynomial<uE> oth = fField.Multiply(imageTerm.coefficient, MonicFactor);

                // update base term
                UnivariatePolynomial<uE> newCoeff =
                    ChineseRemainders.ChineseRemainder(univariateRing, magic, baseTerm.coefficient, oth);
                @base.Put(baseTerm.SetCoefficient(newCoeff));
            }

            basePrime = newBasePrime;

            // set ring back to the normal univariate ring
            @base = @base.SetRingUnsafe(univariateRing);
            // two trials didn't change the result, probably we are done
            MultivariatePolynomial<UnivariatePolynomial<uE>> candidate = @base.Clone().PrimitivePart();
            //first check b since b is less Degree
            if (!isGCDTriplet(b, a, candidate))
                continue;

            return candidate;
        }
    }

    private sealed class IrreduciblePolynomialsIEnumerable<uE>
    {
        readonly UnivariatePolynomial<uE> factory;
        int Degree;

        public IrreduciblePolynomialsIEnumerable(UnivariatePolynomial<uE> factory, int Degree)
        {
            this.factory = factory;
            this.Degree = Degree;
        }

        public UnivariatePolynomial<uE> next()
        {
            return IrreduciblePolynomials.RandomIrreduciblePolynomial(factory, Degree++, PrivateRandom.GetRandom());
        }
    }

    static MultivariatePolynomial<E> interpolateGCD<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b,
        MultivariatePolynomial<E> skeleton, Random rnd)
    {
        a.AssertSameCoefficientRingWith(b);
        a.AssertSameCoefficientRingWith(skeleton);

        // a and b must be Content-free
//        if (!ContentGCD(a, b, 0, MultivariateGCD::PolynomialGCD).IsConstant())
//            return null;
//        MultivariatePolynomialZp64 Content = ContentGCD(a, b, 0, MultivariateGCD::PolynomialGCD);
//        a = divideExact(a, Content);
//        b = divideExact(b, Content);
//        skeleton = divideSkeletonExact(skeleton, Content);

        SparseInterpolation<E> interpolation = CreateInterpolation(-1, a, b, skeleton, 1, rnd);
        if (interpolation == null)
            return null;
        MultivariatePolynomial<E> gcd = interpolation.evaluate();
        if (gcd == null)
            return null;

        return gcd; //.Multiply(Content);
    }

     /* ===================================== Multivariate GCD over finite fields ==================================== */


    // public static  MultivariatePolynomial<E> BrownGCD<E>(
    //         MultivariatePolynomial<E> a,
    //         MultivariatePolynomial<E> b) {
    //     Util.EnsureOverField(a, b);
    //     a.AssertSameCoefficientRingWith(b);
    //
    //     if (CanConvertToZp64(a))
    //         return ConvertFromZp64(BrownGCD(AsOverZp64(a), SsOverZp64(b)));
    //
    //     // prepare input and test for early termination
    //     GCDInput<Monomial<E>, MultivariatePolynomial<E>> gcdInput = preparedGCDInput(a, b, MultivariateGCD.BrownGCD);
    //     if (gcdInput.earlyGCD != null)
    //         return gcdInput.earlyGCD;
    //
    //     if (gcdInput.finiteExtensionDegree > 1)
    //         return KaltofenMonaganSparseModularGCDInGF(gcdInput);
    //
    //     MultivariatePolynomial<E> result = BrownGCD(
    //             gcdInput.aReduced, gcdInput.bReduced, PrivateRandom.GetRandom(),
    //             gcdInput.lastPresentVariable, gcdInput.DegreeBounds, gcdInput.evaluationStackLimit);
    //     if (result == null)
    //         // ground fill is too small for modular algorithm
    //         return KaltofenMonaganSparseModularGCDInGF(gcdInput);
    //
    //     return gcdInput.restoreGCD(result);
    // }
//
//     /**
//      * Actual implementation of dense interpolation
//      *
//      * @param variable             current variable (all variables {@code v > variable} are fixed so far)
//      * @param DegreeBounds         Degree bounds for gcd
//      * @param evaluationStackLimit ring cardinality
//      */
//     
//     private static <E> MultivariatePolynomial<E> BrownGCD(
//             MultivariatePolynomial<E> a,
//             MultivariatePolynomial<E> b,
//             Random rnd,
//             int variable,
//             int[] DegreeBounds,
//             int evaluationStackLimit) {
//
//         //check for trivial gcd
//         MultivariatePolynomial<E> trivialGCD = trivialGCD(a, b);
//         if (trivialGCD != null)
//             return trivialGCD;
//
//         MultivariatePolynomial<E> factory = a;
//         int nVariables = factory.nVariables;
//         if (variable == 0)
//         // switch to univariate gcd
//         {
//             UnivariatePolynomial<E> gcd = UnivariateGCD.PolynomialGCD(a.asUnivariate(), b.asUnivariate());
//             if (gcd.Degree() == 0)
//                 return factory.CreateOne();
//             return AMultivariatePolynomial.asMultivariate(gcd, nVariables, variable, factory.ordering);
//         }
//
//         PrimitiveInput<Monomial<E>, MultivariatePolynomial<E>, UnivariatePolynomial<E>> primitiveInput = makePrimitive(a, b, variable);
//         // primitive parts of a and b as Zp[x_k][x_1 ... x_{k-1}]
//         a = primitiveInput.aPrimitive;
//         b = primitiveInput.bPrimitive;
//         // gcd of Zp[x_k] Content and lc
//         UnivariatePolynomial<E>
//                 ContentGCD = primitiveInput.ContentGCD,
//                 lcGCD = primitiveInput.lcGCD;
//
//         //check again for trivial gcd
//         trivialGCD = trivialGCD(a, b);
//         if (trivialGCD != null) {
//             MultivariatePolynomial<E> poly = AMultivariatePolynomial.asMultivariate(ContentGCD, a.nVariables, variable, a.ordering);
//             return trivialGCD.Multiply(poly);
//         }
//
//         Ring<E> ring = factory.ring;
//         //Degree bound for the previous variable
//         int prevVarExponent = DegreeBounds[variable - 1];
//         //dense interpolation
//         MultivariateInterpolation.Interpolation<E> interpolation = null;
//         //previous interpolation (used to detect whether update doesn't change the result)
//         MultivariatePolynomial<E> previousInterpolation;
//         //store points that were already used in interpolation
//         Set<E> evaluationStack = new HashSet<>();
//
//         int[] aDegrees = a.Degrees(), bDegrees = b.Degrees();
//         main:
//         while (true) {
//             if (evaluationStackLimit == evaluationStack.Size())
//                 // all elements of the ring are tried
//                 // do division check (last chance) and return
//                 return doDivisionCheck(a, b, ContentGCD, interpolation, variable);
//
//             //pickup the next random element for variable
//             E randomPoint = randomElement(ring, rnd);
//             if (evaluationStack.contains(randomPoint))
//                 continue;
//             evaluationStack.add(randomPoint);
//
//             E lcVal = lcGCD.evaluate(randomPoint);
//             if (ring.isZero(lcVal))
//                 continue;
//
//             // evaluate a and b at variable = randomPoint
//             MultivariatePolynomial<E>
//                     aVal = a.evaluate(variable, randomPoint),
//                     bVal = b.evaluate(variable, randomPoint);
//
//             // check for unlucky substitution
//             int[] aValDegrees = aVal.Degrees(), bValDegrees = bVal.Degrees();
//             for (int i = variable - 1; i >= 0; --i)
//                 if (aDegrees[i] != aValDegrees[i] || bDegrees[i] != bValDegrees[i])
//                     continue main;
//
//             // calculate gcd of the result by the recursive call
//             MultivariatePolynomial<E> cVal = BrownGCD(aVal, bVal, rnd, variable - 1, DegreeBounds, evaluationStackLimit);
//             if (cVal == null)
//                 //unlucky homomorphism
//                 continue;
//
//             int currExponent = cVal.Degree(variable - 1);
//             if (currExponent > prevVarExponent)
//                 //unlucky homomorphism
//                 continue;
//
//             // normalize gcd
//             cVal = cVal.Multiply(ring.Multiply(ring.reciprocal(cVal.Lc()), lcVal));
//             assert cVal.Lc().equals(lcVal);
//
//             if (currExponent < prevVarExponent) {
//                 //better Degree bound detected => start over
//                 interpolation = new MultivariateInterpolation.Interpolation<>(variable, randomPoint, cVal);
//                 DegreeBounds[variable - 1] = prevVarExponent = currExponent;
//                 continue;
//             }
//
//             if (interpolation == null) {
//                 //first successful homomorphism
//                 interpolation = new MultivariateInterpolation.Interpolation<>(variable, randomPoint, cVal);
//                 continue;
//             }
//
//             // Cache previous interpolation. NOTE: Clone() is important, since the poly will
//             // be modified inplace by the update() method
//             previousInterpolation = interpolation.getInterpolatingPolynomial().Clone();
//             interpolation.update(randomPoint, cVal);
//
//             // do division test
//             if (DegreeBounds[variable] <= interpolation.numberOfPoints()
//                     || previousInterpolation.equals(interpolation.getInterpolatingPolynomial())) {
//                 MultivariatePolynomial<E> result = doDivisionCheck(a, b, ContentGCD, interpolation, variable);
//                 if (result != null)
//                     return result;
//             }
//         }
//     }
//
//     
    private static MultivariatePolynomial<E> doDivisionCheck<E>(
        MultivariatePolynomial<E> a, MultivariatePolynomial<E> b,
        UnivariatePolynomial<E> ContentGCD, MultivariateInterpolation.Interpolation<E> interpolation, int variable)
    {
        if (interpolation == null)
            return null;
        MultivariatePolynomial<E> interpolated =
            MultivariatePolynomial<E>.AsNormalMultivariate(
                interpolation.GetInterpolatingPolynomial().AsOverUnivariateEliminate(variable).PrimitivePart(),
                variable);
        if (!isGCDTriplet(a, b, interpolated))
            return null;

        if (ContentGCD == null)
            return interpolated;
        MultivariatePolynomial<E> poly =
            MultivariatePolynomial<E>.AsMultivariate(ContentGCD, a.nVariables, variable, a.ordering);
        return interpolated.Multiply(poly);
    }

//
    // when to use fast division check
    private const int LARGE_SIZE_USE_FAST_DIV_TEST = 64_000;


    private static bool isGCDTriplet<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b,
        MultivariatePolynomial<E> gcd)
    {
        if (Math.Max(a.Size(), b.Size()) > LARGE_SIZE_USE_FAST_DIV_TEST)
        {
            Ring<E> ring = a.ring;
            E[] subs = new E[a.nVariables - 1];
            for (int i = 0; i < subs.Length; ++i)
                subs[i] = ring.RandomNonZeroElement(PrivateRandom.GetRandom());

            MultivariatePolynomial<E>.PrecomputedPowersHolder powers =
                a.MkPrecomputedPowers(Utils.Utils.Sequence(0, a.nVariables - 1), subs);

            UnivariatePolynomial<E> uniDiv = subsToUnivariate(gcd, powers);
            // fast check
            UnivariatePolynomial<E> ra = UnivariateDivision.Remainder(subsToUnivariate(a, powers), uniDiv, false);
            if (ra == null || !ra.IsZero())
                return false;
            UnivariatePolynomial<E> rb = UnivariateDivision.Remainder(subsToUnivariate(b, powers), uniDiv, false);
            if (rb == null || !rb.IsZero())
                return false;
        }

        return MultivariateDivision.DividesQ(a, gcd) && MultivariateDivision.DividesQ(b, gcd);
    }


    private static UnivariatePolynomial<E> subsToUnivariate<E>(MultivariatePolynomial<E> a,
        MultivariatePolynomial<E>.PrecomputedPowersHolder powers)
    {
        E[] aUni = a.ring.CreateZeroesArray(a.Degree(a.nVariables - 1) + 1);
        foreach (Monomial<E> t in a.terms)
        {
            E val = t.coefficient;
            for (int i = 0; i < a.nVariables - 1; ++i)
                val = a.ring.Multiply(val, powers.Pow(i, t.exponents[i]));
            aUni[t.exponents[a.nVariables - 1]] = a.ring.Add(aUni[t.exponents[a.nVariables - 1]], val);
        }

        return UnivariatePolynomial<E>.CreateUnsafe(a.ring, aUni);
    }

    public static MultivariatePolynomial<E> ZippelGCD<E>(
        MultivariatePolynomial<E> a,
        MultivariatePolynomial<E> b)
    {
        Util.EnsureOverField(a, b);
        a.AssertSameCoefficientRingWith(b);

        if (CanConvertToZp64(a))
            return ConvertFromZp64<E>(ZippelGCD(AsOverZp64(a), AsOverZp64(b)));

        // prepare input and test for early termination
        GCDInput<E> gcdInput = preparedGCDInput(a, b, MultivariateGCD.ZippelGCD);
        if (gcdInput.earlyGCD != null)
            return gcdInput.earlyGCD;

        if (gcdInput.finiteExtensionDegree > 1)
            return KaltofenMonaganSparseModularGCDInGF(gcdInput);

        a = gcdInput.aReduced;
        b = gcdInput.bReduced;

        // Content in the main variable => avoid raise condition in LINZIP!
        // see Example 4 in "Algorithms for the non-Monic case of the sparse modular GCD algorithm"
        MultivariatePolynomial<E> Content = ContentGCD(a, b, 0, MultivariateGCD.ZippelGCD);
        a = MultivariateDivision.DivideExact(a, Content);
        b = MultivariateDivision.DivideExact(b, Content);

        MultivariatePolynomial<E> result = ZippelGCD(a, b, PrivateRandom.GetRandom(),
            gcdInput.lastPresentVariable, gcdInput.DegreeBounds, gcdInput.evaluationStackLimit);
        if (result == null)
            // ground fill is too small for modular algorithm
            return KaltofenMonaganSparseModularGCDInGF(gcdInput);

        result = result.Multiply(Content);
        return gcdInput.restoreGCD(result);
    }


    private const int MAX_SPARSE_INTERPOLATION_FAILS = 64;

    private const int ALLOWED_OVER_INTERPOLATED_ATTEMPTS = 64;


    private static MultivariatePolynomial<E> ZippelGCD<E>(
        MultivariatePolynomial<E> a,
        MultivariatePolynomial<E> b,
        Random rnd,
        int variable,
        int[] DegreeBounds,
        int evaluationStackLimit)
    {
        //check for trivial gcd
        MultivariatePolynomial<E> trivialGCD = MultivariateGCD.trivialGCD(a, b);
        if (trivialGCD != null)
            return trivialGCD;

        MultivariatePolynomial<E> factory = a;
        int nVariables = factory.nVariables;
        if (variable == 0)
            // switch to univariate gcd
        {
            UnivariatePolynomial<E> gcd = UnivariateGCD.PolynomialGCD(a.AsUnivariate(), b.AsUnivariate());
            if (gcd.Degree() == 0)
                return factory.CreateOne();
            return MultivariatePolynomial<E>.AsMultivariate(gcd, nVariables, variable, factory.ordering);
        }

//        MultivariatePolynomial<E> Content = ZippelContentGCD(a, b, variable);
//        a = divideExact(a, Content);
//        b = divideExact(b, Content);

        PrimitiveInput<E> primitiveInput = makePrimitive(a, b, variable);
        // primitive parts of a and b as Zp[x_k][x_1 ... x_{k-1}]
        a = primitiveInput.aPrimitive;
        b = primitiveInput.bPrimitive;
        // gcd of Zp[x_k] Content and lc
        UnivariatePolynomial<E>
            ContentGCD = primitiveInput.contentGCD,
            lcGCD = primitiveInput.lcGCD;

        //check again for trivial gcd
        trivialGCD = MultivariateGCD.trivialGCD(a, b);
        if (trivialGCD != null)
        {
            MultivariatePolynomial<E> poly =
                MultivariatePolynomial<E>.AsMultivariate(ContentGCD, a.nVariables, variable, a.ordering);
            return trivialGCD.Multiply(poly);
        }

        Ring<E> ring = factory.ring;
        //store points that were already used in interpolation
        HashSet<E> globalEvaluationStack = new HashSet<E>();

        int[] aDegrees = a.DegreesRef(), bDegrees = b.DegreesRef();
        int failedSparseInterpolations = 0;

        int[] tmpDegreeBounds = (int[])DegreeBounds.Clone();

        while (true)
        {
            main: ;
            if (evaluationStackLimit == globalEvaluationStack.Count)
                return null;

            E seedPoint = randomElement(ring, rnd);
            if (globalEvaluationStack.Contains(seedPoint))
                continue;

            globalEvaluationStack.Add(seedPoint);

            E lcVal = lcGCD.Evaluate(seedPoint);
            if (ring.IsZero(lcVal))
                continue;

            // evaluate a and b at variable = randomPoint
            // calculate gcd of the result by the recursive call
            MultivariatePolynomial<E>
                aVal = a.Evaluate(variable, seedPoint),
                bVal = b.Evaluate(variable, seedPoint);

            // check for unlucky substitution
            int[] aValDegrees = aVal.DegreesRef(), bValDegrees = bVal.DegreesRef();
            for (int i = variable - 1; i >= 0; --i)
                if (aDegrees[i] != aValDegrees[i] || bDegrees[i] != bValDegrees[i])
                    goto main;

            MultivariatePolynomial<E> cVal = ZippelGCD(aVal, bVal, rnd, variable - 1, tmpDegreeBounds,
                evaluationStackLimit);
            if (cVal == null)
                //unlucky homomorphism
                continue;

            int currExponent = cVal.Degree(variable - 1);
            if (currExponent > tmpDegreeBounds[variable - 1])
                //unlucky homomorphism
                continue;

            if (currExponent < tmpDegreeBounds[variable - 1])
            {
                //better Degree bound detected
                tmpDegreeBounds[variable - 1] = currExponent;
            }

            cVal = cVal.Multiply(ring.Multiply(ring.Reciprocal(cVal.Lc()), lcVal));
            Debug.Assert(cVal.Lc().Equals(lcVal));

            SparseInterpolation<E> sparseInterpolator =
                CreateInterpolation(variable, a, b, cVal, tmpDegreeBounds[variable], rnd);
            if (sparseInterpolator == null)
                //unlucky homomorphism
                continue;

            // we are applying dense interpolation for univariate skeleton coefficients
            MultivariateInterpolation.Interpolation<E> denseInterpolation =
                new MultivariateInterpolation.Interpolation<E>(variable, seedPoint, cVal);
            //previous interpolation (used to detect whether update doesn't change the result)
            MultivariatePolynomial<E> previousInterpolation;
            //local evaluation stack for points that are calculated via sparse interpolation (but not gcd evaluation) -> always same skeleton
            HashSet<E> localEvaluationStack = new HashSet<E>(globalEvaluationStack);
            while (true)
            {
                if (evaluationStackLimit == localEvaluationStack.Count)
                    return null;

                if (denseInterpolation.NumberOfPoints() >
                    tmpDegreeBounds[variable] + ALLOWED_OVER_INTERPOLATED_ATTEMPTS)
                {
                    // restore original Degree bounds, since unlucky homomorphism may destruct correct bounds
                    tmpDegreeBounds = (int[])DegreeBounds.Clone();
                    goto main;
                }

                E randomPoint = randomElement(ring, rnd);
                if (localEvaluationStack.Contains(randomPoint))
                    continue;
                localEvaluationStack.Add(randomPoint);

                lcVal = lcGCD.Evaluate(randomPoint);
                if (ring.IsZero(lcVal))
                    continue;

                cVal = sparseInterpolator.evaluate(randomPoint);
                if (cVal == null)
                {
                    ++failedSparseInterpolations;
                    if (failedSparseInterpolations == MAX_SPARSE_INTERPOLATION_FAILS)
                        return null; //throw new RuntimeException("Sparse interpolation failed");
                    // restore original Degree bounds, since unlucky homomorphism may destruct correct bounds
                    tmpDegreeBounds = (int[])DegreeBounds.Clone();
                    goto main;
                }

                cVal = cVal.Multiply(ring.Multiply(ring.Reciprocal(cVal.Lc()), lcVal));
                Debug.Assert(cVal.Lc().Equals(lcVal));

                // Cache previous interpolation. NOTE: Clone() is important, since the poly will
                // be modified inplace by the update() method
                previousInterpolation = denseInterpolation.GetInterpolatingPolynomial().Clone();
                denseInterpolation.Update(randomPoint, cVal);

                // do division test
                if ((tmpDegreeBounds[variable] <= denseInterpolation.NumberOfPoints()
                     && denseInterpolation.NumberOfPoints() - tmpDegreeBounds[variable] < 3)
                    || previousInterpolation.Equals(denseInterpolation.GetInterpolatingPolynomial()))
                {
                    MultivariatePolynomial<E> result = doDivisionCheck(a, b, ContentGCD, denseInterpolation, variable);
                    if (result != null)
                        return result;
                }
            }
        }
    }

    private const bool ALWAYS_LINZIP = false;

    public const int MAX_FAILED_SUBSTITUTIONS = 32;

    static SparseInterpolation<E> CreateInterpolation<E>(int variable,
        MultivariatePolynomial<E> a,
        MultivariatePolynomial<E> b,
        MultivariatePolynomial<E> skeleton,
        int expectedNumberOfEvaluations,
        Random rnd)
    {
        Debug.Assert(a.nVariables > 1);
        skeleton = skeleton.Clone().SetAllCoefficientsToUnit();
        if (skeleton.Size() == 1)
            return new TrivialSparseInterpolation<E>(skeleton);

        bool Monic = a.CoefficientOf(0, a.Degree(0)).IsConstant() && b.CoefficientOf(0, a.Degree(0)).IsConstant();
        var globalSkeleton = skeleton.GetSkeleton();
        Dictionary<int, MultivariatePolynomial<E>> univarSkeleton = getSkeleton(skeleton);
        int[] sparseUnivarDegrees = univarSkeleton.Keys.ToArray();

        Ring<E> ring = a.ring;

        int lastVariable = variable == -1 ? a.nVariables - 1 : variable;
        int[] evaluationVariables = Utils.Utils.Sequence(1, lastVariable + 1); //variable inclusive
        E[] evaluationPoint = new E[evaluationVariables.Length];

        MultivariatePolynomial<E>.PrecomputedPowersHolder powers;
        int fails = 0;
        while (true)
        {
            search_for_good_evaluation_point: ;
            if (fails >= MAX_FAILED_SUBSTITUTIONS)
                return null;
            //avoid zero evaluation points
            for (int i = lastVariable - 1; i >= 0; --i)
                do
                {
                    evaluationPoint[i] = randomElement(ring, rnd);
                } while (ring.IsZero(evaluationPoint[i]));

            powers = mkPrecomputedPowers(a, b, evaluationVariables, evaluationPoint);

            foreach (MultivariatePolynomial<E> p in (MultivariatePolynomial<E>[]) [a, b, skeleton])
                if (!p.GetSkeleton(0).Equals(p.Evaluate(powers, evaluationVariables).GetSkeleton()))
                {
                    ++fails;
                    goto search_for_good_evaluation_point;
                }

            break;
        }

        int requiredNumberOfEvaluations = -1, MonicScalingExponent = -1;
        for (var it = univarSkeleton.GetEnumerator(); it.MoveNext();)
        {
            MultivariatePolynomial<E> v = it.Current.Value;
            if (v.Size() > requiredNumberOfEvaluations)
                requiredNumberOfEvaluations = v.Size();
            if (v.Size() == 1)
                MonicScalingExponent = it.Current.Key;
        }

        if (!ALWAYS_LINZIP)
        {
            if (Monic)
                MonicScalingExponent = -1;

            if (Monic || MonicScalingExponent != -1)
                return new MonicInterpolation<E>(ring, variable, a, b, globalSkeleton, univarSkeleton,
                    sparseUnivarDegrees,
                    evaluationVariables, evaluationPoint, powers, expectedNumberOfEvaluations, rnd,
                    requiredNumberOfEvaluations, MonicScalingExponent);
        }

        return new LinZipInterpolation<E>(ring, variable, a, b, globalSkeleton, univarSkeleton, sparseUnivarDegrees,
            evaluationVariables, evaluationPoint, powers, expectedNumberOfEvaluations, rnd);
    }


    private static MultivariatePolynomial<E>.PrecomputedPowersHolder mkPrecomputedPowers<E>(
        MultivariatePolynomial<E> a, MultivariatePolynomial<E> b,
        int[] evaluationVariables, E[] evaluationPoint)
    {
        int[] Degrees = Utils.Utils.Max(a.DegreesRef(), b.DegreesRef());
        MultivariatePolynomial<E>.PrecomputedPowers[]
            pp = new MultivariatePolynomial<E>.PrecomputedPowers[a.nVariables];
        for (int i = 0; i < evaluationVariables.Length; ++i)
            pp[evaluationVariables[i]] = new MultivariatePolynomial<E>.PrecomputedPowers(
                Math.Min(Degrees[evaluationVariables[i]], MultivariatePolynomialZp64.MAX_POWERS_CACHE_SIZE),
                evaluationPoint[i], a.ring);
        return new MultivariatePolynomial<E>.PrecomputedPowersHolder(a.ring, pp);
    }


    public static Dictionary<int, MultivariatePolynomial<E>> getSkeleton<E>(MultivariatePolynomial<E> poly)
    {
        Dictionary<int, MultivariatePolynomial<E>> skeleton = new Dictionary<int, MultivariatePolynomial<E>>();
        foreach (Monomial<E> term in poly.terms)
        {
            Monomial<E> newDV = term.SetZero(0);
            if (skeleton.TryGetValue(term.exponents[0], out var coeff))
                coeff.Add(newDV);
            else
                skeleton.Add(term.exponents[0], MultivariatePolynomial<E>.Create(poly.nVariables,
                    poly.ring, poly.ordering, newDV));
        }

        return skeleton;
    }

    interface SparseInterpolation<E>
    {
        MultivariatePolynomial<E> evaluate();

        MultivariatePolynomial<E> evaluate(E newPoint);
    }

    sealed class TrivialSparseInterpolation<E> : SparseInterpolation<E>
    {
        readonly MultivariatePolynomial<E> val;

        public TrivialSparseInterpolation(MultivariatePolynomial<E> val)
        {
            this.val = val;
        }

        public MultivariatePolynomial<E> evaluate()
        {
            return val;
        }

        public MultivariatePolynomial<E> evaluate(E newPoint)
        {
            return val;
        }
    }

    abstract class ASparseInterpolation<E> : SparseInterpolation<E>
    {
        public readonly Ring<E> ring;

        public readonly int variable;

        public readonly MultivariatePolynomial<E> a;
        public readonly MultivariatePolynomial<E> b;

        public readonly ImmutableList<DegreeVector> globalSkeleton;

        public readonly Dictionary<int, MultivariatePolynomial<E>> univarSkeleton;

        public readonly int[] sparseUnivarDegrees;

        readonly int[] evaluationVariables;

        readonly E[] evaluationPoint;

        public readonly MultivariatePolynomial<E>.PrecomputedPowersHolder powers;

        public readonly ZippelEvaluations<E> aEvals;
        public readonly ZippelEvaluations<E> bEvals;

        readonly Random rnd;

        public ASparseInterpolation(Ring<E> ring, int variable,
            MultivariatePolynomial<E> a, MultivariatePolynomial<E> b,
            ImmutableList<DegreeVector> globalSkeleton,
            Dictionary<int, MultivariatePolynomial<E>> univarSkeleton,
            int[] sparseUnivarDegrees, int[] evaluationVariables,
            E[] evaluationPoint,
            MultivariatePolynomial<E>.PrecomputedPowersHolder powers, int expectedNumberOfEvaluations, Random rnd)
        {
            this.ring = ring;
            this.variable = variable;
            this.a = a;
            this.b = b;
            this.globalSkeleton = globalSkeleton;
            this.univarSkeleton = univarSkeleton;
            this.sparseUnivarDegrees = sparseUnivarDegrees;
            this.evaluationVariables = evaluationVariables;
            this.evaluationPoint = evaluationPoint;
            this.aEvals = CreateEvaluations(a, evaluationVariables, evaluationPoint, powers,
                expectedNumberOfEvaluations);
            this.bEvals = CreateEvaluations(b, evaluationVariables, evaluationPoint, powers,
                expectedNumberOfEvaluations);
            this.powers = powers;
            this.rnd = rnd;
        }

        public MultivariatePolynomial<E> evaluate()
        {
            return evaluate(evaluationPoint[evaluationPoint.Length - 1]);
        }


        public MultivariatePolynomial<E> evaluate(E newPoint)
        {
            // constant is constant
            if (globalSkeleton.Count == 1)
                return a.Create(((Monomial<E>)globalSkeleton[0]).SetCoefficient(ring.GetOne()));
            // variable = newPoint
            evaluationPoint[evaluationPoint.Length - 1] = newPoint;
            powers.Set(evaluationVariables[evaluationVariables.Length - 1], newPoint);
            return evaluate0(newPoint);
        }

        public abstract MultivariatePolynomial<E> evaluate0(E newPoint);
    }


    private const int N_EVALUATIONS_RECURSIVE_SWITCH = 16;

    private const int SIZE_OF_POLY_RECURSIVE_SWITCH = 512;

    public static ZippelEvaluations<E> CreateEvaluations<E>(MultivariatePolynomial<E> poly,
        int[] evaluationVariables,
        E[] evaluationPoint,
        MultivariatePolynomial<E>.PrecomputedPowersHolder basePowers,
        int expectedNumberOfEvaluations)
    {
        if (expectedNumberOfEvaluations > N_EVALUATIONS_RECURSIVE_SWITCH
            && poly.Size() > SIZE_OF_POLY_RECURSIVE_SWITCH)
            return new FastSparseRecursiveEvaluations<E>(poly, evaluationPoint,
                evaluationVariables[evaluationVariables.Length - 1]);
        else
            return new PlainEvaluations<E>(poly, evaluationVariables, evaluationPoint, basePowers);
    }


    public interface ZippelEvaluations<E>
    {
        UnivariatePolynomial<E> evaluate(int raiseFactor, E value);
    }


    private sealed class PlainEvaluations<E> : ZippelEvaluations<E>
    {
        private readonly MultivariatePolynomial<E> poly;

        private readonly int[] evaluationVariables;

        private readonly E[] evaluationPoint;

        private readonly MultivariatePolynomial<E>.PrecomputedPowersHolder basePowers;

        private readonly Dictionary<int, MultivariatePolynomial<E>.PrecomputedPowersHolder> powersCache
            = new Dictionary<int, MultivariatePolynomial<E>.PrecomputedPowersHolder>();

        public PlainEvaluations(MultivariatePolynomial<E> poly,
            int[] evaluationVariables,
            E[] evaluationPoint,
            MultivariatePolynomial<E>.PrecomputedPowersHolder basePowers)
        {
            this.poly = poly;
            this.evaluationVariables = evaluationVariables;
            this.evaluationPoint = evaluationPoint;
            this.basePowers = basePowers.Clone();
            this.powersCache.Add(1, this.basePowers);
        }

        public UnivariatePolynomial<E> evaluate(int raiseFactor, E value)
        {
            basePowers.Set(evaluationVariables[evaluationVariables.Length - 1], value);

            MultivariatePolynomial<E>.PrecomputedPowersHolder powers = powersCache.GetValueOrDefault(raiseFactor, null);
            Ring<E> ring = poly.ring;
            if (powers == null)
            {
                powers = basePowers.Clone();
                for (int i = 0; i < (evaluationVariables.Length - 1); ++i)
                    powers.Set(evaluationVariables[i], ring.Pow(evaluationPoint[i], raiseFactor));
                powersCache.Add(raiseFactor, powers);
            }

            powers.Set(evaluationVariables[evaluationVariables.Length - 1], value);

            E[] result = ring.CreateZeroesArray(poly.Degree(0) + 1);
            foreach (Monomial<E> el in poly.terms)
            {
                E ucf = el.coefficient;
                foreach (int variable in evaluationVariables)
                    ucf = ring.Multiply(ucf, powers.Pow(variable, el.exponents[variable]));
                int uDeg = el.exponents[0];
                result[uDeg] = ring.Add(result[uDeg], ucf);
            }

            return UnivariatePolynomial<E>.Create(ring, result);
        }
    }


    private sealed class FastSparseRecursiveEvaluations<E> : ZippelEvaluations<E>
    {
        private readonly MultivariatePolynomial<E> poly;

        private readonly E[] evaluationPoint;

        private readonly int variable;

        public FastSparseRecursiveEvaluations(MultivariatePolynomial<E> poly, E[] evaluationPoint, int variable)
        {
            this.poly = poly;
            this.variable = variable;
            this.evaluationPoint = evaluationPoint;
        }


        private readonly Dictionary<int, MultivariatePolynomial<MultivariatePolynomial<E>>> bivariateCache 
            = new();


        MultivariatePolynomial<MultivariatePolynomial<E>> getSparseRecursiveForm(int raiseFactor)
        {
            MultivariatePolynomial<MultivariatePolynomial<E>> recForm =
                bivariateCache.GetValueOrDefault(raiseFactor, null);
            if (recForm == null)
            {
                MultivariatePolynomial<E> bivariate;
                if (variable == 1)
                    bivariate = poly;
                else
                {
                    // values for all variables except first and last
                    E[] values = new E[variable - 1];
                    for (int i = 0; i < values.Length; ++i)
                        values[i] = poly.ring.Pow(evaluationPoint[i], raiseFactor);

                    // substitute all that variables to obtain bivariate poly R[x0, xN]
                    bivariate = poly.Evaluate(Utils.Utils.Sequence(1, variable), values);
                }

                if (bivariate.nVariables > 2)
                    bivariate = bivariate.DropSelectVariables(0, variable);
                // swap variables to R[xN, x0]
                bivariate = MultivariatePolynomial<E>.SwapVariables(bivariate, 0, 1);
                // convert to sparse recursive form R[xN][x0]
                recForm = (MultivariatePolynomial<MultivariatePolynomial<E>>)bivariate.ToSparseRecursiveForm();
                bivariateCache.Add(raiseFactor, recForm);
            }

            return recForm;
        }

        public UnivariatePolynomial<E> evaluate(int raiseFactor, E value)
        {
            // get sparse recursive form for fast evaluation
            MultivariatePolynomial<MultivariatePolynomial<E>> recForm = getSparseRecursiveForm(raiseFactor);
            // resulting univariate data
            E[] data = poly.ring.CreateZeroesArray(recForm.Degree() + 1);

            int cacheSize = 128; //recForm.stream().mapToInt(p -> p.Degree()).max().orElse(1);
            // cached exponents for value^i
            MultivariatePolynomial<E>.PrecomputedPowersHolder ph =
                new MultivariatePolynomial<E>.PrecomputedPowersHolder(poly.ring,
                    new MultivariatePolynomial<E>.PrecomputedPowers[]
                        { new MultivariatePolynomial<E>.PrecomputedPowers(cacheSize, value, poly.ring) });
            foreach (Monomial<MultivariatePolynomial<E>> r in recForm.terms)
                // fast Horner-like evaluation of sparse univariate polynomials
                data[r.totalDegree] = MultivariatePolynomial<E>.EvaluateSparseRecursiveForm(r.coefficient, ph, 0);

            return UnivariatePolynomial<E>.Create(poly.ring, data);
        }
    }


    private const int
        SMALL_FIELD_BIT_LENGTH = 13,
        NUMBER_OF_UNDER_DETERMINED_RETRIES = 8,
        NUMBER_OF_UNDER_DETERMINED_RETRIES_SMALL_FIELD = 24;

    sealed class LinZipInterpolation<E> : ASparseInterpolation<E>
    {
        public LinZipInterpolation(Ring<E> ring, int variable, MultivariatePolynomial<E> a, MultivariatePolynomial<E> b,
            ImmutableList<DegreeVector> globalSkeleton, Dictionary<int, MultivariatePolynomial<E>> univarSkeleton,
            int[] sparseUnivarDegrees, int[] evaluationVariables, E[] evaluationPoint,
            MultivariatePolynomial<E>.PrecomputedPowersHolder powers, int expectedNumberOfEvaluations,
            Random rnd) : base(ring, variable, a, b, globalSkeleton, univarSkeleton, sparseUnivarDegrees,
            evaluationVariables,
            evaluationPoint, powers, expectedNumberOfEvaluations, rnd)
        {
        }

        public override MultivariatePolynomial<E> evaluate0(E newPoint)
        {
            LinZipSystem<E>[] systems = new LinZipSystem<E>[sparseUnivarDegrees.Length];
            for (int i = 0; i < sparseUnivarDegrees.Length; i++)
                systems[i] = new LinZipSystem<E>(sparseUnivarDegrees[i], univarSkeleton[sparseUnivarDegrees[i]], powers,
                    variable == -1 ? a.nVariables - 1 : variable - 1);


            int nUnknowns = globalSkeleton.Count, nUnknownScalings = -1;
            int raiseFactor = 0;

            // number of retries before meet raise condition
            int nUnderDeterminedRetries = ring.Characteristic().GetBitLength() <= SMALL_FIELD_BIT_LENGTH
                ? NUMBER_OF_UNDER_DETERMINED_RETRIES_SMALL_FIELD
                : NUMBER_OF_UNDER_DETERMINED_RETRIES;

            int previousFreeVars = -1, underDeterminedTries = 0;
            bool lastChanceUsed = false;
            for (int iTry = 0;; ++iTry)
            {
                if (iTry == nUnderDeterminedRetries)
                {
                    if (lastChanceUsed)
                        // allow this trick only once, then break
                        break;
                    else
                    {
                        // give the last chance
                        lastChanceUsed = true;
                        nUnderDeterminedRetries += nUnderDeterminedLinZip(a, systems, nUnknownScalings);
                    }
                }

                for (;;)
                {
                    // increment at each loop!
                    ++nUnknownScalings;
                    // sequential powers of evaluation point
                    ++raiseFactor;

                    E lastVarValue = newPoint;
                    if (variable == -1)
                        lastVarValue = ring.Pow(lastVarValue, raiseFactor);

                    // evaluate a and b to univariate and calculate gcd
                    UnivariatePolynomial<E>
                        aUnivar = aEvals.evaluate(raiseFactor, lastVarValue),
                        bUnivar = bEvals.evaluate(raiseFactor, lastVarValue),
                        gcdUnivar = UnivariateGCD.PolynomialGCD(aUnivar, bUnivar);

                    if (a.Degree(0) != aUnivar.Degree() || b.Degree(0) != bUnivar.Degree())
                        // unlucky main homomorphism or bad evaluation point
                        return null;

                    Debug.Assert(gcdUnivar.IsMonic());
                    if (!univarSkeleton.Keys.ContainsAll(gcdUnivar.Exponents()))
                        // univariate gcd contains terms that are not present in the skeleton
                        // again unlucky main homomorphism
                        return null;

                    int totalEquations = 0;
                    foreach (LinZipSystem<E> system in systems)
                    {
                        E rhs = gcdUnivar.Degree() < system.univarDegree
                            ? ring.GetZero()
                            : gcdUnivar[system.univarDegree];
                        system.oneMoreEquation(rhs, nUnknownScalings != 0);
                        totalEquations += system.nEquations();
                    }

                    if (nUnknowns + nUnknownScalings <= totalEquations)
                        break;

                    if (underDeterminedTries > nUnderDeterminedRetries)
                        // raise condition: new equations does not fix enough variables
                        return null;

                    int freeVars = nUnknowns + nUnknownScalings - totalEquations;
                    if (freeVars >= previousFreeVars)
                        ++underDeterminedTries;
                    else
                        underDeterminedTries = 0;

                    previousFreeVars = freeVars;
                }

                MultivariatePolynomial<E> result = a.CreateZero();
                LinearSolver.SystemInfo info = solveLinZip(a, systems, nUnknownScalings, result);
                if (info == LinearSolver.SystemInfo.UnderDetermined)
                    //try to generate more equations
                    continue;
                if (info == LinearSolver.SystemInfo.Consistent)
                    //well done
                    return result;
                if (info == LinearSolver.SystemInfo.Inconsistent)
                    //inconsistent system => unlucky homomorphism
                    return null;
            }

            // still under determined
            return null;
        }
    }

    private static int nUnderDeterminedLinZip<E>(MultivariatePolynomial<E> factory, LinZipSystem<E>[] subSystems,
        int nUnknownScalings)
    {
        int nUnknownsMonomials = 0;
        foreach (LinZipSystem<E> system in subSystems)
            nUnknownsMonomials += system.skeleton.Length;

        int nUnknownsTotal = nUnknownsMonomials + nUnknownScalings;
        var lhsGlobal = new List<E[]>();
        int offset = 0;
        Ring<E> ring = factory.ring;
        foreach (LinZipSystem<E> system in subSystems)
        {
            for (int j = 0; j < system.matrix.Count; j++)
            {
                E[] row = ring.CreateZeroesArray(nUnknownsTotal);
                E[] subRow = system.matrix[j];

                Array.Copy(subRow, 0, row, offset, subRow.Length);
                if (j > 0)
                    row[nUnknownsMonomials + j - 1] = system.scalingMatrix[j];
                lhsGlobal.Add(row);
            }

            offset += system.skeleton.Length;
        }

        return LinearSolver.RowEchelonForm(factory.ring, lhsGlobal.ToArray().AsArray2D(), null, false, false);
    }

    private static LinearSolver.SystemInfo solveLinZip<E>(MultivariatePolynomial<E> factory,
        LinZipSystem<E>[] subSystems, int nUnknownScalings, MultivariatePolynomial<E> destination)
    {
        List<Monomial<E>> unknowns = new List<Monomial<E>>();
        foreach (LinZipSystem<E> system in subSystems)
        foreach (Monomial<E> DegreeVector in system.skeleton)
            unknowns.Add(DegreeVector.Set(0, system.univarDegree));

        int nUnknownsMonomials = unknowns.Count;
        int nUnknownsTotal = nUnknownsMonomials + nUnknownScalings;
        List<E[]> lhsGlobal = new List<E[]>();
        List<E> rhsGlobal = new List<E>();
        int offset = 0;
        Ring<E> ring = factory.ring;
        foreach (LinZipSystem<E> system in subSystems)
        {
            for (int j = 0; j < system.matrix.Count; j++)
            {
                E[] row = ring.CreateZeroesArray(nUnknownsTotal);
                E[] subRow = system.matrix[j];

                Array.Copy(subRow, 0, row, offset, subRow.Length);
                if (j > 0)
                    row[nUnknownsMonomials + j - 1] = system.scalingMatrix[j];
                lhsGlobal.Add(row);
                rhsGlobal.Add(system.rhs[j]);
            }

            offset += system.skeleton.Length;
        }

        E[] solution = new E[nUnknownsTotal];
        LinearSolver.SystemInfo info = LinearSolver.Solve(ring, lhsGlobal, rhsGlobal, solution);
        if (info == LinearSolver.SystemInfo.Consistent)
        {
            Monomial<E>[] terms = new Monomial<E>[unknowns.Count];
            for (int i = 0; i < terms.Length; i++)
                terms[i] = unknowns[i].SetCoefficient(solution[i]);
            destination.Add(terms);
        }

        return info;
    }


    sealed class MonicInterpolation<E> : ASparseInterpolation<E>
    {
        readonly int requiredNumberOfEvaluations;

        readonly int MonicScalingExponent;

        public MonicInterpolation(Ring<E> ring, int variable, MultivariatePolynomial<E> a, MultivariatePolynomial<E> b,
            ImmutableList<DegreeVector> globalSkeleton, Dictionary<int, MultivariatePolynomial<E>> univarSkeleton,
            int[] sparseUnivarDegrees, int[] evaluationVariables, E[] evaluationPoint,
            MultivariatePolynomial<E>.PrecomputedPowersHolder powers, int expectedNumberOfEvaluations,
            Random rnd, int requiredNumberOfEvaluations, int MonicScalingExponent) : base(ring, variable, a, b,
            globalSkeleton, univarSkeleton, sparseUnivarDegrees, evaluationVariables,
            evaluationPoint, powers, expectedNumberOfEvaluations, rnd)
        {
            this.requiredNumberOfEvaluations = requiredNumberOfEvaluations;
            this.MonicScalingExponent = MonicScalingExponent;
        }

        public override MultivariatePolynomial<E> evaluate0(E newPoint)
        {
            VandermondeSystem<E>[] systems = new VandermondeSystem<E>[sparseUnivarDegrees.Length];
            for (int i = 0; i < sparseUnivarDegrees.Length; i++)
                systems[i] = new VandermondeSystem<E>(sparseUnivarDegrees[i], univarSkeleton[sparseUnivarDegrees[i]],
                    powers, variable == -1 ? a.nVariables - 1 : variable - 1);

            for (int i = 0; i < requiredNumberOfEvaluations; ++i)
            {
                // sequential powers of evaluation point
                int raiseFactor = i + 1;

                E lastVarValue = newPoint;
                if (variable == -1)
                    lastVarValue = ring.Pow(lastVarValue, raiseFactor);

                // evaluate a and b to univariate and calculate gcd
                UnivariatePolynomial<E>
                    aUnivar = aEvals.evaluate(raiseFactor, lastVarValue),
                    bUnivar = bEvals.evaluate(raiseFactor, lastVarValue),
                    gcdUnivar = UnivariateGCD.PolynomialGCD(aUnivar, bUnivar);

                if (a.Degree(0) != aUnivar.Degree() || b.Degree(0) != bUnivar.Degree())
                    // unlucky main homomorphism or bad evaluation point
                    return null;

                Debug.Assert(gcdUnivar.IsMonic());
                if (!univarSkeleton.Keys.ContainsAll(gcdUnivar.Exponents()))
                    // univariate gcd contains terms that are not present in the skeleton
                    // again unlucky main homomorphism
                    return null;

                if (MonicScalingExponent != -1)
                {
                    // single scaling factor
                    // scale the system according to it

                    if (gcdUnivar.Degree() < MonicScalingExponent || ring.IsZero(gcdUnivar[MonicScalingExponent]))
                        // unlucky homomorphism
                        return null;

                    E normalization = evaluateExceptFirst(ring, powers, ring.GetOne(),
                        univarSkeleton[MonicScalingExponent].Lt(), i + 1,
                        variable == -1 ? a.nVariables - 1 : variable - 1);
                    //normalize univariate gcd in order to reconstruct leading coefficient polynomial
                    normalization = ring.Multiply(ring.Reciprocal(gcdUnivar[MonicScalingExponent]), normalization);
                    gcdUnivar = gcdUnivar.Multiply(normalization);
                }

                bool allDone = true;
                foreach (VandermondeSystem<E> system in systems)
                    if (system.nEquations() < system.nUnknownVariables())
                    {
                        E rhs = gcdUnivar.Degree() < system.univarDegree
                            ? ring.GetZero()
                            : gcdUnivar[system.univarDegree];
                        system.oneMoreEquation(rhs);
                        if (system.nEquations() < system.nUnknownVariables())
                            allDone = false;
                    }

                if (allDone)
                    break;
            }

            foreach (VandermondeSystem<E> system in systems)
            {
                //solve each system
                LinearSolver.SystemInfo info = system.solve();
                if (info != LinearSolver.SystemInfo.Consistent)
                    // system is inconsistent or under determined
                    // unlucky homomorphism
                    return null;
            }

            MultivariatePolynomial<E> gcdVal = a.CreateZero();
            foreach (VandermondeSystem<E> system in systems)
            {
                Debug.Assert(MonicScalingExponent == -1 || system.univarDegree != MonicScalingExponent ||
                             ring.IsOne(system.solution[0]));
                for (int i = 0; i < system.skeleton.Length; i++)
                {
                    Monomial<E> DegreeVector = system.skeleton[i].Set(0, system.univarDegree);
                    E value = system.solution[i];
                    gcdVal.Add(DegreeVector.SetCoefficient(value));
                }
            }

            return gcdVal;
        }
    }

    public abstract class LinearSystem<E>
    {
        public readonly int univarDegree;
        public readonly Ring<E> ring;
        public readonly Monomial<E>[] skeleton;
        public readonly List<E[]> matrix;
        public readonly List<E> rhs = new List<E>();
        public readonly MultivariatePolynomial<E>.PrecomputedPowersHolder powers;
        public readonly int nVars;

        public LinearSystem(int univarDegree, MultivariatePolynomial<E> skeleton,
            MultivariatePolynomial<E>.PrecomputedPowersHolder powers, int nVars)
        {
            this.univarDegree = univarDegree;
            this.ring = skeleton.ring;
            //todo refactor generics
            this.skeleton = skeleton.GetSkeleton().Select(d => (Monomial<E>)d).ToArray();
            this.powers = powers;
            this.nVars = nVars;
            this.matrix = new List<E[]>();
        }

        public int nUnknownVariables()
        {
            return skeleton.Length;
        }

        public int nEquations()
        {
            return matrix.Count;
        }


        public String toString()
        {
            return "{" + string.Join(", ", matrix.Select(row => row.ToString())) + "} = " + rhs;
        }
    }

    private sealed class LinZipSystem<E> : LinearSystem<E>
    {
        public LinZipSystem(int univarDegree, MultivariatePolynomial<E> skeleton,
            MultivariatePolynomial<E>.PrecomputedPowersHolder powers, int nVars) : base(univarDegree, skeleton, powers,
            nVars)
        {
        }

        public readonly List<E> scalingMatrix = new List<E>();

        public void oneMoreEquation(E rhsVal, bool newScalingIntroduced)
        {
            E[] row = new E[skeleton.Length];
            for (int i = 0; i < skeleton.Length; i++)
                row[i] = evaluateExceptFirst(ring, powers, ring.GetOne(), skeleton[i], matrix.Count + 1, nVars);
            matrix.Add(row);

            if (newScalingIntroduced)
            {
                scalingMatrix.Add(ring.Negate(rhsVal));
                rhsVal = ring.GetZero();
            }
            else
                scalingMatrix.Add(ring.GetZero());

            rhs.Add(rhsVal);
        }

        public new String toString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < matrix.Count; i++)
            {
                E[] row = matrix[i];
                for (int j = 0; j < row.Length; j++)
                {
                    if (j != 0)
                        sb.Append("+");
                    sb.Append(row[j]).Append("*c" + j);
                }

                if (i != 0)
                    sb.Append("+").Append(scalingMatrix[i]).Append("*m" + (i - 1));

                sb.Append("=").Append(rhs[i]).Append("\n");
            }

            return sb.ToString();
        }
    }

    public sealed class VandermondeSystem<E> : LinearSystem<E>
    {
        public VandermondeSystem(int univarDegree, MultivariatePolynomial<E> skeleton,
            MultivariatePolynomial<E>.PrecomputedPowersHolder powers, int nVars) : base(univarDegree, skeleton, powers,
            nVars)
        {
        }

        public E[]? solution = null;

        public LinearSolver.SystemInfo solve()
        {
            if (solution == null)
                solution = new E[nUnknownVariables()];

            if (nUnknownVariables() <= 8)
                // for small systems Gaussian elimination is indeed faster
                return LinearSolver.Solve(ring, matrix.ToArray().AsArray2D(), rhs.ToArray(), solution);

            // solve vandermonde system
            E[] vandermondeRow = matrix[0];
            LinearSolver.SystemInfo
                info = LinearSolver.SolveVandermondeT(ring, vandermondeRow, rhs.ToArray(), solution);
            if (info == LinearSolver.SystemInfo.Consistent)
                for (int i = 0; i < solution.Length; ++i)
                    solution[i] = ring.DivideExact(solution[i], vandermondeRow[i]);

            return info;
        }

        public VandermondeSystem<E> oneMoreEquation(E rhsVal)
        {
            E[] row = new E[skeleton.Length];
            for (int i = 0; i < skeleton.Length; i++)
                row[i] = evaluateExceptFirst(ring, powers, ring.GetOne(), skeleton[i], matrix.Count + 1, nVars);
            matrix.Add(row);
            rhs.Add(rhsVal);
            return this;
        }
    }

    private static E evaluateExceptFirst<E>(Ring<E> ring,
        MultivariatePolynomial<E>.PrecomputedPowersHolder powers,
        E coefficient,
        Monomial<E> skeleton,
        int raiseFactor,
        int nVars)
    {
        E tmp = coefficient;
        for (int k = 1; k <= nVars; k++)
            tmp = ring.Multiply(tmp, powers.Pow(k, raiseFactor * skeleton.exponents[k]));
        return tmp;
    }

    private static bool isVandermonde<E>(E[][] lhs, Ring<E> ring)
    {
        for (int i = 1; i < lhs.Length; i++)
        {
            for (int j = 0; j < lhs[0].Length; j++)
            {
                if (!lhs[i][j].Equals(ring.Pow(lhs[0][j], i + 1)))
                    return false;
            }
        }

        return true;
    }
//
//
//     /* ================================================ Machine numbers ============================================= */
//
//     /**
//      * Calculates GCD of two multivariate polynomials over Zp using Brown's algorithm with dense interpolation.
//      *
//      * @param a the first multivariate polynomial
//      * @param b the second multivariate polynomial
//      * @return greatest common divisor of {@code a} and {@code b}
//      */
//     
//     public static MultivariatePolynomialZp64 BrownGCD(
//             MultivariatePolynomialZp64 a,
//             MultivariatePolynomialZp64 b) {
//
//         // prepare input and test for early termination
//         GCDInput<MonomialZp64, MultivariatePolynomialZp64> gcdInput = preparedGCDInput(a, b, MultivariateGCD::BrownGCD);
//         if (gcdInput.earlyGCD != null)
//             return gcdInput.earlyGCD;
//
//         if (gcdInput.finiteExtensionDegree > 1)
//             return lKaltofenMonaganSparseModularGCDInGF(gcdInput);
//
//         MultivariatePolynomialZp64 result = BrownGCD(
//                 gcdInput.aReduced, gcdInput.bReduced, PrivateRandom.GetRandom(),
//                 gcdInput.lastPresentVariable, gcdInput.DegreeBounds, gcdInput.evaluationStackLimit);
//         if (result == null)
//             // ground fill is too small for modular algorithm
//             return lKaltofenMonaganSparseModularGCDInGF(gcdInput);
//
//         return gcdInput.restoreGCD(result);
//     }
//
//     /**
//      * Actual implementation of dense interpolation
//      *
//      * @param variable             current variable (all variables {@code v > variable} are fixed so far)
//      * @param DegreeBounds         Degree bounds for gcd
//      * @param evaluationStackLimit ring cardinality
//      */
//     
//     private static MultivariatePolynomialZp64 BrownGCD(
//             MultivariatePolynomialZp64 a,
//             MultivariatePolynomialZp64 b,
//             Random rnd,
//             int variable,
//             int[] DegreeBounds,
//             int evaluationStackLimit) {
//
//         //check for trivial gcd
//         MultivariatePolynomialZp64 trivialGCD = trivialGCD(a, b);
//         if (trivialGCD != null)
//             return trivialGCD;
//
//         MultivariatePolynomialZp64 factory = a;
//         int nVariables = factory.nVariables;
//         if (variable == 0)
//         // switch to univariate gcd
//         {
//             UnivariatePolynomialZp64 gcd = UnivariateGCD.PolynomialGCD(a.asUnivariate(), b.asUnivariate());
//             if (gcd.Degree() == 0)
//                 return factory.CreateOne();
//             return AMultivariatePolynomial.asMultivariate(gcd, nVariables, variable, factory.ordering);
//         }
//
//         PrimitiveInput<MonomialZp64, MultivariatePolynomialZp64, UnivariatePolynomialZp64> primitiveInput = makePrimitive(a, b, variable);
//         // primitive parts of a and b as Zp[x_k][x_1 ... x_{k-1}]
//         a = primitiveInput.aPrimitive;
//         b = primitiveInput.bPrimitive;
//         // gcd of Zp[x_k] Content and lc
//         UnivariatePolynomialZp64
//                 ContentGCD = primitiveInput.ContentGCD,
//                 lcGCD = primitiveInput.lcGCD;
//
//         //check again for trivial gcd
//         trivialGCD = trivialGCD(a, b);
//         if (trivialGCD != null) {
//             MultivariatePolynomialZp64 poly = AMultivariatePolynomial.asMultivariate(ContentGCD, a.nVariables, variable, a.ordering);
//             return trivialGCD.Multiply(poly);
//         }
//
//         IntegersZp64 ring = factory.ring;
//         //Degree bound for the previous variable
//         int prevVarExponent = DegreeBounds[variable - 1];
//         //dense interpolation
//         MultivariateInterpolation.InterpolationZp64 interpolation = null;
//         //previous interpolation (used to detect whether update doesn't change the result)
//         MultivariatePolynomialZp64 previousInterpolation;
//         //store points that were already used in interpolation
//         TLongHashSet evaluationStack = new TLongHashSet();
//
//         int[] aDegrees = a.Degrees(), bDegrees = b.Degrees();
//         main:
//         while (true) {
//             if (evaluationStackLimit == evaluationStack.Size())
//                 // all elements of the ring are tried
//                 // do division check (last chance) and return
//                 return doDivisionCheck(a, b, ContentGCD, interpolation, variable);
//
//             //pickup the next random element for variable
//             long randomPoint = ring.randomElement(rnd);
//             if (evaluationStack.contains(randomPoint))
//                 continue;
//             evaluationStack.add(randomPoint);
//
//             long lcVal = lcGCD.evaluate(randomPoint);
//             if (lcVal == 0)
//                 continue;
//
//             // evaluate a and b at variable = randomPoint
//             MultivariatePolynomialZp64
//                     aVal = a.evaluate(variable, randomPoint),
//                     bVal = b.evaluate(variable, randomPoint);
//
//             // check for unlucky substitution
//             int[] aValDegrees = aVal.Degrees(), bValDegrees = bVal.Degrees();
//             for (int i = variable - 1; i >= 0; --i)
//                 if (aDegrees[i] != aValDegrees[i] || bDegrees[i] != bValDegrees[i])
//                     continue main;
//
//             // calculate gcd of the result by the recursive call
//             MultivariatePolynomialZp64 cVal = BrownGCD(aVal, bVal, rnd, variable - 1, DegreeBounds, evaluationStackLimit);
//             if (cVal == null)
//                 //unlucky homomorphism
//                 continue;
//
//             int currExponent = cVal.Degree(variable - 1);
//             if (currExponent > prevVarExponent)
//                 //unlucky homomorphism
//                 continue;
//
//             // normalize gcd
//             cVal = cVal.Multiply(ring.Multiply(ring.reciprocal(cVal.Lc()), lcVal));
//             assert cVal.Lc() == lcVal;
//
//             if (currExponent < prevVarExponent) {
//                 //better Degree bound detected => start over
//                 interpolation = new MultivariateInterpolation.InterpolationZp64(variable, randomPoint, cVal);
//                 DegreeBounds[variable - 1] = prevVarExponent = currExponent;
//                 continue;
//             }
//
//             if (interpolation == null) {
//                 //first successful homomorphism
//                 interpolation = new MultivariateInterpolation.InterpolationZp64(variable, randomPoint, cVal);
//                 continue;
//             }
//
//             // Cache previous interpolation. NOTE: Clone() is important, since the poly will
//             // be modified inplace by the update() method
//             previousInterpolation = interpolation.getInterpolatingPolynomial().Clone();
//             // interpolate
//             interpolation.update(randomPoint, cVal);
//
//             // do division test
//             if (DegreeBounds[variable] <= interpolation.numberOfPoints()
//                     || previousInterpolation.equals(interpolation.getInterpolatingPolynomial())) {
//                 MultivariatePolynomialZp64 result = doDivisionCheck(a, b, ContentGCD, interpolation, variable);
//                 if (result != null)
//                     return result;
//             }
//         }
//     }
//
//     
//     static ZippelEvaluationsZp64 CreateEvaluations(MultivariatePolynomialZp64 poly,
//                                                    int[] evaluationVariables,
//                                                    long[] evaluationPoint,
//                                                    MultivariatePolynomialZp64.lPrecomputedPowersHolder basePowers,
//                                                    int expectedNumberOfEvaluations) {
//         if (expectedNumberOfEvaluations > N_EVALUATIONS_RECURSIVE_SWITCH
//                 && poly.Size() > SIZE_OF_POLY_RECURSIVE_SWITCH)
//             return new FastSparseRecursiveEvaluationsZp64(poly, evaluationPoint, evaluationVariables[evaluationVariables.Length - 1]);
//         else
//             return new PlainEvaluationsZp64(poly, evaluationVariables, evaluationPoint, basePowers);
//     }
//
//     
//     interface ZippelEvaluationsZp64 {
//         
//         UnivariatePolynomialZp64 evaluate(int raiseFactor, long value);
//     }
//
//     
//     static final class PlainEvaluationsZp64 implements ZippelEvaluationsZp64 {
//         
//         private final MultivariatePolynomialZp64 poly;
//         /**
//          * variables that will be substituted with random values for sparse interpolation, i.e. {@code [1, 2 ...
//          * variable] }
//          */
//         private final int[] evaluationVariables;
//         
//         private final long[] evaluationPoint;
//         
//         private final MultivariatePolynomialZp64.lPrecomputedPowersHolder basePowers;
//         
//         private final TIntObjectHashMap<MultivariatePolynomialZp64.lPrecomputedPowersHolder> powersCache
//                 = new TIntObjectHashMap<>();
//
//         PlainEvaluationsZp64(MultivariatePolynomialZp64 poly,
//                              int[] evaluationVariables,
//                              long[] evaluationPoint,
//                              MultivariatePolynomialZp64.lPrecomputedPowersHolder basePowers) {
//             this.poly = poly;
//             this.evaluationVariables = evaluationVariables;
//             this.evaluationPoint = evaluationPoint;
//             this.basePowers = basePowers.Clone();
//             this.powersCache.put(1, this.basePowers);
//         }
//
//         @Override
//         public UnivariatePolynomialZp64 evaluate(int raiseFactor, long value) {
//             basePowers.set(evaluationVariables[evaluationVariables.Length - 1], value);
//
//             MultivariatePolynomialZp64.lPrecomputedPowersHolder powers = powersCache.get(raiseFactor);
//             IntegersZp64 ring = poly.ring;
//             if (powers == null) {
//                 powers = basePowers.Clone();
//                 for (int i = 0; i < (evaluationVariables.Length - 1); ++i)
//                     powers.set(evaluationVariables[i], ring.powMod(evaluationPoint[i], raiseFactor));
//                 powersCache.put(raiseFactor, powers);
//             }
//             powers.set(evaluationVariables[evaluationVariables.Length - 1], value);
//
//             long[] result = new long[poly.Degree(0) + 1];
//             for (MonomialZp64 el : poly) {
//                 long ucf = el.coefficient;
//                 for (int variable : evaluationVariables)
//                     ucf = ring.Multiply(ucf, powers.pow(variable, el.exponents[variable]));
//                 int uDeg = el.exponents[0];
//                 result[uDeg] = ring.add(result[uDeg], ucf);
//             }
//
//             return UnivariatePolynomialZp64.Create(ring, result);
//         }
//     }
//
//
//     
//     static final class FastSparseRecursiveEvaluationsZp64 implements ZippelEvaluationsZp64 {
//         
//         private final MultivariatePolynomialZp64 poly;
//         
//         private final long[] evaluationPoint;
//         
//         private final int variable;
//
//         FastSparseRecursiveEvaluationsZp64(MultivariatePolynomialZp64 poly, long[] evaluationPoint, int variable) {
//             this.poly = poly;
//             this.variable = variable;
//             this.evaluationPoint = evaluationPoint;
//         }
//
//         
//         private final TIntObjectHashMap<MultivariatePolynomial<MultivariatePolynomialZp64>> bivariateCache
//                 = new TIntObjectHashMap<>();
//
//         /**
//          * returns a sparse recursive form of initial poly with all but first and last variables evaluated with given
//          * raise factor (that is poly in R[xN][x0])
//          */
//         
//         MultivariatePolynomial<MultivariatePolynomialZp64> getSparseRecursiveForm(int raiseFactor) {
//             MultivariatePolynomial<MultivariatePolynomialZp64> recForm = bivariateCache.get(raiseFactor);
//             if (recForm == null) {
//                 MultivariatePolynomialZp64 bivariate;
//                 if (variable == 1)
//                     bivariate = poly;
//                 else {
//                     // values for all variables except first and last
//                     long[] values = new long[variable - 1];
//                     for (int i = 0; i < values.Length; ++i)
//                         values[i] = poly.ring.powMod(evaluationPoint[i], raiseFactor);
//
//                     // substitute all that variables to obtain bivariate poly R[x0, xN]
//                     bivariate = poly.evaluate(Utils.Utils.Sequence(1, variable), values);
//                 }
//                 if (bivariate.nVariables > 2)
//                     bivariate = bivariate.dropSelectVariables(0, variable);
//                 // swap variables to R[xN, x0]
//                 bivariate = AMultivariatePolynomial.swapVariables(bivariate, 0, 1);
//                 // convert to sparse recursive form R[xN][x0]
//                 recForm = (MultivariatePolynomial<MultivariatePolynomialZp64>) bivariate.toSparseRecursiveForm();
//                 bivariateCache.put(raiseFactor, recForm);
//             }
//             return recForm;
//         }
//
//         /**
//          * Evaluate initial poly with a given raise factor for all but first and last variable and with a given value
//          * for the last variable
//          */
//         @Override
//         public UnivariatePolynomialZp64 evaluate(int raiseFactor, long value) {
//             // get sparse recursive form for fast evaluation
//             MultivariatePolynomial<MultivariatePolynomialZp64> recForm = getSparseRecursiveForm(raiseFactor);
//             // resulting univariate data
//             long[] data = new long[recForm.Degree() + 1];
//
//             int cacheSize = 128;//recForm.stream().mapToInt(p -> p.Degree()).max().orElse(1);
//             // cached exponents for value^i
//             MultivariatePolynomialZp64.lPrecomputedPowersHolder ph =
//                     new MultivariatePolynomialZp64.lPrecomputedPowersHolder(poly.ring,
//                             new MultivariatePolynomialZp64.lPrecomputedPowers[]{new MultivariatePolynomialZp64.lPrecomputedPowers(cacheSize, value, poly.ring)});
//             for (Monomial<MultivariatePolynomialZp64> r : recForm)
//                 // fast Horner-like evaluation of sparse univariate polynomials
//                 data[r.totalDegree] = MultivariatePolynomialZp64.evaluateSparseRecursiveForm(r.coefficient, ph, 0);
//
//             return UnivariatePolynomialZp64.Create(poly.ring, data);
//         }
//     }
//

//
//     private static int nUnderDeterminedLinZip(MultivariatePolynomialZp64 factory,
//                                               lLinZipSystem[] subSystems,
//                                               int nUnknownScalings) {
//
//         int nUnknownsMonomials = 0;
//         for (lLinZipSystem system : subSystems)
//             nUnknownsMonomials += system.skeleton.Length;
//
//         int nUnknownsTotal = nUnknownsMonomials + nUnknownScalings;
//         ArrayList<long[]> lhsGlobal = new ArrayList<>();
//         int offset = 0;
//         for (lLinZipSystem system : subSystems) {
//             for (int j = 0; j < system.matrix.Size(); j++) {
//                 long[] row = new long[nUnknownsTotal];
//                 long[] subRow = system.matrix.get(j);
//
//                 System.arraycopy(subRow, 0, row, offset, subRow.Length);
//                 if (j > 0)
//                     row[nUnknownsMonomials + j - 1] = system.scalingMatrix.get(j);
//                 lhsGlobal.add(row);
//             }
//
//             offset += system.skeleton.Length;
//         }
//
//         return LinearSolver.rowEchelonForm(factory.ring, lhsGlobal.toArray(new long[lhsGlobal.Size()][]), null, false, false);
//     }
//
//     private static LinearSolver.SystemInfo solveLinZip(MultivariatePolynomialZp64 factory,
//                                                        lLinZipSystem[] subSystems,
//                                                        int nUnknownScalings,
//                                                        MultivariatePolynomialZp64 destination) {
//         ArrayList<MonomialZp64> unknowns = new ArrayList<>();
//         for (lLinZipSystem system : subSystems)
//             for (MonomialZp64 DegreeVector : system.skeleton)
//                 unknowns.add(DegreeVector.set(0, system.univarDegree));
//
//         int nUnknownsMonomials = unknowns.Size();
//         int nUnknownsTotal = nUnknownsMonomials + nUnknownScalings;
//         ArrayList<long[]> lhsGlobal = new ArrayList<>();
//         TLongArrayList rhsGlobal = new TLongArrayList();
//         int offset = 0;
//         IntegersZp64 ring = factory.ring;
//         for (lLinZipSystem system : subSystems) {
//             for (int j = 0; j < system.matrix.Size(); j++) {
//                 long[] row = new long[nUnknownsTotal];
//                 long[] subRow = system.matrix.get(j);
//
//                 System.arraycopy(subRow, 0, row, offset, subRow.Length);
//                 if (j > 0)
//                     row[nUnknownsMonomials + j - 1] = system.scalingMatrix.get(j);
//                 lhsGlobal.add(row);
//                 rhsGlobal.add(system.rhs.get(j));
//             }
//
//             offset += system.skeleton.Length;
//         }
//
//         long[] solution = new long[nUnknownsTotal];
//         LinearSolver.SystemInfo info = LinearSolver.solve(ring, lhsGlobal, rhsGlobal, solution);
//         if (info == LinearSolver.SystemInfo.Consistent) {
//             
//             MonomialZp64[] terms = new MonomialZp64[unknowns.Size()];
//             for (int i = 0; i < terms.Length; i++)
//                 terms[i] = unknowns.get(i).setCoefficient(solution[i]);
//             destination.add(terms);
//         }
//
//         return info;
//     }
//

//
//     static abstract class lLinearSystem {
//         final int univarDegree;
//         
//         final IntegersZp64 ring;
//         
//         final MonomialZp64[] skeleton;
//         
//         final ArrayList<long[]> matrix;
//         
//         final TLongArrayList rhs = new TLongArrayList();
//         
//         final MultivariatePolynomialZp64.lPrecomputedPowersHolder powers;
//         
//         final int nVars;
//
//         
//         lLinearSystem(int univarDegree, MultivariatePolynomialZp64 skeleton, MultivariatePolynomialZp64.lPrecomputedPowersHolder powers, int nVars) {
//             this.univarDegree = univarDegree;
//             this.ring = skeleton.ring;
//             //todo refactor generics
//             this.skeleton = skeleton.getSkeleton().toArray(new MonomialZp64[skeleton.Size()]);
//             this.powers = powers;
//             this.nVars = nVars;
//             this.matrix = new ArrayList<>();
//         }
//
//         final int nUnknownVariables() {
//             return skeleton.Length;
//         }
//
//         final int nEquations() {
//             return matrix.Size();
//         }
//
//         @Override
//         public String toString() {
//             return "{" + matrix.stream().map(Arrays::toString).collect(Collectors.joining(",")) + "} = " + rhs;
//         }
//     }
//
//     
//     static final class lLinZipSystem extends lLinearSystem {
//         public lLinZipSystem(int univarDegree, MultivariatePolynomialZp64 skeleton, MultivariatePolynomialZp64.lPrecomputedPowersHolder powers, int nVars) {
//             super(univarDegree, skeleton, powers, nVars);
//         }
//
//         private final TLongArrayList scalingMatrix = new TLongArrayList();
//
//         public void oneMoreEquation(long rhsVal, bool newScalingIntroduced) {
//             long[] row = new long[skeleton.Length];
//             for (int i = 0; i < skeleton.Length; i++)
//                 row[i] = evaluateExceptFirst(ring, powers, 1, skeleton[i], matrix.Size() + 1, nVars);
//             matrix.add(row);
//
//             if (newScalingIntroduced) {
//                 scalingMatrix.add(ring.negate(rhsVal));
//                 rhsVal = 0;
//             } else
//                 scalingMatrix.add(0);
//             rhs.add(rhsVal);
//         }
//     }
//
//     
//     static final class lVandermondeSystem extends lLinearSystem {
//         public lVandermondeSystem(int univarDegree, MultivariatePolynomialZp64 skeleton, MultivariatePolynomialZp64.lPrecomputedPowersHolder powers, int nVars) {
//             super(univarDegree, skeleton, powers, nVars);
//         }
//
//         long[] solution = null;
//
//         LinearSolver.SystemInfo solve() {
//             if (solution == null)
//                 solution = new long[nUnknownVariables()];
//
//             if (nUnknownVariables() <= 8)
//                 // for small systems Gaussian elimination is indeed faster
//                 return LinearSolver.solve(ring, matrix.toArray(new long[matrix.Size()][]), rhs.toArray(), solution);
//
//             // solve vandermonde system
//             long[] vandermondeRow = matrix.get(0);
//             LinearSolver.SystemInfo info = LinearSolver.solveVandermondeT(ring, vandermondeRow, rhs.toArray(), solution);
//             if (info == LinearSolver.SystemInfo.Consistent)
//                 for (int i = 0; i < solution.Length; ++i)
//                     solution[i] = ring.divide(solution[i], vandermondeRow[i]);
//
//             return info;
//         }
//
//         public lVandermondeSystem oneMoreEquation(long rhsVal) {
//             long[] row = new long[skeleton.Length];
//             for (int i = 0; i < skeleton.Length; i++)
//                 row[i] = evaluateExceptFirst(ring, powers, 1, skeleton[i], matrix.Size() + 1, nVars);
//             matrix.add(row);
//             rhs.add(rhsVal);
//             return this;
//         }
//     }
//
//     private static long evaluateExceptFirst(IntegersZp64 ring,
//                                             MultivariatePolynomialZp64.lPrecomputedPowersHolder powers,
//                                             long coefficient,
//                                             MonomialZp64 skeleton,
//                                             int raiseFactor,
//                                             int nVars) {
//         long tmp = coefficient;
//         for (int k = 1; k <= nVars; k++)
//             tmp = ring.Multiply(tmp, powers.pow(k, raiseFactor * skeleton.exponents[k]));
//         return tmp;
//     }
//
//     /* =============================================== EZ-GCD algorithm ============================================ */
//
//
//     /**
//      * Calculates GCD of two multivariate polynomials over Zp using EZ algorithm
//      *
//      * @param a the first multivariate polynomial
//      * @param b the second multivariate polynomial
//      * @return greatest common divisor of {@code a} and {@code b}
//      */
//     
//     public static MultivariatePolynomialZp64 EZGCD(
//             MultivariatePolynomialZp64 a,
//             MultivariatePolynomialZp64 b) {
//
//         // prepare input and test for early termination
//         GCDInput<MonomialZp64, MultivariatePolynomialZp64> gcdInput = preparedGCDInput(a, b, MultivariateGCD::EZGCD);
//         if (gcdInput.earlyGCD != null)
//             return gcdInput.earlyGCD;
//
//         a = gcdInput.aReduced;
//         b = gcdInput.bReduced;
//
//         // remove Content in each variable
//         MultivariatePolynomialZp64 Content = a.CreateOne();
//         for (int i = 0; i < a.nVariables; ++i) {
//             if (a.Degree(i) == 0 || b.Degree(i) == 0)
//                 continue;
//             MultivariatePolynomialZp64 tmpContent = ContentGCD(a, b, i, MultivariateGCD::EZGCD);
//             a = MultivariateDivision.divideExact(a, tmpContent);
//             b = MultivariateDivision.divideExact(b, tmpContent);
//             Content = Content.Multiply(tmpContent);
//         }
//
//         // one more reduction; removing of Content may shuffle required variables order
//         GCDInput<MonomialZp64, MultivariatePolynomialZp64> gcdInput2 = preparedGCDInput(a, b, MultivariateGCD::EZGCD);
//         a = gcdInput2.aReduced;
//         b = gcdInput2.bReduced;
//
//         MultivariatePolynomialZp64 result = gcdInput2.earlyGCD != null
//                 ? gcdInput2.earlyGCD
//                 : gcdInput2.restoreGCD(EZGCD0(a, b, gcdInput2.lastPresentVariable + 1, PrivateRandom.GetRandom()));
//
//         result = result.Multiply(Content);
//         return gcdInput.restoreGCD(result);
//     }
//
//     
//     private static MultivariatePolynomialZp64 EZGCD0(
//             MultivariatePolynomialZp64 a, MultivariatePolynomialZp64 b, int nUsedVariables, Random rnd) {
//
//         // Degree of univariate gcd
//         int ugcdDegree = Integer.MAX_VALUE;
//
//         EZGCDEvaluations evaluations = new EZGCDEvaluations(a, b, nUsedVariables, rnd);
//         HenselLifting.lEvaluation evaluation = new HenselLifting.lEvaluation(a.nVariables, ArraysUtil.arrayOf(0L, a.nVariables - 1), a.ring, a.ordering);
//         choose_evaluation:
//         while (true) {
//             // set new evaluation point (true returned if base variable has changed)
//             bool swapped = evaluations.nextEvaluation();
//             if (swapped)
//                 ugcdDegree = Integer.MAX_VALUE;
//             a = evaluations.aReduced;
//             b = evaluations.bReduced;
//
//             // Degrees of a and b as Z[y1, ... ,yN][x]
//             int
//                     uaDegree = a.Degree(0),
//                     ubDegree = b.Degree(0);
//
//             UnivariatePolynomialZp64
//                     ua = a.evaluateAtZeroAllExcept(0),
//                     ub = b.evaluateAtZeroAllExcept(0);
//
//             assert ua.Degree() == uaDegree;
//             assert ub.Degree() == ubDegree;
//
//             // gcd of a mod I and b mod I (univariate)
//             UnivariatePolynomialZp64 ugcd = UnivariateGCD.PolynomialGCD(ua, ub);
//
//             if (ugcd.Degree() == 0) {
//                 // coprime polynomials
//                 return evaluations.reconstruct(a.CreateOne());
//             }
//
//             if (ugcd.Degree() > ugcdDegree)
//                 // unlucky evaluation
//                 continue choose_evaluation;
//
//             ugcdDegree = ugcd.Degree();
//
//             if (ugcdDegree == uaDegree) {
//                 // a is a divisor of b
//                 if (MultivariateDivision.DividesQ(b, a))
//                     return evaluations.reconstruct(a);
//                 //continue choose_evaluation;
//             }
//
//             if (ugcdDegree == ubDegree) {
//                 // b is a divisor of a
//                 if (MultivariateDivision.DividesQ(a, b))
//                     return evaluations.reconstruct(b);
//                 //continue choose_evaluation;
//             }
//
//             // base polynomial to lift (either a or b)
//             MultivariatePolynomialZp64 base = null;
//             // a cofactor with gcd to lift, i.e. base = ugcd * uCoFactor mod I
//             UnivariatePolynomialZp64 uCoFactor = null;
//             MultivariatePolynomialZp64 coFactorLC = null;
//
//             // deg(b) < deg(a), so it is better to try to lift b
//             UnivariatePolynomialZp64 ubCoFactor = UnivariateDivision.quotient(ub, ugcd, true);
//             if (UnivariateGCD.PolynomialGCD(ugcd, ubCoFactor).IsConstant()) {
//                 // b splits into coprime factors
//                 base = b;
//                 uCoFactor = ubCoFactor;
//                 coFactorLC = b.lc(0);
//             }
//
//             if (base == null) {
//                 // b does not split into coprime factors => try a
//                 UnivariatePolynomialZp64 uaCoFactor = UnivariateDivision.quotient(ua, ugcd, true);
//                 if (UnivariateGCD.PolynomialGCD(ugcd, uaCoFactor).IsConstant()) {
//                     base = a;
//                     uCoFactor = uaCoFactor;
//                     coFactorLC = a.lc(0);
//                 }
//             }
//
//             if (base == null) {
//                 // neither a nor b does not split into coprime factors => square free decomposition required
//
//                 MultivariatePolynomialZp64
//                         bRepeatedFactor = EZGCD(b, b.derivative(0, 1)),
//                         squareFreeGCD = EZGCD(MultivariateDivision.divideExact(b, bRepeatedFactor), a),
//                         aRepeatedFactor = MultivariateDivision.divideExact(a, squareFreeGCD),
//                         gcd = squareFreeGCD.Clone();
//
//                 UnivariatePolynomialZp64
//                         uSquareFreeGCD = squareFreeGCD.evaluateAtZeroAllExcept(0),
//                         uaRepeatedFactor = aRepeatedFactor.evaluateAtZeroAllExcept(0),
//                         ubRepeatedFactor = bRepeatedFactor.evaluateAtZeroAllExcept(0);
//                 while (true) {
//                     ugcd = UnivariateGCD.PolynomialGCD(
//                             uSquareFreeGCD, uaRepeatedFactor, ubRepeatedFactor);
//
//                     if (ugcd.Degree() == 0)
//                         return evaluations.reconstruct(gcd);
//
//                     if (!uSquareFreeGCD.Clone().Monic().equals(ugcd.Clone().Monic())) {
//                         MultivariatePolynomialZp64
//                                 mgcd = MultivariatePolynomialZp64.asMultivariate(ugcd, a.nVariables, 0, a.ordering),
//                                 gcdCoFactor = MultivariatePolynomialZp64.asMultivariate(
//                                         UnivariateDivision.divideExact(uSquareFreeGCD, ugcd, false), a.nVariables, 0, a.ordering);
//
//                         liftPairAutomaticLC(squareFreeGCD, mgcd, gcdCoFactor, evaluation);
//
//                         squareFreeGCD = mgcd.Clone();
//                         uSquareFreeGCD = squareFreeGCD.evaluateAtZeroAllExcept(0);
//                     }
//
//                     gcd = gcd.Multiply(squareFreeGCD);
//                     uaRepeatedFactor = UnivariateDivision.divideExact(uaRepeatedFactor, ugcd, false);
//                     ubRepeatedFactor = UnivariateDivision.divideExact(ubRepeatedFactor, ugcd, false);
//                 }
//             }
//
//             MultivariatePolynomialZp64 gcd = MultivariatePolynomialZp64.asMultivariate(ugcd, a.nVariables, 0, a.ordering);
//             MultivariatePolynomialZp64 coFactor = MultivariatePolynomialZp64.asMultivariate(uCoFactor, a.nVariables, 0, a.ordering);
//
//             // impose the leading coefficient
//             MultivariatePolynomialZp64 lcCorrection = EZGCD(a.lc(0), b.lc(0));
//             assert ZippelGCD(a.lc(0), b.lc(0)).Monic(lcCorrection.Lc()).equals(lcCorrection) : "\n" + a.lc(0) + "  \n " + b.lc(0);
//
//             if (lcCorrection.isOne()) {
//                 assert gcd.IsMonic();
//                 assert evaluation.evaluateFrom(base, 1).equals(gcd.Clone().Multiply(coFactor));
//                 liftPair(base, gcd, coFactor, null, base.lc(0), evaluation);
//             } else {
//                 long lcCorrectionMod = lcCorrection.cc(); // substitute all zeros
//                 assert lcCorrectionMod != 0;
//
//                 coFactor = coFactor.Multiply(gcd.Lc());
//                 gcd = gcd.Monic(lcCorrectionMod);
//
//                 liftPair(base.Clone().Multiply(lcCorrection), gcd, coFactor, lcCorrection, coFactorLC, evaluation);
//                 assert gcd.lc(0).equals(lcCorrection);
//                 gcd = HenselLifting.primitivePart(gcd);
//             }
//
//             if (isGCDTriplet(b, a, gcd))
//                 return evaluations.reconstruct(gcd);
//         }
//     }
//
     private static void liftPair<E>(MultivariatePolynomial<E> @base, MultivariatePolynomial<E> a, MultivariatePolynomial<E> b, MultivariatePolynomial<E> aLC, MultivariatePolynomial<E> bLC, HenselLifting.IEvaluation<E> evaluation) {
         HenselLifting.multivariateLift0(@base,
             [a, b],
             [aLC, bLC],
                 evaluation,
             @base.Degrees());
     }

     private static void liftPairAutomaticLC<E>(MultivariatePolynomial<E> @base, MultivariatePolynomial<E> a, MultivariatePolynomial<E> b, HenselLifting.IEvaluation<E> evaluation) {
         HenselLifting.multivariateLiftAutomaticLC(@base,
                 [a, b],
                 evaluation);
     }
//
//     static ZeroVariables commonPossibleZeroes(MultivariatePolynomialZp64 a, MultivariatePolynomialZp64 b, int nVariables) {
//         return commonPossibleZeroes(possibleZeros(a, nVariables), possibleZeros(b, nVariables));
//     }
//
//     static ZeroVariables commonPossibleZeroes(ZeroVariables a, ZeroVariables b) {
//         if (a.allZeroes)
//             return b;
//         if (b.allZeroes)
//             return a;
//
//         ZeroVariables result = new ZeroVariables(a.nVariables);
//         for (BitSet az : a.pZeros)
//             for (BitSet bz : b.pZeros) {
//                 BitSet tmp = (BitSet) az.Clone();
//                 tmp.and(bz);
//                 result.add(tmp);
//             }
//
//         return result;
//     }
//
//     
//     static ZeroVariables possibleZeros(MultivariatePolynomialZp64 poly, int nVariables) {
//         ZeroVariables result = new ZeroVariables(nVariables);
//         if (poly.cc() != 0) {
//             BitSet s = new BitSet(nVariables);
//             s.set(0, nVariables);
//             result.add(s);
//             return result;
//         }
//         for (MonomialZp64 term : poly) {
//             BitSet zeroes = new BitSet(nVariables);
//             for (int i = 0; i < nVariables; i++)
//                 if (term.exponents[i] == 0)
//                     zeroes.set(i);
//             result.add(zeroes);
//         }
//         return result;
//     }
//
//     
//     static final class ZeroVariables {
//         final List<BitSet> pZeros = new ArrayList<>();
//         final int nVariables;
//
//         public ZeroVariables(int nVariables) {
//             this.nVariables = nVariables;
//         }
//
//         int iMaxPossibleZeros = -1, maxPossibleZeros = -1;
//         bool allZeroes = false;
//
//         void add(BitSet zeros) {
//             if (allZeroes)
//                 return;
//             int nZeros = zeros.cardinality();
//             if (nVariables == nZeros) {
//                 maxPossibleZeros = zeros.Length();
//                 iMaxPossibleZeros = 0;
//                 allZeroes = true;
//                 pZeros.clear();
//                 pZeros.add(zeros);
//                 return;
//             }
//             IEnumerable<BitSet> it = pZeros.iterator();
//             while (it.hasNext()) {
//                 BitSet e = it.next();
//                 if (contains(e, zeros))
//                     return;
//                 if (contains(zeros, e))
//                     it.remove();
//             }
//             pZeros.add(zeros);
//             if (nZeros > maxPossibleZeros) {
//                 maxPossibleZeros = nZeros;
//                 iMaxPossibleZeros = pZeros.Size() - 1;
//             }
//         }
//
//         BitSet maxZeros() {
//             return pZeros.isEmpty() ? new BitSet(nVariables) : pZeros.get(iMaxPossibleZeros);
//         }
//
//         @Override
//         public String toString() {
//             return pZeros.toString();
//         }
//     }
//
//     
//     static bool contains(BitSet major, BitSet minor) {
//         if (major.equals(minor))
//             return true;
//         BitSet tmp = (BitSet) major.Clone();
//         tmp.and(minor);
//         return tmp.equals(minor);
//     }
//
//     private static final int
//             SAME_BASE_VARIABLE_ATTEMPTS = 8,
//             ATTEMPTS_WITH_MAX_ZEROS = 2;
//
//     static final class EZGCDEvaluations {
//         final MultivariatePolynomialZp64 a, b;
//         final Random rnd;
//         // actual number of variables in use [0, ... nVariables] (may be less than a.nVariables)
//         final int nVariables;
//         // variables in which both cc and lc in a and b are non zeroes
//         final TIntArrayList perfectVariables = new TIntArrayList();
//
//         EZGCDEvaluations(MultivariatePolynomialZp64 a,
//                          MultivariatePolynomialZp64 b,
//                          int nVariables,
//                          Random rnd) {
//             this.a = a;
//             this.b = b;
//             this.rnd = rnd;
//             this.nVariables = nVariables;
//
//             // search for main variables
//             for (int var = 0; var < nVariables; ++var) {
//                 UnivariatePolynomialZp64 ub = b.evaluateAtZeroAllExcept(var);
//                 if (ub.Degree() == 0 || ub.cc() == 0 || ub.Degree() != b.Degree(var))
//                     continue;
//
//                 UnivariatePolynomialZp64 ua = a.evaluateAtZeroAllExcept(var);
//                 if (ua.Degree() == 0 || ua.cc() == 0 || ua.Degree() != a.Degree(var))
//                     continue;
//
//                 perfectVariables.add(var);
//             }
//         }
//
//         // modified a and b so that all except first variables
//         // may be evaluated to zero
//         MultivariatePolynomialZp64 aReduced, bReduced;
//
//         // pointers in perfectVariables list
//         int
//                 currentPerfectVariable = -1,
//                 nextPerfectVariable = 0;
//
//         int baseVariable = -1;
//         int[]
//                 zeroVariables, // variables that may be replaced with zeros ( 0 <> baseVariable are swaped )
//                 shiftingVariables; // variables that must be shifted before substituting zeros ( 0 <> baseVariable are swaped )
//         // shift values for shiftingVariables
//         long[] shifts;
//
//         // number of tries keeping as many zero variables as possible
//         int nAttemptsWithZeros = 0;
//         // variable that were used as main variables
//         final TIntHashSet usedBaseVariables = new TIntHashSet();
//         int nAttemptsWithSameBaseVariable = 0;
//
//         // returns whether the main variable was changed
//         bool nextEvaluation() {
//             // first try to evaluate all but one variables to zeroes
//
//             if (nextPerfectVariable < perfectVariables.Size()) {
//                 // we have the main variable
//                 currentPerfectVariable = perfectVariables.get(nextPerfectVariable);
//                 ++nextPerfectVariable;
//
//                 aReduced = AMultivariatePolynomial.swapVariables(a, 0, currentPerfectVariable);
//                 bReduced = AMultivariatePolynomial.swapVariables(b, 0, currentPerfectVariable);
//                 return true;
//             }
//
//             // no possible zero substitutions more
//             // => shift some variables and try to
//             // minimize intermediate expression swell
//             // due to "bad zero problem"
//
//             bool changedMainVariable = false;
//             currentPerfectVariable = -1;
//
//             int previousBaseVariable = baseVariable;
//             // try to substitute as many zeroes as possible
//             if (baseVariable == -1 || nAttemptsWithSameBaseVariable > SAME_BASE_VARIABLE_ATTEMPTS) {
//                 if (usedBaseVariables.Size() == nVariables)
//                     usedBaseVariables.clear();
//                 nAttemptsWithSameBaseVariable = nAttemptsWithZeros = 0;
//                 ZeroVariables maxZeros = null;
//                 for (int var = 0; var < nVariables; ++var) {
//                     if (usedBaseVariables.contains(var))
//                         continue;
//                     ZeroVariables pZeros;
//                     if (a.Degree(var) == 0 || b.Degree(var) == 0)
//                         continue;
//                     pZeros = possibleZeros(b.coefficientOf(var, 0), nVariables);
//                     if (maxZeros != null && pZeros.maxPossibleZeros < maxZeros.maxPossibleZeros)
//                         continue;
//
//                     pZeros = commonPossibleZeroes(pZeros, possibleZeros(b.coefficientOf(var, b.Degree(var)), nVariables));
//                     if (maxZeros != null && pZeros.maxPossibleZeros < maxZeros.maxPossibleZeros)
//                         continue;
//
//                     pZeros = commonPossibleZeroes(pZeros, possibleZeros(a.coefficientOf(var, 0), nVariables));
//                     if (maxZeros != null && pZeros.maxPossibleZeros < maxZeros.maxPossibleZeros)
//                         continue;
//
//                     pZeros = commonPossibleZeroes(pZeros, possibleZeros(a.coefficientOf(var, a.Degree(var)), nVariables));
//                     if (maxZeros != null && pZeros.maxPossibleZeros < maxZeros.maxPossibleZeros)
//                         continue;
//
//                     maxZeros = pZeros;
//                     baseVariable = var;
//                 }
//                 if (maxZeros == null) {
//                     // all free variables are bad
//                     usedBaseVariables.clear();
//                     return nextEvaluation();
//                 }
//                 usedBaseVariables.add(baseVariable);
//
//                 // <- we chose the main variable and those that can be safely substituted with zeros
//
//                 BitSet zeroSubstitutions = maxZeros.maxZeros();
//                 // swap 0 and baseVariable
//                 zeroSubstitutions.set(baseVariable, zeroSubstitutions.get(0));
//                 zeroSubstitutions.clear(0);
//
//                 zeroVariables = new int[zeroSubstitutions.cardinality()];
//                 shiftingVariables = new int[nVariables - zeroSubstitutions.cardinality() - 1];
//                 int iCounter = 0, jCounter = 0;
//                 for (int j = 0; j < nVariables; j++)
//                     if (zeroSubstitutions.get(j))
//                         zeroVariables[iCounter++] = j;
//                     else if (j != 0) // don't shift the main variable
//                         shiftingVariables[jCounter++] = j;
//
//                 if (shiftingVariables.Length == 0) { // <- perfect variable exists; just pickup some var for random substitutions
//                     shiftingVariables = new int[]{zeroVariables[zeroVariables.Length - 1]};
//                     zeroVariables = Arrays.copyOf(zeroVariables, zeroVariables.Length - 1);
//                 }
//                 shifts = new long[shiftingVariables.Length];
//
//                 changedMainVariable = previousBaseVariable != baseVariable;
//             }
//
//             aReduced = AMultivariatePolynomial.swapVariables(a, 0, baseVariable);
//             bReduced = AMultivariatePolynomial.swapVariables(b, 0, baseVariable);
//
//             if (nAttemptsWithZeros > ATTEMPTS_WITH_MAX_ZEROS && zeroVariables.Length != 0) {
//                 // fill with some value some zero variable
//                 shiftingVariables = Arrays.copyOf(shiftingVariables, shiftingVariables.Length + 1);
//                 shiftingVariables[shiftingVariables.Length - 1] = zeroVariables[zeroVariables.Length - 1];
//                 zeroVariables = Arrays.copyOf(zeroVariables, zeroVariables.Length - 1);
//                 shifts = new long[shiftingVariables.Length];
//                 nAttemptsWithZeros = 0;
//             }
//
//             ++nAttemptsWithSameBaseVariable;
//             ++nAttemptsWithZeros;
//             int
//                     ubDegree = bReduced.Degree(0),
//                     uaDegree = aReduced.Degree(0);
//
//             // choosing random shifts
//             for (int nFails = 0; ; ++nFails) {
//                 if (nFails > 8) {
//                     // base variable is unlucky -> start over
//                     nAttemptsWithSameBaseVariable = SAME_BASE_VARIABLE_ATTEMPTS + 1;
//                     return nextEvaluation();
//                 }
//                 for (int i = 0; i < shiftingVariables.Length; ++i)
//                     shifts[i] = a.ring.randomElement(rnd);
//
//                 UnivariatePolynomialZp64 ub = bReduced
//                         .evaluateAtZero(zeroVariables)
//                         .evaluate(shiftingVariables, shifts)
//                         .asUnivariate();
//
//                 if (ub.isZeroAt(0) || ub.isZeroAt(ubDegree))
//                     continue;
//
//                 UnivariatePolynomialZp64 ua = aReduced
//                         .evaluateAtZero(zeroVariables)
//                         .evaluate(shiftingVariables, shifts)
//                         .asUnivariate();
//
//                 if (ua.isZeroAt(0) || ua.isZeroAt(uaDegree))
//                     continue;
//
//                 break;
//             }
//             aReduced = aReduced.shift(shiftingVariables, shifts);
//             bReduced = bReduced.shift(shiftingVariables, shifts);
//
//             return changedMainVariable;
//         }
//
//         MultivariatePolynomialZp64 convert(MultivariatePolynomialZp64 poly) {
//             if (currentPerfectVariable != -1)
//                 // just swap the vars
//                 return currentPerfectVariable == 0 ?
//                         poly.Clone() : MultivariatePolynomialZp64.swapVariables(poly, 0, currentPerfectVariable);
//             return AMultivariatePolynomial.swapVariables(poly, 0, baseVariable).shift(shiftingVariables, shifts);
//         }
//
//         MultivariatePolynomialZp64 reconstruct(MultivariatePolynomialZp64 poly) {
//             if (currentPerfectVariable != -1)
//                 // just swap the vars
//                 return currentPerfectVariable == 0 ?
//                         poly : MultivariatePolynomialZp64.swapVariables(poly, 0, currentPerfectVariable);
//
//             poly = poly.shift(shiftingVariables, ArraysUtil.negate(shifts.Clone()));
//             poly = MultivariatePolynomialZp64.swapVariables(poly, 0, baseVariable);
//             return poly;
//         }
//     }
//
//     /* =============================================== EEZ-GCD algorithm ============================================ */

    public static MultivariatePolynomial<E> EEZGCD<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        return EEZGCD(a, b, false);
    }


    static MultivariatePolynomial<E> EEZGCD<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b,
        bool switchToSparse)
    {
        a.AssertSameCoefficientRingWith(b);
        if (CanConvertToZp64(a))
            return ConvertFromZp64<E>(EEZGCD(AsOverZp64(a), AsOverZp64(b)));

        // prepare input and test for early termination
        GCDInput<E> gcdInput = preparedGCDInput(a, b, MultivariateGCD.EEZGCD);
        if (gcdInput.earlyGCD != null)
            return gcdInput.earlyGCD;

        a = gcdInput.aReduced;
        b = gcdInput.bReduced;

        // remove Content in each variable
        var Content = a.CreateOne();
        for (int i = 0; i < a.nVariables; ++i)
        {
            if (a.Degree(i) == 0 || b.Degree(i) == 0)
                continue;
            var tmpContent = ContentGCD(a, b, i, MultivariateGCD.EEZGCD);
            a = MultivariateDivision.DivideExact(a, tmpContent);
            b = MultivariateDivision.DivideExact(b, tmpContent);
            Content = Content.Multiply(tmpContent);
        }

        // one more reduction; removing of Content may shuffle required variables order
        GCDInput<E> gcdInput2 = preparedGCDInput(a, b, MultivariateGCD.EEZGCD);
        a = gcdInput2.aReduced;
        b = gcdInput2.bReduced;

        MultivariatePolynomial<E> result;
        if (gcdInput2.earlyGCD != null)
            result = gcdInput2.earlyGCD;
        else if (switchToSparse && !isDenseGCDProblem(a, b))
            result = gcdInput2.restoreGCD(PolynomialGCDinGF(a, b));
        else
            result = gcdInput2.restoreGCD(EEZGCD0(a, b));

        result = result.Multiply(Content);
        return gcdInput.restoreGCD(result);
    }

     // private static MultivariatePolynomial<E> KaltofenMonaganEEZModularGCDInGF<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b) {
     //     if (a is MultivariatePolynomialZp64)
     //         return (Poly) KaltofenMonaganEEZModularGCDInGF((MultivariatePolynomialZp64) a, (MultivariatePolynomialZp64) b);
     //     else
     //         return (Poly) KaltofenMonaganEEZModularGCDInGF((MultivariatePolynomial) a, (MultivariatePolynomial) b);
     // }

     
    private static MultivariatePolynomial<E> EEZGCD0<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b)
    {
        // Degree of univariate gcd
        int ugcdDegree = int.MaxValue;
        // Degrees of a and b as Z[y1, ... ,yN][x]
        int
            uaDegree = a.Degree(0),
            ubDegree = b.Degree(0);

        MultivariateFactorization.IEvaluationLoop<E> evaluations = MultivariateFactorization.getEvaluationsGF(a);
        ImmutableList<DegreeVector> aSkeleton = a.GetSkeleton(0), bSkeleton = b.GetSkeleton(0);

        while (true)
        {
            choose_evaluation: ;
            HenselLifting.IEvaluation<E> evaluation = evaluations.next();
            if (evaluation == null)
                // switch to KM algorithm
                return KaltofenMonaganEEZModularGCDInGF(a, b);
            MultivariatePolynomial<E>
                mua = evaluation.evaluateFrom(a, 1),
                mub = evaluation.evaluateFrom(b, 1);

            if (!aSkeleton.Equals(mua.GetSkeleton()) || !bSkeleton.Equals(mub.GetSkeleton()))
                continue;

            UnivariatePolynomial<E>
                ua = mua.AsUnivariate(),
                ub = mub.AsUnivariate();

            Debug.Assert(ua.Degree() == uaDegree);
            Debug.Assert(ub.Degree() == ubDegree);

            // gcd of a mod I and b mod I (univariate)
            UnivariatePolynomial<E> ugcd = UnivariateGCD.PolynomialGCD(ua, ub);

            if (ugcd.Degree() == 0)
                // coprime polynomials
                return a.CreateOne();

            if (ugcd.Degree() > ugcdDegree)
                // unlucky evaluation
                goto choose_evaluation;

            ugcdDegree = ugcd.Degree();

            if (ugcdDegree == uaDegree)
            {
                // a is a divisor of b
                if (MultivariateDivision.DividesQ(b, a))
                    return a;
                //continue choose_evaluation;
            }

            if (ugcdDegree == ubDegree)
            {
                // b is a divisor of a
                if (MultivariateDivision.DividesQ(a, b))
                    return b;
                //continue choose_evaluation;
            }

            // base polynomial to lift (either a or b)
            MultivariatePolynomial<E>? @base = null;
            // a cofactor with gcd to lift, i.e. base = ugcd * uCoFactor mod I
            UnivariatePolynomial<E> uCoFactor = null;
            MultivariatePolynomial<E>? coFactorLC = null;

            // deg(b) < deg(a), so it is better to try to lift b
            UnivariatePolynomial<E> ubCoFactor = UnivariateDivision.Quotient(ub, ugcd, true);
            if (UnivariateGCD.PolynomialGCD(ugcd, ubCoFactor).IsConstant())
            {
                // b splits into coprime factors
                @base = b;
                uCoFactor = ubCoFactor;
                coFactorLC = b.Lc(0);
            }

            if (@base == null)
            {
                // b does not split into coprime factors => try a
                UnivariatePolynomial<E> uaCoFactor = UnivariateDivision.Quotient(ua, ugcd, true);
                if (UnivariateGCD.PolynomialGCD(ugcd, uaCoFactor).IsConstant())
                {
                    @base = a;
                    uCoFactor = uaCoFactor;
                    coFactorLC = a.Lc(0);
                }
            }

            if (@base == null)
            {
                // neither a nor b does not split into coprime factors => square free decomposition required
                MultivariatePolynomial<E>
                    bRepeatedFactor = EEZGCD(b, b.Derivative(0, 1)),
                    squareFreeGCD = EEZGCD(MultivariateDivision.DivideExact(b, bRepeatedFactor), a),
                    aRepeatedFactor = MultivariateDivision.DivideExact(a, squareFreeGCD),
                    gcd1 = squareFreeGCD.Clone();

                UnivariatePolynomial<E>
                    uSquareFreeGCD = evaluation.evaluateFrom(squareFreeGCD, 1).AsUnivariate(),
                    uaRepeatedFactor = evaluation.evaluateFrom(aRepeatedFactor, 1).AsUnivariate(),
                    ubRepeatedFactor = evaluation.evaluateFrom(bRepeatedFactor, 1).AsUnivariate();
                while (true)
                {
                    ugcd = UnivariateGCD.PolynomialGCD(
                        uSquareFreeGCD, uaRepeatedFactor, ubRepeatedFactor);

                    if (ugcd.Degree() == 0)
                        return gcd1;

                    if (!uSquareFreeGCD.Clone().Monic().Equals(ugcd.Clone().Monic()))
                    {
                        MultivariatePolynomial<E>
                            mgcd = MultivariatePolynomial<E>.AsMultivariate(ugcd, a.nVariables, 0, a.ordering),
                            gcdCoFactor = MultivariatePolynomial<E>.AsMultivariate(
                                UnivariateDivision.DivideExact(uSquareFreeGCD, ugcd, false), a.nVariables, 0,
                                a.ordering);

                        liftPairAutomaticLC(squareFreeGCD, mgcd, gcdCoFactor, evaluation);

                        squareFreeGCD = mgcd.Clone();
                        uSquareFreeGCD = evaluation.evaluateFrom(squareFreeGCD, 1).AsUnivariate();
                    }

                    gcd1 = gcd1.Multiply(squareFreeGCD);
                    uaRepeatedFactor = UnivariateDivision.DivideExact(uaRepeatedFactor, ugcd, false);
                    ubRepeatedFactor = UnivariateDivision.DivideExact(ubRepeatedFactor, ugcd, false);
                }
            }

            MultivariatePolynomial<E> gcd = MultivariatePolynomial<E>.AsMultivariate(ugcd, a.nVariables, 0, a.ordering);
            MultivariatePolynomial<E> coFactor =
                MultivariatePolynomial<E>.AsMultivariate(uCoFactor, a.nVariables, 0, a.ordering);

            // impose the leading coefficient
            MultivariatePolynomial<E> lcCorrection = EEZGCD(a.Lc(0), b.Lc(0));
            //assert ZippelGCD(a.lc(0), b.lc(0)).Monic(lcCorrection.Lc()).equals(lcCorrection) : "\n" + a.lc(0) + "  \n " + b.lc(0);

            if (lcCorrection.IsOne())
            {
                liftPair(@base, gcd, coFactor, null, @base.Lc(0), evaluation);
            }
            else
            {
                MultivariatePolynomial<E> lcCorrectionMod = evaluation.evaluateFrom(lcCorrection, 1);
                Debug.Assert(lcCorrectionMod.IsConstant());

                coFactor = coFactor.MultiplyByLC(gcd);
                gcd = gcd.MonicWithLC(lcCorrectionMod);

                liftPair(@base.Clone().Multiply(lcCorrection), gcd, coFactor, lcCorrection, coFactorLC, evaluation);
                Debug.Assert(gcd.Lc(0).Equals(lcCorrection));
                gcd = HenselLifting.primitivePart(gcd);
            }

            if (isGCDTriplet(b, a, gcd))
                return gcd;
        }
    }
}