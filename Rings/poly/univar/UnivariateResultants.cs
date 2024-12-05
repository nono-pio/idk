using System.Diagnostics;
using System.Numerics;
using Rings.primes;

namespace Rings.poly.univar;

public static class UnivariateResultants
{
    public static Poly DiscriminantAsPoly<Poly>(Poly a) where Poly : IUnivariatePolynomial<Poly>
    {
        if (a is UnivariatePolynomialZp64)
            return (Poly)((UnivariatePolynomialZp64)a).createConstant(Discriminant((UnivariatePolynomialZp64)a));
        else
            return (Poly)((UnivariatePolynomial)a).createConstant(Discriminant((UnivariatePolynomial)a));
    }


    public static E Discriminant<E>(UnivariatePolynomial<E> a)
    {
        Ring<E> ring = a.ring;
        E disc = ring.divideExact(Resultant(a, a.derivative()), a.lc());
        return ((a.Degree * (a.Degree - 1) / 2) % 2 == 1) ? ring.negate(disc) : disc;
    }


    public static long Discriminant(UnivariatePolynomialZp64 a)
    {
        IntegersZp64 ring = a.ring;
        long disc = ring.divide(Resultant(a, a.derivative()), a.lc());
        return ((a.Degree * (a.Degree - 1) / 2) % 2 == 1) ? ring.negate(disc) : disc;
    }


    public static Poly ResultantAsPoly<Poly>(Poly a, Poly b) where Poly : IUnivariatePolynomial<Poly>
    {
        if (a is UnivariatePolynomialZp64)
            return (Poly)((UnivariatePolynomialZp64)a).createConstant(Resultant((UnivariatePolynomialZp64)a,
                (UnivariatePolynomialZp64)b));
        else
            return (Poly)((UnivariatePolynomial)a).createConstant(Resultant((UnivariatePolynomial)a,
                (UnivariatePolynomial)b));
    }


