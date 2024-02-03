using ConsoleApp1.core.expr.atoms;
using ConsoleApp1.utils;

namespace ConsoleApp1.core;

public class Poly
{
    /// [a,b,c] -> ax^2+bx+c
    public Expr[] Coefs;

    public Poly(params Expr[] coefs)
    {
        if (coefs.Length == 0)
            coefs = new[] { Zero };
        
        Coefs = coefs;
    }

    // TODO
    public static Poly? ToPoly(Expr expr)
    {
        return null;
    }

    public override string ToString()
    {
        
        var str = "";
        for (var i = 0; i < Coefs.Length; i++)
        {
            var coef = Coefs[i];
            var deg = Deg() - i; 
            if (coef.IsZero()) continue;
            if (deg == 0)
            {
                str += "+" + coef;
                continue;
            }
            if (deg == 1)
            {
                str += "+" + coef + "x";
                continue;
            }
            str += "+" + coef + "x^" + deg;
        }
        
        if (str == "") 
            return "0";
        
        if (str[0] == '+')
            str = str.Substring(1);

        return str;
    }

    public int Deg()
    {
        return Coefs.Length - 1;
    }
    
    public Expr LC()
    {
        return Coefs[0];
    }

    public Expr CoefDeg(int deg)
    {
        return Coefs[Deg() - deg];
    }

    public bool IsZero()
    {
        return Coefs.Length == 1 && Coefs[0].IsZero();
    }
    
    public static Poly Gcd(Poly a, Poly b)
    {
        if (b.Deg() > a.Deg())
        {
            (a, b) = (b, a);
        }

        var quotient = new Poly(Un);
        while (!b.IsZero())
        {
            Console.WriteLine(a+";"+b);
            (quotient, var reste) = Div(a, b);
            Console.WriteLine(quotient+";"+reste);

            a = b;
            b = reste;
        }
        
        
        return quotient;
    }
    
    
    // TODO
    public Poly Derivee()
    {

        var deriveeCoefs = new Expr[Coefs.Length - 1];
        var deg = Deg();
        for (int i = 0; i < Coefs.Length - 1; i++)
        {
            deriveeCoefs[i] = (deg - i).Expr() * Coefs[i];
        }
        
        return new Poly(deriveeCoefs);
    }

    /// https://en.wikipedia.org/wiki/Square-free_polynomial
    public static Poly[] YunSquareFree(Poly f)
    {
        var a = new List<Poly>();
        var df = f.Derivee();
        a.Add(Gcd(f, df));

        var (b, _) = Div(f, a[0]);
        var (c, _) = Div(df, a[0]);
        var d = c - b.Derivee();
        var i = 1;

        while (b.Deg() > 1)
        {
            a[i] = Gcd(b, d);
            (b, _) = Div(b, a[i]);
            (c, _) = Div(d, a[i]);
            d = c - b.Derivee();
            i++;
        }
        
        a.RemoveAt(0);
        return a.ToArray();
    }

    /// a = qb + r
    public static (Poly, Poly) Div(Poly a, Poly b)
    {
        var q = new Expr[a.Deg() - b.Deg() + 1];
        for (int i = 0; i <= a.Deg() - b.Deg(); i++)
        {

            var aLc = a.Coefs[i];
            q[i] = aLc / b.LC();

            for (int j = 0; j <= b.Deg(); j++)
            {
                a.Coefs[j + i] -= b.Coefs[j] * q[i];
            }
        }
        
        Console.WriteLine(a);
        var rCoefs = Array.Empty<Expr>(); 
        var init = false;
        for (int i = 1; i < a.Deg() - b.Deg(); i++)
        {
            Console.WriteLine(a.Coefs[i+b.Deg()]);

            var a_i = a.Coefs[b.Deg() + i];
            if (init)
            {
                rCoefs[i] = a_i;
                continue;
            }

            if (!a_i.IsZero())
            {
                init = true;
                rCoefs = new Expr[a.Deg() - (b.Deg() + i) + 1];
                rCoefs[i] = a_i;
            }
        }
        

        return (new Poly(q), new Poly(rCoefs));
    }
    
    public static Poly operator -(Poly a, Poly b)
    {
        var n = Math.Max(a.Coefs.Length, b.Coefs.Length);
        var c = new Expr[n];
        for (var i = 0; i < n; i++)
        {
            var ai = i < a.Coefs.Length ? a.Coefs[i] : Zero;
            var bi = i < b.Coefs.Length ? b.Coefs[i] : Zero;
            c[i] = ai - bi;
        }
        return new Poly(c);
    }
    
    public static Poly operator +(Poly a, Poly b)
    {
        var n = Math.Max(a.Coefs.Length, b.Coefs.Length);
        var c = new Expr[n];
        for (var i = 0; i < n; i++)
        {
            var ai = i < a.Coefs.Length ? a.Coefs[i] : Zero;
            var bi = i < b.Coefs.Length ? b.Coefs[i] : Zero;
            c[i] = ai + bi;
        }
        return new Poly(c);
    }
    
    public static Poly operator *(Poly a, Poly b)
    {
        var n = a.Coefs.Length + b.Coefs.Length - 1;
        var c = new Expr[n];
        for (var i = 0; i < a.Coefs.Length; i++)
            for (var j = 0; j < b.Coefs.Length; j++)
                c[i + j] += a.Coefs[i] * b.Coefs[j];
        return new Poly(c);
    }

    public Expr[] Solve()
    {
        // 1) c_i x^i + c_i-1 x^(i-1) + ... + c_0 x^m --> c_i x^(i-m) + c_i-1 x^(i-1-m) + ... + c_0

        // [0,0,0,c,b,a] -> m=3
        var m = MinDeg();
        // [c,b,a]
        var newLength = Coefs.Length - m;
        var newCoefficients = new Expr[newLength];
        Array.Copy(Coefs, m, newCoefficients, 0, newLength);
        Coefs = newCoefficients;

        // TODO
        // 2) c_i x^pi + c_i-1 x^p(i-1) + ... + c_0 --> c_i x^i + c_i-1 x^(i-1) + ... + c_0

        // TODO
        // 3) a c_i x^i + a c_i-1 x^(i-1) + ... + a c_0 --> c_i x^i + c_i-1 x^(i-1) + ... + c_0

        // TODO
        // 4) deg <= 4 -> Formula

        // TODO

        return null;
    }

    // [0,0,0,9] -> 3
    // [9,4,3] -> 0
    // [0,0,0] -> Error
    public int MinDeg()
    {
        for (var deg = 0; deg < Coefs.Length; deg++)
            if (!Coefs[deg].IsZero())
                return deg;

        throw new Exception("Polynome Zero");
    }

    public IEnumerable<(Expr, int)> AsCoefDeg()
    {
        for (var deg = 0; deg < Coefs.Length; deg++)
            if (!Coefs[deg].IsZero())
                yield return (Coefs[deg], deg);
    }

    /// a x^2 + b x + c = 0
    public static (Expr, Expr) SolveParabole(Expr a, Expr b, Expr c)
    {
        var delta = b*b - 4*a*c;

        var mb = -b;
        var a2 = 2*a;

        return ((mb + Sqrt(delta)) / a2, (mb - Sqrt(delta)) / a2);
    }
}