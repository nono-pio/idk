using System.Numerics;
using Polynomials.Poly.Multivar;
using Polynomials.Primes;
using Polynomials.Utils;
using static Polynomials.Poly.Univar.Conversions64bit;
using UnivariatePolynomialZ64 = Polynomials.Poly.Univar.UnivariatePolynomial<long>;
using UnivariatePolynomialZp64 = Polynomials.Poly.Univar.UnivariatePolynomial<long>;

namespace Polynomials.Poly.Univar;

public static class UnivariateFactorization
{
    public static PolynomialFactorDecomposition<UnivariatePolynomial<E>> Factor<E>(UnivariatePolynomial<E> poly)
    {
        if (poly.IsOverFiniteField())
            return FactorInGF(poly);
        else if (poly.IsOverZ())
            return FactorInZ(poly);
        else if (Util.IsOverRationals(poly))
            return (PolynomialFactorDecomposition<UnivariatePolynomial<E>>)GenericHandler.InvokeForGeneric<E>(
                typeof(Rational<>), nameof(FactorInQ), typeof(UnivariateFactorization), poly); //FactorInQ(poly);
        else if (Util.IsOverSimpleNumberField(poly))
            return FactorInNumberField(poly.AsT<UnivariatePolynomial<Rational<BigInteger>>>())
                as PolynomialFactorDecomposition<UnivariatePolynomial<E>>;
        else if (Util.IsOverMultipleFieldExtension(poly))
            return (PolynomialFactorDecomposition<UnivariatePolynomial<E>>)GenericHandler.InvokeForGeneric<E>(
                typeof(MultivariatePolynomial<>),
                nameof(FactorInMultipleFieldExtension), typeof(UnivariateFactorization),
                poly); // FactorInMultipleFieldExtension(poly);
        else if (IsOverMultivariate(poly))
            return  (PolynomialFactorDecomposition<UnivariatePolynomial<E>>)GenericHandler.InvokeForGeneric<E>(
                typeof(MultivariatePolynomial<>),
                nameof(FactorOverMultivariate), typeof(UnivariateFactorization),
                poly); // FactorOverMultivariate(poly);
        else if (IsOverUnivariate(poly))
            return (PolynomialFactorDecomposition<UnivariatePolynomial<E>>)GenericHandler.InvokeForGeneric<E>(
                typeof(UnivariatePolynomial<>),
                nameof(FactorOverUnivariate), typeof(UnivariateFactorization),
                poly); // FactorOverUnivariate(poly, MultivariateFactorization.Factor);
        else
            throw new Exception("ring is not supported");
    }

    public static bool IsOverMultivariate<E>(UnivariatePolynomial<E> poly)
    {
        return poly.ring is IMultivariateRing;
    }

    public static bool IsOverUnivariate<E>(UnivariatePolynomial<E> poly)
    {
        return poly.ring is IUnivariateRing;
    }

    public static PolynomialFactorDecomposition<UnivariatePolynomial<MultivariatePolynomial<E>>>
        FactorOverMultivariate<E>(UnivariatePolynomial<MultivariatePolynomial<E>> poly,
            Func<MultivariatePolynomial<E>, PolynomialFactorDecomposition<MultivariatePolynomial<E>>> factorFunction)
    {
        return factorFunction(MultivariatePolynomial<E>.AsMultivariate(poly, 0, true))
            .MapTo((p) => p.AsUnivariateEliminate(0));
    }
    
    
    public static PolynomialFactorDecomposition<UnivariatePolynomial<MultivariatePolynomial<E>>>
        FactorOverMultivariate<E>(UnivariatePolynomial<MultivariatePolynomial<E>> poly)
    {
        return MultivariateFactorization.Factor(MultivariatePolynomial<E>.AsMultivariate(poly, 0, true))
            .MapTo((p) => p.AsUnivariateEliminate(0));
    }

    public static PolynomialFactorDecomposition<UnivariatePolynomial<UnivariatePolynomial<E>>> FactorOverUnivariate<E>(
        UnivariatePolynomial<UnivariatePolynomial<E>> poly,
        Func<MultivariatePolynomial<UnivariatePolynomial<E>>,
                PolynomialFactorDecomposition<MultivariatePolynomial<UnivariatePolynomial<E>>>>
            factorFunction)
    {
        return factorFunction(
                MultivariatePolynomial<UnivariatePolynomial<E>>.AsMultivariate(poly, 1, 0, MonomialOrder.DEFAULT))
            .MapTo(m => m.AsUnivariate());
    }

