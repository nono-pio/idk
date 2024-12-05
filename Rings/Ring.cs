using System.Numerics;
using Rings.io;

namespace Rings;

// todo
/*
Comparator<E>,
Iterable<E>,
IParser<E>,
Stringifiable<E>,
java.io.Serializable
*/
public interface Ring<E> {
  
    bool isField();
    bool isEuclideanRing();
    bool isFinite() {
        return cardinality() != null;
    }
    bool isFiniteField() {
        return isField() && isFinite();
    }
    BigInteger cardinality();

    BigInteger characteristic();

    bool isPerfectPower();

    BigInteger perfectPowerBase();

    BigInteger perfectPowerExponent();
    
    E add(E a, E b);
    
    E add(params E[] elements) {
        E r = elements[0];
        for (int i = 1; i < elements.Length; i++)
            r = add(r, elements[i]);
        return r;
    }

    E increment(E element) {
        return add(element, getOne());
    }

    
    E decrement(E element) {
        return subtract(element, getOne());
    }

    E subtract(E a, E b);

    /**
     * Multiplies two elements
     *
     * @param a the first element
     * @param b the second element
     * @return a * b
     */
    E multiply(E a, E b);

    /**
     * Multiplies two elements
     *
     * @param a the first element
     * @param b the second element
     * @return a * b
     */
    E multiply(E a, long b) {
        return multiply(a, valueOf(b));
    }

    /**
     * Multiplies the array of elements
     *
     * @param elements the elements
     * @return product of the array
     */
    E multiply(params E[] elements) {
        E r = elements[0];
        for (int i = 1; i < elements.Length; i++)
            r = multiply(r, elements[i]);
        return r;
    }

    /**
     * Multiplies the array of elements
     *
     * @param elements the elements
     * @return product of the array
     */
    E multiply(IEnumerable<E> elements) {
        E r = getOne();
        foreach (E e in elements)
            r = multiplyMutable(r, e);
        return r;
    }

    /**
     * Negates the given element
     *
     * @param element the ring element
     * @return -val
     */
    E negate(E element);

    /**
     * Adds two elements and destroys the initial content of {@code a}.
     *
     * @param a the first element (may be destroyed)
     * @param b the second element
     * @return a + b
     */
    E addMutable(E a, E b) {return add(a, b);}

    /**
     * Subtracts {@code b} from {@code a} and destroys the initial content of {@code a}
     *
     * @param a the first element (may be destroyed)
     * @param b the second element
     * @return a - b
     */
    E subtractMutable(E a, E b) {return subtract(a, b);}

    /**
     * Multiplies two elements and destroys the initial content of {@code a}
     *
     * @param a the first element (may be destroyed)
     * @param b the second element
     * @return a * b
     */
    E multiplyMutable(E a, E b) { return multiply(a, b);}

    /**
     * Negates the given element and destroys the initial content of {@code element}
     *
     * @param element the ring element (may be destroyed)
     * @return -element
     */
    E negateMutable(E element) { return negate(element);}

    /**
     * Makes a deep copy of the specified element (for immutable instances the same reference returned).
     *
     * @param element the element
     * @return deep copy of specified element
     */
    E copy(E element);

    int compare(E a, E b);

    /**
     * Returns -1 if {@code element < 0}, 0 if {@code element == 0} and 1 if {@code element > 0}, where comparison is
     * specified by {@link #compare(Object, Object)}
     *
     * @param element the element
     * @return -1 if {@code element < 0}, 0 if {@code element == 0} and 1 otherwise
     */
    int signum(E element) {
        return compare(element, getZero()).CompareTo(0);
    }

    /**
     * Returns the abs value of element (no copy)
     */
    E abs(E el) {
        return signum(el) < 0 ? negate(el) : el;
    }

    /**
     * Returns the max value (no copy)
     */
    E max(E a, E b) {
        return compare(a, b) < 0 ? b : a;
    }

    /**
     * Returns the min value (no copy)
     */
    E min(E a, E b) {
        return compare(a, b) > 0 ? b : a;
    }

    /**
     * Returns quotient and remainder of {@code dividend / divider}
     *
     * @param dividend the dividend
     * @param divider  the divider
     * @return {@code {quotient, remainder}}
     */
    E[] divideAndRemainder(E dividend, E divider);

    /**
     * Returns the quotient of {@code dividend / divider}
     *
     * @param dividend the dividend
     * @param divider  the divider
     * @return the quotient of {@code dividend / divider}
     */
    E quotient(E dividend, E divider) {
        return divideAndRemainder(dividend, divider)[0];
    }

