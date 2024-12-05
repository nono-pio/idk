using System.Diagnostics;
using System.Numerics;
using Rings.poly.univar;

namespace Rings.poly.univar;

public static class HenselLifting
{
    public interface LiftableQuintet<PolyZp> where PolyZp : IUnivariatePolynomial<PolyZp>
    {
        PolyZp polyMod();


        PolyZp aFactorMod();


        PolyZp bFactorMod();


        PolyZp aCoFactorMod();


        PolyZp bCoFactorMod();


        void lift();


        void liftLast();

        void lift(int nIterations)
        {
            for (int i = 0; i < nIterations - 1; ++i)
                lift();
            liftLast();
        }

        void liftWithCoFactors(int nIterations)
        {
            for (int i = 0; i < nIterations; ++i)
                lift();
        }
    }

    /* ************************************ Factory methods ************************************ */


    public static lQuadraticLift createQuadraticLift(long modulus,
        UnivariatePolynomialZ64 poly,
        UnivariatePolynomialZp64 aFactor,
        UnivariatePolynomialZp64 bFactor)
    {
        bFactor = ensureMonic(bFactor);
        if (aFactor.ring.modulus(poly.lc()) != aFactor.lc())
            aFactor = aFactor.clone().monic(poly.lc());
        UnivariatePolynomialZp64[] xgcd = monicExtendedEuclid(aFactor, bFactor);
        return new lQuadraticLift(modulus, poly, aFactor, bFactor, xgcd[1], xgcd[2]);
    }

    private static void ensureIntegersDomain(UnivariatePolynomial<BigInteger> poly)
    {
        if (!poly.ring.Equals(Rings.Z))
            throw new ArgumentException("Not an integers ring ring: " + poly.ring);
    }

    private static void ensureModularDomain(UnivariatePolynomial<BigInteger> poly)
    {
        if (!(poly.ring is IntegersZp))
            throw new ArgumentException("Not a modular ring");
    }

    private static void ensureInputCorrect(UnivariatePolynomial<BigInteger> poly,
        UnivariatePolynomial<BigInteger> aFactor, UnivariatePolynomial<BigInteger> bFactor)
    {
        ensureIntegersDomain(poly);
        ensureModularDomain(aFactor);
        ensureModularDomain(bFactor);
    }


    public static bQuadraticLift createQuadraticLift(BigInteger modulus,
        UnivariatePolynomial<BigInteger> poly,
        UnivariatePolynomial<BigInteger> aFactor,
        UnivariatePolynomial<BigInteger> bFactor)
    {
        ensureInputCorrect(poly, aFactor, bFactor);
        bFactor = ensureMonic(bFactor);
        IntegersZp ring = (IntegersZp)aFactor.ring;
        if (!ring.valueOf(poly.lc()).Equals(aFactor.lc()))
            aFactor = aFactor.clone().monic(poly.lc());
        UnivariatePolynomial<BigInteger>[] xgcd = monicExtendedEuclid(aFactor, bFactor);
        return new bQuadraticLift(modulus, poly, aFactor, bFactor, xgcd[1], xgcd[2]);
    }


    public static bQuadraticLift createQuadraticLift(BigInteger modulus,
        UnivariatePolynomial<BigInteger> poly,
        UnivariatePolynomialZp64 aFactor,
        UnivariatePolynomialZp64 bFactor)
    {
        ensureIntegersDomain(poly);
        bFactor = ensureMonic(bFactor);
        long lc = (long)(poly.lc() % modulus);
        if (lc != aFactor.lc())
            aFactor = aFactor.clone().monic(lc);
        UnivariatePolynomialZp64[] xgcd = monicExtendedEuclid(aFactor, bFactor);
        return new bQuadraticLift(modulus, poly, aFactor.toBigPoly(), bFactor.toBigPoly(), xgcd[1].toBigPoly(),
            xgcd[2].toBigPoly());
    }


    public static lLinearLift createLinearLift(BigInteger modulus,
        UnivariatePolynomialZ64 poly,
        UnivariatePolynomialZp64 aFactor,
        UnivariatePolynomialZp64 bFactor)
    {
        return createLinearLift((long)modulus, poly, aFactor, bFactor);
    }


    public static bLinearLift createLinearLift(BigInteger modulus,
        UnivariatePolynomial<BigInteger> poly,
        UnivariatePolynomialZp64 aFactor,
        UnivariatePolynomialZp64 bFactor)
    {
        return createLinearLift((long)modulus, poly, aFactor, bFactor);
    }


