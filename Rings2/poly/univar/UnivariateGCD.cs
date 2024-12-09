using System.Numerics;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Poly.Multivar;
using Cc.Redberry.Rings.Primes;
using Cc.Redberry.Rings.Util;

namespace Cc.Redberry.Rings.Poly.Univar
{
    /// <summary>
    /// Univariate polynomial GCD.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class UnivariateGCD
    {
        private UnivariateGCD()
        {
        }

        /// <summary>
        /// Calculates the GCD of two polynomials. Depending on the coefficient ring, the algorithm switches between Half-GCD
        /// (polys over finite fields), modular GCD (polys over Z and Q) and subresultant Euclid (other rings).
        /// </summary>
        /// <param name="a">the first polynomial</param>
        /// <param name="b">the second polynomial</param>
        /// <returns>GCD of two polynomials</returns>
        public static T PolynomialGCD<T>(T a, T b)  where T : IUnivariatePolynomial<T>
        {
            a.AssertSameCoefficientRingWith(b);
            if (Util.IsOverMultipleFieldExtension(a))
                return (T)PolynomialGCDInMultipleFieldExtension((UnivariatePolynomial)a, (UnivariatePolynomial)b);
            if (a.IsOverFiniteField())
                return HalfGCD(a, b);
            if (a is UnivariatePolynomialZ64)
                return (T)ModularGCD((UnivariatePolynomialZ64)a, (UnivariatePolynomialZ64)b);
            if (a.IsOverZ())
                return (T)ModularGCD((UnivariatePolynomial)a, (UnivariatePolynomial)b);
            if (Util.IsOverRationals(a))
                return (T)PolynomialGCDInQ((UnivariatePolynomial)a, (UnivariatePolynomial)b);
            if (Util.IsOverRingOfIntegersOfSimpleNumberField(a))
                return (T)PolynomialGCDInRingOfIntegersOfNumberField((UnivariatePolynomial)a, (UnivariatePolynomial)b);
            if (Util.IsOverSimpleNumberField(a))
                return (T)PolynomialGCDInNumberField((UnivariatePolynomial)a, (UnivariatePolynomial)b);
            if (a.IsOverField())
                return HalfGCD(a, b);
            T r = TryNested(a, b);
            if (r != null)
                return r;
            T t = TrivialGCD(a, b);
            if (t != null)
                return t;
            return (T)UnivariateResultants.SubresultantPRS((UnivariatePolynomial)a, (UnivariatePolynomial)b).Gcd();
        }

        private static T TrivialGCD<T>(T a, T b) where T : IUnivariatePolynomial<T>
        {
            if (a.IsConstant() || b.IsConstant())
            {
                if (a.IsOverField())
                    return a.CreateOne();
                else if (a is UnivariatePolynomialZ64)
                    return a.CreateConstant(MachineArithmetic.Gcd(((UnivariatePolynomialZ64)a).Content(), ((UnivariatePolynomialZ64)b).Content()));
                else if (a is UnivariatePolynomial)
                    return (T)((UnivariatePolynomial)a).CreateConstant(((UnivariatePolynomial)a).ring.Gcd(((UnivariatePolynomial)a).Content(), ((UnivariatePolynomial)b).Content()));
            }

            return null;
        }

        private static T TryNested<T>(T a, T b) where T : IUnivariatePolynomial<T>
        {
            if (a is UnivariatePolynomial && ((UnivariatePolynomial)a).ring is MultivariateRing)
                return (T)PolynomialGCDOverMultivariate((UnivariatePolynomial)a, (UnivariatePolynomial)b);
            return null;
        }

        private static UnivariatePolynomial<Poly> PolynomialGCDOverMultivariate<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(UnivariatePolynomial<Poly> a, UnivariatePolynomial<Poly> b)
        {
            return MultivariateGCD.PolynomialGCD(AMultivariatePolynomial.AsMultivariate(a, 0, true), AMultivariatePolynomial.AsMultivariate(b, 0, true)).AsUnivariateEliminate(0);
        }

        private static UnivariatePolynomial<Rational<E>> PolynomialGCDInQ<E>(UnivariatePolynomial<Rational<E>> a, UnivariatePolynomial<Rational<E>> b)
        {
            Tuple2<UnivariatePolynomial<E>, E> aRat = ToCommonDenominator(a);
            Tuple2<UnivariatePolynomial<E>, E> bRat = ToCommonDenominator(b);
            return Util.AsOverRationals(a.ring, PolynomialGCD(aRat._1, bRat._1)).Monic();
        }

        private static UnivariatePolynomial<mPoly> PolynomialGCDInMultipleFieldExtension<Term extends AMonomial<Term>, mPoly extends AMultivariatePolynomial<Term, mPoly>, sPoly extends IUnivariatePolynomial<sPoly>>(UnivariatePolynomial<mPoly> a, UnivariatePolynomial<mPoly> b)
        {
            MultipleFieldExtension<Term, mPoly, sPoly> ring = (MultipleFieldExtension<Term, mPoly, sPoly>)a.ring;
            SimpleFieldExtension<sPoly> simpleExtension = ring.GetSimpleExtension();
            return PolynomialGCD(a.MapCoefficients(simpleExtension, ring.Inverse()), b.MapCoefficients(simpleExtension, ring.Inverse())).MapCoefficients(ring, ring.Image());
        }

        /// <summary>
        /// Computes {@code [gcd(a,b), s, t]} such that {@code s * a + t * b = gcd(a, b)}. Either resultant-based modular
        /// or Half-GCD algorithm is used.
        /// </summary>
        /// <param name="a">the polynomial</param>
        /// <param name="b">the polynomial</param>
        /// <returns>array of {@code [gcd(a,b), s, t]} such that {@code s * a + t * b = gcd(a, b)} (gcd is monic)</returns>
        /// <remarks>@see#ExtendedHalfGCD(IUnivariatePolynomial, IUnivariatePolynomial)</remarks>
        public static T[] PolynomialExtendedGCD<T>(T a, T b) where T : IUnivariatePolynomial<T>
        {
            if (Util.IsOverQ(a))
                return (T[])ModularExtendedResultantGCDInQ((UnivariatePolynomial)a, (UnivariatePolynomial)b);
            if (a.IsOverZ())
                return (T[])ModularExtendedResultantGCDInZ((UnivariatePolynomial)a, (UnivariatePolynomial)b);
            if (a.IsOverField())
                return ExtendedHalfGCD(a, b);
            else
                throw new ArgumentException("Polynomial over field is expected");
        }

        /// <summary>
        /// Returns array of {@code [gcd(a,b), s]} such that {@code s * a + t * b = gcd(a, b)}
        /// </summary>
        /// <param name="a">the first poly for which the Bezout coefficient is computed</param>
        /// <param name="b">the second poly</param>
        /// <returns>array of {@code [gcd(a,b), s]} such that {@code s * a + t * b = gcd(a, b)}</returns>
        public static T[] PolynomialFirstBezoutCoefficient<T>(T a, T b) where T : IUnivariatePolynomial<T>
        {
            if (a.IsOverFiniteField() && Math.Min(a.Degree(), b.Degree()) < 384)

                // this is somewhat faster than computing full xGCD
                return EuclidFirstBezoutCoefficient(a, b);
            else
                return Arrays.CopyOf(PolynomialExtendedGCD(a, b), 2);
        }

        static UnivariatePolynomial<BigInteger>[] PolynomialExtendedGCDInZbyQ(UnivariatePolynomial<BigInteger> a, UnivariatePolynomial<BigInteger> b)
        {
            UnivariatePolynomial<Rational<BigInteger>>[] xgcd = PolynomialExtendedGCD(a.MapCoefficients(Q, Q.MkNumerator()), b.MapCoefficients(Q, Q.MkNumerator()));
            Tuple2<UnivariatePolynomial<BigInteger>, BigInteger> gcd = ToCommonDenominator(xgcd[0]), s = ToCommonDenominator(xgcd[1]), t = ToCommonDenominator(xgcd[2]);
            BigInteger lcm = Z.Lcm(gcd._2, s._2, t._2);
            return new UnivariatePolynomial<BigInteger>[]
            {
                gcd._1.Multiply(lcm.DivideExact(gcd._2)),
                s._1.Multiply(lcm.DivideExact(s._2)),
                t._1.Multiply(lcm.DivideExact(t._2))
            };
        }

        /// <summary>
        /// Returns GCD of a list of polynomials.
        /// </summary>
        /// <param name="polynomials">a set of polynomials</param>
        /// <returns>GCD of polynomials</returns>
        public static T PolynomialGCD<T>(params T[] polynomials) where T : IUnivariatePolynomial<T>
        {
            T gcd = polynomials[0];
            for (int i = 1; i < polynomials.Length; i++)
                gcd = PolynomialGCD(gcd, polynomials[i]);
            return gcd;
        }

        /// <summary>
        /// Returns GCD of a list of polynomials.
        /// </summary>
        /// <param name="polynomials">a set of polynomials</param>
        /// <returns>GCD of polynomials</returns>
        public static T PolynomialGCD<T>(IEnumerable<T> polynomials)  where T : IUnivariatePolynomial<T>
        {
            T gcd = null;
            foreach (T poly in polynomials)
                gcd = gcd == null ? poly : PolynomialGCD(gcd, poly);
            return gcd;
        }

        /* ========================================== implementation ==================================================== */
        private static T TrivialGCD<T>(T a, T b)  where T : IUnivariatePolynomial<T>
        {
            if (a.IsZero())
                return NormalizeGCD(b.Clone());
            if (b.IsZero())
                return NormalizeGCD(a.Clone());
            if (a.IsOne())
                return a.Clone();
            if (b.IsOne())
                return b.Clone();
            if (a == b)
                return NormalizeGCD(a.Clone());
            return null;
        }

        /// <summary>
        /// Returns the GCD calculated with Euclidean algorithm. If coefficient ring of the input is not a field (and thus
        /// polynomials does not form an integral ring), {@code ArithmeticException} may be thrown in case when some exact
        /// divisions are not possible.
        /// </summary>
        /// <param name="a">poly</param>
        /// <param name="b">poly</param>
        /// <returns>the GCD (monic if a and b are over field)</returns>
        public static T EuclidGCD<T>(T a, T b)  where T : IUnivariatePolynomial<T>
        {
            a.AssertSameCoefficientRingWith(b);
            T trivialGCD = TrivialGCD(a, b);
            if (trivialGCD != null)
                return trivialGCD;
            if (CanConvertToZp64(a))
                return Conversions64bit.Convert(EuclidGCD(AsOverZp64(a), AsOverZp64(b)));
            if (a.Degree() < b.Degree())
                return EuclidGCD(b, a);
            T x = a, y = b, r;
            while (true)
            {
                r = UnivariateDivision.Remainder(x, y, true);
                if (r == null)
                    throw new ArithmeticException("Not divisible with remainder: (" + x + ") / (" + y + ")");
                if (r.IsZero())
                    break;
                x = y;
                y = r;
            }

            return NormalizeGCD(y == a ? y.Clone() : (y == b ? y.Clone() : y));
        }

        private static T NormalizeGCD<T>(T gcd) where T : IUnivariatePolynomial<T>
        {
            if (gcd.IsOverField())
                return gcd.Monic();
            else
                return gcd;
        }

        /// <summary>
        /// Runs extended Euclidean algorithm to compute {@code [gcd(a,b), s, t]} such that {@code s * a + t * b = gcd(a,
        /// b)}. If coefficient ring of the input is not a field (and thus polynomials does not form an integral ring),
        /// {@code ArithmeticException} may be thrown in case when some exact divisions are not possible.
        /// </summary>
        /// <param name="a">the polynomial</param>
        /// <param name="b">the polynomial</param>
        /// <returns>array of {@code [gcd(a,b), s, t]} such that {@code s * a + t * b = gcd(a, b)}</returns>
        public static T[] ExtendedEuclidGCD<T>(T a, T b) where T : IUnivariatePolynomial<T>
        {
            a.AssertSameCoefficientRingWith(b);
            if (CanConvertToZp64(a))
                return Conversions64bit.Convert(a, ExtendedEuclidGCD(AsOverZp64(a), AsOverZp64(b)));
            T s = a.CreateZero(), old_s = a.CreateOne();
            T t = a.CreateOne(), old_t = a.CreateZero();
            T r = b.Clone(), old_r = a.Clone();
            T q;
            T tmp;
            while (!r.IsZero())
            {
                q = UnivariateDivision.Quotient(old_r, r, true);
                if (q == null)
                    throw new ArithmeticException("Not divisible with remainder: (" + old_r + ") / (" + r + ")");
                tmp = old_r;
                old_r = r;
                r = tmp.Clone().Subtract(q.Clone().Multiply(r));
                tmp = old_s;
                old_s = s;
                s = tmp.Clone().Subtract(q.Clone().Multiply(s));
                tmp = old_t;
                old_t = t;
                t = tmp.Clone().Subtract(q.Clone().Multiply(t));
            }

            T[] result = a.CreateArray(3);
            result[0] = old_r;
            result[1] = old_s;
            result[2] = old_t;
            return NormalizeExtendedGCD(result);
        }

        static T[] NormalizeExtendedGCD<T>(T[] xgcd) where T : IUnivariatePolynomial<T>
        {
            if (!xgcd[0].IsOverField())
                return xgcd;
            if (xgcd[0].IsZero())
                return xgcd;
            xgcd[1].DivideByLC(xgcd[0]);
            if (xgcd.Length > 2)
                xgcd[2].DivideByLC(xgcd[0]);
            xgcd[0].Monic();
            return xgcd;
        }

        /// <summary>
        /// Returns array of {@code [gcd(a,b), s]} such that {@code s * a + t * b = gcd(a, b)}
        /// </summary>
        /// <param name="a">the first poly for which the Bezout coefficient is computed</param>
        /// <param name="b">the second poly</param>
        /// <returns>array of {@code [gcd(a,b), s]} such that {@code s * a + t * b = gcd(a, b)}</returns>
        public static T[] EuclidFirstBezoutCoefficient<T>(T a, T b) where T : IUnivariatePolynomial<T>
        {
            a.AssertSameCoefficientRingWith(b);
            if (CanConvertToZp64(a))
                return Conversions64bit.Convert(a, EuclidFirstBezoutCoefficient(AsOverZp64(a), AsOverZp64(b)));
            T s = a.CreateZero(), old_s = a.CreateOne();
            T r = b, old_r = a;
            T q;
            T tmp;
            while (!r.IsZero())
            {
                q = UnivariateDivision.Quotient(old_r, r, true);
                if (q == null)
                    throw new ArithmeticException("Not divisible with remainder: (" + old_r + ") / (" + r + ")");
                tmp = old_r;
                old_r = r;
                r = tmp.Clone().Subtract(q.Clone().Multiply(r));
                tmp = old_s;
                old_s = s;
                s = tmp.Clone().Subtract(q.Clone().Multiply(s));
            }

            T[] result = a.CreateArray(2);
            result[0] = old_r;
            result[1] = old_s;
            return NormalizeExtendedGCD(result);
        }

        /// <summary>
        /// for polynomial degrees larger than this a Half-GCD algorithm will be used
        /// </summary>
        static int SWITCH_TO_HALF_GCD_ALGORITHM_DEGREE = 180;
        /// <summary>
        /// for polynomial degrees larger than this a Half-GCD algorithm for hMatrix will be used
        /// </summary>
        static int SWITCH_TO_HALF_GCD_H_MATRIX_DEGREE = 25;
        /// <summary>
        /// Half-GCD algorithm. The algorithm automatically switches to Euclidean algorithm for small input. If coefficient
        /// ring of the input is not a field (and thus polynomials does not form an integral ring), {@code
        /// ArithmeticException} may be thrown in case when some exact divisions are not possible.
        /// </summary>
        /// <param name="a">poly</param>
        /// <param name="b">poly</param>
        /// <returns>the GCD (monic)</returns>
        public static T HalfGCD<T>(T a, T b) where T : IUnivariatePolynomial<T>
        {
            a.AssertSameCoefficientRingWith(b);
            T trivialGCD = TrivialGCD(a, b);
            if (trivialGCD != null)
                return trivialGCD;
            if (CanConvertToZp64(a))
                return Conversions64bit.Convert(HalfGCD(AsOverZp64(a), AsOverZp64(b)));
            if (a.Degree() < b.Degree())
                return HalfGCD(b, a);
            if (a.Degree() == b.Degree())
                b = UnivariateDivision.Remainder(b, a, true);
            while (a.Degree() > SWITCH_TO_HALF_GCD_ALGORITHM_DEGREE && !b.IsZero())
            {
                T[] col = ReduceHalfGCD(a, b);
                a = col[0];
                b = col[1];
                if (!b.IsZero())
                {
                    T remainder = UnivariateDivision.Remainder(a, b, true);
                    if (remainder == null)
                        throw new ArithmeticException("Not divisible with remainder: (" + a + ") / (" + b + ")");
                    a = b;
                    b = remainder;
                }
            }

            return UnivariateGCD.EuclidGCD(a, b);
        }

        /// <summary>
        /// Runs extended Half-GCD algorithm to compute {@code [gcd(a,b), s, t]} such that {@code s * a + t * b = gcd(a, b)}.
        /// If coefficient ring of the input is not a field (and thus polynomials does not form an integral ring), {@code
        /// ArithmeticException} may be thrown in case when some exact divisions are not possible.
        /// </summary>
        /// <param name="a">the polynomial</param>
        /// <param name="b">the polynomial</param>
        /// <returns>array of {@code [gcd(a,b), s, t]} such that {@code s * a + t * b = gcd(a, b)} (gcd is monic)</returns>
        public static T[] ExtendedHalfGCD<T>(T a, T b) where T : IUnivariatePolynomial<T>
        {
            a.AssertSameCoefficientRingWith(b);
            if (CanConvertToZp64(a))
                return Conversions64bit.Convert(a, ExtendedHalfGCD(AsOverZp64(a), AsOverZp64(b)));
            if (a.Degree() < b.Degree())
            {
                T[] r = ExtendedHalfGCD(b, a);
                ArraysUtil.Swap(r, 1, 2);
                return r;
            }

            if (b.IsZero())
            {
                T[] result = a.CreateArray(3);
                result[0] = a.Clone();
                result[1] = a.CreateOne();
                result[2] = a.CreateZero();
                return NormalizeExtendedGCD(result);
            }

            a = a.Clone();
            b = b.Clone();
            T quotient = null;
            if (a.Degree() == b.Degree())
            {
                T[] qd = UnivariateDivision.DivideAndRemainder(a, b, true);
                if (qd == null)
                    throw new ArithmeticException("Not divisible with remainder: (" + a + ") / (" + b + ")");
                quotient = qd[0];
                T remainder = qd[1];
                a = b;
                b = remainder;
            }

            T[, ] hMatrix = ReduceExtendedHalfGCD(a, b, a.Degree() + 1);
            T gcd = a, s, t;
            if (quotient != null)
            {
                s = hMatrix[0,1];
                t = quotient.Multiply(hMatrix[0,1]);
                t = hMatrix[0,0].Subtract(t);
            }
            else
            {
                s = hMatrix[0,0];
                t = hMatrix[0,1];
            }

            T[] result = a.CreateArray(3);
            result[0] = gcd;
            result[1] = s;
            result[2] = t;
            return NormalizeExtendedGCD(result);
        }

        /// <summary>
        /// </summary>
        /// <param name="reduce">whether to reduce a and b inplace</param>
        private static T[,] HMatrixPlain<T>(T a, T b, int degreeToReduce, bool reduce) where T : IUnivariatePolynomial<T>
        {
            T[, ] hMatrix = UnitMatrix(a);
            int goal = a.Degree() - degreeToReduce;
            if (b.Degree() <= goal)
                return hMatrix;
            T tmpA = a, tmpB = b;
            while (tmpB.Degree() > goal && !tmpB.IsZero())
            {
                T[] qd = UnivariateDivision.DivideAndRemainder(tmpA, tmpB, true);
                if (qd == null)
                    throw new ArithmeticException("Not divisible with remainder: (" + tmpA + ") / (" + tmpB + ")");
                T quotient = qd[0], remainder = qd[1];
                T tmp;
                tmp = quotient.Clone().Multiply(hMatrix[1,0]);
                tmp = hMatrix[0,0].Clone().Subtract(tmp);
                hMatrix[0,0] = hMatrix[1,0];
                hMatrix[1,0] = tmp;
                tmp = quotient.Clone().Multiply(hMatrix[1,1]);
                tmp = hMatrix[0,1].Clone().Subtract(tmp);
                hMatrix[0,1] = hMatrix[1,1];
                hMatrix[1,1] = tmp;
                tmpA = tmpB;
                tmpB = remainder;
            }

            if (reduce)
            {
                a.SetAndDestroy(tmpA);
                b.SetAndDestroy(tmpB);
            }

            return hMatrix;
        }

        private static T[,] HMatrixHalfGCD<T>(T a, T b, int d) where T : IUnivariatePolynomial<T>
        {
            if (b.IsZero() || b.Degree() <= a.Degree() - d)
                return UnitMatrix(a);
            int n = a.Degree() - 2 * d + 2;
            if (n < 0)
                n = 0;
            T a1 = a.Clone().ShiftLeft(n);
            T b1 = b.Clone().ShiftLeft(n);
            if (d <= SWITCH_TO_HALF_GCD_H_MATRIX_DEGREE)
                return HMatrixPlain(a1, b1, d, false);
            int dR = (d + 1) / 2;
            if (dR < 1)
                dR = 1;
            if (dR >= d)
                dR = d - 1;
            T[, ] hMatrixR = HMatrixHalfGCD(a1, b1, dR);
            T[] col = ColumnMultiply(hMatrixR, a1, b1);
            a1 = col[0];
            b1 = col[1];
            int dL = b1.Degree() - a.Degree() + n + d;
            if (b1.IsZero() || dL <= 0)
                return hMatrixR;
            T[] qd = UnivariateDivision.DivideAndRemainder(a1, b1, false);
            if (qd == null)
                throw new ArithmeticException("Not divisible with remainder: (" + a1 + ") / (" + b1 + ")");
            T quotient = qd[0], remainder = qd[1];
            T[, ] hMatrixL = HMatrixHalfGCD(b1, remainder, dL);
            T tmp;
            tmp = quotient.Clone().Multiply(hMatrixR[1,0]);
            tmp = hMatrixR[0,0].Clone().Subtract(tmp);
            hMatrixR[0,0] = hMatrixR[1,0];
            hMatrixR[1,0] = tmp;
            tmp = quotient.Clone().Multiply(hMatrixR[1,1]);
            tmp = hMatrixR[0,1].Clone().Subtract(tmp);
            hMatrixR[0,1] = hMatrixR[1,1];
            hMatrixR[1,1] = tmp;
            return MatrixMultiply(hMatrixL, hMatrixR);
        }

        /// <summary>
        /// a and b will be modified
        /// </summary>
        static T[,] ReduceExtendedHalfGCD<T>(T a, T b, int d) where T : IUnivariatePolynomial<T>
        {
            if (b.IsZero() || b.Degree() <= a.Degree() - d)
                return UnitMatrix(a);
            int aDegree = a.Degree();
            if (d <= SWITCH_TO_HALF_GCD_H_MATRIX_DEGREE)
                return HMatrixPlain(a, b, d, true);
            int dL = (d + 1) / 2;
            if (dL < 1)
                dL = 1;
            if (dL >= d)
                dL = d - 1;
            T[, ] hMatrixR = HMatrixHalfGCD(a, b, dL);
            T[] col = ColumnMultiply(hMatrixR, a, b);
            a.SetAndDestroy(col[0]);
            b.SetAndDestroy(col[1]);
            int dR = b.Degree() - aDegree + d;
            if (b.IsZero() || dR <= 0)
                return hMatrixR;
            T[] qd = UnivariateDivision.DivideAndRemainder(a, b, true);
            if (qd == null)
                throw new ArithmeticException("Not divisible with remainder: (" + a + ") / (" + b + ")");
            T quotient = qd[0], remainder = qd[1];
            a.SetAndDestroy(b);
            b.SetAndDestroy(remainder);
            T[, ] hMatrixL = ReduceExtendedHalfGCD(a, b, dR);
            T tmp;
            tmp = quotient.Clone().Multiply(hMatrixR[1,0]);
            tmp = hMatrixR[0,0].Clone().Subtract(tmp);
            hMatrixR[0,0] = hMatrixR[1,0];
            hMatrixR[1,0] = tmp;
            tmp = quotient.Clone().Multiply(hMatrixR[1,1]);
            tmp = hMatrixR[0,1].Clone().Subtract(tmp);
            hMatrixR[0,1] = hMatrixR[1,1];
            hMatrixR[1,1] = tmp;
            return MatrixMultiply(hMatrixL, hMatrixR);
        }

        /// <summary>
        /// a and b will be modified
        /// </summary>
        static T[] ReduceHalfGCD<T>(T a, T b) where T : IUnivariatePolynomial<T>
        {
            int d = (a.Degree() + 1) / 2;
            if (b.IsZero() || b.Degree() <= a.Degree() - d)
                return a.CreateArray(a, b);
            int aDegree = a.Degree();
            int d1 = (d + 1) / 2;
            if (d1 < 1)
                d1 = 1;
            if (d1 >= d)
                d1 = d - 1;
            T[, ] hMatrix = HMatrixHalfGCD(a, b, d1);
            T[] col = ColumnMultiply(hMatrix, a, b);
            a = col[0];
            b = col[1];
            int d2 = b.Degree() - aDegree + d;
            if (b.IsZero() || d2 <= 0)
                return a.CreateArray(a, b);
            T remainder = UnivariateDivision.Remainder(a, b, true);
            if (remainder == null)
                throw new ArithmeticException("Not divisible with remainder: (" + a + ") / (" + b + ")");
            a = b;
            b = remainder;
            return ColumnMultiply(HMatrixHalfGCD(a, b, d2), a, b);
        }

        private static T[,] MatrixMultiply<T>(T[, ] matrix1, T[, ] matrix2) where T : IUnivariatePolynomial<T>
        {
            T[, ] r = new T[2, 2];
            r[0,0] = matrix1[0,0].Clone().Multiply(matrix2[0,0]).Add(matrix1[0,1].Clone().Multiply(matrix2[1,0]));
            r[0,1] = matrix1[0,0].Clone().Multiply(matrix2[0,1]).Add(matrix1[0,1].Clone().Multiply(matrix2[1,1]));
            r[1,0] = matrix1[1,0].Clone().Multiply(matrix2[0,0]).Add(matrix1[1,1].Clone().Multiply(matrix2[1,0]));
            r[1,1] = matrix1[1,0].Clone().Multiply(matrix2[0,1]).Add(matrix1[1,1].Clone().Multiply(matrix2[1,1]));
            return r;
        }

        private static T[] ColumnMultiply<T>(T[, ] hMatrix, T row1, T row2) where T : IUnivariatePolynomial<T>
        {
            T[] resultColumn = row1.CreateArray(2);
            resultColumn[0] = hMatrix[0,0].Clone().Multiply(row1).Add(hMatrix[0,1].Clone().Multiply(row2));
            resultColumn[1] = hMatrix[1,0].Clone().Multiply(row1).Add(hMatrix[1,1].Clone().Multiply(row2));
            return resultColumn;
        }

        private static T[,] UnitMatrix<T>(T factory) where T : IUnivariatePolynomial<T>
        {
            T[, ] m = new T[2, 2];
            m[0,0] = factory.CreateOne();
            m[0,1] = factory.CreateZero();
            m[1,0] = factory.CreateZero();
            m[1,1] = factory.CreateOne();
            return m;
        }

        /// <summary>
        /// Modular GCD algorithm for polynomials over Z.
        /// </summary>
        /// <param name="a">the first polynomial</param>
        /// <param name="b">the second polynomial</param>
        /// <returns>GCD of two polynomials</returns>
        public static UnivariatePolynomialZ64 ModularGCD(UnivariatePolynomialZ64 a, UnivariatePolynomialZ64 b)
        {
            UnivariatePolynomialZ64 trivialGCD = TrivialGCD(a, b);
            if (trivialGCD != null)
                return trivialGCD;
            if (a.degree < b.degree)
                return ModularGCD(b, a);
            long aContent = a.Content(), bContent = b.Content();
            long contentGCD = MachineArithmetic.Gcd(aContent, bContent);
            if (a.IsConstant() || b.IsConstant())
                return UnivariatePolynomialZ64.Create(contentGCD);
            return ModularGCD0(a.Clone().DivideOrNull(aContent), b.Clone().DivideOrNull(bContent)).Multiply(contentGCD);
        }

        /// <summary>
        /// modular GCD for primitive polynomials
        /// </summary>
        private static UnivariatePolynomialZ64 ModularGCD0(UnivariatePolynomialZ64 a, UnivariatePolynomialZ64 b)
        {
            long lcGCD = MachineArithmetic.Gcd(a.Lc(), b.Lc());
            double bound = Math.Max(a.MignotteBound(), b.MignotteBound()) * lcGCD;
            UnivariatePolynomialZ64 previousBase = null;
            UnivariatePolynomialZp64 @base = null;
            long basePrime = -1;
            PrimesIterator primesLoop = new PrimesIterator(3);
            while (true)
            {
                long prime = primesLoop.Take();
                if (a.Lc() % prime == 0 || b.Lc() % prime == 0)
                    continue;
                UnivariatePolynomialZp64 aMod = a.Modulus(prime), bMod = b.Modulus(prime);
                UnivariatePolynomialZp64 modularGCD = HalfGCD(aMod, bMod);

                //clone if necessary
                if (modularGCD == aMod || modularGCD == bMod)
                    modularGCD = modularGCD.Clone();

                //coprime polynomials
                if (modularGCD.degree == 0)
                    return UnivariatePolynomialZ64.One();

                // save the base for the first time or when a new modular image is better
                if (@base == null || @base.degree > modularGCD.degree)
                {

                    //make base monic and multiply lcGCD
                    modularGCD.Monic(lcGCD);
                    @base = modularGCD;
                    basePrime = prime;
                    continue;
                }


                //skip unlucky prime
                if (@base.degree < modularGCD.degree)
                    continue;

                //lifting
                long newBasePrime = MachineArithmetic.SafeMultiply(basePrime, prime);
                long monicFactor = modularGCD.Multiply(MachineArithmetic.ModInverse(modularGCD.Lc(), prime), modularGCD.ring.Modulus(lcGCD));
                ChineseRemainders.ChineseRemaindersMagicZp64 magic = ChineseRemainders.CreateMagic(basePrime, prime);
                for (int i = 0; i <= @base.degree; ++i)
                {

                    //this is monic modularGCD multiplied by lcGCD mod prime
                    //long oth = mod(safeMultiply(mod(safeMultiply(modularGCD.data[i], monicFactor), prime), lcMod), prime);
                    long oth = modularGCD.Multiply(modularGCD.data[i], monicFactor);
                    @base.data[i] = ChineseRemainders.ChineseRemainders(magic, @base.data[i], oth);
                }

                @base = @base.SetModulusUnsafe(newBasePrime);
                basePrime = newBasePrime;

                //either trigger Mignotte's bound or two trials didn't change the result, probably we are done
                UnivariatePolynomialZ64 candidate = @base.AsPolyZSymmetric().PrimitivePart();
                if ((double)basePrime >= 2 * bound || (previousBase != null && candidate.Equals(previousBase)))
                {
                    previousBase = candidate;

                    //first check b since b is less degree
                    UnivariatePolynomialZ64[] div;
                    div = UnivariateDivision.DivideAndRemainder(b, candidate, true);
                    if (div == null || !div[1].IsZero())
                        continue;
                    div = UnivariateDivision.DivideAndRemainder(a, candidate, true);
                    if (div == null || !div[1].IsZero())
                        continue;
                    return candidate;
                }

                previousBase = candidate;
            }
        }

        /// <summary>
        /// Modular GCD algorithm for polynomials over Z.
        /// </summary>
        /// <param name="a">the first polynomial</param>
        /// <param name="b">the second polynomial</param>
        /// <returns>GCD of two polynomials</returns>
        public static UnivariatePolynomial<BigInteger> ModularGCD(UnivariatePolynomial<BigInteger> a, UnivariatePolynomial<BigInteger> b)
        {
            if (!a.ring.Equals(Z))
                throw new ArgumentException("Only polynomials over integers ring are allowed; " + a.ring);
            UnivariatePolynomial<BigInteger> trivialGCD = TrivialGCD(a, b);
            if (trivialGCD != null)
                return trivialGCD;
            if (a.degree < b.degree)
                return ModularGCD(b, a);
            BigInteger aContent = a.Content(), bContent = b.Content();
            BigInteger contentGCD = BigIntegerUtil.Gcd(aContent, bContent);
            if (a.IsConstant() || b.IsConstant())
                return a.CreateConstant(contentGCD);
            return ModularGCD0(a.Clone().DivideOrNull(aContent), b.Clone().DivideOrNull(bContent)).Multiply(contentGCD);
        }

        /// <summary>
        /// modular GCD for primitive polynomials
        /// </summary>
        private static UnivariatePolynomial<BigInteger> ModularGCD0(UnivariatePolynomial<BigInteger> a, UnivariatePolynomial<BigInteger> b)
        {
            BigInteger lcGCD = BigIntegerUtil.Gcd(a.Lc(), b.Lc());
            BigInteger bound2 = BigIntegerUtil.Max(UnivariatePolynomial<BigInteger>.MignotteBound(a), UnivariatePolynomial<BigInteger>.MignotteBound(b)).Multiply(lcGCD).ShiftLeft(1);
            if (bound2.IsLong() && a.MaxAbsCoefficient().IsLong() && b.MaxAbsCoefficient().IsLong())
                try
                {

                    // long overflow may occur here in very very rare cases
                    return ModularGCD(UnivariatePolynomial<BigInteger>.AsOverZ64(a), UnivariatePolynomial<BigInteger>.AsOverZ64(b)).ToBigPoly();
                }
                catch (ArithmeticException e)
                {
                }

            UnivariatePolynomialZ64 previousBase = null;
            UnivariatePolynomialZp64 @base = null;
            long basePrime = -1;
            PrimesIterator primesLoop = new PrimesIterator(1031);
            while (true)
            {
                long prime = primesLoop.Take();
                BigInteger bPrime = new BigInteger(prime);
                if (a.Lc().Remainder(bPrime).IsZero || b.Lc().Remainder(bPrime).IsZero)
                    continue;
                IntegersZp bPrimeDomain = new IntegersZp(bPrime);
                UnivariatePolynomialZp64 aMod = AsOverZp64(a.SetRing(bPrimeDomain)), bMod = AsOverZp64(b.SetRing(bPrimeDomain));
                UnivariatePolynomialZp64 modularGCD = HalfGCD(aMod, bMod);

                //clone if necessary
                if (modularGCD == aMod || modularGCD == bMod)
                    modularGCD = modularGCD.Clone();

                //coprime polynomials
                if (modularGCD.degree == 0)
                    return a.CreateOne();

                // save the base for the first time or when a new modular image is better
                if (@base == null || @base.degree > modularGCD.degree)
                {

                    //make base monic and multiply lcGCD
                    long lLcGCD = lcGCD.Mod(bPrime).LongValueExact();
                    modularGCD.Monic(lLcGCD);
                    @base = modularGCD;
                    basePrime = prime;
                    continue;
                }


                //skip unlucky prime
                if (@base.degree < modularGCD.degree)
                    continue;
                if (MachineArithmetic.IsOverflowMultiply(basePrime, prime) || basePrime * prime > MachineArithmetic.MAX_SUPPORTED_MODULUS)
                    break;

                //lifting
                long newBasePrime = basePrime * prime;
                long monicFactor = modularGCD.Multiply(MachineArithmetic.ModInverse(modularGCD.Lc(), prime), lcGCD.Mod(bPrime).LongValueExact());
                ChineseRemainders.ChineseRemaindersMagicZp64 magic = ChineseRemainders.CreateMagic(basePrime, prime);
                for (int i = 0; i <= @base.degree; ++i)
                {

                    //this is monic modularGCD multiplied by lcGCD mod prime
                    //long oth = mod(safeMultiply(mod(safeMultiply(modularGCD.data[i], monicFactor), prime), lcMod), prime);
                    long oth = modularGCD.Multiply(modularGCD.data[i], monicFactor);
                    @base.data[i] = ChineseRemainders.ChineseRemainders(magic, @base.data[i], oth);
                }

                @base = @base.SetModulusUnsafe(newBasePrime);
                basePrime = newBasePrime;

                //either trigger Mignotte's bound or two trials didn't change the result, probably we are done
                UnivariatePolynomialZ64 lCandidate = @base.AsPolyZSymmetric().PrimitivePart();
                if (new BigInteger(basePrime).CompareTo(bound2) >= 0 || (previousBase != null && lCandidate.Equals(previousBase)))
                {
                    previousBase = lCandidate;
                    UnivariatePolynomial<BigInteger> candidate = lCandidate.ToBigPoly();

                    //first check b since b is less degree
                    UnivariatePolynomial<BigInteger>[] div;
                    div = UnivariateDivision.DivideAndRemainder(b, candidate, true);
                    if (div == null || !div[1].IsZero())
                        continue;
                    div = UnivariateDivision.DivideAndRemainder(a, candidate, true);
                    if (div == null || !div[1].IsZero())
                        continue;
                    return candidate;
                }

                previousBase = lCandidate;
            }


            //continue lifting with multi-precision integers
            UnivariatePolynomial<BigInteger> bPreviousBase = null, bBase = @base.ToBigPoly();
            BigInteger bBasePrime = new BigInteger(basePrime);
            while (true)
            {
                long prime = primesLoop.Take();
                BigInteger bPrime = new BigInteger(prime);
                if (a.Lc().Remainder(bPrime).IsZero || b.Lc().Remainder(bPrime).IsZero)
                    continue;
                IntegersZp bPrimeDomain = new IntegersZp(bPrime);
                UnivariatePolynomialZp64 aMod = AsOverZp64(a.SetRing(bPrimeDomain)), bMod = AsOverZp64(b.SetRing(bPrimeDomain));
                UnivariatePolynomialZp64 modularGCD = HalfGCD(aMod, bMod);

                //clone if necessary
                if (modularGCD == aMod || modularGCD == bMod)
                    modularGCD = modularGCD.Clone();

                //coprime polynomials
                if (modularGCD.degree == 0)
                    return a.CreateOne();

                //save the base
                if (bBase == null || bBase.degree > modularGCD.degree)
                {

                    //make base monic and multiply lcGCD
                    long lLcGCD = lcGCD.Mod(bPrime).LongValueExact();
                    modularGCD.Monic(lLcGCD);
                    bBase = modularGCD.ToBigPoly();
                    bBasePrime = bPrime;
                    continue;
                }


                //skip unlucky prime
                if (bBase.degree < modularGCD.degree)
                    continue;

                //lifting
                BigInteger newBasePrime = bBasePrime.Multiply(bPrime);
                long monicFactor = modularGCD.Multiply(MachineArithmetic.ModInverse(modularGCD.Lc(), prime), lcGCD.Mod(bPrime).LongValueExact());
                ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic = CreateMagic(Z, bBasePrime, bPrime);
                for (int i = 0; i <= bBase.degree; ++i)
                {

                    //this is monic modularGCD multiplied by lcGCD mod prime
                    //long oth = mod(safeMultiply(mod(safeMultiply(modularGCD.data[i], monicFactor), prime), lcMod), prime);
                    long oth = modularGCD.Multiply(modularGCD.data[i], monicFactor);
                    bBase.data[i] = ChineseRemainders.ChineseRemainders(Z, magic, bBase.data[i], new BigInteger(oth));
                }

                bBase = bBase.SetRingUnsafe(new IntegersZp(newBasePrime));
                bBasePrime = newBasePrime;
                UnivariatePolynomial<BigInteger> candidate = UnivariatePolynomial<BigInteger>.AsPolyZSymmetric(bBase).PrimitivePart();

                //either trigger Mignotte's bound or two trials didn't change the result, probably we are done
                if (bBasePrime.CompareTo(bound2) >= 0 || (bPreviousBase != null && candidate.Equals(bPreviousBase)))
                {
                    bPreviousBase = candidate;

                    //first check b since b is less degree
                    UnivariatePolynomial<BigInteger>[] div;
                    div = UnivariateDivision.DivideAndRemainder(b, candidate, true);
                    if (div == null || !div[1].IsZero())
                        continue;
                    div = UnivariateDivision.DivideAndRemainder(a, candidate, true);
                    if (div == null || !div[1].IsZero())
                        continue;
                    return candidate;
                }

                bPreviousBase = candidate;
            }
        }

        /// <summary>
        /// Computes {@code [gcd(a,b), s, t]} such that {@code s * a + t * b = gcd(a, b)}.
        /// </summary>
        /// <param name="a">the polynomial</param>
        /// <param name="b">the polynomial</param>
        /// <returns>array of {@code [gcd(a,b), s, t]} such that {@code s * a + t * b = gcd(a, b)} (gcd is monic)</returns>
        /// <remarks>@see#ExtendedHalfGCD(IUnivariatePolynomial, IUnivariatePolynomial)</remarks>
        public static UnivariatePolynomial<Rational<BigInteger>>[] ModularExtendedRationalGCD(UnivariatePolynomial<Rational<BigInteger>> a, UnivariatePolynomial<Rational<BigInteger>> b)
        {
            if (a == b || a.Equals(b))
                return new UnivariatePolynomial<Rational<BigInteger>>[]
                {
                    a.Clone(),
                    a.CreateZero(),
                    a.CreateOne()
                };
            if (a.Degree() < b.Degree())
            {
                UnivariatePolynomial<Rational<BigInteger>>[] r = ModularExtendedRationalGCD(b, a);
                ArraysUtil.Swap(r, 1, 2);
                return r;
            }

            if (b.IsZero())
            {
                UnivariatePolynomial<Rational<BigInteger>>[] result = a.CreateArray(3);
                result[0] = a.Clone();
                result[1] = a.CreateOne();
                result[2] = a.CreateZero();
                return NormalizeExtendedGCD(result);
            }

            Tuple2<UnivariatePolynomial<BigInteger>, BigInteger> ac = ToCommonDenominator(a), bc = ToCommonDenominator(b);
            UnivariatePolynomial<BigInteger> az = ac._1, bz = bc._1;
            BigInteger aContent = az.Content(), bContent = bz.Content();
            UnivariatePolynomial<Rational<BigInteger>>[] xgcd = ModularExtendedRationalGCD0(az.Clone().DivideOrNull(aContent), bz.Clone().DivideOrNull(bContent));
            xgcd[1].Multiply(new Rational<BigInteger>(Z, ac._2, aContent));
            xgcd[2].Multiply(new Rational<BigInteger>(Z, bc._2, bContent));
            return xgcd;
        }

        /// <summary>
        /// modular extended GCD in Q[x] for primitive polynomials
        /// </summary>
        static UnivariatePolynomial<Rational<BigInteger>>[] ModularExtendedRationalGCD0(UnivariatePolynomial<BigInteger> a, UnivariatePolynomial<BigInteger> b)
        {
            BigInteger lcGCD = BigIntegerUtil.Gcd(a.Lc(), b.Lc());
            UnivariatePolynomial<Rational<BigInteger>> aRat = a.MapCoefficients(Rings.Q, (c) => new Rational(Z, c)), bRat = b.MapCoefficients(Rings.Q, (c) => new Rational(Z, c));
            int degreeMax = Math.Max(a.degree, b.degree);
            BigInteger bound2 = new BigInteger(degreeMax).Increment().Pow(degreeMax).Multiply(BigIntegerUtil.Max(a.NormMax(), b.NormMax()).Pow(a.degree + b.degree)).Multiply(lcGCD).ShiftLeft(1);
            PrimesIterator primesLoop = new PrimesIterator(1031); //SmallPrimes.nextPrime(1 << 25));
            List<BigInteger> primes = [];
            List<UnivariatePolynomial<BigInteger>>[] gst = [
                [],
                [],
                []
            ];
            BigInteger primesMul = BigInteger.One;
            main:
                while (true)
                {
                    while (primesMul.CompareTo(bound2) < 0)
                    {
                        long prime = primesLoop.Take();
                        BigInteger bPrime = new BigInteger(prime);
                        if (a.Lc().Remainder(bPrime).IsZero || b.Lc().Remainder(bPrime).IsZero)
                            continue;
                        IntegersZp bPrimeDomain = new IntegersZp(bPrime);
                        UnivariatePolynomialZp64 aMod = AsOverZp64(a.SetRing(bPrimeDomain)), bMod = AsOverZp64(b.SetRing(bPrimeDomain));
                        UnivariatePolynomialZp64[] modularXGCD = ExtendedHalfGCD(aMod, bMod);

                        //skip unlucky prime
                        if (!gst[0].IsEmpty() && modularXGCD[0].degree > gst[0][0].degree)
                            continue;
                        if (!gst[0].IsEmpty() && modularXGCD[0].degree < gst[0][0].degree)
                        {
                            primes.Clear();
                            primesMul = BigInteger.One;
                            foreach (var g in gst)
                                g.Clear();
                        }

                        long lLcGCD = lcGCD.Mod(bPrime).LongValueExact();
                        long lc = modularXGCD[0].Lc();
                        for (int i = 0; i < modularXGCD.Length; i++)
                            gst[i].Add(modularXGCD[i].Multiply(lLcGCD).Divide(lc).ToBigPoly());
                        primes.Add(bPrime);
                        primesMul = primesMul.Multiply(bPrime);
                    }


                    // CRT
                    UnivariatePolynomial<BigInteger>[] xgcdBase = new UnivariatePolynomial<BigInteger>[3];
                    BigInteger[] primesArray = primes.ToArray(new BigInteger[primes.Count]);
                    for (int i = 0; i < 3; ++i)
                    {
                        xgcdBase[i] = UnivariatePolynomial<BigInteger>.Zero(Z);
                        int deg = gst[i].Stream().MapToInt(UnivariatePolynomial.Degree()).Max().GetAsInt();
                        xgcdBase[i].EnsureCapacity(deg);
                        for (int j = 0; j <= deg; ++j)
                        {
                            int jf = j;
                            BigInteger[] cfs = gst[i].Stream().Map((p) => p[jf]).ToArray(BigInteger[].New());
                            xgcdBase[i].data[j] = ChineseRemainders.ChineseRemainders(primesArray, cfs);
                        }

                        xgcdBase[i].FixDegree();
                    }

                    while (true)
                    {

                        // do rational reconstruction
                        UnivariatePolynomial<Rational<BigInteger>>[] xgcd = ReconstructXGCD(aRat, bRat, xgcdBase, primesMul, bound2);
                        if (xgcd != null)
                            return xgcd;

                        // continue with CRT
                        long prime = primesLoop.Take();
                        BigInteger bPrime = new BigInteger(prime);
                        if (a.Lc().Remainder(bPrime).IsZero || b.Lc().Remainder(bPrime).IsZero)
                            continue;
                        IntegersZp bPrimeDomain = new IntegersZp(bPrime);
                        UnivariatePolynomialZp64 aMod = AsOverZp64(a.SetRing(bPrimeDomain)), bMod = AsOverZp64(b.SetRing(bPrimeDomain));
                        UnivariatePolynomialZp64[] modularXGCD = ExtendedHalfGCD(aMod, bMod);

                        //skip unlucky prime
                        if (modularXGCD[0].degree > xgcdBase[0].degree)
                            continue;
                        if (modularXGCD[0].degree < xgcdBase[0].degree)
                        {
                            primes.Clear();
                            foreach (var g in gst)
                             g.Clear();
                            long lLcGCD = lcGCD.Mod(bPrime).LongValueExact();
                            long lc = modularXGCD[0].Lc();
                            for (int i = 0; i < modularXGCD.Length; i++)
                                gst[i].Add(modularXGCD[i].Multiply(lLcGCD).Divide(lc).ToBigPoly());
                            primes.Add(bPrime);
                            primesMul = bPrime;
                            continue; // <- extremely rare
                        }

                        long lLcGCD = lcGCD.Mod(bPrime).LongValueExact();
                        long lc = modularXGCD[0].Lc();
                        foreach (UnivariatePolynomialZp64 m in modularXGCD)
                            m.Multiply(lLcGCD).Divide(lc);
                        ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic = CreateMagic(Z, primesMul, bPrime);
                        for (int i = 0; i < 3; i++)
                        {
                            modularXGCD[i].EnsureCapacity(xgcdBase[i].degree);
                            for (int j = 0; j <= xgcdBase[i].degree; ++j)
                                xgcdBase[i].data[j] = ChineseRemainders.ChineseRemainders(Z, magic, xgcdBase[i].data[j], new BigInteger(modularXGCD[i].data[j]));
                        }

                        primes.Add(bPrime);
                        primesMul = primesMul.Multiply(bPrime);
                    }
                }
        }

        private static UnivariatePolynomial<Rational<BigInteger>>[] ReconstructXGCD(UnivariatePolynomial<Rational<BigInteger>> aRat, UnivariatePolynomial<Rational<BigInteger>> bRat, UnivariatePolynomial<BigInteger>[] xgcdBase, BigInteger prime, BigInteger bound2)
        {
            UnivariatePolynomial<Rational<BigInteger>>[] candidate = new UnivariatePolynomial<Rational<BigInteger>>[3];
            for (int i = 0; i < 3; i++)
            {
                candidate[i] = UnivariatePolynomial<Rational<BigInteger>>.Zero(Rings.Q);
                candidate[i].EnsureCapacity(xgcdBase[i].degree);
                for (int j = 0; j <= xgcdBase[i].degree; ++j)
                {
                    BigInteger[] numDen = RationalReconstruction.Reconstruct(xgcdBase[i].data[j], prime, bound2, bound2);
                    if (numDen == null)
                        return null;
                    candidate[i].data[j] = new Rational<BigInteger>(Z, numDen[0], numDen[1]);
                }

                candidate[i].FixDegree();
            }

            BigInteger content = candidate[0].MapCoefficients(Z, r => r.Numerator()).Content();
            Rational<BigInteger> corr = new Rational<BigInteger>(Z, Z.GetOne(), content);
            UnivariatePolynomial<Rational<BigInteger>> sCandidate = candidate[1].Multiply(corr), tCandidate = candidate[2].Multiply(corr), gCandidate = candidate[0].Multiply(corr);

            //first check b since b is less degree
            UnivariatePolynomial<Rational<BigInteger>>[] bDiv;
            bDiv = UnivariateDivision.DivideAndRemainder(bRat, gCandidate, true);
            if (!bDiv[1].IsZero())
                return null;
            UnivariatePolynomial<Rational<BigInteger>>[] aDiv;
            aDiv = UnivariateDivision.DivideAndRemainder(aRat, gCandidate, true);
            if (!aDiv[1].IsZero())
                return null;
            if (!SatisfiesXGCD(aDiv[0], sCandidate, bDiv[0], tCandidate))
                return null;
            return candidate;
        }

        private static bool SatisfiesXGCD(UnivariatePolynomial<Rational<BigInteger>> a, UnivariatePolynomial<Rational<BigInteger>> s, UnivariatePolynomial<Rational<BigInteger>> b, UnivariatePolynomial<Rational<BigInteger>> t)
        {
            Rational<BigInteger> zero = Rational<BigInteger>.Zero(Z), one = Rational<BigInteger>.One(Z);
            foreach (Rational<BigInteger> subs in new Rational<BigInteger>[]
            {
                zero,
                one
            }

            )
            {
                Rational<BigInteger> ea = a.Evaluate(subs), es = s.Evaluate(subs), eb = b.Evaluate(subs), et = t.Evaluate(subs);
                if (!ea.Multiply(es).Add(eb.Multiply(et)).IsOne())
                    return false;
            }

            return a.Multiply(s).Add(b.Multiply(t)).IsOne();
        }

        /// <summary>
        /// Modular extended GCD algorithm for polynomials over Q with the use of resultants.
        /// </summary>
        /// <returns>array of {@code [gcd(a,b), s, t]} such that {@code s * a + t * b = gcd(a, b)} (gcd is monic)</returns>
        public static UnivariatePolynomial<Rational<BigInteger>>[] ModularExtendedResultantGCDInQ(UnivariatePolynomial<Rational<BigInteger>> a, UnivariatePolynomial<Rational<BigInteger>> b)
        {
            Tuple2<UnivariatePolynomial<BigInteger>, BigInteger> ra = ToCommonDenominator(a), rb = ToCommonDenominator(b);
            UnivariatePolynomial<BigInteger>[] xgcdZ = ModularExtendedResultantGCDInZ(ra._1, rb._1);
            BigInteger content = Z.Gcd(xgcdZ[0].Content(), ra._2, rb._2);
            xgcdZ[0].DivideExact(content);
            UnivariatePolynomial<Rational<BigInteger>>[] xgcd = Arrays.Stream(xgcdZ).Map((p) => p.MapCoefficients(Q, Q.MkNumerator())).ToArray(UnivariatePolynomial[].New());
            xgcd[1].Multiply(Q.MkNumerator(ra._2.DivideExact(content)));
            xgcd[2].Multiply(Q.MkNumerator(rb._2.DivideExact(content)));
            return xgcd;
        }

        /// <summary>
        /// Modular extended GCD algorithm for polynomials over Z with the use of resultants.
        /// </summary>
        /// <returns>array of {@code [gcd(a,b), s, t]} such that {@code s * a + t * b = gcd(a, b)} (gcd is monic)</returns>
        public static UnivariatePolynomial<BigInteger>[] ModularExtendedResultantGCDInZ(UnivariatePolynomial<BigInteger> a, UnivariatePolynomial<BigInteger> b)
        {
            if (a == b || a.Equals(b))
                return new UnivariatePolynomial<BigInteger>[]
                {
                    a.Clone(),
                    a.CreateZero(),
                    a.CreateOne()
                };
            if (a.Degree() < b.Degree())
            {
                UnivariatePolynomial<BigInteger>[] r = ModularExtendedResultantGCDInZ(b, a);
                ArraysUtil.Swap(r, 1, 2);
                return r;
            }

            if (b.IsZero())
            {
                UnivariatePolynomial<BigInteger>[] result = a.CreateArray(3);
                result[0] = a.Clone();
                result[1] = a.CreateOne();
                result[2] = a.CreateZero();
                return NormalizeExtendedGCD(result);
            }

            BigInteger aContent = a.Content(), bContent = b.Content();
            a = a.Clone().DivideExact(aContent);
            b = b.Clone().DivideExact(bContent);
            UnivariatePolynomial<BigInteger> gcd = PolynomialGCD(a, b);
            a = UnivariateDivision.DivideExact(a, gcd, false);
            b = UnivariateDivision.DivideExact(b, gcd, false);
            UnivariatePolynomial<BigInteger>[] xgcd = ModularExtendedResultantGCD0(a, b);
            xgcd[0].Multiply(gcd);
            UnivariatePolynomial<BigInteger> g = xgcd[0], s = xgcd[1], t = xgcd[2];
            BigInteger @as = Z.Gcd(aContent, s.Content()), bt = Z.Gcd(bContent, t.Content());
            aContent = aContent.DivideExact(@as);
            bContent = bContent.DivideExact(bt);
            s.DivideExact(@as);
            t.DivideExact(bt);
            t.Multiply(aContent);
            g.Multiply(aContent);
            s.Multiply(bContent);
            g.Multiply(bContent);
            return xgcd;
        }

        /// <summary>
        /// modular extended GCD for primitive coprime polynomials
        /// </summary>
        private static UnivariatePolynomial<BigInteger>[] ModularExtendedResultantGCD0(UnivariatePolynomial<BigInteger> a, UnivariatePolynomial<BigInteger> b)
        {
            BigInteger gcd = UnivariateResultants.ModularResultant(a, b);
            UnivariatePolynomial<BigInteger>[] previousBase = null, @base = null;
            BigInteger basePrime = null;
            PrimesIterator primesLoop = new PrimesIterator(SmallPrimes.NextPrime(1 << 28));
            while (true)
            {
                long prime = primesLoop.Take();
                BigInteger bPrime = new BigInteger(prime);
                if (a.Lc().Remainder(bPrime).IsZero || b.Lc().Remainder(bPrime).IsZero)
                    continue;
                IntegersZp ring = new IntegersZp(bPrime);
                UnivariatePolynomialZp64 aMod = AsOverZp64(a.SetRing(ring)), bMod = AsOverZp64(b.SetRing(ring));
                UnivariatePolynomialZp64[] modularXGCD = PolynomialExtendedGCD(aMod, bMod);
                if (modularXGCD[0].degree != 0)
                    continue;

                // resultant correction
                long correction = gcd.Mod(bPrime).LongValueExact();
                Arrays.Stream(modularXGCD).ForEach((p) => p.Multiply(correction));

                //save the base
                if (@base == null)
                {

                    //make base monic and multiply lcGCD
                    @base = Arrays.Stream(modularXGCD).Map(UnivariatePolynomialZp64.ToBigPoly()).ToArray(UnivariatePolynomial[].New());
                    basePrime = bPrime;
                    continue;
                }


                //CRT lifting
                ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic = CreateMagic(Z, basePrime, bPrime);
                BigInteger newBasePrime = basePrime.Multiply(bPrime);
                for (int e = 0; e < 3; ++e)
                {
                    @base[e] = @base[e].SetRingUnsafe(new IntegersZp(newBasePrime));
                    if (@base[e].degree < modularXGCD[e].degree)
                        @base[e].EnsureCapacity(modularXGCD[e].degree);
                    for (int i = 0; i <= @base[e].degree; ++i)
                        @base[e].data[i] = ChineseRemainders.ChineseRemainders(Z, magic, @base[e][i], BigInteger.ValueOf(modularXGCD[e][i]));
                    @base[e].FixDegree();
                }

                basePrime = newBasePrime;

                // compute candidate
                UnivariatePolynomial<BigInteger>[] candidate = Arrays.Stream(@base).Map(UnivariatePolynomial.AsPolyZSymmetric()).ToArray(UnivariatePolynomial[].New());
                BigInteger content = Z.Gcd(candidate[0].Content(), candidate[1].Content(), candidate[2].Content());
                Arrays.Stream(candidate).ForEach((p) => p.DivideExact(content));

                // two trials didn't change the result, probably we are done
                if ((previousBase != null && Arrays.Equals(candidate, previousBase)))
                {
                    previousBase = candidate;
                    if (!SatisfiesXGCD(a, b, candidate))
                        continue;
                    return candidate;
                }

                previousBase = candidate;
            }
        }

        private static bool SatisfiesXGCD<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b, UnivariatePolynomial<E>[] xgcd)
        {
            Ring<E> ring = xgcd[0].ring;
            foreach (E subs in ring.CreateArray(ring.GetZero(), ring.GetOne()))
            {
                E ea = a.Evaluate(subs), es = xgcd[1].Evaluate(subs), eb = b.Evaluate(subs), et = xgcd[2].Evaluate(subs), eg = xgcd[0].Evaluate(subs);
                if (!ring.AddMutable(ring.MultiplyMutable(ea, es), ring.MultiplyMutable(eb, et)).Equals(eg))
                    return false;
            }

            return a.Clone().Multiply(xgcd[1]).Add(b.Clone().Multiply(xgcd[2])).Equals(xgcd[0]);
        }