    /**
     * Returns the remainder of {@code dividend / divider}
     *
     * @param dividend the dividend
     * @param divider  the divider
     * @return the remainder of {@code dividend / divider}
     */
    E remainder(E dividend, E divider) {
        return divideAndRemainder(dividend, divider)[1];
    }

    /**
     * Divides {@code dividend} by {@code divider} or returns {@code null} if exact division is not possible
     *
     * @param dividend the dividend
     * @param divider  the divider
     * @return {@code dividend / divider} or {@code null} if exact division is not possible
     */
    util.Nullable<E> divideOrNull(E dividend, E divider) {
        if (isOne(divider))
            return new util.Nullable<E>(dividend);
        var qd = divideAndRemainder(dividend, divider);
        if (qd == null)
            return new util.Nullable<E>();
        if (!isZero(qd[1]))
            return new util.Nullable<E>();
        return new util.Nullable<E>(qd[0]);
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
    E divideExact(E dividend, E divider) {
        var result = divideOrNull(dividend, divider);
        if (result.IsNull)
            throw new ArithmeticException("not divisible: " + dividend + " / " + divider);
        return result.Value;
    }

    /** Internal API */
    E divideExactMutable(E dividend, E divider) {
        return divideExact(dividend, divider);
    }

    /**
     * Gives the inverse element {@code element ^ (-1) }
     *
     * @param element the element
     * @return {@code element ^ (-1)}
     */
    E reciprocal(E element);

    /**
     * Returns the greatest common divisor of two elements
     *
     * @param a the first element
     * @param b the second element
     * @return gcd
     */
    E gcd(E a, E b) {
        if (isZero(a)) return b;
        if (isZero(b)) return a;
        if (isUnit(a)) return a;
        if (isUnit(b)) return b;
        if (isField()) return a;
        if (!isEuclideanRing())
            throw new ArgumentException("GCD is not supported in this ring");

        // run Euclidean algorithm by default
        E x = a, y = b, r;
        while (true) {
            r = remainder(x, y);
            if (r == null)
                throw new ArithmeticException("Not divisible with remainder: (" + x + ") / (" + y + ")");

            if (isZero(r))
                break;
            x = y;
            y = r;
        }
        return y;
    }

    /**
     * Returns array of {@code [gcd(a,b), s, t]} such that {@code s * a + t * b = gcd(a, b)}
     *
     * @throws UnsupportedOperationException if this is not the Euclidean ring and there is no special implementation
     *                                       provided by particular subtype
     */
    (E gcd, E s, E t) extendedGCD(E a, E b) {
        if (!isEuclideanRing())
            throw new ArgumentException("Extended GCD is not supported in this ring");

        if (isZero(a)) 
            return (b, getOne(), getOne());
        if (isZero(b)) 
            return (a, getOne(), getOne());

        if (isField()) 
            return (getOne(), divideExact(reciprocal(a), valueOf(2)), divideExact(reciprocal(b), valueOf(2)));

        E s = getZero(), old_s = getOne();
        E t = getOne(), old_t = getZero();
        E r = b, old_r = a;

        E q;
        E tmp;
        while (!isZero(r)) {
            q = quotient(old_r, r);
            if (q == null)
                throw new ArithmeticException("Not divisible with remainder: (" + old_r + ") / (" + r + ")");

            tmp = old_r;
            old_r = r;
            r = subtract(tmp, multiply(q, r));

            tmp = old_s;
            old_s = s;
            s = subtract(tmp, multiply(q, s));

            tmp = old_t;
            old_t = t;
            t = subtract(tmp, multiply(q, t));
        }

        return (old_r, old_s, old_t);
    }

    /**
     * Returns array of {@code [gcd(a,b), s]} such that {@code s * a + t * b = gcd(a, b)}
     *
     * @param a the first ring element (for which the Bezout coefficient is computed)
     * @param b the second ring element
     * @return array of {@code [gcd(a,b), s]} such that {@code s * a + t * b = gcd(a, b)}
     */
    (E gcd, E s) firstBezoutCoefficient(E a, E b) {
        E s = getZero(), old_s = getOne();
        E r = b, old_r = a;

        E q;
        E tmp;
        while (!isZero(r)) {
            q = quotient(old_r, r);
            if (q == null)
                throw new ArithmeticException("Not divisible with remainder: (" + old_r + ") / (" + r + ")");

            tmp = old_r;
            old_r = r;
            r = subtract(tmp, multiply(q, r));

            tmp = old_s;
            old_s = s;
            s = subtract(tmp, multiply(q, s));
        }
        
        return (old_r, old_s);
    }

    /**
     * Returns the least common multiple of two elements
     *
     * @param a the first element
     * @param b the second element
     * @return lcm
     */
    E lcm(E a, E b) {
        if (isZero(a) || isZero(b))
            return getZero();
        return multiply(divideExact(a, gcd(a, b)), b);
    }

    /**
     * Returns the least common multiple of two elements
     *
     * @param elements the elements
     * @return lcm
     */
    E lcm(params E[] elements) {
        if (elements.Length == 1)
            return elements[0];
        E lcm_ = lcm(elements[0], elements[1]);
        for (int i = 2; i < elements.Length; ++i)
            lcm_ = lcm(lcm_, elements[i]);
        return lcm_;
    }

    /**
     * Returns the least common multiple of two elements
     *
     * @param elements the elements
     * @return lcm
     */
    E lcm(IEnumerable<E> elements) {
        return lcm(elements.ToArray());
    }

    /**
     * Returns greatest common divisor of specified elements
     *
     * @param elements the elements
     * @return gcd
     */
    E gcd(params E[] elements) {
        return gcd((IEnumerable<E>) elements);
    }

    /**
     * Returns greatest common divisor of specified elements
     *
     * @param elements the elements
     * @return gcd
     */
    E gcd(IEnumerable<E> elements) {
        var gcd_ = new util.Nullable<E>();
        foreach (E e in elements) {
            if (gcd_.IsNull)
                gcd_.Value = e;
            else
                gcd_.Value = gcd(gcd_.Value, e);
        }
        return gcd_.Value;
    }

    /**
     * Square-free factorization of specified element
     */
    FactorDecomposition<E> factorSquareFree(E element) {
        throw new NotImplementedException();
    }

    /**
     * Factor specified element
     */
    FactorDecomposition<E> factor(E element) {
        throw new NotImplementedException();
    }

    /**
     * Returns zero element of this ring
     *
     * @return 0
     */
    E getZero();

    /**
     * Returns unit element of this ring (one)
     *
     * @return 1
     */
    E getOne();

    /**
     * Returns negative unit element of this ring (minus one)
     *
     * @return -1
     */
    E getNegativeOne() {
        return negate(getOne());
    }

    /**
     * Tests whether specified element is zero
     *
     * @param element the ring element
     * @return whether specified element is zero
     */
    bool isZero(E element);

    /**
     * Tests whether specified element is one (exactly)
     *
     * @param element the ring element
     * @return whether specified element is exactly one
     * @see #isUnit(Object)
     */
    bool isOne(E element);

    /**
     * Tests whether specified element is a ring unit
     *
     * @param element the ring element
     * @return whether specified element is a ring unit
     * @see #isOne(Object)
     */
    bool isUnit(E element);

    /**
     * Tests whether specified element is a ring unit or zero
     *
     * @param element the ring element
     * @return whether specified element is a ring unit or zero
     */
    bool isUnitOrZero(E element) {
        return isUnit(element) || isZero(element);
    }

    /**
     * Tests whether specified element is minus one
     *
     * @param e the ring element
     * @return whether specified element is minus one
     */
    bool isMinusOne(E e) {
        return getNegativeOne().Equals(e);
    }

    /**
     * Returns ring element associated with specified {@code long}
     *
     * @param val machine integer
     * @return ring element associated with specified {@code long}
     */
    E valueOf(long val);

    /**
     * Returns ring element associated with specified integer
     *
     * @param val integer
     * @return ring element associated with specified integer
     */
    E valueOfBigInteger(BigInteger val);

    /**
     * Converts array of machine integers to ring elements via {@link #valueOf(long)}
     *
     * @param elements array of machine integers
     * @return array of ring elements
     */
    E[] valueOf(long[] elements) {
        E[] array = new E[elements.Length];
        for (int i = 0; i < elements.Length; i++)
            array[i] = valueOf(elements[i]);
        return array;
    }

    /**
     * Converts a value from other ring to this ring. The result is not guarantied to be a new instance (i.e. {@code val
     * == valueOf(val)} is possible).
     *
     * @param val some element from any ring
     * @return this ring element associated with specified {@code val}
     */
    E valueOf(E val);

    /**
     * Applies {@link #valueOf(Object)} inplace to the specified array
     *
     * @param elements the array
     */
    void setToValueOf(E[] elements) {
        for (int i = 0; i < elements.Length; i++)
            elements[i] = valueOf(elements[i]);
    }

    /**
     * Creates array filled with zero elements
     *
     * @param length array length
     * @return array filled with zero elements of specified {@code length}
     */
    E[] createZeroesArray(int length) {
        E[] array = new E[length];
        fillZeros(array);
        return array;
    }

    /**
     * Fills array with zeros
     */
    void fillZeros(E[] array) {
        for (int i = 0; i < array.Length; i++)
            // NOTE: getZero() is invoked each time in a loop in order to fill array with unique elements
            array[i] = getZero();
    }

    /**
     * Creates 2d array of ring elements of specified shape filled with zero elements
     *
     * @param m result length
     * @param n length of each array in the result
     * @return 2d array E[m][n] filled with zero elements
     */
    E[,] createZeroesArray2d(int m, int n) {
        E[,] arr = new E[m, n];
        for (int i = 0; i < arr.GetLength(0); i++)
        {
            for (int j = 0; j < arr.GetLength(1); j++)
            {
                arr[i, j] = getZero();
            }
        }
        
        return arr;
    }
    
    /**
     * Returns {@code base} in a power of {@code exponent} (non negative)
     *
     * @param base     base
     * @param exponent exponent (non negative)
     * @return {@code base} in a power of {@code exponent}
     */
    E pow(E @base, int exponent) {
        return pow(@base, new BigInteger(exponent));
    }

    /**
     * Returns {@code base} in a power of {@code exponent} (non negative)
     *
     * @param base     base
     * @param exponent exponent (non negative)
     * @return {@code base} in a power of {@code exponent}
     */
    E pow(E @base, long exponent) {
        return pow(@base, new BigInteger(exponent));
    }

    /**
     * Returns {@code base} in a power of {@code exponent} (non negative)
     *
     * @param base     base
     * @param exponent exponent (non negative)
     * @return {@code base} in a power of {@code exponent}
     */
    E pow(E @base, BigInteger exponent) {
        if (exponent.Sign < 0)
            return pow(reciprocal(@base), -exponent);

        if (exponent.IsOne)
            return @base;

        E result = getOne();
        E k2p = copy(@base); // <= copy the base (mutable operations are used below)
        for (; ; ) {
            if ((!exponent.IsEven))
                result = multiplyMutable(result, k2p);
            exponent = exponent >> 1;
            if (exponent.IsZero)
                return result;
            k2p = multiplyMutable(k2p, k2p);
        }
    }

    /**
     * Gives a product of {@code valueOf(1) * valueOf(2) * .... * valueOf(num) }
     *
     * @param num the number
     * @return {@code valueOf(1) * valueOf(2) * .... * valueOf(num) }
     */
    public E factorial(long num) {
        E result = getOne();
        for (int i = 2; i <= num; ++i)
            result = multiplyMutable(result, valueOf(i));
        return result;
    }

//    /**
//     * Returns the element which is next to the specified {@code element} (according to {@link #compare(Object, Object)})
//     * or {@code null} in the case of infinite cardinality
//     *
//     * @param element the element
//     * @return next element
//     */
//    E nextElement(E element);

    /**
     * Returns iterator over ring elements (for finite rings, otherwise throws exception)
     */
    IEnumerable<E> iterator();

    /**
     * Returns a random element from this ring
     *
     * @return random element from this ring
     */
    E randomElement() { return randomElement(Rings.privateRandom);}

    /**
     * Returns a random element from this ring
     *
     * @param rnd the source of randomness
     * @return random element from this ring
     */
    E randomElement(RandomGenerator rnd) { return valueOf(rnd.nextLong());}

    /**
     * If this ring has a complicated nested structure, this method guaranties that the resulting random element will
     * reflect ring complicated structure, i.e. the result will be roughly as complicated as the ring is
     *
     * @return random element from this ring
     */
    E randomElementTree(RandomGenerator rnd) { return randomElement(rnd);}

    /**
     * If this ring has a complicated nested structure, this method guaranties that the resulting random element will
     * reflect ring complicated structure, i.e. the result will be roughly as complicated as the ring is
     *
     * @return random element from this ring
     */
    E randomElementTree() { return randomElementTree(Rings.privateRandom);}

    /**
     * Returns a random non zero element from this ring
     *
     * @param rnd the source of randomness
     * @return random non zero element from this ring
     */
    E randomNonZeroElement(RandomGenerator rnd) {
        E el;
        do {
            el = randomElement(rnd);
        } while (isZero(el));
        return el;
    }

    /**
     * Parse string into ring element
     *
     * @param string string
     * @return ring element
     * @see Coder
     */
    E parse(string @string) {
        return Coder<_,_,_>.mkCoder(this).parse(@string);
    }
//    /**
//     * Returns ring with larger cardinality that contains all elements of this or null if there is no such ring.
//     *
//     * @return ring with larger cardinality that contains all elements of this or null if there is no such ring
//     */
//    Ring<E> getExtension();
}
