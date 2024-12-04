using System.Diagnostics;
using System.Numerics;
using Rings.io;
using ArgumentException = System.ArgumentException;

namespace Rings.poly.univar;

public sealed class UnivariatePolynomialZp64 : AUnivariatePolynomial64<UnivariatePolynomialZp64>
{
    private static long serialVersionUID = 1L;

    public IntegersZp64 ring;

    private UnivariatePolynomialZp64(IntegersZp64 ring, long[] data, int degree)
    {
        this.ring = ring;
        this.data = data;
        this.Degree = degree;
        Debug.Assert(data.Length > 0);
    }

    private UnivariatePolynomialZp64(IntegersZp64 ring, long[] data) : this(ring, data, data.Length - 1)
    {
        fixDegree();
    }

    private static void checkModulus(long modulus)
    {
        if (modulus.CompareTo(MachineArithmetic.MAX_SUPPORTED_MODULUS) > 0)
            throw new ArgumentException("Too large modulus. Modulus should be less than 2^" +
                                        MachineArithmetic.MAX_SUPPORTED_MODULUS_BITS);
    }


    [Obsolete]
    public static UnivariatePolynomialZp64 parse(String str, long modulus)
    {
        return parse(str, new IntegersZp64(modulus));
    }


    [Obsolete]
    public static UnivariatePolynomialZp64 parse(String str, IntegersZp64 modulus)
    {
        return UnivariatePolynomial<BigInteger>.asOverZp64(
            UnivariatePolynomial<BigInteger>.parse(str, modulus.asGenericRing()));
    }


    public static UnivariatePolynomialZp64 parse(String str, IntegersZp64 modulus, String variable)
    {
        return UnivariatePolynomial<BigInteger>.asOverZp64(
            UnivariatePolynomial<BigInteger>.parse(str, modulus.asGenericRing(), variable));
    }


    public static UnivariatePolynomialZp64 create(long modulus, long[] data)
    {
        return create(new IntegersZp64(modulus), data);
    }


    public static UnivariatePolynomialZp64 create(IntegersZp64 ring, long[] data)
    {
        ring.modulus(data);
        return new UnivariatePolynomialZp64(ring, data);
    }


    public static UnivariatePolynomialZp64 linear(long cc, long lc, long modulus)
    {
        return create(modulus, new long[] { cc, lc });
    }


    public static UnivariatePolynomialZp64 createUnsafe(long modulus, long[] data)
    {
        return new UnivariatePolynomialZp64(new IntegersZp64(modulus), data);
    }


    public static UnivariatePolynomialZp64 createUnsafe(IntegersZp64 ring, long[] data)
    {
        return new UnivariatePolynomialZp64(ring, data);
    }


    public static UnivariatePolynomialZp64 monomial(long modulus, long coefficient, int exponent)
    {
        IntegersZp64 ring = new IntegersZp64(modulus);
        coefficient = ring.modulus(coefficient);
        long[] data = new long[exponent + 1];
        data[exponent] = coefficient;
        return new UnivariatePolynomialZp64(ring, data);
    }


    public static UnivariatePolynomialZp64 constant(long modulus, long value)
    {
        return constant(new IntegersZp64(modulus), value);
    }


    public static UnivariatePolynomialZp64 constant(IntegersZp64 ring, long value)
    {
        return new UnivariatePolynomialZp64(ring, new long[] { ring.modulus(value) }, 0);
    }


    public static UnivariatePolynomialZp64 zero(long modulus)
    {
        return constant(modulus, 0L);
    }


    public static UnivariatePolynomialZp64 zero(IntegersZp64 ring)
    {
        return new UnivariatePolynomialZp64(ring, new long[] { 0L }, 0);
    }


    public static UnivariatePolynomialZp64 one(long modulus)
    {
        return constant(modulus, 1L);
    }


    public static UnivariatePolynomialZp64 one(IntegersZp64 ring)
    {
        return new UnivariatePolynomialZp64(ring, new long[] { 1L }, 0);
    }


    public UnivariatePolynomialZp64 setCoefficientRingFrom(UnivariatePolynomialZp64 univariatePolynomialZp64)
    {
        return setModulus(univariatePolynomialZp64.ring);
    }


    public long modulus()
    {
        return ring.Modulus;
    }


    public UnivariatePolynomialZp64 setModulusUnsafe(long newModulus)
    {
        return setModulusUnsafe(new IntegersZp64(newModulus));
    }


    public UnivariatePolynomialZp64 setModulusUnsafe(IntegersZp64 newModulus)
    {
        return new UnivariatePolynomialZp64(newModulus, data, Degree);
    }


