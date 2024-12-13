using System.Numerics;
using Polynomials.Utils;

namespace Polynomials;

public abstract class Ring<E> : IComparer<E>
{
    private BigInteger?[]? perfectPowerDecomposition;

    public BigInteger?[] PerfectPowerDecomposition
    {
        get
        {
            if (perfectPowerDecomposition is null)
            {
                var cardinality = Cardinality();
                if (cardinality is null)
                {
                    perfectPowerDecomposition = [null, null];
                }
                else
                {
                    var ipp = BigIntegerUtils.PerfectPowerDecomposition(cardinality.Value);
                    if (ipp is null)
                    {
                        perfectPowerDecomposition = [cardinality, BigInteger.One];
                    }
                    else
                    {
                        perfectPowerDecomposition = [ipp[0], ipp[1]];
                    }
                }
            }

            return perfectPowerDecomposition;
        }
    }


    public abstract bool IsField();
    public abstract bool IsEuclideanRing();

    public bool IsFinite()
    {
        return Cardinality() is not null;
    }

    public bool IsFiniteField()
    {
        return IsField() && IsFinite();
    }

    public abstract BigInteger? Cardinality();
    public abstract BigInteger Characteristic();

    public bool IsPerfectPower()
    {
        var p = PerfectPowerDecomposition[1];
        return p is not null && !p.Value.IsOne;
    }

    public BigInteger? PerfectPowerBase() => PerfectPowerDecomposition[0];
    public BigInteger? PerfectPowerExponent() => PerfectPowerDecomposition[1];

    public abstract E Add(E a, E b);

    public E Add(params E[] elements)
    {
        var r = elements[0];
        for (var i = 1; i < elements.Length; i++)
            r = Add(r, elements[i]);
        return r;
    }

    public E Increment(E element)
    {
        return Add(element, GetOne());
    }

    public E Decrement(E element)
    {
        return Subtract(element, GetOne());
    }

    public abstract E Subtract(E a, E b);
    public abstract E Multiply(E a, E b);

    public E Multiply(E a, long b)
    {
        return Multiply(a, ValueOf(b));
    }

    public E Multiply(params E[] elements)
    {
        var r = elements[0];
        for (var i = 1; i < elements.Length; i++)
            r = Multiply(r, elements[i]);
        return r;
    }

    public E Multiply(IEnumerable<E> elements)
    {
        var r = GetOne();
        foreach (var e in elements)
            r = MultiplyMutable(r, e);
        return r;
    }

    public abstract E Negate(E element);

    public E AddMutable(E a, E b)
    {
        return Add(a, b);
    }

    public E SubtractMutable(E a, E b)
    {
        return Subtract(a, b);
    }

    public E MultiplyMutable(E a, E b)
    {
        return Multiply(a, b);
    }

    public E NegateMutable(E element)
    {
        return Negate(element);
    }

    public abstract E Copy(E element);
    public abstract bool Equal(E x, E y);
    public abstract int Compare(E x, E y);
    public abstract object Clone();

    public virtual int Signum(E element)
    {
        return Compare(element, GetZero()).CompareTo(0);
    }

    public virtual E Abs(E el)
    {
        return Signum(el) < 0 ? Negate(el) : el;
    }

    public E Max(E a, E b)
    {
        return Compare(a, b) < 0 ? b : a;
    }

    public E Min(E a, E b)
    {
        return Compare(a, b) > 0 ? b : a;
    }

    public abstract E[]? DivideAndRemainder(E dividend, E divider);


    public E Quotient(E dividend, E divider)
    {
        var qr = DivideAndRemainder(dividend, divider);
        if (qr is null)
            throw new ArithmeticException("Not divisible with remainder: (" + dividend + ") / (" + divider + ")");
        return qr[0];
    }

    public virtual E Remainder(E dividend, E divider)
    {
        var qr = DivideAndRemainder(dividend, divider);
        if (qr is null)
            throw new ArithmeticException("Not divisible with remainder: (" + dividend + ") / (" + divider + ")");
        return qr[1];
    }

