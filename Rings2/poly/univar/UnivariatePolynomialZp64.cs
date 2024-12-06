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
    /// Univariate polynomial over Zp ring with modulus in the range of {@code [2, 2^62) } (the last value is specified by
    /// {@link MachineArithmetic#MAX_SUPPORTED_MODULUS_BITS}. Fast methods from {@link IntegersZp64} are used to perform all
    /// arithmetic operations.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class UnivariatePolynomialZp64 : AUnivariatePolynomial64<UnivariatePolynomialZp64>
    {
        private static readonly long serialVersionUID = 1;
        /// <summary>
        /// The coefficient ring
        /// </summary>
        public readonly IntegersZp64 ring;
        /// <summary>
        /// The coefficient ring
        /// </summary>
        private UnivariatePolynomialZp64(IntegersZp64 ring, long[] data, int degree)
        {
            this.ring = ring;
            this.data = data;
            this.degree = degree;
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        private UnivariatePolynomialZp64(IntegersZp64 ring, long[] data) : this(ring, data, data.Length - 1)
        {
            FixDegree();
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        private static void CheckModulus(long modulus)
        {
            if (Long.Compare(modulus, MachineArithmetic.MAX_SUPPORTED_MODULUS) > 0)
                throw new ArgumentException("Too large modulus. Modulus should be less than 2^" + MachineArithmetic.MAX_SUPPORTED_MODULUS_BITS);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        public static UnivariatePolynomialZp64 Parse(string @string, long modulus)
        {
            return Parse(@string, new IntegersZp64(modulus));
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        public static UnivariatePolynomialZp64 Parse(string @string, IntegersZp64 modulus)
        {
            return UnivariatePolynomial.AsOverZp64(UnivariatePolynomial.Parse(@string, modulus.AsGenericRing()));
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        public static UnivariatePolynomialZp64 Parse(string @string, IntegersZp64 modulus, string variable)
        {
            return UnivariatePolynomial.AsOverZp64(UnivariatePolynomial.Parse(@string, modulus.AsGenericRing(), variable));
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        public static UnivariatePolynomialZp64 Create(long modulus, long[] data)
        {
            return Create(new IntegersZp64(modulus), data);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        public static UnivariatePolynomialZp64 Create(IntegersZp64 ring, long[] data)
        {
            ring.Modulus(data);
            return new UnivariatePolynomialZp64(ring, data);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        public static UnivariatePolynomialZp64 Linear(long cc, long lc, long modulus)
        {
            return Create(modulus, new long[] { cc, lc });
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        public static UnivariatePolynomialZp64 CreateUnsafe(long modulus, long[] data)
        {
            return new UnivariatePolynomialZp64(new IntegersZp64(modulus), data);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        public static UnivariatePolynomialZp64 CreateUnsafe(IntegersZp64 ring, long[] data)
        {
            return new UnivariatePolynomialZp64(ring, data);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        public static UnivariatePolynomialZp64 Monomial(long modulus, long coefficient, int exponent)
        {
            IntegersZp64 ring = new IntegersZp64(modulus);
            coefficient = ring.Modulus(coefficient);
            long[] data = new long[exponent + 1];
            data[exponent] = coefficient;
            return new UnivariatePolynomialZp64(ring, data);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        public static UnivariatePolynomialZp64 Constant(long modulus, long value)
        {
            return Constant(new IntegersZp64(modulus), value);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        public static UnivariatePolynomialZp64 Constant(IntegersZp64 ring, long value)
        {
            return new UnivariatePolynomialZp64(ring, new long[] { ring.Modulus(value) }, 0);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        public static UnivariatePolynomialZp64 Zero(long modulus)
        {
            return Constant(modulus, 0);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        public static UnivariatePolynomialZp64 Zero(IntegersZp64 ring)
        {
            return new UnivariatePolynomialZp64(ring, new long[] { 0 }, 0);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        public static UnivariatePolynomialZp64 One(long modulus)
        {
            return Constant(modulus, 1);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        public static UnivariatePolynomialZp64 One(IntegersZp64 ring)
        {
            return new UnivariatePolynomialZp64(ring, new long[] { 1 }, 0);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        public override UnivariatePolynomialZp64 SetCoefficientRingFrom(UnivariatePolynomialZp64 univariatePolynomialZp64)
        {
            return SetModulus(univariatePolynomialZp64.ring);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        public long Modulus()
        {
            return ring.modulus;
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        public UnivariatePolynomialZp64 SetModulusUnsafe(long newModulus)
        {
            return SetModulusUnsafe(new IntegersZp64(newModulus));
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        public UnivariatePolynomialZp64 SetModulusUnsafe(IntegersZp64 newModulus)
        {
            return new UnivariatePolynomialZp64(newModulus, data, degree);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        public UnivariatePolynomialZp64 SetModulus(long newModulus)
        {
            long[] newData = data.Clone();
            IntegersZp64 newDomain = new IntegersZp64(newModulus);
            newDomain.Modulus(newData);
            return new UnivariatePolynomialZp64(newDomain, newData);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        public UnivariatePolynomialZp64 SetModulus(IntegersZp64 newDomain)
        {
            long[] newData = data.Clone();
            newDomain.Modulus(newData);
            return new UnivariatePolynomialZp64(newDomain, newData);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        public UnivariatePolynomialZ64 AsPolyZSymmetric()
        {
            long[] newData = new long[degree + 1];
            for (int i = degree; i >= 0; --i)
                newData[i] = ring.SymmetricForm(data[i]);
            return UnivariatePolynomialZ64.Create(newData);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        public UnivariatePolynomialZ64 AsPolyZ(bool copy)
        {
            return UnivariatePolynomialZ64.Create(copy ? data.Clone() : data);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        public override UnivariatePolynomialZp64[] CreateArray(int length)
        {
            return new UnivariatePolynomialZp64[length];
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        public override UnivariatePolynomialZp64[] CreateArray(UnivariatePolynomialZp64 a, UnivariatePolynomialZp64 b)
        {
            return new UnivariatePolynomialZp64[]
            {
                a,
                b
            };
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        public override UnivariatePolynomialZp64[][] CreateArray2d(int length)
        {
            return new UnivariatePolynomialZp64[length];
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        public override UnivariatePolynomialZp64[][] CreateArray2d(int length1, int length2)
        {
            return new UnivariatePolynomialZp64[length1, length2];
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        public override UnivariatePolynomialZp64 GetRange(int from, int to)
        {
            return new UnivariatePolynomialZp64(ring, Arrays.CopyOfRange(data, from, to));
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        public override bool SameCoefficientRingWith(UnivariatePolynomialZp64 oth)
        {
            return ring.modulus == oth.ring.modulus;
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        public override UnivariatePolynomialZp64 CreateFromArray(long[] newData)
        {
            ring.Modulus(newData);
            return new UnivariatePolynomialZp64(ring, newData);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        public override UnivariatePolynomialZp64 CreateMonomial(long coefficient, int newDegree)
        {
            long[] newData = new long[newDegree + 1];
            newData[newDegree] = ValueOf(coefficient);
            return new UnivariatePolynomialZp64(ring, newData, newDegree);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        public override bool IsOverField()
        {
            return true;
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        public override bool IsOverFiniteField()
        {
            return true;
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        public override bool IsOverZ()
        {
            return false;
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        public override BigInteger CoefficientRingCardinality()
        {
            return BigInteger.ValueOf(Modulus());
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        public override BigInteger CoefficientRingCharacteristic()
        {
            return BigInteger.ValueOf(Modulus());
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        public override bool IsOverPerfectPower()
        {
            return ring.IsPerfectPower();
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        public override BigInteger CoefficientRingPerfectPowerBase()
        {
            return BigInteger.ValueOf(ring.PerfectPowerBase());
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        public override BigInteger CoefficientRingPerfectPowerExponent()
        {
            return BigInteger.ValueOf(ring.PerfectPowerExponent());
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        public override long Content()
        {
            return Lc();
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        override long Add(long a, long b)
        {
            return ring.Add(a, b);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        override long Subtract(long a, long b)
        {
            return ring.Subtract(a, b);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        override long Multiply(long a, long b)
        {
            return ring.Multiply(a, b);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        override long Negate(long a)
        {
            return ring.Negate(a);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        override long ValueOf(long a)
        {
            return ring.Modulus(a);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        public override UnivariatePolynomialZp64 Monic()
        {
            if (IsMonic())
                return this;
            if (IsZero())
                return this;
            if (degree == 0)
            {
                data[0] = 1;
                return this;
            }

            return Multiply(ring.Reciprocal(Lc()));
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor} (that is {@code
        /// monic(modulus).multiply(factor)} ).
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        public override UnivariatePolynomialZp64 Monic(long factor)
        {
            return Multiply(Multiply(ValueOf(factor), ring.Reciprocal(Lc())));
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor} (that is {@code
        /// monic(modulus).multiply(factor)} ).
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        public override UnivariatePolynomialZp64 DivideByLC(UnivariatePolynomialZp64 other)
        {
            return Divide(other.Lc());
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor} (that is {@code
        /// monic(modulus).multiply(factor)} ).
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Divide by specified value
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>{@code this / val}</returns>
        public UnivariatePolynomialZp64 Divide(long val)
        {
            return Multiply(ring.Reciprocal(val));
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor} (that is {@code
        /// monic(modulus).multiply(factor)} ).
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Divide by specified value
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>{@code this / val}</returns>
        public override UnivariatePolynomialZp64 MultiplyByBigInteger(BigInteger factor)
        {
            return Multiply(factor.Mod(BigInteger.ValueOf(Modulus())).LongValueExact());
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor} (that is {@code
        /// monic(modulus).multiply(factor)} ).
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Divide by specified value
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>{@code this / val}</returns>
        public override UnivariatePolynomialZp64 Multiply(UnivariatePolynomialZp64 oth)
        {
            AssertSameCoefficientRingWith(oth);
            if (IsZero())
                return this;
            if (oth.IsZero())
                return ToZero();
            if (this == oth)
                return Square();
            AssertSameCoefficientRingWith(oth);
            if (oth.degree == 0)
                return Multiply(oth.data[0]);
            if (degree == 0)
            {
                long factor = data[0];
                this.Set(oth);
                return Multiply(factor);
            }

            double rBound = NormMax() * oth.NormMax() * Math.Max(degree + 1, oth.degree + 1);
            if (rBound < Long.MAX_VALUE)
            {

                // we can apply fast integer arithmetic and then reduce
                data = MultiplyUnsafe0(oth);
                degree += oth.degree;
                ring.Modulus(data);
                FixDegree();
            }
            else
            {
                data = MultiplySafe0(oth);
                degree += oth.degree;
                FixDegree();
            }

            return this;
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor} (that is {@code
        /// monic(modulus).multiply(factor)} ).
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Divide by specified value
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>{@code this / val}</returns>
        // we can apply fast integer arithmetic and then reduce
        public override UnivariatePolynomialZp64 Square()
        {
            if (IsZero())
                return this;
            if (degree == 0)
                return Multiply(data[0]);
            double norm1 = NormMax();
            double rBound = norm1 * norm1 * (degree + 1);
            if (rBound < Long.MAX_VALUE)
            {

                // we can apply fast integer arithmetic and then reduce
                data = SquareUnsafe0();
                degree += degree;
                ring.Modulus(data);
                FixDegree();
            }
            else
            {
                data = SquareSafe0();
                degree += degree;
                FixDegree();
            }

            return this;
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor} (that is {@code
        /// monic(modulus).multiply(factor)} ).
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Divide by specified value
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>{@code this / val}</returns>
        // we can apply fast integer arithmetic and then reduce
        // we can apply fast integer arithmetic and then reduce
        public override UnivariatePolynomialZp64 Derivative()
        {
            if (IsConstant())
                return CreateZero();
            long[] newData = new long[degree];
            if (degree < ring.modulus)
                for (int i = degree; i > 0; --i)
                    newData[i - 1] = Multiply(data[i], i);
            else
            {
                int i = degree;
                for (; i >= ring.modulus; --i)
                    newData[i - 1] = Multiply(data[i], ValueOf(i));
                for (; i > 0; --i)
                    newData[i - 1] = Multiply(data[i], i);
            }

            return new UnivariatePolynomialZp64(ring, newData);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor} (that is {@code
        /// monic(modulus).multiply(factor)} ).
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Divide by specified value
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>{@code this / val}</returns>
        // we can apply fast integer arithmetic and then reduce
        // we can apply fast integer arithmetic and then reduce
        /// <summary>
        /// {@inheritDoc}. The ring of the result will be exactly those returned by {@code this.ring.asGenericRing() }
        /// </summary>
        public override UnivariatePolynomial<BigInteger> ToBigPoly()
        {
            return UnivariatePolynomial.CreateUnsafe(new IntegersZp(ring.modulus), DataToBigIntegers());
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor} (that is {@code
        /// monic(modulus).multiply(factor)} ).
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Divide by specified value
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>{@code this / val}</returns>
        // we can apply fast integer arithmetic and then reduce
        // we can apply fast integer arithmetic and then reduce
        /// <summary>
        /// {@inheritDoc}. The ring of the result will be exactly those returned by {@code this.ring.asGenericRing() }
        /// </summary>
        public override UnivariatePolynomialZp64 Clone()
        {
            return new UnivariatePolynomialZp64(ring, data.Clone(), degree);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor} (that is {@code
        /// monic(modulus).multiply(factor)} ).
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Divide by specified value
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>{@code this / val}</returns>
        // we can apply fast integer arithmetic and then reduce
        // we can apply fast integer arithmetic and then reduce
        /// <summary>
        /// {@inheritDoc}. The ring of the result will be exactly those returned by {@code this.ring.asGenericRing() }
        /// </summary>
        public override UnivariatePolynomialZp64 ParsePoly(string @string)
        {
            return UnivariatePolynomialZ64.Parse(@string).Modulus(ring);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor} (that is {@code
        /// monic(modulus).multiply(factor)} ).
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Divide by specified value
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>{@code this / val}</returns>
        // we can apply fast integer arithmetic and then reduce
        // we can apply fast integer arithmetic and then reduce
        /// <summary>
        /// {@inheritDoc}. The ring of the result will be exactly those returned by {@code this.ring.asGenericRing() }
        /// </summary>
        override void MultiplyClassicalSafe(long[] result, long[] a, int aFrom, int aTo, long[] b, int bFrom, int bTo)
        {
            if (ring.modulusFits32)
                MultiplyClassicalSafeTrick(result, a, aFrom, aTo, b, bFrom, bTo);
            else
                base.MultiplyClassicalSafe(result, a, aFrom, aTo, b, bFrom, bTo);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor} (that is {@code
        /// monic(modulus).multiply(factor)} ).
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Divide by specified value
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>{@code this / val}</returns>
        // we can apply fast integer arithmetic and then reduce
        // we can apply fast integer arithmetic and then reduce
        /// <summary>
        /// {@inheritDoc}. The ring of the result will be exactly those returned by {@code this.ring.asGenericRing() }
        /// </summary>
        void MultiplyClassicalSafeNoTrick(long[] result, long[] a, int aFrom, int aTo, long[] b, int bFrom, int bTo)
        {
            base.MultiplyClassicalSafe(result, a, aFrom, aTo, b, bFrom, bTo);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor} (that is {@code
        /// monic(modulus).multiply(factor)} ).
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Divide by specified value
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>{@code this / val}</returns>
        // we can apply fast integer arithmetic and then reduce
        // we can apply fast integer arithmetic and then reduce
        /// <summary>
        /// {@inheritDoc}. The ring of the result will be exactly those returned by {@code this.ring.asGenericRing() }
        /// </summary>
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
        void MultiplyClassicalSafeTrick(long[] result, long[] a, int aFrom, int aTo, long[] b, int bFrom, int bTo)
        {

            // this trick is taken from Seyed Mohammad Mahdi Javadi PhD
            // thesis "EFFICIENT ALGORITHMS FOR COMPUTATIONS WITH SPARSE POLYNOMIALS" page 79
            // This trick works only when modulus (so as all elements in arrays) is less
            // than 2^32 (so modulus*modulus fits machine word).
            if (aTo - aFrom > bTo - bFrom)
            {
                MultiplyClassicalSafeTrick(result, b, bFrom, bTo, a, aFrom, aTo);
                return;
            }

            long p2 = ring.modulus * ring.modulus; // this is safe multiplication
            int aDegree = aTo - aFrom - 1, bDegree = bTo - bFrom - 1, resultDegree = aDegree + bDegree;
            for (int i = 0; i <= resultDegree; ++i)
            {
                long acc = 0;
                for (int j = Math.max(0, i - bDegree), to = Math.min(i, aDegree); j <= to; ++j)
                {
                    if (acc > 0)
                        acc = acc - p2;
                    acc = acc + a[aFrom + j] * b[bFrom + i - j];
                }

                result[i] = ring.Modulus(acc);
            }
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor} (that is {@code
        /// monic(modulus).multiply(factor)} ).
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Divide by specified value
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>{@code this / val}</returns>
        // we can apply fast integer arithmetic and then reduce
        // we can apply fast integer arithmetic and then reduce
        /// <summary>
        /// {@inheritDoc}. The ring of the result will be exactly those returned by {@code this.ring.asGenericRing() }
        /// </summary>
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
        // this trick is taken from Seyed Mohammad Mahdi Javadi PhD
        // thesis "EFFICIENT ALGORITHMS FOR COMPUTATIONS WITH SPARSE POLYNOMIALS" page 79
        // This trick works only when modulus (so as all elements in arrays) is less
        // than 2^32 (so modulus*modulus fits machine word).
        // this is safe multiplication
        public override string CoefficientRingToString(IStringifier<UnivariatePolynomialZp64> stringifier)
        {
            return ring.ToString();
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor} (that is {@code
        /// monic(modulus).multiply(factor)} ).
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Divide by specified value
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>{@code this / val}</returns>
        // we can apply fast integer arithmetic and then reduce
        // we can apply fast integer arithmetic and then reduce
        /// <summary>
        /// {@inheritDoc}. The ring of the result will be exactly those returned by {@code this.ring.asGenericRing() }
        /// </summary>
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
        // this trick is taken from Seyed Mohammad Mahdi Javadi PhD
        // thesis "EFFICIENT ALGORITHMS FOR COMPUTATIONS WITH SPARSE POLYNOMIALS" page 79
        // This trick works only when modulus (so as all elements in arrays) is less
        // than 2^32 (so modulus*modulus fits machine word).
        // this is safe multiplication
        public override MultivariatePolynomialZp64 Composition(AMultivariatePolynomial value)
        {
            if (!(value is MultivariatePolynomialZp64))
                throw new ArgumentException();
            if (value.IsOne())
                return AsMultivariate();
            if (value.IsZero())
                return CcAsPoly().AsMultivariate();
            MultivariatePolynomialZp64 result = (MultivariatePolynomialZp64)value.CreateZero();
            for (int i = degree; i >= 0; --i)
                result = result.Multiply((MultivariatePolynomialZp64)value).Add(data[i]);
            return result;
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor} (that is {@code
        /// monic(modulus).multiply(factor)} ).
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Divide by specified value
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>{@code this / val}</returns>
        // we can apply fast integer arithmetic and then reduce
        // we can apply fast integer arithmetic and then reduce
        /// <summary>
        /// {@inheritDoc}. The ring of the result will be exactly those returned by {@code this.ring.asGenericRing() }
        /// </summary>
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
        // this trick is taken from Seyed Mohammad Mahdi Javadi PhD
        // thesis "EFFICIENT ALGORITHMS FOR COMPUTATIONS WITH SPARSE POLYNOMIALS" page 79
        // This trick works only when modulus (so as all elements in arrays) is less
        // than 2^32 (so modulus*modulus fits machine word).
        // this is safe multiplication
        public override MultivariatePolynomialZp64 AsMultivariate()
        {
            return AsMultivariate(MonomialOrder.DEFAULT);
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <remarks>@deprecateduse {@link #parse(String, IntegersZp64, String)}</remarks>
        /// <summary>
        /// Parse string into polynomial
        /// </summary>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates poly with specified coefficients represented as signed integers reducing them modulo {@code modulus}
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="data">coefficients</param>
        /// <returns>the polynomial</returns>
        /// <summary>
        /// Creates linear polynomial of form {@code cc + x * lc}
        /// </summary>
        /// <param name="cc">the  constant coefficient</param>
        /// <param name="lc">the  leading coefficient</param>
        /// <param name="modulus">the modulus</param>
        /// <returns>{@code cc + x * lc}</returns>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// data is not reduced modulo modulus
        /// </summary>
        /// <summary>
        /// Creates monomial {@code coefficient * x^exponent}
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="coefficient">monomial coefficient</param>
        /// <param name="exponent">monomial exponent</param>
        /// <returns>{@code coefficient * x^exponent}</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates zero polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>zero polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Creates unit polynomial
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <returns>unit polynomial</returns>
        /// <summary>
        /// Returns the modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// does not copy the data and does not reduce the data with new modulus
        /// </summary>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Creates new Zp[x] polynomial by coping the coefficients of this and reducing them modulo new modulus.
        /// </summary>
        /// <param name="newDomain">the new domain</param>
        /// <returns>the copy of this reduced modulo new modulus</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[x] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        /// <summary>
        /// Returns Z[x] polynomial formed from the coefficients of this.
        /// </summary>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[x] version of this</returns>
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor} (that is {@code
        /// monic(modulus).multiply(factor)} ).
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        /// <summary>
        /// Divide by specified value
        /// </summary>
        /// <param name="val">the value</param>
        /// <returns>{@code this / val}</returns>
        // we can apply fast integer arithmetic and then reduce
        // we can apply fast integer arithmetic and then reduce
        /// <summary>
        /// {@inheritDoc}. The ring of the result will be exactly those returned by {@code this.ring.asGenericRing() }
        /// </summary>
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
        // this trick is taken from Seyed Mohammad Mahdi Javadi PhD
        // thesis "EFFICIENT ALGORITHMS FOR COMPUTATIONS WITH SPARSE POLYNOMIALS" page 79
        // This trick works only when modulus (so as all elements in arrays) is less
        // than 2^32 (so modulus*modulus fits machine word).
        // this is safe multiplication
        public override MultivariatePolynomialZp64 AsMultivariate(Comparator<DegreeVector> ordering)
        {
            return MultivariatePolynomialZp64.AsMultivariate(this, 1, 0, ordering);
        }
    }
}