using System.Diagnostics;
using System.Numerics;
using System.Text;
using Rings.io;

namespace Rings.poly.univar;


public sealed class UnivariatePolynomial<E> : IUnivariatePolynomial<UnivariatePolynomial<E>> {
    private static readonly long serialVersionUID = 1L;
    
    public readonly Ring<E> ring;
    
    public E[] data;

    public int Degree;

    public UnivariatePolynomial(Ring<E> ring, E[] data, int degree) {
        this.ring = ring;
        this.data = data;
        this.Degree = degree;
        Debug.Assert(data.Length > 0);
    }

    public UnivariatePolynomial(Ring<E> ring, E[] data) :this(ring, data, data.Length - 1){
        fixDegree();
    }

    
    public static  UnivariatePolynomial<E> parse<E>(String str, Ring<E> ring, String var) {
        return Coder<_,_,_>.mkUnivariateCoder(Rings.UnivariateRing(ring), var).parse(str);
    }

    
    [Obsolete]
    public static  UnivariatePolynomial<E> parse<E>(String str, Ring<E> ring) {
        return Coder<_,_,_>.mkUnivariateCoder(Rings.UnivariateRing(ring), guessVariableString(str)).parse(str);
    }

    private static String guessVariableString(String str) {
        Matcher matcher = Pattern.compile("[a-zA-Z]+[0-9]*").matcher(str);
        List<String> variables = new();
        var seen = new HashSet<string>();
        while (matcher.find()) {
            String var = matcher.group();
            if (seen.Contains(var))
                continue;
            seen.Add(var);
            variables.Add(var);
        }
        return variables.Count == 0 ? "x" : variables[0];
    }

    
    public static  UnivariatePolynomial<E> create(Ring<E> ring, params E[] data) {
        ring.setToValueOf(data);
        return new UnivariatePolynomial<E>(ring, data);
    }

    
    public static  UnivariatePolynomial<E> createUnsafe(Ring<E> ring, E[] data) {
        return new UnivariatePolynomial<E>(ring, data);
    }

    
    public static UnivariatePolynomial<BigInteger> create(Ring<BigInteger> ring, params long[] data) {
        return UnivariatePolynomial<BigInteger>.create(ring, ring.valueOf(data));
    }

    
    public static UnivariatePolynomial<BigInteger> create(params long[] data) {
        return create(Rings.Z, data);
    }

    
    public static  UnivariatePolynomial<E> constant(Ring<E> ring, E constant) {
        return create(ring, [constant]);
    }

    
    public static  UnivariatePolynomial<E> zero(Ring<E> ring) {
        return constant(ring, ring.getZero());
    }

    
    public static  UnivariatePolynomial<E> one(Ring<E> ring) {
        return constant(ring, ring.getOne());
    }

    
    public static UnivariatePolynomialZ64 asOverZ64(UnivariatePolynomial<BigInteger> poly) {
        long[] data = new long[poly.Degree + 1];
        for (int i = 0; i < data.Length; i++)
            data[i] = (long) poly.data[i];
        return UnivariatePolynomialZ64.create(data);
    }

    /**
     * Converts Zp[x] poly over BigIntegers to machine-sized polynomial in Zp
     *
     * @param poly the Z/p polynomial over BigIntegers
     * @return machine-sized polynomial in Z/p
     * @throws IllegalArgumentException if {@code poly.ring} is not {@link IntegersZp}
     * @throws ArithmeticException      if some of {@code poly} elements is out of long range
     */
    public static UnivariatePolynomialZp64 asOverZp64(UnivariatePolynomial<BigInteger> poly) {
        if (!(poly.ring is IntegersZp))
            throw new ArgumentException("Not a modular ring: " + poly.ring);
        long[] data = new long[poly.Degree + 1];
        for (int i = 0; i < data.Length; i++)
            data[i] = (long) poly.data[i];
        return UnivariatePolynomialZp64.create((long)((IntegersZp) poly.ring).Modulus, data);
    }

    /**
     * Converts Zp[x] poly over BigIntegers to machine-sized polynomial in Zp
     *
     * @param poly the polynomial over BigIntegers
     * @param ring Zp64 ring
     * @return machine-sized polynomial in Z/p
     * @throws IllegalArgumentException if {@code poly.ring} is not {@link IntegersZp}
     * @throws ArithmeticException      if some of {@code poly} elements is out of long range
     */
    public static UnivariatePolynomialZp64 asOverZp64(UnivariatePolynomial<BigInteger> poly, IntegersZp64 ring) {
        long modulus = ring.Modulus;
        long[] data = new long[poly.Degree + 1];
        for (int i = 0; i < data.Length; i++)
            data[i] = (long)(poly.data[i] % modulus);
        return UnivariatePolynomialZp64.create(ring, data);
    }

    /**
     * Converts Zp[x] poly over rationals to machine-sized polynomial in Zp
     *
     * @param poly the polynomial over rationals
     * @param ring Zp64 ring
     * @return machine-sized polynomial in Z/p
     * @throws IllegalArgumentException if {@code poly.ring} is not {@link IntegersZp}
     * @throws ArithmeticException      if some of {@code poly} elements is out of long range
     */
    public static UnivariatePolynomialZp64 asOverZp64Q(UnivariatePolynomial<Rational<BigInteger>> poly, IntegersZp64 ring) {
        long modulus = ring.Modulus;
        long[] data = new long[poly.Degree + 1];
        for (int i = 0; i < data.Length; i++)
            data[i] = ring.divide((long)(poly.data[i].numerator() % modulus), (long)(poly.data[i].denominator() % modulus));
        return UnivariatePolynomialZp64.create(ring, data);
    }

