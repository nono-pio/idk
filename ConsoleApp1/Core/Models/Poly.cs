using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Models;

public class Poly
{
    /// [a,b,c] -> ax^2+bx+c
    private readonly Expr[] _coefs;

    public Poly(params Expr[] coefs)
    {
        if (coefs.Length == 0)
            coefs = [ Zero ];
        
        _coefs = coefs;
    }
    
    public Poly(params int[] coefs)
    {
        if (coefs.Length == 0)
        {
            _coefs = new[] { Zero };
            return;
        }
        _coefs = new Expr[coefs.Length];
        for (int i = 0; i < coefs.Length; i++)
        {
            _coefs[i] = coefs[i].Expr();
        }
        
    }

    public Poly Integral()
    {
        var coefs = new Expr[_coefs.Length + 1];
        coefs[^1] = Zero;
        for (int i = 0; i < _coefs.Length; i++)
        {
            coefs[i] = _coefs[i] / (Deg() - i + 1);
        }

        return new Poly(coefs);
    }

    public Expr Of(Expr x)
    {
        var result = Zero;
        foreach (var (coef, deg) in AsCoefDeg())
        {
            var therm = deg switch
            {
                0 => coef,
                1 => coef * x,
                _ => coef * Power.Construct(x, deg)
            };
            
            result += therm;
        }

        return result;
    }

    public static bool IsPolynomial(Expr expr, Variable variable)
    {
        if (expr.Constant(variable))
            return true; // deg=0

        return expr switch
        {
            Addition add => add.Therms.All(therm => IsPolynomial(therm, variable)), // deg=max(deg(therm))
            Multiplication mul => mul.Factors.All(factor => IsPolynomial(factor, variable)), // deg=sum(deg(factor))
            Number => true, // deg=0 
            Power pow => IsPolynomial(pow.Base, variable) && pow.Exp.IsNumberIntPositif(), //deg=deg(base)*exp
            Variable var => true, // var.Name == variable ? deg=1 : deg=0
            _ => false
        };
    }
    
    public static Poly ToPoly(Expr expr, Variable variable)
    {
        if (expr.Constant(variable))
            return new Poly(expr);

        switch (expr) 
        {
            case Addition add :

                var poly = add.Therms.Aggregate(PolyZero, (sum, therm) =>
                {
                    var th = ToPoly(therm, variable);
                    return sum + th;
                });
                return poly;
            
            case Multiplication mul :

                poly = mul.Factors.Aggregate(PolyOne, (pro, therm) =>
                {
                    var th = ToPoly(therm, variable);
                    return pro * th;
                });
                return poly;
                
            case Number num:
                return new Poly(num); 
            
            case Power pow:
                
                var basePoly = ToPoly(pow.Base, variable);

                if (!pow.Exp.IsNumberIntPositif())
                    throw new Exception("This is not a polynomial"); 
                
                int exp = pow.Exp.ToInt();

                return basePoly.Pow(exp);
            
            case Variable var: 
                return var == variable ? new Poly(1, 0) : new Poly(var); // var.Name == variable ? deg=1 : deg=0
            
            default:
                throw new Exception("This is not a polynomial"); 
        };
    }

    public static Poly VideDeg(int deg)
    {
        var coefs = new Expr[deg + 1];
        Array.Fill(coefs, Zero);

        return new Poly(coefs);
    }
    
    public static Poly FromMonomial(Expr coef, int deg)
    {
        var poly = VideDeg(deg);
        poly.SetCoefDeg(deg, coef);
        return poly;
    }
    
    public int Deg()
    {
        return _coefs.Length - 1;
    }

    public Expr CoefDeg(int deg)
    {
        if (deg > Deg())
            return Zero;
        
        return _coefs[Deg() - deg];
    }

    public void SetCoefDeg(int deg, Expr value)
    {
        if (deg <= Deg())
            _coefs[Deg() - deg] = value;
    }
    
    public Expr LC()
    {
        return CoefDeg(Deg());
    }

