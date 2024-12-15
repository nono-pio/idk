
namespace Polynomials.Poly.Univar;

public static class UnivariateDivision
{
    public static UnivariatePolynomial<T> RemainderMonomial<T>(UnivariatePolynomial<T> dividend, int xDegree, bool copy = true)
    {
        return (copy ? dividend.Clone() : dividend).Truncate(xDegree - 1);
    }

    private static void CheckZeroDivider<T>(UnivariatePolynomial<T> p)
    {
        if (p.IsZero())
            throw new ArithmeticException("divide by zero");
    }


    // --------------------------------- Division and remainder ---------------------------------

    private static UnivariatePolynomial<E>[]? EarlyDivideAndRemainderChecks<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, bool copy = true)
    {
        CheckZeroDivider(divider);
        if (dividend.IsZero())
            return [dividend.CreateZero(), dividend.CreateZero()];
        if (dividend.degree < divider.degree)
            return [dividend.CreateZero(), copy ? dividend.Clone() : dividend];
        if (divider.degree == 0)
        {
            UnivariatePolynomial<E>? div = copy ? dividend.Clone() : dividend;
            div = div.DivideOrNull(divider.Lc());
            if (div == null)
                return null;
            return [div, dividend.CreateZero()];
        }

        if (divider.degree == 1)
            return DivideAndRemainderLinearDivider(dividend, divider, copy);
        return null;
    }

    public static UnivariatePolynomial<E>[]? DivideAndRemainder<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, bool copy = true)
    {
        var r = EarlyDivideAndRemainderChecks(dividend, divider, copy);
        if (r != null)
            return r;

        return DivideAndRemainderClassic0(dividend, divider, dividend.ring.GetOne(), copy);
    }

    private static UnivariatePolynomial<E>[]? DivideAndRemainderClassic0<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, E dividendRaiseFactor, bool copy = true)
    {
        Ring<E> ring = dividend.ring;
        UnivariatePolynomial<E> remainder = (copy ? dividend.Clone() : dividend).Multiply(dividendRaiseFactor);
        E[] quotient = new E[dividend.degree - divider.degree + 1];
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
                var quot = ring.DivideOrNull(ring.Multiply(remainder.Lc(), lcMultiplier), lcDivider);
                if (quot.IsNull)
                    return null;
                quotient[i] = quot.Value;
                remainder.Subtract(divider, quotient[i], i);
            }
            else
                quotient[i] = ring.GetZero();
        }

        return [dividend.CreateFromArray(quotient), remainder];
    }


    private static UnivariatePolynomial<E>[]? DivideAndRemainderLinearDivider<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, bool copy = true)
    {
        return DivideAndRemainderLinearDivider0(dividend, divider, dividend.ring.GetOne(), copy);
    }

    private static UnivariatePolynomial<E>[]? DivideAndRemainderLinearDivider0<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, E raiseFactor, bool copy = true)
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

        E[] quotient = copy ? new E[dividend.degree] : dividend.data;
        E res = ring.GetZero();
        for (int i = dividend.degree;; --i)
        {
            E tmp = dividend.data[i];
            if (i != dividend.degree)
                quotient[i] = ring.Copy(res);
            res = ring.AddMutable(ring.MultiplyMutable(res, cc), ring.Multiply(raiseFactor, tmp));
            if (i == 0)
                break;
            var quot = ring.DivideOrNull(ring.Multiply(res, lcMultiplier), lcDivider);
            if (quot.IsNull)
                return null;
            res = quot.Value;
        }

        if (!copy)
            quotient[dividend.degree] = ring.GetZero();
        return [dividend.CreateFromArray(quotient), dividend.CreateConstant(res)];
    }

    // --------------------------------- Pseudo Division And Remainder ---------------------------------

    public static UnivariatePolynomial<E>[]? PseudoDivideAndRemainder<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, bool copy = true)
    {
        CheckZeroDivider(divider);
        if (dividend.IsZero())
            return [dividend.CreateZero(), dividend.CreateZero()];
        if (dividend.degree < divider.degree)
            return [dividend.CreateZero(), copy ? dividend.Clone() : dividend];
        E factor = dividend.ring.Pow(divider.Lc(), dividend.degree - divider.degree + 1);
        if (divider.degree == 0)
            return
            [
                (copy ? dividend.Clone() : dividend).Multiply(dividend.ring.DivideExact(factor, divider.Lc())),
                dividend.CreateZero()
            ];
        if (divider.degree == 1)
            return DivideAndRemainderLinearDivider0(dividend, divider, factor, copy);
        return DivideAndRemainderClassic0(dividend, divider, factor, copy);
    }

    // --------------------------------- Quotient ---------------------------------


    public static UnivariatePolynomial<E>? Quotient<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, bool copy = true)
    {
        return DivideAndRemainder(dividend, divider, copy)?[0];
    }

    public static UnivariatePolynomial<E> DivideExact<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, bool copy = true)
    {
        var qr = DivideAndRemainder(dividend, divider, copy);
        if (qr == null || !qr[1].IsZero())
            throw new ArithmeticException("Not divisible: (" + dividend + ") / (" + divider + ")");
        return qr[0];
    }

    public static UnivariatePolynomial<E>? DivideOrNull<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, bool copy = true)
    {
        var qr = DivideAndRemainder(dividend, divider, copy);
        if (qr == null || !qr[1].IsZero())
            return null;
        return qr[0];
    }

    // Remainder

    private static UnivariatePolynomial<E>? EarlyRemainderChecks<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, bool copy = true)
    {
        if (dividend.degree < divider.degree)
            return (copy ? dividend.Clone() : dividend);
        if (divider.degree == 0)
            return dividend.CreateZero();
        if (divider.degree == 1)
        {
            var p = dividend.ring.DivideOrNull(dividend.ring.Negate(divider.Cc()), divider.Lc());
            if (p.IsNull)
                return null;
            return dividend.CreateConstant(dividend.Evaluate(p.Value));
        }

        return null;
    }

    public static UnivariatePolynomial<E>? Remainder<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, bool copy = true)
    {
        var rem = EarlyRemainderChecks(dividend, divider, copy);
        if (rem != null)
            return rem;


        return RemainderClassical0(dividend, divider, copy);
    }

    private static UnivariatePolynomial<E>? RemainderClassical0<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, bool copy = true)
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
                    var quot = ring.DivideOrNull(remainder.Lc(), divider.Lc());
                    if (quot.IsNull)
                        return null;
                    remainder.Subtract(divider, quot.Value, i);
                }
        }

        return remainder;
    }
}