using Cc.Redberry;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Poly.Multivar;
using Cc.Redberry.Rings.Primes;
using Gnu.Trove.List.Array;
using Java.Util;
using Java.Util.Function;
using Cc.Redberry.Rings.Rings;
using Cc.Redberry.Rings.Poly.Util;
using Cc.Redberry.Rings.Poly.Univar.UnivariateGCD;
using Cc.Redberry.Rings.Poly.Univar.UnivariatePolynomial;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Poly.Univar.RoundingMode;
using static Cc.Redberry.Rings.Poly.Univar.Associativity;
using static Cc.Redberry.Rings.Poly.Univar.Operator;
using static Cc.Redberry.Rings.Poly.Univar.TokenType;
using static Cc.Redberry.Rings.Poly.Univar.SystemInfo;

namespace Cc.Redberry.Rings.Poly.Univar
{
    /// <summary>
    /// Various algorithms to compute (sub)resultants via Euclidean algorithm. Implementation is based on Gathen & Lücking,
    /// "Subresultants revisited", https://doi.org/10.1016/S0304-3975(02)00639-4
    /// </summary>
    /// <remarks>@since2.5</remarks>
    public sealed class UnivariateResultants
    {
        private UnivariateResultants()
        {
        }

        /// <summary>
        /// Computes discriminant of polynomial and returns the result as a constant poly
        /// </summary>
        public static Poly DiscriminantAsPoly<Poly extends IUnivariatePolynomial<Poly>>(Poly a)
        {
            if (a is UnivariatePolynomialZp64)
                return (Poly)((UnivariatePolynomialZp64)a).CreateConstant(Discriminant((UnivariatePolynomialZp64)a));
            else
                return (Poly)((UnivariatePolynomial)a).CreateConstant(Discriminant((UnivariatePolynomial)a));
        }

        /// <summary>
        /// Computes discriminant of polynomial
        /// </summary>
        public static E Discriminant<E>(UnivariatePolynomial<E> a)
        {
            Ring<E> ring = a.ring;
            E disc = ring.DivideExact(Resultant(a, a.Derivative()), a.Lc());
            return ((a.degree * (a.degree - 1) / 2) % 2 == 1) ? ring.Negate(disc) : disc;
        }

        /// <summary>
        /// Computes discriminant of polynomial
        /// </summary>
        public static long Discriminant(UnivariatePolynomialZp64 a)
        {
            IntegersZp64 ring = a.ring;
            long disc = ring.Divide(Resultant(a, a.Derivative()), a.Lc());
            return ((a.degree * (a.degree - 1) / 2) % 2 == 1) ? ring.Negate(disc) : disc;
        }

        /// <summary>
        /// Computes resultant of two polynomials and returns the result as a constant poly
        /// </summary>
        public static Poly ResultantAsPoly<Poly extends IUnivariatePolynomial<Poly>>(Poly a, Poly b)
        {
            if (a is UnivariatePolynomialZp64)
                return (Poly)((UnivariatePolynomialZp64)a).CreateConstant(Resultant((UnivariatePolynomialZp64)a, (UnivariatePolynomialZp64)b));
            else
                return (Poly)((UnivariatePolynomial)a).CreateConstant(Resultant((UnivariatePolynomial)a, (UnivariatePolynomial)b));
        }

        /// <summary>
        /// Computes resultant of two polynomials
        /// </summary>
        public static E Resultant<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
        {
            if (Util.IsOverMultipleFieldExtension(a))
                return (E)ResultantInMultipleFieldExtension((UnivariatePolynomial)a, (UnivariatePolynomial)b);
            else if (a.IsOverFiniteField())
                return ClassicalPRS(a, b).Resultant();
            else if (Util.IsOverRationals(a))
                return (E)ResultantInQ((UnivariatePolynomial)a, (UnivariatePolynomial)b);
            else if (a.IsOverZ())
                return (E)ModularResultant((UnivariatePolynomial)a, (UnivariatePolynomial)b);
            else if (Util.IsOverSimpleNumberField(a))
                return (E)ModularResultantInNumberField((UnivariatePolynomial)a, (UnivariatePolynomial)b);
            else if (Util.IsOverRingOfIntegersOfSimpleNumberField(a))
                return (E)ModularResultantInRingOfIntegersOfNumberField((UnivariatePolynomial)a, (UnivariatePolynomial)b);
            else
                return PrimitiveResultant(a, b, (p, q) => SubresultantPRS(p, q).Resultant());
        }

        /// <summary>
        /// Computes resultant of two polynomials
        /// </summary>
        public static long Resultant(UnivariatePolynomialZp64 a, UnivariatePolynomialZp64 b)
        {
            return ClassicalPRS(a, b).Resultant();
        }

        private static Rational<E> ResultantInQ<E>(UnivariatePolynomial<Rational<E>> a, UnivariatePolynomial<Rational<E>> b)
        {
            Util.Tuple2<UnivariatePolynomial<E>, E> aZ = Util.ToCommonDenominator(a), bZ = Util.ToCommonDenominator(b);
            Ring<E> ring = aZ._1.ring;
            E resultant = Resultant(aZ._1, bZ._1);
            E den = ring.Multiply(ring.Pow(aZ._2, b.degree), ring.Pow(bZ._2, a.degree));
            return new Rational(ring, resultant, den);
        }

