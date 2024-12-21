namespace Polynomials.Poly.Multivar;

public static class MultivariateDivision
{
    public static MultivariatePolynomial<E>[] DivideAndRemainder<E>(MultivariatePolynomial<E> dividend, params MultivariatePolynomial<E>[] dividers)
    {
        var quotients = new MultivariatePolynomial<E>[dividers.Length + 1];
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

            var dd = dividend.Clone().DivideByLC(dividers[constDivider]);
            if (dd != null)
            {
                quotients[constDivider] = dd;
                return quotients;
            }
        }

        var mAlgebra = dividend.monomialAlgebra;

        // cache leading terms
        var dividersLTs = dividers.Select(p => p.Lt()).ToArray();
        dividend = dividend.Clone();
        var remainder = quotients[quotients.Length - 1];
        while (!dividend.IsZero())
        {
            Monomial<E>? ltDiv = null;
            var lt = dividend.Lt();
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


    public static MultivariatePolynomial<E> Remainder<E>(MultivariatePolynomial<E> dividend, params MultivariatePolynomial<E>[] dividers)
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
            var dd = dividend.Clone().DivideByLC(dividers[constDivider]);
            if (dd != null)
                return dividend.CreateZero();
        }

        var mAlgebra = dividend.monomialAlgebra;

        // cache leading terms
        var dividersLTs = dividers.Select(p => p.Lt()).ToArray();
        dividend = dividend.Clone();
        var remainder = dividend.CreateZero();
        while (!dividend.IsZero())
        {
            Monomial<E>? ltDiv = null;
            var lt = dividend.Lt();
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


    public static MultivariatePolynomial<E> PseudoRemainder<E>(MultivariatePolynomial<E> dividend, params MultivariatePolynomial<E>[] dividers)
    {
        if (dividend.IsOverField())
            return Remainder(dividend, dividers);
        return PseudoRemainder0(dividend, dividers);
    }

    private static MultivariatePolynomial<E> PseudoRemainder0<E>(MultivariatePolynomial<E> dividend,
        params MultivariatePolynomial<E>[] dividers)
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
            Monomial<E>? ltDiv = null;
            Monomial<E> lt = dividend.Lt();
            int iPseudoDiv = -1;
            DegreeVector? dvPseudoDiv = null;
            for (i = 0; i < dividersLTs.Length; ++i)
            {
                DegreeVector? dvDiv = lt.DvDivideOrNull(dividersLTs[i]);
                if (dvDiv == null)
                    continue;
                var cfDiv = ring.DivideOrNull(lt.coefficient, dividersLTs[i].coefficient);
                if (!cfDiv.IsNull)
                {
                    ltDiv = new Monomial<E>(dvDiv, cfDiv.Value);
                    break;
                }
                else if (iPseudoDiv == -1 ||
                         ring.Compare(dividersLTs[i].coefficient, dividersLTs[iPseudoDiv].coefficient) < 0)
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
            dividend = dividend.Subtract(new Monomial<E>(dvPseudoDiv, ring.DivideExact(lt.coefficient, gcd)),
                dividers[iPseudoDiv]);
        }

        return remainder.PrimitivePartSameSign();
    }


    public static MultivariatePolynomial<E>[] DivideAndRemainder<E>(MultivariatePolynomial<E> dividend, MultivariatePolynomial<E> divider)

    {
        MultivariatePolynomial<E>[] array = [divider];
        return DivideAndRemainder(dividend, array);
    }


    public static MultivariatePolynomial<E> Remainder<E>(MultivariatePolynomial<E> dividend, IEnumerable<MultivariatePolynomial<E>> dividers)
    {
        return Remainder(dividend, dividers.ToArray());
    }


    public static MultivariatePolynomial<E> Remainder<E>(MultivariatePolynomial<E> dividend, MultivariatePolynomial<E> divider)
    {
        MultivariatePolynomial<E>[] array = [divider];
        return Remainder(dividend, array);
    }


    public static MultivariatePolynomial<E> PseudoRemainder<E>(MultivariatePolynomial<E> dividend, IEnumerable<MultivariatePolynomial<E>> dividers)
    {
        return PseudoRemainder(dividend, dividers.ToArray());
    }


    public static MultivariatePolynomial<E> PseudoRemainder<E>(MultivariatePolynomial<E> dividend, MultivariatePolynomial<E> divider)
    {
        MultivariatePolynomial<E>[] array = [divider];
        return PseudoRemainder(dividend, array);
    }


    public static MultivariatePolynomial<E> DivideExact<E>(MultivariatePolynomial<E> dividend, MultivariatePolynomial<E> divider)
    {
        var qd = DivideAndRemainder(dividend, divider);
        if (qd == null || !qd[1].IsZero())
            throw new ArithmeticException("not divisible: " + dividend + " / " + divider);
        return qd[0];
    }


    public static MultivariatePolynomial<E>? DivideOrNull<E>(MultivariatePolynomial<E> dividend, MultivariatePolynomial<E> divider)
    {
        var qd = DivideAndRemainder(dividend, divider);
        if (qd == null || !qd[1].IsZero())
            return null;
        return qd[0];
    }


    public static bool DividesQ<E>(MultivariatePolynomial<E> dividend, MultivariatePolynomial<E> divider)
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
        var mAlgebra = dividend.monomialAlgebra;
        while (!dividend.IsZero())
        {
            var ltDiv = mAlgebra.DivideOrNull(dividend.Lt(), divider.Lt());
            if (ltDiv == null)
                return false;
            dividend = dividend.Subtract(divider.Clone().Multiply(ltDiv));
        }

        return true;
    }


    public static bool NontrivialQuotientQ<E>(MultivariatePolynomial<E> dividend, MultivariatePolynomial<E> divider)
    {
        var lt = divider.Lt();
        foreach (var term in dividend.terms)
            if (term.DvDivisibleBy(lt))
                return true;
        return false;
    }
}