    public Utils.Nullable<E> DivideOrNull(E dividend, E divider)
    {
        if (IsOne(divider))
            return dividend;
        var qd = DivideAndRemainder(dividend, divider);
        if (qd is null || !IsZero(qd[1]))
            return Utils.Nullable<E>.Null;
        return qd[0];
    }

    public E DivideExact(E dividend, E divider)
    {
        var result = DivideOrNull(dividend, divider);
        if (result.IsNull)
            throw new ArithmeticException("not divisible: " + dividend + " / " + divider);
        return result.Value;
    }

    public E DivideExactMutable(E dividend, E divider)
    {
        return DivideExact(dividend, divider);
    }

    public abstract E Reciprocal(E element);


    public virtual E Gcd(E a, E b)
    {
        if (IsZero(a))
            return b;
        if (IsZero(b))
            return a;
        if (IsUnit(a))
            return a;
        if (IsUnit(b))
            return b;
        if (IsField())
            return a;
        if (!IsEuclideanRing())
            throw new NotSupportedException("GCD is not supported in this ring");

        // run Euclidean algorithm by default
        E x = a, y = b;
        while (true)
        {
            var r = Remainder(x, y);
            if (r == null)
                throw new ArithmeticException("Not divisible with remainder: (" + x + ") / (" + y + ")");
            if (IsZero(r))
                break;
            x = y;
            y = r;
        }

        return y;
    }

    public virtual E[] ExtendedGCD(E a, E b)
    {
        if (!IsEuclideanRing())
            throw new NotSupportedException("Extended GCD is not supported in this ring");
        if (IsZero(a))
            return [b, GetOne(), GetOne()];
        if (IsZero(b))
            return [a, GetOne(), GetOne()];
        if (IsField())
            return [GetOne(), DivideExact(Reciprocal(a), ValueOf(2)), DivideExact(Reciprocal(b), ValueOf(2))];

        E s = GetZero(), old_s = GetOne();
        E t = GetOne(), old_t = GetZero();
        E r = b, old_r = a;
        while (!IsZero(r))
        {
            var q = Quotient(old_r, r);
            if (q == null)
                throw new ArithmeticException("Not divisible with remainder: (" + old_r + ") / (" + r + ")");
            var tmp = old_r;
            old_r = r;
            r = Subtract(tmp, Multiply(q, r));
            tmp = old_s;
            old_s = s;
            s = Subtract(tmp, Multiply(q, s));
            tmp = old_t;
            old_t = t;
            t = Subtract(tmp, Multiply(q, t));
        }

        return [old_r, old_s, old_t];
    }


    public E[] FirstBezoutCoefficient(E a, E b)
    {
        E s = GetZero(), old_s = GetOne();
        E r = b, old_r = a;
        while (!IsZero(r))
        {
            var q = Quotient(old_r, r);
            if (q == null)
                throw new ArithmeticException("Not divisible with remainder: (" + old_r + ") / (" + r + ")");
            var tmp = old_r;
            old_r = r;
            r = Subtract(tmp, Multiply(q, r));
            tmp = old_s;
            old_s = s;
            s = Subtract(tmp, Multiply(q, s));
        }

        return [old_r, old_s];
    }

    public E Lcm(E a, E b)
    {
        if (IsZero(a) || IsZero(b))
            return GetZero();
        return Multiply(DivideExact(a, Gcd(a, b)), b);
    }


    public E Lcm(params E[] elements)
    {
        if (elements.Length == 1)
            return elements[0];
        var lcm = Lcm(elements[0], elements[1]);
        for (var i = 2; i < elements.Length; ++i)
            lcm = Lcm(lcm, elements[i]);
        return lcm;
    }


    public E Lcm(IEnumerable<E> elements)
    {
        return Lcm(elements.ToArray());
    }

    public E Gcd(params E[] elements)
    {
        return Gcd(elements.ToList());
    }


