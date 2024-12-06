using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Poly;
using Org.Apache.Commons.Math3.Random;
using Java.Util.Function;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Poly.RoundingMode;
using static Cc.Redberry.Rings.Poly.Associativity;
using static Cc.Redberry.Rings.Poly.Operator;
using static Cc.Redberry.Rings.Poly.TokenType;
using static Cc.Redberry.Rings.Poly.SystemInfo;

namespace Cc.Redberry.Rings.Poly
{
    /// <summary>
    /// Ring of univariate polynomials.
    /// </summary>
    /// <param name="<Poly>">type of univariate polynomials</param>
    /// <remarks>@since1.0</remarks>
    public sealed class UnivariateRing<Poly> : APolynomialRing<Poly>
    {
        private static readonly long serialVersionUID = 1;
        /// <summary>
        /// Creates ring of univariate polynomials which support operations over univariate polynomials of the type same as
        /// of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">factory polynomial (the exact value of {@code factory} is irrelevant)</param>
        public UnivariateRing(Poly factory) : base(factory)
        {
        }

        /// <summary>
        /// Creates ring of univariate polynomials which support operations over univariate polynomials of the type same as
        /// of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">factory polynomial (the exact value of {@code factory} is irrelevant)</param>
        public override int NVariables()
        {
            return 1;
        }

        /// <summary>
        /// Creates ring of univariate polynomials which support operations over univariate polynomials of the type same as
        /// of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">factory polynomial (the exact value of {@code factory} is irrelevant)</param>
        public override Poly Remainder(Poly a, Poly b)
        {
            return UnivariateDivision.Remainder(a, b, true);
        }

        /// <summary>
        /// Creates ring of univariate polynomials which support operations over univariate polynomials of the type same as
        /// of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">factory polynomial (the exact value of {@code factory} is irrelevant)</param>
        public override Poly[] DivideAndRemainder(Poly a, Poly b)
        {
            return UnivariateDivision.DivideAndRemainder(a, b, true);
        }

        /// <summary>
        /// Creates ring of univariate polynomials which support operations over univariate polynomials of the type same as
        /// of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">factory polynomial (the exact value of {@code factory} is irrelevant)</param>
        public override Poly Gcd(Poly a, Poly b)
        {
            return UnivariateGCD.PolynomialGCD(a, b);
        }

        /// <summary>
        /// Creates ring of univariate polynomials which support operations over univariate polynomials of the type same as
        /// of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">factory polynomial (the exact value of {@code factory} is irrelevant)</param>
        public override Poly[] ExtendedGCD(Poly a, Poly b)
        {
            return UnivariateGCD.PolynomialExtendedGCD(a, b);
        }

        /// <summary>
        /// Creates ring of univariate polynomials which support operations over univariate polynomials of the type same as
        /// of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">factory polynomial (the exact value of {@code factory} is irrelevant)</param>
        public override Poly[] FirstBezoutCoefficient(Poly a, Poly b)
        {
            return UnivariateGCD.PolynomialFirstBezoutCoefficient(a, b);
        }

        /// <summary>
        /// Creates ring of univariate polynomials which support operations over univariate polynomials of the type same as
        /// of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">factory polynomial (the exact value of {@code factory} is irrelevant)</param>
        public override PolynomialFactorDecomposition<Poly> FactorSquareFree(Poly element)
        {
            return UnivariateSquareFreeFactorization.SquareFreeFactorization(element);
        }

        /// <summary>
        /// Creates ring of univariate polynomials which support operations over univariate polynomials of the type same as
        /// of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">factory polynomial (the exact value of {@code factory} is irrelevant)</param>
        public override PolynomialFactorDecomposition<Poly> Factor(Poly element)
        {
            return UnivariateFactorization.Factor(element);
        }

        /// <summary>
        /// Creates ring of univariate polynomials which support operations over univariate polynomials of the type same as
        /// of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">factory polynomial (the exact value of {@code factory} is irrelevant)</param>
        public override Poly Variable(int variable)
        {
            if (variable != 0)
                throw new ArgumentException();
            return factory.CreateMonomial(1);
        }

        /// <summary>
        /// Creates ring of univariate polynomials which support operations over univariate polynomials of the type same as
        /// of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">factory polynomial (the exact value of {@code factory} is irrelevant)</param>
        /// <summary>
        /// Gives a random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        /// maxDegree} (exclusive)
        /// </summary>
        /// <param name="minDegree">the minimal degree of the result</param>
        /// <param name="maxDegree">the maximal degree of the result</param>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        ///         maxDegree} (exclusive)</returns>
        /// <remarks>@seeRandomUnivariatePolynomials</remarks>
        public Poly RandomElement(int minDegree, int maxDegree, RandomGenerator rnd)
        {
            return RandomUnivariatePolynomials.RandomPoly(factory, minDegree + (minDegree == maxDegree ? 0 : rnd.NextInt(maxDegree - minDegree)), rnd);
        }

