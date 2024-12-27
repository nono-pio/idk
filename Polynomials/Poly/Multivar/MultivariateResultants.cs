using System.Collections.Immutable;
using System.Numerics;
using Polynomials.Linear;
using Polynomials.Poly.Univar;
using Polynomials.Primes;
using Polynomials.Utils;
using UnivariatePolynomialZp64 = Polynomials.Poly.Univar.UnivariatePolynomial<long>;
using MultivariatePolynomialZp64 = Polynomials.Poly.Multivar.MultivariatePolynomial<long>;

namespace Polynomials.Poly.Multivar;

public static class MultivariateResultants
{
    public static MultivariatePolynomial<E> Discriminant<E>(MultivariatePolynomial<E> a, int variable)
    {
        var disc = MultivariateDivision.DivideExact(Resultant(a, a.Derivative(variable), variable),
            a.Lc(variable));
        return ((a.Degree(variable) * (a.Degree(variable) - 1) / 2) % 2 == 1) ? (disc.Negate()) : disc;
    }


    public static MultivariatePolynomial<E> Resultant<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b,
        int variable)
    {
        a.AssertSameCoefficientRingWith(b);
        if (a.IsOverFiniteField())
            return ResultantInGF(a, b, variable);
        if (a.IsOverZ())
            return ResultantInZ(a.AsZ(), b.AsZ(), variable).AsT<E>();
        if (Util.IsOverRationals(a))
            return (MultivariatePolynomial<E>)GenericHandler.InvokeForGeneric<E>(typeof(Rational<>),
                nameof(ResultantInQ),
                typeof(MultivariateResultants), a, b, variable)!; // ResultantInQ(a, b, variable);
        if (Util.IsOverSimpleNumberField(a))
            return ModularResultantInNumberField(
                a.AsT<UnivariatePolynomial<Rational<BigInteger>>>(),
                b.AsT<UnivariatePolynomial<Rational<BigInteger>>>(),
                variable).AsT<E>();
        if (Util.IsOverRingOfIntegersOfSimpleNumberField(a))
            return ModularResultantInRingOfIntegersOfNumberField(a.AsT<UnivariatePolynomial<BigInteger>>(),
                b.AsT<UnivariatePolynomial<BigInteger>>(), variable).AsT<E>();
        if (Util.IsOverMultipleFieldExtension(a))
            return (MultivariatePolynomial<E>)GenericHandler.InvokeForGeneric<E>(typeof(MultivariatePolynomial<>),
                nameof(ResultantInMultipleFieldExtension),
                typeof(MultivariateResultants), a, b, variable)!; //ResultantInMultipleFieldExtension(a, b, variable);
        if (a.IsOverField())
            return ZippelResultant(a, b,
                variable);
        return TryNested(a, b, variable);
    }

    private static MultivariatePolynomial<E> TryNested<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b,
        int variable)
    {
        if (MultivariateGCD.isOverUnivariate(a))
            return (MultivariatePolynomial<E>)GenericHandler.InvokeForGeneric<E>(typeof(UnivariatePolynomial<>),
                nameof(ResultantOverUnivariate), typeof(MultivariateResultants), a, b,
                variable)!; //ResultantOverUnivariate(a, b, variable);
        else if (MultivariateGCD.isOverMultivariate(a))
            return (MultivariatePolynomial<E>)GenericHandler.InvokeForGeneric<E>(typeof(MultivariatePolynomial<>),
                nameof(ResultantOverMultivariate), typeof(MultivariateResultants), a, b,
                variable)!; //ResultantOverMultivariate(a, b, variable);
        return ClassicalResultant(a, b, variable);
    }

    private static MultivariatePolynomial<UnivariatePolynomial<E>> ResultantOverUnivariate<E>(
        MultivariatePolynomial<UnivariatePolynomial<E>> a, MultivariatePolynomial<UnivariatePolynomial<E>> b,
        int variable)
    {
        return Resultant(MultivariatePolynomial<E>.AsNormalMultivariate(a, 0),
            MultivariatePolynomial<E>.AsNormalMultivariate(b, 0), 1 + variable).AsOverUnivariateEliminate(0);
    }

    private static MultivariatePolynomial<MultivariatePolynomial<E>> ResultantOverMultivariate<E>(
        MultivariatePolynomial<MultivariatePolynomial<E>> a, MultivariatePolynomial<MultivariatePolynomial<E>> b,
        int variable)
    {
        int[] cfVars = Utils.Utils.Sequence(a.Lc().nVariables);
        int[] mainVars = Utils.Utils.Sequence(a.Lc().nVariables, a.Lc().nVariables + a.nVariables);
        return Resultant(MultivariatePolynomial<E>.AsNormalMultivariate(a, cfVars, mainVars),
                MultivariatePolynomial<E>.AsNormalMultivariate(b, cfVars, mainVars), cfVars.Length + variable)
            .AsOverMultivariateEliminate(cfVars);
    }


    static MultivariatePolynomial<Rational<E>> ResultantInQ<E>(MultivariatePolynomial<Rational<E>> a,
        MultivariatePolynomial<Rational<E>> b, int variable)
    {
        var aRat = Util.ToCommonDenominator(a);
        var bRat = Util.ToCommonDenominator(b);
        Ring<E> ring = aRat.Item1.ring;
        E correction = ring.Multiply(ring.Pow(aRat.Item2, b.Degree(variable)),
            ring.Pow(bRat.Item2, a.Degree(variable)));
        return Util.AsOverRationals(a.ring, Resultant(aRat.Item1, bRat.Item1, variable))
            .DivideExact(new Rational<E>(ring, correction));
    }

    private static MultivariatePolynomial<MultivariatePolynomial<E>> ResultantInMultipleFieldExtension<E>(
        MultivariatePolynomial<MultivariatePolynomial<E>> a, MultivariatePolynomial<MultivariatePolynomial<E>> b,
        int variable)
    {
        MultipleFieldExtension<E> ring = (MultipleFieldExtension<E>)a.ring;
        SimpleFieldExtension<E> simpleExtension = ring.GetSimpleExtension();
        return Resultant(a.MapCoefficients(simpleExtension, ring.Inverse),
            b.MapCoefficients(simpleExtension, ring.Inverse), variable).MapCoefficients(ring, ring.Image);
    }


    public static MultivariatePolynomial<E> ResultantInGF<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b,
        int variable)
    {
        return ZippelResultant(a, b, variable);
    }


    public static MultivariatePolynomial<BigInteger> ResultantInZ(MultivariatePolynomial<BigInteger> a,
        MultivariatePolynomial<BigInteger> b, int variable)
    {
        return ModularResultantInZ(a, b, variable);
    }


    public static MultivariatePolynomial<E> ClassicalResultant<E>(MultivariatePolynomial<E> a,
        MultivariatePolynomial<E> b, int variable)
    {
        return UnivariateResultants.Resultant(a.AsUnivariateEliminate(variable), b.AsUnivariateEliminate(variable))
            .InsertVariable(variable);
    }

    /* ============================================== Auxiliary methods ============================================= */


    sealed class ResultantInput<E>
    {
        public readonly MultivariatePolynomial<E>? aReduced0;
        public readonly MultivariatePolynomial<E>? bReduced0;


        public readonly UnivariatePolynomial<MultivariatePolynomial<E>> aReduced;
        public readonly UnivariatePolynomial<MultivariatePolynomial<E>> bReduced;


        public readonly MultivariatePolynomial<E>? earlyResultant;


        public readonly int[] degreeBounds;
        readonly int[] mapping;


        public readonly int lastPresentVariable;


        readonly int evaluationStackLimit;


        readonly MultivariatePolynomial<E> monomialResultant;


        public readonly int finiteExtensionDegree;


        public ResultantInput(MultivariatePolynomial<E> earlyResultant)
        {
            this.earlyResultant = earlyResultant;
            aReduced0 = bReduced0 = null;
            aReduced = bReduced = null;
            degreeBounds = mapping = null;
            lastPresentVariable = evaluationStackLimit = -1;
            monomialResultant = null;
            finiteExtensionDegree = -1;
        }


        public ResultantInput(MultivariatePolynomial<E> aReduced0, MultivariatePolynomial<E> bReduced0,
            MultivariatePolynomial<E> monomialResultant, int evaluationStackLimit,
            int[] degreeBounds, int[] mapping, int lastPresentVariable, int finiteExtensionDegree)
        {
            //assert monomialGCD == null || aReduced.ring.isOne(monomialGCD.coefficient);
            this.aReduced0 = aReduced0;
            this.bReduced0 = bReduced0;
            this.aReduced = aReduced0.AsUnivariateEliminate(0);
            this.bReduced = bReduced0.AsUnivariateEliminate(0);
            this.monomialResultant = monomialResultant;
            this.earlyResultant = null;
            this.evaluationStackLimit = evaluationStackLimit;
            this.degreeBounds = degreeBounds;
            this.mapping = MultivariateGCD.inversePermutation(mapping);
            this.lastPresentVariable = lastPresentVariable;
            this.finiteExtensionDegree = finiteExtensionDegree;
        }


        //assert monomialGCD == null || aReduced.ring.isOne(monomialGCD.coefficient);


        public MultivariatePolynomial<E> RestoreResultant(MultivariatePolynomial<E> result)
        {
            return MultivariatePolynomial<E>.RenameVariables(result.InsertVariable(0), mapping)
                .Multiply(monomialResultant);
        }
    }

    static ResultantInput<E> PreparedResultantInput<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b,
        int variable)
    {
        var trivialResultant = TrivialResultant(a, b, variable);
        if (trivialResultant != null)
            return new ResultantInput<E>(trivialResultant);
        BigInteger? ringSize = a.CoefficientRingCardinality();

        // ring cardinality, i.e. number of possible random choices
        int evaluationStackLimit = ringSize == null ? -1 : (ringSize.Value.IsInt() ? (int)ringSize.Value : -1);

        // find monomial GCD
        // and remove monomial content from a and b
        a = a.Clone();
        b = b.Clone(); // prevent rewriting original data
        var aContent = a.MonomialContent();
        var bContent = b.MonomialContent();
        a = a.DivideOrNull(aContent);
        b = b.DivideOrNull(bContent);
        if (aContent.exponents[variable] != 0 && bContent.exponents[variable] != 0)
            return new ResultantInput<E>(a.CreateZero());
        var monomialResultant = ResultantWithMonomial(aContent, b.Create(bContent), variable)
            .Multiply(ResultantWithMonomial(aContent, b, variable))
            .Multiply(ResultantWithMonomial(a, bContent, variable));
        trivialResultant = TrivialResultant(a, b, variable);
        if (trivialResultant != null)
            return new ResultantInput<E>(trivialResultant.Multiply(monomialResultant));
        int nVariables = a.nVariables;
        int[] aDegrees = a.DegreesRef(),
            bDegrees = b.DegreesRef(),
            degreeBounds = new int[nVariables]; // degree bounds for resultant
        degreeBounds[variable] = int.MaxValue;

        // populate initial resultant degree bounds based on Sylvester matrix
        int nUnused = 0;
        for (int i = 0; i < nVariables; i++)
        {
            if (i == variable)

                // skip main variable
                continue;

            // avoid potential int overflow
            degreeBounds[i] = checked(aDegrees[i] * bDegrees[variable] + bDegrees[i] * aDegrees[variable]);
            if (degreeBounds[i] == 0)
                ++nUnused;
        }

        if (nUnused == (nVariables - 1))
        {
            // all variables are unused => univariate resultant
            var t = TrivialResultant(a, b, variable);
            return new ResultantInput<E>(t.Multiply(monomialResultant));
        }


        // adjust degree bounds with randomized substitutions and univariate images
        AdjustDegreeBounds(a, b, variable, degreeBounds);

        // now swap variables so that the first variable will have the maximal degree (univariate resultant is fast),
        // and all non-used variables are at the end of poly's
        int[] variables = Utils.Utils.Sequence(nVariables);

        //sort in descending order
        Array.Sort(Utils.Utils.Negate(degreeBounds), variables);
        Utils.Utils.Negate(degreeBounds); //recover degreeBounds
        degreeBounds = degreeBounds[1..];
        int lastResVariable = 0; //recalculate lastPresentVariable
        for (; lastResVariable < degreeBounds.Length; ++lastResVariable)
            if (degreeBounds[lastResVariable] == 0)
                break;
        --lastResVariable;

        // resultant variable is always the first
        a = MultivariatePolynomial<E>.RenameVariables(a, variables);
        b = MultivariatePolynomial<E>.RenameVariables(b, variables);

        // check whether coefficient ring cardinality is large enough
        int finiteExtensionDegree = 1;
        int cardinalityBound = checked(9 * degreeBounds.Max());
        if (ringSize != null && ringSize.Value.IsInt() && (int)ringSize.Value < cardinalityBound)
        {
            long ds = (long)ringSize.Value;
            finiteExtensionDegree = 2;
            long tmp = ds;
            for (; tmp < cardinalityBound; ++finiteExtensionDegree)
                tmp = tmp * ds;
        }

        return new ResultantInput<E>(a, b, monomialResultant, evaluationStackLimit, degreeBounds, variables,
            lastResVariable, finiteExtensionDegree);
    }

    private static void AdjustDegreeBounds<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b, int variable,
        int[] degreeBounds)
    {
        if (!a.IsOverFiniteField())
            return;
        if (a.CoefficientRingCharacteristic().GetBitLength() < 16)

            // don't do estimates for domains of small characteristic
            return;

        int nVariables = a.nVariables;
        E[] subs = new E[nVariables];
        Random rnd = PrivateRandom.GetRandom();
        for (int i = 0; i < nVariables; i++)
        {
            if (i == variable)
                continue;
            subs[i] = a.ring.RandomNonZeroElement(rnd);
        }

        for (int i = 0; i < nVariables; i++)
        {
            if (i == variable)
                continue;

            // all vars but i-th and variable
            int[] vars = Utils.Utils.Remove(Utils.Utils.Sequence(0, a.nVariables), new int[] { i, variable });
            E[] vals = Utils.Utils.Remove(subs, new int[] { i, variable });
            MultivariatePolynomial<E> mua = a.Evaluate(vars, vals), mub = b.Evaluate(vars, vals);
            if (!mua.GetSkeleton().Equals(a.GetSkeleton(variable, i)))
                continue;
            if (!mub.GetSkeleton().Equals(b.GetSkeleton(variable, i)))
                continue;
            UnivariatePolynomial<UnivariatePolynomial<E>> ua = mua.AsOverUnivariateEliminate(i).AsUnivariate(),
                ub = mub.AsOverUnivariateEliminate(i).AsUnivariate();
            degreeBounds[i] = Math.Min(degreeBounds[i], UnivariateResultants.Resultant(ua, ub).Degree());
        }
    }

    private static MultivariatePolynomial<E>? TrivialResultant<E>(MultivariatePolynomial<E> a,
        MultivariatePolynomial<E> b, int variable)
    {
        if (Equals(a, b) || a.IsZero() || b.IsZero())
            return a.CreateZero();
        if (a.Degree(variable) == 0)
            return PolynomialMethods.PolyPow(a, b.Degree(variable));
        if (b.Degree(variable) == 0)
            return PolynomialMethods.PolyPow(b, a.Degree(variable));
        if (a.Size() == 1)
            return ResultantWithMonomial(a.Lt(), b, variable);
        if (b.Size() == 1)
            return ResultantWithMonomial(a, b.Lt(), variable);
        if (a.UnivariateVariable() == variable && b.UnivariateVariable() == variable)
            return MultivariatePolynomial<E>.AsMultivariate(
                UnivariateResultants.ResultantAsPoly(a.AsUnivariate(), b.AsUnivariate()), a.nVariables, variable,
                a.ordering);
        return null;
    }


    private static MultivariatePolynomial<E> ResultantWithMonomial<E>(Monomial<E> monomial,
        MultivariatePolynomial<E> poly, int variable)
    {
        int varExponent = monomial.exponents[variable];
        var cFactor = PolynomialMethods.PolyPow(poly.Create(monomial.Set(variable, 0)), poly.Degree(variable));
        var xFactor = PolynomialMethods.PolyPow(poly.EvaluateAtZero(variable), varExponent);
        return cFactor.Multiply(xFactor);
    }


    private static MultivariatePolynomial<E> ResultantWithMonomial<E>(MultivariatePolynomial<E> poly,
        Monomial<E> monomial, int variable)
    {
        var r = ResultantWithMonomial(monomial, poly, variable);
        if (poly.Degree(variable) % 2 == 1 && monomial.exponents[variable] % 2 == 1)
            r.Negate();
        return r;
    }

    static MultivariatePolynomial<E> BivariateResultant<E>(
        UnivariatePolynomial<MultivariatePolynomial<E>> a, UnivariatePolynomial<MultivariatePolynomial<E>> b)
    {
        MultivariatePolynomial<E> factory = a.Lc();
        Ring<E> ring = factory.ring;
        UnivariateRing<E> uRing = Rings.UnivariateRing(ring);
        UnivariatePolynomial<UnivariatePolynomial<E>> aUni =
                a.MapCoefficients(uRing, c => c.AsUnivariate()),
            bUni = b.MapCoefficients(uRing, c => c.AsUnivariate());
        return UnivariateResultants.Resultant(aUni, bUni).AsMultivariate(factory.ordering)
            .SetNVariables(factory.nVariables);
    }

    /* =========================================== In small characteristic ========================================== */


    public static MultivariatePolynomial<E> ResultantInSmallCharacteristic<E>(MultivariatePolynomial<E> a,
        MultivariatePolynomial<E> b, int variable)
    {
        // use "naive" algorithm for now.
        return ClassicalResultant(a, b, variable);
    }


    static MultivariatePolynomial<E> ResultantInSmallCharacteristic<E>(
        UnivariatePolynomial<MultivariatePolynomial<E>> a, UnivariatePolynomial<MultivariatePolynomial<E>> b)
    {
        // use "naive" algorithm for now.
        return UnivariateResultants.Resultant(a, b);
    }

    /* ============================================== Modular resultant ============================================= */


    public static MultivariatePolynomial<BigInteger> ModularResultantInZ(MultivariatePolynomial<BigInteger> a,
        MultivariatePolynomial<BigInteger> b, int variable)
    {
        ResultantInput<BigInteger> resInput =
            PreparedResultantInput(a, b, variable);
        if (resInput.earlyResultant != null)
            return resInput.earlyResultant;
        return resInput.RestoreResultant(ModularResultantInZ0(resInput.aReduced0, resInput.bReduced0));
    }


    private static readonly int N_UNCHANGED_INTERPOLATIONS = 5;

    private static BigInteger CentralMultinomialCoefficient(int n, int d)
    {
        int q = n / d, r = n % d;
        return Rings.Z.Factorial(n) /
               (BigInteger.Pow(Rings.Z.Factorial(q), d - r) / BigInteger.Pow(Rings.Z.Factorial(q + 1), r));
    }


    static MultivariatePolynomial<BigInteger> ModularResultantInZ0(MultivariatePolynomial<BigInteger> a,
        MultivariatePolynomial<BigInteger> b)
    {
        // coefficient bound (not quite optimistic:)
        BigInteger aMax = a.MaxAbsCoefficient(), bMax = b.MaxAbsCoefficient();
        BigInteger bound2 = (BigInteger.Pow(aMax, b.Degree())
                             * CentralMultinomialCoefficient(a.Size(), b.Degree()) * BigInteger.Pow(bMax, a.Degree())
                             * CentralMultinomialCoefficient(b.Size(), a.Degree()) *
                             Rings.Z.Factorial(a.Degree() + b.Degree()))
                            << 1; // symmetric Zp form

        // choose better prime for start
        long startingPrime;
        if (Math.Max(aMax.GetBitLength(), bMax.GetBitLength()) < 128)
            startingPrime = 1 << 30;
        else
            startingPrime = 1L << 60;
        PrimesIterator primesLoop = new PrimesIterator(startingPrime - (1 << 12));
        Random random = PrivateRandom.GetRandom();
        main_loop:
        while (true)
        {
            // prepare the skeleton
            long basePrime = primesLoop.Take();
            IntegersZp64 ring = Rings.Zp64(basePrime);

            // reduce Z -> Zp
            MultivariatePolynomialZp64 aMod = a.MapCoefficientsZp64(ring, (c) => (long)(c % basePrime)),
                bMod = b.MapCoefficientsZp64(ring, (c) => (long)(c % basePrime));
            if (!aMod.SameSkeletonQ(a) || !bMod.SameSkeletonQ(b))
                continue;

            // the base image
            // accumulator to update coefficients via Chineese remainding
            MultivariatePolynomialZp64 @base = ResultantInGF(aMod, bMod, 0).DropVariable(0);
            MultivariatePolynomialZp64 skeleton = @base;
            MultivariatePolynomial<BigInteger> bBase = @base.ToBigPoly();

            // cache the previous base
            MultivariatePolynomial<BigInteger> previousBase = null;

            // number of times interpolation did not change the result
            int nUnchangedInterpolations = 0;
            BigInteger bBasePrime = Rings.Z.ValueOf(basePrime);

            // over all primes
            while (true)
            {
                long prime = primesLoop.Take();
                BigInteger bPrime = Rings.Z.ValueOf(prime);
                ring = Rings.Zp64(prime);

                // reduce Z -> Zp
                aMod = a.MapCoefficientsZp64(ring, (c) => (long)(c % prime));
                bMod = b.MapCoefficientsZp64(ring, (c) => (long)(c % prime));
                if (!aMod.SameSkeletonQ(a) || !bMod.SameSkeletonQ(b))
                    continue;

                // calculate new resultant using previously calculated skeleton via sparse interpolation
                MultivariatePolynomialZp64 modularResultant = InterpolateResultant(aMod, bMod, skeleton, random);
                if (modularResultant == null)
                {
                    // interpolation failed => assumed form is wrong => start over
                    continue;
                }


                // something wrong, start over
                if (!modularResultant.SameSkeletonQ(bBase))
                {
                    @base = modularResultant;
                    skeleton = @base;
                    bBase = modularResultant.ToBigPoly();
                    bBasePrime = bPrime;
                    previousBase = null;
                    nUnchangedInterpolations = 0;
                    continue;
                }


                //lifting
                BigInteger newBasePrime = bBasePrime * bPrime;
                PairedIterator<BigInteger, long> iterator =
                    new PairedIterator<BigInteger, long>(bBase, modularResultant);
                ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic =
                    ChineseRemainders.CreateMagic(Rings.Z, bBasePrime, bPrime);
                while (iterator.MoveNext())
                {
                    var baseTerm = iterator.aTerm;
                    var imageTerm = iterator.bTerm;
                    if (baseTerm.coefficient.IsZero)

                        // term is absent in the base
                        continue;
                    if (imageTerm.coefficient == 0)
                    {
                        // term is absent in the modularResultant => remove it from the base
                        // bBase.subtract(baseTerm);
                        bBase.Subtract(baseTerm);
                        continue;
                    }

                    long oth = imageTerm.coefficient;

                    // update base term
                    BigInteger newCoeff =
                        ChineseRemainders.ChineseRemainder(Rings.Z, magic, baseTerm.coefficient, new BigInteger(oth));
                    bBase.Put(baseTerm.SetCoefficient(newCoeff));
                }

                bBase = bBase.SetRingUnsafe(new IntegersZp(newBasePrime));
                bBasePrime = newBasePrime;

                // two trials didn't change the result, probably we are done
                MultivariatePolynomial<BigInteger> candidate =
                    MultivariatePolynomial<BigInteger>.AsPolyZSymmetric(bBase);
                if (previousBase != null && candidate.Equals(previousBase))
                    ++nUnchangedInterpolations;
                else
                    nUnchangedInterpolations = 0;
                if (nUnchangedInterpolations >= N_UNCHANGED_INTERPOLATIONS || bBasePrime.CompareTo(bound2) > 0)
                    return candidate;
                previousBase = candidate;
            }
        }
    }

    static MultivariatePolynomialZp64 InterpolateResultant(MultivariatePolynomialZp64 a, MultivariatePolynomialZp64 b,
        MultivariatePolynomialZp64 skeleton, Random rnd)
    {
        a.AssertSameCoefficientRingWith(b);
        skeleton = skeleton.SetRingUnsafe(a.ring);
        if (a.nVariables == 2)
        {
            return BivariateResultant(a.AsUnivariateEliminate(0), b.AsUnivariateEliminate(0));
        }

        var interpolation = CreateInterpolation(-1, a.AsUnivariateEliminate(0),
            b.AsUnivariateEliminate(0), skeleton, 1, rnd);
        if (interpolation == null)
            return null;
        MultivariatePolynomialZp64 res = interpolation.Evaluate();
        if (res == null)
            return null;
        return res;
    }

    /* ================================== Modular resultant in simple number fields ================================ */


    private static MultivariatePolynomial<UnivariatePolynomial<E>> TrivialResultantInExtension<E>(
        MultivariatePolynomial<UnivariatePolynomial<E>> a, MultivariatePolynomial<UnivariatePolynomial<E>> b,
        int variable)
    {
        AlgebraicNumberField<E> ring = (AlgebraicNumberField<E>)a.ring;
        if (!a.Stream().All(ring.IsInTheBaseField) || !b.Stream().All(ring.IsInTheBaseField))
            return null;
        Ring<E> cfRing = ring.GetMinimalPolynomial().ring;
        MultivariatePolynomial<E> ar = a.MapCoefficients(cfRing, u => u.Cc()),
            br = b.MapCoefficients(cfRing, u => u.Cc());
        return Resultant(ar, br, variable).MapCoefficients(ring, (cf) => UnivariatePolynomial<E>.Constant(cfRing, cf));
    }


    public static MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> ModularResultantInNumberField(
        MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a,
        MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> b, int variable)
    {
        MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> res =
            TrivialResultantInExtension(a, b, variable);
        if (res != null)
            return res;
        AlgebraicNumberField<Rational<BigInteger>> numberField =
            (AlgebraicNumberField<Rational<BigInteger>>)a.ring;
        UnivariatePolynomial<Rational<BigInteger>> minimalPoly = numberField.GetMinimalPolynomial();
        a = a.Clone();
        b = b.Clone();

        // reduce problem to the case with integer monic minimal polynomial
        if (minimalPoly.Stream().All(r => r.IsIntegral()))
        {
            // minimal poly is already monic & integer
            UnivariatePolynomial<BigInteger> minimalPolyZ = minimalPoly.MapCoefficients(Rings.Z, r => r.Numerator());
            AlgebraicNumberField<BigInteger>
                numberFieldZ = new AlgebraicNumberField<BigInteger>(minimalPolyZ);
            BigInteger aDen = RemoveDenominators(a),
                bDen = RemoveDenominators(b),
                den = BigInteger.Pow(aDen, b.Degree(variable)) * BigInteger.Pow(bDen, a.Degree(variable));
            return ModularResultantInRingOfIntegersOfNumberField(
                    a.MapCoefficients(numberFieldZ, (cf) => cf.MapCoefficients(Rings.Z, r => r.Numerator())),
                    b.MapCoefficients(numberFieldZ, (cf) => cf.MapCoefficients(Rings.Z, r => r.Numerator())), variable)
                .MapCoefficients(numberField, (cf) => cf.MapCoefficients(Rings.Q, (r) => Rings.Q.Mk(r, den)));
        }
        else
        {
            // replace s -> s / lc(minPoly)
            BigInteger minPolyLeadCoeff = Util.CommonDenominator(minimalPoly);
            Rational<BigInteger> scale = new Rational<BigInteger>(Rings.Z, Rings.Z.GetOne(), minPolyLeadCoeff),
                scaleReciprocal = scale.Reciprocal();
            AlgebraicNumberField<Rational<BigInteger>> scaledNumberField =
                new AlgebraicNumberField<Rational<BigInteger>>(minimalPoly.Scale(scale).Monic());
            return ModularResultantInNumberField(a.MapCoefficients(scaledNumberField, (cf) => cf.Scale(scale)),
                    b.MapCoefficients(scaledNumberField, (cf) => cf.Scale(scale)), variable)
                .MapCoefficients(numberField, (cf) => cf.Scale(scaleReciprocal));
        }
    }

    static BigInteger RemoveDenominators(MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a)
    {
        BigInteger denominator = Rings.Z.Lcm(a.Stream().Select(Util.CommonDenominator));
        a.Multiply(a.ring.ValueOfBigInteger(denominator));
        return denominator;
    }


    public static MultivariatePolynomial<UnivariatePolynomial<BigInteger>>
        ModularResultantInRingOfIntegersOfNumberField(MultivariatePolynomial<UnivariatePolynomial<BigInteger>> a,
            MultivariatePolynomial<UnivariatePolynomial<BigInteger>> b, int variable)
    {
        ResultantInput<UnivariatePolynomial<BigInteger>> resInput = PreparedResultantInput(a, b, variable);
        if (resInput.earlyResultant != null)
            return resInput.earlyResultant;
        return resInput.RestoreResultant(
            ModularResultantInRingOfIntegersOfNumberField0(resInput.aReduced0, resInput.bReduced0));
    }


    private static MultivariatePolynomial<UnivariatePolynomial<BigInteger>>
        ModularResultantInRingOfIntegersOfNumberField0(MultivariatePolynomial<UnivariatePolynomial<BigInteger>> a,
            MultivariatePolynomial<UnivariatePolynomial<BigInteger>> b)
    {
        MultivariatePolynomial<UnivariatePolynomial<BigInteger>> r = TrivialResultantInExtension(a, b, 0);
        if (r != null)
            return r;
        AlgebraicNumberField<BigInteger> numberField =
            (AlgebraicNumberField<BigInteger>)a.ring;
        UnivariatePolynomial<BigInteger> minimalPoly = numberField.GetMinimalPolynomial();
        BigInteger aMax = a.Stream().Select(u => u.MaxAbsCoefficient()).Max(Rings.Z), // ?? BigInteger.One
            bMax = b.Stream().Select(u => u.MaxAbsCoefficient()).Max(Rings.Z), // .OrElse(BigInteger.ONE)
            mMax = minimalPoly.MaxAbsCoefficient();

        //        // bound on the value of resultant coefficients
        //        assert a.degree() == a.degree(0) && b.degree() == b.degree(0);
        //        BigInteger bound2 = Z.getOne()
        //                .multiply(UnivariateResultants.polyPowNumFieldCfBound(aMax, mMax, minimalPoly.degree(), b.degree()))  // a coefficients
        //                .multiply(centralMultinomialCoefficient(a.size(), b.degree())) // a multiplication
        //                .multiply(UnivariateResultants.polyPowNumFieldCfBound(bMax, mMax, minimalPoly.degree(), a.degree()))  // b coefficients
        //                .multiply(centralMultinomialCoefficient(b.size(), a.degree())) // b multiplication
        //                .multiply(Z.factorial(a.degree() + b.degree()))          // overall determinant (Leibniz formula)
        //                .shiftLeft(1);                                                // symmetric Zp form
        // choose better prime for start
        long startingPrime;
        if (Math.Max(aMax.GetBitLength(), bMax.GetBitLength()) < 128)
            startingPrime = 1 << 30;
        else
            startingPrime = 1 << 60;
        UnivariateRing<BigInteger> auxRing = Rings.UnivariateRing(Rings.Z);
        PrimesIterator primesLoop = new PrimesIterator(startingPrime - (1 << 12));
        Random random = PrivateRandom.GetRandom();
        main_loop:
        while (true)
        {
            // prepare the skeleton
            long basePrime = primesLoop.Take();
            IntegersZp64 baseRing = Rings.Zp64(basePrime);
            UnivariatePolynomialZp64 minimalPolyMod = UnivariatePolynomialZp64.AsOverZp64(minimalPoly, baseRing);
            FiniteField<long> numberFieldMod = new FiniteField<long>(minimalPolyMod);

            // reduce Z -> Zp
            MultivariatePolynomial<UnivariatePolynomialZp64> aMod =
                    a.MapCoefficients(numberFieldMod, (c) => UnivariatePolynomialZp64.AsOverZp64(c, baseRing)),
                bMod = b.MapCoefficients(numberFieldMod, (c) => UnivariatePolynomialZp64.AsOverZp64(c, baseRing));
            if (!aMod.SameSkeletonQ(a) || !bMod.SameSkeletonQ(b))
                continue;

            // the base image
            // accumulator to update coefficients via Chineese remainding
            MultivariatePolynomial<UnivariatePolynomialZp64> @base;
            try
            {
                @base = ResultantInGF(aMod, bMod, 0).DropVariable(0);
            }
            catch (Exception t)
            {
                continue;
            } // bad base prime

            MultivariatePolynomial<UnivariatePolynomial<BigInteger>> bBase =
                @base.MapCoefficients(auxRing, (cf) => cf.AsPolyZ(false).ToBigPoly());

            // cache the previous base
            MultivariatePolynomial<UnivariatePolynomial<BigInteger>> previousBase = null;

            // number of times interpolation did not change the result
            int nUnchangedInterpolations = 0;
            BigInteger bBasePrime = Rings.Z.ValueOf(basePrime);

            // over all primes
            while (true)
            {
                long prime = primesLoop.Take();
                BigInteger bPrime = Rings.Z.ValueOf(prime);
                IntegersZp64 ring = Rings.Zp64(prime);
                minimalPolyMod = UnivariatePolynomialZp64.AsOverZp64(minimalPoly, ring);
                numberFieldMod = new FiniteField<long>(minimalPolyMod);

                // reduce Z -> Zp
                aMod = a.MapCoefficients(numberFieldMod, (c) => UnivariatePolynomialZp64.AsOverZp64(c, ring));
                bMod = b.MapCoefficients(numberFieldMod, (c) => UnivariatePolynomialZp64.AsOverZp64(c, ring));
                if (!aMod.SameSkeletonQ(a) || !bMod.SameSkeletonQ(b))
                    continue;

                // calculate new resultant using previously calculated skeleton via sparse interpolation
                MultivariatePolynomial<UnivariatePolynomialZp64> modularResultant;
                try
                {
                    modularResultant = InterpolateResultant(aMod, bMod,
                        @base.MapCoefficients(numberFieldMod, (cf) => cf.SetModulusUnsafe(ring.modulus)), random);
                }
                catch (Exception t)
                {
                    continue;
                }

                if (modularResultant == null)
                {
                    // interpolation failed => assumed form is wrong => start over
                    continue;
                }


                // something wrong, start over
                if (!modularResultant.SameSkeletonQ(bBase))
                {
                    @base = modularResultant;
                    bBase = modularResultant.MapCoefficients(auxRing, (cf) => cf.AsPolyZ(false).ToBigPoly());
                    bBasePrime = bPrime;
                    previousBase = null;
                    nUnchangedInterpolations = 0;
                    continue;
                }


                //lifting
                BigInteger newBasePrime = bBasePrime * (bPrime);
                PairedIterator<UnivariatePolynomial<BigInteger>, UnivariatePolynomialZp64> iterator =
                    new PairedIterator<UnivariatePolynomial<BigInteger>, UnivariatePolynomialZp64>(bBase,
                        modularResultant);
                ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic =
                    ChineseRemainders.CreateMagic(Rings.Z, bBasePrime, bPrime);
                while (iterator.MoveNext())
                {
                    Monomial<UnivariatePolynomial<BigInteger>> baseTerm = iterator.aTerm;
                    Monomial<UnivariatePolynomialZp64> imageTerm = iterator.bTerm;
                    if (baseTerm.coefficient.IsZero())

                        // term is absent in the base
                        continue;
                    if (imageTerm.coefficient.IsZero())
                    {
                        // term is absent in the modularResultant => remove it from the base
                        // bBase.subtract(baseTerm);
                        bBase.Subtract(baseTerm);
                        continue;
                    }

                    UnivariateGCD.UpdateCRT(magic, baseTerm.coefficient, imageTerm.coefficient);
                }

                bBasePrime = newBasePrime;
                IntegersZp crtRing = Rings.Zp(bBasePrime);

                // two trials didn't change the result, probably we are done
                MultivariatePolynomial<UnivariatePolynomial<BigInteger>> candidate = bBase.MapCoefficients(numberField,
                    (cf) => UnivariatePolynomialZp64.AsPolyZSymmetric(cf.SetRingUnsafe(crtRing)));
                if (previousBase != null && candidate.Equals(previousBase))
                    ++nUnchangedInterpolations;
                else
                    nUnchangedInterpolations = 0;
                if (nUnchangedInterpolations >= N_UNCHANGED_INTERPOLATIONS)
                    return candidate;
                previousBase = candidate;
            }
        }
    }

    static MultivariatePolynomial<UnivariatePolynomialZp64> InterpolateResultant(
        MultivariatePolynomial<UnivariatePolynomialZp64> a, MultivariatePolynomial<UnivariatePolynomialZp64> b,
        MultivariatePolynomial<UnivariatePolynomialZp64> skeleton, Random rnd)
    {
        a.AssertSameCoefficientRingWith(b);
        if (a.nVariables == 2)
        {
            return BivariateResultant(a.AsUnivariateEliminate(0), b.AsUnivariateEliminate(0));
        }

        skeleton = skeleton.SetRingUnsafe(a.ring);
        SparseInterpolationE<UnivariatePolynomialZp64> interpolation =
            CreateInterpolation(-1, a.AsUnivariateEliminate(0), b.AsUnivariateEliminate(0), skeleton, 1, rnd);
        if (interpolation == null)
            return null;
        MultivariatePolynomial<UnivariatePolynomialZp64> res = interpolation.Evaluate();
        if (res == null)
            return null;
        return res;
    }

    /* ============================================== Brown algorithm ============================================= */


    static MultivariatePolynomial<E> BrownResultant<E>(MultivariatePolynomial<E> a,
        MultivariatePolynomial<E> b, int variable)
    {
        ResultantInput<E> resInput = PreparedResultantInput(a, b, variable);
        if (resInput.earlyResultant != null)
            return resInput.earlyResultant;
        if (resInput.finiteExtensionDegree > 1)
            return resInput.RestoreResultant(ResultantInSmallCharacteristic(resInput.aReduced, resInput.bReduced));
        MultivariatePolynomial<E> result = BrownResultantE(resInput.aReduced, resInput.bReduced, resInput.degreeBounds,
            resInput.lastPresentVariable);
        if (result == null)
            return resInput.RestoreResultant(ResultantInSmallCharacteristic(resInput.aReduced, resInput.bReduced));
        return resInput.RestoreResultant(result);
    }


    static MultivariatePolynomial<E> BrownResultantE<E>(UnivariatePolynomial<MultivariatePolynomial<E>> a,
        UnivariatePolynomial<MultivariatePolynomial<E>> b, int[] degreeBounds, int variable)
    {
        if (variable == 0)
            return BivariateResultant(a, b);
        MultivariateRing<E> mRing = (MultivariateRing<E>)a.ring;
        MultivariatePolynomial<E> factory = mRing.Factory();
        Ring<E> ring = factory.ring;

        //dense interpolation
        MultivariateInterpolation.Interpolation<E> interpolation = null;

        //store points that were already used in interpolation
        HashSet<E> evaluationStack = new HashSet<E>();
        Random rnd = PrivateRandom.GetRandom();
        while (true)
        {
            if (ring.Cardinality().Value.IsInt() && evaluationStack.Count == (int)ring.Cardinality().Value)

                // all elements of the ring are tried
                return null;
            E v;
            do
            {
                v = MultivariateGCD.randomElement(ring, rnd);
            } while (evaluationStack.Contains(v));

            E randomPoint = v;
            evaluationStack.Add(randomPoint);
            MultivariateRing<E> imageRing = mRing.DropVariable();
            UnivariatePolynomial<MultivariatePolynomial<E>> aMod =
                    a.MapCoefficients(imageRing, (cf) => cf.Eliminate(variable, randomPoint)),
                bMod = b.MapCoefficients(imageRing, (cf) => cf.Eliminate(variable, randomPoint));
            if (aMod.Degree() != a.Degree() || bMod.Degree() != b.Degree())
                continue;
            MultivariatePolynomial<E> modResultant =
                BrownResultantE(aMod, bMod, degreeBounds, variable - 1).InsertVariable(variable);
            if (interpolation == null)
            {
                //first successful homomorphism
                interpolation = new MultivariateInterpolation.Interpolation<E>(variable, randomPoint, modResultant);
                continue;
            }


            // update interpolation
            interpolation.Update(randomPoint, modResultant);
            if (interpolation.NumberOfPoints() > degreeBounds[variable])
                return interpolation.GetInterpolatingPolynomial();
        }
    }

    /* ============================================== Zippel algorithm ============================================= */


    static MultivariatePolynomial<E> ZippelResultant<E>(MultivariatePolynomial<E> a,
        MultivariatePolynomial<E> b, int variable)
    {
        ResultantInput<E> resInput = PreparedResultantInput(a, b, variable);
        if (resInput.earlyResultant != null)
            return resInput.earlyResultant;
        if (resInput.finiteExtensionDegree > 1)
            return resInput.RestoreResultant(ResultantInSmallCharacteristic(resInput.aReduced, resInput.bReduced));
        MultivariatePolynomial<E> result = ZippelResultantE(resInput.aReduced, resInput.bReduced, resInput.degreeBounds,
            resInput.lastPresentVariable);
        if (result == null)
            return resInput.RestoreResultant(ResultantInSmallCharacteristic(resInput.aReduced, resInput.bReduced));
        return resInput.RestoreResultant(result);
    }


    static MultivariatePolynomial<E> ZippelResultantE<E>(UnivariatePolynomial<MultivariatePolynomial<E>> a,
        UnivariatePolynomial<MultivariatePolynomial<E>> b, int[] degreeBounds, int variable)
    {
        if (variable == 0)
            return BivariateResultant(a, b);
        MultivariateRing<E> mRing = (MultivariateRing<E>)a.ring;
        MultivariatePolynomial<E> factory = mRing.Factory();
        Ring<E> ring = factory.ring;

        //dense interpolation
        MultivariateInterpolation.Interpolation<E> denseInterpolation;

        //sparse interpolation
        SparseInterpolationE<E> sparseInterpolation;

        //store points that were already used in interpolation
        HashSet<E> globalEvaluationStack = new HashSet<E>();
        Random rnd = PrivateRandom.GetRandom();
        main:
        while (true)
        {
            if (ring.Cardinality().Value.IsInt() && globalEvaluationStack.Count == (int)ring.Cardinality().Value)

                // all elements of the ring are tried
                return null;
            E v;
            do
            {
                v = MultivariateGCD.randomElement(ring, rnd);
            } while (globalEvaluationStack.Contains(v));

            E seedRandomPoint = v;
            globalEvaluationStack.Add(seedRandomPoint);
            MultivariateRing<E> imageRing = mRing.DropVariable();
            UnivariatePolynomial<MultivariatePolynomial<E>> aMod =
                    a.MapCoefficients(imageRing, (cf) => cf.Eliminate(variable, seedRandomPoint)),
                bMod = b.MapCoefficients(imageRing, (cf) => cf.Eliminate(variable, seedRandomPoint));
            if (aMod.Degree() != a.Degree() || bMod.Degree() != b.Degree())
                continue;

            // more checks
            for (int i = 0; i <= a.Degree(); i++)
            {
                ImmutableList<DegreeVector> iniSkeleton = a[i].DropVariable(variable).GetSkeleton();
                ImmutableList<DegreeVector> modSkeleton = aMod[i].GetSkeleton();
                if (!iniSkeleton.Equals(modSkeleton))
                    continue;
            }

            for (int i = 0; i <= b.Degree(); i++)
            {
                ImmutableList<DegreeVector> iniSkeleton = b[i].DropVariable(variable).GetSkeleton();
                ImmutableList<DegreeVector> modSkeleton = bMod[i].GetSkeleton();
                if (!iniSkeleton.Equals(modSkeleton))
                    continue;
            }


            // base evaluation
            MultivariatePolynomial<E> baseResultant =
                ZippelResultantE(aMod, bMod, degreeBounds, variable - 1).InsertVariable(variable);
            denseInterpolation =
                new MultivariateInterpolation.Interpolation<E>(variable, seedRandomPoint, baseResultant);
            sparseInterpolation = CreateInterpolation(variable, a, b, baseResultant, degreeBounds[variable], rnd);

            //local evaluation stack for points that are calculated via sparse interpolation (but not resultant evaluation) -> always same skeleton
            HashSet<E> localEvaluationStack = new HashSet<E>(globalEvaluationStack);
            while (true)
            {
                if (ring.Cardinality().Value.IsInt() && localEvaluationStack.Count == (int)ring.Cardinality().Value)

                    // all elements of the ring are tried
                    continue;
                do
                {
                    v = MultivariateGCD.randomElement(ring, rnd);
                } while (localEvaluationStack.Contains(v));

                E randomPoint = v;
                localEvaluationStack.Add(randomPoint);
                MultivariatePolynomial<E> modResultant = sparseInterpolation.Evaluate(randomPoint);
                if (modResultant == null)
                    continue;

                // update dense interpolation
                denseInterpolation.Update(randomPoint, modResultant);
                if (denseInterpolation.NumberOfPoints() > degreeBounds[variable])
                    return denseInterpolation.GetInterpolatingPolynomial();
            }
        }
    }

    static SparseInterpolationE<E> CreateInterpolation<E>(int variable,
        UnivariatePolynomial<MultivariatePolynomial<E>> a, UnivariatePolynomial<MultivariatePolynomial<E>> b,
        MultivariatePolynomial<E> skeleton, int expectedNumberOfEvaluations, Random rnd)
    {
        MultivariatePolynomial<E> factory = a.Lc();
        skeleton = skeleton.Clone().SetAllCoefficientsToUnit();
        ImmutableList<DegreeVector> globalSkeleton = skeleton.GetSkeleton();
        Dictionary<int, MultivariatePolynomial<E>> univarSkeleton = MultivariateGCD.getSkeleton(skeleton);
        int[] sparseUnivarDegrees = univarSkeleton.Keys.ToArray();
        Ring<E> ring = factory.ring;
        int lastVariable = variable == -1 ? factory.nVariables - 1 : variable;
        int[] evaluationVariables = Utils.Utils.Sequence(1, lastVariable + 1); //variable inclusive
        E[] evaluationPoint = new E[evaluationVariables.Length];
        MultivariatePolynomial<E>.PrecomputedPowersHolder powers;
        int fails = 0;

        while (true)
        {
            if (fails >= MultivariateGCD.MAX_FAILED_SUBSTITUTIONS)
                return null;

            //avoid zero evaluation points
            for (int i = lastVariable - 1; i >= 0; --i)
                do
                {
                    evaluationPoint[i] = MultivariateGCD.randomElement(ring, rnd);
                } while (ring.IsZero(evaluationPoint[i]));

            powers = MkPrecomputedPowers(a, b, evaluationVariables, evaluationPoint);
            IEnumerable<MultivariatePolynomial<E>> it = a.Stream().Concat(b.Stream()).Append(skeleton);
            foreach (var p in it)
            {
                if (!p.GetSkeleton(0).Equals(p.Evaluate(powers, evaluationVariables).GetSkeleton()))
                {
                    ++fails;
                    goto search_for_good_evaluation_point;
                }
            }


            break;
            search_for_good_evaluation_point: ;
        }

        int requiredNumberOfEvaluations = -1;
        foreach (var it in univarSkeleton)
        {
            MultivariatePolynomial<E> v = it.Value;
            if (v.Size() > requiredNumberOfEvaluations)
                requiredNumberOfEvaluations = v.Size();
        }

        return new SparseInterpolationE<E>(ring, variable, a, b, globalSkeleton, univarSkeleton, sparseUnivarDegrees,
            evaluationVariables, evaluationPoint, powers, expectedNumberOfEvaluations, requiredNumberOfEvaluations,
            rnd);
    }

    static MultivariatePolynomial<E>.PrecomputedPowersHolder MkPrecomputedPowers<E>(
        UnivariatePolynomial<MultivariatePolynomial<E>> a, UnivariatePolynomial<MultivariatePolynomial<E>> b,
        int[] evaluationVariables, E[] evaluationPoint)
    {
        MultivariatePolynomial<E> factory = a.Lc();
        int[] degrees = null;
        for (int i = 0; i <= a.Degree(); i++)
        {
            if (degrees == null)
                degrees = a[i].DegreesRef();
            else
                degrees = Utils.Utils.Max(degrees, a[i].DegreesRef());
        }

        for (int i = 0; i <= b.Degree(); i++)
            degrees = Utils.Utils.Max(degrees, b[i].DegreesRef());
        MultivariatePolynomial<E>.PrecomputedPowers[] pp =
            new MultivariatePolynomial<E>.PrecomputedPowers[factory.nVariables];
        for (int i = 0; i < evaluationVariables.Length; ++i)
            pp[evaluationVariables[i]] = new MultivariatePolynomial<E>.PrecomputedPowers(
                Math.Min(degrees[evaluationVariables[i]], MultivariatePolynomialZp64.MAX_POWERS_CACHE_SIZE),
                evaluationPoint[i], factory.ring);
        return new MultivariatePolynomial<E>.PrecomputedPowersHolder(factory.ring, pp);
    }

    sealed class SparseInterpolationE<E>
    {
        readonly Ring<E> ring;


        readonly int variable;


        readonly UnivariatePolynomial<MultivariatePolynomial<E>> a, b;


        readonly ImmutableList<DegreeVector> globalSkeleton;


        readonly Dictionary<int, MultivariatePolynomial<E>> univarSkeleton;


        readonly int[] sparseUnivarDegrees;


        readonly int[] evaluationVariables;


        readonly E[] evaluationPoint;


        readonly MultivariatePolynomial<E>.PrecomputedPowersHolder powers;


        readonly MultivariateGCD.ZippelEvaluations<E>[] aEvals, bEvals;


        readonly int requiredNumberOfEvaluations;


        readonly Random rnd;


        readonly MultivariatePolynomial<E> factory;


        public SparseInterpolationE(Ring<E> ring, int variable, UnivariatePolynomial<MultivariatePolynomial<E>> a,
            UnivariatePolynomial<MultivariatePolynomial<E>> b, ImmutableList<DegreeVector> globalSkeleton,
            Dictionary<int, MultivariatePolynomial<E>> univarSkeleton, int[] sparseUnivarDegrees,
            int[] evaluationVariables, E[] evaluationPoint, MultivariatePolynomial<E>.PrecomputedPowersHolder powers,
            int expectedNumberOfEvaluations, int requiredNumberOfEvaluations, Random rnd)
        {
            this.ring = ring;
            this.variable = variable;
            this.a = a;
            this.b = b;
            this.globalSkeleton = globalSkeleton;
            this.univarSkeleton = univarSkeleton;
            this.sparseUnivarDegrees = sparseUnivarDegrees;
            this.evaluationPoint = evaluationPoint;
            this.aEvals = new MultivariateGCD.ZippelEvaluations<E>[a.Degree() + 1];
            this.bEvals = new MultivariateGCD.ZippelEvaluations<E>[b.Degree() + 1];
            for (int i = 0; i < aEvals.Length; ++i)
            {
                aEvals[i] = MultivariateGCD.CreateEvaluations(a[i], evaluationVariables, evaluationPoint, powers,
                    expectedNumberOfEvaluations);
            }

            for (int i = 0; i < bEvals.Length; ++i)
                bEvals[i] = MultivariateGCD.CreateEvaluations(b[i], evaluationVariables, evaluationPoint, powers,
                    expectedNumberOfEvaluations);
            this.evaluationVariables = evaluationVariables;
            this.powers = powers;
            this.requiredNumberOfEvaluations = requiredNumberOfEvaluations;
            this.rnd = rnd;
            this.factory = a.Lc();
        }


        public MultivariatePolynomial<E> Evaluate()
        {
            return Evaluate(evaluationPoint[evaluationPoint.Length - 1]);
        }


        public MultivariatePolynomial<E> Evaluate(E newPoint)
        {
            // variable = newPoint
            evaluationPoint[evaluationPoint.Length - 1] = newPoint;
            powers.Set(evaluationVariables[evaluationVariables.Length - 1], newPoint);
            return Evaluate0(newPoint);
        }


        // variable = newPoint
        private MultivariatePolynomial<E>? Evaluate0(E newPoint)
        {
            MultivariateGCD.VandermondeSystem<E>[] systems =
                new MultivariateGCD.VandermondeSystem<E>[sparseUnivarDegrees.Length];
            for (int i = 0; i < sparseUnivarDegrees.Length; i++)
                systems[i] = new MultivariateGCD.VandermondeSystem<E>(sparseUnivarDegrees[i],
                    univarSkeleton[sparseUnivarDegrees[i]],
                    powers, variable == -1 ? factory.nVariables - 1 : variable - 1);
            for (int i = 0; i < requiredNumberOfEvaluations; ++i)
            {
                // sequential powers of evaluation point
                int raiseFactor = i + 1;
                E lastVarValue = newPoint;
                if (variable == -1)
                    lastVarValue = ring.Pow(lastVarValue, raiseFactor);

                // evaluate a and b to bivariate and calculate resultant
                var aBivar = UnivariatePolynomial<UnivariatePolynomial<E>>.Zero(Rings.UnivariateRing(ring));
                var bBivar = UnivariatePolynomial<UnivariatePolynomial<E>>.Zero(Rings.UnivariateRing(ring));
                for (int j = 0; j < aEvals.Length; ++j)
                {
                    aBivar[j] = aEvals[j].evaluate(raiseFactor, lastVarValue);
                    if (aBivar[j].Degree() != a[j].Degree(0))
                        return null;
                }

                for (int j = 0; j < bEvals.Length; ++j)
                {
                    bBivar[j] = bEvals[j].evaluate(raiseFactor, lastVarValue);
                    if (bBivar[j].Degree() != b[j].Degree(0))
                        return null;
                }

                UnivariatePolynomial<E> resUnivar = UnivariateResultants.Resultant(aBivar, bBivar);
                if (!univarSkeleton.Keys.ContainsAll(resUnivar.Exponents()))

                    // univariate resultant contains terms that are not present in the skeleton
                    // again unlucky main homomorphism
                    return null;
                bool allDone = true;
                foreach (MultivariateGCD.VandermondeSystem<E> system in systems)
                    if (system.nEquations() < system.nUnknownVariables())
                    {
                        E rhs = resUnivar.Degree() < system.univarDegree
                            ? ring.GetZero()
                            : resUnivar[system.univarDegree];
                        system.oneMoreEquation(rhs);
                        if (system.nEquations() < system.nUnknownVariables())
                            allDone = false;
                    }

                if (allDone)
                    break;
            }

            foreach (MultivariateGCD.VandermondeSystem<E> system in systems)
            {
                //solve each system
                LinearSolver.SystemInfo info = system.solve();
                if (info != LinearSolver.SystemInfo.Consistent)

                    // system is inconsistent or under determined
                    // unlucky homomorphism
                    return null;
            }

            MultivariatePolynomial<E> resVal = factory.CreateZero();
            foreach (MultivariateGCD.VandermondeSystem<E> system in systems)
            {
                for (int i = 0; i < system.skeleton.Length; i++)
                {
                    Monomial<E> degreeVector = system.skeleton[i].Set(0, system.univarDegree);
                    E value = system.solution[i];
                    resVal.Add(degreeVector.SetCoefficient(value));
                }
            }

            return resVal;
        }
    }
}