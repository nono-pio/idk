using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Poly;
using Cc.Redberry.Rings.Poly.Multivar;
using Cc.Redberry.Rings.Primes;
using Cc.Redberry.Rings.Util;

namespace Cc.Redberry.Rings.Poly.Univar
{
    /// <summary>
    /// Factorization of univariate polynomials.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class UnivariateFactorization
    {
        private UnivariateFactorization()
        {
        }

        /// <summary>
        /// Factors univariate {@code poly}.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>factor decomposition</returns>
        public static PolynomialFactorDecomposition<Poly> Factor<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
        {
            if (poly.IsOverFiniteField())
                return FactorInGF(poly);
            else if (poly.IsOverZ())
                return FactorInZ(poly);
            else if (Util.IsOverRationals(poly))
                return FactorInQ((UnivariatePolynomial)poly);
            else if (Util.IsOverSimpleNumberField(poly))
                return (PolynomialFactorDecomposition<Poly>)FactorInNumberField((UnivariatePolynomial)poly);
            else if (Util.IsOverMultipleFieldExtension(poly))
                return (PolynomialFactorDecomposition<Poly>)FactorInMultipleFieldExtension((UnivariatePolynomial)poly);
            else if (IsOverMultivariate(poly))
                return (PolynomialFactorDecomposition<Poly>)FactorOverMultivariate((UnivariatePolynomial)poly, MultivariateFactorization.Factor());
            else if (IsOverUnivariate(poly))
                return (PolynomialFactorDecomposition<Poly>)FactorOverUnivariate((UnivariatePolynomial)poly, MultivariateFactorization.Factor());
            else
                throw new Exception("ring is not supported: " + poly.CoefficientRingToString());
        }

        static bool IsOverMultivariate<T>(T poly) where T : IUnivariatePolynomial<T>
        {
            return (poly is UnivariatePolynomial && ((UnivariatePolynomial)poly).ring is MultivariateRing);
        }

        static bool IsOverUnivariate<T>(T poly) where T : IUnivariatePolynomial<T>
        {
            return (poly is UnivariatePolynomial && ((UnivariatePolynomial)poly).ring is UnivariateRing);
        }

        static PolynomialFactorDecomposition<UnivariatePolynomial<Poly>> FactorOverMultivariate<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(UnivariatePolynomial<Poly> poly, Func<Poly, PolynomialFactorDecomposition<Poly>> factorFunction)
        {
            return factorFunction.Apply(AMultivariatePolynomial.AsMultivariate(poly, 0, true)).MapTo((p) => p.AsUnivariateEliminate(0));
        }

        static PolynomialFactorDecomposition<UnivariatePolynomial<uPoly>> FactorOverUnivariate<uPoly>(UnivariatePolynomial<uPoly> poly, Func<MultivariatePolynomial<uPoly>, PolynomialFactorDecomposition<MultivariatePolynomial<uPoly>>> factorFunction) where uPoly : IUnivariatePolynomial<uPoly>
        {
            return factorFunction(AMultivariatePolynomial.AsMultivariate(poly, 1, 0, MonomialOrder.DEFAULT)).MapTo(m => m.AsUnivariate());
        }

        /// <summary>
        /// Factors polynomial over Q
        /// </summary>
        /// <param name="poly">the polynomial over finite field</param>
        /// <returns>irreducible factor decomposition</returns>
        public static PolynomialFactorDecomposition<UnivariatePolynomial<Rational<E>>> FactorInQ<E>(UnivariatePolynomial<Rational<E>> poly)
        {
            (UnivariatePolynomial<E>, E) cmd = Util.ToCommonDenominator(poly);
            UnivariatePolynomial<E> integral = cmd.Item1;
            E denominator = cmd.Item2;
            return Factor(integral).MapTo((p) => Util.AsOverRationals(poly.ring, p)).AddUnit(poly.CreateConstant(new Rational<E>(integral.ring, integral.ring.GetOne(), denominator)));
        }

        private static PolynomialFactorDecomposition<UnivariatePolynomial<mPoly>> FactorInMultipleFieldExtension<Term extends AMonomial<Term>, mPoly extends AMultivariatePolynomial<Term, mPoly>, sPoly extends IUnivariatePolynomial<sPoly>>(UnivariatePolynomial<mPoly> poly)
        {
            MultipleFieldExtension<Term, mPoly, sPoly> ring = (MultipleFieldExtension<Term, mPoly, sPoly>)poly.ring;
            SimpleFieldExtension<sPoly> simpleExtension = ring.GetSimpleExtension();
            return Factor(poly.MapCoefficients(simpleExtension, ring.Inverse())).MapTo((p) => p.MapCoefficients(ring, ring.Image()));
        }

        /// <summary>
        /// x^n * poly
        /// </summary>
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

        /// <summary>
        /// factor out common monomial term (x^n)
        /// </summary>
        private static FactorMonomial<Poly> FactorOutMonomial<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
        {
            int i = poly.FirstNonZeroCoefficientPosition();
            if (i == 0)
                return new FactorMonomial<Poly>(poly, poly.CreateOne());
            return new FactorMonomial<Poly>(poly.Clone().ShiftLeft(i), poly.CreateMonomial(i));
        }

        /// <summary>
        /// early check for trivial cases
        /// </summary>
        private static PolynomialFactorDecomposition<Poly> EarlyFactorizationChecks<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
        {
            if (poly.Degree() <= 1 || poly.IsMonomial())
                return PolynomialFactorDecomposition<Poly>.Of(poly.LcAsPoly(), poly.IsMonic() ? poly : poly.Clone().Monic());
            return null;
        }

        /* =========================================== Factorization in Zp[x] =========================================== */
        /// <summary>
        /// Factors polynomial over finite field
        /// </summary>
        /// <param name="poly">the polynomial over finite field</param>
        /// <returns>irreducible factor decomposition</returns>
        /// <remarks>
        /// @seeUnivariateSquareFreeFactorization
        /// @seeDistinctDegreeFactorization
        /// @seeEqualDegreeFactorization
        /// </remarks>
        public static PolynomialFactorDecomposition<Poly> FactorInGF<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
        {
            Util.EnsureOverFiniteField(poly);
            if (CanConvertToZp64(poly))
                return FactorInGF(AsOverZp64(poly)).MapTo(Conversions64bit.Convert());
            PolynomialFactorDecomposition<Poly> result = EarlyFactorizationChecks(poly);
            if (result != null)
                return result;
            result = PolynomialFactorDecomposition<Poly>.Empty(poly);
            FactorInGF(poly, result);
            return result.SetUnit(poly.LcAsPoly());
        }

        /// <summary>
        /// Factors square-free polynomial over finite field
        /// </summary>
        /// <param name="poly">the square-free polynomial over finite field</param>
        /// <returns>irreducible factor decomposition</returns>
        /// <remarks>
        /// @seeDistinctDegreeFactorization
        /// @seeEqualDegreeFactorization
        /// </remarks>
        public static PolynomialFactorDecomposition<T> FactorSquareFreeInGF<T>(T poly) where T : IUnivariatePolynomial<T>
        {
            Util.EnsureOverFiniteField(poly);
            if (CanConvertToZp64(poly))
                return FactorInGF(AsOverZp64(poly)).MapTo(Conversions64bit.Convert());
            PolynomialFactorDecomposition<T> result = PolynomialFactorDecomposition.Empty(poly);
            FactorSquareFreeInGF(poly, 1, result);
            return result;
        }

        private static void FactorSquareFreeInGF<T>(T poly, int exponent, PolynomialFactorDecomposition<T> result) where T :IUnivariatePolynomial<T>
        {

            //do distinct-degree factorization
            PolynomialFactorDecomposition<T> ddf = DistinctDegreeFactorization.DistinctDegreeFactorization(poly);

            //assertDistinctDegreeFactorization(sqfFactor, ddf);
            for (int j = 0; j < ddf.Count; ++j)
            {

                //for each distinct-degree factor
                T ddfFactor = ddf[j];
                int ddfExponent = ddf.GetExponent(j);

                //do equal-degree factorization
                PolynomialFactorDecomposition<T> edf = CantorZassenhaus(ddfFactor, ddfExponent);
                foreach (T irreducibleFactor in edf.factors)

                    //put final irreducible factor into the result
                    result.AddFactor(irreducibleFactor.Monic(), exponent);
            }
        }

        private static void FactorInGF<T>(T poly, PolynomialFactorDecomposition<T> result) where T :IUnivariatePolynomial<T>
        {
            FactorMonomial<T> @base = FactorOutMonomial(poly);
            if (!@base.monomial.IsConstant())
                result.AddFactor(poly.CreateMonomial(1), @base.monomial.Degree());

            //do square-free factorization
            PolynomialFactorDecomposition<T> sqf = SquareFreeFactorization(@base.theRest);

            //assert sqf.toPolynomial().equals(base.theRest) : base.toString();
            for (int i = 0; i < sqf.Count; ++i)
            {

                //for each square-free factor
                T sqfFactor = sqf[i];
                int sqfExponent = sqf.GetExponent(i);
                FactorSquareFreeInGF(sqfFactor, sqfExponent, result);
            }
        }

        private static void AssertDistinctDegreeFactorization<T>(T poly, PolynomialFactorDecomposition<T> factorization) where T :IUnivariatePolynomial<T>
        {
        }

        /* =========================================== Factorization in Z[x] =========================================== */
        /// <summary>
        /// assertion for correct Hensel structure
        /// </summary>
        static void AssertHenselLift<T>(HenselLifting.QuadraticLiftAbstract<T> lift) where T : IUnivariatePolynomial<T>
        {
        }

        /// <summary>
        /// cache of references *
        /// </summary>
        private static int[][] naturalSequenceRefCache = new int[32][];
        private static int[] CreateSeq(int n)
        {
            int[] r = new int[n];
            for (int i = 0; i < n; i++)
                r[i] = i;
            return r;
        }

        /// <summary>
        /// returns sequence of natural numbers
        /// </summary>
        private static int[] NaturalSequenceRef(int n)
        {
            if (n >= naturalSequenceRefCache.Length)
                return CreateSeq(n);
            if (naturalSequenceRefCache[n] != null)
                return naturalSequenceRefCache[n];
            return naturalSequenceRefCache[n] = CreateSeq(n);
        }

        /// <summary>
        /// select elements by their positions
        /// </summary>
        private static int[] Select(int[] data, int[] positions)
        {
            int[] r = new int[positions.Length];
            int i = 0;
            foreach (int p in positions)
                r[i++] = data[p];
            return r;
        }

        /// <summary>
        /// reconstruct true factors by enumerating all combinations
        /// </summary>
        static PolynomialFactorDecomposition<UnivariatePolynomialZ64> ReconstructFactorsZ(UnivariatePolynomialZ64 poly, PolynomialFactorDecomposition<UnivariatePolynomialZp64> modularFactors)
        {
            if (modularFactors.IsTrivial())
                return PolynomialFactorDecomposition<UnivariatePolynomialZ64>.Of(poly);
            UnivariatePolynomialZp64 factory = modularFactors[0];
            int[] modIndexes = NaturalSequenceRef(modularFactors.Count);
            PolynomialFactorDecomposition<UnivariatePolynomialZ64> trueFactors = PolynomialFactorDecomposition<UnivariatePolynomialZ64>.Empty(poly);
            UnivariatePolynomialZ64 fRest = poly;
            int s = 1;
            factor_combinations:
                while (2 * s <= modIndexes.Length)
                {
                    foreach (int[] combination in Combinatorics.Combinations(modIndexes.Length, s))
                    {
                        int[] indexes = Select(modIndexes, combination);
                        UnivariatePolynomialZp64 mFactor = factory.CreateConstant(fRest.Lc());
                        foreach (int i in indexes)
                            mFactor = mFactor.Multiply(modularFactors[i]);
                        UnivariatePolynomialZ64 factor = mFactor.AsPolyZSymmetric().PrimitivePart();
                        if (fRest.Lc() % factor.Lc() != 0 || fRest.Cc() % factor.Cc() != 0)
                            continue;
                        UnivariatePolynomialZp64 mRest = factory.CreateConstant(fRest.Lc() / factor.Lc());
                        int[] restIndexes = ArraysUtil.IntSetDifference(modIndexes, indexes);
                        foreach (int i in restIndexes)
                            mRest = mRest.Multiply(modularFactors[i]);
                        UnivariatePolynomialZ64 rest = mRest.AsPolyZSymmetric().PrimitivePart();
                        if (MachineArithmetic.SafeMultiply(factor.Lc(), rest.Lc()) != fRest.Lc() || MachineArithmetic.SafeMultiply(factor.Cc(), rest.Cc()) != fRest.Cc())
                            continue;
                        if (rest.Clone().MultiplyUnsafe(factor).Equals(fRest))
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

        /// <summary>
        /// reconstruct true factors by enumerating all combinations
        /// </summary>
        static PolynomialFactorDecomposition<UnivariatePolynomial<BigInteger>> ReconstructFactorsZ(UnivariatePolynomial<BigInteger> poly, PolynomialFactorDecomposition<UnivariatePolynomial<BigInteger>> modularFactors)
        {
            if (modularFactors.IsTrivial())
                return PolynomialFactorDecomposition<UnivariatePolynomial<BigInteger>>.Of(poly);
            UnivariatePolynomial<BigInteger> factory = modularFactors[0];
            int[] modIndexes = NaturalSequenceRef(modularFactors.Count);
            PolynomialFactorDecomposition<UnivariatePolynomial<BigInteger>> trueFactors = PolynomialFactorDecomposition<UnivariatePolynomial<BigInteger>>.Empty(poly);
            UnivariatePolynomial<BigInteger> fRest = poly;
            int s = 1;
            factor_combinations:
                while (2 * s <= modIndexes.Length)
                {
                    foreach (int[] combination in Combinatorics.Combinations(modIndexes.Length, s))
                    {
                        int[] indexes = Select(modIndexes, combination);
                        UnivariatePolynomial<BigInteger> mFactor = factory.CreateConstant(fRest.Lc());
                        foreach (int i in indexes)
                            mFactor = mFactor.Multiply(modularFactors[i]);
                        UnivariatePolynomial<BigInteger> factor = UnivariatePolynomial<BigInteger>.AsPolyZSymmetric(mFactor).PrimitivePart();
                        if (!fRest.Lc().Remainder(factor.Lc()).IsZero || !fRest.Cc().Remainder(factor.Cc()).IsZero)
                            continue;
                        UnivariatePolynomial<BigInteger> mRest = factory.CreateConstant(fRest.Lc().Divide(factor.Lc()));
                        int[] restIndexes = ArraysUtil.IntSetDifference(modIndexes, indexes);
                        foreach (int i in restIndexes)
                            mRest = mRest.Multiply(modularFactors[i]);
                        UnivariatePolynomial<BigInteger> rest = UnivariatePolynomial<BigInteger>.AsPolyZSymmetric(mRest).PrimitivePart();
                        if (!factor.Lc().Multiply(rest.Lc()).Equals(fRest.Lc()) || !factor.Cc().Multiply(rest.Cc()).Equals(fRest.Cc()))
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

        private static readonly double MAX_PRIME_GAP = 382, MIGNOTTE_MAX_DOUBLE_32 = (2.0 * int.MaxValue) - 10 * MAX_PRIME_GAP, MIGNOTTE_MAX_DOUBLE_64 = MIGNOTTE_MAX_DOUBLE_32 * MIGNOTTE_MAX_DOUBLE_32;
        private static readonly int LOWER_RND_MODULUS_BOUND = 1 << 24, UPPER_RND_MODULUS_BOUND = 1 << 30;
        private static int RandomModulusInf()
        {
            return LOWER_RND_MODULUS_BOUND + PrivateRandom.GetRandom().Next(UPPER_RND_MODULUS_BOUND - LOWER_RND_MODULUS_BOUND);
        }

        private static int Next32BitPrime(int val)
        {
            if (val < 0)
            {
                long l = BigPrimes.NextPrime(int.ToUnsignedLong(val));
                return (int)l;
            }
            else
                return SmallPrimes.NextPrime(val);
        }

        static readonly int N_MIN_MODULAR_FACTORIZATION_TRIALS = 2, N_SIMPLE_MOD_PATTERN_FACTORS = 12, N_MODULAR_FACTORIZATION_TRIALS = 4, N_TOO_MUCH_FACTORS_COUNT = 22, N_MODULAR_FACTORIZATION_TRIALS_TOO_MUCH_FACTORS = 12; // maximal number of modular trials if there are too many factors
        /// <summary>
        /// Factors primitive square-free polynomial using Hensel lifting
        /// </summary>
        /// <param name="poly">Z[x] primitive square-free polynomial</param>
        static PolynomialFactorDecomposition<UnivariatePolynomial<BigInteger>> FactorSquareFreeInZ0(UnivariatePolynomial<BigInteger> poly)
        {
            BigInteger bound2 = new BigInteger(2).Multiply(UnivariatePolynomial<BigInteger>.MignotteBound(poly)).Multiply(poly.Lc().Abs());
            if (bound2.CompareTo(MachineArithmetic.b_MAX_SUPPORTED_MODULUS) < 0)
            {
                PolynomialFactorDecomposition<UnivariatePolynomialZ64> tryLong = FactorSquareFreeInZ0(UnivariatePolynomial<BigInteger>.AsOverZ64(poly));
                if (tryLong != null)
                    return ConvertFactorizationToBigIntegers(tryLong);
            }


            // choose prime at random
            long modulus = -1;
            UnivariatePolynomialZp64 moduloImage;
            PolynomialFactorDecomposition<UnivariatePolynomialZp64> lModularFactors = null;
            for (int attempt = 0;; attempt++)
            {
                if (attempt >= N_MIN_MODULAR_FACTORIZATION_TRIALS && lModularFactors.Count <= N_SIMPLE_MOD_PATTERN_FACTORS)
                    break;
                if (attempt >= N_MODULAR_FACTORIZATION_TRIALS)
                    if (lModularFactors.Count < N_TOO_MUCH_FACTORS_COUNT || attempt >= N_MODULAR_FACTORIZATION_TRIALS_TOO_MUCH_FACTORS)
                        break;
                long tmpModulus;
                do
                {
                    tmpModulus = SmallPrimes.NextPrime(RandomModulusInf());
                    moduloImage = UnivariatePolynomial<BigInteger>.AsOverZp64(poly.SetRing(new IntegersZp(tmpModulus)));
                }
                while (moduloImage.Cc() == 0 || moduloImage.Degree() != poly.Degree() || !UnivariateSquareFreeFactorization.IsSquareFree(moduloImage));

                // do modular factorization
                PolynomialFactorDecomposition<UnivariatePolynomialZp64> tmpFactors = FactorInGF(moduloImage.Monic());
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

            IList<UnivariatePolynomial<BigInteger>> modularFactors = HenselLifting.LiftFactorization(new BigInteger(modulus), bound2, poly, lModularFactors.factors);
            return ReconstructFactorsZ(poly, PolynomialFactorDecomposition<UnivariatePolynomial<BigInteger>>.Of(modularFactors));
        }

        /// <summary>
        /// machine integers -> BigIntegers
        /// </summary>
        static PolynomialFactorDecomposition<UnivariatePolynomial<BigInteger>> ConvertFactorizationToBigIntegers<T>(PolynomialFactorDecomposition<T> decomposition) where T : AUnivariatePolynomial64<T>
        {
            return decomposition.MapTo(p => p.ToBigPoly());
        }

        /// <summary>
        /// determines the lower bound for the possible modulus for Zp trials
        /// </summary>
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

        /// <summary>
        /// Factors square-free polynomial using Hensel lifting
        /// </summary>
        /// <param name="poly">Z[x] square-free polynomial</param>
        static PolynomialFactorDecomposition<UnivariatePolynomialZ64> FactorSquareFreeInZ0(UnivariatePolynomialZ64 poly)
        {
            long lc = poly.Lc();
            double bound2 = 2 * poly.MignotteBound() * Math.Abs(lc);

            // choose prime at random
            int trial32Modulus = ChooseModulusLowerBound(bound2) - 1;
            long modulus;
            UnivariatePolynomialZp64 moduloImage;
            do
            {
                trial32Modulus = Next32BitPrime(trial32Modulus + 1);
                modulus = int.ToUnsignedLong(trial32Modulus);
                moduloImage = poly.Modulus(modulus, true);
            }
            while (!UnivariateSquareFreeFactorization.IsSquareFree(moduloImage));

            // do Hensel lifting
            // determine number of Hensel steps
            int henselIterations = 0;
            long liftedModulus = modulus;
            while (liftedModulus < bound2)
            {
                if (MachineArithmetic.IsOverflowMultiply(liftedModulus, liftedModulus))
                    return null;
                liftedModulus = MachineArithmetic.SafeMultiply(liftedModulus, liftedModulus);
                ++henselIterations;
            }


            // do modular factorization
            PolynomialFactorDecomposition<UnivariatePolynomialZp64> modularFactors = FactorInGF(moduloImage.Monic());

            // actual lift
            if (henselIterations > 0)
                modularFactors = PolynomialFactorDecomposition<UnivariatePolynomialZp64>.Of(HenselLifting.LiftFactorization(modulus, liftedModulus, henselIterations, poly, modularFactors.factors, true)).AddUnit(modularFactors.unit.SetModulus(liftedModulus));

            //reconstruct true factors
            return ReconstructFactorsZ(poly, modularFactors);
        }

        public static PolynomialFactorDecomposition<PolyZ> FactorSquareFreeInZ<PolyZ>(PolyZ poly) where PolyZ : IUnivariatePolynomial<PolyZ>
        {
            EnsureIntegersDomain(poly);
            if (poly.Degree() <= 1 || poly.IsMonomial())
                if (poly.IsMonic())
                    return PolynomialFactorDecomposition<PolyZ>.Of(poly);
                else
                {
                    PolyZ c = poly.ContentAsPoly();
                    return PolynomialFactorDecomposition<PolyZ>.Of(c, poly.Clone().DivideByLC(c));
                }

            PolyZ content = poly.ContentAsPoly();
            if (poly.SignumOfLC() < 0)
                content = content.Negate();
            return FactorSquareFreeInZ0(poly.Clone().DivideByLC(content)).SetUnit(content);
        }

        private static PolynomialFactorDecomposition<PolyZ> FactorSquareFreeInZ0<PolyZ>(PolyZ poly) where PolyZ : IUnivariatePolynomial<PolyZ>
        {
            if (poly is UnivariatePolynomialZ64)
                return (PolynomialFactorDecomposition<PolyZ>)FactorSquareFreeInZ0((UnivariatePolynomialZ64)poly);
            else
                return (PolynomialFactorDecomposition<PolyZ>)FactorSquareFreeInZ0((UnivariatePolynomial)poly);
        }

        private static void EnsureIntegersDomain<PolyZ>(IUnivariatePolynomial<PolyZ> poly) where PolyZ : IUnivariatePolynomial<PolyZ>
        {
            if (poly is UnivariatePolynomialZ64 || (poly is UnivariatePolynomial<PolyZ> && ((UnivariatePolynomial<PolyZ>)poly).ring.Equals(Rings.Z)))
                return;
            throw new ArgumentException("Not an integers ring for factorization in Z[x]");
        }

        /// <summary>
        /// Factors polynomial in Z[x].
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>factor decomposition</returns>
        /// <remarks>
        /// @see#FactorInGF(IUnivariatePolynomial)
        /// @seeHenselLifting
        /// </remarks>
        public static PolynomialFactorDecomposition<Poly> FactorInZ<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
        {
            EnsureIntegersDomain(poly);
            if (poly.Degree() <= 1 || poly.IsMonomial())
                if (poly.IsMonic())
                    return PolynomialFactorDecomposition<Poly>.Of(poly);
                else
                {
                    Poly c = poly.ContentAsPoly();
                    return PolynomialFactorDecomposition<Poly>.Of(c, poly.Clone().DivideByLC(c));
                }

            PolynomialFactorDecomposition<Poly> result = PolynomialFactorDecomposition<Poly>.Empty(poly);
            Poly content = poly.ContentAsPoly();
            if (poly.SignumOfLC() < 0)
                content = content.Negate();
            FactorInZ(poly.Clone().DivideByLC(content), result);
            return result.SetUnit(content);
        }

        private static void FactorInZ<T>(T poly, PolynomialFactorDecomposition<T> result) where T :IUnivariatePolynomial<T>
        {
            FactorGeneric(poly, result, UnivariateFactorization.FactorSquareFreeInZ0);
        }

        private static void FactorGeneric<T>(T poly, PolynomialFactorDecomposition<T> result, Func<T, PolynomialFactorDecomposition<T>> factorSquareFree) where T :IUnivariatePolynomial<T>
        {
            FactorMonomial<T> @base = FactorOutMonomial(poly);
            if (!@base.monomial.IsConstant())
                result.AddFactor(poly.CreateMonomial(1), @base.monomial.Degree());

            //do square-free factorization
            PolynomialFactorDecomposition<T> sqf = SquareFreeFactorization(@base.theRest);
            for (int i = 0; i < sqf.Count; ++i)
            {

                //for each square-free factor
                T sqfFactor = sqf[i];
                int sqfExponent = sqf.GetExponent(i);

                //do distinct-degree factorization
                PolynomialFactorDecomposition<T> cz = factorSquareFree(sqfFactor);

                //do equal-degree factorization
                foreach (T irreducibleFactor in cz.factors)

                    //put final irreducible factor into the result
                    result.AddFactor(irreducibleFactor, sqfExponent);
            }
        }

        /* ======================================== Factorization in Q(alpha)[x] ======================================== */
        /// <summary>
        /// Factors polynomial in Q(alpha)[x] via Trager's algorithm
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>factor decomposition</returns>
        /// <remarks>
        /// @see#FactorInGF(IUnivariatePolynomial)
        /// @seeHenselLifting
        /// </remarks>
        public static PolynomialFactorDecomposition<UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>> FactorInNumberField(UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> poly)
        {
            if (poly.Degree() <= 1 || poly.IsMonomial())
                return PolynomialFactorDecomposition<UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>>.Of(poly);
            PolynomialFactorDecomposition<UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>> result = PolynomialFactorDecomposition<UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>>.Empty(poly);
            FactorInNumberField(poly, result);
            if (result.IsTrivial())
                return PolynomialFactorDecomposition<UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>>.Of(poly);

            // correct l.c.
            AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> numberField = (AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>>)poly.ring;
            UnivariatePolynomial<Rational<BigInteger>> unit = result.unit.Lc();
            for (int i = 0; i < result.Count; i++)
                unit = numberField.Multiply(unit, numberField.Pow(result[i].Lc(), result.GetExponent(i)));
            unit = numberField.DivideExact(poly.Lc(), unit);
            result.AddUnit(UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>.Constant(numberField, unit));
            return result;
        }

        private static void FactorInNumberField(UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> poly, PolynomialFactorDecomposition<UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>> result)
        {
            FactorGeneric(poly, result, UnivariateFactorization.FactorSquareFreeInNumberField);
        }

        /// <summary>
        /// Factors polynomial in Q(alpha)[x] via Trager's algorithm
        /// </summary>
        public static PolynomialFactorDecomposition<UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>> FactorSquareFreeInNumberField(UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> poly)
        {
            AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> numberField = (AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>>)poly.ring;
            for (int s = 0;; ++s)
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

                UnivariatePolynomial<Rational<BigInteger>> sPolyNorm = numberField.NormOfPolynomial(sPoly);
                if (!UnivariateSquareFreeFactorization.IsSquareFree(sPolyNorm))
                    continue;

                // factorize norm
                PolynomialFactorDecomposition<UnivariatePolynomial<Rational<BigInteger>>> normFactors = Factor(sPolyNorm);
                if (normFactors.IsTrivial())
                    return PolynomialFactorDecomposition<UnivariatePolynomial<Rational<BigInteger>>>.Of(poly);
                PolynomialFactorDecomposition<UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>> result = PolynomialFactorDecomposition<UnivariatePolynomial<Rational<BigInteger>>>.Empty(poly);
                for (int i = 0; i < normFactors.Count; i++)
                {
                    UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> factor = UnivariateGCD.PolynomialGCD(sPoly, ToNumberField(numberField, normFactors[i]));
                    if (backSubstitution != null)
                        factor = factor.Composition(backSubstitution);
                    result.AddFactor(factor, 1);
                }

                return result;
            }
        }

        private static UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> ToNumberField(AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> numberField, UnivariatePolynomial<Rational<BigInteger>> poly)
        {
            return poly.MapCoefficients(numberField, (cf) => UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>.Constant(Rings.Q, cf));
        }
    }
}