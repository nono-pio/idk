using System.Diagnostics;

namespace Rings.poly.univar;

static class UnivariateDivision
{
    /* **************************************** Common methods  *************************************** */


    public static T remainderMonomial<T>(T dividend, int xDegree, bool copy) where T : IUnivariatePolynomial<T>
    {
        return (copy ? dividend.clone() : dividend).truncate(xDegree - 1);
    }

    private static void checkZeroDivider<T>(IUnivariatePolynomial<T> p) where T : IUnivariatePolynomial<T>
    {
        if (p.isZero())
            throw new ArithmeticException("divide by zero");
    }

    /* ************************************ Machine-precision division in Z[x]  ************************************ */


    public static UnivariatePolynomialZ64[] divideAndRemainder(UnivariatePolynomialZ64 dividend,
        UnivariatePolynomialZ64 divider,
        bool copy)
    {
        checkZeroDivider(divider);
        if (dividend.isZero())
            return new UnivariatePolynomialZ64[] { UnivariatePolynomialZ64.zero(), UnivariatePolynomialZ64.zero() };
        if (dividend.Degree < divider.Degree)
            return new UnivariatePolynomialZ64[] { UnivariatePolynomialZ64.zero(), copy ? dividend.clone() : dividend };
        if (divider.Degree == 0)
        {
            UnivariatePolynomialZ64 div = copy ? dividend.clone() : dividend;
            div = div.divideOrNull(divider.lc());
            if (div == null) return null;
            return new UnivariatePolynomialZ64[] { div, UnivariatePolynomialZ64.zero() };
        }

        if (divider.Degree == 1)
            return divideAndRemainderLinearDivider(dividend, divider, copy);
        return divideAndRemainderClassic0(dividend, divider, 1, copy);
    }


    public static UnivariatePolynomialZ64[] pseudoDivideAndRemainder(UnivariatePolynomialZ64 dividend,
        UnivariatePolynomialZ64 divider,
        bool copy)
    {
        checkZeroDivider(divider);
        if (dividend.isZero())
            return new UnivariatePolynomialZ64[] { UnivariatePolynomialZ64.zero(), UnivariatePolynomialZ64.zero() };
        if (dividend.Degree < divider.Degree)
            return new UnivariatePolynomialZ64[] { UnivariatePolynomialZ64.zero(), copy ? dividend.clone() : dividend };
        long factor = MachineArithmetic.safePow(divider.lc(), dividend.Degree - divider.Degree + 1);
        if (divider.Degree == 0)
            return new UnivariatePolynomialZ64[]
            {
                (copy ? dividend.clone() : dividend).multiply(factor / divider.lc()), UnivariatePolynomialZ64.zero()
            };
        if (divider.Degree == 1)
            return divideAndRemainderLinearDivider0(dividend, divider, factor, copy);
        return divideAndRemainderClassic0(dividend, divider, factor, copy);
    }


    static UnivariatePolynomialZ64[] divideAndRemainderClassic0(UnivariatePolynomialZ64 dividend,
        UnivariatePolynomialZ64 divider,
        long dividendRaiseFactor,
        bool copy)
    {
        Debug.Assert(dividend.Degree >= divider.Degree);

        if (divider.lc() == 1 && dividendRaiseFactor == 1)
            return divideAndRemainderClassicMonic(dividend, divider, copy);

        UnivariatePolynomialZ64
            remainder = (copy ? dividend.clone() : dividend).multiply(dividendRaiseFactor);
        long[] quotient = new long[dividend.Degree - divider.Degree + 1];


        Magic magic = magicSigned(divider.lc());
        for (int i = dividend.Degree - divider.Degree; i >= 0; --i)
        {
            if (remainder.Degree == divider.Degree + i)
            {
                long quot = divideSignedFast(remainder.lc(), magic);
                if (quot * divider.lc() != remainder.lc())
                    return null;

                quotient[i] = quot;
                remainder.subtract(divider, quotient[i], i);
            }
            else quotient[i] = 0;
        }

        return new UnivariatePolynomialZ64[] { UnivariatePolynomialZ64.create(quotient), remainder };
    }


    private static UnivariatePolynomialZ64[] divideAndRemainderClassicMonic(UnivariatePolynomialZ64 dividend,
        UnivariatePolynomialZ64 divider,
        bool copy)
    {
        Debug.Assert(divider.lc() == 1);

        UnivariatePolynomialZ64
            remainder = (copy ? dividend.clone() : dividend);
        long[] quotient = new long[dividend.Degree - divider.Degree + 1];
        for (int i = dividend.Degree - divider.Degree; i >= 0; --i)
        {
            if (remainder.Degree == divider.Degree + i)
            {
                quotient[i] = remainder.lc();
                remainder.subtract(divider, quotient[i], i);
            }
            else quotient[i] = 0;
        }

        return new UnivariatePolynomialZ64[] { UnivariatePolynomialZ64.create(quotient), remainder };
    }


    static UnivariatePolynomialZ64[] pseudoDivideAndRemainderAdaptive(UnivariatePolynomialZ64 dividend,
        UnivariatePolynomialZ64 divider,
        bool copy)
    {
        checkZeroDivider(divider);
        if (dividend.isZero())
            return new UnivariatePolynomialZ64[] { UnivariatePolynomialZ64.zero(), UnivariatePolynomialZ64.zero() };
        if (dividend.Degree < divider.Degree)
            return new UnivariatePolynomialZ64[] { UnivariatePolynomialZ64.zero(), copy ? dividend.clone() : dividend };
        if (divider.Degree == 0)
            return new UnivariatePolynomialZ64[] { copy ? dividend.clone() : dividend, UnivariatePolynomialZ64.zero() };
        if (divider.Degree == 1)
            return pseudoDivideAndRemainderLinearDividerAdaptive(dividend, divider, copy);
        return pseudoDivideAndRemainderAdaptive0(dividend, divider, copy);
    }


