using Cc.Redberry.Libdivide4j.FastDivision;
using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Io;
using Cc.Redberry.Rings.Poly;
using Cc.Redberry.Rings.Poly.Multivar;
using Java.Util;
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
    /// Univariate polynomial over machine integers in range [-2^63, 2^63]. <b>NOTE:</b> this class is used in internal
    /// routines for performance reasons, for usual polynomials over Z use {@link UnivariatePolynomial} over BigIntegers.
    /// 
    /// <p> Arithmetic operations on instances of this type may cause long overflow in which case a proper {@link
    /// ArithmeticException} will be thrown.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class UnivariatePolynomialZ64 : AUnivariatePolynomial64<UnivariatePolynomialZ64>
    {
        private static readonly long serialVersionUID = 1;
        /// <summary>
        /// main constructor
        /// </summary>
        private UnivariatePolynomialZ64(long[] data)
        {
            this.data = data;
            this.degree = data.Length - 1;
            FixDegree();
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        private UnivariatePolynomialZ64(long[] data, int degree)
        {
            this.data = data;
            this.degree = degree;
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        public static UnivariatePolynomialZ64 Parse(string @string)
        {
            return UnivariatePolynomial.AsOverZ64(UnivariatePolynomial.Parse(@string, Rings.Z));
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        public static UnivariatePolynomialZ64 Create(params long[] data)
        {
            return new UnivariatePolynomialZ64(data);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        public static UnivariatePolynomialZ64 Monomial(long coefficient, int exponent)
        {
            long[] data = new long[exponent + 1];
            data[exponent] = coefficient;
            return new UnivariatePolynomialZ64(data);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        public static UnivariatePolynomialZ64 Zero()
        {
            return new UnivariatePolynomialZ64(new long[] { 0 }, 0);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        public static UnivariatePolynomialZ64 One()
        {
            return new UnivariatePolynomialZ64(new long[] { 1 }, 0);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        public static UnivariatePolynomialZ64 Constant(long value)
        {
            return new UnivariatePolynomialZ64(new long[] { value }, 0);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        public override UnivariatePolynomialZ64 SetCoefficientRingFrom(UnivariatePolynomialZ64 univariatePolynomialZ64)
        {
            return univariatePolynomialZ64.Clone();
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        public UnivariatePolynomialZp64 Modulus(long modulus, bool copy)
        {
            return UnivariatePolynomialZp64.Create(modulus, copy ? data.Clone() : data);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        public UnivariatePolynomialZp64 Modulus(long modulus)
        {
            return Modulus(modulus, true);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        public UnivariatePolynomialZp64 Modulus(IntegersZp64 ring, bool copy)
        {
            long[] data = copy ? this.data.Clone() : this.data;
            for (int i = degree; i >= 0; --i)
                data[i] = ring.Modulus(data[i]);
            return UnivariatePolynomialZp64.CreateUnsafe(ring, data);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        public UnivariatePolynomialZp64 Modulus(IntegersZp64 ring)
        {
            return Modulus(ring, true);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        UnivariatePolynomialZp64 ModulusUnsafe(long modulus)
        {
            return UnivariatePolynomialZp64.CreateUnsafe(modulus, data);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        public override UnivariatePolynomial<BigInteger> ToBigPoly()
        {
            return UnivariatePolynomial.CreateUnsafe(Rings.Z, DataToBigIntegers());
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        public double MignotteBound()
        {
            return Math.Pow(2, degree) * Norm2();
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        public long EvaluateAtRational(long num, long den)
        {
            if (num == 0)
                return Cc();
            long res = 0;
            Magic magic = MagicSigned(den);
            for (int i = degree; i >= 0; --i)
            {
                long x = Multiply(res, num);
                long q = DivideSignedFast(x, magic);
                if (q * den != x)
                    throw new ArgumentException("The answer is not integer");
                res = Add(q, data[i]);
            }

            return res;
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        public override UnivariatePolynomialZ64 GetRange(int from, int to)
        {
            return new UnivariatePolynomialZ64(Arrays.CopyOfRange(data, from, to));
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        public override UnivariatePolynomialZ64[] CreateArray(int length)
        {
            return new UnivariatePolynomialZ64[length];
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        public override UnivariatePolynomialZ64[][] CreateArray2d(int length)
        {
            return new UnivariatePolynomialZ64[length];
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        public override UnivariatePolynomialZ64[][] CreateArray2d(int length1, int length2)
        {
            return new UnivariatePolynomialZ64[length1, length2];
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        public override bool SameCoefficientRingWith(UnivariatePolynomialZ64 oth)
        {
            return true;
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        public override UnivariatePolynomialZ64 CreateFromArray(long[] data)
        {
            return new UnivariatePolynomialZ64(data);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        public override UnivariatePolynomialZ64 CreateMonomial(long coefficient, int degree)
        {
            return Monomial(coefficient, degree);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        public override bool IsOverField()
        {
            return false;
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        public override bool IsOverFiniteField()
        {
            return false;
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        public override bool IsOverZ()
        {
            return true;
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        public override BigInteger CoefficientRingCardinality()
        {
            return null;
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        public override BigInteger CoefficientRingCharacteristic()
        {
            return BigInteger.ZERO;
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        public override bool IsOverPerfectPower()
        {
            return false;
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        public override BigInteger CoefficientRingPerfectPowerBase()
        {
            return null;
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        public override BigInteger CoefficientRingPerfectPowerExponent()
        {
            return null;
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        public override long Content()
        {
            if (degree == 0)
                return data[0];
            return MachineArithmetic.Gcd(data, 0, degree + 1);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        override long Add(long a, long b)
        {
            return MachineArithmetic.SafeAdd(a, b);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        override long Subtract(long a, long b)
        {
            return MachineArithmetic.SafeSubtract(a, b);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        override long Multiply(long a, long b)
        {
            return MachineArithmetic.SafeMultiply(a, b);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        override long Negate(long a)
        {
            return MachineArithmetic.SafeNegate(a);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        override long ValueOf(long a)
        {
            return a;
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        public override UnivariatePolynomialZ64 Monic()
        {
            if (IsZero())
                return this;
            return DivideOrNull(Lc());
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        public override UnivariatePolynomialZ64 Monic(long factor)
        {
            long lc = Lc();
            long gcd = MachineArithmetic.Gcd(lc, factor);
            factor = factor / gcd;
            lc = lc / gcd;
            UnivariatePolynomialZ64 r = DivideOrNull(lc);
            if (r == null)
                return null;
            return r.Multiply(factor);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// Divides this polynomial by a {@code factor} or returns {@code null} (causing loss of internal data) if some of
        /// the elements can't be exactly divided by the {@code factor}. NOTE: is {@code null} is returned, the content of
        /// {@code this} is destroyed.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this} divided by the {@code factor} or {@code null}</returns>
        public UnivariatePolynomialZ64 DivideOrNull(long factor)
        {
            if (factor == 0)
                throw new ArithmeticException("Divide by zero");
            if (factor == 1)
                return this;
            Magic magic = MagicSigned(factor);
            for (int i = degree; i >= 0; --i)
            {
                long l = DivideSignedFast(data[i], magic);
                if (l * factor != data[i])
                    return null;
                data[i] = l;
            }

            return this;
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// Divides this polynomial by a {@code factor} or returns {@code null} (causing loss of internal data) if some of
        /// the elements can't be exactly divided by the {@code factor}. NOTE: is {@code null} is returned, the content of
        /// {@code this} is destroyed.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this} divided by the {@code factor} or {@code null}</returns>
        public override UnivariatePolynomialZ64 DivideByLC(UnivariatePolynomialZ64 other)
        {
            return DivideOrNull(other.Lc());
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// Divides this polynomial by a {@code factor} or returns {@code null} (causing loss of internal data) if some of
        /// the elements can't be exactly divided by the {@code factor}. NOTE: is {@code null} is returned, the content of
        /// {@code this} is destroyed.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this} divided by the {@code factor} or {@code null}</returns>
        public override UnivariatePolynomialZ64 MultiplyByBigInteger(BigInteger factor)
        {
            return Multiply(factor.LongValueExact());
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// Divides this polynomial by a {@code factor} or returns {@code null} (causing loss of internal data) if some of
        /// the elements can't be exactly divided by the {@code factor}. NOTE: is {@code null} is returned, the content of
        /// {@code this} is destroyed.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this} divided by the {@code factor} or {@code null}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        public UnivariatePolynomialZ64 MultiplyUnsafe(long factor)
        {
            for (int i = degree; i >= 0; --i)
                data[i] *= factor;
            return this;
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// Divides this polynomial by a {@code factor} or returns {@code null} (causing loss of internal data) if some of
        /// the elements can't be exactly divided by the {@code factor}. NOTE: is {@code null} is returned, the content of
        /// {@code this} is destroyed.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this} divided by the {@code factor} or {@code null}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        public override UnivariatePolynomialZ64 Multiply(UnivariatePolynomialZ64 oth)
        {
            if (IsZero())
                return this;
            if (oth.IsZero())
                return ToZero();
            if (this == oth)
                return Square();
            if (oth.degree == 0)
                return Multiply(oth.data[0]);
            if (degree == 0)
            {
                long factor = data[0];
                data = oth.data.Clone();
                degree = oth.degree;
                return Multiply(factor);
            }

            double rBound = NormMax() * oth.NormMax() * Math.Max(degree + 1, oth.degree + 1);
            if (rBound < Long.MAX_VALUE)

                // we can apply fast integer arithmetic
                data = MultiplyUnsafe0(oth);
            else
                data = MultiplySafe0(oth);
            degree += oth.degree;
            FixDegree();
            return this;
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// Divides this polynomial by a {@code factor} or returns {@code null} (causing loss of internal data) if some of
        /// the elements can't be exactly divided by the {@code factor}. NOTE: is {@code null} is returned, the content of
        /// {@code this} is destroyed.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this} divided by the {@code factor} or {@code null}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        // we can apply fast integer arithmetic
        /// <summary>
        /// internal API
        /// </summary>
        public UnivariatePolynomialZ64 MultiplyUnsafe(UnivariatePolynomialZ64 oth)
        {
            if (IsZero())
                return this;
            if (oth.IsZero())
                return ToZero();
            if (this == oth)
                return Square();
            if (oth.degree == 0)
                return Multiply(oth.data[0]);
            if (degree == 0)
            {
                long factor = data[0];
                data = oth.data.Clone();
                degree = oth.degree;
                return MultiplyUnsafe(factor);
            }

            data = MultiplyUnsafe0(oth);
            degree += oth.degree;
            FixDegree();
            return this;
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// Divides this polynomial by a {@code factor} or returns {@code null} (causing loss of internal data) if some of
        /// the elements can't be exactly divided by the {@code factor}. NOTE: is {@code null} is returned, the content of
        /// {@code this} is destroyed.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this} divided by the {@code factor} or {@code null}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        // we can apply fast integer arithmetic
        /// <summary>
        /// internal API
        /// </summary>
        public override UnivariatePolynomialZ64 Square()
        {
            if (IsZero())
                return this;
            if (degree == 0)
                return Multiply(data[0]);
            double norm1 = NormMax();
            double rBound = norm1 * norm1 * (degree + 1);
            if (rBound < Long.MAX_VALUE)

                // we can apply fast integer arithmetic
                data = SquareUnsafe0();
            else
                data = SquareSafe0();
            degree += degree;
            FixDegree();
            return this;
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// Divides this polynomial by a {@code factor} or returns {@code null} (causing loss of internal data) if some of
        /// the elements can't be exactly divided by the {@code factor}. NOTE: is {@code null} is returned, the content of
        /// {@code this} is destroyed.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this} divided by the {@code factor} or {@code null}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        // we can apply fast integer arithmetic
        /// <summary>
        /// internal API
        /// </summary>
        // we can apply fast integer arithmetic
        public override UnivariatePolynomialZ64 Derivative()
        {
            if (IsConstant())
                return CreateZero();
            long[] newData = new long[degree];
            for (int i = degree; i > 0; --i)
                newData[i - 1] = Multiply(data[i], i);
            return CreateFromArray(newData);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// Divides this polynomial by a {@code factor} or returns {@code null} (causing loss of internal data) if some of
        /// the elements can't be exactly divided by the {@code factor}. NOTE: is {@code null} is returned, the content of
        /// {@code this} is destroyed.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this} divided by the {@code factor} or {@code null}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        // we can apply fast integer arithmetic
        /// <summary>
        /// internal API
        /// </summary>
        // we can apply fast integer arithmetic
        public override UnivariatePolynomialZ64 Clone()
        {
            return new UnivariatePolynomialZ64(data.Clone(), degree);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// Divides this polynomial by a {@code factor} or returns {@code null} (causing loss of internal data) if some of
        /// the elements can't be exactly divided by the {@code factor}. NOTE: is {@code null} is returned, the content of
        /// {@code this} is destroyed.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this} divided by the {@code factor} or {@code null}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        // we can apply fast integer arithmetic
        /// <summary>
        /// internal API
        /// </summary>
        // we can apply fast integer arithmetic
        public override UnivariatePolynomialZ64 ParsePoly(string @string)
        {
            return Parse(@string);
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// Divides this polynomial by a {@code factor} or returns {@code null} (causing loss of internal data) if some of
        /// the elements can't be exactly divided by the {@code factor}. NOTE: is {@code null} is returned, the content of
        /// {@code this} is destroyed.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this} divided by the {@code factor} or {@code null}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        // we can apply fast integer arithmetic
        /// <summary>
        /// internal API
        /// </summary>
        // we can apply fast integer arithmetic
        public override string CoefficientRingToString(IStringifier<UnivariatePolynomialZ64> stringifier)
        {
            return "Z";
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// Divides this polynomial by a {@code factor} or returns {@code null} (causing loss of internal data) if some of
        /// the elements can't be exactly divided by the {@code factor}. NOTE: is {@code null} is returned, the content of
        /// {@code this} is destroyed.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this} divided by the {@code factor} or {@code null}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        // we can apply fast integer arithmetic
        /// <summary>
        /// internal API
        /// </summary>
        // we can apply fast integer arithmetic
        public override AMultivariatePolynomial Composition(AMultivariatePolynomial value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// main constructor
        /// </summary>
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates Z[x] polynomial from the specified coefficients
        /// </summary>
        /// <param name="data">coefficients</param>
        /// <returns>Z[x] polynomial</returns>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns constant with specified value
        /// </summary>
        /// <returns>constant with specified value</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces this polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <param name="copy">whether to copy the internal data or reduce inplace (in which case the data of this will be lost)</param>
        /// <returns>this modulo {@code modulus}</returns>
        /// <summary>
        /// Reduces (copied) polynomial modulo {@code modulus} and returns the result.
        /// </summary>
        /// <param name="ring">the modulus</param>
        /// <returns>a copy of this modulo {@code modulus}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// {@inheritDoc}. The ring of the result is {@link Rings#Z}
        /// </summary>
        /// <summary>
        /// Returns Mignotte's bound (sqrt(n+1) * 2^n max |this|)
        /// </summary>
        /// <returns>Mignotte's bound</returns>
        /// <summary>
        /// Evaluates this poly at a given rational point {@code num/den}
        /// </summary>
        /// <param name="num">point numerator</param>
        /// <param name="den">point denominator</param>
        /// <returns>value at {@code num/den}</returns>
        /// <exception cref="ArithmeticException">if the result is not integer</exception>
        /// <summary>
        /// Returns the content of this poly (gcd of its coefficients)
        /// </summary>
        /// <returns>polynomial content</returns>
        /// <summary>
        /// Divides this polynomial by a {@code factor} or returns {@code null} (causing loss of internal data) if some of
        /// the elements can't be exactly divided by the {@code factor}. NOTE: is {@code null} is returned, the content of
        /// {@code this} is destroyed.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this} divided by the {@code factor} or {@code null}</returns>
        /// <summary>
        /// internal API
        /// </summary>
        // we can apply fast integer arithmetic
        /// <summary>
        /// internal API
        /// </summary>
        // we can apply fast integer arithmetic
        public override AMultivariatePolynomial AsMultivariate(Comparator<DegreeVector> ordering)
        {
            throw new NotSupportedException();
        }
    }
}