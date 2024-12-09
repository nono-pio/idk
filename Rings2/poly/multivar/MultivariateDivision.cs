

namespace Cc.Redberry.Rings.Poly.Multivar
{
    /// <summary>
    /// Division with remainder of multivariate polynomials (multivariate reduction).
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class MultivariateDivision
    {
        private MultivariateDivision()
        {
        }

        /// <summary>
        /// Performs multivariate division with remainder. The resulting array of quotients and remainder (last element of
        /// the returned array) satisfies {@code dividend = quotient_1 * divider_1 + quotient_2 * divider_2 + ... + remainder
        /// }.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="dividers">the dividers</param>
        /// <returns>array of quotients and remainder in the last position</returns>
        public static Poly[] DivideAndRemainder<Term, Poly>(Poly dividend, params Poly[] dividers) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            Poly[] quotients = new Poly[dividers.Length + 1];
            int i = 0;
            int constDivider = -1;
            for (; i < dividers.Length; i++)
            {
                if (dividers[i].IsZero())
                    throw new ArithmeticException("divide by zero");
                if (dividers[i].IsConstant())
                    constDivider = i;
                quotients[i] = dividend.CreateZero();
            }

            quotients[i] = dividend.CreateZero();
            if (constDivider != -1)
            {
                if (dividers[constDivider].IsOne())
                {
                    quotients[constDivider] = dividend.Clone();
                    return quotients;
                }

                Poly dd = dividend.Clone().DivideByLC(dividers[constDivider]);
                if (dd != null)
                {
                    quotients[constDivider] = dd;
                    return quotients;
                }
            }

            IMonomialAlgebra<Term> mAlgebra = dividend.monomialAlgebra;

            // cache leading terms
            Term[] dividersLTs = dividers.Select(p => p.Lt()).ToArray();
            dividend = dividend.Clone();
            Poly remainder = quotients[quotients.Length - 1];
            while (!dividend.IsZero())
            {
                Term ltDiv = null;
                Term lt = dividend.Lt();
                for (i = 0; i < dividers.Length; i++)
                {
                    ltDiv = mAlgebra.DivideOrNull(lt, dividersLTs[i]);
                    if (ltDiv != null)
                        break;
                }

                if (ltDiv != null)
                {
                    quotients[i] = quotients[i].Add(ltDiv);
                    dividend = dividend.Subtract(ltDiv, dividers[i]);
                }
                else
                {
                    remainder = remainder.Add(lt);
                    dividend = dividend.SubtractLt();
                }
            }

            return quotients;
        }

        /// <summary>
        /// Performs multivariate division with remainder and returns the remainder.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="dividers">the dividers</param>
        /// <returns>the remainder</returns>
        public static Poly Remainder<Term, Poly>(Poly dividend, params Poly[] dividers) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            int i = 0;
            int constDivider = -1;
            for (; i < dividers.Length; i++)
            {
                if (dividers[i].IsZero())
                    throw new ArithmeticException("divide by zero");
                if (dividers[i].IsConstant())
                    constDivider = i;
            }

            if (constDivider != -1)
            {
                if (dividers[constDivider].IsOne())
                    return dividend.CreateZero();
                Poly dd = dividend.Clone().DivideByLC(dividers[constDivider]);
                if (dd != null)
                    return dividend.CreateZero();
            }

            IMonomialAlgebra<Term> mAlgebra = dividend.monomialAlgebra;

            // cache leading terms
            Term[] dividersLTs = dividers.Select(p => p.Lt()).ToArray();
            dividend = dividend.Clone();
            Poly remainder = dividend.CreateZero();
            while (!dividend.IsZero())
            {
                Term ltDiv = null;
                Term lt = dividend.Lt();
                for (i = 0; i < dividersLTs.Length; ++i)
                {
                    ltDiv = mAlgebra.DivideOrNull(lt, dividersLTs[i]);
                    if (ltDiv != null)
                        break;
                }

                if (ltDiv != null)
                    dividend = dividend.Subtract(ltDiv, dividers[i]);
                else
                {
                    remainder = remainder.Add(lt);
                    dividend = dividend.SubtractLt();
                }
            }

