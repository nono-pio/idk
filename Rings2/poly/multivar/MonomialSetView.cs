using System.Collections.ObjectModel;
using Cc.Redberry.Rings.Util;

namespace Cc.Redberry.Rings.Poly.Multivar
{
    /// <summary>
    /// Parent auxiliary interface for multivariate polynomials.
    /// </summary>
    /// <remarks>@since2.3</remarks>
    interface MonomialSetView<Term> : IEnumerable<Term>
    {
        IEnumerable<Term> AscendingIterator();
        IEnumerable<Term> DescendingIterator();
        IEnumerable<Term> Iterator()
        {
            return AscendingIterator();
        }

        Term First()
        {
            return AscendingIterator().First();
        }

        Term Last()
        {
            return DescendingIterator().Last();
        }

        Term Lt()
        {
            return DescendingIterator().Last();
        }

        int Size();
        /// <summary>
        /// Returns an array of degrees of all variables, so that is i-th element of the result is the polynomial degree
        /// (univariate) with respect to i-th variable
        /// </summary>
        /// <returns>array of degrees</returns>
        int[] Degrees();
        /// <summary>
        /// Returns an array of degrees of all variables, so that is i-th element of the result is the polynomial degree
        /// (univariate) with respect to i-th variable
        /// </summary>
        /// <returns>array of degrees</returns>
        /// <summary>
        /// Returns the sum of {@link #degrees()}
        /// </summary>
        /// <returns>sum of {@link #degrees()}</returns>
        int DegreeSum()
        {
            return ArraysUtil.Sum(Degrees());
        }

        /// <summary>
        /// Returns an array of degrees of all variables, so that is i-th element of the result is the polynomial degree
        /// (univariate) with respect to i-th variable
        /// </summary>
        /// <returns>array of degrees</returns>
        /// <summary>
        /// Returns the sum of {@link #degrees()}
        /// </summary>
        /// <returns>sum of {@link #degrees()}</returns>
        /// <summary>
        /// Collection view of all terms
        /// </summary>
        Collection<Term> Collection();
    }
}