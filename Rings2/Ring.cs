using System.Numerics;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Io;


namespace Cc.Redberry.Rings
{
    /// <summary>
    /// Ring of elements. Mathematical operations defined in {@code Ring} interface include all <i>field</i> operations,
    /// though the particular implementations may represent a more restricted rings (general rings, Euclidean rings etc.), in
    /// which case some field operations (e.g. reciprocal) are not applicable (will throw exception).
    /// </summary>
    /// <typeparam name="E">the type of objects that may be operated by this ring </typeparam>
    /// <remarks>@since1.0</remarks>
    public interface Ring<E> : IComparer<E>, IEnumerable<E>, IParser<E>, Stringifiable<E>
    {
        /// <summary>
        /// Returns whether this ring is a field
        /// </summary>
        /// <returns>whether this ring is a field</returns>
        bool IsField();

        /// <summary>
        /// Returns whether this ring is a Euclidean ring
        /// </summary>
        /// <returns>whether this ring is a Euclidean ring</returns>
        bool IsEuclideanRing();

        /// <summary>
        /// Returns whether this ring is finite
        /// </summary>
        /// <returns>whether this ring is finite</returns>
        bool IsFinite()
        {
            return Cardinality() != null;
        }


        /// <summary>
        /// Returns whether this ring is a finite field
        /// </summary>
        /// <returns>whether this ring is a finite field</returns>
        bool IsFiniteField()
        {
            return IsField() && IsFinite();
        }


        /// <summary>
        /// Returns the number of elements in this ring (cardinality) or null if ring is infinite
        /// </summary>
        /// <returns>the number of elements in this ring (cardinality) or null if ring is infinite</returns>
        BigInteger Cardinality();

        /// <summary>
        /// Returns characteristic of this ring
        /// </summary>
        /// <returns>characteristic of this ring</returns>
        BigInteger Characteristic();


        /// <summary>
        /// Returns whether the cardinality is a perfect power (p^k with k > 1)
        /// </summary>
        /// <returns>whether the cardinality is a perfect power (p^k with k > 1)</returns>
        bool IsPerfectPower();


        /// <summary>
        /// Returns {@code base} so that {@code cardinality == base^exponent} or null if cardinality is not finite
        /// </summary>
        /// <returns>{@code base} so that {@code cardinality == base^exponent} or null if cardinality is not finite</returns>
        BigInteger PerfectPowerBase();


        /// <summary>
        /// Returns {@code exponent} so that {@code cardinality == base^exponent} or null if cardinality is not finite
        /// </summary>
        /// <returns>{@code exponent} so that {@code cardinality == base^exponent} or null if cardinality is not finite</returns>
        BigInteger PerfectPowerExponent();


        /// <summary>
        /// Add two elements
        /// </summary>
        /// <param name="a">the first element</param>
        /// <param name="b">the second element</param>
        /// <returns>a + b</returns>
        E Add(E a, E b);


        /// <summary>
        /// Total of the array of elements
        /// </summary>
        /// <param name="elements">elements to sum</param>
        /// <returns>sum of the array</returns>
        E Add(params E[] elements)
        {
            E r = elements[0];
            for (int i = 1; i < elements.Length; i++)
                r = Add(r, elements[i]);
            return r;
        }


        /// <summary>
        /// Returns {@code element + 1}
        /// </summary>
        /// <param name="element">the element</param>
        /// <returns>{@code element + 1}</returns>
        E Increment(E element)
        {
            return Add(element, GetOne());
        }


        /// <summary>
        /// Returns {@code element - 1}
        /// </summary>
        /// <param name="element">the element</param>
        /// <returns>{@code element - 1}</returns>
        E Decrement(E element)
        {
            return Subtract(element, GetOne());
        }


        /// <summary>
        /// Subtracts {@code b} from {@code a}
        /// </summary>
        /// <param name="a">the first element</param>
        /// <param name="b">the second element</param>
        /// <returns>a - b</returns>
        E Subtract(E a, E b);


        /// <summary>
        /// Multiplies two elements
        /// </summary>
        /// <param name="a">the first element</param>
        /// <param name="b">the second element</param>
        /// <returns>a * b</returns>
        E Multiply(E a, E b);


