using System.Diagnostics;
using Polynomials;
using Polynomials.Poly.Univar;

namespace ConsoleApp1.Core.Integrals;

public static class RischDE
{
    public delegate UnivariatePolynomial<K> DiffPoly<K>(UnivariatePolynomial<K> poly);
    
    public static UnivariatePolynomial<K> WeakNormalizer<K>(Rational<UnivariatePolynomial<K>> f, DiffPoly<K> D)
    {
        var (dn, ds) = Risch.SplitFactor(f.Denominator(), D);
        var g = UnivariateGCD.PolynomialGCD(dn, dn.Derivative());
        var d_s = dn / g;
        var d1 = d_s / UnivariateGCD.PolynomialGCD(d_s, g);
        var (a, b) = Risch.ExtendedEuclidieanDiophantine(f.Denominator() / d1, d1, f.Numerator());

        var Dd1 = D(d1);
        // To K[z][t]
        var ringK = a.ring;
        var ringZOverK = Rings.UnivariateRing(ringK);
        var newA = a.MapCoefficients(ringZOverK, cf => PolynomialFactory.Uni(ringK, cf));
        var newD1 = a.MapCoefficients(ringZOverK, cf => PolynomialFactory.Uni(ringK, cf));
        var newDd1 = a.MapCoefficients(ringZOverK, cf => PolynomialFactory.Uni(ringK, cf));
        var newZ = PolynomialFactory.Uni(ringZOverK, PolynomialFactory.Uni(ringK, [ringK.GetZero(), ringK.GetOne()]));
        
        var r = UnivariateResultants.Resultant(newA - newZ * newDd1, newD1); // K[z]
        int[] N = PositiveIntegerRoots(r);
        var result = a.CreateOne();
        foreach (var n in N)
        {
            result *= UnivariateGCD.PolynomialGCD(a - n * D(d1), d1).Pow(n);
        }

        return result;
    }

    public static (UnivariatePolynomial<K> a, Rational<UnivariatePolynomial<K>> b, Rational<UnivariatePolynomial<K>> c, UnivariatePolynomial<K> h)? 
        RdeNormalDenominator<K>(Rational<UnivariatePolynomial<K>> f, Rational<UnivariatePolynomial<K>> g, DiffPoly<K> D)
    {
        var (dn, ds) = Risch.SplitFactor(f.Denominator(), D);
        var (en, es) = Risch.SplitFactor(g.Denominator(), D);
        var p = UnivariateGCD.PolynomialGCD(dn, en);
        var h = UnivariateGCD.PolynomialGCD(en, en.Derivative()) / UnivariateGCD.PolynomialGCD(p, p.Derivative());
        var d_hsquare = dn * h.Clone().Square();
        if (UnivariateDivision.DivideOrNull(d_hsquare, en) is null)
            return null;
        
        return (dn * h, dn * h * f - dn * D(h), d_hsquare * g, h);
    }


