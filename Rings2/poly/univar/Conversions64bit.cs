using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Poly;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Poly.Univar.RoundingMode;
using static Cc.Redberry.Rings.Poly.Univar.Associativity;
using static Cc.Redberry.Rings.Poly.Univar.Operator;
using static Cc.Redberry.Rings.Poly.Univar.TokenType;
using static Cc.Redberry.Rings.Poly.Univar.SystemInfo;

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
        static bool CanConvertToZp64(IUnivariatePolynomial poly)
        {
            return SWITCH_TO_64bit && Util.CanConvertToZp64(poly);
        }

        static UnivariatePolynomialZp64 AsOverZp64<T extends IUnivariatePolynomial<T>>(T poly)
        {
            return UnivariatePolynomial.AsOverZp64((UnivariatePolynomial<BigInteger>)poly);
        }

        static T Convert<T extends IUnivariatePolynomial<T>>(UnivariatePolynomialZp64 p)
        {
            return (T)p.ToBigPoly();
        }

        static T[] Convert<T extends IUnivariatePolynomial<T>>(T factory, UnivariatePolynomialZp64[] p)
        {
            T[] r = factory.CreateArray(p.Length);
            for (int i = 0; i < p.Length; i++)
                r[i] = Convert(p[i]);
            return r;
        }
    }
}