using System.Numerics;
using System.Text;
using Polynomials.Poly.Multivar;
using Polynomials.Utils;
using UnivariatePolynomialZ64 = Polynomials.Poly.Univar.UnivariatePolynomial<long>;
using UnivariatePolynomialZp64 = Polynomials.Poly.Univar.UnivariatePolynomial<long>;

namespace Polynomials.Poly.Univar;

public interface IUnivariatePolynomial
{
    public object GetAsObject(int i);
    public object CcAsObject();
    public int Degree();
}

public class UnivariatePolynomial<E> : Polynomial<UnivariatePolynomial<E>>, IUnivariatePolynomial
{
    public readonly Ring<E> ring;


    public E[] data;


    public int degree;

    public UnivariatePolynomial(Ring<E> ring, E[] data, int degree)
    {
        this.ring = ring;
        this.data = data;
        this.degree = degree;
    }


    public UnivariatePolynomial(Ring<E> ring, E[] data) : this(ring, data, data.Length - 1)
    {
        FixDegree();
    }


    public static UnivariatePolynomial<E> Create(Ring<E> ring, params E[] data)
    {
        ring.SetToValueOf(data);
        return new UnivariatePolynomial<E>(ring, data);
    }


    public static UnivariatePolynomial<E> CreateUnsafe(Ring<E> ring, E[] data)
    {
        return new UnivariatePolynomial<E>(ring, data);
    }


    public static UnivariatePolynomial<BigInteger> Create(Ring<BigInteger> ring, params long[] data)
    {
        return new UnivariatePolynomial<BigInteger>(ring, data.Select(ring.ValueOfLong).ToArray());
    }


    public static UnivariatePolynomial<BigInteger> Create(params long[] data)
    {
        return Create(Rings.Z, data);
    }


    public static UnivariatePolynomial<E> Constant(Ring<E> ring, E constant)
    {
        return Create(ring, constant);
    }


    public static UnivariatePolynomial<E> Zero(Ring<E> ring)
    {
        return Constant(ring, ring.GetZero());
    }


    public static UnivariatePolynomial<E> One(Ring<E> ring)
    {
        return Constant(ring, ring.GetOne());
    }


    public static UnivariatePolynomialZ64 AsOverZ64(UnivariatePolynomial<BigInteger> poly)
    {
        var data = new long[poly.degree + 1];
        for (var i = 0; i < data.Length; i++)
            data[i] = (long)poly.data[i];
        return new UnivariatePolynomialZ64(Rings.Z64, data);
    }


    public static UnivariatePolynomialZp64 AsOverZp64(UnivariatePolynomial<BigInteger> poly)
    {
        if (poly.ring is not IntegersZp zp)
            throw new ArgumentException("Not a modular ring: " + poly.ring);
        var data = new long[poly.degree + 1];
        for (var i = 0; i < data.Length; i++)
            data[i] = ((long)poly.data[i]);
        return UnivariatePolynomialZp64.Create(Rings.Zp64((long)zp.modulus), data);
    }


    public static UnivariatePolynomialZp64 AsOverZp64(UnivariatePolynomial<BigInteger> poly, IntegersZp64 ring)
    {
        var modulus = ring.modulus;
        var data = new long[poly.degree + 1];
        for (var i = 0; i < data.Length; i++)
            data[i] = (long)(poly.data[i] % modulus);
        return UnivariatePolynomialZp64.Create(ring, data);
    }

    public UnivariatePolynomial<BigInteger> ToBigPoly()
    {
        if (ring is Integers64 && this is UnivariatePolynomialZ64 uZ64)
            return uZ64.MapCoefficients(Rings.Z, e => new BigInteger(e));
        if (ring is IntegersZp64 rZp64 && this is UnivariatePolynomialZ64 uZp64)
            return uZp64.MapCoefficients(Rings.Zp(rZp64.modulus), e => new BigInteger(e));
        if (this is UnivariatePolynomial<BigInteger> bigPoly)
            return bigPoly;

        throw new Exception();
    }

    public static UnivariatePolynomialZp64 AsOverZp64Q(UnivariatePolynomial<Rational<BigInteger>> poly,
        IntegersZp64 ring)
    {
        long modulus = ring.modulus;
        long[] data = new long[poly.degree + 1];
        for (int i = 0; i < data.Length; i++)
            data[i] = ring.Divide((long)(poly.data[i].Numerator() % modulus),
                (long)(poly.data[i].Denominator() % modulus));
        return UnivariatePolynomialZp64.Create(ring, data);
    }
    public UnivariatePolynomialZ64 AsPolyZ(bool copy)
    {
        if (this is UnivariatePolynomialZp64 zp && ring is IntegersZp64)
            return new UnivariatePolynomialZ64(Rings.Z64, copy ? (long[])zp.data.Clone() : zp.data);

        throw new Exception();
    }

    public static UnivariatePolynomial<BigInteger> AsPolyZSymmetric(UnivariatePolynomial<BigInteger> poly)
    {
        if (!(poly.ring is IntegersZp))
            throw new ArgumentException("Not a modular ring: " + poly.ring);
        var ring = (IntegersZp)poly.ring;
        var newData = new BigInteger[poly.degree + 1];
        for (var i = poly.degree; i >= 0; --i)
            newData[i] = ring.SymmetricForm(poly.data[i]);
        return UnivariatePolynomial<BigInteger>.CreateUnsafe(Rings.Z, newData);
    }

    public static UnivariatePolynomial<long> AsPolyZ64Symmetric(UnivariatePolynomial<long> poly)
    {
        if (!(poly.ring is IntegersZp64))
            throw new ArgumentException("Not a modular ring: " + poly.ring);
        var ring = (IntegersZp64)poly.ring;
        var newData = new long[poly.degree + 1];
        for (var i = poly.degree; i >= 0; --i)
            newData[i] = ring.SymmetricForm(poly.data[i]);
        return UnivariatePolynomial<long>.CreateUnsafe(Rings.Z64, newData);
    }
    
    public UnivariatePolynomialZ64 MultiplyUnsafe(long factor)
    {
        var thisZ = this.AsZ64();
        
        for (var i = degree; i >= 0; --i)
            thisZ.data[i] *= factor;
        return thisZ;
    }
    
    public bool IsZ64()
    {
        return ring is Integers64;
    }
    
