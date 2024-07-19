namespace ConsoleApp1.Core.TestDir;

// ReSharper disable once InconsistentNaming
public class PolyGF(int[] coefs, int p)
{
    public int[] Coefs = coefs.Map(n => Mod(n, p));
    public int P = p;
    
    public static PolyGF Zero => new PolyGF([0], 11);
    public static PolyGF One => new PolyGF([1], 11);
    public static PolyGF X => new PolyGF([1, 0], 11);
    
    public static implicit operator PolyGF(int n) => new PolyGF([n], 11);

    public override string ToString()
    {
        return string.Join('+', Coefs.Select((coef, i) => (coef, i)).Where(t => t.coef != 0).Select(t => $"{t.coef}*x^{Deg()-t.i}"));
    }

    public int Deg()
    {
        return Coefs.Length - 1;
    }
    
    public int LastCoef()
    {
        return Coefs[Deg()];
    }
    public int this[int i]
    {
        get => i < Coefs.Length ? Coefs[i] : 0;
        set => Coefs[i] = value;
    }

    public static PolyGF operator *(PolyGF a, int b)
    {
        var coefs = new int[a.Deg() + 1];
        for (var i = 0; i <= a.Deg(); i++)
        {
            coefs[i] = (a[i] * b) % a.P;
        }

        return new PolyGF(coefs, a.P);
    }
    
    public static PolyGF operator /(PolyGF a, int b)
    {
        var coefs = new int[a.Deg() + 1];
        for (var i = 0; i <= a.Deg(); i++)
        {
            coefs[i] = (a[i] / b) % a.P;
        }

        return new PolyGF(coefs, a.P);
    }

    public static PolyGF operator *(PolyGF a, PolyGF b)
    {
        var n = a.Deg();
        var m = b.Deg();
        var coefs = new int[n + m + 1];
        for (var i = 0; i <= n; i++)
        {
            for (var j = 0; j <= m; j++)
            {
                coefs[i + j] = (coefs[i + j] + a[i] * b[j]) % a.P;
            }
        }
        
        return new PolyGF(coefs, a.P);
    }

    public static PolyGF operator +(PolyGF a, PolyGF b)
    {
        var n = Math.Max(a.Deg(), b.Deg());
        var coefs = new int[n + 1];
        for (int i = 0; i < a.Coefs.Length; i++)
        {
            coefs[n - i] = a.Coefs[a.Deg() - i];
        }
        for (int i = 0; i < b.Coefs.Length; i++)
        {
            coefs[n - i] = (coefs[n - i] + b.Coefs[b.Deg() - i]) % a.P;
        }
        
        return WithoutLeadingZeros(coefs, a.P);
    }
    
    public static PolyGF operator -(PolyGF a, PolyGF b)
    {
        var n = Math.Max(a.Deg(), b.Deg());
        var coefs = new int[n + 1];
        for (int i = 0; i < a.Coefs.Length; i++)
        {
            coefs[n - i] = a.Coefs[a.Deg() - i];
        }
        for (int i = 0; i < b.Coefs.Length; i++)
        {
            coefs[n - i] = (coefs[n - i] - b.Coefs[b.Deg() - i]) % a.P;
        }
        
        return WithoutLeadingZeros(coefs, a.P);
    }

    public static PolyGF Monic(PolyGF f)
    {
        if (f.Coefs.Length == 0)
            return Zero;

        var lc = f[0];

        if (lc == 1)
            return f;

        return f / lc;
    }

    public static PolyGF Gcd(PolyGF f, PolyGF g)
    {
        while (g.Coefs.Length != 0)
        {
            (f, g) = (g, Rem(f, g));
        }

        return Monic(f);
    }
    
    public static PolyGF Quo(PolyGF f, PolyGF g)
    {
        var df = f.Deg();
        var dg = g.Deg();

        if (g.Coefs.Length == 0)
            throw new Exception("polynomial division");
        else if (df < dg)
            return Zero;

        var inv = g[0] % f.P;

        var h = new PolyGF(f.Coefs, f.P);
        var dq = df - dg;
        var dr = dg - 1;

        for (var i = 0; i < dq + 1; i++)
        {
            var coeff = h[i];

            for (var j = Math.Max(0, dg - i); j < Math.Min(df - i, dr) + 1; j++)
            {
                coeff -= h[i + j - dg] * g[dg - j];
            }

            h[i] = (coeff * inv) % f.P;
        }
        
        var coefs = new int[dq + 1];
        for (var i = 0; i < dq + 1; i++)
        {
            coefs[i] = h[i];
        }

        return new PolyGF(coefs, f.P);
    }
    
