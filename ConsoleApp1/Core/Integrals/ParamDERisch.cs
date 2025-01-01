using System.Diagnostics;
using ConsoleApp1.Core.Sets;
using ConsoleApp1.Core.Solvers;
using Polynomials;
using Polynomials.Poly.Univar;

namespace ConsoleApp1.Core.Integrals;

public static class ParamDERisch
{
    public delegate UnivariatePolynomial<K> DiffPoly<K>(UnivariatePolynomial<K> f);
    public delegate Rational<UnivariatePolynomial<K>> Diff<K>(UnivariatePolynomial<K> f);

    public static (UnivariatePolynomial<K> a, Rational<UnivariatePolynomial<K>> b, Rational<UnivariatePolynomial<K>>[] G
        , UnivariatePolynomial<K> h) ParamRdeNormalDenominator<K>(Rational<UnivariatePolynomial<K>> f,
            Rational<UnivariatePolynomial<K>>[] g, DiffPoly<K> D)
    {
        var UK = f.ring;

        var (dn, ds) = Risch.SplitFactor(f.Denominator(), D);
        var (en, es) = Risch.SplitFactor(UK.Lcm(g.Select(g_i => g_i.Denominator())), D);
        var p = UnivariateGCD.PolynomialGCD(dn, en);
        var h = UnivariateGCD.PolynomialGCD(en, en.Derivative()) / UnivariateGCD.PolynomialGCD(p, p.Derivative());
        var dn_hsquare = dn * h * h;
        return (dn * h, dn * h * f - dn * D(h), g.Select(g_i => dn_hsquare * g_i).ToArray(), h);
    }
    
    public static (UnivariatePolynomial<K> a, UnivariatePolynomial<K> b, Rational<UnivariatePolynomial<K>>[] G
        , UnivariatePolynomial<K> h) ParamRdeSpecialDenomExp<K>(UnivariatePolynomial<K> a, Rational<UnivariatePolynomial<K>> b,
            Rational<UnivariatePolynomial<K>>[] g, DiffPoly<K> D)
    {
        var p ;
        var nb ;
        var nc ;
        var n = Math.Min(0, nc - Math.Min(0, nb));
        if (nb == 0)
        {
            var alpha = UnivariateDivision.Remainder(-b / a, p);
            if (alpha = m D(t)/t - Dz/z)
            {
                n = Math.Min(s, n);
            }
        }

        var N = Math.Max(0, -nb);
        return (a * p.Pow(N), (b + n * a * D(p) / p) * p.Pow(N), g.Select(g_i => g_i * p.Pow(N - n)).ToArray(), p.Pow(-n));
    }


    public static (UnivariatePolynomial<K> a, UnivariatePolynomial<K> b, UnivariatePolynomial<K>[] G,
        UnivariatePolynomial<K> h) ParamRdeSpecielDenomTan<K>(UnivariatePolynomial<K> a, Rational<UnivariatePolynomial<K>> b,
            Rational<UnivariatePolynomial<K>>[] g, DiffPoly<K> D)
    {
        var p;
        var nb;
        var nc;
        var n = Math.Min(0, nc - Math.Min(0, nb));
        if (nb == 0)
        {
            var alpha;
            var beta;
            var eta = Dt / (t ^ 2 + 1);
            throw new NotImplementedException();
        }

        var N = Math.Max(0, -nb);
        return (a * p.Pow(N), (b + n * a * D(p) / p) * p.Pow(N), g.Select(g_i => g_i * p.Pow(N - n)).ToArray(), p.Pow(-n));
    }

    public static (UnivariatePolynomial<K>[] q, K[,] M) LinearConstraints<K>(UnivariatePolynomial<K> a, UnivariatePolynomial<K> b,
        Rational<UnivariatePolynomial<K>>[] g, Diff<K> D)
    {
        var UK = g[0].ring;
        var d = UK.Lcm(g.Select(g_i => g_i.Denominator()));
        UnivariatePolynomial<K>[] q = new UnivariatePolynomial<K>[g.Length];
        UnivariatePolynomial<K>[] r = new UnivariatePolynomial<K>[g.Length];
        for (int i = 0; i < g.Length; i++)
        {
            (q[i], r[i]) = UnivariateDivision.DivideAndRemainder(g[i].Numerator() * d.DivideExact(g[i].Denominator()), d)!.ToTuple2();
        }
        
        var n = (r.All(r_i => r_i.IsZero())) ? -1 : r.Max(r_i => r_i.Degree());
        var M = new K[n, r.Length];
        for (int i = 0; i <= n; i++)
        {
            for (int j = 0; j < r.Length; j++)
            {
                M[i, j] = r[j][i];
            }
        }

        return (q, M);
    }