        /// <summary>
        /// Multiplies two elements
        /// </summary>
        /// <param name="a">the first element</param>
        /// <param name="b">the second element</param>
        /// <returns>a * b</returns>
        E Multiply(E a, long b)
        {
            return Multiply(a, ValueOf(b));
        }


        /// <summary>
        /// Multiplies the array of elements
        /// </summary>
        /// <param name="elements">the elements</param>
        /// <returns>product of the array</returns>
        E Multiply(params E[] elements)
        {
            E r = elements[0];
            for (int i = 1; i < elements.Length; i++)
                r = Multiply(r, elements[i]);
            return r;
        }


        /// <summary>
        /// Multiplies the array of elements
        /// </summary>
        /// <param name="elements">the elements</param>
        /// <returns>product of the array</returns>
        E Multiply(IEnumerable<E> elements)
        {
            E r = GetOne();
            foreach (E e in elements)
                r = MultiplyMutable(r, e);
            return r;
        }


        /// <summary>
        /// Negates the given element
        /// </summary>
        /// <param name="element">the ring element</param>
        /// <returns>-val</returns>
        E Negate(E element);


        /// <summary>
        /// Adds two elements and destroys the initial content of {@code a}.
        /// </summary>
        /// <param name="a">the first element (may be destroyed)</param>
        /// <param name="b">the second element</param>
        /// <returns>a + b</returns>
        E AddMutable(E a, E b)
        {
            return Add(a, b);
        }


        /// <summary>
        /// Subtracts {@code b} from {@code a} and destroys the initial content of {@code a}
        /// </summary>
        /// <param name="a">the first element (may be destroyed)</param>
        /// <param name="b">the second element</param>
        /// <returns>a - b</returns>
        E SubtractMutable(E a, E b)
        {
            return Subtract(a, b);
        }


        /// <summary>
        /// Multiplies two elements and destroys the initial content of {@code a}
        /// </summary>
        /// <param name="a">the first element (may be destroyed)</param>
        /// <param name="b">the second element</param>
        /// <returns>a * b</returns>
        E MultiplyMutable(E a, E b)
        {
            return Multiply(a, b);
        }


        /// <summary>
        /// Negates the given element and destroys the initial content of {@code element}
        /// </summary>
        /// <param name="element">the ring element (may be destroyed)</param>
        /// <returns>-element</returns>
        E NegateMutable(E element)
        {
            return Negate(element);
        }


        /// <summary>
        /// Makes a deep copy of the specified element (for immutable instances the same reference returned).
        /// </summary>
        /// <param name="element">the element</param>
        /// <returns>deep copy of specified element</returns>
        E Copy(E element);

        /// <summary>
        /// Returns -1 if {@code element < 0}, 0 if {@code element == 0} and 1 if {@code element > 0}, where comparison is
        /// specified by {@link #compare(Object, Object)}
        /// </summary>
        /// <param name="element">the element</param>
        /// <returns>-1 if {@code element < 0}, 0 if {@code element == 0} and 1 otherwise</returns>
        int Signum(E element)
        {
            return Compare(element, GetZero()).CompareTo(0);
        }


        /// <summary>
        /// Returns the abs value of element (no copy)
        /// </summary>
        E Abs(E el)
        {
            return Signum(el) < 0 ? Negate(el) : el;
        }


        /// <summary>
        /// Returns the max value (no copy)
        /// </summary>
        E Max(E a, E b)
        {
            return Compare(a, b) < 0 ? b : a;
        }


        /// <summary>
        /// Returns the min value (no copy)
        /// </summary>
        E Min(E a, E b)
        {
            return Compare(a, b) > 0 ? b : a;
        }


        /// <summary>
        /// Returns quotient and remainder of {@code dividend / divider}
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <returns>{@code {quotient, remainder}}</returns>
        E[] DivideAndRemainder(E dividend, E divider);


        /// <summary>
        /// Returns the quotient of {@code dividend / divider}
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <returns>the quotient of {@code dividend / divider}</returns>
        E Quotient(E dividend, E divider)
        {
            return DivideAndRemainder(dividend, divider)[0];
        }