        /// <summary>
        /// Creates ring of univariate polynomials which support operations over univariate polynomials of the type same as
        /// of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">factory polynomial (the exact value of {@code factory} is irrelevant)</param>
        /// <summary>
        /// Gives a random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        /// maxDegree} (exclusive)
        /// </summary>
        /// <param name="minDegree">the minimal degree of the result</param>
        /// <param name="maxDegree">the maximal degree of the result</param>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        ///         maxDegree} (exclusive)</returns>
        /// <remarks>@seeRandomUnivariatePolynomials</remarks>
        /// <summary>
        /// Gives a random univariate polynomial with the specified degree
        /// </summary>
        /// <param name="degree">the degree of the result</param>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial with the specified degree</returns>
        public Poly RandomElement(int degree, RandomGenerator rnd)
        {
            return RandomElement(degree, degree, rnd);
        }

        /// <summary>
        /// Creates ring of univariate polynomials which support operations over univariate polynomials of the type same as
        /// of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">factory polynomial (the exact value of {@code factory} is irrelevant)</param>
        /// <summary>
        /// Gives a random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        /// maxDegree} (exclusive)
        /// </summary>
        /// <param name="minDegree">the minimal degree of the result</param>
        /// <param name="maxDegree">the maximal degree of the result</param>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        ///         maxDegree} (exclusive)</returns>
        /// <remarks>@seeRandomUnivariatePolynomials</remarks>
        /// <summary>
        /// Gives a random univariate polynomial with the specified degree
        /// </summary>
        /// <param name="degree">the degree of the result</param>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial with the specified degree</returns>
        /// <summary>
        /// The minimal degree of polynomial generated with {@link #randomElement(RandomGenerator)}
        /// </summary>
        public static readonly int MIN_DEGREE_OF_RANDOM_POLY = 0;
        /// <summary>
        /// Creates ring of univariate polynomials which support operations over univariate polynomials of the type same as
        /// of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">factory polynomial (the exact value of {@code factory} is irrelevant)</param>
        /// <summary>
        /// Gives a random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        /// maxDegree} (exclusive)
        /// </summary>
        /// <param name="minDegree">the minimal degree of the result</param>
        /// <param name="maxDegree">the maximal degree of the result</param>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        ///         maxDegree} (exclusive)</returns>
        /// <remarks>@seeRandomUnivariatePolynomials</remarks>
        /// <summary>
        /// Gives a random univariate polynomial with the specified degree
        /// </summary>
        /// <param name="degree">the degree of the result</param>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial with the specified degree</returns>
        /// <summary>
        /// The minimal degree of polynomial generated with {@link #randomElement(RandomGenerator)}
        /// </summary>
        /// <summary>
        /// The maximal degree of polynomial generated with {@link #randomElement(RandomGenerator)}
        /// </summary>
        public static readonly int MAX_DEGREE_OF_RANDOM_POLY = 32;
        /// <summary>
        /// Creates ring of univariate polynomials which support operations over univariate polynomials of the type same as
        /// of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">factory polynomial (the exact value of {@code factory} is irrelevant)</param>
        /// <summary>
        /// Gives a random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        /// maxDegree} (exclusive)
        /// </summary>
        /// <param name="minDegree">the minimal degree of the result</param>
        /// <param name="maxDegree">the maximal degree of the result</param>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        ///         maxDegree} (exclusive)</returns>
        /// <remarks>@seeRandomUnivariatePolynomials</remarks>
        /// <summary>
        /// Gives a random univariate polynomial with the specified degree
        /// </summary>
        /// <param name="degree">the degree of the result</param>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial with the specified degree</returns>
        /// <summary>
        /// The minimal degree of polynomial generated with {@link #randomElement(RandomGenerator)}
        /// </summary>
        /// <summary>
        /// The maximal degree of polynomial generated with {@link #randomElement(RandomGenerator)}
        /// </summary>
        /// <summary>
        /// Gives a random univariate polynomial with the degree randomly picked from {@link #MIN_DEGREE_OF_RANDOM_POLY}
        /// (inclusive) to {@link #MAX_DEGREE_OF_RANDOM_POLY} (exclusive)
        /// </summary>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial</returns>
        public override Poly RandomElement(RandomGenerator rnd)
        {
            return RandomElement(MIN_DEGREE_OF_RANDOM_POLY, MAX_DEGREE_OF_RANDOM_POLY, rnd);
        }

