using Cc.Redberry.Rings.Poly;
using Cc.Redberry.Rings.Poly.Univar;


namespace Cc.Redberry.Rings.Poly
{
    /// <summary>
    /// Galois field {@code GF(p, q)}. Galois field is represented as a quotient ring {@code F[x]/<m(x)>} with given
    /// irreducible polynomial {@code m(x)} (minimal polynomial); cardinality of then field is than {@code p^q} where {@code
    /// p} is the cardinality of {@code F} and {@code q} is the degree of minimal polynomial. Since Galois field is in fact a
    /// simple field extension it inherits all corresponding methods.
    /// </summary>
    /// <param name="<E>">type of polynomials that represent elements of this Galois field</param>
    /// <remarks>
    /// @seeAlgebraicNumberField
    /// @seecc.redberry.rings.Rings#GF(IUnivariatePolynomial)
    /// @seecc.redberry.rings.Rings#GF(long, int)
    /// @since1.0
    /// </remarks>
    public sealed class FiniteField<E> : SimpleFieldExtension<E>
    {
        private static readonly long serialVersionUID = 1;
        /// <summary>
        /// GF(3^3)
        /// </summary>
        public static readonly FiniteField<UnivariatePolynomialZp64> GF27 = new FiniteField(UnivariatePolynomialZ64.Create(-1, -1, 0, 1).Modulus(3));
        /// <summary>
        /// GF(3^3)
        /// </summary>
        /// <summary>
        /// GF(17^5)
        /// </summary>
        public static readonly FiniteField<UnivariatePolynomialZp64> GF17p5 = new FiniteField(UnivariatePolynomialZ64.Create(11, 11, 0, 3, 9, 9).Modulus(17).Monic());
        /// <summary>
        /// GF(3^3)
        /// </summary>
        /// <summary>
        /// GF(17^5)
        /// </summary>
        /// <summary>
        /// Constructs finite field from the specified irreducible polynomial.
        /// 
        /// <p><b>NOTE:</b> irreducibility test for the minimal polynomial is not performed here, use {@link
        /// IrreduciblePolynomials#irreducibleQ(IUnivariatePolynomial)} to test irreducibility.
        /// </summary>
        /// <param name="minimalPoly">the minimal polynomial</param>
        public FiniteField(E minimalPoly) : base(minimalPoly)
        {
            if (!minimalPoly.IsOverFiniteField())
                throw new ArgumentException("Irreducible poly must be over finite field.");
        }

        /// <summary>
        /// GF(3^3)
        /// </summary>
        /// <summary>
        /// GF(17^5)
        /// </summary>
        /// <summary>
        /// Constructs finite field from the specified irreducible polynomial.
        /// 
        /// <p><b>NOTE:</b> irreducibility test for the minimal polynomial is not performed here, use {@link
        /// IrreduciblePolynomials#irreducibleQ(IUnivariatePolynomial)} to test irreducibility.
        /// </summary>
        /// <param name="minimalPoly">the minimal polynomial</param>
        public override bool IsField()
        {
            return true;
        }

        /// <summary>
        /// GF(3^3)
        /// </summary>
        /// <summary>
        /// GF(17^5)
        /// </summary>
        /// <summary>
        /// Constructs finite field from the specified irreducible polynomial.
        /// 
        /// <p><b>NOTE:</b> irreducibility test for the minimal polynomial is not performed here, use {@link
        /// IrreduciblePolynomials#irreducibleQ(IUnivariatePolynomial)} to test irreducibility.
        /// </summary>
        /// <param name="minimalPoly">the minimal polynomial</param>
        public override bool IsUnit(E element)
        {
            return !element.IsZero();
        }

        /// <summary>
        /// GF(3^3)
        /// </summary>
        /// <summary>
        /// GF(17^5)
        /// </summary>
        /// <summary>
        /// Constructs finite field from the specified irreducible polynomial.
        /// 
        /// <p><b>NOTE:</b> irreducibility test for the minimal polynomial is not performed here, use {@link
        /// IrreduciblePolynomials#irreducibleQ(IUnivariatePolynomial)} to test irreducibility.
        /// </summary>
        /// <param name="minimalPoly">the minimal polynomial</param>
        public override E Gcd(E a, E b)
        {
            return a;
        }

        /// <summary>
        /// GF(3^3)
        /// </summary>
        /// <summary>
        /// GF(17^5)
        /// </summary>
        /// <summary>
        /// Constructs finite field from the specified irreducible polynomial.
        /// 
        /// <p><b>NOTE:</b> irreducibility test for the minimal polynomial is not performed here, use {@link
        /// IrreduciblePolynomials#irreducibleQ(IUnivariatePolynomial)} to test irreducibility.
        /// </summary>
        /// <param name="minimalPoly">the minimal polynomial</param>
        public override E[] DivideAndRemainder(E a, E b)
        {
            return a.CreateArray(Multiply(a, Reciprocal(b)), GetZero());
        }