    public E Gcd(IEnumerable<E> elements)
    {
        var gcd = Utils.Nullable<E>.Null;
        foreach (var e in elements)
        {
            if (gcd.IsNull)
                gcd = e;
            else
                gcd = Gcd(gcd.Value, e);
        }

        if (gcd.IsNull)
            throw new ArithmeticException("GCD is not defined for empty list");

        return gcd.Value;
    }

    public virtual  FactorDecomposition<E> FactorSquareFree(E element)
    {
        throw new NotSupportedException();
    }


    public virtual  FactorDecomposition<E> Factor(E element)
    {
        throw new NotSupportedException();
    }


    public abstract E GetZero();


    public abstract E GetOne();


    public virtual E GetNegativeOne()
    {
        return Negate(GetOne());
    }


    public abstract bool IsZero(E element);


    public abstract bool IsOne(E element);


    public abstract bool IsUnit(E element);


    public bool IsUnitOrZero(E element)
    {
        return IsUnit(element) || IsZero(element);
    }


    public virtual bool IsMinusOne(E e)
    {
        return Equal(GetNegativeOne(), e);
    }


    public abstract E ValueOf(long val);


    public abstract E ValueOfBigInteger(BigInteger val);


    public E[] ValueOf(long[] elements)
    {
        var array = new E[elements.Length];
        for (var i = 0; i < elements.Length; i++)
            array[i] = ValueOf(elements[i]);
        return array;
    }


    public abstract E ValueOf(E val);


    public void SetToValueOf(E[] elements)
    {
        for (var i = 0; i < elements.Length; i++)
            elements[i] = ValueOf(elements[i]);
    }


    public E[] CreateZeroesArray(int length)
    {
        var array = new E[length];
        Array.Fill(array, GetZero());
        return array;
    }


    public void FillZeros(E[] array)
    {
        for (var i = 0; i < array.Length; i++)

            // NOTE: getZero() is invoked each time in a loop in order to fill array with unique elements
            array[i] = GetZero();
    }


    public E[,] CreateZeroesArray2d(int m, int n)
    {
        var arr = new E[m, n];
        for (var i = 0; i < arr.GetLength(0); i++)
        for (var j = 0; j < arr.GetLength(1); j++)
            arr[i, j] = GetZero();
        return arr;
    }


    public virtual E Pow(E @base, int exponent)
    {
        return Pow(@base, new BigInteger(exponent));
    }


    public virtual E Pow(E @base, long exponent)
    {
        return Pow(@base, new BigInteger(exponent));
    }


    public virtual E Pow(E @base, BigInteger exponent)
    {
        if (exponent.Sign < 0)
            return Pow(Reciprocal(@base), -exponent);
        if (exponent.IsOne)
            return @base;
        var result = GetOne();
        var k2p = Copy(@base); // <= copy the base (mutable operations are used below)
        for (;;)
        {
            if ((exponent.IsEven))
                result = MultiplyMutable(result, k2p);
            exponent = exponent >> 1;
            if (exponent.IsZero)
                return result;
            k2p = MultiplyMutable(k2p, k2p);
        }
    }


    public E Factorial(long num)
    {
        var result = GetOne();
        for (var i = 2; i <= num; ++i)
            result = MultiplyMutable(result, ValueOf(i));
        return result;
    }


    public abstract IEnumerator<E> Iterator();


    public E RandomElement()
    {
        return RandomElement(Rings.privateRandom);
    }


    public E RandomElement(Random rnd)
    {
        return ValueOf(rnd.NextInt64());
    }


    public E RandomElementTree(Random rnd)
    {
        return RandomElement(rnd);
    }


    public E RandomElementTree()
    {
        return RandomElementTree(Rings.privateRandom);
    }


    public E RandomNonZeroElement(Random rnd)
    {
        E el;
        do
        {
            el = RandomElement(rnd);
        } while (IsZero(el));

        return el;
    }
}