    /**
     * Converts Zp[x] polynomial to Z[x] polynomial formed from the coefficients of this represented in symmetric
     * modular form ({@code -modulus/2 <= cfx <= modulus/2}).
     *
     * @param poly Zp polynomial
     * @return Z[x] version of the poly with coefficients represented in symmetric modular form ({@code -modulus/2 <=
     *         cfx <= modulus/2}).
     * @throws IllegalArgumentException is {@code poly.ring} is not a {@link IntegersZp}
     */
    public static UnivariatePolynomial<BigInteger> asPolyZSymmetric(UnivariatePolynomial<BigInteger> poly) {
        if (!(poly.ring is IntegersZp))
            throw new ArgumentException("Not a modular ring: " + poly.ring);
        IntegersZp ring = (IntegersZp) poly.ring;
        BigInteger[] newData = new BigInteger[poly.Degree + 1];
        for (int i = poly.Degree; i >= 0; --i)
            newData[i] = ring.symmetricForm(poly.data[i]);
        return UnivariatePolynomial<BigInteger>.createUnsafe(Rings.Z, newData);
    }


    public E get(int i) { return i > Degree ? ring.getZero() : data[i];}

    
    public UnivariatePolynomial<E> set(int i, E el) {
        el = ring.valueOf(el);
        if (ring.isZero(el)) {
            if (i > Degree)
                return this;
            data[i] = el;
            fixDegree();
            return this;
        }
        ensureCapacity(i);
        data[i] = el;
        fixDegree();
        return this;
    }

    
    public UnivariatePolynomial<E> setLC(E lc) {
        return set(Degree, lc);
    }

    
    public int firstNonZeroCoefficientPosition() {
        if (isZero()) return -1;
        int i = 0;
        while (ring.isZero(data[i])) ++i;
        return i;
    }

    
    public UnivariatePolynomial<E> setRing(Ring<E> newRing) {
        if (ring.Equals(newRing))
            return clone();
        E[] newData = new E[Degree + 1];
        Array.Copy(data, newData, Degree + 1);
        newRing.setToValueOf(newData);
        return new UnivariatePolynomial<E>(newRing, newData);
    }

    
    public UnivariatePolynomial<E> setRingUnsafe(Ring<E> newRing) {
        return new UnivariatePolynomial<E>(newRing, data, Degree);
    }

    
    public E lc() {return data[Degree];}

    
    public UnivariatePolynomial<E> lcAsPoly() {return createConstant(lc());}

    
    public UnivariatePolynomial<E> ccAsPoly() {return createConstant(cc());}

    
    public UnivariatePolynomial<E> getAsPoly(int i) {return createConstant(get(i));}

    
    public E cc() {return data[0];}

    
    public void ensureInternalCapacity(int desiredCapacity) {
        if (data.Length < desiredCapacity) {
            int oldLength = data.Length;
            var newData = new E[desiredCapacity];
            Array.Copy(data, newData, desiredCapacity);
            data = newData;
            fillZeroes(data, oldLength, data.Length);
        }
    }


    public void ensureCapacity(int desiredDegree) {
        if (Degree < desiredDegree)
            Degree = desiredDegree;

        if (data.Length < (desiredDegree + 1)) {
            int oldLen = data.Length;
            data = Arrays.copyOf(data, desiredDegree + 1);
            fillZeroes(data, oldLen, data.Length);
        }
    }


    public void fixDegree() {
        int i = Degree;
        while (i >= 0 && ring.isZero(data[i])) --i;
        if (i < 0) i = 0;

        if (i != Degree) {
            Degree = i;
            // not necessary to fillZeroes here!
            // fillZeroes(data, degree + 1, data.Length);
        }
    }

    
    public UnivariatePolynomial<E> getRange(int from, int to) {
        var newData = new E[to - from];
        Array.Copy(data, from, newData, 0, to - from);
        return new UnivariatePolynomial<E>(ring, newData);
    }

    
    public bool sameCoefficientRingWith(UnivariatePolynomial<E> oth) {
        return ring.Equals(oth.ring);
    }

    
    public UnivariatePolynomial<E> setCoefficientRingFrom(UnivariatePolynomial<E> poly) {
        return setRing(poly.ring);
    }

    
    public UnivariatePolynomial<E> createFromArray(E[] data) {
        ring.setToValueOf(data);
        return new UnivariatePolynomial<E>(ring, data);
    }

    
    public UnivariatePolynomial<E> createMonomial(int degree) {return createMonomial(ring.getOne(), degree);}

    
    public UnivariatePolynomial<E> createLinear(E cc, E lc) {
        return createFromArray([cc, lc]);
    }

    
    public UnivariatePolynomial<E> createMonomial(E coefficient, int degree) {
        coefficient = ring.valueOf(coefficient);
        E[] data = ring.createZeroesArray(degree + 1);
        data[degree] = coefficient;
        return new UnivariatePolynomial<E>(ring, data);
    }

    
    public UnivariatePolynomial<E> createConstant(E val) {
        return createFromArray([val]);
    }

    
    public UnivariatePolynomial<E> createZero() {return createConstant(ring.getZero());}

    
    public UnivariatePolynomial<E> createOne() {return createConstant(ring.getOne());}

    
    public bool isZeroAt(int i) {return i >= data.Length || ring.isZero(data[i]);}

    
    public UnivariatePolynomial<E> setZero(int i) {
        if (i < data.Length)
            data[i] = ring.getZero();
        return this;
    }

    
    public UnivariatePolynomial<E> setFrom(int indexInThis, UnivariatePolynomial<E> poly, int indexInPoly) {
        ensureCapacity(indexInThis);
        data[indexInThis] = poly.get(indexInPoly);
        fixDegree();
        return this;
    }

    
    public bool isZero() {return ring.isZero(data[Degree]);}

    
    public bool isOne() {return Degree == 0 && ring.isOne(data[0]);}

    
    public bool isMonic() {return ring.isOne(lc());}

    
    public bool isUnitCC() {return ring.isOne(cc());}



    public bool isConstant() {return Degree == 0;}

    
    public bool isMonomial() {
        for (int i = Degree - 1; i >= 0; --i)
            if (!ring.isZero(data[i]))
                return false;
        return true;
    }

    
    public int signumOfLC() {
        return ring.signum(lc());
    }

    
    public bool isOverField() {
        return ring.isField();
    }

    
    public bool isOverFiniteField() {
        return ring.isFinite();
    }
    
    
   


