using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Poly;
using Java.Util;
using Java.Util.Stream;
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
        static bool CanConvertToZp64(AMultivariatePolynomial poly)
        {
            return SWITCH_TO_64bit && Util.CanConvertToZp64(poly);
        }

        static MultivariatePolynomialZp64 AsOverZp64<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(Poly poly)
        {
            return MultivariatePolynomial.AsOverZp64((MultivariatePolynomial<BigInteger>)poly);
        }

        static IList<MultivariatePolynomialZp64> AsOverZp64<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> list)
        {
            return list.Stream().Map(Conversions64bit.AsOverZp64()).Collect(Collectors.ToList());
        }

        static Poly ConvertFromZp64<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(MultivariatePolynomialZp64 p)
        {
            return (Poly)p.ToBigPoly();
        }

        static IList<Poly> ConvertFromZp64<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(IList<MultivariatePolynomialZp64> list)
        {
            return (IList<Poly>)list.Stream().Map(MultivariatePolynomialZp64.ToBigPoly()).Collect(Collectors.ToList());
        }

        static Poly[] ConvertFromZp64<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(Poly factory, MultivariatePolynomialZp64[] p)
        {
            Poly[] r = factory.CreateArray(p.Length);
            for (int i = 0; i < p.Length; i++)
                r[i] = ConvertFromZp64(p[i]);
            return r;
        }
    }
}