        /// <summary>
        /// Creates ring of univariate polynomials which support operations over univariate polynomials of the type same as
        /// of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">factory polynomial (the exact value of {@code factory} is irrelevant)</param>
        /// <summary>
        /// Gives a random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        /// maxDegree} (exclusive)
        /// </summary>
        /// <param name="minDegree">the minimal degree of the result</param>
        /// <param name="maxDegree">the maximal degree of the result</param>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        ///         maxDegree} (exclusive)</returns>
        /// <remarks>@seeRandomUnivariatePolynomials</remarks>
        /// <summary>
        /// Gives a random univariate polynomial with the specified degree
        /// </summary>
        /// <param name="degree">the degree of the result</param>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial with the specified degree</returns>
        /// <summary>
        /// The minimal degree of polynomial generated with {@link #randomElement(RandomGenerator)}
        /// </summary>
        /// <summary>
        /// The maximal degree of polynomial generated with {@link #randomElement(RandomGenerator)}
        /// </summary>
        /// <summary>
        /// Gives a random univariate polynomial with the degree randomly picked from {@link #MIN_DEGREE_OF_RANDOM_POLY}
        /// (inclusive) to {@link #MAX_DEGREE_OF_RANDOM_POLY} (exclusive)
        /// </summary>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial</returns>
        /// <summary>
        /// Gives a random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        /// maxDegree} (exclusive) and coefficients generated via {@link Ring#randomElementTree(RandomGenerator)} method
        /// </summary>
        /// <param name="minDegree">the minimal degree of the result</param>
        /// <param name="maxDegree">the maximal degree of the result</param>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        ///         maxDegree} (exclusive)</returns>
        /// <remarks>@seeRandomUnivariatePolynomials</remarks>
        public Poly RandomElementTree(int minDegree, int maxDegree, RandomGenerator rnd)
        {
            if (factory is UnivariatePolynomial)
            {
                UnivariatePolynomial f = (UnivariatePolynomial)this.factory;
                Ring cfRing = f.ring;
                Function<RandomGenerator, TWildcardTodo> method = cfRing.RandomElementTree();
                return (Poly)RandomUnivariatePolynomials.RandomPoly(minDegree + (minDegree == maxDegree ? 0 : rnd.NextInt(maxDegree - minDegree)), cfRing, method, rnd);
            }
            else
                return RandomElement(minDegree, maxDegree, rnd);
        }

        /// <summary>
        /// Creates ring of univariate polynomials which support operations over univariate polynomials of the type same as
        /// of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">factory polynomial (the exact value of {@code factory} is irrelevant)</param>
        /// <summary>
        /// Gives a random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        /// maxDegree} (exclusive)
        /// </summary>
        /// <param name="minDegree">the minimal degree of the result</param>
        /// <param name="maxDegree">the maximal degree of the result</param>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        ///         maxDegree} (exclusive)</returns>
        /// <remarks>@seeRandomUnivariatePolynomials</remarks>
        /// <summary>
        /// Gives a random univariate polynomial with the specified degree
        /// </summary>
        /// <param name="degree">the degree of the result</param>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial with the specified degree</returns>
        /// <summary>
        /// The minimal degree of polynomial generated with {@link #randomElement(RandomGenerator)}
        /// </summary>
        /// <summary>
        /// The maximal degree of polynomial generated with {@link #randomElement(RandomGenerator)}
        /// </summary>
        /// <summary>
        /// Gives a random univariate polynomial with the degree randomly picked from {@link #MIN_DEGREE_OF_RANDOM_POLY}
        /// (inclusive) to {@link #MAX_DEGREE_OF_RANDOM_POLY} (exclusive)
        /// </summary>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial</returns>
        /// <summary>
        /// Gives a random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        /// maxDegree} (exclusive) and coefficients generated via {@link Ring#randomElementTree(RandomGenerator)} method
        /// </summary>
        /// <param name="minDegree">the minimal degree of the result</param>
        /// <param name="maxDegree">the maximal degree of the result</param>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        ///         maxDegree} (exclusive)</returns>
        /// <remarks>@seeRandomUnivariatePolynomials</remarks>
        /// <summary>
        /// Gives a random univariate polynomial with the degree randomly picked from {@link #MIN_DEGREE_OF_RANDOM_POLY}
        /// (inclusive) to {@link #MAX_DEGREE_OF_RANDOM_POLY} (exclusive)
        /// </summary>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial</returns>
        public override Poly RandomElementTree(RandomGenerator rnd)
        {
            return RandomElementTree(MIN_DEGREE_OF_RANDOM_POLY, MAX_DEGREE_OF_RANDOM_POLY, rnd);
        }
    }
}