    static UnivariatePolynomialZ64[] pseudoDivideAndRemainderAdaptive0(UnivariatePolynomialZ64 dividend,
        UnivariatePolynomialZ64 divider,
        bool copy)
    {
        Debug.Assert(dividend.Degree >= divider.Degree);


        UnivariatePolynomialZ64 remainder = copy ? dividend.clone() : dividend;
        long[] quotient = new long[dividend.Degree - divider.Degree + 1];

        Magic magic = magicSigned(divider.lc());
        for (int i = dividend.Degree - divider.Degree; i >= 0; --i)
        {
            if (remainder.Degree == divider.Degree + i)
            {
                long quot = divideSignedFast(remainder.lc(), magic);
                if (quot * divider.lc() != remainder.lc())
                {
                    long gcd = MachineArithmetic.gcd(remainder.lc(), divider.lc());
                    long factor = divider.lc() / gcd;
                    remainder.multiply(factor);
                    for (int j = i + 1; j < quotient.Length; ++j)
                        quotient[j] = MachineArithmetic.safeMultiply(quotient[j], factor);
                    quot = divideSignedFast(remainder.lc(), magic);
                }

                quotient[i] = quot;
                remainder.subtract(divider, quotient[i], i);
            }
            else quotient[i] = 0;
        }

        return new UnivariatePolynomialZ64[] { UnivariatePolynomialZ64.create(quotient), remainder };
    }


    static UnivariatePolynomialZ64[] pseudoDivideAndRemainderLinearDividerAdaptive(UnivariatePolynomialZ64 dividend,
        UnivariatePolynomialZ64 divider, bool copy)
    {
        Debug.Assert(divider.Degree == 1);

        //apply Horner's method

        long cc = -divider.cc(), lc = divider.lc(), factor = 1;
        long[] quotient = copy ? new long[dividend.Degree] : dividend.data;
        long res = 0;
        Magic magic = magicSigned(lc);
        for (int i = dividend.Degree;; --i)
        {
            long tmp = dividend.data[i];
            if (i != dividend.Degree)
                quotient[i] = res;
            res = MachineArithmetic.safeAdd(MachineArithmetic.safeMultiply(res, cc),
                MachineArithmetic.safeMultiply(factor, tmp));
            if (i == 0) break;
            long quot = divideSignedFast(res, magic);
            if (quot * lc != res)
            {
                long gcd = MachineArithmetic.gcd(res, lc), f = lc / gcd;
                factor = MachineArithmetic.safeMultiply(factor, f);
                res = MachineArithmetic.safeMultiply(res, f);
                if (i != dividend.Degree)
                    for (int j = quotient.Length - 1; j >= i; --j)
                        quotient[j] = MachineArithmetic.safeMultiply(quotient[j], f);
                quot = divideSignedFast(res, magic);
            }

            res = quot;
        }

        if (!copy) quotient[dividend.Degree] = 0;
        return new UnivariatePolynomialZ64[]
            { UnivariatePolynomialZ64.create(quotient), UnivariatePolynomialZ64.create(res) };
    }


    static UnivariatePolynomialZ64[] divideAndRemainderLinearDivider(UnivariatePolynomialZ64 dividend,
        UnivariatePolynomialZ64 divider, bool copy)
    {
        return divideAndRemainderLinearDivider0(dividend, divider, 1, copy);
    }


    static UnivariatePolynomialZ64[] pseudoDivideAndRemainderLinearDivider(UnivariatePolynomialZ64 dividend,
        UnivariatePolynomialZ64 divider, bool copy)
    {
        return divideAndRemainderLinearDivider0(dividend, divider,
            MachineArithmetic.safePow(divider.lc(), dividend.Degree), copy);
    }


    static UnivariatePolynomialZ64[] divideAndRemainderLinearDivider0(UnivariatePolynomialZ64 dividend,
        UnivariatePolynomialZ64 divider, long raiseFactor, bool copy)
    {
        assert divider.Degree == 1;

        //apply Horner's method

        long cc = -divider.cc(), lc = divider.lc();
        long[] quotient = copy ? new long[dividend.Degree] : dividend.data;
        long res = 0;
        Magic magic = magicSigned(lc);
        for (int i = dividend.Degree;; --i)
        {
            long tmp = dividend.data[i];
            if (i != dividend.Degree)
                quotient[i] = res;
            res = MachineArithmetic.safeAdd(MachineArithmetic.safeMultiply(res, cc),
                MachineArithmetic.safeMultiply(raiseFactor, tmp));
            if (i == 0)
                break;
            long quot = divideSignedFast(res, magic);
            if (quot * lc != res)
                return null;
            res = quot;
        }

        if (!copy) quotient[dividend.Degree] = 0;
        return new UnivariatePolynomialZ64[]
            { UnivariatePolynomialZ64.create(quotient), UnivariatePolynomialZ64.create(res) };
    }


    public static UnivariatePolynomialZ64 remainder(UnivariatePolynomialZ64 dividend,
        UnivariatePolynomialZ64 divider,
        bool copy)
    {
        checkZeroDivider(divider);
        if (dividend.Degree < divider.Degree)
            return dividend;
        if (divider.Degree == 0)
            return UnivariatePolynomialZ64.zero();
        if (divider.Degree == 1)
            if (divider.cc() % divider.lc() == 0)
                return UnivariatePolynomialZ64.create(dividend.evaluate(-divider.cc() / divider.lc()));
        return remainder0(dividend, divider, copy);
    }


    static UnivariatePolynomialZ64 remainder0(UnivariatePolynomialZ64 dividend,
        UnivariatePolynomialZ64 divider,
        bool copy)
    {
        assert dividend.Degree >= divider.Degree;

        UnivariatePolynomialZ64 remainder = copy ? dividend.clone() : dividend;
        Magic magic = magicSigned(divider.lc());
        for (int i = dividend.Degree - divider.Degree; i >= 0; --i)
            if (remainder.Degree == divider.Degree + i)
            {
                long quot = divideSignedFast(remainder.lc(), magic);
                if (quot * divider.lc() != remainder.lc())
                    return null;
                remainder.subtract(divider, quot, i);
            }

        return remainder;
    }

    /**
     * Returns quotient {@code dividend/ divider}
     *
     * @param dividend the dividend
     * @param divider  the divider
     * @param copy     whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
     *                 dividend} and {@code dividend} data will be lost
     * @return the quotient
     */
    public static UnivariatePolynomialZ64 quotient(UnivariatePolynomialZ64 dividend,
        UnivariatePolynomialZ64 divider,
        bool copy)
    {
        UnivariatePolynomialZ64[] qd = divideAndRemainder(dividend, divider, copy);
        if (qd == null)
            return null;

        return qd[0];
    }

