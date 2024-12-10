using Cc.Redberry.Rings.Io;
using Cc.Redberry.Rings.Poly;
using Cc.Redberry.Rings.Util;
using System.Text;

namespace Cc.Redberry.Rings
{
    /// <summary>
    /// Factor decomposition of element. Unit coefficient of decomposition is stored in {@link #unit}, factors returned by
    /// {@link #get(int)} are non-units. This class is mutable. <i>Iterable</i> specification provides iterator over
    /// non-unit factors only; to iterate over all factors including the constant factor use {@link #iterableWithUnit()}
    /// </summary>
    /// <remarks>
    /// @authorStanislav Poslavsky
    /// @since2.2
    /// </remarks>
    public class FactorDecomposition<E> : Stringifiable<E>
    {
        /// <summary>
        /// The ring
        /// </summary>
        public readonly Ring<E> ring;

        /// <summary>
        /// unit coefficient
        /// </summary>
        public E unit;

        /// <summary>
        /// factors
        /// </summary>
        public readonly List<E> factors;

        /// <summary>
        /// exponents
        /// </summary>
        public readonly TIntArrayList exponents;


        protected FactorDecomposition(Ring<E> ring, E unit, List<E> factors, TIntArrayList exponents)
        {
            this.ring = ring;
            this.unit = unit;
            this.factors = factors;
            this.exponents = exponents;
            if (!IsUnit(unit))
                throw new ArgumentException();
        }

        public IEnumerator<E> Iterator()
        {
            return factors.GetEnumerator();
        }

        /// <summary>
        /// Iterator over all factors including a unit one
        /// </summary>
        /// <returns>iterator over all factors including a unit one</returns>
        public IEnumerator<E> IterableWithUnit()
        {
            var it = new List<E>();
            if (!ring.IsOne(unit))
                it.Add(unit);
            it.AddRange(factors);
            return it.GetEnumerator();
        }

        public virtual bool IsUnit(E element)
        {
            return ring.IsUnit(element);
        }

        /// <summary>
        /// Returns i-th factor
        /// </summary>
        public virtual E Get(int i)
        {
            return factors[i];
        }
        
        /// <summary>
        /// Returns i-th factor
        /// </summary>
        public E this[int i] => factors[i];

        public virtual int GetExponent(int i)
        {
            return exponents[i];
        }

       
        /// <summary>
        /// Number of non-constant factors
        /// </summary>
        public int Count => factors.Count;
        
        /// <summary>
        /// Number of non-constant factors
        /// </summary>
        public int Size()
        {
            return factors.Count;
        }

        /// <summary>
        /// Whether this is a trivial factorization (contains only one factor)
        /// </summary>
        public virtual bool IsTrivial()
        {
            return Size() == 1;
        }

       
        /// <summary>
        /// Sum all exponents
        /// </summary>
        public virtual int SumExponents()
        {
            return exponents.Sum();
        }

       
        /// <summary>
        /// Multiply each exponent by a given factor
        /// </summary>
        public virtual void RaiseExponents(long val)
        {
            for (var i = exponents.size() - 1; i >= 0; --i)
                exponents[i] = MachineArithmetic.SafeToInt(exponents[i] * val);
        }

        /// <summary>
        /// Sets the unit factor
        /// </summary>
        public virtual FactorDecomposition<E> SetUnit(E unit)
        {
            if (!IsUnit(unit))
                throw new ArgumentException("not a unit: " + unit);
            this.unit = unit;
            return this;
        }

       
        /// <summary>
        /// add another unit factor
        /// </summary>
        public virtual FactorDecomposition<E> AddUnit(E unit)
        {

            //        if (!isUnit(unit))
            //            throw new IllegalArgumentException("not a unit: " + unit);
            this.unit = ring.Multiply(this.unit, unit);
            return this;
        }

        /// <summary>
        /// add another unit factor
        /// </summary>
        public virtual FactorDecomposition<E> AddUnit(E unit, int exponent)
        {

            //        if (!isUnit(unit))
            //            throw new IllegalArgumentException("not a unit: " + unit);
            if (ring.IsOne(unit))
                return this;
            this.unit = ring.Multiply(this.unit, ring.Pow(unit, exponent));
            return this;
        }

        /// <summary>
        /// add another factor
        /// </summary>
        public virtual FactorDecomposition<E> AddFactor(E factor, int exponent)
        {
            if (IsUnit(factor))
                return AddUnit(factor, exponent);
            factors.Add(factor);
            exponents.Add(exponent);
            return this;
        }

