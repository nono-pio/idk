﻿using ConsoleApp1.Core.Expr.Atoms;
using ConsoleApp1.utils;

namespace ConsoleApp1.Core;

public class Poly
{
    /// [a,b,c] -> ax^2+bx+c
    private Expr.Expr[] _coefs;

    public Poly(params Expr.Expr[] coefs)
    {
        if (coefs.Length == 0)
            coefs = new[] { Zero };
        
        _coefs = coefs;
    }
    
    public Poly(params int[] coefs)
    {
        if (coefs.Length == 0)
        {
            _coefs = new[] { Zero };
            return;
        }
        _coefs = new Expr.Expr[coefs.Length];
        for (int i = 0; i < coefs.Length; i++)
        {
            _coefs[i] = coefs[i].Expr();
        }
        
    }

    public static Poly VideDeg(int deg)
    {
        var coefs = new Expr.Expr[deg + 1];
        Array.Fill(coefs, Zero);

        return new Poly(coefs);
    }
    
    public int Deg()
    {
        return _coefs.Length - 1;
    }

    public Expr.Expr CoefDeg(int deg)
    {
        if (deg > Deg())
            return Zero;
        
        return _coefs[Deg() - deg];
    }

    public void SetCoefDeg(int deg, Expr.Expr value)
    {
        if (deg <= Deg())
            _coefs[Deg() - deg] = value;
    }
    
    public Expr.Expr LC()
    {
        return CoefDeg(Deg());
    }

    public Poly EnleveZeroInutile()
    {

        if (!LC().IsZero())
            return this;

        Poly? newPoly = null;
        var noZero = false;

        for (int deg = Deg(); deg >= 0; deg--)
        {
            if (noZero)
            {
                newPoly.SetCoefDeg(deg, CoefDeg(deg));
                continue;
            }

            if (!CoefDeg(deg).IsZero())
            {
                newPoly = VideDeg(deg);
                noZero = true;
            }
        }

        return newPoly ?? PolyZero;
    }

    public bool Equals(Poly poly2)
    {
        if (_coefs.Length != poly2._coefs.Length)
            return false;

        for (int i = 0; i < _coefs.Length; i++)
        {
            if (_coefs[i] != poly2._coefs[i])
                return false;
        }

        return true;
    }

    // TODO
    public static Poly? ToPoly(Expr.Expr expr)
    {
        return null;
    }

    public override string ToString()
    {
        
        var str = "";
        for (var i = 0; i < _coefs.Length; i++)
        {
            var coef = _coefs[i];
            var deg = Deg() - i; 
            if (coef.IsZero())
                continue;
            var coef_str = coef.IsOne() && deg != 0 ? "" : coef.ToString();
            switch (deg)
            {
                case 0:
                    str += "+" + coef_str;
                    continue;
                case 1:
                    str += "+" + coef_str + "x";
                    continue;
                default:
                    str += "+" + coef_str + "x^" + deg;
                    break;
            }
        }
        
        if (str == "") 
            return "0";
        
        if (str[0] == '+')
            str = str[1..];

        return str;
    }

    public Poly Clone()
    {
        var coefs = new Expr.Expr[_coefs.Length];
        for (int i = 0; i < coefs.Length; i++)
        {
            coefs[i] = _coefs[i];
        }

        return new Poly(coefs);
    }


    public bool IsZero()
    {
        return Deg() == 0 && CoefDeg(0).IsZero();
    }

    /// (g, (u, v))
    /// g = gcd(a,b)
    /// g = ua+vb
    /// TODO: Ne marche pas avec deux constant (ex: gcd(8, 12) = 1)
    public static (Poly, (Poly, Poly)) ExtendedGcd(Poly a, Poly b)
    {
        var (old_r, r) = (a, b);
        var (old_s, s) = (new Poly(Un), PolyZero);
        var (old_t, t) = (PolyZero, new Poly(Un));
        
        while (!r.IsZero())
        {
            Console.WriteLine((old_r, r));
            var (q, new_r) = Div(old_r, r);
            (old_r, r) = (r, new_r);
            (old_s, s) = (s, old_s - q * s);
            (old_t, t) = (t, old_t - q * t);
        }

        var lc_r = old_r.LC();
        old_r /= lc_r;
        old_s /= lc_r;
        old_t /= lc_r;
        
        return (old_r, (old_s, old_t));
    }
    
    public static Poly Gcd(Poly a, Poly b)
    {
        var (gcd, _) = ExtendedGcd(a, b);
        return gcd;
    }
    
    
    // TODO
    public Poly Derivee()
    {

        var derivee = VideDeg(Deg() - 1);
        for (int deg = 1; deg <= Deg(); deg++)
        {
            // cx^n -> c*nx^(n-1)
            derivee.SetCoefDeg(deg - 1, deg.Expr() * CoefDeg(deg));
        }

        return derivee;
    }

