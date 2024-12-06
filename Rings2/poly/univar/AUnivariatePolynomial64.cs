using Cc.Redberry.Libdivide4j.FastDivision;
using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Io;
using Cc.Redberry.Rings.Poly;
using Cc.Redberry.Rings.Util;
using Java.Util;
using Java.Util.Function;
using Java.Util.Stream;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Poly.Univar.RoundingMode;
using static Cc.Redberry.Rings.Poly.Univar.Associativity;
using static Cc.Redberry.Rings.Poly.Univar.Operator;
using static Cc.Redberry.Rings.Poly.Univar.TokenType;
using static Cc.Redberry.Rings.Poly.Univar.SystemInfo;

namespace Cc.Redberry.Rings.Poly.Univar
{
    /// <summary>
    /// Univariate polynomials over machine integers.
    /// </summary>
    abstract class AUnivariatePolynomial64<lPoly> : IUnivariatePolynomial<lPoly>
    {
        private static readonly long serialVersionUID = 1;
        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        long[] data;
        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        int degree;
        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        private readonly lPoly self = (lPoly)this;
        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        public int Degree()
        {
            return degree;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        public long Get(int i)
        {
            return i > degree ? 0 : data[i];
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        public lPoly Set(int i, long el)
        {
            el = ValueOf(el);
            if (el == 0)
            {
                if (i > degree)
                    return self;
                data[i] = el;
                FixDegree();
                return self;
            }

            EnsureCapacity(i);
            data[i] = el;
            FixDegree();
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        public lPoly SetLC(long lc)
        {
            return Set(degree, lc);
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        public int FirstNonZeroCoefficientPosition()
        {
            if (IsZero())
                return -1;
            int i = 0;
            while (data[i] == 0)
                ++i;
            return i;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        public long Lc()
        {
            return data[degree];
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        public lPoly LcAsPoly()
        {
            return CreateConstant(Lc());
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        public lPoly CcAsPoly()
        {
            return CreateConstant(Cc());
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        public virtual lPoly GetAsPoly(int i)
        {
            return CreateConstant(Get(i));
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        public long Cc()
        {
            return data[0];
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        public void EnsureInternalCapacity(int desiredCapacity)
        {
            if (data.Length < desiredCapacity)

                // the rest will be filled with zeros
                data = Arrays.CopyOf(data, desiredCapacity);
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        void EnsureCapacity(int desiredDegree)
        {
            if (degree < desiredDegree)
                degree = desiredDegree;
            if (data.Length < (desiredDegree + 1))
                data = Arrays.CopyOf(data, desiredDegree + 1);
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        void FixDegree()
        {
            int i = degree;
            while (i >= 0 && data[i] == 0)
                --i;
            if (i < 0)
                i = 0;
            if (i != degree)
            {
                degree = i; // unnecessary clearing
                // Arrays.fill(data, degree + 1, data.length, 0);
            }
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        public abstract UnivariatePolynomial<BigInteger> ToBigPoly();
        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        public abstract lPoly CreateFromArray(long[] data);
        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        public lPoly CreateMonomial(int degree)
        {
            return CreateMonomial(1, degree);
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        public lPoly CreateLinear(long cc, long lc)
        {
            return CreateFromArray(new long[] { cc, lc });
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        public abstract lPoly CreateMonomial(long coefficient, int degree);
        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        public lPoly CreateConstant(long val)
        {
            return CreateFromArray(new long[] { val });
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        public lPoly CreateZero()
        {
            return CreateConstant(0);
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        public lPoly CreateOne()
        {
            return CreateConstant(1);
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        public bool IsZeroAt(int i)
        {
            return i >= data.Length || data[i] == 0;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        public lPoly SetZero(int i)
        {
            if (i < data.Length)
                data[i] = 0;
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        public lPoly SetFrom(int indexInThis, lPoly poly, int indexInPoly)
        {
            EnsureCapacity(indexInThis);
            data[indexInThis] = poly[indexInPoly];
            FixDegree();
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        public bool IsZero()
        {
            return data[degree] == 0;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        public bool IsOne()
        {
            return degree == 0 && data[0] == 1;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        public bool IsMonic()
        {
            return Lc() == 1;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        public bool IsUnitCC()
        {
            return Cc() == 1;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        public bool IsConstant()
        {
            return degree == 0;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        public bool IsMonomial()
        {
            for (int i = degree - 1; i >= 0; --i)
                if (data[i] != 0)
                    return false;
            return true;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        public int SignumOfLC()
        {
            return Long.Signum(Lc());
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        public double Norm1()
        {
            double norm = 0;
            for (int i = 0; i <= degree; ++i)
                norm += Math.Abs(data[i]);
            return norm;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        public double Norm2()
        {
            double norm = 0;
            for (int i = 0; i <= degree; ++i)
                norm += ((double)data[i]) * data[i];
            return Math.Ceil(Math.Sqrt(norm));
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        public double NormMax()
        {
            return (double)MaxAbsCoefficient();
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        public long MaxAbsCoefficient()
        {
            long max = Math.Abs(data[0]);
            for (int i = 1; i <= degree; ++i)
                max = Math.Max(Math.Abs(data[i]), max);
            return max;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        public lPoly ToZero()
        {
            Arrays.Fill(data, 0, degree + 1, 0);
            degree = 0;
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        public lPoly Set(lPoly oth)
        {
            if (oth == this)
                return self;
            this.data = oth.data.Clone();
            this.degree = oth.degree;
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        public lPoly SetAndDestroy(lPoly oth)
        {
            this.data = oth.data;
            oth.data = null; // destroy
            this.degree = oth.degree;
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        public lPoly ShiftLeft(int offset)
        {
            if (offset == 0)
                return self;
            if (offset > degree)
                return ToZero();
            System.Arraycopy(data, offset, data, 0, degree - offset + 1);
            Arrays.Fill(data, degree - offset + 1, degree + 1, 0);
            degree = degree - offset;
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        public lPoly ShiftRight(int offset)
        {
            if (offset == 0)
                return self;
            int degree = this.degree;
            EnsureCapacity(offset + degree);
            System.Arraycopy(data, 0, data, offset, degree + 1);
            Arrays.Fill(data, 0, offset, 0);
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        public lPoly Truncate(int newDegree)
        {
            if (newDegree >= degree)
                return self;
            Arrays.Fill(data, newDegree + 1, degree + 1, 0);
            degree = newDegree;
            FixDegree();
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        public lPoly Reverse()
        {
            ArraysUtil.Reverse(data, 0, degree + 1);
            FixDegree();
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        public abstract long Content();
        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        public lPoly ContentAsPoly()
        {
            return CreateConstant(Content());
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        public lPoly PrimitivePart()
        {
            if (IsZero())
                return self;
            long content = Content();
            if (Lc() < 0)
                content = -content;
            if (content == -1)
                return Negate();
            return PrimitivePart0(content);
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        public lPoly PrimitivePartSameSign()
        {
            return PrimitivePart0(Content());
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        private lPoly PrimitivePart0(long content)
        {
            if (content == 1)
                return self;
            Magic magic = MagicSigned(content);
            for (int i = degree; i >= 0; --i)
                data[i] = DivideSignedFast(data[i], magic);
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        abstract long Add(long a, long b);
        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        abstract long Subtract(long a, long b);
        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        abstract long Multiply(long a, long b);
        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        abstract long Negate(long a);
        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        abstract long ValueOf(long a);
        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        public long Evaluate(long point)
        {
            if (point == 0)
                return Cc();
            point = ValueOf(point);
            long res = 0;
            for (int i = degree; i >= 0; --i)
                res = Add(Multiply(res, point), data[i]);
            return res;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        public lPoly Composition(lPoly value)
        {
            if (value.IsOne())
                return this.Clone();
            if (value.IsZero())
                return CcAsPoly();
            lPoly result = CreateZero();
            for (int i = degree; i >= 0; --i)
                result = result.Multiply(value).Add(data[i]);
            return result;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        public lPoly Shift(long value)
        {
            return Composition(CreateLinear(value, 1));
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        public abstract lPoly Monic(long factor);
        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        public virtual lPoly MonicWithLC(lPoly other)
        {
            if (Lc() == other.Lc())
                return self;
            return Monic(other.Lc());
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        public lPoly Add(long val)
        {
            data[0] = Add(data[0], ValueOf(val));
            FixDegree();
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        public lPoly Subtract(long val)
        {
            data[0] = Subtract(data[0], ValueOf(val));
            FixDegree();
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        public lPoly Decrement()
        {
            return Subtract(CreateOne());
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        public lPoly Increment()
        {
            return Add(CreateOne());
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        public lPoly Add(lPoly oth)
        {
            AssertSameCoefficientRingWith(oth);
            if (oth.IsZero())
                return self;
            if (IsZero())
                return Set(oth);
            AssertSameCoefficientRingWith(oth);
            EnsureCapacity(oth.degree);
            for (int i = oth.degree; i >= 0; --i)
                data[i] = Add(data[i], oth.data[i]);
            FixDegree();
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        public lPoly AddMonomial(long coefficient, int exponent)
        {
            if (coefficient == 0)
                return self;
            EnsureCapacity(exponent);
            data[exponent] = Add(data[exponent], ValueOf(coefficient));
            FixDegree();
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        public lPoly AddMul(lPoly oth, long factor)
        {
            AssertSameCoefficientRingWith(oth);
            if (oth.IsZero())
                return self;
            factor = ValueOf(factor);
            if (factor == 0)
                return self;
            AssertSameCoefficientRingWith(oth);
            EnsureCapacity(oth.degree);
            for (int i = oth.degree; i >= 0; --i)
                data[i] = Add(data[i], Multiply(factor, oth.data[i]));
            FixDegree();
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        public lPoly Subtract(lPoly oth)
        {
            AssertSameCoefficientRingWith(oth);
            if (oth.IsZero())
                return self;
            if (IsZero())
                return Set(oth).Negate();
            AssertSameCoefficientRingWith(oth);
            EnsureCapacity(oth.degree);
            for (int i = oth.degree; i >= 0; --i)
                data[i] = Subtract(data[i], oth.data[i]);
            FixDegree();
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        public lPoly Subtract(lPoly oth, long factor, int exponent)
        {
            AssertSameCoefficientRingWith(oth);
            if (oth.IsZero())
                return self;
            factor = ValueOf(factor);
            if (factor == 0)
                return self;
            AssertSameCoefficientRingWith(oth);
            for (int i = oth.degree + exponent; i >= exponent; --i)
                data[i] = Subtract(data[i], Multiply(factor, oth.data[i - exponent]));
            FixDegree();
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        public lPoly Negate()
        {
            for (int i = degree; i >= 0; --i)
                data[i] = Negate(data[i]);
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        public virtual lPoly MultiplyByLC(lPoly other)
        {
            return Multiply(other.Lc());
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        public lPoly Multiply(long factor)
        {
            factor = ValueOf(factor);
            if (factor == 1)
                return self;
            if (factor == 0)
                return ToZero();
            for (int i = degree; i >= 0; --i)
                data[i] = Multiply(data[i], factor);
            return self;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        public abstract lPoly Clone();
        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        BigInteger[] DataToBigIntegers()
        {
            BigInteger[] bData = new BigInteger[degree + 1];
            for (int i = degree; i >= 0; --i)
                bData[i] = BigInteger.ValueOf(data[i]);
            return bData;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        public long[] GetDataReferenceUnsafe()
        {
            return data;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        public int CompareTo(lPoly o)
        {
            int c = Integer.Compare(degree, o.degree);
            if (c != 0)
                return c;
            for (int i = degree; i >= 0; --i)
            {
                c = Long.Compare(data[i], o.data[i]);
                if (c != 0)
                    return c;
            }

            return 0;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        public virtual string ToString()
        {
            return ToString(IStringifier.Dummy());
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        public virtual string ToString(IStringifier<lPoly> stringifier)
        {
            if (IsConstant())
                return Long.ToString(Cc());
            string varString = stringifier.GetBindings().GetOrDefault(CreateMonomial(1), IStringifier.DefaultVar());
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i <= degree; i++)
            {
                long el = data[i];
                if (el == 0)
                    continue;
                string cfString;
                if (el != 1 || i == 0)
                    cfString = Long.ToString(el);
                else
                    cfString = "";
                if (sb.Length != 0 && !cfString.StartsWith("-"))
                    sb.Append("+");
                sb.Append(cfString);
                if (i == 0)
                    continue;
                if (!cfString.IsEmpty())
                    sb.Append("*");
                sb.Append(varString);
                if (i > 1)
                    sb.Append("^").Append(i);
            }

            return sb.ToString();
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        public virtual string ToStringForCopy()
        {
            string s = ArraysUtil.ToString(data, 0, degree + 1);
            return "of(" + s.Substring(1, s.Length - 1) + ")";
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        public virtual Stream<lPoly> StreamAsPolys()
        {
            return Stream().MapToObj(this.CreateConstant());
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        public LongStream Stream()
        {
            return Arrays.Stream(data, 0, degree + 1);
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        public UnivariatePolynomial<T> MapCoefficients<T>(Ring<T> ring, LongFunction<T> mapper)
        {
            return Stream().MapToObj(mapper).Collect(new PolynomialCollector<T>(ring));
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        public bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType())
                return false;
            AUnivariatePolynomial64 oth = (AUnivariatePolynomial64)obj;
            if (degree != oth.degree)
                return false;
            for (int i = 0; i <= degree; ++i)
                if (data[i] != oth.data[i])
                    return false;
            return true;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        public int GetHashCode()
        {
            int result = 1;
            for (int i = degree; i >= 0; --i)
            {
                long element = data[i];
                int elementHash = (int)(element ^ (element >>> 32));
                result = 31 * result + elementHash;
            }

            return result;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        /* =========================== Multiplication with safe arithmetic =========================== */
        /// <summary>
        /// switch to classical multiplication
        /// </summary>
        static readonly long KARATSUBA_THRESHOLD = 2048;
        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        /* =========================== Multiplication with safe arithmetic =========================== */
        /// <summary>
        /// switch to classical multiplication
        /// </summary>
        /// <summary>
        /// when use Karatsuba fast multiplication
        /// </summary>
        static readonly long MUL_CLASSICAL_THRESHOLD = 256 * 256, MUL_MOD_CLASSICAL_THRESHOLD = 128 * 128;
        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        /* =========================== Multiplication with safe arithmetic =========================== */
        /// <summary>
        /// switch to classical multiplication
        /// </summary>
        /// <summary>
        /// when use Karatsuba fast multiplication
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        long[] MultiplyUnsafe0(lPoly oth)
        {
            if (1 * (degree + 1) * (degree + 1) <= MUL_CLASSICAL_THRESHOLD)
                return MultiplyClassicalUnsafe(data, 0, degree + 1, oth.data, 0, oth.degree + 1);
            else
                return MultiplyKaratsubaUnsafe(data, 0, degree + 1, oth.data, 0, oth.degree + 1);
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        /* =========================== Multiplication with safe arithmetic =========================== */
        /// <summary>
        /// switch to classical multiplication
        /// </summary>
        /// <summary>
        /// when use Karatsuba fast multiplication
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        long[] MultiplySafe0(lPoly oth)
        {
            if (1 * (degree + 1) * (degree + 1) <= MUL_CLASSICAL_THRESHOLD)
                return MultiplyClassicalSafe(data, 0, degree + 1, oth.data, 0, oth.degree + 1);
            else
                return MultiplyKaratsubaSafe(data, 0, degree + 1, oth.data, 0, oth.degree + 1);
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        /* =========================== Multiplication with safe arithmetic =========================== */
        /// <summary>
        /// switch to classical multiplication
        /// </summary>
        /// <summary>
        /// when use Karatsuba fast multiplication
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        long[] SquareUnsafe0()
        {
            if (1 * (degree + 1) * (degree + 1) <= MUL_CLASSICAL_THRESHOLD)
                return SquareClassicalUnsafe(data, 0, degree + 1);
            else
                return SquareKaratsubaUnsafe(data, 0, degree + 1);
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        /* =========================== Multiplication with safe arithmetic =========================== */
        /// <summary>
        /// switch to classical multiplication
        /// </summary>
        /// <summary>
        /// when use Karatsuba fast multiplication
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        long[] SquareSafe0()
        {
            if (1 * (degree + 1) * (degree + 1) <= MUL_CLASSICAL_THRESHOLD)
                return SquareClassicalSafe(data, 0, degree + 1);
            else
                return SquareKaratsubaSafe(data, 0, degree + 1);
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        /* =========================== Multiplication with safe arithmetic =========================== */
        /// <summary>
        /// switch to classical multiplication
        /// </summary>
        /// <summary>
        /// when use Karatsuba fast multiplication
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /* =========================== Exact multiplication with safe arithmetics =========================== */
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <returns>the result</returns>
        long[] MultiplyClassicalSafe(long[] a, int aFrom, int aTo, long[] b, int bFrom, int bTo)
        {
            long[] result = new long[aTo - aFrom + bTo - bFrom - 1];
            MultiplyClassicalSafe(result, a, aFrom, aTo, b, bFrom, bTo);
            return result;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        /* =========================== Multiplication with safe arithmetic =========================== */
        /// <summary>
        /// switch to classical multiplication
        /// </summary>
        /// <summary>
        /// when use Karatsuba fast multiplication
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /* =========================== Exact multiplication with safe arithmetics =========================== */
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <returns>the result</returns>
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="result">where to write the result</param>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        virtual void MultiplyClassicalSafe(long[] result, long[] a, int aFrom, int aTo, long[] b, int bFrom, int bTo)
        {
            if (aTo - aFrom > bTo - bFrom)
            {
                MultiplyClassicalSafe(result, b, bFrom, bTo, a, aFrom, aTo);
                return;
            }

            for (int i = 0; i < aTo - aFrom; ++i)
            {
                long c = a[aFrom + i];
                if (c != 0)
                    for (int j = 0; j < bTo - bFrom; ++j)
                        result[i + j] = Add(result[i + j], Multiply(c, b[bFrom + j]));
            }
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        /* =========================== Multiplication with safe arithmetic =========================== */
        /// <summary>
        /// switch to classical multiplication
        /// </summary>
        /// <summary>
        /// when use Karatsuba fast multiplication
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /* =========================== Exact multiplication with safe arithmetics =========================== */
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <returns>the result</returns>
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="result">where to write the result</param>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <summary>
        /// Karatsuba multiplication
        /// </summary>
        /// <param name="f">the first multiplier</param>
        /// <param name="g">the second multiplier</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <param name="gFrom">begin in g</param>
        /// <param name="gTo">end in g</param>
        /// <returns>the result</returns>
        virtual long[] MultiplyKaratsubaSafe(long[] f, int fFrom, int fTo, long[] g, int gFrom, int gTo)
        {

            // return zero
            if (fFrom >= fTo || gFrom >= gTo)
                return new long[0];

            // single element in f
            if (fTo - fFrom == 1)
            {
                long[] result = new long[gTo - gFrom];
                for (int i = gFrom; i < gTo; ++i)
                    result[i - gFrom] = Multiply(f[fFrom], g[i]);
                return result;
            }


            // single element in g
            if (gTo - gFrom == 1)
            {
                long[] result = new long[fTo - fFrom];

                //single element in b
                for (int i = fFrom; i < fTo; ++i)
                    result[i - fFrom] = Multiply(g[gFrom], f[i]);
                return result;
            }


            // linear factors
            if (fTo - fFrom == 2 && gTo - gFrom == 2)
            {
                long[] result = new long[3];

                //both a and b are linear
                result[0] = Multiply(f[fFrom], g[gFrom]);
                result[1] = Add(Multiply(f[fFrom], g[gFrom + 1]), Multiply(f[fFrom + 1], g[gFrom]));
                result[2] = Multiply(f[fFrom + 1], g[gFrom + 1]);
                return result;
            }


            //switch to classical
            if (1 * (fTo - fFrom) * (gTo - gFrom) < KARATSUBA_THRESHOLD)
                return MultiplyClassicalSafe(g, gFrom, gTo, f, fFrom, fTo);
            if (fTo - fFrom < gTo - gFrom)
                return MultiplyKaratsubaSafe(g, gFrom, gTo, f, fFrom, fTo);

            //we now split a and b into 2 parts:
            int split = (fTo - fFrom + 1) / 2;

            //if we can't split b
            if (gFrom + split >= gTo)
            {
                long[] f0g = MultiplyKaratsubaSafe(f, fFrom, fFrom + split, g, gFrom, gTo);
                long[] f1g = MultiplyKaratsubaSafe(f, fFrom + split, fTo, g, gFrom, gTo);
                long[] result = Arrays.CopyOf(f0g, fTo - fFrom + gTo - gFrom - 1);
                for (int i = 0; i < f1g.Length; i++)
                    result[i + split] = Add(result[i + split], f1g[i]);
                return result;
            }

            int fMid = fFrom + split, gMid = gFrom + split;
            long[] f0g0 = MultiplyKaratsubaSafe(f, fFrom, fMid, g, gFrom, gMid);
            long[] f1g1 = MultiplyKaratsubaSafe(f, fMid, fTo, g, gMid, gTo);

            // f0 + f1
            long[] f0_plus_f1 = new long[Math.Max(fMid - fFrom, fTo - fMid)];
            System.Arraycopy(f, fFrom, f0_plus_f1, 0, fMid - fFrom);
            for (int i = fMid; i < fTo; ++i)
                f0_plus_f1[i - fMid] = Add(f0_plus_f1[i - fMid], f[i]);

            //g0 + g1
            long[] g0_plus_g1 = new long[Math.Max(gMid - gFrom, gTo - gMid)];
            System.Arraycopy(g, gFrom, g0_plus_g1, 0, gMid - gFrom);
            for (int i = gMid; i < gTo; ++i)
                g0_plus_g1[i - gMid] = Add(g0_plus_g1[i - gMid], g[i]);
            long[] mid = MultiplyKaratsubaSafe(f0_plus_f1, 0, f0_plus_f1.Length, g0_plus_g1, 0, g0_plus_g1.Length);
            if (mid.Length < f0g0.Length)
                mid = Arrays.CopyOf(mid, f0g0.Length);
            if (mid.Length < f1g1.Length)
                mid = Arrays.CopyOf(mid, f1g1.Length);

            //subtract f0g0, f1g1
            for (int i = 0; i < f0g0.Length; ++i)
                mid[i] = Subtract(mid[i], f0g0[i]);
            for (int i = 0; i < f1g1.Length; ++i)
                mid[i] = Subtract(mid[i], f1g1[i]);
            long[] result = Arrays.CopyOf(f0g0, (fTo - fFrom) + (gTo - gFrom) - 1);
            for (int i = 0; i < mid.Length; ++i)
                result[i + split] = Add(result[i + split], mid[i]);
            for (int i = 0; i < f1g1.Length; ++i)
                result[i + 2 * split] = Add(result[i + 2 * split], f1g1[i]);
            return result;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        /* =========================== Multiplication with safe arithmetic =========================== */
        /// <summary>
        /// switch to classical multiplication
        /// </summary>
        /// <summary>
        /// when use Karatsuba fast multiplication
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /* =========================== Exact multiplication with safe arithmetics =========================== */
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <returns>the result</returns>
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="result">where to write the result</param>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <summary>
        /// Karatsuba multiplication
        /// </summary>
        /// <param name="f">the first multiplier</param>
        /// <param name="g">the second multiplier</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <param name="gFrom">begin in g</param>
        /// <param name="gTo">end in g</param>
        /// <returns>the result</returns>
        // return zero
        // single element in f
        // single element in g
        //single element in b
        // linear factors
        //both a and b are linear
        //switch to classical
        //we now split a and b into 2 parts:
        //if we can't split b
        // f0 + f1
        //g0 + g1
        //subtract f0g0, f1g1
        virtual long[] SquareClassicalSafe(long[] a, int from, int to)
        {
            long[] x = new long[(to - from) * 2 - 1];
            SquareClassicalSafe(x, a, from, to);
            return x;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        /* =========================== Multiplication with safe arithmetic =========================== */
        /// <summary>
        /// switch to classical multiplication
        /// </summary>
        /// <summary>
        /// when use Karatsuba fast multiplication
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /* =========================== Exact multiplication with safe arithmetics =========================== */
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <returns>the result</returns>
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="result">where to write the result</param>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <summary>
        /// Karatsuba multiplication
        /// </summary>
        /// <param name="f">the first multiplier</param>
        /// <param name="g">the second multiplier</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <param name="gFrom">begin in g</param>
        /// <param name="gTo">end in g</param>
        /// <returns>the result</returns>
        // return zero
        // single element in f
        // single element in g
        //single element in b
        // linear factors
        //both a and b are linear
        //switch to classical
        //we now split a and b into 2 parts:
        //if we can't split b
        // f0 + f1
        //g0 + g1
        //subtract f0g0, f1g1
        /// <summary>
        /// Square the poly {@code data} using classical algorithm
        /// </summary>
        /// <param name="result">result destination</param>
        /// <param name="data">the data</param>
        /// <param name="from">data from</param>
        /// <param name="to">end point in the {@code data}</param>
        virtual void SquareClassicalSafe(long[] result, long[] data, int from, int to)
        {
            int len = to - from;
            for (int i = 0; i < len; ++i)
            {
                long c = data[from + i];
                if (c != 0)
                    for (int j = 0; j < len; ++j)
                        result[i + j] = Add(result[i + j], Multiply(c, data[from + j]));
            }
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        /* =========================== Multiplication with safe arithmetic =========================== */
        /// <summary>
        /// switch to classical multiplication
        /// </summary>
        /// <summary>
        /// when use Karatsuba fast multiplication
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /* =========================== Exact multiplication with safe arithmetics =========================== */
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <returns>the result</returns>
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="result">where to write the result</param>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <summary>
        /// Karatsuba multiplication
        /// </summary>
        /// <param name="f">the first multiplier</param>
        /// <param name="g">the second multiplier</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <param name="gFrom">begin in g</param>
        /// <param name="gTo">end in g</param>
        /// <returns>the result</returns>
        // return zero
        // single element in f
        // single element in g
        //single element in b
        // linear factors
        //both a and b are linear
        //switch to classical
        //we now split a and b into 2 parts:
        //if we can't split b
        // f0 + f1
        //g0 + g1
        //subtract f0g0, f1g1
        /// <summary>
        /// Square the poly {@code data} using classical algorithm
        /// </summary>
        /// <param name="result">result destination</param>
        /// <param name="data">the data</param>
        /// <param name="from">data from</param>
        /// <param name="to">end point in the {@code data}</param>
        /// <summary>
        /// Karatsuba squaring
        /// </summary>
        /// <param name="f">the data</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <returns>the result</returns>
        virtual long[] SquareKaratsubaSafe(long[] f, int fFrom, int fTo)
        {
            if (fFrom >= fTo)
                return new long[0];
            if (fTo - fFrom == 1)
                return new long[]
                {
                    Multiply(f[fFrom], f[fFrom])
                };
            if (fTo - fFrom == 2)
            {
                long[] result = new long[3];
                result[0] = Multiply(f[fFrom], f[fFrom]);
                result[1] = Multiply(Multiply(ValueOf(2), f[fFrom]), f[fFrom + 1]);
                result[2] = Multiply(f[fFrom + 1], f[fFrom + 1]);
                return result;
            }


            //switch to classical
            if (1 * (fTo - fFrom) * (fTo - fFrom) < KARATSUBA_THRESHOLD)
                return SquareClassicalSafe(f, fFrom, fTo);

            //we now split a and b into 2 parts:
            int split = (fTo - fFrom + 1) / 2;
            int fMid = fFrom + split;
            long[] f0g0 = SquareKaratsubaSafe(f, fFrom, fMid);
            long[] f1g1 = SquareKaratsubaSafe(f, fMid, fTo);

            // f0 + f1
            long[] f0_plus_f1 = new long[Math.Max(fMid - fFrom, fTo - fMid)];
            System.Arraycopy(f, fFrom, f0_plus_f1, 0, fMid - fFrom);
            for (int i = fMid; i < fTo; ++i)
                f0_plus_f1[i - fMid] = Add(f0_plus_f1[i - fMid], f[i]);
            long[] mid = SquareKaratsubaSafe(f0_plus_f1, 0, f0_plus_f1.Length);
            if (mid.Length < f0g0.Length)
                mid = Arrays.CopyOf(mid, f0g0.Length);
            if (mid.Length < f1g1.Length)
                mid = Arrays.CopyOf(mid, f1g1.Length);

            //subtract f0g0, f1g1
            for (int i = 0; i < f0g0.Length; ++i)
                mid[i] = Subtract(mid[i], f0g0[i]);
            for (int i = 0; i < f1g1.Length; ++i)
                mid[i] = Subtract(mid[i], f1g1[i]);
            long[] result = Arrays.CopyOf(f0g0, 2 * (fTo - fFrom) - 1);
            for (int i = 0; i < mid.Length; ++i)
                result[i + split] = Add(result[i + split], mid[i]);
            for (int i = 0; i < f1g1.Length; ++i)
                result[i + 2 * split] = Add(result[i + 2 * split], f1g1[i]);
            return result;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        /* =========================== Multiplication with safe arithmetic =========================== */
        /// <summary>
        /// switch to classical multiplication
        /// </summary>
        /// <summary>
        /// when use Karatsuba fast multiplication
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /* =========================== Exact multiplication with safe arithmetics =========================== */
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <returns>the result</returns>
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="result">where to write the result</param>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <summary>
        /// Karatsuba multiplication
        /// </summary>
        /// <param name="f">the first multiplier</param>
        /// <param name="g">the second multiplier</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <param name="gFrom">begin in g</param>
        /// <param name="gTo">end in g</param>
        /// <returns>the result</returns>
        // return zero
        // single element in f
        // single element in g
        //single element in b
        // linear factors
        //both a and b are linear
        //switch to classical
        //we now split a and b into 2 parts:
        //if we can't split b
        // f0 + f1
        //g0 + g1
        //subtract f0g0, f1g1
        /// <summary>
        /// Square the poly {@code data} using classical algorithm
        /// </summary>
        /// <param name="result">result destination</param>
        /// <param name="data">the data</param>
        /// <param name="from">data from</param>
        /// <param name="to">end point in the {@code data}</param>
        /// <summary>
        /// Karatsuba squaring
        /// </summary>
        /// <param name="f">the data</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <returns>the result</returns>
        //switch to classical
        //we now split a and b into 2 parts:
        // f0 + f1
        //subtract f0g0, f1g1
        /* =========================== Exact multiplication with unsafe arithmetics =========================== */
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="result">where to write the result</param>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        static void MultiplyClassicalUnsafe(long[] result, long[] a, int aFrom, int aTo, long[] b, int bFrom, int bTo)
        {
            if (aTo - aFrom > bTo - bFrom)
            {
                MultiplyClassicalUnsafe(result, b, bFrom, bTo, a, aFrom, aTo);
                return;
            }

            for (int i = 0; i < aTo - aFrom; ++i)
            {
                long c = a[aFrom + i];
                if (c != 0)
                    for (int j = 0; j < bTo - bFrom; ++j)
                        result[i + j] = result[i + j] + c * b[bFrom + j];
            }
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        /* =========================== Multiplication with safe arithmetic =========================== */
        /// <summary>
        /// switch to classical multiplication
        /// </summary>
        /// <summary>
        /// when use Karatsuba fast multiplication
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /* =========================== Exact multiplication with safe arithmetics =========================== */
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <returns>the result</returns>
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="result">where to write the result</param>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <summary>
        /// Karatsuba multiplication
        /// </summary>
        /// <param name="f">the first multiplier</param>
        /// <param name="g">the second multiplier</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <param name="gFrom">begin in g</param>
        /// <param name="gTo">end in g</param>
        /// <returns>the result</returns>
        // return zero
        // single element in f
        // single element in g
        //single element in b
        // linear factors
        //both a and b are linear
        //switch to classical
        //we now split a and b into 2 parts:
        //if we can't split b
        // f0 + f1
        //g0 + g1
        //subtract f0g0, f1g1
        /// <summary>
        /// Square the poly {@code data} using classical algorithm
        /// </summary>
        /// <param name="result">result destination</param>
        /// <param name="data">the data</param>
        /// <param name="from">data from</param>
        /// <param name="to">end point in the {@code data}</param>
        /// <summary>
        /// Karatsuba squaring
        /// </summary>
        /// <param name="f">the data</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <returns>the result</returns>
        //switch to classical
        //we now split a and b into 2 parts:
        // f0 + f1
        //subtract f0g0, f1g1
        /* =========================== Exact multiplication with unsafe arithmetics =========================== */
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="result">where to write the result</param>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <returns>the result</returns>
        static long[] MultiplyClassicalUnsafe(long[] a, int aFrom, int aTo, long[] b, int bFrom, int bTo)
        {
            long[] result = new long[aTo - aFrom + bTo - bFrom - 1];
            MultiplyClassicalUnsafe(result, a, aFrom, aTo, b, bFrom, bTo);
            return result;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        /* =========================== Multiplication with safe arithmetic =========================== */
        /// <summary>
        /// switch to classical multiplication
        /// </summary>
        /// <summary>
        /// when use Karatsuba fast multiplication
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /* =========================== Exact multiplication with safe arithmetics =========================== */
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <returns>the result</returns>
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="result">where to write the result</param>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <summary>
        /// Karatsuba multiplication
        /// </summary>
        /// <param name="f">the first multiplier</param>
        /// <param name="g">the second multiplier</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <param name="gFrom">begin in g</param>
        /// <param name="gTo">end in g</param>
        /// <returns>the result</returns>
        // return zero
        // single element in f
        // single element in g
        //single element in b
        // linear factors
        //both a and b are linear
        //switch to classical
        //we now split a and b into 2 parts:
        //if we can't split b
        // f0 + f1
        //g0 + g1
        //subtract f0g0, f1g1
        /// <summary>
        /// Square the poly {@code data} using classical algorithm
        /// </summary>
        /// <param name="result">result destination</param>
        /// <param name="data">the data</param>
        /// <param name="from">data from</param>
        /// <param name="to">end point in the {@code data}</param>
        /// <summary>
        /// Karatsuba squaring
        /// </summary>
        /// <param name="f">the data</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <returns>the result</returns>
        //switch to classical
        //we now split a and b into 2 parts:
        // f0 + f1
        //subtract f0g0, f1g1
        /* =========================== Exact multiplication with unsafe arithmetics =========================== */
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="result">where to write the result</param>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <returns>the result</returns>
        /// <summary>
        /// Karatsuba multiplication
        /// </summary>
        /// <param name="f">the first multiplier</param>
        /// <param name="g">the second multiplier</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <param name="gFrom">begin in g</param>
        /// <param name="gTo">end in g</param>
        /// <returns>the result</returns>
        static long[] MultiplyKaratsubaUnsafe(long[] f, int fFrom, int fTo, long[] g, int gFrom, int gTo)
        {

            // return zero
            if (fFrom >= fTo || gFrom >= gTo)
                return new long[0];

            // single element in f
            if (fTo - fFrom == 1)
            {
                long[] result = new long[gTo - gFrom];
                for (int i = gFrom; i < gTo; ++i)
                    result[i - gFrom] = f[fFrom] * g[i];
                return result;
            }


            // single element in g
            if (gTo - gFrom == 1)
            {
                long[] result = new long[fTo - fFrom];

                //single element in b
                for (int i = fFrom; i < fTo; ++i)
                    result[i - fFrom] = g[gFrom] * f[i];
                return result;
            }


            // linear factors
            if (fTo - fFrom == 2 && gTo - gFrom == 2)
            {
                long[] result = new long[3];

                //both a and b are linear
                result[0] = f[fFrom] * g[gFrom];
                result[1] = f[fFrom] * g[gFrom + 1] + f[fFrom + 1] * g[gFrom];
                result[2] = f[fFrom + 1] * g[gFrom + 1];
                return result;
            }


            //switch to classical
            if (1 * (fTo - fFrom) * (gTo - gFrom) < KARATSUBA_THRESHOLD)
                return MultiplyClassicalUnsafe(g, gFrom, gTo, f, fFrom, fTo);
            if (fTo - fFrom < gTo - gFrom)
                return MultiplyKaratsubaUnsafe(g, gFrom, gTo, f, fFrom, fTo);

            //we now split a and b into 2 parts:
            int split = (fTo - fFrom + 1) / 2;

            //if we can't split b
            if (gFrom + split >= gTo)
            {
                long[] f0g = MultiplyKaratsubaUnsafe(f, fFrom, fFrom + split, g, gFrom, gTo);
                long[] f1g = MultiplyKaratsubaUnsafe(f, fFrom + split, fTo, g, gFrom, gTo);
                long[] result = Arrays.CopyOf(f0g, fTo - fFrom + gTo - gFrom - 1);
                for (int i = 0; i < f1g.Length; i++)
                    result[i + split] = result[i + split] + f1g[i];
                return result;
            }

            int fMid = fFrom + split, gMid = gFrom + split;
            long[] f0g0 = MultiplyKaratsubaUnsafe(f, fFrom, fMid, g, gFrom, gMid);
            long[] f1g1 = MultiplyKaratsubaUnsafe(f, fMid, fTo, g, gMid, gTo);

            // f0 + f1
            long[] f0_plus_f1 = new long[Math.Max(fMid - fFrom, fTo - fMid)];
            System.Arraycopy(f, fFrom, f0_plus_f1, 0, fMid - fFrom);
            for (int i = fMid; i < fTo; ++i)
                f0_plus_f1[i - fMid] = f0_plus_f1[i - fMid] + f[i];

            //g0 + g1
            long[] g0_plus_g1 = new long[Math.Max(gMid - gFrom, gTo - gMid)];
            System.Arraycopy(g, gFrom, g0_plus_g1, 0, gMid - gFrom);
            for (int i = gMid; i < gTo; ++i)
                g0_plus_g1[i - gMid] = g0_plus_g1[i - gMid] + g[i];
            long[] mid = MultiplyKaratsubaUnsafe(f0_plus_f1, 0, f0_plus_f1.Length, g0_plus_g1, 0, g0_plus_g1.Length);
            if (mid.Length < f0g0.Length)
                mid = Arrays.CopyOf(mid, f0g0.Length);
            if (mid.Length < f1g1.Length)
                mid = Arrays.CopyOf(mid, f1g1.Length);

            //subtract f0g0, f1g1
            for (int i = 0; i < f0g0.Length; ++i)
                mid[i] = mid[i] - f0g0[i];
            for (int i = 0; i < f1g1.Length; ++i)
                mid[i] = mid[i] - f1g1[i];
            long[] result = Arrays.CopyOf(f0g0, (fTo - fFrom) + (gTo - gFrom) - 1);
            for (int i = 0; i < mid.Length; ++i)
                result[i + split] = result[i + split] + mid[i];
            for (int i = 0; i < f1g1.Length; ++i)
                result[i + 2 * split] = result[i + 2 * split] + f1g1[i];
            return result;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        /* =========================== Multiplication with safe arithmetic =========================== */
        /// <summary>
        /// switch to classical multiplication
        /// </summary>
        /// <summary>
        /// when use Karatsuba fast multiplication
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /* =========================== Exact multiplication with safe arithmetics =========================== */
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <returns>the result</returns>
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="result">where to write the result</param>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <summary>
        /// Karatsuba multiplication
        /// </summary>
        /// <param name="f">the first multiplier</param>
        /// <param name="g">the second multiplier</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <param name="gFrom">begin in g</param>
        /// <param name="gTo">end in g</param>
        /// <returns>the result</returns>
        // return zero
        // single element in f
        // single element in g
        //single element in b
        // linear factors
        //both a and b are linear
        //switch to classical
        //we now split a and b into 2 parts:
        //if we can't split b
        // f0 + f1
        //g0 + g1
        //subtract f0g0, f1g1
        /// <summary>
        /// Square the poly {@code data} using classical algorithm
        /// </summary>
        /// <param name="result">result destination</param>
        /// <param name="data">the data</param>
        /// <param name="from">data from</param>
        /// <param name="to">end point in the {@code data}</param>
        /// <summary>
        /// Karatsuba squaring
        /// </summary>
        /// <param name="f">the data</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <returns>the result</returns>
        //switch to classical
        //we now split a and b into 2 parts:
        // f0 + f1
        //subtract f0g0, f1g1
        /* =========================== Exact multiplication with unsafe arithmetics =========================== */
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="result">where to write the result</param>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <returns>the result</returns>
        /// <summary>
        /// Karatsuba multiplication
        /// </summary>
        /// <param name="f">the first multiplier</param>
        /// <param name="g">the second multiplier</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <param name="gFrom">begin in g</param>
        /// <param name="gTo">end in g</param>
        /// <returns>the result</returns>
        // return zero
        // single element in f
        // single element in g
        //single element in b
        // linear factors
        //both a and b are linear
        //switch to classical
        //we now split a and b into 2 parts:
        //if we can't split b
        // f0 + f1
        //g0 + g1
        //subtract f0g0, f1g1
        /// <summary>
        /// classical square
        /// </summary>
        static long[] SquareClassicalUnsafe(long[] a, int from, int to)
        {
            long[] x = new long[(to - from) * 2 - 1];
            SquareClassicalUnsafe(x, a, from, to);
            return x;
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        /* =========================== Multiplication with safe arithmetic =========================== */
        /// <summary>
        /// switch to classical multiplication
        /// </summary>
        /// <summary>
        /// when use Karatsuba fast multiplication
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /* =========================== Exact multiplication with safe arithmetics =========================== */
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <returns>the result</returns>
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="result">where to write the result</param>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <summary>
        /// Karatsuba multiplication
        /// </summary>
        /// <param name="f">the first multiplier</param>
        /// <param name="g">the second multiplier</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <param name="gFrom">begin in g</param>
        /// <param name="gTo">end in g</param>
        /// <returns>the result</returns>
        // return zero
        // single element in f
        // single element in g
        //single element in b
        // linear factors
        //both a and b are linear
        //switch to classical
        //we now split a and b into 2 parts:
        //if we can't split b
        // f0 + f1
        //g0 + g1
        //subtract f0g0, f1g1
        /// <summary>
        /// Square the poly {@code data} using classical algorithm
        /// </summary>
        /// <param name="result">result destination</param>
        /// <param name="data">the data</param>
        /// <param name="from">data from</param>
        /// <param name="to">end point in the {@code data}</param>
        /// <summary>
        /// Karatsuba squaring
        /// </summary>
        /// <param name="f">the data</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <returns>the result</returns>
        //switch to classical
        //we now split a and b into 2 parts:
        // f0 + f1
        //subtract f0g0, f1g1
        /* =========================== Exact multiplication with unsafe arithmetics =========================== */
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="result">where to write the result</param>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <returns>the result</returns>
        /// <summary>
        /// Karatsuba multiplication
        /// </summary>
        /// <param name="f">the first multiplier</param>
        /// <param name="g">the second multiplier</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <param name="gFrom">begin in g</param>
        /// <param name="gTo">end in g</param>
        /// <returns>the result</returns>
        // return zero
        // single element in f
        // single element in g
        //single element in b
        // linear factors
        //both a and b are linear
        //switch to classical
        //we now split a and b into 2 parts:
        //if we can't split b
        // f0 + f1
        //g0 + g1
        //subtract f0g0, f1g1
        /// <summary>
        /// classical square
        /// </summary>
        /// <summary>
        /// Square the poly {@code data} using classical algorithm
        /// </summary>
        /// <param name="result">result destination</param>
        /// <param name="data">the data</param>
        /// <param name="from">data from</param>
        /// <param name="to">end point in the {@code data}</param>
        static void SquareClassicalUnsafe(long[] result, long[] data, int from, int to)
        {
            int len = to - from;
            for (int i = 0; i < len; ++i)
            {
                long c = data[from + i];
                if (c != 0)
                    for (int j = 0; j < len; ++j)
                        result[i + j] = result[i + j] + c * data[from + j];
            }
        }

        /// <summary>
        /// array of coefficients { x^0, x^1, ... , x^degree }
        /// </summary>
        /// <summary>
        /// points to the last non zero element in the data array
        /// </summary>
        /// <summary>
        /// casted self *
        /// </summary>
        /// <summary>
        /// Returns the i-th coefficient of this poly (coefficient before x^i)
        /// </summary>
        /// <summary>
        /// Sets i-th element of this poly with the specified value
        /// </summary>
        /// <summary>
        /// Sets hte leading coefficient of this poly with specified value
        /// </summary>
        /// <summary>
        /// Returns the leading coefficient of this poly
        /// </summary>
        /// <returns>leading coefficient</returns>
        /// <summary>
        /// Returns the constant coefficient of this poly
        /// </summary>
        /// <returns>constant coefficient</returns>
        // the rest will be filled with zeros
        /// <summary>
        /// Ensures that the capacity of internal storage is enough for storing polynomial of the {@code desiredDegree}. The
        /// degree of {@code this} is set to {@code desiredDegree} if the latter is greater than the former.
        /// </summary>
        /// <param name="desiredDegree">desired degree</param>
        /// <summary>
        /// Removes zeroes from the end of {@code data} and adjusts the degree
        /// </summary>
        // unnecessary clearing
        // Arrays.fill(data, degree + 1, data.length, 0);
        /// <summary>
        /// Converts this to a polynomial over BigIntegers
        /// </summary>
        /// <returns>polynomial over BigIntegers</returns>
        /// <summary>
        /// Creates new poly with the specified coefficients (over the same ring)
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}  (with the same coefficient ring)
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^degree} (with the same coefficient ring)
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="degree">monomial degree</param>
        /// <returns>{@code coefficient * x^degree}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value (with the same coefficient ring)
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>constant polynomial with specified value</returns>
        /// <summary>
        /// Returns L1 norm of this polynomial, i.e. sum of abs coefficients
        /// </summary>
        /// <returns>L1 norm of {@code this}</returns>
        /// <summary>
        /// Returns L2 norm of this polynomial, i.e. a square root of a sum of coefficient squares.
        /// </summary>
        /// <returns>L2 norm of {@code this}</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        /// <summary>
        /// Returns max coefficient (by absolute value) of this poly
        /// </summary>
        /// <returns>max coefficient (by absolute value)</returns>
        // destroy
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// addition in the coefficient ring *
        /// </summary>
        /// <summary>
        /// subtraction in the coefficient ring *
        /// </summary>
        /// <summary>
        /// multiplication in the coefficient ring *
        /// </summary>
        /// <summary>
        /// negation in the coefficient ring *
        /// </summary>
        /// <summary>
        /// convert long to element of this coefficient ring *
        /// </summary>
        /// <summary>
        /// Evaluates this poly at a given {@code point} (via Horner method).
        /// </summary>
        /// <param name="point">{@code point}</param>
        /// <returns>value at {@code point}</returns>
        /// <summary>
        /// Shifts variable x -> x + value and returns the result (new instance)
        /// </summary>
        /// <param name="value">shift amount</param>
        /// <returns>a copy of this with x -> x + value</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor};
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Add constant to this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Subtract constant from this.
        /// </summary>
        /// <param name="val">some number</param>
        /// <returns>this + val</returns>
        /// <summary>
        /// Adds {@code coefficient*x^exponent} to {@code this}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code this + coefficient*x^exponent}</returns>
        /// <summary>
        /// Adds {@code oth * factor} to {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this + oth * factor} modulo {@code modulus}</returns>
        /// <summary>
        /// Subtracts {@code factor * x^exponent * oth} from {@code this}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <param name="factor">the factor</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code this - factor * x^exponent * oth}</returns>
        /// <summary>
        /// convert this long[] data to BigInteger[]
        /// </summary>
        /// <summary>
        /// internal API >>> direct unsafe access to internal storage
        /// </summary>
        /// <summary>
        /// Returns a sequential {@code Stream} with coefficients of this as its source.
        /// </summary>
        /// <returns>a sequential {@code Stream} over the coefficients in this polynomial</returns>
        /// <summary>
        /// Applies transformation function to this and returns the result. This method is equivalent of {@code
        /// stream().mapToObj(mapper).collect(new PolynomialCollector<>(ring))}.
        /// </summary>
        /// <param name="ring">ring of the new polynomial</param>
        /// <param name="mapper">function that maps coefficients of this to coefficients of the result</param>
        /// <param name="<T>">result elements type</param>
        /// <returns>a new polynomial with the coefficients obtained from this by applying {@code mapper}</returns>
        /* =========================== Multiplication with safe arithmetic =========================== */
        /// <summary>
        /// switch to classical multiplication
        /// </summary>
        /// <summary>
        /// when use Karatsuba fast multiplication
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /// <summary>
        /// switch algorithms
        /// </summary>
        /* =========================== Exact multiplication with safe arithmetics =========================== */
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <returns>the result</returns>
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="result">where to write the result</param>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <summary>
        /// Karatsuba multiplication
        /// </summary>
        /// <param name="f">the first multiplier</param>
        /// <param name="g">the second multiplier</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <param name="gFrom">begin in g</param>
        /// <param name="gTo">end in g</param>
        /// <returns>the result</returns>
        // return zero
        // single element in f
        // single element in g
        //single element in b
        // linear factors
        //both a and b are linear
        //switch to classical
        //we now split a and b into 2 parts:
        //if we can't split b
        // f0 + f1
        //g0 + g1
        //subtract f0g0, f1g1
        /// <summary>
        /// Square the poly {@code data} using classical algorithm
        /// </summary>
        /// <param name="result">result destination</param>
        /// <param name="data">the data</param>
        /// <param name="from">data from</param>
        /// <param name="to">end point in the {@code data}</param>
        /// <summary>
        /// Karatsuba squaring
        /// </summary>
        /// <param name="f">the data</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <returns>the result</returns>
        //switch to classical
        //we now split a and b into 2 parts:
        // f0 + f1
        //subtract f0g0, f1g1
        /* =========================== Exact multiplication with unsafe arithmetics =========================== */
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="result">where to write the result</param>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <summary>
        /// Classical n*m multiplication algorithm
        /// </summary>
        /// <param name="a">the first multiplier</param>
        /// <param name="aFrom">begin in a</param>
        /// <param name="aTo">end in a</param>
        /// <param name="b">the second multiplier</param>
        /// <param name="bFrom">begin in b</param>
        /// <param name="bTo">end in b</param>
        /// <returns>the result</returns>
        /// <summary>
        /// Karatsuba multiplication
        /// </summary>
        /// <param name="f">the first multiplier</param>
        /// <param name="g">the second multiplier</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <param name="gFrom">begin in g</param>
        /// <param name="gTo">end in g</param>
        /// <returns>the result</returns>
        // return zero
        // single element in f
        // single element in g
        //single element in b
        // linear factors
        //both a and b are linear
        //switch to classical
        //we now split a and b into 2 parts:
        //if we can't split b
        // f0 + f1
        //g0 + g1
        //subtract f0g0, f1g1
        /// <summary>
        /// classical square
        /// </summary>
        /// <summary>
        /// Square the poly {@code data} using classical algorithm
        /// </summary>
        /// <param name="result">result destination</param>
        /// <param name="data">the data</param>
        /// <param name="from">data from</param>
        /// <param name="to">end point in the {@code data}</param>
        /// <summary>
        /// Karatsuba squaring
        /// </summary>
        /// <param name="f">the data</param>
        /// <param name="fFrom">begin in f</param>
        /// <param name="fTo">end in f</param>
        /// <returns>the result</returns>
        static long[] SquareKaratsubaUnsafe(long[] f, int fFrom, int fTo)
        {
            if (fFrom >= fTo)
                return new long[0];
            if (fTo - fFrom == 1)
                return new long[]
                {
                    f[fFrom] * f[fFrom]
                };
            if (fTo - fFrom == 2)
            {
                long[] result = new long[3];
                result[0] = f[fFrom] * f[fFrom];
                result[1] = 2 * f[fFrom] * f[fFrom + 1];
                result[2] = f[fFrom + 1] * f[fFrom + 1];
                return result;
            }


            //switch to classical
            if (1 * (fTo - fFrom) * (fTo - fFrom) < KARATSUBA_THRESHOLD)
                return SquareClassicalUnsafe(f, fFrom, fTo);

            //we now split a and b into 2 parts:
            int split = (fTo - fFrom + 1) / 2;
            int fMid = fFrom + split;
            long[] f0g0 = SquareKaratsubaUnsafe(f, fFrom, fMid);
            long[] f1g1 = SquareKaratsubaUnsafe(f, fMid, fTo);

            // f0 + f1
            long[] f0_plus_f1 = new long[Math.Max(fMid - fFrom, fTo - fMid)];
            System.Arraycopy(f, fFrom, f0_plus_f1, 0, fMid - fFrom);
            for (int i = fMid; i < fTo; ++i)
                f0_plus_f1[i - fMid] = f0_plus_f1[i - fMid] + f[i];
            long[] mid = SquareKaratsubaUnsafe(f0_plus_f1, 0, f0_plus_f1.Length);
            if (mid.Length < f0g0.Length)
                mid = Arrays.CopyOf(mid, f0g0.Length);
            if (mid.Length < f1g1.Length)
                mid = Arrays.CopyOf(mid, f1g1.Length);

            //subtract f0g0, f1g1
            for (int i = 0; i < f0g0.Length; ++i)
                mid[i] = mid[i] - f0g0[i];
            for (int i = 0; i < f1g1.Length; ++i)
                mid[i] = mid[i] - f1g1[i];
            long[] result = Arrays.CopyOf(f0g0, 2 * (fTo - fFrom) - 1);
            for (int i = 0; i < mid.Length; ++i)
                result[i + split] = result[i + split] + mid[i];
            for (int i = 0; i < f1g1.Length; ++i)
                result[i + 2 * split] = result[i + 2 * split] + f1g1[i];
            return result;
        }
    }
}