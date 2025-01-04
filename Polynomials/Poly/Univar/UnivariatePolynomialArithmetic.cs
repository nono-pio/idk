using System.Numerics;
using Polynomials.Utils;

namespace Polynomials.Poly.Univar;

public static class UnivariatePolynomialArithmetic
{
    public static UnivariatePolynomial<E>? PolyMod<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> polyModulus, bool copy)
    {
        return UnivariateDivision.Remainder(dividend, polyModulus, copy);
    }


    public static UnivariatePolynomial<E> PolyMod<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> polyModulus, UnivariateDivision.InverseModMonomial<E> invMod, bool copy)
    {
        return UnivariateDivision.RemainderFast(dividend, polyModulus, invMod, copy);
    }


    public static UnivariatePolynomial<E> PolyMultiplyMod<E>(UnivariatePolynomial<E> m1, UnivariatePolynomial<E> m2, UnivariatePolynomial<E> polyModulus, bool copy)
    {
        return PolyMod((copy ? m1.Clone() : m1).Multiply(m2), polyModulus, false);
    }


    public static UnivariatePolynomial<E> PolyMultiplyMod<E>(UnivariatePolynomial<E> m1, UnivariatePolynomial<E> m2, UnivariatePolynomial<E> polyModulus, UnivariateDivision.InverseModMonomial<E> invMod,
        bool copy)
    {
        return PolyMod((copy ? m1.Clone() : m1).Multiply(m2), polyModulus, invMod, false);
    }


    public static UnivariatePolynomial<E> PolyAddMod<E>(UnivariatePolynomial<E> m1, UnivariatePolynomial<E> m2, UnivariatePolynomial<E> polyModulus, UnivariateDivision.InverseModMonomial<E> invMod, bool copy)
    {
        return PolyMod((copy ? m1.Clone() : m1).Add(m2), polyModulus, invMod, false);
    }


    public static UnivariatePolynomial<E> PolyAddMod<E>(UnivariatePolynomial<E> m1, UnivariatePolynomial<E> m2, UnivariatePolynomial<E> polyModulus, bool copy)
    {
        return PolyMod((copy ? m1.Clone() : m1).Add(m2), polyModulus, false);
    }


    public static UnivariatePolynomial<E> PolySubtractMod<E>(UnivariatePolynomial<E> m1, UnivariatePolynomial<E> m2, UnivariatePolynomial<E> polyModulus, UnivariateDivision.InverseModMonomial<E> invMod,
        bool copy)
    {
        return PolyMod((copy ? m1.Clone() : m1).Subtract(m2), polyModulus, invMod, false);
    }


    public static UnivariatePolynomial<E> PolySubtractMod<E>(UnivariatePolynomial<E> m1, UnivariatePolynomial<E> m2, UnivariatePolynomial<E> polyModulus, bool copy)
    {
        return PolyMod((copy ? m1.Clone() : m1).Subtract(m2), polyModulus, false);
    }


    public static UnivariatePolynomial<E> PolyNegateMod<E>(UnivariatePolynomial<E> m1, UnivariatePolynomial<E> polyModulus, UnivariateDivision.InverseModMonomial<E> invMod, bool copy)
    {
        // fixme: better implementation possible ?
        return PolyMod((copy ? m1.Clone() : m1).Negate(), polyModulus, invMod, false);
    }


    public static UnivariatePolynomial<E> PolyNegateMod<E>(UnivariatePolynomial<E> m1, UnivariatePolynomial<E> polyModulus, bool copy)
    {
        return PolyMod((copy ? m1.Clone() : m1).Negate(), polyModulus, false);
    }


    public static UnivariatePolynomial<E> PolyPow<E>(UnivariatePolynomial<E> @base, long exponent, bool copy)
    {
        if (exponent < 0)
            throw new ArgumentException();
        var result = @base.CreateOne();
        var k2p = copy ? @base.Clone() : @base;
        for (;;)
        {
            if ((exponent & 1) != 0)
                result = result.Multiply(k2p);
            exponent = exponent >> 1;
            if (exponent == 0)
                return result;
            k2p = k2p.Multiply(k2p);
        }
    }


    public static UnivariatePolynomial<E> PolyPowMod<E>(UnivariatePolynomial<E> @base, long exponent, UnivariatePolynomial<E> polyModulus, bool copy)
    {
        return PolyPowMod(@base, exponent, polyModulus, UnivariateDivision.FastDivisionPreConditioning(polyModulus),
            copy);
    }


    public static UnivariatePolynomial<E> PolyPowMod<E>(UnivariatePolynomial<E> @base, long exponent, UnivariatePolynomial<E> polyModulus,
        UnivariateDivision.InverseModMonomial<E> invMod, bool copy)
    {
        if (exponent < 0)
            throw new ArgumentException();
        if (exponent == 0)
            return @base.CreateOne();
        var result = @base.CreateOne();
        var k2p = PolyMod(@base, polyModulus, invMod, copy); // this will copy the base
        for (;;)
        {
            if ((exponent & 1) != 0)
                result = PolyMod(result.Multiply(k2p), polyModulus, invMod, false);
            exponent = exponent >> 1;
            if (exponent == 0)
                return result;
            k2p = PolyMod(k2p.Multiply(k2p), polyModulus, invMod, false);
        }
    }


    public static UnivariatePolynomial<E> PolyPowMod<E>(UnivariatePolynomial<E> @base, BigInteger exponent, UnivariatePolynomial<E> polyModulus,
        UnivariateDivision.InverseModMonomial<E> invMod, bool copy)
    {
        if (exponent.Sign < 0)
            throw new ArgumentException();
        if (exponent.IsZero)
            return @base.CreateOne();
        var result = @base.CreateOne();
        var k2p = PolyMod(@base, polyModulus, invMod, copy); // this will copy the base
        for (;;)
        {
            if (!exponent.IsEven)
                result = PolyMod(result.Multiply(k2p), polyModulus, invMod, false);
            exponent = exponent >> 1;
            if (exponent.IsZero)
                return result;
            k2p = PolyMod(k2p.Multiply(k2p), polyModulus, invMod, false);
        }
    }


    public static UnivariatePolynomial<E> PolyPowMod<E>(UnivariatePolynomial<E> @base, BigInteger exponent, UnivariatePolynomial<E> polyModulus, bool copy)
    {
        return PolyPowMod(@base, exponent, polyModulus, UnivariateDivision.FastDivisionPreConditioning(polyModulus),
            copy);
    }


    private static readonly long MONOMIAL_MOD_EXPONENT_THRESHOLD = 64;


    public static UnivariatePolynomial<E> CreateMonomialMod<E>(long exponent, UnivariatePolynomial<E> polyModulus, UnivariateDivision.InverseModMonomial<E> invMod)
    {
        if (exponent < 0)
            throw new ArgumentException("Negative exponent: " + exponent);
        if (exponent == 0)
            return polyModulus.CreateOne();
        if (exponent < MONOMIAL_MOD_EXPONENT_THRESHOLD)
            return SmallMonomial(exponent, polyModulus, invMod);
        else
            return LargeMonomial(exponent, polyModulus, invMod);
    }


    public static UnivariatePolynomial<E> CreateMonomialMod<E>(BigInteger exponent, UnivariatePolynomial<E> polyModulus,
        UnivariateDivision.InverseModMonomial<E> invMod)
    {
        if (exponent.Sign < 0)
            throw new ArgumentException("Negative exponent: " + exponent);
        if (exponent.IsZero)
            return polyModulus.CreateOne();
        if (exponent.IsLong())
            return CreateMonomialMod((long)exponent, polyModulus, invMod);
        else
            return LargeMonomial(exponent, polyModulus, invMod);
    }


    static UnivariatePolynomial<E> SmallMonomial<E>(long exponent, UnivariatePolynomial<E> polyModulus, UnivariateDivision.InverseModMonomial<E> invMod)
    {
        return UnivariatePolynomialArithmetic.PolyMod(polyModulus.CreateMonomial(MachineArithmetic.SafeToInt(exponent)),
            polyModulus, invMod, false);
    }


    static UnivariatePolynomial<E> LargeMonomial<E>(long exponent, UnivariatePolynomial<E> polyModulus, UnivariateDivision.InverseModMonomial<E> invMod)
    {
        return PolyPowMod(polyModulus.CreateMonomial(1), exponent, polyModulus, invMod,
            false); //        T base = UnivariatePolynomialArithmetic.polyMod(
        //                polyModulus.monomial(MachineArithmetic.safeToInt(MONOMIAL_MOD_EXPONENT_THRESHOLD)),
        //                polyModulus, invMod, false);
        //
        //        T result = base.clone();
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


    static UnivariatePolynomial<E> LargeMonomial<E>(BigInteger exponent, UnivariatePolynomial<E> polyModulus, UnivariateDivision.InverseModMonomial<E> invMod)
    {
        return PolyPowMod(polyModulus.CreateMonomial(1), exponent, polyModulus, invMod,
            false); //        T base = UnivariatePolynomialArithmetic.polyMod(
        //                polyModulus.monomial(MachineArithmetic.safeToInt(MONOMIAL_MOD_EXPONENT_THRESHOLD)),
        //                polyModulus, invMod, false);
        //
        //        T result = base.clone();
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