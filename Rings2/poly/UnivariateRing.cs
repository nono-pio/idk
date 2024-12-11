using System.Runtime.InteropServices.JavaScript;
using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Poly.Univar;

namespace Cc.Redberry.Rings.Poly
{
    /// <summary>
    /// Ring of univariate polynomials.
    /// </summary>
    /// <param name="<Poly>">type of univariate polynomials</param>
    /// <remarks>@since1.0</remarks>
    public sealed class UnivariateRing<Poly> : APolynomialRing<Poly> where Poly : IUnivariatePolynomial<Poly>
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

        
        public override int NVariables()
        {
            return 1;
        }

        
        public override Poly Remainder(Poly a, Poly b)
        {
            return UnivariateDivision.Remainder(a, b, true);
        }

        
        public override Poly[] DivideAndRemainder(Poly a, Poly b)
        {
            return UnivariateDivision.DivideAndRemainder(a, b, true);
        }

        
        public override Poly Gcd(Poly a, Poly b)
        {
            return UnivariateGCD.PolynomialGCD(a, b);
        }

        
        public override Poly[] ExtendedGCD(Poly a, Poly b)
        {
            return UnivariateGCD.PolynomialExtendedGCD(a, b);
        }

        
        public override Poly[] FirstBezoutCoefficient(Poly a, Poly b)
        {
            return UnivariateGCD.PolynomialFirstBezoutCoefficient(a, b);
        }

        
        public override PolynomialFactorDecomposition<Poly> FactorSquareFree(Poly element)
        {
            return UnivariateSquareFreeFactorization.SquareFreeFactorization(element);
        }

        
        public override PolynomialFactorDecomposition<Poly> Factor(Poly element)
        {
            return UnivariateFactorization.Factor(element);
        }

        
        public override Poly Variable(int variable)
        {
            if (variable != 0)
                throw new ArgumentException();
            return factory.CreateMonomial(1);
        }

        
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
        public Poly RandomElement(int minDegree, int maxDegree, Random rnd)
        {
            return RandomUnivariatePolynomials.RandomPoly(factory, minDegree + (minDegree == maxDegree ? 0 : rnd.Next(maxDegree - minDegree)), rnd);
        }

        
       
        /// <summary>
        /// Gives a random univariate polynomial with the specified degree
        /// </summary>
        /// <param name="degree">the degree of the result</param>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial with the specified degree</returns>
        public Poly RandomElement(int degree, Random rnd)
        {
            return RandomElement(degree, degree, rnd);
        }

        
        /// <summary>
        /// The minimal degree of polynomial generated with {@link #randomElement(Random)}
        /// </summary>
        public static readonly int MIN_DEGREE_OF_RANDOM_POLY = 0;
        
       
        /// <summary>
        /// The maximal degree of polynomial generated with {@link #randomElement(Random)}
        /// </summary>
        public static readonly int MAX_DEGREE_OF_RANDOM_POLY = 32;
        
        
        /// <summary>
        /// Gives a random univariate polynomial with the degree randomly picked from {@link #MIN_DEGREE_OF_RANDOM_POLY}
        /// (inclusive) to {@link #MAX_DEGREE_OF_RANDOM_POLY} (exclusive)
        /// </summary>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial</returns>
        public override Poly RandomElement(Random rnd)
        {
            return RandomElement(MIN_DEGREE_OF_RANDOM_POLY, MAX_DEGREE_OF_RANDOM_POLY, rnd);
        }

        
        /// <summary>
        /// Gives a random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        /// maxDegree} (exclusive) and coefficients generated via {@link Ring#randomElementTree(Random)} method
        /// </summary>
        /// <param name="minDegree">the minimal degree of the result</param>
        /// <param name="maxDegree">the maximal degree of the result</param>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial with the degree randomly picked from {@code minDegree} (inclusive) to {@code
        ///         maxDegree} (exclusive)</returns>
        /// <remarks>@seeRandomUnivariatePolynomials</remarks>
        public Poly RandomElementTree(int minDegree, int maxDegree, Random rnd)
        {
            if (factory is UnivariatePolynomial)
            {
                UnivariatePolynomial f = (UnivariatePolynomial)this.factory;
                Ring cfRing = f.ring;
                JSType.Function<Random, TWildcardTodo> method = cfRing.RandomElementTree();
                return (Poly)RandomUnivariatePolynomials.RandomPoly(minDegree + (minDegree == maxDegree ? 0 : rnd.Next(maxDegree - minDegree)), cfRing, method, rnd);
            }
            else
                return RandomElement(minDegree, maxDegree, rnd);
        }

        
        /// <summary>
        /// Gives a random univariate polynomial with the degree randomly picked from {@link #MIN_DEGREE_OF_RANDOM_POLY}
        /// (inclusive) to {@link #MAX_DEGREE_OF_RANDOM_POLY} (exclusive)
        /// </summary>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random univariate polynomial</returns>
        public override Poly RandomElementTree(Random rnd)
        {
            return RandomElementTree(MIN_DEGREE_OF_RANDOM_POLY, MAX_DEGREE_OF_RANDOM_POLY, rnd);
        }
    }
}