    public static PolynomialFactorDecomposition<UnivariatePolynomial<UnivariatePolynomial<E>>> FactorOverUnivariate<E>(
        UnivariatePolynomial<UnivariatePolynomial<E>> poly)
    {
        return MultivariateFactorization.Factor(
                MultivariatePolynomial<UnivariatePolynomial<E>>.AsMultivariate(poly, 1, 0, MonomialOrder.DEFAULT))
            .MapTo(m => m.AsUnivariate());
    }

    public static PolynomialFactorDecomposition<UnivariatePolynomial<Rational<E>>> FactorInQ<E>(
        UnivariatePolynomial<Rational<E>> poly)
    {
        var cmd = Util.ToCommonDenominator(poly);
        var integral = cmd.Item1;
        var denominator = cmd.Item2;
        return Factor(integral).MapTo((p) => Util.AsOverRationals(poly.ring, p))
            .AddUnit(poly.CreateConstant(new Rational<E>(integral.ring, integral.ring.GetOne(), denominator)));
    }

    private static PolynomialFactorDecomposition<UnivariatePolynomial<MultivariatePolynomial<E>>>
        FactorInMultipleFieldExtension<E>(
            UnivariatePolynomial<MultivariatePolynomial<E>> poly)
    {
        var ring = (MultipleFieldExtension<E>)poly.ring;
        var simpleExtension = ring.GetSimpleExtension();
        return Factor(poly.MapCoefficients(simpleExtension, ring.Inverse))
            .MapTo((p) => p.MapCoefficients(ring, ring.Image));
    }


    private sealed class FactorMonomial<T>
    {
        public readonly T theRest;
        public readonly T monomial;

        public FactorMonomial(T theRest, T monomial)
        {
            this.theRest = theRest;
            this.monomial = monomial;
        }
    }


    private static FactorMonomial<UnivariatePolynomial<E>> FactorOutMonomial<E>(UnivariatePolynomial<E> poly)
    {
        var i = poly.FirstNonZeroCoefficientPosition();
        if (i == 0)
            return new FactorMonomial<UnivariatePolynomial<E>>(poly, poly.CreateOne());
        return new FactorMonomial<UnivariatePolynomial<E>>(poly.Clone().ShiftLeft(i), poly.CreateMonomial(i));
    }


    private static PolynomialFactorDecomposition<UnivariatePolynomial<E>>? EarlyFactorizationChecks<E>(
        UnivariatePolynomial<E> poly)
    {
        if (poly.Degree() <= 1 || poly.IsMonomial())
            return PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Of(poly.LcAsPoly(),
                poly.IsMonic() ? poly : poly.Clone().Monic());
        return null;
    }

    /* =========================================== Factorization in Zp[x] =========================================== */


    public static PolynomialFactorDecomposition<UnivariatePolynomial<E>> FactorInGF<E>(UnivariatePolynomial<E> poly)
    {
        Util.EnsureOverFiniteField(poly);
        if (CanConvertToZp64(poly))
            return FactorInGF(AsOverZp64(poly)).MapTo(Conversions64bit.Convert<E>);
        var result = EarlyFactorizationChecks(poly);
        if (result != null)
            return result;
        result = PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Empty(poly);
        FactorInGF(poly, result);
        return result.SetUnit(poly.LcAsPoly());
    }


    public static PolynomialFactorDecomposition<UnivariatePolynomial<E>> FactorSquareFreeInGF<E>(
        UnivariatePolynomial<E> poly)
    {
        Util.EnsureOverFiniteField(poly);
        if (CanConvertToZp64(poly))
            return FactorInGF(AsOverZp64(poly)).MapTo(Conversions64bit.Convert<E>);
        var result = PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Empty(poly);
        FactorSquareFreeInGF(poly, 1, result);
        return result;
    }