        /// <summary>
        /// Returns the remainder of {@code dividend / divider}
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <returns>the remainder of {@code dividend / divider}</returns>
        public E Remainder(E dividend, E divider)
        {
            return DivideAndRemainder(dividend, divider)[1];
        }


        /// <summary>
        /// Divides {@code dividend} by {@code divider} or returns {@code null} if exact division is not possible
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <returns>{@code dividend / divider} or {@code null} if exact division is not possible</returns>
        E DivideOrNull(E dividend, E divider)
        {
            if (IsOne(divider))
                return dividend;
            E[] qd = DivideAndRemainder(dividend, divider);
            if (qd == null)
                return null;
            if (!IsZero(qd[1]))
                return null;
            return qd[0];
        }


        /// <summary>
        /// Divides {@code dividend} by {@code divider} or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <param name="dividend">the dividend</param>
        /// <param name="divider">the divider</param>
        /// <returns>{@code dividend / divider}</returns>
        /// <exception cref="ArithmeticException">if exact division is not possible</exception>
        E DivideExact(E dividend, E divider)
        {
            E result = DivideOrNull(dividend, divider);
            if (result == null)
                throw new ArithmeticException("not divisible: " + dividend + " / " + divider);
            return result;
        }


        /// <summary>
        /// Internal API
        /// </summary>
        E DivideExactMutable(E dividend, E divider)
        {
            return DivideExact(dividend, divider);
        }


        /// <summary>
        /// Gives the inverse element {@code element ^ (-1) }
        /// </summary>
        /// <param name="element">the element</param>
        /// <returns>{@code element ^ (-1)}</returns>
        E Reciprocal(E element);


        /// <summary>
        /// Returns the greatest common divisor of two elements
        /// </summary>
        /// <param name="a">the first element</param>
        /// <param name="b">the second element</param>
        /// <returns>gcd</returns>
        E Gcd(E a, E b)
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
            E x = a, y = b, r;
            while (true)
            {
                r = Remainder(x, y);
                if (r == null)
                    throw new ArithmeticException("Not divisible with remainder: (" + x + ") / (" + y + ")");
                if (IsZero(r))
                    break;
                x = y;
                y = r;
            }

            return y;
        }


