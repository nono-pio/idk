using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Limits;

public static class Limit
{
    // lim x->a f(x) = L
    public static Expr? LimitOf(Expr expr, Variable variable, Expr value)
    {
        if (expr.Constant(variable))
            return expr;

        if (value.IsInfinity)
            return LimitInf(expr, variable);
        if (value.IsNegativeInfinity)
            return LimitInf(expr, variable, true);
        
        return LimitFinite(expr, variable, value);
    }

    public static Expr? LimitInf(Expr expr, Variable variable, bool negativeInf = false)
    {
        if (PolyRational.IsPolyRational(expr, variable))
        {
            var poly = PolyRational.ToPolyRational(expr, variable);

            if (poly.Num.Deg() > poly.Den.Deg())
            {
                var deg = poly.Num.Deg();
                bool lcpos;
                if (poly.Num.LC().IsPositive)
                    lcpos = true;
                else if (poly.Num.LC().IsNegative)
                    lcpos = false;
                else
                    return null;
                
                
                if (deg % 2 == 0)
                    return lcpos ? Expr.Inf : Expr.NegInf;

                return negativeInf ^ lcpos ? Expr.Inf : Expr.NegInf;

            }
            else if (poly.Num.Deg() == poly.Den.Deg())
                return poly.Num.LC() / poly.Den.LC();
            else
                return 0;
        }

        return null;
    }

    public static Expr? LimitFinite(Expr expr, Variable variable, Expr value)
    {
        if (expr.IsContinue(variable, value).IsTrue)
            return expr.Substitue(variable, value);
        
        return null;
    }
    
    /*
    Heuristic approach:
    Add, Mul, Pow -> calculate limit of each argument
    Combine the results
    If the combine is indeterminate then :
    1. if 0/0 -> L'Hopital's Rule
    2. if inf/inf -> L'Hopital's Rule
    3. if 0*inf = 0/(1/inf) -> L'Hopital's Rule
    4. if inf0-inf1 = (1/inf1 - 1/inf0)/(1/(inf0*inf1)) -> L'Hopital's Rule
    5. if a^b is inderminate -> exp(b*ln(a))
    */

    public static Expr? Heuristic(Expr f, Variable variable, Expr value)
    {
        if (f.Constant(variable))
            return f;

        if (f.IsVar(variable))
            return value;
        
        return f switch
        {
            Addition add => HeuriticAdd(add.Args, variable, value),
            Multiplication mul => HeuriticMul(mul.Args, variable, value),
            Power pow => HeuriticPow(pow.Base, pow.Exp, variable, value),
            _ => null
        };
    }

    private static Expr? HeuriticAdd(Expr[] exprs, Variable variable, Expr value)
    {
        var l = Heuristic(exprs[0], variable, value);
        var f = exprs[0];

        if (l is null)
            return null;

        for (int i = 1; i < exprs.Length; i++)
        {
            var new_l = Heuristic(exprs[i], variable, value);
            if (new_l is null)
                return null;
            
            // Check oo - oo
            if ((l.IsInfinity && new_l.IsNegativeInfinity)
                || (l.IsNegativeInfinity && new_l.IsInfinity))
            {
                // (1/inf1 + 1/inf0)/(1/(inf0*inf1))
                var n = (1 / new_l + 1 / l) / (1 / (new_l * l));
                var ind = Heuristic(n, variable, value);
                if (ind is null)
                    return null;

                l = ind;
                f = n;
            }

            l += new_l;
            f += exprs[i];
        }
        
        return l;
    }
    
    private static Expr? HeuriticMul(Expr[] power, Variable variable, Expr value)
    {
        return null;
    }
    
    private static Expr? HeuriticPow(Expr @base, Expr exp, Variable variable, Expr value)
    {
        var new_base = Heuristic(@base, variable, value);
        var new_exp = Heuristic(exp, variable, value);

        if (new_base is null || new_exp is null)
            return null;
        
        // check indefine
        if ((new_base.IsZero && new_exp.IsInfinity) ||
            (new_base.IsOne && new_exp.IsInfinity) ||
            (new_base.IsInfinity && new_exp.IsNegativeInfinity))
        {
            var ind = Heuristic(Ln(@base) * exp, variable, value);
            if (ind is null)
                return null;

            return Exp(ind);
        }

        return Pow(new_base, new_exp);
    }
}