    private static void FactorSquareFreeInGF<E>(UnivariatePolynomial<E> poly, int exponent,
        PolynomialFactorDecomposition<UnivariatePolynomial<E>> result)
    {
        //do distinct-degree factorization
        var ddf = DistinctDegreeFactorization.GetDistinctDegreeFactorization(poly);

        //assertDistinctDegreeFactorization(sqfFactor, ddf);
        for (var j = 0; j < ddf.Count; ++j)
        {
            //for each distinct-degree factor
            var ddfFactor = ddf[j];
            var ddfExponent = ddf.Exponents[j];

            //do equal-degree factorization
            var edf = EqualDegreeFactorization.CantorZassenhaus(ddfFactor, ddfExponent);
            foreach (var irreducibleFactor in edf.Factors)

                //put final irreducible factor into the result
                result.AddFactor(irreducibleFactor.Monic(), exponent);
        }
    }

    private static void FactorInGF<E>(UnivariatePolynomial<E> poly,
        PolynomialFactorDecomposition<UnivariatePolynomial<E>> result)
    {
        var @base = FactorOutMonomial(poly);
        if (!@base.monomial.IsConstant())
            result.AddFactor(poly.CreateMonomial(1), @base.monomial.Degree());

        //do square-free factorization
        var sqf = UnivariateSquareFreeFactorization.SquareFreeFactorization(@base.theRest);

        //assert sqf.toPolynomial().equals(base.theRest) : base.toString();
        for (var i = 0; i < sqf.Count; ++i)
        {
            //for each square-free factor
            var sqfFactor = sqf[i];
            var sqfExponent = sqf.Exponents[i];
            FactorSquareFreeInGF(sqfFactor, sqfExponent, result);
        }
    }

    private static void AssertDistinctDegreeFactorization<E>(UnivariatePolynomial<E> poly,
        PolynomialFactorDecomposition<UnivariatePolynomial<E>> factorization)
    {
    }

    /* =========================================== Factorization in Z[x] =========================================== */


    static void AssertHenselLift<E>(HenselLifting.QuadraticLiftAbstract<E> lift)
    {
    }


    private static int[][] naturalSequenceRefCache = new int[32][];

    private static int[] CreateSeq(int n)
    {
        var r = new int[n];
        for (var i = 0; i < n; i++)
            r[i] = i;
        return r;
    }


    private static int[] NaturalSequenceRef(int n)
    {
        if (n >= naturalSequenceRefCache.Length)
            return CreateSeq(n);
        if (naturalSequenceRefCache[n] != null)
            return naturalSequenceRefCache[n];
        return naturalSequenceRefCache[n] = CreateSeq(n);
    }


    private static int[] Select(int[] data, int[] positions)
    {
        var r = new int[positions.Length];
        var i = 0;
        foreach (var p in positions)
            r[i++] = data[p];
        return r;
    }


    static PolynomialFactorDecomposition<UnivariatePolynomialZ64> ReconstructFactorsZ(UnivariatePolynomialZ64 poly,
        PolynomialFactorDecomposition<UnivariatePolynomialZp64> modularFactors)
    {
        if (modularFactors.IsTrivial())
            return PolynomialFactorDecomposition<UnivariatePolynomialZ64>.Of(poly);
        var factory = modularFactors[0];
        var modIndexes = NaturalSequenceRef(modularFactors.Count);
        var trueFactors =
            PolynomialFactorDecomposition<UnivariatePolynomialZ64>.Empty(poly);
        var fRest = poly;
        var s = 1;
        factor_combinations:
        while (2 * s <= modIndexes.Length)
        {
            foreach (var combination in Combinatorics.GetCombinations(modIndexes.Length, s))
            {
                var indexes = Select(modIndexes, combination);
                var mFactor = factory.CreateConstant(fRest.Lc());
                foreach (var i in indexes)
                    mFactor = mFactor.Multiply(modularFactors[i]);
                var factor = UnivariatePolynomialZ64.AsPolyZ64Symmetric(mFactor).PrimitivePart();
                if (fRest.Lc() % factor.Lc() != 0 || fRest.Cc() % factor.Cc() != 0)
                    continue;
                var mRest = factory.CreateConstant(fRest.Lc() / factor.Lc());
                var restIndexes = Utils.Utils.IntSetDifference(modIndexes, indexes);
                foreach (var i in restIndexes)
                    mRest = mRest.Multiply(modularFactors[i]);
                var rest = UnivariatePolynomialZ64.AsPolyZ64Symmetric(mRest).PrimitivePart();
                if (MachineArithmetic.SafeMultiply(factor.Lc(), rest.Lc()) != fRest.Lc() ||
                    MachineArithmetic.SafeMultiply(factor.Cc(), rest.Cc()) != fRest.Cc())
                    continue;
                if (rest.Clone().MultiplyUnsafe(factor).Equals(fRest))
                {
                    modIndexes = restIndexes;
                    trueFactors.AddFactor(factor, 1);
                    fRest = rest.PrimitivePart();
                    goto factor_combinations;
                }
            }

            ++s;
        }

        if (!fRest.IsConstant())
            trueFactors.AddFactor(fRest, 1);
        return trueFactors;
    }


