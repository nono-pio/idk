using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Poly.Multivar;
using Cc.Redberry.Rings.Poly.Univar;
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
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class Util
    {
        private Util()
        {
        }

        public static void EnsureOverFiniteField(params IPolynomial[] polys)
        {
            foreach (IPolynomial poly in polys)
                if (!poly.IsOverFiniteField())
                    throw new ArgumentException("Polynomial over finite field is expected; " + poly.GetType());
        }

        public static void EnsureOverField(params IPolynomial[] polys)
        {
            foreach (IPolynomial poly in polys)
                if (!poly.IsOverField())
                    throw new ArgumentException("Polynomial over finite field is expected; " + poly.GetType());
        }

        public static void EnsureOverZ(params IPolynomial[] polys)
        {
            foreach (IPolynomial poly in polys)
                if (!poly.IsOverZ())
                    throw new ArgumentException("Polynomial over Z is expected, but got " + poly.GetType());
        }

        /// <summary>
        /// Test whether poly is over Zp with modulus less then 2^63
        /// </summary>
        public static bool CanConvertToZp64(IPolynomial poly)
        {
            Ring ring;
            if (poly is UnivariatePolynomial)
                ring = ((UnivariatePolynomial)poly).ring;
            else if (poly is MultivariatePolynomial)
                ring = ((MultivariatePolynomial)poly).ring;
            else
                return false;
            return ring is IntegersZp && ((IntegersZp)ring).modulus.BitLength() < MachineArithmetic.MAX_SUPPORTED_MODULUS_BITS;
        }

        /// <summary>
        /// Whether coefficient domain is rationals
        /// </summary>
        public static bool IsOverRationals<T extends IPolynomial<T>>(T poly)
        {
            if (poly is UnivariatePolynomial && ((UnivariatePolynomial)poly).ring is Rationals)
                return true;
            else if (poly is MultivariatePolynomial && ((MultivariatePolynomial)poly).ring is Rationals)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Whether coefficient domain is F(alpha)
        /// </summary>
        public static bool IsOverSimpleFieldExtension<T extends IPolynomial<T>>(T poly)
        {
            if (poly is UnivariatePolynomial && ((UnivariatePolynomial)poly).ring is SimpleFieldExtension)
                return true;
            else if (poly is MultivariatePolynomial && ((MultivariatePolynomial)poly).ring is SimpleFieldExtension)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Whether coefficient domain is F(alpha1, alpha2, ...)
        /// </summary>
        public static bool IsOverMultipleFieldExtension<T extends IPolynomial<T>>(T poly)
        {
            if (poly is UnivariatePolynomial && ((UnivariatePolynomial)poly).ring is MultipleFieldExtension)
                return true;
            else if (poly is MultivariatePolynomial && ((MultivariatePolynomial)poly).ring is MultipleFieldExtension)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Whether coefficient domain is Q(alpha)
        /// </summary>
        public static bool IsOverSimpleNumberField<T extends IPolynomial<T>>(T poly)
        {
            if (poly is UnivariatePolynomial && ((UnivariatePolynomial)poly).ring is AlgebraicNumberField && IsOverQ(((AlgebraicNumberField)((UnivariatePolynomial)poly).ring).GetMinimalPolynomial()))
                return true;
            else if (poly is MultivariatePolynomial && ((MultivariatePolynomial)poly).ring is AlgebraicNumberField && IsOverQ(((AlgebraicNumberField)((MultivariatePolynomial)poly).ring).GetMinimalPolynomial()))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Whether coefficient domain is Q(alpha)
        /// </summary>
        public static bool IsOverRingOfIntegersOfSimpleNumberField<T extends IPolynomial<T>>(T poly)
        {
            if (poly is UnivariatePolynomial && ((UnivariatePolynomial)poly).ring is AlgebraicNumberField && IsOverZ(((AlgebraicNumberField)((UnivariatePolynomial)poly).ring).GetMinimalPolynomial()))
                return true;
            else if (poly is MultivariatePolynomial && ((MultivariatePolynomial)poly).ring is AlgebraicNumberField && IsOverZ(((AlgebraicNumberField)((MultivariatePolynomial)poly).ring).GetMinimalPolynomial()))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Whether coefficient domain is Q
        /// </summary>
        public static bool IsOverQ<T extends IPolynomial<T>>(T poly)
        {
            object rep;
            if (poly is UnivariatePolynomial)
                rep = ((UnivariatePolynomial)poly).ring.GetOne();
            else if (poly is MultivariatePolynomial)
                rep = ((MultivariatePolynomial)poly).ring.GetOne();
            else
                return false;
            if (!(rep is Rational))
                return false;
            return ((Rational)rep).Numerator() is BigInteger;
        }

        /// <summary>
        /// Whether coefficient domain is Z
        /// </summary>
        public static bool IsOverZ<T extends IPolynomial<T>>(T poly)
        {
            return poly.IsOverZ();
        }

        public sealed class Tuple2<A, B>
        {
            public readonly A _1;
            public readonly B _2;
            public Tuple2(A _1, B _2)
            {
                this._1 = _1;
                this._2 = _2;
            }
        }

        /// <summary>
        /// Brings polynomial with rational coefficients to common denominator
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>(reduced poly, common denominator)</returns>
        public static Tuple2<UnivariatePolynomial<E>, E> ToCommonDenominator<E>(UnivariatePolynomial<Rational<E>> poly)
        {
            Ring<Rational<E>> field = poly.ring;
            Ring<E> integralRing = field.GetOne().ring;
            E denominator = integralRing.GetOne();
            for (int i = 0; i <= poly.Degree(); i++)
                if (!poly.IsZeroAt(i))
                    denominator = integralRing.Lcm(denominator, poly[i].Denominator());
            E[] data = integralRing.CreateArray(poly.Degree() + 1);
            for (int i = 0; i <= poly.Degree(); i++)
            {
                Rational<E> cf = poly[i].Multiply(denominator);
                data[i] = cf.Numerator();
            }

            return new Tuple2(UnivariatePolynomial.CreateUnsafe(integralRing, data), denominator);
        }

        /// <summary>
        /// Returns a common denominator of given poly
        /// </summary>
        public static E CommonDenominator<E>(UnivariatePolynomial<Rational<E>> poly)
        {
            Ring<Rational<E>> field = poly.ring;
            Ring<E> integralRing = field.GetOne().ring;
            E denominator = integralRing.GetOne();
            for (int i = 0; i <= poly.Degree(); i++)
                if (!poly.IsZeroAt(i))
                    denominator = integralRing.Lcm(denominator, poly[i].Denominator());
            return denominator;
        }

        /// <summary>
        /// Returns a common denominator of given poly
        /// </summary>
        public static E CommonDenominator<E>(MultivariatePolynomial<Rational<E>> poly)
        {
            Ring<Rational<E>> field = poly.ring;
            Ring<E> integralRing = field.GetOne().ring;
            E denominator = integralRing.GetOne();
            foreach (Rational<E> cf in poly.Coefficients())
                denominator = integralRing.Lcm(denominator, cf.Denominator());
            return denominator;
        }

        /// <summary>
        /// Brings polynomial with rational coefficients to common denominator
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>(reduced poly, common denominator)</returns>
        public static Tuple2<MultivariatePolynomial<E>, E> ToCommonDenominator<E>(MultivariatePolynomial<Rational<E>> poly)
        {
            Ring<Rational<E>> field = poly.ring;
            Ring<E> integralRing = field.GetOne().ring;
            E denominator = integralRing.GetOne();
            foreach (Rational<E> cf in poly.Coefficients())
                denominator = integralRing.Lcm(denominator, cf.Denominator());
            E d = denominator;
            MultivariatePolynomial<E> integral = poly.MapCoefficients(integralRing, (cf) =>
            {
                Rational<E> r = cf.Multiply(d);
                return r.Numerator();
            });
            return new Tuple2(integral, denominator);
        }

        public static UnivariatePolynomial<Rational<E>> AsOverRationals<E>(Ring<Rational<E>> field, UnivariatePolynomial<E> poly)
        {
            return poly.MapCoefficients(field, (cf) => new Rational(poly.ring, cf));
        }

        public static MultivariatePolynomial<Rational<E>> AsOverRationals<E>(Ring<Rational<E>> field, MultivariatePolynomial<E> poly)
        {
            return poly.MapCoefficients(field, (cf) => new Rational(poly.ring, cf));
        }

        public static UnivariatePolynomial<Rational<E>> DivideOverRationals<E>(Ring<Rational<E>> field, UnivariatePolynomial<E> poly, E denominator)
        {
            return poly.MapCoefficients(field, (cf) => new Rational(poly.ring, cf, denominator));
        }

        public static MultivariatePolynomial<Rational<E>> DivideOverRationals<E>(Ring<Rational<E>> field, MultivariatePolynomial<E> poly, E denominator)
        {
            return poly.MapCoefficients(field, (cf) => new Rational(poly.ring, cf, denominator));
        }
    }
}