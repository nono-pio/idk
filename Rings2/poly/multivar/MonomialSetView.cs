using Cc.Redberry.Rings.Util;
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
    /// Parent auxiliary interface for multivariate polynomials.
    /// </summary>
    /// <remarks>@since2.3</remarks>
    interface MonomialSetView<Term> : Iterable<Term>
    {
        IEnumerator<Term> AscendingIterator();
        IEnumerator<Term> DescendingIterator();
        IEnumerator<Term> Iterator()
        {
            return AscendingIterator();
        }

        Term First()
        {
            return AscendingIterator().Next();
        }

        Term Last()
        {
            return DescendingIterator().Next();
        }

        Term Lt()
        {
            return DescendingIterator().Next();
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