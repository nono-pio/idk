using Cc.Redberry.Rings.Io;
using Cc.Redberry.Rings.Poly;
using Cc.Redberry.Rings.Poly.Multivar;
using Cc.Redberry.Rings.Poly.Univar;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using Cc.Redberry.Rings.Bigint;

namespace Cc.Redberry.Rings
{
    /// <summary>
    /// </summary>
    public class Rational<E> : IComparable<Rational<E>>, Stringifiable<Rational<E>>
    {
        private static readonly long serialVersionUID = 1;

        /// <summary>
        /// The ring.
        /// </summary>
        public readonly Ring<E> ring;

        /// <summary>
        /// The numerator factors.
        /// </summary>
        public readonly Operand numerator;

        /// <summary>
        /// The denominator factors.
        /// </summary>
        public readonly Operand denominator;

        /// <summary>
        /// whether the underlying ring is a polynomial ring
        /// </summary>
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
            if (denominator.IsZero())
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
            if (denominator.IsZero())
                throw new ArithmeticException("division by zero");
            this.ring = ring;
            this.simplicityCriteria = SimplicityCriteria(ring);
            this.numerator = numerator;
            this.denominator = denominator;
        }


        /// <summary>
        /// polynomials with size smaller than this will be multiplied
        /// </summary>
        private static readonly int
            SIMPLE_UPOLY_SIZE = 8,
            SIMPLE_MPOLY_DENSE_SIZE = 3,
            SIMPLE_MPOLY_SPARSE_SIZE = 16; // sparse multivariate polynomials


        private static readonly double SIMPLE_POLY_SPARSITY2 = 0.001;


        /// <summary>
        /// integers with bit length smaller than this will be multiplied
        /// </summary>
        private static readonly int SIMPLE_INTEGER_N_BITS = 512;


        // criteria singletons
        private static readonly Predicate<BigInteger> intSimplicityCriteria =
            (Predicate<BigInteger> & java.io.Serializable)((p) => p.BitLength() <= SIMPLE_INTEGER_N_BITS);


        private static readonly Predicate<IUnivariatePolynomial> upolySimplicityCriteria =
            (Predicate<IUnivariatePolynomial> & java.io.Serializable)((p) => p.Count <= SIMPLE_UPOLY_SIZE);


        private static readonly Predicate<AMultivariatePolynomial> mpolySimplicityCriteria =
            (Predicate<AMultivariatePolynomial> & java.io.Serializable)((p) =>
                p.Count <= SIMPLE_MPOLY_DENSE_SIZE ||
                (p.Count < SIMPLE_MPOLY_SPARSE_SIZE && p.Sparsity2() < SIMPLE_POLY_SPARSITY2));


        private static readonly Predicate defaultFalse = (Predicate & java.io.Serializable)((__) => false);


        private static Predicate<E> SimplicityCriteria<E>(Ring<E> ring)
        {
            if (ring is UnivariateRing)
                return (Predicate<E>)upolySimplicityCriteria;
            else if (ring is MultivariateRing)
                return (Predicate<E>)mpolySimplicityCriteria;
            else if (ring is AIntegers)
                return (Predicate<E>)intSimplicityCriteria;
            else
                return defaultFalse;
        }


        /// <summary>
        /// Constructs zero
        /// </summary>
        public static Rational<E> Zero<E>(Ring<E> ring)
        {
            return new Rational<E>(ring, ring.GetZero());
        }


        /// <summary>
        /// Constructs one
        /// </summary>
        public static Rational<E> One<E>(Ring<E> ring)
        {
            return new Rational<E>(ring, ring.GetOne());
        }


        /// <summary>
        /// A single operand (either numerator or denominator) represented by a list of factors. If there is a unit factor it
        /// is always stored in the first position.
        /// </summary>
        public sealed class Operand : List<E>
        {
            /// <summary>
            /// all factors multiplied
            /// </summary>
            private E expandForm = null;


            /// <summary>
            /// some factors already multiplied (for faster #expand())
            /// </summary>
            private List<E> toExpand = this;


            public Operand()
            {
            }


            public Operand(Collection<TWildcardTodoE> c) : base(c)
            {
                if (Size() == 1)
                    SetExpandForm(Get(0));
            }


            public Operand(E el)
            {
                Add(el);
                SetExpandForm(el);
            }


            private void SetExpandForm(E e)
            {
                expandForm = e;
                toExpand = Collections.SingletonList(e);
            }


