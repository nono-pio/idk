

using System.Numerics;

namespace Cc.Redberry.Rings.Poly.Multivar
{
    /// <summary>
    /// </summary>
    /// <remarks>@since1.0</remarks>
    class Conversions64bit
    {
        private Conversions64bit()
        {
        }

        /// <summary>
        /// whether to switch to 64 bit integer arithmetic when possible (false in tests)
        /// </summary>
        static bool SWITCH_TO_64bit = true;
        static bool CanConvertToZp64<Term, Poly>(AMultivariatePolynomial<Term, Poly> poly) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            return SWITCH_TO_64bit && Util.CanConvertToZp64(poly);
        }

        static MultivariatePolynomialZp64 AsOverZp64<Term, Poly>(Poly poly) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            return MultivariatePolynomialZp64.AsOverZp64((MultivariatePolynomial<BigInteger>)poly);
        }

        static List<MultivariatePolynomialZp64> AsOverZp64<Term, Poly>(List<Poly> list)  where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            return list.Select(AsOverZp64).ToList();
        }

        static Poly ConvertFromZp64<Term, Poly>(MultivariatePolynomialZp64 p) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            return (Poly)p.ToBigPoly();
        }

        static List<Poly> ConvertFromZp64<Term, Poly>(List<MultivariatePolynomialZp64> list) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            return (List<Poly>)list.Select(m => m.ToBigPoly()).ToList();
        }

        static Poly[] ConvertFromZp64<Term, Poly>(Poly factory, MultivariatePolynomialZp64[] p) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            Poly[] r = new Poly[p.Length];
            for (int i = 0; i < p.Length; i++)
                r[i] = ConvertFromZp64(p[i]);
            return r;
        }
    }
}