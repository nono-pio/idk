using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Poly;
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
    /// Helper methods for univariate polynomial arithmetic.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class UnivariatePolynomialArithmetic
    {
        private UnivariatePolynomialArithmetic()
        {
        }

        /// <summary>
        /// Returns the remainder of {@code dividend} and {@code polyModulus}.
        /// </summary>
        /// <param name="dividend">the polynomial</param>
        /// <param name="polyModulus">the modulus</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the result will be placed directly to {@code
        ///                    dividend} and the original {@code dividend} data will be lost</param>
        /// <returns>{@code dividend % polyModulus}</returns>
        public static T PolyMod<T extends IUnivariatePolynomial<T>>(T dividend, T polyModulus, bool copy)
        {
            return UnivariateDivision.Remainder(dividend, polyModulus, copy);
        }

        /// <summary>
        /// Returns the remainder of {@code dividend} and {@code polyModulus} using fast algorithm for pre-conditioned
        /// modulus.
        /// </summary>
        /// <param name="dividend">the polynomial</param>
        /// <param name="polyModulus">the modulus</param>
        /// <param name="invMod">pre-conditioned modulus ({@link UnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)})</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the result will be placed directly to {@code
        ///                    dividend} and the original {@code dividend} data will be lost</param>
        /// <returns>{@code dividend % polyModulus}</returns>
        /// <remarks>@seeUnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)</remarks>
        public static T PolyMod<T extends IUnivariatePolynomial<T>>(T dividend, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod, bool copy)
        {
            return UnivariateDivision.RemainderFast(dividend, polyModulus, invMod, copy);
        }

        /// <summary>
        /// Returns the remainder of the product {@code (m1 * m2)} and {@code polyModulus}.
        /// </summary>
        /// <param name="m1">the first multiplier</param>
        /// <param name="m2">the second multiplier</param>
        /// <param name="polyModulus">the modulus</param>
        /// <param name="copy">whether to clone {@code m1}; if not, the result will be placed directly to the data structure
        ///                    of the first multiplier {@code m1} and the original data of {@code m1} will be lost</param>
        /// <returns>{@code (m1 * m2) % polyModulus}</returns>
        public static T PolyMultiplyMod<T extends IUnivariatePolynomial<T>>(T m1, T m2, T polyModulus, bool copy)
        {
            return PolyMod((copy ? m1.Clone() : m1).Multiply(m2), polyModulus, false);
        }

        /// <summary>
        /// Returns the remainder of the product {@code (m1 * m2)} and {@code polyModulus} using fast algorithm for
        /// pre-conditioned modulus.
        /// </summary>
        /// <param name="m1">the first multiplier</param>
        /// <param name="m2">the second multiplier</param>
        /// <param name="polyModulus">the modulus</param>
        /// <param name="invMod">pre-conditioned modulus ({@link UnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)})</param>
        /// <param name="copy">whether to clone {@code m1}; if not, the result will be placed directly to the data structure
        ///                    of the first multiplier {@code m1} and the original data of {@code m1} will be lost</param>
        /// <returns>{@code (m1 * m2) % polyModulus}</returns>
        /// <remarks>@seeUnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)</remarks>
        public static T PolyMultiplyMod<T extends IUnivariatePolynomial<T>>(T m1, T m2, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod, bool copy)
        {
            return PolyMod((copy ? m1.Clone() : m1).Multiply(m2), polyModulus, invMod, false);
        }

        /// <summary>
        /// Returns the remainder of the sum {@code (m1 + m2)} and {@code polyModulus} using fast algorithm for
        /// pre-conditioned modulus.
        /// </summary>
        /// <param name="m1">the first multiplier</param>
        /// <param name="m2">the second multiplier</param>
        /// <param name="polyModulus">the modulus</param>
        /// <param name="invMod">pre-conditioned modulus ({@link UnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)})</param>
        /// <param name="copy">whether to clone {@code m1}; if not, the result will be placed directly to the data structure
        ///                    of the first multiplier {@code m1} and the original data of {@code m1} will be lost</param>
        /// <returns>{@code (m1 + m2) % polyModulus}</returns>
        /// <remarks>@seeUnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)</remarks>
        public static T PolyAddMod<T extends IUnivariatePolynomial<T>>(T m1, T m2, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod, bool copy)
        {
            return PolyMod((copy ? m1.Clone() : m1).Add(m2), polyModulus, invMod, false);
        }

        /// <summary>
        /// Returns the remainder of the sum {@code (m1 + m2)} and {@code polyModulus}.
        /// </summary>
        /// <param name="m1">the first multiplier</param>
        /// <param name="m2">the second multiplier</param>
        /// <param name="polyModulus">the modulus</param>
        /// <param name="copy">whether to clone {@code m1}; if not, the result will be placed directly to the data structure
        ///                    of the first multiplier {@code m1} and the original data of {@code m1} will be lost</param>
        /// <returns>{@code (m1 + m2) % polyModulus}</returns>
        public static T PolyAddMod<T extends IUnivariatePolynomial<T>>(T m1, T m2, T polyModulus, bool copy)
        {
            return PolyMod((copy ? m1.Clone() : m1).Add(m2), polyModulus, false);
        }

        /// <summary>
        /// Returns the remainder of the difference {@code (m1 - m2)} and {@code polyModulus} using fast algorithm for
        /// pre-conditioned modulus.
        /// </summary>
        /// <param name="m1">the first multiplier</param>
        /// <param name="m2">the second multiplier</param>
        /// <param name="polyModulus">the modulus</param>
        /// <param name="invMod">pre-conditioned modulus ({@link UnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)})</param>
        /// <param name="copy">whether to clone {@code m1}; if not, the result will be placed directly to the data structure
        ///                    of the first multiplier {@code m1} and the original data of {@code m1} will be lost</param>
        /// <returns>{@code (m1 - m2) % polyModulus}</returns>
        /// <remarks>@seeUnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)</remarks>
        public static T PolySubtractMod<T extends IUnivariatePolynomial<T>>(T m1, T m2, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod, bool copy)
        {
            return PolyMod((copy ? m1.Clone() : m1).Subtract(m2), polyModulus, invMod, false);
        }

        /// <summary>
        /// Returns the remainder of the difference {@code (m1 - m2)} and {@code polyModulus}.
        /// </summary>
        /// <param name="m1">the first multiplier</param>
        /// <param name="m2">the second multiplier</param>
        /// <param name="polyModulus">the modulus</param>
        /// <param name="copy">whether to clone {@code m1}; if not, the result will be placed directly to the data structure
        ///                    of the first multiplier {@code m1} and the original data of {@code m1} will be lost</param>
        /// <returns>{@code (m1 - m2) % polyModulus}</returns>
        public static T PolySubtractMod<T extends IUnivariatePolynomial<T>>(T m1, T m2, T polyModulus, bool copy)
        {
            return PolyMod((copy ? m1.Clone() : m1).Subtract(m2), polyModulus, false);
        }

        /// <summary>
        /// Returns the remainder of the negated poly {@code -m1} and {@code polyModulus} using fast algorithm for
        /// pre-conditioned modulus.
        /// </summary>
        /// <param name="m1">the polynomial</param>
        /// <param name="polyModulus">the modulus</param>
        /// <param name="invMod">pre-conditioned modulus ({@link UnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)})</param>
        /// <param name="copy">whether to clone {@code m1}; if not, the result will be placed directly to the data structure
        ///                    of {@code m1} and the original data of {@code m1} will be lost</param>
        /// <returns>{@code (-m1) % polyModulus}</returns>
        /// <remarks>@seeUnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)</remarks>
        public static T PolyNegateMod<T extends IUnivariatePolynomial<T>>(T m1, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod, bool copy)
        {

            // fixme: better implementation possible ?
            return PolyMod((copy ? m1.Clone() : m1).Negate(), polyModulus, invMod, false);
        }

        /// <summary>
        /// Returns the remainder of the negated poly {@code -m1} and {@code polyModulus}.
        /// </summary>
        /// <param name="m1">the polynomial</param>
        /// <param name="polyModulus">the modulus</param>
        /// <param name="copy">whether to clone {@code m1}; if not, the result will be placed directly to the data structure
        ///                    of {@code m1} and the original data of {@code m1} will be lost</param>
        /// <returns>{@code (-m1) % polyModulus}</returns>
        public static T PolyNegateMod<T extends IUnivariatePolynomial<T>>(T m1, T polyModulus, bool copy)
        {
            return PolyMod((copy ? m1.Clone() : m1).Negate(), polyModulus, false);
        }

        /// <summary>
        /// Returns {@code base} in a power of non-negative {@code exponent}
        /// </summary>
        /// <param name="base">the base</param>
        /// <param name="exponent">the non-negative exponent</param>
        /// <param name="copy">whether to clone {@code base}; if not the data of {@code base} will be lost</param>
        /// <returns>{@code base} in a power of {@code e}</returns>
        public static T PolyPow<T extends IUnivariatePolynomial<T>>(T @base, long exponent, bool copy)
        {
            if (exponent < 0)
                throw new ArgumentException();
            T result = @base.CreateOne();
            T k2p = copy ? @base.Clone() : @base;
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

        /// <summary>
        /// Returns {@code base} in a power of non-negative {@code exponent} modulo {@code polyModulus}
        /// </summary>
        /// <param name="base">the base</param>
        /// <param name="exponent">the non-negative exponent</param>
        /// <param name="polyModulus">the modulus</param>
        /// <param name="copy">whether to clone {@code base}; if not the data of {@code base} will be lost</param>
        /// <returns>{@code base} in a power of {@code e} modulo {@code polyModulus}</returns>
        public static T PolyPowMod<T extends IUnivariatePolynomial<T>>(T @base, long exponent, T polyModulus, bool copy)
        {
            return PolyPowMod(@base, exponent, polyModulus, UnivariateDivision.FastDivisionPreConditioning(polyModulus), copy);
        }

        /// <summary>
        /// Returns {@code base} in a power of non-negative {@code exponent} modulo {@code polyModulus}
        /// </summary>
        /// <param name="base">the base</param>
        /// <param name="exponent">the non-negative exponent</param>
        /// <param name="polyModulus">the modulus</param>
        /// <param name="invMod">pre-conditioned modulus ({@link UnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)})</param>
        /// <param name="copy">whether to clone {@code base}; if not the data of {@code base} will be lost</param>
        /// <returns>{@code base} in a power of {@code e} modulo {@code polyModulus}</returns>
        /// <remarks>@seeUnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)</remarks>
        public static T PolyPowMod<T extends IUnivariatePolynomial<T>>(T @base, long exponent, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod, bool copy)
        {
            if (exponent < 0)
                throw new ArgumentException();
            if (exponent == 0)
                return @base.CreateOne();
            T result = @base.CreateOne();
            T k2p = PolyMod(@base, polyModulus, invMod, copy); // this will copy the base
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

        /// <summary>
        /// Returns {@code base} in a power of non-negative {@code exponent} modulo {@code polyModulus}
        /// </summary>
        /// <param name="base">the base</param>
        /// <param name="exponent">the non-negative exponent</param>
        /// <param name="polyModulus">the modulus</param>
        /// <param name="invMod">pre-conditioned modulus ({@link UnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)})</param>
        /// <param name="copy">whether to clone {@code base}; if not the data of {@code base} will be lost</param>
        /// <returns>{@code base} in a power of {@code e} modulo {@code polyModulus}</returns>
        /// <remarks>@seeUnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)</remarks>
        public static T PolyPowMod<T extends IUnivariatePolynomial<T>>(T @base, BigInteger exponent, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod, bool copy)
        {
            if (exponent.Signum() < 0)
                throw new ArgumentException();
            if (exponent.IsZero())
                return @base.CreateOne();
            T result = @base.CreateOne();
            T k2p = PolyMod(@base, polyModulus, invMod, copy); // this will copy the base
            for (;;)
            {
                if (exponent.TestBit(0))
                    result = PolyMod(result.Multiply(k2p), polyModulus, invMod, false);
                exponent = exponent.ShiftRight(1);
                if (exponent.IsZero())
                    return result;
                k2p = PolyMod(k2p.Multiply(k2p), polyModulus, invMod, false);
            }
        }

        /// <summary>
        /// Returns {@code base} in a power of non-negative {@code exponent} modulo {@code polyModulus}
        /// </summary>
        /// <param name="base">the base</param>
        /// <param name="exponent">the non-negative exponent</param>
        /// <param name="polyModulus">the modulus</param>
        /// <param name="copy">whether to clone {@code base}; if not the data of {@code base} will be lost</param>
        /// <returns>{@code base} in a power of {@code e} modulo {@code polyModulus}</returns>
        /// <remarks>@seeUnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)</remarks>
        public static T PolyPowMod<T extends IUnivariatePolynomial<T>>(T @base, BigInteger exponent, T polyModulus, bool copy)
        {
            return PolyPowMod(@base, exponent, polyModulus, UnivariateDivision.FastDivisionPreConditioning(polyModulus), copy);
        }

        /// <summary>
        /// switch between plain and log2 algorithms
        /// </summary>
        private static readonly long MONOMIAL_MOD_EXPONENT_THRESHOLD = 64;
        /// <summary>
        /// Creates {@code x^exponent mod polyModulus}.
        /// </summary>
        /// <param name="exponent">the monomial exponent</param>
        /// <param name="polyModulus">the modulus</param>
        /// <param name="invMod">pre-conditioned modulus ({@link UnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)})</param>
        /// <returns>{@code x^exponent mod polyModulus}</returns>
        public static T CreateMonomialMod<T extends IUnivariatePolynomial<T>>(long exponent, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod)
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

        /// <summary>
        /// Creates {@code x^exponent mod polyModulus}.
        /// </summary>
        /// <param name="exponent">the monomial exponent</param>
        /// <param name="polyModulus">the modulus</param>
        /// <param name="invMod">pre-conditioned modulus ({@link UnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)})</param>
        /// <returns>{@code x^exponent mod polyModulus}</returns>
        public static T CreateMonomialMod<T extends IUnivariatePolynomial<T>>(BigInteger exponent, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod)
        {
            if (exponent.Signum() < 0)
                throw new ArgumentException("Negative exponent: " + exponent);
            if (exponent.IsZero())
                return polyModulus.CreateOne();
            if (exponent.IsLong())
                return CreateMonomialMod(exponent.LongValueExact(), polyModulus, invMod);
            else
                return LargeMonomial(exponent, polyModulus, invMod);
        }

        /// <summary>
        /// plain create and reduce
        /// </summary>
        static T SmallMonomial<T extends IUnivariatePolynomial<T>>(long exponent, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod)
        {
            return UnivariatePolynomialArithmetic.PolyMod(polyModulus.CreateMonomial(MachineArithmetic.SafeToInt(exponent)), polyModulus, invMod, false);
        }

        /// <summary>
        /// repeated squaring
        /// </summary>
        static T LargeMonomial<T extends IUnivariatePolynomial<T>>(long exponent, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod)
        {
            return PolyPowMod(polyModulus.CreateMonomial(1), exponent, polyModulus, invMod, false); //        T base = UnivariatePolynomialArithmetic.polyMod(
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

        /// <summary>
        /// repeated squaring
        /// </summary>
        static T LargeMonomial<T extends IUnivariatePolynomial<T>>(BigInteger exponent, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod)
        {
            return PolyPowMod(polyModulus.CreateMonomial(1), exponent, polyModulus, invMod, false); //        T base = UnivariatePolynomialArithmetic.polyMod(
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
}