            return remainder;
        }

        /// <summary>
        /// Performs multivariate pseudo division with remainder and returns the remainder.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="dividers">the dividers</param>
        /// <returns>the "pseudo" remainder</returns>
        public static Poly PseudoRemainder<Term, Poly>(Poly dividend, params Poly[] dividers) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            if (dividend.IsOverField())
                return Remainder(dividend, dividers);
            return (Poly)PseudoRemainder0((MultivariatePolynomial)dividend, (MultivariatePolynomial[])dividers);
        }

        private static MultivariatePolynomial<E> PseudoRemainder0<E>(MultivariatePolynomial<E> dividend, params MultivariatePolynomial<E>[] dividers)
        {
            int i = 0;
            int constDivider = -1;
            for (; i < dividers.Length; i++)
            {
                if (dividers[i].IsZero())
                    throw new ArithmeticException("divide by zero");
                if (dividers[i].IsConstant())
                    constDivider = i;
            }

            if (constDivider != -1)
                return dividend.CreateZero();
            Ring<E> ring = dividend.ring;

            // cache leading terms
            Monomial<E>[] dividersLTs = dividers.Select(m => m.Lt()).ToArray();
            dividend = dividend.Clone();
            MultivariatePolynomial<E> remainder = dividend.CreateZero();
            while (!dividend.IsZero())
            {
                Monomial<E> ltDiv = null;
                Monomial<E> lt = dividend.Lt();
                int iPseudoDiv = -1;
                DegreeVector dvPseudoDiv = null;
                for (i = 0; i < dividersLTs.Length; ++i)
                {
                    DegreeVector dvDiv = lt.DvDivideOrNull(dividersLTs[i]);
                    if (dvDiv == null)
                        continue;
                    E cfDiv = ring.DivideOrNull(lt.coefficient, dividersLTs[i].coefficient);
                    if (cfDiv != null)
                    {
                        ltDiv = new Monomial<E>(dvDiv, cfDiv);
                        break;
                    }
                    else if (iPseudoDiv == -1 || ring.Compare(dividersLTs[i].coefficient, dividersLTs[iPseudoDiv].coefficient) < 0)
                    {
                        iPseudoDiv = i;
                        dvPseudoDiv = dvDiv;
                    }
                }

                if (ltDiv != null)
                {
                    dividend = dividend.Subtract(dividend.Create(ltDiv).Multiply(dividers[i]));
                    continue;
                }

                if (iPseudoDiv == -1)
                {
                    remainder = remainder.Add(lt);
                    dividend = dividend.SubtractLt();
                    continue;
                }

                E gcd = ring.Gcd(lt.coefficient, dividersLTs[iPseudoDiv].coefficient);
                E factor = ring.DivideExact(dividersLTs[iPseudoDiv].coefficient, gcd);
                dividend.Multiply(factor);
                remainder.Multiply(factor);
                dividend = dividend.Subtract(new Monomial<E>(dvPseudoDiv, ring.DivideExact(lt.coefficient, gcd)), dividers[iPseudoDiv]);
            }

            return remainder.PrimitivePartSameSign();
        }

        /// <summary>
        /// Performs multivariate division with remainder.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <returns>array of quotient and remainder</returns>
        public static Poly[] DivideAndRemainder<Term, Poly>(Poly dividend, Poly divider) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            Poly[] array = [divider];
            return DivideAndRemainder(dividend, array);
        }

        /// <summary>
        /// Performs multivariate division with remainder and rerurns the remainder.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="dividers">the dividers</param>
        /// <returns>array of quotients and remainder at the last position</returns>
        public static Poly Remainder<Term, Poly>(Poly dividend, IEnumerable<Poly> dividers)
        {
            return Remainder(dividend, dividers.ToArray());
        }

        /// <summary>
        /// Performs multivariate division with remainder and rerurns the remainder.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <returns>array of quotients and remainder at the last position</returns>
        public static Poly Remainder<Term, Poly>(Poly dividend, Poly divider)
        {
            Poly[] array = [divider];
            return Remainder(dividend, array);
        }

        /// <summary>
        /// Performs multivariate division with remainder and rerurns the remainder.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="dividers">the dividers</param>
        /// <returns>array of quotients and remainder at the last position</returns>
        public static Poly PseudoRemainder<Term, Poly>(Poly dividend, IEnumerable<Poly> dividers) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            return PseudoRemainder(dividend, dividers.ToArray());
        }

        /// <summary>
        /// Performs multivariate division with remainder and rerurns the remainder.
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <returns>array of quotients and remainder at the last position</returns>
        public static Poly PseudoRemainder<Term, Poly>(Poly dividend, Poly divider) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            Poly[] array = [divider];
            return PseudoRemainder(dividend, array);
        }

        /// <summary>
        /// Divides {@code dividend} by {@code divider} or throws exception if exact division is not possible
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <returns>{@code dividend / divider}</returns>
        /// <exception cref="ArithmeticException">if exact division is not possible</exception>
        public static Poly DivideExact<Term, Poly>(Poly dividend, Poly divider) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            Poly[] qd = DivideAndRemainder(dividend, divider);
            if (qd == null || !qd[1].IsZero())
                throw new ArithmeticException("not divisible: " + dividend + " / " + divider);
            return qd[0];
        }

        /// <summary>
        /// Divides {@code dividend} by {@code divider} or returns null if exact division is not possible
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <returns>{@code dividend / divider} or null if exact division is not possible</returns>
        public static Poly DivideOrNull<Term, Poly>(Poly dividend, Poly divider) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            Poly[] qd = DivideAndRemainder(dividend, divider);
            if (qd == null || !qd[1].IsZero())
                return null;
            return qd[0];
        }

        /// <summary>
        /// Tests whether {@code divisor} is a divisor of {@code poly}
        /// </summary>
        /// <param name="dividend">the polynomial</param>
        /// <param name="divider">the divisor to check</param>
        /// <returns>whether {@code divisor} is a divisor of {@code poly}</returns>
        public static bool DividesQ<Term, Poly>(Poly dividend, Poly divider) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            if (divider.IsOne())
                return true;
            dividend = dividend.Clone();
            if (divider.IsConstant())
                return dividend.DivideByLC(divider) != null;
            int[] dividendDegrees = dividend.Degrees(), dividerDegrees = divider.Degrees();
            for (int i = 0; i < dividendDegrees.Length; i++)
                if (dividendDegrees[i] < dividerDegrees[i])
                    return false;
            IMonomialAlgebra<Term> mAlgebra = dividend.monomialAlgebra;
            while (!dividend.IsZero())
            {
                Term ltDiv = mAlgebra.DivideOrNull(dividend.Lt(), divider.Lt());
                if (ltDiv == null)
                    return false;
                dividend = dividend.Subtract(divider.Clone().Multiply(ltDiv));
            }

            return true;
        }

        /// <summary>
        /// Tests whether there is nontrivial quotient {@code dividend / divider}
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <returns>whether {@code divisor} is a divisor of {@code poly}</returns>
        public static bool NontrivialQuotientQ<Term, Poly>(Poly dividend, Poly divider) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            Term lt = divider.Lt();
            foreach (Term term in dividend)
                if (term.DvDivisibleBy(lt))
                    return true;
            return false;
        }
    }
}