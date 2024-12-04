using System.Numerics;

namespace Rings.poly.univar;

static class Conversions64bit {

    /**
     * whether to switch to 64 bit integer arithmetic when possible (false in tests)
     */
    static bool SWITCH_TO_64bit = true;

    static bool canConvertToZp64(IUnivariatePolynomial poly) {
        return SWITCH_TO_64bit && Util.canConvertToZp64(poly);
    }

    static  UnivariatePolynomialZp64 asOverZp64<T>(T poly) where T : IUnivariatePolynomial<T> {
        return UnivariatePolynomial<BigInteger>.asOverZp64(poly as UnivariatePolynomial<BigInteger>);
    }

    static T convert<T>(UnivariatePolynomialZp64 p) where T : IUnivariatePolynomial<T> {
        return (T) p.toBigPoly();
    }

    static  T[] conver<T>(T factory, UnivariatePolynomialZp64[] p) where T : IUnivariatePolynomial<T>{
        T[] r = new T[p.Length];
        for (int i = 0; i < p.Length; i++)
            r[i] = convert<T>(p[i]);
        return r;
    }
}
