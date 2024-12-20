using System.Collections.ObjectModel;

namespace Polynomials.Poly.Multivar;

interface MonomialSetView<E> : IEnumerable<Monomial<E>>
{
    IEnumerable<Monomial<E>> AscendingIterator();
    IEnumerable<Monomial<E>> DescendingIterator();

    IEnumerable<Monomial<E>> Iterator()
    {
        return AscendingIterator();
    }

    Monomial<E> First()
    {
        return AscendingIterator().First();
    }

    Monomial<E> Last()
    {
        return DescendingIterator().Last();
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