using System.Collections;
using System.Text;
using Polynomials.Utils;

namespace Polynomials;

public class FactorDecomposition<E>
{
    public Ring<E> Ring;
    public E Unit;
    public List<E> Factors;
    public List<int> Exponents;

    public FactorDecomposition(Ring<E> ring, E unit, List<E> factors, List<int> exponents)
    {
        Ring = ring;
        Unit = unit;
        Factors = factors;
        Exponents = exponents;
        if (!ring.IsUnit(unit))
            throw new ArgumentException();
        if (factors.Count != exponents.Count)
            throw new ArgumentException();
    }

    public FactorDecomposition(Ring<E> ring, E unit)
    {
        Ring = ring;
        Unit = unit;
        Factors = new List<E>();
        Exponents = new List<int>();
        if (!ring.IsUnit(unit))
            throw new ArgumentException();
    }


    public bool IsTrivial() => Factors.Count == 1;

    public virtual FactorDecomposition<E> SetUnit(E unit)
    {
        if (!IsUnit(unit))
            throw new ArgumentException("not a unit: " + unit);
        this.Unit = unit;
        return this;
    }

    public virtual bool IsUnit(E element)
    {
        return Ring.IsUnit(element);
    }

    public virtual void RaiseExponents(int val)
    {
        for (var i = Factors.Count - 1; i >= 0; --i)
            Exponents[i] *= val;
    }

    public virtual FactorDecomposition<E> AddUnit(E unit)
    {
        if (!IsUnit(unit))
            throw new ArgumentException();
        this.Unit = Ring.Multiply(this.Unit, unit);
        return this;
    }

    public virtual FactorDecomposition<E> AddUnit(E unit, int exponent)
    {
        if (!IsUnit(unit))
            throw new ArgumentException();
        if (Ring.IsOne(unit))
            return this;
        this.Unit = Ring.Multiply(this.Unit, Ring.Pow(unit, exponent));
        return this;
    }

    public virtual FactorDecomposition<E> AddFactor(E factor, int exponent)
    {
        if (IsUnit(factor))
            return AddUnit(factor, exponent);
        Factors.Add(factor);
        Exponents.Add(exponent);
        return this;
    }

    public virtual FactorDecomposition<E> AddAll(FactorDecomposition<E> other)
    {
        AddUnit(other.Unit);
        Factors.AddRange(other.Factors);
        Exponents.AddRange(other.Exponents);
        return this;
    }

    public virtual FactorDecomposition<E> AddNonUnitFactor(E factor, int exponent)
    {
        Factors.Add(factor);
        Exponents.Add(exponent);
        return this;
    }

    public virtual E MultiplyIgnoreExponents()
    {
        return Multiply0(true);
    }

    private E Multiply0(bool ignoreExponents)
    {
        var r = Ring.Copy(Unit);
        for (var i = 0; i < Factors.Count; i++)
        {
            var tmp = ignoreExponents ? Factors[i] : Ring.Pow(Factors[i], Exponents[i]);
            r = Ring.MultiplyMutable(r, tmp);
        }

        return r;
    }

    public virtual FactorDecomposition<E> Canonical()
    {
        var wr = Factors.ToArray();
        int[] ex = Exponents.ToArray();
        Array.Sort(wr, ex, Ring);
        Factors.Clear();
        Exponents.Clear();
        Factors.AddRange(wr);
        Exponents.AddRange(ex);
        return this;
    }

    public static FactorDecomposition<E> Empty(Ring<E> ring) => new FactorDecomposition<E>(ring, ring.GetOne());

    public static FactorDecomposition<E> Of(Ring<E> ring, E unit, List<E> factors, List<int> exponents)
    {
        if (factors.Count != exponents.Count)
            throw new ArgumentException();
        var r = Empty(ring).AddUnit(unit);
        for (var i = 0; i < factors.Count; i++)
            r.AddFactor(factors[i], exponents[i]);
        return r;
    }

    public static FactorDecomposition<E> Of(Ring<E> ring, IEnumerable<E> factors)
    {
        Dictionary<E, int> map = new Dictionary<E, int>();
        foreach (var e in factors)
        {
            if (!map.TryAdd(e, 1))
                map[e]++;
        }

        List<E> l = new List<E>();
        List<int> ex = new List<int>();
        map.ForEach(ab =>
        {
            l.Add(ab.Key);
            ex.Add(ab.Value);
        });
        return Of(ring, ring.GetOne(), l, ex);
    }

    public static FactorDecomposition<E> Of(Ring<E> ring, E factor)
    {
        return Of(ring, ring.GetOne(), [factor], [1]);
    }

    public virtual FactorDecomposition<R> MapTo<R>(Ring<R> othRing, Func<E, R> mapper)
    {
        return FactorDecomposition<R>.Of(othRing, mapper(Unit), Factors.Select(mapper).ToList(), Exponents);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Unit);
        for (int i = 0; i < Factors.Count; i++)
        {
            sb.Append(" * ");
            sb.Append(Factors[i]);
            if (Exponents[i] > 1)
            {
                sb.Append("^");
                sb.Append(Exponents[i]);
            }
        }

        return sb.ToString();
    }
}