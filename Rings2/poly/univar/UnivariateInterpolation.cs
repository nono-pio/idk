

namespace Cc.Redberry.Rings.Poly.Univar
{
    /// <summary>
    /// Univariate polynomial interpolation.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class UnivariateInterpolation
    {
        private UnivariateInterpolation()
        {
        }

        private static void CheckInput<T>(T[] points, T[] values)
        {
            if (points.Length != values.Length)
                throw new ArgumentException();
        }

        /// <summary>
        /// Constructs an interpolating polynomial which values at {@code points[i]} are exactly {@code values[i]}. This
        /// method uses Lagrange's interpolation formula.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="points">evaluation points</param>
        /// <param name="values">corresponding polynomial values</param>
        /// <returns>the interpolating polynomial</returns>
        public static UnivariatePolynomialZp64 InterpolateLagrange(long modulus, long[] points, long[] values)
        {
            CheckInput(points, values);
            int length = points.Length;
            IntegersZp64 ring = new IntegersZp64(modulus);
            UnivariatePolynomialZp64 result = UnivariatePolynomialZp64.Zero(ring);
            for (int i = 0; i < length; ++i)
            {
                UnivariatePolynomialZp64 interpolant = UnivariatePolynomialZp64.Constant(modulus, values[i]);
                for (int j = 0; j < length; ++j)
                {
                    if (j == i)
                        continue;
                    UnivariatePolynomialZp64 linear = result.CreateLinear(ring.Negate(points[j]), 1).Divide(ring.Subtract(points[i], points[j]));
                    interpolant = interpolant.Multiply(linear);
                }

                result = result.Add(interpolant);
            }

            return result;
        }

        /// <summary>
        /// Constructs an interpolating polynomial which values at {@code points[i]} are exactly {@code values[i]}. This
        /// method uses Lagrange's interpolation formula.
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="points">evaluation points</param>
        /// <param name="values">corresponding polynomial values</param>
        /// <returns>the interpolating polynomial</returns>
        public static UnivariatePolynomial<E> InterpolateLagrange<E>(Ring<E> ring, E[] points, E[] values)
        {
            CheckInput(points, values);
            int length = points.Length;
            UnivariatePolynomial<E> result = UnivariatePolynomial<E>.Zero(ring);
            for (int i = 0; i < length; ++i)
            {
                UnivariatePolynomial<E> interpolant = UnivariatePolynomial<E>.Constant(ring, values[i]);
                for (int j = 0; j < length; ++j)
                {
                    if (j == i)
                        continue;
                    UnivariatePolynomial<E> linear = result.CreateLinear(ring.Negate(points[j]), ring.GetOne()).DivideExact(ring.Subtract(points[i], points[j]));
                    interpolant = interpolant.Multiply(linear);
                }

                result = result.Add(interpolant);
            }

            return result;
        }

        /// <summary>
        /// Constructs an interpolating polynomial which values at {@code points[i]} are exactly {@code values[i]}. This
        /// method uses Newton's mixed radix iterations.
        /// </summary>
        /// <param name="modulus">the modulus</param>
        /// <param name="points">evaluation points</param>
        /// <param name="values">corresponding polynomial values</param>
        /// <returns>the interpolating polynomial</returns>
        public static UnivariatePolynomialZp64 InterpolateNewton(long modulus, long[] points, long[] values)
        {
            return InterpolateNewton(new IntegersZp64(modulus), points, values);
        }

        /// <summary>
        /// Constructs an interpolating polynomial which values at {@code points[i]} are exactly {@code values[i]}. This
        /// method uses Newton's mixed radix iterations.
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="points">evaluation points</param>
        /// <param name="values">corresponding polynomial values</param>
        /// <returns>the interpolating polynomial</returns>
        public static UnivariatePolynomialZp64 InterpolateNewton(IntegersZp64 ring, long[] points, long[] values)
        {
            CheckInput(points, values);
            return new InterpolationZp64(ring).Update(points, values).GetInterpolatingPolynomial();
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
        /// <param name="ring">the ring</param>
        /// <param name="points">evaluation points</param>
        /// <param name="values">corresponding polynomial values</param>
        /// <returns>the interpolating polynomial</returns>
        public static UnivariatePolynomial<E> InterpolateNewton<E>(Ring<E> ring, E[] points, E[] values)
        {
            CheckInput(points, values);
            return new Interpolation<E>(ring).Update(points, values).GetInterpolatingPolynomial();
        }

        /// <summary>
        /// Updatable Newton interpolation
        /// </summary>
        public sealed class InterpolationZp64
        {
            private readonly IntegersZp64 ring;
            /// <summary>
            /// list of evaluation points
            /// </summary>
            private readonly TLongArrayList points = new TLongArrayList();
            /// <summary>
            /// list of values at points
            /// </summary>
            private readonly TLongArrayList values = new TLongArrayList();
            /// <summary>
            /// mixed radix form of interpolating polynomial
            /// </summary>
            private readonly TLongArrayList mixedRadix = new TLongArrayList();
            /// <summary>
            /// total modulus (x_i - points[0])*(x_i - points[1])*...
            /// </summary>
            private readonly UnivariatePolynomialZp64 lins;
            /// <summary>
            /// resulting interpolating polynomial
            /// </summary>
            private readonly UnivariatePolynomialZp64 poly;
            /// <summary>
            /// Start new interpolation with {@code interpolation[point] = value}
            /// </summary>
            /// <param name="ring">the ring</param>
            public InterpolationZp64(IntegersZp64 ring)
            {
                this.ring = ring;
                this.lins = UnivariatePolynomialZp64.One(ring);
                this.poly = UnivariatePolynomialZp64.One(ring);
            }

            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            public InterpolationZp64 Update(long point, long value)
            {
                if (points.IsEmpty())
                {
                    poly.Multiply(value);
                    points.Add(point);
                    values.Add(value);
                    mixedRadix.Add(value);
                    return this;
                }

                long reciprocal = poly.Subtract(point, points[0]);
                long accumulator = mixedRadix[0];
                for (int i = 1; i < points.Count; ++i)
                {
                    accumulator = ring.Add(accumulator, ring.Multiply(mixedRadix[i], reciprocal));
                    reciprocal = ring.Multiply(reciprocal, ring.Subtract(point, points[i]));
                }

                if (reciprocal == 0)
                    throw new ArgumentException("Point " + point + " was already used in interpolation.");
                reciprocal = ring.Reciprocal(reciprocal);
                mixedRadix.Add(ring.Multiply(reciprocal, ring.Subtract(value, accumulator)));
                lins.Multiply(lins.CreateLinear(ring.Negate(points[points.Count - 1]), 1));
                poly.Add(lins.Clone().Multiply(mixedRadix[mixedRadix.Count - 1]));
                points.Add(point);
                values.Add(value);
                return this;
            }

            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="points">evaluation points</param>
            /// <param name="values">polynomial values at {@code points}</param>
            public InterpolationZp64 Update(long[] points, long[] values)
            {
                for (int i = 0; i < points.Length; i++)
                    Update(points[i], values[i]);
                return this;
            }

            /// <summary>
            /// Returns resulting interpolating polynomial
            /// </summary>
            /// <returns>interpolating polynomial</returns>
            public UnivariatePolynomialZp64 GetInterpolatingPolynomial()
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
            public TLongArrayList GetValues()
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

        /// <summary>
        /// Updatable Newton interpolation
        /// </summary>
        public sealed class Interpolation<E>
        {
            private readonly Ring<E> ring;
            /// <summary>
            /// list of evaluation points
            /// </summary>
            private readonly List<E> points = new List<E>();
            /// <summary>
            /// list of evaluation points
            /// </summary>
            /// <summary>
            /// list of values at points
            /// </summary>
            private readonly List<E> values = new List<E>();
            /// <summary>
            /// list of evaluation points
            /// </summary>
            /// <summary>
            /// list of values at points
            /// </summary>
            /// <summary>
            /// mixed radix form of interpolating polynomial
            /// </summary>
            private readonly List<E> mixedRadix = new List<E>();
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
            private readonly UnivariatePolynomial<E> lins;
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
            private readonly UnivariatePolynomial<E> poly;
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
            /// Start new interpolation with {@code interpolation[point] = value}
            /// </summary>
            /// <param name="ring">the ring</param>
            public Interpolation(Ring<E> ring)
            {
                this.ring = ring;
                this.lins = UnivariatePolynomial<E>.One(ring);
                this.poly = UnivariatePolynomial<E>.One(ring);
            }

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
            /// Start new interpolation with {@code interpolation[point] = value}
            /// </summary>
            /// <param name="ring">the ring</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            public Interpolation<E> Update(E point, E value)
            {
                if (points.Count == 0)
                {
                    points.Add(point);
                    values.Add(value);
                    mixedRadix.Add(value);
                    poly.Multiply(value);
                    return this;
                }

                E reciprocal = ring.Subtract(point, points[0]);
                E accumulator = mixedRadix[0];
                for (int i = 1; i < points.Count; ++i)
                {
                    accumulator = ring.Add(accumulator, ring.Multiply(mixedRadix[i], reciprocal));
                    reciprocal = ring.Multiply(reciprocal, ring.Subtract(point, points[i]));
                }

                if (ring.IsZero(reciprocal))
                    throw new ArgumentException("Point " + point + " was already used in interpolation.");
                reciprocal = ring.Reciprocal(reciprocal);
                mixedRadix.Add(ring.Multiply(reciprocal, ring.Subtract(value, accumulator)));
                lins.Multiply(lins.CreateLinear(ring.Negate(points[points.Count - 1]), ring.GetOne()));
                poly.Add(lins.Clone().Multiply(mixedRadix[mixedRadix.Count - 1]));
                points.Add(point);
                values.Add(value);
                return this;
            }

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
            /// Start new interpolation with {@code interpolation[point] = value}
            /// </summary>
            /// <param name="ring">the ring</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="points">evaluation points</param>
            /// <param name="values">polynomial values at {@code points}</param>
            public Interpolation<E> Update(E[] points, E[] values)
            {
                for (int i = 0; i < points.Length; i++)
                    Update(points[i], values[i]);
                return this;
            }

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
            /// Start new interpolation with {@code interpolation[point] = value}
            /// </summary>
            /// <param name="ring">the ring</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="points">evaluation points</param>
            /// <param name="values">polynomial values at {@code points}</param>
            /// <summary>
            /// Returns resulting interpolating polynomial
            /// </summary>
            /// <returns>interpolating polynomial</returns>
            public UnivariatePolynomial<E> GetInterpolatingPolynomial()
            {
                return poly;
            }

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
            /// Start new interpolation with {@code interpolation[point] = value}
            /// </summary>
            /// <param name="ring">the ring</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="points">evaluation points</param>
            /// <param name="values">polynomial values at {@code points}</param>
            /// <summary>
            /// Returns resulting interpolating polynomial
            /// </summary>
            /// <returns>interpolating polynomial</returns>
            /// <summary>
            /// Returns the list of evaluation points used in interpolation
            /// </summary>
            /// <returns>list of evaluation points used in interpolation</returns>
            public List<E> GetPoints()
            {
                return points;
            }

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
            /// Start new interpolation with {@code interpolation[point] = value}
            /// </summary>
            /// <param name="ring">the ring</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="points">evaluation points</param>
            /// <param name="values">polynomial values at {@code points}</param>
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
            public List<E> GetValues()
            {
                return values;
            }

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
            /// Start new interpolation with {@code interpolation[point] = value}
            /// </summary>
            /// <param name="ring">the ring</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="point">evaluation point</param>
            /// <param name="value">polynomial value at {@code point}</param>
            /// <summary>
            /// Updates interpolation, so that interpolating polynomial satisfies {@code interpolation[point] = value}
            /// </summary>
            /// <param name="points">evaluation points</param>
            /// <param name="values">polynomial values at {@code points}</param>
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
    }
}