        private static mPoly ResultantInMultipleFieldExtension<Term extends AMonomial<Term>, mPoly extends AMultivariatePolynomial<Term, mPoly>, sPoly extends IUnivariatePolynomial<sPoly>>(UnivariatePolynomial<mPoly> a, UnivariatePolynomial<mPoly> b)
        {
            MultipleFieldExtension<Term, mPoly, sPoly> ring = (MultipleFieldExtension<Term, mPoly, sPoly>)a.ring;
            SimpleFieldExtension<sPoly> simpleExtension = ring.GetSimpleExtension();
            return ring.Image(Resultant(a.MapCoefficients(simpleExtension, ring.Inverse()), b.MapCoefficients(simpleExtension, ring.Inverse())));
        }

        /// <summary>
        /// Computes sequence of scalar subresultants.
        /// </summary>
        public static IList<E> Subresultants<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
        {
            if (a.IsOverField())
                return ClassicalPRS(a, b).GetSubresultants();
            else
                return SubresultantPRS(a, b).GetSubresultants();
        }

        static E PrimitiveResultant<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b, BiFunction<UnivariatePolynomial<E>, UnivariatePolynomial<E>, E> algorithm)
        {
            E ac = a.Content(), bc = b.Content();
            a = a.Clone().DivideExact(ac);
            b = b.Clone().DivideExact(bc);
            E r = algorithm.Apply(a, b);
            Ring<E> ring = a.ring;
            r = ring.Multiply(r, ring.Pow(ac, b.degree));
            r = ring.Multiply(r, ring.Pow(bc, a.degree));
            return r;
        }

        /// <summary>
        /// Modular algorithm for computing resultants over Z
        /// </summary>
        public static BigInteger ModularResultant(UnivariatePolynomial<BigInteger> a, UnivariatePolynomial<BigInteger> b)
        {
            return PrimitiveResultant(a, b, UnivariateResultants.ModularResultant0());
        }

        private static BigInteger ModularResultant0(UnivariatePolynomial<BigInteger> a, UnivariatePolynomial<BigInteger> b)
        {

            // bound on the value of resultant
            BigInteger bound = UnivariatePolynomial.Norm2(a).Pow(b.degree).Multiply(UnivariatePolynomial.Norm2(b).Pow(a.degree)).ShiftLeft(1);

            // aggregated CRT modulus
            BigInteger bModulus = null;
            BigInteger resultant = null;
            PrimesIterator primes = new PrimesIterator(1 << 25);
            while (true)
            {
                long prime = primes.Take();
                BigInteger bPrime = BigInteger.ValueOf(prime);
                IntegersZp zpRing = Rings.Zp(prime);
                UnivariatePolynomialZp64 aMod = AsOverZp64(a.SetRing(zpRing)), bMod = AsOverZp64(b.SetRing(zpRing));
                if (aMod.degree != a.degree || bMod.degree != b.degree)
                    continue; // unlucky prime
                long resultantMod = ClassicalPRS(aMod, bMod).Resultant();
                BigInteger bResultantMod = BigInteger.ValueOf(resultantMod);
                if (bModulus == null)
                {
                    bModulus = bPrime;
                    resultant = bResultantMod;
                    continue;
                }

                if (!resultant.IsZero() && resultantMod == 0)
                    continue; // unlucky prime
                resultant = ChineseRemainders.ChineseRemainders(bModulus, bPrime, resultant, bResultantMod);
                bModulus = bModulus.Multiply(BigInteger.ValueOf(prime));
                if (bModulus.CompareTo(bound) > 0)
                    return Rings.Zp(bModulus).SymmetricForm(resultant);
            }
        }

        private static UnivariatePolynomial<E> TrivialResultantInNumberField<E>(UnivariatePolynomial<UnivariatePolynomial<E>> a, UnivariatePolynomial<UnivariatePolynomial<E>> b)
        {
            AlgebraicNumberField<UnivariatePolynomial<E>> ring = (AlgebraicNumberField<UnivariatePolynomial<E>>)a.ring;
            if (!a.Stream().AllMatch(ring.IsInTheBaseField()) || !b.Stream().AllMatch(ring.IsInTheBaseField()))
                return null;
            UnivariatePolynomial<E> ar = a.MapCoefficients(ring.GetMinimalPolynomial().ring, UnivariatePolynomial.Cc()), br = b.MapCoefficients(ring.GetMinimalPolynomial().ring, UnivariatePolynomial.Cc());
            return UnivariatePolynomial.Constant(ring.GetMinimalPolynomial().ring, Resultant(ar, br));
        }

