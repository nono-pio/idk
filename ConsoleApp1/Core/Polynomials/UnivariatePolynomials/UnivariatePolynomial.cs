using System.Diagnostics;
using System.Text;
using ConsoleApp1.Core.Polynomials.Rings;

namespace ConsoleApp1.Core.Polynomials.UnivariatePolynomials;

public class UnivariatePolynomial<TElem> : IUnivariatePolynomial<UnivariatePolynomial<TElem>>
{
    public readonly Ring<TElem> Ring;
    public TElem[] Coefs;
    public int Deg => Coefs.Length - 1;

    public UnivariatePolynomial(Ring<TElem> ring, TElem[] coefs)
    {
        Ring = ring;
        Coefs = RemoveZeroes(coefs, ring);
    }
    
    private TElem[] RemoveZeroes(TElem[] coefs, Ring<TElem> ring)
    {
        var i = coefs.Length - 1;
        while (i >= 0 && Ring.IsZero(coefs[i]))
        {
            i--;
        }
        
        if (i == coefs.Length - 1)
            return coefs;
        
        if (i == -1)
            return [ring.Zero];

        return coefs[..(i + 1)];
    }

    public UnivariatePolynomial<TElem> MonomialToPolynomial(TElem coef, int deg)
    {
        var coefs = new TElem[deg + 1];
        coefs[deg] = coef;
        return new UnivariatePolynomial<TElem>(Ring, coefs);
    }

    public UnivariatePolynomial<TElem> Zero => new(Ring, [Ring.Zero]);
    public UnivariatePolynomial<TElem> One => new(Ring, [Ring.One]);
    public UnivariatePolynomial<TElem> ValueOf(int value) => new(Ring, [Ring.ValueOf(value)]);
    public UnivariatePolynomial<TElem> ValueOf(TElem value) => new(Ring, [value]);

    public bool IsZero(UnivariatePolynomial<TElem> e) => (e.Deg == 0 && Ring.IsZero(e.Coefs[0])) || e.Coefs.Length == 0;
    public bool IsOne(UnivariatePolynomial<TElem> e) => e.Deg == 1 && Ring.IsOne(e.Coefs[0]);
    public bool IsInt(UnivariatePolynomial<TElem> e, int value) => e.Deg == 1 && Ring.IsInt(e.Coefs[0], value);
    
    public TElem LC => Coefs[^1];
    public TElem CC => Coefs[0];
    
    public TElem Eval(TElem x)
    {
        var result = Ring.Zero;
        for (int i = Deg; i >= 0; i--)
        {
            result = Ring.Add(Ring.Mul(result, x), Coefs[i]);
        }

        return result;
    }
    
    public UnivariatePolynomial<TElem> Add(UnivariatePolynomial<TElem> a, UnivariatePolynomial<TElem> b)
    {
        var deg = Math.Max(a.Deg, b.Deg);
        var coefs = new TElem[deg + 1];
        for (int i = 0; i <= deg; i++)
        {
            coefs[i] = Ring.Zero;
            if (i <= a.Deg)
                coefs[i] = Ring.Add(coefs[i], a.Coefs[i]);
            if (i <= b.Deg)
                coefs[i] = Ring.Add(coefs[i], b.Coefs[i]);
        }

        return new UnivariatePolynomial<TElem>(Ring, coefs);
    }

    public UnivariatePolynomial<TElem> MAdd(UnivariatePolynomial<TElem> a, UnivariatePolynomial<TElem> b)
    {
        if (a.Deg >= b.Deg)
        {
            for (int i = 0; i <= b.Deg; i++)
            {
                a.Coefs[i] = Ring.Add(a.Coefs[i], b.Coefs[i]);
            }

            return this;
        }

        return Add(a, b);
    }

    public UnivariatePolynomial<TElem> Sub(UnivariatePolynomial<TElem> a, UnivariatePolynomial<TElem> b)
    {
        var deg = Math.Max(a.Deg, b.Deg);
        var coefs = new TElem[deg + 1];
        for (int i = 0; i <= deg; i++)
        {
            coefs[i] = Ring.Zero;
            if (i <= a.Deg)
                coefs[i] = Ring.Add(coefs[i], a.Coefs[i]);
            if (i <= b.Deg)
                coefs[i] = Ring.Sub(coefs[i], b.Coefs[i]);
        }

        return new UnivariatePolynomial<TElem>(Ring, coefs);
    }