    public static (K[,] B, K[] v) ConstantSystem<K>(K[,] MatrixA, K[] VectorU, DiffPoly<K> DPoly, Ring<K> ring)
    {
        K D(K a)
        {
            var poly = PolynomialFactory.Uni(ring, a);
            var dpoly = DPoly(poly);
            return dpoly.Degree() == 0 ? dpoly.Cc() : throw new Exception();
        }
        bool IsConstant(K a) => ring.IsZero(D(a));
        bool IsConstantMatrix(List<K[]> a) => a.All(row => row.All(IsConstant));
        bool IsConstantColumn(List<K[]> a, int col) => a.All(row => IsConstant(row[col]));
        
        (MatrixA, VectorU) = RowEchelon(MatrixA, VectorU);
        List<K[]> A = MatrixA.ToListOfArray();
        List<K> u = VectorU.ToList();
        
        var m = A.Count;
        while (!IsConstantMatrix(A))
        {
            var j = Enumerable.Range(0, A[0].Length).First(col => !IsConstantColumn(A, col));
            var i = Enumerable.Range(0, m).First(row => !IsConstant(A[row][j]));
            var R_i = A[i];
            var a_ij = A[i][j];
            var newRow = R_i.Select(el => ring.DivideExact(D(el), D(a_ij))).ToArray();
            var newU = ring.DivideExact(D(u[i]), D(a_ij));
            for (int s = 0; s < m; s++)
            {
                for (int k = 0; k < newRow.Length; k++)
                {
                    A[s][k] = ring.Subtract(A[s][k], ring.Multiply(a_ij, newRow[k]));
                }
                u[s] = ring.Subtract(u[s], ring.Multiply(a_ij, newU));
            }
            
            A.Add(newRow);
            u.Add(newU);
        }

        return (A.ToArray().ToArray2D(), u.ToArray());
    }

    public static int ParamRdeBoundDegreePrim<K>(UnivariatePolynomial<K> a, UnivariatePolynomial<K> b,
        UnivariatePolynomial<K>[] q, DiffPoly<K> D)
    {
        var da = a.Degree();
        var db = b.Degree();
        var dc = q.Max(q_i => q_i.Degree());
        var n = db > da ? Math.Max(0, dc - db) : Math.Max(0, dc - da + 1);
        var ring = a.ring;
        if (db == da - 1)
        {
            var alpha = ring.Negate(ring.DivideExact(b.Lc(), a.Lc()));
            if ()
                n = Math.Max(n, s);
        }
    
        if (db == da)
        {
            var alpha = ring.Negate(ring.DivideExact(b.Lc(), a.Lc()));
            if ()
            {
                var beta;
                if ()
                    n = Math.Max(n, m);
            }
        }
    
        return n;
    }
    
    public static int ParamRdeBoundDegreeBase<K>(UnivariatePolynomial<K> a, UnivariatePolynomial<K> b,
        UnivariatePolynomial<K>[] q)
    {
        var da = a.Degree();
        var db = b.Degree();
        var dc = q.Max(q_i => q_i.Degree());
        var n = Math.Max(0, dc - Math.Max(db, da - 1));
        if (db == da - 1)
        {
            var ring = a.ring;
            var s = ring.Negate(ring.DivideExact(b.Lc(), a.Lc()));
            if ()
                n = Math.Max(0, s, dc - db);
        }
    
        return n;
    }
    
    public static int ParamRdeBoundDegreeExp<K>(UnivariatePolynomial<K> a, UnivariatePolynomial<K> b,
        UnivariatePolynomial<K>[] q, DiffPoly<K> D)
    {
        var da = a.Degree();
        var db = b.Degree();
        var dc = q.Max(q_i => q_i.Degree());
        var n = Math.Max(0, dc - Math.Max(db, da));
        if (da == db)
        {
            var ring = a.ring;
            var alpha = ring.Negate(ring.DivideExact(b.Lc(), a.Lc()));
            if ()
                n = Math.Max(n, m);
        }
    
        return n;
    }
    
