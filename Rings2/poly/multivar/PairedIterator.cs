using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Poly.Multivar.RoundingMode;
using static Cc.Redberry.Rings.Poly.Multivar.Associativity;
using static Cc.Redberry.Rings.Poly.Multivar.Operator;
using static Cc.Redberry.Rings.Poly.Multivar.TokenType;
using static Cc.Redberry.Rings.Poly.Multivar.SystemInfo;

namespace Cc.Redberry.Rings.Poly.Multivar
{
    /// <summary>
    /// Iterator over a pair of polynomials
    /// </summary>
    public sealed class PairedIterator<Term1, Poly1, Term2, Poly2>
    {
        readonly Term1 zeroTerm1;
        readonly Term2 zeroTerm2;
        readonly Comparator<DegreeVector> ordering;
        readonly IEnumerator<Term1> aIterator;
        readonly IEnumerator<Term2> bIterator;
        public PairedIterator(Poly1 a, Poly2 b)
        {
            this.zeroTerm1 = a.monomialAlgebra.GetZeroTerm(a.nVariables);
            this.zeroTerm2 = b.monomialAlgebra.GetZeroTerm(b.nVariables);
            this.ordering = a.ordering;
            this.aIterator = a.Iterator();
            this.bIterator = b.Iterator();
        }

        public bool HasNext()
        {
            return aIterator.HasNext() || bIterator.HasNext() || aTermCached != null || bTermCached != null;
        }

        public Term1 aTerm = null;
        public Term2 bTerm = null;
        private Term1 aTermCached = null;
        private Term2 bTermCached = null;
        public void Advance()
        {
            if (aTermCached != null)
            {
                aTerm = aTermCached;
                aTermCached = null;
            }
            else
                aTerm = aIterator.HasNext() ? aIterator.Next() : zeroTerm1;
            if (bTermCached != null)
            {
                bTerm = bTermCached;
                bTermCached = null;
            }
            else
                bTerm = bIterator.HasNext() ? bIterator.Next() : zeroTerm2;
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
        }
    }
}