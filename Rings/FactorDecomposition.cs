using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Rings.io;

namespace Rings;

public class FactorDecomposition<E> : IEnumerable<E>, Stringifiable<E>
{
    /** The ring */
    public readonly Ring<E> ring;

    /** unit coefficient */
    public E unit;

    /** factors */
    public readonly List<E> factors;

    /** exponents */
    public readonly TIntArrayList exponents;

    protected FactorDecomposition(Ring<E> ring, E unit, List<E> factors, TIntArrayList exponents)
    {
        this.ring = ring;
        this.unit = unit;
        this.factors = factors;
        this.exponents = exponents;
        
        if (!isUnit(unit))
            throw new ArgumentException();
    }


    /**
     * Iterator over all factors including a unit one
     *
     * @return iterator over all factors including a unit one
     */
    public IEnumerable<E> iterableWithUnit()
    {
        List<E> it = new List<E>();
        if (!ring.isOne(unit))
            it.Add(unit);
        it.AddRange(factors);
        return it;
    }

    public bool isUnit(E element)
    {
        return ring.isUnit(element);
    }

    /** Returns i-th factor */
    public E get(int i)
    {
        return factors[i];
    }

    /** Exponent of i-th factor */
    public int getExponent(int i)
    {
        return exponents[i];
    }

    /** Number of non-constant factors */
    public int size()
    {
        return factors.Count;
    }

    /** Whether this is a trivial factorization (contains only one factor) */
    public bool isTrivial()
    {
        return size() == 1;
    }

    /** Sum all exponents */
    public int sumExponents()
    {
        return exponents.sum();
    }

    /** Multiply each exponent by a given factor */
    public void raiseExponents(long val)
    {
        for (int i = exponents.size() - 1; i >= 0; --i)
            exponents.set(i, MachineArithmetic.safeToInt(exponents[i] * val));
    }

    /** Sets the unit factor */
    public FactorDecomposition<E> setUnit(E unit)
    {
        if (!isUnit(unit))
            throw new ArgumentException("not a unit: " + unit);
        this.unit = unit;
        return this;
    }

    /** add another unit factor */
    public FactorDecomposition<E> addUnit(E unit)
    {
//        if (!isUnit(unit))
//            throw new IllegalArgumentException("not a unit: " + unit);
        this.unit = ring.multiply(this.unit, unit);
        return this;
    }

    /** add another unit factor */
    public FactorDecomposition<E> addUnit(E unit, int exponent)
    {
//        if (!isUnit(unit))
//            throw new IllegalArgumentException("not a unit: " + unit);
        if (ring.isOne(unit))
            return this;
        this.unit = ring.multiply(this.unit, ring.pow(unit, exponent));
        return this;
    }

    /** add another factor */
    public FactorDecomposition<E> addFactor(E factor, int exponent)
    {
        if (isUnit(factor))
            return addUnit(factor, exponent);
        factors.Add(factor);
        exponents.add(exponent);
        return this;
    }

    /** add all factors from other */
    public FactorDecomposition<E> addAll(FactorDecomposition<E> other)
    {
        addUnit(other.unit);
        factors.AddRange(other.factors);
        exponents.addAll(other.exponents);
        return this;
    }

    FactorDecomposition<E> addNonUnitFactor(E factor, int exponent)
    {
        factors.Add(factor);
        exponents.add(exponent);
        return this;
    }

    /**
     * Raise all factors to its corresponding exponents
     */
    public FactorDecomposition<E> applyExponents()
    {
        List<E> newFactors = new();
        for (int i = 0; i < size(); i++)
            newFactors.Add(ring.pow(factors[i], exponents.get(i)));
        return new(ring, unit, newFactors, new TIntArrayList(ArraysUtil.arrayOf(1, size())));
    }

    /**
     * Raise all factors to its corresponding exponents
     */
    public FactorDecomposition<E> applyConstantFactor()
    {
        List<E> newFactors = factors.Select(ring.copy).ToList();
        if (newFactors.Count == 0)
            newFactors.Add(ring.copy(unit));
        else
            newFactors[0] = ring.multiplyMutable(newFactors[0], ring.copy(unit));
        return new(ring, ring.getOne(), newFactors, new TIntArrayList(exponents));
    }

    /**
     * Set all exponents to one
     */
    public FactorDecomposition<E> dropExponents()
    {
        return new(ring, unit, factors, new TIntArrayList(ArraysUtil.arrayOf(1, size())));
    }

    /**
     * Drops constant factor from this (new instance returned)
     */
    public FactorDecomposition<E> dropUnit()
    {
        this.unit = ring.getOne();
        return this;
    }

    /**
     * Remove specified factor
     */
    public FactorDecomposition<E> dropFactor(int i)
    {
        exponents.removeAt(i);
        factors.RemoveAt(i);
        return this;
    }

    /** Stream of all factors */
    public IEnumerable<E> stream()
    {
        return [unit, ..factors];
    }

    /** Stream of all factors except {@link #unit} */
    public IEnumerable<E> streamWithoutUnit()
    {
        return factors;
    }

    /** Array of factors without constant factor */
    public E[] toArrayWithoutUnit()
    {
        return factors.ToArray();
    }

    /** Array of factors without constant factor */
    public E[] toArrayWithUnit()
    {
        E[] array = [unit, ..factors];
        return array;
    }

