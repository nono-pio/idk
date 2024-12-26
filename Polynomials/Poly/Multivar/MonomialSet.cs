using System.Collections.ObjectModel;

namespace Polynomials.Poly.Multivar;

public sealed class MonomialSet<E> : SortedDictionary<DegreeVector, Monomial<E>>, MonomialSetView<E>, ICloneable
{

    public MonomialSet(IComparer<DegreeVector> comparator) : base(comparator)
    {
    }


    public MonomialSet(SortedDictionary<DegreeVector, Monomial<E>> m) : base(m, m.Comparer)
    {
    }


    public IEnumerable<Monomial<E>> Iterator()
    {
        return Values;
    }


    public Monomial<E> Add(Monomial<E> term)
    {
        base.Add(term, term);
        return term;
    }


    public IEnumerable<Monomial<E>> AscendingIterator()
    {
        return Values;
    }


    public IEnumerable<Monomial<E>> DescendingIterator()
    {
        return Values.Reverse();
    }


    public Collection<Monomial<E>> Collection()
    {
        return new Collection<Monomial<E>>(Values.ToList());
    }


    public int Size()
    {
        return Count;
    }

    public int[] Degrees()
    {
        throw new NotSupportedException();
    }


    public object Clone()
    {
        return new MonomialSet<E>(this);
    }

    public IEnumerable<KeyValuePair<DegreeVector, Monomial<E>>> EntryIterator()
    {
        return this;
    }

    public new IEnumerator<Monomial<E>> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    public override int GetHashCode()
    {
        int h = 17;
        using (IEnumerator<KeyValuePair<DegreeVector, Monomial<E>>> i = base.GetEnumerator())
        {
            while (i.MoveNext())
                h += 13 + h * i.Current.Value.GetHashCode();
            return h;
        }
    }


    public int SkeletonHashCode()
    {
        int h = 17;
        using (IEnumerator<KeyValuePair<DegreeVector, Monomial<E>>> i = base.GetEnumerator())
        {
            while (i.MoveNext())
                h += 13 + h * i.Current.Key.GetHashCode();
            return h;
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        var set = (MonomialSet<E>)obj;

        return Enumerable.SequenceEqual(this.Iterator(), set.Iterator());
    }
}