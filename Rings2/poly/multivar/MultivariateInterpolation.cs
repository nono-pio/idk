using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Poly;
using Gnu.Trove.List.Array;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Poly.Multivar.RoundingMode;
using static Cc.Redberry.Rings.Poly.Multivar.Associativity;
using static Cc.Redberry.Rings.Poly.Multivar.Operator;
using static Cc.Redberry.Rings.Poly.Multivar.TokenType;
using static Cc.Redberry.Rings.Poly.Multivar.SystemInfo;

namespace Cc.Redberry.Rings.Poly.Multivar
{
    /// <summary>
    /// Multivariate interpolation
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class MultivariateInterpolation
    {
        private MultivariateInterpolation()
        {
        }

        private static void CheckInput(object[] points, object[] values)
        {
            if (points.Length != values.Length)
                throw new ArgumentException();
        }

        /// <summary>
        /// Constructs an interpolating polynomial which values at {@code points[i]} are exactly {@code values[i]}. This
        /// method uses Newton's mixed radix iterations.
        /// </summary>
        /// <param name="points">evaluation points</param>
        /// <param name="values">corresponding polynomial values</param>
        /// <returns>the interpolating polynomial</returns>
        public static MultivariatePolynomial<E> InterpolateNewton<E>(int variable, E[] points, MultivariatePolynomial<E>[] values)
        {
            CheckInput(points, values);
            return new Interpolation(variable, values[0]).Update(points, values).GetInterpolatingPolynomial(); //        int length = points.length;
            //
            //        // Newton's representation
            //        MultivariatePolynomial<E>[] mixedRadix = new MultivariatePolynomial[length];
            //        mixedRadix[0] = values[0].clone();
            //        MultivariatePolynomial<E> lins = values[0].createOne();
            //        MultivariatePolynomial<E> poly = values[0].clone();
            //
            //        Ring<E> ring = poly.ring;
            //        for (int k = 1; k < length; ++k) {
            //            E reciprocal = ring.subtract(points[k], points[0]);
            //            MultivariatePolynomial<E> accumulator = mixedRadix[0].clone();
            //            for (int i = 1; i < k; ++i) {
            //                accumulator = accumulator.add(mixedRadix[i].clone().multiply(reciprocal));
            //                reciprocal = ring.multiply(reciprocal, ring.subtract(points[k], points[i]));
            //            }
            //            mixedRadix[k] = values[k].clone().subtract(accumulator).multiply(ring.reciprocal(reciprocal));
            //
            //            lins = lins.multiply(lins.createLinear(variable, ring.negate(points[k - 1]), ring.getOne()));
            //            poly = poly.add(lins.clone().multiply(mixedRadix[k]));
            //        }
            //        return poly;
        }

        /// <summary>
        /// Updatable Newton interpolation
        /// </summary>
        public sealed class Interpolation<E>
        {
            /// <summary>
            /// variable
            /// </summary>
            private readonly int variable;
            /// <summary>
            /// variable
            /// </summary>
            /// <summary>
            /// list of evaluation points
            /// </summary>
            private readonly IList<E> points = new List();
            /// <summary>
            /// variable
            /// </summary>
            /// <summary>
            /// list of evaluation points
            /// </summary>
            /// <summary>
            /// list of values at points
            /// </summary>
            private readonly IList<MultivariatePolynomial<E>> values = new List();
            /// <summary>
            /// variable
            /// </summary>
            /// <summary>
            /// list of evaluation points
            /// </summary>
            /// <summary>
            /// list of values at points
            /// </summary>
            /// <summary>
            /// mixed radix form of interpolating polynomial
            /// </summary>
            private readonly IList<MultivariatePolynomial<E>> mixedRadix = new List();
            /// <summary>
            /// variable
            /// </summary>
            /// <summary>
            /// list of evaluation points
            /// </summary>
            /// <summary>
            /// list of values at points
            /// </summary>
            /// <summary>
            /// mixed radix form of interpolating polynomial
            /// </summary>
            /// <summary>
            /// total modulus (x_i - points[0])*(x_i - points[1])*...
            /// </summary>
            private readonly MultivariatePolynomial<E> lins;
            /// <summary>
            /// variable
            /// </summary>
            /// <summary>
            /// list of evaluation points
            /// </summary>
            /// <summary>
            /// list of values at points
            /// </summary>
            /// <summary>
            /// mixed radix form of interpolating polynomial
            /// </summary>
            /// <summary>
            /// total modulus (x_i - points[0])*(x_i - points[1])*...
            /// </summary>
            /// <summary>
            /// resulting interpolating polynomial
            /// </summary>
            private readonly MultivariatePolynomial<E> poly;
            /// <summary>
            /// variable
            /// </summary>
            /// <summary>
            /// list of evaluation points
            /// </summary>
            /// <summary>
            /// list of values at points
            /// </summary>
            /// <summary>
            /// mixed radix form of interpolating polynomial
            /// </summary>
            /// <summary>
            /// total modulus (x_i - points[0])*(x_i - points[1])*...
            /// </summary>
            /// <summary>
            /// resulting interpolating polynomial
            /// </summary>
            /// <summary>
            /// ring
            /// </summary>
            private readonly Ring<E> ring;
            /// <summary>
            /// variable
            /// </summary>
            /// <summary>
            /// list of evaluation points
            /// </summary>
            /// <summary>
            /// list of values at points
            /// </summary>
            /// <summary>
            /// mixed radix form of interpolating polynomial
            /// </summary>
            /// <summary>
            /// total modulus (x_i - points[0])*(x_i - points[1])*...
            /// </summary>
            /// <summary>
            /// resulting interpolating polynomial
            /// </summary>
            /// <summary>
            /// ring
            /// </summary>
            /// <summary>
            /// Start new interpolation with {@code interpolation[variable = point] = value}
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            public Interpolation(int variable, E point, MultivariatePolynomial<E> value)
            {
                this.variable = variable;
                this.lins = value.CreateOne();
                this.poly = value.Clone();
                this.ring = poly.ring;
                points.Add(point);
                values.Add(value);
                mixedRadix.Add(value.Clone());
            }

            /// <summary>
            /// variable
            /// </summary>
            /// <summary>
            /// list of evaluation points
            /// </summary>
            /// <summary>
            /// list of values at points
            /// </summary>
            /// <summary>
            /// mixed radix form of interpolating polynomial
            /// </summary>
            /// <summary>
            /// total modulus (x_i - points[0])*(x_i - points[1])*...
            /// </summary>
            /// <summary>
            /// resulting interpolating polynomial
            /// </summary>
            /// <summary>
            /// ring
            /// </summary>
            /// <summary>
            /// Start new interpolation with {@code interpolation[variable = point] = value}
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Start new interpolation
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="factory">factory polynomial</param>
            public Interpolation(int variable, MultivariatePolynomial<E> factory)
            {
                this.variable = variable;
                this.lins = factory.CreateOne();
                this.poly = factory.CreateOne();
                this.ring = poly.ring;
            }

            /// <summary>
            /// variable
            /// </summary>
            /// <summary>
            /// list of evaluation points
            /// </summary>
            /// <summary>
            /// list of values at points
            /// </summary>
            /// <summary>
            /// mixed radix form of interpolating polynomial
            /// </summary>
            /// <summary>
            /// total modulus (x_i - points[0])*(x_i - points[1])*...
            /// </summary>
            /// <summary>
            /// resulting interpolating polynomial
            /// </summary>
            /// <summary>
            /// ring
            /// </summary>
            /// <summary>
            /// Start new interpolation with {@code interpolation[variable = point] = value}
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Start new interpolation
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="factory">factory polynomial</param>
            /// <summary>
            /// Start new interpolation
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="factory">factory polynomial</param>
            public Interpolation(int variable, IPolynomialRing<MultivariatePolynomial<E>> factory) : this(variable, factory.Factory())
            {
            }

            /// <summary>
            /// variable
            /// </summary>
            /// <summary>
            /// list of evaluation points
            /// </summary>
            /// <summary>
            /// list of values at points
            /// </summary>
            /// <summary>
            /// mixed radix form of interpolating polynomial
            /// </summary>
            /// <summary>
            /// total modulus (x_i - points[0])*(x_i - points[1])*...
            /// </summary>
            /// <summary>
            /// resulting interpolating polynomial
            /// </summary>
            /// <summary>
            /// ring
            /// </summary>
            /// <summary>
            /// Start new interpolation with {@code interpolation[variable = point] = value}
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Start new interpolation
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="factory">factory polynomial</param>
            /// <summary>
            /// Start new interpolation
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="factory">factory polynomial</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            public Interpolation<E> Update(E point, MultivariatePolynomial<E> value)
            {
                if (points.IsEmpty())
                {
                    poly.Multiply(value);
                    points.Add(point);
                    values.Add(value);
                    mixedRadix.Add(value.Clone());
                    return this;
                }

                E reciprocal = ring.Subtract(point, points[0]);
                MultivariatePolynomial<E> accumulator = mixedRadix[0].Clone();
                for (int i = 1; i < points.Count; ++i)
                {
                    accumulator = accumulator.Add(mixedRadix[i].Clone().Multiply(reciprocal));
                    reciprocal = ring.Multiply(reciprocal, ring.Subtract(point, points[i]));
                    if (ring.IsZero(reciprocal))
                        throw new ArgumentException("Point " + point + " was already used in interpolation.");
                }

                mixedRadix.Add(value.Clone().Subtract(accumulator).Multiply(ring.Reciprocal(reciprocal)));
                lins.Multiply(lins.CreateLinear(variable, ring.Negate(points[points.Count - 1]), ring.GetOne()));
                poly.Add(lins.Clone().Multiply(mixedRadix[mixedRadix.Count - 1]));
                points.Add(point);
                values.Add(value);
                return this;
            }

            /// <summary>
            /// variable
            /// </summary>
            /// <summary>
            /// list of evaluation points
            /// </summary>
            /// <summary>
            /// list of values at points
            /// </summary>
            /// <summary>
            /// mixed radix form of interpolating polynomial
            /// </summary>
            /// <summary>
            /// total modulus (x_i - points[0])*(x_i - points[1])*...
            /// </summary>
            /// <summary>
            /// resulting interpolating polynomial
            /// </summary>
            /// <summary>
            /// ring
            /// </summary>
            /// <summary>
            /// Start new interpolation with {@code interpolation[variable = point] = value}
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Start new interpolation
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="factory">factory polynomial</param>
            /// <summary>
            /// Start new interpolation
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="factory">factory polynomial</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="points">evaluation points</param>
            /// <param name="values">polynomial values at {@code point}</param>
            public Interpolation<E> Update(E[] points, MultivariatePolynomial<E>[] values)
            {
                for (int i = 0; i < points.Length; i++)
                    Update(points[i], values[i]);
                return this;
            }

            /// <summary>
            /// variable
            /// </summary>
            /// <summary>
            /// list of evaluation points
            /// </summary>
            /// <summary>
            /// list of values at points
            /// </summary>
            /// <summary>
            /// mixed radix form of interpolating polynomial
            /// </summary>
            /// <summary>
            /// total modulus (x_i - points[0])*(x_i - points[1])*...
            /// </summary>
            /// <summary>
            /// resulting interpolating polynomial
            /// </summary>
            /// <summary>
            /// ring
            /// </summary>
            /// <summary>
            /// Start new interpolation with {@code interpolation[variable = point] = value}
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Start new interpolation
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="factory">factory polynomial</param>
            /// <summary>
            /// Start new interpolation
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="factory">factory polynomial</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="points">evaluation points</param>
            /// <param name="values">polynomial values at {@code point}</param>
            /// <summary>
            /// Returns variable used in the interpolation
            /// </summary>
            /// <returns>variable used in the interpolation</returns>
            public int GetVariable()
            {
                return variable;
            }

            /// <summary>
            /// variable
            /// </summary>
            /// <summary>
            /// list of evaluation points
            /// </summary>
            /// <summary>
            /// list of values at points
            /// </summary>
            /// <summary>
            /// mixed radix form of interpolating polynomial
            /// </summary>
            /// <summary>
            /// total modulus (x_i - points[0])*(x_i - points[1])*...
            /// </summary>
            /// <summary>
            /// resulting interpolating polynomial
            /// </summary>
            /// <summary>
            /// ring
            /// </summary>
            /// <summary>
            /// Start new interpolation with {@code interpolation[variable = point] = value}
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Start new interpolation
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="factory">factory polynomial</param>
            /// <summary>
            /// Start new interpolation
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="factory">factory polynomial</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="points">evaluation points</param>
            /// <param name="values">polynomial values at {@code point}</param>
            /// <summary>
            /// Returns variable used in the interpolation
            /// </summary>
            /// <returns>variable used in the interpolation</returns>
            /// <summary>
            /// Returns resulting interpolating polynomial
            /// </summary>
            /// <returns>interpolating polynomial</returns>
            public MultivariatePolynomial<E> GetInterpolatingPolynomial()
            {
                return poly;
            }

            /// <summary>
            /// variable
            /// </summary>
            /// <summary>
            /// list of evaluation points
            /// </summary>
            /// <summary>
            /// list of values at points
            /// </summary>
            /// <summary>
            /// mixed radix form of interpolating polynomial
            /// </summary>
            /// <summary>
            /// total modulus (x_i - points[0])*(x_i - points[1])*...
            /// </summary>
            /// <summary>
            /// resulting interpolating polynomial
            /// </summary>
            /// <summary>
            /// ring
            /// </summary>
            /// <summary>
            /// Start new interpolation with {@code interpolation[variable = point] = value}
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Start new interpolation
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="factory">factory polynomial</param>
            /// <summary>
            /// Start new interpolation
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="factory">factory polynomial</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="points">evaluation points</param>
            /// <param name="values">polynomial values at {@code point}</param>
            /// <summary>
            /// Returns variable used in the interpolation
            /// </summary>
            /// <returns>variable used in the interpolation</returns>
            /// <summary>
            /// Returns resulting interpolating polynomial
            /// </summary>
            /// <returns>interpolating polynomial</returns>
            /// <summary>
            /// Returns the list of evaluation points used in interpolation
            /// </summary>
            /// <returns>list of evaluation points used in interpolation</returns>
            public IList<E> GetPoints()
            {
                return points;
            }

            /// <summary>
            /// variable
            /// </summary>
            /// <summary>
            /// list of evaluation points
            /// </summary>
            /// <summary>
            /// list of values at points
            /// </summary>
            /// <summary>
            /// mixed radix form of interpolating polynomial
            /// </summary>
            /// <summary>
            /// total modulus (x_i - points[0])*(x_i - points[1])*...
            /// </summary>
            /// <summary>
            /// resulting interpolating polynomial
            /// </summary>
            /// <summary>
            /// ring
            /// </summary>
            /// <summary>
            /// Start new interpolation with {@code interpolation[variable = point] = value}
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Start new interpolation
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="factory">factory polynomial</param>
            /// <summary>
            /// Start new interpolation
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="factory">factory polynomial</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="points">evaluation points</param>
            /// <param name="values">polynomial values at {@code point}</param>
            /// <summary>
            /// Returns variable used in the interpolation
            /// </summary>
            /// <returns>variable used in the interpolation</returns>
            /// <summary>
            /// Returns resulting interpolating polynomial
            /// </summary>
            /// <returns>interpolating polynomial</returns>
            /// <summary>
            /// Returns the list of evaluation points used in interpolation
            /// </summary>
            /// <returns>list of evaluation points used in interpolation</returns>
            /// <summary>
            /// Returns the list of polynomial values at interpolation points
            /// </summary>
            /// <returns>the list of polynomial values at interpolation points</returns>
            public IList<MultivariatePolynomial<E>> GetValues()
            {
                return values;
            }

            /// <summary>
            /// variable
            /// </summary>
            /// <summary>
            /// list of evaluation points
            /// </summary>
            /// <summary>
            /// list of values at points
            /// </summary>
            /// <summary>
            /// mixed radix form of interpolating polynomial
            /// </summary>
            /// <summary>
            /// total modulus (x_i - points[0])*(x_i - points[1])*...
            /// </summary>
            /// <summary>
            /// resulting interpolating polynomial
            /// </summary>
            /// <summary>
            /// ring
            /// </summary>
            /// <summary>
            /// Start new interpolation with {@code interpolation[variable = point] = value}
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Start new interpolation
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="factory">factory polynomial</param>
            /// <summary>
            /// Start new interpolation
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="factory">factory polynomial</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="points">evaluation points</param>
            /// <param name="values">polynomial values at {@code point}</param>
            /// <summary>
            /// Returns variable used in the interpolation
            /// </summary>
            /// <returns>variable used in the interpolation</returns>
            /// <summary>
            /// Returns resulting interpolating polynomial
            /// </summary>
            /// <returns>interpolating polynomial</returns>
            /// <summary>
            /// Returns the list of evaluation points used in interpolation
            /// </summary>
            /// <returns>list of evaluation points used in interpolation</returns>
            /// <summary>
            /// Returns the list of polynomial values at interpolation points
            /// </summary>
            /// <returns>the list of polynomial values at interpolation points</returns>
            /// <summary>
            /// Returns the number of interpolation points used
            /// </summary>
            /// <returns>number of interpolation points used</returns>
            public int NumberOfPoints()
            {
                return points.Count;
            }
        }

        /// <summary>
        /// Updatable Newton interpolation
        /// </summary>
        public sealed class InterpolationZp64
        {
            /// <summary>
            /// variable
            /// </summary>
            private readonly int variable;
            /// <summary>
            /// list of evaluation points
            /// </summary>
            private readonly TLongArrayList points = new TLongArrayList();
            /// <summary>
            /// list of values at points
            /// </summary>
            private readonly IList<MultivariatePolynomialZp64> values = new List();
            /// <summary>
            /// mixed radix form of interpolating polynomial
            /// </summary>
            private readonly IList<MultivariatePolynomialZp64> mixedRadix = new List();
            /// <summary>
            /// total modulus (x_i - points[0])*(x_i - points[1])*...
            /// </summary>
            private readonly MultivariatePolynomialZp64 lins;
            /// <summary>
            /// resulting interpolating polynomial
            /// </summary>
            private readonly MultivariatePolynomialZp64 poly;
            /// <summary>
            /// ring
            /// </summary>
            private readonly IntegersZp64 ring;
            /// <summary>
            /// Start new interpolation with {@code interpolation[variable = point] = value}
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            public InterpolationZp64(int variable, long point, MultivariatePolynomialZp64 value)
            {
                this.variable = variable;
                this.lins = value.CreateOne();
                this.poly = value.Clone();
                this.ring = poly.ring;
                points.Add(point);
                values.Add(value);
                mixedRadix.Add(value.Clone());
            }

            /// <summary>
            /// Start new interpolation
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="factory">factory polynomial</param>
            public InterpolationZp64(int variable, MultivariatePolynomialZp64 factory)
            {
                this.variable = variable;
                this.lins = factory.CreateOne();
                this.poly = factory.CreateOne();
                this.ring = poly.ring;
            }

            /// <summary>
            /// Start new interpolation
            /// </summary>
            /// <param name="variable">interpolating variable</param>
            /// <param name="factory">factory polynomial</param>
            public InterpolationZp64(int variable, IPolynomialRing<MultivariatePolynomialZp64> factory) : this(variable, factory.Factory())
            {
            }

            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            public InterpolationZp64 Update(long point, MultivariatePolynomialZp64 value)
            {
                if (points.IsEmpty())
                {
                    poly.Multiply(value);
                    points.Add(point);
                    values.Add(value);
                    mixedRadix.Add(value.Clone());
                    return this;
                }

                long reciprocal = ring.Subtract(point, points[0]);
                MultivariatePolynomialZp64 accumulator = mixedRadix[0].Clone();
                for (int i = 1; i < points.Count; ++i)
                {
                    accumulator = accumulator.Add(mixedRadix[i].Clone().Multiply(reciprocal));
                    reciprocal = ring.Multiply(reciprocal, ring.Subtract(point, points[i]));
                }

                if (reciprocal == 0)
                    throw new ArgumentException("Point " + point + " was already used in interpolation.");
                mixedRadix.Add(value.Clone().Subtract(accumulator).Multiply(ring.Reciprocal(reciprocal)));
                lins.Multiply(lins.CreateLinear(variable, ring.Negate(points[points.Count - 1]), 1));
                poly.Add(lins.Clone().Multiply(mixedRadix[mixedRadix.Count - 1]));
                points.Add(point);
                values.Add(value);
                return this;
            }

            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="points">evaluation points</param>
            /// <param name="values">polynomial values at {@code point}</param>
            public InterpolationZp64 Update(long[] points, MultivariatePolynomialZp64[] values)
            {
                for (int i = 0; i < points.Length; i++)
                    Update(points[i], values[i]);
                return this;
            }

            /// <summary>
            /// Returns variable used in the interpolation
            /// </summary>
            /// <returns>variable used in the interpolation</returns>
            public int GetVariable()
            {
                return variable;
            }

            /// <summary>
            /// Returns resulting interpolating polynomial
            /// </summary>
            /// <returns>interpolating polynomial</returns>
            public MultivariatePolynomialZp64 GetInterpolatingPolynomial()
            {
                return poly;
            }

            /// <summary>
            /// Returns the list of evaluation points used in interpolation
            /// </summary>
            /// <returns>list of evaluation points used in interpolation</returns>
            public TLongArrayList GetPoints()
            {
                return points;
            }

            /// <summary>
            /// Returns the list of polynomial values at interpolation points
            /// </summary>
            /// <returns>the list of polynomial values at interpolation points</returns>
            public IList<MultivariatePolynomialZp64> GetValues()
            {
                return values;
            }

            /// <summary>
            /// Returns the number of interpolation points used
            /// </summary>
            /// <returns>number of interpolation points used</returns>
            public int NumberOfPoints()
            {
                return points.Count;
            }
        }
    }
}