    /** Multiply factors */
    public E multiply()
    {
        return multiply0(false);
    }

    /** Multiply with no account for exponents */
    public E multiplyIgnoreExponents()
    {
        return multiply0(true);
    }

    /** Square-free part */
    public E squareFreePart()
    {
        return multiplyIgnoreExponents();
    }

    private E multiply0(bool ignoreExponents)
    {
        E r = ring.copy(unit);
        for (int i = 0; i < factors.Count; i++)
        {
            E tmp = ignoreExponents ? factors[i] : ring.pow(factors[i], exponents[i]);
            r = ring.multiplyMutable(r, tmp);
        }

        return r;
    }


    public FactorDecomposition<E> canonical()
    {
        wrapper<E>[] wr = factors.Select(e => new wrapper<E>(ring, e)).ToArray();
        int[] ex = exponents.toArray();
        ArraysUtil.quickSort(wr, ex);
        factors.Clear();
        exponents.clear();
        factors.AddRange(wr.Select(w => w.el).ToList());
        exponents.addAll(ex);
        return this;
    }

    private class wrapper<E> : IComparable<wrapper<E>>
    {
        readonly Ring<E> ring;
        public readonly E el;

        public wrapper(Ring<E> ring, E el)
        {
            this.ring = ring;
            this.el = el;
        }


        public int CompareTo(wrapper<E>? other)
        {
            return ring.compare(el, other.el);
        }
    }

    public FactorDecomposition<R> mapTo<R>(Ring<R> othRing, Func<E, R> mapper)
    {
        return of(othRing, mapper(unit), factors.Select(mapper).ToList(), exponents);
    }

    public FactorDecomposition<E> apply(Func<E, E> mapper)
    {
        return of(ring, mapper(unit), factors.Select(mapper).ToList(), exponents);
    }


    public String toString(IStringifier<E> stringifier)
    {
        if (factors.Count == 0)
            return "(" + stringifier.stringify(unit) + ")";
        StringBuilder sb = new StringBuilder();
        if (!ring.isOne(unit))
            sb.Append("(").Append(stringifier.stringify(unit)).Append(")");
        for (int i = 0; i < factors.Count; i++)
        {
            if (sb.Length > 0)
                sb.Append("*");
            sb.Append("(").Append(stringifier.stringify(factors[i])).Append(")");
            if (exponents[i] != 1)
                sb.Append("^").Append(exponents.get(i));
        }

        return sb.ToString();
    }


    public String toString()
    {
        return toString(IStringifier.dummy());
    }


    public bool equals(Object o)
    {
        if (this == o) return true;
        if (o == null || GetType() != o.GetType()) return false;

        FactorDecomposition <_> factors1 = (FactorDecomposition <_>) o;

        if (!unit.Equals(factors1.unit)) return false;
        if (!factors.Equals(factors1.factors)) return false;
        return exponents.equals(factors1.exponents);
    }


    public int hashCode()
    {
        int result = 17;
        result = 31 * result + unit.GetHashCode();
        result = 31 * result + factors.GetHashCode();
        result = 31 * result + exponents.hashCode();
        return result;
    }


    public FactorDecomposition<E> clone()
    {
        return new(
            ring,
            ring.copy(unit),
            factors.Select(ring.copy).ToList(),
            new TIntArrayList(exponents));
    }

    /** Unit factorization */
    public static FactorDecomposition<E> Unit<E>(Ring<E> ring, E unit)
    {
        if (!ring.isUnitOrZero(unit))
            throw new ArgumentException("not a unit");
        return new(ring, unit, new List<E>(), new TIntArrayList());
    }

    /** Empty factorization */
    public static FactorDecomposition<E> empty<E>(Ring<E> ring)
    {
        return Unit(ring, ring.getOne());
    }

    /**
     * Factor decomposition with specified factors and exponents
     *
     * @param ring      the ring
     * @param unit      the unit coefficient
     * @param factors   the factors
     * @param exponents the exponents
     */
    public static FactorDecomposition<E> of<E>(Ring<E> ring, E unit, List<E> factors, TIntArrayList exponents)
    {
        if (factors.Count != exponents.size())
            throw new ArgumentException();
        FactorDecomposition<E> r = empty(ring).addUnit(unit);
        for (int i = 0; i < factors.Count; i++)
            r.addFactor(factors[i], exponents[i]);
        return r;
    }

    /**
     * Factor decomposition with specified factors and exponents
     *
     * @param ring    the ring
     * @param factors factors
     */
    public static FactorDecomposition<E> of<E>(Ring<E> ring, params E[] factors)
    {
        return of(ring, factors.ToList());
    }

    /**
     * Factor decomposition with specified factors and exponents
     *
     * @param ring    the ring
     * @param factors factors
     */
    public static FactorDecomposition<E> of<E>(Ring<E> ring, IEnumerable<E> factors)
    {
        TObjectIntHashMap<E> map = new();
        foreach (E e in factors)
            map.adjustOrPutValue(e, 1, 1);
        List<E> l = new();
        TIntArrayList e = new TIntArrayList();
        map.forEachEntry((a, b) =>
        {
            l.Add(a);
            e.Add(b);
            return true;
        });
        return of(ring, ring.getOne(), l, e);
    }

    public IEnumerator<E> GetEnumerator()
    {
        return factors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}