    public UnivariatePolynomialZp64 setModulus(long newModulus)
    {
        long[] newData = (long[])data.Clone();
        IntegersZp64 newDomain = new IntegersZp64(newModulus);
        newDomain.modulus(newData);
        return new UnivariatePolynomialZp64(newDomain, newData);
    }


    public UnivariatePolynomialZp64 setModulus(IntegersZp64 newDomain)
    {
        long[] newData = (long[])data.Clone();
        newDomain.modulus(newData);
        return new UnivariatePolynomialZp64(newDomain, newData);
    }

    public UnivariatePolynomialZ64 asPolyZSymmetric()
    {
        long[] newData = new long[Degree + 1];
        for (int i = Degree; i >= 0; --i)
            newData[i] = ring.symmetricForm(data[i]);
        return UnivariatePolynomialZ64.create(newData);
    }


    public UnivariatePolynomialZ64 asPolyZ(bool copy)
    {
        return UnivariatePolynomialZ64.create(copy ? (long[])data.Clone() : data);
    }


    public UnivariatePolynomialZp64 getRange(int from, int to)
    {
        return new UnivariatePolynomialZp64(ring, Arrays.copyOfRange(data, from, to));
    }


    public bool sameCoefficientRingWith(UnivariatePolynomialZp64 oth)
    {
        return ring.Modulus == oth.ring.Modulus;
    }


    public override UnivariatePolynomialZp64 createFromArray(long[] newData)
    {
        ring.modulus(newData);
        return new UnivariatePolynomialZp64(ring, newData);
    }


    public override UnivariatePolynomialZp64 createMonomial(long coefficient, int newDegree)
    {
        long[] newData = new long[newDegree + 1];
        newData[newDegree] = valueOf(coefficient);
        return new UnivariatePolynomialZp64(ring, newData, newDegree);
    }


    public bool isOverField()
    {
        return true;
    }


    public bool isOverFiniteField()
    {
        return true;
    }


    public bool isOverZ()
    {
        return false;
    }


    public BigInteger coefficientRingCardinality()
    {
        return new BigInteger(modulus());
    }


    public BigInteger coefficientRingCharacteristic()
    {
        return new BigInteger(modulus());
    }


    public bool isOverPerfectPower()
    {
        return ring.isPerfectPower();
    }


    public BigInteger coefficientRingPerfectPowerBase()
    {
        return new BigInteger(ring.perfectPowerBase());
    }


    public BigInteger coefficientRingPerfectPowerExponent()
    {
        return new BigInteger(ring.perfectPowerExponent());
    }


    public override long content()
    {
        return lc();
    }


    public override long add(long a, long b)
    {
        return ring.add(a, b);
    }


    public override long subtract(long a, long b)
    {
        return ring.subtract(a, b);
    }


    public override long multiply(long a, long b)
    {
        return ring.multiply(a, b);
    }


    public override long negate(long a)
    {
        return ring.negate(a);
    }


    public override long valueOf(long a)
    {
        return ring.modulus(a);
    }


    public UnivariatePolynomialZp64 monic()
    {
        if (isMonic())
            return this;
        if (isZero())
            return this;
        if (Degree == 0)
        {
            data[0] = 1;
            return this;
        }

        return multiply(ring.reciprocal(lc()));
    }


    public override UnivariatePolynomialZp64 monic(long factor)
    {
        return multiply(multiply(valueOf(factor), ring.reciprocal(lc())));
    }


    public UnivariatePolynomialZp64 divideByLC(UnivariatePolynomialZp64 other)
    {
        return divide(other.lc());
    }

    /**
     * Divide by specified value
     *
     * @param val the value
     * @return {@code this / val}
     */
    public UnivariatePolynomialZp64 divide(long val)
    {
        return multiply(ring.reciprocal(val));
    }


    public UnivariatePolynomialZp64 multiplyByBigInteger(BigInteger factor)
    {
        return multiply((long)(factor % new BigInteger(modulus())));
    }


    public UnivariatePolynomialZp64 multiply(UnivariatePolynomialZp64 oth)
    {
        assertSameCoefficientRingWith(oth);

        if (isZero())
            return this;
        if (oth.isZero())
            return toZero();
        if (this == oth)
            return square();

        assertSameCoefficientRingWith(oth);
        if (oth.Degree == 0)
            return multiply(oth.data[0]);
        if (Degree == 0)
        {
            long factor = data[0];
            this.set(oth);
            return multiply(factor);
        }

        double rBound = normMax() * oth.normMax() * Math.Max(Degree + 1, oth.Degree + 1);
        if (rBound < long.MaxValue)
        {
            // we can apply fast integer arithmetic and then reduce
            data = multiplyUnsafe0(oth);
            Degree += oth.Degree;
            ring.modulus(data);
            fixDegree();
        }
        else
        {
            data = multiplySafe0(oth);
            Degree += oth.Degree;
            fixDegree();
        }

        return this;
    }