        /// <summary>
        /// GF(3^3)
        /// </summary>
        /// <summary>
        /// GF(17^5)
        /// </summary>
        /// <summary>
        /// Constructs finite field from the specified irreducible polynomial.
        /// 
        /// <p><b>NOTE:</b> irreducibility test for the minimal polynomial is not performed here, use {@link
        /// IrreduciblePolynomials#irreducibleQ(IUnivariatePolynomial)} to test irreducibility.
        /// </summary>
        /// <param name="minimalPoly">the minimal polynomial</param>
        public override E Remainder(E dividend, E divider)
        {
            return GetZero();
        }

        /// <summary>
        /// GF(3^3)
        /// </summary>
        /// <summary>
        /// GF(17^5)
        /// </summary>
        /// <summary>
        /// Constructs finite field from the specified irreducible polynomial.
        /// 
        /// <p><b>NOTE:</b> irreducibility test for the minimal polynomial is not performed here, use {@link
        /// IrreduciblePolynomials#irreducibleQ(IUnivariatePolynomial)} to test irreducibility.
        /// </summary>
        /// <param name="minimalPoly">the minimal polynomial</param>
        /// <summary>
        /// Returns iterator over all field elements
        /// </summary>
        /// <returns>iterator over all field elements</returns>
        public override IEnumerator<E> Iterator()
        {
            if (!IsFinite())
                throw new Exception("Ring of infinite cardinality.");
            if (minimalPoly is UnivariatePolynomial)
                return (IEnumerator<E>)new It(((UnivariatePolynomial)minimalPoly).ring, minimalPoly.Degree());
            else if (minimalPoly is UnivariatePolynomialZp64)
                return (IEnumerator<E>)new lIt(((UnivariatePolynomialZp64)minimalPoly).ring, minimalPoly.Degree());
            throw new Exception();
        }

        /// <summary>
        /// GF(3^3)
        /// </summary>
        /// <summary>
        /// GF(17^5)
        /// </summary>
        /// <summary>
        /// Constructs finite field from the specified irreducible polynomial.
        /// 
        /// <p><b>NOTE:</b> irreducibility test for the minimal polynomial is not performed here, use {@link
        /// IrreduciblePolynomials#irreducibleQ(IUnivariatePolynomial)} to test irreducibility.
        /// </summary>
        /// <param name="minimalPoly">the minimal polynomial</param>
        /// <summary>
        /// Returns iterator over all field elements
        /// </summary>
        /// <returns>iterator over all field elements</returns>
        private sealed class It<E> : IEnumerator<UnivariatePolynomial<E>>
        {
            readonly Ring<E> ring;
            readonly E[] data;
            readonly IEnumerator<E>[] iterators;
            It(Ring<E> ring, int degree)
            {
                this.ring = ring;
                this.data = ring.CreateArray(degree);
                this.iterators = new IEnumerator[degree];
                for (int i = 0; i < iterators.Length; i++)
                    iterators[i] = ring.Iterator();
                for (int i = 0; i < data.Length; i++)
                    data[i] = iterators[i].Next();
            }

            public bool HasNext()
            {
                return Arrays.Stream(iterators).AnyMatch(IEnumerator.HasNext());
            }

            private bool first = true;
            public UnivariatePolynomial<E> Next()
            {
                if (first)
                {
                    first = false;
                    return UnivariatePolynomial.Create(ring, data.Clone());
                }

                int i = 0;
                if (!iterators[i].HasNext())
                    while (i < iterators.Length && !iterators[i].HasNext())
                    {
                        iterators[i] = ring.Iterator();
                        data[i] = iterators[i].Next();
                        ++i;
                    }

                if (i >= iterators.Length)
                    return null;
                data[i] = iterators[i].Next();
                return UnivariatePolynomial.CreateUnsafe(ring, data.Clone());
            }
        }

        /// <summary>
        /// GF(3^3)
        /// </summary>
        /// <summary>
        /// GF(17^5)
        /// </summary>
        /// <summary>
        /// Constructs finite field from the specified irreducible polynomial.
        /// 
        /// <p><b>NOTE:</b> irreducibility test for the minimal polynomial is not performed here, use {@link
        /// IrreduciblePolynomials#irreducibleQ(IUnivariatePolynomial)} to test irreducibility.
        /// </summary>
        /// <param name="minimalPoly">the minimal polynomial</param>
        /// <summary>
        /// Returns iterator over all field elements
        /// </summary>
        /// <returns>iterator over all field elements</returns>
        private sealed class lIt : IEnumerator<UnivariatePolynomialZp64>
        {
            readonly IntegersZp64 ring;
            readonly long[] data;
            lIt(IntegersZp64 ring, int degree)
            {
                this.ring = ring;
                this.data = new long[degree];
            }

            public bool HasNext()
            {
                return Arrays.Stream(data).AnyMatch((l) => l < (ring.modulus - 1));
            }

            private bool first = true;
            public UnivariatePolynomialZp64 Next()
            {
                if (first)
                {
                    first = false;
                    return UnivariatePolynomialZp64.CreateUnsafe(ring, data.Clone());
                }

                int i = 0;
                if (data[i] >= ring.modulus - 1)
                    while (i < data.Length && data[i] >= ring.modulus - 1)
                    {
                        data[i] = 0;
                        ++i;
                    }

                if (i >= data.Length)
                    return null;
                ++data[i];
                return UnivariatePolynomialZp64.CreateUnsafe(ring, data.Clone());
            }
        }
    }
}