        ////////////////////////////////////// Modular GCD in algebraic number fields //////////////////////////////////////
        private static UnivariatePolynomial<UnivariatePolynomial<E>> TrivialGCDInNumberField<E>(UnivariatePolynomial<UnivariatePolynomial<E>> a, UnivariatePolynomial<UnivariatePolynomial<E>> b)
        {
            UnivariatePolynomial<UnivariatePolynomial<E>> trivialGCD = TrivialGCD(a, b);
            if (trivialGCD != null)
                return trivialGCD;
            AlgebraicNumberField<UnivariatePolynomial<E>> ring = (AlgebraicNumberField<UnivariatePolynomial<E>>)a.ring;
            if (!a.Stream().AllMatch(ring.IsInTheBaseField()) || !b.Stream().AllMatch(ring.IsInTheBaseField()))
                return null;
            UnivariatePolynomial<E> ar = a.MapCoefficients(ring.GetMinimalPolynomial().ring, UnivariatePolynomial.Cc()), br = b.MapCoefficients(ring.GetMinimalPolynomial().ring, UnivariatePolynomial.Cc());
            return PolynomialGCD(ar, br).MapCoefficients(ring, (cf) => UnivariatePolynomial.Constant(ring.GetMinimalPolynomial().ring, cf));
        }

