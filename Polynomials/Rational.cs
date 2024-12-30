using System.Numerics;
using Polynomials.Poly.Multivar;
using Polynomials.Poly.Univar;
using Polynomials.Utils;


namespace Polynomials;

public class Rational<E>
{
    public readonly Ring<E> ring;


    public readonly Operand numerator;


    public readonly Operand denominator;


    private readonly Predicate<E> simplicityCriteria;


    public Rational(Ring<E> ring, E numerator)
    {
        this.ring = ring;
        this.simplicityCriteria = SimplicityCriteria(ring);
        this.numerator = new Operand(numerator);
        this.denominator = new Operand(ring.GetOne());
    }


    public Rational(Ring<E> ring, E numerator, E denominator)
    {
        if (ring.IsZero(denominator))
            throw new ArithmeticException("division by zero");
        if (!ring.IsZero(numerator))
        {
            E gcd = ring.Gcd(numerator, denominator);
            if (!ring.IsUnit(gcd))
            {
                numerator = ring.DivideExact(numerator, gcd);
                denominator = ring.DivideExact(denominator, gcd);
            }
        }

        this.ring = ring;
        this.simplicityCriteria = SimplicityCriteria(ring);
        Operand[] numden = new Rational<E>.Operand[]
        {
            new Operand(numerator),
            new Operand(denominator)
        };
        Normalize(numden);
        this.numerator = numden[0];
        this.denominator = numden[1];
    }


    public Rational(Ring<E> ring, Operand numerator, Operand denominator)
    {
        if (denominator.IsZero(ring))
            throw new ArithmeticException("division by zero");
        this.ring = ring;
        this.simplicityCriteria = SimplicityCriteria(ring);
        Operand[] numden = new Rational<E>.Operand[]
        {
            numerator,
            denominator
        };
        Normalize(numden);
        this.numerator = numden[0];
        this.denominator = numden[1];
    }


    public Rational(bool skipNormalize, Ring<E> ring, Operand numerator, Operand denominator)
    {
        if (denominator.IsZero(ring))
            throw new ArithmeticException("division by zero");
        this.ring = ring;
        this.simplicityCriteria = SimplicityCriteria(ring);
        this.numerator = numerator;
        this.denominator = denominator;
    }

    private const int
        SIMPLE_UPOLY_SIZE = 8,
        SIMPLE_MPOLY_DENSE_SIZE = 3,
        SIMPLE_MPOLY_SPARSE_SIZE = 16; // sparse multivariate polynomials


    private const double SIMPLE_POLY_SPARSITY2 = 0.001;


    private const int SIMPLE_INTEGER_N_BITS = 512;


    // criteria singletons
    private static readonly Predicate<BigInteger> intSimplicityCriteria = p => p.GetBitLength() <= SIMPLE_INTEGER_N_BITS;


    private static readonly Predicate<IUnivariatePolynomial> upolySimplicityCriteria = p => p.Degree() - 1 <= SIMPLE_UPOLY_SIZE;


    // private static readonly Predicate<IMultivariatePolynomial> mpolySimplicityCriteria =
    //     (Predicate<AMultivariatePolynomial> & java.io.Serializable)((p) =>
    //         p.Count <= SIMPLE_MPOLY_DENSE_SIZE ||
    //         (p.Count < SIMPLE_MPOLY_SPARSE_SIZE && p.Sparsity2() < SIMPLE_POLY_SPARSITY2));


    private static readonly Predicate<E> defaultFalse = _ => false;


    private static Predicate<E> SimplicityCriteria(Ring<E> ring)
    {
        if (ring is IUnivariateRing)
            return upolySimplicityCriteria as Predicate<E>;
        // else if (ring is MultivariateRing)
        //     return (Predicate<E>)mpolySimplicityCriteria;
        else if (ring is Ring<BigInteger>)
            return intSimplicityCriteria as Predicate<E>;
        else
            return defaultFalse;
    }


    public static Rational<E> Zero(Ring<E> ring)
    {
        return new Rational<E>(ring, ring.GetZero());
    }


    public static Rational<E> One(Ring<E> ring)
    {
        return new Rational<E>(ring, ring.GetOne());
    }


    public sealed class Operand : List<E>
    {
        private Utils.Nullable<E> expandForm = Utils.Nullable<E>.Null;