    public static lLinearLift createLinearLift(long modulus,
        UnivariatePolynomialZ64 poly,
        UnivariatePolynomialZp64 aFactor,
        UnivariatePolynomialZp64 bFactor)
    {
        bFactor = ensureMonic(bFactor);
        if (aFactor.ring.modulus(poly.lc()) != aFactor.lc())
            aFactor = aFactor.clone().monic(poly.lc());

        UnivariatePolynomialZp64[] xgcd = monicExtendedEuclid(aFactor, bFactor);
        return new lLinearLift(modulus, poly, aFactor, bFactor, xgcd[1], xgcd[2]);
    }


    public static bLinearLift createLinearLift(long modulus,
        UnivariatePolynomial<BigInteger> poly,
        UnivariatePolynomialZp64 aFactor,
        UnivariatePolynomialZp64 bFactor)
    {
        ensureIntegersDomain(poly);
        BigInteger bModulus = new BigInteger(modulus);
        bFactor = ensureMonic(bFactor);
        long lc = (long)(poly.lc() % bModulus);
        if (lc != aFactor.lc())
            aFactor = aFactor.clone().monic(lc);

        UnivariatePolynomialZp64[] xgcd = monicExtendedEuclid(aFactor, bFactor);
        return new bLinearLift(bModulus, poly, aFactor, bFactor, xgcd[1], xgcd[2]);
    }


    private static PolyZp[] monicExtendedEuclid<PolyZp>(PolyZp a, PolyZp b) where PolyZp : IUnivariatePolynomial<PolyZp>
    {
        PolyZp[] xgcd = UnivariateGCD.PolynomialExtendedGCD(a, b);
        if (xgcd[0].isOne())
            return xgcd;

        // assert xgcd[0].isConstant() : "bad xgcd: " + Arrays.toString(xgcd) + " for xgcd(" + a + ", " + b + ")";

        //normalize: x * a + y * b = 1
        xgcd[2].divideByLC(xgcd[0]);
        xgcd[1].divideByLC(xgcd[0]);
        xgcd[0].monic();

        return xgcd;
    }

    private static PolyZp ensureMonic<PolyZp>(PolyZp p) where PolyZp : IUnivariatePolynomial<PolyZp>
    {
        return p.isMonic() ? p : p.clone().monic();
    }

    private static long[] nIterations(long modulus, long desiredBound, bool quadratic)
    {
        int nIterations = 0;
        long tmp = modulus;
        while (tmp < desiredBound)
        {
            tmp = MachineArithmetic.safeMultiply(tmp, quadratic ? tmp : modulus);
            ++nIterations;
        }

        return new long[] { nIterations, tmp };
    }


    public static List<UnivariatePolynomialZp64> liftFactorization(long modulus,
        long desiredBound,
        UnivariatePolynomialZ64 poly,
        List<UnivariatePolynomialZp64> modularFactors,
        bool quadratic)
    {
        long[] im = nIterations(modulus, desiredBound, quadratic);
        return liftFactorization(modulus, im[1], (int)im[0], poly, modularFactors, quadratic);
    }


    public static List<UnivariatePolynomialZp64> liftFactorization(long modulus,
        long finalModulus,
        int nIterations,
        UnivariatePolynomialZ64 poly,
        List<UnivariatePolynomialZp64> modularFactors,
        bool quadratic)
    {
        Debug.Assert(nIterations > 0);

        // for the future:
        // recursion may be replaced with precomputed binary tree
        // for now the major part of execution time (~99%) is spent in actual lifting step, so irrelevant

        if (modularFactors.Count == 1)
            return Collections.singletonList(poly.modulus(finalModulus, true).monic());

        UnivariatePolynomialZp64 factory = modularFactors.get(0);
        UnivariatePolynomialZp64
            aFactor = factory.createConstant(poly.lc()),
            bFactor = factory.createOne();

        int nHalf = modularFactors.Count / 2, i = 0;
        for (; i < nHalf; ++i)
            aFactor = aFactor.multiply(modularFactors[i]);
        for (; i < modularFactors.Count; ++i)
            bFactor = bFactor.multiply(modularFactors[i]);

        LiftableQuintet<UnivariatePolynomialZp64> hensel = quadratic
            ? createQuadraticLift(modulus, poly, aFactor, bFactor)
            : createLinearLift(modulus, poly, aFactor, bFactor);
        hensel.lift(nIterations);

        UnivariatePolynomialZp64 aFactorRaised = hensel.aFactorMod();
        UnivariatePolynomialZp64 bFactorRaised = hensel.bFactorMod();

        List<UnivariatePolynomialZp64> result = new List<>();
        result.AddRange(liftFactorization(modulus, finalModulus, nIterations, aFactorRaised.asPolyZSymmetric(),
            modularFactors.subList(0, nHalf), quadratic));
        result.AddRange(liftFactorization(modulus, finalModulus, nIterations, bFactorRaised.asPolyZSymmetric(),
            modularFactors.subList(nHalf, modularFactors.Count), quadratic));
        return result;
    }