    public bool isOverZ() {return ring.Equals(Rings.Z);}

    
    public BigInteger coefficientRingCardinality() {
        return ring.cardinality();
    }

    
    public BigInteger coefficientRingCharacteristic() {
        return ring.characteristic();
    }

    
    public bool isOverPerfectPower() {
        return ring.isPerfectPower();
    }

    
    public BigInteger coefficientRingPerfectPowerBase() {
        return ring.perfectPowerBase();
    }

    
    public BigInteger coefficientRingPerfectPowerExponent() {
        return ring.perfectPowerExponent();
    }

    
    public static BigInteger mignotteBound(UnivariatePolynomial<BigInteger> poly) {
        return (BigInteger.One << poly.Degree) * norm2(poly);
    }

    
    public static BigInteger norm1(UnivariatePolynomial<BigInteger> poly) {
        BigInteger norm = BigInteger.Zero;
        for (int i = poly.Degree; i >= 0; --i)
            norm += BigInteger.Abs(poly.data[i]);
        return norm;
    }

    
    public static BigInteger norm2(UnivariatePolynomial<BigInteger> poly) {
        BigInteger norm = BigInteger.Zero;
        for (int i = poly.Degree; i >= 0; --i)
            norm += poly.data[i] * poly.data[i];
        return BigIntegerUtil.sqrtCeil(norm);
    }

    
    public static double norm2Double(UnivariatePolynomial<BigInteger> poly) {
        double norm = 0;
        for (int i = poly.Degree; i >= 0; --i) {
            double d = (double)poly.data[i];
            norm += d * d;
        }
        return Math.Sqrt(norm);
    }

    
    public E maxAbsCoefficient() {
        E el = ring.abs(data[0]);
        for (int i = 1; i <= Degree; i++)
            el = ring.max(el, ring.abs(data[i]));
        return el;
    }

    
    public E normMax() {
        return maxAbsCoefficient();
    }

    private void fillZeroes(E[] data, int from, int to) {
        for (int i = from; i < to; ++i)
            data[i] = ring.getZero(); //invoke getZero() at each cycle
    }

    
    public UnivariatePolynomial<E> toZero() {
        fillZeroes(data, 0, Degree + 1);
        Degree = 0;
        return this;
    }

    
    public UnivariatePolynomial<E> set(UnivariatePolynomial<E> oth) {
        if (oth == this)
            return this;
        this.data = (E[])oth.data.Clone();
        this.Degree = oth.Degree;
        return this;
    }

    
    public UnivariatePolynomial<E> setAndDestroy(UnivariatePolynomial<E> oth) {
        this.data = oth.data;
        oth.data = null; // destroy
        this.Degree = oth.Degree;
        Debug.Assert(data.Length > 0);
        return this;
    }

    
    public UnivariatePolynomial<E> shiftLeft(int offset) {
        if (offset == 0)
            return this;
        if (offset > Degree)
            return toZero();

        Array.Copy(data, offset, data, 0, Degree - offset + 1);
        fillZeroes(data, Degree - offset + 1, Degree + 1);
        Degree = Degree - offset;
        return this;
    }

    
    public UnivariatePolynomial<E> shiftRight(int offset) {
        if (offset == 0)
            return this;
        int degree = this.Degree;
        ensureCapacity(offset + degree);
        Array.Copy(data, 0, data, offset, degree + 1);
        fillZeroes(data, 0, offset);
        return this;
    }

    
    public UnivariatePolynomial<E> truncate(int newDegree) {
        if (newDegree >= Degree)
            return this;
        fillZeroes(data, newDegree + 1, Degree + 1);
        Degree = newDegree;
        fixDegree();
        return this;
    }

    
    public UnivariatePolynomial<E> reverse() {
        ArraysUtil.reverse(data, 0, Degree + 1);
        fixDegree();
        return this;
    }

    
    public E content() {
        if (Degree == 0)
            return data[0];
        return isOverField() ? lc() : ring.gcd(data);
//        E gcd = data[degree];
//        for (int i = degree - 1; i >= 0; --i)
//            gcd = ring.gcd(gcd, data[i]);
//        return gcd;
    }

    
    public UnivariatePolynomial<E> contentAsPoly() {
        return createConstant(content());
    }

    
    public UnivariatePolynomial<E> primitivePart() {
        E content = this.content();
        if (signumOfLC() < 0 && ring.signum(content) > 0)
            content = ring.negate(content);
        if (ring.isMinusOne(content))
            return negate();
        return primitivePart0(content);
    }

    
    public UnivariatePolynomial<E> primitivePartSameSign() {
        return primitivePart0(content());
    }

