using System.Numerics;
using Polynomials.Poly.Univar;
using MultivariatePolynomialZp64 = Polynomials.Poly.Multivar.MultivariatePolynomial<long>;

namespace Polynomials.Poly.Multivar;

public static class Conversions64bit
{
    public static bool SWITCH_TO_64bit = true;

    public static bool CanConvertToZp64<E>(MultivariatePolynomial<E> poly)
    {
        return SWITCH_TO_64bit && Util.CanConvertToZp64(poly);
    }

    public static MultivariatePolynomialZp64 AsOverZp64<E>(MultivariatePolynomial<E> poly)
    {
        return MultivariatePolynomialZp64.AsOverZp64(poly as MultivariatePolynomial<BigInteger>);
    }

    public static List<MultivariatePolynomialZp64> AsOverZp64<E>(List<MultivariatePolynomial<E>> list)
    {
        return list.Select(AsOverZp64).ToList();
    }

    public static MultivariatePolynomial<E> ConvertFromZp64<E>(MultivariatePolynomialZp64 p)
    {
        return p.ToBigPoly().AsT<E>();
    }

    public static List<MultivariatePolynomial<E>> ConvertFromZp64<E>(List<MultivariatePolynomialZp64> list)
    {
        return list.Select(m => m.ToBigPoly().AsT<E>()).ToList();
    }

    public static MultivariatePolynomial<E>[] ConvertFromZp64<E>(MultivariatePolynomial<E> factory, MultivariatePolynomialZp64[] p)
    {
        var r = new MultivariatePolynomial<E>[p.Length];
        for (int i = 0; i < p.Length; i++)
            r[i] = ConvertFromZp64<E>(p[i]);
        return r;
    }
}