    /* ************************************ Machine-precision division in Zp[x]  ************************************ */


    private static bool useClassicalDivision(IUnivariatePolynomial dividend,
        IUnivariatePolynomial divider)
    {
        // practical benchmarks show that without pre-conditioning,
        // classical division is always faster or at least the same fast
        return true;
    }


    private static UnivariatePolynomialZp64[] earlyDivideAndRemainderChecks(UnivariatePolynomialZp64 dividend,
        UnivariatePolynomialZp64 divider,
        bool copy)
    {
        checkZeroDivider(divider);
        if (dividend.isZero())
            return new UnivariatePolynomialZp64[] { dividend.createZero(), dividend.createZero() };
        if (dividend.Degree < divider.Degree)
            return new UnivariatePolynomialZp64[] { dividend.createZero(), copy ? dividend.clone() : dividend };
        if (divider.Degree == 0)
            return new UnivariatePolynomialZp64[]
                { (copy ? dividend.clone() : dividend).divide(divider.lc()), dividend.createZero() };
        if (divider.Degree == 1)
            return divideAndRemainderLinearDividerModulus(dividend, divider, copy);
        return null;
    }


    public static UnivariatePolynomialZp64[] divideAndRemainder(UnivariatePolynomialZp64 dividend,
        UnivariatePolynomialZp64 divider,
        bool copy)
    {
        UnivariatePolynomialZp64[] r = earlyDivideAndRemainderChecks(dividend, divider, copy);
        if (r != null)
            return r;

        if (useClassicalDivision(dividend, divider))
            return divideAndRemainderClassic0(dividend, divider, copy);

        return divideAndRemainderFast0(dividend, divider, copy);
    }


    public static UnivariatePolynomialZp64[] divideAndRemainderClassic(UnivariatePolynomialZp64 dividend,
        UnivariatePolynomialZp64 divider,
        bool copy)
    {
        UnivariatePolynomialZp64[] r = earlyDivideAndRemainderChecks(dividend, divider, copy);
        if (r != null)
            return r;
        return divideAndRemainderClassic0(dividend, divider, copy);
    }


    static UnivariatePolynomialZp64[] divideAndRemainderClassic0(UnivariatePolynomialZp64 dividend,
        UnivariatePolynomialZp64 divider,
        bool copy)
    {
        Debug.Assert(dividend.Degree >= divider.Degree);
        dividend.assertSameCoefficientRingWith(divider);

        UnivariatePolynomialZp64 remainder = copy ? dividend.clone() : dividend;
        long[] quotient = new long[dividend.Degree - divider.Degree + 1];

        long lcInverse = dividend.ring.reciprocal(divider.lc());
        for (int i = dividend.Degree - divider.Degree; i >= 0; --i)
        {
            if (remainder.Degree == divider.Degree + i)
            {
                quotient[i] = remainder.ring.multiply(remainder.lc(), lcInverse);
                remainder.subtract(divider, quotient[i], i);
            }
            else quotient[i] = 0;
        }

        return new UnivariatePolynomialZp64[] { dividend.createFromArray(quotient), remainder };
    }


    static UnivariatePolynomialZp64[] divideAndRemainderLinearDividerModulus(UnivariatePolynomialZp64 dividend,
        UnivariatePolynomialZp64 divider, bool copy)
    {
        Debug.Assert(divider.Degree == 1);
        Debug.Assert(dividend.Degree > 0);
        dividend.assertSameCoefficientRingWith(divider);

        //apply Horner's method

        long cc = dividend.ring.negate(divider.cc());
        long lcInverse = dividend.ring.reciprocal(divider.lc());

        if (divider.lc() != 1)
            cc = dividend.ring.multiply(cc, lcInverse);

        long[] quotient = copy ? new long[dividend.Degree] : dividend.data;
        long res = 0;
        for (int i = dividend.Degree; i >= 0; --i)
        {
            long tmp = dividend.data[i];
            if (i != dividend.Degree)
                quotient[i] = dividend.ring.multiply(res, lcInverse);
            res = dividend.ring.add(dividend.ring.multiply(res, cc), tmp);
        }

        if (!copy) quotient[dividend.Degree] = 0;
        return new UnivariatePolynomialZp64[]
            { dividend.createFromArray(quotient), dividend.createFromArray(new long[] { res }) };
    }

    /* ************************************ Multi-precision division ************************************ */


