using System.Numerics;
using Cc.Redberry.Rings.Bigint;


namespace Cc.Redberry.Rings.Poly.Univar
{
    /// <summary>
    /// Methods for univariate Hensel lifting.
    /// 
    /// <p> <i>Implementation notes.</i> Two methods for Hensel lift are implemented: quadratic and linear. For {@code N}
    /// iterations quadratic lift will lift to p<sup>2^N</sup> while linear just to p<sup>N</sup>. While quadratic lift
    /// converges much faster, it works with BigIntegers in all intermediate steps, so each step is quite expensive. Linear
    /// lift is implemented so that it starts with machine-word modulus, and perform all hard intermediate calculations with
    /// machine-word arithmetic, converting to BigIntegers only a few times. In this way, a single step of linear lift is
    /// very cheap, but the convergence is worse. The actual lifting used in factorization switches between linear and
    /// quadratic lift in order to obtain the best trade-off.
    /// 
    /// NOTE: Quadratic lifts may fail in Z/2
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class HenselLifting
    {
        private HenselLifting()
        {
        }

        /// <summary>
        /// Liftable quintet. Output specifications is as follows:
        /// 
        /// <p>
        /// <pre>
        /// polyMod = aFactor * bFactor mod modulus
        /// 1 = aFactor * aCoFactor + bFactor * bCoFactor mod modulus
        /// </pre>
        /// where {@coode modulus} is the modulus obtained by lifting
        /// </summary>
        /// <param name="<PolyZp>">Zp[x] polynomial type</param>
        public interface LiftableQuintet<PolyZp>
        {
            /// <summary>
            /// Returns initial Z[x] polynomial modulo lifted modulus
            /// </summary>
            /// <returns>initial Z[x] polynomial modulo lifted modulus</returns>
            PolyZp PolyMod();
            /// <summary>
            /// Returns initial Z[x] polynomial modulo lifted modulus
            /// </summary>
            /// <returns>initial Z[x] polynomial modulo lifted modulus</returns>
            /// <summary>
            /// Returns first factor lifted
            /// </summary>
            /// <returns>first factor lifted</returns>
            PolyZp AFactorMod();
            /// <summary>
            /// Returns initial Z[x] polynomial modulo lifted modulus
            /// </summary>
            /// <returns>initial Z[x] polynomial modulo lifted modulus</returns>
            /// <summary>
            /// Returns first factor lifted
            /// </summary>
            /// <returns>first factor lifted</returns>
            /// <summary>
            /// Returns second factor lifted
            /// </summary>
            /// <returns>second factor lifted</returns>
            PolyZp BFactorMod();
            /// <summary>
            /// Returns initial Z[x] polynomial modulo lifted modulus
            /// </summary>
            /// <returns>initial Z[x] polynomial modulo lifted modulus</returns>
            /// <summary>
            /// Returns first factor lifted
            /// </summary>
            /// <returns>first factor lifted</returns>
            /// <summary>
            /// Returns second factor lifted
            /// </summary>
            /// <returns>second factor lifted</returns>
            /// <summary>
            /// Returns first co-factor lifted
            /// </summary>
            /// <returns>first co-factor lifted</returns>
            PolyZp ACoFactorMod();
            /// <summary>
            /// Returns initial Z[x] polynomial modulo lifted modulus
            /// </summary>
            /// <returns>initial Z[x] polynomial modulo lifted modulus</returns>
            /// <summary>
            /// Returns first factor lifted
            /// </summary>
            /// <returns>first factor lifted</returns>
            /// <summary>
            /// Returns second factor lifted
            /// </summary>
            /// <returns>second factor lifted</returns>
            /// <summary>
            /// Returns first co-factor lifted
            /// </summary>
            /// <returns>first co-factor lifted</returns>
            /// <summary>
            /// Returns second co-factor lifted
            /// </summary>
            /// <returns>second co-factor lifted</returns>
            PolyZp BCoFactorMod();
            /// <summary>
            /// Returns initial Z[x] polynomial modulo lifted modulus
            /// </summary>
            /// <returns>initial Z[x] polynomial modulo lifted modulus</returns>
            /// <summary>
            /// Returns first factor lifted
            /// </summary>
            /// <returns>first factor lifted</returns>
            /// <summary>
            /// Returns second factor lifted
            /// </summary>
            /// <returns>second factor lifted</returns>
            /// <summary>
            /// Returns first co-factor lifted
            /// </summary>
            /// <returns>first co-factor lifted</returns>
            /// <summary>
            /// Returns second co-factor lifted
            /// </summary>
            /// <returns>second co-factor lifted</returns>
            /// <summary>
            /// Performs single lift step.
            /// </summary>
            void Lift();
            /// <summary>
            /// Returns initial Z[x] polynomial modulo lifted modulus
            /// </summary>
            /// <returns>initial Z[x] polynomial modulo lifted modulus</returns>
            /// <summary>
            /// Returns first factor lifted
            /// </summary>
            /// <returns>first factor lifted</returns>
            /// <summary>
            /// Returns second factor lifted
            /// </summary>
            /// <returns>second factor lifted</returns>
            /// <summary>
            /// Returns first co-factor lifted
            /// </summary>
            /// <returns>first co-factor lifted</returns>
            /// <summary>
            /// Returns second co-factor lifted
            /// </summary>
            /// <returns>second co-factor lifted</returns>
            /// <summary>
            /// Performs single lift step.
            /// </summary>
            /// <summary>
            /// Performs single lift step but don't lift co-factors (xgcd coefficients).
            /// </summary>
            void LiftLast();
            /// <summary>
            /// Returns initial Z[x] polynomial modulo lifted modulus
            /// </summary>
            /// <returns>initial Z[x] polynomial modulo lifted modulus</returns>
            /// <summary>
            /// Returns first factor lifted
            /// </summary>
            /// <returns>first factor lifted</returns>
            /// <summary>
            /// Returns second factor lifted
            /// </summary>
            /// <returns>second factor lifted</returns>
            /// <summary>
            /// Returns first co-factor lifted
            /// </summary>
            /// <returns>first co-factor lifted</returns>
            /// <summary>
            /// Returns second co-factor lifted
            /// </summary>
            /// <returns>second co-factor lifted</returns>
            /// <summary>
            /// Performs single lift step.
            /// </summary>
            /// <summary>
            /// Performs single lift step but don't lift co-factors (xgcd coefficients).
            /// </summary>
            /// <summary>
            /// Lifts {@code nIterations} times. Co-factor will be lost on the last step.
            /// </summary>
            /// <param name="nIterations">number of lift iterations</param>
            void Lift(int nIterations)
            {
                for (int i = 0; i < nIterations - 1; ++i)
                    Lift();
                LiftLast();
            }

   
            /// <summary>
            /// Lifts {@code nIterations} times.
            /// </summary>
            /// <param name="nIterations">number of lift iterations</param>
            void LiftWithCoFactors(int nIterations)
            {
                for (int i = 0; i < nIterations; ++i)
                    Lift();
            }
        }

        /* ************************************ Factory methods ************************************ */
        /// <summary>
        /// Creates quadratic Hensel lift.
        /// </summary>
        /// <param name="modulus">the initial modulus</param>
        /// <param name="poly">Z[x] polynomial</param>
        /// <param name="aFactor">first factor of {@code poly} that {@code poly = aFactor * bFactor mod modulus}</param>
        /// <param name="bFactor">second factor of {@code poly} that {@code poly = aFactor * bFactor mod modulus}</param>
        /// <returns>quadratic Hensel lift</returns>
        public static lQuadraticLift CreateQuadraticLift(long modulus, UnivariatePolynomialZ64 poly, UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 bFactor)
        {
            bFactor = EnsureMonic(bFactor);
            if (aFactor.ring.Modulus(poly.Lc()) != aFactor.Lc())
                aFactor = aFactor.Clone().Monic(poly.Lc());
            UnivariatePolynomialZp64[] xgcd = MonicExtendedEuclid(aFactor, bFactor);
            return new lQuadraticLift(modulus, poly, aFactor, bFactor, xgcd[1], xgcd[2]);
        }

        private static void EnsureIntegersDomain(UnivariatePolynomial<BigInteger> poly)
        {
            if (!poly.ring.Equals(Rings.Z))
                throw new ArgumentException("Not an integers ring ring: " + poly.ring);
        }

        private static void EnsureModularDomain(UnivariatePolynomial<BigInteger> poly)
        {
            if (!(poly.ring is IntegersZp))
                throw new ArgumentException("Not a modular ring");
        }

        private static void EnsureInputCorrect(UnivariatePolynomial<BigInteger> poly, UnivariatePolynomial<BigInteger> aFactor, UnivariatePolynomial<BigInteger> bFactor)
        {
            EnsureIntegersDomain(poly);
            EnsureModularDomain(aFactor);
            EnsureModularDomain(bFactor);
        }

        /// <summary>
        /// Creates quadratic Hensel lift.
        /// </summary>
        /// <param name="modulus">the initial modulus</param>
        /// <param name="poly">Z[x] polynomial</param>
        /// <param name="aFactor">first factor of {@code poly} that {@code poly = aFactor * bFactor mod modulus}</param>
        /// <param name="bFactor">second factor of {@code poly} that {@code poly = aFactor * bFactor mod modulus}</param>
        /// <returns>quadratic Hensel lift</returns>
        public static bQuadraticLift CreateQuadraticLift(BigInteger modulus, UnivariatePolynomial<BigInteger> poly, UnivariatePolynomial<BigInteger> aFactor, UnivariatePolynomial<BigInteger> bFactor)
        {
            EnsureInputCorrect(poly, aFactor, bFactor);
            bFactor = EnsureMonic(bFactor);
            IntegersZp ring = (IntegersZp)aFactor.ring;
            if (!ring.ValueOf(poly.Lc()).Equals(aFactor.Lc()))
                aFactor = aFactor.Clone().Monic(poly.Lc());
            UnivariatePolynomial<BigInteger>[] xgcd = MonicExtendedEuclid(aFactor, bFactor);
            return new bQuadraticLift(modulus, poly, aFactor, bFactor, xgcd[1], xgcd[2]);
        }

        /// <summary>
        /// Creates quadratic Hensel lift.
        /// </summary>
        /// <param name="modulus">the initial modulus</param>
        /// <param name="poly">Z[x] polynomial</param>
        /// <param name="aFactor">first factor of {@code poly} that {@code poly = aFactor * bFactor mod modulus}</param>
        /// <param name="bFactor">second factor of {@code poly} that {@code poly = aFactor * bFactor mod modulus}</param>
        /// <returns>quadratic Hensel lift</returns>
        public static bQuadraticLift CreateQuadraticLift(BigInteger modulus, UnivariatePolynomial<BigInteger> poly, UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 bFactor)
        {
            EnsureIntegersDomain(poly);
            bFactor = EnsureMonic(bFactor);
            long lc = poly.Lc().Mod(modulus).LongValueExact();
            if (lc != aFactor.Lc())
                aFactor = aFactor.Clone().Monic(lc);
            UnivariatePolynomialZp64[] xgcd = MonicExtendedEuclid(aFactor, bFactor);
            return new bQuadraticLift(modulus, poly, aFactor.ToBigPoly(), bFactor.ToBigPoly(), xgcd[1].ToBigPoly(), xgcd[2].ToBigPoly());
        }

        /// <summary>
        /// Creates linear Hensel lift.
        /// </summary>
        /// <param name="modulus">the initial modulus</param>
        /// <param name="poly">Z[x] polynomial</param>
        /// <param name="aFactor">first factor of {@code poly} that {@code poly = aFactor * bFactor mod modulus}</param>
        /// <param name="bFactor">second factor of {@code poly} that {@code poly = aFactor * bFactor mod modulus}</param>
        /// <returns>linear Hensel lift</returns>
        public static lLinearLift CreateLinearLift(BigInteger modulus, UnivariatePolynomialZ64 poly, UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 bFactor)
        {
            return CreateLinearLift(modulus.LongValueExact(), poly, aFactor, bFactor);
        }

        /// <summary>
        /// Creates linear Hensel lift.
        /// </summary>
        /// <param name="modulus">the initial modulus</param>
        /// <param name="poly">Z[x] polynomial</param>
        /// <param name="aFactor">first factor of {@code poly} that {@code poly = aFactor * bFactor mod modulus}</param>
        /// <param name="bFactor">second factor of {@code poly} that {@code poly = aFactor * bFactor mod modulus}</param>
        /// <returns>linear Hensel lift</returns>
        public static bLinearLift CreateLinearLift(BigInteger modulus, UnivariatePolynomial<BigInteger> poly, UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 bFactor)
        {
            return CreateLinearLift(modulus.LongValueExact(), poly, aFactor, bFactor);
        }

        /// <summary>
        /// Creates linear Hensel lift.
        /// </summary>
        /// <param name="modulus">the initial modulus</param>
        /// <param name="poly">Z[x] polynomial</param>
        /// <param name="aFactor">first factor of {@code poly} that {@code poly = aFactor * bFactor mod modulus}</param>
        /// <param name="bFactor">second factor of {@code poly} that {@code poly = aFactor * bFactor mod modulus}</param>
        /// <returns>linear Hensel lift</returns>
        public static lLinearLift CreateLinearLift(long modulus, UnivariatePolynomialZ64 poly, UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 bFactor)
        {
            bFactor = EnsureMonic(bFactor);
            if (aFactor.ring.Modulus(poly.Lc()) != aFactor.Lc())
                aFactor = aFactor.Clone().Monic(poly.Lc());
            UnivariatePolynomialZp64[] xgcd = MonicExtendedEuclid(aFactor, bFactor);
            return new lLinearLift(modulus, poly, aFactor, bFactor, xgcd[1], xgcd[2]);
        }

        /// <summary>
        /// Creates linear Hensel lift.
        /// </summary>
        /// <param name="modulus">the initial modulus</param>
        /// <param name="poly">Z[x] polynomial</param>
        /// <param name="aFactor">first factor of {@code poly} that {@code poly = aFactor * bFactor mod modulus}</param>
        /// <param name="bFactor">second factor of {@code poly} that {@code poly = aFactor * bFactor mod modulus}</param>
        /// <returns>linear Hensel lift</returns>
        public static bLinearLift CreateLinearLift(long modulus, UnivariatePolynomial<BigInteger> poly, UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 bFactor)
        {
            EnsureIntegersDomain(poly);
            BigInteger bModulus = new BigInteger(modulus);
            bFactor = EnsureMonic(bFactor);
            long lc = poly.Lc().Mod(bModulus).LongValueExact();
            if (lc != aFactor.Lc())
                aFactor = aFactor.Clone().Monic(lc);
            UnivariatePolynomialZp64[] xgcd = MonicExtendedEuclid(aFactor, bFactor);
            return new bLinearLift(bModulus, poly, aFactor, bFactor, xgcd[1], xgcd[2]);
        }

        /// <summary>
        /// runs xgcd for coprime polynomials ensuring that gcd is 1 (not another constant)
        /// </summary>
        private static PolyZp[] MonicExtendedEuclid<PolyZp>(PolyZp a, PolyZp b) where PolyZp : IUnivariatePolynomial<PolyZp>
        {
            PolyZp[] xgcd = UnivariateGCD.PolynomialExtendedGCD(a, b);
            if (xgcd[0].IsOne())
                return xgcd;

            //normalize: x * a + y * b = 1
            xgcd[2].DivideByLC(xgcd[0]);
            xgcd[1].DivideByLC(xgcd[0]);
            xgcd[0].Monic();
            return xgcd;
        }

        private static PolyZp EnsureMonic<PolyZp>(PolyZp p) where PolyZp : IUnivariatePolynomial<PolyZp>
        {
            return p.IsMonic() ? p : p.Clone().Monic();
        }

        private static long[] NIterations(long modulus, long desiredBound, bool quadratic)
        {
            int nIterations = 0;
            long tmp = modulus;
            while (tmp < desiredBound)
            {
                tmp = MachineArithmetic.SafeMultiply(tmp, quadratic ? tmp : modulus);
                ++nIterations;
            }

            return new long[]
            {
                nIterations,
                tmp
            };
        }

        /// <summary>
        /// Lifts modular factorization until {@code modulus} will overcome {@code desiredBound}.
        /// </summary>
        /// <param name="modulus">initial modulus so that {@code modularFactors} are true factors of {@code poly mod
        ///                       modulus}</param>
        /// <param name="desiredBound">desired modulus</param>
        /// <param name="poly">initial Z[x] polynomial</param>
        /// <param name="modularFactors">factorization of {@code poly.modulus(modulus)}</param>
        /// <param name="quadratic">whether to use quadratic of linear lift</param>
        /// <returns>factorization of {@code poly.modulus(finalModulus) } with some {@code finalModulus} greater than {@code
        /// desiredBound}</returns>
        public static List<UnivariatePolynomialZp64> LiftFactorization(long modulus, long desiredBound, UnivariatePolynomialZ64 poly, List<UnivariatePolynomialZp64> modularFactors, bool quadratic)
        {
            long[] im = NIterations(modulus, desiredBound, quadratic);
            return LiftFactorization(modulus, im[1], (int)im[0], poly, modularFactors, quadratic);
        }

        /// <summary>
        /// Lifts modular factorization {@code nIterations} times using whether linear or quadratic lifting.
        /// </summary>
        /// <param name="modulus">initial modulus so that {@code modularFactors} are true factors of {@code poly mod
        ///                       modulus}</param>
        /// <param name="finalModulus">final modulus that will be obtained after lifting</param>
        /// <param name="nIterations">number of lifting steps to do</param>
        /// <param name="poly">initial Z[x] polynomial</param>
        /// <param name="modularFactors">factorization of {@code poly.modulus(modulus)}</param>
        /// <param name="quadratic">whether to use quadratic of linear lift</param>
        /// <returns>factorization of {@code poly.modulus(finalModulus) }</returns>
        public static List<UnivariatePolynomialZp64> LiftFactorization(long modulus, long finalModulus, int nIterations, UnivariatePolynomialZ64 poly, List<UnivariatePolynomialZp64> modularFactors, bool quadratic)
        {

            // for the future:
            // recursion may be replaced with precomputed binary tree
            // for now the major part of execution time (~99%) is spent in actual lifting step, so irrelevant
            if (modularFactors.Count == 1)
                return [poly.Modulus(finalModulus, true).Monic()];
            UnivariatePolynomialZp64 factory = modularFactors[0];
            UnivariatePolynomialZp64 aFactor = factory.CreateConstant(poly.Lc()), bFactor = factory.CreateOne();
            int nHalf = modularFactors.Count / 2, i = 0;
            for (; i < nHalf; ++i)
                aFactor = aFactor.Multiply(modularFactors[i]);
            for (; i < modularFactors.Count; ++i)
                bFactor = bFactor.Multiply(modularFactors[i]);
            LiftableQuintet<UnivariatePolynomialZp64> hensel = quadratic ? CreateQuadraticLift(modulus, poly, aFactor, bFactor) : CreateLinearLift(modulus, poly, aFactor, bFactor);
            hensel.Lift(nIterations);
            UnivariatePolynomialZp64 aFactorRaised = hensel.AFactorMod();
            UnivariatePolynomialZp64 bFactorRaised = hensel.BFactorMod();
            List<UnivariatePolynomialZp64> result = [];
            result.AddRange(LiftFactorization(modulus, finalModulus, nIterations, aFactorRaised.AsPolyZSymmetric(), modularFactors.SubList(0, nHalf), quadratic));
            result.AddRange(LiftFactorization(modulus, finalModulus, nIterations, bFactorRaised.AsPolyZSymmetric(), modularFactors.SubList(nHalf, modularFactors.Count), quadratic));
            return result;
        }

        interface LiftFactory<PolyZp>
        {
            LiftableQuintet<UnivariatePolynomial<BigInteger>> CreateLift(BigInteger modulus, UnivariatePolynomial<BigInteger> polyZ, PolyZp aFactor, PolyZp bFactor);
        }

        /// <summary>
        /// actual multifactor Hensel lifting implementation *
        /// </summary>
        static List<UnivariatePolynomial<BigInteger>> LiftFactorization0<PolyZp>(BigInteger modulus, BigInteger finalModulus, int nIterations, UnivariatePolynomial<BigInteger> poly, List<PolyZp> modularFactors, LiftFactory<PolyZp> liftFactory) where PolyZp : IUnivariatePolynomial<PolyZp>
        {

            // for the future:
            // recursion may be replaced with precomputed binary tree
            // for now the major part of execution time (~99%) is spent in actual lifting step, so irrelevant
            if (modularFactors.Count == 1)
                return [poly.SetRing(new IntegersZp(finalModulus)).Monic()];
            PolyZp factory = modularFactors[0];
            PolyZp aFactor = factory.CreateOne(), bFactor = factory.CreateOne();
            int nHalf = modularFactors.Count / 2, i = 0;
            for (; i < nHalf; ++i)
                aFactor = aFactor.Multiply(modularFactors[i]);
            for (; i < modularFactors.Count; ++i)
                bFactor = bFactor.Multiply(modularFactors[i]);
            LiftableQuintet<UnivariatePolynomial<BigInteger>> hensel = liftFactory.CreateLift(modulus, poly, aFactor, bFactor);
            hensel.Lift(nIterations);
            UnivariatePolynomial<BigInteger> aFactorRaised = hensel.AFactorMod();
            UnivariatePolynomial<BigInteger> bFactorRaised = hensel.BFactorMod();
            List<UnivariatePolynomial<BigInteger>> result = [];
            result.AddRange(LiftFactorization0(modulus, finalModulus, nIterations, UnivariatePolynomial.AsPolyZSymmetric(aFactorRaised), modularFactors.SubList(0, nHalf), liftFactory));
            result.AddRange(LiftFactorization0(modulus, finalModulus, nIterations, UnivariatePolynomial.AsPolyZSymmetric(bFactorRaised), modularFactors.SubList(nHalf, modularFactors.Count), liftFactory));
            return result;
        }

        sealed class LiftingInfo
        {
            public readonly int nIterations;
            public readonly BigInteger finalModulus;

            public LiftingInfo(int nIterations, BigInteger finalModulus)
            {
                this.nIterations = nIterations;
                this.finalModulus = finalModulus;
            }
        }

        static LiftingInfo NIterations(BigInteger modulus, BigInteger desiredBound, bool quadratic)
        {
            int nIterations = 0;
            BigInteger finalModulus = modulus;
            while (finalModulus.CompareTo(desiredBound) < 0)
            {
                finalModulus = finalModulus.Multiply(quadratic ? finalModulus : modulus);
                ++nIterations;
            }

            return new LiftingInfo(nIterations, finalModulus);
        }

        /// <summary>
        /// Lifts modular factorization until {@code modulus} will overcome {@code desiredBound}. <b>Note:</b> if {@code
        /// quadratic == false} modulus must fit 64-bit.
        /// </summary>
        /// <param name="modulus">initial modulus so that {@code modularFactors} are true factors of {@code poly mod
        ///                       modulus}</param>
        /// <param name="desiredBound">desired modulus</param>
        /// <param name="poly">initial Z[x] polynomial</param>
        /// <param name="modularFactors">factorization of {@code poly.modulus(modulus)}</param>
        /// <param name="quadratic">whether to use quadratic of linear lift</param>
        /// <returns>factorization of {@code poly.modulus(finalModulus) } with some {@code finalModulus} greater than {@code
        /// desiredBound}</returns>
        public static List<UnivariatePolynomial<BigInteger>> LiftFactorization(BigInteger modulus, BigInteger desiredBound, UnivariatePolynomial<BigInteger> poly, List<UnivariatePolynomialZp64> modularFactors, bool quadratic)
        {
            if (!quadratic && !modulus.IsLong())
                throw new ArgumentException("Only max 64-bit modulus for linear lift allowed.");
            LiftingInfo im = NIterations(modulus, desiredBound, quadratic);
            if (im.nIterations == 0)
                return modularFactors.Select(f => f.ToBigPoly()).ToList();
            LiftFactory<UnivariatePolynomialZp64> factory = quadratic ? HenselLifting.CreateQuadraticLift() : HenselLifting.CreateLinearLift();
            return LiftFactorization0(modulus, im.finalModulus, im.nIterations, poly, modularFactors, factory);
        }

        /// <summary>
        /// Lifts modular factorization until {@code modulus} will overcome {@code desiredBound}.
        /// </summary>
        /// <param name="modulus">initial modulus so that {@code modularFactors} are true factors of {@code poly mod
        ///                       modulus}</param>
        /// <param name="desiredBound">desired modulus</param>
        /// <param name="poly">initial Z[x] polynomial</param>
        /// <param name="modularFactors">factorization of {@code poly.modulus(modulus)}</param>
        /// <returns>factorization of {@code poly.modulus(finalModulus) } with some {@code finalModulus} greater than {@code
        /// desiredBound}</returns>
        public static List<UnivariatePolynomial<BigInteger>> LiftFactorizationQuadratic(BigInteger modulus, BigInteger desiredBound, UnivariatePolynomial<BigInteger> poly, List<UnivariatePolynomial<BigInteger>> modularFactors)
        {
            LiftingInfo im = NIterations(modulus, desiredBound, true);
            return LiftFactorization0(modulus, im.finalModulus, im.nIterations, poly, modularFactors, HenselLifting.CreateQuadraticLift());
        }

        /// <summary>
        /// Lifts modular factorization until {@code modulus} will overcome {@code desiredBound}. <i>Implementation note:</i>
        /// method will switch between linear and quadratic lift depending on the required lifting iterations.
        /// </summary>
        /// <param name="modulus">initial modulus so that {@code modularFactors} are true factors of {@code poly mod
        ///                       modulus}</param>
        /// <param name="desiredBound">desired modulus</param>
        /// <param name="poly">initial Z[x] polynomial</param>
        /// <param name="modularFactors">factorization of {@code poly.modulus(modulus)}</param>
        /// <returns>factorization of {@code poly.modulus(finalModulus) } with some {@code finalModulus} greater than {@code
        /// desiredBound}</returns>
        public static List<UnivariatePolynomial<BigInteger>> LiftFactorization(BigInteger modulus, BigInteger desiredBound, UnivariatePolynomial<BigInteger> poly, List<UnivariatePolynomialZp64> modularFactors)
        {
            return LiftFactorization(poly, modularFactors, new AdaptiveLift(modulus, desiredBound));
        }

        /// <summary>
        /// actual multifactor Hensel lifting implementation *
        /// </summary>
        private static List<UnivariatePolynomial<BigInteger>> LiftFactorization(UnivariatePolynomial<BigInteger> poly, List<UnivariatePolynomialZp64> modularFactors, AdaptiveLift lifter)
        {

            // for the future:
            // recursion may be replaced with precomputed binary tree
            // for now the major part of execution time (~99%) is spent in actual lifting step, so irrelevant
            if (modularFactors.Count == 1)
                return [poly.SetRing(new IntegersZp(lifter.finalModulus)).Monic()];
            UnivariatePolynomialZp64 factory = modularFactors[0];
            UnivariatePolynomialZp64 aFactor = factory.CreateOne(), bFactor = factory.CreateOne();
            int nHalf = modularFactors.Count / 2, i = 0;
            for (; i < nHalf; ++i)
                aFactor = aFactor.Multiply(modularFactors[i]);
            for (; i < modularFactors.Count; ++i)
                bFactor = bFactor.Multiply(modularFactors[i]);
            UnivariatePolynomial<BigInteger>[] lifted = lifter.Lift(poly, aFactor, bFactor);
            UnivariatePolynomial<BigInteger> aFactorRaised = lifted[0];
            UnivariatePolynomial<BigInteger> bFactorRaised = lifted[1];
            List<UnivariatePolynomial<BigInteger>> result = [];
            result.AddRange(LiftFactorization(UnivariatePolynomial.AsPolyZSymmetric(aFactorRaised), modularFactors.SubList(0, nHalf), lifter));
            result.AddRange(LiftFactorization(UnivariatePolynomial.AsPolyZSymmetric(bFactorRaised), modularFactors.SubList(nHalf, modularFactors.Count), lifter));
            return result;
        }

        private static readonly int SWITCH_TO_QUADRATIC_LIFT = 64;
        private sealed class AdaptiveLift
        {
            readonly BigInteger initialModulus;
            readonly BigInteger finalModulus;
            readonly int nLinearIterations, nQuadraticIterations;
            public AdaptiveLift(BigInteger initialModulus, BigInteger desiredBound)
            {
                this.initialModulus = initialModulus;
                LiftingInfo nLinearIterations = NIterations(initialModulus, desiredBound, false);
                if (nLinearIterations.nIterations < SWITCH_TO_QUADRATIC_LIFT)
                {
                    this.nLinearIterations = nLinearIterations.nIterations;
                    this.nQuadraticIterations = -1;
                    this.finalModulus = nLinearIterations.finalModulus;
                }
                else
                {
                    LiftingInfo nQuadraticIterations = NIterations(initialModulus, desiredBound, true);
                    this.nLinearIterations = -1;
                    this.nQuadraticIterations = nQuadraticIterations.nIterations;
                    this.finalModulus = nQuadraticIterations.finalModulus;
                }
            }

            public UnivariatePolynomial<BigInteger>[] Lift(UnivariatePolynomial<BigInteger> poly, UnivariatePolynomialZp64 a, UnivariatePolynomialZp64 b)
            {
                bool quadratic = nLinearIterations == -1;
                LiftableQuintet<UnivariatePolynomial<BigInteger>> lift = quadratic ? CreateQuadraticLift(initialModulus, poly, a.ToBigPoly(), b.ToBigPoly()) : CreateLinearLift(initialModulus, poly, a, b);
                lift.Lift(quadratic ? nQuadraticIterations : nLinearIterations);
                return new UnivariatePolynomial<BigInteger>[]
                {
                    lift.AFactorMod(),
                    lift.BFactorMod()
                };
            }
        }

        private static void AssertHenselLift<T>(LiftableQuintet<T> lift) where T : IUnivariatePolynomial<T>
        {
        }

        /* ************************************ Quadratic lifts ************************************ */
        /// <summary>
        /// data used in Hensel lifting *
        /// </summary>
        public abstract class QuadraticLiftAbstract<PolyZp> : LiftableQuintet<PolyZp>
        {
            /// <summary>
            /// Two factors of the initial Z[x] poly *
            /// </summary>
            protected PolyZp aFactor, bFactor;
            /// <summary>
            /// Two factors of the initial Z[x] poly *
            /// </summary>
            /// <summary>
            /// xgcd coefficients *
            /// </summary>
            protected PolyZp aCoFactor, bCoFactor;
            /// <summary>
            /// Two factors of the initial Z[x] poly *
            /// </summary>
            /// <summary>
            /// xgcd coefficients *
            /// </summary>
            public QuadraticLiftAbstract(PolyZp aFactor, PolyZp bFactor, PolyZp aCoFactor, PolyZp bCoFactor)
            {
                this.aFactor = aFactor;
                this.bFactor = bFactor;
                this.aCoFactor = aCoFactor;
                this.bCoFactor = bCoFactor;
            }

            /// <summary>
            /// Two factors of the initial Z[x] poly *
            /// </summary>
            /// <summary>
            /// xgcd coefficients *
            /// </summary>
            public virtual PolyZp AFactorMod()
            {
                return aFactor;
            }

            /// <summary>
            /// Two factors of the initial Z[x] poly *
            /// </summary>
            /// <summary>
            /// xgcd coefficients *
            /// </summary>
            public virtual PolyZp BFactorMod()
            {
                return bFactor;
            }

            /// <summary>
            /// Two factors of the initial Z[x] poly *
            /// </summary>
            /// <summary>
            /// xgcd coefficients *
            /// </summary>
            public virtual PolyZp ACoFactorMod()
            {
                return aCoFactor;
            }

            /// <summary>
            /// Two factors of the initial Z[x] poly *
            /// </summary>
            /// <summary>
            /// xgcd coefficients *
            /// </summary>
            public virtual PolyZp BCoFactorMod()
            {
                return bCoFactor;
            }

            /// <summary>
            /// Two factors of the initial Z[x] poly *
            /// </summary>
            /// <summary>
            /// xgcd coefficients *
            /// </summary>
            public abstract void Prepare();
            /// <summary>
            /// Two factors of the initial Z[x] poly *
            /// </summary>
            /// <summary>
            /// xgcd coefficients *
            /// </summary>
            public void Lift()
            {
                Prepare();
                HenselStep0(PolyMod());
            }

            /// <summary>
            /// Two factors of the initial Z[x] poly *
            /// </summary>
            /// <summary>
            /// xgcd coefficients *
            /// </summary>
            public void LiftLast()
            {
                Prepare();
                HenselLastStep0(PolyMod());
            }

            /// <summary>
            /// Two factors of the initial Z[x] poly *
            /// </summary>
            /// <summary>
            /// xgcd coefficients *
            /// </summary>
            private void HenselStep0(PolyZp baseMod)
            {
                PolyZp e = baseMod.Subtract(aFactor.Clone().Multiply(bFactor));
                PolyZp[] qr = UnivariateDivision.DivideAndRemainder(aCoFactor.Clone().Multiply(e), bFactor, false);
                PolyZp q = qr[0], r = qr[1];
                PolyZp aFactorNew = aFactor.Clone().Add(bCoFactor.Clone().Multiply(e)).Add(aFactor.Clone().Multiply(q));
                PolyZp bFactorNew = bFactor.Clone().Add(r);
                PolyZp b = aCoFactor.Clone().Multiply(aFactorNew).Add(bCoFactor.Clone().Multiply(bFactorNew)).Decrement();
                PolyZp[] cd = UnivariateDivision.DivideAndRemainder(aCoFactor.Clone().Multiply(b), bFactorNew, false);
                PolyZp c = cd[0], d = cd[1];
                PolyZp aCoFactorNew = aCoFactor.Subtract(d);
                PolyZp bCoFactorNew = bCoFactor.Subtract(bCoFactor.Clone().Multiply(b)).Subtract(c.Clone().Multiply(aFactorNew));
                aFactor = aFactorNew;
                aCoFactor = aCoFactorNew;
                bFactor = bFactorNew;
                bCoFactor = bCoFactorNew;
            }

            /// <summary>
            /// Two factors of the initial Z[x] poly *
            /// </summary>
            /// <summary>
            /// xgcd coefficients *
            /// </summary>
            private void HenselLastStep0(PolyZp baseMod)
            {
                PolyZp e = baseMod.Subtract(aFactor.Clone().Multiply(bFactor));
                PolyZp[] qr = UnivariateDivision.DivideAndRemainder(aCoFactor.Multiply(e), bFactor, false);
                PolyZp q = qr[0], r = qr[1];
                PolyZp aFactorNew = aFactor.Add(bCoFactor.Multiply(e)).Add(aFactor.Clone().Multiply(q));
                PolyZp bFactorNew = bFactor.Add(r);
                aFactor = aFactorNew;
                aCoFactor = null;
                bFactor = bFactorNew;
                bCoFactor = null;
            }
        }

        /// <summary>
        /// Quadratic Hensel lift for machine word arithmetics. On each {@link #lift()} operation modulus is raised as {@code
        /// modulus = modulus * modulus}.
        /// </summary>
        public sealed class lQuadraticLift : QuadraticLiftAbstract<UnivariatePolynomialZp64>
        {
            /// <summary>
            /// The modulus
            /// </summary>
            public long modulus;
            /// <summary>
            /// The modulus
            /// </summary>
            /// <summary>
            /// Initial Z[x] poly *
            /// </summary>
            public readonly UnivariatePolynomialZ64 @base;
            /// <summary>
            /// The modulus
            /// </summary>
            /// <summary>
            /// Initial Z[x] poly *
            /// </summary>
            public lQuadraticLift(long modulus, UnivariatePolynomialZ64 @base, UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 bFactor, UnivariatePolynomialZp64 aCoFactor, UnivariatePolynomialZp64 bCoFactor) : base(aFactor, bFactor, aCoFactor, bCoFactor)
            {
                this.modulus = modulus;
                this.@base = @base;
            }

            /// <summary>
            /// The modulus
            /// </summary>
            /// <summary>
            /// Initial Z[x] poly *
            /// </summary>
            public override UnivariatePolynomialZp64 PolyMod()
            {
                return @base.Modulus(modulus, true);
            }

            /// <summary>
            /// The modulus
            /// </summary>
            /// <summary>
            /// Initial Z[x] poly *
            /// </summary>
            public override void Prepare()
            {
                modulus = MachineArithmetic.SafeMultiply(modulus, modulus);
                aFactor = aFactor.SetModulusUnsafe(modulus);
                bFactor = bFactor.SetModulusUnsafe(modulus);
                aCoFactor = aCoFactor.SetModulusUnsafe(modulus);
                bCoFactor = bCoFactor.SetModulusUnsafe(modulus);
            }
        }

        /// <summary>
        /// Quadratic Hensel lift for BigIntegers arithmetics. On each {@link #lift()} operation modulus is raised as {@code
        /// modulus = modulus * modulus}.
        /// </summary>
        public sealed class bQuadraticLift : QuadraticLiftAbstract<UnivariatePolynomial<BigInteger>>
        {
            /// <summary>
            /// The modulus
            /// </summary>
            public IntegersZp ring;
            /// <summary>
            /// The modulus
            /// </summary>
            /// <summary>
            /// Initial Z[x] poly *
            /// </summary>
            public readonly UnivariatePolynomial<BigInteger> @base;
            /// <summary>
            /// The modulus
            /// </summary>
            /// <summary>
            /// Initial Z[x] poly *
            /// </summary>
            public bQuadraticLift(BigInteger modulus, UnivariatePolynomial<BigInteger> @base, UnivariatePolynomial<BigInteger> aFactor, UnivariatePolynomial<BigInteger> bFactor, UnivariatePolynomial<BigInteger> aCoFactor, UnivariatePolynomial<BigInteger> bCoFactor) : base(aFactor, bFactor, aCoFactor, bCoFactor)
            {
                this.ring = new IntegersZp(modulus);
                this.@base = @base;
            }

            /// <summary>
            /// The modulus
            /// </summary>
            /// <summary>
            /// Initial Z[x] poly *
            /// </summary>
            public override UnivariatePolynomial<BigInteger> PolyMod()
            {
                return @base.SetRing(ring);
            }

            /// <summary>
            /// The modulus
            /// </summary>
            /// <summary>
            /// Initial Z[x] poly *
            /// </summary>
            public override void Prepare()
            {
                ring = new IntegersZp(ring.modulus.Multiply(ring.modulus));
                aFactor = aFactor.SetRingUnsafe(ring);
                bFactor = bFactor.SetRingUnsafe(ring);
                aCoFactor = aCoFactor.SetRingUnsafe(ring);
                bCoFactor = bCoFactor.SetRingUnsafe(ring);
            }
        }

        /* ************************************ Linear lifts ************************************ */
        public class LinearLiftAbstract<PolyZ>
        {
            /// <summary>
            /// initial Z[x] poly
            /// </summary>
            public readonly PolyZ poly;

            /// <summary>
            /// initial Z[x] poly
            /// </summary>
            /// <summary>
            /// lifted polynomials
            /// </summary>
            public PolyZ aFactor;

            /// <summary>
            /// initial Z[x] poly
            /// </summary>
            /// <summary>
            /// lifted polynomials
            /// </summary>
            public PolyZ bFactor;

            /// <summary>
            /// initial Z[x] poly
            /// </summary>
            /// <summary>
            /// lifted polynomials
            /// </summary>
            public PolyZ aCoFactor;

            /// <summary>
            /// initial Z[x] poly
            /// </summary>
            /// <summary>
            /// lifted polynomials
            /// </summary>
            public PolyZ bCoFactor;
            /// <summary>
            /// initial Z[x] poly
            /// </summary>
            /// <summary>
            /// lifted polynomials
            /// </summary>
            /// <summary>
            /// initial modular data
            /// </summary>
            readonly UnivariatePolynomialZp64 aFactorMod, aFactorModMonic, bFactorMod, aCoFactorMod, bCoFactorMod;
            /// <summary>
            /// initial Z[x] poly
            /// </summary>
            /// <summary>
            /// lifted polynomials
            /// </summary>
            /// <summary>
            /// initial modular data
            /// </summary>
            /// <summary>
            /// precomputed inverses
            /// </summary>
            readonly UnivariateDivision.InverseModMonomial<UnivariatePolynomialZp64> aFactorModMonicInv, bFactorModInv;
            /// <summary>
            /// initial Z[x] poly
            /// </summary>
            /// <summary>
            /// lifted polynomials
            /// </summary>
            /// <summary>
            /// initial modular data
            /// </summary>
            /// <summary>
            /// precomputed inverses
            /// </summary>
            public LinearLiftAbstract(PolyZ poly, PolyZ aFactor, PolyZ bFactor, PolyZ aCoFactor, PolyZ bCoFactor, UnivariatePolynomialZp64 aFactorMod, UnivariatePolynomialZp64 aFactorModMonic, UnivariatePolynomialZp64 bFactorMod, UnivariatePolynomialZp64 aCoFactorMod, UnivariatePolynomialZp64 bCoFactorMod)
            {
                this.poly = poly;
                this.aFactor = aFactor;
                this.bFactor = bFactor;
                this.aCoFactor = aCoFactor;
                this.bCoFactor = bCoFactor;
                this.aFactorMod = aFactorMod;
                this.aFactorModMonic = aFactorModMonic;
                this.bFactorMod = bFactorMod;
                this.aCoFactorMod = aCoFactorMod;
                this.bCoFactorMod = bCoFactorMod;
                this.aFactorModMonicInv = UnivariateDivision.FastDivisionPreConditioning(aFactorModMonic);
                this.bFactorModInv = UnivariateDivision.FastDivisionPreConditioning(bFactorMod);
            }

            /// <summary>
            /// initial Z[x] poly
            /// </summary>
            /// <summary>
            /// lifted polynomials
            /// </summary>
            /// <summary>
            /// initial modular data
            /// </summary>
            /// <summary>
            /// precomputed inverses
            /// </summary>
            protected UnivariatePolynomialZp64 aAdd, bAdd;
            /// <summary>
            /// initial Z[x] poly
            /// </summary>
            /// <summary>
            /// lifted polynomials
            /// </summary>
            /// <summary>
            /// initial modular data
            /// </summary>
            /// <summary>
            /// precomputed inverses
            /// </summary>
            public void CalculateFactorsDiff(UnivariatePolynomialZp64 diff)
            {
                aAdd = diff.Clone();
                aAdd = UnivariatePolynomialArithmetic.PolyMod(aAdd, aFactorModMonic, aFactorModMonicInv, false);
                aAdd = aAdd.Multiply(bCoFactorMod);
                aAdd = UnivariatePolynomialArithmetic.PolyMod(aAdd, aFactorModMonic, aFactorModMonicInv, false);
                bAdd = diff.Clone();
                bAdd = UnivariatePolynomialArithmetic.PolyMod(bAdd, bFactorMod, bFactorModInv, false);
                bAdd = bAdd.Multiply(aCoFactorMod);
                bAdd = UnivariatePolynomialArithmetic.PolyMod(bAdd, bFactorMod, bFactorModInv, false);
            }

            /// <summary>
            /// initial Z[x] poly
            /// </summary>
            /// <summary>
            /// lifted polynomials
            /// </summary>
            /// <summary>
            /// initial modular data
            /// </summary>
            /// <summary>
            /// precomputed inverses
            /// </summary>
            public void CalculateCoFactorsDiff(UnivariatePolynomialZp64 diff)
            {
                aAdd = diff.Clone();
                aAdd = UnivariatePolynomialArithmetic.PolyMod(aAdd, bFactorMod, bFactorModInv, false);
                aAdd = aAdd.Multiply(aCoFactorMod);
                aAdd = UnivariatePolynomialArithmetic.PolyMod(aAdd, bFactorMod, bFactorModInv, false);
                bAdd = diff.Clone();
                bAdd = UnivariatePolynomialArithmetic.PolyMod(bAdd, aFactorModMonic, aFactorModMonicInv, false);
                bAdd = bAdd.Multiply(bCoFactorMod);
                bAdd = UnivariatePolynomialArithmetic.PolyMod(bAdd, aFactorModMonic, aFactorModMonicInv, false);
            }
        }

        /// <summary>
        /// Linear Hensel lift for machine word arithmetics. Linear Hensel lift always starts from the machine-sized modulus;
        /// on each {@link #lift()} operation modulus is raised as {@code modulus = modulus * initialModulus}.
        /// </summary>
        public sealed class lLinearLift : LinearLiftAbstract<UnivariatePolynomialZ64>, LiftableQuintet<UnivariatePolynomialZp64>
        {
            /// <summary>
            /// The initial modulus
            /// </summary>
            public readonly long initialModulus;
            /// <summary>
            /// The initial modulus
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            public long modulus;
            /// <summary>
            /// The initial modulus
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            private lLinearLift(long modulus, UnivariatePolynomialZ64 poly, UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 aFactorMonic, UnivariatePolynomialZp64 bFactor, UnivariatePolynomialZp64 aCoFactor, UnivariatePolynomialZp64 bCoFactor) : base(poly, EnsureMonic(aFactor).AsPolyZ(true).Multiply(poly.Lc()), bFactor.AsPolyZ(true), aCoFactor.AsPolyZ(true), bCoFactor.AsPolyZ(true), aFactor, aFactorMonic, bFactor, aCoFactor, bCoFactor)
            {
                this.initialModulus = modulus;
                this.modulus = modulus;
            }

            /// <summary>
            /// The initial modulus
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            private lLinearLift(long modulus, UnivariatePolynomialZ64 poly, UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 bFactor, UnivariatePolynomialZp64 aCoFactor, UnivariatePolynomialZp64 bCoFactor) : this(modulus, poly, aFactor, aFactor.Clone().Monic(), bFactor, aCoFactor, bCoFactor)
            {
            }

            /// <summary>
            /// The initial modulus
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            public override UnivariatePolynomialZp64 PolyMod()
            {
                return poly.Modulus(modulus);
            }

            /// <summary>
            /// The initial modulus
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            public override UnivariatePolynomialZp64 AFactorMod()
            {
                return aFactor.Modulus(modulus);
            }

            /// <summary>
            /// The initial modulus
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            public override UnivariatePolynomialZp64 BFactorMod()
            {
                return bFactor.Modulus(modulus);
            }

            /// <summary>
            /// The initial modulus
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            public override UnivariatePolynomialZp64 ACoFactorMod()
            {
                return aCoFactor == null ? null : aCoFactor.Modulus(modulus);
            }

            /// <summary>
            /// The initial modulus
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            public override UnivariatePolynomialZp64 BCoFactorMod()
            {
                return bCoFactor == null ? null : bCoFactor.Modulus(modulus);
            }

            /// <summary>
            /// The initial modulus
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            private void LiftFactors()
            {
                UnivariatePolynomialZp64 factorsDiff = poly.Clone().Subtract(aFactor.Clone().Multiply(bFactor)).DivideOrNull(modulus).Modulus(initialModulus);
                CalculateFactorsDiff(factorsDiff);
                aFactor = aFactor.Add(aAdd.AsPolyZ(false).Multiply(modulus));
                bFactor = bFactor.Add(bAdd.AsPolyZ(false).Multiply(modulus));
            }

            /// <summary>
            /// The initial modulus
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            private void LiftCoFactors()
            {
                UnivariatePolynomialZp64 coFactorsDiff = aCoFactor.Clone().Multiply(aFactor).Add(bCoFactor.Clone().Multiply(bFactor)).Decrement().Negate().DivideOrNull(modulus).Modulus(initialModulus);
                CalculateCoFactorsDiff(coFactorsDiff);
                aCoFactor = aCoFactor.Add(aAdd.AsPolyZ(false).Multiply(modulus));
                bCoFactor = bCoFactor.Add(bAdd.AsPolyZ(false).Multiply(modulus));
            }

            /// <summary>
            /// The initial modulus
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            public override void Lift()
            {
                LiftFactors();
                LiftCoFactors();
                modulus = MachineArithmetic.SafeMultiply(modulus, initialModulus);
            }

            /// <summary>
            /// The initial modulus
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            public override void LiftLast()
            {
                LiftFactors();
                modulus = MachineArithmetic.SafeMultiply(modulus, initialModulus);
                aCoFactor = bCoFactor = null;
            }
        }

        /// <summary>
        /// Linear Hensel lift for BigIntegers arithmetics. Linear Hensel lift always starts from the machine-sized modulus;
        /// on each {@link #lift()} operation modulus is raised as {@code modulus = modulus * initialModulus}.
        /// </summary>
        public sealed class bLinearLift : LinearLiftAbstract<UnivariatePolynomial<BigInteger>>, LiftableQuintet<UnivariatePolynomial<BigInteger>>
        {
            /// <summary>
            /// The initial modulus (less than 64-bit)
            /// </summary>
            public readonly IntegersZp initialDomain;
            /// <summary>
            /// The initial modulus (less than 64-bit)
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            public IntegersZp ring;
            /// <summary>
            /// The initial modulus (less than 64-bit)
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            private bLinearLift(BigInteger modulus, UnivariatePolynomial<BigInteger> poly, UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 aFactorMonic, UnivariatePolynomialZp64 bFactor, UnivariatePolynomialZp64 aCoFactor, UnivariatePolynomialZp64 bCoFactor) : base(poly, EnsureMonic(aFactor).AsPolyZ(true).ToBigPoly().Multiply(poly.Lc()), bFactor.AsPolyZ(false).ToBigPoly(), aCoFactor.AsPolyZ(false).ToBigPoly(), bCoFactor.AsPolyZ(false).ToBigPoly(), aFactor, aFactorMonic, bFactor, aCoFactor, bCoFactor)
            {
                this.initialDomain = new IntegersZp(modulus);
                this.ring = new IntegersZp(modulus);
            }

            /// <summary>
            /// The initial modulus (less than 64-bit)
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            private bLinearLift(BigInteger modulus, UnivariatePolynomial<BigInteger> poly, UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 bFactor, UnivariatePolynomialZp64 aCoFactor, UnivariatePolynomialZp64 bCoFactor) : this(modulus, poly, aFactor, aFactor.Clone().Monic(), bFactor, aCoFactor, bCoFactor)
            {
            }

            /// <summary>
            /// The initial modulus (less than 64-bit)
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            public override UnivariatePolynomial<BigInteger> PolyMod()
            {
                return poly.SetRing(ring);
            }

            /// <summary>
            /// The initial modulus (less than 64-bit)
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            public override UnivariatePolynomial<BigInteger> AFactorMod()
            {
                return aFactor.SetRing(ring);
            }

            /// <summary>
            /// The initial modulus (less than 64-bit)
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            public override UnivariatePolynomial<BigInteger> BFactorMod()
            {
                return bFactor.SetRing(ring);
            }

            /// <summary>
            /// The initial modulus (less than 64-bit)
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            public override UnivariatePolynomial<BigInteger> ACoFactorMod()
            {
                return aCoFactor == null ? null : aCoFactor.SetRing(ring);
            }

            /// <summary>
            /// The initial modulus (less than 64-bit)
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            public override UnivariatePolynomial<BigInteger> BCoFactorMod()
            {
                return bCoFactor == null ? null : bCoFactor.SetRing(ring);
            }

            /// <summary>
            /// The initial modulus (less than 64-bit)
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            private void LiftFactors()
            {
                UnivariatePolynomialZp64 factorsDiff = UnivariatePolynomial.AsOverZp64(poly.Clone().Subtract(aFactor.Clone().Multiply(bFactor)).DivideOrNull(ring.modulus).SetRing(initialDomain));
                CalculateFactorsDiff(factorsDiff);
                aFactor = aFactor.Add(aAdd.AsPolyZ(false).ToBigPoly().Multiply(ring.modulus));
                bFactor = bFactor.Add(bAdd.AsPolyZ(false).ToBigPoly().Multiply(ring.modulus));
            }

            /// <summary>
            /// The initial modulus (less than 64-bit)
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            private void LiftCoFactors()
            {
                UnivariatePolynomialZp64 coFactorsDiff = UnivariatePolynomial.AsOverZp64(aCoFactor.Clone().Multiply(aFactor).Add(bCoFactor.Clone().Multiply(bFactor)).Decrement().Negate().DivideOrNull(ring.modulus).SetRing(initialDomain));
                CalculateCoFactorsDiff(coFactorsDiff);
                aCoFactor = aCoFactor.Add(aAdd.AsPolyZ(false).ToBigPoly().Multiply(ring.modulus));
                bCoFactor = bCoFactor.Add(bAdd.AsPolyZ(false).ToBigPoly().Multiply(ring.modulus));
            }

            /// <summary>
            /// The initial modulus (less than 64-bit)
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            public override void Lift()
            {
                LiftFactors();
                LiftCoFactors();
                ring = new IntegersZp(ring.modulus.Multiply(initialDomain.modulus));
            }

            /// <summary>
            /// The initial modulus (less than 64-bit)
            /// </summary>
            /// <summary>
            /// The modulus
            /// </summary>
            public override void LiftLast()
            {
                LiftFactors();
                ring = new IntegersZp(ring.modulus.Multiply(initialDomain.modulus));
                aCoFactor = bCoFactor = null;
            }
        }
    }
}