    static PolynomialFactorDecomposition<UnivariatePolynomial<BigInteger>> ReconstructFactorsZ(
        UnivariatePolynomial<BigInteger> poly,
        PolynomialFactorDecomposition<UnivariatePolynomial<BigInteger>> modularFactors)
    {
        if (modularFactors.IsTrivial())
            return PolynomialFactorDecomposition<UnivariatePolynomial<BigInteger>>.Of(poly);
        var factory = modularFactors[0];
        var modIndexes = NaturalSequenceRef(modularFactors.Count);
        var trueFactors =
            PolynomialFactorDecomposition<UnivariatePolynomial<BigInteger>>.Empty(poly);
        var fRest = poly;
        var s = 1;
        factor_combinations:
        while (2 * s <= modIndexes.Length)
        {
            foreach (var combination in Combinatorics.GetCombinations(modIndexes.Length, s))
            {
                var indexes = Select(modIndexes, combination);
                var mFactor = factory.CreateConstant(fRest.Lc());
                foreach (var i in indexes)
                    mFactor = mFactor.Multiply(modularFactors[i]);
                var factor = UnivariatePolynomial<BigInteger>.AsPolyZSymmetric(mFactor)
                    .PrimitivePart();
                if (!(fRest.Lc() % factor.Lc()).IsZero || !(fRest.Cc() % factor.Cc()).IsZero)
                    continue;
                var mRest = factory.CreateConstant(fRest.Lc() / factor.Lc());
                var restIndexes = Utils.Utils.IntSetDifference(modIndexes, indexes);
                foreach (var i in restIndexes)
                    mRest = mRest.Multiply(modularFactors[i]);
                var rest = UnivariatePolynomial<BigInteger>.AsPolyZSymmetric(mRest)
                    .PrimitivePart();
                if (!(factor.Lc() * rest.Lc()).Equals(fRest.Lc()) ||
                    !(factor.Cc() * rest.Cc()).Equals(fRest.Cc()))
                    continue;
                if (rest.Clone().Multiply(factor).Equals(fRest))
                {
                    modIndexes = restIndexes;
                    trueFactors.AddFactor(factor, 1);
                    fRest = rest.PrimitivePart();
                    continue;
                }
            }

            ++s;
        }

        if (!fRest.IsConstant())
            trueFactors.AddFactor(fRest, 1);
        return trueFactors;
    }

    private static readonly double MAX_PRIME_GAP = 382,
        MIGNOTTE_MAX_DOUBLE_32 = (2.0 * int.MaxValue) - 10 * MAX_PRIME_GAP,
        MIGNOTTE_MAX_DOUBLE_64 = MIGNOTTE_MAX_DOUBLE_32 * MIGNOTTE_MAX_DOUBLE_32;

    private static readonly int LOWER_RND_MODULUS_BOUND = 1 << 24, UPPER_RND_MODULUS_BOUND = 1 << 30;

    private static int RandomModulusInf()
    {
        return LOWER_RND_MODULUS_BOUND +
               PrivateRandom.GetRandom().Next(UPPER_RND_MODULUS_BOUND - LOWER_RND_MODULUS_BOUND);
    }

    private static int Next32BitPrime(int val)
    {
        if (val < 0)
        {
            var l = BigPrimes.NextPrime(unchecked((uint)val));
            return (int)l;
        }
        else
            return SmallPrimes.NextPrime(val);
    }

