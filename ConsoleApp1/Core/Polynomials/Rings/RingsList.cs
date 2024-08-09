using ConsoleApp1.Core.Polynomials.UnivariatePolynomials;

namespace ConsoleApp1.Core.Polynomials.Rings;

public class RingsList
{
    public static Ring<int> ZZ = new Integers();
    public static Ring<int> Zp(int mod) => new IntegersMod(mod);
    public static Ring<UnivariatePolynomial<T>> UnivariateRing<T>(Ring<T> ring) => new UnivariateRing<UnivariatePolynomial<T>>(new(ring, []));

}