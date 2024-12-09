

using System.Collections.ObjectModel;

namespace Cc.Redberry.Rings.Poly.Multivar
{
    /// <summary>
    /// Sorted set of monomials -- basic underlying data structure of multivariate polynomials.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class MonomialSet<Term> : SortedDictionary<DegreeVector, Term>, MonomialSetView<Term>, ICloneable where Term : AMonomial<Term>
    {
        private static readonly long serialVersionUID = 1;
        public MonomialSet(IComparer<DegreeVector> comparator) : base(comparator)
        {
        }

        /// <summary>
        /// Constructs a new monomial set containing the same mappings and using the same ordering as the specified sorted
        /// map.  This method runs in linear time.
        /// </summary>
        /// <param name="m">the sorted map whose mappings are to be placed in this monomial set, and whose comparator is to be used
        ///          to sort this map</param>
        public MonomialSet(SortedDictionary<DegreeVector, Term> m) : base(m)
        {
        }


        public IEnumerable<Term> Iterator()
        {
            return Values;
        }

        /// <summary>
        /// Add monomial to this set
        /// </summary>
        /// <param name="term">monomial</param>
        /// <returns>this</returns>
        public Term Add(Term term)
        {
            base.Add(term, term);
            return term;
        }


        public IEnumerable<Term> AscendingIterator()
        {
            return Values;
        }


        public IEnumerable<Term> DescendingIterator()
        {
            return Values.Reverse();
        }

       
       
        public Collection<Term> Collection()
        {
            return new Collection<Term>(Values.ToList());
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
            return new MonomialSet<Term>(this);
        }


        public new IEnumerator<Term> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        public override int GetHashCode()
        {
            int h = 17;
            using (IEnumerator<KeyValuePair<DegreeVector, Term>> i = base.GetEnumerator())
            {
                while (i.MoveNext())
                    h += 13 + h * i.Current.Value.GetHashCode();
                return h;
            }
        }

       

        public int SkeletonHashCode()
        {
            int h = 17;
            using (IEnumerator<KeyValuePair<DegreeVector, Term>> i = base.GetEnumerator())
            {
                while (i.MoveNext())
                    h += 13 + h * i.Current.Key.GetHashCode();
                return h;
            }
        }
    }
}