        private List<E> toExpand;


        public Operand()
        {
            toExpand = this;
        }


        public Operand(IEnumerable<E> c) : base(c)
        {
            toExpand = this;
            if (Count == 1)
                SetExpandForm(this[0]);
        }


        public Operand(E el)
        {
            toExpand = this;
            Add(el);
            SetExpandForm(el);
        }


        private void SetExpandForm(E e)
        {
            expandForm = e;
            toExpand = [e];
        }


        public void Normalize(Ring<E> ring, Predicate<E> simplicityCriteria)
        {
            if (Count == 0)
            {
                Set(ring.GetOne());
                return;
            }

            if (Count == 1)
            {
                if (ring.Signum(First()) < 0)
                {
                    this[0] = ring.Negate(First());
                    Insert(0, ring.GetNegativeOne());
                }

                return;
            }

            E unit = ring.GetOne(), simple = ring.GetOne();
            for (int i = Count - 1; i >= 0; --i)
                if (ring.IsUnitOrZero(this[i]))
                    unit = ring.MultiplyMutable(unit, this.Pop(i));
                else
                {
                    if (ring.Signum(this[i]) < 0)
                    {
                        this[i] = ring.Negate(this[i]);
                        unit = ring.Negate(unit);
                    }

                    if (simplicityCriteria(this[i]))
                    {
                        if (!simplicityCriteria(simple))
                        {
                            Add(simple);
                            simple = this.Pop(i);
                        }
                        else
                            simple = ring.MultiplyMutable(simple, this.Pop(i));
                    }
                }

            if (!ring.IsOne(simple))
                Add(simple);
            if (ring.IsZero(unit))
                Clear();
            if (!ring.IsOne(unit) || Count == 0)
                Insert(0, unit);
        }


        public E Expand(Ring<E> ring)
        {
            if (!expandForm.IsNull)
                return expandForm.Value;
            if (Count == 1)
                expandForm = this[0];
            else
                expandForm = ring.Multiply(toExpand);
            toExpand = [expandForm.Value];
            return expandForm.Value;
        }


        public bool IsZero(Ring<E> ring)
        {
            return Count == 1 && ring.IsZero(this[0]);
        }


        public bool IsUnit(Ring<E> ring)
        {
            return Count == 1 && ring.IsUnit(this[0]);
        }


        public bool IsOne(Ring<E> ring)
        {
            return IsUnit(ring) && ring.IsOne(this[0]);
        }


        private E First()
        {
            return this[0];
        }


        private E Last()
        {
            return this[^1];
        }


        public Utils.Nullable<E> UnitOrNull(Ring<E> ring)
        {
            E u = First();
            if (ring.IsUnit(u))
                return u;
            else
                return Utils.Nullable<E>.Null;
        }


        private E UnitOrOne(Ring<E> ring)
        {
            var u = UnitOrNull(ring);
            return u.IsNull ? ring.GetOne() : u.Value;
        }


        public Operand Multiply(Operand oth, Ring<E> ring, Predicate<E> simplicityCriteria)
        {
            if (IsOne(ring))
                return oth;
            if (oth.IsOne(ring))
                return this;
            if (IsZero(ring))
                return this;
            if (oth.IsZero(ring))
                return oth;
            Operand r = new Operand();
            r.AddRange(this);
            r.AddRange(oth);
            if (!expandForm.IsNull && !oth.expandForm.IsNull)
                r.SetExpandForm(ring.Multiply(expandForm.Value, oth.expandForm.Value));
            else if (toExpand != this || oth.toExpand != oth)
            {
                r.toExpand = new List<E>();
                r.toExpand.AddRange(toExpand);
                r.toExpand.AddRange(oth.toExpand);
            }


            // fix
            r.Normalize(ring, simplicityCriteria);
            return r;
        }