        /// <summary>
        /// Modular resultant in simple number field
        /// </summary>
        public static UnivariatePolynomial<Rational<BigInteger>> ModularResultantInNumberField(UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a, UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> b)
        {
            UnivariatePolynomial<Rational<BigInteger>> r = TrivialResultantInNumberField(a, b);
            if (r != null)
                return r;
            AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> numberField = (AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>>)a.ring;
            UnivariatePolynomial<Rational<BigInteger>> minimalPoly = numberField.GetMinimalPolynomial();
            a = a.Clone();
            b = b.Clone();

            // reduce problem to the case with integer monic minimal polynomial
            if (minimalPoly.Stream().AllMatch(Rational.IsIntegral()))
            {

                // minimal poly is already monic & integer
                UnivariatePolynomial<BigInteger> minimalPolyZ = minimalPoly.MapCoefficients(Z, Rational.Numerator());
                AlgebraicNumberField<UnivariatePolynomial<BigInteger>> numberFieldZ = new AlgebraicNumberField(minimalPolyZ);
                BigInteger aDen = RemoveDenominators(a), bDen = RemoveDenominators(b), den = aDen.Pow(b.degree).Multiply(bDen.Pow(a.degree));
                return ModularResultantInRingOfIntegersOfNumberField(a.MapCoefficients(numberFieldZ, (cf) => cf.MapCoefficients(Z, Rational.Numerator())), b.MapCoefficients(numberFieldZ, (cf) => cf.MapCoefficients(Z, Rational.Numerator()))).MapCoefficients(Q, (cf) => Q.Mk(cf, den));
            }
            else
            {

                // replace s -> s / lc(minPoly)
                BigInteger minPolyLeadCoeff = ToCommonDenominator(minimalPoly)._1.Lc();
                Rational<BigInteger> scale = new Rational(Z, Z.GetOne(), minPolyLeadCoeff), scaleReciprocal = scale.Reciprocal();
                AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> scaledNumberField = new AlgebraicNumberField(minimalPoly.Scale(scale).Monic());
                return ModularResultantInNumberField(a.MapCoefficients(scaledNumberField, (cf) => cf.Scale(scale)), b.MapCoefficients(scaledNumberField, (cf) => cf.Scale(scale))).Scale(scaleReciprocal);
            }
        }

        public static BigInteger PolyPowNumFieldCfBound(BigInteger maxCf, BigInteger maxMinPolyCf, int minPolyDeg, int exponent)
        {
            return BigInteger.ValueOf(minPolyDeg).Pow(exponent - 1).Multiply(maxCf.Pow(exponent)).Multiply(maxMinPolyCf.Increment().Pow((exponent - 1) * (minPolyDeg + 1)));
        }

        /// <summary>
        /// Modular resultant in the ring of integers of number field
        /// </summary>
        public static UnivariatePolynomial<BigInteger> ModularResultantInRingOfIntegersOfNumberField(UnivariatePolynomial<UnivariatePolynomial<BigInteger>> a, UnivariatePolynomial<UnivariatePolynomial<BigInteger>> b)
        {
            return PrimitiveResultant(a, b, UnivariateResultants.ModularResultantInRingOfIntegersOfNumberField0());
        }

        private static UnivariatePolynomial<BigInteger> ModularResultantInRingOfIntegersOfNumberField0(UnivariatePolynomial<UnivariatePolynomial<BigInteger>> a, UnivariatePolynomial<UnivariatePolynomial<BigInteger>> b)
        {
            UnivariatePolynomial<BigInteger> r = TrivialResultantInNumberField(a, b);
            if (r != null)
                return r;
            AlgebraicNumberField<UnivariatePolynomial<BigInteger>> numberField = (AlgebraicNumberField<UnivariatePolynomial<BigInteger>>)a.ring;
            UnivariatePolynomial<BigInteger> minimalPoly = numberField.GetMinimalPolynomial();
            BigInteger aMax = a.Stream().FlatMap(UnivariatePolynomial.Stream()).Map(Rings.Z.Abs()).Max(Rings.Z).OrElse(BigInteger.ZERO), bMax = b.Stream().FlatMap(UnivariatePolynomial.Stream()).Map(Rings.Z.Abs()).Max(Rings.Z).OrElse(BigInteger.ZERO), mMax = minimalPoly.MaxAbsCoefficient();

            // bound on the value of resultant coefficients
            BigInteger bound = PolyPowNumFieldCfBound(aMax, mMax, minimalPoly.degree, b.degree).Multiply(PolyPowNumFieldCfBound(bMax, mMax, minimalPoly.degree, a.degree));

            // aggregated CRT modulus
            BigInteger bModulus = null;
            UnivariatePolynomial<BigInteger> resultant = null;
            PrimesIterator primes = new PrimesIterator(1 << 25);
            while (true)
            {
                long prime = primes.Take();
                IntegersZp64 zpRing = Rings.Zp64(prime);
                UnivariatePolynomialZp64 minimalPolyMod = AsOverZp64(minimalPoly, zpRing);
                FiniteField<UnivariatePolynomialZp64> numberFieldMod = new FiniteField(minimalPolyMod);
                UnivariatePolynomial<UnivariatePolynomialZp64> aMod = a.MapCoefficients(numberFieldMod, (cf) => AsOverZp64(cf, zpRing)), bMod = b.MapCoefficients(numberFieldMod, (cf) => AsOverZp64(cf, zpRing));
                if (aMod.degree != a.degree || bMod.degree != b.degree)
                    continue; // unlucky prime
                UnivariatePolynomialZp64 resultantMod = ClassicalPRS(aMod, bMod).Resultant();
                if (bModulus == null)
                {
                    bModulus = BigInteger.ValueOf(prime);
                    resultant = resultantMod.ToBigPoly();
                    continue;
                }

                if (!resultant.IsZero() && resultantMod.IsZero())
                    continue; // unlucky prime
                UnivariateGCD.UpdateCRT(ChineseRemainders.CreateMagic(Rings.Z, bModulus, BigInteger.ValueOf(prime)), resultant, resultantMod);
                bModulus = bModulus.Multiply(BigInteger.ValueOf(prime));
                if (bModulus.CompareTo(bound) > 0)
                    return UnivariatePolynomial.AsPolyZSymmetric(resultant.SetRingUnsafe(Rings.Zp(bModulus)));
            }
        }

