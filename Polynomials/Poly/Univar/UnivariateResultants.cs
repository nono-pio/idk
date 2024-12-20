using System.Numerics;
using Polynomials.Primes;
using Polynomials.Utils;
using UnivariatePolynomialZp64 = Polynomials.Poly.Univar.UnivariatePolynomial<long>;
using static Polynomials.Poly.Univar.Conversions64bit;

namespace Polynomials.Poly.Univar;

public static class UnivariateResultants
{


    public static UnivariatePolynomial<E> DiscriminantAsPoly<E>(UnivariatePolynomial<E> a)
    {
        
        return (a).CreateConstant(Discriminant(a));
    }


    public static E Discriminant<E>(UnivariatePolynomial<E> a)
    {
        Ring<E> ring = a.ring;
        var disc = ring.DivideExact(Resultant(a, a.Derivative()), a.Lc());
        return ((a.degree * (a.degree - 1) / 2) % 2 == 1) ? ring.Negate(disc) : disc;
    }


    public static UnivariatePolynomial<E> ResultantAsPoly<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return a.CreateConstant(Resultant(a, b));
    }

    public static E Resultant<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        // if (Util.IsOverMultipleFieldExtension(a))
        //     return (E)ResultantInMultipleFieldExtension((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        if (a.IsOverFiniteField())
            return ClassicalPRS(a, b).Resultant();
        if (Util.IsOverRationals(a))
            return (E)GenericHandler.InvokeForGeneric<E>(typeof(Rational<>), nameof(ResultantInQ), typeof(UnivariateResultants), a, b);//ResultantInQ(a, b);
        if (a.IsOverZ())
            return (E)(object)ModularResultant(a.AsZ(), b.AsZ());
        // if (Util.IsOverSimpleNumberField(a))
        //     return (E)ModularResultantInNumberField((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        // if (Util.IsOverRingOfIntegersOfSimpleNumberField(a))
        //     return (E)ModularResultantInRingOfIntegersOfNumberField((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        else
            return PrimitiveResultant(a, b, (p, q) => SubresultantPRS(p, q).Resultant());
    }

    private static Rational<E> ResultantInQ<E>(UnivariatePolynomial<Rational<E>> a, UnivariatePolynomial<Rational<E>> b)
    {
        var aZ = Util.ToCommonDenominator(a);
        var bZ = Util.ToCommonDenominator(b);
        Ring<E> ring = aZ.Item1.ring;
        E resultant = Resultant(aZ.Item1, bZ.Item1);
        E den = ring.Multiply(ring.Pow(aZ.Item2, b.degree), ring.Pow(bZ.Item2, a.degree));
        return new Rational<E>(ring, resultant, den);
    }

    // TODO
    // private static mPoly ResultantInMultipleFieldExtension<Term extends AMonomial<Term>, mPoly
    //     extends AMultivariatePolynomial<Term, mPoly>, sPoly extends IUnivariatePolynomial<sPoly>>(
    //     UnivariatePolynomial<mPoly> a, UnivariatePolynomial<mPoly> b)
    // {
    //     MultipleFieldExtension<Term, mPoly, sPoly> ring = (MultipleFieldExtension<Term, mPoly, sPoly>)a.ring;
    //     SimpleFieldExtension<sPoly> simpleExtension = ring.GetSimpleExtension();
    //     return ring.Image(Resultant(a.MapCoefficients(simpleExtension, ring.Inverse()),
    //         b.MapCoefficients(simpleExtension, ring.Inverse())));
    // }


    public static List<E> Subresultants<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        if (a.IsOverField())
            return ClassicalPRS(a, b).GetSubresultants();
        else
            return SubresultantPRS(a, b).GetSubresultants();
    }

    static E PrimitiveResultant<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b,
        Func<UnivariatePolynomial<E>, UnivariatePolynomial<E>, E> algorithm)
    {
        E ac = a.Content(), bc = b.Content();
        a = a.Clone().DivideExact(ac);
        b = b.Clone().DivideExact(bc);
        var r = algorithm(a, b);
        Ring<E> ring = a.ring;
        r = ring.Multiply(r, ring.Pow(ac, b.degree));
        r = ring.Multiply(r, ring.Pow(bc, a.degree));
        return r;
    }


    public static BigInteger ModularResultant(UnivariatePolynomial<BigInteger> a, UnivariatePolynomial<BigInteger> b)
    {
        return PrimitiveResultant(a, b, UnivariateResultants.ModularResultant0);
    }

    private static BigInteger ModularResultant0(UnivariatePolynomial<BigInteger> a, UnivariatePolynomial<BigInteger> b)
    {
        // bound on the value of resultant
        var bound = (BigInteger.Pow(UnivariatePolynomial<BigInteger>.Norm2(a), b.degree)
                     * BigInteger.Pow(UnivariatePolynomial<BigInteger>.Norm2(b), a.degree)) << 1;

        // aggregated CRT modulus
        BigInteger? bModulus = null;
        BigInteger? resultant = null;
        var primes = new PrimesIterator(1 << 25);
        while (true)
        {
            var prime = primes.Take();
            var bPrime = new BigInteger(prime);
            var zpRing = Rings.Zp(prime);
            UnivariatePolynomialZp64 aMod = AsOverZp64(a.SetRing(zpRing)), bMod = AsOverZp64(b.SetRing(zpRing));
            if (aMod.degree != a.degree || bMod.degree != b.degree)
                continue; // unlucky prime
            var resultantMod = ClassicalPRS(aMod, bMod).Resultant();
            var bResultantMod = new BigInteger(resultantMod);
            if (bModulus is null)
            {
                bModulus = bPrime;
                resultant = bResultantMod;
                continue;
            }

            if (!resultant.Value.IsZero && resultantMod == 0)
                continue; // unlucky prime
            resultant = ChineseRemainders.ChineseRemainder(bModulus.Value, bPrime, resultant.Value, bResultantMod);
            bModulus = bModulus * prime;
            if (bModulus.Value.CompareTo(bound) > 0)
                return Rings.Zp(bModulus.Value).SymmetricForm(resultant.Value);
        }
    }
    
    // TODO
    // private static UnivariatePolynomial<E> TrivialResultantInNumberField<E>(
    //     UnivariatePolynomial<UnivariatePolynomial<E>> a, UnivariatePolynomial<UnivariatePolynomial<E>> b)
    // {
    //     AlgebraicNumberField<UnivariatePolynomial<E>> ring = (AlgebraicNumberField<UnivariatePolynomial<E>>)a.ring;
    //     if (!a.Stream().AllMatch(ring.IsInTheBaseField) || !b.Stream().AllMatch(ring.IsInTheBaseField))
    //         return null;
    //     UnivariatePolynomial<E> ar = a.MapCoefficients(ring.GetMinimalPolynomial().ring, p => p.Cc()),
    //         br = b.MapCoefficients(ring.GetMinimalPolynomial().ring, p => p.Cc());
    //     return UnivariatePolynomial<E>.Constant(ring.GetMinimalPolynomial().ring, Resultant(ar, br));
    // }

    // TODO
    // public static UnivariatePolynomial<Rational<BigInteger>> ModularResultantInNumberField(
    //     UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a,
    //     UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> b)
    // {
    //     UnivariatePolynomial<Rational<BigInteger>> r = TrivialResultantInNumberField(a, b);
    //     if (r != null)
    //         return r;
    //     AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> numberField =
    //         (AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>>)a.ring;
    //     UnivariatePolynomial<Rational<BigInteger>> minimalPoly = numberField.GetMinimalPolynomial();
    //     a = a.Clone();
    //     b = b.Clone();
    //
    //     // reduce problem to the case with integer monic minimal polynomial
    //     if (minimalPoly.Stream().All(x => x.IsIntegral()))
    //     {
    //         // minimal poly is already monic & integer
    //         UnivariatePolynomial<BigInteger> minimalPolyZ = minimalPoly.MapCoefficients(Z, r_ => r_.Numerator());
    //         var numberFieldZ = new AlgebraicNumberField<UnivariatePolynomial<BigInteger>>(minimalPolyZ);
    //         BigInteger aDen = RemoveDenominators(a),
    //             bDen = RemoveDenominators(b),
    //             den = aDen.Pow(b.degree).Multiply(bDen.Pow(a.degree));
    //         return ModularResultantInRingOfIntegersOfNumberField(
    //                 a.MapCoefficients(numberFieldZ, (cf) => cf.MapCoefficients(Z, Rational.Numerator())),
    //                 b.MapCoefficients(numberFieldZ, (cf) => cf.MapCoefficients(Z, Rational.Numerator())))
    //             .MapCoefficients(Q, (cf) => Q.Mk(cf, den));
    //     }
    //     else
    //     {
    //         // replace s -> s / lc(minPoly)
    //         BigInteger minPolyLeadCoeff = ToCommonDenominator(minimalPoly)._1.Lc();
    //         Rational<BigInteger> scale = new Rational(Z, Z.GetOne(), minPolyLeadCoeff),
    //             scaleReciprocal = scale.Reciprocal();
    //         AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> scaledNumberField =
    //             new AlgebraicNumberField(minimalPoly.Scale(scale).Monic());
    //         return ModularResultantInNumberField(a.MapCoefficients(scaledNumberField, (cf) => cf.Scale(scale)),
    //             b.MapCoefficients(scaledNumberField, (cf) => cf.Scale(scale))).Scale(scaleReciprocal);
    //     }
    // }

    public static BigInteger PolyPowNumFieldCfBound(BigInteger maxCf, BigInteger maxMinPolyCf, int minPolyDeg,
        int exponent)
    {
        return BigInteger.Pow(minPolyDeg, exponent - 1) * BigInteger.Pow(maxCf, exponent) *
            BigInteger.Pow(maxMinPolyCf + 1, (exponent - 1) * (minPolyDeg + 1));
    }

    // TODO
    // public static UnivariatePolynomial<BigInteger> ModularResultantInRingOfIntegersOfNumberField(
    //     UnivariatePolynomial<UnivariatePolynomial<BigInteger>> a,
    //     UnivariatePolynomial<UnivariatePolynomial<BigInteger>> b)
    // {
    //     return PrimitiveResultant(a, b, UnivariateResultants.ModularResultantInRingOfIntegersOfNumberField0);
    // }
    //
    // private static UnivariatePolynomial<BigInteger> ModularResultantInRingOfIntegersOfNumberField0(
    //     UnivariatePolynomial<UnivariatePolynomial<BigInteger>> a,
    //     UnivariatePolynomial<UnivariatePolynomial<BigInteger>> b)
    // {
    //     UnivariatePolynomial<BigInteger> r = TrivialResultantInNumberField(a, b);
    //     if (r != null)
    //         return r;
    //     AlgebraicNumberField<UnivariatePolynomial<BigInteger>> numberField =
    //         (AlgebraicNumberField<UnivariatePolynomial<BigInteger>>)a.ring;
    //     UnivariatePolynomial<BigInteger> minimalPoly = numberField.GetMinimalPolynomial();
    //     BigInteger aMax =
    //             a.Stream().FlatMap(UnivariatePolynomial.Stream()).Map(Rings.Z.Abs()).Max(Rings.Z)
    //                 .OrElse(BigInteger.Zero),
    //         bMax =
    //             b.Stream().FlatMap(UnivariatePolynomial.Stream()).Map(Rings.Z.Abs()).Max(Rings.Z)
    //                 .OrElse(BigInteger.ZERO),
    //         mMax = minimalPoly.MaxAbsCoefficient();
    //
    //     // bound on the value of resultant coefficients
    //     BigInteger bound = PolyPowNumFieldCfBound(aMax, mMax, minimalPoly.degree, b.degree)
    //         .Multiply(PolyPowNumFieldCfBound(bMax, mMax, minimalPoly.degree, a.degree));
    //
    //     // aggregated CRT modulus
    //     BigInteger bModulus = null;
    //     UnivariatePolynomial<BigInteger> resultant = null;
    //     PrimesIterator primes = new PrimesIterator(1 << 25);
    //     while (true)
    //     {
    //         long prime = primes.Take();
    //         IntegersZp64 zpRing = Rings.Zp64(prime);
    //         UnivariatePolynomialZp64 minimalPolyMod = AsOverZp64(minimalPoly, zpRing);
    //         FiniteField<UnivariatePolynomialZp64> numberFieldMod =
    //             new FiniteField<UnivariatePolynomialZp64>(minimalPolyMod);
    //         UnivariatePolynomial<UnivariatePolynomialZp64> aMod =
    //                 a.MapCoefficients(numberFieldMod, (cf) => AsOverZp64(cf, zpRing)),
    //             bMod = b.MapCoefficients(numberFieldMod, (cf) => AsOverZp64(cf, zpRing));
    //         if (aMod.degree != a.degree || bMod.degree != b.degree)
    //             continue; // unlucky prime
    //         UnivariatePolynomialZp64 resultantMod = ClassicalPRS(aMod, bMod).Resultant();
    //         if (bModulus == null)
    //         {
    //             bModulus = new BigInteger(prime);
    //             resultant = resultantMod.ToBigPoly();
    //             continue;
    //         }
    //
    //         if (!resultant.IsZero() && resultantMod.IsZero())
    //             continue; // unlucky prime
    //         UnivariateGCD.UpdateCRT(ChineseRemainders.CreateMagic(Rings.Z, bModulus, new BigInteger(prime)), resultant,
    //             resultantMod);
    //         bModulus = bModulus.Multiply(new BigInteger(prime));
    //         if (bModulus.CompareTo(bound) > 0)
    //             return UnivariatePolynomial<BigInteger>.AsPolyZSymmetric(resultant.SetRingUnsafe(Rings.Zp(bModulus)));
    //     }
    // }


    public static PolynomialRemainderSequenceZp64 ClassicalPRS(UnivariatePolynomialZp64 a, UnivariatePolynomialZp64 b)
    {
        return new PolynomialRemainderSequenceZp64(a, b).Run();
    }


    public static PolynomialRemainderSequence<E> ClassicalPRS<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return new ClassicalPolynomialRemainderSequence<E>(a, b).Run();
    }


    public static PolynomialRemainderSequence<E> PseudoPRS<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return new PseudoPolynomialRemainderSequence<E>(a, b).Run();
    }


    public static PolynomialRemainderSequence<E> PrimitivePRS<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return new PrimitivePolynomialRemainderSequence<E>(a, b).Run();
    }


    public static PolynomialRemainderSequence<E> ReducedPRS<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return new ReducedPolynomialRemainderSequence<E>(a, b).Run();
    }


    public static PolynomialRemainderSequence<E> SubresultantPRS<E>(UnivariatePolynomial<E> a,
        UnivariatePolynomial<E> b)
    {
        return new SubresultantPolynomialRemainderSequence<E>(a, b).Run();
    }


    public class APolynomialRemainderSequence<E> 
    {
        public readonly List<UnivariatePolynomial<E>> remainders = [];


        public readonly List<UnivariatePolynomial<E>> quotients = [];


        public readonly UnivariatePolynomial<E> a, b;


        public APolynomialRemainderSequence(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
        {
            this.a = a;
            this.b = b;
        }


        public UnivariatePolynomial<E> LastRemainder()
        {
            return remainders[remainders.Count - 1];
        }


        public UnivariatePolynomial<E> Gcd()
        {
            if (a.IsZero())
                return b;
            if (b.IsZero())
                return a;
            if (a.IsOverField())
                return LastRemainder().Clone().Monic();
            UnivariatePolynomial<E> r = LastRemainder().Clone().PrimitivePartSameSign();
            return UnivariateGCD.PolynomialGCD(a.ContentAsPoly(), b.ContentAsPoly()).Multiply(r);
        }


        public int Size()
        {
            return remainders.Count;
        }
    }


    public abstract class PolynomialRemainderSequence<E> : APolynomialRemainderSequence<E>
    {
        public readonly List<E> alphas = [];


        public readonly List<E> betas = [];


        public readonly Ring<E> ring;


        readonly bool swap;


        public PolynomialRemainderSequence(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) : base(a, b)
        {
            this.ring = a.ring;
            if (a.degree >= b.degree)
            {
                remainders.Add(a);
                remainders.Add(b);
                swap = false;
            }
            else
            {
                remainders.Add(b);
                remainders.Add(a);
                swap = a.degree % 2 == 1 &&
                       b.degree % 2 == 1; // both degrees are odd => odd permutation of Sylvester matrix
            }
        }


        public abstract E NextAlpha();


        public abstract E NextBeta(UnivariatePolynomial<E> remainder);


        private UnivariatePolynomial<E> Step()
        {
            var i = remainders.Count;
            UnivariatePolynomial<E> dividend = remainders[i - 2].Clone(), divider = remainders[i - 1];
            var alpha = NextAlpha();
            dividend = dividend.Multiply(alpha);
            UnivariatePolynomial<E>[] qd = UnivariateDivision.DivideAndRemainder(dividend, divider, false);
            if (qd == null)
                throw new Exception("exact division is not possible");
            UnivariatePolynomial<E> quotient = qd[0], remainder = qd[1];
            if (remainder.IsZero())

                // remainder is zero => termination of the algorithm
                return remainder;
            var beta = NextBeta(remainder);
            remainder = remainder.DivideExact(beta);
            alphas.Add(alpha);
            betas.Add(beta);
            quotients.Add(quotient);
            remainders.Add(remainder);
            return remainder;
        }


        public PolynomialRemainderSequence<E> Run()
        {
            if (LastRemainder().IsZero())

                // on of the factors is zero
                return this;
            while (!Step().IsZero())
                ;
            return this;
        }


        public int DegreeDiff(int i)
        {
            return remainders[i].degree - remainders[i + 1].degree;
        }


        private readonly List<E> subresultants = [];


        void ComputeSubresultants()
        {
            lock (this)
            {
                if (this.subresultants.Count != 0)
                    return;
                List<E> subresultants = NonZeroSubresultants();
                if (swap)
                    for (var i = 0; i < subresultants.Count; i++)
                        subresultants[i] = ring.Negate(subresultants[i]);
                this.subresultants.EnsureCapacity(remainders[1].degree);
                for (var i = 0; i <= remainders[1].degree; ++i)
                    this.subresultants.Add(ring.GetZero());
                for (var i = 1; i < remainders.Count; i++)
                    this.subresultants[remainders[i].degree] = subresultants[i - 1];
            }
        }


        public virtual List<E> NonZeroSubresultants()
        {
            List<E> subresultants = [];

            // largest subresultant
            var subresultant = ring.Pow(remainders[1].Lc(), DegreeDiff(0));
            subresultants.Add(subresultant);
            for (var i = 1; i < (remainders.Count - 1); ++i)
            {
                // computing (i+1)-th degree subresultant
                var di = DegreeDiff(i);
                var rho = ring.Pow(ring.Multiply(remainders[i + 1].Lc(), remainders[i].Lc()), di);
                var den = ring.GetOne();
                for (var j = 1; j <= i; ++j)
                {
                    rho = ring.Multiply(rho, ring.Pow(betas[j - 1], di));
                    den = ring.Multiply(den, ring.Pow(alphas[j - 1], di));
                }

                if ((di % 2) == 1 && (remainders[0].degree - remainders[i + 1].degree + i + 1) % 2 == 1)
                    rho = ring.Negate(rho);
                subresultant = ring.Multiply(subresultant, rho);
                subresultant = ring.DivideExact(subresultant, den);
                subresultants.Add(subresultant);
            }

            return subresultants;
        }


        public List<E> GetSubresultants()
        {
            if (subresultants.Count == 0)
                ComputeSubresultants();
            return subresultants;
        }


        public E Resultant()
        {
            return GetSubresultants()[0];
        }
    }


    private sealed class ClassicalPolynomialRemainderSequence<E> : PolynomialRemainderSequence<E>
    {
        public ClassicalPolynomialRemainderSequence(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) : base(a, b)
        {
        }

        public override E NextAlpha()
        {
            return ring.GetOne();
        }

        public override E NextBeta(UnivariatePolynomial<E> remainder)
        {
            return ring.GetOne();
        }

        public override List<E> NonZeroSubresultants()
        {
            List<E> subresultants = [];

            // largest subresultant
            var subresultant = ring.Pow(remainders[1].Lc(), DegreeDiff(0));
            subresultants.Add(subresultant);
            for (var i = 1; i < (remainders.Count - 1); ++i)
            {
                // computing (i+1)-th degree subresultant
                var di = DegreeDiff(i);
                var rho = ring.Pow(ring.Multiply(remainders[i + 1].Lc(), remainders[i].Lc()), di);
                if ((di % 2) == 1 && (remainders[0].degree - remainders[i + 1].degree + i + 1) % 2 == 1)
                    rho = ring.Negate(rho);
                subresultant = ring.Multiply(subresultant, rho);
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

        public override E NextAlpha()
        {
            var i = remainders.Count;
            var lc = remainders[i - 1].Lc();
            var deg = remainders[i - 2].degree - remainders[i - 1].degree;
            return ring.Pow(lc, deg + 1);
        }

        public override E NextBeta(UnivariatePolynomial<E> remainder)
        {
            return ring.GetOne();
        }
    }


    private sealed class ReducedPolynomialRemainderSequence<E> : PseudoPolynomialRemainderSequence<E>
    {
        public ReducedPolynomialRemainderSequence(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) : base(a, b)
        {
        }

        public override E NextBeta(UnivariatePolynomial<E> remainder)
        {
            return alphas.Count == 0 ? ring.GetOne() : alphas[alphas.Count - 1];
        }

        public override List<E> NonZeroSubresultants()
        {
            List<E> subresultants = [];

            // largest subresultant
            var subresultant = ring.Pow(remainders[1].Lc(), DegreeDiff(0));
            subresultants.Add(subresultant);
            for (var i = 1; i < (remainders.Count - 1); ++i)
            {
                // computing (i+1)-th degree subresultant
                var di = DegreeDiff(i);
                var rho = ring.Pow(remainders[i + 1].Lc(), di);
                var den = ring.Pow(remainders[i].Lc(), DegreeDiff(i - 1) * di);
                subresultant = ring.Multiply(subresultant, rho);
                subresultant = ring.DivideExact(subresultant, den);
                if ((di % 2) == 1 && (remainders[0].degree - remainders[i + 1].degree + i + 1) % 2 == 1)
                    subresultant = ring.Negate(subresultant);
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

        public override E NextBeta(UnivariatePolynomial<E> remainder)
        {
            return remainder.Content();
        }
    }


    private sealed class SubresultantPolynomialRemainderSequence<E> : PseudoPolynomialRemainderSequence<E>
    {
        readonly List<E> psis = [];

        public SubresultantPolynomialRemainderSequence(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) : base(a,
            b)
        {
        }

        public override E NextBeta(UnivariatePolynomial<E> remainder)
        {
            var i = remainders.Count;
            UnivariatePolynomial<E> prem = remainders[i - 2];
            var lc = i == 2 ? ring.GetOne() : prem.Lc();
            E psi;
            if (i == 2)
                psi = ring.GetNegativeOne();
            else
            {
                var prevPsi = psis[psis.Count - 1];
                var deg = remainders[i - 3].degree - remainders[i - 2].degree;
                var f = ring.Pow(ring.Negate(lc), deg);
                if (1 - deg < 0)
                    psi = ring.DivideExact(f, ring.Pow(prevPsi, deg - 1));
                else
                    psi = ring.Multiply(f, ring.Pow(prevPsi, 1 - deg));
            }

            psis.Add(psi);
            return ring.Negate(ring.Multiply(lc, ring.Pow(psi, remainders[i - 2].degree - remainders[i - 1].degree)));
        }

        private int Eij(int i, int j)
        {
            var e = DegreeDiff(j - 1);
            for (var k = j; k <= i; ++k)
                e *= 1 - DegreeDiff(k);
            return e;
        }

        public override List<E> NonZeroSubresultants()
        {
            List<E> subresultants = [];

            // largest subresultant
            var subresultant = ring.Pow(remainders[1].Lc(), DegreeDiff(0));
            subresultants.Add(subresultant);
            for (var i = 1; i < (remainders.Count - 1); ++i)
            {
                // computing (i+1)-th degree subresultant
                var di = DegreeDiff(i);
                var rho = ring.Pow(remainders[i + 1].Lc(), di);
                var den = ring.GetOne();
                for (var k = 1; k <= i; ++k)
                {
                    var deg = -di * Eij(i - 1, k);
                    if (deg >= 0)
                        rho = ring.Multiply(rho, ring.Pow(remainders[k].Lc(), deg));
                    else
                        den = ring.Multiply(den, ring.Pow(remainders[k].Lc(), -deg));
                }

                subresultant = ring.Multiply(subresultant, rho);
                subresultant = ring.DivideExact(subresultant, den);
                subresultants.Add(subresultant);
            }

            return subresultants;
        }
    }


    public sealed class PolynomialRemainderSequenceZp64 : APolynomialRemainderSequence<long>
    {
        readonly IntegersZp64 ring;


        readonly bool swap;


        public PolynomialRemainderSequenceZp64(UnivariatePolynomialZp64 a, UnivariatePolynomialZp64 b) : base(a, b)
        {
            this.ring = a.ring as IntegersZp64 ?? throw new ArgumentException();
            if (a.degree >= b.degree)
            {
                remainders.Add(a);
                remainders.Add(b);
                swap = false;
            }
            else
            {
                remainders.Add(b);
                remainders.Add(a);
                swap = a.degree % 2 == 1 &&
                       b.degree % 2 == 1; // both degrees are odd => odd permutation of Sylvester matrix
            }
        }


        private UnivariatePolynomialZp64 Step()
        {
            var i = remainders.Count;
            UnivariatePolynomialZp64 dividend = remainders[i - 2].Clone(), divider = remainders[i - 1];
            UnivariatePolynomialZp64[] qd = UnivariateDivision.DivideAndRemainder(dividend, divider, false);
            if (qd == null)
                throw new Exception("exact division is not possible");
            UnivariatePolynomialZp64 quotient = qd[0], remainder = qd[1];
            if (remainder.IsZero())

                // remainder is zero => termination of the algorithm
                return remainder;
            quotients.Add(quotient);
            remainders.Add(remainder);
            return remainder;
        }


        public PolynomialRemainderSequenceZp64 Run()
        {
            while (!Step().IsZero())
                ;
            return this;
        }


        int DegreeDiff(int i)
        {
            return remainders[i].degree - remainders[i + 1].degree;
        }


        private readonly List<long> subresultants = new List<long>();


        void ComputeSubresultants()
        {
            lock (this)
            {
                if (this.subresultants.Count != 0)
                    return;
                var subresultants = NonZeroSubresultants();
                if (swap)
                    for (var i = 0; i < subresultants.Count; ++i)
                        subresultants[i] = ring.Negate(subresultants[i]);
                this.subresultants.EnsureCapacity(remainders[1].degree);
                for (var i = 0; i <= remainders[1].degree; ++i)
                    this.subresultants.Add(0);
                for (var i = 1; i < remainders.Count; i++)
                    this.subresultants[remainders[i].degree] = subresultants[i - 1];
            }
        }


        List<long> NonZeroSubresultants()
        {
            var subresultants = new List<long>();

            // largest subresultant
            var subresultant = ring.PowMod(remainders[1].Lc(), DegreeDiff(0));
            subresultants.Add(subresultant);
            for (var i = 1; i < (remainders.Count - 1); ++i)
            {
                // computing (i+1)-th degree subresultant
                var di = DegreeDiff(i);
                var rho = ring.PowMod(ring.Multiply(remainders[i + 1].Lc(), remainders[i].Lc()), di);
                if ((di % 2) == 1 && (remainders[0].degree - remainders[i + 1].degree + i + 1) % 2 == 1)
                    rho = ring.Negate(rho);
                subresultant = ring.Multiply(subresultant, rho);
                subresultants.Add(subresultant);
            }

            return subresultants;
        }


        public List<long> GetSubresultants()
        {
            if (subresultants.Count == 0)
                ComputeSubresultants();
            return subresultants;
        }


        public long Resultant()
        {
            return GetSubresultants()[0];
        }
    }
}
