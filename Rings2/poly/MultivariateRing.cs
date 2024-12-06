using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Poly;
using Org.Apache.Commons.Math3.Random;
using Java.Util;
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
    /// Ring of multivariate polynomials.
    /// </summary>
    /// <param name="<Poly>">type of multivariate polynomials</param>
    /// <remarks>@since1.0</remarks>
    public sealed class MultivariateRing<Poly> : APolynomialRing<Poly>
    {
        private static readonly long serialVersionUID = 1;
        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        public MultivariateRing(Poly factory) : base(factory)
        {
        }

        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        public IMonomialAlgebra<Term> MonomialAlgebra<Term extends AMonomial<Term>>()
        {
            IMonomialAlgebra<TWildcardTodo> monomialAlgebra = factory.monomialAlgebra;
            return (IMonomialAlgebra<Term>)monomialAlgebra;
        }

        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        public override int NVariables()
        {
            return factory.nVariables;
        }

        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        public Comparator<DegreeVector> Ordering()
        {
            return factory.ordering;
        }

        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        public MultivariateRing<Poly> DropVariable()
        {
            return new MultivariateRing(factory.DropVariable(0));
        }

        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        public override Poly[] DivideAndRemainder(Poly dividend, Poly divider)
        {
            Poly[] arr = divider.CreateArray(1);
            arr[0] = divider;
            return (Poly[])MultivariateDivision.DivideAndRemainder((AMultivariatePolynomial)dividend, (AMultivariatePolynomial[])arr);
        }

        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        public override Poly Gcd(Poly a, Poly b)
        {
            return MultivariateGCD.PolynomialGCD(a, b);
        }

        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        public override Poly Gcd(Poly[] elements)
        {
            return MultivariateGCD.PolynomialGCD(elements);
        }

        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        public override Poly Gcd(Iterable<Poly> elements)
        {
            return MultivariateGCD.PolynomialGCD(elements);
        }

        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        public override PolynomialFactorDecomposition<Poly> FactorSquareFree(Poly element)
        {
            return (PolynomialFactorDecomposition<Poly>)MultivariateSquareFreeFactorization.SquareFreeFactorization((AMultivariatePolynomial)element);
        }

        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        public override PolynomialFactorDecomposition<Poly> Factor(Poly element)
        {
            return MultivariateFactorization.Factor(element);
        }

        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        public override Poly Variable(int variable)
        {
            return factory.CreateMonomial(variable, 1);
        }

        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        /// <summary>
        /// Creates multivariate polynomial over the same ring as this with the single monomial
        /// </summary>
        /// <param name="term">the monomial</param>
        /// <returns>multivariate polynomial</returns>
        public Poly Create(DegreeVector term)
        {
            return factory.Create(term);
        }

        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        /// <summary>
        /// Creates multivariate polynomial over the same ring as this with the single monomial
        /// </summary>
        /// <param name="term">the monomial</param>
        /// <returns>multivariate polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        public Poly RandomElement(int degree, int size, RandomGenerator rnd)
        {
            return (Poly)RandomMultivariatePolynomials.RandomPolynomial((AMultivariatePolynomial)factory, degree, size, rnd);
        }

        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        /// <summary>
        /// Creates multivariate polynomial over the same ring as this with the single monomial
        /// </summary>
        /// <param name="term">the monomial</param>
        /// <returns>multivariate polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        public Poly RandomElementTree(int degree, int size, RandomGenerator rnd)
        {
            if (factory is MultivariatePolynomial)
            {
                MultivariatePolynomial f = (MultivariatePolynomial)this.factory;
                Ring cfRing = f.ring;
                Function<RandomGenerator, TWildcardTodo> method = cfRing.RandomElementTree();
                return (Poly)RandomMultivariatePolynomials.RandomPolynomial(NVariables(), degree, size, cfRing, ((MultivariatePolynomial)factory).ordering, method, rnd);
            }
            else
                return RandomElement(degree, size, rnd);
        }

        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        /// <summary>
        /// Creates multivariate polynomial over the same ring as this with the single monomial
        /// </summary>
        /// <param name="term">the monomial</param>
        /// <returns>multivariate polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        private static readonly RandomGenerator privateRandom = new Well44497b(System.NanoTime());
        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        /// <summary>
        /// Creates multivariate polynomial over the same ring as this with the single monomial
        /// </summary>
        /// <param name="term">the monomial</param>
        /// <returns>multivariate polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <returns>random polynomial</returns>
        public Poly RandomElement(int degree, int size)
        {
            return RandomElement(degree, size, privateRandom);
        }

        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        /// <summary>
        /// Creates multivariate polynomial over the same ring as this with the single monomial
        /// </summary>
        /// <param name="term">the monomial</param>
        /// <returns>multivariate polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <returns>random polynomial</returns>
        /// <summary>
        /// Gives a random constant polynomial. For generating non-constant random polynomials see {@link
        /// cc.redberry.rings.poly.multivar.RandomMultivariatePolynomials}
        /// </summary>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random constant polynomial</returns>
        /// <remarks>@seecc.redberry.rings.poly.multivar.RandomMultivariatePolynomials</remarks>
        public override Poly RandomElement(RandomGenerator rnd)
        {
            return base.RandomElement(rnd);
        }

        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        /// <summary>
        /// Creates multivariate polynomial over the same ring as this with the single monomial
        /// </summary>
        /// <param name="term">the monomial</param>
        /// <returns>multivariate polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <returns>random polynomial</returns>
        /// <summary>
        /// Gives a random constant polynomial. For generating non-constant random polynomials see {@link
        /// cc.redberry.rings.poly.multivar.RandomMultivariatePolynomials}
        /// </summary>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random constant polynomial</returns>
        /// <remarks>@seecc.redberry.rings.poly.multivar.RandomMultivariatePolynomials</remarks>
        /// <summary>
        /// Default degree of polynomial generated with {@link #randomElementTree(RandomGenerator)}
        /// </summary>
        public static readonly int DEGREE_OF_RANDOM_POLY = 16;
        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        /// <summary>
        /// Creates multivariate polynomial over the same ring as this with the single monomial
        /// </summary>
        /// <param name="term">the monomial</param>
        /// <returns>multivariate polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <returns>random polynomial</returns>
        /// <summary>
        /// Gives a random constant polynomial. For generating non-constant random polynomials see {@link
        /// cc.redberry.rings.poly.multivar.RandomMultivariatePolynomials}
        /// </summary>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random constant polynomial</returns>
        /// <remarks>@seecc.redberry.rings.poly.multivar.RandomMultivariatePolynomials</remarks>
        /// <summary>
        /// Default degree of polynomial generated with {@link #randomElementTree(RandomGenerator)}
        /// </summary>
        /// <summary>
        /// Default size of polynomial generated with {@link #randomElementTree(RandomGenerator)}
        /// </summary>
        public static readonly int SIZE_OF_RANDOM_POLY = 16;
        /// <summary>
        /// Creates ring of multivariate polynomials which support operations over multivariate polynomials of the type and
        /// number of variables same as of provided {@code factory} polynomial
        /// </summary>
        /// <param name="factory">the factory polynomial (the exact value of {@code factory} is irrelevant) which fixes the element
        ///                type of this ring, coefficient ring and the number of variables</param>
        /// <summary>
        /// Creates multivariate polynomial over the same ring as this with the single monomial
        /// </summary>
        /// <param name="term">the monomial</param>
        /// <returns>multivariate polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <returns>random polynomial</returns>
        /// <summary>
        /// Gives a random constant polynomial. For generating non-constant random polynomials see {@link
        /// cc.redberry.rings.poly.multivar.RandomMultivariatePolynomials}
        /// </summary>
        /// <param name="rnd">the source of randomness</param>
        /// <returns>random constant polynomial</returns>
        /// <remarks>@seecc.redberry.rings.poly.multivar.RandomMultivariatePolynomials</remarks>
        /// <summary>
        /// Default degree of polynomial generated with {@link #randomElementTree(RandomGenerator)}
        /// </summary>
        /// <summary>
        /// Default size of polynomial generated with {@link #randomElementTree(RandomGenerator)}
        /// </summary>
        public override Poly RandomElementTree(RandomGenerator rnd)
        {
            return RandomElementTree(DEGREE_OF_RANDOM_POLY, SIZE_OF_RANDOM_POLY, rnd);
        }
    }
}