        /// <summary>
        /// Computes polynomial remainder sequence using classical division algorithm
        /// </summary>
        public static PolynomialRemainderSequenceZp64 ClassicalPRS(UnivariatePolynomialZp64 a, UnivariatePolynomialZp64 b)
        {
            return new PolynomialRemainderSequenceZp64(a, b).Run();
        }

        /// <summary>
        /// Computes polynomial remainder sequence using classical division algorithm
        /// </summary>
        public static PolynomialRemainderSequence<E> ClassicalPRS<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
        {
            return new ClassicalPolynomialRemainderSequence(a, b).Run();
        }

        /// <summary>
        /// Computes polynomial remainder sequence using pseudo division algorithm
        /// </summary>
        public static PolynomialRemainderSequence<E> PseudoPRS<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
        {
            return new PseudoPolynomialRemainderSequence(a, b).Run();
        }

        /// <summary>
        /// Computes polynomial remainder sequence using primitive division algorithm
        /// </summary>
        public static PolynomialRemainderSequence<E> PrimitivePRS<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
        {
            return new PrimitivePolynomialRemainderSequence(a, b).Run();
        }

        /// <summary>
        /// Computes polynomial remainder sequence using reduced division algorithm
        /// </summary>
        public static PolynomialRemainderSequence<E> ReducedPRS<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
        {
            return new ReducedPolynomialRemainderSequence(a, b).Run();
        }

        /// <summary>
        /// Computes subresultant polynomial remainder sequence
        /// </summary>
        public static PolynomialRemainderSequence<E> SubresultantPRS<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
        {
            return new SubresultantPolynomialRemainderSequence(a, b).Run();
        }

        /// <summary>
        /// Polynomial remainder sequence (PRS).
        /// </summary>
        public class APolynomialRemainderSequence<Poly>
        {
            /// <summary>
            /// Polynomial remainder sequence
            /// </summary>
            public readonly IList<Poly> remainders = new List();
            /// <summary>
            /// Polynomial remainder sequence
            /// </summary>
            /// <summary>
            /// Quotients arised in PRS
            /// </summary>
            public readonly IList<Poly> quotients = new List();
            /// <summary>
            /// Polynomial remainder sequence
            /// </summary>
            /// <summary>
            /// Quotients arised in PRS
            /// </summary>
            /// <summary>
            /// Initial polynomials
            /// </summary>
            public readonly Poly a, b;
            /// <summary>
            /// Polynomial remainder sequence
            /// </summary>
            /// <summary>
            /// Quotients arised in PRS
            /// </summary>
            /// <summary>
            /// Initial polynomials
            /// </summary>
            public APolynomialRemainderSequence(Poly a, Poly b)
            {
                this.a = a;
                this.b = b;
            }

            /// <summary>
            /// Polynomial remainder sequence
            /// </summary>
            /// <summary>
            /// Quotients arised in PRS
            /// </summary>
            /// <summary>
            /// Initial polynomials
            /// </summary>
            /// <summary>
            /// The last element in PRS, that is the GCD
            /// </summary>
            public Poly LastRemainder()
            {
                return remainders[remainders.Count - 1];
            }

            /// <summary>
            /// Polynomial remainder sequence
            /// </summary>
            /// <summary>
            /// Quotients arised in PRS
            /// </summary>
            /// <summary>
            /// Initial polynomials
            /// </summary>
            /// <summary>
            /// The last element in PRS, that is the GCD
            /// </summary>
            /// <summary>
            /// The last element in PRS, that is the GCD
            /// </summary>
            public Poly Gcd()
            {
                if (a.IsZero())
                    return b;
                if (b.IsZero())
                    return a;
                if (a.IsOverField())
                    return LastRemainder().Clone().Monic();
                Poly r = LastRemainder().Clone().PrimitivePartSameSign();
                return UnivariateGCD.PolynomialGCD(a.ContentAsPoly(), b.ContentAsPoly()).Multiply(r);
            }

            /// <summary>
            /// Polynomial remainder sequence
            /// </summary>
            /// <summary>
            /// Quotients arised in PRS
            /// </summary>
            /// <summary>
            /// Initial polynomials
            /// </summary>
            /// <summary>
            /// The last element in PRS, that is the GCD
            /// </summary>
            /// <summary>
            /// The last element in PRS, that is the GCD
            /// </summary>
            public int Size()
            {
                return remainders.Count;
            }
        }