    public bool IsZp64()
    {
        return ring is IntegersZp64;
    }
    
    public UnivariatePolynomialZ64 MultiplyUnsafe(UnivariatePolynomialZ64 oth)
    {

        var thisZ = this.AsZ64();
        
        if (IsZero())
            return thisZ;
        if (oth.IsZero())
            return thisZ.ToZero();
        if (thisZ == oth)
            return thisZ.Square();
        if (oth.degree == 0)
            return thisZ.Multiply(oth.data[0]);
        if (degree == 0)
        {
            var factor = thisZ.data[0];
            thisZ.data = (long[])oth.data.Clone();
            thisZ.degree = oth.degree;
            return thisZ.MultiplyUnsafe(factor);
        }

        thisZ.data = thisZ.MultiplySafe0(oth);
        thisZ.degree += oth.degree;
        thisZ.FixDegree();
        return thisZ;
    }

    public object GetAsObject(int i)
    {
        return Get(i);
    }

    public object CcAsObject()
    {
        return Cc();
    }

    public int Degree()
    {
        return degree;
    }


    public E this[int i]
    {
        get => Get(i);
        set => Set(i, value);
    }

    public E Get(int i)
    {
        return i > degree ? ring.GetZero() : data[i];
    }


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


    public UnivariatePolynomial<E> SetLC(E lc)
    {
        return Set(degree, lc);
    }
    
    public int NNonZeroTerms() {
        int c = 0;
        for (int i = Degree(); i >= 0; --i)
            if (!IsZeroAt(i))
                ++c;
        return c;
    }


    public int FirstNonZeroCoefficientPosition()
    {
        if (IsZero())
            return -1;
        var i = 0;
        while (ring.IsZero(data[i]))
            ++i;
        return i;
    }


    public UnivariatePolynomial<E> SetRing(Ring<E> newRing)
    {
        if (ring == newRing)
            return Clone();
        E[] newData = (E[])data.Clone();
        newRing.SetToValueOf(newData);
        return new UnivariatePolynomial<E>(newRing, newData);
    }


    public UnivariatePolynomial<E> SetRingUnsafe(Ring<E> newRing)
    {
        return new UnivariatePolynomial<E>(newRing, data, degree);
    }

    public UnivariatePolynomial<E> SetModulusUnsafe(long mod)
    {
        if (ring is not IntegersZp64)
            throw new Exception();
        
        return this.AsZp64().SetRingUnsafe(Rings.Zp64(mod)).AsT<E>();
    }

    public UnivariatePolynomialZp64 Modulus(long mod)
    {
        if (ring is Integers64)
            return AsZ64().MapCoefficients(Rings.Zp64(mod), c => c % mod);
        
        if (ring is Integers)
            return AsZ().MapCoefficients(Rings.Zp64(mod), c => (long)(c % mod));

        throw new Exception();
    }
    
    public UnivariatePolynomialZp64 Modulus(long modulus, bool copy)
    {
        return Modulus(modulus);
    }

    public UnivariatePolynomialZp64 SetModulus(long newModulus)
    {
        var newData = (long[])data.Clone();
        var newDomain = new IntegersZp64(newModulus);
        newDomain.Modulus(newData);
        return new UnivariatePolynomialZp64(newDomain, newData);
    }

    public E Lc()
    {
        return data[degree];
    }

    public override UnivariatePolynomial<E> LcAsPoly()
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


    public E Cc()
    {
        return data[0];
    }


    public void EnsureInternalCapacity(int desiredCapacity)
    {
        if (data.Length < desiredCapacity)
        {
            var oldLength = data.Length;
            var newData = new E[desiredCapacity];
            Array.Copy(data, newData, oldLength);
            data = newData;
            FillZeroes(data, oldLength, data.Length);
        }
    }


    public void EnsureCapacity(int desiredDegree)
    {
        if (degree < desiredDegree)
            degree = desiredDegree;
        if (data.Length < (desiredDegree + 1))
        {
            var oldLen = data.Length;
            var newData = new E[desiredDegree + 1];
            Array.Copy(data, newData, oldLen);
            data = newData;
            FillZeroes(data, oldLen, data.Length);
        }
    }


