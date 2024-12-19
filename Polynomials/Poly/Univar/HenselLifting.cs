using System.Numerics;
using Polynomials.Utils;
using UnivariatePolynomialZp64 = Polynomials.Poly.Univar.UnivariatePolynomial<long>;
using UnivariatePolynomialZ64 = Polynomials.Poly.Univar.UnivariatePolynomial<long>;

namespace Polynomials.Poly.Univar;

public static class HenselLifting
{
    public interface LiftableQuintet<E>
    {
        UnivariatePolynomial<E> PolyMod();


        UnivariatePolynomial<E> AFactorMod();


        UnivariatePolynomial<E> BFactorMod();


        UnivariatePolynomial<E> ACoFactorMod();


        UnivariatePolynomial<E> BCoFactorMod();


        void Lift();


        void LiftLast();


        void Lift(int nIterations)
        {
            for (var i = 0; i < nIterations - 1; ++i)
                Lift();
            LiftLast();
        }

        void LiftWithCoFactors(int nIterations)
        {
            for (var i = 0; i < nIterations; ++i)
                Lift();
        }
    }

    /* ************************************ Factory methods ************************************ */


    public static lQuadraticLift CreateQuadraticLift(long modulus, UnivariatePolynomialZ64 poly,
        UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 bFactor)
    {
        bFactor = EnsureMonic(bFactor);
        if (((IntegersZp64)aFactor.ring).Modulus(poly.Lc()) != aFactor.Lc())
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

    private static void EnsureInputCorrect(UnivariatePolynomial<BigInteger> poly,
        UnivariatePolynomial<BigInteger> aFactor, UnivariatePolynomial<BigInteger> bFactor)
    {
        EnsureIntegersDomain(poly);
        EnsureModularDomain(aFactor);
        EnsureModularDomain(bFactor);
    }


    public static bQuadraticLift CreateQuadraticLift(BigInteger modulus, UnivariatePolynomial<BigInteger> poly,
        UnivariatePolynomial<BigInteger> aFactor, UnivariatePolynomial<BigInteger> bFactor)
    {
        EnsureInputCorrect(poly, aFactor, bFactor);
        bFactor = EnsureMonic(bFactor);
        var ring = (IntegersZp)aFactor.ring;
        if (!ring.ValueOf(poly.Lc()).Equals(aFactor.Lc()))
            aFactor = aFactor.Clone().Monic(poly.Lc());
        UnivariatePolynomial<BigInteger>[] xgcd = MonicExtendedEuclid(aFactor, bFactor);
        return new bQuadraticLift(modulus, poly, aFactor, bFactor, xgcd[1], xgcd[2]);
    }


    public static bQuadraticLift CreateQuadraticLift(BigInteger modulus, UnivariatePolynomial<BigInteger> poly,
        UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 bFactor)
    {
        EnsureIntegersDomain(poly);
        bFactor = EnsureMonic(bFactor);
        var lc = (long)(poly.Lc() % modulus);
        if (lc != aFactor.Lc())
            aFactor = aFactor.Clone().Monic(lc);
        UnivariatePolynomialZp64[] xgcd = MonicExtendedEuclid(aFactor, bFactor);
        return new bQuadraticLift(modulus, poly, aFactor.ToBigPoly(), bFactor.ToBigPoly(), xgcd[1].ToBigPoly(),
            xgcd[2].ToBigPoly());
    }


    public static lLinearLift CreateLinearLift(BigInteger modulus, UnivariatePolynomialZ64 poly,
        UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 bFactor)
    {
        return CreateLinearLift((long)modulus, poly, aFactor, bFactor);
    }


    public static bLinearLift CreateLinearLift(BigInteger modulus, UnivariatePolynomial<BigInteger> poly,
        UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 bFactor)
    {
        return CreateLinearLift((long)modulus, poly, aFactor, bFactor);
    }


    public static lLinearLift CreateLinearLift(long modulus, UnivariatePolynomialZ64 poly,
        UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 bFactor)
    {
        bFactor = EnsureMonic(bFactor);
        if (((IntegersZp64)aFactor.ring).Modulus(poly.Lc()) != aFactor.Lc())
            aFactor = aFactor.Clone().Monic(poly.Lc());
        UnivariatePolynomialZp64[] xgcd = MonicExtendedEuclid(aFactor, bFactor);
        return new lLinearLift(modulus, poly, aFactor, bFactor, xgcd[1], xgcd[2]);
    }


    public static bLinearLift CreateLinearLift(long modulus, UnivariatePolynomial<BigInteger> poly,
        UnivariatePolynomialZp64 aFactor, UnivariatePolynomialZp64 bFactor)
    {
        EnsureIntegersDomain(poly);
        var bModulus = new BigInteger(modulus);
        bFactor = EnsureMonic(bFactor);
        var lc = (long)(poly.Lc() % bModulus);
        if (lc != aFactor.Lc())
            aFactor = aFactor.Clone().Monic(lc);
        UnivariatePolynomialZp64[] xgcd = MonicExtendedEuclid(aFactor, bFactor);
        return new bLinearLift(bModulus, poly, aFactor, bFactor, xgcd[1], xgcd[2]);
    }


    private static UnivariatePolynomial<E>[] MonicExtendedEuclid<E>(UnivariatePolynomial<E> a,
        UnivariatePolynomial<E> b)
    {
        var xgcd = UnivariateGCD.PolynomialExtendedGCD(a, b);
        if (xgcd[0].IsOne())
            return xgcd;

        //normalize: x * a + y * b = 1
        xgcd[2].DivideByLC(xgcd[0]);
        xgcd[1].DivideByLC(xgcd[0]);
        xgcd[0].Monic();
        return xgcd;
    }

    private static UnivariatePolynomial<E> EnsureMonic<E>(UnivariatePolynomial<E> p)
    {
        return p.IsMonic() ? p : p.Clone().Monic();
    }

    private static long[] NIterations(long modulus, long desiredBound, bool quadratic)
    {
        var nIterations = 0;
        var tmp = modulus;
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


    public static List<UnivariatePolynomialZp64> LiftFactorization(long modulus, long desiredBound,
        UnivariatePolynomialZ64 poly, List<UnivariatePolynomialZp64> modularFactors, bool quadratic)
    {
        var im = NIterations(modulus, desiredBound, quadratic);
        return LiftFactorization(modulus, im[1], (int)im[0], poly, modularFactors, quadratic);
    }


    public static List<UnivariatePolynomialZp64> LiftFactorization(long modulus, long finalModulus, int nIterations,
        UnivariatePolynomialZ64 poly, List<UnivariatePolynomialZp64> modularFactors, bool quadratic)
    {
        // for the future:
        // recursion may be replaced with precomputed binary tree
        // for now the major part of execution time (~99%) is spent in actual lifting step, so irrelevant
        if (modularFactors.Count == 1)
            return [poly.Modulus(finalModulus, true).Monic()];
        var factory = modularFactors[0];
        UnivariatePolynomialZp64 aFactor = factory.CreateConstant(poly.Lc()), bFactor = factory.CreateOne();
        int nHalf = modularFactors.Count / 2, i = 0;
        for (; i < nHalf; ++i)
            aFactor = aFactor.Multiply(modularFactors[i]);
        for (; i < modularFactors.Count; ++i)
            bFactor = bFactor.Multiply(modularFactors[i]);
        LiftableQuintet<long> hensel = quadratic
            ? CreateQuadraticLift(modulus, poly, aFactor, bFactor)
            : CreateLinearLift(modulus, poly, aFactor, bFactor);
        hensel.Lift(nIterations);
        var aFactorRaised = hensel.AFactorMod();
        var bFactorRaised = hensel.BFactorMod();
        List<UnivariatePolynomialZp64> result = [];
        result.AddRange(LiftFactorization(modulus, finalModulus, nIterations,
            UnivariatePolynomialZ64.AsPolyZ64Symmetric(bFactorRaised),
            modularFactors[0..nHalf], quadratic));
        result.AddRange(LiftFactorization(modulus, finalModulus, nIterations,
            UnivariatePolynomialZ64.AsPolyZ64Symmetric(bFactorRaised),
            modularFactors[nHalf..modularFactors.Count], quadratic));
        return result;
    }

    class LiftFactory<E>
    {

        private Func<BigInteger, UnivariatePolynomial<BigInteger>, UnivariatePolynomial<E>, UnivariatePolynomial<E>,
            LiftableQuintet<BigInteger>> func;
        
        public LiftFactory(Func<BigInteger, UnivariatePolynomial<BigInteger>, UnivariatePolynomial<E>, UnivariatePolynomial<E>,
            LiftableQuintet<BigInteger>> func)
        {
            this.func = func;
        }

        public LiftableQuintet<BigInteger> CreateLift(BigInteger modulus,
            UnivariatePolynomial<BigInteger> polyZ, UnivariatePolynomial<E> aFactor, UnivariatePolynomial<E> bFactor) => func(modulus, polyZ, aFactor, bFactor);
    }

    static List<UnivariatePolynomial<BigInteger>> LiftFactorization0<E>(BigInteger modulus,
        BigInteger finalModulus, int nIterations, UnivariatePolynomial<BigInteger> poly,
        List<UnivariatePolynomial<E>> modularFactors,
        LiftFactory<E> liftFactory)
    {
        // for the future:
        // recursion may be replaced with precomputed binary tree
        // for now the major part of execution time (~99%) is spent in actual lifting step, so irrelevant
        if (modularFactors.Count == 1)
            return [poly.SetRing(new IntegersZp(finalModulus)).Monic()];
        var factory = modularFactors[0];
        var aFactor = factory.CreateOne();
        var bFactor = factory.CreateOne();
        int nHalf = modularFactors.Count / 2, i = 0;
        for (; i < nHalf; ++i)
            aFactor = aFactor.Multiply(modularFactors[i]);
        for (; i < modularFactors.Count; ++i)
            bFactor = bFactor.Multiply(modularFactors[i]);
        var hensel =
            liftFactory.CreateLift(modulus, poly, aFactor, bFactor);
        hensel.Lift(nIterations);
        var aFactorRaised = hensel.AFactorMod();
        var bFactorRaised = hensel.BFactorMod();
        List<UnivariatePolynomial<BigInteger>> result = [];
        result.AddRange(LiftFactorization0(modulus, finalModulus, nIterations,
            UnivariatePolynomialZ64.AsPolyZSymmetric(aFactorRaised), modularFactors[0..nHalf], liftFactory));
        result.AddRange(LiftFactorization0(modulus, finalModulus, nIterations,
            UnivariatePolynomialZ64.AsPolyZSymmetric(bFactorRaised), modularFactors[nHalf..modularFactors.Count],
            liftFactory));
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
        var nIterations = 0;
        var finalModulus = modulus;
        while (finalModulus.CompareTo(desiredBound) < 0)
        {
            finalModulus = finalModulus * (quadratic ? finalModulus : modulus);
            ++nIterations;
        }

        return new LiftingInfo(nIterations, finalModulus);
    }


    public static List<UnivariatePolynomial<BigInteger>> LiftFactorization(BigInteger modulus, BigInteger desiredBound,
        UnivariatePolynomial<BigInteger> poly, List<UnivariatePolynomialZp64> modularFactors, bool quadratic)
    {
        if (!quadratic && !modulus.IsLong())
            throw new ArgumentException("Only max 64-bit modulus for linear lift allowed.");
        var im = NIterations(modulus, desiredBound, quadratic);
        if (im.nIterations == 0)
            return modularFactors.Select(f => f.ToBigPoly()).ToList();
        var factory = quadratic ? 
            new LiftFactory<long>(HenselLifting.CreateQuadraticLift) : 
            new LiftFactory<long>(HenselLifting.CreateLinearLift);
        return LiftFactorization0(modulus, im.finalModulus, im.nIterations, poly, modularFactors, factory);
    }


    public static List<UnivariatePolynomial<BigInteger>> LiftFactorizationQuadratic(BigInteger modulus,
        BigInteger desiredBound, UnivariatePolynomial<BigInteger> poly,
        List<UnivariatePolynomial<BigInteger>> modularFactors)
    {
        var im = NIterations(modulus, desiredBound, true);
        return LiftFactorization0(modulus, im.finalModulus, im.nIterations, poly, modularFactors,
            new LiftFactory<BigInteger>(HenselLifting.CreateQuadraticLift));
    }


    public static List<UnivariatePolynomial<BigInteger>> LiftFactorization(BigInteger modulus, BigInteger desiredBound,
        UnivariatePolynomial<BigInteger> poly, List<UnivariatePolynomialZp64> modularFactors)
    {
        return LiftFactorization(poly, modularFactors, new AdaptiveLift(modulus, desiredBound));
    }


    private static List<UnivariatePolynomial<BigInteger>> LiftFactorization(UnivariatePolynomial<BigInteger> poly,
        List<UnivariatePolynomialZp64> modularFactors, AdaptiveLift lifter)
    {
        // for the future:
        // recursion may be replaced with precomputed binary tree
        // for now the major part of execution time (~99%) is spent in actual lifting step, so irrelevant
        if (modularFactors.Count == 1)
            return [poly.SetRing(new IntegersZp(lifter.finalModulus)).Monic()];
        var factory = modularFactors[0];
        UnivariatePolynomialZp64 aFactor = factory.CreateOne(), bFactor = factory.CreateOne();
        int nHalf = modularFactors.Count / 2, i = 0;
        for (; i < nHalf; ++i)
            aFactor = aFactor.Multiply(modularFactors[i]);
        for (; i < modularFactors.Count; ++i)
            bFactor = bFactor.Multiply(modularFactors[i]);
        UnivariatePolynomial<BigInteger>[] lifted = lifter.Lift(poly, aFactor, bFactor);
        var aFactorRaised = lifted[0];
        var bFactorRaised = lifted[1];
        List<UnivariatePolynomial<BigInteger>> result = [];
        result.AddRange(LiftFactorization(UnivariatePolynomialZ64.AsPolyZSymmetric(aFactorRaised),
            modularFactors[0..nHalf], lifter));
        result.AddRange(LiftFactorization(UnivariatePolynomialZ64.AsPolyZSymmetric(bFactorRaised),
            modularFactors[nHalf..modularFactors.Count], lifter));
        return result;
    }

    private static readonly int SWITCH_TO_QUADRATIC_LIFT = 64;

    private sealed class AdaptiveLift
    {
        readonly BigInteger initialModulus;
        public readonly BigInteger finalModulus;
        readonly int nLinearIterations, nQuadraticIterations;

        public AdaptiveLift(BigInteger initialModulus, BigInteger desiredBound)
        {
            this.initialModulus = initialModulus;
            var nLinearIterations = NIterations(initialModulus, desiredBound, false);
            if (nLinearIterations.nIterations < SWITCH_TO_QUADRATIC_LIFT)
            {
                this.nLinearIterations = nLinearIterations.nIterations;
                this.nQuadraticIterations = -1;
                this.finalModulus = nLinearIterations.finalModulus;
            }
            else
            {
                var nQuadraticIterations = NIterations(initialModulus, desiredBound, true);
                this.nLinearIterations = -1;
                this.nQuadraticIterations = nQuadraticIterations.nIterations;
                this.finalModulus = nQuadraticIterations.finalModulus;
            }
        }

        public UnivariatePolynomial<BigInteger>[] Lift(UnivariatePolynomial<BigInteger> poly,
            UnivariatePolynomialZp64 a, UnivariatePolynomialZp64 b)
        {
            var quadratic = nLinearIterations == -1;
            LiftableQuintet<BigInteger> lift = quadratic
                ? CreateQuadraticLift(initialModulus, poly, a.ToBigPoly(), b.ToBigPoly())
                : CreateLinearLift(initialModulus, poly, a, b);
            lift.Lift(quadratic ? nQuadraticIterations : nLinearIterations);
            return new UnivariatePolynomial<BigInteger>[]
            {
                lift.AFactorMod(),
                lift.BFactorMod()
            };
        }
    }

    private static void AssertHenselLift<T>(LiftableQuintet<T> lift)
    {
    }

    /* ************************************ Quadratic lifts ************************************ */


    public abstract class QuadraticLiftAbstract<E> : LiftableQuintet<E>
    {
        protected UnivariatePolynomial<E> aFactor, bFactor;


        protected UnivariatePolynomial<E> aCoFactor, bCoFactor;

        public QuadraticLiftAbstract(UnivariatePolynomial<E> aFactor, UnivariatePolynomial<E> bFactor,
            UnivariatePolynomial<E> aCoFactor, UnivariatePolynomial<E> bCoFactor)
        {
            this.aFactor = aFactor;
            this.bFactor = bFactor;
            this.aCoFactor = aCoFactor;
            this.bCoFactor = bCoFactor;
        }

        public abstract UnivariatePolynomial<E> PolyMod();

        public virtual UnivariatePolynomial<E> AFactorMod()
        {
            return aFactor;
        }


        public virtual UnivariatePolynomial<E> BFactorMod()
        {
            return bFactor;
        }

        public virtual UnivariatePolynomial<E> ACoFactorMod()
        {
            return aCoFactor;
        }

        public virtual UnivariatePolynomial<E> BCoFactorMod()
        {
            return bCoFactor;
        }

        public abstract void Prepare();

        public void Lift()
        {
            Prepare();
            HenselStep0(PolyMod());
        }

        public void LiftLast()
        {
            Prepare();
            HenselLastStep0(PolyMod());
        }

        private void HenselStep0(UnivariatePolynomial<E> baseMod)
        {
            var e = baseMod.Subtract(aFactor.Clone().Multiply(bFactor));
            var qr = UnivariateDivision.DivideAndRemainder(aCoFactor.Clone().Multiply(e), bFactor, false);
            var q = qr[0];
            var r = qr[1];
            var aFactorNew = aFactor.Clone().Add(bCoFactor.Clone().Multiply(e)).Add(aFactor.Clone().Multiply(q));
            var bFactorNew = bFactor.Clone().Add(r);
            var b = aCoFactor.Clone().Multiply(aFactorNew).Add(bCoFactor.Clone().Multiply(bFactorNew)).Decrement();
            var cd = UnivariateDivision.DivideAndRemainder(aCoFactor.Clone().Multiply(b), bFactorNew, false);
            var c = cd[0];
            var d = cd[1];
            var aCoFactorNew = aCoFactor.Subtract(d);
            var bCoFactorNew = bCoFactor.Subtract(bCoFactor.Clone().Multiply(b))
                .Subtract(c.Clone().Multiply(aFactorNew));
            aFactor = aFactorNew;
            aCoFactor = aCoFactorNew;
            bFactor = bFactorNew;
            bCoFactor = bCoFactorNew;
        }


        private void HenselLastStep0(UnivariatePolynomial<E> baseMod)
        {
            var e = baseMod.Subtract(aFactor.Clone().Multiply(bFactor));
            var qr = UnivariateDivision.DivideAndRemainder(aCoFactor.Multiply(e), bFactor, false);
            var q = qr[0];
            var r = qr[1];
            var aFactorNew = aFactor.Add(bCoFactor.Multiply(e)).Add(aFactor.Clone().Multiply(q));
            var bFactorNew = bFactor.Add(r);
            aFactor = aFactorNew;
            aCoFactor = null;
            bFactor = bFactorNew;
            bCoFactor = null;
        }
    }


    public sealed class lQuadraticLift : QuadraticLiftAbstract<long>
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


        public override UnivariatePolynomialZp64 PolyMod()
        {
            return @base.Modulus(modulus, true);
        }


        public override void Prepare()
        {
            modulus = MachineArithmetic.SafeMultiply(modulus, modulus);
            aFactor = aFactor.SetModulusUnsafe(modulus);
            bFactor = bFactor.SetModulusUnsafe(modulus);
            aCoFactor = aCoFactor.SetModulusUnsafe(modulus);
            bCoFactor = bCoFactor.SetModulusUnsafe(modulus);
        }
    }


    public sealed class bQuadraticLift : QuadraticLiftAbstract<BigInteger>
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


        public override UnivariatePolynomial<BigInteger> PolyMod()
        {
            return @base.SetRing(ring);
        }


        public override void Prepare()
        {
            ring = new IntegersZp(ring.modulus * ring.modulus);
            aFactor = aFactor.SetRingUnsafe(ring);
            bFactor = bFactor.SetRingUnsafe(ring);
            aCoFactor = aCoFactor.SetRingUnsafe(ring);
            bCoFactor = bCoFactor.SetRingUnsafe(ring);
        }
    }