        /// <summary>
        /// Polynomial remainder sequence (PRS). It also implements abstract division rule, used to build PRS. At each step
        /// of Euclidean algorithm the polynomials {@code qout, rem} and coefficients {@code alpha, beta} are computed so
        /// that {@code alpha_i r_(i - 2) = quot_(i - 1) * r_(i - 1) + beta_i * r_i} where {@code {r_i} } is PRS.
        /// </summary>
        public abstract class PolynomialRemainderSequence<E> : APolynomialRemainderSequence<UnivariatePolynomial<E>>
        {
            /// <summary>
            /// alpha coefficients
            /// </summary>
            public readonly IList<E> alphas = new List();
            /// <summary>
            /// alpha coefficients
            /// </summary>
            /// <summary>
            /// beta coefficients
            /// </summary>
            public readonly IList<E> betas = new List();
            /// <summary>
            /// alpha coefficients
            /// </summary>
            /// <summary>
            /// beta coefficients
            /// </summary>
            /// <summary>
            /// the ring
            /// </summary>
            readonly Ring<E> ring;
            /// <summary>
            /// alpha coefficients
            /// </summary>
            /// <summary>
            /// beta coefficients
            /// </summary>
            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            readonly bool swap;
            /// <summary>
            /// alpha coefficients
            /// </summary>
            /// <summary>
            /// beta coefficients
            /// </summary>
            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            PolynomialRemainderSequence(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) : base(a, b)
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
                    swap = a.degree % 2 == 1 && b.degree % 2 == 1; // both degrees are odd => odd permutation of Sylvester matrix
                }
            }

            /// <summary>
            /// alpha coefficients
            /// </summary>
            /// <summary>
            /// beta coefficients
            /// </summary>
            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            // both degrees are odd => odd permutation of Sylvester matrix
            /// <summary>
            /// compute alpha based on obtained so far PRS
            /// </summary>
            abstract E NextAlpha();
            /// <summary>
            /// alpha coefficients
            /// </summary>
            /// <summary>
            /// beta coefficients
            /// </summary>
            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            // both degrees are odd => odd permutation of Sylvester matrix
            /// <summary>
            /// compute alpha based on obtained so far PRS
            /// </summary>
            /// <summary>
            /// compute beta based on obtained so far PRS and newly computed remainder
            /// </summary>
            abstract E NextBeta(UnivariatePolynomial<E> remainder);
            /// <summary>
            /// alpha coefficients
            /// </summary>
            /// <summary>
            /// beta coefficients
            /// </summary>
            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            // both degrees are odd => odd permutation of Sylvester matrix
            /// <summary>
            /// compute alpha based on obtained so far PRS
            /// </summary>
            /// <summary>
            /// compute beta based on obtained so far PRS and newly computed remainder
            /// </summary>
            /// <summary>
            /// A single step of the Euclidean algorithm
            /// </summary>
            private UnivariatePolynomial<E> Step()
            {
                int i = remainders.Count;
                UnivariatePolynomial<E> dividend = remainders[i - 2].Clone(), divider = remainders[i - 1];
                E alpha = NextAlpha();
                dividend = dividend.Multiply(alpha);
                UnivariatePolynomial<E>[] qd = UnivariateDivision.DivideAndRemainder(dividend, divider, false);
                if (qd == null)
                    throw new Exception("exact division is not possible");
                UnivariatePolynomial<E> quotient = qd[0], remainder = qd[1];
                if (remainder.IsZero())

                    // remainder is zero => termination of the algorithm
                    return remainder;
                E beta = NextBeta(remainder);
                remainder = remainder.DivideExact(beta);
                alphas.Add(alpha);
                betas.Add(beta);
                quotients.Add(quotient);
                remainders.Add(remainder);
                return remainder;
            }

            /// <summary>
            /// alpha coefficients
            /// </summary>
            /// <summary>
            /// beta coefficients
            /// </summary>
            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            // both degrees are odd => odd permutation of Sylvester matrix
            /// <summary>
            /// compute alpha based on obtained so far PRS
            /// </summary>
            /// <summary>
            /// compute beta based on obtained so far PRS and newly computed remainder
            /// </summary>
            /// <summary>
            /// A single step of the Euclidean algorithm
            /// </summary>
            // remainder is zero => termination of the algorithm
            /// <summary>
            /// Run all steps.
            /// </summary>
            PolynomialRemainderSequence<E> Run()
            {
                if (LastRemainder().IsZero())

                    // on of the factors is zero
                    return this;
                while (!Step().IsZero())
                    ;
                return this;
            }

            /// <summary>
            /// alpha coefficients
            /// </summary>
            /// <summary>
            /// beta coefficients
            /// </summary>
            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            // both degrees are odd => odd permutation of Sylvester matrix
            /// <summary>
            /// compute alpha based on obtained so far PRS
            /// </summary>
            /// <summary>
            /// compute beta based on obtained so far PRS and newly computed remainder
            /// </summary>
            /// <summary>
            /// A single step of the Euclidean algorithm
            /// </summary>
            // remainder is zero => termination of the algorithm
            /// <summary>
            /// Run all steps.
            /// </summary>
            // on of the factors is zero
            /// <summary>
            /// n_i - n_{i+1}
            /// </summary>
            int DegreeDiff(int i)
            {
                return remainders[i].degree - remainders[i + 1].degree;
            }

            /// <summary>
            /// alpha coefficients
            /// </summary>
            /// <summary>
            /// beta coefficients
            /// </summary>
            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            // both degrees are odd => odd permutation of Sylvester matrix
            /// <summary>
            /// compute alpha based on obtained so far PRS
            /// </summary>
            /// <summary>
            /// compute beta based on obtained so far PRS and newly computed remainder
            /// </summary>
            /// <summary>
            /// A single step of the Euclidean algorithm
            /// </summary>
            // remainder is zero => termination of the algorithm
            /// <summary>
            /// Run all steps.
            /// </summary>
            // on of the factors is zero
            /// <summary>
            /// n_i - n_{i+1}
            /// </summary>
            /// <summary>
            /// scalar subresultants
            /// </summary>
            private readonly List<E> subresultants = new List();
            /// <summary>
            /// alpha coefficients
            /// </summary>
            /// <summary>
            /// beta coefficients
            /// </summary>
            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            // both degrees are odd => odd permutation of Sylvester matrix
            /// <summary>
            /// compute alpha based on obtained so far PRS
            /// </summary>
            /// <summary>
            /// compute beta based on obtained so far PRS and newly computed remainder
            /// </summary>
            /// <summary>
            /// A single step of the Euclidean algorithm
            /// </summary>
            // remainder is zero => termination of the algorithm
            /// <summary>
            /// Run all steps.
            /// </summary>
            // on of the factors is zero
            /// <summary>
            /// n_i - n_{i+1}
            /// </summary>
            /// <summary>
            /// scalar subresultants
            /// </summary>
            void ComputeSubresultants()
            {
                lock (this)
                {
                    if (!subresultants.IsEmpty())
                        return;
                    IList<E> subresultants = NonZeroSubresultants();
                    if (swap)
                        subresultants.ReplaceAll(ring.Negate());
                    this.subresultants.EnsureCapacity(remainders[1].degree);
                    for (int i = 0; i <= remainders[1].degree; ++i)
                        this.subresultants.Add(ring.GetZero());
                    for (int i = 1; i < remainders.Count; i++)
                        this.subresultants[remainders[i].degree] = subresultants[i - 1];
                }
            }

            /// <summary>
            /// alpha coefficients
            /// </summary>
            /// <summary>
            /// beta coefficients
            /// </summary>
            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            // both degrees are odd => odd permutation of Sylvester matrix
            /// <summary>
            /// compute alpha based on obtained so far PRS
            /// </summary>
            /// <summary>
            /// compute beta based on obtained so far PRS and newly computed remainder
            /// </summary>
            /// <summary>
            /// A single step of the Euclidean algorithm
            /// </summary>
            // remainder is zero => termination of the algorithm
            /// <summary>
            /// Run all steps.
            /// </summary>
            // on of the factors is zero
            /// <summary>
            /// n_i - n_{i+1}
            /// </summary>
            /// <summary>
            /// scalar subresultants
            /// </summary>
            /// <summary>
            /// general setting for Fundamental Theorem of Resultant Theory
            /// </summary>
            virtual IList<E> NonZeroSubresultants()
            {
                IList<E> subresultants = new List();

                // largest subresultant
                E subresultant = ring.Pow(remainders[1].Lc(), DegreeDiff(0));
                subresultants.Add(subresultant);
                for (int i = 1; i < (remainders.Count - 1); ++i)
                {

                    // computing (i+1)-th degree subresultant
                    int di = DegreeDiff(i);
                    E rho = ring.Pow(ring.Multiply(remainders[i + 1].Lc(), remainders[i].Lc()), di);
                    E den = ring.GetOne();
                    for (int j = 1; j <= i; ++j)
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

            /// <summary>
            /// alpha coefficients
            /// </summary>
            /// <summary>
            /// beta coefficients
            /// </summary>
            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            // both degrees are odd => odd permutation of Sylvester matrix
            /// <summary>
            /// compute alpha based on obtained so far PRS
            /// </summary>
            /// <summary>
            /// compute beta based on obtained so far PRS and newly computed remainder
            /// </summary>
            /// <summary>
            /// A single step of the Euclidean algorithm
            /// </summary>
            // remainder is zero => termination of the algorithm
            /// <summary>
            /// Run all steps.
            /// </summary>
            // on of the factors is zero
            /// <summary>
            /// n_i - n_{i+1}
            /// </summary>
            /// <summary>
            /// scalar subresultants
            /// </summary>
            /// <summary>
            /// general setting for Fundamental Theorem of Resultant Theory
            /// </summary>
            // largest subresultant
            // computing (i+1)-th degree subresultant
            /// <summary>
            /// Gives a list of scalar subresultant where i-th list element is i-th subresultant.
            /// </summary>
            public IList<E> GetSubresultants()
            {
                if (subresultants.IsEmpty())
                    ComputeSubresultants();
                return subresultants;
            }

            /// <summary>
            /// alpha coefficients
            /// </summary>
            /// <summary>
            /// beta coefficients
            /// </summary>
            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            // both degrees are odd => odd permutation of Sylvester matrix
            /// <summary>
            /// compute alpha based on obtained so far PRS
            /// </summary>
            /// <summary>
            /// compute beta based on obtained so far PRS and newly computed remainder
            /// </summary>
            /// <summary>
            /// A single step of the Euclidean algorithm
            /// </summary>
            // remainder is zero => termination of the algorithm
            /// <summary>
            /// Run all steps.
            /// </summary>
            // on of the factors is zero
            /// <summary>
            /// n_i - n_{i+1}
            /// </summary>
            /// <summary>
            /// scalar subresultants
            /// </summary>
            /// <summary>
            /// general setting for Fundamental Theorem of Resultant Theory
            /// </summary>
            // largest subresultant
            // computing (i+1)-th degree subresultant
            /// <summary>
            /// Gives a list of scalar subresultant where i-th list element is i-th subresultant.
            /// </summary>
            /// <summary>
            /// Resultant of initial polynomials
            /// </summary>
            public E Resultant()
            {
                return GetSubresultants()[0];
            }
        }

        /// <summary>
        /// Classical division rule with alpha = beta = 1
        /// </summary>
        private sealed class ClassicalPolynomialRemainderSequence<E> : PolynomialRemainderSequence<E>
        {
            ClassicalPolynomialRemainderSequence(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) : base(a, b)
            {
            }

            override E NextAlpha()
            {
                return ring.GetOne();
            }

            override E NextBeta(UnivariatePolynomial<E> remainder)
            {
                return ring.GetOne();
            }

            override IList<E> NonZeroSubresultants()
            {
                IList<E> subresultants = new List();

                // largest subresultant
                E subresultant = ring.Pow(remainders[1].Lc(), DegreeDiff(0));
                subresultants.Add(subresultant);
                for (int i = 1; i < (remainders.Count - 1); ++i)
                {

                    // computing (i+1)-th degree subresultant
                    int di = DegreeDiff(i);
                    E rho = ring.Pow(ring.Multiply(remainders[i + 1].Lc(), remainders[i].Lc()), di);
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
            PseudoPolynomialRemainderSequence(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) : base(a, b)
            {
            }

            override E NextAlpha()
            {
                int i = remainders.Count;
                E lc = remainders[i - 1].Lc();
                int deg = remainders[i - 2].degree - remainders[i - 1].degree;
                return ring.Pow(lc, deg + 1);
            }

            override E NextBeta(UnivariatePolynomial<E> remainder)
            {
                return ring.GetOne();
            }
        }

        /// <summary>
        /// Reduced pseudo-division
        /// </summary>
        private sealed class ReducedPolynomialRemainderSequence<E> : PseudoPolynomialRemainderSequence<E>
        {
            ReducedPolynomialRemainderSequence(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) : base(a, b)
            {
            }

            override E NextBeta(UnivariatePolynomial<E> remainder)
            {
                return alphas.IsEmpty() ? ring.GetOne() : alphas[alphas.Count - 1];
            }

            override IList<E> NonZeroSubresultants()
            {
                IList<E> subresultants = new List();

                // largest subresultant
                E subresultant = ring.Pow(remainders[1].Lc(), DegreeDiff(0));
                subresultants.Add(subresultant);
                for (int i = 1; i < (remainders.Count - 1); ++i)
                {

                    // computing (i+1)-th degree subresultant
                    int di = DegreeDiff(i);
                    E rho = ring.Pow(remainders[i + 1].Lc(), di);
                    E den = ring.Pow(remainders[i].Lc(), DegreeDiff(i - 1) * di);
                    subresultant = ring.Multiply(subresultant, rho);
                    subresultant = ring.DivideExact(subresultant, den);
                    if ((di % 2) == 1 && (remainders[0].degree - remainders[i + 1].degree + i + 1) % 2 == 1)
                        subresultant = ring.Negate(subresultant);
                    subresultants.Add(subresultant);
                }

                return subresultants;
            }
        }

        /// <summary>
        /// Primitive pseudo-division
        /// </summary>
        private sealed class PrimitivePolynomialRemainderSequence<E> : PseudoPolynomialRemainderSequence<E>
        {
            PrimitivePolynomialRemainderSequence(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) : base(a, b)
            {
            }

            override E NextBeta(UnivariatePolynomial<E> remainder)
            {
                return remainder.Content();
            }
        }

        /// <summary>
        /// Subresultant sequence
        /// </summary>
        private sealed class SubresultantPolynomialRemainderSequence<E> : PseudoPolynomialRemainderSequence<E>
        {
            readonly IList<E> psis = new List();
            SubresultantPolynomialRemainderSequence(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) : base(a, b)
            {
            }

            override E NextBeta(UnivariatePolynomial<E> remainder)
            {
                int i = remainders.Count;
                UnivariatePolynomial<E> prem = remainders[i - 2];
                E lc = i == 2 ? ring.GetOne() : prem.Lc();
                E psi;
                if (i == 2)
                    psi = ring.GetNegativeOne();
                else
                {
                    E prevPsi = psis[psis.Count - 1];
                    int deg = remainders[i - 3].degree - remainders[i - 2].degree;
                    E f = ring.Pow(ring.Negate(lc), deg);
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
                int e = DegreeDiff(j - 1);
                for (int k = j; k <= i; ++k)
                    e *= 1 - DegreeDiff(k);
                return e;
            }

            override IList<E> NonZeroSubresultants()
            {
                IList<E> subresultants = new List();

                // largest subresultant
                E subresultant = ring.Pow(remainders[1].Lc(), DegreeDiff(0));
                subresultants.Add(subresultant);
                for (int i = 1; i < (remainders.Count - 1); ++i)
                {

                    // computing (i+1)-th degree subresultant
                    int di = DegreeDiff(i);
                    E rho = ring.Pow(remainders[i + 1].Lc(), di);
                    E den = ring.GetOne();
                    for (int k = 1; k <= i; ++k)
                    {
                        int deg = -di * Eij(i - 1, k);
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

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Classical division rule for polynomials over Zp
        /// </summary>
        public sealed class PolynomialRemainderSequenceZp64 : APolynomialRemainderSequence<UnivariatePolynomialZp64>
        {
            /// <summary>
            /// the ring
            /// </summary>
            readonly IntegersZp64 ring;
            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            readonly bool swap;
            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            PolynomialRemainderSequenceZp64(UnivariatePolynomialZp64 a, UnivariatePolynomialZp64 b) : base(a, b)
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
                    swap = a.degree % 2 == 1 && b.degree % 2 == 1; // both degrees are odd => odd permutation of Sylvester matrix
                }
            }

            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            // both degrees are odd => odd permutation of Sylvester matrix
            /// <summary>
            /// A single step of the Euclidean algorithm
            /// </summary>
            private UnivariatePolynomialZp64 Step()
            {
                int i = remainders.Count;
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

            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            // both degrees are odd => odd permutation of Sylvester matrix
            /// <summary>
            /// A single step of the Euclidean algorithm
            /// </summary>
            // remainder is zero => termination of the algorithm
            /// <summary>
            /// Run all steps.
            /// </summary>
            private PolynomialRemainderSequenceZp64 Run()
            {
                while (!Step().IsZero())
                    ;
                return this;
            }

            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            // both degrees are odd => odd permutation of Sylvester matrix
            /// <summary>
            /// A single step of the Euclidean algorithm
            /// </summary>
            // remainder is zero => termination of the algorithm
            /// <summary>
            /// Run all steps.
            /// </summary>
            /// <summary>
            /// n_i - n_{i+1}
            /// </summary>
            int DegreeDiff(int i)
            {
                return remainders[i].degree - remainders[i + 1].degree;
            }

            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            // both degrees are odd => odd permutation of Sylvester matrix
            /// <summary>
            /// A single step of the Euclidean algorithm
            /// </summary>
            // remainder is zero => termination of the algorithm
            /// <summary>
            /// Run all steps.
            /// </summary>
            /// <summary>
            /// n_i - n_{i+1}
            /// </summary>
            /// <summary>
            /// scalar subresultants
            /// </summary>
            private readonly TLongArrayList subresultants = new TLongArrayList();
            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            // both degrees are odd => odd permutation of Sylvester matrix
            /// <summary>
            /// A single step of the Euclidean algorithm
            /// </summary>
            // remainder is zero => termination of the algorithm
            /// <summary>
            /// Run all steps.
            /// </summary>
            /// <summary>
            /// n_i - n_{i+1}
            /// </summary>
            /// <summary>
            /// scalar subresultants
            /// </summary>
            void ComputeSubresultants()
            {
                lock (this)
                {
                    if (!subresultants.IsEmpty())
                        return;
                    TLongArrayList subresultants = NonZeroSubresultants();
                    if (swap)
                        for (int i = 0; i < subresultants.Count; ++i)
                            subresultants[i] = ring.Negate(subresultants[i]);
                    this.subresultants.EnsureCapacity(remainders[1].degree);
                    for (int i = 0; i <= remainders[1].degree; ++i)
                        this.subresultants.Add(0);
                    for (int i = 1; i < remainders.Count; i++)
                        this.subresultants[remainders[i].degree] = subresultants[i - 1];
                }
            }

            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            // both degrees are odd => odd permutation of Sylvester matrix
            /// <summary>
            /// A single step of the Euclidean algorithm
            /// </summary>
            // remainder is zero => termination of the algorithm
            /// <summary>
            /// Run all steps.
            /// </summary>
            /// <summary>
            /// n_i - n_{i+1}
            /// </summary>
            /// <summary>
            /// scalar subresultants
            /// </summary>
            /// <summary>
            /// general setting for Fundamental Theorem of Resultant Theory
            /// </summary>
            TLongArrayList NonZeroSubresultants()
            {
                TLongArrayList subresultants = new TLongArrayList();

                // largest subresultant
                long subresultant = ring.PowMod(remainders[1].Lc(), DegreeDiff(0));
                subresultants.Add(subresultant);
                for (int i = 1; i < (remainders.Count - 1); ++i)
                {

                    // computing (i+1)-th degree subresultant
                    int di = DegreeDiff(i);
                    long rho = ring.PowMod(ring.Multiply(remainders[i + 1].Lc(), remainders[i].Lc()), di);
                    if ((di % 2) == 1 && (remainders[0].degree - remainders[i + 1].degree + i + 1) % 2 == 1)
                        rho = ring.Negate(rho);
                    subresultant = ring.Multiply(subresultant, rho);
                    subresultants.Add(subresultant);
                }

                return subresultants;
            }

            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            // both degrees are odd => odd permutation of Sylvester matrix
            /// <summary>
            /// A single step of the Euclidean algorithm
            /// </summary>
            // remainder is zero => termination of the algorithm
            /// <summary>
            /// Run all steps.
            /// </summary>
            /// <summary>
            /// n_i - n_{i+1}
            /// </summary>
            /// <summary>
            /// scalar subresultants
            /// </summary>
            /// <summary>
            /// general setting for Fundamental Theorem of Resultant Theory
            /// </summary>
            // largest subresultant
            // computing (i+1)-th degree subresultant
            /// <summary>
            /// Gives a list of scalar subresultant where i-th list element is i-th subresultant.
            /// </summary>
            public TLongArrayList GetSubresultants()
            {
                if (subresultants.IsEmpty())
                    ComputeSubresultants();
                return subresultants;
            }

            /// <summary>
            /// the ring
            /// </summary>
            /// <summary>
            /// whether the first poly had smaller degree than the second
            /// </summary>
            // both degrees are odd => odd permutation of Sylvester matrix
            /// <summary>
            /// A single step of the Euclidean algorithm
            /// </summary>
            // remainder is zero => termination of the algorithm
            /// <summary>
            /// Run all steps.
            /// </summary>
            /// <summary>
            /// n_i - n_{i+1}
            /// </summary>
            /// <summary>
            /// scalar subresultants
            /// </summary>
            /// <summary>
            /// general setting for Fundamental Theorem of Resultant Theory
            /// </summary>
            // largest subresultant
            // computing (i+1)-th degree subresultant
            /// <summary>
            /// Gives a list of scalar subresultant where i-th list element is i-th subresultant.
            /// </summary>
            /// <summary>
            /// Resultant of initial polynomials
            /// </summary>
            public long Resultant()
            {
                return GetSubresultants()[0];
            }
        }
    }
}