using System.Diagnostics;
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
        Coefs = coefs;
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

    public bool IsZero(UnivariatePolynomial<TElem> e) => (e.Deg == 0 && Ring.IsZero(e.Coefs[0])) || e.Coefs.Length == 0;
    public bool IsOne(UnivariatePolynomial<TElem> e) => e.Deg == 1 && Ring.IsOne(e.Coefs[0]);
    public bool IsInt(UnivariatePolynomial<TElem> e, int value) => e.Deg == 1 && Ring.IsInt(e.Coefs[0], value);
    
    public TElem LC => Coefs[^1];
    
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
                coefs[i] = Ring.Sub(coefs[i], a.Coefs[i]);
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
        return DivRem(a, b).Remainder;
    }

    public (UnivariatePolynomial<TElem> Quotient, UnivariatePolynomial<TElem> Remainder) DivRem(UnivariatePolynomial<TElem> n, UnivariatePolynomial<TElem> d)
    {
        if (IsZero(d))
            throw new DivideByZeroException();
        
        var q = Zero;
        var r = d;
        while (!IsZero(r) && r.Deg >= d.Deg)
        {
            // TODO: Optimize create MonoAdd et MonoMul
            var t = MonomialToPolynomial(Ring.Div(r.LC, d.LC), r.Deg - d.Deg);
            q = q + t;
            r = r - t * d;
        }

        return (q, r);
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
        var old_r = a;
        var r = b;
        
        while (!IsZero(r)){
            
            var new_r = Rem(old_r, r);
            old_r = r;
            r = new_r;
        }

        return old_r;
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
}