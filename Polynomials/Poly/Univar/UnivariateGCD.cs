using System.Diagnostics;
using System.Numerics;
using Polynomials.Primes;
using Polynomials.Utils;
using UnivariatePolynomialZ64 = Polynomials.Poly.Univar.UnivariatePolynomial<long>;
using UnivariatePolynomialZp64 = Polynomials.Poly.Univar.UnivariatePolynomial<long>;
using static Polynomials.Poly.Univar.Conversions64bit;

namespace Polynomials.Poly.Univar;

public static class UnivariateGCD
{
    public static UnivariatePolynomial<E> PolynomialGCD<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        a.AssertSameCoefficientRingWith(b);
        // if (Util.IsOverMultipleFieldExtension(a)) TODO
        //     return PolynomialGCDInMultipleFieldExtension(a, b);
        if (a.IsOverFiniteField())
            return HalfGCD(a, b);
        if (a.ring is Integers64)
            return ModularGCD(a.AsZ64(), b.AsZ64()).AsT<E>();
        if (a.IsOverZ())
            return ModularGCD(a.AsZ(), b.AsZ()).AsT<E>();
        // if (Util.IsOverRationals(a)) TODO
        //     return (T)PolynomialGCDInQ((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        // if (Util.IsOverRingOfIntegersOfSimpleNumberField(a)) TODO
        //     return (T)PolynomialGCDInRingOfIntegersOfNumberField((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        // if (Util.IsOverSimpleNumberField(a)) TODO
        //     return (T)PolynomialGCDInNumberField((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        if (a.IsOverField())
            return HalfGCD(a, b);
        
        var r = TryNested(a, b);
        if (r != null)
            return r;
        var t = trivialGCD(a, b);
        if (t != null)
            return t;