    public UnivariatePolynomial<TElem> MSub(UnivariatePolynomial<TElem> a, UnivariatePolynomial<TElem> b)
    {
        if (a.Deg >= b.Deg)
        {
            for (int i = 0; i <= b.Deg; i++)
            {
                a.Coefs[i] = Ring.Sub(a.Coefs[i], b.Coefs[i]);
            }

            return this;
        }

        return Sub(a, b);
    }

    public UnivariatePolynomial<TElem> Neg(UnivariatePolynomial<TElem> a)
    {
        var coefs = new TElem[a.Deg + 1];
        for (int i = 0; i <= a.Deg; i++)
        {
            coefs[i] = Ring.Neg(a.Coefs[i]);
        }

        return new UnivariatePolynomial<TElem>(Ring, coefs);
    }

    public UnivariatePolynomial<TElem> Mul(UnivariatePolynomial<TElem> a, UnivariatePolynomial<TElem> b)
    {
        var coefs = new TElem[a.Deg + b.Deg + 1];
        Array.Fill(coefs, Ring.Zero);
        
        for (int i = 0; i <= a.Deg; i++)
        {
            for (int j = 0; j <= b.Deg; j++)
            {
                coefs[i + j] = Ring.Add(coefs[i + j], Ring.Mul(a.Coefs[i], b.Coefs[j]));
            }
        }
        
        return new UnivariatePolynomial<TElem>(Ring, coefs);
    }
    
    public UnivariatePolynomial<TElem> MulInt(UnivariatePolynomial<TElem> a, int b)
    {
        var coefs = new TElem[a.Deg + 1];
        for (int i = 0; i <= a.Deg; i++)
        {
            coefs[i] = Ring.MulInt(a.Coefs[i], b);
        }

        return new UnivariatePolynomial<TElem>(Ring, coefs);
    }
    
    public UnivariatePolynomial<TElem> Mul(UnivariatePolynomial<TElem> a, TElem b)
    {
        var coefs = new TElem[a.Deg + 1];
        for (int i = 0; i <= a.Deg; i++)
        {
            coefs[i] = Ring.Mul(a.Coefs[i], b);
        }

        return new UnivariatePolynomial<TElem>(Ring, coefs);
    }

    
    public UnivariatePolynomial<TElem> MMul(UnivariatePolynomial<TElem> a, UnivariatePolynomial<TElem> b)
    {
        return Mul(a, b);
    }

    public UnivariatePolynomial<TElem> Div(UnivariatePolynomial<TElem> a, UnivariatePolynomial<TElem> b)
    {
        return DivRem(a, b).Quotient;
    }

    public (bool isNull, UnivariatePolynomial<TElem> Value) SafeDiv(UnivariatePolynomial<TElem> a, UnivariatePolynomial<TElem> b)
    {
        return (false, a / b);
    }

    public UnivariatePolynomial<TElem> Div(UnivariatePolynomial<TElem> a, TElem b)
    {
        var coefs = new TElem[a.Deg + 1];
        for (int i = 0; i <= a.Deg; i++)
        {
            coefs[i] = Ring.Div(a.Coefs[i], b);
        }

        return new UnivariatePolynomial<TElem>(Ring, coefs);
    }

    public UnivariatePolynomial<TElem> Rem(UnivariatePolynomial<TElem> a, UnivariatePolynomial<TElem> b)
    {
        var rem = UnivariateDivision.RemCheck(a, b);
        if (rem is not null)
            return rem;
        
        return DivRem(a, b).Remainder;
    }

    public (UnivariatePolynomial<TElem> Quotient, UnivariatePolynomial<TElem> Remainder) DivRem(UnivariatePolynomial<TElem> n, UnivariatePolynomial<TElem> d)
    {
        return UnivariateDivision.DivisonWithRemainder(n, d);
    }
    
    public static UnivariatePolynomial<TElem> operator +(UnivariatePolynomial<TElem> a, UnivariatePolynomial<TElem> b)
    {
        Debug.Assert(a.Ring == b.Ring);
        return a.Add(a, b);
    }
    
    public static UnivariatePolynomial<TElem> operator -(UnivariatePolynomial<TElem> a, UnivariatePolynomial<TElem> b)
    {
        Debug.Assert(a.Ring == b.Ring);
        return a.Sub(a, b);
    }
    