    public static PolyGF Rem(PolyGF a, PolyGF b)
    {
        if (a.Deg() < b.Deg())
            return a;
        
        return a - Quo(a, b) * b;
    }
    
    public static PolyGF PowMod(PolyGF f, PolyGF g, int n)
    {

        if (n == 0)
            return One;
        else if (n == 1)
            return Rem(f, g);
        else if (n == 2)
            return Rem(Sqr(f), g);

        var h = One;
        
        while (true)
        {
            if ((n & 1) != 0)
            {
                h = h * f;
                h = Rem(h, g);
                n -= 1;
            }


            n >>= 1;

            if (n == 0)
                break;

            f = Sqr(f);
            f = Rem(f, g);
        }

        return h;
    }
    
    public static PolyGF WithoutLeadingZeros(int[] array, int p)
    {
        var numZero = 0;
        for (var i = 0; i < array.Length; i++)
        {
            if (array[i] == 0)
                numZero += 1;
            else
                break;
        }
        var coefs = new int[array.Length - numZero];
        for (var i = 0; i < coefs.Length; i++)
        {
            coefs[i] = array[i + numZero];
        }

        return new PolyGF(coefs, p);
    }
    
    public static PolyGF Sqr(PolyGF f)
    {
        var df = f.Deg();

        var dh = 2 * df;
        var h = new int[dh + 1];

        for (var i = 0; i < dh + 1; i++)
        {
            var coeff = 0;

            var jmin = Math.Max(0, i - df);
            var jmax = Math.Min(i, df);

            var n = jmax - jmin + 1;

            jmax = jmin + n/2 - 1;

            for (var j = jmin; j < jmax + 1; j++)
                coeff += f[j] * f[i - j];

            coeff += coeff;

            if ((n & 1) != 0)
            {
                var elem = f[jmax + 1];
                coeff += elem * elem;
            }

            h[i] = coeff;
        }

        return WithoutLeadingZeros(h, f.P);
    }
    public static PolyGF LShift(PolyGF a, int n)
    {
        var coefs = new int[a.Coefs.Length + n];
        for (var i = 0; i < a.Coefs.Length; i++)
        {
            coefs[i] = a[i];
        }
        return new PolyGF(coefs, a.P);
    }
    
    public bool IsOne()
    {
        return Deg() == 0 && Coefs[0] == 1;
    }
    
    public static int Mod(int a, int p)
    {
        return a;
    }
    
}

public class PolyRoots
{
    public static List<(PolyGF, int)> AsFactors()
    {
        var p = 11;
        var i = 1;
        var g = PolyGF.X;
        var f = new PolyGF([1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10], p);
        var b = GetB(f, p);
        var factors = new List<(PolyGF, int)>();
        while (2*i <= f.Deg())
        {
            g = GetG(g, f, b, p);
            var h = PolyGF.Gcd(f, g - PolyGF.X);

            if (!h.IsOne())
            {
                factors.Add((h, i));

                f = PolyGF.Quo(f, h);
                g = PolyGF.Rem(g, f);
                b = GetB(f, p);
            }

            i += 1;
        }

        if (!f.IsOne())
            factors.Add((f, f.Deg()));
        
        return factors;
    }
    


    public static PolyGF[] GetB(PolyGF g, int p)
    {
        var n = g.Deg();
        if (n <= 0)
            return [];
        var b = new PolyGF[n];
        b[0] = PolyGF.One;
        if (p < n)
        {
            for (var i = 1; i < n; i++)
            {
                var mon = PolyGF.LShift(b[i - 1], p);
                b[i] = PolyGF.Rem(mon, g);
            }
        }
        else if (n != 1)
        {
            b[1] = PolyGF.PowMod(PolyGF.X, g, p);
            for (var i = 2; i < n; i++)
            {
                b[i] = b[i - 1] * b[1];
                b[i] = PolyGF.Rem(b[i], g);
            }
        }
            
        return b;
    }

    public static PolyGF GetG(PolyGF f, PolyGF g, PolyGF[] b, int p)
    {
        var m = g.Deg();
        if (f.Deg() >= m)
            f = PolyGF.Rem(f, g);

        var n = f.Deg();
        PolyGF sf = f.LastCoef();
        for (var i = 1; i <= n; i++)
        {
            var v = b[i] * f[n - i];
            sf = sf + v;
        }
        
        return sf;
    }

}