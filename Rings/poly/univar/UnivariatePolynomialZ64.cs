using System.Diagnostics;
using System.Numerics;
using Rings.io;

namespace Rings.poly.univar;

public sealed class UnivariatePolynomialZ64 : AUnivariatePolynomial64<UnivariatePolynomialZ64> {
    private static readonly long serialVersionUID = 1L;

    /** main constructor */
    public UnivariatePolynomialZ64(long[] data) {
        this.data = data;
        this.Degree = data.Length - 1;
        fixDegree();
        Debug.Assert( data.Length > 0);
    }

    /** copy constructor */
    private UnivariatePolynomialZ64(long[] data, int degree) {
        this.data = data;
        this.Degree = degree;
        Debug.Assert(data.Length > 0);
    }

    /**
     * Parse string into polynomial
     */
    public static UnivariatePolynomialZ64 parse(String str) {
        return UnivariatePolynomial<BigInteger>.asOverZ64(UnivariatePolynomial<BigInteger>.parse(str, Rings.Z));
    }

    /**
     * Creates Z[x] polynomial from the specified coefficients
     *
     * @param data coefficients
     * @return Z[x] polynomial
     */
    public static UnivariatePolynomialZ64 create(params long[] data) {
        return new UnivariatePolynomialZ64(data);
    }

    /**
     * Creates monomial {@code coefficient * x^exponent}
     *
     * @param coefficient monomial coefficient
     * @param exponent    monomial exponent
     * @return {@code coefficient * x^exponent}
     */
    public static UnivariatePolynomialZ64 monomial(long coefficient, int exponent) {
        long[] data = new long[exponent + 1];
        data[exponent] = coefficient;
        return new UnivariatePolynomialZ64(data);
    }

    /**
     * Creates zero polynomial
     *
     * @return zero polynomial
     */
    public static UnivariatePolynomialZ64 zero() {
        return new UnivariatePolynomialZ64(new long[]{0}, 0);
    }

    /**
     * Creates unit polynomial
     *
     * @return unit polynomial
     */
    public static UnivariatePolynomialZ64 one() {
        return new UnivariatePolynomialZ64(new long[]{1}, 0);
    }

    /**
     * Returns constant with specified value
     *
     * @return constant with specified value
     */
    public static UnivariatePolynomialZ64 constant(long value) {
        return new UnivariatePolynomialZ64(new long[]{value}, 0);
    }

    
    public UnivariatePolynomialZ64 setCoefficientRingFrom(UnivariatePolynomialZ64 univariatePolynomialZ64) {
        return univariatePolynomialZ64.clone();
    }

    /**
     * Reduces this polynomial modulo {@code modulus} and returns the result.
     *
     * @param modulus the modulus
     * @param copy    whether to copy the internal data or reduce inplace (in which case the data of this will be lost)
     * @return this modulo {@code modulus}
     */
    public UnivariatePolynomialZp64 modulus(long modulus, bool copy) {
        return UnivariatePolynomialZp64.create(modulus, copy ? (long[])data.Clone() : data);
    }

    /**
     * Reduces (copied) polynomial modulo {@code modulus} and returns the result.
     *
     * @param modulus the modulus
     * @return a copy of this modulo {@code modulus}
     */
    public UnivariatePolynomialZp64 modulus(long _modulus) {
        return modulus(_modulus, true);
    }

    /**
     * Reduces this polynomial modulo {@code modulus} and returns the result.
     *
     * @param ring the modulus
     * @param copy whether to copy the internal data or reduce inplace (in which case the data of this will be lost)
     * @return this modulo {@code modulus}
     */
    public UnivariatePolynomialZp64 modulus(IntegersZp64 ring, bool copy) {
        long[] data = copy ? (long[])this.data.Clone() : this.data;
        for (int i = Degree; i >= 0; --i)
            data[i] = ring.modulus(data[i]);
        return UnivariatePolynomialZp64.createUnsafe(ring, data);
    }

