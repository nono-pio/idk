using System.Numerics;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Io;
using Cc.Redberry.Rings.Poly.Multivar;
using Cc.Redberry.Rings.Util;
using System.Text;

namespace Cc.Redberry.Rings.Poly.Univar
{
    /// <summary>
    /// Univariate polynomial over generic ring.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class UnivariatePolynomial<E> : IUnivariatePolynomial<UnivariatePolynomial<E>>, IEnumerable<E>
    {
        private static readonly long serialVersionUID = 1;

        /// <summary>
        /// The coefficient ring
        /// </summary>
        public readonly Ring<E> ring;

        /// <summary>
        /// list of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        public E[] data;

        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        public int degree;

        public UnivariatePolynomial(Ring<E> ring, E[] data, int degree)
        {
            this.ring = ring;
            this.data = data;
            this.degree = degree;
        }


        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        public UnivariatePolynomial(Ring<E> ring, E[] data) : this(ring, data, data.Length - 1)
        {
            FixDegree();
        }


        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <param name="string">string expression</param>
        /// <param name="ring">the ring</param>
        /// <param name="var">variable string</param>
        public static UnivariatePolynomial<E> Parse(string @string, Ring<E> ring, string var)
        {
            return Coder.MkUnivariateCoder(Rings.UnivariateRing(ring), var).Parse(@string);
        }


        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, Ring, String)}</remarks>
        public static UnivariatePolynomial<E> Parse(string @string, Ring<E> ring)
        {
            return Coder.MkUnivariateCoder(Rings.UnivariateRing(ring), GuessVariableString(@string)).Parse(@string);
        }


        private static string GuessVariableString(string @string)
        {
            Matcher matcher = Pattern.Compile("[a-zA-Z]+[0-9]*").Matcher(@string);
            List<string> variables = [];
            HashSet<string> seen = [];
            while (matcher.Find())
            {
                string var = matcher.Group();
                if (seen.Contains(var))
                    continue;
                seen.Add(var);
                variables.Add(var);
            }

            return variables.Count == 0 ? "x" : variables[0];
        }

        /// <summary>
        /// Creates new univariate polynomial over specified ring with the specified coefficients. Note: the array {@code
        /// data} will not be copied.
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">the coefficients</param>
        /// <returns>new univariate polynomial over specified ring with specified coefficients</returns>
        public static UnivariatePolynomial<E> Create(Ring<E> ring, params E[] data)
        {
            ring.SetToValueOf(data);
            return new UnivariatePolynomial<E>(ring, data);
        }


        /// <summary>
        /// skips {@code ring.setToValueOf(data)}
        /// </summary>
        public static UnivariatePolynomial<E> CreateUnsafe(Ring<E> ring, E[] data)
        {
            return new UnivariatePolynomial(ring, data);
        }


        /// <summary>
        /// Creates univariate polynomial over specified ring (with integer elements) with the specified coefficients
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">the coefficients</param>
        /// <returns>new univariate polynomial over specified ring with specified coefficients</returns>
        public static UnivariatePolynomial<BigInteger> Create(Ring<BigInteger> ring, params long[] data)
        {
            return Create(ring, ring.ValueOf(data));
        }


        /// <summary>
        /// Creates new univariate Z[x] polynomial
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>new univariate Z[x] polynomial with specified coefficients</returns>
        public static UnivariatePolynomial<BigInteger> Create(params long[] data)
        {
            return Create(Rings.Z, data);
        }


        /// <summary>
        /// Creates constant polynomial over specified ring
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="constant">the value</param>
        /// <returns>constant polynomial over specified ring</returns>
        public static UnivariatePolynomial<E> Constant(Ring<E> ring, E constant)
        {
            return Create(ring, ring.CreateArray(constant));
        }


        /// <summary>
        /// Creates zero polynomial over specified ring
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial over specified ring</returns>
        public static UnivariatePolynomial<E> Zero(Ring<E> ring)
        {
            return Constant(ring, ring.GetZero());
        }


        /// <summary>
        /// Creates unit polynomial over specified ring
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial over specified ring</returns>
        public static UnivariatePolynomial<E> One(Ring<E> ring)
        {
            return Constant(ring, ring.GetOne());
        }


        /// <summary>
        /// Converts poly over BigIntegers to machine-sized polynomial in Z
        /// </summary>
        /// <param name="poly">the polynomial over BigIntegers</param>
        /// <returns>machine-sized polynomial in Z</returns>
        /// <exception cref="ArithmeticException">if some of coefficients is out of long range</exception>
        public static UnivariatePolynomialZ64 AsOverZ64(UnivariatePolynomial<BigInteger> poly)
        {
            long[] data = new long[poly.degree + 1];
            for (int i = 0; i < data.Length; i++)
                data[i] = poly.data[i].LongValueExact();
            return UnivariatePolynomialZ64.Create(data);
        }


        /// <summary>
        /// Converts Zp[x] poly over BigIntegers to machine-sized polynomial in Zp
        /// </summary>
        /// <param name="poly">the Z/p polynomial over BigIntegers</param>
        /// <returns>machine-sized polynomial in Z/p</returns>
        /// <exception cref="IllegalArgumentException">if {@code poly.ring} is not {@link IntegersZp}</exception>
        /// <exception cref="ArithmeticException">if some of {@code poly} elements is out of long range</exception>
        public static UnivariatePolynomialZp64 AsOverZp64(UnivariatePolynomial<BigInteger> poly)
        {
            if (!(poly.ring is IntegersZp))
                throw new ArgumentException("Not a modular ring: " + poly.ring);
            long[] data = new long[poly.degree + 1];
            for (int i = 0; i < data.Length; i++)
                data[i] = ((BigInteger)poly.data[i]).LongValueExact();
            return UnivariatePolynomialZp64.Create(((IntegersZp)poly.ring).modulus.LongValueExact(), data);
        }


        public static UnivariatePolynomialZp64 AsOverZp64(UnivariatePolynomial<BigInteger> poly, IntegersZp64 ring)
        {
            long modulus = ring.modulus;
            long[] data = new long[poly.degree + 1];
            for (int i = 0; i < data.Length; i++)
                data[i] = poly.data[i].Mod(modulus).LongValueExact();
            return UnivariatePolynomialZp64.Create(ring, data);
        }


        public static UnivariatePolynomialZp64 AsOverZp64Q(UnivariatePolynomial<Rational<BigInteger>> poly,
            IntegersZp64 ring)
        {
            long modulus = ring.modulus;
            long[] data = new long[poly.degree + 1];
            for (int i = 0; i < data.Length; i++)
                data[i] = ring.Divide(poly.data[i].Numerator().Mod(modulus).LongValueExact(),
                    poly.data[i].Denominator().Mod(modulus).LongValueExact());
            return UnivariatePolynomialZp64.Create(ring, data);
        }


        public static UnivariatePolynomial<BigInteger> AsPolyZSymmetric(UnivariatePolynomial<BigInteger> poly)
        {
            if (!(poly.ring is IntegersZp))
                throw new ArgumentException("Not a modular ring: " + poly.ring);
            IntegersZp ring = (IntegersZp)poly.ring;
            BigInteger[] newData = new BigInteger[poly.degree + 1];
            for (int i = poly.degree; i >= 0; --i)
                newData[i] = ring.SymmetricForm(poly.data[i]);
            return UnivariatePolynomial<BigInteger>.CreateUnsafe(Rings.Z, newData);
        }


        public int Degree()
        {
            return degree;
        }


        /// <summary>
        /// Returns i-th coefficient of this poly
        /// </summary>
        public E Get(int i)
        {
            return i > degree ? ring.GetZero() : data[i];
        }


        /// <summary>
        /// Sets i-th coefficient of this poly with specified value
        /// </summary>
        public UnivariatePolynomial<E> Set(int i, E el)
        {
            el = ring.ValueOf(el);
            if (ring.IsZero(el))
            {
                if (i > degree)
                    return this;
                data[i] = el;
                FixDegree();
                return this;
            }

            EnsureCapacity(i);
            data[i] = el;
            FixDegree();
            return this;
        }


        /// <summary>
        /// Sets the leading coefficient of this poly
        /// </summary>
        public UnivariatePolynomial<E> SetLC(E lc)
        {
            return Set(degree, lc);
        }


        /// <summary>
        /// Sets the leading coefficient of this poly
        /// </summary>
        public int FirstNonZeroCoefficientPosition()
        {
            if (IsZero())
                return -1;
            int i = 0;
            while (ring.IsZero(data[i]))
                ++i;
            return i;
        }


        /// <summary>
        /// Returns a copy of this with elements reduced to a new coefficient ring
        /// </summary>
        /// <param name="newRing">the new ring</param>
        /// <returns>a copy of this with elements reduced to a new coefficient ring</returns>
        public UnivariatePolynomial<E> SetRing(Ring<E> newRing)
        {
            if (ring == newRing)
                return Clone();
            E[] newData = Arrays.CopyOf(data, degree + 1);
            newRing.SetToValueOf(newData);
            return new UnivariatePolynomial<E>(newRing, newData);
        }


        /// <summary>
        /// internal API
        /// </summary>
        public UnivariatePolynomial<E> SetRingUnsafe(Ring<E> newRing)
        {
            return new UnivariatePolynomial<E>(newRing, data, degree);
        }


        /// <summary>
        /// Returns the leading coefficient
        /// </summary>
        /// <returns>leading coefficient</returns>
        public E Lc()
        {
            return data[degree];
        }

        public UnivariatePolynomial<E> LcAsPoly()
        {
            return CreateConstant(Lc());
        }


        public UnivariatePolynomial<E> CcAsPoly()
        {
            return CreateConstant(Cc());
        }


        public UnivariatePolynomial<E> GetAsPoly(int i)
        {
            return CreateConstant(Get(i));
        }


        /// <summary>
        /// Returns the constant coefficient
        /// </summary>
        /// <returns>constant coefficient</returns>
        public E Cc()
        {
            return data[0];
        }


        public void EnsureInternalCapacity(int desiredCapacity)
        {
            if (data.Length < desiredCapacity)
            {
                int oldLength = data.Length;
                data = Arrays.CopyOf(data, desiredCapacity);
                FillZeroes(data, oldLength, data.Length);
            }
        }


        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        public void EnsureCapacity(int desiredDegree)
        {
            if (degree < desiredDegree)
                degree = desiredDegree;
            if (data.Length < (desiredDegree + 1))
            {
                int oldLen = data.Length;
                data = Arrays.CopyOf(data, desiredDegree + 1);
                FillZeroes(data, oldLen, data.Length);
            }
        }


        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        public void FixDegree()
        {
            int i = degree;
            while (i >= 0 && ring.IsZero(data[i]))
                --i;
            if (i < 0)
                i = 0;
            if (i != degree)
            {
                degree = i; // not necessary to fillZeroes here!
                // fillZeroes(data, degree + 1, data.length);
            }
        }


        public UnivariatePolynomial<E> GetRange(int from, int to)
        {
            return new UnivariatePolynomial<E>(ring, Arrays.CopyOfRange(data, from, to));
        }


        public UnivariatePolynomial<E>[] CreateArray(int length)
        {
            return new UnivariatePolynomial<E>[length];
        }


        public UnivariatePolynomial<E>[] CreateArray(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
        {
            return new UnivariatePolynomial<E>[]
            {
                a,
                b
            };
        }

        public UnivariatePolynomial<E>[,] CreateArray2d(int length)
        {
            return new UnivariatePolynomial<E>[length];
        }


        public UnivariatePolynomial<E>[,] CreateArray2d(int length1, int length2)
        {
            return new UnivariatePolynomial<E>[length1, length2];
        }


        public bool SameCoefficientRingWith(UnivariatePolynomial<E> oth)
        {
            return ring.Equals(oth.ring);
        }

        public UnivariatePolynomial<E> SetCoefficientRingFrom(UnivariatePolynomial<E> poly)
        {
            return SetRing(poly.ring);
        }


        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        public UnivariatePolynomial<E> CreateFromArray(E[] data)
        {
            ring.SetToValueOf(data);
            return new UnivariatePolynomial<E>(ring, data);
        }


        public UnivariatePolynomial<E> CreateMonomial(int degree)
        {
            return CreateMonomial(ring.GetOne(), degree);
        }


        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc} (over the same ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        public UnivariatePolynomial<E> CreateLinear(E cc, E lc)
        {
            return CreateFromArray(ring.CreateArray(cc, lc));
        }


        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (over the same ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        public UnivariatePolynomial<E> CreateMonomial(E coefficient, int degree)
        {
            coefficient = ring.ValueOf(coefficient);
            E[] data = ring.CreateZeroesArray(degree + 1);
            data[degree] = coefficient;
            return new UnivariatePolynomial<E>(ring, data);
        }


        /// <summary>
        /// Creates constant polynomial with specified value (over the same ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        public UnivariatePolynomial<E> CreateConstant(E val)
        {
            E[] array = ring.CreateArray(1);
            array[0] = val;
            return CreateFromArray(array);
        }

        public UnivariatePolynomial<E> CreateZero()
        {
            return CreateConstant(ring.GetZero());
        }


        public UnivariatePolynomial<E> CreateOne()
        {
            return CreateConstant(ring.GetOne());
        }


        public bool IsZeroAt(int i)
        {
            return i >= data.Length || ring.IsZero(data[i]);
        }


        public UnivariatePolynomial<E> SetZero(int i)
        {
            if (i < data.Length)
                data[i] = ring.GetZero();
            return this;
        }


        public UnivariatePolynomial<E> SetFrom(int indexInThis, UnivariatePolynomial<E> poly, int indexInPoly)
        {
            EnsureCapacity(indexInThis);
            data[indexInThis] = poly[indexInPoly];
            FixDegree();
            return this;
        }


        public bool IsZero()
        {
            return ring.IsZero(data[degree]);
        }


        public bool IsOne()
        {
            return degree == 0 && ring.IsOne(data[0]);
        }


        public bool IsMonic()
        {
            return ring.IsOne(Lc());
        }


        public bool IsUnitCC()
        {
            return ring.IsOne(Cc());
        }


        public bool IsConstant()
        {
            return degree == 0;
        }


        public bool IsMonomial()
        {
            for (int i = degree - 1; i >= 0; --i)
                if (!ring.IsZero(data[i]))
                    return false;
            return true;
        }


        public int SignumOfLC()
        {
            return ring.Signum(Lc());
        }


        public bool IsOverField()
        {
            return ring.IsField();
        }


        public bool IsOverFiniteField()
        {
            return ring.IsFinite();
        }


        public bool IsOverZ()
        {
            return ring.Equals(Rings.Z);
        }


        public BigInteger CoefficientRingCardinality()
        {
            return ring.Cardinality();
        }


        public BigInteger CoefficientRingCharacteristic()
        {
            return ring.Characteristic();
        }


        public bool IsOverPerfectPower()
        {
            return ring.IsPerfectPower();
        }


        public BigInteger CoefficientRingPerfectPowerBase()
        {
            return ring.PerfectPowerBase();
        }


        public BigInteger CoefficientRingPerfectPowerExponent()
        {
            return ring.PerfectPowerExponent();
        }


        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|) of the poly
        /// </summary>
        public static BigInteger MignotteBound(UnivariatePolynomial<BigInteger> poly)
        {
            return BigInteger.One.ShiftLeft(poly.degree).Multiply(Norm2(poly));
        }


        /// <summary>
        /// Returns L1 norm of the polynomial, i.e. sum of abs coefficients
        /// </summary>
        public static BigInteger Norm1(UnivariatePolynomial<BigInteger> poly)
        {
            BigInteger norm = BigInteger.Zero;
            for (int i = poly.degree; i >= 0; --i)
                norm = norm.Add(BigInteger.Abs(poly.data[i]));
            return norm;
        }


        /// <summary>
        /// Returns L2 norm of the polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        public static BigInteger Norm2(UnivariatePolynomial<BigInteger> poly)
        {
            BigInteger norm = BigInteger.Zero;
            for (int i = poly.degree; i >= 0; --i)
                norm = norm.Add(poly.data[i].Multiply(poly.data[i]));
            return BigIntegerUtil.SqrtCeil(norm);
        }


        /// <summary>
        /// Returns L2 norm of the poly, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        public static double Norm2Double(UnivariatePolynomial<BigInteger> poly)
        {
            double norm = 0;
            for (int i = poly.degree; i >= 0; --i)
            {
                double d = poly.data[i].DoubleValue();
                norm += d * d;
            }

            return Math.Sqrt(norm);
        }


        /// <summary>
        /// Returns max abs coefficient of the poly
        /// </summary>
        public E MaxAbsCoefficient()
        {
            E el = ring.Abs(data[0]);
            for (int i = 1; i <= degree; i++)
                el = ring.Max(el, ring.Abs(data[i]));
            return el;
        }


        public E NormMax()
        {
            return MaxAbsCoefficient();
        }


        private void FillZeroes(E[] data, int from, int to)
        {
            for (int i = from; i < to; ++i)
                data[i] = ring.GetZero(); //invoke getZero() at each cycle
        }


        public UnivariatePolynomial<E> ToZero()
        {
            FillZeroes(data, 0, degree + 1);
            degree = 0;
            return this;
        }

        public UnivariatePolynomial<E> Set(UnivariatePolynomial<E> oth)
        {
            if (oth == this)
                return this;
            this.data = (E[])oth.data.Clone();
            this.degree = oth.degree;
            return this;
        }


        public UnivariatePolynomial<E> SetAndDestroy(UnivariatePolynomial<E> oth)
        {
            this.data = oth.data;
            oth.data = null; // destroy
            this.degree = oth.degree;
            return this;
        }


        public UnivariatePolynomial<E> ShiftLeft(int offset)
        {
            if (offset == 0)
                return this;
            if (offset > degree)
                return ToZero();
            System.Arraycopy(data, offset, data, 0, degree - offset + 1);
            FillZeroes(data, degree - offset + 1, degree + 1);
            degree = degree - offset;
            return this;
        }


        public UnivariatePolynomial<E> ShiftRight(int offset)
        {
            if (offset == 0)
                return this;
            int degree = this.degree;
            EnsureCapacity(offset + degree);
            System.Arraycopy(data, 0, data, offset, degree + 1);
            FillZeroes(data, 0, offset);
            return this;
        }


        public UnivariatePolynomial<E> Truncate(int newDegree)
        {
            if (newDegree >= degree)
                return this;
            FillZeroes(data, newDegree + 1, degree + 1);
            degree = newDegree;
            FixDegree();
            return this;
        }


        public UnivariatePolynomial<E> Reverse()
        {
            ArraysUtil.Reverse(data, 0, degree + 1);
            FixDegree();
            return this;
        }


        /// <summary>
        /// Returns the content of the poly
        /// </summary>
        /// <returns>polynomial content</returns>
        public E Content()
        {
            if (degree == 0)
                return data[0];
            return IsOverField() ? Lc() : ring.Gcd(this); //        E gcd = data[degree];
            //        for (int i = degree - 1; i >= 0; --i)
            //            gcd = ring.gcd(gcd, data[i]);
            //        return gcd;
        }


        public UnivariatePolynomial<E> ContentAsPoly()
        {
            return CreateConstant(Content());
        }


        public UnivariatePolynomial<E> PrimitivePart()
        {
            E content = Content();
            if (SignumOfLC() < 0 && ring.Signum(content) > 0)
                content = ring.Negate(content);
            if (ring.IsMinusOne(content))
                return Negate();
            return PrimitivePart0(content);
        }


        public UnivariatePolynomial<E> PrimitivePartSameSign()
        {
            return PrimitivePart0(Content());
        }

        private UnivariatePolynomial<E> PrimitivePart0(E content)
        {
            if (IsZero())
                return this;
            if (ring.IsOne(content))
                return this;
            for (int i = degree; i >= 0; --i)
            {
                data[i] = ring.DivideOrNull(data[i], content);
                if (data[i] == null)
                    return null;
            }

            return this;
        }


        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        public E Evaluate(long point)
        {
            return Evaluate(ring.ValueOf(point));
        }


        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        public E Evaluate(E point)
        {
            if (ring.IsZero(point))
                return Cc();
            point = ring.ValueOf(point);
            E res = ring.GetZero();
            for (int i = degree; i >= 0; --i)
                res = ring.AddMutable(ring.MultiplyMutable(res, point), data[i]);
            return res;
        }


        public UnivariatePolynomial<E> Composition(UnivariatePolynomial<E> value)
        {
            if (value.IsOne())
                return this.Clone();
            if (value.IsZero())
                return CcAsPoly();
            if (value.degree == 1 && value.IsMonomial() && ring.IsOne(value.Lc()))
                return Clone();
            UnivariatePolynomial<E> result = CreateZero();
            for (int i = degree; i >= 0; --i)
                result = result.Multiply(value).Add(data[i]);
            return result;
        }


        public MultivariatePolynomial<E> Composition(AMultivariatePolynomial<E> value)
        {
            if (!(value is MultivariatePolynomial))
                throw new ArgumentException();
            if (!((MultivariatePolynomial)value).ring.Equals(ring))
                throw new ArgumentException();
            if (value.IsOne())
                return AsMultivariate();
            if (value.IsZero())
                return CcAsPoly().AsMultivariate();
            MultivariatePolynomial<E> result = (MultivariatePolynomial<E>)value.CreateZero();
            for (int i = degree; i >= 0; --i)
                result = result.Multiply((MultivariatePolynomial<E>)value).Add(data[i]);
            return result;
        }


        /// <summary>
        /// Replaces x -> scale * x and returns a copy
        /// </summary>
        public UnivariatePolynomial<E> Scale(E scaling)
        {
            if (ring.IsOne(scaling))
                return this.Clone();
            if (ring.IsZero(scaling))
                return CcAsPoly();
            E factor = ring.GetOne();
            E[] result = ring.CreateArray(degree + 1);
            for (int i = 0; i <= degree; ++i)
            {
                result[i] = ring.Multiply(data[i], factor);
                factor = ring.Multiply(factor, scaling);
            }

            return CreateUnsafe(ring, result);
        }


        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        public UnivariatePolynomial<E> Shift(E value)
        {
            return Composition(CreateLinear(value, ring.GetOne()));
        }


        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        public UnivariatePolynomial<E> Add(E val)
        {
            data[0] = ring.Add(data[0], ring.ValueOf(val));
            FixDegree();
            return this;
        }


        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this - val</returns>
        public UnivariatePolynomial<E> Subtract(E val)
        {
            data[0] = ring.Subtract(data[0], ring.ValueOf(val));
            FixDegree();
            return this;
        }


        public UnivariatePolynomial<E> Decrement()
        {
            return Subtract(CreateOne());
        }


        public UnivariatePolynomial<E> Increment()
        {
            return Add(CreateOne());
        }


        public UnivariatePolynomial<E> Add(UnivariatePolynomial<E> oth)
        {
            AssertSameCoefficientRingWith(oth);
            if (oth.IsZero())
                return this;
            if (IsZero())
                return Set(oth);
            AssertSameCoefficientRingWith(oth);
            EnsureCapacity(oth.degree);
            for (int i = oth.degree; i >= 0; --i)
                data[i] = ring.Add(data[i], oth.data[i]);
            FixDegree();
            return this;
        }


        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        public UnivariatePolynomial<E> AddMonomial(E coefficient, int exponent)
        {
            if (ring.IsZero(coefficient))
                return this;
            EnsureCapacity(exponent);
            data[exponent] = ring.Add(data[exponent], ring.ValueOf(coefficient));
            FixDegree();
            return this;
        }


        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        public UnivariatePolynomial<E> AddMul(UnivariatePolynomial<E> oth, E factor)
        {
            AssertSameCoefficientRingWith(oth);
            if (oth.IsZero())
                return this;
            factor = ring.ValueOf(factor);
            if (ring.IsZero(factor))
                return this;
            AssertSameCoefficientRingWith(oth);
            EnsureCapacity(oth.degree);
            for (int i = oth.degree; i >= 0; --i)
                data[i] = ring.Add(data[i], ring.Multiply(factor, oth.data[i]));
            FixDegree();
            return this;
        }


        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        public UnivariatePolynomial<E> Subtract(UnivariatePolynomial<E> oth)
        {
            AssertSameCoefficientRingWith(oth);
            if (oth.IsZero())
                return this;
            if (IsZero())
                return Set(oth).Negate();
            AssertSameCoefficientRingWith(oth);
            EnsureCapacity(oth.degree);
            for (int i = oth.degree; i >= 0; --i)
                data[i] = ring.Subtract(data[i], oth.data[i]);
            FixDegree();
            return this;
        }


        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        public UnivariatePolynomial<E> Subtract(UnivariatePolynomial<E> oth, E factor, int exponent)
        {
            AssertSameCoefficientRingWith(oth);
            if (oth.IsZero())
                return this;
            factor = ring.ValueOf(factor);
            if (ring.IsZero(factor))
                return this;
            AssertSameCoefficientRingWith(oth);
            for (int i = oth.degree + exponent; i >= exponent; --i)
                data[i] = ring.Subtract(data[i], ring.Multiply(factor, oth.data[i - exponent]));
            FixDegree();
            return this;
        }


        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        public UnivariatePolynomial<E> Negate()
        {
            for (int i = degree; i >= 0; --i)
                if (!ring.IsZero(data[i]))
                    data[i] = ring.Negate(data[i]);
            return this;
        }


        /// <summary>
        /// Multiplies {@code this} by the {@code factor}
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code} this multiplied by the {@code factor}</returns>
        public UnivariatePolynomial<E> Multiply(E factor)
        {
            factor = ring.ValueOf(factor);
            if (ring.IsOne(factor))
                return this;
            if (ring.IsZero(factor))
                return ToZero();
            for (int i = degree; i >= 0; --i)
                data[i] = ring.Multiply(data[i], factor);
            return this;
        }


        /// <summary>
        /// Multiplies {@code this} by the {@code factor}
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code} this multiplied by the {@code factor}</returns>
        public UnivariatePolynomial<E> MultiplyByLC(UnivariatePolynomial<E> other)
        {
            return Multiply(other.Lc());
        }


        /// <summary>
        /// Multiplies {@code this} by the {@code factor}
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code} this multiplied by the {@code factor}</returns>
        public UnivariatePolynomial<E> Multiply(long factor)
        {
            return Multiply(ring.ValueOf(factor));
        }


        /// <summary>
        /// Multiplies {@code this} by the {@code factor}
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code} this multiplied by the {@code factor}</returns>
        public UnivariatePolynomial<E> DivideByLC(UnivariatePolynomial<E> other)
        {
            return DivideOrNull(other.Lc());
        }


        /// <summary>
        /// Divides this polynomial by a {@code factor} or returns {@code null} (causing loss of internal data) if some of
        /// the elements can't be exactly divided by the {@code factor}. NOTE: if {@code null} is returned, the content of
        /// {@code this} is destroyed.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this} divided by the {@code factor} or {@code null}</returns>
        public UnivariatePolynomial<E> DivideOrNull(E factor)
        {
            factor = ring.ValueOf(factor);
            if (ring.IsZero(factor))
                throw new ArithmeticException("Divide by zero");
            if (ring.IsOne(factor))
                return this;
            if (ring.IsMinusOne(factor))
                return Negate();
            if (ring.IsField())
                return Multiply(ring.Reciprocal(factor));
            for (int i = degree; i >= 0; --i)
            {
                E l = ring.DivideOrNull(data[i], factor);
                if (l == null)
                    return null;
                data[i] = l;
            }

            return this;
        }


        /// <summary>
        /// Divides this polynomial by a {@code factor} or throws exception if exact division is not possible
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this} divided by the {@code factor}</returns>
        /// <exception cref="ArithmeticException">if exact division is not possible</exception>
        public UnivariatePolynomial<E> DivideExact(E factor)
        {
            UnivariatePolynomial<E> r = DivideOrNull(factor);
            if (r == null)
                throw new ArithmeticException("not divisible " + this + " / " + factor);
            return r;
        }


        /// <summary>
        /// Divides this polynomial by a {@code factor} or throws exception if exact division is not possible
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this} divided by the {@code factor}</returns>
        /// <exception cref="ArithmeticException">if exact division is not possible</exception>
        public UnivariatePolynomial<E> Monic()
        {
            if (IsZero())
                return this;
            return DivideOrNull(Lc());
        }


        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor}.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        public UnivariatePolynomial<E> Monic(E factor)
        {
            E lc = Lc();
            return Multiply(factor).DivideOrNull(lc);
        }


        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor}.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        public UnivariatePolynomial<E> MonicWithLC(UnivariatePolynomial<E> other)
        {
            if (Lc().Equals(other.Lc()))
                return this;
            return Monic(other.Lc());
        }


        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor}.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        public UnivariatePolynomial<E> MultiplyByBigInteger(BigInteger factor)
        {
            return Multiply(ring.ValueOfBigInteger(factor));
        }


        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor}.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        public UnivariatePolynomial<E> Multiply(UnivariatePolynomial<E> oth)
        {
            AssertSameCoefficientRingWith(oth);
            if (IsZero())
                return this;
            if (oth.IsZero())
                return ToZero();
            if (this == oth)
                return Square();
            if (oth.degree == 0)
                return Multiply(oth.data[0]);
            if (degree == 0)
            {
                E factor = data[0];
                data = (E[])oth.data.Clone();
                degree = oth.degree;
                return Multiply(factor);
            }

            if (ring is IntegersZp)
            {
                // faster method with exact operations
                UnivariatePolynomial<E> iThis = SetRingUnsafe((Ring<E>)Rings.Z),
                    iOth = oth.SetRingUnsafe((Ring<E>)Rings.Z);
                data = iThis.Multiply(iOth).data;
                ring.SetToValueOf(data);
            }
            else
                data = MultiplySafe0(oth);

            degree += oth.degree;
            FixDegree();
            return this;
        }


        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor}.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        // faster method with exact operations
        public UnivariatePolynomial<E> Square()
        {
            if (IsZero())
                return this;
            if (degree == 0)
                return Multiply(data[0]);
            if (ring is IntegersZp)
            {
                // faster method with exact operations
                UnivariatePolynomial<E> iThis = SetRingUnsafe((Ring<E>)Rings.Z);
                data = iThis.Square().data;
                ring.SetToValueOf(data);
            }
            else
                data = SquareSafe0();

            degree += degree;
            FixDegree();
            return this;
        }


        public UnivariatePolynomial<E> Derivative()
        {
            if (IsConstant())
                return CreateZero();
            E[] newData = ring.CreateArray(degree);
            for (int i = degree; i > 0; --i)
                newData[i - 1] = ring.Multiply(data[i], ring.ValueOf(i));
            return CreateFromArray(newData);
        }


        public UnivariatePolynomial<E> Clone()
        {
            return new UnivariatePolynomial<E>(ring, (E[])data.Clone(), degree);
        }


        public UnivariatePolynomial<E> ParsePoly(string @string)
        {
            return Parse(@string, ring);
        }


        public IEnumerator<E> Iterator()
        {
            return new It();
        }


        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        public IEnumerable<E> Stream()
        {
            return Arrays.Stream(data, 0, degree + 1);
        }


        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        public IEnumerable<UnivariatePolynomial<E>> StreamAsPolys()
        {
            return Stream().Select(this.CreateConstant);
        }


        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().map(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        public UnivariatePolynomial<T> MapCoefficients<T>(Ring<T> ring, Func<E, T> mapper)
        {
            return new UnivariatePolynomial<T>(ring, Stream().Select(mapper).ToArray());
        }


        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().map(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        public UnivariatePolynomialZp64 MapCoefficients(IntegersZp64 ring, Func<E, long> mapper)
        {
            return UnivariatePolynomialZp64.Create(ring, Stream().Select(mapper).ToArray());
        }


        /// <summary>
        /// internal API
        /// </summary>
        public E[] GetDataReferenceUnsafe()
        {
            return data;
        }


        public MultivariatePolynomial<E> AsMultivariate()
        {
            return AsMultivariate(MonomialOrder.DEFAULT);
        }


        public MultivariatePolynomial<E> AsMultivariate(Comparator<DegreeVector> ordering)
        {
            return MultivariatePolynomial<E>.AsMultivariate(this, 1, 0, ordering);
        }


        public int CompareTo(UnivariatePolynomial<E>? o)
        {
            int c = degree.CompareTo(o.degree);
            if (c != 0)
                return c;
            for (int i = degree; i >= 0; --i)
            {
                c = ring.Compare(data[i], o.data[i]);
                if (c != 0)
                    return c;
            }

            return 0;
        }


        public string CoefficientRingToString(IStringifier<UnivariatePolynomial<E>> stringifier)
        {
            return ring.ToString(stringifier.Substringifier(ring));
        }


        public string ToString()
        {
            return ToString(IStringifier<UnivariatePolynomial<E>>.Dummy<UnivariatePolynomial<E>>());
        }


        public string ToString(IStringifier<UnivariatePolynomial<E>> stringifier)
        {
            IStringifier<E> cfStringifier = stringifier.Substringifier(ring);
            if (IsConstant())
                return cfStringifier.Stringify(Cc());
            string varString = stringifier.GetBindings()
                .GetValueOrDefault(CreateMonomial(1), IStringifier<UnivariatePolynomial<E>>.DefaultVar());
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i <= degree; i++)
            {
                E el = data[i];
                if (ring.IsZero(el))
                    continue;
                string cfString;
                if (!ring.IsOne(el) || i == 0)
                    cfString = cfStringifier.Stringify(el);
                else
                    cfString = "";
                if (i != 0 && IStringifier<UnivariatePolynomial<E>>.NeedParenthesisInSum(cfString))
                    cfString = "(" + cfString + ")";
                if (sb.Length != 0 && !cfString.StartsWith("-"))
                    sb.Append("+");
                sb.Append(cfString);
                if (i == 0)
                    continue;
                if (cfString.Length != 0)
                    sb.Append("*");
                sb.Append(varString);
                if (i > 1)
                    sb.Append("^").Append(i);
            }

            return sb.ToString();
        }


        string ToStringForCopy()
        {
            string s = ArraysUtil.ToString(data, 0, degree + 1, (x) => "new BigInteger(\"" + x + "\")");
            return "of(" + s.Substring(1, s.Length - 1) + ")";
        }


        public bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType())
                return false;
            UnivariatePolynomial<E> oth = (UnivariatePolynomial<E>)obj;
            if (degree != oth.degree)
                return false;
            for (int i = 0; i <= degree; ++i)
                if (!(data[i].Equals(oth.data[i])))
                    return false;
            return true;
        }


        public int GetHashCode()
        {
            int result = 1;
            for (int i = degree; i >= 0; --i)
                result = 31 * result + data[i].GetHashCode();
            return result;
        }


        /* =========================== Exact multiplication with safe arithmetics =========================== */
        /// <summary>
        /// switch to classical multiplication
        /// </summary>
        static readonly long KARATSUBA_THRESHOLD = 1024;


        /// <summary>
        /// when use Classical/Karatsuba/Schoenhage-Strassen fast multiplication
        /// </summary>
        static readonly long MUL_CLASSICAL_THRESHOLD = 256 * 256,
            MUL_KRONECKER_THRESHOLD = 32 * 32,
            MUL_MOD_CLASSICAL_THRESHOLD = 128 * 128;


        /// <summary>
        /// switch algorithms
        /// </summary>
        E[] MultiplySafe0(UnivariatePolynomial<E> oth)
        {
            long md = 1 * (degree + 1) * (oth.degree + 1);
            if (IsOverZ() && md >= MUL_KRONECKER_THRESHOLD)
                return (E[])MultiplyKronecker0((UnivariatePolynomial)this, (UnivariatePolynomial)oth);
            if (md <= MUL_CLASSICAL_THRESHOLD)
                return MultiplyClassicalSafe(data, 0, degree + 1, oth.data, 0, oth.degree + 1);
            else
                return MultiplyKaratsubaSafe(data, 0, degree + 1, oth.data, 0, oth.degree + 1);
        }


        /// <summary>
        /// switch algorithms
        /// </summary>
        E[] SquareSafe0()
        {
            long md = 1 * (degree + 1) * (degree + 1);
            if (IsOverZ() && md >= MUL_KRONECKER_THRESHOLD)
                return (E[])SquareKronecker0((UnivariatePolynomial)this);
            if (md <= MUL_CLASSICAL_THRESHOLD)
                return SquareClassicalSafe(data, 0, degree + 1);
            else
                return SquareKaratsubaSafe(data, 0, degree + 1);
        }


        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <returns>the result</returns>
        E[] MultiplyClassicalSafe(E[] a, int aFrom, int aTo, E[] b, int bFrom, int bTo)
        {
            E[] result = ring.CreateZeroesArray(aTo - aFrom + bTo - bFrom - 1);
            MultiplyClassicalSafe(result, a, aFrom, aTo, b, bFrom, bTo);
            return result;
        }


        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="result">where to write the result</param>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        void MultiplyClassicalSafe(E[] result, E[] a, int aFrom, int aTo, E[] b, int bFrom, int bTo)
        {
            if (aTo - aFrom > bTo - bFrom)
            {
                MultiplyClassicalSafe(result, b, bFrom, bTo, a, aFrom, aTo);
                return;
            }

            for (int i = 0; i < aTo - aFrom; ++i)
            {
                E c = a[aFrom + i];
                if (!ring.IsZero(c))
                    for (int j = 0; j < bTo - bFrom; ++j)
                        result[i + j] = ring.AddMutable(result[i + j], ring.Multiply(c, b[bFrom + j]));
            }
        }


        /// <summary>
        /// Karatsuba multiplication
        /// </summary>
        /// <param name="f">the first multiplier</param>
        /// <param name="g">the second multiplier</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <param name="gFrom">begin in g</param>
        /// <param name="gTo">end in g</param>
        /// <returns>the result</returns>
        E[] MultiplyKaratsubaSafe(E[] f, int fFrom, int fTo, E[] g, int gFrom, int gTo)
        {
            // return zero
            if (fFrom >= fTo || gFrom >= gTo)
                return ring.CreateArray(0);

            // single element in f
            if (fTo - fFrom == 1)
            {
                E[] result = ring.CreateArray(gTo - gFrom);
                for (int i = gFrom; i < gTo; ++i)
                    result[i - gFrom] = ring.Multiply(f[fFrom], g[i]);
                return result;
            }


            // single element in g
            if (gTo - gFrom == 1)
            {
                E[] result = ring.CreateArray(fTo - fFrom);

                //single element in b
                for (int i = fFrom; i < fTo; ++i)
                    result[i - fFrom] = ring.Multiply(g[gFrom], f[i]);
                return result;
            }


            // linear factors
            if (fTo - fFrom == 2 && gTo - gFrom == 2)
            {
                E[] result = ring.CreateArray(3);

                //both a and b are linear
                result[0] = ring.Multiply(f[fFrom], g[gFrom]);
                result[1] = ring.AddMutable(ring.Multiply(f[fFrom], g[gFrom + 1]),
                    ring.Multiply(f[fFrom + 1], g[gFrom]));
                result[2] = ring.Multiply(f[fFrom + 1], g[gFrom + 1]);
                return result;
            }


            //switch to classical
            if (1 * (fTo - fFrom) * (gTo - gFrom) < KARATSUBA_THRESHOLD)
                return MultiplyClassicalSafe(g, gFrom, gTo, f, fFrom, fTo);
            if (fTo - fFrom < gTo - gFrom)
                return MultiplyKaratsubaSafe(g, gFrom, gTo, f, fFrom, fTo);

            //we now split a and b into 2 parts:
            int split = (fTo - fFrom + 1) / 2;

            //if we can't split b
            if (gFrom + split >= gTo)
            {
                E[] f0g = MultiplyKaratsubaSafe(f, fFrom, fFrom + split, g, gFrom, gTo);
                E[] f1g = MultiplyKaratsubaSafe(f, fFrom + split, fTo, g, gFrom, gTo);
                int oldLen = f0g.Length, newLen = fTo - fFrom + gTo - gFrom - 1;
                E[] result = Arrays.CopyOf(f0g, newLen);
                FillZeroes(result, oldLen, newLen);
                for (int i = 0; i < f1g.Length; i++)
                    result[i + split] = ring.AddMutable(result[i + split], f1g[i]);
                return result;
            }

            int fMid = fFrom + split, gMid = gFrom + split;
            E[] f0g0 = MultiplyKaratsubaSafe(f, fFrom, fMid, g, gFrom, gMid);
            E[] f1g1 = MultiplyKaratsubaSafe(f, fMid, fTo, g, gMid, gTo);

            // f0 + f1
            E[] f0_plus_f1 = ring.CreateArray(Math.Max(fMid - fFrom, fTo - fMid));
            System.Arraycopy(f, fFrom, f0_plus_f1, 0, fMid - fFrom);
            FillZeroes(f0_plus_f1, fMid - fFrom, f0_plus_f1.Length);
            for (int i = fMid; i < fTo; ++i)
                f0_plus_f1[i - fMid] = ring.Add(f0_plus_f1[i - fMid], f[i]);

            //g0 + g1
            E[] g0_plus_g1 = ring.CreateArray(Math.Max(gMid - gFrom, gTo - gMid));
            System.Arraycopy(g, gFrom, g0_plus_g1, 0, gMid - gFrom);
            FillZeroes(g0_plus_g1, gMid - gFrom, g0_plus_g1.Length);
            for (int i = gMid; i < gTo; ++i)
                g0_plus_g1[i - gMid] = ring.Add(g0_plus_g1[i - gMid], g[i]);
            E[] mid = MultiplyKaratsubaSafe(f0_plus_f1, 0, f0_plus_f1.Length, g0_plus_g1, 0, g0_plus_g1.Length);
            if (mid.Length < f0g0.Length)
            {
                int oldLen = mid.Length;
                mid = Arrays.CopyOf(mid, f0g0.Length);
                FillZeroes(mid, oldLen, mid.Length);
            }

            if (mid.Length < f1g1.Length)
            {
                int oldLen = mid.Length;
                mid = Arrays.CopyOf(mid, f1g1.Length);
                FillZeroes(mid, oldLen, mid.Length);
            }


            //subtract f0g0, f1g1
            for (int i = 0; i < f0g0.Length; ++i)
                mid[i] = ring.SubtractMutable(mid[i], f0g0[i]);
            for (int i = 0; i < f1g1.Length; ++i)
                mid[i] = ring.SubtractMutable(mid[i], f1g1[i]);
            int oldLen = f0g0.Length;
            E[] result = Arrays.CopyOf(f0g0, (fTo - fFrom) + (gTo - gFrom) - 1);
            FillZeroes(result, oldLen, result.Length);
            for (int i = 0; i < mid.Length; ++i)
                result[i + split] = ring.AddMutable(result[i + split], mid[i]);
            for (int i = 0; i < f1g1.Length; ++i)
                result[i + 2 * split] = ring.AddMutable(result[i + 2 * split], f1g1[i]);
            return result;
        }


        E[] SquareClassicalSafe(E[] a, int from, int to)
        {
            E[] x = ring.CreateZeroesArray((to - from) * 2 - 1);
            SquareClassicalSafe(x, a, from, to);
            return x;
        }


        /// <summary>
        /// Square the poly {@code data} using classical algorithm
        /// </summary>
        /// <param name="result">result destination</param>
        /// <param name="data">the data</param>
        /// <param name="from">data from</param>
        /// <param name="to">end point in the {@code data}</param>
        void SquareClassicalSafe(E[] result, E[] data, int from, int to)
        {
            int len = to - from;
            for (int i = 0; i < len; ++i)
            {
                E c = data[from + i];
                if (!ring.IsZero(c))
                    for (int j = 0; j < len; ++j)
                        result[i + j] = ring.AddMutable(result[i + j], ring.Multiply(c, data[from + j]));
            }
        }


        /// <summary>
        /// Karatsuba squaring
        /// </summary>
        /// <param name="f">the data</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <returns>the result</returns>
        E[] SquareKaratsubaSafe(E[] f, int fFrom, int fTo)
        {
            if (fFrom >= fTo)
                return ring.CreateArray(0);
            if (fTo - fFrom == 1)
            {
                E[] r = ring.CreateArray(1);
                r[0] = ring.Multiply(f[fFrom], f[fFrom]);
                return r;
            }

            if (fTo - fFrom == 2)
            {
                E[] result = ring.CreateArray(3);
                result[0] = ring.Multiply(f[fFrom], f[fFrom]);
                result[1] = ring.MultiplyMutable(ring.Multiply(f[fFrom], f[fFrom + 1]), ring.ValueOf(2));
                result[2] = ring.Multiply(f[fFrom + 1], f[fFrom + 1]);
                return result;
            }


            //switch to classical
            if (1 * (fTo - fFrom) * (fTo - fFrom) < KARATSUBA_THRESHOLD)
                return SquareClassicalSafe(f, fFrom, fTo);

            //we now split a and b into 2 parts:
            int split = (fTo - fFrom + 1) / 2;
            int fMid = fFrom + split;
            E[] f0g0 = SquareKaratsubaSafe(f, fFrom, fMid);
            E[] f1g1 = SquareKaratsubaSafe(f, fMid, fTo);

            // f0 + f1
            E[] f0_plus_f1 = ring.CreateArray(Math.Max(fMid - fFrom, fTo - fMid));
            System.Arraycopy(f, fFrom, f0_plus_f1, 0, fMid - fFrom);
            FillZeroes(f0_plus_f1, fMid - fFrom, f0_plus_f1.Length);
            for (int i = fMid; i < fTo; ++i)
                f0_plus_f1[i - fMid] = ring.Add(f0_plus_f1[i - fMid], f[i]);
            E[] mid = SquareKaratsubaSafe(f0_plus_f1, 0, f0_plus_f1.Length);
            if (mid.Length < f0g0.Length)
            {
                int oldLen = mid.Length;
                mid = Arrays.CopyOf(mid, f0g0.Length);
                FillZeroes(mid, oldLen, mid.Length);
            }

            if (mid.Length < f1g1.Length)
            {
                int oldLen = mid.Length;
                mid = Arrays.CopyOf(mid, f1g1.Length);
                FillZeroes(mid, oldLen, mid.Length);
            }


            //subtract f0g0, f1g1
            for (int i = 0; i < f0g0.Length; ++i)
                mid[i] = ring.SubtractMutable(mid[i], f0g0[i]);
            for (int i = 0; i < f1g1.Length; ++i)
                mid[i] = ring.SubtractMutable(mid[i], f1g1[i]);
            int oldLen = f0g0.Length;
            E[] result = Arrays.CopyOf(f0g0, 2 * (fTo - fFrom) - 1);
            FillZeroes(result, oldLen, result.Length);
            for (int i = 0; i < mid.Length; ++i)
                result[i + split] = ring.AddMutable(result[i + split], mid[i]);
            for (int i = 0; i < f1g1.Length; ++i)
                result[i + 2 * split] = ring.AddMutable(result[i + 2 * split], f1g1[i]);
            return result;
        }


        /* ====================== Schönhage–Strassen algorithm algorithm via Kronecker substitution ====================== */
        /// <summary>
        /// Kronecker substitution adapted from https://github.com/tbuktu/ntru/blob/master/src/main/java/net/sf/ntru/polynomial/BigIntPolynomial.java
        /// </summary>
        static UnivariatePolynomial<BigInteger> SquareKronecker(UnivariatePolynomial<BigInteger> poly)
        {
            return Create(Rings.Z, SquareKronecker0(poly));
        }


        /// <summary>
        /// Kronecker substitution adapted from https://github.com/tbuktu/ntru/blob/master/src/main/java/net/sf/ntru/polynomial/BigIntPolynomial.java
        /// </summary>
        private static BigInteger[] SquareKronecker0(UnivariatePolynomial<BigInteger> poly)
        {
            int len = poly.degree + 1;

            // determine #bits needed per coefficient
            int logMinDigits = 32 - int.LeadingZeroCount(len - 1);
            int maxLength = 0;
            foreach (BigInteger cf in poly)
                maxLength = Math.Max(maxLength, cf.BitLength());
            int k = logMinDigits + 2 * maxLength + 1; // in bits
            k = (k + 31) / 32; // in ints

            // encode each polynomial into an int[]
            int[] pInt = ToIntArray(poly, k);
            int[] cInt = ToIntArray(ToBigInteger(pInt).Pow(2));

            // decode poly coefficients from the product
            BigInteger[] cPoly = new BigInteger[2 * len - 1];
            DecodePoly(k, cInt, cPoly);
            return cPoly;
        }


        private static void DecodePoly(int k, int[] cInt, BigInteger[] cPoly)
        {
            BigInteger _2k = BigInteger.One.ShiftLeft(k * 32);
            Array.Fill(cPoly, BigInteger.Zero);
            for (int i = 0; i < cPoly.Length; i++)
            {
                int[] cfInt = Arrays.CopyOfRange(cInt, i * k, (i + 1) * k);
                BigInteger cf = ToBigInteger(cfInt);
                if (cfInt[k - 1] < 0)
                {
                    // if coeff > 2^(k-1)
                    cf = cf.Subtract(_2k);

                    // add 2^k to cInt which is the same as subtracting coeff
                    bool carry;
                    int cIdx = (i + 1) * k;
                    do
                    {
                        cInt[cIdx]++;
                        carry = cInt[cIdx] == 0;
                        cIdx++;
                    } while (carry);
                }

                cPoly[i] = cPoly[i].Add(cf);
            }
        }


        /// <summary>
        /// Kronecker substitution adapted from https://github.com/tbuktu/ntru/blob/master/src/main/java/net/sf/ntru/polynomial/BigIntPolynomial.java
        /// </summary>
        static UnivariatePolynomial<BigInteger> MultiplyKronecker(UnivariatePolynomial<BigInteger> poly1,
            UnivariatePolynomial<BigInteger> poly2)
        {
            return Create(Rings.Z, MultiplyKronecker0(poly1, poly2));
        }


        /// <summary>
        /// Kronecker substitution adapted from https://github.com/tbuktu/ntru/blob/master/src/main/java/net/sf/ntru/polynomial/BigIntPolynomial.java
        /// </summary>
        /// <summary>
        /// Kronecker substitution adapted from https://github.com/tbuktu/ntru/blob/master/src/main/java/net/sf/ntru/polynomial/BigIntPolynomial.java
        /// </summary>
        static BigInteger[] MultiplyKronecker0(UnivariatePolynomial<BigInteger> poly1,
            UnivariatePolynomial<BigInteger> poly2)
        {
            if (poly2.degree > poly1.degree)
                return MultiplyKronecker0(poly2, poly1);
            int len1 = poly1.degree + 1;
            int len2 = poly2.degree + 1;

            // determine #bits needed per coefficient
            int logMinDigits = 32 - int.LeadingZeroCount(len1 - 1);
            int maxLengthA = 0;
            foreach (BigInteger cf in poly1)
                maxLengthA = Math.Max(maxLengthA, cf.BitLength());
            int maxLengthB = 0;
            foreach (BigInteger cf in poly2)
                maxLengthB = Math.Max(maxLengthB, cf.BitLength());
            int k = logMinDigits + maxLengthA + maxLengthB + 1; // in bits
            k = (k + 31) / 32; // in ints

            // encode each polynomial into an int[]
            int[] aInt = ToIntArray(poly1, k);
            int[] bInt = ToIntArray(poly2, k);

            // multiply
            int[] cInt = ToIntArray(ToBigInteger(aInt).Multiply(ToBigInteger(bInt)));

            // decode poly coefficients from the product
            BigInteger[] cPoly = new BigInteger[len1 + len2 - 1];
            DecodePoly(k, cInt, cPoly);
            int aSign = poly1.Lc().Signum();
            int bSign = poly2.Lc().Signum();
            if (aSign * bSign < 0)
                for (int i = 0; i < cPoly.Length; i++)
                    cPoly[i] = cPoly[i].Negate();
            return cPoly;
        }


        /// <summary>
        /// Converts a <code>int</code> array to a {@link BigInteger}.
        /// </summary>
        /// <returns>the <code>BigInteger</code> representation of the array</returns>
        private static BigInteger ToBigInteger(int[] a)
        {
            byte[] b = new byte[a.Length * 4];
            for (int i = 0; i < a.Length; i++)
            {
                int iRev = a.Length - 1 - i;
                b[i * 4] = (byte)(a[iRev] >>> 24);
                b[i * 4 + 1] = (byte)((a[iRev] >>> 16) & 0xFF);
                b[i * 4 + 2] = (byte)((a[iRev] >>> 8) & 0xFF);
                b[i * 4 + 3] = (byte)(a[iRev] & 0xFF);
            }

            return new BigInteger(1, b);
        }


        /// <summary>
        /// Converts a {@link BigInteger} to an <code>int</code> array.
        /// </summary>
        /// <returns>an <code>int</code> array that is compatible with the <code>mult()</code> methods</returns>
        private static int[] ToIntArray(BigInteger a)
        {
            byte[] aArr = a.ToByteArray();
            int[] b = new int[(aArr.Length + 3) / 4];
            for (int i = 0; i < aArr.Length; i++)
                b[i / 4] += (aArr[aArr.Length - 1 - i] & 0xFF) << ((i % 4) * 8);
            return b;
        }


        /// <summary>
        /// Converts a {@link BigInteger} to an <code>int</code> array.
        /// </summary>
        /// <returns>an <code>int</code> array that is compatible with the <code>mult()</code> methods</returns>
        private static int[] ToIntArray(UnivariatePolynomial<BigInteger> a, int k)
        {
            int len = a.degree + 1;
            int sign = a.Lc().Signum();
            int[] aInt = new int[len * k];
            for (int i = len - 1; i >= 0; i--)
            {
                int[] cArr = ToIntArray(a.data[i].Abs());
                if (a.data[i].Signum() * sign < 0)
                    SubShifted(aInt, cArr, i * k);
                else
                    AddShifted(aInt, cArr, i * k);
            }

            return aInt;
        }


        /// <summary>
        /// drops elements of b that are shifted outside the valid range
        /// </summary>
        private static void AddShifted(int[] a, int[] b, int numElements)
        {
            bool carry = false;
            int i = 0;
            while (i < Math.Min(b.Length, a.Length - numElements))
            {
                int ai = a[i + numElements];
                int sum = ai + b[i];
                if (carry)
                    sum++;
                carry = ((sum >>> 31) < (ai >>> 31) + (b[i] >>> 31)); // carry if signBit(sum) < signBit(a)+signBit(b)
                a[i + numElements] = sum;
                i++;
            }

            i += numElements;
            while (carry)
            {
                a[i]++;
                carry = a[i] == 0;
                i++;
            }
        }


        /// <summary>
        /// drops elements of b that are shifted outside the valid range
        /// </summary>
        private static void SubShifted(int[] a, int[] b, int numElements)
        {
            bool carry = false;
            int i = 0;
            while (i < Math.Min(b.Length, a.Length - numElements))
            {
                int ai = a[i + numElements];
                int diff = ai - b[i];
                if (carry)
                    diff--;
                carry = ((diff >>> 31) >
                         (a[i] >>> 31) - (b[i] >>> 31)); // carry if signBit(diff) > signBit(a)-signBit(b)
                a[i + numElements] = diff;
                i++;
            }

            i += numElements;
            while (carry)
            {
                a[i]--;
                carry = a[i] == -1;
                i++;
            }
        }
    }
}