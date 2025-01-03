using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Polynomials;
using ConsoleApp1.Core.Solvers;
using Polynomials;
using Polynomials.Poly.Multivar;
using Polynomials.Poly.Univar;

namespace ConsoleApp1.Core.Integrals;

public static class RationalPolynomialIntegration
{

    /// Integrate a polynomial
    public static UnivariatePolynomial<K> IntegratePoly<K>(UnivariatePolynomial<K> p)
    {
        var newData = new K[p.data.Length + 1];
        newData[0] = p.ring.GetZero();
        for (int i = 0; i < p.data.Length; i++)
        {
            newData[i + 1] = p.ring.DivideExact(p.data[i], p.ring.ValueOfLong(i + 1));
        }
        
        return new UnivariatePolynomial<K>(p.ring, newData);
    }
    
    /// Integrate a rational polynomial
    public static Expr IntegrateRationalPolynomial<K>(UnivariatePolynomial<K> A, UnivariatePolynomial<K> D, UniDiffField<K> Diff)
    {
        (var p, A) = UnivariateDivision.DivideAndRemainder(A, D)!.ToTuple2(); // A = p + A/D
        var (g, h) = HorowitzOstrogradsky(A, D, Diff); // A/D = d/dx g + h
        var residue = IntRationalLogPart(h.Numerator(), h.Denominator(), Diff); // log part of A/D
        
        return Diff.ToExpr(IntegratePoly(p)) 
               + Diff.ToExpr(g.Numerator()) / Diff.ToExpr(g.Denominator()) 
               + residue.ToExpression();
    }

    /// Write A/D as d/dx g + h
    public static (Rational<UnivariatePolynomial<K>> g, Rational<UnivariatePolynomial<K>> h) HorowitzOstrogradsky<K>(UnivariatePolynomial<K> A, UnivariatePolynomial<K> D, UniDiffField<K> Diff)
    {
        var D_ = UnivariateGCD.PolynomialGCD(D, D.Derivative());
        var Ds = D / D_;
        var n = D_.Degree() - 1;
        var m = Ds.Degree() - 1;
        var B = Polynomial.UnknowPoly(n, "b");
        var C = Polynomial.UnknowPoly(m, "c");
        var H = Diff.ToPolynomial(A) - B.Derivee() * Diff.ToPolynomial(Ds) + B * Diff.ToPolynomial(Ds * D_.Derivative() / D_) -
                C * Diff.ToPolynomial(D_);
        var solutions = Solve.SolveEquations(H.Coefs.Select(c => (c, Zero)).ToArray(),
            B.Coefs.Concat(C.Coefs).Where(c => !c.IsZero).Cast<Variable>().ToArray());

        if (solutions is null)
            throw new Exception("No solution found");

        var bCoefs = solutions[..(n + 1)].Select(Diff.FromExpr).ToArray();
        var cCoefs = solutions[(n + 1)..].Select(Diff.FromExpr).ToArray();

        var newB = PolynomialFactory.Uni(Diff.Ring, bCoefs);
        var newC = PolynomialFactory.Uni(Diff.Ring, cCoefs);

        return (PolynomialFactory.RationalPoly(newB, D_), PolynomialFactory.RationalPoly(newC, Ds));
    }

    /// Write A/D as residue (Integral of A/D = residue.ToExpression())
    public static Risch.Residue<K> IntRationalLogPart<K>(UnivariatePolynomial<K> A, UnivariatePolynomial<K> D, UniDiffField<K> Diff)
    {
        var ringK = A.ring;
        var t = PolynomialFactory.Uni(Rings.UnivariateRing(ringK),
            PolynomialFactory.Uni(ringK, [ringK.GetZero(), ringK.GetOne()])); // t in K[t][x]
        var newA = A.MapCoefficients(Rings.UnivariateRing(ringK), cf => PolynomialFactory.Uni(ringK, cf));
        var newD = D.MapCoefficients(Rings.UnivariateRing(ringK), cf => PolynomialFactory.Uni(ringK, cf));
        var (R, Rs) = Risch.SubResultant(newD, newA - t * newD.Derivative());
        var (_, Q) = Risch.SquareFree(R);
        var S = new UnivariatePolynomial<UnivariatePolynomial<K>>[Q.Length];
        for (int i = 0; i < Q.Length; i++)
        {
            if (Q[i].Degree() == 0)
                continue;
            var ring = Rings.SimpleFieldExtension(Q[i]);
            if (Q[i].Degree() == i)
                S[i] = newD.MapCoefficients(ring, ring.ValueOf);
            else
            {
                S[i] = Rs.First(r_i => r_i.Degree() == i).MapCoefficients(ring, ring.ValueOf);
                var (_, As) = Risch.SquareFree(S[i].Lc());
                for (int j = 0; j < As.Length; j++)
                {
                    S[i] /= UnivariateGCD.PolynomialGCD(As[j], Q[i]).Pow(j);
                }
            }
        }
        
        return new Risch.Residue<K>(Q, S, true, Diff);
    }

    public static Expr LogToATan<K>(UnivariatePolynomial<K> A, UnivariatePolynomial<K> B, UniDiffField<K> Diff)
    {
        if (UnivariateDivision.DivideOrNull(A, B) is not null)
            return 2 * ATan(Diff.ToExpr(A / B));
        
        if (A.Degree() < B.Degree())
            return LogToATan(-B, A, Diff);

        var (D, C, G) = UnivariateGCD.PolynomialExtendedGCD(B, -A).ToTuple3();
        return 2 * ATan(Diff.ToExpr((A*D + B*C)/ G)) + LogToATan(D, C, Diff);
    }

    public static ()? LogToReal<K>(UnivariatePolynomial<K> R, UnivariatePolynomial<UnivariatePolynomial<K>> S,
        UniDiffField<K> Diff)
    {
        (MultivariatePolynomial<K> P, MultivariatePolynomial<K> Q) = RealComplexPoly(R);
        var (A, B) = RealComplexPoly(S);
        var H = UnivariateResultants.Resultant(P.AsUnivariate(1), Q.AsUnivariate(1)).AsUnivariate(); // H = Res_v(P, Q)
        var roots = Roots(H);
        var result = Zero;
        foreach (K a in roots)
        {
            var P_eval = P.Evaluate(0, a).AsUnivariate();
            var roots2 = Roots(P_eval);
            foreach (K b in roots2)
            {
                var A_eval = A.Evaluate(a, b, x);
                var B_eval = A.Evaluate(a, b, x);
                result += Diff.ToExpr(a) * Ln(Pow(Diff.ToExpr(A_eval), 2) + Pow(Diff.ToExpr(B_eval), 2));
                result += Diff.ToExpr(b) * LogToATan(A, B).Evaluate(a, b, x);
            }
        }

        var logRoots = Roots(R);
        foreach (K a in logRoots)
        {
            result += Diff.ToExpr(a) * Ln(S.Evaluate(a, x));
        }
        
        return result;
    }
}