        public Operand Multiply(E oth, Ring<E> ring, Predicate<E> simplicityCriteria)
        {
            if (IsOne(ring))
                return new Operand(oth);
            if (ring.IsOne(oth))
                return this;
            if (IsZero(ring))
                return this;
            if (ring.IsZero(oth))
                return new Operand(oth);
            Operand r = new Operand(this);
            r.Add(oth);
            if (!expandForm.IsNull)
                r.SetExpandForm(ring.Multiply(expandForm.Value, oth));
            if (toExpand != this)
            {
                r.toExpand = new List<E>(toExpand);
                r.toExpand.Add(oth);
            }

            if (ring.IsUnit(oth) || ring.Signum(oth) < 0 || simplicityCriteria(oth))

                // fix
                r.Normalize(ring, simplicityCriteria);
            return r;
        }


        public void Divide(int i, E divider, Ring<E> ring)
        {
            E div = ring.DivideExact(this[i], divider);
            this[i] = div;
            if (!expandForm.IsNull)
            {
                if (Count == 1 && toExpand == this)
                    expandForm = this[0]; // already divided
                else
                    expandForm = ring.DivideExact(expandForm.Value, divider); // divide
                toExpand = [expandForm.Value];
            }
            else
            {
                expandForm = Utils.Nullable<E>.Null;
                toExpand = this;
            } // don't normalize here!
        }


        public new void Clear()
        {
            base.Clear();
            toExpand = this;
        }


        private void Set(E value)
        {
            Clear();
            Add(value);
            SetExpandForm(value);
        }


        public Operand ShallowCopy()
        {
            Operand op = new Operand(this);
            if (!expandForm.IsNull)
                op.SetExpandForm(expandForm.Value);
            else if (toExpand != this)
                op.toExpand = new List<E>(toExpand);
            return op;
        }


        public Operand DeepCopy(Ring<E> ring)
        {
            return Map(ring.Copy);
        }

        public Operand Map(Func<E, E> mapper)
        {
            Operand op = new Operand(this.Select(mapper));
            if (toExpand != this)
                op.toExpand = toExpand.Select(mapper).ToList();
            if (!expandForm.IsNull)
                op.SetExpandForm(mapper(expandForm.Value));
            return op;
        }


        public Operand Pow(long exponent, Ring<E> ring)
        {
            return Pow(new BigInteger(exponent), ring);
        }


        public Operand Pow(BigInteger exponent, Ring<E> ring)
        {
            if (exponent.IsOne)
                return DeepCopy(ring);
            Operand pow = new Operand(this.Select((f) => ring.Pow(f, exponent)));
            if (!expandForm.IsNull)
            {
                if (Count == 1)
                    pow.expandForm = pow.First();
                else
                    pow.expandForm = ring.Pow(expandForm.Value, exponent);
                pow.toExpand = [pow.expandForm.Value];
            }
            else if (toExpand != this)
                pow.toExpand = toExpand.Select((f) => ring.Pow(f, exponent)).ToList();

            return pow;
        }
    }

    private void Normalize(Operand[] numden)
    {
        Operand numerator = numden[0].ShallowCopy();
        Operand denominator;
        if (numerator.IsZero(ring))
            denominator = new Operand(ring.GetOne());
        else
            denominator = numden[1].ShallowCopy();
        numerator.Normalize(ring, simplicityCriteria);
        denominator.Normalize(ring, simplicityCriteria);
        if (!numerator.IsZero(ring))
            if (denominator.IsUnit(ring))
            {
                numerator = numerator.Multiply(ring.Reciprocal(denominator.Expand(ring)), ring, simplicityCriteria);
                denominator = new Operand(ring.GetOne());
            }
            else if (!(denominator.UnitOrNull(ring)).IsNull)
            {
                E du = ring.Reciprocal(denominator.UnitOrNull(ring).Value);
                denominator = denominator.Multiply(du, ring, simplicityCriteria);
                numerator = numerator.Multiply(du, ring, simplicityCriteria);
            }

        numden[0] = numerator;
        numden[1] = denominator;
    }