    /**
     * Reduces (copied) polynomial modulo {@code modulus} and returns the result.
     *
     * @param ring the modulus
     * @return a copy of this modulo {@code modulus}
     */
    public UnivariatePolynomialZp64 modulus(IntegersZp64 ring) {
        return modulus(ring, true);
    }

    /** internal API */
    UnivariatePolynomialZp64 modulusUnsafe(long modulus) {
        return UnivariatePolynomialZp64.createUnsafe(modulus, data);
    }

    /**
     * {@inheritDoc}. The ring of the result is {@link Rings#Z}
     */
    
    public override UnivariatePolynomial<BigInteger> toBigPoly() {
        return UnivariatePolynomial<BigInteger>.createUnsafe(Rings.Z, dataToBigIntegers());
    }

    /**
     * Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
     *
     * @return Mignotte's bound
     */
    public double mignotteBound() {
        return Math.Pow(2.0, Degree) * norm2();
    }

    /**
     * Evaluates this poly at a given rational point {@code num/den}
     *
     * @param num point numerator
     * @param den point denominator
     * @return value at {@code num/den}
     * @throws ArithmeticException if the result is not integer
     */
    public long evaluateAtRational(long num, long den) {
        if (num == 0)
            return cc();
        long res = 0;
        Magic magic = magicSigned(den);
        for (int i = Degree; i >= 0; --i) {
            long x = multiply(res, num);
            long q = divideSignedFast(x, magic);
            if (q * den != x)
                throw new ArgumentException("The answer is not integer");
            res = add(q, data[i]);
        }
        return res;
    }

    
    public UnivariatePolynomialZ64 getRange(int from, int to) {
        return new UnivariatePolynomialZ64(Arrays.copyOfRange(data, from, to));
    }
    
    
    public bool sameCoefficientRingWith(UnivariatePolynomialZ64 oth) {return true;}

    
    public override UnivariatePolynomialZ64 createFromArray(long[] data) {
        return new UnivariatePolynomialZ64(data);
    }

    
    public override UnivariatePolynomialZ64 createMonomial(long coefficient, int degree) {
        return monomial(coefficient, degree);
    }

    
    public bool isOverField() {return false;}

    
    public bool isOverFiniteField() {return false;}

    
    public bool isOverZ() {return true;}

    
    public BigInteger coefficientRingCardinality() {return null;}

    
    public BigInteger coefficientRingCharacteristic() {
        return BigInteger.Zero;
    }

    
    public bool isOverPerfectPower() {
        return false;
    }

    
    public BigInteger coefficientRingPerfectPowerBase() {
        return null;
    }

    
    public BigInteger coefficientRingPerfectPowerExponent() {
        return null;
    }

    /**
     * Returns the content of this poly (gcd of its coefficients)
     *
     * @return polynomial content
     */
    
    public override long content() {
        if (Degree == 0)
            return data[0];
        return MachineArithmetic.gcd(data, 0, Degree + 1);
    }


    public override long add(long a, long b) {return MachineArithmetic.safeAdd(a, b);}


    public override long  subtract(long a, long b) {return MachineArithmetic.safeSubtract(a, b);}


    public override long multiply(long a, long b) {return MachineArithmetic.safeMultiply(a, b);}


    public override long negate(long a) {return MachineArithmetic.safeNegate(a);}


    public override long valueOf(long a) {return a;}

    
    public UnivariatePolynomialZ64 monic() {
        if (isZero())
            return this;
        return divideOrNull(lc());
    }

    
    public override UnivariatePolynomialZ64 monic(long factor) {
        long lc = this.lc();
        long gcd = MachineArithmetic.gcd(lc, factor);
        factor = factor / gcd;
        lc = lc / gcd;
        UnivariatePolynomialZ64 r = divideOrNull(lc);
        if (r == null)
            return null;
        return r.multiply(factor);
    }

