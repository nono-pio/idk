

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
        static bool CanConvertToZp64<E>(IUnivariatePolynomial<E> poly) where E : IUnivariatePolynomial<E>
        {
            return SWITCH_TO_64bit && Util.CanConvertToZp64(poly);
        }

        static UnivariatePolynomialZp64 AsOverZp64<T>(T poly) where T : IUnivariatePolynomial<T>
        {
            return UnivariatePolynomial<T>.AsOverZp64((UnivariatePolynomial<BigInteger>) poly);
        }

        public static T Convert<T>(UnivariatePolynomialZp64 p) where T : IUnivariatePolynomial<T>
        {
            return (T)p.ToBigPoly();
        }

        public static T[] Convert<T>(T factory, UnivariatePolynomialZp64[] p) where T : IUnivariatePolynomial<T>
        {
            T[] r = factory.CreateArray(p.Length);
            for (int i = 0; i < p.Length; i++)
                r[i] = Convert<T>(p[i]);
            return r;
        }
    }
}