    public Poly EnleveZeroInutile()
    {

        if (!LC().IsZero)
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

            if (!CoefDeg(deg).IsZero)
            {
                newPoly = VideDeg(deg);
                newPoly.SetCoefDeg(deg, CoefDeg(deg));
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

    // (x+1)(x^2+2) -> [1, x+1, x^2+2], x
    // (e^(2x)+e^x+1) -> [(e^x)^2+e^x+1], e^x
    // 2 -> [2], null
    public static (Poly[], Expr?) AsPolyFactors(Expr expr, Variable variable)
    {

        Expr? variablePart = null;
        List<Poly> polys = [new Poly(Un)];
        
        // (x+1)(x^2+2)
        foreach (var factor in expr.GetEnumerableFactors())
        {

            if (!factor.Constant(variable))
            {
                polys[0] *= factor;
                continue;
            }

            Poly poly = PolyZero;
            // (x+1)
            foreach (var monomial in factor.GetEnumerableTherms())
            {
                // monomial = coef * newVariablePart ^ deg
                var (coef, variablePartPow) = monomial.AsMulCsteNCste(variable);
                var (deg, newVariablePart) = variablePartPow.AsPowInt();

                if (variablePart is null)
                {
                    variablePart = newVariablePart;
                }
                else if (variablePart != newVariablePart)
                {
                    throw new Exception("Multinomial not accepted");
                }

                poly += FromMonomial(coef, deg);
            }
            
            polys.Add(poly);
        }

        return (polys.ToArray(), variablePart);
    }

    public override string ToString()
    {
        
        var str = "";
        for (var i = 0; i < _coefs.Length; i++)
        {
            var coef = _coefs[i];
            var deg = Deg() - i; 
            if (coef.IsZero)
                continue;
            var coef_str = coef.IsOne && deg != 0 ? "" : coef.ToString();
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
    
    public string ToLatex(string var)
    {
        
        var str = "";
        for (var i = 0; i < _coefs.Length; i++)
        {
            var coef = _coefs[i];
            var deg = Deg() - i; 
            if (coef.IsZero)
                continue;
            var coef_str = coef.IsOne && deg != 0 ? "" : coef.ToLatex();
            switch (deg)
            {
                case 0:
                    str += Symbols.Add + var;
                    continue;
                case 1:
                    str += Symbols.Add + coef_str + var;
                    continue;
                default:
                    str += Symbols.Add + coef_str + LatexUtils.Power(var, deg.ToString());
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
        var coefs = new Expr[_coefs.Length];
        for (int i = 0; i < coefs.Length; i++)
        {
            coefs[i] = _coefs[i];
        }

        return new Poly(coefs);
    }


    public bool IsZero()
    {
        return Deg() == 0 && CoefDeg(0).IsZero;
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
        var q = new Expr[a.Deg() - b.Deg() + 1];
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
    
    public static Poly operator /(Poly a, Expr b)
    {
        for (int deg = 0; deg <= a.Deg(); deg++)
        {
            a.SetCoefDeg(deg, a.CoefDeg(deg) / b);
        }

        return a;
    }
    
    public static Poly operator *(Poly a, Expr b)
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
                result.SetCoefDeg(deg_a + deg_b, a.CoefDeg(deg_a) * b.CoefDeg(deg_b) + result.CoefDeg(deg_a + deg_b));
            }
        }

        return result;
    }

    public static Poly operator /(Poly a, Poly b)
    {
        var (q, _) = Div(a, b);
        return q;
    }

    public static Poly operator %(Poly a, Poly b)
    {
        var (_, r) = Div(a, b);
        return r;
    }
    
    public Poly Pow(int exp)
    {
        var result = PolyOne;
        for (int i = 0; i < exp; i++)
        {
            result *= this;
        }

        return result;
    }

    public Expr[] Solve() // P[x] = 0 -> x = [x1, x2, ...]
    {
        if (Deg() == 0)
            return Array.Empty<Expr>();

        if (Deg() == 1)
            return [SolveLinear(_coefs[0], _coefs[1])];
        
        if (Deg() == 2)
            return SolveQuadratic(_coefs[0], _coefs[1], _coefs[2]);
        
        // TODO: Factorization
        throw new NotImplementedException();
    }

    public IEnumerable<(Expr, int)> AsCoefDeg()
    {
        for (int deg = 0; deg <= Deg(); deg++)
        {
            yield return (CoefDeg(deg), deg);
        }
    }
    
    
    public IEnumerable<(Expr, int)> AsCoefNotZeroDeg()
    {
        for (int deg = 0; deg <= Deg(); deg++)
        {
            var coef = CoefDeg(deg);
            if (!coef.IsZero)
                yield return (coef, deg);
        }
    }

    /// a x + b = 0
    public static Expr SolveLinear(Expr a, Expr b)
    {
        return -b / a;
    }
    
    /// a x^2 + b x + c = 0
    public static Expr[] SolveQuadratic(Expr a, Expr b, Expr c)
    {
        var delta = b*b - 4*a*c;

        var mb = -b;
        var a2 = 2*a;

        return [(mb + Sqrt(delta)) / a2, (mb - Sqrt(delta)) / a2];
    }
}