using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Poly;
using Cc.Redberry.Rings.Poly.Multivar;
using Java.Util;
using Cc.Redberry.Rings.Poly.Univar.Conversions64bit;
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
    /// Square-free factorization of univariate polynomials over Z and Zp.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class UnivariateSquareFreeFactorization
    {
        private UnivariateSquareFreeFactorization()
        {
        }

        /// <summary>
        /// Returns {@code true} if {@code poly} is square-free and {@code false} otherwise
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>{@code true} if {@code poly} is square-free and {@code false} otherwise</returns>
        public static bool IsSquareFree<T extends IUnivariatePolynomial<T>>(T poly)
        {
            return UnivariateGCD.PolynomialGCD(poly, poly.Derivative()).IsConstant();
        }

        /// <summary>
        /// Performs square-free factorization of a {@code poly}.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>square-free decomposition</returns>
        public static PolynomialFactorDecomposition<T> SquareFreeFactorization<T extends IUnivariatePolynomial<T>>(T poly)
        {
            if (poly.IsOverFiniteField())
                return SquareFreeFactorizationMusser(poly);
            else if (UnivariateFactorization.IsOverMultivariate(poly))
                return (PolynomialFactorDecomposition<T>)UnivariateFactorization.FactorOverMultivariate((UnivariatePolynomial)poly, MultivariateSquareFreeFactorization.SquareFreeFactorization());
            else if (UnivariateFactorization.IsOverUnivariate(poly))
                return (PolynomialFactorDecomposition<T>)UnivariateFactorization.FactorOverUnivariate((UnivariatePolynomial)poly, MultivariateSquareFreeFactorization.SquareFreeFactorization());
            else if (poly.CoefficientRingCharacteristic().IsZero())
                return SquareFreeFactorizationYunZeroCharacteristics(poly);
            else
                return SquareFreeFactorizationMusser(poly);
        }

        /// <summary>
        /// Returns square-free part of the {@code poly}
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>square free part</returns>
        public static T SquareFreePart<T extends IUnivariatePolynomial<T>>(T poly)
        {
            return SquareFreeFactorization(poly).factors.Stream().Filter((x) => !x.IsMonomial()).Reduce(poly.CreateOne(), IUnivariatePolynomial<T>.Multiply());
        }

        /// <summary>
        /// Performs square-free factorization of a {@code poly} which coefficient ring has zero characteristic using Yun's
        /// algorithm.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>square-free decomposition</returns>
        public static PolynomialFactorDecomposition<Poly> SquareFreeFactorizationYunZeroCharacteristics<Poly extends IUnivariatePolynomial<Poly>>(Poly poly)
        {
            if (!poly.CoefficientRingCharacteristic().IsZero())
                throw new ArgumentException("Characteristics 0 expected");
            if (poly.IsConstant())
                return PolynomialFactorDecomposition.Of(poly);

            // x^2 + x^3 -> x^2 (1 + x)
            int exponent = 0;
            while (exponent <= poly.Degree() && poly.IsZeroAt(exponent))
            {
                ++exponent;
            }

            if (exponent == 0)
                return SquareFreeFactorizationYun0(poly);
            Poly expFree = poly.GetRange(exponent, poly.Degree() + 1);
            PolynomialFactorDecomposition<Poly> fd = SquareFreeFactorizationYun0(expFree);
            fd.AddFactor(poly.CreateMonomial(1), exponent);
            return fd;
        }

        /// <summary>
        /// Performs square-free factorization of a poly using Yun's algorithm.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>square-free decomposition</returns>
        static PolynomialFactorDecomposition<Poly> SquareFreeFactorizationYun0<Poly extends IUnivariatePolynomial<Poly>>(Poly poly)
        {
            if (poly.IsConstant())
                return PolynomialFactorDecomposition.Of(poly);
            Poly content = poly.ContentAsPoly();
            if (poly.SignumOfLC() < 0)
                content = content.Negate();
            poly = poly.Clone().DivideByLC(content);
            if (poly.Degree() <= 1)
                return PolynomialFactorDecomposition.Of(content, poly);
            PolynomialFactorDecomposition<Poly> factorization = PolynomialFactorDecomposition.Of(content);
            SquareFreeFactorizationYun0(poly, factorization);
            return factorization;
        }

        private static void SquareFreeFactorizationYun0<Poly extends IUnivariatePolynomial<Poly>>(Poly poly, PolynomialFactorDecomposition<Poly> factorization)
        {
            Poly derivative = poly.Derivative(), gcd = UnivariateGCD.PolynomialGCD(poly, derivative);
            if (gcd.IsConstant())
            {
                factorization.AddFactor(poly, 1);
                return;
            }

            Poly quot = UnivariateDivision.DivideAndRemainder(poly, gcd, false)[0], dQuot = UnivariateDivision.DivideAndRemainder(derivative, gcd, false)[0]; // safely destroy (cloned) derivative (not used further)
            int i = 0;
            while (!quot.IsConstant())
            {
                ++i;
                dQuot = dQuot.Subtract(quot.Derivative());
                Poly factor = UnivariateGCD.PolynomialGCD(quot, dQuot);
                quot = UnivariateDivision.DivideAndRemainder(quot, factor, false)[0]; // can destroy quot in divideAndRemainder
                dQuot = UnivariateDivision.DivideAndRemainder(dQuot, factor, false)[0]; // can destroy dQuot in divideAndRemainder
                if (!factor.IsOne())
                    factorization.AddFactor(factor, i);
            }
        }

        /// <summary>
        /// Performs square-free factorization of a poly which coefficient ring has zero characteristic using Musser's
        /// algorithm.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>square-free decomposition</returns>
        public static PolynomialFactorDecomposition<Poly> SquareFreeFactorizationMusserZeroCharacteristics<Poly extends IUnivariatePolynomial<Poly>>(Poly poly)
        {
            if (!poly.CoefficientRingCharacteristic().IsZero())
                throw new ArgumentException("Characteristics 0 expected");
            if (poly.IsConstant())
                return PolynomialFactorDecomposition.Of(poly);
            Poly content = poly.ContentAsPoly();
            if (poly.SignumOfLC() < 0)
                content = content.Negate();
            poly = poly.Clone().DivideByLC(content);
            if (poly.Degree() <= 1)
                return PolynomialFactorDecomposition.Of(content, poly);
            PolynomialFactorDecomposition<Poly> factorization = PolynomialFactorDecomposition.Of(content);
            SquareFreeFactorizationMusserZeroCharacteristics0(poly, factorization);
            return factorization;
        }

        private static void SquareFreeFactorizationMusserZeroCharacteristics0<Poly extends IUnivariatePolynomial<Poly>>(Poly poly, PolynomialFactorDecomposition<Poly> factorization)
        {
            Poly derivative = poly.Derivative(), gcd = UnivariateGCD.PolynomialGCD(poly, derivative);
            if (gcd.IsConstant())
            {
                factorization.AddFactor(poly, 1);
                return;
            }

            Poly quot = UnivariateDivision.DivideAndRemainder(poly, gcd, false)[0]; // safely destroy (cloned) poly
            int i = 0;
            while (true)
            {
                ++i;
                Poly nextQuot = UnivariateGCD.PolynomialGCD(gcd, quot);
                gcd = UnivariateDivision.DivideAndRemainder(gcd, nextQuot, false)[0]; // safely destroy gcd (reassigned)
                Poly factor = UnivariateDivision.DivideAndRemainder(quot, nextQuot, false)[0]; // safely destroy quot (reassigned further)
                if (!factor.IsConstant())
                    factorization.AddFactor(factor, i);
                if (nextQuot.IsConstant())
                    break;
                quot = nextQuot;
            }
        }

        /// <summary>
        /// Performs square-free factorization of a {@code poly} using Musser's algorithm (both zero and non-zero
        /// characteristic of coefficient ring allowed).
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>square-free decomposition</returns>
        public static PolynomialFactorDecomposition<Poly> SquareFreeFactorizationMusser<Poly extends IUnivariatePolynomial<Poly>>(Poly poly)
        {
            if (CanConvertToZp64(poly))
                return SquareFreeFactorizationMusser(AsOverZp64(poly)).MapTo(Conversions64bit.Convert());
            poly = poly.Clone();
            Poly lc = poly.LcAsPoly();

            //make poly monic
            poly = poly.Monic();
            if (poly.IsConstant())
                return PolynomialFactorDecomposition.Of(lc);
            if (poly.Degree() <= 1)
                return PolynomialFactorDecomposition.Of(lc, poly);
            PolynomialFactorDecomposition<Poly> factorization;

            // x^2 + x^3 -> x^2 (1 + x)
            int exponent = 0;
            while (exponent <= poly.Degree() && poly.IsZeroAt(exponent))
            {
                ++exponent;
            }

            if (exponent == 0)
                factorization = SquareFreeFactorizationMusser0(poly);
            else
            {
                Poly expFree = poly.GetRange(exponent, poly.Degree() + 1);
                factorization = SquareFreeFactorizationMusser0(expFree);
                factorization.AddFactor(poly.CreateMonomial(1), exponent);
            }

            return factorization.SetUnit(lc);
        }

        /// <summary>
        /// {@code poly} will be destroyed
        /// </summary>
        private static PolynomialFactorDecomposition<Poly> SquareFreeFactorizationMusser0<Poly extends IUnivariatePolynomial<Poly>>(Poly poly)
        {
            poly.Monic();
            if (poly.IsConstant())
                return PolynomialFactorDecomposition.Of(poly);
            if (poly.Degree() <= 1)
                return PolynomialFactorDecomposition.Of(poly);
            Poly derivative = poly.Derivative();
            if (!derivative.IsZero())
            {
                Poly gcd = UnivariateGCD.PolynomialGCD(poly, derivative);
                if (gcd.IsConstant())
                    return PolynomialFactorDecomposition.Of(poly);
                Poly quot = UnivariateDivision.DivideAndRemainder(poly, gcd, false)[0]; // can safely destroy poly (not used further)
                PolynomialFactorDecomposition<Poly> result = PolynomialFactorDecomposition.Of(poly.CreateOne());
                int i = 0;

                //if (!quot.isConstant())
                while (true)
                {
                    ++i;
                    Poly nextQuot = UnivariateGCD.PolynomialGCD(gcd, quot);
                    Poly factor = UnivariateDivision.DivideAndRemainder(quot, nextQuot, false)[0]; // can safely destroy quot (reassigned further)
                    if (!factor.IsConstant())
                        result.AddFactor(factor.Monic(), i);
                    gcd = UnivariateDivision.DivideAndRemainder(gcd, nextQuot, false)[0]; // can safely destroy gcd
                    if (nextQuot.IsConstant())
                        break;
                    quot = nextQuot;
                }

                if (!gcd.IsConstant())
                {
                    gcd = PRoot(gcd);
                    PolynomialFactorDecomposition<Poly> gcdFactorization = SquareFreeFactorizationMusser0(gcd);
                    gcdFactorization.RaiseExponents(poly.CoefficientRingCharacteristic().IntValueExact());
                    result.AddAll(gcdFactorization);
                    return result;
                }
                else
                    return result;
            }
            else
            {
                Poly pRoot = PRoot(poly);
                PolynomialFactorDecomposition<Poly> fd = SquareFreeFactorizationMusser0(pRoot);
                fd.RaiseExponents(poly.CoefficientRingCharacteristic().IntValueExact());
                return fd.SetUnit(poly.CreateOne());
            }
        }

        /// <summary>
        /// p-th root of poly
        /// </summary>
        private static Poly PRoot<Poly extends IUnivariatePolynomial<Poly>>(Poly poly)
        {
            if (poly is UnivariatePolynomialZp64)
                return (Poly)PRoot((UnivariatePolynomialZp64)poly);
            else if (poly is UnivariatePolynomial)
                return (Poly)PRoot((UnivariatePolynomial)poly);
            else
                throw new Exception(poly.GetType().ToString());
        }

        /// <summary>
        /// p-th root of poly
        /// </summary>
        private static UnivariatePolynomialZp64 PRoot(UnivariatePolynomialZp64 poly)
        {
            if (poly.ring.modulus > Integer.MAX_VALUE)
                throw new ArgumentException("Too big modulus: " + poly.ring.modulus);
            int modulus = MachineArithmetic.SafeToInt(poly.ring.modulus);
            long[] rootData = new long[poly.degree / modulus + 1];
            Arrays.Fill(rootData, 0);
            for (int i = poly.degree; i >= 0; --i)
                if (poly.data[i] != 0)
                {
                    rootData[i / modulus] = poly.data[i];
                }

            return poly.CreateFromArray(rootData);
        }

        /// <summary>
        /// p-th root of poly
        /// </summary>
        private static UnivariatePolynomial<E> PRoot<E>(UnivariatePolynomial<E> poly)
        {
            if (!poly.CoefficientRingCharacteristic().IsInt())
                throw new ArgumentException("Infinite or too large ring: " + poly.ring);
            Ring<E> ring = poly.ring;

            // p^(m -1) used for computing p-th root of elements
            BigInteger inverseFactor = ring.Cardinality().Divide(ring.Characteristic());
            int modulus = poly.CoefficientRingCharacteristic().IntValueExact();
            E[] rootData = poly.ring.CreateZeroesArray(poly.degree / modulus + 1);
            for (int i = poly.degree; i >= 0; --i)
                if (!poly.ring.IsZero(poly.data[i]))
                {
                    rootData[i / modulus] = ring.Pow(poly.data[i], inverseFactor); // pRoot(poly.data[i], ring);
                }

            return poly.CreateFromArray(rootData);
        }
    }
}