using Polynomials.Poly.Univar;
using MultivariatePolynomialZp64 = Polynomials.Poly.Multivar.MultivariatePolynomial<long>;

namespace Polynomials.Poly.Multivar;

public static class MultivariateInterpolation
{
    private static void CheckInput<T>(T[] points, MultivariatePolynomial<T>[] values)
    {
        if (points.Length != values.Length)
            throw new ArgumentException();
    }


    public static MultivariatePolynomial<E> InterpolateNewton<E>(int variable, E[] points,
        MultivariatePolynomial<E>[] values)
    {
        CheckInput(points, values);
        return new Interpolation<E>(variable, values[0]).Update(points, values)
            .GetInterpolatingPolynomial(); //        int length = points.length;
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


    public sealed class Interpolation<E>
    {
        private readonly int variable;


        private readonly List<E> points = new List<E>();


        private readonly List<MultivariatePolynomial<E>> values = [];


        private readonly List<MultivariatePolynomial<E>> mixedRadix = [];


        private readonly MultivariatePolynomial<E> lins;


        private readonly MultivariatePolynomial<E> poly;


        private readonly Ring<E> ring;


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


        public Interpolation(int variable, MultivariatePolynomial<E> factory)
        {
            this.variable = variable;
            this.lins = factory.CreateOne();
            this.poly = factory.CreateOne();
            this.ring = poly.ring;
        }


        public Interpolation(int variable, PolynomialRing<MultivariatePolynomial<E>> factory) : this(variable,
            factory.Factory())
        {
        }


        public Interpolation<E> Update(E point, MultivariatePolynomial<E> value)
        {
            if (points.Count == 0)
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


        public Interpolation<E> Update(E[] points, MultivariatePolynomial<E>[] values)
        {
            for (int i = 0; i < points.Length; i++)
                Update(points[i], values[i]);
            return this;
        }


        public int GetVariable()
        {
            return variable;
        }


        public MultivariatePolynomial<E> GetInterpolatingPolynomial()
        {
            return poly;
        }


        public List<E> GetPoints()
        {
            return points;
        }


        public List<MultivariatePolynomial<E>> GetValues()
        {
            return values;
        }


        public int NumberOfPoints()
        {
            return points.Count;
        }
    }
}