    public static E Resultant<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        if (Util.isOverMultipleFieldExtension(a))
            return (E)ResultantInMultipleFieldExtension((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        else if (a.isOverFiniteField())
            return ClassicalPRS(a, b).resultant();
        else if (Util.isOverRationals(a))
            return (E)ResultantInQ((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        else if (a.isOverZ())
            return (E)ModularResultant((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        else if (Util.isOverSimpleNumberField(a))
            return (E)ModularResultantInNumberField((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        else if (Util.isOverRingOfIntegersOfSimpleNumberField(a))
            return (E)ModularResultantInRingOfIntegersOfNumberField((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        else
            return PrimitiveResultant(a, b, (p, q) => SubresultantPRS(p, q).resultant());
    }


    public static long Resultant(UnivariatePolynomialZp64 a, UnivariatePolynomialZp64 b)
    {
        return ClassicalPRS(a, b).resultant();
    }

    private static Rational<E> ResultantInQ<E>(UnivariatePolynomial<Rational<E>> a,
        UnivariatePolynomial<Rational<E>> b)
    {
        Util.Tuple2<UnivariatePolynomial<E>, E>
            aZ = Util.toCommonDenominator(a),
            bZ = Util.toCommonDenominator(b);

        Ring<E> ring = aZ._1.ring;

        E resultant = Resultant(aZ._1, bZ._1);
        E den = ring.multiply(
            ring.pow(aZ._2, b.Degree),
            ring.pow(bZ._2, a.Degree));
        return new Rational<E>(ring, resultant, den);
    }

    private static <
    Term extends AMonomial<Term>,
    mPoly extends AMultivariatePolynomial<Term, mPoly>,
    sPoly extends IUnivariatePolynomial<sPoly>
    > mPoly
        ResultantInMultipleFieldExtension(UnivariatePolynomial<mPoly> a, UnivariatePolynomial<mPoly> b)
    {
        MultipleFieldExtension<Term, mPoly, sPoly> ring = (MultipleFieldExtension<Term, mPoly, sPoly>)a.ring;
        SimpleFieldExtension<sPoly> simpleExtension = ring.getSimpleExtension();
        return ring.image(Resultant(
            a.mapCoefficients(simpleExtension, ring::inverse),
            b.mapCoefficients(simpleExtension, ring::inverse)));
    }


    public static List<E> Subresultants<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        if (a.isOverField())
            return ClassicalPRS(a, b).getSubresultants();
        else
            return SubresultantPRS(a, b).getSubresultants();
    }

    static E PrimitiveResultant<E>(UnivariatePolynomial<E> a,
        UnivariatePolynomial<E> b,
        BiFunction<UnivariatePolynomial<E>, UnivariatePolynomial<E>, E> algorithm)
    {
        E ac = a.content(), bc = b.content();
        a = a.clone().divideExact(ac);
        b = b.clone().divideExact(bc);
        E r = algorithm.apply(a, b);
        Ring<E> ring = a.ring;
        r = ring.multiply(r, ring.pow(ac, b.Degree));
        r = ring.multiply(r, ring.pow(bc, a.Degree));
        return r;
    }


    public static BigInteger ModularResultant(UnivariatePolynomial<BigInteger> a,
        UnivariatePolynomial<BigInteger> b)
    {
        return PrimitiveResultant(a, b, UnivariateResultants::ModularResultant0);
    }

    private static BigInteger ModularResultant0(UnivariatePolynomial<BigInteger> a,
        UnivariatePolynomial<BigInteger> b)
    {
        // bound on the value of resultant
        BigInteger bound = BigInteger.Pow(UnivariatePolynomial<BigInteger>.norm2(a), b.Degree)
            * BigInteger.Pow(UnivariatePolynomial<BigInteger>.norm2(b), a.Degree) << 1;

        // aggregated CRT modulus
        BigInteger bModulus = null;
        BigInteger resultant = null;
        PrimesIterator primes = new PrimesIterator(1L << 25);
        while (true)
        {
            long prime = primes.take();
            BigInteger bPrime = new BigInteger(prime);
            IntegersZp zpRing = Rings.Zp(prime);
            UnivariatePolynomialZp64
                aMod = asOverZp64(a.setRing(zpRing)),
                bMod = asOverZp64(b.setRing(zpRing));

            if (aMod.Degree != a.Degree || bMod.Degree != b.Degree)
                continue; // unlucky prime

            long resultantMod = ClassicalPRS(aMod, bMod).resultant();
            BigInteger bResultantMod = new BigInteger(resultantMod);
            if (bModulus == null)
            {
                bModulus = bPrime;
                resultant = bResultantMod;
                continue;
            }

            if (!resultant.IsZero && resultantMod == 0)
                continue; // unlucky prime

            resultant = ChineseRemainders.ChineseRemainders(bModulus, bPrime, resultant, bResultantMod);
            bModulus = bModulus * new BigInteger(prime);

            if (bModulus.CompareTo(bound) > 0)
                return Rings.Zp(bModulus).symmetricForm(resultant);
        }
    }

    private static UnivariatePolynomial<E>
        TrivialResultantInNumberField<E>(UnivariatePolynomial<UnivariatePolynomial<E>> a,
            UnivariatePolynomial<UnivariatePolynomial<E>> b)
    {
        AlgebraicNumberField<UnivariatePolynomial<E>> ring
            = (AlgebraicNumberField<UnivariatePolynomial<E>>)a.ring;

        if (!a.stream().allMatch(ring::isInTheBaseField)
            || !b.stream().allMatch(ring::isInTheBaseField))
            return null;

        UnivariatePolynomial<E>
            ar = a.mapCoefficients(ring.getMinimalPolynomial().ring, UnivariatePolynomial::cc),
            br = b.mapCoefficients(ring.getMinimalPolynomial().ring, UnivariatePolynomial::cc);
        return UnivariatePolynomial<E>.constant(ring.getMinimalPolynomial().ring, Resultant(ar, br));
    }


    public static UnivariatePolynomial<Rational<BigInteger>> ModularResultantInNumberField(
        UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a,
        UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> b)
    {
        UnivariatePolynomial<Rational<BigInteger>> r = TrivialResultantInNumberField(a, b);
        if (r != null)
            return r;

        AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> numberField =
            (AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>>)a.ring;
        UnivariatePolynomial<Rational<BigInteger>> minimalPoly = numberField.getMinimalPolynomial();

        Debug.Assert(numberField.isField());

        a = a.clone();
        b = b.clone();

        // reduce problem to the case with integer monic minimal polynomial
        if (minimalPoly.stream().allMatch(Rational::isIntegral))
        {
            // minimal poly is already monic & integer

            UnivariatePolynomial<BigInteger> minimalPolyZ = minimalPoly.mapCoefficients(Z, e => e.numerator);
            AlgebraicNumberField<UnivariatePolynomial<BigInteger>> numberFieldZ =
                new AlgebraicNumberField<UnivariatePolynomial<BigInteger>>(minimalPolyZ);

            BigInteger
                aDen = removeDenominators(a),
                bDen = removeDenominators(b),
                den = BigInteger.Pow(aDen, b.Degree) * BigInteger.Pow(bDen, a.Degree);

            Debug.Assert(a.stream().All(p => p.stream().All(e => e.isIntegral())));
            Debug.Assert(b.stream().All(p => p.stream().All(e => e.isIntegral())));

            return ModularResultantInRingOfIntegersOfNumberField(
                    a.mapCoefficients(numberFieldZ, cf => cf.mapCoefficients(Z, Rational::numerator)),
                    b.mapCoefficients(numberFieldZ, cf => cf.mapCoefficients(Z, Rational::numerator)))
                .mapCoefficients(Q, cf => Q.mk(cf, den));
        }
        else
        {
            // replace s -> s / lc(minPoly)
            BigInteger minPolyLeadCoeff = toCommonDenominator(minimalPoly)._1.lc();
            Rational<BigInteger>
                scale = new Rational<BigInteger>(Z, Z.getOne(), minPolyLeadCoeff),
                scaleReciprocal = scale.reciprocal();

            AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>>
                scaledNumberField =
                    new AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>>(minimalPoly.scale(scale)
                        .monic());
            return ModularResultantInNumberField(
                    a.mapCoefficients(scaledNumberField, cf => cf.scale(scale)),
                    b.mapCoefficients(scaledNumberField, cf => cf.scale(scale)))
                .scale(scaleReciprocal);
        }
    }

    public static BigInteger polyPowNumFieldCfBound(BigInteger maxCf, BigInteger maxMinPolyCf, int minPolyDeg,
        int exponent)
    {
        return BigInteger.Pow(new BigInteger(minPolyDeg), exponent - 1)
               * BigInteger.Pow(maxCf, exponent) * BigInteger.Pow(maxMinPolyCf + 1, (exponent - 1) * (minPolyDeg + 1));
    }


    public static UnivariatePolynomial<BigInteger> ModularResultantInRingOfIntegersOfNumberField(
        UnivariatePolynomial<UnivariatePolynomial<BigInteger>> a,
        UnivariatePolynomial<UnivariatePolynomial<BigInteger>> b)
    {
        return PrimitiveResultant(a, b, UnivariateResultants::ModularResultantInRingOfIntegersOfNumberField0);
    }

    private static UnivariatePolynomial<BigInteger> ModularResultantInRingOfIntegersOfNumberField0(
        UnivariatePolynomial<UnivariatePolynomial<BigInteger>> a,
        UnivariatePolynomial<UnivariatePolynomial<BigInteger>> b)
    {
        UnivariatePolynomial<BigInteger> r = TrivialResultantInNumberField(a, b);
        if (r != null)
            return r;

        AlgebraicNumberField<UnivariatePolynomial<BigInteger>> numberField =
            (AlgebraicNumberField<UnivariatePolynomial<BigInteger>>)a.ring;
        UnivariatePolynomial<BigInteger> minimalPoly = numberField.getMinimalPolynomial();


        BigInteger
            aMax = a.stream().flatMap(UnivariatePolynomial.stream).map(Rings.Z.abs).max(Rings.Z)
                .orElse(BigInteger.Zero),
            bMax = b.stream().flatMap(UnivariatePolynomial.stream).map(Rings.Z.abs).max(Rings.Z)
                .orElse(BigInteger.Zero),
            mMax = minimalPoly.maxAbsCoefficient();

        // bound on the value of resultant coefficients
        BigInteger bound =
            polyPowNumFieldCfBound(aMax, mMax, minimalPoly.Degree, b.Degree)
                .multiply(polyPowNumFieldCfBound(bMax, mMax, minimalPoly.Degree, a.Degree));

        // aggregated CRT modulus
        BigInteger bModulus = null;
        UnivariatePolynomial<BigInteger> resultant = null;
        PrimesIterator primes = new PrimesIterator(1L << 25);
        while (true)
        {
            long prime = primes.take();
            IntegersZp64 zpRing = Rings.Zp64(prime);

            UnivariatePolynomialZp64 minimalPolyMod = asOverZp64(minimalPoly, zpRing);
            FiniteField<UnivariatePolynomialZp64> numberFieldMod = new FiniteField<>(minimalPolyMod);

            UnivariatePolynomial<UnivariatePolynomialZp64>
                aMod = a.mapCoefficients(numberFieldMod, cf => asOverZp64(cf, zpRing)),
                bMod = b.mapCoefficients(numberFieldMod, cf => asOverZp64(cf, zpRing));

            if (aMod.Degree != a.Degree || bMod.Degree != b.Degree)
                continue; // unlucky prime

            UnivariatePolynomialZp64 resultantMod = ClassicalPRS(aMod, bMod).resultant();
            if (bModulus == null)
            {
                bModulus = new BigInteger(prime);
                resultant = resultantMod.toBigPoly();
                continue;
            }

            if (!resultant.isZero() && resultantMod.isZero())
                continue; // unlucky prime

            UnivariateGCD.updateCRT(ChineseRemainders.createMagic(Rings.Z, bModulus, new BigInteger(prime)), resultant,
                resultantMod);
            bModulus = bModulus * new BigInteger(prime);

            if (bModulus.CompareTo(bound) > 0)
                return UnivariatePolynomial<BigInteger>.asPolyZSymmetric(resultant.setRingUnsafe(Rings.Zp(bModulus)));
        }
    }


    public static PolynomialRemainderSequenceZp64 ClassicalPRS(UnivariatePolynomialZp64 a, UnivariatePolynomialZp64 b)
    {
        return new PolynomialRemainderSequenceZp64(a, b).run();
    }


    public static PolynomialRemainderSequence<E> ClassicalPRS<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return new ClassicalPolynomialRemainderSequence<E>(a, b).run();
    }


    public static PolynomialRemainderSequence<E> PseudoPRS<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return new PseudoPolynomialRemainderSequence<E>(a, b).run();
    }


    public static PolynomialRemainderSequence<E> PrimitivePRS<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return new PrimitivePolynomialRemainderSequence<E>(a, b).run();
    }


    public static PolynomialRemainderSequence<E> ReducedPRS<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return new ReducedPolynomialRemainderSequence<E>(a, b).run();
    }


    public static PolynomialRemainderSequence<E> SubresultantPRS<E>(UnivariatePolynomial<E> a,
        UnivariatePolynomial<E> b)
    {
        return new SubresultantPolynomialRemainderSequence<E>(a, b).run();
    }


    public class APolynomialRemainderSequence<Poly> where Poly : IUnivariatePolynomial<Poly>
    {
        public readonly List<Poly> remainders = new List<Poly>();

        public readonly List<Poly> quotients = new List<Poly>();

        public readonly Poly a, b;

        public APolynomialRemainderSequence(Poly a, Poly b)
        {
            this.a = a;
            this.b = b;
        }


        public Poly lastRemainder()
        {
            return remainders[remainders.Count - 1];
        }


        public Poly gcd()
        {
            if (a.isZero()) return b;
            if (b.isZero()) return a;

            if (a.isOverField())
                return lastRemainder().clone().monic();

            Poly r = lastRemainder().clone().primitivePartSameSign();
            return UnivariateGCD.PolynomialGCD(a.contentAsPoly(), b.contentAsPoly()).multiply(r);
        }

        public int size()
        {
            return remainders.Count;
        }
    }


    public abstract class PolynomialRemainderSequence<E> : APolynomialRemainderSequence<UnivariatePolynomial<E>>
    {
        public readonly List<E> alphas = new List<E>();

        public readonly List<E> betas = new List<E>();

        public readonly Ring<E> ring;

        readonly bool swap;

        public PolynomialRemainderSequence(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) : base(a, b)
        {
            this.ring = a.ring;
            if (a.Degree >= b.Degree)
            {
                remainders.Add(a);
                remainders.Add(b);
                swap = false;
            }
            else
            {
                remainders.Add(b);
                remainders.Add(a);
                swap = a.Degree % 2 == 1 &&
                       b.Degree % 2 == 1; // both Degrees are odd => odd permutation of Sylvester matrix
            }
        }


        public abstract E nextAlpha();


        public abstract E nextBeta(UnivariatePolynomial<E> remainder);


        private UnivariatePolynomial<E> step()
        {
            int i = remainders.Count;
            UnivariatePolynomial<E>
                dividend = remainders[i - 2].clone(),
                divider = remainders[i - 1];

            E alpha = nextAlpha();
            dividend = dividend.multiply(alpha);

            UnivariatePolynomial<E>[] qd = UnivariateDivision.divideAndRemainder(dividend, divider, false);
            if (qd == null)
                throw new Exception("exact division is not possible");

            UnivariatePolynomial<E> quotient = qd[0], remainder = qd[1];
            if (remainder.isZero())
                // remainder is zero => termination of the algorithm
                return remainder;

            E beta = nextBeta(remainder);
            remainder = remainder.divideExact(beta);

            alphas.Add(alpha);
            betas.Add(beta);
            quotients.Add(quotient);
            remainders.Add(remainder);
            return remainder;
        }


        public PolynomialRemainderSequence<E> run()
        {
            if (lastRemainder().isZero())
                // on of the factors is zero
                return this;
            while (!step().isZero()) ;
            return this;
        }


        public int DegreeDiff(int i)
        {
            return remainders[i].Degree - remainders[i + 1].Degree;
        }


        private readonly List<E> subresultants = new List<E>();

        synchronized void computeSubresultants()
        {
            if (subresultants.Count != 0)
                return;

            List<E> subresultants = nonZeroSubresultants();
            if (swap) subresultants.replaceAll(ring.negate);
            this.subresultants.ensureCapacity(remainders[1].Degree);
            for (int i = 0; i <= remainders[1].Degree; ++i)
                this.subresultants.Add(ring.getZero());
            for (int i = 1; i < remainders.Count; i++)
                this.subresultants[remainders[i].Degree] = subresultants[i - 1];
        }


        List<E> nonZeroSubresultants()
        {
            List<E> subresultants = new List<E>();
            // largest subresultant
            E subresultant = ring.pow(remainders[1].lc(), DegreeDiff(0));
            subresultants.Add(subresultant);

            for (int i = 1; i < (remainders.Count - 1); ++i)
            {
                // computing (i+1)-th Degree subresultant

                int di = DegreeDiff(i);
                E rho = ring.pow(ring.multiply(remainders[i + 1].lc(), remainders[i].lc()), di);
                E den = ring.getOne();
                for (int j = 1; j <= i; ++j)
                {
                    rho = ring.multiply(rho, ring.pow(betas[j - 1], di));
                    den = ring.multiply(den, ring.pow(alphas[j - 1], di));
                }

                if ((di % 2) == 1 && (remainders[0].Degree - remainders[i + 1].Degree + i + 1) % 2 == 1)
                    rho = ring.negate(rho);
                subresultant = ring.multiply(subresultant, rho);
                subresultant = ring.divideExact(subresultant, den);

                subresultants.Add(subresultant);
            }

            return subresultants;
        }


        public List<E> getSubresultants()
        {
            if (subresultants.Count == 0)
                computeSubresultants();
            return subresultants;
        }


        public E resultant()
        {
            return getSubresultants()[0];
        }
    }


    private sealed class ClassicalPolynomialRemainderSequence<E> : PolynomialRemainderSequence<E>
    {
        public ClassicalPolynomialRemainderSequence(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) : base(a, b)
        {
        }

        public override E nextAlpha()
        {
            return ring.getOne();
        }

        public override E nextBeta(UnivariatePolynomial<E> remainder)
        {
            return ring.getOne();
        }

        List<E> nonZeroSubresultants()
        {
            List<E> subresultants = new List<E>();
            // largest subresultant
            E subresultant = ring.pow(remainders[1].lc(), DegreeDiff(0));
            subresultants.Add(subresultant);

            for (int i = 1; i < (remainders.Count - 1); ++i)
            {
                // computing (i+1)-th Degree subresultant

                int di = DegreeDiff(i);
                E rho = ring.pow(ring.multiply(remainders[i + 1].lc(), remainders[i].lc()), di);
                if ((di % 2) == 1 && (remainders[0].Degree - remainders[i + 1].Degree + i + 1) % 2 == 1)
                    rho = ring.negate(rho);
                subresultant = ring.multiply(subresultant, rho);
                subresultants.Add(subresultant);
            }

            return subresultants;
        }
    }

    private class PseudoPolynomialRemainderSequence<E> : PolynomialRemainderSequence<E>
    {
        public PseudoPolynomialRemainderSequence(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) : base(a, b)
        {
        }

        public override E nextAlpha()
        {
            int i = remainders.Count;
            E lc = remainders[i - 1].lc();
            int deg = remainders[i - 2].Degree - remainders[i - 1].Degree;
            return ring.pow(lc, deg + 1);
        }

        public override E nextBeta(UnivariatePolynomial<E> remainder)
        {
            return ring.getOne();
        }
    }


    private sealed class ReducedPolynomialRemainderSequence<E> : PseudoPolynomialRemainderSequence<E>
    {
        public ReducedPolynomialRemainderSequence(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) : base(a, b)
        {
        }

        new E nextBeta(UnivariatePolynomial<E> remainder)
        {
            return alphas.Count == 0 ? ring.getOne() : alphas[alphas.Count - 1];
        }

        List<E> nonZeroSubresultants()
        {
            List<E> subresultants = new List<E>();
            // largest subresultant
            E subresultant = ring.pow(remainders[1].lc(), DegreeDiff(0));
            subresultants.Add(subresultant);

            for (int i = 1; i < (remainders.Count - 1); ++i)
            {
                // computing (i+1)-th Degree subresultant
                int di = DegreeDiff(i);
                E rho = ring.pow(remainders[i + 1].lc(), di);
                E den = ring.pow(remainders[i].lc(), DegreeDiff(i - 1) * di);
                subresultant = ring.multiply(subresultant, rho);
                subresultant = ring.divideExact(subresultant, den);
                if ((di % 2) == 1 && (remainders[0].Degree - remainders[i + 1].Degree + i + 1) % 2 == 1)
                    subresultant = ring.negate(subresultant);
                subresultants.Add(subresultant);
            }

            return subresultants;
        }
    }


    private sealed class PrimitivePolynomialRemainderSequence<E> : PseudoPolynomialRemainderSequence<E>
    {
        public PrimitivePolynomialRemainderSequence(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) : base(a, b)
        {
        }

        new E nextBeta(UnivariatePolynomial<E> remainder)
        {
            return remainder.content();
        }
    }


    private sealed class SubresultantPolynomialRemainderSequence<E> : PseudoPolynomialRemainderSequence<E>
    {
        readonly List<E> psis = new List<E>();

        public SubresultantPolynomialRemainderSequence(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) : base(a,
            b)
        {
        }

        new E nextBeta(UnivariatePolynomial<E> remainder)
        {
            int i = remainders.Count;
            UnivariatePolynomial<E> prem = remainders[i - 2];
            E lc = i == 2 ? ring.getOne() : prem.lc();
            E psi;
            if (i == 2)
                psi = ring.getNegativeOne();
            else
            {
                E prevPsi = psis[psis.Count - 1];
                int deg = remainders[i - 1].Degree - remainders[i - 2].Degree;
                E f = ring.pow(ring.negate(lc), deg);
                if (1 - deg < 0)
                    psi = ring.divideExact(f, ring.pow(prevPsi, deg - 1));
                else
                    psi = ring.multiply(f, ring.pow(prevPsi, 1 - deg));
            }

            psis.Add(psi);
            return ring.negate(ring.multiply(lc, ring.pow(psi, remainders[i - 2].Degree - remainders[i - 1].Degree)));
        }

        private int eij(int i, int j)
        {
            int e = DegreeDiff(j - 1);
            for (int k = j; k <= i; ++k)
                e *= 1 - DegreeDiff(k);
            return e;
        }

        List<E> nonZeroSubresultants()
        {
            List<E> subresultants = new List<E>();
            // largest subresultant
            E subresultant = ring.pow(remainders[1].lc(), DegreeDiff(0));
            subresultants.Add(subresultant);

            for (int i = 1; i < (remainders.Count - 1); ++i)
            {
                // computing (i+1)-th Degree subresultant

                int di = DegreeDiff(i);
                E rho = ring.pow(remainders[i + 1].lc(), di);
                E den = ring.getOne();
                for (int k = 1; k <= i; ++k)
                {
                    int deg = -di * eij(i - 1, k);
                    if (deg >= 0)
                        rho = ring.multiply(rho, ring.pow(remainders[k].lc(), deg));
                    else
                        den = ring.multiply(den, ring.pow(remainders[k].lc(), -deg));
                }

                subresultant = ring.multiply(subresultant, rho);
                subresultant = ring.divideExact(subresultant, den);
                subresultants.Add(subresultant);
            }

            return subresultants;
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    public sealed class PolynomialRemainderSequenceZp64 : APolynomialRemainderSequence<UnivariatePolynomialZp64>
    {
        readonly IntegersZp64 ring;

        readonly bool swap;

        public PolynomialRemainderSequenceZp64(UnivariatePolynomialZp64 a, UnivariatePolynomialZp64 b) : base(a, b)
        {
            this.ring = a.ring;
            if (a.Degree >= b.Degree)
            {
                remainders.Add(a);
                remainders.Add(b);
                swap = false;
            }
            else
            {
                remainders.Add(b);
                remainders.Add(a);
                swap = a.Degree % 2 == 1 &&
                       b.Degree % 2 == 1; // both Degrees are odd => odd permutation of Sylvester matrix
            }
        }


        private UnivariatePolynomialZp64 step()
        {
            int i = remainders.Count;
            UnivariatePolynomialZp64
                dividend = remainders[i - 2].clone(),
                divider = remainders[i - 1];

            UnivariatePolynomialZp64[] qd = UnivariateDivision.divideAndRemainder(dividend, divider, false);
            if (qd == null)
                throw new Exception("exact division is not possible");

            UnivariatePolynomialZp64 quotient = qd[0], remainder = qd[1];
            if (remainder.isZero())
                // remainder is zero => termination of the algorithm
                return remainder;

            quotients.Add(quotient);
            remainders.Add(remainder);
            return remainder;
        }


        public PolynomialRemainderSequenceZp64 run()
        {
            while (!step().isZero()) ;
            return this;
        }


        int DegreeDiff(int i)
        {
            return remainders[i].Degree - remainders[i + 1].Degree;
        }


        private TLongList subresultants = new TLongList();

        synchronized final void computeSubresultants()
        {
            if (!subresultants.isEmpty())
                return;

            TLongList subresultants = nonZeroSubresultants();
            if (swap)
                for (int i = 0; i < subresultants.Count; ++i)
                    subresultants.set(i, ring.negate(subresultants[i]));

            this.subresultants.ensureCapacity(remainders[1].Degree);
            for (int i = 0; i <= remainders[1].Degree; ++i)
                this.subresultants.add(0L);
            for (int i = 1; i < remainders.Count; i++)
                this.subresultants.set(remainders[i].Degree, subresultants[i - 1]);
        }


        TLongList nonZeroSubresultants()
        {
            TLongList subresultants = new TLongList();
            // largest subresultant
            long subresultant = ring.powMod(remainders[1].lc(), DegreeDiff(0));
            subresultants.add(subresultant);

            for (int i = 1; i < (remainders.Count - 1); ++i)
            {
                // computing (i+1)-th Degree subresultant

                int di = DegreeDiff(i);
                long rho = ring.powMod(ring.multiply(remainders[i + 1].lc(), remainders[i].lc()), di);
                if ((di % 2) == 1 && (remainders[0].Degree - remainders[i + 1].Degree + i + 1) % 2 == 1)
                    rho = ring.negate(rho);
                subresultant = ring.multiply(subresultant, rho);
                subresultants.add(subresultant);
            }

            return subresultants;
        }


        public TLongList getSubresultants()
        {
            if (subresultants.isEmpty())
                computeSubresultants();
            return subresultants;
        }


        public long resultant()
        {
            return getSubresultants()[0];
        }
    }
}