        /// <summary>
        /// Returns array of {@code [gcd(a,b), s, t]} such that {@code s * a + t * b = gcd(a, b)}
        /// </summary>
        /// <exception cref="UnsupportedOperationException">if this is not the Euclidean ring and there is no special implementation
        ///                                       provided by particular subtype</exception>
        E[] ExtendedGCD(E a, E b)
        {
            if (!IsEuclideanRing())
                throw new NotSupportedException("Extended GCD is not supported in this ring");
            if (IsZero(a))
                return [b, GetOne(), GetOne()];
            if (IsZero(b))
                return [a, GetOne(), GetOne()];
            if (IsField())
                return  [GetOne(), DivideExact(Reciprocal(a), ValueOf(2)), DivideExact(Reciprocal(b), ValueOf(2))];

            E s = GetZero(), old_s = GetOne();
            E t = GetOne(), old_t = GetZero();
            E r = b, old_r = a;
            E q;
            E tmp;
            while (!IsZero(r))
            {
                q = Quotient(old_r, r);
                if (q == null)
                    throw new ArithmeticException("Not divisible with remainder: (" + old_r + ") / (" + r + ")");
                tmp = old_r;
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


        /// <summary>
        /// Returns array of {@code [gcd(a,b), s]} such that {@code s * a + t * b = gcd(a, b)}
        /// </summary>
        /// <param name="a">the first ring element (for which the Bezout coefficient is computed)</param>
        /// <param name="b">the second ring element</param>
        /// <returns>array of {@code [gcd(a,b), s]} such that {@code s * a + t * b = gcd(a, b)}</returns>
        E[] FirstBezoutCoefficient(E a, E b)
        {
            E s = GetZero(), old_s = GetOne();
            E r = b, old_r = a;
            E q;
            E tmp;
            while (!IsZero(r))
            {
                q = Quotient(old_r, r);
                if (q == null)
                    throw new ArithmeticException("Not divisible with remainder: (" + old_r + ") / (" + r + ")");
                tmp = old_r;
                old_r = r;
                r = Subtract(tmp, Multiply(q, r));
                tmp = old_s;
                old_s = s;
                s = Subtract(tmp, Multiply(q, s));
            }

            E[] result = CreateArray(2);
            result[0] = old_r;
            result[1] = old_s;
            return result;
        }


        /// <summary>
        /// Returns the least common multiple of two elements
        /// </summary>
        /// <param name="a">the first element</param>
        /// <param name="b">the second element</param>
        /// <returns>lcm</returns>
        E Lcm(E a, E b)
        {
            if (IsZero(a) || IsZero(b))
                return GetZero();
            return Multiply(DivideExact(a, Gcd(a, b)), b);
        }


        /// <summary>
        /// Returns the least common multiple of two elements
        /// </summary>
        /// <param name="elements">the elements</param>
        /// <returns>lcm</returns>
        E Lcm(params E[] elements)
        {
            if (elements.Length == 1)
                return elements[0];
            E lcm = Lcm(elements[0], elements[1]);
            for (int i = 2; i < elements.Length; ++i)
                lcm = Lcm(lcm, elements[i]);
            return lcm;
        }


        /// <summary>
        /// Returns the least common multiple of two elements
        /// </summary>
        /// <param name="elements">the elements</param>
        /// <returns>lcm</returns>
        E Lcm(IEnumerable<E> elements)
        {
            return Lcm(StreamSupport.Stream(elements.Spliterator(), false).ToArray());
        }


        /// <summary>
        /// Returns greatest common divisor of specified elements
        /// </summary>
        /// <param name="elements">the elements</param>
        /// <returns>gcd</returns>
        E Gcd(params E[] elements)
        {
            return Gcd(elements.ToList());
        }


        /// <summary>
        /// Returns greatest common divisor of specified elements
        /// </summary>
        /// <param name="elements">the elements</param>
        /// <returns>gcd</returns>
        E Gcd(IEnumerable<E> elements)
        {
            E gcd = null;
            foreach (E e in elements)
            {
                if (gcd == null)
                    gcd = e;
                else
                    gcd = Gcd(gcd, e);
            }

            return gcd;
        }


        /// <summary>
        /// Square-free factorization of specified element
        /// </summary>
        FactorDecomposition<E> FactorSquareFree(E element)
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// Factor specified element
        /// </summary>
        FactorDecomposition<E> Factor(E element)
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// Returns zero element of this ring
        /// </summary>
        /// <returns>0</returns>
        E GetZero();


        /// <summary>
        /// Returns unit element of this ring (one)
        /// </summary>
        /// <returns>1</returns>
        E GetOne();


        /// <summary>
        /// Returns negative unit element of this ring (minus one)
        /// </summary>
        /// <returns>-1</returns>
        E GetNegativeOne()
        {
            return Negate(GetOne());
        }

        /// <summary>
        /// Tests whether specified element is zero
        /// </summary>
        /// <param name="element">the ring element</param>
        /// <returns>whether specified element is zero</returns>
        bool IsZero(E element);


        /// <summary>
        /// Tests whether specified element is one (exactly)
        /// </summary>
        /// <param name="element">the ring element</param>
        /// <returns>whether specified element is exactly one</returns>
        /// <remarks>@see#isUnit(Object)</remarks>
        bool IsOne(E element);


        /// <summary>
        /// Tests whether specified element is a ring unit
        /// </summary>
        /// <param name="element">the ring element</param>
        /// <returns>whether specified element is a ring unit</returns>
        /// <remarks>@see#isOne(Object)</remarks>
        bool IsUnit(E element);


        /// <summary>
        /// Tests whether specified element is a ring unit or zero
        /// </summary>
        /// <param name="element">the ring element</param>
        /// <returns>whether specified element is a ring unit or zero</returns>
        bool IsUnitOrZero(E element)
        {
            return IsUnit(element) || IsZero(element);
        }


        /// <summary>
        /// Tests whether specified element is minus one
        /// </summary>
        /// <param name="e">the ring element</param>
        /// <returns>whether specified element is minus one</returns>
        bool IsMinusOne(E e)
        {
            return GetNegativeOne().Equals(e);
        }


        /// <summary>
        /// Returns ring element associated with specified {@code long}
        /// </summary>
        /// <param name="val">machine integer</param>
        /// <returns>ring element associated with specified {@code long}</returns>
        E ValueOf(long val);


        /// <summary>
        /// Returns ring element associated with specified integer
        /// </summary>
        /// <param name="val">integer</param>
        /// <returns>ring element associated with specified integer</returns>
        E ValueOfBigInteger(BigInteger val);


        /// <summary>
        /// Converts array of machine integers to ring elements via {@link #valueOf(long)}
        /// </summary>
        /// <param name="elements">array of machine integers</param>
        /// <returns>array of ring elements</returns>
        E[] ValueOf(long[] elements)
        {
            E[] array = CreateArray(elements.Length);
            for (int i = 0; i < elements.Length; i++)
                array[i] = ValueOf(elements[i]);
            return array;
        }


        /// <summary>
        /// Converts a value from other ring to this ring. The result is not guarantied to be a new instance (i.e. {@code val
        /// == valueOf(val)} is possible).
        /// </summary>
        /// <param name="val">some element from any ring</param>
        /// <returns>this ring element associated with specified {@code val}</returns>
        E ValueOf(E val);


        /// <summary>
        /// Applies {@link #valueOf(Object)} inplace to the specified array
        /// </summary>
        /// <param name="elements">the array</param>
        void SetToValueOf(E[] elements)
        {
            for (int i = 0; i < elements.Length; i++)
                elements[i] = ValueOf(elements[i]);
        }


        /// <summary>
        /// Creates generic array of ring elements of specified length
        /// </summary>
        /// <param name="length">array length</param>
        /// <returns>array of ring elements of specified {@code length}</returns>
        E[] CreateArray(int length)
        {
            return new E[length];
        }


        /// <summary>
        /// Creates 2d array of ring elements of specified length
        /// </summary>
        /// <param name="length">array length</param>
        /// <returns>2d array of ring elements of specified {@code length}</returns>
        E[,] CreateArray2d(int length)
        {
            return new E[length, 0];
        }


        /// <summary>
        /// Creates 2d array of ring elements of specified shape
        /// </summary>
        /// <param name="m">result length</param>
        /// <param name="n">length of each array in the result</param>
        /// <returns>2d array E[m][n]</returns>
        E[,] CreateArray2d(int m, int n)
        {
            return new E[m, n];
        }

        /// <summary>
        /// Creates array filled with zero elements
        /// </summary>
        /// <param name="length">array length</param>
        /// <returns>array filled with zero elements of specified {@code length}</returns>
        E[] CreateZeroesArray(int length)
        {
            E[] array = new E[length];
            FillZeros(array);
            return array;
        }


        /// <summary>
        /// Fills array with zeros
        /// </summary>
        void FillZeros(E[] array)
        {
            for (int i = 0; i < array.Length; i++)

                // NOTE: getZero() is invoked each time in a loop in order to fill array with unique elements
                array[i] = GetZero();
        }


        /// <summary>
        /// Creates 2d array of ring elements of specified shape filled with zero elements
        /// </summary>
        /// <param name="m">result length</param>
        /// <param name="n">length of each array in the result</param>
        /// <returns>2d array E[m][n] filled with zero elements</returns>
        E[,] CreateZeroesArray2d(int m, int n)
        {
            E[,] arr = CreateArray2d(m, n);
            for (int i = 0; i < arr.GetLength(0); i++)
                for (int j = 0; j < arr.GetLength(1); j++)
                    arr[i, j] = GetZero();
            return arr;
        }


        /// <summary>
        /// Creates generic array of {@code {a, b}}
        /// </summary>
        /// <param name="a">the first element of array</param>
        /// <param name="b">the second element of array</param>
        /// <returns>array {@code {a,b}}</returns>
        E[] CreateArray(E a, E b)
        {
            E[] array = CreateArray(2);
            array[0] = a;
            array[1] = b;
            return array;
        }


        /// <summary>
        /// Creates generic array of {@code {a, b, c}}
        /// </summary>
        E[] CreateArray(E a, E b, E c)
        {
            E[] array = CreateArray(3);
            array[0] = a;
            array[1] = b;
            array[2] = c;
            return array;
        }


        /// <summary>
        /// Creates generic array with single element
        /// </summary>
        /// <param name="element">the element</param>
        /// <returns>array with single specified element</returns>
        E[] CreateArray(E element)
        {
            E[] array = CreateArray(1);
            array[0] = element;
            return array;
        }


        /// <summary>
        /// Returns {@code base} in a power of {@code exponent} (non negative)
        /// </summary>
        /// <param name="base">base</param>
        /// <param name="exponent">exponent (non negative)</param>
        /// <returns>{@code base} in a power of {@code exponent}</returns>
        public virtual E Pow(E @base, int exponent)
        {
            return Pow(@base, new BigInteger(exponent));
        }


        /// <summary>
        /// Returns {@code base} in a power of {@code exponent} (non negative)
        /// </summary>
        /// <param name="base">base</param>
        /// <param name="exponent">exponent (non negative)</param>
        /// <returns>{@code base} in a power of {@code exponent}</returns>
        E Pow(E @base, long exponent)
        {
            return Pow(@base, new BigInteger(exponent));
        }


        /// <summary>
        /// Returns {@code base} in a power of {@code exponent} (non negative)
        /// </summary>
        /// <param name="base">base</param>
        /// <param name="exponent">exponent (non negative)</param>
        /// <returns>{@code base} in a power of {@code exponent}</returns>
        E Pow(E @base, BigInteger exponent)
        {
            if (exponent.Signum() < 0)
                return Pow(Reciprocal(@base), exponent.Negate());
            if (exponent.IsOne)
                return @base;
            E result = GetOne();
            E k2p = Copy(@base); // <= copy the base (mutable operations are used below)
            for (;;)
            {
                if ((exponent.TestBit(0)))
                    result = MultiplyMutable(result, k2p);
                exponent = exponent.ShiftRight(1);
                if (exponent.IsZero)
                    return result;
                k2p = MultiplyMutable(k2p, k2p);
            }
        }


        /// <summary>
        /// Gives a product of {@code valueOf(1) * valueOf(2) * .... * valueOf(num) }
        /// </summary>
        /// <param name="num">the number</param>
        /// <returns>{@code valueOf(1) * valueOf(2) * .... * valueOf(num) }</returns>
        E Factorial(long num)
        {
            E result = GetOne();
            for (int i = 2; i <= num; ++i)
                result = MultiplyMutable(result, ValueOf(i));
            return result;
        }


        /// <summary>
        /// Returns iterator over ring elements (for finite rings, otherwise throws exception)
        /// </summary>
        IEnumerator<E> Iterator();


        /// <summary>
        /// Returns a random element from this ring
        /// </summary>
        /// <returns>random element from this ring</returns>
        E RandomElement()
        {
            return RandomElement(Rings.privateRandom);
        }


        /// <summary>
        /// Returns a random element from this ring
        /// </summary>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random element from this ring</returns>
        E RandomElement(Random rnd)
        {
            return ValueOf(rnd.NextInt64());
        }


        /// <summary>
        /// If this ring has a complicated nested structure, this method guaranties that the resulting random element will
        /// reflect ring complicated structure, i.e. the result will be roughly as complicated as the ring is
        /// </summary>
        /// <returns>random element from this ring</returns>
        E RandomElementTree(Random rnd)
        {
            return RandomElement(rnd);
        }


        /// <summary>
        /// If this ring has a complicated nested structure, this method guaranties that the resulting random element will
        /// reflect ring complicated structure, i.e. the result will be roughly as complicated as the ring is
        /// </summary>
        /// <returns>random element from this ring</returns>
        E RandomElementTree()
        {
            return RandomElementTree(Rings.privateRandom);
        }


        /// <summary>
        /// Returns a random non zero element from this ring
        /// </summary>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random non zero element from this ring</returns>
        E RandomNonZeroElement(Random rnd)
        {
            E el;
            do
            {
                el = RandomElement(rnd);
            } while (IsZero(el));

            return el;
        }

        /// <summary>
        /// Parse string into ring element
        /// </summary>
        /// <param name="string">string</param>
        /// <returns>ring element</returns>
        /// <remarks>@seeCoder</remarks>
        new E Parse(string @string)
        {
            return Coder.MkCoder(this).Parse(@string);
        }
    }
}