    private static UnivariatePolynomial<E>[] earlyDivideAndRemainderChecks<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider,
        bool copy)
    {
        checkZeroDivider(divider);
        if (dividend.isZero())
            return [dividend.createZero(), dividend.createZero()];
        if (dividend.Degree < divider.Degree)
            return [dividend.createZero(), copy ? dividend.clone() : dividend];
        if (divider.Degree == 0)
        {
            UnivariatePolynomial<E> div = copy ? dividend.clone() : dividend;
            div = div.divideOrNull(divider.lc());
            if (div == null) return null;
            return [div, dividend.createZero()];
        }

        if (divider.Degree == 1)
            return divideAndRemainderLinearDivider(dividend, divider, copy);
        return null;
    }


    public static UnivariatePolynomial<E>[] divideAndRemainder<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider,
        bool copy)
    {
        UnivariatePolynomial<E>[] r = earlyDivideAndRemainderChecks(dividend, divider, copy);
        if (r != null)
            return r;

        if (useClassicalDivision(dividend, divider))
            return divideAndRemainderClassic0(dividend, divider, dividend.ring.getOne(), copy);

        return divideAndRemainderFast0(dividend, divider, copy);
    }


    public static UnivariatePolynomial<E>[] pseudoDivideAndRemainder<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider,
        bool copy)
    {
        checkZeroDivider(divider);
        if (dividend.isZero())
            return [dividend.createZero(), dividend.createZero()];
        if (dividend.Degree < divider.Degree)
            return dividend.createArray(dividend.createZero(), copy ? dividend.clone() : dividend);
        E factor = dividend.ring.pow(divider.lc(), dividend.Degree - divider.Degree + 1);
        if (divider.Degree == 0)
            return dividend.createArray(
                (copy ? dividend.clone() : dividend).multiply(dividend.ring.divideExact(factor, divider.lc())),
                dividend.createZero());
        if (divider.Degree == 1)
            return divideAndRemainderLinearDivider0(dividend, divider, factor, copy);
        return divideAndRemainderClassic0(dividend, divider, factor, copy);
    }


    public static UnivariatePolynomial<E>[] divideAndRemainderClassic<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider,
        bool copy)
    {
        UnivariatePolynomial<E>[] r = earlyDivideAndRemainderChecks(dividend, divider, copy);
        if (r != null)
            return r;
        return divideAndRemainderClassic0(dividend, divider, dividend.ring.getOne(), copy);
    }


    static UnivariatePolynomial<E>[] divideAndRemainderClassic0<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider,
        E dividendRaiseFactor,
        bool copy)
    {
        assert dividend.Degree >= divider.Degree;

        Ring<E> ring = dividend.ring;
        UnivariatePolynomial<E>
            remainder = (copy ? dividend.clone() : dividend).multiply(dividendRaiseFactor);
        E[] quotient = ring.createArray(dividend.Degree - divider.Degree + 1);

        E lcMultiplier, lcDivider;
        if (ring.isField())
        {
            lcMultiplier = ring.reciprocal(divider.lc());
            lcDivider = ring.getOne();
        }
        else
        {
            lcMultiplier = ring.getOne();
            lcDivider = divider.lc();
        }

        for (int i = dividend.Degree - divider.Degree; i >= 0; --i)
        {
            if (remainder.Degree == divider.Degree + i)
            {
                E quot = ring.divideOrNull(ring.multiply(remainder.lc(), lcMultiplier), lcDivider);
                if (quot == null)
                    return null;

                quotient[i] = quot;
                remainder.subtract(divider, quotient[i], i);
            }
            else quotient[i] = ring.getZero();
        }

        return dividend.createArray(dividend.createFromArray(quotient), remainder);
    }


    static UnivariatePolynomial<E>[] divideAndRemainderLinearDivider<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, bool copy)
    {
        return divideAndRemainderLinearDivider0(dividend, divider, dividend.ring.getOne(), copy);
    }


    static UnivariatePolynomial<E>[] pseudoDivideAndRemainderLinearDivider<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, bool copy)
    {
        return divideAndRemainderLinearDivider0(dividend, divider, dividend.ring.pow(divider.lc(), dividend.Degree),
            copy);
    }


    static UnivariatePolynomial<E>[] divideAndRemainderLinearDivider0<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, E raiseFactor, bool copy)
    {
        assert divider.Degree == 1;
        assert dividend.Degree > 0;
        dividend.assertSameCoefficientRingWith(divider);

        //apply Horner's method

        Ring<E> ring = dividend.ring;
        E cc = ring.negate(divider.cc()), lcDivider, lcMultiplier;
        if (ring.isField())
        {
            lcMultiplier = ring.reciprocal(divider.lc());
            lcDivider = ring.getOne();
        }
        else
        {
            lcMultiplier = ring.getOne();
            lcDivider = divider.lc();
        }

        E[] quotient = copy ? ring.createArray(dividend.Degree) : dividend.data;
        E res = ring.getZero();
        for (int i = dividend.Degree;; --i)
        {
            E tmp = dividend.data[i];
            if (i != dividend.Degree)
                quotient[i] = ring.copy(res);
            res = ring.addMutable(ring.multiplyMutable(res, cc), ring.multiply(raiseFactor, tmp));
            if (i == 0)
                break;
            E quot = ring.divideOrNull(ring.multiply(res, lcMultiplier), lcDivider);
            if (quot == null)
                return null;
            res = quot;
        }

        if (!copy) quotient[dividend.Degree] = ring.getZero();
        return dividend.createArray(dividend.createFromArray(quotient), dividend.createConstant(res));
    }


    static UnivariatePolynomial<E>[] pseudoDivideAndRemainderAdaptive<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider,
        bool copy)
    {
        checkZeroDivider(divider);
        if (dividend.isZero())
            return new UnivariatePolynomial[]
                { UnivariatePolynomial.zero(dividend.ring), UnivariatePolynomial.zero(dividend.ring) };
        if (dividend.Degree < divider.Degree)
            return new UnivariatePolynomial[]
                { UnivariatePolynomial.zero(dividend.ring), copy ? dividend.clone() : dividend };
        if (divider.Degree == 0)
            return new UnivariatePolynomial[]
                { copy ? dividend.clone() : dividend, UnivariatePolynomial.zero(dividend.ring) };
        if (divider.Degree == 1)
            return pseudoDivideAndRemainderLinearDividerAdaptive(dividend, divider, copy);
        return pseudoDivideAndRemainderAdaptive0(dividend, divider, copy);
    }


    static UnivariatePolynomial<E>[] pseudoDivideAndRemainderAdaptive0<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider,
        bool copy)
    {
        assert dividend.Degree >= divider.Degree;

        Ring<E> ring = dividend.ring;
        UnivariatePolynomial<E> remainder = copy ? dividend.clone() : dividend;
        E[] quotient = ring.createArray(dividend.Degree - divider.Degree + 1);

        E dlc = divider.lc();
        for (int i = dividend.Degree - divider.Degree; i >= 0; --i)
        {
            if (remainder.Degree == divider.Degree + i)
            {
                E quot = ring.divideOrNull(remainder.lc(), dlc);
                if (quot == null)
                {
                    E gcd = ring.gcd(remainder.lc(), divider.lc());
                    E factor = ring.divideExact(divider.lc(), gcd);
                    remainder.multiply(factor);
                    for (int j = i + 1; j < quotient.Length; ++j)
                        quotient[j] = ring.multiply(quotient[j], factor);
                    quot = ring.divideExact(remainder.lc(), dlc);
                }

                quotient[i] = quot;
                remainder.subtract(divider, quotient[i], i);
            }
            else quotient[i] = ring.getZero();
        }

        return new UnivariatePolynomial[] { UnivariatePolynomial.create(ring, quotient), remainder };
    }


    static UnivariatePolynomial<E>[] pseudoDivideAndRemainderLinearDividerAdaptive<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, bool copy)
    {
        assert divider.Degree == 1;

        //apply Horner's method
        Ring<E> ring = dividend.ring;
        E cc = ring.negate(divider.cc()), lc = divider.lc(), factor = ring.getOne();
        E[] quotient = copy ? ring.createArray(dividend.Degree) : dividend.data;
        E res = ring.getZero();
        for (int i = dividend.Degree();; --i)
        {
            E tmp = dividend.data[i];
            if (i != dividend.Degree())
                quotient[i] = res;
            res = ring.add(ring.multiply(res, cc), ring.multiply(factor, tmp));
            if (i == 0) break;
            E quot = ring.divideOrNull(res, lc);
            if (quot == null)
            {
                E gcd = ring.gcd(res, lc), f = ring.divideExact(lc, gcd);
                factor = ring.multiply(factor, f);
                res = ring.multiply(res, f);
                if (i != dividend.Degree)
                    for (int j = quotient.Length - 1; j >= i; --j)
                        quotient[j] = ring.multiply(quotient[j], f);
                quot = ring.divideExact(res, lc);
            }

            res = quot;
        }

        if (!copy) quotient[dividend.Degree] = ring.getZero();
        return new UnivariatePolynomial[]
            { UnivariatePolynomial.create(ring, quotient), UnivariatePolynomial.create(ring, res) };
    }


    static UnivariatePolynomial<E> pseudoRemainderAdaptive<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider,
        bool copy)
    {
        checkZeroDivider(divider);
        if (dividend.isZero())
            return UnivariatePolynomial.zero(dividend.ring);
        if (dividend.Degree < divider.Degree)
            return copy ? dividend.clone() : dividend;
        if (divider.Degree == 0)
            return UnivariatePolynomial.zero(dividend.ring);
        if (divider.Degree == 1)
            return pseudoRemainderLinearDividerAdaptive(dividend, divider, copy);
        return pseudoRemainderAdaptive0(dividend, divider, copy);
    }


    static UnivariatePolynomial<E> pseudoRemainderAdaptive0<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider,
        bool copy)
    {
        assert dividend.Degree >= divider.Degree;

        Ring<E> ring = dividend.ring;
        UnivariatePolynomial<E> remainder = copy ? dividend.clone() : dividend;

        E dlc = divider.lc();
        for (int i = dividend.Degree - divider.Degree; i >= 0; --i)
        {
            if (remainder.Degree == divider.Degree + i)
            {
                E quot = ring.divideOrNull(remainder.lc(), dlc);
                if (quot == null)
                {
                    E gcd = ring.gcd(remainder.lc(), divider.lc());
                    E factor = ring.divideExact(divider.lc(), gcd);
                    remainder.multiply(factor);
                    quot = ring.divideExact(remainder.lc(), dlc);
                }

                remainder.subtract(divider, quot, i);
            }
        }

        return remainder;
    }


    static UnivariatePolynomial<E> pseudoRemainderLinearDividerAdaptive<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider, bool copy)
    {
        assert divider.Degree == 1;

        //apply Horner's method
        Ring<E> ring = dividend.ring;
        E cc = ring.negate(divider.cc()), lc = divider.lc(), factor = ring.getOne();
        E res = ring.getZero();
        for (int i = dividend.Degree;; --i)
        {
            E tmp = dividend.data[i];
            res = ring.add(ring.multiply(res, cc), ring.multiply(factor, tmp));
            if (i == 0) break;
            E quot = ring.divideOrNull(res, lc);
            if (quot == null)
            {
                E gcd = ring.gcd(res, lc), f = ring.divideExact(lc, gcd);
                factor = ring.multiply(factor, f);
                res = ring.multiply(res, f);
                quot = ring.divideExact(res, lc);
            }

            res = quot;
        }

        return UnivariatePolynomial.create(ring, res);
    }

    /* ********************************** Fast division algorithm ********************************** */

    /* that is [log2] */
    static int log2(int l)
    {
        if (l <= 0)
            throw new ArgumentException();
        return 33 - Integer.numberOfLeadingZeros(l - 1);
    }


    public class InverseModMonomial<Poly> where Poly : IUnivariatePolynomial<Poly>
    {
        private static long serialVersionUID = 1L;
        Poly poly;

        public InverseModMonomial(Poly poly)
        {
            if (!poly.isUnitCC())
                throw new ArgumentException("Smallest coefficient is not a unit: " + poly);
            this.poly = poly;
        }


        private List<Poly> inverses = new List<Poly>();


        public Poly getInverse(int xDegree)
        {
            if (xDegree < 1)
                return null;
            int r = log2(xDegree);
            if (inverses.Count >= r)
                return inverses[r - 1];
            int currentSize = inverses.Count;
            Poly gPrev = currentSize == 0 ? poly.createOne() : inverses[inverses.Count - 1];
            for (int i = currentSize; i < r; ++i)
            {
                Poly tmp = gPrev.clone().multiply(2).subtract(gPrev.clone().square().multiply(poly));
                inverses.Add(gPrev = remainderMonomial(tmp, 1 << i, false));
            }

            return gPrev;
        }
    }

    public static InverseModMonomial<Poly>
        fastDivisionPreConditioning<Poly>(Poly divider) where Poly : IUnivariatePolynomial<Poly>
    {
        if (!divider.isMonic())
            throw new ArgumentException("Only monic polynomials allowed. Input: " + divider);
        return new InverseModMonomial<Poly>(divider.clone().reverse());
    }

    public static InverseModMonomial<Poly>
        fastDivisionPreConditioningWithLCCorrection<Poly>(Poly divider) where Poly : IUnivariatePolynomial<Poly>
    {
        return new InverseModMonomial<>(divider.clone().monic().reverse());
    }

    public static Poly[] divideAndRemainderFast0<Poly>(Poly dividend, Poly divider,
        InverseModMonomial<Poly> invRevMod,
        bool copy) where Poly : IUnivariatePolynomial<Poly>
    {
        int m = dividend.degree() - divider.degree();
        Poly q = remainderMonomial(dividend.clone().reverse().multiply(invRevMod.getInverse(m + 1)), m + 1, false)
            .reverse();
        if (q.degree() < m)
            q.shiftRight(m - q.degree());
        Poly r = (copy ? dividend.clone() : dividend).subtract(divider.clone().multiply(q));
        return [q, r];
    }

