

using System.Numerics;
using System.Reflection;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Poly.Multivar;
using Cc.Redberry.Rings.Poly.Univar;

namespace Cc.Redberry.Rings.Poly
{
    /// <summary>
    /// High-level methods for polynomials.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class PolynomialMethods
    {


        /// <summary>
        /// Factor polynomial.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>irreducible factor decomposition</returns>
        public static PolynomialFactorDecomposition<Poly> Factor<Poly>(Poly poly)
            where Poly : IUnivariatePolynomial<Poly>
        {
            return UnivariateFactorization.Factor<Poly>(poly);
        }
        
        /// <summary>
        /// Factor polynomial.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>irreducible factor decomposition</returns>
        public static PolynomialFactorDecomposition<Poly> Factor<Term, Poly>(Poly poly)
            where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            return MultivariateFactorization.Factor(poly);
        }
        

        /// <summary>
        /// Square-free factorization of polynomial.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>irreducible square-free factor decomposition</returns>
        public static PolynomialFactorDecomposition<Poly> FactorSquareFree<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
        { 
            return UnivariateSquareFreeFactorization.SquareFreeFactorization(poly);
        }
        
        /// <summary>
        /// Square-free factorization of polynomial.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>irreducible square-free factor decomposition</returns>
        public static PolynomialFactorDecomposition<Poly> FactorSquareFree<Term, Poly>(Poly poly) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        { 
            return MultivariateSquareFreeFactorization.SquareFreeFactorization<Term, Poly>(poly);
        }