    /* ************************************ Linear lifts ************************************ */
    public class LinearLiftAbstract<E>
    {
        public readonly UnivariatePolynomial<E> poly;


        public UnivariatePolynomial<E> aFactor;


        public UnivariatePolynomial<E> bFactor;


        public UnivariatePolynomial<E> aCoFactor;


        public UnivariatePolynomial<E> bCoFactor;


        readonly UnivariatePolynomialZp64 aFactorMod, aFactorModMonic, bFactorMod, aCoFactorMod, bCoFactorMod;


        readonly UnivariateDivision.InverseModMonomial<long> aFactorModMonicInv, bFactorModInv;


        public LinearLiftAbstract(UnivariatePolynomial<E> poly, UnivariatePolynomial<E> aFactor,
            UnivariatePolynomial<E> bFactor, UnivariatePolynomial<E> aCoFactor, UnivariatePolynomial<E> bCoFactor,
            UnivariatePolynomialZp64 aFactorMod, UnivariatePolynomialZp64 aFactorModMonic,
            UnivariatePolynomialZp64 bFactorMod, UnivariatePolynomialZp64 aCoFactorMod,
            UnivariatePolynomialZp64 bCoFactorMod)
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


        protected UnivariatePolynomialZp64 aAdd, bAdd;


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


