using System.Reflection;
using Cc.Redberry.Rings.Poly.Multivar;

namespace Cc.Redberry.Rings.Poly.Univar
{
    /// <summary>
    /// Parent interface for univariate polynomials. Dense representation (array of coefficients) is used to hold univariate
    /// polynomials. Positional operations treat index so that i-th coefficient corresponds to {@code x^i} monomial.
    /// </summary>
    /// <param name="<Poly>">the type of polynomial (self type)</param>
    /// <remarks>@since1.0</remarks>
    public interface IUnivariatePolynomial<Poly> : IPolynomial<Poly> where Poly : IUnivariatePolynomial<Poly>
    {
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        new int Size()
        {
            return Degree() + 1;
        }

        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        int NNonZeroTerms()
        {
            int c = 0;
            for (int i = Degree(); i >= 0; --i)
                if (!IsZeroAt(i))
                    ++c;
            return c;
        }

        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        bool IsZeroAt(int i);
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        new bool IsZeroCC()
        {
            return IsZeroAt(0);
        }

        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        Poly SetZero(int i);
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        Poly SetFrom(int indexInThis, Poly poly, int indexInPoly);
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        Poly GetAsPoly(int i);
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        HashSet<int> Exponents()
        {
            HashSet<int> degrees = new HashSet<int>();
            for (int i = Degree(); i >= 0; --i)
                if (!IsZeroAt(i))
                    degrees.Add(i);
            return degrees;
        }

        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        int FirstNonZeroCoefficientPosition();
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        /// <summary>
        /// Returns the quotient {@code this / x^offset}, it is polynomial with coefficient list formed by shifting
        /// coefficients of {@code this} to the left by {@code offset}.
        /// </summary>
        /// <param name="offset">shift amount</param>
        /// <returns>the quotient {@code this / x^offset}</returns>
        Poly ShiftLeft(int offset);
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        /// <summary>
        /// Returns the quotient {@code this / x^offset}, it is polynomial with coefficient list formed by shifting
        /// coefficients of {@code this} to the left by {@code offset}.
        /// </summary>
        /// <param name="offset">shift amount</param>
        /// <returns>the quotient {@code this / x^offset}</returns>
        /// <summary>
        /// Multiplies {@code this} by the {@code x^offset}.
        /// </summary>
        /// <param name="offset">monomial exponent</param>
        /// <returns>{@code this * x^offset}</returns>
        Poly ShiftRight(int offset);
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        /// <summary>
        /// Returns the quotient {@code this / x^offset}, it is polynomial with coefficient list formed by shifting
        /// coefficients of {@code this} to the left by {@code offset}.
        /// </summary>
        /// <param name="offset">shift amount</param>
        /// <returns>the quotient {@code this / x^offset}</returns>
        /// <summary>
        /// Multiplies {@code this} by the {@code x^offset}.
        /// </summary>
        /// <param name="offset">monomial exponent</param>
        /// <returns>{@code this * x^offset}</returns>
        /// <summary>
        /// Returns the remainder {@code this rem x^(newDegree + 1)}, it is polynomial formed by coefficients of this from
        /// zero to {@code newDegree} (both inclusive)
        /// </summary>
        /// <param name="newDegree">new degree</param>
        /// <returns>remainder {@code this rem x^(newDegree + 1)}</returns>
        Poly Truncate(int newDegree);
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        /// <summary>
        /// Returns the quotient {@code this / x^offset}, it is polynomial with coefficient list formed by shifting
        /// coefficients of {@code this} to the left by {@code offset}.
        /// </summary>
        /// <param name="offset">shift amount</param>
        /// <returns>the quotient {@code this / x^offset}</returns>
        /// <summary>
        /// Multiplies {@code this} by the {@code x^offset}.
        /// </summary>
        /// <param name="offset">monomial exponent</param>
        /// <returns>{@code this * x^offset}</returns>
        /// <summary>
        /// Returns the remainder {@code this rem x^(newDegree + 1)}, it is polynomial formed by coefficients of this from
        /// zero to {@code newDegree} (both inclusive)
        /// </summary>
        /// <param name="newDegree">new degree</param>
        /// <returns>remainder {@code this rem x^(newDegree + 1)}</returns>
        /// <summary>
        /// Creates polynomial formed from the coefficients of this starting from {@code from} (inclusive) to {@code to}
        /// (exclusive)
        /// </summary>
        /// <param name="from">the initial index of the range to be copied, inclusive</param>
        /// <param name="to">the final index of the range to be copied, exclusive.</param>
        /// <returns>polynomial formed from the range of coefficients of this</returns>
        Poly GetRange(int from, int to);
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        /// <summary>
        /// Returns the quotient {@code this / x^offset}, it is polynomial with coefficient list formed by shifting
        /// coefficients of {@code this} to the left by {@code offset}.
        /// </summary>
        /// <param name="offset">shift amount</param>
        /// <returns>the quotient {@code this / x^offset}</returns>
        /// <summary>
        /// Multiplies {@code this} by the {@code x^offset}.
        /// </summary>
        /// <param name="offset">monomial exponent</param>
        /// <returns>{@code this * x^offset}</returns>
        /// <summary>
        /// Returns the remainder {@code this rem x^(newDegree + 1)}, it is polynomial formed by coefficients of this from
        /// zero to {@code newDegree} (both inclusive)
        /// </summary>
        /// <param name="newDegree">new degree</param>
        /// <returns>remainder {@code this rem x^(newDegree + 1)}</returns>
        /// <summary>
        /// Creates polynomial formed from the coefficients of this starting from {@code from} (inclusive) to {@code to}
        /// (exclusive)
        /// </summary>
        /// <param name="from">the initial index of the range to be copied, inclusive</param>
        /// <param name="to">the final index of the range to be copied, exclusive.</param>
        /// <returns>polynomial formed from the range of coefficients of this</returns>
        /// <summary>
        /// Reverses the coefficients of this
        /// </summary>
        /// <returns>reversed polynomial</returns>
        Poly Reverse();
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        /// <summary>
        /// Returns the quotient {@code this / x^offset}, it is polynomial with coefficient list formed by shifting
        /// coefficients of {@code this} to the left by {@code offset}.
        /// </summary>
        /// <param name="offset">shift amount</param>
        /// <returns>the quotient {@code this / x^offset}</returns>
        /// <summary>
        /// Multiplies {@code this} by the {@code x^offset}.
        /// </summary>
        /// <param name="offset">monomial exponent</param>
        /// <returns>{@code this * x^offset}</returns>
        /// <summary>
        /// Returns the remainder {@code this rem x^(newDegree + 1)}, it is polynomial formed by coefficients of this from
        /// zero to {@code newDegree} (both inclusive)
        /// </summary>
        /// <param name="newDegree">new degree</param>
        /// <returns>remainder {@code this rem x^(newDegree + 1)}</returns>
        /// <summary>
        /// Creates polynomial formed from the coefficients of this starting from {@code from} (inclusive) to {@code to}
        /// (exclusive)
        /// </summary>
        /// <param name="from">the initial index of the range to be copied, inclusive</param>
        /// <param name="to">the final index of the range to be copied, exclusive.</param>
        /// <returns>polynomial formed from the range of coefficients of this</returns>
        /// <summary>
        /// Reverses the coefficients of this
        /// </summary>
        /// <returns>reversed polynomial</returns>
        /// <summary>
        /// Creates new monomial {@code x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="degree">monomial degree</param>
        /// <returns>new monomial {@code coefficient * x^degree}</returns>
        Poly CreateMonomial(int degree);
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        /// <summary>
        /// Returns the quotient {@code this / x^offset}, it is polynomial with coefficient list formed by shifting
        /// coefficients of {@code this} to the left by {@code offset}.
        /// </summary>
        /// <param name="offset">shift amount</param>
        /// <returns>the quotient {@code this / x^offset}</returns>
        /// <summary>
        /// Multiplies {@code this} by the {@code x^offset}.
        /// </summary>
        /// <param name="offset">monomial exponent</param>
        /// <returns>{@code this * x^offset}</returns>
        /// <summary>
        /// Returns the remainder {@code this rem x^(newDegree + 1)}, it is polynomial formed by coefficients of this from
        /// zero to {@code newDegree} (both inclusive)
        /// </summary>
        /// <param name="newDegree">new degree</param>
        /// <returns>remainder {@code this rem x^(newDegree + 1)}</returns>
        /// <summary>
        /// Creates polynomial formed from the coefficients of this starting from {@code from} (inclusive) to {@code to}
        /// (exclusive)
        /// </summary>
        /// <param name="from">the initial index of the range to be copied, inclusive</param>
        /// <param name="to">the final index of the range to be copied, exclusive.</param>
        /// <returns>polynomial formed from the range of coefficients of this</returns>
        /// <summary>
        /// Reverses the coefficients of this
        /// </summary>
        /// <returns>reversed polynomial</returns>
        /// <summary>
        /// Creates new monomial {@code x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="degree">monomial degree</param>
        /// <returns>new monomial {@code coefficient * x^degree}</returns>
        /// <summary>
        /// Returns the formal derivative of this poly (new instance, so the content of this is not changed)
        /// </summary>
        /// <returns>the formal derivative</returns>
        Poly Derivative();
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        /// <summary>
        /// Returns the quotient {@code this / x^offset}, it is polynomial with coefficient list formed by shifting
        /// coefficients of {@code this} to the left by {@code offset}.
        /// </summary>
        /// <param name="offset">shift amount</param>
        /// <returns>the quotient {@code this / x^offset}</returns>
        /// <summary>
        /// Multiplies {@code this} by the {@code x^offset}.
        /// </summary>
        /// <param name="offset">monomial exponent</param>
        /// <returns>{@code this * x^offset}</returns>
        /// <summary>
        /// Returns the remainder {@code this rem x^(newDegree + 1)}, it is polynomial formed by coefficients of this from
        /// zero to {@code newDegree} (both inclusive)
        /// </summary>
        /// <param name="newDegree">new degree</param>
        /// <returns>remainder {@code this rem x^(newDegree + 1)}</returns>
        /// <summary>
        /// Creates polynomial formed from the coefficients of this starting from {@code from} (inclusive) to {@code to}
        /// (exclusive)
        /// </summary>
        /// <param name="from">the initial index of the range to be copied, inclusive</param>
        /// <param name="to">the final index of the range to be copied, exclusive.</param>
        /// <returns>polynomial formed from the range of coefficients of this</returns>
        /// <summary>
        /// Reverses the coefficients of this
        /// </summary>
        /// <returns>reversed polynomial</returns>
        /// <summary>
        /// Creates new monomial {@code x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="degree">monomial degree</param>
        /// <returns>new monomial {@code coefficient * x^degree}</returns>
        /// <summary>
        /// Returns the formal derivative of this poly (new instance, so the content of this is not changed)
        /// </summary>
        /// <returns>the formal derivative</returns>
        Poly Clone();
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        /// <summary>
        /// Returns the quotient {@code this / x^offset}, it is polynomial with coefficient list formed by shifting
        /// coefficients of {@code this} to the left by {@code offset}.
        /// </summary>
        /// <param name="offset">shift amount</param>
        /// <returns>the quotient {@code this / x^offset}</returns>
        /// <summary>
        /// Multiplies {@code this} by the {@code x^offset}.
        /// </summary>
        /// <param name="offset">monomial exponent</param>
        /// <returns>{@code this * x^offset}</returns>
        /// <summary>
        /// Returns the remainder {@code this rem x^(newDegree + 1)}, it is polynomial formed by coefficients of this from
        /// zero to {@code newDegree} (both inclusive)
        /// </summary>
        /// <param name="newDegree">new degree</param>
        /// <returns>remainder {@code this rem x^(newDegree + 1)}</returns>
        /// <summary>
        /// Creates polynomial formed from the coefficients of this starting from {@code from} (inclusive) to {@code to}
        /// (exclusive)
        /// </summary>
        /// <param name="from">the initial index of the range to be copied, inclusive</param>
        /// <param name="to">the final index of the range to be copied, exclusive.</param>
        /// <returns>polynomial formed from the range of coefficients of this</returns>
        /// <summary>
        /// Reverses the coefficients of this
        /// </summary>
        /// <returns>reversed polynomial</returns>
        /// <summary>
        /// Creates new monomial {@code x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="degree">monomial degree</param>
        /// <returns>new monomial {@code coefficient * x^degree}</returns>
        /// <summary>
        /// Returns the formal derivative of this poly (new instance, so the content of this is not changed)
        /// </summary>
        /// <returns>the formal derivative</returns>
        /// <summary>
        /// Sets the content of this with {@code oth} and destroys oth
        /// </summary>
        /// <param name="oth">the polynomial (will be destroyed)</param>
        /// <returns>this := oth</returns>
        Poly SetAndDestroy(Poly oth);
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        /// <summary>
        /// Returns the quotient {@code this / x^offset}, it is polynomial with coefficient list formed by shifting
        /// coefficients of {@code this} to the left by {@code offset}.
        /// </summary>
        /// <param name="offset">shift amount</param>
        /// <returns>the quotient {@code this / x^offset}</returns>
        /// <summary>
        /// Multiplies {@code this} by the {@code x^offset}.
        /// </summary>
        /// <param name="offset">monomial exponent</param>
        /// <returns>{@code this * x^offset}</returns>
        /// <summary>
        /// Returns the remainder {@code this rem x^(newDegree + 1)}, it is polynomial formed by coefficients of this from
        /// zero to {@code newDegree} (both inclusive)
        /// </summary>
        /// <param name="newDegree">new degree</param>
        /// <returns>remainder {@code this rem x^(newDegree + 1)}</returns>
        /// <summary>
        /// Creates polynomial formed from the coefficients of this starting from {@code from} (inclusive) to {@code to}
        /// (exclusive)
        /// </summary>
        /// <param name="from">the initial index of the range to be copied, inclusive</param>
        /// <param name="to">the final index of the range to be copied, exclusive.</param>
        /// <returns>polynomial formed from the range of coefficients of this</returns>
        /// <summary>
        /// Reverses the coefficients of this
        /// </summary>
        /// <returns>reversed polynomial</returns>
        /// <summary>
        /// Creates new monomial {@code x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="degree">monomial degree</param>
        /// <returns>new monomial {@code coefficient * x^degree}</returns>
        /// <summary>
        /// Returns the formal derivative of this poly (new instance, so the content of this is not changed)
        /// </summary>
        /// <returns>the formal derivative</returns>
        /// <summary>
        /// Sets the content of this with {@code oth} and destroys oth
        /// </summary>
        /// <param name="oth">the polynomial (will be destroyed)</param>
        /// <returns>this := oth</returns>
        /// <summary>
        /// Calculates the composition of this(oth) (new instance, so the content of this is not changed))
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        Poly Composition(Poly value);
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        /// <summary>
        /// Returns the quotient {@code this / x^offset}, it is polynomial with coefficient list formed by shifting
        /// coefficients of {@code this} to the left by {@code offset}.
        /// </summary>
        /// <param name="offset">shift amount</param>
        /// <returns>the quotient {@code this / x^offset}</returns>
        /// <summary>
        /// Multiplies {@code this} by the {@code x^offset}.
        /// </summary>
        /// <param name="offset">monomial exponent</param>
        /// <returns>{@code this * x^offset}</returns>
        /// <summary>
        /// Returns the remainder {@code this rem x^(newDegree + 1)}, it is polynomial formed by coefficients of this from
        /// zero to {@code newDegree} (both inclusive)
        /// </summary>
        /// <param name="newDegree">new degree</param>
        /// <returns>remainder {@code this rem x^(newDegree + 1)}</returns>
        /// <summary>
        /// Creates polynomial formed from the coefficients of this starting from {@code from} (inclusive) to {@code to}
        /// (exclusive)
        /// </summary>
        /// <param name="from">the initial index of the range to be copied, inclusive</param>
        /// <param name="to">the final index of the range to be copied, exclusive.</param>
        /// <returns>polynomial formed from the range of coefficients of this</returns>
        /// <summary>
        /// Reverses the coefficients of this
        /// </summary>
        /// <returns>reversed polynomial</returns>
        /// <summary>
        /// Creates new monomial {@code x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="degree">monomial degree</param>
        /// <returns>new monomial {@code coefficient * x^degree}</returns>
        /// <summary>
        /// Returns the formal derivative of this poly (new instance, so the content of this is not changed)
        /// </summary>
        /// <returns>the formal derivative</returns>
        /// <summary>
        /// Sets the content of this with {@code oth} and destroys oth
        /// </summary>
        /// <param name="oth">the polynomial (will be destroyed)</param>
        /// <returns>this := oth</returns>
        /// <summary>
        /// Calculates the composition of this(oth) (new instance, so the content of this is not changed))
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Calculates the composition of this(oth) (new instance, so the content of this is not changed))
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        Poly Composition(Ring<Poly> ring, Poly value)
        {
            if (value.IsOne())
                return ring.ValueOf(this.Clone());
            if (value.IsZero())
                return CcAsPoly();
            Poly result = ring.GetZero();
            for (int i = Degree(); i >= 0; --i)
                result = ring.Add(ring.Multiply(result, value), GetAsPoly(i));
            return result;
        }

        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        /// <summary>
        /// Returns the quotient {@code this / x^offset}, it is polynomial with coefficient list formed by shifting
        /// coefficients of {@code this} to the left by {@code offset}.
        /// </summary>
        /// <param name="offset">shift amount</param>
        /// <returns>the quotient {@code this / x^offset}</returns>
        /// <summary>
        /// Multiplies {@code this} by the {@code x^offset}.
        /// </summary>
        /// <param name="offset">monomial exponent</param>
        /// <returns>{@code this * x^offset}</returns>
        /// <summary>
        /// Returns the remainder {@code this rem x^(newDegree + 1)}, it is polynomial formed by coefficients of this from
        /// zero to {@code newDegree} (both inclusive)
        /// </summary>
        /// <param name="newDegree">new degree</param>
        /// <returns>remainder {@code this rem x^(newDegree + 1)}</returns>
        /// <summary>
        /// Creates polynomial formed from the coefficients of this starting from {@code from} (inclusive) to {@code to}
        /// (exclusive)
        /// </summary>
        /// <param name="from">the initial index of the range to be copied, inclusive</param>
        /// <param name="to">the final index of the range to be copied, exclusive.</param>
        /// <returns>polynomial formed from the range of coefficients of this</returns>
        /// <summary>
        /// Reverses the coefficients of this
        /// </summary>
        /// <returns>reversed polynomial</returns>
        /// <summary>
        /// Creates new monomial {@code x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="degree">monomial degree</param>
        /// <returns>new monomial {@code coefficient * x^degree}</returns>
        /// <summary>
        /// Returns the formal derivative of this poly (new instance, so the content of this is not changed)
        /// </summary>
        /// <returns>the formal derivative</returns>
        /// <summary>
        /// Sets the content of this with {@code oth} and destroys oth
        /// </summary>
        /// <param name="oth">the polynomial (will be destroyed)</param>
        /// <returns>this := oth</returns>
        /// <summary>
        /// Calculates the composition of this(oth) (new instance, so the content of this is not changed))
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Calculates the composition of this(oth) (new instance, so the content of this is not changed))
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Stream polynomial coefficients as constant polynomials
        /// </summary>
        IEnumerable<Poly> StreamAsPolys();
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        /// <summary>
        /// Returns the quotient {@code this / x^offset}, it is polynomial with coefficient list formed by shifting
        /// coefficients of {@code this} to the left by {@code offset}.
        /// </summary>
        /// <param name="offset">shift amount</param>
        /// <returns>the quotient {@code this / x^offset}</returns>
        /// <summary>
        /// Multiplies {@code this} by the {@code x^offset}.
        /// </summary>
        /// <param name="offset">monomial exponent</param>
        /// <returns>{@code this * x^offset}</returns>
        /// <summary>
        /// Returns the remainder {@code this rem x^(newDegree + 1)}, it is polynomial formed by coefficients of this from
        /// zero to {@code newDegree} (both inclusive)
        /// </summary>
        /// <param name="newDegree">new degree</param>
        /// <returns>remainder {@code this rem x^(newDegree + 1)}</returns>
        /// <summary>
        /// Creates polynomial formed from the coefficients of this starting from {@code from} (inclusive) to {@code to}
        /// (exclusive)
        /// </summary>
        /// <param name="from">the initial index of the range to be copied, inclusive</param>
        /// <param name="to">the final index of the range to be copied, exclusive.</param>
        /// <returns>polynomial formed from the range of coefficients of this</returns>
        /// <summary>
        /// Reverses the coefficients of this
        /// </summary>
        /// <returns>reversed polynomial</returns>
        /// <summary>
        /// Creates new monomial {@code x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="degree">monomial degree</param>
        /// <returns>new monomial {@code coefficient * x^degree}</returns>
        /// <summary>
        /// Returns the formal derivative of this poly (new instance, so the content of this is not changed)
        /// </summary>
        /// <returns>the formal derivative</returns>
        /// <summary>
        /// Sets the content of this with {@code oth} and destroys oth
        /// </summary>
        /// <param name="oth">the polynomial (will be destroyed)</param>
        /// <returns>this := oth</returns>
        /// <summary>
        /// Calculates the composition of this(oth) (new instance, so the content of this is not changed))
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Calculates the composition of this(oth) (new instance, so the content of this is not changed))
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Stream polynomial coefficients as constant polynomials
        /// </summary>
        UnivariatePolynomial<E> MapCoefficientsAsPolys<E>(Ring<E> ring, Func<Poly, E> mapper)
        {
            return new UnivariatePolynomial<E>(ring, StreamAsPolys().Select(mapper).ToArray());
        }

        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        /// <summary>
        /// Returns the quotient {@code this / x^offset}, it is polynomial with coefficient list formed by shifting
        /// coefficients of {@code this} to the left by {@code offset}.
        /// </summary>
        /// <param name="offset">shift amount</param>
        /// <returns>the quotient {@code this / x^offset}</returns>
        /// <summary>
        /// Multiplies {@code this} by the {@code x^offset}.
        /// </summary>
        /// <param name="offset">monomial exponent</param>
        /// <returns>{@code this * x^offset}</returns>
        /// <summary>
        /// Returns the remainder {@code this rem x^(newDegree + 1)}, it is polynomial formed by coefficients of this from
        /// zero to {@code newDegree} (both inclusive)
        /// </summary>
        /// <param name="newDegree">new degree</param>
        /// <returns>remainder {@code this rem x^(newDegree + 1)}</returns>
        /// <summary>
        /// Creates polynomial formed from the coefficients of this starting from {@code from} (inclusive) to {@code to}
        /// (exclusive)
        /// </summary>
        /// <param name="from">the initial index of the range to be copied, inclusive</param>
        /// <param name="to">the final index of the range to be copied, exclusive.</param>
        /// <returns>polynomial formed from the range of coefficients of this</returns>
        /// <summary>
        /// Reverses the coefficients of this
        /// </summary>
        /// <returns>reversed polynomial</returns>
        /// <summary>
        /// Creates new monomial {@code x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="degree">monomial degree</param>
        /// <returns>new monomial {@code coefficient * x^degree}</returns>
        /// <summary>
        /// Returns the formal derivative of this poly (new instance, so the content of this is not changed)
        /// </summary>
        /// <returns>the formal derivative</returns>
        /// <summary>
        /// Sets the content of this with {@code oth} and destroys oth
        /// </summary>
        /// <param name="oth">the polynomial (will be destroyed)</param>
        /// <returns>this := oth</returns>
        /// <summary>
        /// Calculates the composition of this(oth) (new instance, so the content of this is not changed))
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Calculates the composition of this(oth) (new instance, so the content of this is not changed))
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Stream polynomial coefficients as constant polynomials
        /// </summary>
        /// <summary>
        /// Calculates the composition of this(oth)
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        AMultivariatePolynomial<Term, MPoly> Composition<Term, MPoly>(AMultivariatePolynomial<Term, MPoly> value);
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        /// <summary>
        /// Returns the quotient {@code this / x^offset}, it is polynomial with coefficient list formed by shifting
        /// coefficients of {@code this} to the left by {@code offset}.
        /// </summary>
        /// <param name="offset">shift amount</param>
        /// <returns>the quotient {@code this / x^offset}</returns>
        /// <summary>
        /// Multiplies {@code this} by the {@code x^offset}.
        /// </summary>
        /// <param name="offset">monomial exponent</param>
        /// <returns>{@code this * x^offset}</returns>
        /// <summary>
        /// Returns the remainder {@code this rem x^(newDegree + 1)}, it is polynomial formed by coefficients of this from
        /// zero to {@code newDegree} (both inclusive)
        /// </summary>
        /// <param name="newDegree">new degree</param>
        /// <returns>remainder {@code this rem x^(newDegree + 1)}</returns>
        /// <summary>
        /// Creates polynomial formed from the coefficients of this starting from {@code from} (inclusive) to {@code to}
        /// (exclusive)
        /// </summary>
        /// <param name="from">the initial index of the range to be copied, inclusive</param>
        /// <param name="to">the final index of the range to be copied, exclusive.</param>
        /// <returns>polynomial formed from the range of coefficients of this</returns>
        /// <summary>
        /// Reverses the coefficients of this
        /// </summary>
        /// <returns>reversed polynomial</returns>
        /// <summary>
        /// Creates new monomial {@code x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="degree">monomial degree</param>
        /// <returns>new monomial {@code coefficient * x^degree}</returns>
        /// <summary>
        /// Returns the formal derivative of this poly (new instance, so the content of this is not changed)
        /// </summary>
        /// <returns>the formal derivative</returns>
        /// <summary>
        /// Sets the content of this with {@code oth} and destroys oth
        /// </summary>
        /// <param name="oth">the polynomial (will be destroyed)</param>
        /// <returns>this := oth</returns>
        /// <summary>
        /// Calculates the composition of this(oth) (new instance, so the content of this is not changed))
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Calculates the composition of this(oth) (new instance, so the content of this is not changed))
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Stream polynomial coefficients as constant polynomials
        /// </summary>
        /// <summary>
        /// Calculates the composition of this(oth)
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Convert to multivariate polynomial
        /// </summary>
        AMultivariatePolynomial<Term, MPoly> AsMultivariate<Term, MPoly>(IComparer<DegreeVector> ordering);
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        /// <summary>
        /// Returns the quotient {@code this / x^offset}, it is polynomial with coefficient list formed by shifting
        /// coefficients of {@code this} to the left by {@code offset}.
        /// </summary>
        /// <param name="offset">shift amount</param>
        /// <returns>the quotient {@code this / x^offset}</returns>
        /// <summary>
        /// Multiplies {@code this} by the {@code x^offset}.
        /// </summary>
        /// <param name="offset">monomial exponent</param>
        /// <returns>{@code this * x^offset}</returns>
        /// <summary>
        /// Returns the remainder {@code this rem x^(newDegree + 1)}, it is polynomial formed by coefficients of this from
        /// zero to {@code newDegree} (both inclusive)
        /// </summary>
        /// <param name="newDegree">new degree</param>
        /// <returns>remainder {@code this rem x^(newDegree + 1)}</returns>
        /// <summary>
        /// Creates polynomial formed from the coefficients of this starting from {@code from} (inclusive) to {@code to}
        /// (exclusive)
        /// </summary>
        /// <param name="from">the initial index of the range to be copied, inclusive</param>
        /// <param name="to">the final index of the range to be copied, exclusive.</param>
        /// <returns>polynomial formed from the range of coefficients of this</returns>
        /// <summary>
        /// Reverses the coefficients of this
        /// </summary>
        /// <returns>reversed polynomial</returns>
        /// <summary>
        /// Creates new monomial {@code x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="degree">monomial degree</param>
        /// <returns>new monomial {@code coefficient * x^degree}</returns>
        /// <summary>
        /// Returns the formal derivative of this poly (new instance, so the content of this is not changed)
        /// </summary>
        /// <returns>the formal derivative</returns>
        /// <summary>
        /// Sets the content of this with {@code oth} and destroys oth
        /// </summary>
        /// <param name="oth">the polynomial (will be destroyed)</param>
        /// <returns>this := oth</returns>
        /// <summary>
        /// Calculates the composition of this(oth) (new instance, so the content of this is not changed))
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Calculates the composition of this(oth) (new instance, so the content of this is not changed))
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Stream polynomial coefficients as constant polynomials
        /// </summary>
        /// <summary>
        /// Calculates the composition of this(oth)
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Convert to multivariate polynomial
        /// </summary>
        /// <summary>
        /// Convert to multivariate polynomial
        /// </summary>
        AMultivariatePolynomial<Term, MPoly> AsMultivariate<Term, MPoly>()
        {
            return AsMultivariate<Term, MPoly>(MonomialOrder.DEFAULT);
        }

        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        /// <summary>
        /// Returns the quotient {@code this / x^offset}, it is polynomial with coefficient list formed by shifting
        /// coefficients of {@code this} to the left by {@code offset}.
        /// </summary>
        /// <param name="offset">shift amount</param>
        /// <returns>the quotient {@code this / x^offset}</returns>
        /// <summary>
        /// Multiplies {@code this} by the {@code x^offset}.
        /// </summary>
        /// <param name="offset">monomial exponent</param>
        /// <returns>{@code this * x^offset}</returns>
        /// <summary>
        /// Returns the remainder {@code this rem x^(newDegree + 1)}, it is polynomial formed by coefficients of this from
        /// zero to {@code newDegree} (both inclusive)
        /// </summary>
        /// <param name="newDegree">new degree</param>
        /// <returns>remainder {@code this rem x^(newDegree + 1)}</returns>
        /// <summary>
        /// Creates polynomial formed from the coefficients of this starting from {@code from} (inclusive) to {@code to}
        /// (exclusive)
        /// </summary>
        /// <param name="from">the initial index of the range to be copied, inclusive</param>
        /// <param name="to">the final index of the range to be copied, exclusive.</param>
        /// <returns>polynomial formed from the range of coefficients of this</returns>
        /// <summary>
        /// Reverses the coefficients of this
        /// </summary>
        /// <returns>reversed polynomial</returns>
        /// <summary>
        /// Creates new monomial {@code x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="degree">monomial degree</param>
        /// <returns>new monomial {@code coefficient * x^degree}</returns>
        /// <summary>
        /// Returns the formal derivative of this poly (new instance, so the content of this is not changed)
        /// </summary>
        /// <returns>the formal derivative</returns>
        /// <summary>
        /// Sets the content of this with {@code oth} and destroys oth
        /// </summary>
        /// <param name="oth">the polynomial (will be destroyed)</param>
        /// <returns>this := oth</returns>
        /// <summary>
        /// Calculates the composition of this(oth) (new instance, so the content of this is not changed))
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Calculates the composition of this(oth) (new instance, so the content of this is not changed))
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Stream polynomial coefficients as constant polynomials
        /// </summary>
        /// <summary>
        /// Calculates the composition of this(oth)
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Convert to multivariate polynomial
        /// </summary>
        /// <summary>
        /// Convert to multivariate polynomial
        /// </summary>
        /// <summary>
        /// ensures that internal storage has enough size to store {@code desiredCapacity} elements
        /// </summary>
        void EnsureInternalCapacity(int desiredCapacity);
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        /// <summary>
        /// Returns the quotient {@code this / x^offset}, it is polynomial with coefficient list formed by shifting
        /// coefficients of {@code this} to the left by {@code offset}.
        /// </summary>
        /// <param name="offset">shift amount</param>
        /// <returns>the quotient {@code this / x^offset}</returns>
        /// <summary>
        /// Multiplies {@code this} by the {@code x^offset}.
        /// </summary>
        /// <param name="offset">monomial exponent</param>
        /// <returns>{@code this * x^offset}</returns>
        /// <summary>
        /// Returns the remainder {@code this rem x^(newDegree + 1)}, it is polynomial formed by coefficients of this from
        /// zero to {@code newDegree} (both inclusive)
        /// </summary>
        /// <param name="newDegree">new degree</param>
        /// <returns>remainder {@code this rem x^(newDegree + 1)}</returns>
        /// <summary>
        /// Creates polynomial formed from the coefficients of this starting from {@code from} (inclusive) to {@code to}
        /// (exclusive)
        /// </summary>
        /// <param name="from">the initial index of the range to be copied, inclusive</param>
        /// <param name="to">the final index of the range to be copied, exclusive.</param>
        /// <returns>polynomial formed from the range of coefficients of this</returns>
        /// <summary>
        /// Reverses the coefficients of this
        /// </summary>
        /// <returns>reversed polynomial</returns>
        /// <summary>
        /// Creates new monomial {@code x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="degree">monomial degree</param>
        /// <returns>new monomial {@code coefficient * x^degree}</returns>
        /// <summary>
        /// Returns the formal derivative of this poly (new instance, so the content of this is not changed)
        /// </summary>
        /// <returns>the formal derivative</returns>
        /// <summary>
        /// Sets the content of this with {@code oth} and destroys oth
        /// </summary>
        /// <param name="oth">the polynomial (will be destroyed)</param>
        /// <returns>this := oth</returns>
        /// <summary>
        /// Calculates the composition of this(oth) (new instance, so the content of this is not changed))
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Calculates the composition of this(oth) (new instance, so the content of this is not changed))
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Stream polynomial coefficients as constant polynomials
        /// </summary>
        /// <summary>
        /// Calculates the composition of this(oth)
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Convert to multivariate polynomial
        /// </summary>
        /// <summary>
        /// Convert to multivariate polynomial
        /// </summary>
        /// <summary>
        /// ensures that internal storage has enough size to store {@code desiredCapacity} elements
        /// </summary>
        bool IsLinearOrConstant()
        {
            return Degree() <= 1;
        }

        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree of this polynomial</returns>
        /// <summary>
        /// Returns the number of non zero terms in this poly
        /// </summary>
        /// <summary>
        /// Returns whether i-th coefficient of this is zero
        /// </summary>
        /// <param name="i">the position</param>
        /// <returns>whether i-th coefficient of this is zero</returns>
        /// <summary>
        /// Fills i-th element with zero
        /// </summary>
        /// <param name="i">position</param>
        /// <returns>self</returns>
        /// <summary>
        /// Sets i-th element of this by j-th element of other poly
        /// </summary>
        /// <param name="indexInThis">index in self</param>
        /// <param name="poly">other polynomial</param>
        /// <param name="indexInPoly">index in other polynomial</param>
        /// <returns>self</returns>
        /// <summary>
        /// Returns i-th coefficient of this as a constant polynomial
        /// </summary>
        /// <param name="i">index in this</param>
        /// <returns>i-th coefficient of this as a constant polynomial</returns>
        /// <summary>
        /// Returns a set of exponents of non-zero terms
        /// </summary>
        /// <returns>a set of exponents of non-zero terms</returns>
        /// <summary>
        /// Returns position of the first non-zero coefficient, that is common monomial exponent (e.g. 2 for x^2 + x^3 +
        /// ...). In the case of zero polynomial, -1 returned
        /// </summary>
        /// <returns>position of the first non-zero coefficient or -1 if this is zero</returns>
        /// <summary>
        /// Returns the quotient {@code this / x^offset}, it is polynomial with coefficient list formed by shifting
        /// coefficients of {@code this} to the left by {@code offset}.
        /// </summary>
        /// <param name="offset">shift amount</param>
        /// <returns>the quotient {@code this / x^offset}</returns>
        /// <summary>
        /// Multiplies {@code this} by the {@code x^offset}.
        /// </summary>
        /// <param name="offset">monomial exponent</param>
        /// <returns>{@code this * x^offset}</returns>
        /// <summary>
        /// Returns the remainder {@code this rem x^(newDegree + 1)}, it is polynomial formed by coefficients of this from
        /// zero to {@code newDegree} (both inclusive)
        /// </summary>
        /// <param name="newDegree">new degree</param>
        /// <returns>remainder {@code this rem x^(newDegree + 1)}</returns>
        /// <summary>
        /// Creates polynomial formed from the coefficients of this starting from {@code from} (inclusive) to {@code to}
        /// (exclusive)
        /// </summary>
        /// <param name="from">the initial index of the range to be copied, inclusive</param>
        /// <param name="to">the final index of the range to be copied, exclusive.</param>
        /// <returns>polynomial formed from the range of coefficients of this</returns>
        /// <summary>
        /// Reverses the coefficients of this
        /// </summary>
        /// <returns>reversed polynomial</returns>
        /// <summary>
        /// Creates new monomial {@code x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="degree">monomial degree</param>
        /// <returns>new monomial {@code coefficient * x^degree}</returns>
        /// <summary>
        /// Returns the formal derivative of this poly (new instance, so the content of this is not changed)
        /// </summary>
        /// <returns>the formal derivative</returns>
        /// <summary>
        /// Sets the content of this with {@code oth} and destroys oth
        /// </summary>
        /// <param name="oth">the polynomial (will be destroyed)</param>
        /// <returns>this := oth</returns>
        /// <summary>
        /// Calculates the composition of this(oth) (new instance, so the content of this is not changed))
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Calculates the composition of this(oth) (new instance, so the content of this is not changed))
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Stream polynomial coefficients as constant polynomials
        /// </summary>
        /// <summary>
        /// Calculates the composition of this(oth)
        /// </summary>
        /// <param name="value">polynomial</param>
        /// <returns>composition {@code this(oth)}</returns>
        /// <summary>
        /// Convert to multivariate polynomial
        /// </summary>
        /// <summary>
        /// Convert to multivariate polynomial
        /// </summary>
        /// <summary>
        /// ensures that internal storage has enough size to store {@code desiredCapacity} elements
        /// </summary>
        bool IsLinearExactly()
        {
            return Degree() == 1;
        }
    }
}