    public void FixDegree()
    {
        var i = degree;
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

    public void AssertSameCoefficientRingWith(UnivariatePolynomial<E> oth)
    {
        if (!SameCoefficientRingWith(oth))
            throw new ArgumentException();
    }


    public bool SameCoefficientRingWith(UnivariatePolynomial<E> oth)
    {
        return ring.Equals(oth.ring);
    }

    public UnivariatePolynomial<E> SetCoefficientRingFrom(UnivariatePolynomial<E> poly)
    {
        return SetRing(poly.ring);
    }


    public UnivariatePolynomial<E> CreateFromArray(E[] data)
    {
        ring.SetToValueOf(data);
        return new UnivariatePolynomial<E>(ring, data);
    }


    public UnivariatePolynomial<E> CreateMonomial(int degree)
    {
        return CreateMonomial(ring.GetOne(), degree);
    }


    public UnivariatePolynomial<E> CreateLinear(E cc, E lc)
    {
        return CreateFromArray([cc, lc]);
    }


    public UnivariatePolynomial<E> CreateMonomial(E coefficient, int degree)
    {
        coefficient = ring.ValueOf(coefficient);
        E[] data = ring.CreateZeroesArray(degree + 1);
        data[degree] = coefficient;
        return new UnivariatePolynomial<E>(ring, data);
    }


    public UnivariatePolynomial<E> CreateConstant(E val)
    {
        return CreateFromArray([val]);
    }

    public UnivariatePolynomial<E> CreateZero()
    {
        return CreateConstant(ring.GetZero());
    }


    public override UnivariatePolynomial<E> CreateOne()
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


    public override bool IsOne()
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


    public override bool IsConstant()
    {
        return degree == 0;
    }


    public override bool IsMonomial()
    {
        for (var i = degree - 1; i >= 0; --i)
            if (!ring.IsZero(data[i]))
                return false;
        return true;
    }


    public override int SignumOfLC()
    {
        return ring.Signum(Lc());
    }


    public override bool IsOverField()
    {
        return ring.IsField();
    }


    public bool IsOverFiniteField()
    {
        return ring.IsFinite();
    }


    public bool IsOverZ()
    {
        return ring.Equals(Rings.Z) || ring.Equals(Rings.Z64);
    }


    public BigInteger? CoefficientRingCardinality()
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


    public BigInteger? CoefficientRingPerfectPowerBase()
    {
        return ring.PerfectPowerBase();
    }


    public BigInteger? CoefficientRingPerfectPowerExponent()
    {
        return ring.PerfectPowerExponent();
    }
    
    public HashSet<int> Exponents() {
        HashSet<int> degrees = new HashSet<int>();
        for (int i = Degree(); i >= 0; --i)
            if (!IsZeroAt(i))
                degrees.Add(i);
        return degrees;
    }


    public static BigInteger MignotteBound(UnivariatePolynomial<BigInteger> poly)
    {
        return (BigInteger.One << poly.degree) * (Norm2(poly));
    }
    
    public static long MignotteBound(UnivariatePolynomial<long> poly)
    {
        return (1 << poly.degree) * (Norm2(poly));
    }


    public static BigInteger Norm1(UnivariatePolynomial<BigInteger> poly)
    {
        var norm = BigInteger.Zero;
        for (var i = poly.degree; i >= 0; --i)
            norm = norm + (BigInteger.Abs(poly.data[i]));
        return norm;
    }


    public static BigInteger Norm2(UnivariatePolynomial<BigInteger> poly)
    {
        var norm = BigInteger.Zero;
        for (var i = poly.degree; i >= 0; --i)
            norm += poly.data[i] * (poly.data[i]);
        return BigIntegerUtils.SqrtCeil(norm);
    }
    
    public static long Norm2(UnivariatePolynomial<long> poly)
    {
        double norm = 0;
        for (var i = poly.degree; i >= 0; --i)
            norm += (double)poly.data[i] * (poly.data[i]);
        return (long)Math.Ceiling(Math.Sqrt(norm));
    }


    public static double Norm2Double(UnivariatePolynomial<BigInteger> poly)
    {
        double norm = 0;
        for (var i = poly.degree; i >= 0; --i)
        {
            var d = (double)poly.data[i];
            norm += d * d;
        }

        return Math.Sqrt(norm);
    }


    public E MaxAbsCoefficient()
    {
        var el = ring.Abs(data[0]);
        for (var i = 1; i <= degree; i++)
            el = ring.Max(el, ring.Abs(data[i]));
        return el;
    }


    public E NormMax()
    {
        return MaxAbsCoefficient();
    }


    private void FillZeroes(E[] data, int from, int to)
    {
        for (var i = from; i < to; ++i)
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
        Array.Copy(data, offset, data, 0, degree - offset + 1);
        FillZeroes(data, degree - offset + 1, degree + 1);
        degree = degree - offset;
        return this;
    }


    public UnivariatePolynomial<E> ShiftRight(int offset)
    {
        if (offset == 0)
            return this;
        var degree = this.degree;
        EnsureCapacity(offset + degree);
        Array.Copy(data, 0, data, offset, degree + 1);
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
        Array.Reverse(data, 0, degree + 1);
        FixDegree();
        return this;
    }


    public E Content()
    {
        if (degree == 0)
            return data[0];
        return IsOverField() ? Lc() : ring.Gcd(data); //        E gcd = data[degree];
        //        for (int i = degree - 1; i >= 0; --i)
        //            gcd = ring.gcd(gcd, data[i]);
        //        return gcd;
    }


    public override UnivariatePolynomial<E> ContentAsPoly()
    {
        return CreateConstant(Content());
    }


    public UnivariatePolynomial<E>? PrimitivePart()
    {
        var content = Content();
        if (SignumOfLC() < 0 && ring.Signum(content) > 0)
            content = ring.Negate(content);
        if (ring.IsMinusOne(content))
            return Negate();
        return PrimitivePart0(content);
    }


    public UnivariatePolynomial<E>? PrimitivePartSameSign()
    {
        return PrimitivePart0(Content());
    }

    private UnivariatePolynomial<E>? PrimitivePart0(E content)
    {
        if (IsZero())
            return this;
        if (ring.IsOne(content))
            return this;
        for (var i = degree; i >= 0; --i)
        {
            var div = ring.DivideOrNull(data[i], content);
            if (div.IsNull)
                return null;
            data[i] = div.Value;
        }

        return this;
    }


    public E Evaluate(long point)
    {
        return Evaluate(ring.ValueOfLong(point));
    }


    public E Evaluate(E point)
    {
        if (ring.IsZero(point))
            return Cc();
        point = ring.ValueOf(point);
        var res = ring.GetZero();
        for (var i = degree; i >= 0; --i)
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
        for (var i = degree; i >= 0; --i)
            result = result.Multiply(value).Add(data[i]);
        return result;
    }

    public MultivariatePolynomial<E> Composition(MultivariatePolynomial<E> value)
    {
        if (!value.ring.Equals(ring))
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


    public UnivariatePolynomial<E> Scale(E scaling)
    {
        if (ring.IsOne(scaling))
            return this.Clone();
        if (ring.IsZero(scaling))
            return CcAsPoly();
        var factor = ring.GetOne();
        E[] result = new E[degree + 1];
        for (var i = 0; i <= degree; ++i)
        {
            result[i] = ring.Multiply(data[i], factor);
            factor = ring.Multiply(factor, scaling);
        }

        return CreateUnsafe(ring, result);
    }


    public UnivariatePolynomial<E> Shift(E value)
    {
        return Composition(CreateLinear(value, ring.GetOne()));
    }
    
    public UnivariatePolynomial<E> Composition(Ring<UnivariatePolynomial<E>> ring, UnivariatePolynomial<E> value)
    {
        if (value.IsOne())
            return ring.ValueOf(this.Clone());
        if (value.IsZero())
            return CcAsPoly();
        UnivariatePolynomial<E> result = ring.GetZero();
        for (int i = Degree(); i >= 0; --i)
            result = ring.Add(ring.Multiply(result, value), GetAsPoly(i));
        return result;
    }


    public UnivariatePolynomial<E> Add(E val)
    {
        data[0] = ring.Add(data[0], ring.ValueOf(val));
        FixDegree();
        return this;
    }


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


    public override UnivariatePolynomial<E> Add(UnivariatePolynomial<E> oth)
    {
        AssertSameCoefficientRingWith(oth);
        if (oth.IsZero())
            return this;
        if (IsZero())
            return Set(oth);
        AssertSameCoefficientRingWith(oth);
        EnsureCapacity(oth.degree);
        for (var i = oth.degree; i >= 0; --i)
            data[i] = ring.Add(data[i], oth.data[i]);
        FixDegree();
        return this;
    }


    public UnivariatePolynomial<E> AddMonomial(E coefficient, int exponent)
    {
        if (ring.IsZero(coefficient))
            return this;
        EnsureCapacity(exponent);
        data[exponent] = ring.Add(data[exponent], ring.ValueOf(coefficient));
        FixDegree();
        return this;
    }


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
        for (var i = oth.degree; i >= 0; --i)
            data[i] = ring.Add(data[i], ring.Multiply(factor, oth.data[i]));
        FixDegree();
        return this;
    }


    public UnivariatePolynomial<E> Subtract(UnivariatePolynomial<E> oth)
    {
        AssertSameCoefficientRingWith(oth);
        if (oth.IsZero())
            return this;
        if (IsZero())
            return Set(oth).Negate();
        AssertSameCoefficientRingWith(oth);
        EnsureCapacity(oth.degree);
        for (var i = oth.degree; i >= 0; --i)
            data[i] = ring.Subtract(data[i], oth.data[i]);
        FixDegree();
        return this;
    }


    public UnivariatePolynomial<E> Subtract(UnivariatePolynomial<E> oth, E factor, int exponent)
    {
        AssertSameCoefficientRingWith(oth);
        if (oth.IsZero())
            return this;
        factor = ring.ValueOf(factor);
        if (ring.IsZero(factor))
            return this;
        AssertSameCoefficientRingWith(oth);
        for (var i = oth.degree + exponent; i >= exponent; --i)
            data[i] = ring.Subtract(data[i], ring.Multiply(factor, oth.data[i - exponent]));
        FixDegree();
        return this;
    }


    public override UnivariatePolynomial<E> Negate()
    {
        for (var i = degree; i >= 0; --i)
            if (!ring.IsZero(data[i]))
                data[i] = ring.Negate(data[i]);
        return this;
    }


    public UnivariatePolynomial<E> Multiply(E factor)
    {
        factor = ring.ValueOf(factor);
        if (ring.IsOne(factor))
            return this;
        if (ring.IsZero(factor))
            return ToZero();
        for (var i = degree; i >= 0; --i)
            data[i] = ring.Multiply(data[i], factor);
        return this;
    }


    public UnivariatePolynomial<E> MultiplyByLC(UnivariatePolynomial<E> other)
    {
        return Multiply(other.Lc());
    }


    public UnivariatePolynomial<E> Multiply(long factor)
    {
        return Multiply(ring.ValueOfLong(factor));
    }


    public override UnivariatePolynomial<E>? DivideByLC(UnivariatePolynomial<E> other)
    {
        return DivideOrNull(other.Lc());
    }


    public UnivariatePolynomial<E>? DivideOrNull(E factor)
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
        for (var i = degree; i >= 0; --i)
        {
            var l = ring.DivideOrNull(data[i], factor);
            if (l.IsNull)
                return null;
            data[i] = l.Value;
        }

        return this;
    }


