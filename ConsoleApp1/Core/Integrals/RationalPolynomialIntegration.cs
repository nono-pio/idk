using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Polynomials;
using ConsoleApp1.Core.Solvers;
using Polynomials;
using Polynomials.Poly;
using Polynomials.Poly.Multivar;
using Polynomials.Poly.Univar;

namespace ConsoleApp1.Core.Integrals;

public static class RationalPolynomialIntegration
{
    // TODO : transformer Roots K[] en Expr[] et trouver les racines dans R

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
    public static Expr IntegrateRationalPolynomial<K>(UnivariatePolynomial<K> A, UnivariatePolynomial<K> D,
        UniDiffField<K> Diff)
    {
        (var p, A) = UnivariateDivision.DivideAndRemainder(A, D)!.ToTuple2(); // A = p + A/D
        var (g, h) = HorowitzOstrogradsky(A, D, Diff); // A/D = d/dx g + h
        var residue = IntRationalLogPart(h.Numerator(), h.Denominator(), Diff); // log part of A/D

        return Diff.ToExpr(IntegratePoly(p))
               + Diff.ToExpr(g.Numerator()) / Diff.ToExpr(g.Denominator())
               + ResidueToExpr(residue, Diff);
    }

    private static Expr ResidueToExpr<K>(Risch.Residue<K> residue, UniDiffField<K> Diff)
    {
        var result = Zero;
        for (int i = 0; i < residue.s.Length; i++)
        {
            if (residue.s[i].Degree() == 0)
                continue;

            result += LogToReal(residue.s[i], residue.S[i], Diff);
        }

        return result;
    }

    /// Write A/D as d/dx g + h
    public static (Rational<UnivariatePolynomial<K>> g, Rational<UnivariatePolynomial<K>> h) HorowitzOstrogradsky<K>(
        UnivariatePolynomial<K> A, UnivariatePolynomial<K> D, UniDiffField<K> Diff)
    {
        var D_ = UnivariateGCD.PolynomialGCD(D, D.Derivative());
        var Ds = D / D_;
        var n = D_.Degree() - 1;
        var m = Ds.Degree() - 1;
        var B = Polynomial.UnknowPoly(n, "b");
        var C = Polynomial.UnknowPoly(m, "c");
        var H = Diff.ToPolynomial(A) - B.Derivee() * Diff.ToPolynomial(Ds) +
                B * Diff.ToPolynomial(Ds * D_.Derivative() / D_) -
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
    public static Risch.Residue<K> IntRationalLogPart<K>(UnivariatePolynomial<K> A, UnivariatePolynomial<K> D,
        UniDiffField<K> Diff)
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

        return new Risch.Residue<K>(Q, S, true, Diff, calculate_alphas: false);
    }

    public static Expr LogToATan<K>(UnivariatePolynomial<K> A, UnivariatePolynomial<K> B, UniDiffField<K> Diff)
    {
        if (UnivariateDivision.DivideOrNull(A, B) is not null)
            return 2 * ATan(Diff.ToExpr(A / B));

        if (A.Degree() < B.Degree())
            return LogToATan(-B, A, Diff);

        var (G, D, C) = UnivariateGCD.PolynomialExtendedGCD(B, -A).ToTuple3();
        return 2 * ATan(Diff.ToExpr((A * D + B * C) / G)) + LogToATan(D, C, Diff);
    }

    public static Expr LogToReal<K>(UnivariatePolynomial<K> R, UnivariatePolynomial<UnivariatePolynomial<K>> S,
        UniDiffField<K> Diff)
    {
        var (P, Q) = RealComplexPoly(R);
        var A = S.MapCoefficients(Rings.MultivariateRing(2, Diff.Ring),
            c => RealComplexPoly(c).Item1); // K[t][x] -> K[u, v][x]
        var B = S.MapCoefficients(Rings.MultivariateRing(2, Diff.Ring),
            c => RealComplexPoly(c).Item2); // K[t][x] -> K[u, v][x]
        var H = UnivariateResultants.Resultant(P.AsUnivariate(1), Q.AsUnivariate(1)).AsUnivariate(); // H = Res_v(P, Q)
        var roots = Roots(H).ToHashSet(Diff.Ring);
        var result = Zero;
        foreach (var a in roots)
        {
            var P_eval = P.Evaluate(0, a).AsUnivariate();
            var roots2 = RemoveInvalidRoot(Roots(P_eval), Diff.Ring);
            foreach (var b in roots2)
            {
                var A_eval = A.MapCoefficients(Diff.Ring, a_i => a_i.Evaluate(a, b));
                var B_eval = B.MapCoefficients(Diff.Ring, b_i => b_i.Evaluate(a, b));
                result += Diff.ToExpr(a) * Ln(Pow(Diff.ToExpr(A_eval), 2) + Pow(Diff.ToExpr(B_eval), 2));
                result += Diff.ToExpr(b) * LogToATan(A_eval, B_eval, Diff);
            }
        }

        var logRoots = Roots(R);
        foreach (K a in logRoots)
        {
            result += Diff.ToExpr(a) * Ln(Diff.ToExpr(S.MapCoefficients(Diff.Ring, c => c.Evaluate(a))));
        }

        return result;
    }

    /// return a list with only the positive roots
    private static HashSet<K> RemoveInvalidRoot<K>(K[] roots, Ring<K> ring)
    {
        var newRoots = new HashSet<K>(ring);

        foreach (var root in roots)
        {
            if (ring.Signum(root) > 0)
                newRoots.Add(root);

            if (ring.IsZero(root) && !newRoots.Contains(root))
                newRoots.Add(root);
        }

        return newRoots;
    }

    /// Write p(u+iv) = P(u,v) + iQ(u,v)
    private static (MultivariatePolynomial<K>, MultivariatePolynomial<K>) RealComplexPoly<K>(UnivariatePolynomial<K> p)
    {
        var P = MultivariatePolynomial<K>.Zero(2, p.ring, MonomialOrder.DEFAULT);
        var Q = MultivariatePolynomial<K>.Zero(2, p.ring, MonomialOrder.DEFAULT);
        for (int i = 0; i < p.data.Length; i++)
        {
            var c = p.data[i];
            for (int k = 0; k <= i; k++)
            {
                var m = NumberUtils.Binomial(i, k);
                var sign = k / 2 % 2 == 0 ? 1 : -1;
                if (k % 2 == 0)
                {
                    P.Add(new Monomial<K>([i - k, k], p.ring.MultiplyLong(c, sign * m)));
                }
                else
                {
                    Q.Add(new Monomial<K>([i - k, k], p.ring.MultiplyLong(c, sign * m)));
                }
            }
        }

        return (P, Q);
    }

    /// Return the roots of Poly in K (may don't return all the roots)
    public static K[] Roots<K>(UnivariatePolynomial<K> poly, bool factor = true)
    {
        var ring = poly.ring;
        switch (poly.Degree())
        {
            case 0:
                return [];
            case 1:
                return [ring.DivideExact(ring.Negate(poly.Cc()), poly.Lc())];
            case 2: // TODO
                break;
        }

        if (!factor)
            return [];

        var sols = new List<K>();
        var fac = UnivariateFactorization.Factor(poly);
        for (int i = 0; i < fac.Count; i++)
        {
            var fSols = Roots(fac.Factors[i], false);
            for (int j = 0; j < fac.Exponents[i]; j++)
                sols.AddRange(fSols);
        }

        return sols.ToArray();
    }
}