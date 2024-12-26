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


    public static MultivariatePolynomial<E> Resultant<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b, int variable)
    {
        a.AssertSameCoefficientRingWith(b);
        if (a.IsOverFiniteField())
            return ResultantInGF(a, b, variable);
        if (a.IsOverZ())
            return ResultantInZ(a, b, variable);
        if (Util.IsOverRationals(a))
            return ResultantInQ(a, b, variable);
        if (Util.IsOverSimpleNumberField(a))
            return ModularResultantInNumberField(a, b, variable);
        if (Util.IsOverRingOfIntegersOfSimpleNumberField(a))
            return ModularResultantInRingOfIntegersOfNumberField(a,
                b, variable);
        if (Util.IsOverMultipleFieldExtension(a))
            return ResultantInMultipleFieldExtension(a, b,
                variable);
        if (a.IsOverField())
            return ZippelResultant(a, b,
                variable);
        return TryNested(a, b, variable);
    }

    private static MultivariatePolynomial<E> TryNested<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b, int variable)
    {
        if (IsOverUnivariate(a))
            return ResultantOverUnivariate(a, b, variable);
        else if (IsOverUnivariateZp64(a))
            return ResultantOverUnivariateZp64(a, b, variable);
        else if (IsOverMultivariate(a))
            return ResultantOverMultivariate(a, b, variable);
        else if (IsOverMultivariateZp64(a))
            return ResultantOverMultivariateZp64(a, b, variable);
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

    private static MultivariatePolynomial<UnivariatePolynomialZp64> ResultantOverUnivariateZp64(
        MultivariatePolynomial<UnivariatePolynomialZp64> a, MultivariatePolynomial<UnivariatePolynomialZp64> b,
        int variable)
    {
        return Resultant(MultivariatePolynomialZp64.AsNormalMultivariate(a, 0),
            MultivariatePolynomialZp64.AsNormalMultivariate(b, 0), 1 + variable).AsOverUnivariateEliminate(0);
    }

    private static MultivariatePolynomial<MultivariatePolynomialZp64> ResultantOverMultivariateZp64(
        MultivariatePolynomial<MultivariatePolynomialZp64> a, MultivariatePolynomial<MultivariatePolynomialZp64> b,
        int variable)
    {
        int[] cfVars = Utils.Utils.Sequence(a.Lc().nVariables);
        int[] mainVars = Utils.Utils.Sequence(a.Lc().nVariables, a.Lc().nVariables + a.nVariables);
        return Resultant(MultivariatePolynomialZp64.AsNormalMultivariate(a, cfVars, mainVars),
                MultivariatePolynomialZp64.AsNormalMultivariate(b, cfVars, mainVars), cfVars.Length + variable)
            .AsOverMultivariateEliminate(cfVars);
    }

    static MultivariatePolynomial<Rational<E>> ResultantInQ<E>(MultivariatePolynomial<Rational<E>> a,
        MultivariatePolynomial<Rational<E>> b, int variable)
    {
        var aRat = Util.ToCommonDenominator(a);
        var bRat = Util.ToCommonDenominator(b);
        Ring<E> ring = aRat.Item1.ring;
        E correction = ring.Multiply(ring.Pow(aRat.Item2, b.Degree(variable)), ring.Pow(bRat.Item2, a.Degree(variable)));
        return Util.AsOverRationals(a.ring, Resultant(aRat.Item1, bRat.Item1, variable))
            .DivideExact(new Rational<E>(ring, correction));
    }

    private static MultivariatePolynomial<MultivariatePolynomial<E>> ResultantInMultipleFieldExtension<E>(
        MultivariatePolynomial<MultivariatePolynomial<E>> a, MultivariatePolynomial<MultivariatePolynomial<E>> b, int variable)
    {
        MultipleFieldExtension<E> ring = (MultipleFieldExtension<E>)a.ring;
        SimpleFieldExtension<E> simpleExtension = ring.GetSimpleExtension();
        return Resultant(a.MapCoefficients(simpleExtension, ring.Inverse),
            b.MapCoefficients(simpleExtension, ring.Inverse), variable).MapCoefficients(ring, ring.Image);
    }


    public static MultivariatePolynomial<E> ResultantInGF<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b, int variable)
    {
        return ZippelResultant(a, b, variable);
    }


    public static MultivariatePolynomial<BigInteger> ResultantInZ(MultivariatePolynomial<BigInteger> a,
        MultivariatePolynomial<BigInteger> b, int variable)
    {
        return ModularResultantInZ(a, b, variable);
    }


    public static MultivariatePolynomial<E> ClassicalResultant<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b, int variable)
    {
        if (CanConvertToZp64(a))
            return ConvertFromZp64(ClassicalResultant(AsOverZp64(a), AsOverZp64(b), variable));
        return UnivariateResultants.Resultant(a.AsUnivariateEliminate(variable), b.AsUnivariateEliminate(variable))
            .InsertVariable(variable);
    }

    /* ============================================== Auxiliary methods ============================================= */


    sealed class ResultantInput<E>
    {
        public readonly MultivariatePolynomial<E> aReduced0;
        public readonly MultivariatePolynomial<E> bReduced0;


        readonly UnivariatePolynomial<MultivariatePolynomial<E>> aReduced, bReduced;


        public readonly MultivariatePolynomial<E> earlyResultant;


        readonly int[] degreeBounds, mapping;


        readonly int lastPresentVariable;


        readonly int evaluationStackLimit;


        readonly MultivariatePolynomial<E> monomialResultant;


        readonly int finiteExtensionDegree;


        ResultantInput(MultivariatePolynomial<E> earlyResultant)
        {
            this.earlyResultant = earlyResultant;
            aReduced0 = bReduced0 = null;
            aReduced = bReduced = null;
            degreeBounds = mapping = null;
            lastPresentVariable = evaluationStackLimit = -1;
            monomialResultant = null;
            finiteExtensionDegree = -1;
        }


        ResultantInput(MultivariatePolynomial<E> aReduced0, MultivariatePolynomial<E> bReduced0, MultivariatePolynomial<E> monomialResultant, int evaluationStackLimit,
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
            this.mapping = InversePermutation(mapping);
            this.lastPresentVariable = lastPresentVariable;
            this.finiteExtensionDegree = finiteExtensionDegree;
        }


        //assert monomialGCD == null || aReduced.ring.isOne(monomialGCD.coefficient);


        public MultivariatePolynomial<E> RestoreResultant(MultivariatePolynomial<E> result)
        {
            return AMultivariatePolynomial.RenameVariables(result.InsertVariable(0), mapping)
                .Multiply(monomialResultant);
        }
    }

    static ResultantInput<E> PreparedResultantInput<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b, int variable)
    {
        var trivialResultant = TrivialResultant(a, b, variable);
        if (trivialResultant != null)
            return new ResultantInput<E>(trivialResultant);
        BigInteger? ringSize = a.CoefficientRingCardinality();

        // ring cardinality, i.e. number of possible random choices
        int evaluationStackLimit = ringSize == null ? -1 : (ringSize.IsInt() ? ringSize.IntValue() : -1);

        // find monomial GCD
        // and remove monomial content from a and b
        a = a.Clone();
        b = b.Clone(); // prevent rewriting original data
        var aContent = a.MonomialContent();var bContent = b.MonomialContent();
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
            degreeBounds[i] = SafeToInt(SafeAdd(SafeMultiply(aDegrees[i], bDegrees[variable]),
                SafeMultiply(bDegrees[i], aDegrees[variable])));
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
        degreeBounds = Arrays.CopyOfRange(degreeBounds, 1, degreeBounds.Length);
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
        int cardinalityBound = SafeToInt(SafeMultiply(9, ArraysUtil.Max(degreeBounds)));
        if (ringSize != null && ringSize.IsInt() && ringSize.IntValueExact() < cardinalityBound)
        {
            long ds = ringSize.IntValueExact();
            finiteExtensionDegree = 2;
            long tmp = ds;
            for (; tmp < cardinalityBound; ++finiteExtensionDegree)
                tmp = tmp * ds;
        }

        return new ResultantInput<E>(a, b, monomialResultant, evaluationStackLimit, degreeBounds, variables,
            lastResVariable, finiteExtensionDegree);
    }

    private static void AdjustDegreeBounds<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b, int variable, int[] degreeBounds)
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

    private static MultivariatePolynomial<E> TrivialResultant<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b, int variable)
    {
        if (a == b || a.IsZero() || b.IsZero() || a.Equals(b))
            return a.CreateZero();
        if (a.Degree(variable) == 0)
            return PolynomialMethods.PolyPow(a, b.Degree(variable));
        if (b.Degree(variable) == 0)
            return PolynomialMethods.PolyPow(b, a.Degree(variable));
        if (a.Count == 1)
            return ResultantWithMonomial(a.Lt(), b, variable);
        if (b.Count == 1)
            return ResultantWithMonomial(a, b.Lt(), variable);
        if (a.UnivariateVariable() == variable && b.UnivariateVariable() == variable)
            return MultivariatePolynomial<E>.AsMultivariate(
                UnivariateResultants.ResultantAsPoly(a.AsUnivariate(), b.AsUnivariate()), a.nVariables, variable,
                a.ordering);
        return null;
    }


    private static MultivariatePolynomial<E> ResultantWithMonomial<E>(Monomial<E> monomial, MultivariatePolynomial<E> poly, int variable)
    {
        int varExponent = monomial.exponents[variable];
        var cFactor = PolynomialMethods.PolyPow(poly.Create(monomial.Set(variable, 0)), poly.Degree(variable));
        var xFactor = PolynomialMethods.PolyPow(poly.EvaluateAtZero(variable), varExponent);
        return cFactor.Multiply(xFactor);
    }


    private static MultivariatePolynomial<E> ResultantWithMonomial<E>(MultivariatePolynomial<E> poly, Monomial<E> monomial, int variable)
    {
        var r = ResultantWithMonomial(monomial, poly, variable);
        if (poly.Degree(variable) % 2 == 1 && monomial.exponents[variable] % 2 == 1)
            r.Negate();
        return r;
    }


    static MultivariatePolynomialZp64 BivariateResultantZp64(UnivariatePolynomial<MultivariatePolynomialZp64> a,
        UnivariatePolynomial<MultivariatePolynomialZp64> b)
    {
        MultivariatePolynomialZp64 factory = a.Lc();
        IntegersZp64 ring = (IntegersZp64)factory.ring;
        var uRing = Rings.UnivariateRing(ring);
        UnivariatePolynomial<UnivariatePolynomialZp64> aUni =
                a.MapCoefficients(uRing, c => c.AsUnivariate()),
            bUni = b.MapCoefficients(uRing, c => c.AsUnivariate());
        return UnivariateResultants.Resultant(aUni, bUni).AsMultivariate(factory.ordering)
            .SetNVariables(factory.nVariables);
    }


    static MultivariatePolynomial<E> BivariateResultantE<E>(UnivariatePolynomial<MultivariatePolynomial<E>> a,
        UnivariatePolynomial<MultivariatePolynomial<E>> b)
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


    static MultivariatePolynomial<E> BivariateResultant<E>(
        UnivariatePolynomial<MultivariatePolynomial<E>> a, UnivariatePolynomial<MultivariatePolynomial<E>> b)
    {
        if (a.Lc() is MultivariatePolynomialZp64)
            return BivariateResultantZp64((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        else
            return BivariateResultantE((UnivariatePolynomial)a, (UnivariatePolynomial)b);
    }

    /* =========================================== In small characteristic ========================================== */


    public static MultivariatePolynomial<E> ResultantInSmallCharacteristic<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b, int variable)
    {
        // use "naive" algorithm for now.
        return ClassicalResultant(a, b, variable);
    }


    static MultivariatePolynomial<E> ResultantInSmallCharacteristic<E>(UnivariatePolynomial<MultivariatePolynomial<E>> a, UnivariatePolynomial<MultivariatePolynomial<E>> b)
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
        return Rings.Z.Factorial(n).DivideExact(Rings.Z.Factorial(q).Pow(d - r)).DivideExact(Rings.Z.Factorial(q + 1).Pow(r));
    }


    static MultivariatePolynomial<BigInteger> ModularResultantInZ0(MultivariatePolynomial<BigInteger> a,
        MultivariatePolynomial<BigInteger> b)
    {
        // coefficient bound (not quite optimistic:)
        BigInteger aMax = a.MaxAbsCoefficient(), bMax = b.MaxAbsCoefficient();
        BigInteger bound2 = Rings.Z.GetOne().Multiply(aMax.Pow(b.Degree()))
            .Multiply(CentralMultinomialCoefficient(a.Count, b.Degree())).Multiply(bMax.Pow(a.Degree()))
            .Multiply(CentralMultinomialCoefficient(b.Count, a.Degree())).Multiply(Z.Factorial(a.Degree() + b.Degree()))
            .ShiftLeft(1); // symmetric Zp form

        // choose better prime for start
        long startingPrime;
        if (Math.Max(aMax.GetBitLength(), bMax.GetBitLength()) < 128)
            startingPrime = 1 << 30;
        else
            startingPrime = 1 << 60;
        PrimesIterator primesLoop = new PrimesIterator(startingPrime - (1 << 12));
        Random random = PrivateRandom.GetRandom();
        main_loop:
        while (true)
        {
            // prepare the skeleton
            long basePrime = primesLoop.Take();
            IntegersZp64 ring = Zp64(basePrime);

            // reduce Z -> Zp
            MultivariatePolynomialZp64 aMod = a.MapCoefficientsZp64(ring, (c) => c.Mod(basePrime).LongValue()),
                bMod = b.MapCoefficientsZp64(ring, (c) => c.Mod(basePrime).LongValue());
            if (!aMod.SameSkeletonQ(a) || !bMod.SameSkeletonQ(b))
                continue;

            // the base image
            // accumulator to update coefficients via Chineese remainding
            MultivariatePolynomialZp64 base = ResultantInGF(aMod, bMod, 0).DropVariable(0);
            MultivariatePolynomialZp64 skeleton = @base;
            MultivariatePolynomial<BigInteger> bBase = @base.ToBigPoly();

            // cache the previous base
            MultivariatePolynomial<BigInteger> previousBase = null;

            // number of times interpolation did not change the result
            int nUnchangedInterpolations = 0;
            BigInteger bBasePrime = Z.ValueOf(basePrime);

            // over all primes
            while (true)
            {
                long prime = primesLoop.Take();
                BigInteger bPrime = Z.ValueOf(prime);
                ring = Zp64(prime);

                // reduce Z -> Zp
                aMod = a.MapCoefficientsZp64(ring, (c) => c.Mod(prime).LongValue());
                bMod = b.MapCoefficientsZp64(ring, (c) => c.Mod(prime).LongValue());
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
                BigInteger newBasePrime = bBasePrime.Multiply(bPrime);
                PairedIterator<Monomial<BigInteger>, MultivariatePolynomial<BigInteger>, MonomialZp64,
                    MultivariatePolynomialZp64> iterator = new PairedIterator(bBase, modularResultant);
                ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic = CreateMagic(Z, bBasePrime, bPrime);
                while (iterator.HasNext())
                {
                    iterator.Advance();
                    Monomial<BigInteger> baseTerm = iterator.aTerm;
                    MonomialZp64 imageTerm = iterator.bTerm;
                    if (baseTerm.coefficient.IsZero())

                        // term is absent in the base
                        continue;
                    if (imageTerm.coefficient == 0)
                    {
                        // term is absent in the modularResultant => remove it from the base
                        // bBase.subtract(baseTerm);
                        iterator.aIterator.Remove();
                        continue;
                    }

                    long oth = imageTerm.coefficient;

                    // update base term
                    BigInteger newCoeff = ChineseRemainders(Z, magic, baseTerm.coefficient, BigInteger.ValueOf(oth));
                    bBase.Put(baseTerm.SetCoefficient(newCoeff));
                }

                bBase = bBase.SetRingUnsafe(new IntegersZp(newBasePrime));
                bBasePrime = newBasePrime;

                // two trials didn't change the result, probably we are done
                MultivariatePolynomial<BigInteger> candidate = MultivariatePolynomial.AsPolyZSymmetric(bBase);
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
        MultivariatePolynomialZp64 skeleton, RandomGenerator rnd)
    {
        a.AssertSameCoefficientRingWith(b);
        skeleton = skeleton.SetRingUnsafe(a.ring);
        if (a.nVariables == 2)
        {
            return BivariateResultant(a.AsUnivariateEliminate(0), b.AsUnivariateEliminate(0));
        }

        SparseInterpolationZp64 interpolation = CreateInterpolation(-1, a.AsUnivariateEliminate(0),
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
        AlgebraicNumberField<UnivariatePolynomial<E>> ring = (AlgebraicNumberField<UnivariatePolynomial<E>>)a.ring;
        if (!a.Stream().AllMatch(ring.IsInTheBaseField()) || !b.Stream().AllMatch(ring.IsInTheBaseField()))
            return null;
        Ring<E> cfRing = ring.GetMinimalPolynomial().ring;
        MultivariatePolynomial<E> ar = a.MapCoefficients(cfRing, UnivariatePolynomial.Cc()),
            br = b.MapCoefficients(cfRing, UnivariatePolynomial.Cc());
        return Resultant(ar, br, variable).MapCoefficients(ring, (cf) => UnivariatePolynomial.Constant(cfRing, cf));
    }


    public static MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> ModularResultantInNumberField(
        MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a,
        MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> b, int variable)
    {
        MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> res =
            TrivialResultantInExtension(a, b, variable);
        if (res != null)
            return res;
        AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> numberField =
            (AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>>)a.ring;
        UnivariatePolynomial<Rational<BigInteger>> minimalPoly = numberField.GetMinimalPolynomial();
        a = a.Clone();
        b = b.Clone();

        // reduce problem to the case with integer monic minimal polynomial
        if (minimalPoly.Stream().AllMatch(Rational.IsIntegral()))
        {
            // minimal poly is already monic & integer
            UnivariatePolynomial<BigInteger> minimalPolyZ = minimalPoly.MapCoefficients(Z, Rational.Numerator());
            AlgebraicNumberField<UnivariatePolynomial<BigInteger>>
                numberFieldZ = new AlgebraicNumberField(minimalPolyZ);
            BigInteger aDen = RemoveDenominators(a),
                bDen = RemoveDenominators(b),
                den = aDen.Pow(b.Degree(variable)).Multiply(bDen.Pow(a.Degree(variable)));
            return ModularResultantInRingOfIntegersOfNumberField(
                    a.MapCoefficients(numberFieldZ, (cf) => cf.MapCoefficients(Z, Rational.Numerator())),
                    b.MapCoefficients(numberFieldZ, (cf) => cf.MapCoefficients(Z, Rational.Numerator())), variable)
                .MapCoefficients(numberField, (cf) => cf.MapCoefficients(Q, (r) => Q.Mk(r, den)));
        }
        else
        {
            // replace s -> s / lc(minPoly)
            BigInteger minPolyLeadCoeff = CommonDenominator(minimalPoly);
            Rational<BigInteger> scale = new Rational(Z, Z.GetOne(), minPolyLeadCoeff),
                scaleReciprocal = scale.Reciprocal();
            AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> scaledNumberField =
                new AlgebraicNumberField(minimalPoly.Scale(scale).Monic());
            return ModularResultantInNumberField(a.MapCoefficients(scaledNumberField, (cf) => cf.Scale(scale)),
                    b.MapCoefficients(scaledNumberField, (cf) => cf.Scale(scale)), variable)
                .MapCoefficients(numberField, (cf) => cf.Scale(scaleReciprocal));
        }
    }

    static BigInteger RemoveDenominators(MultivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a)
    {
        BigInteger denominator = Z.Lcm(() => a.Stream().Map(Util.CommonDenominator()).Iterator());
        a.Multiply(a.ring.ValueOfBigInteger(denominator));
        return denominator;
    }


    public static MultivariatePolynomial<UnivariatePolynomial<BigInteger>>
        ModularResultantInRingOfIntegersOfNumberField(MultivariatePolynomial<UnivariatePolynomial<BigInteger>> a,
            MultivariatePolynomial<UnivariatePolynomial<BigInteger>> b, int variable)
    {
        ResultantInput<Monomial<UnivariatePolynomial<BigInteger>>,
            MultivariatePolynomial<UnivariatePolynomial<BigInteger>>> resInput = PreparedResultantInput(a, b, variable);
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
        AlgebraicNumberField<UnivariatePolynomial<BigInteger>> numberField =
            (AlgebraicNumberField<UnivariatePolynomial<BigInteger>>)a.ring;
        UnivariatePolynomial<BigInteger> minimalPoly = numberField.GetMinimalPolynomial();
        BigInteger aMax = a.Stream().Map(UnivariatePolynomial.MaxAbsCoefficient()).Max(Rings.Z).OrElse(BigInteger.ONE),
            bMax = b.Stream().Map(UnivariatePolynomial.MaxAbsCoefficient()).Max(Rings.Z).OrElse(BigInteger.ONE),
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
        if (Math.Max(aMax.BitLength(), bMax.BitLength()) < 128)
            startingPrime = 1 << 30;
        else
            startingPrime = 1 << 60;
        UnivariateRing<UnivariatePolynomial<BigInteger>> auxRing = UnivariateRing(Z);
        PrimesIterator primesLoop = new PrimesIterator(startingPrime - (1 << 12));
        RandomGenerator random = PrivateRandom.GetRandom();
        main_loop:
        while (true)
        {
            // prepare the skeleton
            long basePrime = primesLoop.Take();
            IntegersZp64 baseRing = Zp64(basePrime);
            UnivariatePolynomialZp64 minimalPolyMod = UnivariatePolynomial.AsOverZp64(minimalPoly, baseRing);
            FiniteField<UnivariatePolynomialZp64> numberFieldMod = new FiniteField(minimalPolyMod);

            // reduce Z -> Zp
            MultivariatePolynomial<UnivariatePolynomialZp64> aMod =
                    a.MapCoefficients(numberFieldMod, (c) => UnivariatePolynomial.AsOverZp64(c, baseRing)),
                bMod = b.MapCoefficients(numberFieldMod, (c) => UnivariatePolynomial.AsOverZp64(c, baseRing));
            if (!aMod.SameSkeletonQ(a) || !bMod.SameSkeletonQ(b))
                continue;

            // the base image
            // accumulator to update coefficients via Chineese remainding
            MultivariatePolynomial < UnivariatePolynomialZp64 > base;
            try
            {
                @base = ResultantInGF(aMod, bMod, 0).DropVariable(0);
            }
            catch (Throwable t)
            {
                continue;
            } // bad base prime

            MultivariatePolynomial<UnivariatePolynomial<BigInteger>> bBase =
                @base.MapCoefficients(auxRing, (cf) => cf.AsPolyZ(false).ToBigPoly());

            // cache the previous base
            MultivariatePolynomial<UnivariatePolynomial<BigInteger>> previousBase = null;

            // number of times interpolation did not change the result
            int nUnchangedInterpolations = 0;
            BigInteger bBasePrime = Z.ValueOf(basePrime);

            // over all primes
            while (true)
            {
                long prime = primesLoop.Take();
                BigInteger bPrime = Z.ValueOf(prime);
                IntegersZp64 ring = Zp64(prime);
                minimalPolyMod = UnivariatePolynomial.AsOverZp64(minimalPoly, ring);
                numberFieldMod = new FiniteField(minimalPolyMod);

                // reduce Z -> Zp
                aMod = a.MapCoefficients(numberFieldMod, (c) => UnivariatePolynomial.AsOverZp64(c, ring));
                bMod = b.MapCoefficients(numberFieldMod, (c) => UnivariatePolynomial.AsOverZp64(c, ring));
                if (!aMod.SameSkeletonQ(a) || !bMod.SameSkeletonQ(b))
                    continue;

                // calculate new resultant using previously calculated skeleton via sparse interpolation
                MultivariatePolynomial<UnivariatePolynomialZp64> modularResultant;
                try
                {
                    modularResultant = InterpolateResultant(aMod, bMod,
                        @base.MapCoefficients(numberFieldMod, (cf) => cf.SetModulusUnsafe(ring)), random);
                }
                catch (Throwable t)
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
                BigInteger newBasePrime = bBasePrime.Multiply(bPrime);
                PairedIterator<Monomial<UnivariatePolynomial<BigInteger>>,
                    MultivariatePolynomial<UnivariatePolynomial<BigInteger>>, Monomial<UnivariatePolynomialZp64>,
                    MultivariatePolynomial<UnivariatePolynomialZp64>> iterator =
                    new PairedIterator(bBase, modularResultant);
                ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic = CreateMagic(Z, bBasePrime, bPrime);
                while (iterator.HasNext())
                {
                    iterator.Advance();
                    Monomial<UnivariatePolynomial<BigInteger>> baseTerm = iterator.aTerm;
                    Monomial<UnivariatePolynomialZp64> imageTerm = iterator.bTerm;
                    if (baseTerm.coefficient.IsZero())

                        // term is absent in the base
                        continue;
                    if (imageTerm.coefficient.IsZero())
                    {
                        // term is absent in the modularResultant => remove it from the base
                        // bBase.subtract(baseTerm);
                        iterator.aIterator.Remove();
                        continue;
                    }

                    UnivariateGCD.UpdateCRT(magic, baseTerm.coefficient, imageTerm.coefficient);
                }

                bBasePrime = newBasePrime;
                IntegersZp crtRing = Zp(bBasePrime);

                // two trials didn't change the result, probably we are done
                MultivariatePolynomial<UnivariatePolynomial<BigInteger>> candidate = bBase.MapCoefficients(numberField,
                    (cf) => UnivariatePolynomial.AsPolyZSymmetric(cf.SetRingUnsafe(crtRing)));
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
        MultivariatePolynomial<UnivariatePolynomialZp64> skeleton, RandomGenerator rnd)
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


    static Poly BrownResultant<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(Poly a,
        Poly b, int variable)
    {
        if (a is MultivariatePolynomialZp64)
            return (Poly)BrownResultant((MultivariatePolynomialZp64)a, (MultivariatePolynomialZp64)b, variable);
        else
            return (Poly)BrownResultant((MultivariatePolynomial)a, (MultivariatePolynomial)b, variable);
    }


    public static MultivariatePolynomialZp64 BrownResultant(MultivariatePolynomialZp64 a, MultivariatePolynomialZp64 b,
        int variable)
    {
        ResultantInput<MonomialZp64, MultivariatePolynomialZp64> resInput = PreparedResultantInput(a, b, variable);
        if (resInput.earlyResultant != null)
            return resInput.earlyResultant;
        if (resInput.finiteExtensionDegree > 1)
            return resInput.RestoreResultant(ResultantInSmallCharacteristic(resInput.aReduced, resInput.bReduced));
        MultivariatePolynomialZp64 result = BrownResultantZp64(resInput.aReduced, resInput.bReduced,
            resInput.degreeBounds, resInput.lastPresentVariable);
        if (result == null)
            return resInput.RestoreResultant(ResultantInSmallCharacteristic(resInput.aReduced, resInput.bReduced));
        return resInput.RestoreResultant(result);
    }


    static MultivariatePolynomialZp64 BrownResultantZp64(UnivariatePolynomial<MultivariatePolynomialZp64> a,
        UnivariatePolynomial<MultivariatePolynomialZp64> b, int[] degreeBounds, int variable)
    {
        if (variable == 0)
            return BivariateResultantZp64(a, b);
        MultivariateRing<MultivariatePolynomialZp64> mRing = (MultivariateRing<MultivariatePolynomialZp64>)a.ring;
        MultivariatePolynomialZp64 factory = mRing.Factory();
        IntegersZp64 ring = factory.ring;

        //dense interpolation
        MultivariateInterpolation.InterpolationZp64 interpolation = null;

        //store points that were already used in interpolation
        TLongHashSet evaluationStack = new TLongHashSet();
        RandomGenerator rnd = PrivateRandom.GetRandom();
        while (true)
        {
            if (evaluationStack.Count == ring.modulus)

                // all elements of the ring are tried
                return null;
            long v;
            do
            {
                v = ring.RandomElement(rnd);
            } while (evaluationStack.Contains(v));

            long randomPoint = v;
            evaluationStack.Add(randomPoint);
            MultivariateRing<MultivariatePolynomialZp64> imageRing = mRing.DropVariable();
            UnivariatePolynomial<MultivariatePolynomialZp64> aMod =
                    a.MapCoefficients(imageRing, (cf) => cf.Eliminate(variable, randomPoint)),
                bMod = b.MapCoefficients(imageRing, (cf) => cf.Eliminate(variable, randomPoint));
            if (aMod.Degree() != a.Degree() || bMod.Degree() != b.Degree())
                continue;
            MultivariatePolynomialZp64 modResultant =
                BrownResultantZp64(aMod, bMod, degreeBounds, variable - 1).InsertVariable(variable);
            if (interpolation == null)
            {
                //first successful homomorphism
                interpolation = new InterpolationZp64(variable, randomPoint, modResultant);
                continue;
            }


            // update interpolation
            interpolation.Update(randomPoint, modResultant);
            if (interpolation.NumberOfPoints() > degreeBounds[variable])
                return interpolation.GetInterpolatingPolynomial();
        }
    }


    public static MultivariatePolynomial<E> BrownResultant<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b,
        int variable)
    {
        if (CanConvertToZp64(a))
            return ConvertFromZp64(BrownResultant(AsOverZp64(a), AsOverZp64(b), variable));
        ResultantInput<Monomial<E>, MultivariatePolynomial<E>> resInput = PreparedResultantInput(a, b, variable);
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
            return BivariateResultantE(a, b);
        MultivariateRing<MultivariatePolynomial<E>> mRing = (MultivariateRing<MultivariatePolynomial<E>>)a.ring;
        MultivariatePolynomial<E> factory = mRing.Factory();
        Ring<E> ring = factory.ring;

        //dense interpolation
        MultivariateInterpolation.Interpolation<E> interpolation = null;

        //store points that were already used in interpolation
        HashSet<E> evaluationStack = new HashSet();
        RandomGenerator rnd = PrivateRandom.GetRandom();
        while (true)
        {
            if (ring.Cardinality().IsInt() && evaluationStack.Count == ring.Cardinality().IntValue())

                // all elements of the ring are tried
                return null;
            E v;
            do
            {
                v = RandomElement(ring, rnd);
            } while (evaluationStack.Contains(v));

            E randomPoint = v;
            evaluationStack.Add(randomPoint);
            MultivariateRing<MultivariatePolynomial<E>> imageRing = mRing.DropVariable();
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
                interpolation = new MultivariateInterpolation.Interpolation<>(variable, randomPoint, modResultant);
                continue;
            }


            // update interpolation
            interpolation.Update(randomPoint, modResultant);
            if (interpolation.NumberOfPoints() > degreeBounds[variable])
                return interpolation.GetInterpolatingPolynomial();
        }
    }

    /* ============================================== Zippel algorithm ============================================= */


    static Poly ZippelResultant<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(Poly a,
        Poly b, int variable)
    {
        if (a is MultivariatePolynomialZp64)
            return (Poly)ZippelResultant((MultivariatePolynomialZp64)a, (MultivariatePolynomialZp64)b, variable);
        else
            return (Poly)ZippelResultant((MultivariatePolynomial)a, (MultivariatePolynomial)b, variable);
    }


    public static MultivariatePolynomialZp64 ZippelResultant(MultivariatePolynomialZp64 a, MultivariatePolynomialZp64 b,
        int variable)
    {
        ResultantInput<MonomialZp64, MultivariatePolynomialZp64> resInput = PreparedResultantInput(a, b, variable);
        if (resInput.earlyResultant != null)
            return resInput.earlyResultant;
        if (resInput.finiteExtensionDegree > 1)
            return resInput.RestoreResultant(ResultantInSmallCharacteristic(resInput.aReduced, resInput.bReduced));
        MultivariatePolynomialZp64 result = ZippelResultantZp64(resInput.aReduced, resInput.bReduced,
            resInput.degreeBounds, resInput.lastPresentVariable);
        if (result == null)
            return resInput.RestoreResultant(ResultantInSmallCharacteristic(resInput.aReduced, resInput.bReduced));
        return resInput.RestoreResultant(result);
    }


    static MultivariatePolynomialZp64 ZippelResultantZp64(UnivariatePolynomial<MultivariatePolynomialZp64> a,
        UnivariatePolynomial<MultivariatePolynomialZp64> b, int[] degreeBounds, int variable)
    {
        if (variable == 0)
            return BivariateResultantZp64(a, b);
        MultivariateRing<MultivariatePolynomialZp64> mRing = (MultivariateRing<MultivariatePolynomialZp64>)a.ring;
        MultivariatePolynomialZp64 factory = mRing.Factory();
        IntegersZp64 ring = factory.ring;

        //dense interpolation
        MultivariateInterpolation.InterpolationZp64 denseInterpolation;

        //sparse interpolation
        SparseInterpolationZp64 sparseInterpolation;

        //store points that were already used in interpolation
        TLongHashSet globalEvaluationStack = new TLongHashSet();
        RandomGenerator rnd = PrivateRandom.GetRandom();
        main:
        while (true)
        {
            if (globalEvaluationStack.Count == ring.modulus)

                // all elements of the ring are tried
                return null;
            long v;
            do
            {
                v = ring.RandomElement(rnd);
            } while (globalEvaluationStack.Contains(v));

            long seedRandomPoint = v;
            globalEvaluationStack.Add(seedRandomPoint);
            MultivariateRing<MultivariatePolynomialZp64> imageRing = mRing.DropVariable();
            UnivariatePolynomial<MultivariatePolynomialZp64> aMod =
                    a.MapCoefficients(imageRing, (cf) => cf.Eliminate(variable, seedRandomPoint)),
                bMod = b.MapCoefficients(imageRing, (cf) => cf.Eliminate(variable, seedRandomPoint));
            if (aMod.Degree() != a.Degree() || bMod.Degree() != b.Degree())
                continue;

            // more checks
            for (int i = 0; i <= a.Degree(); i++)
            {
                HashSet<DegreeVector> iniSkeleton = a[i].DropVariable(variable).GetSkeleton();
                HashSet<DegreeVector> modSkeleton = aMod[i].GetSkeleton();
                if (!iniSkeleton.Equals(modSkeleton))
                    continue;
            }

            for (int i = 0; i <= b.Degree(); i++)
            {
                HashSet<DegreeVector> iniSkeleton = b[i].DropVariable(variable).GetSkeleton();
                HashSet<DegreeVector> modSkeleton = bMod[i].GetSkeleton();
                if (!iniSkeleton.Equals(modSkeleton))
                    continue;
            }


            // base evaluation
            MultivariatePolynomialZp64 baseResultant =
                ZippelResultantZp64(aMod, bMod, degreeBounds, variable - 1).InsertVariable(variable);
            denseInterpolation = new InterpolationZp64(variable, seedRandomPoint, baseResultant);
            sparseInterpolation = CreateInterpolation(variable, a, b, baseResultant, degreeBounds[variable], rnd);

            //local evaluation stack for points that are calculated via sparse interpolation (but not resultant evaluation) -> always same skeleton
            TLongHashSet localEvaluationStack = new TLongHashSet(globalEvaluationStack);
            while (true)
            {
                if (localEvaluationStack.Count == ring.modulus)

                    // all elements of the ring are tried
                    continue;
                do
                {
                    v = ring.RandomElement(rnd);
                } while (localEvaluationStack.Contains(v));

                long randomPoint = v;
                localEvaluationStack.Add(randomPoint);
                MultivariatePolynomialZp64 modResultant = sparseInterpolation.Evaluate(randomPoint);
                if (modResultant == null)
                    continue;

                // update dense interpolation
                denseInterpolation.Update(randomPoint, modResultant);
                if (denseInterpolation.NumberOfPoints() > degreeBounds[variable])
                    return denseInterpolation.GetInterpolatingPolynomial();
            }
        }
    }


    public static MultivariatePolynomial<E> ZippelResultant<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b,
        int variable)
    {
        if (CanConvertToZp64(a))
            return ConvertFromZp64(ZippelResultant(AsOverZp64(a), AsOverZp64(b), variable));
        ResultantInput<Monomial<E>, MultivariatePolynomial<E>> resInput = PreparedResultantInput(a, b, variable);
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
            return BivariateResultantE(a, b);
        MultivariateRing<MultivariatePolynomial<E>> mRing = (MultivariateRing<MultivariatePolynomial<E>>)a.ring;
        MultivariatePolynomial<E> factory = mRing.Factory();
        Ring<E> ring = factory.ring;

        //dense interpolation
        MultivariateInterpolation.Interpolation<E> denseInterpolation;

        //sparse interpolation
        SparseInterpolationE<E> sparseInterpolation;

        //store points that were already used in interpolation
        HashSet<E> globalEvaluationStack = new HashSet();
        RandomGenerator rnd = PrivateRandom.GetRandom();
        main:
        while (true)
        {
            if (ring.Cardinality().IsInt() && globalEvaluationStack.Count == ring.Cardinality().IntValueExact())

                // all elements of the ring are tried
                return null;
            E v;
            do
            {
                v = RandomElement(ring, rnd);
            } while (globalEvaluationStack.Contains(v));

            E seedRandomPoint = v;
            globalEvaluationStack.Add(seedRandomPoint);
            MultivariateRing<MultivariatePolynomial<E>> imageRing = mRing.DropVariable();
            UnivariatePolynomial<MultivariatePolynomial<E>> aMod =
                    a.MapCoefficients(imageRing, (cf) => cf.Eliminate(variable, seedRandomPoint)),
                bMod = b.MapCoefficients(imageRing, (cf) => cf.Eliminate(variable, seedRandomPoint));
            if (aMod.Degree() != a.Degree() || bMod.Degree() != b.Degree())
                continue;

            // more checks
            for (int i = 0; i <= a.Degree(); i++)
            {
                HashSet<DegreeVector> iniSkeleton = a[i].DropVariable(variable).GetSkeleton();
                HashSet<DegreeVector> modSkeleton = aMod[i].GetSkeleton();
                if (!iniSkeleton.Equals(modSkeleton))
                    continue;
            }

            for (int i = 0; i <= b.Degree(); i++)
            {
                HashSet<DegreeVector> iniSkeleton = b[i].DropVariable(variable).GetSkeleton();
                HashSet<DegreeVector> modSkeleton = bMod[i].GetSkeleton();
                if (!iniSkeleton.Equals(modSkeleton))
                    continue;
            }


            // base evaluation
            MultivariatePolynomial<E> baseResultant =
                ZippelResultantE(aMod, bMod, degreeBounds, variable - 1).InsertVariable(variable);
            denseInterpolation = new MultivariateInterpolation.Interpolation<>(variable, seedRandomPoint, baseResultant);
            sparseInterpolation = CreateInterpolation(variable, a, b, baseResultant, degreeBounds[variable], rnd);

            //local evaluation stack for points that are calculated via sparse interpolation (but not resultant evaluation) -> always same skeleton
            HashSet<E> localEvaluationStack = new HashSet(globalEvaluationStack);
            while (true)
            {
                if (ring.Cardinality().IsInt() && localEvaluationStack.Count == ring.Cardinality().IntValueExact())

                    // all elements of the ring are tried
                    continue;
                do
                {
                    v = RandomElement(ring, rnd);
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

    static SparseInterpolationZp64 CreateInterpolation(int variable, UnivariatePolynomial<MultivariatePolynomialZp64> a,
        UnivariatePolynomial<MultivariatePolynomialZp64> b, MultivariatePolynomialZp64 skeleton,
        int expectedNumberOfEvaluations, RandomGenerator rnd)
    {
        MultivariatePolynomialZp64 factory = a.Lc();
        skeleton = skeleton.Clone().SetAllCoefficientsToUnit();
        HashSet<DegreeVector> globalSkeleton = skeleton.GetSkeleton();
        TIntObjectHashMap<MultivariatePolynomialZp64> univarSkeleton = GetSkeleton(skeleton);
        int[] sparseUnivarDegrees = univarSkeleton.Keys();
        IntegersZp64 ring = factory.ring;
        int lastVariable = variable == -1 ? factory.nVariables - 1 : variable;
        int[] evaluationVariables = ArraysUtil.Sequence(1, lastVariable + 1); //variable inclusive
        long[] evaluationPoint = new long[evaluationVariables.Length];
        MultivariatePolynomialZp64.lPrecomputedPowersHolder powers;
        int fails = 0;
        search_for_good_evaluation_point:
        while (true)
        {
            if (fails >= MAX_FAILED_SUBSTITUTIONS)
                return null;

            //avoid zero evaluation points
            for (int i = lastVariable - 1; i >= 0; --i)
                do
                {
                    evaluationPoint[i] = ring.RandomElement(rnd);
                } while (evaluationPoint[i] == 0);

            powers = MkPrecomputedPowers(a, b, evaluationVariables, evaluationPoint);
            IEnumerator<MultivariatePolynomialZp64> it =
                Stream.Concat(Stream.Concat(a.Stream(), b.Stream()), Stream.Of(skeleton)).Iterator();
            while (it.HasNext())
            {
                MultivariatePolynomialZp64 p = it.Next();
                if (!p.GetSkeleton(0).Equals(p.Evaluate(powers, evaluationVariables).GetSkeleton()))
                {
                    ++fails;
                    continue;
                }
            }

            break;
        }

        int requiredNumberOfEvaluations = -1;
        for (TIntObjectIterator<MultivariatePolynomialZp64> it = univarSkeleton.iterator(); it.HasNext();)
        {
            it.Advance();
            MultivariatePolynomialZp64 v = it.Value();
            if (v.Count > requiredNumberOfEvaluations)
                requiredNumberOfEvaluations = v.Count;
        }

        return new SparseInterpolationZp64(ring, variable, a, b, globalSkeleton, univarSkeleton, sparseUnivarDegrees,
            evaluationVariables, evaluationPoint, powers, expectedNumberOfEvaluations, requiredNumberOfEvaluations,
            rnd);
    }

    static MultivariatePolynomialZp64.lPrecomputedPowersHolder MkPrecomputedPowers(
        UnivariatePolynomial<MultivariatePolynomialZp64> a, UnivariatePolynomial<MultivariatePolynomialZp64> b,
        int[] evaluationVariables, long[] evaluationPoint)
    {
        MultivariatePolynomialZp64 factory = a.Lc();
        int[] degrees = null;
        for (int i = 0; i <= a.Degree(); i++)
        {
            if (degrees == null)
                degrees = a[i].DegreesRef();
            else
                degrees = ArraysUtil.Max(degrees, a[i].DegreesRef());
        }

        for (int i = 0; i <= b.Degree(); i++)
            degrees = ArraysUtil.Max(degrees, b[i].DegreesRef());
        MultivariatePolynomialZp64.lPrecomputedPowers[] pp =
            new MultivariatePolynomialZp64.lPrecomputedPowers[factory.nVariables];
        for (int i = 0; i < evaluationVariables.Length; ++i)
            pp[evaluationVariables[i]] = new lPrecomputedPowers(
                Math.Min(degrees[evaluationVariables[i]], MultivariatePolynomialZp64.MAX_POWERS_CACHE_SIZE),
                evaluationPoint[i], factory.ring);
        return new lPrecomputedPowersHolder(factory.ring, pp);
    }

    sealed class SparseInterpolationZp64
    {
        readonly IntegersZp64 ring;


        readonly int variable;


        readonly UnivariatePolynomial<MultivariatePolynomialZp64> a, b;


        readonly HashSet<DegreeVector> globalSkeleton;


        readonly TIntObjectHashMap<MultivariatePolynomialZp64> univarSkeleton;


        readonly int[] sparseUnivarDegrees;


        readonly int[] evaluationVariables;


        readonly long[] evaluationPoint;


        readonly MultivariatePolynomialZp64.lPrecomputedPowersHolder powers;


        readonly MultivariateGCD.ZippelEvaluationsZp64[] aEvals, bEvals;


        readonly int requiredNumberOfEvaluations;


        readonly RandomGenerator rnd;


        readonly MultivariatePolynomialZp64 factory;

        SparseInterpolationZp64(IntegersZp64 ring, int variable, UnivariatePolynomial<MultivariatePolynomialZp64> a,
            UnivariatePolynomial<MultivariatePolynomialZp64> b, HashSet<DegreeVector> globalSkeleton,
            TIntObjectHashMap<MultivariatePolynomialZp64> univarSkeleton, int[] sparseUnivarDegrees,
            int[] evaluationVariables, long[] evaluationPoint,
            MultivariatePolynomialZp64.lPrecomputedPowersHolder powers, int expectedNumberOfEvaluations,
            int requiredNumberOfEvaluations, RandomGenerator rnd)
        {
            this.ring = ring;
            this.variable = variable;
            this.a = a;
            this.b = b;
            this.globalSkeleton = globalSkeleton;
            this.univarSkeleton = univarSkeleton;
            this.sparseUnivarDegrees = sparseUnivarDegrees;
            this.evaluationPoint = evaluationPoint;
            this.aEvals = new MultivariateGCD.ZippelEvaluationsZp64[a.Degree() + 1];
            this.bEvals = new MultivariateGCD.ZippelEvaluationsZp64[b.Degree() + 1];
            for (int i = 0; i < aEvals.Length; ++i)
                aEvals[i] = MultivariateGCD.CreateEvaluations(a[i], evaluationVariables, evaluationPoint, powers,
                    expectedNumberOfEvaluations);
            for (int i = 0; i < bEvals.Length; ++i)
                bEvals[i] = MultivariateGCD.CreateEvaluations(b[i], evaluationVariables, evaluationPoint, powers,
                    expectedNumberOfEvaluations);
            this.evaluationVariables = evaluationVariables;
            this.powers = powers;
            this.requiredNumberOfEvaluations = requiredNumberOfEvaluations;
            this.rnd = rnd;
            this.factory = a.Lc();
        }


        public MultivariatePolynomialZp64 Evaluate()
        {
            return Evaluate(evaluationPoint[evaluationPoint.Length - 1]);
        }


        public MultivariatePolynomialZp64 Evaluate(long newPoint)
        {
            // variable = newPoint
            evaluationPoint[evaluationPoint.Length - 1] = newPoint;
            powers[evaluationVariables[evaluationVariables.Length - 1]] = newPoint;
            return Evaluate0(newPoint);
        }

        private MultivariatePolynomialZp64 Evaluate0(long newPoint)
        {
            MultivariateGCD.lVandermondeSystem[] systems =
                new MultivariateGCD.lVandermondeSystem[sparseUnivarDegrees.Length];
            for (int i = 0; i < sparseUnivarDegrees.Length; i++)
                systems[i] = new lVandermondeSystem(sparseUnivarDegrees[i], univarSkeleton[sparseUnivarDegrees[i]],
                    powers, variable == -1 ? factory.nVariables - 1 : variable - 1);
            for (int i = 0; i < requiredNumberOfEvaluations; ++i)
            {
                // sequential powers of evaluation point
                int raiseFactor = i + 1;
                long lastVarValue = newPoint;
                if (variable == -1)
                    lastVarValue = ring.PowMod(lastVarValue, raiseFactor);

                // evaluate a and b to bivariate and calculate resultant
                UnivariatePolynomial<UnivariatePolynomialZp64> aBivar =
                        UnivariatePolynomial.Zero(UnivariateRingZp64(ring)),
                    bBivar = UnivariatePolynomial.Zero(UnivariateRingZp64(ring));
                for (int j = 0; j < aEvals.Length; ++j)
                {
                    aBivar[j] = aEvals[j].Evaluate(raiseFactor, lastVarValue);
                    if (aBivar[j].Degree() != a[j].Degree(0))
                        return null;
                }

                for (int j = 0; j < bEvals.Length; ++j)
                {
                    bBivar[j] = bEvals[j].Evaluate(raiseFactor, lastVarValue);
                    if (bBivar[j].Degree() != b[j].Degree(0))
                        return null;
                }

                UnivariatePolynomialZp64 resUnivar = UnivariateResultants.Resultant(aBivar, bBivar);
                if (!univarSkeleton.KeySet().ContainsAll(resUnivar.Exponents()))

                    // univariate resultant contains terms that are not present in the skeleton
                    // again unlucky main homomorphism
                    return null;
                bool allDone = true;
                foreach (MultivariateGCD.lVandermondeSystem system in systems)
                    if (system.NEquations() < system.NUnknownVariables())
                    {
                        long rhs = resUnivar.Degree() < system.univarDegree ? 0 : resUnivar[system.univarDegree];
                        system.OneMoreEquation(rhs);
                        if (system.NEquations() < system.NUnknownVariables())
                            allDone = false;
                    }

                if (allDone)
                    break;
            }

            foreach (MultivariateGCD.lVandermondeSystem system in systems)
            {
                //solve each system
                LinearSolver.SystemInfo info = system.Solve();
                if (info != LinearSolver.SystemInfo.Consistent)

                    // system is inconsistent or under determined
                    // unlucky homomorphism
                    return null;
            }

            MultivariatePolynomialZp64 resVal = factory.CreateZero();
            foreach (MultivariateGCD.lVandermondeSystem system in systems)
            {
                for (int i = 0; i < system.skeleton.Length; i++)
                {
                    MonomialZp64 degreeVector = system.skeleton[i][0] = system.univarDegree;
                    long value = system.solution[i];
                    resVal.Add(degreeVector.SetCoefficient(value));
                }
            }

            return resVal;
        }
    }

    static SparseInterpolationE<E> CreateInterpolation<E>(int variable,
        UnivariatePolynomial<MultivariatePolynomial<E>> a, UnivariatePolynomial<MultivariatePolynomial<E>> b,
        MultivariatePolynomial<E> skeleton, int expectedNumberOfEvaluations, RandomGenerator rnd)
    {
        MultivariatePolynomial<E> factory = a.Lc();
        skeleton = skeleton.Clone().SetAllCoefficientsToUnit();
        HashSet<DegreeVector> globalSkeleton = skeleton.GetSkeleton();
        TIntObjectHashMap<MultivariatePolynomial<E>> univarSkeleton = GetSkeleton(skeleton);
        int[] sparseUnivarDegrees = univarSkeleton.Keys();
        Ring<E> ring = factory.ring;
        int lastVariable = variable == -1 ? factory.nVariables - 1 : variable;
        int[] evaluationVariables = ArraysUtil.Sequence(1, lastVariable + 1); //variable inclusive
        E[] evaluationPoint = ring.CreateArray(evaluationVariables.Length);
        MultivariatePolynomial.PrecomputedPowersHolder<E> powers;
        int fails = 0;
        search_for_good_evaluation_point:
        while (true)
        {
            if (fails >= MAX_FAILED_SUBSTITUTIONS)
                return null;

            //avoid zero evaluation points
            for (int i = lastVariable - 1; i >= 0; --i)
                do
                {
                    evaluationPoint[i] = RandomElement(ring, rnd);
                } while (ring.IsZero(evaluationPoint[i]));

            powers = MkPrecomputedPowers(a, b, evaluationVariables, evaluationPoint);
            IEnumerator<MultivariatePolynomial<E>> it =
                Stream.Concat(Stream.Concat(a.Stream(), b.Stream()), Stream.Of(skeleton)).Iterator();
            while (it.HasNext())
            {
                MultivariatePolynomial<E> p = it.Next();
                if (!p.GetSkeleton(0).Equals(p.Evaluate(powers, evaluationVariables).GetSkeleton()))
                {
                    ++fails;
                    continue;
                }
            }

            break;
        }

        int requiredNumberOfEvaluations = -1;
        for (TIntObjectIterator<MultivariatePolynomial<E>> it = univarSkeleton.iterator(); it.HasNext();)
        {
            it.Advance();
            MultivariatePolynomial<E> v = it.Value();
            if (v.Count > requiredNumberOfEvaluations)
                requiredNumberOfEvaluations = v.Count;
        }

        return new SparseInterpolationE(ring, variable, a, b, globalSkeleton, univarSkeleton, sparseUnivarDegrees,
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
                degrees = ArraysUtil.Max(degrees, a[i].DegreesRef());
        }

        for (int i = 0; i <= b.Degree(); i++)
            degrees = ArraysUtil.Max(degrees, b[i].DegreesRef());
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


        readonly HashSet<DegreeVector> globalSkeleton;


        readonly TIntObjectHashMap<MultivariatePolynomial<E>> univarSkeleton;


        readonly int[] sparseUnivarDegrees;


        readonly int[] evaluationVariables;


        readonly E[] evaluationPoint;


        readonly MultivariatePolynomial.PrecomputedPowersHolder<E> powers;


        readonly MultivariateGCD.ZippelEvaluations<E>[] aEvals, bEvals;


        readonly int requiredNumberOfEvaluations;


        readonly RandomGenerator rnd;


        readonly MultivariatePolynomial<E> factory;


        SparseInterpolationE(Ring<E> ring, int variable, UnivariatePolynomial<MultivariatePolynomial<E>> a,
            UnivariatePolynomial<MultivariatePolynomial<E>> b, HashSet<DegreeVector> globalSkeleton,
            TIntObjectHashMap<MultivariatePolynomial<E>> univarSkeleton, int[] sparseUnivarDegrees,
            int[] evaluationVariables, E[] evaluationPoint, MultivariatePolynomial.PrecomputedPowersHolder<E> powers,
            int expectedNumberOfEvaluations, int requiredNumberOfEvaluations, RandomGenerator rnd)
        {
            this.ring = ring;
            this.variable = variable;
            this.a = a;
            this.b = b;
            this.globalSkeleton = globalSkeleton;
            this.univarSkeleton = univarSkeleton;
            this.sparseUnivarDegrees = sparseUnivarDegrees;
            this.evaluationPoint = evaluationPoint;
            this.aEvals = new MultivariateGCD.ZippelEvaluations[a.Degree() + 1];
            this.bEvals = new MultivariateGCD.ZippelEvaluations[b.Degree() + 1];
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
            powers[evaluationVariables[evaluationVariables.Length - 1]] = newPoint;
            return Evaluate0(newPoint);
        }


        // variable = newPoint
        private MultivariatePolynomial<E> Evaluate0(E newPoint)
        {
            MultivariateGCD.VandermondeSystem<E>[] systems =
                new MultivariateGCD.VandermondeSystem<E>[sparseUnivarDegrees.Length];
            for (int i = 0; i < sparseUnivarDegrees.Length; i++)
                systems[i] = new MultivariateGCD.VandermondeSystem<E>(sparseUnivarDegrees[i], univarSkeleton[sparseUnivarDegrees[i]],
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
                if (!univarSkeleton.KeySet().ContainsAll(resUnivar.Exponents()))

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
                    Monomial<E> degreeVector = system.skeleton[i][0] = system.univarDegree;
                    E value = system.solution[i];
                    resVal.Add(degreeVector.SetCoefficient(value));
                }
            }

            return resVal;
        }
    }
}