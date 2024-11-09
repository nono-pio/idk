using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;

public class Gruntz
{

    public static Expr Limit(Expr f, Variable x, Expr x0)
    {
        if (x0.IsInfinity)
            return LimitInf(f, x);
        if (x0.IsNegativeInfinity)
            return LimitInf(f.Substitue(x, -x), x);

        return LimitInf(f.Substitue(x, x0 + 1 / x), x);
    }
    
    public static List<Expr> Mrv(Expr f, Variable variable)
    {
        if (f.Constant(variable))
            return [];

        if (f.IsVar(variable))
            return [f];

        if (f is Addition add)
            return add.Args.Select(e => Mrv(e, variable)).Aggregate((a, b) => MaxMrv(a, b, variable));
        if (f is Multiplication mul)
            return mul.Args.Select(e => Mrv(e, variable)).Aggregate((a, b) => MaxMrv(a, b, variable));

        if (f is Power pow)
        {
            if (pow.IsExp)
            {
                if (pow.Exp.IsVar(variable))
                    return [f];
                if (LimitInf(pow.Exp, variable).IsInfinite)
                    return MaxMrv([f], Mrv(pow.Exp, variable), variable);
                return [pow.Exp];
            }
            
            return Mrv(pow.Base, variable);
        }

        if (f is Logarithm log)
        {
            return Mrv(log.Value, variable);
        }
        
        return f.Args.Select(e => Mrv(e, variable)).Aggregate((a, b) => MaxMrv(a, b, variable));
    }
    
    public static List<Expr> MaxMrv(List<Expr> f, List<Expr> g, Variable variable)
    {
        if (f.Count == 0)
            return g;
        if (g.Count == 0)
            return f;
        
        var a = Ln(f[0]);
        var b = Ln(g[0]);

        var c = LimitInf(a / b, variable);
        if (c.IsZero)
            return g;
        if (c.IsInfinite)
            return f;
        
        return f.Concat(g).ToList();
    }
    
    public static int SignInf(Expr f, Variable variable)
    {
        if (f.Constant(variable))
            return Sign(f).ToInt();

        if (f.IsVar(variable) || (f is Power pow && SignInf(pow.Base, variable) == 1))
            return 1;

        if (f is Multiplication mul)
            return mul.Args.Select(e => SignInf(e, variable)).Aggregate((a, b) => a * b);
        
        var (c0, e0) = LeadTerm(f, variable);
        return SignInf(c0, variable); // w^e0 is always positive
    }

    public static Expr LimitInf(Expr f, Variable variable)
    {
        if (f.Constant(variable))
            return f;
        
        var (c0, e0) = LeadTerm(f, variable);
        var sign = SignInf(e0, variable);
        var result = sign switch
        {
            1 => 0,
            -1 => Expr.Inf * SignInf(c0, variable),
            0 => LimitInf(c0, variable),
            _ => throw new ArgumentException("Sign must be -1, 0 or 1")
        };
        
        // Console.WriteLine($"Input: {f}, Output: {result}");
        return result;
    }

    /// c0 * w^e0
    public static (Expr c0, Expr e0) LeadTerm(Expr f, Variable variable)
    {
        if (f.Constant(variable))
            return (f, Num(0));

        f = f.Map<Power>(pow => pow.Exp.Constant(variable) || pow.IsExp ? pow : Exp(Ln(pow.Base) * pow.Exp));
        f = f.Map<Multiplication>(mul => Mul(
            mul.Args.Where(e => e is not Power pow || !pow.IsExp)
                .Append(
                    Exp(Add(mul.Args.Where(e => e is Power pow && pow.IsExp).Select(e => ((Power) e).Exp).ToArray()))
                    ).ToArray()));


        var w = new Variable("w", dummy: true);
        (f, var logw) = Rewrite(f, w, variable);

        var lt = ComputeLeadingTerm(f, w, logw);
        var (c0, e0) = AsCoefExp(lt, w);
        
        return (c0, e0);
    }
    
    public static (Expr f, Expr? logw) Rewrite(Expr f, Variable w, Variable variable)
    {
        
        var omega = Mrv(f, variable);
        
        if (omega.Count == 0)
            return (f, null);

        if (omega.Contains(variable, new ExprComparer()))
        {
            f = f.Substitue(variable, Exp(variable));
            omega = omega.Select(e => e.Substitue(variable, Exp(variable))).ToList();
        }
        
        var Omega = omega.OrderBy(e => -Mrv(e, variable).Count).Select(e => (Power) e).ToList();

        Power? g = null;
        int sign = 0;
        for (int i = 0; i < Omega.Count; i++)
        { 
            g = Omega[i];
            sign = SignInf(g.Exp, variable);
            if (sign != 1 && sign != -1)
                throw new Exception();
        }

        if (g is null)
            throw new Exception();

        var W = sign == 1 ? 1 / w : w;
        foreach (var a in Omega)
        {
            var c = LimitInf(a.Exp / g.Exp, variable);
            var b = Exp(a.Exp - c * g.Exp) * Pow(W, c);
            f = f.Substitue(a, b);
        }
        
        return (f, -sign * g.Exp);
    }
    