        /// <summary>
        /// add all factors from other
        /// </summary>
        public virtual FactorDecomposition<E> AddAll(FactorDecomposition<E> other)
        {
            AddUnit(other.unit);
            factors.AddRange(other.factors);
            exponents.AddAll(other.exponents);
            return this;
        }

      
        public virtual FactorDecomposition<E> AddNonUnitFactor(E factor, int exponent)
        {
            factors.Add(factor);
            exponents.Add(exponent);
            return this;
        }

        /// <summary>
        /// Raise all factors to its corresponding exponents
        /// </summary>
        public virtual FactorDecomposition<E> ApplyExponents()
        {
            var newFactors = new List<E>();
            for (var i = 0; i < Size(); i++)
                newFactors.Add(ring.Pow(factors[i], exponents[i]));
            return new FactorDecomposition<E>(ring, unit, newFactors, new TIntArrayList(ArraysUtil.ArrayOf(1, Size())));
        }

       
        /// <summary>
        /// Raise all factors to its corresponding exponents
        /// </summary>
        public virtual FactorDecomposition<E> ApplyConstantFactor()
        {
            var newFactors = factors.Select(ring.Copy).ToList();
            if (newFactors.Count == 0)
                newFactors.Add(ring.Copy(unit));
            else
                newFactors[0] = ring.MultiplyMutable(newFactors[0], ring.Copy(unit));
            return new FactorDecomposition<E>(ring, ring.GetOne(), newFactors, new TIntArrayList(exponents));
        }

      
        /// <summary>
        /// Set all exponents to one
        /// </summary>
        public virtual FactorDecomposition<E> DropExponents()
        {
            return new FactorDecomposition<E>(ring, unit, factors, new TIntArrayList(ArraysUtil.ArrayOf(1, Size())));
        }

       
        /// <summary>
        /// Drops constant factor from this (new instance returned)
        /// </summary>
        public virtual FactorDecomposition<E> DropUnit()
        {
            this.unit = ring.GetOne();
            return this;
        }

        /// <summary>
        /// Remove specified factor
        /// </summary>
        public virtual FactorDecomposition<E> DropFactor(int i)
        {
            exponents.RemoveAt(i);
            factors.RemoveAt(i);
            return this;
        }

       
        /// <summary>
        /// Stream of all factors
        /// </summary>
        public virtual IEnumerable<E> Stream()
        {
            return Stream.Concat(Stream.Of(unit), factors.Stream());
        }

        
        /// <summary>
        /// Stream of all factors except {@link #unit}
        /// </summary>
        public virtual IEnumerable<E> StreamWithoutUnit()
        {
            return factors;
        }

       
        /// <summary>
        /// Array of factors without constant factor
        /// </summary>
        public virtual E[] ToArrayWithoutUnit()
        {
            return factors.ToArray();
        }

       
        /// <summary>
        /// Array of factors without constant factor
        /// </summary>
        public virtual E[] ToArrayWithUnit()
        {
            var array = new E[Size() + 1];
            array[0] = unit;
            factors.CopyTo(array, 1);
            return array;
        }

       
        /// <summary>
        /// Multiply factors
        /// </summary>
        public virtual E Multiply()
        {
            return Multiply0(false);
        }

        
        /// <summary>
        /// Multiply with no account for exponents
        /// </summary>
        public virtual E MultiplyIgnoreExponents()
        {
            return Multiply0(true);
        }

   
     
        /// <summary>
        /// Square-free part
        /// </summary>
        public virtual E SquareFreePart()
        {
            return MultiplyIgnoreExponents();
        }

       
        private E Multiply0(bool ignoreExponents)
        {
            var r = ring.Copy(unit);
            for (var i = 0; i < factors.Count; i++)
            {
                var tmp = ignoreExponents ? factors[i] : ring.Pow(factors[i], exponents[i]);
                r = ring.MultiplyMutable(r, tmp);
            }

            return r;
        }

        /// <summary>
        /// Sort factors.
        /// </summary>
        public virtual FactorDecomposition<E> Canonical()
        {
            var wr = factors.Select((e) => new wrapper(ring, e)).ToArray();
            int[] ex = exponents.ToArray();
            ArraysUtil.QuickSort(wr, ex);
            factors.Clear();
            exponents.Clear();
            factors.AddRange(wr.Select(w => w.el).ToList());
            exponents.AddAll(ex);
            return this;
        }