    interface LiftFactory<PolyZp extends IUnivariatePolynomial<PolyZp>> {
        LiftableQuintet<UnivariatePolynomial<BigInteger>> createLift(BigInteger modulus,
            UnivariatePolynomial<BigInteger> polyZ, PolyZp aFactor, PolyZp bFactor);
    }


    static <PolyZp extends IUnivariatePolynomial<PolyZp>>

    List<UnivariatePolynomial<BigInteger>> liftFactorization0(BigInteger modulus,
        BigInteger finalModulus,
        int nIterations,
        UnivariatePolynomial<BigInteger> poly,
        List<PolyZp> modularFactors,
        LiftFactory<PolyZp> liftFactory)
    {
        assert nIterations > 0;

        // for the future:
        // recursion may be replaced with precomputed binary tree
        // for now the major part of execution time (~99%) is spent in actual lifting step, so irrelevant

        if (modularFactors.Count == 1)
            return Collections.singletonList(poly.setRing(new IntegersZp(finalModulus)).monic());

        PolyZp factory = modularFactors.get(0);
        PolyZp
            aFactor = factory.createOne(),
            bFactor = factory.createOne();

        int nHalf = modularFactors.Count / 2, i = 0;
        for (; i < nHalf; ++i)
            aFactor = aFactor.multiply(modularFactors[i]);
        for (; i < modularFactors.Count; ++i)
            bFactor = bFactor.multiply(modularFactors[i]);

        LiftableQuintet<UnivariatePolynomial<BigInteger>> hensel =
            liftFactory.createLift(modulus, poly, aFactor, bFactor);
        hensel.lift(nIterations);

        UnivariatePolynomial<BigInteger> aFactorRaised = hensel.aFactorMod();
        UnivariatePolynomial<BigInteger> bFactorRaised = hensel.bFactorMod();

        List<UnivariatePolynomial<BigInteger>> result = new List<>();
        result.addAll(liftFactorization0(modulus, finalModulus, nIterations,
            UnivariatePolynomial.asPolyZSymmetric(aFactorRaised), modularFactors.subList(0, nHalf), liftFactory));
        result.addAll(liftFactorization0(modulus, finalModulus, nIterations,
            UnivariatePolynomial.asPolyZSymmetric(bFactorRaised), modularFactors.subList(nHalf, modularFactors.Count),
            liftFactory));
        return result;
    }

    sealed class LiftingInfo
    {
        readonly int nIterations;
        readonly BigInteger finalModulus;

        public LiftingInfo(int nIterations, BigInteger finalModulus)
        {
            this.nIterations = nIterations;
            this.finalModulus = finalModulus;
        }
    }

    static LiftingInfo nIterations(BigInteger modulus, BigInteger desiredBound, bool quadratic)
    {
        int nIterations = 0;
        BigInteger finalModulus = modulus;
        while (finalModulus.CompareTo(desiredBound) < 0)
        {
            finalModulus = finalModulus.multiply(quadratic ? finalModulus : modulus);
            ++nIterations;
        }

        return new LiftingInfo(nIterations, finalModulus);
    }