    public static UnivariatePolynomial<TElem> operator *(UnivariatePolynomial<TElem> a, UnivariatePolynomial<TElem> b)
    {
        Debug.Assert(a.Ring == b.Ring);
        return a.Mul(a, b);
    }
    
    public static UnivariatePolynomial<TElem> operator *(UnivariatePolynomial<TElem> a, TElem b)
    {
        return a.Mul(a, b);
    }
    
    public static UnivariatePolynomial<TElem> operator /(UnivariatePolynomial<TElem> a, UnivariatePolynomial<TElem> b)
    {
        Debug.Assert(a.Ring == b.Ring);
        return a.Div(a, b);
    }
    
    public static UnivariatePolynomial<TElem> operator /(UnivariatePolynomial<TElem> a, TElem b)
    {
        return a.Div(a, b);
    }
    
    public static UnivariatePolynomial<TElem> operator %(UnivariatePolynomial<TElem> a, UnivariatePolynomial<TElem> b)
    {
        Debug.Assert(a.Ring == b.Ring);
        return a.Rem(a, b);
    }

    public UnivariatePolynomial<TElem> Gcd(UnivariatePolynomial<TElem> a, UnivariatePolynomial<TElem> b)
    {
        
        var g = Ring.Gcd(a.Content(), b.Content());
        
        var old_r = a / g;
        var r = b / g;
        
        while (!IsZero(r)){
            
            var new_r = Rem(old_r, r);
            old_r = r;
            r = new_r;
        }

        return old_r.PrimitivePart() * g;
    }

    public TElem Content()
    {
        return Ring.Gcd(Coefs);
    }
    
    public UnivariatePolynomial<TElem> PrimitivePart()
    {
        var content = Content();
        if (Ring.IsOne(content))
            return this;
        
        return Div(this, content);
    }
    
    public (TElem Content, UnivariatePolynomial<TElem> Primitive) AsPrimitive()
    {
        var content = Content();
        if (Ring.IsOne(content))
            return (content, this);
        
        return (content, Div(this, content));
    }

    public (UnivariatePolynomial<TElem> Gcd, UnivariatePolynomial<TElem> X, UnivariatePolynomial<TElem> Y) ExtendedGcd(UnivariatePolynomial<TElem> a, UnivariatePolynomial<TElem> b){
        
        var (old_r, r) = (a, b);
        var (old_s, s) = (One, Zero);
        var (old_t, t) = (Zero, One);
        
        while (!IsZero(r))
        {
            var (q, new_r) = DivRem(old_r, r);
            (old_r, r) = (r, new_r);
            (old_s, s) = (s, old_s - q * s);
            (old_t, t) = (t, old_t - q * t);
        }

        var lc_r = old_r.LC;
        old_r /= lc_r;
        old_s /= lc_r;
        old_t /= lc_r;
        
        return (old_r, old_s, old_t);
    }

    public UnivariatePolynomial<TElem> Derivee()
    {
        var coefs = new TElem[Deg];
        
        for (int i = 1; i <= Deg; i++)
        {
            coefs[i - 1] = Ring.MulInt(Coefs[i], i);
        }
        
        return new UnivariatePolynomial<TElem>(Ring, coefs);
    }
    
    public UnivariatePolynomial<TElem>[] YunSquareFree()
    {
        var f = this;
        var df = f.Derivee();
        var a = new List<UnivariatePolynomial<TElem>> { Gcd(f, df) };

        var b = Div(f, a[0]);
        var c = Div(df, a[0]);
        var d = c - b.Derivee();
        var i = 1;

        while (b.Deg > 0)
        {
            a.Add(Gcd(b, d));
            b = Div(b, a[i]);
            c = Div(d, a[i]);
            d = c - b.Derivee();

            i++;
        }
        
        a.RemoveAt(0);
        return a.ToArray();
    }

    public override string ToString()
    {
        if (IsZero(this))
            return "0";
        
        var sb = new StringBuilder();
        for (int i = 0; i < Coefs.Length; i++)
        {
            if (Ring.IsZero(Coefs[i]))
                continue;
            
            if (i != 0)
                sb.Append(" + ");
            
            sb.Append(Coefs[i]);
            
            switch (i)
            {
                case 0:
                    continue;
                case 1:
                    sb.Append('x');
                    continue;
                default:
                    sb.Append($"x^{i}");
                    break;
            }
        }

        return sb.ToString();
    }
}