    private UnivariatePolynomial<E> primitivePart0(E content) {
        if (isZero())
            return this;
        if (ring.isOne(content))
            return this;
        for (int i = Degree; i >= 0; --i) {
            var d = ring.divideOrNull(data[i], content);
            if (d.IsNull)
                return null;
            
            data[i] = d.Value;
        }
        return this;
    }

    
    public E evaluate(long point) {
        return evaluate(ring.valueOf(point));
    }

    
    public E evaluate(E point) {
        if (ring.isZero(point))
            return cc();

        point = ring.valueOf(point);
        E res = ring.getZero();
        for (int i = Degree; i >= 0; --i)
            res = ring.addMutable(ring.multiplyMutable(res, point), data[i]);
        return res;
    }

    
    public UnivariatePolynomial<E> composition(UnivariatePolynomial<E> value) {
        if (value.isOne())
            return this.clone();
        if (value.isZero())
            return ccAsPoly();
        if (value.Degree == 1 && value.isMonomial() && ring.isOne(value.lc()))
            return clone();

        UnivariatePolynomial<E> result = createZero();
        for (int i = Degree; i >= 0; --i)
            result = result.multiply(value).add(data[i]);
        return result;
    }

    
    public MultivariatePolynomial<E> composition(AMultivariatePolynomial value) {
        if (!(value is MultivariatePolynomial))
            throw new ArgumentException();
        if (!((MultivariatePolynomial) value).ring.equals(ring))
            throw new ArgumentException();
        if (value.isOne())
            return asMultivariate();
        if (value.isZero())
            return ccAsPoly().asMultivariate();

        MultivariatePolynomial<E> result = (MultivariatePolynomial<E>) value.createZero();
        for (int i = Degree; i >= 0; --i)
            result = result.multiply((MultivariatePolynomial<E>) value).add(data[i]);
        return result;
    }

    
    public UnivariatePolynomial<E> scale(E scaling) {
        if (ring.isOne(scaling))
            return this.clone();
        if (ring.isZero(scaling))
            return ccAsPoly();

        E factor = ring.getOne();
        E[] result = new E[Degree + 1];
        for (int i = 0; i <= Degree; ++i) {
            result[i] = ring.multiply(data[i], factor);
            factor = ring.multiply(factor, scaling);
        }
        return createUnsafe(ring, result);
    }

    
    public UnivariatePolynomial<E> shift(E value) {
        return composition(createLinear(value, ring.getOne()));
    }

    
    public UnivariatePolynomial<E> add(E val) {
        data[0] = ring.add(data[0], ring.valueOf(val));
        fixDegree();
        return this;
    }

    
    public UnivariatePolynomial<E> subtract(E val) {
        data[0] = ring.subtract(data[0], ring.valueOf(val));
        fixDegree();
        return this;
    }

    
    public UnivariatePolynomial<E> decrement() {
        return subtract(createOne());
    }

    
    public UnivariatePolynomial<E> increment() {
        return add(createOne());
    }

    
    public UnivariatePolynomial<E> add(UnivariatePolynomial<E> oth) {
        assertSameCoefficientRingWith(oth);

        if (oth.isZero())
            return this;
        if (isZero())
            return set(oth);

        assertSameCoefficientRingWith(oth);
        ensureCapacity(oth.Degree);
        for (int i = oth.Degree; i >= 0; --i)
            data[i] = ring.add(data[i], oth.data[i]);
        fixDegree();
        return this;
    }

    
    public UnivariatePolynomial<E> addMonomial(E coefficient, int exponent) {
        if (ring.isZero(coefficient))
            return this;

        ensureCapacity(exponent);
        data[exponent] = ring.add(data[exponent], ring.valueOf(coefficient));
        fixDegree();
        return this;
    }

    
    public UnivariatePolynomial<E> addMul(UnivariatePolynomial<E> oth, E factor) {
        assertSameCoefficientRingWith(oth);

        if (oth.isZero())
            return this;

        factor = ring.valueOf(factor);
        if (ring.isZero(factor))
            return this;

        assertSameCoefficientRingWith(oth);
        ensureCapacity(oth.Degree);
        for (int i = oth.Degree; i >= 0; --i)
            data[i] = ring.add(data[i], ring.multiply(factor, oth.data[i]));
        fixDegree();
        return this;
    }

    
    public UnivariatePolynomial<E> subtract(UnivariatePolynomial<E> oth) {
        assertSameCoefficientRingWith(oth);

        if (oth.isZero())
            return this;
        if (isZero())
            return set(oth).negate();

        assertSameCoefficientRingWith(oth);
        ensureCapacity(oth.Degree);
        for (int i = oth.Degree; i >= 0; --i)
            data[i] = ring.subtract(data[i], oth.data[i]);
        fixDegree();
        return this;
    }

    
    public UnivariatePolynomial<E> subtract(UnivariatePolynomial<E> oth, E factor, int exponent) {
        assertSameCoefficientRingWith(oth);

        if (oth.isZero())
            return this;

        factor = ring.valueOf(factor);
        if (ring.isZero(factor))
            return this;

        assertSameCoefficientRingWith(oth);
        for (int i = oth.Degree + exponent; i >= exponent; --i)
            data[i] = ring.subtract(data[i], ring.multiply(factor, oth.data[i - exponent]));

        fixDegree();
        return this;
    }

    
    public UnivariatePolynomial<E> negate() {
        for (int i = Degree; i >= 0; --i)
            if (!ring.isZero(data[i]))
                data[i] = ring.negate(data[i]);
        return this;
    }

    
    public UnivariatePolynomial<E> multiply(E factor) {
        factor = ring.valueOf(factor);
        if (ring.isOne(factor))
            return this;

        if (ring.isZero(factor))
            return toZero();

        for (int i = Degree; i >= 0; --i)
            data[i] = ring.multiply(data[i], factor);
        return this;
    }

    
    public UnivariatePolynomial<E> multiplyByLC(UnivariatePolynomial<E> other) {
        return multiply(other.lc());
    }

    
    public UnivariatePolynomial<E> multiply(long factor) {
        return multiply(ring.valueOf(factor));
    }

    
    public UnivariatePolynomial<E> divideByLC(UnivariatePolynomial<E> other) {
        return divideOrNull(other.lc());
    }

    
    public UnivariatePolynomial<E> divideOrNull(E factor) {
        factor = ring.valueOf(factor);
        if (ring.isZero(factor))
            throw new ArithmeticException("Divide by zero");
        if (ring.isOne(factor))
            return this;
        if (ring.isMinusOne(factor))
            return negate();
        if (ring.isField()) // this is typically much faster
            return multiply(ring.reciprocal(factor));

        for (int i = Degree; i >= 0; --i) {
            util.Nullable<E> l = ring.divideOrNull(data[i], factor);
            if (l.IsNull)
                return null;
            data[i] = l.Value;
        }
        return this;
    }

    
    public UnivariatePolynomial<E> divideExact(E factor) {
        UnivariatePolynomial<E> r = divideOrNull(factor);
        if (r == null)
            throw new ArithmeticException("not divisible " + this + " / " + factor);
        return r;
    }

    
    public UnivariatePolynomial<E> monic() {
        if (isZero())
            return this;
        return divideOrNull(lc());
    }

    
    public UnivariatePolynomial<E> monic(E factor) {
        E lc = this.lc();
        return multiply(factor).divideOrNull(lc);
    }

    
    public UnivariatePolynomial<E> monicWithLC(UnivariatePolynomial<E> other) {
        if (lc().Equals(other.lc()))
            return this;
        return monic(other.lc());
    }

    
    public UnivariatePolynomial<E> multiplyByBigInteger(BigInteger factor) {
        return multiply(ring.valueOfBigInteger(factor));
    }

    
    public UnivariatePolynomial<E> multiply(UnivariatePolynomial<E> oth) {
        assertSameCoefficientRingWith(oth);

        if (isZero())
            return this;
        if (oth.isZero())
            return toZero();
        if (this == oth)
            return square();

        if (oth.Degree == 0)
            return multiply(oth.data[0]);
        if (Degree == 0) {
            E factor = data[0];
            data = (E[])oth.data.Clone();
            Degree = oth.Degree;
            return multiply(factor);
        }

        if (ring is IntegersZp) {
            // faster method with exact operations
            UnivariatePolynomial<E>
                    iThis = setRingUnsafe(Rings.Z as Ring<E>),
                    iOth = oth.setRingUnsafe(Rings.Z as Ring<E>);
            data = iThis.multiply(iOth).data;
            ring.setToValueOf(data);
        } else
            data = multiplySafe0(oth);
        Degree += oth.Degree;
        fixDegree();
        return this;
    }

    
    public UnivariatePolynomial<E> square() {
        if (isZero())
            return this;
        if (Degree == 0)
            return multiply(data[0]);

        if (ring is IntegersZp) {
            // faster method with exact operations
            UnivariatePolynomial<E> iThis = setRingUnsafe(Rings.Z as Ring<E>);
            data = iThis.square().data;
            ring.setToValueOf(data);
        } else
            data = squareSafe0();
        Degree += Degree;
        fixDegree();
        return this;
    }

    
    public UnivariatePolynomial<E> derivative() {
        if (isConstant())
            return createZero();
        E[] newData = new E[Degree];
        for (int i = Degree; i > 0; --i)
            newData[i - 1] = ring.multiply(data[i], ring.valueOf(i));
        return createFromArray(newData);
    }

    
    public UnivariatePolynomial<E> clone() {
        return new UnivariatePolynomial<E>(ring, (E[])data.Clone(), Degree);
    }

    
    public UnivariatePolynomial<E> parsePoly(String str) {
        return parse(str, ring);
    }

    
    public IEnumerable<E> iterator() {
        return data.Take(..(Degree + 1));
    }

    
    public IEnumerable<E> stream() {
        return data.Take(..(Degree + 1));
    }

    
    public IEnumerable<UnivariatePolynomial<E>> streamAsPolys() {
        return stream().Select(createConstant);
    }
    
