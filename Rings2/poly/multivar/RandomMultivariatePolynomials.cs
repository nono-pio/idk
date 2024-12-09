
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Util;

namespace Cc.Redberry.Rings.Poly.Multivar
{
    /// <summary>
    /// Methods to generate random multivariate polynomials.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class RandomMultivariatePolynomials
    {
        private RandomMultivariatePolynomials()
        {
        }

        /// <summary>
        /// Generates random Z[X] polynomial with coefficients bounded by {@code bound}
        /// </summary>
        /// <param name="nVars">number of variables</param>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="bound">coefficient bound</param>
        /// <param name="ordering">monomial order</param>
        /// <param name="rnd">random source</param>
        /// <returns>random Z[X] polynomial</returns>
        public static MultivariatePolynomial<BigInteger> RandomPolynomial(int nVars, int degree, int size, BigInteger bound, IComparable<DegreeVector> ordering, Random rnd)
        {
            int nd = 3 * degree / 2;
            Monomial<BigInteger>[] terms = new Monomial<BigInteger>[size];
            for (int i = 0; i < size; i++)
            {
                BigInteger cfx = RandomUtil.RandomInt(bound, rnd);
                if (rnd.NextBoolean() && rnd.NextBoolean())
                    cfx = cfx.Negate();
                int[] exponents = RandomUtil.RandomIntArray(nVars, 0, nd, rnd);
                terms[i] = new Monomial<BigInteger>(exponents, cfx);
            }

            return MultivariatePolynomial<BigInteger>.Create(nVars, Rings.Z, ordering, terms);
        }

        /// <summary>
        /// Generates random Z[X] polynomial
        /// </summary>
        /// <param name="nVars">number of variables</param>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        public static MultivariatePolynomial<BigInteger> RandomPolynomial(int nVars, int degree, int size, Random rnd)
        {
            return RandomPolynomial(nVars, degree, size, 10, MonomialOrder.DEFAULT, rnd);
        }

        /// <summary>
        /// Generates random polynomial
        /// </summary>
        /// <param name="nVars">number of variables</param>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="ring">the coefficient ring</param>
        /// <param name="ordering">monomial order</param>
        /// <param name="method">method for generating random coefficients</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        public static MultivariatePolynomial<E> RandomPolynomial<E>(int nVars, int degree, int size, Ring<E> ring, IComparable<DegreeVector> ordering, Func<Random, E> method, Random rnd)
        {
            int nd = 3 * degree / 2;
            Monomial<E>[] terms = new Monomial<E>[size];
            for (int i = 0; i < size; i++)
            {
                E cfx = method(rnd);
                int[] exponents = RandomUtil.RandomIntArray(nVars, 0, nd, rnd);
                terms[i] = new Monomial<E>(exponents, cfx);
            }

            return MultivariatePolynomial<E>.Create(nVars, ring, ordering, terms);
        }

        /// <summary>
        /// Generates random polynomial
        /// </summary>
        /// <param name="nVars">number of variables</param>
        /// <param name="minDegree">minimal exponent</param>
        /// <param name="maxDegree">maximalexponent</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="ring">the coefficient ring</param>
        /// <param name="ordering">monomial order</param>
        /// <param name="method">method for generating random coefficients</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        public static MultivariatePolynomial<E> RandomPolynomial<E>(int nVars, int minDegree, int maxDegree, int size, Ring<E> ring, IComparable<DegreeVector> ordering, Func<Random, E> method, Random rnd)
        {
            Monomial<E>[] terms = new Monomial<E>[size];
            for (int i = 0; i < size; i++)
            {
                E cfx = method(rnd);
                int[] exponents = RandomUtil.RandomIntArray(nVars, minDegree, maxDegree, rnd);
                terms[i] = new Monomial<E>(exponents, cfx);
            }

            return MultivariatePolynomial<E>.Create(nVars, ring, ordering, terms);
        }

        /// <summary>
        /// Generates random polynomial
        /// </summary>
        /// <param name="nVars">number of variables</param>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="ring">the coefficient ring</param>
        /// <param name="ordering">monomial order</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        public static MultivariatePolynomial<E> RandomPolynomial<E>(int nVars, int degree, int size, Ring<E> ring, IComparable<DegreeVector> ordering, Random rnd)
        {
            return RandomPolynomial(nVars, degree, size, ring, ordering, ring.RandomElement(), rnd);
        }

        /// <summary>
        /// Generates random Zp[X] polynomial over machine integers
        /// </summary>
        /// <param name="nVars">number of variables</param>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="ring">the coefficient ring</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        public static MultivariatePolynomialZp64 RandomPolynomial(int nVars, int degree, int size, IntegersZp64 ring, Random rnd)
        {
            return RandomPolynomial(nVars, degree, size, ring, MonomialOrder.DEFAULT, rnd);
        }

        /// <summary>
        /// Generates random Zp[X] polynomial over machine integers
        /// </summary>
        /// <param name="nVars">number of variables</param>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="ring">the coefficient ring</param>
        /// <param name="ordering">monomial order</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        public static MultivariatePolynomialZp64 RandomPolynomial(int nVars, int degree, int size, IntegersZp64 ring, IComparable<DegreeVector> ordering, Random rnd)
        {
            int nd = 3 * degree / 2;
            MonomialZp64[] terms = new MonomialZp64[size];
            for (int i = 0; i < size; i++)
            {
                long cfx = ring.RandomElement(rnd);
                int[] exponents = RandomUtil.RandomIntArray(nVars, 0, nd, rnd);
                terms[i] = new MonomialZp64(exponents, cfx);
            }

            return MultivariatePolynomialZp64.Create(nVars, ring, ordering, terms);
        }

        /// <summary>
        /// Generates random Zp[X] polynomial over machine integers
        /// </summary>
        /// <param name="nVars">number of variables</param>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="ring">the coefficient ring</param>
        /// <param name="ordering">monomial order</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        public static MultivariatePolynomialZp64 RandomSharpPolynomial(int nVars, int degree, int size, IntegersZp64 ring, IComparable<DegreeVector> ordering, Random rnd)
        {
            MonomialZp64[] terms = new MonomialZp64[size];
            for (int i = 0; i < size; i++)
            {
                long cfx = ring.RandomElement(rnd);
                int[] exponents = RandomUtil.RandomSharpIntArray(nVars, degree, rnd);
                terms[i] = new MonomialZp64(exponents, cfx);
            }

            return MultivariatePolynomialZp64.Create(nVars, ring, ordering, terms);
        }

        /// <summary>
        /// Generates random Zp[X] polynomial over machine integers
        /// </summary>
        /// <param name="nVars">number of variables</param>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="ring">the coefficient ring</param>
        /// <param name="ordering">monomial order</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        public static MultivariatePolynomial<E> RandomSharpPolynomial<E>(int nVars, int degree, int size, Ring<E> ring, IComparable<DegreeVector> ordering, Func<Random, E> rndCoefficients, Random rnd)
        {
            Monomial<E>[] terms = new Monomial<E>[size];
            for (int i = 0; i < size; i++)
            {
                E cfx = rndCoefficients(rnd);
                int[] exponents = RandomUtil.RandomSharpIntArray(nVars, degree, rnd);
                terms[i] = new Monomial<E>(exponents, cfx);
            }

            return MultivariatePolynomial<E>.Create(nVars, ring, ordering, terms);
        }

        /// <summary>
        /// Generates random multivariate polynomial
        /// </summary>
        /// <param name="factory">factory polynomial</param>
        /// <param name="degree">maximal degree of the result</param>
        /// <param name="size">number of elements in the result</param>
        /// <param name="rnd">random source</param>
        /// <returns>random polynomial</returns>
        public static Poly RandomPolynomial<Term, Poly>(Poly factory, int degree, int size, Random rnd) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            if (factory is MultivariatePolynomialZp64)
                return (Poly)RandomPolynomial(((MultivariatePolynomialZp64)factory).nVariables, degree, size, ((MultivariatePolynomialZp64)factory).ring, ((MultivariatePolynomialZp64)factory).ordering, rnd);
            else
                return (Poly)RandomPolynomial(((MultivariatePolynomial)factory).nVariables, degree, size, ((MultivariatePolynomial)factory).ring, ((MultivariatePolynomial)factory).ordering, rnd);
        }
    }
}