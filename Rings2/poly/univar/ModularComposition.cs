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
    /// Univariate polynomial modular composition.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class ModularComposition
    {
        private ModularComposition()
        {
        }

        /// <summary>
        /// Returns {@code x^{i*modulus} mod polyModulus} for i in {@code [0...degree]}, where {@code degree} is {@code
        /// polyModulus} degree.
        /// </summary>
        /// <param name="polyModulus">the monic modulus</param>
        /// <param name="invMod">pre-conditioned modulus ({@link UnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)})</param>
        /// <returns>{@code x^{i*modulus} mod polyModulus} for i in {@code [0...degree]}, where {@code degree} is {@code
        /// polyModulus} degree</returns>
        /// <remarks>@seeUnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)</remarks>
        public static List<T> XPowers<T extends IUnivariatePolynomial<T>>(T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod)
        {
            return PolyPowers(UnivariatePolynomialArithmetic.CreateMonomialMod(polyModulus.CoefficientRingCardinality(), polyModulus, invMod), polyModulus, invMod, polyModulus.Degree());
        }

        /// <summary>
        /// Returns {@code poly^{i} mod polyModulus} for i in {@code [0...nIterations]}
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <param name="polyModulus">the monic polynomial modulus</param>
        /// <param name="invMod">pre-conditioned modulus ({@link UnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)})</param>
        /// <returns>{@code poly^{i} mod polyModulus} for i in {@code [0...nIterations]}</returns>
        /// <remarks>@seeUnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)</remarks>
        public static List<T> PolyPowers<T extends IUnivariatePolynomial<T>>(T poly, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod, int nIterations)
        {
            List<T> exponents = new List();
            PolyPowers(UnivariatePolynomialArithmetic.PolyMod(poly, polyModulus, invMod, true), polyModulus, invMod, nIterations, exponents);
            return exponents;
        }

        /// <summary>
        /// writes poly^{i} mod polyModulus for i in [0...nIterations] to exponents
        /// </summary>
        private static void PolyPowers<T extends IUnivariatePolynomial<T>>(T polyReduced, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod, int nIterations, List<T> exponents)
        {
            exponents.Add(polyReduced.CreateOne());

            // polyReduced must be reduced!
            T base = polyReduced.Clone(); //polyMod(poly, polyModulus, invMod, true);
            exponents.Add(@base);
            T prev = @base;
            for (int i = 0; i < nIterations; i++)
                exponents.Add(prev = UnivariatePolynomialArithmetic.PolyMod(prev.Clone().Multiply(@base), polyModulus, invMod, false));
        }

        /// <summary>
        /// Returns {@code poly^modulus mod polyModulus} using precomputed monomial powers {@code x^{i*modulus} mod
        /// polyModulus} for i in {@code [0...degree(poly)]}
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <param name="polyModulus">the monic polynomial modulus</param>
        /// <param name="invMod">pre-conditioned modulus ({@link UnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)})</param>
        /// <param name="xPowers">precomputed monomial powers {@code x^{i*modulus} mod polyModulus} for i in {@code
        ///                    [0...degree(poly)]}</param>
        /// <returns>{@code poly^modulus mod polyModulus}</returns>
        /// <remarks>
        /// @see#xPowers(IUnivariatePolynomial, UnivariateDivision.InverseModMonomial)
        /// @seeUnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)
        /// </remarks>
        public static UnivariatePolynomialZp64 PowModulusMod(UnivariatePolynomialZp64 poly, UnivariatePolynomialZp64 polyModulus, UnivariateDivision.InverseModMonomial<UnivariatePolynomialZp64> invMod, List<UnivariatePolynomialZp64> xPowers)
        {
            poly = UnivariatePolynomialArithmetic.PolyMod(poly, polyModulus, invMod, true);
            return PowModulusMod0(poly, polyModulus, invMod, xPowers);
        }

        /// <summary>
        /// doesn't do poly mod polyModulus first
        /// </summary>
        private static UnivariatePolynomialZp64 PowModulusMod0(UnivariatePolynomialZp64 poly, UnivariatePolynomialZp64 polyModulus, UnivariateDivision.InverseModMonomial<UnivariatePolynomialZp64> invMod, List<UnivariatePolynomialZp64> xPowers)
        {
            UnivariatePolynomialZp64 res = poly.CreateZero();
            for (int i = poly.degree; i >= 0; --i)
            {
                if (poly.data[i] == 0)
                    continue;
                res.AddMul(xPowers[i], poly.data[i]);
            }

            return UnivariatePolynomialArithmetic.PolyMod(res, polyModulus, invMod, false);
        }

        /// <summary>
        /// Returns {@code poly^modulus mod polyModulus} using precomputed monomial powers {@code x^{i*modulus} mod
        /// polyModulus} for i in {@code [0...degree(poly)]}
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <param name="polyModulus">the monic polynomial modulus</param>
        /// <param name="invMod">pre-conditioned modulus ({@link UnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)})</param>
        /// <param name="xPowers">precomputed monomial powers {@code x^{i*modulus} mod polyModulus} for i in {@code
        ///                    [0...degree(poly)]}</param>
        /// <returns>{@code poly^modulus mod polyModulus}</returns>
        /// <remarks>
        /// @see#xPowers(IUnivariatePolynomial, UnivariateDivision.InverseModMonomial)
        /// @seeUnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)
        /// </remarks>
        public static UnivariatePolynomial<E> PowModulusMod<E>(UnivariatePolynomial<E> poly, UnivariatePolynomial<E> polyModulus, UnivariateDivision.InverseModMonomial<UnivariatePolynomial<E>> invMod, List<UnivariatePolynomial<E>> xPowers)
        {
            poly = UnivariatePolynomialArithmetic.PolyMod(poly, polyModulus, invMod, true);
            return PowModulusMod0(poly, polyModulus, invMod, xPowers);
        }

        /// <summary>
        /// doesn't do poly mod polyModulus first
        /// </summary>
        private static UnivariatePolynomial<E> PowModulusMod0<E>(UnivariatePolynomial<E> poly, UnivariatePolynomial<E> polyModulus, UnivariateDivision.InverseModMonomial<UnivariatePolynomial<E>> invMod, List<UnivariatePolynomial<E>> xPowers)
        {
            UnivariatePolynomial<E> res = poly.CreateZero();
            for (int i = poly.degree; i >= 0; --i)
            {
                if (poly.ring.IsZero(poly.data[i]))
                    continue;
                res.AddMul(xPowers[i], poly.data[i]);
            }

            return UnivariatePolynomialArithmetic.PolyMod(res, polyModulus, invMod, false);
        }

        /// <summary>
        /// Returns {@code poly^modulus mod polyModulus} using precomputed monomial powers {@code x^{i*modulus} mod
        /// polyModulus} for i in {@code [0...degree(poly)]}
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <param name="polyModulus">the monic polynomial modulus</param>
        /// <param name="invMod">pre-conditioned modulus ({@link UnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)})</param>
        /// <param name="xPowers">precomputed monomial powers {@code x^{i*modulus} mod polyModulus} for i in {@code
        ///                    [0...degree(poly)]}</param>
        /// <returns>{@code poly^modulus mod polyModulus}</returns>
        /// <remarks>
        /// @see#xPowers(IUnivariatePolynomial, UnivariateDivision.InverseModMonomial)
        /// @seeUnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)
        /// </remarks>
        public static T PowModulusMod<T extends IUnivariatePolynomial<T>>(T poly, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod, List<T> xPowers)
        {
            if (poly is UnivariatePolynomialZp64)
                return (T)PowModulusMod((UnivariatePolynomialZp64)poly, (UnivariatePolynomialZp64)polyModulus, (UnivariateDivision.InverseModMonomial<UnivariatePolynomialZp64>)invMod, (List<UnivariatePolynomialZp64>)xPowers);
            else if (poly is UnivariatePolynomial)
                return (T)PowModulusMod((UnivariatePolynomial)poly, (UnivariatePolynomial)polyModulus, (UnivariateDivision.InverseModMonomial)invMod, (List)xPowers);
            else
                throw new Exception();
        }

        private static T PowModulusMod0<T extends IUnivariatePolynomial<T>>(T poly, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod, List<T> xPowers)
        {
            if (poly is UnivariatePolynomialZp64)
                return (T)PowModulusMod0((UnivariatePolynomialZp64)poly, (UnivariatePolynomialZp64)polyModulus, (UnivariateDivision.InverseModMonomial)invMod, (List)xPowers);
            else if (poly is UnivariatePolynomial)
                return (T)PowModulusMod0((UnivariatePolynomial)poly, (UnivariatePolynomial)polyModulus, (UnivariateDivision.InverseModMonomial)invMod, (List)xPowers);
            else
                throw new Exception();
        }

        /// <summary>
        /// Returns modular composition {@code poly(point) mod polyModulus } calculated using Brent & Kung algorithm for
        /// modular composition.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <param name="pointPowers">precomputed powers of evaluation point {@code point^{i} mod polyModulus}</param>
        /// <param name="polyModulus">the monic polynomial modulus</param>
        /// <param name="invMod">pre-conditioned modulus ({@link UnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)})</param>
        /// <param name="tBrentKung">Brent-Kung splitting parameter (optimal choice is ~sqrt(main.degree))</param>
        /// <returns>modular composition {@code poly(point) mod polyModulus }</returns>
        /// <remarks>
        /// @see#polyPowers(IUnivariatePolynomial, IUnivariatePolynomial, UnivariateDivision.InverseModMonomial, int)
        /// @seeUnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)
        /// </remarks>
        public static T CompositionBrentKung<T extends IUnivariatePolynomial<T>>(T poly, List<T> pointPowers, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod, int tBrentKung)
        {
            if (poly.IsConstant())
                return poly;
            List<T> gj = new List();
            int degree = poly.Degree();
            for (int i = 0; i <= degree;)
            {
                int to = i + tBrentKung;
                if (to > (degree + 1))
                    to = degree + 1;
                T g = poly.GetRange(i, to);
                gj.Add(PowModulusMod0(g, polyModulus, invMod, pointPowers));
                i = to;
            }

            T pt = pointPowers[tBrentKung];
            T res = poly.CreateZero();
            for (int i = gj.size() - 1; i >= 0; --i)
                res = UnivariatePolynomialArithmetic.PolyMod(res.Multiply(pt).Add(gj[i]), polyModulus, invMod, false);
            return res;
        }

        /// <summary>
        /// Returns modular composition {@code poly(point) mod polyModulus } calculated using Brent & Kung algorithm for
        /// modular composition.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <param name="point">the evaluation point</param>
        /// <param name="polyModulus">the monic polynomial modulus</param>
        /// <param name="invMod">pre-conditioned modulus ({@link UnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)}
        ///                    )})</param>
        /// <returns>modular composition {@code poly(point) mod polyModulus }</returns>
        /// <remarks>@seeUnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)</remarks>
        public static T CompositionBrentKung<T extends IUnivariatePolynomial<T>>(T poly, T point, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod)
        {
            if (poly.IsConstant())
                return poly;
            int t = SafeToInt(Math.Sqrt(poly.Degree()));
            List<T> hPowers = PolyPowers(point, polyModulus, invMod, t);
            return CompositionBrentKung(poly, hPowers, polyModulus, invMod, t);
        }

        private static int SafeToInt(double dbl)
        {
            if (dbl > Integer.MAX_VALUE || dbl < Integer.MIN_VALUE)
                throw new ArithmeticException("int overflow");
            return (int)dbl;
        }

        /// <summary>
        /// Returns modular composition {@code poly(point) mod polyModulus } calculated with plain Horner scheme.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <param name="point">the evaluation point</param>
        /// <param name="polyModulus">the monic polynomial modulus</param>
        /// <param name="invMod">pre-conditioned modulus ({@link UnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)}
        ///                    )})</param>
        /// <returns>modular composition {@code poly(point) mod polyModulus }</returns>
        /// <remarks>@seeUnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)</remarks>
        public static UnivariatePolynomialZp64 CompositionHorner(UnivariatePolynomialZp64 poly, UnivariatePolynomialZp64 point, UnivariatePolynomialZp64 polyModulus, UnivariateDivision.InverseModMonomial<UnivariatePolynomialZp64> invMod)
        {
            if (poly.IsConstant())
                return poly;
            UnivariatePolynomialZp64 res = poly.CreateZero();
            for (int i = poly.degree; i >= 0; --i)
                res = UnivariatePolynomialArithmetic.PolyMod(res.Multiply(point).AddMonomial(poly.data[i], 0), polyModulus, invMod, false);
            return res;
        }

        /// <summary>
        /// Returns modular composition {@code poly(point) mod polyModulus}. Brent & Kung algorithm used ({@link
        /// #compositionBrentKung(IUnivariatePolynomial, ArrayList, IUnivariatePolynomial,
        /// UnivariateDivision.InverseModMonomial, int)}
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <param name="point">the evaluation point</param>
        /// <param name="polyModulus">the monic polynomial modulus</param>
        /// <param name="invMod">pre-conditioned modulus ({@link UnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)})</param>
        /// <returns>modular composition {@code poly(point) mod polyModulus }</returns>
        /// <remarks>
        /// @see#polyPowers(IUnivariatePolynomial, IUnivariatePolynomial, UnivariateDivision.InverseModMonomial, int)
        /// @seeUnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)
        /// </remarks>
        public static T Composition<T extends IUnivariatePolynomial<T>>(T poly, T point, T polyModulus, UnivariateDivision.InverseModMonomial<T> invMod)
        {
            return CompositionBrentKung(poly, point, polyModulus, invMod);
        }

        /// <summary>
        /// Returns modular composition {@code poly(point) mod polyModulus}. Brent & Kung algorithm used ({@link
        /// #compositionBrentKung(IUnivariatePolynomial, ArrayList, IUnivariatePolynomial,
        /// UnivariateDivision.InverseModMonomial, int)}
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <param name="point">the evaluation point</param>
        /// <param name="polyModulus">the monic polynomial modulus</param>
        /// <returns>modular composition {@code poly(point) mod polyModulus }</returns>
        /// <remarks>
        /// @see#polyPowers(IUnivariatePolynomial, IUnivariatePolynomial, UnivariateDivision.InverseModMonomial, int)
        /// @seeUnivariateDivision#fastDivisionPreConditioning(IUnivariatePolynomial)
        /// </remarks>
        public static T Composition<T extends IUnivariatePolynomial<T>>(T poly, T point, T polyModulus)
        {
            return CompositionBrentKung(poly, point, polyModulus, UnivariateDivision.FastDivisionPreConditioning(point));
        }
    }
}