    public static int ParamRdeBoundDegreeNonLinear<K>(UnivariatePolynomial<K> a, UnivariatePolynomial<K> b,
        UnivariatePolynomial<K>[] q, DiffPoly<K> D)
    {
        var da = a.Degree();
        var db = b.Degree();
        var dc = q.Max(q_i => q_i.Degree());
        var delta = D(t).Degree();
        var lambda = D(t).Lc();
        var n = Math.Max(0, dc - Math.Max(da + delta - 1, db));
        if (db == da + delta - 1)
        {
            var ring = a.ring;
            var m = ring.Negate(ring.DivideExact(b.Lc(), ring.Multiply(lambda, a.Lc())));
            if ()
                n = Math.Max(0, m, dc - db);
        }
    
        return n;
    }
    
    public static (UnivariatePolynomial<K> a, UnivariatePolynomial<K> b, UnivariatePolynomial<K>[] q, UnivariatePolynomial<K>[] r, int n)
        ParSPDE<K>(UnivariatePolynomial<K> a, UnivariatePolynomial<K> b, UnivariatePolynomial<K>[] q,
        DiffPoly<K> D, int n)
    {
        var q_ = new UnivariatePolynomial<K>[q.Length];
        var r = new UnivariatePolynomial<K>[q.Length];
        for (int i = 0; i < q.Length; i++)
        {
            (r[i], var z) = Risch.ExtendedEuclidieanDiophantine(b, a, q[i]);
            q_[i] = z - D(r[i]);
        }

        return (a, b + D(a), q_, r, n - a.Degree());
    }
    
    public static (UnivariatePolynomial<K>[], K[,]) ParamPolyRischDENoCancel1<K>(UnivariatePolynomial<K> b, UnivariatePolynomial<K>[] q,
        DiffPoly<K> D, int n)
    {
        var db = b.Degree();
        var bd = b.Lc();
        var h = new UnivariatePolynomial<K>[q.Length];
        for (int i = 0; i < h.Length; i++)
            h[i] = q[i].CreateZero();
        while (n >= 0)
        {
            for (int i = 0; i < q.Length; i++)
            {
                var s = q[i].CreateMonomial(q[i].ring.DivideExact(q[i].Get(n + db), bd), n);
                h[i] += s;
                q[i] -= D(s) + b * s;
            }
        }
        
        var dc = q.All(q_i => q_i.IsZero()) ? -1 : q.Max(q_i => q_i.Degree());
        var M = new K[dc, q.Length];
        for (int i = 0; i < dc; i++)
        {
            for (int j = 0; j < q.Length; j++)
            {
                M[i, j] = q[j].Get(i);
            }
        }
        
        var (A, u) = ConstantSystem(M, q[0].ring.CreateZeroesArray(dc), D, q[0].ring);
        var n_eq = A.GetLength(0);
        for (int i = 0; i < q.Length; i++)
        {
            A[i + n_eq, i] = q[0].ring.GetOne();
            A[i + n_eq, i + q.Length] = q[0].ring.GetNegativeOne();
        }

        return (h, A);
    }
    
    public static (UnivariatePolynomial<K>[] h, K[,] A) ParamPolyRischDENoCancel2<K>(UnivariatePolynomial<K> b, UnivariatePolynomial<K>[] q,
        DiffPoly<K> D, int n)
    {
        var delta = delta(t);
        var lambda = lambda(t);
        var h = new UnivariatePolynomial<K>[q.Length];
        for (int i = 0; i < h.Length; i++)
            h[i] = q[i].CreateZero();

        while (n > 0)
        {
            for (int i = 0; i < h.Length; i++)
            {
                var s = q[i].CreateMonomial(q[i].Get(n + delta - 1) / (n*lambda), n);
                h[i] = h[i] + s;
                q[i] = q[i] - D(s) - b*s;
            }

            n--;
        }

        if (b.Degree() > 0)
        {
            for (int i = 0; i < q.Length; i++)
            {
                var s = q[i].CreateConstant(q[i].Get(b.Degree()) / (b.Lc()));
                h[i] = h[i] + s;
                q[i] = q[i] - D(s) - b*s;
            }
            var dc = q.All(q_i => q_i.IsZero()) ? -1 : q.Max(q_i => q_i.Degree());
            var M = new K[dc, q.Length];
            for (int i = 0; i < dc; i++)
            {
                for (int j = 0; j < q.Length; j++)
                {
                    M[i, j] = q[j].Get(i);
                }
            }
            
            var (A, u) = ConstantSystem(M, q[0].ring.CreateZeroesArray(dc), D, q[0].ring);
            var n_eq = A.GetLength(0);
            for (int i = 0; i < q.Length; i++)
            {
                A[i + n_eq, i] = q[0].ring.GetOne();
                A[i + n_eq, i + q.Length] = q[0].ring.GetNegativeOne();
            }

            return (h, A);
        }
        else
        {
            var (f, B) = ParamRischDE(b, q.Select(q_i => q_i.Evaluate(0)).ToArray());
            int dc;
            if (q.All(q_i => q_i.IsZero()))
                dc = f.All(f_i => (D(f_i) + b*f_i).IsZero()) ? -1 : 0;
            else
                dc = q.Max(q_i => q_i.Degree());

            var M = new K[dc, q.Length];
            for (int i = 0; i < dc; i++)
            {
                for (int j = 0; j < q.Length; j++)
                {
                    M[i, j] = q[j].Get(i);
                }
            }

            for (int j = 0; j < f.Length; j++)
            {
                M[0, j + q.Length] = -D(f[j]) - b * f[j];
            }
            var (A, u) = ConstantSystem(M, q[0].ring.CreateZeroesArray(dc), D, q[0].ring);
            A = A.union(B);
            var n_eq = A.GetLength(0);
            for (int i = 0; i < q.Length; i++)
            {
                A[i + n_eq, i] = q[0].ring.GetOne();
                A[i + n_eq, i + f.Length + q.Length] = q[0].ring.GetNegativeOne();
            }
            
            return (f, h, A);
        }
    }