    static readonly int N_MIN_MODULAR_FACTORIZATION_TRIALS = 2,
        N_SIMPLE_MOD_PATTERN_FACTORS = 12,
        N_MODULAR_FACTORIZATION_TRIALS = 4,
        N_TOO_MUCH_FACTORS_COUNT = 22,
        N_MODULAR_FACTORIZATION_TRIALS_TOO_MUCH_FACTORS =
            12; // maximal number of modular trials if there are too many factors


    static PolynomialFactorDecomposition<UnivariatePolynomial<BigInteger>> FactorSquareFreeInZ0(
        UnivariatePolynomial<BigInteger> poly)
    {
        var bound2 = new BigInteger(2) * (UnivariatePolynomial<BigInteger>.MignotteBound(poly))
                                       * BigInteger.Abs(poly.Lc());
        if (bound2.CompareTo(MachineArithmetic.b_MAX_SUPPORTED_MODULUS) < 0)
        {
            var tryLong =
                FactorSquareFreeInZ0(UnivariatePolynomial<BigInteger>.AsOverZ64(poly));
            if (tryLong != null)
                return ConvertFactorizationToBigIntegers(tryLong);
        }


        // choose prime at random
        long modulus = -1;
        UnivariatePolynomialZp64 moduloImage;
        PolynomialFactorDecomposition<UnivariatePolynomialZp64> lModularFactors = null;
        for (var attempt = 0;; attempt++)
        {
            if (attempt >= N_MIN_MODULAR_FACTORIZATION_TRIALS && lModularFactors.Count <= N_SIMPLE_MOD_PATTERN_FACTORS)
                break;
            if (attempt >= N_MODULAR_FACTORIZATION_TRIALS)
                if (lModularFactors.Count < N_TOO_MUCH_FACTORS_COUNT ||
                    attempt >= N_MODULAR_FACTORIZATION_TRIALS_TOO_MUCH_FACTORS)
                    break;
            long tmpModulus;
            do
            {
                tmpModulus = SmallPrimes.NextPrime(RandomModulusInf());
                moduloImage = UnivariatePolynomial<BigInteger>.AsOverZp64(poly.SetRing(new IntegersZp(tmpModulus)));
            } while (moduloImage.Cc() == 0 || moduloImage.Degree() != poly.Degree() ||
                     !UnivariateSquareFreeFactorization.IsSquareFree(moduloImage));

            // do modular factorization
            var tmpFactors = FactorInGF(moduloImage.Monic());
            if (tmpFactors.Count == 1)
                return PolynomialFactorDecomposition<UnivariatePolynomial<BigInteger>>.Of(poly);
            if (lModularFactors == null || lModularFactors.Count > tmpFactors.Count)
            {
                lModularFactors = tmpFactors;
                modulus = tmpModulus;
            }

            if (lModularFactors.Count <= 3)
                break;
        }

        IList<UnivariatePolynomial<BigInteger>> modularFactors =
            HenselLifting.LiftFactorization(new BigInteger(modulus), bound2, poly, lModularFactors.Factors);
        return ReconstructFactorsZ(poly,
            PolynomialFactorDecomposition<UnivariatePolynomial<BigInteger>>.Of(modularFactors));
    }


    static PolynomialFactorDecomposition<UnivariatePolynomial<BigInteger>> ConvertFactorizationToBigIntegers<E>(
        PolynomialFactorDecomposition<UnivariatePolynomial<E>> decomposition)
    {
        return decomposition.MapTo(p => p.ToBigPoly());
    }


    private static int ChooseModulusLowerBound(double bound2)
    {
        long infinum;
        if (bound2 < MIGNOTTE_MAX_DOUBLE_32)
        {
            // we can use single 32-bit modulus
            infinum = (long)bound2;
        }
        else if (bound2 < MIGNOTTE_MAX_DOUBLE_64)
        {
            // we still can use machine-size words
            // two options possible:
            // 1) use 64-bit "(long) bound2" as modulus (which is more than 32-bit and thus slow)
            // 2) use 32-bit modulus and perform a single Hensel step
            // we use 2) option
            infinum = (long)Math.Sqrt(bound2);
        }
        else
        {
            // coefficient bound is large -> we anyway need several Hensel steps
            // so we just pick 32-bit prime at random and must use BigInteger arithmetics
            throw new ArgumentException(); //infinum = randomModulusInf();
        }

        return (int)infinum;
    }


