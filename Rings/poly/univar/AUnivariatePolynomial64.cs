using System.Diagnostics;
using System.Numerics;
using System.Text;
using Rings.io;

namespace Rings.poly.univar;

/**
 * Univariate polynomials over machine integers.
 */
public abstract class AUnivariatePolynomial64<lPoly> : IUnivariatePolynomial<lPoly>
    where lPoly : AUnivariatePolynomial64<lPoly>
{
    private static long serialVersionUID = 1L;

    /** array of coefficients { x^0, x^1, ... , x^Degree } */
    protected long[] data;

    /** points to the last non zero element in the data array */
    protected int Degree;

    /** casted self **/
    private lPoly self = (lPoly)this;

    public int degree()
    {
        return Degree;
    }

    /**
     * Returns the i-th coefficient of this poly (coefficient before x^i)
     */
    public long get(int i)
    {
        return i > Degree ? 0 : data[i];
    }

    /**
     * Sets i-th element of this poly with the specified value
     */
    public lPoly set(int i, long el)
    {
        el = valueOf(el);
        if (el == 0)
        {
            if (i > Degree)
                return self;
            data[i] = el;
            fixDegree();
            return self;
        }

        ensureCapacity(i);
        data[i] = el;
        fixDegree();
        return self;
    }

    /**
     * Sets hte leading coefficient of this poly with specified value
     */
    public lPoly setLC(long lc)
    {
        return set(Degree, lc);
    }

    public int firstNonZeroCoefficientPosition()
    {
        if (isZero()) return -1;
        int i = 0;
        while (data[i] == 0) ++i;
        return i;
    }

    /**
     * Returns the leading coefficient of this poly
     *
     * @return leading coefficient
     */
    public long lc()
    {
        return data[Degree];
    }

    public lPoly lcAsPoly()
    {
        return createConstant(lc());
    }

    public lPoly ccAsPoly()
    {
        return createConstant(cc());
    }

    public lPoly getAsPoly(int i)
    {
        return createConstant(get(i));
    }

    /**
     * Returns the constant coefficient of this poly
     *
     * @return constant coefficient
     */
    public long cc()
    {
        return data[0];
    }

    public void ensureInternalCapacity(int desiredCapacity)
    {
        if (data.Length < desiredCapacity)
            // the rest will be filled with zeros
            data = Arrays.copyOf(data, desiredCapacity);
    }

    /**
     * Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
     * Degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
     *
     * @param desiredDegree desired Degree
     */
    void ensureCapacity(int desiredDegree)
    {
        if (Degree < desiredDegree)
            Degree = desiredDegree;

        if (data.Length < (desiredDegree + 1))
            data = Arrays.copyOf(data, desiredDegree + 1);
    }

    /**
     * Removes zeroes from the end of {@code data} and adjusts the Degree
     */
    protected void fixDegree()
    {
        int i = Degree;
        while (i >= 0 && data[i] == 0) --i;
        if (i < 0) i = 0;

        if (i != Degree)
        {
            Degree = i;
            // unnecessary clearing
            // Arrays.fill(data, Degree + 1, data.Length, 0);
        }
    }

    /**
     * Converts this to a polynomial over BigIntegers
     *
     * @return polynomial over BigIntegers
     */
    public abstract UnivariatePolynomial<BigInteger> toBigPoly();

    /**
     * Creates new poly with the specified coefficients (over the same ring)
     *
     * @param data the data
     * @return polynomial
     */
    public abstract lPoly createFromArray(long[] data);

    public lPoly createMonomial(int Degree)
    {
        return createMonomial(1L, Degree);
    }

    /**
     * Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
     *
     * @param cc the  constant coefficient
     * @param lc the  leading coefficient
     * @return {@code cc + x * lc}
     */
    public lPoly createLinear(long cc, long lc)
    {
        return createFromArray(new long[] { cc, lc });
    }

    /**
     * Creates monomial {@code coefficient * x^Degree} (with the same coefficient ring)
     *
     * @param coefficient monomial coefficient
     * @param Degree      monomial Degree
     * @return {@code coefficient * x^Degree}
     */
    public abstract lPoly createMonomial(long coefficient, int Degree);

    /**
     * Creates constant polynomial with specified value (with the same coefficient ring)
     *
     * @param val the value
     * @return constant polynomial with specified value
     */
    public lPoly createConstant(long val)
    {
        return createFromArray(new long[] { val });
    }

    public lPoly createZero()
    {
        return createConstant(0);
    }

    public lPoly createOne()
    {
        return createConstant(1);
    }


    public bool isZeroAt(int i)
    {
        return i >= data.Length || data[i] == 0;
    }

    public lPoly setZero(int i)
    {
        if (i < data.Length)
            data[i] = 0;
        return self;
    }


    public lPoly setFrom(int indexInThis, lPoly poly, int indexInPoly)
    {
        ensureCapacity(indexInThis);
        data[indexInThis] = poly.get(indexInPoly);
        fixDegree();
        return self;
    }

    public bool isZero()
    {
        return data[Degree] == 0;
    }

    public bool isOne()
    {
        return Degree == 0 && data[0] == 1;
    }


    public bool isMonic()
    {
        return lc() == 1;
    }


    public bool isUnitCC()
    {
        return cc() == 1;
    }


    public bool isConstant()
    {
        return Degree == 0;
    }


    public bool isMonomial()
    {
        for (int i = Degree - 1; i >= 0; --i)
            if (data[i] != 0)
                return false;
        return true;
    }


    public int signumOfLC()
    {
        return long.Sign(lc());
    }

    /**
     * Returns L1 norm of this polynomial, i.e. sum of abs coefficients
     *
     * @return L1 norm of {@code this}
     */
    public double norm1()
    {
        double norm = 0;
        for (int i = 0; i <= Degree; ++i)
            norm += Math.Abs(data[i]);
        return norm;
    }

    /**
     * Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
     *
     * @return L2 norm of {@code this}
     */
    public double norm2()
    {
        double norm = 0;
        for (int i = 0; i <= Degree; ++i)
            norm += ((double)data[i]) * data[i];
        return Math.Ceiling(Math.Sqrt(norm));
    }

    /**
     * Returns max coefficient (by absolute value) of this poly
     *
     * @return max coefficient (by absolute value)
     */
    public double normMax()
    {
        return (double)maxAbsCoefficient();
    }

    /**
     * Returns max coefficient (by absolute value) of this poly
     *
     * @return max coefficient (by absolute value)
     */
    public long maxAbsCoefficient()
    {
        long max = Math.Abs(data[0]);
        for (int i = 1; i <= Degree; ++i)
            max = Math.Max(Math.Abs(data[i]), max);
        return max;
    }

    public lPoly toZero()
    {
        Array.Fill(data, 0, Degree + 1, 0);
        Degree = 0;
        return self;
    }


    public lPoly set(lPoly oth)
    {
        if (oth == this)
            return self;
        this.data = (long[])oth.data.Clone();
        this.Degree = oth.Degree;
        Debug.Assert(data.Length > 0);
        return self;
    }


    public lPoly setAndDestroy(lPoly oth)
    {
        this.data = oth.data;
        oth.data = null; // destroy
        this.Degree = oth.Degree;
        Debug.Assert(data.Length > 0);
        return self;
    }

    public lPoly shiftLeft(int offset)
    {
        if (offset == 0)
            return self;
        if (offset > Degree)
            return toZero();

        System.arraycopy(data, offset, data, 0, Degree - offset + 1);
        Array.Fill(data, Degree - offset + 1, Degree + 1, 0);
        Degree = Degree - offset;
        return self;
    }


    public lPoly shiftRight(int offset)
    {
        if (offset == 0)
            return self;
        int Degree = this.Degree;
        ensureCapacity(offset + Degree);
        System.arraycopy(data, 0, data, offset, Degree + 1);
        Array.Fill(data, 0, offset, 0);
        return self;
    }


    public lPoly truncate(int newDegree)
    {
        if (newDegree >= Degree)
            return self;
        Array.Fill(data, newDegree + 1, Degree + 1, 0);
        Degree = newDegree;
        fixDegree();
        return self;
    }


    public lPoly reverse()
    {
        ArraysUtil.reverse(data, 0, Degree + 1);
        fixDegree();
        return self;
    }

    /**
     * Returns the content of this poly (gcd of its coefficients)
     *
     * @return polynomial content
     */
    public abstract long content();


    public lPoly contentAsPoly()
    {
        return createConstant(content());
    }


    public lPoly primitivePart()
    {
        if (isZero())
            return self;
        long content = this.content();
        if (lc() < 0)
            content = -content;
        if (content == -1)
            return negate();
        return primitivePart0(content);
    }


    public lPoly primitivePartSameSign()
    {
        return primitivePart0(content());
    }

    private lPoly primitivePart0(long content)
    {
        if (content == 1)
            return self;
        Magic magic = magicSigned(content);
        for (int i = Degree; i >= 0; --i)
            data[i] = divideSignedFast(data[i], magic);
        return self;
    }

    /** addition in the coefficient ring **/
    public abstract long add(long a, long b);

    /** subtraction in the coefficient ring **/
    public abstract long subtract(long a, long b);

    /** multiplication in the coefficient ring **/
    public abstract long multiply(long a, long b);

    /** negation in the coefficient ring **/
    public abstract long negate(long a);

    /** convert long to element of this coefficient ring **/
    public abstract long valueOf(long a);

    /**
     * Evaluates this poly at a given {@code point} (via Horner method).
     *
     * @param point {@code point}
     * @return value at {@code point}
     */
    public long evaluate(long point)
    {
        if (point == 0)
            return cc();

        point = valueOf(point);
        long res = 0;
        for (int i = Degree; i >= 0; --i)
            res = add(multiply(res, point), data[i]);
        return res;
    }


    public lPoly composition(lPoly value)
    {
        if (value.isOne())
            return this.clone();
        if (value.isZero())
            return ccAsPoly();

        lPoly result = createZero();
        for (int i = Degree; i >= 0; --i)
            result = result.multiply(value).add(data[i]);
        return result;
    }

    /**
     * Shifts variable x -> x + value and returns the result (new instance)
     *
     * @param value shift amount
     * @return a copy of this with x -> x + value
     */
    public lPoly shift(long value)
    {
        return composition(createLinear(value, 1));
    }

    /**
     * Sets {@code this} to its monic part multiplied by the {@code factor};
     *
     * @param factor the factor
     * @return {@code this}
     */
    public abstract lPoly monic(long factor);


    public lPoly monicWithLC(lPoly other)
    {
        if (lc() == other.lc())
            return self;
        return monic(other.lc());
    }

    /**
     * Add constant to this.
     *
     * @param val some number
     * @return this + val
     */
    public lPoly add(long val)
    {
        data[0] = add(data[0], valueOf(val));
        fixDegree();
        return self;
    }

    /**
     * Subtract constant from this.
     *
     * @param val some number
     * @return this + val
     */
    public lPoly subtract(long val)
    {
        data[0] = subtract(data[0], valueOf(val));
        fixDegree();
        return self;
    }


    public lPoly decrement()
    {
        return subtract(createOne());
    }


    public lPoly increment()
    {
        return add(createOne());
    }


    public lPoly add(lPoly oth)
    {
        assertSameCoefficientRingWith(oth);

        if (oth.isZero())
            return self;
        if (isZero())
            return set(oth);

        assertSameCoefficientRingWith(oth);
        ensureCapacity(oth.Degree);
        for (int i = oth.Degree; i >= 0; --i)
            data[i] = add(data[i], oth.data[i]);
        fixDegree();
        return self;
    }

    /**
     * Adds {@code coefficient*x^exponent} to {@code this}
     *
     * @param coefficient monomial coefficient
     * @param exponent    monomial exponent
     * @return {@code this + coefficient*x^exponent}
     */
    public lPoly addMonomial(long coefficient, int exponent)
    {
        if (coefficient == 0)
            return self;

        ensureCapacity(exponent);
        data[exponent] = add(data[exponent], valueOf(coefficient));
        fixDegree();
        return self;
    }

    /**
     * Adds {@code oth * factor} to {@code this}
     *
     * @param oth    the polynomial
     * @param factor the factor
     * @return {@code this + oth * factor} modulo {@code modulus}
     */
    public lPoly addMul(lPoly oth, long factor)
    {
        assertSameCoefficientRingWith(oth);

        if (oth.isZero())
            return self;

        factor = valueOf(factor);
        if (factor == 0)
            return self;

        assertSameCoefficientRingWith(oth);
        ensureCapacity(oth.Degree);
        for (int i = oth.Degree; i >= 0; --i)
            data[i] = add(data[i], multiply(factor, oth.data[i]));
        fixDegree();
        return self;
    }


    public lPoly subtract(lPoly oth)
    {
        assertSameCoefficientRingWith(oth);

        if (oth.isZero())
            return self;
        if (isZero())
            return set(oth).negate();

        assertSameCoefficientRingWith(oth);
        ensureCapacity(oth.Degree);
        for (int i = oth.Degree; i >= 0; --i)
            data[i] = subtract(data[i], oth.data[i]);
        fixDegree();
        return self;
    }

    /**
     * Subtracts {@code factor * x^exponent * oth} from {@code this}
     *
     * @param oth      the polynomial
     * @param factor   the factor
     * @param exponent the exponent
     * @return {@code this - factor * x^exponent * oth}
     */
    public lPoly subtract(lPoly oth, long factor, int exponent)
    {
        assertSameCoefficientRingWith(oth);

        if (oth.isZero())
            return self;

        factor = valueOf(factor);
        if (factor == 0)
            return self;

        assertSameCoefficientRingWith(oth);
        for (int i = oth.Degree + exponent; i >= exponent; --i)
            data[i] = subtract(data[i], multiply(factor, oth.data[i - exponent]));

        fixDegree();
        return self;
    }


    public lPoly negate()
    {
        for (int i = Degree; i >= 0; --i)
            data[i] = negate(data[i]);
        return self;
    }


    public lPoly multiplyByLC(lPoly other)
    {
        return multiply(other.lc());
    }


    public lPoly multiply(long factor)
    {
        factor = valueOf(factor);
        if (factor == 1)
            return self;

        if (factor == 0)
            return toZero();

        for (int i = Degree; i >= 0; --i)
            data[i] = multiply(data[i], factor);
        return self;
    }


    public abstract lPoly clone();

    /** convert this long[] data to BigInteger[] */
    protected BigInteger[] dataToBigIntegers()
    {
        BigInteger[] bData = new BigInteger[Degree + 1];
        for (int i = Degree; i >= 0; --i)
            bData[i] = new BigInteger(data[i]);
        return bData;
    }

    /** internal API >>> direct unsafe access to internal storage */
    public long[] getDataReferenceUnsafe()
    {
        return data;
    }


    public int compareTo(lPoly o)
    {
        int c = Degree.CompareTo(o.Degree);
        if (c != 0)
            return c;
        for (int i = Degree; i >= 0; --i)
        {
            c = data[i].CompareTo(o.data[i]);
            if (c != 0)
                return c;
        }

        return 0;
    }


    public String toString()
    {
        return toString(IStringifier<lPoly>.dummy<lPoly>());
    }


    public String toString(IStringifier<lPoly> stringifier)
    {
        if (isConstant())
            return cc().ToString();

        String varString = stringifier.getBindings()
            .GetValueOrDefault(createMonomial(1), IStringifier<lPoly>.defaultVar());
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i <= Degree; i++)
        {
            long el = data[i];
            if (el == 0)
                continue;

            String cfString;
            if (el != 1 || i == 0)
                cfString = el.ToString();
            else
                cfString = "";

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

    public String toStringForCopy()
    {
        String s = ArraysUtil.toString(data, 0, Degree + 1);
        return "of(" + s.Substring(1, s.Length - 1) + ")";
    }


    public IEnumerable<lPoly> streamAsPolys()
    {
        return stream().Select(createConstant);
    }

    /**
     * Returns a sequential {@code Stream} with coefficients of this as its source.
     *
     * @return a sequential {@code Stream} over the coefficients in this polynomial
     */
    public IEnumerable<long> stream()
    {
        return Arrays.stream(data, 0, Degree + 1);
    }

    /**
     * Applies transformation function to this and returns the result. This method is equivalent of {@code
     * stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
     *
     * @param ring   ring of the new polynomial
     * @param mapper function that maps coefficients of this to coefficients of the result
     * @param <T>    result elements type
     * @return a new polynomial with the coefficients obtained from this by applying {@code mapper}
     */
    public UnivariatePolynomial<T> mapCoefficients<T>(Ring<T> ring, Func<long, T> mapper)
    {
        return new UnivariatePolynomial<T>(ring, stream().Select(mapper).ToArray());
    }


    public bool equals(Object obj)
    {
        if (obj.GetType() != this.GetType())
            return false;
        var oth = (AUnivariatePolynomial64<lPoly>)obj;
        if (Degree != oth.Degree)
            return false;
        for (int i = 0; i <= Degree; ++i)
            if (data[i] != oth.data[i])
                return false;
        return true;
    }


    public int hashCode()
    {
        int result = 1;
        for (int i = Degree; i >= 0; --i)
        {
            long element = data[i];
            int elementHash = (int)(element ^ (element >>> 32));
            result = 31 * result + elementHash;
        }

        return result;
    }

    /* =========================== Multiplication with safe arithmetic =========================== */

    /** switch to classical multiplication */
    static long KARATSUBA_THRESHOLD = 2048L;

    /** when use Karatsuba fast multiplication */
    static long
        MUL_CLASSICAL_THRESHOLD = 256L * 256L,
        MUL_MOD_CLASSICAL_THRESHOLD = 128L * 128L;

    /** switch algorithms */
    protected long[] multiplyUnsafe0(lPoly oth)
    {
        if (1L * (Degree + 1) * (Degree + 1) <= MUL_CLASSICAL_THRESHOLD)
            return multiplyClassicalUnsafe(data, 0, Degree + 1, oth.data, 0, oth.Degree + 1);
        else
            return multiplyKaratsubaUnsafe(data, 0, Degree + 1, oth.data, 0, oth.Degree + 1);
    }

    /** switch algorithms */
    protected long[] multiplySafe0(lPoly oth)
    {
        if (1L * (Degree + 1) * (Degree + 1) <= MUL_CLASSICAL_THRESHOLD)
            return multiplyClassicalSafe(data, 0, Degree + 1, oth.data, 0, oth.Degree + 1);
        else
            return multiplyKaratsubaSafe(data, 0, Degree + 1, oth.data, 0, oth.Degree + 1);
    }


    /** switch algorithms */
    protected long[] squareUnsafe0()
    {
        if (1L * (Degree + 1) * (Degree + 1) <= MUL_CLASSICAL_THRESHOLD)
            return squareClassicalUnsafe(data, 0, Degree + 1);
        else
            return squareKaratsubaUnsafe(data, 0, Degree + 1);
    }

    /** switch algorithms */
    protected long[] squareSafe0()
    {
        if (1L * (Degree + 1) * (Degree + 1) <= MUL_CLASSICAL_THRESHOLD)
            return squareClassicalSafe(data, 0, Degree + 1);
        else
            return squareKaratsubaSafe(data, 0, Degree + 1);
    }

    /* =========================== Exact multiplication with safe arithmetics =========================== */

    /**
     * Classical n*m multiplication algorithm
     *
     * @param a     the first multiplier
     * @param aFrom begin in a
     * @param aTo   end in a
     * @param b     the second multiplier
     * @param bFrom begin in b
     * @param bTo   end in b
     * @return the result
     */
    long[] multiplyClassicalSafe(long[] a, int aFrom, int aTo, long[] b, int bFrom, int bTo)
    {
        long[] result = new long[aTo - aFrom + bTo - bFrom - 1];
        multiplyClassicalSafe(result, a, aFrom, aTo, b, bFrom, bTo);
        return result;
    }

    /**
     * Classical n*m multiplication algorithm
     *
     * @param result where to write the result
     * @param a      the first multiplier
     * @param aFrom  begin in a
     * @param aTo    end in a
     * @param b      the second multiplier
     * @param bFrom  begin in b
     * @param bTo    end in b
     */
    protected void multiplyClassicalSafe(long[] result, long[] a, int aFrom, int aTo, long[] b, int bFrom, int bTo)
    {
        if (aTo - aFrom > bTo - bFrom)
        {
            multiplyClassicalSafe(result, b, bFrom, bTo, a, aFrom, aTo);
            return;
        }

        for (int i = 0; i < aTo - aFrom; ++i)
        {
            long c = a[aFrom + i];
            if (c != 0)
                for (int j = 0; j < bTo - bFrom; ++j)
                    result[i + j] = add(result[i + j], multiply(c, b[bFrom + j]));
        }
    }

    /**
     * Karatsuba multiplication
     *
     * @param f     the first multiplier
     * @param g     the second multiplier
     * @param fFrom begin in f
     * @param fTo   end in f
     * @param gFrom begin in g
     * @param gTo   end in g
     * @return the result
     */
    long[] multiplyKaratsubaSafe(
        long[] f, int fFrom, int fTo,
        long[] g, int gFrom, int gTo)
    {
        // return zero
        if (fFrom >= fTo || gFrom >= gTo)
            return new long[0];

        // single element in f
        if (fTo - fFrom == 1)
        {
            long[] result = new long[gTo - gFrom];
            for (int i = gFrom; i < gTo; ++i)
                result[i - gFrom] = multiply(f[fFrom], g[i]);
            return result;
        }

        // single element in g
        if (gTo - gFrom == 1)
        {
            long[] result = new long[fTo - fFrom];
            //single element in b
            for (int i = fFrom; i < fTo; ++i)
                result[i - fFrom] = multiply(g[gFrom], f[i]);
            return result;
        }

        // linear factors
        if (fTo - fFrom == 2 && gTo - gFrom == 2)
        {
            long[] result = new long[3];
            //both a and b are linear
            result[0] = multiply(f[fFrom], g[gFrom]);
            result[1] = add(multiply(f[fFrom], g[gFrom + 1]), multiply(f[fFrom + 1], g[gFrom]));
            result[2] = multiply(f[fFrom + 1], g[gFrom + 1]);
            return result;
        }

        //switch to classical
        if (1L * (fTo - fFrom) * (gTo - gFrom) < KARATSUBA_THRESHOLD)
            return multiplyClassicalSafe(g, gFrom, gTo, f, fFrom, fTo);

        if (fTo - fFrom < gTo - gFrom)
            return multiplyKaratsubaSafe(g, gFrom, gTo, f, fFrom, fTo);


        //we now split a and b into 2 parts:
        int split = (fTo - fFrom + 1) / 2;
        //if we can't split b
        if (gFrom + split >= gTo)
        {
            long[] f0g = multiplyKaratsubaSafe(f, fFrom, fFrom + split, g, gFrom, gTo);
            long[] f1g = multiplyKaratsubaSafe(f, fFrom + split, fTo, g, gFrom, gTo);

            long[] result = Arrays.copyOf(f0g, fTo - fFrom + gTo - gFrom - 1);
            for (int i = 0; i < f1g.Length; i++)
                result[i + split] = add(result[i + split], f1g[i]);
            return result;
        }

        int fMid = fFrom + split, gMid = gFrom + split;
        long[] f0g0 = multiplyKaratsubaSafe(f, fFrom, fMid, g, gFrom, gMid);
        long[] f1g1 = multiplyKaratsubaSafe(f, fMid, fTo, g, gMid, gTo);

        // f0 + f1
        long[] f0_plus_f1 = new long[Math.Max(fMid - fFrom, fTo - fMid)];
        System.arraycopy(f, fFrom, f0_plus_f1, 0, fMid - fFrom);
        for (int i = fMid; i < fTo; ++i)
            f0_plus_f1[i - fMid] = add(f0_plus_f1[i - fMid], f[i]);

        //g0 + g1
        long[] g0_plus_g1 = new long[Math.Max(gMid - gFrom, gTo - gMid)];
        System.arraycopy(g, gFrom, g0_plus_g1, 0, gMid - gFrom);
        for (int i = gMid; i < gTo; ++i)
            g0_plus_g1[i - gMid] = add(g0_plus_g1[i - gMid], g[i]);

        long[] mid = multiplyKaratsubaSafe(f0_plus_f1, 0, f0_plus_f1.Length, g0_plus_g1, 0, g0_plus_g1.Length);

        if (mid.Length < f0g0.Length)
            mid = Arrays.copyOf(mid, f0g0.Length);
        if (mid.Length < f1g1.Length)
            mid = Arrays.copyOf(mid, f1g1.Length);

        //subtract f0g0, f1g1
        for (int i = 0; i < f0g0.Length; ++i)
            mid[i] = subtract(mid[i], f0g0[i]);
        for (int i = 0; i < f1g1.Length; ++i)
            mid[i] = subtract(mid[i], f1g1[i]);


        long[] result = Arrays.copyOf(f0g0, (fTo - fFrom) + (gTo - gFrom) - 1);
        for (int i = 0; i < mid.Length; ++i)
            result[i + split] = add(result[i + split], mid[i]);
        for (int i = 0; i < f1g1.Length; ++i)
            result[i + 2 * split] = add(result[i + 2 * split], f1g1[i]);

        return result;
    }

    long[] squareClassicalSafe(long[] a, int from, int to)
    {
        long[] x = new long[(to - from) * 2 - 1];
        squareClassicalSafe(x, a, from, to);
        return x;
    }


    /**
     * Square the poly {@code data} using classical algorithm
     *
     * @param result result destination
     * @param data   the data
     * @param from   data from
     * @param to     end point in the {@code data}
     */
    void squareClassicalSafe(long[] result, long[] data, int from, int to)
    {
        int len = to - from;
        for (int i = 0; i < len; ++i)
        {
            long c = data[from + i];
            if (c != 0)
                for (int j = 0; j < len; ++j)
                    result[i + j] = add(result[i + j], multiply(c, data[from + j]));
        }
    }

    /**
     * Karatsuba squaring
     *
     * @param f     the data
     * @param fFrom begin in f
     * @param fTo   end in f
     * @return the result
     */
    long[] squareKaratsubaSafe(long[] f, int fFrom, int fTo)
    {
        if (fFrom >= fTo)
            return new long[0];
        if (fTo - fFrom == 1)
            return new long[] { multiply(f[fFrom], f[fFrom]) };
        if (fTo - fFrom == 2)
        {
            long[] result = new long[3];
            result[0] = multiply(f[fFrom], f[fFrom]);
            result[1] = multiply(multiply(valueOf(2L), f[fFrom]), f[fFrom + 1]);
            result[2] = multiply(f[fFrom + 1], f[fFrom + 1]);
            return result;
        }

        //switch to classical
        if (1L * (fTo - fFrom) * (fTo - fFrom) < KARATSUBA_THRESHOLD)
            return squareClassicalSafe(f, fFrom, fTo);


        //we now split a and b into 2 parts:
        int split = (fTo - fFrom + 1) / 2;
        int fMid = fFrom + split;
        long[] f0g0 = squareKaratsubaSafe(f, fFrom, fMid);
        long[] f1g1 = squareKaratsubaSafe(f, fMid, fTo);

        // f0 + f1
        long[] f0_plus_f1 = new long[Math.Max(fMid - fFrom, fTo - fMid)];
        System.arraycopy(f, fFrom, f0_plus_f1, 0, fMid - fFrom);
        for (int i = fMid; i < fTo; ++i)
            f0_plus_f1[i - fMid] = add(f0_plus_f1[i - fMid], f[i]);

        long[] mid = squareKaratsubaSafe(f0_plus_f1, 0, f0_plus_f1.Length);

        if (mid.Length < f0g0.Length)
            mid = Arrays.copyOf(mid, f0g0.Length);
        if (mid.Length < f1g1.Length)
            mid = Arrays.copyOf(mid, f1g1.Length);

        //subtract f0g0, f1g1
        for (int i = 0; i < f0g0.Length; ++i)
            mid[i] = subtract(mid[i], f0g0[i]);
        for (int i = 0; i < f1g1.Length; ++i)
            mid[i] = subtract(mid[i], f1g1[i]);


        long[] result = Arrays.copyOf(f0g0, 2 * (fTo - fFrom) - 1);
        for (int i = 0; i < mid.Length; ++i)
            result[i + split] = add(result[i + split], mid[i]);
        for (int i = 0; i < f1g1.Length; ++i)
            result[i + 2 * split] = add(result[i + 2 * split], f1g1[i]);

        return result;
    }

    /* =========================== Exact multiplication with unsafe arithmetics =========================== */

    /**
     * Classical n*m multiplication algorithm
     *
     * @param result where to write the result
     * @param a      the first multiplier
     * @param aFrom  begin in a
     * @param aTo    end in a
     * @param b      the second multiplier
     * @param bFrom  begin in b
     * @param bTo    end in b
     */
    static void multiplyClassicalUnsafe(long[] result, long[] a, int aFrom, int aTo, long[] b, int bFrom, int bTo)
    {
        if (aTo - aFrom > bTo - bFrom)
        {
            multiplyClassicalUnsafe(result, b, bFrom, bTo, a, aFrom, aTo);
            return;
        }

        for (int i = 0; i < aTo - aFrom; ++i)
        {
            long c = a[aFrom + i];
            if (c != 0)
                for (int j = 0; j < bTo - bFrom; ++j)
                    result[i + j] = result[i + j] + c * b[bFrom + j];
        }
    }

    /**
     * Classical n*m multiplication algorithm
     *
     * @param a     the first multiplier
     * @param aFrom begin in a
     * @param aTo   end in a
     * @param b     the second multiplier
     * @param bFrom begin in b
     * @param bTo   end in b
     * @return the result
     */
    static long[] multiplyClassicalUnsafe(long[] a, int aFrom, int aTo, long[] b, int bFrom, int bTo)
    {
        long[] result = new long[aTo - aFrom + bTo - bFrom - 1];
        multiplyClassicalUnsafe(result, a, aFrom, aTo, b, bFrom, bTo);
        return result;
    }

    /**
     * Karatsuba multiplication
     *
     * @param f     the first multiplier
     * @param g     the second multiplier
     * @param fFrom begin in f
     * @param fTo   end in f
     * @param gFrom begin in g
     * @param gTo   end in g
     * @return the result
     */
    static long[] multiplyKaratsubaUnsafe(
        long[] f, int fFrom, int fTo,
        long[] g, int gFrom, int gTo)
    {
        // return zero
        if (fFrom >= fTo || gFrom >= gTo)
            return new long[0];

        // single element in f
        if (fTo - fFrom == 1)
        {
            long[] result = new long[gTo - gFrom];
            for (int i = gFrom; i < gTo; ++i)
                result[i - gFrom] = f[fFrom] * g[i];
            return result;
        }

        // single element in g
        if (gTo - gFrom == 1)
        {
            long[] result = new long[fTo - fFrom];
            //single element in b
            for (int i = fFrom; i < fTo; ++i)
                result[i - fFrom] = g[gFrom] * f[i];
            return result;
        }

        // linear factors
        if (fTo - fFrom == 2 && gTo - gFrom == 2)
        {
            long[] result = new long[3];
            //both a and b are linear
            result[0] = f[fFrom] * g[gFrom];
            result[1] = f[fFrom] * g[gFrom + 1] + f[fFrom + 1] * g[gFrom];
            result[2] = f[fFrom + 1] * g[gFrom + 1];
            return result;
        }

        //switch to classical
        if (1L * (fTo - fFrom) * (gTo - gFrom) < KARATSUBA_THRESHOLD)
            return multiplyClassicalUnsafe(g, gFrom, gTo, f, fFrom, fTo);

        if (fTo - fFrom < gTo - gFrom)
            return multiplyKaratsubaUnsafe(g, gFrom, gTo, f, fFrom, fTo);


        //we now split a and b into 2 parts:
        int split = (fTo - fFrom + 1) / 2;
        //if we can't split b
        if (gFrom + split >= gTo)
        {
            long[] f0g = multiplyKaratsubaUnsafe(f, fFrom, fFrom + split, g, gFrom, gTo);
            long[] f1g = multiplyKaratsubaUnsafe(f, fFrom + split, fTo, g, gFrom, gTo);

            long[] result = Arrays.copyOf(f0g, fTo - fFrom + gTo - gFrom - 1);
            for (int i = 0; i < f1g.Length; i++)
                result[i + split] = result[i + split] + f1g[i];
            return result;
        }

        int fMid = fFrom + split, gMid = gFrom + split;
        long[] f0g0 = multiplyKaratsubaUnsafe(f, fFrom, fMid, g, gFrom, gMid);
        long[] f1g1 = multiplyKaratsubaUnsafe(f, fMid, fTo, g, gMid, gTo);

        // f0 + f1
        long[] f0_plus_f1 = new long[Math.Max(fMid - fFrom, fTo - fMid)];
        System.arraycopy(f, fFrom, f0_plus_f1, 0, fMid - fFrom);
        for (int i = fMid; i < fTo; ++i)
            f0_plus_f1[i - fMid] = f0_plus_f1[i - fMid] + f[i];

        //g0 + g1
        long[] g0_plus_g1 = new long[Math.Max(gMid - gFrom, gTo - gMid)];
        System.arraycopy(g, gFrom, g0_plus_g1, 0, gMid - gFrom);
        for (int i = gMid; i < gTo; ++i)
            g0_plus_g1[i - gMid] = g0_plus_g1[i - gMid] + g[i];

        long[] mid = multiplyKaratsubaUnsafe(f0_plus_f1, 0, f0_plus_f1.Length, g0_plus_g1, 0, g0_plus_g1.Length);

        if (mid.Length < f0g0.Length)
            mid = Arrays.copyOf(mid, f0g0.Length);
        if (mid.Length < f1g1.Length)
            mid = Arrays.copyOf(mid, f1g1.Length);

        //subtract f0g0, f1g1
        for (int i = 0; i < f0g0.Length; ++i)
            mid[i] = mid[i] - f0g0[i];
        for (int i = 0; i < f1g1.Length; ++i)
            mid[i] = mid[i] - f1g1[i];


        long[] result = Arrays.copyOf(f0g0, (fTo - fFrom) + (gTo - gFrom) - 1);
        for (int i = 0; i < mid.Length; ++i)
            result[i + split] = result[i + split] + mid[i];
        for (int i = 0; i < f1g1.Length; ++i)
            result[i + 2 * split] = result[i + 2 * split] + f1g1[i];

        return result;
    }

    /** classical square */
    static long[] squareClassicalUnsafe(long[] a, int from, int to)
    {
        long[] x = new long[(to - from) * 2 - 1];
        squareClassicalUnsafe(x, a, from, to);
        return x;
    }

    /**
     * Square the poly {@code data} using classical algorithm
     *
     * @param result result destination
     * @param data   the data
     * @param from   data from
     * @param to     end point in the {@code data}
     */
    static void squareClassicalUnsafe(long[] result, long[] data, int from, int to)
    {
        int len = to - from;
        for (int i = 0; i < len; ++i)
        {
            long c = data[from + i];
            if (c != 0)
                for (int j = 0; j < len; ++j)
                    result[i + j] = result[i + j] + c * data[from + j];
        }
    }

    /**
     * Karatsuba squaring
     *
     * @param f     the data
     * @param fFrom begin in f
     * @param fTo   end in f
     * @return the result
     */
    static long[] squareKaratsubaUnsafe(long[] f, int fFrom, int fTo)
    {
        if (fFrom >= fTo)
            return new long[0];
        if (fTo - fFrom == 1)
            return new long[] { f[fFrom] * f[fFrom] };
        if (fTo - fFrom == 2)
        {
            long[] result = new long[3];
            result[0] = f[fFrom] * f[fFrom];
            result[1] = 2L * f[fFrom] * f[fFrom + 1];
            result[2] = f[fFrom + 1] * f[fFrom + 1];
            return result;
        }

        //switch to classical
        if (1L * (fTo - fFrom) * (fTo - fFrom) < KARATSUBA_THRESHOLD)
            return squareClassicalUnsafe(f, fFrom, fTo);


        //we now split a and b into 2 parts:
        int split = (fTo - fFrom + 1) / 2;
        int fMid = fFrom + split;
        long[] f0g0 = squareKaratsubaUnsafe(f, fFrom, fMid);
        long[] f1g1 = squareKaratsubaUnsafe(f, fMid, fTo);

        // f0 + f1
        long[] f0_plus_f1 = new long[Math.Max(fMid - fFrom, fTo - fMid)];
        System.arraycopy(f, fFrom, f0_plus_f1, 0, fMid - fFrom);
        for (int i = fMid; i < fTo; ++i)
            f0_plus_f1[i - fMid] = f0_plus_f1[i - fMid] + f[i];

        long[] mid = squareKaratsubaUnsafe(f0_plus_f1, 0, f0_plus_f1.Length);

        if (mid.Length < f0g0.Length)
            mid = Arrays.copyOf(mid, f0g0.Length);
        if (mid.Length < f1g1.Length)
            mid = Arrays.copyOf(mid, f1g1.Length);

        //subtract f0g0, f1g1
        for (int i = 0; i < f0g0.Length; ++i)
            mid[i] = mid[i] - f0g0[i];
        for (int i = 0; i < f1g1.Length; ++i)
            mid[i] = mid[i] - f1g1[i];


        long[] result = Arrays.copyOf(f0g0, 2 * (fTo - fFrom) - 1);
        for (int i = 0; i < mid.Length; ++i)
            result[i + split] = result[i + split] + mid[i];
        for (int i = 0; i < f1g1.Length; ++i)
            result[i + 2 * split] = result[i + 2 * split] + f1g1[i];

        return result;
    }
}