    public UnivariatePolynomial<E> DivideExact(E factor)
    {
        var r = DivideOrNull(factor);
        if (r is null)
            throw new ArithmeticException("not divisible " + this + " / " + factor);
        return r;
    }
    
    public UnivariatePolynomial<E> GetRange(int from, int to)
    {
        return new UnivariatePolynomial<E>(ring, data[from..to]);
    }

    public override UnivariatePolynomial<E>? Monic()
    {
        if (IsZero())
            return this;
        return DivideOrNull(Lc());
    }


    public UnivariatePolynomial<E> Monic(E factor)
    {
        var lc = Lc();
        return Multiply(factor).DivideOrNull(lc);
    }


    public UnivariatePolynomial<E> MonicWithLC(UnivariatePolynomial<E> other)
    {
        if (Lc().Equals(other.Lc()))
            return this;
        return Monic(other.Lc());
    }


    public UnivariatePolynomial<E> MultiplyByBigInteger(BigInteger factor)
    {
        return Multiply(ring.ValueOfBigInteger(factor));
    }


    public override UnivariatePolynomial<E> Multiply(UnivariatePolynomial<E> oth)
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
            var factor = data[0];
            data = (E[])oth.data.Clone();
            degree = oth.degree;
            return Multiply(factor);
        }

        if (ring is IntegersZp)
        {
            // faster method with exact operations
            UnivariatePolynomial<E> iThis = this.SetRingUnsafe(Rings.Z as Ring<E>),
                iOth = oth.SetRingUnsafe(Rings.Z as Ring<E>);
            data = iThis.Multiply(iOth).data;
            ring.SetToValueOf(data);
        }
        else
            data = MultiplySafe0(oth);

        degree += oth.degree;
        FixDegree();
        return this;
    }


    // faster method with exact operations
    public override UnivariatePolynomial<E> Square()
    {
        if (IsZero())
            return this;
        if (degree == 0)
            return Multiply(data[0]);
        if (ring is IntegersZp)
        {
            // faster method with exact operations
            UnivariatePolynomial<E> iThis = SetRingUnsafe(Rings.Z as Ring<E>);
            data = iThis.Square().data;
            ring.SetToValueOf(data);
        }
        else
            data = SquareSafe0();

        degree += degree;
        FixDegree();
        return this;
    }

    public override UnivariatePolynomial<E> DivideExact(UnivariatePolynomial<E> other)
    {
        return UnivariateDivision.DivideExact(this, other);
    }

    public override UnivariatePolynomial<E> Gcd(UnivariatePolynomial<E> other)
    {
        return UnivariateGCD.PolynomialGCD(this, other);
    }


    public UnivariatePolynomial<E> Derivative()
    {
        if (IsConstant())
            return CreateZero();
        E[] newData = new E[degree];
        for (var i = degree; i > 0; --i)
            newData[i - 1] = ring.Multiply(data[i], ring.ValueOfLong(i));
        return CreateFromArray(newData);
    }


    public override UnivariatePolynomial<E> Clone()
    {
        return new UnivariatePolynomial<E>(ring, (E[])data.Clone(), degree);
    }

    public IEnumerator<E> Iterator()
    {
        foreach (var d in data)
        {
            yield return d;
        }
    }


    public IEnumerable<E> Stream()
    {
        return data;
    }


    public IEnumerable<UnivariatePolynomial<E>> StreamAsPolys()
    {
        return Stream().Select(CreateConstant);
    }


    public UnivariatePolynomial<T> MapCoefficients<T>(Ring<T> ring, Func<E, T> mapper)
    {
        return new UnivariatePolynomial<T>(ring, Stream().Select(mapper).ToArray());
    }
    
    public UnivariatePolynomial<T> MapCoefficientsAsPolys<T>(Ring<T> ring, Func<UnivariatePolynomial<E>, T> mapper)
    {
        return new UnivariatePolynomial<T>(ring, Stream().Select(c => mapper(Constant(this.ring, c))).ToArray());
    }

    public UnivariatePolynomialZp64 MapCoefficients(IntegersZp64 ring, Func<E, long> mapper)
    {
        return UnivariatePolynomialZp64.Create(ring, Stream().Select(mapper).ToArray());
    }


    public E[] GetDataReferenceUnsafe()
    {
        return data;
    }

    public MultivariatePolynomial<E> AsMultivariate()
    {
        return AsMultivariate(MonomialOrder.DEFAULT);
    }

    public MultivariatePolynomial<E> AsMultivariate(IComparer<DegreeVector> ordering)
    {
        return MultivariatePolynomial<E>.AsMultivariate(this, 1, 0, ordering);
    }


    public int CompareTo(UnivariatePolynomial<E>? o)
    {
        var c = degree.CompareTo(o.degree);
        if (c != 0)
            return c;
        for (var i = degree; i >= 0; --i)
        {
            c = ring.Compare(data[i], o.data[i]);
            if (c != 0)
                return c;
        }

        return 0;
    }

    public override string ToString()
    {
        if (IsConstant())
            return Cc().ToString();
        var sb = new StringBuilder();
        for (var i = 0; i <= degree; i++)
        {
            var el = data[i];
            if (ring.IsZero(el))
                continue;
            string cfString;
            if (!ring.IsOne(el) || i == 0)
                cfString = el.ToString();
            else
                cfString = "";
            if (i != 0 && (cfString.Contains("+") || cfString.Contains("-"))) // TODO Need parentheses
                cfString = "(" + cfString + ")";
            if (sb.Length != 0 && !cfString.StartsWith("-"))
                sb.Append("+");
            sb.Append(cfString);
            if (i == 0)
                continue;
            if (cfString.Length != 0)
                sb.Append("*");
            sb.Append("x"); // TODO param var name
            if (i > 1)
                sb.Append("^").Append(i);
        }

        return sb.ToString();
    }


    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (obj.GetType() != this.GetType())
            return false;
        var oth = (UnivariatePolynomial<E>)obj;
        if (degree != oth.degree)
            return false;
        for (var i = 0; i <= degree; ++i)
            if (!(Equals(data[i], oth.data[i])))
                return false;
        return true;
    }

    public static bool operator ==(UnivariatePolynomial<E>? a, UnivariatePolynomial<E>? b) => Equals(a, b);
    public static bool operator !=(UnivariatePolynomial<E>? a, UnivariatePolynomial<E>? b) => !Equals(a, b);

    public override int GetHashCode()
    {
        var result = 1;
        for (var i = degree; i >= 0; --i)
            result = 31 * result + data[i].GetHashCode();
        return result;
    }

    


    /* =========================== Exact multiplication with safe arithmetics =========================== */


    static readonly long KARATSUBA_THRESHOLD = 1024;


    static readonly long MUL_CLASSICAL_THRESHOLD = 256 * 256,
        MUL_KRONECKER_THRESHOLD = 32 * 32,
        MUL_MOD_CLASSICAL_THRESHOLD = 128 * 128;


    E[] MultiplySafe0(UnivariatePolynomial<E> oth)
    {
        long md = 1 * (degree + 1) * (oth.degree + 1);
        if (IsOverZ() && md >= MUL_KRONECKER_THRESHOLD)
            return MultiplyKronecker0(
                    this as UnivariatePolynomial<BigInteger>,
                    oth as UnivariatePolynomial<BigInteger>)
                as E[];
        if (md <= MUL_CLASSICAL_THRESHOLD)
            return MultiplyClassicalSafe(data, 0, degree + 1, oth.data, 0, oth.degree + 1);
        else
            return MultiplyKaratsubaSafe(data, 0, degree + 1, oth.data, 0, oth.degree + 1);
    }


    E[] SquareSafe0()
    {
        long md = 1 * (degree + 1) * (degree + 1);
        if (IsOverZ() && md >= MUL_KRONECKER_THRESHOLD)
            return SquareKronecker0(this as UnivariatePolynomial<BigInteger>) as E[];
        if (md <= MUL_CLASSICAL_THRESHOLD)
            return SquareClassicalSafe(data, 0, degree + 1);
        else
            return SquareKaratsubaSafe(data, 0, degree + 1);
    }


    E[] MultiplyClassicalSafe(E[] a, int aFrom, int aTo, E[] b, int bFrom, int bTo)
    {
        E[] result = ring.CreateZeroesArray(aTo - aFrom + bTo - bFrom - 1);
        MultiplyClassicalSafe(result, a, aFrom, aTo, b, bFrom, bTo);
        return result;
    }


    void MultiplyClassicalSafe(E[] result, E[] a, int aFrom, int aTo, E[] b, int bFrom, int bTo)
    {
        if (aTo - aFrom > bTo - bFrom)
        {
            MultiplyClassicalSafe(result, b, bFrom, bTo, a, aFrom, aTo);
            return;
        }

        for (var i = 0; i < aTo - aFrom; ++i)
        {
            var c = a[aFrom + i];
            if (!ring.IsZero(c))
                for (var j = 0; j < bTo - bFrom; ++j)
                    result[i + j] = ring.AddMutable(result[i + j], ring.Multiply(c, b[bFrom + j]));
        }
    }


    E[] MultiplyKaratsubaSafe(E[] f, int fFrom, int fTo, E[] g, int gFrom, int gTo)
    {
        // return zero
        if (fFrom >= fTo || gFrom >= gTo)
            return [];

        // single element in f
        if (fTo - fFrom == 1)
        {
            E[] result1 = new E[gTo - gFrom];
            for (var i = gFrom; i < gTo; ++i)
                result1[i - gFrom] = ring.Multiply(f[fFrom], g[i]);
            return result1;
        }


        // single element in g
        if (gTo - gFrom == 1)
        {
            E[] result2 = new E[fTo - fFrom];

            //single element in b
            for (var i = fFrom; i < fTo; ++i)
                result2[i - fFrom] = ring.Multiply(g[gFrom], f[i]);
            return result2;
        }


        // linear factors
        if (fTo - fFrom == 2 && gTo - gFrom == 2)
        {
            E[] result3 = new E[3];

            //both a and b are linear
            result3[0] = ring.Multiply(f[fFrom], g[gFrom]);
            result3[1] = ring.AddMutable(ring.Multiply(f[fFrom], g[gFrom + 1]),
                ring.Multiply(f[fFrom + 1], g[gFrom]));
            result3[2] = ring.Multiply(f[fFrom + 1], g[gFrom + 1]);
            return result3;
        }


        //switch to classical
        if (1 * (fTo - fFrom) * (gTo - gFrom) < KARATSUBA_THRESHOLD)
            return MultiplyClassicalSafe(g, gFrom, gTo, f, fFrom, fTo);
        if (fTo - fFrom < gTo - gFrom)
            return MultiplyKaratsubaSafe(g, gFrom, gTo, f, fFrom, fTo);

        //we now split a and b into 2 parts:
        var split = (fTo - fFrom + 1) / 2;

        //if we can't split b
        if (gFrom + split >= gTo)
        {
            E[] f0g = MultiplyKaratsubaSafe(f, fFrom, fFrom + split, g, gFrom, gTo);
            E[] f1g = MultiplyKaratsubaSafe(f, fFrom + split, fTo, g, gFrom, gTo);
            int oldLen4 = f0g.Length, newLen = fTo - fFrom + gTo - gFrom - 1;
            E[] result4 = new E[newLen];
            Array.Copy(f0g, result4, newLen);
            FillZeroes(result4, oldLen4, newLen);
            for (var i = 0; i < f1g.Length; i++)
                result4[i + split] = ring.AddMutable(result4[i + split], f1g[i]);
            return result4;
        }

        int fMid = fFrom + split, gMid = gFrom + split;
        E[] f0g0 = MultiplyKaratsubaSafe(f, fFrom, fMid, g, gFrom, gMid);
        E[] f1g1 = MultiplyKaratsubaSafe(f, fMid, fTo, g, gMid, gTo);

        // f0 + f1
        E[] f0_plus_f1 = new E[Math.Max(fMid - fFrom, fTo - fMid)];
        Array.Copy(f, fFrom, f0_plus_f1, 0, fMid - fFrom);
        FillZeroes(f0_plus_f1, fMid - fFrom, f0_plus_f1.Length);
        for (var i = fMid; i < fTo; ++i)
            f0_plus_f1[i - fMid] = ring.Add(f0_plus_f1[i - fMid], f[i]);

        //g0 + g1
        E[] g0_plus_g1 = new E[Math.Max(gMid - gFrom, gTo - gMid)];
        Array.Copy(g, gFrom, g0_plus_g1, 0, gMid - gFrom);
        FillZeroes(g0_plus_g1, gMid - gFrom, g0_plus_g1.Length);
        for (var i = gMid; i < gTo; ++i)
            g0_plus_g1[i - gMid] = ring.Add(g0_plus_g1[i - gMid], g[i]);
        E[] mid = MultiplyKaratsubaSafe(f0_plus_f1, 0, f0_plus_f1.Length, g0_plus_g1, 0, g0_plus_g1.Length);
        if (mid.Length < f0g0.Length)
        {
            var oldLen5 = mid.Length;
            var newMid = new E[f0g0.Length];
            Array.Copy(mid, newMid, oldLen5);
            FillZeroes(mid, oldLen5, mid.Length);
        }

        if (mid.Length < f1g1.Length)
        {
            var oldLen6 = mid.Length;
            var newMid = new E[f1g1.Length];
            Array.Copy(mid, newMid, oldLen6);
            FillZeroes(mid, oldLen6, mid.Length);
        }


        //subtract f0g0, f1g1
        for (var i = 0; i < f0g0.Length; ++i)
            mid[i] = ring.SubtractMutable(mid[i], f0g0[i]);
        for (var i = 0; i < f1g1.Length; ++i)
            mid[i] = ring.SubtractMutable(mid[i], f1g1[i]);
        var oldLen = f0g0.Length;
        var result = new E[(fTo - fFrom) + (gTo - gFrom) - 1];
        Array.Copy(f0g0, result, result.Length);
        FillZeroes(result, oldLen, result.Length);
        for (var i = 0; i < mid.Length; ++i)
            result[i + split] = ring.AddMutable(result[i + split], mid[i]);
        for (var i = 0; i < f1g1.Length; ++i)
            result[i + 2 * split] = ring.AddMutable(result[i + 2 * split], f1g1[i]);
        return result;
    }


    E[] SquareClassicalSafe(E[] a, int from, int to)
    {
        E[] x = ring.CreateZeroesArray((to - from) * 2 - 1);
        SquareClassicalSafe(x, a, from, to);
        return x;
    }


    void SquareClassicalSafe(E[] result, E[] data, int from, int to)
    {
        var len = to - from;
        for (var i = 0; i < len; ++i)
        {
            var c = data[from + i];
            if (!ring.IsZero(c))
                for (var j = 0; j < len; ++j)
                    result[i + j] = ring.AddMutable(result[i + j], ring.Multiply(c, data[from + j]));
        }
    }


    E[] SquareKaratsubaSafe(E[] f, int fFrom, int fTo)
    {
        if (fFrom >= fTo)
            return [];
        if (fTo - fFrom == 1)
        {
            E[] r = new E[1];
            r[0] = ring.Multiply(f[fFrom], f[fFrom]);
            return r;
        }

        if (fTo - fFrom == 2)
        {
            E[] result3 = new E[3];
            result3[0] = ring.Multiply(f[fFrom], f[fFrom]);
            result3[1] = ring.MultiplyMutable(ring.Multiply(f[fFrom], f[fFrom + 1]), ring.ValueOfLong(2));
            result3[2] = ring.Multiply(f[fFrom + 1], f[fFrom + 1]);
            return result3;
        }


        //switch to classical
        if (1 * (fTo - fFrom) * (fTo - fFrom) < KARATSUBA_THRESHOLD)
            return SquareClassicalSafe(f, fFrom, fTo);

        //we now split a and b into 2 parts:
        var split = (fTo - fFrom + 1) / 2;
        var fMid = fFrom + split;
        E[] f0g0 = SquareKaratsubaSafe(f, fFrom, fMid);
        E[] f1g1 = SquareKaratsubaSafe(f, fMid, fTo);

        // f0 + f1
        E[] f0_plus_f1 = new E[Math.Max(fMid - fFrom, fTo - fMid)];
        Array.Copy(f, fFrom, f0_plus_f1, 0, fMid - fFrom);
        FillZeroes(f0_plus_f1, fMid - fFrom, f0_plus_f1.Length);
        for (var i = fMid; i < fTo; ++i)
            f0_plus_f1[i - fMid] = ring.Add(f0_plus_f1[i - fMid], f[i]);
        E[] mid = SquareKaratsubaSafe(f0_plus_f1, 0, f0_plus_f1.Length);
        if (mid.Length < f0g0.Length)
        {
            var oldLen2 = mid.Length;
            var newMid = new E[f0g0.Length];
            Array.Copy(mid, newMid, oldLen2);
            FillZeroes(mid, oldLen2, mid.Length);
        }

        if (mid.Length < f1g1.Length)
        {
            var oldLen3 = mid.Length;
            var newMid = new E[f1g1.Length];
            Array.Copy(mid, newMid, oldLen3);
            FillZeroes(mid, oldLen3, mid.Length);
        }


        //subtract f0g0, f1g1
        for (var i = 0; i < f0g0.Length; ++i)
            mid[i] = ring.SubtractMutable(mid[i], f0g0[i]);
        for (var i = 0; i < f1g1.Length; ++i)
            mid[i] = ring.SubtractMutable(mid[i], f1g1[i]);
        var oldLen = f0g0.Length;
        var result = new E[2 * (fTo - fFrom) - 1];
        Array.Copy(f0g0, result, result.Length);
        FillZeroes(result, oldLen, result.Length);
        for (var i = 0; i < mid.Length; ++i)
            result[i + split] = ring.AddMutable(result[i + split], mid[i]);
        for (var i = 0; i < f1g1.Length; ++i)
            result[i + 2 * split] = ring.AddMutable(result[i + 2 * split], f1g1[i]);
        return result;
    }


    /* ====================== Schönhage–Strassen algorithm algorithm via Kronecker substitution ====================== */


    static UnivariatePolynomial<BigInteger> SquareKronecker(UnivariatePolynomial<BigInteger> poly)
    {
        return UnivariatePolynomial<BigInteger>.Create(Rings.Z, SquareKronecker0(poly));
    }


    private static BigInteger[] SquareKronecker0(UnivariatePolynomial<BigInteger> poly)
    {
        var len = poly.degree + 1;

        // determine #bits needed per coefficient
        var logMinDigits = 32 - int.LeadingZeroCount(len - 1);
        var maxLength = 0;
        foreach (var cf in poly.data)
            maxLength = Math.Max(maxLength, (int)cf.GetBitLength());
        var k = logMinDigits + 2 * maxLength + 1; // in bits
        k = (k + 31) / 32; // in ints

        // encode each polynomial into an int[]
        var pInt = ToIntArray(poly, k);
        var cInt = ToIntArray(BigInteger.Pow(ToBigInteger(pInt), 2));

        // decode poly coefficients from the product
        var cPoly = new BigInteger[2 * len - 1];
        DecodePoly(k, cInt, cPoly);
        return cPoly;
    }


    private static void DecodePoly(int k, int[] cInt, BigInteger[] cPoly)
    {
        var _2k = BigInteger.One << (k * 32);
        Array.Fill(cPoly, BigInteger.Zero);
        for (var i = 0; i < cPoly.Length; i++)
        {
            var cfInt = new int[k];
            Array.Copy(cInt, i * k, cfInt, 0, k);
            var cf = ToBigInteger(cfInt);
            if (cfInt[k - 1] < 0)
            {
                // if coeff > 2^(k-1)
                cf -= _2k;

                // add 2^k to cInt which is the same as subtracting coeff
                bool carry;
                var cIdx = (i + 1) * k;
                do
                {
                    cInt[cIdx]++;
                    carry = cInt[cIdx] == 0;
                    cIdx++;
                } while (carry);
            }

            cPoly[i] += cf;
        }
    }


    static UnivariatePolynomial<BigInteger> MultiplyKronecker(UnivariatePolynomial<BigInteger> poly1,
        UnivariatePolynomial<BigInteger> poly2)
    {
        return UnivariatePolynomial<BigInteger>.Create(Rings.Z, MultiplyKronecker0(poly1, poly2));
    }


    static BigInteger[] MultiplyKronecker0(UnivariatePolynomial<BigInteger> poly1,
        UnivariatePolynomial<BigInteger> poly2)
    {
        if (poly2.degree > poly1.degree)
            return MultiplyKronecker0(poly2, poly1);
        var len1 = poly1.degree + 1;
        var len2 = poly2.degree + 1;

        // determine #bits needed per coefficient
        var logMinDigits = 32 - int.LeadingZeroCount(len1 - 1);
        var maxLengthA = 0;
        foreach (var cf in poly1.data)
            maxLengthA = Math.Max(maxLengthA, (int)cf.GetBitLength());
        var maxLengthB = 0;
        foreach (var cf in poly2.data)
            maxLengthB = Math.Max(maxLengthB, (int)cf.GetBitLength());
        var k = logMinDigits + maxLengthA + maxLengthB + 1; // in bits
        k = (k + 31) / 32; // in ints

        // encode each polynomial into an int[]
        var aInt = ToIntArray(poly1, k);
        var bInt = ToIntArray(poly2, k);

        // multiply
        var cInt = ToIntArray(ToBigInteger(aInt) * ToBigInteger(bInt));

        // decode poly coefficients from the product
        var cPoly = new BigInteger[len1 + len2 - 1];
        DecodePoly(k, cInt, cPoly);
        var aSign = poly1.Lc().Sign;
        var bSign = poly2.Lc().Sign;
        if (aSign * bSign < 0)
            for (var i = 0; i < cPoly.Length; i++)
                cPoly[i] = -cPoly[i];
        return cPoly;
    }


    private static BigInteger ToBigInteger(int[] a)
    {
        var b = new byte[a.Length * 4];
        for (var i = 0; i < a.Length; i++)
        {
            var iRev = a.Length - 1 - i;
            b[i * 4] = (byte)(a[iRev] >>> 24);
            b[i * 4 + 1] = (byte)((a[iRev] >>> 16) & 0xFF);
            b[i * 4 + 2] = (byte)((a[iRev] >>> 8) & 0xFF);
            b[i * 4 + 3] = (byte)(a[iRev] & 0xFF);
        }

        return new BigInteger(b, isUnsigned: true);
    }


    private static int[] ToIntArray(BigInteger a)
    {
        var aArr = a.ToByteArray();
        var b = new int[(aArr.Length + 3) / 4];
        for (var i = 0; i < aArr.Length; i++)
            b[i / 4] += (aArr[aArr.Length - 1 - i] & 0xFF) << ((i % 4) * 8);
        return b;
    }


    private static int[] ToIntArray(UnivariatePolynomial<BigInteger> a, int k)
    {
        var len = a.degree + 1;
        var sign = a.Lc().Sign;
        var aInt = new int[len * k];
        for (var i = len - 1; i >= 0; i--)
        {
            var cArr = ToIntArray(BigInteger.Abs(a.data[i]));
            if (a.data[i].Sign * sign < 0)
                SubShifted(aInt, cArr, i * k);
            else
                AddShifted(aInt, cArr, i * k);
        }

        return aInt;
    }


    private static void AddShifted(int[] a, int[] b, int numElements)
    {
        var carry = false;
        var i = 0;
        while (i < Math.Min(b.Length, a.Length - numElements))
        {
            var ai = a[i + numElements];
            var sum = ai + b[i];
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


    private static void SubShifted(int[] a, int[] b, int numElements)
    {
        var carry = false;
        var i = 0;
        while (i < Math.Min(b.Length, a.Length - numElements))
        {
            var ai = a[i + numElements];
            var diff = ai - b[i];
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

    
    public UnivariatePolynomial<BigInteger> AsZ()
    {
        if (ring is not Integers)
            throw new Exception();
        return this as UnivariatePolynomial<BigInteger>;
    }
    
    public UnivariatePolynomial<BigInteger> AsZp()
    {
        if (ring is not IntegersZp)
            throw new Exception();
        return this as UnivariatePolynomial<BigInteger>;
    }

    
    public UnivariatePolynomialZ64 AsZ64()
    {
        if (ring is not Integers64)
            throw new Exception();
        return this as UnivariatePolynomial<long>;
    }
    
    public UnivariatePolynomialZp64 AsZp64()
    {
        if (ring is not IntegersZp64)
            throw new Exception();
        return this as UnivariatePolynomial<long>;
    }
    
    public UnivariatePolynomial<BigInteger> AsBigInteger()
    {
        if (typeof(BigInteger) != typeof(E))
            throw new Exception();
        return this as UnivariatePolynomial<BigInteger>;
    }
    
    public UnivariatePolynomial<T> AsT<T>()
    {
        if (typeof(T) != typeof(E))
            throw new Exception();
        return this as UnivariatePolynomial<T>;
    }

    public override PolynomialRing<UnivariatePolynomial<E>> AsRing()
    {
        return new UnivariateRing<E>(this);
    }

    public UnivariatePolynomial<E> Pow(int n)
    {
        return PolynomialMethods.PolyPow(this, n);
    }

    public static UnivariatePolynomial<E> operator +(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) =>
        a.Clone().Add(b);
    public static UnivariatePolynomial<E> operator -(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) =>
        a.Clone().Subtract(b);
    public static UnivariatePolynomial<E> operator *(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) =>
        a.Clone().Multiply(b);
    public static UnivariatePolynomial<E> operator /(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) =>
        UnivariateDivision.DivideExact(a, b);
    public static UnivariatePolynomial<E> operator %(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) =>
        UnivariateDivision.Remainder(a, b)!;
    public static UnivariatePolynomial<E> operator -(UnivariatePolynomial<E> a) =>
        a.Clone().Negate();
    
    
    public static UnivariatePolynomial<E> operator +(E a, UnivariatePolynomial<E> b) =>
        b.Clone().Add(a);
    public static UnivariatePolynomial<E> operator -(E a, UnivariatePolynomial<E> b) =>
        b.Clone().Subtract(a);
    public static UnivariatePolynomial<E> operator *(E a, UnivariatePolynomial<E> b) =>
        b.Clone().Multiply(a);
    
    public static UnivariatePolynomial<E> operator +(UnivariatePolynomial<E> a, E b) =>
        a.Clone().Add(b);
    public static UnivariatePolynomial<E> operator -(UnivariatePolynomial<E> a, E b) =>
        a.Clone().Subtract(b);
    public static UnivariatePolynomial<E> operator *(UnivariatePolynomial<E> a, E b) =>
        a.Clone().Multiply(b);
    public static UnivariatePolynomial<E> operator /(UnivariatePolynomial<E> a, E b) =>
        a.Clone().DivideExact(b);

    public static UnivariatePolynomial<E> operator +(long a, UnivariatePolynomial<E> b) =>
        b + b.ring.ValueOfLong(a);

    public static UnivariatePolynomial<E> operator -(long a, UnivariatePolynomial<E> b) =>
        b - b.ring.ValueOfLong(a);

    public static UnivariatePolynomial<E> operator *(long a, UnivariatePolynomial<E> b) =>
        b * b.ring.ValueOfLong(a);


    public static UnivariatePolynomial<E> operator +(UnivariatePolynomial<E> a, long b) =>
        a + a.ring.ValueOfLong(b);
    public static UnivariatePolynomial<E> operator -(UnivariatePolynomial<E> a, long b) =>
        a - a.ring.ValueOfLong(b);
 
    public static UnivariatePolynomial<E> operator *(UnivariatePolynomial<E> a, long b) =>
        a * a.ring.ValueOfLong(b);
    public static UnivariatePolynomial<E> operator /(UnivariatePolynomial<E> a, long b) =>
        a / a.ring.ValueOfLong(b);
}