    /**
     * Lifts modular factorization until {@code modulus} will overcome {@code desiredBound}. <b>Note:</b> if {@code
     * quadratic == false} modulus must fit 64-bit.
     *
     * @param modulus        initial modulus so that {@code modularFactors} are true factors of {@code poly mod
     *                       modulus}
     * @param desiredBound   desired modulus
     * @param poly           initial Z[x] polynomial
     * @param modularFactors factorization of {@code poly.modulus(modulus)}
     * @param quadratic      whether to use quadratic of linear lift
     * @return factorization of {@code poly.modulus(finalModulus) } with some {@code finalModulus} greater than {@code
     * desiredBound}
     */
    public static List<UnivariatePolynomial<BigInteger>> liftFactorization(BigInteger modulus,
        BigInteger desiredBound,
        UnivariatePolynomial<BigInteger> poly,
        List<UnivariatePolynomialZp64> modularFactors,
        bool quadratic)
    {
        if (!quadratic && !modulus.isLong())
            throw new ArgumentException("Only max 64-bit modulus for linear lift allowed.");
        LiftingInfo im = nIterations(modulus, desiredBound, quadratic);
        if (im.nIterations == 0)
            return modularFactors.stream().map(UnivariatePolynomialZp64::toBigPoly).collect(Collectors.toList());
        LiftFactory<UnivariatePolynomialZp64> factory =
            quadratic ? HenselLifting::createQuadraticLift : HenselLifting::createLinearLift;
        return liftFactorization0(modulus, im.finalModulus, im.nIterations, poly, modularFactors, factory);
    }


    public static List<UnivariatePolynomial<BigInteger>> liftFactorizationQuadratic(BigInteger modulus,
        BigInteger desiredBound,
        UnivariatePolynomial<BigInteger> poly,
        List<UnivariatePolynomial<BigInteger>> modularFactors)
    {
        LiftingInfo im = nIterations(modulus, desiredBound, true);
        return liftFactorization0(modulus, im.finalModulus, im.nIterations, poly, modularFactors,
            HenselLifting::createQuadraticLift);
    }

    /**
     * Lifts modular factorization until {@code modulus} will overcome {@code desiredBound}. <i>Implementation note:</i>
     * method will switch between linear and quadratic lift depending on the required lifting iterations.
     *
     * @param modulus        initial modulus so that {@code modularFactors} are true factors of {@code poly mod
     *                       modulus}
     * @param desiredBound   desired modulus
     * @param poly           initial Z[x] polynomial
     * @param modularFactors factorization of {@code poly.modulus(modulus)}
     * @return factorization of {@code poly.modulus(finalModulus) } with some {@code finalModulus} greater than {@code
     * desiredBound}
     */
    public static List<UnivariatePolynomial<BigInteger>> liftFactorization(BigInteger modulus,
        BigInteger desiredBound,
        UnivariatePolynomial<BigInteger> poly,
        List<UnivariatePolynomialZp64> modularFactors)
    {
        return liftFactorization(poly, modularFactors, new AdaptiveLift(modulus, desiredBound));
    }


    private static List<UnivariatePolynomial<BigInteger>> liftFactorization(UnivariatePolynomial<BigInteger> poly,
        List<UnivariatePolynomialZp64> modularFactors,
        AdaptiveLift lifter)
    {
        // for the future:
        // recursion may be replaced with precomputed binary tree
        // for now the major part of execution time (~99%) is spent in actual lifting step, so irrelevant

        if (modularFactors.Count == 1)
            return Collections.singletonList(poly.setRing(new IntegersZp(lifter.finalModulus)).monic());

        UnivariatePolynomialZp64 factory = modularFactors[0];
        UnivariatePolynomialZp64
            aFactor = factory.createOne(),
            bFactor = factory.createOne();

        int nHalf = modularFactors.Count / 2, i = 0;
        for (; i < nHalf; ++i)
            aFactor = aFactor.multiply(modularFactors[i]);
        for (; i < modularFactors.Count; ++i)
            bFactor = bFactor.multiply(modularFactors[i]);

        UnivariatePolynomial<BigInteger>[] lifted = lifter.lift(poly, aFactor, bFactor);
        UnivariatePolynomial<BigInteger> aFactorRaised = lifted[0];
        UnivariatePolynomial<BigInteger> bFactorRaised = lifted[1];

        List<UnivariatePolynomial<BigInteger>> result = new List<>();
        result.AddRange(liftFactorization(UnivariatePolynomial.asPolyZSymmetric(aFactorRaised),
            modularFactors.subList(0, nHalf), lifter));
        result.AddRange(liftFactorization(UnivariatePolynomial.asPolyZSymmetric(bFactorRaised),
            modularFactors.subList(nHalf, modularFactors.Count), lifter));
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
            LiftingInfo
                nLinearIterations = nIterations(initialModulus, desiredBound, false);

            if (nLinearIterations.nIterations < SWITCH_TO_QUADRATIC_LIFT)
            {
                this.nLinearIterations = nLinearIterations.nIterations;
                this.nQuadraticIterations = -1;
                this.finalModulus = nLinearIterations.finalModulus;
            }
            else
            {
                LiftingInfo nQuadraticIterations = nIterations(initialModulus, desiredBound, true);
                this.nLinearIterations = -1;
                this.nQuadraticIterations = nQuadraticIterations.nIterations;
                this.finalModulus = nQuadraticIterations.finalModulus;
            }
        }