    private Operand[] ReduceGcd(Operand a, Operand b)
    {
        if (a.IsUnit(ring) || b.IsUnit(ring))
            return new Rational<E>.Operand[]
            {
                a,
                b,
                new Operand(ring.GetOne())
            };
        Operand aReduced = a.ShallowCopy(), bReduced = b.ShallowCopy(), gcd = new Operand();
        for (int ia = 0; ia < aReduced.Count; ++ia)
        {
            for (int ib = 0; ib < bReduced.Count; ++ib)
            {
                E af = aReduced[ia], bf = bReduced[ib];
                if (ring.IsUnit(af) || ring.IsUnit(bf))
                    continue;
                E gf = ring.Gcd(af, bf);
                if (ring.IsUnit(gf))
                    continue;
                gcd.Add(gf);
                aReduced.Divide(ia, gf, ring);
                bReduced.Divide(ib, gf, ring);
            }
        }

        if (gcd.Count == 0)
            return new Rational<E>.Operand[]
            {
                a,
                b,
                new Operand(ring.GetOne())
            };
        aReduced.Normalize(ring, simplicityCriteria);
        bReduced.Normalize(ring, simplicityCriteria);
        gcd.Normalize(ring, simplicityCriteria);
        return new Rational<E>.Operand[]
        {
            aReduced,
            bReduced,
            gcd
        };
    }


    public virtual bool IsZero()
    {
        return numerator.IsZero(ring);
    }


    public virtual bool IsOne()
    {
        return denominator.IsOne(ring) && numerator.IsOne(ring);
    }


    public virtual bool IsIntegral()
    {
        return denominator.IsOne(ring);
    }


    public virtual E Numerator()
    {
        return numerator.Expand(ring);
    }


    public virtual E NumeratorExact()
    {
        if (!IsIntegral())
            throw new ArgumentException("not integral fraction");
        return Numerator();
    }


    public virtual E Denominator()
    {
        return denominator.Expand(ring);
    }


    public virtual FactorDecomposition<E> FactorDenominator()
    {
        return denominator.Select(ring.Factor)
            .Aggregate(FactorDecomposition<E>.Empty(ring), (factors, newFac) => factors.AddAll(newFac));
    }


    public virtual FactorDecomposition<E> FactorNumerator()
    {
        return numerator.Select(ring.Factor)
            .Aggregate(FactorDecomposition<E>.Empty(ring), (factors, newFac) => factors.AddAll(newFac));
    }


    public virtual Rational<E>[] Normal()
    {
        if (IsIntegral())
            return new Rational<E>[]
            {
                this
            };
        E[] qd = ring.DivideAndRemainder(Numerator(), Denominator());
        if (qd[0] == null)
            throw new Exception("division with remainder is not supported.");
        if (ring.IsZero(qd[0]) || ring.IsZero(qd[1]))
            return new Rational<E>[]
            {
                this
            };
        return new Rational<E>[]
        {
            new Rational<E>(ring, qd[0]),
            new Rational<E>(ring, qd[1], Denominator())
        };
    }


    public virtual int Signum()
    {
        return ring.Signum(Numerator()) * ring.Signum(Denominator());
    }


    public virtual Rational<E> Abs()
    {
        return Signum() >= 0 ? this : Negate();
    }


    public virtual Rational<E> Reciprocal()
    {
        return new Rational<E>(ring, denominator, numerator);
    }


    public virtual Rational<E> Multiply(Rational<E> oth)
    {
        if (IsOne())
            return oth;
        if (IsZero())
            return this;
        if (oth.IsOne())
            return this;
        if (oth.IsZero())
            return oth;
        Operand[] numden;
        numden = ReduceGcd(numerator, oth.denominator);
        Operand thisNum = numden[0], thatDen = numden[1];
        numden = ReduceGcd(oth.numerator, denominator);
        Operand thatNum = numden[0], thisDen = numden[1];
        return new Rational<E>(ring, thisNum.Multiply(thatNum, ring, simplicityCriteria), thisDen.Multiply(thatDen, ring, simplicityCriteria));
    }


    public virtual Rational<E> Divide(Rational<E> oth)
    {
        return Multiply(oth.Reciprocal());
    }


    public virtual Rational<E> Multiply(E oth)
    {
        return Multiply(new Rational<E>(ring, oth));
    }


    public virtual Rational<E> Divide(E oth)
    {
        return Divide(new Rational<E>(ring, oth));
    }


    public virtual Rational<E> Negate()
    {
        return new Rational<E>(ring, numerator.Multiply(ring.GetNegativeOne(), ring, simplicityCriteria), denominator);
    }