        /// <summary>
        /// Computes GCD via Langemyr & Mccallum modular algorithm over algebraic number field
        /// </summary>
        public static UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> PolynomialGCDInNumberField(UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a, UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> b)
        {
            UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> simpleGCD = TrivialGCDInNumberField(a, b);
            if (simpleGCD != null)
                return simpleGCD;
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
                RemoveDenominators(a);
                RemoveDenominators(b);
                UnivariatePolynomial<UnivariatePolynomial<BigInteger>> gcdZ = GcdAssociateInNumberField(a.MapCoefficients(numberFieldZ, (cf) => cf.MapCoefficients(Z, Rational.Numerator())), b.MapCoefficients(numberFieldZ, (cf) => cf.MapCoefficients(Z, Rational.Numerator())));
                return gcdZ.MapCoefficients(numberField, (p) => p.MapCoefficients(Q, (cf) => new Rational(Z, cf))).Monic();
            }
            else
            {

                // replace s -> s / lc(minPoly)
                BigInteger minPolyLeadCoeff = CommonDenominator(minimalPoly);
                Rational<BigInteger> scale = new Rational(Z, Z.GetOne(), minPolyLeadCoeff), scaleReciprocal = scale.Reciprocal();
                AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> scaledNumberField = new AlgebraicNumberField(minimalPoly.Scale(scale).Monic());
                return PolynomialGCDInNumberField(a.MapCoefficients(scaledNumberField, (cf) => cf.Scale(scale)), b.MapCoefficients(scaledNumberField, (cf) => cf.Scale(scale))).MapCoefficients(numberField, (cf) => cf.Scale(scaleReciprocal));
            }
        }

        private static void PseudoMonicize(UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a)
        {
            UnivariatePolynomial<Rational<BigInteger>> inv = a.ring.Reciprocal(a.Lc());
            a.Multiply(Util.ToCommonDenominator(inv)._1.MapCoefficients(Q, Q.MkNumerator()));
        }

        static BigInteger RemoveDenominators(UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a)
        {
            BigInteger denominator = Z.Lcm(() => a.Stream().Map(Util.CommonDenominator()).Iterator());
            a.Multiply(a.ring.ValueOfBigInteger(denominator));
            return denominator;
        }

        /// <summary>
        /// Computes some GCD associate via Langemyr & Mccallum modular algorithm over algebraic integers
        /// </summary>
        public static UnivariatePolynomial<UnivariatePolynomial<BigInteger>> PolynomialGCDInRingOfIntegersOfNumberField(UnivariatePolynomial<UnivariatePolynomial<BigInteger>> a, UnivariatePolynomial<UnivariatePolynomial<BigInteger>> b)
        {
            if (!a.Lc().IsConstant() || !b.Lc().IsConstant())
                throw new ArgumentException("Univariate GCD in non-field extensions requires polynomials have integer leading coefficients.");
            UnivariatePolynomial<BigInteger> aContent = a.Content(), bContent = b.Content();
            UnivariatePolynomial<BigInteger> contentGCD = aContent.CreateConstant(aContent.Cc().Gcd(bContent.Cc()));
            a = a.Clone().DivideExact(aContent);
            b = b.Clone().DivideExact(bContent);
            return GcdAssociateInNumberField0(a, b).Multiply(contentGCD);
        }

        /// <summary>
        /// Computes some GCD associate via Langemyr & McCallum modular algorithm over algebraic integers
        /// </summary>
        static UnivariatePolynomial<UnivariatePolynomial<BigInteger>> GcdAssociateInNumberField(UnivariatePolynomial<UnivariatePolynomial<BigInteger>> a, UnivariatePolynomial<UnivariatePolynomial<BigInteger>> b)
        {
            AlgebraicNumberField<UnivariatePolynomial<BigInteger>> numberField = (AlgebraicNumberField<UnivariatePolynomial<BigInteger>>)a.ring;
            IntegerPrimitivePart(a);
            IntegerPrimitivePart(b);
            if (!a.Lc().IsConstant())
                a.Multiply(numberField.Normalizer(a.Lc()));
            if (!b.Lc().IsConstant())
                b.Multiply(numberField.Normalizer(b.Lc()));
            IntegerPrimitivePart(a);
            IntegerPrimitivePart(b);

            // if all coefficients are simple numbers (no algebraic elements)
            UnivariatePolynomial<UnivariatePolynomial<BigInteger>> simpleGCD = TrivialGCDInNumberField(a, b);
            if (simpleGCD != null)
                return simpleGCD;
            return GcdAssociateInNumberField0(a, b);
        }

        static BigInteger IntegerPrimitivePart(UnivariatePolynomial<UnivariatePolynomial<BigInteger>> p)
        {
            BigInteger gcd = Z.Gcd(p.Stream().FlatMap(UnivariatePolynomial.Stream()).Sorted().Collect(Collectors.ToList()));
            p.Stream().ForEach((cf) => cf.DivideExact(gcd));
            return gcd;
        }

        /// <summary>
        /// Langemyr & McCallum modular algorithm for primitive polynomials with integer lead coefficients
        /// </summary>
        static UnivariatePolynomial<UnivariatePolynomial<BigInteger>> GcdAssociateInNumberField0(UnivariatePolynomial<UnivariatePolynomial<BigInteger>> a, UnivariatePolynomial<UnivariatePolynomial<BigInteger>> b)
        {
            AlgebraicNumberField<UnivariatePolynomial<BigInteger>> numberField = (AlgebraicNumberField<UnivariatePolynomial<BigInteger>>)a.ring;
            UnivariateRing<UnivariatePolynomial<BigInteger>> auxRing = Rings.UnivariateRing(Z);
            UnivariatePolynomial<BigInteger> minimalPoly = numberField.GetMinimalPolynomial();

            // Weinberger & Rothschild (1976) correction denominator
            BigInteger lcGCD = Z.Gcd(a.Lc().Cc(), b.Lc().Cc()), disc = UnivariateResultants.Discriminant(minimalPoly), correctionFactor = disc.Multiply(lcGCD);
            BigInteger crtPrime = null;
            UnivariatePolynomial<UnivariatePolynomial<BigInteger>> gcd = null, prevCandidate = null;
            PrimesIterator primes = new PrimesIterator(1 << 20);
            while (true)
            {
                long prime = primes.Take();
                IntegersZp64 zpRing = new IntegersZp64(prime);
                UnivariatePolynomialZp64 minimalPolyMod = AsOverZp64(minimalPoly, zpRing);
                if (minimalPolyMod.NNonZeroTerms() != minimalPoly.NNonZeroTerms())

                    // bad prime
                    continue;
                FiniteField<UnivariatePolynomialZp64> modRing = new FiniteField<UnivariatePolynomialZp64>(minimalPolyMod);
                UnivariatePolynomial<UnivariatePolynomialZp64> aMod = a.MapCoefficients(modRing, (cf) => AsOverZp64(cf, zpRing)), bMod = b.MapCoefficients(modRing, (cf) => AsOverZp64(cf, zpRing));
                UnivariatePolynomial<UnivariatePolynomialZp64> gcdMod;
                try
                {
                    gcdMod = PolynomialGCD(aMod, bMod);
                }
                catch (Exception e)
                {

                    // bad prime
                    continue;
                }

                if (gcdMod.IsConstant())
                    return a.CreateOne();
                gcdMod.Multiply(correctionFactor.Mod(prime).LongValue());
                BigInteger bPrime = BigInteger.ValueOf(prime);
                if (crtPrime == null || gcdMod.degree < gcd.degree)
                {
                    crtPrime = bPrime;
                    gcd = gcdMod.MapCoefficients(auxRing, (cf) => cf.ToBigPoly().SetRing(Z));
                    continue;
                }

                if (gcdMod.degree > gcd.degree)

                    // bad prime
                    continue;
                ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic = CreateMagic(Z, crtPrime, bPrime);
                bool updated = false;
                for (int i = gcd.degree; i >= 0; --i)
                {
                    bool u = UpdateCRT(magic, gcd.data[i], gcdMod.data[i]);
                    if (u)
                        updated = true;
                }

                crtPrime = crtPrime.Multiply(bPrime);

                // do trial division
                IntegersZp crtRing = new IntegersZp(crtPrime);
                UnivariatePolynomial<UnivariatePolynomial<BigInteger>> candidate = gcd.MapCoefficients(numberField, (cf) => numberField.ValueOf(UnivariatePolynomial.AsPolyZSymmetric(cf.SetRingUnsafe(crtRing)))).PrimitivePart();
                if (prevCandidate == null)
                {
                    prevCandidate = candidate;
                    continue;
                }

                if (!updated || prevCandidate.Equals(candidate))
                {
                    UnivariatePolynomial<UnivariatePolynomial<BigInteger>> rem;
                    rem = UnivariateDivision.PseudoRemainderAdaptive(b, candidate, true);
                    if (rem == null || !rem.IsZero())
                        continue;
                    rem = UnivariateDivision.PseudoRemainderAdaptive(a, candidate, true);
                    if (rem == null || !rem.IsZero())
                        continue;
                    return candidate;
                }

                prevCandidate = candidate;
            }
        }

        /// <summary>
        /// Apply CRT to a poly
        /// </summary>
        public static bool UpdateCRT(ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic, UnivariatePolynomial<BigInteger> accumulated, UnivariatePolynomialZp64 update)
        {
            bool updated = false;
            accumulated.EnsureCapacity(update.degree);
            for (int i = Math.Max(accumulated.degree, update.degree); i >= 0; --i)
            {
                BigInteger oldCf = accumulated[i];
                BigInteger newCf = ChineseRemainders.ChineseRemainders(Z, magic, oldCf, new BigInteger(update[i]));
                if (!oldCf.Equals(newCf))
                    updated = true;
                accumulated.data[i] = newCf;
            }

            accumulated.FixDegree();
            return updated;
        }
    }
}