/* ********************************* Machine-precision fast division in Zp[x]  ******************************** */


    public static UnivariatePolynomialZp64[] divideAndRemainderFast(UnivariatePolynomialZp64 dividend,
        UnivariatePolynomialZp64 divider,
        bool copy)
    {
        UnivariatePolynomialZp64[] r = earlyDivideAndRemainderChecks(dividend, divider, copy);
        if (r != null)
            return r;
        return divideAndRemainderFast0(dividend, divider, copy);
    }


    public static UnivariatePolynomialZp64[] divideAndRemainderFast(UnivariatePolynomialZp64 dividend,
        UnivariatePolynomialZp64 divider,
        InverseModMonomial<UnivariatePolynomialZp64> invMod,
        bool copy)
    {
        UnivariatePolynomialZp64[] r = earlyDivideAndRemainderChecks(dividend, divider, copy);
        if (r != null)
            return r;
        return divideAndRemainderFastCorrectLC(dividend, divider, invMod, copy);
    }

    static UnivariatePolynomialZp64[] divideAndRemainderFastCorrectLC(UnivariatePolynomialZp64 dividend,
        UnivariatePolynomialZp64 divider,
        InverseModMonomial<UnivariatePolynomialZp64> invMod,
        bool copy)
    {
        // if the divider can be directly inverted modulo x^i
        if (divider.isMonic())
            return divideAndRemainderFast0(dividend, divider, invMod, copy);

        long lc = divider.lc();
        long lcInv = divider.ring.reciprocal(lc);
        // make the divisor monic
        divider.multiply(lcInv);
        // perform fast arithmetic with monic divisor
        UnivariatePolynomialZp64[] result = divideAndRemainderFast0(dividend, divider, invMod, copy);
        // reconstruct divisor's lc
        divider.multiply(lc);
        // reconstruct actual quotient
        result[0].multiply(lcInv);
        return result;
    }

    static UnivariatePolynomialZp64[] divideAndRemainderFast0(UnivariatePolynomialZp64 dividend,
        UnivariatePolynomialZp64 divider,
        bool copy)
    {
        // if the divider can be directly inverted modulo x^i
        if (divider.isMonic())
            return divideAndRemainderFast0(dividend, divider, fastDivisionPreConditioning(divider), copy);

        long lc = divider.lc();
        long lcInv = divider.ring.reciprocal(lc);
        // make the divisor monic
        divider.multiply(lcInv);
        // perform fast arithmetic with monic divisor
        UnivariatePolynomialZp64[] result =
            divideAndRemainderFast0(dividend, divider, fastDivisionPreConditioning(divider), copy);
        // reconstruct divisor's lc
        divider.multiply(lc);
        // reconstruct actual quotient
        result[0].multiply(lcInv);
        return result;
    }

