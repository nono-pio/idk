using System.Numerics;
using UnivariatePolynomialZp64 = Polynomials.Poly.Univar.UnivariatePolynomial<long>;

namespace Polynomials.Poly.Univar;

public static class Conversions64bit
{
    static bool SWITCH_TO_64bit = true;

    public static bool CanConvertToZp64<E>(UnivariatePolynomial<E> poly)
    {
        return SWITCH_TO_64bit && Util.CanConvertToZp64(poly);
    }

    public static UnivariatePolynomialZp64 AsOverZp64<E>(UnivariatePolynomial<E> poly)
    {
        if (poly is not UnivariatePolynomial<BigInteger> bigPoly)
            throw new ArgumentException();

        return UnivariatePolynomial<BigInteger>.AsOverZp64(bigPoly);
    }

    public static UnivariatePolynomial<BigInteger> Convert(UnivariatePolynomialZp64 p)
    {
        return p.ToBigPoly();
    }
    
    public static UnivariatePolynomial<T> Convert<T>(UnivariatePolynomialZp64 p)
    {
        return p.ToBigPoly().AsT<T>();
    }

    public static UnivariatePolynomial<BigInteger>[] Convert(UnivariatePolynomial<BigInteger> factory,
        UnivariatePolynomialZp64[] p)
    {
        var r = new UnivariatePolynomial<BigInteger>[p.Length];
        for (int i = 0; i < p.Length; i++)
            r[i] = Convert(p[i]);
        return r;
    }
    
    public static UnivariatePolynomial<T>[] Convert<T>(UnivariatePolynomial<BigInteger> factory,
        UnivariatePolynomialZp64[] p)
    {
        var r = new UnivariatePolynomial<T>[p.Length];
        for (int i = 0; i < p.Length; i++)
            r[i] = Convert<T>(p[i]);
        return r;
    }
}