    static PolynomialFactorDecomposition<UnivariatePolynomialZ64> FactorSquareFreeInZ0(UnivariatePolynomialZ64 poly)
    {
        var lc = poly.Lc();
        double bound2 = 2 * UnivariatePolynomialZ64.MignotteBound(poly) * Math.Abs(lc);

        // choose prime at random
        var trial32Modulus = ChooseModulusLowerBound(bound2) - 1;
        long modulus;
        UnivariatePolynomialZp64 moduloImage;
        do
        {
            trial32Modulus = Next32BitPrime(trial32Modulus + 1);
            modulus = trial32Modulus;
            moduloImage = poly.Modulus(modulus, true);
        } while (!UnivariateSquareFreeFactorization.IsSquareFree(moduloImage));

        // do Hensel lifting
        // determine number of Hensel steps
        var henselIterations = 0;
        var liftedModulus = modulus;
        while (liftedModulus < bound2)
        {
            if (MachineArithmetic.IsOverflowMultiply(liftedModulus, liftedModulus))
                return null;
            liftedModulus = MachineArithmetic.SafeMultiply(liftedModulus, liftedModulus);
            ++henselIterations;
        }


        // do modular factorization
        var modularFactors = FactorInGF(moduloImage.Monic());

        // actual lift
        if (henselIterations > 0)
            modularFactors = PolynomialFactorDecomposition<UnivariatePolynomialZp64>
                .Of(HenselLifting.LiftFactorization(modulus, liftedModulus, henselIterations, poly,
                    modularFactors.Factors, true)).AddUnit(modularFactors.Unit.SetModulus(liftedModulus));

        //reconstruct true factors
        return ReconstructFactorsZ(poly, modularFactors);
    }

    public static PolynomialFactorDecomposition<UnivariatePolynomial<E>> FactorSquareFreeInZ<E>(
        UnivariatePolynomial<E> poly)
    {
        EnsureIntegersDomain(poly);
        if (poly.Degree() <= 1 || poly.IsMonomial())
            if (poly.IsMonic())
                return PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Of(poly);
            else
            {
                var c = poly.ContentAsPoly();
                return PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Of(c, poly.Clone().DivideByLC(c));
            }

        var content = poly.ContentAsPoly();
        if (poly.SignumOfLC() < 0)
            content = content.Negate();
        return FactorSquareFreeInZ0(poly.Clone().DivideByLC(content)).SetUnit(content);
    }

    private static PolynomialFactorDecomposition<UnivariatePolynomial<E>> FactorSquareFreeInZ0<E>(
        UnivariatePolynomial<E> poly)
    {
        if (poly is UnivariatePolynomialZ64 pZ64)
            return FactorSquareFreeInZ0(pZ64) as PolynomialFactorDecomposition<UnivariatePolynomial<E>>;
        else if (poly is UnivariatePolynomial<BigInteger> pZ)
            return FactorSquareFreeInZ0(pZ) as PolynomialFactorDecomposition<UnivariatePolynomial<E>>;

        throw new NotImplementedException();
    }

    private static void EnsureIntegersDomain<E>(UnivariatePolynomial<E> poly)
    {
        if (poly.ring is Integers || poly.ring is Integers64)
            return;

        throw new ArgumentException("Not an integers ring for factorization in Z[x]");
    }


    public static PolynomialFactorDecomposition<UnivariatePolynomial<E>> FactorInZ<E>(UnivariatePolynomial<E> poly)
    {
        EnsureIntegersDomain(poly);
        if (poly.Degree() <= 1 || poly.IsMonomial())
            if (poly.IsMonic())
                return PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Of(poly);
            else
            {
                var c = poly.ContentAsPoly();
                return PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Of(c, poly.Clone().DivideByLC(c));
            }

        var result = PolynomialFactorDecomposition<UnivariatePolynomial<E>>.Empty(poly);
        var content = poly.ContentAsPoly();
        if (poly.SignumOfLC() < 0)
            content = content.Negate();
        FactorInZ(poly.Clone().DivideByLC(content), result);
        return result.SetUnit(content);
    }

    private static void FactorInZ<E>(UnivariatePolynomial<E> poly,
        PolynomialFactorDecomposition<UnivariatePolynomial<E>> result)
    {
        FactorGeneric(poly, result, UnivariateFactorization.FactorSquareFreeInZ0);
    }

