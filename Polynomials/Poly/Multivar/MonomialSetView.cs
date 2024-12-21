using System.Collections.ObjectModel;

namespace Polynomials.Poly.Multivar;

interface MonomialSetView<E>
{
    IEnumerable<Monomial<E>> AscendingIterator();
    IEnumerable<Monomial<E>> DescendingIterator();

    IEnumerable<Monomial<E>> Iterator()
    {
        return AscendingIterator();
    }

    public Monomial<E> First()
    {
        return AscendingIterator().First();
    }

    public Monomial<E> Last()
    {
        return DescendingIterator().First();
    }

    Monomial<E> Lt()
    {
        return DescendingIterator().Last();
    }

    int Size();


    int[] Degrees();


    int DegreeSum()
    {
        return Degrees().Sum();
    }


    Collection<Monomial<E>> Collection();
}