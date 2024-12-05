namespace Rings.poly.univar;


public static class UnivariateInterpolation {
    
    public static UnivariatePolynomialZp64 interpolateLagrange(long modulus, long[] points, long[] values) {
        checkInput(points, values);

        int Length = points.Length;
        IntegersZp64 ring = new IntegersZp64(modulus);
        UnivariatePolynomialZp64 result = UnivariatePolynomialZp64.zero(ring);
        for (int i = 0; i < Length; ++i) {
            UnivariatePolynomialZp64 interpolant = UnivariatePolynomialZp64.constant(modulus, values[i]);
            for (int j = 0; j < Length; ++j) {
                if (j == i)
                    continue;
                UnivariatePolynomialZp64 linear = result
                        .createLinear(ring.negate(points[j]), 1)
                        .divide(ring.subtract(points[i], points[j]));
                interpolant = interpolant.multiply(linear);
            }
            result = result.add(interpolant);
        }
        return result;
    }

    
    public static  UnivariatePolynomial<E> interpolateLagrange<E>(Ring<E> ring, E[] points, E[] values) {
        checkInput(points, values);

        int Length = points.Length;
        UnivariatePolynomial<E> result = UnivariatePolynomial<E>.zero(ring);
        for (int i = 0; i < Length; ++i) {
            UnivariatePolynomial<E> interpolant = UnivariatePolynomial<E>.constant(ring, values[i]);
            for (int j = 0; j < Length; ++j) {
                if (j == i)
                    continue;
                UnivariatePolynomial<E> linear = result
                        .createLinear(ring.negate(points[j]), ring.getOne())
                        .divideExact(ring.subtract(points[i], points[j]));
                interpolant = interpolant.multiply(linear);
            }
            result = result.add(interpolant);
        }
        return result;
    }

    
    public static UnivariatePolynomialZp64 interpolateNewton(long modulus, long[] points, long[] values) {
        return interpolateNewton(new IntegersZp64(modulus), points, values);
    }

    
    public static UnivariatePolynomialZp64 interpolateNewton(IntegersZp64 ring, long[] points, long[] values) {
        checkInput(points, values);
        return new InterpolationZp64(ring)
                .update(points, values)
                .getInterpolatingPolynomial();
    }

    private static void checkInput<E>(E[] points, E[] values) {
        if (points.Length != values.Length)
            throw new ArgumentException();
    }

    
    public static  UnivariatePolynomial<E> interpolateNewton<E>(Ring<E> ring, E[] points, E[] values) {
        checkInput(points, values);
        return new Interpolation<E>(ring)
                .update(points, values)
                .getInterpolatingPolynomial();
    }

    
    public class InterpolationZp64 {
        private readonly IntegersZp64 ring;
        
        private readonly TLongArrayList points = new TLongArrayList();
        
        private readonly TLongArrayList values = new TLongArrayList();
        
        private readonly TLongArrayList mixedRadix = new TLongArrayList();
        
        private readonly UnivariatePolynomialZp64 lins;
        
        private readonly UnivariatePolynomialZp64 poly;

        
        public InterpolationZp64(IntegersZp64 ring) {
            this.ring = ring;
            this.lins = UnivariatePolynomialZp64.one(ring);
            this.poly = UnivariatePolynomialZp64.one(ring);
        }

        
        public InterpolationZp64 update(long point, long value) {
            if (points.isEmpty()) {
                poly.multiply(value);
                points.add(point);
                values.add(value);
                mixedRadix.add(value);
                return this;
            }
            long reciprocal = poly.subtract(point, points.get(0));
            long accumulator = mixedRadix.get(0);
            for (int i = 1; i < points.size(); ++i) {
                accumulator = ring.add(accumulator, ring.multiply(mixedRadix.get(i), reciprocal));
                reciprocal = ring.multiply(reciprocal, ring.subtract(point, points.get(i)));
            }
            if (reciprocal == 0)
                throw new ArgumentException("Point " + point + " was already used in interpolation.");
            reciprocal = ring.reciprocal(reciprocal);
            mixedRadix.add(ring.multiply(reciprocal, ring.subtract(value, accumulator)));

            lins.multiply(lins.createLinear(ring.negate(points.get(points.size() - 1)), 1));
            poly.add(lins.clone().multiply(mixedRadix.get(mixedRadix.size() - 1)));

            points.add(point);
            values.add(value);
            return this;
        }

        
        public InterpolationZp64 update(long[] points, long[] values) {
            for (int i = 0; i < points.Length; i++)
                update(points[i], values[i]);
            return this;
        }

        
        public UnivariatePolynomialZp64 getInterpolatingPolynomial() {return poly;}

        
        public TLongArrayList getPoints() {return points;}

        
        public TLongArrayList getValues() {return values;}

        
        public int numberOfPoints() {return points.size();}
    }

    
    public sealed class Interpolation<E> {
        private readonly Ring<E> ring;
        
        private readonly List<E> points = new List<E>();
        
        private readonly List<E> values = new List<E>();
        
        private readonly List<E> mixedRadix = new List<E>();
        
        private readonly UnivariatePolynomial<E> lins;
        
        private readonly UnivariatePolynomial<E> poly;

        
        public Interpolation(Ring<E> ring) {
            this.ring = ring;
            this.lins = UnivariatePolynomial<E>.one(ring);
            this.poly = UnivariatePolynomial<E>.one(ring);
        }

        
        public Interpolation<E> update(E point, E value) {
            if (points.isEmpty()) {
                points.add(point);
                values.add(value);
                mixedRadix.add(value);
                poly.multiply(value);
                return this;
            }
            E reciprocal = ring.subtract(point, points.get(0));
            E accumulator = mixedRadix.get(0);
            for (int i = 1; i < points.size(); ++i) {
                accumulator = ring.add(accumulator, ring.multiply(mixedRadix.get(i), reciprocal));
                reciprocal = ring.multiply(reciprocal, ring.subtract(point, points.get(i)));
            }
            if (ring.isZero(reciprocal))
                throw new ArgumentException("Point " + point + " was already used in interpolation.");
            reciprocal = ring.reciprocal(reciprocal);
            mixedRadix.add(ring.multiply(reciprocal, ring.subtract(value, accumulator)));

            lins.multiply(lins.createLinear(ring.negate(points.get(points.size() - 1)), ring.getOne()));
            poly.add(lins.clone().multiply(mixedRadix.get(mixedRadix.size() - 1)));

            points.add(point);
            values.add(value);
            return this;
        }

        
        public Interpolation<E> update(E[] points, E[] values) {
            for (int i = 0; i < points.Length; i++)
                update(points[i], values[i]);
            return this;
        }

        
        public UnivariatePolynomial<E> getInterpolatingPolynomial() {return poly;}

        
        public List<E> getPoints() {return points;}

        
        public List<E> getValues() {return values;}

        
        public int numberOfPoints() {return points.size();}
    }
}