        UnivariatePolynomial<BigInteger>[] lift(UnivariatePolynomial<BigInteger> poly, UnivariatePolynomialZp64 a,
            UnivariatePolynomialZp64 b)
        {
            bool quadratic = nLinearIterations == -1;
            LiftableQuintet<UnivariatePolynomial<BigInteger>> lift =
                quadratic
                    ? createQuadraticLift(initialModulus, poly, a.toBigPoly(), b.toBigPoly())
                    : createLinearLift(initialModulus, poly, a, b);
            lift.lift(quadratic ? nQuadraticIterations : nLinearIterations);
            return new UnivariatePolynomial[] { lift.aFactorMod(), lift.bFactorMod() };
        }
    }

    private static void assertHenselLift<T>(LiftableQuintet<T> lift) where T : IUnivariatePolynomial<T>
    {
        assert lift.polyMod().equals(lift.aFactorMod().clone().multiply(lift.bFactorMod())) : lift.toString();
        assert(lift.aCoFactorMod() == null && lift.bCoFactorMod() == null) ||
            lift.aFactorMod().clone().multiply(lift.aCoFactorMod())
                .add(lift.bFactorMod().clone().multiply(lift.bCoFactorMod()))
                .isOne() :
        lift.aFactorMod().clone().multiply(lift.aCoFactorMod())
                .add(lift.bFactorMod().clone().multiply(lift.bCoFactorMod())) + "  --- " +
            ((UnivariatePolynomial<BigInteger>)lift.aFactorMod()).ring;
    }

    /* ************************************ Quadratic lifts ************************************ */


    public abstract class QuadraticLiftAbstract<PolyZp> : LiftableQuintet<PolyZp>
        where PolyZp : IUnivariatePolynomial<PolyZp>
    {
        protected PolyZp aFactor, bFactor;

        protected PolyZp aCoFactor, bCoFactor;

        public QuadraticLiftAbstract(PolyZp aFactor, PolyZp bFactor, PolyZp aCoFactor, PolyZp bCoFactor)
        {
            this.aFactor = aFactor;
            this.bFactor = bFactor;
            this.aCoFactor = aCoFactor;
            this.bCoFactor = bCoFactor;
        }


        public PolyZp aFactorMod()
        {
            return aFactor;
        }


        public PolyZp bFactorMod()
        {
            return bFactor;
        }


        public PolyZp aCoFactorMod()
        {
            return aCoFactor;
        }


        public PolyZp bCoFactorMod()
        {
            return bCoFactor;
        }

        public abstract void prepare();


        public void lift()
        {
            prepare();
            henselStep0(polyMod());
        }


        public void liftLast()
        {
            prepare();
            henselLastStep0(polyMod());
        }

        private void henselStep0(PolyZp baseMod)
        {
            PolyZp e = baseMod.subtract(aFactor.clone().multiply(bFactor));

            PolyZp[] qr = UnivariateDivision.divideAndRemainder(
                aCoFactor.clone().multiply(e),
                bFactor, false);
            PolyZp q = qr[0], r = qr[1];

            PolyZp aFactorNew = aFactor.clone()
                .add(bCoFactor.clone().multiply(e))
                .add(aFactor.clone().multiply(q));

            PolyZp bFactorNew = bFactor.clone().add(r);

            PolyZp b = aCoFactor.clone().multiply(aFactorNew)
                .add(bCoFactor.clone().multiply(bFactorNew))
                .decrement();

            PolyZp[] cd = UnivariateDivision.divideAndRemainder(
                aCoFactor.clone().multiply(b),
                bFactorNew, false);
            PolyZp c = cd[0], d = cd[1];

            PolyZp aCoFactorNew = aCoFactor.subtract(d);
            PolyZp bCoFactorNew = bCoFactor
                .subtract(bCoFactor.clone().multiply(b))
                .subtract(c.clone().multiply(aFactorNew));

            assert aFactorNew.degree() == aFactor.degree() : String.format("%s > %s", aFactorNew.degree(),
                aFactor.degree());
            assert bFactorNew.degree() == bFactor.degree() : String.format("%s > %s", bFactorNew.degree(),
                bFactor.degree());

            aFactor = aFactorNew;
            aCoFactor = aCoFactorNew;
            bFactor = bFactorNew;
            bCoFactor = bCoFactorNew;

            assert bFactor.isMonic();
            assert aCoFactor.degree() < bFactor.degree();
            assert bCoFactor.degree() < aFactor.degree();
        }

        private void henselLastStep0(PolyZp baseMod)
        {
            PolyZp e = baseMod.subtract(aFactor.clone().multiply(bFactor));

            PolyZp[] qr = UnivariateDivision.divideAndRemainder(
                aCoFactor.multiply(e),
                bFactor, false);
            PolyZp q = qr[0], r = qr[1];

            PolyZp aFactorNew = aFactor
                .add(bCoFactor.multiply(e))
                .add(aFactor.clone().multiply(q));

            PolyZp bFactorNew = bFactor.add(r);

            aFactor = aFactorNew;
            aCoFactor = null;
            bFactor = bFactorNew;
            bCoFactor = null;

            assert bFactor.isMonic();
        }
    }


    public class lQuadraticLift : QuadraticLiftAbstract<UnivariatePolynomialZp64>
    {
        public long modulus;
        public readonly UnivariatePolynomialZ64 @base;

        public lQuadraticLift(long modulus, UnivariatePolynomialZ64 @base, UnivariatePolynomialZp64 aFactor,
            UnivariatePolynomialZp64 bFactor, UnivariatePolynomialZp64 aCoFactor,
            UnivariatePolynomialZp64 bCoFactor) : base(aFactor, bFactor, aCoFactor, bCoFactor)
        {
            this.modulus = modulus;
            this.@base = @base;
        }


        public UnivariatePolynomialZp64 polyMod()
        {
            return base.modulus(modulus, true);
        }


        void prepare()
        {
            modulus = MachineArithmetic.safeMultiply(modulus, modulus);
            aFactor = aFactor.setModulusUnsafe(modulus);
            bFactor = bFactor.setModulusUnsafe(modulus);
            aCoFactor = aCoFactor.setModulusUnsafe(modulus);
            bCoFactor = bCoFactor.setModulusUnsafe(modulus);
        }
    }


    public class bQuadraticLift : QuadraticLiftAbstract<UnivariatePolynomial<BigInteger>>
    {
        public IntegersZp ring;
        public readonly UnivariatePolynomial<BigInteger> @base;

        public bQuadraticLift(BigInteger modulus, UnivariatePolynomial<BigInteger> @base,
            UnivariatePolynomial<BigInteger> aFactor, UnivariatePolynomial<BigInteger> bFactor,
            UnivariatePolynomial<BigInteger> aCoFactor, UnivariatePolynomial<BigInteger> bCoFactor) : base(aFactor,
            bFactor, aCoFactor, bCoFactor)
        {
            this.ring = new IntegersZp(modulus);
            this.@base = @base;
        }

        public UnivariatePolynomial<BigInteger> polyMod()
        {
            return base.setRing(ring);
        }

        void prepare()
        {
            ring = new IntegersZp(ring.modulus.multiply(ring.modulus));
            aFactor = aFactor.setRingUnsafe(ring);
            bFactor = bFactor.setRingUnsafe(ring);
            aCoFactor = aCoFactor.setRingUnsafe(ring);
            bCoFactor = bCoFactor.setRingUnsafe(ring);
        }
    }
