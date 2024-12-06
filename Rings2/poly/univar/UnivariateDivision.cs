using Cc.Redberry.Libdivide4j.FastDivision;
using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Poly;
using Java.Util;
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
    /// Division with remainder of univariate polynomials.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class UnivariateDivision
    {
        private UnivariateDivision()
        {
        }

        /* **************************************** Common methods  *************************************** */
        /// <summary>
        /// Returns the remainder of {@code dividend} and monomial {@code x^xDegree}
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="xDegree">monomial degree</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>the remainder</returns>
        public static T RemainderMonomial<T extends IUnivariatePolynomial<T>>(T dividend, int xDegree, bool copy)
        {
            return (copy ? dividend.Clone() : dividend).Truncate(xDegree - 1);
        }

        private static void CheckZeroDivider(IUnivariatePolynomial p)
        {
            if (p.IsZero())
                throw new ArithmeticException("divide by zero");
        }

        /* ************************************ Machine-precision division in Z[x]  ************************************ */
        /// <summary>
        /// Returns {@code {quotient, remainder}} or {@code null} if the division is not possible.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>{quotient, remainder} or {@code null} if the division is not possible</returns>
        public static UnivariatePolynomialZ64[] DivideAndRemainder(UnivariatePolynomialZ64 dividend, UnivariatePolynomialZ64 divider, bool copy)
        {
            CheckZeroDivider(divider);
            if (dividend.IsZero())
                return new UnivariatePolynomialZ64[]
                {
                    UnivariatePolynomialZ64.Zero(),
                    UnivariatePolynomialZ64.Zero()
                };
            if (dividend.degree < divider.degree)
                return new UnivariatePolynomialZ64[]
                {
                    UnivariatePolynomialZ64.Zero(),
                    copy ? dividend.Clone() : dividend
                };
            if (divider.degree == 0)
            {
                UnivariatePolynomialZ64 div = copy ? dividend.Clone() : dividend;
                div = div.DivideOrNull(divider.Lc());
                if (div == null)
                    return null;
                return new UnivariatePolynomialZ64[]
                {
                    div,
                    UnivariatePolynomialZ64.Zero()
                };
            }

            if (divider.degree == 1)
                return DivideAndRemainderLinearDivider(dividend, divider, copy);
            return DivideAndRemainderClassic0(dividend, divider, 1, copy);
        }

        /// <summary>
        /// Returns quotient and remainder using pseudo division.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>{quotient, remainder}</returns>
        public static UnivariatePolynomialZ64[] PseudoDivideAndRemainder(UnivariatePolynomialZ64 dividend, UnivariatePolynomialZ64 divider, bool copy)
        {
            CheckZeroDivider(divider);
            if (dividend.IsZero())
                return new UnivariatePolynomialZ64[]
                {
                    UnivariatePolynomialZ64.Zero(),
                    UnivariatePolynomialZ64.Zero()
                };
            if (dividend.degree < divider.degree)
                return new UnivariatePolynomialZ64[]
                {
                    UnivariatePolynomialZ64.Zero(),
                    copy ? dividend.Clone() : dividend
                };
            long factor = MachineArithmetic.SafePow(divider.Lc(), dividend.degree - divider.degree + 1);
            if (divider.degree == 0)
                return new UnivariatePolynomialZ64[]
                {
                    (copy ? dividend.Clone() : dividend).Multiply(factor / divider.Lc()),
                    UnivariatePolynomialZ64.Zero()
                };
            if (divider.degree == 1)
                return DivideAndRemainderLinearDivider0(dividend, divider, factor, copy);
            return DivideAndRemainderClassic0(dividend, divider, factor, copy);
        }

        /// <summary>
        /// Plain school implementation
        /// </summary>
        static UnivariatePolynomialZ64[] DivideAndRemainderClassic0(UnivariatePolynomialZ64 dividend, UnivariatePolynomialZ64 divider, long dividendRaiseFactor, bool copy)
        {
            if (divider.Lc() == 1 && dividendRaiseFactor == 1)
                return DivideAndRemainderClassicMonic(dividend, divider, copy);
            UnivariatePolynomialZ64 remainder = (copy ? dividend.Clone() : dividend).Multiply(dividendRaiseFactor);
            long[] quotient = new long[dividend.degree - divider.degree + 1];
            Magic magic = MagicSigned(divider.Lc());
            for (int i = dividend.degree - divider.degree; i >= 0; --i)
            {
                if (remainder.degree == divider.degree + i)
                {
                    long quot = DivideSignedFast(remainder.Lc(), magic);
                    if (quot * divider.Lc() != remainder.Lc())
                        return null;
                    quotient[i] = quot;
                    remainder.Subtract(divider, quotient[i], i);
                }
                else
                    quotient[i] = 0;
            }

            return new UnivariatePolynomialZ64[]
            {
                UnivariatePolynomialZ64.Create(quotient),
                remainder
            };
        }

        /// <summary>
        /// Plain school implementation
        /// </summary>
        private static UnivariatePolynomialZ64[] DivideAndRemainderClassicMonic(UnivariatePolynomialZ64 dividend, UnivariatePolynomialZ64 divider, bool copy)
        {
            UnivariatePolynomialZ64 remainder = (copy ? dividend.Clone() : dividend);
            long[] quotient = new long[dividend.degree - divider.degree + 1];
            for (int i = dividend.degree - divider.degree; i >= 0; --i)
            {
                if (remainder.degree == divider.degree + i)
                {
                    quotient[i] = remainder.Lc();
                    remainder.Subtract(divider, quotient[i], i);
                }
                else
                    quotient[i] = 0;
            }

            return new UnivariatePolynomialZ64[]
            {
                UnivariatePolynomialZ64.Create(quotient),
                remainder
            };
        }

        /// <summary>
        /// Returns quotient and remainder using adaptive pseudo division.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>{quotient, remainder}</returns>
        static UnivariatePolynomialZ64[] PseudoDivideAndRemainderAdaptive(UnivariatePolynomialZ64 dividend, UnivariatePolynomialZ64 divider, bool copy)
        {
            CheckZeroDivider(divider);
            if (dividend.IsZero())
                return new UnivariatePolynomialZ64[]
                {
                    UnivariatePolynomialZ64.Zero(),
                    UnivariatePolynomialZ64.Zero()
                };
            if (dividend.degree < divider.degree)
                return new UnivariatePolynomialZ64[]
                {
                    UnivariatePolynomialZ64.Zero(),
                    copy ? dividend.Clone() : dividend
                };
            if (divider.degree == 0)
                return new UnivariatePolynomialZ64[]
                {
                    copy ? dividend.Clone() : dividend,
                    UnivariatePolynomialZ64.Zero()
                };
            if (divider.degree == 1)
                return PseudoDivideAndRemainderLinearDividerAdaptive(dividend, divider, copy);
            return PseudoDivideAndRemainderAdaptive0(dividend, divider, copy);
        }

        /// <summary>
        /// general implementation
        /// </summary>
        static UnivariatePolynomialZ64[] PseudoDivideAndRemainderAdaptive0(UnivariatePolynomialZ64 dividend, UnivariatePolynomialZ64 divider, bool copy)
        {
            UnivariatePolynomialZ64 remainder = copy ? dividend.Clone() : dividend;
            long[] quotient = new long[dividend.degree - divider.degree + 1];
            Magic magic = MagicSigned(divider.Lc());
            for (int i = dividend.degree - divider.degree; i >= 0; --i)
            {
                if (remainder.degree == divider.degree + i)
                {
                    long quot = DivideSignedFast(remainder.Lc(), magic);
                    if (quot * divider.Lc() != remainder.Lc())
                    {
                        long gcd = MachineArithmetic.Gcd(remainder.Lc(), divider.Lc());
                        long factor = divider.Lc() / gcd;
                        remainder.Multiply(factor);
                        for (int j = i + 1; j < quotient.Length; ++j)
                            quotient[j] = MachineArithmetic.SafeMultiply(quotient[j], factor);
                        quot = DivideSignedFast(remainder.Lc(), magic);
                    }

                    quotient[i] = quot;
                    remainder.Subtract(divider, quotient[i], i);
                }
                else
                    quotient[i] = 0;
            }

            return new UnivariatePolynomialZ64[]
            {
                UnivariatePolynomialZ64.Create(quotient),
                remainder
            };
        }

        /// <summary>
        /// Fast division with remainder for divider of the form f(x) = x - u *
        /// </summary>
        static UnivariatePolynomialZ64[] PseudoDivideAndRemainderLinearDividerAdaptive(UnivariatePolynomialZ64 dividend, UnivariatePolynomialZ64 divider, bool copy)
        {

            //apply Horner's method
            long cc = -divider.Cc(), lc = divider.Lc(), factor = 1;
            long[] quotient = copy ? new long[dividend.degree] : dividend.data;
            long res = 0;
            Magic magic = MagicSigned(lc);
            for (int i = dividend.degree;; --i)
            {
                long tmp = dividend.data[i];
                if (i != dividend.degree)
                    quotient[i] = res;
                res = MachineArithmetic.SafeAdd(MachineArithmetic.SafeMultiply(res, cc), MachineArithmetic.SafeMultiply(factor, tmp));
                if (i == 0)
                    break;
                long quot = DivideSignedFast(res, magic);
                if (quot * lc != res)
                {
                    long gcd = MachineArithmetic.Gcd(res, lc), f = lc / gcd;
                    factor = MachineArithmetic.SafeMultiply(factor, f);
                    res = MachineArithmetic.SafeMultiply(res, f);
                    if (i != dividend.degree)
                        for (int j = quotient.length - 1; j >= i; --j)
                            quotient[j] = MachineArithmetic.SafeMultiply(quotient[j], f);
                    quot = DivideSignedFast(res, magic);
                }

                res = quot;
            }

            if (!copy)
                quotient[dividend.degree] = 0;
            return new UnivariatePolynomialZ64[]
            {
                UnivariatePolynomialZ64.Create(quotient),
                UnivariatePolynomialZ64.Create(res)
            };
        }

        /// <summary>
        /// Fast division with remainder for divider of the form f(x) = x - u *
        /// </summary>
        static UnivariatePolynomialZ64[] DivideAndRemainderLinearDivider(UnivariatePolynomialZ64 dividend, UnivariatePolynomialZ64 divider, bool copy)
        {
            return DivideAndRemainderLinearDivider0(dividend, divider, 1, copy);
        }

        /// <summary>
        /// Fast division with remainder for divider of the form f(x) = x - u *
        /// </summary>
        static UnivariatePolynomialZ64[] PseudoDivideAndRemainderLinearDivider(UnivariatePolynomialZ64 dividend, UnivariatePolynomialZ64 divider, bool copy)
        {
            return DivideAndRemainderLinearDivider0(dividend, divider, MachineArithmetic.SafePow(divider.Lc(), dividend.degree), copy);
        }

        /// <summary>
        /// Fast division with remainder for divider of the form f(x) = x - u *
        /// </summary>
        static UnivariatePolynomialZ64[] DivideAndRemainderLinearDivider0(UnivariatePolynomialZ64 dividend, UnivariatePolynomialZ64 divider, long raiseFactor, bool copy)
        {

            //apply Horner's method
            long cc = -divider.Cc(), lc = divider.Lc();
            long[] quotient = copy ? new long[dividend.degree] : dividend.data;
            long res = 0;
            Magic magic = MagicSigned(lc);
            for (int i = dividend.degree;; --i)
            {
                long tmp = dividend.data[i];
                if (i != dividend.degree)
                    quotient[i] = res;
                res = MachineArithmetic.SafeAdd(MachineArithmetic.SafeMultiply(res, cc), MachineArithmetic.SafeMultiply(raiseFactor, tmp));
                if (i == 0)
                    break;
                long quot = DivideSignedFast(res, magic);
                if (quot * lc != res)
                    return null;
                res = quot;
            }

            if (!copy)
                quotient[dividend.degree] = 0;
            return new UnivariatePolynomialZ64[]
            {
                UnivariatePolynomialZ64.Create(quotient),
                UnivariatePolynomialZ64.Create(res)
            };
        }

        /// <summary>
        /// Returns remainder of {@code dividend} and {@code divider} or {@code null} if division is not possible.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>the remainder or {@code null} if division is not possible</returns>
        public static UnivariatePolynomialZ64 Remainder(UnivariatePolynomialZ64 dividend, UnivariatePolynomialZ64 divider, bool copy)
        {
            CheckZeroDivider(divider);
            if (dividend.degree < divider.degree)
                return dividend;
            if (divider.degree == 0)
                return UnivariatePolynomialZ64.Zero();
            if (divider.degree == 1)
                if (divider.Cc() % divider.Lc() == 0)
                    return UnivariatePolynomialZ64.Create(dividend.Evaluate(-divider.Cc() / divider.Lc()));
            return Remainder0(dividend, divider, copy);
        }

        /// <summary>
        /// Plain school implementation
        /// </summary>
        static UnivariatePolynomialZ64 Remainder0(UnivariatePolynomialZ64 dividend, UnivariatePolynomialZ64 divider, bool copy)
        {
            UnivariatePolynomialZ64 remainder = copy ? dividend.Clone() : dividend;
            Magic magic = MagicSigned(divider.Lc());
            for (int i = dividend.degree - divider.degree; i >= 0; --i)
                if (remainder.degree == divider.degree + i)
                {
                    long quot = DivideSignedFast(remainder.Lc(), magic);
                    if (quot * divider.Lc() != remainder.Lc())
                        return null;
                    remainder.Subtract(divider, quot, i);
                }

            return remainder;
        }

        /// <summary>
        /// Returns quotient {@code dividend/ divider}
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>the quotient</returns>
        public static UnivariatePolynomialZ64 Quotient(UnivariatePolynomialZ64 dividend, UnivariatePolynomialZ64 divider, bool copy)
        {
            UnivariatePolynomialZ64[] qd = DivideAndRemainder(dividend, divider, copy);
            if (qd == null)
                return null;
            return qd[0];
        }

        /* ************************************ Machine-precision division in Zp[x]  ************************************ */
        /// <summary>
        /// when to switch between classical and Newton's
        /// </summary>
        private static bool UseClassicalDivision(IUnivariatePolynomial dividend, IUnivariatePolynomial divider)
        {

            // practical benchmarks show that without pre-conditioning,
            // classical division is always faster or at least the same fast
            return true;
        }

        /// <summary>
        /// early checks for division
        /// </summary>
        private static UnivariatePolynomialZp64[] EarlyDivideAndRemainderChecks(UnivariatePolynomialZp64 dividend, UnivariatePolynomialZp64 divider, bool copy)
        {
            CheckZeroDivider(divider);
            if (dividend.IsZero())
                return new UnivariatePolynomialZp64[]
                {
                    dividend.CreateZero(),
                    dividend.CreateZero()
                };
            if (dividend.degree < divider.degree)
                return new UnivariatePolynomialZp64[]
                {
                    dividend.CreateZero(),
                    copy ? dividend.Clone() : dividend
                };
            if (divider.degree == 0)
                return new UnivariatePolynomialZp64[]
                {
                    (copy ? dividend.Clone() : dividend).Divide(divider.Lc()),
                    dividend.CreateZero()
                };
            if (divider.degree == 1)
                return DivideAndRemainderLinearDividerModulus(dividend, divider, copy);
            return null;
        }

        /// <summary>
        /// Returns quotient and remainder.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>{quotient, remainder}</returns>
        public static UnivariatePolynomialZp64[] DivideAndRemainder(UnivariatePolynomialZp64 dividend, UnivariatePolynomialZp64 divider, bool copy)
        {
            UnivariatePolynomialZp64[] r = EarlyDivideAndRemainderChecks(dividend, divider, copy);
            if (r != null)
                return r;
            if (UseClassicalDivision(dividend, divider))
                return DivideAndRemainderClassic0(dividend, divider, copy);
            return DivideAndRemainderFast0(dividend, divider, copy);
        }

        /// <summary>
        /// Classical algorithm for division with remainder.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>{quotient, remainder}</returns>
        public static UnivariatePolynomialZp64[] DivideAndRemainderClassic(UnivariatePolynomialZp64 dividend, UnivariatePolynomialZp64 divider, bool copy)
        {
            UnivariatePolynomialZp64[] r = EarlyDivideAndRemainderChecks(dividend, divider, copy);
            if (r != null)
                return r;
            return DivideAndRemainderClassic0(dividend, divider, copy);
        }

        /// <summary>
        /// Plain school implementation
        /// </summary>
        static UnivariatePolynomialZp64[] DivideAndRemainderClassic0(UnivariatePolynomialZp64 dividend, UnivariatePolynomialZp64 divider, bool copy)
        {
            dividend.AssertSameCoefficientRingWith(divider);
            UnivariatePolynomialZp64 remainder = copy ? dividend.Clone() : dividend;
            long[] quotient = new long[dividend.degree - divider.degree + 1];
            long lcInverse = dividend.ring.Reciprocal(divider.Lc());
            for (int i = dividend.degree - divider.degree; i >= 0; --i)
            {
                if (remainder.degree == divider.degree + i)
                {
                    quotient[i] = remainder.ring.Multiply(remainder.Lc(), lcInverse);
                    remainder.Subtract(divider, quotient[i], i);
                }
                else
                    quotient[i] = 0;
            }

            return new UnivariatePolynomialZp64[]
            {
                dividend.CreateFromArray(quotient),
                remainder
            };
        }

        /// <summary>
        /// Fast division with remainder for divider of the form f(x) = x - u *
        /// </summary>
        static UnivariatePolynomialZp64[] DivideAndRemainderLinearDividerModulus(UnivariatePolynomialZp64 dividend, UnivariatePolynomialZp64 divider, bool copy)
        {
            dividend.AssertSameCoefficientRingWith(divider);

            //apply Horner's method
            long cc = dividend.ring.Negate(divider.Cc());
            long lcInverse = dividend.ring.Reciprocal(divider.Lc());
            if (divider.Lc() != 1)
                cc = dividend.ring.Multiply(cc, lcInverse);
            long[] quotient = copy ? new long[dividend.degree] : dividend.data;
            long res = 0;
            for (int i = dividend.degree; i >= 0; --i)
            {
                long tmp = dividend.data[i];
                if (i != dividend.degree)
                    quotient[i] = dividend.ring.Multiply(res, lcInverse);
                res = dividend.ring.Add(dividend.ring.Multiply(res, cc), tmp);
            }

            if (!copy)
                quotient[dividend.degree] = 0;
            return new UnivariatePolynomialZp64[]
            {
                dividend.CreateFromArray(quotient),
                dividend.CreateFromArray(new long[] { res })
            };
        }

        /* ************************************ Multi-precision division ************************************ */
        /// <summary>
        /// early checks for division
        /// </summary>
        private static UnivariatePolynomial<E>[] EarlyDivideAndRemainderChecks<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, bool copy)
        {
            CheckZeroDivider(divider);
            if (dividend.IsZero())
                return dividend.CreateArray(dividend.CreateZero(), dividend.CreateZero());
            if (dividend.degree < divider.degree)
                return dividend.CreateArray(dividend.CreateZero(), copy ? dividend.Clone() : dividend);
            if (divider.degree == 0)
            {
                UnivariatePolynomial<E> div = copy ? dividend.Clone() : dividend;
                div = div.DivideOrNull(divider.Lc());
                if (div == null)
                    return null;
                return dividend.CreateArray(div, dividend.CreateZero());
            }

            if (divider.degree == 1)
                return DivideAndRemainderLinearDivider(dividend, divider, copy);
            return null;
        }

        /// <summary>
        /// Returns quotient and remainder.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>{quotient, remainder}</returns>
        public static UnivariatePolynomial<E>[] DivideAndRemainder<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, bool copy)
        {
            UnivariatePolynomial<E>[] r = EarlyDivideAndRemainderChecks(dividend, divider, copy);
            if (r != null)
                return r;
            if (UseClassicalDivision(dividend, divider))
                return DivideAndRemainderClassic0(dividend, divider, dividend.ring.GetOne(), copy);
            return DivideAndRemainderFast0(dividend, divider, copy);
        }

        /// <summary>
        /// Returns quotient and remainder using pseudo division.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>{quotient, remainder}</returns>
        public static UnivariatePolynomial<E>[] PseudoDivideAndRemainder<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, bool copy)
        {
            CheckZeroDivider(divider);
            if (dividend.IsZero())
                return dividend.CreateArray(dividend.CreateZero(), dividend.CreateZero());
            if (dividend.degree < divider.degree)
                return dividend.CreateArray(dividend.CreateZero(), copy ? dividend.Clone() : dividend);
            E factor = dividend.ring.Pow(divider.Lc(), dividend.degree - divider.degree + 1);
            if (divider.degree == 0)
                return dividend.CreateArray((copy ? dividend.Clone() : dividend).Multiply(dividend.ring.DivideExact(factor, divider.Lc())), dividend.CreateZero());
            if (divider.degree == 1)
                return DivideAndRemainderLinearDivider0(dividend, divider, factor, copy);
            return DivideAndRemainderClassic0(dividend, divider, factor, copy);
        }

        /// <summary>
        /// Classical algorithm for division with remainder.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>{quotient, remainder}</returns>
        public static UnivariatePolynomial<E>[] DivideAndRemainderClassic<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, bool copy)
        {
            UnivariatePolynomial<E>[] r = EarlyDivideAndRemainderChecks(dividend, divider, copy);
            if (r != null)
                return r;
            return DivideAndRemainderClassic0(dividend, divider, dividend.ring.GetOne(), copy);
        }

        /// <summary>
        /// Plain school implementation
        /// </summary>
        static UnivariatePolynomial<E>[] DivideAndRemainderClassic0<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, E dividendRaiseFactor, bool copy)
        {
            Ring<E> ring = dividend.ring;
            UnivariatePolynomial<E> remainder = (copy ? dividend.Clone() : dividend).Multiply(dividendRaiseFactor);
            E[] quotient = ring.CreateArray(dividend.degree - divider.degree + 1);
            E lcMultiplier, lcDivider;
            if (ring.IsField())
            {
                lcMultiplier = ring.Reciprocal(divider.Lc());
                lcDivider = ring.GetOne();
            }
            else
            {
                lcMultiplier = ring.GetOne();
                lcDivider = divider.Lc();
            }

            for (int i = dividend.degree - divider.degree; i >= 0; --i)
            {
                if (remainder.degree == divider.degree + i)
                {
                    E quot = ring.DivideOrNull(ring.Multiply(remainder.Lc(), lcMultiplier), lcDivider);
                    if (quot == null)
                        return null;
                    quotient[i] = quot;
                    remainder.Subtract(divider, quotient[i], i);
                }
                else
                    quotient[i] = ring.GetZero();
            }

            return dividend.CreateArray(dividend.CreateFromArray(quotient), remainder);
        }

        /// <summary>
        /// Fast division with remainder for divider of the form f(x) = x - u *
        /// </summary>
        static UnivariatePolynomial<E>[] DivideAndRemainderLinearDivider<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, bool copy)
        {
            return DivideAndRemainderLinearDivider0(dividend, divider, dividend.ring.GetOne(), copy);
        }

        /// <summary>
        /// Fast division with remainder for divider of the form f(x) = x - u *
        /// </summary>
        static UnivariatePolynomial<E>[] PseudoDivideAndRemainderLinearDivider<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, bool copy)
        {
            return DivideAndRemainderLinearDivider0(dividend, divider, dividend.ring.Pow(divider.Lc(), dividend.degree), copy);
        }

        /// <summary>
        /// Fast division with remainder for divider of the form f(x) = x - u *
        /// </summary>
        static UnivariatePolynomial<E>[] DivideAndRemainderLinearDivider0<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, E raiseFactor, bool copy)
        {
            dividend.AssertSameCoefficientRingWith(divider);

            //apply Horner's method
            Ring<E> ring = dividend.ring;
            E cc = ring.Negate(divider.Cc()), lcDivider, lcMultiplier;
            if (ring.IsField())
            {
                lcMultiplier = ring.Reciprocal(divider.Lc());
                lcDivider = ring.GetOne();
            }
            else
            {
                lcMultiplier = ring.GetOne();
                lcDivider = divider.Lc();
            }

            E[] quotient = copy ? ring.CreateArray(dividend.degree) : dividend.data;
            E res = ring.GetZero();
            for (int i = dividend.degree;; --i)
            {
                E tmp = dividend.data[i];
                if (i != dividend.degree)
                    quotient[i] = ring.Copy(res);
                res = ring.AddMutable(ring.MultiplyMutable(res, cc), ring.Multiply(raiseFactor, tmp));
                if (i == 0)
                    break;
                E quot = ring.DivideOrNull(ring.Multiply(res, lcMultiplier), lcDivider);
                if (quot == null)
                    return null;
                res = quot;
            }

            if (!copy)
                quotient[dividend.degree] = ring.GetZero();
            return dividend.CreateArray(dividend.CreateFromArray(quotient), dividend.CreateConstant(res));
        }

        /// <summary>
        /// Returns quotient and remainder using adaptive pseudo division.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>{quotient, remainder}</returns>
        static UnivariatePolynomial<E>[] PseudoDivideAndRemainderAdaptive<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, bool copy)
        {
            CheckZeroDivider(divider);
            if (dividend.IsZero())
                return new UnivariatePolynomial[]
                {
                    UnivariatePolynomial.Zero(dividend.ring),
                    UnivariatePolynomial.Zero(dividend.ring)
                };
            if (dividend.degree < divider.degree)
                return new UnivariatePolynomial[]
                {
                    UnivariatePolynomial.Zero(dividend.ring),
                    copy ? dividend.Clone() : dividend
                };
            if (divider.degree == 0)
                return new UnivariatePolynomial[]
                {
                    copy ? dividend.Clone() : dividend,
                    UnivariatePolynomial.Zero(dividend.ring)
                };
            if (divider.degree == 1)
                return PseudoDivideAndRemainderLinearDividerAdaptive(dividend, divider, copy);
            return PseudoDivideAndRemainderAdaptive0(dividend, divider, copy);
        }

        /// <summary>
        /// general implementation
        /// </summary>
        static UnivariatePolynomial<E>[] PseudoDivideAndRemainderAdaptive0<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, bool copy)
        {
            Ring<E> ring = dividend.ring;
            UnivariatePolynomial<E> remainder = copy ? dividend.Clone() : dividend;
            E[] quotient = ring.CreateArray(dividend.degree - divider.degree + 1);
            E dlc = divider.Lc();
            for (int i = dividend.degree - divider.degree; i >= 0; --i)
            {
                if (remainder.degree == divider.degree + i)
                {
                    E quot = ring.DivideOrNull(remainder.Lc(), dlc);
                    if (quot == null)
                    {
                        E gcd = ring.Gcd(remainder.Lc(), divider.Lc());
                        E factor = ring.DivideExact(divider.Lc(), gcd);
                        remainder.Multiply(factor);
                        for (int j = i + 1; j < quotient.Length; ++j)
                            quotient[j] = ring.Multiply(quotient[j], factor);
                        quot = ring.DivideExact(remainder.Lc(), dlc);
                    }

                    quotient[i] = quot;
                    remainder.Subtract(divider, quotient[i], i);
                }
                else
                    quotient[i] = ring.GetZero();
            }

            return new UnivariatePolynomial[]
            {
                UnivariatePolynomial.Create(ring, quotient),
                remainder
            };
        }

        /// <summary>
        /// Fast division with remainder for divider of the form f(x) = x - u *
        /// </summary>
        static UnivariatePolynomial<E>[] PseudoDivideAndRemainderLinearDividerAdaptive<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, bool copy)
        {

            //apply Horner's method
            Ring<E> ring = dividend.ring;
            E cc = ring.Negate(divider.Cc()), lc = divider.Lc(), factor = ring.GetOne();
            E[] quotient = copy ? ring.CreateArray(dividend.degree) : dividend.data;
            E res = ring.GetZero();
            for (int i = dividend.degree;; --i)
            {
                E tmp = dividend.data[i];
                if (i != dividend.degree)
                    quotient[i] = res;
                res = ring.Add(ring.Multiply(res, cc), ring.Multiply(factor, tmp));
                if (i == 0)
                    break;
                E quot = ring.DivideOrNull(res, lc);
                if (quot == null)
                {
                    E gcd = ring.Gcd(res, lc), f = ring.DivideExact(lc, gcd);
                    factor = ring.Multiply(factor, f);
                    res = ring.Multiply(res, f);
                    if (i != dividend.degree)
                        for (int j = quotient.length - 1; j >= i; --j)
                            quotient[j] = ring.Multiply(quotient[j], f);
                    quot = ring.DivideExact(res, lc);
                }

                res = quot;
            }

            if (!copy)
                quotient[dividend.degree] = ring.GetZero();
            return new UnivariatePolynomial[]
            {
                UnivariatePolynomial.Create(ring, quotient),
                UnivariatePolynomial.Create(ring, res)
            };
        }

        /// <summary>
        /// Returns quotient and remainder using adaptive pseudo division.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>{quotient, remainder}</returns>
        static UnivariatePolynomial<E> PseudoRemainderAdaptive<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, bool copy)
        {
            CheckZeroDivider(divider);
            if (dividend.IsZero())
                return UnivariatePolynomial.Zero(dividend.ring);
            if (dividend.degree < divider.degree)
                return copy ? dividend.Clone() : dividend;
            if (divider.degree == 0)
                return UnivariatePolynomial.Zero(dividend.ring);
            if (divider.degree == 1)
                return PseudoRemainderLinearDividerAdaptive(dividend, divider, copy);
            return PseudoRemainderAdaptive0(dividend, divider, copy);
        }

        /// <summary>
        /// general implementation
        /// </summary>
        static UnivariatePolynomial<E> PseudoRemainderAdaptive0<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, bool copy)
        {
            Ring<E> ring = dividend.ring;
            UnivariatePolynomial<E> remainder = copy ? dividend.Clone() : dividend;
            E dlc = divider.Lc();
            for (int i = dividend.degree - divider.degree; i >= 0; --i)
            {
                if (remainder.degree == divider.degree + i)
                {
                    E quot = ring.DivideOrNull(remainder.Lc(), dlc);
                    if (quot == null)
                    {
                        E gcd = ring.Gcd(remainder.Lc(), divider.Lc());
                        E factor = ring.DivideExact(divider.Lc(), gcd);
                        remainder.Multiply(factor);
                        quot = ring.DivideExact(remainder.Lc(), dlc);
                    }

                    remainder.Subtract(divider, quot, i);
                }
            }

            return remainder;
        }

        /// <summary>
        /// Fast division with remainder for divider of the form f(x) = x - u *
        /// </summary>
        static UnivariatePolynomial<E> PseudoRemainderLinearDividerAdaptive<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, bool copy)
        {

            //apply Horner's method
            Ring<E> ring = dividend.ring;
            E cc = ring.Negate(divider.Cc()), lc = divider.Lc(), factor = ring.GetOne();
            E res = ring.GetZero();
            for (int i = dividend.degree;; --i)
            {
                E tmp = dividend.data[i];
                res = ring.Add(ring.Multiply(res, cc), ring.Multiply(factor, tmp));
                if (i == 0)
                    break;
                E quot = ring.DivideOrNull(res, lc);
                if (quot == null)
                {
                    E gcd = ring.Gcd(res, lc), f = ring.DivideExact(lc, gcd);
                    factor = ring.Multiply(factor, f);
                    res = ring.Multiply(res, f);
                    quot = ring.DivideExact(res, lc);
                }

                res = quot;
            }

            return UnivariatePolynomial.Create(ring, res);
        }

        /* ********************************** Fast division algorithm ********************************** */
        /* that is [log2] */
        static int Log2(int l)
        {
            if (l <= 0)
                throw new ArgumentException();
            return 33 - Integer.NumberOfLeadingZeros(l - 1);
        }

        /// <summary>
        /// Holds {@code poly^(-1) mod x^i }
        /// </summary>
        public sealed class InverseModMonomial<Poly> : Serializable
        {
            private static readonly long serialVersionUID = 1;
            readonly Poly poly;
            private InverseModMonomial(Poly poly)
            {
                if (!poly.IsUnitCC())
                    throw new ArgumentException("Smallest coefficient is not a unit: " + poly);
                this.poly = poly;
            }

            /// <summary>
            /// the inverses
            /// </summary>
            private readonly List<Poly> inverses = new List();
            /// <summary>
            /// the inverses
            /// </summary>
            /// <summary>
            /// Returns {@code poly^(-1) mod x^xDegree }. Newton iterations are inside.
            /// </summary>
            /// <param name="xDegree">monomial degree</param>
            /// <returns>{@code poly^(-1) mod x^xDegree }</returns>
            public Poly GetInverse(int xDegree)
            {
                if (xDegree < 1)
                    return null;
                int r = Log2(xDegree);
                if (inverses.Count >= r)
                    return inverses[r - 1];
                int currentSize = inverses.Count;
                Poly gPrev = currentSize == 0 ? poly.CreateOne() : inverses[inverses.Count - 1];
                for (int i = currentSize; i < r; ++i)
                {
                    Poly tmp = gPrev.Clone().Multiply(2).Subtract(gPrev.Clone().Square().Multiply(poly));
                    inverses.Add(gPrev = RemainderMonomial(tmp, 1 << i, false));
                }

                return gPrev;
            }
        }

        /// <summary>
        /// Prepares {@code rev(divider)^(-1) mod x^i } for fast division.
        /// </summary>
        /// <param name="divider">the divider</param>
        public static InverseModMonomial<Poly> FastDivisionPreConditioning<Poly extends IUnivariatePolynomial<Poly>>(Poly divider)
        {
            if (!divider.IsMonic())
                throw new ArgumentException("Only monic polynomials allowed. Input: " + divider);
            return new InverseModMonomial(divider.Clone().Reverse());
        }

        /// <summary>
        /// Prepares {@code rev(divider)^(-1) mod x^i } for fast division.
        /// </summary>
        /// <param name="divider">the divider</param>
        public static InverseModMonomial<Poly> FastDivisionPreConditioningWithLCCorrection<Poly extends IUnivariatePolynomial<Poly>>(Poly divider)
        {
            return new InverseModMonomial(divider.Clone().Monic().Reverse());
        }

        /// <summary>
        /// fast division implementation
        /// </summary>
        public static Poly[] DivideAndRemainderFast0<Poly extends IUnivariatePolynomial<Poly>>(Poly dividend, Poly divider, InverseModMonomial<Poly> invRevMod, bool copy)
        {
            int m = dividend.Degree() - divider.Degree();
            Poly q = RemainderMonomial(dividend.Clone().Reverse().Multiply(invRevMod.GetInverse(m + 1)), m + 1, false).Reverse();
            if (q.Degree() < m)
                q.ShiftRight(m - q.Degree());
            Poly r = (copy ? dividend.Clone() : dividend).Subtract(divider.Clone().Multiply(q));
            return dividend.CreateArray(q, r);
        }

        /* ********************************* Machine-precision fast division in Zp[x]  ******************************** */
        /// <summary>
        /// Fast algorithm for division with remainder using Newton's iteration.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>{quotient, remainder}</returns>
        public static UnivariatePolynomialZp64[] DivideAndRemainderFast(UnivariatePolynomialZp64 dividend, UnivariatePolynomialZp64 divider, bool copy)
        {
            UnivariatePolynomialZp64[] r = EarlyDivideAndRemainderChecks(dividend, divider, copy);
            if (r != null)
                return r;
            return DivideAndRemainderFast0(dividend, divider, copy);
        }

        /// <summary>
        /// Fast algorithm for division with remainder using Newton's iteration.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="invMod">pre-conditioned divider ({@code fastDivisionPreConditioning(divider)})</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>{quotient, remainder}</returns>
        public static UnivariatePolynomialZp64[] DivideAndRemainderFast(UnivariatePolynomialZp64 dividend, UnivariatePolynomialZp64 divider, InverseModMonomial<UnivariatePolynomialZp64> invMod, bool copy)
        {
            UnivariatePolynomialZp64[] r = EarlyDivideAndRemainderChecks(dividend, divider, copy);
            if (r != null)
                return r;
            return DivideAndRemainderFastCorrectLC(dividend, divider, invMod, copy);
        }

        static UnivariatePolynomialZp64[] DivideAndRemainderFastCorrectLC(UnivariatePolynomialZp64 dividend, UnivariatePolynomialZp64 divider, InverseModMonomial<UnivariatePolynomialZp64> invMod, bool copy)
        {

            // if the divider can be directly inverted modulo x^i
            if (divider.IsMonic())
                return DivideAndRemainderFast0(dividend, divider, invMod, copy);
            long lc = divider.Lc();
            long lcInv = divider.ring.Reciprocal(lc);

            // make the divisor monic
            divider.Multiply(lcInv);

            // perform fast arithmetic with monic divisor
            UnivariatePolynomialZp64[] result = DivideAndRemainderFast0(dividend, divider, invMod, copy);

            // reconstruct divisor's lc
            divider.Multiply(lc);

            // reconstruct actual quotient
            result[0].Multiply(lcInv);
            return result;
        }

        static UnivariatePolynomialZp64[] DivideAndRemainderFast0(UnivariatePolynomialZp64 dividend, UnivariatePolynomialZp64 divider, bool copy)
        {

            // if the divider can be directly inverted modulo x^i
            if (divider.IsMonic())
                return DivideAndRemainderFast0(dividend, divider, FastDivisionPreConditioning(divider), copy);
            long lc = divider.Lc();
            long lcInv = divider.ring.Reciprocal(lc);

            // make the divisor monic
            divider.Multiply(lcInv);

            // perform fast arithmetic with monic divisor
            UnivariatePolynomialZp64[] result = DivideAndRemainderFast0(dividend, divider, FastDivisionPreConditioning(divider), copy);

            // reconstruct divisor's lc
            divider.Multiply(lc);

            // reconstruct actual quotient
            result[0].Multiply(lcInv);
            return result;
        }

        /* ********************************* Multi-precision fast division in Zp[x]  ******************************** */
        /// <summary>
        /// Fast algorithm for division with remainder using Newton's iteration.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>{quotient, remainder}</returns>
        public static UnivariatePolynomial<E>[] DivideAndRemainderFast<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, bool copy)
        {
            UnivariatePolynomial<E>[] r = EarlyDivideAndRemainderChecks(dividend, divider, copy);
            if (r != null)
                return r;
            return DivideAndRemainderFast0(dividend, divider, copy);
        }

        /// <summary>
        /// Fast algorithm for division with remainder using Newton's iteration.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="invMod">pre-conditioned divider ({@code fastDivisionPreConditioning(divider)})</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>{quotient, remainder}</returns>
        public static UnivariatePolynomial<E>[] DivideAndRemainderFast<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, InverseModMonomial<UnivariatePolynomial<E>> invMod, bool copy)
        {
            UnivariatePolynomial<E>[] r = EarlyDivideAndRemainderChecks(dividend, divider, copy);
            if (r != null)
                return r;
            return DivideAndRemainderFastCorrectLC(dividend, divider, invMod, copy);
        }

        static UnivariatePolynomial<E>[] DivideAndRemainderFastCorrectLC<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, InverseModMonomial<UnivariatePolynomial<E>> invMod, bool copy)
        {

            // if the divider can be directly inverted modulo x^i
            if (divider.IsMonic())
                return DivideAndRemainderFast0(dividend, divider, invMod, copy);
            E lc = divider.Lc();
            E lcInv = dividend.ring.Reciprocal(lc);

            // make the divisor monic
            divider.Multiply(lcInv);

            // perform fast arithmetic with monic divisor
            UnivariatePolynomial<E>[] result = DivideAndRemainderFast0(dividend, divider, invMod, copy);

            // reconstruct divisor's lc
            divider.Multiply(lc);

            // reconstruct actual quotient
            result[0].Multiply(lcInv);
            return result;
        }

        static UnivariatePolynomial<E>[] DivideAndRemainderFast0<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, bool copy)
        {

            // if the divider can be directly inverted modulo x^i
            if (divider.IsMonic())
                return DivideAndRemainderFast0(dividend, divider, FastDivisionPreConditioning(divider), copy);
            E lc = divider.Lc();
            E lcInv = dividend.ring.Reciprocal(lc);

            // make the divisor monic
            divider.Multiply(lcInv);

            // perform fast arithmetic with monic divisor
            UnivariatePolynomial<E>[] result = DivideAndRemainderFast0(dividend, divider, FastDivisionPreConditioning(divider), copy);

            // reconstruct divisor's lc
            divider.Multiply(lc);

            // reconstruct actual quotient
            result[0].Multiply(lcInv);
            return result;
        }

        /* ********************************* Machine-precision remainders ******************************** */
        /// <summary>
        /// fast division checks
        /// </summary>
        private static UnivariatePolynomialZp64 EarlyRemainderChecks(UnivariatePolynomialZp64 dividend, UnivariatePolynomialZp64 divider, bool copy)
        {
            if (dividend.degree < divider.degree)
                return (copy ? dividend.Clone() : dividend);
            if (divider.degree == 0)
                return dividend.CreateZero();
            if (divider.degree == 1)
            {
                IntegersZp64 ring = dividend.ring;
                return dividend.CreateFromArray(new long[] { dividend.Evaluate(ring.Multiply(ring.Negate(divider.Cc()), ring.Reciprocal(divider.Lc()))) });
            }

            return null;
        }

        /// <summary>
        /// Returns remainder of dividing {@code dividend} by {@code divider}.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>the remainder</returns>
        public static UnivariatePolynomialZp64 Remainder(UnivariatePolynomialZp64 dividend, UnivariatePolynomialZp64 divider, bool copy)
        {
            UnivariatePolynomialZp64 rem = EarlyRemainderChecks(dividend, divider, copy);
            if (rem != null)
                return rem;
            if (UseClassicalDivision(dividend, divider))
                return RemainderClassical0(dividend, divider, copy);
            return DivideAndRemainderFast0(dividend, divider, copy)[1];
        }

        /// <summary>
        /// Plain school implementation
        /// </summary>
        static UnivariatePolynomialZp64 RemainderClassical0(UnivariatePolynomialZp64 dividend, UnivariatePolynomialZp64 divider, bool copy)
        {
            dividend.AssertSameCoefficientRingWith(divider);
            UnivariatePolynomialZp64 remainder = copy ? dividend.Clone() : dividend;
            long lcInverse = dividend.ring.Reciprocal(divider.Lc());
            for (int i = dividend.degree - divider.degree; i >= 0; --i)
                if (remainder.degree == divider.degree + i)
                    remainder.Subtract(divider, remainder.ring.Multiply(remainder.Lc(), lcInverse), i);
            return remainder;
        }

        /// <summary>
        /// Fast remainder using Newton's iteration with switch to classical remainder for small polynomials.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="invMod">pre-conditioned divider ({@code fastDivisionPreConditioning(divider)})</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>the remainder</returns>
        public static UnivariatePolynomialZp64 RemainderFast(UnivariatePolynomialZp64 dividend, UnivariatePolynomialZp64 divider, InverseModMonomial<UnivariatePolynomialZp64> invMod, bool copy)
        {
            UnivariatePolynomialZp64 rem = EarlyRemainderChecks(dividend, divider, copy);
            if (rem != null)
                return rem;
            return DivideAndRemainderFastCorrectLC(dividend, divider, invMod, copy)[1];
        }

        /// <summary>
        /// Returns quotient of dividing {@code dividend} by {@code divider}.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>the quotient</returns>
        public static UnivariatePolynomialZp64 Quotient(UnivariatePolynomialZp64 dividend, UnivariatePolynomialZp64 divider, bool copy)
        {
            UnivariatePolynomialZp64[] qd = EarlyDivideAndRemainderChecks(dividend, divider, copy);
            if (qd != null)
                return qd[0];
            if (UseClassicalDivision(dividend, divider))
                return DivideAndRemainderClassic(dividend, divider, copy)[0];
            return DivideAndRemainderFast0(dividend, divider, copy)[0];
        }

        /// <summary>
        /// Fast quotient using Newton's iteration.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="invMod">pre-conditioned divider ({@code fastDivisionPreConditioning(divider)})</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>the quotient</returns>
        public static UnivariatePolynomialZp64 QuotientFast(UnivariatePolynomialZp64 dividend, UnivariatePolynomialZp64 divider, InverseModMonomial<UnivariatePolynomialZp64> invMod, bool copy)
        {
            UnivariatePolynomialZp64[] qd = EarlyDivideAndRemainderChecks(dividend, divider, copy);
            if (qd != null)
                return qd[0];
            return DivideAndRemainderFastCorrectLC(dividend, divider, invMod, copy)[0];
        }

        /* ********************************* Multi-precision remainders ******************************** */
        /// <summary>
        /// fast division checks
        /// </summary>
        private static UnivariatePolynomial<E> EarlyRemainderChecks<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, bool copy)
        {
            if (dividend.degree < divider.degree)
                return (copy ? dividend.Clone() : dividend);
            if (divider.degree == 0)
                return dividend.CreateZero();
            if (divider.degree == 1)
            {
                E p = dividend.ring.DivideOrNull(dividend.ring.Negate(divider.Cc()), divider.Lc());
                if (p == null)
                    return null;
                return dividend.CreateConstant(dividend.Evaluate(p));
            }

            return null;
        }

        /// <summary>
        /// Returns remainder of {@code dividend} and {@code divider}.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>the remainder</returns>
        public static UnivariatePolynomial<E> Remainder<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, bool copy)
        {
            UnivariatePolynomial<E> rem = EarlyRemainderChecks(dividend, divider, copy);
            if (rem != null)
                return rem;
            if (UseClassicalDivision(dividend, divider))
                return RemainderClassical0(dividend, divider, copy);
            return DivideAndRemainderFast0(dividend, divider, copy)[1];
        }

        /// <summary>
        /// Plain school implementation
        /// </summary>
        static UnivariatePolynomial<E> RemainderClassical0<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, bool copy)
        {
            dividend.AssertSameCoefficientRingWith(divider);
            UnivariatePolynomial<E> remainder = copy ? dividend.Clone() : dividend;
            Ring<E> ring = dividend.ring;
            if (ring.IsField())
            {
                E lcInverse = ring.Reciprocal(divider.Lc());
                for (int i = dividend.degree - divider.degree; i >= 0; --i)
                    if (remainder.degree == divider.degree + i)
                        remainder.Subtract(divider, ring.Multiply(remainder.Lc(), lcInverse), i);
            }
            else
            {
                for (int i = dividend.degree - divider.degree; i >= 0; --i)
                    if (remainder.degree == divider.degree + i)
                    {
                        E quot = ring.DivideOrNull(remainder.Lc(), divider.Lc());
                        if (quot == null)
                            return null;
                        remainder.Subtract(divider, quot, i);
                    }
            }

            return remainder;
        }

        /// <summary>
        /// Fast remainder using Newton's iteration with switch to classical remainder for small polynomials.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="invMod">pre-conditioned divider ({@code fastDivisionPreConditioning(divider)})</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>the remainder</returns>
        public static UnivariatePolynomial<E> RemainderFast<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, InverseModMonomial<UnivariatePolynomial<E>> invMod, bool copy)
        {
            UnivariatePolynomial<E> rem = EarlyRemainderChecks(dividend, divider, copy);
            if (rem != null)
                return rem;
            return DivideAndRemainderFastCorrectLC(dividend, divider, invMod, copy)[1];
        }

        /// <summary>
        /// Returns quotient of {@code dividend} and {@code divider}.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>the quotient</returns>
        public static UnivariatePolynomial<E> Quotient<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, bool copy)
        {
            UnivariatePolynomial<E>[] qd = EarlyDivideAndRemainderChecks(dividend, divider, copy);
            if (qd != null)
                return qd[0];
            if (UseClassicalDivision(dividend, divider))
                return DivideAndRemainderClassic(dividend, divider, copy)[0];
            return DivideAndRemainderFast0(dividend, divider, copy)[0];
        }

        /// <summary>
        /// Fast quotient using Newton's iteration.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="invMod">pre-conditioned divider ({@code fastDivisionPreConditioning(divider)})</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>the quotient</returns>
        public static UnivariatePolynomial<E> QuotientFast<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, InverseModMonomial<UnivariatePolynomial<E>> invMod, bool copy)
        {
            UnivariatePolynomial<E>[] qd = EarlyDivideAndRemainderChecks(dividend, divider, copy);
            if (qd != null)
                return qd[0];
            return DivideAndRemainderFastCorrectLC(dividend, divider, invMod, copy)[0];
        }

        /* ********************************** Common conversion ********************************** */
        /// <summary>
        /// Returns quotient and remainder of {@code dividend} and {@code divider} using pseudo division.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>{quotient, remainder}</returns>
        public static Poly[] PseudoDivideAndRemainder<Poly extends IUnivariatePolynomial<Poly>>(Poly dividend, Poly divider, bool copy)
        {
            if (dividend is UnivariatePolynomialZ64)
                return (Poly[])PseudoDivideAndRemainder((UnivariatePolynomialZ64)dividend, (UnivariatePolynomialZ64)divider, copy);
            if (dividend is UnivariatePolynomialZp64)
                return (Poly[])DivideAndRemainder((UnivariatePolynomialZp64)dividend, (UnivariatePolynomialZp64)divider, copy);
            else if (dividend is UnivariatePolynomial)
            {
                if (dividend.IsOverField())
                    return (Poly[])DivideAndRemainder((UnivariatePolynomial)dividend, (UnivariatePolynomial)divider, copy);
                else
                    return (Poly[])PseudoDivideAndRemainder((UnivariatePolynomial)dividend, (UnivariatePolynomial)divider, copy);
            }
            else
                throw new Exception(dividend.GetType().ToString());
        }

        /// <summary>
        /// Returns {@code {quotient, remainder}} of {@code dividend} and {@code divider} or {@code null} if the division is
        /// not possible.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>{quotient, remainder} or {@code null} if the division is not possible</returns>
        public static Poly[] DivideAndRemainder<Poly extends IUnivariatePolynomial<Poly>>(Poly dividend, Poly divider, bool copy)
        {
            if (dividend is UnivariatePolynomialZ64)
                return (Poly[])DivideAndRemainder((UnivariatePolynomialZ64)dividend, (UnivariatePolynomialZ64)divider, copy);
            else if (dividend is UnivariatePolynomialZp64)
                return (Poly[])DivideAndRemainder((UnivariatePolynomialZp64)dividend, (UnivariatePolynomialZp64)divider, copy);
            else if (dividend is UnivariatePolynomial)
                return (Poly[])DivideAndRemainder((UnivariatePolynomial)dividend, (UnivariatePolynomial)divider, copy);
            else
                throw new Exception(dividend.GetType().ToString());
        }

        /// <summary>
        /// Returns {@code {quotient, remainder}} of {@code dividend} and {@code divider}
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <param name="invMod">precomputed Newton inverses</param>
        /// <returns>{quotient, remainder} or {@code null} if the division is not possible</returns>
        public static Poly[] DivideAndRemainderFast<Poly extends IUnivariatePolynomial<Poly>>(Poly dividend, Poly divider, InverseModMonomial<Poly> invMod, bool copy)
        {
            if (dividend is UnivariatePolynomialZp64)
                return (Poly[])DivideAndRemainderFast((UnivariatePolynomialZp64)dividend, (UnivariatePolynomialZp64)divider, (InverseModMonomial)invMod, copy);
            else if (dividend is UnivariatePolynomial)
                return (Poly[])DivideAndRemainderFast((UnivariatePolynomial)dividend, (UnivariatePolynomial)divider, (InverseModMonomial)invMod, copy);
            else
                throw new Exception(dividend.GetType().ToString());
        }

        /// <summary>
        /// Divides {@code dividend} by {@code divider} or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <returns>{@code dividend / divider}</returns>
        /// <exception cref="ArithmeticException">if exact division is not possible</exception>
        public static Poly DivideExact<Poly extends IUnivariatePolynomial<Poly>>(Poly dividend, Poly divider, bool copy)
        {
            Poly[] qr = DivideAndRemainder(dividend, divider, copy);
            if (qr == null || !qr[1].IsZero())
                throw new ArithmeticException("Not divisible: (" + dividend + ") / (" + divider + ")");
            return qr[0];
        }

        /// <summary>
        /// Divides {@code dividend} by {@code divider} or returns {@code null} if exact division is not possible
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <returns>{@code dividend / divider} or {@code null} if exact division is not possible</returns>
        public static Poly DivideOrNull<Poly extends IUnivariatePolynomial<Poly>>(Poly dividend, Poly divider, bool copy)
        {
            Poly[] qr = DivideAndRemainder(dividend, divider, copy);
            if (qr == null || !qr[1].IsZero())
                return null;
            return qr[0];
        }

        /// <summary>
        /// Returns remainder of {@code dividend} and {@code divider}.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>the remainder</returns>
        public static Poly Remainder<Poly extends IUnivariatePolynomial<Poly>>(Poly dividend, Poly divider, bool copy)
        {
            if (dividend is UnivariatePolynomialZ64)
                return (Poly)Remainder((UnivariatePolynomialZ64)dividend, (UnivariatePolynomialZ64)divider, copy);
            else if (dividend is UnivariatePolynomialZp64)
                return (Poly)Remainder((UnivariatePolynomialZp64)dividend, (UnivariatePolynomialZp64)divider, copy);
            else if (dividend is UnivariatePolynomial)
                return (Poly)Remainder((UnivariatePolynomial)dividend, (UnivariatePolynomial)divider, copy);
            else
                throw new Exception(dividend.GetType().ToString());
        }

        /// <summary>
        /// Returns quotient {@code dividend/ divider} or null if exact division o
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>the quotient</returns>
        public static Poly Quotient<Poly extends IUnivariatePolynomial<Poly>>(Poly dividend, Poly divider, bool copy)
        {
            if (dividend is UnivariatePolynomialZ64)
                return (Poly)Quotient((UnivariatePolynomialZ64)dividend, (UnivariatePolynomialZ64)divider, copy);
            else if (dividend is UnivariatePolynomialZp64)
                return (Poly)Quotient((UnivariatePolynomialZp64)dividend, (UnivariatePolynomialZp64)divider, copy);
            else if (dividend is UnivariatePolynomial)
                return (Poly)Quotient((UnivariatePolynomial)dividend, (UnivariatePolynomial)divider, copy);
            else
                throw new Exception(dividend.GetType().ToString());
        }

        /// <summary>
        /// Fast remainder using Newton's iteration with switch to classical remainder for small polynomials.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <param name="invMod">pre-conditioned divider ({@code fastDivisionPreConditioning(divider)})</param>
        /// <param name="copy">whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
        ///                 dividend} and {@code dividend} data will be lost</param>
        /// <returns>the remainder</returns>
        public static Poly RemainderFast<Poly extends IUnivariatePolynomial<Poly>>(Poly dividend, Poly divider, InverseModMonomial<Poly> invMod, bool copy)
        {
            if (dividend is UnivariatePolynomialZp64)
                return (Poly)RemainderFast((UnivariatePolynomialZp64)dividend, (UnivariatePolynomialZp64)divider, (InverseModMonomial<UnivariatePolynomialZp64>)invMod, copy);
            else if (dividend is UnivariatePolynomial)
                return (Poly)RemainderFast((UnivariatePolynomial)dividend, (UnivariatePolynomial)divider, (InverseModMonomial)invMod, copy);
            else
                throw new Exception(dividend.GetType().ToString());
        }

        /// <summary>
        /// Gives an upper bound on the coefficients of remainder of division of {@code dividend} by {@code divider}
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <returns>upper bound on the coefficients of remainder</returns>
        public static E RemainderCoefficientBound<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider)
        {
            if (divider.degree < dividend.degree)
                return dividend.MaxAbsCoefficient();
            Ring<E> ring = dividend.ring;

            // see e.g. http://www.csd.uwo.ca/~moreno//AM583/Lectures/Newton2Hensel.html/node13.html
            return ring.Multiply(dividend.MaxAbsCoefficient(), ring.Pow(ring.Increment(ring.Quotient(divider.MaxAbsCoefficient(), divider.Lc())), dividend.degree - divider.degree + 1));
        }
    }
}