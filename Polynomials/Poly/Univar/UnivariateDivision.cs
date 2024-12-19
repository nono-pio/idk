namespace Polynomials.Poly.Univar;
using UnivariatePolynomialZp64 = UnivariatePolynomial<long>;

public static class UnivariateDivision
{
    public static UnivariatePolynomial<T> RemainderMonomial<T>(UnivariatePolynomial<T> dividend, int xDegree,
        bool copy = true)
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
            var div = copy ? dividend.Clone() : dividend;
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
        var ring = dividend.ring;
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

        for (var i = dividend.degree - divider.degree; i >= 0; --i)
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
        var ring = dividend.ring;
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
        var res = ring.GetZero();
        for (var i = dividend.degree;; --i)
        {
            var tmp = dividend.data[i];
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
        var factor = dividend.ring.Pow(divider.Lc(), dividend.degree - divider.degree + 1);
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
        var remainder = copy ? dividend.Clone() : dividend;
        Ring<E> ring = dividend.ring;
        if (ring.IsField())
        {
            var lcInverse = ring.Reciprocal(divider.Lc());
            for (var i = dividend.degree - divider.degree; i >= 0; --i)
                if (remainder.degree == divider.degree + i)
                    remainder.Subtract(divider, ring.Multiply(remainder.Lc(), lcInverse), i);
        }
        else
        {
            for (var i = dividend.degree - divider.degree; i >= 0; --i)
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

    #region Fast

    public sealed class InverseModMonomial<E>
    {
        readonly UnivariatePolynomial<E> poly;

        public InverseModMonomial(UnivariatePolynomial<E> poly)
        {
            if (!poly.IsUnitCC())
                throw new ArgumentException("Smallest coefficient is not a unit: " + poly);
            this.poly = poly;
        }


        private readonly List<UnivariatePolynomial<E>> inverses = [];

        static int Log2(int l)
        {
            if (l <= 0)
                throw new ArgumentException();
            return 33 - int.LeadingZeroCount(l - 1);
        }
        public UnivariatePolynomial<E>? GetInverse(int xDegree)
        {
            if (xDegree < 1)
                return null;
            var r = Log2(xDegree);
            if (inverses.Count >= r)
                return inverses[r - 1];
            var currentSize = inverses.Count;
            UnivariatePolynomial<E> gPrev = currentSize == 0 ? poly.CreateOne() : inverses[inverses.Count - 1];
            for (var i = currentSize; i < r; ++i)
            {
                UnivariatePolynomial<E> tmp = gPrev.Clone().Multiply(2).Subtract(gPrev.Clone().Square().Multiply(poly));
                inverses.Add(gPrev = RemainderMonomial(tmp, 1 << i, false));
            }

            return gPrev;
        }
    }


    public static InverseModMonomial<E> FastDivisionPreConditioning<E>(UnivariatePolynomial<E> divider)
    {
        if (!divider.IsMonic())
            throw new ArgumentException("Only monic polynomials allowed. Input: " + divider);
        return new InverseModMonomial<E>(divider.Clone().Reverse());
    }


    public static InverseModMonomial<E> FastDivisionPreConditioningWithLCCorrection<E>(UnivariatePolynomial<E> divider)
    {
        return new InverseModMonomial<E>(divider.Clone().Monic().Reverse());
    }


    public static UnivariatePolynomial<E>[] DivideAndRemainderFast0<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, InverseModMonomial<E> invRevMod,
        bool copy)
    {
        var m = dividend.Degree() - divider.Degree();
        var q = RemainderMonomial(dividend.Clone().Reverse().Multiply(invRevMod.GetInverse(m + 1)), m + 1, false)
            .Reverse();
        if (q.Degree() < m)
            q.ShiftRight(m - q.Degree());
        var r = (copy ? dividend.Clone() : dividend).Subtract(divider.Clone().Multiply(q));
        return [q, r];
    }
    
    static UnivariatePolynomial<E>[] DivideAndRemainderFast0<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, bool copy)
    {
        // if the divider can be directly inverted modulo x^i
        if (divider.IsMonic())
            return DivideAndRemainderFast0(dividend, divider, FastDivisionPreConditioning(divider), copy);
        var lc = divider.Lc();
        var lcInv = dividend.ring.Reciprocal(lc);

        // make the divisor monic
        divider.Multiply(lcInv);

        // perform fast arithmetic with monic divisor
        UnivariatePolynomial<E>[] result =
            DivideAndRemainderFast0(dividend, divider, FastDivisionPreConditioning(divider), copy);

        // reconstruct divisor's lc
        divider.Multiply(lc);

        // reconstruct actual quotient
        result[0].Multiply(lcInv);
        return result;
    }


    /* ********************************* Machine-precision fast division in Zp[x]  ******************************** */

    public static UnivariatePolynomial<E>[] DivideAndRemainderFast<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, bool copy)
    {
        var r = EarlyDivideAndRemainderChecks(dividend, divider, copy);
        if (r != null)
            return r;
        return DivideAndRemainderFast0(dividend, divider, copy);
    }


    public static UnivariatePolynomial<E>[] DivideAndRemainderFast<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, InverseModMonomial<E> invMod, bool copy)
    {
        var r = EarlyDivideAndRemainderChecks(dividend, divider, copy);
        if (r != null)
            return r;
        return DivideAndRemainderFastCorrectLC(dividend, divider, invMod, copy);
    }

    static UnivariatePolynomial<E>[] DivideAndRemainderFastCorrectLC<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, InverseModMonomial<E> invMod, bool copy)
    {
        // if the divider can be directly inverted modulo x^i
        if (divider.IsMonic())
            return DivideAndRemainderFast0(dividend, divider, invMod, copy);
        var lc = divider.Lc();
        var lcInv = dividend.ring.Reciprocal(lc);

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
    
    public static UnivariatePolynomial<E> RemainderFast<E>(UnivariatePolynomial<E> dividend, UnivariatePolynomial<E> divider, InverseModMonomial<E> invMod, bool copy)
    {
        UnivariatePolynomial<E> rem = EarlyRemainderChecks(dividend, divider, copy);
        if (rem != null)
            return rem;
        return DivideAndRemainderFastCorrectLC(dividend, divider, invMod, copy)[1];
    }

    #endregion
}