    public static (UnivariatePolynomial<K>, Rational<UnivariatePolynomial<K>> b, UnivariatePolynomial<K>, int N, Rational<UnivariatePolynomial<K>>, Rational<UnivariatePolynomial<K>>[]) LimitedIntegrateReduce<K>(Rational<UnivariatePolynomial<K>> f,
        Rational<UnivariatePolynomial<K>>[] w, Diff<K> D)
    {
        var (dn, ds) = Risch.SplitFactor(f.Denominator(), D);
        var en = new UnivariatePolynomial<K>[w.Length];
        var es = new UnivariatePolynomial<K>[w.Length];
        for (int i = 0; i < w.Length; i++)
        {
            (en[i], es[i]) = Risch.SplitFactor(w[i].Denominator(), D);
        }

        var c = f.ring.Lcm([dn, ..en]);
        var hn = UnivariateGCD.PolynomialGCD(c, c.Derivative());
        var a = hn;
        var b = -D(hn);
        var N = 0;
        if ()
        {
            var hs = f.ring.Lcm([ds, ..es]);
            a = hn * hs;
            b = -D(hn) - hn * D(hs) / hs;
            var mu = w.Append(f).Min(p => order_inf(p));
            N = hn.Degree() + hs.Degree() + Math.Max(0, 1 - delta(t) - mu);
        }

        return (a, b, a, N, a * hn * f, w.Select(w_i => -a * hn * w_i).ToArray());
    }
    
    public static ()? ParametricLogarithmicDerivative<K>(Rational<UnivariatePolynomial<K>> f, Rational<UnivariatePolynomial<K>> theta, DiffPoly<K> D)
    {
        var w = D(theta) / theta;
        var d = f.Denominator();
        var e = w.Denominator();
        var (p, a) = UnivariateDivision.DivideAndRemainder(f.Numerator(), d).ToTuple2();
        var (q, b) = UnivariateDivision.DivideAndRemainder(w.Numerator(), e).ToTuple2();
        var B = Math.Max(0, D(t).Degree() - 1);
        var C = Math.Max(p.Degree(), q.Degree());
        if (q.Degree() > B)
        {
            var s = solve();
            if (s is null || s not in Q)
                return null;
            var N = s.Numerator();
            var M = s.Denominator();
            if ()
                return (Q * N, Q * M, v);
            return null;
        }

        if (p.Degree() > B)
            return null;

        var l = f.ring.Lcm(d, e);
        var (ln, ls) = Risch.SplitFactor(l, D);
        var z = ls*UnivariateGCD.PolynomialGCD(ln, ln.Derivative());
        if (z.IsConstant())
            return null;

        var (u1, r1) = UnivariateDivision.DivideAndRemainder((l * f).NumeratorExact(), z).ToTuple2();
        var (u2, r2) = UnivariateDivision.DivideAndRemainder((l * w), z).ToTuple2();

        var s = solve();
        if (s is null || s not in Q)
            return null;
        
        var M = s.Numerator();
        var N = s.Denominator();
        if ()
            return (Q * N, Q * M, v);
        return null;
    }
   
}