    public virtual Rational<E> Add(Rational<E> that)
    {
        if (IsZero())
            return that;
        if (that.IsZero())
            return this;
        if (denominator.Expand(ring).Equals(that.denominator.Expand(ring)))
        {
            Operand num = new Operand(ring.Add(this.numerator.Expand(ring), that.numerator.Expand(ring)));
            Operand den =
                (denominator.Count > that.denominator.Count ? denominator : that.denominator).ShallowCopy();
            Operand[] numden = ReduceGcd(num, den);
            num = numden[0];
            den = numden[1];
            return new Rational<E>(ring, num, den);
        }
        else
        {
            Operand[] dens = ReduceGcd(denominator, that.denominator);
            Operand thisDen = dens[0],
                thatDen = dens[1],
                thisNum = this.numerator,
                thatNum = that.numerator,
                gcdDen = dens[2];
            Operand num = new Operand(ring.AddMutable(ring.Multiply(thisNum.Expand(ring), thatDen.Expand(ring)),
                ring.Multiply(thatNum.Expand(ring), thisDen.Expand(ring))));
            Operand den = thisDen.Multiply(thatDen, ring, simplicityCriteria).Multiply(gcdDen, ring, simplicityCriteria);
            Operand[] numden = ReduceGcd(num, den);
            num = numden[0];
            den = numden[1];
            return new Rational<E>(ring, num, den);
        }
    }


    public virtual Rational<E> Subtract(Rational<E> that)
    {
        if (IsZero())
            return that.Negate();
        if (that.IsZero())
            return this;
        if (denominator.Expand(ring).Equals(that.denominator.Expand(ring)))
        {
            Operand num = new Operand(ring.Subtract(this.numerator.Expand(ring), that.numerator.Expand(ring)));
            Operand den =
                (denominator.Count > that.denominator.Count ? denominator : that.denominator).ShallowCopy();
            Operand[] numden = ReduceGcd(num, den);
            num = numden[0];
            den = numden[1];
            return new Rational<E>(ring, num, den);
        }
        else
        {
            Operand[] dens = ReduceGcd(denominator, that.denominator);
            Operand thisDen = dens[0],
                thatDen = dens[1],
                thisNum = this.numerator,
                thatNum = that.numerator,
                gcdDen = dens[2];
            Operand num = new Operand(ring.SubtractMutable(ring.Multiply(thisNum.Expand(ring), thatDen.Expand(ring)),
                ring.Multiply(thatNum.Expand(ring), thisDen.Expand(ring))));
            Operand den = thisDen.Multiply(thatDen, ring, simplicityCriteria).Multiply(gcdDen, ring, simplicityCriteria);
            Operand[] numden = ReduceGcd(num, den);
            num = numden[0];
            den = numden[1];
            return new Rational<E>(ring, num, den);
        }
    }


    public virtual Rational<E> Add(E that)
    {
        return Add(new Rational<E>(ring, that));
    }


    public virtual Rational<E> Subtract(E that)
    {
        return Subtract(new Rational<E>(ring, that));
    }


    public virtual int CompareTo(Rational<E>? @object)
    {
        E nOd = ring.Multiply(numerator.Expand(ring), @object.denominator.Expand(ring));
        E dOn = ring.Multiply(denominator.Expand(ring), @object.numerator.Expand(ring));
        return ring.Compare(nOd, dOn);
    }


    public virtual Rational<E> Pow(int exponent)
    {
        return exponent >= 0
            ? new Rational<E>(true, ring, numerator.Pow(exponent, ring), denominator.Pow(exponent, ring))
            : Reciprocal().Pow(-exponent);
    }


    public virtual Rational<E> Pow(long exponent)
    {
        return exponent >= 0
            ? new Rational<E>(true, ring, numerator.Pow(exponent, ring), denominator.Pow(exponent, ring))
            : Reciprocal().Pow(-exponent);
    }


    public virtual Rational<E> Pow(BigInteger exponent)
    {
        return exponent.Sign >= 0
            ? new Rational<E>(true, ring, numerator.Pow(exponent, ring), denominator.Pow(exponent, ring))
            : Reciprocal().Pow(-exponent);
    }


    public virtual Rational<O> Map<O>(Ring<O> ring, Func<E, O> function)
    {
        return new Rational<O>(ring, function(numerator.Expand(this.ring)), function(denominator.Expand(this.ring)));
    }