/* ********************************* Multi-precision fast division in Zp[x]  ******************************** */
    public static UnivariatePolynomial<E>[] divideAndRemainderFast<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider,
        bool copy)
    {
        UnivariatePolynomial<E>[] r = earlyDivideAndRemainderChecks(dividend, divider, copy);
        if (r != null)
            return r;
        return divideAndRemainderFast0(dividend, divider, copy);
    }

    public static UnivariatePolynomial<E>[] divideAndRemainderFast<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider,
        InverseModMonomial<UnivariatePolynomial<E>> invMod,
        bool copy)
    {
        UnivariatePolynomial<E>[] r = earlyDivideAndRemainderChecks(dividend, divider, copy);
        if (r != null)
            return r;
        return divideAndRemainderFastCorrectLC(dividend, divider, invMod, copy);
    }

    static UnivariatePolynomial<E>[] divideAndRemainderFastCorrectLC<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider,
        InverseModMonomial<UnivariatePolynomial<E>> invMod,
        bool copy)
    {
        // if the divider can be directly inverted modulo x^i
        if (divider.isMonic())
            return divideAndRemainderFast0(dividend, divider, invMod, copy);

        E lc = divider.lc();
        E lcInv = dividend.ring.reciprocal(lc);
        // make the divisor monic
        divider.multiply(lcInv);
        // perform fast arithmetic with monic divisor
        UnivariatePolynomial<E>[] result = divideAndRemainderFast0(dividend, divider, invMod, copy);
        // reconstruct divisor's lc
        divider.multiply(lc);
        // reconstruct actual quotient
        result[0].multiply(lcInv);
        return result;
    }

    static UnivariatePolynomial<E>[] divideAndRemainderFast0<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider,
        bool copy)
    {
        // if the divider can be directly inverted modulo x^i
        if (divider.isMonic())
            return divideAndRemainderFast0(dividend, divider, fastDivisionPreConditioning(divider), copy);

        E lc = divider.lc();
        E lcInv = dividend.ring.reciprocal(lc);
        // make the divisor monic
        divider.multiply(lcInv);
        // perform fast arithmetic with monic divisor
        UnivariatePolynomial<E>[] result =
            divideAndRemainderFast0(dividend, divider, fastDivisionPreConditioning(divider), copy);
        // reconstruct divisor's lc
        divider.multiply(lc);
        // reconstruct actual quotient
        result[0].multiply(lcInv);
        return result;
    }