    /// https://en.wikipedia.org/wiki/Square-free_polynomial
    public static Poly[] YunSquareFree(Poly f)
    {
        var df = f.Derivee();
        Console.WriteLine((f,df));
        var a = new List<Poly> { Gcd(f.Clone(), df.Clone()) };

        var (b, _) = Div(f, a[0]);
        var (c, _) = Div(df, a[0]);
        var d = c - b.Derivee();
        var i = 1;
        
        Console.WriteLine((a.Last(),b,c,d));

        while (b.Deg() > 0)
        {
            a.Add(Gcd(b.Clone(), d.Clone()));
            (b, _) = Div(b, a[i]);
            (c, _) = Div(d, a[i]);
            d = c - b.Derivee();
            Console.WriteLine((i,a.Last(),b,c,d));

            i++;
        }
        
        a.RemoveAt(0);
        return a.ToArray();
    }

    /// a = qb + r
    public static (Poly, Poly) Div(Poly a, Poly b)
    {
        var q = new Expr.Expr[a.Deg() - b.Deg() + 1];
        for (int i = 0; i <= a.Deg() - b.Deg(); i++)
        {

            var aLc = a._coefs[i];
            q[i] = aLc / b.LC();

            for (int j = 0; j <= b.Deg(); j++)
            {
                a._coefs[j + i] -= b._coefs[j] * q[i];
            }
        }

        var r = new Poly(a._coefs).EnleveZeroInutile();

        return (new Poly(q), r);
    }

    public static Poly operator -(Poly a)
    {
        for (int deg = 0; deg <= a.Deg(); deg++)
        {
            a.SetCoefDeg(deg, -a.CoefDeg(deg));
        }

        return a;
    }
    
    public static Poly operator /(Poly a, Expr.Expr b)
    {
        for (int deg = 0; deg <= a.Deg(); deg++)
        {
            a.SetCoefDeg(deg, a.CoefDeg(deg) / b);
        }

        return a;
    }
    
    public static Poly operator *(Poly a, Expr.Expr b)
    {
        for (int deg = 0; deg <= a.Deg(); deg++)
        {
            a.SetCoefDeg(deg, a.CoefDeg(deg) * b);
        }

        return a;
    }
    
    public static Poly operator -(Poly a, Poly b)
    {
        var newDeg = Math.Max(a.Deg(), b.Deg());
        var result = VideDeg(newDeg);
        for (int deg = 0; deg <= newDeg; deg++)
        {
            result.SetCoefDeg( deg, a.CoefDeg(deg) - b.CoefDeg(deg) );
        }

        return result.EnleveZeroInutile();
    }
    
    
    public static Poly operator +(Poly a, Poly b)
    {
        
        var newDeg = Math.Max(a.Deg(), b.Deg());
        var result = VideDeg(newDeg);
        for (int deg = 0; deg <= newDeg; deg++)
        {
            result.SetCoefDeg( deg, a.CoefDeg(deg) + b.CoefDeg(deg) );
        }

        return result.EnleveZeroInutile();
    }
    
    public static Poly operator *(Poly a, Poly b)
    {
        var newDeg = a.Deg() + b.Deg();
        var result = VideDeg(newDeg);
        
        for (int deg_a = 0; deg_a <= a.Deg(); deg_a++)
        {
            for (int deg_b = 0; deg_b <= b.Deg(); deg_b++)
            {
                result.SetCoefDeg(deg_a + deg_b, a.CoefDeg(deg_a) * b.CoefDeg(deg_b));
            }
        }

        return result.EnleveZeroInutile();
    }

    public Expr.Expr[] Solve()
    {
        // TODO
        throw new NotImplementedException();
    }

    public IEnumerable<(Expr.Expr, int)> AsCoefDeg()
    {
        for (int deg = 0; deg <= Deg(); deg++)
        {
            yield return (CoefDeg(deg), deg);
        }
    }
    
    
    public IEnumerable<(Expr.Expr, int)> AsCoefNotZeroDeg()
    {
        for (int deg = 0; deg <= Deg(); deg++)
        {
            var coef = CoefDeg(deg);
            if (coef.IsNotZero())
                yield return (coef, deg);
        }
    }

    /// a x^2 + b x + c = 0
    public static (Expr.Expr, Expr.Expr) SolveParabole(Expr.Expr a, Expr.Expr b, Expr.Expr c)
    {
        var delta = b*b - 4*a*c;

        var mb = -b;
        var a2 = 2*a;

        return ((mb + Sqrt(delta)) / a2, (mb - Sqrt(delta)) / a2);
    }
}