            /// <summary>
            /// normalize list of factors: get rid of units and treat other special cases
            /// </summary>
            public void Normalize()
            {
                if (IsEmpty())
                {
                    Set(ring.GetOne());
                    return;
                }

                if (Size() == 1)
                {
                    if (ring.Signum(First()) < 0)
                    {
                        Set(0, ring.Negate(First()));
                        Add(0, ring.GetNegativeOne());
                    }

                    return;
                }

                E unit = ring.GetOne(), simple = ring.GetOne();
                for (int i = size() - 1; i >= 0; --i)
                    if (ring.IsUnitOrZero(Get(i)))
                        unit = ring.MultiplyMutable(unit, Remove(i));
                    else
                    {
                        if (ring.Signum(Get(i)) < 0)
                        {
                            Set(i, ring.Negate(Get(i)));
                            unit = ring.Negate(unit);
                        }

                        if (simplicityCriteria.Test(Get(i)))
                        {
                            if (!simplicityCriteria.Test(simple))
                            {
                                Add(simple);
                                simple = Remove(i);
                            }
                            else
                                simple = ring.MultiplyMutable(simple, Remove(i));
                        }
                    }

                if (!ring.IsOne(simple))
                    Add(simple);
                if (ring.IsZero(unit))
                    Clear();
                if (!ring.IsOne(unit) || IsEmpty())
                    Add(0, unit);
            }


            /// <summary>
            /// Gives expanded form of this operand
            /// </summary>
            public E Expand()
            {
                if (expandForm != null)
                    return expandForm;
                if (Size() == 1)
                    expandForm = Get(0);
                else
                    expandForm = ring.Multiply(toExpand);
                toExpand = Collections.SingletonList(expandForm);
                return expandForm;
            }


            /// <summary>
            /// Whether the operand is zero
            /// </summary>
            public bool IsZero()
            {
                return Size() == 1 && ring.IsZero(Get(0));
            }


            /// <summary>
            /// Whether the operand is unit
            /// </summary>
            public bool IsUnit()
            {
                return Size() == 1 && ring.IsUnit(Get(0));
            }


            /// <summary>
            /// Whether the operand is one
            /// </summary>
            public bool IsOne()
            {
                return IsUnit() && ring.IsOne(Get(0));
            }


            /// <summary>
            /// get the first element in list
            /// </summary>
            private E First()
            {
                return Get(0);
            }


            /// <summary>
            /// get the last element in list
            /// </summary>
            private E Last()
            {
                return Get(Size() - 1);
            }


            /// <summary>
            /// get the unit element if it is in list
            /// </summary>
            public E UnitOrNull()
            {
                E u = First();
                if (ring.IsUnit(u))
                    return u;
                else
                    return null;
            }


            /// <summary>
            /// get the unit element if it is in list
            /// </summary>
            private E UnitOrOne()
            {
                E u = UnitOrNull();
                return u == null ? ring.GetOne() : u;
            }


            /// <summary>
            /// Multiply by other operand (shallow copy)
            /// </summary>
            Operand Multiply(Operand oth)
            {
                if (IsOne())
                    return oth;
                if (oth.IsOne())
                    return this;
                if (IsZero())
                    return this;
                if (oth.IsZero())
                    return oth;
                Operand r = new Operand();
                r.AddRange(this);
                r.AddRange(oth);
                if (expandForm != null && oth.expandForm != null)
                    r.SetExpandForm(ring.Multiply(expandForm, oth.expandForm));
                else if (toExpand != this || oth.toExpand != oth)
                {
                    r.toExpand = new List();
                    r.toExpand.AddRange(toExpand);
                    r.toExpand.AddRange(oth.toExpand);
                }


                // fix
                r.Normalize();
                return r;
            }


            /// <summary>
            /// Multiply by other operand (shallow copy)
            /// </summary>
            Operand Multiply(E oth)
            {
                if (IsOne())
                    return new Operand(oth);
                if (ring.IsOne(oth))
                    return this;
                if (IsZero())
                    return this;
                if (ring.IsZero(oth))
                    return new Operand(oth);
                Operand r = new Operand(this);
                r.Add(oth);
                if (expandForm != null)
                    r.SetExpandForm(ring.Multiply(expandForm, oth));
                if (toExpand != this)
                {
                    r.toExpand = new List<E>(toExpand);
                    r.toExpand.Add(oth);
                }

                if (ring.IsUnit(oth) || ring.Signum(oth) < 0 || simplicityCriteria.Test(oth))

                    // fix
                    r.Normalize();
                return r;
            }


