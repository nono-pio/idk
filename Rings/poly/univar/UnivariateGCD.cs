using System.Diagnostics;
using System.Numerics;
using Rings.primes;

namespace Rings.poly.univar;

public static class UnivariateGCD
{
    public static T PolynomialGCD<T>(T a, T b) where T : IUnivariatePolynomial<T>
    {
        a.assertSameCoefficientRingWith(b);
        if (Util.isOverMultipleFieldExtension(a))
            return (T)PolynomialGCDInMultipleFieldExtension((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        if (a.isOverFiniteField())
            return HalfGCD(a, b);
        if (a is UnivariatePolynomialZ64)
            return (T)ModularGCD((UnivariatePolynomialZ64)a, (UnivariatePolynomialZ64)b);
        if (a.isOverZ())
            return (T)ModularGCD((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        if (Util.isOverRationals(a))
            return (T)PolynomialGCDInQ((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        if (Util.isOverRingOfIntegersOfSimpleNumberField(a))
            return (T)PolynomialGCDInRingOfIntegersOfNumberField((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        if (Util.isOverSimpleNumberField(a))
            return (T)PolynomialGCDInNumberField((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        if (a.isOverField())
            return HalfGCD(a, b);
        T r = tryNested(a, b);
        if (r != null)
            return r;
        T t = trivialGCD(a, b);
        if (t != null)
            return t;
        return (T)UnivariateResultants.SubresultantPRS((UnivariatePolynomial)a, (UnivariatePolynomial)b).gcd();
    }


    private static T trivialGCD<T>(T a, T b) where T : IUnivariatePolynomial<T>
    {
        if (a.isConstant() || b.isConstant())
        {
            if (a.isOverField())
                return a.createOne();
            else if (a is UnivariatePolynomialZ64)
                return a.createConstant(MachineArithmetic.gcd(
                    ((UnivariatePolynomialZ64)a).content(),
                    ((UnivariatePolynomialZ64)b).content()));
            else if (a is UnivariatePolynomial)
                return (T)((UnivariatePolynomial)a).createConstant(
                    ((UnivariatePolynomial)a).ring.gcd(
                        ((UnivariatePolynomial)a).content(),
                        ((UnivariatePolynomial)b).content()));
        }

        return null;
    }


    private static T tryNested<T>(T a, T b) where T : IUnivariatePolynomial<T>
    {
        if (a is UnivariatePolynomial && ((UnivariatePolynomial)a).ring is MultivariateRing)
            return (T)PolynomialGCDOverMultivariate((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        return null;
    }

    private static <Term extends AMonomial<Term>,
    Poly extends AMultivariatePolynomial<Term, Poly>>

    UnivariatePolynomial<Poly> PolynomialGCDOverMultivariate(UnivariatePolynomial<Poly> a,
        UnivariatePolynomial<Poly> b)
    {
        return MultivariateGCD.PolynomialGCD(
                AMultivariatePolynomial.asMultivariate(a, 0, true),
                AMultivariatePolynomial.asMultivariate(b, 0, true))
            .asUnivariateEliminate(0);
    }

    private static  UnivariatePolynomial<Rational<E>> PolynomialGCDInQ<E>(
        UnivariatePolynomial<Rational<E>> a,
        UnivariatePolynomial<Rational<E>> b)
    {
        Tuple2<UnivariatePolynomial<E>, E> aRat = toCommonDenominator(a);
        Tuple2<UnivariatePolynomial<E>, E> bRat = toCommonDenominator(b);

        return Util.asOverRationals(a.ring, PolynomialGCD(aRat._1, bRat._1)).monic();
    }

    private static <
    Term extends AMonomial<Term>,
    mPoly extends AMultivariatePolynomial<Term, mPoly>,
    sPoly extends IUnivariatePolynomial<sPoly>
    > UnivariatePolynomial<mPoly>
        PolynomialGCDInMultipleFieldExtension(UnivariatePolynomial<mPoly> a, UnivariatePolynomial<mPoly> b)
    {
        MultipleFieldExtension<Term, mPoly, sPoly> ring = (MultipleFieldExtension<Term, mPoly, sPoly>)a.ring;
        SimpleFieldExtension<sPoly> simpleExtension = ring.getSimpleExtension();
        return PolynomialGCD(
                a.mapCoefficients(simpleExtension, ring.inverse),
                b.mapCoefficients(simpleExtension, ring.inverse))
            .mapCoefficients(ring, ring.image);
    }


    public static T[] PolynomialExtendedGCD<T>(T a, T b) where T:IUnivariatePolynomial<T>
    {
        if (Util.isOverQ(a))
            return (T[])ModularExtendedResultantGCDInQ((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        if (a.isOverZ())
            return (T[])ModularExtendedResultantGCDInZ((UnivariatePolynomial)a, (UnivariatePolynomial)b);
        if (a.isOverField())
            return ExtendedHalfGCD(a, b);
        else
            throw new IllegalArgumentException("Polynomial over field is expected");
    }


    public static T[] PolynomialFirstBezoutCoefficient<T>(T a, T b) where T:IUnivariatePolynomial<T>
    {
        if (a.isOverFiniteField() && Math.Min(a.Degree(), b.Degree()) < 384)
            // this is somewhat faster than computing full xGCD
            return EuclidFirstBezoutCoefficient(a, b);
        else
            return Arrays.copyOf(PolynomialExtendedGCD(a, b), 2);
    }


    static UnivariatePolynomial<BigInteger>[] PolynomialExtendedGCDInZbyQ(
        UnivariatePolynomial<BigInteger> a, UnivariatePolynomial<BigInteger> b)
    {
        UnivariatePolynomial<Rational<BigInteger>>[] xgcd = PolynomialExtendedGCD(
            a.mapCoefficients(Q, Q::mkNumerator),
            b.mapCoefficients(Q, Q::mkNumerator));

        Tuple2<UnivariatePolynomial<BigInteger>, BigInteger>
            gcd = toCommonDenominator(xgcd[0]),
            s = toCommonDenominator(xgcd[1]),
            t = toCommonDenominator(xgcd[2]);
        BigInteger lcm = Z.lcm(gcd._2, s._2, t._2);
        return new UnivariatePolynomial[]
        {
            gcd._1.multiply(lcm.divideExact(gcd._2)),
            s._1.multiply(lcm.divideExact(s._2)),
            t._1.multiply(lcm.divideExact(t._2))
        };
    }


    public static T PolynomialGCD<T>(params T[] polynomials) where T:IUnivariatePolynomial<T>
    {
        T gcd = polynomials[0];
        for (int i = 1; i < polynomials.Length; i++)
            gcd = PolynomialGCD(gcd, polynomials[i]);
        return gcd;
    }


    public static T PolynomialGCD<T>(IEnumerable<T> polynomials)  where T:IUnivariatePolynomial<T>
    {
        T gcd = null;
        foreach (T poly in
        polynomials)
        gcd = gcd == null ? poly : PolynomialGCD(gcd, poly);
        return gcd;
    }

    /* ========================================== implementation ==================================================== */

    private static T TrivialGCD<T>(T a, T b)  where T:IUnivariatePolynomial<T> {
        if (a.isZero()) return normalizeGCD(b.clone());
        if (b.isZero()) return normalizeGCD(a.clone());
        if (a.isOne()) return a.clone();
        if (b.isOne()) return b.clone();
        if (a == b) return normalizeGCD(a.clone());
        return null;
    }


    public static T EuclidGCD<T>(T a, T b)  where T:IUnivariatePolynomial<T> {
        a.assertSameCoefficientRingWith(b);

        T trivialGCD = TrivialGCD(a, b);
        if (trivialGCD != null)
            return trivialGCD;

        if (canConvertToZp64(a))
            return Conversions64bit.convert(EuclidGCD(asOverZp64(a), asOverZp64(b)));

        if (a.Degree() < b.Degree())
            return EuclidGCD(b, a);

        T x = a, y = b, r;
        while (true)
        {
            r = UnivariateDivision.remainder(x, y, true);
            if (r == null)
                throw new ArithmeticException("Not divisible with remainder: (" + x + ") / (" + y + ")");

            if (r.isZero())
                break;
            x = y;
            y = r;
        }

        return normalizeGCD(y == a ? y.clone() : (y == b ? y.clone() : y));
    }

    private static T normalizeGCD<T>(T gcd)  where T:IUnivariatePolynomial<T>
    {
        if (gcd.isOverField())
            return gcd.monic();
        else
            return gcd;
    }


    public static T[] ExtendedEuclidGCD<T>(T a, T b)  where T:IUnivariatePolynomial<T> {
        a.assertSameCoefficientRingWith(b);
        if (canConvertToZp64(a))
            return Conversions64bit.convert(a, ExtendedEuclidGCD(asOverZp64(a), asOverZp64(b)));

        T s = a.createZero(), old_s = a.createOne();
        T t = a.createOne(), old_t = a.createZero();
        T r = b.clone(), old_r = a.clone();

        T q;
        T tmp;
        while (!r.isZero())
        {
            q = UnivariateDivision.quotient(old_r, r, true);
            if (q == null)
                throw new ArithmeticException("Not divisible with remainder: (" + old_r + ") / (" + r + ")");

            tmp = old_r;
            old_r = r;
            r = tmp.clone().subtract(q.clone().multiply(r));

            tmp = old_s;
            old_s = s;
            s = tmp.clone().subtract(q.clone().multiply(s));

            tmp = old_t;
            old_t = t;
            t = tmp.clone().subtract(q.clone().multiply(t));
        }

        // assert old_r.equals(a.clone().multiply(old_s).add(b.clone().multiply(old_t))) : a.clone().multiply(old_s).add
        //     (b.clone().multiply(old_t));

        return normalizeExtendedGCD([old_r, old_s, old_t]);
    }

    static T[] normalizeExtendedGCD<T>(T[] xgcd)  where T:IUnivariatePolynomial<T>
    {
        if (!xgcd[0].isOverField())
            return xgcd;

        if (xgcd[0].isZero())
            return xgcd;
        xgcd[1].divideByLC(xgcd[0]);
        if (xgcd.Length > 2)
            xgcd[2].divideByLC(xgcd[0]);
        xgcd[0].monic();
        return xgcd;
    }


    public static T[] EuclidFirstBezoutCoefficient<T>(T a, T b) where T:IUnivariatePolynomial<T> {
        a.assertSameCoefficientRingWith(b);
        if (canConvertToZp64(a))
            return Conversions64bit.convert(a, EuclidFirstBezoutCoefficient(asOverZp64(a), asOverZp64(b)));

        T s = a.createZero(), old_s = a.createOne();
        T r = b, old_r = a;

        T q;
        T tmp;
        while (!r.isZero())
        {
            q = UnivariateDivision.quotient(old_r, r, true);
            if (q == null)
                throw new ArithmeticException("Not divisible with remainder: (" + old_r + ") / (" + r + ")");

            tmp = old_r;
            old_r = r;
            r = tmp.clone().subtract(q.clone().multiply(r));

            tmp = old_s;
            old_s = s;
            s = tmp.clone().subtract(q.clone().multiply(s));
        }

        return normalizeExtendedGCD([old_r, old_s]);
    }


    static int SWITCH_TO_HALF_GCD_ALGORITHM_DEGREE = 180;

    static int SWITCH_TO_HALF_GCD_H_MATRIX_DEGREE = 25;


    public static T HalfGCD<T>(T a, T b)  where T:IUnivariatePolynomial<T>
    {
        a.assertSameCoefficientRingWith(b);

        T trivialGCD = TrivialGCD(a, b);
        if (trivialGCD != null)
            return trivialGCD;

        if (canConvertToZp64(a))
            return Conversions64bit.convert(HalfGCD(asOverZp64(a), asOverZp64(b)));

        if (a.Degree() < b.Degree())
            return HalfGCD(b, a);

        if (a.Degree() == b.Degree())
            b = UnivariateDivision.remainder(b, a, true);

        while (a.Degree() > SWITCH_TO_HALF_GCD_ALGORITHM_DEGREE && !b.isZero())
        {
            T[] col = reduceHalfGCD(a, b);
            a = col[0];
            b = col[1];

            if (!b.isZero())
            {
                T remainder = UnivariateDivision.remainder(a, b, true);
                if (remainder == null)
                    throw new ArithmeticException("Not divisible with remainder: (" + a + ") / (" + b + ")");
                a = b;
                b = remainder;
            }
        }

        return UnivariateGCD.EuclidGCD(a, b);
    }


    public static T[] ExtendedHalfGCD<T>(T a, T b)  where T:IUnivariatePolynomial<T>
    {
        a.assertSameCoefficientRingWith(b);
        if (canConvertToZp64(a))
            return Conversions64bit.convert(a, ExtendedHalfGCD(asOverZp64(a), asOverZp64(b)));

        if (a.Degree() < b.Degree())
        {
            T[] r = ExtendedHalfGCD(b, a);
            ArraysUtil.swap(r, 1, 2);
            return r;
        }

        if (b.isZero())
        {
            T[] result = a.createArray(3);
            result[0] = a.clone();
            result[1] = a.createOne();
            result[2] = a.createZero();
            return normalizeExtendedGCD(result);
        }

        a = a.clone();
        b = b.clone();

        T quotient = null;
        if (a.Degree() == b.Degree())
        {
            T[] qd = UnivariateDivision.divideAndRemainder(a, b, true);
            if (qd == null)
                throw new ArithmeticException("Not divisible with remainder: (" + a + ") / (" + b + ")");
            quotient = qd[0];
            T remainder = qd[1];
            a = b;
            b = remainder;
        }

        T[][] hMatrix = reduceExtendedHalfGCD(a, b, a.Degree() + 1);
        T gcd = a, s, t;

        if (quotient != null)
        {
            s = hMatrix[0][1];
            t = quotient.multiply(hMatrix[0][1]);
            t = hMatrix[0][0].subtract(t);
        }
        else
        {
            s = hMatrix[0][0];
            t = hMatrix[0][1];
        }

        return normalizeExtendedGCD([gcd, s, t]);
    }


    private static T[][] hMatrixPlain<T>(T a, T b, int degreeToReduce, bool reduce)  where T:IUnivariatePolynomial<T>
    {
        T[][] hMatrix = unitMatrix(a);
        int goal = a.Degree() - degreeToReduce;
        if (b.Degree() <= goal)
            return hMatrix;

        T tmpA = a, tmpB = b;
        while (tmpB.Degree() > goal && !tmpB.isZero())
        {
            T[] qd = UnivariateDivision.divideAndRemainder(tmpA, tmpB, true);
            if (qd == null)
                throw new ArithmeticException("Not divisible with remainder: (" + tmpA + ") / (" + tmpB + ")");
            T quotient = qd[0], remainder = qd[1];

            T tmp;
            tmp = quotient.clone().multiply(hMatrix[1][0]);
            tmp = hMatrix[0][0].clone().subtract(tmp);

            hMatrix[0][0] = hMatrix[1][0];
            hMatrix[1][0] = tmp;

            tmp = quotient.clone().multiply(hMatrix[1][1]);
            tmp = hMatrix[0][1].clone().subtract(tmp);

            hMatrix[0][1] = hMatrix[1][1];
            hMatrix[1][1] = tmp;

            tmpA = tmpB;
            tmpB = remainder;
        }

        if (reduce)
        {
            a.setAndDestroy(tmpA);
            b.setAndDestroy(tmpB);
        }

        return hMatrix;
    }

    private static T[][] hMatrixHalfGCD<T>(T a, T b, int d)  where T:IUnivariatePolynomial<T>
    {
        if (b.isZero() || b.Degree() <= a.Degree() - d)
            return unitMatrix(a);

        int n = a.Degree() - 2 * d + 2;
        if (n < 0) n = 0;

        T a1 = a.clone().shiftLeft(n);
        T b1 = b.clone().shiftLeft(n);

        if (d <= SWITCH_TO_HALF_GCD_H_MATRIX_DEGREE)
            return hMatrixPlain(a1, b1, d, false);

        int dR = (d + 1) / 2;
        if (dR < 1)
            dR = 1;
        if (dR >= d)
            dR = d - 1;

        T[][] hMatrixR = hMatrixHalfGCD(a1, b1, dR);
        T[] col = columnMultiply(hMatrixR, a1, b1);
        a1 = col[0];
        b1 = col[1];


        int dL = b1.Degree() - a.Degree() + n + d;
        if (b1.isZero() || dL <= 0)
            return hMatrixR;

        T[] qd = UnivariateDivision.divideAndRemainder(a1, b1, false);
        if (qd == null)
            throw new ArithmeticException("Not divisible with remainder: (" + a1 + ") / (" + b1 + ")");
        T quotient = qd[0], remainder = qd[1];
        T[][] hMatrixL = hMatrixHalfGCD(b1, remainder, dL);

        T tmp;
        tmp = quotient.clone().multiply(hMatrixR[1][0]);
        tmp = hMatrixR[0][0].clone().subtract(tmp);
        hMatrixR[0][0] = hMatrixR[1][0];
        hMatrixR[1][0] = tmp;

        tmp = quotient.clone().multiply(hMatrixR[1][1]);
        tmp = hMatrixR[0][1].clone().subtract(tmp);
        hMatrixR[0][1] = hMatrixR[1][1];
        hMatrixR[1][1] = tmp;

        return matrixMultiply(hMatrixL, hMatrixR);
    }


    static T[][] reduceExtendedHalfGCD<T>(T a, T b, int d) where T:IUnivariatePolynomial<T>
    {
        Debug.Assert(a.Degree() >= b.Degree());
        if (b.isZero() || b.Degree() <= a.Degree() - d)
            return unitMatrix(a);

        int aDegree = a.Degree();
        if (d <= SWITCH_TO_HALF_GCD_H_MATRIX_DEGREE)
            return hMatrixPlain(a, b, d, true);

        int dL = (d + 1) / 2;
        if (dL < 1)
            dL = 1;
        if (dL >= d)
            dL = d - 1;

        T[][] hMatrixR = hMatrixHalfGCD(a, b, dL);
        T[] col = columnMultiply(hMatrixR, a, b);
        a.setAndDestroy(col[0]);
        b.setAndDestroy(col[1]);

        int dR = b.Degree() - aDegree + d;
        if (b.isZero() || dR <= 0)
            return hMatrixR;

        T[] qd = UnivariateDivision.divideAndRemainder(a, b, true);
        if (qd == null)
            throw new ArithmeticException("Not divisible with remainder: (" + a + ") / (" + b + ")");
        T quotient = qd[0], remainder = qd[1];

        a.setAndDestroy(b);
        b.setAndDestroy(remainder);
        T[][] hMatrixL = reduceExtendedHalfGCD(a, b, dR);

        T tmp;
        tmp = quotient.clone().multiply(hMatrixR[1][0]);
        tmp = hMatrixR[0][0].clone().subtract(tmp);

        hMatrixR[0][0] = hMatrixR[1][0];
        hMatrixR[1][0] = tmp;


        tmp = quotient.clone().multiply(hMatrixR[1][1]);
        tmp = hMatrixR[0][1].clone().subtract(tmp);
        hMatrixR[0][1] = hMatrixR[1][1];
        hMatrixR[1][1] = tmp;

        return matrixMultiply(hMatrixL, hMatrixR);
    }


    static T[] reduceHalfGCD<T>(T a, T b)  where T:IUnivariatePolynomial<T>
    {
        int d = (a.Degree() + 1) / 2;

        if (b.isZero() || b.Degree() <= a.Degree() - d)
            return [a,b];

        int aDegree = a.Degree();

        int d1 = (d + 1) / 2;
        if (d1 < 1)
            d1 = 1;
        if (d1 >= d)
            d1 = d - 1;


        T[][] hMatrix = hMatrixHalfGCD(a, b, d1);
        T[] col = columnMultiply(hMatrix, a, b);
        a = col[0];
        b = col[1];

        int d2 = b.Degree() - aDegree + d;

        if (b.isZero() || d2 <= 0)
            return [a, b];

        T remainder = UnivariateDivision.remainder(a, b, true);
        if (remainder == null)
            throw new ArithmeticException("Not divisible with remainder: (" + a + ") / (" + b + ")");
        a = b;
        b = remainder;

        return columnMultiply(hMatrixHalfGCD(a, b, d2), a, b);
    }

    private static T[][] matrixMultiply<T>(T[][] matrix1, T[][] matrix2)  where T:IUnivariatePolynomial<T>
    {
        T[][] r = matrix1[0][0].createArray2d(2, 2);
        r[0][0] = matrix1[0][0].clone().multiply(matrix2[0][0]).add(matrix1[0][1].clone().multiply(matrix2[1][0]));
        r[0][1] = matrix1[0][0].clone().multiply(matrix2[0][1]).add(matrix1[0][1].clone().multiply(matrix2[1][1]));
        r[1][0] = matrix1[1][0].clone().multiply(matrix2[0][0]).add(matrix1[1][1].clone().multiply(matrix2[1][0]));
        r[1][1] = matrix1[1][0].clone().multiply(matrix2[0][1]).add(matrix1[1][1].clone().multiply(matrix2[1][1]));
        return r;
    }

    private static T[] columnMultiply<T>(T[][] hMatrix, T row1, T row2) where T:IUnivariatePolynomial<T>
    {
        T[] resultColumn = row1.createArray(2);
        resultColumn[0] = hMatrix[0][0].clone().multiply(row1).add(hMatrix[0][1].clone().multiply(row2));
        resultColumn[1] = hMatrix[1][0].clone().multiply(row1).add(hMatrix[1][1].clone().multiply(row2));
        return resultColumn;
    }

    private static T[][] unitMatrix<T>(T factory) where T:IUnivariatePolynomial<T>
    {
        T[][] m = factory.createArray2d(2, 2);
        m[0][0] = factory.createOne();
        m[0][1] = factory.createZero();
        m[1][0] = factory.createZero();
        m[1][1] = factory.createOne();
        return m;
    }


    public static UnivariatePolynomialZ64 ModularGCD(UnivariatePolynomialZ64 a, UnivariatePolynomialZ64 b)
    {
        UnivariatePolynomialZ64 trivialGCD = TrivialGCD(a, b);
        if (trivialGCD != null)
            return trivialGCD;

        if (a.Degree < b.Degree)
            return ModularGCD(b, a);
        long aContent = a.content(), bContent = b.content();
        long contentGCD = MachineArithmetic.gcd(aContent, bContent);
        if (a.isConstant() || b.isConstant())
            return UnivariatePolynomialZ64.create(contentGCD);

        return ModularGCD0(a.clone().divideOrNull(aContent), b.clone().divideOrNull(bContent)).multiply(contentGCD);
    }

    

    private static UnivariatePolynomialZ64 ModularGCD0(UnivariatePolynomialZ64 a, UnivariatePolynomialZ64 b)
    {
        Debug.Assert( a.Degree >= b.Degree);

        long lcGCD = MachineArithmetic.gcd(a.lc(), b.lc());
        double bound = Math.Max(a.mignotteBound(), b.mignotteBound()) * lcGCD;

        UnivariatePolynomialZ64 previousBase = null;
        UnivariatePolynomialZp64 @base = null;
        long basePrime = -1;

        PrimesIterator primesLoop = new PrimesIterator(3);
        while (true)
        {
            long prime = primesLoop.take();
            // assert prime != -1 : "long overflow";

            if (a.lc() % prime == 0 || b.lc() % prime == 0)
                continue;

            UnivariatePolynomialZp64 aMod = a.modulus(prime), bMod = b.modulus(prime);
            UnivariatePolynomialZp64 modularGCD = HalfGCD(aMod, bMod);
            //clone if necessary
            if (modularGCD == aMod || modularGCD == bMod)
                modularGCD = modularGCD.clone();

            //coprime polynomials
            if (modularGCD.Degree == 0)
                return UnivariatePolynomialZ64.one();

            // save the @base for the first time or when a new modular image is better
            if (@base == null || @base.Degree > modularGCD.Degree)
            {
                //make @base monic and multiply lcGCD
                modularGCD.monic(lcGCD);
                @base = modularGCD;
                basePrime = prime;
                continue;
            }

            //skip unlucky prime
            if (@base.Degree < modularGCD.Degree)
                continue;

            //lifting
            long newBasePrime = MachineArithmetic.safeMultiply(basePrime, prime);
            long monicFactor =
                modularGCD.multiply(
                    MachineArithmetic.modInverse(modularGCD.lc(), prime),
                    modularGCD.ring.modulus(lcGCD));
            ChineseRemainders.ChineseRemaindersMagicZp64 magic = ChineseRemainders.createMagic(basePrime, prime);
            for (int i = 0; i <= @base.Degree; ++i)
            {
                //this is monic modularGCD multiplied by lcGCD mod prime
                //long oth = mod(safeMultiply(mod(safeMultiply(modularGCD.data[i], monicFactor), prime), lcMod), prime);

                long oth = modularGCD.multiply(modularGCD.data[i], monicFactor);
                @base.data[i] = ChineseRemainders(magic, @base.data[i], oth);
            }

            @base = @base.setModulusUnsafe(newBasePrime);
            basePrime = newBasePrime;

            //either trigger Mignotte's bound or two trials didn't change the result, probably we are done
            UnivariatePolynomialZ64 candidate = @base.asPolyZSymmetric().primitivePart();
            if ((double)basePrime >= 2 * bound || (previousBase != null && candidate.equals(previousBase)))
            {
                previousBase = candidate;
                //first check b since b is less Degree
                UnivariatePolynomialZ64[] div;
                div = UnivariateDivision.divideAndRemainder(b, candidate, true);
                if (div == null || !div[1].isZero())
                    continue;

                div = UnivariateDivision.divideAndRemainder(a, candidate, true);
                if (div == null || !div[1].isZero())
                    continue;

                return candidate;
            }

            previousBase = candidate;
        }
    }



    public static UnivariatePolynomial<BigInteger> ModularGCD(UnivariatePolynomial<BigInteger> a,
        UnivariatePolynomial<BigInteger> b)
    {
        if (!a.ring.Equals(Z))
            throw new ArgumentException("Only polynomials over integers ring are allowed; " + a.ring);
        UnivariatePolynomial<BigInteger> trivialGCD = TrivialGCD(a, b);
        if (trivialGCD != null)
            return trivialGCD;

        if (a.Degree < b.Degree)
            return ModularGCD(b, a);
        BigInteger aContent = a.content(), bContent = b.content();
        BigInteger contentGCD = BigIntegerUtil.gcd(aContent, bContent);
        if (a.isConstant() || b.isConstant())
            return a.createConstant(contentGCD);

        return ModularGCD0(a.clone().divideOrNull(aContent), b.clone().divideOrNull(bContent)).multiply(contentGCD);
    }



    private static UnivariatePolynomial<BigInteger> ModularGCD0(UnivariatePolynomial<BigInteger> a,
        UnivariatePolynomial<BigInteger> b)
    {
        Debug.Assert( a.Degree >= b.Degree);

        BigInteger lcGCD = BigIntegerUtil.gcd(a.lc(), b.lc());
        BigInteger bound2 = BigIntegerUtil
            .Max(UnivariatePolynomial.mignotteBound(a), UnivariatePolynomial.mignotteBound(b)).multiply(lcGCD)
            .shiftLeft(1);
        if (bound2.isLong()
            && a.maxAbsCoefficient().isLong()
            && b.maxAbsCoefficient().isLong())
            try
            {
                // long overflow may occur here in very very rare cases
                return ModularGCD(UnivariatePolynomial.asOverZ64(a), UnivariatePolynomial.asOverZ64(b)).toBigPoly();
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
            long prime = primesLoop.take();
            // assert prime != -1 : "long overflow";

            BigInteger bPrime = new BigInteger(prime);
            if (a.lc().remainder(bPrime).isZero() || b.lc().remainder(bPrime).isZero())
                continue;

            IntegersZp bPrimeDomain = new IntegersZp(bPrime);
            UnivariatePolynomialZp64 aMod = asOverZp64(a.setRing(bPrimeDomain)),
                bMod = asOverZp64(b.setRing(bPrimeDomain));
            UnivariatePolynomialZp64 modularGCD = HalfGCD(aMod, bMod);
            //clone if necessary
            if (modularGCD == aMod || modularGCD == bMod)
                modularGCD = modularGCD.clone();

            //coprime polynomials
            if (modularGCD.Degree == 0)
                return a.createOne();

            // save the @base for the first time or when a new modular image is better
            if (@base == null || @base.Degree > modularGCD.Degree)
            {
                //make @base monic and multiply lcGCD
                long lLcGCD = (long)(lcGCD%bPrime);
                modularGCD.monic(lLcGCD);
                @base = modularGCD;
                basePrime = prime;
                continue;
            }

            //skip unlucky prime
            if (@base.Degree < modularGCD.Degree)
                continue;

            if (MachineArithmetic.isOverflowMultiply(basePrime, prime) ||
                basePrime * prime > MachineArithmetic.MAX_SUPPORTED_MODULUS)
                break;

            //lifting
            long newBasePrime = basePrime * prime;
            long monicFactor = modularGCD.multiply(
                MachineArithmetic.modInverse(modularGCD.lc(), prime),
                lcGCD.mod(bPrime).longValueExact());
            ChineseRemainders.ChineseRemaindersMagicZp64 magic = ChineseRemainders.createMagic(basePrime, prime);
            for (int i = 0; i <= @base.Degree; ++i)
            {
                //this is monic modularGCD multiplied by lcGCD mod prime
                //long oth = mod(safeMultiply(mod(safeMultiply(modularGCD.data[i], monicFactor), prime), lcMod), prime);

                long oth = modularGCD.multiply(modularGCD.data[i], monicFactor);
                @base.data[i] = ChineseRemainders(magic, @base.data[i], oth);
            }

            @base = @base.setModulusUnsafe(newBasePrime);
            basePrime = newBasePrime;

            //either trigger Mignotte's bound or two trials didn't change the result, probably we are done
            UnivariatePolynomialZ64 lCandidate = @base.asPolyZSymmetric().primitivePart();
            if (new BigInteger(basePrime).CompareTo(bound2) >= 0 ||
                (previousBase != null && lCandidate.equals(previousBase)))
            {
                previousBase = lCandidate;
                UnivariatePolynomial<BigInteger> candidate = lCandidate.toBigPoly();
                //first check b since b is less Degree
                UnivariatePolynomial<BigInteger>[] div;
                div = UnivariateDivision.divideAndRemainder(b, candidate, true);
                if (div == null || !div[1].isZero())
                    continue;

                div = UnivariateDivision.divideAndRemainder(a, candidate, true);
                if (div == null || !div[1].isZero())
                    continue;

                return candidate;
            }

            previousBase = lCandidate;
        }

        //continue lifting with multi-precision integers
        UnivariatePolynomial<BigInteger> bPreviousBase = null, bBase = @base.toBigPoly();
        BigInteger bBasePrime = new BigInteger(basePrime);

        while (true)
        {
            long prime = primesLoop.take();
            // assert prime != -1 : "long overflow";

            BigInteger bPrime = new BigInteger(prime);
            if (a.lc().remainder(bPrime).isZero() || b.lc().remainder(bPrime).isZero())
                continue;

            IntegersZp bPrimeDomain = new IntegersZp(bPrime);
            UnivariatePolynomialZp64 aMod = asOverZp64(a.setRing(bPrimeDomain)),
                bMod = asOverZp64(b.setRing(bPrimeDomain));
            UnivariatePolynomialZp64 modularGCD = HalfGCD(aMod, bMod);
            //clone if necessary
            if (modularGCD == aMod || modularGCD == bMod)
                modularGCD = modularGCD.clone();

            //coprime polynomials
            if (modularGCD.Degree == 0)
                return a.createOne();

            //save the @base
            if (bBase == null || bBase.Degree > modularGCD.Degree)
            {
                //make @base monic and multiply lcGCD
                long lLcGCD = lcGCD.mod(bPrime).longValueExact();
                modularGCD.monic(lLcGCD);
                bBase = modularGCD.toBigPoly();
                bBasePrime = bPrime;
                continue;
            }

            //skip unlucky prime
            if (bBase.Degree < modularGCD.Degree)
                continue;

            //lifting
            BigInteger newBasePrime = bBasePrime.multiply(bPrime);
            long monicFactor = modularGCD.multiply(
                MachineArithmetic.modInverse(modularGCD.lc(), prime),
                lcGCD.mod(bPrime).longValueExact());
            ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic = createMagic(Z, bBasePrime, bPrime);
            for (int i = 0; i <= bBase.Degree; ++i)
            {
                //this is monic modularGCD multiplied by lcGCD mod prime
                //long oth = mod(safeMultiply(mod(safeMultiply(modularGCD.data[i], monicFactor), prime), lcMod), prime);

                long oth = modularGCD.multiply(modularGCD.data[i], monicFactor);
                bBase.data[i] = ChineseRemainders(Z, magic, bBase.data[i], new BigInteger(oth));
            }

            bBase = bBase.setRingUnsafe(new IntegersZp(newBasePrime));
            bBasePrime = newBasePrime;

            UnivariatePolynomial<BigInteger> candidate = UnivariatePolynomial.asPolyZSymmetric(bBase).primitivePart();
            //either trigger Mignotte's bound or two trials didn't change the result, probably we are done
            if (bBasePrime.compareTo(bound2) >= 0 || (bPreviousBase != null && candidate.equals(bPreviousBase)))
            {
                bPreviousBase = candidate;
                //first check b since b is less Degree
                UnivariatePolynomial<BigInteger>[] div;
                div = UnivariateDivision.divideAndRemainder(b, candidate, true);
                if (div == null || !div[1].isZero())
                    continue;

                div = UnivariateDivision.divideAndRemainder(a, candidate, true);
                if (div == null || !div[1].isZero())
                    continue;

                return candidate;
            }

            bPreviousBase = candidate;
        }
    }

    

    public static UnivariatePolynomial<Rational<BigInteger>>[] ModularExtendedRationalGCD(
        UnivariatePolynomial<Rational<BigInteger>> a,
        UnivariatePolynomial<Rational<BigInteger>> b)
    {
        if (a == b || a.equals(b))
            return new UnivariatePolynomial[] { a.clone(), a.createZero(), a.createOne() };

        if (a.Degree() < b.Degree())
        {
            UnivariatePolynomial<Rational<BigInteger>>[] r = ModularExtendedRationalGCD(b, a);
            ArraysUtil.swap(r, 1, 2);
            return r;
        }

        if (b.isZero())
        {
            UnivariatePolynomial<Rational<BigInteger>>[] result = a.createArray(3);
            result[0] = a.clone();
            result[1] = a.createOne();
            result[2] = a.createZero();
            return normalizeExtendedGCD(result);
        }

        Tuple2<UnivariatePolynomial<BigInteger>, BigInteger>
            ac = toCommonDenominator(a),
            bc = toCommonDenominator(b);

        UnivariatePolynomial<BigInteger> az = ac._1, bz = bc._1;
        BigInteger
            aContent = az.content(),
            bContent = bz.content();

        UnivariatePolynomial<Rational<BigInteger>>[] xgcd = ModularExtendedRationalGCD0(
            az.clone().divideOrNull(aContent),
            bz.clone().divideOrNull(bContent));
        xgcd[1].multiply(new Rational<BigInteger>(Z, ac._2, aContent));
        xgcd[2].multiply(new Rational<BigInteger>(Z, bc._2, bContent));
        return xgcd;
    }




    static UnivariatePolynomial<Rational<BigInteger>>[] ModularExtendedRationalGCD0(
        UnivariatePolynomial<BigInteger> a,
        UnivariatePolynomial<BigInteger> b)
    {
        Debug.Assert(a.Degree >= b.Degree);
        BigInteger lcGCD = BigIntegerUtil.gcd(a.lc(), b.lc());

        UnivariatePolynomial<Rational<BigInteger>>
            aRat = a.mapCoefficients(Rings.Q, c=> new Rational<BigInteger>(Z, c)),
            bRat = b.mapCoefficients(Rings.Q, c=> new Rational<BigInteger>(Z, c));

        int degreeMax = Math.Max(a.Degree, b.Degree);
        BigInteger bound2 = new BigInteger(degreeMax).increment().pow(degreeMax)
            .multiply(BigIntegerUtil.Max(a.normMax(), b.normMax()).pow(a.Degree + b.Degree))
            .multiply(lcGCD)
            .shiftLeft(1);

        PrimesIterator primesLoop = new PrimesIterator(1031); //SmallPrimes.nextPrime(1 << 25));

        List<BigInteger> primes = new List<BigInteger>();
        List<UnivariatePolynomial<BigInteger>>[] gst = new List[]
            { [], [], [] };
        BigInteger primesMul = BigInteger.One;
        main:
        while (true)
        {
            while (primesMul.CompareTo(bound2) < 0)
            {
                long prime = primesLoop.take();
                BigInteger bPrime = new BigInteger(prime);
                if (a.lc().remainder(bPrime).isZero() || b.lc().remainder(bPrime).isZero())
                    continue;

                IntegersZp bPrimeDomain = new IntegersZp(bPrime);
                UnivariatePolynomialZp64
                    aMod = asOverZp64(a.setRing(bPrimeDomain)),
                    bMod = asOverZp64(b.setRing(bPrimeDomain));

                UnivariatePolynomialZp64[] modularXGCD = ExtendedHalfGCD(aMod, bMod);
                Debug.Assert( modularXGCD[0].isMonic());

                //skip unlucky prime
                if (!gst[0].isEmpty() && modularXGCD[0].Degree > gst[0].get(0).Degree)
                    continue;

                if (!gst[0].isEmpty() && modularXGCD[0].Degree < gst[0].get(0).Degree)
                {
                    primes.Clear();
                    primesMul = BigInteger.One;
                    Arrays.stream(gst).forEach(List::clear);
                }

                long lLcGCD = lcGCD.mod(bPrime).longValueExact();
                long lc = modularXGCD[0].lc();
                for (int i = 0; i < modularXGCD.Length; i++)
                    gst[i].Add(modularXGCD[i].multiply(lLcGCD).divide(lc).toBigPoly());
                primes.Add(bPrime);
                primesMul = primesMul.multiply(bPrime);
            }


            // CRT
            UnivariatePolynomial<BigInteger>[] xgcdBase = new UnivariatePolynomial[3];
            BigInteger[] primesArray = primes.toArray(new BigInteger[primes.size()]);
            for (int i = 0; i < 3; ++i)
            {
                xgcdBase[i] = UnivariatePolynomial.zero(Z);
                int deg = gst[i].stream().mapToInt(UnivariatePolynomial::Degree).Max().getAsInt();
                xgcdBase[i].ensureCapacity(deg);
                for (int j = 0; j <= deg; ++j)
                {
                    int jf = j;
                    BigInteger[] cfs = gst[i].stream().map(p->p.get(jf)).toArray(BigInteger[]::new);
                    xgcdBase[i].data[j] = ChineseRemainders(primesArray, cfs);
                }

                xgcdBase[i].fixDegree();
            }

            while (true)
            {
                // do rational reconstruction
                UnivariatePolynomial<Rational<BigInteger>>[] xgcd = reconstructXGCD(aRat, bRat, xgcdBase, primesMul,
                    bound2);
                if (xgcd != null)
                    return xgcd;

                // continue with CRT
                long prime = primesLoop.take();
                BigInteger bPrime = new BigInteger(prime);
                if (a.lc().remainder(bPrime).isZero() || b.lc().remainder(bPrime).isZero())
                    continue;

                IntegersZp bPrimeDomain = new IntegersZp(bPrime);
                UnivariatePolynomialZp64
                    aMod = asOverZp64(a.setRing(bPrimeDomain)),
                    bMod = asOverZp64(b.setRing(bPrimeDomain));

                UnivariatePolynomialZp64[] modularXGCD = ExtendedHalfGCD(aMod, bMod);
                assert modularXGCD[0].isMonic();

                //skip unlucky prime
                if (modularXGCD[0].Degree > xgcdBase[0].Degree)
                    continue;

                if (modularXGCD[0].Degree < xgcdBase[0].Degree)
                {
                    primes.clear();
                    Arrays.stream(gst).forEach(List::clear);

                    long lLcGCD = lcGCD.mod(bPrime).longValueExact();
                    long lc = modularXGCD[0].lc();
                    for (int i = 0; i < modularXGCD.Length; i++)
                        gst[i].add(modularXGCD[i].multiply(lLcGCD).divide(lc).toBigPoly());
                    primes.add(bPrime);
                    primesMul = bPrime;

                    continue main; // <- extremely rare
                }

                long lLcGCD = lcGCD.mod(bPrime).longValueExact();
                long lc = modularXGCD[0].lc();
                for (UnivariatePolynomialZp64 m :
                modularXGCD)
                m.multiply(lLcGCD).divide(lc);

                ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic = createMagic(Z, primesMul, bPrime);
                for (int i = 0; i < 3; i++)
                {
                    modularXGCD[i].ensureCapacity(xgcdBase[i].Degree);
                    assert modularXGCD[i].Degree <= xgcdBase[i].Degree;
                    for (int j = 0; j <= xgcdBase[i].Degree; ++j)
                        xgcdBase[i].data[j] = ChineseRemainders(Z, magic, xgcdBase[i].data[j],
                            new BigInteger(modularXGCD[i].data[j]));
                }

                primes.add(bPrime);
                primesMul = primesMul.multiply(bPrime);
            }
        }
    }


    private static UnivariatePolynomial<Rational<BigInteger>>[] reconstructXGCD(
        UnivariatePolynomial<Rational<BigInteger>> aRat, UnivariatePolynomial<Rational<BigInteger>> bRat,
        UnivariatePolynomial<BigInteger>[] xgcdBase, BigInteger prime, BigInteger bound2)
    {
        UnivariatePolynomial<Rational<BigInteger>>[] candidate = new UnivariatePolynomial[3];
        for (int i = 0; i < 3; i++)
        {
            candidate[i] = UnivariatePolynomial.zero(Rings.Q);
            candidate[i].ensureCapacity(xgcdBase[i].Degree);
            for (int j = 0; j <= xgcdBase[i].Degree; ++j)
            {
                BigInteger[] numDen = RationalReconstruction.reconstruct(xgcdBase[i].data[j], prime, bound2, bound2);
                if (numDen == null)
                    return null;
                candidate[i].data[j] = new Rational<>(Z, numDen[0], numDen[1]);
            }

            candidate[i].fixDegree();
        }

        BigInteger content = candidate[0].mapCoefficients(Z, Rational::numerator).content();
        Rational<BigInteger> corr = new Rational<>(Z, Z.getOne(), content);

        UnivariatePolynomial<Rational<BigInteger>>
            sCandidate = candidate[1].multiply(corr),
            tCandidate = candidate[2].multiply(corr),
            gCandidate = candidate[0].multiply(corr);

        //first check b since b is less Degree
        UnivariatePolynomial<Rational<BigInteger>>[] bDiv;
        bDiv = UnivariateDivision.divideAndRemainder(bRat, gCandidate, true);
        if (!bDiv[1].isZero())
            return null;
        UnivariatePolynomial<Rational<BigInteger>>[] aDiv;
        aDiv = UnivariateDivision.divideAndRemainder(aRat, gCandidate, true);
        if (!aDiv[1].isZero())
            return null;

        if (!satisfiesXGCD(aDiv[0], sCandidate, bDiv[0], tCandidate))
            return null;
        return candidate;
    }


    private static bool satisfiesXGCD(
        UnivariatePolynomial<Rational<BigInteger>> a, UnivariatePolynomial<Rational<BigInteger>> s,
        UnivariatePolynomial<Rational<BigInteger>> b, UnivariatePolynomial<Rational<BigInteger>> t)
    {
        Rational<BigInteger>
            zero = Rational.zero(Z),
            one = Rational.one(Z);
        for (Rational < BigInteger > subs : new Rational[] { zero, one })
        {
            Rational<BigInteger>
                ea = a.evaluate(subs),
                es = s.evaluate(subs),
                eb = b.evaluate(subs),
                et = t.evaluate(subs);

            if (!ea.multiply(es).add(eb.multiply(et)).isOne())
                return false;
        }
        return a.multiply(s).add(b.multiply(t)).isOne();
    }




    public static UnivariatePolynomial<Rational<BigInteger>>[]
        ModularExtendedResultantGCDInQ(UnivariatePolynomial<Rational<BigInteger>> a,
            UnivariatePolynomial<Rational<BigInteger>> b)
    {
        Tuple2<UnivariatePolynomial<BigInteger>, BigInteger>
            ra = toCommonDenominator(a),
            rb = toCommonDenominator(b);

        UnivariatePolynomial<BigInteger>[] xgcdZ = ModularExtendedResultantGCDInZ(ra._1, rb._1);
        BigInteger content = Z.gcd(xgcdZ[0].content(), ra._2, rb._2);
        xgcdZ[0].divideExact(content);

        UnivariatePolynomial<Rational<BigInteger>>[] xgcd =
            Arrays.stream(xgcdZ)
                .map(p->p.mapCoefficients(Q, Q::mkNumerator))
                .toArray(UnivariatePolynomial[]::new);

        xgcd[1].multiply(Q.mkNumerator(ra._2.divideExact(content)));
        xgcd[2].multiply(Q.mkNumerator(rb._2.divideExact(content)));
        return xgcd;
    }
    

    public static UnivariatePolynomial<BigInteger>[]
        ModularExtendedResultantGCDInZ(UnivariatePolynomial<BigInteger> a,
            UnivariatePolynomial<BigInteger> b)
    {
        if (a == b || a.equals(b))
            return new UnivariatePolynomial[] { a.clone(), a.createZero(), a.createOne() };

        if (a.Degree < b.Degree)
        {
            UnivariatePolynomial<BigInteger>[] r = ModularExtendedResultantGCDInZ(b, a);
            ArraysUtil.swap(r, 1, 2);
            return r;
        }

        if (b.isZero())
        {
            UnivariatePolynomial<BigInteger>[] result = a.createArray(3);
            result[0] = a.clone();
            result[1] = a.createOne();
            result[2] = a.createZero();
            return normalizeExtendedGCD(result);
        }

        BigInteger
            aContent = a.content(),
            bContent = b.content();

        a = a.clone().divideExact(aContent);
        b = b.clone().divideExact(bContent);

        UnivariatePolynomial<BigInteger> gcd = PolynomialGCD(a, b);
        a = UnivariateDivision.divideExact(a, gcd, false);
        b = UnivariateDivision.divideExact(b, gcd, false);

        UnivariatePolynomial<BigInteger>[] xgcd = ModularExtendedResultantGCD0(a, b);
        xgcd[0].multiply(gcd);

        UnivariatePolynomial<BigInteger> g = xgcd[0], s = xgcd[1], t = xgcd[2];

        BigInteger
            as = Z.gcd(aContent, s.content()),
        bt = Z.gcd(bContent, t.content());
        aContent = aContent.divideExact( as);
        bContent = bContent.divideExact(bt);

        s.divideExact( as);
        t.divideExact(bt);

        t.multiply(aContent);
        g.multiply(aContent);
        s.multiply(bContent);
        g.multiply(bContent);

        return xgcd;
    }

    

    private static UnivariatePolynomial<BigInteger>[] ModularExtendedResultantGCD0(UnivariatePolynomial<BigInteger> a,
        UnivariatePolynomial<BigInteger> b)
    {
        assert a.Degree >= b.Degree;

        BigInteger gcd = UnivariateResultants.ModularResultant(a, b);
        UnivariatePolynomial<BigInteger>[] previousBase = null,  @base = null;
        BigInteger basePrime = null;

        PrimesIterator primesLoop = new PrimesIterator(SmallPrimes.nextPrime(1 << 28));
        while (true)
        {
            long prime = primesLoop.take();
            assert prime != -1 : "long overflow";

            BigInteger bPrime = new BigInteger(prime);
            if (a.lc().remainder(bPrime).isZero() || b.lc().remainder(bPrime).isZero())
                continue;

            IntegersZp ring = new IntegersZp(bPrime);
            UnivariatePolynomialZp64
                aMod = asOverZp64(a.setRing(ring)),
                bMod = asOverZp64(b.setRing(ring));

            UnivariatePolynomialZp64[] modularXGCD = PolynomialExtendedGCD(aMod, bMod);
            if (modularXGCD[0].Degree != 0)
                continue;

            // xgcd over finite fields are always normalized
            assert modularXGCD[0].isOne();

            // resultant correction
            long correction = gcd.mod(bPrime).longValueExact();
            Arrays.stream(modularXGCD).forEach(p->p.multiply(correction));

            //save the @base
            if (@base == null)
            {
                //make @base monic and multiply lcGCD
                @base = Arrays.stream(modularXGCD).map(UnivariatePolynomialZp64::toBigPoly)
                    .toArray(UnivariatePolynomial[]::new);
                basePrime = bPrime;
                continue;
            }

            //CRT lifting
            ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic = createMagic(Z, basePrime, bPrime);
            BigInteger newBasePrime = basePrime.multiply(bPrime);
            for (int e = 0; e < 3; ++e)
            {
                @base[e] = @base[e].setRingUnsafe(new IntegersZp(newBasePrime));

                if (@base[e].Degree < modularXGCD[e].Degree)
                    @base[e].ensureCapacity(modularXGCD[e].Degree);

                for (int i = 0; i <= @base[e].Degree; ++i)
                    @base[e].data[i] = ChineseRemainders(Z, magic, @base[e].get(i),
                        new BigInteger(modularXGCD[e].get(i)));

                @base[e].fixDegree();
            }

            basePrime = newBasePrime;

            // compute candidate
            UnivariatePolynomial<BigInteger>[] candidate = Arrays.stream(@base)
                .map(UnivariatePolynomial::asPolyZSymmetric)
                .toArray(UnivariatePolynomial[]::new);
            BigInteger content = Z.gcd(candidate[0].content(), candidate[1].content(), candidate[2].content());
            Arrays.stream(candidate).forEach(p->p.divideExact(content));
            // two trials didn't change the result, probably we are done
            if ((previousBase != null && Arrays.equals(candidate, previousBase)))
            {
                previousBase = candidate;
                if (!satisfiesXGCD(a, b, candidate))
                    continue;
                return candidate;
            }

            previousBase = candidate;
        }
    }


    private static  bool satisfiesXGCD<E>(UnivariatePolynomial<E> a,
        UnivariatePolynomial<E> b,
        UnivariatePolynomial<E>[] xgcd)
    {
        Ring<E> ring = xgcd[0].ring;
        for (E subs :
        ring.createArray(ring.getZero(), ring.getOne())) {
            E
                ea = a.evaluate(subs),
                es = xgcd[1].evaluate(subs),
                eb = b.evaluate(subs),
                et = xgcd[2].evaluate(subs),
                eg = xgcd[0].evaluate(subs);

            if (!ring.addMutable(ring.multiplyMutable(ea, es), ring.multiplyMutable(eb, et)).equals(eg))
                return false;
        }
        return a.clone().multiply(xgcd[1]).add(b.clone().multiply(xgcd[2])).equals(xgcd[0]);
    }

    ////////////////////////////////////// Modular GCD in algebraic number fields //////////////////////////////////////

    private static  UnivariatePolynomial<UnivariatePolynomial<E>>
        TrivialGCDInNumberField<E>(UnivariatePolynomial<UnivariatePolynomial<E>> a,
            UnivariatePolynomial<UnivariatePolynomial<E>> b)
    {
        UnivariatePolynomial<UnivariatePolynomial<E>> trivialGCD = TrivialGCD(a, b);
        if (trivialGCD != null)
            return trivialGCD;

        AlgebraicNumberField<UnivariatePolynomial<E>> ring
            = (AlgebraicNumberField<UnivariatePolynomial<E>>)a.ring;

        if (!a.stream().allMatch(ring::isInTheBaseField)
            || !b.stream().allMatch(ring::isInTheBaseField))
            return null;

        UnivariatePolynomial<E>
            ar = a.mapCoefficients(ring.getMinimalPolynomial().ring, UnivariatePolynomial::cc),
            br = b.mapCoefficients(ring.getMinimalPolynomial().ring, UnivariatePolynomial::cc);
        return PolynomialGCD(ar, br)
            .mapCoefficients(ring, cf->UnivariatePolynomial.constant(ring.getMinimalPolynomial().ring, cf));
    }



    public static UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>>
        PolynomialGCDInNumberField(UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a,
            UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> b)
    {
        UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> simpleGCD = TrivialGCDInNumberField(a, b);
        if (simpleGCD != null)
            return simpleGCD;

        AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>>
            numberField = (AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>>)a.ring;
        UnivariatePolynomial<Rational<BigInteger>> minimalPoly = numberField.getMinimalPolynomial();

        assert numberField.isField();

        a = a.clone();
        b = b.clone();

        // reduce problem to the case with integer monic minimal polynomial
        if (minimalPoly.stream().allMatch(Rational::isIntegral))
        {
            // minimal poly is already monic & integer

            UnivariatePolynomial<BigInteger> minimalPolyZ = minimalPoly.mapCoefficients(Z, Rational::numerator);
            AlgebraicNumberField<UnivariatePolynomial<BigInteger>> numberFieldZ =
                new AlgebraicNumberField<>(minimalPolyZ);

            removeDenominators(a);
            removeDenominators(b);

            assert a.stream().allMatch(p->p.stream().allMatch(Rational::isIntegral));
            assert b.stream().allMatch(p->p.stream().allMatch(Rational::isIntegral));

            UnivariatePolynomial<UnivariatePolynomial<BigInteger>> gcdZ =
                gcdAssociateInNumberField(
                    a.mapCoefficients(numberFieldZ, cf->cf.mapCoefficients(Z, Rational::numerator)),
                    b.mapCoefficients(numberFieldZ, cf->cf.mapCoefficients(Z, Rational::numerator)));

            return gcdZ
                .mapCoefficients(numberField, p->p.mapCoefficients(Q, cf-> new Rational<>(Z, cf)))
                .monic();
        }
        else
        {
            // replace s -> s / lc(minPoly)
            BigInteger minPolyLeadCoeff = commonDenominator(minimalPoly);
            Rational<BigInteger>
                scale = new Rational<>(Z, Z.getOne(), minPolyLeadCoeff),
                scaleReciprocal = scale.reciprocal();

            AlgebraicNumberField<UnivariatePolynomial<Rational<BigInteger>>>
                scaledNumberField = new AlgebraicNumberField<>(minimalPoly.scale(scale).monic());
            return PolynomialGCDInNumberField(
                    a.mapCoefficients(scaledNumberField, cf->cf.scale(scale)),
                    b.mapCoefficients(scaledNumberField, cf->cf.scale(scale)))
                .mapCoefficients(numberField, cf->cf.scale(scaleReciprocal));
        }
    }

    private static void pseudoMonicize(UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a)
    {
        UnivariatePolynomial<Rational<BigInteger>> inv = a.ring.reciprocal(a.lc());
        a.multiply(Util.toCommonDenominator(inv)._1.mapCoefficients(Q, Q::mkNumerator));
        assert a.lc().isConstant();
    }

    static BigInteger removeDenominators(UnivariatePolynomial<UnivariatePolynomial<Rational<BigInteger>>> a)
    {
        BigInteger denominator = Z.lcm(()->a.stream().map(Util::commonDenominator).iterator());
        a.multiply(a.ring.valueOfBigInteger(denominator));
        return denominator;
    }



    public static UnivariatePolynomial<UnivariatePolynomial<BigInteger>>
        PolynomialGCDInRingOfIntegersOfNumberField(UnivariatePolynomial<UnivariatePolynomial<BigInteger>> a,
            UnivariatePolynomial<UnivariatePolynomial<BigInteger>> b)
    {
        if (!a.lc().isConstant() || !b.lc().isConstant())
            throw new IllegalArgumentException(
                "Univariate GCD in non-field extensions requires polynomials have integer leading coefficients.");

        UnivariatePolynomial<BigInteger>
            aContent = a.content(),
            bContent = b.content();
        assert aContent.isConstant();
        assert bContent.isConstant();

        UnivariatePolynomial<BigInteger> contentGCD = aContent.createConstant(aContent.cc().gcd(bContent.cc()));

        a = a.clone().divideExact(aContent);
        b = b.clone().divideExact(bContent);

        return gcdAssociateInNumberField0(a, b).multiply(contentGCD);
    }



    static UnivariatePolynomial<UnivariatePolynomial<BigInteger>> gcdAssociateInNumberField(
        UnivariatePolynomial<UnivariatePolynomial<BigInteger>> a,
        UnivariatePolynomial<UnivariatePolynomial<BigInteger>> b)
    {
        AlgebraicNumberField<UnivariatePolynomial<BigInteger>> numberField
            = (AlgebraicNumberField<UnivariatePolynomial<BigInteger>>)a.ring;

        integerPrimitivePart(a);
        integerPrimitivePart(b);

        if (!a.lc().isConstant())
            a.multiply(numberField.normalizer(a.lc()));

        if (!b.lc().isConstant())
            b.multiply(numberField.normalizer(b.lc()));

        integerPrimitivePart(a);
        integerPrimitivePart(b);

        // if all coefficients are simple numbers (no algebraic elements)
        UnivariatePolynomial<UnivariatePolynomial<BigInteger>> simpleGCD = TrivialGCDInNumberField(a, b);
        if (simpleGCD != null)
            return simpleGCD;

        return gcdAssociateInNumberField0(a, b);
    }

    static BigInteger integerPrimitivePart(UnivariatePolynomial<UnivariatePolynomial<BigInteger>> p)
    {
        BigInteger gcd = Z.gcd(p.stream().flatMap(UnivariatePolynomial::stream).sorted().collect(Collectors.toList()));
        p.stream().forEach(cf->cf.divideExact(gcd));
        return gcd;
    }



    static UnivariatePolynomial<UnivariatePolynomial<BigInteger>> gcdAssociateInNumberField0(
        UnivariatePolynomial<UnivariatePolynomial<BigInteger>> a,
        UnivariatePolynomial<UnivariatePolynomial<BigInteger>> b)
    {
        // conditions for Langemyr & McCallum algorithm
        assert a.lc().isConstant();
        assert b.lc().isConstant();

        AlgebraicNumberField<UnivariatePolynomial<BigInteger>> numberField
            = (AlgebraicNumberField<UnivariatePolynomial<BigInteger>>)a.ring;
        UnivariateRing<UnivariatePolynomial<BigInteger>> auxRing
            = Rings.UnivariateRing(Z);

        UnivariatePolynomial<BigInteger> minimalPoly = numberField.getMinimalPolynomial();

        // Weinberger & Rothschild (1976) correction denominator
        BigInteger
            lcGCD = Z.gcd(a.lc().cc(), b.lc().cc()),
            disc = UnivariateResultants.Discriminant(minimalPoly),
            correctionFactor = disc.multiply(lcGCD);

        BigInteger crtPrime = null;
        UnivariatePolynomial<UnivariatePolynomial<BigInteger>> gcd = null, prevCandidate = null;
        PrimesIterator primes = new PrimesIterator(1 << 20);
        while (true)
        {
            long prime = primes.take();
            IntegersZp64 zpRing = new IntegersZp64(prime);
            UnivariatePolynomialZp64 minimalPolyMod = asOverZp64(minimalPoly, zpRing);
            if (minimalPolyMod.nNonZeroTerms() != minimalPoly.nNonZeroTerms())
                // bad prime
                continue;

            FiniteField<UnivariatePolynomialZp64> modRing = new FiniteField<>(minimalPolyMod);
            UnivariatePolynomial<UnivariatePolynomialZp64>
                aMod = a.mapCoefficients(modRing, cf->asOverZp64(cf, zpRing)),
                bMod = b.mapCoefficients(modRing, cf->asOverZp64(cf, zpRing));

            UnivariatePolynomial<UnivariatePolynomialZp64> gcdMod;
            try
            {
                gcdMod = PolynomialGCD(aMod, bMod);
            }
            catch (Throwable e)
            {
                // bad prime
                continue;
            }

            if (gcdMod.isConstant())
                return a.createOne();

            gcdMod.multiply(correctionFactor.mod(prime).longValue());

            BigInteger bPrime = new BigInteger(prime);
            if (crtPrime == null || gcdMod.Degree < gcd.Degree)
            {
                crtPrime = bPrime;
                gcd = gcdMod.mapCoefficients(auxRing, cf->cf.toBigPoly().setRing(Z));
                continue;
            }

            if (gcdMod.Degree > gcd.Degree)
                // bad prime
                continue;

            ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic = createMagic(Z, crtPrime, bPrime);
            bool updated = false;
            for (int i = gcd.Degree; i >= 0; --i)
            {
                bool u = updateCRT(magic, gcd.data[i], gcdMod.data[i]);
                if (u) updated = true;
            }

            crtPrime = crtPrime.multiply(bPrime);

            // do trial division
            IntegersZp crtRing = new IntegersZp(crtPrime);
            UnivariatePolynomial<UnivariatePolynomial<BigInteger>> candidate =
                gcd.mapCoefficients(numberField,
                        cf->numberField.valueOf(UnivariatePolynomial.asPolyZSymmetric(cf.setRingUnsafe(crtRing))))
                    .primitivePart();

            if (prevCandidate == null)
            {
                prevCandidate = candidate;
                continue;
            }

            if (!updated || prevCandidate.equals(candidate))
            {
                UnivariatePolynomial<UnivariatePolynomial<BigInteger>> rem;

                rem = UnivariateDivision.pseudoRemainderAdaptive(b, candidate, true);
                if (rem == null || !rem.isZero())
                    continue;

                rem = UnivariateDivision.pseudoRemainderAdaptive(a, candidate, true);
                if (rem == null || !rem.isZero())
                    continue;

                return candidate;
            }

            prevCandidate = candidate;
        }
    }


    public static bool updateCRT(ChineseRemainders.ChineseRemaindersMagic<BigInteger> magic,
        UnivariatePolynomial<BigInteger> accumulated,
        UnivariatePolynomialZp64 update)
    {
        bool updated = false;
        accumulated.ensureCapacity(update.Degree);
        for (int i = Math.Max(accumulated.Degree, update.Degree); i >= 0; --i)
        {
            BigInteger oldCf = accumulated.get(i);
            BigInteger newCf = ChineseRemainders(Z, magic, oldCf, new BigInteger(update.get(i)));
            if (!oldCf.Equals(newCf))
                updated = true;
            accumulated.data[i] = newCf;
        }

        accumulated.fixDegree();
        return updated;
    }
}