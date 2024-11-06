
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;

public class Gruntz
{
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
        return sign switch
        {
            1 => 0,
            -1 => Expr.Inf * SignInf(c0, variable),
            0 => LimitInf(c0, variable),
            _ => throw new ArgumentException("Sign must be -1, 0 or 1")
        };
    }

    /// c0 * w^e0
    public static (Expr c0, Expr e0) LeadTerm(Expr f, Variable variable)
    {
        if (f.Constant(variable))
            return (f, Num(0));

        f = f.Map<Power>(pow => Exp(Ln(pow.Base) * pow.Exp));
        f = f.Map<Multiplication>(mul => Mul(
            mul.Args.Where(e => e is not Power)
                .Append(
                    Exp(Add(mul.Args.Where(e => e is Power).Select(e => ((Power) e).Exp).ToArray()))
                    ).ToArray()));


        var w = new Variable("w", dummy: true);
        (f, var logw) = Rewrite(f, w, variable);
        // TODO : series
        return (null, null);
    }
    
    public static (Expr f, Expr logw) Rewrite(Expr f, Variable w, Variable variable)
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