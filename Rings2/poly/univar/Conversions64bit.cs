

using System.Numerics;

namespace Cc.Redberry.Rings.Poly.Univar
{
    /// <summary>
    /// </summary>
    /// <remarks>@since1.0</remarks>
    sealed class Conversions64bit
    {
        private Conversions64bit()
        {
        }

        /// <summary>
        /// whether to switch to 64 bit integer arithmetic when possible (false in tests)
        /// </summary>
        static bool SWITCH_TO_64bit = true;

        public static bool CanConvertToZp64<E>(IUnivariatePolynomial<E> poly) where E : IUnivariatePolynomial<E>
        {
            return SWITCH_TO_64bit && Util.CanConvertToZp64(poly);
        }

        public static UnivariatePolynomialZp64 AsOverZp64<E>(IUnivariatePolynomial<E> poly)where E : IUnivariatePolynomial<E>
        {
            if (poly is not UnivariatePolynomial<BigInteger> bigPoly)
                throw new ArgumentException();
            
            return UnivariatePolynomial<BigInteger>.AsOverZp64(bigPoly);
        }

        public static UnivariatePolynomial<BigInteger> Convert(UnivariatePolynomialZp64 p)
        {
            return p.ToBigPoly();
        }

        public static UnivariatePolynomial<BigInteger>[] Convert(UnivariatePolynomial<BigInteger> factory, UnivariatePolynomialZp64[] p)
        {
            var r = new UnivariatePolynomial<BigInteger>[p.Length];
            for (int i = 0; i < p.Length; i++)
                r[i] = Convert(p[i]);
            return r;
        }
    }
}