        private sealed class wrapper : IComparable<wrapper>
        {
            readonly Ring<E> ring;
            public readonly E el;

            public wrapper(Ring<E> ring, E el)
            {
                this.ring = ring;
                this.el = el;
            }

            public int CompareTo(wrapper o)
            {
                return ring.Compare(el, o.el);
            }
        }

      
        public virtual FactorDecomposition<R> MapTo<R>(Ring<R> othRing, Func<E, R> mapper)
        {
            return Of(othRing, mapper(unit), factors.Select(mapper).ToList(), exponents);
        }

        
        public virtual FactorDecomposition<E> Apply(Func<E, E> mapper)
        {
            return Of(ring, mapper(unit), factors.Select(mapper).ToList(), exponents);
        }

        public virtual string ToString(IStringifier<E> stringifier)
        {
            if (factors.Count == 0)
                return "(" + stringifier.Stringify(unit) + ")";
            var sb = new StringBuilder();
            if (!ring.IsOne(unit))
                sb.Append("(").Append(stringifier.Stringify(unit)).Append(")");
            for (var i = 0; i < factors.Count; i++)
            {
                if (sb.Length > 0)
                    sb.Append("*");
                sb.Append("(").Append(stringifier.Stringify(factors[i])).Append(")");
                if (exponents[i] != 1)
                    sb.Append("^").Append(exponents[i]);
            }

            return sb.ToString();
        }

       
        public override string ToString()
        {
            return ToString(IStringifier<E>.Dummy<E>());
        }

        
        public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            var factors1 = (FactorDecomposition<E>)o;
            if (!unit.Equals(factors1.unit))
                return false;
            if (!factors.Equals(factors1.factors))
                return false;
            return exponents.Equals(factors1.exponents);
        }

        
        public override int GetHashCode()
        {
            var result = 17;
            result = 31 * result + unit.GetHashCode();
            result = 31 * result + factors.GetHashCode();
            result = 31 * result + exponents.GetHashCode();
            return result;
        }

       
        public virtual FactorDecomposition<E> Clone()
        {
            return new FactorDecomposition<E>(ring, ring.Copy(unit), factors.Select(ring.Copy).ToList(), new TIntArrayList(exponents));
        }

       
        /// <summary>
        /// Unit factorization
        /// </summary>
        public static FactorDecomposition<E> Unit<E>(Ring<E> ring, E unit)
        {
            if (!ring.IsUnitOrZero(unit))
                throw new ArgumentException("not a unit");
            return new FactorDecomposition<E>(ring, unit, new List<E>(), new TIntArrayList());
        }

        
        /// <summary>
        /// Empty factorization
        /// </summary>
        public static FactorDecomposition<E> Empty<E>(Ring<E> ring)
        {
            return Unit(ring, ring.GetOne());
        }

        
        /// <summary>
        /// Factor decomposition with specified factors and exponents
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="unit">the unit coefficient</param>
        /// <param name="factors">the factors</param>
        /// <param name="exponents">the exponents</param>
        public static FactorDecomposition<E> Of<E>(Ring<E> ring, E unit, IList<E> factors, TIntArrayList exponents)
        {
            if (factors.Count != exponents.Count)
                throw new ArgumentException();
            var r = Empty(ring).AddUnit(unit);
            for (var i = 0; i < factors.Count; i++)
                r.AddFactor(factors[i], exponents[i]);
            return r;
        }

      
        /// <summary>
        /// Factor decomposition with specified factors and exponents
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="factors">factors</param>
        public static FactorDecomposition<E> Of<E>(Ring<E> ring, params E[] factors)
        {
            return Of(ring, factors.ToList());
        }

      
        /// <summary>
        /// Factor decomposition with specified factors and exponents
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="factors">factors</param>
        public static FactorDecomposition<E> Of<E>(Ring<E> ring, IEnumerable<E> factors)
        {
            TObjectIntHashMap<E> map = new TObjectIntHashMap();
            foreach (var e in factors)
                map.AdjustOrPutValue(e, 1, 1);
            List<E> l = new List<E>();
            TIntArrayList ex = new TIntArrayList();
            map.ForEachEntry((a, b) =>
            {
                l.Add(a);
                ex.Add(b);
                return true;
            });
            return Of(ring, ring.GetOne(), l, ex);
        }
    }
}