using System.Numerics;

namespace Rings.poly.univar;

public static class UnivariatePolynomialArithmetic
{
    public static T polyMod<T>(T dividend, T polyModulus, bool copy) where T : IUnivariatePolynomial<T>
    {
        return UnivariateDivision.remainder(dividend, polyModulus, copy);
    }


    public static T polyMod<T>(T dividend, T polyModulus,
        UnivariateDivision.InverseModMonomial<T> invMod, bool copy) where T : IUnivariatePolynomial<T>
    {
        return UnivariateDivision.remainderFast(dividend, polyModulus, invMod, copy);
    }


    public static T polyMultiplyMod<T>(T m1, T m2, T polyModulus, bool copy) where T : IUnivariatePolynomial<T>
    {
        return polyMod((copy ? m1.clone() : m1).multiply(m2), polyModulus, false);
    }


    public static T polyMultiplyMod<T>(T m1, T m2,
        T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod,
        bool copy) where T : IUnivariatePolynomial<T>
    {
        return polyMod((copy ? m1.clone() : m1).multiply(m2), polyModulus, invMod, false);
    }


    public static T polyAddMod<T>(T m1, T m2,
        T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod,
        bool copy) where T : IUnivariatePolynomial<T>
    {
        return polyMod((copy ? m1.clone() : m1).add(m2), polyModulus, invMod, false);
    }


    public static T polyAddMod<T>(T m1, T m2, T polyModulus, bool copy) where T : IUnivariatePolynomial<T>
    {
        return polyMod((copy ? m1.clone() : m1).add(m2), polyModulus, false);
    }


    public static T polySubtractMod<T>(T m1, T m2,
        T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod,
        bool copy) where T : IUnivariatePolynomial<T>
    {
        return polyMod((copy ? m1.clone() : m1).subtract(m2), polyModulus, invMod, false);
    }


    public static T polySubtractMod<T>(T m1, T m2, T polyModulus, bool copy) where T : IUnivariatePolynomial<T>
    {
        return polyMod((copy ? m1.clone() : m1).subtract(m2), polyModulus, false);
    }


    public static T polyNegateMod<T>(T m1,
        T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod,
        bool copy) where T : IUnivariatePolynomial<T>
    {
        // fixme: better implementation possible ?
        return polyMod((copy ? m1.clone() : m1).negate(), polyModulus, invMod, false);
    }


    public static T polyNegateMod<T>(T m1, T polyModulus, bool copy) where T : IUnivariatePolynomial<T>
    {
        return polyMod((copy ? m1.clone() : m1).negate(), polyModulus, false);
    }


    public static T polyPow<T>(T @base, long exponent, bool copy) where T : IUnivariatePolynomial<T>
    {
        if (exponent < 0)
            throw new ArgumentException();

        T result = @base.createOne();
        T k2p = copy ? @base.clone() : @base;
        for (;;)
        {
            if ((exponent & 1) != 0)
                result = result.multiply(k2p);
            exponent = exponent >> 1;
            if (exponent == 0)
                return result;
            k2p = k2p.multiply(k2p);
        }
    }


    public static T polyPowMod<T>(T @base, long exponent,
        T polyModulus,
        bool copy) where T : IUnivariatePolynomial<T>
    {
        return polyPowMod(@base, exponent, polyModulus, UnivariateDivision.fastDivisionPreConditioning(polyModulus),
            copy);
    }


    public static T polyPowMod<T>(T @base, long exponent,
        T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod,
        bool copy) where T : IUnivariatePolynomial<T>
    {
        if (exponent < 0)
            throw new ArgumentException();
        if (exponent == 0)
            return @base.createOne();

        T result = @base.createOne();
        T k2p = polyMod(@base, polyModulus, invMod, copy); // this will copy the @base
        for (;;)
        {
            if ((exponent & 1) != 0)
                result = polyMod(result.multiply(k2p), polyModulus, invMod, false);
            exponent = exponent >> 1;
            if (exponent == 0)
                return result;
            k2p = polyMod(k2p.multiply(k2p), polyModulus, invMod, false);
        }
    }