/* ************************************ Linear lifts ************************************ */

    private class LinearLiftAbstract<PolyZ> where PolyZ : IUnivariatePolynomial<PolyZ>
    {
        readonly PolyZ poly;

        PolyZ aFactor, bFactor, aCoFactor, bCoFactor;

        readonly UnivariatePolynomialZp64 aFactorMod, aFactorModMonic, bFactorMod, aCoFactorMod, bCoFactorMod;

        readonly UnivariateDivision.InverseModMonomial<UnivariatePolynomialZp64> aFactorModMonicInv, bFactorModInv;

        public LinearLiftAbstract(PolyZ poly,
            PolyZ aFactor, PolyZ bFactor, PolyZ aCoFactor, PolyZ bCoFactor,
            UnivariatePolynomialZp64 aFactorMod, UnivariatePolynomialZp64 aFactorModMonic,
            UnivariatePolynomialZp64 bFactorMod,
            UnivariatePolynomialZp64 aCoFactorMod, UnivariatePolynomialZp64 bCoFactorMod)
        {
            assert bFactor.isMonic();
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
            this.aFactorModMonicInv = UnivariateDivision.fastDivisionPreConditioning(aFactorModMonic);
            this.bFactorModInv = UnivariateDivision.fastDivisionPreConditioning(bFactorMod);
        }

        protected UnivariatePolynomialZp64 aAdd, bAdd;

        void calculateFactorsDiff(UnivariatePolynomialZp64 diff)
        {
            aAdd = diff.clone();
            aAdd = UnivariatePolynomialArithmetic.polyMod(aAdd, aFactorModMonic, aFactorModMonicInv, false);
            aAdd = aAdd.multiply(bCoFactorMod);
            aAdd = UnivariatePolynomialArithmetic.polyMod(aAdd, aFactorModMonic, aFactorModMonicInv, false);

            bAdd = diff.clone();
            bAdd = UnivariatePolynomialArithmetic.polyMod(bAdd, bFactorMod, bFactorModInv, false);
            bAdd = bAdd.multiply(aCoFactorMod);
            bAdd = UnivariatePolynomialArithmetic.polyMod(bAdd, bFactorMod, bFactorModInv, false);
        }

        void calculateCoFactorsDiff(UnivariatePolynomialZp64 diff)
        {
            aAdd = diff.clone();
            aAdd = UnivariatePolynomialArithmetic.polyMod(aAdd, bFactorMod, bFactorModInv, false);
            aAdd = aAdd.multiply(aCoFactorMod);
            aAdd = UnivariatePolynomialArithmetic.polyMod(aAdd, bFactorMod, bFactorModInv, false);

            bAdd = diff.clone();
            bAdd = UnivariatePolynomialArithmetic.polyMod(bAdd, aFactorModMonic, aFactorModMonicInv, false);
            bAdd = bAdd.multiply(bCoFactorMod);
            bAdd = UnivariatePolynomialArithmetic.polyMod(bAdd, aFactorModMonic, aFactorModMonicInv, false);
        }
    }


    public class lLinearLift : LinearLiftAbstract<UnivariatePolynomialZ64>, LiftableQuintet<UnivariatePolynomialZp64>
    {
        public readonly long initialModulus;
        public long modulus;

        private lLinearLift(long modulus, UnivariatePolynomialZ64 poly,
            UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 aFactorMonic, UnivariatePolynomialZp64 bFactor,
            UnivariatePolynomialZp64 aCoFactor, UnivariatePolynomialZp64 bCoFactor) : base(poly,
            ensureMonic(aFactor).asPolyZ(true).multiply(poly.lc()), bFactor.asPolyZ(true),
            aCoFactor.asPolyZ(true), bCoFactor.asPolyZ(true),
            aFactor, aFactorMonic, bFactor, aCoFactor, bCoFactor)
        {
            this.initialModulus = modulus;
            this.modulus = modulus;
        }

        private lLinearLift(long modulus, UnivariatePolynomialZ64 poly,
            UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 bFactor,
            UnivariatePolynomialZp64 aCoFactor, UnivariatePolynomialZp64 bCoFactor) : this(modulus, poly, aFactor,
            aFactor.clone().monic(), bFactor, aCoFactor, bCoFactor)
        {
        }

        public UnivariatePolynomialZp64 polyMod()
        {
            return poly.modulus(modulus);
        }

        public UnivariatePolynomialZp64 aFactorMod()
        {
            return aFactor.modulus(modulus);
        }

        public UnivariatePolynomialZp64 bFactorMod()
        {
            return bFactor.modulus(modulus);
        }

        public UnivariatePolynomialZp64 aCoFactorMod()
        {
            return aCoFactor == null ? null : aCoFactor.modulus(modulus);
        }

        public UnivariatePolynomialZp64 bCoFactorMod()
        {
            return bCoFactor == null ? null : bCoFactor.modulus(modulus);
        }

        private void liftFactors()
        {
            UnivariatePolynomialZp64 factorsDiff = poly.clone().subtract(aFactor.clone().multiply(bFactor))
                .divideOrNull(modulus)
                .modulus(initialModulus);

            calculateFactorsDiff(factorsDiff);
            aFactor = aFactor.add(aAdd.asPolyZ(false).multiply(modulus));
            bFactor = bFactor.add(bAdd.asPolyZ(false).multiply(modulus));
        }

        private void liftCoFactors()
        {
            UnivariatePolynomialZp64 coFactorsDiff = aCoFactor.clone().multiply(aFactor)
                .add(bCoFactor.clone().multiply(bFactor))
                .decrement()
                .negate()
                .divideOrNull(modulus)
                .modulus(initialModulus);

            calculateCoFactorsDiff(coFactorsDiff);
            aCoFactor = aCoFactor.add(aAdd.asPolyZ(false).multiply(modulus));
            bCoFactor = bCoFactor.add(bAdd.asPolyZ(false).multiply(modulus));
        }

        public void lift()
        {
            liftFactors();
            liftCoFactors();
            modulus = MachineArithmetic.safeMultiply(modulus, initialModulus);
        }

        public void liftLast()
        {
            liftFactors();
            modulus = MachineArithmetic.safeMultiply(modulus, initialModulus);
            aCoFactor = bCoFactor = null;
        }
    }


    public class bLinearLift : LinearLiftAbstract<UnivariatePolynomial<BigInteger>>,
        HenselLifting.LiftableQuintet<UnivariatePolynomial<BigInteger>>
    {
        public readonly IntegersZp initialDomain;
        public IntegersZp ring;

        private bLinearLift(BigInteger modulus, UnivariatePolynomial<BigInteger> poly,
            UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 aFactorMonic, UnivariatePolynomialZp64 bFactor,
            UnivariatePolynomialZp64 aCoFactor, UnivariatePolynomialZp64 bCoFactor) : base(poly,
            ensureMonic(aFactor).asPolyZ(true).toBigPoly().multiply(poly.lc()), bFactor.asPolyZ(false).toBigPoly(),
            aCoFactor.asPolyZ(false).toBigPoly(), bCoFactor.asPolyZ(false).toBigPoly(),
            aFactor, aFactorMonic, bFactor, aCoFactor, bCoFactor)
        {
            this.initialDomain = new IntegersZp(modulus);
            this.ring = new IntegersZp(modulus);
            assert modulus.isLong();
        }

        private bLinearLift(BigInteger modulus, UnivariatePolynomial<BigInteger> poly,
            UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 bFactor,
            UnivariatePolynomialZp64 aCoFactor, UnivariatePolynomialZp64 bCoFactor) : this(modulus, poly, aFactor,
            aFactor.clone().monic(), bFactor, aCoFactor, bCoFactor)
        {
        }

        public UnivariatePolynomial<BigInteger> polyMod()
        {
            return poly.setRing(ring);
        }

        public UnivariatePolynomial<BigInteger> aFactorMod()
        {
            return aFactor.setRing(ring);
        }

        public UnivariatePolynomial<BigInteger> bFactorMod()
        {
            return bFactor.setRing(ring);
        }

        public UnivariatePolynomial<BigInteger> aCoFactorMod()
        {
            return aCoFactor == null ? null : aCoFactor.setRing(ring);
        }

        public UnivariatePolynomial<BigInteger> bCoFactorMod()
        {
            return bCoFactor == null ? null : bCoFactor.setRing(ring);
        }

        private void liftFactors()
        {
            UnivariatePolynomialZp64 factorsDiff = UnivariatePolynomial.asOverZp64(
                poly.clone().subtract(aFactor.clone().multiply(bFactor))
                    .divideOrNull(ring.modulus)
                    .setRing(initialDomain));

            calculateFactorsDiff(factorsDiff);
            aFactor = aFactor.add(aAdd.asPolyZ(false).toBigPoly().multiply(ring.modulus));
            bFactor = bFactor.add(bAdd.asPolyZ(false).toBigPoly().multiply(ring.modulus));
        }

        private void liftCoFactors()
        {
            UnivariatePolynomialZp64 coFactorsDiff = UnivariatePolynomial.asOverZp64(
                aCoFactor.clone().multiply(aFactor).add(bCoFactor.clone().multiply(bFactor)).decrement().negate()
                    .divideOrNull(ring.modulus)
                    .setRing(initialDomain));

            calculateCoFactorsDiff(coFactorsDiff);

            aCoFactor = aCoFactor.add(aAdd.asPolyZ(false).toBigPoly().multiply(ring.modulus));
            bCoFactor = bCoFactor.add(bAdd.asPolyZ(false).toBigPoly().multiply(ring.modulus));
        }

        public void lift()
        {
            liftFactors();
            liftCoFactors();
            ring = new IntegersZp(ring.modulus.multiply(initialDomain.modulus));
        }

        public void liftLast()
        {
            liftFactors();
            ring = new IntegersZp(ring.modulus.multiply(initialDomain.modulus));
            aCoFactor = bCoFactor = null;
        }
    }
}