    public static Expr Series(Expr f, int n, Variable w, Expr? logw)
    {
        if (f.Constant(w))
            return f;

        if (f.IsVar(w))
            return w;
        
        switch (f)
        {
            case Addition add:
                return Add(add.Therms.Select(e => Series(e, n, w, logw)).ToArray());
            case Multiplication mul:
                return Mul(mul.Factors.Select(e => Series(e, n, w, logw)).ToArray());
            case Power pow:
                // 2 cases :
                // 1. e^f(x)
                // 2. f(x)^n
                // si exp n'est pas cste -> e^(ln base * exp) (case 1)

                if (pow.IsExp)
                {
                    var e_series = Series(pow.Exp, n, w, logw);
                    var e0 = Limit(e_series, w, 0);
                    if (e0.IsInfinite)
                        throw new NotImplementedException();
                    var t = e_series - e0;
                    var exp_series = Exp(e0);
                    var term = exp_series;
                    for (int i = 1; i < n; i++)
                    {
                        term *= t / i;
                        term = Series(term, n, w, logw);
                        exp_series += term;

                    }

                    return exp_series;
                }
                
                if (!pow.Exp.Constant(w))
                    return Series(Exp(Ln(pow.Base) * pow.Exp), n, w, logw);

                var b_series = Series(pow.Base, n, w, logw);
                var b0 = ComputeLeadingTerm(b_series, w, null);
                var t2 = b_series / b0 - 1;
                if (t2 is Addition add2)
                    t2 = Add(add2.Therms.Where(e => !Limit(e, w, 0).IsInfinite).ToArray());
                var (c, e) = AsCoefExp(b0, w);
                var pow_series = 1.Expr();
                var term2 = pow_series;
                for (int i = 1; i < n; i++)
                {
                    term2 *= (pow.Exp - i + 1) * t2 / i;
                    term2 = Series(term2, n, w, logw);
                    pow_series += term2;

                }

                var factor = Pow(b0, pow.Exp);

                if (c.IsNegative)
                    throw new NotImplementedException();

                pow_series = factor * pow_series;
                return pow_series;
            case FonctionExpr func:

                if (func.Args.Length != 1)
                    throw new NotImplementedException();

                // Try taylor series
                var _x = new Variable("_x", dummy:true);
                var fx = func.Eval([_x], func.GetArgs());
                Expr xfac = 1;
                var result = fx.Substitue(_x, 0) * xfac;
                for (int i = 1; i < n; i++)
                {
                    xfac *= _x / i;
                    fx = fx.Derivee(_x);
                    result += fx.Substitue(_x, 0) * xfac;
                }

                var series = Series(result.Substitue(_x, func.X), n, w, logw);
                
                /*Example
                 Sin(w^2) = Sin(_x) = _x - _x^3/6 + _x^5/120 = w^2 - w^6/6 + w^10/120
                 */
                
                return series;
        }

        throw new NotImplementedException();
    }

    public static (Expr Coef, Expr Exp) AsCoefExp(Expr expr, Variable variable)
    {
        if (expr.IsVar(variable))
            return (1, 1);

        if (expr.Constant(variable))
            return (expr, 0);

        if (expr is Multiplication mul)
        {
            var (cste, variate) = mul.AsMulCsteNCste(variable);
            var (coef, exp) = AsCoefExp(variate, variable);
            return (coef * cste, exp);
        }

        if (expr is Power pow && pow.Base.IsVar(variable)) 
            return (1, pow.Exp);

        throw new NotImplementedException();
    }

    public static Expr ComputeLeadingTerm(Expr f, Variable w, Expr? logw)
    {
        f = f.Develop();
        Expr series = Series(f, 10, w, logw);
        series = series.Develop();
        
        if (series is Addition add)
        {
            var (cmin, emin) = AsCoefExp(add.Therms[0], w);
            foreach (var therm in add.Therms.Skip(1))
            {
                var (c, e) = AsCoefExp(therm, w);
                if (e < emin)
                {
                    cmin = c;
                    emin = e;
                }
            }

            return cmin * Pow(w, emin);
        }

        return series;
    }

    private class ExprComparer : IComparer<Expr>, IEqualityComparer<Expr>
    {
        public int Compare(Expr? x, Expr? y)
        {
            return (x, y) switch
            {
                (null, null) => 0,
                (null, _) => -1,
                (_, null) => 1,
                _ => x.CompareTo(y)
            };
        }

        public bool Equals(Expr? x, Expr? y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode(Expr obj)
        {
            return obj.Args.GetHashCode();
        }
    }
    
}