    public sealed class lLinearLift : LinearLiftAbstract<long>,
        LiftableQuintet<long>
    {
        public readonly long initialModulus;


        public long modulus;


        private lLinearLift(long modulus, UnivariatePolynomialZ64 poly, UnivariatePolynomialZp64 aFactor,
            UnivariatePolynomialZp64 aFactorMonic, UnivariatePolynomialZp64 bFactor, UnivariatePolynomialZp64 aCoFactor,
            UnivariatePolynomialZp64 bCoFactor) : base(poly, EnsureMonic(aFactor).AsPolyZ(true).Multiply(poly.Lc()),
            bFactor.AsPolyZ(true), aCoFactor.AsPolyZ(true), bCoFactor.AsPolyZ(true), aFactor, aFactorMonic, bFactor,
            aCoFactor, bCoFactor)
        {
            this.initialModulus = modulus;
            this.modulus = modulus;
        }


        public lLinearLift(long modulus, UnivariatePolynomialZ64 poly, UnivariatePolynomialZp64 aFactor,
            UnivariatePolynomialZp64 bFactor, UnivariatePolynomialZp64 aCoFactor,
            UnivariatePolynomialZp64 bCoFactor) : this(modulus, poly, aFactor, aFactor.Clone().Monic(), bFactor,
            aCoFactor, bCoFactor)
        {
        }


        public UnivariatePolynomialZp64 PolyMod()
        {
            return poly.Modulus(modulus);
        }


        public UnivariatePolynomialZp64 AFactorMod()
        {
            return aFactor.Modulus(modulus);
        }


        public UnivariatePolynomialZp64 BFactorMod()
        {
            return bFactor.Modulus(modulus);
        }


        public UnivariatePolynomialZp64 ACoFactorMod()
        {
            return aCoFactor == null ? null : aCoFactor.Modulus(modulus);
        }


        public UnivariatePolynomialZp64 BCoFactorMod()
        {
            return bCoFactor == null ? null : bCoFactor.Modulus(modulus);
        }


        private void LiftFactors()
        {
            var factorsDiff = poly.Clone().Subtract(aFactor.Clone().Multiply(bFactor))
                .DivideOrNull(modulus).Modulus(initialModulus);
            CalculateFactorsDiff(factorsDiff);
            aFactor = aFactor.Add(aAdd.AsPolyZ(false).Multiply(modulus));
            bFactor = bFactor.Add(bAdd.AsPolyZ(false).Multiply(modulus));
        }


        private void LiftCoFactors()
        {
            var coFactorsDiff = aCoFactor.Clone().Multiply(aFactor)
                .Add(bCoFactor.Clone().Multiply(bFactor)).Decrement().Negate().DivideOrNull(modulus)
                .Modulus(initialModulus);
            CalculateCoFactorsDiff(coFactorsDiff);
            aCoFactor = aCoFactor.Add(aAdd.AsPolyZ(false).Multiply(modulus));
            bCoFactor = bCoFactor.Add(bAdd.AsPolyZ(false).Multiply(modulus));
        }


        public void Lift()
        {
            LiftFactors();
            LiftCoFactors();
            modulus = MachineArithmetic.SafeMultiply(modulus, initialModulus);
        }


        public void LiftLast()
        {
            LiftFactors();
            modulus = MachineArithmetic.SafeMultiply(modulus, initialModulus);
            aCoFactor = bCoFactor = null;
        }
    }