            /// <summary>
            /// divide i-th factor by a given divider
            /// </summary>
            public void Divide(int i, E divider)
            {
                E div = ring.DivideExact(Get(i), divider);
                Set(i, div);
                if (expandForm != null)
                {
                    if (Size() == 1 && toExpand == this)
                        expandForm = Get(0); // already divided
                    else
                        expandForm = ring.DivideExact(expandForm, divider); // divide
                    toExpand = [expandForm];
                }
                else
                {
                    expandForm = null;
                    toExpand = this;
                } // don't normalize here!
            }

            /// <summary>
            /// divide i-th factor by a given divider
            /// </summary>
            // already divided
            // divide
            // don't normalize here!
            public override void Clear()
            {
                base.Clear();
                toExpand = this;
            }

            /// <summary>
            /// divide i-th factor by a given divider
            /// </summary>
            // already divided
            // divide
            // don't normalize here!
            private void Set(E value)
            {
                Clear();
                Add(value);
                SetExpandForm(value);
            }

            /// <summary>
            /// shallow copy of this
            /// </summary>
            public Operand ShallowCopy()
            {
                Operand op = new Operand(this);
                if (expandForm != null)
                    op.SetExpandForm(expandForm);
                else if (toExpand != this)
                    op.toExpand = new List<E>(toExpand);
                return op;
            }

            /// <summary>
            /// deep copy of this
            /// </summary>
            public Operand DeepCopy()
            {
                return Map(ring.Copy);
            }

            public Operand Map(Func<E, E> mapper)
            {
                Operand op = Stream().Map(mapper).Collect(Collectors.ToCollection(Operand.New()));
                if (toExpand != this)
                    op.toExpand = toExpand.Select(mapper).ToList();
                if (expandForm != null)
                    op.SetExpandForm(mapper(expandForm));
                return op;
            }


            public Operand Pow(long exponent)
            {
                return Pow(new BigInteger(exponent));
            }


            public Operand Pow(BigInteger exponent)
            {
                if (exponent.IsOne)
                    return DeepCopy();
                Operand pow = Stream().Map((f) => ring.Pow(f, exponent))
                    .Collect(Collectors.ToCollection(Operand.New()));
                if (expandForm != null)
                {
                    if (Size() == 1)
                        pow.expandForm = pow.First();
                    else
                        pow.expandForm = ring.Pow(expandForm, exponent);
                    pow.toExpand = [pow.expandForm];
                }
                else if (toExpand != this)
                    pow.toExpand = toExpand.Select((f) => ring.Pow(f, exponent)).ToList();

                return pow;
            }
        }


        /// <summary>
        /// bring fraction to canonical form
        /// </summary>
        private void Normalize(Operand[] numden)
        {
            Operand numerator = numden[0].ShallowCopy();
            Operand denominator;
            if (numerator.IsZero())
                denominator = new Operand(ring.GetOne());
            else
                denominator = numden[1].ShallowCopy();
            numerator.Normalize();
            denominator.Normalize();
            if (!numerator.IsZero())
                if (denominator.IsUnit())
                {
                    numerator = numerator.Multiply(ring.Reciprocal(denominator.Expand()));
                    denominator = new Operand(ring.GetOne());
                }
                else if (denominator.UnitOrNull() != null)
                {
                    E du = ring.Reciprocal(denominator.UnitOrNull());
                    denominator = denominator.Multiply(du);
                    numerator = numerator.Multiply(du);
                }

            numden[0] = numerator;
            numden[1] = denominator;
        }