    private static void FactorGeneric<E>(UnivariatePolynomial<E> poly,
        PolynomialFactorDecomposition<UnivariatePolynomial<E>> result,
        Func<UnivariatePolynomial<E>, PolynomialFactorDecomposition<UnivariatePolynomial<E>>> factorSquareFree)
    {
        var @base = FactorOutMonomial(poly);
        if (!@base.monomial.IsConstant())
            result.AddFactor(poly.CreateMonomial(1), @base.monomial.Degree());

        //do square-free factorization
        var sqf = UnivariateSquareFreeFactorization.SquareFreeFactorization(@base.theRest);
        for (var i = 0; i < sqf.Count; ++i)
        {
            //for each square-free factor
            var sqfFactor = sqf[i];
            var sqfExponent = sqf.Exponents[i];

            //do distinct-degree factorization
            var cz = factorSquareFree(sqfFactor);

            //do equal-degree factorization
            foreach (var irreducibleFactor in cz.Factors)

                //put final irreducible factor into the result
                result.AddFactor(irreducibleFactor, sqfExponent);
        }
    }

    /* ======================================== Factorization in Q(alpha)[x] ======================================== */


    public static PolynomialFactorDecomposition<UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>>
        FactorInNumberField(UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> poly)
    {
        if (poly.Degree() <= 1 || poly.IsMonomial())
            return PolynomialFactorDecomposition<UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>>
                .Of(poly);
        var result =
            PolynomialFactorDecomposition<UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>>.Empty(poly);
        FactorInNumberField(poly, result);
        if (result.IsTrivial())
            return PolynomialFactorDecomposition<UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>>
                .Of(poly);

        // correct l.c.
        var numberField =
            (AlgebraicNumberField<Rational<BigInteger>>)poly.ring;
        UnivariatePolynomial<Rational<BigInteger>> unit = result.Unit.Lc();
        for (var i = 0; i < result.Count; i++)
            unit = numberField.Multiply(unit, numberField.Pow(result[i].Lc(), result.Exponents[i]));
        unit = numberField.DivideExact(poly.Lc(), unit);
        result.AddUnit(UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>.Constant(numberField, unit));
        return result;
    }

    private static void FactorInNumberField(UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> poly,
        PolynomialFactorDecomposition<UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>> result)
    {
        FactorGeneric(poly, result, UnivariateFactorization.FactorSquareFreeInNumberField);
    }


    public static PolynomialFactorDecomposition<UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>>
        FactorSquareFreeInNumberField(UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> poly)
    {
        var numberField =
            (AlgebraicNumberField<Rational<BigInteger>>)poly.ring;
        for (var s = 0;; ++s)
        {
            // choose a substitution f(z) -> f(z - s*alpha) so that norm is square-free
            UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> backSubstitution, sPoly;
            if (s == 0)
            {
                backSubstitution = null;
                sPoly = poly;
            }
            else
            {
                sPoly = poly.Composition(poly.CreateMonomial(1).Subtract(numberField.Generator().Multiply(s)));
                backSubstitution = poly.CreateMonomial(1).Add(numberField.Generator().Multiply(s));
            }

            var sPolyNorm = numberField.NormOfPolynomial(sPoly);
            if (!UnivariateSquareFreeFactorization.IsSquareFree(sPolyNorm))
                continue;

            // factorize norm
            var normFactors = Factor(sPolyNorm);
            if (normFactors.IsTrivial())
                return PolynomialFactorDecomposition<UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>>
                    .Of(poly);
            var result =
                PolynomialFactorDecomposition<UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>>
                    .Empty(poly);
            for (var i = 0; i < normFactors.Count; i++)
            {
                UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> factor =
                    UnivariateGCD.PolynomialGCD(sPoly, ToNumberField(numberField, normFactors[i]));
                if (backSubstitution != null)
                    factor = factor.Composition(backSubstitution);
                result.AddFactor(factor, 1);
            }

            return result;
        }
    }

    private static UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> ToNumberField(
        AlgebraicNumberField<Rational<BigInteger>> numberField,
        UnivariatePolynomial<Rational<BigInteger>> poly)
    {
        return poly.MapCoefficients(numberField,
            (cf) => UnivariatePolynomial<Rational<BigInteger>>.Constant(Rings.Q, cf));
    }
}