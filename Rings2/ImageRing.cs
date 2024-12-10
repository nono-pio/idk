using System.Numerics;

namespace Cc.Redberry.Rings
{
    /// <summary>
    /// A ring obtained via isomorphism specified by {@link #image(Object)} and {@link #inverse(Object)} Funcs.
    /// </summary>
    public class ImageRing<F, I> : Ring<I>
    {
        /// <summary>
        /// the ring
        /// </summary>
        public readonly Ring<F> ring;

        public readonly Func<F, I> imageFunc;
        public readonly Func<I, F> inverseFunc;

        public ImageRing(Ring<F> ring, Func<I, F> inverseFunc, Func<F, I> imageFunc)
        {
            this.ring = ring;
            this.inverseFunc = inverseFunc;
            this.imageFunc = imageFunc;
        }

        public I Image(F el)
        {
            return imageFunc(el);
        }

        public I[] Image(F[] el)
        {
            I[] array = new I[el.Length];
            for (int i = 0; i < array.Length; i++)
                array[i] = Image(el[i]);
            return array;
        }

        public F Inverse(I el)
        {
            return inverseFunc(el);
        }

        public F[] Inverse(I[] el)
        {
            F[] array = new F[el.Length];
            for (int i = 0; i < array.Length; i++)
                array[i] = Inverse(el[i]);
            return array;
        }

        public virtual bool IsField()
        {
            return ring.IsField();
        }

        public virtual bool IsEuclideanRing()
        {
            return ring.IsEuclideanRing();
        }

        public virtual BigInteger? Cardinality()
        {
            return ring.Cardinality();
        }

        public virtual BigInteger Characteristic()
        {
            return ring.Characteristic();
        }

        public virtual bool IsPerfectPower()
        {
            return ring.IsPerfectPower();
        }

        public virtual BigInteger? PerfectPowerBase()
        {
            return ring.PerfectPowerBase();
        }

        public virtual BigInteger? PerfectPowerExponent()
        {
            return ring.PerfectPowerExponent();
        }

        public virtual I Add(I a, I b)
        {
            return Image(ring.Add(Inverse(a), Inverse(b)));
        }

        public virtual I Subtract(I a, I b)
        {
            return Image(ring.Subtract(Inverse(a), Inverse(b)));
        }

        public virtual I Multiply(I a, I b)
        {
            return Image(ring.Multiply(Inverse(a), Inverse(b)));
        }

        public virtual I Negate(I element)
        {
            return Image(ring.Negate(Inverse(element)));
        }

        public virtual I Increment(I element)
        {
            return Image(ring.Increment(Inverse(element)));
        }

        public virtual I Decrement(I element)
        {
            return Image(ring.Decrement(Inverse(element)));
        }

        public virtual I Add(params I[] elements)
        {
            return Image(ring.Add(Inverse(elements)));
        }

        public virtual I Multiply(params I[] elements)
        {
            return Image(ring.Multiply(Inverse(elements)));
        }

        public virtual I Abs(I el)
        {
            return Image(ring.Abs(Inverse(el)));
        }

        public virtual I Copy(I element)
        {
            return element;
        }

        public virtual I[]? DivideAndRemainder(I dividend, I divider)
        {
            var qr = ring.DivideAndRemainder(Inverse(dividend), Inverse(divider));
            return qr is null ? null : Image(qr);
        }

        public virtual I Quotient(I dividend, I divider)
        {
            return Image(ring.Quotient(Inverse(dividend), Inverse(divider)));
        }

        public virtual I Remainder(I dividend, I divider)
        {
            return Image(ring.Remainder(Inverse(dividend), Inverse(divider)));
        }

        public virtual I Reciprocal(I element)
        {
            return Image(ring.Reciprocal(Inverse(element)));
        }

        public virtual I GetZero()
        {
            return Image(ring.GetZero());
        }

        public virtual I GetOne()
        {
            return Image(ring.GetOne());
        }

        public virtual bool IsZero(I element)
        {
            return ring.IsZero(Inverse(element));
        }

        public virtual bool IsOne(I element)
        {
            return ring.IsOne(Inverse(element));
        }

        public virtual bool IsUnit(I element)
        {
            return ring.IsUnit(Inverse(element));
        }

        public virtual I ValueOf(long val)
        {
            return Image(ring.ValueOf(val));
        }

        public virtual I ValueOfBigInteger(BigInteger val)
        {
            return Image(ring.ValueOfBigInteger(val));
        }

        public virtual I ValueOf(I val)
        {
            return Image(ring.ValueOf(Inverse(val)));
        }

        public virtual IEnumerator<I> Iterator()
        {
            return StreamSupport
                .Stream(Spliterators.SpliteratorUnknownSize(ring.Iterator(), Spliterator.ORDERED), false)
                .Map(this.Image()).Iterator();
        }

        public virtual I Gcd(I a, I b)
        {
            return Image(ring.Gcd(Inverse(a), Inverse(b)));
        }

        public virtual I[] ExtendedGCD(I a, I b)
        {
            return Image(ring.ExtendedGCD(Inverse(a), Inverse(b)));
        }

        public virtual I Lcm(I a, I b)
        {
            return Image(ring.Lcm(Inverse(a), Inverse(b)));
        }

        public virtual I Gcd(params I[] elements)
        {
            return Image(ring.Gcd(Inverse(elements)));
        }

        public virtual I Gcd(IEnumerable<I> elements)
        {
            return Image(ring.Gcd(elements.Select(Inverse)));
        }

        public virtual int Signum(I element)
        {
            return ring.Signum(Inverse(element));
        }

        public virtual FactorDecomposition<I> FactorSquareFree(I element)
        {
            return ring.FactorSquareFree(Inverse(element)).MapTo(this, Image);
        }

        public virtual FactorDecomposition<I> Factor(I element)
        {
            return ring.Factor(Inverse(element)).MapTo(this, Image);
        }

        public virtual I Parse(string @string)
        {
            return Image(ring.Parse(@string));
        }

        public virtual I Pow(I @base, int exponent)
        {
            return Image(ring.Pow(Inverse(@base), exponent));
        }

        public virtual I Pow(I @base, long exponent)
        {
            return Image(ring.Pow(Inverse(@base), exponent));
        }

        public virtual I Pow(I @base, BigInteger exponent)
        {
            return Image(ring.Pow(Inverse(@base), exponent));
        }

        public virtual I Factorial(long num)
        {
            return Image(ring.Factorial(num));
        }

        public virtual I RandomElement(Random rnd)
        {
            return Image(ring.RandomElement(rnd));
        }

        public virtual int Compare(I? o1, I? o2)
        {
            return ring.Compare(Inverse(o1), Inverse(o2));
        }

        public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            ImageRing<F, I> that = (ImageRing<F, I>)o;
            if (!ring.Equals(that.ring))
                return false;
            if (!inverseFunc.Equals(that.inverseFunc))
                return false;
            return imageFunc.Equals(that.imageFunc);
        }

        public override int GetHashCode()
        {
            int result = ring.GetHashCode();
            result = 31 * result + inverseFunc.GetHashCode();
            result = 31 * result + imageFunc.GetHashCode();
            return result;
        }
    }
}