    public UnivariatePolynomialZp64 square()
    {
        if (isZero())
            return this;
        if (Degree == 0)
            return multiply(data[0]);

        double norm1 = normMax();
        double rBound = norm1 * norm1 * (Degree + 1);
        if (rBound < long.MaxValue)
        {
            // we can apply fast integer arithmetic and then reduce
            data = squareUnsafe0();
            Degree += Degree;
            ring.modulus(data);
            fixDegree();
        }
        else
        {
            data = squareSafe0();
            Degree += Degree;
            fixDegree();
        }

        return this;
    }


    public UnivariatePolynomialZp64 derivative()
    {
        if (isConstant())
            return createZero();
        long[] newData = new long[Degree];
        if (Degree < ring.Modulus)
            for (int i = Degree; i > 0; --i)
                newData[i - 1] = multiply(data[i], i);
        else
        {
            int i = Degree;
            for (; i >= ring.Modulus; --i)
                newData[i - 1] = multiply(data[i], valueOf(i));
            for (; i > 0; --i)
                newData[i - 1] = multiply(data[i], i);
        }

        return new UnivariatePolynomialZp64(ring, newData);
    }


    public override UnivariatePolynomial<BigInteger> toBigPoly()
    {
        return UnivariatePolynomial<BigInteger>.createUnsafe(new IntegersZp(ring.Modulus), dataToBigIntegers());
    }


    public override UnivariatePolynomialZp64 clone()
    {
        return new UnivariatePolynomialZp64(ring, (long[])data.Clone(), Degree);
    }


    public UnivariatePolynomialZp64 parsePoly(String str)
    {
        return UnivariatePolynomialZ64.parse(str).modulus(ring);
    }


    void multiplyClassicalSafe(long[] result, long[] a, int aFrom, int aTo, long[] b, int bFrom, int bTo)
    {
        if (ring.ModulusFits32)
            multiplyClassicalSafeTrick(result, a, aFrom, aTo, b, bFrom, bTo);
        else
            base.multiplyClassicalSafe(result, a, aFrom, aTo, b, bFrom, bTo);
    }

    void multiplyClassicalSafeNoTrick(long[] result, long[] a, int aFrom, int aTo, long[] b, int bFrom, int bTo)
    {
        base.multiplyClassicalSafe(result, a, aFrom, aTo, b, bFrom, bTo);
    }


    void multiplyClassicalSafeTrick(long[] result, long[] a, int aFrom, int aTo, long[] b, int bFrom, int bTo)
    {
        // this trick is taken from Seyed Mohammad Mahdi Javadi PhD
        // thesis "EFFICIENT ALGORITHMS FOR COMPUTATIONS WITH SPARSE POLYNOMIALS" page 79
        // This trick works only when modulus (so as all elements in arrays) is less
        // than 2^32 (so modulus*modulus fits machine word).

        if (aTo - aFrom > bTo - bFrom)
        {
            multiplyClassicalSafeTrick(result, b, bFrom, bTo, a, aFrom, aTo);
            return;
        }

        long p2 = ring.Modulus * ring.Modulus; // this is safe multiplication
        int
            aDegree = aTo - aFrom - 1,
            bDegree = bTo - bFrom - 1,
            resultDegree = aDegree + bDegree;

        for (int i = 0; i <= resultDegree; ++i)
        {
            long acc = 0;
            for (int j = Math.Max(0, i - bDegree), to = Math.Min(i, aDegree); j <= to; ++j)
            {
                if (acc > 0)
                    acc = acc - p2;
                acc = acc + a[aFrom + j] * b[bFrom + i - j];
            }

            result[i] = ring.modulus(acc);
        }
    }


    public String coefficientRingToString(IStringifier<UnivariatePolynomialZp64> stringifier)
    {
        return ring.toString();
    }


    public MultivariatePolynomialZp64 composition(AMultivariatePolynomial value)
    {
        if (!(value is MultivariatePolynomialZp64))
            throw new IllegalArgumentException();
        if (value.isOne())
            return asMultivariate();
        if (value.isZero())
            return ccAsPoly().asMultivariate();

        MultivariatePolynomialZp64 result = (MultivariatePolynomialZp64)value.createZero();
        for (int i = Degree; i >= 0; --i)
            result = result.multiply((MultivariatePolynomialZp64)value).add(data[i]);
        return result;
    }


    public MultivariatePolynomialZp64 asMultivariate()
    {
        return asMultivariate(MonomialOrder.DEFAULT);
    }


    public MultivariatePolynomialZp64 asMultivariate(Comparator<DegreeVector> ordering)
    {
        return MultivariatePolynomialZp64.asMultivariate(this, 1, 0, ordering);
    }
}