    public virtual Rational<E> Map(Func<E, E> function)
    {
        Operand num = numerator.Map(function);
        Operand den = denominator.Map(function);
        Operand[] nd = ReduceGcd(num, den);
        return new Rational<E>(ring, nd[0], nd[1]);
    }


    public virtual IEnumerable<E> Stream()
    {
        return [numerator.Expand(ring), denominator.Expand(ring)];
    }

    public override bool Equals(object? o)
    {
        if (this == o)
            return true;
        if (o == null || GetType() != o.GetType())
            return false;
        Rational<E> that = (Rational<E>)o;
        if (!numerator.Expand(ring).Equals(that.numerator.Expand(ring)))
            return false;
        return denominator.Expand(ring).Equals(that.denominator.Expand(ring));
    }

    public override int GetHashCode()
    {
        int result = numerator.Expand(ring).GetHashCode();
        result = 31 * result + denominator.Expand(ring).GetHashCode();
        return result;
    }

    // public virtual string ToStringFactors(IStringifier<Rational<E>> stringifier)
    // {
    //     IStringifier<E> str = stringifier.Substringifier(ring);
    //     if (IsIntegral())
    //         return str.Stringify(numerator.Expand(ring));
    //     string num = string.Join('*', numerator.Select((s) => "(" + str.Stringify(s) + ")"));
    //     string den = string.Join('*', denominator.Select((s) => "(" + str.Stringify(s) + ")"));
    //     return EncloseMathParenthesisInSumIfNeeded(num) + "/" +
    //            (IStringifier<Rational<E>>.HasMulDivPlusMinus(0, den) ? "(" + den + ")" : den);
    // }


    public override string ToString()
    {
        if (IsIntegral())
            return numerator.Expand(ring).ToString();
        string num = (numerator.Expand(ring)).ToString();
        string den = (denominator.Expand(ring)).ToString();
        return $"({num})/({den})";
    }

    public Rational<E> Clone()
    {
        return new Rational<E>(ring, numerator.DeepCopy(ring), denominator.DeepCopy(ring));
    }
    
    public static Rational<E> operator +(Rational<E> a, Rational<E> b) =>
        a.Clone().Add(b);
    public static Rational<E> operator -(Rational<E> a, Rational<E> b) =>
        a.Clone().Subtract(b);
    public static Rational<E> operator *(Rational<E> a, Rational<E> b) =>
        a.Clone().Multiply(b);
    public static Rational<E> operator /(Rational<E> a, Rational<E> b) =>
        a.Divide(b);
    public static Rational<E> operator %(Rational<E> a, Rational<E> b) =>
        Rational<E>.Zero(a.ring);
    public static Rational<E> operator -(Rational<E> a) =>
        a.Clone().Negate();
    
    
    public static Rational<E> operator +(E a, Rational<E> b) =>
        b.Clone().Add(a);
    public static Rational<E> operator -(E a, Rational<E> b) =>
        b.Clone().Subtract(a);
    public static Rational<E> operator *(E a, Rational<E> b) =>
        b.Clone().Multiply(a);
    
    public static Rational<E> operator +(Rational<E> a, E b) =>
        a.Clone().Add(b);
    public static Rational<E> operator -(Rational<E> a, E b) =>
        a.Clone().Subtract(b);
    public static Rational<E> operator *(Rational<E> a, E b) =>
        a.Clone().Multiply(b);
    public static Rational<E> operator /(Rational<E> a, E b) =>
        a.Clone().Divide(b);

    public static Rational<E> operator +(long a, Rational<E> b) =>
        b + b.ring.ValueOfLong(a);

    public static Rational<E> operator -(long a, Rational<E> b) =>
        b - b.ring.ValueOfLong(a);

    public static Rational<E> operator *(long a, Rational<E> b) =>
        b * b.ring.ValueOfLong(a);


    public static Rational<E> operator +(Rational<E> a, long b) =>
        a + a.ring.ValueOfLong(b);
    public static Rational<E> operator -(Rational<E> a, long b) =>
        a - a.ring.ValueOfLong(b);
 
    public static Rational<E> operator *(Rational<E> a, long b) =>
        a * a.ring.ValueOfLong(b);
    public static Rational<E> operator /(Rational<E> a, long b) =>
        a / a.ring.ValueOfLong(b);
}