    public sealed class bLinearLift : LinearLiftAbstract<BigInteger>,
        LiftableQuintet<BigInteger>
    {
        public readonly IntegersZp initialDomain;


        public IntegersZp ring;


        private bLinearLift(BigInteger modulus, UnivariatePolynomial<BigInteger> poly, UnivariatePolynomialZp64 aFactor,
            UnivariatePolynomialZp64 aFactorMonic, UnivariatePolynomialZp64 bFactor, UnivariatePolynomialZp64 aCoFactor,
            UnivariatePolynomialZp64 bCoFactor) : base(poly,
            EnsureMonic(aFactor).AsPolyZ(true).ToBigPoly().Multiply(poly.Lc()), bFactor.AsPolyZ(false).ToBigPoly(),
            aCoFactor.AsPolyZ(false).ToBigPoly(), bCoFactor.AsPolyZ(false).ToBigPoly(), aFactor, aFactorMonic, bFactor,
            aCoFactor, bCoFactor)
        {
            this.initialDomain = new IntegersZp(modulus);
            this.ring = new IntegersZp(modulus);
        }


        public bLinearLift(BigInteger modulus, UnivariatePolynomial<BigInteger> poly, UnivariatePolynomialZp64 aFactor,
            UnivariatePolynomialZp64 bFactor, UnivariatePolynomialZp64 aCoFactor,
            UnivariatePolynomialZp64 bCoFactor) : this(modulus, poly, aFactor, aFactor.Clone().Monic(), bFactor,
            aCoFactor, bCoFactor)
        {
        }


        public UnivariatePolynomial<BigInteger> PolyMod()
        {
            return poly.SetRing(ring);
        }


        public UnivariatePolynomial<BigInteger> AFactorMod()
        {
            return aFactor.SetRing(ring);
        }


        public UnivariatePolynomial<BigInteger> BFactorMod()
        {
            return bFactor.SetRing(ring);
        }


        public UnivariatePolynomial<BigInteger> ACoFactorMod()
        {
            return aCoFactor == null ? null : aCoFactor.SetRing(ring);
        }


        public UnivariatePolynomial<BigInteger> BCoFactorMod()
        {
            return bCoFactor == null ? null : bCoFactor.SetRing(ring);
        }


        private void LiftFactors()
        {
            var factorsDiff = UnivariatePolynomialZ64.AsOverZp64(poly.Clone()
                .Subtract(aFactor.Clone().Multiply(bFactor)).DivideOrNull(ring.modulus).SetRing(initialDomain));
            CalculateFactorsDiff(factorsDiff);
            aFactor = aFactor.Add(aAdd.AsPolyZ(false).ToBigPoly().Multiply(ring.modulus));
            bFactor = bFactor.Add(bAdd.AsPolyZ(false).ToBigPoly().Multiply(ring.modulus));
        }


        private void LiftCoFactors()
        {
            var coFactorsDiff = UnivariatePolynomialZ64.AsOverZp64(aCoFactor.Clone().Multiply(aFactor)
                .Add(bCoFactor.Clone().Multiply(bFactor)).Decrement().Negate().DivideOrNull(ring.modulus)
                .SetRing(initialDomain));
            CalculateCoFactorsDiff(coFactorsDiff);
            aCoFactor = aCoFactor.Add(aAdd.AsPolyZ(false).ToBigPoly().Multiply(ring.modulus));
            bCoFactor = bCoFactor.Add(bAdd.AsPolyZ(false).ToBigPoly().Multiply(ring.modulus));
        }


        public void Lift()
        {
            LiftFactors();
            LiftCoFactors();
            ring = new IntegersZp(ring.modulus * (initialDomain.modulus));
        }


        public void LiftLast()
        {
            LiftFactors();
            ring = new IntegersZp(ring.modulus * (initialDomain.modulus));
            aCoFactor = bCoFactor = null;
        }
    }
}