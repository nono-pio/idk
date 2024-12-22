namespace Polynomials.Poly.Multivar;

public sealed class PairedIterator<E1, E2>
{
    public readonly Monomial<E1> zeroTerm1;
    public readonly Monomial<E2> zeroTerm2;
    public readonly IComparer<DegreeVector> ordering;
    public readonly IEnumerator<Monomial<E1>> aIterator;
    public readonly IEnumerator<Monomial<E2>> bIterator;

    public PairedIterator(MultivariatePolynomial<E1> a, MultivariatePolynomial<E2> b)
    {
        this.zeroTerm1 = a.monomialAlgebra.GetZeroTerm(a.nVariables);
        this.zeroTerm2 = b.monomialAlgebra.GetZeroTerm(b.nVariables);
        this.ordering = a.ordering;
        this.aIterator = a.Iterator().GetEnumerator();
        this.bIterator = b.Iterator().GetEnumerator();
    }
    
    public Monomial<E1>? aTerm = null;
    public Monomial<E2>? bTerm = null;
    private Monomial<E1>? aTermCached = null;
    private Monomial<E2>? bTermCached = null;
    
    public bool MoveNext()
    {
        var hasNext = false;
        
        
        if (aTermCached != null)
        {
            aTerm = aTermCached;
            aTermCached = null;
            hasNext = true;
        }
        else
        {
            if (aIterator.MoveNext())
            {
                aTerm = aIterator.Current;
                hasNext = true;
            }

            aTerm = zeroTerm1;
        }

        if (bTermCached != null)
        {
            bTerm = bTermCached;
            bTermCached = null;
            hasNext = true;
        }
        else
        {
            if (bIterator.MoveNext())
            {
                bTerm = bIterator.Current;
                hasNext = true;
            }

            bTerm = zeroTerm2;
        }

        int c = ordering.Compare(aTerm, bTerm);
        if (c < 0 && aTerm != zeroTerm1)
        {
            bTermCached = bTerm;
            bTerm = zeroTerm2;
        }
        else if (c > 0 && bTerm != zeroTerm2)
        {
            aTermCached = aTerm;
            aTerm = zeroTerm1;
        }
        
        return hasNext;
    }
}