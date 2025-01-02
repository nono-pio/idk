using Polynomials;
using Polynomials.Poly.Univar;

namespace ConsoleApp1.Core.Integrals;

public  static class RationalPolynomialIntegration
{
    public static (Rational<UnivariatePolynomial<K>> g, Rational<UnivariatePolynomial<K>> h) HorowitzOstrogradsky<K>(UnivariatePolynomial<K> A, UnivariatePolynomial<K> D)
    {
        // p.46
        var D_ = UnivariateGCD.PolynomialGCD(D, D.Derivative());
        var Ds = D / D_;
        var n = D_.Degree() - 1;
        var m = Ds.Degree() - 1;
    }

    public static (UnivariatePolynomial<K>[] Q, UnivariatePolynomial<UnivariatePolynomial<K>>[] S) IntRationalLogPart<K>(UnivariatePolynomial<K> A, UnivariatePolynomial<K> D)
    {
        var ringK = A.ring;
        var t = PolynomialFactory.Uni(Rings.UnivariateRing(ringK), PolynomialFactory.Uni(ringK, [ringK.GetZero(), ringK.GetOne()])); // t in K[t][x]
        var newA = A.MapCoefficients(Rings.UnivariateRing(ringK), cf => PolynomialFactory.Uni(ringK, cf));
        var newD = D.MapCoefficients(Rings.UnivariateRing(ringK), cf => PolynomialFactory.Uni(ringK, cf));
        var (R, Rs) = Risch.SubResultant(newD, newA - t * newD.Derivative());
        var (_, Q) = Risch.SquareFree(R);
        var S = new UnivariatePolynomial<UnivariatePolynomial<K>>[Q.Length];
        for (int i = 0; i < Q.Length; i++)
        {
            if (Q[i].Degree() == 0)
                continue;

            if (Q[i].Degree() == i)
                S[i] = newD;
            else
            {
                S[i] = Rs.First(r_i => r_i.Degree() == i);
                var (_, As) = Risch.SquareFree(S[i].Lc());
                for (int j = 0; j < As.Length; j++)
                {
                    S[i] /= UnivariateGCD.PolynomialGCD(As[j], Q[i]).Pow(j);
                }
            }
        }

        return (Q, S);
    }
}