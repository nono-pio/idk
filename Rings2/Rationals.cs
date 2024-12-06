using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using Cc.Redberry.Rings.Io;


namespace Cc.Redberry.Rings
{
    /// <summary>
    /// The ring of rationals (Q).
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class Rationals<E> : Ring<Rational<E>>
    {
        private static readonly long serialVersionUID = 1;

        /// <summary>
        /// Ring that numerator and denominator belongs to
        /// </summary>
        public readonly Ring<E> ring;

        public Rationals(Ring<E> ring)
        {
            this.ring = ring;
        }

        /// <summary>
        /// Gives rational with a given numerator and unit denominator
        /// </summary>
        public Rational<E> MkNumerator(E num)
        {
            return new Rational<E>(ring, num);
        }


        /// <summary>
        /// Gives rational with a given numerator and unit denominator
        /// </summary>
        public Rational<E> MkNumerator(long num)
        {
            return MkNumerator(ring.ValueOf(num));
        }


        /// <summary>
        /// Gives rational with a given denominator and unit numerator
        /// </summary>
        public Rational<E> MkDenominator(E den)
        {
            return new Rational<E>(ring, ring.GetOne(), den);
        }


        /// <summary>
        /// Gives rational with a given denominator and unit numerator
        /// </summary>
        public Rational<E> MkDenominator(long den)
        {
            return MkDenominator(ring.ValueOf(den));
        }


        /// <summary>
        /// Gives rational with a given numerator and denominator
        /// </summary>
        public Rational<E> Mk(E num, E den)
        {
            return new Rational<E>(ring, num, den);
        }


        /// <summary>
        /// Gives rational with a given numerator and denominator
        /// </summary>
        public Rational<E> Mk(long num, long den)
        {
            return new Rational<E>(ring, ring.ValueOf(num), ring.ValueOf(den));
        }


        public bool IsField()
        {
            return true;
        }


        public bool IsEuclideanRing()
        {
            return true;
        }


        public BigInteger Cardinality()
        {
            return null;
        }


        public BigInteger Characteristic()
        {
            return BigInteger.Zero;
        }


        public bool IsPerfectPower()
        {
            return false;
        }


        public BigInteger PerfectPowerBase()
        {
            return null;
        }


        public BigInteger PerfectPowerExponent()
        {
            return null;
        }


        public Rational<E> Add(Rational<E> a, Rational<E> b)
        {
            return a.Add(b);
        }


        public Rational<E> Subtract(Rational<E> a, Rational<E> b)
        {
            return a.Subtract(b);
        }


        public Rational<E> Multiply(Rational<E> a, Rational<E> b)
        {
            return a.Multiply(b);
        }


        public Rational<E> Negate(Rational<E> element)
        {
            return element.Negate();
        }


        public int Signum(Rational<E> element)
        {
            return element.Signum();
        }

        public Rational<E>[] DivideAndRemainder(Rational<E> dividend, Rational<E> divider)
        {
            return new Rational<E>[]
            {
                dividend.Divide(divider),
                Rational<E>.Zero(ring)
            };
        }

        public Rational<E> Reciprocal(Rational<E> element)
        {
            return element.Reciprocal();
        }


        public Rational<E> Gcd(Rational<E> a, Rational<E> b)
        {
            return Rational<E>.One(ring);
        }


        private FactorDecomposition<Rational<E>> Factor(Rational<E> element, Func<E, FactorDecomposition<E>> factor)
        {
            if (element.IsZero())
                return FactorDecomposition<Rational<E>>.Of(this, element);
            FactorDecomposition<E> numFactors = element.numerator.Select(factor).Aggregate(
                FactorDecomposition<E>.Empty(ring),
                (f, newFac) => f.AddAll(newFac));
            FactorDecomposition<Rational<E>> factors = FactorDecomposition<Rational<E>>.Empty(this);
            for (int i = 0; i < numFactors.Count; i++)
                factors.AddNonUnitFactor(new Rational<E>(ring, numFactors[i]), numFactors.GetExponent(i));
            factors.AddFactor(new Rational<E>(ring, numFactors.unit), 1);
            FactorDecomposition<E> denFactors = element.denominator.Select(factor).Aggregate(
                FactorDecomposition<E>.Empty(ring),
                (f, newFac) => f.AddAll(newFac));
            for (int i = 0; i < denFactors.Count; i++)
                factors.AddNonUnitFactor(new Rational<E>(ring, ring.GetOne(), denFactors[i]),
                    denFactors.GetExponent(i));
            factors.AddFactor(new Rational<E>(ring, ring.GetOne(), denFactors.unit), 1);
            return factors;
        }

        public FactorDecomposition<Rational<E>> FactorSquareFree(Rational<E> element)
        {
            return Factor(element, ring.FactorSquareFree);
        }


        public FactorDecomposition<Rational<E>> Factor(Rational<E> element)
        {
            return Factor(element, ring.Factor);
        }


        public Rational<E> GetZero()
        {
            return Rational<E>.Zero(ring);
        }


        public Rational<E> GetOne()
        {
            return Rational<E>.One(ring);
        }


        public bool IsZero(Rational<E> element)
        {
            return element.IsZero();
        }

        public bool IsOne(Rational<E> element)
        {
            return element.IsOne();
        }


        public bool IsUnit(Rational<E> element)
        {
            return !IsZero(element);
        }

        public Rational<E> ValueOf(long val)
        {
            return new Rational<E>(ring, ring.ValueOf(val));
        }


        public Rational<E> ValueOfBigInteger(BigInteger val)
        {
            return new Rational<E>(ring, ring.ValueOfBigInteger(val));
        }


        public Rational<E> Copy(Rational<E> element)
        {
            return new Rational<E>(true, ring, element.numerator.DeepCopy(), element.denominator.DeepCopy());
        }


        public Rational<E> ValueOf(Rational<E> val)
        {
            if (val.ring.Equals(ring))
                return val;
            else
                return new Rational<E>(ring, val.numerator.Map(ring.ValueOf), val.denominator.Map(ring.ValueOf));
        }

        public int Compare(Rational<E> o1, Rational<E> o2)
        {
            return o1.CompareTo(o2);
        }


        public Rational<E> GetNegativeOne()
        {
            return Rational<E>.One(ring).Negate();
        }

        public Rational<E> RandomElement(Random rnd)
        {
            long den;
            E eden;
            do
            {
                den = rnd.Next();
            } while (ring.IsZero(eden = ring.ValueOf(den)));

            return new Rational<E>(ring, ring.ValueOf(rnd.Next()), eden);
        }

        public Rational<E> RandomElementTree(Random rnd)
        {
            E den;
            do
            {
                den = ring.RandomElementTree(rnd);
            } while (ring.IsZero(den));

            return new Rational<E>(ring, ring.RandomElementTree(rnd), den);
        }

        public IEnumerator<Rational<E>> Iterator()
        {
            throw new NotSupportedException("Ring of infinite cardinality.");
        }


        public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            Rationals<TWildcardTodo> rationals = (Rationals<TWildcardTodo>)o;
            return ring.Equals(rationals.ring);
        }

        public override int GetHashCode()
        {
            return ring.GetHashCode();
        }


        public string ToString(IStringifier<Rational<E>> stringifier)
        {
            return ring.Equals(Rings.Z) ? "Q" : "Frac(" + ring.ToString(stringifier.Substringifier(ring)) + ")";
        }


        public override string ToString()
        {
            return ToString(IStringifier<Rational<E>>.Dummy<Rational<E>>());
        }
    }
}