    public static T polyPowMod<T>(T @base, BigInteger exponent,
        T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod,
        bool copy) where T : IUnivariatePolynomial<T>
    {
        if (exponent.Sign < 0)
            throw new ArgumentException();
        if (exponent.IsZero)
            return @base.createOne();


        T result = @base.createOne();
        T k2p = polyMod(@base, polyModulus, invMod, copy); // this will copy the @base
        for (;;)
        {
            if (exponent.testBit(0))
                result = polyMod(result.multiply(k2p), polyModulus, invMod, false);
            exponent = exponent >> 1;
            if (exponent.IsZero)
                return result;
            k2p = polyMod(k2p.multiply(k2p), polyModulus, invMod, false);
        }
    }


    public static T polyPowMod<T>(T @base, BigInteger exponent,
        T polyModulus,
        bool copy) where T : IUnivariatePolynomial<T>
    {
        return polyPowMod(@base, exponent, polyModulus, UnivariateDivision.fastDivisionPreConditioning(polyModulus),
            copy);
    }


    private static readonly long MONOMIAL_MOD_EXPONENT_THRESHOLD = 64;


    public static T createMonomialMod<T>(long exponent,
        T polyModulus,
        UnivariateDivision.InverseModMonomial<T> invMod) where T : IUnivariatePolynomial<T>
    {
        if (exponent < 0)
            throw new ArgumentException("Negative exponent: " + exponent);

        if (exponent == 0)
            return polyModulus.createOne();

        if (exponent < MONOMIAL_MOD_EXPONENT_THRESHOLD)
            return smallMonomial(exponent, polyModulus, invMod);
        else
            return largeMonomial(exponent, polyModulus, invMod);
    }


    public static T createMonomialMod<T>(BigInteger exponent,
        T polyModulus,
        UnivariateDivision.InverseModMonomial<T> invMod) where T : IUnivariatePolynomial<T>
    {
        if (exponent.Sign < 0)
            throw new ArgumentException("Negative exponent: " + exponent);

        if (exponent.IsZero)
            return polyModulus.createOne();

        if (exponent.isLong())
            return createMonomialMod((long)exponent, polyModulus, invMod);
        else
            return largeMonomial(exponent, polyModulus, invMod);
    }


    static T smallMonomial<T>(long exponent, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod)
        where T : IUnivariatePolynomial<T>
    {
        return UnivariatePolynomialArithmetic.polyMod(polyModulus.createMonomial(MachineArithmetic.safeToInt(exponent)),
            polyModulus, invMod, false);
    }


    static T largeMonomial<T>(long exponent, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod)
        where T : IUnivariatePolynomial<T>
    {
        return polyPowMod(polyModulus.createMonomial(1), exponent, polyModulus, invMod, false);
//        T @base = UnivariatePolynomialArithmetic.polyMod(
//                polyModulus.monomial(MachineArithmetic.safeToInt(MONOMIAL_MOD_EXPONENT_THRESHOLD)),
//                polyModulus, invMod, false);
//
//        T result = @base.clone();
//        long exp = MONOMIAL_MOD_EXPONENT_THRESHOLD;
//        for (; ; ) {
//            if (MachineArithmetic.isOverflowAdd(exp, exp) || exp + exp > exponent)
//                break;
//            result = UnivariatePolynomialArithmetic.polyMultiplyMod(result, result, polyModulus, invMod, false);
//            exp += exp;
//        }
//
//        T rest = createMonomialMod(exponent - exp, polyModulus, invMod);
//        return UnivariatePolynomialArithmetic.polyMultiplyMod(result, rest, polyModulus, invMod, false);
    }


    static T largeMonomial<T>(BigInteger exponent, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod)
        where T : IUnivariatePolynomial<T>
    {
        return polyPowMod(polyModulus.createMonomial(1), exponent, polyModulus, invMod, false);
//        T @base = UnivariatePolynomialArithmetic.polyMod(
//                polyModulus.monomial(MachineArithmetic.safeToInt(MONOMIAL_MOD_EXPONENT_THRESHOLD)),
//                polyModulus, invMod, false);
//
//        T result = @base.clone();
//        BigInteger exp = BigInteger.valueOf(MONOMIAL_MOD_EXPONENT_THRESHOLD);
//        for (; ; ) {
//            if (exp.shiftLeft(1).compareTo(exponent) > 0)
//                break;
//            result = UnivariatePolynomialArithmetic.polyMultiplyMod(result, result, polyModulus, invMod, false);
//            exp = exp.shiftLeft(1);
//        }
//
//        T rest = createMonomialMod(exponent.subtract(exp), polyModulus, invMod);
//        return UnivariatePolynomialArithmetic.polyMultiplyMod(result, rest, polyModulus, invMod, false);
    }
}