        /// <summary>
        /// Reduce common content in two operands (divide each by the gcd).
        /// </summary>
        /// <returns>{aReduced, bReduced, gcd}</returns>
        private Operand[] ReduceGcd(Operand a, Operand b)
        {
            if (a.IsUnit() || b.IsUnit())
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
                    aReduced.Divide(ia, gf);
                    bReduced.Divide(ib, gf);
                }
            }

            if (gcd.IsEmpty())
                return new Rational<E>.Operand[]
                {
                    a,
                    b,
                    new Operand(ring.GetOne())
                };
            aReduced.Normalize();
            bReduced.Normalize();
            gcd.Normalize();
            return new Rational<E>.Operand[]
            {
                aReduced,
                bReduced,
                gcd
            };
        }


        /// <summary>
        /// whether this rational is zero
        /// </summary>
        public virtual bool IsZero()
        {
            return numerator.IsZero();
        }


        /// <summary>
        /// whether this rational is one
        /// </summary>
        public virtual bool IsOne()
        {
            return denominator.IsOne() && numerator.IsOne();
        }


        /// <summary>
        /// whether this rational is integral
        /// </summary>
        public virtual bool IsIntegral()
        {
            return denominator.IsOne();
        }


        /// <summary>
        /// Numerator of this rational
        /// </summary>
        public virtual E Numerator()
        {
            return numerator.Expand();
        }


        /// <summary>
        /// Numerator of this rational
        /// </summary>
        public virtual E NumeratorExact()
        {
            if (!IsIntegral())
                throw new ArgumentException("not integral fraction");
            return Numerator();
        }


        /// <summary>
        /// Denominator of this rational
        /// </summary>
        public virtual E Denominator()
        {
            return denominator.Expand();
        }


        /// <summary>
        /// Factor decomposition of denominator
        /// </summary>
        public virtual FactorDecomposition<E> FactorDenominator()
        {
            return denominator.Select(ring.Factor)
                .Aggregate(FactorDecomposition<E>.Empty(ring), (factors, newFac) => factors.AddAll(newFac));

        }


        /// <summary>
        /// Factor decomposition of denominator
        /// </summary>
        public virtual FactorDecomposition<E> FactorNumerator()
        {
            return numerator.Select(ring.Factor)
                .Aggregate(FactorDecomposition<E>.Empty(ring), (factors, newFac) => factors.AddAll(newFac));
        }


        /// <summary>
        /// Reduces this rational to normal form by doing division with remainder, that is if {@code numerator = div *
        /// denominator + rem} then the array {@code (div, rem/denominator)} will be returned. If either div or rem is zero
        /// an singleton array with this instance will be returned.
        /// </summary>
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


        /// <summary>
        /// Signum of this rational
        /// </summary>
        public virtual int Signum()
        {
            return ring.Signum(Numerator()) * ring.Signum(Denominator());
        }

        /// <summary>
        /// Returns the absolute value of this {@link Rational}.
        /// </summary>
        /// <returns>the absolute value as a {@link Rational}.</returns>
        public virtual Rational<E> Abs()
        {
            return Signum() >= 0 ? this : Negate();
        }


        /// <summary>
        /// Reciprocal of this
        /// </summary>
        public virtual Rational<E> Reciprocal()
        {
            return new Rational<E>(ring, denominator, numerator);
        }


        /// <summary>
        /// Multiply this by oth
        /// </summary>
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
            return new Rational<E>(ring, thisNum.Multiply(thatNum), thisDen.Multiply(thatDen));
        }


        /// <summary>
        /// Divide this by oth
        /// </summary>
        public virtual Rational<E> Divide(Rational<E> oth)
        {
            return Multiply(oth.Reciprocal());
        }


        /// <summary>
        /// Multiply this by oth
        /// </summary>
        public virtual Rational<E> Multiply(E oth)
        {
            return Multiply(new Rational<E>(ring, oth));
        }


        /// <summary>
        /// Divide this by oth
        /// </summary>
        public virtual Rational<E> Divide(E oth)
        {
            return Divide(new Rational<E>(ring, oth));
        }

        /// <summary>
        /// Negate this fraction
        /// </summary>
        public virtual Rational<E> Negate()
        {
            return new Rational<E>(ring, numerator.Multiply(ring.GetNegativeOne()), denominator);
        }


        /// <summary>
        /// Add that to this
        /// </summary>
        public virtual Rational<E> Add(Rational<E> that)
        {
            if (IsZero())
                return that;
            if (that.IsZero())
                return this;
            if (denominator.Expand().Equals(that.denominator.Expand()))
            {
                Operand num = new Operand(ring.Add(this.numerator.Expand(), that.numerator.Expand()));
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
                Operand num = new Operand(ring.AddMutable(ring.Multiply(thisNum.Expand(), thatDen.Expand()),
                    ring.Multiply(thatNum.Expand(), thisDen.Expand())));
                Operand den = thisDen.Multiply(thatDen).Multiply(gcdDen);
                Operand[] numden = ReduceGcd(num, den);
                num = numden[0];
                den = numden[1];
                return new Rational<E>(ring, num, den);
            }
        }


        /// <summary>
        /// Add that to this
        /// </summary>
        public virtual Rational<E> Subtract(Rational<E> that)
        {
            if (IsZero())
                return that.Negate();
            if (that.IsZero())
                return this;
            if (denominator.Expand().Equals(that.denominator.Expand()))
            {
                Operand num = new Operand(ring.Subtract(this.numerator.Expand(), that.numerator.Expand()));
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
                Operand num = new Operand(ring.SubtractMutable(ring.Multiply(thisNum.Expand(), thatDen.Expand()),
                    ring.Multiply(thatNum.Expand(), thisDen.Expand())));
                Operand den = thisDen.Multiply(thatDen).Multiply(gcdDen);
                Operand[] numden = ReduceGcd(num, den);
                num = numden[0];
                den = numden[1];
                return new Rational<E>(ring, num, den);
            }
        }


        /// <summary>
        /// Add that to this
        /// </summary>
        public virtual Rational<E> Add(E that)
        {
            return Add(new Rational<E>(ring, that));
        }


        /// <summary>
        /// Subtract that from this
        /// </summary>
        public virtual Rational<E> Subtract(E that)
        {
            return Subtract(new Rational<E>(ring, that));
        }


        /// <summary>
        /// Subtract that from this
        /// </summary>
        public virtual int CompareTo(Rational<E>? @object)
        {
            E nOd = ring.Multiply(numerator.Expand(), @object.denominator.Expand());
            E dOn = ring.Multiply(denominator.Expand(), @object.numerator.Expand());
            return ring.Compare(nOd, dOn);
        }


        /// <summary>
        /// Raise this in a power {@code exponent}
        /// </summary>
        /// <param name="exponent">exponent</param>
        public virtual Rational<E> Pow(int exponent)
        {
            return exponent >= 0
                ? new Rational<E>(true, ring, numerator.Pow(exponent), denominator.Pow(exponent))
                : Reciprocal().Pow(-exponent);
        }


        /// <summary>
        /// Raise this in a power {@code exponent}
        /// </summary>
        /// <param name="exponent">exponent</param>
        public virtual Rational<E> Pow(long exponent)
        {
            return exponent >= 0
                ? new Rational<E>(true, ring, numerator.Pow(exponent), denominator.Pow(exponent))
                : Reciprocal().Pow(-exponent);
        }


        /// <summary>
        /// Raise this in a power {@code exponent}
        /// </summary>
        /// <param name="exponent">exponent</param>
        public virtual Rational<E> Pow(BigInteger exponent)
        {
            return exponent.Signum() >= 0
                ? new Rational<E>(true, ring, numerator.Pow(exponent), denominator.Pow(exponent))
                : Reciprocal().Pow(exponent.Negate());
        }


        /// <summary>
        /// Maps rational to a new ring
        /// </summary>
        public virtual Rational<O> Map<O>(Ring<O> ring, Func<E, O> function)
        {
            return new Rational<O>(ring, function(numerator.Expand()), function(denominator.Expand()));
        }


        /// <summary>
        /// Maps rational
        /// </summary>
        public virtual Rational<E> Map(Func<E, E> function)
        {
            Operand num = numerator.Map(function);
            Operand den = denominator.Map(function);
            Operand[] nd = ReduceGcd(num, den);
            return new Rational<E>(ring, nd[0], nd[1]);
        }


        /// <summary>
        /// Stream of numerator and denominator
        /// </summary>
        public virtual IEnumerable<E> Stream()
        {
            return [numerator.Expand(), denominator.Expand()];
        }

        public override bool Equals(object o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            Rational that = (Rational)o;
            if (!numerator.Expand().Equals(that.numerator.Expand()))
                return false;
            return denominator.Expand().Equals(that.denominator.Expand());
        }

        public override int GetHashCode()
        {
            int result = numerator.Expand().GetHashCode();
            result = 31 * result + denominator.Expand().GetHashCode();
            return result;
        }


        public virtual string ToString(IStringifier<Rational<E>> stringifier)
        {
            IStringifier<E> str = stringifier.Substringifier(ring);
            if (IsIntegral())
                return str.Stringify(numerator.Expand());
            string num = str.Stringify(numerator.Expand());
            string den = str.Stringify(denominator.Expand());
            return EncloseMathParenthesisInSumIfNeeded(num) + "/" +
                   (IStringifier<Rational<E>>.HasMulDivPlusMinus(0, den) ? "(" + den + ")" : den);
        }


        public virtual string ToStringFactors(IStringifier<Rational<E>> stringifier)
        {
            IStringifier<E> str = stringifier.Substringifier(ring);
            if (IsIntegral())
                return str.Stringify(numerator.Expand());
            string num = string.Join('*', numerator.Select((s) => "(" + str.Stringify(s) + ")"));
            string den = string.Join('*', denominator.Select((s) => "(" + str.Stringify(s) + ")"));
            return EncloseMathParenthesisInSumIfNeeded(num) + "/" +
                   (IStringifier<Rational<E>>.HasMulDivPlusMinus(0, den) ? "(" + den + ")" : den);
        }


        public virtual string ToString()
        {
            return ToString(IStringifier<Rational<E>>.Dummy<Rational<E>>());
        }
    }
}