/* ********************************* Machine-precision remainders ******************************** */


    private static UnivariatePolynomialZp64 earlyRemainderChecks(UnivariatePolynomialZp64
            dividend,
        UnivariatePolynomialZp64 divider,
        bool copy)
    {
        if (dividend.Degree < divider.Degree)
            return (copy ? dividend.clone() : dividend);
        if (divider.Degree == 0)
            return dividend.createZero();
        if (divider.Degree == 1)
        {
            IntegersZp64 ring = dividend.ring;
            return dividend.createFromArray(new long[]
            {
                dividend.evaluate(
                    ring.multiply(ring.negate(divider.cc()), ring.reciprocal(divider.lc())))
            });
        }

        return null;
    }


    public static UnivariatePolynomialZp64 remainder(UnivariatePolynomialZp64
            dividend,
        UnivariatePolynomialZp64 divider,
        bool copy)
    {
        UnivariatePolynomialZp64 rem = earlyRemainderChecks(dividend, divider, copy);
        if (rem != null)
            return rem;

        if (useClassicalDivision(dividend, divider))
            return remainderClassical0(dividend, divider, copy);

        return divideAndRemainderFast0(dividend, divider, copy)[1];
    }


    static UnivariatePolynomialZp64 remainderClassical0(UnivariatePolynomialZp64
            dividend,
        UnivariatePolynomialZp64 divider,
        bool copy)
    {
        assert dividend.Degree >= divider.Degree;
        dividend.assertSameCoefficientRingWith(divider);

        UnivariatePolynomialZp64 remainder = copy ? dividend.clone() : dividend;
        long lcInverse = dividend.ring.reciprocal(divider.lc());
        for (int i = dividend.Degree - divider.Degree; i >= 0; --i)
            if (remainder.Degree == divider.Degree + i)
                remainder.subtract(divider, remainder.ring.multiply(remainder.lc(), lcInverse), i);

        return remainder;
    }


    public static UnivariatePolynomialZp64 remainderFast(UnivariatePolynomialZp64
            dividend,
        UnivariatePolynomialZp64 divider,
        InverseModMonomial<UnivariatePolynomialZp64> invMod,
        bool copy)
    {
        UnivariatePolynomialZp64 rem = earlyRemainderChecks(dividend, divider, copy);
        if (rem != null)
            return rem;

        return divideAndRemainderFastCorrectLC(dividend, divider, invMod, copy)[1];
    }


    public static UnivariatePolynomialZp64 quotient(UnivariatePolynomialZp64
            dividend,
        UnivariatePolynomialZp64 divider,
        bool copy)
    {
        UnivariatePolynomialZp64[] qd = earlyDivideAndRemainderChecks(dividend, divider, copy);
        if (qd != null)
            return qd[0];

        if (useClassicalDivision(dividend, divider))
            return divideAndRemainderClassic(dividend, divider, copy)[0];

        return divideAndRemainderFast0(dividend, divider, copy)[0];
    }


    public static UnivariatePolynomialZp64 quotientFast(UnivariatePolynomialZp64
            dividend,
        UnivariatePolynomialZp64 divider,
        InverseModMonomial<UnivariatePolynomialZp64> invMod,
        bool copy)
    {
        UnivariatePolynomialZp64[] qd = earlyDivideAndRemainderChecks(dividend, divider, copy);
        if (qd != null)
            return qd[0];

        return divideAndRemainderFastCorrectLC(dividend, divider, invMod, copy)[0];
    }

/* ********************************* Multi-precision remainders ******************************** */
    private static UnivariatePolynomial<E> earlyRemainderChecks<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider,
        bool copy)
    {
        if (dividend.Degree < divider.Degree)
            return (copy ? dividend.clone() : dividend);
        if (divider.Degree == 0)
            return dividend.createZero();
        if (divider.Degree == 1)
        {
            util.Nullable<E> p = dividend.ring.divideOrNull(dividend.ring.negate(divider.cc()), divider.lc());
            if (p.IsNull)
                return null;
            return dividend.createConstant(dividend.evaluate(p.Value));
        }

        return null;
    }

    public static UnivariatePolynomial<E> remainder<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider,
        bool copy)
    {
        UnivariatePolynomial<E> rem = earlyRemainderChecks(dividend, divider, copy);
        if (rem != null)
            return rem;

        if (useClassicalDivision(dividend, divider))
            return remainderClassical0(dividend, divider, copy);

        return divideAndRemainderFast0(dividend, divider, copy)[1];
    }

    static UnivariatePolynomial<E> remainderClassical0<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider,
        bool copy)
    {
        assert dividend.Degree >= divider.Degree;
        dividend.assertSameCoefficientRingWith(divider);

        UnivariatePolynomial<E> remainder = copy ? dividend.clone() : dividend;
        Ring<E> ring = dividend.ring;
        if (ring.isField())
        {
            E lcInverse = ring.reciprocal(divider.lc());
            for (int i = dividend.Degree - divider.Degree; i >= 0; --i)
                if (remainder.Degree == divider.Degree + i)
                    remainder.subtract(divider, ring.multiply(remainder.lc(), lcInverse), i);
        }
        else
        {
            for (int i = dividend.Degree - divider.Degree; i >= 0; --i)
                if (remainder.Degree == divider.Degree + i)
                {
                    E quot = ring.divideOrNull(remainder.lc(), divider.lc());
                    if (quot == null)
                        return null;
                    remainder.subtract(divider, quot, i);
                }
        }

        return remainder;
    }

    public static UnivariatePolynomial<E> remainderFast<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider,
        InverseModMonomial<UnivariatePolynomial<E>> invMod,
        bool copy)
    {
        UnivariatePolynomial<E> rem = earlyRemainderChecks(dividend, divider, copy);
        if (rem != null)
            return rem;

        return divideAndRemainderFastCorrectLC(dividend, divider, invMod, copy)[1];
    }

    public static UnivariatePolynomial<E> quotient<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider,
        bool copy)
    {
        UnivariatePolynomial<E>[] qd = earlyDivideAndRemainderChecks(dividend, divider, copy);
        if (qd != null)
            return qd[0];

        if (useClassicalDivision(dividend, divider))
            return divideAndRemainderClassic(dividend, divider, copy)[0];

        return divideAndRemainderFast0(dividend, divider, copy)[0];
    }

    public static UnivariatePolynomial<E> quotientFast<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider,
        InverseModMonomial<UnivariatePolynomial<E>> invMod,
        bool copy)
    {
        UnivariatePolynomial<E>[] qd = earlyDivideAndRemainderChecks(dividend, divider, copy);
        if (qd != null)
            return qd[0];

        return divideAndRemainderFastCorrectLC(dividend, divider, invMod, copy)[0];
    }