    public  UnivariatePolynomial<T> mapCoefficients<T>(Ring<T> ring, Func<E, T> mapper) {
        return new UnivariatePolynomial<T>(ring, stream().Select(mapper).ToArray());
    }

    
    public UnivariatePolynomialZp64 mapCoefficients(IntegersZp64 ring, Func<E, long> mapper) {
        return UnivariatePolynomialZp64.create(ring, stream().Select(mapper).ToArray());
    }

    
    public E[] getDataReferenceUnsafe() {return data;}

    
    public MultivariatePolynomial<E> asMultivariate() {
        return asMultivariate(MonomialOrder.DEFAULT);
    }

    
    public MultivariatePolynomial<E> asMultivariate(Comparator<DegreeVector> ordering) {
        return MultivariatePolynomial.asMultivariate(this, 1, 0, ordering);
    }

    
    public int compareTo(UnivariatePolynomial<E> o) {
        int c = Degree.CompareTo(o.Degree);
        if (c != 0)
            return c;
        for (int i = Degree; i >= 0; --i) {
            c = ring.compare(data[i], o.data[i]);
            if (c != 0)
                return c;
        }
        return 0;
    }

    
    public String coefficientRingToString(IStringifier<UnivariatePolynomial<E>> stringifier) {
        return ring.toString(stringifier.substringifier(ring));
    }

    
    public String toString() {
        return toString(IStringifier<UnivariatePolynomial<E>>.dummy<UnivariatePolynomial<E>>());
    }

    
    public String toString(IStringifier<UnivariatePolynomial<E>> stringifier) {
        IStringifier<E> cfStringifier = stringifier.substringifier(ring);
        if (isConstant())
            return cfStringifier.stringify(cc());

        String varString = stringifier.getBindings().GetValueOrDefault(createMonomial(1), IStringifier<UnivariatePolynomial<E>>.defaultVar());
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i <= Degree; i++) {
            E el = data[i];
            if (ring.isZero(el))
                continue;

            String cfString;
            if (!ring.isOne(el) || i == 0)
                cfString = cfStringifier.stringify(el);
            else
                cfString = "";

            if (i != 0 && IStringifier<UnivariatePolynomial<E>>.needParenthesisInSum(cfString))
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

    String toStringForCopy() {
        String s = ArraysUtil.toString(data, 0, Degree + 1, x => "new BigInteger(\"" + x + "\")");
        return "of(" + s.Substring(1, s.Length - 1) + ")";
    }

    
    public bool equals(Object obj) {
        if (obj.GetType() != this.GetType())
            return false;
        
        UnivariatePolynomial<E> oth = (UnivariatePolynomial<E>) obj;
        if (Degree != oth.Degree)
            return false;
        for (int i = 0; i <= Degree; ++i)
            if (!(data[i].Equals(oth.data[i])))
                return false;
        return true;
    }

    
    public int hashCode() {
        int result = 1;
        for (int i = Degree; i >= 0; --i)
            result = 31 * result + data[i].GetHashCode();
        return result;
    }

    /* =========================== Exact multiplication with safe arithmetics =========================== */

    
    static readonly long KARATSUBA_THRESHOLD = 1024L;
    /** when use Classical/Karatsuba/Schoenhage-Strassen fast multiplication */
    static readonly long
            MUL_CLASSICAL_THRESHOLD = 256L * 256L,
            MUL_KRONECKER_THRESHOLD = 32L * 32L,
            MUL_MOD_CLASSICAL_THRESHOLD = 128L * 128L;

    
    E[] multiplySafe0(UnivariatePolynomial<E> oth) {
        long md = 1L * (Degree + 1) * (oth.Degree + 1);
        if (isOverZ() && md >= MUL_KRONECKER_THRESHOLD)
            return multiplyKronecker0(this as UnivariatePolynomial<BigInteger>, oth as UnivariatePolynomial<BigInteger>) as E[];
        if (md <= MUL_CLASSICAL_THRESHOLD)
            return multiplyClassicalSafe(data, 0, Degree + 1, oth.data, 0, oth.Degree + 1);
        else
            return multiplyKaratsubaSafe(data, 0, Degree + 1, oth.data, 0, oth.Degree + 1);
    }

    
    E[] squareSafe0() {
        long md = 1L * (Degree + 1) * (Degree + 1);
        if (isOverZ() && md >= MUL_KRONECKER_THRESHOLD)
            return squareKronecker0(this as UnivariatePolynomial<BigInteger>) as E[];
        if (md <= MUL_CLASSICAL_THRESHOLD)
            return squareClassicalSafe(data, 0, Degree + 1);
        else
            return squareKaratsubaSafe(data, 0, Degree + 1);
    }

    
    E[] multiplyClassicalSafe(E[] a, int aFrom, int aTo, E[] b, int bFrom, int bTo) {
        E[] result = ring.createZeroesArray(aTo - aFrom + bTo - bFrom - 1);
        multiplyClassicalSafe(result, a, aFrom, aTo, b, bFrom, bTo);
        return result;
    }

    
    void multiplyClassicalSafe(E[] result, E[] a, int aFrom, int aTo, E[] b, int bFrom, int bTo) {
        if (aTo - aFrom > bTo - bFrom) {
            multiplyClassicalSafe(result, b, bFrom, bTo, a, aFrom, aTo);
            return;
        }
        for (int i = 0; i < aTo - aFrom; ++i) {
            E c = a[aFrom + i];
            if (!ring.isZero(c))
                for (int j = 0; j < bTo - bFrom; ++j)
                    result[i + j] = ring.addMutable(result[i + j], ring.multiply(c, b[bFrom + j]));
        }
    }

    
    E[] multiplyKaratsubaSafe(
            E[] f, int fFrom, int fTo,
            E[] g, int gFrom, int gTo) {
        // return zero
        if (fFrom >= fTo || gFrom >= gTo)
            return [];

        // single element in f
        if (fTo - fFrom == 1) {
            E[] result = new E[gTo - gFrom];
            for (int i = gFrom; i < gTo; ++i)
                result[i - gFrom] = ring.multiply(f[fFrom], g[i]);
            return result;
        }
        // single element in g
        if (gTo - gFrom == 1) {
            E[] result2 = new E[fTo - fFrom];
            //single element in b
            for (int i = fFrom; i < fTo; ++i)
                result2[i - fFrom] = ring.multiply(g[gFrom], f[i]);
            return result2;
        }
        // linear factors
        if (fTo - fFrom == 2 && gTo - gFrom == 2) {
            E[] result3 = new E[3];
            //both a and b are linear
            result3[0] = ring.multiply(f[fFrom], g[gFrom]);
            result3[1] = ring.addMutable(ring.multiply(f[fFrom], g[gFrom + 1]), ring.multiply(f[fFrom + 1], g[gFrom]));
            result3[2] = ring.multiply(f[fFrom + 1], g[gFrom + 1]);
            return result3;
        }
        //switch to classical
        if (1L * (fTo - fFrom) * (gTo - gFrom) < KARATSUBA_THRESHOLD)
            return multiplyClassicalSafe(g, gFrom, gTo, f, fFrom, fTo);

        if (fTo - fFrom < gTo - gFrom)
            return multiplyKaratsubaSafe(g, gFrom, gTo, f, fFrom, fTo);


        //we now split a and b into 2 parts:
        int split = (fTo - fFrom + 1) / 2;
        //if we can't split b
        if (gFrom + split >= gTo) {
            E[] f0g = multiplyKaratsubaSafe(f, fFrom, fFrom + split, g, gFrom, gTo);
            E[] f1g = multiplyKaratsubaSafe(f, fFrom + split, fTo, g, gFrom, gTo);

            int _oldLen = f0g.Length, newLen = fTo - fFrom + gTo - gFrom - 1;
            E[] result4 = new E[newLen];
            Array.Copy(f0g, result4, newLen);
            fillZeroes(result4, _oldLen, newLen);
            for (int i = 0; i < f1g.Length; i++)
                result4[i + split] = ring.addMutable(result4[i + split], f1g[i]);
            return result4;
        }

        int fMid = fFrom + split, gMid = gFrom + split;
        E[] f0g0 = multiplyKaratsubaSafe(f, fFrom, fMid, g, gFrom, gMid);
        E[] f1g1 = multiplyKaratsubaSafe(f, fMid, fTo, g, gMid, gTo);

        // f0 + f1
        E[] f0_plus_f1 = new E[Math.Max(fMid - fFrom, fTo - fMid)];
        Array.Copy(f, fFrom, f0_plus_f1, 0, fMid - fFrom);
        fillZeroes(f0_plus_f1, fMid - fFrom, f0_plus_f1.Length);
        for (int i = fMid; i < fTo; ++i)
            f0_plus_f1[i - fMid] = ring.add(f0_plus_f1[i - fMid], f[i]);

        //g0 + g1
        E[] g0_plus_g1 = new E[Math.Max(gMid - gFrom, gTo - gMid)];
        Array.Copy(g, gFrom, g0_plus_g1, 0, gMid - gFrom);
        fillZeroes(g0_plus_g1, gMid - gFrom, g0_plus_g1.Length);
        for (int i = gMid; i < gTo; ++i)
            g0_plus_g1[i - gMid] = ring.add(g0_plus_g1[i - gMid], g[i]);

        E[] mid = multiplyKaratsubaSafe(f0_plus_f1, 0, f0_plus_f1.Length, g0_plus_g1, 0, g0_plus_g1.Length);

        if (mid.Length < f0g0.Length) {
            int _oldLen = mid.Length;
            mid = new E[f0g0.Length];
            Array.Copy(mid, mid, f0g0.Length);
            fillZeroes(mid, _oldLen, mid.Length);
        }
        if (mid.Length < f1g1.Length) {
            int _oldLen = mid.Length;
            mid = new E[f1g1.Length];
            Array.Copy(mid, mid, f1g1.Length);
            fillZeroes(mid, _oldLen, mid.Length);
        }

        //subtract f0g0, f1g1
        for (int i = 0; i < f0g0.Length; ++i)
            mid[i] = ring.subtractMutable(mid[i], f0g0[i]);
        for (int i = 0; i < f1g1.Length; ++i)
            mid[i] = ring.subtractMutable(mid[i], f1g1[i]);


        int oldLen = f0g0.Length;
        E[] result5 = new E[(fTo - fFrom) + (gTo - gFrom) - 1];
        Array.Copy(f0g0, result5, (fTo - fFrom) + (gTo - gFrom) - 1);
        fillZeroes(result5, oldLen, result5.Length);
        for (int i = 0; i < mid.Length; ++i)
            result5[i + split] = ring.addMutable(result5[i + split], mid[i]);
        for (int i = 0; i < f1g1.Length; ++i)
            result5[i + 2 * split] = ring.addMutable(result5[i + 2 * split], f1g1[i]);

        return result5;
    }

    E[] squareClassicalSafe(E[] a, int from, int to) {
        E[] x = ring.createZeroesArray((to - from) * 2 - 1);
        squareClassicalSafe(x, a, from, to);
        return x;
    }


    
    void squareClassicalSafe(E[] result, E[] data, int from, int to) {
        int len = to - from;
        for (int i = 0; i < len; ++i) {
            E c = data[from + i];
            if (!ring.isZero(c))
                for (int j = 0; j < len; ++j)
                    result[i + j] = ring.addMutable(result[i + j], ring.multiply(c, data[from + j]));
        }
    }

    
    E[] squareKaratsubaSafe(E[] f, int fFrom, int fTo) {
        if (fFrom >= fTo)
            return [];
        if (fTo - fFrom == 1) {
            return [ring.multiply(f[fFrom], f[fFrom])];
        }
        if (fTo - fFrom == 2) {
            E[] result = new E[3];
            result[0] = ring.multiply(f[fFrom], f[fFrom]);
            result[1] = ring.multiplyMutable(ring.multiply(f[fFrom], f[fFrom + 1]), ring.valueOf(2));
            result[2] = ring.multiply(f[fFrom + 1], f[fFrom + 1]);
            return result;
        }
        //switch to classical
        if (1L * (fTo - fFrom) * (fTo - fFrom) < KARATSUBA_THRESHOLD)
            return squareClassicalSafe(f, fFrom, fTo);


        //we now split a and b into 2 parts:
        int split = (fTo - fFrom + 1) / 2;
        int fMid = fFrom + split;
        E[] f0g0 = squareKaratsubaSafe(f, fFrom, fMid);
        E[] f1g1 = squareKaratsubaSafe(f, fMid, fTo);

        // f0 + f1
        E[] f0_plus_f1 = new E[Math.Max(fMid - fFrom, fTo - fMid)];
        Array.Copy(f, fFrom, f0_plus_f1, 0, fMid - fFrom);
        fillZeroes(f0_plus_f1, fMid - fFrom, f0_plus_f1.Length);
        for (int i = fMid; i < fTo; ++i)
            f0_plus_f1[i - fMid] = ring.add(f0_plus_f1[i - fMid], f[i]);

        E[] mid = squareKaratsubaSafe(f0_plus_f1, 0, f0_plus_f1.Length);

        if (mid.Length < f0g0.Length) {
            int oldLen = mid.Length;
            Array.Copy(mid, mid, f0g0.Length);
            fillZeroes(mid, oldLen, mid.Length);
        }
        if (mid.Length < f1g1.Length) {
            int oldLen = mid.Length;
            Array.Copy(mid, mid, f1g1.Length);
            fillZeroes(mid, oldLen, mid.Length);
        }


        //subtract f0g0, f1g1
        for (int i = 0; i < f0g0.Length; ++i)
            mid[i] = ring.subtractMutable(mid[i], f0g0[i]);
        for (int i = 0; i < f1g1.Length; ++i)
            mid[i] = ring.subtractMutable(mid[i], f1g1[i]);


        int _oldLen = f0g0.Length;
        E[] _result = new E[ 2 * (fTo - fFrom) - 1];
        Array.Copy(f0g0, _result, 2 * (fTo - fFrom) - 1);
        fillZeroes(_result, _oldLen, _result.Length);
        for (int i = 0; i < mid.Length; ++i)
            _result[i + split] = ring.addMutable(_result[i + split], mid[i]);
        for (int i = 0; i < f1g1.Length; ++i)
            _result[i + 2 * split] = ring.addMutable(_result[i + 2 * split], f1g1[i]);

        return _result;
    }

    /* ====================== Schönhage–Strassen algorithm algorithm via Kronecker substitution ====================== */

    /**
     * Kronecker substitution adapted from https://github.com/tbuktu/ntru/blob/master/src/main/java/net/sf/ntru/polynomial/BigIntPolynomial.java
     */
    static UnivariatePolynomial<BigInteger> squareKronecker(UnivariatePolynomial<BigInteger> poly) {
        return UnivariatePolynomial<BigInteger>.create(Rings.Z, squareKronecker0(poly));
    }

    /**
     * Kronecker substitution adapted from https://github.com/tbuktu/ntru/blob/master/src/main/java/net/sf/ntru/polynomial/BigIntPolynomial.java
     */
    private static BigInteger[] squareKronecker0(UnivariatePolynomial<BigInteger> poly) {
        int len = poly.Degree + 1;

        // determine #bits needed per coefficient
        int logMinDigits = 32 - int.LeadingZeroCount(len - 1);
        int maxLength = 0;
        foreach (BigInteger cf in poly.data)
            maxLength = Math.Max(maxLength, (int)cf.GetBitLength());

        int k = logMinDigits + 2 * maxLength + 1;   // in bits
        k = (k + 31) / 32;   // in ints

        // encode each polynomial into an int[]
        int[] pInt = toIntArray(poly, k);
        int[] cInt = toIntArray(BigInteger.Pow(toBigInteger(pInt), 2));

        // decode poly coefficients from the product
        BigInteger[] cPoly = new BigInteger[2 * len - 1];
        decodePoly(k, cInt, cPoly);
        return cPoly;
    }

    private static void decodePoly(int k, int[] cInt, BigInteger[] cPoly) {
        BigInteger _2k = BigInteger.One << k * 32;
        Array.Fill(cPoly, BigInteger.Zero);
        for (int i = 0; i < cPoly.Length; i++) {
            int[] cfInt = new int[k];
            Array.Copy(cInt, i * k, cfInt, 0, k);
            
            BigInteger cf = toBigInteger(cfInt);
            if (cfInt[k - 1] < 0) {   // if coeff > 2^(k-1)
                cf = cf - _2k;

                // add 2^k to cInt which is the same as subtracting coeff
                bool carry;
                int cIdx = (i + 1) * k;
                do {
                    cInt[cIdx]++;
                    carry = cInt[cIdx] == 0;
                    cIdx++;
                } while (carry);
            }
            cPoly[i] = cPoly[i] + cf;
        }
    }

    /**
     * Kronecker substitution adapted from https://github.com/tbuktu/ntru/blob/master/src/main/java/net/sf/ntru/polynomial/BigIntPolynomial.java
     */
    static UnivariatePolynomial<BigInteger> multiplyKronecker(UnivariatePolynomial<BigInteger> poly1,
                      UnivariatePolynomial<BigInteger> poly2) {
        return UnivariatePolynomial<BigInteger>.create(Rings.Z, multiplyKronecker0(poly1, poly2));
    }

    /**
     * Kronecker substitution adapted from https://github.com/tbuktu/ntru/blob/master/src/main/java/net/sf/ntru/polynomial/BigIntPolynomial.java
     */
    static BigInteger[] multiplyKronecker0(UnivariatePolynomial<BigInteger> poly1,
                       UnivariatePolynomial<BigInteger> poly2) {
        if (poly2.Degree > poly1.Degree)
            return multiplyKronecker0(poly2, poly1);
        int len1 = poly1.Degree + 1;
        int len2 = poly2.Degree + 1;

        // determine #bits needed per coefficient
        int logMinDigits = 32 - int.LeadingZeroCount(len1 - 1);
        int maxLengthA = 0;
        foreach (BigInteger cf in poly1.data)
            maxLengthA = Math.Max(maxLengthA, (int)cf.GetBitLength());
        int maxLengthB = 0;
        foreach (BigInteger cf in poly2.data)
            maxLengthB = Math.Max(maxLengthB, (int)cf.GetBitLength());

        int k = logMinDigits + maxLengthA + maxLengthB + 1;   // in bits
        k = (k + 31) / 32;   // in ints

        // encode each polynomial into an int[]
        int[] aInt = toIntArray(poly1, k);
        int[] bInt = toIntArray(poly2, k);
        // multiply
        int[] cInt = toIntArray(toBigInteger(aInt) * toBigInteger(bInt));

        // decode poly coefficients from the product
        BigInteger[] cPoly = new BigInteger[len1 + len2 - 1];
        decodePoly(k, cInt, cPoly);

        int aSign = poly1.lc().Sign;
        int bSign = poly2.lc().Sign;
        if (aSign * bSign < 0)
            for (int i = 0; i < cPoly.Length; i++)
                cPoly[i] = -cPoly[i];

        return cPoly;
    }

    /**
     * Converts a <code>int</code> array to a {@link BigInteger}.
     *
     * @return the <code>BigInteger</code> representation of the array
     */
    private static BigInteger toBigInteger(int[] a) {
        byte[] b = new byte[a.Length * 4];
        for (int i = 0; i < a.Length; i++) {
            int iRev = a.Length - 1 - i;
            b[i * 4] = (byte) (a[iRev] >>> 24);
            b[i * 4 + 1] = (byte) ((a[iRev] >>> 16) & 0xFF);
            b[i * 4 + 2] = (byte) ((a[iRev] >>> 8) & 0xFF);
            b[i * 4 + 3] = (byte) (a[iRev] & 0xFF);
        }
        return new BigInteger(1, b);
    }

    /**
     * Converts a {@link BigInteger} to an <code>int</code> array.
     *
     * @return an <code>int</code> array that is compatible with the <code>mult()</code> methods
     */
    private static int[] toIntArray(BigInteger a) {
        byte[] aArr = a.ToByteArray();
        int[] b = new int[(aArr.Length + 3) / 4];
        for (int i = 0; i < aArr.Length; i++)
            b[i / 4] += (aArr[aArr.Length - 1 - i] & 0xFF) << ((i % 4) * 8);
        return b;
    }

    private static int[] toIntArray(UnivariatePolynomial<BigInteger> a, int k) {
        int len = a.Degree + 1;
        int sign = a.lc().Sign;

        int[] aInt = new int[len * k];
        for (int i = len - 1; i >= 0; i--) {
            int[] cArr = toIntArray(BigInteger.Abs(a.data[i]));
            if (a.data[i].Sign * sign < 0)
                subShifted(aInt, cArr, i * k);
            else
                addShifted(aInt, cArr, i * k);
        }

        return aInt;
    }

    
    private static void addShifted(int[] a, int[] b, int numElements) {
        bool carry = false;
        int i = 0;
        while (i < Math.Min(b.Length, a.Length - numElements)) {
            int ai = a[i + numElements];
            int sum = ai + b[i];
            if (carry)
                sum++;
            carry = ((sum >>> 31) < (ai >>> 31) + (b[i] >>> 31));   // carry if signBit(sum) < signBit(a)+signBit(b)
            a[i + numElements] = sum;
            i++;
        }
        i += numElements;
        while (carry) {
            a[i]++;
            carry = a[i] == 0;
            i++;
        }
    }

    
    private static void subShifted(int[] a, int[] b, int numElements) {
        bool carry = false;
        int i = 0;
        while (i < Math.Min(b.Length, a.Length - numElements)) {
            int ai = a[i + numElements];
            int diff = ai - b[i];
            if (carry)
                diff--;
            carry = ((diff >>> 31) > (a[i] >>> 31) - (b[i] >>> 31));   // carry if signBit(diff) > signBit(a)-signBit(b)
            a[i + numElements] = diff;
            i++;
        }
        i += numElements;
        while (carry) {
            a[i]--;
            carry = a[i] == -1;
            i++;
        }
    }
    
    // From IUnivariatePolynomial
    public int degree() => Degree;
    public int size() => Degree + 1;
    public bool isLinearOrConstant() => Degree <= 1;
    public bool isLinearExactly() => Degree == 1;
    public bool isZeroCC() => isZeroAt(0);
}