    public static (UnivariatePolynomial<K> a, UnivariatePolynomial<K> b, UnivariatePolynomial<K> c, UnivariatePolynomial<K> h) RdeSpecialDenomExp<K>(UnivariatePolynomial<K> a, Rational<UnivariatePolynomial<K>> b,
        Rational<UnivariatePolynomial<K>> c, DiffPoly<K> D)
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
                n = Math.Min(m, n);
            }
        }

        var N = Math.Max(0, -nb, n - nc);
        return (a * p.Pow(N), (b + n * a * D(p) / p) * p.Pow(N), c * p.Pow(N - n), p.Pow(-n));
    }


    public static (UnivariatePolynomial<K> a, UnivariatePolynomial<K> b, UnivariatePolynomial<K> c,
        UnivariatePolynomial<K> h) RdeSpecielDenomTan<K>(UnivariatePolynomial<K> a, Rational<UnivariatePolynomial<K>> b,
            Rational<UnivariatePolynomial<K>> c, DiffPoly<K> D)
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

        var N = Math.Max(0, -nb, n - nc);
        return (a * p.Pow(N), (b + n * a * D(p) / p) * p.Pow(N), c * p.Pow(N - n), p.Pow(-n));
    }

    public static int RdeBoundDegreePrim<K>(UnivariatePolynomial<K> a, UnivariatePolynomial<K> b,
        UnivariatePolynomial<K> c, DiffPoly<K> D)
    {
        var da = a.Degree();
        var db = b.Degree();
        var dc = c.Degree();
        var n = db > da ? Math.Max(0, dc - db) : Math.Max(0, dc - da + 1);
        var ring = a.ring;
        if (db == da - 1)
        {
            var alpha = ring.Negate(ring.DivideExact(b.Lc(), a.Lc()));
            if ()
                n = Math.Max(n, m);
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

    public static int RdeBoundDegreeBase<K>(UnivariatePolynomial<K> a, UnivariatePolynomial<K> b,
        UnivariatePolynomial<K> c)
    {
        var da = a.Degree();
        var db = b.Degree();
        var dc = c.Degree();
        var n = Math.Max(0, dc - Math.Max(db, da - 1));
        if (db == da - 1)
        {
            var ring = a.ring;
            var m = ring.Negate(ring.DivideExact(b.Lc(), a.Lc()));
            if ()
                n = Math.Max(0, m, dc - db);
        }

        return n;
    }

    public static int RdeBoundDegreeExp<K>(UnivariatePolynomial<K> a, UnivariatePolynomial<K> b,
        UnivariatePolynomial<K> c, DiffPoly<K> D)
    {
        var da = a.Degree();
        var db = b.Degree();
        var dc = c.Degree();
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

    public static int RdeBoundDegreeNonLinear<K>(UnivariatePolynomial<K> a, UnivariatePolynomial<K> b,
        UnivariatePolynomial<K> c, DiffPoly<K> D)
    {
        var da = a.Degree();
        var db = b.Degree();
        var dc = c.Degree();
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

    public static (UnivariatePolynomial<K> b, UnivariatePolynomial<K> c, int m, UnivariatePolynomial<K> alpha, UnivariatePolynomial<K> beta)? SPDE<K>(UnivariatePolynomial<K> a, UnivariatePolynomial<K> b, UnivariatePolynomial<K> c,
        DiffPoly<K> D, int n)
    {
        if (n < 0)
            return c.IsZero() ? (a.CreateZero(), a.CreateZero(), 0, a.CreateZero(), a.CreateZero()) : null;
        var g = UnivariateGCD.PolynomialGCD(a, b);
        if (UnivariateDivision.DivideOrNull(c, g) is null)
            return null;
        a = a / g;
        b = b / g;
        c = c / g;
        if (a.Degree() == 0)
            return (b / a, c / a, n, a.CreateOne(), a.CreateZero());
        var (r, z) = Risch.ExtendedEuclidieanDiophantine(b, a, c);
        var u = SPDE(a, b + D(a), z - D(r), D, n - a.Degree());
        if (u is null)
            return null;

        var sol = u.Value;
        return (sol.b, sol.c, sol.m, a * sol.alpha, a * sol.beta + r);
    }

    public static UnivariatePolynomial<K>? PolyRischDENoCancel1<K>(UnivariatePolynomial<K> b, UnivariatePolynomial<K> c,
        DiffPoly<K> D, int n)
    {
        var q = b.CreateZero();
        while (!c.IsZero())
        {
            var m = c.Degree() - b.Degree();
            if (n < 0 || m < 0 || m > n)
                return null;

            var p = b.CreateMonomial(b.ring.DivideExact(c.Lc(), b.Lc()), m);
            q += p;
            n = m - 1;
            c = c - D(p) - b * p;
        }

        return q;
    }

    public static UnivariatePolynomial<K>? PolyRischDENoCancel2<K>(UnivariatePolynomial<K> b, UnivariatePolynomial<K> c,
        DiffPoly<K> D, int n)
    {
        var q = b.CreateZero();
        while (!c.IsZero())
        {
            var m = n == 0 ? 0 : c.Degree() - delta(t) + 1;
            if (n < 0 || m < 0 || m > n)
                return null;

            UnivariatePolynomial<K> p;
            if (m > 0)
                p = b.CreateMonomial(b.ring.DivideExact(c.Lc(), m * lambda(t)), m);
            else
            {
                if (b.Degree() != c.Degree())
                    return null;
                if (b.Degree() == 0)
                    return (q, b, c)
                p = b.CreateConstant(b.ring.DivideExact(c.Lc(), b.Lc()));
            }
                
            q += p;
            n = m - 1;
            c = c - D(p) - b * p;
        }

        return q;
    }

}