/* ********************************** Common conversion ********************************** */


    public static Poly[] pseudoDivideAndRemainder<Poly>(Poly dividend,
        Poly divider,
        bool copy) where Poly : IUnivariatePolynomial<Poly>
    {
        if (dividend is UnivariatePolynomialZ64)
            return (Poly[])pseudoDivideAndRemainder((UnivariatePolynomialZ64)dividend, (UnivariatePolynomialZ64)divider,
                copy);
        if (dividend is UnivariatePolynomialZp64)
            return (Poly[])divideAndRemainder((UnivariatePolynomialZp64)dividend, (UnivariatePolynomialZp64)divider,
                copy);
        else if (dividend is UnivariatePolynomial)
        {
            if (dividend.isOverField())
                return (Poly[])divideAndRemainder((UnivariatePolynomial)dividend, (UnivariatePolynomial)divider, copy);
            else
                return (Poly[])pseudoDivideAndRemainder((UnivariatePolynomial)dividend, (UnivariatePolynomial)divider,
                    copy);
        }
        else
            throw new Exception(dividend.GetType().ToString());
    }


    public static Poly[] divideAndRemainder<Poly>(Poly dividend, Poly divider,
        bool copy) where Poly : IUnivariatePolynomial<Poly>
    {
        if (dividend is UnivariatePolynomialZ64)
            return (Poly[])divideAndRemainder((UnivariatePolynomialZ64)dividend, (UnivariatePolynomialZ64)divider,
                copy);
        else if (dividend is UnivariatePolynomialZp64)
            return (Poly[])divideAndRemainder((UnivariatePolynomialZp64)dividend, (UnivariatePolynomialZp64)divider,
                copy);
        else if (dividend is UnivariatePolynomial)
            return (Poly[])divideAndRemainder((UnivariatePolynomial)dividend, (UnivariatePolynomial)divider, copy);
        else
            throw new Exception(dividend.GetType().ToString());
    }


    public static Poly[] divideAndRemainderFast<Poly>(Poly dividend, Poly divider,
        InverseModMonomial<Poly> invMod, bool copy) where Poly : IUnivariatePolynomial<Poly>
    {
        if (dividend is UnivariatePolynomialZp64)
            return (Poly[])divideAndRemainderFast((UnivariatePolynomialZp64)dividend, (UnivariatePolynomialZp64)divider,
                (InverseModMonomial)invMod, copy);
        else if (dividend is UnivariatePolynomial)
            return (Poly[])divideAndRemainderFast((UnivariatePolynomial)dividend, (UnivariatePolynomial)divider,
                (InverseModMonomial)invMod, copy);
        else
            throw new Exception(dividend.GetType().ToString());
    }

    /**
     * Divides {@code dividend} by {@code divider} or throws {@code ArithmeticException} if exact division is not
     * possible
     *
     * @param dividend the dividend
     * @param divider  the divider
     * @return {@code dividend / divider}
     * @throws ArithmeticException if exact division is not possible
     */
    public static Poly divideExact<Poly>(Poly dividend, Poly divider, bool copy)
        where Poly : IUnivariatePolynomial<Poly>
    {
        Poly[] qr = divideAndRemainder(dividend, divider, copy);
        if (qr == null || !qr[1].isZero())
            throw new ArithmeticException("Not divisible: (" + dividend + ") / (" + divider + ")");
        return qr[0];
    }

    /**
     * Divides {@code dividend} by {@code divider} or returns {@code null} if exact division is not possible
     *
     * @param dividend the dividend
     * @param divider  the divider
     * @return {@code dividend / divider} or {@code null} if exact division is not possible
     */
    public static Poly divideOrNull<Poly>(Poly dividend, Poly divider, bool copy)
        where Poly : IUnivariatePolynomial<Poly>
    {
        Poly[] qr = divideAndRemainder(dividend, divider, copy);
        if (qr == null || !qr[1].isZero())
            return null;
        return qr[0];
    }


    public static Poly remainder<Poly>(Poly dividend, Poly divider, bool copy) where Poly : IUnivariatePolynomial<Poly>
    {
        if (dividend is UnivariatePolynomialZ64)
            return (Poly)remainder((UnivariatePolynomialZ64)dividend, (UnivariatePolynomialZ64)divider, copy);
        else if (dividend is UnivariatePolynomialZp64)
            return (Poly)remainder((UnivariatePolynomialZp64)dividend, (UnivariatePolynomialZp64)divider, copy);
        else if (dividend is UnivariatePolynomial)
            return (Poly)remainder((UnivariatePolynomial)dividend, (UnivariatePolynomial)divider, copy);
        else
            throw new Exception(dividend.GetType().ToString());
    }

    /**
     * Returns quotient {@code dividend/ divider} or null if exact division o
     *
     * @param dividend the dividend
     * @param divider  the divider
     * @param copy     whether to clone {@code dividend}; if not, the remainder will be placed directly to {@code
     *                 dividend} and {@code dividend} data will be lost
     * @return the quotient
     */
    public static Poly quotient<Poly>(Poly dividend, Poly divider, bool copy) where Poly : IUnivariatePolynomial<Poly>
    {
        if (dividend is UnivariatePolynomialZ64)
            return (Poly)quotient((UnivariatePolynomialZ64)dividend, (UnivariatePolynomialZ64)divider, copy);
        else if (dividend is UnivariatePolynomialZp64)
            return (Poly)quotient((UnivariatePolynomialZp64)dividend, (UnivariatePolynomialZp64)divider, copy);
        else if (dividend is UnivariatePolynomial)
            return (Poly)quotient((UnivariatePolynomial)dividend, (UnivariatePolynomial)divider, copy);
        else
            throw new Exception(dividend.GetType().ToString());
    }


    public static Poly remainderFast<Poly>(Poly dividend,
        Poly divider,
        InverseModMonomial<Poly> invMod,
        bool copy) where Poly : IUnivariatePolynomial<Poly>
    {
        if (dividend is UnivariatePolynomialZp64)
            return (Poly)remainderFast((UnivariatePolynomialZp64)dividend, (UnivariatePolynomialZp64)divider,
                (InverseModMonomial<UnivariatePolynomialZp64>)invMod, copy);
        else if (dividend is UnivariatePolynomial)
            return (Poly)remainderFast((UnivariatePolynomial)dividend, (UnivariatePolynomial)divider,
                (InverseModMonomial)invMod, copy);
        else
            throw new Exception(dividend.GetType().ToString());
    }

    public static E remainderCoefficientBound<E>(UnivariatePolynomial<E> dividend,
        UnivariatePolynomial<E> divider)
    {
        if (divider.Degree < dividend.Degree)
            return dividend.maxAbsCoefficient();
        Ring<E> ring = dividend.ring;
        // see e.g. http://www.csd.uwo.ca/~moreno//AM583/Lectures/Newton2Hensel.html/node13.html
        return ring.multiply(dividend.maxAbsCoefficient(),
            ring.pow(ring.increment(ring.quotient(divider.maxAbsCoefficient(), divider.lc())),
                dividend.Degree - divider.Degree + 1));
    }
}