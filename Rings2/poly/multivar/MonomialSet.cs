using Java;
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
    /// Sorted set of monomials -- basic underlying data structure of multivariate polynomials.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class MonomialSet<Term> : TreeMap<DegreeVector, Term>, MonomialSetView<Term>, Iterable<Term>, Cloneable
    {
        private static readonly long serialVersionUID = 1;
        public MonomialSet(Comparator<TWildcardTodoDegreeVector> comparator) : base(comparator)
        {
        }

        /// <summary>
        /// Constructs a new monomial set containing the same mappings and using the same ordering as the specified sorted
        /// map.  This method runs in linear time.
        /// </summary>
        /// <param name="m">the sorted map whose mappings are to be placed in this monomial set, and whose comparator is to be used
        ///          to sort this map</param>
        public MonomialSet(SortedMap<DegreeVector, TWildcardTodoTerm> m) : base(m)
        {
        }

        /// <summary>
        /// Constructs a new monomial set containing the same mappings and using the same ordering as the specified sorted
        /// map.  This method runs in linear time.
        /// </summary>
        /// <param name="m">the sorted map whose mappings are to be placed in this monomial set, and whose comparator is to be used
        ///          to sort this map</param>
        public override IEnumerator<Term> Iterator()
        {
            return Values().Iterator();
        }

        /// <summary>
        /// Constructs a new monomial set containing the same mappings and using the same ordering as the specified sorted
        /// map.  This method runs in linear time.
        /// </summary>
        /// <param name="m">the sorted map whose mappings are to be placed in this monomial set, and whose comparator is to be used
        ///          to sort this map</param>
        /// <summary>
        /// Add monomial to this set
        /// </summary>
        /// <param name="term">monomial</param>
        /// <returns>this</returns>
        public Term Add(Term term)
        {
            return Put(term, term);
        }

        /// <summary>
        /// Constructs a new monomial set containing the same mappings and using the same ordering as the specified sorted
        /// map.  This method runs in linear time.
        /// </summary>
        /// <param name="m">the sorted map whose mappings are to be placed in this monomial set, and whose comparator is to be used
        ///          to sort this map</param>
        /// <summary>
        /// Add monomial to this set
        /// </summary>
        /// <param name="term">monomial</param>
        /// <returns>this</returns>
        /// <summary>
        /// First monomial in this set
        /// </summary>
        public override Term First()
        {
            return FirstEntry().GetValue();
        }

        /// <summary>
        /// Constructs a new monomial set containing the same mappings and using the same ordering as the specified sorted
        /// map.  This method runs in linear time.
        /// </summary>
        /// <param name="m">the sorted map whose mappings are to be placed in this monomial set, and whose comparator is to be used
        ///          to sort this map</param>
        /// <summary>
        /// Add monomial to this set
        /// </summary>
        /// <param name="term">monomial</param>
        /// <returns>this</returns>
        /// <summary>
        /// First monomial in this set
        /// </summary>
        /// <summary>
        /// Last monomial in this set
        /// </summary>
        public override Term Last()
        {
            return LastEntry().GetValue();
        }

        /// <summary>
        /// Constructs a new monomial set containing the same mappings and using the same ordering as the specified sorted
        /// map.  This method runs in linear time.
        /// </summary>
        /// <param name="m">the sorted map whose mappings are to be placed in this monomial set, and whose comparator is to be used
        ///          to sort this map</param>
        /// <summary>
        /// Add monomial to this set
        /// </summary>
        /// <param name="term">monomial</param>
        /// <returns>this</returns>
        /// <summary>
        /// First monomial in this set
        /// </summary>
        /// <summary>
        /// Last monomial in this set
        /// </summary>
        public override IEnumerator<Term> AscendingIterator()
        {
            return Values().Iterator();
        }

        /// <summary>
        /// Constructs a new monomial set containing the same mappings and using the same ordering as the specified sorted
        /// map.  This method runs in linear time.
        /// </summary>
        /// <param name="m">the sorted map whose mappings are to be placed in this monomial set, and whose comparator is to be used
        ///          to sort this map</param>
        /// <summary>
        /// Add monomial to this set
        /// </summary>
        /// <param name="term">monomial</param>
        /// <returns>this</returns>
        /// <summary>
        /// First monomial in this set
        /// </summary>
        /// <summary>
        /// Last monomial in this set
        /// </summary>
        public override IEnumerator<Term> DescendingIterator()
        {
            return DescendingMap().Values().Iterator();
        }

        /// <summary>
        /// Constructs a new monomial set containing the same mappings and using the same ordering as the specified sorted
        /// map.  This method runs in linear time.
        /// </summary>
        /// <param name="m">the sorted map whose mappings are to be placed in this monomial set, and whose comparator is to be used
        ///          to sort this map</param>
        /// <summary>
        /// Add monomial to this set
        /// </summary>
        /// <param name="term">monomial</param>
        /// <returns>this</returns>
        /// <summary>
        /// First monomial in this set
        /// </summary>
        /// <summary>
        /// Last monomial in this set
        /// </summary>
        public override Collection<Term> Collection()
        {
            return Values();
        }

        /// <summary>
        /// Constructs a new monomial set containing the same mappings and using the same ordering as the specified sorted
        /// map.  This method runs in linear time.
        /// </summary>
        /// <param name="m">the sorted map whose mappings are to be placed in this monomial set, and whose comparator is to be used
        ///          to sort this map</param>
        /// <summary>
        /// Add monomial to this set
        /// </summary>
        /// <param name="term">monomial</param>
        /// <returns>this</returns>
        /// <summary>
        /// First monomial in this set
        /// </summary>
        /// <summary>
        /// Last monomial in this set
        /// </summary>
        public override int[] Degrees()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Constructs a new monomial set containing the same mappings and using the same ordering as the specified sorted
        /// map.  This method runs in linear time.
        /// </summary>
        /// <param name="m">the sorted map whose mappings are to be placed in this monomial set, and whose comparator is to be used
        ///          to sort this map</param>
        /// <summary>
        /// Add monomial to this set
        /// </summary>
        /// <param name="term">monomial</param>
        /// <returns>this</returns>
        /// <summary>
        /// First monomial in this set
        /// </summary>
        /// <summary>
        /// Last monomial in this set
        /// </summary>
        public override MonomialSet<Term> Clone()
        {
            return (MonomialSet<Term>)base.Clone();
        }

        /// <summary>
        /// Constructs a new monomial set containing the same mappings and using the same ordering as the specified sorted
        /// map.  This method runs in linear time.
        /// </summary>
        /// <param name="m">the sorted map whose mappings are to be placed in this monomial set, and whose comparator is to be used
        ///          to sort this map</param>
        /// <summary>
        /// Add monomial to this set
        /// </summary>
        /// <param name="term">monomial</param>
        /// <returns>this</returns>
        /// <summary>
        /// First monomial in this set
        /// </summary>
        /// <summary>
        /// Last monomial in this set
        /// </summary>
        public override int GetHashCode()
        {
            int h = 17;
            Iterator<Map.Entry<DegreeVector, Term>> i = EntrySet().Iterator();
            while (i.HasNext())
                h += 13 + h * i.Next().GetValue().GetHashCode();
            return h;
        }

        /// <summary>
        /// Constructs a new monomial set containing the same mappings and using the same ordering as the specified sorted
        /// map.  This method runs in linear time.
        /// </summary>
        /// <param name="m">the sorted map whose mappings are to be placed in this monomial set, and whose comparator is to be used
        ///          to sort this map</param>
        /// <summary>
        /// Add monomial to this set
        /// </summary>
        /// <param name="term">monomial</param>
        /// <returns>this</returns>
        /// <summary>
        /// First monomial in this set
        /// </summary>
        /// <summary>
        /// Last monomial in this set
        /// </summary>
        public int SkeletonHashCode()
        {
            int h = 17;
            Iterator<Map.Entry<DegreeVector, Term>> i = EntrySet().Iterator();
            while (i.HasNext())
                h += 13 + h * i.Next().GetKey().Dv().GetHashCode();
            return h;
        }
    }
}