        throw new NotImplementedException();
        // TODO
        // return (T)UnivariateResultants.SubresultantPRS((UnivariatePolynomial)a, (UnivariatePolynomial)b).Gcd();
    }

    private static UnivariatePolynomial<E>? trivialGCD<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        if (a.IsConstant() || b.IsConstant())
        {
            if (a.IsOverField())
                return a.CreateOne();

            return a.CreateConstant(
                a.ring.Gcd(a.Content(), b.Content())
            );
        }

        return null;
    }
    
    private static UnivariatePolynomial<E>? TrivialGCD<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        if (a.IsZero())
            return NormalizeGCD(b.Clone());
        if (b.IsZero())
            return NormalizeGCD(a.Clone());
        if (a.IsOne())
            return a.Clone();
        if (b.IsOne())
            return b.Clone();
        if (Equals(a, b))
            return NormalizeGCD(a.Clone());
        return null;
    }

    private static UnivariatePolynomial<E>? TryNested<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        // if (a.ring is MultivariateRing) TODO
        //     return PolynomialGCDOverMultivariate(a, b);
        return null;
    }

    // TODO
    // private static UnivariatePolynomial<Poly> PolynomialGCDOverMultivariate<Term extends AMonomial<Term>, Poly
    //     extends AMultivariatePolynomial<Term, Poly>>(UnivariatePolynomial<Poly> a, UnivariatePolynomial<Poly> b)
    // {
    //     return MultivariateGCD.PolynomialGCD(AMultivariatePolynomial.AsMultivariate(a, 0, true),
    //         AMultivariatePolynomial.AsMultivariate(b, 0, true)).AsUnivariateEliminate(0);
    // }
    //
    // private static UnivariatePolynomial<Rational<E>> PolynomialGCDInQ<E>(UnivariatePolynomial<Rational<E>> a,
    //     UnivariatePolynomial<Rational<E>> b)
    // {
    //     Tuple2<UnivariatePolynomial<E>, E> aRat = ToCommonDenominator(a);
    //     Tuple2<UnivariatePolynomial<E>, E> bRat = ToCommonDenominator(b);
    //     return Util.AsOverRationals(a.ring, PolynomialGCD(aRat._1, bRat._1)).Monic();
    // }
    //
    // private static UnivariatePolynomial<mPoly> PolynomialGCDInMultipleFieldExtension<Term extends AMonomial<Term>, mPoly
    //     extends AMultivariatePolynomial<Term, mPoly>, sPoly extends IUnivariatePolynomial<sPoly>>(
    //     UnivariatePolynomial<mPoly> a, UnivariatePolynomial<mPoly> b)
    // {
    //     MultipleFieldExtension<Term, mPoly, sPoly> ring = (MultipleFieldExtension<Term, mPoly, sPoly>)a.ring;
    //     SimpleFieldExtension<sPoly> simpleExtension = ring.GetSimpleExtension();
    //     return PolynomialGCD(a.MapCoefficients(simpleExtension, ring.Inverse()),
    //         b.MapCoefficients(simpleExtension, ring.Inverse())).MapCoefficients(ring, ring.Image());
    // }


    public static UnivariatePolynomial<E>[] PolynomialExtendedGCD<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        // if (Util.IsOverQ(a)) TODO
        //     return ModularExtendedResultantGCDInQ(a, b);
        // if (a.IsOverZ()) TODO
        //     return ModularExtendedResultantGCDInZ(a, b);
        if (a.IsOverField())
            return ExtendedHalfGCD(a, b);
        else
            throw new ArgumentException("Polynomial over field is expected");
    }


    public static UnivariatePolynomial<E>[] PolynomialFirstBezoutCoefficient<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        if (a.IsOverFiniteField() && Math.Min(a.Degree(), b.Degree()) < 384)

            // this is somewhat faster than computing full xGCD
            return EuclidFirstBezoutCoefficient(a, b);

        var exGcd = PolynomialExtendedGCD(a, b);
        return [exGcd[0], exGcd[1]];
    }

    // TODO
    // static UnivariatePolynomial<BigInteger>[] PolynomialExtendedGCDInZbyQ(UnivariatePolynomial<BigInteger> a,
    //     UnivariatePolynomial<BigInteger> b)
    // {
    //     UnivariatePolynomial<Rational<BigInteger>>[] xgcd = PolynomialExtendedGCD(a.MapCoefficients(Q, Q.MkNumerator()),
    //         b.MapCoefficients(Q, Q.MkNumerator()));
    //     Tuple2<UnivariatePolynomial<BigInteger>, BigInteger> gcd = ToCommonDenominator(xgcd[0]),
    //         s = ToCommonDenominator(xgcd[1]),
    //         t = ToCommonDenominator(xgcd[2]);
    //     BigInteger lcm = Z.Lcm(gcd._2, s._2, t._2);
    //     return new UnivariatePolynomial<BigInteger>[]
    //     {
    //         gcd._1.Multiply(lcm.DivideExact(gcd._2)),
    //         s._1.Multiply(lcm.DivideExact(s._2)),
    //         t._1.Multiply(lcm.DivideExact(t._2))
    //     };
    // }


    public static UnivariatePolynomial<E> PolynomialGCD<E>(params UnivariatePolynomial<E>[] polynomials)
    {
        var gcd = polynomials[0];
        for (var i = 1; i < polynomials.Length; i++)
            gcd = PolynomialGCD(gcd, polynomials[i]);
        return gcd;
    }


    public static UnivariatePolynomial<E> PolynomialGCD<E>(IEnumerable<UnivariatePolynomial<E>> polynomials)
    {
        UnivariatePolynomial<E>? gcd = null;
        foreach (var poly in polynomials)
            gcd = gcd == null ? poly : PolynomialGCD(gcd, poly);
        return gcd ?? throw new Exception();
    }

    /* ========================================== implementation ==================================================== */
    


    public static UnivariatePolynomial<E> EuclidGCD<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        a.AssertSameCoefficientRingWith(b);
        var trivialGCD = TrivialGCD(a, b);
        if (trivialGCD is not null)
            return trivialGCD;
        if (CanConvertToZp64(a))
            return Convert(EuclidGCD(AsOverZp64(a), AsOverZp64(b))).AsT<E>();
        if (a.Degree() < b.Degree())
            return EuclidGCD(b, a);
        UnivariatePolynomial<E> x = a, y = b;
        while (true)
        {
            var r = UnivariateDivision.Remainder(x, y, true);
            if (r is null)
                throw new ArithmeticException("Not divisible with remainder: (" + x + ") / (" + y + ")");
            if (r.IsZero())
                break;
            x = y;
            y = r;
        }

        return NormalizeGCD(y == a ? y.Clone() : (y == b ? y.Clone() : y));
    }

    private static UnivariatePolynomial<E> NormalizeGCD<E>(UnivariatePolynomial<E> gcd)
    {
        if (gcd.IsOverField())
            return gcd.Monic();
        else
            return gcd;
    }


    public static UnivariatePolynomial<E>[] ExtendedEuclidGCD<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        a.AssertSameCoefficientRingWith(b);
        if (CanConvertToZp64(a))
            return Convert<E>(a.AsBigInteger(), ExtendedEuclidGCD(AsOverZp64(a), AsOverZp64(b)));
        UnivariatePolynomial<E> s = a.CreateZero(), old_s = a.CreateOne();
        UnivariatePolynomial<E> t = a.CreateOne(), old_t = a.CreateZero();
        UnivariatePolynomial<E> r = b.Clone(), old_r = a.Clone();
        while (!r.IsZero())
        {
            var q = UnivariateDivision.Quotient(old_r, r, true);
            if (q is null)
                throw new ArithmeticException("Not divisible with remainder: (" + old_r + ") / (" + r + ")");
            
            var tmp = old_r;
            old_r = r;
            r = tmp.Clone().Subtract(q.Clone().Multiply(r));
            
            tmp = old_s;
            old_s = s;
            s = tmp.Clone().Subtract(q.Clone().Multiply(s));
            
            tmp = old_t;
            old_t = t;
            t = tmp.Clone().Subtract(q.Clone().Multiply(t));
        }

        return NormalizeExtendedGCD([old_r, old_s, old_t]);
    }

    static UnivariatePolynomial<E>[] NormalizeExtendedGCD<E>(UnivariatePolynomial<E>[] xgcd)
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


    public static UnivariatePolynomial<E>[] EuclidFirstBezoutCoefficient<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        a.AssertSameCoefficientRingWith(b);
        if (CanConvertToZp64(a))
            return Convert<E>(a.AsBigInteger(), EuclidFirstBezoutCoefficient(AsOverZp64(a), AsOverZp64(b)));
        UnivariatePolynomial<E> s = a.CreateZero(), old_s = a.CreateOne();
        UnivariatePolynomial<E> r = b, old_r = a;
        while (!r.IsZero())
        {
            var q = UnivariateDivision.Quotient(old_r, r, true);
            if (q is null)
                throw new ArithmeticException("Not divisible with remainder: (" + old_r + ") / (" + r + ")");
            
            var tmp = old_r;
            old_r = r;
            r = tmp.Clone().Subtract(q.Clone().Multiply(r));
            
            tmp = old_s;
            old_s = s;
            s = tmp.Clone().Subtract(q.Clone().Multiply(s));
        }

        return NormalizeExtendedGCD([old_r, old_s]);
    }


    static int SWITCH_TO_HALF_GCD_ALGORITHM_DEGREE = 180;


    static int SWITCH_TO_HALF_GCD_H_MATRIX_DEGREE = 25;


    public static UnivariatePolynomial<E> HalfGCD<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        a.AssertSameCoefficientRingWith(b);
        var trivialGCD = TrivialGCD(a, b);
        if (trivialGCD is not null)
            return trivialGCD;
        if (CanConvertToZp64(a))
            return Convert<E>(HalfGCD(AsOverZp64(a), AsOverZp64(b)));
        if (a.Degree() < b.Degree())
            return HalfGCD(b, a);
        if (a.Degree() == b.Degree())
            b = UnivariateDivision.Remainder(b, a, true) ?? throw new UnreachableException();
        while (a.Degree() > SWITCH_TO_HALF_GCD_ALGORITHM_DEGREE && !b.IsZero())
        {
            UnivariatePolynomial<E>[] col = ReduceHalfGCD(a, b);
            a = col[0];
            b = col[1];
            if (!b.IsZero())
            {
                var remainder = UnivariateDivision.Remainder(a, b, true);
                if (remainder is null)
                    throw new ArithmeticException("Not divisible with remainder: (" + a + ") / (" + b + ")");
                a = b;
                b = remainder;
            }
        }

        return UnivariateGCD.EuclidGCD(a, b);
    }


    public static UnivariatePolynomial<E>[] ExtendedHalfGCD<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        a.AssertSameCoefficientRingWith(b);
        if (CanConvertToZp64(a))
            return Convert<E>(a.ToBigPoly(), ExtendedHalfGCD(AsOverZp64(a), AsOverZp64(b)));
        if (a.Degree() < b.Degree())
        {
            var r = ExtendedHalfGCD(b, a);
            (r[1], r[2]) = (r[2], r[1]);
            return r;
        }

        if (b.IsZero())
        {
            return NormalizeExtendedGCD([a.Clone(), a.CreateOne(), a.CreateZero()]);
        }

        a = a.Clone();
        b = b.Clone();
        UnivariatePolynomial<E>? quotient = null;
        if (a.Degree() == b.Degree())
        {
            var qd = UnivariateDivision.DivideAndRemainder(a, b, true);
            if (qd == null)
                throw new ArithmeticException("Not divisible with remainder: (" + a + ") / (" + b + ")");
            
            quotient = qd[0];
            var remainder = qd[1];
            a = b;
            b = remainder;
        }

        var hMatrix = ReduceExtendedHalfGCD(a, b, a.Degree() + 1);
        UnivariatePolynomial<E> gcd = a, s, t;
        if (quotient != null)
        {
            s = hMatrix[0, 1];
            t = quotient.Multiply(hMatrix[0, 1]);
            t = hMatrix[0, 0].Subtract(t);
        }
        else
        {
            s = hMatrix[0, 0];
            t = hMatrix[0, 1];
        }

        return NormalizeExtendedGCD([gcd, s, t]);
    }


    private static UnivariatePolynomial<E>[,] HMatrixPlain<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b, int degreeToReduce, bool reduce)
    {
        var hMatrix = UnitMatrix(a);
        var goal = a.Degree() - degreeToReduce;
        if (b.Degree() <= goal)
            return hMatrix;
        var tmpA = a;
        var tmpB = b;
        while (tmpB.Degree() > goal && !tmpB.IsZero())
        {
            var qd = UnivariateDivision.DivideAndRemainder(tmpA, tmpB, true);
            if (qd == null)
                throw new ArithmeticException("Not divisible with remainder: (" + tmpA + ") / (" + tmpB + ")");
            var quotient = qd[0];
            var remainder = qd[1];
            var tmp = quotient.Clone().Multiply(hMatrix[1, 0]);
            tmp = hMatrix[0, 0].Clone().Subtract(tmp);
            hMatrix[0, 0] = hMatrix[1, 0];
            hMatrix[1, 0] = tmp;
            tmp = quotient.Clone().Multiply(hMatrix[1, 1]);
            tmp = hMatrix[0, 1].Clone().Subtract(tmp);
            hMatrix[0, 1] = hMatrix[1, 1];
            hMatrix[1, 1] = tmp;
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

    private static UnivariatePolynomial<E>[,] HMatrixHalfGCD<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b, int d)
    {
        if (b.IsZero() || b.Degree() <= a.Degree() - d)
            return UnitMatrix(a);
        var n = a.Degree() - 2 * d + 2;
        if (n < 0)
            n = 0;
        var a1 = a.Clone().ShiftLeft(n);
        var b1 = b.Clone().ShiftLeft(n);
        if (d <= SWITCH_TO_HALF_GCD_H_MATRIX_DEGREE)
            return HMatrixPlain(a1, b1, d, false);
        var dR = (d + 1) / 2;
        if (dR < 1)
            dR = 1;
        if (dR >= d)
            dR = d - 1;
        var hMatrixR = HMatrixHalfGCD(a1, b1, dR);
        var col = ColumnMultiply(hMatrixR, a1, b1);
        a1 = col[0];
        b1 = col[1];
        var dL = b1.Degree() - a.Degree() + n + d;
        if (b1.IsZero() || dL <= 0)
            return hMatrixR;
        var qd = UnivariateDivision.DivideAndRemainder(a1, b1, false);
        if (qd == null)
            throw new ArithmeticException("Not divisible with remainder: (" + a1 + ") / (" + b1 + ")");
        var quotient = qd[0];
        var remainder = qd[1];
        var hMatrixL = HMatrixHalfGCD(b1, remainder, dL);
        var tmp = quotient.Clone().Multiply(hMatrixR[1, 0]);
        tmp = hMatrixR[0, 0].Clone().Subtract(tmp);
        hMatrixR[0, 0] = hMatrixR[1, 0];
        hMatrixR[1, 0] = tmp;
        tmp = quotient.Clone().Multiply(hMatrixR[1, 1]);
        tmp = hMatrixR[0, 1].Clone().Subtract(tmp);
        hMatrixR[0, 1] = hMatrixR[1, 1];
        hMatrixR[1, 1] = tmp;
        return MatrixMultiply(hMatrixL, hMatrixR);
    }


    static UnivariatePolynomial<E>[,] ReduceExtendedHalfGCD<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b, int d)
    {
        if (b.IsZero() || b.Degree() <= a.Degree() - d)
            return UnitMatrix(a);
        var aDegree = a.Degree();
        if (d <= SWITCH_TO_HALF_GCD_H_MATRIX_DEGREE)
            return HMatrixPlain(a, b, d, true);
        var dL = (d + 1) / 2;
        if (dL < 1)
            dL = 1;
        if (dL >= d)
            dL = d - 1;
        var hMatrixR = HMatrixHalfGCD(a, b, dL);
        var col = ColumnMultiply(hMatrixR, a, b);
        a.SetAndDestroy(col[0]);
        b.SetAndDestroy(col[1]);
        var dR = b.Degree() - aDegree + d;
        if (b.IsZero() || dR <= 0)
            return hMatrixR;
        var qd = UnivariateDivision.DivideAndRemainder(a, b, true);
        if (qd == null)
            throw new ArithmeticException("Not divisible with remainder: (" + a + ") / (" + b + ")");
        var quotient = qd[0];
        var remainder = qd[1];
        a.SetAndDestroy(b);
        b.SetAndDestroy(remainder);
        var hMatrixL = ReduceExtendedHalfGCD(a, b, dR);
        var tmp = quotient.Clone().Multiply(hMatrixR[1, 0]);
        tmp = hMatrixR[0, 0].Clone().Subtract(tmp);
        hMatrixR[0, 0] = hMatrixR[1, 0];
        hMatrixR[1, 0] = tmp;
        tmp = quotient.Clone().Multiply(hMatrixR[1, 1]);
        tmp = hMatrixR[0, 1].Clone().Subtract(tmp);
        hMatrixR[0, 1] = hMatrixR[1, 1];
        hMatrixR[1, 1] = tmp;
        return MatrixMultiply(hMatrixL, hMatrixR);
    }


    static UnivariatePolynomial<E>[] ReduceHalfGCD<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        var d = (a.Degree() + 1) / 2;
        if (b.IsZero() || b.Degree() <= a.Degree() - d)
            return [a, b];
        var aDegree = a.Degree();
        var d1 = (d + 1) / 2;
        if (d1 < 1)
            d1 = 1;
        if (d1 >= d)
            d1 = d - 1;
        var hMatrix = HMatrixHalfGCD(a, b, d1);
        var col = ColumnMultiply(hMatrix, a, b);
        a = col[0];
        b = col[1];
        var d2 = b.Degree() - aDegree + d;
        if (b.IsZero() || d2 <= 0)
            return [a, b];
        var remainder = UnivariateDivision.Remainder(a, b, true);
        if (remainder == null)
            throw new ArithmeticException("Not divisible with remainder: (" + a + ") / (" + b + ")");
        a = b;
        b = remainder;
        return ColumnMultiply(HMatrixHalfGCD(a, b, d2), a, b);
    }

    private static UnivariatePolynomial<E>[,] MatrixMultiply<E>(UnivariatePolynomial<E>[,] matrix1, UnivariatePolynomial<E>[,] matrix2)
    {
        var r = new UnivariatePolynomial<E>[2, 2];
        r[0, 0] = matrix1[0, 0].Clone().Multiply(matrix2[0, 0]).Add(matrix1[0, 1].Clone().Multiply(matrix2[1, 0]));
        r[0, 1] = matrix1[0, 0].Clone().Multiply(matrix2[0, 1]).Add(matrix1[0, 1].Clone().Multiply(matrix2[1, 1]));
        r[1, 0] = matrix1[1, 0].Clone().Multiply(matrix2[0, 0]).Add(matrix1[1, 1].Clone().Multiply(matrix2[1, 0]));
        r[1, 1] = matrix1[1, 0].Clone().Multiply(matrix2[0, 1]).Add(matrix1[1, 1].Clone().Multiply(matrix2[1, 1]));
        return r;
    }

    private static UnivariatePolynomial<E>[] ColumnMultiply<E>(UnivariatePolynomial<E>[,] hMatrix, UnivariatePolynomial<E> row1, UnivariatePolynomial<E> row2)
    {
        var resultColumn = new UnivariatePolynomial<E>[2];
        resultColumn[0] = hMatrix[0, 0].Clone().Multiply(row1).Add(hMatrix[0, 1].Clone().Multiply(row2));
        resultColumn[1] = hMatrix[1, 0].Clone().Multiply(row1).Add(hMatrix[1, 1].Clone().Multiply(row2));
        return resultColumn;
    }

    private static UnivariatePolynomial<E>[,] UnitMatrix<E>(UnivariatePolynomial<E> factory)
    {
        var m = new UnivariatePolynomial<E>[2, 2];
        m[0, 0] = factory.CreateOne();
        m[0, 1] = factory.CreateZero();
        m[1, 0] = factory.CreateZero();
        m[1, 1] = factory.CreateOne();
        return m;
    }


    public static UnivariatePolynomialZ64 ModularGCD(UnivariatePolynomialZ64 a, UnivariatePolynomialZ64 b)
    {
        if (!a.ring.Equals(Rings.Z64))
            throw new ArgumentException("Only polynomials over integers ring are allowed; " + a.ring);
        
        var trivialGCD = TrivialGCD(a, b);
        if (trivialGCD != null)
            return (UnivariatePolynomialZ64)trivialGCD;
        if (a.degree < b.degree)
            return ModularGCD(b, a);
        long aContent = a.Content(), bContent = b.Content();
        var contentGCD = MachineArithmetic.Gcd(aContent, bContent);
        if (a.IsConstant() || b.IsConstant())
            return a.CreateConstant(contentGCD);
        a = a.Clone().DivideOrNull(aContent) ?? throw new UnreachableException();
        b = b.Clone().DivideOrNull(bContent) ?? throw new UnreachableException();
        return ModularGCD0(a, b).Multiply(contentGCD);
    }


    private static UnivariatePolynomialZ64 ModularGCD0(UnivariatePolynomialZ64 a, UnivariatePolynomialZ64 b)
    {
        var lcGCD = MachineArithmetic.Gcd(a.Lc(), b.Lc());
        double bound = Math.Max(UnivariatePolynomialZ64.MignotteBound(a), UnivariatePolynomialZ64.MignotteBound(b)) * lcGCD;
        UnivariatePolynomialZ64? previousBase = null;
        UnivariatePolynomialZp64? @base = null;
        long basePrime = -1;
        var primesLoop = new PrimesIterator(3);
        while (true)
        {
            var prime = primesLoop.Take();
            if (a.Lc() % prime == 0 || b.Lc() % prime == 0)
                continue;
            UnivariatePolynomialZp64 aMod = a.Modulus(prime), bMod = b.Modulus(prime);
            var modularGCD = HalfGCD(aMod, bMod);

            //clone if necessary
            if (modularGCD == aMod || modularGCD == bMod)
                modularGCD = modularGCD.Clone();

            //coprime polynomials
            if (modularGCD.degree == 0)
                return UnivariatePolynomialZ64.One(Rings.Z64);

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
            var newBasePrime = MachineArithmetic.SafeMultiply(basePrime, prime);
            var monicFactor = modularGCD.ring.Multiply(MachineArithmetic.ModInverse(modularGCD.Lc(), prime),
                ((IntegersZp64) modularGCD.ring).Modulus(lcGCD));
            var magic = ChineseRemainders.CreateMagic(basePrime, prime);
            for (var i = 0; i <= @base.degree; ++i)
            {
                //this is monic modularGCD multiplied by lcGCD mod prime
                //long oth = mod(safeMultiply(mod(safeMultiply(modularGCD.data[i], monicFactor), prime), lcMod), prime);
                var oth = modularGCD.ring.Multiply(modularGCD.data[i], monicFactor);
                @base.data[i] = ChineseRemainders.ChineseRemainder(magic, @base.data[i], oth);
            }

            @base = @base.SetModulusUnsafe(newBasePrime);
            basePrime = newBasePrime;

            //either trigger Mignotte's bound or two trials didn't change the result, probably we are done
            var candidate = UnivariatePolynomial<long>.AsPolyZ64Symmetric(@base).PrimitivePart() ?? throw new Exception();
            if ((double)basePrime >= 2 * bound || (previousBase != null && candidate.Equals(previousBase)))
            {
                previousBase = candidate;

                //first check b since b is less degree
                var div = UnivariateDivision.DivideAndRemainder(b, candidate, true);
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


    public static UnivariatePolynomial<BigInteger> ModularGCD(UnivariatePolynomial<BigInteger> a,
        UnivariatePolynomial<BigInteger> b)
    {
        if (!a.ring.Equals(Rings.Z))
            throw new ArgumentException("Only polynomials over integers ring are allowed; " + a.ring);
        var trivialGCD = TrivialGCD(a, b);
        if (trivialGCD != null)
            return trivialGCD;
        if (a.degree < b.degree)
            return ModularGCD(b, a);
        BigInteger aContent = a.Content(), bContent = b.Content();
        var contentGCD = BigInteger.GreatestCommonDivisor(aContent, bContent);
        if (a.IsConstant() || b.IsConstant())
            return a.CreateConstant(contentGCD);
        a = a.Clone().DivideOrNull(aContent) ?? throw new UnreachableException();
        b = b.Clone().DivideOrNull(bContent) ?? throw new UnreachableException();
        return ModularGCD0(a, b).Multiply(contentGCD);
    }


    private static UnivariatePolynomial<BigInteger> ModularGCD0(UnivariatePolynomial<BigInteger> a,
        UnivariatePolynomial<BigInteger> b)
    {
        var lcGCD = BigInteger.GreatestCommonDivisor(a.Lc(), b.Lc());
        var bound2 = (BigInteger
                          .Max(UnivariatePolynomial<BigInteger>.MignotteBound(a), UnivariatePolynomial<BigInteger>.MignotteBound(b))
                      * lcGCD) << 1;
        if (bound2.IsLong() && a.MaxAbsCoefficient().IsLong() && b.MaxAbsCoefficient().IsLong())
            try
            {
                // long overflow may occur here in very very rare cases
                return ModularGCD(UnivariatePolynomial<BigInteger>.AsOverZ64(a),
                    UnivariatePolynomial<BigInteger>.AsOverZ64(b)).ToBigPoly();
            }
            catch (ArithmeticException e)
            {
            }

        UnivariatePolynomialZ64? previousBase = null;
        UnivariatePolynomialZp64? @base = null;
        long basePrime = -1;
        var primesLoop = new PrimesIterator(1031);
        while (true)
        {
            var prime = primesLoop.Take();
            var bPrime = new BigInteger(prime);
            if ((a.Lc() % bPrime).IsZero || (b.Lc() % bPrime).IsZero)
                continue;
            var bPrimeDomain = new IntegersZp(bPrime);
            UnivariatePolynomialZp64 aMod = AsOverZp64(a.SetRing(bPrimeDomain)),
                bMod = AsOverZp64(b.SetRing(bPrimeDomain));
            var modularGCD = HalfGCD(aMod, bMod);

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
                var lLcGCD = (long)(lcGCD % bPrime);
                modularGCD.Monic(lLcGCD);
                @base = (UnivariatePolynomialZp64?)modularGCD;
                basePrime = prime;
                continue;
            }


            //skip unlucky prime
            if (@base.degree < modularGCD.degree)
                continue;
            if (MachineArithmetic.IsOverflowMultiply(basePrime, prime) ||
                basePrime * prime > MachineArithmetic.MAX_SUPPORTED_MODULUS)
                break;

            //lifting
            var newBasePrime = basePrime * prime;
            var monicFactor = modularGCD.ring.Multiply(MachineArithmetic.ModInverse(modularGCD.Lc(), prime),
                (long)(lcGCD % bPrime));
            var magic = ChineseRemainders.CreateMagic(basePrime, prime);
            for (var i = 0; i <= @base.degree; ++i)
            {
                //this is monic modularGCD multiplied by lcGCD mod prime
                //long oth = mod(safeMultiply(mod(safeMultiply(modularGCD.data[i], monicFactor), prime), lcMod), prime);
                var oth = modularGCD.ring.Multiply(modularGCD.data[i], monicFactor);
                @base.data[i] = ChineseRemainders.ChineseRemainder(magic, @base.data[i], oth);
            }

            @base = @base.SetModulusUnsafe(newBasePrime);
            basePrime = newBasePrime;

            //either trigger Mignotte's bound or two trials didn't change the result, probably we are done
            var lCandidate = UnivariatePolynomial<long>.AsPolyZ64Symmetric(@base).PrimitivePart() ?? throw new Exception();
            if (new BigInteger(basePrime).CompareTo(bound2) >= 0 ||
                (previousBase != null && Equals(lCandidate, previousBase)))
            {
                previousBase = lCandidate;
                var candidate = lCandidate.ToBigPoly();

                //first check b since b is less degree
                var div = UnivariateDivision.DivideAndRemainder(b, candidate, true);
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
        UnivariatePolynomial<BigInteger>? bPreviousBase = null;
        var bBase = @base.ToBigPoly();
        var bBasePrime = new BigInteger(basePrime);
        while (true)
        {
            var prime = primesLoop.Take();
            var bPrime = new BigInteger(prime);
            if ((a.Lc() % bPrime).IsZero || (b.Lc() % bPrime).IsZero)
                continue;
            var bPrimeDomain = new IntegersZp(bPrime);
            UnivariatePolynomialZp64 aMod = AsOverZp64(a.SetRing(bPrimeDomain)),
                bMod = AsOverZp64(b.SetRing(bPrimeDomain));
            var modularGCD = HalfGCD(aMod, bMod);

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
                var lLcGCD = (long)(lcGCD % bPrime);
                modularGCD.Monic(lLcGCD);
                bBase = modularGCD.ToBigPoly();
                bBasePrime = bPrime;
                continue;
            }


            //skip unlucky prime
            if (bBase.degree < modularGCD.degree)
                continue;

            //lifting
            var newBasePrime = bBasePrime * bPrime;
            var monicFactor = modularGCD.ring.Multiply(MachineArithmetic.ModInverse(modularGCD.Lc(), prime),
                (long)(lcGCD % bPrime));
            var magic = ChineseRemainders.CreateMagic(Rings.Z, bBasePrime, bPrime);
            for (var i = 0; i <= bBase.degree; ++i)
            {
                //this is monic modularGCD multiplied by lcGCD mod prime
                //long oth = mod(safeMultiply(mod(safeMultiply(modularGCD.data[i], monicFactor), prime), lcMod), prime);
                var oth = modularGCD.ring.Multiply(modularGCD.data[i], monicFactor);
                bBase.data[i] = ChineseRemainders.ChineseRemainder(Rings.Z, magic, bBase.data[i], new BigInteger(oth));
            }

            bBase = bBase.SetRingUnsafe(new IntegersZp(newBasePrime));
            bBasePrime = newBasePrime;
            var candidate =
                UnivariatePolynomial<BigInteger>.AsPolyZSymmetric(bBase).PrimitivePart() ?? throw new UnreachableException();

            //either trigger Mignotte's bound or two trials didn't change the result, probably we are done
            if (bBasePrime.CompareTo(bound2) >= 0 || (bPreviousBase != null && candidate.Equals(bPreviousBase)))
            {
                bPreviousBase = candidate;

                //first check b since b is less degree
                var div = UnivariateDivision.DivideAndRemainder(b, candidate, true);
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

    // TODO
    // public static UnivariatePolynomial<Rational<BigInteger>>[] ModularExtendedRationalGCD(
    //     UnivariatePolynomial<Rational<BigInteger>> a, UnivariatePolynomial<Rational<BigInteger>> b)
    // {
    //     if (a == b || a.Equals(b))
    //         return new UnivariatePolynomial<Rational<BigInteger>>[]
    //         {
    //             a.Clone(),
    //             a.CreateZero(),
    //             a.CreateOne()
    //         };
    //     if (a.Degree() < b.Degree())
    //     {
    //         UnivariatePolynomial<Rational<BigInteger>>[] r = ModularExtendedRationalGCD(b, a);
    //         ArraysUtil.Swap(r, 1, 2);
    //         return r;
    //     }
    //
    //     if (b.IsZero())
    //     {
    //         UnivariatePolynomial<Rational<BigInteger>>[] result = a.CreateArray(3);
    //         result[0] = a.Clone();
    //         result[1] = a.CreateOne();
    //         result[2] = a.CreateZero();
    //         return NormalizeExtendedGCD(result);
    //     }
    //
    //     Tuple2<UnivariatePolynomial<BigInteger>, BigInteger> ac = ToCommonDenominator(a), bc = ToCommonDenominator(b);
    //     UnivariatePolynomial<BigInteger> az = ac._1, bz = bc._1;
    //     BigInteger aContent = az.Content(), bContent = bz.Content();
    //     UnivariatePolynomial<Rational<BigInteger>>[] xgcd =
    //         ModularExtendedRationalGCD0(az.Clone().DivideOrNull(aContent), bz.Clone().DivideOrNull(bContent));
    //     xgcd[1].Multiply(new Rational<BigInteger>(Z, ac._2, aContent));
    //     xgcd[2].Multiply(new Rational<BigInteger>(Z, bc._2, bContent));
    //     return xgcd;
    // }

    // TODO
    // static UnivariatePolynomial<Rational<BigInteger>>[] ModularExtendedRationalGCD0(UnivariatePolynomial<BigInteger> a,
    //     UnivariatePolynomial<BigInteger> b)
    // {
    //     BigInteger lcGCD = BigIntegerUtil.Gcd(a.Lc(), b.Lc());
    //     UnivariatePolynomial<Rational<BigInteger>> aRat = a.MapCoefficients(Rings.Q, (c) => new Rational(Z, c)),
    //         bRat = b.MapCoefficients(Rings.Q, (c) => new Rational(Z, c));
    //     int degreeMax = Math.Max(a.degree, b.degree);
    //     BigInteger bound2 = new BigInteger(degreeMax).Increment().Pow(degreeMax)
    //         .Multiply(BigIntegerUtil.Max(a.NormMax(), b.NormMax()).Pow(a.degree + b.degree)).Multiply(lcGCD)
    //         .ShiftLeft(1);
    //     PrimesIterator primesLoop = new PrimesIterator(1031); //SmallPrimes.nextPrime(1 << 25));
    //     List<BigInteger> primes = [];
    //     List<UnivariatePolynomial<BigInteger>>[] gst =
    //     [
    //         [],
    //         [],
    //         []
    //     ];
    //     BigInteger primesMul = BigInteger.One;
    //     main:
    //     while (true)
    //     {
    //         while (primesMul.CompareTo(bound2) < 0)
    //         {
    //             long prime = primesLoop.Take();
    //             BigInteger bPrime = new BigInteger(prime);
    //             if (a.Lc().Remainder(bPrime).IsZero || b.Lc().Remainder(bPrime).IsZero)
    //                 continue;
    //             IntegersZp bPrimeDomain = new IntegersZp(bPrime);
    //             UnivariatePolynomialZp64 aMod = AsOverZp64(a.SetRing(bPrimeDomain)),
    //                 bMod = AsOverZp64(b.SetRing(bPrimeDomain));
    //             UnivariatePolynomialZp64[] modularXGCD = ExtendedHalfGCD(aMod, bMod);
    //
    //             //skip unlucky prime
    //             if (!gst[0].IsEmpty() && modularXGCD[0].degree > gst[0][0].degree)
    //                 continue;
    //             if (!gst[0].IsEmpty() && modularXGCD[0].degree < gst[0][0].degree)
    //             {
    //                 primes.Clear();
    //                 primesMul = BigInteger.One;
    //                 foreach (var g in gst)
    //                     g.Clear();
    //             }
    //
    //             long lLcGCD = lcGCD.Mod(bPrime).LongValueExact();
    //             long lc = modularXGCD[0].Lc();
    //             for (int i = 0; i < modularXGCD.Length; i++)
    //                 gst[i].Add(modularXGCD[i].Multiply(lLcGCD).Divide(lc).ToBigPoly());
    //             primes.Add(bPrime);
    //             primesMul = primesMul.Multiply(bPrime);
    //         }
    //
    //
    //         // CRT
    //         UnivariatePolynomial<BigInteger>[] xgcdBase = new UnivariatePolynomial<BigInteger>[3];
    //         BigInteger[] primesArray = primes.ToArray(new BigInteger[primes.Count]);
    //         for (int i = 0; i < 3; ++i)
    //         {
    //             xgcdBase[i] = UnivariatePolynomial<BigInteger>.Zero(Z);
    //             int deg = gst[i].Stream().MapToInt(UnivariatePolynomial.Degree()).Max().GetAsInt();
    //             xgcdBase[i].EnsureCapacity(deg);
    //             for (int j = 0; j <= deg; ++j)
    //             {
    //                 int jf = j;
    //                 BigInteger[] cfs = gst[i].Stream().Map((p) => p[jf]).ToArray(BigInteger[].New());
    //                 xgcdBase[i].data[j] = ChineseRemainders.ChineseRemainders(primesArray, cfs);
    //             }
    //
    //             xgcdBase[i].FixDegree();
    //         }
    //
    //         while (true)
    //         {
    //             // do rational reconstruction
    //             UnivariatePolynomial<Rational<BigInteger>>[] xgcd = ReconstructXGCD(aRat, bRat, xgcdBase, primesMul,
    //                 bound2);
    //             if (xgcd != null)
    //                 return xgcd;
    //
    //             // continue with CRT
    //             long prime = primesLoop.Take();
    //             BigInteger bPrime = new BigInteger(prime);
    //             if (a.Lc().Remainder(bPrime).IsZero || b.Lc().Remainder(bPrime).IsZero)
    //                 continue;
    //             IntegersZp bPrimeDomain = new IntegersZp(bPrime);
    //             UnivariatePolynomialZp64 aMod = AsOverZp64(a.SetRing(bPrimeDomain)),
    //                 bMod = AsOverZp64(b.SetRing(bPrimeDomain));
    //             UnivariatePolynomialZp64[] modularXGCD = ExtendedHalfGCD(aMod, bMod);
    //
    //             //skip unlucky prime
    //             if (modularXGCD[0].degree > xgcdBase[0].degree)
    //                 continue;
    //             if (modularXGCD[0].degree < xgcdBase[0].degree)
    //             {
    //                 primes.Clear();
    //                 foreach (var g in gst)
    //                     g.Clear();
    //                 long lLcGCD = lcGCD.Mod(bPrime).LongValueExact();
    //                 long lc = modularXGCD[0].Lc();
    //                 for (int i = 0; i < modularXGCD.Length; i++)
    //                     gst[i].Add(modularXGCD[i].Multiply(lLcGCD).Divide(lc).ToBigPoly());
    //                 primes.Add(bPrime);
    //                 primesMul = bPrime;
    //                 continue; // <- extremely rare
    //             }
    //
    //             long lLcGCD = lcGCD.Mod(bPrime).LongValueExact();
    //             long lc = modularXGCD[0].Lc();
    //             foreach (UnivariatePolynomialZp64 m in modularXGCD)
    //                 m.Multiply(lLcGCD).Divide(lc);
    //             ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic = CreateMagic(Z, primesMul, bPrime);
    //             for (int i = 0; i < 3; i++)
    //             {
    //                 modularXGCD[i].EnsureCapacity(xgcdBase[i].degree);
    //                 for (int j = 0; j <= xgcdBase[i].degree; ++j)
    //                     xgcdBase[i].data[j] = ChineseRemainders.ChineseRemainders(Z, magic, xgcdBase[i].data[j],
    //                         new BigInteger(modularXGCD[i].data[j]));
    //             }
    //
    //             primes.Add(bPrime);
    //             primesMul = primesMul.Multiply(bPrime);
    //         }
    //     }
    // }

    // TODO
    // private static UnivariatePolynomial<Rational<BigInteger>>[] ReconstructXGCD(
    //     UnivariatePolynomial<Rational<BigInteger>> aRat, UnivariatePolynomial<Rational<BigInteger>> bRat,
    //     UnivariatePolynomial<BigInteger>[] xgcdBase, BigInteger prime, BigInteger bound2)
    // {
    //     UnivariatePolynomial<Rational<BigInteger>>[] candidate = new UnivariatePolynomial<Rational<BigInteger>>[3];
    //     for (int i = 0; i < 3; i++)
    //     {
    //         candidate[i] = UnivariatePolynomial<Rational<BigInteger>>.Zero(Rings.Q);
    //         candidate[i].EnsureCapacity(xgcdBase[i].degree);
    //         for (int j = 0; j <= xgcdBase[i].degree; ++j)
    //         {
    //             BigInteger[] numDen = RationalReconstruction.Reconstruct(xgcdBase[i].data[j], prime, bound2, bound2);
    //             if (numDen == null)
    //                 return null;
    //             candidate[i].data[j] = new Rational<BigInteger>(Z, numDen[0], numDen[1]);
    //         }
    //
    //         candidate[i].FixDegree();
    //     }
    //
    //     BigInteger content = candidate[0].MapCoefficients(Z, r => r.Numerator()).Content();
    //     Rational<BigInteger> corr = new Rational<BigInteger>(Z, Z.GetOne(), content);
    //     UnivariatePolynomial<Rational<BigInteger>> sCandidate = candidate[1].Multiply(corr),
    //         tCandidate = candidate[2].Multiply(corr),
    //         gCandidate = candidate[0].Multiply(corr);
    //
    //     //first check b since b is less degree
    //     UnivariatePolynomial<Rational<BigInteger>>[] bDiv;
    //     bDiv = UnivariateDivision.DivideAndRemainder(bRat, gCandidate, true);
    //     if (!bDiv[1].IsZero())
    //         return null;
    //     UnivariatePolynomial<Rational<BigInteger>>[] aDiv;
    //     aDiv = UnivariateDivision.DivideAndRemainder(aRat, gCandidate, true);
    //     if (!aDiv[1].IsZero())
    //         return null;
    //     if (!SatisfiesXGCD(aDiv[0], sCandidate, bDiv[0], tCandidate))
    //         return null;
    //     return candidate;
    // }

    // TODO
    // private static bool SatisfiesXGCD(UnivariatePolynomial<Rational<BigInteger>> a,
    //     UnivariatePolynomial<Rational<BigInteger>> s, UnivariatePolynomial<Rational<BigInteger>> b,
    //     UnivariatePolynomial<Rational<BigInteger>> t)
    // {
    //     Rational<BigInteger> zero = Rational<BigInteger>.Zero(Z), one = Rational<BigInteger>.One(Z);
    //     foreach (Rational<BigInteger> subs in new Rational<BigInteger>[]
    //              {
    //                  zero,
    //                  one
    //              }
    //             )
    //     {
    //         Rational<BigInteger> ea = a.Evaluate(subs),
    //             es = s.Evaluate(subs),
    //             eb = b.Evaluate(subs),
    //             et = t.Evaluate(subs);
    //         if (!ea.Multiply(es).Add(eb.Multiply(et)).IsOne())
    //             return false;
    //     }
    //
    //     return a.Multiply(s).Add(b.Multiply(t)).IsOne();
    // }

    // TODO
    // public static UnivariatePolynomial<Rational<BigInteger>>[] ModularExtendedResultantGCDInQ(
    //     UnivariatePolynomial<Rational<BigInteger>> a, UnivariatePolynomial<Rational<BigInteger>> b)
    // {
    //     Tuple2<UnivariatePolynomial<BigInteger>, BigInteger> ra = ToCommonDenominator(a), rb = ToCommonDenominator(b);
    //     UnivariatePolynomial<BigInteger>[] xgcdZ = ModularExtendedResultantGCDInZ(ra._1, rb._1);
    //     BigInteger content = Z.Gcd(xgcdZ[0].Content(), ra._2, rb._2);
    //     xgcdZ[0].DivideExact(content);
    //     UnivariatePolynomial<Rational<BigInteger>>[] xgcd = Arrays.Stream(xgcdZ)
    //         .Map((p) => p.MapCoefficients(Q, Q.MkNumerator())).ToArray(UnivariatePolynomial[].New());
    //     xgcd[1].Multiply(Q.MkNumerator(ra._2.DivideExact(content)));
    //     xgcd[2].Multiply(Q.MkNumerator(rb._2.DivideExact(content)));
    //     return xgcd;
    // }

    // TODO
    // public static UnivariatePolynomial<BigInteger>[] ModularExtendedResultantGCDInZ(UnivariatePolynomial<BigInteger> a,
    //     UnivariatePolynomial<BigInteger> b)
    // {
    //     if (a == b || a.Equals(b))
    //         return new UnivariatePolynomial<BigInteger>[]
    //         {
    //             a.Clone(),
    //             a.CreateZero(),
    //             a.CreateOne()
    //         };
    //     if (a.Degree() < b.Degree())
    //     {
    //         UnivariatePolynomial<BigInteger>[] r = ModularExtendedResultantGCDInZ(b, a);
    //         ArraysUtil.Swap(r, 1, 2);
    //         return r;
    //     }
    //
    //     if (b.IsZero())
    //     {
    //         UnivariatePolynomial<BigInteger>[] result = a.CreateArray(3);
    //         result[0] = a.Clone();
    //         result[1] = a.CreateOne();
    //         result[2] = a.CreateZero();
    //         return NormalizeExtendedGCD(result);
    //     }
    //
    //     BigInteger aContent = a.Content(), bContent = b.Content();
    //     a = a.Clone().DivideExact(aContent);
    //     b = b.Clone().DivideExact(bContent);
    //     UnivariatePolynomial<BigInteger> gcd = PolynomialGCD(a, b);
    //     a = UnivariateDivision.DivideExact(a, gcd, false);
    //     b = UnivariateDivision.DivideExact(b, gcd, false);
    //     UnivariatePolynomial<BigInteger>[] xgcd = ModularExtendedResultantGCD0(a, b);
    //     xgcd[0].Multiply(gcd);
    //     UnivariatePolynomial<BigInteger> g = xgcd[0], s = xgcd[1], t = xgcd[2];
    //     BigInteger @as = Z.Gcd(aContent, s.Content()), bt = Z.Gcd(bContent, t.Content());
    //     aContent = aContent.DivideExact(@as);
    //     bContent = bContent.DivideExact(bt);
    //     s.DivideExact(@as);
    //     t.DivideExact(bt);
    //     t.Multiply(aContent);
    //     g.Multiply(aContent);
    //     s.Multiply(bContent);
    //     g.Multiply(bContent);
    //     return xgcd;
    // }

    // TODO
    // private static UnivariatePolynomial<BigInteger>[] ModularExtendedResultantGCD0(UnivariatePolynomial<BigInteger> a,
    //     UnivariatePolynomial<BigInteger> b)
    // {
    //     BigInteger gcd = UnivariateResultants.ModularResultant(a, b);
    //     UnivariatePolynomial<BigInteger>[] previousBase = null, @base = null;
    //     BigInteger basePrime = null;
    //     PrimesIterator primesLoop = new PrimesIterator(SmallPrimes.NextPrime(1 << 28));
    //     while (true)
    //     {
    //         long prime = primesLoop.Take();
    //         BigInteger bPrime = new BigInteger(prime);
    //         if (a.Lc().Remainder(bPrime).IsZero || b.Lc().Remainder(bPrime).IsZero)
    //             continue;
    //         IntegersZp ring = new IntegersZp(bPrime);
    //         UnivariatePolynomialZp64 aMod = AsOverZp64(a.SetRing(ring)), bMod = AsOverZp64(b.SetRing(ring));
    //         UnivariatePolynomialZp64[] modularXGCD = PolynomialExtendedGCD(aMod, bMod);
    //         if (modularXGCD[0].degree != 0)
    //             continue;
    //
    //         // resultant correction
    //         long correction = gcd.Mod(bPrime).LongValueExact();
    //         Arrays.Stream(modularXGCD).ForEach((p) => p.Multiply(correction));
    //
    //         //save the base
    //         if (@base == null)
    //         {
    //             //make base monic and multiply lcGCD
    //             @base = Arrays.Stream(modularXGCD).Map(UnivariatePolynomialZp64.ToBigPoly())
    //                 .ToArray(UnivariatePolynomial[].New());
    //             basePrime = bPrime;
    //             continue;
    //         }
    //
    //
    //         //CRT lifting
    //         ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic = CreateMagic(Z, basePrime, bPrime);
    //         BigInteger newBasePrime = basePrime.Multiply(bPrime);
    //         for (int e = 0; e < 3; ++e)
    //         {
    //             @base[e] = @base[e].SetRingUnsafe(new IntegersZp(newBasePrime));
    //             if (@base[e].degree < modularXGCD[e].degree)
    //                 @base[e].EnsureCapacity(modularXGCD[e].degree);
    //             for (int i = 0; i <= @base[e].degree; ++i)
    //                 @base[e].data[i] = ChineseRemainders.ChineseRemainders(Z, magic, @base[e][i],
    //                     BigInteger.ValueOf(modularXGCD[e][i]));
    //             @base[e].FixDegree();
    //         }
    //
    //         basePrime = newBasePrime;
    //
    //         // compute candidate
    //         UnivariatePolynomial<BigInteger>[] candidate = Arrays.Stream(@base)
    //             .Map(UnivariatePolynomial.AsPolyZSymmetric()).ToArray(UnivariatePolynomial[].New());
    //         BigInteger content = Z.Gcd(candidate[0].Content(), candidate[1].Content(), candidate[2].Content());
    //         Arrays.Stream(candidate).ForEach((p) => p.DivideExact(content));
    //
    //         // two trials didn't change the result, probably we are done
    //         if ((previousBase != null && Arrays.Equals(candidate, previousBase)))
    //         {
    //             previousBase = candidate;
    //             if (!SatisfiesXGCD(a, b, candidate))
    //                 continue;
    //             return candidate;
    //         }
    //
    //         previousBase = candidate;
    //     }
    // }

    private static bool SatisfiesXGCD<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b,
        UnivariatePolynomial<E>[] xgcd)
    {
        Ring<E> ring = xgcd[0].ring;
        foreach (var subs in (E[])[ring.GetZero(), ring.GetOne()])
        {
            E ea = a.Evaluate(subs),
                es = xgcd[1].Evaluate(subs),
                eb = b.Evaluate(subs),
                et = xgcd[2].Evaluate(subs),
                eg = xgcd[0].Evaluate(subs);
            if (!ring.AddMutable(ring.MultiplyMutable(ea, es), ring.MultiplyMutable(eb, et)).Equals(eg))
                return false;
        }

        return a.Clone().Multiply(xgcd[1]).Add(b.Clone().Multiply(xgcd[2])).Equals(xgcd[0]);
    }

    // TODO
    // private static UnivariatePolynomial<UnivariatePolynomial<E>> TrivialGCDInNumberField<E>(
    //     UnivariatePolynomial<UnivariatePolynomial<E>> a, UnivariatePolynomial<UnivariatePolynomial<E>> b)
    // {
    //     UnivariatePolynomial<UnivariatePolynomial<E>> trivialGCD = TrivialGCD(a, b);
    //     if (trivialGCD != null)
    //         return trivialGCD;
    //     AlgebraicNumberField<UnivariatePolynomial<E>> ring = (AlgebraicNumberField<UnivariatePolynomial<E>>)a.ring;
    //     if (!a.Stream().AllMatch(ring.IsInTheBaseField()) || !b.Stream().AllMatch(ring.IsInTheBaseField()))
    //         return null;
    //     UnivariatePolynomial<E> ar = a.MapCoefficients(ring.GetMinimalPolynomial().ring, UnivariatePolynomial.Cc()),
    //         br = b.MapCoefficients(ring.GetMinimalPolynomial().ring, UnivariatePolynomial.Cc());
    //     return PolynomialGCD(ar, br).MapCoefficients(ring,
    //         (cf) => UnivariatePolynomial.Constant(ring.GetMinimalPolynomial().ring, cf));
    // }

    //TODO
    // public static UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> PolynomialGCDInNumberField(
    //     UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a,
    //     UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> b)
    // {
    //     UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> simpleGCD = TrivialGCDInNumberField(a, b);
    //     if (simpleGCD != null)
    //         return simpleGCD;
    //     AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> numberField =
    //         (AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>>)a.ring;
    //     UnivariatePolynomial<Rational<BigInteger>> minimalPoly = numberField.GetMinimalPolynomial();
    //     a = a.Clone();
    //     b = b.Clone();
    //
    //     // reduce problem to the case with integer monic minimal polynomial
    //     if (minimalPoly.Stream().AllMatch(Rational.IsIntegral()))
    //     {
    //         // minimal poly is already monic & integer
    //         UnivariatePolynomial<BigInteger> minimalPolyZ = minimalPoly.MapCoefficients(Z, Rational.Numerator());
    //         AlgebraicNumberField<UnivariatePolynomial<BigInteger>>
    //             numberFieldZ = new AlgebraicNumberField(minimalPolyZ);
    //         RemoveDenominators(a);
    //         RemoveDenominators(b);
    //         UnivariatePolynomial<UnivariatePolynomial<BigInteger>> gcdZ =
    //             GcdAssociateInNumberField(
    //                 a.MapCoefficients(numberFieldZ, (cf) => cf.MapCoefficients(Z, Rational.Numerator())),
    //                 b.MapCoefficients(numberFieldZ, (cf) => cf.MapCoefficients(Z, Rational.Numerator())));
    //         return gcdZ.MapCoefficients(numberField, (p) => p.MapCoefficients(Q, (cf) => new Rational(Z, cf))).Monic();
    //     }
    //     else
    //     {
    //         // replace s -> s / lc(minPoly)
    //         BigInteger minPolyLeadCoeff = CommonDenominator(minimalPoly);
    //         Rational<BigInteger> scale = new Rational(Z, Z.GetOne(), minPolyLeadCoeff),
    //             scaleReciprocal = scale.Reciprocal();
    //         AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>> scaledNumberField =
    //             new AlgebraicNumberField(minimalPoly.Scale(scale).Monic());
    //         return PolynomialGCDInNumberField(a.MapCoefficients(scaledNumberField, (cf) => cf.Scale(scale)),
    //                 b.MapCoefficients(scaledNumberField, (cf) => cf.Scale(scale)))
    //             .MapCoefficients(numberField, (cf) => cf.Scale(scaleReciprocal));
    //     }
    // }
    
    // TODO
    // private static void PseudoMonicize(UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a)
    // {
    //     UnivariatePolynomial<Rational<BigInteger>> inv = a.ring.Reciprocal(a.Lc());
    //     a.Multiply(Util.ToCommonDenominator(inv)._1.MapCoefficients(Q, Q.MkNumerator()));
    // }

    // TODO
    // static BigInteger RemoveDenominators(UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a)
    // {
    //     BigInteger denominator = Rings.Z.Lcm(() => a.Stream().Select(Util.CommonDenominator()).Iterator());
    //     a.Multiply(a.ring.ValueOfBigInteger(denominator));
    //     return denominator;
    // }

    // TODO
    // public static UnivariatePolynomial<UnivariatePolynomial<BigInteger>> PolynomialGCDInRingOfIntegersOfNumberField(
    //     UnivariatePolynomial<UnivariatePolynomial<BigInteger>> a,
    //     UnivariatePolynomial<UnivariatePolynomial<BigInteger>> b)
    // {
    //     if (!a.Lc().IsConstant() || !b.Lc().IsConstant())
    //         throw new ArgumentException(
    //             "Univariate GCD in non-field extensions requires polynomials have integer leading coefficients.");
    //     UnivariatePolynomial<BigInteger> aContent = a.Content(), bContent = b.Content();
    //     UnivariatePolynomial<BigInteger> contentGCD = aContent.CreateConstant(BigInteger.GreatestCommonDivisor(aContent.Cc(), bContent.Cc()));
    //     a = a.Clone().DivideExact(aContent);
    //     b = b.Clone().DivideExact(bContent);
    //     return GcdAssociateInNumberField0(a, b).Multiply(contentGCD);
    // }

    // TODO
    // static UnivariatePolynomial<UnivariatePolynomial<BigInteger>> GcdAssociateInNumberField(
    //     UnivariatePolynomial<UnivariatePolynomial<BigInteger>> a,
    //     UnivariatePolynomial<UnivariatePolynomial<BigInteger>> b)
    // {
    //     AlgebraicNumberField<UnivariatePolynomial<BigInteger>> numberField =
    //         (AlgebraicNumberField<UnivariatePolynomial<BigInteger>>)a.ring;
    //     IntegerPrimitivePart(a);
    //     IntegerPrimitivePart(b);
    //     if (!a.Lc().IsConstant())
    //         a.Multiply(numberField.Normalizer(a.Lc()));
    //     if (!b.Lc().IsConstant())
    //         b.Multiply(numberField.Normalizer(b.Lc()));
    //     IntegerPrimitivePart(a);
    //     IntegerPrimitivePart(b);
    //
    //     // if all coefficients are simple numbers (no algebraic elements)
    //     UnivariatePolynomial<UnivariatePolynomial<BigInteger>> simpleGCD = TrivialGCDInNumberField(a, b);
    //     if (simpleGCD != null)
    //         return simpleGCD;
    //     return GcdAssociateInNumberField0(a, b);
    // }

    static BigInteger IntegerPrimitivePart(UnivariatePolynomial<UnivariatePolynomial<BigInteger>> p)
    {
        var gcd = Rings.Z.Gcd(p.Stream().SelectMany(cp => cp.Stream()).Order().ToList());
        p.Stream().ForEach((cf) => cf.DivideExact(gcd));
        return gcd;
    }

    // TODO
    // static UnivariatePolynomial<UnivariatePolynomial<BigInteger>> GcdAssociateInNumberField0(
    //     UnivariatePolynomial<UnivariatePolynomial<BigInteger>> a,
    //     UnivariatePolynomial<UnivariatePolynomial<BigInteger>> b)
    // {
    //     AlgebraicNumberField<UnivariatePolynomial<BigInteger>> numberField =
    //         (AlgebraicNumberField<UnivariatePolynomial<BigInteger>>)a.ring;
    //     UnivariateRing<UnivariatePolynomial<BigInteger>> auxRing = Rings.UnivariateRing(Z);
    //     UnivariatePolynomial<BigInteger> minimalPoly = numberField.GetMinimalPolynomial();
    //
    //     // Weinberger & Rothschild (1976) correction denominator
    //     BigInteger lcGCD = Z.Gcd(a.Lc().Cc(), b.Lc().Cc()),
    //         disc = UnivariateResultants.Discriminant(minimalPoly),
    //         correctionFactor = disc.Multiply(lcGCD);
    //     BigInteger crtPrime = null;
    //     UnivariatePolynomial<UnivariatePolynomial<BigInteger>> gcd = null, prevCandidate = null;
    //     PrimesIterator primes = new PrimesIterator(1 << 20);
    //     while (true)
    //     {
    //         long prime = primes.Take();
    //         IntegersZp64 zpRing = new IntegersZp64(prime);
    //         UnivariatePolynomialZp64 minimalPolyMod = AsOverZp64(minimalPoly, zpRing);
    //         if (minimalPolyMod.NNonZeroTerms() != minimalPoly.NNonZeroTerms())
    //
    //             // bad prime
    //             continue;
    //         FiniteField<UnivariatePolynomialZp64> modRing = new FiniteField<UnivariatePolynomialZp64>(minimalPolyMod);
    //         UnivariatePolynomial<UnivariatePolynomialZp64> aMod =
    //                 a.MapCoefficients(modRing, (cf) => AsOverZp64(cf, zpRing)),
    //             bMod = b.MapCoefficients(modRing, (cf) => AsOverZp64(cf, zpRing));
    //         UnivariatePolynomial<UnivariatePolynomialZp64> gcdMod;
    //         try
    //         {
    //             gcdMod = PolynomialGCD(aMod, bMod);
    //         }
    //         catch (Exception e)
    //         {
    //             // bad prime
    //             continue;
    //         }
    //
    //         if (gcdMod.IsConstant())
    //             return a.CreateOne();
    //         gcdMod.Multiply(correctionFactor.Mod(prime).LongValue());
    //         BigInteger bPrime = BigInteger.ValueOf(prime);
    //         if (crtPrime == null || gcdMod.degree < gcd.degree)
    //         {
    //             crtPrime = bPrime;
    //             gcd = gcdMod.MapCoefficients(auxRing, (cf) => cf.ToBigPoly().SetRing(Z));
    //             continue;
    //         }
    //
    //         if (gcdMod.degree > gcd.degree)
    //
    //             // bad prime
    //             continue;
    //         ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic = CreateMagic(Z, crtPrime, bPrime);
    //         bool updated = false;
    //         for (int i = gcd.degree; i >= 0; --i)
    //         {
    //             bool u = UpdateCRT(magic, gcd.data[i], gcdMod.data[i]);
    //             if (u)
    //                 updated = true;
    //         }
    //
    //         crtPrime = crtPrime.Multiply(bPrime);
    //
    //         // do trial division
    //         IntegersZp crtRing = new IntegersZp(crtPrime);
    //         UnivariatePolynomial<UnivariatePolynomial<BigInteger>> candidate = gcd.MapCoefficients(numberField,
    //                 (cf) => numberField.ValueOf(UnivariatePolynomial.AsPolyZSymmetric(cf.SetRingUnsafe(crtRing))))
    //             .PrimitivePart();
    //         if (prevCandidate == null)
    //         {
    //             prevCandidate = candidate;
    //             continue;
    //         }
    //
    //         if (!updated || prevCandidate.Equals(candidate))
    //         {
    //             UnivariatePolynomial<UnivariatePolynomial<BigInteger>> rem;
    //             rem = UnivariateDivision.PseudoRemainderAdaptive(b, candidate, true);
    //             if (rem == null || !rem.IsZero())
    //                 continue;
    //             rem = UnivariateDivision.PseudoRemainderAdaptive(a, candidate, true);
    //             if (rem == null || !rem.IsZero())
    //                 continue;
    //             return candidate;
    //         }
    //
    //         prevCandidate = candidate;
    //     }
    // }

    // TODO
    // public static bool UpdateCRT(ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic,
    //     UnivariatePolynomial<BigInteger> accumulated, UnivariatePolynomialZp64 update)
    // {
    //     bool updated = false;
    //     accumulated.EnsureCapacity(update.degree);
    //     for (int i = Math.Max(accumulated.degree, update.degree); i >= 0; --i)
    //     {
    //         BigInteger oldCf = accumulated[i];
    //         BigInteger newCf = ChineseRemainders.ChineseRemainders(Z, magic, oldCf, new BigInteger(update[i]));
    //         if (!oldCf.Equals(newCf))
    //             updated = true;
    //         accumulated.data[i] = newCf;
    //     }
    //
    //     accumulated.FixDegree();
    //     return updated;
    // }
}