        /// <summary>
        /// Compute GCD of two polynomials.
        /// </summary>
        /// <param name="a">the polynomial</param>
        /// <param name="b">the polynomial</param>
        /// <returns>the GCD</returns>
        public static Poly PolynomialGCD<Poly>(Poly a, Poly b) where Poly : IUnivariatePolynomial<Poly>
        {
            return UnivariateGCD.PolynomialGCD(a, b);
        }
        /// <summary>
        /// Compute GCD of two polynomials.
        /// </summary>
        /// <param name="a">the polynomial</param>
        /// <param name="b">the polynomial</param>
        /// <returns>the GCD</returns>
        public static Poly PolynomialGCD<Term, Poly>(Poly a, Poly b) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            return MultivariateGCD.PolynomialGCD(a, b);
        }

        /// <summary>
        /// Compute GCD of array of polynomials.
        /// </summary>
        /// <param name="array">the polynomials</param>
        /// <returns>the GCD</returns>
        public static Poly PolynomialGCD<Poly>(params Poly[] array) where Poly : IUnivariatePolynomial<Poly>
        {
            return UnivariateGCD.PolynomialGCD(array);
        }
        /// <summary>
        /// Compute GCD of array of polynomials.
        /// </summary>
        /// <param name="array">the polynomials</param>
        /// <returns>the GCD</returns>
        public static Poly PolynomialGCD<Term, Poly>(params Poly[] array) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            return MultivariateGCD.PolynomialGCD(array);
        }

        /// <summary>
        /// Compute GCD of collection of polynomials.
        /// </summary>
        /// <param name="array">the polynomials</param>
        /// <returns>the GCD</returns>
        public static Poly PolynomialGCD<Poly>(IEnumerable<Poly> array) where Poly : IUnivariatePolynomial<Poly>
        {
            return UnivariateGCD.PolynomialGCD(array);
        }
        /// <summary>
        /// Compute GCD of collection of polynomials.
        /// </summary>
        /// <param name="array">the polynomials</param>
        /// <returns>the GCD</returns>
        public static Poly PolynomialGCD<Term, Poly>(IEnumerable<Poly> array) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            return MultivariateGCD.PolynomialGCD(array);
        }

        /// <summary>
        /// Computes {@code [gcd(a,b), s, t]} such that {@code s * a + t * b = gcd(a, b)}. Half-GCD algorithm is used.
        /// </summary>
        /// <param name="a">the univariate polynomial</param>
        /// <param name="b">the univariate  polynomial</param>
        /// <returns>array of {@code [gcd(a,b), s, t]} such that {@code s * a + t * b = gcd(a, b)} (gcd is monic)</returns>
        /// <remarks>@seeUnivariateGCD#PolynomialExtendedGCD(IUnivariatePolynomial, IUnivariatePolynomial)</remarks>
        public static T[] PolynomialExtendedGCD<T>(T a, T b) where T : IUnivariatePolynomial<T>
        {
            if (a.IsOverField())
                return UnivariateGCD.PolynomialExtendedGCD(a, b);
            else
                throw new ArgumentException("Polynomial over field is expected");
        }

        /// <summary>
        /// Returns quotient and remainder of a and b.
        /// </summary>
        /// <param name="a">the dividend</param>
        /// <param name="b">the divider</param>
        /// <returns>{quotient, remainder}</returns>
        public static Poly[] DivideAndRemainder<Poly>(Poly a, Poly b) where Poly : IUnivariatePolynomial<Poly>
        {
            return UnivariateDivision.DivideAndRemainder(a, b, true);
        }
        public static Poly[] DivideAndRemainder<Term, Poly>(Poly a, Poly b) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            return MultivariateDivision.DivideAndRemainder<Term, Poly>(a, b);
        
        }

        /// <summary>
        /// Returns quotient and remainder of a and b.
        /// </summary>
        /// <param name="a">the dividend</param>
        /// <param name="b">the divider</param>
        /// <returns>{quotient, remainder}</returns>
        public static Poly Remainder<Poly  >(Poly a, Poly b) where Poly : IPolynomial<Poly>
        {
            if (a is IUnivariatePolynomial)
                return (Poly)UnivariateDivision.Remainder((IUnivariatePolynomial)a, (IUnivariatePolynomial)b, true);
            else if (a is AMultivariatePolynomial)
                return (Poly)MultivariateDivision.DivideAndRemainder((AMultivariatePolynomial)a, (AMultivariatePolynomial)b)[1];
            else
                throw new Exception();
        }

        /// <summary>
        /// Returns the quotient of a and b or throws {@code ArithmeticException} if exact division is not possible
        /// </summary>
        /// <param name="a">the dividend</param>
        /// <param name="b">the divider</param>
        /// <returns>quotient</returns>
        /// <exception cref="ArithmeticException">if exact division is not possible</exception>
        public static Poly DivideOrNull<Poly >(Poly a, Poly b) where Poly : IPolynomial<Poly>
        {
            if (a is IUnivariatePolynomial)
                return (Poly)UnivariateDivision.DivideOrNull((IUnivariatePolynomial)a, (IUnivariatePolynomial)b, true);
            else if (a is AMultivariatePolynomial)
                return (Poly)MultivariateDivision.DivideOrNull((AMultivariatePolynomial)a, (AMultivariatePolynomial)b);
            else
                throw new Exception();
        }

        /// <summary>
        /// Returns the quotient of a and b or throws {@code ArithmeticException} if exact division is not possible
        /// </summary>
        /// <param name="a">the dividend</param>
        /// <param name="b">the divider</param>
        /// <returns>quotient</returns>
        /// <exception cref="ArithmeticException">if exact division is not possible</exception>
        public static Poly DivideExact<Poly >(Poly a, Poly b) where Poly : IPolynomial<Poly>
        {
            if (a is IUnivariatePolynomial)
                return (Poly)UnivariateDivision.DivideExact((IUnivariatePolynomial)a, (IUnivariatePolynomial)b, true);
            else if (a is AMultivariatePolynomial)
                return (Poly)MultivariateDivision.DivideExact((AMultivariatePolynomial)a, (AMultivariatePolynomial)b);
            else
                throw new Exception();
        }

        /// <summary>
        /// Returns whether specified polynomials are coprime.
        /// </summary>
        /// <param name="polynomials">the polynomials</param>
        /// <returns>whether specified polynomials are coprime</returns>
        public static bool CoprimeQ<Poly>(params Poly[] polynomials) where Poly : IPolynomial<Poly>
        {
            for (int i = 0; i < polynomials.Length - 1; i++)
                for (int j = i + 1; j < polynomials.Length; j++)
                    if (!PolynomialGCD(polynomials[i], polynomials[j]).IsConstant())
                        return false;
            return true;
        }

        /// <summary>
        /// Returns whether specified polynomials are coprime.
        /// </summary>
        /// <param name="polynomials">the polynomials</param>
        /// <returns>whether specified polynomials are coprime</returns>
        public static bool CoprimeQ<Poly  >(IEnumerable<Poly> polynomials) where Poly : IPolynomial<Poly>
        {
            return CoprimeQ(polynomials.ToArray());
        }

        /// <summary>
        /// Returns whether specified polynomial is irreducible
        /// </summary>
        public static bool IrreducibleQ<Poly  >(Poly poly) where Poly : IPolynomial<Poly>
        {
            if (poly is IUnivariatePolynomial)
                return IrreduciblePolynomials.IrreducibleQ((IUnivariatePolynomial)poly);
            else
                return MultivariateFactorization.Factor((AMultivariatePolynomial)poly).IsTrivial();
        }

        /// <summary>
        /// Returns {@code base} in a power of non-negative {@code exponent}.
        /// </summary>
        /// <param name="base">the base</param>
        /// <param name="exponent">the non-negative exponent</param>
        /// <param name="copy">whether to clone {@code base}; if not the data of {@code base} will be lost</param>
        /// <returns>{@code base} in a power of {@code e}</returns>
        public static T PolyPow<T  >(T @base, BigInteger exponent, bool copy) where T : IPolynomial<T>
        {
            if (exponent.Signum() < 0)
                throw new ArgumentException();
            if (exponent.IsOne|| @base.IsOne())
                return copy ? @base.Clone() : @base;
            T result = @base.CreateOne();
            T k2p = copy ? @base.Clone() : @base;
            for (;;)
            {
                if (exponent.TestBit(0))
                    result = result.Multiply(k2p);
                exponent = exponent.ShiftRight(1);
                if (exponent.IsZero)
                    return result;
                k2p = k2p.Multiply(k2p);
            }
        }

        /// <summary>
        /// Returns {@code base} in a power of non-negative {@code exponent}
        /// </summary>
        /// <param name="base">the base</param>
        /// <param name="exponent">the non-negative exponent</param>
        /// <returns>{@code base} in a power of {@code e}</returns>
        public static T PolyPow<T >(T @base, long exponent) where T : IPolynomial<T>
        {
            return PolyPow(@base, exponent, true);
        }

        /// <summary>
        /// Returns {@code base} in a power of non-negative {@code exponent}
        /// </summary>
        /// <param name="base">the base</param>
        /// <param name="exponent">the non-negative exponent</param>
        /// <returns>{@code base} in a power of {@code e}</returns>
        public static T PolyPow<T  >(T @base, BigInteger exponent) where T : IPolynomial<T>
        {
            return PolyPow(@base, exponent, true);
        }

        /// <summary>
        /// Returns {@code base} in a power of non-negative {@code exponent}
        /// </summary>
        /// <param name="base">the base</param>
        /// <param name="exponent">the non-negative exponent</param>
        /// <param name="copy">whether to clone {@code base}; if not the data of {@code base} will be lost</param>
        /// <returns>{@code base} in a power of {@code e}</returns>
        public static T PolyPow<T >(T @base, long exponent, bool copy) where T : IPolynomial<T>
        {
            if (exponent < 0)
                throw new ArgumentException();
            if (exponent == 1 || @base.IsOne())
                return copy ? @base.Clone() : @base;
            T result = @base.CreateOne();
            T k2p = copy ? @base.Clone() : @base;
            for (;;)
            {
                if ((exponent & 1) != 0)
                    result = result.Multiply(k2p);
                exponent = exponent >> 1;
                if (exponent == 0)
                    return result;
                k2p = k2p.Multiply(k2p);
            }
        }

        /// <summary>
        /// Returns {@code base} in a power of non-negative {@code exponent}
        /// </summary>
        /// <param name="base">the base</param>
        /// <param name="exponent">the non-negative exponent</param>
        /// <param name="copy">whether to clone {@code base}; if not the data of {@code base} will be lost</param>
        /// <param name="cache">cache to store all intermediate powers</param>
        /// <returns>{@code base} in a power of {@code e}</returns>
        public static T PolyPow<T >(T @base, int exponent, bool copy, TIntObjectHashMap<T> cache) where T : IPolynomial<T>
        {
            if (exponent < 0)
                throw new ArgumentException();
            if (exponent == 1)
                return copy ? @base.Clone() : @base;
            T cached = cache[exponent];
            if (cached != null)
                return cached.Clone();
            T result = @base.CreateOne();
            T k2p = copy ? @base.Clone() : @base;
            int rExp = 0, kExp = 1;
            for (;;)
            {
                if ((exponent & 1) != 0)
                    cache.Put(rExp += kExp, result.Multiply(k2p).Clone());
                exponent = exponent >> 1;
                if (exponent == 0)
                {
                    cache.Put(rExp, result);
                    return result;
                }

                cache.Put(kExp *= 2, k2p.Square().Clone());
            }
        }
    }
}