    /**
     * Divides this polynomial by a {@code factor} or returns {@code null} (causing loss of internal data) if some of
     * the elements can't be exactly divided by the {@code factor}. NOTE: is {@code null} is returned, the content of
     * {@code this} is destroyed.
     *
     * @param factor the factor
     * @return {@code this} divided by the {@code factor} or {@code null}
     */
    public UnivariatePolynomialZ64 divideOrNull(long factor) {
        if (factor == 0)
            throw new ArithmeticException("Divide by zero");
        if (factor == 1)
            return this;
        Magic magic = magicSigned(factor);
        for (int i = Degree; i >= 0; --i) {
            long l = divideSignedFast(data[i], magic);
            if (l * factor != data[i])
                return null;
            data[i] = l;
        }
        return this;
    }

    
    public UnivariatePolynomialZ64 divideByLC(UnivariatePolynomialZ64 other) {
        return divideOrNull(other.lc());
    }

    
    public UnivariatePolynomialZ64 multiplyByBigInteger(BigInteger factor) {
        return multiply((long)factor);
    }

    /** internal API */
    UnivariatePolynomialZ64 multiplyUnsafe(long factor) {
        for (int i = Degree; i >= 0; --i)
            data[i] *= factor;
        return this;
    }

    
    public UnivariatePolynomialZ64 multiply(UnivariatePolynomialZ64 oth) {
        if (isZero())
            return this;
        if (oth.isZero())
            return toZero();
        if (this == oth)
            return square();

        if (oth.Degree == 0)
            return multiply(oth.data[0]);
        if (Degree == 0) {
            long factor = data[0];
            data = (long[])oth.data.Clone();
            Degree = oth.Degree;
            return multiply(factor);
        }

        double rBound = normMax() * oth.normMax() * Math.Max(Degree + 1, oth.Degree + 1);
        if (rBound < long.MaxValue)
            // we can apply fast integer arithmetic
            data = multiplyUnsafe0(oth);
        else
            data = multiplySafe0(oth);

        Degree += oth.Degree;
        fixDegree();
        return this;
    }

    /** internal API */
    UnivariatePolynomialZ64 multiplyUnsafe(UnivariatePolynomialZ64 oth) {
        if (isZero())
            return this;
        if (oth.isZero())
            return toZero();
        if (this == oth)
            return square();

        if (oth.Degree == 0)
            return multiply(oth.data[0]);
        if (Degree == 0) {
            long factor = data[0];
            data = (long[])oth.data.Clone();
            Degree = oth.Degree;
            return multiplyUnsafe(factor);
        }

        data = multiplyUnsafe0(oth);
        Degree += oth.Degree;
        fixDegree();
        return this;
    }

    
    public UnivariatePolynomialZ64 square() {
        if (isZero())
            return this;
        if (Degree == 0)
            return multiply(data[0]);

        double norm1 = normMax();
        double rBound = norm1 * norm1 * (Degree + 1);
        if (rBound < long.MaxValue)
            // we can apply fast integer arithmetic
            data = squareUnsafe0();
        else
            data = squareSafe0();

        Degree += Degree;
        fixDegree();
        return this;
    }

    
    public UnivariatePolynomialZ64 derivative() {
        if (isConstant())
            return createZero();
        long[] newData = new long[Degree];
        for (int i = Degree; i > 0; --i)
            newData[i - 1] = multiply(data[i], i);
        return createFromArray(newData);
    }

    
    public UnivariatePolynomialZ64 clone() {
        return new UnivariatePolynomialZ64((long[])data.Clone(), Degree);
    }

    
    public UnivariatePolynomialZ64 parsePoly(String str) {
        return parse(str);
    }

    
    public String coefficientRingToString(IStringifier<UnivariatePolynomialZ64> stringifier) {
        return "Z";
    }

    
    public AMultivariatePolynomial composition(AMultivariatePolynomial value) {
        throw new InvalidOperationException();
    }

    
    public AMultivariatePolynomial asMultivariate(Comparator<DegreeVector> ordering) {
        throw new InvalidOperationException();
    }
}
